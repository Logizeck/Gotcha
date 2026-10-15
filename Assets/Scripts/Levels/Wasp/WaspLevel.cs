using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 9 — la vespa furiosa. Una vespa svolazza e ogni tanto si lancia contro lo schermo per pungere chi
    /// guarda (bagliore rosso e "OUCH!"). Nell'erba c'è una paletta: va raccolta con un tocco, poi ogni tocco la fa
    /// calare sul punto toccato. Servono 10 colpi; al decimo la vespa cade a terra a zampe all'aria, stecchita.
    /// Senza paletta i tocchi sulla vespa non le fanno niente (e lei se la ride).
    /// </summary>
    public class WaspLevel : LevelController
    {
        enum State { Flying, Attacking, Falling }

        [Header("Vespa")]
        [SerializeField] Transform wasp;
        [SerializeField] GameObject alive;      // vespa viva (corpo + ali)
        [SerializeField] GameObject dead;       // vespa stecchita
        [SerializeField] SpriteRenderer body;
        [SerializeField] AudioSource buzz;
        [SerializeField] int hitsNeeded = 10;
        [SerializeField] float hitRadius = 0.85f;     // area colpibile (più ampia della vespa)
        [SerializeField] float baseSpeed = 2.2f;
        [SerializeField] float speedPerHit = 0.3f;
        [SerializeField] Vector2 areaMin = new Vector2(-2.3f, -1.8f);
        [SerializeField] Vector2 areaMax = new Vector2(2.3f, 2.9f);
        [SerializeField] float groundY = -3.6f;

        [Header("Attacco allo schermo")]
        [SerializeField] SpriteRenderer stingFlash;   // bagliore rosso ai bordi
        [SerializeField] SpeechBubble bubble;
        [SerializeField] float attackEveryMin = 2.5f;
        [SerializeField] float attackEveryMax = 4.5f;
        [SerializeField] float attackScale = 3.2f;

        [Header("Paletta")]
        [SerializeField] Transform swatter;           // pivot all'impugnatura, rete a +swatterReach
        [SerializeField] float swatterReach = 1.9f;
        [SerializeField] float pickRadius = 1.0f;
        [SerializeField] Vector3 heldPosition = new Vector3(2.0f, -3.9f, 0f);

        Camera cam;
        State state;
        Vector2 target;
        int hits;
        float flash, stun, nextAttack;
        bool hasSwatter, swinging;
        Vector3 swatterGroundPos;
        Quaternion swatterGroundRot;

        public int Hits => hits;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            if (dead != null) dead.SetActive(false);
            if (alive != null) alive.SetActive(true);
            if (stingFlash != null) stingFlash.color = new Color(1f, 1f, 1f, 0f);
            if (bubble != null) bubble.Hide();
            if (swatter != null) { swatterGroundPos = swatter.localPosition; swatterGroundRot = swatter.localRotation; }
            state = State.Flying;
            nextAttack = Time.time + Random.Range(attackEveryMin, attackEveryMax);
            PickTarget();
            if (buzz != null)
            {
                buzz.clip = BuzzClip();
                buzz.loop = true;
                buzz.volume = 0.35f;
                buzz.Play();
            }
        }

        void OnDestroy()
        {
            if (buzz != null && buzz.clip != null) Destroy(buzz.clip);
        }

        void Update()
        {
            if (state == State.Falling || IsFinished) return;

            if (state == State.Flying)
            {
                if (stun > 0f) stun -= Time.deltaTime;
                else Fly();
                if (Time.time >= nextAttack) StartCoroutine(Attack());
            }

            if (flash > 0f) flash -= Time.deltaTime;
            if (body != null) body.color = flash > 0f ? new Color(1f, 0.45f, 0.45f) : Color.white;
            if (buzz != null) buzz.pitch = (1f + hits * 0.06f) * (state == State.Attacking ? 1.25f : 1f);

            if (PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out _) && began && !PointerInput.IsOverUi())
                OnTap(PointerInput.ToWorld(cam, sp));
        }

        // ---------- Volo e attacco ----------

        void Fly()
        {
            float speed = baseSpeed + hits * speedPerHit;
            Vector2 pos = wasp.localPosition;
            Vector2 next = Vector2.MoveTowards(pos, target, speed * Time.deltaTime);
            next.y += Mathf.Sin(Time.time * 9f) * 0.012f;
            wasp.localPosition = next;
            if (Vector2.Distance(next, target) < 0.1f) PickTarget();
            float dir = target.x - pos.x;
            if (Mathf.Abs(dir) > 0.01f) Face(dir < 0f ? 1f : -1f, 1f);
        }

        void Face(float flipX, float scale)
        {
            wasp.localScale = new Vector3(flipX * scale, scale, 1f);
        }

        /// <summary>La vespa si ferma, prende la mira e si lancia verso lo schermo (diventa enorme), punge e torna indietro.</summary>
        IEnumerator Attack()
        {
            state = State.Attacking;
            float flipX = Mathf.Sign(wasp.localScale.x);
            Vector3 start = wasp.localPosition;
            // mira: piccolo tremolio
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.45f)
            {
                if (state != State.Attacking) yield break;
                wasp.localPosition = start + (Vector3)(Random.insideUnitCircle * 0.05f);
                yield return null;
            }
            // affondo verso chi guarda
            Vector3 aim = new Vector3(Mathf.Clamp(start.x, -1f, 1f) * 0.5f, 0.2f, 0f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.35f)
            {
                if (state != State.Attacking) yield break;
                float e = k * k;
                wasp.localPosition = Vector3.Lerp(start, aim, e);
                Face(flipX, Mathf.Lerp(1f, attackScale, e));
                yield return null;
            }
            // puntura!
            if (stingFlash != null) StartCoroutine(Flash());
            if (bubble != null) { bubble.Show("OUCH!"); StartCoroutine(HideBubble(0.9f)); }
            Vector3 shakeFrom = transform.localPosition;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.3f)
            {
                transform.localPosition = shakeFrom + (Vector3)(Random.insideUnitCircle * 0.12f * (1f - k));
                yield return null;
            }
            transform.localPosition = shakeFrom;
            // torna a volare
            Vector3 back = wasp.localPosition;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.4f)
            {
                if (state != State.Attacking) yield break;
                wasp.localPosition = Vector3.Lerp(back, start, k);
                Face(flipX, Mathf.Lerp(attackScale, 1f, k));
                yield return null;
            }
            Face(flipX, 1f);
            if (state == State.Attacking) state = State.Flying;
            nextAttack = Time.time + Random.Range(attackEveryMin, attackEveryMax);
        }

        IEnumerator Flash()
        {
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.5f)
            {
                stingFlash.color = new Color(1f, 1f, 1f, 1f - k);
                yield return null;
            }
            stingFlash.color = new Color(1f, 1f, 1f, 0f);
        }

        IEnumerator HideBubble(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (bubble != null) bubble.Hide();
        }

        void PickTarget()
        {
            target = new Vector2(Random.Range(areaMin.x, areaMax.x), Random.Range(areaMin.y, areaMax.y));
        }

        // ---------- Paletta e colpi ----------

        void OnTap(Vector2 world)
        {
            if (!hasSwatter)
            {
                if (swatter != null && Vector2.Distance(world, SwatterHead()) <= pickRadius) StartCoroutine(PickUp());
                else if (Vector2.Distance(world, wasp.position) <= hitRadius * Mathf.Abs(wasp.localScale.y) && bubble != null)
                {
                    bubble.Show("Ha-ha!");   // a mani nude non le fai niente
                    StartCoroutine(HideBubble(0.8f));
                }
                return;
            }
            if (swinging) return;
            bool hit = Vector2.Distance(world, wasp.position) <= hitRadius * Mathf.Abs(wasp.localScale.y);
            StartCoroutine(Swing(world, hit));
        }

        Vector2 SwatterHead()
        {
            return swatter.position + swatter.up * swatterReach;
        }

        IEnumerator PickUp()
        {
            hasSwatter = true;
            Vector3 from = swatter.localPosition;
            Quaternion rFrom = swatter.localRotation, rTo = Quaternion.Euler(0, 0, 25f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.3f)
            {
                swatter.localPosition = Vector3.Lerp(from, heldPosition, k);
                swatter.localRotation = Quaternion.Slerp(rFrom, rTo, k);
                yield return null;
            }
            swatter.localPosition = heldPosition;
            swatter.localRotation = rTo;
        }

        /// <summary>La paletta parte alzata e cala di colpo con la rete sul punto toccato, poi torna in mano.</summary>
        IEnumerator Swing(Vector2 point, bool hit)
        {
            swinging = true;
            Vector3 grip = new Vector3(point.x + 0.5f, point.y - swatterReach * 0.95f, 0f);
            Vector3 local = transform.InverseTransformPoint(grip);
            swatter.localPosition = new Vector3(local.x, local.y, 0f);
            Quaternion raised = Quaternion.Euler(0, 0, -55f), down = Quaternion.Euler(0, 0, 15f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.08f)
            {
                swatter.localRotation = Quaternion.Slerp(raised, down, k * k);
                yield return null;
            }
            swatter.localRotation = down;
            if (hit && state != State.Falling) Hit();
            Vector3 squash = swatter.localScale;
            swatter.localScale = new Vector3(squash.x * 1.08f, squash.y * 0.92f, squash.z);   // "SPLAT"
            yield return new WaitForSeconds(0.09f);
            swatter.localScale = squash;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.15f)
            {
                swatter.localPosition = Vector3.Lerp(swatter.localPosition, heldPosition, k);
                swatter.localRotation = Quaternion.Slerp(down, Quaternion.Euler(0, 0, 25f), k);
                yield return null;
            }
            swatter.localPosition = heldPosition;
            swinging = false;
        }

        void Hit()
        {
            hits++;
            flash = 0.15f;
            stun = 0.15f;
            if (state == State.Attacking) { state = State.Flying; Face(Mathf.Sign(wasp.localScale.x), 1f); nextAttack = Time.time + attackEveryMax; }
            wasp.localPosition += (Vector3)(Random.insideUnitCircle.normalized * 0.35f);   // sbanda
            PickTarget();
            if (hits >= hitsNeeded) StartCoroutine(Fall());
        }

        IEnumerator Fall()
        {
            state = State.Falling;
            if (buzz != null) buzz.Stop();
            if (alive != null) alive.SetActive(false);
            if (dead != null) dead.SetActive(true);
            wasp.localScale = Vector3.one;
            float v = 0f;
            float spin = 0f;
            while (wasp.localPosition.y > groundY)
            {
                v += 14f * Time.deltaTime;
                spin += 720f * Time.deltaTime;
                wasp.localPosition += Vector3.down * (v * Time.deltaTime);
                wasp.localRotation = Quaternion.Euler(0, 0, spin);
                yield return null;
            }
            wasp.localPosition = new Vector3(wasp.localPosition.x, groundY, 0f);
            wasp.localRotation = Quaternion.identity;   // a pancia in su, zampe all'aria
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.25f)
            {
                wasp.localPosition = new Vector3(wasp.localPosition.x, groundY + Mathf.Sin(k * Mathf.PI) * 0.15f, 0f);
                yield return null;
            }
            yield return new WaitForSeconds(1.0f);
            Win();
        }

        /// <summary>Ronzio: onda a dente di sega modulata, generata da codice.</summary>
        public static AudioClip BuzzClip()
        {
            const int rate = 22050;
            int n = rate;   // un secondo in loop
            var data = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)rate;
                float f = 210f + Mathf.Sin(t * Mathf.PI * 2f * 6f) * 14f;
                float saw = 2f * Mathf.Repeat(t * f, 1f) - 1f;
                data[i] = saw * 0.35f * (0.75f + 0.25f * Mathf.Sin(t * Mathf.PI * 2f * 30f));
            }
            AudioClip clip = AudioClip.Create("buzz", n, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
