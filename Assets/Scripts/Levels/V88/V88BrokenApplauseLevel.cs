using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels.V64;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 138 della roadmap "The Broken Applause" (id broken_applause) — "GET A ROUND OF APPLAUSE." Un palco vuoto, due
    /// grandi mani a guanto da cartone animato ai lati dello schermo e, sotto, due file di spettatori annoiati che
    /// sbadigliano sulle poltroncine rosse.
    /// Inganno: una mano toccata da sola fa solo un debole "pat."; trascinata con un dito solo fa resistenza (avanza appena,
    /// trema e si piega all'indietro) e al rilascio torna a posto con una molla ("Boing."); toccare il pubblico lo fa solo
    /// sbadigliare ("*yaaawn*", "Is it over yet?"). Tutto reversibile, mai sconfitta.
    /// Soluzione: due dita, una per mano (in qualunque ordine), e portarle a incontrarsi: tenute tutte e due seguono le
    /// dita, da vicine fremono e il pubblico alza la testa incuriosito; quando i palmi si toccano (alla stessa altezza),
    /// un solo battito ("CLAP!", stella dell'impatto). Se un dito si alza, quella mano torna a posto. Lo stesso dito non
    /// può tenerle tutte e due.
    /// Vittoria: dopo il battito le mani continuano ad applaudire da sole, uno spettatore le imita, poi tutti, e l'applauso
    /// cresce fino a una standing ovation esagerata (tutti in piedi, "BRAVO!", le mani fanno l'inchino); LevelReward
    /// "THE CROWD LOVES YOU!" (coriandoli) -> Win una volta sola. Il rumore della folla è su una sorgente audio dedicata
    /// che si ferma quando il livello si spegne, ricomincia o finisce.
    /// Dita: al massimo due (V64AFingers). Pausa, app in background o livello spento lasciano andare tutto.
    /// La logica è in Gotcha.Levels.V64.BrokenApplauseLogic (coordinate locali rispetto al punto in mezzo alle mani).
    /// Tasti di prova nell'Editor: C soluzione a due dita (le mani si incontrano), O un dito trascina la mano sinistra e
    /// lascia, I un dito trascina la destra e lascia, L due dita vicine e poi un dito si alza, Y lo stesso dito prova a
    /// prenderle tutte e due, T tocco sulla sinistra, K tocco sulla destra, A tocco sul pubblico, P pausa simulata (dita
    /// perse), R ricomincia. Shift + clic pianta un secondo dito fermo (V64AFingers): tiene ferma una mano mentre il mouse
    /// trascina l'altra.
    /// </summary>
    public class V88BrokenApplauseLevel : LevelController
    {
        [Header("Le due mani: 0 = sinistra, 1 = destra")]
        [SerializeField] Transform[] hands;        // radici posate dal livello (collider generoso)
        [SerializeField] Transform[] handLooks;    // figli per gli effetti (la destra è rovesciata in x)
        [SerializeField] Collider2D[] handAreas;
        [SerializeField] Transform impact;         // stella del battito (nascosta)
        [SerializeField] SpriteRenderer clapGlow;
        [Header("Il pubblico")]
        [SerializeField] Transform[] fans;         // radici (sedute)
        [SerializeField] Transform[] facesBored;
        [SerializeField] Transform[] facesYawn;
        [SerializeField] Transform[] facesLook;
        [SerializeField] Transform[] facesHappy;
        [SerializeField] Transform[] fanClaps;     // manine che applaudono (nascoste)
        [SerializeField] Collider2D audienceArea;
        [Header("Scritte e festa")]
        [SerializeField] SpeechBubble fanBubble;   // si sposta sopra lo spettatore che parla
        [SerializeField] TextMesh popWord;         // "pat." / "CLAP!" vicino alle mani
        [SerializeField] TextMesh infoText;
        [SerializeField] Transform infoHolder;
        [SerializeField] LevelReward reward;

        const string Compliment = "THE CROWD LOVES YOU!";
        const int L = BrokenApplauseLogic.Left, R = BrokenApplauseLogic.Right;
        const float Follow = 22f, Spring = 60f, Damping = 7f;
        const float MeetGap = 0.84f, StandUp = 0.32f, StandScale = 1.08f, YawnTime = 1.3f;

        static readonly string[] PatLines = { "That's not applause. That's a pat.", "pat. ...the crowd is not impressed.", "Half a clap is just a pat." };
        static readonly string[] BackLines = { "Boing. Right back where it was.", "It won't go over there alone.", "A hand on its own just waves.", "It needs a partner." };
        static readonly string[] SplitLines = { "So close! Don't let go.", "Almost a clap...", "One let go. The moment is gone." };
        static readonly string[] YawnLines = { "*yaaawn*", "Is it over yet?", "Zzz...", "Wake me when it's over.", "I paid for this?" };
        static readonly string[] OvationLines = { "BRAVO!", "ENCORE!" };

        enum Face { Bored, Yawn, Look, Happy }

        sealed class FxSlot
        {
            public Coroutine co;
            public Vector3 v;
            public Quaternion q;
        }

        BrokenApplauseLogic logic;
        Camera cam;
        HwToast toast;
        HwSay say;
        AudioSource crowd;   // il rumore della folla (in loop): sorgente dedicata, sempre fermata all'uscita
        readonly Dictionary<int, Vector2> starts = new Dictionary<int, Vector2>();
        readonly HashSet<int> spoke = new HashSet<int>();
        readonly HashSet<int> seen = new HashSet<int>();
        readonly Dictionary<Transform, FxSlot>[] fxSlots = { new Dictionary<Transform, FxSlot>(), new Dictionary<Transform, FxSlot>() };
        readonly Vector2[] vis = new Vector2[2], vel = new Vector2[2];
        Vector3[] handScale, fanPos, fanScale;
        float[] yawnT, nextYawn;
        Vector3 centre, bubblePos, impactScale, popScale;
        Coroutine popCo;
        int patLines, backLines, splitLines, yawnLines;
        float lastShake = -10f;
        bool captured, finishing;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            StopAllCoroutines();
            StopFx();
            if (!captured)
            {
                captured = true;
                logic = new BrokenApplauseLogic();
                if (fans == null) fans = new Transform[0];
                centre = (hands[L].position + hands[R].position) * 0.5f;
                handScale = new[] { hands[L].localScale, hands[R].localScale };
                int n = fans != null ? fans.Length : 0;
                fanPos = new Vector3[n];
                fanScale = new Vector3[n];
                yawnT = new float[n];
                nextYawn = new float[n];
                for (int i = 0; i < n; i++) if (fans[i] != null) { fanPos[i] = fans[i].localPosition; fanScale[i] = fans[i].localScale; }
                if (fanBubble != null) bubblePos = fanBubble.transform.position;
                if (impact != null) impactScale = impact.localScale;
                if (popWord != null) popScale = popWord.transform.localScale;
                var go = new GameObject("CrowdAudio");
                go.transform.SetParent(transform, false);
                crowd = go.AddComponent<AudioSource>();
                crowd.playOnAwake = false;
                crowd.loop = true;
            }
            StopCrowd();
            logic.Reset();
            if (reward != null) reward.ResetReward();
            V64AFingers.Reset();
            starts.Clear();
            spoke.Clear();
            finishing = false;
            popCo = null;
            patLines = backLines = splitLines = yawnLines = 0;
            if (fanBubble != null) fanBubble.transform.position = bubblePos;
            say = new HwSay(fanBubble);
            toast = new HwToast(infoText, infoHolder);
            RestPose();
        }

        // ---------- Pose (sempre ricavate dalla logica: uno stato valido dopo pausa/spegnimento) ----------

        void RestPose()
        {
            say?.Hide();
            for (int i = 0; i < 2; i++)
            {
                vis[i] = new Vector2(logic.X(i), logic.Y(i));
                vel[i] = Vector2.zero;
                hands[i].position = Pos(i, vis[i]);
                hands[i].localRotation = Quaternion.identity;
                hands[i].localScale = handScale[i];
            }
            HalloweenKit.Show(impact, false);
            if (impact != null) impact.localScale = impactScale;
            if (clapGlow != null) HalloweenKit.SetAlpha(clapGlow, 0f);
            HidePop();
            RestFans();
        }

        void RestFans()
        {
            if (fans == null) return;
            for (int i = 0; i < fans.Length; i++)
            {
                if (fans[i] == null) continue;
                fans[i].localPosition = fanPos[i];
                fans[i].localScale = fanScale[i];
                fans[i].localRotation = Quaternion.identity;
                yawnT[i] = 0f;
                nextYawn[i] = Time.time + 1.5f + 1.7f * i % 6f;
                Faces(i, Face.Bored);
                HalloweenKit.Show(Pick(fanClaps, i), false);
            }
        }

        static Transform Pick(Transform[] a, int i) => a != null && i >= 0 && i < a.Length ? a[i] : null;

        Vector3 Pos(int i, Vector2 local) => new Vector3(centre.x + local.x, centre.y + local.y, hands[i].position.z);

        void Faces(int i, Face f)
        {
            HalloweenKit.Show(Pick(facesBored, i), f == Face.Bored);
            HalloweenKit.Show(Pick(facesYawn, i), f == Face.Yawn);
            HalloweenKit.Show(Pick(facesLook, i), f == Face.Look);
            HalloweenKit.Show(Pick(facesHappy, i), f == Face.Happy);
        }

        void HidePop()
        {
            if (popCo != null) { StopCoroutine(popCo); popCo = null; }
            if (popWord != null) { popWord.transform.localScale = popScale; HalloweenKit.Show(popWord, false); }
        }

        // ---------- Ciclo di vita ----------

        void OnDisable()
        {
            // pausa/pubblicità/livello spento: si molla tutto, la folla tace, si torna a uno stato valido
            CancelAll();
            StopFx();
            StopCrowd();
            if (captured && !finishing && !IsFinished && logic != null) RestPose();
        }

        void OnDestroy() { StopCrowd(); }

        void OnEnable()
        {
            if (!captured || IsFinished || logic == null) return;
            // spento a metà dell'ovazione: la si rifà da capo (la vittoria resta una sola)
            if (logic.Stage == BrokenApplauseStage.Ovation) { finishing = false; StartCoroutine(Finale()); }
        }

        void OnApplicationPause(bool paused) { if (paused) CancelAll(); }
        void OnApplicationFocus(bool focus) { if (!focus) CancelAll(); }

        /// <summary>Le dita spariscono: le mani tornano a posto (Pose le fa scivolare), niente battute.</summary>
        void CancelAll()
        {
            starts.Clear();
            spoke.Clear();
            V64AFingers.Reset();
            if (logic != null && !logic.InputLocked) logic.Cancel();
        }

        void StopCrowd()
        {
            if (crowd == null) return;
            crowd.Stop();
            crowd.clip = null;
        }

        void Update()
        {
            toast?.Tick();
            say?.Tick();
            if (crowd != null && crowd.clip != null)
            {
                // menu di pausa a metà ovazione: la folla si zittisce e riprende con il gioco
                if (Time.timeScale <= 0f) { if (crowd.isPlaying) crowd.Pause(); }
                else if (!crowd.isPlaying) crowd.UnPause();
            }
            if (IsFinished || toast == null || logic == null) return;
            if (finishing) return;
            if (Time.timeScale <= 0f) { CancelAll(); return; }   // pausa o pubblicità: niente input
#if UNITY_EDITOR
            EditorKeys();
#endif
            if (!finishing) ReadFingers();
            if (!finishing) { Pose(); PoseFans(); }
        }

        /// <summary>
        /// Le mani: tenute seguono la logica da vicino, lasciate tornano con una molla (rimbalzano un po' oltre). Da sola,
        /// trascinata, trema e si piega all'indietro; tutte e due tenute si sporgono l'una verso l'altra e, vicine, fremono.
        /// </summary>
        void Pose()
        {
            float dt = Mathf.Min(Time.deltaTime, 0.05f);
            bool together = logic.Together, near = logic.Near;
            for (int i = 0; i < 2; i++)
            {
                Vector2 target = new Vector2(logic.X(i), logic.Y(i));
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
                Vector2 shown = vis[i];
                shown.x = Mathf.Clamp(shown.x, -BrokenApplauseLogic.MaxX - 0.1f, BrokenApplauseLogic.MaxX + 0.1f);
                shown.y = Mathf.Clamp(shown.y, BrokenApplauseLogic.MinY, BrokenApplauseLogic.MaxY);
                float inDir = i == L ? 1f : -1f;   // verso l'altra mano
                bool resisting = logic.IsHeld(i) && !together;
                if (resisting) shown.x += 0.03f * Mathf.Sin(Time.time * 55f + i);          // trema: non vuole andare
                else if (near) shown.y += 0.025f * Mathf.Sin(Time.time * 40f + i * 2f);    // freme: ci siamo quasi
                hands[i].position = Pos(i, shown);

                float lean;
                if (resisting) lean = inDir * 14f;                                  // si piega all'indietro (verso l'esterno)
                else if (together) lean = -inDir * (near ? 9f : 5f);                // si sporge verso l'altra
                else lean = Mathf.Clamp(vel[i].x * 3f, -12f, 12f) + 2.5f * Mathf.Sin(Time.time * 1.6f + i * 3f);   // ondeggia piano
                hands[i].localRotation = Quaternion.Slerp(hands[i].localRotation, Quaternion.Euler(0f, 0f, lean), 1f - Mathf.Exp(-dt * 12f));
                if (resisting && Time.time - lastShake > 0.3f) { lastShake = Time.time; V88BrokenApplauseSound.Play(this, "creak", 0.18f); }
            }
        }

        /// <summary>Il pubblico seduto: ogni tanto qualcuno sbadiglia da solo; con le mani vicine tutti alzano la testa.</summary>
        void PoseFans()
        {
            if (fans == null) return;
            float dt = Time.deltaTime;
            bool near = logic.Near;
            for (int i = 0; i < fans.Length; i++)
            {
                if (fans[i] == null) continue;
                if (yawnT[i] > 0f) yawnT[i] = Mathf.Max(0f, yawnT[i] - dt);
                else if (!near && Time.time >= nextYawn[i])
                {
                    yawnT[i] = YawnTime;
                    nextYawn[i] = Time.time + 4f + (i * 2.3f) % 5f;
                }
                Faces(i, yawnT[i] > 0f ? Face.Yawn : near ? Face.Look : Face.Bored);
                float stretch = yawnT[i] > 0f ? Mathf.Sin((1f - yawnT[i] / YawnTime) * Mathf.PI) : 0f;
                fans[i].localPosition = fanPos[i] + new Vector3(0f, (near ? 0.06f : 0f) + 0.05f * stretch, 0f);
                fans[i].localScale = new Vector3(fanScale[i].x * (1f - 0.03f * stretch), fanScale[i].y * (1f + 0.06f * stretch), fanScale[i].z);
                fans[i].localRotation = Quaternion.Euler(0f, 0f, near ? 0f : 4f * stretch * (i % 2 == 0 ? 1f : -1f));
            }
        }

        // ---------- Effetti che non si accavallano (come nei livelli 0.73-0.86) ----------

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

        /// <summary>Una parolina che salta fuori vicino alle mani ("pat.", "CLAP!") e sparisce.</summary>
        void Pop(string word, Vector3 at, float seconds)
        {
            if (popWord == null || !isActiveAndEnabled) return;
            HidePop();
            popWord.text = word;
            popWord.transform.position = new Vector3(at.x, at.y, popWord.transform.position.z);
            HalloweenKit.Show(popWord, true);
            popCo = StartCoroutine(PopRun(seconds));
        }

        IEnumerator PopRun(float seconds)
        {
            Transform t = popWord.transform;
            Color c = popWord.color;
            for (float e = 0f; e < seconds; e += Time.deltaTime)
            {
                float k = e / seconds;
                t.localScale = popScale * (k < 0.15f ? Mathf.Lerp(0.3f, 1.15f, k / 0.15f) : Mathf.Lerp(1.15f, 1f, (k - 0.15f) / 0.85f));
                t.position += new Vector3(0f, 0.25f * Time.deltaTime, 0f);
                popWord.color = new Color(c.r, c.g, c.b, k < 0.7f ? 1f : 1f - (k - 0.7f) / 0.3f);
                yield return null;
            }
            popWord.color = new Color(c.r, c.g, c.b, 1f);
            t.localScale = popScale;
            HalloweenKit.Show(popWord, false);
            popCo = null;
        }

        // ---------- Dita (al massimo due, una mano per dito) ----------

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
                if (h != BrokenApplauseLogic.NoFinger && !seen.Contains(h)) EndFinger(h);
            }
        }

        static bool Over(Collider2D c, Vector2 p) => c != null && c.gameObject.activeInHierarchy && c.OverlapPoint(p);

        /// <summary>Il dito prende la mano libera più vicina sotto di sé (mai quella già tenuta, mai tutte e due); sul pubblico: sbadiglio.</summary>
        void BeginFinger(int id, Vector2 w)
        {
            if (finishing || logic.InputLocked) return;
            int who = -1;
            float best = float.MaxValue;
            for (int i = 0; i < 2; i++)
            {
                if (logic.IsHeld(i) || !Over(handAreas != null && i < handAreas.Length ? handAreas[i] : null, w)) continue;
                float d = Vector2.Distance(w, hands[i].position);
                if (d < best) { best = d; who = i; }
            }
            if (who < 0)
            {
                bool overHand = false;
                for (int i = 0; i < 2; i++) overHand |= Over(handAreas != null && i < handAreas.Length ? handAreas[i] : null, w);
                if (!overHand && Over(audienceArea, w)) TapAudience(w);
                return;
            }
            Vector2 p = Local(w);
            if (!logic.Grab(id, who, p.x, p.y)) return;
            starts[id] = w;
            spoke.Remove(id);
            Haptics.Tap();
            if (logic.Together)
            {
                // tutte e due prese: un fremito, il pubblico apre un occhio
                V88BrokenApplauseSound.Play(this, "boop", 0.35f);
                for (int i = 0; i < 2; i++) FxPop(Pick(handLooks, i), 1.06f, 0.25f);
            }
            else V88BrokenApplauseSound.Play(this, "grab", 0.3f);
        }

        void MoveFinger(int id, Vector2 w)
        {
            Vector2 p = Local(w);
            BrokenApplauseMove m = logic.Move(id, p.x, p.y);
            if (m == BrokenApplauseMove.Clap) { StartCoroutine(Finale()); return; }
            if (m != BrokenApplauseMove.Resisting || spoke.Contains(id)) return;
            if (!starts.TryGetValue(id, out Vector2 s) || Vector2.Distance(s, w) <= BrokenApplauseLogic.TapSlop) return;
            // la prima volta che la si trascina da sola: fa resistenza
            spoke.Add(id);
            int who = logic.HeldBy(id);
            if (who < 0) return;
            FxWiggle(Pick(handLooks, who), 8f, 0.4f, 30f);
            V88BrokenApplauseSound.Play(this, "creak", 0.35f);
            Haptics.Light();
        }

        void EndFinger(int id)
        {
            int who = logic.HeldBy(id);
            starts.Remove(id);
            spoke.Remove(id);
            BrokenApplauseDrop drop = logic.Release(id);
            switch (drop)
            {
                case BrokenApplauseDrop.Pat: Pat(who); break;
                case BrokenApplauseDrop.SprangBack: SprangBack(who); break;
                case BrokenApplauseDrop.Separated: Separated(); break;
            }
        }

        // ---------- Le false piste ----------

        Vector3 PalmSide(int who) => hands[who].position + new Vector3(who == L ? 1.0f : -1.0f, 0.1f, 0f);

        void Pat(int who)
        {
            if (who < 0) return;
            FxPop(Pick(handLooks, who), 0.92f, 0.22f);
            V88BrokenApplauseSound.Play(this, "pat", 0.35f);
            Pop("pat.", PalmSide(who), 0.8f);
            toast.Show(PatLines[patLines++ % PatLines.Length], 1.7f);
        }

        void SprangBack(int who)
        {
            if (who >= 0) FxWiggle(Pick(handLooks, who), 10f, 0.45f, 22f);
            V88BrokenApplauseSound.Play(this, "boing", 0.4f);
            Haptics.Light();
            toast.Show(BackLines[backLines++ % BackLines.Length], 1.7f);
        }

        void Separated()
        {
            V88BrokenApplauseSound.Play(this, "boing", 0.35f);
            toast.Show(SplitLines[splitLines++ % SplitLines.Length], 1.7f);
        }

        void TapAudience(Vector2 w)
        {
            int n = logic.TapAudience();
            if (n < 0 || fans == null || fans.Length == 0) return;
            int who = -1;
            float best = float.MaxValue;
            for (int i = 0; i < fans.Length; i++)
            {
                if (fans[i] == null) continue;
                float d = Vector2.Distance(w, fans[i].position);
                if (d < best) { best = d; who = i; }
            }
            if (who < 0) return;
            yawnT[who] = YawnTime;
            nextYawn[who] = Time.time + 5f;
            V88BrokenApplauseSound.Play(this, "yawn", 0.45f);
            if (fanBubble != null)
            {
                fanBubble.transform.position = fans[who].position + new Vector3(0.05f, DoodleHeadTop, 0f);
                say.Show(YawnLines[yawnLines++ % YawnLines.Length], 1.6f);
            }
        }

        /// <summary>Altezza della punta del fumetto sopra il centro della testa di uno spettatore.</summary>
        const float DoodleHeadTop = 0.45f;

        // ---------- La soluzione ----------

        /// <summary>
        /// Il battito e l'ovazione: le mani si chiudono di colpo ("CLAP!", stella, bagliore), poi continuano ad applaudire
        /// da sole; uno spettatore le imita, poi tutti uno dopo l'altro; tutti in piedi, "BRAVO!", le mani fanno l'inchino;
        /// festa comune con i coriandoli; la folla tace; Win una volta sola. Rifatta da capo se il livello si spegne a metà.
        /// </summary>
        IEnumerator Finale()
        {
            if (finishing) yield break;
            finishing = true;
            starts.Clear();
            spoke.Clear();
            V64AFingers.Reset();
            StopFx();
            StopCrowd();
            HalloweenKit.Show(infoHolder, false);
            say.Hide();
            HidePop();
            RestFans();
            for (int i = 0; i < fans.Length; i++) Faces(i, Face.Look);

            // 1) un battito secco: le mani si chiudono dove si sono incontrate
            Vector3[] from = { hands[L].position, hands[R].position };
            Vector3 mid = (from[L] + from[R]) * 0.5f;
            mid.x = Mathf.Clamp(mid.x, -1.2f, 1.2f);
            Vector3[] meet = { mid + new Vector3(-MeetGap * 0.5f, 0f, 0f), mid + new Vector3(MeetGap * 0.5f, 0f, 0f) };
            meet[L].z = from[L].z;
            meet[R].z = from[R].z;
            for (float t = 0f; t < 0.08f; t += Time.deltaTime)
            {
                float k = Mathf.Clamp01(t / 0.08f);
                for (int i = 0; i < 2; i++)
                {
                    hands[i].position = Vector3.Lerp(from[i], meet[i], k * k);
                    hands[i].localRotation = Quaternion.Slerp(hands[i].localRotation, Quaternion.identity, k);
                }
                yield return null;
            }
            for (int i = 0; i < 2; i++) { hands[i].position = meet[i]; hands[i].localRotation = Quaternion.identity; }
            ClapFx(mid, 0.9f, true);
            Pop("CLAP!", mid + new Vector3(0f, 1.25f, 0f), 0.9f);
            Haptics.Medium();
            yield return new WaitForSeconds(0.35f);

            // 2) le mani applaudono da sole; il pubblico le imita, uno dopo l'altro
            int[] order = { 6, 2, 5, 7, 1, 3, 0, 8, 4 };
            int joined = 0;
            float[] clapPhase = new float[fans.Length];
            for (int beat = 0; beat < 5; beat++)
            {
                yield return HandsBeat(mid, 0.14f);
                ClapFx(mid, 0.55f, false);
                for (int j = 0; j < 2 && joined < order.Length; j++)
                {
                    int f = order[joined++];
                    if (f >= fans.Length || fans[f] == null) continue;
                    Faces(f, Face.Happy);
                    HalloweenKit.Show(Pick(fanClaps, f), true);
                    FxPop(fans[f], 1.12f, 0.2f);
                    clapPhase[f] = beat * 0.37f;
                }
                if (beat == 1)
                {
                    crowd.clip = V88BrokenApplauseSound.Crowd();
                    crowd.volume = 0.15f;
                    crowd.Play();
                }
                if (crowd.clip != null) crowd.volume = Mathf.Min(0.75f, crowd.volume + 0.15f);
            }
            for (int f = 0; f < fans.Length; f++) { Faces(f, Face.Happy); HalloweenKit.Show(Pick(fanClaps, f), true); }

            // 3) la standing ovation: tutti in piedi, "BRAVO!", le mani fanno l'inchino
            StartCoroutine(FansCheer(clapPhase));
            V88BrokenApplauseSound.Play(this, "whoo", 0.5f);
            if (fanBubble != null && fans.Length > 1 && fans[1] != null)
            {
                fanBubble.transform.position = fans[1].position + new Vector3(0.05f, DoodleHeadTop + StandUp, 0f);
                say.Show(OvationLines[0], 2.2f);
            }
            if (crowd.clip != null) crowd.volume = 0.85f;
            Vector3[] bowFrom = { hands[L].position, hands[R].position };
            for (float t = 0f; t < 0.5f; t += Time.deltaTime)
            {
                // l'inchino: le mani si abbassano e si piegano verso l'esterno, poi si rialzano
                float k = Mathf.Sin(Mathf.Clamp01(t / 0.5f) * Mathf.PI);
                for (int i = 0; i < 2; i++)
                {
                    float sx = i == L ? -1f : 1f;
                    hands[i].position = bowFrom[i] + new Vector3(sx * 0.12f * k, -0.18f * k, 0f);
                    hands[i].localRotation = Quaternion.Euler(0f, 0f, -sx * 22f * k);
                }
                yield return null;
            }
            for (int i = 0; i < 2; i++) { hands[i].position = bowFrom[i]; hands[i].localRotation = Quaternion.identity; }

            if (reward != null) yield return reward.Play(Compliment);
            else yield return new WaitForSeconds(1.5f);
            // la folla si spegne prima del risultato
            for (float t = 0f; t < 0.25f && crowd.clip != null; t += Time.deltaTime)
            {
                crowd.volume = Mathf.Lerp(0.85f, 0f, t / 0.25f);
                yield return null;
            }
            StopCrowd();
            say.Hide();
            if (logic.TryComplete()) Win(Compliment);
        }

        /// <summary>Le mani si aprono e si richiudono una volta (un battito da sole).</summary>
        IEnumerator HandsBeat(Vector3 mid, float half)
        {
            for (float t = 0f; t < half * 2f; t += Time.deltaTime)
            {
                float k = Mathf.Sin(Mathf.Clamp01(t / (half * 2f)) * Mathf.PI);
                for (int i = 0; i < 2; i++)
                {
                    float sx = i == L ? -1f : 1f;
                    hands[i].position = new Vector3(mid.x + sx * (MeetGap * 0.5f + 0.35f * k), mid.y, hands[i].position.z);
                    hands[i].localRotation = Quaternion.Euler(0f, 0f, sx * 10f * k);
                }
                yield return null;
            }
            for (int i = 0; i < 2; i++)
            {
                hands[i].position = new Vector3(mid.x + (i == L ? -1f : 1f) * MeetGap * 0.5f, mid.y, hands[i].position.z);
                hands[i].localRotation = Quaternion.identity;
            }
        }

        /// <summary>Il battito: stella dell'impatto, bagliore, schiacciata delle mani e il suono (forte o leggero).</summary>
        void ClapFx(Vector3 at, float volume, bool big)
        {
            V88BrokenApplauseSound.Play(this, "clap", volume);
            for (int i = 0; i < 2; i++) FxPop(Pick(handLooks, i), big ? 0.88f : 0.94f, 0.16f);
            if (impact != null)
            {
                impact.position = new Vector3(at.x, at.y + 0.35f, impact.position.z);
                impact.localScale = impactScale * (big ? 1f : 0.6f);
                HalloweenKit.Show(impact, true);
                FxPop(impact, 1.35f, 0.18f);
                StartCoroutine(HideLater(impact, 0.2f));
            }
            if (clapGlow != null)
            {
                clapGlow.transform.position = new Vector3(at.x, at.y + 0.35f, clapGlow.transform.position.z);
                HalloweenKit.SetAlpha(clapGlow, big ? 0.9f : 0.5f);
                StartCoroutine(HalloweenKit.Fade(clapGlow, 0f, 0.35f));
            }
        }

        static IEnumerator HideLater(Transform t, float seconds)
        {
            yield return new WaitForSeconds(seconds);
            HalloweenKit.Show(t, false);
        }

        /// <summary>
        /// Il pubblico in piedi che applaude (per tutta la festa): si alzano uno dopo l'altro, saltellano, le manine
        /// battono a tempo; le grandi mani continuano ad applaudire piano.
        /// </summary>
        IEnumerator FansCheer(float[] phase)
        {
            int n = fans.Length;
            for (float t = 0f; t < 6f; t += Time.deltaTime)
            {
                for (int i = 0; i < n; i++)
                {
                    if (fans[i] == null) continue;
                    float rise = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((t - i * 0.04f) / 0.3f));
                    float hop = Mathf.Abs(Mathf.Sin((t + phase[i]) * 9f)) * 0.06f * rise;
                    fans[i].localPosition = fanPos[i] + new Vector3(0f, StandUp * rise + hop, 0f);
                    fans[i].localScale = fanScale[i] * Mathf.Lerp(1f, StandScale, rise);
                    fans[i].localRotation = Quaternion.Euler(0f, 0f, 5f * Mathf.Sin((t + phase[i]) * 6f));
                    Transform c = Pick(fanClaps, i);
                    if (c != null) c.localScale = new Vector3(1f + 0.12f * Mathf.Sin((t + phase[i]) * 30f), 1f, 1f);
                }
                yield return null;
            }
        }

#if UNITY_EDITOR
        // ---------- Tasti di prova nell'Editor ----------

        const int KeyA = 901, KeyB = 902;

        void EditorKeys()
        {
            if (logic.InputLocked || logic.HeldCount > 0) return;
            if (Input.GetKeyDown(KeyCode.C)) KeyTogether(0.3f, true);
            if (Input.GetKeyDown(KeyCode.L)) KeyTogether(0.75f, false);
            if (Input.GetKeyDown(KeyCode.O)) KeySolo(L, KeyA);
            if (Input.GetKeyDown(KeyCode.I)) KeySolo(R, KeyA);
            if (Input.GetKeyDown(KeyCode.Y))
            {
                // lo stesso dito sulla sinistra e poi sulla destra: la destra non viene presa
                if (logic.Grab(KeyA, L, -BrokenApplauseLogic.HomeX, 0f)) logic.Grab(KeyA, R, BrokenApplauseLogic.HomeX, 0f);
                KeyDrag(KeyA, L);
            }
            if (Input.GetKeyDown(KeyCode.T)) { if (logic.Grab(KeyA, L, logic.X(L), 0f)) EndFinger(KeyA); }
            if (Input.GetKeyDown(KeyCode.K)) { if (logic.Grab(KeyA, R, logic.X(R), 0f)) EndFinger(KeyA); }
            if (Input.GetKeyDown(KeyCode.A) && fans != null && fans.Length > 0 && fans[0] != null) TapAudience(fans[Random.Range(0, fans.Length)].position);
            if (Input.GetKeyDown(KeyCode.P)) CancelAll();
            if (Input.GetKeyDown(KeyCode.R)) OnLevelStart();
        }

        /// <summary>Due dita finte prendono le mani e le portano a ±x dal centro; senza battito una delle due si alza.</summary>
        void KeyTogether(float x, bool clap)
        {
            if (!logic.Grab(KeyA, L, logic.X(L), 0f) || !logic.Grab(KeyB, R, logic.X(R), 0f)) { logic.Cancel(); return; }
            float l0 = logic.X(L), r0 = logic.X(R);
            for (int s = 1; s <= 10; s++)
            {
                float k = s / 10f;
                if (logic.Move(KeyA, Mathf.Lerp(l0, -x, k), 0f) == BrokenApplauseMove.Clap || logic.Move(KeyB, Mathf.Lerp(r0, x, k), 0f) == BrokenApplauseMove.Clap)
                {
                    StartCoroutine(Finale());
                    return;
                }
            }
            if (clap) return;
            EndFinger(KeyB);   // un dito si alza: la destra torna a posto
            logic.Cancel();    // e il dito finto della sinistra sparisce senza contare
        }

        void KeySolo(int who, int finger)
        {
            if (!logic.Grab(finger, who, logic.X(who), 0f)) return;
            KeyDrag(finger, who);
        }

        void KeyDrag(int finger, int who)
        {
            starts[finger] = Pos(who, new Vector2(logic.X(who), 0f));
            MoveFinger(finger, centre);   // trascinata fino al centro: fa resistenza
            vis[who] = new Vector2(logic.X(who), logic.Y(who));
            EndFinger(finger);
        }
#endif
    }

    /// <summary>
    /// Suoni sintetizzati di The Broken Applause: "pat" debole, battito secco, cigolio della resistenza, molla, presa,
    /// "boop", sbadiglio, "whoo" del pubblico e il rumore della folla che applaude (in loop, per la sorgente dedicata).
    /// </summary>
    public static class V88BrokenApplauseSound
    {
        const int Rate = 22050;
        static readonly Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>();
        static readonly System.Random rnd = new System.Random(388);

        public static void Play(Component owner, string kind, float volume = 0.6f)
        {
            if (owner == null) return;
            AudioClip c = Clip(kind);
            if (c == null) { Sfx.Play(owner, kind, volume); return; }
            AudioSource src = owner.GetComponent<AudioSource>();
            if (src == null) { src = owner.gameObject.AddComponent<AudioSource>(); src.playOnAwake = false; }
            src.PlayOneShot(c, volume);
        }

        /// <summary>Il rumore della folla che applaude: tanti battiti a caso sovrapposti, 2 s che si ripetono senza stacchi.</summary>
        public static AudioClip Crowd()
        {
            if (cache.TryGetValue("crowd", out AudioClip c) && c != null) return c;
            const float seconds = 2f;
            int n = (int)(seconds * Rate);
            var data = new float[n];
            var r = new System.Random(1388);
            int claps = 260;
            for (int k = 0; k < claps; k++)
            {
                int at = r.Next(n);
                float amp = 0.08f + 0.12f * (float)r.NextDouble();
                float decay = 250f + 250f * (float)r.NextDouble();
                int len = (int)(0.03f * Rate);
                float lp = 0f;
                for (int i = 0; i < len; i++)
                {
                    float t = i / (float)Rate;
                    lp += ((float)r.NextDouble() * 2f - 1f - lp) * 0.6f;
                    data[(at + i) % n] += lp * amp * Mathf.Exp(-t * decay);   // avvolto: il loop non ha giunture
                }
            }
            for (int i = 0; i < n; i++) data[i] = Mathf.Clamp(data[i], -1f, 1f);
            c = AudioClip.Create("v88a_crowd", n, 1, Rate, false);
            c.SetData(data, 0);
            cache["crowd"] = c;
            return c;
        }

        static float N() => (float)rnd.NextDouble() * 2f - 1f;

        static AudioClip Clip(string kind)
        {
            if (cache.TryGetValue(kind, out AudioClip c) && c != null) return c;
            switch (kind)
            {
                case "pat":     // "pat." morbido e smorzato: un colpetto ovattato
                    {
                        float lp = 0f;
                        c = Make(kind, 0.09f, t =>
                        {
                            lp += (N() - lp) * 0.08f;
                            return (lp * 0.9f + Mathf.Sin(t * 2f * Mathf.PI * 180f) * 0.15f) * Mathf.Exp(-t * 45f);
                        });
                        break;
                    }
                case "clap":    // battito secco: schiocco di rumore brillante con un corpo breve
                    {
                        float lp = 0f;
                        c = Make(kind, 0.16f, t =>
                        {
                            lp += (N() - lp) * 0.7f;
                            return lp * 0.85f * Mathf.Exp(-t * 38f) + Mathf.Sin(t * 2f * Mathf.PI * 260f) * 0.2f * Mathf.Exp(-t * 50f);
                        });
                        break;
                    }
                case "creak":   // cigolio della mano che fa resistenza
                    c = Make(kind, 0.2f, t =>
                    {
                        float f = 320f + 60f * Mathf.Sin(t * 2f * Mathf.PI * 18f);
                        float saw = (t * f) % 1f * 2f - 1f;
                        return saw * 0.18f * Mathf.Sin(t / 0.2f * Mathf.PI);
                    });
                    break;
                case "boing":   // la molla che la riporta a posto
                    c = Make(kind, 0.35f, t =>
                    {
                        float f = 220f + 160f * Mathf.Sin(t * 2f * Mathf.PI * 9f) * Mathf.Exp(-t * 6f);
                        return Mathf.Sin(t * 2f * Mathf.PI * f) * 0.3f * Mathf.Exp(-t * 7f);
                    });
                    break;
                case "grab":    // "plip"
                    c = Make(kind, 0.08f, t => Mathf.Sin(t * 2f * Mathf.PI * Mathf.Lerp(600f, 1000f, t / 0.08f)) * 0.4f * Mathf.Exp(-t * 30f));
                    break;
                case "boop":    // tutte e due prese: "bu-bup" che sale
                    c = Make(kind, 0.22f, t =>
                    {
                        float f = t < 0.1f ? 420f : 640f;
                        return Mathf.Sin(t * 2f * Mathf.PI * f) * 0.3f * Mathf.Exp(-(t % 0.11f) * 25f);
                    });
                    break;
                case "yawn":    // sbadiglio: vocale che sale e poi scende, col soffio
                    {
                        float lp = 0f;
                        c = Make(kind, 0.8f, t =>
                        {
                            lp += (N() - lp) * 0.1f;
                            float k = t / 0.8f;
                            float f = k < 0.35f ? Mathf.Lerp(180f, 260f, k / 0.35f) : Mathf.Lerp(260f, 120f, (k - 0.35f) / 0.65f);
                            float v = Mathf.Sin(t * 2f * Mathf.PI * f) + 0.35f * Mathf.Sin(t * 4f * Mathf.PI * f);
                            return (v * 0.16f + lp * 0.12f) * Mathf.Sin(k * Mathf.PI);
                        });
                        break;
                    }
                case "whoo":    // "whooo!" del pubblico: vocali che salgono, un po' stonate
                    c = Make(kind, 0.7f, t =>
                    {
                        float k = t / 0.7f, v = 0f;
                        float[] b = { 300f, 380f, 450f };
                        for (int i = 0; i < b.Length; i++)
                        {
                            float f = b[i] * (1f + 0.5f * k) * (1f + 0.015f * Mathf.Sin(t * 2f * Mathf.PI * (5f + i)));
                            v += Mathf.Sin(t * 2f * Mathf.PI * f) * 0.1f;
                        }
                        return v * Mathf.Min(1f, t * 12f) * Mathf.Clamp01((0.7f - t) * 4f);
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
            AudioClip clip = AudioClip.Create("v88a_" + name, n, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
