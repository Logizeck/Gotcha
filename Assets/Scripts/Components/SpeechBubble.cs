using System.Collections;
using UnityEngine;

namespace Gotcha.Components
{
    /// <summary>Fumetto con testo (TextMesh figlio). Show() lo fa apparire con un "pop"; il testo si adatta alla larghezza.</summary>
    public class SpeechBubble : MonoBehaviour
    {
        public TextMesh text;
        public float maxTextWidth = 2.4f;
        [Tooltip("Altezza utile dentro il fumetto (coordinate del fumetto).")]
        public float maxTextHeight = 0.85f;
        [Tooltip("Quanto si sposta in su il centro del corpo quando il fumetto si allunga in altezza.")]
        public float bodyOffset = 0.12f;
        public float popSeconds = 0.25f;
        [Tooltip("Fumetto di un gatto: quando parla si sente il verso (miagolio, soffio, fusa...).")]
        public bool catVoice;

        static AudioSource voice;   // sorgente unica e persistente: il fumetto si nasconde ma il verso non si interrompe

        Vector3 baseScale = Vector3.one;
        Vector3 textBaseScale = Vector3.one;
        bool initialized;

        Vector3 appliedShift;   // spostamento fatto per rientrare nello schermo (si annulla al fumetto successivo)

        void Init()
        {
            if (appliedShift != Vector3.zero) { transform.position -= appliedShift; appliedShift = Vector3.zero; }
            if (initialized) return;
            initialized = true;
            baseScale = transform.localScale;
            if (text != null) textBaseScale = text.transform.localScale;
        }

        /// <summary>Mostra il fumetto. Con <paramref name="silent"/> il gatto non fa il verso (es. parla la vespa).</summary>
        public void Show(string message, bool silent = false)
        {
            Init();
            if (catVoice && !silent && !string.IsNullOrEmpty(message))
            {
                if (voice == null)
                {
                    var go = new GameObject("CatVoice");
                    DontDestroyOnLoad(go);
                    voice = go.AddComponent<AudioSource>();
                    voice.playOnAwake = false;
                }
                voice.pitch = Random.Range(0.93f, 1.08f);
                voice.PlayOneShot(CatVoice.For(message), 0.9f);
            }
            gameObject.SetActive(true);
            StopAllCoroutines();
            StartCoroutine(ShowRoutine(message));
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        IEnumerator ShowRoutine(string message)
        {
            InitArt();
            // si misura il testo a fumetto quasi invisibile (le misure relative restano esatte)
            transform.localScale = baseScale * 0.001f;
            if (art != null) { art.localScale = artBaseScale; art.localPosition = artBasePos; }
            if (text == null) { yield return Pop(); yield break; }
            text.transform.localScale = textBaseScale;
            text.text = message;
            yield return null;   // il mesh del testo è pronto dal frame successivo

            // 1) va a capo sulle parole se la riga è troppo lunga
            Vector2 size = LocalSize();
            if (size.x > maxTextWidth && message.Length > 1)
            {
                text.text = Wrap(message, size.x, maxTextWidth);
                yield return null;
                size = LocalSize();
            }

            // 2) se non entra ancora, si allarga il fumetto (il testo resta grande e leggibile); il testo si
            //    rimpicciolisce solo se il fumetto diventerebbe più largo dello schermo
            float maxGrowX = MaxGrowX();
            float ts = Mathf.Min(1f, maxGrowX * maxTextWidth / Mathf.Max(size.x, 0.0001f));
            text.transform.localScale = textBaseScale * ts;
            if (art != null)
            {
                float gx = Mathf.Max(1f, size.x * ts / maxTextWidth * 1.08f), gy = Mathf.Max(1f, size.y * ts / maxTextHeight * 1.1f);
                art.localScale = new Vector3(artBaseScale.x * gx, artBaseScale.y * gy, artBaseScale.z);
                // il corpo del fumetto sta un po' sopra il centro del disegno (sotto c'è la codina)
                Vector3 tp = text.transform.localPosition;
                text.transform.localPosition = new Vector3(tp.x, textBaseY + (gy - 1f) * bodyOffset, tp.z);
            }
            yield return Pop();
            KeepOnScreen();
        }

        /// <summary>Di quanto può allargarsi il fumetto prima di uscire dallo schermo (in larghezza).</summary>
        float MaxGrowX()
        {
            Camera cam = Camera.main;
            if (cam == null || !cam.orthographic || art == null) return 3f;
            SpriteRenderer sr = art.GetComponent<SpriteRenderer>();
            if (sr == null || sr.sprite == null) return 3f;
            float screenW = cam.orthographicSize * 2f * cam.aspect * 0.94f;
            float artW = sr.sprite.bounds.size.x * Mathf.Abs(artBaseScale.x * baseScale.x) * ParentScaleX();
            return Mathf.Max(1f, screenW / Mathf.Max(artW, 0.0001f));
        }

        float ParentScaleX() => transform.parent != null ? Mathf.Abs(transform.parent.lossyScale.x) : 1f;

        /// <summary>Se il fumetto ingrandito esce dallo schermo, lo si sposta quanto basta per farlo rientrare.</summary>
        void KeepOnScreen()
        {
            Camera cam = Camera.main;
            if (cam == null || !cam.orthographic || art == null) return;
            SpriteRenderer sr = art.GetComponent<SpriteRenderer>();
            if (sr == null) return;
            Bounds b = sr.bounds;
            float h = cam.orthographicSize, w = h * cam.aspect;
            Vector3 c = cam.transform.position;
            Vector3 shift = Vector3.zero;
            if (b.size.x < w * 2f)
            {
                if (b.min.x < c.x - w) shift.x = (c.x - w) - b.min.x + 0.05f;
                else if (b.max.x > c.x + w) shift.x = (c.x + w) - b.max.x - 0.05f;
            }
            if (b.size.y < h * 2f)
            {
                if (b.max.y > c.y + h) shift.y = (c.y + h) - b.max.y - 0.05f;
                else if (b.min.y < c.y - h) shift.y = (c.y - h) - b.min.y + 0.05f;
            }
            if (shift != Vector3.zero) { transform.position += shift; appliedShift = shift; }
        }

        IEnumerator Pop()
        {
            for (float k = 0f; k < 1f; k += Time.deltaTime / popSeconds)
            {
                float s = 1f + Mathf.Sin(k * Mathf.PI) * 0.25f;   // piccolo rimbalzo
                transform.localScale = baseScale * (k * s);
                yield return null;
            }
            transform.localScale = baseScale;
        }

        /// <summary>Dimensioni del testo nelle coordinate del fumetto (indipendenti dalla scala del fumetto).</summary>
        Vector2 LocalSize()
        {
            Vector3 b = text.GetComponent<MeshRenderer>().bounds.size;
            Vector3 l = transform.lossyScale;
            return new Vector2(b.x / Mathf.Max(Mathf.Abs(l.x), 1e-6f), b.y / Mathf.Max(Mathf.Abs(l.y), 1e-6f));
        }

        /// <summary>A capo sulle parole: la larghezza di ogni parola è stimata in proporzione ai caratteri.</summary>
        static string Wrap(string message, float lineWidth, float maxWidth)
        {
            string flat = message.Replace("\n", " ");
            float perChar = lineWidth / Mathf.Max(1, flat.Length);
            string[] words = flat.Split(' ');
            var sb = new System.Text.StringBuilder();
            float x = 0f;
            foreach (string w in words)
            {
                if (w.Length == 0) continue;
                float ww = w.Length * perChar, space = perChar;
                if (x > 0f && x + space + ww > maxWidth) { sb.Append('\n'); x = 0f; }
                else if (x > 0f) { sb.Append(' '); x += space; }
                sb.Append(w);
                x += ww;
            }
            return sb.ToString();
        }

        Transform art;
        Vector3 artBaseScale = Vector3.one, artBasePos;
        float textBaseY;
        bool artInit;

        void InitArt()
        {
            if (artInit) return;
            artInit = true;
            art = transform.Find("Art");
            if (art != null) { artBaseScale = art.localScale; artBasePos = art.localPosition; }
            if (text != null) textBaseY = text.transform.localPosition.y;
        }
    }
}
