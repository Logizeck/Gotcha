using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Gotcha.Core
{
    /// <summary>
    /// Vite: al massimo <see cref="MaxLives"/>; ogni fallimento ne costa una; se ne ricarica una ogni 15 minuti,
    /// una guardando una pubblicità, una per ogni amico che accetta un invito.
    ///
    /// A prova di "cambio dell'ora": la ricarica NON usa mai l'orologio del telefono (che l'utente può spostare).
    /// Usa il tempo monotòno dall'accensione (Android SystemClock.elapsedRealtime, che non si può modificare) e il
    /// contatore dei riavvii (Settings.Global.BOOT_COUNT):
    ///  - stessa accensione: tempo trascorso = elapsed attuale − elapsed salvato (esatto);
    ///  - dopo un riavvio: si accredita solo il tempo dall'accensione, che è sicuramente minore o uguale al tempo
    ///    passato davvero (si perde il tempo a telefono spento, mai il contrario).
    /// I dati salvati sono firmati (HMAC): se vengono modificati a mano, le vite ripartono da zero.
    /// </summary>
    public class LivesManager : MonoBehaviour
    {
        public const int MaxLives = 5;
        public const float RegenSeconds = 15f * 60f;

        const string DataKey = "gotcha.lives.v1";
        const string SigKey = "gotcha.lives.sig";
        const string Salt = "G0tcha!-hearts-7f3a";

        public static LivesManager Instance { get; private set; }

        public int Lives { get; private set; }
        /// <summary>Secondi mancanti alla prossima vita (0 se piene).</summary>
        public float SecondsToNext => Lives >= MaxLives ? 0f : Mathf.Max(0f, RegenSeconds - accum);
        public event Action<int, int> Changed;   // (vite prima, vite dopo)

        float accum;            // secondi accumulati verso la prossima vita
        float infiniteLeft;     // secondi di vite infinite rimasti (orologio monotòno, come la ricarica)
        long lastElapsedMs;     // tempo monotòno all'ultimo aggiornamento
        int lastBoot;
        float pollTimer, sinceSave;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
            Load();
            Tick();
        }

        void OnDestroy() { if (Instance == this) Instance = null; }

        void Update()
        {
            pollTimer -= Time.unscaledDeltaTime;
            if (pollTimer > 0f) return;
            pollTimer = 1f;
            Tick();
        }

        void OnApplicationPause(bool paused) { if (paused) Save(); else Tick(); }
        void OnApplicationFocus(bool focus) { if (focus) Tick(); }
        void OnApplicationQuit() { Save(); }

        // ---------- API ----------

        public bool CanPlay => Lives > 0 || Infinite;

        /// <summary>Vite infinite (acquisto da 0,99 €): fallire non costa niente finché dura.</summary>
        public bool Infinite => infiniteLeft > 0f;
        public float InfiniteSecondsLeft => infiniteLeft;

        public void GrantInfinite(float seconds)
        {
            Tick();
            infiniteLeft += seconds;
            Save();
            Changed?.Invoke(Lives, Lives);
        }

        /// <summary>Un fallimento: una vita in meno (la ricarica parte da qui se prima erano piene).</summary>
        public void LoseLife()
        {
            Tick();
            if (Infinite || Lives <= 0) return;
            int before = Lives;
            if (Lives >= MaxLives) accum = 0f;
            Lives--;
            Save();
            Changed?.Invoke(before, Lives);
        }

        /// <summary>Vita extra (pubblicità guardata, amico che ha accettato l'invito).</summary>
        public void GrantLife(int count = 1)
        {
            Tick();
            int before = Lives;
            Lives = Mathf.Min(MaxLives, Lives + count);
            if (Lives >= MaxLives) accum = 0f;
            Save();
            if (Lives != before) Changed?.Invoke(before, Lives);
        }

        // ---------- Tempo ----------

        void Tick()
        {
            long now = ElapsedMs();
            int boot = BootCount();
            double delta;
            bool sameBoot = boot >= 0 && lastBoot >= 0 ? boot == lastBoot : now >= lastElapsedMs;
            if (sameBoot) delta = (now - lastElapsedMs) / 1000.0;
            else delta = now / 1000.0;   // riavvio: solo il tempo dall'accensione (limite inferiore sicuro)
            if (delta < 0) delta = 0;
            lastElapsedMs = now;
            lastBoot = boot;
            sinceSave += (float)delta;
            bool wasInfinite = infiniteLeft > 0f;
            if (infiniteLeft > 0f) infiniteLeft = Mathf.Max(0f, infiniteLeft - (float)delta);
            if (wasInfinite && infiniteLeft <= 0f) { Save(); Changed?.Invoke(Lives, Lives); }

            int before = Lives;
            if (Lives < MaxLives)
            {
                accum += (float)Math.Min(delta, RegenSeconds * MaxLives);
                while (accum >= RegenSeconds && Lives < MaxLives) { accum -= RegenSeconds; Lives++; }
                if (Lives >= MaxLives) accum = 0f;
            }
            // salva quando cambia qualcosa, altrimenti ogni 15 secondi (ancora e accumulo restano coerenti)
            if (Lives != before || sinceSave >= 15f) { Save(); sinceSave = 0f; }
            if (Lives != before) Changed?.Invoke(before, Lives);
        }

        static long ElapsedMs()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try { using (var clock = new AndroidJavaClass("android.os.SystemClock")) return clock.CallStatic<long>("elapsedRealtime"); }
            catch { }
#endif
            // Editor e altre piattaforme: orologio di sistema (solo per i test)
            return DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;
        }

        static int BootCount()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var activity = player.GetStatic<AndroidJavaObject>("currentActivity"))
                using (var resolver = activity.Call<AndroidJavaObject>("getContentResolver"))
                using (var global = new AndroidJavaClass("android.provider.Settings$Global"))
                    return global.CallStatic<int>("getInt", resolver, "boot_count", -1);
            }
            catch { return -1; }
#else
            return -1;
#endif
        }

        // ---------- Salvataggio firmato ----------

        void Load()
        {
            string data = PlayerPrefs.GetString(DataKey, "");
            string sig = PlayerPrefs.GetString(SigKey, "");
            if (string.IsNullOrEmpty(data))
            {
                // prima partita: vite piene
                Lives = MaxLives;
                accum = 0f;
                lastElapsedMs = ElapsedMs();
                lastBoot = BootCount();
                Save();
                return;
            }
            string[] p = data.Split('|');
            if (sig != Sign(data) || (p.Length != 4 && p.Length != 5) ||
                !int.TryParse(p[0], out int lives) || !float.TryParse(p[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float acc) ||
                !long.TryParse(p[2], out long el) || !int.TryParse(p[3], out int bc))
            {
                Debug.LogWarning("[Gotcha] Dati delle vite non validi: si riparte da zero vite.");
                Lives = 0;
                accum = 0f;
                lastElapsedMs = ElapsedMs();
                lastBoot = BootCount();
                Save();
                return;
            }
            Lives = Mathf.Clamp(lives, 0, MaxLives);
            accum = Mathf.Clamp(acc, 0f, RegenSeconds);
            lastElapsedMs = el;
            lastBoot = bc;
            infiniteLeft = p.Length == 5 && float.TryParse(p[4], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float inf) ? Mathf.Clamp(inf, 0f, 24f * 3600f) : 0f;
        }

        void Save()
        {
            string data = Lives + "|" + accum.ToString("R", System.Globalization.CultureInfo.InvariantCulture) + "|" + lastElapsedMs + "|" + lastBoot
                          + "|" + infiniteLeft.ToString("R", System.Globalization.CultureInfo.InvariantCulture);
            PlayerPrefs.SetString(DataKey, data);
            PlayerPrefs.SetString(SigKey, Sign(data));
            PlayerPrefs.Save();
        }

        static string Sign(string data)
        {
            byte[] key = Encoding.UTF8.GetBytes(Salt + SystemInfo.deviceUniqueIdentifier);
            using (var h = new HMACSHA256(key))
                return Convert.ToBase64String(h.ComputeHash(Encoding.UTF8.GetBytes(data)));
        }
    }
}
