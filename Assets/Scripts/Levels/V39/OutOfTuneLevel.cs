using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "Out of tune" — la chitarra del gatto rockstar è scordata. Tocca una corda per pizzicarla, tocca il
    /// diapason per sentire la nota giusta di quella corda, trascina su/giù la sua chiavetta per alzare/abbassare
    /// il tono. L'accordatore a lancetta mostra calante/crescente solo mentre la corda suona; se suonano insieme
    /// corda e diapason si vede (e si sente) il "battimento". Una corda entro ±0.12 semitoni si illumina: accordate
    /// tutte e tre, il gatto suona un riff. Nessuna sconfitta.
    /// </summary>
    public class OutOfTuneLevel : LevelController
    {
        [SerializeField] LineRenderer[] strings;
        [SerializeField] LineRenderer[] glows;
        [SerializeField] Transform[] bridges, nuts, posts;
        [SerializeField] Transform[] pegs;
        [SerializeField] TapButton fork;
        [SerializeField] TextMesh forkLabel;
        [SerializeField] SpriteRenderer beatRing;
        [SerializeField] Transform needle;
        [SerializeField] TextMesh meterLabel;
        [SerializeField] TextMesh tunedText;
        [SerializeField] Transform cat;
        [SerializeField] SpriteRenderer noteTemplate;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] float tolerance = 0.12f, semitonesPerUnit = 0.8f, pegRadius = 0.6f;

        static readonly float[] baseHz = { 196f, 246.94f, 329.63f };
        static readonly string[] names = { "G", "B", "E" };
        const float ClipHz = 220f;
        const int Points = 14;

        float[] detune, ring;
        AudioSource[] voices;
        AudioSource forkVoice;
        float forkRing;
        int selected = -1, dragging = -1;
        float dragLastY;
        bool done;
        Camera cam;
        Vector3 catBase;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            catBase = cat.localPosition;
            noteTemplate.gameObject.SetActive(false);
            detune = new float[strings.Length];
            ring = new float[strings.Length];
            voices = new AudioSource[strings.Length];
            AudioClip pluck = PluckClip(), tone = ToneClip();
            for (int i = 0; i < strings.Length; i++)
            {
                detune[i] = (Random.value < 0.5f ? -1f : 1f) * Random.Range(0.6f, 1.8f);
                pegs[i].localRotation = Quaternion.Euler(0, 0, -detune[i] * 120f);
                voices[i] = strings[i].gameObject.AddComponent<AudioSource>();
                voices[i].playOnAwake = false;
                voices[i].clip = pluck;
                strings[i].positionCount = Points + 1;
                glows[i].positionCount = Points + 1;
            }
            forkVoice = fork.gameObject.AddComponent<AudioSource>();
            forkVoice.playOnAwake = false;
            forkVoice.clip = tone;
            forkVoice.volume = 0.5f;
            fork.Pressed += _ => PlayFork();
            beatRing.gameObject.SetActive(false);
            Select(0);
            Say("My guitar sounds like a cat fight. Tune it!", 2.6f);
        }

        // ---------- Suoni sintetizzati ----------

        /// <summary>Corda pizzicata (Karplus-Strong) a 220 Hz; il tono giusto si ottiene con AudioSource.pitch.</summary>
        static AudioClip PluckClip()
        {
            const int rate = 22050;
            int n = (int)(rate * 2.2f), period = Mathf.RoundToInt(rate / ClipHz);
            var buf = new float[period];
            var rnd = new System.Random(39);
            for (int i = 0; i < period; i++) buf[i] = (float)rnd.NextDouble() * 2f - 1f;
            var data = new float[n];
            for (int i = 0; i < n; i++)
            {
                int k = i % period;
                float v = buf[k];
                data[i] = v * 0.7f;
                buf[k] = 0.5f * (v + buf[(k + 1) % period]) * 0.996f;
            }
            AudioClip c = AudioClip.Create("pluck", n, 1, rate, false);
            c.SetData(data, 0);
            return c;
        }

        /// <summary>Nota pura del diapason (sinusoide a 220 Hz con attacco e coda morbidi).</summary>
        static AudioClip ToneClip()
        {
            const int rate = 22050;
            int n = (int)(rate * 2.5f);
            var data = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)rate;
                data[i] = Mathf.Sin(2f * Mathf.PI * ClipHz * t) * Mathf.Min(1f, t * 30f) * Mathf.Min(1f, (2.5f - t) * 2f) * 0.6f;
            }
            AudioClip c = AudioClip.Create("fork", n, 1, rate, false);
            c.SetData(data, 0);
            return c;
        }

        float Pitch(int i, float semis) => baseHz[i] / ClipHz * Mathf.Pow(2f, semis / 12f);

        void Pluck(int i)
        {
            voices[i].pitch = Pitch(i, detune[i]);
            voices[i].Play();
            ring[i] = 2.0f;
        }

        void PlayFork()
        {
            if (done || selected < 0) return;
            forkVoice.pitch = Pitch(selected, 0f);
            forkVoice.Play();
            forkRing = 2.4f;
        }

        void Select(int i)
        {
            selected = i;
            forkLabel.text = "NOTE " + names[i];
        }

        // ---------- Input ----------

        void Update()
        {
            float dt = Time.deltaTime;
            for (int i = 0; i < ring.Length; i++) ring[i] = Mathf.Max(0f, ring[i] - dt);
            forkRing = Mathf.Max(0f, forkRing - dt);
            if (!done && !IsFinished) HandleInput();
            DrawStrings();
            DrawMeter();
            if (!done && dragging < 0 && AllTuned()) StartCoroutine(Riff());
        }

        void HandleInput()
        {
            if (!PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out bool ended)) { dragging = -1; return; }
            Vector2 w = PointerInput.ToWorld(cam, sp);
            if (began)
            {
                for (int i = 0; i < pegs.Length; i++)
                {
                    if (Vector2.Distance(w, pegs[i].position) > pegRadius) continue;
                    dragging = i;
                    dragLastY = w.y;
                    Select(i);
                    Pluck(i);
                    Sfx.Play(this, "tick", 0.4f);
                    return;
                }
                for (int i = 0; i < strings.Length; i++)
                {
                    Vector3 a = bridges[i].position, b = nuts[i].position;
                    if (w.y < a.y - 0.2f || w.y > b.y + 0.1f || Mathf.Abs(w.x - a.x) > 0.38f) continue;
                    Select(i);
                    Pluck(i);
                    return;
                }
            }
            if (dragging >= 0)
            {
                float dy = w.y - dragLastY;
                dragLastY = w.y;
                float before = detune[dragging];
                detune[dragging] = Mathf.Clamp(detune[dragging] + dy * semitonesPerUnit, -2.2f, 2.2f);
                pegs[dragging].localRotation = Quaternion.Euler(0, 0, -detune[dragging] * 120f);
                voices[dragging].pitch = Pitch(dragging, detune[dragging]);
                ring[dragging] = Mathf.Max(ring[dragging], 1.0f);
                if (!voices[dragging].isPlaying) Pluck(dragging);
                if (Mathf.Abs(before) > tolerance && Mathf.Abs(detune[dragging]) <= tolerance) Haptics.Light();   // scatto quando entra in zona
                if (ended) dragging = -1;
            }
        }

        bool AllTuned()
        {
            foreach (float d in detune) if (Mathf.Abs(d) > tolerance) return false;
            return true;
        }

        // ---------- Grafica ----------

        void DrawStrings()
        {
            for (int i = 0; i < strings.Length; i++)
            {
                Vector3 a = bridges[i].position, b = nuts[i].position;
                float amp = Mathf.Min(0.09f, ring[i] * 0.05f);
                float wob = Mathf.Sin(Time.time * 55f + i * 1.7f);
                for (int k = 0; k < Points; k++)
                {
                    float t = k / (float)(Points - 1);
                    Vector3 p = Vector3.Lerp(a, b, t) + new Vector3(Mathf.Sin(t * Mathf.PI) * amp * wob, 0f, 0f);
                    strings[i].SetPosition(k, p);
                    glows[i].SetPosition(k, p);
                }
                strings[i].SetPosition(Points, posts[i].position);
                glows[i].SetPosition(Points, posts[i].position);
                bool tuned = Mathf.Abs(detune[i]) <= tolerance;
                glows[i].enabled = tuned;
                if (tuned)
                {
                    Color g = new Color(1f, 0.9f, 0.3f, 0.45f + 0.2f * Mathf.Sin(Time.time * 5f + i));
                    glows[i].startColor = glows[i].endColor = g;
                }
                Color sc = i == selected ? new Color(1f, 1f, 0.85f) : new Color(0.85f, 0.87f, 0.92f);
                strings[i].startColor = strings[i].endColor = sc;
            }
            int n = 0;
            foreach (float d in detune) if (Mathf.Abs(d) <= tolerance) n++;
            tunedText.text = "TUNED " + n + "/3";
        }

        void DrawMeter()
        {
            bool live = selected >= 0 && ring[selected] > 0f;
            needle.gameObject.SetActive(live);
            if (!live)
            {
                meterLabel.text = "PLUCK!";
                meterLabel.color = Color.white;
            }
            else
            {
                float d = detune[selected];
                float angle = -Mathf.Clamp(d / 1.2f, -1f, 1f) * 62f;
                // battimento: se suona anche il diapason la lancetta trema alla frequenza dei battimenti
                float beatHz = baseHz[selected] * Mathf.Abs(Mathf.Pow(2f, d / 12f) - 1f);
                if (forkRing > 0f) angle += Mathf.Sin(Time.time * 2f * Mathf.PI * beatHz) * Mathf.Min(6f, Mathf.Abs(d) * 20f);
                needle.localRotation = Quaternion.Euler(0, 0, angle);
                bool ok = Mathf.Abs(d) <= tolerance;
                meterLabel.text = names[selected] + (ok ? "  OK!" : d < 0f ? "  LOW" : "  HIGH");
                meterLabel.color = ok ? new Color(0.5f, 1f, 0.55f) : new Color(1f, 0.6f, 0.5f);
            }
            // anello del diapason che pulsa coi battimenti
            bool both = forkRing > 0f && selected >= 0 && ring[selected] > 0f;
            beatRing.gameObject.SetActive(forkRing > 0f);
            if (forkRing > 0f)
            {
                float beat = both ? baseHz[selected] * Mathf.Abs(Mathf.Pow(2f, detune[selected] / 12f) - 1f) : 0f;
                float pulse = both ? 0.5f + 0.5f * Mathf.Cos(Time.time * 2f * Mathf.PI * Mathf.Min(beat, 12f)) : 0.8f;
                beatRing.color = new Color(1f, 0.9f, 0.4f, 0.25f + 0.5f * pulse * Mathf.Min(1f, forkRing));
                beatRing.transform.localScale = Vector3.one * (1.4f + 0.4f * pulse);
            }
        }

        // ---------- Vittoria ----------

        IEnumerator Riff()
        {
            done = true;
            Say("NOW we're talking. Hit it!", 0f);
            yield return new WaitForSeconds(0.6f);
            int[] str = { 0, 0, 1, 0, 2, 1, 0, 2, 2 };
            int[] semi = { 0, 3, 0, 5, 0, 3, 0, 3, 7 };
            float[] len = { 0.22f, 0.22f, 0.3f, 0.22f, 0.3f, 0.22f, 0.22f, 0.3f, 0.6f };
            for (int n = 0; n < str.Length; n++)
            {
                int i = str[n];
                voices[i].pitch = Pitch(i, semi[n]);
                voices[i].Play();
                ring[i] = 0.6f;
                StartCoroutine(FloatNote(new Vector3(cat.position.x + Random.Range(-0.3f, 0.5f), cat.position.y + 0.6f, 0f)));
                for (float t = 0f; t < len[n]; t += Time.deltaTime)
                {
                    cat.localPosition = catBase + new Vector3(0f, Mathf.Abs(Mathf.Sin(t / len[n] * Mathf.PI)) * 0.15f, 0f);
                    cat.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(t / len[n] * Mathf.PI) * (n % 2 == 0 ? 8f : -8f));
                    yield return null;
                }
            }
            cat.localPosition = catBase;
            cat.localRotation = Quaternion.identity;
            Haptics.Medium();
            Say("Thank you, goodnight!", 0f);
            yield return new WaitForSeconds(1.6f);
            Win();
        }

        IEnumerator FloatNote(Vector3 from)
        {
            SpriteRenderer n = Instantiate(noteTemplate, noteTemplate.transform.parent);
            n.gameObject.SetActive(true);
            for (float t = 0f; t < 1f; t += Time.deltaTime / 1.2f)
            {
                n.transform.position = from + new Vector3(Mathf.Sin(t * 8f) * 0.15f, t * 1.6f, 0f);
                n.color = new Color(1f, 1f, 1f, 1f - t);
                yield return null;
            }
            Destroy(n.gameObject);
        }

        int said;

        void Say(string text, float seconds)
        {
            if (bubble == null) return;
            bubble.Show(text);
            int id = ++said;
            if (seconds > 0f) StartCoroutine(HideLater(seconds, id));
        }

        IEnumerator HideLater(float seconds, int id)
        {
            yield return new WaitForSeconds(seconds);
            if (bubble != null && !done && id == said) bubble.Hide();
        }
    }
}
