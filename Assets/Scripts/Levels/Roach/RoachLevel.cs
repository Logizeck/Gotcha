using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 9 — lo scarafaggio cattivo. Cammina avanti e indietro sulla parete della cucina. Tenendo il dito sullo
    /// schermo si impugna la bomboletta bianca col teschio: lo spray esce verso l'alto, sopra il dito. Lo scarafaggio
    /// va tenuto nella nuvola tossica per un po' (scappa più veloce quando lo colpisci). La bomboletta però si svuota:
    /// se finisce prima, lo scarafaggio se la ride e sparisce in una crepa. (Editor: tieni premuto il mouse.)
    /// </summary>
    public class RoachLevel : LevelController
    {
        [SerializeField] Transform roach;
        [SerializeField] GameObject legsA, legsB;
        [SerializeField] GameObject alive, dead;
        [SerializeField] SpriteRenderer body;
        [SerializeField] Transform can;
        [SerializeField] Transform nozzle;
        [SerializeField] SmokeEmitter mist;
        [SerializeField] Transform gauge;            // barra del contenuto (scala x 0..1)
        [SerializeField] Transform crack;            // dove scappa se la bomboletta finisce
        [SerializeField] SpeechBubble bubble;
        [SerializeField] AudioSource hiss;
        [SerializeField] SmokeEmitter cough;          // sbuffi verdi quando è nella nuvola
        [SerializeField] SmokeEmitter dust;           // polverina dietro quando scappa
        [SerializeField] SmokeEmitter soul;           // l'animella che sale (finale comico)
        [SerializeField] float speed = 1.4f;
        [SerializeField] float panicSpeed = 3.0f;
        [SerializeField] float poisonNeeded = 1.3f;   // secondi nella nuvola
        [SerializeField] float sprayCapacity = 7f;   // secondi di spray
        [SerializeField] float reach = 1.6f, width = 0.5f;
        [SerializeField] Vector2 areaMin = new Vector2(-2.3f, -2.4f), areaMax = new Vector2(2.3f, 2.8f);

        Camera cam;
        Vector2 target;
        float poison, spray, panic, legClock, retarget, sniff, hitShake;
        bool over;
        Vector3 canHome;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            spray = sprayCapacity;
            canHome = can.position;
            if (dead != null) dead.SetActive(false);
            if (mist != null) mist.Stop();
            foreach (SmokeEmitter e in new[] { cough, dust, soul }) if (e != null) e.Stop();
            if (hiss != null) { hiss.clip = HissClip(); hiss.loop = true; hiss.volume = 0f; hiss.Play(); }
            PickTarget();
            if (bubble != null) { bubble.Show("Heh heh heh..."); StartCoroutine(HideLater(1.6f)); }
        }

        void OnDestroy() { if (hiss != null && hiss.clip != null) Destroy(hiss.clip); }

        void PickTarget()
        {
            target = new Vector2(Random.Range(areaMin.x, areaMax.x), Random.Range(areaMin.y, areaMax.y));
            retarget = Random.Range(1.2f, 2.5f);
        }

        void Update()
        {
            if (over || IsFinished) return;
            bool spraying = false;
            if (PointerInput.TryGetPrimary(out Vector2 sp, out _, out _) && !PointerInput.IsOverUi() && spray > 0f)
            {
                Vector2 w = PointerInput.ToWorld(cam, sp);
                // l'ugello sta appena sopra il dito: lo spray va verso l'alto, il dito non copre il bersaglio
                Vector3 offset = can.position - nozzle.position;
                can.position = Vector3.Lerp(can.position, new Vector3(w.x, w.y + 0.25f, 0f) + offset, 1f - Mathf.Exp(-25f * Time.deltaTime));
                spraying = true;
            }
            else can.position = Vector3.Lerp(can.position, canHome, 1f - Mathf.Exp(-8f * Time.deltaTime));
            // la bomboletta vibra mentre spruzza
            can.localRotation = Quaternion.Euler(0, 0, spraying ? Mathf.Sin(Time.time * 70f) * 2.5f : 0f);

            if (spraying) spray -= Time.deltaTime;
            if (gauge != null) gauge.localScale = new Vector3(Mathf.Clamp01(spray / sprayCapacity), 1f, 1f);
            if (mist != null) { if (spraying && !mist.IsPlaying) mist.Play(); else if (!spraying && mist.IsPlaying) mist.Stop(); }
            if (hiss != null) hiss.volume = Mathf.MoveTowards(hiss.volume, spraying ? 0.35f : 0f, Time.deltaTime * 4f);

            // nuvola tossica: una capsula sopra l'ugello
            Vector2 n = nozzle.position, r = roach.position;
            float along = r.y - n.y;
            bool hit = spraying && along > -0.1f && along < reach && Mathf.Abs(r.x - n.x) < width + along * 0.25f;
            if (cough != null) { if (hit && !cough.IsPlaying) cough.Play(); else if (!hit && cough.IsPlaying) cough.Stop(); }
            hitShake = hit ? 0.25f : Mathf.Max(0f, hitShake - Time.deltaTime);
            if (hit)
            {
                poison += Time.deltaTime;
                panic = 0.8f;
                sniff = 0f;
                if (body != null) body.color = Color.Lerp(Color.white, new Color(0.6f, 1f, 0.5f), poison / poisonNeeded);
                if (poison >= poisonNeeded) { StartCoroutine(Die()); return; }
            }
            Walk();
            if (spray <= 0f && !spraying) StartCoroutine(Escape());
        }

        void Walk()
        {
            panic -= Time.deltaTime;
            retarget -= Time.deltaTime;
            Vector2 pos = roach.position;
            // ogni tanto si ferma ad annusare l'aria (si gira di qua e di là), se non è in panico
            if (sniff > 0f)
            {
                sniff -= Time.deltaTime;
                roach.rotation *= Quaternion.Euler(0, 0, Mathf.Sin(Time.time * 9f) * 90f * Time.deltaTime);
                Animate(0f);
                return;
            }
            if (retarget <= 0f || Vector2.Distance(pos, target) < 0.2f)
            {
                PickTarget();
                if (panic <= 0f && Random.value < 0.35f) sniff = Random.Range(0.4f, 0.9f);
            }
            float v = panic > 0f ? panicSpeed : speed;
            Vector2 dir = (target - pos).normalized;
            Vector2 next = Vector2.MoveTowards(pos, target, v * Time.deltaTime);
            roach.position = new Vector3(next.x, next.y, roach.position.z);
            // la testa guarda dove va (lo sprite ha la testa verso +y)
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
            roach.rotation = Quaternion.Slerp(roach.rotation, Quaternion.Euler(0, 0, angle), 1f - Mathf.Exp(-10f * Time.deltaTime));
            Animate(v);
        }

        /// <summary>Zampe alternate, dondolio del corpo e piccolo schiacciamento a ogni passo; tremolio quando è avvelenato.</summary>
        void Animate(float v)
        {
            legClock += Time.deltaTime * Mathf.Max(v, 0.4f) * 6f;
            bool a = Mathf.Repeat(legClock, 1f) < 0.5f;
            if (legsA != null) legsA.SetActive(a);
            if (legsB != null) legsB.SetActive(!a);
            if (alive == null) return;
            Transform at = alive.transform;   // corpo + zampe: si dondola e si schiaccia correndo
            float step = Mathf.Sin(legClock * Mathf.PI * 2f);
            float run = Mathf.Clamp01(v / panicSpeed);
            at.localRotation = Quaternion.Euler(0, 0, step * (3f + run * 5f));
            at.localScale = new Vector3(1f + step * 0.03f * run, 1f - step * 0.03f * run, 1f);
            at.localPosition = hitShake > 0f ? (Vector3)(Random.insideUnitCircle * 0.03f) : Vector3.zero;
            if (dust != null) { if (panic > 0f && !dust.IsPlaying) dust.Play(); else if (panic <= 0f && dust.IsPlaying) dust.Stop(); }
        }

        IEnumerator Die()
        {
            over = true;
            if (mist != null) mist.Stop();
            if (hiss != null) hiss.Stop();
            if (cough != null) cough.Stop();
            if (dust != null) dust.Stop();
            if (bubble != null) bubble.Show("Ugh... *cough* *cough*", true);
            // barcolla, rallenta, gira su se stesso...
            for (float k = 0f; k < 0.9f; k += Time.deltaTime)
            {
                roach.rotation *= Quaternion.Euler(0, 0, (360f - k * 300f) * Time.deltaTime);
                legClock += Time.deltaTime * (6f - k * 6f);
                bool a = Mathf.Repeat(legClock, 1f) < 0.5f;
                if (legsA != null) legsA.SetActive(a);
                if (legsB != null) legsB.SetActive(!a);
                if (alive != null) alive.transform.localPosition = (Vector3)(Random.insideUnitCircle * 0.03f);
                yield return null;
            }
            // ...e si ribalta a zampe all'aria (schiacciamento orizzontale come una moneta che gira)
            Vector3 s0 = roach.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.18f) { roach.localScale = new Vector3(s0.x * (1f - k), s0.y, s0.z); yield return null; }
            if (alive != null) alive.SetActive(false);
            if (dead != null) dead.SetActive(true);
            Haptics.Light();
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.18f) { roach.localScale = new Vector3(s0.x * k, s0.y, s0.z); yield return null; }
            roach.localScale = s0;
            // zampette che scalciano sempre più piano, poi l'animella vola via
            for (float k = 0f; k < 1.2f; k += Time.deltaTime)
            {
                roach.localRotation = Quaternion.Euler(0, 0, roach.localEulerAngles.z + Mathf.Sin(k * 30f) * 6f * (1.2f - k) * Time.deltaTime * 10f);
                if (dead != null) dead.transform.localScale = new Vector3(1f + Mathf.Sin(k * 40f) * 0.04f * (1.2f - k), 1f, 1f);
                yield return null;
            }
            if (soul != null) { soul.transform.position = roach.position; soul.Play(); }
            if (bubble != null) bubble.Show("...I'll be back.", true);
            yield return new WaitForSeconds(0.8f);
            if (soul != null) soul.Stop();
            yield return new WaitForSeconds(0.5f);
            Win();
        }

        IEnumerator Escape()
        {
            over = true;
            if (mist != null) mist.Stop();
            if (hiss != null) hiss.Stop();
            if (bubble != null) bubble.Show("HAHA! Empty can!", true);
            Vector3 from = roach.position, to = crack != null ? crack.position : from + Vector3.up * 5f;
            float ang = Mathf.Atan2(to.y - from.y, to.x - from.x) * Mathf.Rad2Deg - 90f;
            roach.rotation = Quaternion.Euler(0, 0, ang);
            if (dust != null) dust.Play();
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.9f)
            {
                if (alive != null) alive.transform.localScale = new Vector3(0.9f, 1.15f, 1f);   // allungato dalla fretta
                roach.position = Vector3.Lerp(from, to, k);
                legClock += Time.deltaTime * 30f;
                bool a = Mathf.Repeat(legClock, 1f) < 0.5f;
                if (legsA != null) legsA.SetActive(a);
                if (legsB != null) legsB.SetActive(!a);
                yield return null;
            }
            roach.gameObject.SetActive(false);
            yield return new WaitForSeconds(0.8f);
            Fail();
        }

        IEnumerator HideLater(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (bubble != null && !over) bubble.Hide();
        }

        /// <summary>Soffio dello spray: rumore bianco filtrato.</summary>
        static AudioClip HissClip()
        {
            const int rate = 22050;
            int n = rate;
            var data = new float[n];
            var rnd = new System.Random(3);
            float lp = 0f;
            for (int i = 0; i < n; i++)
            {
                float white = (float)rnd.NextDouble() * 2f - 1f;
                lp += (white - lp) * 0.55f;
                data[i] = (white - lp) * 0.5f;   // passa-alto: "psssss"
            }
            AudioClip clip = AudioClip.Create("hiss", n, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
