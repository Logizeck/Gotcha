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

        /// <summary>Build di prova: tutti i livelli giocabili dalla selezione (il progresso vero resta salvato).</summary>
        public const bool UnlockAllLevels = true;

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
        LevelResult lastResult;
        bool paused;

        public LevelDefinition GetDefinition(int index)
        {
            return index >= 0 && index < LevelCount ? catalog.levels[index] : null;
        }

        void Awake()
        {
            Instance = this;
            Ads = new AdBreakManager(new SimulatedAdProvider());   // pubblicità simulata finché non c'è AdMob
            Ads.AdsRemoved = () => SaveData.AdsRemoved;
            Lives = GetComponent<LivesManager>();
            if (Lives == null) Lives = gameObject.AddComponent<LivesManager>();
            if (GetComponent<StoreManager>() == null) gameObject.AddComponent<StoreManager>();   // acquisti in-app
            // Mentre il gioco è aperto lo schermo non si spegne e non si abbassa (molti livelli si giocano
            // senza toccarlo: inclinazione, attesa...). Su Android è il flag KEEP_SCREEN_ON.
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
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
            if (current == null) return;
            current.Finished -= OnFinished;
            Destroy(current.gameObject);
            current = null;
            CurrentDefinition = null;
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
            GameObject go = Instantiate(CurrentDefinition.prefab, levelHost);
            current = go.GetComponent<LevelController>();
            if (current == null)
            {
                Debug.LogError("Il prefab del livello '" + CurrentDefinition.title + "' non ha un LevelController.");
                return;
            }
            current.Finished += OnFinished;
            current.OnLevelStart();
            LevelStarted?.Invoke(CurrentDefinition, index);
        }

        void OnFinished(LevelResult result, string eggId)
        {
            lastResult = result;
            Ads.RegisterLevelResult(result == LevelResult.Solved);
            if (result == LevelResult.Solved) SaveData.UnlockUpTo(CurrentIndex + 1);
            if (!string.IsNullOrEmpty(eggId)) SaveData.AddEasterEgg(eggId);
            if (result == LevelResult.Failed && (string.IsNullOrEmpty(eggId) || easterEggCostsLife)) Lives.LoseLife();
            LevelFinished?.Invoke(CurrentDefinition, result, eggId);
        }

        /// <summary>
        /// Da chiamare quando il giocatore preme "Avanti"/"Riprova" (dopo l'eventuale schermata di condivisione).
        /// Se è dovuta, la pubblicità parte qui, cioè sempre tra un livello e l'altro.
        /// </summary>
        public void Continue()
        {
            int next = lastResult == LevelResult.Solved ? CurrentIndex + 1 : CurrentIndex;
            if (next >= LevelCount)
            {
                GameCompleted?.Invoke();
                return;
            }
            Ads.TryShowBreak(() => TryStartLevel(next));
        }
    }
}
