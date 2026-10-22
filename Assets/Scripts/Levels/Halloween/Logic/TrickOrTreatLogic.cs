namespace Gotcha.Levels.Halloween
{
    public enum TrickState
    {
        Building,       // si monta il travestimento (o si prepara lo scherzo di Toni)
        LadyAtDoor,     // la signora è alla porta (dà il cioccolatino o rifiuta): input bloccato
        NormalEnding,   // il mostro unico la spaventa: caramelle per tutti
        EggEnding,      // Easter egg: Toni col braccio in bocca
        Done,
    }

    /// <summary>Dove si trova il braccio dello zombie.</summary>
    public enum ArmPlace { OnZombie, Loose, InToniMouth }

    public enum TrickRingResult { Ignored, GaveChocolate, AlreadySeen, Scared, Egg }

    /// <summary>
    /// Logica pura del livello 250 "Trick or treat!". Percorso normale (dipendenze, non un ordine rigido): lo zombie fa
    /// da corpo davanti alla porta; Toni si nasconde sotto i suoi vestiti e la zucca prende il posto della testa (in
    /// qualsiasi ordine); il fantasma avvolge tutti; la strega completa con cappello e incantesimo; poi il campanello.
    /// Easter egg (mai nei suggerimenti): staccare il braccio dello zombie, darlo a Toni, portare Toni accanto alla
    /// porta e suonare il campanello con la mano dello zombie. L'egg è possibile solo finché Toni non è nel costume e il
    /// fantasma non ha avvolto nessuno; con il braccio staccato il costume non si può completare (lo si riattacca
    /// rilasciandolo sullo zombie). I due finali si escludono: una sola conclusione.
    /// </summary>
    public sealed class TrickOrTreatLogic
    {
        public TrickState State { get; private set; } = TrickState.Building;
        public bool ZombieAtDoor { get; private set; }
        public bool ToniInside { get; private set; }
        public bool PumpkinHead { get; private set; }
        public bool GhostWrapped { get; private set; }
        public bool WitchDone { get; private set; }
        public bool ToniAtDoor { get; private set; }
        public ArmPlace Arm { get; private set; } = ArmPlace.OnZombie;
        public int Chocolates { get; private set; }
        public bool ChangedSinceLastRing { get; private set; }
        public bool CompletionReported { get; private set; }
        public bool EndedWithEgg { get; private set; }

        public bool DisguiseComplete => WitchDone;
        /// <summary>Lo scherzo di Toni è ancora possibile (il costume non è montato in modo incompatibile).</summary>
        public bool EggAvailable => State == TrickState.Building && !ToniInside && !GhostWrapped && !WitchDone;
        public bool InputLocked => State != TrickState.Building;
        public int DisguiseSteps => (ZombieAtDoor ? 1 : 0) + (ToniInside ? 1 : 0) + (PumpkinHead ? 1 : 0) + (GhostWrapped ? 1 : 0) + (WitchDone ? 1 : 0);

        public void Reset()
        {
            State = TrickState.Building;
            ZombieAtDoor = ToniInside = PumpkinHead = GhostWrapped = WitchDone = ToniAtDoor = false;
            Arm = ArmPlace.OnZombie;
            Chocolates = 0;
            ChangedSinceLastRing = false;
            CompletionReported = false;
            EndedWithEgg = false;
        }

        bool Changed() { ChangedSinceLastRing = true; return true; }

        // ---------- travestimento ----------

        public bool TryZombieToDoor()
        {
            if (InputLocked || ZombieAtDoor) return false;
            ZombieAtDoor = true;
            return Changed();
        }

        public bool TryHideToni()
        {
            if (InputLocked || !ZombieAtDoor || ToniInside || Arm != ArmPlace.OnZombie) return false;
            ToniInside = true;
            ToniAtDoor = false;
            return Changed();
        }

        public bool TryPumpkinHead()
        {
            if (InputLocked || !ZombieAtDoor || PumpkinHead) return false;
            PumpkinHead = true;
            return Changed();
        }

        public bool TryGhostWrap()
        {
            if (InputLocked || GhostWrapped || !ZombieAtDoor || !ToniInside || !PumpkinHead || Arm != ArmPlace.OnZombie) return false;
            GhostWrapped = true;
            return Changed();
        }

        public bool TryWitchSpell()
        {
            if (InputLocked || !GhostWrapped || WitchDone) return false;
            WitchDone = true;
            return Changed();
        }

        // ---------- braccio e Toni (Easter egg) ----------

        /// <summary>Si può afferrare il braccio? (attaccato: solo se l'egg è ancora possibile; staccato: sempre)</summary>
        public bool CanGrabArm => !InputLocked && (Arm != ArmPlace.OnZombie || EggAvailable);

        /// <summary>Il braccio viene staccato e lasciato a terra.</summary>
        public bool TryDetachArm()
        {
            if (!CanGrabArm || Arm != ArmPlace.OnZombie) return false;
            Arm = ArmPlace.Loose;
            return Changed();
        }

        /// <summary>Il braccio rilasciato sullo zombie si riattacca (il costume torna completabile).</summary>
        public bool TryAttachArm()
        {
            if (InputLocked || Arm == ArmPlace.OnZombie) return false;
            Arm = ArmPlace.OnZombie;
            return Changed();
        }

        public bool TryGiveArmToToni()
        {
            if (InputLocked || Arm != ArmPlace.Loose || ToniInside) return false;
            Arm = ArmPlace.InToniMouth;
            return Changed();
        }

        /// <summary>Toni portato accanto alla porta (true) o altrove (false).</summary>
        public bool TrySetToniAtDoor(bool atDoor)
        {
            if (InputLocked || ToniInside || ToniAtDoor == atDoor) return false;
            ToniAtDoor = atDoor;
            return Changed();
        }

        // ---------- campanello ----------

        /// <summary>Tocco normale sul campanello.</summary>
        public TrickRingResult TryRingBell()
        {
            if (InputLocked) return TrickRingResult.Ignored;
            if (DisguiseComplete)
            {
                State = TrickState.NormalEnding;
                return TrickRingResult.Scared;
            }
            State = TrickState.LadyAtDoor;
            ChangedSinceLastRing = false;
            if (Chocolates == 0)
            {
                Chocolates = 1;
                return TrickRingResult.GaveChocolate;
            }
            return TrickRingResult.AlreadySeen;
        }

        /// <summary>
        /// Campanello suonato con la mano dello zombie (il braccio trascinato sul campanello). Easter egg se Toni ha il
        /// braccio in bocca, è accanto alla porta e l'egg è ancora possibile; altrimenti vale come un tocco normale.
        /// </summary>
        public TrickRingResult TryRingWithArm()
        {
            if (InputLocked || Arm == ArmPlace.OnZombie) return TrickRingResult.Ignored;
            if (Arm == ArmPlace.InToniMouth && ToniAtDoor && EggAvailable)
            {
                State = TrickState.EggEnding;
                return TrickRingResult.Egg;
            }
            return TryRingBell();
        }

        /// <summary>La signora richiude la porta: si torna a costruire.</summary>
        public bool TryLadyLeaves()
        {
            if (State != TrickState.LadyAtDoor) return false;
            State = TrickState.Building;
            return true;
        }

        /// <summary>Fine dell'animazione finale: true una sola volta (normale o egg, mai entrambi).</summary>
        public bool TryCompleteEnding()
        {
            if (CompletionReported || (State != TrickState.NormalEnding && State != TrickState.EggEnding)) return false;
            EndedWithEgg = State == TrickState.EggEnding;
            State = TrickState.Done;
            CompletionReported = true;
            return true;
        }
    }
}
