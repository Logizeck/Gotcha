using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels.V64;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 91 della roadmap "Mirror Liar" (id mirror_liar) — "FIND THE REAL ONE." Una stanza con uno specchio
    /// verticale in mezzo e due personaggi identici ai due lati (quello a destra è lo stesso disegno girato), che si
    /// contendono il titolo: "I'm the real one!" / "No, I am!".
    /// Inganno: un tocco su uno fa reagire allo stesso modo anche l'altro (saltello, sorriso e la stessa battuta in
    /// coro); trascinarne uno con un dito muove anche l'altro, specchiato rispetto allo specchio: con un dito non li si
    /// distingue. Toccare lo specchio non serve. Tutto reversibile, mai sconfitta.
    /// Soluzione: due dita, una su ciascun personaggio, mosse in modo diverso (direzioni opposte rispetto al riflesso:
    /// per esempio tutte e due verso destra, o una ferma e l'altra no). Il vero segue il suo dito, il falso resta il
    /// riflesso del vero e ignora il suo dito: più il suo dito si allontana, più il falso trema, diventa vetroso e lo
    /// specchio scricchiola (reversibile). Oltre MirrorLiarLogic.RevealDistance il falso si appiattisce come un vetro,
    /// si crepa insieme allo specchio e va in frantumi; il vero sbatte le palpebre e sorride; LevelReward "YOU PASSED
    /// THE MIRROR TEST!" -> Win una volta sola. Chi è il vero è casuale a ogni tentativo (seme dall'orologio).
    /// Dita: al massimo due (V64AFingers), in qualunque ordine; alzandone una si torna al comportamento speculare col
    /// dito rimasto che continua a trascinare (niente blocchi). Pausa, app in background o livello spento lasciano
    /// andare tutto (i personaggi restano dove sono). La logica è in Gotcha.Levels.V64.MirrorLiarLogic.
    /// Tasti di prova nell'Editor: W soluzione (due dita entrambe verso destra), M due dita speculari (niente),
    /// D trascinamento a un dito, T/Y tocco sul personaggio di sinistra/destra, K tocco sullo specchio, 1/2 il vero a
    /// sinistra/destra, P annulla le dita (come l'app in background), R ricomincia. Shift+clic pianta un secondo dito.
    /// </summary>
    public class V85MirrorLiarLevel : LevelController
    {
        [Header("Personaggi (0 sinistra, 1 destra; radice ai piedi, il destro ha scala x -1; Body per gli effetti)")]
        [SerializeField] Transform[] figures;
        [SerializeField] Transform[] bodies;
        [SerializeField] SpriteRenderer[] guyArts;
        [SerializeField] SpriteRenderer[] facesOpen;
        [SerializeField] SpriteRenderer[] facesBlink;
        [SerializeField] SpriteRenderer[] facesSmile;
        [SerializeField] SpriteRenderer[] facesWorry;
        [SerializeField] SpriteRenderer[] guyCracks;
        [SerializeField] Transform[] shadows;
        [Header("Specchio")]
        [SerializeField] Transform mirror;
        [SerializeField] SpriteRenderer mirrorCrack;
        [Header("Effetti")]
        [SerializeField] Transform[] shards;
        [SerializeField] SpriteRenderer sparkle;
        [Header("Fumetti, scritte e festa")]
        [SerializeField] SpeechBubble[] bubbles;
        [SerializeField] TextMesh infoText;
        [SerializeField] Transform infoHolder;
        [SerializeField] LevelReward reward;

        const string Compliment = "YOU PASSED THE MIRROR TEST!";
        const float Head = 1.2f;                      // tasti di prova: il dito sul busto
        const float BubbleTipY = 2.12f;               // punta della codina sopra la testa (dai piedi)
        const float BubbleScale = 0.8f;               // scala dei fumetti (come nel builder)
        static readonly Vector2 TipAnchor = new Vector2(-0.85f, -0.8f) * BubbleScale;   // punta della codina rispetto al fumetto (SpeechBubble, scala 0.8)
        static readonly Color GlassTint = new Color(0.72f, 0.9f, 1f, 0.62f);

        const string IntroL = "I'm the real one!";
        const string IntroR = "No, I am!";
        static readonly string[] TapLines = { "Copycat!", "Stop it!", "You stop it!", "I said it first!" };
        static readonly string[] DragLines = { "They move as one.", "Perfectly in sync.", "Can't tell them apart.", "Same moves. Same face." };
        static readonly string[] SyncLines = { "A flawless reflection.", "Both moved the same way.", "Mirror moves. No clue." };
        static readonly string[] MirrorLines = { "Just a mirror. Or is it?", "Seven years bad luck? No.", "Knock knock. Nobody." };
        const string SameLine = "One finger per twin.";
        const string RealLine = "Told you!";

        enum Face { Open, Blink, Smile, Worry }

        sealed class FxSlot
        {
            public Coroutine co;
            public Vector3 v;
            public Quaternion q;
        }

        MirrorLiarLogic logic;
        Camera cam;
        HwToast toast;
        readonly HwSay[] says = new HwSay[2];
        readonly List<int> down = new List<int>();
        readonly List<int> gone = new List<int>();
        readonly Dictionary<Transform, FxSlot>[] fxSlots = { new Dictionary<Transform, FxSlot>(), new Dictionary<Transform, FxSlot>() };
        readonly Vector2[] vis = new Vector2[2];
        readonly float[] tilt = new float[2];
        readonly Vector3[] bodyRest = new Vector3[2];
        readonly float[] faceUntil = new float[2];
        readonly Face[] faceOverride = new Face[2];
        Color[] baseGuy, baseOpen, baseBlink, baseSmile, baseWorry;
        Vector3[] rootScale;
        Vector3 mirrorHome;
        float nextBlink, blinkUntil, lastCreak = -10f, prevStrain;
        int tapLines, dragLines, syncLines, mirrorLines;
        bool captured, finishing;
        Coroutine introCo;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            StopAllCoroutines();
            introCo = null;
            StopFx();
            if (!captured)
            {
                captured = true;
                logic = new MirrorLiarLogic(System.Environment.TickCount);
                baseGuy = Colors(guyArts);
                baseOpen = Colors(facesOpen);
                baseBlink = Colors(facesBlink);
                baseSmile = Colors(facesSmile);
                baseWorry = Colors(facesWorry);
                rootScale = new Vector3[2];
                for (int s = 0; s < 2; s++) rootScale[s] = Fig(s) != null ? Fig(s).localScale : Vector3.one;
                if (mirror != null) mirrorHome = mirror.localPosition;
            }
            logic.Reset();   // il vero si pesca di nuovo a ogni tentativo
            if (reward != null) reward.ResetReward();
            V64AFingers.Reset();
            down.Clear();
            finishing = false;
            tapLines = dragLines = syncLines = mirrorLines = 0;
            toast = new HwToast(infoText, infoHolder);
            for (int s = 0; s < 2; s++) says[s] = new HwSay(Get(bubbles, s));
            RestPose();
            if (isActiveAndEnabled) introCo = StartCoroutine(Intro());
        }

        static Color[] Colors(SpriteRenderer[] list)
        {
            var c = new Color[list != null ? list.Length : 0];
            for (int i = 0; i < c.Length; i++) c[i] = list[i] != null ? list[i].color : Color.white;
            return c;
        }

        static T Get<T>(T[] list, int i) where T : class => list != null && i >= 0 && i < list.Length ? list[i] : null;
        Transform Fig(int s) => Get(figures, s);
        Transform Bod(int s) => Get(bodies, s);

        // ---------- Pose (sempre ricavate dalla logica: uno stato valido dopo pausa/spegnimento) ----------

        void RestPose()
        {
            for (int s = 0; s < 2; s++)
            {
                says[s]?.Hide();
                vis[s] = new Vector2(logic.X(s), logic.FeetY);
                tilt[s] = 0f;
                faceUntil[s] = -1f;
                Transform f = Fig(s);
                if (f != null)
                {
                    HalloweenKit.Show(f, true);
                    f.localRotation = Quaternion.identity;
                    f.localScale = rootScale[s];
                }
                Transform b = Bod(s);
                if (b != null) { b.localPosition = Vector3.zero; b.localRotation = Quaternion.identity; b.localScale = Vector3.one; }
                bodyRest[s] = Vector3.zero;
                HalloweenKit.Show(Get(shadows, s), true);
                HalloweenKit.Show(Get(guyCracks, s), false);
                SetGlass(s, 0f);
                SetFace(s, Face.Open);
                Place(s);
            }
            if (mirror != null) { mirror.localPosition = mirrorHome; mirror.localRotation = Quaternion.identity; }
            HalloweenKit.SetAlpha(mirrorCrack, 0f);
            HalloweenKit.Show(mirrorCrack, false);
            if (shards != null) foreach (Transform t in shards) HalloweenKit.Show(t, false);
            HalloweenKit.Show(sparkle, false);
            prevStrain = 0f;
            blinkUntil = -1f;
            nextBlink = Time.time + 2.5f;
        }

        void Place(int s)
        {
            Transform f = Fig(s);
            if (f == null) return;
            f.position = new Vector3(vis[s].x, vis[s].y, f.position.z);
            Transform sh = Get(shadows, s);
            if (sh != null) sh.position = new Vector3(vis[s].x, vis[s].y + 0.02f, sh.position.z);
        }

        void SetFace(int s, Face face)
        {
            HalloweenKit.Show(Get(facesOpen, s), face == Face.Open);
            HalloweenKit.Show(Get(facesBlink, s), face == Face.Blink);
            HalloweenKit.Show(Get(facesSmile, s), face == Face.Smile);
            HalloweenKit.Show(Get(facesWorry, s), face == Face.Worry);
        }

        /// <summary>Quanto il personaggio sembra di vetro (0 normale, 1 riflesso vetroso e trasparente).</summary>
        void SetGlass(int s, float k)
        {
            Tint(Get(guyArts, s), Get(baseGuy, s), k);
            Tint(Get(facesOpen, s), Get(baseOpen, s), k);
            Tint(Get(facesBlink, s), Get(baseBlink, s), k);
            Tint(Get(facesSmile, s), Get(baseSmile, s), k);
            Tint(Get(facesWorry, s), Get(baseWorry, s), k);
        }

        static Color Get(Color[] list, int i) => list != null && i >= 0 && i < list.Length ? list[i] : Color.white;

        static void Tint(SpriteRenderer sr, Color baseColor, float k)
        {
            if (sr == null) return;
            Color c = Color.Lerp(baseColor, new Color(GlassTint.r, GlassTint.g, GlassTint.b, baseColor.a * GlassTint.a), Mathf.Clamp01(k));
            sr.color = c;
        }

        // ---------- Ciclo di vita ----------

        void OnDisable()
        {
            // pausa/pubblicità/livello spento: si molla tutto, si torna a uno stato valido
            CancelAll();
            StopFx();
            introCo = null;
            if (captured && !finishing && !IsFinished && logic != null) RestPose();
        }

        void OnEnable()
        {
            if (!captured || IsFinished || logic == null) return;
            // spento a metà della festa: la si rifà da capo (la vittoria resta una sola)
            if (logic.Stage == MirrorLiarStage.Revealed) { finishing = false; StartCoroutine(Finale()); }
        }

        void OnApplicationPause(bool paused) { if (paused) CancelAll(); }
        void OnApplicationFocus(bool focus) { if (!focus) CancelAll(); }

        /// <summary>Le dita spariscono: i personaggi restano dove sono, la "tensione" del falso sparisce.</summary>
        void CancelAll()
        {
            down.Clear();
            V64AFingers.Reset();
            if (logic == null || logic.InputLocked) return;
            logic.Cancel();
        }

        void Update()
        {
            toast?.Tick();
            for (int s = 0; s < 2; s++) says[s]?.Tick();
            if (IsFinished || toast == null || logic == null) return;
            if (finishing) return;
            if (Time.timeScale <= 0f) { CancelAll(); return; }   // pausa o pubblicità: niente input
#if UNITY_EDITOR
            EditorKeys();
#endif
            if (!finishing) ReadFingers();
            if (!finishing) Pose();
        }

        /// <summary>
        /// I personaggi seguono la logica (sempre speculari) e si inclinano un poco col movimento, specchiati; presi si
        /// sollevano appena. Con due dita il falso trema, diventa vetroso e fa la faccia preoccupata in proporzione alla
        /// "tensione" (quanto il suo dito si allontana); lo specchio scricchiola. Sbattono le palpebre insieme.
        /// </summary>
        void Pose()
        {
            float dt = Mathf.Min(Time.deltaTime, 0.05f);
            bool two = logic.TwoHanded;
            float strain = two ? logic.Strain : 0f;
            int fake = logic.FakeSide;
            bool held = logic.Held(0) || logic.Held(1);
            if (Time.time >= nextBlink && blinkUntil < 0f) { blinkUntil = Time.time + 0.13f; }
            if (blinkUntil > 0f && Time.time >= blinkUntil) { blinkUntil = -1f; nextBlink = Time.time + Random.Range(2.2f, 4f); }
            for (int s = 0; s < 2; s++)
            {
                Vector2 target = new Vector2(logic.X(s), logic.FeetY);
                float before = vis[s].x;
                vis[s] = Vector2.Lerp(vis[s], target, 1f - Mathf.Exp(-dt * 28f));
                float vx = dt > 0f ? (vis[s].x - before) / dt : 0f;
                float want = held ? Mathf.Clamp(-vx * 2.5f, -8f, 8f) * (s == 0 ? 1f : -1f) : 0f;
                tilt[s] = Mathf.Lerp(tilt[s], want, 1f - Mathf.Exp(-dt * 12f));
                Transform f = Fig(s);
                if (f != null) f.localRotation = Quaternion.Euler(0f, 0f, s == 0 ? tilt[s] : -tilt[s]);
                Place(s);

                bool isFake = two && s == fake;
                float k = isFake ? strain : 0f;
                Transform b = Bod(s);
                if (b != null)
                {
                    float lift = held ? 0.06f : 0f;
                    Vector3 jitter = isFake && k > 0.05f ? new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f) * (0.05f * k) : Vector3.zero;
                    bodyRest[s] = Vector3.Lerp(bodyRest[s], new Vector3(0f, lift, 0f), 1f - Mathf.Exp(-dt * 18f));
                    b.localPosition = bodyRest[s] + jitter;
                }
                SetGlass(s, k * 0.7f);
                Face face = Face.Open;
                if (faceUntil[s] > Time.time) face = faceOverride[s];
                else if (isFake && k > 0.4f) face = Face.Worry;
                else if (blinkUntil > 0f) face = Face.Blink;
                SetFace(s, face);
            }
            // lo specchio comincia a creparsi (torna intatto se le dita tornano speculari)
            float crack = Mathf.Clamp01((strain - 0.5f) * 1.2f);
            HalloweenKit.Show(mirrorCrack, crack > 0.01f);
            HalloweenKit.SetAlpha(mirrorCrack, crack);
            if (strain > 0.5f && prevStrain <= 0.5f && Time.time - lastCreak > 0.4f)
            {
                lastCreak = Time.time;
                V85MirrorLiarSound.Play(this, "creak", 0.35f);
                Haptics.Light();
            }
            prevStrain = strain;
        }

        // ---------- Effetti che non si accavallano (come nei livelli 0.73-0.84) ----------

        void FxPop(Transform t, float peak, float seconds) => Fx(0, t, HalloweenKit.Pop(t, peak, seconds));
        void FxWiggle(Transform t, float degrees, float seconds, float speed) => Fx(1, t, HalloweenKit.Wiggle(t, degrees, seconds, speed));

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
                slot.v = t.localScale;
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
            else t.localRotation = slot.q;
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

        // ---------- Dita (al massimo due) ----------

        void ReadFingers()
        {
            List<V64AFinger> fingers = V64AFingers.Read(cam);
            foreach (V64AFinger f in fingers)
            {
                if (f.began)
                {
                    if (logic.Tracks(f.id)) Lift(f.id);
                    if (finishing) return;
                    OnPress(logic.Press(f.id, f.world.x, f.world.y));
                    if (logic.Tracks(f.id) && !down.Contains(f.id)) down.Add(f.id);
                }
                if (!logic.Tracks(f.id)) continue;
                Handle(logic.Move(f.id, f.world.x, f.world.y));
                if (finishing) return;
                if (f.ended) Lift(f.id);
                if (finishing) return;
            }
            if (fingers.Count == 0 && down.Count > 0)
            {
                // dita sparite senza "Ended": si chiudono i gesti
                gone.Clear();
                gone.AddRange(down);
                foreach (int id in gone) { if (finishing) return; Lift(id); }
            }
        }

        void Lift(int id)
        {
            down.Remove(id);
            Handle(logic.Release(id));
        }

        void OnPress(MirrorLiarPress p)
        {
            switch (p)
            {
                case MirrorLiarPress.Left:
                case MirrorLiarPress.Right:
                    // chiunque si prenda, reagiscono tutti e due
                    Haptics.Tap();
                    V85MirrorLiarSound.Play(this, "pick", 0.3f);
                    if (introCo != null) { StopCoroutine(introCo); introCo = null; }
                    break;
                case MirrorLiarPress.SameFigure:
                    toast.Show(SameLine, 1.4f);
                    break;
            }
        }

        // ---------- Risultati dei gesti ----------

        void Handle(MirrorLiarEvent e)
        {
            switch (e)
            {
                case MirrorLiarEvent.Revealed:
                    StartCoroutine(Finale());
                    break;
                case MirrorLiarEvent.Tapped:
                    MirroredReaction();
                    break;
                case MirrorLiarEvent.Dropped:
                    V85MirrorLiarSound.Play(this, "drop", 0.3f);
                    toast.Show(DragLines[dragLines++ % DragLines.Length], 1.5f);
                    break;
                case MirrorLiarEvent.Synced:
                    V85MirrorLiarSound.Play(this, "drop", 0.3f);
                    toast.Show(SyncLines[syncLines++ % SyncLines.Length], 1.6f);
                    break;
                case MirrorLiarEvent.MirrorTapped:
                    V85MirrorLiarSound.Play(this, "tink", 0.4f);
                    FxWiggle(mirror, 1.5f, 0.3f, 40f);
                    toast.Show(MirrorLines[mirrorLines++ % MirrorLines.Length], 1.6f);
                    break;
            }
        }

        /// <summary>Il tocco su uno dei due: saltello, sorriso e la stessa battuta in coro, specchiati.</summary>
        void MirroredReaction()
        {
            if (introCo != null) { StopCoroutine(introCo); introCo = null; }
            V85MirrorLiarSound.Play(this, "boop", 0.4f);
            Haptics.Light();
            string line = TapLines[tapLines++ % TapLines.Length];
            for (int s = 0; s < 2; s++)
            {
                FxPop(Bod(s), 1.1f, 0.25f);
                faceOverride[s] = Face.Smile;
                faceUntil[s] = Time.time + 0.6f;
                Say(s, line, 1.4f);
            }
        }

        /// <summary>Il fumetto sopra la testa del personaggio; la codina indica lui, il fumetto resta nella sua metà.</summary>
        void Say(int s, string line, float seconds)
        {
            SpeechBubble b = Get(bubbles, s);
            if (b == null) return;
            float x = vis[s].x;
            float tipX = s == 0 ? Mathf.Clamp(x - 0.75f, -2.25f, -2.0f) : Mathf.Clamp(x - 0.6f, 0.55f, 0.75f);   // fumetto (scala 0.8) largo al massimo ~1.95: resta nella sua metà, dentro x ±2.5
            float tipY = Mathf.Min(vis[s].y + BubbleTipY, 0.3f);
            Vector2 pos = new Vector2(tipX, tipY) - TipAnchor;
            b.transform.position = new Vector3(pos.x, pos.y, b.transform.position.z);
            says[s].Show(line, seconds);
        }

        IEnumerator Intro()
        {
            yield return new WaitForSeconds(0.5f);
            Say(0, IntroL, 1.8f);
            yield return new WaitForSeconds(0.8f);
            Say(1, IntroR, 1.6f);
            introCo = null;
        }

        // ---------- La soluzione ----------

        /// <summary>
        /// Il falso non ha seguito il suo dito: si appiattisce come un vetro, si crepa insieme allo specchio e va in
        /// frantumi; il vero sbatte le palpebre due volte, sorride e saltella ("Told you!"); poi la festa comune e Win, una
        /// volta sola. Rifatta da capo se il livello si spegne a metà.
        /// </summary>
        IEnumerator Finale()
        {
            if (finishing) yield break;
            finishing = true;
            down.Clear();
            V64AFingers.Reset();
            if (introCo != null) { StopCoroutine(introCo); introCo = null; }
            StopFx();
            HalloweenKit.Show(infoHolder, false);
            int real = logic.RealSide, fake = logic.FakeSide;
            // stato di partenza della festa (anche se rifatta dopo uno spegnimento)
            for (int s = 0; s < 2; s++)
            {
                says[s].Hide();
                vis[s] = new Vector2(logic.X(s), logic.FeetY);
                tilt[s] = 0f;
                Transform f = Fig(s);
                if (f != null) { HalloweenKit.Show(f, true); f.localRotation = Quaternion.identity; f.localScale = rootScale[s]; }
                Transform b = Bod(s);
                if (b != null) { b.localPosition = Vector3.zero; b.localRotation = Quaternion.identity; b.localScale = Vector3.one; }
                HalloweenKit.Show(Get(shadows, s), true);
                HalloweenKit.Show(Get(guyCracks, s), false);
                SetGlass(s, 0f);
                Place(s);
            }
            SetFace(real, Face.Open);
            SetFace(fake, Face.Worry);
            if (shards != null) foreach (Transform t in shards) HalloweenKit.Show(t, false);
            HalloweenKit.Show(sparkle, false);
            HalloweenKit.Show(mirrorCrack, false);
            if (mirror != null) { mirror.localPosition = mirrorHome; mirror.localRotation = Quaternion.identity; }
            Haptics.Medium();

            // 1) il falso diventa un riflesso piatto e vetroso
            V85MirrorLiarSound.Play(this, "creak", 0.45f);
            Transform fb = Bod(fake);
            for (float t = 0f; t < 0.35f; t += Time.deltaTime)
            {
                float k = t / 0.35f;
                SetGlass(fake, k);
                if (fb != null) fb.localScale = new Vector3(Mathf.Lerp(1f, 0.25f, k * k), 1f, 1f);
                yield return null;
            }
            SetGlass(fake, 1f);
            if (fb != null) fb.localScale = new Vector3(0.25f, 1f, 1f);

            // 2) si crepa, e con lui lo specchio
            V85MirrorLiarSound.Play(this, "crack", 0.55f);
            Haptics.Medium();
            HalloweenKit.Show(Get(guyCracks, fake), true);
            HalloweenKit.Show(mirrorCrack, true);
            HalloweenKit.SetAlpha(mirrorCrack, 1f);
            if (mirror != null) StartCoroutine(HalloweenKit.Shake(mirror, 0.05f, 0.25f));
            yield return new WaitForSeconds(0.25f);

            // 3) va in frantumi; il vero sbatte le palpebre e sorride
            V85MirrorLiarSound.Play(this, "shatter", 0.55f);
            Vector2 burst = vis[fake] + new Vector2(0f, 1.0f);
            HalloweenKit.Show(Fig(fake), false);
            HalloweenKit.Show(Get(shadows, fake), false);
            StartCoroutine(Shatter(burst));
            SetFace(real, Face.Blink);
            yield return new WaitForSeconds(0.12f);
            SetFace(real, Face.Open);
            yield return new WaitForSeconds(0.1f);
            SetFace(real, Face.Blink);
            yield return new WaitForSeconds(0.12f);
            SetFace(real, Face.Smile);
            FxPop(Bod(real), 1.12f, 0.3f);
            V85MirrorLiarSound.Play(this, "yay", 0.45f);
            StartCoroutine(Sparkle(vis[real] + new Vector2(real == 0 ? 0.45f : -0.45f, 2.0f)));
            Say(real, RealLine, 0.9f);
            yield return new WaitForSeconds(0.55f);
            says[real].Hide();

            if (reward != null) yield return reward.Play(Compliment);
            else yield return new WaitForSeconds(1.5f);
            if (logic.TryComplete()) Win(Compliment);
        }

        /// <summary>Le schegge volano dal falso, ricadono ruotando e spariscono (niente fisica).</summary>
        IEnumerator Shatter(Vector2 from)
        {
            if (shards == null || shards.Length == 0) yield break;
            int n = shards.Length;
            var vel = new Vector2[n];
            var spin = new float[n];
            for (int i = 0; i < n; i++)
            {
                if (shards[i] == null) continue;
                float a = (i / (float)n) * Mathf.PI * 2f + 0.3f;
                vel[i] = new Vector2(Mathf.Cos(a) * 1.6f, Mathf.Sin(a) * 1.4f + 1.6f);
                spin[i] = (i % 2 == 0 ? 1f : -1f) * (300f + 60f * i);
                shards[i].position = new Vector3(from.x + Mathf.Cos(a) * 0.15f, from.y + Mathf.Sin(a) * 0.3f, shards[i].position.z);
                shards[i].localScale = Vector3.one;
                shards[i].localRotation = Quaternion.Euler(0f, 0f, i * 47f);
                HalloweenKit.Show(shards[i], true);
            }
            for (float t = 0f; t < 0.8f; t += Time.deltaTime)
            {
                float dt = Time.deltaTime;
                for (int i = 0; i < n; i++)
                {
                    Transform s = shards[i];
                    if (s == null) continue;
                    vel[i].y -= 7f * dt;
                    Vector3 p = s.position + (Vector3)(vel[i] * dt);
                    p.x = Mathf.Clamp(p.x, -2.5f, 2.5f);
                    s.position = p;
                    s.localRotation = Quaternion.Euler(0f, 0f, s.localRotation.eulerAngles.z + spin[i] * dt);
                    s.localScale = Vector3.one * Mathf.Clamp01(1.2f - t / 0.8f);
                }
                yield return null;
            }
            foreach (Transform s in shards) HalloweenKit.Show(s, false);
        }

        IEnumerator Sparkle(Vector2 at)
        {
            if (sparkle == null) yield break;
            sparkle.transform.position = new Vector3(at.x, at.y, sparkle.transform.position.z);
            HalloweenKit.Show(sparkle, true);
            for (float t = 0f; t < 0.6f; t += Time.deltaTime)
            {
                float k = t / 0.6f;
                sparkle.transform.localScale = Vector3.one * (1.1f * Mathf.Sin(k * Mathf.PI));
                sparkle.transform.localRotation = Quaternion.Euler(0f, 0f, k * 120f);
                yield return null;
            }
            HalloweenKit.Show(sparkle, false);
        }

#if UNITY_EDITOR
        // ---------- Tasti di prova nell'Editor ----------

        const int KeyA = 901, KeyB = 902;

        void EditorKeys()
        {
            if (logic.InputLocked || down.Count > 0 || logic.FingerCount > 0) return;
            if (Input.GetKeyDown(KeyCode.W)) KeyTwo(0.6f, 0f, 0.6f, 0f);
            if (Input.GetKeyDown(KeyCode.M)) KeyTwo(-0.4f, 0.3f, 0.4f, 0.3f);
            if (Input.GetKeyDown(KeyCode.D)) KeySolo(0, 0.3f, 0.3f);
            if (Input.GetKeyDown(KeyCode.T)) KeyTap(logic.X(0), logic.FeetY + Head);
            if (Input.GetKeyDown(KeyCode.Y)) KeyTap(logic.X(1), logic.FeetY + Head);
            if (Input.GetKeyDown(KeyCode.K)) KeyTap(MirrorLiarLogic.MirrorX, -1f);
            if (Input.GetKeyDown(KeyCode.Alpha1) && logic.SetRealSide(MirrorLiarLogic.LeftSide)) toast.Show("Editor: real = LEFT", 1.2f);
            if (Input.GetKeyDown(KeyCode.Alpha2) && logic.SetRealSide(MirrorLiarLogic.RightSide)) toast.Show("Editor: real = RIGHT", 1.2f);
            if (Input.GetKeyDown(KeyCode.P)) CancelAll();
            if (Input.GetKeyDown(KeyCode.R)) OnLevelStart();
        }

        void KeyTap(float x, float y)
        {
            OnPress(logic.Press(KeyA, x, y));
            Handle(logic.Release(KeyA));
        }

        void KeySolo(int side, float dx, float dy)
        {
            float x = logic.X(side), y = logic.FeetY + Head;
            OnPress(logic.Press(KeyA, x, y));
            for (int i = 1; i <= 6; i++) logic.Move(KeyA, x + dx * i / 6f, y + dy * i / 6f);
            Handle(logic.Release(KeyA));
        }

        /// <summary>Due dita simulate, una per personaggio, mosse di (ldx, ldy) e (rdx, rdy).</summary>
        void KeyTwo(float ldx, float ldy, float rdx, float rdy)
        {
            float lx = logic.X(0), rx = logic.X(1), y = logic.FeetY + Head;
            OnPress(logic.Press(KeyA, lx, y));
            OnPress(logic.Press(KeyB, rx, y));
            for (int i = 1; i <= 8; i++)
            {
                float k = i / 8f;
                MirrorLiarEvent a = logic.Move(KeyA, lx + ldx * k, y + ldy * k);
                MirrorLiarEvent b = logic.Move(KeyB, rx + rdx * k, y + rdy * k);
                if (a == MirrorLiarEvent.Revealed || b == MirrorLiarEvent.Revealed) { Handle(MirrorLiarEvent.Revealed); return; }
            }
            Handle(logic.Release(KeyA));
            Handle(logic.Release(KeyB));
        }
#endif
    }

    /// <summary>Suoni sintetizzati di Mirror Liar: presa, posa, "boop" del tocco in coro, "tin" dello specchio, scricchiolio, crepa, frantumi, "evviva".</summary>
    public static class V85MirrorLiarSound
    {
        const int Rate = 22050;
        static readonly Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>();
        static readonly System.Random rnd = new System.Random(291);

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

        static AudioClip Clip(string kind)
        {
            if (cache.TryGetValue(kind, out AudioClip c) && c != null) return c;
            switch (kind)
            {
                case "pick":     // "plip"
                    c = Make(kind, 0.08f, t => Mathf.Sin(t * 2f * Mathf.PI * Mathf.Lerp(600f, 1100f, t / 0.08f)) * 0.4f * Mathf.Exp(-t * 30f));
                    break;
                case "drop":     // passo sul pavimento
                    c = Make(kind, 0.1f, t => (N() * 0.25f + Mathf.Sin(t * 2f * Mathf.PI * 180f) * 0.4f) * Mathf.Exp(-t * 45f));
                    break;
                case "boop":     // due "boop" identici, in coro
                    c = Make(kind, 0.22f, t =>
                    {
                        float f = Mathf.Lerp(520f, 380f, t / 0.22f);
                        return (Mathf.Sin(t * 2f * Mathf.PI * f) + 0.6f * Mathf.Sin(t * 2f * Mathf.PI * f * 1.005f)) * 0.22f * Mathf.Sin(t / 0.22f * Mathf.PI);
                    });
                    break;
                case "tink":     // dito sul vetro
                    c = Make(kind, 0.35f, t => (Mathf.Sin(t * 2f * Mathf.PI * 2300f) * 0.25f + Mathf.Sin(t * 2f * Mathf.PI * 3470f) * 0.12f) * Mathf.Exp(-t * 12f));
                    break;
                case "creak":    // il vetro che scricchiola
                    c = Make(kind, 0.3f, t =>
                    {
                        float f = 900f + 300f * Mathf.Sin(t * 2f * Mathf.PI * 13f);
                        return (Mathf.Sin(t * 2f * Mathf.PI * f) * 0.18f + N() * 0.05f) * Mathf.Sin(t / 0.3f * Mathf.PI);
                    });
                    break;
                case "crack":    // la crepa
                    c = Make(kind, 0.18f, t => (N() * 0.6f * Mathf.Exp(-t * 35f) + Mathf.Sin(t * 2f * Mathf.PI * 1800f) * 0.2f * Mathf.Exp(-t * 25f)));
                    break;
                case "shatter":  // frantumi: tanti "tin" sparsi
                    c = Make(kind, 0.6f, t =>
                    {
                        float tinkles = Mathf.Sin(t * 2f * Mathf.PI * 3100f) * Mathf.Exp(-((t * 37f) % 1f) * 6f) * 0.15f
                                      + Mathf.Sin(t * 2f * Mathf.PI * 4200f) * Mathf.Exp(-((t * 23f) % 1f) * 7f) * 0.12f;
                        return (N() * 0.5f * Mathf.Exp(-t * 14f) + tinkles) * Mathf.Clamp01((0.6f - t) * 5f);
                    });
                    break;
                case "yay":      // "evviva" che sale
                    c = Make(kind, 0.6f, t =>
                    {
                        float f = Mathf.Lerp(420f, 900f, t / 0.6f) + 25f * Mathf.Sin(t * 2f * Mathf.PI * 7f);
                        return Mathf.Sin(t * 2f * Mathf.PI * f) * 0.25f * Mathf.Min(1f, t * 30f) * Mathf.Clamp01((0.6f - t) * 6f);
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
            AudioClip clip = AudioClip.Create("v85b_" + name, n, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
