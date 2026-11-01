using System;
using System.Collections.Generic;

namespace Gotcha.Progress
{
    /// <summary>
    /// Progressi legati all'identificativo dei livelli (dalla 0.76): quando l'ordine cambia (roadmap 1-300) o arrivano
    /// livelli nuovi in mezzo, nessun livello superato viene perso. Regole:
    ///  - un livello è aperto se è già stato superato oppure se sta prima o sulla "frontiera";
    ///  - la frontiera è il primo livello, nell'ordine attuale, non ancora superato ("Continua" porta lì);
    ///  - un livello nuovo inserito presto diventa la frontiera anche per chi è già avanti: lo gioca, poi la frontiera
    ///    salta al successivo non superato.
    /// Senza Unity: testata in ProgressLogicTests.
    /// </summary>
    public static class ProgressLogic
    {
        /// <summary>
        /// Vecchio salvataggio (fino alla 0.75): "highestUnlocked" era la posizione più alta aperta, quindi i livelli
        /// superati erano quelli nelle posizioni 0..highestUnlocked-1 del vecchio ordine.
        /// </summary>
        public static List<string> MigrateFromIndex(int highestUnlocked, IList<string> oldOrder)
        {
            var done = new List<string>();
            if (oldOrder == null) return done;
            int n = Math.Min(Math.Max(0, highestUnlocked), oldOrder.Count);
            for (int i = 0; i < n; i++)
                if (!string.IsNullOrEmpty(oldOrder[i]) && !done.Contains(oldOrder[i])) done.Add(oldOrder[i]);
            return done;
        }

        /// <summary>Primo livello non superato (se sono tutti superati, l'ultimo).</summary>
        public static int Frontier(IList<string> order, Func<string, bool> completed)
        {
            if (order == null || order.Count == 0) return 0;
            for (int i = 0; i < order.Count; i++)
                if (!completed(order[i])) return i;
            return order.Count - 1;
        }

        public static bool IsOpen(int index, IList<string> order, Func<string, bool> completed)
        {
            if (order == null || index < 0 || index >= order.Count) return false;
            return index <= Frontier(order, completed) || completed(order[index]);
        }

        /// <summary>Elenco salvato "id1|id2|..." → insieme.</summary>
        public static HashSet<string> Parse(string saved)
        {
            var set = new HashSet<string>(StringComparer.Ordinal);
            if (string.IsNullOrEmpty(saved)) return set;
            foreach (string s in saved.Split('|'))
                if (s.Length > 0) set.Add(s);
            return set;
        }

        public static string Join(IEnumerable<string> ids) => string.Join("|", ids);
    }
}
