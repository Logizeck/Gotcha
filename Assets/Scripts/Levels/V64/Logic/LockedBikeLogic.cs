namespace Gotcha.Levels.V64
{
    /// <summary>Fasi del livello 276.</summary>
    public enum LockedBikeStage
    {
        Playing,    // si prova di tutto
        Rolling,    // la bici (ancora col lucchetto) se ne va a destra: festa in corso, input bloccato
        Done,
        Busted,     // terza tronchesata: arriva il vigile (sconfitta), input bloccato
    }

    /// <summary>Dove sta la bici.</summary>
    public enum LockedBikeSpot
    {
        Locked,     // la catena gira attorno al palo e al telaio
        Free,       // sollevata oltre la cima del palo: la catena è sfilata (il lucchetto resta chiuso sul telaio)
    }

    /// <summary>La bici tirata su col dito (mentre la si tiene).</summary>
    public enum LockedBikeLift
    {
        None,       // input bloccato o bici già libera
        Rising,     // sale, ma la catena è ancora attorno al palo
        Cleared,    // la catena è passata sopra la cima del palo: la bici è libera (la soluzione)
    }

    /// <summary>La bici lasciata andare.</summary>
    public enum LockedBikeDrop
    {
        None,       // input bloccato
        SnapBack,   // ancora legata al palo: ricade giù al suo posto (il sollevamento non era abbastanza alto)
        Parked,     // libera: posata a terra dove la si è lasciata
        Rolled,     // libera e portata a destra: verso casa (vittoria dopo la festa)
    }

    /// <summary>Una chiave del mazzo provata nel lucchetto.</summary>
    public enum LockedBikeKey
    {
        None,
        Wrong,          // "Wrong key."
        HouseKey,       // la quinta: è la chiave di casa
        BottleOpener,   // la sesta: è un apribottiglie
    }

    /// <summary>Una combinazione provata sulle 4 rotelle.</summary>
    public enum LockedBikeCode
    {
        None,
        Zeros,      // "0000? Really?"
        Obvious,    // 1234
        Wrong,      // un numero a caso: mai giusto
    }

    /// <summary>La tronchese sulla catena: tre colpi e arriva il vigile.</summary>
    public enum LockedBikeCut
    {
        None,
        Bounce,     // 1° colpo: rimbalza via ("Hardened steel. Your arms are not.") e si sente un fischietto lontano
        Bend,       // 2° colpo: si piega (poi torna dritta) e il vigile si affaccia a bordo schermo, sospettoso
        Busted,     // 3° colpo: il vigile ti ferma ("Stealing your own bike? Tell it to the judge.") -> sconfitta
    }

    /// <summary>Calcio/scossone al palo.</summary>
    public enum LockedBikePole
    {
        None,
        Wobble,             // il palo ondeggia (il piccione era già volato via)
        WobblePigeonFlew,   // il palo ondeggia e il piccione vola via (poi torna)
    }

    /// <summary>Cosa è cambiato da solo col passare del tempo.</summary>
    public enum LockedBikeTick
    {
        None,
        PigeonBack, // il piccione è tornato sulla cima del palo
    }

    /// <summary>
    /// Logica pura del livello 276 "The Locked Bike" — "Take your bike home!" Una strada: la bici è legata con una grossa
    /// catena e un lucchetto a combinazione a un palo CORTO (il cartello che c'era in cima è caduto ed è per terra lì
    /// accanto); un piccione sta appollaiato sulla cima del palo; sul marciapiede un mazzo di chiavi e una tronchese.
    /// False piste (nessuna azzera i progressi, niente sconfitta): le chiavi (nessuna entra: "Wrong key." più volte, poi la
    /// chiave di casa, poi un apribottiglie), le rotelle della combinazione (mai giusta: "0000? Really?"), la tronchese
    /// (rimbalza o si piega: "Hardened steel. Your arms are not."), calciare/scuotere il palo (ondeggia, il piccione vola
    /// via e torna), tirare la bici di lato (la catena la trattiene), sollevarla poco (ricade giù).
    /// Sconfitta (regola del proprietario: ogni livello si può perdere): la tronchese conta i colpi — al primo un fischietto
    /// lontano, al secondo il vigile si affaccia ("Hey! What are you doing?"), al terzo ti arresta (Fail, una volta sola;
    /// dopo niente festa né vittoria). La soluzione resta sempre raggiungibile: non c'è un limite di tempo.
    /// Indizio: il cartello caduto ("the pole has no top"), la cima del palo più bassa del manubrio quando si solleva la
    /// bici, il piccione proprio sulla cima.
    /// Soluzione: sollevare la bici (trascinarla in su, generoso) oltre la cima del palo: la catena si sfila dall'alto col
    /// lucchetto ancora chiuso sul telaio; poi portare la bici a destra -> vittoria, una volta sola.
    /// Nessuna dipendenza da Unity.
    /// </summary>
    public sealed class LockedBikeLogic
    {
        /// <summary>Sollevamento (unità del mondo) oltre il quale la catena passa sopra la cima del palo.</summary>
        public const float ClearLift = 1.0f;
        /// <summary>Sollevamento massimo mentre la bici è ancora legata.</summary>
        public const float MaxLift = 1.9f;
        /// <summary>Sotto questo sollevamento il rilascio non conta come tentativo (era solo un tocco tremolante).</summary>
        public const float MinLift = 0.12f;
        /// <summary>Gioco laterale della catena (la bici legata si sposta di lato al massimo di tanto).</summary>
        public const float Slack = 0.14f;
        /// <summary>Trascinamento laterale oltre il quale la bici legata "strattona" la catena.</summary>
        public const float YankDrag = 0.45f;
        /// <summary>Altezza della bici sollevata alla quale il piccione si spaventa e vola via.</summary>
        public const float PigeonSpook = 0.45f;
        /// <summary>Secondi prima che il piccione torni sulla cima del palo.</summary>
        public const float PigeonAwaySeconds = 2.4f;
        /// <summary>Posizione x (mondo) del centro della bici oltre la quale la bici libera parte verso casa.</summary>
        public const float ExitX = 1.45f;
        /// <summary>Limite sinistro dove si può posare la bici libera.</summary>
        public const float ParkMinX = -1.2f;
        /// <summary>Le cose sul portachiavi: 4 chiavi sbagliate, la chiave di casa, l'apribottiglie.</summary>
        public const int KeyCount = 6;
        /// <summary>Colpi di tronchese che fanno arrivare il vigile (al terzo si perde).</summary>
        public const int CutStrikes = 3;

        public LockedBikeStage Stage { get; private set; } = LockedBikeStage.Playing;
        public LockedBikeSpot Spot { get; private set; } = LockedBikeSpot.Locked;
        /// <summary>Dove è posata la bici libera (x del mondo); quella legata sta sempre al suo posto.</summary>
        public float ParkX { get; private set; }
        public float HomeX { get; private set; }
        public bool PigeonAway { get; private set; }
        public float PigeonAwayLeft { get; private set; }

        public int KeyTries { get; private set; }
        public int CodeTries { get; private set; }
        public int CutTries { get; private set; }
        public int PoleKicks { get; private set; }
        public int Yanks { get; private set; }
        public int LowLifts { get; private set; }
        public int PigeonFlights { get; private set; }
        public int PigeonTaps { get; private set; }
        public int SignReads { get; private set; }
        public int BikeTaps { get; private set; }
        public int Parks { get; private set; }
        public int LastCode { get; private set; }
        public bool CompletionReported { get; private set; }
        public bool FailReported { get; private set; }

        public LockedBikeLogic(float homeX = 0.4f)
        {
            HomeX = homeX;
            ParkX = homeX;
        }

        public bool InputLocked => Stage != LockedBikeStage.Playing;
        public bool Free => Spot == LockedBikeSpot.Free;
        /// <summary>Il lucchetto è sempre chiuso (anche alla fine: resta sul telaio).</summary>
        public bool LockClosed => true;
        /// <summary>Il vigile ha sentito il fischietto (dopo il 1° colpo di tronchese).</summary>
        public bool WhistleHeard => CutTries >= 1;
        /// <summary>Il vigile è a bordo schermo e guarda (dopo il 2° colpo di tronchese).</summary>
        public bool OfficerWatching => CutTries >= CutStrikes - 1;
        /// <summary>Colpi di tronchese ancora "permessi" prima dell'arresto.</summary>
        public int CutsLeft => CutStrikes - CutTries < 0 ? 0 : CutStrikes - CutTries;
        public bool Busted => Stage == LockedBikeStage.Busted;
        /// <summary>Dove sta la bici a riposo (x del mondo).</summary>
        public float RestX => Free ? ParkX : HomeX;
        /// <summary>Tutte le mosse sbagliate fatte finora (solo statistica: non cambiano niente).</summary>
        public int WrongMoves => KeyTries + CodeTries + CutTries + PoleKicks + Yanks + LowLifts;

        public void Reset()
        {
            Stage = LockedBikeStage.Playing;
            Spot = LockedBikeSpot.Locked;
            ParkX = HomeX;
            PigeonAway = false;
            PigeonAwayLeft = 0f;
            KeyTries = CodeTries = CutTries = PoleKicks = Yanks = LowLifts = 0;
            PigeonFlights = PigeonTaps = SignReads = BikeTaps = Parks = LastCode = 0;
            CompletionReported = FailReported = false;
        }

        // ---------- Il tempo ----------

        /// <summary>Fa passare il tempo (secondi di gioco, fermi in pausa): il piccione torna sul palo.</summary>
        public LockedBikeTick Tick(float dt)
        {
            if (dt <= 0f || !PigeonAway || Stage == LockedBikeStage.Done || Stage == LockedBikeStage.Busted) return LockedBikeTick.None;
            PigeonAwayLeft -= dt;
            if (PigeonAwayLeft > 0f) return LockedBikeTick.None;
            PigeonAway = false;
            PigeonAwayLeft = 0f;
            return LockedBikeTick.PigeonBack;
        }

        /// <summary>Il piccione vola via (se era sul palo). True se è appena partito.</summary>
        bool Fly()
        {
            if (PigeonAway) { PigeonAwayLeft = PigeonAwaySeconds; return false; }
            PigeonAway = true;
            PigeonAwayLeft = PigeonAwaySeconds;
            PigeonFlights++;
            return true;
        }

        /// <summary>Il piccione torna subito (la festa finale lo vuole sul palo).</summary>
        public void PigeonHome()
        {
            PigeonAway = false;
            PigeonAwayLeft = 0f;
        }

        // ---------- La bici ----------

        /// <summary>
        /// La bici tenuta col dito e sollevata di <paramref name="height"/> (unità del mondo): appena la catena supera la
        /// cima del palo la bici è libera. Una bici sollevata abbastanza spaventa il piccione.
        /// </summary>
        public LockedBikeLift Lift(float height)
        {
            if (InputLocked || Free) return LockedBikeLift.None;
            if (height >= PigeonSpook && !PigeonAway) Fly();
            if (height < ClearLift) return LockedBikeLift.Rising;
            Spot = LockedBikeSpot.Free;
            ParkX = HomeX;
            return LockedBikeLift.Cleared;
        }

        /// <summary>
        /// La bici lasciata andare: legata ricade al suo posto (un tentativo se era stata sollevata almeno MinLift),
        /// libera si posa in x (o, oltre ExitX, parte verso casa).
        /// </summary>
        public LockedBikeDrop Drop(float x, float lifted)
        {
            if (InputLocked) return LockedBikeDrop.None;
            if (!Free)
            {
                if (lifted >= MinLift) LowLifts++;
                return LockedBikeDrop.SnapBack;
            }
            if (x >= ExitX)
            {
                Stage = LockedBikeStage.Rolling;
                ParkX = x;
                return LockedBikeDrop.Rolled;
            }
            Park(x);
            return LockedBikeDrop.Parked;
        }

        /// <summary>Posa la bici libera in x senza mai farla partire (pausa, dito perso). True se era libera.</summary>
        public bool Park(float x)
        {
            if (InputLocked || !Free) return false;
            float max = ExitX - 0.05f;
            ParkX = x < ParkMinX ? ParkMinX : x > max ? max : x;
            Parks++;
            return true;
        }

        /// <summary>La bici legata tirata di lato: la catena la trattiene. True se conta.</summary>
        public bool Yank()
        {
            if (InputLocked || Free) return false;
            Yanks++;
            return true;
        }

        /// <summary>Toccare la bici. True se conta.</summary>
        public bool TapBike()
        {
            if (InputLocked) return false;
            BikeTaps++;
            return true;
        }

        // ---------- Le false piste ----------

        /// <summary>La prossima chiave del mazzo nel lucchetto: mai quella giusta (dopo l'apribottiglie si ricomincia).</summary>
        public LockedBikeKey TryKey()
        {
            if (InputLocked) return LockedBikeKey.None;
            KeyTries++;
            int i = (KeyTries - 1) % KeyCount;
            if (i == KeyCount - 2) return LockedBikeKey.HouseKey;
            if (i == KeyCount - 1) return LockedBikeKey.BottleOpener;
            return LockedBikeKey.Wrong;
        }

        /// <summary>
        /// La combinazione del tentativo numero <paramref name="attempt"/> (da 0): prima 0000 (le rotelle partono così), al
        /// quarto tentativo 1234, poi numeri "a caso" ma ripetibili. Mai quella giusta, perché non ce n'è una da indovinare.
        /// </summary>
        public static int CodeFor(int attempt)
        {
            if (attempt <= 0) return 0;
            if (attempt == 3) return 1234;
            uint h = (uint)attempt * 2654435761u + 276u;
            h ^= h >> 13;
            h *= 2246822519u;
            h ^= h >> 16;
            int c = (int)(h % 10000u);
            if (c == 0 || c == 1234) c = 4711;
            return c;
        }

        /// <summary>Il codice in 4 cifre ("0427").</summary>
        public static string CodeLabel(int code) => (code < 0 ? 0 : code % 10000).ToString("0000");

        /// <summary>Gira le rotelle sulla prossima combinazione e la prova: non si apre mai.</summary>
        public LockedBikeCode TryCode()
        {
            if (InputLocked) return LockedBikeCode.None;
            LastCode = CodeFor(CodeTries);
            CodeTries++;
            if (LastCode == 0) return LockedBikeCode.Zeros;
            if (LastCode == 1234) return LockedBikeCode.Obvious;
            return LockedBikeCode.Wrong;
        }

        /// <summary>
        /// La tronchese sulla catena: mai un taglio. Il 1° colpo rimbalza (fischietto lontano), il 2° la piega (il vigile
        /// si affaccia), il 3° fa arrestare il giocatore: la partita è persa (vale anche con la bici già libera).
        /// </summary>
        public LockedBikeCut Cut()
        {
            if (InputLocked) return LockedBikeCut.None;
            CutTries++;
            if (CutTries >= CutStrikes)
            {
                Stage = LockedBikeStage.Busted;
                return LockedBikeCut.Busted;
            }
            return CutTries == 1 ? LockedBikeCut.Bounce : LockedBikeCut.Bend;
        }

        /// <summary>Calciare o scuotere il palo (anche scuotendo il telefono): ondeggia, il piccione vola via.</summary>
        public LockedBikePole KickPole()
        {
            if (InputLocked) return LockedBikePole.None;
            PoleKicks++;
            return Fly() ? LockedBikePole.WobblePigeonFlew : LockedBikePole.Wobble;
        }

        /// <summary>Toccare il piccione: svolazza via e torna. True se è partito adesso.</summary>
        public bool TapPigeon()
        {
            if (InputLocked) return false;
            PigeonTaps++;
            return Fly();
        }

        /// <summary>Leggere il cartello caduto (l'indizio). True se conta.</summary>
        public bool ReadSign()
        {
            if (InputLocked) return false;
            SignReads++;
            return true;
        }

        // ---------- Il finale ----------

        /// <summary>Fine della festa (bici andata via col lucchetto ancora chiuso): vittoria, una volta sola.</summary>
        public bool TryComplete()
        {
            if (Stage != LockedBikeStage.Rolling || CompletionReported) return false;
            CompletionReported = true;
            Stage = LockedBikeStage.Done;
            return true;
        }

        /// <summary>Fine della scena dell'arresto: sconfitta, una volta sola (mai dopo una vittoria).</summary>
        public bool TryFail()
        {
            if (Stage != LockedBikeStage.Busted || FailReported || CompletionReported) return false;
            FailReported = true;
            return true;
        }
    }
}
