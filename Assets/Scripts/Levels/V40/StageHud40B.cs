using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Barra delle fasi dei livelli compositi (gruppo B): pallini delle fasi in alto a sinistra, cuori dei tentativi in
    /// alto a destra, riga dell'obiettivo e scritta grande di passaggio da una fase all'altra.
    /// </summary>
    public class StageHud40B : MonoBehaviour
    {
        [SerializeField] SpriteRenderer[] dots;
        [SerializeField] SpriteRenderer[] hearts;
        [SerializeField] TextMesh goal;
        [SerializeField] TextMesh banner;
        [SerializeField] SpriteRenderer bannerBack;

        static readonly Color Done = new Color(0.49f, 0.85f, 0.34f), Current = new Color(1f, 0.82f, 0.25f), Todo = new Color(1f, 1f, 1f, 0.35f);

        public int TriesLeft { get; private set; }

        public void Begin(int tries)
        {
            TriesLeft = tries;
            for (int i = 0; i < hearts.Length; i++) hearts[i].gameObject.SetActive(i < tries);
            ShowBanner(false);
            SetStage(0);
        }

        public void SetStage(int index)
        {
            for (int i = 0; i < dots.Length; i++)
            {
                dots[i].color = i < index ? Done : i == index ? Current : Todo;
                dots[i].transform.localScale = Vector3.one * (i == index ? 1.25f : 1f);
            }
        }

        public void Goal(string text) { if (goal != null) goal.text = text; }

        /// <summary>Toglie un tentativo; true se non ne restano più.</summary>
        public bool LoseTry()
        {
            if (TriesLeft <= 0) return true;
            TriesLeft--;
            if (TriesLeft < hearts.Length) StartCoroutine(Drop(hearts[TriesLeft]));
            Haptics.Light();
            return TriesLeft <= 0;
        }

        IEnumerator Drop(SpriteRenderer h)
        {
            Vector3 p = h.transform.localPosition, s = h.transform.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.5f)
            {
                h.transform.localPosition = p + new Vector3(0f, -k * k * 0.8f, 0f);
                h.transform.localScale = s * (1f + Mathf.Sin(k * Mathf.PI) * 0.4f);
                h.color = new Color(1f, 1f, 1f, 1f - k);
                yield return null;
            }
            h.gameObject.SetActive(false);
            h.transform.localPosition = p;
            h.transform.localScale = s;
            h.color = Color.white;
        }

        void ShowBanner(bool on)
        {
            if (banner != null) banner.gameObject.SetActive(on);
            if (bannerBack != null) bannerBack.gameObject.SetActive(on);
        }

        /// <summary>Passaggio di fase: il pallino diventa verde, scritta grande, poi la nuova riga dell'obiettivo.</summary>
        public IEnumerator Next(int index, string title, string goalText)
        {
            SetStage(index);
            Sfx.Play(this, "ding", 0.6f);
            Goal("");
            if (banner != null)
            {
                banner.text = title;
                ShowBanner(true);
                for (float k = 0f; k < 1f; k += Time.deltaTime / 0.25f)
                {
                    banner.transform.localScale = Vector3.one * Mathf.Lerp(0.3f, 1f, 1f - (1f - k) * (1f - k));
                    yield return null;
                }
                banner.transform.localScale = Vector3.one;
                yield return new WaitForSeconds(1.1f);
                ShowBanner(false);
            }
            Goal(goalText);
        }

        /// <summary>Scritta grande finale (resta sullo schermo).</summary>
        public void Shout(string title)
        {
            if (banner == null) return;
            banner.text = title;
            banner.transform.localScale = Vector3.one;
            ShowBanner(true);
            Goal("");
        }
    }
}
