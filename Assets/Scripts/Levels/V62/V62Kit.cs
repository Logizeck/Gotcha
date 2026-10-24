using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Base dei livelli 0.62 (251-255, "vecchi oggetti, azioni nuove"). Riusa gli aiuti dello speciale Halloween
    /// (HalloweenKit, HwDragger, HwToast, HwSay) e aggiunge l'Easter egg "a metà livello": il segreto si registra
    /// subito (così non si perde se il giocatore esce) e si mostra con una fascia; il livello poi continua e, alla
    /// vittoria, lo riporta con WinWithEgg (per la schermata finale e il video da condividere).
    /// La logica di ogni livello è in Gotcha.Levels.V62 (cartella Logic, senza Unity): qui solo input e presentazione.
    /// </summary>
    public abstract class V62Level : LevelController
    {
        [SerializeField] protected GameObject eggBanner;
        [SerializeField] protected TextMesh eggName;
        [SerializeField] protected SmokeEmitter[] confetti;

        float eggHideAt = -1f;

        /// <summary>Da chiamare in OnLevelStart: nasconde fascia e coriandoli.</summary>
        protected void ResetV62()
        {
            eggHideAt = -1f;
            HalloweenKit.Show(eggBanner, false);
            HalloweenKit.Confetti(confetti, false);
        }

        /// <summary>Da chiamare in Update: nasconde la fascia dell'Easter egg allo scadere.</summary>
        protected void TickV62()
        {
            if (eggHideAt > 0f && Time.time >= eggHideAt)
            {
                eggHideAt = -1f;
                HalloweenKit.Show(eggBanner, false);
            }
        }

        /// <summary>Easter egg trovato: lo salva subito e mostra la fascia col suo nome (il livello continua).</summary>
        protected void RevealEgg(string eggId)
        {
            SaveData.AddEasterEgg(eggId);
            if (eggName != null) eggName.text = EasterEggs.Name(eggId).ToUpperInvariant() + "!";
            HalloweenKit.Show(eggBanner, true);
            if (eggBanner != null) StartCoroutine(HalloweenKit.Pop(eggBanner.transform, 1.12f, 0.3f));
            eggHideAt = Time.time + 2.6f;
            Sfx.Play(this, "ding", 0.8f);
            Haptics.Success();
        }

        /// <summary>Vittoria standard (con l'Easter egg, se trovato in questa partita).</summary>
        protected void Victory(bool eggFound, string eggId)
        {
            if (eggFound) WinWithEgg(eggId);
            else Win();
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
