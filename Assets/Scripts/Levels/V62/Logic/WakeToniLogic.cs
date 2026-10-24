namespace Gotcha.Levels.V62
{
    /// <summary>Stati del livello 252 (svegliare Toni).</summary>
    public enum WakeToniState
    {
        Sleeping,       // Toni dorme, il biscotto è sul ripiano
        HoldingCookie,  // il giocatore trascina il biscotto
        CatStealing,    // il gatto ha rubato il biscotto (torna sul ripiano dopo un po')
        Waking,         // Toni annusa il biscotto e si sveglia (input bloccato)
        Done,
    }

    /// <summary>Cosa fa Toni (o il gatto) dopo un'azione del giocatore.</summary>
    public enum WakeReaction { None, SlapsAlarm, TurnsOver, CatSteals, CookieBack, Wakes }

    /// <summary>
    /// Logica pura del livello 252 "Wake up, Toni!": la sveglia la prende a schiaffi, la lampada lo fa solo girare e
    /// il biscotto, se il gatto è libero, se lo prende il gatto. Soluzione: la piuma al gatto (gioca e resta occupato),
    /// poi il biscotto sotto il naso di Toni. Easter egg: la piuma su Toni strofinata avanti e indietro 3 volte lo fa
    /// starnutire nel sonno. Nessuna dipendenza da Unity.
    /// </summary>
    public sealed class WakeToniLogic
    {
        public const string EggId = "v62_feather_tickle";
        /// <summary>Strofinate (cambi di direzione della piuma su Toni) per lo starnuto.</summary>
        public const int RubsForSneeze = 3;

        public WakeToniState State { get; private set; } = WakeToniState.Sleeping;
        public bool CatBusy { get; private set; }
        public bool LampOn { get; private set; }
        /// <summary>Lato verso cui dorme Toni (cambia ogni volta che si gira).</summary>
        public bool FacingLeft { get; private set; } = true;
        public int Snore { get; private set; } = 1;
        public int Rubs { get; private set; }
        public int Steals { get; private set; }
        public bool EggFound { get; private set; }
        public bool CompletionReported { get; private set; }

        public bool InputLocked => State == WakeToniState.Waking || State == WakeToniState.Done;

        public void Reset()
        {
            State = WakeToniState.Sleeping;
            CatBusy = false;
            LampOn = false;
            FacingLeft = true;
            Snore = 1;
            Rubs = 0;
            Steals = 0;
            EggFound = false;
            CompletionReported = false;
        }

        /// <summary>La sveglia suona: Toni la scaccia con una zampata e russa più forte.</summary>
        public WakeReaction TapAlarm()
        {
            if (InputLocked) return WakeReaction.None;
            if (Snore < 3) Snore++;
            return WakeReaction.SlapsAlarm;
        }

        /// <summary>La lampada si accende/spegne: accesa, Toni si gira dall'altra parte.</summary>
        public WakeReaction TapLamp()
        {
            if (InputLocked) return WakeReaction.None;
            LampOn = !LampOn;
            if (!LampOn) return WakeReaction.None;
            FacingLeft = !FacingLeft;
            return WakeReaction.TurnsOver;
        }

        /// <summary>La piuma sul gatto: ci gioca e resta occupato finché non gliela si toglie.</summary>
        public bool TryGiveFeatherToCat()
        {
            if (InputLocked || CatBusy) return false;
            CatBusy = true;
            Rubs = 0;
            return true;
        }

        /// <summary>La piuma viene ripresa dal gatto: il gatto torna libero.</summary>
        public bool TryTakeFeatherFromCat()
        {
            if (InputLocked || !CatBusy) return false;
            CatBusy = false;
            return true;
        }

        /// <summary>
        /// Una strofinata della piuma su Toni (un cambio di direzione). True quando arriva allo starnuto: la prima
        /// volta è l'Easter egg (Toni continua a dormire), e il conteggio riparte.
        /// </summary>
        public bool TryRub()
        {
            if (InputLocked || CatBusy) return false;
            Rubs++;
            if (Rubs < RubsForSneeze) return false;
            Rubs = 0;
            EggFound = true;
            return true;
        }

        /// <summary>La piuma si allontana da Toni: le strofinate ripartono da zero.</summary>
        public void EndRub() { Rubs = 0; }

        public bool TryPickCookie()
        {
            if (State != WakeToniState.Sleeping) return false;
            State = WakeToniState.HoldingCookie;
            return true;
        }

        /// <summary>
        /// Il biscotto arriva sotto il naso di Toni (o viene lasciato lì vicino): col gatto libero lo ruba il gatto,
        /// col gatto occupato Toni lo annusa e si sveglia.
        /// </summary>
        public WakeReaction CookieAtToni()
        {
            if (State != WakeToniState.HoldingCookie) return WakeReaction.None;
            if (!CatBusy)
            {
                Steals++;
                State = WakeToniState.CatStealing;
                return WakeReaction.CatSteals;
            }
            State = WakeToniState.Waking;
            return WakeReaction.Wakes;
        }

        /// <summary>Il biscotto lasciato altrove torna sul ripiano.</summary>
        public WakeReaction DropCookieElsewhere()
        {
            if (State != WakeToniState.HoldingCookie) return WakeReaction.None;
            State = WakeToniState.Sleeping;
            return WakeReaction.CookieBack;
        }

        /// <summary>Il biscotto rubato ricompare sul ripiano.</summary>
        public bool TryRespawnCookie()
        {
            if (State != WakeToniState.CatStealing) return false;
            State = WakeToniState.Sleeping;
            return true;
        }

        public bool TryCompleteWake()
        {
            if (State != WakeToniState.Waking || CompletionReported) return false;
            State = WakeToniState.Done;
            CompletionReported = true;
            return true;
        }
    }
}
