using UnityEngine;
using UnityEngine.EventSystems;

namespace Gotcha.Core
{
    /// <summary>Unifica touch (dispositivo) e mouse (Editor) in un unico puntatore primario.</summary>
    public static class PointerInput
    {
        public static bool TryGetPrimary(out Vector2 position, out bool began, out bool ended)
        {
            if (Input.touchCount > 0)
            {
                Touch t = Input.GetTouch(0);
                position = t.position;
                began = t.phase == TouchPhase.Began;
                ended = t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled;
                return true;
            }
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButton(0) || Input.GetMouseButtonUp(0))
            {
                position = Input.mousePosition;
                began = Input.GetMouseButtonDown(0);
                ended = Input.GetMouseButtonUp(0);
                return true;
            }
            position = default;
            began = false;
            ended = false;
            return false;
        }

        public static bool TryGetPinch(out float distance)
        {
            if (Input.touchCount >= 2)
            {
                distance = Vector2.Distance(Input.GetTouch(0).position, Input.GetTouch(1).position);
                return true;
            }
            distance = 0f;
            return false;
        }

        public static bool IsOverUi()
        {
            if (EventSystem.current == null) return false;
            if (Input.touchCount > 0) return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
            return EventSystem.current.IsPointerOverGameObject();
        }

        /// <summary>True se almeno un dito (o il mouse) sta premendo dentro il collider.</summary>
        public static bool IsHeld(Camera cam, Collider2D area)
        {
            if (area == null || cam == null) return false;
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch t = Input.GetTouch(i);
                if (t.phase != TouchPhase.Ended && t.phase != TouchPhase.Canceled && area.OverlapPoint(ToWorld(cam, t.position))) return true;
            }
            return Input.touchCount == 0 && Input.GetMouseButton(0) && area.OverlapPoint(ToWorld(cam, Input.mousePosition));
        }

        /// <summary>True nel frame in cui un dito qualsiasi (o il mouse) tocca lo schermo.</summary>
        public static bool AnyTouchBegan()
        {
            for (int i = 0; i < Input.touchCount; i++) if (Input.GetTouch(i).phase == TouchPhase.Began) return true;
            return Input.GetMouseButtonDown(0);
        }

        public static Vector2 ToWorld(Camera cam, Vector2 screen)
        {
            Vector3 p = cam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, Mathf.Abs(cam.transform.position.z)));
            return new Vector2(p.x, p.y);
        }
    }
}
