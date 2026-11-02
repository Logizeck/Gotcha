using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels.V64;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 5 della roadmap "The Cowardly Lion" — "SCARE THE LION." Nella savana un enorme leone minaccioso, con una
    /// criniera esagerata e i denti aguzzi, e sotto un grosso bottone ROAR.
    /// Inganno: toccato (corpo o muso), il leone ruggisce "ROAAAR!" e si gonfia un po' di più ogni volta (con un tetto; se
    /// lo si lascia in pace si sgonfia da solo). Il bottone ROAR lo fa rispondere ancora più forte ("ROOOOAAAAR!!" / "Is
    /// that all you've got?"). Niente di tutto questo porta da qualche parte, e niente si perde.
    /// Indizi: la cerniera sul petto, l'elastico sotto il mento, l'etichetta "COSTUME" appesa alla criniera; un tocco
    /// leggero sulla criniera la fa ondeggiare ("...is that mane ATTACHED?").
    /// Soluzione: la CRINIERA (col muso, è una maschera) è un oggetto a sé: trascinata scopre man mano, da dietro, la
    /// testolina del gattino che sta nel costume (prima, coperto da criniera e muso, del gattino non si vede niente).
    /// Lasciata vicino (meno di CowardlyLionLogic.RevealDistance = 1.6 unità da casa) torna su da sola; lasciata più
    /// lontano resta via: il gattino fa "mew?", si spaventa del proprio miagolio, salta fuori dal costume e scappa; dall'alto
    /// cade una coppetta -> LevelReward "KING OF THE JUNGLE DEFEATED!" -> Win (una volta sola).
    /// Conta la distanza AL RILASCIO (oltre la soglia durante il trascinamento non basta). Dito perso (pausa, app in
    /// background, secondo dito): la criniera torna su. La logica è in Gotcha.Levels.V64.CowardlyLionLogic.
    /// Tasti di prova nell'Editor: T tocco sul leone, B bottone ROAR, M tocco leggero sulla criniera, N criniera tirata poco
    /// (torna su), D criniera tirata via (soluzione).
    /// </summary>
    public class V76CowardlyLionLevel : LevelController
    {
        [Header("Leone (costume)")]
        [SerializeField] Transform rig;                 // tutto il leone: si gonfia quando ruggisce (scala dai piedi, moltiplica quella di riposo)
        [SerializeField] Transform body;                // il costume seduto
        [SerializeField] Collider2D bodyArea;
        [SerializeField] Transform kittenHead;          // la testolina nascosta dietro la criniera
        [Header("Criniera (trascinabile, col muso)")]
        [SerializeField] Transform mane;
        [SerializeField] Collider2D maneArea;
        [SerializeField] Collider2D faceArea;           // il muso: un tocco qui è un tocco sul leone
        [SerializeField] Transform face;
        [SerializeField] Transform faceRoar;
        [SerializeField] Transform maneTag;             // l'etichetta "COSTUME"
        [SerializeField] SpriteRenderer roarWaves;
        [Header("Bottone, gattino, coppa")]
        [SerializeField] Transform button;
        [SerializeField] Collider2D buttonArea;
        [SerializeField] Transform kittenScared;
        [SerializeField] Transform kittenRun;
        [SerializeField] Transform trophy;
        [SerializeField] Transform trophyLanding;
        [SerializeField] LevelReward reward;
        [SerializeField] TextMesh infoText;
        [SerializeField] Transform infoHolder;

        const string Compliment = "KING OF THE JUNGLE DEFEATED!";
        const float TapSlop = 0.2f, RoarShow = 0.55f, GrowPerLevel = 0.035f;   // al ruggito massimo la criniera non tocca la fascia dei messaggi
        const float ManeMinX = -2.25f, ManeMaxX = 2.25f, ManeMinY = -4.1f, ManeMaxY = 2.55f;

        static readonly string[] CappedLines = { "ROAAAAAAR! ...it can't get any bigger.", "Maximum scary. Still just roaring.", "ROAAAAAAR! (same size. it tried.)" };
        static readonly string[] RoarsBackLines = { "ROOOOAAAAR!!", "ROOOOOOAAAAAAR!!!", "MEGA ROOOAAAR!!" };
        static readonly string[] TauntLines = { "Is that all you've got?", "Cute. Try again.", "Was that a sneeze?" };
        static readonly string[] WobbleLines = { "...is that mane ATTACHED?", "It wobbles. Real manes don't wobble.", "There's a tag on it: \"COSTUME\"." };
        static readonly string[] SnapLines = { "Boing! It snapped back on.", "It came off a bit... and sprang back.", "Elastic. Interesting." };

        enum Kind { None, Mane, Body, Button }

        sealed class Grip
        {
            public Kind kind;
            public Vector2 start, last;
            public Vector3 offset;
            public bool moved, onFace;
        }

        sealed class FxSlot
        {
            public Coroutine co;
            public Vector3 v;
            public Quaternion q;
        }

        CowardlyLionLogic logic;
        Camera cam;
        HwToast toast;
        Grip grip;
        readonly Dictionary<Transform, FxSlot>[] fxSlots = { new Dictionary<Transform, FxSlot>(), new Dictionary<Transform, FxSlot>(), new Dictionary<Transform, FxSlot>() };
        Coroutine maneCo;
        Vector3 rigBase, maneHome, kittenHeadHome, trophyStart, buttonScale, waveScale;
        float roarUntil, roarSize, rigScale = 1f;
        int cappedLines, roarsBackLines, tauntLines, wobbleLines, snapLines;
        bool captured, finishing;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            StopAllCoroutines();
            StopFx();
            maneCo = null;
            if (!captured)
            {
                captured = true;
                logic = new CowardlyLionLogic();
                rigBase = rig.localScale;
                maneHome = mane.localPosition;
                kittenHeadHome = kittenHead.localPosition;
                trophyStart = trophy.position;
                buttonScale = button.localScale;
                waveScale = roarWaves != null ? roarWaves.transform.localScale : Vector3.one;
            }
            logic.Reset();
            if (reward != null) reward.ResetReward();
            grip = null;
            finishing = false;
            cappedLines = roarsBackLines = tauntLines = wobbleLines = snapLines = 0;
            toast = new HwToast(infoText, infoHolder);
            RestPose();
        }

        // ---------- Pose (sempre ricavate dalla logica: uno stato valido dopo pausa/spegnimento) ----------

        void RestPose()
        {
            if (maneCo != null) { StopCoroutine(maneCo); maneCo = null; }
            roarUntil = 0f;
            rigScale = 1f + GrowPerLevel * logic.RoarLevel;
            rig.localScale = rigBase * rigScale;
            body.localScale = Vector3.one;
            body.localRotation = Quaternion.identity;
            HalloweenKit.Show(mane, true);
            mane.localPosition = maneHome;
            mane.localRotation = Quaternion.identity;
            mane.localScale = Vector3.one;
            if (maneTag != null) maneTag.localRotation = Quaternion.identity;
            ShowRoarFace(false);
            WavesPose(0f);
            HalloweenKit.Show(kittenHead, true);
            kittenHead.localPosition = kittenHeadHome;
            kittenHead.localScale = Vector3.one;
            HalloweenKit.Show(kittenScared, false);
            HalloweenKit.Show(kittenRun, false);
            HalloweenKit.Show(trophy, false);
            trophy.position = trophyStart;
            trophy.localRotation = Quaternion.identity;
            button.localScale = buttonScale;
        }

        void ShowRoarFace(bool on)
        {
            HalloweenKit.Show(face, !on);
            HalloweenKit.Show(faceRoar, on);
        }

        void WavesPose(float k)
        {
            if (roarWaves == null) return;
            HalloweenKit.SetAlpha(roarWaves, 0.9f * k);
            roarWaves.transform.localScale = waveScale * (roarSize * (1.25f - 0.25f * k));
        }

        // ---------- Ciclo di vita ----------

        void OnDisable()
        {
            // pausa/pubblicità/livello spento: si molla tutto, si torna a uno stato valido
            CancelGrip();
            StopFx();
            maneCo = null;
            if (captured && !finishing && !IsFinished && logic != null) RestPose();
        }

        void OnEnable()
        {
            if (!captured || IsFinished || logic == null) return;
            // spento a metà della scenetta finale: la si rifà da capo (la vittoria resta una sola)
            if (logic.Stage == CowardlyLionStage.Revealing) { finishing = false; StartCoroutine(Finale()); }
        }

        void OnApplicationPause(bool paused) { if (paused) CancelGrip(); }
        void OnApplicationFocus(bool focus) { if (!focus) CancelGrip(); }

        /// <summary>Il dito sparisce: la criniera tenuta torna subito al suo posto (mai una vittoria), il bottone si rialza.</summary>
        void CancelGrip()
        {
            Grip g = grip;
            grip = null;
            if (button != null) button.localScale = buttonScale;
            if (g == null || logic == null) return;
            if (g.kind == Kind.Mane && logic.CancelMane() && mane != null)
            {
                mane.localPosition = maneHome;
                mane.localRotation = Quaternion.identity;
            }
        }

        void Update()
        {
            toast?.Tick();
            if (IsFinished || toast == null || finishing) return;
            if (Time.timeScale <= 0f) { CancelGrip(); return; }   // pausa o pubblicità: niente input
            logic.Tick(Time.deltaTime);
            Ambient();
#if UNITY_EDITOR
            EditorKeys();
            if (finishing) return;
#endif
            ReadPointer();
        }

        /// <summary>Il leone respira e si gonfia/sgonfia col livello del ruggito; il muso ruggisce per un attimo; l'etichetta dondola.</summary>
        void Ambient()
        {
            bool holding = grip != null && grip.kind == Kind.Mane;
            float target = 1f + GrowPerLevel * logic.RoarLevel;
            // mentre si tira la criniera il leone resta com'è (la "casa" della criniera non si sposta sotto il dito)
            if (!holding) rigScale = Mathf.MoveTowards(rigScale, target, Time.deltaTime * (target > rigScale ? 1.2f : 0.15f));
            float breathe = holding ? 0f : 0.012f * Mathf.Sin(Time.time * 2.2f);
            rig.localScale = new Vector3(rigBase.x * rigScale, rigBase.y * rigScale * (1f + breathe), rigBase.z);
            bool roaring = Time.time < roarUntil && logic.Mane == CowardlyLionMane.On;
            ShowRoarFace(roaring);
            WavesPose(roaring ? Mathf.Clamp01((roarUntil - Time.time) / RoarShow) : 0f);
            if (maneTag != null && !holding) maneTag.localRotation = Quaternion.Euler(0f, 0f, 6f * Mathf.Sin(Time.time * 1.6f));
        }

        // ---------- Effetti che non si accavallano (come nei livelli 0.73-0.75) ----------

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
            if (Input.touchCount >= 2) { CancelGrip(); return; }   // due dita: il gesto si annulla (la criniera torna su)
            if (!HalloweenKit.ReadPointer(cam, out Vector2 w, out bool began, out bool ended))
            {
                if (grip != null) End(grip);
                return;
            }
            if (began) { if (grip != null) End(grip); grip = Begin(w); }
            if (grip == null) return;
            grip.last = w;
            if (Vector2.Distance(w, grip.start) > TapSlop) grip.moved = true;
            if (grip.kind == Kind.Mane && grip.moved) MoveMane(grip, w);
            if (ended && grip != null) End(grip);
        }

        Grip Begin(Vector2 w)
        {
            var g = new Grip { start = w, last = w, kind = Kind.None };
            if (finishing || logic.InputLocked) return g;
            if (buttonArea.OverlapPoint(w))
            {
                g.kind = Kind.Button;
                button.localScale = new Vector3(buttonScale.x * 1.04f, buttonScale.y * 0.88f, buttonScale.z);
                V76CowardlyLionSound.Play(this, "click", 0.5f);
                return g;
            }
            if (maneArea.OverlapPoint(w) && logic.GrabMane())
            {
                if (maneCo != null) { StopCoroutine(maneCo); maneCo = null; }
                StopFxOn(mane);
                g.kind = Kind.Mane;
                g.onFace = faceArea != null && faceArea.OverlapPoint(w);
                g.offset = mane.position - (Vector3)w;
                roarUntil = 0f;
                Haptics.Tap();
                return g;
            }
            if (bodyArea.OverlapPoint(w)) g.kind = Kind.Body;
            return g;
        }

        Vector3 ManeHomeWorld() => rig.TransformPoint(maneHome);

        /// <summary>La criniera segue il dito (dentro l'area sicura): scopre man mano il gattino che c'è dietro.</summary>
        void MoveMane(Grip g, Vector2 w)
        {
            Vector3 p = (Vector3)w + g.offset;
            p.x = Mathf.Clamp(p.x, ManeMinX, ManeMaxX);
            p.y = Mathf.Clamp(p.y, ManeMinY, ManeMaxY);
            p.z = mane.position.z;
            mane.position = p;
            mane.localRotation = Quaternion.Euler(0f, 0f, Mathf.Clamp(w.x - g.start.x, -1f, 1f) * -10f);
            logic.DragMane(Vector2.Distance(p, ManeHomeWorld()));
        }

        void End(Grip g)
        {
            grip = null;
            switch (g.kind)
            {
                case Kind.Button:
                    button.localScale = buttonScale;
                    if (!g.moved || buttonArea.OverlapPoint(g.last)) PressButton();
                    break;
                case Kind.Body:
                    TapLion();
                    break;
                case Kind.Mane:
                    if (!g.moved && g.onFace)
                    {
                        // un tocco sul muso è un tocco sul leone, non sulla criniera
                        logic.CancelMane();
                        mane.localPosition = maneHome;
                        mane.localRotation = Quaternion.identity;
                        TapLion();
                        break;
                    }
                    float d = g.moved ? Vector2.Distance(mane.position, ManeHomeWorld()) : 0f;
                    ReleaseMane(d);
                    break;
            }
        }

        // ---------- Le false piste ----------

        void TapLion()
        {
            CowardlyLionRoar r = logic.TapLion();
            if (r == CowardlyLionRoar.None) return;
            Roar(0.9f + 0.1f * logic.RoarLevel, 0.45f + 0.1f * logic.RoarLevel);
            if (r == CowardlyLionRoar.Capped) toast.Show(CappedLines[cappedLines++ % CappedLines.Length], 1.8f);
            else toast.Show("RO" + new string('A', 1 + logic.RoarLevel) + "R!", 1.4f);
        }

        void PressButton()
        {
            CowardlyLionButton r = logic.PressRoar();
            if (r == CowardlyLionButton.None) return;
            FxShake(button, 0.03f, 0.2f);
            Roar(1.5f, 0.95f);
            if (r == CowardlyLionButton.RoarsBack) toast.Show(RoarsBackLines[roarsBackLines++ % RoarsBackLines.Length], 1.6f);
            else toast.Show(TauntLines[tauntLines++ % TauntLines.Length], 1.8f);
        }

        /// <summary>Il ruggito: muso spalancato, onde del suono, il leone che trema (reversibile: poi si calma).</summary>
        void Roar(float size, float volume)
        {
            roarSize = size;
            roarUntil = Time.time + RoarShow;
            V76CowardlyLionSound.Play(this, "roar", Mathf.Clamp01(volume));
            FxShake(body, 0.02f + 0.01f * logic.RoarLevel, 0.35f);
            FxPop(faceRoar, 1.12f, 0.3f);
            Haptics.Medium();
        }

        void ReleaseMane(float distance)
        {
            switch (logic.ReleaseMane(distance))
            {
                case CowardlyLionRelease.Wobble:
                    mane.localPosition = maneHome;
                    mane.localRotation = Quaternion.identity;
                    FxWiggle(mane, 7f, 0.5f, 26f);
                    if (maneTag != null) FxWiggle(maneTag, 25f, 0.6f, 18f);
                    V76CowardlyLionSound.Play(this, "boing", 0.35f);
                    toast.Show(WobbleLines[wobbleLines++ % WobbleLines.Length], 2.0f);
                    break;
                case CowardlyLionRelease.SnapBack:
                    maneCo = StartCoroutine(SnapBack());
                    break;
                case CowardlyLionRelease.Off:
                    StartCoroutine(Finale());
                    break;
            }
        }

        /// <summary>La criniera tirata poco torna su da sola, con un "boing" d'elastico.</summary>
        IEnumerator SnapBack()
        {
            Vector3 from = mane.localPosition;
            Quaternion q0 = mane.localRotation;
            float t = 0f, dur = 0.16f;
            while (t < dur)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / dur);
                mane.localPosition = Vector3.LerpUnclamped(from, maneHome, k * k);
                mane.localRotation = Quaternion.Slerp(q0, Quaternion.identity, k);
                yield return null;
            }
            mane.localPosition = maneHome;
            mane.localRotation = Quaternion.identity;
            maneCo = null;
            V76CowardlyLionSound.Play(this, "boing", 0.6f);
            Haptics.Light();
            FxWiggle(mane, 5f, 0.35f, 30f);
            if (maneTag != null) FxWiggle(maneTag, 20f, 0.5f, 20f);
            toast.Show(SnapLines[snapLines++ % SnapLines.Length], 1.8f);
        }

        // ---------- La soluzione ----------

        /// <summary>
        /// La criniera resta via: il gattino fa "mew?", si spaventa del proprio miagolio, salta fuori dal costume e scappa;
        /// cade la coppetta; poi la festa comune e Win, una volta sola. Rifatta da capo se il livello si spegne a metà.
        /// </summary>
        IEnumerator Finale()
        {
            finishing = true;
            CancelGrip();
            StopFx();
            if (maneCo != null) { StopCoroutine(maneCo); maneCo = null; }
            roarUntil = 0f;
            ShowRoarFace(false);
            WavesPose(0f);
            rigScale = 1f;
            rig.localScale = rigBase;
            body.localScale = Vector3.one;
            body.localRotation = Quaternion.identity;
            HalloweenKit.Show(kittenHead, true);
            kittenHead.localPosition = kittenHeadHome;
            kittenHead.localScale = Vector3.one;
            HalloweenKit.Show(kittenScared, false);
            HalloweenKit.Show(kittenRun, false);
            HalloweenKit.Show(trophy, false);
            trophy.position = trophyStart;
            // la criniera (vuota) cade a terra lì dov'è
            Vector3 home = ManeHomeWorld();
            if (Vector2.Distance(mane.position, home) < CowardlyLionLogic.RevealDistance)
                mane.position = home + new Vector3(home.x > 0f ? -1.75f : 1.75f, -0.3f, 0f);   // ripresa dopo uno spegnimento
            // a terra di lato e un po' più piccola, così non copre il pulsante ROAR
            float side = mane.position.x >= 0f ? 1f : -1f;
            Vector3 ground = new Vector3(Mathf.Clamp(side * 1.6f, ManeMinX, ManeMaxX), -2.35f, mane.position.z);
            StartCoroutine(HalloweenKit.ScaleTo(mane, mane.localScale * 0.75f, 0.25f));
            yield return HalloweenKit.Move(mane, ground, 0.25f);
            mane.localRotation = Quaternion.Euler(0f, 0f, mane.position.x > 0f ? -18f : 18f);
            V76CowardlyLionSound.Play(this, "thud", 0.5f);
            // il gattino, scoperto
            FxPop(kittenHead, 1.2f, 0.25f);
            V76CowardlyLionSound.Play(this, "mew", 0.7f);
            toast.Show("mew?", 1.2f);
            yield return new WaitForSeconds(0.45f);
            // ...e si spaventa da solo: salta fuori dal costume, che si affloscia
            StopFxOn(kittenHead);
            HalloweenKit.Show(kittenHead, false);
            HalloweenKit.Show(kittenScared, true);
            Vector3 jumpFrom = kittenHead.position + new Vector3(0f, -0.3f, 0f);
            kittenScared.position = jumpFrom;
            V76CowardlyLionSound.Play(this, "yowl", 0.7f);
            Haptics.Light();
            toast.Show("It scared itself.", 1.4f);
            body.localScale = new Vector3(1.05f, 0.82f, 1f);
            body.localRotation = Quaternion.Euler(0f, 0f, 4f);
            Vector3 landing = new Vector3(jumpFrom.x + 0.9f, rig.position.y, jumpFrom.z);
            yield return HalloweenKit.Move(kittenScared, landing, 0.4f, 0.7f);
            // via di corsa, a destra
            HalloweenKit.Show(kittenScared, false);
            HalloweenKit.Show(kittenRun, true);
            kittenRun.position = new Vector3(landing.x, landing.y, kittenRun.position.z);
            V76CowardlyLionSound.Play(this, "patter", 0.5f);
            yield return HalloweenKit.Move(kittenRun, new Vector3(4.3f, landing.y, kittenRun.position.z), 0.45f);
            HalloweenKit.Show(kittenRun, false);
            // la coppetta cade dall'alto dove sedeva il "re", mentre parte la festa
            StartCoroutine(TrophyDrop());
            if (reward != null) yield return reward.Play(Compliment);
            else yield return new WaitForSeconds(1.5f);
            if (logic.TryComplete()) Win(Compliment);
        }

        IEnumerator TrophyDrop()
        {
            HalloweenKit.Show(trophy, true);
            trophy.position = trophyStart;
            Vector3 to = trophyLanding != null ? trophyLanding.position : new Vector3(trophyStart.x, rig.position.y, trophyStart.z);
            float t = 0f, dur = 0.45f;
            while (t < dur)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / dur);
                trophy.position = Vector3.LerpUnclamped(trophyStart, to, k * k);
                yield return null;
            }
            trophy.position = to;
            V76CowardlyLionSound.Play(this, "clink", 0.6f);
            yield return HalloweenKit.Move(trophy, to, 0.25f, 0.2f);   // piccolo rimbalzo
            FxPop(trophy, 1.15f, 0.25f);
        }

#if UNITY_EDITOR
        // ---------- Tasti di prova nell'Editor ----------

        void EditorKeys()
        {
            if (logic.InputLocked || grip != null) return;
            if (Input.GetKeyDown(KeyCode.T)) TapLion();
            if (Input.GetKeyDown(KeyCode.B)) PressButton();
            if (Input.GetKeyDown(KeyCode.M) && logic.GrabMane()) ReleaseMane(0f);
            if (Input.GetKeyDown(KeyCode.N) && logic.GrabMane())
            {
                mane.position = ManeHomeWorld() + new Vector3(0.8f, -0.3f, 0f);
                ReleaseMane(Vector2.Distance(mane.position, ManeHomeWorld()));
            }
            if (Input.GetKeyDown(KeyCode.D) && logic.GrabMane())
            {
                mane.position = ManeHomeWorld() + new Vector3(1.9f, -0.9f, 0f);
                ReleaseMane(Vector2.Distance(mane.position, ManeHomeWorld()));
            }
        }
#endif
    }

    /// <summary>Suoni sintetizzati del leone fifone: ruggito, miagolio, strillo, elastico, tonfo, passetti, clic, tintinnio.</summary>
    public static class V76CowardlyLionSound
    {
        const int Rate = 22050;
        static readonly Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>();
        static readonly System.Random rnd = new System.Random(282);

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
                case "roar":     // ringhio basso e ruvido che sale e scende
                    {
                        float lp = 0f;
                        c = Make(kind, 0.9f, t =>
                        {
                            float env = Mathf.Min(1f, t * 8f) * Mathf.Clamp01((0.9f - t) * 3f);
                            float f = Mathf.Lerp(95f, 140f, Mathf.Sin(t / 0.9f * Mathf.PI));
                            float growl = Mathf.Sin(t * 2f * Mathf.PI * f + 3f * Mathf.Sin(t * 2f * Mathf.PI * 31f));
                            lp += (N() - lp) * 0.08f;
                            return (growl * 0.55f + lp * 1.6f) * env * (0.75f + 0.25f * Mathf.Sin(t * 2f * Mathf.PI * 23f));
                        });
                        break;
                    }
                case "mew":      // miagolio minuscolo: "mew?" (sale alla fine)
                    c = Make(kind, 0.35f, t => Mathf.Sin(t * 2f * Mathf.PI * (t < 0.2f ? Mathf.Lerp(700f, 900f, t / 0.2f) : Mathf.Lerp(900f, 1250f, (t - 0.2f) / 0.15f))) * 0.4f * Mathf.Sin(t / 0.35f * Mathf.PI));
                    break;
                case "yowl":     // lo strillo del gattino spaventato
                    c = Make(kind, 0.4f, t => (Mathf.Sin(t * 2f * Mathf.PI * Mathf.Lerp(1300f, 1700f, t / 0.4f) * (1f + 0.05f * Mathf.Sin(t * 2f * Mathf.PI * 25f))) * 0.35f + N() * 0.04f) * Mathf.Min(1f, t * 40f) * Mathf.Exp(-t * 3f));
                    break;
                case "boing":    // l'elastico della criniera
                    c = Make(kind, 0.4f, t => Mathf.Sin(t * 2f * Mathf.PI * Mathf.Lerp(220f, 130f, t / 0.4f) * (1f + 0.1f * Mathf.Sin(t * 2f * Mathf.PI * 16f))) * 0.45f * Mathf.Exp(-t * 6f));
                    break;
                case "thud":     // la criniera vuota che cade a terra
                    c = Make(kind, 0.25f, t => Mathf.Sin(t * 2f * Mathf.PI * Mathf.Lerp(110f, 55f, t / 0.25f)) * 0.6f * Mathf.Exp(-t * 16f) + N() * 0.25f * Mathf.Exp(-t * 40f));
                    break;
                case "patter":   // passetti veloci
                    c = Make(kind, 0.5f, t => { float u = t % 0.06f; return N() * 0.5f * Mathf.Exp(-u * 160f); });
                    break;
                case "click":    // il bottone che si abbassa
                    c = Make(kind, 0.05f, t => N() * 0.5f * Mathf.Exp(-t * 120f) + Mathf.Sin(t * 2f * Mathf.PI * 900f) * 0.2f * Mathf.Exp(-t * 80f));
                    break;
                case "clink":    // la coppetta che tocca terra
                    c = Make(kind, 0.5f, t => (Mathf.Sin(t * 2f * Mathf.PI * 2400f) + Mathf.Sin(t * 2f * Mathf.PI * 3600f) * 0.5f) * 0.3f * Mathf.Exp(-t * 8f));
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
            AudioClip clip = AudioClip.Create("v76b_" + name, n, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
