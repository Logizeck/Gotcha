using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;
using Gotcha.Sensors;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "Space station" — tre fasi. 1) Corrente saltata: tocca le 6 piastrelle dei cavi (griglia 3×3) per girarle di
    /// 90° finché l'energia scorre dal reattore (a sinistra) alla parabola (a destra). 2) Inclina il telefono a destra e a
    /// sinistra per girare la parabola finché il segnale è al massimo, e tienilo fermo 2 secondi. 3) Una lampada lampeggia
    /// un codice di lancio di 3 cifre (lampi corti e lunghi): decodificalo con la tabella sullo schermo e scrivilo sul
    /// tastierino. 3 tentativi per il codice. Vittoria: il razzo parte.
    /// </summary>
    public class SpaceStationLevel : LevelController
    {
        // griglia: cella = riga * 3 + colonna (riga 0 in alto). Direzioni: 0 E, 1 N, 2 O, 3 S.
        public static readonly int[] Cells = { 3, 4, 1, 2, 5, 8 };
        public static readonly bool[] Corner = { false, true, true, true, false, true };
        static readonly int[][] Need = { new[] { 2, 0 }, new[] { 2, 1 }, new[] { 3, 0 }, new[] { 2, 3 }, new[] { 1, 3 }, new[] { 1, 0 } };
        /// <summary>Tabella del codice: per ogni cifra 0-9, S = lampo corto, L = lampo lungo.</summary>
        public static readonly string[] Codes = { "LLL", "SL", "LS", "LL", "SSL", "SSS", "LSS", "LLS", "SLL", "LSL" };

        [SerializeField] StageHud40B hud;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] SpriteRenderer darkness;
        [SerializeField] Transform gridGroup, dishGroup, codeGroup;
        [SerializeField] Transform[] tiles;             // le 6 piastrelle girevoli (con Collider2D), nell'ordine di Cells
        [SerializeField] SpriteRenderer[] glows;        // filo d'energia di ogni piastrella
        [SerializeField] SpriteRenderer reactorGlow, dishLight;
        [SerializeField] Transform dish;
        [SerializeField] Transform satellite;
        [SerializeField] SpriteRenderer[] signalBars;
        [SerializeField] Transform holdBar;
        [SerializeField] TextMesh noiseText;
        [SerializeField] SpriteRenderer lampGlow;
        [SerializeField] SpriteRenderer[] tapeMarks;
        [SerializeField] Collider2D[] keys;             // 1..9, C, 0, OK
        [SerializeField] TextMesh display;
        [SerializeField] Transform rocket;
        [SerializeField] Transform flame;
        [SerializeField] float holdSeconds = 2f, signalWidth = 13f;

        int stage;
        int[] rot;
        bool[] powered;
        int[] flowOrder;
        bool busy;
        float targetAngle, angle, hold, clock;
        SteadyTilt tilt;
        int[] code;
        string typed = "";
        Camera cam;
        Coroutine hider;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            rot = new int[tiles.Length];
            powered = new bool[tiles.Length];
            flowOrder = new int[tiles.Length];
            for (int i = 0; i < tiles.Length; i++)
            {
                var wrong = new List<int>();
                for (int r = 0; r < 4; r++) if (!Fits(i, r)) wrong.Add(r);
                rot[i] = wrong[Random.Range(0, wrong.Count)];
                tiles[i].localRotation = Quaternion.Euler(0, 0, rot[i] * 90f);
            }
            dishGroup.gameObject.SetActive(false);
            codeGroup.gameObject.SetActive(false);
            rocket.gameObject.SetActive(false);
            satellite.gameObject.SetActive(false);
            foreach (SpriteRenderer m in tapeMarks) m.enabled = false;
            lampGlow.enabled = false;
            hud.Begin(3);
            hud.Goal("Power's out! Tap the cable tiles to turn them.");
            Flow();
            Say("Mission control: the reactor's cut off. Reconnect the dish!", 3f);
        }

        void Say(string text, float seconds)
        {
            if (bubble == null) return;
            bubble.Show(text);
            if (hider != null) StopCoroutine(hider);
            hider = StartCoroutine(HideBubble(seconds));
        }

        IEnumerator HideBubble(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (bubble != null) bubble.Hide();
        }

        static int[] Openings(bool corner, int r)
        {
            int a = corner ? 2 : 0, b = corner ? 1 : 2;
            return new[] { (a + r) % 4, (b + r) % 4 };
        }

        bool Fits(int i, int r)
        {
            int[] o = Openings(Corner[i], r);
            return (o[0] == Need[i][0] && o[1] == Need[i][1]) || (o[0] == Need[i][1] && o[1] == Need[i][0]);
        }

        void Update()
        {
            if (IsFinished) return;
            clock += Time.deltaTime;
            // l'energia "scorre" lungo i cavi accesi
            for (int i = 0; i < glows.Length; i++)
                glows[i].color = new Color(1f, 1f, 1f, powered[i] ? 0.6f + 0.4f * Mathf.Sin(clock * 7f - flowOrder[i] * 0.9f) : 0f);
            if (reactorGlow != null) reactorGlow.color = new Color(0.5f, 1f, 0.4f, 0.45f + 0.25f * Mathf.Sin(clock * 4f));
            if (busy) return;
            if (stage == 0) UpdateGrid();
            else if (stage == 1) UpdateDish();
            else if (stage == 2) UpdateKeypad();
        }

        bool Tapped(out Vector2 w)
        {
            w = default;
            if (!PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out _) || !began || PointerInput.IsOverUi()) return false;
            w = PointerInput.ToWorld(cam, sp);
            return true;
        }

        // ---------- fase 1: i cavi ----------

        void UpdateGrid()
        {
            if (!Tapped(out Vector2 w)) return;
            for (int i = 0; i < tiles.Length; i++)
            {
                if (!tiles[i].GetComponent<Collider2D>().OverlapPoint(w)) continue;
                rot[i] = (rot[i] + 3) % 4;   // un quarto di giro in senso orario
                Sfx.Play(this, "tick", 0.8f);
                StartCoroutine(Turn(i));
                return;
            }
        }

        IEnumerator Turn(int i)
        {
            busy = true;
            Quaternion from = tiles[i].localRotation, to = Quaternion.Euler(0, 0, rot[i] * 90f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.12f) { tiles[i].localRotation = Quaternion.Slerp(from, to, k); yield return null; }
            tiles[i].localRotation = to;
            busy = false;
            if (Flow()) StartCoroutine(PowerOn());
        }

        /// <summary>Segue l'energia dal reattore; true se arriva alla parabola.</summary>
        bool Flow()
        {
            for (int i = 0; i < powered.Length; i++) powered[i] = false;
            int r = 1, c = 0, from = 2, step = 0;
            while (true)
            {
                if (r < 0 || r > 2 || c < 0 || c > 2) return false;
                int t = System.Array.IndexOf(Cells, r * 3 + c);
                if (t < 0) return false;
                int[] o = Openings(Corner[t], rot[t]);
                int other;
                if (o[0] == from) other = o[1];
                else if (o[1] == from) other = o[0];
                else return false;
                powered[t] = true;
                flowOrder[t] = step++;
                if (r == 2 && c == 2 && other == 0) return true;
                if (other == 0) c++; else if (other == 1) r--; else if (other == 2) c--; else r++;
                from = (other + 2) % 4;
            }
        }

        IEnumerator PowerOn()
        {
            busy = true;
            Sfx.Play(this, "ding", 0.8f);
            Haptics.Medium();
            dishLight.color = new Color(0.6f, 1f, 1f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.8f)
            {
                Color d = darkness.color;
                darkness.color = new Color(d.r, d.g, d.b, Mathf.Lerp(0.55f, 0f, k));
                yield return null;
            }
            darkness.enabled = false;
            Say("Power's back! Lights on!", 1.8f);
            yield return new WaitForSeconds(0.8f);
            yield return Swap(gridGroup, dishGroup);
            targetAngle = (Random.value < 0.5f ? -1f : 1f) * Random.Range(25f, 45f);
            tilt = new SteadyTilt();
            yield return hud.Next(1, "STAGE 2: FIND THE SIGNAL", "Tilt the phone to turn the dish. Hold the best signal 2 s!");
            stage = 1;
            busy = false;
        }

        IEnumerator Swap(Transform outGroup, Transform inGroup)
        {
            Vector3 a = outGroup.localPosition;
            if (inGroup != null) inGroup.gameObject.SetActive(true);
            Vector3 b = inGroup != null ? inGroup.localPosition : Vector3.zero;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.5f)
            {
                float e = k * k * (3f - 2f * k);
                outGroup.localPosition = a + Vector3.left * 7f * e;
                if (inGroup != null) inGroup.localPosition = b + Vector3.right * 7f * (1f - e);
                yield return null;
            }
            outGroup.gameObject.SetActive(false);
            outGroup.localPosition = a;
            if (inGroup != null) inGroup.localPosition = b;
        }

        // ---------- fase 2: la parabola ----------

        void UpdateDish()
        {
            float dt = Time.deltaTime;
            float d = tilt.Update(dt);
            angle = Mathf.Lerp(angle, Mathf.Clamp(-d * 110f, -70f, 70f), 1f - Mathf.Exp(-dt * 8f));
            dish.localRotation = Quaternion.Euler(0, 0, angle);
            float diff = angle - targetAngle;
            float signal = Mathf.Exp(-(diff * diff) / (signalWidth * signalWidth));
            int lit = Mathf.Clamp(Mathf.FloorToInt(signal * 5.2f), 0, 5);
            for (int i = 0; i < signalBars.Length; i++)
                signalBars[i].color = i < lit ? Color.Lerp(new Color(1f, 0.6f, 0.2f), new Color(0.5f, 1f, 0.4f), lit / 5f) : new Color(1f, 1f, 1f, 0.2f);
            if (noiseText != null)
            {
                noiseText.text = signal > 0.85f ? "BEEP... BEEP... LOCKING ON" : signal > 0.4f ? "kssh... b..p... kssh" : "KSSSSHHHHH";
                noiseText.transform.localPosition = new Vector3(Random.Range(-1f, 1f) * (1f - signal) * 0.03f, noiseText.transform.localPosition.y, 0f);
            }
            if (signal > 0.85f) hold += dt;
            else hold = Mathf.Max(0f, hold - dt * 2f);
            holdBar.localScale = new Vector3(Mathf.Clamp01(hold / holdSeconds), 1f, 1f);
            if (hold >= holdSeconds) StartCoroutine(Locked());
        }

        IEnumerator Locked()
        {
            busy = true;
            Sfx.Play(this, "ding", 0.9f);
            Haptics.Medium();
            if (noiseText != null) noiseText.text = "SIGNAL LOCKED!";
            satellite.gameObject.SetActive(true);
            Vector3 dir = Quaternion.Euler(0, 0, targetAngle) * Vector3.up;
            Vector3 to = dish.position + dir * 2.9f;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.8f) { satellite.position = to + Vector3.right * (1f - k) * 3f; yield return null; }
            satellite.position = to;
            Say("Incoming message from mission control!", 1.8f);
            yield return new WaitForSeconds(1.0f);
            yield return Swap(dishGroup, codeGroup);
            code = new int[3];
            var pool = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            for (int i = 0; i < 3; i++) { int k = Random.Range(0, pool.Count); code[i] = pool[k]; pool.RemoveAt(k); }
            yield return hud.Next(2, "STAGE 3: LAUNCH CODE", "Decode the blinking lamp with the table. Type the code!");
            Display();
            stage = 2;   // prima di Blink(): il lampeggio gira solo finché si è nella fase 2
            busy = false;
            StartCoroutine(Blink());
        }

        // ---------- fase 3: il codice ----------

        IEnumerator Blink()
        {
            const float shortW = 0.14f, longW = 0.42f, gap = 0.1f, digitGap = 0.4f;
            while (!IsFinished && stage == 2)
            {
                // larghezza totale del nastro, per centrarlo
                float total = 0f;
                for (int d = 0; d < 3; d++)
                {
                    string s = Codes[code[d]];
                    foreach (char ch in s) total += (ch == 'L' ? longW : shortW) + gap;
                    total += d < 2 ? digitGap - gap : -gap;
                }
                foreach (SpriteRenderer m in tapeMarks) m.enabled = false;
                yield return new WaitForSeconds(0.8f);
                float x = -total * 0.5f;
                int mark = 0;
                for (int d = 0; d < 3; d++)
                {
                    foreach (char ch in Codes[code[d]])
                    {
                        if (stage != 2) yield break;
                        bool isLong = ch == 'L';
                        float w = isLong ? longW : shortW;
                        lampGlow.enabled = true;
                        Sfx.Play(this, isLong ? "lamp_long" : "lamp", 0.25f);
                        Haptics.Pulse(isLong ? 380 : 90, isLong ? 200 : 160);   // il lampo si sente anche in mano
                        if (mark < tapeMarks.Length)
                        {
                            SpriteRenderer m = tapeMarks[mark++];
                            m.enabled = true;
                            m.transform.localPosition = new Vector3(x + w * 0.5f, m.transform.localPosition.y, 0f);
                            m.transform.localScale = new Vector3(w, 0.14f, 1f);
                        }
                        x += w + gap;
                        yield return new WaitForSeconds(isLong ? 0.9f : 0.3f);
                        lampGlow.enabled = false;
                        yield return new WaitForSeconds(0.35f);
                    }
                    x += digitGap - gap;
                    yield return new WaitForSeconds(0.9f);
                }
                yield return new WaitForSeconds(2.2f);
            }
        }

        void Display() { display.text = typed.PadRight(3, '_'); }

        void UpdateKeypad()
        {
            if (!Tapped(out Vector2 w)) return;
            for (int i = 0; i < keys.Length; i++)
            {
                if (!keys[i].OverlapPoint(w)) continue;
                StartCoroutine(Press(keys[i].transform));
                if (i < 9 || i == 10)
                {
                    if (typed.Length < 3) typed += i == 10 ? "0" : (i + 1).ToString();
                    Sfx.Play(this, "tick", 0.8f);
                }
                else if (i == 9) { typed = ""; Sfx.Play(this, "pop", 0.5f); }
                else Check();
                Display();
                return;
            }
        }

        IEnumerator Press(Transform k)
        {
            Vector3 s = k.localScale;
            k.localScale = s * 0.88f;
            yield return new WaitForSeconds(0.08f);
            k.localScale = s;
        }

        void Check()
        {
            if (typed.Length < 3) { Say("The code has 3 digits!", 1.5f); Sfx.Play(this, "buzz", 0.4f); return; }
            string want = "" + code[0] + code[1] + code[2];
            if (typed == want) { StartCoroutine(Launch()); return; }
            typed = "";
            Sfx.Play(this, "buzz", 0.7f);
            bool over = hud.LoseTry();
            if (over) { StartCoroutine(Fizzle()); return; }
            Say(hud.TriesLeft == 2 ? "WRONG CODE. The rocket sneezed." : "WRONG CODE. Last try, cadet!", 2f);
            StartCoroutine(FlashDisplay());
        }

        IEnumerator FlashDisplay()
        {
            busy = true;
            for (int i = 0; i < 3; i++) { display.text = "ERR"; yield return new WaitForSeconds(0.15f); display.text = ""; yield return new WaitForSeconds(0.1f); }
            Display();
            busy = false;
        }

        IEnumerator Launch()
        {
            busy = true;
            stage = 3;
            display.text = "GO!";
            lampGlow.enabled = false;
            Sfx.Play(this, "ding", 1f);
            hud.SetStage(3);
            hud.Shout("3... 2... 1... LIFTOFF!");
            yield return new WaitForSeconds(0.6f);
            yield return Swap(codeGroup, null);
            rocket.gameObject.SetActive(true);
            Haptics.Heavy();
            Sfx.Play(this, "whoosh", 1f);
            Sfx.Play(this, "clunk", 1f);
            Vector3 from = rocket.localPosition, to = from + Vector3.up * 13f;
            Vector3 groundPos = transform.localPosition;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 2.6f)
            {
                rocket.localPosition = Vector3.Lerp(from, to, k * k);
                flame.localScale = new Vector3(1f, 1f + Random.Range(0f, 0.6f), 1f);
                transform.localPosition = groundPos + (Vector3)Random.insideUnitCircle * 0.06f * (1f - k);
                if (Mathf.Repeat(k, 0.12f) < Time.deltaTime / 2.6f) Haptics.Light();
                yield return null;
            }
            transform.localPosition = groundPos;
            Say("Houston, we have cheese... er, liftoff!", 2f);
            yield return new WaitForSeconds(1.2f);
            Win();
        }

        IEnumerator Fizzle()
        {
            busy = true;
            stage = 3;
            display.text = "LOCK";
            hud.Shout("LAUNCH ABORTED");
            Say("Three wrong codes. Launch cancelled. Back to training!", 2.5f);
            Haptics.Medium();
            yield return new WaitForSeconds(2.4f);
            Fail();
        }
    }
}
