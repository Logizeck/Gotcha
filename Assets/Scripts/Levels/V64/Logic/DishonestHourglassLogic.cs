namespace Gotcha.Levels.V64
{
    /// <summary>Stato del livello "The Dishonest Hourglass" (roadmap n. 82).</summary>
    public enum DishonestHourglassState
    {
        Running,    // la sabbia scende, il display conta alla rovescia (ADD TIME toglie secondi e accelera)
        Empty,      // arrivato a 00:00: "ding" triste, dopo EmptyTime si riempie da sola e riparte (nessuna sconfitta)
        Won,        // telefono capovolto e tenuto HoldTime: la clessidra si gira, +10 secondi, festa (input chiuso)
    }

    /// <summary>Cosa è successo dopo un input o un passo di tempo.</summary>
    public enum DishonestHourglassEvent
    {
        None,           // niente da mostrare
        Ignored,        // input dopo la vittoria
        Subtracted,     // ADD TIME: toglie PressPenalty secondi e accelera la sabbia ("Adding... negative time.")
        NothingToTake,  // ADD TIME con il display a 00:00: "Nothing left to add." (sbagliato, innocuo)
        RanOut,         // il tempo è finito (aspettando o premendo ADD TIME): ding, display 00:00
        Refilled,       // dopo EmptyTime: di nuovo 00:08, velocità normale (mai un game over)
        GlassTapped,    // tocco sulla clessidra: traballa, niente cambia
        DisplayTapped,  // tocco sul display: "I just show the numbers."
        Tipping,        // telefono capovolto (e armato): comincia la tenuta (nessuna parola, la sabbia trema)
        TippedBack,     // raddrizzato prima di HoldTime: annullato
        Turned,         // VITTORIA: capovolto per HoldTime, la clessidra guadagna 10 secondi (una volta sola)
    }

    /// <summary>
    /// Logica pura del livello "The Dishonest Hourglass" (id dishonest_hourglass, roadmap n. 82) — "GET 10 MORE
    /// SECONDS." Una clessidra quasi vuota e un display che conta alla rovescia da <see cref="StartSeconds"/>. Il
    /// pulsante ADD TIME mente: ogni pressione TOGLIE <see cref="PressPenalty"/> secondi e fa scendere la sabbia più in
    /// fretta (velocità +<see cref="SpeedStep"/>, al massimo <see cref="MaxSpeed"/>). Aspettare non serve: a zero suona
    /// un "ding", il display resta a 00:00 per <see cref="EmptyTime"/> s e poi tutto si riempie e riparte (velocità
    /// normale). Nessuna sconfitta, nessuna vita persa: tutto reversibile.
    /// Soluzione esatta: capovolgere davvero il telefono (la clessidra si gira): tenuto capovolto per
    /// <see cref="HoldTime"/> s guadagna <see cref="Bonus"/> secondi e vince (una volta sola, da Running o da Empty).
    /// Gravità: accelerometro nello schermo (gioco bloccato in verticale: dritto y ≈ -1, capovolto y ≈ +1), filtrata con
    /// <see cref="V64AGravity"/> e con le STESSE soglie, la stessa isteresi e lo stesso "armamento" di
    /// <see cref="AristocraticCatLogic"/>/<see cref="LuckHackLogic"/>: capovolto sopra <see cref="InvertOn"/>, smette
    /// sotto <see cref="InvertOff"/>; conta solo dopo aver visto il telefono dritto (y filtrata sotto
    /// <see cref="ArmBelow"/>), quindi un telefono già capovolto all'avvio non vince finché non lo si raddrizza.
    /// Cancel() (pausa, perdita del fuoco) annulla la tenuta, disarma e fa ripartire il filtro; il tempo resta quello che
    /// era (valido). Nessuna dipendenza da Unity.
    /// </summary>
    public sealed class DishonestHourglassLogic
    {
        public const float InvertOn = AristocraticCatLogic.InvertOn;     // stesse soglie del gatto aristocratico e di Luck Hack
        public const float InvertOff = AristocraticCatLogic.InvertOff;
        public const float ArmBelow = AristocraticCatLogic.ArmBelow;
        public const float Smoothing = AristocraticCatLogic.Smoothing;
        public const float HoldTime = 0.3f;       // tenuta capovolta che vince (oltre al filtro: niente vittorie per un traballio)
        public const float StartSeconds = 8f;     // il display parte da 00:08
        public const float Bonus = 10f;           // i secondi guadagnati capovolgendo
        public const float PressPenalty = 1.5f;   // secondi tolti da ogni ADD TIME
        public const float SpeedStep = 0.5f;      // accelerazione della sabbia per ogni ADD TIME
        public const float MaxSpeed = 3f;
        public const float EmptyTime = 1.5f;      // a 00:00 per questo tempo, poi si riempie e riparte
        /// <summary>Capienza della clessidra in secondi (per la sabbia disegnata: in alto Remaining / Capacity).</summary>
        public const float Capacity = 30f;
        const float TimeEps = 1e-4f;

        readonly V64AGravity gravity = new V64AGravity();
        float t, holdT;
        bool armed, inverted, completed;

        public DishonestHourglassLogic()
        {
            gravity.Smoothing = Smoothing;
            Reset();
        }

        public DishonestHourglassState State { get; private set; }
        /// <summary>Secondi rimasti (0..StartSeconds; dopo la vittoria quelli guadagnati).</summary>
        public float Remaining { get; private set; }
        /// <summary>Velocità della sabbia (1 = secondi veri).</summary>
        public float Speed { get; private set; }
        /// <summary>Secondi mostrati dal display (arrotondati per eccesso, come un conto alla rovescia).</summary>
        public int DisplaySeconds => Ceil(Remaining);
        /// <summary>Alla vittoria: secondi mostrati prima e dopo i +10 (il display conta su dall'uno all'altro).</summary>
        public int GainedFrom { get; private set; }
        public int GainedTo { get; private set; }
        /// <summary>Sabbia nel bulbo di sopra 0..1 (Remaining / Capacity); quella di sotto è il resto.</summary>
        public float TopSand01 => Clamp01(Remaining / Capacity);
        /// <summary>Secondi da quando il display è a 00:00 (solo in Empty).</summary>
        public float EmptySeconds => State == DishonestHourglassState.Empty ? t : 0f;
        public bool Armed => armed;
        public bool Inverted => inverted;
        /// <summary>Tenuta capovolta in corso (armata).</summary>
        public bool Tipping { get; private set; }
        /// <summary>Avanzamento della tenuta 0..1 (1 dopo la vittoria).</summary>
        public float HoldProgress => State == DishonestHourglassState.Won ? 1f : Tipping ? Clamp01(holdT / HoldTime) : 0f;
        public float GravityY => gravity.Y;
        public float GravityX => gravity.X;
        public int Presses { get; private set; }
        public int EmptyPresses { get; private set; }
        public int RunOuts { get; private set; }
        public int GlassTaps { get; private set; }
        public int DisplayTaps { get; private set; }
        public int TipBacks { get; private set; }
        /// <summary>Tentativi sbagliati (ADD TIME, tocchi, tempo finito).</summary>
        public int WrongMoves { get; private set; }
        public bool InputLocked => State == DishonestHourglassState.Won;

        /// <summary>Si ricomincia da capo (rientro nel livello): 00:08, velocità normale, non armato, contatori a zero.</summary>
        public void Reset()
        {
            State = DishonestHourglassState.Running;
            gravity.Reset();
            Remaining = StartSeconds;
            Speed = 1f;
            t = holdT = 0f;
            armed = inverted = completed = false;
            Tipping = false;
            GainedFrom = GainedTo = 0;
            Presses = EmptyPresses = RunOuts = GlassTaps = DisplayTaps = TipBacks = WrongMoves = 0;
        }

        /// <summary>
        /// Pausa, app in sottofondo, perdita del fuoco: la tenuta si annulla, il telefono va rivisto dritto (disarmato), il
        /// filtro riparte dalla prossima lettura. Il tempo rimasto non cambia.
        /// </summary>
        public void Cancel()
        {
            if (State == DishonestHourglassState.Won) return;
            holdT = 0f;
            Tipping = false;
            inverted = armed = false;
            gravity.Reset();
        }

        /// <summary>Pulsante ADD TIME (che toglie).</summary>
        public DishonestHourglassEvent PressAdd()
        {
            if (State == DishonestHourglassState.Won) return DishonestHourglassEvent.Ignored;
            WrongMoves++;
            if (State == DishonestHourglassState.Empty)
            {
                EmptyPresses++;
                return DishonestHourglassEvent.NothingToTake;
            }
            Presses++;
            Speed = Speed + SpeedStep > MaxSpeed ? MaxSpeed : Speed + SpeedStep;
            Remaining -= PressPenalty;
            if (Remaining <= TimeEps) return RunOut();
            return DishonestHourglassEvent.Subtracted;
        }

        /// <summary>Tocco sulla clessidra (la si scuote un po': non cambia niente).</summary>
        public DishonestHourglassEvent TapGlass()
        {
            if (State == DishonestHourglassState.Won) return DishonestHourglassEvent.Ignored;
            GlassTaps++;
            WrongMoves++;
            return DishonestHourglassEvent.GlassTapped;
        }

        /// <summary>Tocco sul display.</summary>
        public DishonestHourglassEvent TapDisplay()
        {
            if (State == DishonestHourglassState.Won) return DishonestHourglassEvent.Ignored;
            DisplayTaps++;
            WrongMoves++;
            return DishonestHourglassEvent.DisplayTapped;
        }

        /// <summary>Passo di tempo con la lettura dell'accelerometro (già nel sistema dello schermo, in g).</summary>
        public DishonestHourglassEvent Tick(float dt, float ax, float ay)
        {
            if (State == DishonestHourglassState.Won) return DishonestHourglassEvent.None;
            if (dt < 0f || float.IsNaN(dt) || float.IsInfinity(dt)) dt = 0f;
            gravity.Feed(ax, ay, dt);
            float y = gravity.Y;
            if (!armed && y < ArmBelow) armed = true;
            if (!inverted && y > InvertOn) inverted = true;
            else if (inverted && y < InvertOff) inverted = false;

            // il capovolgimento (vale sia mentre la sabbia scende sia a 00:00)
            if (Tipping)
            {
                if (!inverted)
                {
                    Tipping = false;
                    holdT = 0f;
                    TipBacks++;
                    return DishonestHourglassEvent.TippedBack;
                }
                holdT += dt;
                if (holdT >= HoldTime - TimeEps)
                {
                    Tipping = false;
                    GainedFrom = DisplaySeconds;
                    GainedTo = GainedFrom + (int)Bonus;
                    Remaining = GainedTo;
                    Speed = 1f;
                    State = DishonestHourglassState.Won;
                    t = 0f;
                    return DishonestHourglassEvent.Turned;
                }
            }
            else if (inverted && armed)
            {
                Tipping = true;
                holdT = 0f;
                return DishonestHourglassEvent.Tipping;
            }

            // il tempo (mentre si tiene capovolto la sabbia si ferma: sta per girarsi)
            if (Tipping) return DishonestHourglassEvent.None;
            t += dt;
            if (State == DishonestHourglassState.Empty)
            {
                if (t >= EmptyTime - TimeEps)
                {
                    State = DishonestHourglassState.Running;
                    Remaining = StartSeconds;
                    Speed = 1f;
                    t = 0f;
                    return DishonestHourglassEvent.Refilled;
                }
                return DishonestHourglassEvent.None;
            }
            Remaining -= dt * Speed;
            if (Remaining <= TimeEps) return RunOut();
            return DishonestHourglassEvent.None;
        }

        DishonestHourglassEvent RunOut()
        {
            Remaining = 0f;
            State = DishonestHourglassState.Empty;
            t = 0f;
            RunOuts++;
            WrongMoves++;
            return DishonestHourglassEvent.RanOut;
        }

        /// <summary>Vittoria da registrare: true una volta sola, dopo il capovolgimento.</summary>
        public bool TryComplete()
        {
            if (State != DishonestHourglassState.Won || completed) return false;
            completed = true;
            return true;
        }

        /// <summary>"00:08": minuti e secondi a due cifre (mai negativi).</summary>
        public static string Format(int seconds)
        {
            if (seconds < 0) seconds = 0;
            int m = seconds / 60, s = seconds % 60;
            return (m < 10 ? "0" : "") + m + ":" + (s < 10 ? "0" : "") + s;
        }

        static int Ceil(float v)
        {
            if (v <= TimeEps) return 0;
            int i = (int)v;
            return v - i > TimeEps ? i + 1 : i;
        }

        static float Clamp01(float v) => v < 0f ? 0f : v > 1f ? 1f : v;
    }
}
