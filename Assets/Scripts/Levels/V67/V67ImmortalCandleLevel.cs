using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels.V64;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 262 "The Immortal Candle" — "Blow out the candle." Una torta di compleanno con una sola candelina accesa,
    /// un bicchiere d'acqua e un ventilatore a colonna. Soffiare nel microfono fa solo crescere la fiamma ("KEEP GOING,
    /// I'M GROWING!" ogni tanto), il ventilatore la piega dall'altra parte, l'acqua fa tanto vapore, un dito la fa
    /// ridacchiare e dimenare (indizio: è "solida"). Soluzione: due dita nella fiamma, trascinarle in su finché si stacca
    /// dallo stoppino (la candela è spenta) e portarla oltre il bordo in alto. La fiamma torna a fare la linguaccia e
    /// sparisce per sempre. Il microfono non serve per vincere: senza microfono compare un pulsante "tieni premuto per
    /// soffiare" solo per provare la mossa sbagliata.
    /// Editor: Shift+clic = secondo dito fermo (V64AFingers), B/N = soffio, F = ventilatore, G = bicchiere,
    /// T = solletico, C = presa a due dita e trasporto automatici.
    /// La logica è in Gotcha.Levels.V64.ImmortalCandleLogic (senza Unity): qui solo input e presentazione.
    /// </summary>
    public class V67ImmortalCandleLevel : V62Level
    {
        [SerializeField] Transform wick;               // punta dello stoppino
        [SerializeField] Transform flame;              // centro della fiamma (si stacca e segue le dita)
        [SerializeField] Transform flameBody;          // base della fiamma: da qui si allunga e si piega
        [SerializeField] CircleCollider2D flameArea;   // area di presa (il raggio nel mondo va alla logica)
        [SerializeField] SpriteRenderer flameGlow;
        [SerializeField] SpriteRenderer giggleFace;    // faccina che ridacchia (nascosta finché non la si tocca)
        [SerializeField] SmokeEmitter wickSmoke;       // filo di fumo dallo stoppino spento
        [SerializeField] SmokeEmitter steam;           // vapore quando arriva l'acqua
        [SerializeField] Transform fan;
        [SerializeField] Collider2D fanArea;
        [SerializeField] Transform fanBlades;
        [SerializeField] SpriteRenderer fanWind;       // linee del vento tra ventilatore e fiamma
        [SerializeField] Transform glass;
        [SerializeField] Collider2D glassArea;
        [SerializeField] SmokeEmitter pourDrops;       // l'acqua che cade dal bordo del bicchiere
        [SerializeField] Transform peek;               // la fiammella che torna a fare la linguaccia (nascosta)
        [SerializeField] SpeechBubble flameBubble;
        [SerializeField] Transform micButton;          // alternativa al microfono (tenere premuto), solo se manca
        [SerializeField] Collider2D micButtonArea;
        [SerializeField] TextMesh infoText;
        [SerializeField] Transform infoHolder;

        const float PourZoneHalfWidth = 1.5f;          // il bicchiere lasciato qui sopra versa sulla candela
        const float PourZoneBelowWick = 2.2f, PourZoneAboveWick = 2.6f;
        const float FanLean = 28f;                     // gradi: la fiamma si piega lontano dal ventilatore (a sinistra)
        const float FanSpin = 1100f;                   // gradi al secondo a regime

        static readonly string[] TickleLines = { "Hee hee!", "Stop it, that tickles!", "Hihihi! Again?" };
        static readonly string[] GrabLines = { "W-what are you doing?", "Hey! Hands off!", "Not this again!" };
        static readonly string[] BlowLines = { "The flame got BIGGER.", "That's... not how wind works.", "It's thriving." };
        static readonly string[] FanLines = { "Whirrr... The flame leans away. Politely.", "Maximum power. Minimum effect.", "It's enjoying the breeze." };
        static readonly string[] PourLines = { "Pssssh! Lots of steam. Still lit.", "Hiss! The cake is soggy. The flame isn't.", "Unlimited water. Unlimited flame." };
        static readonly string[] SnapLines = { "Boing! It snapped back onto the wick.", "It's stuck on there pretty well. Pull harder!", "Boing. Again." };

        readonly ImmortalCandleLogic logic = new ImmortalCandleLogic();
        readonly V64AMicGate gate = new V64AMicGate();
        V64AMic mic;
        Camera cam;
        HwToast toast;
        HwSay say;
        HwDragger dragger;
        HwDragger.Item glassItem;
        bool captured, busy, micHeld, pouring;
        int glassFinger = int.MinValue, micFinger = int.MinValue;
        Vector3 flameHome, bodyScale, glassHome, glassScale, peekHome, glowScale;
        Quaternion glassRot;
        SpriteRenderer[] flameSprites;
        int[] flameOrders;
        bool flameOnTop;
        Color glowColor;
        float lean, spin, tickleT, pourFlicker, faceT, wobbleSeed;
        int tickleLines, grabLines, blowLines, fanLines, pourLines, snapLines;
#if UNITY_EDITOR
        int editorCarry;   // tasto C: presa automatica (0 = spenta)
#endif

        public override void OnLevelStart()
        {
            cam = Camera.main;
            if (!captured)
            {
                captured = true;
                flameHome = flame.position;
                bodyScale = flameBody.localScale;
                glassHome = glass.position;
                glassRot = glass.rotation;
                glassScale = glass.localScale;
                peekHome = peek.position;
                glowScale = flameGlow != null ? flameGlow.transform.localScale : Vector3.one;
                glowColor = flameGlow != null ? flameGlow.color : Color.white;
                flameSprites = flame.GetComponentsInChildren<SpriteRenderer>(true);
                flameOrders = new int[flameSprites.Length];
                for (int i = 0; i < flameSprites.Length; i++) flameOrders[i] = flameSprites[i].sortingOrder;
            }
            flameOnTop = true;
            FlameOnTop(false);
            StopAllCoroutines();
            ResetV62();
            V64AFingers.Reset();
            busy = micHeld = pouring = false;
            glassFinger = micFinger = int.MinValue;
            tickleT = pourFlicker = faceT = 0f;
            lean = spin = 0f;
            wobbleSeed = Random.Range(0f, 10f);
#if UNITY_EDITOR
            editorCarry = 0;
#endif

            // la logica lavora nel mondo: casa della fiamma e raggio della presa dal prefab
            logic.SetHome(flameHome.x, flameHome.y);
            logic.GrabRadius = flameArea != null ? flameArea.radius * Mathf.Abs(flameArea.transform.lossyScale.x) : 1f;
            logic.Reset();

            flame.position = flameHome;
            flame.localRotation = Quaternion.identity;
            HalloweenKit.Show(flame, true);
            flameBody.localScale = bodyScale;
            flameBody.localRotation = Quaternion.identity;
            if (giggleFace != null) HalloweenKit.Show(giggleFace, false);
            glass.position = glassHome;
            glass.rotation = glassRot;
            glass.localScale = glassScale;
            HalloweenKit.Show(peek, false);
            peek.position = peekHome;
            peek.localRotation = Quaternion.identity;
            if (fanWind != null) HalloweenKit.SetAlpha(fanWind, 0f);
            foreach (SmokeEmitter e in new[] { wickSmoke, steam, pourDrops }) if (e != null) e.Stop();

            toast = new HwToast(infoText, infoHolder);
            say = new HwSay(flameBubble);
            say.Hide();
            dragger = new HwDragger(this);
            glassItem = dragger.Add(glass, glassArea, 1);
            if (mic != null) mic.Use(false);
            mic = new V64AMic(this);
            mic.Use(true);   // soffiare è la prima cosa che tutti provano
            HalloweenKit.Show(micButton, false);
            Present(0f);
        }

        void OnDisable() { if (mic != null) mic.Use(false); }
        void OnDestroy() { if (mic != null) mic.Use(false); }
        void OnApplicationPause(bool paused) { if (paused) DropAll(); }
        void OnApplicationFocus(bool focused) { if (!focused) DropAll(); }

        /// <summary>Pausa, pubblicità, app in secondo piano: si lascia tutto in uno stato valido.</summary>
        void DropAll()
        {
            if (toast == null || busy || logic.InputLocked) return;
            React(logic.CancelFingers());
            V64AFingers.Reset();
            if (glassFinger != int.MinValue) { glassFinger = int.MinValue; if (!pouring) dragger.Cancel(); else dragger.Release(); }
            micHeld = false;
            micFinger = int.MinValue;
#if UNITY_EDITOR
            editorCarry = 0;
#endif
        }

        void Update()
        {
            if (toast == null) return;
            toast.Tick();
            say.Tick();
            if (IsFinished || busy || logic.InputLocked) return;
            if (Time.timeScale <= 0f)   // pausa o pubblicità: niente input né microfono
            {
                DropAll();
                mic.Use(false);
                return;
            }
            mic.Use(true);
            float dt = Time.deltaTime;

            ReadFingers();
            if (busy) return;
#if UNITY_EDITOR
            EditorKeys();
            if (busy) return;
#endif
            bool loud = gate.Feed(mic.Level(), dt) || micHeld;
            React(logic.BlowFeed(loud, dt));
            React(logic.Tick(dt));
            if (busy) return;
            Present(dt);
            ShowMicButton();
        }

        // ---------- Dita ----------

        void ReadFingers()
        {
            List<V64AFinger> fingers = V64AFingers.Read(cam);
            foreach (V64AFinger f in fingers)
            {
                if (busy) return;
                if (f.began) Began(f);
                else if (logic.IsFlameFinger(f.id)) React(logic.FingerMove(f.id, f.world.x, f.world.y));
                else if (f.id == glassFinger) dragger.MoveHeld(f.world);
                if (busy) return;
                if (!f.ended) continue;
                if (logic.IsFlameFinger(f.id)) React(logic.FingerUp(f.id));
                if (f.id == glassFinger) ReleaseGlass();
                if (f.id == micFinger) { micFinger = int.MinValue; micHeld = false; }
            }
            // dita sparite senza "Ended" (notifica, telefonata...): si lascia tutto
            if (fingers.Count == 0)
            {
#if UNITY_EDITOR
                if (editorCarry > 0) return;
#endif
                if (logic.FlameFingerCount > 0) React(logic.CancelFingers());
                if (glassFinger != int.MinValue) ReleaseGlass();
                micFinger = int.MinValue;
                micHeld = false;
            }
        }

        void Began(V64AFinger f)
        {
            Vector2 w = f.world;
            if (micButton.gameObject.activeSelf && micButtonArea.OverlapPoint(w)) { micFinger = f.id; micHeld = true; return; }
            if (logic.InGrabArea(w.x, w.y))
            {
                CandleEvent ev = logic.FingerDown(f.id, w.x, w.y);
                if (logic.IsFlameFinger(f.id)) { React(ev); return; }
            }
            if (glassFinger == int.MinValue && !pouring && glassItem.Enabled && dragger.TryPick(w) == glassItem)
            {
                glassFinger = f.id;
                Sfx.Play(this, "tick", 0.5f);
                return;
            }
            if (fanArea.OverlapPoint(w)) React(logic.TapFan());
        }

        // ---------- Bicchiere ----------

        bool InPourZone(Vector3 p)
        {
            Vector3 k = wick.position;
            return Mathf.Abs(p.x - k.x) < PourZoneHalfWidth && p.y > k.y - PourZoneBelowWick && p.y < k.y + PourZoneAboveWick;
        }

        void ReleaseGlass()
        {
            glassFinger = int.MinValue;
            HwDragger.Item it = dragger.Release();
            if (it == null) return;
            if (InPourZone(glass.position))
            {
                CandleEvent ev = logic.Pour();
                if (ev != CandleEvent.None) { StartCoroutine(Pour(ev)); return; }
            }
            dragger.SendHome(it, 0.35f);   // lasciato altrove: torna al suo posto
        }

        /// <summary>Il bicchiere si inclina sopra la fiamma, versa, fa vapore e torna a casa pieno (dura PourSeconds).</summary>
        IEnumerator Pour(CandleEvent ev)
        {
            pouring = true;
            glassItem.Enabled = false;
            Vector3 k = wick.position;
            Vector3 from = glass.position, to = new Vector3(k.x - 0.75f, k.y + 1.35f, glass.position.z);
            Quaternion r0 = glass.rotation, r1 = Quaternion.Euler(0f, 0f, -115f);
            for (float t = 0f; t < 1f; t += Time.deltaTime / 0.3f)
            {
                float e = t * t * (3f - 2f * t);
                glass.position = Vector3.Lerp(from, to, e);
                glass.rotation = Quaternion.Slerp(r0, r1, e);
                yield return null;
            }
            glass.position = to;
            glass.rotation = r1;
            if (pourDrops != null) pourDrops.Play();
            V64ASound.Play(this, "glug", 0.6f);
            yield return new WaitForSeconds(0.2f);
            if (ev == CandleEvent.Poured)
            {
                if (steam != null) steam.Play();
                V67BSound.Play(this, "hiss", 0.7f);
                pourFlicker = 1f;
                toast.Show(PourLines[pourLines++ % PourLines.Length], 1.8f);
            }
            else toast.Show("Splash! There's no flame here to drown.", 1.6f);
            yield return new WaitForSeconds(0.45f);
            if (pourDrops != null) pourDrops.Stop();
            yield return new WaitForSeconds(0.15f);
            if (steam != null) steam.Stop();
            // torna a casa dritto (e di nuovo pieno: l'acqua non finisce mai)
            from = glass.position;
            for (float t = 0f; t < 1f; t += Time.deltaTime / 0.3f)
            {
                float e = t * t * (3f - 2f * t);
                glass.position = Vector3.Lerp(from, glassHome, e);
                glass.rotation = Quaternion.Slerp(r1, glassRot, e);
                yield return null;
            }
            glass.position = glassHome;
            glass.rotation = glassRot;
            glassItem.Enabled = true;
            pouring = false;
        }

        // ---------- Reazioni ----------

        void React(CandleEvent ev)
        {
            switch (ev)
            {
                case CandleEvent.Tickled:
                    tickleT = 0.7f;
                    faceT = 0.9f;
                    V67BSound.Play(this, "giggle", 0.6f);
                    Haptics.Tap();
                    if (logic.Tickles <= 3 || logic.Tickles % 3 == 0) say.Show(TickleLines[tickleLines++ % TickleLines.Length], 1.4f);
                    break;
                case CandleEvent.Grabbed:
                    faceT = 1f;
                    Haptics.Light();
                    V67BSound.Play(this, "giggle", 0.45f);
                    if (grabLines < GrabLines.Length) say.Show(GrabLines[grabLines++], 1.4f);
                    break;
                case CandleEvent.SnapBack:
                    V67BSound.Play(this, "boing", 0.6f);
                    tickleT = 0.4f;
                    toast.Show(SnapLines[snapLines++ % SnapLines.Length], 1.6f);
                    break;
                case CandleEvent.Detached:
                    Sfx.Play(this, "pop", 0.9f);
                    Sfx.Play(this, "whoosh", 0.5f);
                    Haptics.Medium();
                    say.Hide();
                    if (wickSmoke != null) wickSmoke.Play();
                    toast.Show("THE CANDLE IS OUT! NOW LOSE THAT FLAME.", 2f);
                    break;
                case CandleEvent.Dropped:
                    V67BSound.Play(this, "giggle", 0.4f);
                    toast.Show("Careful! It wants to go back home.", 1.6f);
                    break;
                case CandleEvent.Regrabbed:
                    Haptics.Light();
                    faceT = 1f;
                    break;
                case CandleEvent.Relit:
                    if (wickSmoke != null) wickSmoke.Stop();
                    V67BSound.Play(this, "fwoosh", 0.4f);
                    say.Show("Home sweet wick!", 1.4f);
                    break;
                case CandleEvent.Escaped:
                    StartCoroutine(Escape());
                    break;
                case CandleEvent.BlowGrow:
                    V67BSound.Play(this, "fwoosh", 0.6f);
                    if (blowLines < BlowLines.Length) toast.Show(BlowLines[blowLines++], 1.6f);
                    break;
                case CandleEvent.BlowTaunt:
                    V67BSound.Play(this, "fwoosh", 0.7f);
                    say.Show("KEEP GOING, I'M GROWING!", 1.8f);
                    break;
                case CandleEvent.FanOn:
                    V67BSound.Play(this, "whirr", 0.5f);
                    Haptics.Tap();
                    StartCoroutine(HalloweenKit.Shake(fan, 0.025f, 0.35f));
                    toast.Show(logic.FlameAttached ? FanLines[fanLines++ % FanLines.Length] : "Whirrr. Nothing to blow on.", 1.8f);
                    break;
                case CandleEvent.FanAgain:
                    V67BSound.Play(this, "whirr", 0.35f);
                    break;
            }
        }

        // ---------- Presentazione ----------

        void Present(float dt)
        {
            float t = Time.time + wobbleSeed;
            flame.position = new Vector3(logic.FlameX, logic.FlameY, flame.position.z);

            // fiammella che tremola; il soffio la fa crescere, la presa la allunga, l'acqua la fa rimpicciolire un attimo
            float grow = logic.Grow, pull = logic.Pull;
            pourFlicker = Mathf.Max(0f, pourFlicker - dt * 1.2f);
            float shrink = 1f - 0.35f * Mathf.Sin(Mathf.Clamp01(pourFlicker) * Mathf.PI);
            float sx = (1f + 0.04f * Mathf.Sin(t * 13f)) * (1f + 0.25f * grow) * (1f - 0.2f * pull) * shrink;
            float sy = (1f + 0.07f * Mathf.Sin(t * 9.7f) + 0.03f * Mathf.Sin(t * 23f)) * (1f + 0.75f * grow) * (1f + 0.8f * pull) * shrink;
            flameBody.localScale = new Vector3(bodyScale.x * sx, bodyScale.y * sy, bodyScale.z);

            // inclinazione: il ventilatore la piega a sinistra; il solletico la fa dimenare; in volo ondeggia
            float target = logic.FanBend ? FanLean + 5f * Mathf.Sin(t * 11f) : 0f;
            lean = Mathf.Lerp(lean, target, 1f - Mathf.Exp(-6f * dt));
            tickleT = Mathf.Max(0f, tickleT - dt);
            float wiggle = tickleT > 0f ? Mathf.Sin(t * 40f) * 14f * Mathf.Min(1f, tickleT / 0.3f) : 0f;
            float sway = logic.CandleLit ? 3f * Mathf.Sin(t * 2.3f) : 10f * Mathf.Sin(t * 5f);
            flameBody.localRotation = Quaternion.Euler(0f, 0f, lean + wiggle + sway);

            // staccata, la fiamma passa davanti a tutto (anche alla scritta di commento)
            FlameOnTop(!logic.CandleLit);

            // faccina: solo mentre la si tocca o la si porta (all'inizio sembra una fiamma normalissima)
            bool touched = logic.FlameFingerCount > 0 || logic.State == CandleState.Carried;
            faceT = touched ? Mathf.Max(faceT, 0.4f) : Mathf.Max(0f, faceT - dt);
            if (giggleFace != null && giggleFace.gameObject.activeSelf != faceT > 0f) HalloweenKit.Show(giggleFace, faceT > 0f);

            if (flameGlow != null)
            {
                float g = (1f + 0.5f * grow) * (0.95f + 0.05f * Mathf.Sin(t * 7f)) * shrink;
                flameGlow.transform.localScale = glowScale * g;
                Color c = glowColor;
                c.a = glowColor.a * (0.85f + 0.15f * Mathf.Sin(t * 5.3f));
                flameGlow.color = c;
            }

            // ventilatore: le pale accelerano e rallentano, le linee del vento compaiono solo mentre gira
            spin = Mathf.MoveTowards(spin, logic.FanOn ? FanSpin : 0f, FanSpin * 1.2f * dt);
            fanBlades.Rotate(0f, 0f, spin * dt);
            if (fanWind != null)
            {
                float k = spin / FanSpin;
                HalloweenKit.SetAlpha(fanWind, 0.85f * k * (0.6f + 0.4f * Mathf.Sin(t * 14f)));
                // il contenitore è specchiato (soffia a sinistra): x locale che cresce = verso la fiamma
                fanWind.transform.localPosition = new Vector3(Mathf.Repeat(t * 1.6f, 0.35f), 0f, 0f);
            }
        }

        void FlameOnTop(bool on)
        {
            if (on == flameOnTop || flameSprites == null) return;
            flameOnTop = on;
            for (int i = 0; i < flameSprites.Length; i++) if (flameSprites[i] != null) flameSprites[i].sortingOrder = flameOrders[i] + (on ? 75 : 0);
        }

        /// <summary>Il pulsante "soffia" compare solo se il microfono non c'è (negato, nessun dispositivo).</summary>
        void ShowMicButton()
        {
            bool show = mic.Unavailable() && logic.FlameAttached;
            if (show != micButton.gameObject.activeSelf)
            {
                HalloweenKit.Show(micButton, show);
                if (show) StartCoroutine(HalloweenKit.Pop(micButton, 1.2f, 0.3f));
                else { micHeld = false; micFinger = int.MinValue; }
            }
            if (show) micButton.localScale = Vector3.one * (micHeld ? 0.9f : 1f);
        }

        // ---------- Vittoria ----------

        IEnumerator Escape()
        {
            busy = true;
            micHeld = false;
            mic.Use(false);
            V64AFingers.Reset();
            if (glassFinger != int.MinValue) { glassFinger = int.MinValue; dragger.Cancel(); }
            HalloweenKit.Show(micButton, false);
            say.Hide();
            if (steam != null) steam.Stop();
            if (wickSmoke != null) wickSmoke.Play();   // la candela resta spenta: filo di fumo
            Haptics.Medium();
            Sfx.Play(this, "whoosh", 0.8f);

            // la fiamma schizza fuori dal bordo in alto
            Vector3 from = flame.position, to = new Vector3(from.x, 6f, from.z);
            Vector3 s0 = flameBody.localScale;
            for (float t = 0f; t < 1f; t += Time.deltaTime / 0.3f)
            {
                flame.position = Vector3.Lerp(from, to, t * t);
                flameBody.localScale = new Vector3(s0.x * (1f - 0.3f * t), s0.y * (1f + 0.6f * t), s0.z);
                yield return null;
            }
            HalloweenKit.Show(flame, false);
            yield return new WaitForSeconds(0.4f);

            // ...e torna a sbirciare dal bordo, linguaccia, poi sparisce per sempre
            float px = Mathf.Clamp(from.x, -1.5f, 1.5f);
            Vector3 up = new Vector3(px, peekHome.y, peekHome.z), down = new Vector3(px, peekHome.y - 1.35f, peekHome.z);
            peek.position = up;
            HalloweenKit.Show(peek, true);
            yield return HalloweenKit.Move(peek, down, 0.3f);
            V67BSound.Play(this, "raspberry", 0.8f);
            yield return HalloweenKit.Wiggle(peek, 10f, 0.7f, 22f);
            yield return HalloweenKit.Move(peek, up, 0.25f);
            HalloweenKit.Show(peek, false);

            toast.Show("CANDLE: OUT. TECHNICALLY.", 2.4f);
            yield return Celebrate(1f);
            if (wickSmoke != null) wickSmoke.Stop();
            if (logic.TryComplete()) Win();
        }

#if UNITY_EDITOR
        // ---------- Tasti di prova nell'Editor ----------

        void EditorKeys()
        {
            const int A = 901, B = 902;
            if (Input.GetKeyDown(KeyCode.F)) React(logic.TapFan());
            if (Input.GetKeyDown(KeyCode.G) && !pouring && glassFinger == int.MinValue)
            {
                CandleEvent ev = logic.Pour();
                if (ev != CandleEvent.None) StartCoroutine(Pour(ev));
            }
            if (Input.GetKeyDown(KeyCode.T) && editorCarry == 0)
            {
                React(logic.FingerDown(A, logic.FlameX, logic.FlameY));
                logic.FingerUp(A);
            }
            if (Input.GetKeyDown(KeyCode.C) && editorCarry == 0 && logic.FlameFingerCount == 0)
            {
                editorCarry = 1;
                React(logic.FingerDown(A, logic.FlameX - 0.2f, logic.FlameY));
                React(logic.FingerDown(B, logic.FlameX + 0.2f, logic.FlameY));
            }
            if (editorCarry > 0)
            {
                editorCarry++;
                float y = flameHome.y + editorCarry * 0.04f;
                React(logic.FingerMove(A, flameHome.x - 0.2f, y));
                if (!busy) React(logic.FingerMove(B, flameHome.x + 0.2f, y));
                if (busy || logic.State != CandleState.Grabbed && logic.State != CandleState.Carried) editorCarry = 0;
            }
        }
#endif
    }

    /// <summary>Suoni sintetizzati del livello 262 (risatina, vampata, ronzio del ventilatore, sfrigolio, "boing", pernacchia).</summary>
    public static class V67BSound
    {
        const int Rate = 22050;
        static readonly Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>();
        static readonly System.Random rnd = new System.Random(67);

        public static void Play(Component owner, string kind, float volume = 0.6f)
        {
            if (owner == null) return;
            AudioClip c = Clip(kind);
            if (c == null) { V64ASound.Play(owner, kind, volume); return; }
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
                case "giggle":   // "hi-hi-hi-hi" acuto che sale
                    c = Make(kind, 0.62f, t =>
                    {
                        float k = t * 7f % 1f;
                        int i = (int)(t * 7f);
                        float f = 900f + 90f * i + 120f * Mathf.Sin(k * Mathf.PI);
                        return Mathf.Sin(t * 2f * Mathf.PI * f) * 0.4f * Mathf.Sin(Mathf.Clamp01(k / 0.7f) * Mathf.PI) * (k < 0.7f ? 1f : 0f);
                    });
                    break;
                case "fwoosh":   // vampata: fruscio che si gonfia
                    {
                        float lp = 0f;
                        c = Make(kind, 0.55f, t => { lp += (N() - lp) * 0.12f; return lp * 1.8f * Mathf.Sin(t / 0.55f * Mathf.PI) * (0.7f + 0.3f * Mathf.Sin(t * 2f * Mathf.PI * 9f)); });
                    }
                    break;
                case "whirr":    // ventilatore che parte e gira
                    c = Make(kind, 1.2f, t =>
                    {
                        float f = Mathf.Lerp(60f, 140f, Mathf.Clamp01(t / 0.4f));
                        return (Mathf.Sin(t * 2f * Mathf.PI * f) * 0.3f + N() * 0.12f) * Mathf.Min(1f, t * 8f) * Mathf.Min(1f, (1.2f - t) * 3f);
                    });
                    break;
                case "hiss":     // acqua sulla fiamma
                    c = Make(kind, 0.8f, t => N() * 0.45f * Mathf.Exp(-t * 3.5f) * Mathf.Min(1f, t * 40f));
                    break;
                case "boing":
                    c = Make(kind, 0.4f, t => Mathf.Sin(t * 2f * Mathf.PI * (180f + 80f * Mathf.Sin(t * 2f * Mathf.PI * 14f))) * 0.45f * Mathf.Exp(-t * 6f));
                    break;
                case "raspberry": // pernacchia: dente di sega basso con tremolio
                    c = Make(kind, 0.6f, t =>
                    {
                        float saw = 2f * (t * 95f % 1f) - 1f;
                        return (saw * 0.4f + N() * 0.15f) * (0.55f + 0.45f * Mathf.Sin(t * 2f * Mathf.PI * 32f)) * Mathf.Min(1f, t * 30f) * Mathf.Min(1f, (0.6f - t) * 8f);
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
            AudioClip clip = AudioClip.Create("v67_" + name, n, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
