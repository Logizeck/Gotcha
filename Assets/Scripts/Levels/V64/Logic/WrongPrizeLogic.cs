namespace Gotcha.Levels.V64
{
    /// <summary>Fasi del livello "The Wrong Prize".</summary>
    public enum WrongPrizeStage
    {
        Playing,      // il podio dice 2 - 1 - 3: il corridore è sul gradino del 3
        Celebrating,  // il numero 1 è sul gradino del corridore: festa in corso (input bloccato)
        Done,         // vittoria già data
    }

    /// <summary>Cosa tocca il dito che scende.</summary>
    public enum WrongPrizePart
    {
        Runner,   // il corridore "YOU"
        Trophy,   // la coppa (avvitata al gradino)
        Digit,    // il numero sul davanti di un gradino
    }

    /// <summary>Esito di un dito che scende su una parte del podio.</summary>
    public enum WrongPrizeGrab
    {
        None,     // input bloccato, parte non valida o un altro dito sta già trascinando
        Runner,   // il corridore si stacca dal gradino e segue il dito
        Digit,    // il numero si stacca dal gradino e segue il dito
        Bolted,   // la coppa è avvitata: oscilla e basta ("It's screwed on.")
    }

    /// <summary>Esito del dito che lascia ciò che trascinava.</summary>
    public enum WrongPrizeDrop
    {
        None,            // il dito non trascinava niente (o input bloccato)
        RunnerTap,       // corridore toccato senza trascinarlo: saltella sul suo gradino
        RunnerSentBack,  // corridore lasciato sul gradino del primo posto: fischio, torna al suo gradino
        RunnerReturned,  // corridore lasciato altrove: torna al suo gradino
        DigitTap,        // numero toccato senza trascinarlo: oscilla, resta al suo posto
        DigitReturned,   // numero lasciato nel vuoto (o sul suo gradino): torna al suo posto
        NotYourStep,     // numero lasciato su un altro gradino che non è quello del corridore: torna al suo posto
        SecondPlace,     // scambio: ora il gradino del corridore dice 2 ("Second place. Still not first.")
        ThirdAgain,      // scambio: il gradino del corridore torna a dire 3
        FirstPlace,      // scambio: il gradino del corridore dice 1: VITTORIA (una volta sola)
    }

    /// <summary>
    /// Logica pura del livello 47 della roadmap "The Wrong Prize" (id wrong_prize) — "WIN FIRST PRIZE." Un podio a tre
    /// gradini: a sinistra il 2 (con un rivale tronfio), al centro il più alto con l'1 (e la coppa avvitata), a destra il
    /// 3 dove sta il corridore del giocatore ("YOU").
    /// Inganno: il corridore portato sul primo gradino torna indietro (fischio dell'arbitro: "Back to your place!"); la
    /// coppa non si stacca. Soluzione esatta: trascinare il NUMERO 1 sul gradino del corridore: i numeri si scambiano
    /// (l'1 va sul gradino del corridore, il suo numero va sul gradino da cui veniva l'1) e il corridore vince. Conta il
    /// NUMERO lasciato sul gradino, non la posizione del corridore. Un numero lasciato sul gradino del corridore si
    /// scambia sempre con il suo (2 -> "Second place. Still not first.", 3 -> di nuovo terzo): ogni scambio è reversibile
    /// e l'1 resta sempre trascinabile. Un numero lasciato altrove (nel vuoto, sul suo gradino, su un altro gradino)
    /// torna al suo posto.
    /// Gradini per posizione: 0 sinistra, 1 centro, 2 destra (<see cref="RunnerStep"/>, il corridore non si sposta mai).
    /// Un dito alla volta trascina. Cancel() (pausa, app in background, livello spento) rimette a posto ciò che era in
    /// mano senza contare niente. Non si perde mai. Nessuna dipendenza da Unity.
    /// </summary>
    public sealed class WrongPrizeLogic
    {
        /// <summary>Numero di gradini e posizioni.</summary>
        public const int StepCount = 3, LeftStep = 0, CentreStep = 1, RightStep = 2;
        /// <summary>Il gradino su cui sta il corridore (non cambia mai: cambiano i numeri).</summary>
        public const int RunnerStep = RightStep;
        /// <summary>"Nessun gradino sotto ciò che si lascia".</summary>
        public const int NoStep = -1;
        /// <summary>"Nessun dito".</summary>
        public const int NoFinger = int.MinValue;
        /// <summary>Spostamento del dito oltre il quale non è più un tocco.</summary>
        public const float TapSlop = 0.15f;

        /// <summary>I numeri iniziali per gradino (sinistra, centro, destra).</summary>
        public static int StartNumber(int step) => step == LeftStep ? 2 : step == CentreStep ? 1 : step == RightStep ? 3 : 0;

        readonly int[] numbers = new int[StepCount];
        int finger = NoFinger, fromStep = NoStep;
        WrongPrizeGrab held = WrongPrizeGrab.None;
        float sx, sy;
        bool moved, completed;

        public WrongPrizeStage Stage { get; private set; } = WrongPrizeStage.Playing;
        /// <summary>Input bloccato (festa, finito).</summary>
        public bool InputLocked => Stage != WrongPrizeStage.Playing;
        public bool Won => Stage != WrongPrizeStage.Playing;

        public bool Dragging => finger != NoFinger;
        public int DragFinger => finger;
        /// <summary>Cosa si trascina (Runner o Digit), None se niente.</summary>
        public WrongPrizeGrab Held => Dragging ? held : WrongPrizeGrab.None;
        /// <summary>Il gradino da cui viene il numero trascinato (NoStep se non si trascina un numero).</summary>
        public int DragStep => Held == WrongPrizeGrab.Digit ? fromStep : NoStep;
        /// <summary>Il numero trascinato (0 se non si trascina un numero).</summary>
        public int DragNumber => Held == WrongPrizeGrab.Digit ? numbers[fromStep] : 0;
        /// <summary>Il dito che trascina si è mosso oltre <see cref="TapSlop"/>.</summary>
        public bool DragMoved => Dragging && moved;

        /// <summary>Il posto del corridore (il numero sul suo gradino).</summary>
        public int RunnerPlace => numbers[RunnerStep];

        public int SentBack { get; private set; }
        public int RunnerReturns { get; private set; }
        public int RunnerTaps { get; private set; }
        public int TrophyTries { get; private set; }
        public int DigitTaps { get; private set; }
        public int DigitReturns { get; private set; }
        public int WrongSteps { get; private set; }
        public int Swaps { get; private set; }
        public int WrongMoves => SentBack + RunnerReturns + TrophyTries + DigitReturns + WrongSteps;

        public WrongPrizeLogic() { Reset(); }

        public void Reset()
        {
            Stage = WrongPrizeStage.Playing;
            for (int s = 0; s < StepCount; s++) numbers[s] = StartNumber(s);
            finger = NoFinger;
            fromStep = NoStep;
            held = WrongPrizeGrab.None;
            sx = sy = 0f;
            moved = completed = false;
            SentBack = RunnerReturns = RunnerTaps = TrophyTries = DigitTaps = DigitReturns = WrongSteps = Swaps = 0;
        }

        /// <summary>Il numero scritto sul gradino <paramref name="step"/> (0 se il gradino non esiste).</summary>
        public int NumberOn(int step) => step >= 0 && step < StepCount ? numbers[step] : 0;

        /// <summary>Il gradino che mostra il numero <paramref name="number"/> (NoStep se nessuno).</summary>
        public int StepOf(int number)
        {
            for (int s = 0; s < StepCount; s++) if (numbers[s] == number) return s;
            return NoStep;
        }

        /// <summary>Il gradino del primo posto in questo momento (dove sta il numero 1).</summary>
        public int FirstStep => StepOf(1);

        // ---------- Dita ----------

        /// <summary>
        /// Il dito <paramref name="id"/> scende in (x, y) su <paramref name="part"/>; per un numero, <paramref name="step"/> è
        /// il gradino su cui il numero è scritto.
        /// </summary>
        public WrongPrizeGrab Grab(int id, WrongPrizePart part, int step, float x, float y)
        {
            if (InputLocked || id == NoFinger || Dragging || Bad(x) || Bad(y)) return WrongPrizeGrab.None;
            switch (part)
            {
                case WrongPrizePart.Trophy:
                    TrophyTries++;
                    return WrongPrizeGrab.Bolted;
                case WrongPrizePart.Runner:
                    Hold(id, WrongPrizeGrab.Runner, NoStep, x, y);
                    return WrongPrizeGrab.Runner;
                case WrongPrizePart.Digit:
                    if (step < 0 || step >= StepCount) return WrongPrizeGrab.None;
                    Hold(id, WrongPrizeGrab.Digit, step, x, y);
                    return WrongPrizeGrab.Digit;
            }
            return WrongPrizeGrab.None;
        }

        void Hold(int id, WrongPrizeGrab what, int step, float x, float y)
        {
            finger = id;
            held = what;
            fromStep = step;
            sx = x;
            sy = y;
            moved = false;
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
        /// Il dito lascia ciò che trascinava sopra il gradino <paramref name="stepUnder"/> (<see cref="NoStep"/> se non è
        /// sopra nessun gradino). Conta il NUMERO lasciato sul gradino del corridore, mai la posizione del corridore. Un dito
        /// che si alza senza essersi mosso è sempre un tocco (non sposta niente).
        /// </summary>
        public WrongPrizeDrop Drop(int id, int stepUnder)
        {
            if (id == NoFinger || id != finger) return WrongPrizeDrop.None;
            bool wasMoved = moved;
            WrongPrizeGrab what = held;
            int from = fromStep;
            finger = NoFinger;
            held = WrongPrizeGrab.None;
            fromStep = NoStep;
            moved = false;
            if (InputLocked) return WrongPrizeDrop.None;
            if (stepUnder < 0 || stepUnder >= StepCount) stepUnder = NoStep;

            if (what == WrongPrizeGrab.Runner)
            {
                if (!wasMoved) { RunnerTaps++; return WrongPrizeDrop.RunnerTap; }
                if (stepUnder != NoStep && stepUnder == FirstStep) { SentBack++; return WrongPrizeDrop.RunnerSentBack; }
                RunnerReturns++;
                return WrongPrizeDrop.RunnerReturned;
            }
            if (what != WrongPrizeGrab.Digit || from == NoStep) return WrongPrizeDrop.None;

            if (!wasMoved) { DigitTaps++; return WrongPrizeDrop.DigitTap; }
            if (stepUnder == NoStep || stepUnder == from) { DigitReturns++; return WrongPrizeDrop.DigitReturned; }
            if (stepUnder != RunnerStep) { WrongSteps++; return WrongPrizeDrop.NotYourStep; }

            // il numero lasciato sul gradino del corridore: i due numeri si scambiano
            int n = numbers[from];
            numbers[from] = numbers[RunnerStep];
            numbers[RunnerStep] = n;
            Swaps++;
            if (n == 1)
            {
                Stage = WrongPrizeStage.Celebrating;
                return WrongPrizeDrop.FirstPlace;
            }
            return n == 2 ? WrongPrizeDrop.SecondPlace : WrongPrizeDrop.ThirdAgain;
        }

        /// <summary>Dita perse (pausa, app in background, livello spento): ciò che era in mano torna al suo posto, niente conta.</summary>
        public bool Cancel()
        {
            bool any = Dragging;
            finger = NoFinger;
            held = WrongPrizeGrab.None;
            fromStep = NoStep;
            moved = false;
            return any;
        }

        /// <summary>Fine della festa: true una volta sola (allora il livello chiama Win).</summary>
        public bool TryComplete()
        {
            if (Stage != WrongPrizeStage.Celebrating || completed) return false;
            completed = true;
            Stage = WrongPrizeStage.Done;
            return true;
        }

        static bool Bad(float v) => float.IsNaN(v) || float.IsInfinity(v);
    }
}
