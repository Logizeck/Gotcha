using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "Claw machine" — la gru del luna park. Muovi l'artiglio con le frecce (o trascinando sul vetro) e premi
    /// DROP: scende, si chiude, risale e porta il peluche allo scivolo. Se non è ben centrato il peluche può
    /// scivolare. Cinque gettoni, uno per discesa. Bisogna pescare l'orsetto con la CHIAVE d'oro, ma all'inizio ha un
    /// pesce di peluche sopra: prima togli il pesce, poi prendi l'orsetto. Gettoni finiti = sconfitta.
    /// </summary>
    public class ClawMachineLevel : LevelController
    {
        [SerializeField] Transform claw;               // il punto di presa
        [SerializeField] SpriteRenderer clawArt;
        [SerializeField] Sprite openSprite, closedSprite;
        [SerializeField] Transform carriage;           // il carrello sul binario
        [SerializeField] LineRenderer cable;
        [SerializeField] Transform[] toys;
        [SerializeField] Transform keyToy;
        [SerializeField] Transform keyPrize;           // la chiave che esce dallo sportello
        [SerializeField] TapButton dropButton;
        [SerializeField] Collider2D leftArrow, rightArrow, glass;
        [SerializeField] TextMesh coinsText;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] SmokeEmitter[] confetti;
        [SerializeField] int coins = 5;
        [SerializeField] float minX = -1.8f, maxX = 1.95f, restY = 2.0f, floorTop = -1.3f, chuteEdge = -1.3f, chuteX = -1.8f;
        [SerializeField] float halfW = 0.38f, halfH = 0.31f, grabTolerance = 0.25f, moveSpeed = 1.8f;

        readonly HashSet<Transform> gone = new HashSet<Transform>();
        readonly Dictionary<Transform, float> fallSpeed = new Dictionary<Transform, float>();
        Transform held;
        bool busy, done;
        Camera cam;
        float cableTop;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            cableTop = carriage.position.y;
            keyPrize.gameObject.SetActive(false);
            foreach (SmokeEmitter c in confetti) if (c != null) c.Stop();
            foreach (Transform t in toys) fallSpeed[t] = 0f;
            claw.position = new Vector3(0f, restY, 0f);
            clawArt.sprite = openSprite;
            dropButton.Pressed += _ => { if (!busy && !done && coins > 0) StartCoroutine(DropSeq()); };
            Coins();
            Say("Win me the KEY! I have 5 coins.", 2.6f);
        }

        void Coins() { coinsText.text = "COINS: " + coins; }

        void Update()
        {
            if (!busy && !done && !IsFinished) Steer();
            Settle();
            Vector3 c = claw.position;
            carriage.position = new Vector3(c.x, carriage.position.y, carriage.position.z);
            cable.SetPosition(0, new Vector3(c.x, cableTop, 0f));
            cable.SetPosition(1, new Vector3(c.x, c.y + 0.5f, 0f));
        }

        void Steer()
        {
            float x = claw.position.x;
            if (PointerInput.IsHeld(cam, leftArrow)) x -= moveSpeed * Time.deltaTime;
            else if (PointerInput.IsHeld(cam, rightArrow)) x += moveSpeed * Time.deltaTime;
            else if (PointerInput.TryGetPrimary(out Vector2 sp, out _, out bool ended) && !ended)
            {
                Vector2 w = PointerInput.ToWorld(cam, sp);
                if (glass.OverlapPoint(w)) x = Mathf.MoveTowards(x, w.x, moveSpeed * 1.6f * Time.deltaTime);
            }
            claw.position = new Vector3(Mathf.Clamp(x, minX, maxX), claw.position.y, 0f);
        }

        /// <summary>Quota a cui si appoggia un peluche in x (sul pavimento o su un altro peluche più in basso).</summary>
        float Support(Transform toy, float x, float y)
        {
            float s = floorTop + halfH;
            foreach (Transform o in toys)
            {
                if (o == toy || o == held || gone.Contains(o)) continue;
                if (Mathf.Abs(o.position.x - x) < halfW * 1.6f && o.position.y < y - 0.1f) s = Mathf.Max(s, o.position.y + halfH * 2f);
            }
            return s;
        }

        /// <summary>Gravità semplice: i peluche non tenuti cadono finché non trovano appoggio; nello scivolo spariscono.</summary>
        void Settle()
        {
            foreach (Transform t in toys)
            {
                if (t == held || gone.Contains(t)) continue;
                Vector3 p = t.position;
                if (p.x < chuteEdge) { gone.Add(t); StartCoroutine(IntoChute(t)); continue; }
                float to = Support(t, p.x, p.y);
                if (p.y > to + 0.001f)
                {
                    fallSpeed[t] += 14f * Time.deltaTime;
                    p.y = Mathf.Max(to, p.y - fallSpeed[t] * Time.deltaTime);
                    t.position = p;
                    if (p.y <= to + 0.001f)
                    {
                        if (fallSpeed[t] > 2f) { Haptics.Medium(); Sfx.Play(this, "bonk", 0.6f); }   // tonfo
                        fallSpeed[t] = 0f;
                    }
                }
                else fallSpeed[t] = 0f;
                t.localRotation = Quaternion.Lerp(t.localRotation, Quaternion.identity, 8f * Time.deltaTime);
            }
        }

        IEnumerator DropSeq()
        {
            busy = true;
            coins--;
            Coins();
            Sfx.Play(this, "clunk", 0.6f);
            float x = claw.position.x;
            // scende fino al peluche più alto sotto l'artiglio (o fino al pavimento)
            Transform top = null;
            foreach (Transform t in toys)
            {
                if (gone.Contains(t) || Mathf.Abs(t.position.x - x) > halfW + 0.1f) continue;
                if (top == null || t.position.y > top.position.y) top = t;
            }
            float stopY = top != null ? top.position.y + 0.1f : floorTop + 0.3f;
            yield return MoveClaw(new Vector3(x, stopY, 0f), 2.2f);
            yield return new WaitForSeconds(0.15f);
            clawArt.sprite = closedSprite;
            Haptics.Light();
            Sfx.Play(this, "tick", 0.8f);
            yield return new WaitForSeconds(0.3f);
            float offset = top != null ? Mathf.Abs(top.position.x - x) : 99f;
            if (top != null && offset < grabTolerance)
            {
                held = top;
                Vector3 grip = held.position - claw.position;
                float slipChance = Mathf.InverseLerp(0.08f, grabTolerance, offset) * 0.7f;
                bool slip = Random.value < slipChance;
                float slipAt = Random.Range(0.3f, 0.9f);
                // risale e va allo scivolo; il peluche dondola (di più se preso male)
                Vector3 a = claw.position, b = new Vector3(x, restY, 0f), c = new Vector3(chuteX, restY, 0f);
                float upTime = (restY - a.y) / 1.6f, sideTime = Mathf.Abs(x - chuteX) / 1.4f, total = upTime + sideTime;
                for (float t = 0f; t < total; t += Time.deltaTime)
                {
                    claw.position = t < upTime ? Vector3.Lerp(a, b, t / upTime) : Vector3.Lerp(b, c, (t - upTime) / Mathf.Max(0.01f, sideTime));
                    if (held != null)
                    {
                        held.position = claw.position + grip;
                        held.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(t * 12f) * (6f + offset * 40f));
                        if (slip && t / total > slipAt)
                        {
                            Sfx.Play(this, "whoosh", 0.5f);
                            Say(held == keyToy ? "NOOO! It slipped!" : "Whoops, it slipped.", 1.6f);
                            fallSpeed[held] = 0f;
                            held = null;
                        }
                    }
                    yield return null;
                }
                claw.position = c;
                yield return new WaitForSeconds(0.2f);
                clawArt.sprite = openSprite;
                Sfx.Play(this, "tick", 0.6f);
                if (held != null)
                {
                    Transform t = held;
                    held = null;
                    fallSpeed[t] = 0f;
                    gone.Add(t);
                    yield return IntoChute(t);
                }
                else yield return new WaitForSeconds(0.8f);
            }
            else
            {
                Sfx.Play(this, "buzz", 0.4f);
                Say(top == null ? "Nothing but air." : "Missed! Aim for the middle.", 1.6f);
                yield return MoveClaw(new Vector3(x, restY, 0f), 1.6f);
                clawArt.sprite = openSprite;
            }
            if (done) yield break;
            if (coins <= 0)
            {
                done = true;
                Say("Out of coins... and no key.", 0f);
                yield return new WaitForSeconds(1.8f);
                Fail();
                yield break;
            }
            busy = false;
        }

        IEnumerator MoveClaw(Vector3 to, float speed)
        {
            while ((claw.position - to).sqrMagnitude > 0.0001f)
            {
                claw.position = Vector3.MoveTowards(claw.position, to, speed * Time.deltaTime);
                yield return null;
            }
        }

        /// <summary>Il peluche cade nello scivolo e sparisce; se è l'orsetto con la chiave, la chiave esce dallo sportello.</summary>
        IEnumerator IntoChute(Transform t)
        {
            Vector3 p = t.position;
            Vector3 s = t.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.4f)
            {
                t.position = new Vector3(Mathf.Lerp(p.x, chuteX, k), p.y - k * k * (p.y + 1.9f), 0f);
                if (t.position.y < -1.3f) t.localScale = s * Mathf.Max(0f, 1f - (-1.3f - t.position.y) * 2f);
                yield return null;
            }
            t.gameObject.SetActive(false);
            Haptics.Medium();
            Sfx.Play(this, "bonk", 0.8f);
            if (t != keyToy)
            {
                Say(Random.value < 0.5f ? "A prize! A useless prize." : "Cute. But I said the KEY.", 1.8f);
                yield break;
            }
            done = true;
            busy = true;
            keyPrize.gameObject.SetActive(true);
            Vector3 ks = keyPrize.localScale, kp = keyPrize.localPosition;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.5f)
            {
                keyPrize.localScale = ks * Mathf.Lerp(0.2f, 1f, k) * (1f + Mathf.Sin(k * Mathf.PI) * 0.3f);
                keyPrize.localPosition = kp + new Vector3(0f, Mathf.Sin(k * Mathf.PI) * 0.5f + k * 0.2f, 0f);
                yield return null;
            }
            Sfx.Play(this, "clunk", 0.9f);
            yield return new WaitForSeconds(0.15f);
            Sfx.Play(this, "ding", 0.9f);
            foreach (SmokeEmitter c in confetti) if (c != null) c.Play();
            Say("The KEY! Jackpot!", 0f);
            yield return new WaitForSeconds(1.2f);
            foreach (SmokeEmitter c in confetti) if (c != null) c.Stop();
            yield return new WaitForSeconds(1.0f);
            Win();
        }

        int said;

        void Say(string text, float seconds)
        {
            if (bubble == null) return;
            bubble.Show(text);
            int id = ++said;
            if (seconds > 0f) StartCoroutine(HideLater(seconds, id));
        }

        IEnumerator HideLater(float seconds, int id)
        {
            yield return new WaitForSeconds(seconds);
            if (bubble != null && !done && id == said) bubble.Hide();
        }
    }
}
