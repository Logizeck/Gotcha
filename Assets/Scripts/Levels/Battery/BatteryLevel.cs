using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;
using Gotcha.Sensors;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 21 — batteria scarica. Il robottino è spento: "low battery...". Si risolve collegando DAVVERO il
    /// telefono al caricatore. Se il telefono era già in carica all'inizio, bisogna staccarlo e riattaccarlo
    /// (deve esserci un vero "attacco" durante il livello). Nell'Editor il caricatore si simula col tasto C.
    /// Senza caricatore a portata di mano: "NO CHARGER?" fa comparire una dinamo a manovella da girare col dito.
    /// Funziona, ma il robot si accende col muso lungo (la corrente vera è un'altra cosa).
    /// </summary>
    public class BatteryLevel : LevelController
    {
        [SerializeField] GameObject robotOff;
        [SerializeField] GameObject robotOn;
        [SerializeField] Transform robot;
        [SerializeField] Transform batteryFill;      // scala x da 0 a 1, ancorata a sinistra
        [SerializeField] SpriteRenderer batteryBar;
        [SerializeField] SpriteRenderer batteryFrame;
        [SerializeField] GameObject bolt;
        [SerializeField] SmokeEmitter notes;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] float chargeSeconds = 2.5f;
        [Header("Senza caricatore: dinamo a manovella")]
        [SerializeField] TapButton skipButton;
        [SerializeField] GameObject crank;
        [SerializeField] Transform crankArm;
        [SerializeField] float crankRadius = 1.4f;
        [SerializeField] float turnsNeeded = 6f;
        [SerializeField] GameObject robotSad;

        bool armed, done, cranking, cranked;
        float lastAngle;
        bool turning;
        float charge = 0.06f;
        Camera cam;
        Vector3 robotHome;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            if (robot != null) robotHome = robot.localPosition;
            bool plugged = SensorHub.Instance != null && SensorHub.Instance.IsCharging;
            armed = !plugged;   // già in carica: prima va staccato
            if (robotOn != null) robotOn.SetActive(false);
            if (robotSad != null) robotSad.SetActive(false);
            if (crank != null) crank.SetActive(false);
            if (skipButton != null) skipButton.Pressed += OnSkip;
            if (bolt != null) bolt.SetActive(false);
            if (notes != null) notes.Stop();
            if (bubble != null) { bubble.Show("Low... battery..."); StartCoroutine(HideLater(2f)); }
            Show();
        }

        void OnDestroy() { if (skipButton != null) skipButton.Pressed -= OnSkip; }

        void OnSkip(TapButton b)
        {
            if (done || cranking) return;
            cranking = true;
            skipButton.gameObject.SetActive(false);
            if (crank != null) crank.SetActive(true);
            if (bubble != null) { bubble.Show("Spin it! Spin it!"); StartCoroutine(HideLater(1.5f)); }
        }

        /// <summary>Girando la manovella col dito: ogni giro carica un po' (in qualsiasi verso).</summary>
        void Crank()
        {
            if (!cranking || crank == null) return;
            Vector2 c = crank.transform.position;
            if (PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out bool ended) && !ended)
            {
                Vector2 w = PointerInput.ToWorld(cam, sp) - c;
                if (w.magnitude < crankRadius && w.magnitude > 0.15f)
                {
                    float a = Mathf.Atan2(w.y, w.x) * Mathf.Rad2Deg;
                    if (turning && !began)
                    {
                        float d = Mathf.DeltaAngle(lastAngle, a);
                        charge = Mathf.Min(1f, charge + Mathf.Abs(d) / (360f * turnsNeeded));
                        if (Mathf.Abs(d) > 0.5f) cranked = true;
                    }
                    turning = true;
                    lastAngle = a;
                    if (crankArm != null) crankArm.rotation = Quaternion.Euler(0, 0, a);
                }
                else turning = false;
            }
            else turning = false;
            if (charge >= 1f && cranked) StartCoroutine(PowerOn(true));
        }

        void Update()
        {
            if (done || IsFinished) return;
            Crank();
            if (done) return;
            bool plugged = SensorHub.Instance != null && SensorHub.Instance.IsCharging;
            if (!plugged) armed = true;
            bool charging = armed && plugged;
            if (bolt != null) bolt.SetActive(charging);

            if (charging)
            {
                charge = Mathf.MoveTowards(charge, 1f, Time.deltaTime / chargeSeconds);
                if (robot != null) robot.localPosition = robotHome + new Vector3(Mathf.Sin(Time.time * 50f) * 0.02f * charge, 0f, 0f);   // brividi di energia
                if (charge >= 1f) StartCoroutine(PowerOn(false));
            }
            else
            {
                if (robot != null) robot.localPosition = robotHome;
                // toccare il robot spento: russa
                if (PointerInput.AnyTouchBegan() && PointerInput.TryGetPrimary(out Vector2 sp, out _, out _) && robot != null)
                {
                    Collider2D c = robot.GetComponent<Collider2D>();
                    if (c != null && c.OverlapPoint(PointerInput.ToWorld(cam, sp)) && bubble != null) { bubble.Show("Need... energy... zzz"); StartCoroutine(HideLater(1.4f)); }
                }
            }
            Show();
        }

        void Show()
        {
            if (batteryFill != null) batteryFill.localScale = new Vector3(charge, 1f, 1f);
            if (batteryBar != null) batteryBar.color = charge < 0.25f ? new Color(1f, 0.25f, 0.25f) : charge < 0.6f ? new Color(1f, 0.82f, 0.2f) : new Color(0.36f, 1f, 0.45f);
            if (batteryFrame != null)
            {
                bool blink = charge < 0.25f && Mathf.Repeat(Time.time * 2f, 1f) < 0.5f;
                batteryFrame.color = blink ? new Color(1f, 0.4f, 0.4f) : Color.white;
            }
        }

        IEnumerator PowerOn(bool byHand)
        {
            done = true;
            if (robotOff != null) robotOff.SetActive(false);
            if (bolt != null) bolt.SetActive(false);
            if (skipButton != null) skipButton.gameObject.SetActive(false);
            if (byHand)
            {
                // acceso a manovella: funziona, ma che tristezza
                if (robotSad != null) robotSad.SetActive(true);
                if (bubble != null) bubble.Show("Meh... tastes like hamster wheel. :(");
                Vector3 h = robot.localPosition;
                for (float k = 0f; k < 1.8f; k += Time.deltaTime)
                {
                    robot.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(k * 2.5f) * 3f);   // dondola sconsolato
                    yield return null;
                }
                robot.localRotation = Quaternion.identity;
                robot.localPosition = h;
                Win();
                yield break;
            }
            if (robotOn != null) robotOn.SetActive(true);
            if (notes != null) notes.Play();
            if (bubble != null) bubble.Show("100%! Let's dance!");
            Vector3 home = robotHome;
            for (float k = 0f; k < 1.6f; k += Time.deltaTime)
            {
                robot.localPosition = home + new Vector3(Mathf.Sin(k * 9f) * 0.15f, Mathf.Abs(Mathf.Sin(k * 9f)) * 0.25f, 0f);
                robot.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(k * 9f) * 8f);
                yield return null;
            }
            robot.localPosition = home;
            robot.localRotation = Quaternion.identity;
            if (notes != null) notes.Stop();
            Win();
        }

        IEnumerator HideLater(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (bubble != null && !done) bubble.Hide();
        }
    }
}
