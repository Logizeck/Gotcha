using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "Reverse parking" — parcheggio visto dall'alto: l'auto (col muso in giù, quindi FWD e REV sembrano
    /// invertiti) deve entrare nel posto stretto fra due auto parcheggiate. Il volante torna dritto appena lo si lascia e
    /// l'auto sterza solo mentre si muove: serve un pollice sul volante e uno sul pedale insieme. Una scia di puntini
    /// mostra la traiettoria. Soluzione: affiancarsi all'auto di sotto e fare una breve retromarcia sterzando.
    /// Tre urti contro le altre auto = sconfitta. Nell'Editor: A/D sterzo, W avanti, S retro.
    /// </summary>
    public class ReverseParkingLevel : LevelController
    {
        [SerializeField] Transform car;
        [SerializeField] Transform[] parked;
        [SerializeField] Transform wheel;
        [SerializeField] Collider2D wheelArea;
        [SerializeField] Collider2D fwdPedal;
        [SerializeField] Collider2D revPedal;
        [SerializeField] SpriteRenderer[] dots;
        [SerializeField] TextMesh goal;
        [SerializeField] SpriteRenderer[] hearts;
        [SerializeField] SpeechBubble bubble;

        const float HalfW = 0.27f, HalfL = 0.56f, Wheelbase = 0.7f, RearOffset = 0.35f, MaxSteer = 38f, TopSpeed = 1.15f;
        static readonly Vector2 Spot = new Vector2(1.75f, 0f);

        Camera cam;
        Vector2 rear;            // centro dell'asse posteriore
        float heading;           // gradi: 0 = muso in su
        float speed, steer, lastDir = -1f;
        Vector2 safeRear;
        float safeHeading;
        int strikes;
        bool busy;
        int wheelFinger = -99;
        float wheelStartX, parkedFor, invuln, oneFingerTime, bubbleUntil;
        bool tipShown;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            heading = 180f;
            Vector2 c = car.position;
            rear = c - Dir(heading) * RearOffset;
            safeRear = rear;
            safeHeading = heading;
            Kit41C.Hearts(hearts, 3);
            Kit41C.Goal(goal, "PARK IN THE YELLOW SPACE");
            Apply();
            Say("Tight spot... good luck!", 2f);
        }

        static Vector2 Dir(float deg) => new Vector2(-Mathf.Sin(deg * Mathf.Deg2Rad), Mathf.Cos(deg * Mathf.Deg2Rad));

        void Say(string text, float seconds)
        {
            if (bubble == null) return;
            bubble.Show(text);
            bubbleUntil = Time.time + seconds;
        }

        void Update()
        {
            if (IsFinished) return;
            if (bubble != null && bubbleUntil > 0f && Time.time > bubbleUntil) { bubble.Hide(); bubbleUntil = 0f; }
            if (busy) return;
            float dt = Time.deltaTime;
            invuln -= dt;

            // ---- comandi: ogni dito può stare sul volante o su un pedale ----
            bool fwd = false, rev = false, steering = false;
            foreach (Kit41C.Finger f in Kit41C.Fingers(cam))
            {
                if (f.ended) { if (f.id == wheelFinger) wheelFinger = -99; continue; }
                if (f.began && wheelArea != null && wheelArea.OverlapPoint(f.world)) { wheelFinger = f.id; wheelStartX = f.world.x - steer * 0.6f; }
                if (f.id == wheelFinger)
                {
                    steering = true;
                    steer = Mathf.Clamp((f.world.x - wheelStartX) / 0.6f, -1f, 1f);
                    continue;
                }
                if (fwdPedal != null && fwdPedal.OverlapPoint(f.world)) fwd = true;
                if (revPedal != null && revPedal.OverlapPoint(f.world)) rev = true;
            }
#if UNITY_EDITOR
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) { steering = true; steer = Mathf.MoveTowards(steer, -1f, dt * 3f); }
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) { steering = true; steer = Mathf.MoveTowards(steer, 1f, dt * 3f); }
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) fwd = true;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) rev = true;
#endif
            if (!steering) steer = Mathf.MoveTowards(steer, 0f, dt * 4f);   // il volante torna dritto
            if (wheel != null) wheel.localRotation = Quaternion.Euler(0f, 0f, -steer * 120f);

            float target = fwd == rev ? 0f : (fwd ? TopSpeed : -TopSpeed);
            speed = Mathf.MoveTowards(speed, target, dt * (target == 0f ? 4f : 2.5f));
            if (Mathf.Abs(speed) > 0.05f) lastDir = Mathf.Sign(speed);

            // un solo dito per un po': suggerimento
            if (!tipShown && (steering ^ (fwd || rev))) oneFingerTime += dt;
            if (!tipShown && oneFingerTime > 4f) { tipShown = true; Say("Steer AND drive at the same time!", 2.5f); }

            // ---- movimento (bicicletta: l'asse posteriore segue il muso) ----
            if (Mathf.Abs(speed) > 0.001f)
            {
                heading += -speed / Wheelbase * Mathf.Tan(steer * MaxSteer * Mathf.Deg2Rad) * Mathf.Rad2Deg * dt;
                rear += Dir(heading) * speed * dt;
                if (HitsCurb())
                {
                    rear = safeRear;
                    heading = safeHeading;
                    speed = 0f;
                    Sfx.Play(this, "tick", 0.5f);
                }
                else if (HitParked(out int which))
                {
                    rear = safeRear;
                    heading = safeHeading;
                    speed = 0f;
                    if (invuln <= 0f) { StartCoroutine(Crash(which)); return; }
                }
                else { safeRear = rear; safeHeading = heading; }
            }
            Apply();
            DrawPath();
            CheckParked(fwd || rev);
        }

        Vector2 Center() => rear + Dir(heading) * RearOffset;

        void Apply()
        {
            car.position = Center();
            car.rotation = Quaternion.Euler(0f, 0f, heading);
        }

        void Corners(Vector2 c, float deg, float hw, float hl, Vector2[] o)
        {
            Vector2 f = Dir(deg), r = new Vector2(f.y, -f.x);
            o[0] = c + f * hl + r * hw; o[1] = c + f * hl - r * hw; o[2] = c - f * hl - r * hw; o[3] = c - f * hl + r * hw;
        }

        readonly Vector2[] ca = new Vector2[4], cb = new Vector2[4];

        bool HitsCurb()
        {
            Corners(Center(), heading, HalfW, HalfL, ca);
            foreach (Vector2 p in ca)
                if (p.x < -1.6f || p.x > 2.45f || p.y > 2.6f || p.y < -2.85f) return true;
            return false;
        }

        bool HitParked(out int which)
        {
            // collisione tollerante: i rettangoli sono un po' più piccoli del disegno
            Corners(Center(), heading, HalfW - 0.04f, HalfL - 0.05f, ca);
            for (int i = 0; i < parked.Length; i++)
            {
                Corners(parked[i].position, parked[i].eulerAngles.z, HalfW - 0.04f, HalfL - 0.05f, cb);
                if (Overlap(ca, cb)) { which = i; return true; }
            }
            which = -1;
            return false;
        }

        static bool Overlap(Vector2[] a, Vector2[] b)
        {
            return !Separated(a, b, a) && !Separated(a, b, b);
        }

        static bool Separated(Vector2[] a, Vector2[] b, Vector2[] axesFrom)
        {
            for (int i = 0; i < 2; i++)
            {
                Vector2 e = axesFrom[i + 1] - axesFrom[i];
                Vector2 n = new Vector2(-e.y, e.x);
                float amin = float.MaxValue, amax = float.MinValue, bmin = float.MaxValue, bmax = float.MinValue;
                foreach (Vector2 p in a) { float d = Vector2.Dot(p, n); amin = Mathf.Min(amin, d); amax = Mathf.Max(amax, d); }
                foreach (Vector2 p in b) { float d = Vector2.Dot(p, n); bmin = Mathf.Min(bmin, d); bmax = Mathf.Max(bmax, d); }
                if (amax < bmin || bmax < amin) return true;
            }
            return false;
        }

        void DrawPath()
        {
            if (dots == null) return;
            Vector2 r = rear;
            float h = heading;
            float v = TopSpeed * lastDir;
            const float step = 0.13f;
            for (int i = 0; i < dots.Length; i++)
            {
                for (int k = 0; k < 3; k++)
                {
                    h += -v / Wheelbase * Mathf.Tan(steer * MaxSteer * Mathf.Deg2Rad) * Mathf.Rad2Deg * step / 3f;
                    r += Dir(h) * v * step / 3f;
                }
                if (dots[i] == null) continue;
                // la scia parte dal paraurti nella direzione di marcia
                dots[i].transform.position = r + Dir(h) * (RearOffset + HalfL * lastDir);
                Color c = lastDir < 0f ? Color.white : new Color(1f, 0.9f, 0.35f);
                c.a = 0.85f * (1f - i / (float)dots.Length);
                dots[i].color = c;
            }
        }

        void CheckParked(bool pressing)
        {
            Vector2 c = Center();
            float a = Mathf.Abs(Mathf.DeltaAngle(heading, 0f));
            a = Mathf.Min(a, 180f - a);
            bool inSpot = Mathf.Abs(c.x - Spot.x) < 0.28f && Mathf.Abs(c.y - Spot.y) < 0.4f && a < 15f;
            if (inSpot && !pressing && Mathf.Abs(speed) < 0.05f) parkedFor += Time.deltaTime;
            else parkedFor = 0f;
            if (parkedFor > 0.35f) StartCoroutine(Parked());
        }

        IEnumerator Crash(int which)
        {
            busy = true;
            strikes++;
            invuln = 0.8f;
            Kit41C.Hearts(hearts, 3 - strikes);
            Sfx.Play(this, "bonk", 0.9f);
            Sfx.Play(this, "buzz", 0.5f);
            Haptics.Heavy();
            if (which >= 0) StartCoroutine(Kit41C.Shake(parked[which], 0.1f));
            yield return Kit41C.Shake(car, 0.08f);
            Apply();
            if (strikes >= 3)
            {
                Kit41C.Goal(goal, "INSURANCE CANCELLED!");
                Say("BEEEEP! That's three dents!", 2f);
                yield return new WaitForSeconds(1.4f);
                Fail();
                yield break;
            }
            Say(strikes == 1 ? "Ouch! Watch the bumper!" : "Another dent! Last chance.", 1.8f);
            busy = false;
        }

        IEnumerator Parked()
        {
            busy = true;
            foreach (SpriteRenderer d in dots) if (d != null) d.enabled = false;
            // si allinea al centro del posto
            Vector3 from = car.position;
            Quaternion r0 = car.rotation;
            float up = Mathf.Abs(Mathf.DeltaAngle(heading, 0f)) < 90f ? 0f : 180f;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.35f)
            {
                car.position = Vector3.Lerp(from, (Vector3)Spot, k);
                car.rotation = Quaternion.Slerp(r0, Quaternion.Euler(0f, 0f, up), k);
                yield return null;
            }
            Kit41C.Goal(goal, "PERFECT PARKING!");
            Sfx.Play(this, "ding", 0.8f);
            Say("Beep beep! Nailed it!", 2f);
            for (int i = 0; i < 3; i++) { yield return Kit41C.Pop(car, 0.12f, 0.2f); }
            yield return new WaitForSeconds(0.8f);
            Win();
        }
    }
}
