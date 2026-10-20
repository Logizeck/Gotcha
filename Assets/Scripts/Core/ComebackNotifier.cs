using System;
using System.Collections;
using UnityEngine;
#if UNITY_ANDROID && !UNITY_EDITOR
using Unity.Notifications.Android;
#endif

namespace Gotcha.Core
{
    /// <summary>
    /// Notifica "I'm missing you! &lt;3" 24 ore dopo l'ultimo accesso, ma solo tra le 08:00 e le 22:00: se le 24 ore
    /// cadono di notte, arriva alle 08:00 successive. Ogni volta che il gioco va in pausa o si chiude la notifica viene
    /// riprogrammata da capo (resta sempre una sola), e all'apertura quella in attesa viene cancellata.
    /// Il permesso (Android 13+) si chiede dopo il primo livello superato, non all'avvio.
    /// Richiede il pacchetto com.unity.mobile.notifications (Packages/manifest.json).
    /// </summary>
    public class ComebackNotifier : MonoBehaviour
    {
        public const string Title = "Gotcha!";
        public const string Message = "I'm missing you! <3";
        const string ChannelId = "gotcha_comeback";
        const string EnabledKey = "gotcha.notifications";
        const string AskedKey = "gotcha.notifications.asked";

        public static ComebackNotifier Instance { get; private set; }

        /// <summary>Scelta del giocatore dalle Impostazioni (di base attive).</summary>
        public static bool Enabled
        {
            get => PlayerPrefs.GetInt(EnabledKey, 1) == 1;
            set
            {
                PlayerPrefs.SetInt(EnabledKey, value ? 1 : 0);
                PlayerPrefs.Save();
                if (Instance != null) { if (value) Instance.Schedule(); else Instance.CancelAll(); }
            }
        }

        /// <summary>Quando arriva la notifica: 24 ore dopo, spostata alle 08:00 se cade fuori dalla fascia 08-22.</summary>
        public static DateTime FireTime(DateTime lastSeen)
        {
            DateTime t = lastSeen.AddHours(24);
            if (t.TimeOfDay < TimeSpan.FromHours(8)) return t.Date.AddHours(8);
            if (t.TimeOfDay > TimeSpan.FromHours(22)) return t.Date.AddDays(1).AddHours(8);
            return t;
        }

        void Awake()
        {
            Instance = this;
#if UNITY_ANDROID && !UNITY_EDITOR
            AndroidNotificationCenter.RegisterNotificationChannel(new AndroidNotificationChannel(ChannelId, "Reminders", "A nudge when you haven't played for a day", Importance.Default));
#endif
            CancelAll();   // il giocatore è qui: niente promemoria in sospeso
        }

        void OnDestroy() { if (Instance == this) Instance = null; }

        void OnApplicationPause(bool paused) { if (paused) Schedule(); else CancelAll(); }
        void OnApplicationQuit() { Schedule(); }

        /// <summary>Chiede il permesso delle notifiche (una volta sola), es. dopo il primo livello superato.</summary>
        public void AskPermissionOnce()
        {
            if (PlayerPrefs.GetInt(AskedKey, 0) == 1) return;
            PlayerPrefs.SetInt(AskedKey, 1);
            PlayerPrefs.Save();
            StartCoroutine(AskPermission());
        }

        IEnumerator AskPermission()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            var request = new PermissionRequest();
            while (request.Status == PermissionStatus.RequestPending) yield return null;
#else
            yield break;
#endif
        }

        void Schedule()
        {
            CancelAll();
            if (!Enabled) return;
#if UNITY_ANDROID && !UNITY_EDITOR
            var n = new AndroidNotification
            {
                Title = Title,
                Text = Message,
                FireTime = FireTime(DateTime.Now),
            };
            AndroidNotificationCenter.SendNotification(n, ChannelId);
#endif
        }

        void CancelAll()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            AndroidNotificationCenter.CancelAllScheduledNotifications();
            AndroidNotificationCenter.CancelAllDisplayedNotifications();
#endif
        }
    }
}
