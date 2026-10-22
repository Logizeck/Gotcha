using System;
using UnityEngine;

namespace Gotcha.Levels
{
    public enum LevelResult { Solved, Failed }

    /// <summary>
    /// Base di ogni livello (si mette sul prefab radice). Il livello assembla componenti
    /// riutilizzabili e chiama Win(), Fail() o FailWithEgg() quando ha un esito.
    /// </summary>
    public abstract class LevelController : MonoBehaviour
    {
        public event Action<LevelResult, string> Finished;
        public bool IsFinished { get; private set; }

        public virtual void OnLevelStart() { }

        /// <summary>
        /// Suggerimenti calcolati durante la partita (es. la risposta dell'indovinello uscito a caso). Se non è null
        /// sostituisce i suggerimenti fissi del livello, nello stesso ordine.
        /// </summary>
        public virtual string[] DynamicHints => null;

        protected void Win() { Finish(LevelResult.Solved, null); }
        protected void Fail() { Finish(LevelResult.Failed, null); }

        /// <summary>Frase di scherno su misura per la schermata GOTCHA! (al posto di quella a caso).</summary>
        public string FailTaunt { get; private set; }

        /// <summary>Sconfitta con una presa in giro specifica (es. l'ananas sulla pizza).</summary>
        protected void Fail(string taunt) { FailTaunt = taunt; Fail(); }
        /// <summary>Vittoria che svela anche un Easter egg (es. il robot che si evolve).</summary>
        protected void WinWithEgg(string eggId) { Finish(LevelResult.Solved, eggId); }

        /// <summary>Fallimento "divertente": registra l'Easter egg trovato.</summary>
        protected void FailWithEgg(string eggId) { Finish(LevelResult.Failed, eggId); }

        void Finish(LevelResult result, string eggId)
        {
            if (IsFinished) return;
            IsFinished = true;
            Finished?.Invoke(result, eggId);
        }
    }
}
