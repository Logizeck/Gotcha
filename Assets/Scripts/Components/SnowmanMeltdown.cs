using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gotcha.Components
{
    /// <summary>
    /// La "tragedia" del pupazzo di neve, uguale ovunque (Easter egg dei livelli 11 e 29):
    /// URLO (voce femminile), occhi che schizzano via in direzioni opposte, bocca a "O", cappello che salta,
    /// braccia che si agitano; poi lo scioglimento vero (SnowmanMelter) mentre le braccia salutano disperate,
    /// "I'm meltiiiing!", "blub..." e infine gli occhi che ricadono nella pozzanghera: plop, plop.
    /// </summary>
    public class SnowmanMeltdown : MonoBehaviour
    {
        public Transform snowman;
        public SnowmanMelter melter;
        public Transform hat;
        public Transform armLeft;
        public Transform armRight;
        public Transform scarf;
        public SpriteRenderer mouthO;
        public SpriteRenderer blush;
        public SpeechBubble bubble;
        public SmokeEmitter smoke;
        public AudioSource screamSource;
        public AudioClip screamClip;
        public float meltSeconds = 7f;
        public Color blushTint = new Color(1f, 0.78f, 0.83f);

        public bool Running { get; private set; }

        /// <summary>
        /// Esegue tutta la scena. <paramref name="eyes"/>: occhi da sparare via (possono mancare);
        /// <paramref name="attached"/>: altri oggetti non di neve (naso, carota...) che scendono e galleggiano.
        /// </summary>
        public IEnumerator Play(List<Transform> eyes, List<Transform> attached)
        {
            Running = true;
            if (screamSource != null) { if (screamClip == null) screamClip = ScreamSynth.Create(); screamSource.PlayOneShot(screamClip); }
            if (bubble != null) bubble.Show("AAAAAAH!!");
            if (smoke != null) smoke.Play();
            Shoot(eyes);
            if (mouthO != null && !mouthO.gameObject.activeSelf) { mouthO.gameObject.SetActive(true); StartCoroutine(PopIn(mouthO.transform)); }

            if (hat != null) { melter.Attach(hat); melter.Drop(hat, new Vector2(1.4f, 6.5f)); }   // il cappello salta per lo spavento
            if (scarf != null) melter.Attach(scarf, snowman.TransformPoint(new Vector2(-0.05f, 2.82f)), false);
            if (armLeft != null) melter.Attach(armLeft);
            if (armRight != null) melter.Attach(armRight);
            if (attached != null) foreach (Transform t in attached) if (t != null && t.gameObject.activeSelf) melter.Attach(t);
            if (mouthO != null) melter.Attach(mouthO.transform, true);
            if (blush != null) melter.Attach(blush.transform, true);

            StartCoroutine(Flail());
            Vector3 home = snowman.localPosition;
            for (float k = 0f; k < 1.4f; k += Time.deltaTime)
            {
                snowman.localPosition = home + (Vector3)(Random.insideUnitCircle * 0.05f);   // trema tutto
                yield return null;
            }
            snowman.localPosition = home;
            if (smoke != null) smoke.Stop();

            melter.Begin();
            float from = melter.Progress;
            if (melter.body != null) melter.body.color = blushTint;
            bool said = false, blub = false;
            for (float k = 0f; k < 1f; k += Time.deltaTime / meltSeconds)
            {
                float p = Mathf.Lerp(from, 1f, k * k * 0.5f + k * 0.5f);   // piano all'inizio, poi sempre più veloce
                melter.SetProgress(p);
                if (melter.body != null) melter.body.color = Color.Lerp(blushTint, Color.white, Mathf.Clamp01(p * 3f));
                if (!said && p > 0.2f && bubble != null) { said = true; bubble.Show("I'm meltiiiing!"); }
                if (!blub && p > 0.72f && bubble != null) { blub = true; bubble.Show("blub..."); }
                yield return null;
            }
            melter.SetProgress(1f);
            melter.End();
            if (bubble != null) bubble.Hide();

            // gli occhi, che erano volati via, ricadono nella pozzanghera: plop, plop
            if (eyes != null)
                for (int i = 0; i < eyes.Count; i++)
                {
                    Transform eye = eyes[i];
                    if (eye == null) continue;
                    eye.gameObject.SetActive(true);
                    eye.localScale = Vector3.one * 1.4f;
                    eye.position = snowman.TransformPoint(new Vector2(i == 0 ? -0.7f : 0.6f, 9f));
                    melter.Attach(eye);
                    melter.Drop(eye, Vector2.zero);
                    yield return new WaitForSeconds(0.35f);
                    if (bubble != null) bubble.Show("plop!");
                }
            yield return new WaitForSeconds(1.6f);
            Running = false;
        }

        void Shoot(List<Transform> eyes)
        {
            if (eyes == null || eyes.Count == 0) return;
            eyes.Sort((a, b) => a.position.x.CompareTo(b.position.x));
            for (int i = 0; i < eyes.Count; i++)
            {
                float side = eyes.Count == 1 ? (eyes[i].position.x < snowman.position.x ? -1f : 1f) : (i == 0 ? -1f : 1f);
                melter.Detach(eyes[i]);
                eyes[i].SetParent(snowman.parent, true);
                StartCoroutine(Fly(eyes[i], new Vector2(side * Random.Range(7f, 9f), Random.Range(5f, 7f))));
            }
        }

        IEnumerator Fly(Transform t, Vector2 v)
        {
            float spin = Random.Range(600f, 900f) * Mathf.Sign(v.x);
            Vector3 scale = t.localScale;
            for (float time = 0f; time < 1.6f && t != null; time += Time.deltaTime)
            {
                v.y -= 9f * Time.deltaTime;
                t.position += (Vector3)(v * Time.deltaTime);
                t.Rotate(0, 0, -spin * Time.deltaTime);
                t.localScale = scale * (1f + time * 0.6f);   // "boing!": si ingrandiscono come nei cartoni
                yield return null;
            }
            if (t != null) t.gameObject.SetActive(false);
        }

        /// <summary>Braccia che si agitano in preda al panico; poi, mentre affonda, salutano disperate.</summary>
        IEnumerator Flail()
        {
            float t = 0f;
            while (melter.Progress < 0.62f)
            {
                t += Time.deltaTime;
                bool panic = !melter.Begun;
                float speed = panic ? 22f : 9f, amp = panic ? 38f : 25f;
                if (armLeft != null) armLeft.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(t * speed) * amp - (panic ? 0f : 15f));
                if (armRight != null) armRight.localRotation = Quaternion.Euler(0, 0, -Mathf.Sin(t * speed + 1.3f) * amp + (panic ? 0f : 15f));
                yield return null;
            }
        }

        static IEnumerator PopIn(Transform t)
        {
            Vector3 s = t.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.2f)
            {
                t.localScale = s * (k < 0.7f ? k / 0.7f * 1.25f : Mathf.Lerp(1.25f, 1f, (k - 0.7f) / 0.3f));
                yield return null;
            }
            t.localScale = s;
        }
    }
}
