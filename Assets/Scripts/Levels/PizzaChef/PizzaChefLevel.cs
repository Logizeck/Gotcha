using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 32 — pizza chef. Sei ciotole di ingredienti: si trascinano sulla pasta per condire la pizza. Quali
    /// mettere lo suggerisce, senza dirlo, la lavagna sul muro: i disegnini di nonna (pomodoro, mozzarella, fungo,
    /// oliva). Poi si trascina la pizza nel forno, si taglia in quattro (due tagli dal centro) e si dà una fetta
    /// a ogni topolino.
    /// - Condimenti giusti: festa. Manca qualcosa o c'è l'ananas: "Yuck!", si riprova.
    /// - Easter egg `spicy_pizza`: col peperoncino i topi diventano paonazzi, furiosi, con gli occhi in fiamme e
    ///   la lingua di fuori.
    /// </summary>
    public class PizzaChefLevel : LevelController
    {
        [Header("Ingredienti (ciotole trascinabili) e strati della pizza")]
        [SerializeField] Draggable2D[] bowls;               // itemId: tomato, mozzarella, mushroom, olive, chili, pineapple
        [SerializeField] string[] layerIds;                 // per ogni strato, l'ingrediente che lo accende
        [SerializeField] GameObject[] wholeLayers;           // strati sulla pizza intera
        [SerializeField] SpriteRenderer[] bakeTint;          // pasta e mozzarella: dorate dopo il forno
        [SerializeField] Transform pizza;
        [SerializeField] float dropRadius = 1.2f;
        [SerializeField] string[] required = { "tomato", "mozzarella", "mushroom", "olive" };

        [Header("Forno")]
        [SerializeField] Collider2D ovenMouth;
        [SerializeField] SpriteRenderer ovenGlow;
        [SerializeField] SmokeEmitter ovenSmoke;

        [Header("Taglio e servizio")]
        [SerializeField] LineRenderer cutMark;
        [SerializeField] LineRenderer trail;
        [SerializeField] Transform[] slices;                 // 4 spicchi; ogni spicchio ha gli strati figli come la pizza intera
        [SerializeField] DropSlot[] plates;
        [SerializeField] GameObject mice;                    // i 4 topolini (arrivano quando la pizza è cotta)
        [SerializeField] SpriteRenderer[] miceArt;
        [SerializeField] GameObject[] spicyFaces;            // occhi in fiamme, lingua, rabbia (uno per topo)
        [SerializeField] SmokeEmitter hearts;
        [SerializeField] SmokeEmitter steam;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] string eggId = "spicy_pizza";

        enum Phase { Toppings, Baking, Cutting, Serving, Done }
        Phase phase = Phase.Toppings;
        readonly HashSet<string> added = new HashSet<string>();
        Draggable2D pizzaDrag;
        Camera cam;
        Vector2 cutStart;
        bool cutting;
        int cuts, served;
        float firstAngle;
        readonly List<Vector3> trailPts = new List<Vector3>();
        readonly List<float> trailTimes = new List<float>();
        const float Radius = 1.1f;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            foreach (Draggable2D b in bowls) b.Dropped += OnIngredient;
            pizzaDrag = pizza.GetComponent<Draggable2D>();
            if (pizzaDrag != null) { pizzaDrag.enabled = false; pizzaDrag.Dropped += OnPizzaDropped; }
            foreach (GameObject l in wholeLayers) if (l != null) l.SetActive(false);
            foreach (Transform s in slices) s.gameObject.SetActive(false);
            foreach (DropSlot p in plates) p.Filled += OnServed;
            if (mice != null) mice.SetActive(false);
            foreach (GameObject f in spicyFaces) if (f != null) f.SetActive(false);
            if (ovenGlow != null) ovenGlow.color = new Color(1f, 0.5f, 0.1f, 0f);
            if (cutMark != null) cutMark.enabled = false;
            if (bubble != null) { bubble.Show("Pizza time! Grandma's recipe..."); StartCoroutine(HideLater(2.2f)); }
            if (ClipRecorder.Instance != null) ClipRecorder.Instance.Begin();
        }

        void OnDestroy()
        {
            if (bowls != null) foreach (Draggable2D b in bowls) if (b != null) b.Dropped -= OnIngredient;
            if (plates != null) foreach (DropSlot p in plates) if (p != null) p.Filled -= OnServed;
            if (ClipRecorder.Instance != null && ClipRecorder.Instance.IsCapturing) ClipRecorder.Instance.Discard();
        }

        // ---------- 1. condimenti ----------

        void OnIngredient(Draggable2D bowl, DropSlot slot)
        {
            if (phase != Phase.Toppings) return;
            // la ciotola è già tornata al suo posto (returnOnMiss): conta dove è stata lasciata, cioè il dito
            Vector2 at = PointerInput.TryGetPrimary(out Vector2 sp, out _, out _) ? PointerInput.ToWorld(cam, sp) : (Vector2)bowl.transform.position;
            if (Vector2.Distance(at, pizza.position) > dropRadius) return;
            if (!added.Add(bowl.itemId)) return;
            for (int i = 0; i < layerIds.Length; i++)
                if (layerIds[i] == bowl.itemId && wholeLayers[i] != null) { wholeLayers[i].SetActive(true); StartCoroutine(Plop(wholeLayers[i].transform)); }
            foreach (Transform s in slices)
                foreach (Transform layer in s)
                    if (layer.name == "Layer_" + bowl.itemId) layer.gameObject.SetActive(true);
            if (pizzaDrag != null && !pizzaDrag.enabled)
            {
                pizzaDrag.enabled = true;   // ora la pizza si può infornare
                if (bubble != null) { bubble.Show("Into the oven when ready!"); StartCoroutine(HideLater(1.6f)); }
            }
        }

        static IEnumerator Plop(Transform t)
        {
            Vector3 s = t.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.2f) { t.localScale = s * (1f + Mathf.Sin(k * Mathf.PI) * 0.12f); yield return null; }
            t.localScale = s;
        }

        // ---------- 2. forno ----------

        void OnPizzaDropped(Draggable2D d, DropSlot slot)
        {
            if (phase != Phase.Toppings) return;
            if (ovenMouth != null && ovenMouth.OverlapPoint(d.transform.position)) StartCoroutine(Bake());
        }

        IEnumerator Bake()
        {
            phase = Phase.Baking;
            pizzaDrag.enabled = false;
            foreach (Draggable2D b in bowls) b.gameObject.SetActive(false);
            Vector3 home = pizzaHome();
            Vector3 inOven = ovenMouth.bounds.center;
            pizza.position = inOven;
            Vector3 scale = pizza.localScale;
            pizza.localScale = scale * 0.45f;
            if (ovenSmoke != null) ovenSmoke.Play();
            for (float k = 0f; k < 1f; k += Time.deltaTime / 2.2f)
            {
                if (ovenGlow != null) ovenGlow.color = new Color(1f, 0.5f, 0.1f, 0.5f + Mathf.Sin(k * 30f) * 0.15f);
                foreach (SpriteRenderer r in bakeTint) if (r != null) r.color = Color.Lerp(Color.white, new Color(1f, 0.82f, 0.55f), k);
                yield return null;
            }
            if (ovenSmoke != null) ovenSmoke.Stop();
            if (ovenGlow != null) ovenGlow.color = new Color(1f, 0.5f, 0.1f, 0f);
            foreach (Transform s in slices)
                foreach (SpriteRenderer r in s.GetComponentsInChildren<SpriteRenderer>(true))
                    if (r.name.StartsWith("Dough") || r.name.StartsWith("Cheese")) r.color = new Color(1f, 0.82f, 0.55f);
            // fuori dal forno, fumante
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.4f)
            {
                pizza.position = Vector3.Lerp(inOven, home, k);
                pizza.localScale = Vector3.Lerp(scale * 0.45f, scale, k);
                yield return null;
            }
            if (steam != null) { steam.transform.position = home; steam.Play(); }
            if (mice != null) mice.SetActive(true);
            if (bubble != null) { bubble.Show("Smells great! Now cut it in four."); StartCoroutine(HideLater(1.8f)); }
            phase = Phase.Cutting;
        }

        Vector3 homePos;
        bool homeSet;
        Vector3 pizzaHome()
        {
            if (!homeSet) { homeSet = true; homePos = new Vector3(0f, -0.4f, pizza.position.z); }
            return homePos;
        }

        // ---------- 3. taglio ----------

        void Update()
        {
            FadeTrail();
            if (phase != Phase.Cutting || IsFinished) return;
            if (!PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out bool ended)) return;
            Vector2 w = PointerInput.ToWorld(cam, sp);
            if (began) { cutStart = w; cutting = true; }
            if (cutting) { trailPts.Add(new Vector3(w.x, w.y, 0f)); trailTimes.Add(Time.time); }
            if (ended && cutting) { cutting = false; TryCut(cutStart, w); }
        }

        void TryCut(Vector2 a, Vector2 b)
        {
            Vector2 c = pizza.position;
            if (Vector2.Distance(a, c) < Radius * 0.9f || Vector2.Distance(b, c) < Radius * 0.9f || (b - a).magnitude < Radius * 1.6f) return;
            Vector2 dir = (b - a).normalized;
            float off = Mathf.Abs((c.x - a.x) * dir.y - (c.y - a.y) * dir.x);
            float along = Vector2.Dot(c - a, dir);
            if (along < 0f || along > (b - a).magnitude) return;
            if (off > 0.32f) { if (bubble != null) { bubble.Show("Through the middle!"); StartCoroutine(HideLater(1.3f)); } return; }
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            if (cuts == 0)
            {
                firstAngle = angle;
                cuts = 1;
                if (cutMark != null)
                {
                    cutMark.enabled = true;
                    cutMark.SetPosition(0, (Vector3)(c - dir * Radius) + Vector3.back * 0.01f);
                    cutMark.SetPosition(1, (Vector3)(c + dir * Radius) + Vector3.back * 0.01f);
                }
                return;
            }
            float diff = Mathf.Abs(Mathf.DeltaAngle(angle, firstAngle));
            if (diff > 90f) diff = 180f - diff;
            if (diff < 45f) return;
            StartCoroutine(Split());
        }

        IEnumerator Split()
        {
            phase = Phase.Serving;
            if (cutMark != null) cutMark.enabled = false;
            if (steam != null) steam.Stop();
            pizza.gameObject.SetActive(false);
            Vector3 c = pizza.position;
            float centroid = 4f * Radius / (3f * Mathf.PI);
            for (float k = 0f; k <= 1f; k += Time.deltaTime / 0.3f)
            {
                for (int i = 0; i < slices.Length; i++)
                {
                    float a = (firstAngle + i * 90f + 45f) * Mathf.Deg2Rad;
                    slices[i].position = c + new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f) * (centroid + 0.18f * k);
                    slices[i].rotation = Quaternion.Euler(0, 0, firstAngle + i * 90f);
                    if (!slices[i].gameObject.activeSelf) slices[i].gameObject.SetActive(true);
                }
                yield return null;
            }
        }

        // ---------- 4. servizio ed esito ----------

        void OnServed(DropSlot plate, Draggable2D slice)
        {
            served++;
            if (served >= plates.Length) StartCoroutine(Verdict());
        }

        IEnumerator Verdict()
        {
            phase = Phase.Done;
            yield return new WaitForSeconds(0.4f);
            if (added.Contains("chili"))
            {
                // EASTER EGG: piccantissima! paonazzi, furiosi, occhi in fiamme, lingua di fuori
                if (bubble != null) bubble.Show("HOT!!! MAMMA MIAAA!!!");
                foreach (GameObject f in spicyFaces) if (f != null) f.SetActive(true);
                for (float k = 0f; k < 2f; k += Time.deltaTime)
                {
                    float t = Mathf.Clamp01(k / 0.6f);
                    foreach (SpriteRenderer m in miceArt)
                        if (m != null)
                        {
                            m.color = Color.Lerp(Color.white, new Color(0.85f, 0.25f, 0.45f), t);
                            m.transform.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(k * 30f + m.GetInstanceID()) * 9f);   // tremano dalla rabbia
                        }
                    yield return null;
                }
                yield return new WaitForSeconds(0.6f);
                if (ClipRecorder.Instance != null) ClipRecorder.Instance.Stop();
                FailWithEgg(eggId);
                yield break;
            }
            if (ClipRecorder.Instance != null) ClipRecorder.Instance.Discard();
            bool ok = !added.Contains("pineapple");
            foreach (string r in required) if (!added.Contains(r)) ok = false;
            if (ok)
            {
                if (hearts != null) hearts.Play();
                if (bubble != null) bubble.Show("Just like Grandma's!");
                yield return new WaitForSeconds(1.6f);
                if (hearts != null) hearts.Stop();
                Win();
            }
            else
            {
                if (bubble != null) bubble.Show(added.Contains("pineapple") ? "PINEAPPLE?! Yuck!" : "Hmm... something's missing.");
                yield return new WaitForSeconds(1.6f);
                Fail();
            }
        }

        void FadeTrail()
        {
            if (trail == null) return;
            while (trailTimes.Count > 0 && Time.time - trailTimes[0] > 0.15f) { trailTimes.RemoveAt(0); trailPts.RemoveAt(0); }
            trail.positionCount = trailPts.Count;
            trail.SetPositions(trailPts.ToArray());
        }

        IEnumerator HideLater(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (bubble != null && phase != Phase.Done) bubble.Hide();
        }
    }
}
