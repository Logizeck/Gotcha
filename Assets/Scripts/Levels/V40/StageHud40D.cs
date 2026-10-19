using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// HUD dei livelli compositi del gruppo D (v0.40): pallini delle fasi in alto, riga con l'obiettivo della fase
    /// e una scritta grande per il passaggio da una fase all'altra.
    /// </summary>
    public class StageHud40D : MonoBehaviour
    {
        [SerializeField] SpriteRenderer[] dots;
        [SerializeField] TextMesh goal;
        [SerializeField] Transform bannerRoot;       // scritta grande + fondino
        [SerializeField] TextMesh banner;
        [SerializeField] Color doneColor = new Color(0.36f, 0.8f, 0.42f), currentColor = new Color(1f, 0.82f, 0.25f), todoColor = new Color(1f, 1f, 1f, 0.35f);

        int current = -1;
        float punch;

        public int Current => current;

        void Awake()
        {
            if (bannerRoot != null) bannerRoot.gameObject.SetActive(false);
            SetStage(0);
        }

        /// <summary>Fase corrente (le precedenti diventano verdi).</summary>
        public void SetStage(int stage)
        {
            current = stage;
            if (dots == null) return;
            for (int i = 0; i < dots.Length; i++)
            {
                if (dots[i] == null) continue;
                dots[i].color = i < stage ? doneColor : i == stage ? currentColor : todoColor;
                dots[i].transform.localScale = Vector3.one * (i == stage ? 1.15f : 1f) * 0.32f;
            }
        }

        public void Goal(string text)
        {
            if (goal == null) return;
            if (goal.text == text) return;
            goal.text = text;
            punch = 1f;
        }

        /// <summary>Aggiorna il testo senza il "colpetto" (per i conti alla rovescia).</summary>
        public void GoalQuiet(string text) { if (goal != null) goal.text = text; }

        /// <summary>Scritta grande che compare e sparisce (passaggio di fase).</summary>
        public IEnumerator Banner(string text, float seconds = 1.3f)
        {
            if (banner == null || bannerRoot == null) yield break;
            banner.text = text;
            Transform t = bannerRoot;
            t.gameObject.SetActive(true);
            Sfx.Play(this, "ding", 0.6f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.25f)
            {
                t.localScale = Vector3.one * (Mathf.Lerp(0.3f, 1f, k) + Mathf.Sin(k * Mathf.PI) * 0.15f);
                yield return null;
            }
            t.localScale = Vector3.one;
            yield return new WaitForSeconds(Mathf.Max(0.1f, seconds - 0.45f));
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.2f) { t.localScale = Vector3.one * (1f - k); yield return null; }
            t.gameObject.SetActive(false);
            t.localScale = Vector3.one;
        }

        void Update()
        {
            float pulse = 1.15f + Mathf.Sin(Time.time * 6f) * 0.08f;
            if (dots != null && current >= 0 && current < dots.Length && dots[current] != null)
                dots[current].transform.localScale = Vector3.one * pulse * 0.32f;
            if (goal != null && punch > 0f)
            {
                punch = Mathf.Max(0f, punch - Time.deltaTime * 4f);
                goal.transform.localScale = Vector3.one * (1f + punch * 0.18f);
            }
        }
    }
}
