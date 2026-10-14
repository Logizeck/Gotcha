using System;
using UnityEngine;

namespace Gotcha.Components
{
    /// <summary>Area bersaglio: scatta quando un oggetto col tag richiesto resta dentro per holdSeconds.</summary>
    [RequireComponent(typeof(Collider2D))]
    public class GoalZone : MonoBehaviour
    {
        public string requiredTag = "Player";
        public float holdSeconds = 0.3f;

        public event Action Reached;

        float held;
        bool done;

        void Reset() { GetComponent<Collider2D>().isTrigger = true; }

        void OnTriggerStay2D(Collider2D other)
        {
            if (done || !other.CompareTag(requiredTag)) return;
            held += Time.deltaTime;
            if (held >= holdSeconds) { done = true; Reached?.Invoke(); }
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag(requiredTag)) held = 0f;
        }
    }
}
