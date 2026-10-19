using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "Zero gravity dinner" — nella cucina della stazione spaziale cinque polpette fluttuano rimbalzando piano
    /// sulle pareti (traiettorie fisse). Si trascina la ciotola sulla loro strada per raccoglierle; quando ci sono
    /// tutte e cinque bisogna tenerla ferma 3 secondi. Due boccaporti laterali sono aperti: se una polpetta ci passa
    /// vola nello spazio (l'erogatore ne spara un'altra); tre polpette perse = sconfitta.
    /// </summary>
    public class ZeroGravityDinnerLevel : LevelController
    {
        [SerializeField] Transform bowl;          // bocca della ciotola in (0, 0.15)
        [SerializeField] Transform[] balls;
        [SerializeField] TextMesh holdText;
        [SerializeField] TextMesh goal;
        [SerializeField] SpriteRenderer[] hearts;
        [SerializeField] SpeechBubble bubble;

        const float R = 0.2f, MinX = -2.55f, MaxX = 2.55f, MinY = -4.0f, MaxY = 2.2f;
        static readonly Vector2[] StartPos = { new Vector2(-1.5f, 1.6f), new Vector2(1.2f, 1.0f), new Vector2(-0.5f, -1.6f), new Vector2(1.6f, -3.0f), new Vector2(0f, -0.2f) };
        static readonly Vector2[] StartVel = { new Vector2(0.75f, 0.45f), new Vector2(-0.6f, -0.65f), new Vector2(0.8f, 0.35f), new Vector2(-0.5f, 0.75f), new Vector2(-0.8f, -0.4f) };
        static readonly Vector2[] Respawn = { new Vector2(0.55f, -0.7f), new Vector2(-0.6f, -0.6f), new Vector2(0.4f, -0.8f) };
        static readonly Vector2[] Seats = { new Vector2(-0.3f, 0.2f), new Vector2(0.3f, 0.2f), new Vector2(0f, 0.24f), new Vector2(-0.16f, 0.42f), new Vector2(0.16f, 0.42f) };

        Camera cam;
        Vector2[] vel;
        int[] state;             // 0 libera, 1 nella ciotola, 2 persa (in attesa)
        int caught, lost, respawns;
        bool dragging, busy;
        Vector2 grabOffset, anchor;
        float still, bubbleUntil;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            vel = new Vector2[balls.Length];
            state = new int[balls.Length];
            for (int i = 0; i < balls.Length; i++)
            {
                balls[i].position = StartPos[i % StartPos.Length];
                vel[i] = StartVel[i % StartVel.Length];
            }
            if (holdText != null) holdText.text = "";
            Kit41C.Hearts(hearts, 3);
            Kit41C.Goal(goal, "CATCH ALL 5 MEATBALLS");
            Say("Dinner's floating away!", 2f);
        }

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
            DragBowl();
            float dt = Mathf.Min(Time.deltaTime, 0.05f);
            for (int i = 0; i < balls.Length; i++) if (state[i] == 0) MoveBall(i, dt);
            HoldStill(dt);
        }

        void DragBowl()
        {
            if (!Kit41C.Pointer(cam, out Vector2 w, out bool began, out bool ended)) { dragging = false; return; }
            if (began && Vector2.Distance(w, bowl.position) < 1.1f) { dragging = true; grabOffset = (Vector2)bowl.position - w; }
            if (dragging)
            {
                Vector2 p = w + grabOffset;
                bowl.position = new Vector3(Mathf.Clamp(p.x, -2.0f, 2.0f), Mathf.Clamp(p.y, -3.7f, 1.8f), 0f);
            }
            if (ended) dragging = false;
        }

        void MoveBall(int i, float dt)
        {
            Transform b = balls[i];
            Vector2 p = (Vector2)b.position + vel[i] * dt;
            // pareti con i boccaporti aperti (sinistra y 0.6..1.4, destra y -2.3..-1.5)
            if (p.x < MinX)
            {
                if (p.y > 0.6f && p.y < 1.4f) { StartCoroutine(Lose(i, -1f)); return; }
                p.x = MinX; vel[i].x = Mathf.Abs(vel[i].x); Sfx.Play(this, "tick", 0.2f);
            }
            if (p.x > MaxX)
            {
                if (p.y > -2.3f && p.y < -1.5f) { StartCoroutine(Lose(i, 1f)); return; }
                p.x = MaxX; vel[i].x = -Mathf.Abs(vel[i].x); Sfx.Play(this, "tick", 0.2f);
            }
            if (p.y < MinY) { p.y = MinY; vel[i].y = Mathf.Abs(vel[i].y); Sfx.Play(this, "tick", 0.2f); }
            if (p.y > MaxY) { p.y = MaxY; vel[i].y = -Mathf.Abs(vel[i].y); Sfx.Play(this, "tick", 0.2f); }
            b.position = p;
            b.Rotate(0f, 0f, vel[i].x * -60f * dt);
            // presa: la polpetta entra nella bocca della ciotola (molto generosa)
            Vector2 m = p - ((Vector2)bowl.position + new Vector2(0f, 0.15f));
            if (Mathf.Abs(m.x) < 0.6f && m.y > -0.35f && m.y < 0.3f) Catch(i);
        }

        void Catch(int i)
        {
            state[i] = 1;
            Transform b = balls[i];
            b.SetParent(bowl, true);
            b.localPosition = Seats[caught % Seats.Length];
            b.localRotation = Quaternion.identity;
            caught++;
            Sfx.Play(this, "pop", 0.7f);
            Haptics.Light();
            StartCoroutine(Kit41C.Pop(bowl, 0.12f));
            Kit41C.Goal(goal, caught >= balls.Length ? "NOW HOLD THE BOWL STILL!" : "MEATBALLS: " + caught + " / " + balls.Length);
            anchor = bowl.position;
            still = 0f;
        }

        void HoldStill(float dt)
        {
            if (caught < balls.Length) return;
            if (Vector2.Distance(bowl.position, anchor) > 0.12f) { anchor = bowl.position; still = 0f; }
            still += dt;
            if (holdText != null) holdText.text = still < 3f ? Mathf.CeilToInt(3f - still).ToString() : "";
            if (still >= 3f) StartCoroutine(Served());
        }

        IEnumerator Lose(int i, float side)
        {
            state[i] = 2;
            lost++;
            Kit41C.Hearts(hearts, 3 - lost);
            Haptics.Medium();
            Sfx.Play(this, "whoosh", 0.7f);
            Transform b = balls[i];
            Vector3 from = b.position;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.6f)
            {
                b.position = from + new Vector3(side * k * 1.2f, k * 0.3f, 0f);
                b.localScale = Vector3.one * (1f - k * 0.8f);
                yield return null;
            }
            b.localScale = Vector3.one;
            if (lost >= 3)
            {
                busy = true;
                Kit41C.Goal(goal, "DINNER FLOATED AWAY!");
                Say("Three meatballs lost in space!", 2f);
                yield return new WaitForSeconds(1.4f);
                Fail();
                yield break;
            }
            Say(lost == 1 ? "One floated into space!" : "Careful! Last spare meatball!", 1.8f);
            b.gameObject.SetActive(false);
            yield return new WaitForSeconds(1.0f);
            // l'erogatore ne spara una nuova
            b.position = new Vector3(0f, 2.15f, 0f);
            b.gameObject.SetActive(true);
            Sfx.Play(this, "pop", 0.6f);
            vel[i] = Respawn[respawns++ % Respawn.Length];
            state[i] = 0;
        }

        IEnumerator Served()
        {
            busy = true;
            if (holdText != null) holdText.text = "";
            Kit41C.Goal(goal, "DINNER IS SERVED!");
            Say("Mmm, zero-g meatballs!", 2f);
            Sfx.Play(this, "ding", 0.8f);
            for (int k = 0; k < 3; k++)
            {
                foreach (Transform b in balls) StartCoroutine(Kit41C.Pop(b, 0.25f, 0.2f));
                Sfx.Play(this, "pop", 0.5f);
                yield return new WaitForSeconds(0.3f);
            }
            yield return new WaitForSeconds(0.7f);
            Win();
        }
    }
}
