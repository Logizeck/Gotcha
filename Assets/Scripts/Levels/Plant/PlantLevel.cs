using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 26 — annaffia la piantina con la fionda. Ci sono 10 gocce; ne bastano 5 nel vaso.
    /// Si afferra la goccia nella fionda e la si tira all'indietro (lontano dal vaso): più la tendi e più va lontano,
    /// l'angolo decide la traiettoria (mostrata a puntini, senza vento!). Al rilascio la goccia parte verso il vaso.
    /// Il vento (manica a vento e strisce d'aria) sposta la goccia in volo: costante a ogni tiro, oppure a raffiche
    /// di 1-4 secondi con pause di 2-4. Ogni tanto passa un uccellino: colpirlo con l'acqua è l'Easter egg
    /// `angry_bird` (si arrabbia: "#%$@!" e "CRA CRA").
    /// </summary>
    public class PlantLevel : LevelController
    {
        [SerializeField] Rigidbody2D[] drops;
        [SerializeField] Transform pocket;           // punto di riposo della goccia nella fionda
        [SerializeField] Transform bandBack, bandFront;   // punte dei due rebbi (gli elastici partono da lì)
        [SerializeField] LineRenderer bands;
        [SerializeField] Transform[] aimDots;
        [SerializeField] Collider2D potZone;
        [SerializeField] float groundY = -3.1f;
        [SerializeField] float maxPull = 1.4f;
        [SerializeField] float power = 6.8f;
        [SerializeField] float grabRadius = 0.8f;
        [SerializeField] Transform plantDry;
        [SerializeField] GameObject plantBloom;
        [SerializeField] SmokeEmitter sparkle;
        [SerializeField] SmokeEmitter splash;
        [SerializeField] TextMesh counter;
        [SerializeField] TextMesh ammo;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] int needed = 5;

        public enum WindMode { None, PerShot, Gusts }
        [Header("Vento")]
        [SerializeField] WindMode windMode = WindMode.PerShot;
        [SerializeField] float windMax = 1.6f;               // accelerazione laterale massima (unità/s²)
        [SerializeField] Vector2 gustSeconds = new Vector2(1f, 4f);
        [SerializeField] Vector2 calmSeconds = new Vector2(2f, 4f);
        [SerializeField] Transform windsock;
        [SerializeField] SpriteRenderer[] streaks;
        [SerializeField] Transform[] ammoIcons;
        [SerializeField] Transform pouch;               // tasca di cuoio: segue la goccia quando tendi l'elastico

        [Header("Uccellino (Easter egg)")]
        [SerializeField] Transform bird;
        [SerializeField] GameObject birdUp, birdDown, birdAngry;
        [SerializeField] SpeechBubble birdBubble;
        [SerializeField] Vector2 birdEvery = new Vector2(5f, 9f);
        [SerializeField] float birdSpeed = 1.7f;
        [SerializeField] string eggId = "angry_bird";

        int next, absorbed;
        float wind, windTarget, windTimer, birdTimer, birdDir;
        bool gusting, birdFlying;
        AudioSource sfx;
        Rigidbody2D current;
        bool aiming, flying, done;
        float flyTime;
        Camera cam;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            foreach (Rigidbody2D d in drops) d.gameObject.SetActive(false);
            if (plantBloom != null) plantBloom.SetActive(false);
            if (sparkle != null) sparkle.Stop();
            if (splash != null) splash.Stop();
            ShowAim(false);
            if (bubble != null) { bubble.Show("Water... please..."); StartCoroutine(HideLater(2f)); }
            sfx = gameObject.AddComponent<AudioSource>();
            if (bird != null) bird.gameObject.SetActive(false);
            if (birdBubble != null) birdBubble.Hide();
            birdTimer = Random.Range(birdEvery.x, birdEvery.y);
            windTimer = Random.Range(calmSeconds.x, calmSeconds.y);
            for (int i = 0; streaks != null && i < streaks.Length; i++)
                streaks[i].transform.position = new Vector3(Random.Range(-3.2f, 3.2f), Random.Range(-2f, 3f), 0f);
            Load();
        }

        /// <summary>Mette la goccia successiva nella fionda.</summary>
        void Load()
        {
            current = null;
            UpdateTexts();
            if (next >= drops.Length) return;
            current = drops[next++];
            current.gameObject.SetActive(true);
            current.bodyType = RigidbodyType2D.Kinematic;
            current.position = pocket.position;
            current.transform.position = pocket.position;
            DrawBands(pocket.position);
            if (windMode == WindMode.PerShot)
            {
                // un vento diverso a ogni goccia (mai proprio zero)
                windTarget = Random.Range(0.3f, 1f) * windMax * (Random.value < 0.5f ? -1f : 1f);
            }
        }

        void UpdateWind()
        {
            if (windMode == WindMode.Gusts)
            {
                windTimer -= Time.deltaTime;
                if (windTimer <= 0f)
                {
                    gusting = !gusting;
                    windTimer = gusting ? Random.Range(gustSeconds.x, gustSeconds.y) : Random.Range(calmSeconds.x, calmSeconds.y);
                    windTarget = gusting ? Random.Range(0.6f, 1f) * windMax * (Random.value < 0.5f ? -1f : 1f) : 0f;
                }
            }
            else if (windMode == WindMode.None) windTarget = 0f;
            wind = Mathf.MoveTowards(wind, windTarget, windMax * 2.5f * Time.deltaTime);
            float k = windMax > 0f ? Mathf.Abs(wind) / windMax : 0f;
            if (windsock != null)
            {
                // la manica si gonfia e si alza col vento, e punta dove soffia
                float flutter = Mathf.Sin(Time.time * (6f + k * 10f)) * 4f * (0.3f + k);
                float droop = Mathf.Lerp(-80f, -5f, k) + flutter;
                windsock.localScale = new Vector3(wind < 0f ? -1f : 1f, Mathf.Lerp(0.55f, 1f, k), 1f);
                windsock.localRotation = Quaternion.Euler(0, 0, wind < 0f ? -droop : droop);
            }
            if (streaks != null)
                foreach (SpriteRenderer st in streaks)
                {
                    Vector3 p = st.transform.position;
                    p.x += (wind * 2.2f + Mathf.Sign(wind) * 1.2f) * Time.deltaTime;
                    if (p.x > 3.3f) { p.x = -3.3f; p.y = Random.Range(-2f, 3f); }
                    if (p.x < -3.3f) { p.x = 3.3f; p.y = Random.Range(-2f, 3f); }
                    st.transform.position = p;
                    st.color = new Color(1f, 1f, 1f, 0.55f * k);
                }
        }

        void FixedUpdate()
        {
            if (flying && current != null && current.bodyType == RigidbodyType2D.Dynamic)
                current.AddForce(Vector2.right * wind * current.mass);
        }

        void UpdateBird()
        {
            if (bird == null) return;
            if (!birdFlying)
            {
                birdTimer -= Time.deltaTime;
                if (birdTimer > 0f) return;
                birdFlying = true;
                birdDir = Random.value < 0.5f ? 1f : -1f;
                bird.position = new Vector3(-3.6f * birdDir, Random.Range(1.0f, 2.6f), 0f);
                bird.localScale = new Vector3(birdDir, 1f, 1f);
                bird.gameObject.SetActive(true);
            }
            Vector3 b = bird.position;
            b.x += birdDir * birdSpeed * Time.deltaTime;
            b.y += Mathf.Sin(Time.time * 3f) * 0.3f * Time.deltaTime;
            bird.position = b;
            bool up = Mathf.Repeat(Time.time * 7f, 1f) < 0.5f;
            if (birdUp != null) birdUp.SetActive(up);
            if (birdDown != null) birdDown.SetActive(!up);
            if (Mathf.Abs(b.x) > 3.7f)
            {
                birdFlying = false;
                bird.gameObject.SetActive(false);
                birdTimer = Random.Range(birdEvery.x, birdEvery.y);
            }
        }

        /// <summary>Easter egg: la goccia centra l'uccellino, che si infuria.</summary>
        IEnumerator BirdHit()
        {
            done = true;
            flying = false;
            if (current != null) current.gameObject.SetActive(false);
            if (splash != null) { splash.transform.position = bird.position; splash.Play(); }
            if (birdUp != null) birdUp.SetActive(false);
            if (birdDown != null) birdDown.SetActive(false);
            if (birdAngry != null) birdAngry.SetActive(true);
            if (birdBubble != null)
            {
                // il fumetto è figlio dell'uccellino: va "rigirato" se vola verso sinistra, o il testo esce a specchio
                Vector3 bs = birdBubble.transform.localScale;
                birdBubble.transform.localScale = new Vector3(Mathf.Abs(bs.x) * Mathf.Sign(bird.localScale.x), bs.y, bs.z);
                birdBubble.Show("#%$@!", true);
            }
            if (sfx != null) sfx.PlayOneShot(CraCra());
            Vector3 home = bird.position;
            for (float k = 0f; k < 1.1f; k += Time.deltaTime)
            {
                bird.position = home + (Vector3)(Random.insideUnitCircle * 0.06f);   // trema di rabbia, gocciolando
                if (splash != null && k > 0.15f) splash.Stop();
                yield return null;
            }
            if (sfx != null) sfx.PlayOneShot(CraCra());
            if (birdBubble != null) birdBubble.Show("CRA! CRA!", true);
            for (float k = 0f; k < 0.9f; k += Time.deltaTime)
            {
                bird.position += new Vector3(birdDir * 3.5f, 2f, 0f) * Time.deltaTime;   // se ne va offeso
                yield return null;
            }
            if (birdBubble != null) birdBubble.Hide();
            if (bubble != null) bubble.Show("...was that a bird?");
            yield return new WaitForSeconds(1.2f);
            FailWithEgg(eggId);
        }

        static AudioClip craCra;

        /// <summary>"CRA CRA": due versi rauchi da corvo, sintetizzati.</summary>
        static AudioClip CraCra()
        {
            if (craCra != null) return craCra;
            const int rate = 22050;
            int n = (int)(rate * 0.62f);
            var data = new float[n];
            var rnd = new System.Random(5);
            float ph = 0f;
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)rate;
                float local = t < 0.25f ? t : t - 0.34f;
                if (local < 0f || local > 0.25f) { data[i] = 0f; continue; }
                float f = Mathf.Lerp(620f, 430f, local / 0.25f);
                ph += f / rate;
                float saw = 2f * Mathf.Repeat(ph, 1f) - 1f;
                float rough = (float)rnd.NextDouble() * 2f - 1f;
                float env = Mathf.Min(1f, local / 0.02f) * Mathf.Clamp01((0.25f - local) / 0.08f);
                data[i] = (saw * 0.6f + rough * 0.35f) * env * (0.7f + 0.3f * Mathf.Sin(t * 2f * Mathf.PI * 70f)) * 0.5f;
            }
            craCra = AudioClip.Create("cracra", n, 1, rate, false);
            craCra.SetData(data, 0);
            return craCra;
        }

        void Update()
        {
            if (done || IsFinished) return;
            UpdateWind();
            UpdateBird();
            if (flying) { Fly(); return; }
            if (current == null) return;

            if (PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out bool ended))
            {
                Vector2 w = PointerInput.ToWorld(cam, sp);
                if (began && Vector2.Distance(w, pocket.position) < grabRadius) aiming = true;
                if (aiming)
                {
                    Vector2 pull = Vector2.ClampMagnitude(w - (Vector2)pocket.position, maxPull);
                    Vector2 at = (Vector2)pocket.position + pull;
                    current.transform.position = at;
                    DrawBands(at);
                    ShowTrajectory(at, -pull * power);
                    if (ended)
                    {
                        aiming = false;
                        ShowAim(false);
                        if (pull.magnitude < 0.2f) { current.transform.position = pocket.position; DrawBands(pocket.position); }
                        else Launch(at, -pull * power);
                    }
                }
            }
            else if (aiming)
            {
                aiming = false;
                ShowAim(false);
            }
        }

        void Launch(Vector2 at, Vector2 velocity)
        {
            current.position = at;
            current.bodyType = RigidbodyType2D.Dynamic;
            current.AddForce(velocity * current.mass, ForceMode2D.Impulse);   // impulso = velocità iniziale
            flying = true;
            flyTime = 0f;
            DrawBands(pocket.position);   // l'elastico torna dritto
        }

        void Fly()
        {
            flyTime += Time.deltaTime;
            Vector2 p = current.position;
            if (birdFlying && bird != null && Vector2.Distance(p, bird.position) < 0.42f) { StartCoroutine(BirdHit()); return; }
            if (potZone.OverlapPoint(p)) { StartCoroutine(Absorbed()); return; }
            if (p.y < groundY || Mathf.Abs(p.x) > 3.6f || flyTime > 4f) StartCoroutine(Missed(p));
        }

        IEnumerator Absorbed()
        {
            flying = false;
            current.gameObject.SetActive(false);
            absorbed++;
            Grow();
            UpdateTexts();
            if (sparkle != null) { sparkle.Play(); yield return new WaitForSeconds(0.2f); sparkle.Stop(); }
            if (absorbed >= needed) { yield return Bloom(); yield break; }
            yield return new WaitForSeconds(0.3f);
            Load();
        }

        IEnumerator Missed(Vector2 at)
        {
            flying = false;
            current.gameObject.SetActive(false);
            if (splash != null) { splash.transform.position = new Vector3(at.x, Mathf.Max(at.y, groundY), 0f); splash.Play(); }
            yield return new WaitForSeconds(0.15f);
            if (splash != null) splash.Stop();
            if (drops.Length - next + absorbed < needed)
            {
                done = true;
                UpdateTexts();
                if (bubble != null) bubble.Show("Not enough water...");
                yield return new WaitForSeconds(1.3f);
                Fail();
                yield break;
            }
            yield return new WaitForSeconds(0.3f);
            Load();
        }

        void Grow()
        {
            if (plantDry == null) return;
            float k = Mathf.Clamp01(absorbed / (float)needed);
            plantDry.localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(0f, 12f, k));   // si raddrizza un po'
        }

        void UpdateTexts()
        {
            if (counter != null) counter.text = Mathf.Min(absorbed, needed) + " / " + needed;
            int leftDrops = drops.Length - next + (current != null && !flying ? 1 : 0);
            if (ammo != null) ammo.text = "DROPS LEFT: " + leftDrops;
            if (ammoIcons != null) for (int i = 0; i < ammoIcons.Length; i++) if (ammoIcons[i] != null) ammoIcons[i].gameObject.SetActive(i < leftDrops);
        }

        void DrawBands(Vector2 at)
        {
            if (bands == null) return;
            bands.positionCount = 3;
            bands.SetPosition(0, bandBack.position);
            bands.SetPosition(1, new Vector3(at.x, at.y, 0f));
            bands.SetPosition(2, bandFront.position);
            if (pouch != null) pouch.position = new Vector3(at.x, at.y, pouch.position.z);
        }

        void ShowTrajectory(Vector2 from, Vector2 v)
        {
            Vector2 g = Physics2D.gravity * (current != null ? current.gravityScale : 1f);
            for (int i = 0; i < aimDots.Length; i++)
            {
                float t = (i + 1) * 0.08f;
                Vector2 p = from + v * t + 0.5f * g * t * t;
                aimDots[i].position = new Vector3(p.x, p.y, 0f);
                aimDots[i].gameObject.SetActive(true);
            }
        }

        void ShowAim(bool on) { foreach (Transform d in aimDots) d.gameObject.SetActive(on); }

        IEnumerator Bloom()
        {
            done = true;
            if (plantDry != null) plantDry.gameObject.SetActive(false);
            if (plantBloom != null)
            {
                // il fiore sboccia e fa due saltelli di felicità (stesse dimensioni, si muove soltanto)
                plantBloom.SetActive(true);
                Transform t = plantBloom.transform;
                Vector3 p = t.localPosition;
                for (int hop = 0; hop < 2; hop++)
                    for (float k = 0f; k < 1f; k += Time.deltaTime / 0.35f)
                    {
                        t.localPosition = p + Vector3.up * Mathf.Sin(k * Mathf.PI) * 0.25f;
                        t.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(k * Mathf.PI * 2f) * 6f);
                        yield return null;
                    }
                t.localPosition = p;
                t.localRotation = Quaternion.identity;
            }
            if (sparkle != null) sparkle.Play();
            if (bubble != null) bubble.Show("Thank you!");
            yield return new WaitForSeconds(1.4f);
            if (sparkle != null) sparkle.Stop();
            Win();
        }

        IEnumerator HideLater(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (bubble != null && !done) bubble.Hide();
        }
    }
}
