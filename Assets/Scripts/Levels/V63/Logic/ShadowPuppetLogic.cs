namespace Gotcha.Levels.V63
{
    /// <summary>Gli oggetti del livello 258 (ognuno fa un'ombra diversa).</summary>
    public enum PuppetItem { None, Yarn, PizzaA, PizzaB, Feather, Apple, Boot, Paw }

    /// <summary>Le quattro zone del supporto davanti alla lampada.</summary>
    public enum PuppetZone { Head, EarLeft, EarRight, Tail }

    /// <summary>Dove sta il supporto: più vicino alla lampada, più grande l'ombra sul muro.</summary>
    public enum PuppetDistance { Far, Mid, Near }

    /// <summary>Che cosa vede il cane sul muro.</summary>
    public enum Silhouette { Empty, BigCat, Kitten, Rabbit, Pizza, AppleCat, TaillessCat, WeirdTail, Ball, Boot, ModernArt }

    /// <summary>Stati del livello 258.</summary>
    public enum PuppetState
    {
        Composing,  // lampada spenta: si sistemano gli oggetti e la distanza
        Showing,    // lampada accesa: il cane guarda l'ombra (input bloccato)
        DogFlees,   // ombra di gatto gigante: il cane scappa (input bloccato)
        Failed,     // tre ombre sbagliate: il cane si arrabbia
        Done,
    }

    /// <summary>
    /// Logica pura del livello 258 "Shadow puppet": il cane da guardia davanti alla porta ha paura dei gatti GIGANTI.
    /// Sul supporto davanti alla lampada ci sono quattro zone (testa, orecchio sinistro, orecchio destro, coda) e il
    /// supporto scorre su tre posizioni (lontano, a metà, vicino alla lampada). Accendendo la lampada il cane vede
    /// l'ombra: testa tonda (il gomitolo) con due orecchie a triangolo (le fette di pizza) e la coda (la piuma) è un
    /// gatto; vicino alla lampada è un gatto gigante e il cane scappa, altrimenti è un gattino e il cane ride. Ogni
    /// ombra sbagliata (non vuota) avvicina il cane di un passo; alla terza si perde. Nessuna dipendenza da Unity.
    /// </summary>
    public sealed class ShadowPuppetLogic
    {
        public const int Zones = 4;
        public const int MaxStrikes = 3;

        readonly PuppetItem[] zones = new PuppetItem[Zones];

        public PuppetState State { get; private set; } = PuppetState.Composing;
        public PuppetDistance Distance { get; private set; } = PuppetDistance.Far;
        public int Strikes { get; private set; }
        public int Shows { get; private set; }
        public Silhouette LastShow { get; private set; }
        public bool CompletionReported { get; private set; }

        public bool InputLocked => State != PuppetState.Composing;

        public void Reset()
        {
            for (int i = 0; i < Zones; i++) zones[i] = PuppetItem.None;
            State = PuppetState.Composing;
            Distance = PuppetDistance.Far;
            Strikes = 0;
            Shows = 0;
            LastShow = Silhouette.Empty;
            CompletionReported = false;
        }

        public PuppetItem ZoneItem(PuppetZone z) => zones[(int)z];

        /// <summary>In che zona è l'oggetto (-1 se è sul tavolo).</summary>
        public int ZoneOf(PuppetItem item)
        {
            if (item == PuppetItem.None) return -1;
            for (int i = 0; i < Zones; i++) if (zones[i] == item) return i;
            return -1;
        }

        /// <summary>Mette un oggetto in una zona (togliendolo da dov'era); quello che c'era torna sul tavolo.</summary>
        public bool TryPlace(PuppetItem item, PuppetZone zone, out PuppetItem displaced)
        {
            displaced = PuppetItem.None;
            int z = (int)zone;
            if (InputLocked || item == PuppetItem.None || z < 0 || z >= Zones) return false;
            int from = ZoneOf(item);
            if (from == z) return true;
            if (from >= 0) zones[from] = PuppetItem.None;
            displaced = zones[z];
            zones[z] = item;
            return true;
        }

        public bool TryRemove(PuppetItem item)
        {
            if (InputLocked) return false;
            int from = ZoneOf(item);
            if (from < 0) return false;
            zones[from] = PuppetItem.None;
            return true;
        }

        /// <summary>Sposta il supporto (lontano / a metà / vicino alla lampada). Solo a lampada spenta.</summary>
        public bool TrySetDistance(PuppetDistance d)
        {
            if (InputLocked || d < PuppetDistance.Far || d > PuppetDistance.Near) return false;
            Distance = d;
            return true;
        }

        /// <summary>L'interruttore: si accende la lampada e il cane guarda l'ombra.</summary>
        public Silhouette TryShow()
        {
            if (State != PuppetState.Composing) return Silhouette.Empty;
            Shows++;
            LastShow = Classify(zones[0], zones[1], zones[2], zones[3], Distance);
            if (LastShow == Silhouette.BigCat) { State = PuppetState.DogFlees; return LastShow; }
            if (LastShow != Silhouette.Empty) Strikes++;
            State = PuppetState.Showing;
            return LastShow;
        }

        /// <summary>Fine della reazione del cane: si spegne la lampada e si riprova (o, al terzo errore, si perde).</summary>
        public bool TryFinishShow()
        {
            if (State != PuppetState.Showing) return false;
            State = Strikes >= MaxStrikes ? PuppetState.Failed : PuppetState.Composing;
            return true;
        }

        public bool TryCompleteFlee()
        {
            if (State != PuppetState.DogFlees || CompletionReported) return false;
            State = PuppetState.Done;
            CompletionReported = true;
            return true;
        }

        static bool Round(PuppetItem i) => i == PuppetItem.Yarn || i == PuppetItem.Apple;
        static bool Triangle(PuppetItem i) => i == PuppetItem.PizzaA || i == PuppetItem.PizzaB;
        static bool Long(PuppetItem i) => i == PuppetItem.Feather || i == PuppetItem.Paw;

        /// <summary>L'ombra sul muro, dalla più importante alla più generica.</summary>
        public static Silhouette Classify(PuppetItem head, PuppetItem earL, PuppetItem earR, PuppetItem tail, PuppetDistance d)
        {
            PuppetItem[] all = { head, earL, earR, tail };
            int count = 0, pizzas = 0;
            foreach (PuppetItem i in all)
            {
                if (i == PuppetItem.None) continue;
                count++;
                if (Triangle(i)) pizzas++;
                if (i == PuppetItem.Boot) return Silhouette.Boot;
            }
            if (count == 0) return Silhouette.Empty;
            bool catEars = Triangle(earL) && Triangle(earR);
            if (head == PuppetItem.Yarn && catEars)
            {
                if (tail == PuppetItem.Feather) return d == PuppetDistance.Near ? Silhouette.BigCat : Silhouette.Kitten;
                return tail == PuppetItem.None ? Silhouette.TaillessCat : Silhouette.WeirdTail;
            }
            if (Round(head) && Long(earL) && Long(earR)) return Silhouette.Rabbit;
            if (pizzas == count) return Silhouette.Pizza;
            if (head == PuppetItem.Apple && catEars) return Silhouette.AppleCat;
            if (Round(head) && earL == PuppetItem.None && earR == PuppetItem.None) return Silhouette.Ball;
            return Silhouette.ModernArt;
        }

        /// <summary>Cosa dice il cane (in inglese, testo di gioco).</summary>
        public static string Reaction(Silhouette s)
        {
            switch (s)
            {
                case Silhouette.Empty: return "Just a wall. Riveting.";
                case Silhouette.BigCat: return "YIPE! A G-G-GIANT CAT!";
                case Silhouette.Kitten: return "Aww, a kitten! Wanna play?";
                case Silhouette.Rabbit: return "A rabbit? I EAT rabbits.";
                case Silhouette.Pizza: return "That's a pizza. Now I'm hungry.";
                case Silhouette.AppleCat: return "An apple with ears? Nice try.";
                case Silhouette.TaillessCat: return "A cat with no tail? Suspicious.";
                case Silhouette.WeirdTail: return "Cats don't have tails like THAT.";
                case Silhouette.Ball: return "A BALL! ...oh. Just a shadow.";
                case Silhouette.Boot: return "A boot? I chew those for breakfast.";
                default: return "What IS that? Modern art?";
            }
        }

        public const string FailTaunt = "Woof. That means 'no'.";
    }
}
