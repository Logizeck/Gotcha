using System;

namespace Gotcha.Levels.V64
{
    /// <summary>Fasi del livello 263.</summary>
    public enum SnackStage
    {
        Stuck,      // la barretta B3 è incastrata sul bordo della spirale: si prova di tutto
        Dropping,   // comprata la A3: cade, colpisce la B3 e finiscono tutte e due nello sportello (input bloccato)
        InFlap,     // le due barrette sono nello sportello: basta prenderle
        Taken,      // prese: festa finale (input bloccato)
        Done,
    }

    /// <summary>Esito di un tasto premuto sulla tastiera del distributore.</summary>
    public enum VendResult
    {
        None,       // tasto non valido o input bloccato
        NeedCoin,   // nessun credito: la scelta resta in memoria, "INSERT COIN"
        SoldOut,    // B3 (la barretta incastrata): "SOLD OUT", moneta restituita
        Error,      // qualunque altro scomparto tranne A3: il prodotto si incastra e torna a posto, moneta restituita
        Vend,       // A3: la soluzione
    }

    /// <summary>Cosa succede infilando la mano nello sportello.</summary>
    public enum FlapReaction { None, ArmStuck, Take }

    /// <summary>
    /// Logica pura del livello 263 "The Stuck Snack" — "Get the snack." Il distributore ha appena incastrato la barretta
    /// B3 sul bordo della spirale; la stessa barretta è anche in A3, proprio sopra. C'è UNA sola moneta.
    /// False piste (nessuna azzera i progressi, la moneta non si perde mai): calci/colpi al distributore, scuotere il
    /// telefono, infilare il braccio nello sportello, ripremere B3 ("SOLD OUT", moneta resa), comprare un altro
    /// scomparto (si incastra anche quello, poi "MACHINE ERROR — REFUNDED", moneta resa), la fessura del resto (vuota).
    /// Soluzione: moneta + A3 (in qualunque ordine): la A3 cade, colpisce la B3 e cadono tutte e due nello sportello;
    /// poi le si prende dallo sportello. Due al prezzo di una. Nessuna dipendenza da Unity.
    /// Scomparti: righe 0..2 = A, B, C (dall'alto); colonne 0..2 = 1, 2, 3.
    /// </summary>
    public sealed class StuckSnackLogic
    {
        public const int Rows = 3, Cols = 3;
        /// <summary>Lo scomparto con la barretta incastrata (B3) e quello giusto, proprio sopra (A3).</summary>
        public const int StuckRow = 1, StuckCol = 2, SolutionRow = 0, SolutionCol = 2;

        public SnackStage Stage { get; private set; } = SnackStage.Stuck;
        /// <summary>La moneta è nel portafoglio (true) oppure è dentro il distributore come credito.</summary>
        public bool CoinInWallet { get; private set; } = true;
        /// <summary>La moneta è stata spesa per la A3 (solo dopo la soluzione).</summary>
        public bool CoinSpent { get; private set; }
        public bool HasCredit => !CoinInWallet && !CoinSpent;
        /// <summary>Scelta fatta senza credito (-1 = nessuna): si compra appena entra la moneta.</summary>
        public int PendingRow { get; private set; } = -1;
        public int PendingCol { get; private set; } = -1;
        public bool HasPending => PendingRow >= 0;

        public int Kicks { get; private set; }
        public int Shakes { get; private set; }
        public int ArmAttempts { get; private set; }
        public int SoldOuts { get; private set; }
        public int Errors { get; private set; }
        public int ChangeTaps { get; private set; }
        public int NoCoinPresses { get; private set; }
        public int Refunds { get; private set; }
        public bool CompletionReported { get; private set; }

        public bool InputLocked => Stage != SnackStage.Stuck && Stage != SnackStage.InFlap;
        public bool Solved => Stage != SnackStage.Stuck;
        /// <summary>Tutte le mosse sbagliate fatte finora (solo statistica: non cambiano niente).</summary>
        public int WrongMoves => Kicks + Shakes + ArmAttempts + SoldOuts + Errors + ChangeTaps;

        public void Reset()
        {
            Stage = SnackStage.Stuck;
            CoinInWallet = true;
            CoinSpent = false;
            PendingRow = PendingCol = -1;
            Kicks = Shakes = ArmAttempts = SoldOuts = Errors = ChangeTaps = NoCoinPresses = Refunds = 0;
            CompletionReported = false;
        }

        public static bool IsStuckSlot(int row, int col) => row == StuckRow && col == StuckCol;
        public static bool IsSolutionSlot(int row, int col) => row == SolutionRow && col == SolutionCol;
        public static bool ValidSlot(int row, int col) => row >= 0 && row < Rows && col >= 0 && col < Cols;
        /// <summary>Nome dello scomparto come sulla tastiera ("A3").</summary>
        public static string SlotName(int row, int col) => ValidSlot(row, col) ? ((char)('A' + row)).ToString() + (col + 1) : "??";

        // ---------- Le false piste ----------

        /// <summary>Un colpo/calcio al distributore: traballa, la barretta oscilla ma resta lì.</summary>
        public int Kick()
        {
            if (Stage != SnackStage.Stuck) return 0;
            return ++Kicks;
        }

        /// <summary>Il telefono scosso: il distributore dondola, l'adesivo lampeggia, bip; la barretta resta lì.</summary>
        public int Shake()
        {
            if (Stage != SnackStage.Stuck) return 0;
            return ++Shakes;
        }

        /// <summary>La fessura del resto: vuota (esce una falena).</summary>
        public int TapChange()
        {
            if (Stage != SnackStage.Stuck) return 0;
            return ++ChangeTaps;
        }

        /// <summary>
        /// La mano nello sportello: prima della soluzione il braccio resta incastrato un attimo (niente cambia);
        /// quando le barrette sono nello sportello, le si prende (una volta sola).
        /// </summary>
        public FlapReaction ReachFlap()
        {
            if (Stage == SnackStage.InFlap) { Stage = SnackStage.Taken; return FlapReaction.Take; }
            if (Stage != SnackStage.Stuck) return FlapReaction.None;
            ArmAttempts++;
            return FlapReaction.ArmStuck;
        }

        // ---------- Moneta e tastiera ----------

        /// <summary>
        /// La moneta entra nella fessura (trascinata o toccata). Se c'era già una scelta in memoria la si compra subito
        /// (l'esito è quello di PressSlot); altrimenti NeedCoin vuol dire "credito 1, scegli". None se non c'è moneta.
        /// </summary>
        public VendResult InsertCoin()
        {
            if (Stage != SnackStage.Stuck || !CoinInWallet || CoinSpent) return VendResult.None;
            CoinInWallet = false;
            if (!HasPending) return VendResult.NeedCoin;
            int r = PendingRow, c = PendingCol;
            PendingRow = PendingCol = -1;
            return Buy(r, c);
        }

        /// <summary>Un tasto dello scomparto (riga, colonna). Senza credito la scelta resta in memoria.</summary>
        public VendResult PressSlot(int row, int col)
        {
            if (Stage != SnackStage.Stuck || !ValidSlot(row, col)) return VendResult.None;
            if (!HasCredit)
            {
                NoCoinPresses++;
                PendingRow = row;
                PendingCol = col;
                return VendResult.NeedCoin;
            }
            PendingRow = PendingCol = -1;
            return Buy(row, col);
        }

        /// <summary>La scelta in memoria scade (la si dimentica: nessun effetto sulla moneta).</summary>
        public void ClearPending() { PendingRow = PendingCol = -1; }

        VendResult Buy(int row, int col)
        {
            if (IsSolutionSlot(row, col))
            {
                CoinSpent = true;
                Stage = SnackStage.Dropping;
                return VendResult.Vend;
            }
            // qualunque altra scelta: la moneta torna nel portafoglio
            CoinInWallet = true;
            Refunds++;
            if (IsStuckSlot(row, col)) { SoldOuts++; return VendResult.SoldOut; }
            Errors++;
            return VendResult.Error;
        }

        // ---------- La soluzione ----------

        /// <summary>La caduta è finita: le due barrette sono nello sportello.</summary>
        public bool FinishDrop()
        {
            if (Stage != SnackStage.Dropping) return false;
            Stage = SnackStage.InFlap;
            return true;
        }

        /// <summary>Fine della festa: vittoria, una volta sola.</summary>
        public bool TryComplete()
        {
            if (Stage != SnackStage.Taken || CompletionReported) return false;
            CompletionReported = true;
            Stage = SnackStage.Done;
            return true;
        }
    }
}
