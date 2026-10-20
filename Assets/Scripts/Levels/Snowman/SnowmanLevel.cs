using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 11 — pupazzo di neve.
    /// Occhi (2 carboni) e naso (carota) si attaccano dove li lasci sulla testa: nessuna posizione è indicata.
    /// Per vincere bastano due occhi e il naso attaccati alla testa, DOVE SI VUOLE, e una bocca disegnata col dito
    /// (qualsiasi forma: resta come un solco nella neve). Più la faccia è storta, più il pupazzo commenta buffo.
    /// Il pupazzo è in posa di 3/4 (guarda a sinistra): il sedere sporge sul retro, a destra in basso.
    /// Easter egg: lasciare la carota sul sedere → la carota resta piantata lì, il pupazzo arrossisce, gli occhi
    /// schizzano via, la bocca resta a "O", e si scioglie del tutto dall'alto in basso (SnowmanMelter) finché resta
    /// solo una pozzanghera con cappello, braccia e carota che galleggiano. La scena viene registrata per condividerla.
    /// </summary>
    public class SnowmanLevel : LevelController
    {
        [Header("Parti del pupazzo (pivot sui punti di appoggio)")]
        [SerializeField] Transform snowman;      // radice ai piedi
        [SerializeField] Transform basePart;     // pivot y = 0
        [SerializeField] Transform headPart;     // pivot y = 3.42
        [SerializeField] SpriteRenderer[] bodyRenderers;
        [SerializeField] SpriteRenderer blush;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] StrokeDrawer mouthPen;  // centrato sulla testa
        [SerializeField] SnowmanMeltdown meltdown;   // urlo + scioglimento (uguale al livello 29)
        [SerializeField] SpriteRenderer mouthO;  // bocca stupita (nascosta finché non serve)

        [Header("Coordinate del pupazzo, piedi in 0,0")]
        [SerializeField] Vector2 headCenter = new Vector2(-0.1f, 3.7f);
        [SerializeField] float stickRadius = 1.15f;                             // dove gli oggetti restano attaccati
        [SerializeField] Vector2 buttCenter = new Vector2(0.86f, 1.02f);
        [SerializeField] float buttRadius = 0.5f;
        [SerializeField] Vector2 buttSurface = new Vector2(0.86f, 1.02f);       // dove si pianta la punta della carota

        [Header("Easter egg")]
        [SerializeField] string eggId = "snowman_butt";
        [SerializeField] float groundY = -3.9f;

        static readonly Color BlushTint = new Color(1f, 0.78f, 0.83f);
        static readonly Color MeltTint = new Color(0.86f, 0.94f, 1f);

        Draggable2D[] items;
        bool resolving;
        bool mouthOk;

        public override void OnLevelStart()
        {
            items = GetComponentsInChildren<Draggable2D>(true);
            foreach (Draggable2D d in items) d.Dropped += OnItemDropped;
            if (mouthPen != null) mouthPen.StrokeFinished += OnMouthDrawn;

            if (blush != null) blush.color = new Color(1f, 1f, 1f, 0f);
            if (mouthO != null) mouthO.gameObject.SetActive(false);
            if (bubble != null) bubble.Hide();
        }

        void OnDestroy()
        {
            if (items != null) foreach (Draggable2D d in items) if (d != null) d.Dropped -= OnItemDropped;
            if (mouthPen != null) mouthPen.StrokeFinished -= OnMouthDrawn;
        }

        Vector2 Local(Vector3 world) { return snowman.InverseTransformPoint(world); }

        // ---------- Faccia ----------

        void OnItemDropped(Draggable2D item, DropSlot slot)
        {
            if (IsFinished || resolving) return;
            Vector2 p = Local(item.transform.position);

            if (item.itemId == "carrot" && Vector2.Distance(p, buttCenter) <= buttRadius)
            {
                resolving = true;
                StartCoroutine(MeltRoutine(item));
                return;
            }
            if (Vector2.Distance(p, headCenter) <= stickRadius)
            {
                item.transform.SetParent(headPart, true);   // resta lì, attaccato alla testa
                CheckComplete();
            }
            else
            {
                item.transform.SetParent(transform, true);
                StartCoroutine(FallToGround(item.transform));
            }
        }

        void OnMouthDrawn(StrokeDrawer pen)
        {
            // qualsiasi bocca va bene: sorriso, broncio, zig-zag... basta che ci sia
            IReadOnlyList<Vector3> pts = pen.LastStrokePoints;
            mouthOk = pts != null && pts.Count >= 2;
            CheckComplete();
        }

        void CheckComplete()
        {
            if (IsFinished || resolving || !mouthOk) return;
            if (Face(out _, out _, out _))
            {
                resolving = true;
                StartCoroutine(WinRoutine());
            }
        }

        /// <summary>Due occhi e il naso attaccati alla testa, in qualunque punto (coordinate del pupazzo).</summary>
        bool Face(out Vector2 eyeA, out Vector2 eyeB, out Vector2 nose)
        {
            eyeA = eyeB = nose = Vector2.zero;
            int eyes = 0;
            bool hasNose = false;
            foreach (Draggable2D d in items)
            {
                if (d == null || d.transform.parent != headPart) continue;
                Vector2 p = Local(d.transform.position);
                if (d.itemId == "eye") { if (eyes == 0) eyeA = p; else eyeB = p; eyes++; }
                else if (d.itemId == "carrot") { nose = p; hasNose = true; }
            }
            return eyes >= 2 && hasNose;
        }

        /// <summary>Il commento del pupazzo sulla faccia che gli hai fatto: più è storta, più è buffo.</summary>
        string FaceComment(out bool silly)
        {
            silly = true;
            Face(out Vector2 a, out Vector2 b, out Vector2 nose);
            IReadOnlyList<Vector3> pts = mouthPen != null ? mouthPen.LastStrokePoints : null;
            Vector2 mouth = Vector2.zero, m0 = Vector2.zero, m1 = Vector2.zero;
            float topY = float.MinValue;
            if (pts != null && pts.Count > 0)
            {
                for (int i = 0; i < pts.Count; i++)
                {
                    Vector2 p = Local(mouthPen.transform.TransformPoint(pts[i]));
                    mouth += p;
                    if (i > 0 && i < pts.Count - 1) topY = Mathf.Max(topY, p.y);
                }
                mouth /= pts.Count;
                m0 = Local(mouthPen.transform.TransformPoint(pts[0]));
                m1 = Local(mouthPen.transform.TransformPoint(pts[pts.Count - 1]));
            }
            if (mouth.y > Mathf.Max(a.y, b.y)) return "Is my face... upside down?";
            if (nose.y > Mathf.Max(a.y, b.y) + 0.1f) return "Who nose where my nose goes?!";
            if (Mathf.Abs(a.y - b.y) > 0.35f) return "I see the world... sideways!";
            if (Vector2.Distance(a, b) < 0.4f) return "Cross-eyed and PROUD!";
            if (Vector2.Distance(a, b) > 1.1f) return "I can see behind me!";
            if (mouth.y > nose.y) return "My nose is on my chin. Chic!";
            if (Mathf.Abs(nose.x - (a.x + b.x) * 0.5f) > 0.6f) return "Nose to the side: very modern!";
            if (pts != null && pts.Count > 2 && topY > (m0.y + m1.y) * 0.5f + 0.06f) return "Grumpy... but HANDSOME!";
            silly = false;
            return "Yay! Handsome!";
        }

        IEnumerator FallToGround(Transform t)
        {
            float target = groundY + Random.Range(-0.35f, 0.25f);
            if (t.position.y <= target + 0.3f) yield break;
            float v = 0f;
            while (t != null && t.position.y > target)
            {
                if (t.parent == headPart || Draggable2D.Active != null && Draggable2D.Active.transform == t) yield break;
                v += 18f * Time.deltaTime;
                t.position += Vector3.down * (v * Time.deltaTime);
                yield return null;
            }
            if (t != null) { t.position = new Vector3(t.position.x, target, t.position.z); Haptics.Light(); }
        }

        void LockInput()
        {
            foreach (Draggable2D d in items) if (d != null) d.enabled = false;
            if (mouthPen != null) mouthPen.enabled = false;
        }

        IEnumerator WinRoutine()
        {
            LockInput();
            string line = FaceComment(out bool silly);
            if (bubble != null) bubble.Show(line);
            // gli occhi di carbone ballano come occhi finti, il pupazzo sghignazza e saltella tutto storto
            var eyes = new List<Transform>();
            foreach (Draggable2D d in items) if (d != null && d.itemId == "eye" && d.transform.parent == headPart) eyes.Add(d.transform);
            var eyeHome = new List<Vector3>();
            foreach (Transform e in eyes) eyeHome.Add(e.localPosition);
            Vector3 start = snowman.localPosition;
            Quaternion headRot = headPart.localRotation;
            float hops = silly ? 3 : 2;
            for (int hop = 0; hop < hops; hop++)
            {
                Sfx.Play(this, "pop", 0.5f);
                for (float k = 0f; k < 1f; k += Time.deltaTime / 0.35f)
                {
                    float s = Mathf.Sin(k * Mathf.PI);
                    snowman.localPosition = start + Vector3.up * (s * 0.3f);
                    snowman.localRotation = Quaternion.Euler(0, 0, (hop % 2 == 0 ? 1f : -1f) * s * (silly ? 7f : 3f));
                    headPart.localRotation = headRot * Quaternion.Euler(0, 0, Mathf.Sin((hop + k) * Mathf.PI * 3f) * (silly ? 9f : 4f));
                    for (int i = 0; i < eyes.Count; i++)
                    {
                        float ph = (hop + k) * 13f + i * 2.1f;
                        eyes[i].localPosition = eyeHome[i] + new Vector3(Mathf.Cos(ph), Mathf.Sin(ph), 0f) * 0.05f;
                    }
                    yield return null;
                }
            }
            snowman.localPosition = start;
            snowman.localRotation = Quaternion.identity;
            headPart.localRotation = headRot;
            for (int i = 0; i < eyes.Count; i++) eyes[i].localPosition = eyeHome[i];
            Haptics.Light();
            yield return new WaitForSeconds(0.7f);
            Win();
        }

        // ---------- Easter egg: carota sul sedere ----------

        IEnumerator MeltRoutine(Draggable2D carrot)
        {
            LockInput();
            // la carota resta piantata nel sedere, con la punta verso il pupazzo
            Transform t = carrot.transform;
            t.SetParent(basePart, true);
            t.localScale = new Vector3(-Mathf.Abs(t.localScale.x), t.localScale.y, t.localScale.z);
            Vector3 target = snowman.TransformPoint(buttSurface + new Vector2(0.4f * Mathf.Abs(t.localScale.x), 0f));
            target.z = t.position.z;
            Vector3 from = t.position;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.15f) { t.position = Vector3.Lerp(from, target, k); yield return null; }
            t.position = target;
            Haptics.Medium();

            // 1. sobbalzo e rossore, tremando dall'imbarazzo
            Vector3 home = snowman.localPosition;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.25f)
            {
                snowman.localPosition = home + Vector3.up * Mathf.Sin(k * Mathf.PI) * 0.35f;
                yield return null;
            }
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.9f)
            {
                SetTint(Color.Lerp(Color.white, BlushTint, k), k);
                snowman.localPosition = home + (Vector3)(Random.insideUnitCircle * 0.03f);
                yield return null;
            }
            snowman.localPosition = home;
            SetTint(BlushTint, 1f);

            // 2. URLO, occhi che schizzano, cappello che salta, braccia nel panico... e poi lo scioglimento vero
            var eyes = new List<Transform>();
            var attached = new List<Transform>();
            foreach (Draggable2D d in items)
            {
                if (d == null || !d.gameObject.activeSelf) continue;
                if (d.itemId == "eye" && d.transform.IsChildOf(headPart)) eyes.Add(d.transform);
                else if (d.transform.IsChildOf(headPart) || d.transform.IsChildOf(basePart)) attached.Add(d.transform);
            }
            SurprisedMouth();
            if (meltdown != null)
            {
                if (mouthO == null || !mouthO.gameObject.activeSelf) meltdown.mouthO = null;   // niente bocca disegnata: niente "O"
                yield return meltdown.Play(eyes, attached);
            }
            FailWithEgg(eggId);
        }

        /// <summary>Se la bocca era stata disegnata, il solco diventa una "O" di stupore nello stesso punto.</summary>
        void SurprisedMouth()
        {
            if (mouthPen == null || !mouthPen.HasStroke || mouthO == null) return;
            IReadOnlyList<Vector3> pts = mouthPen.LastStrokePoints;
            Vector3 c = Vector3.zero;
            if (pts != null && pts.Count > 0)
            {
                foreach (Vector3 p in pts) c += p;
                c /= pts.Count;
                c = mouthPen.transform.TransformPoint(c);
            }
            else c = mouthO.transform.position;
            mouthPen.Clear();
            mouthO.transform.position = new Vector3(c.x, c.y - 0.05f, mouthO.transform.position.z);
            mouthO.gameObject.SetActive(true);
            StartCoroutine(PopIn(mouthO.transform));
        }

        IEnumerator PopIn(Transform t)
        {
            Vector3 s = t.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.2f)
            {
                t.localScale = s * (k < 0.7f ? k / 0.7f * 1.25f : Mathf.Lerp(1.25f, 1f, (k - 0.7f) / 0.3f));
                yield return null;
            }
            t.localScale = s;
        }

        void SetTint(Color c, float blushAlpha)
        {
            if (bodyRenderers != null) foreach (SpriteRenderer r in bodyRenderers) if (r != null) r.color = c;
            if (blush != null) blush.color = new Color(1f, 1f, 1f, blushAlpha);
        }
    }
}
