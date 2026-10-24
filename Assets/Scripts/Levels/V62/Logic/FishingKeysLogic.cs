namespace Gotcha.Levels.V62
{
    /// <summary>Stati del livello 251 (la chiave nello scarico).</summary>
    public enum FishingKeysState
    {
        Idle,       // canna appoggiata, chiave giù nello scarico
        Casting,    // la canna è calata nello scarico (animazione, input bloccato)
        KeyOut,     // la chiave è uscita attaccata alla calamita: si può prendere
        HoldingKey, // il giocatore trascina la chiave
        Opening,    // chiave nella serratura: la porta si apre (input bloccato)
        Done,
    }

    /// <summary>Cosa c'è appeso all'amo della canna.</summary>
    public enum HookBait { None, Magnet, Fish }

    /// <summary>Cosa esce dallo scarico calando la canna.</summary>
    public enum CastResult { Nothing, Boot, Key, Piranha }

    /// <summary>
    /// Logica pura del livello 251 "Fishing for keys": la chiave è caduta nello scarico del lavandino. Le dita non ci
    /// passano; la canna da sola pesca uno stivale. Soluzione: la calamita sull'amo, poi la canna nello scarico: la
    /// chiave sale attaccata alla calamita e va portata nella serratura. Easter egg: il pesce della ciotola del gatto
    /// sull'amo fa uscire un piranha sorridente (poi si continua). Nessuna dipendenza da Unity.
    /// </summary>
    public sealed class FishingKeysLogic
    {
        public const string EggId = "v62_fish_in_sink";

        public FishingKeysState State { get; private set; } = FishingKeysState.Idle;
        public HookBait Bait { get; private set; } = HookBait.None;
        public CastResult LastCast { get; private set; } = CastResult.Nothing;
        public int Boots { get; private set; }
        public int FingerTries { get; private set; }
        public bool EggFound { get; private set; }
        public bool CompletionReported { get; private set; }

        public bool InputLocked => State == FishingKeysState.Casting || State == FishingKeysState.Opening || State == FishingKeysState.Done;
        /// <summary>La chiave è ancora giù nello scarico?</summary>
        public bool KeyInDrain => State == FishingKeysState.Idle || State == FishingKeysState.Casting && LastCast != CastResult.Key;

        public void Reset()
        {
            State = FishingKeysState.Idle;
            Bait = HookBait.None;
            LastCast = CastResult.Nothing;
            Boots = 0;
            FingerTries = 0;
            EggFound = false;
            CompletionReported = false;
        }

        /// <summary>Il dito nello scarico: "non ci passa". True se la battuta va mostrata (chiave ancora giù).</summary>
        public bool TryFinger()
        {
            if (State != FishingKeysState.Idle) return false;
            FingerTries++;
            return true;
        }

        /// <summary>Appende qualcosa all'amo (solo se è vuoto e la canna è ferma).</summary>
        public bool TryAttach(HookBait bait)
        {
            if (State != FishingKeysState.Idle || bait == HookBait.None || Bait != HookBait.None) return false;
            Bait = bait;
            return true;
        }

        /// <summary>Stacca quello che c'è sull'amo (torna al suo posto). Restituisce cosa è stato staccato.</summary>
        public HookBait TryDetach()
        {
            if (State != FishingKeysState.Idle || Bait == HookBait.None) return HookBait.None;
            HookBait b = Bait;
            Bait = HookBait.None;
            return b;
        }

        /// <summary>Cala la canna nello scarico: cosa viene su dipende da cosa c'è sull'amo.</summary>
        public CastResult TryCast()
        {
            if (State != FishingKeysState.Idle) return CastResult.Nothing;
            State = FishingKeysState.Casting;
            switch (Bait)
            {
                case HookBait.Magnet: LastCast = CastResult.Key; break;
                case HookBait.Fish: LastCast = CastResult.Piranha; break;
                default: LastCast = CastResult.Boot; break;
            }
            return LastCast;
        }

        /// <summary>
        /// Fine dell'animazione della pesca. Stivale: torna giù, si riprova. Piranha: si mangia il pesce (che
        /// ricompare nella ciotola), saluta ed è l'Easter egg. Chiave: resta attaccata alla calamita, pronta da prendere.
        /// </summary>
        public bool TryFinishCast()
        {
            if (State != FishingKeysState.Casting) return false;
            switch (LastCast)
            {
                case CastResult.Key:
                    State = FishingKeysState.KeyOut;
                    break;
                case CastResult.Piranha:
                    EggFound = true;
                    Bait = HookBait.None;
                    State = FishingKeysState.Idle;
                    break;
                default:
                    Boots++;
                    State = FishingKeysState.Idle;
                    break;
            }
            return true;
        }

        public bool TryPickKey()
        {
            if (State != FishingKeysState.KeyOut) return false;
            State = FishingKeysState.HoldingKey;
            return true;
        }

        /// <summary>Rilascio della chiave: true se è sulla serratura (la porta si apre), altrimenti torna sull'amo.</summary>
        public bool TryDropKey(bool onLock)
        {
            if (State != FishingKeysState.HoldingKey) return false;
            State = onLock ? FishingKeysState.Opening : FishingKeysState.KeyOut;
            return onLock;
        }

        /// <summary>Fine dell'animazione della porta: true una sola volta (è il momento della vittoria).</summary>
        public bool TryCompleteOpening()
        {
            if (State != FishingKeysState.Opening || CompletionReported) return false;
            State = FishingKeysState.Done;
            CompletionReported = true;
            return true;
        }
    }
}
