namespace Gotcha.Levels.V63
{
    /// <summary>Gli oggetti del livello 256. Il palloncino è legato vicino al campanello: negli slot non ci resta.</summary>
    public enum ChainItem { None, Ball, Book, Domino, Candle, Magnet, Balloon }

    /// <summary>Stati del livello 256 (la macchina alla Rube Goldberg).</summary>
    public enum ChainState
    {
        Building,   // si sistemano gli oggetti negli slot
        Running,    // la catena è partita (animazione, input bloccato)
        Ringing,    // il palloncino ha suonato il campanello (input bloccato)
        Done,
    }

    /// <summary>Come finisce una partenza della catena: Ding è la vittoria, il resto è il primo anello rotto.</summary>
    public enum ChainOutcome
    {
        Ding,
        NothingToPush,      // slot 1 vuoto
        StartDomino,        // slot 1: i domino cadono sulla mensola
        StartBook,          // slot 1: il libro cade piatto
        StartCandle,        // slot 1: la candela si rovescia
        StartMagnet,        // slot 1: la calamita non rotola
        NoRamp,             // slot 2 vuoto: la palla si ferma sul bordo
        RampDomino,         // slot 2: domino al posto della rampa
        RampCandle,         // slot 2: candela al posto della rampa
        MagnetGrabsBall,    // slot 2 o 3: la calamita si tiene la palla
        BallRollsAway,      // slot 3 vuoto: la palla va per conto suo
        HitsBook,           // slot 3: la palla sbatte sul libro
        HitsCandle,         // slot 3: la palla butta giù la candela
        ToastedDominoes,    // slot 3 candela e slot 4 domino: la candela cade sui domino
        NoCandle,           // slot 4 vuoto: l'ultimo domino cade sul niente
        DominoOnMagnet,     // slot 4: la calamita si prende il domino
        DominoOnBook,       // slot 4: l'ultimo domino cade sul libro
        DominoOnBall,       // slot 4: l'ultimo domino tocca la palla (non può succedere: la palla è una sola)
    }

    /// <summary>Esito di una partenza: quanti anelli hanno funzionato (0-4) e come è finita.</summary>
    public readonly struct ChainRun
    {
        public readonly int Links;
        public readonly ChainOutcome Outcome;
        public ChainRun(int links, ChainOutcome outcome) { Links = links; Outcome = outcome; }
        public bool Success => Outcome == ChainOutcome.Ding;
        /// <summary>Lo slot (0-3) dove la catena si è rotta; -1 se ha funzionato.</summary>
        public int BrokenSlot => Success ? -1 : Links;
    }

    /// <summary>
    /// Logica pura del livello 256 "Chain reaction": quattro slot lungo il percorso (in cima alla mensola, la rampa,
    /// il pavimento, prima del filo del palloncino) e cinque oggetti (palla, libro, fila di domino, candela, calamita).
    /// GO fa partire la catena (deterministica): palla -> giù dal libro -> domino -> l'ultimo domino spinge la candela
    /// sotto il filo -> il filo brucia, il palloncino sale e suona il campanello. Ogni combinazione sbagliata si ferma al
    /// primo anello rotto con una battuta; gli oggetti restano negli slot, si corregge e si riprova (tentativi
    /// illimitati). Il palloncino messo in uno slot vola via subito (e ricompare). Nessuna dipendenza da Unity.
    /// </summary>
    public sealed class ChainReactionLogic
    {
        public const int Slots = 4;

        readonly ChainItem[] slots = new ChainItem[Slots];

        public ChainState State { get; private set; } = ChainState.Building;
        public int Attempts { get; private set; }
        public int BalloonEscapes { get; private set; }
        public ChainRun LastRun { get; private set; }
        public bool CompletionReported { get; private set; }

        public bool InputLocked => State != ChainState.Building;

        public void Reset()
        {
            for (int i = 0; i < Slots; i++) slots[i] = ChainItem.None;
            State = ChainState.Building;
            Attempts = 0;
            BalloonEscapes = 0;
            LastRun = default;
            CompletionReported = false;
        }

        public ChainItem SlotItem(int slot) => slot >= 0 && slot < Slots ? slots[slot] : ChainItem.None;

        /// <summary>In che slot è l'oggetto (-1 se è sul tavolo).</summary>
        public int SlotOf(ChainItem item)
        {
            if (item == ChainItem.None) return -1;
            for (int i = 0; i < Slots; i++) if (slots[i] == item) return i;
            return -1;
        }

        /// <summary>
        /// Mette un oggetto in uno slot (togliendolo da dov'era). Se lo slot era occupato, l'oggetto che c'era torna
        /// sul tavolo (<paramref name="displaced"/>). Il palloncino non si può mettere: vedi TryBalloonInSlot.
        /// </summary>
        public bool TryPlace(ChainItem item, int slot, out ChainItem displaced)
        {
            displaced = ChainItem.None;
            if (InputLocked || item == ChainItem.None || item == ChainItem.Balloon || slot < 0 || slot >= Slots) return false;
            int from = SlotOf(item);
            if (from == slot) return true;
            if (from >= 0) slots[from] = ChainItem.None;
            displaced = slots[slot];
            slots[slot] = item;
            return true;
        }

        /// <summary>L'oggetto preso da uno slot torna sul tavolo.</summary>
        public bool TryRemove(ChainItem item)
        {
            if (InputLocked) return false;
            int from = SlotOf(item);
            if (from < 0) return false;
            slots[from] = ChainItem.None;
            return true;
        }

        /// <summary>Il palloncino lasciato in uno slot: vola via subito ("Bye bye, balloon.") e ricompare legato.</summary>
        public bool TryBalloonInSlot()
        {
            if (InputLocked) return false;
            BalloonEscapes++;
            return true;
        }

        /// <summary>GO: la catena parte con gli oggetti negli slot. L'esito si mostra, poi TryFinishRun.</summary>
        public bool TryGo(out ChainRun run)
        {
            run = default;
            if (State != ChainState.Building) return false;
            Attempts++;
            run = Evaluate(slots[0], slots[1], slots[2], slots[3]);
            LastRun = run;
            State = ChainState.Running;
            return true;
        }

        /// <summary>Fine dell'animazione: se ha suonato si va avanti, altrimenti la scena si rimette com'era.</summary>
        public bool TryFinishRun()
        {
            if (State != ChainState.Running) return false;
            State = LastRun.Success ? ChainState.Ringing : ChainState.Building;
            return true;
        }

        public bool TryCompleteRing()
        {
            if (State != ChainState.Ringing || CompletionReported) return false;
            State = ChainState.Done;
            CompletionReported = true;
            return true;
        }

        /// <summary>La catena, anello per anello: si ferma al primo che non funziona.</summary>
        public static ChainRun Evaluate(ChainItem s1, ChainItem s2, ChainItem s3, ChainItem s4)
        {
            // 0: la spinta iniziale in cima alla mensola
            switch (s1)
            {
                case ChainItem.Ball: break;
                case ChainItem.Domino: return new ChainRun(0, ChainOutcome.StartDomino);
                case ChainItem.Book: return new ChainRun(0, ChainOutcome.StartBook);
                case ChainItem.Candle: return new ChainRun(0, ChainOutcome.StartCandle);
                case ChainItem.Magnet: return new ChainRun(0, ChainOutcome.StartMagnet);
                default: return new ChainRun(0, ChainOutcome.NothingToPush);
            }
            // 1: dalla mensola al pavimento serve una rampa
            switch (s2)
            {
                case ChainItem.Book: break;
                case ChainItem.Domino: return new ChainRun(1, ChainOutcome.RampDomino);
                case ChainItem.Candle: return new ChainRun(1, ChainOutcome.RampCandle);
                case ChainItem.Magnet: return new ChainRun(1, ChainOutcome.MagnetGrabsBall);
                default: return new ChainRun(1, ChainOutcome.NoRamp);
            }
            // 2: la palla corre sul pavimento e deve colpire la fila di domino
            switch (s3)
            {
                case ChainItem.Domino: break;
                case ChainItem.Magnet: return new ChainRun(2, ChainOutcome.MagnetGrabsBall);
                case ChainItem.Book: return new ChainRun(2, ChainOutcome.HitsBook);
                case ChainItem.Candle: return new ChainRun(2, s4 == ChainItem.Domino ? ChainOutcome.ToastedDominoes : ChainOutcome.HitsCandle);
                default: return new ChainRun(2, ChainOutcome.BallRollsAway);
            }
            // 3: l'ultimo domino spinge quello che c'è sotto il filo del palloncino
            switch (s4)
            {
                case ChainItem.Candle: return new ChainRun(4, ChainOutcome.Ding);
                case ChainItem.Magnet: return new ChainRun(3, ChainOutcome.DominoOnMagnet);
                case ChainItem.Book: return new ChainRun(3, ChainOutcome.DominoOnBook);
                case ChainItem.Ball: return new ChainRun(3, ChainOutcome.DominoOnBall);
                default: return new ChainRun(3, ChainOutcome.NoCandle);
            }
        }

        /// <summary>La battuta (in inglese, è testo di gioco) per ogni esito.</summary>
        public static string Joke(ChainOutcome o)
        {
            switch (o)
            {
                case ChainOutcome.Ding: return "DING!";
                case ChainOutcome.NothingToPush: return "GO... where? The shelf is empty.";
                case ChainOutcome.StartDomino: return "The dominoes fall over. On the shelf. Riveting.";
                case ChainOutcome.StartBook: return "The book falls flat. Plot twist: nothing happens.";
                case ChainOutcome.StartCandle: return "The candle tips over and goes out. Phew.";
                case ChainOutcome.StartMagnet: return "Magnets don't roll. It just sits there. Magnetically.";
                case ChainOutcome.NoRamp: return "The ball refuses to jump.";
                case ChainOutcome.RampDomino: return "Dominoes can't climb. Or be climbed.";
                case ChainOutcome.RampCandle: return "A candle is not a ramp. The ball is offended.";
                case ChainOutcome.MagnetGrabsBall: return "The magnet loves the ball more than you do.";
                case ChainOutcome.BallRollsAway: return "Nothing to hit. Bye bye, ball.";
                case ChainOutcome.HitsBook: return "Bonk. The book is not impressed.";
                case ChainOutcome.HitsCandle: return "Strike! Wrong game.";
                case ChainOutcome.ToastedDominoes: return "Toasted dominoes.";
                case ChainOutcome.NoCandle: return "The last domino falls on... nothing.";
                case ChainOutcome.DominoOnMagnet: return "Clack! The magnet keeps the domino. The bell keeps quiet.";
                case ChainOutcome.DominoOnBook: return "The domino falls on the book. Great read, no ring.";
                case ChainOutcome.DominoOnBall: return "Nudge. The ball sighs.";
                default: return "Hmm.";
            }
        }
    }
}
