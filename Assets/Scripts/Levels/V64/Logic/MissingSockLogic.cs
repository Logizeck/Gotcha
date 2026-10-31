namespace Gotcha.Levels.V64
{
    /// <summary>Fasi del livello 272.</summary>
    public enum SockStage
    {
        Playing,    // si prova di tutto
        Matching,   // il calzino giusto è sullo stendino accanto al gemello: festa in corso, input bloccato
        Done,
    }

    /// <summary>Lo sportello della lavatrice toccato.</summary>
    public enum SockWasherResult
    {
        None,           // input bloccato
        OpenedEmpty,    // aperto: il cestello sembra vuoto ("Empty. Suspiciously empty.")
        Closed,         // richiuso
    }

    /// <summary>Il cestello toccato (a sportello aperto).</summary>
    public enum SockDrumResult
    {
        None,       // input bloccato o sportello chiuso
        Empty,      // vuoto, davvero
    }

    /// <summary>La guarnizione di gomma dello sportello (tirata, o toccato il pezzetto di stoffa che spunta).</summary>
    public enum SockSealResult
    {
        None,       // input bloccato
        Blocked,    // sportello chiuso: la guarnizione è schiacciata, non ci si arriva
        Found,      // la piega della guarnizione nascondeva il calzino gemello: esce (la soluzione, prima metà)
        Nothing,    // già trovato: dentro non c'è altro
    }

    /// <summary>L'asciugatrice toccata.</summary>
    public enum SockDryerResult
    {
        None,
        OpenedLint,     // aperta: solo pelucchi (e un coniglietto di pelucchi)
        Closed,
    }

    /// <summary>L'animale che dorme toccato.</summary>
    public enum SockPetResult
    {
        None,
        RevealsSock,    // la prima volta si gira: dormiva su un calzino... a pois, non a stelline
        Sleeps,         // già girato: continua a dormire
    }

    /// <summary>Un calzino portato sullo stendino accanto al gemello.</summary>
    public enum SockPairResult
    {
        None,       // input bloccato o calzino che non si può avere in mano
        Wrong,      // un calzino della cesta: critica di moda ("Bold. But no.")
        Close,      // quello del gatto: verde e giallo, ma a pois
        Match,      // il gemello vero: vittoria (dopo la festa)
    }

    /// <summary>
    /// Logica pura del livello 272 "The Missing Sock" — "Find the matching sock!" Una lavanderia: la lavatrice a oblò
    /// (sportello chiuso, guarnizione di gomma), l'asciugatrice, la cesta con sette calzini spaiati tutti diversi, lo
    /// stendino con il calzino da accoppiare (verde a stelline gialle), il gatto che dorme, il detersivo.
    /// False piste (nessuna azzera i progressi, niente sconfitta): accoppiare un calzino della cesta ("Bold. But no."),
    /// aprire l'asciugatrice (solo pelucchi), svegliare il gatto (dormiva su un calzino a pois: "Nice try"), scuotere il
    /// telefono (i calzini nella cesta saltellano), aprire la lavatrice (il cestello è vuoto: "Empty. Suspiciously
    /// empty."). Indizio: un pezzetto di stoffa a stelline spunta dalla guarnizione dello sportello (si vede anche a
    /// sportello chiuso, ogni tanto si muove).
    /// Soluzione: aprire lo sportello, poi tirare la guarnizione (o toccare il pezzetto che spunta): il calzino gemello
    /// era nella piega della gomma; portarlo sullo stendino accanto al gemello -> vittoria, una volta sola.
    /// La guarnizione si raggiunge solo a sportello aperto. Nessuna dipendenza da Unity.
    /// </summary>
    public sealed class MissingSockLogic
    {
        /// <summary>I calzini che si possono tenere in mano: 0..6 quelli della cesta, poi quello del gatto e il gemello.</summary>
        public const int BasketSocks = 7;
        public const int PetSock = BasketSocks;
        public const int MatchSock = BasketSocks + 1;

        public SockStage Stage { get; private set; } = SockStage.Playing;
        public bool WasherOpen { get; private set; }
        public bool DryerOpen { get; private set; }
        /// <summary>Il gatto si è girato e il calzino a pois è libero.</summary>
        public bool PetRevealed { get; private set; }
        /// <summary>Il gemello è uscito dalla guarnizione (e da lì resta fuori).</summary>
        public bool SockFound { get; private set; }
        /// <summary>Il gemello è sullo stendino accanto all'altro.</summary>
        public bool Paired { get; private set; }

        public int WasherOpens { get; private set; }
        public int WasherCloses { get; private set; }
        public int DrumTaps { get; private set; }
        public int SealTries { get; private set; }
        public int BlockedSealTries { get; private set; }
        public int DryerOpens { get; private set; }
        public int LintTaps { get; private set; }
        public int PetTaps { get; private set; }
        public int Shakes { get; private set; }
        public int WrongPairs { get; private set; }
        public int ClosePairs { get; private set; }
        public int RackSockTaps { get; private set; }
        public int DetergentTaps { get; private set; }
        public bool CompletionReported { get; private set; }

        public bool InputLocked => Stage != SockStage.Playing;
        /// <summary>Il pezzetto di stoffa a stelline spunta ancora dalla guarnizione.</summary>
        public bool PeekVisible => !SockFound;
        /// <summary>La guarnizione si può tirare solo a sportello aperto.</summary>
        public bool SealReachable => WasherOpen && !InputLocked;
        /// <summary>Tutte le mosse sbagliate fatte finora (solo statistica: non cambiano niente).</summary>
        public int WrongMoves => WasherOpens + DrumTaps + BlockedSealTries + DryerOpens + LintTaps + PetTaps + Shakes + WrongPairs + ClosePairs + RackSockTaps + DetergentTaps;

        /// <summary>Il calzino si può prendere in mano adesso (quelli della cesta sempre, gli altri solo una volta scoperti).</summary>
        public bool CanHold(int sock)
        {
            if (InputLocked) return false;
            if (sock >= 0 && sock < BasketSocks) return true;
            if (sock == PetSock) return PetRevealed;
            if (sock == MatchSock) return SockFound && !Paired;
            return false;
        }

        public void Reset()
        {
            Stage = SockStage.Playing;
            WasherOpen = DryerOpen = PetRevealed = SockFound = Paired = false;
            WasherOpens = WasherCloses = DrumTaps = SealTries = BlockedSealTries = DryerOpens = LintTaps = 0;
            PetTaps = Shakes = WrongPairs = ClosePairs = RackSockTaps = DetergentTaps = 0;
            CompletionReported = false;
        }

        // ---------- La lavatrice ----------

        /// <summary>Toccare lo sportello: si apre (cestello vuoto) o si richiude.</summary>
        public SockWasherResult ToggleWasher()
        {
            if (InputLocked) return SockWasherResult.None;
            WasherOpen = !WasherOpen;
            if (WasherOpen) { WasherOpens++; return SockWasherResult.OpenedEmpty; }
            WasherCloses++;
            return SockWasherResult.Closed;
        }

        /// <summary>Toccare il cestello (solo a sportello aperto): vuoto.</summary>
        public SockDrumResult TapDrum()
        {
            if (InputLocked || !WasherOpen) return SockDrumResult.None;
            DrumTaps++;
            return SockDrumResult.Empty;
        }

        /// <summary>Tirare la guarnizione (o toccare il pezzetto che spunta): solo a sportello aperto esce il gemello.</summary>
        public SockSealResult PullSeal()
        {
            if (InputLocked) return SockSealResult.None;
            SealTries++;
            if (!WasherOpen) { BlockedSealTries++; return SockSealResult.Blocked; }
            if (SockFound) return SockSealResult.Nothing;
            SockFound = true;
            return SockSealResult.Found;
        }

        // ---------- Le false piste ----------

        /// <summary>Toccare l'asciugatrice: si apre (pelucchi) o si richiude.</summary>
        public SockDryerResult ToggleDryer()
        {
            if (InputLocked) return SockDryerResult.None;
            DryerOpen = !DryerOpen;
            if (DryerOpen) { DryerOpens++; return SockDryerResult.OpenedLint; }
            return SockDryerResult.Closed;
        }

        /// <summary>Toccare il coniglietto di pelucchi (solo ad asciugatrice aperta). True se c'era.</summary>
        public bool TapLint()
        {
            if (InputLocked || !DryerOpen) return false;
            LintTaps++;
            return true;
        }

        /// <summary>Toccare il gatto: la prima volta si gira e scopre il calzino su cui dormiva.</summary>
        public SockPetResult TapPet()
        {
            if (InputLocked) return SockPetResult.None;
            PetTaps++;
            if (PetRevealed) return SockPetResult.Sleeps;
            PetRevealed = true;
            return SockPetResult.RevealsSock;
        }

        /// <summary>Scuotere il telefono: i calzini nella cesta saltellano, niente di nuovo. True se conta.</summary>
        public bool Shake()
        {
            if (InputLocked) return false;
            Shakes++;
            return true;
        }

        /// <summary>Toccare il calzino solitario sullo stendino (dice com'è fatto il gemello).</summary>
        public bool TapRackSock()
        {
            if (InputLocked) return false;
            RackSockTaps++;
            return true;
        }

        /// <summary>Toccare il detersivo.</summary>
        public bool TapDetergent()
        {
            if (InputLocked) return false;
            DetergentTaps++;
            return true;
        }

        // ---------- Lo stendino ----------

        /// <summary>Un calzino lasciato sullo stendino accanto al gemello: solo quello giusto vince.</summary>
        public SockPairResult TryPair(int sock)
        {
            if (!CanHold(sock)) return SockPairResult.None;
            if (sock == MatchSock)
            {
                Paired = true;
                Stage = SockStage.Matching;
                return SockPairResult.Match;
            }
            if (sock == PetSock) { ClosePairs++; return SockPairResult.Close; }
            WrongPairs++;
            return SockPairResult.Wrong;
        }

        // ---------- Il finale ----------

        /// <summary>Fine della festa dei calzini: vittoria, una volta sola.</summary>
        public bool TryComplete()
        {
            if (Stage != SockStage.Matching || CompletionReported) return false;
            CompletionReported = true;
            Stage = SockStage.Done;
            return true;
        }
    }
}
