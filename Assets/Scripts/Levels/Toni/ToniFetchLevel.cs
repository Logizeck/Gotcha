using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 150 — "FETCH!", il livello composito di Toni al parco, in quattro tappe:
    /// 1) la pallina di Toni è incastrata sull'albero: si scuote il telefono finché cade... dritta nello stagno;
    /// 2) galleggia: si soffia nel microfono (o si "sventola" col dito sull'acqua) per spingerla fino alla riva;
    /// 3) arriva il gatto e ci si siede sopra: si trascina il gomitolo lontano e il gatto gli corre dietro;
    /// 4) si lancia la pallina con uno swipe verso l'alto: Toni corre, salta e la prende al volo.
    /// </summary>
    public class ToniFetchLevel : LevelController
    {
        [SerializeField] ToniPug toni;
        [SerializeField] Transform tree;
        [SerializeField] Transform ball;
        [SerializeField] Transform cat;
        [SerializeField] Draggable2D yarn;
        [SerializeField] Collider2D water;
        [SerializeField] TextMesh goal;
        [SerializeField] SpriteRenderer[] stageDots;
        [SerializeField] SmokeEmitter splash;
        [SerializeField] SmokeEmitter confetti;
        [SerializeField] Vector2 pondLanding = new Vector2(-1.15f, -0.75f);
        [SerializeField] float pondExitX = 1.65f;
        [SerializeField] Vector2 shore = new Vector2(1.75f, -2.15f);
        [SerializeField] float shakeNeeded = 1.3f;     // secondi di scuotimento
        [SerializeField] float blowLevel = 0.3f;

        Camera cam;
        int stage;
        float shook, nextNag;

        static readonly Color DotOff = new Color(1f, 1f, 1f, 0.35f), DotOn = new Color(1f, 0.85f, 0.25f);

        public override void OnLevelStart()
        {
            cam = Camera.main;
            cat.gameObject.SetActive(false);
            yarn.gameObject.SetActive(false);
            yarn.returnOnMiss = false;
            if (splash != null) splash.Stop();
            if (confetti != null) confetti.Stop();
            toni.Stand(true);
            toni.Face(false);
            toni.WakeUp(2);
            toni.LookTarget = ball;
            StartCoroutine(Run());
        }

        void OnDestroy() { Inputs.ReleaseMic(this); }

        void Stage(int i, string text)
        {
            stage = i;
            goal.text = text;
            for (int d = 0; d < stageDots.Length; d++) stageDots[d].color = d < i ? DotOn : DotOff;
            if (i > 0) { Sfx.Play(this, "ding", 0.5f); Haptics.Light(); }
        }

        IEnumerator Run()
        {
            // ---------- 1: la pallina sull'albero ----------
            Stage(0, "Get the ball out of the tree");
            toni.Say("My ball! It's stuck up there!", 2f, "woof");
            nextNag = Time.time + 6f;
            while (shook < shakeNeeded)
            {
                float s = Inputs.Shake();
                if (s > 6f)
                {
                    shook += Time.deltaTime;
                    tree.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(Time.time * 40f) * 3f * Mathf.Clamp01(s / 15f));
                    ball.localPosition += (Vector3)Random.insideUnitCircle * 0.01f;
                    if (Random.value < 0.08f) Haptics.Light();
                }
                else tree.localRotation = Quaternion.Slerp(tree.localRotation, Quaternion.identity, Time.deltaTime * 8f);
                if (PointerInput.AnyTouchBegan() && Time.time > nextNag)
                {
                    nextNag = Time.time + 3f;
                    toni.Say("Too high to reach! SHAKE it!", 1.6f);
                }
                yield return null;
            }
            tree.localRotation = Quaternion.identity;
            Sfx.Play(this, "whoosh", 0.8f);
            // cade... nello stagno
            Vector3 from = ball.localPosition, to = pondLanding;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.7f)
            {
                ball.localPosition = Vector3.Lerp(from, to, k) + Vector3.up * Mathf.Sin(k * Mathf.PI) * 0.5f;
                ball.localRotation = Quaternion.Euler(0, 0, -k * 540f);
                yield return null;
            }
            ball.localPosition = to;
            Sfx.Play(this, "bonk", 0.8f);
            Haptics.Medium();
            if (splash != null) StartCoroutine(Burst(splash, ball.position, 0.3f));
            toni.Say("NOOO! The pond!", 1.5f, "woof");
            yield return new WaitForSeconds(1.2f);

            // ---------- 2: soffia la pallina fino a riva ----------
            Stage(1, "Bring the ball to the shore");
            toni.Say("I can't swim... Blow it over here!", 2.2f);
            float bob = 0f;
            Vector2 lastFinger = Vector2.zero;
            bool fingerOn = false;
            nextNag = Time.time + 8f;
            while (ball.localPosition.x < pondExitX)
            {
                float push = 0f;
                float mic = Inputs.Mic(this);
                if (mic > blowLevel) push += Mathf.Lerp(0.5f, 1.4f, Mathf.InverseLerp(blowLevel, 1f, mic));
                // in alternativa: sventolare col dito sull'acqua dietro la pallina
                if (cam != null && PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out _))
                {
                    Vector2 w = PointerInput.ToWorld(cam, sp);
                    if (began || !fingerOn) lastFinger = w;
                    fingerOn = true;
                    if (water.OverlapPoint(w) && w.x < ball.position.x && w.x - lastFinger.x > 0f)
                        push += Mathf.Min(1.2f, (w.x - lastFinger.x) / Mathf.Max(Time.deltaTime, 0.001f) * 0.08f);
                    lastFinger = w;
                }
                else fingerOn = false;
                bob += Time.deltaTime;
                Vector3 p = ball.localPosition;
                p.x += push * Time.deltaTime;
                p.y = pondLanding.y + Mathf.Sin(bob * 2.5f) * 0.04f;
                ball.localPosition = p;
                ball.localRotation = Quaternion.Euler(0, 0, -p.x * 120f);
                if (push > 0f) nextNag = Time.time + 8f;
                else if (Time.time > nextNag) { nextNag = Time.time + 8f; toni.Say("Blow! Like a birthday candle!", 1.8f); }
                yield return null;
            }
            Inputs.ReleaseMic(this);
            // a riva rotolando
            from = ball.localPosition;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.6f)
            {
                ball.localPosition = Vector3.Lerp(from, shore, k) + Vector3.up * Mathf.Sin(k * Mathf.PI) * 0.25f;
                ball.localRotation = Quaternion.Euler(0, 0, -400f * k);
                yield return null;
            }
            ball.localPosition = shore;
            Sfx.Play(this, "pop", 0.6f);
            toni.Say("Yes! My ball!", 1f, "woof");
            yield return new WaitForSeconds(0.6f);

            // ---------- 3: il gatto si siede sulla pallina ----------
            cat.gameObject.SetActive(true);
            Vector3 catSeat = (Vector3)shore + new Vector3(0.05f, 0.45f, 0f);
            cat.localPosition = catSeat + Vector3.right * 3f;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.7f)
            {
                cat.localPosition = Vector3.Lerp(catSeat + Vector3.right * 3f, catSeat, k) + Vector3.up * Mathf.Abs(Mathf.Sin(k * Mathf.PI * 4f)) * 0.12f;
                yield return null;
            }
            cat.localPosition = catSeat;
            Sfx.Play(this, "bonk", 0.4f);
            toni.Say("Hey! That's MY ball!", 1.5f, "woof");
            yield return new WaitForSeconds(0.8f);
            Stage(2, "Get the cat off the ball");
            yarn.gameObject.SetActive(true);
            nextNag = Time.time + 10f;
            while (true)
            {
                bool dragging = Draggable2D.Active == yarn;
                // il gatto segue il gomitolo con lo sguardo (si gira)
                Vector3 cs = cat.localScale;
                cs.x = Mathf.Abs(cs.x) * (yarn.transform.position.x < cat.position.x ? 1f : -1f);
                cat.localScale = cs;
                if (!dragging && Vector2.Distance(yarn.transform.position, cat.position) > 1.6f && yarn.transform.position.y < 0.5f) break;
                if (PointerInput.AnyTouchBegan() && !dragging && Time.time > nextNag)
                {
                    nextNag = Time.time + 6f;
                    toni.Say("Cats can't resist yarn...", 1.6f);
                }
                yield return null;
            }
            // il gatto balza sul gomitolo e se ne va giocando
            yarn.enabled = false;
            Vector3 catFrom = cat.localPosition, yarnAt = yarn.transform.localPosition + new Vector3(0f, 0.35f, 0f);
            Sfx.Play(this, "whoosh", 0.7f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.5f)
            {
                cat.localPosition = Vector3.Lerp(catFrom, yarnAt, k) + Vector3.up * Mathf.Sin(k * Mathf.PI) * 0.8f;
                yield return null;
            }
            Haptics.Light();
            StartCoroutine(CatPlays());
            yield return new WaitForSeconds(0.5f);

            // ---------- 4: lancia la pallina a Toni ----------
            Stage(3, "Throw the ball for Toni!");
            toni.Say("Throw it! THROW IT!", 1.8f, "woof");
            toni.Wag = 1f;
            Vector2 throwVel = Vector2.zero;
            while (throwVel == Vector2.zero)
            {
                yield return StartCoroutine(WaitFlick(v => throwVel = v));
                if (throwVel.y < 2f) { toni.Say("Higher! Swipe UP!", 1.2f); throwVel = Vector2.zero; ball.localPosition = shore; }
            }
            yield return StartCoroutine(Fetch(throwVel));
        }

        /// <summary>Aspetta uno swipe che parte dalla pallina e restituisce la velocità del lancio (unità/s).</summary>
        IEnumerator WaitFlick(System.Action<Vector2> done)
        {
            bool holding = false;
            Vector2 start = Vector2.zero, last = Vector2.zero, vel = Vector2.zero;
            while (true)
            {
                if (cam != null && PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out bool ended))
                {
                    Vector2 w = PointerInput.ToWorld(cam, sp);
                    if (began && Vector2.Distance(w, ball.position) < 0.9f) { holding = true; start = last = w; vel = Vector2.zero; }
                    else if (holding)
                    {
                        Vector2 v = (w - last) / Mathf.Max(Time.deltaTime, 0.001f);
                        vel = Vector2.Lerp(vel, v, 0.5f);
                        last = w;
                        ball.position = new Vector3(Mathf.Lerp(shore.x, w.x, 0.3f), Mathf.Lerp(shore.y, w.y, 0.3f), ball.position.z);
                        if (ended)
                        {
                            holding = false;
                            if ((w - start).magnitude > 0.5f) { done(vel); yield break; }
                            ball.localPosition = shore;
                        }
                    }
                }
                else if (holding)
                {
                    holding = false;
                    if ((last - start).magnitude > 0.5f) { done(vel); yield break; }
                    ball.localPosition = shore;
                }
                yield return null;
            }
        }

        IEnumerator Fetch(Vector2 vel)
        {
            Sfx.Play(this, "whoosh", 1f);
            // lancio a parabola: la pallina vola nel cielo del parco e ricade sul prato
            float vy = Mathf.Clamp(vel.y * 0.3f, 4f, 8f);
            Vector3 p = ball.localPosition;
            float groundY = toni.transform.localPosition.y + 0.5f;
            float flight = (vy + Mathf.Sqrt(vy * vy + 18f * Mathf.Max(0f, p.y - groundY))) / 9f;   // g = 9
            float landX = Mathf.Clamp(p.x + vel.x * 0.25f * flight, -2.2f, 2.2f);
            float vx = (landX - p.x) / flight;
            StartCoroutine(toni.RunTo(landX + 0.1f, 4.2f));
            bool caught = false;
            float t = 0f;
            Vector3 p0 = p;
            while (!caught)
            {
                t += Time.deltaTime;
                p = p0 + new Vector3(vx * t, vy * t - 4.5f * t * t, 0f);
                ball.localPosition = p;
                ball.localRotation = Quaternion.Euler(0, 0, -t * 720f);
                float fall = vy - 9f * t;
                if (fall < 0f && p.y < groundY + 1.1f && Mathf.Abs(toni.transform.localPosition.x - p.x) < 0.8f)
                {
                    caught = true;
                    StartCoroutine(toni.Jump(0.6f, 0.3f));
                }
                if (p.y < groundY - 0.3f) { p.y = groundY - 0.3f; ball.localPosition = p; break; }
                yield return null;
            }
            // al volo o da terra: Toni la prende comunque
            toni.MouthOpen(true);
            yield return new WaitForSeconds(0.15f);
            Vector3 b0 = ball.position;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.15f)
            {
                Vector3 m = toni.MouthWorld; m.z = b0.z;
                ball.position = Vector3.Lerp(b0, m, k);
                yield return null;
            }
            toni.MouthOpen(false);
            Sfx.Play(this, "bonk", 0.8f);
            Haptics.Medium();
            toni.Say(caught ? "GOT IT! Best catch EVER!" : "Got it! Again! AGAIN!", 1.8f, "woof");
            goal.text = "";
            for (int d = 0; d < stageDots.Length; d++) stageDots[d].color = DotOn;
            if (confetti != null) StartCoroutine(Burst(confetti, toni.HeadWorld + Vector3.up, 0.8f));
            float until = Time.time + 2.2f;
            StartCoroutine(toni.Happy(2f));
            while (Time.time < until)
            {
                Vector3 m = toni.MouthWorld; m.z = ball.position.z;
                ball.position = m;
                yield return null;
            }
            Win();
        }

        IEnumerator CatPlays()
        {
            Vector3 c0 = cat.localPosition, y0 = yarn.transform.localPosition;
            for (float t = 0f; !IsFinished; t += Time.deltaTime)
            {
                cat.localPosition = c0 + new Vector3(Mathf.Sin(t * 2f) * 0.3f, Mathf.Abs(Mathf.Sin(t * 6f)) * 0.1f, 0f);
                yarn.transform.localPosition = y0 + new Vector3(Mathf.Sin(t * 2f + 0.6f) * 0.35f, 0f, 0f);
                yarn.transform.localRotation = Quaternion.Euler(0, 0, -t * 200f);
                yield return null;
            }
        }

        IEnumerator Burst(SmokeEmitter e, Vector3 at, float seconds)
        {
            e.transform.position = at;
            e.Play();
            yield return new WaitForSeconds(seconds);
            e.Stop();
        }
    }
}
