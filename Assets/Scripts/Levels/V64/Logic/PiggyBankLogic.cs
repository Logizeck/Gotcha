namespace Gotcha.Levels.V64
{
    /// <summary>Fasi del livello 273.</summary>
    public enum PiggyStage
    {
        Playing,    // il salvadanaio è chiuso: si prova di tutto
        Pouring,    // il tappo è stato tolto: pioggia di monete (finale), input bloccato
        Done,
    }

    /// <summary>Come sta il salvadanaio sulla scrivania.</summary>
    public enum PiggyPose
    {
        Upright,    // in piedi: la fessura in alto, il tappo nascosto sotto la pancia
        Flipped,    // capovolto (pancia all'aria): si vede il tappo di gomma
    }

    /// <summary>Girare il salvadanaio (sollevarlo/ruotarlo trascinandolo).</summary>
    public enum PiggyTurn
    {
        None,           // input bloccato
        Flipped,        // capovolto: esce una monetina dalla fessura e sotto la pancia si vede il tappo
        FlippedNoCoin,  // capovolto, ma dalla fessura non esce più niente (monetine finite)
        Righted,        // rimesso in piedi
    }

    /// <summary>Una monetina che esce (scossone o coltello): mai abbastanza.</summary>
    public enum PiggyTrickle
    {
        None,       // input bloccato
        Coin,       // esce una monetina: un centesimo in più
        Jammed,     // non esce più niente (le monete sono incastrate di traverso)
    }

    /// <summary>Il coltello da burro infilato nella fessura.</summary>
    public enum PiggyKnife
    {
        None,       // input bloccato
        Coin,       // esce una monetina, poi il coltello si incastra e salta via
        Stuck,      // niente monetina (finite): il coltello si incastra e salta via
        SlotUnder,  // il salvadanaio è capovolto: la fessura è sotto
    }

    /// <summary>Il tappo di gomma sotto la pancia.</summary>
    public enum PiggyPlug
    {
        None,       // input bloccato
        Hidden,     // il salvadanaio è in piedi: il tappo è sotto, non ci si arriva
        Pulled,     // tolto! pioggia di monete (la soluzione)
    }

    /// <summary>
    /// Logica pura del livello 273 "The Piggy Bank" — "Get the money out!" Il salvadanaio di ceramica Mr. Oink, pieno
    /// di monete, sulla scrivania della cameretta; il furgone dei gelati arriva (solo atmosfera: niente sconfitta).
    /// False piste (nessuna azzera i progressi, niente sconfitta): il martello (il bambino lo protegge, "NO!": non si
    /// rompe MAI), scuotere il telefono (una monetina alla volta, lentissimo: i centesimi raccolti restano ma non
    /// bastano mai), il coltello da burro nella fessura (una monetina, poi si incastra e salta via), toccare la fessura
    /// ("Coins go IN there."). Girare il salvadanaio lo capovolge (anche lui fa uscire una monetina dalla fessura).
    /// Indizio: l'adesivo con la freccia in giù sul fianco; scuotendolo qualcosa sotto "suona diverso".
    /// Soluzione: capovolgere il salvadanaio (sotto la pancia c'è un tappo di gomma) e tirare via il tappo: tutte le
    /// monete escono. Nessuna dipendenza da Unity.
    /// </summary>
    public sealed class PiggyBankLogic
    {
        /// <summary>Quanti centesimi ci sono nel salvadanaio (12.47 dollari).</summary>
        public const int TotalCents = 1247;
        /// <summary>Oltre tante monetine (scossoni + coltello + capovolte) non esce più niente dalla fessura.</summary>
        public const int TrickleMax = 12;
        /// <summary>Trascinamento (unità di mondo) oltre il quale il salvadanaio si gira.</summary>
        public const float TurnDrag = 0.5f;
        /// <summary>Trascinamento (unità di mondo) del tappo oltre il quale si stacca.</summary>
        public const float PlugDrag = 0.3f;

        public PiggyStage Stage { get; private set; } = PiggyStage.Playing;
        public PiggyPose Pose { get; private set; } = PiggyPose.Upright;
        /// <summary>Centesimi raccolti finora (restano sempre; alla fine diventano tutti).</summary>
        public int Cents { get; private set; }
        /// <summary>True se il tappo è stato visto almeno una volta (salvadanaio capovolto).</summary>
        public bool PlugSeen { get; private set; }
        public bool NoteRead { get; private set; }

        public int HammerHits { get; private set; }
        public int Shakes { get; private set; }
        public int KnifeTries { get; private set; }
        public int SlotTaps { get; private set; }
        public int Flips { get; private set; }
        public int Rights { get; private set; }
        public int PlugTaps { get; private set; }
        public int PiggyTaps { get; private set; }
        public int KidTaps { get; private set; }
        public int TeddyTaps { get; private set; }
        public int WindowTaps { get; private set; }
        /// <summary>Monetine uscite dalla fessura (scossoni, coltello, capovolte).</summary>
        public int Trickled { get; private set; }
        public bool CompletionReported { get; private set; }

        public bool InputLocked => Stage != PiggyStage.Playing;
        public bool Flipped => Pose == PiggyPose.Flipped;
        /// <summary>Il salvadanaio è sempre intero (il martello non lo rompe mai).</summary>
        public bool PiggyIntact => true;
        public bool PlugOut => Stage != PiggyStage.Playing;
        public bool SlotJammed => Trickled >= TrickleMax;
        /// <summary>Tutte le mosse sbagliate fatte finora (solo statistica: non cambiano niente).</summary>
        public int WrongMoves => HammerHits + Shakes + KnifeTries + SlotTaps;

        public void Reset()
        {
            Stage = PiggyStage.Playing;
            Pose = PiggyPose.Upright;
            Cents = 0;
            PlugSeen = NoteRead = false;
            HammerHits = Shakes = KnifeTries = SlotTaps = Flips = Rights = PlugTaps = PiggyTaps = 0;
            KidTaps = TeddyTaps = WindowTaps = Trickled = 0;
            CompletionReported = false;
        }

        /// <summary>Una monetina dalla fessura, se ce n'è ancora una che passa.</summary>
        bool TrickleOne()
        {
            if (SlotJammed) return false;
            Trickled++;
            Cents++;
            return true;
        }

        // ---------- Le false piste ----------

        /// <summary>Il martello sul salvadanaio: il bambino lo protegge. Restituisce quante martellate finora (0 = bloccato).</summary>
        public int Hammer()
        {
            if (InputLocked) return 0;
            HammerHits++;
            return HammerHits;
        }

        /// <summary>Scuotere il telefono (o il salvadanaio): una monetina alla volta.</summary>
        public PiggyTrickle Shake()
        {
            if (InputLocked) return PiggyTrickle.None;
            Shakes++;
            return TrickleOne() ? PiggyTrickle.Coin : PiggyTrickle.Jammed;
        }

        /// <summary>Il coltello da burro nella fessura.</summary>
        public PiggyKnife Knife()
        {
            if (InputLocked) return PiggyKnife.None;
            KnifeTries++;
            if (Flipped) return PiggyKnife.SlotUnder;
            return TrickleOne() ? PiggyKnife.Coin : PiggyKnife.Stuck;
        }

        /// <summary>Toccare la fessura: "Coins go IN there." False se bloccato.</summary>
        public bool TapSlot()
        {
            if (InputLocked) return false;
            SlotTaps++;
            return true;
        }

        // ---------- Girare il salvadanaio ----------

        /// <summary>Capovolge il salvadanaio (o lo rimette in piedi). Capovolgendolo esce una monetina e si vede il tappo.</summary>
        public PiggyTurn Turn()
        {
            if (InputLocked) return PiggyTurn.None;
            if (Flipped)
            {
                Pose = PiggyPose.Upright;
                Rights++;
                return PiggyTurn.Righted;
            }
            Pose = PiggyPose.Flipped;
            Flips++;
            PlugSeen = true;
            return TrickleOne() ? PiggyTurn.Flipped : PiggyTurn.FlippedNoCoin;
        }

        // ---------- Il tappo ----------

        /// <summary>Toccare il tappo (solo col salvadanaio capovolto). False se non ci si arriva.</summary>
        public bool TapPlug()
        {
            if (InputLocked || !Flipped) return false;
            PlugTaps++;
            return true;
        }

        /// <summary>Tirare via il tappo: solo col salvadanaio capovolto (il tappo è sotto la pancia).</summary>
        public PiggyPlug PullPlug()
        {
            if (InputLocked) return PiggyPlug.None;
            if (!Flipped) return PiggyPlug.Hidden;
            Stage = PiggyStage.Pouring;
            return PiggyPlug.Pulled;
        }

        // ---------- Il resto della stanza ----------

        public bool TapPiggy()
        {
            if (InputLocked) return false;
            PiggyTaps++;
            return true;
        }

        public bool TapKid()
        {
            if (InputLocked) return false;
            KidTaps++;
            return true;
        }

        public bool TapTeddy()
        {
            if (InputLocked) return false;
            TeddyTaps++;
            return true;
        }

        public bool TapWindow()
        {
            if (InputLocked) return false;
            WindowTaps++;
            return true;
        }

        public void ReadNote()
        {
            if (InputLocked) return;
            NoteRead = true;
        }

        // ---------- Il finale ----------

        /// <summary>Fine della pioggia di monete: tutti i centesimi raccolti, vittoria una volta sola.</summary>
        public bool TryComplete()
        {
            if (Stage != PiggyStage.Pouring || CompletionReported) return false;
            CompletionReported = true;
            Cents = TotalCents;
            Stage = PiggyStage.Done;
            return true;
        }

        /// <summary>Testo del contatore: "1 CENT", "7 CENTS", "$12.47".</summary>
        public static string CentsLabel(int cents)
        {
            if (cents >= 100) return "$" + (cents / 100) + "." + (cents % 100).ToString("00");
            return cents == 1 ? "1 CENT" : cents + " CENTS";
        }
    }
}
