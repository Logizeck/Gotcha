using UnityEngine;

namespace Gotcha.Components
{
    /// <summary>
    /// Vibrazione del telefono per i momenti "fisici": esplosioni, oggetti che si rompono, scosse, punture.
    /// Su Android usa il Vibrator di sistema con la durata richiesta (impulsi brevi e forti, non il ronzio fisso da
    /// mezzo secondo di Handheld.Vibrate); se non è disponibile ripiega su Handheld.Vibrate.
    /// </summary>
    public static class Haptics
    {
        const string Key = "gotcha.haptics";
        public static bool Enabled
        {
            get => PlayerPrefs.GetInt(Key, 1) == 1;
            set { PlayerPrefs.SetInt(Key, value ? 1 : 0); PlayerPrefs.Save(); }
        }

        /// <summary>Colpo leggero (tocco, scatto), medio (rottura, puntura), forte (esplosione).</summary>
        public static void Light() => Vibrate(25);
        public static void Medium() => Vibrate(90);
        public static void Heavy() => Vibrate(350);

        public static void Vibrate(long ms)
        {
            if (!Enabled) return;
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var activity = player.GetStatic<AndroidJavaObject>("currentActivity"))
                using (var vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator"))
                    vibrator.Call("vibrate", ms);
            }
            catch { Handheld.Vibrate(); }
#elif UNITY_IOS && !UNITY_EDITOR
            Handheld.Vibrate();
#endif
        }
    }
}
