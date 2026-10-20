using UnityEngine;

namespace Gotcha.Components
{
    /// <summary>
    /// Vibrazione del telefono come feedback: tocchi, esplosioni, rotture, punture, vittoria e sconfitta.
    /// Su Android usa VibrationEffect (durata E intensità, API 26+) con l'uso "gioco", che segue l'intensità delle
    /// vibrazioni multimediali: la vecchia vibrate(ms) senza categoria su molti telefoni recenti viene ignorata o è
    /// impercettibile. Il permesso VIBRATE lo aggiunge al manifest GotchaAndroidManifest (Editor) a ogni build.
    /// Si spegne dalle Impostazioni (VIBRATION: ON/OFF).
    /// </summary>
    public static class Haptics
    {
        const string Key = "gotcha.haptics";
        public static bool Enabled
        {
            get => PlayerPrefs.GetInt(Key, 1) == 1;
            set { PlayerPrefs.SetInt(Key, value ? 1 : 0); PlayerPrefs.Save(); }
        }

        /// <summary>Tocco di un pulsante: appena percettibile.</summary>
        public static void Tap() => Pulse(18, 70);
        /// <summary>Colpo leggero (tocco, scatto), medio (rottura, puntura), forte (esplosione).</summary>
        public static void Light() => Pulse(30, 110);
        public static void Medium() => Pulse(80, 190);
        public static void Heavy() => Pulse(320, 255);

        /// <summary>Livello superato: due colpetti allegri.</summary>
        public static void Success() => Pattern(new long[] { 0, 40, 70, 60 }, new[] { 0, 150, 0, 230 });
        /// <summary>Livello perso: tre scossoni che sghignazzano, come la risata.</summary>
        public static void Fail() => Pattern(new long[] { 0, 90, 60, 70, 60, 160 }, new[] { 0, 220, 0, 160, 0, 255 });

        /// <summary>Compatibilità: vibrazione semplice della durata indicata.</summary>
        public static void Vibrate(long ms) => Pulse(ms, ms >= 200 ? 255 : ms >= 60 ? 190 : 110);

        public static void Pulse(long ms, int amplitude)
        {
            if (!Enabled) return;
#if UNITY_ANDROID && !UNITY_EDITOR
            Android.OneShot(ms, amplitude);
#elif UNITY_IOS && !UNITY_EDITOR
            if (ms >= 60) Handheld.Vibrate();
#endif
        }

        public static void Pattern(long[] timings, int[] amplitudes)
        {
            if (!Enabled) return;
#if UNITY_ANDROID && !UNITY_EDITOR
            Android.Waveform(timings, amplitudes);
#elif UNITY_IOS && !UNITY_EDITOR
            Handheld.Vibrate();
#endif
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        static class Android
        {
            static AndroidJavaObject vibrator, attributes;
            static int sdk = -1;
            static bool failed;

            static bool Ready()
            {
                if (failed) return false;
                if (vibrator != null) return true;
                try
                {
                    using (var version = new AndroidJavaClass("android.os.Build$VERSION")) sdk = version.GetStatic<int>("SDK_INT");
                    using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                    using (var activity = player.GetStatic<AndroidJavaObject>("currentActivity"))
                        vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator");
                    if (vibrator == null || !vibrator.Call<bool>("hasVibrator")) { failed = true; return false; }
                    if (sdk >= 26)
                    {
                        // AudioAttributes.USAGE_GAME (14): segue l'intensità delle vibrazioni "multimediali"
                        using (var builder = new AndroidJavaObject("android.media.AudioAttributes$Builder"))
                        {
                            builder.Call<AndroidJavaObject>("setUsage", 14).Dispose();
                            attributes = builder.Call<AndroidJavaObject>("build");
                        }
                    }
                    return true;
                }
                catch { failed = true; return false; }
            }

            public static void OneShot(long ms, int amplitude)
            {
                if (!Ready()) { Fallback(); return; }
                try
                {
                    if (sdk >= 26)
                    {
                        using (var effects = new AndroidJavaClass("android.os.VibrationEffect"))
                        using (var effect = effects.CallStatic<AndroidJavaObject>("createOneShot", ms, Mathf.Clamp(amplitude, 1, 255)))
                            Vibrate(effect);
                    }
                    else vibrator.Call("vibrate", ms);
                }
                catch { Fallback(); }
            }

            public static void Waveform(long[] timings, int[] amplitudes)
            {
                if (!Ready()) { Fallback(); return; }
                try
                {
                    if (sdk >= 26)
                    {
                        using (var effects = new AndroidJavaClass("android.os.VibrationEffect"))
                        using (var effect = effects.CallStatic<AndroidJavaObject>("createWaveform", timings, amplitudes, -1))
                            Vibrate(effect);
                    }
                    else vibrator.Call("vibrate", timings, -1);
                }
                catch { Fallback(); }
            }

            static void Vibrate(AndroidJavaObject effect)
            {
                if (attributes != null) vibrator.Call("vibrate", effect, attributes);
                else vibrator.Call("vibrate", effect);
            }

            static void Fallback()
            {
                try { Handheld.Vibrate(); } catch { }
            }
        }
#endif
    }
}
