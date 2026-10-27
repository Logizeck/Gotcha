using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels.V64;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 254 "The Neighbour's Alarm" — "Turn off that damn alarm!" Camera da letto di notte: qualcuno dorme, sul
    /// comodino una sveglia che squilla (le onde partono da lì), un cuscino, una biglia per terra. La sveglia è una
    /// falsa pista: toccarla cambia l'ora e squilla più forte, tenerla la zittisce ma il suono continua dal pavimento,
    /// il cuscino la attutisce soltanto, scuotere il telefono la fa cadere. Il suono viene dal vicino di sotto.
    /// Soluzione: spostare il comodino con due dita (con un dito solo non si muove: è pesante) e scoprire la crepa,
    /// far rotolare la biglia nella crepa inclinando il telefono, poi un solo suono forte nel microfono: l'onda d'urto
    /// fa rimbalzare la biglia sul tasto SNOOZE del telefono del vicino. Poi canta il gallo.
    /// Il microfono si accende solo nell'ultima fase; senza microfono compare un pulsante "BANG!".
    /// La logica è in Gotcha.Levels.V64.NeighboursAlarmLogic (senza Unity): qui solo input e presentazione.
    /// </summary>
    public class V64NeighboursAlarmLevel : V62Level
    {
        [SerializeField] SpriteRenderer sleeperArt;
        [SerializeField] Sprite sleeperAsleep;
        [SerializeField] Sprite sleeperAwake;
        [SerializeField] Transform sleeperHead;        // dove finisce il cuscino nell'urlo finale
        [SerializeField] SmokeEmitter zzz;
        [SerializeField] Transform table;              // il comodino (si sposta solo in x)
        [SerializeField] Collider2D tableArea;
        [SerializeField] Transform tableArt;
        [SerializeField] Transform clock;
        [SerializeField] Collider2D clockArea;
        [SerializeField] Transform hourHand;
        [SerializeField] Transform minuteHand;
        [SerializeField] SmokeEmitter clockWaves;
        [SerializeField] SmokeEmitter floorWaves;
        [SerializeField] Transform pillow;
        [SerializeField] Collider2D pillowArea;
        [SerializeField] Transform marble;
        [SerializeField] Collider2D marbleArea;
        [SerializeField] Transform crack;
        [SerializeField] Collider2D crackArea;
        [SerializeField] Transform peephole;           // lo spioncino sul piano di sotto (nascosto all'inizio)
        [SerializeField] Transform neighbourPhone;
        [SerializeField] TextMesh neighbourScreen;
        [SerializeField] Transform snoozeTarget;
        [SerializeField] Transform insetMarble;
        [SerializeField] Transform shockRing;
        [SerializeField] SmokeEmitter phoneWaves;
        [SerializeField] Transform rooster;
        [SerializeField] SpeechBubble roosterBubble;
        [SerializeField] Transform micButton;
        [SerializeField] Collider2D micButtonArea;
        [SerializeField] TextMesh infoText;
        [SerializeField] Transform infoHolder;

        const float FloorY = -3.3f;              // centro della biglia sul pavimento
        const float ShakeLevel = 14f;
        const float TapTime = 0.3f, HoldTime = 0.35f, TapSlop = 0.3f;
        const float PillowSnap = 0.8f;

        static readonly string[] TapLines = { "Snooze? No. LOUDER.", "Now it's 9 o'clock. Still ringing.", "You changed the time. Not the noise." };
        static readonly string[] HeavyLines = { "It's heavier than it looks.", "Nope. Solid oak. Probably.", "One finger? For THAT?" };

        sealed class Grip { public Vector2 start, last; public float t0; public bool onClock, onTable, onPillow, moved; }

        readonly NeighboursAlarmLogic logic = new NeighboursAlarmLogic();
        readonly V64AGravity gravity = new V64AGravity();
        readonly V64AImpulse impulse = new V64AImpulse();
        readonly Dictionary<int, Grip> grips = new Dictionary<int, Grip>();
        readonly List<int> gone = new List<int>(4);
        V64AMic mic;
        Camera cam;
        HwToast toast;
        HwSay roosterSay;
        Vector3 pillowHome, clockLocalHome, marbleScale, clockScale, snoozeScale;
        float nextRing, heavyReady, scrapeReady, shakeReady, micStageAt, marbleSpin;
        bool busy, pillowHeld, dragging;
        int tapLines, heavyLines;
        Vector2 pillowOffset;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            logic.Reset();
            gravity.Reset();
            impulse.Reset();
            ResetV62();
            V64AFingers.Reset();
            mic = new V64AMic(this);
            toast = new HwToast(infoText, infoHolder);
            roosterSay = new HwSay(roosterBubble);
            pillowHome = pillow.position;
            clockLocalHome = clock.localPosition;
            clockScale = clock.localScale;
            marbleScale = marble.localScale;
            snoozeScale = snoozeTarget.localScale;
            sleeperArt.sprite = sleeperAsleep;
            HalloweenKit.Show(peephole, false);
            HalloweenKit.Show(rooster, false);
            HalloweenKit.Show(shockRing, false);
            HalloweenKit.Show(micButton, false);
            if (zzz != null) zzz.Play();
            if (clockWaves != null) clockWaves.Play();
            if (floorWaves != null) floorWaves.Play();
            if (phoneWaves != null) phoneWaves.Stop();
            nextRing = Time.time + 0.3f;
            ShowClockTime();
            PlaceTableAndMarble();
        }

        void OnDisable() { if (mic != null) mic.Use(false); grips.Clear(); }
        void OnDestroy() { if (mic != null) mic.Use(false); }

        void Update()
        {
            toast.Tick();
            roosterSay.Tick();
            if (IsFinished || logic.InputLocked) return;
            if (Time.timeScale <= 0f) { grips.Clear(); if (pillowHeld) DropPillow(); return; }   // pausa o pubblicità: niente input, sensori né microfono
            float dt = Time.deltaTime;
            Vector2 tilt = Inputs.Tilt();
            gravity.Feed(tilt.x, tilt.y, dt);
            RingLoop();

            if (!busy && Time.time > shakeReady && Inputs.Shake() > ShakeLevel)
            {
                shakeReady = Time.time + 2f;
                StartCoroutine(ShakeReaction(logic.Shake()));
            }

            if (!busy) ReadFingers();

            if (!busy)
            {
                MarbleEvent me = logic.StepMarble(gravity.X, dt);
                if (me == MarbleEvent.Bump) { Sfx.Play(this, "tick", 0.6f); Haptics.Tap(); }
                if (me == MarbleEvent.Fell) { StartCoroutine(MarbleFalls()); return; }
            }
            PlaceTableAndMarble();
            ShowWaves();
            MicStage(dt);
        }

        // ---------- Squillo ----------

        void RingLoop()
        {
            if (!logic.Ringing || Time.time < nextRing) return;
            nextRing = Time.time + 1.1f;
            bool muffled = logic.ClockHeld || logic.PillowOnClock || logic.ClockFallen;
            float loud = 0.2f + 0.1f * (logic.Loudness - 1);
            // la sveglia "squilla", ma il suono vero viene da sotto: tenuta o coperta, resta solo quello attutito
            if (!muffled) V64ASound.Play(this, "ring", loud);
            V64ASound.Play(this, "ring_low", muffled ? 0.55f : 0.25f);
            if (!busy && !logic.ClockHeld && !logic.PillowOnClock && !logic.ClockFallen) StartCoroutine(HalloweenKit.Shake(clock, 0.03f, 0.35f));
            if (logic.Stage == AlarmStage.Neighbour) StartCoroutine(HalloweenKit.Shake(neighbourPhone, 0.025f, 0.4f));
        }

        void ShowWaves()
        {
            bool muffled = logic.ClockHeld || logic.PillowOnClock;
            if (clockWaves != null)
            {
                if (logic.ClockHeld || !logic.Ringing) clockWaves.Stop(); else clockWaves.Play();
                clockWaves.startScale = logic.PillowOnClock ? 0.12f : 0.25f + 0.06f * logic.Loudness;
                clockWaves.transform.position = clock.position + new Vector3(0f, 0.95f * clockScale.y, 0f);
            }
            if (floorWaves != null)
            {
                floorWaves.tint = new Color(1f, 1f, 1f, muffled || logic.ClockFallen || logic.TableMoved ? 0.85f : 0.28f);
                floorWaves.puffsPerSecond = muffled ? 4f : 1.5f;
            }
        }

        void ShowClockTime()
        {
            float m = logic.ClockMinute / 60f;
            float h = ((logic.ClockHour % 12) + m) / 12f;
            minuteHand.localRotation = Quaternion.Euler(0f, 0f, -360f * m);
            hourHand.localRotation = Quaternion.Euler(0f, 0f, -360f * h);
        }

        // ---------- Dita ----------

        void ReadFingers()
        {
            List<V64AFinger> fingers = V64AFingers.Read(cam);
            int onTable = 0;
            Vector2 tableDelta = Vector2.zero;
            foreach (V64AFinger f in fingers)
            {
                if (f.began)
                {
                    var g = new Grip { start = f.world, last = f.world, t0 = Time.time };
                    if (micButton.gameObject.activeSelf && micButtonArea.OverlapPoint(f.world)) { MicButtonPressed(); continue; }
                    if (!pillowHeld && pillowArea.OverlapPoint(f.world) && grips.Count == 0)
                    {
                        g.onPillow = true;
                        pillowHeld = true;
                        pillowOffset = (Vector2)pillow.position - f.world;
                        if (logic.PillowOnClock) logic.SetPillowOnClock(false);
                        Haptics.Tap();
                    }
                    else
                    {
                        g.onClock = clockArea.OverlapPoint(f.world);
                        g.onTable = tableArea.OverlapPoint(f.world) || (g.onClock && !logic.ClockFallen);   // la sveglia sul comodino fa parte del comodino
                        if (!g.onClock && !g.onTable) TapScene(f.world);
                    }
                    grips[f.id] = g;
                }
                if (!grips.TryGetValue(f.id, out Grip gr)) continue;
                Vector2 d = f.world - gr.last;
                gr.last = f.world;
                if (Vector2.Distance(f.world, gr.start) > TapSlop) gr.moved = true;
                if (gr.onPillow && !f.ended) pillow.position = new Vector3(f.world.x + pillowOffset.x, f.world.y + pillowOffset.y, pillow.position.z);
                if (gr.onTable) { onTable++; tableDelta += d; }
                if (f.ended) EndGrip(f.id, gr, f.world);
            }
            // dita perse senza "fine" (es. tocco annullato)
            if (fingers.Count == 0 && grips.Count > 0)
            {
                gone.Clear();
                foreach (int id in grips.Keys) gone.Add(id);
                foreach (int id in gone) EndGrip(id, grips[id], grips[id].last);
            }

            // due dita sul comodino: si sposta (pesante); il dito sulla sveglia smette di "tenerla"
            int tableGrips = 0;
            foreach (Grip g in grips.Values) if (g.onTable) tableGrips++;
            dragging = tableGrips >= 2;
            if (dragging)
            {
                if (logic.ClockHeld) logic.HoldClock(false);
                float dx = onTable > 0 ? tableDelta.x / onTable : 0f;
                if (Mathf.Abs(dx) > 0.0001f)
                {
                    float before = logic.TableOffset;
                    if (logic.DragTable(dx)) CrackRevealed();
                    if (Mathf.Abs(logic.TableOffset - before) > 0.0001f && Time.time > scrapeReady)
                    {
                        scrapeReady = Time.time + 0.3f;
                        V64ASound.Play(this, "scrape", 0.6f);
                        Haptics.Tap();
                    }
                }
                return;
            }
            // un dito solo: la sveglia (tocco o pressione lunga) oppure il comodino che non si muove
            foreach (KeyValuePair<int, Grip> kv in grips)
            {
                Grip g = kv.Value;
                if (g.onClock && !g.moved && Time.time - g.t0 >= HoldTime && !logic.ClockHeld)
                {
                    logic.HoldClock(true);
                    Haptics.Light();
                    toast.Show("Silence? ...no. It's still ringing. Somewhere.", 1.8f);
                }
                if (g.onTable && !g.onClock && g.moved && Time.time > heavyReady)
                {
                    heavyReady = Time.time + 1.8f;
                    logic.PushWithOneFinger();
                    StartCoroutine(HalloweenKit.Shake(tableArt, 0.03f, 0.25f));
                    Sfx.Play(this, "bonk", 0.4f);
                    toast.Show(HeavyLines[heavyLines++ % HeavyLines.Length], 1.6f);
                }
            }
        }

        void EndGrip(int id, Grip g, Vector2 at)
        {
            grips.Remove(id);
            if (g.onPillow) { DropPillow(); return; }
            if (g.onClock)
            {
                if (logic.ClockHeld) { logic.HoldClock(false); return; }
                if (!dragging && !g.moved && Time.time - g.t0 <= TapTime + 0.2f) TapClock();
            }
        }

        void TapClock()
        {
            if (!logic.TapClock()) return;
            ShowClockTime();
            V64ASound.Play(this, "ring", 0.25f + 0.1f * logic.Loudness);
            StartCoroutine(HalloweenKit.Pop(clock, 1.15f, 0.2f));
            Haptics.Light();
            toast.Show(TapLines[tapLines++ % TapLines.Length], 1.6f);
        }

        void DropPillow()
        {
            pillowHeld = false;
            if (Vector2.Distance(pillow.position, clock.position + Vector3.up * 0.35f) < PillowSnap)
            {
                logic.SetPillowOnClock(true);
                Sfx.Play(this, "bonk", 0.4f);
                toast.Show("Muffled. Not stopped. Mostly just a fluffier noise.", 1.8f);
                return;
            }
            StartCoroutine(HalloweenKit.Move(pillow, pillowHome, 0.3f, 0.3f));
        }

        void TapScene(Vector2 w)
        {
            if (marbleArea.OverlapPoint(w))
            {
                StartCoroutine(HalloweenKit.Pop(marble, 1.15f, 0.2f));
                Sfx.Play(this, "tick", 0.6f);
                toast.Show("It won't roll for fingers. Only for slopes.", 1.6f);
                return;
            }
            if (logic.TableMoved && crackArea.OverlapPoint(w))
            {
                V64ASound.Play(this, "ring_low", 0.5f);
                toast.Show("The crack is... ringing?", 1.5f);
                return;
            }
            if (sleeperArt != null && Vector2.Distance(w, sleeperArt.transform.position) < 1.2f)
            {
                Sfx.Play(this, "snore", 0.5f);
                toast.Show("Zzz... make it stop... zzz...", 1.5f);
            }
        }

        void CrackRevealed()
        {
            Sfx.Play(this, "clunk", 0.6f);
            Haptics.Medium();
            StartCoroutine(HalloweenKit.Pop(crack, 1.3f, 0.3f));
            toast.Show("A crack in the floor. And it's RINGING.", 2f);
        }

        // ---------- Posizioni ----------

        void PlaceTableAndMarble()
        {
            Vector3 tp = table.position;
            table.position = new Vector3(logic.TableX, tp.y, tp.z);
            if (logic.PillowOnClock && !pillowHeld) pillow.position = new Vector3(clock.position.x, clock.position.y + 0.42f, pillow.position.z);
            if (logic.Stage == AlarmStage.Neighbour) return;
            Vector3 mp = marble.position;
            float x = logic.MarbleX;
            marbleSpin -= (x - mp.x) / 0.15f * Mathf.Rad2Deg;
            marble.position = new Vector3(x, FloorY, mp.z);
            marble.localRotation = Quaternion.Euler(0f, 0f, marbleSpin);
        }

        // ---------- Scossone ----------

        IEnumerator ShakeReaction(bool clockFalls)
        {
            busy = clockFalls;
            Haptics.Medium();
            Sfx.Play(this, "clunk", 0.6f);
            StartCoroutine(HalloweenKit.Shake(tableArt, 0.08f, 0.5f));
            if (!clockFalls)
            {
                toast.Show("Rattle rattle. Still ringing.", 1.5f);
                yield break;
            }
            grips.Clear();
            pillowHeld = false;
            if (pillow.position != pillowHome) StartCoroutine(HalloweenKit.Move(pillow, pillowHome, 0.35f, 0.4f));
            yield return HalloweenKit.Shake(clock, 0.1f, 0.4f);
            clock.SetParent(transform, true);
            Vector3 to = new Vector3(logic.TableX - 0.95f, FloorY - 0.15f, clock.position.z);
            for (float t = 0f; t < 1f; t += Time.deltaTime / 0.45f)
            {
                clock.position = Vector3.Lerp(clock.position, to, t) + Vector3.up * Mathf.Sin(t * Mathf.PI) * 0.05f;
                clock.localRotation = Quaternion.Euler(0f, 0f, 80f * t);
                yield return null;
            }
            clock.position = to;
            clock.localRotation = Quaternion.Euler(0f, 0f, 80f);
            Sfx.Play(this, "bonk", 0.8f);
            Haptics.Light();
            toast.Show("The clock fell. The ringing didn't.", 1.8f);
            busy = false;
        }

        // ---------- La biglia e il vicino ----------

        IEnumerator MarbleFalls()
        {
            busy = true;
            grips.Clear();
            pillowHeld = false;
            Vector3 c = new Vector3(NeighboursAlarmLogic.CrackX, FloorY, marble.position.z);
            yield return HalloweenKit.Move(marble, c, 0.15f);
            for (float t = 0f; t < 1f; t += Time.deltaTime / 0.3f)
            {
                marble.position = c + Vector3.down * 0.3f * t;
                marble.localScale = marbleScale * (1f - t);
                yield return null;
            }
            HalloweenKit.Show(marble, false);
            V64ASound.Play(this, "thuds", 0.8f);
            yield return new WaitForSeconds(0.6f);
            // si vede il piano di sotto: il telefono del vicino, SNOOZE e la biglia appena atterrata
            HalloweenKit.Show(peephole, true);
            Vector3 s = peephole.localScale;
            peephole.localScale = s * 0.2f;
            Sfx.Play(this, "pop", 0.7f);
            yield return HalloweenKit.ScaleTo(peephole, s, 0.25f);
            if (phoneWaves != null) phoneWaves.Play();
            toast.Show("Downstairs! One good BANG and that marble would hit SNOOZE.", 2.6f);
            micStageAt = Time.time;
            impulse.Reset();
            busy = false;
        }

        void MicStage(float dt)
        {
            mic.Use(logic.NeedsMic);   // il microfono solo in questa fase
            if (!logic.NeedsMic)
            {
                HalloweenKit.Show(micButton, false);
                return;
            }
            float pulse = 1f + 0.12f * Mathf.Sin(Time.time * 6f);
            snoozeTarget.localScale = snoozeScale * pulse;
            if (busy) return;
            if (impulse.Feed(mic.Level(), dt)) { Shockwave(); return; }
            bool show = mic.Unavailable() || Time.time - micStageAt > 8f;
            if (show && !micButton.gameObject.activeSelf)
            {
                HalloweenKit.Show(micButton, true);
                StartCoroutine(HalloweenKit.Pop(micButton, 1.2f, 0.3f));
            }
        }

        void MicButtonPressed()
        {
            if (logic.NeedsMic && !busy) Shockwave();
        }

        void Shockwave()
        {
            if (!logic.LoudSound()) return;
            StartCoroutine(Snooze());
        }

        IEnumerator Snooze()
        {
            busy = true;
            mic.Use(false);
            HalloweenKit.Show(micButton, false);
            // l'onda d'urto da cartone: la biglia rimbalza sul tasto SNOOZE
            V64ASound.Play(this, "boom", 0.9f);
            Haptics.Heavy();
            HalloweenKit.Show(shockRing, true);
            shockRing.position = insetMarble.position;
            SpriteRenderer ring = shockRing.GetComponentInChildren<SpriteRenderer>();
            for (float t = 0f; t < 1f; t += Time.deltaTime / 0.35f)
            {
                shockRing.localScale = Vector3.one * Mathf.Lerp(0.1f, 0.9f, t);
                HalloweenKit.SetAlpha(ring, 1f - t);
                yield return null;
            }
            HalloweenKit.Show(shockRing, false);
            yield return HalloweenKit.Move(insetMarble, snoozeTarget.position, 0.4f, 0.6f);
            Sfx.Play(this, "tick", 0.9f);
            if (phoneWaves != null) phoneWaves.Stop();
            if (clockWaves != null) clockWaves.Stop();
            if (floorWaves != null) floorWaves.Stop();
            neighbourScreen.text = "SNOOZED";
            HalloweenKit.Show(snoozeTarget, false);
            toast.Show("ALARM SNOOZED — 5 MINUTES.", 2.2f);
            Haptics.Success();
            yield return new WaitForSeconds(1f);
            // il gallo
            HalloweenKit.Show(rooster, true);
            Vector3 rs = rooster.localScale;
            rooster.localScale = rs * 0.2f;
            yield return HalloweenKit.ScaleTo(rooster, rs, 0.15f);
            V64ASound.Play(this, "crow", 1f);
            roosterSay.Show("COCK-A-DOODLE-DOO!", 2f);
            StartCoroutine(HalloweenKit.Wiggle(rooster, 8f, 1.2f, 20f));
            // chi dormiva urla in silenzio nel cuscino
            if (zzz != null) zzz.Stop();
            sleeperArt.sprite = sleeperAwake;
            StartCoroutine(HalloweenKit.Move(pillow, sleeperHead.position, 0.25f, 0.3f));
            yield return HalloweenKit.Shake(sleeperArt.transform, 0.06f, 0.9f);
            yield return Celebrate(1.2f);
            if (logic.TryComplete()) Win();
        }
    }
}
