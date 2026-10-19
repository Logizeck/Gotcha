using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "Air Hockey" — trascina il tuo mazzuolo nella metà in basso; il gatto gioca in alto con la zampa.
    /// Chi segna 3 gol vince. Il gatto è battibile: reagisce in ritardo, mira male, ogni tanto si distrae e lascia
    /// la porta scoperta; la sua porta è più larga della tua e il tuo mazzuolo colpisce da più lontano.
    /// Sorpresa: sul 2-0 per te il gatto bara e copre la porta con uno zampone: toccalo (basta toccare vicino,
    /// "boop!") per farglielo ritirare, poi tira subito prima che torni.
    /// </summary>
    public class AirHockeyLevel : LevelController
    {
        // ---------- geometria (coordinate locali, condivise con il builder) ----------
        public const float HalfW = 2.5f, Top = 2.75f, Bottom = -4.45f, MidY = -0.85f, GoalHalf = 0.75f, CatGoalHalf = 1.15f;
        public const float MalletR = 0.38f, PuckR = 0.24f, PlayerHitR = 0.6f;
        public static readonly Vector2 CatHome = new Vector2(0f, 2.0f), PlayerHome = new Vector2(0f, -3.6f);

        [SerializeField] Rigidbody2D puck;
        [SerializeField] Transform playerMallet;
        [SerializeField] Transform catMallet;
        [SerializeField] Transform bigPaw;          // zampone che copre la porta del gatto
        [SerializeField] Collider2D bigPawBlock;
        [SerializeField] Transform catFace;
        [SerializeField] Transform[] pupils;
        [SerializeField] GameObject anger;
        [SerializeField] TextMesh score;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] int goalsToWin = 3;
        [SerializeField] float catSpeed = 2.7f, catStrike = 4.8f, playerMaxSpeed = 26f, maxPuck = 7.5f;
        [SerializeField] float catReaction = 0.28f, catAimError = 1.3f;
        [SerializeField] float pawUpY = 5.2f, pawDownY = 3.05f, pawReturn = 8f;

        int mine, cats;
        bool busy, done, pawWanted, pawDown;
        float pawCool, stillT, seenT, dazeT, dazeCool = 3f, aimX;
        Vector2 seenP, seenV, dazeSpot;
        Vector2 playerPrev, catPrev, playerTarget;
        Vector3[] pupilHome;
        Camera cam;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            puck.GetComponent<Collider2D>().sharedMaterial = new PhysicsMaterial2D("Puck") { bounciness = 0.9f, friction = 0f };
            puck.gravityScale = 0f;
            puck.freezeRotation = true;
            puck.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            puck.interpolation = RigidbodyInterpolation2D.Interpolate;
#if UNITY_6000_0_OR_NEWER
            puck.linearDamping = 0.25f;
#else
            puck.drag = 0.25f;
#endif
            pupilHome = new Vector3[pupils.Length];
            for (int i = 0; i < pupils.Length; i++) pupilHome[i] = pupils[i].localPosition;
            anger.SetActive(false);
            bigPaw.localPosition = new Vector3(0f, pawUpY, 0f);
            bigPawBlock.enabled = false;
            Serve(false);
            ShowScore();
            if (bubble != null) { bubble.Show("First to 3 goals! Meow."); StartCoroutine(HideLater(2.2f)); }
        }

        static Vector2 Vel(Rigidbody2D rb)
        {
#if UNITY_6000_0_OR_NEWER
            return rb.linearVelocity;
#else
            return rb.velocity;
#endif
        }

        static void SetVel(Rigidbody2D rb, Vector2 v)
        {
#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = v;
#else
            rb.velocity = v;
#endif
        }

        Vector2 PuckLocal => transform.InverseTransformPoint(puck.position);
        void SetPuckLocal(Vector2 p) => puck.position = transform.TransformPoint(p);

        /// <summary>Rimette il disco in gioco dal lato di chi ha subito il gol.</summary>
        void Serve(bool catSide)
        {
            SetPuckLocal(new Vector2(0f, catSide ? 0.4f : -2.0f));
            puck.transform.localPosition = new Vector3(0f, catSide ? 0.4f : -2.0f, 0f);
            SetVel(puck, Vector2.zero);
            puck.simulated = true;
            playerMallet.localPosition = PlayerHome;
            catMallet.localPosition = CatHome;
            playerPrev = playerTarget = PlayerHome;
            catPrev = CatHome;
            seenP = new Vector2(0f, catSide ? 0.4f : -2.0f);
            seenV = Vector2.zero;
            seenT = 0f;
            dazeT = 0f;
            aimX = Random.Range(-catAimError, catAimError);
        }

        void ShowScore() { score.text = "YOU " + mine + " : " + cats + " CAT"; }

        // ---------- input ----------

        void Update()
        {
            if (IsFinished || done) return;
            // occhi del gatto che seguono il disco
            Vector2 pp = puck.transform.localPosition;
            for (int i = 0; i < pupils.Length; i++)
            {
                Vector2 eye = catFace.localPosition + pupilHome[i] * catFace.localScale.x;
                Vector2 look = (pp - eye).normalized * 0.06f / Mathf.Max(0.01f, catFace.localScale.x);
                pupils[i].localPosition = pupilHome[i] + (Vector3)look;
            }
            if (Time.timeScale == 0f) return;
            if (PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out _) && !PointerInput.IsOverUi())
            {
                Vector2 w = transform.InverseTransformPoint(PointerInput.ToWorld(cam, sp));
                // boop facile: basta toccare nella zona della porta del gatto, anche mentre lo zampone scende
                bool pawThere = pawDown || (pawWanted && bigPaw.localPosition.y < pawUpY - 0.6f);
                if (began && pawThere && Mathf.Abs(w.x - bigPaw.localPosition.x) < 2.0f && w.y > Top - 1.6f)
                    StartCoroutine(Boop());
                playerTarget = new Vector2(Mathf.Clamp(w.x, -HalfW + MalletR, HalfW - MalletR), Mathf.Clamp(w.y, Bottom + MalletR, MidY - MalletR));
            }
            UpdatePaw();
        }

        // ---------- fisica ----------

        void FixedUpdate()
        {
            if (IsFinished || done || busy || !puck.simulated) return;
            float dt = Time.fixedDeltaTime;
            Vector2 p = PuckLocal, v = Vel(puck);

            // mazzuolo del giocatore: segue il dito con un limite di velocità
            Vector2 pm = Vector2.MoveTowards(playerPrev, playerTarget, playerMaxSpeed * dt);
            Strike(playerPrev, pm, PlayerHitR, ref p, ref v, dt);
            playerMallet.localPosition = pm;
            playerPrev = pm;

            // il gatto "vede" il disco in ritardo (tempo di reazione)
            seenT += dt;
            if (seenT >= catReaction) { seenT = 0f; seenP = p; seenV = v; }
            // ogni tanto si distrae (si lecca la zampa) e lascia un buco in porta
            dazeCool -= dt;
            if (dazeT > 0f) dazeT -= dt;
            else if (dazeCool <= 0f)
            {
                dazeCool = Random.Range(3.5f, 6f);
                if (Random.value < 0.55f)
                {
                    dazeT = Random.Range(0.8f, 1.4f);
                    dazeSpot = new Vector2(Random.value < 0.5f ? -1.9f : 1.9f, CatHome.y + 0.3f);
                }
            }
            // gatto: difende la porta seguendo la x del disco, attacca quando il disco è nella sua metà
            Vector2 goal = CatTarget(seenP, seenV, out float speed);
            if (dazeT > 0f) { goal = dazeSpot; speed = catSpeed * 0.6f; }
            Vector2 cm = Vector2.MoveTowards(catPrev, goal, speed * dt);
            cm = new Vector2(Mathf.Clamp(cm.x, -HalfW + MalletR, HalfW - MalletR), Mathf.Clamp(cm.y, MidY + MalletR, Top - MalletR));
            Strike(catPrev, cm, MalletR, ref p, ref v, dt);
            catMallet.localPosition = cm;
            catPrev = cm;

            // il disco resta nel tavolo (tranne davanti alle porte) e non supera la velocità massima
            float mouth = p.y > MidY ? CatGoalHalf : GoalHalf;
            if (Mathf.Abs(p.x) > mouth - PuckR * 0.5f) p.y = Mathf.Clamp(p.y, Bottom + PuckR, Top - PuckR);
            p.x = Mathf.Clamp(p.x, -HalfW + PuckR, HalfW - PuckR);
            if (v.magnitude > maxPuck) v = v.normalized * maxPuck;
            SetPuckLocal(p);
            SetVel(puck, v);

            // disco fermo dove nessuno lo raggiunge: una spintarella verso il centro
            stillT = v.magnitude < 0.2f ? stillT + dt : 0f;
            if (stillT > 4f) { stillT = 0f; SetVel(puck, (new Vector2(0f, MidY) - p).normalized * 2.5f); }

            if (p.y > Top + 0.15f && Mathf.Abs(p.x) < CatGoalHalf) StartCoroutine(Goal(true));
            else if (p.y < Bottom - 0.15f && Mathf.Abs(p.x) < GoalHalf) StartCoroutine(Goal(false));
        }

        /// <summary>Urto mazzuolo → disco con test "spazzato" lungo il movimento del mazzuolo (niente tunneling).</summary>
        void Strike(Vector2 m0, Vector2 m1, float hitR, ref Vector2 p, ref Vector2 v, float dt)
        {
            float R = hitR + PuckR;
            Vector2 d = m1 - m0, f = m0 - p;
            float s;
            if (f.magnitude <= R) s = 0f;
            else
            {
                float a = Vector2.Dot(d, d), b = 2f * Vector2.Dot(f, d), c = Vector2.Dot(f, f) - R * R;
                float disc = b * b - 4f * a * c;
                if (a < 1e-6f || disc < 0f) return;
                s = (-b - Mathf.Sqrt(disc)) / (2f * a);
                if (s < 0f || s > 1f) return;
            }
            Vector2 mc = m0 + d * s;
            Vector2 n = (p - mc).sqrMagnitude > 1e-6f ? (p - mc).normalized : Vector2.up;
            Vector2 vm = d / dt;
            float vn = Vector2.Dot(v - vm, n);
            if (vn < 0f)
            {
                if (hitR == MalletR) aimX = Random.Range(-catAimError, catAimError);   // nuovo tiro del gatto, nuova mira storta
                v -= 1.9f * vn * n;
                Sfx.Play(this, "tick", Mathf.Clamp01(-vn * 0.1f));
            }
            if ((p - m1).magnitude < R) p = m1 + n * (R + 0.01f);
        }

        Vector2 CatTarget(Vector2 p, Vector2 v, out float speed)
        {
            speed = catSpeed;
            if (p.y > MidY && v.y > -3f)
            {
                // attacco: si mette dietro al disco (verso la propria porta) e poi tira verso la porta del giocatore
                // mira imperfetta: punta a un punto a caso attorno alla porta (spesso fuori)
                Vector2 toGoal = (new Vector2(aimX, Bottom) - p).normalized;
                Vector2 behind = p - toGoal * (MalletR + PuckR + 0.15f);
                Vector2 rel = catPrev - p;
                if (Vector2.Dot(rel, -toGoal) > rel.magnitude * 0.8f && rel.magnitude < 1.2f)
                {
                    speed = catStrike;
                    return p + toGoal * 0.6f;
                }
                if (behind.y > Top - MalletR) behind.y = Top - MalletR;
                return behind;
            }
            // in difesa segue il disco solo a metà: gli angoli della porta larga restano scoperti
            return new Vector2(Mathf.Clamp(p.x * 0.6f, -0.9f, 0.9f), CatHome.y + 0.2f);
        }

        // ---------- zampone baro ----------

        void UpdatePaw()
        {
            pawCool -= Time.deltaTime;
            pawWanted = mine == goalsToWin - 1 && cats == 0 && pawCool <= 0f;
            float targetY = pawWanted ? pawDownY : pawUpY;
            Vector3 lp = bigPaw.localPosition;
            float speed = pawWanted ? 2.5f : 7f;
            lp.y = Mathf.MoveTowards(lp.y, targetY, speed * Time.deltaTime);
            bigPaw.localPosition = lp;
            bool down = lp.y < pawDownY + 0.3f;
            if (down && !pawDown)
            {
                Sfx.Play(this, "clunk", 0.7f);
                if (bubble != null) bubble.Show("Hehe... goal CLOSED. Don't boop my paw!");
            }
            pawDown = down;
            bigPawBlock.enabled = down;
        }

        IEnumerator Boop()
        {
            pawCool = pawReturn;
            pawDown = false;
            bigPawBlock.enabled = false;
            Sfx.Play(this, "pop", 0.9f);
            Haptics.Light();
            anger.SetActive(true);
            dazeT = 1.5f;   // offeso: per un attimo non para
            dazeSpot = new Vector2(catPrev.x < 0f ? -1.9f : 1.9f, CatHome.y + 0.3f);
            if (bubble != null) bubble.Show("Boop?! HEY! My paw!");
            yield return new WaitForSeconds(2.2f);
            anger.SetActive(false);
            if (bubble != null && !done) bubble.Hide();
        }

        // ---------- gol ----------

        IEnumerator Goal(bool forMe)
        {
            busy = true;
            puck.simulated = false;
            if (forMe)
            {
                mine++;
                Sfx.Play(this, "ding", 0.8f);
                if (bubble != null) bubble.Show(mine >= goalsToWin ? "Nooo! You win..." : "Hiss! Lucky shot.");
            }
            else
            {
                cats++;
                Haptics.Medium();
                Sfx.Play(this, "bonk", 0.9f);
                if (bubble != null) bubble.Show(cats >= goalsToWin ? "Cat wins! Purrfect." : "GOAL! Meow-ha-ha!");
            }
            ShowScore();
            if (mine >= goalsToWin || cats >= goalsToWin)
            {
                done = true;
                anger.SetActive(mine >= goalsToWin);
                for (float t = 0f; t < 1.6f; t += Time.deltaTime)
                {
                    catFace.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(t * 14f) * (mine >= goalsToWin ? 10f : 4f));
                    yield return null;
                }
                if (mine >= goalsToWin) Win(); else Fail();
                yield break;
            }
            yield return new WaitForSeconds(1.2f);
            if (bubble != null) bubble.Hide();
            Serve(forMe);
            busy = false;
        }

        IEnumerator HideLater(float s)
        {
            yield return new WaitForSeconds(s);
            if (bubble != null && !done && !busy && !pawDown) bubble.Hide();
        }
    }
}
