using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "Box Pusher" — Sokoban nel magazzino. Il topolino si muove di una casella per swipe (o toccando verso
    /// una direzione) e spinge le casse di formaggio se dietro c'è spazio. Tre casse sui tre segni rossi = festa del
    /// formaggio. Sorpresa: ogni tanto una zampa di gatto entra di lato su una casella a caso (ombra un secondo prima):
    /// se il topolino è lì viene spedito alla partenza. Soluzione (25 mosse): ←↑↑↑↑ ↓→→↓→ ↑↑←↑← ←↓↓↓↓ →→↓→↑.
    /// ANNULLA e RICOMINCIA sempre disponibili.
    /// </summary>
    public class BoxPusherLevel : LevelController
    {
        // mappa: # muro, . segno, $ cassa, @ topolino (riga 0 in alto)
        public static readonly string[] Layout =
        {
            "#######",
            "#.   .#",
            "# $#  #",
            "#  $  #",
            "## # .#",
            "#  $  #",
            "#  @  #",
            "#######",
        };
        public const float Cell = 0.72f;
        public const float BoardY = -0.35f;
        public static readonly Vector2Int PawCell = new Vector2Int(5, 3);

        /// <summary>Centro della casella (colonna c, riga r) in coordinate locali del livello.</summary>
        public static Vector2 CellPos(int c, int r) => new Vector2((c - 3) * Cell, BoardY + (3.5f - r) * Cell);

        [SerializeField] Transform mouse;
        [SerializeField] Transform[] crates;
        [SerializeField] GameObject[] glows;
        [SerializeField] Transform paw;
        [SerializeField] SpriteRenderer pawShadow;
        [SerializeField] TapButton undoButton;
        [SerializeField] TapButton resetButton;
        [SerializeField] TextMesh info;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] float stepSpeed = 7f;
        [SerializeField] float pawIdle = 2.8f, pawWarn = 1.0f, pawStrike = 0.6f;

        struct State { public Vector2Int me; public Vector2Int[] boxes; }

        readonly Stack<State> history = new Stack<State>();
        readonly List<Vector2Int> goals = new List<Vector2Int>();
        Vector2Int me, start;
        Vector2Int[] boxes, boxStart;
        bool busy, done;
        float pawT, mouseFace = 1f;
        Vector2Int pawCell = PawCell;   // casella che la zampa colpisce in questo giro (cambia a ogni colpo)
        readonly List<Vector2Int> floor = new List<Vector2Int>();
        Vector3 mouseScale, pawOut, pawIn;
        Vector2 downPos;
        bool tracking, swiped;
        Camera cam;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            var bx = new List<Vector2Int>();
            for (int r = 0; r < Layout.Length; r++)
                for (int c = 0; c < Layout[r].Length; c++)
                {
                    char ch = Layout[r][c];
                    if (ch == '.') goals.Add(new Vector2Int(c, r));
                    if (ch == '$') bx.Add(new Vector2Int(c, r));
                    if (ch == '@') start = new Vector2Int(c, r);
                    if (ch != '#') floor.Add(new Vector2Int(c, r));
                }
            boxStart = bx.ToArray();
            mouseScale = mouse.localScale;
            AimPaw(RandomPawCell());
            paw.localPosition = pawOut;
            SetShadow(0f);
            undoButton.Pressed += _ => Undo();
            resetButton.Pressed += _ => ResetBoard();
            Load(new State { me = start, boxes = (Vector2Int[])boxStart.Clone() }, true);
            if (bubble != null) { bubble.Show("Push all the cheese onto the marks!"); StartCoroutine(HideLater(2.6f)); }
        }

        // ---------- stato ----------

        void Load(State s, bool snap)
        {
            me = s.me;
            boxes = (Vector2Int[])s.boxes.Clone();
            if (snap)
            {
                mouse.localPosition = CellPos(me.x, me.y);
                for (int i = 0; i < crates.Length; i++) crates[i].localPosition = CellPos(boxes[i].x, boxes[i].y);
            }
            Refresh();
        }

        State Snapshot() => new State { me = me, boxes = (Vector2Int[])boxes.Clone() };

        bool Wall(Vector2Int p) => p.y < 0 || p.y >= Layout.Length || p.x < 0 || p.x >= Layout[p.y].Length || Layout[p.y][p.x] == '#';

        int BoxAt(Vector2Int p)
        {
            for (int i = 0; i < boxes.Length; i++) if (boxes[i] == p) return i;
            return -1;
        }

        int OnGoals()
        {
            int n = 0;
            for (int i = 0; i < boxes.Length; i++)
            {
                bool on = goals.Contains(boxes[i]);
                if (glows[i] != null) glows[i].SetActive(on);
                if (on) n++;
            }
            return n;
        }

        void Refresh() { info.text = "ON MARKS " + OnGoals() + "/" + goals.Count; }

        // ---------- mosse ----------

        void Move(Vector2Int dir)
        {
            if (busy || done || IsFinished) return;
            if (dir.x != 0) mouseFace = dir.x > 0 ? -1f : 1f;   // il disegno guarda a sinistra
            Vector2Int n = me + dir;
            if (Wall(n)) { Sfx.Play(this, "tick", 0.4f); StartCoroutine(Nudge(mouse, dir)); return; }
            int b = BoxAt(n);
            if (b >= 0)
            {
                Vector2Int beyond = n + dir;
                if (Wall(beyond) || BoxAt(beyond) >= 0)
                {
                    // la cassa non si muove: tonfo
                    Sfx.Play(this, "clunk", 0.7f);
                    Haptics.Light();
                    StartCoroutine(Nudge(crates[b], dir));
                    return;
                }
                history.Push(Snapshot());
                boxes[b] = beyond;
                Sfx.Play(this, "tick", 0.6f);
            }
            else history.Push(Snapshot());
            me = n;
            int on = OnGoals();
            info.text = "ON MARKS " + on + "/" + goals.Count;
            if (b >= 0 && goals.Contains(boxes[b])) Sfx.Play(this, "pop", 0.7f);
            if (on == goals.Count) StartCoroutine(Party());
        }

        void Undo()
        {
            if (busy || done || IsFinished || history.Count == 0) return;
            Load(history.Pop(), false);
            Sfx.Play(this, "whoosh", 0.4f);
        }

        void ResetBoard()
        {
            if (busy || done || IsFinished) return;
            history.Push(Snapshot());
            Load(new State { me = start, boxes = (Vector2Int[])boxStart.Clone() }, false);
            Sfx.Play(this, "whoosh", 0.5f);
        }

        IEnumerator Nudge(Transform t, Vector2Int dir)
        {
            Vector3 off = new Vector3(dir.x, -dir.y, 0f) * 0.08f;
            t.localPosition += off;
            yield return new WaitForSeconds(0.06f);
            t.localPosition -= off;
        }

        // ---------- ciclo ----------

        void Update()
        {
            if (IsFinished) return;
            float dt = Time.deltaTime;
            // le pedine scivolano verso la casella logica
            mouse.localPosition = Vector3.MoveTowards(mouse.localPosition, CellPos(me.x, me.y), stepSpeed * dt);
            for (int i = 0; i < crates.Length; i++)
                crates[i].localPosition = Vector3.MoveTowards(crates[i].localPosition, CellPos(boxes[i].x, boxes[i].y), stepSpeed * dt);
            float hop = Vector2.Distance(mouse.localPosition, CellPos(me.x, me.y)) > 0.02f ? 1f + Mathf.Sin(Time.time * 30f) * 0.06f : 1f;
            mouse.localScale = new Vector3(mouseScale.x * mouseFace, mouseScale.y * hop, mouseScale.z);
            if (done) return;
            UpdatePaw(dt);
            if (Time.timeScale > 0f) ReadInput();
        }

        /// <summary>
        /// La zampa punta una casella: entra dal lato più vicino (destra o sinistra), l'ombra di avviso va sulla casella.
        /// </summary>
        void AimPaw(Vector2Int cell)
        {
            pawCell = cell;
            Vector2 pc = CellPos(cell.x, cell.y);
            float side = cell.x >= 3 ? 1f : -1f;
            pawIn = new Vector3(pc.x + 0.1f * side, pc.y, 0f);
            pawOut = new Vector3(pc.x + 3.6f * side, pc.y, 0f);
            paw.localRotation = Quaternion.Euler(0, 0, -90f * side);
            pawShadow.transform.localPosition = new Vector3(pc.x, pc.y, pawShadow.transform.localPosition.z);
        }

        /// <summary>Una casella libera a caso (mai due volte di fila la stessa, mai la partenza).</summary>
        Vector2Int RandomPawCell()
        {
            var options = floor.FindAll(c => c != pawCell && c != start);
            return options.Count > 0 ? options[Random.Range(0, options.Count)] : PawCell;
        }

        void UpdatePaw(float dt)
        {
            pawT += dt;
            float cycle = pawIdle + pawWarn + pawStrike;
            if (pawT >= cycle) { pawT -= cycle; AimPaw(RandomPawCell()); }
            if (pawT < pawIdle) { SetShadow(0f); paw.localPosition = pawOut; return; }
            if (pawT < pawIdle + pawWarn)
            {
                // avviso: l'ombra cresce e pulsa
                float k = (pawT - pawIdle) / pawWarn;
                SetShadow(0.35f + 0.65f * k * (0.75f + 0.25f * Mathf.Sin(Time.time * 25f)));
                paw.localPosition = Vector3.Lerp(pawOut, pawIn, k * 0.25f);
                return;
            }
            float s = (pawT - pawIdle - pawWarn) / pawStrike;
            float reach = s < 0.2f ? s / 0.2f : s > 0.7f ? 1f - (s - 0.7f) / 0.3f : 1f;
            paw.localPosition = Vector3.Lerp(pawOut, pawIn, reach);
            SetShadow(1f - s);
            if (reach > 0.8f && me == pawCell && !busy) StartCoroutine(Bonk());
        }

        void SetShadow(float a)
        {
            pawShadow.color = new Color(1f, 1f, 1f, Mathf.Clamp01(a));
            pawShadow.transform.localScale = Vector3.one * (0.6f + 0.5f * Mathf.Clamp01(a));
        }

        /// <summary>La zampa colpisce: il topolino vola alla partenza (o alla casella libera più vicina).</summary>
        IEnumerator Bonk()
        {
            busy = true;
            Haptics.Medium();
            Sfx.Play(this, "bonk", 0.9f);
            if (bubble != null) bubble.Show("BONK! Sneaky paw!");
            Vector2Int to = FreeNear(start);
            Vector3 a = mouse.localPosition, b = CellPos(to.x, to.y);
            me = to;
            for (float t = 0f; t < 0.5f; t += Time.deltaTime)
            {
                float k = t / 0.5f;
                mouse.localPosition = Vector3.Lerp(a, b, k) + Vector3.up * Mathf.Sin(k * Mathf.PI) * 1.2f;
                mouse.localRotation = Quaternion.Euler(0, 0, k * 720f);
                yield return null;
            }
            mouse.localRotation = Quaternion.identity;
            mouse.localPosition = b;
            busy = false;
            yield return new WaitForSeconds(1.2f);
            if (bubble != null && !done) bubble.Hide();
        }

        Vector2Int FreeNear(Vector2Int from)
        {
            var seen = new HashSet<Vector2Int> { from };
            var q = new Queue<Vector2Int>();
            q.Enqueue(from);
            Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            while (q.Count > 0)
            {
                Vector2Int p = q.Dequeue();
                if (BoxAt(p) < 0 && p != pawCell) return p;
                foreach (Vector2Int d in dirs)
                {
                    Vector2Int n = p + d;
                    if (!Wall(n) && seen.Add(n)) q.Enqueue(n);
                }
            }
            return me;
        }

        void ReadInput()
        {
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.LeftArrow)) Move(new Vector2Int(-1, 0));
            if (Input.GetKeyDown(KeyCode.RightArrow)) Move(new Vector2Int(1, 0));
            if (Input.GetKeyDown(KeyCode.UpArrow)) Move(new Vector2Int(0, -1));
            if (Input.GetKeyDown(KeyCode.DownArrow)) Move(new Vector2Int(0, 1));
#endif
            if (!PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out bool ended)) { tracking = false; return; }
            Vector2 w = transform.InverseTransformPoint(PointerInput.ToWorld(cam, sp));
            if (began)
            {
                Vector2 wp = PointerInput.ToWorld(cam, sp);
                bool onButton = undoButton.GetComponent<Collider2D>().OverlapPoint(wp) || resetButton.GetComponent<Collider2D>().OverlapPoint(wp);
                tracking = !onButton && !PointerInput.IsOverUi();
                swiped = false;
                downPos = w;
            }
            if (!tracking) return;
            Vector2 d = w - downPos;
            if (d.magnitude > 0.45f)
            {
                // swipe: si può continuare a trascinare per fare più passi
                Move(Dominant(d));
                swiped = true;
                downPos = w;
            }
            if (ended)
            {
                tracking = false;
                if (swiped) return;
                Vector2 rel = w - (Vector2)mouse.localPosition;
                if (rel.magnitude > 0.3f) Move(Dominant(rel));   // tocco: un passo verso il dito
            }
        }

        static Vector2Int Dominant(Vector2 d) =>
            Mathf.Abs(d.x) > Mathf.Abs(d.y) ? new Vector2Int(d.x > 0 ? 1 : -1, 0) : new Vector2Int(0, d.y > 0 ? -1 : 1);

        /// <summary>Vittoria: casse illuminate e il topolino si abbuffa.</summary>
        IEnumerator Party()
        {
            done = true;
            SetShadow(0f);
            yield return new WaitForSeconds(0.25f);
            Sfx.Play(this, "ding", 0.8f);
            Haptics.Light();
            if (bubble != null) bubble.Show("Cheese party! Nom nom nom!");
            for (float t = 0f; t < 1.8f; t += Time.deltaTime)
            {
                paw.localPosition = Vector3.MoveTowards(paw.localPosition, pawOut, 6f * Time.deltaTime);
                mouse.localPosition = (Vector3)CellPos(me.x, me.y) + Vector3.up * Mathf.Abs(Mathf.Sin(t * 9f)) * 0.25f;
                for (int i = 0; i < crates.Length; i++) crates[i].localRotation = Quaternion.Euler(0, 0, Mathf.Sin(t * 12f + i) * 8f);
                yield return null;
            }
            Win();
        }

        IEnumerator HideLater(float s)
        {
            yield return new WaitForSeconds(s);
            if (bubble != null && !done && !busy) bubble.Hide();
        }
    }
}
