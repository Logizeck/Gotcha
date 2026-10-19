using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "Potion lab" — tre fasi. 1) La ricetta è un indovinello nel libro degli incantesimi ("first the tears of
    /// the sky..." = acqua): trascina i 4 ingredienti giusti nel calderone nell'ordine giusto (sbagli = il calderone
    /// rutta fumo verde, l'ordine riparte e si perde un tentativo su 3). 2) Mescola: disegna 5 cerchi in senso orario
    /// nel calderone (in senso antiorario si torna indietro). 3) Scalda: soffia sul fuoco (microfono) finché il
    /// termometro è nel verde e tienilo lì 2 secondi (troppo = trabocca e il calore torna a metà). Finale: la pozione
    /// trasforma per sbaglio il gatto in una rana... per un secondo. "Oops."
    /// </summary>
    public class PotionLabLevel : LevelController
    {
        [SerializeField] TextMesh goalText;
        [SerializeField] SpriteRenderer[] dots;            // 3 fasi
        [SerializeField] SpriteRenderer[] hearts;          // 3 tentativi
        [SerializeField] TextMesh leftPage;
        [SerializeField] TextMesh rightPage;
        [SerializeField] Transform[] ingredients;          // 8 ingredienti (con Collider2D), nell'ordine di Clues
        [SerializeField] Transform cauldronMouth;
        [SerializeField] SpriteRenderer liquid;
        [SerializeField] SmokeEmitter burp;
        [SerializeField] SmokeEmitter steam;
        [SerializeField] Transform ladle;
        [SerializeField] TextMesh counter;
        [SerializeField] Transform fire;
        [SerializeField] Transform mercury;                // scala y = calore 0..1
        [SerializeField] Transform cat;
        [SerializeField] Transform frog;
        [SerializeField] Transform zap;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] float shelfY = -3.75f, shelfGap = 0.92f;

        static readonly string[] Clues =
        {
            "the tears of the sky", "what the cow gives", "the hen's little gift", "the bees' sweet gold",
            "the forest's tiny umbrella", "a tickle from a bird", "the mouse's dream", "the cat's favourite dinner",
        };
        static readonly Color[] Tints =
        {
            new Color(0.45f, 0.75f, 1f), new Color(0.95f, 0.95f, 0.92f), new Color(1f, 0.9f, 0.55f), new Color(1f, 0.75f, 0.25f),
            new Color(0.9f, 0.4f, 0.35f), new Color(0.55f, 0.85f, 0.55f), new Color(1f, 0.85f, 0.3f), new Color(0.95f, 0.6f, 0.35f),
        };
        static readonly string[] Prefix = { "First, ", "Then ", "Then ", "Last, " };
        static readonly Color Done = new Color(0.42f, 0.88f, 0.48f);
        static readonly Color Water = new Color(0.35f, 0.6f, 0.75f);

        readonly List<int> recipe = new List<int>();
        readonly List<int> shown = new List<int>();
        Vector3[] home;
        Collider2D[] cols;
        int stage, step, tries = 3, dragging = -1;
        bool busy;
        Vector2 dragOffset;
        float bubbleUntil, clock;
        // fase 2
        float stirAngle, prevAngle;
        bool stirring, warnedWrongWay;
        int shownTurns;
        // fase 3
        float heat = 0.3f, greenTime;
        Camera cam;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            home = new Vector3[ingredients.Length];
            cols = new Collider2D[ingredients.Length];
            for (int i = 0; i < ingredients.Length; i++) cols[i] = ingredients[i].GetComponent<Collider2D>();
            var pool = new List<int>();
            for (int i = 0; i < ingredients.Length; i++) pool.Add(i);
            for (int k = 0; k < 6; k++) { int j = Random.Range(0, pool.Count); shown.Add(pool[j]); pool.RemoveAt(j); }
            for (int k = 0; k < 4; k++) recipe.Add(shown[k]);    // i primi 4 sono la ricetta (in ordine)
            var onShelf = new List<int>(shown);
            for (int i = onShelf.Count - 1; i > 0; i--) { int j = Random.Range(0, i + 1); (onShelf[i], onShelf[j]) = (onShelf[j], onShelf[i]); }
            for (int i = 0; i < ingredients.Length; i++) ingredients[i].gameObject.SetActive(false);
            for (int s = 0; s < onShelf.Count; s++)
            {
                int id = onShelf[s];
                home[id] = new Vector3((s - 2.5f) * shelfGap, shelfY, 0f);
                ingredients[id].localPosition = home[id];
                ingredients[id].gameObject.SetActive(true);
            }
            liquid.color = Water;
            ladle.gameObject.SetActive(false);
            frog.gameObject.SetActive(false);
            zap.gameObject.SetActive(false);
            counter.text = "";
            SetMercury();
            foreach (SpriteRenderer d in dots) d.color = new Color(1f, 1f, 1f, 0.5f);
            dots[0].color = Color.white;
            WriteRecipe();
            goalText.text = "Read the riddle. Drop the ingredients in!";
        }

        void OnDestroy() { Inputs.ReleaseMic(this); }

        void Say(string text, float secs)
        {
            if (bubble == null) return;
            bubble.Show(text);
            bubbleUntil = clock + secs;
        }

        static string Wrap(string s)
        {
            if (s.Length < 18) return s;
            int mid = s.Length / 2, best = -1;
            for (int i = 0; i < s.Length; i++) if (s[i] == ' ' && (best < 0 || Mathf.Abs(i - mid) < Mathf.Abs(best - mid))) best = i;
            return best < 0 ? s : s.Substring(0, best) + "\n" + s.Substring(best + 1);
        }

        string Step(int k)
        {
            string line = (k + 1) + ". " + Wrap(Prefix[k] + Clues[recipe[k]] + ".");
            return k < step ? "<color=#3FA34D>" + line + "</color>" : line;
        }

        void WriteRecipe()
        {
            leftPage.text = Step(0) + "\n\n" + Step(1);
            rightPage.text = Step(2) + "\n\n" + Step(3);
        }

        void Update()
        {
            if (IsFinished) return;
            clock += Time.deltaTime;
            if (bubble != null && bubbleUntil > 0f && clock > bubbleUntil) { bubble.Hide(); bubbleUntil = 0f; }
            if (busy) return;
            if (stage == 0) UpdateIngredients();
            else if (stage == 1) UpdateStir();
            else if (stage == 2) UpdateHeat();
        }

        // ---------- Fase 1: la ricetta ----------

        void UpdateIngredients()
        {
            bool has = PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out bool ended);
            Vector2 w = has ? PointerInput.ToWorld(cam, sp) : Vector2.zero;
            if (dragging >= 0)
            {
                Transform t = ingredients[dragging];
                if (has) t.position = new Vector3(w.x + dragOffset.x, w.y + dragOffset.y, t.position.z);
                if (!has || ended)
                {
                    int id = dragging;
                    dragging = -1;
                    SetOrder(t, 6);
                    if (Vector2.Distance(t.position, cauldronMouth.position) < 1.25f) StartCoroutine(DropIn(id));
                    else StartCoroutine(MoveTo(t, home[id], 0.2f));
                }
                return;
            }
            if (!has || !began || PointerInput.IsOverUi()) return;
            for (int i = 0; i < ingredients.Length; i++)
            {
                if (!ingredients[i].gameObject.activeSelf || cols[i] == null || !cols[i].OverlapPoint(w)) continue;
                dragging = i;
                dragOffset = (Vector2)ingredients[i].position - w;
                SetOrder(ingredients[i], 20);
                Sfx.Play(this, "tick", 0.5f);
                return;
            }
        }

        static void SetOrder(Transform t, int order)
        {
            foreach (SpriteRenderer sr in t.GetComponentsInChildren<SpriteRenderer>()) sr.sortingOrder = order;
        }

        IEnumerator MoveTo(Transform t, Vector3 to, float secs)
        {
            Vector3 from = t.localPosition;
            for (float k = 0f; k < 1f; k += Time.deltaTime / secs)
            {
                t.localPosition = Vector3.Lerp(from, to, k);
                yield return null;
            }
            t.localPosition = to;
        }

        IEnumerator DropIn(int id)
        {
            busy = true;
            Transform t = ingredients[id];
            Vector3 from = t.position, to = cauldronMouth.position;
            Vector3 scale = t.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.3f)
            {
                t.position = Vector3.Lerp(from, to, k) + Vector3.up * Mathf.Sin(k * Mathf.PI) * 0.4f;
                t.localScale = scale * (1f - k * 0.7f);
                yield return null;
            }
            t.gameObject.SetActive(false);
            t.localScale = scale;
            Sfx.Play(this, "pop", 0.8f);
            StartCoroutine(Wobble(liquid.transform));

            if (recipe[step] == id)
            {
                liquid.color = Color.Lerp(liquid.color, Tints[id], 0.6f);
                step++;
                WriteRecipe();
                if (step >= recipe.Count)
                {
                    yield return new WaitForSeconds(0.4f);
                    yield return StageDone(0);
                    StartStir();
                }
                busy = false;
                yield break;
            }

            // sbagliato: rutto verde, l'ordine riparte
            liquid.color = new Color(0.45f, 0.8f, 0.3f);
            burp.Play();
            Sfx.Play(this, "bonk", 0.9f);
            Sfx.Play(this, "whoosh", 0.7f);
            Haptics.Medium();
            tries--;
            for (int i = 0; i < hearts.Length; i++) hearts[i].color = i < tries ? Color.white : new Color(0.3f, 0.3f, 0.3f, 0.5f);
            Say(tries > 0 ? "BUUURP! Wrong order... start again." : "KA-BLOOP!", 1.8f);
            yield return new WaitForSeconds(1.0f);
            burp.Stop();
            if (tries <= 0)
            {
                Haptics.Heavy();
                for (float k = 0f; k < 1f; k += Time.deltaTime / 0.6f)
                {
                    liquid.transform.localScale = Vector3.one * (1f + k * 1.5f);
                    yield return null;
                }
                Fail();
                yield break;
            }
            step = 0;
            liquid.color = Water;
            WriteRecipe();
            foreach (int s in shown)
            {
                if (ingredients[s].gameObject.activeSelf) continue;
                ingredients[s].gameObject.SetActive(true);
                ingredients[s].localPosition = home[s] + Vector3.up * 2f;
                StartCoroutine(MoveTo(ingredients[s], home[s], 0.3f));
            }
            yield return new WaitForSeconds(0.35f);
            busy = false;
        }

        IEnumerator Wobble(Transform t)
        {
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.4f)
            {
                float s = 1f + Mathf.Sin(k * Mathf.PI * 3f) * 0.12f * (1f - k);
                t.localScale = new Vector3(s, 2f - s, 1f);
                yield return null;
            }
            t.localScale = Vector3.one;
        }

        IEnumerator StageDone(int finished)
        {
            dots[finished].color = Done;
            if (finished + 1 < dots.Length) dots[finished + 1].color = Color.white;
            Sfx.Play(this, "ding", 0.5f);
            steam.Play();
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.5f)
            {
                dots[finished].transform.localScale = Vector3.one * (1f + Mathf.Sin(k * Mathf.PI) * 0.6f);
                yield return null;
            }
            dots[finished].transform.localScale = Vector3.one;
            yield return new WaitForSeconds(0.4f);
            steam.Stop();
        }

        // ---------- Fase 2: mescolare ----------

        void StartStir()
        {
            stage = 1;
            goalText.text = "Stir clockwise 5 times!";
            ladle.gameObject.SetActive(true);
            PlaceLadle(-90f);
            counter.text = "STIR: 0 / 5";
            Say("Round and round!", 1.5f);
        }

        void PlaceLadle(float deg)
        {
            float a = deg * Mathf.Deg2Rad;
            ladle.position = cauldronMouth.position + new Vector3(Mathf.Cos(a) * 0.65f, Mathf.Sin(a) * 0.12f, 0f);
        }

        void UpdateStir()
        {
            bool has = PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out bool ended);
            if (!has || ended) { stirring = false; return; }
            Vector2 w = PointerInput.ToWorld(cam, sp);
            Vector2 d = w - (Vector2)cauldronMouth.position;
            if (d.magnitude < 0.15f || d.magnitude > 3f) { stirring = false; return; }
            float ang = Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;
            if (began || !stirring) { stirring = true; prevAngle = ang; PlaceLadle(ang); return; }
            float delta = Mathf.DeltaAngle(prevAngle, ang);
            prevAngle = ang;
            stirAngle = Mathf.Min(stirAngle + delta, 0f);   // orario = angolo negativo; non si va sotto zero giri
            PlaceLadle(ang);
            liquid.transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(stirAngle * Mathf.Deg2Rad) * 3f);
            int turns = Mathf.FloorToInt(-stirAngle / 360f);
            if (turns != shownTurns)
            {
                if (turns < shownTurns)
                {
                    Sfx.Play(this, "bonk", 0.5f);
                    if (!warnedWrongWay) { warnedWrongWay = true; Say("Wrong way! That undoes it.", 1.8f); }
                }
                else
                {
                    Sfx.Play(this, "pop", 0.7f);
                    Haptics.Light();
                    liquid.color = Color.Lerp(liquid.color, new Color(0.75f, 0.45f, 0.95f), 0.35f);
                }
                shownTurns = turns;
                counter.text = "STIR: " + Mathf.Min(turns, 5) + " / 5";
                if (turns >= 5) StartCoroutine(StirDone());
            }
        }

        IEnumerator StirDone()
        {
            busy = true;
            stirring = false;
            liquid.color = new Color(0.75f, 0.45f, 0.95f);
            yield return StageDone(1);
            ladle.gameObject.SetActive(false);
            liquid.transform.localRotation = Quaternion.identity;
            stage = 2;
            goalText.text = "Blow on the fire! Keep it in the green.";
            Say("Blow into the mic!", 1.8f);
            busy = false;
        }

        // ---------- Fase 3: il fuoco ----------

        void SetMercury() { mercury.localScale = new Vector3(1f, Mathf.Max(0.02f, heat), 1f); }

        void UpdateHeat()
        {
            float mic = Inputs.Mic(this);
            heat = Mathf.Clamp01(heat + (mic * 0.85f - 0.16f) * Time.deltaTime);
            SetMercury();
            float flicker = 1f + Mathf.Sin(clock * 23f) * 0.05f;
            fire.localScale = new Vector3(0.8f + mic * 0.4f, (0.5f + mic * 1.1f + heat * 0.4f) * flicker, 1f);
            bool green = heat >= 0.55f && heat <= 0.75f;
            greenTime = green ? greenTime + Time.deltaTime : 0f;
            counter.text = green ? "HOLD IT: " + Mathf.Min(greenTime, 2f).ToString("0.0") + " / 2.0" : heat > 0.75f ? "TOO HOT!" : "HEAT IT UP";
            if (heat >= 0.92f) { StartCoroutine(BoilOver()); return; }
            if (greenTime >= 2f) StartCoroutine(Finale());
        }

        IEnumerator BoilOver()
        {
            busy = true;
            Say("It's boiling over!", 1.6f);
            Sfx.Play(this, "whoosh", 0.8f);
            Haptics.Medium();
            steam.Play();
            Color c = liquid.color;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.8f)
            {
                liquid.transform.localScale = new Vector3(1f + Mathf.Sin(k * Mathf.PI) * 0.25f, 1f + Mathf.Sin(k * Mathf.PI) * 1.2f, 1f);
                liquid.color = Color.Lerp(c, Color.white, Mathf.Sin(k * Mathf.PI) * 0.6f);
                heat = Mathf.Lerp(heat, 0.5f, k);
                SetMercury();
                yield return null;
            }
            liquid.transform.localScale = Vector3.one;
            liquid.color = c;
            heat = 0.5f;
            greenTime = 0f;
            SetMercury();
            steam.Stop();
            busy = false;
        }

        // ---------- Finale ----------

        IEnumerator Finale()
        {
            busy = true;
            Inputs.ReleaseMic(this);
            counter.text = "";
            dots[2].color = Done;
            goalText.text = "The potion is ready!";
            Sfx.Play(this, "ding", 0.7f);
            liquid.color = new Color(1f, 0.5f, 0.85f);
            steam.Play();
            yield return new WaitForSeconds(0.8f);
            // la scintilla parte e colpisce il gatto
            zap.gameObject.SetActive(true);
            Vector3 from = cauldronMouth.position, to = cat.position + Vector3.up * 0.3f;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.5f)
            {
                zap.position = Vector3.Lerp(from, to, k) + Vector3.up * Mathf.Sin(k * Mathf.PI) * 1.2f;
                zap.localRotation = Quaternion.Euler(0f, 0f, k * 540f);
                yield return null;
            }
            zap.gameObject.SetActive(false);
            Sfx.Play(this, "pop", 1f);
            Haptics.Light();
            cat.gameObject.SetActive(false);
            frog.gameObject.SetActive(true);
            Say("Ribbit?!", 1.2f);
            yield return new WaitForSeconds(1.1f);
            Sfx.Play(this, "pop", 1f);
            frog.gameObject.SetActive(false);
            cat.gameObject.SetActive(true);
            steam.Stop();
            Say("Oops.", 1.8f);
            yield return new WaitForSeconds(1.8f);
            Win();
        }
    }
}
