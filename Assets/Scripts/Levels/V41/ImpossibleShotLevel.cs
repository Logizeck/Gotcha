using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "The impossible shot" — campetto visto dall'alto: il canestro è dietro due muretti sfalsati e il tiro
    /// diretto è impossibile. Si tira con la fionda (direzione e potenza); la palla rimbalza sulle sponde con una
    /// fisica fatta in casa e deterministica. La soluzione: un tiro di sponda che rimbalza due volte (a sinistra e a
    /// destra) e infila il canestro. Cinque tiri sbagliati = sconfitta; dopo tre errori compare la traiettoria.
    /// </summary>
    public class ImpossibleShotLevel : LevelController
    {
        [SerializeField] Transform ball, basket, forkLeft, forkRight;
        [SerializeField] LineRenderer band;
        [SerializeField] SpriteRenderer[] dots;
        [SerializeField] TextMesh missText, bounceText;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] SmokeEmitter[] confetti;
        [SerializeField] int missesAllowed = 5;
        [SerializeField] int assistAfter = 3;
        [SerializeField] float maxPull = 1.6f, minSpeed = 3.5f, maxSpeed = 12f, friction = 2.5f, radius = 0.15f;

        // campo e muretti (coordinate mondo): x0, x1, y, mezzo spessore
        const float XL = -2.4f, XR = 2.4f, YB = -4.2f, YT = 2.9f;
        static readonly Vector4[] Walls = { new Vector4(-0.7f, 2.4f, -1.2f, 0.12f), new Vector4(-2.4f, 0.7f, 0.9f, 0.12f) };
        const float Step = 1f / 240f;

        Camera cam;
        Vector2 anchor, pos, vel;
        bool aiming, flying, busy;
        int misses, rebounds;
        float acc, flyTime;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            anchor = ball.position;
            ResetBall();
            S41LKit.Confetti(confetti, false);
            UpdateMisses();
            bounceText.text = "";
            S41LKit.Say(bubble, "Pull the ball back and let go!");
        }

        void UpdateMisses() { missText.text = "MISSED SHOTS " + misses + "/" + missesAllowed; }

        void ResetBall()
        {
            pos = anchor;
            vel = Vector2.zero;
            ball.position = new Vector3(pos.x, pos.y, ball.position.z);
            ball.localScale = Vector3.one * 0.45f;
            ShowDots(false);
            DrawBand(true);
        }

        void DrawBand(bool on)
        {
            band.positionCount = on ? 3 : 2;
            band.SetPosition(0, forkLeft.position);
            if (on) { band.SetPosition(1, ball.position); band.SetPosition(2, forkRight.position); }
            else band.SetPosition(1, forkRight.position);
        }

        void Update()
        {
            if (IsFinished || busy) return;
            if (flying) { Fly(); return; }
            bool down = S41LKit.Pointer(cam, out Vector2 w, out bool began, out bool ended);
            if (began && Vector2.Distance(w, anchor) < 1.0f) { aiming = true; if (bubble != null) bubble.Hide(); }
            if (!aiming) return;
            Vector2 pull = Vector2.ClampMagnitude(anchor - (down ? w : (Vector2)ball.position), maxPull);
            // la direzione va a scatti di mezzo grado: più facile ripetere un tiro
            float ang = Mathf.Round(Mathf.Atan2(pull.y, pull.x) * Mathf.Rad2Deg * 2f) / 2f;
            Vector2 dir = new Vector2(Mathf.Cos(ang * Mathf.Deg2Rad), Mathf.Sin(ang * Mathf.Deg2Rad));
            float power = pull.magnitude / maxPull;
            Vector2 at = anchor - dir * pull.magnitude;
            ball.position = new Vector3(at.x, at.y, ball.position.z);
            DrawBand(true);
            Preview(dir, power);
            if (ended || !down)
            {
                aiming = false;
                if (pull.magnitude < 0.3f) { ResetBall(); return; }
                Launch(dir, power);
            }
        }

        float SpeedFor(float power) => Mathf.Lerp(minSpeed, maxSpeed, power);

        /// <summary>Freccetta di mira (corta); dopo qualche errore la traiettoria completa con i rimbalzi.</summary>
        void Preview(Vector2 dir, float power)
        {
            bool full = misses >= assistAfter;
            Vector2 p = anchor, v = dir * SpeedFor(power);
            int reb = 0, shown = 0;
            float travelled = 0f, next = 0.25f, limit = full ? 18f : 1.4f;
            for (int i = 0; i < 2400 && shown < dots.Length; i++)
            {
                Vector2 before = p;
                if (!StepBall(ref p, ref v, ref reb, false)) break;
                travelled += Vector2.Distance(before, p);
                if (travelled > limit) break;
                if (travelled >= next)
                {
                    next += full ? 0.55f : 0.25f;
                    dots[shown].transform.position = new Vector3(p.x, p.y, 0f);
                    dots[shown].gameObject.SetActive(true);
                    S41LKit.Alpha(dots[shown], 0.9f - shown * (full ? 0.02f : 0.12f));
                    shown++;
                }
            }
            for (int i = shown; i < dots.Length; i++) dots[i].gameObject.SetActive(false);
        }

        void ShowDots(bool on) { foreach (SpriteRenderer d in dots) d.gameObject.SetActive(on); }

        void Launch(Vector2 dir, float power)
        {
            ShowDots(false);
            pos = anchor;
            vel = dir * SpeedFor(power);
            rebounds = 0;
            acc = 0f;
            flyTime = 0f;
            flying = true;
            DrawBand(false);
            Sfx.Play(this, "whoosh", 0.7f);
            Haptics.Light();
        }

        /// <summary>Un passo di fisica (attrito, sponde, muretti). Restituisce false quando la palla è ferma.</summary>
        bool StepBall(ref Vector2 p, ref Vector2 v, ref int reb, bool sound)
        {
            float sp = v.magnitude;
            if (sp < 0.3f) return false;
            v *= Mathf.Max(0f, sp - friction * Step) / sp;
            // aiutino: vicino al canestro, dopo due rimbalzi, la palla ci scivola dentro
            Vector2 toB = (Vector2)basket.position - p;
            if (reb >= 2 && toB.magnitude < 1.0f) v += toB.normalized * 7f * Step;
            Vector2 n = p + v * Step;
            bool hit = false;
            if (n.x < XL + radius) { n.x = XL + radius; v.x = -v.x; hit = true; }
            if (n.x > XR - radius) { n.x = XR - radius; v.x = -v.x; hit = true; }
            if (n.y < YB + radius) { n.y = YB + radius; v.y = -v.y; hit = true; }
            if (n.y > YT - radius) { n.y = YT - radius; v.y = -v.y; hit = true; }
            foreach (Vector4 w in Walls)
            {
                if (n.x > w.x - radius && n.x < w.y + radius && n.y > w.z - w.w - radius && n.y < w.z + w.w + radius)
                {
                    if (!(p.y > w.z - w.w - radius && p.y < w.z + w.w + radius)) { v.y = -v.y; n.y = p.y; }
                    else { v.x = -v.x; n.x = p.x; }
                    hit = true;
                }
            }
            if (hit)
            {
                reb++;
                v *= 0.97f;
                if (sound) OnBounce(reb);
            }
            p = n;
            return true;
        }

        void OnBounce(int n)
        {
            Sfx.Play(this, "bonk", 0.6f);
            Haptics.Light();
            bounceText.text = "BOUNCE " + n + "!";
            bounceText.transform.position = new Vector3(Mathf.Clamp(pos.x, -1.9f, 1.9f), pos.y + 0.4f, 0f);
        }

        void Fly()
        {
            acc += Time.deltaTime;
            flyTime += Time.deltaTime;
            bool moving = true;
            while (acc >= Step && moving)
            {
                acc -= Step;
                moving = StepBall(ref pos, ref vel, ref rebounds, true);
                if (Vector2.Distance(pos, basket.position) < 0.45f)
                {
                    flying = false;
                    ball.position = new Vector3(pos.x, pos.y, ball.position.z);
                    StartCoroutine(Score());
                    return;
                }
            }
            ball.position = new Vector3(pos.x, pos.y, ball.position.z);
            ball.Rotate(0f, 0f, -vel.magnitude * 60f * Time.deltaTime);
            if (!moving || flyTime > 7f) { flying = false; StartCoroutine(Miss()); }
        }

        IEnumerator Miss()
        {
            busy = true;
            misses++;
            UpdateMisses();
            Sfx.Play(this, "buzz", 0.5f);
            S41LKit.Say(bubble, misses >= missesAllowed ? "Out of balls!" : rebounds < 2 ? "A straight shot won't do it!" : "So close! Try another angle.");
            if (misses == assistAfter) S41LKit.Say(bubble, "Here, have an aiming guide!");
            yield return new WaitForSeconds(0.9f);
            bounceText.text = "";
            if (misses >= missesAllowed) { yield return new WaitForSeconds(0.4f); Fail(); yield break; }
            ResetBall();   // ripartenza immediata
            busy = false;
        }

        IEnumerator Score()
        {
            busy = true;
            Sfx.Play(this, "ding", 0.9f);
            Haptics.Medium();
            Vector3 from = ball.position;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.35f)
            {
                ball.position = Vector3.Lerp(from, basket.position, k);
                ball.localScale = Vector3.one * 0.45f * (1f - k * 0.5f);
                yield return null;
            }
            bounceText.text = "SWISH! " + rebounds + " BOUNCES!";
            bounceText.transform.position = basket.position + Vector3.down * 0.9f;
            StartCoroutine(S41LKit.Pop(basket, 0.25f, 0.35f));
            S41LKit.Say(bubble, "IMPOSSIBLE... BUT DONE!");
            S41LKit.Confetti(confetti, true);
            yield return new WaitForSeconds(2.2f);
            S41LKit.Confetti(confetti, false);
            Win();
        }
    }
}
