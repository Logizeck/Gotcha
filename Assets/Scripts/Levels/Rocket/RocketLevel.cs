using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 20 — si parte! Sequenza: 1) trascina la tanica sul bocchettone del razzo (si riempie il serbatoio);
    /// 2) alza il coperchio di sicurezza del pulsante (tocco); 3) tieni premuto il pulsante per tutto il conto alla
    /// rovescia 3-2-1: se lo lasci, "ABORT!" e il conto riparte. Senza carburante il razzo fa solo "pfff".
    /// </summary>
    public class RocketLevel : LevelController
    {
        [SerializeField] Transform rocket;
        [SerializeField] DropSlot fuelCap;
        [SerializeField] Transform fuelFill;          // barra del carburante (scala x da 0 a 1)
        [SerializeField] TapButton cover;
        [SerializeField] Transform coverHinge;
        [SerializeField] Collider2D launchButton;
        [SerializeField] Transform launchVisual;
        [SerializeField] TextMesh countdown;
        [SerializeField] Transform flame;
        [SerializeField] SmokeEmitter smoke;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] float countSeconds = 3f;

        bool fueled, coverOpen, launched, holdingBefore;
        float held, pfffCooldown;
        Camera cam;
        Vector3 buttonScale, rocketBase;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            if (rocket != null) rocketBase = rocket.localPosition;
            if (fuelCap != null) fuelCap.Filled += OnFuel;
            if (cover != null) cover.Pressed += OnCover;
            if (fuelFill != null) fuelFill.localScale = new Vector3(0f, 1f, 1f);
            if (flame != null) flame.gameObject.SetActive(false);
            if (launchVisual != null) buttonScale = launchVisual.localScale;
            if (countdown != null) countdown.text = "";
            if (bubble != null) bubble.Hide();
        }

        void OnDestroy()
        {
            if (fuelCap != null) fuelCap.Filled -= OnFuel;
            if (cover != null) cover.Pressed -= OnCover;
        }

        void OnFuel(DropSlot slot, Draggable2D can) { StartCoroutine(Refuel(can)); }

        IEnumerator Refuel(Draggable2D can)
        {
            Transform t = can.transform;
            // la tanica si inclina e versa
            for (float k = 0f; k < 1f; k += Time.deltaTime / 1.4f)
            {
                t.localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(0f, 70f, Mathf.Min(1f, k * 4f)) + Mathf.Sin(k * 40f) * 3f);
                if (fuelFill != null) fuelFill.localScale = new Vector3(k, 1f, 1f);
                yield return null;
            }
            if (fuelFill != null) fuelFill.localScale = Vector3.one;
            t.gameObject.SetActive(false);
            fueled = true;
            if (bubble != null) { bubble.Show("Glug glug!"); StartCoroutine(HideLater(1f)); }
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
            if (bubble != null) bubble.Show("Pfff... no fuel!");
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
            if (bubble != null && !launched) bubble.Hide();
        }
    }
}
