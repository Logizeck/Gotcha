using System;
using UnityEngine;
using Gotcha.Core;

namespace Gotcha.Components
{
    /// <summary>Pulsante nel mondo di gioco (non UI): scatta al tocco dentro il suo Collider2D e si abbassa un attimo.</summary>
    [RequireComponent(typeof(Collider2D))]
    public class TapButton : MonoBehaviour
    {
        public string id;
        public Transform visual;            // cosa si abbassa (di solito il figlio con lo sprite)
        public float pressedScaleY = 0.82f;
        public bool interactable = true;

        public event Action<TapButton> Pressed;

        Collider2D area;
        Camera cam;
        Vector3 baseScale;
        float pressedFor;

        void Awake()
        {
            area = GetComponent<Collider2D>();
            cam = Camera.main;
            if (visual == null) visual = transform;
            baseScale = visual.localScale;
        }

        void Update()
        {
            pressedFor -= Time.deltaTime;
            Vector3 target = pressedFor > 0f ? new Vector3(baseScale.x, baseScale.y * pressedScaleY, baseScale.z) : baseScale;
            visual.localScale = Vector3.Lerp(visual.localScale, target, 25f * Time.deltaTime);

            if (!interactable) return;
            if (!PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out _) || !began || PointerInput.IsOverUi()) return;
            if (!area.OverlapPoint(PointerInput.ToWorld(cam, sp))) return;
            pressedFor = 0.15f;
            Pressed?.Invoke(this);
        }
    }
}
