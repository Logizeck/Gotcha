using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Barra delle fasi dei livelli compositi (gruppo A della 0.40): riga dell'obiettivo in alto, pallini delle fasi
    /// (fatte = verdi, attuale = gialla che pulsa), cuori dei tentativi, scritta di fase superata e dissolvenza in nero.
    /// </summary>
    public class StageHud40A : MonoBehaviour
    {
        [SerializeField] TextMesh goal;               // dentro un contenitore (che pulsa)
        [SerializeField] SpriteRenderer[] dots;
        [SerializeField] SpriteRenderer[] tries;
        [SerializeField] TextMesh banner;             // dentro un contenitore (che salta) con lo sfondo
        [SerializeField] SpriteRenderer fade;

        static readonly Color DoneColor = new Color(0.45f, 0.86f, 0.45f), NowColor = new Color(1f, 0.83f, 0.3f), LaterColor = new Color(1f, 1f, 1f, 0.35f);

        int current = -1, triesLeft;
        Vector3 goalScale = Vector3.one;

        public int TriesLeft => triesLeft;

        void Awake()
        {
            if (goal != null) goalScale = goal.transform.parent.localScale;
            if (banner != null) banner.transform.parent.gameObject.SetActive(false);
            if (fade != null) { fade.color = new Color(0f, 0f, 0f, 0f); fade.enabled = false; }
            triesLeft = tries != null ? tries.Length : 0;
            Paint();
        }

        /// <summary>Inizia la fase i (da 0) con la sua riga d'obiettivo.</summary>
        public void Stage(int i, string text)
        {
            current = i;
            Paint();
            Goal(text);
        }

        /// <summary>Cambia la riga dell'obiettivo con un piccolo "pop".</summary>
        public void Goal(string text)
        {
            if (goal == null) return;
            goal.text = text;
            StopCoroutine(nameof(PopGoal));
            StartCoroutine(nameof(PopGoal));
        }

        IEnumerator PopGoal()
        {
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.25f)
            {
                goal.transform.parent.localScale = goalScale * (1f + Mathf.Sin(k * Mathf.PI) * 0.15f);
                yield return null;
            }
            goal.transform.parent.localScale = goalScale;
        }

        void Paint()
        {
            if (dots == null) return;
            for (int i = 0; i < dots.Length; i++) dots[i].color = i < current ? DoneColor : i == current ? NowColor : LaterColor;
        }

        void Update()
        {
            if (dots == null || current < 0 || current >= dots.Length) return;
            dots[current].transform.localScale = Vector3.one * (1f + Mathf.Sin(Time.time * 6f) * 0.12f);
        }

        /// <summary>Fase superata: pallino verde, scritta che salta, campanello. Da aspettare con yield.</summary>
        public IEnumerator Clear(string message)
        {
            if (dots != null && current >= 0 && current < dots.Length) { dots[current].color = DoneColor; dots[current].transform.localScale = Vector3.one * 1.25f; }
            current++;
            Sfx.Play(this, "ding", 0.6f);
            Haptics.Light();
            if (banner != null)
            {
                banner.text = message;
                Transform t = banner.transform.parent;   // si scala il contenitore: l'etichetta si adatta da sola al testo
                t.gameObject.SetActive(true);
                for (float k = 0f; k < 1f; k += Time.deltaTime / 1.1f)
                {
                    float s = k < 0.2f ? Mathf.Lerp(0.3f, 1.15f, k / 0.2f) : k < 0.3f ? Mathf.Lerp(1.15f, 1f, (k - 0.2f) / 0.1f) : 1f;
                    t.localScale = Vector3.one * s;
                    t.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(k * 12f) * 4f * (1f - k));
                    Color c = banner.color; c.a = k > 0.8f ? 1f - (k - 0.8f) / 0.2f : 1f; banner.color = c;
                    yield return null;
                }
                t.gameObject.SetActive(false);
            }
            else yield return new WaitForSeconds(0.8f);
            if (dots != null) foreach (SpriteRenderer d in dots) d.transform.localScale = Vector3.one;
            Paint();
        }

        /// <summary>Toglie un tentativo (il cuore trema e si spegne). Restituisce quanti ne restano.</summary>
        public int LoseTry()
        {
            if (triesLeft <= 0) return 0;
            triesLeft--;
            if (tries != null && triesLeft < tries.Length) StartCoroutine(HeartOut(tries[triesLeft]));
            return triesLeft;
        }

        IEnumerator HeartOut(SpriteRenderer h)
        {
            Vector3 p = h.transform.localPosition, s = h.transform.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.5f)
            {
                h.transform.localPosition = p + new Vector3(Mathf.Sin(k * 50f) * 0.05f, 0f, 0f);
                h.transform.localScale = s * (1f + (1f - k) * 0.3f);
                yield return null;
            }
            h.transform.localPosition = p;
            h.transform.localScale = s;
            h.color = new Color(0.2f, 0.2f, 0.25f, 0.45f);
        }

        /// <summary>Dissolvenza verso il nero (to = 1) o dal nero (to = 0).</summary>
        public IEnumerator Fade(float to, float seconds)
        {
            if (fade == null) yield break;
            fade.enabled = true;
            float from = fade.color.a;
            for (float k = 0f; k < 1f; k += Time.deltaTime / seconds)
            {
                fade.color = new Color(0f, 0f, 0f, Mathf.Lerp(from, to, k));
                yield return null;
            }
            fade.color = new Color(0f, 0f, 0f, to);
            if (to <= 0f) fade.enabled = false;
        }
    }
}
