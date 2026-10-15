using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;
using Gotcha.Sensors;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 18 — la cassaforte nascosta. Azioni in sequenza:
    /// 1) il quadro è ben fissato: bisogna SCUOTERE il telefono finché non cade (nell'Editor: tasto S);
    /// 2) dietro c'è una cassaforte; il codice è scritto sul post-it dietro al quadro caduto;
    /// 3) tocca la cassaforte, digita il codice sulla tastiera: si apre e dentro c'è la chiave;
    /// 4) trascina la chiave nella serratura della porta.
    /// </summary>
    public class SafeLevel : LevelController
    {
        [SerializeField] Transform portrait;
        [SerializeField] GameObject paintingBack;
        [SerializeField] TextMesh codeNote;
        [SerializeField] Collider2D safeArea;
        [SerializeField] GameObject safeClosed;
        [SerializeField] GameObject safeOpen;
        [SerializeField] GameObject keypad;           // pannello ingrandito con la tastiera
        [SerializeField] Collider2D keypadPanel;      // toccare fuori dal pannello lo chiude
        [SerializeField] TapButton[] digits;          // id = "0".."9"
        [SerializeField] TextMesh display;
        [SerializeField] TextMesh safeDisplay;
        [SerializeField] Draggable2D key;
        [SerializeField] SmokeEmitter dust;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] float shakeNeeded = 0.9f;    // secondi di scossone (cumulati)
        [SerializeField] float shakeStrength = 11f;   // basta uno scossone deciso, non violento

        enum Step { Painting, Code, Key, Done }
        Step step = Step.Painting;
        string code, typed = "";
        float shaken, rattle;
        Camera cam;
        Vector3 portraitPos;
        bool busy;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            code = Random.Range(1, 10).ToString() + Random.Range(0, 10) + Random.Range(0, 10);
            if (codeNote != null) codeNote.text = code;
            if (paintingBack != null) paintingBack.SetActive(false);
            if (safeOpen != null) safeOpen.SetActive(false);
            if (keypad != null) keypad.SetActive(false);
            if (key != null) key.gameObject.SetActive(false);
            if (portrait != null) portraitPos = portrait.localPosition;
            foreach (TapButton d in digits) if (d != null) d.Pressed += OnDigit;
            LockedDoor door = GetComponentInChildren<LockedDoor>(true);
            if (door != null) door.Opened += OnDoorOpened;
            if (bubble != null) bubble.Hide();
            UpdateDisplays();
        }

        void OnDestroy()
        {
            if (digits != null) foreach (TapButton d in digits) if (d != null) d.Pressed -= OnDigit;
        }

        void Update()
        {
            if (IsFinished || busy) return;
            if (step == Step.Painting) UpdatePainting();
            else if (step == Step.Code) UpdateSafe();
        }

        void UpdatePainting()
        {
            bool shaking = SensorHub.Instance != null && SensorHub.Instance.ShakeStrength > shakeStrength;
#if UNITY_EDITOR
            shaking |= Input.GetKey(KeyCode.S);
#endif
            // toccare il quadro: si muove appena, è fissato bene
            if (PointerInput.AnyTouchBegan() && PointerInput.TryGetPrimary(out Vector2 sp, out _, out _))
            {
                Collider2D c = portrait.GetComponent<Collider2D>();
                if (c != null && c.OverlapPoint(PointerInput.ToWorld(cam, sp)))
                {
                    rattle = 0.25f;
                    if (bubble != null) bubble.Show("It's stuck to the wall...");
                }
            }
            if (shaking) { shaken += Time.deltaTime; rattle = 0.15f; }
            rattle -= Time.deltaTime;
            float wobble = rattle > 0f ? Mathf.Sin(Time.time * 50f) * (2f + shaken * 6f) : 0f;
            portrait.localRotation = Quaternion.Euler(0, 0, wobble + shaken * 6f);
            if (shaken >= shakeNeeded) StartCoroutine(PaintingFalls());
        }

        IEnumerator PaintingFalls()
        {
            busy = true;
            if (bubble != null) bubble.Hide();
            Vector3 from = portrait.localPosition;
            float floorY = -2.6f;
            float spin = 0f;
            for (float v = 0f; portrait.localPosition.y > floorY; )
            {
                v += 14f * Time.deltaTime;
                spin += Time.deltaTime * 140f;
                portrait.localPosition += new Vector3(-0.25f * Time.deltaTime, -v * Time.deltaTime, 0f);
                portrait.localRotation = Quaternion.Euler(0, 0, 6f + spin);
                yield return null;
            }
            if (dust != null) { dust.transform.position = new Vector3(portrait.position.x, portrait.position.y - 0.9f, 0f); dust.Play(); }
            // atterra a faccia in giù: si vede il retro col post-it
            portrait.gameObject.SetActive(false);
            if (paintingBack != null) paintingBack.SetActive(true);
            yield return new WaitForSeconds(0.3f);
            if (dust != null) dust.Stop();
            if (bubble != null) bubble.Show("A safe!");
            step = Step.Code;
            busy = false;
        }

        void UpdateSafe()
        {
            if (!PointerInput.AnyTouchBegan() || !PointerInput.TryGetPrimary(out Vector2 sp, out _, out _)) return;
            Vector2 w = PointerInput.ToWorld(cam, sp);
            if (!keypad.activeSelf)
            {
                if (safeArea != null && safeArea.OverlapPoint(w)) { keypad.SetActive(true); if (bubble != null) bubble.Hide(); }
            }
            else if (keypadPanel != null && !keypadPanel.OverlapPoint(w)) keypad.SetActive(false);
        }

        void OnDigit(TapButton b)
        {
            if (step != Step.Code || busy || keypad == null || !keypad.activeSelf) return;
            typed += b.id;
            UpdateDisplays();
            if (typed.Length >= code.Length) StartCoroutine(Check());
        }

        IEnumerator Check()
        {
            busy = true;
            yield return new WaitForSeconds(0.25f);
            if (typed == code)
            {
                SetDisplays("OPEN", new Color(0.36f, 1f, 0.6f));
                yield return new WaitForSeconds(0.5f);
                keypad.SetActive(false);
                if (safeClosed != null) safeClosed.SetActive(false);
                if (safeOpen != null) safeOpen.SetActive(true);
                if (key != null) key.gameObject.SetActive(true);
                step = Step.Key;
            }
            else
            {
                SetDisplays("ERR", new Color(1f, 0.3f, 0.3f));
                yield return new WaitForSeconds(0.6f);
                typed = "";
                UpdateDisplays();
            }
            busy = false;
        }

        void UpdateDisplays()
        {
            string s = typed.PadRight(3, '_');
            SetDisplays(s.Substring(0, 1) + " " + s.Substring(1, 1) + " " + s.Substring(2, 1), new Color(0.36f, 1f, 0.6f));
        }

        void SetDisplays(string text, Color color)
        {
            foreach (TextMesh t in new[] { display, safeDisplay })
                if (t != null) { t.text = text; t.color = color; }
        }

        void OnDoorOpened()
        {
            step = Step.Done;
            Win();
        }
    }
}
