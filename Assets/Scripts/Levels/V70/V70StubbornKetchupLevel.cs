using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels.V64;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 268 "The Stubborn Ketchup" — "Put ketchup on the fries." Una tavola calda: patatine, un flacone di ketchup
    /// nuovo (adesivo "NEW!", una linguetta argentata che spunta da sotto il tappo e ogni tanto luccica), un coltello e
    /// un tovagliolo. False piste (nessuna azzera i progressi, niente sconfitta): portare il flacone sopra le patatine (si
    /// capovolge: non esce niente), scuotere il telefono (sbatacchia: "Not a drop."), strizzarlo (pizzico a due dita sul
    /// flacone o un tocco: "pfff", si gonfia), le pacche sul fondo (tonfi), il coltello nel collo (esce pulito).
    /// Soluzione: svitare il tappo (girare il dito attorno al tappo, oppure due tocchi), strappare il sigillo tirando la
    /// linguetta ("IT WAS NEVER OPENED."), poi capovolgere/strizzare il flacone sopra le patatine: uno smile di ketchup,
    /// "IT WAS STILL SEALED.", vittoria. Il tappo si può rimettere (trascinandolo sul flacone) e ritogliere quando si vuole.
    /// Niente microfono. La logica è in Gotcha.Levels.V64.StubbornKetchupLogic (senza Unity): qui solo input e presentazione.
    /// Tasti di prova nell'Editor: K soluzione completa, U mezzo giro di tappo, C rimette il tappo, P strappa il sigillo,
    /// 1 capovolge/raddrizza, 2 strizza, 3 pacca sul fondo, 4 coltello (S = scossone, da Inputs.Shake; Shift+clic =
    /// secondo dito fermo per provare il pizzico).
    /// </summary>
    public class V70StubbornKetchupLevel : V62Level
    {
        [Header("Tavolo")]
        [SerializeField] Transform fries;
        [SerializeField] Collider2D friesArea;
        [SerializeField] Collider2D pourZone;           // il flacone lasciato qui si capovolge sopra le patatine
        [SerializeField] Transform ketchup;             // contenitore delle gocce (centro = centro delle patatine)
        [SerializeField] Transform blobTemplate;
        [SerializeField] Transform knife;
        [SerializeField] Collider2D knifeArea;
        [SerializeField] Transform knifeTip;
        [SerializeField] Transform napkin;
        [SerializeField] Collider2D napkinArea;
        [Header("Flacone")]
        [SerializeField] Transform bottle;
        [SerializeField] Transform bottleBody;          // si gonfia strizzandolo
        [SerializeField] Collider2D bottleArea;
        [SerializeField] Collider2D bottomArea;         // le pacche sul fondo
        [SerializeField] Transform sticker;
        [SerializeField] Transform foil;
        [SerializeField] Collider2D tabArea;            // la punta della linguetta che spunta (tappo avvitato)
        [SerializeField] Collider2D sealArea;           // tutto il sigillo (tappo tolto)
        [SerializeField] SpriteRenderer glint;
        [SerializeField] Transform cap;
        [SerializeField] Collider2D capArea;
        [SerializeField] Transform capSocket;
        [SerializeField] Transform capRest;
        [SerializeField] Transform nozzle;
        [SerializeField] SmokeEmitter pfff;
        [SerializeField] Transform scrap;
        [SerializeField] Transform scrapRest;
        [SerializeField] Transform stream;
        [SerializeField] TextMesh infoText;
        [SerializeField] Transform infoHolder;

        const float TapTime = 0.45f, TapSlop = 0.22f;
        const float ShakeLevel = 12f, ShakeCooldown = 1.4f;
        const float CapSnap = 0.9f, KnifeSnap = 0.95f;
        const float PinchNear = 1.5f, PinchRatio = 0.75f, PinchMin = 0.18f;
        const float TwistMinRadius = 0.12f;
        const float GlintEvery = 6.5f;
        static readonly Vector3 TipOffset = new Vector3(0f, 1.8f, 0f);   // flacone capovolto: sopra le patatine

        static readonly string[] TipLines = { "Nothing comes out.", "Still nothing. Gravity gave up.", "It's upside down. It doesn't care." };
        static readonly string[] RattleLines = { "Rattle rattle. Not a drop.", "Not a drop.", "The ketchup is holding on for dear life." };
        static readonly string[] ShrugLines = { "Shake shake. Nothing changes.", "It's ketchup, not a snow globe.", "It sloshes. Smugly." };
        static readonly string[] SqueezeLines = { "Pfff.", "A sad little wheeze.", "It's full. It just won't share." };
        static readonly string[] SlapLines = { "Thud.", "Thud thud.", "THUD. Nothing.", "The bottom is not the problem." };
        static readonly string[] KnifeLines = { "The knife comes out clean.", "Clean. Suspiciously clean.", "Not a speck of red." };
        static readonly string[] TugLines = { "Something shiny under the cap.", "It's stuck under the cap.", "The cap is sitting on it." };
        static readonly string[] CapBackLines = { "Screwed back on. Still sealed underneath.", "Back on. Didn't help.", "Cap on. Same problem." };

        enum Kind { None, Bottle, Twist, Tab, Knife, LooseCap, Pinch, Other }

        sealed class Grip
        {
            public Kind kind;
            public Vector2 start, last;
            public float t0, lastAngle;
            public bool moved, hasAngle, onBottom;
        }

        readonly StubbornKetchupLogic logic = new StubbornKetchupLogic();
        readonly Dictionary<int, Grip> grips = new Dictionary<int, Grip>();
        readonly List<int> gone = new List<int>(4);
        readonly List<GameObject> blobs = new List<GameObject>();
        Camera cam;
        HwToast toast;
        HwDragger dragger;
        HwDragger.Item bottleItem, knifeItem, capItem;
        Vector3 homePos, bottleScale, bodyPos, bodyScale, capLocalPos, capLocalScale, foilLocalPos, knifeHome, scrapScale, capLoose;
        Quaternion knifeRot, capLocalRot;
        Coroutine poseCo, bodyCo;
        int tipLines, rattleLines, shrugLines, squeezeLines, slapLines, knifeLines, tugLines, capBackLines;
        float shakeReadyAt, glintAt, pinchD0;
        bool busy, finishing, pinchDone, captured;

        Vector3 TipPos => fries.position + TipOffset;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            StopAllCoroutines();
            poseCo = bodyCo = null;
            logic.Reset();
            ResetV62();
            V64AFingers.Reset();
            grips.Clear();
            busy = finishing = pinchDone = false;
            tipLines = rattleLines = shrugLines = squeezeLines = slapLines = knifeLines = tugLines = capBackLines = 0;
            toast = new HwToast(infoText, infoHolder);
            if (!captured)
            {
                captured = true;
                homePos = bottle.position;
                bottleScale = bottle.localScale;
                bodyPos = bottleBody.localPosition;
                bodyScale = bottleBody.localScale;
                capLocalPos = cap.localPosition;
                capLocalScale = cap.localScale;
                capLocalRot = cap.localRotation;
                foilLocalPos = foil.localPosition;
                knifeHome = knife.position;
                knifeRot = knife.localRotation;
                scrapScale = scrap.localScale;
            }
            capLoose = capRest.position;
            ClearBlobs();
            AttachCap();
            dragger = new HwDragger(this);
            bottleItem = dragger.Add(bottle, bottleArea, 1);
            knifeItem = dragger.Add(knife, knifeArea, 2);
            capItem = dragger.Add(cap, capArea, 3);
            shakeReadyAt = Time.time + 0.5f;
            glintAt = Time.time + 3f;
            RestPose();
        }

        // ---------- Pose (sempre ricavate dalla logica: uno stato valido dopo pausa/spegnimento) ----------

        void RestPose()
        {
            StopPose();
            StopBody();
            bottle.position = logic.Tipped ? TipPos : homePos;
            bottle.localRotation = Quaternion.Euler(0f, 0f, logic.Tipped ? 180f : 0f);
            bottle.localScale = bottleScale;
            bottleItem.Home = bottle.position;
            bottleItem.Enabled = true;
            if (logic.CapOn) { AttachCap(); CapPose(logic.CapProgress, 0f); }
            else DetachCap(capLoose);
            capItem.Enabled = !logic.CapOn;
            foil.localPosition = foilLocalPos;
            foil.localRotation = Quaternion.identity;
            HalloweenKit.Show(foil, logic.Sealed);
            HalloweenKit.Show(scrap, !logic.Sealed);
            scrap.position = scrapRest.position;
            scrap.localRotation = Quaternion.Euler(0f, 0f, 15f);
            scrap.localScale = scrapScale;
            knife.position = knifeHome;
            knife.localRotation = knifeRot;
            knifeItem.Home = knifeHome;
            knifeItem.Enabled = true;
            HalloweenKit.Show(stream, false);
            HalloweenKit.SetAlpha(glint, 0f);
            if (pfff != null) pfff.Stop();
        }

        /// <summary>Il tappo avvitato: figlio del flacone, alzato un po' secondo lo svitamento.</summary>
        void AttachCap()
        {
            if (cap.parent != bottle) cap.SetParent(bottle, false);
            cap.localPosition = capLocalPos;
            cap.localRotation = capLocalRot;
            cap.localScale = capLocalScale;
        }

        /// <summary>Il tappo sul tavolo: figlio del livello, oggetto trascinabile.</summary>
        void DetachCap(Vector3 at)
        {
            if (cap.parent != transform) cap.SetParent(transform, true);
            cap.position = new Vector3(at.x, at.y, cap.position.z);
            cap.localRotation = Quaternion.Euler(0f, 0f, 70f);
            cap.localScale = capLocalScale * bottleScale.x;
            if (dragger != null) { dragger.SetBaseScale(capItem, cap.localScale); capItem.Home = cap.position; }
        }

        /// <summary>k = svitamento 0..1; spin = angolo di "giro" (le righe del tappo scorrono).</summary>
        void CapPose(float k, float spin)
        {
            cap.localPosition = capLocalPos + new Vector3(0f, 0.09f * k, 0f);
            cap.localRotation = capLocalRot * Quaternion.Euler(0f, 0f, 4f * Mathf.Sin(spin * Mathf.Deg2Rad * 2f) * Mathf.Min(1f, k * 3f + 0.3f));
            cap.localScale = new Vector3(capLocalScale.x * (1f - 0.05f * Mathf.Abs(Mathf.Sin(spin * Mathf.Deg2Rad * 3f))), capLocalScale.y, capLocalScale.z);
        }

        void StopPose() { if (poseCo != null) { StopCoroutine(poseCo); poseCo = null; } }

        void StopBody()
        {
            if (bodyCo != null) { StopCoroutine(bodyCo); bodyCo = null; }
            if (bottleBody != null) { bottleBody.localPosition = bodyPos; bottleBody.localScale = bodyScale; }
        }

        void BodyFx(IEnumerator fx)
        {
            StopBody();
            if (isActiveAndEnabled) bodyCo = StartCoroutine(fx);
        }

        /// <summary>Porta il flacone in posa (dritto a casa o capovolto sopra le patatine), posizione e rotazione insieme.</summary>
        void PoseTo(bool tipped, float seconds)
        {
            StopPose();
            dragger.Stop(bottleItem);
            bottleItem.Home = tipped ? TipPos : homePos;
            if (!isActiveAndEnabled) { RestPose(); return; }
            poseCo = StartCoroutine(PoseRun(tipped, seconds));
        }

        IEnumerator PoseRun(bool tipped, float seconds)
        {
            yield return PoseMove(tipped ? TipPos : homePos, tipped ? 180f : 0f, seconds);
            poseCo = null;
        }

        IEnumerator PoseMove(Vector3 to, float angle, float seconds)
        {
            Vector3 from = bottle.position;
            float a0 = bottle.localEulerAngles.z;
            for (float k = 0f; k < 1f; k += Time.deltaTime / Mathf.Max(0.01f, seconds))
            {
                float e = 1f - (1f - k) * (1f - k);
                bottle.position = Vector3.Lerp(from, to, e) + Vector3.up * (0.25f * Mathf.Sin(k * Mathf.PI));
                bottle.localRotation = Quaternion.Euler(0f, 0f, Mathf.LerpAngle(a0, angle, e));
                yield return null;
            }
            bottle.position = to;
            bottle.localRotation = Quaternion.Euler(0f, 0f, angle);
        }

        // ---------- Ciclo di vita ----------

        void OnDisable()
        {
            // pausa/pubblicità/livello spento: le animazioni si fermano con l'oggetto, si torna a uno stato valido
            CancelGrips();
            busy = false;
            poseCo = bodyCo = null;
            if (captured && !finishing && dragger != null) RestPose();
        }

        void OnEnable()
        {
            if (!captured || IsFinished || dragger == null) return;
            // spento a metà del ketchup: si ricomincia lo smile da capo
            if (logic.Stage == KetchupStage.Pouring) { finishing = false; StartCoroutine(Finale()); }
        }

        void OnApplicationPause(bool paused) { if (paused) CancelGrips(); }
        void OnApplicationFocus(bool focus) { if (!focus) CancelGrips(); }

        void CancelGrips()
        {
            grips.Clear();
            pinchDone = false;
            if (dragger != null && dragger.Held != null)
            {
                HwDragger.Item it = dragger.Release();
                if (it == bottleItem) { if (isActiveAndEnabled) PoseTo(logic.Tipped, 0.25f); }
                else if (it != null) dragger.SendHome(it, 0.25f);
            }
            if (logic.Sealed && foil != null) { foil.localPosition = foilLocalPos; foil.localRotation = Quaternion.identity; }
        }

        void Update()
        {
            toast?.Tick();
            if (IsFinished || finishing) return;
            if (Time.timeScale <= 0f) { CancelGrips(); return; }   // pausa o pubblicità: niente input
            Glint();
            if (busy) { CancelGrips(); return; }
#if UNITY_EDITOR
            EditorKeys();
            if (busy || finishing) return;
#endif
            if (Inputs.Shake() > ShakeLevel && Time.time >= shakeReadyAt) { shakeReadyAt = Time.time + ShakeCooldown; Shake(); return; }
            ReadFingers();
        }

        /// <summary>L'indizio: ogni tanto la linguetta argentata luccica (finché c'è il sigillo).</summary>
        void Glint()
        {
            if (glint == null) return;
            if (!logic.Sealed) { HalloweenKit.SetAlpha(glint, 0f); return; }
            float k = Time.time - glintAt;
            if (k < 0f) { HalloweenKit.SetAlpha(glint, 0f); return; }
            if (k > 0.6f) { glintAt = Time.time + GlintEvery; HalloweenKit.SetAlpha(glint, 0f); return; }
            HalloweenKit.SetAlpha(glint, 0.9f * Mathf.Sin(k / 0.6f * Mathf.PI));
        }

        // ---------- Dita ----------

        void ReadFingers()
        {
            List<V64AFinger> fingers = V64AFingers.Read(cam);
            foreach (V64AFinger f in fingers)
            {
                if (f.began) grips[f.id] = Begin(f.world);
                if (!grips.TryGetValue(f.id, out Grip g)) continue;
                g.last = f.world;
                if (Vector2.Distance(f.world, g.start) > TapSlop) g.moved = true;
                Drag(g);
                if (f.ended) End(f.id, g);
                if (busy || finishing) { CancelGrips(); return; }
            }
            CheckPinch();
            if (fingers.Count == 0 && grips.Count > 0)
            {
                gone.Clear();
                foreach (int id in grips.Keys) gone.Add(id);
                foreach (int id in gone) if (grips.TryGetValue(id, out Grip g)) End(id, g);
            }
        }

        Grip Begin(Vector2 w)
        {
            var g = new Grip { start = w, last = w, t0 = Time.time, kind = Kind.Other };
            if (busy || finishing) { g.kind = Kind.None; return g; }
            // un secondo dito sul flacone mentre il primo lo tiene: è un pizzico (strizzare)
            if (grips.Count == 1 && Vector2.Distance(w, bottle.position) < PinchNear)
            {
                foreach (Grip other in grips.Values)
                    if (other.kind == Kind.Bottle || other.kind == Kind.Twist || other.kind == Kind.Other || other.kind == Kind.Tab)
                    {
                        if (other.kind == Kind.Bottle && dragger.Held == bottleItem) { dragger.Release(); PoseTo(logic.Tipped, 0.15f); }
                        if (other.kind == Kind.Tab) RestFoil();
                        other.kind = Kind.Pinch;
                        g.kind = Kind.Pinch;
                        pinchD0 = Vector2.Distance(w, other.last);
                        pinchDone = false;
                        return g;
                    }
            }
            if (grips.Count >= 1) { g.kind = Kind.None; return g; }   // un dito alla volta per il resto
            if (!logic.Tipped)
            {
                if (logic.CapOn)
                {
                    if (tabArea.OverlapPoint(w)) { g.kind = Kind.Tab; return g; }
                    if (capArea.OverlapPoint(w)) { g.kind = Kind.Twist; return g; }
                }
                else if (logic.Sealed && sealArea.OverlapPoint(w)) { g.kind = Kind.Tab; return g; }
            }
            else if (Vector2.Distance(w, capSocket.position) < 0.45f)
            {
                // il tappo/la bocca puntati sulle patatine: il flacone torna dritto per poterci lavorare
                g.kind = Kind.None;
                logic.Upright();
                PoseTo(false, 0.35f);
                Sfx.Play(this, "whoosh", 0.3f);
                return g;
            }
            HwDragger.Item it = dragger.TryPick(w);
            if (it == bottleItem) { StopPose(); g.kind = Kind.Bottle; g.onBottom = bottomArea.OverlapPoint(w); }
            else if (it == knifeItem) g.kind = Kind.Knife;
            else if (it == capItem) g.kind = Kind.LooseCap;
            return g;
        }

        void Drag(Grip g)
        {
            switch (g.kind)
            {
                case Kind.Bottle:
                    if (dragger.Held != bottleItem) return;
                    if (g.moved) dragger.MoveHeld(g.last);
                    else return;
                    // sopra le patatine si capovolge, altrove torna dritto
                    float target = pourZone.OverlapPoint(bottle.position) ? 180f : 0f;
                    float z = Mathf.MoveTowardsAngle(bottle.localEulerAngles.z, target, 540f * Time.deltaTime);
                    bottle.localRotation = Quaternion.Euler(0f, 0f, z);
                    break;
                case Kind.Knife:
                case Kind.LooseCap:
                    dragger.MoveHeld(g.last);
                    break;
                case Kind.Twist:
                    Twist(g);
                    break;
                case Kind.Tab:
                    if (!logic.CapOn && logic.Sealed)
                    {
                        // il sigillo segue un po' il dito (si stacca davvero solo tirando abbastanza)
                        Vector2 d = g.last - g.start;
                        float k = Mathf.Clamp01(d.magnitude / StubbornKetchupLogic.PeelDistance);
                        Vector3 local = bottle.InverseTransformVector(d.normalized * 0.22f * k);
                        foil.localPosition = foilLocalPos + local;
                        foil.localRotation = Quaternion.Euler(0f, 0f, -25f * k);
                    }
                    break;
            }
        }

        void Twist(Grip g)
        {
            if (!logic.CapOn || logic.Tipped) return;
            Vector2 c = cap.position;
            Vector2 v = g.last - c;
            if (v.magnitude < TwistMinRadius) { g.hasAngle = false; return; }
            float a = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
            if (!g.hasAngle) { g.hasAngle = true; g.lastAngle = a; return; }
            float delta = Mathf.DeltaAngle(g.lastAngle, a);
            g.lastAngle = a;
            if (Mathf.Abs(delta) < 0.5f) return;
            float before = logic.CapTurn;
            bool off = logic.TurnCap(delta);
            if (Mathf.FloorToInt(before / 60f) != Mathf.FloorToInt(logic.CapTurn / 60f)) { V70KetchupSound.Play(this, "creak", 0.45f); Haptics.Light(); }
            if (off) { CapOff(); return; }
            CapPose(logic.CapProgress, logic.CapTurn);
        }

        void End(int id, Grip g)
        {
            grips.Remove(id);
            bool tap = !g.moved && Time.time - g.t0 <= TapTime;
            switch (g.kind)
            {
                case Kind.Bottle: DropBottle(g, tap); break;
                case Kind.Knife: DropKnife(tap); break;
                case Kind.LooseCap: DropCap(tap); break;
                case Kind.Twist:
                    if (tap) TapCap();
                    else if (logic.CapOn) CapPose(logic.CapProgress, 0f);
                    break;
                case Kind.Tab: ReleaseTab(g, tap); break;
                case Kind.Pinch:
                    if (grips.Count == 0) pinchDone = false;
                    break;
                case Kind.Other:
                    if (tap) TapScene(g.start);
                    break;
            }
        }

        void CheckPinch()
        {
            if (pinchDone || grips.Count != 2) return;
            Grip a = null, b = null;
            foreach (Grip g in grips.Values) { if (g.kind != Kind.Pinch) return; if (a == null) a = g; else b = g; }
            if (a == null || b == null) return;
            float d = Vector2.Distance(a.last, b.last);
            if (d < pinchD0 * PinchRatio && pinchD0 - d > PinchMin)
            {
                pinchDone = true;
                Squeeze();
            }
        }

        void TapScene(Vector2 w)
        {
            if (friesArea.OverlapPoint(w))
            {
                StartCoroutine(HalloweenKit.Pop(fries, 1.04f, 0.2f));
                toast.Show("They're getting cold.", 1.4f);
                return;
            }
            if (napkinArea.OverlapPoint(w))
            {
                StartCoroutine(HalloweenKit.Wiggle(napkin, 4f, 0.3f, 30f));
                toast.Show("Very absorbent. Not helpful.", 1.5f);
            }
        }

        // ---------- Il flacone ----------

        void DropBottle(Grip g, bool tap)
        {
            if (dragger.Held == bottleItem) dragger.Release();
            if (tap)
            {
                PoseTo(logic.Tipped, 0.1f);
                if (g.onBottom) Slap();
                else Squeeze();
                return;
            }
            bool over = pourZone.OverlapPoint(bottle.position);
            if (!over)
            {
                logic.Upright();
                PoseTo(false, 0.3f);
                return;
            }
            bool was = logic.Tipped;
            PoseTo(true, 0.25f);
            if (was) return;
            KetchupResult r = logic.TipOver();
            if (r == KetchupResult.Pour) { StartCoroutine(Finale()); return; }
            if (r == KetchupResult.Nothing)
            {
                V70KetchupSound.Play(this, "slosh", 0.5f);
                toast.Show(TipLines[tipLines++ % TipLines.Length], 1.6f);
            }
        }

        void Shake()
        {
            bool tipped = logic.Tipped;
            KetchupResult r = logic.Shake();
            if (r == KetchupResult.None) return;
            if (r == KetchupResult.Pour) { StartCoroutine(Finale()); return; }
            Haptics.Medium();
            V70KetchupSound.Play(this, "rattle", 0.6f);
            BodyFx(Rattle(0.8f));
            StartCoroutine(HalloweenKit.Wiggle(sticker, 10f, 0.6f, 35f));
            if (tipped) toast.Show(RattleLines[rattleLines++ % RattleLines.Length], 1.6f);
            else toast.Show(ShrugLines[shrugLines++ % ShrugLines.Length], 1.6f);
        }

        IEnumerator Rattle(float seconds)
        {
            for (float k = 0f; k < seconds; k += Time.deltaTime)
            {
                float a = 1f - k / seconds;
                bottleBody.localPosition = bodyPos + new Vector3(Mathf.Sin(k * 55f) * 0.05f * a, Mathf.Sin(k * 31f) * 0.03f * a, 0f);
                bottleBody.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(k * 40f) * 4f * a);
                yield return null;
            }
            bottleBody.localPosition = bodyPos;
            bottleBody.localRotation = Quaternion.identity;
            bodyCo = null;
        }

        void Squeeze()
        {
            KetchupResult r = logic.Squeeze();
            if (r == KetchupResult.None) return;
            if (r == KetchupResult.Pour) { StartCoroutine(Finale()); return; }
            Haptics.Light();
            V70KetchupSound.Play(this, "wheeze", 0.6f);
            if (pfff != null) StartCoroutine(HalloweenKit.Burst(pfff, 0.25f));
            BodyFx(Bulge());
            toast.Show(SqueezeLines[squeezeLines++ % SqueezeLines.Length], 1.4f);
        }

        IEnumerator Bulge()
        {
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.55f)
            {
                float b = Mathf.Sin(k * Mathf.PI);
                bottleBody.localScale = new Vector3(bodyScale.x * (1f + 0.16f * b), bodyScale.y * (1f - 0.05f * b), bodyScale.z);
                yield return null;
            }
            bottleBody.localScale = bodyScale;
            bodyCo = null;
        }

        void Slap()
        {
            KetchupResult r = logic.Slap();
            if (r == KetchupResult.None) return;
            if (r == KetchupResult.Pour) { StartCoroutine(Finale()); return; }
            Haptics.Light();
            V70KetchupSound.Play(this, "thud", 0.75f);
            BodyFx(Thump());
            toast.Show(SlapLines[slapLines++ % SlapLines.Length], 1.2f);
        }

        IEnumerator Thump()
        {
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.22f)
            {
                float b = Mathf.Sin(k * Mathf.PI) * (1f - k);
                bottleBody.localPosition = bodyPos + new Vector3(0f, 0.07f * b, 0f);
                bottleBody.localScale = new Vector3(bodyScale.x * (1f + 0.05f * b), bodyScale.y * (1f - 0.06f * b), bodyScale.z);
                yield return null;
            }
            bottleBody.localPosition = bodyPos;
            bottleBody.localScale = bodyScale;
            bodyCo = null;
        }

        // ---------- Il coltello ----------

        void DropKnife(bool tap)
        {
            dragger.Release();
            if (tap)
            {
                dragger.SendHome(knifeItem, 0.2f);
                toast.Show("A knife. For fries? Bold.", 1.4f);
                return;
            }
            Vector3 mouth = capSocket.position;
            bool near = Vector2.Distance(knifeTip.position, mouth) < KnifeSnap || Vector2.Distance(knife.position, mouth) < KnifeSnap
                || bottleArea.OverlapPoint(knifeTip.position);
            if (!near) { dragger.SendHome(knifeItem, 0.3f); return; }
            KnifeResult r = logic.Knife();
            if (r == KnifeResult.None) { dragger.SendHome(knifeItem, 0.3f); return; }
            StartCoroutine(KnifePoke(r));
        }

        IEnumerator KnifePoke(KnifeResult r)
        {
            busy = true;
            CancelGrips();
            knifeItem.Enabled = false;
            // la lama punta dentro il collo (giù se il flacone è dritto, su se è capovolto)
            Vector3 mouth = capSocket.position;
            Vector3 axis = (mouth - bottle.position).normalized;       // dal centro verso la bocca
            float ang = Mathf.Atan2(-axis.y, -axis.x) * Mathf.Rad2Deg;   // la lama (asse +x del coltello) verso il centro
            Vector3 tipLocal = knife.InverseTransformPoint(knifeTip.position);
            Quaternion rot = Quaternion.Euler(0f, 0f, ang);
            Vector3 tipOffsetW = rot * Vector3.Scale(tipLocal, knife.lossyScale);
            Vector3 outside = mouth + axis * 0.35f - tipOffsetW;
            Vector3 inside = mouth - axis * 0.25f - tipOffsetW;
            Quaternion r0 = knife.localRotation;
            Vector3 p0 = knife.position;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.25f)
            {
                knife.position = Vector3.Lerp(p0, outside, k);
                knife.localRotation = Quaternion.Slerp(r0, rot, k);
                yield return null;
            }
            yield return HalloweenKit.Move(knife, inside, 0.15f);
            V70KetchupSound.Play(this, r == KnifeResult.Foil ? "tink" : "scrape", 0.6f);
            Haptics.Light();
            yield return HalloweenKit.Shake(knife, 0.03f, 0.3f);
            yield return HalloweenKit.Move(knife, outside, 0.2f);
            switch (r)
            {
                case KnifeResult.Clean: toast.Show(KnifeLines[knifeLines++ % KnifeLines.Length], 1.6f); break;
                case KnifeResult.Foil: toast.Show("Tink. Don't stab it. There's a tab.", 1.8f); break;
                default: toast.Show("No need. It's open.", 1.4f); break;
            }
            StartCoroutine(HalloweenKit.Pop(knife, 1.08f, 0.2f));
            yield return new WaitForSeconds(0.3f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.3f)
            {
                knife.localRotation = Quaternion.Slerp(rot, knifeRot, k);
                knife.position = Vector3.Lerp(outside, knifeHome, 1f - (1f - k) * (1f - k));
                yield return null;
            }
            knife.position = knifeHome;
            knife.localRotation = knifeRot;
            knifeItem.Enabled = true;
            busy = false;
        }

        // ---------- Tappo e sigillo ----------

        void TapCap()
        {
            if (!logic.CapOn || logic.Tipped) return;
            V70KetchupSound.Play(this, "creak", 0.55f);
            Haptics.Light();
            if (logic.TapCap()) { CapOff(); return; }
            StartCoroutine(TwistAnim(logic.CapProgress));
            toast.Show("Creak. It turns...", 1.2f);
        }

        IEnumerator TwistAnim(float k)
        {
            for (float t = 0f; t < 1f; t += Time.deltaTime / 0.3f)
            {
                if (!logic.CapOn || cap.parent != bottle) yield break;
                CapPose(k, t * 180f);
                yield return null;
            }
            if (logic.CapOn && cap.parent == bottle) CapPose(k, 0f);
        }

        void CapOff()
        {
            CancelGrips();
            StartCoroutine(CapPop());
        }

        IEnumerator CapPop()
        {
            busy = true;
            V70KetchupSound.Play(this, "pop", 0.7f);
            Haptics.Medium();
            Vector3 up = cap.position + bottle.up * 0.35f;
            DetachCap(cap.position);
            capItem.Enabled = false;
            cap.localRotation = Quaternion.identity;
            yield return HalloweenKit.Move(cap, up, 0.15f);
            capLoose = capRest.position;
            StartCoroutine(HalloweenKit.Wiggle(cap, 25f, 0.35f, 30f));
            yield return HalloweenKit.Move(cap, capLoose, 0.35f, 0.4f);
            cap.localRotation = Quaternion.Euler(0f, 0f, 70f);
            capItem.Home = capLoose;
            capItem.Enabled = true;
            V70KetchupSound.Play(this, "clack", 0.4f);
            toast.Show(logic.Sealed ? "Cap off. Wait... what's that?" : "Cap off.", 1.8f);
            if (logic.Sealed) StartCoroutine(HalloweenKit.Pop(foil, 1.2f, 0.3f));
            busy = false;
        }

        void DropCap(bool tap)
        {
            dragger.Release();
            if (tap)
            {
                StartCoroutine(HalloweenKit.Wiggle(cap, 10f, 0.3f, 30f));
                toast.Show("Just a cap.", 1.2f);
                return;
            }
            bool near = Vector2.Distance(cap.position, capSocket.position) < CapSnap || bottleArea.OverlapPoint(cap.position);
            if (near && logic.PutCapBack())
            {
                AttachCap();
                CapPose(0f, 0f);
                capItem.Enabled = false;
                V70KetchupSound.Play(this, "creak", 0.5f);
                StartCoroutine(TwistAnim(0f));
                toast.Show(logic.Sealed ? CapBackLines[capBackLines++ % CapBackLines.Length] : "Back on. The seal's still gone.", 1.6f);
                return;
            }
            if (near && logic.Tipped) toast.Show("Not while it's upside down.", 1.4f);
            // resta dov'è (dentro lo schermo, sopra il tavolo)
            Vector3 p = cap.position;
            p.y = Mathf.Min(p.y, 0.6f);
            capLoose = p;
            dragger.SendTo(capItem, p, 0.15f, 0f);
            capItem.Home = p;
        }

        void RestFoil()
        {
            foil.localPosition = foilLocalPos;
            foil.localRotation = Quaternion.identity;
        }

        void ReleaseTab(Grip g, bool tap)
        {
            float dist = Vector2.Distance(g.last, g.start);
            PeelResult r = logic.Peel(tap ? 0f : dist);
            switch (r)
            {
                case PeelResult.CapInTheWay:
                    V70KetchupSound.Play(this, "tink", 0.4f);
                    StartCoroutine(HalloweenKit.Wiggle(cap, 6f, 0.3f, 40f));
                    toast.Show(TugLines[tugLines++ % TugLines.Length], 1.6f);
                    break;
                case PeelResult.Partial:
                    RestFoil();
                    StartCoroutine(HalloweenKit.Pop(foil, 1.15f, 0.2f));
                    toast.Show(tap ? "A foil tab. Pull it." : "Almost. Pull it all the way.", 1.4f);
                    break;
                case PeelResult.Peeled:
                    StartCoroutine(PeelAway(g.last - g.start));
                    break;
            }
        }

        IEnumerator PeelAway(Vector2 dir)
        {
            busy = true;
            V70KetchupSound.Play(this, "rip", 0.7f);
            Haptics.Medium();
            HalloweenKit.Show(foil, false);
            RestFoil();
            HalloweenKit.Show(scrap, true);
            scrap.position = new Vector3(capSocket.position.x, capSocket.position.y, scrap.position.z);
            scrap.localScale = scrapScale;
            Vector3 fling = scrap.position + (Vector3)(dir.sqrMagnitude > 0.01f ? dir.normalized : Vector2.right) * 0.6f + Vector3.up * 0.2f;
            yield return HalloweenKit.Move(scrap, fling, 0.2f);
            StartCoroutine(HalloweenKit.Wiggle(scrap, 30f, 0.5f, 25f));
            yield return HalloweenKit.Move(scrap, scrapRest.position, 0.45f, 0.5f);
            scrap.localRotation = Quaternion.Euler(0f, 0f, 15f);
            toast.Show("IT WAS NEVER OPENED.", 2.4f);
            StartCoroutine(HalloweenKit.Pop(bottle, 1.05f, 0.25f));
            busy = false;
        }

        // ---------- La soluzione ----------

        IEnumerator Finale()
        {
            finishing = true;
            CancelGrips();
            StopBody();
            StopPose();
            ClearBlobs();
            HalloweenKit.SetAlpha(glint, 0f);
            // il flacone sopra le patatine, capovolto
            yield return PoseMove(TipPos, 180f, 0.35f);
            V70KetchupSound.Play(this, "squirt", 0.7f);
            Haptics.Medium();
            HalloweenKit.Show(stream, true);
            // lo smile: la bocca da sinistra a destra, poi i due occhi
            Vector3 c = fries.position + new Vector3(0f, 0.12f, 0f);
            Vector3 lift = bottle.position - nozzle.position;
            const int Mouth = 16;
            for (int i = 0; i < Mouth; i++)
            {
                float a = Mathf.Deg2Rad * (205f + i * 130f / (Mouth - 1));
                Vector3 p = c + new Vector3(Mathf.Cos(a) * 0.62f, 0.08f + Mathf.Sin(a) * 0.4f, 0f);
                yield return SquirtAt(p, lift, 0.06f, 1f);
            }
            foreach (float ex in new[] { -0.26f, 0.26f })
            {
                HalloweenKit.Show(stream, false);
                Vector3 p = c + new Vector3(ex, 0.3f, 0f);
                yield return PoseMove(p + lift, 180f, 0.12f);
                HalloweenKit.Show(stream, true);
                V70KetchupSound.Play(this, "squirt", 0.5f);
                yield return SquirtAt(p, lift, 0.1f, 1.7f);
            }
            HalloweenKit.Show(stream, false);
            V70KetchupSound.Play(this, "pop", 0.5f);
            yield return PoseMove(homePos, 0f, 0.35f);
            toast.Show("IT WAS STILL SEALED.", 2.6f);
            StartCoroutine(HalloweenKit.Pop(ketchup, 1.08f, 0.3f));
            yield return Celebrate(1.4f);
            if (logic.TryComplete()) Win();
        }

        /// <summary>Il beccuccio va sul punto e ci lascia una goccia.</summary>
        IEnumerator SquirtAt(Vector3 p, Vector3 lift, float seconds, float size)
        {
            Vector3 from = bottle.position, to = p + lift;
            for (float k = 0f; k < 1f; k += Time.deltaTime / Mathf.Max(0.01f, seconds))
            {
                bottle.position = Vector3.Lerp(from, to, k);
                PlaceStream();
                yield return null;
            }
            bottle.position = to;
            PlaceStream();
            AddBlob(p, size);
        }

        void PlaceStream()
        {
            if (stream == null) return;
            stream.position = new Vector3(nozzle.position.x, nozzle.position.y, stream.position.z);
        }

        void AddBlob(Vector3 p, float size)
        {
            if (blobTemplate == null) return;
            GameObject b = Instantiate(blobTemplate.gameObject, ketchup);
            b.SetActive(true);
            b.transform.position = new Vector3(p.x, p.y, blobTemplate.position.z);
            b.transform.localScale = blobTemplate.localScale * size * Random.Range(0.9f, 1.1f);
            blobs.Add(b);
            StartCoroutine(HalloweenKit.Pop(b.transform, 1.25f, 0.15f));
        }

        void ClearBlobs()
        {
            foreach (GameObject b in blobs) if (b != null) Destroy(b);
            blobs.Clear();
            if (ketchup != null) ketchup.localScale = Vector3.one;
        }

#if UNITY_EDITOR
        // ---------- Tasti di prova nell'Editor ----------

        void EditorKeys()
        {
            if (Input.GetKeyDown(KeyCode.K))
            {
                // soluzione completa: tappo via, sigillo via, ketchup
                if (logic.Tipped) { logic.Upright(); RestPose(); }
                for (int i = 0; i < 4 && logic.CapOn; i++) logic.TapCap();
                logic.Peel(StubbornKetchupLogic.PeelDistance);
                capLoose = capRest.position;
                RestPose();
                Squeeze();
                return;
            }
            if (Input.GetKeyDown(KeyCode.U)) { if (logic.Tipped) { logic.Upright(); PoseTo(false, 0.2f); } TapCap(); }
            if (Input.GetKeyDown(KeyCode.C) && !logic.CapOn && !logic.Tipped && logic.PutCapBack()) { AttachCap(); CapPose(0f, 0f); capItem.Enabled = false; }
            if (Input.GetKeyDown(KeyCode.P))
            {
                PeelResult r = logic.Peel(StubbornKetchupLogic.PeelDistance);
                if (r == PeelResult.Peeled) StartCoroutine(PeelAway(Vector2.right));
                else if (r == PeelResult.CapInTheWay) toast.Show(TugLines[tugLines++ % TugLines.Length], 1.6f);
            }
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                if (logic.Tipped) { logic.Upright(); PoseTo(false, 0.3f); }
                else
                {
                    bottle.position = TipPos;
                    var g = new Grip { kind = Kind.Bottle };
                    DropBottle(g, false);
                }
            }
            if (Input.GetKeyDown(KeyCode.Alpha2)) Squeeze();
            if (Input.GetKeyDown(KeyCode.Alpha3)) Slap();
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                KnifeResult r = logic.Knife();
                if (r != KnifeResult.None) StartCoroutine(KnifePoke(r));
            }
        }
#endif
    }

    /// <summary>Suoni sintetizzati del livello 268: sbuffo del flacone, tonfi, sbatacchio, tappo, alluminio, ketchup.</summary>
    public static class V70KetchupSound
    {
        const int Rate = 22050;
        static readonly Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>();
        static readonly System.Random rnd = new System.Random(268);

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
                case "wheeze":   // "pfff": aria che esce a fatica, scende di tono
                    {
                        float lp = 0f;
                        c = Make(kind, 0.5f, t => { lp += (N() - lp) * Mathf.Lerp(0.5f, 0.12f, t / 0.5f); return lp * 0.8f * Mathf.Min(1f, t * 30f) * Mathf.Exp(-t * 4f); });
                    }
                    break;
                case "thud":     // pacca sul fondo: tonfo sordo
                    c = Make(kind, 0.2f, t => (Mathf.Sin(t * 2f * Mathf.PI * Mathf.Lerp(140f, 60f, t / 0.2f)) * 0.9f + N() * 0.2f) * Mathf.Exp(-t * 22f));
                    break;
                case "rattle":   // flacone scosso: il ketchup che sbatte dentro
                    c = Make(kind, 0.8f, t => (N() * 0.25f + Mathf.Sin(t * 2f * Mathf.PI * 95f) * 0.25f) * (Mathf.Sin(t * 2f * Mathf.PI * 9f) > 0f ? 1f : 0.25f) * Mathf.Exp(-t * 2.2f));
                    break;
                case "slosh":    // capovolto: il ketchup scivola giù (e si ferma)
                    {
                        float lp = 0f;
                        c = Make(kind, 0.45f, t => { lp += (N() - lp) * 0.08f; return lp * 1.6f * Mathf.Sin(t / 0.45f * Mathf.PI); });
                    }
                    break;
                case "creak":    // tappo che gira: scricchiolio di plastica
                    c = Make(kind, 0.18f, t => N() * 0.4f * (Mathf.Repeat(t * 70f, 1f) < 0.25f ? 1f : 0.15f) * Mathf.Sin(t / 0.18f * Mathf.PI));
                    break;
                case "pop":      // tappo che salta
                    c = Make(kind, 0.12f, t => Mathf.Sin(t * 2f * Mathf.PI * Mathf.Lerp(700f, 250f, t / 0.12f)) * 0.7f * Mathf.Exp(-t * 30f));
                    break;
                case "clack":    // tappo che cade sul tavolo
                    c = Make(kind, 0.1f, t => (N() * 0.5f + Mathf.Sin(t * 2f * Mathf.PI * 800f) * 0.3f) * Mathf.Exp(-t * 60f));
                    break;
                case "tink":     // coltello/dito sull'alluminio
                    c = Make(kind, 0.25f, t => (Mathf.Sin(t * 2f * Mathf.PI * 2900f) + Mathf.Sin(t * 2f * Mathf.PI * 4100f) * 0.4f) * 0.3f * Mathf.Exp(-t * 25f));
                    break;
                case "scrape":   // coltello nel collo vuoto
                    c = Make(kind, 0.3f, t => N() * 0.3f * Mathf.Abs(Mathf.Sin(t * 2f * Mathf.PI * 30f)) * Mathf.Sin(t / 0.3f * Mathf.PI));
                    break;
                case "rip":      // sigillo strappato
                    c = Make(kind, 0.35f, t => N() * 0.55f * (0.6f + 0.4f * Mathf.Sin(t * 2f * Mathf.PI * 60f)) * Mathf.Min(1f, t * 40f) * Mathf.Exp(-t * 7f));
                    break;
                case "squirt":   // ketchup che esce: pernacchietta umida
                    {
                        float lp = 0f;
                        c = Make(kind, 0.55f, t => { lp += (N() - lp) * 0.25f; return (lp * 0.6f + Mathf.Sign(Mathf.Sin(t * 2f * Mathf.PI * 75f)) * 0.12f) * Mathf.Sin(t / 0.55f * Mathf.PI); });
                    }
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
            AudioClip clip = AudioClip.Create("v70_" + name, n, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
