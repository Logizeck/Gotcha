namespace Gotcha.Levels.V62
{
    /// <summary>Come è messo l'ombrello.</summary>
    public enum UmbrellaPose { Closed, Open, Inverted }

    /// <summary>Stati del livello 255 (annaffiare la pianta nella serra).</summary>
    public enum UmbrellaState
    {
        Playing,    // si gioca: l'ombrello si apre, si gira, si riempie e si svuota
        Blooming,   // la pianta beve e fiorisce (input bloccato)
        Done,
    }

    /// <summary>Esito di un versamento.</summary>
    public enum PourResult { Nothing, Wasted, NotEnough, Bloom }

    /// <summary>
    /// Logica pura del livello 255 "Upside-down umbrella": il rubinetto non ha la manopola, l'ombrello aperto sotto la
    /// pioggia ripara e basta. Soluzione: aprirlo, girarlo a testa in giù, riempirlo sotto la pioggia (~4 s), portarlo
    /// sopra la pianta e versare (inclinando o toccando). Versare altrove spreca l'acqua (si riempie di nuovo).
    /// Easter egg: l'ombrello aperto (dritto) sulla talpa, che lo indossa come cappello. Nessuna dipendenza da Unity.
    /// </summary>
    public sealed class UmbrellaLogic
    {
        public const string EggId = "v62_umbrella_hat";
        /// <summary>Secondi di pioggia per riempire l'ombrello rovesciato.</summary>
        public const float FillSeconds = 4f;

        public UmbrellaState State { get; private set; } = UmbrellaState.Playing;
        public UmbrellaPose Pose { get; private set; } = UmbrellaPose.Closed;
        /// <summary>Acqua raccolta, da 0 (vuoto) a 1 (pieno).</summary>
        public float Water { get; private set; }
        public int Wasted { get; private set; }
        public bool EggFound { get; private set; }
        public bool CompletionReported { get; private set; }

        public bool InputLocked => State != UmbrellaState.Playing;
        public bool Full => Water >= 1f;

        public void Reset()
        {
            State = UmbrellaState.Playing;
            Pose = UmbrellaPose.Closed;
            Water = 0f;
            Wasted = 0;
            EggFound = false;
            CompletionReported = false;
        }

        /// <summary>Tocco sull'ombrello chiuso: si apre (dritto).</summary>
        public bool TryOpen()
        {
            if (InputLocked || Pose != UmbrellaPose.Closed) return false;
            Pose = UmbrellaPose.Open;
            return true;
        }

        /// <summary>
        /// Gira l'ombrello aperto (doppio tocco o pulsante). Se era rovesciato e conteneva acqua, girandolo la versa
        /// dov'è: in quel caso restituisce il risultato del versamento in <paramref name="poured"/>.
        /// </summary>
        public bool TryFlip(bool overPlant, out PourResult poured)
        {
            poured = PourResult.Nothing;
            if (InputLocked || Pose == UmbrellaPose.Closed) return false;
            if (Pose == UmbrellaPose.Inverted && Water > 0f) poured = Pour(overPlant);
            if (State != UmbrellaState.Playing) return true;
            Pose = Pose == UmbrellaPose.Open ? UmbrellaPose.Inverted : UmbrellaPose.Open;
            return true;
        }

        /// <summary>
        /// Pioggia sull'ombrello per dt secondi: rovesciato si riempie, dritto ripara. Restituisce true se si sta
        /// riempiendo davvero.
        /// </summary>
        public bool Rain(float dt)
        {
            if (InputLocked || Pose != UmbrellaPose.Inverted || dt <= 0f || Full) return false;
            Water = System.Math.Min(1f, Water + dt / FillSeconds);
            return true;
        }

        /// <summary>Versa l'acqua dell'ombrello rovesciato (inclinando il telefono o toccandolo).</summary>
        public PourResult TryPour(bool overPlant)
        {
            if (InputLocked || Pose != UmbrellaPose.Inverted || Water <= 0f) return PourResult.Nothing;
            return Pour(overPlant);
        }

        PourResult Pour(bool overPlant)
        {
            bool full = Full;
            Water = 0f;
            if (overPlant && full)
            {
                State = UmbrellaState.Blooming;
                return PourResult.Bloom;
            }
            Wasted++;
            return overPlant ? PourResult.NotEnough : PourResult.Wasted;
        }

        /// <summary>L'ombrello aperto e dritto sulla talpa: la talpa lo indossa come cappello. È l'Easter egg.</summary>
        public bool TryHatOnMole()
        {
            if (InputLocked || Pose != UmbrellaPose.Open) return false;
            EggFound = true;
            return true;
        }

        public bool TryCompleteBloom()
        {
            if (State != UmbrellaState.Blooming || CompletionReported) return false;
            State = UmbrellaState.Done;
            CompletionReported = true;
            return true;
        }
    }
}
