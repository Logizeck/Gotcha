using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 25 — "MOVE, TONI!". Toni dorme... no, è sdraiato proprio sul pulsante da premere, ed è pesantissimo:
    /// spingerlo non serve (sbuffa e torna al suo posto). Non stacca gli occhi dalla sua pallina. La soluzione è
    /// LANCIARE la pallina lontano (trascinala e lasciala andare con uno scatto): Toni le corre dietro, e mentre è
    /// via il pulsante è libero. Ma torna in fretta e si risdraia lì sopra!
    /// </summary>
    public class ToniButtonLevel : LevelController
    {
        [SerializeField] ToniPug toni;
        [SerializeField] TapButton button;
        [SerializeField] Transform ball;
        [SerializeField] float ballRadius = 0.2f;
        [SerializeField] float floorY = -3.2f;
        [SerializeField] float wallX = 2.5f;
        [SerializeField] float ceilingY = 3.4f;
        [SerializeField] float gravity = 18f;
        [SerializeField] float fetchDistance = 1.5f;

        enum State { OnButton, Fetching, Back }

        State state;
        Camera cam;
        Vector3 home;
        Vector2 vel, lastPointer;
        bool holding, carried, thrown;
        float pokeCooldown, nextBark;
        int pokes;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            home = toni.transform.localPosition;
            toni.Stand(false);
            toni.WakeUp(2);
            toni.LookTarget = ball;
            toni.Wag = 0.35f;
            button.Pressed += OnButton;
            nextBark = Time.time + 2f;
        }

        void OnDestroy() { if (button != null) button.Pressed -= OnButton; }

        void Update()
        {
            if (IsFinished) return;
            pokeCooldown -= Time.deltaTime;
            HandlePointer();
            if (!holding && !carried) MoveBall();
            if (carried)
            {
                Vector3 m = toni.MouthWorld;
                ball.position = new Vector3(m.x, m.y - 0.05f, ball.position.z);
            }
            if (state == State.OnButton && Time.time >= nextBark)
            {
                nextBark = Time.time + Random.Range(4f, 6f);
                toni.Bark(Random.value < 0.5f ? "Woof?" : "Ball? BALL?");
            }
            // lanciata lontano: Toni parte all'inseguimento
            if (state == State.OnButton && thrown && Mathf.Abs(ball.position.x - toni.transform.position.x) > fetchDistance)
                StartCoroutine(Fetch());
        }

        void HandlePointer()
        {
            if (!PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out bool ended) || cam == null) { if (holding) Release(); return; }
            Vector2 w = PointerInput.ToWorld(cam, sp);
            if (began && !PointerInput.IsOverUi())
            {
                if (!carried && Vector2.Distance(w, ball.position) < ballRadius + 0.3f)
                {
                    holding = true;
                    thrown = false;
                    vel = Vector2.zero;
                    lastPointer = w;
                }
                else if (state == State.OnButton && IsOnToni(w)) Poke();
            }
            if (!holding) return;
            Vector2 d = w - lastPointer;
            if (Time.deltaTime > 0f) vel = Vector2.Lerp(vel, d / Time.deltaTime, 0.5f);
            lastPointer = w;
            ball.position = new Vector3(Mathf.Clamp(w.x, -wallX, wallX), Mathf.Clamp(w.y, floorY, ceilingY), ball.position.z);
            if (ended) Release();
        }

        void Release()
        {
            holding = false;
            vel = Vector2.ClampMagnitude(vel, 16f);
            thrown = vel.magnitude > 3f;
            if (thrown) Sfx.Play(this, "whoosh", 0.5f);
        }

        bool IsOnToni(Vector2 w)
        {
            Vector2 l = toni.transform.InverseTransformPoint(w);
            return (l.x * l.x) / (1.3f * 1.3f) + ((l.y - 0.2f) * (l.y - 0.2f)) / (0.7f * 0.7f) < 1f;
        }

        void Poke()
        {
            if (pokeCooldown > 0f) return;
            pokeCooldown = 0.6f;
            pokes++;
            string[] lines = { "Hmpf!", "*snort*", "Nope.", "I'm comfy.", "Grrr..." };
            toni.Say(lines[Mathf.Min(pokes - 1, lines.Length - 1)], 1.1f, "bonk");
            StartCoroutine(Wiggle());
        }

        IEnumerator Wiggle()
        {
            // si sposta di un pelo e torna giù come un sacco di patate
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.35f)
            {
                if (state != State.OnButton) yield break;
                toni.transform.localPosition = home + new Vector3(Mathf.Sin(k * Mathf.PI) * 0.12f, 0f, 0f);
                yield return null;
            }
            if (state == State.OnButton) toni.transform.localPosition = home;
        }

        void MoveBall()
        {
            Vector3 p = ball.position;
            bool resting = Mathf.Abs(p.y - floorY) < 0.01f && vel.sqrMagnitude < 0.04f;
            if (resting) { vel = Vector2.zero; return; }
            vel.y -= gravity * Time.deltaTime;
            p += (Vector3)(vel * Time.deltaTime);
            if (p.y < floorY)
            {
                p.y = floorY;
                if (vel.y < -2f) Sfx.Play(this, "pop", Mathf.Clamp01(-vel.y / 12f) * 0.6f);
                vel.y = -vel.y * 0.55f;
                vel.x *= 0.8f;
                if (Mathf.Abs(vel.y) < 0.6f) vel.y = 0f;
            }
            if (p.y > ceilingY) { p.y = ceilingY; vel.y = -Mathf.Abs(vel.y) * 0.5f; }
            if (Mathf.Abs(p.x) > wallX) { p.x = Mathf.Sign(p.x) * wallX; vel.x = -vel.x * 0.6f; Sfx.Play(this, "bonk", 0.3f); }
            if (Mathf.Abs(p.y - floorY) < 0.01f) vel.x = Mathf.MoveTowards(vel.x, 0f, 3f * Time.deltaTime);
            ball.position = p;
            ball.Rotate(0, 0, -vel.x * 200f * Time.deltaTime);
        }

        IEnumerator Fetch()
        {
            state = State.Fetching;
            thrown = false;
            toni.Alert(true);
            toni.Bark("WOOF!!");
            yield return toni.Jump(0.35f, 0.3f);
            toni.Alert(false);
            // insegue la pallina finché non la raggiunge (anche se sta ancora rotolando)
            float timeout = Time.time + 6f;
            while (Time.time < timeout)
            {
                float target = toni.transform.parent.InverseTransformPoint(ball.position).x + (ball.position.x > toni.transform.position.x ? -0.9f : 0.9f);
                if (holding) target = toni.transform.localPosition.x;
                if (Mathf.Abs(toni.transform.localPosition.x - target) < 0.08f && !holding && vel.sqrMagnitude < 1f) break;
                IEnumerator run = toni.RunTo(target, 3.4f);
                float stepUntil = Time.time + 0.12f;
                while (Time.time < stepUntil && run.MoveNext()) yield return run.Current;
            }
            if (!holding)
            {
                carried = true;
                vel = Vector2.zero;
                Sfx.Play(this, "pop", 0.6f);
                toni.Wag = 1f;
                // scuote la pallina tutto contento
                for (float k = 0f; k < 0.5f; k += Time.deltaTime)
                {
                    toni.transform.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(k * 40f) * 6f);
                    yield return null;
                }
                toni.transform.localRotation = Quaternion.identity;
            }
            state = State.Back;
            yield return toni.RunTo(home.x, 2.6f);
            toni.Face(false);
            toni.Stand(false);
            toni.transform.localPosition = home;
            toni.GroundY = home.y;
            if (carried)
            {
                carried = false;
                ball.position = toni.transform.TransformPoint(new Vector2(-1.5f, -0.2f));   // la lascia lì davanti, pronta per un altro lancio
                vel = new Vector2(-1f, 2f);
            }
            toni.Say("*plop*", 0.8f, "bonk");
            toni.Wag = 0.35f;
            state = State.OnButton;
        }

        void OnButton(TapButton b)
        {
            if (IsFinished) return;
            if (state == State.OnButton || Mathf.Abs(toni.transform.localPosition.x - home.x) < 0.9f) return;   // sotto Toni: non si preme
            StartCoroutine(WinRoutine());
        }

        IEnumerator WinRoutine()
        {
            button.interactable = false;
            Sfx.Play(this, "ding", 0.8f);
            Haptics.Light();
            toni.Say("WOOF?!", 1.2f, "woof");
            yield return new WaitForSeconds(1.2f);
            Win();
        }
    }
}
