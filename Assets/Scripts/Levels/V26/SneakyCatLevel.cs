using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;
using Gotcha.Levels.V64;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 101 — il topo furtivo. Il gatto di guardia gira la torcia di qua e di là. Trascina il topolino fino al
    /// formaggio restando al buio: i mobili (sei, messi a caso a ogni partita) fanno ombra. E piano: se il topo corre,
    /// le zampette fanno rumore e la torcia scatta verso di lui. (Nel codice "cat" e "fish" sono il topo e il
    /// formaggio, "dog" è il gatto di guardia.)
    /// Il topo parte NASCOSTO nella sua tana nel muro in basso (si vedono solo gli occhietti) e lì non può essere
    /// scoperto; si tira fuori trascinandolo dalla tana. La luce è un ventaglio di raggi fermati dagli ostacoli
    /// (SneakyLightFan, logica pura in V64/Logic): lo stesso poligono si disegna (mesh) e decide se il topo è visto,
    /// così la zona illuminata e le ombre dietro i mobili sono esattamente quelle che contano.
    /// </summary>
    public class SneakyCatLevel : LevelController
    {
        [SerializeField] Transform cat;
        [SerializeField] Transform dog;
        [SerializeField] Transform cone;              // pivot sulla torcia, a 0° punta in basso
        [SerializeField] Transform fish;
        [SerializeField] Collider2D[] furniture;
        [Tooltip("Ostacoli fissi per la luce (il muro in basso con la tana).")]
        [SerializeField] Collider2D[] walls;
        [SerializeField] Transform hole;              // la tana nel muro: il topo parte qui dentro
        [SerializeField] GameObject mouseArt;         // il disegno del topo (spento quando è nella tana)
        [SerializeField] Transform peekEyes;          // gli occhietti che brillano nella tana
        [SerializeField] MeshFilter lightMesh;        // il ventaglio di luce (MeshRenderer con Sprites/Default)
        [SerializeField] SpriteRenderer noiseIcon;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] float halfAngle = 20f, length = 7f;
        [SerializeField] float sweepRange = 65f, sweepSpeed = 0.8f;
        [Tooltip("Gradi al secondo della torcia: in perlustrazione e quando sente il topo.")]
        [SerializeField] float sweepTurn = 110f, chaseTurn = 200f;
        [Tooltip("Posti possibili dei mobili (coordinate del livello): ne vengono usati tanti quanti i mobili, a caso.")]
        [SerializeField] Vector2[] furnitureSlots =
        {
            new Vector2(-1.2f, 0.9f), new Vector2(0.9f, 0.95f), new Vector2(-0.1f, -0.15f), new Vector2(1.6f, -0.5f),
            new Vector2(-1.65f, -0.9f), new Vector2(0.45f, -1.6f), new Vector2(-0.8f, -2.4f), new Vector2(1.7f, -2.25f),
            new Vector2(-2.0f, 0.1f),
        };
        [SerializeField] float maxQuiet = 2.6f, catSpeed = 6f;
        [Tooltip("Mezza larghezza dell'imbocco della tana e distanza minima del topo dal muro quando è fuori.")]
        [SerializeField] float holeHalfWidth = 0.4f, wallMargin = 0.3f;
        [SerializeField] int rays = 96;
        [SerializeField] Color lightNear = new Color(1f, 0.93f, 0.55f, 0.58f), lightFar = new Color(1f, 0.82f, 0.4f, 0.16f);

        float look, sweepClock, seen, noise, wallTop;
        bool holding, done, inHole;
        Camera cam;
        SneakyLightFan fan;
        SneakyLightBox[] boxes;
        int boxCount;
        Mesh mesh;
        Vector3[] verts;
        Color[] colors;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            sweepClock = Random.Range(0f, 6f);
            done = holding = false;
            seen = noise = 0f;
            look = Mathf.Sin(sweepClock * sweepSpeed) * sweepRange;
            PlaceFurniture();
            CollectObstacles();
            SetupLightMesh();
            // il topo parte dentro la tana, nascosto
            cat.position = new Vector3(hole.position.x, hole.position.y, cat.position.z);
            UpdateHidden();
            cone.localRotation = Quaternion.Euler(0, 0, look);
            UpdateLight();
            if (bubble != null) { bubble.Show("Psst... drag me to the cheese. Quietly."); StartCoroutine(HideLater(2.2f)); }
        }

        /// <summary>Mobili in posti a caso; il divano (largo) solo dove c'è spazio ai lati.</summary>
        void PlaceFurniture()
        {
            var free = new System.Collections.Generic.List<Vector2>(furnitureSlots);
            for (int i = free.Count - 1; i > 0; i--) { int j = Random.Range(0, i + 1); (free[i], free[j]) = (free[j], free[i]); }
            foreach (Collider2D f in furniture)
            {
                if (f == null || free.Count == 0) continue;
                bool wide = f is BoxCollider2D b && b.size.x > 1.5f;
                int k = wide ? free.FindIndex(v => Mathf.Abs(v.x) <= 1.2f) : 0;
                if (k < 0) k = 0;
                f.transform.localPosition = new Vector3(free[k].x, free[k].y, f.transform.localPosition.z);
                free.RemoveAt(k);
            }
            Physics2D.SyncTransforms();
        }

        /// <summary>I rettangoli che fermano la luce (mobili + muro), in coordinate mondo: si leggono una volta sola.</summary>
        void CollectObstacles()
        {
            int n = (furniture != null ? furniture.Length : 0) + (walls != null ? walls.Length : 0);
            if (boxes == null || boxes.Length != n) boxes = new SneakyLightBox[n];
            boxCount = 0;
            wallTop = float.NegativeInfinity;
            if (furniture != null) foreach (Collider2D f in furniture) AddBox(f, false);
            if (walls != null) foreach (Collider2D w in walls) AddBox(w, true);
            if (float.IsNegativeInfinity(wallTop)) wallTop = hole.position.y + 0.25f;
        }

        void AddBox(Collider2D c, bool wall)
        {
            if (c == null || !(c is BoxCollider2D b)) return;
            Vector3 center = b.transform.TransformPoint(b.offset);
            Vector3 s = b.transform.lossyScale;
            var box = new SneakyLightBox(center.x, center.y, b.size.x * 0.5f * Mathf.Abs(s.x), b.size.y * 0.5f * Mathf.Abs(s.y));
            boxes[boxCount++] = box;
            if (wall) wallTop = Mathf.Max(wallTop, box.CenterY + box.HalfH);
        }

        void SetupLightMesh()
        {
            if (fan == null || fan.Rays != rays) fan = new SneakyLightFan(rays);
            if (mesh != null || lightMesh == null) return;
            int n = fan.Rays + 2;
            verts = new Vector3[n];
            colors = new Color[n];
            var tris = new int[fan.Rays * 3];
            for (int i = 0; i < fan.Rays; i++) { tris[i * 3] = 0; tris[i * 3 + 1] = i + 1; tris[i * 3 + 2] = i + 2; }
            mesh = new Mesh { name = "TorchLight" };
            mesh.MarkDynamic();
            mesh.vertices = verts;
            mesh.colors = colors;
            mesh.triangles = tris;
            lightMesh.sharedMesh = mesh;
        }

        /// <summary>Ricostruisce il ventaglio (stessi raggi per il disegno e per la regola). Nessuna allocazione.</summary>
        void UpdateLight()
        {
            Vector3 o = cone.position;
            fan.Build(o.x, o.y, look, halfAngle, length, boxes, boxCount);
            if (mesh == null) return;
            Transform mt = lightMesh.transform;
            verts[0] = mt.InverseTransformPoint(o);
            colors[0] = lightNear;
            for (int i = 0; i <= fan.Rays; i++)
            {
                verts[i + 1] = mt.InverseTransformPoint(new Vector3(fan.HitX[i], fan.HitY[i], o.z));
                colors[i + 1] = Color.Lerp(lightNear, lightFar, fan.HitDistance[i] / Mathf.Max(0.01f, length));
            }
            mesh.vertices = verts;
            mesh.colors = colors;
            mesh.RecalculateBounds();
        }

        /// <summary>Nella tana (dentro l'imbocco, sotto il bordo del muro) il topo non si vede: solo gli occhietti.</summary>
        void UpdateHidden()
        {
            Vector2 p = cat.position;
            inHole = Mathf.Abs(p.x - hole.position.x) <= holeHalfWidth && p.y <= wallTop + 0.05f;
            if (mouseArt != null && mouseArt.activeSelf == inHole) mouseArt.SetActive(!inHole);
            if (peekEyes != null)
            {
                if (peekEyes.gameObject.activeSelf != inHole) peekEyes.gameObject.SetActive(inHole);
                float blink = Mathf.Repeat(Time.time, 3.2f) < 0.12f ? 0.15f : 1f;   // ogni tanto sbatte le palpebre
                Vector3 sc = peekEyes.localScale;
                peekEyes.localScale = new Vector3(sc.x, Mathf.Abs(sc.x) * blink, sc.z);
            }
        }

        /// <summary>Il topo resta nella stanza; il muro in basso si attraversa solo dall'imbocco della tana.</summary>
        Vector2 ClampMouse(Vector2 p)
        {
            p.x = Mathf.Clamp(p.x, -2.45f, 2.45f);
            float minY = Mathf.Abs(p.x - hole.position.x) <= holeHalfWidth ? hole.position.y : wallTop + wallMargin;
            p.y = Mathf.Clamp(p.y, minY, 2.3f);
            return p;
        }

        /// <summary>Il topo è visto? Esattamente la zona disegnata dalla luce; nella tana mai.</summary>
        bool Lit(Vector2 p) => !inHole && fan.Contains(p.x, p.y);

        void Update()
        {
            if (done || IsFinished) return;
            float dt = Time.deltaTime;
            // il gatto di guardia: spazza con la torcia, ma se sente rumore la punta verso il topo
            Vector2 toCat = cat.position - cone.position;
            float catAngle = Vector2.SignedAngle(Vector2.down, toCat);
            sweepClock += dt;
            float want = noise > 1f ? catAngle : Mathf.Sin(sweepClock * sweepSpeed) * sweepRange;
            look = Mathf.MoveTowardsAngle(look, want, (noise > 1f ? chaseTurn : sweepTurn) * dt);
            cone.localRotation = Quaternion.Euler(0, 0, look);

            float moved = 0f;
            if (PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out bool ended))
            {
                Vector2 w = PointerInput.ToWorld(cam, sp);
                if (began) holding = Vector2.Distance(w, cat.position) < (inHole ? 0.9f : 0.8f);
                if (holding)
                {
                    Vector2 p = ClampMouse(Vector2.MoveTowards(cat.position, w, catSpeed * dt));
                    moved = Vector2.Distance(p, cat.position);
                    if (Mathf.Abs(p.x - cat.position.x) > 0.001f) cat.localScale = new Vector3(Mathf.Abs(cat.localScale.x) * (p.x > cat.position.x ? -1f : 1f), cat.localScale.y, 1f);
                    cat.position = new Vector3(p.x, p.y, cat.position.z);
                }
                if (ended) holding = false;
            }
            else holding = false;
            float speed = moved / Mathf.Max(dt, 1e-4f);
            noise = Mathf.Max(0f, noise + (speed > maxQuiet ? dt * 5f : -dt * 1.5f));   // sente subito il topo che corre
            if (noiseIcon != null) noiseIcon.color = new Color(1f, 1f, 1f, Mathf.Clamp01(noise));

            UpdateHidden();
            UpdateLight();
            seen = Lit(cat.position) ? seen + dt : 0f;
            if (seen > 0.2f) { StartCoroutine(Woof()); return; }
            if (Vector2.Distance(cat.position, fish.position) < 0.5f) StartCoroutine(Yum());
        }

        IEnumerator Woof()
        {
            done = true;
            Sfx.Play(this, "bonk", 1f);
            Haptics.Medium();
            if (bubble != null) bubble.Show("MEOW! I SEE YOU, LITTLE MOUSE!");
            Vector3 d = dog.position;
            for (float k = 0f; k < 0.8f; k += Time.deltaTime) { dog.position = d + Vector3.up * Mathf.Abs(Mathf.Sin(k * 20f)) * 0.15f; yield return null; }
            yield return new WaitForSeconds(0.6f);
            Fail();
        }

        IEnumerator Yum()
        {
            done = true;
            fish.gameObject.SetActive(false);
            if (bubble != null) bubble.Show("Yoink. Ninja mouse.");
            yield return new WaitForSeconds(1.5f);
            Win();
        }

        IEnumerator HideLater(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (bubble != null && !done) bubble.Hide();
        }
    }
}
