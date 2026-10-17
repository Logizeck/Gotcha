using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Toni, il carlino dei livelli 25, 50, 75 e 100. Pezzi in comune: pose (sdraiato / in piedi), occhi aperti,
    /// assonnati o chiusi, pupille che seguono un bersaglio (o il dito), coda che scodinzola, bocca e lingua,
    /// abbaio, corsetta a saltelli e il morso. Testa a sinistra: guardare a destra = scala x negativa.
    /// </summary>
    public class ToniPug : MonoBehaviour
    {
        [SerializeField] GameObject lieArt;
        [SerializeField] GameObject standArt;
        [SerializeField] GameObject eyesOpen;
        [SerializeField] GameObject eyesHalf;
        [SerializeField] Transform[] pupils;          // figli degli occhi aperti
        [SerializeField] Vector2[] eyeCenters;        // coordinate locali di Toni
        [SerializeField] float pupilTravel = 0.045f;
        [SerializeField] float standLift = 0.48f;
        [SerializeField] GameObject mouthOpen;
        [SerializeField] GameObject tongue;
        [SerializeField] Transform tail;
        [SerializeField] GameObject alert;
        public SpeechBubble bubble;
        public SmokeEmitter zzz;
        public SmokeEmitter hearts;
        public SmokeEmitter crumbs;
        public Vector2 mouth = new Vector2(-0.74f, 0.12f);
        public Vector2 nose = new Vector2(-0.72f, 0.27f);
        public Vector2 headCenter = new Vector2(-0.72f, 0.38f);

        /// <summary>Cosa guarda (null = il dito, se c'è).</summary>
        public Transform LookTarget { get; set; }
        public Vector3? LookPoint { get; set; }
        public float Wag { get; set; } = 0.3f;
        public bool IsStanding { get; private set; }
        public bool IsAwake { get; private set; } = true;

        float baseScale = 1f, hideBubbleAt = -1f, hop;

        /// <summary>Altezza del pavimento (y locale) su cui corre: la corsetta saltella sempre da qui.</summary>
        public float GroundY { get; set; }

        void Awake()
        {
            baseScale = Mathf.Abs(transform.localScale.x);
            GroundY = transform.localPosition.y;
            if (mouthOpen != null) mouthOpen.SetActive(false);
            if (tongue != null) tongue.SetActive(false);
            if (alert != null) alert.SetActive(false);
            Stand(false);
        }

        public Vector3 MouthWorld => transform.TransformPoint(mouth);
        public Vector3 NoseWorld => transform.TransformPoint(nose);
        public Vector3 HeadWorld => transform.TransformPoint(headCenter);
        public bool FacingRight => transform.localScale.x < 0f;

        /// <summary>In piedi le zampe allungano Toni verso il basso: si alza il corpo per tenere i piedi sul pavimento.</summary>
        public void Stand(bool on)
        {
            if (on != IsStanding)
            {
                float d = (on ? standLift : -standLift) * baseScale;
                transform.localPosition += new Vector3(0f, d, 0f);
                GroundY += d;
            }
            IsStanding = on;
            if (lieArt != null) lieArt.SetActive(!on);
            if (standArt != null) standArt.SetActive(on);
        }

        /// <summary>Occhi: 0 chiusi (dorme), 1 assonnati, 2 spalancati.</summary>
        public void Eyes(int open)
        {
            IsAwake = open > 0;
            if (eyesOpen != null) eyesOpen.SetActive(open == 2);
            if (eyesHalf != null) eyesHalf.SetActive(open == 1);
        }

        public void Sleep()
        {
            Eyes(0);
            Wag = 0.03f;
            if (zzz != null) zzz.Play();
        }

        public void WakeUp(int eyes = 2)
        {
            Eyes(eyes);
            if (zzz != null) zzz.Stop();
        }

        public void Alert(bool on) { if (alert != null) alert.SetActive(on); }

        public void Face(bool right)
        {
            Vector3 s = transform.localScale;
            s.x = right ? -baseScale : baseScale;
            transform.localScale = s;
        }

        /// <summary>Fumetto sopra la testa (segue Toni anche quando corre).</summary>
        public void Say(string text, float seconds = 1.3f, string sound = null)
        {
            if (bubble == null) return;
            bubble.Show(text, true);
            hideBubbleAt = Time.time + seconds;
            if (!string.IsNullOrEmpty(sound)) Sfx.Play(this, sound, 0.75f);
        }

        public void Bark(string text = "Woof!") { Say(text, 1.1f, "woof"); }

        void Update()
        {
            if (hideBubbleAt > 0f && Time.time >= hideBubbleAt)
            {
                hideBubbleAt = -1f;
                if (bubble != null) bubble.Hide();
            }
        }

        void LateUpdate()
        {
            if (bubble != null && bubble.gameObject.activeSelf)
            {
                Vector3 at = HeadWorld + new Vector3(0f, 1.05f * baseScale, 0f);
                at.z = bubble.transform.position.z;
                bubble.transform.position = at;
            }
            if (zzz != null) zzz.transform.position = HeadWorld + new Vector3(FacingRight ? -0.35f : 0.35f, 0.55f, 0f) * baseScale;
            if (tail != null) tail.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(Time.time * (6f + Wag * 20f)) * 25f * Wag);
            if (pupils == null || eyeCenters == null) return;
            Vector3? look = LookPoint;
            if (LookTarget != null && LookTarget.gameObject.activeInHierarchy) look = LookTarget.position;
            if (!look.HasValue && PointerInput.TryGetPrimary(out Vector2 sp, out _, out _) && Camera.main != null) look = PointerInput.ToWorld(Camera.main, sp);
            for (int i = 0; i < pupils.Length && i < eyeCenters.Length; i++)
            {
                Vector2 c = eyeCenters[i];
                Vector2 to = look.HasValue ? (Vector2)transform.InverseTransformPoint(look.Value) - c : Vector2.down * 0.2f;
                Vector2 off = to.sqrMagnitude > 0.0001f ? to.normalized * Mathf.Min(pupilTravel, to.magnitude * 0.6f) : Vector2.zero;
                Vector3 want = transform.TransformPoint(c + off);
                want.z = pupils[i].position.z;
                pupils[i].position = Vector3.Lerp(pupils[i].position, want, 1f - Mathf.Exp(-20f * Time.deltaTime));
            }
        }

        /// <summary>Di corsa fino a x (coordinate locali del genitore), a saltelli, girandosi dalla parte giusta.</summary>
        public IEnumerator RunTo(float x, float speed = 3.2f)
        {
            Stand(true);
            Vector3 p = transform.localPosition;
            float y0 = GroundY;
            Face(x > p.x);
            Wag = 1f;
            while (Mathf.Abs(transform.localPosition.x - x) > 0.02f)
            {
                hop += Time.deltaTime * 14f;
                p.x = Mathf.MoveTowards(p.x, x, speed * Time.deltaTime);
                p.y = y0 + Mathf.Abs(Mathf.Sin(hop)) * 0.13f;
                transform.localPosition = p;
                transform.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(hop) * 6f);
                if (Mathf.Sin(hop) * Mathf.Sin(hop - Time.deltaTime * 14f) < 0f) Sfx.Play(this, "tick", 0.35f);
                yield return null;
            }
            p.y = y0;
            transform.localPosition = p;
            transform.localRotation = Quaternion.identity;
        }

        /// <summary>Saltello sul posto (felicità o spavento).</summary>
        public IEnumerator Jump(float height = 0.45f, float seconds = 0.35f)
        {
            Vector3 p = transform.localPosition;
            for (float k = 0f; k < 1f; k += Time.deltaTime / seconds)
            {
                transform.localPosition = p + Vector3.up * Mathf.Sin(k * Mathf.PI) * height;
                yield return null;
            }
            transform.localPosition = p;
        }

        /// <summary>Addenta un oggetto: bocca spalancata, GNAM, tre morsi scuotendo la testa; l'oggetto sparisce.</summary>
        public IEnumerator Chomp(Transform item, bool swallow = true)
        {
            Vector3 s0 = transform.localScale;
            if (mouthOpen != null) mouthOpen.SetActive(true);
            Vector3 from = item.position;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.2f)
            {
                Vector3 m = MouthWorld;
                m.z = from.z;
                item.position = Vector3.Lerp(from, m, k);
                yield return null;
            }
            if (mouthOpen != null) mouthOpen.SetActive(false);
            Sfx.Play(this, "bonk", 0.9f);
            Haptics.Light();
            Say("GNAM!", 0.8f);
            if (crumbs != null) { crumbs.transform.position = MouthWorld; crumbs.Play(); }
            Vector3 itemScale = item.localScale;
            for (int bite = 0; bite < 3; bite++)
            {
                for (float k = 0f; k < 1f; k += Time.deltaTime / 0.24f)
                {
                    float shake = Mathf.Sin(k * Mathf.PI * 2f) * 7f;
                    transform.localRotation = Quaternion.Euler(0, 0, shake);
                    if (mouthOpen != null) mouthOpen.SetActive(k > 0.3f && k < 0.55f);
                    Vector3 m = MouthWorld;
                    m.z = item.position.z;
                    item.position = m;
                    yield return null;
                }
                Sfx.Play(this, "tick", 1f);
                if (swallow) item.localScale = itemScale * (1f - (bite + 1) * 0.28f);
            }
            transform.localRotation = Quaternion.identity;
            if (crumbs != null) crumbs.Stop();
            if (swallow)
            {
                item.gameObject.SetActive(false);
                item.localScale = itemScale;
                for (float k = 0f; k < 1f; k += Time.deltaTime / 0.22f)
                {
                    transform.localScale = new Vector3(s0.x, s0.y * (1f + Mathf.Sin(k * Mathf.PI) * 0.08f), s0.z);
                    yield return null;
                }
                transform.localScale = s0;
                yield return Lick(2);
            }
        }

        public IEnumerator Lick(int times)
        {
            if (tongue == null) yield break;
            for (int i = 0; i < times; i++)
            {
                tongue.SetActive(true);
                yield return new WaitForSeconds(0.18f);
                tongue.SetActive(false);
                yield return new WaitForSeconds(0.12f);
            }
        }

        public void MouthOpen(bool on) { if (mouthOpen != null) mouthOpen.SetActive(on); }
        public void TongueOut(bool on) { if (tongue != null) tongue.SetActive(on); }

        public IEnumerator Happy(float seconds = 1.2f)
        {
            Wag = 1f;
            if (hearts != null) { hearts.transform.position = HeadWorld + Vector3.up * 0.5f; hearts.Play(); }
            yield return Jump(0.3f, 0.3f);
            yield return new WaitForSeconds(Mathf.Max(0f, seconds - 0.3f));
            if (hearts != null) hearts.Stop();
        }
    }
}
