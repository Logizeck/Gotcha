using UnityEngine;

namespace Gotcha.Components
{
    /// <summary>
    /// Sfera cromata che riflette ciò che ha intorno. Una piccola camera segue la sfera e riprende la zona
    /// circostante (pareti comprese) in una RenderTexture; lo shader Gotcha/ChromeBall la deforma come
    /// su uno specchio sferico e la mescola alla base argentata. La sfera deve stare su un layer suo
    /// (es. TransparentFX) per non riflettere se stessa.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class ChromeBall : MonoBehaviour
    {
        public float captureRadius = 1.3f;
        public int resolution = 128;
        public Color background = new Color(0.04f, 0.055f, 0.11f);

        static readonly int ReflectionId = Shader.PropertyToID("_ReflectionTex");

        SpriteRenderer sr;
        MaterialPropertyBlock block;
        Camera reflectionCam;
        RenderTexture rt;

        void OnEnable()
        {
            sr = GetComponent<SpriteRenderer>();
            block = new MaterialPropertyBlock();
            rt = new RenderTexture(resolution, resolution, 16) { name = "BallReflection" };

            var go = new GameObject("BallReflectionCamera") { hideFlags = HideFlags.DontSave };
            reflectionCam = go.AddComponent<Camera>();
            reflectionCam.orthographic = true;
            reflectionCam.orthographicSize = captureRadius;
            reflectionCam.clearFlags = CameraClearFlags.SolidColor;
            reflectionCam.backgroundColor = background;
            int mask = ~(1 << 5);                                   // niente UI
            if (gameObject.layer != 0) mask &= ~(1 << gameObject.layer);   // niente sfera
            reflectionCam.cullingMask = mask;
            reflectionCam.depth = -10;                              // prima della camera principale
            reflectionCam.targetTexture = rt;
            Follow();
        }

        void OnDisable()
        {
            if (reflectionCam != null) Destroy(reflectionCam.gameObject);
            if (rt != null) { rt.Release(); Destroy(rt); }
            reflectionCam = null;
            rt = null;
        }

        void LateUpdate()
        {
            Follow();
            sr.GetPropertyBlock(block);
            block.SetTexture(ReflectionId, rt);
            sr.SetPropertyBlock(block);
        }

        void Follow()
        {
            if (reflectionCam == null) return;
            Vector3 p = transform.position;
            reflectionCam.transform.position = new Vector3(p.x, p.y, p.z - 10f);
        }
    }
}
