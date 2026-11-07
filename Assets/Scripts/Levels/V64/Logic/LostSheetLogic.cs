namespace Gotcha.Levels.V64
{
    /// <summary>Stato della stampante del livello 299 "The Impossible Print" (id lost_sheet).</summary>
    public enum LostSheetState
    {
        NoPaper,        // nessun foglio in tutto il livello: si può solo provare (e fallire in modo buffo)
        SheetArriving,  // il foglio portato da PAPERWORK NIGHTMARE entra svolazzando da sinistra (breve animazione)
        SheetOnDesk,    // il foglio è sulla scrivania: si può trascinare
        Dragging,       // il foglio è sotto il dito
        Feeding,        // lasciato sulla stampante: la stampante lo tira dentro (input chiuso)
        Printing,       // sta stampando (input chiuso)
        PageOut,        // esce il foglio con l'uovo d'oro e "You rock!" (festa, input chiuso)
        Won,            // vittoria registrata (una volta sola)
    }

    /// <summary>Cosa toccare (le aree della scena).</summary>
    public enum LostSheetTarget { PrintButton, Tray, Bin, Stapler, Printer }

    /// <summary>Cosa è successo dopo un input o un passo di tempo.</summary>
    public enum LostSheetEvent
    {
        None,
        Ignored,        // input durante l'animazione finale o mentre il foglio è in mano
        PrintNoPaper,   // PRINT senza carta: "PAPER JAM. Just kidding. NO PAPER."
        PrintNeedsFeed, // PRINT con il foglio sulla scrivania ma non nella stampante
        TrayEmpty,      // il vassoio della carta: vuoto
        BinNothing,     // il cestino: niente di utile
        StaplerNothing, // la cucitrice: niente da cucire
        PrinterBlink,   // tocco sulla stampante: la spia lampeggia
        Rattle,         // telefono scosso: la stampante sbatacchia
        SheetLanded,    // il foglio arrivato da fuori si è posato sulla scrivania
        Grabbed,        // il foglio è stato preso
        SheetReturned,  // il foglio lasciato altrove torna sulla scrivania (reversibile)
        Fed,            // il foglio lasciato sulla stampante: comincia il caricamento
        PrintStarted,   // il foglio è dentro: stampa
        PageOut,        // stampa finita: esce il foglio con l'uovo d'oro (il livello festeggia e chiama TryComplete)
    }

    /// <summary>
    /// Logica pura del livello 299 "The Impossible Print" (id lost_sheet) — "PRINT SOMETHING." Una stampante senza
    /// carta su una scrivania: dentro il livello non c'è nessun foglio, ogni tentativo fallisce in modo buffo (PRINT, il
    /// vassoio, il cestino, la cucitrice, la stampante, scuotere il telefono) e non blocca mai niente.
    /// La soluzione è un Easter egg fra due livelli: in PAPERWORK NIGHTMARE si trascina un foglio oltre il bordo DESTRO
    /// dello schermo (<see cref="ShouldLeaveRight"/>); il gioco salva subito la preferenza <see cref="CarriedKey"/> = 1.
    /// Tornati qui, il foglio entra svolazzando da sinistra; trascinato sulla stampante (zona generosa decisa dal livello)
    /// viene tirato dentro, stampato, ed esce un foglio con l'uovo d'oro e "You rock!" -> WinWithEgg("you_rock").
    /// Scelta sulla preferenza: una volta portato il foglio, la preferenza RESTA (non si consuma): ogni nuova partita del
    /// livello 299 ha di nuovo il foglio, così si può rigiocare. Nessuna sconfitta: Cancel() (pausa, dito perso) rimette
    /// il foglio sulla scrivania; le animazioni sono guidate da Tick e riprendono da sole.
    /// Nessuna dipendenza da Unity (le preferenze le legge e scrive il livello).
    /// </summary>
    public sealed class LostSheetLogic
    {
        /// <summary>Preferenza (PlayerPrefs, intero 1) scritta da PAPERWORK NIGHTMARE quando un foglio esce a destra.</summary>
        public const string CarriedKey = "gotcha.lostsheet.carried";
        /// <summary>Id dell'Easter egg (nome in EasterEggs: "You Rock!").</summary>
        public const string EggId = "you_rock";

        /// <summary>Il foglio di PAPERWORK NIGHTMARE "esce" quando il suo centro arriva a questo margine dal bordo destro.</summary>
        public const float EdgeMargin = 0.3f;

        public const float ArriveTime = 1.3f;     // il foglio che entra svolazzando
        public const float FeedTime = 0.7f;       // la stampante tira dentro il foglio
        public const float PrintTime = 1.1f;      // stampa (rumore, scossoni)
        public const float ShakeLevel = 12f;      // forza di scuotimento che fa sbatacchiare la stampante
        public const float ShakeCooldown = 1.2f;
        public const int NudgeAfter = 6;          // tentativi a vuoto (senza foglio) dopo cui arriva la battuta-indizio

        public static readonly string[] PrintLines =
        {
            "PAPER JAM. Just kidding. NO PAPER.",
            "Printing nothing... done. Nothing.",
            "Error 404: paper not found.",
            "It needs paper. Paper. PAPER.",
        };
        public static readonly string[] NeedsFeedLines =
        {
            "Put the paper IN the printer first.",
            "The paper goes in the slot on top.",
        };
        public static readonly string[] TrayLines =
        {
            "The paper tray is empty.",
            "Still empty. Shocking.",
            "Not even a sticky note.",
        };
        public static readonly string[] BinLines =
        {
            "Nothing useful in here.",
            "A banana peel. Not paper.",
            "Just regrets in here.",
        };
        public static readonly string[] StaplerLines =
        {
            "Ka-chunk! Nothing to staple.",
            "You stapled the air.",
        };
        public static readonly string[] PrinterLines =
        {
            "INSERT PAPER. INSERT PAPER.",
            "The light blinks. Angrily.",
            "Beep. No paper. Beep.",
        };
        public static readonly string[] RattleLines =
        {
            "*rattle rattle* Still no paper.",
            "Shaking it won't make paper.",
            "*clunk* Something fell off. Not paper.",
        };
        public const string NudgeLine = "Some other level is drowning in paper...";
        public const string ArriveLine = "...where did THAT come from?";

        float t, shakeCool;
        bool completed, nudged;
        int printN, feedN, trayN, binN, staplerN, printerN, rattleN;

        public LostSheetLogic() { Start(false); }

        public LostSheetState State { get; private set; }
        /// <summary>Il foglio portato da PAPERWORK NIGHTMARE è in questo livello (in arrivo, sulla scrivania o in mano).</summary>
        public bool HasSheet => State == LostSheetState.SheetArriving || State == LostSheetState.SheetOnDesk || State == LostSheetState.Dragging;
        /// <summary>Animazione finale in corso: nessun input conta.</summary>
        public bool InputLocked => State == LostSheetState.Feeding || State == LostSheetState.Printing || State == LostSheetState.PageOut || State == LostSheetState.Won;
        /// <summary>Tentativi a vuoto (tocchi e scossoni che non servono a niente).</summary>
        public int Misses { get; private set; }
        public bool Completed => completed;

        /// <summary>Avanzamento 0..1 della fase animata in corso (arrivo, caricamento, stampa); 1 nelle altre.</summary>
        public float Progress
        {
            get
            {
                float d = State == LostSheetState.SheetArriving ? ArriveTime : State == LostSheetState.Feeding ? FeedTime : State == LostSheetState.Printing ? PrintTime : 0f;
                return d > 0f ? Clamp01(t / d) : 1f;
            }
        }

        /// <summary>Inizio (o rientro) nel livello: <paramref name="carried"/> = la preferenza CarriedKey vale 1.</summary>
        public void Start(bool carried)
        {
            State = carried ? LostSheetState.SheetArriving : LostSheetState.NoPaper;
            t = 0f;
            shakeCool = 0f;
            completed = nudged = false;
            Misses = 0;
            printN = feedN = trayN = binN = staplerN = printerN = rattleN = 0;
        }

        public LostSheetEvent Tick(float dt)
        {
            if (dt < 0f) dt = 0f;
            if (shakeCool > 0f) shakeCool -= dt;
            switch (State)
            {
                case LostSheetState.SheetArriving:
                    t += dt;
                    if (t >= ArriveTime) { State = LostSheetState.SheetOnDesk; t = 0f; return LostSheetEvent.SheetLanded; }
                    break;
                case LostSheetState.Feeding:
                    t += dt;
                    if (t >= FeedTime) { State = LostSheetState.Printing; t = 0f; return LostSheetEvent.PrintStarted; }
                    break;
                case LostSheetState.Printing:
                    t += dt;
                    if (t >= PrintTime) { State = LostSheetState.PageOut; t = 0f; return LostSheetEvent.PageOut; }
                    break;
            }
            return LostSheetEvent.None;
        }

        /// <summary>Tocco su un'area della scena (mai vincente, mai bloccante).</summary>
        public LostSheetEvent Tap(LostSheetTarget target)
        {
            if (InputLocked || State == LostSheetState.Dragging) return LostSheetEvent.Ignored;
            LostSheetEvent e;
            switch (target)
            {
                case LostSheetTarget.PrintButton: e = HasSheet ? LostSheetEvent.PrintNeedsFeed : LostSheetEvent.PrintNoPaper; break;
                case LostSheetTarget.Tray: e = LostSheetEvent.TrayEmpty; break;
                case LostSheetTarget.Bin: e = LostSheetEvent.BinNothing; break;
                case LostSheetTarget.Stapler: e = LostSheetEvent.StaplerNothing; break;
                default: e = LostSheetEvent.PrinterBlink; break;
            }
            Misses++;
            return e;
        }

        /// <summary>Forza di scuotimento del telefono in questo fotogramma.</summary>
        public LostSheetEvent Shake(float strength)
        {
            if (InputLocked) return LostSheetEvent.Ignored;
            if (strength < ShakeLevel || shakeCool > 0f) return LostSheetEvent.None;
            shakeCool = ShakeCooldown;
            Misses++;
            return LostSheetEvent.Rattle;
        }

        /// <summary>Il dito è sceso sul foglio.</summary>
        public LostSheetEvent Grab()
        {
            if (State != LostSheetState.SheetOnDesk) return LostSheetEvent.Ignored;
            State = LostSheetState.Dragging;
            return LostSheetEvent.Grabbed;
        }

        /// <summary>Il dito ha lasciato il foglio: sopra la stampante (<paramref name="overPrinter"/>) lo carica, altrove torna sulla scrivania.</summary>
        public LostSheetEvent Release(bool overPrinter)
        {
            if (State != LostSheetState.Dragging) return LostSheetEvent.Ignored;
            t = 0f;
            if (overPrinter) { State = LostSheetState.Feeding; return LostSheetEvent.Fed; }
            State = LostSheetState.SheetOnDesk;
            return LostSheetEvent.SheetReturned;
        }

        /// <summary>Pausa, perdita del fuoco, secondo dito: il foglio in mano torna sulla scrivania. Le animazioni proseguono.</summary>
        public LostSheetEvent Cancel()
        {
            if (State != LostSheetState.Dragging) return LostSheetEvent.None;
            State = LostSheetState.SheetOnDesk;
            return LostSheetEvent.SheetReturned;
        }

        /// <summary>Fine della festa: true una volta sola (poi il livello chiama WinWithEgg).</summary>
        public bool TryComplete()
        {
            if (completed || State != LostSheetState.PageOut) return false;
            completed = true;
            State = LostSheetState.Won;
            return true;
        }

        /// <summary>La battuta-indizio (una volta per partita) dopo <see cref="NudgeAfter"/> tentativi a vuoto senza foglio.</summary>
        public bool TakeNudge()
        {
            if (nudged || HasSheet || InputLocked || Misses < NudgeAfter) return false;
            nudged = true;
            return true;
        }

        /// <summary>La frase per l'evento (a rotazione, ogni tipo per conto suo); null se l'evento non ha frase.</summary>
        public string LineFor(LostSheetEvent e)
        {
            switch (e)
            {
                case LostSheetEvent.PrintNoPaper: return PrintLines[printN++ % PrintLines.Length];
                case LostSheetEvent.PrintNeedsFeed: return NeedsFeedLines[feedN++ % NeedsFeedLines.Length];
                case LostSheetEvent.TrayEmpty: return TrayLines[trayN++ % TrayLines.Length];
                case LostSheetEvent.BinNothing: return BinLines[binN++ % BinLines.Length];
                case LostSheetEvent.StaplerNothing: return StaplerLines[staplerN++ % StaplerLines.Length];
                case LostSheetEvent.PrinterBlink: return PrinterLines[printerN++ % PrinterLines.Length];
                case LostSheetEvent.Rattle: return RattleLines[rattleN++ % RattleLines.Length];
                case LostSheetEvent.SheetLanded: return ArriveLine;
                default: return null;
            }
        }

        // ---------- Il gancio in PAPERWORK NIGHTMARE ----------

        /// <summary>
        /// Il foglio trascinato in PAPERWORK NIGHTMARE va via a destra? Sì quando il suo centro (posizione NON limitata,
        /// dito + presa) arriva a <paramref name="margin"/> dal bordo destro della camera, ma non mentre è sopra la bocca del
        /// cestino (entro <paramref name="binRadius"/>: lì vince il cestino, anche sugli schermi stretti dove il cestino è
        /// vicino al bordo).
        /// </summary>
        public static bool ShouldLeaveRight(float sheetX, float sheetY, float cameraRightEdge, float binX, float binY, float binRadius, float margin = EdgeMargin)
        {
            if (sheetX < cameraRightEdge - margin) return false;
            float dx = sheetX - binX, dy = sheetY - binY;
            return dx * dx + dy * dy >= binRadius * binRadius;
        }

        static float Clamp01(float v) => v < 0f ? 0f : v > 1f ? 1f : v;
    }
}
