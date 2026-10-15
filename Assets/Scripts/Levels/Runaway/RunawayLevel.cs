using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 25 — la sveglia. Suona a tutto volume e il suo pulsante OFF è saltato giù e scappa per la stanza:
    /// gironzola e, se il dito tocca vicino, schiva. Col dito si disegnano staccionate (massimo 4, la più vecchia
    /// sparisce) contro cui il pulsante sbatte: chiudilo in un angolo e premilo. Il pulsante torna sulla sveglia
    /// e finalmente silenzio.
    /// </summary>
    public class RunawayLevel : LevelController
    {
        [SerializeField] Transform button;
        [SerializeField] Transform visual;
        [SerializeField] Transform legs;
        [SerializeField] StrokeDrawer pen;
        [SerializeField] Transform clock;
        [SerializeField] Transform clockSocket;      // dove torna il pulsante
        [SerializeField] GameObject waves;
        [SerializeField] AudioSource alarm;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] float hitRadius = 0.5f;
        [SerializeField] float scareRadius = 1.3f;
        [SerializeField] float speed = 2.2f;
        [SerializeField] float bodyRadius = 0.4f;
        [SerializeField] Vector2 boundsMin = new Vector2(-2.35f, -3.9f);
        [SerializeField] Vector2 boundsMax = new Vector2(2.35f, 2.4f);

        Vector2 dir, target;
        float retarget, dash, squash, cornered, ringTimer;
        Camera cam;
        Vector3 visualScale, clockHome;
        bool done;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            visualScale = visual.localScale;
            if (clock != null) clockHome = clock.localPosition;
            target = button.position;
            dir = Vector2.left;
            if (alarm != null)
            {
                alarm.clip = AlarmClip();
                alarm.loop = true;
                alarm.volume = 0.3f;
                alarm.Play();
            }
            if (bubble != null) bubble.Show("RIIIIING!");
        }

        void OnDestroy() { if (alarm != null && alarm.clip != null) Destroy(alarm.clip); }

        void Update()
        {
            if (done || IsFinished) return;
            // la sveglia trema e strilla
            if (clock != null) clock.localPosition = clockHome + new Vector3(Mathf.Sin(Time.time * 70f) * 0.03f, Mathf.Abs(Mathf.Sin(Time.time * 35f)) * 0.04f, 0f);
            ringTimer -= Time.deltaTime;
            if (ringTimer <= 0f && bubble != null) { ringTimer = 2.4f; bubble.Show(Random.value < 0.5f ? "RIIIIING!" : "DRIIIIN!"); }

            Vector2 pos = button.position;
            if (PointerInput.AnyTouchBegan() && !PointerInput.IsOverUi() && PointerInput.TryGetPrimary(out Vector2 sp, out _, out _))
            {
                Vector2 p = PointerInput.ToWorld(cam, sp);
                float d = Vector2.Distance(p, pos + new Vector2(0f, 0.1f));
                if (d < hitRadius) { StartCoroutine(Caught()); return; }
                if (d < scareRadius) Flee(pos, p);
            }

            retarget -= Time.deltaTime;
            if (retarget <= 0f || Vector2.Distance(pos, target) < 0.1f)
            {
                retarget = Random.Range(0.7f, 1.4f);
                target = Clamp(pos + Random.insideUnitCircle.normalized * Random.Range(1f, 2.5f));
            }
            dash -= Time.deltaTime;
            float v = speed * (dash > 0f ? 3.5f : 1f) * (cornered > 0f ? 0.35f : 1f);
            Vector2 step = Vector2.ClampMagnitude(target - pos, v * Time.deltaTime);
            if (step.sqrMagnitude > 0f && Blocked(pos, step))
            {
                // sbatte contro una staccionata: cerca un'altra strada; se non ce n'è, è all'angolo
                if (!FindWay(pos)) { cornered = 1.2f; if (bubble != null && Random.value < 0.02f) bubble.Show("Eek!", true); }
                step = Vector2.zero;
            }
            cornered -= Time.deltaTime;
            Vector2 next = pos + step;
            button.position = new Vector3(next.x, next.y, button.position.z);

            float run = step.magnitude / Mathf.Max(Time.deltaTime, 0.0001f);
            if (legs != null) legs.localScale = new Vector3(step.x < 0f ? -1f : 1f, 1f + Mathf.Sin(Time.time * 30f) * 0.15f * Mathf.Clamp01(run), 1f);
            float shake = cornered > 0f ? Mathf.Sin(Time.time * 60f) * 0.04f : 0f;   // trema, intrappolato
            visual.localPosition = new Vector3(shake, 0f, 0f);
            squash -= Time.deltaTime;
            visual.localScale = Vector3.Lerp(visual.localScale, squash > 0f ? new Vector3(visualScale.x * 1.15f, visualScale.y * 0.7f, 1f) : visualScale, 20f * Time.deltaTime);
        }

        bool Blocked(Vector2 pos, Vector2 step)
        {
            if (pen == null) return false;
            Vector2 next = pos + step;
            return StrokeBarrier.Resolve(pen.transform, pos, ref next, bodyRadius, pen.colliderRadius, out _);
        }

        /// <summary>Prova qualche direzione libera; false se le staccionate lo chiudono da ogni parte.</summary>
        bool FindWay(Vector2 pos)
        {
            for (int i = 0; i < 10; i++)
            {
                Vector2 d = Random.insideUnitCircle.normalized;
                Vector2 t = Clamp(pos + d * Random.Range(0.8f, 2f));
                if ((t - pos).magnitude > 0.4f && !Blocked(pos, (t - pos) * 0.5f)) { target = t; retarget = 1f; return true; }
            }
            return false;
        }

        void Flee(Vector2 pos, Vector2 from)
        {
            Vector2 away = (pos - from).sqrMagnitude > 0.0001f ? (pos - from).normalized : Random.insideUnitCircle.normalized;
            for (int i = 0; i < 6; i++)
            {
                Vector2 d = Quaternion.Euler(0, 0, Random.Range(-60f, 60f)) * away;
                Vector2 t = Clamp(pos + d * 1.8f);
                if (!Blocked(pos, (t - pos) * 0.6f)) { target = t; dash = 0.3f; retarget = 0.8f; return; }
            }
            cornered = 1.2f;
            if (bubble != null) bubble.Show("Nowhere to run!", true);
        }

        IEnumerator Caught()
        {
            done = true;
            squash = 0.3f;
            visual.localScale = new Vector3(visualScale.x * 1.2f, visualScale.y * 0.6f, 1f);
            if (bubble != null) bubble.Show("OK OK, you got me!", true);
            yield return new WaitForSeconds(0.4f);
            // torna saltellando al suo posto sulla sveglia
            Vector3 from = button.position, to = clockSocket != null ? clockSocket.position : from;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.6f)
            {
                button.position = Vector3.Lerp(from, to, k) + Vector3.up * Mathf.Sin(k * Mathf.PI) * 1.2f;
                yield return null;
            }
            button.position = to;
            visual.localScale = new Vector3(visualScale.x * 1.2f, visualScale.y * 0.6f, 1f);   // CLICK
            if (alarm != null) alarm.Stop();
            if (waves != null) waves.SetActive(false);
            if (clock != null) clock.localPosition = clockHome;
            if (bubble != null) bubble.Show("...silence. Ahhh.", true);
            yield return new WaitForSeconds(1.4f);
            Win();
        }

        Vector2 Clamp(Vector2 p) { return new Vector2(Mathf.Clamp(p.x, boundsMin.x, boundsMax.x), Mathf.Clamp(p.y, boundsMin.y, boundsMax.y)); }

        /// <summary>Suoneria: quattro bip acuti e una pausa, in loop.</summary>
        static AudioClip AlarmClip()
        {
            const int rate = 22050;
            int n = rate;   // un secondo
            var data = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)rate;
                float slot = Mathf.Repeat(t, 1f);
                bool on = slot < 0.56f && Mathf.Repeat(slot, 0.14f) < 0.08f;
                float sq = Mathf.Repeat(t * 2050f, 1f) < 0.5f ? 1f : -1f;
                float bell = Mathf.Sin(t * Mathf.PI * 2f * 3100f) * 0.4f;
                data[i] = on ? (sq * 0.25f + bell * 0.2f) : 0f;
            }
            AudioClip clip = AudioClip.Create("alarm", n, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
