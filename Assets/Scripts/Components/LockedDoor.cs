using System;
using System.Collections;
using UnityEngine;

namespace Gotcha.Components
{
    /// <summary>
    /// Porta con serratura, riutilizzabile. La serratura è un DropSlot figlio: quando la chiave entra
    /// la chiave gira, l'anta si apre e parte Opened. Se la chiave è troppo grande (DropSlot.maxItemScale)
    /// la porta trema e parte KeyRejected.
    /// </summary>
    public class LockedDoor : MonoBehaviour
    {
        [SerializeField] DropSlot keyhole;
        [SerializeField] Transform panel;        // anta: ha il pivot sul lato dei cardini
        [SerializeField] float openSeconds = 0.6f;

        public bool IsOpen { get; private set; }
        public event Action Opened;
        public event Action KeyRejected;

        bool shaking;

        void Awake()
        {
            if (keyhole == null) keyhole = GetComponentInChildren<DropSlot>();
            if (keyhole == null) { Debug.LogError("LockedDoor senza serratura (DropSlot)."); return; }
            keyhole.Filled += OnKeyInserted;
            keyhole.Rejected += OnKeyRejected;
        }

        void OnDestroy()
        {
            if (keyhole == null) return;
            keyhole.Filled -= OnKeyInserted;
            keyhole.Rejected -= OnKeyRejected;
        }

        void OnKeyInserted(DropSlot slot, Draggable2D key)
        {
            PinchScalable pinch = key.GetComponent<PinchScalable>();
            if (pinch != null) pinch.enabled = false;
            key.transform.SetParent(panel != null ? panel : transform, true);   // la chiave si apre con l'anta
            StartCoroutine(OpenRoutine(key.transform));
        }

        void OnKeyRejected(DropSlot slot, Draggable2D key)
        {
            KeyRejected?.Invoke();
            if (!shaking) StartCoroutine(Shake());
        }

        IEnumerator OpenRoutine(Transform key)
        {
            // La chiave gira (compressione verticale che va e torna)
            Vector3 ks = key.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.4f)
            {
                key.localScale = new Vector3(ks.x, ks.y * Mathf.Cos(k * Mathf.PI * 2f), ks.z);
                yield return null;
            }
            key.localScale = ks;
            Haptics.Light();   // clac della serratura

            if (panel != null)
            {
                Vector3 ps = panel.localScale;
                for (float k = 0f; k < 1f; k += Time.deltaTime / openSeconds)
                {
                    float e = 1f - (1f - k) * (1f - k);
                    panel.localScale = new Vector3(ps.x * Mathf.Lerp(1f, 0.08f, e), ps.y, ps.z);
                    yield return null;
                }
                panel.localScale = new Vector3(ps.x * 0.08f, ps.y, ps.z);
            }

            IsOpen = true;
            Opened?.Invoke();
        }

        IEnumerator Shake()
        {
            shaking = true;
            Vector3 start = transform.localPosition;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.3f)
            {
                transform.localPosition = start + Vector3.right * (Mathf.Sin(k * Mathf.PI * 8f) * 0.08f * (1f - k));
                yield return null;
            }
            transform.localPosition = start;
            shaking = false;
        }
    }
}
