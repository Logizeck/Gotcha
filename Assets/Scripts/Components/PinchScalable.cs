using System;
using UnityEngine;
using Gotcha.Core;

namespace Gotcha.Components
{
    /// <summary>Ridimensiona l'oggetto con il pizzico a due dita (rotella del mouse nell'Editor).</summary>
    public class PinchScalable : MonoBehaviour
    {
        public float minScale = 0.3f;
        public float maxScale = 1.5f;

        public event Action<float> ScaleChanged;
        public float Scale => transform.localScale.x;

        float prevDistance;

        void Update()
        {
            if (PointerInput.TryGetPinch(out float d))
            {
                if (prevDistance > 1f) SetScale(Scale * d / prevDistance);
                prevDistance = d;
                return;
            }
            prevDistance = 0f;
#if UNITY_EDITOR
            float wheel = Input.mouseScrollDelta.y;
            if (Mathf.Abs(wheel) > 0.01f) SetScale(Scale * (1f + wheel * 0.05f));
#endif
        }

        void SetScale(float s)
        {
            s = Mathf.Clamp(s, minScale, maxScale);
            transform.localScale = Vector3.one * s;
            ScaleChanged?.Invoke(s);
        }
    }
}
