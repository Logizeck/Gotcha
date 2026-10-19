using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Gone Fishing — tocca per tirare SOLO quando un pesce abbocca davvero: il galleggiante va tutto sott'acqua con
    /// uno spruzzo e un grosso "!" (1.4 s per reagire), annunciato da cerchi d'acqua che si allargano. Prima arrivano
    /// pochi scherzi, piccoli e ben distinguibili: assaggi, la rana che ci salta sopra, la foglia, la papera.
    /// Tirare troppo presto o troppo tardi costa uno dei 5 tentativi. Servono 2 pesci; la seconda presa è uno
    /// scarpone ("Nice boot."): non conta, ma non costa nulla.
    /// </summary>
    public class FishingLevel : LevelController
    {
        [SerializeField] Transform bobber;       // origine sul pelo dell'acqua
        [SerializeField] Transform rod;          // perno all'impugnatura
        [SerializeField] Transform rodTip;
        [SerializeField] LineRenderer line;
        [SerializeField] Transform ripple;
        [SerializeField] Transform bigRipple;    // cerchi grandi che annunciano il morso vero
        [SerializeField] Transform splash;
        [SerializeField] Transform alertMark;
        [SerializeField] Transform frog;
        [SerializeField] Transform duck;
        [SerializeField] Transform leaf;
        [SerializeField] Transform caughtFish;
        [SerializeField] Transform caughtBoot;
        [SerializeField] SpriteRenderer[] catchIcons;
        [SerializeField] Transform[] hearts;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] float biteWindow = 1.4f;
        [SerializeField] int fishNeeded = 2;

        enum State { Busy, Waiting, Fake, Bite }
        State state = State.Busy;
        Vector3 rest, frogHome;
        Quaternion rodRest;
        Coroutine round;
        int catches, tries = 5, lastFake = -1;
        bool bootDone;

        public override void OnLevelStart()
        {
            rest = bobber.localPosition;
            frogHome = frog.localPosition;
            rodRest = rod.localRotation;
            ResetActors();
            foreach (SpriteRenderer s in catchIcons) s.color = new Color(0.1f, 0.15f, 0.25f, 0.45f);
            StartCoroutine(Intro());
        }

        IEnumerator Intro()
        {
            if (bubble != null) bubble.Show("Strike only when a fish REALLY bites!");
            yield return new WaitForSeconds(2.2f);
            if (bubble != null) bubble.Hide();
            round = StartCoroutine(Round());
        }

        void ResetActors()
        {
            bobber.localPosition = rest;
            bobber.localScale = Vector3.one;
            bobber.localRotation = Quaternion.identity;
            ripple.gameObject.SetActive(false);
            if (bigRipple != null) bigRipple.gameObject.SetActive(false);
            splash.gameObject.SetActive(false);
            alertMark.gameObject.SetActive(false);
            frog.gameObject.SetActive(true);
            frog.localPosition = frogHome;
            frog.localScale = Vector3.one * 0.9f;
            duck.gameObject.SetActive(false);
            leaf.gameObject.SetActive(false);
            caughtFish.gameObject.SetActive(false);
            caughtBoot.gameObject.SetActive(false);
            rod.localRotation = rodRest;
        }

        void LateUpdate()
        {
            line.SetPosition(0, rodTip.position);
            line.SetPosition(1, bobber.position + bobber.up * 0.3f * bobber.localScale.y);
        }

        void Update()
        {
            if (IsFinished || state == State.Busy) return;
            if (!PointerInput.AnyTouchBegan() || PointerInput.IsOverUi()) return;
            Haptics.Light();
            if (round != null) StopCoroutine(round);
            round = null;
            if (state == State.Bite) StartCoroutine(Catch());
            else StartCoroutine(Penalty("Too early! You scared it."));
            state = State.Busy;
        }

        IEnumerator Round()
        {
            ResetActors();
            state = State.Waiting;
            // pochi scherzi: uno, a volte due dopo la prima presa
            int fakes = 1 + (catches > 0 && Random.value < 0.5f ? 1 : 0);
            for (int i = 0; i < fakes; i++)
            {
                yield return new WaitForSeconds(Random.Range(1.0f, 2.2f));
                int kind;
                do kind = Random.Range(0, 4); while (kind == lastFake);
                lastFake = kind;
                state = State.Fake;
                switch (kind)
                {
                    case 0: yield return Nibble(); break;
                    case 1: yield return FrogJump(); break;
                    case 2: yield return LeafDrift(); break;
                    default: yield return DuckPeck(); break;
                }
                state = State.Waiting;
            }
            yield return new WaitForSeconds(Random.Range(0.8f, 1.8f));
            // il morso vero, ben annunciato: cerchi grandi e due strattoni (toccare qui non costa nulla), poi giù fino in fondo
            state = State.Busy;
            if (bigRipple != null) StartCoroutine(Rings());
            yield return Dip(0.16f, 0.12f);
            yield return Dip(0.2f, 0.12f);
            state = State.Bite;
            Sfx.Play(this, "whoosh", 0.8f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.12f) { Sink(k); yield return null; }
            Sink(1f);
            splash.gameObject.SetActive(true);
            alertMark.gameObject.SetActive(true);
            StartCoroutine(Pop(splash, 0.15f));
            StartCoroutine(Pop(alertMark, 0.12f));
            StartCoroutine(Pulse(alertMark));
            yield return new WaitForSeconds(biteWindow);
            state = State.Busy;
            round = null;
            yield return Penalty("It got away!");
        }

        IEnumerator Rings()
        {
            bigRipple.gameObject.SetActive(true);
            bigRipple.localPosition = rest;
            for (int i = 0; i < 3 && !IsFinished; i++)
            {
                Sfx.Play(this, "tick", 0.7f);
                yield return Grow(bigRipple, 0.6f, 2.2f, 0.35f);
            }
            if (state != State.Bite) yield break;
            // finché il pesce è attaccato, i cerchi continuano
            while (state == State.Bite && bigRipple.gameObject.activeSelf) yield return Grow(bigRipple, 0.8f, 2.4f, 0.4f);
        }

        IEnumerator Pulse(Transform t)
        {
            yield return new WaitForSeconds(0.15f);
            Vector3 size = t.localScale;
            for (float k = 0f; state == State.Bite && t.gameObject.activeSelf; k += Time.deltaTime)
            {
                t.localScale = size * (1f + 0.15f * Mathf.Sin(k * 18f));
                yield return null;
            }
            t.localScale = size;
        }

        void Sink(float k)
        {
            bobber.localScale = new Vector3(1f, 1f - k, 1f);
            bobber.localPosition = rest + Vector3.down * 0.08f * k;
        }

        IEnumerator Dip(float depth, float seconds)
        {
            for (float k = 0f; k < 1f; k += Time.deltaTime / seconds) { Twitch(Mathf.Sin(k * Mathf.PI) * depth); yield return null; }
            Twitch(0f);
            yield return new WaitForSeconds(0.12f);
        }

        void Twitch(float d)
        {
            bobber.localPosition = rest + Vector3.down * d * 0.5f;
            bobber.localScale = new Vector3(1f, 1f - d * 2.2f, 1f);
        }

        IEnumerator Nibble()
        {
            ripple.gameObject.SetActive(true);
            ripple.localPosition = rest;
            for (int i = 0; i < 3; i++)
            {
                Sfx.Play(this, "tick", 0.5f);
                ripple.localScale = Vector3.one * 0.4f;
                StartCoroutine(Grow(ripple, 0.4f, 0.9f, 0.4f));
                yield return Dip(0.035f, 0.1f);
                yield return new WaitForSeconds(0.2f);
            }
            ripple.gameObject.SetActive(false);
        }

        IEnumerator FrogJump()
        {
            Vector3 top = rest + new Vector3(-0.05f, 0.22f, 0f);
            yield return Arc(frog, frogHome, top, 1.1f, 0.5f);
            Sfx.Play(this, "pop", 0.6f);
            for (float k = 0f; k < 0.6f; k += Time.deltaTime) { float d = 0.06f * (1f - k / 0.6f); Twitch(d); frog.localPosition = top + Vector3.down * d * 0.5f; yield return null; }
            Twitch(0f);
            yield return Arc(frog, frog.localPosition, frogHome, 1.0f, 0.5f);
            Sfx.Play(this, "pop", 0.4f);
        }

        IEnumerator LeafDrift()
        {
            leaf.gameObject.SetActive(true);
            Vector3 from = rest + new Vector3(-3.4f, -0.04f, 0f), touch = rest + new Vector3(-0.32f, -0.04f, 0f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 1.8f)
            {
                leaf.localPosition = Vector3.Lerp(from, touch, Mathf.SmoothStep(0f, 1f, k));
                leaf.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(k * 9f) * 12f);
                yield return null;
            }
            Sfx.Play(this, "tick", 0.5f);
            for (float k = 0f; k < 0.8f; k += Time.deltaTime)
            {
                bobber.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(k * 25f) * 14f * (1f - k / 0.8f));
                leaf.localPosition = touch + Vector3.down * Mathf.Min(1f, k / 0.8f) * 0.25f;
                yield return null;
            }
            bobber.localRotation = Quaternion.identity;
            Vector3 at = leaf.localPosition;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 1.6f) { leaf.localPosition = Vector3.Lerp(at, at + new Vector3(3.3f, -0.4f, 0f), k); yield return null; }
            leaf.gameObject.SetActive(false);
        }

        IEnumerator DuckPeck()
        {
            duck.gameObject.SetActive(true);
            duck.localScale = Vector3.one * 0.9f;
            Vector3 from = rest + new Vector3(3.3f, 0.02f, 0f), near = rest + new Vector3(0.7f, 0.02f, 0f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 1.5f)
            {
                duck.localPosition = Vector3.Lerp(from, near, Mathf.SmoothStep(0f, 1f, k)) + Vector3.up * Mathf.Sin(k * 20f) * 0.015f;
                yield return null;
            }
            for (int i = 0; i < 2; i++)
            {
                for (float k = 0f; k < 1f; k += Time.deltaTime / 0.25f)
                {
                    float s = Mathf.Sin(k * Mathf.PI);
                    duck.localRotation = Quaternion.Euler(0, 0, 25f * s);
                    Twitch(0.05f * s);
                    yield return null;
                }
                Sfx.Play(this, "pop", 0.5f);
                yield return new WaitForSeconds(0.15f);
            }
            duck.localRotation = Quaternion.identity;
            Twitch(0f);
            duck.localScale = new Vector3(-0.9f, 0.9f, 1f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 1.4f) { duck.localPosition = Vector3.Lerp(near, from, k); yield return null; }
            duck.gameObject.SetActive(false);
        }

        IEnumerator Arc(Transform t, Vector3 from, Vector3 to, float height, float seconds)
        {
            for (float k = 0f; k < 1f; k += Time.deltaTime / seconds)
            {
                t.localPosition = Vector3.Lerp(from, to, k) + Vector3.up * Mathf.Sin(k * Mathf.PI) * height;
                yield return null;
            }
            t.localPosition = to;
        }

        IEnumerator Grow(Transform t, float from, float to, float seconds)
        {
            for (float k = 0f; k < 1f && t.gameObject.activeSelf; k += Time.deltaTime / seconds) { t.localScale = Vector3.one * Mathf.Lerp(from, to, k); yield return null; }
        }

        IEnumerator Pop(Transform t, float seconds)
        {
            Vector3 size = t.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / seconds) { t.localScale = size * Mathf.Lerp(0.3f, 1f, k); yield return null; }
            t.localScale = size;
        }

        IEnumerator Catch()
        {
            bool boot = catches == 1 && !bootDone;
            Transform prize = boot ? caughtBoot : caughtFish;
            alertMark.gameObject.SetActive(false);
            if (bigRipple != null) bigRipple.gameObject.SetActive(false);
            Sfx.Play(this, "whoosh", 0.8f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.15f) { rod.localRotation = rodRest * Quaternion.Euler(0, 0, 14f * k); yield return null; }
            bobber.localScale = Vector3.one;
            prize.gameObject.SetActive(true);
            Vector3 show = new Vector3(0f, 1.2f, 0f);
            yield return Arc(prize, rest, show, 1.0f, 0.5f);
            splash.gameObject.SetActive(false);
            Haptics.Medium();
            Sfx.Play(this, boot ? "bonk" : "ding", 0.7f);
            for (float k = 0f; k < 0.5f; k += Time.deltaTime) { prize.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(k * 30f) * 12f); yield return null; }
            prize.localRotation = Quaternion.identity;
            if (boot)
            {
                bootDone = true;
                if (bubble != null) bubble.Show("Nice boot. Doesn't count.");
                yield return new WaitForSeconds(1.4f);
                yield return Arc(prize, show, show + new Vector3(3.5f, -2f, 0f), 0.6f, 0.6f);
            }
            else
            {
                catches++;
                if (bubble != null) bubble.Show(catches >= fishNeeded ? "What a catch!" : "Got one!");
                Transform icon = catchIcons[catches - 1].transform;
                yield return new WaitForSeconds(0.6f);
                Vector3 from = prize.position;
                for (float k = 0f; k < 1f; k += Time.deltaTime / 0.35f) { prize.position = Vector3.Lerp(from, icon.position, k); yield return null; }
                catchIcons[catches - 1].color = Color.white;
                StartCoroutine(Pop(icon, 0.2f));
            }
            prize.gameObject.SetActive(false);
            if (catches >= fishNeeded)
            {
                yield return new WaitForSeconds(1.0f);
                Win();
                yield break;
            }
            if (bubble != null) bubble.Hide();
            round = StartCoroutine(Round());
        }

        IEnumerator Penalty(string message)
        {
            state = State.Busy;
            ResetActors();
            Haptics.Medium();
            Sfx.Play(this, "buzz", 0.5f);
            tries--;
            if (tries >= 0 && tries < hearts.Length) hearts[tries].gameObject.SetActive(false);
            if (bubble != null) bubble.Show(message);
            ripple.gameObject.SetActive(true);
            ripple.localPosition = rest + new Vector3(0.6f, -0.3f, 0f);
            yield return Grow(ripple, 0.3f, 1.3f, 0.6f);
            yield return new WaitForSeconds(0.9f);
            if (tries <= 0)
            {
                if (bubble != null) bubble.Show("No fish today...");
                yield return new WaitForSeconds(1.2f);
                Fail();
                yield break;
            }
            if (bubble != null) bubble.Hide();
            round = StartCoroutine(Round());
        }
    }
}
