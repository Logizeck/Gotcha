using UnityEngine;

namespace Gotcha.Components
{
    /// <summary>
    /// Adatta la camera ortografica in modo che un'area minima (larghezza e altezza in unità mondo)
    /// sia sempre visibile, su qualsiasi proporzione dello schermo (16:9, 20:9, tablet...).
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class CameraFit : MonoBehaviour
    {
        public float minWidth = 5.8f;
        public float minHeight = 10f;

        Camera cam;

        void Awake() { cam = GetComponent<Camera>(); }

        void LateUpdate()
        {
            float aspect = cam.aspect > 0f ? cam.aspect : 9f / 16f;
            cam.orthographicSize = Mathf.Max(minHeight * 0.5f, minWidth * 0.5f / aspect);
        }
    }
}
