using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 20 — si parte! Ci sono DUE taniche, "H" e "O", e l'indicatore del carburante ha tre finestrelle
    /// vuote: la ricetta non è scritta da nessuna parte (è... H2O!). 1) Trascina le taniche sul bocchettone del razzo:
    /// ogni travaso è una dose e riempie la finestrella successiva con la sua lettera; dopo tre dosi il serbatoio è
    /// pieno, qualunque cosa ci sia dentro. 2) Alza il coperchio di sicurezza del pulsante (tocco); 3) tieni premuto il
    /// pulsante per tutto il conto alla rovescia 3-2-1: se lo lasci, "ABORT!". Al decollo, con H, H, O il razzo parte;
    /// con qualsiasi altra miscela (es. una H e due O) ESPLODE e il livello è perso. Senza carburante fa solo "pfff".
    /// </summary>
    public class RocketLevel : LevelController
    {
        [SerializeField] Transform rocket;
        [SerializeField] Collider2D fuelInlet;        // bocchettone sul razzo
        [SerializeField] Draggable2D canH;
        [SerializeField] Draggable2D canO;
        [SerializeField] Transform[] cellFills;       // tre dosi in ordine di travaso: scala y da 0 a 1, ancorate in basso
        [SerializeField] SpriteRenderer[] cellBars;
        [SerializeField] TextMesh[] cellLetters;
        [SerializeField] SmokeEmitter fizz;           // bollicine sul bocchettone quando la miscela è sbagliata (sfrigola)
        [SerializeField] Transform boom;              // esplosione (nascosta finché non serve)
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
        readonly char[] dose = new char[3];   // 'H', 'O' o 0 (vuota), in ordine di travaso
        int doses;
        bool RightMix => doses == 3 && System.Array.FindAll(dose, c => c == 'H').Length == 2;
        float held, pfffCooldown;
        Camera cam;
        Vector3 buttonScale, rocketBase, homeH, homeO;

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
            if (boom != null) boom.gameObject.SetActive(false);
        }

        void OnDestroy()
        {
            if (cover != null) cover.Pressed -= OnCover;
            if (canH != null) canH.Dropped -= OnCanDropped;
            if (canO != null) canO.Dropped -= OnCanDropped;
        }

        /// <summary>Livello della finestrella i (0..1), colore e lettera della dose che contiene.</summary>
        void SetCell(int i, float level)
        {
            char c = dose[i];
            if (cellFills != null && i < cellFills.Length && cellFills[i] != null) cellFills[i].localScale = new Vector3(1f, level, 1f);
            if (cellBars != null && i < cellBars.Length && cellBars[i] != null) cellBars[i].color = c == 'O' ? OColor : HColor;
            // le finestrelle sono vuote: la lettera compare solo quando la dose è dentro (la ricetta va indovinata)
            if (cellLetters != null && i < cellLetters.Length && cellLetters[i] != null)
            {
                cellLetters[i].text = level > 0.5f && c != 0 ? c.ToString() : "";
                cellLetters[i].color = Color.white;
            }
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

        /// <summary>Una dose: la tanica si inclina sul bocchettone e la finestrella successiva si riempie.</summary>
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
            int cell = doses;
            dose[cell] = isH ? 'H' : 'O';
            float lastGlug = 0f;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.9f)
            {
                t.localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(0f, 70f, Mathf.Min(1f, k * 4f)) + Mathf.Sin(k * 40f) * 3f);
                SetCell(cell, Mathf.Clamp01((k - 0.2f) / 0.8f));
                if (k - lastGlug > 0.3f) { lastGlug = k; Sfx.Play(this, "pop", 0.35f); }
                yield return null;
            }
            yield return GoHome(can, 0.3f);
            doses++;
            SetCell(cell, 1f);
            Haptics.Light();
            if (doses == 3)
            {
                fueled = true;
                Sfx.Play(this, "ding", 0.7f);
                // non si dice se la miscela è giusta: lo scopre il decollo. Se è sbagliata, il bocchettone sfrigola appena
                if (!RightMix && fizz != null) fizz.Play();
                if (bubble != null) { bubble.Show("Tank full! Ready for launch?"); StartCoroutine(HideLater(1.6f)); }
            }
            else if (bubble != null) { bubble.Show(isH ? "Glug glug! One H." : "Glug glug! One O."); StartCoroutine(HideLater(1f)); }
            pouring = false;
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
                if (left <= 0f) StartCoroutine(RightMix ? Launch() : Explode());
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
            bool some = doses > 0;
            if (bubble != null) bubble.Show(some ? "Pfff... not enough fuel!" : "Pfff... no fuel!");
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
            if (bubble != null) bubble.Show("H + H + O = H2O?! It runs on WATER?!");
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

        /// <summary>Miscela sbagliata: il razzo trema, si gonfia e... KABOOM. Livello perso.</summary>
        IEnumerator Explode()
        {
            launched = true;
            if (countdown != null) { countdown.text = "UH-OH..."; countdown.color = new Color(1f, 0.35f, 0.35f); }
            if (bubble != null) bubble.Show(System.Array.FindAll(dose, c => c == 'O').Length > 1 ? "Too much O! That's not fuel..." : "Too much H! That's not fuel...");
            if (flame != null) flame.gameObject.SetActive(true);
            // sobbalza sulla rampa sempre più forte e si gonfia
            Vector3 scale = rocket.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 1.1f)
            {
                rocket.localPosition = rocketBase + new Vector3(Mathf.Sin(Time.time * 70f) * 0.05f * k, Random.Range(0f, 0.08f) * k, 0f);
                rocket.localScale = new Vector3(scale.x * (1f + 0.15f * k), scale.y * (1f + 0.08f * k), scale.z);
                if (k > 0.5f) Haptics.Pulse(30, 120);
                yield return null;
            }
            Haptics.Heavy();
            Sfx.Play(this, "clunk", 1f);
            Sfx.Play(this, "whoosh", 1f);
            if (countdown != null) { countdown.text = "KABOOM!"; countdown.color = new Color(1f, 0.55f, 0.15f); }
            if (bubble != null) bubble.Show("KABOOM! *cough* Wrong mix...");
            rocket.localScale = scale;
            rocket.gameObject.SetActive(false);
            if (fizz != null) fizz.Stop();
            if (smoke != null) { smoke.transform.position = rocket.position; smoke.puffsPerSecond = 40f; smoke.startScale = 1.4f; smoke.Play(); }
            if (boom != null)
            {
                boom.position = new Vector3(rocket.position.x, rocket.position.y, boom.position.z);
                boom.gameObject.SetActive(true);
                Vector3 bs = boom.localScale;
                for (float k = 0f; k < 1f; k += Time.deltaTime / 0.35f) { boom.localScale = bs * Mathf.Lerp(0.2f, 1.2f, k); yield return null; }
                boom.localScale = bs;
            }
            yield return new WaitForSeconds(1.2f);
            if (smoke != null) smoke.Stop();
            Fail();
        }

        IEnumerator HideLater(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (bubble != null && !launched && !pouring) bubble.Hide();
        }
    }
}
