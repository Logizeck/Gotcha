namespace Gotcha.Levels.V64
{
    /// <summary>Fasi del livello "The Cowardly Lion".</summary>
    public enum CowardlyLionStage
    {
        Playing,    // si prova di tutto
        Revealing,  // la criniera è venuta via: il gattino miagola, si spaventa da solo e scappa (input bloccato)
        Done,       // vittoria già data
    }

    /// <summary>Dove sta la criniera (un oggetto a sé, trascinabile: è un costume).</summary>
    public enum CowardlyLionMane
    {
        On,         // al suo posto sulla testa del "leone"
        Held,       // il dito la sta tirando via
        Off,        // tolta (oltre la soglia e lasciata): il gattino è scoperto
    }

    /// <summary>Il ruggito del leone toccato.</summary>
    public enum CowardlyLionRoar
    {
        None,       // input bloccato o criniera già tolta
        Roar,       // ruggisce più forte della volta prima
        Capped,     // ruggisce al massimo (non cresce più)
    }

    /// <summary>Il bottone ROAR premuto: il leone risponde ancora più feroce.</summary>
    public enum CowardlyLionButton
    {
        None,
        RoarsBack,  // "ROOOOAAAAR!!"
        Taunt,      // "Is that all you've got?"
    }

    /// <summary>La criniera lasciata andare.</summary>
    public enum CowardlyLionRelease
    {
        None,       // input bloccato o criniera non tenuta
        Wobble,     // tocco leggero (quasi nessuno spostamento): oscilla, indizio
        SnapBack,   // tirata poco: torna su da sola (reversibile)
        Off,        // tirata oltre la soglia: resta via -> rivelazione (una volta sola)
    }

    /// <summary>
    /// Logica pura del livello 5 della roadmap "The Cowardly Lion" — "SCARE THE LION." Un enorme leone minaccioso con una
    /// criniera esagerata e i denti aguzzi, e un bottone ROAR.
    /// Inganno: toccato, il leone ruggisce (sempre più grosso, con un tetto, e poi si calma da solo); il bottone ROAR lo fa
    /// solo ruggire ancora più forte. Nessuna delle due cose porta da qualche parte.
    /// Soluzione: la CRINIERA è un oggetto indipendente. Trascinarla via di almeno <see cref="RevealDistance"/> unità dalla
    /// sua posizione e lasciarla -> resta via e scopre un gattino minuscolo che la indossava come costume; il gattino
    /// miagola, si spaventa del proprio miagolio e scappa -> vittoria (una volta sola).
    /// Scelta documentata: la rivelazione avviene SOLO al rilascio oltre la soglia (passare la soglia durante il
    /// trascinamento non basta: se il dito torna indietro, conta la distanza al rilascio). Tirata poco e lasciata, la
    /// criniera torna su (reversibile). Dito perso (pausa, app in background): la criniera torna su, mai una vittoria.
    /// Non si può perdere (regola del proprietario per i livelli della roadmap). Nessuna dipendenza da Unity.
    /// </summary>
    public sealed class CowardlyLionLogic
    {
        /// <summary>Distanza (unità del mondo) dalla posizione di casa oltre la quale la criniera lasciata resta via.</summary>
        public const float RevealDistance = 1.6f;
        /// <summary>Sotto questa distanza il rilascio è un tocco leggero (la criniera oscilla), non un tentativo.</summary>
        public const float WobbleDistance = 0.2f;
        /// <summary>Livello massimo del ruggito (il leone non cresce oltre).</summary>
        public const int MaxRoar = 4;
        /// <summary>Secondi senza ruggiti dopo i quali il leone comincia a calmarsi.</summary>
        public const float CalmDelay = 2.5f;
        /// <summary>Secondi per scendere di un livello di ruggito mentre si calma.</summary>
        public const float CalmStep = 1.2f;

        public CowardlyLionStage Stage { get; private set; } = CowardlyLionStage.Playing;
        public CowardlyLionMane Mane { get; private set; } = CowardlyLionMane.On;
        /// <summary>Quanto è "gonfio" il leone adesso (0 = calmo, <see cref="MaxRoar"/> = al massimo).</summary>
        public int RoarLevel { get; private set; }
        public int LionTaps { get; private set; }
        public int ButtonPresses { get; private set; }
        public int ManeWobbles { get; private set; }
        public int SnapBacks { get; private set; }
        public int WrongMoves => LionTaps + ButtonPresses + ManeWobbles + SnapBacks;
        /// <summary>Distanza della criniera da casa durante la presa (per lo svelamento progressivo).</summary>
        public float PullDistance { get; private set; }
        /// <summary>Durante la presa, la criniera è già oltre la soglia (se la si lascia lì resta via).</summary>
        public bool PastThreshold => Mane == CowardlyLionMane.Held && PullDistance >= RevealDistance;
        public bool InputLocked => Stage != CowardlyLionStage.Playing;

        float calmTimer;
        bool completed;

        public void Reset()
        {
            Stage = CowardlyLionStage.Playing;
            Mane = CowardlyLionMane.On;
            RoarLevel = 0;
            LionTaps = ButtonPresses = ManeWobbles = SnapBacks = 0;
            PullDistance = 0f;
            calmTimer = 0f;
            completed = false;
        }

        /// <summary>Quanto del gattino è scoperto (0..1) per una data distanza della criniera da casa.</summary>
        public static float RevealFraction(float distance)
        {
            float f = distance / RevealDistance;
            return f < 0f ? 0f : f > 1f ? 1f : f;
        }

        /// <summary>Il tempo passa: senza ruggiti il leone si sgonfia un livello alla volta (l'escalation è reversibile).</summary>
        public void Tick(float dt)
        {
            if (InputLocked || RoarLevel <= 0 || dt <= 0f) return;
            calmTimer += dt;
            while (RoarLevel > 0 && calmTimer >= CalmDelay)
            {
                RoarLevel--;
                calmTimer -= CalmStep;
            }
        }

        int Grow(int amount)
        {
            int before = RoarLevel;
            RoarLevel = RoarLevel + amount > MaxRoar ? MaxRoar : RoarLevel + amount;
            calmTimer = 0f;
            return RoarLevel - before;
        }

        /// <summary>Tocco sul leone (corpo o muso): ruggisce più forte, fino al tetto.</summary>
        public CowardlyLionRoar TapLion()
        {
            if (InputLocked || Mane != CowardlyLionMane.On) return CowardlyLionRoar.None;
            LionTaps++;
            return Grow(1) > 0 ? CowardlyLionRoar.Roar : CowardlyLionRoar.Capped;
        }

        /// <summary>Il bottone ROAR: il leone risponde ancora più forte (due livelli, col tetto). Alterna le due battute.</summary>
        public CowardlyLionButton PressRoar()
        {
            if (InputLocked || Mane != CowardlyLionMane.On) return CowardlyLionButton.None;
            ButtonPresses++;
            Grow(2);
            return ButtonPresses % 2 == 1 ? CowardlyLionButton.RoarsBack : CowardlyLionButton.Taunt;
        }

        /// <summary>Il dito afferra la criniera (mentre la si tiene, tocchi e bottone non fanno niente).</summary>
        public bool GrabMane()
        {
            if (InputLocked || Mane != CowardlyLionMane.On) return false;
            Mane = CowardlyLionMane.Held;
            PullDistance = 0f;
            return true;
        }

        /// <summary>La criniera tenuta si sposta: distanza (unità del mondo) dalla sua posizione di casa.</summary>
        public void DragMane(float distance)
        {
            if (InputLocked || Mane != CowardlyLionMane.Held) return;
            PullDistance = distance < 0f ? 0f : distance;
        }

        /// <summary>La criniera lasciata a <paramref name="distance"/> unità da casa.</summary>
        public CowardlyLionRelease ReleaseMane(float distance)
        {
            if (InputLocked || Mane != CowardlyLionMane.Held) return CowardlyLionRelease.None;
            PullDistance = 0f;
            if (distance >= RevealDistance)
            {
                Mane = CowardlyLionMane.Off;
                Stage = CowardlyLionStage.Revealing;
                return CowardlyLionRelease.Off;
            }
            Mane = CowardlyLionMane.On;
            if (distance < WobbleDistance)
            {
                ManeWobbles++;
                return CowardlyLionRelease.Wobble;
            }
            SnapBacks++;
            return CowardlyLionRelease.SnapBack;
        }

        /// <summary>Il dito sparisce a metà presa (pausa, app in background, livello spento): la criniera torna su.</summary>
        public bool CancelMane()
        {
            if (Mane != CowardlyLionMane.Held) return false;
            Mane = CowardlyLionMane.On;
            PullDistance = 0f;
            return true;
        }

        /// <summary>Fine della scenetta del gattino: true una volta sola (allora il livello chiama Win).</summary>
        public bool TryComplete()
        {
            if (Stage != CowardlyLionStage.Revealing || completed) return false;
            completed = true;
            Stage = CowardlyLionStage.Done;
            return true;
        }
    }
}
