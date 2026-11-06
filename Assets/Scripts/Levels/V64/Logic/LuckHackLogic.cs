namespace Gotcha.Levels.V64
{
    /// <summary>Stato del livello "Luck Hack" (roadmap n. 54).</summary>
    public enum LuckHackState
    {
        Idle,       // moneta ferma sul tavolo, TAILS in su: si può lanciare (o toccare: gira su se stessa)
        Spinning,   // toccata da ferma: gira sul tavolo per SpinTime e resta TAILS (FLIP resta permesso)
        Flying,     // lanciata: in volo per FlightTime (finestra del capovolgimento aperta)
        Settling,   // atterrata su TAILS ma ancora traballante: GraceTime di tolleranza (finestra ancora aperta)
        Falling,    // toccata in volo: cade dritta sul tavolo (TAILS) per DropTime, FLIP ignorato
        Won,        // atterrata (o rigirata) su HEADS: festa, input chiuso
    }

    /// <summary>Cosa è successo dopo un input o un passo di tempo.</summary>
    public enum LuckHackEvent
    {
        None,          // niente da mostrare
        Ignored,       // input che non conta (FLIP in volo, tocco mentre gira o cade, tutto dopo la vittoria)
        Flipped,       // FLIP: la moneta parte (volo deterministico)
        NeedUpright,   // FLIP con il telefono già capovolto (o quasi): "Hold the phone normally to flip." (non parte)
        Hacked,        // capovolgimento nuovo durante la finestra: la moneta "sente" il mondo girarsi (atterrerà HEADS)
        TouchedDown,   // atterrata su TAILS (inizia la tolleranza: traballa)
        Tails,         // fine della tolleranza: TAILS definitivo (battute "TAILS. Again." ecc.)
        Touched,       // toccata in volo o mentre traballa: "You touched it. TAILS." (cade subito)
        Fell,          // fine della caduta dopo il tocco: di nuovo ferma
        Spun,          // toccata da ferma: gira sul tavolo
        SpinStopped,   // fine della giravolta: TAILS, di nuovo ferma
        LandedHeads,   // VITTORIA: capovolta durante il volo, atterra su HEADS
        FlippedOver,   // VITTORIA: capovolta durante la tolleranza, la moneta si rigira su HEADS
    }

    /// <summary>
    /// Logica pura del livello "Luck Hack" (id luck_hack, roadmap n. 54) — "FLIP HEADS." Una moneta sul tavolo e il
    /// pulsante FLIP. Lanciata normalmente atterra SEMPRE su TAILS; toccarla in volo la fa cadere subito su TAILS; toccata
    /// da ferma gira sul tavolo e resta TAILS. Tutto reversibile, mai una sconfitta.
    /// Soluzione esatta: premere FLIP e, DURANTE la finestra (tutto il volo <see cref="FlightTime"/> più la tolleranza
    /// <see cref="GraceTime"/>), capovolgere davvero il telefono: la moneta atterra (o si rigira) su HEADS.
    /// Gravità: accelerometro nello schermo (gioco bloccato in verticale: dritto y ≈ -1, capovolto y ≈ +1), filtrata
    /// passa-basso con <see cref="V64AGravity"/> e con le STESSE soglie e la stessa isteresi di
    /// <see cref="AristocraticCatLogic"/> (capovolto sopra <see cref="InvertOn"/>, smette sotto <see cref="InvertOff"/>).
    /// Il capovolgimento deve cominciare dopo il FLIP: se al momento del FLIP la y filtrata non è sotto
    /// <see cref="ArmBelow"/> (telefono capovolto o quasi) la moneta non parte ("Hold the phone normally to flip."), così
    /// un capovolgimento "vecchio" non vale mai; ogni capovolgimento che conta è una transizione nuova dentro la finestra.
    /// Dopo la finestra (moneta ferma) capovolgere non fa niente. Cancel() (pausa, perdita del fuoco) rimette la moneta
    /// ferma su TAILS senza contare niente. Nessuna dipendenza da Unity.
    /// </summary>
    public sealed class LuckHackLogic
    {
        public const float InvertOn = AristocraticCatLogic.InvertOn;     // stesse soglie del gatto aristocratico
        public const float InvertOff = AristocraticCatLogic.InvertOff;
        public const float ArmBelow = AristocraticCatLogic.ArmBelow;     // al FLIP la y filtrata deve essere sotto questa
        public const float Smoothing = AristocraticCatLogic.Smoothing;
        public const float FlightTime = 2.0f;   // volo: sale, gira, ricade (deterministico, niente fisica)
        public const float GraceTime = 0.3f;    // tolleranza dopo l'atterraggio (la moneta traballa)
        public const float SpinTime = 0.9f;     // giravolta sul tavolo dopo un tocco da ferma
        public const float DropTime = 0.3f;     // caduta dopo il tocco in volo
        const float TimeEps = 1e-4f;

        readonly V64AGravity gravity = new V64AGravity();
        float t;
        bool inverted, completed;

        public LuckHackLogic()
        {
            gravity.Smoothing = Smoothing;
            Reset();
        }

        public LuckHackState State { get; private set; }
        /// <summary>Secondi dall'inizio dello stato corrente (nel volo e nella tolleranza: dal FLIP).</summary>
        public float StateTime => t;
        /// <summary>Avanzamento del volo 0..1 (1 dall'atterraggio in poi; 0 da ferma).</summary>
        public float Flight01 => State == LuckHackState.Flying ? Clamp01(t / FlightTime) : State == LuckHackState.Settling ? 1f : 0f;
        /// <summary>Capovolta dentro la finestra: atterrerà su HEADS (si azzera a ogni FLIP, a ogni tocco e con Cancel).</summary>
        public bool Hacked { get; private set; }
        /// <summary>La finestra del capovolgimento è aperta (volo o tolleranza).</summary>
        public bool WindowOpen => State == LuckHackState.Flying || State == LuckHackState.Settling;
        /// <summary>Capovolto (con isteresi), in qualsiasi stato.</summary>
        public bool Inverted => inverted;
        /// <summary>Componente y della gravità filtrata (-1 dritto, +1 capovolto).</summary>
        public float GravityY => gravity.Y;
        public float GravityX => gravity.X;
        /// <summary>Faccia in su della moneta ferma o atterrata (true = HEADS: solo dopo la vittoria).</summary>
        public bool HeadsUp => State == LuckHackState.Won;
        /// <summary>Lanci finiti su TAILS normalmente (per variare le battute).</summary>
        public int Flips { get; private set; }
        public int Touches { get; private set; }
        public int Spins { get; private set; }
        public int Refusals { get; private set; }
        /// <summary>Tentativi sbagliati (lanci su TAILS, tocchi, giravolte, FLIP col telefono capovolto).</summary>
        public int WrongMoves { get; private set; }
        public bool InputLocked => State == LuckHackState.Won;

        /// <summary>Si ricomincia da capo (rientro nel livello): moneta ferma su TAILS, contatori a zero.</summary>
        public void Reset()
        {
            State = LuckHackState.Idle;
            gravity.Reset();
            t = 0f;
            inverted = completed = false;
            Hacked = false;
            Flips = Touches = Spins = Refusals = WrongMoves = 0;
        }

        /// <summary>Pausa, app in sottofondo, perdita del fuoco: la moneta torna ferma su TAILS, il filtro riparte.</summary>
        public void Cancel()
        {
            if (State == LuckHackState.Won) return;
            State = LuckHackState.Idle;
            t = 0f;
            Hacked = false;
            inverted = false;
            gravity.Reset();   // la prossima lettura riempie subito il filtro (niente salti "finti")
        }

        /// <summary>Pulsante FLIP.</summary>
        public LuckHackEvent PressFlip()
        {
            if (State != LuckHackState.Idle && State != LuckHackState.Spinning) return LuckHackEvent.Ignored;
            if (gravity.Y >= ArmBelow)
            {
                // telefono già capovolto (o quasi) prima del lancio: non vale, la moneta non parte
                Refusals++;
                WrongMoves++;
                return LuckHackEvent.NeedUpright;
            }
            State = LuckHackState.Flying;
            t = 0f;
            Hacked = false;
            inverted = false;   // y < ArmBelow < InvertOff: comunque non capovolto
            return LuckHackEvent.Flipped;
        }

        /// <summary>Tocco sulla moneta (ovunque sia).</summary>
        public LuckHackEvent TouchCoin()
        {
            switch (State)
            {
                case LuckHackState.Idle:
                    State = LuckHackState.Spinning;
                    t = 0f;
                    Spins++;
                    WrongMoves++;
                    return LuckHackEvent.Spun;
                case LuckHackState.Flying:
                case LuckHackState.Settling:
                    // toccata in volo (o mentre traballa): cade subito su TAILS, anche se era già "truccata"
                    State = LuckHackState.Falling;
                    t = 0f;
                    Hacked = false;
                    Touches++;
                    WrongMoves++;
                    return LuckHackEvent.Touched;
                default:
                    return LuckHackEvent.Ignored;   // gira già, sta cadendo, o ha già vinto
            }
        }

        /// <summary>Passo di tempo con la lettura dell'accelerometro (già nel sistema dello schermo, in g).</summary>
        public LuckHackEvent Tick(float dt, float ax, float ay)
        {
            if (State == LuckHackState.Won) return LuckHackEvent.None;
            if (dt < 0f || float.IsNaN(dt) || float.IsInfinity(dt)) dt = 0f;
            gravity.Feed(ax, ay, dt);
            float y = gravity.Y;
            bool wasInverted = inverted;
            if (!inverted && y > InvertOn) inverted = true;
            else if (inverted && y < InvertOff) inverted = false;
            bool newInversion = inverted && !wasInverted;
            t += dt;

            switch (State)
            {
                case LuckHackState.Spinning:
                    if (t >= SpinTime - TimeEps) { State = LuckHackState.Idle; t = 0f; return LuckHackEvent.SpinStopped; }
                    return LuckHackEvent.None;
                case LuckHackState.Falling:
                    if (t >= DropTime - TimeEps) { State = LuckHackState.Idle; t = 0f; return LuckHackEvent.Fell; }
                    return LuckHackEvent.None;
                case LuckHackState.Flying:
                    if (newInversion && !Hacked)
                    {
                        Hacked = true;
                        if (t < FlightTime - TimeEps) return LuckHackEvent.Hacked;
                    }
                    if (t >= FlightTime - TimeEps)
                    {
                        if (Hacked) { State = LuckHackState.Won; return LuckHackEvent.LandedHeads; }
                        State = LuckHackState.Settling;
                        return LuckHackEvent.TouchedDown;
                    }
                    return LuckHackEvent.None;
                case LuckHackState.Settling:
                    if (newInversion)
                    {
                        Hacked = true;
                        State = LuckHackState.Won;
                        return LuckHackEvent.FlippedOver;
                    }
                    if (t >= FlightTime + GraceTime - TimeEps)
                    {
                        State = LuckHackState.Idle;
                        t = 0f;
                        Flips++;
                        WrongMoves++;
                        return LuckHackEvent.Tails;
                    }
                    return LuckHackEvent.None;
                default:
                    return LuckHackEvent.None;   // Idle: capovolgere il telefono da ferma non fa niente
            }
        }

        /// <summary>Vittoria da registrare: true una volta sola, dopo HEADS.</summary>
        public bool TryComplete()
        {
            if (State != LuckHackState.Won || completed) return false;
            completed = true;
            return true;
        }

        static float Clamp01(float v) => v < 0f ? 0f : v > 1f ? 1f : v;
    }
}
