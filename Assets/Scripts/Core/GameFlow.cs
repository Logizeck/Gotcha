using System;
using UnityEngine;
using Gotcha.Ads;
using Gotcha.Levels;

namespace Gotcha.Core
{
    /// <summary>
    /// Regista del gioco: carica i livelli dal catalogo, raccoglie l'esito,
    /// salva i progressi e gestisce le pause pubblicitarie tra un livello e l'altro.
    /// La UI (menu, hint, schermata di condivisione) si aggancia agli eventi.
    /// </summary>
    public class GameFlow : MonoBehaviour
    {
        public static GameFlow Instance { get; private set; }

        /// <summary>I livelli si sbloccano solo superandoli (anche per i beta tester).</summary>
        public const bool UnlockAllLevels = false;

        /// <summary>Tutti i livelli giocabili dall'elenco: build di prova o password beta attiva (per i tester).</summary>
        public static bool AllLevelsOpen => UnlockAllLevels || BetaMode.Active;

        [SerializeField] LevelCatalog catalog;
        [SerializeField] Transform levelHost;

        public event Action<LevelDefinition, int> LevelStarted;
        public event Action<LevelDefinition, LevelResult, string> LevelFinished;
        public event Action GameCompleted;
        /// <summary>Si voleva giocare ma le vite sono finite: la UI mostra il pannello (attesa, pubblicità, invito).</summary>
        public event Action<int> OutOfLives;

        [Tooltip("Se falso, i fallimenti che svelano un Easter egg non costano vite (sono una scoperta, non un errore).")]
        [SerializeField] bool easterEggCostsLife = false;

        public LivesManager Lives { get; private set; }

        /// <summary>Pausa (menu, hint): ferma il tempo di gioco, quindi timer e animazioni dei livelli.</summary>
        public bool Paused
        {
            get => paused;
            set { paused = value; Time.timeScale = value ? 0f : 1f; }
        }
        public int CurrentIndex { get; private set; } = -1;
        public int LevelCount => catalog != null ? catalog.levels.Count : 0;
        public AdBreakManager Ads { get; private set; }
        public LevelDefinition CurrentDefinition { get; private set; }

        LevelController current;
        /// <summary>Il livello in corso (null se non se ne sta giocando uno).</summary>
        public LevelController Current => current;
        LevelResult lastResult;
        bool paused;

        public LevelDefinition GetDefinition(int index)
        {
            return index >= 0 && index < LevelCount ? catalog.levels[index] : null;
        }

        void Awake()
        {
            Instance = this;
            Ads = new AdBreakManager(CreateAdProvider());
            Ads.AdsRemoved = () => SaveData.AdsRemoved || BetaMode.Active;
            Lives = GetComponent<LivesManager>();
            if (Lives == null) Lives = gameObject.AddComponent<LivesManager>();
            if (GetComponent<StoreManager>() == null) gameObject.AddComponent<StoreManager>();   // acquisti in-app
            if (GetComponent<ComebackNotifier>() == null) gameObject.AddComponent<ComebackNotifier>();   // "I'm missing you! <3"
            // Mentre il gioco è aperto lo schermo non si spegne e non si abbassa (molti livelli si giocano
            // senza toccarlo: inclinazione, attesa...). Su Android è il flag KEEP_SCREEN_ON.
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            LockPortrait();
        }

        /// <summary>
        /// Solo verticale: molti livelli si giocano inclinando il telefono e, con la rotazione automatica attiva,
        /// lo schermo girerebbe. Vale anche se le impostazioni del Player del progetto fossero rimaste su Auto Rotation.
        /// </summary>
        static void LockPortrait()
        {
            Screen.autorotateToPortrait = false;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = false;
            Screen.autorotateToLandscapeRight = false;
            Screen.orientation = ScreenOrientation.Portrait;
        }

        /// <summary>AdMob se il plugin è installato (define GOTCHA_ADMOB); nella build per lo store senza AdMob nessuna
        /// pubblicità; nelle build di prova la pubblicità simulata.</summary>
        static IAdProvider CreateAdProvider()
        {
#if GOTCHA_ADMOB && !UNITY_EDITOR
            return new AdMobProvider();
#elif GOTCHA_RELEASE
            return new NoAdsProvider();
#else
            return new SimulatedAdProvider();
#endif
        }

        void OnApplicationFocus(bool focus)
        {
            if (focus) Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
            Time.timeScale = 1f;
        }

        /// <summary>Primo livello non ancora risolto (per "Continua").</summary>
        public int ResumeIndex => Mathf.Clamp(SaveData.HighestUnlocked, 0, Mathf.Max(0, LevelCount - 1));

        // All'avvio non parte nessun livello: la UI mostra la schermata del titolo.

        /// <summary>Chiude il livello in corso (si torna alla schermata del titolo).</summary>
        public void StopLevel()
        {
            Paused = false;
            if (ClipRecorder.Instance != null) ClipRecorder.Instance.Discard();
            if (current == null) return;
            current.Finished -= OnFinished;
            Destroy(current.gameObject);
            current = null;
            CurrentDefinition = null;
            if (MusicPlayer.Instance != null) MusicPlayer.Instance.Play(MusicTrack.Title);   // di nuovo al titolo
        }

        /// <summary>Avvia un livello solo se c'è almeno una vita; altrimenti avvisa la UI.</summary>
        public bool TryStartLevel(int index)
        {
            if (!Lives.CanPlay) { OutOfLives?.Invoke(index); return false; }
            StartLevel(index);
            return true;
        }

        void Update()
        {
            bool playing = current != null && !current.IsFinished && !Paused && Application.isFocused;
            Ads.Tick(Time.unscaledDeltaTime, playing);
        }

        public void StartLevel(int index)
        {
            if (index < 0 || index >= LevelCount) return;
            Paused = false;

            if (current != null)
            {
                current.Finished -= OnFinished;
                Destroy(current.gameObject);
                current = null;
            }

            CurrentIndex = index;
            CurrentDefinition = catalog.levels[index];
            // musica del livello: se cambia, quella vecchia sfuma e la nuova entra; se è la stessa continua in loop
            if (MusicPlayer.Instance != null) MusicPlayer.Instance.Play(CurrentDefinition.music);
            GameObject go = Instantiate(CurrentDefinition.prefab, levelHost);
            current = go.GetComponent<LevelController>();
            if (current == null)
            {
                Debug.LogError("Il prefab del livello '" + CurrentDefinition.title + "' non ha un LevelController.");
                return;
            }
            current.Finished += OnFinished;
            current.OnLevelStart();
            // si registra sempre: se salta fuori un Easter egg, il video (con l'audio) è già pronto da condividere
            if (ClipRecorder.Instance != null) ClipRecorder.Instance.Begin();
            LevelStarted?.Invoke(CurrentDefinition, index);
        }

        void OnFinished(LevelResult result, string eggId)
        {
            lastResult = result;
            if (ClipRecorder.Instance != null)
            {
                if (string.IsNullOrEmpty(eggId)) ClipRecorder.Instance.Discard();
                else ClipRecorder.Instance.Stop();   // la clip resta pronta per la condivisione
            }
            Ads.RegisterLevelResult(result == LevelResult.Solved);
            if (result == LevelResult.Solved)
            {
                SaveData.UnlockUpTo(CurrentIndex + 1);
                DailyReward.RegisterSolved();   // obiettivo del giorno: 5 livelli superati
                if (ComebackNotifier.Instance != null) ComebackNotifier.Instance.AskPermissionOnce();
            }
            if (!string.IsNullOrEmpty(eggId)) SaveData.AddEasterEgg(eggId);
            if (result == LevelResult.Failed && (string.IsNullOrEmpty(eggId) || easterEggCostsLife)) Lives.LoseLife();
            LevelFinished?.Invoke(CurrentDefinition, result, eggId);
        }

        /// <summary>
        /// Da chiamare quando il giocatore preme "Avanti"/"Riprova" (dopo l'eventuale schermata di condivisione).
        /// Se è dovuta (5 livelli superati o 140 s di gioco dall'ultima), la pubblicità parte qui, alla fine del
        /// livello, sia dopo una vittoria sia dopo una sconfitta.
        /// </summary>
        public void Continue()
        {
            int next = lastResult == LevelResult.Solved ? CurrentIndex + 1 : CurrentIndex;
            if (next >= LevelCount)
            {
                if (MusicPlayer.Instance != null) MusicPlayer.Instance.Play(MusicTrack.Title);
                GameCompleted?.Invoke();
                return;
            }
            Ads.TryShowBreak(() => TryStartLevel(next));
        }
    }
}
