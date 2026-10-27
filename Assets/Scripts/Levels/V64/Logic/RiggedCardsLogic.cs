using System;

namespace Gotcha.Levels.V64
{
    /// <summary>Stati del livello 258 "The rigged three-card game".</summary>
    public enum CardsState
    {
        Shuffling,      // il mazziere mescola (lento e sempre uguale): input bloccato
        Playing,        // tre carte coperte: indovinare dà sempre il due di picche
        Sliding,        // due carte tenute, la terza scivola inclinando il telefono
        DealerFetching, // la carta caduta non scopriva niente: il mazziere la rimette a posto
        StainExposed,   // la carta di mezzo è caduta: la macchia a corona è scoperta, il mazziere la cerca sotto il tavolo
        QueenMade,      // l'inchiostro è passato su una carta: ora è una Regina
        Won,            // "HEY! THAT'S CHEATING!" (animazione finale, input bloccato)
        Done,
    }

    /// <summary>Esito di una carta scelta.</summary>
    public enum CardPick
    {
        Ignored,
        TwoOfSpades,          // una carta qualsiasi: sempre il due di picche
        TwoOfSpadesRemembered,// proprio quella dove si era vista la Regina: ancora il due di picche
        Queen,                // la Regina fatta con l'inchiostro: vittoria
    }

    /// <summary>Cosa è successo durante lo scivolamento.</summary>
    public enum CardsSlide
    {
        None,
        Started,        // la carta libera comincia a scivolare
        Cancelled,      // presa lasciata prima che cadesse: la carta torna al suo posto
        FellNothing,    // è caduta, ma sotto non c'era niente: il mazziere la recupera
        FellStain,      // è caduta la carta sopra la macchia: la corona d'inchiostro è scoperta
    }

    /// <summary>
    /// Logica pura del livello 258 "The rigged three-card game". Il mazziere mostra una Regina e mescola lentamente
    /// (sempre gli stessi scambi: niente caso). Qualsiasi carta scelta è il due di picche; spostare le carte col dito
    /// gliele fa riordinare, scuotere sparpaglia le monete. Soluzione: non c'è nessuna Regina, bisogna fabbricarla.
    /// Si tengono due carte con due dita, si inclina il telefono verso un lato aperto del tavolo: la terza scivola via
    /// e cade; se era quella sopra la macchia d'inchiostro a forma di corona, la macchia resta scoperta (e il mazziere
    /// si distrae a cercare la carta). Si trascina la macchia su una delle due carte rimaste: diventa una Regina.
    /// Si tocca la Regina e si vince. Lo stato raggiunto non si perde. Nessuna dipendenza da Unity.
    /// </summary>
    public sealed class RiggedCardsLogic
    {
        public const int Cards = 3;
        /// <summary>Indice della carta che all'inizio viene mostrata come Regina.</summary>
        public const int ShownQueenCard = 0;
        /// <summary>La macchia a corona è sotto la posizione di mezzo.</summary>
        public const int StainSlot = 1;
        /// <summary>Inclinazione laterale (gravità x filtrata) per far partire la carta e, sotto, per fermarla.</summary>
        public const float SlideStart = 0.3f, SlideStop = 0.15f;
        /// <summary>Oltre questo spostamento la carta non torna più indietro.</summary>
        public const float Commit = 0.9f;
        /// <summary>Metà larghezza del tavolo (in unità mondo, dal centro): oltre, la carta cade.</summary>
        public const float TableHalf = 2.3f;
        public const float SlideAccel = 6f, MaxSpeed = 5f;
        public const float Smoothing = 0.15f;

        /// <summary>Scambi del mescolamento (posizioni), sempre gli stessi.</summary>
        public static readonly int[][] ShuffleSwaps = { new[] { 1, 2 }, new[] { 0, 1 }, new[] { 1, 2 }, new[] { 0, 2 } };
        /// <summary>Riordini veloci del mazziere quando si prova a spostare le carte (a turno).</summary>
        public static readonly int[][] RearrangeSwaps = { new[] { 0, 2 }, new[] { 0, 1 }, new[] { 1, 2 } };
        /// <summary>Carte nelle posizioni prima del mescolamento: la "Regina" (carta 0) al centro.</summary>
        public static readonly int[] StartSlots = { 1, 0, 2 };
        /// <summary>Posizione x delle tre posizioni sul tavolo.</summary>
        public static readonly float[] SlotX = { -1.35f, 0f, 1.35f };

        public CardsState State { get; private set; } = CardsState.Shuffling;
        public int Guesses { get; private set; }
        public int Rearranges { get; private set; }
        public int Spills { get; private set; }
        public int Falls { get; private set; }
        /// <summary>La carta diventata Regina (-1 finché non c'è).</summary>
        public int QueenCard { get; private set; } = -1;
        /// <summary>La carta che sta scivolando (-1 se nessuna).</summary>
        public int FreeCard { get; private set; } = -1;
        /// <summary>Spostamento x della carta che scivola rispetto alla sua posizione.</summary>
        public float Offset { get; private set; }
        public float Speed { get; private set; }
        public float FilteredTilt { get; private set; }
        public bool CompletionReported { get; private set; }
        public bool InputLocked => State == CardsState.Shuffling || State == CardsState.Won || State == CardsState.Done;
        public bool StainVisible => State == CardsState.StainExposed;
        /// <summary>Il mazziere è sotto il tavolo a cercare la carta caduta.</summary>
        public bool DealerDistracted => State == CardsState.StainExposed || State == CardsState.QueenMade;

        readonly int[] cardAt = new int[Cards];
        readonly bool[] gone = new bool[Cards];
        bool tiltPrimed;

        public RiggedCardsLogic() { Reset(); }

        public void Reset()
        {
            State = CardsState.Shuffling;
            Guesses = Rearranges = Spills = Falls = 0;
            QueenCard = FreeCard = -1;
            Offset = Speed = FilteredTilt = 0f;
            CompletionReported = false;
            tiltPrimed = false;
            Array.Copy(StartSlots, cardAt, Cards);
            Array.Clear(gone, 0, Cards);
        }

        public int CardAt(int slot) => cardAt[slot];
        public int SlotOf(int card) { for (int s = 0; s < Cards; s++) if (cardAt[s] == card) return s; return -1; }
        public bool IsGone(int card) => gone[card];

        /// <summary>Esegue uno scambio del mescolamento (posizioni a e b).</summary>
        void Swap(int a, int b) { int t = cardAt[a]; cardAt[a] = cardAt[b]; cardAt[b] = t; }

        /// <summary>Applica tutto il mescolamento e passa al gioco (il runtime anima ShuffleSwaps nello stesso ordine).</summary>
        public bool FinishShuffle()
        {
            if (State != CardsState.Shuffling) return false;
            foreach (int[] s in ShuffleSwaps) Swap(s[0], s[1]);
            State = CardsState.Playing;
            return true;
        }

        /// <summary>Il giocatore sceglie una carta (tocco breve).</summary>
        public CardPick Pick(int card)
        {
            if (card < 0 || card >= Cards || gone[card]) return CardPick.Ignored;
            switch (State)
            {
                case CardsState.Playing:
                case CardsState.StainExposed:
                    Guesses++;
                    return card == ShownQueenCard ? CardPick.TwoOfSpadesRemembered : CardPick.TwoOfSpades;
                case CardsState.QueenMade:
                    if (card == QueenCard) { State = CardsState.Won; return CardPick.Queen; }
                    Guesses++;
                    return CardPick.TwoOfSpades;
                default:
                    return CardPick.Ignored;
            }
        }

        /// <summary>
        /// Si prova a spostare le carte col dito: il mazziere le riordina in fretta (solo durante il gioco normale).
        /// Ritorna lo scambio di posizioni fatto, o null.
        /// </summary>
        public int[] TryRearrange()
        {
            if (State != CardsState.Playing) return null;
            int[] s = RearrangeSwaps[Rearranges % RearrangeSwaps.Length];
            Rearranges++;
            Swap(s[0], s[1]);
            return s;
        }

        /// <summary>Scossone: le monete si sparpagliano (e il mazziere le raccoglie). Nessun cambio di stato.</summary>
        public bool Shake()
        {
            if (InputLocked) return false;
            Spills++;
            return true;
        }

        /// <summary>
        /// Un passo dello scivolamento. held[c] = la carta c è tenuta da un dito; tiltX = gravità x dello schermo.
        /// </summary>
        public CardsSlide Tick(float dt, float tiltX, bool[] held)
        {
            if (float.IsNaN(tiltX)) tiltX = 0f;
            if (!tiltPrimed) { tiltPrimed = true; FilteredTilt = tiltX; }
            else if (dt > 0f) FilteredTilt += (tiltX - FilteredTilt) * (1f - (float)Math.Exp(-dt / Smoothing));
            if (State == CardsState.Playing)
            {
                int free = FreeWithTwoHeld(held);
                if (free < 0 || Math.Abs(FilteredTilt) < SlideStart) return CardsSlide.None;
                FreeCard = free;
                Offset = Speed = 0f;
                State = CardsState.Sliding;
                return CardsSlide.Started;
            }
            if (State != CardsState.Sliding || dt <= 0f) return CardsSlide.None;
            bool committed = Math.Abs(Offset) >= Commit;
            if (!committed && FreeWithTwoHeld(held) != FreeCard)
            {
                // presa lasciata prima che la carta cadesse: torna al suo posto, nient'altro cambia
                State = CardsState.Playing;
                FreeCard = -1;
                Offset = Speed = 0f;
                return CardsSlide.Cancelled;
            }
            float push = Math.Abs(FilteredTilt) >= SlideStop ? FilteredTilt : 0f;
            if (committed) push = Math.Sign(Offset) * Math.Max(Math.Abs(FilteredTilt), SlideStop);   // ormai cade da sola
            if (push == 0f) Speed *= (float)Math.Exp(-dt * 6f);                     // attrito: si ferma dov'è
            else Speed += push * SlideAccel * dt;
            Speed = Math.Max(-MaxSpeed, Math.Min(MaxSpeed, Speed));
            Offset += Speed * dt;
            int slot = SlotOf(FreeCard);
            if (Math.Abs(SlotX[slot] + Offset) < TableHalf) return CardsSlide.None;
            // caduta dal tavolo
            gone[FreeCard] = true;
            Falls++;
            if (slot == StainSlot)
            {
                State = CardsState.StainExposed;
                return CardsSlide.FellStain;
            }
            State = CardsState.DealerFetching;
            return CardsSlide.FellNothing;
        }

        /// <summary>Il mazziere ha rimesso al suo posto la carta caduta (sotto non c'era niente).</summary>
        public bool ReturnFallenCard()
        {
            if (State != CardsState.DealerFetching) return false;
            gone[FreeCard] = false;
            FreeCard = -1;
            Offset = Speed = 0f;
            State = CardsState.Playing;
            return true;
        }

        /// <summary>La macchia d'inchiostro lasciata su una carta: quella carta diventa la Regina.</summary>
        public bool TransferInk(int card)
        {
            if (State != CardsState.StainExposed || card < 0 || card >= Cards || gone[card]) return false;
            QueenCard = card;
            State = CardsState.QueenMade;
            return true;
        }

        /// <summary>Fine dell'animazione: true una volta sola.</summary>
        public bool TryComplete()
        {
            if (State != CardsState.Won || CompletionReported) return false;
            CompletionReported = true;
            State = CardsState.Done;
            return true;
        }

        /// <summary>La carta libera se esattamente due carte (presenti) sono tenute; altrimenti -1.</summary>
        int FreeWithTwoHeld(bool[] held)
        {
            if (held == null) return -1;
            int count = 0, free = -1;
            for (int c = 0; c < Cards; c++)
            {
                if (gone[c]) return -1;
                if (c < held.Length && held[c]) count++;
                else free = c;
            }
            return count == 2 ? free : -1;
        }
    }
}
