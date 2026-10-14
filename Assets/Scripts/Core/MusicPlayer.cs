using UnityEngine;

namespace Gotcha.Core
{
    /// <summary>Colonna sonora in loop. Si può spegnere dal menu; la scelta resta salvata.</summary>
    [RequireComponent(typeof(AudioSource))]
    public class MusicPlayer : MonoBehaviour
    {
        const string MutedKey = "gotcha.musicMuted";

        public AudioClip soundtrack;
        [Range(0f, 1f)] public float volume = 0.6f;

        public static MusicPlayer Instance { get; private set; }
        public bool Muted { get; private set; }

        AudioSource source;

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

        public void SetMuted(bool muted)
        {
            Muted = muted;
            source.mute = muted;
            PlayerPrefs.SetInt(MutedKey, muted ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
}
