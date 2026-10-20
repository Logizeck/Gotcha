using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "Cold coffee" — un impiegato è furioso: il suo caffè è congelato. Soluzione: trascina la tazza sotto la
    /// lampada, poi tieni premuto il pulsante HEAT; il termometro sale con inerzia (la lampada resta calda anche dopo il
    /// rilascio): smetti in tempo perché la temperatura si fermi nella zona verde. Sconfitta: oltre il massimo il caffè
    /// trabocca.
    /// </summary>
    public class ColdCoffeeLevel : LevelController
    {
        [SerializeField] Transform cup, lampSpot, heatButton, thermoFill, worker;
        [SerializeField] Collider2D cupArea, heatArea;
        [SerializeField] SpriteRenderer cupArt, workerArt, cone, bulbGlow, spill;
        [SerializeField] Sprite hotCupSprite, happyWorkerSprite;
        [SerializeField] SmokeEmitter steam;
        [SerializeField] TextMesh tempText;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] SmokeEmitter[] confetti;
        [SerializeField] float greenMin = 58f, greenMax = 75f, heatUp = 1.2f, heatDown = 0.65f, heatRate = 30f, coolRate = 2f;
        [SerializeField] float snapDistance = 0.9f;

        Camera cam;
        Vector3 cupHome;
        Vector2 grab;
        bool dragging, underLamp, busy, melted, warnedAway, warnedHot;
        float heat, temp, settle, clock;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            cupHome = cup.localPosition;
            S41EKit.Confetti(confetti, false);
            if (steam != null) steam.Stop();
            if (spill != null) spill.gameObject.SetActive(false);
            S41EKit.SetAlpha(cone, 0f);
            S41EKit.SetAlpha(bulbGlow, 0f);
            S41EKit.Say(bubble, "MY COFFEE IS FROZEN SOLID!");
            UpdateThermo();
        }

        void UpdateThermo()
        {
            thermoFill.localScale = new Vector3(1f, Mathf.Clamp01(temp / 100f), 1f);
            if (tempText != null) tempText.text = Mathf.RoundToInt(temp) + "°";
        }

        void Update()
        {
            if (IsFinished || busy) return;
            float dt = Time.deltaTime;
            clock += dt;
            bool holding = PointerInput.IsHeld(cam, heatArea) && !dragging;
#if UNITY_EDITOR
            if (Input.GetKey(KeyCode.H)) holding = true;
#endif
            HandleDrag(holding);
            heatButton.localScale = Vector3.one * (holding ? 0.9f : 1f);

            // la lampada si scalda e si raffredda lentamente: è lei a dare l'inerzia
            heat = Mathf.Clamp01(heat + (holding ? heatUp : -heatDown) * dt);
            S41EKit.SetAlpha(cone, heat * 0.9f);
            S41EKit.SetAlpha(bulbGlow, heat);
            if (holding && !underLamp && !warnedAway) { warnedAway = true; S41EKit.Say(bubble, "You're heating the AIR! The cup is over here!"); }

            if (underLamp) temp += heat * heatRate * dt;
            if (heat < 0.05f && temp > 0f) temp = Mathf.Max(0f, temp - coolRate * dt);
            UpdateThermo();

            if (!melted && temp > 35f)
            {
                melted = true;
                cupArt.sprite = hotCupSprite;
                Sfx.Play(this, "pop", 0.7f);
                S41EKit.Say(bubble, "It's melting! Keep going...");
            }
            if (steam != null)
            {
                if (temp > 50f && !steam.gameObject.activeSelf) steam.gameObject.SetActive(true);
                if (temp > 50f) steam.Play(); else steam.Stop();
            }
            if (temp > 85f)
            {
                cup.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(clock * 50f) * 3f);
                if (!warnedHot) { warnedHot = true; S41EKit.Say(bubble, "Too hot! TOO HOT!"); }
            }
            else cup.localRotation = Quaternion.identity;

            if (temp >= 100f) { StartCoroutine(Overflow()); return; }

            bool inGreen = temp >= greenMin && temp <= greenMax;
            if (inGreen && !holding && heat < 0.05f && underLamp && !dragging) settle += dt; else settle = 0f;
            if (settle > 0.8f) StartCoroutine(Perfect());
        }

        void HandleDrag(bool holding)
        {
            if (!S41EKit.Pointer(cam, out Vector2 w, out bool began, out bool ended))
            {
                if (dragging) Drop();
                return;
            }
            if (began && !holding && cupArea.OverlapPoint(w))
            {
                dragging = true;
                underLamp = false;
                grab = (Vector2)cup.position - w;
                Sfx.Play(this, "tick", 0.5f);
            }
            if (dragging)
            {
                Vector2 p = w + grab;
                cup.position = new Vector3(Mathf.Clamp(p.x, -2.5f, 2.5f), Mathf.Clamp(p.y, -1.85f, 2.5f), cup.position.z);
                if (ended) Drop();
            }
        }

        void Drop()
        {
            dragging = false;
            if (Vector2.Distance(cup.position, lampSpot.position) < snapDistance)
            {
                underLamp = true;
                StartCoroutine(S41EKit.Move(cup, cup.parent.InverseTransformPoint(lampSpot.position), 0.15f));
                Sfx.Play(this, "clunk", 0.6f);
                S41EKit.Say(bubble, "Under the lamp? ...Interesting.");
            }
            else
            {
                StartCoroutine(S41EKit.Move(cup, cupHome, 0.25f));
            }
        }

        IEnumerator Perfect()
        {
            busy = true;
            S41EKit.SetAlpha(cone, 0f);
            S41EKit.SetAlpha(bulbGlow, 0f);
            Sfx.Play(this, "ding", 0.8f);
            if (happyWorkerSprite != null) workerArt.sprite = happyWorkerSprite;
            S41EKit.Say(bubble, "AAAH. PERFECT TEMPERATURE!");
            yield return StartCoroutine(S41EKit.Pop(worker, 1.08f, 0.35f));
            yield return StartCoroutine(S41EKit.Move(cup, cup.localPosition + Vector3.up * 0.4f, 0.4f, 0.3f));
            S41EKit.Confetti(confetti, true);
            yield return new WaitForSeconds(1.8f);
            S41EKit.Confetti(confetti, false);
            Win();
        }

        IEnumerator Overflow()
        {
            busy = true;
            S41EKit.Say(bubble, "IT'S BOILING OVER! MY DESK!");
            Haptics.Medium();
            Sfx.Play(this, "buzz", 0.8f);
            if (spill != null)
            {
                spill.gameObject.SetActive(true);
                Transform s = spill.transform;
                Vector3 full = s.localScale;
                for (float k = 0f; k < 1f; k += Time.deltaTime / 0.6f) { s.localScale = new Vector3(full.x * k, full.y, 1f); yield return null; }
                s.localScale = full;
            }
            yield return StartCoroutine(S41EKit.Shake(worker, 0.12f, 0.6f));
            yield return new WaitForSeconds(0.8f);
            Fail();
        }
    }
}
