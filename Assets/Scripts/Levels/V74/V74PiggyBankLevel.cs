using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels.V64;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 273 "The Piggy Bank" — "Get the money out!" La cameretta: sulla scrivania il salvadanaio di ceramica
    /// Mr. Oink pieno di monete (si vede il bordo d'oro nella fessura, tintinna); sul tappeto il coltello da burro e il
    /// martello, per terra l'orsetto; il bambino preoccupato accanto alla scrivania; sul muro il biglietto "Ice cream truck in
    /// 30 seconds!" e la finestra sulla strada, da cui si sente il motivetto del furgone dei gelati che si avvicina
    /// (solo atmosfera: niente sconfitta).
    /// False piste (nessuna azzera i progressi, niente sconfitta): il martello portato sul salvadanaio (il bambino si
    /// mette in mezzo a braccia tese, "NO!", il martello rimbalza: non si rompe MAI — "Not Mr. Oink!"), lo scossone
    /// del telefono (una monetina alla volta, lentissimo: "At this rate, the truck leaves in 2031."; i centesimi
    /// raccolti restano sulla scrivania, ma dopo una dozzina la fessura non ne lascia passare più), il coltello da
    /// burro nella fessura (una monetina scivola fuori, poi il coltello si incastra e salta via), il tocco sulla
    /// fessura ("Coins go IN there. Not out."), il bambino, l'orsetto, il biglietto, la finestra.
    /// Indizio: l'adesivo con la freccia in giù sul fianco del maialino; dal secondo scossone si sente anche un "thup"
    /// gommoso "da sotto"; girandolo si vede il tappo di gomma sotto la pancia.
    /// Soluzione: trascinare il salvadanaio (sollevarlo/girarlo: basta un trascinamento qualsiasi di mezza unità, area
    /// generosa) -> si capovolge a pancia all'aria e si vede il tappo; tirare via il tappo (trascinarlo) -> il bambino
    /// lo solleva e tutte le monete escono a pioggia -> "PIGGY BANKS HAVE A PLUG." -> "MR. OINK IS SAFE." -> vittoria.
    /// Niente microfono. Pausa/focus perso: il dito si annulla e la scena torna a una posa ricavata dalla logica (un
    /// finale interrotto si rifà da capo).
    /// La logica è in Gotcha.Levels.V64.PiggyBankLogic (senza Unity).
    /// Tasti di prova nell'Editor: F gira il salvadanaio, P tira il tappo, H martello, K coltello, O fessura,
    /// C bambino, B orsetto, N biglietto, W finestra (S = scossone, da Inputs).
    /// </summary>
    public class V74PiggyBankLevel : V62Level
    {
        [Header("Salvadanaio")]
        [SerializeField] Transform piggy;               // il maialino (si solleva e ruota attorno al centro della pancia)
        [SerializeField] Collider2D piggyArea;          // fermo sulla scrivania, generoso
        [SerializeField] Collider2D slotArea;           // figlio del maialino (la fessura in alto)
        [SerializeField] Transform slotMouth;           // figlio del maialino: da qui escono le monetine
        [SerializeField] Transform plug;                // figlio del maialino, sotto la pancia (si vede solo da capovolto)
        [SerializeField] Collider2D plugArea;           // figlio del maialino, generoso
        [SerializeField] Transform plugHole;            // figlio del maialino: il buco lasciato dal tappo
        [SerializeField] Transform plugRest;            // dove cade il tappo tolto (sulla scrivania)
        [Header("Monete")]
        [SerializeField] Transform[] stackCoins;        // le monetine raccolte sulla scrivania (al massimo 6 visibili)
        [SerializeField] Transform dropCoin;            // la monetina che esce (scossone, coltello, capovolta)
        [SerializeField] Transform[] showerCoins;       // la pioggia del finale
        [SerializeField] Transform coinPile;            // il mucchio del finale
        [SerializeField] Transform[] sparkles;
        [SerializeField] TextMesh centsLabel;
        [Header("Bambino")]
        [SerializeField] Transform kid;
        [SerializeField] Collider2D kidArea;
        [SerializeField] SpriteRenderer kidArt;
        [SerializeField] Sprite kidWorried;
        [SerializeField] Sprite kidNo;
        [SerializeField] Sprite kidCheer;
        [SerializeField] Transform noBurst;             // lo scoppio "NO!" sopra il salvadanaio
        [Header("Attrezzi")]
        [SerializeField] Transform hammer;
        [SerializeField] Collider2D hammerArea;
        [SerializeField] Transform knife;
        [SerializeField] Collider2D knifeArea;
        [Header("Stanza")]
        [SerializeField] Transform teddy;
        [SerializeField] Collider2D teddyArea;
        [SerializeField] Transform note;
        [SerializeField] Collider2D noteArea;
        [SerializeField] Collider2D windowArea;
        [SerializeField] Transform truck;               // il furgone dei gelati nella finestra (solo nel finale)
        [SerializeField] TextMesh infoText;
        [SerializeField] Transform infoHolder;

        const float TapSlop = 0.6f, MoveSlop = 0.2f, TapTime = 1.2f;
        const float FlipLift = 0.1f, ShakeLevel = 12f, ShakeCooldown = 0.9f, PlugNudgeEvery = 5f;
        const float JingleEvery = 9f, JingleFirst = 2.5f;

        static readonly string[] HammerLines = { "Not Mr. Oink!", "NO! Mr. Oink is family.", "The kid won't let you. Ever.", "Put. The hammer. Down." };
        static readonly string[] ShakeLines = { "...that rattle came from underneath.", "At this rate, the truck leaves in 2031.", "Ice cream costs $2.50. Keep shaking. For a year.", "Clink. Thup? Something under the belly." };
        static readonly string[] JamLines = { "Nothing. The coins are stuck sideways.", "Shake all you want. Mr. Oink holds on.", "Not even a cent. The slot is jammed." };
        static readonly string[] SlotLines = { "Coins go IN there. Not out.", "It's a one-way street.", "In: yes. Out: no. That's the deal." };
        static readonly string[] PiggyLines = { "Jingle jingle. Mr. Oink is FULL.", "A little sticker on its side: an arrow pointing down.", "Mr. Oink smiles. Mr. Oink keeps the money." };
        static readonly string[] FlippedLines = { "Legs in the air. Very dignified.", "Mr. Oink, belly up.", "Upside down. Still smiling." };
        static readonly string[] PlugLines = { "A rubber plug...?", "It wiggles. It's squishy.", "A plug. On a piggy bank. Hm." };
        static readonly string[] KidLines = { "That's Mr. Oink. Be gentle!", "The truck is coming! Do something!", "Mr. Oink has been with me since I was FOUR." };
        static readonly string[] TeddyLines = { "Teddy is broke.", "Teddy has no pockets.", "Teddy saw nothing." };
        static readonly string[] WindowLines = { "Not here yet. But you can hear it.", "Ding-a-ling... getting closer.", "Somewhere out there: ice cream." };
        static readonly string[] KnifeTapLines = { "A butter knife. For butter. Usually.", "Thin enough for the slot?" };
        static readonly string[] HammerTapLines = { "A hammer. Tempting...", "The kid is watching you." };

        enum Kind { None, Piggy, Plug, Tool, Kid, Teddy, Note, Window, Other }

        sealed class Grip
        {
            public Kind kind;
            public Vector2 start, last;
            public float t0, maxDist;
            public bool moved, onSlot;
        }

        /// <summary>Un effetto (rimbalzo, oscillazione, tremolio) in corso su un oggetto e il valore a cui tornare.</summary>
        sealed class FxSlot
        {
            public Coroutine co;
            public Vector3 v;
            public Quaternion q;
        }

        readonly PiggyBankLogic logic = new PiggyBankLogic();
        Camera cam;
        HwToast toast;
        HwDragger dragger;
        HwDragger.Item hammerItem, knifeItem;
        Grip grip;
        Vector3 piggyHome, kidHome, plugLocal, truckHome, pileScale, piggyScale, noScale;
        Vector3[] stackHome, sparkleHome;
        Quaternion hammerRot, knifeRot, plugRot;
        Coroutine coinCo;
        readonly Dictionary<Transform, FxSlot>[] fxSlots = { new Dictionary<Transform, FxSlot>(), new Dictionary<Transform, FxSlot>(), new Dictionary<Transform, FxSlot>() };
        int hammerLines, shakeLines, jamLines, slotLines, piggyLines, flippedLines, plugLines, kidLines, teddyLines, windowLines, knifeTapLines, hammerTapLines;
        float shakeReadyAt, nextJingle, startedAt, plugNudgeAt;
        bool busy, finishing, captured, plugHeld;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            StopAllCoroutines();
            StopFx();
            coinCo = null;
            logic.Reset();
            ResetV62();
            grip = null;
            busy = finishing = plugHeld = false;
            hammerLines = shakeLines = jamLines = slotLines = piggyLines = flippedLines = plugLines = 0;
            kidLines = teddyLines = windowLines = knifeTapLines = hammerTapLines = 0;
            toast = new HwToast(infoText, infoHolder);
            if (!captured)
            {
                captured = true;
                piggyHome = piggy.localPosition;
                piggyScale = piggy.localScale;
                kidHome = kid.localPosition;
                plugLocal = plug.localPosition;
                plugRot = plug.localRotation;
                truckHome = truck.localPosition;
                pileScale = coinPile.localScale;
                noScale = noBurst.localScale;
                hammerRot = hammer.localRotation;
                knifeRot = knife.localRotation;
                stackHome = new Vector3[stackCoins.Length];
                for (int i = 0; i < stackCoins.Length; i++) stackHome[i] = stackCoins[i].localPosition;
                sparkleHome = new Vector3[sparkles.Length];
                for (int i = 0; i < sparkles.Length; i++) sparkleHome[i] = sparkles[i].localPosition;
            }
            dragger = new HwDragger(this);
            hammerItem = dragger.Add(hammer, hammerArea, 2);
            knifeItem = dragger.Add(knife, knifeArea, 2);
            startedAt = Time.time;
            shakeReadyAt = Time.time + 0.5f;
            nextJingle = Time.time + JingleFirst;
            plugNudgeAt = Time.time + PlugNudgeEvery;
            RestPose();
        }

        // ---------- Pose (sempre ricavate dalla logica: uno stato valido dopo pausa/spegnimento) ----------

        void RestPose()
        {
            PiggyRest();
            kid.localPosition = kidHome;
            kid.localRotation = Quaternion.identity;
            if (kidArt != null) kidArt.sprite = logic.Stage == PiggyStage.Done ? kidCheer : kidWorried;
            HalloweenKit.Show(noBurst, false);
            noBurst.localScale = noScale;
            if (coinCo != null) { StopCoroutine(coinCo); coinCo = null; }
            HalloweenKit.Show(dropCoin, false);
            SyncCoins();
            foreach (Transform c in showerCoins) HalloweenKit.Show(c, false);
            HalloweenKit.Show(coinPile, false);
            coinPile.localScale = pileScale;
            for (int i = 0; i < sparkles.Length; i++)
            {
                sparkles[i].localPosition = sparkleHome[i];
                HalloweenKit.Show(sparkles[i], false);
            }
            HalloweenKit.Show(truck, false);
            truck.localPosition = truckHome;
            foreach (HwDragger.Item it in new[] { hammerItem, knifeItem })
            {
                dragger.Stop(it);
                it.T.position = it.Home;
                it.Enabled = true;
            }
            hammer.localRotation = hammerRot;
            knife.localRotation = knifeRot;
        }

        /// <summary>Il maialino dove dice la logica: in piedi o a pancia all'aria; il tappo si vede solo da capovolto.</summary>
        void PiggyRest()
        {
            plugHeld = false;
            piggy.localScale = piggyScale;
            piggy.localPosition = piggyHome + (logic.Flipped ? new Vector3(0f, FlipLift, 0f) : Vector3.zero);
            piggy.localRotation = Quaternion.Euler(0f, 0f, logic.Flipped ? 180f : 0f);
            if (!logic.PlugOut)
            {
                if (plug.parent != piggy) plug.SetParent(piggy, false);
                plug.localPosition = plugLocal;
                plug.localRotation = plugRot;
                plug.localScale = Vector3.one;
            }
            HalloweenKit.Show(plug, logic.Flipped && !logic.PlugOut);
            HalloweenKit.Show(plugHole, logic.PlugOut);
        }

        /// <summary>Le monetine raccolte sulla scrivania e il contatore, dalla logica.</summary>
        void SyncCoins()
        {
            int shown = logic.PlugOut ? 0 : Mathf.Min(logic.Cents, stackCoins.Length);
            for (int i = 0; i < stackCoins.Length; i++)
            {
                stackCoins[i].localPosition = stackHome[i];
                HalloweenKit.Show(stackCoins[i], i < shown);
            }
            HalloweenKit.Show(centsLabel, logic.Cents > 0);
            if (centsLabel != null) centsLabel.text = PiggyBankLogic.CentsLabel(logic.Cents);
        }

        // ---------- Ciclo di vita ----------

        void OnDisable()
        {
            // pausa/pubblicità/livello spento: niente resta a metà, si torna a una posa ricavata dalla logica
            CancelGrip();
            busy = false;
            StopFx();
            coinCo = null;
            if (captured && dragger != null && !IsFinished && !finishing) RestPose();
        }

        void OnEnable()
        {
            if (!captured || IsFinished || dragger == null) return;
            // spento a metà del finale: si rifà da capo
            if (logic.Stage == PiggyStage.Pouring) { finishing = false; StartCoroutine(Pour()); }
        }

        void OnApplicationPause(bool paused) { if (paused) CancelGrip(); }
        void OnApplicationFocus(bool focus) { if (!focus) CancelGrip(); }

        /// <summary>Il dito sparisce (pausa, focus perso, animazione che parte): niente resta a metà.</summary>
        void CancelGrip()
        {
            Grip g = grip;
            grip = null;
            if (captured && !finishing && (plugHeld || (g != null && (g.kind == Kind.Piggy || g.kind == Kind.Plug))) && piggy != null) PiggyRest();
            if (dragger != null && dragger.Held != null)
            {
                HwDragger.Item it = dragger.Release();
                if (it != null) dragger.SendHome(it, 0.2f);
            }
        }

        void Update()
        {
            toast?.Tick();
            if (IsFinished || toast == null || finishing) return;
            if (Time.timeScale <= 0f) { CancelGrip(); return; }   // pausa o pubblicità: niente input
            Ambient();
            if (busy) { CancelGrip(); return; }
#if UNITY_EDITOR
            EditorKeys();
            if (busy || finishing) return;
#endif
            if (!logic.InputLocked && Inputs.Shake() > ShakeLevel && Time.time >= shakeReadyAt && grip == null)
            {
                shakeReadyAt = Time.time + ShakeCooldown;
                Shake();
                return;
            }
            ReadPointer();
        }

        /// <summary>Il motivetto del furgone che si avvicina (sempre più forte, ma niente sconfitta) e il tappo che si muove.</summary>
        void Ambient()
        {
            if (Time.time >= nextJingle)
            {
                nextJingle = Time.time + JingleEvery;
                float near = Mathf.Clamp01((Time.time - startedAt) / 60f);
                V74APiggySound.Play(this, "icecream", Mathf.Lerp(0.06f, 0.2f, near));
            }
            if (logic.Flipped && !logic.PlugOut && !plugHeld && Time.time >= plugNudgeAt)
            {
                plugNudgeAt = Time.time + PlugNudgeEvery;
                FxWiggle(plug, 10f, 0.35f, 30f);
            }
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

        void StopFx()
        {
            for (int kind = 0; kind < fxSlots.Length; kind++)
            {
                foreach (KeyValuePair<Transform, FxSlot> kv in fxSlots[kind])
                    if (kv.Value.co != null) { StopCoroutine(kv.Value.co); Restore(kind, kv.Key, kv.Value); kv.Value.co = null; }
                fxSlots[kind].Clear();
            }
        }

        /// <summary>Ferma gli effetti in corso su un oggetto (prima di animarlo a mano).</summary>
        void StopFxOn(Transform t)
        {
            for (int kind = 0; kind < fxSlots.Length; kind++)
                if (fxSlots[kind].TryGetValue(t, out FxSlot slot))
                {
                    if (slot.co != null) { StopCoroutine(slot.co); Restore(kind, t, slot); }
                    fxSlots[kind].Remove(t);
                }
        }

        // ---------- Dito ----------

        void ReadPointer()
        {
            if (!HalloweenKit.ReadPointer(cam, out Vector2 w, out bool began, out bool ended))
            {
                if (grip != null) End(grip);
                return;
            }
            if (began) { if (grip != null) End(grip); grip = Begin(w); }
            if (grip == null) return;
            grip.last = w;
            float dist = Vector2.Distance(w, grip.start);
            grip.maxDist = Mathf.Max(grip.maxDist, dist);
            if (dist > MoveSlop) grip.moved = true;
            Drag(grip);
            if (grip != null && ended) End(grip);
        }

        Grip Begin(Vector2 w)
        {
            var g = new Grip { start = w, last = w, t0 = Time.time, kind = Kind.Other };
            if (busy || finishing || logic.InputLocked) { g.kind = Kind.None; return g; }
            // prima il tappo (solo da capovolto), poi gli attrezzi, il salvadanaio e il resto
            if (logic.Flipped && plug.gameObject.activeInHierarchy && plugArea.OverlapPoint(w))
            {
                g.kind = Kind.Plug;
                StopFxOn(plug);
                plugHeld = true;
                Haptics.Tap();
                return g;
            }
            HwDragger.Item it = dragger.TryPick(w);
            if (it != null) { g.kind = Kind.Tool; return g; }
            if (piggyArea.OverlapPoint(w))
            {
                g.kind = Kind.Piggy;
                g.onSlot = !logic.Flipped && slotArea.OverlapPoint(w);
                StopFxOn(piggy);
                return g;
            }
            if (kidArea.OverlapPoint(w)) { g.kind = Kind.Kid; return g; }
            if (teddyArea.OverlapPoint(w)) { g.kind = Kind.Teddy; return g; }
            if (noteArea.OverlapPoint(w)) { g.kind = Kind.Note; return g; }
            if (windowArea.OverlapPoint(w)) { g.kind = Kind.Window; return g; }
            return g;
        }

        void Drag(Grip g)
        {
            switch (g.kind)
            {
                case Kind.Tool:
                    if (dragger.Held != null && g.moved) dragger.MoveHeld(g.last);
                    break;
                case Kind.Piggy:
                    if (g.moved)
                    {
                        // sollevato col dito: sale un po' e si inclina verso dove lo si tira
                        Vector2 d = g.last - g.start;
                        float lift = Mathf.Clamp(Mathf.Abs(d.y) * 0.5f + Mathf.Abs(d.x) * 0.15f, 0f, 0.7f);
                        float tilt = Mathf.Clamp(-d.x * 45f + (d.y > 0f ? 0f : d.y * 20f), -60f, 60f);
                        piggy.localPosition = piggyHome + new Vector3(Mathf.Clamp(d.x * 0.25f, -0.35f, 0.35f), lift + (logic.Flipped ? FlipLift : 0f), 0f);
                        piggy.localRotation = Quaternion.Euler(0f, 0f, (logic.Flipped ? 180f : 0f) + tilt);
                        // sollevandolo o inclinandolo si intravede il tappo sotto la pancia
                        HalloweenKit.Show(plug, logic.Flipped || Mathf.Abs(tilt) > 35f || lift > 0.45f);
                    }
                    break;
                case Kind.Plug:
                    {
                        // il tappo si allunga verso il dito; oltre la soglia si stacca
                        Vector2 d = g.last - g.start;
                        Vector3 give = Vector2.ClampMagnitude(d * 0.45f, 0.15f);
                        plug.position = piggy.TransformPoint(plugLocal) + give;
                        if (g.maxDist >= PiggyBankLogic.PlugDrag) { grip = null; PullPlug(); }
                    }
                    break;
            }
        }

        void End(Grip g)
        {
            grip = null;
            bool tap = Vector2.Distance(g.last, g.start) < TapSlop && Time.time - g.t0 <= TapTime;
            switch (g.kind)
            {
                case Kind.Tool: DropTool(g); break;
                case Kind.Piggy:
                    if (g.maxDist >= PiggyBankLogic.TurnDrag) { StartCoroutine(TurnRun()); break; }
                    PiggyRest();
                    if (!tap) break;
                    if (g.onSlot) TapSlot();
                    else TapPiggy();
                    break;
                case Kind.Plug:
                    plugHeld = false;
                    PiggyRest();
                    if (tap && logic.TapPlug())
                    {
                        V74APiggySound.Play(this, "thup", 0.6f);
                        FxWiggle(plug, 14f, 0.35f, 30f);
                        toast.Show(PlugLines[plugLines++ % PlugLines.Length], 1.7f);
                    }
                    break;
                case Kind.Kid: if (tap) TapKid(); break;
                case Kind.Teddy: if (tap) TapTeddy(); break;
                case Kind.Note: if (tap) ReadNote(); break;
                case Kind.Window: if (tap) TapWindow(); break;
            }
        }

        // ---------- Il salvadanaio ----------

        void TapPiggy()
        {
            if (!logic.TapPiggy()) return;
            V74APiggySound.Play(this, "jingle", 0.5f);
            FxWiggle(piggy, 5f, 0.35f, 28f);
            if (logic.Flipped) toast.Show(FlippedLines[flippedLines++ % FlippedLines.Length], 1.6f);
            else toast.Show(PiggyLines[piggyLines++ % PiggyLines.Length], 1.9f);
        }

        void TapSlot()
        {
            if (!logic.TapSlot()) return;
            Sfx.Play(this, "tick", 0.5f);
            FxPop(piggy, 1.05f, 0.2f);
            toast.Show(SlotLines[slotLines++ % SlotLines.Length], 1.7f);
        }

        /// <summary>Girare il salvadanaio: si solleva, ruota di mezzo giro e si posa (in piedi o a pancia all'aria).</summary>
        IEnumerator TurnRun()
        {
            busy = true;
            CancelGrip();
            PiggyTurn r = logic.Turn();
            if (r == PiggyTurn.None) { busy = false; yield break; }
            StopFxOn(piggy);
            bool flipped = logic.Flipped;
            Vector3 p0 = piggy.localPosition;
            float a0 = piggy.localEulerAngles.z;
            // mezzo giro dalla posa di prima, dalla parte verso cui lo si stava inclinando
            float d = Mathf.DeltaAngle(flipped ? 0f : 180f, a0);
            float delta = (d >= 0f ? 180f : -180f) - d;
            Vector3 p1 = piggyHome + (flipped ? new Vector3(0f, FlipLift, 0f) : Vector3.zero);
            V74APiggySound.Play(this, "jingle", 0.6f);
            HalloweenKit.Show(plug, true);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.45f)
            {
                float e = k * k * (3f - 2f * k);
                piggy.localPosition = Vector3.Lerp(p0, p1, e) + new Vector3(0f, 0.55f * Mathf.Sin(k * Mathf.PI), 0f);
                piggy.localRotation = Quaternion.Euler(0f, 0f, a0 + delta * e);
                yield return null;
            }
            PiggyRest();
            Sfx.Play(this, "clunk", 0.5f);
            Haptics.Light();
            FxShake(piggy, 0.02f, 0.15f);
            switch (r)
            {
                case PiggyTurn.Flipped:
                    CoinOut();
                    toast.Show(logic.Flips == 1 ? "Upside down! One coin fell out." : "Clink. " + PiggyBankLogic.CentsLabel(logic.Cents) + ".", 1.9f);
                    break;
                case PiggyTurn.FlippedNoCoin:
                    toast.Show("Upside down. Nothing falls out anymore.", 1.8f);
                    break;
                default:
                    toast.Show("Back on its feet.", 1.4f);
                    break;
            }
            plugNudgeAt = Time.time + PlugNudgeEvery * 0.6f;
            yield return new WaitForSeconds(0.15f);
            busy = false;
        }

        // ---------- Lo scossone e le monetine ----------

        void Shake()
        {
            PiggyTrickle r = logic.Shake();
            if (r == PiggyTrickle.None) return;
            V74APiggySound.Play(this, "jingle", 0.7f);
            Haptics.Light();
            FxShake(piggy, 0.06f, 0.4f);
            if (logic.Shakes >= 2) V74APiggySound.Play(this, "thup", 0.45f);   // l'indizio: qualcosa sotto suona diverso
            if (r == PiggyTrickle.Coin)
            {
                CoinOut();
                toast.Show(logic.Shakes == 1 ? "Clink. " + PiggyBankLogic.CentsLabel(logic.Cents) + "." : PiggyBankLogic.CentsLabel(logic.Cents) + "... " + ShakeLines[shakeLines++ % ShakeLines.Length], 2.2f);
            }
            else toast.Show(JamLines[jamLines++ % JamLines.Length], 1.9f);
        }

        /// <summary>Una monetina esce dalla fessura e va a posarsi con le altre sulla scrivania.</summary>
        void CoinOut()
        {
            if (coinCo != null) StopCoroutine(coinCo);
            SyncCoins();
            int slot = Mathf.Clamp(logic.Cents - 1, 0, stackCoins.Length - 1);
            // la monetina nuova si vede solo quando arriva
            if (logic.Cents - 1 < stackCoins.Length) HalloweenKit.Show(stackCoins[slot], false);
            coinCo = StartCoroutine(CoinRun(stackCoins[slot]));
        }

        IEnumerator CoinRun(Transform target)
        {
            Vector3 from = slotMouth.position;
            Vector3 to = target.position;
            dropCoin.position = from;
            dropCoin.localRotation = Quaternion.identity;
            HalloweenKit.Show(dropCoin, true);
            V74APiggySound.Play(this, "clink", 0.6f);
            // un saltino fuori dalla fessura (verso l'alto se è in piedi), poi giù sulle altre
            float up = logic.Flipped ? 0.1f : 0.45f;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.5f)
            {
                Vector3 p = Vector3.Lerp(from, to, k);
                p.y += up * Mathf.Sin(k * Mathf.PI);
                dropCoin.position = p;
                dropCoin.localRotation = Quaternion.Euler(0f, 0f, k * 540f);
                yield return null;
            }
            HalloweenKit.Show(dropCoin, false);
            V74APiggySound.Play(this, "clink", 0.4f);
            SyncCoins();
            if (centsLabel != null) FxPop(centsLabel.transform, 1.3f, 0.25f);
            coinCo = null;
        }

        // ---------- Gli attrezzi ----------

        void DropTool(Grip g)
        {
            HwDragger.Item it = dragger.Release();
            if (it == null) return;
            bool isHammer = it == hammerItem;
            if (!g.moved)
            {
                dragger.SendHome(it, 0.15f);
                FxWiggle(it.T, 6f, 0.3f, 25f);
                toast.Show(isHammer ? HammerTapLines[hammerTapLines++ % HammerTapLines.Length] : KnifeTapLines[knifeTapLines++ % KnifeTapLines.Length], 1.6f);
                return;
            }
            bool onPiggy = piggyArea.OverlapPoint(g.last) || piggyArea.OverlapPoint(it.T.position)
                || Vector2.Distance(it.T.position, piggy.position) < 1.2f;
            if (!onPiggy) { dragger.SendHome(it, 0.3f); return; }
            StartCoroutine(isHammer ? HammerRun() : KnifeRun());
        }

        /// <summary>Il martello: il bambino si mette in mezzo, "NO!", il martello rimbalza. Il salvadanaio non si rompe mai.</summary>
        IEnumerator HammerRun()
        {
            busy = true;
            CancelGrip();
            int n = logic.Hammer();
            if (n == 0) { dragger.SendHome(hammerItem, 0.3f); busy = false; yield break; }
            // il bambino accorre a braccia tese
            if (kidArt != null) kidArt.sprite = kidNo;
            V74APiggySound.Play(this, "gasp", 0.6f);
            StartCoroutine(HalloweenKit.Move(kid, kid.parent.TransformPoint(kidHome + new Vector3(0.4f, 0f, 0f)), 0.15f, 0.15f));
            Vector3 top = piggy.position + new Vector3(0.1f, 1.05f, 0f);
            yield return HalloweenKit.Move(hammer, top + new Vector3(0.3f, 0.35f, 0f), 0.15f);
            // giù... e rimbalza sul "NO!"
            HalloweenKit.Show(noBurst, true);
            noBurst.localScale = noScale;
            FxPop(noBurst, 1.35f, 0.25f);
            yield return HalloweenKit.Move(hammer, top, 0.07f);
            Sfx.Play(this, "bonk", 0.7f);
            Haptics.Medium();
            FxShake(piggy, 0.03f, 0.2f);
            toast.Show(HammerLines[hammerLines++ % HammerLines.Length], 1.9f);
            Vector3 h0 = hammer.position;
            Vector3 home = hammerItem.Home;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.55f)
            {
                Vector3 p = Vector3.Lerp(h0, home, k);
                p.y += 1.1f * Mathf.Sin(k * Mathf.PI);
                hammer.position = p;
                hammer.localRotation = hammerRot * Quaternion.Euler(0f, 0f, 720f * k);
                yield return null;
            }
            hammer.position = home;
            hammer.localRotation = hammerRot;
            Sfx.Play(this, "tick", 0.5f);
            yield return new WaitForSeconds(0.35f);
            HalloweenKit.Show(noBurst, false);
            yield return HalloweenKit.Move(kid, kid.parent.TransformPoint(kidHome), 0.2f);
            if (kidArt != null) kidArt.sprite = kidWorried;
            busy = false;
        }

        /// <summary>Il coltello nella fessura: una monetina scivola fuori, poi il coltello si incastra e salta via.</summary>
        IEnumerator KnifeRun()
        {
            busy = true;
            CancelGrip();
            PiggyKnife r = logic.Knife();
            if (r == PiggyKnife.None) { dragger.SendHome(knifeItem, 0.3f); busy = false; yield break; }
            if (r == PiggyKnife.SlotUnder)
            {
                yield return HalloweenKit.Move(knife, piggy.position + new Vector3(1.1f, 0.3f, 0f), 0.15f);
                FxWiggle(knife, 10f, 0.4f, 25f);
                toast.Show("The slot is on the bottom now.", 1.7f);
                yield return new WaitForSeconds(0.4f);
                dragger.SendHome(knifeItem, 0.3f);
                yield return new WaitForSeconds(0.3f);
                busy = false;
                yield break;
            }
            // il coltello in verticale, punta nella fessura
            Vector3 mouth = slotMouth.position;
            knife.localRotation = Quaternion.Euler(0f, 0f, -90f);
            yield return HalloweenKit.Move(knife, mouth + new Vector3(0f, 0.75f, 0f), 0.2f);
            yield return HalloweenKit.Move(knife, mouth + new Vector3(0f, 0.45f, 0f), 0.1f);
            V74APiggySound.Play(this, "scrape", 0.6f);
            for (int i = 0; i < 3; i++)
            {
                knife.localRotation = Quaternion.Euler(0f, 0f, -90f + (i % 2 == 0 ? 8f : -8f));
                yield return new WaitForSeconds(0.08f);
            }
            knife.localRotation = Quaternion.Euler(0f, 0f, -90f);
            if (r == PiggyKnife.Coin)
            {
                CoinOut();
                toast.Show("One coin slid out! ...and the knife is stuck.", 1.6f);
            }
            else toast.Show("No coins. Just a stuck knife.", 1.6f);
            // incastrato: tira, tira...
            for (float k = 0f; k < 0.5f; k += Time.deltaTime)
            {
                knife.position = mouth + new Vector3(Random.Range(-0.02f, 0.02f), 0.45f + Random.Range(0f, 0.04f), 0f);
                yield return null;
            }
            // ...e salta via
            V74APiggySound.Play(this, "boing", 0.6f);
            Vector3 k0 = knife.position;
            Vector3 home = knifeItem.Home;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.5f)
            {
                Vector3 p = Vector3.Lerp(k0, home, k);
                p.y += 0.9f * Mathf.Sin(k * Mathf.PI);
                knife.position = p;
                knife.localRotation = Quaternion.Euler(0f, 0f, -90f + 450f * k);
                yield return null;
            }
            knife.position = home;
            knife.localRotation = knifeRot;
            Sfx.Play(this, "tick", 0.5f);
            if (r == PiggyKnife.Coin) toast.Show("Pop! The knife gives up. " + PiggyBankLogic.CentsLabel(logic.Cents) + ".", 1.8f);
            yield return new WaitForSeconds(0.2f);
            busy = false;
        }

        // ---------- Il resto della stanza ----------

        void TapKid()
        {
            if (!logic.TapKid()) return;
            Sfx.Play(this, "pop", 0.4f);
            FxPop(kid, 1.06f, 0.25f);
            toast.Show(KidLines[kidLines++ % KidLines.Length], 2f);
        }

        void TapTeddy()
        {
            if (!logic.TapTeddy()) return;
            Sfx.Play(this, "pop", 0.35f);
            FxWiggle(teddy, 8f, 0.4f, 24f);
            toast.Show(TeddyLines[teddyLines++ % TeddyLines.Length], 1.5f);
        }

        void ReadNote()
        {
            if (logic.InputLocked) return;
            logic.ReadNote();
            Sfx.Play(this, "tick", 0.5f);
            FxPop(note, 1.15f, 0.25f);
            toast.Show("\"Ice cream truck in 30 seconds!\" Hurry!", 2f);
        }

        void TapWindow()
        {
            if (!logic.TapWindow()) return;
            V74APiggySound.Play(this, "icecream", 0.22f);
            toast.Show(WindowLines[windowLines++ % WindowLines.Length], 1.8f);
        }

        // ---------- La soluzione ----------

        void PullPlug()
        {
            PiggyPlug r = logic.PullPlug();
            if (r == PiggyPlug.Pulled) { StartCoroutine(Pour()); return; }
            if (r == PiggyPlug.Hidden)
            {
                PiggyRest();
                toast.Show("There's nothing to pull from up here.", 1.6f);
            }
        }

        /// <summary>Il finale: il tappo salta via, il bambino solleva il maialino e le monete escono a pioggia.</summary>
        IEnumerator Pour()
        {
            finishing = true;
            busy = false;
            CancelGrip();
            StopFx();
            if (coinCo != null) { StopCoroutine(coinCo); coinCo = null; }
            HalloweenKit.Show(dropCoin, false);
            foreach (HwDragger.Item it in new[] { hammerItem, knifeItem })
            {
                dragger.Stop(it);
                it.T.position = it.Home;
            }
            hammer.localRotation = hammerRot;
            knife.localRotation = knifeRot;
            HalloweenKit.Show(noBurst, false);
            kid.localPosition = kidHome;
            if (kidArt != null) kidArt.sprite = kidWorried;
            // il maialino a pancia all'aria, il tappo che salta via
            piggy.localScale = piggyScale;
            piggy.localPosition = piggyHome + new Vector3(0f, FlipLift, 0f);
            piggy.localRotation = Quaternion.Euler(0f, 0f, 180f);
            HalloweenKit.Show(plugHole, true);
            V74APiggySound.Play(this, "plop", 0.8f);
            Haptics.Medium();
            HalloweenKit.Show(plug, true);
            Vector3 plugFrom = piggy.TransformPoint(plugLocal);
            plug.SetParent(transform, true);
            plug.position = plugFrom;
            plug.localScale = Vector3.one;
            StartCoroutine(PlugFly(plugFrom));
            yield return new WaitForSeconds(0.25f);
            // si solleva e si gira col buco in giù
            Vector3 up = piggyHome + new Vector3(0f, 0.85f, 0f);
            Vector3 p0 = piggy.localPosition;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.4f)
            {
                float e = k * k * (3f - 2f * k);
                piggy.localPosition = Vector3.Lerp(p0, up, e);
                piggy.localRotation = Quaternion.Euler(0f, 0f, 180f + 170f * e);
                yield return null;
            }
            piggy.localRotation = Quaternion.Euler(0f, 0f, 350f);
            // la pioggia di monete
            if (kidArt != null) kidArt.sprite = kidCheer;
            FxPop(kid, 1.1f, 0.3f);
            V74APiggySound.Play(this, "shower", 0.8f);
            HalloweenKit.Show(centsLabel, true);
            for (int i = 0; i < stackCoins.Length; i++) HalloweenKit.Show(stackCoins[i], false);
            HalloweenKit.Show(coinPile, true);
            coinPile.localScale = new Vector3(pileScale.x, pileScale.y * 0.05f, pileScale.z);
            StartCoroutine(TruckArrives());
            int n = showerCoins.Length;
            for (int i = 0; i < n; i++) StartCoroutine(CoinFall(showerCoins[i], i * 0.06f));
            const float showerTime = 1.1f;
            int startCents = Mathf.Min(logic.Cents, PiggyBankLogic.TotalCents);
            for (float k = 0f; k < 1f; k += Time.deltaTime / showerTime)
            {
                coinPile.localScale = new Vector3(pileScale.x * Mathf.Lerp(0.4f, 1f, k), pileScale.y * Mathf.Lerp(0.05f, 1f, k), pileScale.z);
                if (centsLabel != null) centsLabel.text = PiggyBankLogic.CentsLabel(Mathf.RoundToInt(Mathf.Lerp(startCents, PiggyBankLogic.TotalCents, k)));
                yield return null;
            }
            coinPile.localScale = pileScale;
            if (centsLabel != null) { centsLabel.text = PiggyBankLogic.CentsLabel(PiggyBankLogic.TotalCents) + "!"; FxPop(centsLabel.transform, 1.4f, 0.3f); }
            for (int i = 0; i < sparkles.Length; i++) { sparkles[i].localPosition = sparkleHome[i]; HalloweenKit.Show(sparkles[i], true); FxPop(sparkles[i], 1.4f, 0.4f); }
            toast.Show("PIGGY BANKS HAVE A PLUG.", 2f);
            yield return new WaitForSeconds(0.9f);
            // Mr. Oink torna giù, intero
            Vector3 q0 = piggy.localPosition;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.3f)
            {
                piggy.localPosition = Vector3.Lerp(q0, piggyHome, k);
                piggy.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(350f, 360f, k));
                yield return null;
            }
            piggy.localPosition = piggyHome;
            piggy.localRotation = Quaternion.identity;
            Sfx.Play(this, "clunk", 0.4f);
            FxWiggle(piggy, 6f, 0.6f, 20f);
            toast.Show("MR. OINK IS SAFE.", 2.4f);
            yield return Celebrate(1.2f);
            if (logic.TryComplete()) Win();
        }

        IEnumerator PlugFly(Vector3 from)
        {
            Vector3 to = plugRest.position;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.5f)
            {
                Vector3 p = Vector3.Lerp(from, to, k);
                p.y += 0.9f * Mathf.Sin(k * Mathf.PI);
                plug.position = p;
                plug.rotation = Quaternion.Euler(0f, 0f, 180f + 540f * k);
                yield return null;
            }
            plug.position = to;
            plug.rotation = Quaternion.Euler(0f, 0f, 160f);
            Sfx.Play(this, "tick", 0.5f);
        }

        IEnumerator CoinFall(Transform c, float delay)
        {
            HalloweenKit.Show(c, false);
            yield return new WaitForSeconds(delay);
            Vector3 from = piggy.TransformPoint(plugLocal);
            Vector3 to = coinPile.position + new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(0.05f, 0.3f), 0f);
            c.position = from;
            HalloweenKit.Show(c, true);
            float t = Random.Range(0.35f, 0.5f), spin = Random.Range(-720f, 720f);
            float side = Random.Range(-0.5f, 0.5f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / t)
            {
                // cade accelerando, con un piccolo sbuffo di lato
                Vector3 p = new Vector3(Mathf.Lerp(from.x, to.x, k) + side * Mathf.Sin(k * Mathf.PI) * 0.3f, Mathf.Lerp(from.y, to.y, k * k), from.z);
                c.position = p;
                c.localRotation = Quaternion.Euler(0f, 0f, spin * k);
                yield return null;
            }
            c.position = to;
            if (Random.value < 0.4f) V74APiggySound.Play(this, "clink", 0.3f);
        }

        IEnumerator TruckArrives()
        {
            HalloweenKit.Show(truck, true);
            Vector3 from = truckHome;
            Vector3 to = truckHome + new Vector3(-0.2f, 0f, 0f);
            V74APiggySound.Play(this, "icecream", 0.7f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 1.2f)
            {
                float e = 1f - (1f - k) * (1f - k);
                truck.localPosition = Vector3.Lerp(from, to, e) + new Vector3(0f, 0.015f * Mathf.Abs(Mathf.Sin(k * 30f)), 0f);
                yield return null;
            }
            truck.localPosition = to;
        }

#if UNITY_EDITOR
        // ---------- Tasti di prova nell'Editor ----------

        void EditorKeys()
        {
            if (logic.InputLocked) return;
            if (Input.GetKeyDown(KeyCode.F)) { CancelGrip(); StartCoroutine(TurnRun()); return; }
            if (Input.GetKeyDown(KeyCode.P)) { CancelGrip(); PullPlug(); return; }
            if (Input.GetKeyDown(KeyCode.H)) { CancelGrip(); StartCoroutine(HammerRun()); return; }
            if (Input.GetKeyDown(KeyCode.K)) { CancelGrip(); StartCoroutine(KnifeRun()); return; }
            if (Input.GetKeyDown(KeyCode.O)) TapSlot();
            if (Input.GetKeyDown(KeyCode.C)) TapKid();
            if (Input.GetKeyDown(KeyCode.B)) TapTeddy();
            if (Input.GetKeyDown(KeyCode.N)) ReadNote();
            if (Input.GetKeyDown(KeyCode.W)) TapWindow();
        }
#endif
    }

    /// <summary>Suoni sintetizzati del livello 273: monete nel salvadanaio, monetina, "thup" gommoso, tappo, pioggia di monete, furgone dei gelati.</summary>
    public static class V74APiggySound
    {
        const int Rate = 22050;
        static readonly Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>();
        static readonly System.Random rnd = new System.Random(273);

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

        /// <summary>Un tintinnio di moneta (parziali metallici che si spengono).</summary>
        static float Ting(float u, float f)
        {
            if (u <= 0f) return 0f;
            return (Mathf.Sin(u * 2f * Mathf.PI * f) + 0.5f * Mathf.Sin(u * 2f * Mathf.PI * f * 2.76f)) * Mathf.Exp(-u * 22f);
        }

        static AudioClip Clip(string kind)
        {
            if (cache.TryGetValue(kind, out AudioClip c) && c != null) return c;
            switch (kind)
            {
                case "clink":    // una monetina sola
                    c = Make(kind, 0.25f, t => Ting(t, 3400f) * 0.35f);
                    break;
                case "jingle":   // le monete che sbatacchiano dentro la ceramica
                    {
                        var hits = new float[9];
                        var freq = new float[9];
                        for (int i = 0; i < hits.Length; i++) { hits[i] = (float)rnd.NextDouble() * 0.35f; freq[i] = 2600f + (float)rnd.NextDouble() * 1800f; }
                        c = Make(kind, 0.5f, t =>
                        {
                            float v = 0f;
                            for (int i = 0; i < hits.Length; i++) v += Ting(t - hits[i], freq[i]);
                            return v * 0.12f + N() * 0.04f * Mathf.Exp(-t * 8f);
                        });
                    }
                    break;
                case "shower":   // la pioggia di monete del finale
                    {
                        var hits = new float[40];
                        var freq = new float[40];
                        for (int i = 0; i < hits.Length; i++) { hits[i] = (float)rnd.NextDouble() * 1.2f; freq[i] = 2300f + (float)rnd.NextDouble() * 2200f; }
                        c = Make(kind, 1.5f, t =>
                        {
                            float v = 0f;
                            for (int i = 0; i < hits.Length; i++) v += Ting(t - hits[i], freq[i]);
                            return v * 0.09f;
                        });
                    }
                    break;
                case "thup":     // il tappo di gomma: un tonfo sordo e molle
                    c = Make(kind, 0.15f, t => (Mathf.Sin(t * 2f * Mathf.PI * Mathf.Lerp(160f, 90f, t / 0.15f)) * 0.7f + N() * 0.1f) * Mathf.Exp(-t * 30f));
                    break;
                case "plop":     // il tappo che salta via
                    c = Make(kind, 0.2f, t => Mathf.Sin(t * 2f * Mathf.PI * Mathf.Lerp(250f, 900f, t / 0.2f)) * 0.6f * Mathf.Exp(-t * 18f));
                    break;
                case "boing":    // il coltello che salta via
                    c = Make(kind, 0.5f, t => Mathf.Sin(t * 2f * Mathf.PI * (300f + 120f * Mathf.Sin(t * 40f))) * 0.4f * Mathf.Exp(-t * 5f));
                    break;
                case "scrape":   // il coltello nella fessura
                    c = Make(kind, 0.3f, t => N() * 0.25f * (0.5f + 0.5f * Mathf.Sin(t * 2f * Mathf.PI * 60f)) * Mathf.Sin(t / 0.3f * Mathf.PI));
                    break;
                case "gasp":     // il bambino che trattiene il fiato
                    {
                        float lp = 0f;
                        c = Make(kind, 0.3f, t => { lp += (N() - lp) * 0.25f; return lp * 0.7f * Mathf.Sin(t / 0.3f * Mathf.PI) * (1f + 0.3f * Mathf.Sin(t * 2f * Mathf.PI * 520f)); });
                    }
                    break;
                case "icecream": // il motivetto del furgone dei gelati (carillon)
                    {
                        float[] notes = { 659f, 784f, 880f, 784f, 659f, 523f, 587f, 659f, 784f, 659f };
                        const float step = 0.2f;
                        c = Make(kind, notes.Length * step + 0.4f, t =>
                        {
                            float v = 0f;
                            for (int i = 0; i < notes.Length; i++)
                            {
                                float u = t - i * step;
                                if (u <= 0f || u > 0.6f) continue;
                                v += (Mathf.Sin(u * 2f * Mathf.PI * notes[i]) + 0.3f * Mathf.Sin(u * 2f * Mathf.PI * notes[i] * 2f)) * Mathf.Exp(-u * 7f);
                            }
                            return v * 0.3f;
                        });
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
            AudioClip clip = AudioClip.Create("v74a_" + name, n, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
