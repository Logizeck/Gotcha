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

        protected void Win() { Finish(LevelResult.Solved, null); }
        protected void Fail() { Finish(LevelResult.Failed, null); }
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
