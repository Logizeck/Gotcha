using System.Collections.Generic;
using System.Text;

namespace Gotcha.Levels.Halloween
{
    /// <summary>
    /// La parola del fumetto dello zombie (livello 249): lettere che si tolgono una alla volta, annulla, ripristina e
    /// verifica esplicita della parola obiettivo (BRAAAINS → BRAIN). Le lettere tolte non si duplicano mai.
    /// </summary>
    public sealed class ZombieWord
    {
        public const string DefaultOriginal = "BRAAAINS";
        public const string DefaultTarget = "BRAIN";

        readonly bool[] present;
        readonly List<int> removed = new List<int>();   // in ordine: l'ultima si ripristina per prima

        public string Original { get; }
        public string Target { get; }
        public int Length => Original.Length;

        public ZombieWord(string original = DefaultOriginal, string target = DefaultTarget)
        {
            Original = original;
            Target = target;
            present = new bool[original.Length];
            Reset();
        }

        public void Reset()
        {
            for (int i = 0; i < present.Length; i++) present[i] = true;
            removed.Clear();
        }

        public bool IsPresent(int index) => index >= 0 && index < present.Length && present[index];
        public int VisibleCount => present.Length - removed.Count;
        public int RemovedCount => removed.Count;

        public string Text
        {
            get
            {
                var sb = new StringBuilder(Original.Length);
                for (int i = 0; i < present.Length; i++) if (present[i]) sb.Append(Original[i]);
                return sb.ToString();
            }
        }

        public bool IsTarget => Text == Target;

        /// <summary>Indici delle lettere visibili, nell'ordine (per disporre le tessere).</summary>
        public List<int> VisibleIndices()
        {
            var list = new List<int>(present.Length);
            for (int i = 0; i < present.Length; i++) if (present[i]) list.Add(i);
            return list;
        }

        /// <summary>Toglie una lettera. False se è già tolta, fuori dai limiti o se è l'ultima rimasta.</summary>
        public bool TryRemove(int index)
        {
            if (!IsPresent(index) || VisibleCount <= 1) return false;
            present[index] = false;
            removed.Add(index);
            return true;
        }

        /// <summary>Rimette l'ultima lettera tolta. Restituisce il suo indice, -1 se non c'è niente da annullare.</summary>
        public int Undo()
        {
            if (removed.Count == 0) return -1;
            int i = removed[removed.Count - 1];
            removed.RemoveAt(removed.Count - 1);
            present[i] = true;
            return i;
        }
    }
}
