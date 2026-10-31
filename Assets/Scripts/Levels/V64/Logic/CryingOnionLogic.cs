namespace Gotcha.Levels.V64
{
    /// <summary>Fasi del livello 274.</summary>
    public enum OnionStage
    {
        Playing,    // si prova di tutto
        Chopping,   // la cipolla fredda è sotto il coltello: festa in corso, input bloccato
        Done,
    }

    /// <summary>Dove sta la cipolla (quando non è in mano).</summary>
    public enum OnionSpot
    {
        Board,      // sul tagliere
        Fridge,     // dentro il frigo
    }

    /// <summary>Una coltellata (strisciata sulla cipolla o coltello portato sulla cipolla).</summary>
    public enum OnionChopResult
    {
        None,       // input bloccato
        NoOnion,    // la cipolla non è sul tagliere (è in frigo): si taglia l'aria
        Tears,      // cipolla a temperatura ambiente: fontane di lacrime, il taglio si ferma, la cipolla si ricompone
        Clean,      // cipolla fredda: niente lacrime, la soluzione (vittoria dopo la festa)
    }

    /// <summary>Lo sportello del frigo toccato.</summary>
    public enum OnionFridgeResult
    {
        None,           // input bloccato
        OpenedEmpty,    // aperto, senza cipolla dentro
        OpenedWarm,     // aperto troppo presto (meno di RushAfter secondi): la cipolla non è fredda, il freddo accumulato resta
        OpenedCold,     // aperto: la cipolla dentro è fredda e brinata
        Rushed,         // aperto dopo almeno RushAfter secondi di freddo: il tocco accelera, la cipolla è fredda subito
        Closed,         // richiuso senza cipolla (o con la cipolla già fredda)
        ClosedChilling, // richiuso con la cipolla tiepida dentro: parte (o riprende) il conto alla rovescia del freddo
    }

    /// <summary>La cipolla lasciata da qualche parte.</summary>
    public enum OnionDropResult
    {
        None,       // input bloccato o cipolla che non si può prendere
        OnBoard,    // torna (o resta) sul tagliere
        InFridge,   // messa nel frigo aperto
    }

    /// <summary>Gli occhialini da nuoto messi al cuoco.</summary>
    public enum OnionGogglesResult
    {
        None,
        Fogged,     // si appannano subito: "Now you're crying AND blind." (poi tornano al gancio)
    }

    /// <summary>Il fiato trattenuto (dito tenuto sulla bocca del cuoco).</summary>
    public enum OnionBreathResult
    {
        None,
        Gasp,       // lasciato prima: un respiro affannato, non cambia niente
        Blue,       // tenuto troppo: diventa blu e boccheggia
    }

    /// <summary>Un rimedio della nonna (candela accesa vicino alla cipolla, pane in bocca).</summary>
    public enum OnionRemedyResult
    {
        None,
        Applied,    // ora è attivo (alla prossima coltellata: effetto minuscolo, lacrime lo stesso, "Grandma lied.")
        Already,    // era già attivo
    }

    /// <summary>Cosa è cambiato da solo con il passare del tempo.</summary>
    public enum OnionTick
    {
        None,
        Chilled,    // la cipolla in frigo (chiuso) è diventata fredda
        WarmedUp,   // la cipolla fredda è rimasta fuori troppo: è tornata a temperatura ambiente (si può rifare)
    }

    /// <summary>
    /// Logica pura del livello 274 "The Crying Onion" — "Chop the onion without crying!" Un bancone di cucina: il tagliere
    /// con una grossa cipolla, il coltello, il cuoco con gli occhi lucidi, gli occhialini da nuoto appesi a un gancio, un
    /// ventilatore, il frigo con un biglietto sotto la calamita ("Cold onions don't bite."), una candela, una fetta di pane.
    /// False piste (nessuna azzera i progressi, niente sconfitta): tagliare subito (fontane di lacrime, "Too many
    /// feelings.", la cipolla si ricompone), gli occhialini (si appannano: "Now you're crying AND blind."), il ventilatore
    /// (le lacrime volano di lato), trattenere il fiato (diventa blu, boccheggia), candela e pane in bocca (rimedi della
    /// nonna: effetto minuscolo, "Grandma lied."). Indizio: il biglietto sul frigo e i fumi della cipolla, che si
    /// rimpiccioliscono vicino al frigo.
    /// Soluzione: mettere la cipolla nel frigo, chiudere, aspettare (ChillSeconds; oppure aprire il frigo dopo RushAfter
    /// secondi), riprenderla (fredda, senza fumi) e tagliarla -> niente lacrime -> vittoria, una volta sola.
    /// Fuori dal frigo la cipolla fredda torna tiepida dopo ColdLasts secondi (si può raffreddare di nuovo).
    /// Nessuna dipendenza da Unity.
    /// </summary>
    public sealed class CryingOnionLogic
    {
        /// <summary>Secondi di frigo chiuso perché la cipolla diventi fredda.</summary>
        public const float ChillSeconds = 5f;
        /// <summary>Dopo questi secondi un tocco sul frigo chiuso accelera il freddo.</summary>
        public const float RushAfter = 3f;
        /// <summary>Secondi fuori dal frigo prima che la cipolla fredda torni tiepida.</summary>
        public const float ColdLasts = 30f;
        /// <summary>Secondi di fiato trattenuto prima di diventare blu.</summary>
        public const float BlueAfter = 1.6f;
        /// <summary>Distanza dal frigo sotto la quale i fumi cominciano a rimpicciolirsi (unità del mondo).</summary>
        public const float FumeNear = 1.5f, FumeMin = 0.25f;

        public OnionStage Stage { get; private set; } = OnionStage.Playing;
        public OnionSpot Spot { get; private set; } = OnionSpot.Board;
        public bool FridgeOpen { get; private set; }
        /// <summary>Secondi di freddo accumulati dalla cipolla (restano anche se si apre il frigo troppo presto).</summary>
        public float Chill { get; private set; }
        public bool Cold { get; private set; }
        /// <summary>Secondi di freddo che restano alla cipolla fredda fuori dal frigo.</summary>
        public float ColdLeft { get; private set; }
        public bool FanOn { get; private set; }
        public bool CandleLit { get; private set; }
        public bool BreadInMouth { get; private set; }

        public int WarmChops { get; private set; }
        public int AirChops { get; private set; }
        public int GogglesTries { get; private set; }
        public int FanToggles { get; private set; }
        public int BreathHolds { get; private set; }
        public int BlueFaces { get; private set; }
        public int CandleTries { get; private set; }
        public int BreadTries { get; private set; }
        public int GrandmaChops { get; private set; }
        public int SidewaysChops { get; private set; }
        public int EarlyOpens { get; private set; }
        public int NoteReads { get; private set; }
        public int CookTaps { get; private set; }
        public int KnifeTaps { get; private set; }
        public int TimesChilled { get; private set; }
        public int TimesWarmed { get; private set; }
        public bool CompletionReported { get; private set; }

        public bool InputLocked => Stage != OnionStage.Playing;
        /// <summary>Un rimedio della nonna è attivo (candela accesa o pane in bocca).</summary>
        public bool RemedyActive => CandleLit || BreadInMouth;
        /// <summary>La cipolla sta prendendo freddo adesso (in frigo, sportello chiuso, non ancora fredda).</summary>
        public bool Chilling => Spot == OnionSpot.Fridge && !FridgeOpen && !Cold && !InputLocked;
        /// <summary>Avanzamento del freddo, 0..1 (per la brina sullo sportello).</summary>
        public float ChillProgress => Cold ? 1f : Clamp01(Chill / ChillSeconds);
        /// <summary>La cipolla si può prendere in mano (sul tagliere, o nel frigo aperto).</summary>
        public bool CanGrabOnion => !InputLocked && (Spot == OnionSpot.Board || FridgeOpen);
        /// <summary>Il cuoco ha gli occhi lucidi: la cipolla tiepida è sul tagliere.</summary>
        public bool EyesWatery => Spot == OnionSpot.Board && !Cold && Stage == OnionStage.Playing;
        /// <summary>Tutte le mosse sbagliate fatte finora (solo statistica: non cambiano niente).</summary>
        public int WrongMoves => WarmChops + AirChops + GogglesTries + FanToggles + BreathHolds + CandleTries + BreadTries + EarlyOpens + CookTaps + KnifeTaps;

        static float Clamp01(float v) => v < 0f ? 0f : v > 1f ? 1f : v;

        public void Reset()
        {
            Stage = OnionStage.Playing;
            Spot = OnionSpot.Board;
            FridgeOpen = Cold = FanOn = CandleLit = BreadInMouth = false;
            Chill = ColdLeft = 0f;
            WarmChops = AirChops = GogglesTries = FanToggles = BreathHolds = BlueFaces = CandleTries = BreadTries = 0;
            GrandmaChops = SidewaysChops = EarlyOpens = NoteReads = CookTaps = KnifeTaps = 0;
            TimesChilled = TimesWarmed = 0;
            CompletionReported = false;
        }

        /// <summary>
        /// Quanto sono forti i fumi della cipolla (1 = pieni, 0 = niente): spariscono se è fredda o chiusa nel frigo e si
        /// rimpiccioliscono avvicinandosi al frigo (l'indizio).
        /// </summary>
        public static float FumeStrength(bool cold, bool hidden, float distanceToFridge)
        {
            if (cold || hidden) return 0f;
            if (distanceToFridge >= FumeNear) return 1f;
            float k = distanceToFridge <= 0f ? 0f : distanceToFridge / FumeNear;
            return FumeMin + (1f - FumeMin) * k;
        }

        // ---------- Il tempo ----------

        /// <summary>Fa passare il tempo (secondi di gioco, fermi in pausa): il frigo raffredda, fuori la cipolla si scalda.</summary>
        public OnionTick Tick(float dt)
        {
            if (InputLocked || dt <= 0f) return OnionTick.None;
            if (Chilling)
            {
                Chill += dt;
                if (Chill >= ChillSeconds) { MakeCold(); return OnionTick.Chilled; }
                return OnionTick.None;
            }
            if (Cold && Spot == OnionSpot.Fridge) { ColdLeft = ColdLasts; return OnionTick.None; }   // in frigo resta fredda
            if (Cold)
            {
                ColdLeft -= dt;
                if (ColdLeft <= 0f)
                {
                    Cold = false;
                    Chill = 0f;
                    ColdLeft = 0f;
                    TimesWarmed++;
                    return OnionTick.WarmedUp;
                }
            }
            return OnionTick.None;
        }

        void MakeCold()
        {
            Cold = true;
            Chill = ChillSeconds;
            ColdLeft = ColdLasts;
            TimesChilled++;
        }

        // ---------- Il frigo ----------

        /// <summary>
        /// Toccare lo sportello del frigo: si apre o si chiude. Aprendolo con la cipolla tiepida dentro: prima di RushAfter
        /// secondi è troppo presto (il freddo accumulato resta, richiudendo riprende), dopo il tocco "accelera" e la
        /// cipolla esce subito fredda.
        /// </summary>
        public OnionFridgeResult TapFridge()
        {
            if (InputLocked) return OnionFridgeResult.None;
            bool inside = Spot == OnionSpot.Fridge;
            if (!FridgeOpen)
            {
                bool rush = Chilling && Chill >= RushAfter;
                FridgeOpen = true;
                if (!inside) return OnionFridgeResult.OpenedEmpty;
                if (Cold) return OnionFridgeResult.OpenedCold;
                if (rush) { MakeCold(); return OnionFridgeResult.Rushed; }
                EarlyOpens++;
                return OnionFridgeResult.OpenedWarm;
            }
            FridgeOpen = false;
            return inside && !Cold ? OnionFridgeResult.ClosedChilling : OnionFridgeResult.Closed;
        }

        /// <summary>Lasciare la cipolla: nel frigo (solo se aperto) o di nuovo sul tagliere.</summary>
        public OnionDropResult DropOnion(bool overFridge)
        {
            if (!CanGrabOnion) return OnionDropResult.None;
            if (overFridge && FridgeOpen)
            {
                Spot = OnionSpot.Fridge;
                if (Cold) ColdLeft = ColdLasts;
                return OnionDropResult.InFridge;
            }
            Spot = OnionSpot.Board;
            return OnionDropResult.OnBoard;
        }

        // ---------- Il coltello ----------

        /// <summary>Una coltellata: fredda vince, tiepida fa piangere (sempre, qualunque rimedio ci sia).</summary>
        public OnionChopResult Chop()
        {
            if (InputLocked) return OnionChopResult.None;
            if (Spot != OnionSpot.Board) { AirChops++; return OnionChopResult.NoOnion; }
            if (Cold)
            {
                Stage = OnionStage.Chopping;
                return OnionChopResult.Clean;
            }
            WarmChops++;
            if (FanOn) SidewaysChops++;
            if (RemedyActive) GrandmaChops++;
            return OnionChopResult.Tears;
        }

        /// <summary>Toccare il coltello senza tagliare (dice come si usa). True se conta.</summary>
        public bool TapKnife()
        {
            if (InputLocked) return false;
            KnifeTaps++;
            return true;
        }

        // ---------- Le false piste ----------

        /// <summary>Gli occhialini da nuoto sul cuoco: si appannano subito.</summary>
        public OnionGogglesResult Goggles()
        {
            if (InputLocked) return OnionGogglesResult.None;
            GogglesTries++;
            return OnionGogglesResult.Fogged;
        }

        /// <summary>Accendere/spegnere il ventilatore. Ritorna lo stato nuovo (false anche se l'input è bloccato).</summary>
        public bool ToggleFan()
        {
            if (InputLocked) return false;
            FanToggles++;
            FanOn = !FanOn;
            return FanOn;
        }

        /// <summary>Il dito tenuto sulla bocca del cuoco per tanti secondi: oltre BlueAfter diventa blu.</summary>
        public OnionBreathResult HoldBreath(float seconds)
        {
            if (InputLocked) return OnionBreathResult.None;
            BreathHolds++;
            if (seconds >= BlueAfter) { BlueFaces++; return OnionBreathResult.Blue; }
            return OnionBreathResult.Gasp;
        }

        /// <summary>Accendere la candela vicino alla cipolla (rimedio della nonna).</summary>
        public OnionRemedyResult LightCandle()
        {
            if (InputLocked) return OnionRemedyResult.None;
            CandleTries++;
            if (CandleLit) return OnionRemedyResult.Already;
            CandleLit = true;
            return OnionRemedyResult.Applied;
        }

        /// <summary>La fetta di pane in bocca al cuoco (rimedio della nonna).</summary>
        public OnionRemedyResult Bread()
        {
            if (InputLocked) return OnionRemedyResult.None;
            BreadTries++;
            if (BreadInMouth) return OnionRemedyResult.Already;
            BreadInMouth = true;
            return OnionRemedyResult.Applied;
        }

        /// <summary>Leggere il biglietto sotto la calamita del frigo (l'indizio). True se conta.</summary>
        public bool ReadNote()
        {
            if (InputLocked) return false;
            NoteReads++;
            return true;
        }

        /// <summary>Toccare il cuoco (tirata su col naso). True se conta.</summary>
        public bool TapCook()
        {
            if (InputLocked) return false;
            CookTaps++;
            return true;
        }

        // ---------- Il finale ----------

        /// <summary>Fine della festa (cipolla tagliata a cubetti, occhi asciutti): vittoria, una volta sola.</summary>
        public bool TryComplete()
        {
            if (Stage != OnionStage.Chopping || CompletionReported) return false;
            CompletionReported = true;
            Stage = OnionStage.Done;
            return true;
        }
    }
}
