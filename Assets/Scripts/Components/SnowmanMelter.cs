using System.Collections.Generic;
using UnityEngine;

namespace Gotcha.Components
{
    /// <summary>
    /// Scioglimento del pupazzo di neve, riutilizzabile. Una maschera scende dall'alto: tutto ciò che sta sopra
    /// la "linea di neve" sparisce davvero (niente pezzi deformati). Sulla linea scorre una fascia di neve bagnata
    /// con le colature, larga esattamente quanto il pupazzo a quella quota; da lì gocciolano acqua e sale vapore,
    /// sotto si allarga la pozzanghera. Gli oggetti attaccati (cappello, braccia, carota, occhi...) cadono quando
    /// la linea li raggiunge e restano a galleggiare nella pozzanghera.
    /// Progress va da 0 (intero) a 1 (solo pozzanghera); può avanzare a tempo o a piccoli passi (livello del sole).
    /// </summary>
    public class SnowmanMelter : MonoBehaviour
    {
        [Tooltip("Radice del pupazzo, ai piedi (coordinate della sagoma).")]
        public Transform snowman;
        public SpriteRenderer[] masked;
        public Sprite maskSprite;
        public SpriteRenderer front;
        public SmokeEmitter drips;
        public SmokeEmitter steam;
        public Transform puddle;
        public Vector3 puddleFullScale = new Vector3(1.15f, 1.05f, 1f);
        [Header("Sagoma (calcolata dal builder)")]
        public float profileY0;
        public float profileStep = 0.05f;
        public float[] profileLeft;
        public float[] profileRight;
        public float topY = 4.8f;
        public float bottomY = -0.05f;
        public float waterY = 0.05f;                      // pelo dell'acqua nella pozzanghera (coordinate del pupazzo)
        public Color meltTint = new Color(0.86f, 0.94f, 1f);

        public float Progress { get; private set; }
        public float LineY => Mathf.Lerp(topY, bottomY, Progress);

        class Loose { public Transform t; public float y; public float spin; public bool floating; public Vector2 v; public float phase; }
        readonly List<Loose> loose = new List<Loose>();
        SpriteMask mask;
        bool begun;
        Color[] baseColors;

        /// <summary>Un oggetto che cade quando la linea di neve scende sotto la sua quota.</summary>
        public void AddLoose(Transform t)
        {
            if (t == null) return;
            foreach (Loose l in loose) if (l.t == t) return;
            loose.Add(new Loose { t = t, y = snowman.InverseTransformPoint(t.position).y, spin = Random.Range(-260f, 260f), phase = Random.value * 6f });
        }

        public void Begin()
        {
            if (begun) return;
            begun = true;
            var go = new GameObject("MeltMask");
            go.transform.SetParent(snowman, false);
            mask = go.AddComponent<SpriteMask>();
            mask.sprite = maskSprite;
            go.transform.localScale = new Vector3(10f, 12f, 1f);
            baseColors = new Color[masked.Length];
            for (int i = 0; i < masked.Length; i++)
            {
                if (masked[i] == null) continue;
                masked[i].maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
                baseColors[i] = masked[i].color;
            }
            if (front != null) front.gameObject.SetActive(true);
            if (drips != null) drips.Play();
            if (steam != null) steam.Play();
            SetProgress(0f);
        }

        /// <summary>Aggiunge al gruppo mascherato un disegno comparso dopo (es. la bocca stupita).</summary>
        public void AddMasked(SpriteRenderer r)
        {
            if (r == null) return;
            r.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
            var list = new List<SpriteRenderer>(masked) { r };
            masked = list.ToArray();
            var cols = new List<Color>(baseColors ?? new Color[0]) { r.color };
            baseColors = cols.ToArray();
        }

        public void SetProgress(float p)
        {
            if (!begun) Begin();
            Progress = Mathf.Clamp01(Mathf.Max(Progress, p));
            float line = LineY;
            if (mask != null) mask.transform.localPosition = new Vector3(0f, line - 6f, 0f);

            Width(line, out float l, out float r);
            bool any = r - l > 0.05f && Progress < 1f;
            if (front != null)
            {
                front.gameObject.SetActive(any);
                float wobble = 1f + Mathf.Sin(Time.time * 7f) * 0.04f;
                front.transform.localPosition = new Vector3((l + r) * 0.5f, line, 0f);
                front.transform.localScale = new Vector3(Mathf.Max(0.2f, r - l + 0.1f), wobble, 1f);
            }
            if (drips != null)
            {
                drips.transform.localPosition = new Vector3((l + r) * 0.5f, line - 0.25f, 0f);
                drips.spread = Mathf.Max(0.1f, (r - l) * 0.45f);
                if (!any) drips.Stop();
            }
            if (steam != null)
            {
                steam.transform.localPosition = new Vector3((l + r) * 0.5f, line + 0.1f, 0f);
                steam.spread = Mathf.Max(0.1f, (r - l) * 0.35f);
                if (!any) steam.Stop();
            }
            if (puddle != null)
            {
                float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(Progress * 1.3f));
                puddle.localScale = Vector3.Scale(puddleFullScale, new Vector3(Mathf.Lerp(0.25f, 1f, k), k, 1f));
            }
            // la neve si fa più azzurrina e bagnata man mano
            for (int i = 0; i < masked.Length; i++)
                if (masked[i] != null && baseColors != null && i < baseColors.Length)
                    masked[i].color = Color.Lerp(baseColors[i], baseColors[i] * meltTint, Progress);

            foreach (Loose o in loose)
                if (!o.floating && o.v == Vector2.zero && o.t != null && line < o.y + 0.05f) Release(o);
        }

        public void End()
        {
            if (front != null) front.gameObject.SetActive(false);
            if (drips != null) drips.Stop();
            if (steam != null) steam.Stop();
        }

        void Release(Loose o)
        {
            o.t.SetParent(snowman, true);
            float side = o.t.localPosition.x >= -0.05f ? 1f : -1f;
            o.v = new Vector2(side * Random.Range(0.4f, 1.1f), Random.Range(0.5f, 1.6f));
            foreach (Draggable2D d in o.t.GetComponentsInChildren<Draggable2D>()) d.enabled = false;
        }

        void Update()
        {
            foreach (Loose o in loose)
            {
                if (o.t == null) continue;
                if (o.floating)
                {
                    // galleggia nella pozzanghera, dondolando
                    Vector3 p = o.t.localPosition;
                    o.t.localPosition = new Vector3(p.x + Mathf.Sin(Time.time * 1.3f + o.phase) * 0.0015f, waterY + Mathf.Sin(Time.time * 2.2f + o.phase) * 0.03f, p.z);
                    o.t.localRotation = Quaternion.Euler(0, 0, o.spin * 0.08f + Mathf.Sin(Time.time * 1.7f + o.phase) * 6f);
                    continue;
                }
                if (o.v == Vector2.zero) continue;
                o.v.y -= 11f * Time.deltaTime;
                o.t.localPosition += (Vector3)(o.v * Time.deltaTime);
                o.t.Rotate(0, 0, o.spin * Time.deltaTime);
                if (o.t.localPosition.y <= waterY && o.v.y < 0f)
                {
                    o.floating = true;
                    Vector3 p = o.t.localPosition;
                    float reach = puddle != null ? 1.9f * puddleFullScale.x : 1.8f;
                    o.t.localPosition = new Vector3(Mathf.Clamp(p.x, -reach, reach), waterY, p.z);
                }
            }
        }

        void Width(float y, out float l, out float r)
        {
            l = r = 0f;
            if (profileLeft == null || profileLeft.Length == 0) return;
            float f = (y - profileY0) / profileStep;
            int i = Mathf.Clamp(Mathf.FloorToInt(f), 0, profileLeft.Length - 1);
            int j = Mathf.Min(i + 1, profileLeft.Length - 1);
            float t = Mathf.Clamp01(f - i);
            l = Mathf.Lerp(profileLeft[i], profileLeft[j], t);
            r = Mathf.Lerp(profileRight[i], profileRight[j], t);
        }
    }
}
