using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 20 — si parte! Ci sono DUE taniche, "H" e "O", e l'indicatore del carburante ha tre finestrelle:
    /// H, H, O (la ricetta è... H2O!). 1) Trascina le taniche sul bocchettone del razzo: ogni travaso è una dose
    /// (2 di H e 1 di O, in qualsiasi ordine). Una dose di troppo (es. due O) e la miscela fa "fizz": l'indicatore
    /// si svuota e si ricomincia. 2) Alza il coperchio di sicurezza del pulsante (tocco); 3) tieni premuto il
    /// pulsante per tutto il conto alla rovescia 3-2-1: se lo lasci, "ABORT!". Senza carburante fa solo "pfff".
    /// </summary>
    public class RocketLevel : LevelController
    {
        [SerializeField] Transform rocket;
        [SerializeField] Collider2D fuelInlet;        // bocchettone sul razzo
        [SerializeField] Draggable2D canH;
        [SerializeField] Draggable2D canO;
        [SerializeField] Transform[] cellFills;       // H, H, O: scala y da 0 a 1, ancorate in basso
        [SerializeField] SpriteRenderer[] cellBars;
        [SerializeField] TextMesh[] cellLetters;
        [SerializeField] SmokeEmitter fizz;           // bollicine della miscela sbagliata
        [SerializeField] TapButton cover;
        [SerializeField] Transform coverHinge;
        [SerializeField] Collider2D launchButton;
        [SerializeField] Transform launchVisual;
        [SerializeField] TextMesh countdown;
        [SerializeField] Transform flame;
        [SerializeField] SmokeEmitter smoke;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] float countSeconds = 3f;

        static readonly Color HColor = new Color(0.25f, 0.66f, 0.96f);
        static readonly Color OColor = new Color(1f, 0.48f, 0.24f);

        bool fueled, coverOpen, launched, holdingBefore, pouring;
        readonly bool[] filled = new bool[3];
        float held, pfffCooldown;
        Camera cam;
        Vector3 buttonScale, rocketBase, homeH, homeO;
        int fizzles;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            if (rocket != null) rocketBase = rocket.localPosition;
            if (cover != null) cover.Pressed += OnCover;
            if (canH != null) { homeH = canH.transform.localPosition; canH.Dropped += OnCanDropped; }
            if (canO != null) { homeO = canO.transform.localPosition; canO.Dropped += OnCanDropped; }
            for (int i = 0; i < 3; i++) SetCell(i, 0f);
            if (fizz != null) fizz.Stop();
            if (flame != null) flame.gameObject.SetActive(false);
            if (launchVisual != null) buttonScale = launchVisual.localScale;
            if (countdown != null) countdown.text = "";
            if (bubble != null) bubble.Hide();
        }

        void OnDestroy()
        {
            if (cover != null) cover.Pressed -= OnCover;
            if (canH != null) canH.Dropped -= OnCanDropped;
            if (canO != null) canO.Dropped -= OnCanDropped;
        }

        /// <summary>Livello della finestrella i (0..1), colore della sua dose e lettera accesa.</summary>
        void SetCell(int i, float level)
        {
            if (cellFills != null && i < cellFills.Length && cellFills[i] != null) cellFills[i].localScale = new Vector3(1f, level, 1f);
            if (cellBars != null && i < cellBars.Length && cellBars[i] != null) cellBars[i].color = i < 2 ? HColor : OColor;
            if (cellLetters != null && i < cellLetters.Length && cellLetters[i] != null)
                cellLetters[i].color = new Color(1f, 1f, 1f, level > 0.5f ? 1f : 0.45f);
        }

        Vector3 Home(Draggable2D can) => can == canH ? homeH : homeO;

        void OnCanDropped(Draggable2D can, DropSlot slot)
        {
            bool onInlet = fuelInlet != null && (fuelInlet.OverlapPoint(can.transform.position)
                || Vector2.Distance(can.transform.position, fuelInlet.transform.position) < 1.0f);
            if (!pouring && !launched && !fueled && onInlet) StartCoroutine(Pour(can));
            else
            {
                if (fueled && onInlet && bubble != null) { bubble.Show("The tank is full!"); StartCoroutine(HideLater(1f)); }
                StartCoroutine(GoHome(can, 0.2f));
            }
        }

        IEnumerator GoHome(Draggable2D can, float seconds)
        {
            can.enabled = false;
            Transform t = can.transform;
            Vector3 from = t.localPosition, to = Home(can);   // coordinate locali: il livello può muoversi
            Quaternion rot = t.localRotation;
            for (float k = 0f; k < 1f; k += Time.deltaTime / seconds)
            {
                float e = Mathf.SmoothStep(0f, 1f, k);
                t.localPosition = Vector3.Lerp(from, to, e) + new Vector3(0f, Mathf.Sin(k * Mathf.PI) * 0.4f, 0f);
                t.localRotation = Quaternion.Slerp(rot, Quaternion.identity, e);
                yield return null;
            }
            t.localPosition = to;
            t.localRotation = Quaternion.identity;
            can.enabled = true;
        }

        /// <summary>Una dose: la tanica si inclina sul bocchettone e la sua finestrella si riempie (o trabocca).</summary>
        IEnumerator Pour(Draggable2D can)
        {
            pouring = true;
            can.enabled = false;
            bool isH = can == canH;
            Transform t = can.transform;
            Vector3 spout = fuelInlet.transform.position + new Vector3(0.55f, 0.45f, 0f);
            Vector3 from = t.position;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.15f) { t.position = Vector3.Lerp(from, spout, k); yield return null; }
            t.position = spout;
            int cell = isH ? (!filled[0] ? 0 : !filled[1] ? 1 : -1) : (!filled[2] ? 2 : -1);
            float lastGlug = 0f;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.9f)
            {
                t.localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(0f, 70f, Mathf.Min(1f, k * 4f)) + Mathf.Sin(k * 40f) * 3f);
                if (cell >= 0) SetCell(cell, Mathf.Clamp01((k - 0.2f) / 0.8f));
                if (k - lastGlug > 0.3f) { lastGlug = k; Sfx.Play(this, "pop", 0.35f); }
                yield return null;
            }
            yield return GoHome(can, 0.3f);
            if (cell >= 0)
            {
                filled[cell] = true;
                SetCell(cell, 1f);
                Haptics.Light();
                if (filled[0] && filled[1] && filled[2])
                {
                    fueled = true;
                    Sfx.Play(this, "ding", 0.7f);
                    if (bubble != null) { bubble.Show("H + H + O = H2O?! It runs on WATER?!"); StartCoroutine(HideLater(2.4f)); }
                }
                else if (bubble != null) { bubble.Show(isH ? "Glug glug! One H." : "Glug glug! One O."); StartCoroutine(HideLater(1f)); }
            }
            else yield return Fizzle(isH);
            pouring = false;
        }

        /// <summary>Miscela sbagliata: bollicine, "fizz" e l'indicatore si svuota.</summary>
        IEnumerator Fizzle(bool tooMuchH)
        {
            fizzles++;
            string[] hLines = { "Pfffzzz! Too much H. Not rocket science... oh wait, it is.", "Fizzle! H, H, H? That's a hiccup, not fuel." };
            string[] oLines = { "Fizzle! Too much O. Now it's just fizzy air.", "Pssshh! O-O... that's a surprised face, not fuel." };
            string[] lines = tooMuchH ? hLines : oLines;
            if (bubble != null) bubble.Show(lines[(fizzles - 1) % lines.Length]);
            Sfx.Play(this, "buzz", 0.6f);
            Haptics.Medium();
            if (fizz != null) fizz.Play();
            float[] start = new float[3];
            for (int i = 0; i < 3; i++) start[i] = cellFills != null && i < cellFills.Length && cellFills[i] != null ? cellFills[i].localScale.y : 0f;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 1.1f)
            {
                for (int i = 0; i < 3; i++)
                {
                    SetCell(i, start[i] * (1f - k));
                    if (cellBars != null && i < cellBars.Length && cellBars[i] != null)
                        cellBars[i].color = Mathf.Repeat(k * 6f, 1f) < 0.5f ? new Color(0.55f, 0.9f, 0.4f) : (i < 2 ? HColor : OColor);   // ribolle verdognolo
                }
                if (rocket != null) rocket.localPosition = rocketBase + new Vector3(Mathf.Sin(Time.time * 45f) * 0.02f * (1f - k), 0f, 0f);
                yield return null;
            }
            if (rocket != null) rocket.localPosition = rocketBase;
            if (fizz != null) fizz.Stop();
            for (int i = 0; i < 3; i++) { filled[i] = false; SetCell(i, 0f); }
            yield return new WaitForSeconds(0.8f);
            if (bubble != null && !launched && !pouring) bubble.Show("Recipe: H, H, O. Try again!");
            StartCoroutine(HideLater(1.6f));
        }

        void OnCover(TapButton b)
        {
            if (coverOpen) return;
            coverOpen = true;
            cover.interactable = false;
            StartCoroutine(FlipCover());
        }

        IEnumerator FlipCover()
        {
            Collider2D c = cover.GetComponent<Collider2D>();
            if (c != null) c.enabled = false;   // il pulsante sotto ora si può premere
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.25f)
            {
                if (coverHinge != null) coverHinge.localScale = new Vector3(1f, Mathf.Lerp(1f, -0.35f, k), 1f);   // si ribalta all'indietro
                yield return null;
            }
            if (coverHinge != null) coverHinge.localScale = new Vector3(1f, -0.35f, 1f);
            Haptics.Light();
        }

        void Update()
        {
            if (launched || IsFinished) return;
            pfffCooldown -= Time.deltaTime;
            bool holding = coverOpen && PointerInput.IsHeld(cam, launchButton);
#if UNITY_EDITOR
            holding |= coverOpen && Input.GetKey(KeyCode.Space);
#endif
            if (launchVisual != null)
            {
                Vector3 target = holding ? new Vector3(buttonScale.x, buttonScale.y * 0.8f, buttonScale.z) : buttonScale;
                launchVisual.localScale = Vector3.Lerp(launchVisual.localScale, target, 25f * Time.deltaTime);
            }

            if (holding && !fueled)
            {
                if (pfffCooldown <= 0f) StartCoroutine(Pfff());
                holdingBefore = holding;
                return;
            }
            if (holding)
            {
                held += Time.deltaTime;
                float left = countSeconds - held;
                if (countdown != null) { countdown.text = Mathf.CeilToInt(left).ToString(); countdown.color = new Color(1f, 0.9f, 0.4f); }
                if (rocket != null) rocket.localPosition = rocketBase + new Vector3(Mathf.Sin(Time.time * 60f) * 0.015f * held, 0f, 0f);
                if (left <= 0f) StartCoroutine(Launch());
            }
            else if (holdingBefore && fueled && held > 0f)
            {
                held = 0f;
                if (countdown != null) { countdown.text = "ABORT!"; countdown.color = new Color(1f, 0.35f, 0.35f); }
                if (rocket != null) rocket.localPosition = rocketBase;
            }
            holdingBefore = holding;
        }

        IEnumerator Pfff()
        {
            pfffCooldown = 1.5f;
            bool some = filled[0] || filled[1] || filled[2];
            if (bubble != null) bubble.Show(some ? "Pfff... the recipe says H, H, O!" : "Pfff... no fuel!");
            Haptics.Light();
            if (smoke != null) smoke.Play();
            yield return new WaitForSeconds(0.3f);
            if (smoke != null) smoke.Stop();
            yield return new WaitForSeconds(0.8f);
            if (bubble != null && !launched) bubble.Hide();
        }

        IEnumerator Launch()
        {
            Haptics.Heavy();
            launched = true;
            if (countdown != null) { countdown.text = "LIFT OFF!"; countdown.color = new Color(0.5f, 1f, 0.55f); }
            if (flame != null) flame.gameObject.SetActive(true);
            if (smoke != null) { smoke.puffsPerSecond *= 2f; smoke.Play(); }
            Vector3 start = rocketBase;
            // un attimo di rombo sulla rampa, poi su accelerando
            for (float k = 0f; k < 0.6f; k += Time.deltaTime)
            {
                rocket.localPosition = start + new Vector3(Mathf.Sin(Time.time * 70f) * 0.03f, 0f, 0f);
                if (flame != null) flame.localScale = new Vector3(1f, 0.8f + Mathf.PingPong(Time.time * 8f, 0.5f), 1f);
                yield return null;
            }
            float v = 0f;
            while (rocket.localPosition.y < 12f)
            {
                v += 9f * Time.deltaTime;
                rocket.localPosition += new Vector3(0f, v * Time.deltaTime, 0f);
                if (flame != null) flame.localScale = new Vector3(1f, 1.1f + Mathf.PingPong(Time.time * 10f, 0.6f), 1f);
                if (rocket.localPosition.y > start.y + 2f && smoke != null) smoke.Stop();
                yield return null;
            }
            yield return new WaitForSeconds(0.3f);
            Win();
        }

        IEnumerator HideLater(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (bubble != null && !launched && !pouring) bubble.Hide();
        }
    }
}
