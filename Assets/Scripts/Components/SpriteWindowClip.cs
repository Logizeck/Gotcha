using UnityEngine;

namespace Gotcha.Components
{
    /// <summary>
    /// Disegna uno sprite ritagliato su una "finestra" rettangolare (la finestra dello scanner a raggi X, il bordo
    /// della buca della talpa...) SENZA SpriteMask: un quad con le UV tagliate, ricalcolato in LateUpdate. Non usa lo
    /// stencil, quindi si vede uguale su tutti i telefoni. La finestra è un rettangolo nelle coordinate locali di
    /// <see cref="window"/> (centro <see cref="windowOffset"/>, misure <see cref="windowSize"/>); lo sprite deve avere
    /// la mesh "Full Rect" (come tutti i disegni del gioco). Nessuna allocazione per frame.
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class SpriteWindowClip : MonoBehaviour
    {
        [SerializeField] Sprite sprite;
        [SerializeField] Color color = Color.white;
        [SerializeField] int sortingOrder;
        [SerializeField] Transform window;
        [SerializeField] Vector2 windowOffset;
        [SerializeField] Vector2 windowSize = Vector2.one;

        static Material fallbackMaterial;
        static readonly int MainTexId = Shader.PropertyToID("_MainTex");

        MeshRenderer mr;
        Mesh mesh;
        readonly Vector3[] verts = new Vector3[4];
        readonly Vector2[] uvs = new Vector2[4];
        readonly Color[] colors = new Color[4];
        static readonly int[] Tris = { 0, 1, 2, 0, 2, 3 };
        Vector2 uvMin, uvMax;
        Bounds spriteBounds;

        public Color Color
        {
            get => color;
            set { color = value; for (int i = 0; i < 4; i++) colors[i] = value; if (mesh != null) mesh.colors = colors; }
        }

        /// <summary>Impostazione dal builder (i campi finiscono serializzati nel prefab).</summary>
        public void Configure(Sprite sprite, Color color, int sortingOrder, Transform window, Vector2 windowOffset, Vector2 windowSize)
        {
            this.sprite = sprite;
            this.color = color;
            this.sortingOrder = sortingOrder;
            this.window = window;
            this.windowOffset = windowOffset;
            this.windowSize = windowSize;
            GetComponent<MeshRenderer>().sortingOrder = sortingOrder;
        }

        void Awake() { Setup(); }

        void Setup()
        {
            if (mesh != null) return;
            mr = GetComponent<MeshRenderer>();
            if (mr.sharedMaterial == null)
            {
                if (fallbackMaterial == null) fallbackMaterial = new Material(Shader.Find("Sprites/Default"));
                mr.sharedMaterial = fallbackMaterial;
            }
            mr.sortingOrder = sortingOrder;
            mesh = new Mesh { name = "ClippedSprite" };
            mesh.MarkDynamic();
            for (int i = 0; i < 4; i++) colors[i] = color;
            mesh.vertices = verts;
            mesh.uv = uvs;
            mesh.colors = colors;
            mesh.triangles = Tris;
            GetComponent<MeshFilter>().sharedMesh = mesh;
            if (sprite != null)
            {
                var block = new MaterialPropertyBlock();
                block.SetTexture(MainTexId, sprite.texture);
                mr.SetPropertyBlock(block);
                Rect tr = sprite.textureRect;
                float w = sprite.texture.width, h = sprite.texture.height;
                uvMin = new Vector2(tr.xMin / w, tr.yMin / h);
                uvMax = new Vector2(tr.xMax / w, tr.yMax / h);
                spriteBounds = sprite.bounds;
            }
            Refresh();
        }

        void OnEnable() { if (mesh != null) Refresh(); }

        void LateUpdate() { Refresh(); }

        /// <summary>Ricalcola il ritaglio (la finestra o lo sprite si sono mossi).</summary>
        public void Refresh()
        {
            if (mesh == null) { Setup(); return; }
            if (sprite == null || window == null) { mr.enabled = false; return; }
            // gli angoli della finestra nelle coordinate locali di questo oggetto (rettangolo che li contiene)
            Vector2 h = windowSize * 0.5f;
            Vector2 min = new Vector2(float.MaxValue, float.MaxValue), max = new Vector2(float.MinValue, float.MinValue);
            for (int i = 0; i < 4; i++)
            {
                Vector2 c = windowOffset + new Vector2((i & 1) == 0 ? -h.x : h.x, (i & 2) == 0 ? -h.y : h.y);
                Vector2 l = transform.InverseTransformPoint(window.TransformPoint(c));
                min = Vector2.Min(min, l);
                max = Vector2.Max(max, l);
            }
            Vector2 bMin = spriteBounds.min, bMax = spriteBounds.max;
            float x0 = Mathf.Max(min.x, bMin.x), x1 = Mathf.Min(max.x, bMax.x);
            float y0 = Mathf.Max(min.y, bMin.y), y1 = Mathf.Min(max.y, bMax.y);
            if (x1 <= x0 || y1 <= y0) { mr.enabled = false; return; }
            mr.enabled = true;
            verts[0] = new Vector3(x0, y0, 0f);
            verts[1] = new Vector3(x0, y1, 0f);
            verts[2] = new Vector3(x1, y1, 0f);
            verts[3] = new Vector3(x1, y0, 0f);
            for (int i = 0; i < 4; i++)
            {
                float u = Mathf.InverseLerp(bMin.x, bMax.x, verts[i].x), v = Mathf.InverseLerp(bMin.y, bMax.y, verts[i].y);
                uvs[i] = new Vector2(Mathf.Lerp(uvMin.x, uvMax.x, u), Mathf.Lerp(uvMin.y, uvMax.y, v));
            }
            mesh.vertices = verts;
            mesh.uv = uvs;
            mesh.RecalculateBounds();
        }

        void OnDestroy() { if (mesh != null) Destroy(mesh); }
    }
}
