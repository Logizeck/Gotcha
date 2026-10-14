using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 3 — pupazzo di neve.
    /// Occhi (2 carboni) e naso (carota) si attaccano dove li lasci sulla testa: nessuna posizione è indicata.
    /// Per vincere ogni occhio deve stare nella sua zona (ampia, invisibile), il naso nella sua, e la bocca va
    /// disegnata col dito: un sorriso che resta come un solco nella neve.
    /// Il pupazzo è in posa di 3/4 (guarda a sinistra): il sedere sporge sul retro, a destra in basso.
    /// Easter egg: lasciare la carota sul sedere → la carota resta piantata lì, il pupazzo arrossisce, fuma,
    /// "Oh noooo!" e si scioglie lentamente dalla testa in giù, mentre la base resta a terra.
    /// </summary>
    public class SnowmanLevel : LevelController
    {
        [Header("Parti del pupazzo (pivot sui punti di appoggio)")]
        [SerializeField] Transform snowman;      // radice ai piedi
        [SerializeField] Transform basePart;     // pivot y = 0
        [SerializeField] Transform middlePart;   // pivot y = 1.95
        [SerializeField] Transform armsPart;     // pivot alle spalle, y = 2.85
        [SerializeField] Transform headPart;     // pivot y = 3.42
        [SerializeField] Transform hatPart;      // pivot y = 4.55
        [SerializeField] SpriteRenderer[] bodyRenderers;
        [SerializeField] SpriteRenderer blush;
        [SerializeField] Transform puddle;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] SmokeEmitter smoke;
        [SerializeField] StrokeDrawer mouthPen;  // centrato sulla testa

        [Header("Zone invisibili (coordinate del pupazzo, piedi in 0,0)")]
        [SerializeField] Vector2 headCenter = new Vector2(-0.12f, 4.02f);
        [SerializeField] float stickRadius = 0.8f;                              // dove gli oggetti restano attaccati
        [SerializeField] Rect leftEyeZone = new Rect(-0.8f, 3.98f, 0.6f, 0.55f);
        [SerializeField] Rect rightEyeZone = new Rect(-0.22f, 3.98f, 0.62f, 0.55f);
        [SerializeField] Rect noseZone = new Rect(-0.8f, 3.66f, 1.0f, 0.46f);
        [SerializeField] Rect mouthZone = new Rect(-0.78f, 3.38f, 1.25f, 0.6f); // dove deve stare il sorriso
        [SerializeField] Vector2 buttCenter = new Vector2(1.45f, 0.78f);
        [SerializeField] float buttRadius = 0.6f;
        [SerializeField] Vector2 buttSurface = new Vector2(1.58f, 0.78f);         // dove si pianta la punta della carota

        [Header("Easter egg")]
        [SerializeField] string eggId = "snowman_butt";
        [SerializeField] float groundY = -3.9f;

        const float MiddlePivot = 1.95f, ShoulderY = 2.85f, HeadPivot = 3.42f, HatPivot = 4.55f;
        static readonly Color BlushTint = new Color(1f, 0.78f, 0.83f);
        static readonly Color MeltTint = new Color(0.86f, 0.94f, 1f);

        Draggable2D[] items;
        bool resolving;
        bool smileOk;

        public override void OnLevelStart()
        {
            items = GetComponentsInChildren<Draggable2D>(true);
            foreach (Draggable2D d in items) d.Dropped += OnItemDropped;
            if (mouthPen != null) mouthPen.StrokeFinished += OnMouthDrawn;

            if (blush != null) blush.color = new Color(1f, 1f, 1f, 0f);
            if (puddle != null) puddle.localScale = Vector3.zero;
            if (bubble != null) bubble.Hide();
        }

        void OnDestroy()
        {
            if (items != null) foreach (Draggable2D d in items) if (d != null) d.Dropped -= OnItemDropped;
            if (mouthPen != null) mouthPen.StrokeFinished -= OnMouthDrawn;
        }

        Vector2 Local(Vector3 world) { return snowman.InverseTransformPoint(world); }

        // ---------- Faccia ----------

        void OnItemDropped(Draggable2D item, DropSlot slot)
        {
            if (IsFinished || resolving) return;
            Vector2 p = Local(item.transform.position);

            if (item.itemId == "carrot" && Vector2.Distance(p, buttCenter) <= buttRadius)
            {
                resolving = true;
                StartCoroutine(MeltRoutine(item));
                return;
            }
            if (Vector2.Distance(p, headCenter) <= stickRadius)
            {
                item.transform.SetParent(headPart, true);   // resta lì, attaccato alla testa
                CheckComplete();
            }
            else
            {
                item.transform.SetParent(transform, true);
                StartCoroutine(FallToGround(item.transform));
            }
        }

        void OnMouthDrawn(StrokeDrawer pen)
        {
            smileOk = IsSmile(pen.LastStrokePoints);
            CheckComplete();
        }

        /// <summary>Un sorriso: abbastanza largo, nella zona della bocca, con le punte più in alto del centro.</summary>
        bool IsSmile(IReadOnlyList<Vector3> pts)
        {
            if (pts == null || pts.Count < 3) return false;
            float minX = float.MaxValue, maxX = float.MinValue, minY = float.MaxValue, sumY = 0f;
            int lowest = 0;
            for (int i = 0; i < pts.Count; i++)
            {
                Vector2 p = Local(mouthPen.transform.TransformPoint(pts[i]));
                if (!mouthZone.Contains(p)) return false;
                minX = Mathf.Min(minX, p.x);
                maxX = Mathf.Max(maxX, p.x);
                if (p.y < minY) { minY = p.y; lowest = i; }
                sumY += p.y;
            }
            Vector2 a = Local(mouthPen.transform.TransformPoint(pts[0]));
            Vector2 b = Local(mouthPen.transform.TransformPoint(pts[pts.Count - 1]));
            bool wide = maxX - minX >= 0.25f;
            bool curved = (a.y + b.y) * 0.5f - minY >= 0.03f;
            bool lowInMiddle = lowest > 0 && lowest < pts.Count - 1;
            return wide && curved && lowInMiddle;
        }

        void CheckComplete()
        {
            if (IsFinished || resolving) return;
            bool left = false, right = false, nose = false;
            foreach (Draggable2D d in items)
            {
                if (d == null || d.transform.parent != headPart) continue;
                Vector2 p = Local(d.transform.position);
                if (d.itemId == "eye")
                {
                    if (!left && leftEyeZone.Contains(p)) left = true;
                    else if (!right && rightEyeZone.Contains(p)) right = true;
                }
                if (d.itemId == "carrot" && noseZone.Contains(p)) nose = true;
            }
            if (left && right && nose && smileOk)
            {
                resolving = true;
                StartCoroutine(WinRoutine());
            }
        }

        IEnumerator FallToGround(Transform t)
        {
            float target = groundY + Random.Range(-0.35f, 0.25f);
            if (t.position.y <= target + 0.3f) yield break;
            float v = 0f;
            while (t != null && t.position.y > target)
            {
                if (t.parent == headPart || Draggable2D.Active != null && Draggable2D.Active.transform == t) yield break;
                v += 18f * Time.deltaTime;
                t.position += Vector3.down * (v * Time.deltaTime);
                yield return null;
            }
            if (t != null) t.position = new Vector3(t.position.x, target, t.position.z);
        }

        void LockInput()
        {
            foreach (Draggable2D d in items) if (d != null) d.enabled = false;
            if (mouthPen != null) mouthPen.enabled = false;
        }

        IEnumerator WinRoutine()
        {
            LockInput();
            if (bubble != null) bubble.Show("Yay!");
            Vector3 start = snowman.localPosition;
            for (int hop = 0; hop < 2; hop++)
                for (float k = 0f; k < 1f; k += Time.deltaTime / 0.35f)
                {
                    snowman.localPosition = start + Vector3.up * (Mathf.Sin(k * Mathf.PI) * 0.3f);
                    yield return null;
                }
            snowman.localPosition = start;
            yield return new WaitForSeconds(0.4f);
            Win();
        }

        // ---------- Easter egg: carota sul sedere ----------

        IEnumerator MeltRoutine(Draggable2D carrot)
        {
            LockInput();
            // la carota resta piantata nel sedere, con la punta verso il pupazzo
            Transform t = carrot.transform;
            t.SetParent(basePart, true);
            t.localScale = new Vector3(-Mathf.Abs(t.localScale.x), t.localScale.y, t.localScale.z);
            Vector3 target = snowman.TransformPoint(buttSurface + new Vector2(0.36f, 0f));
            target.z = t.position.z;
            Vector3 from = t.position;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.15f) { t.position = Vector3.Lerp(from, target, k); yield return null; }
            t.position = target;

            // 1. arrossisce
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.9f)
            {
                SetTint(Color.Lerp(Color.white, BlushTint, k), k);
                yield return null;
            }
            SetTint(BlushTint, 1f);

            // 2. fuma e strilla
            if (bubble != null) bubble.Show("Oh noooo!");
            if (smoke != null) smoke.Play();
            yield return new WaitForSeconds(0.9f);

            // 3. si scioglie lentamente dalla testa in giù; la base resta a terra
            float sh = 1f, sm = 1f, sb = 1f;
            yield return Melt(1.8f, k => { sh = Mathf.Lerp(1f, 0.18f, k); Pose(sb, sm, sh, 0.25f * k); });
            yield return Melt(1.8f, k => { sm = Mathf.Lerp(1f, 0.22f, k); Pose(sb, sm, sh, 0.25f + 0.35f * k); });
            yield return Melt(2.2f, k => { sb = Mathf.Lerp(1f, 0.42f, k); Pose(sb, sm, sh, 0.6f + 0.4f * k); });

            if (smoke != null) smoke.Stop();
            yield return new WaitForSeconds(1.0f);
            FailWithEgg(eggId);
        }

        delegate void MeltStep(float k);

        IEnumerator Melt(float seconds, MeltStep step)
        {
            for (float k = 0f; k < 1f; k += Time.deltaTime / seconds)
            {
                step(k * k * (3f - 2f * k));
                yield return null;
            }
            step(1f);
        }

        /// <summary>Mette in posa le parti: ognuna appoggiata su quella sotto, schiacciata del suo fattore.</summary>
        void Pose(float sb, float sm, float sh, float overall)
        {
            Squash(basePart, 0f, sb);
            float middleY = MiddlePivot * sb;
            Squash(middlePart, middleY, sm);
            if (armsPart != null) armsPart.localPosition = new Vector3(0f, middleY + (ShoulderY - MiddlePivot) * sm, 0f);
            float headY = middleY + (HeadPivot - MiddlePivot) * sm;
            Squash(headPart, headY, sh);
            if (hatPart != null) hatPart.localPosition = new Vector3(0f, headY + (HatPivot - HeadPivot) * sh, 0f);

            if (puddle != null) puddle.localScale = Vector3.one * Mathf.Min(1f, overall * 1.1f);
            SetTint(Color.Lerp(BlushTint, MeltTint, overall), 1f - Mathf.Clamp01(overall * 2.5f));
        }

        static void Squash(Transform t, float y, float s)
        {
            if (t == null) return;
            t.localPosition = new Vector3(0f, y, 0f);
            t.localScale = new Vector3(1f + (1f - s) * 0.35f, s, 1f);
        }

        void SetTint(Color c, float blushAlpha)
        {
            if (bodyRenderers != null) foreach (SpriteRenderer r in bodyRenderers) if (r != null) r.color = c;
            if (blush != null) blush.color = new Color(1f, 1f, 1f, blushAlpha);
        }
    }
}
