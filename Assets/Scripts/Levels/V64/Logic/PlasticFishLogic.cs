using System;

namespace Gotcha.Levels.V64
{
    /// <summary>Fasi del livello 253.</summary>
    public enum FishState
    {
        Swimming,   // esplorazione: il pesce "nuota" (galleggia) nella boccia
        Drained,    // il giocatore ha versato troppa acqua: il pesce resta sospeso, poi l'acqua torna
        Refilling,  // l'acqua torna da sola
        Inflating,  // il pesce si gonfia ed esce dalla boccia (input bloccato)
        Done,
    }

    /// <summary>Cosa è appena successo (una cosa per volta: il livello sceglie scritta, suono e animazione).</summary>
    public enum FishEvent
    {
        None,
        ValveOpened,     // le due dita hanno allargato la valvolina: resta aperta
        FloatedUp,       // prima volta a telefono capovolto: il pesce sale come un giocattolo
        BlowBubbles,     // soffio a valvola chiusa: bolle e ossigeno giù più in fretta ("THAT DIDN'T HELP.")
        BlowUnderwater,  // soffio a valvola aperta ma sott'acqua: solo bolle
        Inflating,       // soffio buono: il pesce si gonfia un po'
        Drowning,        // troppa acqua versata: "HE'S STILL DROWNING. SOMEHOW."
        Refilled,        // l'acqua è tornata
        OxygenEmpty,     // ossigeno a zero... e il pesce sta benissimo (si ricarica)
        Inflated,        // due secondi di soffio buono: vittoria
    }

    /// <summary>
    /// Geometria della boccia tonda (raggio 1 = raggio interno): altezza del pelo dell'acqua lungo la verticale vera
    /// (opposta alla gravità) in funzione del riempimento, posizione del pesce che galleggia e della valvolina.
    /// Il cerchio è simmetrico, quindi vale con qualsiasi inclinazione. Riempimento 1 = boccia piena fino al collo.
    /// </summary>
    public static class FishBowlGeometry
    {
        public const float NeckSurface = 1.15f;   // pelo dell'acqua dentro il collo quando la boccia è piena
        public const float FishHalfHeight = 0.22f;
        public const float ValveUp = 0.15f;       // la valvola è sulla schiena, sopra il centro del pesce
        public const float FloatDepth = 0.05f;    // il centro del pesce sta appena sotto il pelo dell'acqua

        /// <summary>Frazione di cerchio sotto la retta a distanza d dal centro (d in -1..1).</summary>
        public static float FractionBelow(float d)
        {
            d = Math.Max(-1f, Math.Min(1f, d));
            return (float)((Math.Acos(-d) + d * Math.Sqrt(1.0 - d * d)) / Math.PI);
        }

        /// <summary>Altezza del pelo dell'acqua (dal centro, verso l'alto vero) per un riempimento.</summary>
        public static float SurfaceHeight(float fill)
        {
            if (fill >= 1f) return NeckSurface + (fill - 1f);
            if (fill <= 0f) return -1f;
            float lo = -1f, hi = 1f;
            for (int i = 0; i < 40; i++)
            {
                float mid = (lo + hi) * 0.5f;
                if (FractionBelow(mid) < fill) lo = mid; else hi = mid;
            }
            return (lo + hi) * 0.5f;
        }

        /// <summary>Centro del pesce: galleggia sul pelo dell'acqua, ma non può passare il vetro.</summary>
        public static float FishHeight(float fill, bool stuckMidBowl)
        {
            if (stuckMidBowl) return 0f;   // svuotata: resta sospeso a mezz'aria, in modo imbarazzante
            float h = SurfaceHeight(fill) - FloatDepth;
            return Math.Max(-1f + FishHalfHeight, Math.Min(1f - FishHalfHeight, h));
        }

        /// <summary>La valvolina (sulla schiena, sempre verso l'alto vero) è fuori dall'acqua?</summary>
        public static bool ValveExposed(float fill, bool stuckMidBowl)
        {
            return FishHeight(fill, stuckMidBowl) + ValveUp > SurfaceHeight(fill) + 0.01f;
        }
    }

    /// <summary>
    /// Logica pura del livello 253 "The Fish That Is Afraid of Water": il pesce è un giocattolo gonfiabile. La pompa
    /// aggiunge acqua, versarla non serve, soffiare a valvola chiusa fa solo bolle, l'ossigeno è finto. Soluzione:
    /// aprire la valvolina con due dita (resta aperta), capovolgere il telefono (il pesce sale nella bolla d'aria con la
    /// valvola fuori dall'acqua) e soffiare per circa due secondi in tutto. Nessuna dipendenza da Unity.
    /// </summary>
    public sealed class PlasticFishLogic
    {
        public const float BlowSeconds = 2f;
        public const float FullFill = 1f, InvertedFill = 0.72f, DrownFill = 0.35f, MaxFill = 1.15f;
        public const float OxygenDrain = 0.7f;        // % al secondo: scende pianissimo
        public const float OxygenBlowDrain = 9f;      // % al secondo soffiando a vuoto
        public const float SpillRate = 0.3f;          // riempimento perso al secondo versando di lato
        public const float GlugRate = 0.6f;           // capovolto: l'acqua scende al livello della bolla d'aria
        public const float RefillRate = 0.45f;
        public const float DrainedSeconds = 1.6f;
        public const float PumpStep = 0.06f;
        public const float BlowEventGap = 1.5f;       // la stessa reazione al soffio non si ripete prima di così

        public FishState State { get; private set; } = FishState.Swimming;
        public bool ValveOpen { get; private set; }
        public float Water { get; private set; } = FullFill;
        public float Oxygen { get; private set; } = 100f;
        public float BlowProgress { get; private set; }
        public float Distress { get; private set; }
        public int Squeaks { get; private set; }
        public int Pumps { get; private set; }
        public int Drownings { get; private set; }
        public bool FloatedOnce { get; private set; }
        public bool CompletionReported { get; private set; }

        float stateTimer, blowEventWait;
        FishEvent lastBlowEvent;

        public bool InputLocked => State == FishState.Inflating || State == FishState.Done;
        public bool StuckMidBowl => State == FishState.Drained;
        public bool ValveExposed => FishBowlGeometry.ValveExposed(Water, StuckMidBowl);
        public float BlowFraction => Math.Min(1f, BlowProgress / BlowSeconds);

        public void Reset()
        {
            State = FishState.Swimming;
            ValveOpen = false;
            Water = FullFill;
            Oxygen = 100f;
            BlowProgress = Distress = 0f;
            Squeaks = Pumps = Drownings = 0;
            FloatedOnce = CompletionReported = false;
            stateTimer = blowEventWait = 0f;
            lastBlowEvent = FishEvent.None;
        }

        /// <summary>Il pesce toccato: squittisce come la gomma (indizio).</summary>
        public int TapFish()
        {
            if (InputLocked) return Squeaks;
            Squeaks++;
            return Squeaks;
        }

        /// <summary>Le due dita allargano la valvolina: true la prima volta (poi resta aperta).</summary>
        public bool OpenValve()
        {
            if (InputLocked || ValveOpen) return false;
            ValveOpen = true;
            return true;
        }

        /// <summary>La pompa: aggiunge acqua, non ossigeno. Il pesce si agita di più.</summary>
        public bool Pump()
        {
            if (InputLocked) return false;
            Pumps++;
            if (State == FishState.Swimming) Water = Math.Min(MaxFill, Math.Max(Water, FullFill) + PumpStep);
            Distress = Math.Min(1f, Distress + 0.35f);
            return true;
        }

        /// <summary>
        /// Un passo di simulazione. inverted = telefono capovolto (con isteresi), sideways = versando di lato da un po',
        /// blowing = soffio sopra la soglia calibrata (o il pulsante alternativo tenuto premuto).
        /// </summary>
        public FishEvent Tick(float dt, bool inverted, bool sideways, bool blowing)
        {
            dt = Math.Max(0f, dt);
            if (InputLocked) return FishEvent.None;
            if (blowEventWait > 0f) blowEventWait -= dt;
            Distress = Math.Max(0f, Distress - 0.25f * dt);
            FishEvent ev = FishEvent.None;

            // acqua
            switch (State)
            {
                case FishState.Swimming:
                    if (inverted)
                    {
                        Water = Math.Max(InvertedFill, Math.Min(Water, FullFill) - GlugRate * dt);
                        if (!FloatedOnce) { FloatedOnce = true; ev = FishEvent.FloatedUp; }
                    }
                    else if (sideways)
                    {
                        Water -= SpillRate * dt;
                        if (Water < DrownFill)
                        {
                            Water = DrownFill;
                            State = FishState.Drained;
                            stateTimer = DrainedSeconds;
                            Drownings++;
                            ev = FishEvent.Drowning;
                        }
                    }
                    else if (Water > FullFill) Water = Math.Max(FullFill, Water - 0.08f * dt);   // trabocca piano
                    else if (Water < FullFill) Water = Math.Min(FullFill, Water + RefillRate * dt); // la pompa rimbocca
                    break;
                case FishState.Drained:
                    stateTimer -= dt;
                    if (stateTimer <= 0f) State = FishState.Refilling;
                    break;
                case FishState.Refilling:
                    Water = Math.Min(FullFill, Water + RefillRate * 1.5f * dt);
                    if (Water >= FullFill) { State = FishState.Swimming; if (ev == FishEvent.None) ev = FishEvent.Refilled; }
                    break;
            }

            // ossigeno (finto)
            Oxygen -= OxygenDrain * dt;
            bool exposed = ValveExposed;
            if (blowing && !(ValveOpen && exposed)) Oxygen -= OxygenBlowDrain * dt;
            if (Oxygen <= 0f)
            {
                Oxygen = 100f;
                if (ev == FishEvent.None) ev = FishEvent.OxygenEmpty;
            }

            // soffio
            if (blowing)
            {
                FishEvent b;
                if (!ValveOpen) b = FishEvent.BlowBubbles;
                else if (!exposed) b = FishEvent.BlowUnderwater;
                else
                {
                    BlowProgress += dt;
                    b = FishEvent.Inflating;
                    if (BlowProgress >= BlowSeconds)
                    {
                        BlowProgress = BlowSeconds;
                        State = FishState.Inflating;
                        return FishEvent.Inflated;
                    }
                }
                if (b != lastBlowEvent || blowEventWait <= 0f)
                {
                    lastBlowEvent = b;
                    blowEventWait = BlowEventGap;
                    if (ev == FishEvent.None || b == FishEvent.Inflating) ev = b;
                }
            }
            return ev;
        }

        /// <summary>Fine della scenetta di vittoria: true una volta sola.</summary>
        public bool TryComplete()
        {
            if (State != FishState.Inflating || CompletionReported) return false;
            CompletionReported = true;
            State = FishState.Done;
            return true;
        }
    }
}
