using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels.V64;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 270 "The Lost Remote" — "Change the channel!" Un soggiorno: il televisore a tubo manda "PAINT DRYING LIVE
    /// — HOUR 37" (un tizio che guarda la vernice asciugare), il divano con tre cuscini, il tavolino col libretto delle
    /// istruzioni, l'orologio a muro (sbagliato di tre ore), il gatto che dorme sul suo cuscino, una pianta, la spina del
    /// televisore nella presa. False piste (nessuna azzera i progressi, niente sconfitta): alzare i cuscini (monetine,
    /// un calzino, briciole e un topolino che dorme: "Not a remote."), toccare il gatto (si gira e scopre il telecomando:
    /// i tasti non fanno niente, "Battery compartment: empty."; le pile dell'orologio, prese toccando l'orologio o
    /// portandoci sopra il telecomando, fermano l'orologio e sono scariche anche loro), urlare al televisore (microfono)
    /// o toccare lo schermo ("Hour 38."), scuotere il telefono (una botta al televisore), staccare la spina (schermo nero:
    /// "That's not changing the channel. That's giving up."; si riattacca trascinandola alla presa o toccandola), il
    /// mobiletto, la pianta. "CH -" porta a repliche ancora peggiori ("HOUR 36 (RERUN)"), "CH +" risale.
    /// Indizio: il libretto (un tocco lo apre) ha una pagina col televisorino e una freccia sul fianco, "CH +/-"; i
    /// tastini veri sono sul fianco destro del televisore, piccoli ma visibili (dopo aver letto il libretto luccicano ogni
    /// tanto). Soluzione: "CH +" sul fianco (area di tocco generosa: la metà alta del fianco) -> neve, la disco dei gatti,
    /// il gatto si sveglia e guarda contento, "TVs HAD BUTTONS ALL ALONG.", vittoria.
    /// Microfono solo mentre si gioca (si spegne in pausa, nel finale e all'uscita); senza microfono l'urlo si fa
    /// toccando lo schermo (stessa reazione). La logica è in Gotcha.Levels.V64.LostRemoteLogic (senza Unity).
    /// Tasti di prova nell'Editor: C = CH +, V = CH -, M libretto, T gatto, R telecomando, K orologio, 1/2/3 cuscini,
    /// U stacca/riattacca la spina, Y urlo (B/N = voce dal microfono finto, S = scossone, da Inputs).
    /// </summary>
    public class V72LostRemoteLevel : V62Level
    {
        [Header("Televisore")]
        [SerializeField] Transform tv;
        [SerializeField] Collider2D screenArea;         // tutta la cassa davanti: toccarla = urlargli contro
        [SerializeField] Collider2D chUpArea;           // metà alta del fianco destro (generosa)
        [SerializeField] Collider2D chDownArea;         // metà bassa del fianco destro
        [SerializeField] Transform chUpButton;
        [SerializeField] Transform chDownButton;
        [SerializeField] SpriteRenderer buttonGlint;    // l'indizio: i tastini luccicano (dopo aver letto il libretto)
        [SerializeField] SpriteRenderer showPaint;
        [SerializeField] Transform showDisco;
        [SerializeField] Transform dancer;
        [SerializeField] SpriteRenderer staticFx;
        [SerializeField] TextMesh showTitle;
        [SerializeField] SpriteRenderer powerLed;
        [SerializeField] Collider2D cabinetArea;
        [Header("Spina")]
        [SerializeField] Transform plug;
        [SerializeField] Collider2D plugArea;
        [SerializeField] Transform cordAnchor;          // dove il filo esce dal televisore
        [SerializeField] Transform cordEnd;             // dove il filo entra nella spina (figlio della spina)
        [SerializeField] LineRenderer cord;
        [Header("Orologio")]
        [SerializeField] Transform clock;
        [SerializeField] Collider2D clockArea;
        [SerializeField] Transform secondHand;
        [SerializeField] Transform clockBatteries;      // le due pile (nascoste: si vedono solo mentre passano)
        [Header("Tavolino e pianta")]
        [SerializeField] Transform manual;
        [SerializeField] Collider2D manualArea;
        [SerializeField] Transform manualPopup;         // la pagina aperta (si chiude con un tocco qualsiasi)
        [SerializeField] Transform plant;
        [SerializeField] Collider2D plantArea;
        [Header("Gatto e telecomando")]
        [SerializeField] Transform cat;
        [SerializeField] Collider2D catArea;
        [SerializeField] Transform catEyes;
        [SerializeField] Transform zzz;
        [SerializeField] Transform remote;
        [SerializeField] Collider2D remoteArea;
        [SerializeField] Transform remoteHidden;
        [SerializeField] Transform remoteRest;
        [Header("Divano")]
        [SerializeField] Transform[] cushions;
        [SerializeField] Collider2D[] cushionAreas;
        [SerializeField] Transform[] finds;             // cosa c'è sotto ogni cuscino (si vede solo a cuscino alzato)
        [SerializeField] TextMesh infoText;
        [SerializeField] Transform infoHolder;

        const float TapSlop = 0.6f, TapTime = 0.6f;
        const float PlugSnap = 0.7f, PlugPull = 0.45f, CordReach = 1.9f;
        const float ShakeLevel = 12f, ShakeCooldown = 1.4f, ShoutCooldown = 1.2f;
        const float GlintEvery = 7f, CushionLift = 0.42f;
        static readonly Color RerunTint = new Color(0.86f, 0.78f, 0.62f), TitleLive = Color.white, TitleGood = new Color(1f, 0.9f, 0.37f);
        static readonly Vector3 ZzzOffset = new Vector3(-0.95f, 1.05f, 0f);   // sopra la testa del gatto (coordinate del gatto)

        static readonly string[] CoinLines = { "Two coins. Not a remote.", "Still just coins. You're 20 cents richer.", "Coins. Again." };
        static readonly string[] SockLines = { "A sock. Not a remote.", "The sock is still there. Nobody wants it.", "Sock. Singular. The other one is gone too." };
        static readonly string[] MouseLines = { "Crumbs. And a sleeping mouse. Not a remote.", "Shh. The mouse is sleeping.", "The mouse does not have the remote." };
        static readonly string[] CatLines = { "Purr. Zzz.", "Do not disturb.", "The cat has done enough." };
        static readonly string[] EmptyLines = { "Battery compartment: empty.", "Click. Click. Still empty.", "Pressing harder won't make batteries." };
        static readonly string[] DeadLines = { "These are dead too.", "Dead. Like the clock.", "Click. Nothing. Still dead." };
        static readonly string[] WrongLines = { "It's three hours wrong. Nobody ever fixes it.", "Tick tock. Wrong o'clock.", "Still three hours wrong." };
        static readonly string[] StoppedLines = { "Stopped. Now it's right twice a day.", "The clock is on a break.", "No batteries. No time." };
        static readonly string[] ShoutLines = { "Hour {0}.", "It can't hear you. Hour {0}.", "Yelling won't help. Hour {0}." };
        static readonly string[] TapLines = { "Hour {0}.", "Tap tap. Hour {0}.", "It's a TV, not a touchscreen. Hour {0}." };
        static readonly string[] SmackLines = { "Percussive maintenance: failed.", "That worked in 1987. Not today.", "The paint keeps drying." };
        static readonly string[] PlantLines = { "Just a plant. No remote in there.", "Just dirt.", "The plant doesn't watch TV." };
        static readonly string[] CabinetLines = { "Drawers full of old cables. No remote.", "More cables. Still no remote.", "Cables, cables, a spoon. No remote." };

        enum Kind { None, ChUp, ChDown, Plug, Remote, Clock, Manual, Cushion, Cat, Screen, Plant, Cabinet, Other }

        sealed class Grip
        {
            public Kind kind;
            public int index;
            public Vector2 start, last;
            public float t0;
            public bool moved;
        }

        /// <summary>Un effetto (rimbalzo, oscillazione, tremolio) in corso su un oggetto e il valore a cui tornare.</summary>
        sealed class FxSlot
        {
            public Coroutine co;
            public Vector3 v;
            public Quaternion q;
        }

        readonly LostRemoteLogic logic = new LostRemoteLogic();
        readonly V64AImpulse shout = new V64AImpulse();
        V64AMic mic;
        Camera cam;
        HwToast toast;
        HwDragger dragger;
        HwDragger.Item plugItem, remoteItem;
        Grip grip;
        Coroutine[] cushionCo;
        bool[] cushionUp, cushionHeld, cushionReady;
        Vector3[] cushionHome;
        Quaternion[] cushionRot;
        Vector3 plugHome, plugRest, remoteScale, catScale, popupScale, tvPos;
        Quaternion plugRot, remoteRot;
        SpriteRenderer remoteArt;
        int remoteOrder;
        Coroutine staticCo, danceCo;
        readonly Dictionary<Transform, FxSlot>[] fxSlots = { new Dictionary<Transform, FxSlot>(), new Dictionary<Transform, FxSlot>(), new Dictionary<Transform, FxSlot>() };
        int coinLines, sockLines, mouseLines, catLines, emptyLines, deadLines, wrongLines, stoppedLines, shoutLines, tapLines, smackLines, plantLines, cabinetLines;
        float shakeReadyAt, shoutReadyAt, glintAt, clockAngle;
        bool busy, finishing, captured, manualOpen;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            StopAllCoroutines();
            staticCo = danceCo = null;
            StopFx();
            logic.Reset();
            ResetV62();
            shout.Reset();
            grip = null;
            busy = finishing = manualOpen = false;
            coinLines = sockLines = mouseLines = catLines = emptyLines = deadLines = wrongLines = stoppedLines = 0;
            shoutLines = tapLines = smackLines = plantLines = cabinetLines = 0;
            toast = new HwToast(infoText, infoHolder);
            int n = cushions != null ? cushions.Length : 0;
            if (!captured)
            {
                captured = true;
                tvPos = tv.localPosition;
                plugHome = plug.position;
                plugRot = plug.localRotation;
                remoteScale = remote.localScale;
                remoteRot = remote.localRotation;
                remoteArt = remote.GetComponentInChildren<SpriteRenderer>();
                remoteOrder = remoteArt != null ? remoteArt.sortingOrder : 0;
                catScale = cat.localScale;
                popupScale = manualPopup.localScale;
                cushionHome = new Vector3[n];
                cushionRot = new Quaternion[n];
                for (int i = 0; i < n; i++) { cushionHome[i] = cushions[i].localPosition; cushionRot[i] = cushions[i].localRotation; }
            }
            cushionCo = new Coroutine[n];
            cushionUp = new bool[n];
            cushionHeld = new bool[n];
            cushionReady = new bool[n];
            plugRest = plugHome;
            clockAngle = 0f;
            dragger = new HwDragger(this);
            plugItem = dragger.Add(plug, plugArea, 2);
            remoteItem = dragger.Add(remote, remoteArea, 3);
            if (mic != null) mic.Use(false);
            mic = new V64AMic(this);
            mic.Use(true);   // urlare al televisore è una delle prime cose che vengono in mente
            shakeReadyAt = Time.time + 0.5f;
            shoutReadyAt = Time.time + 1f;
            glintAt = float.MaxValue;
            RestPose();
        }

        // ---------- Pose (sempre ricavate dalla logica: uno stato valido dopo pausa/spegnimento) ----------

        void RestPose()
        {
            if (tv != null) { tv.localPosition = tvPos; tv.localRotation = Quaternion.identity; }
            ShowScreen();
            HalloweenKit.SetAlpha(staticFx, 0f);
            HalloweenKit.SetAlpha(buttonGlint, 0f);
            if (dancer != null) dancer.localRotation = Quaternion.identity;
            // spina
            plug.localRotation = plugRot;
            plug.position = logic.Plugged ? plugHome : plugRest;
            plugItem.Home = plug.position;
            plugItem.Enabled = true;
            UpdateCord();
            // orologio
            HalloweenKit.Show(clockBatteries, false);
            if (clockBatteries != null) clockBatteries.localPosition = Vector3.zero;
            // gatto e telecomando
            cat.localScale = new Vector3(catScale.x * (logic.CatRolled ? -1f : 1f), catScale.y, catScale.z);
            cat.localRotation = Quaternion.identity;
            HalloweenKit.Show(catEyes, false);
            HalloweenKit.Show(zzz, true);
            remote.localScale = remoteScale;
            remote.localRotation = remoteRot;
            remote.position = logic.RemoteFound ? remoteRest.position : remoteHidden.position;
            if (logic.RemoteFound) remote.localRotation = remoteRest.localRotation;
            if (remoteArt != null) remoteArt.sortingOrder = logic.RemoteFound ? remoteOrder + 4 : remoteOrder;
            remoteItem.Home = remote.position;
            remoteItem.Enabled = logic.RemoteFound;
            // cuscini giù
            for (int i = 0; i < cushions.Length; i++) CushionDown(i);
            // libretto
            HalloweenKit.Show(manualPopup, manualOpen);
            manualPopup.localScale = popupScale;
        }

        /// <summary>Lo schermo secondo la logica: spento, la vernice (in diretta o in replica) o la disco.</summary>
        void ShowScreen()
        {
            bool on = logic.Plugged;
            bool good = logic.Channel >= LostRemoteLogic.GoodChannel;
            HalloweenKit.Show(showPaint, on && !good);
            HalloweenKit.Show(showDisco, on && good);
            if (showPaint != null) showPaint.color = logic.OnRerun ? RerunTint : Color.white;
            if (showTitle != null)
            {
                HalloweenKit.Show(showTitle, on);
                showTitle.text = logic.ScreenTitle;
                showTitle.color = good ? TitleGood : logic.OnRerun ? RerunTint : TitleLive;
            }
            if (powerLed != null) powerLed.color = on ? new Color(1f, 0.25f, 0.2f) : new Color(0.35f, 0.15f, 0.15f);
        }

        void CushionDown(int i)
        {
            if (cushionCo != null && cushionCo[i] != null) { StopCoroutine(cushionCo[i]); cushionCo[i] = null; }
            cushions[i].localPosition = cushionHome[i];
            cushions[i].localRotation = cushionRot[i];
            if (cushionUp != null) cushionUp[i] = false;
            if (cushionHeld != null) cushionHeld[i] = false;
            if (cushionReady != null) cushionReady[i] = false;
            if (finds != null && i < finds.Length) HalloweenKit.Show(finds[i], false);
        }

        void UpdateCord()
        {
            if (cord == null || cordAnchor == null || cordEnd == null) return;
            Vector3 a = cordAnchor.position, b = cordEnd.position;
            if (cord.positionCount != 4) cord.positionCount = 4;
            float sag = Mathf.Clamp(0.5f - Vector2.Distance(a, b) * 0.2f, 0.08f, 0.5f);
            Vector3 m1 = Vector3.Lerp(a, b, 0.33f) + Vector3.down * sag;
            Vector3 m2 = Vector3.Lerp(a, b, 0.7f) + Vector3.down * sag * 0.8f;
            cord.SetPosition(0, new Vector3(a.x, a.y, 0f));
            cord.SetPosition(1, new Vector3(m1.x, m1.y, 0f));
            cord.SetPosition(2, new Vector3(m2.x, m2.y, 0f));
            cord.SetPosition(3, new Vector3(b.x, b.y, 0f));
        }

        // ---------- Ciclo di vita ----------

        void OnDisable()
        {
            // pausa/pubblicità/livello spento: microfono spento, le animazioni si fermano, si torna a uno stato valido
            if (mic != null) mic.Use(false);
            CancelGrip();
            busy = false;
            staticCo = danceCo = null;
            StopFx();
            if (cushionCo != null) for (int i = 0; i < cushionCo.Length; i++) cushionCo[i] = null;
            if (captured && !finishing && dragger != null && !IsFinished) RestPose();
        }

        void OnDestroy() { if (mic != null) mic.Use(false); }

        void OnEnable()
        {
            if (!captured || IsFinished || dragger == null) return;
            // spento a metà del cambio canale: si rifà il finale da capo
            if (logic.Stage == RemoteStage.Switching) { finishing = false; StartCoroutine(Finale(false)); }
        }

        void OnApplicationPause(bool paused) { if (paused) CancelGrip(); }
        void OnApplicationFocus(bool focus) { if (!focus) CancelGrip(); }

        /// <summary>Il dito sparisce (pausa, focus perso, animazione che parte): niente resta a metà.</summary>
        void CancelGrip()
        {
            Grip g = grip;
            grip = null;
            if (g != null && g.kind == Kind.Cushion && cushionHeld != null && g.index < cushionHeld.Length) cushionHeld[g.index] = false;
            if (dragger != null && dragger.Held != null)
            {
                HwDragger.Item it = dragger.Release();
                if (it == plugItem) { plugItem.Home = logic.Plugged ? plugHome : plugRest; dragger.SendHome(plugItem, 0.2f); }
                else if (it != null) dragger.SendHome(it, 0.2f);
            }
        }

        void Update()
        {
            toast?.Tick();
            if (IsFinished || finishing || toast == null) return;
            if (Time.timeScale <= 0f) { CancelGrip(); mic.Use(false); return; }   // pausa o pubblicità: niente input né microfono
            mic.Use(true);
            float dt = Time.deltaTime;
            Ambient();
            bool loud = shout.Feed(mic.Level(), dt);
            if (busy) { CancelGrip(); return; }
#if UNITY_EDITOR
            EditorKeys();
            if (busy || finishing) return;
#endif
            if (manualOpen) { ReadPointer(); return; }
            if (loud && Time.time >= shoutReadyAt) { shoutReadyAt = Time.time + ShoutCooldown; Shout(); return; }
            if (Inputs.Shake() > ShakeLevel && Time.time >= shakeReadyAt) { shakeReadyAt = Time.time + ShakeCooldown; Smack(); return; }
            ReadPointer();
        }

        /// <summary>Le piccole cose vive: i "zzz" del gatto, la lancetta dei secondi, il luccichio dei tastini.</summary>
        void Ambient()
        {
            if (zzz != null && zzz.gameObject.activeSelf)
            {
                Vector3 p = cat.TransformPoint(ZzzOffset);   // segue la testa anche a gatto girato
                zzz.position = new Vector3(p.x, p.y + 0.06f * Mathf.Sin(Time.time * 2f), zzz.position.z);
            }
            if (secondHand != null && logic.ClockRunning)
            {
                clockAngle = -6f * Mathf.Floor(Time.time);
                secondHand.localRotation = Quaternion.Euler(0f, 0f, clockAngle);
            }
            UpdateCord();
            if (buttonGlint == null || !logic.ManualRead) return;
            float k = Time.time - glintAt;
            if (k < 0f) return;
            if (k > 0.7f) { glintAt = Time.time + GlintEvery; HalloweenKit.SetAlpha(buttonGlint, 0f); return; }
            HalloweenKit.SetAlpha(buttonGlint, 0.95f * Mathf.Sin(k / 0.7f * Mathf.PI));
        }

        // ---------- Effetti che non si accavallano ----------

        // 0 = scala (rimbalzo), 1 = rotazione (oscillazione), 2 = posizione (tremolio): un effetto nuovo sullo stesso
        // oggetto ferma quello vecchio e riparte dal valore di riposo, così tocchi ripetuti non lasciano niente storto
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

        /// <summary>Ferma tutti gli effetti (le coroutine sono già ferme o si fermano qui) e rimette i valori di riposo.</summary>
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
            if (!HalloweenKit.ReadPointer(cam, out Vector2 w, out bool began, out bool ended))
            {
                if (grip != null) { Grip g0 = grip; End(g0); }
                return;
            }
            if (began) { if (grip != null) End(grip); grip = Begin(w); }
            if (grip == null) return;
            grip.last = w;
            if (Vector2.Distance(w, grip.start) > TapSlop * 0.4f) grip.moved = true;
            Drag(grip);
            if (ended && grip != null) End(grip);
        }

        Grip Begin(Vector2 w)
        {
            var g = new Grip { start = w, last = w, t0 = Time.time, kind = Kind.Other };
            if (busy || finishing) { g.kind = Kind.None; return g; }
            if (manualOpen) { CloseManual(); g.kind = Kind.None; return g; }
            // prima i tastini del fianco (aree generose), poi le cose che si trascinano, poi il resto
            if (chUpArea.OverlapPoint(w)) { g.kind = Kind.ChUp; Haptics.Tap(); return g; }
            if (chDownArea.OverlapPoint(w)) { g.kind = Kind.ChDown; Haptics.Tap(); return g; }
            HwDragger.Item it = dragger.TryPick(w);
            if (it == plugItem) { g.kind = Kind.Plug; return g; }
            if (it == remoteItem) { g.kind = Kind.Remote; return g; }
            if (clockArea.OverlapPoint(w)) { g.kind = Kind.Clock; return g; }
            if (manualArea.OverlapPoint(w)) { g.kind = Kind.Manual; return g; }
            for (int i = 0; i < cushionAreas.Length; i++)
                if (cushionAreas[i] != null && cushionAreas[i].OverlapPoint(w))
                {
                    g.kind = Kind.Cushion;
                    g.index = i;
                    LiftCushion(i);
                    return g;
                }
            if (catArea.OverlapPoint(w)) { g.kind = Kind.Cat; return g; }
            if (screenArea.OverlapPoint(w)) { g.kind = Kind.Screen; return g; }
            if (plantArea.OverlapPoint(w)) { g.kind = Kind.Plant; return g; }
            if (cabinetArea.OverlapPoint(w)) { g.kind = Kind.Cabinet; return g; }
            return g;
        }

        void Drag(Grip g)
        {
            switch (g.kind)
            {
                case Kind.Plug:
                    if (dragger.Held != plugItem || !g.moved) return;
                    // il filo è lungo quanto è lungo: la spina resta a portata del televisore
                    Vector2 a = cordAnchor.position;
                    Vector2 p = g.last;
                    if (Vector2.Distance(p, a) > CordReach) p = a + (p - a).normalized * CordReach;
                    dragger.MoveHeld(p);
                    break;
                case Kind.Remote:
                    if (dragger.Held == remoteItem && g.moved) dragger.MoveHeld(g.last);
                    break;
                case Kind.Cushion:
                    // il cuscino segue un po' il dito verso l'alto
                    if (cushionReady[g.index])
                    {
                        float up = Mathf.Clamp(g.last.y - g.start.y, 0f, 0.4f);
                        cushions[g.index].localPosition = cushionHome[g.index] + new Vector3(0f, CushionLift + up, 0f);
                    }
                    break;
            }
        }

        void End(Grip g)
        {
            grip = null;
            bool tap = Vector2.Distance(g.last, g.start) < TapSlop && Time.time - g.t0 <= TapTime + 0.6f;
            switch (g.kind)
            {
                case Kind.ChUp: if (tap) PressChannel(true); break;
                case Kind.ChDown: if (tap) PressChannel(false); break;
                case Kind.Plug: DropPlug(g); break;
                case Kind.Remote: DropRemote(g); break;
                case Kind.Clock: if (tap) TapClock(false); break;
                case Kind.Manual: if (tap) OpenManual(); break;
                case Kind.Cushion: ReleaseCushion(g.index); break;
                case Kind.Cat: if (tap) TapCat(); break;
                case Kind.Screen: if (tap) TapScreen(); break;
                case Kind.Plant:
                    if (!tap) break;
                    FxWiggle(plant, 6f, 0.5f, 25f);
                    toast.Show(PlantLines[plantLines++ % PlantLines.Length], 1.5f);
                    break;
                case Kind.Cabinet:
                    if (!tap) break;
                    Sfx.Play(this, "clunk", 0.4f);
                    toast.Show(CabinetLines[cabinetLines++ % CabinetLines.Length], 1.6f);
                    break;
            }
        }

        // ---------- Televisore ----------

        void TapScreen()
        {
            ShoutResult r = logic.TapScreen();
            if (r == ShoutResult.None) return;
            Haptics.Light();
            V72LostRemoteSound.Play(this, "tap", 0.5f);
            ShoutReaction(r, TapLines[tapLines++ % TapLines.Length], "It's off. Black and smug.");
        }

        void Shout()
        {
            ShoutResult r = logic.Shout();
            if (r == ShoutResult.None) return;
            ShoutReaction(r, ShoutLines[shoutLines++ % ShoutLines.Length], "It's off. It still can't hear you.");
        }

        void ShoutReaction(ShoutResult r, string liveLine, string offLine)
        {
            FxShake(tv, 0.03f, 0.25f);
            switch (r)
            {
                case ShoutResult.NextHour:
                    ShowScreen();
                    if (showTitle != null) FxPop(showTitle.transform, 1.25f, 0.3f);
                    toast.Show(string.Format(liveLine, logic.ShownHour), 1.6f);
                    break;
                case ShoutResult.Rerun: toast.Show("It's a rerun. It really can't hear you.", 1.7f); break;
                case ShoutResult.Off: toast.Show(offLine, 1.6f); break;
            }
        }

        void Smack()
        {
            bool on = logic.Smack();
            Haptics.Medium();
            Sfx.Play(this, "bonk", 0.6f);
            FxShake(tv, 0.08f, 0.45f);
            if (!on) { toast.Show("It's off. Hitting it won't help.", 1.6f); return; }
            Static(0.3f);
            toast.Show(SmackLines[smackLines++ % SmackLines.Length], 1.6f);
        }

        void PressChannel(bool up)
        {
            Transform button = up ? chUpButton : chDownButton;
            FlashButton(button);
            ChannelResult r = logic.PressChannel(up);
            switch (r)
            {
                case ChannelResult.None: return;
                case ChannelResult.Good: StartCoroutine(Finale(true)); return;
                case ChannelResult.NoPower:
                    V72LostRemoteSound.Play(this, "button", 0.5f);
                    toast.Show("Click. No power. Plug it back in.", 1.6f);
                    return;
            }
            V72LostRemoteSound.Play(this, "button", 0.6f);
            Haptics.Light();
            Static(0.35f);
            switch (r)
            {
                case ChannelResult.Down:
                    toast.Show(logic.Channel == LostRemoteLogic.LiveChannel - 1 ? "Even worse. It's a rerun." : "Hour " + logic.ShownHour + ". The director's cut.", 1.8f);
                    break;
                case ChannelResult.Bottom:
                    toast.Show("Lowest channel. It's paint all the way down.", 1.8f);
                    break;
                case ChannelResult.Up:
                    toast.Show(logic.OnRerun ? "Back up. Still paint." : "Live again. Hour " + logic.ShownHour + ".", 1.6f);
                    break;
            }
        }

        void FlashButton(Transform button)
        {
            if (button == null || buttonGlint == null) return;
            buttonGlint.transform.position = new Vector3(button.position.x, button.position.y, buttonGlint.transform.position.z);
            StartCoroutine(GlintOnce());
        }

        IEnumerator GlintOnce()
        {
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.35f)
            {
                HalloweenKit.SetAlpha(buttonGlint, Mathf.Sin(k * Mathf.PI));
                yield return null;
            }
            HalloweenKit.SetAlpha(buttonGlint, 0f);
        }

        /// <summary>Un attimo di neve, poi lo schermo secondo la logica.</summary>
        void Static(float seconds)
        {
            if (staticCo != null) StopCoroutine(staticCo);
            staticCo = StartCoroutine(StaticRun(seconds));
        }

        IEnumerator StaticRun(float seconds)
        {
            V72LostRemoteSound.Play(this, "static", 0.45f);
            for (float k = 0f; k < seconds; k += Time.deltaTime)
            {
                HalloweenKit.SetAlpha(staticFx, 0.75f + 0.25f * Mathf.Sin(k * 60f));
                if (staticFx != null) staticFx.flipX = Mathf.Repeat(k * 30f, 2f) > 1f;
                yield return null;
            }
            HalloweenKit.SetAlpha(staticFx, 0f);
            ShowScreen();
            staticCo = null;
        }

        // ---------- Spina ----------

        void DropPlug(Grip g)
        {
            if (dragger.Held == plugItem) dragger.Release();
            float toSocket = Vector2.Distance(plug.position, plugHome);
            if (!g.moved)
            {
                if (logic.Plugged)
                {
                    FxWiggle(plug, 8f, 0.3f, 30f);
                    toast.Show("It's plugged in. That part works.", 1.5f);
                }
                else Replug();
                return;
            }
            if (logic.Plugged)
            {
                if (toSocket < PlugPull) { dragger.SendTo(plugItem, plugHome, 0.15f, 0f); return; }
                logic.Unplug();
                Sfx.Play(this, "clunk", 0.6f);
                Haptics.Medium();
                plugRest = RestingSpot(plug.position);
                plugItem.Home = plugRest;
                dragger.SendTo(plugItem, plugRest, 0.15f, 0f);
                if (staticCo != null) { StopCoroutine(staticCo); staticCo = null; HalloweenKit.SetAlpha(staticFx, 0f); }
                ShowScreen();
                FxShake(tv, 0.02f, 0.2f);
                toast.Show("That's not changing the channel. That's giving up.", 2.2f);
                return;
            }
            if (toSocket < PlugSnap) { Replug(); return; }
            plugRest = RestingSpot(plug.position);
            plugItem.Home = plugRest;
            dragger.SendTo(plugItem, plugRest, 0.15f, 0f);
        }

        /// <summary>Dove resta la spina staccata: dentro lo schermo, sotto la barra in alto.</summary>
        Vector3 RestingSpot(Vector3 p)
        {
            Vector2 c = HalloweenKit.ClampSafe(p);
            c.x = Mathf.Clamp(c.x, -2.2f, 2.2f);
            c.y = Mathf.Clamp(c.y, -3.2f, 2.6f);
            return new Vector3(c.x, c.y, plug.position.z);
        }

        void Replug()
        {
            if (!logic.Replug()) return;
            Sfx.Play(this, "clunk", 0.5f);
            Haptics.Light();
            plugItem.Home = plugHome;
            plugRest = plugHome;
            dragger.SendTo(plugItem, plugHome, 0.2f, 0.1f);
            Static(0.4f);
            toast.Show("Hour " + logic.ShownHour + ". You missed nothing.", 1.8f);
        }

        // ---------- Libretto ----------

        void OpenManual()
        {
            if (busy) return;
            logic.ReadManual();
            manualOpen = true;
            CancelGrip();
            HalloweenKit.Show(manualPopup, true);
            manualPopup.localScale = popupScale;
            FxPop(manualPopup, 1.06f, 0.25f);
            FxPop(manual, 1.15f, 0.2f);
            V72LostRemoteSound.Play(this, "page", 0.5f);
            toast.Show("Page 1. The only page anyone needs.", 1.8f);
        }

        void CloseManual()
        {
            if (!manualOpen) return;
            manualOpen = false;
            HalloweenKit.Show(manualPopup, false);
            V72LostRemoteSound.Play(this, "page", 0.35f);
            // un luccichio sui tastini veri appena chiuso il libretto, poi ogni tanto
            if (buttonGlint != null && chUpButton != null && chDownButton != null)
                buttonGlint.transform.position = new Vector3((chUpButton.position.x + chDownButton.position.x) * 0.5f,
                    (chUpButton.position.y + chDownButton.position.y) * 0.5f, buttonGlint.transform.position.z);
            glintAt = Time.time + 0.4f;
        }

        // ---------- Divano ----------

        void LiftCushion(int i)
        {
            cushionHeld[i] = true;
            if (cushionUp[i]) return;   // già su: lo si tiene e basta
            CushionFind f = logic.LiftCushion(i);
            if (f == CushionFind.None) return;
            cushionUp[i] = true;
            if (cushionCo[i] != null) StopCoroutine(cushionCo[i]);
            cushionCo[i] = StartCoroutine(CushionRun(i));
            V72LostRemoteSound.Play(this, "flump", 0.5f);
            Haptics.Light();
            switch (f)
            {
                case CushionFind.Coins:
                    V72LostRemoteSound.Play(this, "coins", 0.45f);
                    toast.Show(CoinLines[coinLines++ % CoinLines.Length], 1.7f);
                    break;
                case CushionFind.Sock:
                    toast.Show(SockLines[sockLines++ % SockLines.Length], 1.7f);
                    break;
                case CushionFind.Mouse:
                    V64ASound.Play(this, "squeak", 0.25f);
                    toast.Show(MouseLines[mouseLines++ % MouseLines.Length], 1.9f);
                    break;
            }
        }

        /// <summary>Il cuscino si alza, resta su finché il dito c'è (più un attimo per guardare), poi ricade.</summary>
        IEnumerator CushionRun(int i)
        {
            Transform c = cushions[i];
            if (finds != null && i < finds.Length) HalloweenKit.Show(finds[i], true);
            float tilt = i == 0 ? 10f : i == 2 ? -10f : 6f;
            Vector3 up = cushionHome[i] + new Vector3(0f, CushionLift, 0f);
            Quaternion r1 = cushionRot[i] * Quaternion.Euler(0f, 0f, tilt);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.18f)
            {
                c.localPosition = Vector3.Lerp(cushionHome[i], up, k);
                c.localRotation = Quaternion.Slerp(cushionRot[i], r1, k);
                yield return null;
            }
            c.localPosition = up;
            c.localRotation = r1;
            cushionReady[i] = true;
            float downAt = Time.time + 0.9f;
            while (true)
            {
                if (cushionHeld[i]) downAt = Time.time + 0.9f;
                else if (Time.time >= downAt) break;
                yield return null;
            }
            cushionReady[i] = false;
            Vector3 p0 = c.localPosition;
            Quaternion r0 = c.localRotation;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.18f)
            {
                c.localPosition = Vector3.Lerp(p0, cushionHome[i], k * k);
                c.localRotation = Quaternion.Slerp(r0, cushionRot[i], k);
                yield return null;
            }
            V72LostRemoteSound.Play(this, "flump", 0.35f);
            cushionCo[i] = null;
            CushionDown(i);
        }

        void ReleaseCushion(int i)
        {
            if (i < 0 || i >= cushionHeld.Length) return;
            cushionHeld[i] = false;
        }

        // ---------- Gatto, telecomando, orologio ----------

        void TapCat()
        {
            CatResult r = logic.TapCat();
            if (r == CatResult.None) return;
            if (r == CatResult.RollsOver) { StartCoroutine(RollOver()); return; }
            FxWiggle(cat, 3f, 0.4f, 20f);
            Sfx.Play(this, "snore", 0.4f);
            toast.Show(CatLines[catLines++ % CatLines.Length], 1.4f);
        }

        IEnumerator RollOver()
        {
            busy = true;
            CancelGrip();
            Sfx.Play(this, "snore", 0.5f);
            toast.Show("Mrrrp. The cat rolls over...", 1.4f);
            Vector3 home = cat.localPosition;
            float s0 = cat.localScale.x;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.45f)
            {
                cat.localScale = new Vector3(Mathf.Lerp(s0, -s0, k), catScale.y * (1f + 0.08f * Mathf.Sin(k * Mathf.PI)), catScale.z);
                cat.localPosition = home + new Vector3(0f, 0.12f * Mathf.Sin(k * Mathf.PI), 0f);
                yield return null;
            }
            cat.localScale = new Vector3(-s0, catScale.y, catScale.z);
            cat.localPosition = home;
            // il telecomando scivola fuori da sotto il cuscino
            if (remoteArt != null) remoteArt.sortingOrder = remoteOrder + 4;
            V72LostRemoteSound.Play(this, "flump", 0.4f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.35f)
            {
                remote.position = Vector3.Lerp(remoteHidden.position, remoteRest.position, 1f - (1f - k) * (1f - k));
                remote.localRotation = Quaternion.Slerp(remoteRot, remoteRest.localRotation, k);
                yield return null;
            }
            remote.position = remoteRest.position;
            remote.localRotation = remoteRest.localRotation;
            remoteItem.Home = remote.position;
            remoteItem.Enabled = true;
            FxPop(remote, 1.3f, 0.3f);
            Sfx.Play(this, "ding", 0.4f);
            toast.Show("THE REMOTE! It was under the cat.", 2f);
            busy = false;
        }

        void DropRemote(Grip g)
        {
            if (dragger.Held == remoteItem) dragger.Release();
            // portato sull'orologio: gli si rubano le pile
            if (g.moved && (clockArea.OverlapPoint(remote.position) || Vector2.Distance(remote.position, clock.position) < 0.8f))
            {
                dragger.SendHome(remoteItem, 0.3f);
                TapClock(true);
                return;
            }
            if (g.moved) dragger.SendHome(remoteItem, 0.3f);
            PressRemote();
        }

        void PressRemote()
        {
            RemoteResult r = logic.PressRemote();
            if (r == RemoteResult.None) return;
            V72LostRemoteSound.Play(this, "click", 0.6f);
            Haptics.Tap();
            FxPop(remote, 1.15f, 0.2f);
            if (r == RemoteResult.Empty) toast.Show(EmptyLines[emptyLines++ % EmptyLines.Length], 1.7f);
            else toast.Show(DeadLines[deadLines++ % DeadLines.Length], 1.7f);
        }

        void TapClock(bool fromRemote)
        {
            ClockResult r = logic.TapClock();
            switch (r)
            {
                case ClockResult.None: return;
                case ClockResult.BatteriesTaken: StartCoroutine(StealBatteries()); return;
                case ClockResult.Wrong:
                    FxWiggle(clock, 5f, 0.4f, 25f);
                    Sfx.Play(this, "tick", 0.6f);
                    toast.Show(WrongLines[wrongLines++ % WrongLines.Length], 1.8f);
                    return;
                case ClockResult.Stopped:
                    FxWiggle(clock, 3f, 0.3f, 25f);
                    toast.Show(StoppedLines[stoppedLines++ % StoppedLines.Length], 1.6f);
                    if (fromRemote) PressRemote();
                    return;
            }
        }

        IEnumerator StealBatteries()
        {
            busy = true;
            CancelGrip();
            V72LostRemoteSound.Play(this, "click", 0.5f);
            HalloweenKit.Show(clockBatteries, true);
            clockBatteries.position = clock.position;
            yield return HalloweenKit.Move(clockBatteries, clock.position + new Vector3(0f, -0.25f, 0f), 0.15f);
            toast.Show("The clock stops. It was three hours wrong anyway.", 2f);
            yield return HalloweenKit.Move(clockBatteries, remote.position, 0.55f, 0.6f);
            HalloweenKit.Show(clockBatteries, false);
            clockBatteries.localPosition = Vector3.zero;
            V72LostRemoteSound.Play(this, "click", 0.5f);
            FxPop(remote, 1.2f, 0.2f);
            yield return new WaitForSeconds(1.2f);
            busy = false;
            PressRemote();   // "These are dead too."
        }

        // ---------- La soluzione ----------

        IEnumerator Finale(bool pressed)
        {
            finishing = true;
            busy = false;
            CancelGrip();
            mic.Use(false);
            manualOpen = false;
            HalloweenKit.Show(manualPopup, false);
            if (staticCo != null) { StopCoroutine(staticCo); staticCo = null; }
            if (pressed) { V72LostRemoteSound.Play(this, "button", 0.7f); Haptics.Medium(); }
            // neve e scossone
            V72LostRemoteSound.Play(this, "static", 0.6f);
            HalloweenKit.Show(showTitle, false);
            for (float k = 0f; k < 0.6f; k += Time.deltaTime)
            {
                HalloweenKit.SetAlpha(staticFx, 0.8f + 0.2f * Mathf.Sin(k * 60f));
                if (staticFx != null) staticFx.flipX = Mathf.Repeat(k * 30f, 2f) > 1f;
                yield return null;
            }
            HalloweenKit.SetAlpha(staticFx, 0f);
            // il canale bello
            ShowScreen();
            V72LostRemoteSound.Play(this, "jingle", 0.6f);
            if (showTitle != null) FxPop(showTitle.transform, 1.3f, 0.35f);
            if (danceCo != null) StopCoroutine(danceCo);
            danceCo = StartCoroutine(Dance());
            yield return new WaitForSeconds(0.4f);
            // il gatto si sveglia e si gira verso il televisore
            HalloweenKit.Show(zzz, false);
            HalloweenKit.Show(catEyes, true);
            cat.localScale = catScale;
            Sfx.Play(this, "pop", 0.5f);
            FxPop(cat, 1.12f, 0.35f);
            yield return new WaitForSeconds(0.35f);
            FxWiggle(cat, 4f, 1.2f, 14f);
            toast.Show("TVs HAD BUTTONS ALL ALONG.", 2.6f);
            yield return Celebrate(1.4f);
            if (logic.TryComplete()) Win();
        }

        IEnumerator Dance()
        {
            for (float t = 0f; ; t += Time.deltaTime)
            {
                if (dancer != null)
                {
                    dancer.localRotation = Quaternion.Euler(0f, 0f, 14f * Mathf.Sin(t * 9f));
                    dancer.localScale = Vector3.one * (1f + 0.06f * Mathf.Abs(Mathf.Sin(t * 9f)));
                }
                yield return null;
            }
        }

#if UNITY_EDITOR
        // ---------- Tasti di prova nell'Editor ----------

        void EditorKeys()
        {
            if (Input.GetKeyDown(KeyCode.C)) { if (manualOpen) CloseManual(); PressChannel(true); return; }
            if (Input.GetKeyDown(KeyCode.V)) { if (manualOpen) CloseManual(); PressChannel(false); return; }
            if (Input.GetKeyDown(KeyCode.M)) { if (manualOpen) CloseManual(); else OpenManual(); return; }
            if (manualOpen) return;
            if (Input.GetKeyDown(KeyCode.T)) TapCat();
            if (Input.GetKeyDown(KeyCode.R)) PressRemote();
            if (Input.GetKeyDown(KeyCode.K)) TapClock(false);
            if (Input.GetKeyDown(KeyCode.Y)) Shout();
            for (int i = 0; i < cushions.Length && i < 3; i++)
                if (Input.GetKeyDown(KeyCode.Alpha1 + i)) { LiftCushion(i); cushionHeld[i] = false; }
            if (Input.GetKeyDown(KeyCode.U))
            {
                if (logic.Plugged)
                {
                    plug.position = RestingSpot(plugHome + new Vector3(-0.2f, -1.0f, 0f));
                    DropPlug(new Grip { kind = Kind.Plug, moved = true });
                }
                else Replug();
            }
        }
#endif
    }

    /// <summary>Suoni sintetizzati del livello 270: tasti, neve del televisore, cuscini, monetine, pagina, sigla.</summary>
    public static class V72LostRemoteSound
    {
        const int Rate = 22050;
        static readonly Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>();
        static readonly System.Random rnd = new System.Random(270);

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
                case "click":    // tasto di gomma del telecomando: due clic secchi
                    c = Make(kind, 0.16f, t => (N() * 0.5f + Mathf.Sin(t * 2f * Mathf.PI * 1800f) * 0.3f) * (Mathf.Exp(-t * 120f) + (t > 0.08f ? Mathf.Exp(-(t - 0.08f) * 120f) : 0f)));
                    break;
                case "button":   // tastino del televisore: clac di plastica dura
                    c = Make(kind, 0.1f, t => (Mathf.Sin(t * 2f * Mathf.PI * 950f) * 0.6f + N() * 0.3f) * Mathf.Exp(-t * 55f));
                    break;
                case "tap":      // dito sul vetro
                    c = Make(kind, 0.08f, t => Mathf.Sin(t * 2f * Mathf.PI * 2400f) * 0.35f * Mathf.Exp(-t * 70f));
                    break;
                case "static":   // neve del televisore
                    c = Make(kind, 0.6f, t => N() * 0.35f * Mathf.Min(1f, t * 40f) * Mathf.Min(1f, (0.6f - t) * 12f));
                    break;
                case "flump":    // cuscino alzato/rimesso
                    {
                        float lp = 0f;
                        c = Make(kind, 0.25f, t => { lp += (N() - lp) * 0.06f; return lp * 2.2f * Mathf.Sin(t / 0.25f * Mathf.PI); });
                    }
                    break;
                case "coins":    // due monetine che tintinnano
                    c = Make(kind, 0.35f, t => (Mathf.Sin(t * 2f * Mathf.PI * 3300f) * Mathf.Exp(-t * 18f) + (t > 0.12f ? Mathf.Sin(t * 2f * Mathf.PI * 3900f) * Mathf.Exp(-(t - 0.12f) * 18f) : 0f)) * 0.3f);
                    break;
                case "page":     // pagina sfogliata
                    c = Make(kind, 0.25f, t => N() * 0.3f * Mathf.Sin(t / 0.25f * Mathf.PI) * (0.5f + 0.5f * Mathf.Sin(t * 2f * Mathf.PI * 25f)));
                    break;
                case "jingle":   // la sigla della disco: arpeggio allegro
                    c = Make(kind, 0.9f, t =>
                    {
                        float[] notes = { 523f, 659f, 784f, 1047f, 784f, 1047f };
                        int i = Mathf.Min(notes.Length - 1, (int)(t / 0.15f));
                        float u = t - i * 0.15f;
                        return (Mathf.Sign(Mathf.Sin(t * 2f * Mathf.PI * notes[i])) * 0.18f + Mathf.Sin(t * 2f * Mathf.PI * notes[i] * 2f) * 0.1f) * Mathf.Exp(-u * 9f);
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
            AudioClip clip = AudioClip.Create("v72_" + name, n, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
