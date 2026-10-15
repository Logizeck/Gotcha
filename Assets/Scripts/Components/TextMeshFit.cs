using UnityEngine;

namespace Gotcha.Components
{
    /// <summary>Rimpicciolisce un'etichetta (TextMesh) quanto basta perché non superi la larghezza indicata.</summary>
    [RequireComponent(typeof(TextMesh))]
    public class TextMeshFit : MonoBehaviour
    {
        public float maxWidth = 1f;

        TextMesh text;
        MeshRenderer mr;
        string fitted;
        Vector3 baseScale;

        void Awake()
        {
            text = GetComponent<TextMesh>();
            mr = GetComponent<MeshRenderer>();
            baseScale = transform.localScale;
        }

        void LateUpdate()
        {
            if (text.text == fitted) return;
            transform.localScale = baseScale;
            float w = mr.bounds.size.x / Mathf.Max(Mathf.Abs(transform.parent != null ? transform.parent.lossyScale.x : 1f), 1e-6f);
            if (w <= 0f) return;   // mesh non ancora pronto: si riprova al frame dopo
            fitted = text.text;
            if (w > maxWidth) transform.localScale = baseScale * (maxWidth / w);
        }
    }
}
