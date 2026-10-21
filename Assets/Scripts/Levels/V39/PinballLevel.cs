using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "Pinball" — flipper vero. Tocca o tieni premuta la metà sinistra/destra dello schermo per le palette.
    /// La pallina parte dalla corsia di destra con uno swipe in su o un tocco. Accendi i 3 respingenti: il gatto in
    /// alto spalanca la bocca; manda la pallina nella bocca per vincere. Tre palline: se cadono tutte fra le palette,
    /// hai perso.
    /// </summary>
    public class PinballLevel : LevelController
    {
        // ---------- geometria del tavolo (coordinate locali, condivise con il builder) ----------
        public const float BallRadius = 0.17f;
        public static readonly Vector2 LeftPivot = new Vector2(-1.25f, -3.45f), RightPivot = new Vector2(0.85f, -3.45f);
        public const float FlipperLength = 0.84f, RestAngle = -30f, UpAngle = 28f;
        public static readonly Vector2 LaunchPos = new Vector2(2.415f, -4.1f);
        public static readonly Vector2[] Bumpers = { new Vector2(-1.45f, 1.15f), new Vector2(0.95f, 1.15f), new Vector2(-1.05f, -0.45f) };
        public const float BumperRadius = 0.36f;
        public static readonly Vector2 CatPos = new Vector2(-0.25f, 2.5f);
        public const float CatScale = 0.85f, CatRadius = 0.49f;
        public static readonly Vector2 MouthPos = new Vector2(-0.25f, 2.3f);
        public static readonly Vector2[] RightSlope = { new Vector2(2.15f, -2.55f), new Vector2(0.93f, -3.38f) };
        public static readonly Vector2[] Gate = { new Vector2(2.15f, 1.3f), new Vector2(2.68f, 1.8f) };

        /// <summary>Muro esterno: scivolo sinistro, parete, arco in alto, parete destra, fondo e divisorio della corsia.</summary>
        public static Vector2[] OuterWall()
        {
            var pts = new System.Collections.Generic.List<Vector2> { new Vector2(-1.33f, -3.38f), new Vector2(-2.55f, -2.55f), new Vector2(-2.55f, 2.4f) };
            const float cx = 0.065f, cy = 2.4f, rx = 2.615f, ry = 1.1f;
            for (int i = 1; i < 24; i++)
            {
                float a = Mathf.PI * (1f - i / 24f);
                pts.Add(new Vector2(cx + Mathf.Cos(a) * rx, cy + Mathf.Sin(a) * ry));
            }
            pts.Add(new Vector2(2.68f, 2.4f));
            pts.Add(new Vector2(2.68f, -4.3f));
            pts.Add(new Vector2(2.15f, -4.3f));
            pts.Add(new Vector2(2.15f, 1.3f));
            return pts.ToArray();
        }

        [SerializeField] Rigidbody2D ball;
        [SerializeField] Rigidbody2D leftFlipper;
        [SerializeField] Rigidbody2D rightFlipper;
        [SerializeField] Transform[] bumpers;
        [SerializeField] GameObject[] bumperLit;
        [SerializeField] GameObject catClosed;
        [SerializeField] GameObject catOpen;
        [SerializeField] Collider2D gate;
        [SerializeField] Transform plunger;
        [SerializeField] TextMesh ballsLabel;
        [SerializeField] TextMesh prompt;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] float gravity = 6.5f, launchSpeed = 11.5f, maxSpeed = 13f, bumperKick = 7f;
        [SerializeField] float flipUpSpeed = 1400f, flipDownSpeed = 650f;

        int balls = 3;
        bool waiting = true, busy, done, mouthOpen;
        readonly bool[] lit = new bool[3];
        readonly float[] bumperCool = new float[3];
        float leftK, rightK, leftCool, rightCool, stuck;
        Vector3 plungerScale;
        Vector3[] bumperScale;

        public override void OnLevelStart()
        {
            var mat = new PhysicsMaterial2D("PinBall") { bounciness = 0.35f, friction = 0.02f };
            ball.GetComponent<Collider2D>().sharedMaterial = mat;
            ball.gravityScale = 0f;
            ball.freezeRotation = true;
            ball.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            ball.interpolation = RigidbodyInterpolation2D.Interpolate;
            plungerScale = plunger.localScale;
            bumperScale = new Vector3[bumpers.Length];
            for (int i = 0; i < bumpers.Length; i++) { bumperScale[i] = bumpers[i].localScale; bumperLit[i].SetActive(false); }
            SetMouth(false);
            SetFlipper(leftFlipper, LeftPivot, 0f, false);
            SetFlipper(rightFlipper, RightPivot, 0f, true);
            NewBall();
            if (bubble != null) { bubble.Show("Light my 3 bumpers... then feed me!"); StartCoroutine(HideLater(2.8f)); }
        }

        static Vector2 Vel(Rigidbody2D rb)
        {
#if UNITY_6000_0_OR_NEWER
            return rb.linearVelocity;
#else
            return rb.GetVelocity();
#endif
        }

        static void SetVel(Rigidbody2D rb, Vector2 v)
        {
#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = v;
#else
            rb.SetVelocity(v);
#endif
        }

        void NewBall()
        {
            ball.transform.localPosition = LaunchPos;
            ball.position = ball.transform.position;
            SetVel(ball, Vector2.zero);
            ball.simulated = true;
            ball.transform.localScale = Vector3.one;
            gate.enabled = false;
            waiting = true;
            stuck = 0f;
            ballsLabel.text = "BALLS " + balls;
            prompt.text = "SWIPE UP TO LAUNCH";
        }

        void SetMouth(bool open)
        {
            mouthOpen = open;
            catOpen.SetActive(open);
            catClosed.SetActive(!open);
        }

        // ---------- input e palette ----------

        void Update()
        {
            if (IsFinished || done || Time.timeScale == 0f) return;
            if (waiting && PointerInput.AnyTouchBegan() && !PointerInput.IsOverUi()) StartCoroutine(Launch());
#if UNITY_EDITOR
            if (waiting && Input.GetKeyDown(KeyCode.Space)) StartCoroutine(Launch());
#endif
        }

        void ReadFlippers(out bool left, out bool right)
        {
            left = right = false;
            float half = Screen.width * 0.5f;
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch t = Input.GetTouch(i);
                if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled) continue;
                if (t.position.x < half) left = true; else right = true;
            }
            if (Input.touchCount == 0 && Input.GetMouseButton(0))
            {
                if (Input.mousePosition.x < half) left = true; else right = true;
            }
#if UNITY_EDITOR
            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.Z)) left = true;
            if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.M)) right = true;
#endif
        }

        /// <summary>Angolo della paletta: k = 0 a riposo, 1 alzata. La destra è lo specchio della sinistra.</summary>
        static float FlipAngle(float k, bool mirrored)
        {
            float a = Mathf.Lerp(RestAngle, UpAngle, k);
            return mirrored ? 180f - a : a;
        }

        void SetFlipper(Rigidbody2D rb, Vector2 pivot, float k, bool mirrored)
        {
            rb.transform.localPosition = pivot;
            rb.transform.localRotation = Quaternion.Euler(0, 0, FlipAngle(k, mirrored));
        }

        void FixedUpdate()
        {
            if (IsFinished || done) return;
            float dt = Time.fixedDeltaTime;
            bool l = false, r = false;
            if (Time.timeScale > 0f) ReadFlippers(out l, out r);
            leftCool -= dt; rightCool -= dt;
            leftK = MoveFlipper(leftFlipper, LeftPivot, leftK, l, false, ref leftCool, dt);
            rightK = MoveFlipper(rightFlipper, RightPivot, rightK, r, true, ref rightCool, dt);

            if (!ball.simulated || busy) return;
            // gravità del tavolo (leggera, indipendente da quella globale)
            ball.AddForce(Vector2.down * gravity * ball.mass);
            Vector2 bp = transform.InverseTransformPoint(ball.position);
            Vector2 v = Vel(ball);

            // respingenti: calcio deciso verso l'esterno
            for (int i = 0; i < bumpers.Length; i++)
            {
                bumperCool[i] -= dt;
                Vector2 d = bp - Bumpers[i];
                if (d.magnitude < BumperRadius + BallRadius + 0.04f && bumperCool[i] <= 0f)
                {
                    bumperCool[i] = 0.12f;
                    Vector2 n = d.normalized;
                    Vector2 tangent = v - Vector2.Dot(v, n) * n;
                    v = n * Mathf.Max(bumperKick, v.magnitude * 0.8f) + tangent * 0.3f;
                    SetVel(ball, transform.TransformDirection(v));
                    StartCoroutine(Flash(i));
                }
            }

            // corsia di lancio: se la pallina ne è uscita il cancelletto si chiude; se ci ricade, si rilancia
            bool inLane = bp.x > 2.15f && bp.y < 1.3f;
            if (!gate.enabled && !inLane && bp.x < 2.05f) gate.enabled = true;
            if (!waiting && inLane && v.magnitude < 0.4f && bp.y < -3.8f) { waiting = true; prompt.text = "SWIPE UP TO LAUNCH"; }

            // bocca aperta: la pallina che arriva lì viene mangiata
            if (mouthOpen && Vector2.Distance(bp, MouthPos) < 0.5f) { StartCoroutine(Gulp()); return; }

            // velocità massima e anti-incastro
            v = Vel(ball);
            if (v.magnitude > maxSpeed) SetVel(ball, v.normalized * maxSpeed);
            if (!waiting && v.magnitude < 0.15f && bp.y > -2.9f) stuck += dt; else stuck = 0f;
            if (stuck > 2f) { stuck = 0f; SetVel(ball, new Vector2(Random.Range(-2f, 2f), 4f)); }

            if (bp.y < -4.75f) StartCoroutine(Drain());
        }

        float MoveFlipper(Rigidbody2D rb, Vector2 pivot, float k, bool held, bool mirrored, ref float cool, float dt)
        {
            float range = UpAngle - RestAngle;
            float nk = held ? Mathf.Min(1f, k + flipUpSpeed * dt / range) : Mathf.Max(0f, k - flipDownSpeed * dt / range);
            rb.MoveRotation(transform.eulerAngles.z + FlipAngle(nk, mirrored));
            if (held && nk > k && cool <= 0f && ball.simulated && !busy && HitBall(pivot, nk, mirrored)) cool = 0.15f;
            if (held && k == 0f && nk > 0f) Sfx.Play(this, "tick", 0.5f);
            return nk;
        }

        /// <summary>Colpo della paletta che sale: spinta lungo la normale, più forte verso la punta (niente tunneling).</summary>
        bool HitBall(Vector2 pivot, float k, bool mirrored)
        {
            float a = FlipAngle(k, mirrored) * Mathf.Deg2Rad;
            Vector2 along = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
            Vector2 normal = mirrored ? new Vector2(Mathf.Sin(a), -Mathf.Cos(a)) : new Vector2(-Mathf.Sin(a), Mathf.Cos(a));
            Vector2 bp = transform.InverseTransformPoint(ball.position);
            Vector2 rel = bp - pivot;
            float t = Vector2.Dot(rel, along), n = Vector2.Dot(rel, normal);
            if (t < -0.1f || t > FlipperLength + 0.12f || n < -0.06f || n > 0.42f) return false;
            float kick = Mathf.Lerp(6.5f, 11f, Mathf.Clamp01(t / FlipperLength));
            Vector2 v = transform.InverseTransformDirection(Vel(ball));
            float vn = Vector2.Dot(v, normal);
            if (vn >= kick) return false;
            if (n < 0.3f) ball.position = transform.TransformPoint(pivot + along * Mathf.Clamp(t, 0f, FlipperLength) + normal * 0.3f);
            v += normal * (kick - vn);
            SetVel(ball, transform.TransformDirection(v));
            Haptics.Light();
            return true;
        }

        // ---------- eventi ----------

        IEnumerator Launch()
        {
            waiting = false;
            prompt.text = "";
            for (float t = 0f; t < 0.15f; t += Time.deltaTime)
            {
                plunger.localScale = new Vector3(plungerScale.x, plungerScale.y * (1f - t / 0.15f * 0.4f), plungerScale.z);
                yield return null;
            }
            plunger.localScale = plungerScale;
            if (done) yield break;
            SetVel(ball, transform.TransformDirection(Vector2.up * launchSpeed * Random.Range(0.97f, 1.03f)));
            Sfx.Play(this, "whoosh", 0.6f);
        }

        IEnumerator Flash(int i)
        {
            Sfx.Play(this, "pop", 0.7f);
            Haptics.Light();
            if (!lit[i])
            {
                lit[i] = true;
                bumperLit[i].SetActive(true);
                if (lit[0] && lit[1] && lit[2] && !mouthOpen) StartCoroutine(OpenMouth());
            }
            for (float t = 0f; t < 0.18f; t += Time.deltaTime)
            {
                bumpers[i].localScale = bumperScale[i] * (1f + Mathf.Sin(t / 0.18f * Mathf.PI) * 0.18f);
                yield return null;
            }
            bumpers[i].localScale = bumperScale[i];
        }

        IEnumerator OpenMouth()
        {
            yield return new WaitForSeconds(0.2f);
            SetMouth(true);
            Sfx.Play(this, "ding", 0.6f);
            if (bubble != null && !done) bubble.Show("Aaaah! Feed me the ball!");
            yield return new WaitForSeconds(2.2f);
            if (bubble != null && !done) bubble.Hide();
        }

        IEnumerator Drain()
        {
            busy = true;
            ball.simulated = false;
            balls--;
            ballsLabel.text = "BALLS " + balls;
            Haptics.Medium();
            Sfx.Play(this, "buzz", 0.6f);
            if (balls <= 0)
            {
                done = true;
                if (bubble != null) bubble.Show("No more balls? Boring human.");
                yield return new WaitForSeconds(1.4f);
                Fail();
                yield break;
            }
            if (bubble != null) bubble.Show(balls == 1 ? "Last ball!" : "Drained! " + balls + " balls left.");
            yield return new WaitForSeconds(1.0f);
            if (bubble != null) bubble.Hide();
            busy = false;
            NewBall();
        }

        /// <summary>Vittoria: la pallina sparisce nella bocca del gatto, che la mastica soddisfatto.</summary>
        IEnumerator Gulp()
        {
            done = true;
            ball.simulated = false;
            Vector3 a = ball.transform.localPosition;
            for (float t = 0f; t < 0.35f; t += Time.deltaTime)
            {
                float k = t / 0.35f;
                ball.transform.localPosition = Vector3.Lerp(a, MouthPos, k);
                ball.transform.localScale = Vector3.one * (1f - k);
                yield return null;
            }
            ball.gameObject.SetActive(false);
            Sfx.Play(this, "ding", 0.9f);
            Haptics.Light();
            if (bubble != null) bubble.Show("GULP! Crunchy and shiny!");
            for (int i = 0; i < 4; i++)
            {
                SetMouth(i % 2 == 1);
                yield return new WaitForSeconds(0.2f);
            }
            SetMouth(false);
            yield return new WaitForSeconds(0.8f);
            Win();
        }

        IEnumerator HideLater(float s)
        {
            yield return new WaitForSeconds(s);
            if (bubble != null && !done && !mouthOpen) bubble.Hide();
        }
    }
}
