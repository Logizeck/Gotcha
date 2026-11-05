namespace Gotcha.Levels.V64
{
    /// <summary>Fasi del livello "The Honest Thief".</summary>
    public enum HonestThiefStage
    {
        Playing,      // si prova di tutto
        Celebrating,  // il ladro ha il diamante: festa (input bloccato)
        Done,         // vittoria già data
    }

    /// <summary>Dov'è il cartellino FREE.</summary>
    public enum HonestThiefSignAt
    {
        Home,     // appeso al cestino dei volantini gratis
        Held,     // il dito lo sta trascinando
        Case,     // agganciato alla teca, sopra la targhetta: la teca ora dice FREE (resta lì: blocco al posto)
    }

    /// <summary>Dov'è il diamante.</summary>
    public enum HonestThiefDiamondAt
    {
        Case,     // sul cuscino nella teca
        Held,     // il dito lo sta trascinando (solo dopo il cartellino FREE)
        Thief,    // in mano al ladro (vittoria)
    }

    /// <summary>Tocco sul diamante o sulla teca.</summary>
    public enum HonestThiefTap
    {
        None,        // input bloccato
        Alarm,       // l'allarme parte (sirena, lampo, suono): si spegne da solo dopo AlarmSeconds
        AlarmAgain,  // l'allarme suonava già: riparte da capo (niente di nuovo, mai bloccante)
        Free,        // la teca è FREE: niente allarme ("prendilo pure")
    }

    /// <summary>Tentativo di trascinare il diamante.</summary>
    public enum HonestThiefGrab
    {
        None,      // input bloccato o il dito tiene già qualcosa
        Guard,     // prima del cartellino: arriva la guardia ("Hey! Hands off!"), il diamante torna al suo posto
        Grabbed,   // dopo il cartellino: il diamante si stacca e segue il dito
    }

    /// <summary>Il diamante lasciato andare.</summary>
    public enum HonestThiefDiamondDrop
    {
        None,      // input bloccato o diamante non tenuto
        Returned,  // lasciato lontano dal ladro: torna sul cuscino
        Taken,     // lasciato sul ladro: VITTORIA (una volta sola)
    }

    /// <summary>Il cartellino lasciato andare.</summary>
    public enum HonestThiefSignDrop
    {
        None,      // input bloccato o cartellino non tenuto
        Returned,  // lasciato lontano dalla teca: torna appeso al cestino
        Placed,    // lasciato sulla teca: si aggancia alla targhetta, la teca è FREE (allarme spento)
    }

    /// <summary>Cosa è finito durante un passo di tempo (si possono combinare).</summary>
    [System.Flags]
    public enum HonestThiefTick
    {
        None = 0,
        AlarmOff = 1,    // la sirena si è spenta da sola
        GuardLeft = 2,   // la guardia se n'è andata
    }

    /// <summary>
    /// Logica pura del livello 15 della roadmap "The Honest Thief" (id honest_thief) — "STEAL THE DIAMOND." Un diamante
    /// in una teca di vetro con l'allarme (sirena in cima), un ladro da fumetto (maglia a righe, mascherina) e, di lato,
    /// un cestino di volantini gratis con un cartellino appeso "FREE".
    /// Inganno: toccare diamante o teca fa partire l'allarme (si spegne da solo dopo <see cref="AlarmSeconds"/>, nessuna
    /// sconfitta); provare a trascinare il diamante fa arrivare la guardia ("Hey! Hands off!"), il diamante torna al suo
    /// posto e la guardia se ne va dopo <see cref="GuardSeconds"/>. Toccare il ladro: "Psst. Any ideas?".
    /// Soluzione esatta: trascinare il cartellino FREE sulla teca (zona generosa, <see cref="InZone"/> con
    /// <see cref="CaseMargin"/>): si aggancia sopra la targhetta "PRICELESS" e la teca diventa FREE. Scelta documentata:
    /// BLOCCO AL POSTO (una volta agganciato il cartellino resta lì; nessun vicolo cieco, perché da lì in poi la
    /// soluzione è sempre disponibile). Solo ALLORA il diamante si stacca: portato sul ladro (<see cref="ThiefMargin"/>)
    /// -> vittoria una volta sola; lasciato prima torna sul cuscino. Il cartellino lasciato altrove torna al cestino.
    /// Un dito alla volta; Cancel() (pausa, app in background, secondo dito) rimette a casa quello che si teneva.
    /// Non si può perdere. Nessuna dipendenza da Unity.
    /// </summary>
    public sealed class HonestThiefLogic
    {
        /// <summary>Secondi in cui suona l'allarme dopo l'ultimo tocco.</summary>
        public const float AlarmSeconds = 1.5f;
        /// <summary>Secondi in cui la guardia resta in scena dopo l'ultimo tentativo.</summary>
        public const float GuardSeconds = 1.6f;
        /// <summary>Margine attorno alla teca (vetro + piedistallo) per agganciare il cartellino.</summary>
        public const float CaseMargin = 0.35f;
        /// <summary>Margine attorno al ladro per consegnargli il diamante.</summary>
        public const float ThiefMargin = 0.15f;

        public HonestThiefStage Stage { get; private set; } = HonestThiefStage.Playing;
        public HonestThiefSignAt SignAt { get; private set; } = HonestThiefSignAt.Home;
        public HonestThiefDiamondAt DiamondAt { get; private set; } = HonestThiefDiamondAt.Case;
        public bool AlarmOn => alarmT > 0f;
        public bool GuardOn => guardT > 0f;
        /// <summary>Secondi che restano all'allarme / alla guardia (0 se spenti).</summary>
        public float AlarmLeft => alarmT;
        public float GuardLeft => guardT;

        public int Alarms { get; private set; }
        public int GuardCalls { get; private set; }
        public int SignReturns { get; private set; }
        public int DiamondReturns { get; private set; }
        public int ThiefTaps { get; private set; }
        public int SignTaps { get; private set; }
        public int BasketTaps { get; private set; }
        public int FreeTaps { get; private set; }
        public int WrongMoves => Alarms + GuardCalls + SignReturns + DiamondReturns;

        public bool InputLocked => Stage != HonestThiefStage.Playing;
        public bool Holding => SignAt == HonestThiefSignAt.Held || DiamondAt == HonestThiefDiamondAt.Held;
        /// <summary>La teca dice FREE (cartellino agganciato): il diamante si può prendere.</summary>
        public bool CaseIsFree => SignAt == HonestThiefSignAt.Case;

        float alarmT, guardT;
        bool completed;

        public void Reset()
        {
            Stage = HonestThiefStage.Playing;
            SignAt = HonestThiefSignAt.Home;
            DiamondAt = HonestThiefDiamondAt.Case;
            alarmT = guardT = 0f;
            Alarms = GuardCalls = SignReturns = DiamondReturns = ThiefTaps = SignTaps = BasketTaps = FreeTaps = 0;
            completed = false;
        }

        /// <summary>Il tempo passa: allarme e guardia si spengono da soli.</summary>
        public HonestThiefTick Tick(float dt)
        {
            if (dt <= 0f) return HonestThiefTick.None;
            HonestThiefTick e = HonestThiefTick.None;
            if (alarmT > 0f)
            {
                alarmT -= dt;
                if (alarmT <= 0f) { alarmT = 0f; e |= HonestThiefTick.AlarmOff; }
            }
            if (guardT > 0f)
            {
                guardT -= dt;
                if (guardT <= 0f) { guardT = 0f; e |= HonestThiefTick.GuardLeft; }
            }
            return e;
        }

        // ---------- Le false piste ----------

        /// <summary>Tocco (senza trascinare) sul diamante o sulla teca.</summary>
        public HonestThiefTap TapCase()
        {
            if (InputLocked || Holding) return HonestThiefTap.None;
            if (CaseIsFree) { FreeTaps++; return HonestThiefTap.Free; }
            bool again = AlarmOn;
            Alarms++;
            alarmT = AlarmSeconds;
            return again ? HonestThiefTap.AlarmAgain : HonestThiefTap.Alarm;
        }

        public bool TapThief()
        {
            if (InputLocked) return false;
            ThiefTaps++;
            return true;
        }

        /// <summary>Tocco (senza trascinare) sul cartellino ancora appeso.</summary>
        public bool TapSign()
        {
            if (InputLocked || Holding) return false;
            SignTaps++;
            return true;
        }

        public bool TapBasket()
        {
            if (InputLocked) return false;
            BasketTaps++;
            return true;
        }

        // ---------- Il diamante ----------

        /// <summary>Il dito comincia a trascinare il diamante: prima del cartellino arriva la guardia, dopo si stacca.</summary>
        public HonestThiefGrab GrabDiamond()
        {
            if (InputLocked || Holding || DiamondAt != HonestThiefDiamondAt.Case) return HonestThiefGrab.None;
            if (!CaseIsFree)
            {
                GuardCalls++;
                guardT = GuardSeconds;
                return HonestThiefGrab.Guard;
            }
            DiamondAt = HonestThiefDiamondAt.Held;
            return HonestThiefGrab.Grabbed;
        }

        /// <summary>Diamante lasciato: sul ladro vince, altrove torna sul cuscino.</summary>
        public HonestThiefDiamondDrop ReleaseDiamond(bool onThief)
        {
            if (InputLocked || DiamondAt != HonestThiefDiamondAt.Held) return HonestThiefDiamondDrop.None;
            if (!onThief)
            {
                DiamondAt = HonestThiefDiamondAt.Case;
                DiamondReturns++;
                return HonestThiefDiamondDrop.Returned;
            }
            DiamondAt = HonestThiefDiamondAt.Thief;
            Stage = HonestThiefStage.Celebrating;
            alarmT = 0f;
            return HonestThiefDiamondDrop.Taken;
        }

        // ---------- Il cartellino FREE ----------

        /// <summary>Il dito comincia a trascinare il cartellino (solo finché è appeso al cestino).</summary>
        public bool GrabSign()
        {
            if (InputLocked || Holding || SignAt != HonestThiefSignAt.Home) return false;
            SignAt = HonestThiefSignAt.Held;
            return true;
        }

        /// <summary>Cartellino lasciato: sulla teca si aggancia (e lì resta), altrove torna al cestino.</summary>
        public HonestThiefSignDrop ReleaseSign(bool onCase)
        {
            if (InputLocked || SignAt != HonestThiefSignAt.Held) return HonestThiefSignDrop.None;
            if (!onCase)
            {
                SignAt = HonestThiefSignAt.Home;
                SignReturns++;
                return HonestThiefSignDrop.Returned;
            }
            SignAt = HonestThiefSignAt.Case;
            alarmT = 0f;   // la teca ora è FREE: la sirena tace subito
            return HonestThiefSignDrop.Placed;
        }

        /// <summary>Dito perso (pausa, app in background, secondo dito, livello spento): quello che si teneva torna a casa.</summary>
        public bool Cancel()
        {
            bool any = false;
            if (SignAt == HonestThiefSignAt.Held) { SignAt = HonestThiefSignAt.Home; any = true; }
            if (DiamondAt == HonestThiefDiamondAt.Held) { DiamondAt = HonestThiefDiamondAt.Case; any = true; }
            return any;
        }

        /// <summary>Fine della festa: true una volta sola (allora il livello chiama Win).</summary>
        public bool TryComplete()
        {
            if (Stage != HonestThiefStage.Celebrating || completed) return false;
            completed = true;
            Stage = HonestThiefStage.Done;
            return true;
        }

        // ---------- Zone di rilascio (dai limiti degli oggetti, mai coordinate dello schermo) ----------

        /// <summary>
        /// Punto (px, py) dentro il rettangolo (centro cx, cy; mezze misure halfW, halfH) allargato di margin per lato.
        /// Per la teca: rettangolo di vetro + piedistallo con <see cref="CaseMargin"/>; per il ladro: la sua sagoma con
        /// <see cref="ThiefMargin"/>.
        /// </summary>
        public static bool InZone(float px, float py, float cx, float cy, float halfW, float halfH, float margin)
        {
            float hw = Abs(halfW) + (margin > 0f ? margin : 0f), hh = Abs(halfH) + (margin > 0f ? margin : 0f);
            return Abs(px - cx) <= hw && Abs(py - cy) <= hh;
        }

        static float Abs(float v) => v < 0f ? -v : v;
    }
}
