using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 4 — taglia la mela col dito. Un taglio deciso che attraversa la mela la divide in due metà
    /// (orientate come il taglio) che si separano e cadono sul tagliere, con qualche goccia di succo.
    /// </summary>
    public class AppleLevel : LevelController
    {
        [SerializeField] SwipeCutter cutter;
        [SerializeField] GameObject wholeApple;
        [SerializeField] Transform halves;          // contiene le due metà, tagliate lungo il loro asse verticale locale
        [SerializeField] Rigidbody2D leftHalf;
        [SerializeField] Rigidbody2D rightHalf;
        [SerializeField] SmokeEmitter juice;
        [SerializeField] float pushForce = 2.2f;

        public override void OnLevelStart()
        {
            if (halves != null) halves.gameObject.SetActive(false);
            if (cutter != null) cutter.Cut += OnCut;
        }

        void OnDestroy()
        {
            if (cutter != null) cutter.Cut -= OnCut;
        }

        void OnCut(Vector2 entry, Vector2 exit)
        {
            if (IsFinished) return;
            StartCoroutine(CutRoutine(entry, exit));
        }

        IEnumerator CutRoutine(Vector2 entry, Vector2 exit)
        {
            Vector2 dir = (exit - entry).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;   // l'asse del taglio delle metà è "su"
            if (angle < -180f) angle += 360f;
            if (Mathf.Abs(angle) > 90f) angle += 180f;                         // non capovolgere la mela

            if (wholeApple != null) wholeApple.SetActive(false);
            if (halves != null)
            {
                halves.rotation = Quaternion.Euler(0, 0, angle);
                halves.gameObject.SetActive(true);
                Vector2 normal = halves.right;
                if (leftHalf != null)
                {
                    leftHalf.AddForce(-normal * pushForce + Vector2.up * 1.2f, ForceMode2D.Impulse);
                    leftHalf.AddTorque(0.6f, ForceMode2D.Impulse);
                }
                if (rightHalf != null)
                {
                    rightHalf.AddForce(normal * pushForce + Vector2.up * 1.2f, ForceMode2D.Impulse);
                    rightHalf.AddTorque(-0.6f, ForceMode2D.Impulse);
                }
            }
            if (juice != null)
            {
                juice.transform.position = (entry + exit) * 0.5f;
                juice.Play();
                yield return new WaitForSeconds(0.35f);
                juice.Stop();
            }
            yield return new WaitForSeconds(1.2f);
            Win();
        }
    }
}
