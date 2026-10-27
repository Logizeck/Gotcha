using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels.V64;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 253 "The Fish That Is Afraid of Water" — "Save the fish before it drowns!" Una boccia tonda piena fino al
    /// collo, un pesce arancione che si agita, il misuratore dell'ossigeno, un secchio e una pompa. Il pesce però è un
    /// giocattolo gonfiabile: corpo rigido, una valvolina sulla schiena, squittisce quando sbatte sul vetro o lo si
    /// tocca, galleggia come un tappo. La pompa aggiunge acqua ("MORE WATER. BRILLIANT."), versare l'acqua inclinando
    /// di lato lo lascia sospeso a mezz'aria ("HE'S STILL DROWNING. SOMEHOW."), soffiare a valvola chiusa fa solo bolle
    /// ("THAT DIDN'T HELP."). Soluzione: allargare la valvola con due dita (resta aperta), capovolgere il telefono (il
    /// pesce sale nella bolla d'aria con la valvola fuori dall'acqua) e soffiare per circa due secondi in tutto.
    /// Senza microfono compare un pulsante col microfono da tenere premuto.
    /// La logica è in Gotcha.Levels.V64.PlasticFishLogic (senza Unity): qui solo input e presentazione.
    /// </summary>
    public class V64PlasticFishLevel : V62Level
    {
        [SerializeField] Transform bowl;               // centro della sfera della boccia
        [SerializeField] Transform waterPivot;         // ruota perché il pelo dell'acqua resti orizzontale
        [SerializeField] Transform water;              // rettangolo d'acqua (mascherato), il bordo alto è il pelo
        [SerializeField] Transform waterLine;          // riga chiara sul pelo dell'acqua
        [SerializeField] Transform fish;
        [SerializeField] Transform fishBody;           // per schiacciarlo e gonfiarlo
        [SerializeField] Collider2D fishArea;
        [SerializeField] Transform valve;
        [SerializeField] SpriteRenderer valveArt;
        [SerializeField] Sprite valveClosed;
        [SerializeField] Sprite valveOpen;
        [SerializeField] SmokeEmitter blowBubbles;     // bolle del soffio, alla bocca del pesce
        [SerializeField] SmokeEmitter tankBubbles;     // bollicine decorative
        [SerializeField] SmokeEmitter spill;           // l'acqua che esce dal collo
        [SerializeField] Transform neck;               // bocca della boccia
        [SerializeField] Transform[] shards;           // vetri che saltano via alla fine
        [SerializeField] Transform pump;
        [SerializeField] Transform pumpHandle;
        [SerializeField] Collider2D pumpArea;
        [SerializeField] Transform bucket;
        [SerializeField] Collider2D bucketArea;
        [SerializeField] Transform oxygenFill;         // scala x = ossigeno
        [SerializeField] TextMesh oxygenText;
        [SerializeField] Transform micButton;          // alternativa al microfono (tenere premuto)
        [SerializeField] Collider2D micButtonArea;
        [SerializeField] TextMesh infoText;
        [SerializeField] Transform infoHolder;

        const float BowlRadius = 1.5f;                 // raggio interno della sfera (mondo)
        const float FishHalfWidth = 0.55f;
        const float ValveGrab = 0.95f;                 // dita entro questa distanza dalla valvola la "prendono"
        const float StretchNeeded = 0.4f;              // di quanto si devono allontanare le due dita
        const float SidewaysOn = 0.65f, SidewaysOff = 0.45f, SidewaysDelay = 0.6f;

        static readonly string[] SqueakLines = { "SQUEAK!", "Squeak? ...squeak.", "That was a very rubbery squeak." };
        static readonly string[] PumpLines = { "MORE WATER. BRILLIANT.", "EVEN MORE WATER. GENIUS.", "He's a fish, not a sponge." };

        readonly PlasticFishLogic logic = new PlasticFishLogic();
        readonly V64AGravity gravity = new V64AGravity();
        readonly Dictionary<int, Vector2> valveFingers = new Dictionary<int, Vector2>();
        V64AMic mic;
        readonly V64AMicGate gate = new V64AMicGate();
        Camera cam;
        HwToast toast;
        Vector3 fishScale, valveScale, handleHome, bucketHome;
        float shownAngle, lateral, lateralTarget, nextDart, squeakReady, sidewaysFor, stretchStart, valveOpenedAt;
        float lastBlowSound, nextTankBubbleCheck, squash = 1f, facing = 1f;
        bool sideways, stretching, micHeld, busy;
        int squeakLines, pumpLines;
        int micFinger = int.MinValue;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            logic.Reset();
            gravity.Reset();
            gate.Reset();
            ResetV62();
            V64AFingers.Reset();
            mic = new V64AMic(this);
            mic.Use(true);   // soffiare è una delle cose da provare fin dall'inizio
            toast = new HwToast(infoText, infoHolder);
            fishScale = fishBody.localScale;
            valveScale = valve.localScale;
            handleHome = pumpHandle.localPosition;
            bucketHome = bucket.localPosition;
            valveArt.sprite = valveClosed;
            shownAngle = 0f;
            lateral = 0f;
            lateralTarget = 0.3f;
            nextDart = Time.time + 0.5f;
            if (blowBubbles != null) blowBubbles.Stop();
            if (spill != null) spill.Stop();
            if (tankBubbles != null) tankBubbles.Play();
            if (shards != null) foreach (Transform s in shards) HalloweenKit.Show(s, false);
            HalloweenKit.Show(micButton, false);
            PlaceFishAndWater(0f);
        }

        void OnDisable() { if (mic != null) mic.Use(false); }
        void OnDestroy() { if (mic != null) mic.Use(false); }

        void Update()
        {
            toast.Tick();
            if (IsFinished || logic.InputLocked) return;
            if (Time.timeScale <= 0f)   // pausa o pubblicità: niente input, sensori né microfono
            {
                valveFingers.Clear();
                micHeld = false;
                micFinger = int.MinValue;
                return;
            }
            float dt = Time.deltaTime;
            Vector2 tilt = Inputs.Tilt();
            gravity.Feed(tilt.x, tilt.y, dt);

            // versare di lato: serve un'inclinazione forte e un po' di pazienza (girare il telefono non conta)
            bool side = !gravity.Inverted && gravity.Y < 0.35f && Mathf.Abs(gravity.X) > (sideways ? SidewaysOff : SidewaysOn);
            sidewaysFor = side ? sidewaysFor + dt : 0f;
            sideways = side && sidewaysFor >= SidewaysDelay;

            ReadFingers();
            bool blowing = gate.Feed(mic.Level(), dt) || micHeld;
            FishEvent ev = logic.Tick(dt, gravity.Inverted, sideways, blowing);
            React(ev, blowing);
            if (ev == FishEvent.Inflated) { StartCoroutine(Inflate()); return; }

            PlaceFishAndWater(dt);
            ShowMeters();
            ShowMicButton();
        }

        // ---------- Dita ----------

        void ReadFingers()
        {
            List<V64AFinger> fingers = V64AFingers.Read(cam);
            foreach (V64AFinger f in fingers)
            {
                if (f.began)
                {
                    if (micButton.gameObject.activeSelf && micButtonArea.OverlapPoint(f.world)) { micFinger = f.id; micHeld = true; }
                    else if (Vector2.Distance(f.world, valve.position) < ValveGrab || fishArea.OverlapPoint(f.world))
                    {
                        valveFingers[f.id] = f.world;
                        if (valveFingers.Count == 1) TouchFish(Vector2.Distance(f.world, valve.position) < 0.45f);
                    }
                    else if (!busy) Tap(f.world);
                }
                else if (valveFingers.ContainsKey(f.id)) valveFingers[f.id] = f.world;
                if (f.ended)
                {
                    valveFingers.Remove(f.id);
                    if (f.id == micFinger) { micFinger = int.MinValue; micHeld = false; }
                }
            }
            if (micFinger != int.MinValue && fingers.Count == 0) { micFinger = int.MinValue; micHeld = false; }

            // due dita sulla valvola che si allontanano: si apre (e resta aperta)
            if (valveFingers.Count < 2)
            {
                if (stretching && !logic.ValveOpen) valve.localScale = valveScale;
                stretching = false;
                return;
            }
            float d = PairDistance();
            if (!stretching) { stretching = true; stretchStart = d; return; }
            float k = Mathf.Clamp01((d - stretchStart) / StretchNeeded);
            if (!logic.ValveOpen) valve.localScale = valveScale * (1f + 0.6f * k);
            if (k >= 1f && logic.OpenValve()) ValveOpened();
        }

        float PairDistance()
        {
            Vector2 a = Vector2.zero, b = Vector2.zero;
            int i = 0;
            foreach (KeyValuePair<int, Vector2> kv in valveFingers) { if (i == 0) a = kv.Value; else if (i == 1) b = kv.Value; i++; }
            return Vector2.Distance(a, b);
        }

        void TouchFish(bool onValve)
        {
            int n = logic.TapFish();
            V64ASound.Play(this, "squeak", 0.7f);
            Haptics.Tap();
            StartCoroutine(Squash());
            if (onValve && !logic.ValveOpen) { StartCoroutine(HalloweenKit.Wiggle(valve, 20f, 0.3f, 40f)); toast.Show("The tiny valve won't budge.", 1.4f); return; }
            if (n <= 3 || n % 4 == 0) toast.Show(SqueakLines[squeakLines++ % SqueakLines.Length], 1.4f);
        }

        IEnumerator Squash()
        {
            for (squash = 0f; squash < 1f; squash += Time.deltaTime / 0.2f) yield return null;
            squash = 1f;
        }

        void Tap(Vector2 w)
        {
            if (pumpArea.OverlapPoint(w))
            {
                if (!logic.Pump()) return;
                StartCoroutine(PumpStroke());
                StartCoroutine(HalloweenKit.Shake(pump, 0.03f, 0.25f));
                V64ASound.Play(this, "glug", 0.6f);
                toast.Show(PumpLines[pumpLines++ % PumpLines.Length], 1.6f);
                return;
            }
            if (bucketArea.OverlapPoint(w))
            {
                Sfx.Play(this, "clunk", 0.5f);
                StartCoroutine(HalloweenKit.Wiggle(bucket, 6f, 0.35f));
                toast.Show("The bucket is ALSO full of water.", 1.6f);
            }
        }

        IEnumerator PumpStroke()
        {
            for (float t = 0f; t < 1f; t += Time.deltaTime / 0.25f)
            {
                pumpHandle.localPosition = handleHome + Vector3.down * Mathf.Sin(t * Mathf.PI) * 0.35f;
                yield return null;
            }
            pumpHandle.localPosition = handleHome;
        }

        void ValveOpened()
        {
            valveArt.sprite = valveOpen;
            valveOpenedAt = Time.time;
            valve.localScale = valveScale * 1.8f;
            Sfx.Play(this, "pop", 0.8f);
            V64ASound.Play(this, "inflate", 0.4f);
            Haptics.Light();
            StartCoroutine(HalloweenKit.Pop(valve, 1.3f, 0.25f));
            toast.Show("*pssst* The valve is open.", 1.8f);
        }

        // ---------- Reazioni ----------

        void React(FishEvent ev, bool blowing)
        {
            if (blowBubbles != null)
            {
                bool bubbling = blowing && !(logic.ValveOpen && logic.ValveExposed);
                if (bubbling) blowBubbles.Play(); else blowBubbles.Stop();
                blowBubbles.riseSpeed = gravity.Inverted ? -0.8f : 0.8f;
            }
            if (blowing && Time.time > lastBlowSound + 0.45f)
            {
                lastBlowSound = Time.time;
                V64ASound.Play(this, logic.ValveOpen && logic.ValveExposed ? "inflate" : "bubbles", 0.45f);
            }
            switch (ev)
            {
                case FishEvent.BlowBubbles:
                    toast.Show("THAT DIDN'T HELP.", 1.6f);
                    break;
                case FishEvent.BlowUnderwater:
                    toast.Show("Blub. You're blowing bubbles at a valve. Underwater.", 1.8f);
                    break;
                case FishEvent.Drowning:
                    Sfx.Play(this, "bonk", 0.5f);
                    toast.Show("HE'S STILL DROWNING. SOMEHOW.", 2f);
                    break;
                case FishEvent.Refilled:
                    V64ASound.Play(this, "glug", 0.5f);
                    break;
                case FishEvent.FloatedUp:
                    V64ASound.Play(this, "squeak", 0.5f);
                    toast.Show("Bloop. He floats like a cork.", 1.6f);
                    break;
                case FishEvent.OxygenEmpty:
                    toast.Show("OXYGEN: 0%. FISH: SUSPICIOUSLY FINE.", 2f);
                    break;
            }
        }

        // ---------- Presentazione ----------

        void PlaceFishAndWater(float dt)
        {
            // il pelo dell'acqua resta perpendicolare alla gravità (filtrata); telefono piatto = angolo di prima
            shownAngle = Mathf.LerpAngle(shownAngle, gravity.AngleDeg, 1f - Mathf.Exp(-8f * dt));
            waterPivot.localRotation = Quaternion.Euler(0f, 0f, shownAngle);
            float h = FishBowlGeometry.SurfaceHeight(logic.Water);
            float surface = Mathf.Min(h, 1.35f) * BowlRadius;
            water.localPosition = new Vector3(0f, surface - water.localScale.y * 0.5f, 0f);
            waterLine.localPosition = new Vector3(0f, surface, 0f);

            // il pesce: galleggia sul pelo dell'acqua (o resta sotto il vetro), si agita di lato come un giocattolo
            float y = FishBowlGeometry.FishHeight(logic.Water, logic.StuckMidBowl) * BowlRadius;
            float half = Mathf.Max(0f, Mathf.Sqrt(Mathf.Max(0f, BowlRadius * BowlRadius - y * y)) - FishHalfWidth);
            float frantic = logic.StuckMidBowl ? 0.15f : 1f + 1.5f * logic.Distress;
            if (Time.time > nextDart)
            {
                nextDart = Time.time + Random.Range(0.35f, 0.9f) / frantic;
                lateralTarget = Random.Range(-1f, 1f) * (half + 0.2f);
            }
            float before = lateral;
            lateral = Mathf.MoveTowards(lateral, lateralTarget, 2.2f * frantic * dt);
            if (Mathf.Abs(lateral) > half)
            {
                lateral = Mathf.Sign(lateral) * half;
                lateralTarget = -lateralTarget * 0.6f;
                if (Mathf.Abs(before) < half + 0.001f && Time.time > squeakReady)
                {
                    squeakReady = Time.time + 0.8f;
                    V64ASound.Play(this, "squeak", 0.35f);   // sbatte sul vetro: squittisce come la gomma
                }
            }
            float bob = Mathf.Sin(Time.time * 3.1f) * 0.035f;
            Quaternion rot = Quaternion.Euler(0f, 0f, shownAngle);
            Vector3 local = rot * new Vector3(lateral, y + bob, 0f);
            fish.position = new Vector3(bowl.position.x + local.x, bowl.position.y + local.y, fish.position.z);
            float jitter = Mathf.Sin(Time.time * 9f) * 4f * Mathf.Min(1.5f, frantic);
            fish.localRotation = Quaternion.Euler(0f, 0f, shownAngle + jitter);
            if (Mathf.Abs(lateralTarget - lateral) > 0.05f) facing = lateralTarget > lateral ? -1f : 1f;   // guarda dove va
            // corpo rigido (nessuna coda che si piega): solo lo schiacciamento quando lo si tocca e il gonfiore
            float sq = Mathf.Sin(Mathf.Clamp01(squash) * Mathf.PI) * 0.12f;
            float inflate = 1f + 0.25f * logic.BlowFraction;
            fishBody.localScale = new Vector3(Mathf.Abs(fishScale.x) * facing * (1f + sq), fishScale.y * (1f - sq), fishScale.z) * inflate;

            if (tankBubbles != null && Time.time > nextTankBubbleCheck)
            {
                nextTankBubbleCheck = Time.time + 0.5f;
                if (Mathf.Abs(Mathf.DeltaAngle(shownAngle, 0f)) < 45f && logic.Water >= 0.95f) tankBubbles.Play(); else tankBubbles.Stop();
            }
            if (spill != null)
            {
                bool pouring = (sideways || (gravity.Inverted && logic.Water > PlasticFishLogic.InvertedFill + 0.01f) || logic.Water > PlasticFishLogic.FullFill + 0.01f)
                    && logic.State == FishState.Swimming;
                if (pouring) spill.Play(); else spill.Stop();
                spill.riseSpeed = gravity.Inverted ? 1.6f : -1.6f;
                spill.sideDrift = Mathf.Abs(gravity.X) > 0.4f ? 1.2f : 0.3f;
            }
        }

        void ShowMeters()
        {
            float o = Mathf.Clamp01(logic.Oxygen / 100f);
            oxygenFill.localScale = new Vector3(o, 1f, 1f);
            oxygenText.text = "O2 " + Mathf.CeilToInt(logic.Oxygen) + "%";
        }

        /// <summary>Il pulsante col microfono: se il microfono non c'è, o dopo un po' che la valvola è aperta.</summary>
        void ShowMicButton()
        {
            bool show = mic.Unavailable() || (logic.ValveOpen && Time.time - valveOpenedAt > 20f);
            if (show != micButton.gameObject.activeSelf)
            {
                HalloweenKit.Show(micButton, show);
                if (show) StartCoroutine(HalloweenKit.Pop(micButton, 1.2f, 0.3f));
                else micHeld = false;
            }
            if (show) micButton.localScale = Vector3.one * (micHeld ? 0.9f : 1f);
        }

        IEnumerator Inflate()
        {
            busy = true;
            micHeld = false;
            mic.Use(false);
            HalloweenKit.Show(micButton, false);
            if (blowBubbles != null) blowBubbles.Stop();
            if (spill != null) spill.Stop();
            if (tankBubbles != null) tankBubbles.Stop();
            V64ASound.Play(this, "inflate", 0.8f);
            Haptics.Medium();
            // si gonfia come un pallone e si raddrizza verso la bocca della boccia
            Vector3 from = fishBody.localScale, big = new Vector3(Mathf.Abs(fishScale.x) * 2.6f, fishScale.y * 3.4f, 1f);
            Vector3 start = fish.position, mid = new Vector3(bowl.position.x, bowl.position.y + 0.6f, start.z);
            Quaternion r0 = fish.localRotation;
            for (float t = 0f; t < 1f; t += Time.deltaTime / 0.8f)
            {
                float e = t * t * (3f - 2f * t);
                fishBody.localScale = Vector3.Lerp(from, big, e) + new Vector3(0.05f, -0.05f, 0f) * Mathf.Sin(t * 30f);
                fish.position = Vector3.Lerp(start, mid, e);
                fish.localRotation = Quaternion.Slerp(r0, Quaternion.identity, e);
                yield return null;
            }
            // sfonda la bocca della boccia
            Sfx.Play(this, "clunk", 0.9f);
            Sfx.Play(this, "pop", 1f);
            Haptics.Heavy();
            if (shards != null)
                for (int i = 0; i < shards.Length; i++)
                {
                    HalloweenKit.Show(shards[i], true);
                    shards[i].position = neck.position;
                    float side = (i - (shards.Length - 1) * 0.5f) * 0.9f;
                    StartCoroutine(HalloweenKit.Move(shards[i], neck.position + new Vector3(side, -1.4f - 0.3f * (i % 2), 0f), 0.6f, 1.1f));
                    StartCoroutine(HalloweenKit.Wiggle(shards[i], 90f, 0.6f, 12f));
                }
            // e vola via sgonfiandosi, a zig-zag
            V64ASound.Play(this, "deflate", 0.9f);
            toast.Show("YOU SAVED A PLASTIC FISH.", 2.4f);
            Vector3 p0 = fish.position;
            for (float t = 0f; t < 1f; t += Time.deltaTime / 1.3f)
            {
                fish.position = p0 + new Vector3(Mathf.Sin(t * 22f) * 0.6f * (1f - t * 0.3f), t * 7f, 0f);
                fish.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * 30f) * 35f);
                fishBody.localScale = Vector3.Lerp(big, big * 0.55f, t);
                yield return null;
            }
            HalloweenKit.Show(fish, false);
            toast.Show("THE OCEAN IS VERY PROUD.", 2.4f);
            yield return Celebrate(1.4f);
            if (logic.TryComplete()) Win();
        }
    }
}
