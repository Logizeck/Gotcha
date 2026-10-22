using System.Collections;
using UnityEngine;

namespace Gotcha.Components
{
    /// <summary>
    /// Fumetto con testo (TextMesh figlio). Show() lo fa apparire con un "pop". Il fumetto non ha una misura fissa:
    /// il disegno (figlio "Art", sprite 9-slice "bubble_box") viene costruito della misura del testo, che va a capo
    /// sulle parole se è troppo lungo. La punta della codina resta ferma (indica chi parla) e il fumetto cresce verso
    /// l'alto e verso destra; se esce dallo schermo si sposta quanto basta per rientrare.
    /// </summary>
    public class SpeechBubble : MonoBehaviour
    {
        public TextMesh text;
        [Tooltip("Larghezza massima di una riga di testo (coordinate del fumetto): oltre si va a capo.")]
        public float maxTextWidth = 2.4f;
        [Tooltip("Non più usato: l'altezza del fumetto segue il testo.")]
        public float maxTextHeight = 0.85f;
        [Tooltip("Non più usato.")]
        public float bodyOffset = 0.12f;
        public float popSeconds = 0.25f;
        [Tooltip("Fumetto di un gatto: quando parla si sente il verso (miagolio, soffio, fusa...).")]
        public bool catVoice;

        // geometria dello sprite "bubble_box" (DoodleSprites.BubbleBox), in unità, rispetto ai bordi del rettangolo
        const float BodyInsetX = 0.05f, BodyInsetTop = 0.05f, BodyInsetBottom = 0.30f;   // corpo dentro il rettangolo
        const float PadX = 0.24f, PadY = 0.16f;                                           // margine del testo nel corpo
        const float TipFromLeft = 0.2f, TipFromBottom = 0.03f;                            // punta della codina
        const float MinW = 1.0f, MinH = 0.92f;                                            // somma dei bordi 9-slice
        static readonly Vector2 TipAnchor = new Vector2(-0.85f, -0.8f);                   // dove punta la codina (come il vecchio fumetto)

        static AudioSource voice;   // sorgente unica e persistente: il fumetto si nasconde ma il verso non si interrompe

        Vector3 baseScale = Vector3.one;
        Vector3 textBaseScale = Vector3.one;
        bool initialized;
        Vector3 appliedShift;   // spostamento fatto per rientrare nello schermo (si annulla al fumetto successivo)

        Transform art;
        SpriteRenderer artSr;
        Vector3 artBaseScale = Vector3.one, artBasePos;
        Vector2 artBaseSize;
        float textBaseY;
        bool artInit;

        bool showing;

        /// <summary>All'avvio del livello un fumetto senza testo non si vede: compare solo quando qualcuno parla.</summary>
        void Awake()
        {
            if (!showing && (text == null || string.IsNullOrEmpty(text.text))) gameObject.SetActive(false);
        }

        void Init()
        {
            if (appliedShift != Vector3.zero) { transform.position -= appliedShift; appliedShift = Vector3.zero; }
            if (initialized) return;
            initialized = true;
            baseScale = transform.localScale;
            if (text != null) textBaseScale = text.transform.localScale;
        }

        void InitArt()
        {
            if (artInit) return;
            artInit = true;
            art = transform.Find("Art");
            if (art != null)
            {
                artBaseScale = art.localScale;
                artBasePos = art.localPosition;
                artSr = art.GetComponent<SpriteRenderer>();
                if (artSr != null) artBaseSize = artSr.size;
            }
            if (text != null) textBaseY = text.transform.localPosition.y;
        }

        bool Sliced => artSr != null && artSr.drawMode != SpriteDrawMode.Simple && artSr.sprite != null && artSr.sprite.border != Vector4.zero;

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
            showing = true;   // Awake (alla prima attivazione) non deve rinasconderlo
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
            // si misura a fumetto quasi invisibile (le misure relative restano esatte)
            transform.localScale = baseScale * 0.001f;
            if (text == null) { yield return Pop(); yield break; }
            text.transform.localScale = textBaseScale;
            Vector2 before = LocalSize();
            text.text = message;
            yield return Measure(before);

            // 1) a capo sulle parole se la riga è troppo lunga
            if (measured.x > maxTextWidth && message.Length > 1)
            {
                before = LocalSize();
                text.text = Wrap(message, maxTextWidth);
                yield return Measure(before);
            }

            // 2) il testo si rimpicciolisce solo se il fumetto diventerebbe più largo dello schermo
            Vector2 size = measured;
            float maxW = MaxLocalWidth();
            float needW = size.x + 2f * (BodyInsetX + PadX);
            float ts = needW > maxW ? Mathf.Max(0.3f, (maxW - 2f * (BodyInsetX + PadX)) / Mathf.Max(size.x, 1e-4f)) : 1f;
            text.transform.localScale = textBaseScale * ts;
            size *= ts;

            // 3) il fumetto della misura del testo, poi verifica sul testo disegnato davvero (il font disegna le lettere
            //    più in alto della riga "teorica": si centra il disegno vero e, se serve, si allarga ancora)
            Layout(size);
            for (int pass = 0; pass < 4 && Sliced; pass++)
            {
                if (!FitCheck()) break;
            }
            yield return Pop();
            KeepOnScreen();
        }

        /// <summary>Costruisce il disegno attorno a un testo di queste dimensioni (coordinate del fumetto).</summary>
        void Layout(Vector2 textSize)
        {
            if (art == null) return;
            if (Sliced)
            {
                float w = Mathf.Max(MinW, textSize.x + 2f * (BodyInsetX + PadX));
                float h = Mathf.Max(MinH, textSize.y + BodyInsetTop + BodyInsetBottom + 2f * PadY);
                art.localScale = Vector3.one;
                artSr.size = new Vector2(w, h);
                // la punta della codina resta dov'era: il fumetto cresce verso l'alto e verso destra
                Vector2 tip = new Vector2(-w * 0.5f + TipFromLeft, -h * 0.5f + TipFromBottom);
                Vector2 center = TipAnchor - tip;
                art.localPosition = new Vector3(center.x, center.y, artBasePos.z);
                // centro del corpo: fra il bordo alto e la base del corpo (sotto c'è la codina)
                float bodyCy = center.y + ((h * 0.5f - BodyInsetTop) + (-h * 0.5f + BodyInsetBottom)) * 0.5f;
                Vector3 tp = text.transform.localPosition;
                text.transform.localPosition = new Vector3(center.x, bodyCy, tp.z);
            }
            else
            {
                // vecchio fumetto non 9-slice: si allarga lo sprite (margini abbondanti)
                SpriteRenderer sr = art.GetComponent<SpriteRenderer>();
                Vector2 spriteSize = sr != null && sr.sprite != null ? (Vector2)sr.sprite.bounds.size : new Vector2(3.1f, 1.57f);
                float gx = Mathf.Max(1f, textSize.x * 1.3f / (spriteSize.x * 0.8f)), gy = Mathf.Max(1f, textSize.y * 1.35f / (spriteSize.y * 0.6f));
                art.localScale = new Vector3(artBaseScale.x * gx, artBaseScale.y * gy, artBaseScale.z);
                art.localPosition = artBasePos;
                Vector3 tp = text.transform.localPosition;
                text.transform.localPosition = new Vector3(tp.x, textBaseY + (gy - 1f) * 0.12f, tp.z);
            }
        }

        Vector2 measured;

        /// <summary>
        /// Misura il testo quando il suo mesh è davvero aggiornato: Unity lo rigenera con qualche fotogramma di
        /// ritardo. Si aspetta che la misura cambi rispetto al testo di prima (o al massimo 6 fotogrammi, se il nuovo
        /// testo ha la stessa misura) e poi che resti uguale per due fotogrammi di fila.
        /// </summary>
        IEnumerator Measure(Vector2 before)
        {
            Vector2 last = new Vector2(-1f, -1f);
            bool changed = false;
            for (int i = 0; i < 20; i++)
            {
                yield return null;
                Vector2 now = LocalSize();
                if (!changed)
                {
                    if ((now - before).sqrMagnitude > 1e-6f || i >= 5) changed = true;
                    else continue;
                }
                if (now.x > 0f && (now - last).sqrMagnitude < 1e-6f) { measured = now; yield break; }
                last = now;
            }
            measured = last;
        }

        /// <summary>
        /// Controlla il testo disegnato rispetto al corpo del fumetto: lo centra (spostandolo di quanto il disegno
        /// delle lettere è fuori centro) e, se non ci sta, allarga il fumetto. Restituisce true se ha cambiato qualcosa.
        /// </summary>
        bool FitCheck()
        {
            MeshRenderer mr = text.GetComponent<MeshRenderer>();
            if (mr == null || artSr == null) return false;
            TextBox(out Vector2 tc, out Vector2 tsz);
            float w = artSr.size.x, h = artSr.size.y;
            Vector2 c = art.localPosition;
            float bodyCy = c.y + ((h * 0.5f - BodyInsetTop) + (-h * 0.5f + BodyInsetBottom)) * 0.5f;
            float innerW = w - 2f * (BodyInsetX + PadX), innerH = h - BodyInsetTop - BodyInsetBottom - 2f * PadY;
            if (tsz.x > innerW + 0.01f || tsz.y > innerH + 0.01f)
            {
                // più grande: si rifà il fumetto sul testo misurato adesso (il centraggio al giro dopo)
                Layout(new Vector2(Mathf.Max(tsz.x, innerW), Mathf.Max(tsz.y, innerH)));
                return true;
            }
            bool changed = false;
            Vector2 off = new Vector2(c.x - tc.x, bodyCy - tc.y);
            if (off.sqrMagnitude > 1e-4f)
            {
                text.transform.localPosition += new Vector3(off.x, off.y, 0f);
                changed = true;
            }
            return changed;
        }

        /// <summary>Larghezza massima del fumetto (coordinate del fumetto) per restare nello schermo.</summary>
        float MaxLocalWidth()
        {
            Camera cam = Camera.main;
            if (cam == null || !cam.orthographic) return 100f;
            float screenW = cam.orthographicSize * 2f * cam.aspect * 0.94f;
            float s = Mathf.Abs(baseScale.x) * ParentScaleX();
            return screenW / Mathf.Max(s, 1e-4f);
        }

        float ParentScaleX() => transform.parent != null ? Mathf.Abs(transform.parent.lossyScale.x) : 1f;

        /// <summary>Se il fumetto esce dallo schermo, lo si sposta quanto basta per farlo rientrare.</summary>
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
            TextBox(out _, out Vector2 size);
            return size;
        }

        /// <summary>
        /// Centro e dimensioni del testo disegnato nelle coordinate del fumetto. Si usano i limiti LOCALI del testo: quelli
        /// "del mondo" sono allineati allo schermo e, con il fumetto ruotato (es. telefono girato), scambierebbero
        /// larghezza e altezza, e il fumetto verrebbe costruito nell'orientamento sbagliato.
        /// </summary>
        void TextBox(out Vector2 center, out Vector2 size)
        {
            MeshRenderer mr = text.GetComponent<MeshRenderer>();
#if UNITY_2021_2_OR_NEWER
            Bounds lb = mr.localBounds;
            Transform tt = text.transform;
            // il testo è figlio diretto del fumetto: basta la sua scala locale
            size = new Vector2(lb.size.x * Mathf.Abs(tt.localScale.x), lb.size.y * Mathf.Abs(tt.localScale.y));
            center = transform.InverseTransformPoint(tt.TransformPoint(lb.center));
#else
            Bounds b = mr.bounds;
            Vector3 l = transform.lossyScale;
            size = new Vector2(b.size.x / Mathf.Max(Mathf.Abs(l.x), 1e-6f), b.size.y / Mathf.Max(Mathf.Abs(l.y), 1e-6f));
            center = transform.InverseTransformPoint(b.center);
#endif
        }

        /// <summary>A capo sulle parole: la larghezza di ogni parola è stimata dalla larghezza media misurata.</summary>
        string Wrap(string message, float maxWidth)
        {
            string[] lines = text.text.Split('\n');
            int longest = 1;
            foreach (string l in lines) longest = Mathf.Max(longest, l.Length);
            float perChar = measured.x / longest * 1.05f;   // un filo di margine: le maiuscole sono più larghe
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
    }
}
