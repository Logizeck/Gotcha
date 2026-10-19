using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "Train switches" — smistamento in tempo reale: dalla galleria escono 8 treni (rossi, blu, gialli), uno
    /// alla volta e sempre più spesso; tocca i 3 scambi (la freccia mostra dove vanno) per mandare ogni treno alla
    /// stazione del suo colore. Lo scambio in alto sceglie sinistra/destra, i due in basso scelgono fra la stazione
    /// esterna e quella blu al centro (ci si arriva da entrambi i lati). Una luce accanto alla galleria mostra il colore
    /// del prossimo treno. Ammessi 2 errori (al terzo si perde); 8 treni consegnati = vittoria. I treni vanno tutti alla
    /// stessa velocità e partono distanziati, quindi non si scontrano mai.
    /// </summary>
    public class TrainSwitchesLevel : LevelController
    {
        /// <summary>Tratti di binario (coordinate del livello): 0 tronco, 1-2 dallo scambio A, 3-4 da B, 5-6 da C, 7 ultimo tratto blu.</summary>
        public static readonly Vector2[][] Segs =
        {
            new[] { new Vector2(0f, 2.75f), new Vector2(0f, 1.5f) },
            new[] { new Vector2(0f, 1.5f), new Vector2(-1.3f, -0.2f) },
            new[] { new Vector2(0f, 1.5f), new Vector2(1.3f, -0.2f) },
            new[] { new Vector2(-1.3f, -0.2f), new Vector2(-2.0f, -1.3f), new Vector2(-2.0f, -3.0f) },
            new[] { new Vector2(-1.3f, -0.2f), new Vector2(0f, -1.9f) },
            new[] { new Vector2(1.3f, -0.2f), new Vector2(0f, -1.9f) },
            new[] { new Vector2(1.3f, -0.2f), new Vector2(2.0f, -1.3f), new Vector2(2.0f, -3.0f) },
            new[] { new Vector2(0f, -1.9f), new Vector2(0f, -3.0f) },
        };
        public static readonly Vector2[] SwitchAt = { new Vector2(0f, 1.5f), new Vector2(-1.3f, -0.2f), new Vector2(1.3f, -0.2f) };
        public static readonly Color[] LineColors = { new Color(0.92f, 0.3f, 0.26f), new Color(0.27f, 0.55f, 0.97f), new Color(1f, 0.83f, 0.2f) };

        [SerializeField] TextMesh goalText;
        [SerializeField] TextMesh scoreText;
        [SerializeField] SpriteRenderer[] trainDots;     // 8 pallini, uno per treno
        [SerializeField] LineRenderer[] beds;            // massicciata per ogni tratto (8)
        [SerializeField] LineRenderer[] rails;           // rotaie per ogni tratto (8)
        [SerializeField] Collider2D[] switches;          // 3 scambi
        [SerializeField] Transform[] arrows;             // 3 frecce
        [SerializeField] Transform[] stations;           // rossa, blu, gialla
        [SerializeField] Transform[] trains;             // pool di locomotive
        [SerializeField] SpriteRenderer signal;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] float speed = 1.3f;
        [SerializeField] float[] gaps = { 1.5f, 5.0f, 4.5f, 4.0f, 3.5f, 3.0f, 2.6f, 2.3f };

        class Train { public Transform t; public SpriteRenderer art; public int color, seg; public float dist; public bool on; }

        readonly List<Train> pool = new List<Train>();
        readonly int[] state = new int[3];   // 0 = sinistra, 1 = destra
        int[] colors;
        int launched, processed, delivered, mistakes;
        float nextLaunch, bubbleUntil, clock;
        bool over;
        Camera cam;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            foreach (Transform t in trains)
            {
                t.gameObject.SetActive(false);
                pool.Add(new Train { t = t, art = t.GetComponentInChildren<SpriteRenderer>() });
            }
            colors = new int[gaps.Length];
            for (int i = 0; i < colors.Length; i++)
            {
                int c;
                do c = Random.Range(0, 3); while (i >= 2 && c == colors[i - 1] && c == colors[i - 2]);
                colors[i] = c;
            }
            for (int s = 0; s < 3; s++) state[s] = Random.Range(0, 2);
            for (int i = 0; i < stations.Length; i++) stations[i].GetComponentInChildren<SpriteRenderer>().color = LineColors[i];
            foreach (SpriteRenderer d in trainDots) d.color = new Color(1f, 1f, 1f, 0.45f);
            nextLaunch = gaps[0];
            goalText.text = "Send each train to its colour!";
            RefreshSwitches();
            UpdateScore();
            signal.color = LineColors[colors[0]];
        }

        void Say(string text, float secs)
        {
            if (bubble == null) return;
            bubble.Show(text);
            bubbleUntil = clock + secs;
        }

        void UpdateScore() { scoreText.text = "MISTAKES: " + mistakes + " / 2"; }

        // ---------- Scambi ----------

        void RefreshSwitches()
        {
            for (int s = 0; s < 3; s++)
            {
                int a = 1 + s * 2, b = a + 1;   // i due tratti in uscita dallo scambio
                int on = state[s] == 0 ? a : b, off = state[s] == 0 ? b : a;
                SetActiveLook(on, true);
                SetActiveLook(off, false);
                Vector2 d = Segs[on][1] - Segs[on][0];
                arrows[s].localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg);
            }
            SetActiveLook(0, true);
            SetActiveLook(7, true);
        }

        void SetActiveLook(int seg, bool on)
        {
            float a = on ? 1f : 0.3f;
            Color bed = new Color(0.55f, 0.38f, 0.25f, a), rail = on ? new Color(0.93f, 0.93f, 0.96f, 1f) : new Color(0.75f, 0.75f, 0.78f, 0.35f);
            beds[seg].startColor = beds[seg].endColor = bed;
            rails[seg].startColor = rails[seg].endColor = rail;
        }

        // ---------- Ciclo ----------

        void Update()
        {
            if (IsFinished || over) return;
            clock += Time.deltaTime;
            if (bubble != null && bubbleUntil > 0f && clock > bubbleUntil) { bubble.Hide(); bubbleUntil = 0f; }

            if (PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out _) && began && !PointerInput.IsOverUi())
            {
                Vector2 w = PointerInput.ToWorld(cam, sp);
                for (int s = 0; s < switches.Length; s++)
                {
                    if (!switches[s].OverlapPoint(w)) continue;
                    state[s] = 1 - state[s];
                    RefreshSwitches();
                    Sfx.Play(this, "clunk", 0.6f);
                    Haptics.Light();
                    break;
                }
            }

            if (launched < colors.Length)
            {
                nextLaunch -= Time.deltaTime;
                if (nextLaunch <= 0f) Launch();
            }

            foreach (Train tr in pool) if (tr.on) Move(tr);
        }

        void Launch()
        {
            Train tr = null;
            foreach (Train p in pool) if (!p.on) { tr = p; break; }
            if (tr == null) { nextLaunch = 0.2f; return; }
            tr.on = true;
            tr.color = colors[launched];
            tr.seg = 0;
            tr.dist = 0f;
            tr.art.color = LineColors[tr.color];
            tr.t.gameObject.SetActive(true);
            tr.t.localScale = Vector3.one;
            Place(tr);
            Sfx.Play(this, "whoosh", 0.4f);
            trainDots[launched].color = Color.white;
            launched++;
            if (launched < colors.Length)
            {
                nextLaunch = gaps[launched];
                signal.color = LineColors[colors[launched]];
            }
            else signal.color = new Color(0.4f, 0.4f, 0.4f);
        }

        static float Length(Vector2[] seg)
        {
            float l = 0f;
            for (int i = 1; i < seg.Length; i++) l += Vector2.Distance(seg[i - 1], seg[i]);
            return l;
        }

        void Place(Train tr)
        {
            Vector2[] seg = Segs[tr.seg];
            float d = tr.dist;
            for (int i = 1; i < seg.Length; i++)
            {
                float l = Vector2.Distance(seg[i - 1], seg[i]);
                if (d <= l || i == seg.Length - 1)
                {
                    Vector2 dir = (seg[i] - seg[i - 1]).normalized;
                    Vector2 p = seg[i - 1] + dir * Mathf.Min(d, l);
                    tr.t.localPosition = new Vector3(p.x, p.y, 0f);
                    tr.t.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
                    return;
                }
                d -= l;
            }
        }

        void Move(Train tr)
        {
            tr.dist += speed * Time.deltaTime;
            float len = Length(Segs[tr.seg]);
            if (tr.dist >= len)
            {
                float extra = tr.dist - len;
                int next;
                switch (tr.seg)
                {
                    case 0: next = state[0] == 0 ? 1 : 2; break;
                    case 1: next = state[1] == 0 ? 3 : 4; break;
                    case 2: next = state[2] == 0 ? 5 : 6; break;
                    case 4: case 5: next = 7; break;
                    default: next = -1; break;
                }
                if (next < 0)
                {
                    int station = tr.seg == 3 ? 0 : tr.seg == 7 ? 1 : 2;
                    tr.dist = len;
                    Place(tr);
                    Arrive(tr, station);
                    return;
                }
                tr.seg = next;
                tr.dist = extra;
            }
            Place(tr);
        }

        // ---------- Arrivi ----------

        void Arrive(Train tr, int station)
        {
            tr.on = false;
            int idx = processed++;
            bool ok = station == tr.color;
            StartCoroutine(Enter(tr.t));
            if (ok)
            {
                delivered++;
                trainDots[idx].color = new Color(0.42f, 0.88f, 0.48f);
                Sfx.Play(this, "ding", 0.5f);
                Haptics.Light();
                StartCoroutine(Bounce(stations[station]));
            }
            else
            {
                mistakes++;
                UpdateScore();
                trainDots[idx].color = new Color(0.9f, 0.27f, 0.24f);
                Sfx.Play(this, "buzz", 0.7f);
                Haptics.Medium();
                StartCoroutine(Shake(stations[station]));
                if (mistakes > 2) { StartCoroutine(Lose()); return; }
                Say(mistakes == 1 ? "Wrong station! One more slip allowed." : "Wrong station! No more mistakes!", 1.6f);
            }
            if (processed >= colors.Length) StartCoroutine(Victory());
        }

        IEnumerator Enter(Transform t)
        {
            Vector3 s = t.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.35f)
            {
                t.localScale = s * (1f - k);
                yield return null;
            }
            t.gameObject.SetActive(false);
            t.localScale = s;
        }

        IEnumerator Bounce(Transform t)
        {
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.35f)
            {
                t.localScale = Vector3.one * (1f + Mathf.Sin(k * Mathf.PI) * 0.18f);
                yield return null;
            }
            t.localScale = Vector3.one;
        }

        IEnumerator Shake(Transform t)
        {
            Vector3 home = t.localPosition;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.4f)
            {
                t.localPosition = home + Vector3.right * Mathf.Sin(k * 40f) * 0.08f * (1f - k);
                yield return null;
            }
            t.localPosition = home;
        }

        IEnumerator Lose()
        {
            over = true;
            Say("Three mistakes... the trains are on strike!", 2f);
            Haptics.Heavy();
            yield return new WaitForSeconds(2f);
            Fail();
        }

        IEnumerator Victory()
        {
            over = true;
            goalText.text = "All trains delivered!";
            yield return new WaitForSeconds(0.5f);
            Say("Toot toot! Right on time!", 2f);
            for (int r = 0; r < 3; r++)
            {
                foreach (Transform s in stations) StartCoroutine(Bounce(s));
                Sfx.Play(this, "pop", 0.7f);
                yield return new WaitForSeconds(0.4f);
            }
            yield return new WaitForSeconds(1.2f);
            Win();
        }
    }
}
