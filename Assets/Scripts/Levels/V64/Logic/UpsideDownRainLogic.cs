namespace Gotcha.Levels.V64
{
    /// <summary>Stato del livello "The Upside-Down Rain" (roadmap n. 101).</summary>
    public enum UpsideDownRainState
    {
        Raining,    // piove: ogni DripInterval una goccia passa da un buco dell'ombrello e cade in testa all'omino
        Soaked,     // bagnato fradicio (Wetness 1): battuta, per SoakTime la nuvola tira il fiato e lui si asciuga un po'
        Reversing,  // telefono capovolto (e armato): la pioggia torna su nella nuvola; asciutto per DryTime = vittoria
        Won,        // vittoria: esce il sole, l'omino chiude l'ombrello (input chiuso)
    }

    /// <summary>Cosa è successo dopo un input o un passo di tempo.</summary>
    public enum UpsideDownRainEvent
    {
        None,           // niente da mostrare
        Ignored,        // input dopo la vittoria
        Dripped,        // una goccia è appena passata da un buco (cade verso la testa: DripProgress 0..1)
        Hit,            // la goccia è arrivata in testa: Wetness + HitWet ("Brrr!")
        Soaked,         // Wetness arrivata a 1: "Maximum soggy." (nessuna sconfitta)
        Shook,          // fine di Soaked: si è scrollato (Wetness SoakDryTo), riprende a piovere
        UmbrellaMoved,  // l'ombrello è stato spostato/inclinato col dito: piove dentro lo stesso (sbagliato, innocuo)
        HolePlugged,    // dito su un buco: "There are too many holes." (la goccia passa da un altro buco)
        ManTapped,      // tocco sull'omino: "Brrr!" (sbagliato, innocuo)
        CloudTapped,    // tocco sulla nuvola: la nuvola se ne frega (sbagliato, innocuo)
        Reversing,      // telefono capovolto (armato): la pioggia inverte la traiettoria, comincia l'intervallo asciutto
        ReverseStopped, // raddrizzato prima di DryTime: riprende a piovere (annullato, niente perso)
        Fixed,          // VITTORIA: asciutto per DryTime col telefono capovolto (una volta sola)
    }

    /// <summary>
    /// Logica pura del livello "The Upside-Down Rain" (id upside_down_rain, roadmap n. 101) — "KEEP HIM DRY." Un omino
    /// sotto un ombrello pieno di buchi e una nuvola nera. Mentre piove, ogni <see cref="DripInterval"/> s una goccia
    /// passa da un buco e dopo <see cref="DripFall"/> s gli cade in testa (+<see cref="HitWet"/> di bagnato; da solo si
    /// asciuga di <see cref="DryRate"/> al secondo). A bagnato pieno: battuta, per <see cref="SoakTime"/> s niente gocce e
    /// lui si scrolla fino a <see cref="SoakDryTo"/>, poi riprende a piovere. Nessuna sconfitta, tutto reversibile.
    /// Inganni: spostare/inclinare l'ombrello e tappare i buchi col dito non servono (contano solo come tentativi).
    /// Soluzione esatta: capovolgere il telefono. La pioggia torna su nella nuvola (la goccia in volo risale e non
    /// colpisce più); se l'omino resta asciutto (nessuna goccia) per <see cref="DryTime"/> s col telefono capovolto, vince
    /// (una volta sola, da Raining o da Soaked). Raddrizzato prima: riprende a piovere, niente perso.
    /// Gravità: accelerometro nello schermo (gioco bloccato in verticale: dritto y ≈ -1, capovolto y ≈ +1), filtrata con
    /// <see cref="V64AGravity"/> e con le STESSE soglie, la stessa isteresi e lo stesso "armamento" di
    /// <see cref="AristocraticCatLogic"/>/<see cref="DishonestHourglassLogic"/>: capovolto sopra <see cref="InvertOn"/>,
    /// smette sotto <see cref="InvertOff"/>; conta solo dopo aver visto il telefono dritto (y filtrata sotto
    /// <see cref="ArmBelow"/>), quindi un telefono già capovolto all'avvio non vince finché non lo si raddrizza.
    /// Cancel() (pausa, perdita del fuoco) annulla l'intervallo asciutto, disarma e fa ripartire il filtro; il bagnato
    /// resta quello che era (valido). Nessuna dipendenza da Unity.
    /// </summary>
    public sealed class UpsideDownRainLogic
    {
        public const float InvertOn = AristocraticCatLogic.InvertOn;     // stesse soglie del gatto aristocratico e della clessidra
        public const float InvertOff = AristocraticCatLogic.InvertOff;
        public const float ArmBelow = AristocraticCatLogic.ArmBelow;
        public const float Smoothing = AristocraticCatLogic.Smoothing;
        public const float DryTime = 1.5f;        // asciutto col telefono capovolto per questo tempo = vittoria
        public const float DripInterval = 0.9f;   // ogni quanto una goccia passa da un buco
        public const float DripFall = 0.35f;      // dal buco alla testa
        public const float HitWet = 0.12f;        // bagnato aggiunto da una goccia in testa
        public const float DryRate = 0.03f;       // asciugatura spontanea mentre piove (al secondo)
        public const float StartWet = 0.25f;      // all'avvio è già un po' bagnato (e triste)
        public const float SoakTime = 2.5f;       // bagnato fradicio: tanto dura la pausa in cui si scrolla
        public const float SoakDryTo = 0.7f;      // ...e a tanto scende il bagnato
        public const float ReverseDryRate = 0.5f; // mentre la pioggia torna su anche le gocce addosso risalgono (solo grafica)
        /// <summary>Passo massimo di tempo (un fotogramma lunghissimo non salta eventi né la tenuta).</summary>
        public const float MaxStep = 0.1f;
        const float TimeEps = 1e-4f;

        readonly V64AGravity gravity = new V64AGravity();
        float dripT, fallT, soakT, dryT;
        bool armed, inverted, completed;

        public UpsideDownRainLogic()
        {
            gravity.Smoothing = Smoothing;
            Reset();
        }

        public UpsideDownRainState State { get; private set; }
        /// <summary>Quanto è bagnato l'omino 0..1 (il misuratore WET, le gocce addosso, la pozzanghera).</summary>
        public float Wetness { get; private set; }
        public bool Armed => armed;
        public bool Inverted => inverted;
        public bool Reversing => State == UpsideDownRainState.Reversing;
        /// <summary>Una goccia sta cadendo dal buco verso la testa.</summary>
        public bool DripInFlight => fallT >= 0f && State == UpsideDownRainState.Raining;
        /// <summary>Avanzamento della goccia in volo 0..1 (0 se non ce n'è).</summary>
        public float DripProgress => DripInFlight ? Clamp01(fallT / DripFall) : 0f;
        /// <summary>Avanzamento dell'intervallo asciutto col telefono capovolto 0..1 (1 dopo la vittoria).</summary>
        public float DryProgress => State == UpsideDownRainState.Won ? 1f : Reversing ? Clamp01(dryT / DryTime) : 0f;
        /// <summary>Avanzamento della pausa da fradicio 0..1.</summary>
        public float SoakProgress => State == UpsideDownRainState.Soaked ? Clamp01(soakT / SoakTime) : 0f;
        public float GravityY => gravity.Y;
        public float GravityX => gravity.X;
        public int Drips { get; private set; }
        public int Hits { get; private set; }
        public int Soaks { get; private set; }
        public int UmbrellaMoves { get; private set; }
        public int HolePlugs { get; private set; }
        public int ManTaps { get; private set; }
        public int CloudTaps { get; private set; }
        public int ReverseStops { get; private set; }
        /// <summary>Tentativi sbagliati (ombrello, buchi, tocchi).</summary>
        public int WrongMoves { get; private set; }
        public bool InputLocked => State == UpsideDownRainState.Won;

        /// <summary>Si ricomincia da capo (rientro nel livello): piove, un po' bagnato, non armato, contatori a zero.</summary>
        public void Reset()
        {
            State = UpsideDownRainState.Raining;
            gravity.Reset();
            Wetness = StartWet;
            dripT = soakT = dryT = 0f;
            fallT = -1f;
            armed = inverted = completed = false;
            Drips = Hits = Soaks = UmbrellaMoves = HolePlugs = ManTaps = CloudTaps = ReverseStops = WrongMoves = 0;
        }

        /// <summary>
        /// Pausa, app in sottofondo, perdita del fuoco: l'intervallo asciutto si annulla (si torna a piovere), il telefono
        /// va rivisto dritto (disarmato), il filtro riparte dalla prossima lettura. Il bagnato non cambia.
        /// </summary>
        public void Cancel()
        {
            if (State == UpsideDownRainState.Won) return;
            if (State == UpsideDownRainState.Reversing) { State = UpsideDownRainState.Raining; dripT = 0f; }
            dryT = 0f;
            inverted = armed = false;
            gravity.Reset();
        }

        /// <summary>L'ombrello è stato spostato/inclinato col dito (chiamata al rilascio, se si è mosso davvero).</summary>
        public UpsideDownRainEvent MoveUmbrella() => Wrong(UpsideDownRainEvent.UmbrellaMoved);

        /// <summary>Dito su un buco dell'ombrello.</summary>
        public UpsideDownRainEvent PlugHole() => Wrong(UpsideDownRainEvent.HolePlugged);

        public UpsideDownRainEvent TapMan() => Wrong(UpsideDownRainEvent.ManTapped);

        public UpsideDownRainEvent TapCloud() => Wrong(UpsideDownRainEvent.CloudTapped);

        UpsideDownRainEvent Wrong(UpsideDownRainEvent e)
        {
            if (State == UpsideDownRainState.Won) return UpsideDownRainEvent.Ignored;
            WrongMoves++;
            switch (e)
            {
                case UpsideDownRainEvent.UmbrellaMoved: UmbrellaMoves++; break;
                case UpsideDownRainEvent.HolePlugged: HolePlugs++; break;
                case UpsideDownRainEvent.ManTapped: ManTaps++; break;
                case UpsideDownRainEvent.CloudTapped: CloudTaps++; break;
            }
            return e;
        }

        /// <summary>Passo di tempo con la lettura dell'accelerometro (già nel sistema dello schermo, in g).</summary>
        public UpsideDownRainEvent Tick(float dt, float ax, float ay)
        {
            if (State == UpsideDownRainState.Won) return UpsideDownRainEvent.None;
            if (dt < 0f || float.IsNaN(dt) || float.IsInfinity(dt)) dt = 0f;
            if (dt > MaxStep) dt = MaxStep;
            gravity.Feed(ax, ay, dt);
            float y = gravity.Y;
            if (!armed && y < ArmBelow) armed = true;
            if (!inverted && y > InvertOn) inverted = true;
            else if (inverted && y < InvertOff) inverted = false;

            // la pioggia al contrario
            if (State == UpsideDownRainState.Reversing)
            {
                if (!inverted)
                {
                    State = UpsideDownRainState.Raining;
                    dryT = 0f;
                    dripT = 0f;
                    ReverseStops++;
                    return UpsideDownRainEvent.ReverseStopped;
                }
                dryT += dt;
                Wetness = Wetness - ReverseDryRate * dt < 0f ? 0f : Wetness - ReverseDryRate * dt;
                if (dryT >= DryTime - TimeEps)
                {
                    State = UpsideDownRainState.Won;
                    return UpsideDownRainEvent.Fixed;
                }
                return UpsideDownRainEvent.None;
            }
            if (inverted && armed)
            {
                State = UpsideDownRainState.Reversing;
                dryT = 0f;
                fallT = -1f;   // la goccia in volo torna su: non lo colpisce più
                soakT = 0f;
                return UpsideDownRainEvent.Reversing;
            }

            // fradicio: niente gocce, si scrolla fino a SoakDryTo
            if (State == UpsideDownRainState.Soaked)
            {
                soakT += dt;
                float w = 1f - (1f - SoakDryTo) * Clamp01(soakT / SoakTime);
                Wetness = w < SoakDryTo ? SoakDryTo : w;
                if (soakT >= SoakTime - TimeEps)
                {
                    State = UpsideDownRainState.Raining;
                    Wetness = SoakDryTo;
                    dripT = 0f;
                    return UpsideDownRainEvent.Shook;
                }
                return UpsideDownRainEvent.None;
            }

            // piove
            Wetness = Wetness - DryRate * dt < 0f ? 0f : Wetness - DryRate * dt;
            if (fallT >= 0f)
            {
                fallT += dt;
                if (fallT >= DripFall - TimeEps)
                {
                    fallT = -1f;
                    Hits++;
                    Wetness += HitWet;
                    if (Wetness >= 1f - TimeEps)
                    {
                        Wetness = 1f;
                        State = UpsideDownRainState.Soaked;
                        soakT = 0f;
                        Soaks++;
                        return UpsideDownRainEvent.Soaked;
                    }
                    return UpsideDownRainEvent.Hit;
                }
            }
            dripT += dt;
            if (dripT >= DripInterval - TimeEps)
            {
                dripT -= DripInterval;
                if (dripT < 0f) dripT = 0f;
                fallT = 0f;
                Drips++;
                return UpsideDownRainEvent.Dripped;
            }
            return UpsideDownRainEvent.None;
        }

        /// <summary>Vittoria da registrare: true una volta sola, dopo l'intervallo asciutto.</summary>
        public bool TryComplete()
        {
            if (State != UpsideDownRainState.Won || completed) return false;
            completed = true;
            return true;
        }

        static float Clamp01(float v) => v < 0f ? 0f : v > 1f ? 1f : v;
    }
}
