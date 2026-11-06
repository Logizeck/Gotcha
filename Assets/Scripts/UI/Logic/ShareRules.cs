namespace Gotcha.UI
{
    /// <summary>Esito del controllo di una condivisione.</summary>
    public enum ShareOutcome { Pending, Verified, Rejected }

    /// <summary>
    /// Logica pura (senza Unity) del controllo "la condivisione è avvenuta davvero?".
    /// Android non dice mai se il messaggio è stato INVIATO: il menu di condivisione di sistema non lo riporta.
    /// Il massimo che si può sapere senza un server è:
    ///  (a) QUALE app il giocatore ha scelto nel menu (Intent.createChooser con IntentSender: il sistema avvisa
    ///      il nostro receiver solo se si tocca un'app; se il menu viene chiuso non arriva niente);
    ///  (b) che dopo la scelta il gioco è rimasto in secondo piano almeno <see cref="MinAwaySeconds"/> secondi
    ///      (il giocatore ha passato del tempo nell'app scelta, non l'ha aperta e chiusa al volo).
    /// La condivisione conta solo con entrambe. Tutti i tempi sono in millisecondi dello stesso orologio monotòno
    /// (Android SystemClock.elapsedRealtime, che l'utente non può spostare).
    /// </summary>
    public sealed class ShareVerifier
    {
        public const long MinAwayMs = 4000;
        /// <summary>Dopo il ritorno nel gioco, quanto si aspetta ancora l'avviso della scelta (arriva in un broadcast).</summary>
        public const long PickGraceMs = 1500;
        /// <summary>Se il menu non si apre (il gioco non perde mai il fuoco) entro questo tempo, si rinuncia.</summary>
        public const long OpenTimeoutMs = 8000;
        public const float MinAwaySeconds = MinAwayMs / 1000f;

        long startedAt = -1, leftAt = -1, returnedAt = -1;
        ShareOutcome outcome = ShareOutcome.Rejected;

        public ShareOutcome Outcome => outcome;
        public bool Active => outcome == ShareOutcome.Pending;

        /// <summary>La regola: app scelta dopo l'apertura del menu, e ritorno almeno 4 s dopo la scelta.</summary>
        public static bool IsVerified(bool picked, long pickedAt, long startedAt, long returnedAt)
            => picked && startedAt >= 0 && pickedAt >= startedAt && returnedAt >= 0 && returnedAt - pickedAt >= MinAwayMs;

        /// <summary>Il menu di condivisione sta per aprirsi.</summary>
        public void Begin(long now)
        {
            startedAt = now;
            leftAt = returnedAt = -1;
            outcome = ShareOutcome.Pending;
        }

        /// <summary>Annulla (es. il menu non si è potuto aprire).</summary>
        public void Cancel() { if (outcome == ShareOutcome.Pending) outcome = ShareOutcome.Rejected; }

        /// <summary>Il gioco ha perso il fuoco o è andato in pausa (si è aperto il menu o l'app scelta).</summary>
        public void OnLeft(long now)
        {
            if (outcome != ShareOutcome.Pending) return;
            if (leftAt < 0) leftAt = now;
            returnedAt = -1;   // se torna e riparte (menu chiuso e riaperto da un'altra app) conta l'ultimo ritorno
        }

        /// <summary>Il gioco è di nuovo in primo piano.</summary>
        public void OnReturned(long now)
        {
            if (outcome != ShareOutcome.Pending || leftAt < 0 || returnedAt >= 0) return;
            returnedAt = now;
        }

        /// <summary>
        /// Da chiamare a ogni fotogramma mentre è attivo: decide appena può.
        /// picked/pickedAt: cosa ha registrato il receiver della scelta (pickedAt nello stesso orologio).
        /// </summary>
        public ShareOutcome Poll(long now, bool picked, long pickedAt)
        {
            if (outcome != ShareOutcome.Pending) return outcome;
            if (leftAt < 0)
            {
                if (now - startedAt > OpenTimeoutMs) outcome = ShareOutcome.Rejected;   // il menu non si è mai aperto
                return outcome;
            }
            if (returnedAt < 0) return outcome;   // ancora fuori dal gioco
            if (IsVerified(picked, pickedAt, startedAt, returnedAt)) outcome = ShareOutcome.Verified;
            // menu chiuso senza scegliere, o app scelta ma tornati troppo presto: si aspetta ancora un attimo (l'avviso
            // della scelta arriva in un broadcast; un ritorno "di passaggio" tra il menu e l'app scelta non deve
            // bocciare), poi si rinuncia
            else if (now - returnedAt > PickGraceMs) outcome = ShareOutcome.Rejected;
            return outcome;
        }
    }

    /// <summary>
    /// "Ti è piaciuto? Condividilo con un amico: +2 vite". Logica pura di quando proporlo e del limite giornaliero.
    /// Scelte: si propone solo sui livelli superati "memorabili" (quelli con la festa LevelReward della roadmap) o
    /// ogni 5 livelli superati; mai se le vite sono piene o infinite (il premio non servirebbe), mai oltre 3 premi
    /// al giorno. Le 2 vite si aggiungono fino al massimo normale (LivesManager.GrantLife le limita a MaxLives).
    /// </summary>
    public static class ViralShareRules
    {
        public const int RewardLives = 2;
        public const int DailyCap = 3;
        public const int EverySolved = 5;

        /// <summary>Premi già presi oggi, dati il giorno e il conteggio salvati. Se l'orologio è tornato indietro
        /// rispetto all'ultimo premio, oggi non se ne danno più (niente premi spostando la data avanti e indietro).</summary>
        public static int UsedToday(int storedDay, int storedCount, int today)
        {
            if (storedCount < 0) storedCount = 0;
            if (today == storedDay) return storedCount;
            if (today < storedDay) return DailyCap;
            return 0;
        }

        public static bool CapReached(int usedToday) => usedToday >= DailyCap;

        /// <summary>Il livello appena superato merita la proposta? (memorabile, o ogni 5 vittorie)</summary>
        public static bool IsMoment(bool memorable, int solvedCounter) => memorable || (solvedCounter > 0 && solvedCounter % EverySolved == 0);

        public static bool ShouldOffer(bool solved, bool memorable, int solvedCounter, int lives, int maxLives, bool infinite, int usedToday, bool eggShareShown)
            => solved && !eggShareShown && IsMoment(memorable, solvedCounter) && !infinite && lives < maxLives && !CapReached(usedToday);

        /// <summary>Vite dopo il premio (limitate al massimo).</summary>
        public static int LivesAfterReward(int lives, int maxLives)
        {
            int n = lives + RewardLives;
            return n > maxLives ? maxLives : n;
        }
    }
}
