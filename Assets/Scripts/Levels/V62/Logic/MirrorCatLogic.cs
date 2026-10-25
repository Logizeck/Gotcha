namespace Gotcha.Levels.V62
{
    /// <summary>Stati del livello 254 (il gatto che spinge il vaso).</summary>
    public enum MirrorCatState
    {
        Pushing,    // il gatto spinge il vaso verso il bordo, una spinta alla volta
        Paused,     // si è fermato un attimo (toccato o sta mangiando), poi riprende
        Scared,     // ha visto il suo riflesso: si gonfia e scappa (input bloccato)
        Fallen,     // il vaso è caduto (input bloccato, sconfitta)
        Done,       // vaso salvo
    }

    /// <summary>Dove viene lasciato lo specchio.</summary>
    public enum MirrorTarget { Nowhere, InFrontOfCat }

    /// <summary>
    /// Logica pura del livello 254 "Mirror, mirror": il gatto spinge il vaso prezioso un po' ogni secondo (~25 s).
    /// Toccarlo lo ferma un attimo ma poi spinge più veloce; il pesce lo mangia e riprende; il campanello lo ignora;
    /// il vaso è troppo pesante. Soluzione: lo specchio sulla mensola davanti al gatto, che si spaventa del riflesso.
    /// Nessuna dipendenza da Unity.
    /// </summary>
    public sealed class MirrorCatLogic
    {
        /// <summary>Spinte che portano il vaso dal centro della mensola al bordo.</summary>
        public const int PushesToFall = 25;
        public const float BaseInterval = 1f;
        public const float MinInterval = 0.45f;
        /// <summary>Ogni tocco al gatto: pausa breve, poi spinge più veloce (l'intervallo si moltiplica per questo).</summary>
        public const float AngrySpeedUp = 0.8f;
        public const float TouchPause = 1.2f;
        public const float EatPause = 1.6f;

        public MirrorCatState State { get; private set; } = MirrorCatState.Pushing;
        public int Pushes { get; private set; }
        public float Interval { get; private set; } = BaseInterval;
        public int Touches { get; private set; }
        public int FishEaten { get; private set; }
        public bool CompletionReported { get; private set; }

        float timer, pause;

        public float Progress => (float)Pushes / PushesToFall;
        public bool InputLocked => State == MirrorCatState.Scared || State == MirrorCatState.Fallen || State == MirrorCatState.Done;
        public int PushesLeft => PushesToFall - Pushes;

        public void Reset()
        {
            State = MirrorCatState.Pushing;
            Pushes = 0;
            Interval = BaseInterval;
            Touches = 0;
            FishEaten = 0;
            CompletionReported = false;
            timer = pause = 0f;
        }

        /// <summary>
        /// Avanza il tempo: restituisce quante spinte ci sono state (0 o più). Quando il vaso arriva al bordo lo
        /// stato diventa Fallen.
        /// </summary>
        public int Tick(float dt)
        {
            if (dt <= 0f || InputLocked) return 0;
            if (State == MirrorCatState.Paused)
            {
                pause -= dt;
                if (pause > 0f) return 0;
                dt = -pause;
                pause = 0f;
                timer = 0f;
                State = MirrorCatState.Pushing;
            }
            timer += dt;
            int n = 0;
            while (timer >= Interval && State == MirrorCatState.Pushing)
            {
                timer -= Interval;
                Pushes++;
                n++;
                if (Pushes >= PushesToFall) { State = MirrorCatState.Fallen; break; }
            }
            return n;
        }

        /// <summary>Tocco sul gatto: "Don't touch me." — si ferma un attimo, poi spinge più veloce.</summary>
        public bool TryTouchCat()
        {
            if (InputLocked) return false;
            Touches++;
            Interval = System.Math.Max(MinInterval, Interval * AngrySpeedUp);
            Pause(TouchPause);
            return true;
        }

        /// <summary>Il pesce al gatto: un boccone, una pausa, e poi riprende a spingere.</summary>
        public bool TryFeedFish()
        {
            if (InputLocked) return false;
            FishEaten++;
            Pause(EatPause);
            return true;
        }

        /// <summary>Il campanello: il gatto non fa una piega (nessun effetto sul vaso).</summary>
        public bool TryRingBell() => !InputLocked;

        /// <summary>Il vaso non si sposta: troppo pesante.</summary>
        public bool TryDragVase() => !InputLocked;

        /// <summary>Lo specchio viene lasciato: davanti al gatto lo spaventa (true), altrove torna al suo posto.</summary>
        public bool TryDropMirror(MirrorTarget target)
        {
            if (InputLocked || target != MirrorTarget.InFrontOfCat) return false;
            State = MirrorCatState.Scared;
            return true;
        }

        public bool TryCompleteSave()
        {
            if (State != MirrorCatState.Scared || CompletionReported) return false;
            State = MirrorCatState.Done;
            CompletionReported = true;
            return true;
        }

        void Pause(float seconds)
        {
            State = MirrorCatState.Paused;
            pause = System.Math.Max(pause, seconds);
        }
    }
}
