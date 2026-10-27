using System;

namespace Gotcha.Levels.V64
{
    /// <summary>Stati del livello 257 "The perfect liar".</summary>
    public enum LiarState
    {
        Exploring,      // la macchina è montata capovolta: i test danno risultati assurdi
        Gripping,       // due dita sulle maniglie: si aspetta che il telefono venga capovolto
        Corrected,      // il meccanismo è scivolato al suo posto (resta così): si aspetta che le dita si stacchino
        Calibrated,     // tre spie verdi: tutti innocenti? si aspetta il TEST
        Awaiting,       // "AWAITING FINAL STATEMENT": basta un suono qualsiasi
        Accusing,       // "PLAYER — LIAR": animazione finale (input bloccato)
        Done,
    }

    /// <summary>Cosa è successo in un passo della logica (per l'animazione).</summary>
    public enum LiarEvent
    {
        None,
        GripStarted,    // entrambe le maniglie afferrate
        GripLost,       // un dito si è staccato prima del capovolgimento: si ricomincia la presa (niente altro si perde)
        Corrected,      // telefono capovolto con la presa: il meccanismo scivola giù, il logo è dritto
        Calibrated,     // dita staccate dopo la correzione: spie verdi
    }

    /// <summary>Risposta al pulsante TEST.</summary>
    public enum LiarTest
    {
        Ignored,
        Contradictory,  // prima della correzione: tre spie accese e risultati contraddittori
        LetGoFirst,     // macchina corretta ma ancora tenuta: "release the handles"
        AwaitingStatement,
        AlreadyWaiting,
    }

    /// <summary>Risposta a un suono nel microfono.</summary>
    public enum LiarSound
    {
        Ignored,
        Unstable,       // urlo prima del momento giusto: tutti si girano, "UNSTABLE"
        PlayerIsLiar,   // il suono viene da fuori dai tre sospettati: vittoria
    }

    /// <summary>
    /// Capovolgimento del telefono dalla gravità filtrata (y dello schermo: -1 dritto, +1 capovolto), con isteresi:
    /// si entra sopra Enter e si esce sotto Exit. Il gioco è bloccato in verticale, quindi basta l'asse y.
    /// </summary>
    public sealed class LiarFlipSensor
    {
        public const float Enter = 0.5f, Exit = 0.2f;
        /// <summary>Costante di tempo del filtro passa-basso (secondi).</summary>
        public const float Smoothing = 0.12f;

        public float Filtered { get; private set; } = -1f;
        public bool UpsideDown { get; private set; }
        bool primed;

        public void Reset() { Filtered = -1f; UpsideDown = false; primed = false; }

        public bool Feed(float dt, float gravityY)
        {
            if (float.IsNaN(gravityY)) gravityY = 0f;
            if (!primed) { primed = true; Filtered = gravityY; }
            else if (dt > 0f) Filtered += (gravityY - Filtered) * (1f - (float)Math.Exp(-dt / Smoothing));
            if (!UpsideDown && Filtered > Enter) UpsideDown = true;
            else if (UpsideDown && Filtered < Exit) UpsideDown = false;
            return UpsideDown;
        }
    }

    /// <summary>
    /// Rilevatore di suoni semplice (solo ampiezza, niente parole): impara il rumore di fondo quando è tranquillo e
    /// segnala un suono quando il livello supera la soglia per un attimo; poi deve tornare quieto prima di contare un
    /// altro suono (un suono lungo non si ripete di continuo). Un suono che durante la stessa "raffica" supera anche
    /// la soglia alta viene segnalato una volta come urlo.
    /// </summary>
    public sealed class LiarMicGate
    {
        public const float Warmup = 0.5f;        // secondi iniziali solo per imparare il rumore di fondo
        public const float MinThreshold = 0.16f, MaxThreshold = 0.55f;
        public const float LoudMin = 0.62f;
        public const float Onset = 0.06f;        // secondi sopra soglia per contare un suono
        public const float Rearm = 0.3f;         // secondi di quiete prima del suono successivo

        public float Ambient { get; private set; } = 0.03f;
        public float Threshold => Clamp(Ambient * 2f + 0.1f, MinThreshold, MaxThreshold);
        public float LoudThreshold => Math.Max(LoudMin, Threshold + 0.3f);

        float warm, above, quiet;
        bool inBurst, loudSent;

        public void Reset() { Ambient = 0.03f; warm = above = quiet = 0f; inBurst = loudSent = false; }

        /// <summary>Un passo: ritorna (suono iniziato, urlo) per questo frame.</summary>
        public void Feed(float dt, float level, out bool sound, out bool loud)
        {
            sound = loud = false;
            if (dt <= 0f) return;
            if (float.IsNaN(level) || level < 0f) level = 0f;
            if (warm < Warmup)
            {
                warm += dt;
                Ambient += (Math.Min(level, MaxThreshold) - Ambient) * Math.Min(1f, dt * 6f);
                return;
            }
            float thr = Threshold;
            if (level < thr)
            {
                // si impara il rumore di fondo solo quando nessuno sta facendo rumore
                if (!inBurst) Ambient += (level - Ambient) * Math.Min(1f, dt * 0.8f);
                above = 0f;
                if (inBurst) { quiet += dt; if (quiet >= Rearm) { inBurst = false; loudSent = false; } }
                return;
            }
            quiet = 0f;
            if (!inBurst)
            {
                above += dt;
                if (above >= Onset) { inBurst = true; sound = true; }
            }
            if (inBurst && !loudSent && level >= LoudThreshold) { loudSent = true; loud = true; }
        }

        static float Clamp(float v, float a, float b) => v < a ? a : v > b ? b : v;
    }

    /// <summary>
    /// Logica pura del livello 257 "The perfect liar". Tre sospettati, una macchina della verità installata a testa in
    /// giù. Il TEST dà risultati contraddittori, scuotere fa solo picchi senza senso, urlare fa girare tutti e dà
    /// "UNSTABLE". Soluzione: afferrare con due dita le due estremità della macchina e, tenendole, capovolgere il
    /// telefono (conta l'orientamento, non la rotazione delle dita); il meccanismo scivola al suo posto e lo stato
    /// resta. Si staccano le dita (spie verdi), si preme TEST ("AWAITING FINAL STATEMENT") e si fa un suono qualsiasi:
    /// il bugiardo è il giocatore. Niente vera macchina della verità: è una battuta scritta. Nessuna dipendenza da Unity.
    /// </summary>
    public sealed class PerfectLiarLogic
    {
        /// <summary>Secondi di telefono capovolto (con la presa) per far scivolare il meccanismo.</summary>
        public const float FlipHold = 0.35f;
        /// <summary>Tocchi sullo stesso sospettato dopo i quali si arrabbia.</summary>
        public const int AnnoyedAfter = 3;

        public LiarState State { get; private set; } = LiarState.Exploring;
        public bool MachineCorrected => State >= LiarState.Corrected;
        public int Tests { get; private set; }
        public int Shouts { get; private set; }
        public int Shakes { get; private set; }
        public int GripsLost { get; private set; }
        public bool CompletionReported { get; private set; }
        public bool InputLocked => State == LiarState.Accusing || State == LiarState.Done;
        /// <summary>Il microfono serve davvero (per la battuta finale) solo da qui in poi.</summary>
        public bool ListeningForStatement => State == LiarState.Awaiting;

        public readonly LiarFlipSensor Flip = new LiarFlipSensor();
        readonly int[] taps = new int[3];
        float upsideFor;
        bool sawUpright;

        public void Reset()
        {
            State = LiarState.Exploring;
            Tests = Shouts = Shakes = GripsLost = 0;
            CompletionReported = false;
            Flip.Reset();
            Array.Clear(taps, 0, taps.Length);
            upsideFor = 0f;
            sawUpright = false;
        }

        /// <summary>
        /// Un passo: gravità y dello schermo (non filtrata) e quali estremità della macchina sono tenute.
        /// </summary>
        public LiarEvent Tick(float dt, float gravityY, bool leftHeld, bool rightHeld)
        {
            bool upside = Flip.Feed(dt, gravityY);
            bool both = leftHeld && rightHeld;
            switch (State)
            {
                case LiarState.Exploring:
                    if (!both) return LiarEvent.None;
                    State = LiarState.Gripping;
                    upsideFor = 0f;
                    // se la presa comincia già a telefono capovolto, prima bisogna rigirarlo: conta il cambiamento
                    sawUpright = !upside;
                    return LiarEvent.GripStarted;
                case LiarState.Gripping:
                    if (!both)
                    {
                        State = LiarState.Exploring;
                        GripsLost++;
                        upsideFor = 0f;
                        return LiarEvent.GripLost;
                    }
                    if (!upside) { sawUpright = true; upsideFor = 0f; return LiarEvent.None; }
                    if (!sawUpright) return LiarEvent.None;
                    upsideFor += Math.Max(0f, dt);
                    if (upsideFor < FlipHold) return LiarEvent.None;
                    State = LiarState.Corrected;
                    return LiarEvent.Corrected;
                case LiarState.Corrected:
                    if (leftHeld || rightHeld) return LiarEvent.None;
                    State = LiarState.Calibrated;
                    return LiarEvent.Calibrated;
                default:
                    return LiarEvent.None;
            }
        }

        public LiarTest PressTest()
        {
            switch (State)
            {
                case LiarState.Exploring:
                case LiarState.Gripping:
                    Tests++;
                    return LiarTest.Contradictory;
                case LiarState.Corrected: return LiarTest.LetGoFirst;
                case LiarState.Calibrated:
                    State = LiarState.Awaiting;
                    return LiarTest.AwaitingStatement;
                case LiarState.Awaiting: return LiarTest.AlreadyWaiting;
                default: return LiarTest.Ignored;
            }
        }

        /// <summary>Un suono rilevato (loud = urlo). Prima del TEST finale solo l'urlo fa reagire.</summary>
        public LiarSound Sound(bool loud)
        {
            if (State == LiarState.Awaiting)
            {
                State = LiarState.Accusing;
                return LiarSound.PlayerIsLiar;
            }
            if (InputLocked || !loud) return LiarSound.Ignored;
            Shouts++;
            return LiarSound.Unstable;
        }

        /// <summary>Tocco su un sospettato: ritorna quante volte è stato toccato (dopo AnnoyedAfter si arrabbia).</summary>
        public int TapSuspect(int index)
        {
            if (InputLocked || index < 0 || index >= taps.Length) return 0;
            return ++taps[index];
        }

        public bool IsAnnoyed(int index) => index >= 0 && index < taps.Length && taps[index] >= AnnoyedAfter;

        /// <summary>Scossone: solo picchi senza senso sul grafico (mai un cambio di stato).</summary>
        public bool Shake()
        {
            if (InputLocked) return false;
            Shakes++;
            return true;
        }

        /// <summary>Fine dell'animazione: true una volta sola.</summary>
        public bool TryComplete()
        {
            if (State != LiarState.Accusing || CompletionReported) return false;
            CompletionReported = true;
            State = LiarState.Done;
            return true;
        }
    }
}
