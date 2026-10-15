using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 11 — pupazzo di neve.
    /// Occhi (2 carboni) e naso (carota) si attaccano dove li lasci sulla testa: nessuna posizione è indicata.
    /// Per vincere ogni occhio deve stare nella sua zona (ampia, invisibile), il naso nella sua, e la bocca va
    /// disegnata col dito: un sorriso che resta come un solco nella neve.
    /// Il pupazzo è in posa di 3/4 (guarda a sinistra): il sedere sporge sul retro, a destra in basso.
    /// Easter egg: lasciare la carota sul sedere → la carota resta piantata lì, il pupazzo arrossisce, gli occhi
    /// schizzano via, la bocca resta a "O", e si scioglie del tutto dall'alto in basso (SnowmanMelter) finché resta
    /// solo una pozzanghera con cappello, braccia e carota che galleggiano. La scena viene registrata per condividerla.
    /// </summary>
    public class SnowmanLevel : LevelController
    {
        [Header("Parti del pupazzo (pivot sui punti di appoggio)")]
        [SerializeField] Transform snowman;      // radice ai piedi
        [SerializeField] Transform basePart;     // pivot y = 0
        [SerializeField] Transform headPart;     // pivot y = 3.42
        [SerializeField] SpriteRenderer[] bodyRenderers;
        [SerializeField] SpriteRenderer blush;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] StrokeDrawer mouthPen;  // centrato sulla testa
        [SerializeField] SnowmanMeltdown meltdown;   // urlo + scioglimento (uguale al livello 29)
        [SerializeField] SpriteRenderer mouthO;  // bocca stupita (nascosta finché non serve)

        [Header("Zone invisibili (coordinate del pupazzo, piedi in 0,0)")]
        [SerializeField] Vector2 headCenter = new Vector2(-0.1f, 3.7f);
        [SerializeField] float stickRadius = 1.15f;                             // dove gli oggetti restano attaccati
        [SerializeField] Rect leftEyeZone = new Rect(-1.05f, 3.72f, 0.85f, 0.75f);
        [SerializeField] Rect rightEyeZone = new Rect(-0.2f, 3.72f, 0.9f, 0.75f);
        [SerializeField] Rect noseZone = new Rect(-1.05f, 3.3f, 1.5f, 0.66f);
        [SerializeField] Rect mouthZone = new Rect(-1.05f, 2.8f, 1.8f, 0.85f);  // dove deve stare il sorriso
        [SerializeField] Vector2 buttCenter = new Vector2(0.86f, 1.02f);
        [SerializeField] float buttRadius = 0.5f;
        [SerializeField] Vector2 buttSurface = new Vector2(0.86f, 1.02f);       // dove si pianta la punta della carota

        [Header("Easter egg")]
        [SerializeField] string eggId = "snowman_butt";
        [SerializeField] float groundY = -3.9f;

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
            if (mouthO != null) mouthO.gameObject.SetActive(false);
            if (bubble != null) bubble.Hide();
            // registra sempre gli ultimi secondi: se parte l'Easter egg, il video è pronto da condividere
            if (ClipRecorder.Instance != null) ClipRecorder.Instance.Begin();
        }

        void OnDestroy()
        {
            if (ClipRecorder.Instance != null && ClipRecorder.Instance.IsCapturing) ClipRecorder.Instance.Discard();
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
            if (ClipRecorder.Instance != null) ClipRecorder.Instance.Discard();
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
            Vector3 target = snowman.TransformPoint(buttSurface + new Vector2(0.4f * Mathf.Abs(t.localScale.x), 0f));
            target.z = t.position.z;
            Vector3 from = t.position;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.15f) { t.position = Vector3.Lerp(from, target, k); yield return null; }
            t.position = target;

            // 1. sobbalzo e rossore, tremando dall'imbarazzo
            Vector3 home = snowman.localPosition;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.25f)
            {
                snowman.localPosition = home + Vector3.up * Mathf.Sin(k * Mathf.PI) * 0.35f;
                yield return null;
            }
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.9f)
            {
                SetTint(Color.Lerp(Color.white, BlushTint, k), k);
                snowman.localPosition = home + (Vector3)(Random.insideUnitCircle * 0.03f);
                yield return null;
            }
            snowman.localPosition = home;
            SetTint(BlushTint, 1f);

            // 2. URLO, occhi che schizzano, cappello che salta, braccia nel panico... e poi lo scioglimento vero
            var eyes = new List<Transform>();
            var attached = new List<Transform>();
            foreach (Draggable2D d in items)
            {
                if (d == null || !d.gameObject.activeSelf) continue;
                if (d.itemId == "eye" && d.transform.IsChildOf(headPart)) eyes.Add(d.transform);
                else if (d.transform.IsChildOf(headPart) || d.transform.IsChildOf(basePart)) attached.Add(d.transform);
            }
            SurprisedMouth();
            if (meltdown != null)
            {
                if (mouthO == null || !mouthO.gameObject.activeSelf) meltdown.mouthO = null;   // niente bocca disegnata: niente "O"
                yield return meltdown.Play(eyes, attached);
            }
            if (ClipRecorder.Instance != null) ClipRecorder.Instance.Stop();   // il video si può condividere
            FailWithEgg(eggId);
        }

        /// <summary>Se la bocca era stata disegnata, il solco diventa una "O" di stupore nello stesso punto.</summary>
        void SurprisedMouth()
        {
            if (mouthPen == null || !mouthPen.HasStroke || mouthO == null) return;
            IReadOnlyList<Vector3> pts = mouthPen.LastStrokePoints;
            Vector3 c = Vector3.zero;
            if (pts != null && pts.Count > 0)
            {
                foreach (Vector3 p in pts) c += p;
                c /= pts.Count;
                c = mouthPen.transform.TransformPoint(c);
            }
            else c = mouthO.transform.position;
            mouthPen.Clear();
            mouthO.transform.position = new Vector3(c.x, c.y - 0.05f, mouthO.transform.position.z);
            mouthO.gameObject.SetActive(true);
            StartCoroutine(PopIn(mouthO.transform));
        }

        IEnumerator PopIn(Transform t)
        {
            Vector3 s = t.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.2f)
            {
                t.localScale = s * (k < 0.7f ? k / 0.7f * 1.25f : Mathf.Lerp(1.25f, 1f, (k - 0.7f) / 0.3f));
                yield return null;
            }
            t.localScale = s;
        }

        void SetTint(Color c, float blushAlpha)
        {
            if (bodyRenderers != null) foreach (SpriteRenderer r in bodyRenderers) if (r != null) r.color = c;
            if (blush != null) blush.color = new Color(1f, 1f, 1f, blushAlpha);
        }
    }
}
