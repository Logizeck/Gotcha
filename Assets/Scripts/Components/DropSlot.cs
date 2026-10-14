using System;
using UnityEngine;

namespace Gotcha.Components
{
    /// <summary>Punto in cui si può rilasciare un Draggable2D (occhio, naso, bocca, serratura...).</summary>
    [RequireComponent(typeof(Collider2D))]
    public class DropSlot : MonoBehaviour
    {
        public string acceptedId;        // vuoto = accetta qualsiasi oggetto
        public bool snap = true;
        public bool lockOnFill = true;   // blocca l'oggetto una volta agganciato
        [Tooltip("Se > 0, rifiuta gli oggetti con scala maggiore (es. chiave troppo grande per la serratura).")]
        public float maxItemScale = 0f;

        public Draggable2D Occupant { get; private set; }
        public event Action<DropSlot, Draggable2D> Filled;
        /// <summary>Oggetto giusto ma troppo grande (vedi maxItemScale).</summary>
        public event Action<DropSlot, Draggable2D> Rejected;

        public bool TryAccept(Draggable2D d)
        {
            if (Occupant != null) return false;
            if (!string.IsNullOrEmpty(acceptedId) && d.itemId != acceptedId) return false;
            if (maxItemScale > 0f && d.transform.localScale.x > maxItemScale)
            {
                Rejected?.Invoke(this, d);
                return false;
            }

            Occupant = d;
            if (snap) d.transform.position = new Vector3(transform.position.x, transform.position.y, d.transform.position.z);
            if (lockOnFill) d.enabled = false;
            Filled?.Invoke(this, d);
            return true;
        }
    }
}
