using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "Garden seasons" — un anno nell'orto in quattro fasi. PRIMAVERA: trascina il seme nella buca, poi
    /// inclina il telefono a sinistra per versare l'annaffiatoio finché la terra è scura. ESTATE: scorri via le nuvole
    /// davanti al sole (la pianta cresce), poi arriva la canicola: trascina l'ombrellone perché la sua OMBRA (che
    /// cade dalla parte opposta al sole, e il sole si sposta!) copra la pianta per 5 secondi. AUTUNNO: crescono 4
    /// zucche; la talpa sbuca da 3 tane per rubarle: toccala per darle una botta, salvane almeno 3 (ma non colpire il
    /// gatto, che ogni tanto fa capolino). INVERNO: scuoti il telefono per far cadere la neve dal tetto della serra
    /// finché passa la tormenta. Vittoria: festa del raccolto. 3 cuori: ogni fase fallita costa un cuore e si ripete.
    /// </summary>
    public class SeasonsLevel : LevelController
    {
        [SerializeField] StageHud40D hud;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] SpriteRenderer[] lives;
        [SerializeField] SpriteRenderer backdrop;
        [SerializeField] Transform meter;             // perno della barra (scala x)
        [SerializeField] SpriteRenderer meterFill;
        [SerializeField] TextMesh meterLabel;
        // primavera
        [SerializeField] Transform seed;
        [SerializeField] Collider2D seedArea;
        [SerializeField] Transform hole;
        [SerializeField] SpriteRenderer mound;
        [SerializeField] Transform can;
        [SerializeField] Transform spout;            // punta del beccuccio (figlio dell'annaffiatoio)
        [SerializeField] Transform[] drops;
        // estate
        [SerializeField] Transform[] clouds;
        [SerializeField] Transform sun;
        [SerializeField] SpriteRenderer sunGlow;
        [SerializeField] Transform sprout;
        [SerializeField] Transform plant;
        [SerializeField] SpriteRenderer plantArt;
        [SerializeField] Transform umbrella;
        [SerializeField] Collider2D umbrellaArea;
        [SerializeField] Transform shadow;
        // autunno
        [SerializeField] Transform[] pumpkins;
        [SerializeField] Transform[] holes;
        [SerializeField] Transform mole;
        [SerializeField] Transform cat;              // il gatto che fa capolino da una tana
        [SerializeField] Transform stars;
        // inverno
        [SerializeField] Transform greenhouse;
        [SerializeField] Transform[] lumps;
        [SerializeField] Transform[] flakes;
        [SerializeField] SpriteRenderer snowGround;
        // festa
        [SerializeField] Transform[] confetti;

        static readonly Color Spring = Color.white, Summer = new Color(1f, 0.96f, 0.84f), Heat = new Color(1f, 0.85f, 0.68f),
            Autumn = new Color(1f, 0.84f, 0.62f), Winter = new Color(0.8f, 0.87f, 1f);

        Camera cam;
        int livesLeft = 3;
        float bubbleUntil, clock;
        Vector3 seedHome, umbrellaHome, greenhouseHome;
        Vector3[] pumpkinHome, lumpHome;
        bool[] lumpFalling;
        float plantY;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            seedHome = seed.position;
            umbrellaHome = umbrella.position;
            greenhouseHome = greenhouse.position;
            plantY = plant.position.y;
            pumpkinHome = new Vector3[pumpkins.Length];
            for (int i = 0; i < pumpkins.Length; i++) { pumpkinHome[i] = pumpkins[i].position; pumpkins[i].gameObject.SetActive(false); }
            lumpHome = new Vector3[lumps.Length];
            lumpFalling = new bool[lumps.Length];
            for (int i = 0; i < lumps.Length; i++) { lumpHome[i] = lumps[i].localPosition; lumps[i].gameObject.SetActive(false); }
            foreach (Transform d in drops) d.gameObject.SetActive(false);
            foreach (Transform f in flakes) f.gameObject.SetActive(false);
            foreach (Transform c in confetti) c.gameObject.SetActive(false);
            foreach (Transform c in clouds) c.gameObject.SetActive(false);
            mound.gameObject.SetActive(false);
            can.gameObject.SetActive(false);
            sprout.gameObject.SetActive(false);
            plant.gameObject.SetActive(false);
            umbrella.gameObject.SetActive(false);
            shadow.gameObject.SetActive(false);
            mole.gameObject.SetActive(false);
            cat.gameObject.SetActive(false);
            stars.gameObject.SetActive(false);
            foreach (Transform h in holes) h.gameObject.SetActive(false);
            greenhouse.gameObject.SetActive(false);
            snowGround.gameObject.SetActive(false);
            if (sunGlow != null) sunGlow.gameObject.SetActive(false);
            ShowMeter(false);
            StartCoroutine(Run());
        }

        void Say(string text, float seconds)
        {
            if (bubble == null) return;
            bubble.Show(text);
            bubbleUntil = clock + seconds;
        }

        void Update()
        {
            clock += Time.deltaTime;
            if (bubble != null && bubbleUntil > 0f && clock > bubbleUntil) { bubble.Hide(); bubbleUntil = 0f; }
        }

        // ---------- aiuti ----------

        void ShowMeter(bool on, string label = "")
        {
            meter.parent.gameObject.SetActive(on);
            if (meterLabel != null) meterLabel.text = label;
        }

        void SetMeter(float v, Color c)
        {
            meter.localScale = new Vector3(Mathf.Clamp01(v), 1f, 1f);
            if (meterFill != null) meterFill.color = c;
        }

        IEnumerator Tint(Color to, float seconds)
        {
            Color from = backdrop.color;
            for (float k = 0f; k < 1f; k += Time.deltaTime / seconds) { backdrop.color = Color.Lerp(from, to, k); yield return null; }
            backdrop.color = to;
        }

        IEnumerator Pop(Transform t, float to = 1f, float seconds = 0.3f)
        {
            t.gameObject.SetActive(true);
            for (float k = 0f; k < 1f; k += Time.deltaTime / seconds) { t.localScale = Vector3.one * to * (k + Mathf.Sin(k * Mathf.PI) * 0.25f); yield return null; }
            t.localScale = Vector3.one * to;
        }

        /// <summary>Perde un cuore; false se erano finiti (livello perso).</summary>
        bool LoseLife()
        {
            livesLeft--;
            Haptics.Medium();
            Sfx.Play(this, "buzz", 0.7f);
            if (livesLeft >= 0 && livesLeft < lives.Length) StartCoroutine(FadeHeart(lives[livesLeft]));
            return livesLeft > 0;
        }

        IEnumerator FadeHeart(SpriteRenderer h)
        {
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.4f) { h.transform.localScale = Vector3.one * 0.6f * (1f + k * 0.5f); h.color = new Color(1f, 1f, 1f, 1f - k); yield return null; }
            h.gameObject.SetActive(false);
        }

        IEnumerator Lost(string why)
        {
            hud.Goal("Out of hearts!");
            Say(why, 3f);
            yield return new WaitForSeconds(2.2f);
            Fail();
        }

        bool Pointer(out Vector2 w, out bool began, out bool ended)
        {
            w = Vector2.zero;
            if (!PointerInput.TryGetPrimary(out Vector2 sp, out began, out ended)) return false;
            if (began && PointerInput.IsOverUi()) { began = false; return false; }
            w = PointerInput.ToWorld(cam, sp);
            return true;
        }

        // ---------- sequenza ----------

        IEnumerator Run()
        {
            yield return Spring1();
            yield return Summer2();
            if (IsFinished) yield break;
            yield return Autumn3();
            if (IsFinished) yield break;
            yield return Winter4();
            if (IsFinished) yield break;
            yield return Party();
        }

        // ---------- 1. primavera: seme e annaffiatoio ----------

        IEnumerator Spring1()
        {
            hud.SetStage(0);
            hud.Goal("SPRING: drag the seed into the hole");
            Say("Plant me! Plant me!", 2f);
            bool planted = false;
            bool dragging = false;
            Vector2 offset = Vector2.zero;
            while (!planted)
            {
                if (Pointer(out Vector2 w, out bool began, out bool ended))
                {
                    if (began && seedArea.OverlapPoint(w)) { dragging = true; offset = (Vector2)seed.position - w; Sfx.Play(this, "tick", 0.6f); }
                    if (dragging)
                    {
                        Vector2 p = w + offset;
                        seed.position = new Vector3(Mathf.Clamp(p.x, -2.6f, 2.6f), Mathf.Clamp(p.y, -4.3f, 2.5f), seed.position.z);
                        if (ended)
                        {
                            dragging = false;
                            if (Vector2.Distance(seed.position, hole.position) < 0.7f) planted = true;
                            else { Sfx.Play(this, "whoosh", 0.4f); yield return Slide(seed, seedHome, 0.25f); Say("In the HOLE, please.", 1.6f); }
                        }
                    }
                }
                else dragging = false;
                yield return null;
            }
            // il seme scende nella buca e la terra lo copre
            Sfx.Play(this, "pop", 0.7f);
            Vector3 from = seed.position;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.35f) { seed.position = Vector3.Lerp(from, hole.position, k); seed.localScale = Vector3.one * (1f - k * 0.6f); yield return null; }
            seed.gameObject.SetActive(false);
            hole.gameObject.SetActive(false);
            mound.color = Color.white;
            yield return Pop(mound.transform);
            Haptics.Light();
            Say("Thirsty...", 1.5f);

            // arriva l'annaffiatoio
            can.gameObject.SetActive(true);
            Vector3 canAt = can.position;
            can.position = canAt + Vector3.up * 4f;
            yield return Slide(can, canAt, 0.4f);
            hud.Goal("Tilt the phone LEFT to water it!");
            ShowMeter(true, "WET SOIL");
            float wet = 0f, angle = 0f, dropTimer = 0f, hintTimer = 0f;
            int nextDrop = 0;
            Color dry = Color.white, soaked = new Color(0.5f, 0.36f, 0.3f);
            while (wet < 1f)
            {
                Vector2 tilt = Inputs.Tilt();
                float target = Mathf.Clamp(-tilt.x * 110f, -12f, 65f);
                angle = Mathf.Lerp(angle, target, Time.deltaTime * 6f);
                can.localRotation = Quaternion.Euler(0, 0, angle);
                bool pouring = angle > 25f;
                if (pouring)
                {
                    wet += Time.deltaTime / 3.2f * Mathf.InverseLerp(25f, 45f, angle) + Time.deltaTime * 0.05f;
                    dropTimer -= Time.deltaTime;
                    if (dropTimer <= 0f && drops.Length > 0)
                    {
                        dropTimer = 0.07f;
                        StartCoroutine(Drop(drops[nextDrop], spout.position, mound.transform.position.y + 0.15f));
                        nextDrop = (nextDrop + 1) % drops.Length;
                    }
                }
                if (angle < -6f)
                {
                    hintTimer += Time.deltaTime;
                    if (hintTimer > 0.6f) { Say("Other way! The spout is on the LEFT.", 2f); hintTimer = -2f; }
                }
                mound.color = Color.Lerp(dry, soaked, wet);
                SetMeter(wet, new Color(0.35f, 0.62f, 0.95f));
                yield return null;
            }
            Sfx.Play(this, "ding", 0.6f);
            ShowMeter(false);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.4f) { can.localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(angle, 0f, k)); yield return null; }
            yield return Slide(can, can.position + Vector3.up * 5f, 0.4f);
            can.gameObject.SetActive(false);
            yield return hud.Banner("SPRING DONE!");
        }

        IEnumerator Drop(Transform d, Vector3 from, float floorY)
        {
            d.gameObject.SetActive(true);
            Vector3 p = from + new Vector3(Random.Range(-0.08f, 0.08f), 0f, 0f);
            float vy = -0.5f, vx = Random.Range(-0.3f, 0.1f);
            while (p.y > floorY)
            {
                vy -= 14f * Time.deltaTime;
                p += new Vector3(vx, vy, 0f) * Time.deltaTime;
                d.position = p;
                yield return null;
            }
            d.gameObject.SetActive(false);
        }

        IEnumerator Slide(Transform t, Vector3 to, float seconds)
        {
            Vector3 from = t.position;
            for (float k = 0f; k < 1f; k += Time.deltaTime / seconds) { t.position = Vector3.Lerp(from, to, 1f - (1f - k) * (1f - k)); yield return null; }
            t.position = to;
        }

        // ---------- 2. estate: nuvole e canicola ----------

        IEnumerator Summer2()
        {
            hud.SetStage(1);
            StartCoroutine(Tint(Summer, 1f));
            yield return Pop(sprout);
            // le nuvole coprono il sole
            Vector3[] cloudHome = new Vector3[clouds.Length];
            for (int i = 0; i < clouds.Length; i++)
            {
                cloudHome[i] = clouds[i].position;
                clouds[i].gameObject.SetActive(true);
                clouds[i].position = cloudHome[i] + Vector3.right * (i % 2 == 0 ? -4f : 4f);
            }
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.7f)
            {
                for (int i = 0; i < clouds.Length; i++) clouds[i].position = Vector3.Lerp(clouds[i].position, cloudHome[i], k);
                yield return null;
            }
            hud.Goal("SUMMER: swipe the clouds away from the sun!");
            Say("I need sunshine!", 1.8f);
            int left = clouds.Length;
            int grabbed = -1;
            Vector2 start = Vector2.zero;
            while (left > 0)
            {
                // le nuvole ondeggiano un po'
                for (int i = 0; i < clouds.Length; i++)
                    if (clouds[i].gameObject.activeSelf && i != grabbed) clouds[i].position = cloudHome[i] + Vector3.right * Mathf.Sin(clock * 1.3f + i) * 0.08f;
                if (Pointer(out Vector2 w, out bool began, out bool ended))
                {
                    if (began)
                    {
                        grabbed = -1;
                        for (int i = clouds.Length - 1; i >= 0; i--)
                        {
                            Collider2D c = clouds[i].GetComponent<Collider2D>();
                            if (clouds[i].gameObject.activeSelf && c != null && c.OverlapPoint(w)) { grabbed = i; start = w; break; }
                        }
                    }
                    if (grabbed >= 0)
                    {
                        Vector2 d = w - start;
                        clouds[grabbed].position = cloudHome[grabbed] + (Vector3)(d * 0.4f);
                        if (d.magnitude > 0.6f)
                        {
                            StartCoroutine(FlyAway(clouds[grabbed], d.normalized));
                            grabbed = -1;
                            left--;
                        }
                        else if (ended) grabbed = -1;
                    }
                }
                else grabbed = -1;
                yield return null;
            }
            // il sole splende, la pianta cresce
            if (sunGlow != null) StartCoroutine(Pop(sunGlow.transform, 2.6f, 0.5f));
            Sfx.Play(this, "ding", 0.7f);
            yield return new WaitForSeconds(0.4f);
            plant.localScale = Vector3.zero;
            plant.gameObject.SetActive(true);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.9f)
            {
                sprout.localScale = Vector3.one * (1f - k);
                plant.localScale = new Vector3(0.6f + 0.4f * k, k, 1f);
                yield return null;
            }
            sprout.gameObject.SetActive(false);
            plant.localScale = Vector3.one;
            Say("Ahh... growing!", 1.5f);
            yield return new WaitForSeconds(1.2f);

            // canicola
            hud.Goal("HEATWAVE! Keep its SHADE on the plant for 5 s");
            StartCoroutine(Tint(Heat, 0.8f));
            Sfx.Play(this, "whoosh", 0.6f);
            Vector3 sunHome = sun.position;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.5f) { sun.localScale = Vector3.one * (1f + k * 0.35f); if (sunGlow != null) sunGlow.color = Color.Lerp(new Color(1f, 0.95f, 0.6f, 0.6f), new Color(1f, 0.5f, 0.2f, 0.7f), k); yield return null; }
            umbrella.gameObject.SetActive(true);
            umbrella.position = umbrellaHome;
            shadow.gameObject.SetActive(true);
            Say("It's SO hot!! Umbrella!", 2f);
            ShowMeter(true, "SHADE 0/5 s");
            float shade = 0f, health = 1f, sunT = 0f;
            bool dragging = false;
            Vector2 offset = Vector2.zero;
            float plantX = plant.position.x, plantBase = plantY;
            while (shade < 5f)
            {
                // il sole attraversa il cielo avanti e indietro
                sunT += Time.deltaTime;
                float sx = Mathf.Sin(sunT * 0.55f) * 1.9f;
                sun.position = new Vector3(sunHome.x + (sx - sunHome.x) * Mathf.Min(1f, sunT), sunHome.y, sunHome.z);
                if (Pointer(out Vector2 w, out bool began, out bool ended))
                {
                    if (began && umbrellaArea.OverlapPoint(w)) { dragging = true; offset = (Vector2)umbrella.position - w; }
                    if (dragging)
                    {
                        Vector2 p = w + offset;
                        umbrella.position = new Vector3(Mathf.Clamp(p.x, -2.5f, 2.5f), Mathf.Clamp(p.y, -3.0f, 2.2f), umbrella.position.z);
                        if (ended) dragging = false;
                    }
                }
                else dragging = false;
                // ombra dalla parte opposta al sole, sul terreno della pianta
                float h = Mathf.Max(0f, umbrella.position.y - plantBase);
                float shadowX = umbrella.position.x - (sun.position.x - umbrella.position.x) * 0.22f * (0.4f + h * 0.3f);
                shadow.position = new Vector3(shadowX, plantBase, shadow.position.z);
                bool high = umbrella.position.y > plantBase + 1.2f;
                shadow.gameObject.SetActive(high);
                bool shaded = high && Mathf.Abs(shadowX - plantX) < 0.55f;
                if (shaded) { shade += Time.deltaTime; health = Mathf.Min(1f, health + Time.deltaTime * 0.6f); }
                else health -= Time.deltaTime / 3.5f;
                plantArt.color = Color.Lerp(new Color(0.75f, 0.6f, 0.25f), Color.white, health);
                plant.localRotation = Quaternion.Euler(0, 0, (1f - health) * 18f);
                plant.localScale = new Vector3(1f, 0.75f + 0.25f * health, 1f);
                SetMeter(shade / 5f, shaded ? new Color(0.36f, 0.8f, 0.42f) : new Color(1f, 0.55f, 0.25f));
                if (meterLabel != null) meterLabel.text = "SHADE " + Mathf.FloorToInt(shade) + "/5 s";
                if (health <= 0f)
                {
                    Say("It's wilting!! Watch where the SHADOW falls!", 2.4f);
                    if (!LoseLife()) { yield return Lost("Toasted..."); yield break; }
                    health = 1f; shade = 0f;
                    yield return new WaitForSeconds(0.6f);
                }
                yield return null;
            }
            Sfx.Play(this, "ding", 0.7f);
            ShowMeter(false);
            shadow.gameObject.SetActive(false);
            plantArt.color = Color.white;
            plant.localRotation = Quaternion.identity;
            plant.localScale = Vector3.one;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.5f) { sun.localScale = Vector3.one * (1.35f - k * 0.35f); sun.position = Vector3.Lerp(sun.position, sunHome, k); yield return null; }
            if (sunGlow != null) sunGlow.gameObject.SetActive(false);
            yield return Slide(umbrella, umbrellaHome + Vector3.down * 3f, 0.4f);
            umbrella.gameObject.SetActive(false);
            Say("Phew. Thanks!", 1.4f);
            yield return hud.Banner("SUMMER DONE!");
        }

        IEnumerator FlyAway(Transform t, Vector2 dir)
        {
            Sfx.Play(this, "whoosh", 0.6f);
            Vector3 from = t.position;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.45f) { t.position = from + (Vector3)(dir * k * 5f); yield return null; }
            t.gameObject.SetActive(false);
        }

        // ---------- 3. autunno: la talpa ladra ----------

        IEnumerator Autumn3()
        {
            hud.SetStage(2);
            StartCoroutine(Tint(Autumn, 1f));
            plantArt.color = new Color(1f, 0.85f, 0.6f);
            foreach (Transform h in holes) StartCoroutine(Pop(h));
            while (true)
            {
                for (int i = 0; i < pumpkins.Length; i++)
                {
                    pumpkins[i].position = pumpkinHome[i];
                    pumpkins[i].localScale = Vector3.zero;
                    StartCoroutine(Pop(pumpkins[i], 1f, 0.4f));
                    Sfx.Play(this, "pop", 0.5f);
                    yield return new WaitForSeconds(0.15f);
                }
                hud.Goal("AUTUMN: bonk the mole! Save 3 pumpkins");
                Say("Mmm, pumpkins...", 1.5f);
                yield return new WaitForSeconds(1.0f);
                // 10 apparizioni, due sono il gatto
                int[] seq = { 0, 2, 1, 0, 2, 1, 2, 0, 1, 2 };
                int catA = Random.Range(2, 5), catB = Random.Range(6, 9);
                bool catHit = false;
                for (int n = 0; n < seq.Length && !catHit; n++)
                {
                    float window = Mathf.Lerp(1.35f, 0.8f, n / (float)(seq.Length - 1));
                    bool isCat = n == catA || n == catB;
                    int hIdx = seq[n];
                    if (Random.value < 0.4f) hIdx = (hIdx + 1 + Random.Range(0, 2)) % holes.Length;
                    var result = new int[1];
                    yield return PopUp(isCat ? cat : mole, holes[hIdx], window, isCat, hIdx, result);
                    if (result[0] == 2) catHit = true;
                    yield return new WaitForSeconds(Random.Range(0.35f, 0.75f));
                    if (Saved() < 3) break;
                }
                int saved = Saved();
                if (!catHit && saved >= 3) break;
                Say(catHit ? "NOT THE CAT!! Start over." : "The mole got too many!", 2.2f);
                if (!LoseLife()) { yield return Lost("No harvest this year..."); yield break; }
                yield return new WaitForSeconds(1.2f);
                foreach (Transform p in pumpkins) p.gameObject.SetActive(false);
            }
            Say("Ha! Go dig somewhere else!", 1.8f);
            foreach (Transform h in holes) h.gameObject.SetActive(false);
            yield return hud.Banner("AUTUMN DONE!");
        }

        int Saved()
        {
            int n = 0;
            foreach (Transform p in pumpkins) if (p.gameObject.activeSelf) n++;
            return n;
        }

        /// <summary>Una talpa (o il gatto) sbuca dalla tana. result[0]: 0 = niente, 1 = talpa colpita, 2 = gatto colpito.</summary>
        IEnumerator PopUp(Transform who, Transform hole, float window, bool isCat, int holeIndex, int[] result)
        {
            Collider2D col = who.GetComponent<Collider2D>();
            Vector3 up = hole.position + new Vector3(0f, -0.08f, 0f), down = up + Vector3.down * 1.0f;
            who.position = down;
            who.localRotation = Quaternion.identity;
            who.gameObject.SetActive(true);
            Sfx.Play(this, "pop", 0.4f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.18f) { who.position = Vector3.Lerp(down, up, k); yield return null; }
            who.position = up;
            if (isCat) Say("Mrrp?", 1f);
            bool hit = false;
            for (float t = 0f; t < window && !hit; t += Time.deltaTime)
            {
                if (Pointer(out Vector2 w, out bool began, out bool _) && began && col != null && col.OverlapPoint(w)) hit = true;
                yield return null;
            }
            if (hit)
            {
                result[0] = isCat ? 2 : 1;
                Haptics.Light();
                Sfx.Play(this, "bonk", 0.9f);
                stars.position = up + Vector3.up * 0.95f;
                stars.gameObject.SetActive(true);
                for (float k = 0f; k < 1f; k += Time.deltaTime / 0.35f)
                {
                    who.localScale = new Vector3(1f + 0.25f * Mathf.Sin(k * Mathf.PI), 1f - 0.3f * Mathf.Sin(k * Mathf.PI), 1f);
                    stars.localRotation = Quaternion.Euler(0, 0, k * 360f);
                    yield return null;
                }
                who.localScale = Vector3.one;
                stars.gameObject.SetActive(false);
                if (isCat) { Sfx.Play(this, "woof", 0.5f); Say("HISSS!", 1f); }
            }
            else if (!isCat)
            {
                // la talpa si porta via una zucca vicina
                int a = holeIndex, b = holeIndex + 1, pick = -1;
                bool aOk = a < pumpkins.Length && pumpkins[a].gameObject.activeSelf, bOk = b < pumpkins.Length && pumpkins[b].gameObject.activeSelf;
                if (aOk && bOk) pick = Random.value < 0.5f ? a : b; else if (aOk) pick = a; else if (bOk) pick = b;
                if (pick >= 0)
                {
                    Sfx.Play(this, "whoosh", 0.5f);
                    Transform p = pumpkins[pick];
                    Vector3 from = p.position;
                    for (float k = 0f; k < 1f; k += Time.deltaTime / 0.3f) { p.position = Vector3.Lerp(from, up, k); p.localScale = Vector3.one * (1f - k * 0.7f); yield return null; }
                    p.gameObject.SetActive(false);
                    Say("Yoink!", 0.9f);
                }
            }
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.15f) { who.position = Vector3.Lerp(up, down, k); yield return null; }
            who.gameObject.SetActive(false);
        }

        // ---------- 4. inverno: la neve sulla serra ----------

        IEnumerator Winter4()
        {
            hud.SetStage(3);
            StartCoroutine(Tint(Winter, 1.2f));
            snowGround.gameObject.SetActive(true);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 1f) { snowGround.color = new Color(1f, 1f, 1f, k); yield return null; }
            // arriva la serra dall'alto
            greenhouse.gameObject.SetActive(true);
            greenhouse.position = greenhouseHome + Vector3.up * 6f;
            yield return Slide(greenhouse, greenhouseHome, 0.5f);
            Sfx.Play(this, "clunk", 0.7f);
            Haptics.Light();
            foreach (Transform f in flakes) { f.gameObject.SetActive(true); f.position = new Vector3(Random.Range(-2.8f, 2.8f), Random.Range(-4.5f, 4.5f), 0f); }
            const float storm = 14f;
            while (true)
            {
                hud.Goal("WINTER: shake the phone to clear the roof!");
                Say("Brrr... the roof is creaking!", 2f);
                ShowMeter(true, "SNOW ON ROOF");
                float load = 0.15f, t = 0f, creak = 0f;
                bool collapsed = false;
                while (t < storm)
                {
                    t += Time.deltaTime;
                    MoveFlakes();
                    float rate = Mathf.Lerp(1f / 7f, 1f / 4.5f, t / storm);
                    load += rate * Time.deltaTime;
                    float shake = Inputs.Shake();
                    if (shake > 5f) load -= Time.deltaTime * Mathf.Lerp(0.6f, 1.6f, Mathf.InverseLerp(5f, 20f, shake));
                    load = Mathf.Clamp01(load);
                    UpdateLumps(load);
                    SetMeter(load, load > 0.7f ? new Color(0.95f, 0.3f, 0.25f) : new Color(0.6f, 0.8f, 1f));
                    hud.GoalQuiet("Shake the phone! Storm ends in " + Mathf.CeilToInt(storm - t));
                    // scricchiola quando è quasi piena
                    if (load > 0.75f)
                    {
                        greenhouse.position = greenhouseHome + new Vector3(Mathf.Sin(clock * 40f) * 0.02f * load, 0f, 0f);
                        creak -= Time.deltaTime;
                        if (creak <= 0f) { creak = 1.2f; Sfx.Play(this, "clunk", 0.3f); Say("CREAK...", 0.8f); }
                    }
                    else greenhouse.position = greenhouseHome;
                    if (load >= 1f) { collapsed = true; break; }
                    yield return null;
                }
                greenhouse.position = greenhouseHome;
                if (!collapsed) break;
                // crollo!
                Haptics.Heavy();
                Sfx.Play(this, "clunk", 1f);
                Sfx.Play(this, "buzz", 0.5f);
                Say("CRASH! Shake harder next time!", 2.2f);
                for (float k = 0f; k < 1f; k += Time.deltaTime / 0.5f) { greenhouse.localScale = new Vector3(1f + k * 0.1f, 1f - k * 0.25f, 1f); yield return null; }
                if (!LoseLife()) { yield return Lost("Squashed pumpkins..."); yield break; }
                yield return new WaitForSeconds(1f);
                greenhouse.localScale = Vector3.one;
                UpdateLumps(0f);
            }
            ShowMeter(false);
            UpdateLumps(0f);
            foreach (Transform f in flakes) f.gameObject.SetActive(false);
            Say("The storm is over!", 1.6f);
            yield return hud.Banner("WINTER DONE!");
        }

        void MoveFlakes()
        {
            foreach (Transform f in flakes)
            {
                Vector3 p = f.position;
                p.y -= Time.deltaTime * 1.4f;
                p.x += Mathf.Sin(clock * 2f + p.y) * Time.deltaTime * 0.4f;
                if (p.y < -4.8f) { p.y = 3.6f; p.x = Random.Range(-2.8f, 2.8f); }
                f.position = p;
            }
        }

        void UpdateLumps(float load)
        {
            int n = Mathf.CeilToInt(load * lumps.Length - 0.01f);
            for (int i = 0; i < lumps.Length; i++)
            {
                if (lumpFalling[i]) continue;
                bool want = i < n;
                if (want && !lumps[i].gameObject.activeSelf)
                {
                    lumps[i].localPosition = lumpHome[i];
                    StartCoroutine(Pop(lumps[i], 1f, 0.2f));
                }
                else if (!want && lumps[i].gameObject.activeSelf) StartCoroutine(LumpFall(i));
            }
        }

        IEnumerator LumpFall(int i)
        {
            lumpFalling[i] = true;
            Transform l = lumps[i];
            float side = lumpHome[i].x < 0f ? -1f : 1f;
            Vector3 p = l.position;
            float vx = side * 1.5f, vy = 1f;
            for (float t = 0f; t < 0.7f; t += Time.deltaTime)
            {
                vy -= 12f * Time.deltaTime;
                p += new Vector3(vx, vy, 0f) * Time.deltaTime;
                l.position = p;
                yield return null;
            }
            l.gameObject.SetActive(false);
            l.localPosition = lumpHome[i];
            lumpFalling[i] = false;
        }

        // ---------- festa del raccolto ----------

        IEnumerator Party()
        {
            hud.SetStage(4);
            hud.Goal("HARVEST PARTY!");
            StartCoroutine(Tint(Spring, 1f));
            yield return Slide(greenhouse, greenhouseHome + Vector3.up * 7f, 0.6f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.6f) { snowGround.color = new Color(1f, 1f, 1f, 1f - k); yield return null; }
            snowGround.gameObject.SetActive(false);
            plantArt.color = Color.white;
            Sfx.Play(this, "ding", 0.9f);
            Haptics.Light();
            Vector3[] cv = new Vector3[confetti.Length];
            for (int i = 0; i < confetti.Length; i++)
            {
                confetti[i].gameObject.SetActive(true);
                confetti[i].position = new Vector3(0f, -1.0f, 0f);
                cv[i] = new Vector3(Random.Range(-3f, 3f), Random.Range(4f, 8f), 0f);
            }
            Say("Harvest party! Pumpkin pie for everyone!", 3f);
            for (float t = 0f; t < 2.6f; t += Time.deltaTime)
            {
                for (int i = 0; i < confetti.Length; i++)
                {
                    cv[i].y -= 9f * Time.deltaTime;
                    confetti[i].position += cv[i] * Time.deltaTime;
                    confetti[i].localRotation = Quaternion.Euler(0, 0, t * 400f + i * 40f);
                }
                for (int i = 0; i < pumpkins.Length; i++)
                    if (pumpkins[i].gameObject.activeSelf) pumpkins[i].position = pumpkinHome[i] + Vector3.up * Mathf.Abs(Mathf.Sin(t * 6f + i)) * 0.3f;
                yield return null;
            }
            Win();
        }
    }
}
