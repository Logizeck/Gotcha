using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "The crooked bridge" — il ponte tra due piattaforme è storto e fa cadere il topo (tre cadute =
    /// sconfitta). Trascinare un'estremità sola lo fa basculare intorno al centro (l'altra va dalla parte opposta),
    /// quindi non si raddrizza mai. Il trucco: due dita, una per estremità, e si portano insieme a livello delle
    /// piattaforme; poi si tocca il topo e attraversa.
    /// </summary>
    public class TheCrookedBridgeLevel : S41JLevelBase
    {
        [SerializeField] Transform plank;             // asse del ponte (centro, lunga 3.2 a scala 1)
        [SerializeField] SpriteRenderer plankArt;
        [SerializeField] Transform leftKnob, rightKnob;
        [SerializeField] Transform mouse;
        [SerializeField] Collider2D mouseArea;
        [SerializeField] Transform cheese;
        [SerializeField] TextMesh fallsText;
        [SerializeField] float endX = 1.55f, topY = -1.0f, plankLength = 3.2f;

        const int MaxFalls = 3;
        const float LevelTolerance = 0.2f, MinY = -3.6f, MaxY = 0.6f;
        float yL = -2.6f, yR = -1.4f;
        int holdL = -99, holdR = -99, falls;
        float offL, offR;
        bool saidHint;
        Vector3 mouseHome;

        protected override void Begin()
        {
            mouseHome = mouse.localPosition;
            UpdateBridge();
            UpdateFalls();
            Say("Is that bridge... safe? Tap me to cross!", 3f);
        }

        void UpdateFalls() { if (fallsText != null) fallsText.text = "FALLS " + falls + "/" + MaxFalls; }

        bool IsLevel => Mathf.Abs(yL - topY) < LevelTolerance && Mathf.Abs(yR - topY) < LevelTolerance;

        void UpdateBridge()
        {
            Vector2 a = new Vector2(-endX, yL), b = new Vector2(endX, yR);
            Vector2 d = b - a;
            plank.localPosition = (a + b) * 0.5f;
            plank.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg);
            plank.localScale = new Vector3(d.magnitude / plankLength, 1f, 1f);
            leftKnob.localPosition = a;
            rightKnob.localPosition = b;
            if (plankArt != null) plankArt.color = IsLevel ? new Color(0.75f, 1f, 0.75f) : Color.white;
        }

        protected override void Tick()
        {
            if (busy) return;
            bool wasLevel = IsLevel;
            float? fl = null, fr = null;
            foreach (Finger f in Fingers())
            {
                if (f.began)
                {
                    if (holdL == -99 && Vector2.Distance(f.world, leftKnob.position) < 0.7f) { holdL = f.id; offL = yL - f.world.y; Sfx.Play(this, "tick", 0.4f); }
                    else if (holdR == -99 && Vector2.Distance(f.world, rightKnob.position) < 0.7f) { holdR = f.id; offR = yR - f.world.y; Sfx.Play(this, "tick", 0.4f); }
                    else if (holdL == -99 && holdR == -99 && mouseArea != null && mouseArea.OverlapPoint(f.world)) { StartCoroutine(Cross()); return; }
                }
                if (f.id == holdL) { if (f.ended) holdL = -99; else fl = f.world.y + offL; }
                if (f.id == holdR) { if (f.ended) holdR = -99; else fr = f.world.y + offR; }
            }
            if (fl == null && holdL != -99) holdL = -99;   // dito sparito senza "ended"
            if (fr == null && holdR != -99) holdR = -99;

            bool both = fl.HasValue && fr.HasValue;
#if UNITY_EDITOR
            if (Input.GetKey(KeyCode.LeftShift) && (fl.HasValue || fr.HasValue)) both = true;   // Editor: Shift = seconda mano ferma
            if (both && !fl.HasValue) fl = yL;
            if (both && !fr.HasValue) fr = yR;
#endif
            if (both)
            {
                yL = Mathf.Clamp(fl.Value, MinY, MaxY);
                yR = Mathf.Clamp(fr.Value, MinY, MaxY);
            }
            else if (fl.HasValue || fr.HasValue)
            {
                // una mano sola: il ponte bascula intorno al centro
                float c = (yL + yR) * 0.5f;
                float span = Mathf.Min(c - MinY, MaxY - c);
                if (fl.HasValue) { yL = Mathf.Clamp(fl.Value, c - span, c + span); yR = 2f * c - yL; }
                else { yR = Mathf.Clamp(fr.Value, c - span, c + span); yL = 2f * c - yR; }
                if (!saidHint && Mathf.Abs(yL - yR) > 0.5f) { saidHint = true; Say("It just wobbles around the middle!", 2.4f); }
            }
            if (!fl.HasValue && !fr.HasValue && IsLevel && (yL != topY || yR != topY))
            {
                yL = yR = topY;   // aggancio
                Sfx.Play(this, "clunk", 0.6f);
                Haptics.Light();
            }
            if (IsLevel && !wasLevel) Sfx.Play(this, "pop", 0.5f);
            UpdateBridge();
        }

        IEnumerator Cross()
        {
            busy = true;
            holdL = holdR = -99;
            Vector3 edge = new Vector3(-endX + 0.1f, topY, 0f);
            if (IsLevel)
            {
                yL = yR = topY;
                UpdateBridge();
                Say("Here I go!", 1.5f);
                yield return Walk(mouse, new Vector3(endX + 0.6f, topY, 0f), 1.8f, 0.06f);
                yield return Walk(mouse, new Vector3(cheese.localPosition.x - 0.4f, topY, 0f), 1.8f, 0.06f);
                Sfx.Play(this, "pop", 0.8f);
                Say("CHEESE! Nice bridge!", 2f);
                yield return Move(mouse, mouse.localPosition, 0.35f, 0.45f);
                yield return Celebrate();
                yield break;
            }
            Say("Okay... one step...", 1.2f);
            yield return Walk(mouse, edge, 1.6f, 0.06f);
            // il ponte storto lo fa cadere
            falls++;
            UpdateFalls();
            Say(falls >= MaxFalls ? "NOT AGAIN! I give up!" : "WHOAAA!", 1.8f);
            Sfx.Play(this, "whoosh", 0.7f);
            Vector3 v = new Vector3(0.8f, 1.5f, 0f);
            for (float k = 0f; k < 1.2f; k += Time.deltaTime)
            {
                v.y -= 9f * Time.deltaTime;
                mouse.localPosition += v * Time.deltaTime;
                mouse.Rotate(0f, 0f, -360f * Time.deltaTime);
                yield return null;
            }
            Sfx.Play(this, "bonk", 0.9f);
            Haptics.Heavy();
            yield return new WaitForSeconds(0.5f);
            if (falls >= MaxFalls) { Fail(); yield break; }
            mouse.localRotation = Quaternion.identity;
            mouse.localPosition = mouseHome + Vector3.up * 3f;
            yield return Move(mouse, mouseHome, 0.4f);
            Sfx.Play(this, "bonk", 0.5f);
            Haptics.Light();
            busy = false;
        }
    }
}
