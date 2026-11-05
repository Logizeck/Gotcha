using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels.V64;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 38 della roadmap "The Impossible Hug" (id impossible_hug) — "MAKE THE TWO FRIENDS MAKE UP." Nel parco due
    /// amici tondi, Bo (blu, a sinistra) e Pip (rosa, a destra), stanno a braccia conserte con la faccia rossa di rabbia e
    /// guardano altrove; in mezzo, sopra di loro, un cuore spezzato in due metà lontane.
    /// Inganno: trascinato con un dito solo, un amico punta i piedi (avanza appena, si inclina all'indietro, segno di
    /// rabbia) e l'altro si allontana ancora un po' ("He has to come first!", "Not until she says sorry!"); lasciato,
    /// tutti e due fanno un passo indietro oltre la partenza e poi tornano a casa (sempre dentro lo schermo). Un tocco:
    /// "Hmph!". Tutto reversibile, mai sconfitta.
    /// Soluzione: due dita, una per amico (in qualunque ordine), e trascinarli insieme verso il centro: tenuti tutti e
    /// due seguono le dita, da vicini si guardano timidi e le metà del cuore si avvicinano; quando TUTTI E DUE sono nella
    /// zona centrale mentre sono tenuti, si abbracciano. Se un dito si alza, quell'amico torna indietro e l'altro torna a
    /// puntare i piedi. Lo stesso dito non può tenerli tutti e due.
    /// Vittoria: le metà del cuore si uniscono e brillano, i due si abbracciano a occhi chiusi ("Sorry!" / "Me too!") e
    /// "abbracciano lo schermo" (si chinano verso la camera, crescono un po'), cuoricini che volano; LevelReward
    /// "YOU SAVED A FRIENDSHIP!" -> Win una volta sola.
    /// Dita: al massimo due (V64AFingers). Pausa, app in background o livello spento lasciano andare tutto.
    /// La logica è in Gotcha.Levels.V64.ImpossibleHugLogic (coordinate locali rispetto al centro della scena).
    /// Tasti di prova nell'Editor: H soluzione a due dita (tutti e due al centro), O un dito trascina Bo e lascia,
    /// I un dito trascina Pip e lascia, L due dita vicine al centro e poi un dito si alza, Y lo stesso dito prova a
    /// prenderli tutti e due, T tocco su Bo, K tocco su Pip, P pausa simulata (dita perse), R ricomincia.
    /// Shift + clic pianta un secondo dito fermo (V64AFingers): tiene fermo un amico mentre il mouse trascina l'altro.
    /// </summary>
    public class V81ImpossibleHugLevel : LevelController
    {
        [Header("I due amici: 0 = Bo (sinistra), 1 = Pip (destra)")]
        [SerializeField] Transform[] buddies;      // radici posate dal livello (collider a cerchio generoso)
        [SerializeField] Transform[] looks;        // figli per gli effetti (Pip è rovesciato in x)
        [SerializeField] Collider2D[] areas;
        [SerializeField] Transform[] facesAngry;
        [SerializeField] Transform[] facesShy;
        [SerializeField] Transform[] facesHug;
        [SerializeField] Transform[] armsCrossed;
        [SerializeField] Transform[] armsHug;
        [SerializeField] Transform[] angerMarks;
        [Header("Il cuore spezzato")]
        [SerializeField] Transform heartLeft;
        [SerializeField] Transform heartRight;
        [SerializeField] SpriteRenderer heartGlow;
        [SerializeField] Transform[] floatHearts;
        [Header("Fumetti, scritte e festa")]
        [SerializeField] SpeechBubble[] bubbles;   // uno per amico
        [SerializeField] TextMesh infoText;
        [SerializeField] Transform infoHolder;
        [SerializeField] LevelReward reward;

        const string Compliment = "YOU SAVED A FRIENDSHIP!";
        const int L = ImpossibleHugLogic.Left, R = ImpossibleHugLogic.Right;
        const float Follow = 22f, Spring = 55f, Damping = 8f, SulkTime = 0.7f, SulkStep = 0.22f;
        const float HeartGapMax = 0.44f, HugX = 0.5f, HugLift = 0.12f, ScreenHug = 1.28f, FloatHeartSize = 0.7f;

        static readonly string[][] SoloLines =
        {
            new[] { "Not until she says sorry!", "She started it!", "Nope. Not me first.", "I'm not moving first!" },
            new[] { "He has to come first!", "Why should I go first?", "Hmph! Him first.", "Not a chance!" },
        };
        static readonly string[][] TapLines =
        {
            new[] { "Hmph!", "Don't poke me.", "I'm still mad." },
            new[] { "Hmph!", "Go away.", "I'm NOT talking to him." },
        };
        static readonly string[] BackLines = { "They both step back.", "Further apart than before.", "Pride: 1. Friendship: 0.", "Nobody wants to go first." };
        static readonly string[] SplitLines = { "So close... and apart again.", "One let go. The other sulks.", "Almost made up. Almost." };
        static readonly string[] HugLines = { "Sorry!", "Me too!" };

        enum Face { Angry, Shy, Hug }

        sealed class FxSlot
        {
            public Coroutine co;
            public Vector3 v;
            public Quaternion q;
        }

        ImpossibleHugLogic logic;
        Camera cam;
        HwToast toast;
        readonly HwSay[] says = new HwSay[2];
        readonly Dictionary<int, Vector2> starts = new Dictionary<int, Vector2>();
        readonly HashSet<int> spoke = new HashSet<int>();
        readonly HashSet<int> seen = new HashSet<int>();
        readonly Dictionary<Transform, FxSlot>[] fxSlots = { new Dictionary<Transform, FxSlot>(), new Dictionary<Transform, FxSlot>() };
        readonly Vector2[] vis = new Vector2[2], vel = new Vector2[2];
        readonly float[] sulkT = new float[2];
        readonly int[] soloLines = new int[2], tapLines = new int[2];
        Vector3[] buddyScale, bubblePos;
        Vector3 centre, heartLeftPos, heartRightPos, heartLeftScale, heartRightScale;
        int backLines, splitLines;
        float lastScuff = -10f;
        bool captured, finishing;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            StopAllCoroutines();
            StopFx();
            if (!captured)
            {
                captured = true;
                logic = new ImpossibleHugLogic();
                centre = (buddies[L].position + buddies[R].position) * 0.5f;
                buddyScale = new[] { buddies[L].localScale, buddies[R].localScale };
                bubblePos = new Vector3[2];
                for (int i = 0; i < 2; i++) if (bubbles != null && i < bubbles.Length && bubbles[i] != null) bubblePos[i] = bubbles[i].transform.position;
                if (heartLeft != null) { heartLeftPos = heartLeft.localPosition; heartLeftScale = heartLeft.localScale; }
                if (heartRight != null) { heartRightPos = heartRight.localPosition; heartRightScale = heartRight.localScale; }
            }
            logic.Reset();
            if (reward != null) reward.ResetReward();
            V64AFingers.Reset();
            starts.Clear();
            spoke.Clear();
            finishing = false;
            backLines = splitLines = 0;
            for (int i = 0; i < 2; i++)
            {
                soloLines[i] = tapLines[i] = 0;
                if (bubbles != null && i < bubbles.Length && bubbles[i] != null) bubbles[i].transform.position = bubblePos[i];
                says[i] = new HwSay(bubbles != null && i < bubbles.Length ? bubbles[i] : null);
            }
            toast = new HwToast(infoText, infoHolder);
            RestPose();
        }

        // ---------- Pose (sempre ricavate dalla logica: uno stato valido dopo pausa/spegnimento) ----------

        void RestPose()
        {
            for (int i = 0; i < 2; i++)
            {
                says[i]?.Hide();
                vis[i] = new Vector2(logic.X(i), logic.Y(i));
                vel[i] = Vector2.zero;
                sulkT[i] = 0f;
                buddies[i].position = Pos(i, vis[i]);
                buddies[i].localRotation = Quaternion.identity;
                buddies[i].localScale = buddyScale[i];
                Faces(i, Face.Angry);
                Arms(i, false);
                HalloweenKit.Show(Pick(angerMarks, i), false);
            }
            if (heartLeft != null) { heartLeft.localScale = heartLeftScale; heartLeft.localPosition = heartLeftPos; }
            if (heartRight != null) { heartRight.localScale = heartRightScale; heartRight.localPosition = heartRightPos; }
            if (heartGlow != null) HalloweenKit.SetAlpha(heartGlow, 0f);
            HideHearts();
            PoseHeart(1f, 0f);
        }

        static Transform Pick(Transform[] a, int i) => a != null && i < a.Length ? a[i] : null;

        Vector3 Pos(int i, Vector2 local) => new Vector3(centre.x + local.x, centre.y + local.y, buddies[i].position.z);

        void Faces(int i, Face f)
        {
            HalloweenKit.Show(Pick(facesAngry, i), f == Face.Angry);
            HalloweenKit.Show(Pick(facesShy, i), f == Face.Shy);
            HalloweenKit.Show(Pick(facesHug, i), f == Face.Hug);
        }

        void Arms(int i, bool hug)
        {
            HalloweenKit.Show(Pick(armsCrossed, i), !hug);
            HalloweenKit.Show(Pick(armsHug, i), hug);
        }

        void HideHearts()
        {
            if (floatHearts == null) return;
            foreach (Transform h in floatHearts) HalloweenKit.Show(h, false);
        }

        /// <summary>Le metà del cuore: più gli amici sono lontani, più le metà si allontanano e si piegano in fuori.</summary>
        void PoseHeart(float apart, float shy)
        {
            float k = Mathf.Clamp(apart, 0f, 1.4f);
            float gap = 0.03f + HeartGapMax * k;
            float tilt = 3f + 11f * k;
            float beat = 1f + shy * 0.06f * Mathf.Sin(Time.time * 9f);
            if (heartLeft != null)
            {
                heartLeft.localPosition = heartLeftPos + new Vector3(-gap, 0f, 0f);
                heartLeft.localRotation = Quaternion.Euler(0f, 0f, tilt);
                heartLeft.localScale = heartLeftScale * beat;
            }
            if (heartRight != null)
            {
                heartRight.localPosition = heartRightPos + new Vector3(gap, 0f, 0f);
                heartRight.localRotation = Quaternion.Euler(0f, 0f, -tilt);
                heartRight.localScale = heartRightScale * beat;
            }
        }

        // ---------- Ciclo di vita ----------

        void OnDisable()
        {
            // pausa/pubblicità/livello spento: si molla tutto, si torna a uno stato valido
            CancelAll();
            StopFx();
            if (captured && !finishing && !IsFinished && logic != null) RestPose();
        }

        void OnEnable()
        {
            if (!captured || IsFinished || logic == null) return;
            // spento a metà della festa: la si rifà da capo (la vittoria resta una sola)
            if (logic.Stage == ImpossibleHugStage.Hugging) { finishing = false; StartCoroutine(Finale()); }
        }

        void OnApplicationPause(bool paused) { if (paused) CancelAll(); }
        void OnApplicationFocus(bool focus) { if (!focus) CancelAll(); }

        /// <summary>Le dita spariscono: gli amici tornano a casa (Pose li fa scivolare), niente battute.</summary>
        void CancelAll()
        {
            starts.Clear();
            spoke.Clear();
            V64AFingers.Reset();
            if (logic != null && !logic.InputLocked) logic.Cancel();
        }

        void Update()
        {
            toast?.Tick();
            says[L]?.Tick();
            says[R]?.Tick();
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
        /// Gli amici: tenuti seguono la logica da vicino, lasciati tornano con una molla; il passo indietro "offeso" li porta
        /// un po' oltre la casa e poi indietro (sempre dentro i limiti). Saltellano quando si muovono, da soli si inclinano
        /// all'indietro (puntano i piedi), insieme si sporgono l'uno verso l'altro. Facce, segni di rabbia e cuore.
        /// </summary>
        void Pose()
        {
            float dt = Mathf.Min(Time.deltaTime, 0.05f);
            bool together = logic.Together, shy = logic.Shy;
            float avg = 0f;
            for (int i = 0; i < 2; i++)
            {
                Vector2 target = new Vector2(logic.X(i), logic.Y(i));
                Vector2 before = vis[i];
                if (logic.IsHeld(i))
                {
                    vis[i] = Vector2.Lerp(vis[i], target, 1f - Mathf.Exp(-dt * Follow));
                    vel[i] = Vector2.zero;
                }
                else
                {
                    vel[i] += (-(vis[i] - target) * Spring - vel[i] * Damping) * dt;
                    vis[i] += vel[i] * dt;
                }
                float outDir = i == L ? -1f : 1f;
                float extra = 0f;
                if (sulkT[i] > 0f)
                {
                    sulkT[i] = Mathf.Max(0f, sulkT[i] - dt);
                    extra = SulkStep * Mathf.Sin((1f - sulkT[i] / SulkTime) * Mathf.PI);
                }
                Vector2 shown = vis[i] + new Vector2(outDir * extra, 0f);
                shown.x = Mathf.Clamp(shown.x, -ImpossibleHugLogic.MaxX, ImpossibleHugLogic.MaxX);
                shown.y = Mathf.Clamp(shown.y, ImpossibleHugLogic.MinY, ImpossibleHugLogic.MaxY);
                float speed = dt > 0f ? (vis[i] - before).magnitude / dt : 0f;
                float hop = Mathf.Abs(Mathf.Sin(Time.time * 15f + i)) * 0.07f * Mathf.Clamp01(speed * 0.8f + extra * 4f);
                buddies[i].position = Pos(i, shown + new Vector2(0f, hop));

                bool resisting = logic.IsHeld(i) && !together;
                float lean;
                if (resisting) lean = -outDir * 12f + 3f * Mathf.Sin(Time.time * 30f);          // punta i piedi, all'indietro
                else if (together) lean = outDir * 5f;                                           // si sporge verso l'altro
                else lean = -outDir * 8f * extra / SulkStep;                                     // passo indietro offeso
                buddies[i].localRotation = Quaternion.Slerp(buddies[i].localRotation, Quaternion.Euler(0f, 0f, lean), 1f - Mathf.Exp(-dt * 12f));

                Faces(i, shy ? Face.Shy : Face.Angry);
                Transform anger = Pick(angerMarks, i);
                bool angry = resisting || sulkT[i] > 0f || (logic.IsHeld(1 - i) && !together);
                HalloweenKit.Show(anger, angry);
                if (angry && anger != null) anger.localScale = Vector3.one * (1f + 0.18f * Mathf.Abs(Mathf.Sin(Time.time * 10f)));
                avg += shown.magnitude;
                if (speed > 0.6f && Time.time - lastScuff > 0.22f) { lastScuff = Time.time; V81ImpossibleHugSound.Play(this, "scuff", 0.25f); }
            }
            avg *= 0.5f;
            float apart = (avg - ImpossibleHugLogic.HugRadius) / (ImpossibleHugLogic.HomeX - ImpossibleHugLogic.HugRadius);
            PoseHeart(apart, shy ? 1f : 0f);
        }

        // ---------- Effetti che non si accavallano (come nei livelli 0.73-0.80) ----------

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

        // ---------- Dita (al massimo due, un amico per dito) ----------

        Vector2 Local(Vector2 w) => new Vector2(w.x - centre.x, w.y - centre.y);

        void ReadFingers()
        {
            List<V64AFinger> fingers = V64AFingers.Read(cam);
            seen.Clear();
            foreach (V64AFinger f in fingers)
            {
                seen.Add(f.id);
                if (f.began)
                {
                    if (logic.HeldBy(f.id) >= 0) EndFinger(f.id);
                    BeginFinger(f.id, f.world);
                }
                if (logic.HeldBy(f.id) >= 0)
                {
                    MoveFinger(f.id, f.world);
                    if (finishing) return;
                }
                if (f.ended) EndFinger(f.id);
            }
            // dita sparite senza "Ended": si chiudono i gesti
            for (int i = 0; i < 2; i++)
            {
                int h = logic.HolderOf(i);
                if (h != ImpossibleHugLogic.NoFinger && !seen.Contains(h)) EndFinger(h);
            }
        }

        static bool Over(Collider2D c, Vector2 p) => c != null && c.gameObject.activeInHierarchy && c.OverlapPoint(p);

        /// <summary>Il dito prende l'amico libero più vicino sotto di sé (mai quello già tenuto, mai tutti e due).</summary>
        void BeginFinger(int id, Vector2 w)
        {
            if (finishing || logic.InputLocked) return;
            int who = -1;
            float best = float.MaxValue;
            for (int i = 0; i < 2; i++)
            {
                if (logic.IsHeld(i) || !Over(areas != null && i < areas.Length ? areas[i] : null, w)) continue;
                float d = Vector2.Distance(w, buddies[i].position);
                if (d < best) { best = d; who = i; }
            }
            if (who < 0) return;
            Vector2 p = Local(w);
            if (!logic.Grab(id, who, p.x, p.y)) return;
            starts[id] = w;
            spoke.Remove(id);
            Haptics.Tap();
            if (logic.Together)
            {
                // tutti e due presi: si girano un attimo a guardarsi
                V81ImpossibleHugSound.Play(this, "boop", 0.35f);
                for (int i = 0; i < 2; i++) { says[i].Hide(); FxPop(Pick(looks, i), 1.06f, 0.25f); }
            }
            else V81ImpossibleHugSound.Play(this, "grab", 0.3f);
        }

        void MoveFinger(int id, Vector2 w)
        {
            Vector2 p = Local(w);
            ImpossibleHugMove m = logic.Move(id, p.x, p.y);
            if (m == ImpossibleHugMove.Hug) { StartCoroutine(Finale()); return; }
            if (m != ImpossibleHugMove.Resisting || spoke.Contains(id)) return;
            if (!starts.TryGetValue(id, out Vector2 s) || Vector2.Distance(s, w) <= ImpossibleHugLogic.TapSlop) return;
            // la prima volta che lo si trascina da solo: punta i piedi e protesta
            spoke.Add(id);
            int who = logic.HeldBy(id);
            if (who < 0) return;
            Say(who, SoloLines[who][soloLines[who]++ % SoloLines[who].Length], 1.8f);
            V81ImpossibleHugSound.Play(this, "hmph", 0.45f);
            Haptics.Light();
        }

        void EndFinger(int id)
        {
            int who = logic.HeldBy(id);
            starts.Remove(id);
            spoke.Remove(id);
            ImpossibleHugDrop drop = logic.Release(id);
            switch (drop)
            {
                case ImpossibleHugDrop.Tap: Tap(who); break;
                case ImpossibleHugDrop.SteppedBack: SteppedBack(); break;
                case ImpossibleHugDrop.Separated: Separated(who); break;
            }
        }

        void Say(int who, string line, float seconds)
        {
            says[1 - who]?.Hide();   // un fumetto alla volta: non si coprono
            says[who]?.Show(line, seconds);
        }

        // ---------- Le false piste ----------

        void Tap(int who)
        {
            if (who < 0) return;
            FxPop(Pick(looks, who), 1.1f, 0.3f);
            V81ImpossibleHugSound.Play(this, "hmph", 0.45f);
            Say(who, TapLines[who][tapLines[who]++ % TapLines[who].Length], 1.4f);
        }

        void SteppedBack()
        {
            sulkT[L] = sulkT[R] = SulkTime;
            V81ImpossibleHugSound.Play(this, "huff", 0.45f);
            Haptics.Light();
            toast.Show(BackLines[backLines++ % BackLines.Length], 1.7f);
        }

        void Separated(int who)
        {
            if (who >= 0) sulkT[who] = SulkTime;
            V81ImpossibleHugSound.Play(this, "huff", 0.4f);
            toast.Show(SplitLines[splitLines++ % SplitLines.Length], 1.7f);
        }

        // ---------- La soluzione ----------

        /// <summary>
        /// L'abbraccio: i due si stringono al centro (braccia aperte, occhi chiusi), le metà del cuore si uniscono e brillano,
        /// "Sorry!" / "Me too!", poi "abbracciano lo schermo" (si chinano verso la camera e crescono un po') mentre volano
        /// i cuoricini; festa comune; Win una volta sola. Rifatta da capo se il livello si spegne a metà.
        /// </summary>
        IEnumerator Finale()
        {
            if (finishing) yield break;
            finishing = true;
            starts.Clear();
            spoke.Clear();
            V64AFingers.Reset();
            StopFx();
            HalloweenKit.Show(infoHolder, false);
            HideHearts();
            for (int i = 0; i < 2; i++)
            {
                says[i].Hide();
                sulkT[i] = 0f;
                HalloweenKit.Show(Pick(angerMarks, i), false);
                Faces(i, Face.Hug);
                Arms(i, true);
            }

            // 1) si stringono al centro (si sporgono l'uno verso l'altro)
            V81ImpossibleHugSound.Play(this, "pop", 0.5f);
            Haptics.Medium();
            Vector3[] from = { buddies[L].position, buddies[R].position };
            Quaternion[] rotFrom = { buddies[L].localRotation, buddies[R].localRotation };
            for (float t = 0f; t < 0.3f; t += Time.deltaTime)
            {
                float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / 0.3f));
                for (int i = 0; i < 2; i++)
                {
                    float sx = i == L ? -1f : 1f;
                    buddies[i].position = Vector3.Lerp(from[i], Pos(i, new Vector2(sx * HugX, HugLift)), k);
                    buddies[i].localRotation = Quaternion.Slerp(rotFrom[i], Quaternion.Euler(0f, 0f, sx * 7f), k);
                }
                PoseHeart(Mathf.Lerp(0.5f, 0f, k), 0f);
                yield return null;
            }
            for (int i = 0; i < 2; i++)
            {
                buddies[i].position = Pos(i, new Vector2((i == L ? -1f : 1f) * HugX, HugLift));
                buddies[i].localRotation = Quaternion.Euler(0f, 0f, (i == L ? -1f : 1f) * 7f);
            }

            // 2) le metà del cuore si uniscono e brillano
            PoseHeart(-1f, 0f);
            if (heartLeft != null) { heartLeft.localPosition = heartLeftPos; heartLeft.localRotation = Quaternion.identity; }
            if (heartRight != null) { heartRight.localPosition = heartRightPos; heartRight.localRotation = Quaternion.identity; }
            V81ImpossibleHugSound.Play(this, "chime", 0.5f);
            FxPop(heartLeft, 1.25f, 0.35f);
            FxPop(heartRight, 1.25f, 0.35f);
            if (heartGlow != null) StartCoroutine(HalloweenKit.Fade(heartGlow, 0.85f, 0.3f));
            Say(L, HugLines[0], 0.5f);
            yield return new WaitForSeconds(0.45f);
            Say(R, HugLines[1], 0.55f);

            // 3) "abbracciano lo schermo": si chinano verso la camera, crescono un po', cuoricini che volano
            V81ImpossibleHugSound.Play(this, "aww", 0.5f);
            StartCoroutine(FloatHearts());
            for (float t = 0f; t < 0.5f; t += Time.deltaTime)
            {
                float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / 0.5f));
                for (int i = 0; i < 2; i++)
                {
                    float sx = i == L ? -1f : 1f;
                    buddies[i].localScale = buddyScale[i] * Mathf.Lerp(1f, ScreenHug, k);
                    buddies[i].position = Pos(i, new Vector2(sx * HugX * Mathf.Lerp(1f, ScreenHug, k), HugLift - 0.15f * k));
                    buddies[i].localRotation = Quaternion.Euler(0f, 0f, sx * Mathf.Lerp(7f, 3f, k));
                }
                yield return null;
            }
            yield return new WaitForSeconds(0.15f);
            for (int i = 0; i < 2; i++) says[i].Hide();

            if (reward != null) yield return reward.Play(Compliment);
            else yield return new WaitForSeconds(1.5f);
            if (logic.TryComplete()) Win(Compliment);
        }

        /// <summary>I cuoricini salgono dall'abbraccio ondeggiando e svaniscono (durante tutta la festa); gli amici dondolano piano.</summary>
        IEnumerator FloatHearts()
        {
            int n = floatHearts != null ? floatHearts.Length : 0;
            var ht = new float[n];
            for (int i = 0; i < n; i++) ht[i] = -1f;
            float next = 0f;
            int idx = 0;
            for (float t = 0f; t < 4f; t += Time.deltaTime)
            {
                float dt = Time.deltaTime;
                for (int b = 0; b < 2; b++)
                {
                    float sx = b == L ? -1f : 1f;
                    buddies[b].localRotation = Quaternion.Euler(0f, 0f, sx * 3f + 4f * Mathf.Sin(t * 3f));
                }
                if (n > 0 && t >= next)
                {
                    next = t + 0.18f;
                    int i = idx++ % n;
                    ht[i] = 0f;
                    float x = centre.x + ((i % 2 == 0) ? -1f : 1f) * (0.15f + 0.22f * (i % 3));
                    floatHearts[i].position = new Vector3(x, centre.y + 0.6f, floatHearts[i].position.z);
                    HalloweenKit.Show(floatHearts[i], true);
                }
                for (int i = 0; i < n; i++)
                {
                    if (ht[i] < 0f || floatHearts[i] == null) continue;
                    ht[i] += dt;
                    float k = ht[i] / 1.4f;
                    if (k >= 1f) { ht[i] = -1f; HalloweenKit.Show(floatHearts[i], false); continue; }
                    Transform h = floatHearts[i];
                    h.position += new Vector3(Mathf.Sin(ht[i] * 5f + i) * 0.35f * dt, 1.3f * dt, 0f);
                    h.localScale = Vector3.one * (FloatHeartSize * Mathf.Sin(Mathf.Min(1f, k * 3f) * Mathf.PI * 0.5f) * (1f - k * k));
                }
                yield return null;
            }
            HideHearts();
        }

#if UNITY_EDITOR
        // ---------- Tasti di prova nell'Editor ----------

        const int KeyA = 901, KeyB = 902;

        void EditorKeys()
        {
            if (logic.InputLocked || logic.HeldCount > 0) return;
            if (Input.GetKeyDown(KeyCode.H)) KeyTogether(0.15f, true);
            if (Input.GetKeyDown(KeyCode.L)) KeyTogether(0.9f, false);
            if (Input.GetKeyDown(KeyCode.O)) KeySolo(L, KeyA);
            if (Input.GetKeyDown(KeyCode.I)) KeySolo(R, KeyA);
            if (Input.GetKeyDown(KeyCode.Y))
            {
                // lo stesso dito su Bo e poi su Pip: Pip non viene preso
                if (logic.Grab(KeyA, L, -ImpossibleHugLogic.HomeX, 0f)) logic.Grab(KeyA, R, ImpossibleHugLogic.HomeX, 0f);
                KeyDrag(KeyA, L);
            }
            if (Input.GetKeyDown(KeyCode.T)) { if (logic.Grab(KeyA, L, logic.X(L), 0f)) EndFinger(KeyA); }
            if (Input.GetKeyDown(KeyCode.K)) { if (logic.Grab(KeyA, R, logic.X(R), 0f)) EndFinger(KeyA); }
            if (Input.GetKeyDown(KeyCode.P)) CancelAll();
            if (Input.GetKeyDown(KeyCode.R)) OnLevelStart();
        }

        /// <summary>Due dita finte prendono Bo e Pip e li portano a ±x dal centro; senza abbraccio una delle due si alza.</summary>
        void KeyTogether(float x, bool hug)
        {
            if (!logic.Grab(KeyA, L, logic.X(L), 0f) || !logic.Grab(KeyB, R, logic.X(R), 0f)) { logic.Cancel(); return; }
            float l0 = logic.X(L), r0 = logic.X(R);
            for (int s = 1; s <= 10; s++)
            {
                float k = s / 10f;
                if (logic.Move(KeyA, Mathf.Lerp(l0, -x, k), 0f) == ImpossibleHugMove.Hug || logic.Move(KeyB, Mathf.Lerp(r0, x, k), 0f) == ImpossibleHugMove.Hug)
                {
                    StartCoroutine(Finale());
                    return;
                }
            }
            if (hug) return;
            EndFinger(KeyB);   // un dito si alza: Pip torna indietro
            logic.Cancel();    // e il dito finto di Bo sparisce senza contare
        }

        void KeySolo(int who, int finger)
        {
            if (!logic.Grab(finger, who, logic.X(who), 0f)) return;
            KeyDrag(finger, who);
        }

        void KeyDrag(int finger, int who)
        {
            starts[finger] = Pos(who, new Vector2(logic.X(who), 0f));
            MoveFinger(finger, centre);   // trascinato fino al centro: punta i piedi
            vis[who] = new Vector2(logic.X(who), logic.Y(who));
            vis[1 - who] = new Vector2(logic.X(1 - who), logic.Y(1 - who));
            EndFinger(finger);
        }
#endif
    }

    /// <summary>Suoni sintetizzati di The Impossible Hug: "hmph", sbuffo, passetti, presa, "boop", schiocco, rintocco del cuore, "aww".</summary>
    public static class V81ImpossibleHugSound
    {
        const int Rate = 22050;
        static readonly Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>();
        static readonly System.Random rnd = new System.Random(381);

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
                case "hmph":    // "hmph" nasale: tono basso che scende, col soffio
                    {
                        float lp = 0f;
                        c = Make(kind, 0.32f, t =>
                        {
                            lp += (N() - lp) * 0.2f;
                            float f = Mathf.Lerp(210f, 140f, t / 0.32f);
                            float tone = Mathf.Sin(t * 2f * Mathf.PI * f) + 0.4f * Mathf.Sin(t * 4f * Mathf.PI * f);
                            return (tone * 0.22f + lp * 0.25f) * Mathf.Min(1f, t * 60f) * Mathf.Exp(-t * 7f);
                        });
                        break;
                    }
                case "huff":    // sbuffo offeso
                    {
                        float lp = 0f;
                        c = Make(kind, 0.35f, t =>
                        {
                            lp += (N() - lp) * Mathf.Lerp(0.6f, 0.15f, t / 0.35f);
                            return lp * 0.5f * Mathf.Sin(t / 0.35f * Mathf.PI);
                        });
                        break;
                    }
                case "scuff":   // passetto sul prato
                    c = Make(kind, 0.06f, t => N() * 0.3f * Mathf.Exp(-t * 60f));
                    break;
                case "grab":    // "plip"
                    c = Make(kind, 0.08f, t => Mathf.Sin(t * 2f * Mathf.PI * Mathf.Lerp(600f, 1000f, t / 0.08f)) * 0.4f * Mathf.Exp(-t * 30f));
                    break;
                case "boop":    // tutti e due presi: "bu-bup" interrogativo che sale
                    c = Make(kind, 0.22f, t =>
                    {
                        float f = t < 0.1f ? 420f : 640f;
                        return Mathf.Sin(t * 2f * Mathf.PI * f) * 0.3f * Mathf.Exp(-(t % 0.11f) * 25f);
                    });
                    break;
                case "pop":     // l'abbraccio: schiocco morbido
                    c = Make(kind, 0.12f, t => N() * 0.4f * Mathf.Exp(-t * 60f) + Mathf.Sin(t * 2f * Mathf.PI * 700f) * 0.3f * Mathf.Exp(-t * 30f));
                    break;
                case "chime":   // il cuore si riunisce: arpeggio luminoso
                    c = Make(kind, 0.7f, t =>
                    {
                        float[] notes = { 784f, 988f, 1175f, 1568f };
                        float v = 0f;
                        for (int k = 0; k < notes.Length; k++)
                        {
                            float s = t - k * 0.08f;
                            if (s < 0f) continue;
                            v += Mathf.Sin(s * 2f * Mathf.PI * notes[k]) * Mathf.Exp(-s * 6f) * 0.18f;
                        }
                        return v;
                    });
                    break;
                case "aww":     // "aww" tenero: vocale che scende con vibrato
                    c = Make(kind, 0.6f, t =>
                    {
                        float f = Mathf.Lerp(520f, 380f, t / 0.6f) * (1f + 0.02f * Mathf.Sin(t * 2f * Mathf.PI * 6f));
                        float v = Mathf.Sin(t * 2f * Mathf.PI * f) + 0.3f * Mathf.Sin(t * 4f * Mathf.PI * f);
                        return v * 0.2f * Mathf.Min(1f, t * 20f) * Mathf.Clamp01((0.6f - t) * 5f);
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
            AudioClip clip = AudioClip.Create("v81b_" + name, n, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
