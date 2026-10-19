using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;
using Gotcha.Sensors;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 22 — zuppa bollente. Il topolino ha fame, ma la zuppa scotta (il termometro è al massimo).
    /// Prima si soffia nel microfono per raffreddarla (il vapore cala), POI si trascina la scodella al topolino.
    /// Easter egg `hot_soup`: se gliela dai ancora bollente, il topo diventa rosso, fuma, prende fuoco e resta
    /// solo lo scheletrino con le X al posto degli occhi e la linguaccia. (Editor: tasto B per soffiare.)
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
        [Header("Easter egg: zuppa bollente")]
        [SerializeField] SpriteRenderer mouseArt;
        [SerializeField] GameObject skeleton;
        [SerializeField] SmokeEmitter fire;
        [SerializeField] SmokeEmitter blackSmoke;
        [SerializeField] SpriteRenderer burnGlow;
        [SerializeField] string eggId = "hot_soup";

        float temp = 1f;
        [SerializeField] float fanDistance = 14f;    // quanto "sventolare" col dito per raffreddarla del tutto
        bool fanning, fanHint;
        Vector2 lastFan;
        float quietFor;
        Camera cam;
        Vector3 bowlHome;
        bool done, busy;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            if (SensorHub.Instance != null) SensorHub.Instance.UseMic(this);
            bowlHome = bowl.transform.position;
            bowl.Dropped += OnDropped;
            if (steam != null) steam.Play();
            if (hearts != null) hearts.Stop();
            if (bubble != null) { bubble.Show("Soup! Soup!"); StartCoroutine(HideLater(1.5f)); }
            if (skeleton != null) skeleton.SetActive(false);
            if (fire != null) fire.Stop();
            if (blackSmoke != null) blackSmoke.Stop();
            if (burnGlow != null) burnGlow.color = new Color(1f, 0.5f, 0.1f, 0f);
        }

        void OnDestroy()
        {
            if (bowl != null) bowl.Dropped -= OnDropped;
            if (SensorHub.Instance != null) SensorHub.Instance.ReleaseMic(this);
            if (squeak != null) Destroy(squeak);
        }

        void Update()
        {
            if (done || IsFinished) return;
            float level = SensorHub.Instance != null ? SensorHub.Instance.MicLevel01 : 0f;
#if UNITY_EDITOR
            if (Input.GetKey(KeyCode.B)) level = 1f;
#endif
            if (level > blowThreshold) temp = Mathf.Max(0f, temp - Time.deltaTime / blowSeconds);

            // senza microfono (permesso negato, telefono senza mic...) si può anche fare vento col dito sul vapore
            if (Draggable2D.Active == null && PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out bool ended) && !ended)
            {
                Vector2 w = PointerInput.ToWorld(cam, sp);
                Vector2 b = bowl.transform.position;
                bool overSteam = Mathf.Abs(w.x - b.x) < 1.1f && w.y > b.y + 0.45f && w.y < b.y + 2.3f;
                if (overSteam && fanning && !began) temp = Mathf.Max(0f, temp - Vector2.Distance(w, lastFan) / fanDistance);
                fanning = overSteam;
                lastFan = w;
            }
            else fanning = false;
            quietFor = level > 0.02f ? 0f : quietFor + Time.deltaTime;
            if (!fanHint && quietFor > 7f && temp > cool && bubble != null) { fanHint = true; bubble.Show("No breath? Fan it with your finger!"); StartCoroutine(HideLater(2.2f)); }
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
            if (temp > cool) StartCoroutine(Burnt());
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

        AudioClip squeak;

        /// <summary>L'urletto del topolino (come quello del pupazzo di neve, ma acutissimo).</summary>
        void Squeak()
        {
            AudioSource src = GetComponent<AudioSource>();
            if (src == null) src = gameObject.AddComponent<AudioSource>();
            if (squeak == null) squeak = ScreamSynth.Mouse();
            src.PlayOneShot(squeak, 0.9f);
        }

        /// <summary>Zuppa bollente: il topo la beve d'un fiato... e si carbonizza.</summary>
        IEnumerator Burnt()
        {
            done = true;
            bowl.enabled = false;
            if (bubble != null) bubble.Show("Slurp!");
            yield return new WaitForSeconds(0.5f);
            // diventa rosso fuoco e trema, fumo dalle orecchie
            Vector3 m = mouse.localPosition;
            if (bubble != null) bubble.Show("HOT HOT HOT!!!");
            Squeak();
            Haptics.Medium();
            if (steam != null) { steam.transform.position = mouse.position + Vector3.up * 0.7f; steam.puffsPerSecond = 14f; steam.Play(); }
            for (float k = 0f; k < 1f; k += Time.deltaTime / 1.1f)
            {
                if (mouseArt != null) mouseArt.color = Color.Lerp(Color.white, new Color(1f, 0.35f, 0.3f), k);
                mouse.localPosition = m + (Vector3)(Random.insideUnitCircle * 0.05f * k) + Vector3.up * Mathf.Abs(Mathf.Sin(k * 20f)) * 0.15f;
                if (burnGlow != null) burnGlow.color = new Color(1f, 0.5f, 0.1f, k * 0.6f);
                yield return null;
            }
            // FWOOSH: fiammata
            if (fire != null) fire.Play();
            Haptics.Heavy();
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.6f)
            {
                if (burnGlow != null) { burnGlow.color = new Color(1f, 0.55f, 0.15f, 0.6f + 0.4f * Mathf.Sin(k * Mathf.PI)); burnGlow.transform.localScale = Vector3.one * (4f + k * 3f); }
                yield return null;
            }
            // resta lo scheletrino
            if (mouseArt != null) mouseArt.gameObject.SetActive(false);
            if (skeleton != null) skeleton.SetActive(true);
            mouse.localPosition = m;
            if (fire != null) fire.Stop();
            if (steam != null) steam.Stop();
            if (blackSmoke != null) blackSmoke.Play();
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.5f)
            {
                if (burnGlow != null) burnGlow.color = new Color(1f, 0.5f, 0.1f, (1f - k) * 0.8f);
                yield return null;
            }
            if (bubble != null) bubble.Show("...a bit hot.");
            yield return new WaitForSeconds(1.8f);
            if (blackSmoke != null) blackSmoke.Stop();
            FailWithEgg(eggId);
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
