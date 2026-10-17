using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;
using Gotcha.Sensors;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 21 — batteria scarica. Il robottino è spento: "low battery...". Si risolve collegando DAVVERO il
    /// telefono al caricatore. Se il telefono era già in carica all'inizio, bisogna staccarlo e riattaccarlo
    /// (deve esserci un vero "attacco" durante il livello). Nell'Editor il caricatore si simula col tasto C.
    /// Senza caricatore a portata di mano: "NO CHARGER?" fa comparire una dinamo a manovella da girare col dito.
    /// Funziona, ma il robot si accende col muso lungo (la corrente vera è un'altra cosa).
    /// </summary>
    public class BatteryLevel : LevelController
    {
        [SerializeField] GameObject robotOff;
        [SerializeField] GameObject robotOn;
        [SerializeField] Transform robot;
        [SerializeField] Transform batteryFill;      // scala x da 0 a 1, ancorata a sinistra
        [SerializeField] SpriteRenderer batteryBar;
        [SerializeField] SpriteRenderer batteryFrame;
        [SerializeField] GameObject bolt;
        [SerializeField] SmokeEmitter notes;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] float chargeSeconds = 2.5f;
        [Header("Senza caricatore: dinamo a manovella")]
        [SerializeField] TapButton skipButton;
        [SerializeField] GameObject crank;
        [SerializeField] Transform crankArm;
        [SerializeField] float crankRadius = 1.4f;
        [SerializeField] float turnsNeeded = 6f;
        [SerializeField] GameObject robotSad;
        [Header("Easter egg: col caricatore vero il robot si evolve")]
        [SerializeField] GameObject robotMecha;
        [SerializeField] GameObject batteryUi;
        [SerializeField] SpriteRenderer aura;
        [SerializeField] SpriteRenderer flash;
        [SerializeField] SmokeEmitter sparks;
        [SerializeField] string eggId = "mecha_robot";
        [SerializeField] SpriteRenderer dim;          // velo scuro dietro al robot
        [SerializeField] Transform rays;              // raggiera che gira
        [SerializeField] Transform shockRing;
        [SerializeField] Transform[] armor;           // pezzi d'armatura che arrivano in volo
        [SerializeField] LineRenderer[] lightning;    // archi elettrici attorno al robot
        [SerializeField] TextMesh banner;             // "GATTAI!!"
        [SerializeField] AudioSource sfx;

        bool armed, done, cranking, cranked;
        float lastAngle;
        bool turning;
        float charge = 0.06f;
        Camera cam;
        Vector3 robotHome;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            if (robot != null) robotHome = robot.localPosition;
            bool plugged = SensorHub.Instance != null && SensorHub.Instance.IsCharging;
            armed = !plugged;   // già in carica: prima va staccato
            if (robotOn != null) robotOn.SetActive(false);
            if (robotSad != null) robotSad.SetActive(false);
            if (robotMecha != null) robotMecha.SetActive(false);
            if (aura != null) aura.color = new Color(1f, 1f, 1f, 0f);
            if (flash != null) flash.color = new Color(1f, 1f, 1f, 0f);
            if (dim != null) dim.color = new Color(0f, 0f, 0f, 0f);
            if (rays != null) rays.gameObject.SetActive(false);
            if (shockRing != null) shockRing.gameObject.SetActive(false);
            if (banner != null) banner.gameObject.SetActive(false);
            if (armor != null) foreach (Transform a in armor) if (a != null) a.gameObject.SetActive(false);
            if (lightning != null) foreach (LineRenderer l in lightning) if (l != null) l.enabled = false;
            if (crank != null) crank.SetActive(false);
            if (skipButton != null) skipButton.Pressed += OnSkip;
            if (bolt != null) bolt.SetActive(false);
            if (notes != null) notes.Stop();
            if (bubble != null) { bubble.Show("Low... battery..."); StartCoroutine(HideLater(2f)); }
            Show();
        }

        void OnDestroy()
        {
            if (skipButton != null) skipButton.Pressed -= OnSkip;
        }

        void OnSkip(TapButton b)
        {
            if (done || cranking) return;
            cranking = true;
            skipButton.gameObject.SetActive(false);
            if (crank != null) crank.SetActive(true);
            if (bubble != null) { bubble.Show("Spin it! Spin it!"); StartCoroutine(HideLater(1.5f)); }
        }

        /// <summary>Girando la manovella col dito: ogni giro carica un po' (in qualsiasi verso).</summary>
        void Crank()
        {
            if (!cranking || crank == null) return;
            Vector2 c = crank.transform.position;
            if (PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out bool ended) && !ended)
            {
                Vector2 w = PointerInput.ToWorld(cam, sp) - c;
                if (w.magnitude < crankRadius && w.magnitude > 0.15f)
                {
                    float a = Mathf.Atan2(w.y, w.x) * Mathf.Rad2Deg;
                    if (turning && !began)
                    {
                        float d = Mathf.DeltaAngle(lastAngle, a);
                        charge = Mathf.Min(1f, charge + Mathf.Abs(d) / (360f * turnsNeeded));
                        if (Mathf.Abs(d) > 0.5f) cranked = true;
                    }
                    turning = true;
                    lastAngle = a;
                    if (crankArm != null) crankArm.rotation = Quaternion.Euler(0, 0, a);
                }
                else turning = false;
            }
            else turning = false;
            if (charge >= 1f && cranked) StartCoroutine(PowerOn(true));
        }

        void Update()
        {
            if (done || IsFinished) return;
            Crank();
            if (done) return;
            bool plugged = SensorHub.Instance != null && SensorHub.Instance.IsCharging;
            if (!plugged) armed = true;
            bool charging = armed && plugged;
            if (bolt != null) bolt.SetActive(charging);

            if (charging)
            {
                charge = Mathf.MoveTowards(charge, 1f, Time.deltaTime / chargeSeconds);
                if (robot != null) robot.localPosition = robotHome + new Vector3(Mathf.Sin(Time.time * 50f) * 0.02f * charge, 0f, 0f);   // brividi di energia
                if (charge >= 1f) StartCoroutine(PowerOn(false));
            }
            else
            {
                if (robot != null) robot.localPosition = robotHome;
                // toccare il robot spento: russa
                if (PointerInput.AnyTouchBegan() && PointerInput.TryGetPrimary(out Vector2 sp, out _, out _) && robot != null)
                {
                    Collider2D c = robot.GetComponent<Collider2D>();
                    if (c != null && c.OverlapPoint(PointerInput.ToWorld(cam, sp)) && bubble != null) { bubble.Show("Need... energy... zzz"); StartCoroutine(HideLater(1.4f)); }
                }
            }
            Show();
        }

        void Show()
        {
            if (batteryFill != null) batteryFill.localScale = new Vector3(charge, 1f, 1f);
            if (batteryBar != null) batteryBar.color = charge < 0.25f ? new Color(1f, 0.25f, 0.25f) : charge < 0.6f ? new Color(1f, 0.82f, 0.2f) : new Color(0.36f, 1f, 0.45f);
            if (batteryFrame != null)
            {
                bool blink = charge < 0.25f && Mathf.Repeat(Time.time * 2f, 1f) < 0.5f;
                batteryFrame.color = blink ? new Color(1f, 0.4f, 0.4f) : Color.white;
            }
        }

        IEnumerator PowerOn(bool byHand)
        {
            done = true;
            if (robotOff != null) robotOff.SetActive(false);
            if (bolt != null) bolt.SetActive(false);
            if (skipButton != null) skipButton.gameObject.SetActive(false);
            if (byHand)
            {
                // acceso a manovella: funziona, ma che tristezza
                if (robotSad != null) robotSad.SetActive(true);
                if (bubble != null) bubble.Show("Meh... tastes like hamster wheel. :(");
                Vector3 h = robot.localPosition;
                for (float k = 0f; k < 1.8f; k += Time.deltaTime)
                {
                    robot.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(k * 2.5f) * 3f);   // dondola sconsolato
                    yield return null;
                }
                robot.localRotation = Quaternion.identity;
                robot.localPosition = h;
                Win();
                yield break;
            }
            // caricatore vero: 100%... e poi sovraccarico! Il robot si evolve in un robot da battaglia
            yield return Evolve();
        }

        Vector3 Center => robot.position + new Vector3(0f, 1.9f, 0f);

        /// <summary>
        /// Trasformazione "da anime": fulmini e tremori, il robot si solleva bianco incandescente davanti a una raggiera,
        /// i pezzi dell'armatura arrivano in volo uno a uno (clang!), lampo, GATTAI!!, atterraggio con onda d'urto.
        /// </summary>
        IEnumerator Evolve()
        {
            SpriteRenderer onArt = robotOn != null ? robotOn.GetComponent<SpriteRenderer>() : null;
            if (robotOn != null) robotOn.SetActive(true);
            if (bubble != null) bubble.Show("100%! ...wait... MORE?!");
            Vector3 home = robotHome;
            Transform level = transform;
            Vector3 levelHome = level.localPosition;

            // 1. sovraccarico: trema sempre di più, scintille e archi elettrici
            for (float k = 0f; k < 1.8f; k += Time.deltaTime)
            {
                float e = k / 1.8f;
                robot.localPosition = home + (Vector3)(Random.insideUnitCircle * 0.07f * e);
                if (aura != null) { aura.color = new Color(0.6f, 0.95f, 1f, e * 0.9f); aura.transform.localScale = Vector3.one * (6f + Mathf.Sin(k * 30f) * 0.6f); }
                if (sparks != null && k > 0.3f) sparks.Play();
                Zap(e);
                if (k > 1.2f) level.localPosition = levelHome + (Vector3)(Random.insideUnitCircle * 0.05f);
                yield return null;
            }
            level.localPosition = levelHome;
            if (bubble != null) bubble.Show("OVERCHARGE!!!");
            if (batteryUi != null) batteryUi.SetActive(false);

            // 2. si solleva, bianco incandescente, davanti alla raggiera; lo sfondo si oscura
            if (rays != null) rays.gameObject.SetActive(true);
            Vector3 up = home + Vector3.up * 1.1f;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.8f)
            {
                float e = Mathf.SmoothStep(0f, 1f, k);
                robot.localPosition = Vector3.Lerp(home, up, e) + (Vector3)(Random.insideUnitCircle * 0.03f);
                if (dim != null) dim.color = new Color(0.05f, 0.03f, 0.12f, 0.8f * e);
                if (rays != null) { rays.localScale = Vector3.one * Mathf.Lerp(0.2f, 1.6f, e); rays.Rotate(0, 0, 90f * Time.deltaTime); }
                if (onArt != null) onArt.color = Color.Lerp(Color.white, new Color(2f, 2f, 2.4f), e);
                Zap(1f);
                yield return null;
            }
            if (sparks != null) sparks.Stop();
            Zap(-1f);

            // 3. i pezzi dell'armatura arrivano dai bordi e si agganciano, uno dopo l'altro
            if (armor != null)
                for (int i = 0; i < armor.Length; i++)
                {
                    Transform piece = armor[i];
                    if (piece == null) continue;
                    piece.gameObject.SetActive(true);
                    float side = i % 2 == 0 ? -1f : 1f;
                    Vector3 from = Center + new Vector3(side * 4.5f, 2.5f - i * 1.2f, 0f);
                    Vector3 to = Center + new Vector3(side * 0.15f, 0.6f - i * 0.35f, 0f);
                    for (float k = 0f; k < 1f; k += Time.deltaTime / 0.28f)
                    {
                        piece.position = Vector3.Lerp(from, to, k * k);
                        piece.Rotate(0, 0, side * -900f * Time.deltaTime);
                        if (rays != null) rays.Rotate(0, 0, 90f * Time.deltaTime);
                        yield return null;
                    }
                    piece.rotation = Quaternion.identity;
                    if (sparks != null) { sparks.transform.position = to; sparks.Play(); }
                    if (sfx != null) sfx.PlayOneShot(Clang());
                    level.localPosition = levelHome + (Vector3)(Random.insideUnitCircle * 0.08f);
                    for (float k = 0f; k < 1f; k += Time.deltaTime / 0.12f)
                    {
                        piece.localScale = Vector3.one * Mathf.Lerp(1.3f, 0.2f, k);   // si fonde col corpo
                        yield return null;
                    }
                    level.localPosition = levelHome;
                    if (sparks != null) sparks.Stop();
                    piece.gameObject.SetActive(false);
                }

            // 4. lampo bianco: durante il lampo il robot diventa un robot da battaglia
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.12f) { if (flash != null) flash.color = new Color(1f, 1f, 1f, k); yield return null; }
            if (robotOn != null) robotOn.SetActive(false);
            if (robotMecha != null) robotMecha.SetActive(true);
            home = robotHome + new Vector3(0.5f, 0f, 0f);   // il robot da battaglia è più grosso: un passo verso il centro
            up = home + Vector3.up * 1.1f;
            robot.localPosition = up;
            if (banner != null) banner.gameObject.SetActive(true);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.5f)
            {
                if (flash != null) flash.color = new Color(1f, 1f, 1f, 1f - k);
                if (aura != null) aura.color = new Color(1f, 0.85f, 0.4f, (1f - k) * 0.9f);
                if (banner != null) banner.transform.localScale = Vector3.one * (k < 0.4f ? Mathf.Lerp(3f, 0.85f, k / 0.4f) : Mathf.Lerp(0.85f, 1f, (k - 0.4f) / 0.6f));
                if (rays != null) rays.Rotate(0, 0, 120f * Time.deltaTime);
                yield return null;
            }

            // 5. atterraggio pesante con onda d'urto
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.25f)
            {
                robot.localPosition = Vector3.Lerp(up, home, k * k);
                if (rays != null) rays.Rotate(0, 0, 120f * Time.deltaTime);
                yield return null;
            }
            robot.localPosition = home;
            if (sfx != null) sfx.PlayOneShot(Clang());
            if (shockRing != null) shockRing.gameObject.SetActive(true);
            SpriteRenderer ring = shockRing != null ? shockRing.GetComponent<SpriteRenderer>() : null;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.6f)
            {
                level.localPosition = levelHome + (Vector3)(Random.insideUnitCircle * 0.12f * (1f - k));
                if (shockRing != null) { shockRing.position = robot.position + new Vector3(0f, 0.2f, 0f); shockRing.localScale = new Vector3(1f, 0.35f, 1f) * Mathf.Lerp(0.5f, 4.5f, k); }
                if (ring != null) ring.color = new Color(1f, 1f, 1f, 1f - k);
                if (rays != null) rays.Rotate(0, 0, 120f * Time.deltaTime);
                yield return null;
            }
            level.localPosition = levelHome;
            if (shockRing != null) shockRing.gameObject.SetActive(false);
            if (bubble != null) bubble.Show("ARIGATO, MASTER!");
            if (sparks != null) { sparks.transform.position = robot.position + new Vector3(-1.1f, 1.2f, 0f); sparks.Play(); yield return new WaitForSeconds(0.2f); sparks.Stop(); }
            for (float k = 0f; k < 1.6f; k += Time.deltaTime)
            {
                if (rays != null) rays.Rotate(0, 0, 60f * Time.deltaTime);
                yield return null;
            }
            // il velo scuro sparisce
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.4f)
            {
                if (dim != null) dim.color = new Color(0.05f, 0.03f, 0.12f, 0.8f * (1f - k));
                if (rays != null) rays.localScale = Vector3.one * Mathf.Lerp(1.6f, 0f, k);
                yield return null;
            }
            WinWithEgg(eggId);
        }

        /// <summary>Archi elettrici a zig-zag attorno al robot (intensità 0..1; negativo = spenti).</summary>
        void Zap(float intensity)
        {
            if (lightning == null) return;
            foreach (LineRenderer lr in lightning)
            {
                if (lr == null) continue;
                bool on = intensity > 0f && Random.value < 0.35f + intensity * 0.5f;
                lr.enabled = on;
                if (!on) continue;
                Vector3 c = Center;
                Vector3 a = c + (Vector3)(Random.insideUnitCircle.normalized * Random.Range(0.6f, 1.2f));
                Vector3 b = c + (Vector3)(Random.insideUnitCircle.normalized * Random.Range(1.4f, 2.4f));
                lr.positionCount = 6;
                for (int i = 0; i < 6; i++)
                {
                    Vector3 p = Vector3.Lerp(a, b, i / 5f);
                    if (i > 0 && i < 5) p += (Vector3)(Random.insideUnitCircle * 0.22f);
                    lr.SetPosition(i, p);
                }
            }
        }

        static AudioClip clang;

        /// <summary>"CLANG": colpo metallico sintetizzato (generato una volta).</summary>
        static AudioClip Clang()
        {
            if (clang != null) return clang;
            const int rate = 22050;
            int n = rate / 2;
            var data = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)rate;
                float env = Mathf.Exp(-t * 9f);
                data[i] = (Mathf.Sin(t * 2f * Mathf.PI * 523f) * 0.5f + Mathf.Sin(t * 2f * Mathf.PI * 1390f) * 0.3f + Mathf.Sin(t * 2f * Mathf.PI * 2210f) * 0.2f) * env * 0.5f;
            }
            clang = AudioClip.Create("clang", n, 1, rate, false);
            clang.SetData(data, 0);
            return clang;
        }

        IEnumerator HideLater(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (bubble != null && !done) bubble.Hide();
        }
    }
}
