using UnityEngine;

namespace Gotcha.Components
{
    /// <summary>
    /// Tiene un'etichetta (TextMesh) dentro il suo spazio: la rimpicciolisce quanto basta perché non superi la
    /// larghezza e l'altezza indicate (in coordinate del genitore, 0 = nessun limite) e, in ogni caso, perché non
    /// esca dallo schermo. Si ricalcola da sola ogni volta che il testo cambia.
    /// </summary>
    [RequireComponent(typeof(TextMesh))]
    public class TextMeshFit : MonoBehaviour
    {
        public float maxWidth;
        public float maxHeight;
        [Tooltip("Mai più larga dello schermo (meno un piccolo margine).")]
        public bool keepOnScreen = true;

        TextMesh text;
        MeshRenderer mr;
        string fitted;
        int pending;
        Vector3 baseScale;
        Camera cam;

        void Awake()
        {
            text = GetComponent<TextMesh>();
            mr = GetComponent<MeshRenderer>();
            baseScale = transform.localScale;
            cam = Camera.main;
        }

        void LateUpdate()
        {
            if (text.text != fitted) { fitted = text.text; pending = 3; }   // il mesh del testo si aggiorna con un po' di ritardo
            if (pending <= 0) return;
            pending--;
            transform.localScale = baseScale;
            Vector3 size = mr.bounds.size;
            if (size.x <= 0f) return;
            Vector3 ps = transform.parent != null ? transform.parent.lossyScale : Vector3.one;
            float sx = Mathf.Max(Mathf.Abs(ps.x), 1e-6f), sy = Mathf.Max(Mathf.Abs(ps.y), 1e-6f);
            float k = 1f;
            if (maxWidth > 0f && size.x / sx > maxWidth) k = Mathf.Min(k, maxWidth / (size.x / sx));
            if (maxHeight > 0f && size.y / sy > maxHeight) k = Mathf.Min(k, maxHeight / (size.y / sy));
            if (keepOnScreen && cam != null && cam.orthographic)
            {
                float screen = cam.orthographicSize * 2f * cam.aspect - 0.4f;
                if (size.x > screen) k = Mathf.Min(k, screen / size.x);
            }
            transform.localScale = baseScale * k;
        }
    }
}
