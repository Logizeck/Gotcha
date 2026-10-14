using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 1 — Pizzica e apri. La chiave è troppo grande per la serratura: va rimpicciolita
    /// col pizzico e poi trascinata nella serratura. Quando la porta si apre il livello è superato.
    /// </summary>
    public class KeyDoorLevel : LevelController
    {
        LockedDoor door;

        public override void OnLevelStart()
        {
            door = GetComponentInChildren<LockedDoor>(true);
            if (door != null) door.Opened += OnDoorOpened;
        }

        void OnDestroy()
        {
            if (door != null) door.Opened -= OnDoorOpened;
        }

        void OnDoorOpened() { Win(); }
    }
}
