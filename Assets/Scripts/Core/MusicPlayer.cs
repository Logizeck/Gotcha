using System.Collections.Generic;
using UnityEngine;

namespace Gotcha.Core
{
    /// <summary>
    /// Colonna sonora in loop. Si può spegnere dal menu; la scelta resta salvata.
    /// I livelli possono abbassarla temporaneamente (Duck/Unduck): ogni richiesta è legata a chi la fa, e se
    /// quell'oggetto viene distrutto la richiesta decade da sola, così la musica non resta mai muta per errore.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class MusicPlayer : MonoBehaviour
    {
        const string MutedKey = "gotcha.musicMuted";

        public AudioClip soundtrack;
        [Range(0f, 1f)] public float volume = 0.6f;

        public static MusicPlayer Instance { get; private set; }
        /// <summary>Scelta del giocatore dal menu (true = musica spenta).</summary>
        public bool Muted { get; private set; }

        AudioSource source;
        readonly Dictionary<Object, float> ducks = new Dictionary<Object, float>();
        readonly List<Object> stale = new List<Object>();

        void Awake()
        {
            Instance = this;
            source = GetComponent<AudioSource>();
            source.clip = soundtrack;
            source.loop = true;
            source.playOnAwake = false;
            source.volume = volume;
            Muted = PlayerPrefs.GetInt(MutedKey, 0) == 1;
            source.mute = Muted;
            if (soundtrack != null) source.Play();
        }

        void OnDestroy() { if (Instance == this) Instance = null; }

        void Update()
        {
            // le richieste di chi non esiste più (livello chiuso) decadono
            stale.Clear();
            foreach (Object owner in ducks.Keys) if (owner == null) stale.Add(owner);
            foreach (Object owner in stale) ducks.Remove(owner);

            float factor = 1f;
            foreach (float f in ducks.Values) factor = Mathf.Min(factor, f);
            source.volume = volume * factor;
            source.mute = Muted;
            if (!source.isPlaying && soundtrack != null && !AudioListener.pause) source.Play();
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
