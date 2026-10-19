using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "The upside-down king" — un re sul trono ha la corona incastrata (a punte in su) nel soffitto. Soluzione:
    /// inclinare il telefono per far scivolare la scala sotto la corona, poi trascinare il re sulla scala: sale e se la
    /// riprende. Sconfitta: la scala cade tre volte (inclinazione troppo forte troppo a lungo, o botta contro il muro).
    /// (Editor: frecce destra/sinistra. Senza accelerometro compaiono due frecce da tenere premute.)
    /// </summary>
    public class UpsideDownKingLevel : LevelController
    {
        [SerializeField] Transform ladder;               // perno alla base della scala
        [SerializeField] Transform king;
        [SerializeField] Transform crown;
        [SerializeField] Transform throneSeat;           // dove siede il re
        [SerializeField] Transform kingHead;             // punto della testa (figlio del re)
        [SerializeField] float wallX = 2.15f, ladderStartX = -0.35f, crownX = 1.45f, ladderTopY = 0.95f;
        [SerializeField] GameObject arrows;              // ripiego senza accelerometro
        [SerializeField] Collider2D leftArrow, rightArrow;
        [SerializeField] SpriteRenderer[] hearts;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] SpriteRenderer dust;

        Camera cam;
        float x, vel, tooSteep;
        int falls;
        bool busy, dragKing;
        Vector3 crownStuck;
        Quaternion crownStuckRot;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            x = ladderStartX;
            crownStuck = crown.position;
            crownStuckRot = crown.rotation;
            bool fallback = !SystemInfo.supportsAccelerometer;
#if UNITY_EDITOR
            fallback = false;
#endif
            if (arrows != null) arrows.SetActive(fallback);
            if (dust != null) dust.gameObject.SetActive(false);
            Kit41B.Hearts(hearts, 3);
            king.position = throneSeat.position;
            PlaceLadder(0f);
            StartCoroutine(Intro());
        }

        IEnumerator Intro()
        {
            yield return new WaitForSeconds(0.5f);
            if (!busy) Kit41B.Say(bubble, "Who put my crown up THERE?!");
            yield return new WaitForSeconds(2.4f);
            if (!busy && bubble != null) bubble.Hide();
        }

        void PlaceLadder(float lean)
        {
            ladder.localPosition = new Vector3(x, ladder.localPosition.y, ladder.localPosition.z);
            ladder.localRotation = Quaternion.Euler(0f, 0f, lean);
        }

        float TiltX()
        {
            float gx = Inputs.Tilt().x;
            if (arrows != null && arrows.activeSelf)
            {
                if (PointerInput.IsHeld(cam, leftArrow)) gx = -0.4f;
                else if (PointerInput.IsHeld(cam, rightArrow)) gx = 0.4f;
            }
            return gx;
        }

        void Update()
        {
            if (IsFinished || busy) return;
            float dt = Time.deltaTime;
            float gx = TiltX();

            // la scala scivola (zona morta piccola), e se l'inclinazione è troppa comincia a pendere
            float push = Mathf.Abs(gx) > 0.08f ? gx : 0f;
            vel += push * 4f * dt;
            vel *= 1f - Mathf.Min(1f, dt * 2.5f);
            x += vel * dt;
            if (Mathf.Abs(gx) > 0.6f) tooSteep += dt; else tooSteep = Mathf.Max(0f, tooSteep - dt * 2f);
            float lean = -Mathf.Sign(gx) * Mathf.Max(0f, Mathf.Abs(gx) - 0.35f) * 30f - Mathf.Sign(gx) * tooSteep * 25f;
            if (Mathf.Abs(x) > wallX)
            {
                bool hard = Mathf.Abs(vel) > 1.6f;
                x = Mathf.Sign(x) * wallX;
                if (hard) { StartCoroutine(Topple(Mathf.Sign(vel), "BONK! Too fast!")); return; }
                if (Mathf.Abs(vel) > 0.3f) Sfx.Play(this, "tick", 0.5f);
                vel = 0f;
            }
            if (tooSteep > 0.7f) { StartCoroutine(Topple(Mathf.Sign(gx), "Too much tilt!")); return; }
            PlaceLadder(lean);

            HandleKing();
        }

        void HandleKing()
        {
            if (!Kit41B.Pointer(cam, out Vector2 w, out bool began, out bool ended))
            {
                if (dragKing) DropKing(king.position);
                return;
            }
            if (began)
            {
                if (Vector2.Distance(w, king.position + Vector3.up * 0.4f) < 0.8f) { dragKing = true; Sfx.Play(this, "pop", 0.4f); }
                return;
            }
            if (!dragKing) return;
            king.position = new Vector3(w.x, w.y - 0.3f, king.position.z);
            if (ended) DropKing(w);
        }

        void DropKing(Vector2 w)
        {
            dragKing = false;
            if (Mathf.Abs(w.x - ladder.position.x) < 0.75f && w.y > -2.6f && w.y < ladderTopY + 1.2f)
            {
                StartCoroutine(Climb(Mathf.Abs(ladder.position.x - crownX) < 0.4f));
                return;
            }
            king.position = throneSeat.position;
            Kit41B.Say(bubble, "Put me on something TALL!");
        }

        IEnumerator Topple(float side, string why)
        {
            busy = true;
            if (dragKing) { dragKing = false; king.position = throneSeat.position; }
            falls++;
            Kit41B.Hearts(hearts, 3 - falls);
            Kit41B.Say(bubble, why);
            float from = ladder.localEulerAngles.z;
            if (from > 180f) from -= 360f;
            float to = -side * 88f;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.55f)
            {
                ladder.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(from, to, k * k));
                yield return null;
            }
            ladder.localRotation = Quaternion.Euler(0f, 0f, to);
            Sfx.Play(this, "bonk", 1f);
            Sfx.Play(this, "clunk", 0.8f);
            Haptics.Heavy();
            yield return Kit41B.Shake(ladder, 0.1f, 0.3f);
            yield return new WaitForSeconds(0.8f);
            if (falls >= 3) { Kit41B.Say(bubble, "Guards! Ban all ladders!"); yield return new WaitForSeconds(1f); Fail(); yield break; }
            // la scala viene rimessa in piedi
            x = ladderStartX;
            vel = 0f;
            tooSteep = 0f;
            Sfx.Play(this, "whoosh", 0.5f);
            PlaceLadder(0f);
            yield return Kit41B.Pop(ladder, 0.08f, 0.25f);
            if (bubble != null) bubble.Hide();
            busy = false;
        }

        IEnumerator Climb(bool reaches)
        {
            busy = true;
            PlaceLadder(0f);
            Vector3 bottom = new Vector3(ladder.position.x, ladder.position.y + 0.15f, king.position.z);
            Vector3 top = new Vector3(ladder.position.x, ladderTopY, king.position.z);
            king.position = bottom;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 1.4f)
            {
                king.position = Vector3.Lerp(bottom, top, k) + Vector3.right * Mathf.Sin(k * 30f) * 0.04f;
                if (Mathf.Repeat(k, 0.15f) < Time.deltaTime / 1.4f) Sfx.Play(this, "tick", 0.4f);
                yield return null;
            }
            king.position = top;
            if (!reaches)
            {
                Kit41B.Say(bubble, "Can't reach it! Move the ladder!");
                yield return Kit41B.Shake(king, 0.06f, 0.5f);
                yield return new WaitForSeconds(0.8f);
                for (float k = 0f; k < 1f; k += Time.deltaTime / 0.8f) { king.position = Vector3.Lerp(top, bottom, k); yield return null; }
                king.position = throneSeat.position;
                busy = false;
                yield break;
            }
            // la stacca dal soffitto e la rimette dritta in testa
            if (dust != null) { dust.gameObject.SetActive(true); dust.transform.position = crownStuck; StartCoroutine(Kit41B.Pop(dust.transform, 0.4f, 0.4f)); }
            Sfx.Play(this, "pop", 0.9f);
            Haptics.Light();
            Vector3 c0 = crown.position;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.6f)
            {
                Vector3 head = kingHead != null ? kingHead.position : king.position + Vector3.up * 0.9f;
                crown.position = Vector3.Lerp(c0, head, k) + Vector3.up * Mathf.Sin(k * Mathf.PI) * 0.4f;
                crown.rotation = Quaternion.Slerp(crownStuckRot, Quaternion.identity, k);
                yield return null;
            }
            crown.SetParent(kingHead != null ? kingHead : king, true);
            crown.rotation = Quaternion.identity;
            if (dust != null) dust.gameObject.SetActive(false);
            Sfx.Play(this, "ding", 0.9f);
            Kit41B.Say(bubble, "Long live ME!");
            yield return Kit41B.Pop(king, 0.15f, 0.4f);
            yield return new WaitForSeconds(1.3f);
            Win();
        }
    }
}
