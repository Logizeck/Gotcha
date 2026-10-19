using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;
using Gotcha.Sensors;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "Gravity resigned" — un omino sta a testa in giù attaccato al soffitto, "GET HIM DOWN". Soluzione:
    /// capovolgere il telefono e tenerlo così 2 secondi: per lui il "giù" cambia e cade sul materasso. Inclinando di lato
    /// scivola lungo il soffitto: se arriva al muro ci sbatte (tre volte = sconfitta). EASTER EGG "Gravity Resigned":
    /// prima di finire, ruotare 0°→180°→0°→180°→0° entro 12 s con una pausa riconoscibile in ogni posizione (non
    /// scuotendo): la gravità in persona, un impiegatino col nastro, timbra il cartellino, lancia la lettera di
    /// dimissioni contro lo schermo e se ne va con la valigetta; l'omino resta a galleggiare: "WELL, THAT'S AWKWARD".
    /// (Editor: freccia su = capovolto, frecce destra/sinistra = di lato. Senza accelerometro: pulsante FLIP.)
    /// </summary>
    public class GravityResignedLevel : LevelController
    {
        enum Pose { None, Up, Down }

        struct Stop { public Pose pose; public float settledAt, leftAt; }

        [SerializeField] Transform man;                  // disegnato dritto: appeso al soffitto è ruotato di 180°
        [SerializeField] float ceilingY = 2.05f, floorY = -2.75f, wallX = 2.05f;
        [SerializeField] Transform holdRing;             // barretta del "tieni capovolto"
        [SerializeField] SpriteRenderer[] hearts;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] GameObject flipButton;          // ripiego senza accelerometro
        [SerializeField] Collider2D flipArea;
        [SerializeField] TextMesh goal;
        // Easter egg
        [SerializeField] Transform worker, timeClock, letter, sign;
        [SerializeField] Transform cushion;

        Camera cam;
        float manX, velX, downFor, poseFor, clock;
        int falls;
        bool busy;
        Pose rawPose = Pose.None, settledPose = Pose.Up;
        readonly List<Stop> stops = new List<Stop>();

        public override void OnLevelStart()
        {
            cam = Camera.main;
            if (bubble != null) bubble.Hide();
            Kit41B.Hearts(hearts, 3);
            bool fallback = !SystemInfo.supportsAccelerometer;
#if UNITY_EDITOR
            fallback = false;
#endif
            if (flipButton != null) flipButton.SetActive(fallback);
            foreach (Transform t in new[] { worker, timeClock, letter, sign }) if (t != null) t.gameObject.SetActive(false);
            stops.Add(new Stop { pose = Pose.Up, settledAt = 0f, leftAt = -1f });
            PlaceMan();
            if (holdRing != null) holdRing.localScale = new Vector3(0f, 1f, 1f);
        }

        void PlaceMan()
        {
            man.localPosition = new Vector3(manX, ceilingY, man.localPosition.z);
            man.localRotation = Quaternion.Euler(0f, 0f, 180f + Mathf.Sin(clock * 2f) * 4f - velX * 6f);
        }

        Vector2 Gravity()
        {
            Vector2 g = Inputs.Tilt();
            if (flipButton != null && flipButton.activeSelf && PointerInput.IsHeld(cam, flipArea)) g = new Vector2(0f, 1f);
            return g;
        }

        void Update()
        {
            if (IsFinished || busy) return;
            float dt = Time.deltaTime;
            clock += dt;
            Vector2 g = Gravity();

            // posa del telefono: dritto, capovolto o in mezzo
            Pose p = Pose.None;
            if (g.magnitude > 0.5f)
            {
                float a = Mathf.Abs(Mathf.Atan2(g.x, -g.y) * Mathf.Rad2Deg);
                p = a < 40f ? Pose.Up : a > 140f ? Pose.Down : Pose.None;
            }
            TrackEgg(p, dt);
            if (busy) return;

            // capovolto: dopo 2 secondi cade
            if (g.y > 0.7f)
            {
                downFor += dt;
                if (holdRing != null) holdRing.localScale = new Vector3(Mathf.Clamp01(downFor / 2f), 1f, 1f);
                if (downFor >= 2f) { StartCoroutine(FallDown()); return; }
            }
            else
            {
                downFor = 0f;
                if (holdRing != null) holdRing.localScale = new Vector3(0f, 1f, 1f);
            }

            // di lato: scivola sul soffitto (zona morta, così le rotazioni veloci non contano)
            float gx = Mathf.Abs(g.x) > 0.35f ? g.x : 0f;
            velX += gx * 3.2f * dt;
            velX *= 1f - Mathf.Min(1f, dt * 1.6f);
            if (gx == 0f) velX += -manX * 0.9f * dt;   // il soffitto è un po' concavo: torna piano al centro
            manX += velX * dt;
            if (Mathf.Abs(manX) >= wallX) { StartCoroutine(WallBonk()); return; }
            PlaceMan();
        }

        void TrackEgg(Pose p, float dt)
        {
            bool shaking = SensorHub.Instance != null && SensorHub.Instance.IsShaking;
            if (p != rawPose) { rawPose = p; poseFor = 0f; }
            else poseFor += dt;
            if (shaking) poseFor = 0f;
            if (p == Pose.None || poseFor < 0.35f || p == settledPose) return;
            // nuova posa ferma: chiude la sosta precedente
            Stop last = stops[stops.Count - 1];
            last.leftAt = clock - poseFor;
            stops[stops.Count - 1] = last;
            settledPose = p;
            stops.Add(new Stop { pose = p, settledAt = clock, leftAt = -1f });
            if (stops.Count > 5) stops.RemoveAt(0);
            if (stops.Count == 5 && stops[0].pose == Pose.Up && stops[0].leftAt >= 0f && stops[4].settledAt - stops[0].leftAt <= 12f)
            {
                bool alternate = true;
                for (int i = 0; i < 5; i++) if (stops[i].pose != (i % 2 == 0 ? Pose.Up : Pose.Down)) alternate = false;
                if (alternate) StartCoroutine(Resignation());
            }
        }

        IEnumerator WallBonk()
        {
            busy = true;
            falls++;
            Kit41B.Hearts(hearts, 3 - falls);
            float side = Mathf.Sign(manX);
            Sfx.Play(this, "bonk", 0.9f);
            Haptics.Medium();
            // sbatte e scivola giù lungo il muro
            Vector3 p0 = man.localPosition;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.5f)
            {
                man.localPosition = p0 + new Vector3(0f, -k * k * 1.2f, 0f);
                man.localRotation = Quaternion.Euler(0f, 0f, 180f - side * 90f * k);
                yield return null;
            }
            Kit41B.Say(bubble, falls >= 3 ? "Ouch. I give up." : "OUCH! The WALL!");
            yield return new WaitForSeconds(0.9f);
            if (falls >= 3) { Fail(); yield break; }
            manX = 0f;
            velX = 0f;
            Sfx.Play(this, "whoosh", 0.5f);
            if (bubble != null) bubble.Hide();
            PlaceMan();
            busy = false;
        }

        IEnumerator FallDown()
        {
            busy = true;
            if (holdRing != null) holdRing.localScale = new Vector3(0f, 1f, 1f);
            Kit41B.Say(bubble, "Whoaaa!");
            Sfx.Play(this, "whoosh", 0.8f);
            Vector3 p0 = man.localPosition;
            float x = Mathf.Clamp(p0.x, -1.3f, 1.3f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.7f)
            {
                man.localPosition = new Vector3(Mathf.Lerp(p0.x, x, k), Mathf.Lerp(p0.y, floorY, k * k), p0.z);
                man.localRotation = Quaternion.Euler(0f, 0f, 180f + 180f * k);
                yield return null;
            }
            man.localPosition = new Vector3(x, floorY, p0.z);
            man.localRotation = Quaternion.identity;
            Sfx.Play(this, "bonk", 0.8f);
            Haptics.Medium();
            if (cushion != null) StartCoroutine(Kit41B.Pop(cushion, -0.25f, 0.3f));
            yield return Kit41B.Pop(man, 0.2f, 0.3f);
            Kit41B.Say(bubble, "Feet on the ground. Finally!");
            yield return new WaitForSeconds(1.4f);
            Win();
        }

        // ---------- Easter egg: la gravità si licenzia ----------

        IEnumerator Resignation()
        {
            busy = true;
            if (holdRing != null) holdRing.localScale = new Vector3(0f, 1f, 1f);
            if (goal != null) goal.text = "...UH OH";
            Kit41B.Say(bubble, "Hey! Stop spinning me!");
            yield return new WaitForSeconds(0.8f);

            // entra l'impiegato "gravità"
            timeClock.gameObject.SetActive(true);
            StartCoroutine(Kit41B.Pop(timeClock, 0.3f, 0.3f));
            worker.gameObject.SetActive(true);
            Vector3 stand = worker.localPosition, from = stand + Vector3.right * 3.5f;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 1.1f)
            {
                worker.localPosition = Vector3.Lerp(from, stand, k) + Vector3.up * Mathf.Abs(Mathf.Sin(k * 18f)) * 0.06f;
                yield return null;
            }
            worker.localPosition = stand;
            if (bubble != null) bubble.transform.position = worker.position + new Vector3(-0.6f, 1.6f, 0f);
            Kit41B.Say(bubble, "Up, down, up, down... THAT'S IT. I QUIT!");
            yield return new WaitForSeconds(1.8f);
            // timbra il cartellino
            Sfx.Play(this, "clunk", 0.9f);
            Haptics.Light();
            yield return Kit41B.Shake(timeClock, 0.08f, 0.3f);
            Kit41B.Say(bubble, "*CLOCK OUT*");
            yield return new WaitForSeconds(1f);

            // la lettera di dimissioni contro lo schermo
            if (bubble != null) bubble.Hide();
            letter.gameObject.SetActive(true);
            Vector3 l0 = worker.localPosition + new Vector3(-0.3f, 0.6f, 0f), l1 = new Vector3(0f, 0.2f, 0f);
            Sfx.Play(this, "whoosh", 0.8f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.55f)
            {
                letter.localPosition = Vector3.Lerp(l0, l1, k) + Vector3.up * Mathf.Sin(k * Mathf.PI) * 0.8f;
                letter.localScale = Vector3.one * Mathf.Lerp(0.25f, 1.6f, k * k);
                letter.localRotation = Quaternion.Euler(0f, 0f, (1f - k) * 540f);
                yield return null;
            }
            letter.localRotation = Quaternion.Euler(0f, 0f, -4f);
            Sfx.Play(this, "bonk", 1f);
            Haptics.Heavy();
            yield return new WaitForSeconds(1.8f);
            Vector3 lp = letter.localPosition;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.8f)
            {
                letter.localPosition = lp + Vector3.down * k * k * 6f;
                letter.localRotation = Quaternion.Euler(0f, 0f, -4f - k * 25f);
                yield return null;
            }
            letter.gameObject.SetActive(false);

            // se ne va con la valigetta
            if (bubble != null) bubble.transform.position = worker.position + new Vector3(-0.6f, 1.6f, 0f);
            Kit41B.Say(bubble, "Find another force. Bye!");
            Vector3 w0 = worker.localPosition;
            SpriteRenderer wsr = worker.GetComponentInChildren<SpriteRenderer>();
            if (wsr != null) wsr.flipX = true;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 1.6f)
            {
                worker.localPosition = w0 + Vector3.left * k * 5f + Vector3.up * Mathf.Abs(Mathf.Sin(k * 26f)) * 0.06f;
                yield return null;
            }
            worker.gameObject.SetActive(false);
            if (bubble != null) bubble.Hide();

            // l'omino resta a galleggiare
            Vector3 m0 = man.localPosition;
            Vector3 mid = new Vector3(0f, 0.3f, m0.z);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 1.5f)
            {
                man.localPosition = Vector3.Lerp(m0, mid, k * k * (3f - 2f * k));
                man.localRotation = Quaternion.Euler(0f, 0f, 180f - 160f * k);
                yield return null;
            }
            sign.gameObject.SetActive(true);
            Sfx.Play(this, "pop", 0.8f);
            yield return Kit41B.Pop(sign, 0.3f, 0.35f);
            if (goal != null) goal.text = "GRAVITY RESIGNED";
            for (float t = 0f; t < 2.8f; t += Time.deltaTime)
            {
                man.localPosition = mid + new Vector3(Mathf.Sin(t * 1.3f) * 0.15f, Mathf.Sin(t * 2.1f) * 0.12f, 0f);
                man.localRotation = Quaternion.Euler(0f, 0f, 20f + Mathf.Sin(t * 1.7f) * 12f);
                yield return null;
            }
            WinWithEgg("gravity_resigned");
        }
    }
}
