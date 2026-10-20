using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "Upside-down code" — sulla cassaforte il codice è stampato capovolto (cifre 0,1,6,8,9: a telefono dritto
    /// si legge un numero sbagliato, i 6 sembrano 9). Soluzione: capovolgere il telefono (rilevato dall'inclinazione: il
    /// codice si illumina e diventa leggibile), leggerlo e digitarlo sul tastierino. L'interfaccia non ruota. Tre codici
    /// sbagliati: allarme e sconfitta. Senza accelerometro compare un pulsante "FLIP VIEW" da tenere premuto.
    /// </summary>
    public class UpsideDownCodeLevel : S41ILevelBase
    {
        [SerializeField] Transform plate, safe;
        [SerializeField] TextMesh codeText, display, triesText;
        [SerializeField] Collider2D[] keys;            // 0..9, poi CLR (10) e OK (11)
        [SerializeField] SpriteRenderer safeArt, glow;
        [SerializeField] Sprite safeOpen;
        [SerializeField] GameObject flipButton;
        [SerializeField] Collider2D flipArea;

        static readonly string[] codes = { "6819", "1986", "9061", "6098", "8916", "1069" };
        string code, typed = "";
        int wrong;
        bool fallback;
        float flipped;

        protected override void Begin()
        {
            code = codes[Random.Range(0, codes.Length)];
            codeText.text = code;
            display.text = "____";
            triesText.text = "WRONG CODES: " + Marks(0, 3);
            fallback = !SystemInfo.supportsAccelerometer;
#if UNITY_EDITOR
            fallback = false;
#endif
            if (flipButton != null) flipButton.SetActive(fallback);
            Say("Enter the code!", 2f);
        }

        bool UpsideDown()
        {
            if (fallback) return PointerInput.IsHeld(cam, flipArea);
            return Inputs.Tilt().y > 0.7f;
        }

        protected override void Tick()
        {
            // a telefono capovolto il codice si illumina (leggibile solo così)
            bool down = UpsideDown();
            flipped = Mathf.MoveTowards(flipped, down ? 1f : 0f, Time.deltaTime * 3f);
            codeText.color = Color.Lerp(new Color(0.45f, 0.75f, 0.5f, 0.55f), new Color(0.55f, 1f, 0.6f, 1f), flipped);
            Alpha(glow, flipped * 0.8f);
            if (fallback) plate.localRotation = Quaternion.Euler(0, 0, 180f * flipped);

            if (busy || !Pointer(out Vector2 w, out bool began, out _) || !began) return;
            for (int i = 0; i < keys.Length; i++)
            {
                if (keys[i] == null || !keys[i].OverlapPoint(w)) continue;
                StartCoroutine(Pop(keys[i].transform, 0.85f, 0.12f));
                Sfx.Play(this, "tick", 0.7f);
                Haptics.Light();
                if (i <= 9) { if (typed.Length < 4) typed += i.ToString(); }
                else if (i == 10) typed = "";
                else if (typed.Length > 0) StartCoroutine(Check());
                display.text = typed.PadRight(4, '_');
                if (i <= 9 && typed.Length == 4) StartCoroutine(Check());
                return;
            }
        }

        IEnumerator Check()
        {
            busy = true;
            yield return new WaitForSeconds(0.25f);
            if (typed == code)
            {
                display.text = "OPEN";
                Sfx.Play(this, "clunk", 0.9f);
                Haptics.Medium();
                yield return StartCoroutine(Shake(safe, 0.06f, 0.3f));
                if (safeArt != null && safeOpen != null) safeArt.sprite = safeOpen;
                plate.gameObject.SetActive(false);
                Say("Read it the right way up!", 0f);
                yield return StartCoroutine(Celebrate(1.8f));
                yield break;
            }
            wrong++;
            triesText.text = "WRONG CODES: " + Marks(wrong, 3);
            display.text = "NOPE";
            Sfx.Play(this, "buzz", 0.8f);
            Haptics.Medium();
            yield return StartCoroutine(Shake(display.transform, 0.1f, 0.4f));
            if (wrong >= 3)
            {
                display.text = "ALARM!";
                Say("WEE-OOO! WEE-OOO!", 0f);
                Haptics.Heavy();
                for (int i = 0; i < 6; i++) { Sfx.Play(this, "buzz", 1f); yield return new WaitForSeconds(0.25f); }
                Fail();
                yield break;
            }
            Say(wrong == 1 ? "Wrong! Are you reading it right?" : "Still wrong. Look at it from another angle!", 2.4f);
            typed = "";
            display.text = "____";
            busy = false;
        }
    }
}
