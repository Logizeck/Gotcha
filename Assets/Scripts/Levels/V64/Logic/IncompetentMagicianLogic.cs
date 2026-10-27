using System;

namespace Gotcha.Levels.V64
{
    /// <summary>Stati del livello 256 "The Incompetent Magician".</summary>
    public enum MagicianState
    {
        Performing,   // il mago guarda (e rovina ogni trucco)
        Blindfolded,  // due dita sugli occhi: occhi chiusi, non interferisce
        RubbingEyes,  // botola già scoperta, dita tolte: per qualche secondo si stropiccia gli occhi
        Vanishing,    // il coniglio è sceso nella botola: animazione finale (input bloccato)
        Done,
    }

    /// <summary>Cosa succede dopo un'azione del giocatore.</summary>
    public enum MagicianReaction
    {
        None,
        EyesClosed,         // entrambi gli occhi coperti
        EyesOpen,           // un dito tolto prima del sussurro: riapre gli occhi
        RubsEyes,           // botola già scoperta: resta bendato per un po' (si stropiccia gli occhi)
        RabbitPoints,       // il coniglio ha sentito il sussurro e indica la botola
        FailedSpell,        // suono al microfono a occhi aperti: tenta una magia, non succede niente
        Bows,               // tocco sul mago: si inchina e non aiuta
        Confetti,           // tocco sulla bacchetta: coriandoli, si applaude da solo
        RabbitWearsHat,     // coniglio nel cilindro: salta fuori col cilindro in testa
        RabbitReappears,    // coniglio in una scatola: ricompare fuori dalla stessa scatola
        HollowFloor,        // tocco sulla botola ancora nascosta: suono vuoto (indizio)
        TrapdoorOpens,
        MagicianStopsYou,   // il mago vede la botola aprirsi e la richiude
        MagicianSlamsShut,  // finito lo stropicciamento, trova la botola aperta e la richiude
        MagicianCatchesRabbit, // coniglio nella botola sotto gli occhi del mago: lo ripesca
        RabbitVanishes,
    }

    /// <summary>
    /// Logica pura del livello 256: il mago rovina ogni trucco finché vede. Soluzione: coprirgli entrambi gli occhi
    /// con due dita, fare un piccolo suono (il coniglio "sente" e indica la botola nascosta), poi, mentre il mago si
    /// stropiccia gli occhi, aprire la botola e trascinarci il coniglio. La scoperta della botola non si azzera mai.
    /// Senza microfono (o se il giocatore non può fare suoni) basta tenere coperti gli occhi più a lungo.
    /// Nessuna dipendenza da Unity.
    /// </summary>
    public sealed class IncompetentMagicianLogic
    {
        /// <summary>Secondi di suono (sopra la soglia) a occhi coperti perché il coniglio senta.</summary>
        public const float WhisperNeeded = 0.25f;
        /// <summary>Secondi in cui il mago resta "bendato" dopo che si tolgono le dita (botola già scoperta).</summary>
        public const float GraceSeconds = 7f;
        /// <summary>Alternativa senza microfono: compare dopo questi secondi a occhi coperti e si completa tenendo ancora.</summary>
        public const float FallbackAfter = 8f, FallbackHold = 2.5f;
        public const float SpellCooldown = 2.5f;

        public MagicianState State { get; private set; } = MagicianState.Performing;
        public bool TrapdoorFound { get; private set; }
        public bool TrapdoorOpen { get; private set; }
        public float GraceLeft { get; private set; }
        public float Whisper { get; private set; }
        /// <summary>Secondi totali a occhi coperti prima della scoperta (per mostrare l'alternativa al microfono).</summary>
        public float BlindTime { get; private set; }
        public float FallbackProgress { get; private set; }
        public int Bows { get; private set; }
        public int Spells { get; private set; }
        public int HatTries { get; private set; }
        public int BoxTries { get; private set; }
        public int Catches { get; private set; }
        public bool CompletionReported { get; private set; }

        float spellWait;

        public bool InputLocked => State == MagicianState.Vanishing || State == MagicianState.Done;
        public bool EyesClosed => State == MagicianState.Blindfolded || State == MagicianState.RubbingEyes;
        /// <summary>Il mago vede il palco (e quindi rovina il trucco).</summary>
        public bool Watching => State == MagicianState.Performing;
        /// <summary>Il microfono serve solo finché la botola non è stata scoperta.</summary>
        public bool NeedsMic => !TrapdoorFound && !InputLocked;

        public void Reset()
        {
            State = MagicianState.Performing;
            TrapdoorFound = TrapdoorOpen = false;
            GraceLeft = Whisper = BlindTime = FallbackProgress = 0f;
            Bows = Spells = HatTries = BoxTries = Catches = 0;
            CompletionReported = false;
            spellWait = 0f;
        }

        /// <summary>L'alternativa al microfono è disponibile (microfono assente/negato o tanto tempo a occhi coperti).</summary>
        public bool FallbackReady(bool micAvailable) => !TrapdoorFound && (!micAvailable || BlindTime >= FallbackAfter);

        /// <summary>Entrambi gli occhi coperti (true) o no (false). Va chiamato a ogni cambiamento.</summary>
        public MagicianReaction SetEyesCovered(bool covered)
        {
            if (InputLocked) return MagicianReaction.None;
            if (covered)
            {
                if (State == MagicianState.Blindfolded) return MagicianReaction.None;
                State = MagicianState.Blindfolded;
                GraceLeft = 0f;
                return MagicianReaction.EyesClosed;
            }
            if (State != MagicianState.Blindfolded) return MagicianReaction.None;
            Whisper = 0f;
            FallbackProgress = 0f;
            if (TrapdoorFound)
            {
                State = MagicianState.RubbingEyes;
                GraceLeft = GraceSeconds;
                return MagicianReaction.RubsEyes;
            }
            State = MagicianState.Performing;
            return MagicianReaction.EyesOpen;
        }

        /// <summary>
        /// Avanza il tempo. sound = il microfono sente qualcosa sopra la soglia calibrata in questo frame.
        /// Restituisce RabbitPoints (scoperta), FailedSpell (suono a occhi aperti), EyesOpen / MagicianSlamsShut (fine
        /// dello stropicciamento).
        /// </summary>
        public MagicianReaction Tick(float dt, bool sound, bool micAvailable)
        {
            if (InputLocked) return MagicianReaction.None;
            if (dt < 0f) dt = 0f;
            if (spellWait > 0f) spellWait = Math.Max(0f, spellWait - dt);
            switch (State)
            {
                case MagicianState.Blindfolded:
                    if (TrapdoorFound) return MagicianReaction.None;
                    BlindTime += dt;
                    if (sound) Whisper += dt;
                    if (FallbackReady(micAvailable)) FallbackProgress += dt;
                    if (Whisper >= WhisperNeeded || FallbackProgress >= FallbackHold)
                    {
                        TrapdoorFound = true;
                        return MagicianReaction.RabbitPoints;
                    }
                    return MagicianReaction.None;
                case MagicianState.RubbingEyes:
                    GraceLeft -= dt;
                    if (GraceLeft > 0f) return MagicianReaction.None;
                    GraceLeft = 0f;
                    State = MagicianState.Performing;
                    if (TrapdoorOpen) { TrapdoorOpen = false; return MagicianReaction.MagicianSlamsShut; }
                    return MagicianReaction.EyesOpen;
                default:
                    if (!sound || spellWait > 0f || TrapdoorFound) return MagicianReaction.None;
                    spellWait = SpellCooldown;
                    Spells++;
                    return MagicianReaction.FailedSpell;
            }
        }

        public MagicianReaction TapMagician()
        {
            if (InputLocked || !Watching) return MagicianReaction.None;
            Bows++;
            return MagicianReaction.Bows;
        }

        public MagicianReaction TapWand() => InputLocked || !Watching ? MagicianReaction.None : MagicianReaction.Confetti;

        public MagicianReaction RabbitIntoHat()
        {
            if (InputLocked) return MagicianReaction.None;
            HatTries++;
            return MagicianReaction.RabbitWearsHat;
        }

        public MagicianReaction RabbitIntoBox()
        {
            if (InputLocked) return MagicianReaction.None;
            BoxTries++;
            return MagicianReaction.RabbitReappears;
        }

        /// <summary>Il giocatore apre (trascina) la botola.</summary>
        public MagicianReaction TryOpenTrapdoor()
        {
            if (InputLocked || TrapdoorOpen) return MagicianReaction.None;
            if (!TrapdoorFound) return MagicianReaction.HollowFloor;
            if (Watching) return MagicianReaction.MagicianStopsYou;
            TrapdoorOpen = true;
            return MagicianReaction.TrapdoorOpens;
        }

        /// <summary>Il coniglio lasciato sopra la botola aperta.</summary>
        public MagicianReaction DropRabbitInTrapdoor()
        {
            if (InputLocked || !TrapdoorOpen) return MagicianReaction.None;
            if (Watching)
            {
                Catches++;
                TrapdoorOpen = false;
                return MagicianReaction.MagicianCatchesRabbit;
            }
            State = MagicianState.Vanishing;
            return MagicianReaction.RabbitVanishes;
        }

        public bool TryComplete()
        {
            if (State != MagicianState.Vanishing || CompletionReported) return false;
            State = MagicianState.Done;
            CompletionReported = true;
            return true;
        }
    }

    /// <summary>
    /// Soglia del microfono calibrata sul rumore di fondo: la base segue lentamente il livello quando non c'è suono,
    /// e un suono vale solo se resta sopra la soglia per un attimo (niente scatti singoli). Nessun riconoscimento vocale.
    /// </summary>
    public sealed class MagicianMicGate
    {
        public const float MinThreshold = 0.07f, Margin = 0.05f, HoldNeeded = 0.12f, CalibrationSeconds = 0.5f;
        public float Baseline { get; private set; } = 0.02f;
        public float Threshold => Math.Max(MinThreshold, Baseline * 1.6f + Margin);
        float above, calibrated;
        int samples;

        public bool Calibrating => calibrated < CalibrationSeconds;

        public void Reset() { Baseline = 0.02f; above = 0f; calibrated = 0f; samples = 0; }

        /// <summary>Un campione del livello (0..1). True mentre c'è un suono "valido".</summary>
        public bool Feed(float level, float dt)
        {
            if (float.IsNaN(level) || level < 0f) level = 0f;
            if (dt < 0f) dt = 0f;
            if (calibrated < CalibrationSeconds)
            {
                // primo mezzo secondo: si ascolta solo il rumore di fondo (media), nessun suono vale
                calibrated += dt;
                samples++;
                Baseline += (Math.Min(level, 0.5f) - Baseline) / samples;
                return false;
            }
            if (level > Threshold)
            {
                above += dt;
                // rumore forte e continuo da secoli (ventilatore, strada): è il nuovo rumore di fondo
                if (above > 4f) Baseline += (Math.Min(level, 0.5f) - Baseline) * Math.Min(1f, dt * 0.3f);
                return above >= HoldNeeded;
            }
            above = 0f;
            Baseline += (Math.Min(level, 0.5f) - Baseline) * Math.Min(1f, dt * 0.8f);   // poi la base si segue piano
            return false;
        }
    }
}
