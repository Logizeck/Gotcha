using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 32 — pizza chef. Sei ciotole di ingredienti: si trascinano sulla pasta per condire la pizza. La ricetta
    /// della nonna è sulla lavagna: 3 o 4 ingredienti scelti a caso a ogni partita, disegnati col gesso col nome sotto.
    /// Poi si inforna, si taglia in quattro (due tagli dal centro) e si dà una fetta a ogni topolino, che la mangia.
    /// - Esattamente la ricetta: festa. Manca qualcosa o c'è un ingrediente in più: "Yuck!", si riprova.
    /// - Easter egg `spicy_pizza`: col peperoncino i topi strillano, diventano paonazzi e furiosi, con gli occhi in
    ///   fiamme che divampano e la lingua penzolante.
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
        [Header("Ricetta casuale sulla lavagna")]
        [SerializeField] bool randomRecipe = true;
        [SerializeField] SpriteRenderer[] recipeIcons;      // 4 posti sulla lavagna
        [SerializeField] TextMesh[] recipeNames;
        [SerializeField] Sprite[] chalkSprites;             // stesso ordine di RecipePool

        [Header("Forno")]
        [SerializeField] Collider2D ovenMouth;
        [SerializeField] DropSlot ovenSlot;          // trascinare la pizza sul forno la inforna
        [SerializeField] TapButton ovenButton;       // ...e basta anche toccare il forno
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
        static readonly string[] RecipePool = { "tomato", "mozzarella", "mushroom", "olive", "pineapple" };
        static readonly string[] RecipeLabels = { "TOMATO", "MOZZARELLA", "MUSHROOMS", "OLIVES", "PINEAPPLE" };
        AudioSource voice;
        AudioClip squeak;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            foreach (Draggable2D b in bowls) b.Dropped += OnIngredient;
            pizzaDrag = pizza.GetComponent<Draggable2D>();
            if (pizzaDrag != null) pizzaDrag.enabled = false;
            if (ovenSlot != null) ovenSlot.Filled += OnOvenFilled;
            if (ovenButton != null) ovenButton.Pressed += OnOvenTapped;
            foreach (GameObject l in wholeLayers) if (l != null) l.SetActive(false);
            foreach (Transform s in slices) s.gameObject.SetActive(false);
            foreach (DropSlot p in plates) p.Filled += OnServed;
            if (mice != null) mice.SetActive(false);
            foreach (GameObject f in spicyFaces) if (f != null) f.SetActive(false);
            if (ovenGlow != null) ovenGlow.color = new Color(1f, 0.5f, 0.1f, 0f);
            if (cutMark != null) cutMark.enabled = false;
            if (randomRecipe) PickRecipe();
            ShowRecipe();
            voice = gameObject.AddComponent<AudioSource>();
            if (bubble != null) { bubble.Show("Pizza time! Grandma's recipe is on the board."); StartCoroutine(HideLater(2.4f)); }
        }

        /// <summary>3 o 4 ingredienti a caso (sempre almeno pomodoro o mozzarella); il peperoncino mai.</summary>
        void PickRecipe()
        {
            var pool = new List<string>(RecipePool);
            for (int i = pool.Count - 1; i > 0; i--) { int j = Random.Range(0, i + 1); (pool[i], pool[j]) = (pool[j], pool[i]); }
            int n = Random.Range(3, 5);
            var pick = pool.GetRange(0, n);
            if (!pick.Contains("tomato") && !pick.Contains("mozzarella")) pick[n - 1] = Random.value < 0.5f ? "tomato" : "mozzarella";
            pick.Sort((a, b) => System.Array.IndexOf(RecipePool, a).CompareTo(System.Array.IndexOf(RecipePool, b)));
            required = pick.ToArray();
        }

        void ShowRecipe()
        {
            if (recipeIcons == null) return;
            for (int i = 0; i < recipeIcons.Length; i++)
            {
                bool on = i < required.Length;
                int k = on ? System.Array.IndexOf(RecipePool, required[i]) : -1;
                if (recipeIcons[i] != null)
                {
                    recipeIcons[i].gameObject.SetActive(on && k >= 0);
                    if (on && k >= 0 && chalkSprites != null && k < chalkSprites.Length) recipeIcons[i].sprite = chalkSprites[k];
                }
                if (recipeNames != null && i < recipeNames.Length && recipeNames[i] != null)
                {
                    recipeNames[i].gameObject.SetActive(on && k >= 0);
                    if (on && k >= 0) recipeNames[i].text = RecipeLabels[k];
                }
            }
        }

        void OnDestroy()
        {
            if (squeak != null) Destroy(squeak);
            if (bowls != null) foreach (Draggable2D b in bowls) if (b != null) b.Dropped -= OnIngredient;
            if (plates != null) foreach (DropSlot p in plates) if (p != null) p.Filled -= OnServed;
            if (ovenSlot != null) ovenSlot.Filled -= OnOvenFilled;
            if (ovenButton != null) ovenButton.Pressed -= OnOvenTapped;
        }

        // ---------- 1. condimenti ----------

        void OnIngredient(Draggable2D bowl, DropSlot slot)
        {
            if (phase != Phase.Toppings) return;
            // la ciotola è già tornata al suo posto (returnOnMiss): conta dove è stata lasciata, cioè il dito
            Vector2 at = PointerInput.TryGetPrimary(out Vector2 sp, out _, out _) ? PointerInput.ToWorld(cam, sp) : (Vector2)bowl.transform.position;
            if (Vector2.Distance(at, pizza.position) > dropRadius) return;
            if (!added.Add(bowl.itemId)) return;
            Haptics.Light();
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

        void OnOvenFilled(DropSlot slot, Draggable2D d)
        {
            if (phase == Phase.Toppings && added.Count > 0) StartCoroutine(Bake());
        }

        void OnOvenTapped(TapButton b)
        {
            if (phase != Phase.Toppings) return;
            if (added.Count == 0) { if (bubble != null) { bubble.Show("Toppings first!"); StartCoroutine(HideLater(1.3f)); } return; }
            StartCoroutine(Bake());
        }

        IEnumerator Bake()
        {
            phase = Phase.Baking;
            pizzaDrag.enabled = false;
            foreach (Draggable2D b in bowls) b.gameObject.SetActive(false);
            Vector3 home = pizzaHome();
            Vector3 inOven = ovenMouth.bounds.center;
            Vector3 scale = pizza.localScale;
            Vector3 from = pizza.position;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.35f)   // la pizza vola dentro il forno
            {
                pizza.position = Vector3.Lerp(from, inOven, k) + Vector3.up * Mathf.Sin(k * Mathf.PI) * 0.4f;
                pizza.localScale = Vector3.Lerp(scale, scale * 0.45f, k);
                yield return null;
            }
            pizza.position = inOven;
            pizza.localScale = scale * 0.45f;
            Haptics.Light();
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
            if (phase == Phase.Toppings && ovenGlow != null)   // il forno "chiama" quando la pizza ha i condimenti
                ovenGlow.color = new Color(1f, 0.6f, 0.2f, added.Count > 0 ? 0.25f + Mathf.PingPong(Time.time, 0.5f) * 0.6f : 0f);
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
            Haptics.Light();
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
            Haptics.Light();
            StartCoroutine(Munch(slice.transform, plate.transform.position + new Vector3(0f, 0.45f, 0f)));
            if (served >= plates.Length) StartCoroutine(Verdict());
        }

        /// <summary>Il topolino si mangia la fetta: rimpicciolisce verso la bocca e sparisce (così si vede la faccia).</summary>
        static IEnumerator Munch(Transform slice, Vector3 mouth)
        {
            slice.GetComponent<Draggable2D>().enabled = false;
            Vector3 from = slice.position, scale = slice.localScale;
            yield return new WaitForSeconds(0.15f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.45f)
            {
                slice.position = Vector3.Lerp(from, mouth, k);
                slice.localScale = scale * (1f - k) * (1f + Mathf.Sin(k * 40f) * 0.08f);   // gnam gnam
                yield return null;
            }
            slice.gameObject.SetActive(false);
        }

        IEnumerator Verdict()
        {
            phase = Phase.Done;
            yield return new WaitForSeconds(0.7f);   // il tempo di finire l'ultima fetta
            if (added.Contains("chili"))
            {
                // EASTER EGG: piccantissima! strillano, paonazzi, furiosi, occhi in fiamme, lingua penzolante
                if (bubble != null) bubble.Show("HOT!!! MAMMA MIAAA!!!");
                Haptics.Medium();
                StartCoroutine(Squeaks());
                foreach (GameObject f in spicyFaces) if (f != null) f.SetActive(true);
                if (steam != null) { steam.puffsPerSecond = 18f; steam.transform.position = pizzaHome() + Vector3.up * 0.6f; steam.Play(); }
                var bases = new Vector3[miceArt.Length];
                for (int i = 0; i < miceArt.Length; i++) if (miceArt[i] != null) bases[i] = miceArt[i].transform.parent.localPosition;
                for (float k = 0f; k < 2.6f; k += Time.deltaTime)
                {
                    float t = Mathf.Clamp01(k / 0.6f);
                    for (int i = 0; i < miceArt.Length; i++)
                    {
                        SpriteRenderer m = miceArt[i];
                        if (m == null) continue;
                        m.color = Color.Lerp(Color.white, new Color(0.85f, 0.25f, 0.45f), t);
                        m.transform.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(k * 30f + i * 1.7f) * 9f);   // tremano dalla rabbia
                        m.transform.parent.localPosition = bases[i] + Vector3.up * Mathf.Abs(Mathf.Sin(k * 9f + i)) * 0.12f;   // saltellano
                        AnimateSpicy(i < spicyFaces.Length ? spicyFaces[i] : null, k + i * 0.37f, t);
                    }
                    yield return null;
                }
                if (steam != null) steam.Stop();
                yield return new WaitForSeconds(0.6f);
                FailWithEgg(eggId);
                yield break;
            }
            bool missing = false, extra = false;
            foreach (string r in required) if (!added.Contains(r)) missing = true;
            foreach (string a in added) if (System.Array.IndexOf(required, a) < 0) extra = true;
            bool ok = !missing && !extra;
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
                string why = extra && added.Contains("pineapple") && System.Array.IndexOf(required, "pineapple") < 0 ? "PINEAPPLE?! Yuck!"
                    : extra ? "Yuck! That's not on the board!" : "Hmm... something's missing.";
                if (bubble != null) bubble.Show(why);
                yield return new WaitForSeconds(1.6f);
                Fail();
            }
        }

        /// <summary>Fiamme negli occhi che divampano e tremolano, lingua che penzola e sventola, vena che pulsa.</summary>
        static void AnimateSpicy(GameObject face, float time, float grow)
        {
            if (face == null) return;
            foreach (Transform part in face.transform)
            {
                if (part.name == "FireEye")
                {
                    float flick = 1f + Mathf.Sin(time * 27f) * 0.18f + Mathf.PerlinNoise(time * 9f, part.localPosition.x * 10f) * 0.35f;
                    part.localScale = new Vector3(1.5f * (1f - (flick - 1f) * 0.4f), 1.5f * flick * Mathf.Lerp(0.3f, 1.25f, grow), 1f);
                    part.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(time * 13f + part.localPosition.x * 20f) * 10f);
                }
                else if (part.name == "Tongue")
                {
                    part.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(time * 14f) * 22f);
                    part.localScale = new Vector3(1.3f, 1.3f * (1f + Mathf.Abs(Mathf.Sin(time * 7f)) * 0.45f) * grow, 1f);
                }
                else if (part.name == "Anger")
                    part.localScale = Vector3.one * (1f + Mathf.Abs(Mathf.Sin(time * 10f)) * 0.4f);
            }
        }

        /// <summary>Quattro urletti acuti (come il pupazzo di neve, ma da topolino), uno dopo l'altro.</summary>
        IEnumerator Squeaks()
        {
            if (voice == null) yield break;
            if (squeak == null) squeak = ScreamSynth.Mouse();
            for (int i = 0; i < 4; i++)
            {
                voice.pitch = Random.Range(0.9f, 1.15f);
                voice.PlayOneShot(squeak, 0.7f);
                yield return new WaitForSeconds(Random.Range(0.12f, 0.25f));
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
