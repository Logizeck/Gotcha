using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Sensors;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 22 — zuppa bollente. Il topolino ha fame, ma la zuppa scotta (il termometro è al massimo).
    /// Prima si soffia nel microfono per raffreddarla (il vapore cala), POI si trascina la scodella al topolino.
    /// Se gliela dai ancora calda: "HOT HOT HOT!" e la scodella torna indietro. (Editor: tasto B per soffiare.)
    /// </summary>
    public class SoupLevel : LevelController
    {
        [SerializeField] Draggable2D bowl;
        [SerializeField] SmokeEmitter steam;
        [SerializeField] Transform mercury;          // scala y da 0 a 1, ancorata in basso
        [SerializeField] SpriteRenderer mercuryBar;
        [SerializeField] Transform mouse;
        [SerializeField] SmokeEmitter hearts;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] float blowSeconds = 2.5f;
        [SerializeField] float blowThreshold = 0.25f;
        [SerializeField] float feedRadius = 1.1f;
        [SerializeField] float cool = 0.15f;

        float temp = 1f;
        Vector3 bowlHome;
        bool done, busy;

        public override void OnLevelStart()
        {
            if (SensorHub.Instance != null) SensorHub.Instance.EnableMic();
            bowlHome = bowl.transform.position;
            bowl.Dropped += OnDropped;
            if (steam != null) steam.Play();
            if (hearts != null) hearts.Stop();
            if (bubble != null) { bubble.Show("Soup! Soup!"); StartCoroutine(HideLater(1.5f)); }
        }

        void OnDestroy()
        {
            if (bowl != null) bowl.Dropped -= OnDropped;
            if (SensorHub.Instance != null) SensorHub.Instance.DisableMic();
        }

        void Update()
        {
            if (done || IsFinished) return;
            float level = SensorHub.Instance != null ? SensorHub.Instance.MicLevel01 : 0f;
#if UNITY_EDITOR
            if (Input.GetKey(KeyCode.B)) level = 1f;
#endif
            if (level > blowThreshold) temp = Mathf.Max(0f, temp - Time.deltaTime / blowSeconds);
            if (steam != null)
            {
                steam.puffsPerSecond = Mathf.Lerp(0.5f, 9f, temp);
                if (temp < 0.05f) steam.Stop(); else steam.Play();
            }
            if (mercury != null) mercury.localScale = new Vector3(1f, Mathf.Lerp(0.08f, 1f, temp), 1f);
            if (mercuryBar != null) mercuryBar.color = Color.Lerp(new Color(0.3f, 0.6f, 1f), new Color(1f, 0.2f, 0.2f), temp);
        }

        void OnDropped(Draggable2D d, DropSlot slot)
        {
            if (done || busy) return;
            if (Vector2.Distance(d.transform.position, mouse.position) > feedRadius) { StartCoroutine(SlideHome()); return; }
            if (temp > cool) StartCoroutine(TooHot());
            else StartCoroutine(Eat());
        }

        IEnumerator SlideHome()
        {
            busy = true;
            Transform t = bowl.transform;
            Vector3 from = t.position;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.25f) { t.position = Vector3.Lerp(from, bowlHome, k); yield return null; }
            t.position = bowlHome;
            busy = false;
        }

        IEnumerator TooHot()
        {
            busy = true;
            if (bubble != null) bubble.Show("HOT HOT HOT!");
            Vector3 m = mouse.localPosition;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.45f)
            {
                mouse.localPosition = m + Vector3.up * Mathf.Sin(k * Mathf.PI) * 0.8f;
                yield return null;
            }
            mouse.localPosition = m;
            busy = false;
            yield return SlideHome();
            yield return new WaitForSeconds(0.6f);
            if (bubble != null && !done) bubble.Hide();
        }

        IEnumerator Eat()
        {
            done = true;
            bowl.enabled = false;
            if (hearts != null) hearts.Play();
            if (bubble != null) bubble.Show("Slurp! Yum!");
            yield return new WaitForSeconds(1.6f);
            if (hearts != null) hearts.Stop();
            Win();
        }

        IEnumerator HideLater(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (bubble != null && !done && !busy) bubble.Hide();
        }
    }
}
