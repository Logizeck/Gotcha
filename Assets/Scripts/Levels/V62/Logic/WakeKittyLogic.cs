namespace Gotcha.Levels.V62
{
    /// <summary>Stati del livello 252 (svegliare il gatto per la cena).</summary>
    public enum WakeKittyState
    {
        Sleeping,       // il gatto dorme, il pesce è sulla mensola
        HoldingFish,    // il giocatore trascina il pesce
        MouseStealing,  // il topo ha rubato il pesce (ricompare sulla mensola dopo un po')
        Waking,         // il gatto annusa il pesce e si sveglia (input bloccato)
        Done,
    }

    /// <summary>Cosa fa il gatto (o il topo) dopo un'azione del giocatore.</summary>
    public enum KittyReaction
    {
        None,
        SlapsAlarm,     // zampata alla sveglia, russa più forte
        HidesFromLight, // lampada accesa: si copre gli occhi con la coda e si gira
        Stretches,      // la piuma: si stiracchia e continua a dormire
        IgnoresCheese,  // il formaggio al gatto: "sono un gatto, non un topo"
        MouseSteals,    // il topo sfreccia fuori e ruba il pesce
        FishBack,       // il pesce lasciato altrove torna sulla mensola
        MouseTakesCheese,
        Wakes,
    }

    /// <summary>
    /// Logica pura del livello 252 "Wake up, Kitty!": la sveglia la prende a zampate, la lampada gli fa solo coprire
    /// gli occhi, la piuma lo fa stiracchiare e il pesce, se il topo è libero, se lo ruba il topo ("Finders keepers!").
    /// Soluzione: il formaggio davanti alla tana (il topo lo porta dentro e resta occupato a mangiarlo), poi il pesce
    /// sotto il naso del gatto. Nessuna dipendenza da Unity.
    /// </summary>
    public sealed class WakeKittyLogic
    {
        public WakeKittyState State { get; private set; } = WakeKittyState.Sleeping;
        /// <summary>Il topo è nella tana a mangiare il formaggio (non esce più).</summary>
        public bool MouseBusy { get; private set; }
        public bool LampOn { get; private set; }
        /// <summary>Lato verso cui dorme il gatto (cambia ogni volta che si gira).</summary>
        public bool FacingLeft { get; private set; } = true;
        public int Snore { get; private set; } = 1;
        public int Steals { get; private set; }
        public bool CompletionReported { get; private set; }

        public bool InputLocked => State == WakeKittyState.Waking || State == WakeKittyState.Done;

        public void Reset()
        {
            State = WakeKittyState.Sleeping;
            MouseBusy = false;
            LampOn = false;
            FacingLeft = true;
            Snore = 1;
            Steals = 0;
            CompletionReported = false;
        }

        /// <summary>La sveglia suona: il gatto la scaccia con una zampata e russa più forte.</summary>
        public KittyReaction TapAlarm()
        {
            if (InputLocked) return KittyReaction.None;
            if (Snore < 3) Snore++;
            return KittyReaction.SlapsAlarm;
        }

        /// <summary>La lampada si accende/spegne: accesa, il gatto si copre gli occhi con la coda e si gira.</summary>
        public KittyReaction TapLamp()
        {
            if (InputLocked) return KittyReaction.None;
            LampOn = !LampOn;
            if (!LampOn) return KittyReaction.None;
            FacingLeft = !FacingLeft;
            return KittyReaction.HidesFromLight;
        }

        /// <summary>La piuma sul gatto: falsa pista, si stiracchia e dorme ancora.</summary>
        public KittyReaction FeatherOnCat() => InputLocked ? KittyReaction.None : KittyReaction.Stretches;

        /// <summary>Il formaggio portato al gatto: non gli interessa.</summary>
        public KittyReaction CheeseOnCat() => InputLocked ? KittyReaction.None : KittyReaction.IgnoresCheese;

        /// <summary>
        /// Il formaggio lasciato davanti alla tana: il topo se lo porta dentro e da quel momento è occupato a
        /// mangiarlo. Non mentre il giocatore tiene il pesce né quando il topo l'ha già.
        /// </summary>
        public KittyReaction TryGiveCheese()
        {
            if (InputLocked || MouseBusy || State == WakeKittyState.HoldingFish) return KittyReaction.None;
            MouseBusy = true;
            return KittyReaction.MouseTakesCheese;
        }

        public bool TryPickFish()
        {
            if (State != WakeKittyState.Sleeping) return false;
            State = WakeKittyState.HoldingFish;
            return true;
        }

        /// <summary>
        /// Il pesce arriva sotto il naso del gatto: col topo libero lo ruba il topo, col topo occupato il gatto lo
        /// annusa e si sveglia.
        /// </summary>
        public KittyReaction FishAtCat()
        {
            if (State != WakeKittyState.HoldingFish) return KittyReaction.None;
            if (!MouseBusy)
            {
                Steals++;
                State = WakeKittyState.MouseStealing;
                return KittyReaction.MouseSteals;
            }
            State = WakeKittyState.Waking;
            return KittyReaction.Wakes;
        }

        /// <summary>Il pesce lasciato altrove torna sulla mensola.</summary>
        public KittyReaction DropFishElsewhere()
        {
            if (State != WakeKittyState.HoldingFish) return KittyReaction.None;
            State = WakeKittyState.Sleeping;
            return KittyReaction.FishBack;
        }

        /// <summary>Il pesce rubato ricompare sulla mensola.</summary>
        public bool TryRespawnFish()
        {
            if (State != WakeKittyState.MouseStealing) return false;
            State = WakeKittyState.Sleeping;
            return true;
        }

        public bool TryCompleteWake()
        {
            if (State != WakeKittyState.Waking || CompletionReported) return false;
            State = WakeKittyState.Done;
            CompletionReported = true;
            return true;
        }
    }
}
