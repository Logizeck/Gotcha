using System.Collections.Generic;
using UnityEngine;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 8 — tienili premuti insieme. Due pulsanti collegati a un timer: il conto alla rovescia da 5 secondi
    /// parte solo mentre entrambi sono premuti contemporaneamente (due dita). Se uno viene rilasciato prima della fine,
    /// il timer torna a 5. Nell'Editor il secondo dito si simula tenendo premuta la barra spaziatrice.
    /// </summary>
    public class HoldLevel : LevelController
    {
        [SerializeField] Collider2D leftButton;
        [SerializeField] Collider2D rightButton;
        [SerializeField] Transform leftVisual;
        [SerializeField] Transform rightVisual;
        [SerializeField] TextMesh display;
        [SerializeField] LineRenderer[] wires;
        [SerializeField] float seconds = 5f;

        static readonly Color WireIdle = new Color(0.75f, 0.62f, 0.15f);
        static readonly Color WireLive = new Color(1f, 0.95f, 0.45f);
        static readonly Color LedRed = new Color(1f, 0.2f, 0.2f);
        static readonly Color LedGreen = new Color(0.3f, 1f, 0.45f);

        Camera cam;
        float remaining;
        Vector3 leftScale, rightScale;
        readonly List<Vector2> touches = new List<Vector2>();

        public override void OnLevelStart()
        {
            cam = Camera.main;
            remaining = seconds;
            if (leftVisual != null) leftScale = leftVisual.localScale;
            if (rightVisual != null) rightScale = rightVisual.localScale;
            Show();
        }

        void Update()
        {
            if (IsFinished) return;
            touches.Clear();
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch t = Input.GetTouch(i);
                if (t.phase != TouchPhase.Ended && t.phase != TouchPhase.Canceled) touches.Add(PointerInput.ToWorld(cam, t.position));
            }
            if (Input.touchCount == 0 && Input.GetMouseButton(0)) touches.Add(PointerInput.ToWorld(cam, Input.mousePosition));

            bool left = Pressed(leftButton), right = Pressed(rightButton);
#if UNITY_EDITOR
            if (Input.GetKey(KeyCode.Space)) { if (left) right = true; else if (right) left = true; }
#endif
            Squash(leftVisual, leftScale, left);
            Squash(rightVisual, rightScale, right);

            bool both = left && right;
            if (both) remaining -= Time.deltaTime;
            else remaining = seconds;   // rilasciati prima della fine: si ricomincia
            Show(both);

            if (remaining <= 0f)
            {
                remaining = 0f;
                if (display != null) { display.text = "OK!"; display.color = LedGreen; }
                Win();
            }
        }

        bool Pressed(Collider2D button)
        {
            if (button == null) return false;
            foreach (Vector2 p in touches) if (button.OverlapPoint(p)) return true;
            return false;
        }

        static void Squash(Transform t, Vector3 baseScale, bool pressed)
        {
            if (t == null) return;
            Vector3 target = pressed ? new Vector3(baseScale.x, baseScale.y * 0.8f, baseScale.z) : baseScale;
            t.localScale = Vector3.Lerp(t.localScale, target, 25f * Time.deltaTime);
        }

        void Show(bool live = false)
        {
            if (display != null)
            {
                display.text = remaining.ToString("0.0");
                display.color = LedRed;
            }
            if (wires != null)
                foreach (LineRenderer w in wires)
                    if (w != null) w.startColor = w.endColor = live ? WireLive : WireIdle;
        }
    }
}
