using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 39 — la pista per la biglia. Disegna col dito fino a 3 rampe, poi premi GO: la biglia parte dalla
    /// mensola in alto e deve finire nella tazza, oltre il muretto. Si ha UN SOLO tiro: se la biglia manca la tazza
    /// (cade fuori, si ferma o esce dai lati) il livello è perso. Più a lungo si tiene premuto GO, più forte è la spinta
    /// (la barra accanto al pulsante si riempie in 1.2 secondi). Mentre la biglia corre non si disegna più.
    /// Variante 55 (specchiata): easter egg "Shattered Screen" — una rampa che spara la biglia dritta in alto a piena
    /// potenza la fa schiantare contro il vetro del telefono, che va in frantumi.
    /// </summary>
    public class MarbleRunLevel : LevelController
    {
        [SerializeField] Rigidbody2D marble;
        [SerializeField] Collider2D goButton;
        [SerializeField] Transform goVisual;
        [SerializeField] Transform powerFill;        // barra della spinta (scala x da 0 a 1)
        [SerializeField] float minPush = 1.5f;       // velocità di partenza con un tocco
        [SerializeField] float maxPush = 6.5f;       // velocità con la barra piena
        [SerializeField] float chargeSeconds = 1.2f;
        [SerializeField] float direction = 1f;       // 1 = verso destra, -1 = verso sinistra (variante specchiata)
        [SerializeField] Collider2D cupZone;
        [SerializeField] float lostBelow = -3.0f;   // sotto il piano di lavoro, fuori dalla tazza = mancata
        [SerializeField] SmokeEmitter confetti;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] StrokeDrawer pen;
        [SerializeField] ScreenShatter shatter;      // solo nella variante con l'easter egg
        [SerializeField] float eggMinCharge = 0.9f;  // "a piena potenza"
        [SerializeField] float eggUpSpeed = 5f;      // velocità verso l'alto oltre cui la biglia "vola verso lo schermo"

        Vector3 start;
        bool rolling, done, launched;
        float inCup, rollTime, charge, still, launchCharge;
        Vector2 last;
        bool charging;
        Camera cam;
        Vector3 goScale;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            start = marble.transform.position;
            if (goVisual != null) goScale = goVisual.localScale;
            ShowPower(0f);
            Hold();
            if (confetti != null) confetti.Stop();
            if (bubble != null) { bubble.Show("Draw a path, then GO! One shot only!"); StartCoroutine(HideLater(2.4f)); }
        }

        void Hold()
        {
            marble.bodyType = RigidbodyType2D.Kinematic;
            marble.transform.position = start;
            marble.position = start;
            marble.angularVelocity = 0f;
#if UNITY_6000_0_OR_NEWER
            marble.linearVelocity = Vector2.zero;
#else
            marble.velocity = Vector2.zero;
#endif
            rolling = false;
        }

        void Launch(float power)
        {
            Hold();
            launched = true;
            launchCharge = power;
            if (pen != null) pen.enabled = false;   // un solo tiro: niente ritocchi in corsa
            marble.bodyType = RigidbodyType2D.Dynamic;
            Vector2 v = new Vector2(Mathf.Lerp(minPush, maxPush, power) * direction, 0f);
#if UNITY_6000_0_OR_NEWER
            marble.linearVelocity = v;
#else
            marble.velocity = v;
#endif
            marble.WakeUp();
            rolling = true;
            still = 0f;
            last = marble.position;
            rollTime = 0f;
            inCup = 0f;
        }

        void ShowPower(float k)
        {
            if (powerFill != null) powerFill.localScale = new Vector3(k, 1f, 1f);
        }

        /// <summary>GO: si carica tenendo premuto, parte al rilascio.</summary>
        void ChargeGo()
        {
            if (launched) return;
            if (!PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out bool ended))
            {
                if (charging) Release();
                return;
            }
            if (began && goButton != null && goButton.OverlapPoint(PointerInput.ToWorld(cam, sp))) { charging = true; charge = 0f; }
            if (!charging) return;
            charge = Mathf.Min(1f, charge + Time.deltaTime / chargeSeconds);
            ShowPower(charge);
            if (goVisual != null) goVisual.localScale = new Vector3(goScale.x * 1.08f, goScale.y * (0.85f - charge * 0.1f), goScale.z);
            if (ended) Release();
        }

        void Release()
        {
            charging = false;
            if (goVisual != null) goVisual.localScale = goScale;
            Launch(charge);
        }

        void Update()
        {
            if (done || IsFinished) return;
            ChargeGo();
            if (!rolling) return;
            rollTime += Time.deltaTime;
            Vector2 p = marble.position;
            bool cupped = cupZone.OverlapPoint(p);
            if (cupped && inCup <= 0f) Haptics.Light();   // tac nella tazza
            inCup = cupped ? inCup + Time.deltaTime : 0f;
            if (inCup > 0.5f) { StartCoroutine(Score()); return; }
            Vector2 v = Velocity();
            if (shatter != null && launchCharge >= eggMinCharge && !cupped
                && ((v.y > eggUpSpeed && Mathf.Abs(v.x) < v.y * 0.5f) || (p.y > 4.3f && v.y > 0.5f)))
            {
                StartCoroutine(Shatter());
                return;
            }
            // ferma fuori dalla tazza (spinta troppo debole o incastrata su una rampa): un solo tiro, persa
            still = !cupped && (p - last).magnitude < 0.003f ? still + Time.deltaTime : 0f;
            last = p;
            if (still > 1.2f) { StartCoroutine(Missed("Stuck! No second shot...")); return; }
            if ((p.y < lostBelow && !cupped) || Mathf.Abs(p.x) > 3.4f || rollTime > 12f)
                StartCoroutine(Missed("Missed! Only one shot..."));
        }

        Vector2 Velocity()
        {
#if UNITY_6000_0_OR_NEWER
            return marble.linearVelocity;
#else
            return marble.velocity;
#endif
        }

        IEnumerator Missed(string say)
        {
            done = true;
            rolling = false;
            Sfx.Play(this, "bonk", 0.6f);
            Haptics.Light();
            if (bubble != null) bubble.Show(say);
            yield return new WaitForSeconds(1.4f);
            Fail();
        }

        /// <summary>Easter egg: la biglia vola dritta contro il vetro e lo schermo va in frantumi.</summary>
        IEnumerator Shatter()
        {
            done = true;
            rolling = false;
            if (bubble != null) bubble.Hide();
            // la biglia "esce" verso chi guarda: si ferma, cresce e si schianta sul vetro
            Vector2 v = Velocity();
            marble.bodyType = RigidbodyType2D.Kinematic;
#if UNITY_6000_0_OR_NEWER
            marble.linearVelocity = Vector2.zero;
#else
            marble.velocity = Vector2.zero;
#endif
            Transform t = marble.transform;
            Vector3 from = t.position, scale = t.localScale;
            Vector3 to = new Vector3(Mathf.Clamp(from.x + v.x * 0.15f, -2.3f, 2.3f), Mathf.Min(from.y + 0.6f, 3.3f), from.z);
            Sfx.Play(this, "whoosh", 0.8f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.35f)
            {
                t.position = Vector3.Lerp(from, to, k);
                t.localScale = scale * Mathf.Lerp(1f, 4.5f, k * k);
                yield return null;
            }
            Vector2 hit = t.position;
            yield return shatter.Play(hit);
            // la biglia ricade giù dietro le crepe
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.5f)
            {
                t.position = new Vector3(hit.x, hit.y - k * k * 9f, from.z);
                t.localScale = scale * Mathf.Lerp(4.5f, 1f, k);
                yield return null;
            }
            if (bubble != null) bubble.Show("You broke the screen!");
            yield return new WaitForSeconds(1.2f);
            FailWithEgg("glass_smash");
        }

        IEnumerator Score()
        {
            done = true;
            marble.bodyType = RigidbodyType2D.Kinematic;   // ferma nella tazza: niente più discesa
#if UNITY_6000_0_OR_NEWER
            marble.linearVelocity = Vector2.zero;
#else
            marble.velocity = Vector2.zero;
#endif
            marble.angularVelocity = 0f;
            if (confetti != null) confetti.Play();
            if (bubble != null) bubble.Show("Hole in one!");
            yield return new WaitForSeconds(1.5f);
            if (confetti != null) confetti.Stop();
            Win();
        }

        IEnumerator HideLater(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (bubble != null && !done) bubble.Hide();
        }
    }
}
