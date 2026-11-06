using System;
using UnityEngine;
using Gotcha.UI;

namespace Gotcha.Core
{
    /// <summary>
    /// Condivisioni "controllate": il premio arriva solo se la condivisione è (per quanto Android permette di sapere)
    /// avvenuta davvero. Android NON dice se il messaggio è stato inviato; si controlla che:
    ///  (a) nel menu di condivisione il giocatore abbia scelto un'app (ricevuta del chooser, GotchaShare.java);
    ///  (b) dopo la scelta il gioco sia rimasto in secondo piano almeno 4 s (ShareVerifier.MinAwayMs).
    /// Se il menu viene chiuso, se si torna subito o se il menu non si apre, il risultato è false.
    /// Uso: ShareText(testo, titolo, ok => ...) per i testi; per i video ClipRecorder chiama Begin prima di aprire il
    /// menu di GotchaVideo (che passa dallo stesso chooser con ricevuta) e Abort se non si apre.
    /// Nell'Editor e sulle altre piattaforme il testo va negli appunti e la condivisione si considera riuscita (simulata).
    /// </summary>
    public class VerifiedShare : MonoBehaviour
    {
        static VerifiedShare instance;
        readonly ShareVerifier verifier = new ShareVerifier();
        Action<bool> onDone;
#if UNITY_ANDROID && !UNITY_EDITOR
        float nextPoll;
#else
        float simulateAt = -1f;
#endif

        /// <summary>Una condivisione è in attesa del controllo.</summary>
        public static bool Busy => instance != null && instance.verifier.Active;

        static VerifiedShare Ensure()
        {
            if (instance != null) return instance;
            var go = new GameObject("VerifiedShare");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<VerifiedShare>();
            return instance;
        }

        /// <summary>Inizia a seguire una condivisione (il menu si aprirà subito dopo). false se ce n'è già una in corso.</summary>
        public static bool Begin(Action<bool> done)
        {
            if (Busy) return false;
            VerifiedShare s = Ensure();
            s.onDone = done;
#if UNITY_ANDROID && !UNITY_EDITOR
            try { using (var cls = new AndroidJavaClass("com.gotcha.game.GotchaShare")) cls.CallStatic("begin"); }
            catch (Exception e) { Debug.LogWarning("[Gotcha] Ricevuta di condivisione non disponibile: " + e.Message); }
#else
            s.simulateAt = Time.unscaledTime + 0.6f;
#endif
            s.verifier.Begin(Now());
            return true;
        }

        /// <summary>Il menu non si è potuto aprire: la condivisione in corso finisce con false.</summary>
        public static void Abort()
        {
            if (instance == null || !instance.verifier.Active) return;
            instance.verifier.Cancel();
            instance.Finish(false);
        }

        /// <summary>Condivide un testo (link compreso) e chiama done(true) solo se la condivisione risulta avvenuta.</summary>
        public static void ShareText(string text, string chooserTitle, Action<bool> done)
        {
            if (!Begin(done)) { done?.Invoke(false); return; }
#if UNITY_ANDROID && !UNITY_EDITOR
            bool opened = false;
            try { using (var cls = new AndroidJavaClass("com.gotcha.game.GotchaShare")) opened = cls.CallStatic<bool>("shareText", text, chooserTitle); }
            catch (Exception e) { Debug.LogWarning("[Gotcha] Condivisione non riuscita: " + e.Message); }
            if (!opened) Abort();
#else
            GUIUtility.systemCopyBuffer = text;
            Debug.Log("[Gotcha] Condivisione simulata (copiata negli appunti): " + text);
#endif
        }

        void OnApplicationFocus(bool focus)
        {
            if (!verifier.Active) return;
            if (focus) verifier.OnReturned(Now()); else verifier.OnLeft(Now());
        }

        void OnApplicationPause(bool paused)
        {
            if (!verifier.Active) return;
            if (paused) verifier.OnLeft(Now()); else verifier.OnReturned(Now());
        }

        void Update()
        {
            if (!verifier.Active) return;
#if UNITY_ANDROID && !UNITY_EDITOR
            if (Time.unscaledTime < nextPoll) return;
            nextPoll = Time.unscaledTime + 0.1f;   // una chiamata JNI ogni 0,1 s mentre si aspetta
            long pickedAt = -1;
            try { using (var cls = new AndroidJavaClass("com.gotcha.game.GotchaShare")) pickedAt = cls.CallStatic<long>("pickedAt"); }
            catch { }
            ShareOutcome outcome = verifier.Poll(Now(), pickedAt >= 0, pickedAt);
#else
            // Editor: nessun menu vero, dopo un attimo la condivisione si considera riuscita (simulata)
            if (simulateAt < 0f || Time.unscaledTime < simulateAt) return;
            simulateAt = -1f;
            verifier.Cancel();
            ShareOutcome outcome = ShareOutcome.Verified;
#endif
            if (outcome != ShareOutcome.Pending) Finish(outcome == ShareOutcome.Verified);
        }

        void Finish(bool ok)
        {
            Action<bool> cb = onDone;
            onDone = null;
#if UNITY_ANDROID && !UNITY_EDITOR
            if (!ok) Debug.Log("[Gotcha] Condivisione non confermata (menu chiuso o ritorno troppo rapido).");
#endif
            cb?.Invoke(ok);
        }

        /// <summary>Orologio monotòno in ms: su Android lo stesso della ricevuta (elapsedRealtime).</summary>
        static long Now()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try { using (var cls = new AndroidJavaClass("com.gotcha.game.GotchaShare")) return cls.CallStatic<long>("now"); }
            catch { }
#endif
            return (long)(Time.realtimeSinceStartupAsDouble * 1000.0);
        }
    }
}
