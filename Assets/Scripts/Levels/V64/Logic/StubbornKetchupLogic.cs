namespace Gotcha.Levels.V64
{
    /// <summary>Fasi del livello 268.</summary>
    public enum KetchupStage
    {
        Playing,    // si prova di tutto; tappo e sigillo cambiano stato qui
        Pouring,    // il ketchup scende sulle patatine (input bloccato)
        Done,
    }

    /// <summary>Cosa succede strizzando, scuotendo, sbattendo o capovolgendo il flacone.</summary>
    public enum KetchupResult
    {
        None,       // input bloccato
        Nothing,    // ancora sigillato (o al contrario): non esce niente — la reazione dipende dalla mossa
        Pour,       // sigillo tolto e flacone sopra le patatine: il ketchup scende (la soluzione)
    }

    /// <summary>Il coltello infilato nel collo del flacone.</summary>
    public enum KnifeResult
    {
        None,
        Clean,      // tappo avvitato: il coltello esce pulito
        Foil,       // tappo tolto, sigillo ancora lì: "non pugnalarlo, c'è una linguetta"
        Open,       // già aperto: niente di speciale
    }

    /// <summary>Tirare la linguetta del sigillo.</summary>
    public enum PeelResult
    {
        None,       // input bloccato o già tolto
        CapInTheWay,// il tappo è ancora avvitato: la linguetta non viene
        Partial,    // tirata poco: torna giù, nessuna perdita
        Peeled,     // sigillo tolto: "IT WAS NEVER OPENED."
    }

    /// <summary>
    /// Logica pura del livello 268 "The Stubborn Ketchup" — "Put ketchup on the fries." Il flacone è nuovo: sotto il
    /// tappo c'è ancora il sigillo di alluminio (si vede solo una linguetta argentata che spunta, e sul flacone c'è
    /// l'adesivo "NEW!"). False piste (nessuna azzera i progressi, niente sconfitta): capovolgerlo sopra le patatine,
    /// scuoterlo al contrario, strizzarlo, sbattere il fondo, infilare il coltello nel collo.
    /// Soluzione: svitare il tappo (giri attorno al tappo, oppure due tocchi), togliere il sigillo tirando la linguetta,
    /// poi capovolgere/strizzare il flacone sopra le patatine. Il tappo si può rimettere e ritogliere quante volte si vuole;
    /// il sigillo si può tirare solo a tappo tolto; il ketchup esce solo a sigillo tolto. Nessuna dipendenza da Unity.
    /// </summary>
    public sealed class StubbornKetchupLogic
    {
        /// <summary>Gradi di rotazione per svitare il tappo (un giro); un tocco vale mezzo giro.</summary>
        public const float CapTurnNeeded = 360f, TapTurn = 180f;
        /// <summary>Quanto tirare la linguetta (unità del mondo) perché il sigillo venga via.</summary>
        public const float PeelDistance = 0.6f;

        public KetchupStage Stage { get; private set; } = KetchupStage.Playing;
        public bool CapOn { get; private set; } = true;
        public bool Sealed { get; private set; } = true;
        /// <summary>Il flacone è capovolto sopra le patatine.</summary>
        public bool Tipped { get; private set; }
        /// <summary>Gradi già svitati (0..CapTurnNeeded), si azzera solo rimettendo il tappo.</summary>
        public float CapTurn { get; private set; }

        public int Tips { get; private set; }
        public int Shakes { get; private set; }
        public int Squeezes { get; private set; }
        public int Slaps { get; private set; }
        public int Knifes { get; private set; }
        public int TabTugs { get; private set; }
        public int CapsRemoved { get; private set; }
        public int CapsReplaced { get; private set; }
        public bool CompletionReported { get; private set; }

        public bool InputLocked => Stage != KetchupStage.Playing;
        public bool Open => !Sealed;
        /// <summary>Tutte le mosse sbagliate fatte finora (solo statistica: non cambiano niente).</summary>
        public int WrongMoves => Tips + Shakes + Squeezes + Slaps + Knifes + TabTugs;
        /// <summary>Avanzamento dello svitamento, 0..1.</summary>
        public float CapProgress => CapOn ? Clamp01(CapTurn / CapTurnNeeded) : 1f;

        public void Reset()
        {
            Stage = KetchupStage.Playing;
            CapOn = Sealed = true;
            Tipped = false;
            CapTurn = 0f;
            Tips = Shakes = Squeezes = Slaps = Knifes = TabTugs = CapsRemoved = CapsReplaced = 0;
            CompletionReported = false;
        }

        static float Clamp01(float v) => v < 0f ? 0f : v > 1f ? 1f : v;

        KetchupResult Pour()
        {
            Stage = KetchupStage.Pouring;
            return KetchupResult.Pour;
        }

        // ---------- Il flacone ----------

        /// <summary>
        /// Il flacone portato sopra le patatine (si capovolge). Sigillato: non esce niente. Aperto: la soluzione.
        /// </summary>
        public KetchupResult TipOver()
        {
            if (InputLocked) return KetchupResult.None;
            Tipped = true;
            if (Open) return Pour();
            Tips++;
            return KetchupResult.Nothing;
        }

        /// <summary>Il flacone torna dritto al suo posto (nessun effetto sui progressi).</summary>
        public void Upright()
        {
            if (InputLocked) return;
            Tipped = false;
        }

        /// <summary>Il telefono scosso: al contrario sopra le patatine e aperto esce; altrimenti sbatacchia e basta.</summary>
        public KetchupResult Shake()
        {
            if (InputLocked) return KetchupResult.None;
            if (Open && Tipped) return Pour();
            Shakes++;
            return KetchupResult.Nothing;
        }

        /// <summary>
        /// Strizzare il flacone (pizzico a due dita o tocco). Aperto: esce sulle patatine (se è dritto, il livello lo
        /// porta prima sopra le patatine: si considera capovolto). Sigillato: "pfff", si gonfia.
        /// </summary>
        public KetchupResult Squeeze()
        {
            if (InputLocked) return KetchupResult.None;
            if (Open) { Tipped = true; return Pour(); }
            Squeezes++;
            return KetchupResult.Nothing;
        }

        /// <summary>Le pacche sul fondo: aperto e capovolto esce; altrimenti tonfo e niente.</summary>
        public KetchupResult Slap()
        {
            if (InputLocked) return KetchupResult.None;
            if (Open && Tipped) return Pour();
            Slaps++;
            return KetchupResult.Nothing;
        }

        /// <summary>Il coltello nel collo del flacone.</summary>
        public KnifeResult Knife()
        {
            if (InputLocked) return KnifeResult.None;
            if (Open) return KnifeResult.Open;
            Knifes++;
            return CapOn ? KnifeResult.Clean : KnifeResult.Foil;
        }

        // ---------- Tappo e sigillo ----------

        /// <summary>
        /// Gira il tappo (gradi in valore assoluto: in qualunque verso va bene). Restituisce true quando il tappo viene via.
        /// Solo a flacone dritto (capovolto il tappo è puntato sulle patatine: il livello lo raddrizza prima).
        /// </summary>
        public bool TurnCap(float degrees)
        {
            if (InputLocked || !CapOn || Tipped) return false;
            if (degrees < 0f) degrees = -degrees;
            CapTurn += degrees;
            if (CapTurn < CapTurnNeeded) return false;
            CapTurn = CapTurnNeeded;
            CapOn = false;
            CapsRemoved++;
            return true;
        }

        /// <summary>Un tocco sul tappo: mezzo giro (due tocchi lo tolgono).</summary>
        public bool TapCap() => TurnCap(TapTurn);

        /// <summary>Il tappo rimesso sul flacone (si riavvita, lo svitamento riparte da zero). Il sigillo resta com'era.</summary>
        public bool PutCapBack()
        {
            if (InputLocked || CapOn || Tipped) return false;
            CapOn = true;
            CapTurn = 0f;
            CapsReplaced++;
            return true;
        }

        /// <summary>La linguetta tirata di "distance" (unità del mondo) e poi lasciata.</summary>
        public PeelResult Peel(float distance)
        {
            if (InputLocked || !Sealed) return PeelResult.None;
            if (CapOn) { TabTugs++; return PeelResult.CapInTheWay; }
            if (distance < PeelDistance) return PeelResult.Partial;
            Sealed = false;
            return PeelResult.Peeled;
        }

        // ---------- Il finale ----------

        /// <summary>Fine del ketchup sulle patatine: vittoria, una volta sola.</summary>
        public bool TryComplete()
        {
            if (Stage != KetchupStage.Pouring || CompletionReported) return false;
            CompletionReported = true;
            Stage = KetchupStage.Done;
            return true;
        }
    }
}
