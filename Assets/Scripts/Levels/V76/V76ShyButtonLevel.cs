using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels.V64;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 2 della roadmap "The Shy Button" (id shy_button) — "PRESS START." Su uno sfondo semplice c'è un grosso
    /// pulsante verde START con gli occhi e l'aria nervosa. Un tocco veloce lo fa scappare (alla risalita del dito, con
    /// uno sbuffo) in un altro punto dello schermo, sempre dentro la zona sicura; dopo 3 fughe suda e ha gli occhi più
    /// nervosi, dopo 5 dice una frase-indizio ("It's scared of being touched... and of being HELD.").
    /// Soluzione: tenere un dito sul pulsante (0,25 s: si blocca, trema, suda, "Uh oh.") e, senza alzarlo, toccarlo con
    /// un secondo dito (anche appena fuori dal bordo) -> occhi spalancati, START!, coriandoli, LevelReward
    /// "YOU OUTSMARTED A BUTTON!", Win. Alzare il dito che lo blocca prima del secondo tocco -> scappa di nuovo.
    /// Mai sconfitta. Dita: V64AFingers (al massimo due id, nell'Editor Shift+clic pianta un secondo dito fermo).
    /// Nello stesso fotogramma si passano prima le dita che scendono e poi quelle che salgono (scelta generosa).
    /// La logica è in Gotcha.Levels.V64.ShyButtonLogic.
    /// Tasti di prova nell'Editor: T tocco veloce sul pulsante, H appoggia/alza il dito che lo tiene, J tocco del
    /// secondo dito sul pulsante, F secondo dito lontano, G tocco vicino (sobbalzo), P pausa simulata (dita annullate),
    /// R ricomincia il livello. Col mouse: Shift+clic sul pulsante (dito piantato), attendere, clic sul pulsante.
    /// </summary>
    public class V76ShyButtonLevel : LevelController
    {
        [Header("Pulsante")]
        [SerializeField] Transform button;           // radice che si sposta
        [SerializeField] Transform body;             // la parte che trema, si schiaccia, si allunga
        [SerializeField] Transform faceNervous;
        [SerializeField] Transform facePanic;
        [SerializeField] Transform facePinned;
        [SerializeField] Transform faceWide;
        [SerializeField] SpriteRenderer sweat;       // la goccia che compare dopo 3 fughe
        [SerializeField] SpriteRenderer sweatPinned; // la seconda goccia, quando è bloccato
        [SerializeField] TextMesh startLabel;
        [SerializeField] SpriteRenderer puff;        // lo sbuffo lasciato al posto di prima
        [Header("Limiti del centro del pulsante (zona sicura: pulsante in scala 1.2, largo 2.9, bordi entro x ±2.25)")]
        [SerializeField] float minX = -0.8f;
        [SerializeField] float maxX = 0.8f;
        [SerializeField] float minY = -3.0f;
        [SerializeField] float maxY = 1.75f;
        [Header("Scritte e festa")]
        [SerializeField] TextMesh infoText;
        [SerializeField] Transform infoHolder;
        [SerializeField] LevelReward reward;

        const string Compliment = "YOU OUTSMARTED A BUTTON!";
        const string HintLine = "It's scared of being touched... and of being HELD.";
        static readonly string[] DodgeLines = { "Nope!", "Too slow!", "Hehe." };
        static readonly string[] FlinchLines = { "Eek!", "Don't even think about it.", "Personal space!" };
        static readonly string[] MissLines = { "Missed me! Phew.", "Ha! Wrong spot.", "Not even close. Phew." };
        static readonly string[] PinLines = { "Uh oh.", "Uh oh. Uh oh. Uh oh.", "Oh no. Not again." };
        const string NudgeLine = "Just... don't bring a friend. Please.";
        static readonly Color StartColor = Color.white, PressedColor = new Color(1f, 0.92f, 0.35f);

        ShyButtonLogic logic;
        Camera cam;
        HwToast toast;
        Vector3 bodyHome, sweatHome, sweatPinnedHome;
        float puffT = 1f;
        Vector3 puffAt;
        int dodgeLines, flinchLines, missLines, pinLines;
        bool captured, finishing;
#if UNITY_EDITOR
        const int EditorHoldId = 800, EditorTapId = 801, EditorSecondId = 802;
        bool editorHolding;
#endif

        public override void OnLevelStart()
        {
            cam = Camera.main;
            StopAllCoroutines();
            if (!captured)
            {
                captured = true;
                logic = new ShyButtonLogic(minX, maxX, minY, maxY, button.localPosition.x, button.localPosition.y);
                bodyHome = body.localPosition;
                sweatHome = sweat != null ? sweat.transform.localPosition : Vector3.zero;
                sweatPinnedHome = sweatPinned != null ? sweatPinned.transform.localPosition : Vector3.zero;
            }
            logic.Reset();
            V64AFingers.Reset();
            finishing = false;
            dodgeLines = flinchLines = missLines = pinLines = 0;
            puffT = 1f;
            toast = new HwToast(infoText, infoHolder);
            if (reward != null) reward.ResetReward();
            if (startLabel != null) { startLabel.text = "START"; startLabel.color = StartColor; }
#if UNITY_EDITOR
            editorHolding = false;
#endif
            Pose(0f);
        }

        // ---------- Ciclo di vita ----------

        void OnDisable()
        {
            // pausa/pubblicità/livello spento: dita annullate, il pulsante resta fermo in uno stato valido
            CancelFingers();
        }

        void OnEnable()
        {
            if (!captured || IsFinished || logic == null) return;
            // spento a metà della festa: si rifà il finale da capo (Win una volta sola, la logica fa da guardia)
            if (logic.State == ShyButtonState.Won) { finishing = false; StartCoroutine(Finale()); }
        }

        void OnApplicationPause(bool paused) { if (paused) CancelFingers(); }
        void OnApplicationFocus(bool focus) { if (!focus) CancelFingers(); }

        void CancelFingers()
        {
            V64AFingers.Reset();
#if UNITY_EDITOR
            editorHolding = false;
#endif
            if (logic == null || logic.InputLocked) return;
            logic.Cancel();
            if (button != null) Pose(0f);
        }

        void Update()
        {
            toast?.Tick();
            if (logic == null || IsFinished || finishing || toast == null) return;
            if (Time.timeScale <= 0f) { CancelFingers(); return; }   // pausa o pubblicità: niente input
#if UNITY_EDITOR
            EditorKeys();
            if (finishing) return;
#endif
            ReadFingers();
            if (finishing) return;
            React(logic.Tick(Time.deltaTime));
            Pose(Time.deltaTime);
        }

        void ReadFingers()
        {
            List<V64AFinger> fingers = V64AFingers.Read(cam);
            // prima le dita che scendono, poi quelle che si muovono, poi quelle che salgono
            for (int i = 0; i < fingers.Count && !finishing; i++)
                if (fingers[i].began) React(logic.Down(fingers[i].id, fingers[i].world.x, fingers[i].world.y));
            for (int i = 0; i < fingers.Count && !finishing; i++)
                if (!fingers[i].began && !fingers[i].ended) React(logic.Move(fingers[i].id, fingers[i].world.x, fingers[i].world.y));
            for (int i = 0; i < fingers.Count && !finishing; i++)
                if (fingers[i].ended) React(logic.Up(fingers[i].id));
        }

        void React(ShyButtonEvent e)
        {
            switch (e)
            {
                case ShyButtonEvent.Grabbed:
                    Sfx.Play(this, "tick", 0.5f);
                    break;
                case ShyButtonEvent.Flinch:
                    toast.Show(FlinchLines[flinchLines++ % FlinchLines.Length], 1.1f);
                    StartCoroutine(HalloweenKit.Shake(body, 0.06f, 0.2f));
                    break;
                case ShyButtonEvent.Pinned:
                    toast.Show(PinLines[pinLines++ % PinLines.Length], 1.6f);
                    Sfx.Play(this, "clunk", 0.5f);
                    Haptics.Light();
                    break;
                case ShyButtonEvent.Dodged:
                    toast.Show(logic.TakeHint() ? HintLine : DodgeLines[dodgeLines++ % DodgeLines.Length], logic.Dodges == ShyButtonLogic.HintAfter ? 2.8f : 1.2f);
                    puffAt = new Vector3(logic.FromX, logic.FromY - 0.3f, button.localPosition.z);
                    puffT = 0f;
                    Sfx.Play(this, "whoosh", 0.6f);
                    Haptics.Tap();
                    break;
                case ShyButtonEvent.Missed:
                    toast.Show(MissLines[missLines++ % MissLines.Length], 1.2f);
                    break;
                case ShyButtonEvent.Nudge:
                    toast.Show(NudgeLine, 2.4f);
                    break;
                case ShyButtonEvent.Started:
                    StartCoroutine(Finale());
                    break;
            }
        }

        // ---------- Pose (sempre ricavata dalla logica) ----------

        void Pose(float dt)
        {
            ShyButtonState s = logic.State;
            float t = Time.time;
            // posizione: durante la fuga un arco veloce dal posto di prima a quello nuovo
            Vector3 to = new Vector3(logic.X, logic.Y, button.localPosition.z);
            if (s == ShyButtonState.Dodging)
            {
                float k = logic.DodgeProgress;
                float e = 1f - (1f - k) * (1f - k);
                Vector3 from = new Vector3(logic.FromX, logic.FromY, to.z);
                // il saltello non porta mai il pulsante sotto la riga dell'obiettivo
                float arc = Mathf.Clamp(maxY + 0.15f - Mathf.Max(from.y, to.y), 0f, 0.45f);
                button.localPosition = Vector3.Lerp(from, to, e) + Vector3.up * (Mathf.Sin(k * Mathf.PI) * arc);
            }
            else if (s != ShyButtonState.Won) button.localPosition = to;

            // corpo: respiro nervoso, schiacciato se tenuto, tremante se bloccato, allungato in fuga
            Vector3 scale = Vector3.one;
            Vector3 offset = Vector3.zero;
            switch (s)
            {
                case ShyButtonState.Free:
                    float fidget = 0.012f + 0.01f * logic.Nervous;
                    offset = new Vector3(Mathf.Sin(t * (7f + 3f * logic.Nervous)) * fidget, 0f, 0f);
                    scale = new Vector3(1f, 1f + Mathf.Sin(t * 5f) * 0.015f, 1f);
                    break;
                case ShyButtonState.Holding:
                    scale = new Vector3(1.04f, 0.94f, 1f);
                    offset = new Vector3(Mathf.Sin(t * 40f) * 0.015f, 0f, 0f);
                    break;
                case ShyButtonState.Pinned:
                    scale = new Vector3(1.06f, 0.9f, 1f);
                    offset = new Vector3(Mathf.Sin(t * 63f) * 0.03f, Mathf.Sin(t * 51f) * 0.012f, 0f);
                    break;
                case ShyButtonState.Dodging:
                    scale = new Vector3(0.85f, 1.15f, 1f);
                    break;
            }
            if (s != ShyButtonState.Won)
            {
                body.localPosition = bodyHome + offset;
                body.localScale = scale;
            }

            // facce
            bool won = s == ShyButtonState.Won;
            bool pinned = s == ShyButtonState.Pinned || s == ShyButtonState.Holding;
            bool panic = !won && !pinned && (logic.Nervous >= 1 || s == ShyButtonState.Dodging);
            HalloweenKit.Show(faceWide, won);
            HalloweenKit.Show(facePinned, !won && pinned);
            HalloweenKit.Show(facePanic, panic);
            HalloweenKit.Show(faceNervous, !won && !pinned && !panic);

            // sudore: una goccia che scivola giù in loop
            bool sweaty = !won && logic.Nervous >= 1;
            HalloweenKit.Show(sweat, sweaty);
            if (sweaty && sweat != null) sweat.transform.localPosition = sweatHome + Vector3.down * Mathf.Repeat(t * 0.35f, 0.3f);
            bool pinSweat = !won && s == ShyButtonState.Pinned;
            HalloweenKit.Show(sweatPinned, pinSweat);
            if (pinSweat && sweatPinned != null) sweatPinned.transform.localPosition = sweatPinnedHome + Vector3.down * Mathf.Repeat(t * 0.5f + 0.15f, 0.3f);

            // lo sbuffo al posto lasciato
            if (puff != null)
            {
                if (puffT < 1f)
                {
                    puffT = Mathf.Min(1f, puffT + dt / 0.45f);
                    puff.gameObject.SetActive(true);
                    puff.transform.localPosition = puffAt;
                    puff.transform.localScale = Vector3.one * Mathf.Lerp(1.1f, 2.0f, puffT);
                    HalloweenKit.SetAlpha(puff, 0.9f * (1f - puffT));
                }
                else puff.gameObject.SetActive(false);
            }
        }

        // ---------- Vittoria ----------

        IEnumerator Finale()
        {
            if (finishing) yield break;
            finishing = true;
            V64AFingers.Reset();
            Pose(0f);
            if (puff != null) puff.gameObject.SetActive(false);
            HalloweenKit.Show(sweat, false);
            HalloweenKit.Show(sweatPinned, false);
            body.localPosition = bodyHome;
            body.localScale = Vector3.one;
            // occhi spalancati... un attimo di silenzio... START premuto fino in fondo!
            yield return new WaitForSeconds(0.3f);
            if (startLabel != null) { startLabel.text = "START!"; startLabel.color = PressedColor; }
            Sfx.Play(this, "pop", 0.8f);
            float t = 0f;
            while (t < 0.15f)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / 0.15f);
                body.localScale = new Vector3(Mathf.Lerp(1f, 1.2f, k), Mathf.Lerp(1f, 0.7f, k), 1f);
                yield return null;
            }
            // e poi esplode di gioia (coriandoli e sbuffo grande)
            if (puff != null)
            {
                puff.gameObject.SetActive(true);
                puff.transform.localPosition = button.localPosition;
                puff.transform.localScale = Vector3.one * 2.4f;
                HalloweenKit.SetAlpha(puff, 0.8f);
            }
            t = 0f;
            while (t < 0.35f)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / 0.35f);
                body.localScale = new Vector3(Mathf.Lerp(1.2f, 1f, k), Mathf.Lerp(0.7f, 1f, k), 1f) * (1f + 0.12f * Mathf.Sin(k * Mathf.PI));
                if (puff != null) { puff.transform.localScale = Vector3.one * Mathf.Lerp(2.4f, 3.4f, k); HalloweenKit.SetAlpha(puff, 0.8f * (1f - k)); }
                yield return null;
            }
            if (puff != null) puff.gameObject.SetActive(false);
            body.localScale = Vector3.one;
            if (reward != null) yield return reward.Play(Compliment);
            if (!IsFinished && logic.TryComplete()) Win(Compliment);
        }

#if UNITY_EDITOR
        // ---------- Tasti di prova nell'Editor ----------

        void EditorKeys()
        {
            if (logic.InputLocked) return;
            float x = logic.X, y = logic.Y;
            if (Input.GetKeyDown(KeyCode.T)) { React(logic.Down(EditorTapId, x, y)); React(logic.Up(EditorTapId)); }
            if (Input.GetKeyDown(KeyCode.H))
            {
                if (!editorHolding) { editorHolding = true; React(logic.Down(EditorHoldId, x, y)); }
                else { editorHolding = false; React(logic.Up(EditorHoldId)); }
            }
            if (finishing) return;
            if (Input.GetKeyDown(KeyCode.J)) { React(logic.Down(EditorSecondId, x + 0.4f, y - 0.2f)); if (!finishing) React(logic.Up(EditorSecondId)); }
            if (finishing) return;
            if (Input.GetKeyDown(KeyCode.F)) { React(logic.Down(EditorSecondId, x > 0f ? x - 2.4f : x + 2.4f, y + 1.2f)); React(logic.Up(EditorSecondId)); }
            if (Input.GetKeyDown(KeyCode.G)) { React(logic.Down(EditorTapId, x + 1.45f, y)); React(logic.Up(EditorTapId)); }
            if (Input.GetKeyDown(KeyCode.P)) CancelFingers();
            if (Input.GetKeyDown(KeyCode.R)) OnLevelStart();
        }
#endif
    }
}
