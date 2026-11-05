namespace Gotcha.Levels.V64
{
    /// <summary>Fasi del livello "The Stupid Genius 2".</summary>
    public enum StupidGenius2Stage
    {
        Wrong,        // la lavagna dice 2 + 2 = 5: ogni risposta è "sbagliata"
        Fixing,       // il 5 è stato lasciato sul pulsante del 4: animazione dello scambio (input bloccato)
        Fixed,        // la lavagna dice 2 + 2 = 4: ora conta il valore giusto
        Celebrating,  // risposta giusta: festa in corso (input bloccato)
        Done,         // vittoria già data
    }

    /// <summary>Esito della pressione di un pulsante-risposta (per VALORE logico, mai per indice).</summary>
    public enum StupidGenius2Answer
    {
        None,               // input bloccato o trascinamento in corso: niente
        BoardSaysFive,      // prima della correzione, la risposta vera (4): "WRONG! The board says 5."
        AgreedWithBoard,    // prima della correzione, il risultato scritto sulla lavagna (5): "You agreed with a wrong equation."
        NotEvenClose,       // prima della correzione, un altro valore (3): "Not even close."
        BoardAgreesWrong,   // dopo la correzione, un valore sbagliato: "Wrong. And now the board agrees."
        Correct,            // dopo la correzione, il valore uguale al risultato della lavagna: VITTORIA (una volta sola)
    }

    /// <summary>Esito di un dito che scende su un simbolo della lavagna.</summary>
    public enum StupidGenius2Grab
    {
        None,          // input bloccato, simbolo non valido o un altro dito sta già trascinando
        Dragging,      // il risultato sbagliato (il 5) si stacca dalla lavagna e segue il dito
        NotDraggable,  // un altro simbolo (2, +, =, o il 4 già corretto): oscilla e basta
    }

    /// <summary>Esito del dito che lascia il 5.</summary>
    public enum StupidGenius2Drop
    {
        None,        // il dito non trascinava niente (o input bloccato)
        Tap,         // toccato senza trascinare: torna al suo posto ("Don't touch my 5.")
        Returned,    // lasciato in un punto qualunque: torna sulla lavagna
        DoesntHelp,  // lasciato su un pulsante di valore sbagliato (3 o 5): torna sulla lavagna ("That doesn't help.")
        Fixed,       // lasciato sul pulsante col valore giusto (4): la lavagna diventa 2 + 2 = 4
    }

    /// <summary>
    /// Logica pura del livello 44 della roadmap "The Stupid Genius 2" (id stupid_genius_2) — "PROVE YOU'RE SMARTER THAN THE
    /// GENIUS." Un genio arrogante accanto a una lavagna con "2 + 2 = 5"; sotto, tre pulsanti-risposta (3, 4, 5).
    /// Inganno: prima della correzione OGNI risposta è dichiarata sbagliata (anche il 4: "la lavagna dice 5").
    /// Soluzione esatta: trascinare il 5 della lavagna sul pulsante del 4 (il risultato si corregge: la lavagna dice
    /// 2 + 2 = 4; il pulsante del 4 resta un 4), poi premere il pulsante il cui VALORE è uguale al risultato della lavagna.
    /// I pulsanti sono identificati solo dal loro valore logico (il livello passa il numero mostrato, mai l'indice):
    /// l'ordine dei pulsanti non conta.
    /// Simboli della lavagna: 0 "2", 1 "+", 2 "2", 3 "=", 4 il risultato (<see cref="ResultSymbol"/>); solo il risultato
    /// sbagliato si trascina. Un dito alla volta trascina. Ogni mossa sbagliata è reversibile; Cancel() (pausa, app in
    /// background, livello spento) rimette il 5 al suo posto senza contare niente. Non si perde mai. Nessuna dipendenza da
    /// Unity.
    /// </summary>
    public sealed class StupidGenius2Logic
    {
        /// <summary>Gli addendi e il risultato sbagliato scritto all'inizio.</summary>
        public const int A = 2, B = 2, WrongResult = 5;
        /// <summary>Il risultato vero (2 + 2).</summary>
        public const int TrueSum = A + B;
        /// <summary>Numero di simboli della lavagna e indice del risultato.</summary>
        public const int SymbolCount = 5, ResultSymbol = 4;
        /// <summary>"Nessun pulsante sotto il 5 lasciato".</summary>
        public const int NoTarget = int.MinValue;
        /// <summary>"Nessun dito".</summary>
        public const int NoFinger = int.MinValue;
        /// <summary>Spostamento del dito oltre il quale non è più un tocco.</summary>
        public const float TapSlop = 0.15f;

        int finger = NoFinger;
        float sx, sy;
        bool moved, completed;

        public StupidGenius2Stage Stage { get; private set; } = StupidGenius2Stage.Wrong;
        /// <summary>Input bloccato (scambio in corso, festa, finito).</summary>
        public bool InputLocked => Stage == StupidGenius2Stage.Fixing || Stage == StupidGenius2Stage.Celebrating || Stage == StupidGenius2Stage.Done;
        /// <summary>La lavagna è già stata corretta (anche durante l'animazione dello scambio).</summary>
        public bool BoardFixed => Stage != StupidGenius2Stage.Wrong;
        /// <summary>Il risultato scritto sulla lavagna in questo momento.</summary>
        public int BoardResult => BoardFixed ? TrueSum : WrongResult;
        public bool Dragging => finger != NoFinger;
        public int DragFinger => finger;
        /// <summary>Il dito che trascina si è mosso oltre <see cref="TapSlop"/>.</summary>
        public bool DragMoved => Dragging && moved;

        public int WrongAnswers { get; private set; }
        public int WrongDrops { get; private set; }
        public int Returns { get; private set; }
        public int Taps { get; private set; }
        public int Wiggles { get; private set; }
        public int WrongMoves => WrongAnswers + WrongDrops + Returns;

        public StupidGenius2Logic() { Reset(); }

        public void Reset()
        {
            Stage = StupidGenius2Stage.Wrong;
            finger = NoFinger;
            sx = sy = 0f;
            moved = completed = false;
            WrongAnswers = WrongDrops = Returns = Taps = Wiggles = 0;
        }

        // ---------- Pulsanti-risposta ----------

        /// <summary>Si preme il pulsante che MOSTRA il valore <paramref name="value"/>.</summary>
        public StupidGenius2Answer Answer(int value)
        {
            if (InputLocked || Dragging || value == NoTarget) return StupidGenius2Answer.None;
            if (Stage == StupidGenius2Stage.Fixed)
            {
                if (value == BoardResult)
                {
                    Stage = StupidGenius2Stage.Celebrating;
                    return StupidGenius2Answer.Correct;
                }
                WrongAnswers++;
                return StupidGenius2Answer.BoardAgreesWrong;
            }
            WrongAnswers++;
            if (value == TrueSum) return StupidGenius2Answer.BoardSaysFive;
            if (value == WrongResult) return StupidGenius2Answer.AgreedWithBoard;
            return StupidGenius2Answer.NotEvenClose;
        }

        /// <summary>L'indice del pulsante giusto in questo momento (valore == risultato della lavagna corretta), -1 se nessuno.</summary>
        public int CorrectButton(int[] values)
        {
            if (!BoardFixed || values == null) return -1;
            for (int i = 0; i < values.Length; i++) if (values[i] == BoardResult) return i;
            return -1;
        }

        // ---------- Il 5 della lavagna ----------

        /// <summary>Il dito <paramref name="id"/> scende sul simbolo <paramref name="symbol"/> in (x, y).</summary>
        public StupidGenius2Grab Grab(int id, int symbol, float x, float y)
        {
            if (InputLocked || id == NoFinger || Dragging || symbol < 0 || symbol >= SymbolCount || Bad(x) || Bad(y)) return StupidGenius2Grab.None;
            if (symbol != ResultSymbol || Stage != StupidGenius2Stage.Wrong)
            {
                Wiggles++;
                return StupidGenius2Grab.NotDraggable;
            }
            finger = id;
            sx = x;
            sy = y;
            moved = false;
            return StupidGenius2Grab.Dragging;
        }

        /// <summary>Il dito che trascina si sposta in (x, y). Vero se è il dito che trascina.</summary>
        public bool Move(int id, float x, float y)
        {
            if (InputLocked || id == NoFinger || id != finger || Bad(x) || Bad(y)) return false;
            float dx = x - sx, dy = y - sy;
            if (dx * dx + dy * dy > TapSlop * TapSlop) moved = true;
            return true;
        }

        /// <summary>
        /// Il dito lascia il 5 sopra il pulsante di valore <paramref name="targetValue"/> (<see cref="NoTarget"/> se non è
        /// sopra nessun pulsante).
        /// </summary>
        public StupidGenius2Drop Drop(int id, int targetValue)
        {
            if (id == NoFinger || id != finger) return StupidGenius2Drop.None;
            bool wasMoved = moved;
            finger = NoFinger;
            moved = false;
            if (InputLocked) return StupidGenius2Drop.None;
            if (!wasMoved && targetValue == NoTarget) { Taps++; return StupidGenius2Drop.Tap; }
            if (targetValue == NoTarget) { Returns++; return StupidGenius2Drop.Returned; }
            if (targetValue == TrueSum)
            {
                Stage = StupidGenius2Stage.Fixing;
                return StupidGenius2Drop.Fixed;
            }
            WrongDrops++;
            return StupidGenius2Drop.DoesntHelp;
        }

        /// <summary>Fine dell'animazione dello scambio: la lavagna corretta accetta di nuovo l'input. Vero se cambia stato.</summary>
        public bool FinishFix()
        {
            if (Stage != StupidGenius2Stage.Fixing) return false;
            Stage = StupidGenius2Stage.Fixed;
            return true;
        }

        /// <summary>Dita perse (pausa, app in background, livello spento): il 5 torna al suo posto, niente conta.</summary>
        public bool Cancel()
        {
            bool any = Dragging;
            finger = NoFinger;
            moved = false;
            return any;
        }

        /// <summary>Fine della festa: true una volta sola (allora il livello chiama Win).</summary>
        public bool TryComplete()
        {
            if (Stage != StupidGenius2Stage.Celebrating || completed) return false;
            completed = true;
            Stage = StupidGenius2Stage.Done;
            return true;
        }

        static bool Bad(float v) => float.IsNaN(v) || float.IsInfinity(v);
    }
}
