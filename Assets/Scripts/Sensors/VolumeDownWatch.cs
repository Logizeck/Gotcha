using UnityEngine;

namespace Gotcha.Sensors
{
    /// <summary>
    /// Regola comune ai livelli in cui si abbassa il volume: il livello si supera solo se, DOPO l'inizio del livello,
    /// il giocatore ha davvero premuto "volume -" almeno una volta e il volume è (o arriva) al minimo.
    /// Così un telefono già a volume zero non risolve il livello da solo.
    /// Se il tasto non fosse intercettabile su qualche telefono, vale anche un calo del volume osservato.
    /// </summary>
    public class VolumeDownWatch
    {
        int startPresses;
        float lastVolume;
        bool pressed;

        public bool PressedSinceStart => pressed;

        public void Begin()
        {
            SensorHub hub = SensorHub.Instance;
            startPresses = hub != null ? hub.VolumeDownPresses : 0;
            lastVolume = hub != null ? hub.SystemVolume01 : 1f;
            pressed = false;
        }

        /// <summary>Da chiamare ogni frame. Restituisce true quando la condizione "volume abbassato a zero col tasto" è soddisfatta.</summary>
        public bool Tick(float silentBelow)
        {
            SensorHub hub = SensorHub.Instance;
            if (hub == null) return false;
            float v = hub.SystemVolume01;
            if (hub.VolumeDownPresses > startPresses || v < lastVolume - 0.001f) pressed = true;
            lastVolume = v;
            return pressed && v <= silentBelow;
        }
    }
}
