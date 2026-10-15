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

        bool armed, done;
        float charge = 0.06f;
        Camera cam;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            bool plugged = SensorHub.Instance != null && SensorHub.Instance.IsCharging;
            armed = !plugged;   // già in carica: prima va staccato
            if (robotOn != null) robotOn.SetActive(false);
            if (bolt != null) bolt.SetActive(false);
            if (notes != null) notes.Stop();
            if (bubble != null) { bubble.Show("Low... battery..."); StartCoroutine(HideLater(2f)); }
            Show();
        }

        void Update()
        {
            if (done || IsFinished) return;
            bool plugged = SensorHub.Instance != null && SensorHub.Instance.IsCharging;
            if (!plugged) armed = true;
            bool charging = armed && plugged;
            if (bolt != null) bolt.SetActive(charging);

            if (charging)
            {
                charge = Mathf.MoveTowards(charge, 1f, Time.deltaTime / chargeSeconds);
                if (robot != null) robot.localPosition = new Vector3(Mathf.Sin(Time.time * 50f) * 0.02f * charge, robot.localPosition.y, 0f);   // brividi di energia
                if (charge >= 1f) StartCoroutine(PowerOn());
            }
            else
            {
                if (robot != null) robot.localPosition = new Vector3(0f, robot.localPosition.y, 0f);
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

        IEnumerator PowerOn()
        {
            done = true;
            if (robotOff != null) robotOff.SetActive(false);
            if (robotOn != null) robotOn.SetActive(true);
            if (bolt != null) bolt.SetActive(false);
            if (notes != null) notes.Play();
            if (bubble != null) bubble.Show("100%! Let's dance!");
            Vector3 home = robot.localPosition;
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
