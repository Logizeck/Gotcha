using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 1 — pupazzo di neve. Si vince mettendo occhi, naso e bocca sulla testa (tutti i DropSlot pieni).
    /// Easter egg: portare la carota al coniglio, che se la mangia (fallimento divertente).
    /// Slot e trigger vengono cercati tra i figli, così il prefab si può ridisegnare senza toccare il codice.
    /// </summary>
    public class SnowmanLevel : LevelController
    {
        [SerializeField] Transform snowman;
        [SerializeField] Transform bunny;
        [SerializeField] float animSeconds = 0.45f;

        DropSlot[] slots;
        EasterEggTrigger egg;
        bool resolving;

        public override void OnLevelStart()
        {
            slots = GetComponentsInChildren<DropSlot>(true);
            foreach (DropSlot s in slots) s.Filled += OnSlotFilled;

            egg = GetComponentInChildren<EasterEggTrigger>(true);
            if (egg != null) egg.Triggered += OnEggTriggered;
        }

        void OnDestroy()
        {
            if (slots != null) foreach (DropSlot s in slots) s.Filled -= OnSlotFilled;
            if (egg != null) egg.Triggered -= OnEggTriggered;
        }

        void OnSlotFilled(DropSlot slot, Draggable2D item)
        {
            item.transform.SetParent(slot.transform, true);   // il pezzo ora segue il pupazzo
            if (IsFinished || resolving) return;
            foreach (DropSlot s in slots) if (s.Occupant == null) return;
            resolving = true;
            StartCoroutine(WinRoutine());
        }

        void OnEggTriggered(string eggId)
        {
            if (IsFinished || resolving) return;
            resolving = true;
            StartCoroutine(EggRoutine(Draggable2D.Active, eggId));
        }

        IEnumerator WinRoutine()
        {
            if (snowman != null) yield return Hop(snowman, 0.35f);
            Win();
        }

        IEnumerator EggRoutine(Draggable2D carrot, string eggId)
        {
            if (carrot != null)
            {
                carrot.enabled = false;   // lascia il dito: la carota ora è del coniglio
                Transform t = carrot.transform;
                Vector3 from = t.position;
                Vector3 to = bunny != null ? new Vector3(bunny.position.x, bunny.position.y, from.z) : from;
                Vector3 scale = t.localScale;
                for (float k = 0f; k < 1f; k += Time.deltaTime / animSeconds)
                {
                    t.position = Vector3.Lerp(from, to, k);
                    t.localScale = scale * (1f - k);
                    yield return null;
                }
                carrot.gameObject.SetActive(false);
            }
            if (bunny != null) yield return Hop(bunny, 0.25f);
            FailWithEgg(eggId);
        }

        IEnumerator Hop(Transform t, float height)
        {
            Vector3 start = t.localPosition;
            for (float k = 0f; k < 1f; k += Time.deltaTime / animSeconds)
            {
                t.localPosition = start + Vector3.up * (Mathf.Sin(k * Mathf.PI) * height);
                yield return null;
            }
            t.localPosition = start;
        }
    }
}
