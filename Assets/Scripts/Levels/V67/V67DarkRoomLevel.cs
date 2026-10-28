using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels.V64;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 261 "The Dark Room" — "Turn on the light." Soggiorno al buio: il dito è una piccola torcia (una
    /// SpriteMask che segue il dito sopra il velo scuro; senza dito si intuiscono appena le sagome). Nel buio: un
    /// interruttore (clic... niente: la lampadina non c'è), la lampada del soffitto, una lampada da comodino staccata con
    /// la spina per terra (il filo è rosicchiato, il topo lì accanto fa il vago), fiammiferi bagnati e una candela, un
    /// orologio che segna le 2 del pomeriggio, una radio che parla di un bel pomeriggio di sole, e le tende lunghe con una
    /// sottilissima riga di luce lungo il bordo (l'indizio, visibile anche senza torcia).
    /// Soluzione: aprire le tende (trascinarne una di lato, oppure due dita che le allargano, oppure tirare il cordino)
    /// oltre una soglia generosa: entra il sole. Era giorno.
    /// Niente microfono né sensori. La logica è in Gotcha.Levels.V64.DarkRoomLogic (senza Unity): qui solo input e
    /// presentazione. Tasti di prova nell'Editor: K apre la tenda a scatti, 1-6 le false piste.
    /// </summary>
    public class V67DarkRoomLevel : V62Level
    {
        [Header("Buio e torcia")]
        [SerializeField] SpriteRenderer darkness;      // velo scuro (visibile fuori dalla maschera)
        [SerializeField] Transform torch;              // segue il dito: dentro ha la SpriteMask e l'alone morbido
        [SerializeField] SpriteRenderer torchRing;     // alone morbido (visibile dentro la maschera)
        [SerializeField] SpriteRenderer edgeLine;      // la riga di luce tra le tende (sopra il buio)
        [SerializeField] SpriteRenderer edgeGlow;
        [SerializeField] SpriteRenderer floorSpill;    // la lama di luce sotto le tende
        [Header("Tende")]
        [SerializeField] Transform curtainLeft;
        [SerializeField] Transform curtainRight;
        [SerializeField] Transform curtainLeftArt;     // scala x: la stoffa si raccoglie aprendo
        [SerializeField] Transform curtainRightArt;
        [SerializeField] Collider2D curtainLeftArea;
        [SerializeField] Collider2D curtainRightArea;
        [SerializeField] Transform cordTassel;
        [SerializeField] Collider2D cordArea;
        [SerializeField] LineRenderer cordLine;
        [Header("False piste")]
        [SerializeField] Collider2D switchArea;
        [SerializeField] SpriteRenderer switchArt;
        [SerializeField] Sprite switchOffSprite;
        [SerializeField] Sprite switchOnSprite;
        [SerializeField] Transform ceilingLampPivot;
        [SerializeField] SpriteRenderer ceilingLampArt;
        [SerializeField] Collider2D ceilingLampArea;
        [SerializeField] Transform tableLamp;
        [SerializeField] Collider2D tableLampArea;
        [SerializeField] Transform plug;
        [SerializeField] Collider2D plugArea;
        [SerializeField] Transform socket;
        [SerializeField] Transform chew;               // il punto rosicchiato del filo
        [SerializeField] LineRenderer plugCable;       // dal morso alla spina (segue la spina)
        [SerializeField] SmokeEmitter sparks;
        [SerializeField] Transform mouse;
        [SerializeField] Collider2D mouseArea;
        [SerializeField] Transform matchbox;
        [SerializeField] Collider2D matchboxArea;
        [SerializeField] Transform candle;
        [SerializeField] Collider2D candleArea;
        [SerializeField] SmokeEmitter fizzle;
        [SerializeField] Transform radio;
        [SerializeField] Collider2D radioArea;
        [SerializeField] SmokeEmitter radioWaves;
        [SerializeField] SpeechBubble radioBubble;
        [SerializeField] Transform clock;
        [SerializeField] Collider2D clockArea;
        [Header("Il giorno")]
        [SerializeField] SpriteRenderer sunbeam;
        [SerializeField] Transform sun;
        [SerializeField] Transform bird;
        [SerializeField] TextMesh infoText;
        [SerializeField] Transform infoHolder;

        const float DarkAlpha = 0.93f;
        const float TapTime = 0.45f, TapSlop = 0.25f;
        const float PlugSnap = 0.8f, MatchSnap = 0.9f;
        const float CurtainW = 1.25f;                  // larghezza di ogni metà della tenda chiusa
        const float CordMaxPull = 0.7f;

        static readonly string[] SwitchLines = { "Click. Click. Click. Still dark.", "Flicking harder won't help.", "The switch works. The bulb is on holiday." };
        static readonly string[] LampLines = { "Unplugged.", "Still unplugged. The plug is on the floor.", "A lamp without power. So... a hat stand." };
        static readonly string[] PlugLines = { "ZZT! Somebody chewed the cable.", "ZZT! Still chewed. Still dangerous.", "Stop it. The mouse is getting nervous." };
        static readonly string[] MatchLines = { "Wet matches. Of course.", "Fsss... nope. Soggy.", "Who keeps matches in the sink?!" };
        static readonly string[] RadioLines = { "...and it's a beautiful sunny afternoon...", "(birds chirping)", "...not a cloud in the sky, folks..." };
        static readonly string[] ClockLines = { "Tick. Tock. 2 PM.", "2 PM. The little sun means AFTERNOON.", "Still 2 PM. Time flies in the dark." };
        static readonly string[] CurtainLines = { "The edge is... glowing?", "Something bright is behind there." };
        static readonly string[] MouseLines = { "Squeak. (It wasn't me.)", "*whistles innocently*", "The mouse avoids eye contact." };

        enum Kind { None, CurtainLeft, CurtainRight, Cord, Plug, Matchbox, Other }

        sealed class Grip { public Kind kind; public Vector2 start, last; public float t0; public bool moved; }

        readonly DarkRoomLogic logic = new DarkRoomLogic();
        readonly Dictionary<int, Grip> grips = new Dictionary<int, Grip>();
        readonly List<int> gone = new List<int>(4);
        readonly Dictionary<Transform, Coroutine> wobbles = new Dictionary<Transform, Coroutine>();
        Camera cam;
        HwToast toast;
        HwSay radioSay;
        HwDragger dragger;
        HwDragger.Item plugItem, matchItem;
        Vector3 leftHome, rightHome, leftArtScale, rightArtScale, tasselHome, sunScale, birdScale;
        float seamX, cordPull, edgeAlpha, lineWidth, nextShimmer;
        int switchLines, lampLines, plugLines, matchLines, radioLines, clockLines, curtainLines, mouseLines;
        int lampOrder;
        bool busy, finishing;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            logic.Reset();
            ResetV62();
            V64AFingers.Reset();
            grips.Clear();
            busy = finishing = false;
            wobbles.Clear();
            switchLines = lampLines = plugLines = matchLines = radioLines = clockLines = curtainLines = mouseLines = 0;
            toast = new HwToast(infoText, infoHolder);
            radioSay = new HwSay(radioBubble);
            dragger = new HwDragger(this);
            plugItem = dragger.Add(plug, plugArea, 2);
            matchItem = dragger.Add(matchbox, matchboxArea, 1);
            leftHome = curtainLeft.position;
            rightHome = curtainRight.position;
            seamX = (leftHome.x + rightHome.x) * 0.5f;
            leftArtScale = curtainLeftArt.localScale;
            rightArtScale = curtainRightArt.localScale;
            tasselHome = cordTassel.position;
            sunScale = sun.localScale;
            birdScale = bird.localScale;
            edgeAlpha = edgeLine.color.a;
            lineWidth = edgeLine.transform.localScale.x;
            lampOrder = ceilingLampArt.sortingOrder;
            cordPull = 0f;
            switchArt.sprite = switchOffSprite;
            HalloweenKit.Show(torch, false);
            HalloweenKit.Show(darkness, true);
            HalloweenKit.Show(edgeLine, true);
            HalloweenKit.Show(edgeGlow, true);
            HalloweenKit.Show(floorSpill, true);
            HalloweenKit.Show(sun, false);
            HalloweenKit.Show(bird, false);
            HalloweenKit.SetAlpha(sunbeam, 0f);
            HalloweenKit.Show(sunbeam, false);
            if (sparks != null) sparks.Stop();
            if (fizzle != null) fizzle.Stop();
            if (radioWaves != null) radioWaves.Stop();
            ShowCurtains();
            ShowPlugCable();
        }

        void OnDisable()
        {
            CancelGrips();
            // le animazioni si fermano con l'oggetto: niente prese bloccate al ritorno
            busy = false;
            wobbles.Clear();
            if (ceilingLampArt != null && lampOrder != 0) ceilingLampArt.sortingOrder = lampOrder;
            if (plugItem != null) { plugItem.Enabled = true; if (plug != null) plug.position = plugItem.Home; }
        }

        void OnEnable()
        {
            // se il livello è stato spento a metà della festa finale, la si riprende (vittoria comunque una volta sola)
            if (logic.Stage == DarkStage.Sunlit && !IsFinished && cam != null) StartCoroutine(Daylight());
        }
        void OnApplicationPause(bool paused) { if (paused) CancelGrips(); }
        void OnApplicationFocus(bool focus) { if (!focus) CancelGrips(); }

        /// <summary>Pausa, perdita del focus, pubblicità: si lasciano le prese; le tende restano dove sono (stato valido).</summary>
        void CancelGrips()
        {
            grips.Clear();
            if (dragger != null && dragger.Held != null) dragger.Cancel();
            if (torch != null) HalloweenKit.Show(torch, false);
            if (cordTassel != null && cordPull != 0f) { cordPull = 0f; ShowCurtains(); }
        }

        void Update()
        {
            toast?.Tick();
            radioSay?.Tick();
            if (IsFinished || finishing || logic.InputLocked) return;
            if (Time.timeScale <= 0f) { CancelGrips(); return; }   // pausa o pubblicità: niente input
#if UNITY_EDITOR
            EditorKeys();
            if (logic.InputLocked) return;
#endif
            ReadFingers();
            if (logic.InputLocked) return;
            ShowCurtains();
            ShowPlugCable();
        }

        // ---------- Dita ----------

        void ReadFingers()
        {
            List<V64AFinger> fingers = V64AFingers.Read(cam);
            // la torcia segue il primo dito ancora giù
            bool lit = false;
            foreach (V64AFinger f in fingers)
            {
                if (f.ended) continue;
                torch.position = new Vector3(f.world.x, f.world.y, torch.position.z);
                lit = true;
                break;
            }
            HalloweenKit.Show(torch, lit);

            foreach (V64AFinger f in fingers)
            {
                if (f.began) grips[f.id] = Begin(f.world);
                if (!grips.TryGetValue(f.id, out Grip g)) continue;
                Vector2 d = f.world - g.last;
                g.last = f.world;
                if (Vector2.Distance(f.world, g.start) > TapSlop) g.moved = true;
                Drag(g, f.world, d);
                if (logic.InputLocked) return;
                if (f.ended) End(f.id, g);
            }
            // dita perse senza "fine" (tocco annullato)
            if (fingers.Count == 0 && grips.Count > 0)
            {
                gone.Clear();
                foreach (int id in grips.Keys) gone.Add(id);
                foreach (int id in gone) End(id, grips[id]);
            }
        }

        bool SideHeld(Kind k)
        {
            foreach (Grip g in grips.Values) if (g.kind == k) return true;
            return false;
        }

        Grip Begin(Vector2 w)
        {
            var g = new Grip { start = w, last = w, t0 = Time.time, kind = Kind.Other };
            if (busy) { g.kind = Kind.None; return g; }
            // prima le cose che si trascinano (la spina e i fiammiferi), poi il cordino, poi le tende
            if (dragger.Held == null)
            {
                HwDragger.Item it = dragger.TryPick(w);
                if (it != null) { g.kind = it == plugItem ? Kind.Plug : Kind.Matchbox; return g; }
            }
            if (switchArea.OverlapPoint(w)) { FlipSwitch(); return g; }   // l'interruttore resta raggiungibile anche con la tenda aperta a metà
            if (cordArea.OverlapPoint(w) && !SideHeld(Kind.Cord)) { g.kind = Kind.Cord; Haptics.Tap(); return g; }
            bool onLeft = curtainLeftArea.OverlapPoint(w), onRight = curtainRightArea.OverlapPoint(w);
            if (onLeft && onRight) { if (w.x < seamX) onRight = false; else onLeft = false; }
            if (onLeft && !SideHeld(Kind.CurtainLeft)) { g.kind = Kind.CurtainLeft; return g; }
            if (onRight && !SideHeld(Kind.CurtainRight)) { g.kind = Kind.CurtainRight; return g; }
            if (onLeft || onRight) { g.kind = Kind.None; return g; }   // un secondo dito sulla stessa metà: niente
            TapScene(w);
            return g;
        }

        void Drag(Grip g, Vector2 w, Vector2 d)
        {
            switch (g.kind)
            {
                case Kind.Plug:
                case Kind.Matchbox:
                    dragger.MoveHeld(w);
                    break;
                case Kind.CurtainLeft:
                    if (Mathf.Abs(d.x) > 0.0001f && logic.DragCurtain(CurtainSide.Left, d.x)) StartCoroutine(Daylight());
                    break;
                case Kind.CurtainRight:
                    if (Mathf.Abs(d.x) > 0.0001f && logic.DragCurtain(CurtainSide.Right, d.x)) StartCoroutine(Daylight());
                    break;
                case Kind.Cord:
                    // chi parte dal cordino ma trascina di lato vuole spostare la tenda destra: diventa quella presa
                    float side = w.x - g.start.x;
                    if (cordPull < 0.05f && Mathf.Abs(side) > TapSlop && Mathf.Abs(side) > g.start.y - w.y)
                    {
                        cordPull = 0f;
                        g.kind = SideHeld(Kind.CurtainRight) ? Kind.None : Kind.CurtainRight;
                        if (g.kind == Kind.CurtainRight && logic.DragCurtain(CurtainSide.Right, side)) StartCoroutine(Daylight());
                        break;
                    }
                    // il cordino scende col dito (fino a un certo punto) e apre le tende
                    float pull = Mathf.Clamp(g.start.y - w.y, 0f, CordMaxPull);
                    float dy = cordPull - pull;   // negativo quando si tira più giù
                    cordPull = pull;
                    if (dy < -0.0001f && logic.PullCord(dy)) StartCoroutine(Daylight());
                    break;
            }
        }

        void End(int id, Grip g)
        {
            grips.Remove(id);
            bool tap = !g.moved && Time.time - g.t0 <= TapTime;
            switch (g.kind)
            {
                case Kind.Plug:
                    DropPlug();
                    break;
                case Kind.Matchbox:
                    DropMatches(tap);
                    break;
                case Kind.Cord:
                    cordPull = 0f;   // il cordino torna su da solo; le tende restano dove sono
                    if (tap) TapCurtain(curtainRight);
                    break;
                case Kind.CurtainLeft:
                    if (tap) TapCurtain(curtainLeft);
                    break;
                case Kind.CurtainRight:
                    if (tap) TapCurtain(curtainRight);
                    break;
            }
        }

        // ---------- Tocchi sulle false piste ----------

        void TapScene(Vector2 w)
        {
            if (ceilingLampArea.OverlapPoint(w)) { SwingLamp(false); toast.Show("No bulb up there. Just a sad little socket.", 1.8f); return; }
            if (tableLampArea.OverlapPoint(w)) { TapLamp(); return; }
            if (radioArea.OverlapPoint(w)) { TapRadio(); return; }
            if (clockArea.OverlapPoint(w)) { TapClock(); return; }
            if (mouseArea.OverlapPoint(w)) { TapMouse(); return; }
            if (candleArea.OverlapPoint(w))
            {
                Wobble(candle, 6f, 0.4f, 25f);
                toast.Show("A candle. Now all you need is fire.", 1.6f);
            }
        }

        void FlipSwitch()
        {
            SwitchReaction r = logic.FlipSwitch();
            if (r == SwitchReaction.None) return;
            switchArt.sprite = logic.SwitchOn ? switchOnSprite : switchOffSprite;
            V67DarkRoomSound.Play(this, "click", 0.7f);
            Haptics.Tap();
            switch (r)
            {
                case SwitchReaction.Nothing:
                    toast.Show("Click... nothing.", 1.5f);
                    break;
                case SwitchReaction.BulbGone:
                    SwingLamp(true);
                    toast.Show("Bulb's gone.", 1.8f);
                    break;
                default:
                    toast.Show(SwitchLines[switchLines++ % SwitchLines.Length], 1.6f);
                    break;
            }
        }

        /// <summary>La lampada del soffitto oscilla e per un attimo si vede (sopra il buio): il portalampada è vuoto.</summary>
        void SwingLamp(bool reveal)
        {
            if (reveal) ceilingLampArt.sortingOrder = 56;
            Wobble(ceilingLampPivot, reveal ? 14f : 8f, reveal ? 1.6f : 0.9f, 6f);
        }

        /// <summary>Oscillazione che non si accumula: una nuova annulla la precedente e alla fine torna dritto.</summary>
        void Wobble(Transform t, float degrees, float seconds, float speed)
        {
            if (t == null || !isActiveAndEnabled) return;
            if (wobbles.TryGetValue(t, out Coroutine c) && c != null) StopCoroutine(c);
            t.localRotation = Quaternion.identity;
            wobbles[t] = StartCoroutine(WobbleRun(t, degrees, seconds, speed));
        }

        IEnumerator WobbleRun(Transform t, float degrees, float seconds, float speed)
        {
            for (float k = 0f; k < seconds; k += Time.deltaTime)
            {
                t.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(k * speed) * degrees * (1f - k / seconds));
                yield return null;
            }
            t.localRotation = Quaternion.identity;
            wobbles.Remove(t);
            if (t == ceilingLampPivot) ceilingLampArt.sortingOrder = lampOrder;
        }

        void TapLamp()
        {
            int n = logic.TapLamp();
            if (n == 0) return;
            V67DarkRoomSound.Play(this, "click", 0.5f);
            Wobble(tableLamp, 5f, 0.35f, 40f);
            toast.Show(LampLines[(n - 1) % LampLines.Length], 1.6f);
        }

        void TapRadio()
        {
            int n = logic.TapRadio();
            if (n == 0) return;
            string line = RadioLines[radioLines++ % RadioLines.Length];
            bool birds = line.StartsWith("(");
            V67DarkRoomSound.Play(this, birds ? "birds" : "static", 0.6f);
            Wobble(radio, 4f, 0.5f, 30f);
            if (radioWaves != null) StartCoroutine(HalloweenKit.Burst(radioWaves, 0.8f));
            radioSay.Show(line, 2.4f);
        }

        void TapClock()
        {
            int n = logic.TapClock();
            if (n == 0) return;
            V67DarkRoomSound.Play(this, "ticktock", 0.6f);
            Wobble(clock, 6f, 0.6f, 14f);
            toast.Show(ClockLines[clockLines++ % ClockLines.Length], 1.8f);
        }

        void TapMouse()
        {
            if (logic.TapMouse() == 0) return;
            V64ASound.Play(this, "squeak", 0.5f);
            Wobble(mouse, 8f, 0.6f, 25f);
            toast.Show(MouseLines[mouseLines++ % MouseLines.Length], 1.6f);
        }

        void TapCurtain(Transform panel)
        {
            if (logic.TapCurtain() == 0) return;
            Sfx.Play(this, "whoosh", 0.3f);
            Wobble(panel, 1.5f, 0.4f, 20f);
            nextShimmer = Time.time + 0.6f;   // la riga di luce brilla un attimo di più
            toast.Show(CurtainLines[curtainLines++ % CurtainLines.Length], 1.6f);
        }

        // ---------- La spina e i fiammiferi ----------

        void DropPlug()
        {
            dragger.Release();
            if (Vector2.Distance(plug.position, socket.position) < PlugSnap && logic.TryPlug() > 0) { StartCoroutine(Sparks()); return; }
            dragger.SendHome(plugItem, 0.3f);
        }

        IEnumerator Sparks()
        {
            busy = true;
            plugItem.Enabled = false;
            yield return HalloweenKit.Move(plug, socket.position + new Vector3(-0.25f, 0f, 0f), 0.12f);
            V67DarkRoomSound.Play(this, "zap", 0.8f);
            Haptics.Medium();
            if (sparks != null)
            {
                sparks.transform.position = chew.position;
                StartCoroutine(HalloweenKit.Burst(sparks, 0.5f));
            }
            StartCoroutine(HalloweenKit.Shake(chew, 0.05f, 0.4f));
            toast.Show(PlugLines[plugLines++ % PlugLines.Length], 1.8f);
            yield return new WaitForSeconds(0.45f);
            // il topo fa finta di niente
            Wobble(mouse, 10f, 0.8f, 22f);
            V64ASound.Play(this, "squeak", 0.35f);
            dragger.SendHome(plugItem, 0.35f);
            yield return new WaitForSeconds(0.4f);
            plugItem.Enabled = true;
            busy = false;
        }

        void DropMatches(bool tap)
        {
            dragger.Release();
            bool onCandle = Vector2.Distance(matchbox.position, candle.position + Vector3.up * 0.3f) < MatchSnap;
            if ((onCandle || tap) && logic.TryMatches() > 0)
            {
                V67DarkRoomSound.Play(this, "fizzle", 0.6f);
                if (fizzle != null)
                {
                    fizzle.transform.position = onCandle ? candle.position + Vector3.up * 0.7f : matchbox.position + Vector3.up * 0.2f;
                    StartCoroutine(HalloweenKit.Burst(fizzle, 0.5f));
                }
                toast.Show(MatchLines[matchLines++ % MatchLines.Length], 1.6f);
            }
            dragger.SendHome(matchItem, 0.3f);
        }

        // ---------- Presentazione delle tende ----------

        void ShowCurtains()
        {
            float lo = logic.LeftOpen, ro = logic.RightOpen;
            // ogni metà: il bordo interno si sposta verso l'esterno, la stoffa si raccoglie un po'
            float lk = 1f - 0.45f * lo / DarkRoomLogic.MaxSlide, rk = 1f - 0.45f * ro / DarkRoomLogic.MaxSlide;
            curtainLeftArt.localScale = new Vector3(leftArtScale.x * lk, leftArtScale.y, leftArtScale.z);
            curtainRightArt.localScale = new Vector3(rightArtScale.x * rk, rightArtScale.y, rightArtScale.z);
            float lInner = seamX - lo, rInner = seamX + ro;
            curtainLeft.position = new Vector3(lInner - CurtainW * lk * 0.5f, leftHome.y, leftHome.z);
            curtainRight.position = new Vector3(rInner + CurtainW * rk * 0.5f, rightHome.y, rightHome.z);

            // la riga di luce si allarga con lo spiraglio; il buio si schiarisce un po'
            float gap = logic.Gap;
            Transform lt = edgeLine.transform;
            lt.position = new Vector3((lInner + rInner) * 0.5f, lt.position.y, lt.position.z);
            lt.localScale = new Vector3(lineWidth + gap, lt.localScale.y, 1f);
            float shimmer = 0.85f + 0.15f * Mathf.Sin(Time.time * 2.2f) + (Time.time < nextShimmer ? 0.3f : 0f);
            HalloweenKit.SetAlpha(edgeLine, Mathf.Clamp01(edgeAlpha * shimmer));
            Transform gt = edgeGlow.transform;
            gt.position = new Vector3(lt.position.x, gt.position.y, gt.position.z);
            gt.localScale = new Vector3(0.9f + gap * 1.6f, gt.localScale.y, 1f);
            Transform ft = floorSpill.transform;
            ft.position = new Vector3(lt.position.x, ft.position.y, ft.position.z);
            float frac = logic.OpenFraction;
            HalloweenKit.SetAlpha(darkness, DarkAlpha * (1f - 0.45f * frac));
            HalloweenKit.SetAlpha(torchRing, darkness.color.a);

            // il cordino
            cordTassel.position = tasselHome + Vector3.down * cordPull;
            if (cordLine != null) cordLine.SetPosition(1, cordTassel.position);
        }

        void ShowPlugCable()
        {
            if (plugCable == null) return;
            Vector3 a = chew.position + new Vector3(0.17f, 0f, 0f);
            Vector3 b = plug.TransformPoint(new Vector3(-0.28f, -0.02f, 0f));
            Vector3 mid = (a + b) * 0.5f + Vector3.down * Mathf.Min(0.25f, Vector3.Distance(a, b) * 0.15f);
            plugCable.SetPosition(0, a);
            plugCable.SetPosition(1, mid);
            plugCable.SetPosition(2, b);
        }

        // ---------- Era giorno ----------

        IEnumerator Daylight()
        {
            finishing = true;
            grips.Clear();
            if (dragger.Held != null) dragger.Cancel();
            HalloweenKit.Show(torch, false);
            cordPull = 0f;
            Sfx.Play(this, "whoosh", 0.7f);
            Haptics.Medium();
            // le tende si spalancano
            float l0 = logic.LeftOpen, r0 = logic.RightOpen;
            Vector3 lFrom = curtainLeft.position, rFrom = curtainRight.position;
            Vector3 lTo = new Vector3(seamX - DarkRoomLogic.MaxSlide - CurtainW * 0.55f * 0.5f, lFrom.y, lFrom.z);
            Vector3 rTo = new Vector3(seamX + DarkRoomLogic.MaxSlide + CurtainW * 0.55f * 0.5f, rFrom.y, rFrom.z);
            Vector3 lsFrom = curtainLeftArt.localScale, rsFrom = curtainRightArt.localScale;
            Vector3 lsTo = new Vector3(leftArtScale.x * 0.55f, leftArtScale.y, leftArtScale.z), rsTo = new Vector3(rightArtScale.x * 0.55f, rightArtScale.y, rightArtScale.z);
            HalloweenKit.Show(sunbeam, true);
            float startDark = darkness.color.a;
            for (float t = 0f; t < 1f; t += Time.deltaTime / 0.45f)
            {
                float e = 1f - (1f - t) * (1f - t);
                curtainLeft.position = Vector3.Lerp(lFrom, lTo, e);
                curtainRight.position = Vector3.Lerp(rFrom, rTo, e);
                curtainLeftArt.localScale = Vector3.Lerp(lsFrom, lsTo, e);
                curtainRightArt.localScale = Vector3.Lerp(rsFrom, rsTo, e);
                HalloweenKit.SetAlpha(darkness, Mathf.Lerp(startDark, startDark * 0.5f, e));
                cordTassel.position = tasselHome;
                if (cordLine != null) cordLine.SetPosition(1, cordTassel.position);
                yield return null;
            }
            HalloweenKit.Show(edgeLine, false);
            HalloweenKit.Show(edgeGlow, false);
            HalloweenKit.Show(floorSpill, false);
            // il sole inonda la stanza, il buio sparisce, cantano gli uccellini
            V67DarkRoomSound.Play(this, "birds", 0.8f);
            HalloweenKit.Show(sun, true);
            sun.localScale = sunScale * 0.2f;
            StartCoroutine(HalloweenKit.ScaleTo(sun, sunScale, 0.35f));
            HalloweenKit.Show(bird, true);
            bird.localScale = birdScale * 0.2f;
            StartCoroutine(HalloweenKit.ScaleTo(bird, birdScale, 0.25f));
            toast.Show("IT WAS DAYTIME.", 1.4f);
            float d0 = darkness.color.a;
            for (float t = 0f; t < 1f; t += Time.deltaTime / 0.9f)
            {
                HalloweenKit.SetAlpha(darkness, Mathf.Lerp(d0, 0f, t));
                HalloweenKit.SetAlpha(sunbeam, Mathf.Lerp(0f, 0.55f, t));
                yield return null;
            }
            HalloweenKit.SetAlpha(darkness, 0f);
            HalloweenKit.Show(darkness, false);
            HalloweenKit.SetAlpha(sunbeam, 0.55f);
            Wobble(bird, 10f, 1.2f, 16f);
            Wobble(sun, 6f, 1.6f, 6f);
            yield return new WaitForSeconds(0.5f);
            toast.Show("YOU'RE WELCOME, SUN.", 1.8f);
            yield return Celebrate(1.1f);
            if (logic.TryComplete()) Win();
        }

#if UNITY_EDITOR
        // ---------- Tasti di prova nell'Editor ----------

        void EditorKeys()
        {
            if (busy) return;
            if (Input.GetKeyDown(KeyCode.K) && logic.DragCurtain(CurtainSide.Right, 0.25f)) { StartCoroutine(Daylight()); return; }
            if (Input.GetKeyDown(KeyCode.Alpha1)) FlipSwitch();
            if (Input.GetKeyDown(KeyCode.Alpha2)) TapLamp();
            if (Input.GetKeyDown(KeyCode.Alpha3) && dragger.Held == null && logic.TryPlug() > 0) StartCoroutine(Sparks());
            if (Input.GetKeyDown(KeyCode.Alpha4)) DropMatches(true);
            if (Input.GetKeyDown(KeyCode.Alpha5)) TapRadio();
            if (Input.GetKeyDown(KeyCode.Alpha6)) TapClock();
        }
#endif
    }

    /// <summary>Suoni sintetizzati del livello 261: clic dell'interruttore, scintille, fiammifero bagnato, radio, uccellini.</summary>
    public static class V67DarkRoomSound
    {
        const int Rate = 22050;
        static readonly Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>();
        static readonly System.Random rnd = new System.Random(67);

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
                case "click":    // interruttore: due scatti secchi
                    c = Make(kind, 0.09f, t => (N() * 0.6f + Mathf.Sin(t * 2f * Mathf.PI * 2400f) * 0.4f) * (Mathf.Exp(-t * 400f) + (t > 0.05f ? Mathf.Exp(-(t - 0.05f) * 500f) * 0.6f : 0f)));
                    break;
                case "zap":      // scintille: crepitio elettrico
                    c = Make(kind, 0.5f, t => (Mathf.Sign(Mathf.Sin(t * 2f * Mathf.PI * 120f)) * 0.25f + N() * 0.5f)
                        * (Mathf.Sin(t * 2f * Mathf.PI * 37f) > 0.2f ? 1f : 0.25f) * Mathf.Exp(-t * 5f));
                    break;
                case "fizzle":   // fiammifero bagnato: "fsss" che si spegne
                    {
                        float lp = 0f;
                        c = Make(kind, 0.6f, t => { lp += (N() - lp) * 0.5f; return lp * 0.6f * Mathf.Min(1f, t * 40f) * Mathf.Exp(-t * 6f); });
                    }
                    break;
                case "static":   // radio: fruscio e una vocina che borbotta
                    c = Make(kind, 0.9f, t => N() * 0.12f + Saw(t, 160f + 40f * Mathf.Sin(t * 2f * Mathf.PI * 5f)) * 0.18f
                        * Mathf.Abs(Mathf.Sin(t * 2f * Mathf.PI * 4f)) * Mathf.Min(1f, (0.9f - t) * 10f));
                    break;
                case "birds":    // cinguettii: fischi brevi che salgono e scendono
                    c = Make(kind, 1.2f, t =>
                    {
                        float v = 0f;
                        float[] at = { 0f, 0.14f, 0.28f, 0.6f, 0.72f, 0.95f };
                        for (int i = 0; i < at.Length; i++)
                        {
                            float k = t - at[i];
                            if (k < 0f || k > 0.09f) continue;
                            float f = (i % 2 == 0 ? 3200f : 2700f) + 1400f * Mathf.Sin(k / 0.09f * Mathf.PI);
                            v += Mathf.Sin(2f * Mathf.PI * f * k) * Mathf.Sin(k / 0.09f * Mathf.PI) * 0.35f;
                        }
                        return v;
                    });
                    break;
                case "ticktock": // tic... tac
                    c = Make(kind, 0.5f, t => Mathf.Sin(t * 2f * Mathf.PI * (t < 0.25f ? 1900f : 1300f)) * 0.4f * Mathf.Exp(-(t % 0.25f) * 70f));
                    break;
                default:
                    return null;
            }
            cache[kind] = c;
            return c;
        }

        static float Saw(float t, float f) => 2f * (t * f % 1f) - 1f;

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
