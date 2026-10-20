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
            text.transform.localPosition = new Vector3(text.transform.localPosition.x, textBaseY, text.transform.localPosition.z);
            text.text = message;
            yield return Settle();   // il mesh del testo si aggiorna con qualche fotogramma di ritardo

            // 1) va a capo sulle parole se la riga è troppo lunga
            Vector2 size = LocalSize();
            if (size.x > maxTextWidth && message.Length > 1)
            {
                text.text = Wrap(message, maxTextWidth);
                yield return Settle();
                size = LocalSize();
            }

            // 2) se non entra ancora, si allarga il fumetto (il testo resta grande e leggibile); il testo si
            //    rimpicciolisce solo se il fumetto diventerebbe più largo dello schermo
            float maxGrowX = MaxGrowX();
            float ts = Mathf.Min(1f, maxGrowX * maxTextWidth / Mathf.Max(size.x * 1.15f, 0.0001f));
            text.transform.localScale = textBaseScale * ts;
            Grow(size * ts, 1.15f, 1.2f);

            // 3) verifica sul fumetto vero: finché il testo esce dal corpo del fumetto, lo si allarga ancora
            for (int pass = 0; pass < 3 && art != null; pass++)
            {
                yield return null;
                Vector2 over = Overflow();
                if (over.x <= 1.001f && over.y <= 1.001f) break;
                art.localScale = new Vector3(art.localScale.x * Mathf.Max(1f, over.x * 1.05f), art.localScale.y * Mathf.Max(1f, over.y * 1.05f), art.localScale.z);
                float gy = art.localScale.y / artBaseScale.y;
                Vector3 tp = text.transform.localPosition;
                text.transform.localPosition = new Vector3(tp.x, textBaseY + (gy - 1f) * bodyOffset, tp.z);
            }
            yield return Pop();
            KeepOnScreen();
        }

        static IEnumerator Settle()
        {
            for (int i = 0; i < 3; i++) yield return null;
        }

        /// <summary>Allarga il disegno del fumetto per contenere un testo di queste dimensioni (coordinate del fumetto).</summary>
        void Grow(Vector2 textSize, float marginX, float marginY)
        {
            if (art == null) return;
            float gx = Mathf.Max(1f, textSize.x / maxTextWidth * marginX), gy = Mathf.Max(1f, textSize.y / maxTextHeight * marginY);
            art.localScale = new Vector3(artBaseScale.x * gx, artBaseScale.y * gy, artBaseScale.z);
            // il corpo del fumetto sta un po' sopra il centro del disegno (sotto c'è la codina)
            Vector3 tp = text.transform.localPosition;
            text.transform.localPosition = new Vector3(tp.x, textBaseY + (gy - 1f) * bodyOffset, tp.z);
        }

        /// <summary>
        /// Quanto il testo supera il corpo del fumetto (1 = entra giusto): il corpo è il disegno senza la codina in
        /// basso e senza il bordo arrotondato.
        /// </summary>
        Vector2 Overflow()
        {
            SpriteRenderer sr = art.GetComponent<SpriteRenderer>();
            MeshRenderer mr = text.GetComponent<MeshRenderer>();
            if (sr == null || mr == null) return Vector2.one;
            Bounds a = sr.bounds, t = mr.bounds;
            if (t.size.x <= 0f) return Vector2.one;
            // disegno "bubble": corpo tra il 20% e il 94% dell'altezza (sotto c'è la codina), 90% della larghezza;
            // tolti bordo e angoli arrotondati restano circa 80% x 60%
            float bodyW = a.size.x * 0.8f, bodyH = a.size.y * 0.6f;
            float bodyCy = a.min.y + a.size.y * 0.572f;
            float needH = Mathf.Max(t.size.y, 2f * Mathf.Max(t.max.y - bodyCy, bodyCy - t.min.y));
            float needW = Mathf.Max(t.size.x, 2f * Mathf.Max(t.max.x - a.center.x, a.center.x - t.min.x));
            return new Vector2(needW / Mathf.Max(bodyW, 1e-4f), needH / Mathf.Max(bodyH, 1e-4f));
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
        string Wrap(string message, float maxWidth)
        {
            // larghezza media di un carattere misurata sul testo attuale (riga più lunga / suoi caratteri)
            string[] lines = text.text.Split('\n');
            int longest = 1;
            foreach (string l in lines) longest = Mathf.Max(longest, l.Length);
            float perChar = LocalSize().x / longest;
            string flat = message.Replace("\n", " ");
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
