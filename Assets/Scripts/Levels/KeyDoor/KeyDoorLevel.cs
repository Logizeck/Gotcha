using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 1 — Pizzica e apri. La chiave è troppo grande per la serratura: va rimpicciolita
    /// col pizzico e poi trascinata nella serratura. Quando la porta si apre il livello è superato.
    /// </summary>
    public class KeyDoorLevel : LevelController
    {
        [Header("Livello 10: chiave dipinta nel quadro (facoltativo)")]
        [SerializeField] SpriteRenderer painting;
        [SerializeField] Sprite paintingWithoutKey;
        [SerializeField] SmokeEmitter sparkle;

        LockedDoor door;
        Draggable2D key;
        bool pulledOut;

        public override void OnLevelStart()
        {
            door = GetComponentInChildren<LockedDoor>(true);
            if (door != null) door.Opened += OnDoorOpened;
            // una chiave appesa storta (es. nel quadro) si raddrizza appena la prendi in mano
            foreach (Draggable2D d in GetComponentsInChildren<Draggable2D>(true))
                if (d.itemId == "key") { key = d; key.DragStarted += Straighten; }
        }

        void OnDestroy()
        {
            if (door != null) door.Opened -= OnDoorOpened;
            if (key != null) key.DragStarted -= Straighten;
        }

        void Straighten(Draggable2D d)
        {
            d.transform.localRotation = Quaternion.identity;
            if (pulledOut || painting == null) return;
            // la chiave era solo "dipinta": al primo tocco si stacca dalla tela e diventa vera
            pulledOut = true;
            foreach (SpriteRenderer r in d.GetComponentsInChildren<SpriteRenderer>(true)) r.enabled = true;
            if (paintingWithoutKey != null) painting.sprite = paintingWithoutKey;
            if (sparkle != null) StartCoroutine(Pop(d.transform.position));
        }

        System.Collections.IEnumerator Pop(Vector3 at)
        {
            sparkle.transform.position = at;
            sparkle.Play();
            yield return new WaitForSeconds(0.2f);
            sparkle.Stop();
        }

        void OnDoorOpened() { Win(); }
    }
}
