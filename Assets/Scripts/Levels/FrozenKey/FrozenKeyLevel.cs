using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 23 — la chiave nel ghiaccio. Azioni in sequenza: 1) strofina il blocco di ghiaccio col dito (avanti e
    /// indietro, veloce: l'attrito lo scioglie); 2) la chiave liberata è troppo grande: rimpiccioliscila col pizzico;
    /// 3) infilala nella serratura.
    /// </summary>
    public class FrozenKeyLevel : LevelController
    {
        [SerializeField] Transform ice;
        [SerializeField] SpriteRenderer iceArt;
        [SerializeField] Collider2D iceArea;
        [SerializeField] Draggable2D key;
        [SerializeField] Transform puddle;
        [SerializeField] SmokeEmitter drips;
        [SerializeField] SmokeEmitter steam;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] float rubNeeded = 16f;    // metri di strofinio (unità mondo)

        float rubbed, rubbingFor;
        Vector2 last;
        bool tracking, free;
        Camera cam;
        Vector3 iceScale;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            iceScale = ice.localScale;
            SetKey(false);
            if (puddle != null) puddle.localScale = Vector3.zero;
            if (drips != null) drips.Stop();
            if (steam != null) steam.Stop();
            LockedDoor door = GetComponentInChildren<LockedDoor>(true);
            if (door != null) door.Opened += () => Win();
            if (bubble != null) bubble.Hide();
        }

        void SetKey(bool on)
        {
            key.enabled = on;
            PinchScalable pinch = key.GetComponent<PinchScalable>();
            if (pinch != null) pinch.enabled = on;
        }

        void Update()
        {
            if (free || IsFinished) return;
            bool rubbing = false;
            if (PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out bool ended) && !ended)
            {
                Vector2 w = PointerInput.ToWorld(cam, sp);
                if (iceArea.OverlapPoint(w))
                {
                    if (tracking && !began)
                    {
                        float d = Vector2.Distance(w, last);
                        if (d > 0.01f) { rubbed += d; rubbing = true; }
                    }
                    tracking = true;
                    last = w;
                }
                else tracking = false;
            }
            else tracking = false;

            rubbingFor = rubbing ? 0.2f : rubbingFor - Time.deltaTime;
            bool active = rubbingFor > 0f;
            if (drips != null) { if (active) drips.Play(); else drips.Stop(); }
            if (steam != null) { if (active) steam.Play(); else steam.Stop(); }

            float k = Mathf.Clamp01(rubbed / rubNeeded);
            ice.localScale = new Vector3(iceScale.x * Mathf.Lerp(1f, 0.85f, k), iceScale.y * Mathf.Lerp(1f, 0.25f, k), 1f);
            if (iceArt != null) iceArt.color = new Color(1f, 1f, 1f, Mathf.Lerp(1f, 0.35f, k));
            if (puddle != null) puddle.localScale = new Vector3(k * 0.6f, k * 0.8f, 1f);
            if (rubbed >= rubNeeded) StartCoroutine(Free());
        }

        IEnumerator Free()
        {
            free = true;
            if (drips != null) drips.Stop();
            if (steam != null) steam.Stop();
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.4f)
            {
                if (iceArt != null) iceArt.color = new Color(1f, 1f, 1f, Mathf.Lerp(0.35f, 0f, k));
                yield return null;
            }
            ice.gameObject.SetActive(false);
            SetKey(true);
            if (bubble != null) { bubble.Show("Free! ...but huge."); yield return new WaitForSeconds(1.8f); bubble.Hide(); }
        }
    }
}
