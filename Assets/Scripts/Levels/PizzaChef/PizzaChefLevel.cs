using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;
using Gotcha.Levels.V64;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 32 — pizza chef. Nove ciotole di ingredienti sul bancone (pomodoro, mozzarella, funghi, olive,
    /// prosciutto, salamino, carciofi, peperoncino, ananas): si trascinano sulla pasta per condire la pizza, e i pezzi
    /// cadono sparsi a caso sulla pizza (ruotati, mai ammucchiati: <see cref="PizzaChefScatter"/>). La ricetta della
    /// nonna è sulla lavagna: 4 o 5 ingredienti scelti a caso a ogni partita (mai l'ananas), disegnati col gesso su due
    /// righe col nome sotto. Poi si inforna, si taglia in quattro (due tagli dal centro) e si dà una fetta a ogni
    /// topolino, che la mangia. La logica (ricetta, fasi, esito, pezzi sparsi) è in <see cref="PizzaChefLogic"/>.
    /// - Esattamente la ricetta: festa. Manca qualcosa o c'è un ingrediente in più: "Yuck!", si riprova.
    /// - ANANAS: sconfitta SEMPRE, subito, con una presa in giro della nonna (a rotazione), in qualunque momento: prima
    ///   o dopo gli altri ingredienti, sulla pizza in forno, già cotta o già tagliata, anche se la ricetta era giusta.
    /// - Easter egg `spicy_pizza` ("Inferno Pizza"): col peperoncino i topi strillano, diventano paonazzi e furiosi,
    ///   con gli occhi in fiamme che divampano e la lingua penzolante.
    /// </summary>
    public class PizzaChefLevel : LevelController
    {
        [Header("Ingredienti (ciotole trascinabili) e strati della pizza")]
        [SerializeField] Draggable2D[] bowls;               // itemId: vedi PizzaChefLogic.Ingredients
        [SerializeField] string[] layerIds;                 // per ogni strato, l'ingrediente che lo accende
        [SerializeField] GameObject[] wholeLayers;           // strati sulla pizza intera (i condimenti a pezzi hanno i figli "Bit")
        [SerializeField] SpriteRenderer[] bakeTint;          // pasta e mozzarella: dorate dopo il forno
        [SerializeField] Transform pizza;
        [SerializeField] float dropRadius = 1.2f;
        [SerializeField] string[] required = { "tomato", "mozzarella", "mushroom", "olive" };
        [Header("Ricetta casuale sulla lavagna")]
        [SerializeField] bool randomRecipe = true;
        [SerializeField] Transform[] recipeSlots;           // 5 posti sulla lavagna (icona + nome), su due righe
        [SerializeField] SpriteRenderer[] recipeIcons;
        [SerializeField] TextMesh[] recipeNames;
        [SerializeField] Sprite[] chalkSprites;             // stesso ordine di PizzaChefLogic.RecipePool

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

        PizzaChefLogic logic;
        Draggable2D pizzaDrag;
        Camera cam;
        Vector2 cutStart;
        bool cutting, bowlHeld;
        int bowlFrame = -1;
        int cuts, served;
        float firstAngle;
        Vector3 pizzaScale0;
        readonly List<Vector3> trailPts = new List<Vector3>();
        readonly List<float> trailTimes = new List<float>();
        const float Radius = 1.1f;
        static int tauntTurn;   // le prese in giro dell'ananas girano tra un tentativo e l'altro
        AudioSource voice;
        AudioClip squeak;

        /// <summary>I suggerimenti dicono la ricetta uscita in questa partita (e ricordano la regola dell'ananas).</summary>
        public override string[] DynamicHints => logic == null ? null : new[]
        {
            "Read the board carefully. Grandma has one rule: NEVER pineapple.",
            "Tonight: " + RecipeText() + ". Nothing else, no pineapple! Bake it, cut it in four, serve the mice.",
        };

        string RecipeText()
        {
            var names = new List<string>();
            foreach (string r in logic.Required) names.Add(PizzaChefLogic.LabelOf(r));
            if (names.Count < 2) return string.Join("", names);
            return string.Join(", ", names.GetRange(0, names.Count - 1)) + " and " + names[names.Count - 1];
        }

        public override void OnLevelStart()
        {
            cam = Camera.main;
            // seme diverso a ogni tentativo: ricetta e pezzi sparsi cambiano, ma restano riproducibili dalla logica
            logic = new PizzaChefLogic(Random.Range(int.MinValue, int.MaxValue), randomRecipe ? null : required);
            required = logic.Required;
            foreach (Draggable2D b in bowls)
            {
                b.Dropped += OnIngredient;
                b.DragStarted += OnBowlGrabbed;
            }
            pizzaDrag = pizza.GetComponent<Draggable2D>();
            if (pizzaDrag != null) pizzaDrag.enabled = false;
            pizzaScale0 = pizza.localScale;
            if (ovenSlot != null) ovenSlot.Filled += OnOvenFilled;
            if (ovenButton != null) ovenButton.Pressed += OnOvenTapped;
            foreach (GameObject l in wholeLayers) if (l != null) l.SetActive(false);
            foreach (Transform s in slices) s.gameObject.SetActive(false);
            foreach (DropSlot p in plates) p.Filled += OnServed;
            if (mice != null) mice.SetActive(false);
            foreach (GameObject f in spicyFaces) if (f != null) f.SetActive(false);
            if (ovenGlow != null) ovenGlow.color = new Color(1f, 0.5f, 0.1f, 0f);
            if (cutMark != null) cutMark.enabled = false;
            ShowRecipe();
            voice = gameObject.AddComponent<AudioSource>();
            Say("Pizza time! Grandma's recipe is on the board.", 2.4f);
        }

        /// <summary>La ricetta sulla lavagna: 4 ingredienti su due righe da 2, 5 su una riga da 3 e una da 2.</summary>
        void ShowRecipe()
        {
            int n = required.Length;
            int top = n > 4 ? 3 : Mathf.Min(n, 2);
            for (int i = 0; i < Mathf.Max(recipeSlots != null ? recipeSlots.Length : 0, recipeIcons != null ? recipeIcons.Length : 0); i++)
            {
                bool on = i < n;
                int k = on ? System.Array.IndexOf(PizzaChefLogic.RecipePool, required[i]) : -1;
                bool show = on && k >= 0;
                if (recipeSlots != null && i < recipeSlots.Length && recipeSlots[i] != null)
                {
                    int row = i < top ? 0 : 1, inRow = row == 0 ? top : n - top, col = row == 0 ? i : i - top;
                    recipeSlots[i].localPosition = new Vector3((col - (inRow - 1) * 0.5f) * 0.74f, row == 0 ? 0.28f : -0.32f, recipeSlots[i].localPosition.z);
                    recipeSlots[i].gameObject.SetActive(show);
                }
                if (recipeIcons != null && i < recipeIcons.Length && recipeIcons[i] != null)
                {
                    recipeIcons[i].gameObject.SetActive(show);
                    if (show && chalkSprites != null && k < chalkSprites.Length && chalkSprites[k] != null) recipeIcons[i].sprite = chalkSprites[k];
                }
                if (recipeNames != null && i < recipeNames.Length && recipeNames[i] != null)
                {
                    recipeNames[i].gameObject.SetActive(show);
                    if (show) recipeNames[i].text = PizzaChefLogic.LabelOf(required[i]);
                }
            }
        }

        void OnDestroy()
        {
            if (squeak != null) Destroy(squeak);
            if (bowls != null) foreach (Draggable2D b in bowls) if (b != null) { b.Dropped -= OnIngredient; b.DragStarted -= OnBowlGrabbed; }
            if (plates != null) foreach (DropSlot p in plates) if (p != null) p.Filled -= OnServed;
            if (ovenSlot != null) ovenSlot.Filled -= OnOvenFilled;
            if (ovenButton != null) ovenButton.Pressed -= OnOvenTapped;
        }

        // ---------- 1. condimenti ----------

        void OnBowlGrabbed(Draggable2D bowl)
        {
            bowlHeld = true;
            bowlFrame = Time.frameCount;
            cutting = false;   // trascinare una ciotola non è un taglio
        }

        void OnIngredient(Draggable2D bowl, DropSlot slot)
        {
            bowlHeld = false;
            bowlFrame = Time.frameCount;
            if (logic == null || logic.IsDone || IsFinished) return;
            // la ciotola è già tornata al suo posto (returnOnMiss): conta dove è stata lasciata, cioè il dito
            Vector2 at = PointerInput.TryGetPrimary(out Vector2 sp, out _, out _) ? PointerInput.ToWorld(cam, sp) : (Vector2)bowl.transform.position;
            // la pizza in forno è più piccola; tagliata, gli spicchi stanno un po' più larghi
            float reach = dropRadius * Mathf.Max(0.6f, pizzaScale0.x > 0f ? pizza.localScale.x / pizzaScale0.x : 1f);
            if (logic.Phase == PizzaChefPhase.Serving || logic.Phase == PizzaChefPhase.Judging) reach = dropRadius * 1.3f;
            if (Vector2.Distance(at, pizza.position) > reach) return;

            PizzaChefAdd result = logic.TryAdd(bowl.itemId);
            switch (result)
            {
                case PizzaChefAdd.Pineapple:
                    PineappleFail(bowl.itemId);
                    return;
                case PizzaChefAdd.TooLate:
                    Say(logic.Phase == PizzaChefPhase.Baking ? "It's in the oven already!" : "Too late, it's baked!", 1.4f);
                    return;
                case PizzaChefAdd.Added:
                    Haptics.Light();
                    PutOn(bowl.itemId);
                    if (pizzaDrag != null && !pizzaDrag.enabled)
                    {
                        pizzaDrag.enabled = true;   // ora la pizza si può infornare
                        Say("Into the oven when ready!", 1.6f);
                    }
                    return;
            }
        }

        /// <summary>Accende lo strato dell'ingrediente: sugo e mozzarella interi, gli altri a pezzi sparsi a caso.</summary>
        void PutOn(string id)
        {
            List<PizzaChefPiece> pieces = null;
            for (int i = 0; i < layerIds.Length; i++)
            {
                if (layerIds[i] != id || i >= wholeLayers.Length || wholeLayers[i] == null) continue;
                Transform layer = wholeLayers[i].transform;
                int bits = 0;
                foreach (Transform b in layer) if (b.name == "Bit") bits++;
                if (bits > 0)
                {
                    if (pieces == null) pieces = logic.PlacePieces(bits);
                    int j = 0;
                    foreach (Transform b in layer)
                    {
                        if (b.name != "Bit") continue;
                        PizzaChefPiece p = pieces[j++];
                        b.localPosition = new Vector3(p.X, p.Y, b.localPosition.z);
                        b.localRotation = Quaternion.Euler(0, 0, p.Rotation);
                    }
                }
                wholeLayers[i].SetActive(true);
                StartCoroutine(Plop(layer));
                if (pizza.gameObject.activeSelf == false || slicesShown) CopyToSlices(layer, id);   // già tagliata
            }
            foreach (Transform s in slices)
                foreach (Transform layer in s)
                    if (layer.name == "Layer_" + id) layer.gameObject.SetActive(true);
        }

        static IEnumerator Plop(Transform t)
        {
            Vector3 s = t.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.2f) { t.localScale = s * (1f + Mathf.Sin(k * Mathf.PI) * 0.12f); yield return null; }
            t.localScale = s;
        }

        // ---------- ananas: sempre sconfitta ----------

        /// <summary>
        /// L'ananas è arrivato sulla pizza: si ferma tutto (forno, festa, uovo...), la nonna prende in giro il giocatore
        /// e dopo un attimo Fail con la stessa frase (che si legge anche nella schermata GOTCHA!). Una volta sola.
        /// </summary>
        void PineappleFail(string id)
        {
            StopAllCoroutines();   // niente vittoria, niente uovo, niente verdetto dopo questo
            cutting = false;
            if (bowls != null) foreach (Draggable2D b in bowls) if (b != null) b.enabled = false;
            if (pizzaDrag != null) pizzaDrag.enabled = false;
            foreach (Transform s in slices) { Draggable2D d = s.GetComponent<Draggable2D>(); if (d != null) d.enabled = false; }
            if (hearts != null) hearts.Stop();
            if (ovenSmoke != null) ovenSmoke.Stop();
            Haptics.Medium();
            PutOn(id);   // si vede l'ananas sulla pizza (o sugli spicchi)
            string taunt = PizzaChefLogic.Taunt(tauntTurn++);
            if (bubble != null) bubble.Show(taunt);
            StartCoroutine(PineappleShame(taunt));
        }

        IEnumerator PineappleShame(string taunt)
        {
            // i topolini (se ci sono) scuotono la testa disgustati
            float t0 = Time.time;
            while (Time.time - t0 < 2.4f)
            {
                float k = Time.time - t0;
                if (miceArt != null)
                    for (int i = 0; i < miceArt.Length; i++)
                        if (miceArt[i] != null) miceArt[i].transform.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(k * 18f + i) * 12f * Mathf.Clamp01(2.4f - k));
                yield return null;
            }
            if (!IsFinished) Fail(taunt);
        }

        // ---------- 2. forno ----------

        void OnOvenFilled(DropSlot slot, Draggable2D d)
        {
            if (logic != null && logic.StartBake()) StartCoroutine(Bake());
        }

        void OnOvenTapped(TapButton b)
        {
            if (logic == null || logic.Phase != PizzaChefPhase.Toppings) return;
            if (logic.AddedCount == 0) { Say("Toppings first!", 1.3f); return; }
            if (logic.StartBake()) StartCoroutine(Bake());
        }

        IEnumerator Bake()
        {
            pizzaDrag.enabled = false;
            Vector3 home = pizzaHome();
            Vector3 inOven = ovenMouth.bounds.center;
            Vector3 scale = pizzaScale0;
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
            pizza.position = home;
            pizza.localScale = scale;
            if (steam != null) { steam.transform.position = home; steam.Play(); }
            if (mice != null) mice.SetActive(true);
            Say("Smells great! Now cut it in four.", 1.8f);
            logic.BakeDone();
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
            if (logic == null) return;
            if (logic.Phase == PizzaChefPhase.Toppings && ovenGlow != null)   // il forno "chiama" quando la pizza ha i condimenti
                ovenGlow.color = new Color(1f, 0.6f, 0.2f, logic.AddedCount > 0 ? 0.25f + Mathf.PingPong(Time.time, 0.5f) * 0.6f : 0f);
            if (logic.Phase != PizzaChefPhase.Cutting || IsFinished) return;
            if (!PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out bool ended)) return;
            Vector2 w = PointerInput.ToWorld(cam, sp);
            bool bowlStroke = bowlHeld || bowlFrame == Time.frameCount;   // le ciotole restano sul bancone: non sono tagli
            if (began && !bowlStroke) { cutStart = w; cutting = true; }
            if (cutting) { trailPts.Add(new Vector3(w.x, w.y, 0f)); trailTimes.Add(Time.time); }
            if (ended && cutting) { cutting = false; if (!bowlStroke) TryCut(cutStart, w); }
        }

        void TryCut(Vector2 a, Vector2 b)
        {
            Vector2 c = pizza.position;
            if (Vector2.Distance(a, c) < Radius * 0.9f || Vector2.Distance(b, c) < Radius * 0.9f || (b - a).magnitude < Radius * 1.6f) return;
            Vector2 dir = (b - a).normalized;
            float off = Mathf.Abs((c.x - a.x) * dir.y - (c.y - a.y) * dir.x);
            float along = Vector2.Dot(c - a, dir);
            if (along < 0f || along > (b - a).magnitude) return;
            if (off > 0.32f) { Say("Through the middle!", 1.3f); return; }
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
            if (logic.CutDone()) StartCoroutine(Split());
        }

        bool slicesShown;

        IEnumerator Split()
        {
            if (cutMark != null) cutMark.enabled = false;
            if (steam != null) steam.Stop();
            // i pezzi sparsi a caso finiscono sullo spicchio giusto, nella stessa posizione
            for (int i = 0; i < wholeLayers.Length && i < layerIds.Length; i++)
                if (wholeLayers[i] != null && wholeLayers[i].activeSelf) CopyToSlices(wholeLayers[i].transform, layerIds[i]);
            slicesShown = true;
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

        /// <summary>
        /// Copia i pezzi di uno strato della pizza intera sugli spicchi: ogni pezzo va sullo spicchio che copre il suo
        /// angolo (lo spicchio i copre da firstAngle + 90·i a firstAngle + 90·(i+1); l'angolo della pizza è in (-c, -c)
        /// nelle coordinate dello spicchio, c = baricentro / √2).
        /// </summary>
        void CopyToSlices(Transform wholeLayer, string id)
        {
            if (slices == null || slices.Length == 0) return;
            float c = 4f * Radius / (3f * Mathf.PI) / Mathf.Sqrt(2f);
            foreach (Transform bit in wholeLayer)
            {
                if (bit.name != "Bit") continue;
                SpriteRenderer src = bit.GetComponent<SpriteRenderer>();
                if (src == null) continue;
                Vector2 p = bit.localPosition;
                float ang = Mathf.Atan2(p.y, p.x) * Mathf.Rad2Deg, r = p.magnitude;
                float rel = Mathf.Repeat(ang - firstAngle, 360f);
                int i = Mathf.Clamp((int)(rel / 90f), 0, slices.Length - 1);
                float local = (rel - i * 90f) * Mathf.Deg2Rad;
                Transform layer = slices[i].Find("Layer_" + id);
                if (layer == null) layer = slices[i];
                Transform dough = slices[i].Find("Dough");
                SpriteRenderer doughSr = dough != null ? dough.GetComponent<SpriteRenderer>() : null;
                var go = new GameObject("Bit");
                go.transform.SetParent(layer, false);
                go.transform.localPosition = new Vector3(Mathf.Cos(local) * r - c, Mathf.Sin(local) * r - c, 0f);
                go.transform.localRotation = Quaternion.Euler(0, 0, bit.localEulerAngles.z - firstAngle - i * 90f);
                go.transform.localScale = bit.localScale;
                SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = src.sprite;
                sr.color = src.color;
                sr.sortingOrder = (doughSr != null ? doughSr.sortingOrder : 12 + i * 8) + 3;
            }
        }

        // ---------- 4. servizio ed esito ----------

        void OnServed(DropSlot plate, Draggable2D slice)
        {
            if (logic == null || logic.Phase != PizzaChefPhase.Serving) return;
            served++;
            Haptics.Light();
            StartCoroutine(Munch(slice.transform, plate.transform.position + new Vector3(0f, 0.45f, 0f)));
            if (served >= plates.Length) StartCoroutine(Verdict(logic.ServedAll()));
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

        IEnumerator Verdict(PizzaChefVerdict verdict)
        {
            if (verdict == PizzaChefVerdict.None) yield break;
            yield return new WaitForSeconds(0.7f);   // il tempo di finire l'ultima fetta
            if (verdict == PizzaChefVerdict.SpicyEgg)
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
                if (logic.Commit() == PizzaChefVerdict.SpicyEgg && !IsFinished) FailWithEgg(eggId);
                yield break;
            }
            if (verdict == PizzaChefVerdict.Win)
            {
                if (hearts != null) hearts.Play();
                if (bubble != null) bubble.Show("Just like Grandma's!");
                yield return new WaitForSeconds(1.6f);
                if (hearts != null) hearts.Stop();
                if (logic.Commit() == PizzaChefVerdict.Win && !IsFinished) Win();
            }
            else
            {
                string why = verdict == PizzaChefVerdict.Extra ? "Yuck! That's not on the board!" : "Hmm... something's missing.";
                if (bubble != null) bubble.Show(why);
                yield return new WaitForSeconds(1.6f);
                if (logic.Commit() != PizzaChefVerdict.None && !IsFinished) Fail();
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

        void Say(string text, float seconds)
        {
            if (bubble == null) return;
            bubble.Show(text);
            StartCoroutine(HideLater(seconds));
        }

        IEnumerator HideLater(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (bubble != null && logic != null && logic.Phase != PizzaChefPhase.Judging && !logic.IsDone) bubble.Hide();
        }
    }
}
