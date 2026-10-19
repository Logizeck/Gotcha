using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "Knight's quest" — scacchiera 5x5. Fase 1: il cavaliere (gatto a cavallo) parte da a1 e deve mangiare i
    /// 4 topi-pedone (b5, c2, d4, e3) in al massimo 6 mosse a L (si toccano le caselle evidenziate). Soluzione unica in 5
    /// (verificata con una ricerca): c2, e3, c2 (si torna indietro!), d4, b5. Fase 2: il gatto-re dorme nell'angolo e5
    /// col suo gomitolo in d5; con la torre (a1) e il cavaliere (b2) hai 2 mosse per dare scacco matto: torre in a4 e
    /// cavaliere in d3 (in qualsiasi ordine). Senza riuscirci si perde un cuore (3) e la fase ricomincia. C'è "annulla".
    /// </summary>
    public class KnightLevel : LevelController
    {
        public const float Cell = 0.95f;
        public static readonly Vector2 Origin = new Vector2(-1.9f, -2.5f);   // centro di a1
        public static readonly Vector2Int[] MiceCells = { new Vector2Int(1, 4), new Vector2Int(2, 1), new Vector2Int(3, 3), new Vector2Int(4, 2) };
        public static readonly Vector2Int KingCell = new Vector2Int(4, 4), YarnCell = new Vector2Int(3, 4);
        public static readonly Vector2Int RookStart = new Vector2Int(0, 0), KnightStart2 = new Vector2Int(1, 1);

        [SerializeField] Transform knight, rook, king, yarn;
        [SerializeField] Transform[] mice;
        [SerializeField] Transform[] moveDots;
        [SerializeField] Transform selectRing;
        [SerializeField] Collider2D undoButton;
        [SerializeField] TextMesh movesText;
        [SerializeField] TextMesh banner;
        [SerializeField] SpriteRenderer[] dots;
        [SerializeField] SpriteRenderer[] hearts;
        [SerializeField] SpriteRenderer veil;
        [SerializeField] TextMesh goal;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] int maxMoves1 = 6, maxMoves2 = 2;

        static readonly Vector2Int[] Jumps =
        {
            new Vector2Int(1, 2), new Vector2Int(2, 1), new Vector2Int(-1, 2), new Vector2Int(-2, 1),
            new Vector2Int(1, -2), new Vector2Int(2, -1), new Vector2Int(-1, -2), new Vector2Int(-2, -1),
        };

        struct Snapshot { public Vector2Int knight, rook; public int captured; }

        Camera cam;
        int stage, lives = 3, moves, captured;
        Vector2Int knightAt, rookAt;
        Transform selected;
        readonly List<Vector2Int> targets = new List<Vector2Int>();
        readonly Stack<Snapshot> history = new Stack<Snapshot>();
        bool busy;
        float bubbleUntil, clock;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            if (banner != null) banner.gameObject.SetActive(false);
            E40Kit.Hearts(hearts, lives);
            StartStage1();
            Say("Catch the mice! Knights jump in an L.", 2.4f);
        }

        static Vector3 Pos(Vector2Int c) => new Vector3(Origin.x + c.x * Cell, Origin.y + c.y * Cell, 0f);

        static Vector2Int CellAt(Vector2 w)
        {
            int c = Mathf.RoundToInt((w.x - Origin.x) / Cell), r = Mathf.RoundToInt((w.y - Origin.y) / Cell);
            return new Vector2Int(c, r);
        }

        static bool In(Vector2Int c) => c.x >= 0 && c.x < 5 && c.y >= 0 && c.y < 5;

        void Say(string text, float seconds)
        {
            if (bubble == null) return;
            bubble.Show(text);
            bubbleUntil = clock + seconds;
        }

        // ---------- Fase 1 ----------

        void StartStage1()
        {
            stage = 0;
            moves = 0;
            captured = 0;
            history.Clear();
            knightAt = Vector2Int.zero;
            knight.localPosition = Pos(knightAt);
            for (int i = 0; i < mice.Length; i++) { mice[i].gameObject.SetActive(true); mice[i].localPosition = Pos(MiceCells[i]); mice[i].localScale = Vector3.one; }
            rook.gameObject.SetActive(false);
            king.gameObject.SetActive(false);
            yarn.gameObject.SetActive(false);
            E40Kit.Dots(dots, 0);
            E40Kit.Goal(goal, "Catch all 4 mice in 6 moves or less");
            selected = knight;
            ShowTargets();
        }

        // ---------- Fase 2 ----------

        void StartStage2()
        {
            stage = 1;
            moves = 0;
            history.Clear();
            foreach (Transform m in mice) m.gameObject.SetActive(false);
            knightAt = KnightStart2;
            rookAt = RookStart;
            knight.localPosition = Pos(knightAt);
            rook.gameObject.SetActive(true);
            rook.localPosition = Pos(rookAt);
            king.gameObject.SetActive(true);
            king.localPosition = Pos(KingCell);
            king.localRotation = Quaternion.identity;
            yarn.gameObject.SetActive(true);
            yarn.localPosition = Pos(YarnCell);
            E40Kit.Dots(dots, 1);
            E40Kit.Goal(goal, "Checkmate the sleeping king in 2 moves");
            selected = null;
            ShowTargets();
        }

        void UpdateMoves()
        {
            if (movesText != null) movesText.text = "Moves: " + moves + "/" + (stage == 0 ? maxMoves1 : maxMoves2);
        }

        bool Occupied(Vector2Int c)
        {
            if (stage == 1) return c == KingCell || c == YarnCell || c == knightAt || c == rookAt;
            return false;
        }

        List<Vector2Int> MovesOf(Transform piece)
        {
            var list = new List<Vector2Int>();
            if (piece == knight)
            {
                foreach (Vector2Int j in Jumps)
                {
                    Vector2Int n = knightAt + j;
                    if (In(n) && !Occupied(n)) list.Add(n);
                }
            }
            else if (piece == rook)
            {
                foreach (Vector2Int d in new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
                    for (Vector2Int n = rookAt + d; In(n) && !Occupied(n); n += d) list.Add(n);
            }
            return list;
        }

        void ShowTargets()
        {
            targets.Clear();
            if (selected != null) targets.AddRange(MovesOf(selected));
            for (int i = 0; i < moveDots.Length; i++)
            {
                bool on = i < targets.Count;
                moveDots[i].gameObject.SetActive(on);
                if (!on) continue;
                moveDots[i].localPosition = Pos(targets[i]);
                bool prey = stage == 0 && MouseAt(targets[i]) >= 0;
                SpriteRenderer sr = moveDots[i].GetComponentInChildren<SpriteRenderer>();
                if (sr != null) sr.color = prey ? new Color(1f, 0.35f, 0.35f, 0.9f) : new Color(0.35f, 0.9f, 0.45f, 0.85f);
            }
            if (selectRing != null)
            {
                selectRing.gameObject.SetActive(selected != null);
                if (selected != null) selectRing.localPosition = selected.localPosition;
            }
            UpdateMoves();
        }

        int MouseAt(Vector2Int c)
        {
            for (int i = 0; i < MiceCells.Length; i++) if ((captured & (1 << i)) == 0 && MiceCells[i] == c) return i;
            return -1;
        }

        void Update()
        {
            if (IsFinished) return;
            clock += Time.deltaTime;
            if (bubble != null && bubbleUntil > 0f && clock > bubbleUntil) { bubble.Hide(); bubbleUntil = 0f; }
            if (busy) return;
            if (!PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out bool _) || !began || PointerInput.IsOverUi()) return;
            Vector2 w = PointerInput.ToWorld(cam, sp);
            if (undoButton != null && undoButton.OverlapPoint(w)) { Undo(); return; }
            Vector2Int c = CellAt(w);
            if (!In(c)) return;
            if (targets.Contains(c)) { StartCoroutine(Move(c)); return; }
            if (stage == 1)
            {
                if (c == knightAt) Select(knight);
                else if (c == rookAt) Select(rook);
                else if (c == KingCell) Say("Zzz... (he won't move. Trap him!)", 1.8f);
                else Select(null);
            }
            else if (c != knightAt) { Sfx.Play(this, "bonk", 0.4f); Say("Knights only jump in an L!", 1.4f); }
        }

        void Select(Transform piece)
        {
            selected = piece;
            if (piece != null) { Sfx.Play(this, "tick", 0.6f); StartCoroutine(E40Kit.Pop(piece, 0.12f, 0.2f)); }
            ShowTargets();
        }

        void Undo()
        {
            if (history.Count == 0) { Say("Nothing to undo.", 1f); return; }
            Snapshot s = history.Pop();
            knightAt = s.knight;
            rookAt = s.rook;
            captured = s.captured;
            moves--;
            knight.localPosition = Pos(knightAt);
            if (stage == 1) rook.localPosition = Pos(rookAt);
            for (int i = 0; i < mice.Length; i++) mice[i].gameObject.SetActive(stage == 0 && (captured & (1 << i)) == 0);
            Sfx.Play(this, "whoosh", 0.4f);
            if (stage == 0) selected = knight;
            ShowTargets();
        }

        IEnumerator Move(Vector2Int to)
        {
            busy = true;
            history.Push(new Snapshot { knight = knightAt, rook = rookAt, captured = captured });
            Transform piece = selected;
            foreach (Transform d in moveDots) d.gameObject.SetActive(false);
            if (selectRing != null) selectRing.gameObject.SetActive(false);
            Vector3 from = piece.localPosition, end = Pos(to);
            bool jump = piece == knight;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.28f)
            {
                piece.localPosition = Vector3.Lerp(from, end, k) + (jump ? Vector3.up * Mathf.Sin(k * Mathf.PI) * 0.45f : Vector3.zero);
                yield return null;
            }
            piece.localPosition = end;
            Sfx.Play(this, "clunk", 0.6f);
            if (piece == knight) knightAt = to; else rookAt = to;
            moves++;
            if (stage == 0) yield return AfterMove1(to);
            else yield return AfterMove2();
        }

        IEnumerator AfterMove1(Vector2Int to)
        {
            int m = MouseAt(to);
            if (m >= 0)
            {
                captured |= 1 << m;
                Sfx.Play(this, "pop", 0.8f);
                Haptics.Light();
                Transform mouse = mice[m];
                for (float k = 0f; k < 1f; k += Time.deltaTime / 0.25f) { mouse.localScale = Vector3.one * (1f - k); yield return null; }
                mouse.gameObject.SetActive(false);
                mouse.localScale = Vector3.one;
            }
            if (captured == (1 << mice.Length) - 1)
            {
                Say("Got them all!", 1.6f);
                Sfx.Play(this, "ding", 0.8f);
                yield return Transition();
                StartStage2();
                Say("The cat-king is napping. Trap him!", 2.6f);
                busy = false;
                yield break;
            }
            if (moves >= maxMoves1) { yield return LoseLife("Out of moves! The mice escaped."); if (!IsFinished) { StartStage1(); busy = false; } yield break; }
            ShowTargets();
            busy = false;
        }

        IEnumerator AfterMove2()
        {
            if (Mate())
            {
                yield return Checkmate();
                yield break;
            }
            if (moves >= maxMoves2)
            {
                bool check = Attacked(KingCell, rookAt, knightAt, false);
                yield return LoseLife(check ? "Check... but he can slip away!" : "That's not even check!");
                if (!IsFinished) { StartStage2(); busy = false; }
                yield break;
            }
            if (Attacked(KingCell, rookAt, knightAt, false)) Say("Check! Now close every escape.", 1.8f);
            selected = null;
            ShowTargets();
            busy = false;
        }

        IEnumerator LoseLife(string why)
        {
            lives--;
            E40Kit.Hearts(hearts, lives);
            Haptics.Medium();
            Sfx.Play(this, "buzz", 0.6f);
            Say(why, 2f);
            yield return new WaitForSeconds(1.2f);
            if (lives <= 0) { Fail(); yield break; }
            yield return Transition();
        }

        IEnumerator Transition()
        {
            yield return E40Kit.Fade(veil, 0.85f, 0.25f);
            yield return new WaitForSeconds(0.1f);
            StartCoroutine(E40Kit.Fade(veil, 0f, 0.3f));
        }

        // ---------- Regole dello scacco ----------

        /// <summary>La casella è attaccata da torre o cavaliere? (la torre è bloccata dai pezzi; "kingGone" = il re si è spostato).</summary>
        bool Attacked(Vector2Int sq, Vector2Int r, Vector2Int n, bool kingGone, bool rookAlive = true, bool knightAlive = true)
        {
            if (knightAlive)
            {
                Vector2Int d = sq - n;
                if ((Mathf.Abs(d.x) == 1 && Mathf.Abs(d.y) == 2) || (Mathf.Abs(d.x) == 2 && Mathf.Abs(d.y) == 1)) return true;
            }
            if (rookAlive && r != sq && (r.x == sq.x || r.y == sq.y))
            {
                Vector2Int step = new Vector2Int(System.Math.Sign(sq.x - r.x), System.Math.Sign(sq.y - r.y));
                for (Vector2Int p = r + step; p != sq; p += step)
                {
                    if (p == YarnCell || (knightAlive && p == n)) return false;
                    if (p == KingCell && !kingGone) return false;
                }
                return true;
            }
            return false;
        }

        bool Mate()
        {
            if (!Attacked(KingCell, rookAt, knightAt, false)) return false;
            for (int dx = -1; dx <= 1; dx++)
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;
                    Vector2Int s = KingCell + new Vector2Int(dx, dy);
                    if (!In(s) || s == YarnCell) continue;
                    bool rookAlive = s != rookAt, knightAlive = s != knightAt;   // il re può mangiare un pezzo indifeso
                    if (!Attacked(s, rookAt, knightAt, true, rookAlive, knightAlive)) return false;
                }
            return true;
        }

        IEnumerator Checkmate()
        {
            E40Kit.Dots(dots, 2);
            E40Kit.Goal(goal, "CHECKMATE!");
            Sfx.Play(this, "ding", 0.9f);
            Haptics.Light();
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.6f)
            {
                king.localRotation = Quaternion.Euler(0, 0, -90f * k * k);
                yield return null;
            }
            Sfx.Play(this, "bonk", 0.8f);
            if (banner != null) banner.gameObject.SetActive(true);
            Say("Checkmate, Your Majesty!", 2.4f);
            yield return E40Kit.Pop(banner != null ? banner.transform : king, 0.2f, 0.4f);
            yield return new WaitForSeconds(1.6f);
            Win();
        }
    }
}
