using UnityEngine;

namespace Gotcha.Components
{
    /// <summary>
    /// Tiene un'etichetta (TextMesh) dentro il suo spazio: la rimpicciolisce quanto basta perché non superi la
    /// larghezza e l'altezza indicate (in coordinate del genitore, 0 = nessun limite) e, in ogni caso, perché non
    /// esca dallo schermo (tutti e quattro i bordi, tenendo conto di dove si trova, non solo della larghezza).
    /// Dalla 0.78 convive con le animazioni: se un livello cambia la scala dell'etichetta (un "pop") o del suo genitore,
    /// o la sposta, la riduzione viene ricalcolata e applicata SOPRA la scala voluta dall'animazione. Prima il pop
    /// cancellava la riduzione e le scritte di vittoria potevano uscire dallo schermo.
    /// </summary>
    [RequireComponent(typeof(TextMesh))]
    public class TextMeshFit : MonoBehaviour
    {
        public float maxWidth;
        public float maxHeight;
        [Tooltip("Mai fuori dallo schermo (meno un piccolo margine).")]
        public bool keepOnScreen = true;

        const float MarginX = 0.18f, MarginY = 0.12f;

        TextMesh text;
        MeshRenderer mr;
        string fitted;
        int pending;
        Vector3 baseScale, lastWritten, lastParentScale, lastPosition;
        Camera cam;

        void Awake()
        {
            text = GetComponent<TextMesh>();
            mr = GetComponent<MeshRenderer>();
            baseScale = lastWritten = transform.localScale;
            cam = Camera.main;
        }

        void LateUpdate()
        {
            bool dirty = false;
            Vector3 ls = transform.localScale;
            if (ls != lastWritten) { baseScale = ls; dirty = true; }   // scala cambiata da un'animazione del livello
            if (text.text != fitted) { fitted = text.text; pending = 3; }   // il mesh del testo si aggiorna con un po' di ritardo
            if (pending > 0) { pending--; dirty = true; }
            Vector3 ps = transform.parent != null ? transform.parent.lossyScale : Vector3.one;
            if (ps != lastParentScale) { lastParentScale = ps; dirty = true; }
            Vector3 pos = transform.position;
            if ((pos - lastPosition).sqrMagnitude > 1e-6f) { lastPosition = pos; dirty = true; }
            if (!dirty) return;

            transform.localScale = baseScale;
            float k = 1f;
            Bounds b = mr.bounds;
            Vector3 size = b.size;
            if (size.x > 0f && baseScale.x != 0f)
            {
                float sx = Mathf.Max(Mathf.Abs(ps.x), 1e-6f), sy = Mathf.Max(Mathf.Abs(ps.y), 1e-6f);
                if (maxWidth > 0f && size.x / sx > maxWidth) k = Mathf.Min(k, maxWidth / (size.x / sx));
                if (maxHeight > 0f && size.y / sy > maxHeight) k = Mathf.Min(k, maxHeight / (size.y / sy));
                if (keepOnScreen) k = Mathf.Min(k, ScreenFactor(b, pos));
            }
            lastWritten = baseScale * k;
            transform.localScale = lastWritten;
        }

        /// <summary>
        /// Quanto ridurre (attorno al punto d'ancoraggio dell'etichetta) perché il testo stia fra i bordi dello schermo.
        /// </summary>
        float ScreenFactor(Bounds b, Vector3 pivot)
        {
            if (cam == null) cam = Camera.main;
            if (cam == null || !cam.orthographic) return 1f;
            Vector3 c = cam.transform.position;
            float halfH = cam.orthographicSize, halfW = halfH * cam.aspect;
            float k = 1f;
            k = Mathf.Min(k, Axis(b.min.x, b.max.x, pivot.x, c.x - halfW + MarginX, c.x + halfW - MarginX));
            k = Mathf.Min(k, Axis(b.min.y, b.max.y, pivot.y, c.y - halfH + MarginY, c.y + halfH - MarginY));
            return Mathf.Max(0.05f, k);
        }

        static float Axis(float min, float max, float pivot, float lo, float hi)
        {
            float k = 1f;
            // se anche l'ancoraggio è fuori dallo schermo non si può rimediare rimpicciolendo: si limita alla misura
            if (pivot <= lo || pivot >= hi)
            {
                float span = hi - lo, size = max - min;
                return size > span && size > 0f ? span / size : 1f;
            }
            if (min < lo && pivot - min > 1e-5f) k = Mathf.Min(k, (pivot - lo) / (pivot - min));
            if (max > hi && max - pivot > 1e-5f) k = Mathf.Min(k, (hi - pivot) / (max - pivot));
            return k;
        }
    }
}
