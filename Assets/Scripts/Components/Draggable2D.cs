using System;
using UnityEngine;
using Gotcha.Core;

namespace Gotcha.Components
{
    /// <summary>Oggetto trascinabile con il dito. Se rilasciato su uno slot compatibile, ci si aggancia.</summary>
    [RequireComponent(typeof(Collider2D))]
    public class Draggable2D : MonoBehaviour
    {
        public string itemId;
        public bool returnOnMiss = true;
        [Tooltip("Impedisce di trascinare l'oggetto fuori dallo schermo (margine in unità mondo; in alto c'è la barra).")]
        public bool clampToCamera = true;
        public float screenMargin = 0.25f;
        public float topMargin = 1.45f;   // barra in alto + fila dei cuori

        public static Draggable2D Active { get; private set; }

        public event Action<Draggable2D> DragStarted;
        public event Action<Draggable2D, DropSlot> Dropped;

        Camera cam;
        Vector3 startPos;
        Vector2 offset;
        bool dragging;

        void Awake()
        {
            cam = Camera.main;
            startPos = transform.position;
        }

        void OnDisable()
        {
            if (Active == this) Active = null;
            dragging = false;
        }

        void Update()
        {
            if (Input.touchCount >= 2) { CancelDrag(); return; }
            if (!PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out bool ended)) return;

            Vector2 world = PointerInput.ToWorld(cam, sp);

            if (began && Active == null && !PointerInput.IsOverUi() && IsTopAt(world))
            {
                Active = this;
                dragging = true;
                offset = (Vector2)transform.position - world;
                Haptics.Tap();
                DragStarted?.Invoke(this);
            }

            if (!dragging) return;
            if (ended) { Drop(); return; }
            Vector2 target = ClampToCamera(world + offset);
            transform.position = new Vector3(target.x, target.y, transform.position.z);
        }

        Vector2 ClampToCamera(Vector2 p)
        {
            if (!clampToCamera || cam == null || !cam.orthographic) return p;
            float h = cam.orthographicSize, w = h * cam.aspect;
            Vector3 c = cam.transform.position;
            return new Vector2(
                Mathf.Clamp(p.x, c.x - w + screenMargin, c.x + w - screenMargin),
                Mathf.Clamp(p.y, c.y - h + screenMargin, c.y + h - topMargin));
        }

        bool IsTopAt(Vector2 world)
        {
            Draggable2D best = null;
            int bestOrder = int.MinValue;
            foreach (Collider2D h in Physics2D.OverlapPointAll(world))
            {
                Draggable2D d = h.GetComponentInParent<Draggable2D>();
                if (d == null || !d.enabled) continue;
                int o = d.SortOrder;
                if (o > bestOrder) { best = d; bestOrder = o; }
            }
            return best == this;
        }

        int SortOrder
        {
            get { var sr = GetComponentInChildren<SpriteRenderer>(); return sr != null ? sr.sortingOrder : 0; }
        }

        void CancelDrag()
        {
            if (!dragging) return;
            dragging = false;
            if (Active == this) Active = null;
            if (returnOnMiss) transform.position = startPos;
        }

        void Drop()
        {
            dragging = false;
            Active = null;

            DropSlot slot = null;
            foreach (Collider2D h in Physics2D.OverlapPointAll(transform.position))
            {
                DropSlot s = h.GetComponent<DropSlot>();
                if (s != null && s.TryAccept(this)) { slot = s; break; }
            }
            if (slot == null && returnOnMiss) transform.position = startPos;
            if (slot != null) Haptics.Light();   // si aggancia: "clic"
            Dropped?.Invoke(this, slot);
        }
    }
}
