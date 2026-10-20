using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "The broken ruler" — il ponte sul burrone deve essere lungo esattamente 7 unità, ma il righello è rotto
    /// in tre pezzi con la numerazione incompleta. Si riattaccano i pezzi facendo combaciare le rotture (si agganciano
    /// da soli), si appoggia il righello al ponte e si trascina la fine del ponte sul 7. Poi TEST: l'ispettore attraversa.
    /// Tre ponti della lunghezza sbagliata = sconfitta.
    /// </summary>
    public class TheBrokenRulerLevel : LevelController
    {
        [SerializeField] Transform[] pieces;          // A (0-3), B (3-5), C (5-8): origine all'estremo sinistro
        [SerializeField] Collider2D[] pieceAreas;
        [SerializeField] Transform plank, handle, inspector, testButton;
        [SerializeField] Collider2D handleArea, testArea;
        [SerializeField] SpriteRenderer stamp;
        [SerializeField] TextMesh missText;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] SmokeEmitter[] confetti;
        [SerializeField] int missesAllowed = 3, targetUnits = 7, startUnits = 5;
        [SerializeField] float unit = 0.5f, bridgeX = -2.0f, bridgeY = -1.2f, rulerY = -0.72f, rightCliffX = 1.1f;

        static readonly int[] PieceUnits = { 3, 2, 3 };

        Camera cam;
        int drag = -1, dragFinger, misses;
        bool ab, bc, busy, handleDrag;
        float lengthUnits;
        Vector2 dragOffset;
        Vector3 inspectorHome;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            inspectorHome = inspector.localPosition;
            SetLength(startUnits);
            stamp.gameObject.SetActive(false);
            S41HKit.Confetti(confetti, false);
            UpdateMisses();
            S41HKit.Say(bubble, "Exactly 7 units, please. My ruler? Er... it broke.");
        }

        void UpdateMisses() { if (missText != null) missText.text = "BAD BRIDGES " + misses + "/" + missesAllowed; }

        void SetLength(float units)
        {
            lengthUnits = Mathf.Clamp(Mathf.Round(units * 2f) / 2f, 3f, 9f);
            float w = lengthUnits * unit;
            plank.localScale = new Vector3(w, 1f, 1f);
            handle.localPosition = new Vector3(bridgeX + w, bridgeY, 0f);
        }

        float Width(int i) => PieceUnits[i] * unit;
        Vector3 RightEnd(int i) => pieces[i].position + new Vector3(Width(i), 0f, 0f);

        void Update()
        {
            if (IsFinished || busy) return;
            foreach (S41HKit.Finger f in S41HKit.Fingers(cam))
            {
                if (f.began && drag < 0 && !handleDrag)
                {
                    if (testArea.OverlapPoint(f.world)) { StartCoroutine(Test()); return; }
                    if (handleArea.OverlapPoint(f.world)) { handleDrag = true; dragFinger = f.id; Sfx.Play(this, "tick", 0.5f); }
                    else
                        for (int i = pieces.Length - 1; i >= 0; i--)
                            if (pieceAreas[i].OverlapPoint(f.world)) { drag = i; dragFinger = f.id; dragOffset = (Vector2)pieces[i].position - f.world; Sfx.Play(this, "tick", 0.5f); break; }
                }
                if (f.id != dragFinger) continue;
                if (handleDrag)
                {
                    float before = lengthUnits;
                    SetLength((f.world.x - bridgeX) / unit);
                    if (!Mathf.Approximately(before, lengthUnits)) Sfx.Play(this, "tick", 0.35f);
                    if (f.ended) handleDrag = false;
                }
                else if (drag >= 0)
                {
                    MoveChain(drag, f.world + dragOffset);
                    if (f.ended) { int i = drag; drag = -1; DropPiece(i); }
                }
            }
            if (S41HKit.Fingers(cam).Count == 0)
            {
                handleDrag = false;
                if (drag >= 0) { int i = drag; drag = -1; DropPiece(i); }
            }
        }

        /// <summary>Sposta il pezzo trascinato insieme a quelli già agganciati.</summary>
        void MoveChain(int i, Vector2 leftEnd, bool clamp = true)
        {
            if (clamp)
            {
                leftEnd.x = Mathf.Clamp(leftEnd.x, -2.7f, 2.6f - Width(i));
                leftEnd.y = Mathf.Clamp(leftEnd.y, -4.2f, 2.6f);
            }
            Vector3 delta = (Vector3)leftEnd - pieces[i].position;
            delta.z = 0f;
            pieces[i].position += delta;
            if (i == 0) { if (ab) { pieces[1].position += delta; if (bc) pieces[2].position += delta; } }
            else if (i == 1) { if (ab) pieces[0].position += delta; if (bc) pieces[2].position += delta; }
            else { if (bc) { pieces[1].position += delta; if (ab) pieces[0].position += delta; } }
        }

        void DropPiece(int i)
        {
            bool snapped = false;
            if (!ab && Vector2.Distance(RightEnd(0), pieces[1].position) < 0.4f)
            {
                ab = true;
                snapped = true;
                if (i == 0) MoveChain(0, pieces[1].position - new Vector3(Width(0), 0f, 0f), false);
                else MoveChain(1, RightEnd(0), false);
            }
            if (!bc && Vector2.Distance(RightEnd(1), pieces[2].position) < 0.4f)
            {
                bc = true;
                snapped = true;
                if (i == 2) MoveChain(2, RightEnd(1), false);
                else MoveChain(1, pieces[2].position - new Vector3(Width(1), 0f, 0f), false);
            }
            if (snapped)
            {
                Sfx.Play(this, "clunk", 0.8f);
                Haptics.Light();
                S41HKit.Say(bubble, ab && bc ? "Good as new! Now measure the bridge." : "Click! The marks match.");
            }
            else if (WrongNeighbour(i)) { S41HKit.Say(bubble, "Those broken edges don't match."); Sfx.Play(this, "bonk", 0.4f); }

            // righello intero appoggiato al ponte: si allinea allo zero
            if (ab && bc && Vector2.Distance(pieces[0].position, new Vector2(bridgeX, rulerY)) < 0.9f)
            {
                MoveChain(0, new Vector2(bridgeX, rulerY), false);
                Sfx.Play(this, "clunk", 0.6f);
            }
        }

        bool WrongNeighbour(int i)
        {
            for (int a = 0; a < pieces.Length; a++)
                for (int b = 0; b < pieces.Length; b++)
                {
                    if (a == b || b == a + 1 || (a != i && b != i)) continue;
                    if (Vector2.Distance(RightEnd(a), pieces[b].position) < 0.4f) return true;
                }
            return false;
        }

        IEnumerator Test()
        {
            busy = true;
            StartCoroutine(S41HKit.Pop(testButton, 0.15f, 0.2f));
            Sfx.Play(this, "tick", 0.6f);
            float end = bridgeX + lengthUnits * unit;
            bool reaches = end > rightCliffX + 0.1f;
            bool exact = Mathf.Approximately(lengthUnits, targetUnits);
            Vector3 p = inspector.localPosition;
            if (!reaches)
            {
                yield return StartCoroutine(S41HKit.Move(inspector, new Vector3(end + 0.1f, p.y, 0f), 1.2f));
                S41HKit.Say(bubble, "TOO SHORT! Aaaah!");
                Sfx.Play(this, "whoosh", 0.7f);
                yield return StartCoroutine(S41HKit.Move(inspector, new Vector3(end + 0.4f, -4.6f, 0f), 0.6f));
                Sfx.Play(this, "bonk", 0.7f);
                Haptics.Medium();
                yield return StartCoroutine(Miss(null));
                yield break;
            }
            yield return StartCoroutine(S41HKit.Move(inspector, new Vector3(-0.1f, p.y, 0f), 1.2f));
            if (!exact)
            {
                yield return StartCoroutine(Miss(lengthUnits.ToString("0.#") + " units?! I ordered 7!"));
                yield break;
            }
            yield return StartCoroutine(S41HKit.Move(inspector, new Vector3(2.3f, p.y, 0f), 1.2f));
            stamp.gameObject.SetActive(true);
            Sfx.Play(this, "clunk", 0.9f);
            Haptics.Medium();
            StartCoroutine(S41HKit.Pop(stamp.transform, 0.3f, 0.3f));
            S41HKit.Say(bubble, "EXACTLY 7! APPROVED!");
            Sfx.Play(this, "ding", 0.8f);
            S41HKit.Confetti(confetti, true);
            yield return new WaitForSeconds(2.2f);
            S41HKit.Confetti(confetti, false);
            Win();
        }

        IEnumerator Miss(string complaint)
        {
            misses++;
            UpdateMisses();
            if (complaint != null) S41HKit.Say(bubble, complaint);
            Sfx.Play(this, "buzz", 0.5f);
            StartCoroutine(S41HKit.Shake(plank, 0.08f, 0.4f));
            yield return new WaitForSeconds(1.2f);
            if (misses >= missesAllowed)
            {
                S41HKit.Say(bubble, "Contract cancelled. Bye!");
                yield return new WaitForSeconds(1.1f);
                Fail();
                yield break;
            }
            inspector.localPosition = inspectorHome;
            SetLength(startUnits);
            S41HKit.Say(bubble, ab && bc ? "Use the ruler! Count the marks." : "Fix the ruler first. Match the broken edges.");
            busy = false;
        }
    }
}
