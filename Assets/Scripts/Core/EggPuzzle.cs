namespace Gotcha.Core
{
    /// <summary>
    /// Il puzzle degli Easter egg: un'immagine di Toni in palestra divisa in 50 pezzi (5 colonne x 10 righe). Ogni
    /// Easter egg trovato sblocca un pezzo; i pezzi si scoprono in un ordine sparso ma sempre uguale, così l'immagine
    /// si intravede un po' alla volta.
    /// </summary>
    public static class EggPuzzle
    {
        public const int Cols = 5, Rows = 10, Pieces = Cols * Rows;

        static int[] rank;   // rank[pezzo] = quanti Easter egg servono prima di vederlo (0..49)

        static int[] Rank
        {
            get
            {
                if (rank != null) return rank;
                var order = new int[Pieces];
                for (int i = 0; i < Pieces; i++) order[i] = i;
                var rnd = new System.Random(2026);
                for (int i = Pieces - 1; i > 0; i--) { int j = rnd.Next(i + 1); (order[i], order[j]) = (order[j], order[i]); }
                rank = new int[Pieces];
                for (int i = 0; i < Pieces; i++) rank[order[i]] = i;
                return rank;
            }
        }

        /// <summary>Pezzi sbloccati: uno per ogni Easter egg trovato (al massimo 50).</summary>
        public static int Unlocked => System.Math.Min(SaveData.EasterEggCount, Pieces);

        /// <summary>Il pezzo (colonna c, riga r dall'alto) è già scoperto?</summary>
        public static bool IsUnlocked(int c, int r) => Rank[r * Cols + c] < Unlocked;

        public static bool Complete => Unlocked >= Pieces;
    }
}
