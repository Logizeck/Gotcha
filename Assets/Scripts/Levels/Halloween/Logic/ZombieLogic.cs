namespace Gotcha.Levels.Halloween
{
    public enum ZombieFood { Brain, Burger, Broccoli }

    public enum ZombieFeedResult { Ignored, Refused, Eaten }

    public enum ZombieState
    {
        Hungry,          // si possono togliere lettere, dare cibo, trascinare il fumetto (se dice BRAIN)
        Reacting,        // reazione a un cibo in corso
        HoldingBalloon,  // il fumetto BRAIN è in mano al giocatore
        Devouring,       // finale: il broccolo "cervello" viene divorato
        Done,
    }

    /// <summary>
    /// Logica pura del livello 249 "The vegetarian zombie": cervello e hamburger rifiutati, il broccolo mangiato ma non
    /// basta. Soluzione: togliere lettere da BRAAAINS fino a BRAIN e trascinare il fumetto su un broccolo.
    /// I broccoli non finiscono mai (almeno uno resta sempre disponibile).
    /// </summary>
    public sealed class ZombieLogic
    {
        public const int StartBroccoli = 3;

        public ZombieWord Word { get; } = new ZombieWord();
        public ZombieState State { get; private set; } = ZombieState.Hungry;
        public int Broccoli { get; private set; } = StartBroccoli;
        public int BroccoliEaten { get; private set; }
        public bool CompletionReported { get; private set; }

        public bool BalloonReady => State == ZombieState.Hungry && Word.IsTarget;
        public bool InputLocked => State == ZombieState.Devouring || State == ZombieState.Done;

        public void Reset()
        {
            Word.Reset();
            State = ZombieState.Hungry;
            Broccoli = StartBroccoli;
            BroccoliEaten = 0;
            CompletionReported = false;
        }

        /// <summary>Tocco su una lettera: la toglie (solo se la parola non è già BRAIN: allora il fumetto si trascina).</summary>
        public bool TryTapLetter(int index)
        {
            if (State != ZombieState.Hungry || Word.IsTarget) return false;
            return Word.TryRemove(index);
        }

        /// <summary>Annulla l'ultima lettera tolta. Restituisce l'indice rimesso o -1.</summary>
        public int TryUndo()
        {
            if (State != ZombieState.Hungry) return -1;
            return Word.Undo();
        }

        public ZombieFeedResult TryFeed(ZombieFood food)
        {
            if (State != ZombieState.Hungry) return ZombieFeedResult.Ignored;
            if (food == ZombieFood.Broccoli && Broccoli <= 0) return ZombieFeedResult.Ignored;
            State = ZombieState.Reacting;
            if (food != ZombieFood.Broccoli) return ZombieFeedResult.Refused;
            Broccoli--;
            BroccoliEaten++;
            if (Broccoli <= 0) Broccoli = 1;   // ricresce: ne resta sempre uno per la soluzione
            return ZombieFeedResult.Eaten;
        }

        public bool TryEndReaction()
        {
            if (State != ZombieState.Reacting) return false;
            State = ZombieState.Hungry;
            return true;
        }

        public bool TryPickBalloon()
        {
            if (!BalloonReady) return false;
            State = ZombieState.HoldingBalloon;
            return true;
        }

        /// <summary>True se il fumetto BRAIN è finito su un broccolo (parte il finale); altrimenti torna al suo posto.</summary>
        public bool TryDropBalloon(bool onBroccoli)
        {
            if (State != ZombieState.HoldingBalloon) return false;
            State = onBroccoli && Broccoli > 0 ? ZombieState.Devouring : ZombieState.Hungry;
            return State == ZombieState.Devouring;
        }

        public bool TryCompleteEnding()
        {
            if (State != ZombieState.Devouring || CompletionReported) return false;
            State = ZombieState.Done;
            CompletionReported = true;
            return true;
        }
    }
}
