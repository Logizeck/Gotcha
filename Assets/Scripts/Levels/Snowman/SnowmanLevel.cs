using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 3 — pupazzo di neve. Occhi (2 carboni) e naso (carota) vanno trascinati in un punto qualsiasi
    /// della faccia; la bocca va tracciata col dito sulla faccia. Nessuna posizione è indicata.
    /// Easter egg: avvicinare la carota al sedere del pupazzo → arrossisce, fuma, dice "Oh noooo!" e si scioglie.
    /// </summary>
    public class SnowmanLevel : LevelController
    {
        [SerializeField] Transform snowman;          // pivot ai piedi: si scioglie verso il basso
        [SerializeField] SpriteRenderer body;
        [SerializeField] SpriteRenderer blush;
        [SerializeField] Transform puddle;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] SmokeEmitter smoke;
        [SerializeField] StrokeDrawer mouthPen;

        [Header("Faccia (coordinate locali del pupazzo)")]
        [SerializeField] Vector2 faceCenter = new Vector2(0f, 4.0f);
        [SerializeField] float faceRadius = 0.52f;

        [Header("Easter egg")]
        [SerializeField] string eggId = "snowman_butt";
        [SerializeField] Vector2 buttPoint = new Vector2(0f, 0.35f);
        [SerializeField] float meltSeconds = 3.5f;

        [Header("Oggetti lasciati fuori dalla faccia cadono nella neve")]
        [SerializeField] float groundY = -3.9f;

        static readonly Color BlushTint = new Color(1f, 0.8f, 0.85f);

        Draggable2D[] items;
        EasterEggTrigger egg;
        bool resolving;

        public override void OnLevelStart()
        {
            items = GetComponentsInChildren<Draggable2D>(true);
            foreach (Draggable2D d in items) d.Dropped += OnItemDropped;

            egg = GetComponentInChildren<EasterEggTrigger>(true);
            if (egg != null) egg.Triggered += OnEggTriggered;
            if (mouthPen != null) mouthPen.StrokeFinished += OnMouthDrawn;

            if (blush != null) blush.color = new Color(1f, 1f, 1f, 0f);
            if (puddle != null) puddle.localScale = Vector3.zero;
            if (bubble != null) bubble.Hide();
        }

        void OnDestroy()
        {
            if (items != null) foreach (Draggable2D d in items) if (d != null) d.Dropped -= OnItemDropped;
            if (egg != null) egg.Triggered -= OnEggTriggered;
            if (mouthPen != null) mouthPen.StrokeFinished -= OnMouthDrawn;
        }

        // ---------- Faccia ----------

        bool OnFace(Vector3 world)
        {
            Vector2 local = snowman.InverseTransformPoint(world);
            return Vector2.Distance(local, faceCenter) <= faceRadius;
        }

        void OnItemDropped(Draggable2D item, DropSlot slot)
        {
            if (IsFinished || resolving) return;
            if (OnFace(item.transform.position))
            {
                item.transform.SetParent(snowman, true);   // ora fa parte del pupazzo
                CheckComplete();
            }
            else
            {
                item.transform.SetParent(transform, true);
                StartCoroutine(FallToGround(item.transform));
            }
        }

        void OnMouthDrawn(StrokeDrawer pen) { CheckComplete(); }

        void CheckComplete()
        {
            if (IsFinished || resolving) return;
            int eyes = 0;
            bool nose = false;
            foreach (Draggable2D d in items)
            {
                if (d == null || d.transform.parent != snowman) continue;
                if (d.itemId == "eye") eyes++;
                if (d.itemId == "carrot") nose = true;
            }
            if (eyes >= 2 && nose && mouthPen != null && mouthPen.HasStroke)
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
                if (t.parent == snowman || Draggable2D.Active != null && Draggable2D.Active.transform == t) yield break;
                v += 18f * Time.deltaTime;
                t.position += Vector3.down * (v * Time.deltaTime);
                yield return null;
            }
            if (t != null) t.position = new Vector3(t.position.x, target, t.position.z);
        }

        IEnumerator WinRoutine()
        {
            foreach (Draggable2D d in items) if (d != null) d.enabled = false;
            if (bubble != null) bubble.Show("Yay!");
            Vector3 start = snowman.localPosition;
            for (int hop = 0; hop < 2; hop++)
                for (float k = 0f; k < 1f; k += Time.deltaTime / 0.35f)
                {
                    snowman.localPosition = start + Vector3.up * (Mathf.Sin(k * Mathf.PI) * 0.3f);
                    yield return null;
                }
            snowman.localPosition = start;
            yield return new WaitForSeconds(0.3f);
            Win();
        }

        // ---------- Easter egg: carota sul sedere ----------

        void OnEggTriggered(string id)
        {
            if (IsFinished || resolving) return;
            resolving = true;
            StartCoroutine(MeltRoutine(Draggable2D.Active));
        }

        IEnumerator MeltRoutine(Draggable2D carrot)
        {
            foreach (Draggable2D d in items) if (d != null) d.enabled = false;
            if (mouthPen != null) mouthPen.enabled = false;

            // la carota "punge" e poi cade nella neve
            if (carrot != null)
            {
                Transform t = carrot.transform;
                t.SetParent(transform, true);
                Vector3 from = t.position, poke = snowman.TransformPoint(buttPoint);
                poke.z = from.z;
                for (float k = 0f; k < 1f; k += Time.deltaTime / 0.12f) { t.position = Vector3.Lerp(from, poke, k); yield return null; }
                StartCoroutine(FallToGround(t));
            }

            // arrossisce
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.6f)
            {
                SetBlush(k);
                yield return null;
            }
            SetBlush(1f);

            if (bubble != null) bubble.Show("Oh noooo!");
            if (smoke != null) smoke.Play();
            yield return new WaitForSeconds(0.6f);

            // si scioglie lentamente
            Vector3 baseScale = snowman.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / meltSeconds)
            {
                float e = k * k * (3f - 2f * k);
                snowman.localScale = new Vector3(baseScale.x * (1f + 0.45f * e), baseScale.y * (1f - 0.9f * e), baseScale.z);
                if (puddle != null) puddle.localScale = Vector3.one * Mathf.Min(1f, e * 1.3f);
                if (body != null) body.color = Color.Lerp(BlushTint, new Color(0.88f, 0.95f, 1f), e);
                if (blush != null) blush.color = new Color(1f, 1f, 1f, 1f - e);
                yield return null;
            }

            if (smoke != null) smoke.Stop();
            yield return new WaitForSeconds(1.0f);
            FailWithEgg(eggId);
        }

        void SetBlush(float k)
        {
            if (blush != null) blush.color = new Color(1f, 1f, 1f, k);
            if (body != null) body.color = Color.Lerp(Color.white, BlushTint, k);
        }
    }
}
