using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Base dei livelli 0.62/0.63 (251-258). Riusa gli aiuti dello speciale Halloween (HalloweenKit, HwDragger,
    /// HwToast, HwSay) e aggiunge solo la festa finale coi coriandoli. Niente Easter egg: per scelta del proprietario
    /// non ce n'è uno in ogni livello, e questi ne sono senza.
    /// La logica di ogni livello è in Gotcha.Levels.V62 / V63 (cartelle Logic, senza Unity): qui solo input e presentazione.
    /// </summary>
    public abstract class V62Level : LevelController
    {
        [SerializeField] protected SmokeEmitter[] confetti;

        /// <summary>Da chiamare in OnLevelStart: spegne i coriandoli.</summary>
        protected void ResetV62()
        {
            HalloweenKit.Confetti(confetti, false);
        }

        /// <summary>Festa finale: coriandoli per qualche secondo.</summary>
        protected IEnumerator Celebrate(float seconds)
        {
            HalloweenKit.Confetti(confetti, true);
            Sfx.Play(this, "ding", 0.8f);
            Haptics.Success();
            yield return new WaitForSeconds(seconds);
            HalloweenKit.Confetti(confetti, false);
        }
    }
}
