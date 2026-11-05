using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels.V64;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 19 della roadmap "The Shy Monster" (id shy_monster) — "DEFEAT THE MONSTER." Nella tana un mostro verde
    /// enorme (corna, tanti denti, ma buffo) riempie la parte alta; sul pavimento una coccarda abbandonata "BEST MONSTER",
    /// una spadina e una fionda.
    /// Inganno: toccare o colpire il mostro lo fa CRESCERE di un gradino (ringhio, sopracciglia più scure e aggrottate,
    /// simbolo della rabbia; al massimo 4 gradini, resta nello schermo) e dopo qualche secondo di calma si sgonfia piano
    /// fino al normale. Spadina e fionda lasciate sul mostro lo fanno solo ridere ("Ha! Tickles.") e crescere; tornano sul
    /// pavimento. Tutto reversibile, mai sconfitta.
    /// Soluzione: trascinare la coccarda sul PETTO del mostro e lasciarla lì (conta il rilascio nella zona del petto,
    /// calcolata dalla scala attuale del mostro). Lasciata su testa/braccia: "It didn't notice." e torna giù; lasciata
    /// altrove torna sul pavimento. Sul petto: si appunta, il mostro la legge ("BEST MONSTER? ME?"), arrossisce (guance
    /// rosse che si allargano, tutto rosa), si rimpicciolisce e scappa a destra coprendosi gli occhi; LevelReward
    /// "YOU DEFEATED EVIL WITH KINDNESS!" -> Win una volta sola.
    /// Un dito alla volta (HalloweenKit.ReadPointer); due dita, pausa, app in background o livello spento annullano la
    /// presa (quello che si teneva torna sul pavimento). La logica è in Gotcha.Levels.V64.ShyMonsterLogic.
    /// Tasti di prova nell'Editor: T colpo al mostro, M coccarda sul petto (soluzione), H coccarda sulla testa (non se ne
    /// accorge), F coccarda lasciata sul pavimento, O coccarda passata sul petto ma lasciata altrove, S spadina sul mostro,
    /// L fionda sul mostro, D spadina lasciata sul pavimento, P pausa simulata (presa annullata), R ricomincia.
    /// </summary>
    public class V79ShyMonsterLevel : LevelController
    {
        [Header("Mostro (origine ai piedi; collider per i colpi e per 'sopra il mostro')")]
        [SerializeField] Transform monster;
        [SerializeField] BoxCollider2D monsterArea;
        [SerializeField] SpriteRenderer monsterBody;
        [SerializeField] Transform armLeft;            // perni sulle spalle (il sinistro è specchiato)
        [SerializeField] Transform armRight;
        [SerializeField] Transform browLeft;           // sopracciglia (ruotano: arrabbiate / preoccupate)
        [SerializeField] Transform browRight;
        [SerializeField] SpriteRenderer blush;         // guance rosse (trasparenti all'inizio)
        [SerializeField] Transform shyArms;            // braccia che coprono gli occhi (nascoste all'inizio)
        [SerializeField] Transform anger;              // simbolo della rabbia (nascosto all'inizio)
        [SerializeField] float pinY = 1.85f;           // punto del petto dove si appunta la coccarda (locale, scala 1)
        [SerializeField] float exitX = 3.8f;
        [Header("Oggetti sul pavimento")]
        [SerializeField] Transform medal;
        [SerializeField] Collider2D medalArea;
        [SerializeField] TextMesh medalText;
        [SerializeField] Transform sword;
        [SerializeField] Collider2D swordArea;
        [SerializeField] Transform slingshot;
        [SerializeField] Collider2D slingshotArea;
        [Header("Fumetto, scritte e festa")]
        [SerializeField] SpeechBubble monsterBubble;
        [SerializeField] TextMesh infoText;
        [SerializeField] Transform infoHolder;
        [SerializeField] LevelReward reward;

        const string Compliment = "YOU DEFEATED EVIL WITH KINDNESS!";
        const float TapSlop = 0.15f, Follow = 14f, GrowFollow = 7f, Lift = 1.12f, PinnedScale = 0.8f;
        const int HeldOrderBoost = 40;
        const float BrowAngry = 14f, BrowAngryStep = 4f, BrowWorried = -16f;
        static readonly Color BodyCalm = Color.white, BodyAngry = new Color(0.8f, 0.86f, 0.78f), BodyBlush = new Color(1f, 0.84f, 0.84f);
        static readonly Color BrowCalm = Color.white, BrowDark = new Color(0.3f, 0.3f, 0.3f);

        static readonly string[] GrowLines = { "GRRR!", "ROOOAR!", "Is that all?", "I'M INVINCIBLE!" };
        static readonly string[] MaxLines = { "MAXIMUM MONSTER!", "Can't get any scarier!", "ROAR! Still huge." };
        static readonly string[] TickleLines = { "Ha! Tickles.", "Hee hee! Stop it!", "A toothpick? Cute." };
        static readonly string[] IgnoredLines = { "It didn't notice.", "Hm. It didn't notice.", "It didn't notice. Not there." };
        static readonly string[] MedalFloorLines = { "The medal falls back down.", "Back on the floor." };
        static readonly string[] DecoyFloorLines = { "Clatter.", "Back on the floor." };
        static readonly string[] MedalTapLines = { "A medal: BEST MONSTER.", "Someone dropped a prize.", "BEST MONSTER. Who's that for?" };
        static readonly string[] SwordTapLines = { "A tiny sword. Very tiny.", "Sharp-ish." };
        static readonly string[] SlingTapLines = { "A slingshot. Pew.", "It fires... pebbles." };
        const string ReadLine = "BEST MONSTER? ME?";

        enum Kind { None, Medal, Sword, Slingshot, Monster }

        sealed class Grip
        {
            public Kind kind;
            public Vector2 start;
            public Vector3 offset;
            public bool moved, held;
        }

        sealed class FxSlot
        {
            public Coroutine co;
            public Vector3 v;
            public Quaternion q;
        }

        ShyMonsterLogic logic;
        Camera cam;
        HwToast toast;
        HwSay say;
        Grip grip;
        Renderer[] medalRenderers, swordRenderers, slingRenderers;
        int[] medalOrders, swordOrders, slingOrders;
        SpriteRenderer browLeftSr, browRightSr;
        readonly Dictionary<Transform, FxSlot>[] fxSlots = { new Dictionary<Transform, FxSlot>(), new Dictionary<Transform, FxSlot>(), new Dictionary<Transform, FxSlot>() };
        Vector3 monsterHome, monsterScale, medalHome, medalScale, swordHome, swordScale, slingHome, slingScale;
        Vector3 browLeftHome, browRightHome, blushScale;
        Quaternion medalRot, swordRot, slingRot;
        float vis = 1f, rage, lastGrowl = -10f;
        int growLines, maxLines, tickleLines, ignoredLines, medalFloorLines, decoyFloorLines, medalTapLines, swordTapLines, slingTapLines;
        bool captured, finishing;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            StopAllCoroutines();
            StopFx();
            if (!captured)
            {
                captured = true;
                logic = new ShyMonsterLogic();
                monsterHome = monster.position;
                monsterScale = monster.localScale;
                medalHome = medal.position;
                medalScale = medal.localScale;
                medalRot = medal.localRotation;
                swordHome = sword.position;
                swordScale = sword.localScale;
                swordRot = sword.localRotation;
                slingHome = slingshot.position;
                slingScale = slingshot.localScale;
                slingRot = slingshot.localRotation;
                browLeftHome = browLeft.localPosition;
                browRightHome = browRight.localPosition;
                browLeftSr = browLeft.GetComponentInChildren<SpriteRenderer>(true);
                browRightSr = browRight.GetComponentInChildren<SpriteRenderer>(true);
                blushScale = blush != null ? blush.transform.localScale : Vector3.one;
                medalRenderers = medal.GetComponentsInChildren<Renderer>(true);
                swordRenderers = sword.GetComponentsInChildren<Renderer>(true);
                slingRenderers = slingshot.GetComponentsInChildren<Renderer>(true);
                medalOrders = Orders(medalRenderers);
                swordOrders = Orders(swordRenderers);
                slingOrders = Orders(slingRenderers);
            }
            logic.Reset();
            if (reward != null) reward.ResetReward();
            grip = null;
            finishing = false;
            rage = 0f;
            growLines = maxLines = tickleLines = ignoredLines = medalFloorLines = decoyFloorLines = medalTapLines = swordTapLines = slingTapLines = 0;
            toast = new HwToast(infoText, infoHolder);
            say = new HwSay(monsterBubble);
            RestPose();
        }

        static int[] Orders(Renderer[] rs)
        {
            var o = new int[rs.Length];
            for (int i = 0; i < rs.Length; i++) o[i] = rs[i] != null ? rs[i].sortingOrder : 0;
            return o;
        }

        static void Raise(Renderer[] rs, int[] baseOrders, int boost)
        {
            if (rs == null) return;
            for (int i = 0; i < rs.Length; i++) if (rs[i] != null) rs[i].sortingOrder = baseOrders[i] + boost;
        }

        // ---------- Pose (sempre ricavate dalla logica: uno stato valido dopo pausa/spegnimento) ----------

        void RestPose()
        {
            say?.Hide();
            RaiseAll(0);
            vis = logic.Scale;
            monster.position = monsterHome;
            monster.localRotation = Quaternion.identity;
            monster.localScale = monsterScale * vis;
            HalloweenKit.Show(armLeft, true);
            HalloweenKit.Show(armRight, true);
            HalloweenKit.Show(shyArms, false);
            if (armLeft != null) armLeft.localRotation = Quaternion.identity;
            if (armRight != null) armRight.localRotation = Quaternion.identity;
            if (blush != null)
            {
                blush.transform.localScale = blushScale;
                HalloweenKit.SetAlpha(blush, 0f);
            }
            Face(logic.Growth, 0f);
            ItemsHome(true);
        }

        void RaiseAll(int boost)
        {
            Raise(medalRenderers, medalOrders, boost);
            Raise(swordRenderers, swordOrders, boost);
            Raise(slingRenderers, slingOrders, boost);
        }

        void ItemsHome(bool snap)
        {
            float f = snap ? 1f : 1f - Mathf.Exp(-Time.deltaTime * Follow);
            if (logic.Held != ShyMonsterItem.Medal) Home(medal, medalHome, medalScale, medalRot, f);
            if (logic.Held != ShyMonsterItem.Sword) Home(sword, swordHome, swordScale, swordRot, f);
            if (logic.Held != ShyMonsterItem.Slingshot) Home(slingshot, slingHome, slingScale, slingRot, f);
        }

        void Home(Transform t, Vector3 pos, Vector3 scale, Quaternion rot, float f)
        {
            if (t == null || FxBusy(t)) return;
            t.position = Vector3.Lerp(t.position, pos, f);
            t.localScale = Vector3.Lerp(t.localScale, scale, f);
            t.localRotation = Quaternion.Slerp(t.localRotation, rot, f);
        }

        /// <summary>Faccia secondo la rabbia: sopracciglia più aggrottate, scure e basse a ogni gradino; corpo più cupo; simbolo della rabbia dal 2° gradino.</summary>
        void Face(int growth, float wobble)
        {
            float k = Mathf.Clamp01(growth / (float)ShyMonsterLogic.MaxGrowth);
            float angle = BrowAngry + BrowAngryStep * growth + wobble;
            Vector3 down = new Vector3(0f, -0.035f * growth, 0f);
            if (browLeft != null) { browLeft.localRotation = Quaternion.Euler(0f, 0f, -angle); browLeft.localPosition = browLeftHome + down; }
            if (browRight != null) { browRight.localRotation = Quaternion.Euler(0f, 0f, angle); browRight.localPosition = browRightHome + down; }
            Color bc = Color.Lerp(BrowCalm, BrowDark, k);
            if (browLeftSr != null) browLeftSr.color = bc;
            if (browRightSr != null) browRightSr.color = bc;
            if (monsterBody != null) monsterBody.color = Color.Lerp(BodyCalm, BodyAngry, k);
            if (anger != null)
            {
                bool on = growth >= 2;
                anger.gameObject.SetActive(on);
                if (on) anger.localScale = Vector3.one * (0.85f + 0.15f * k + 0.12f * Mathf.Abs(Mathf.Sin(Time.time * 7f)));
            }
        }

        /// <summary>Scala vista del mostro (senza il respiro): la zona del petto si calcola da questa.</summary>
        float VisScale => vis;

        // ---------- Ciclo di vita ----------

        void OnDisable()
        {
            // pausa/pubblicità/livello spento: si molla tutto, si torna a uno stato valido
            CancelGrip();
            StopFx();
            if (captured && !finishing && !IsFinished && logic != null) RestPose();
        }

        void OnEnable()
        {
            if (!captured || IsFinished || logic == null) return;
            // spento a metà della festa: la si rifà da capo (la vittoria resta una sola)
            if (logic.Stage == ShyMonsterStage.Celebrating) { finishing = false; StartCoroutine(Finale()); }
        }

        void OnApplicationPause(bool paused) { if (paused) CancelGrip(); }
        void OnApplicationFocus(bool focus) { if (!focus) CancelGrip(); }

        /// <summary>Il dito sparisce: quello che si teneva torna sul pavimento (Pose lo fa scivolare), niente battute.</summary>
        void CancelGrip()
        {
            grip = null;
            if (logic != null && !logic.InputLocked) logic.Cancel();
            if (medalRenderers != null && !finishing) RaiseAll(0);
        }

        void Update()
        {
            toast?.Tick();
            say?.Tick();
            if (IsFinished || toast == null || logic == null) return;
            if (finishing) return;
            if (Time.timeScale <= 0f) { CancelGrip(); return; }   // pausa o pubblicità: niente input
            if (logic.Tick(Time.deltaTime)) V79ShyMonsterSound.Play(this, "deflate", 0.25f);
#if UNITY_EDITOR
            EditorKeys();
#endif
            if (!finishing) ReadPointer();
            if (!finishing) Pose();
        }

        /// <summary>Il mostro segue la scala voluta (cresce/si sgonfia morbido) e respira; gli oggetti non tenuti tornano a terra.</summary>
        void Pose()
        {
            float dt = Time.deltaTime;
            vis = Mathf.Lerp(vis, logic.Scale, 1f - Mathf.Exp(-dt * GrowFollow));
            rage = Mathf.Max(0f, rage - dt);
            float breathe = 1f + 0.015f * Mathf.Sin(Time.time * 2.2f);
            monster.localScale = new Vector3(monsterScale.x * vis, monsterScale.y * vis * breathe, monsterScale.z);
            // braccia: dondolano piano, si agitano dopo un colpo (di più se è arrabbiato)
            float k = logic.Growth / (float)ShyMonsterLogic.MaxGrowth;
            float sway = 4f * Mathf.Sin(Time.time * 1.8f) + (rage > 0f ? (10f + 8f * k) * Mathf.Sin(Time.time * 26f) * Mathf.Min(1f, rage * 2f) : 0f);
            if (armLeft != null) armLeft.localRotation = Quaternion.Euler(0f, 0f, sway);
            if (armRight != null) armRight.localRotation = Quaternion.Euler(0f, 0f, sway);
            Face(logic.Growth, rage > 0f ? 4f * Mathf.Sin(Time.time * 30f) : 0f);
            ItemsHome(false);
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

        bool FxBusy(Transform t)
        {
            for (int kind = 0; kind < fxSlots.Length; kind++)
                if (fxSlots[kind].TryGetValue(t, out FxSlot s) && s.co != null) return true;
            return false;
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
            if (Input.touchCount >= 2) { CancelGrip(); return; }   // due dita: il gesto si annulla (si torna a terra)
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

        Grip Begin(Vector2 w)
        {
            var g = new Grip { start = w, kind = Kind.None };
            if (finishing || logic.InputLocked) return g;
            // prima gli oggetti del pavimento (davanti, bersagli generosi), poi il mostro
            if (Over(medalArea, w)) g.kind = Kind.Medal;
            else if (Over(swordArea, w)) g.kind = Kind.Sword;
            else if (Over(slingshotArea, w)) g.kind = Kind.Slingshot;
            else if (Over(monsterArea, w)) g.kind = Kind.Monster;
            Transform t = ItemTransform(g.kind);
            if (t != null) g.offset = t.position - (Vector3)w;
            return g;
        }

        Transform ItemTransform(Kind k) => k == Kind.Medal ? medal : k == Kind.Sword ? sword : k == Kind.Slingshot ? slingshot : null;

        static ShyMonsterItem ItemOf(Kind k) =>
            k == Kind.Medal ? ShyMonsterItem.Medal : k == Kind.Sword ? ShyMonsterItem.Sword : k == Kind.Slingshot ? ShyMonsterItem.Slingshot : ShyMonsterItem.None;

        /// <summary>Il dito ha superato la soglia del tocco: si prende l'oggetto (il mostro invece si colpisce al rilascio).</summary>
        void StartDrag(Grip g)
        {
            ShyMonsterItem item = ItemOf(g.kind);
            if (item == ShyMonsterItem.None) return;
            if (!logic.Grab(item)) { g.kind = Kind.None; return; }
            g.held = true;
            Transform t = ItemTransform(g.kind);
            StopFxOn(t);
            t.localScale = (t == medal ? medalScale : t == sword ? swordScale : slingScale) * Lift;
            t.localRotation = Quaternion.identity;
            RaiseAll(0);
            if (t == medal) Raise(medalRenderers, medalOrders, HeldOrderBoost);
            else if (t == sword) Raise(swordRenderers, swordOrders, HeldOrderBoost);
            else Raise(slingRenderers, slingOrders, HeldOrderBoost);
            Haptics.Tap();
            V79ShyMonsterSound.Play(this, "pick", 0.4f);
        }

        void Drag(Grip g, Vector2 w)
        {
            Transform t = ItemTransform(g.kind);
            if (t == null) return;
            Vector2 p = HalloweenKit.ClampSafe(w + (Vector2)g.offset);
            t.position = new Vector3(p.x, p.y, t.position.z);
        }

        void End(Grip g)
        {
            grip = null;
            switch (g.kind)
            {
                case Kind.Monster: HitMonster(); break;   // tocco o colpetto: conta come un colpo
                case Kind.Medal:
                case Kind.Sword:
                case Kind.Slingshot:
                    if (g.held) Drop(ItemTransform(g.kind));
                    else TapItem(g.kind);
                    break;
            }
        }

        // ---------- Zone (dal mostro com'è adesso, nel mondo) ----------

        bool InChest(Vector3 p) => ShyMonsterLogic.InChest(p.x, p.y, monster.position.x, monster.position.y, VisScale * Mathf.Abs(monsterScale.y));

        bool OnMonster(Vector3 p)
        {
            if (monsterArea == null) return false;
            Vector2 c = monsterArea.transform.TransformPoint(monsterArea.offset);
            Vector3 s = monsterArea.transform.lossyScale;
            return ShyMonsterLogic.InZone(p.x, p.y, c.x, c.y, monsterArea.size.x * 0.5f * Mathf.Abs(s.x), monsterArea.size.y * 0.5f * Mathf.Abs(s.y), 0f);
        }

        // ---------- Le false piste ----------

        void HitMonster()
        {
            switch (logic.Hit())
            {
                case ShyMonsterHit.Grew:
                    Angry(GrowLines[growLines++ % GrowLines.Length], 0.55f);
                    break;
                case ShyMonsterHit.Maxed:
                    Angry(MaxLines[maxLines++ % MaxLines.Length], 0.45f);
                    break;
            }
        }

        void Angry(string line, float volume)
        {
            rage = 0.8f;
            FxShake(monster, 0.06f, 0.3f);
            if (Time.time - lastGrowl > 0.25f) { lastGrowl = Time.time; V79ShyMonsterSound.Play(this, "growl", volume); }
            Haptics.Medium();
            say.Show(line, 1.5f);
        }

        void TapItem(Kind k)
        {
            if (!logic.TapItem(ItemOf(k))) return;
            Transform t = ItemTransform(k);
            FxWiggle(t, 10f, 0.4f, 26f);
            V79ShyMonsterSound.Play(this, "pick", 0.25f);
            string line = k == Kind.Medal ? MedalTapLines[medalTapLines++ % MedalTapLines.Length]
                : k == Kind.Sword ? SwordTapLines[swordTapLines++ % SwordTapLines.Length]
                : SlingTapLines[slingTapLines++ % SlingTapLines.Length];
            toast.Show(line, 1.6f);
        }

        void Drop(Transform t)
        {
            RaiseAll(0);
            Vector3 p = t.position;
            switch (logic.Release(InChest(p), OnMonster(p)))
            {
                case ShyMonsterDrop.Floor:
                    V79ShyMonsterSound.Play(this, "swish", 0.35f);
                    toast.Show(t == medal ? MedalFloorLines[medalFloorLines++ % MedalFloorLines.Length] : DecoyFloorLines[decoyFloorLines++ % DecoyFloorLines.Length], 1.5f);
                    break;
                case ShyMonsterDrop.Ignored:
                    V79ShyMonsterSound.Play(this, "swish", 0.35f);
                    toast.Show(IgnoredLines[ignoredLines++ % IgnoredLines.Length], 1.6f);
                    break;
                case ShyMonsterDrop.Tickled:
                    rage = 0.6f;
                    FxShake(monster, 0.04f, 0.35f);
                    V79ShyMonsterSound.Play(this, "laugh", 0.5f);
                    Haptics.Light();
                    say.Show(TickleLines[tickleLines++ % TickleLines.Length], 1.6f);
                    break;
                case ShyMonsterDrop.Pinned:
                    StartCoroutine(Finale());
                    break;
            }
        }

        // ---------- La soluzione ----------

        Vector3 PinWorld(float scale) => monster.position + new Vector3(0f, pinY * scale * Mathf.Abs(monsterScale.y), 0f);

        /// <summary>
        /// La coccarda si appunta sul petto; il mostro la legge ("BEST MONSTER? ME?"), le sopracciglia diventano
        /// preoccupate, arrossisce (guance che si allargano, corpo rosa), si rimpicciolisce coprendosi gli occhi e scappa a
        /// destra. Poi la festa comune e Win, una volta sola. Rifatta da capo se il livello si spegne a metà.
        /// </summary>
        IEnumerator Finale()
        {
            if (finishing) yield break;
            finishing = true;
            grip = null;
            StopFx();
            HalloweenKit.Show(infoHolder, false);
            say.Hide();
            RaiseAll(0);
            Raise(medalRenderers, medalOrders, 10);   // davanti al corpo del mostro
            float s0 = logic.Scale;
            vis = s0;
            monster.position = monsterHome;
            monster.localRotation = Quaternion.identity;
            monster.localScale = monsterScale * s0;
            HalloweenKit.Show(armLeft, true);
            HalloweenKit.Show(armRight, true);
            HalloweenKit.Show(shyArms, false);
            if (blush != null) { blush.transform.localScale = blushScale; HalloweenKit.SetAlpha(blush, 0f); }
            sword.position = swordHome; sword.localScale = swordScale; sword.localRotation = swordRot;
            slingshot.position = slingHome; slingshot.localScale = slingScale; slingshot.localRotation = slingRot;

            // 1) la coccarda si appunta sul petto
            Vector3 from = medal.position, fromScale = medal.localScale;
            Quaternion fromRot = medal.localRotation;
            for (float t = 0f; t < 0.15f; t += Time.deltaTime)
            {
                float k = t / 0.15f;
                Vector3 to = PinWorld(s0); to.z = from.z;
                medal.position = Vector3.Lerp(from, to, k);
                medal.localScale = Vector3.Lerp(fromScale, medalScale * PinnedScale * s0, k);
                medal.localRotation = Quaternion.Slerp(fromRot, Quaternion.identity, k);
                yield return null;
            }
            PinMedal(s0);
            V79ShyMonsterSound.Play(this, "click", 0.55f);
            Haptics.Light();

            // 2) la legge: si calma, sopracciglia preoccupate, "BEST MONSTER? ME?"
            say.Show(ReadLine, 1.1f);
            if (medalText != null) FxPop(medalText.transform, 1.25f, 0.35f);   // la scritta "si fa leggere"
            if (anger != null) anger.gameObject.SetActive(false);
            float b0 = BrowAngry + BrowAngryStep * logic.Growth;
            for (float t = 0f; t < 0.4f; t += Time.deltaTime)
            {
                float k = Mathf.SmoothStep(0f, 1f, t / 0.4f);
                float a = Mathf.Lerp(b0, BrowWorried, k);
                if (browLeft != null) { browLeft.localRotation = Quaternion.Euler(0f, 0f, -a); browLeft.localPosition = browLeftHome + new Vector3(0f, 0.08f * k, 0f); }
                if (browRight != null) { browRight.localRotation = Quaternion.Euler(0f, 0f, a); browRight.localPosition = browRightHome + new Vector3(0f, 0.08f * k, 0f); }
                if (browLeftSr != null) browLeftSr.color = Color.Lerp(browLeftSr.color, BrowCalm, k);
                if (browRightSr != null) browRightSr.color = Color.Lerp(browRightSr.color, BrowCalm, k);
                if (armLeft != null) armLeft.localRotation = Quaternion.Euler(0f, 0f, -50f * k);   // le braccia si abbassano
                if (armRight != null) armRight.localRotation = Quaternion.Euler(0f, 0f, -50f * k);
                monster.localScale = monsterScale * s0;
                yield return null;
            }

            // 3) arrossisce: guance rosse che si allargano, tutto rosa
            V79ShyMonsterSound.Play(this, "aww", 0.5f);
            Color bodyFrom = monsterBody != null ? monsterBody.color : Color.white;
            for (float t = 0f; t < 0.4f; t += Time.deltaTime)
            {
                float k = Mathf.Clamp01(t / 0.4f);
                if (blush != null)
                {
                    HalloweenKit.SetAlpha(blush, k);
                    blush.transform.localScale = blushScale * Mathf.Lerp(0.7f, 1.45f, k);
                }
                if (monsterBody != null) monsterBody.color = Color.Lerp(bodyFrom, BodyBlush, k);
                yield return null;
            }

            // 4) si copre gli occhi e si rimpicciolisce (la coccarda resta sul petto)
            say.Hide();
            HalloweenKit.Show(armLeft, false);
            HalloweenKit.Show(armRight, false);
            HalloweenKit.Show(shyArms, true);
            Haptics.Light();
            const float tiny = 0.3f;
            for (float t = 0f; t < 0.3f; t += Time.deltaTime)
            {
                float k = 1f - (1f - t / 0.3f) * (1f - t / 0.3f);
                float s = Mathf.Lerp(s0, tiny, k);
                monster.localScale = monsterScale * s;
                PinMedal(s);
                yield return null;
            }
            monster.localScale = monsterScale * tiny;
            PinMedal(tiny);

            // 5) scappa a destra saltellando
            V79ShyMonsterSound.Play(this, "patter", 0.45f);
            Vector3 mFrom = monster.position;
            for (float t = 0f; t < 0.45f; t += Time.deltaTime)
            {
                float k = Mathf.Clamp01(t / 0.45f);
                float hop = Mathf.Abs(Mathf.Sin(t * 30f)) * 0.12f;
                monster.position = new Vector3(Mathf.Lerp(mFrom.x, exitX, k * k), mFrom.y + hop, mFrom.z);
                monster.localRotation = Quaternion.Euler(0f, 0f, -8f * Mathf.Sin(t * 30f));
                PinMedal(tiny);
                yield return null;
            }
            monster.position = new Vector3(exitX, mFrom.y, mFrom.z);
            PinMedal(tiny);

            if (reward != null) yield return reward.Play(Compliment);
            else yield return new WaitForSeconds(1.5f);
            if (logic.TryComplete()) Win(Compliment);
        }

        void PinMedal(float scale)
        {
            Vector3 to = PinWorld(scale);
            to.z = medal.position.z;
            medal.position = to;
            medal.localScale = medalScale * PinnedScale * scale;
            medal.localRotation = Quaternion.identity;
        }

#if UNITY_EDITOR
        // ---------- Tasti di prova nell'Editor ----------

        void EditorKeys()
        {
            if (logic.InputLocked || grip != null || logic.Holding) return;
            if (Input.GetKeyDown(KeyCode.T)) HitMonster();
            if (Input.GetKeyDown(KeyCode.M)) KeyDrop(Kind.Medal, PinWorld(VisScale));
            if (Input.GetKeyDown(KeyCode.H)) KeyDrop(Kind.Medal, monster.position + new Vector3(0f, 3.3f * VisScale, 0f));
            if (Input.GetKeyDown(KeyCode.F)) KeyDrop(Kind.Medal, new Vector3(-2f, -1.5f, 0f));
            if (Input.GetKeyDown(KeyCode.O))
            {
                // passa sopra il petto (nessun effetto) e viene lasciata lontano
                var g = new Grip { kind = Kind.Medal };
                StartDrag(g);
                if (g.held)
                {
                    Vector3 c = PinWorld(VisScale);
                    medal.position = new Vector3(c.x, c.y, medal.position.z);
                    medal.position = new Vector3(2.1f, -2.6f, medal.position.z);
                    Drop(medal);
                }
            }
            if (Input.GetKeyDown(KeyCode.S)) KeyDrop(Kind.Sword, PinWorld(VisScale));
            if (Input.GetKeyDown(KeyCode.L)) KeyDrop(Kind.Slingshot, monster.position + new Vector3(0.9f, 2.5f * VisScale, 0f));
            if (Input.GetKeyDown(KeyCode.D)) KeyDrop(Kind.Sword, new Vector3(2f, 2f, 0f));
            if (Input.GetKeyDown(KeyCode.P)) CancelGrip();
            if (Input.GetKeyDown(KeyCode.R)) OnLevelStart();
        }

        void KeyDrop(Kind k, Vector3 at)
        {
            var g = new Grip { kind = k };
            StartDrag(g);
            if (!g.held) return;
            Transform t = ItemTransform(k);
            t.position = new Vector3(at.x, at.y, t.position.z);
            Drop(t);
        }
#endif
    }

    /// <summary>Suoni sintetizzati di The Shy Monster: ringhio, risata, presa, clic, fruscio, sgonfiata, "aww", passettini.</summary>
    public static class V79ShyMonsterSound
    {
        const int Rate = 22050;
        static readonly Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>();
        static readonly System.Random rnd = new System.Random(290);

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
                case "growl":    // ringhio: dente di sega grave che vibra, con rumore
                    {
                        float ph = 0f;
                        c = Make(kind, 0.6f, t =>
                        {
                            float f = 70f + 25f * Mathf.Sin(t * 2f * Mathf.PI * 9f) + 40f * t;
                            ph += f / Rate;
                            float saw = (ph - Mathf.Floor(ph)) * 2f - 1f;
                            return (saw * 0.5f + N() * 0.18f) * Mathf.Min(1f, t * 25f) * Mathf.Clamp01((0.6f - t) * 6f);
                        });
                        break;
                    }
                case "laugh":    // "ho-ho-ho": tre colpetti
                    c = Make(kind, 0.55f, t =>
                    {
                        float u = t % 0.17f;
                        return (Mathf.Sin(t * 2f * Mathf.PI * (190f - 60f * t)) * 0.45f + N() * 0.06f) * Mathf.Exp(-u * 18f) * Mathf.Min(1f, u * 200f);
                    });
                    break;
                case "pick":     // oggetto che si stacca: "plip"
                    c = Make(kind, 0.08f, t => Mathf.Sin(t * 2f * Mathf.PI * Mathf.Lerp(600f, 1100f, t / 0.08f)) * 0.4f * Mathf.Exp(-t * 30f));
                    break;
                case "click":    // coccarda appuntata
                    c = Make(kind, 0.06f, t => N() * 0.5f * Mathf.Exp(-t * 110f) + Mathf.Sin(t * 2f * Mathf.PI * 1200f) * 0.25f * Mathf.Exp(-t * 70f));
                    break;
                case "swish":    // torna a terra
                    {
                        float lp = 0f;
                        c = Make(kind, 0.22f, t =>
                        {
                            lp += (N() - lp) * 0.3f;
                            return lp * 0.5f * Mathf.Sin(t / 0.22f * Mathf.PI);
                        });
                        break;
                    }
                case "deflate":  // si sgonfia di un gradino: "pfff" che scende
                    {
                        float lp = 0f;
                        c = Make(kind, 0.3f, t =>
                        {
                            lp += (N() - lp) * Mathf.Lerp(0.5f, 0.1f, t / 0.3f);
                            return lp * 0.4f * Mathf.Sin(t / 0.3f * Mathf.PI);
                        });
                        break;
                    }
                case "aww":      // imbarazzo: glissando dolce in su e in giù
                    c = Make(kind, 0.6f, t =>
                    {
                        float f = 520f + 260f * Mathf.Sin(t / 0.6f * Mathf.PI);
                        return Mathf.Sin(t * 2f * Mathf.PI * f) * 0.3f * Mathf.Min(1f, t * 30f) * Mathf.Clamp01((0.6f - t) * 8f);
                    });
                    break;
                case "patter":   // passettini veloci
                    c = Make(kind, 0.45f, t =>
                    {
                        float u = t % 0.06f;
                        return (N() * 0.4f + Mathf.Sin(t * 2f * Mathf.PI * 300f) * 0.2f) * Mathf.Exp(-u * 90f);
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
            AudioClip clip = AudioClip.Create("v79a_" + name, n, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
