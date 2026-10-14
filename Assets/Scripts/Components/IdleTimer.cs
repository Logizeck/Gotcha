using System;
using UnityEngine;
using Gotcha.Core;

namespace Gotcha.Components
{
    /// <summary>Scatta se il giocatore non tocca lo schermo per N secondi (ogni tocco azzera il timer).</summary>
    public class IdleTimer : MonoBehaviour
    {
        public float seconds = 7f;
        public bool ignoreUi = true;   // i tocchi su Menu/Hint non contano

        public float Elapsed { get; private set; }
        public event Action Completed;

        bool done;

        void Update()
        {
            if (done) return;
            bool touching = Input.touchCount > 0 || Input.GetMouseButton(0);
            if (touching && !(ignoreUi && PointerInput.IsOverUi())) Elapsed = 0f;
            else Elapsed += Time.deltaTime;

            if (Elapsed >= seconds) { done = true; Completed?.Invoke(); }
        }
    }
}
