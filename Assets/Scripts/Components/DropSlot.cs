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

        public Draggable2D Occupant { get; private set; }
        public event Action<DropSlot, Draggable2D> Filled;

        public bool TryAccept(Draggable2D d)
        {
            if (Occupant != null) return false;
            if (!string.IsNullOrEmpty(acceptedId) && d.itemId != acceptedId) return false;

            Occupant = d;
            if (snap) d.transform.position = new Vector3(transform.position.x, transform.position.y, d.transform.position.z);
            if (lockOnFill) d.enabled = false;
            Filled?.Invoke(this, d);
            return true;
        }
    }
}
