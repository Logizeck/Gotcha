using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels.V64;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 24 della roadmap "The Lazy Sun" (id lazy_sun) — "MAKE THE SUN RISE." Paesaggio di notte: cielo stellato,
    /// la luna in alto a sinistra, una grande collina tonda e, davanti, un prato con lo steccato su cui dorme un gallo.
    /// Da dietro la cima della collina salgono dei "Zzz": lì dietro dorme il sole, col berretto da notte.
    /// Inganno: toccare cielo (le stelle brillano, "The sky ignores you."), luna ("Not my shift."), gallo (apre un occhio:
    /// "Too early."), collina o prato non serve; aspettare nemmeno (ogni 10 s di quiete un russare più forte, "ZZZ...",
    /// mai una vittoria); il sole, dove si vede, non si trascina ("It's too heavy to lift. And asleep."). Tutto reversibile.
    /// Soluzione: trascinare GIÙ la collina (presa generosa lungo tutta la parte alta, anche un po' sopra il bordo). Si
    /// muove solo in verticale, limitata tra la posizione iniziale e la profondità massima (in su oltre l'inizio non va).
    /// Scelta documentata: lasciata sotto la soglia (75% della corsa) RESTA DOV'È (si può riprendere; se il sole si vede
    /// borbotta "Five more minutes..." e continua a russare). Lasciata oltre la soglia -> la collina scivola in fondo, il
    /// sole apre gli occhi e sorride, il berretto vola via, il cielo diventa l'alba, il gallo canta; LevelReward
    /// "YOU STARTED A NEW DAY!" -> Win una volta sola. La vittoria dipende solo dall'orizzonte, mai dal sole.
    /// Il sole sta DIETRO la collina (ordine di disegno) e la collina è larga quanto ogni schermo: all'inizio non si vede.
    /// Un dito alla volta (HalloweenKit.ReadPointer); due dita, pausa, app in background o livello spento mollano la presa
    /// (la collina resta dov'è, niente vittoria). La logica è in Gotcha.Levels.V64.LazySunLogic.
    /// Tasti di prova nell'Editor: T tocco sul cielo, M luna, K gallo, H tocco sulla collina, G prato, S trascinare il
    /// sole, L collina abbassata a metà e lasciata, U collina tirata in su (limitata), W collina abbassata del tutto e
    /// lasciata (soluzione), P pausa simulata (presa annullata), R ricomincia.
    /// </summary>
    public class V79LazySunLevel : LevelController
    {
        [Header("Cielo")]
        [SerializeField] SpriteRenderer skyDawn;          // cielo dell'alba sopra quello di notte (trasparente all'inizio)
        [SerializeField] Transform[] stars;               // stelline che brillano (spariscono all'alba)
        [SerializeField] Transform moon;
        [SerializeField] Collider2D moonArea;
        [Header("Sole (dietro la collina)")]
        [SerializeField] Transform sun;                   // centro del sole
        [SerializeField] Transform sunBody;               // disco coi raggi (figlio del sole)
        [SerializeField] Transform sunFaceSleep;
        [SerializeField] Transform sunFaceAwake;
        [SerializeField] Transform nightcap;              // figlio del sole
        [SerializeField] SpriteRenderer sunGlow;
        [SerializeField] float sunRadius = 1.05f;
        [SerializeField] Transform[] zzz;                 // "Zzz" che salgono da dietro la collina
        [Header("Collina (si trascina in giù)")]
        [SerializeField] Transform hill;                  // origine sulla cima
        [SerializeField] float hillTravel = 3.1f;         // corsa massima verso il basso
        [SerializeField] float hillHalfWidth = 4.9f;
        [SerializeField] float hillHeight = 4.6f;
        [SerializeField] float meadowTop = -3.3f;         // sotto c'è il prato davanti
        [SerializeField] SpriteRenderer[] landTint;       // collina, prato, steccato: blu di notte
        [SerializeField] SpriteRenderer[] softTint;       // il gallo: appena un po' blu di notte
        [Header("Gallo")]
        [SerializeField] Transform rooster;
        [SerializeField] Collider2D roosterArea;
        [SerializeField] Transform roosterSleep;
        [SerializeField] Transform roosterPeek;
        [SerializeField] Transform roosterCrow;
        [Header("Fumetti, scritte e festa")]
        [SerializeField] SpeechBubble moonBubble;
        [SerializeField] SpeechBubble roosterBubble;
        [SerializeField] SpeechBubble sunBubble;
        [SerializeField] TextMesh infoText;
        [SerializeField] Transform infoHolder;
        [SerializeField] LevelReward reward;

        const string Compliment = "YOU STARTED A NEW DAY!";
        const float TapSlop = 0.15f, Follow = 14f, GrabAbove = 0.45f, GrabBelow = 2.6f;
        const float SnoreEvery = 1.9f, ZzzLife = 3.2f, ZzzSpeed = 0.6f, PeekSeconds = 1.4f, SunRise = 0.45f;
        static readonly Color NightLand = new Color(0.42f, 0.48f, 0.78f), DayLand = new Color(1f, 0.97f, 0.92f);
        static readonly Color NightSoft = new Color(0.72f, 0.75f, 0.92f), StarColor = new Color(1f, 0.96f, 0.75f);

        static readonly string[] SkyLines = { "The sky ignores you.", "The stars twinkle. That's all.", "Still night. Very night." };
        static readonly string[] MoonLines = { "Not my shift.", "I only do nights.", "Day shift is late." };
        static readonly string[] RoosterLines = { "Too early.", "No sun, no song.", "Wake me when it's bright." };
        static readonly string[] HillLines = { "Just a big hill.", "Is the hill... snoring?", "A very sleepy horizon." };
        static readonly string[] GroundLines = { "Sleepy grass.", "Nothing here but grass." };
        static readonly string[] SunTapLines = { "Zzz... snore...", "Mmh... go away...", "Zzz... not today..." };
        static readonly string[] SunMumbles = { "Five more minutes...", "Mmh... too early...", "Zzz... shh..." };
        static readonly string[] HillStayLines = { "The hill stays there.", "The hill sinks a bit.", "Still a big hill." };
        static readonly string[] BigSnoreLines = { "ZZZ...", "ZZZZZ... SNRK!", "ZZZ... (still asleep)" };
        const string TooHeavy = "It's too heavy to lift. And asleep.";
        const string SunHello = "Good morning!", Crow = "COCK-A- DOODLE-DOO!";

        enum Kind { None, Hill, Sun, Moon, Rooster, Sky, Ground }

        sealed class Grip
        {
            public Kind kind;
            public Vector2 start;
            public float grabLowered;
            public bool moved, held;
        }

        sealed class FxSlot
        {
            public Coroutine co;
            public Vector3 v;
            public Quaternion q;
        }

        LazySunLogic logic;
        Camera cam;
        HwToast toast;
        HwSay moonSay, roosterSay, sunSay;
        Grip grip;
        readonly Dictionary<Transform, FxSlot>[] fxSlots = { new Dictionary<Transform, FxSlot>(), new Dictionary<Transform, FxSlot>(), new Dictionary<Transform, FxSlot>() };
        SpriteRenderer[] zzzArt, starArt;
        SpriteRenderer[] moonArt, capArt;
        float[] zzzAge, zzzBoost;
        Vector3[] starScales;
        Vector3 hillHome, sunHome, moonHome, capPos, capScale, zzzScale, faceScale, bodyScale;
        Quaternion capRot;
        float snoreT, peekT, burstT, rumbleT, lastScrape = -1f, lastScrapeLowered;
        int nextZzz, snores, skyLines, moonLines, roosterLines, hillLines, groundLines, sunTapLines, sunMumbles, hillStayLines, bigSnoreLines;
        bool captured, finishing;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            StopAllCoroutines();
            StopFx();
            if (!captured)
            {
                captured = true;
                logic = new LazySunLogic();
                hillHome = hill.position;
                sunHome = sun.position;
                moonHome = moon.position;
                capPos = nightcap.localPosition;
                capRot = nightcap.localRotation;
                capScale = nightcap.localScale;
                faceScale = sunFaceSleep.localScale;
                bodyScale = sunBody.localScale;
                moonArt = moon.GetComponentsInChildren<SpriteRenderer>(true);
                capArt = nightcap.GetComponentsInChildren<SpriteRenderer>(true);
                int nz = zzz != null ? zzz.Length : 0;
                zzzArt = new SpriteRenderer[nz];
                zzzAge = new float[nz];
                zzzBoost = new float[nz];
                for (int i = 0; i < nz; i++) zzzArt[i] = zzz[i] != null ? zzz[i].GetComponentInChildren<SpriteRenderer>(true) : null;
                zzzScale = nz > 0 && zzz[0] != null ? zzz[0].localScale : Vector3.one;
                int ns = stars != null ? stars.Length : 0;
                starArt = new SpriteRenderer[ns];
                starScales = new Vector3[ns];
                for (int i = 0; i < ns; i++)
                {
                    starArt[i] = stars[i] != null ? stars[i].GetComponentInChildren<SpriteRenderer>(true) : null;
                    starScales[i] = stars[i] != null ? stars[i].localScale : Vector3.one;
                }
            }
            logic.Reset();
            if (reward != null) reward.ResetReward();
            grip = null;
            finishing = false;
            snoreT = 0.6f;
            peekT = burstT = rumbleT = 0f;
            nextZzz = snores = 0;
            skyLines = moonLines = roosterLines = hillLines = groundLines = sunTapLines = sunMumbles = hillStayLines = bigSnoreLines = 0;
            toast = new HwToast(infoText, infoHolder);
            moonSay = new HwSay(moonBubble);
            roosterSay = new HwSay(roosterBubble);
            sunSay = new HwSay(sunBubble);
            RestPose();
        }

        // ---------- Pose (sempre ricavate dalla logica: uno stato valido dopo pausa/spegnimento) ----------

        void RestPose()
        {
            moonSay?.Hide();
            roosterSay?.Hide();
            sunSay?.Hide();
            hill.position = HillTarget();
            sun.position = sunHome;
            sun.localRotation = Quaternion.identity;
            sunBody.localScale = bodyScale;
            sunBody.localRotation = Quaternion.identity;
            sunFaceSleep.localScale = faceScale;
            sunFaceAwake.localScale = faceScale;
            Face(false);
            nightcap.localPosition = capPos;
            nightcap.localRotation = capRot;
            nightcap.localScale = capScale;
            SetAlpha(capArt, 1f);
            HalloweenKit.Show(nightcap, true);
            moon.position = moonHome;
            moon.localRotation = Quaternion.identity;
            if (rooster != null) rooster.localRotation = Quaternion.identity;
            RoosterPose(0);
            for (int i = 0; i < zzzAge.Length; i++) { zzzAge[i] = -1f; HalloweenKit.Show(zzz[i], false); }
            for (int i = 0; i < starScales.Length; i++) if (stars[i] != null) stars[i].localScale = starScales[i];
            Dawn(0f);
        }

        Vector3 HillTarget() => HillAt(logic.Lowered);

        Vector3 HillAt(float lowered) => hillHome - new Vector3(0f, lowered * hillTravel, 0f);

        /// <summary>Altezza (mondo) del bordo della collina sopra il punto x.</summary>
        float EdgeY(float x) => hill.position.y - LazySunLogic.EdgeDrop(x - hill.position.x, hillHalfWidth, hillHeight);

        void Face(bool awake)
        {
            HalloweenKit.Show(sunFaceSleep, !awake);
            HalloweenKit.Show(sunFaceAwake, awake);
        }

        /// <summary>0 dorme, 1 un occhio aperto, 2 canta.</summary>
        void RoosterPose(int k)
        {
            HalloweenKit.Show(roosterSleep, k == 0);
            HalloweenKit.Show(roosterPeek, k == 1);
            HalloweenKit.Show(roosterCrow, k == 2);
        }

        /// <summary>Notte (0) -> alba (1): cielo, stelle, luna, bagliore del sole, colori della terra.</summary>
        void Dawn(float k)
        {
            if (skyDawn != null) skyDawn.color = new Color(1f, 1f, 1f, k);
            if (starArt != null) foreach (SpriteRenderer s in starArt) if (s != null) s.color = new Color(StarColor.r, StarColor.g, StarColor.b, 1f - k);
            SetAlpha(moonArt, 1f - k);
            if (sunGlow != null) sunGlow.color = new Color(1f, 0.85f, 0.45f, 0.75f * k);
            if (landTint != null) foreach (SpriteRenderer s in landTint) if (s != null) s.color = Color.Lerp(NightLand, DayLand, k);
            if (softTint != null) foreach (SpriteRenderer s in softTint) if (s != null) s.color = Color.Lerp(NightSoft, Color.white, k);
        }

        static void SetAlpha(SpriteRenderer[] rs, float a)
        {
            if (rs == null) return;
            foreach (SpriteRenderer s in rs) if (s != null) { Color c = s.color; c.a = a; s.color = c; }
        }

        // ---------- Ciclo di vita ----------

        void OnDisable()
        {
            // pausa/pubblicità/livello spento: si molla la presa, si torna a uno stato valido
            CancelGrip();
            StopFx();
            if (captured && !finishing && !IsFinished && logic != null) RestPose();
        }

        void OnEnable()
        {
            if (!captured || IsFinished || logic == null) return;
            // spento a metà della festa: la si rifà da capo (la vittoria resta una sola)
            if (logic.Stage == LazySunStage.Celebrating) { finishing = false; StartCoroutine(Finale()); }
        }

        void OnApplicationPause(bool paused) { if (paused) CancelGrip(); }
        void OnApplicationFocus(bool focus) { if (!focus) CancelGrip(); }

        /// <summary>Il dito sparisce: la collina resta dov'è (nessuna vittoria senza un rilascio vero), niente battute.</summary>
        void CancelGrip()
        {
            grip = null;
            if (logic != null && !logic.InputLocked) logic.Cancel();
        }

        void Update()
        {
            toast?.Tick();
            moonSay?.Tick();
            roosterSay?.Tick();
            sunSay?.Tick();
            if (IsFinished || toast == null || logic == null) return;
            if (!finishing)
            {
                if (Time.timeScale <= 0f) { CancelGrip(); return; }   // pausa o pubblicità: niente input
                if (logic.Tick(Time.deltaTime)) BigSnore();
#if UNITY_EDITOR
                EditorKeys();
#endif
                if (!finishing) ReadPointer();
                if (!finishing) Pose();
            }
            Ambient(Time.deltaTime);
        }

        /// <summary>La collina scivola verso la posizione voluta dalla logica (se non la tiene il dito); un tremolio se il sole russa forte.</summary>
        void Pose()
        {
            rumbleT = Mathf.Max(0f, rumbleT - Time.deltaTime);
            Vector3 target = HillTarget() + new Vector3(rumbleT > 0f ? Mathf.Sin(Time.time * 70f) * 0.035f * Mathf.Clamp01(rumbleT * 3f) : 0f, 0f, 0f);
            if (logic.Holding) hill.position = new Vector3(target.x, hill.position.y, target.z);
            else hill.position = Vector3.Lerp(hill.position, target, 1f - Mathf.Exp(-Time.deltaTime * Follow));
            // il gallo riapre l'occhio per poco
            if (peekT > 0f)
            {
                peekT -= Time.deltaTime;
                if (peekT <= 0f) RoosterPose(0);
            }
        }

        /// <summary>Stelle che brillano, "Zzz" che salgono (solo finché il sole dorme).</summary>
        void Ambient(float dt)
        {
            burstT = Mathf.Max(0f, burstT - dt);
            for (int i = 0; i < starScales.Length; i++)
            {
                if (stars[i] == null) continue;
                float k = 0.8f + 0.2f * Mathf.Sin(Time.time * (1.3f + i * 0.37f) + i * 1.7f);
                if (burstT > 0f) k += 0.6f * burstT * Mathf.Abs(Mathf.Sin(Time.time * 14f + i));
                stars[i].localScale = starScales[i] * k;
            }
            bool asleep = !finishing && !logic.InputLocked;
            if (asleep)
            {
                snoreT -= dt;
                if (snoreT <= 0f)
                {
                    snoreT = SnoreEvery;
                    SpawnZzz(1f);
                    if (snores++ % 2 == 0) V79LazySunSound.Play(this, "snore", 0.22f);
                }
            }
            for (int i = 0; i < zzzAge.Length; i++)
            {
                if (zzz[i] == null || zzzAge[i] < 0f) continue;
                zzzAge[i] += dt;
                float t = zzzAge[i];
                if (t >= ZzzLife) { zzzAge[i] = -1f; HalloweenKit.Show(zzz[i], false); continue; }
                Vector3 p = zzz[i].position;
                zzz[i].position = new Vector3(p.x + Mathf.Sin(t * 3f + i) * 0.25f * dt, p.y + ZzzSpeed * dt * (1f + 0.4f * zzzBoost[i]), p.z);
                zzz[i].localScale = zzzScale * Mathf.Lerp(0.6f, 1.15f, t / ZzzLife) * (1f + 0.7f * zzzBoost[i]);
                zzz[i].localRotation = Quaternion.Euler(0f, 0f, 12f * Mathf.Sin(t * 2.5f + i));
                if (zzzArt[i] != null) zzzArt[i].color = new Color(1f, 1f, 1f, Mathf.Clamp01((ZzzLife - t) / 0.8f));
            }
        }

        /// <summary>Un "Zzz" parte dalla testa del sole (dietro la collina: spunta dal bordo), o poco sotto il bordo se il sole è coperto.</summary>
        void SpawnZzz(float boost)
        {
            if (zzz == null || zzz.Length == 0) return;
            int i = nextZzz++ % zzz.Length;
            if (zzz[i] == null) return;
            float x = sun.position.x + 0.45f;
            float y = Mathf.Max(sun.position.y + 0.75f, EdgeY(x) - 0.3f);
            zzz[i].position = new Vector3(x, y, zzz[i].position.z);
            zzzAge[i] = 0f;
            zzzBoost[i] = boost > 1f ? 1f : 0f;
            HalloweenKit.Show(zzz[i], true);
        }

        void BigSnore()
        {
            SpawnZzz(2f);
            V79LazySunSound.Play(this, "bigsnore", 0.55f);
            Haptics.Light();
            rumbleT = 0.5f;
            if (logic.SunPeeking) FxShake(sunBody, 0.04f, 0.4f);
            toast.Show(BigSnoreLines[bigSnoreLines++ % BigSnoreLines.Length], 1.8f);
        }

        // ---------- Effetti che non si accavallano (come nei livelli 0.73-0.78) ----------

        void FxPop(Transform t, float peak, float seconds) => Fx(0, t, HalloweenKit.Pop(t, peak, seconds));
        void FxWiggle(Transform t, float degrees, float seconds, float speed) => Fx(1, t, HalloweenKit.Wiggle(t, degrees, seconds, speed));
        void FxShake(Transform t, float amplitude, float seconds) => Fx(2, t, HalloweenKit.Shake(t, amplitude, seconds));

        void Fx(int kind, Transform t, IEnumerator e)
        {
            if (t == null || !isActiveAndEnabled) return;
            Dictionary<Transform, FxSlot> d = fxSlots[kind];
            if (d.TryGetValue(t, out FxSlot slot) && slot.co != null)
            {
                StopCoroutine(slot.co);
                Restore(kind, t, slot);
            }
            else
            {
                if (slot == null) { slot = new FxSlot(); d[t] = slot; }
                slot.v = kind == 0 ? t.localScale : t.localPosition;
                slot.q = t.localRotation;
            }
            slot.co = StartCoroutine(FxRun(slot, e));
        }

        static IEnumerator FxRun(FxSlot slot, IEnumerator e)
        {
            yield return e;
            slot.co = null;
        }

        static void Restore(int kind, Transform t, FxSlot slot)
        {
            if (t == null) return;
            if (kind == 0) t.localScale = slot.v;
            else if (kind == 1) t.localRotation = slot.q;
            else t.localPosition = slot.v;
        }

        void StopFx()
        {
            for (int kind = 0; kind < fxSlots.Length; kind++)
            {
                foreach (KeyValuePair<Transform, FxSlot> kv in fxSlots[kind])
                    if (kv.Value.co != null) { StopCoroutine(kv.Value.co); Restore(kind, kv.Key, kv.Value); kv.Value.co = null; }
                fxSlots[kind].Clear();
            }
        }

        // ---------- Dito ----------

        void ReadPointer()
        {
            if (Input.touchCount >= 2) { CancelGrip(); return; }   // due dita: il gesto si annulla (la collina resta dov'è)
            if (!HalloweenKit.ReadPointer(cam, out Vector2 w, out bool began, out bool ended))
            {
                if (grip != null) End(grip);
                return;
            }
            if (began) { if (grip != null) End(grip); grip = Begin(w); }
            if (grip == null) return;
            if (!grip.moved && Vector2.Distance(w, grip.start) > TapSlop)
            {
                grip.moved = true;
                StartDrag(grip);
            }
            if (grip != null && grip.held) Drag(grip, w);
            if (ended && grip != null) End(grip);
        }

        static bool Over(Collider2D c, Vector2 p) => c != null && c.gameObject.activeInHierarchy && c.OverlapPoint(p);

        /// <summary>Cosa c'è sotto il dito: gallo, luna, prato, sole (solo dove si vede bene), collina (fascia generosa), cielo.</summary>
        Kind Pick(Vector2 w)
        {
            if (Over(roosterArea, w)) return Kind.Rooster;
            if (Over(moonArea, w)) return Kind.Moon;
            if (w.y < meadowTop) return Kind.Ground;
            float edge = EdgeY(w.x);
            if (LazySunLogic.OnVisibleSun(w.x, w.y, sun.position.x, sun.position.y, sunRadius, edge, GrabAbove)) return Kind.Sun;
            if (LazySunLogic.InHillGrab(w.y, edge, GrabAbove, GrabBelow)) return Kind.Hill;
            return Kind.Sky;
        }

        Grip Begin(Vector2 w)
        {
            var g = new Grip { start = w, kind = Kind.None };
            if (finishing || logic.InputLocked) return g;
            g.kind = Pick(w);
            return g;
        }

        /// <summary>Il dito ha superato la soglia del tocco: si prova a trascinare.</summary>
        void StartDrag(Grip g)
        {
            switch (g.kind)
            {
                case Kind.Hill:
                    if (!logic.GrabHill()) { g.kind = Kind.None; return; }
                    g.held = true;
                    g.grabLowered = logic.Lowered;
                    lastScrapeLowered = logic.Lowered;
                    Haptics.Tap();
                    V79LazySunSound.Play(this, "rumble", 0.35f);
                    break;
                case Kind.Sun:
                    g.kind = Kind.None;   // gesto consumato: il sole non si muove
                    GrabSun();
                    break;
            }
        }

        void Drag(Grip g, Vector2 w)
        {
            if (g.kind != Kind.Hill || !logic.Holding) return;
            float lowered = logic.DragHill(LazySunLogic.LoweredFromDrag(g.grabLowered, g.start.y, w.y, hillTravel));
            Vector3 p = HillAt(lowered);
            hill.position = new Vector3(p.x, p.y, hill.position.z);
            // un "grrr" di terra e un colpetto ogni tanto mentre scende
            if (Mathf.Abs(lowered - lastScrapeLowered) > 0.08f && Time.time - lastScrape > 0.12f)
            {
                lastScrapeLowered = lowered;
                lastScrape = Time.time;
                Haptics.Tap();
                V79LazySunSound.Play(this, "rumble", 0.22f);
            }
        }

        void End(Grip g)
        {
            grip = null;
            switch (g.kind)
            {
                case Kind.Hill:
                    if (g.held) DropHill();
                    else TapHill();
                    break;
                case Kind.Sun: TapSun(); break;
                case Kind.Moon: TapMoon(); break;
                case Kind.Rooster: TapRooster(); break;
                case Kind.Sky: TapSky(); break;
                case Kind.Ground: TapGround(); break;
            }
        }

        // ---------- Le false piste ----------

        void TapSky()
        {
            if (!logic.Tap(LazySunSpot.Sky)) return;
            burstT = 0.9f;
            V79LazySunSound.Play(this, "twinkle", 0.35f);
            toast.Show(SkyLines[skyLines++ % SkyLines.Length], 1.6f);
        }

        void TapMoon()
        {
            if (!logic.Tap(LazySunSpot.Moon)) return;
            FxWiggle(moon, 8f, 0.45f, 20f);
            V79LazySunSound.Play(this, "pop", 0.3f);
            roosterSay.Hide();
            moonSay.Show(MoonLines[moonLines++ % MoonLines.Length], 1.8f);
        }

        void TapRooster()
        {
            if (!logic.Tap(LazySunSpot.Rooster)) return;
            RoosterPose(1);
            peekT = PeekSeconds;
            FxPop(rooster, 1.06f, 0.2f);
            V79LazySunSound.Play(this, "cluck", 0.4f);
            moonSay.Hide();
            roosterSay.Show(RoosterLines[roosterLines++ % RoosterLines.Length], PeekSeconds);
        }

        void TapHill()
        {
            if (!logic.Tap(LazySunSpot.Hill)) return;
            rumbleT = 0.25f;
            V79LazySunSound.Play(this, "rumble", 0.25f);
            toast.Show(HillLines[hillLines++ % HillLines.Length], 1.6f);
        }

        void TapGround()
        {
            if (!logic.Tap(LazySunSpot.Ground)) return;
            toast.Show(GroundLines[groundLines++ % GroundLines.Length], 1.4f);
        }

        void TapSun()
        {
            if (!logic.Tap(LazySunSpot.Sun)) return;
            FxPop(sunBody, 1.05f, 0.25f);
            V79LazySunSound.Play(this, "snore", 0.3f);
            sunSay.Show(SunTapLines[sunTapLines++ % SunTapLines.Length], 1.8f);
        }

        void GrabSun()
        {
            if (!logic.GrabSun()) return;
            FxShake(sunBody, 0.05f, 0.3f);
            Haptics.Light();
            V79LazySunSound.Play(this, "rumble", 0.3f);
            toast.Show(TooHeavy, 2.0f);
        }

        // ---------- La collina ----------

        void DropHill()
        {
            switch (logic.ReleaseHill())
            {
                case LazySunRelease.Stays:
                    if (logic.SunPeeking)
                    {
                        // il sole si vede già un po': borbotta e continua a dormire
                        FxPop(sunBody, 1.04f, 0.3f);
                        sunSay.Show(SunMumbles[sunMumbles++ % SunMumbles.Length], 1.8f);
                        snoreT = Mathf.Min(snoreT, 0.3f);
                    }
                    else toast.Show(HillStayLines[hillStayLines++ % HillStayLines.Length], 1.5f);
                    break;
                case LazySunRelease.Rises:
                    StartCoroutine(Finale());
                    break;
            }
        }

        // ---------- La soluzione ----------

        /// <summary>
        /// La collina scivola in fondo, il sole si stiracchia, apre gli occhi e sorride ("Good morning!"), il berretto vola
        /// via, il sole sale un po' mentre il cielo diventa l'alba (stelle e luna svaniscono), il gallo canta. Poi la festa
        /// comune e Win, una volta sola. Rifatta da capo se il livello si spegne a metà.
        /// </summary>
        IEnumerator Finale()
        {
            if (finishing) yield break;
            finishing = true;
            grip = null;
            StopFx();
            HalloweenKit.Show(infoHolder, false);
            moonSay.Hide();
            roosterSay.Hide();
            sunSay.Hide();
            peekT = 0f;
            RoosterPose(0);
            for (int i = 0; i < zzzAge.Length; i++) { zzzAge[i] = -1f; HalloweenKit.Show(zzz[i], false); }
            sun.position = sunHome;
            sunBody.localScale = bodyScale;
            sunBody.localRotation = Quaternion.identity;
            nightcap.localPosition = capPos;
            nightcap.localRotation = capRot;
            nightcap.localScale = capScale;
            SetAlpha(capArt, 1f);
            HalloweenKit.Show(nightcap, true);
            Face(false);
            Dawn(0f);

            // la collina scivola fino in fondo
            Vector3 from = hill.position, to = HillTarget();
            V79LazySunSound.Play(this, "rumble", 0.45f);
            for (float t = 0f; t < 0.3f; t += Time.deltaTime)
            {
                float k = Mathf.Clamp01(t / 0.3f);
                hill.position = Vector3.Lerp(from, to, 1f - (1f - k) * (1f - k));
                yield return null;
            }
            hill.position = to;

            // il sole si stiracchia e apre gli occhi
            Face(true);
            Haptics.Light();
            V79LazySunSound.Play(this, "ding", 0.5f);
            sunSay.Show(SunHello, 1.2f);
            Vector3 capFrom = nightcap.localPosition;
            Quaternion capFromRot = nightcap.localRotation;
            bool crowed = false;
            const float dawnSeconds = 1.2f;
            for (float t = 0f; t < dawnSeconds; t += Time.deltaTime)
            {
                float k = Mathf.Clamp01(t / dawnSeconds);
                float e = 1f - (1f - k) * (1f - k);
                // stiracchiata: schiacciato e poi allungato, che si assesta
                float s = Mathf.Sin(Mathf.Clamp01(t / 0.45f) * Mathf.PI);
                sunBody.localScale = new Vector3(bodyScale.x * (1f + 0.12f * s), bodyScale.y * (1f - 0.08f * s), bodyScale.z);
                sunBody.localRotation = Quaternion.Euler(0f, 0f, -20f * e);   // i raggi girano piano
                sun.position = sunHome + new Vector3(0f, SunRise * e, 0f);
                // il berretto vola via in alto a destra, girando, e svanisce
                float c = Mathf.Clamp01(t / 0.7f);
                nightcap.localPosition = capFrom + new Vector3(1.3f * c, 1.1f * c - 0.6f * c * c, 0f);
                nightcap.localRotation = capFromRot * Quaternion.Euler(0f, 0f, -200f * c);
                SetAlpha(capArt, 1f - Mathf.Clamp01((c - 0.5f) / 0.5f));
                Dawn(e);
                if (!crowed && t >= 0.55f)
                {
                    crowed = true;
                    RoosterPose(2);
                    FxPop(rooster, 1.12f, 0.35f);
                    V79LazySunSound.Play(this, "crow", 0.55f);
                    roosterSay.Show(Crow, 1.5f);
                }
                yield return null;
            }
            sun.position = sunHome + new Vector3(0f, SunRise, 0f);
            sunBody.localScale = bodyScale;
            HalloweenKit.Show(nightcap, false);
            Dawn(1f);
            if (!crowed)
            {
                RoosterPose(2);
                V79LazySunSound.Play(this, "crow", 0.55f);
                roosterSay.Show(Crow, 1.5f);
            }
            yield return new WaitForSeconds(0.3f);

            if (reward != null) yield return reward.Play(Compliment);
            else yield return new WaitForSeconds(1.5f);
            if (logic.TryComplete()) Win(Compliment);
        }

#if UNITY_EDITOR
        // ---------- Tasti di prova nell'Editor ----------

        void EditorKeys()
        {
            if (logic.InputLocked || grip != null || logic.Holding) return;
            if (Input.GetKeyDown(KeyCode.T)) TapSky();
            if (Input.GetKeyDown(KeyCode.M)) TapMoon();
            if (Input.GetKeyDown(KeyCode.K)) TapRooster();
            if (Input.GetKeyDown(KeyCode.H)) TapHill();
            if (Input.GetKeyDown(KeyCode.G)) TapGround();
            if (Input.GetKeyDown(KeyCode.S)) GrabSun();
            if (Input.GetKeyDown(KeyCode.L)) EditorDrag(0.5f - logic.Lowered);
            if (Input.GetKeyDown(KeyCode.U)) EditorDrag(-1f);
            if (Input.GetKeyDown(KeyCode.W)) EditorDrag(1f);
            if (Input.GetKeyDown(KeyCode.P)) CancelGrip();
            if (Input.GetKeyDown(KeyCode.R)) OnLevelStart();
        }

        /// <summary>Prende la collina sulla cima, la sposta di <paramref name="fraction"/> della corsa (in giù se positivo) e la lascia.</summary>
        void EditorDrag(float fraction)
        {
            Vector2 start = new Vector2(hill.position.x, hill.position.y - 0.2f);
            var g = new Grip { kind = Kind.Hill, start = start, moved = true };
            StartDrag(g);
            if (!g.held) return;
            Drag(g, start - new Vector2(0f, fraction * hillTravel));
            End(g);
        }
#endif
    }

    /// <summary>Suoni sintetizzati di The Lazy Sun: russare (piano e forte), stelline, "pop", borbottio del gallo, chicchirichì, terra che scivola, "ding".</summary>
    public static class V79LazySunSound
    {
        const int Rate = 22050;
        static readonly Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>();
        static readonly System.Random rnd = new System.Random(295);

        public static void Play(Component owner, string kind, float volume = 0.6f)
        {
            if (owner == null) return;
            AudioClip c = Clip(kind);
            if (c == null) { Sfx.Play(owner, kind, volume); return; }
            AudioSource src = owner.GetComponent<AudioSource>();
            if (src == null) { src = owner.gameObject.AddComponent<AudioSource>(); src.playOnAwake = false; }
            src.PlayOneShot(c, volume);
        }

        static float N() => (float)rnd.NextDouble() * 2f - 1f;

        static AudioClip Snore(string kind, float seconds, float gain)
        {
            // russare: rumore filtrato con un ronzio basso che "gratta", inspirazione che sale e poi lo sbuffo
            float lp = 0f, ph = 0f;
            return Make(kind, seconds, t =>
            {
                float u = t / seconds;
                float env = Mathf.Sin(Mathf.Clamp01(u) * Mathf.PI);
                float f = Mathf.Lerp(55f, 80f, u);
                ph += f / Rate;
                float buzz = (ph - Mathf.Floor(ph)) * 2f - 1f;
                lp += (N() - lp) * 0.08f;
                return (buzz * 0.35f * (0.6f + 0.4f * Mathf.Sin(t * 2f * Mathf.PI * 23f)) + lp * 1.6f) * env * gain;
            });
        }

        static AudioClip Clip(string kind)
        {
            if (cache.TryGetValue(kind, out AudioClip c) && c != null) return c;
            switch (kind)
            {
                case "snore": c = Snore(kind, 0.9f, 0.5f); break;
                case "bigsnore": c = Snore(kind, 1.4f, 0.9f); break;
                case "twinkle":  // tre campanellini in salita
                    c = Make(kind, 0.5f, t =>
                    {
                        int n = Mathf.Min(2, (int)(t / 0.1f));
                        float f = n == 0 ? 1568f : n == 1 ? 2093f : 2637f;
                        float u = t - n * 0.1f;
                        return Mathf.Sin(t * 2f * Mathf.PI * f) * 0.3f * Mathf.Exp(-u * 9f) * Mathf.Min(1f, u * 400f);
                    });
                    break;
                case "pop":
                    c = Make(kind, 0.1f, t => Mathf.Sin(t * 2f * Mathf.PI * Mathf.Lerp(500f, 900f, t / 0.1f)) * 0.4f * Mathf.Exp(-t * 28f));
                    break;
                case "cluck":    // borbottio del gallo: due "bok" bassi
                    {
                        float ph = 0f;
                        c = Make(kind, 0.32f, t =>
                        {
                            float u = t < 0.14f ? t : t - 0.16f;
                            if (u < 0f) return 0f;
                            float f = (t < 0.14f ? 420f : 360f) * (1f - u * 1.5f);
                            ph += f / Rate;
                            float s = Mathf.Sin(ph * 2f * Mathf.PI);
                            return Mathf.Clamp(s * 2f, -1f, 1f) * 0.25f * Mathf.Exp(-u * 18f) * Mathf.Min(1f, u * 300f);
                        });
                        break;
                    }
                case "crow":     // chicchirichì: quattro note, l'ultima lunga e calante
                    {
                        float ph = 0f;
                        c = Make(kind, 1.1f, t =>
                        {
                            float f; float env;
                            if (t < 0.12f) { f = 700f; env = 1f; }
                            else if (t < 0.24f) { f = 900f; env = 1f; }
                            else if (t < 0.38f) { f = 1050f; env = 1f; }
                            else { f = Mathf.Lerp(1150f, 820f, (t - 0.38f) / 0.72f); env = Mathf.Clamp01((1.1f - t) / 0.4f); }
                            f += 25f * Mathf.Sin(t * 2f * Mathf.PI * 30f);
                            ph += f / Rate;
                            float saw = (ph - Mathf.Floor(ph)) * 2f - 1f;
                            float gap = Mathf.Clamp01(Mathf.Abs(Mathf.Repeat(t, 0.12f) - 0.06f) * 40f);
                            if (t >= 0.38f) gap = 1f;
                            return (saw * 0.22f + Mathf.Sin(ph * 2f * Mathf.PI) * 0.2f) * env * gap * Mathf.Min(1f, t * 80f);
                        });
                        break;
                    }
                case "rumble":   // terra che scivola: rumore basso
                    {
                        float lp = 0f;
                        c = Make(kind, 0.25f, t =>
                        {
                            lp += (N() - lp) * 0.05f;
                            return lp * 2.2f * Mathf.Sin(t / 0.25f * Mathf.PI);
                        });
                        break;
                    }
                case "ding":     // due note in salita
                    c = Make(kind, 0.6f, t =>
                    {
                        float f = t < 0.12f ? 784f : 1175f;
                        float u = t < 0.12f ? t : t - 0.12f;
                        return Mathf.Sin(t * 2f * Mathf.PI * f) * 0.35f * Mathf.Exp(-u * 6f) * Mathf.Min(1f, u * 300f);
                    });
                    break;
                default:
                    return null;
            }
            cache[kind] = c;
            return c;
        }

        static AudioClip Make(string name, float seconds, System.Func<float, float> f)
        {
            int n = Mathf.Max(1, (int)(seconds * Rate));
            var data = new float[n];
            for (int i = 0; i < n; i++) data[i] = Mathf.Clamp(f(i / (float)Rate), -1f, 1f);
            AudioClip clip = AudioClip.Create("v79b_" + name, n, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
