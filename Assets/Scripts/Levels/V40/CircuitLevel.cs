using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "Circuit board" — sulla porta della cassaforte c'è una scheda 5x5: una pila da 9V, due lampadine e un LED
    /// rosso ("2V MAX"), piste fisse e 9 zoccoli tratteggiati vuoti. Dal vassoio si trascinano fili dritti, curve,
    /// l'interruttore e la resistenza (un tocco su un pezzo lo gira, trascinarlo fuori lo rimette nel vassoio).
    /// Fase 1: collegare tutto. Fase 2: toccare l'interruttore per accendere: le tre luci accendono il display e la
    /// cassaforte si apre (fase 3). Trucco: la resistenza deve stare sul ramo del LED (in (3,2) o (4,3)), altrimenti il
    /// LED brucia (pop, vibrazione) e va sostituito con un ricambio dal vassoio: 3 LED in tutto, finiti = sconfitta.
    /// </summary>
    public class CircuitLevel : LevelController
    {
        public const float Cell = 0.95f;
        public static readonly Vector2 Origin = new Vector2(-1.9f, -2.25f);   // centro della cella (0,0)
        public static readonly Vector2Int[] Sockets =
        {
            new Vector2Int(0, 1), new Vector2Int(0, 2), new Vector2Int(1, 2), new Vector2Int(2, 3), new Vector2Int(3, 2),
            new Vector2Int(4, 2), new Vector2Int(4, 3), new Vector2Int(2, 1), new Vector2Int(2, 0),
        };
        public static readonly Vector2Int BatteryCell = new Vector2Int(0, 0), CrossCell = new Vector2Int(2, 2), WireCell = new Vector2Int(3, 0);
        public static readonly Vector2Int BulbACell = new Vector2Int(2, 4), BulbBCell = new Vector2Int(4, 0), LedCell = new Vector2Int(4, 4);
        public static readonly float[] TrayX = { -2.1f, -1.05f, 0f, 1.05f, 2.1f };
        public const float TrayY = -3.85f;

        const int N = 1, E = 2, S = 4, W = 8;
        const int KStraight = 0, KCorner = 1, KSwitch = 2, KResistor = 3, KLed = 4;

        [SerializeField] Transform[] straights;
        [SerializeField] Transform[] corners;
        [SerializeField] Transform switchPiece, resistorPiece;
        [SerializeField] Transform[] leds;                 // il primo parte montato
        [SerializeField] Sprite switchOff, switchOn, ledOff, ledOn, ledBurnt, bulbOff, bulbOn;
        [SerializeField] SpriteRenderer bulbA, bulbB, crossArt, wireArt;
        [SerializeField] TextMesh[] digits;
        [SerializeField] TextMesh[] counts;                // numero di pezzi in ogni scomparto del vassoio
        [SerializeField] Transform door;                   // perno sul cardine sinistro
        [SerializeField] SpriteRenderer[] dots;
        [SerializeField] SpriteRenderer[] hearts;
        [SerializeField] TextMesh goal;
        [SerializeField] SpeechBubble bubble;

        class Piece { public Transform t; public SpriteRenderer art; public int kind; public Vector2Int cell = NoCell; public int rot; public bool gone; }
        static readonly Vector2Int NoCell = new Vector2Int(-1, -1);

        readonly List<Piece> pieces = new List<Piece>();
        Camera cam;
        bool busy, switchClosed, opened;
        int ledsLeft, stage = -1;
        Piece grabbed;
        Vector2 grabStart;
        bool dragging;
        float bubbleUntil, clock;
        bool[,] powered = new bool[5, 5];
        bool ledNoRes, ledLit, aLit, bLit;
        readonly string code = "742";

        static readonly Color Copper = new Color(0.95f, 0.66f, 0.42f), Live = new Color(1f, 0.95f, 0.35f);

        public override void OnLevelStart()
        {
            cam = Camera.main;
            foreach (Transform t in straights) Add(t, KStraight);
            foreach (Transform t in corners) Add(t, KCorner);
            Add(switchPiece, KSwitch);
            Add(resistorPiece, KResistor);
            foreach (Transform t in leds) Add(t, KLed);
            ledsLeft = leds.Length;
            foreach (Piece p in pieces) if (p.kind == KLed) { p.cell = LedCell; Snap(p); break; }
            foreach (TextMesh d in digits) if (d != null) d.color = new Color(1f, 0.3f, 0.3f, 0.15f);
            E40Kit.Hearts(hearts, ledsLeft);
            LayoutTray();
            Evaluate();
            Say("Wire it up!", 2f);
        }

        void Add(Transform t, int kind)
        {
            if (t == null) return;
            pieces.Add(new Piece { t = t, art = t.GetComponentInChildren<SpriteRenderer>(), kind = kind });
        }

        void Say(string text, float seconds)
        {
            if (bubble == null) return;
            bubble.Show(text);
            bubbleUntil = clock + seconds;
        }

        static Vector3 CellPos(Vector2Int c) => new Vector3(Origin.x + c.x * Cell, Origin.y + c.y * Cell, 0f);

        static Vector2Int CellAt(Vector2 w)
        {
            int c = Mathf.RoundToInt((w.x - Origin.x) / Cell), r = Mathf.RoundToInt((w.y - Origin.y) / Cell);
            return c >= 0 && c < 5 && r >= 0 && r < 5 ? new Vector2Int(c, r) : NoCell;
        }

        static bool IsSocket(Vector2Int c) { foreach (Vector2Int s in Sockets) if (s == c) return true; return false; }

        static int Rot(int m, int r) { for (int i = 0; i < r; i++) m = ((m << 1) | (m >> 3)) & 15; return m; }
        static int Opp(int d) => Rot(d, 2);

        static int BaseMask(int kind) => kind == KCorner ? (E | N) : kind == KLed ? S : (E | W);

        Piece PieceAt(Vector2Int c) { foreach (Piece p in pieces) if (!p.gone && p.cell == c) return p; return null; }

        /// <summary>Collegamenti della cella; "closed" forza l'interruttore chiuso (per capire se il circuito è completo).</summary>
        int MaskAt(Vector2Int c, bool forceClosed)
        {
            if (c == BatteryCell) return N;
            if (c == CrossCell) return N | E | S | W;
            if (c == WireCell) return E | W;
            if (c == BulbACell) return S;
            if (c == BulbBCell) return W;
            Piece p = PieceAt(c);
            if (p == null) return 0;
            if (p.kind == KLed && p.art.sprite == ledBurnt) return 0;
            if (p.kind == KSwitch && !switchClosed && !forceClosed) return 0;
            return Rot(BaseMask(p.kind), p.rot);
        }

        /// <summary>Corrente dalla pila (visita in ampiezza, ricordando se si è già passati da una resistenza).</summary>
        void Simulate(bool forceClosed, bool[,] reach, out bool noRes, out bool withRes)
        {
            var seen = new bool[5, 5, 2];
            var q = new Queue<(Vector2Int, int)>();
            q.Enqueue((BatteryCell, 0));
            seen[0, 0, 0] = true;
            Vector2Int[] step = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
            while (q.Count > 0)
            {
                (Vector2Int c, int res) = q.Dequeue();
                int m = MaskAt(c, forceClosed);
                for (int k = 0; k < 4; k++)
                {
                    int d = 1 << k;
                    if ((m & d) == 0) continue;
                    Vector2Int n = c + step[k];
                    if (n.x < 0 || n.x > 4 || n.y < 0 || n.y > 4) continue;
                    if ((MaskAt(n, forceClosed) & Opp(d)) == 0) continue;
                    Piece np = PieceAt(n);
                    int nr = res == 1 || (np != null && np.kind == KResistor) ? 1 : 0;
                    if (seen[n.x, n.y, nr]) continue;
                    seen[n.x, n.y, nr] = true;
                    q.Enqueue((n, nr));
                }
            }
            for (int x = 0; x < 5; x++) for (int y = 0; y < 5; y++) reach[x, y] = seen[x, y, 0] || seen[x, y, 1];
            noRes = seen[LedCell.x, LedCell.y, 0];
            withRes = seen[LedCell.x, LedCell.y, 1];
        }

        void Evaluate()
        {
            if (busy || opened) return;
            Simulate(false, powered, out bool noRes, out bool withRes);
            Piece led = PieceAt(LedCell);
            bool ledOk = led != null && led.art.sprite != ledBurnt;
            ledNoRes = ledOk && noRes;
            ledLit = ledOk && withRes && !noRes;
            aLit = powered[BulbACell.x, BulbACell.y];
            bLit = powered[BulbBCell.x, BulbBCell.y];
            // grafica: piste accese, lampadine, LED
            foreach (Piece p in pieces)
            {
                if (p.gone) continue;
                if (p.kind == KLed) { if (p.art.sprite != ledBurnt) p.art.sprite = p.cell == LedCell && ledLit ? ledOn : ledOff; continue; }
                if (p.kind == KSwitch) p.art.sprite = switchClosed ? switchOn : switchOff;
                bool live = p.cell != NoCell && powered[p.cell.x, p.cell.y];
                p.art.color = p.kind == KStraight || p.kind == KCorner ? (live ? Live : Copper) : (live ? new Color(1f, 1f, 0.8f) : Color.white);
            }
            if (crossArt != null) crossArt.color = powered[CrossCell.x, CrossCell.y] ? Live : Copper;
            if (wireArt != null) wireArt.color = powered[WireCell.x, WireCell.y] ? Live : Copper;
            if (bulbA != null) bulbA.sprite = aLit ? bulbOn : bulbOff;
            if (bulbB != null) bulbB.sprite = bLit ? bulbOn : bulbOff;
            if (digits.Length >= 3)
            {
                digits[0].color = aLit ? new Color(1f, 0.35f, 0.3f) : new Color(1f, 0.3f, 0.3f, 0.15f);
                digits[1].color = bLit ? new Color(1f, 0.35f, 0.3f) : new Color(1f, 0.3f, 0.3f, 0.15f);
                digits[2].color = ledLit ? new Color(1f, 0.35f, 0.3f) : new Color(1f, 0.3f, 0.3f, 0.15f);
            }

            if (ledNoRes) { StartCoroutine(Burn(led)); return; }
            if (aLit && bLit && ledLit && switchClosed && PieceOf(KSwitch).cell != NoCell) { StartCoroutine(Open()); return; }

            var full = new bool[5, 5];
            Simulate(true, full, out bool fNo, out bool fRes);
            bool complete = full[BulbACell.x, BulbACell.y] && full[BulbBCell.x, BulbBCell.y] && (fNo || fRes) && PieceOf(KSwitch).cell != NoCell;
            SetStage(complete ? 1 : 0);
        }

        Piece PieceOf(int kind) { foreach (Piece p in pieces) if (p.kind == kind) return p; return null; }

        void SetStage(int s)
        {
            if (s == stage) return;
            stage = s;
            E40Kit.Dots(dots, s);
            E40Kit.Goal(goal, s == 0 ? "Connect the battery to all 3 lights" : "All wired! Tap the switch to power up");
            if (s == 1) Sfx.Play(this, "ding", 0.4f);
        }

        void LayoutTray()
        {
            var n = new int[5];
            foreach (Piece p in pieces)
            {
                if (p.gone || p.cell != NoCell || p == grabbed) continue;
                p.t.localPosition = new Vector3(TrayX[p.kind] + n[p.kind] * 0.04f, TrayY + n[p.kind] * 0.04f, 0f);
                p.t.localRotation = Quaternion.identity;
                p.t.localScale = Vector3.one * 0.85f;
                p.rot = 0;
                p.art.sortingOrder = 22 + n[p.kind];
                n[p.kind]++;
            }
            for (int k = 0; k < counts.Length && k < 5; k++) if (counts[k] != null) counts[k].text = n[k] > 0 ? "x" + n[k] : "";
        }

        void Snap(Piece p)
        {
            p.t.localPosition = CellPos(p.cell);
            p.t.localRotation = Quaternion.Euler(0, 0, -90f * p.rot);
            p.t.localScale = Vector3.one;
            p.art.sortingOrder = 12;
        }

        void Update()
        {
            if (IsFinished) return;
            clock += Time.deltaTime;
            if (bubble != null && bubbleUntil > 0f && clock > bubbleUntil) { bubble.Hide(); bubbleUntil = 0f; }
            if (busy || opened) { grabbed = null; return; }
            if (!PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out bool ended)) { if (grabbed != null) Drop(grabStart); return; }
            Vector2 w = PointerInput.ToWorld(cam, sp);
            if (began)
            {
                if (PointerInput.IsOverUi()) return;
                grabbed = Pick(w);
                grabStart = w;
                dragging = false;
                return;
            }
            if (grabbed == null) return;
            if (!dragging && (w - grabStart).magnitude > 0.15f)
            {
                if (grabbed.kind == KLed && grabbed.art.sprite == ledBurnt) { grabbed = null; return; }
                dragging = true;
                grabbed.art.sortingOrder = 40;
                grabbed.t.localScale = Vector3.one;
            }
            if (dragging) grabbed.t.position = new Vector3(w.x, w.y + 0.25f, 0f);
            if (ended) Drop(w);
        }

        Piece Pick(Vector2 w)
        {
            Vector2Int c = CellAt(w);
            if (c != NoCell) { Piece p = PieceAt(c); if (p != null) return p; }
            for (int k = 0; k < 5; k++)
            {
                if (Mathf.Abs(w.x - TrayX[k]) > 0.5f || Mathf.Abs(w.y - TrayY) > 0.55f) continue;
                Piece top = null;
                foreach (Piece p in pieces) if (!p.gone && p.kind == k && p.cell == NoCell) top = p;
                if (top == null) Say(k == KLed ? "No spare LEDs left!" : "None left here.", 1.2f);
                return top;
            }
            return null;
        }

        void Drop(Vector2 w)
        {
            Piece p = grabbed;
            grabbed = null;
            if (p == null) return;
            if (!dragging)
            {
                if (p.cell == NoCell) { Say("Drag it onto a dashed socket.", 1.6f); return; }
                if (p.kind == KSwitch)
                {
                    switchClosed = !switchClosed;
                    Sfx.Play(this, "clunk", 0.7f);
                    Haptics.Light();
                }
                else if (p.kind != KLed)
                {
                    p.rot = (p.rot + 1) % 4;
                    Sfx.Play(this, "tick", 0.6f);
                    Snap(p);
                }
                Evaluate();
                return;
            }
            dragging = false;
            Vector2Int c = CellAt(w + Vector2.up * 0.25f);
            bool ok = c != NoCell && ((p.kind == KLed && c == LedCell) || (p.kind != KLed && IsSocket(c)));
            if (ok)
            {
                Piece other = PieceAt(c);
                if (other != null && other != p)
                {
                    if (other.kind == KLed && other.art.sprite == ledBurnt) ok = false;
                    else other.cell = NoCell;
                }
            }
            if (ok)
            {
                p.cell = c;
                p.rot = p.kind == KLed ? 0 : BestRot(p, c);
                Snap(p);
                Sfx.Play(this, "clunk", 0.6f);
                Haptics.Light();
            }
            else
            {
                if (p.cell != NoCell && p.kind == KSwitch) switchClosed = false;
                p.cell = NoCell;
                Sfx.Play(this, "pop", 0.4f);
            }
            if (p.kind == KSwitch && p.cell == NoCell) switchClosed = false;
            LayoutTray();
            Evaluate();
        }

        /// <summary>Verso automatico al momento di posare: quello che si collega a più vicini.</summary>
        int BestRot(Piece p, Vector2Int c)
        {
            int baseMask = BaseMask(p.kind), best = 0;
            float bestScore = -1f;
            Vector2Int[] step = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
            int turns = p.kind == KCorner ? 4 : 2;
            for (int r = 0; r < turns; r++)
            {
                int m = Rot(baseMask, r);
                float score = 0f;
                for (int k = 0; k < 4; k++)
                {
                    int d = 1 << k;
                    if ((m & d) == 0) continue;
                    Vector2Int n = c + step[k];
                    if (n.x < 0 || n.x > 4 || n.y < 0 || n.y > 4) continue;
                    if ((MaskAt(n, true) & Opp(d)) != 0) score += 1f;
                    else if (IsSocket(n) && PieceAt(n) == null) score += 0.4f;
                }
                if (score > bestScore + 0.01f) { bestScore = score; best = r; }
            }
            return best;
        }

        IEnumerator Burn(Piece led)
        {
            busy = true;
            led.art.sprite = ledBurnt;
            led.art.color = Color.white;
            Haptics.Medium();
            Sfx.Play(this, "pop", 1f);
            Sfx.Play(this, "buzz", 0.5f);
            StartCoroutine(E40Kit.Shake(led.t, 0.08f));
            ledsLeft--;
            E40Kit.Hearts(hearts, ledsLeft);
            switchClosed = false;
            Say(ledsLeft > 0 ? "POP! 9 volts fried the LED!" : "POP! That was the last LED...", 2.2f);
            yield return new WaitForSeconds(1.1f);
            // il LED bruciato si stacca e sparisce
            Vector3 s = led.t.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.3f) { led.t.localScale = s * (1f - k); yield return null; }
            led.gone = true;
            led.cell = NoCell;
            led.t.gameObject.SetActive(false);
            if (ledsLeft <= 0)
            {
                yield return new WaitForSeconds(0.6f);
                Fail();
                yield break;
            }
            Say("Fit a spare LED... and protect it!", 2.4f);
            E40Kit.Goal(goal, "Replace the LED from the tray");
            stage = -1;
            busy = false;
            LayoutTray();
            Evaluate();
        }

        IEnumerator Open()
        {
            busy = true;
            opened = true;
            E40Kit.Dots(dots, 2);
            E40Kit.Goal(goal, "Power on! The safe is opening...");
            for (int i = 0; i < digits.Length && i < code.Length; i++)
            {
                digits[i].text = code[i].ToString();
                digits[i].color = new Color(0.4f, 1f, 0.45f);
                Sfx.Play(this, "tick", 0.8f);
                yield return new WaitForSeconds(0.25f);
            }
            Sfx.Play(this, "ding", 0.8f);
            yield return new WaitForSeconds(0.6f);
            Sfx.Play(this, "clunk", 1f);
            Haptics.Light();
            foreach (Piece p in pieces) if (!p.gone && p.cell != NoCell) p.t.SetParent(door, true);
            Vector3 s = door.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.9f)
            {
                door.localScale = new Vector3(s.x * Mathf.Lerp(1f, 0.08f, Mathf.SmoothStep(0f, 1f, k)), s.y, s.z);
                yield return null;
            }
            E40Kit.Dots(dots, 3);
            Say("The golden cheese!", 2.5f);
            Sfx.Play(this, "ding", 0.9f);
            yield return new WaitForSeconds(1.6f);
            Win();
        }
    }
}
