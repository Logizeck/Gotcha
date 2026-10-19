using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "Museum night" — furto al museo in tre fasi. 1) Trascina il gatto ladro lungo il pavimento fino al
    /// quadro senza farti illuminare dalla torcia del guardiano che fa la ronda: nasconditi dietro i piedistalli
    /// delle statue o passagli alle spalle. 2) Sotto il quadro: il guardiano fissa il dipinto, ma quando sbadiglia
    /// (preavviso) si gira per 3 secondi: trascina il falso (un disegno a pastelli!) dal vassoio sulla cornice, il vero
    /// finisce nel sacco. 3) All'uscita la griglia laser lampeggia a ritmo: tieni premuto per far avanzare il gatto
    /// acquattato e lascia prima che un laser si riaccenda (tremola prima di accendersi). Ogni volta che vieni
    /// scoperto suona la sirena e perdi uno dei 3 tentativi; finiti i tentativi il livello è perso.
    /// </summary>
    public class MuseumLevel : LevelController
    {
        [SerializeField] StageHud40D hud;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] SpriteRenderer[] lives;
        [SerializeField] Transform cat;
        [SerializeField] Transform catArt;            // si gira con la scala x
        [SerializeField] Collider2D catArea;
        [SerializeField] Transform guard;
        [SerializeField] Transform guardArt;          // si gira con la scala x (cono compreso)
        [SerializeField] Transform coneApex;          // vertice del cono (figlio di guardArt)
        [SerializeField] SpriteRenderer cone;
        [SerializeField] Transform[] statues;
        [SerializeField] SpriteRenderer siren;
        [SerializeField] Transform painting;          // il quadro vero sul muro
        [SerializeField] Transform fake;              // il falso nel vassoio
        [SerializeField] Collider2D fakeArea;
        [SerializeField] Transform bag;               // sacco nel vassoio
        [SerializeField] Transform tray;
        [SerializeField] TextMesh countdown;
        [SerializeField] Transform[] lasers;
        [SerializeField] Transform zzz;
        [SerializeField] Transform exitPoint;
        [SerializeField] float coneLength = 2.43f, coneMinDeg = -41f, coneMaxDeg = -3f;
        [SerializeField] float patrolMin = -1.4f, patrolMax = 1.3f, patrolSpeed = 0.9f, catSpeed = 2.6f;

        Camera cam;
        int triesLeft = 3;
        float bubbleUntil, clock;
        Vector3 catStart, fakeHome, paintingHome;
        float guardFacing = 1f;
        SpriteRenderer[] laserArts;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            catStart = cat.position;
            fakeHome = fake.position;
            paintingHome = painting.position;
            laserArts = new SpriteRenderer[lasers.Length];
            for (int i = 0; i < lasers.Length; i++) { laserArts[i] = lasers[i].GetComponentInChildren<SpriteRenderer>(); lasers[i].gameObject.SetActive(false); }
            siren.color = new Color(1f, 0f, 0f, 0f);
            tray.gameObject.SetActive(false);
            countdown.gameObject.SetActive(false);
            zzz.gameObject.SetActive(false);
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

        bool Pointer(out Vector2 w, out bool began, out bool ended)
        {
            w = Vector2.zero;
            if (!PointerInput.TryGetPrimary(out Vector2 sp, out began, out ended)) return false;
            if (began && PointerInput.IsOverUi()) { began = false; return false; }
            w = PointerInput.ToWorld(cam, sp);
            return true;
        }

        void FaceGuard(float dir)
        {
            guardFacing = dir;
            Vector3 s = guardArt.localScale;
            guardArt.localScale = new Vector3(Mathf.Abs(s.x) * dir, s.y, s.z);
        }

        void FaceCat(float dir)
        {
            Vector3 s = catArt.localScale;
            catArt.localScale = new Vector3(Mathf.Abs(s.x) * dir, s.y, s.z);
        }

        /// <summary>Il gatto è dentro il cono di luce (e non è dietro una statua)?</summary>
        bool Spotted()
        {
            if (!cone.gameObject.activeInHierarchy) return false;
            foreach (Transform s in statues) if (Mathf.Abs(cat.position.x - s.position.x) < 0.32f) return false;
            Vector2 apex = coneApex.position;
            foreach (float h in new[] { 0.35f, 0.75f })
            {
                Vector2 r = (Vector2)cat.position + new Vector2(0f, h) - apex;
                r.x *= guardFacing;
                if (r.magnitude > coneLength || r.x <= 0f) continue;
                float a = Mathf.Atan2(r.y, r.x) * Mathf.Rad2Deg;
                if (a >= coneMinDeg && a <= coneMaxDeg) return true;
            }
            return false;
        }

        /// <summary>Sirena e un tentativo in meno; false se erano finiti.</summary>
        IEnumerator Caught(string line, bool[] alive)
        {
            Haptics.Medium();
            Sfx.Play(this, "buzz", 1f);
            Say(line, 1.8f);
            triesLeft--;
            if (triesLeft >= 0 && triesLeft < lives.Length) lives[triesLeft].gameObject.SetActive(false);
            for (float t = 0f; t < 1.2f; t += Time.deltaTime)
            {
                siren.color = new Color(1f, 0f, 0f, 0.15f + 0.25f * Mathf.Abs(Mathf.Sin(t * 10f)));
                if (Mathf.Repeat(t, 0.4f) < Time.deltaTime) Sfx.Play(this, "buzz", 0.6f);
                yield return null;
            }
            siren.color = new Color(1f, 0f, 0f, 0f);
            alive[0] = triesLeft > 0;
            if (!alive[0])
            {
                hud.Goal("BUSTED!");
                Say("Busted! Off to cat jail.", 2.5f);
                yield return new WaitForSeconds(2f);
                Fail();
            }
        }

        IEnumerator Run()
        {
            yield return Sneak1();
            if (IsFinished) yield break;
            yield return Swap2();
            if (IsFinished) yield break;
            yield return Lasers3();
        }

        // ---------- 1. ronda del guardiano ----------

        IEnumerator Sneak1()
        {
            hud.SetStage(0);
            hud.Goal("Drag the cat to the painting. Stay out of the light!");
            Say("Psst. Stay in the shadows...", 2f);
            FaceGuard(1f);
            FaceCat(1f);
            bool dragging = false;
            float pause = 0f, grace = 0.5f;
            var alive = new bool[1];
            while (cat.position.x < paintingHome.x - 0.05f)
            {
                // ronda: avanti e indietro con una pausa ai lati
                if (pause > 0f) pause -= Time.deltaTime;
                else
                {
                    Vector3 g = guard.position;
                    g.x += guardFacing * patrolSpeed * Time.deltaTime;
                    if (g.x > patrolMax) { g.x = patrolMax; pause = 0.9f; FaceGuard(-1f); }
                    if (g.x < patrolMin) { g.x = patrolMin; pause = 0.9f; FaceGuard(1f); }
                    guard.position = g;
                }
                // gatto trascinato lungo il pavimento (con velocità massima)
                if (Pointer(out Vector2 w, out bool began, out bool ended))
                {
                    if (began && catArea.OverlapPoint(w)) dragging = true;
                    if (dragging)
                    {
                        float dx = Mathf.Clamp(w.x - cat.position.x, -catSpeed * Time.deltaTime, catSpeed * Time.deltaTime);
                        if (Mathf.Abs(dx) > 0.001f) FaceCat(Mathf.Sign(dx));
                        float nx = Mathf.Clamp(cat.position.x + dx, catStart.x, paintingHome.x);
                        cat.position = new Vector3(nx, catStart.y + Mathf.Abs(Mathf.Sin(nx * 6f)) * 0.04f, cat.position.z);
                        if (ended) dragging = false;
                    }
                }
                else dragging = false;
                grace -= Time.deltaTime;
                if (grace <= 0f && Spotted())
                {
                    dragging = false;
                    yield return Caught("HEY! Who's there?!", alive);
                    if (!alive[0]) yield break;
                    yield return Run2(cat, catStart, 0.5f);
                    FaceCat(1f);
                    grace = 1.2f;
                }
                yield return null;
            }
            Sfx.Play(this, "ding", 0.6f);
            yield return hud.Banner("MADE IT!");
        }

        IEnumerator Run2(Transform t, Vector3 to, float seconds)
        {
            Vector3 from = t.position;
            for (float k = 0f; k < 1f; k += Time.deltaTime / seconds) { t.position = Vector3.Lerp(from, to, k) + Vector3.up * Mathf.Abs(Mathf.Sin(k * 18f)) * 0.06f; yield return null; }
            t.position = to;
        }

        // ---------- 2. lo scambio del quadro ----------

        IEnumerator Swap2()
        {
            hud.SetStage(1);
            // il guardiano si piazza a fissare il quadro
            Vector3 watch = new Vector3(-0.4f, guard.position.y, guard.position.z);
            FaceGuard(watch.x > guard.position.x ? 1f : -1f);
            yield return Run2(guard, watch, 0.8f);
            FaceGuard(1f);
            cone.gameObject.SetActive(true);
            tray.gameObject.SetActive(true);
            hud.Goal("When he looks away: drag the FAKE onto the frame!");
            Say("Wait for his yawn...", 2.2f);
            bool swapped = false;
            var alive = new bool[1];
            bool dragging = false;
            Vector2 offset = Vector2.zero;
            float phase = 0f, watchFor = 2.6f;
            int state = 0;   // 0 guarda, 1 sbadiglia (preavviso), 2 girato
            while (!swapped)
            {
                phase += Time.deltaTime;
                if (state == 0 && phase > watchFor)
                {
                    state = 1; phase = 0f;
                    Say("*yaaawn*", 1f);
                    Sfx.Play(this, "snore", 0.5f);
                }
                else if (state == 1)
                {
                    guardArt.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(phase / 1f * Mathf.PI) * 6f);
                    if (phase > 1f) { state = 2; phase = 0f; guardArt.localRotation = Quaternion.identity; FaceGuard(-1f); countdown.gameObject.SetActive(true); Sfx.Play(this, "tick", 0.6f); }
                }
                else if (state == 2)
                {
                    int left = Mathf.CeilToInt(3f - phase);
                    countdown.text = left.ToString();
                    if (phase > 3f)
                    {
                        state = 0; phase = 0f; watchFor = Random.Range(2.0f, 3.2f);
                        countdown.gameObject.SetActive(false);
                        FaceGuard(1f);
                        Sfx.Play(this, "clunk", 0.4f);
                        if (dragging)
                        {
                            dragging = false;
                            yield return Caught("What's THAT in your paws?!", alive);
                            if (!alive[0]) yield break;
                            yield return Run2(fake, fakeHome, 0.3f);
                            fake.localScale = Vector3.one * 0.4f;
                        }
                    }
                }

                if (Pointer(out Vector2 w, out bool began, out bool ended))
                {
                    if (began && FakeHit(w)) { dragging = true; offset = (Vector2)fake.position - w; Sfx.Play(this, "tick", 0.6f); }
                    if (dragging)
                    {
                        Vector2 p = w + offset;
                        fake.position = new Vector3(Mathf.Clamp(p.x, -2.4f, 2.4f), Mathf.Clamp(p.y, -4.3f, 1.6f), fake.position.z);
                        float near = Mathf.InverseLerp(4f, 0.5f, Vector2.Distance(fake.position, paintingHome));
                        fake.localScale = Vector3.one * Mathf.Lerp(0.4f, 0.75f, near);
                        if (ended)
                        {
                            dragging = false;
                            if (Vector2.Distance(fake.position, paintingHome) < 0.9f)
                            {
                                if (state == 2) swapped = true;
                                else
                                {
                                    yield return Caught("HEY! Hands off the art!", alive);
                                    if (!alive[0]) yield break;
                                    yield return Run2(fake, fakeHome, 0.3f);
                                    fake.localScale = Vector3.one * 0.4f;
                                }
                            }
                            else { yield return Run2(fake, fakeHome, 0.25f); fake.localScale = Vector3.one * 0.4f; }
                        }
                    }
                }
                else if (dragging) { dragging = false; yield return Run2(fake, fakeHome, 0.25f); fake.localScale = Vector3.one * 0.4f; }
                yield return null;
            }
            // scambio: il falso sulla cornice, il vero nel sacco
            Sfx.Play(this, "whoosh", 0.7f);
            Vector3 realFrom = painting.position, f0 = fake.position;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.35f)
            {
                fake.position = Vector3.Lerp(f0, paintingHome, k);
                fake.localScale = Vector3.one * 0.75f;
                painting.position = Vector3.Lerp(realFrom, bag.position, k);
                painting.localScale = Vector3.one * Mathf.Lerp(0.75f, 0.15f, k);
                yield return null;
            }
            painting.gameObject.SetActive(false);
            fake.SetParent(transform, true);   // ora fa parte del muro, non del vassoio
            foreach (SpriteRenderer sr in fake.GetComponentsInChildren<SpriteRenderer>()) sr.sortingOrder = -10;
            fake.position = paintingHome;
            Haptics.Light();
            Sfx.Play(this, "clunk", 0.5f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.3f) { bag.localScale = Vector3.one * (1f + Mathf.Sin(k * Mathf.PI) * 0.3f); yield return null; }
            // aspetta che il guardiano si rigiri... e non si accorge di niente
            while (state == 2 && phase < 3f) { phase += Time.deltaTime; countdown.text = Mathf.CeilToInt(3f - phase).ToString(); yield return null; }
            countdown.gameObject.SetActive(false);
            FaceGuard(1f);
            yield return new WaitForSeconds(0.3f);
            Say("...Hm. Lovely brushwork.", 1.8f);
            yield return new WaitForSeconds(1.6f);
            yield return hud.Banner("SWAPPED!");
        }

        bool FakeHit(Vector2 w) => fakeArea != null && fakeArea.OverlapPoint(w);

        // ---------- 3. la griglia laser ----------

        bool LaserOn(int i, float t, out float alpha)
        {
            // periodo 2.6 s: acceso 1.5, spento 1.1; tremola 0.4 s prima di riaccendersi
            float ph = Mathf.Repeat(t + i * 0.85f, 2.6f);
            if (ph < 1.5f) { alpha = 1f; return true; }
            if (ph > 2.2f) { alpha = (Mathf.Repeat(t * 14f, 1f) < 0.5f) ? 0.45f : 0.1f; return false; }
            alpha = 0f;
            return false;
        }

        IEnumerator Lasers3()
        {
            hud.SetStage(2);
            // il guardiano si addormenta
            cone.gameObject.SetActive(false);
            zzz.gameObject.SetActive(true);
            guardArt.localRotation = Quaternion.Euler(0, 0, 6f);
            Sfx.Play(this, "snore", 0.6f);
            tray.gameObject.SetActive(false);
            foreach (Transform l in lasers) l.gameObject.SetActive(true);
            FaceCat(-1f);
            hud.Goal("HOLD to sneak to the exit. Let go when a laser blinks!");
            Say("Lasers! Wait for them to switch off...", 2.4f);
            Vector3 start = cat.position;
            float t = 0f;
            var alive = new bool[1];
            while (cat.position.x > exitPoint.position.x)
            {
                t += Time.deltaTime;
                bool[] on = new bool[lasers.Length];
                for (int i = 0; i < lasers.Length; i++)
                {
                    on[i] = LaserOn(i, t, out float a);
                    if (laserArts[i] != null) laserArts[i].color = new Color(1f, 1f, 1f, a);
                }
                bool holding = Pointer(out _, out _, out bool ended) && !ended;
                if (holding)
                {
                    Vector3 p = cat.position;
                    p.x -= 1.3f * Time.deltaTime;
                    p.y = start.y + Mathf.Abs(Mathf.Sin(p.x * 7f)) * 0.03f;
                    cat.position = p;
                    catArt.localScale = new Vector3(catArt.localScale.x, Mathf.Abs(catArt.localScale.x) * 0.85f, 1f);
                }
                else catArt.localScale = new Vector3(catArt.localScale.x, Mathf.Abs(catArt.localScale.x), 1f);
                for (int i = 0; i < lasers.Length; i++)
                {
                    if (!on[i] || Mathf.Abs(cat.position.x - lasers[i].position.x) > 0.3f) continue;
                    Haptics.Medium();
                    yield return Caught("ALARM! Laser tripped!", alive);
                    if (!alive[0]) yield break;
                    cat.position = start;
                    t = 0f;
                    break;
                }
                yield return null;
            }
            // fuori dalla porta
            foreach (Transform l in lasers) l.gameObject.SetActive(false);
            catArt.localScale = new Vector3(catArt.localScale.x, Mathf.Abs(catArt.localScale.x), 1f);
            SpriteRenderer cs = catArt.GetComponentInChildren<SpriteRenderer>();
            Vector3 c0 = cat.position;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.6f)
            {
                cat.position = c0 + Vector3.left * k * 0.4f;
                if (cs != null) cs.color = new Color(1f, 1f, 1f, 1f - k);
                yield return null;
            }
            hud.SetStage(3);
            hud.Goal("The perfect crime!");
            Sfx.Play(this, "ding", 0.9f);
            zzz.gameObject.SetActive(false);
            guardArt.localRotation = Quaternion.identity;
            FaceGuard(1f);
            Say("*snort* ...What a lovely painting.", 2.4f);
            yield return new WaitForSeconds(2.6f);
            Win();
        }
    }
}
