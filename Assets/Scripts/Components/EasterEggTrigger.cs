using System;
using UnityEngine;

namespace Gotcha.Components
{
    /// <summary>
    /// Zona "segreta": scatta quando l'oggetto trascinato con l'id indicato entra nella sua area.
    /// Il livello decide cosa fare (animazione, poi FailWithEgg).
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class EasterEggTrigger : MonoBehaviour
    {
        public string eggId;
        public string triggerItemId;

        public event Action<string> Triggered;

        Collider2D area;
        bool fired;

        void Awake() { area = GetComponent<Collider2D>(); }

        void Update()
        {
            if (fired) return;
            Draggable2D d = Draggable2D.Active;
            if (d == null || d.itemId != triggerItemId) return;
            if (area.OverlapPoint(d.transform.position))
            {
                fired = true;
                Triggered?.Invoke(eggId);
            }
        }
    }
}
