namespace Gotcha.Levels.Halloween
{
    public enum GhostState
    {
        Covered,        // il fantasma è sotto il lenzuolo (non fa paura)
        SheetLoose,     // lenzuolo tolto e posato: il fantasma è invisibile
        HoldingSheet,   // il giocatore trascina il lenzuolo
        Ending,         // lenzuolo sull'uomo: si guarda allo specchio e si spaventa
        Done,
    }

    /// <summary>Dove viene lasciato il lenzuolo.</summary>
    public enum GhostDropTarget { Nowhere, Man, Ghost }

    /// <summary>
    /// Logica pura del livello 247 "The ghost who can't scare": il "BOO!" non funziona, sveglia e lampada non
    /// bastano. Soluzione: togliere il lenzuolo al fantasma (resta invisibile) e metterlo sull'uomo, che allo specchio
    /// si vede fantasma e si spaventa. Sveglia e lampada possono essere in qualsiasi stato.
    /// </summary>
    public sealed class GhostLogic
    {
        public GhostState State { get; private set; } = GhostState.Covered;
        public bool ManAwake { get; private set; }
        public bool LampOn { get; private set; } = true;
        public int Boos { get; private set; }
        public bool CompletionReported { get; private set; }

        public bool GhostInvisible => State != GhostState.Covered;
        public bool InputLocked => State == GhostState.Ending || State == GhostState.Done;

        public void Reset()
        {
            State = GhostState.Covered;
            ManAwake = false;
            LampOn = true;
            Boos = 0;
            CompletionReported = false;
        }

        /// <summary>Tocco sul fantasma: "BOO!" (solo col lenzuolo addosso; non spaventa nessuno).</summary>
        public bool TryBoo()
        {
            if (InputLocked || State != GhostState.Covered) return false;
            Boos++;
            return true;
        }

        public bool TryRingAlarm()
        {
            if (InputLocked) return false;
            ManAwake = true;
            return true;
        }

        /// <summary>L'uomo si riaddormenta (dopo un po' che è sveglio). Non durante il finale.</summary>
        public bool TryManSleeps()
        {
            if (InputLocked || !ManAwake) return false;
            ManAwake = false;
            return true;
        }

        public bool TryToggleLamp()
        {
            if (InputLocked) return false;
            LampOn = !LampOn;
            return true;
        }

        public bool TryPickSheet()
        {
            if (State != GhostState.Covered && State != GhostState.SheetLoose) return false;
            State = GhostState.HoldingSheet;
            return true;
        }

        /// <summary>True se il lenzuolo è finito sull'uomo (parte il finale).</summary>
        public bool TryDropSheet(GhostDropTarget target)
        {
            if (State != GhostState.HoldingSheet) return false;
            switch (target)
            {
                case GhostDropTarget.Man: State = GhostState.Ending; return true;
                case GhostDropTarget.Ghost: State = GhostState.Covered; return false;
                default: State = GhostState.SheetLoose; return false;
            }
        }

        public bool TryCompleteEnding()
        {
            if (State != GhostState.Ending || CompletionReported) return false;
            State = GhostState.Done;
            CompletionReported = true;
            return true;
        }
    }
}
