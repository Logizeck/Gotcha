using System.Collections.Generic;
using UnityEngine;
using Gotcha.Levels;

namespace Gotcha.Core
{
    /// <summary>
    /// Musica di sottofondo: un brano per il menu (Title) e uno per ogni tipo di livello (Dungeon, Animals, Kitchen,
    /// Pressure), tutti in loop. Quando il brano richiesto cambia, quello in corso sfuma e il nuovo entra in dissolvenza;
    /// se è lo stesso, continua senza interruzioni. Si può spegnere dal menu; la scelta resta salvata.
    /// I livelli possono abbassarla temporaneamente (Duck/Unduck): ogni richiesta è legata a chi la fa, e se
    /// quell'oggetto viene distrutto la richiesta decade da sola, così la musica non resta mai muta per errore.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class MusicPlayer : MonoBehaviour
    {
        const string MutedKey = "gotcha.musicMuted";

        [Tooltip("Brano usato dagli stereo dei livelli 7/19 (non suona come sottofondo).")]
        public AudioClip soundtrack;
        public AudioClip title, dungeon, animals, kitchen, pressure, halloween;
        [Range(0f, 1f)] public float volume = 0.6f;
        public float fadeOut = 0.7f, fadeIn = 0.9f;

        public static MusicPlayer Instance { get; private set; }
        /// <summary>Scelta del giocatore dal menu (true = musica spenta).</summary>
        public bool Muted { get; private set; }
        public MusicTrack Current { get; private set; } = MusicTrack.Title;

        AudioSource source;
        readonly Dictionary<Object, float> ducks = new Dictionary<Object, float>();
        readonly List<Object> stale = new List<Object>();
        AudioClip pending;     // brano che entra dopo la sfumata
        float fade = 1f;       // 0..1, moltiplica il volume
        bool fadingOut;
        int lastSamples = -1;  // per accorgersi di un brano "in play" ma fermo (uscita audio persa)
        float stuckFor;

        void Awake()
        {
            Instance = this;
            source = GetComponent<AudioSource>();
            source.loop = true;
            source.playOnAwake = false;
            Muted = PlayerPrefs.GetInt(MutedKey, 0) == 1;
            source.mute = Muted;
            // all'apertura del gioco: la musica del titolo, in dissolvenza
            source.clip = Clip(MusicTrack.Title);
            fade = 0f;
            source.volume = 0f;
            if (source.clip != null) source.Play();
        }

        void OnDestroy() { if (Instance == this) Instance = null; }

        // Al primo avvio Android mostra la richiesta del microfono: l'app perde il fuoco e, su molti telefoni, al ritorno
        // l'AudioSource dice di suonare ma l'uscita audio è stata ricreata e resta muta. Quindi al ritorno non ci si
        // fida di isPlaying: si riavvia il brano dal punto in cui era (e lo si fa anche se la clip non era ancora
        // caricata al primo Play).
        void OnApplicationFocus(bool focus) { if (focus) Resume(); }
        void OnApplicationPause(bool paused) { if (!paused) Resume(); }

        void Resume()
        {
            if (source == null || source.clip == null || AudioListener.pause) return;
            if (source.clip.loadState == AudioDataLoadState.Unloaded) source.clip.LoadAudioData();
            if (source.clip.loadState != AudioDataLoadState.Loaded) return;   // ci pensa Update appena è pronta
            int at = source.timeSamples;
            source.Stop();
            source.Play();
            source.timeSamples = Mathf.Clamp(at, 0, Mathf.Max(0, source.clip.samples - 1));
            stuckFor = 0f;
        }

        public AudioClip Clip(MusicTrack track)
        {
            AudioClip c;
            switch (track)
            {
                case MusicTrack.Title: c = title; break;
                case MusicTrack.Dungeon: c = dungeon; break;
                case MusicTrack.Animals: c = animals; break;
                case MusicTrack.Kitchen: c = kitchen; break;
                case MusicTrack.Halloween: c = halloween != null ? halloween : pressure; break;
                default: c = pressure; break;
            }
            return c != null ? c : soundtrack;
        }

        /// <summary>Passa al brano richiesto: sfuma quello in corso e fa entrare il nuovo. Stesso brano = nessun cambio.</summary>
        public void Play(MusicTrack track)
        {
            AudioClip next = Clip(track);
            Current = track;
            if (next == null) return;
            if (pending == null && source.clip == next && !fadingOut) return;   // stesso brano: resta in loop
            if (pending == next) return;
            if (source.clip == next && fadingOut) { pending = null; fadingOut = false; return; }   // ci ripensa: risale
            pending = next;
            fadingOut = source.clip != null && source.isPlaying && fade > 0.01f;
            if (!fadingOut) Switch();
        }

        void Switch()
        {
            source.Stop();
            source.clip = pending;
            pending = null;
            fadingOut = false;
            fade = 0f;
            source.Play();
        }

        void Update()
        {
            // le richieste di chi non esiste più (livello chiuso) decadono
            stale.Clear();
            foreach (Object owner in ducks.Keys) if (owner == null) stale.Add(owner);
            foreach (Object owner in stale) ducks.Remove(owner);

            float dt = Time.unscaledDeltaTime;   // le sfumate continuano anche in pausa
            if (fadingOut)
            {
                fade = Mathf.MoveTowards(fade, 0f, dt / Mathf.Max(0.01f, fadeOut));
                if (fade <= 0f) Switch();
            }
            else fade = Mathf.MoveTowards(fade, 1f, dt / Mathf.Max(0.01f, fadeIn));

            float factor = 1f;
            foreach (float f in ducks.Values) factor = Mathf.Min(factor, f);
            source.volume = volume * factor * fade;
            source.mute = Muted;
            if (!source.isPlaying && source.clip != null && !AudioListener.pause && source.clip.loadState != AudioDataLoadState.Loading)
            {
                if (source.clip.loadState == AudioDataLoadState.Unloaded) source.clip.LoadAudioData();
                else source.Play();
            }
            // controllo: se il brano risulta in play ma non avanza da più di un secondo, si riavvia
            if (source.isPlaying && !AudioListener.pause && source.clip != null)
            {
                if (source.timeSamples == lastSamples) stuckFor += dt; else stuckFor = 0f;
                lastSamples = source.timeSamples;
                if (stuckFor > 1f) Resume();
            }
            else { stuckFor = 0f; lastSamples = -1; }
        }

        /// <summary>Abbassa la musica finché <paramref name="owner"/> non chiama Unduck (o viene distrutto).</summary>
        public void Duck(Object owner, float factor = 0.15f)
        {
            if (owner != null) ducks[owner] = Mathf.Clamp01(factor);
        }

        public void Unduck(Object owner)
        {
            if (owner != null) ducks.Remove(owner);
        }

        public void SetMuted(bool muted)
        {
            Muted = muted;
            source.mute = muted;
            PlayerPrefs.SetInt(MutedKey, muted ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
}
