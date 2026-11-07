using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels.V64;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 119 della roadmap "The Fireless Dragon" (id fireless_dragon) — "LIGHT THE DRAGON'S FIRE." Un draghetto
    /// colorato seduto sul prato soffia soltanto bolle di sapone; accanto c'è un piccolo falò acceso con una fiammella
    /// sospesa sopra. Inganno: toccare il drago fa solo altre bolle ("Blub."), scuotere il telefono lo fa tossire (sbuffo
    /// di fumo grigio, "*cough*"), le bolle toccate scoppiano. Il microfono non serve (scelta voluta: niente permessi).
    /// Soluzione: trascinare la fiammella dal falò nella bocca del drago (zona generosa: basta entrarci trascinando, oppure
    /// lasciarla lì; vale anche il dito). La fiammella sparisce in bocca, il drago gonfia le guance e sputa una grande
    /// fiammata da cartone (sequenza scritta di scala e trasparenza, niente fisica né particelle costose, resta sotto la
    /// barra in alto e svanisce prima della festa), poi sorride: LevelReward "DRAGON POWER RESTORED!" e Win.
    /// La fiammella lasciata altrove si spegne (sbuffo grigio, "Pfft!") e dopo un attimo ne ricompare una nuova sul falò;
    /// lasciata sul corpo del drago lo scotta ("Ow!") e si spegne lo stesso; lasciata vicino al falò torna su. Tutto
    /// reversibile, mai una sconfitta. Logica in Gotcha.Levels.V64.FirelessDragonLogic.
    /// Tasti di prova nell'Editor: T tocca il drago, S scuote il telefono (Inputs.Shake), F porta la fiammella in bocca
    /// (vittoria), G lascia la fiammella sul prato (si spegne), H la lascia sulla pancia del drago, B scoppia una bolla,
    /// P pausa simulata (dito annullato, fiammella sul falò), R ricomincia il livello.
    /// </summary>
    public class V87FirelessDragonLevel : LevelController
    {
        [Header("Drago (radice ai piedi)")]
        [SerializeField] Transform dragonBody;      // perno ai piedi: respiro e schiacciamenti
        [SerializeField] Transform headIdle;
        [SerializeField] Transform headPuff;
        [SerializeField] Transform headRoar;
        [SerializeField] Transform headCough;
        [SerializeField] Transform headHappy;
        [SerializeField] Transform mouth;           // segnaposto della bocca (figlio della testa)
        [SerializeField] Collider2D dragonArea;
        [SerializeField] SpeechBubble dragonBubble;
        [Header("Falò e fiammella")]
        [SerializeField] Transform flame;           // fiammella trascinabile (radice al centro)
        [SerializeField] Transform flameArt;        // contenitore del disegno (tremolio senza toccare la scala della radice)
        [SerializeField] Collider2D flameArea;
        [SerializeField] SpriteRenderer flameSprite;
        [SerializeField] SpriteRenderer flameGlow;
        [SerializeField] Transform[] fireFlames;    // fiamme del falò (tremolano)
        [SerializeField] SpriteRenderer fireGlow;
        [Header("Effetti")]
        [SerializeField] SpriteRenderer[] soapBubbles;
        [SerializeField] SpriteRenderer[] smokePuffs;
        [SerializeField] SpriteRenderer fizzlePuff;
        [SerializeField] SpriteRenderer mouthGlow;
        [SerializeField] Transform burst;           // fiammata: perno alla bocca
        [SerializeField] SpriteRenderer burstSprite;
        [SerializeField] TextMesh infoText;
        [SerializeField] Transform infoHolder;
        [Header("Geometria")]
        [SerializeField] float mouthRadius = 0.7f;
        [SerializeField] float fireRadius = 0.6f;
        [SerializeField] LevelReward reward;

        const string Compliment = "DRAGON POWER RESTORED!";
        const string HintLine = "If only I had a spark...";
        static readonly string[] TapLines = { "Blub.", "More bubbles. Great.", "I'm a dragon, not a bathtub!", "Blub blub... sigh." };
        static readonly string[] CoughLines = { "*cough*", "*cough* *cough*", "*cough* Just smoke." };
        static readonly string[] SingeLines = { "Ow! Not on my belly!", "Hot! Wrong end!" };
        static readonly string[] FizzleLines = { "Pfft! It fizzled out.", "Sssss... gone. Here's another.", "Flames don't like the grass." };
        const float ShakeLevel = 12f, TapSlop = 0.18f, SayGap = 0.35f, BubbleEvery = 0.65f;

        enum Grip { None, Flame, Other }

        FirelessDragonLogic logic;
        Camera cam;
        HwDragger dragger;
        HwDragger.Item flameItem;
        HwToast toast;
        Vector3 flameHome, bodyBase, flameArtBase;
        Vector3[] fireBase;
        Grip grip;
        Vector2 gripStart, gripLast;
        bool gripMoved, captured, finishing;
        float sayHideAt = -1f, lastSayAt = -10f, coughT = -1f, tapT = -1f, respawnT = -1f, bubbleTimer, burstQueueT;
        int tapLines, coughLines, singeLines, fizzleLines, burstQueue;
        Vector3 fizzleAt;

        // bolle di sapone (insieme fisso di sprite, animazione scritta)
        Vector3[] bPos, bVel;
        float[] bAge, bLife, bSize, bPop;
        float[] puffAge;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            StopAllCoroutines();
            if (!captured)
            {
                captured = true;
                flameHome = flame.position;
                bodyBase = dragonBody.localScale;
                flameArtBase = flameArt != null ? flameArt.localScale : Vector3.one;
                fireBase = new Vector3[fireFlames != null ? fireFlames.Length : 0];
                for (int i = 0; i < fireBase.Length; i++) fireBase[i] = fireFlames[i] != null ? fireFlames[i].localScale : Vector3.one;
                Vector3 m = mouth.position;
                logic = new FirelessDragonLogic(m.x, m.y, mouthRadius, flameHome.x, flameHome.y, fireRadius);
                int n = soapBubbles != null ? soapBubbles.Length : 0;
                bPos = new Vector3[n]; bVel = new Vector3[n];
                bAge = new float[n]; bLife = new float[n]; bSize = new float[n]; bPop = new float[n];
                puffAge = new float[smokePuffs != null ? smokePuffs.Length : 0];
            }
            logic.Reset();
            finishing = false;
            grip = Grip.None;
            tapLines = coughLines = singeLines = fizzleLines = 0;
            lastSayAt = -10f;
            coughT = tapT = respawnT = -1f;
            bubbleTimer = 0.3f;
            burstQueue = 0;
            dragger = new HwDragger(this);
            flameItem = dragger.Add(flame, flameArea, 10);
            toast = new HwToast(infoText, infoHolder);
            if (reward != null) reward.ResetReward();
            RestFlame();
            RestDragon();
            ClearEffects();
            HideSay();
        }

        // ---------- Stati di riposo (sempre ricavati dalla logica: valido dopo pausa/spegnimento) ----------

        /// <summary>Fiammella ferma sul falò, intera e luminosa.</summary>
        void RestFlame()
        {
            if (dragger != null)
            {
                if (dragger.Held != null) dragger.Release();
                dragger.Stop(flameItem);
                flameItem.Enabled = true;
            }
            flame.gameObject.SetActive(true);
            flame.position = flameHome;
            flame.localScale = Vector3.one;
            if (flameSprite != null) flameSprite.color = Color.white;
            HalloweenKit.SetAlpha(flameGlow, 0.6f);
            HalloweenKit.Show(fizzlePuff, false);
            respawnT = -1f;
        }

        void RestDragon()
        {
            dragonBody.localScale = bodyBase;
            dragonBody.localPosition = Vector3.zero;
            ShowHead(headIdle);
            coughT = tapT = -1f;
        }

        void ClearEffects()
        {
            if (soapBubbles != null) for (int i = 0; i < soapBubbles.Length; i++) { bLife[i] = 0f; HalloweenKit.Show(soapBubbles[i], false); }
            if (smokePuffs != null) for (int i = 0; i < smokePuffs.Length; i++) { puffAge[i] = -1f; HalloweenKit.Show(smokePuffs[i], false); }
            HalloweenKit.Show(mouthGlow, false);
            if (burst != null) burst.gameObject.SetActive(false);
        }

        void ShowHead(Transform pose)
        {
            foreach (Transform t in new[] { headIdle, headPuff, headRoar, headCough, headHappy })
                if (t != null) t.gameObject.SetActive(t == pose);
        }

        // ---------- Ciclo di vita ----------

        void OnDisable()
        {
            // pausa/pubblicità/livello spento: dito annullato, fiammella sul falò, effetti spenti
            CancelAll();
            if (captured && !finishing && dragger != null && !IsFinished)
            {
                RestFlame();
                RestDragon();
                ClearEffects();
                HideSay();
            }
        }

        void OnEnable()
        {
            if (!captured || IsFinished || logic == null) return;
            // spento a metà della festa: si rifà il finale da capo (Win una volta sola, la logica fa da guardia)
            if (logic.Stage == FirelessDragonStage.Celebrating) { finishing = false; StartCoroutine(Finale()); }
        }

        void OnApplicationPause(bool paused) { if (paused) CancelAll(); }
        void OnApplicationFocus(bool focus) { if (!focus) CancelAll(); }

        /// <summary>Il dito sparisce: la fiammella tenuta (o che si stava spegnendo) torna sul falò.</summary>
        void CancelAll()
        {
            grip = Grip.None;
            if (logic == null || dragger == null || logic.InputLocked) return;
            if (logic.Cancel() == FirelessDragonEvent.Returned) RestFlame();
            else if (dragger.Held != null) { dragger.Release(); RestFlame(); }
        }

        void Update()
        {
            SayTick();
            if (toast != null) toast.Tick();
            if (logic == null || IsFinished || dragger == null) return;
            float dt = Time.deltaTime;
            TickBubbles(dt);
            TickPuffs(dt);
            if (finishing) return;
            if (Time.timeScale <= 0f) { CancelAll(); return; }   // pausa o pubblicità: niente input né sensori
#if UNITY_EDITOR
            EditorKeys();
            if (finishing) return;
#endif
            React(logic.Tick(dt));
            if (Inputs.Shake() > ShakeLevel) React(logic.Shake());
            ReadPointer();
            if (finishing) return;
            Animate(dt);
        }

        // ---------- Dito ----------

        void ReadPointer()
        {
            if (!HalloweenKit.ReadPointer(cam, out Vector2 w, out bool began, out bool ended))
            {
                if (grip == Grip.Flame) CancelGrip();   // dito sparito o secondo dito: la fiammella torna sul falò
                grip = Grip.None;
                return;
            }
            if (began)
            {
                if (grip == Grip.Flame) End(gripLast);
                Begin(w);
            }
            if (grip == Grip.None) return;
            gripLast = w;
            if (Vector2.Distance(w, gripStart) > TapSlop) gripMoved = true;
            if (grip == Grip.Flame && gripMoved)
            {
                dragger.MoveHeld(w);
                FirelessDragonEvent e = logic.Move(flame.position.x, flame.position.y);
                if (e == FirelessDragonEvent.None) e = logic.Move(w.x, w.y);
                React(e);
                if (finishing) return;
            }
            if (ended) End(w);
        }

        void Begin(Vector2 w)
        {
            grip = Grip.None;
            gripStart = gripLast = w;
            gripMoved = false;
            if (logic.InputLocked) return;
            if (logic.CanPick && flameArea != null && flameArea.OverlapPoint(w))
            {
                HwDragger.Item it = dragger.TryPick(w);
                if (it != null && logic.Pick() == FirelessDragonEvent.Picked)
                {
                    grip = Grip.Flame;
                    respawnT = -1f;   // presa mentre ricompariva: la scala la decide il trascinamento
                    HalloweenKit.SetAlpha(flameGlow, 0.6f);
                    Sfx.Play(this, "tick", 0.4f);
                    return;
                }
                if (it != null) dragger.Release();
            }
            grip = Grip.Other;
            int b = BubbleAt(w);
            if (b >= 0)
            {
                if (logic.PopBubble() == FirelessDragonEvent.Popped) PopBubble(b, true);
                return;
            }
            if (dragonArea != null && dragonArea.OverlapPoint(w)) React(logic.TapDragon());   // il tocco risponde subito (anche a raffica)
        }

        void End(Vector2 finger)
        {
            Grip g = grip;
            grip = Grip.None;
            if (g != Grip.Flame || dragger.Held == null) return;
            dragger.Release();
            Vector2 p = flame.position;
            bool onDragon = dragonArea != null && (dragonArea.OverlapPoint(p) || dragonArea.OverlapPoint(finger));
            React(logic.Release(p.x, p.y, finger.x, finger.y, onDragon));
        }

        void CancelGrip()
        {
            grip = Grip.None;
            if (dragger.Held != null) dragger.Release();
            if (logic.Cancel() == FirelessDragonEvent.Returned) dragger.SendHome(flameItem, 0.25f);
        }

        // ---------- Reazioni ----------

        void React(FirelessDragonEvent e)
        {
            switch (e)
            {
                case FirelessDragonEvent.Bubbles:
                    tapT = 0f;
                    burstQueue = Mathf.Min(burstQueue + 4, 8);   // una raffica di bolle (tetto: a raffica non si accumulano)
                    burstQueueT = 0f;
                    Sfx.Play(this, "pop", 0.35f);
                    Haptics.Tap();
                    Say(HintOr(TapLines[tapLines++ % TapLines.Length]), 1.6f);
                    break;
                case FirelessDragonEvent.Cough:
                    coughT = 0f;
                    StartPuffs();
                    Sfx.Play(this, "bonk", 0.45f);
                    Haptics.Light();
                    Say(HintOr(CoughLines[coughLines++ % CoughLines.Length]), 1.6f);
                    break;
                case FirelessDragonEvent.Returned:
                    dragger.SendHome(flameItem, 0.2f);
                    break;
                case FirelessDragonEvent.Fizzled:
                case FirelessDragonEvent.Singed:
                    StartFizzle();
                    if (e == FirelessDragonEvent.Singed)
                    {
                        tapT = 0f;
                        coughT = 0.35f;   // smorfia breve (faccia strizzata)
                        Say(HintOr(SingeLines[singeLines++ % SingeLines.Length]), 1.6f);
                    }
                    else if (toast != null)
                    {
                        string line = FizzleLines[fizzleLines++ % FizzleLines.Length];
                        if (logic.TakeHint()) Say(HintLine, 2.2f);
                        toast.Show(line, 1.8f);
                    }
                    break;
                case FirelessDragonEvent.Respawned:
                    flame.gameObject.SetActive(true);
                    flame.position = flameHome;
                    flame.localScale = Vector3.zero;
                    if (flameSprite != null) flameSprite.color = Color.white;
                    flameItem.Enabled = true;
                    respawnT = 0f;
                    HalloweenKit.Show(fizzlePuff, false);
                    Sfx.Play(this, "tick", 0.3f);
                    break;
                case FirelessDragonEvent.Absorbed:
                    StartCoroutine(Finale());
                    break;
            }
        }

        /// <summary>Dopo qualche tentativo sbagliato, una volta sola, la frase sottile al posto della battuta.</summary>
        string HintOr(string line) => logic.TakeHint() ? HintLine : line;

        void StartFizzle()
        {
            flameItem.Enabled = false;
            fizzleAt = flame.position;
            if (fizzlePuff != null)
            {
                fizzlePuff.gameObject.SetActive(true);
                fizzlePuff.transform.position = new Vector3(fizzleAt.x, fizzleAt.y + 0.1f, fizzlePuff.transform.position.z);
                fizzlePuff.transform.localScale = Vector3.one * 0.4f;
                HalloweenKit.SetAlpha(fizzlePuff, 0.9f);
            }
            Sfx.Play(this, "whoosh", 0.35f);
            Haptics.Tap();
        }

        // ---------- Fumetto del drago (si adatta al testo e resta nello schermo) ----------

        void Say(string text, float seconds)
        {
            if (dragonBubble == null) return;
            bool quiet = Time.time - lastSayAt < SayGap;
            lastSayAt = Time.time;
            dragonBubble.Show(text, quiet);
            sayHideAt = Time.time + seconds;
        }

        void HideSay()
        {
            sayHideAt = -1f;
            if (dragonBubble != null) dragonBubble.Hide();
        }

        void SayTick()
        {
            if (sayHideAt > 0f && Time.time >= sayHideAt) HideSay();
        }

        // ---------- Animazioni del gioco ----------

        void Animate(float dt)
        {
            float t = Time.time;
            // falò e fiammella che tremolano
            for (int i = 0; i < fireBase.Length; i++)
                if (fireFlames[i] != null)
                    fireFlames[i].localScale = new Vector3(fireBase[i].x * (1f + 0.06f * Mathf.Sin(t * 13f + i * 2.1f)), fireBase[i].y * (1f + 0.12f * Mathf.Sin(t * 9f + i * 1.3f)), 1f);
            if (fireGlow != null) HalloweenKit.SetAlpha(fireGlow, 0.5f + 0.08f * Mathf.Sin(t * 7f));
            if (flameArt != null) flameArt.localScale = new Vector3(flameArtBase.x * (1f + 0.05f * Mathf.Sin(t * 15f)), flameArtBase.y * (1f + 0.1f * Mathf.Sin(t * 11f)), 1f);

            // fiammella che si spegne (resta dov'era, si rimpicciolisce e ingrigisce) o che ricompare sul falò
            if (logic.Flame == FirelessDragonFlame.Fizzling)
            {
                float k = logic.FizzleProgress;
                flame.position = fizzleAt;
                flame.localScale = Vector3.one * Mathf.Max(0f, 1f - k * 1.6f);
                if (flameSprite != null) flameSprite.color = Color.Lerp(Color.white, new Color(0.5f, 0.5f, 0.55f, 1f), k * 2f);
                HalloweenKit.SetAlpha(flameGlow, 0.6f * Mathf.Max(0f, 1f - k * 2f));
                if (fizzlePuff != null && fizzlePuff.gameObject.activeSelf)
                {
                    fizzlePuff.transform.position += Vector3.up * dt * 0.5f;
                    fizzlePuff.transform.localScale = Vector3.one * Mathf.Lerp(0.4f, 1.0f, k);
                    HalloweenKit.SetAlpha(fizzlePuff, 0.9f * (1f - k));
                }
            }
            else if (respawnT >= 0f)
            {
                respawnT += dt;
                float k = Mathf.Clamp01(respawnT / 0.25f);
                flame.localScale = Vector3.one * (k < 1f ? Mathf.Sin(k * Mathf.PI * 0.5f) * (1f + 0.15f * Mathf.Sin(k * Mathf.PI)) : 1f);
                HalloweenKit.SetAlpha(flameGlow, 0.6f * k);
                if (k >= 1f) respawnT = -1f;
            }

            // drago: respiro, rimbalzo del tocco, tosse (scossone) e faccia
            float breathe = Mathf.Sin(t * 2.4f);
            float pop = 0f;
            if (tapT >= 0f) { tapT += dt; float k = tapT / 0.25f; pop = k < 1f ? Mathf.Sin(k * Mathf.PI) : 0f; if (k >= 1f) tapT = -1f; }
            dragonBody.localScale = new Vector3(bodyBase.x * (1f + 0.01f * breathe + 0.07f * pop), bodyBase.y * (1f + 0.018f * breathe - 0.06f * pop), 1f);
            bool coughing = false;
            if (coughT >= 0f)
            {
                coughT += dt;
                coughing = coughT < 0.7f;
                float a = coughing ? 0.05f * (1f - coughT / 0.7f) : 0f;
                dragonBody.localPosition = new Vector3(Mathf.Sin(t * 60f) * a, Mathf.Abs(Mathf.Sin(t * 30f)) * a, 0f);
                if (!coughing) { coughT = -1f; dragonBody.localPosition = Vector3.zero; }
            }
            ShowHead(coughing ? headCough : headIdle);

            // bolle: una ogni tanto, più la raffica dei tocchi
            bubbleTimer -= dt;
            if (bubbleTimer <= 0f) { bubbleTimer = BubbleEvery; SpawnBubble(); }
            if (burstQueue > 0)
            {
                burstQueueT -= dt;
                if (burstQueueT <= 0f) { burstQueueT = 0.07f; burstQueue--; SpawnBubble(); }
            }
        }

        void SpawnBubble()
        {
            if (soapBubbles == null || soapBubbles.Length == 0) return;
            int pick = -1;
            float oldest = -1f;
            for (int i = 0; i < soapBubbles.Length; i++)
            {
                if (bLife[i] <= 0f) { pick = i; break; }
                if (bAge[i] > oldest) { oldest = bAge[i]; pick = i; }   // tutte in volo: si ricicla la più vecchia
            }
            if (pick < 0 || soapBubbles[pick] == null) return;
            Vector3 m = mouth.position;
            bPos[pick] = new Vector3(m.x + 0.08f, m.y, soapBubbles[pick].transform.position.z);
            bVel[pick] = new Vector3(Random.Range(0.3f, 0.6f), Random.Range(0.45f, 0.8f), 0f);
            bAge[pick] = 0f;
            bLife[pick] = Random.Range(2.4f, 3.4f);
            bSize[pick] = Random.Range(0.32f, 0.55f);   // bolle ben visibili sul cielo chiaro
            bPop[pick] = -1f;
            SpriteRenderer sr = soapBubbles[pick];
            sr.gameObject.SetActive(true);
            sr.color = Color.white;
            sr.transform.position = bPos[pick];
            sr.transform.localScale = Vector3.zero;
        }

        void TickBubbles(float dt)
        {
            if (soapBubbles == null) return;
            for (int i = 0; i < soapBubbles.Length; i++)
            {
                SpriteRenderer sr = soapBubbles[i];
                if (sr == null || bLife[i] <= 0f) continue;
                if (bPop[i] >= 0f)
                {
                    bPop[i] += dt;
                    float k = bPop[i] / 0.12f;
                    sr.transform.localScale = Vector3.one * bSize[i] * (1f + 0.4f * k);
                    HalloweenKit.SetAlpha(sr, Mathf.Max(0f, 1f - k));
                    if (k >= 1f) { bLife[i] = 0f; sr.gameObject.SetActive(false); }
                    continue;
                }
                bAge[i] += dt;
                bPos[i] += bVel[i] * dt;
                float wob = Mathf.Sin(bAge[i] * 3f + i) * 0.08f;
                sr.transform.position = bPos[i] + new Vector3(wob, 0f, 0f);
                sr.transform.localScale = Vector3.one * bSize[i] * Mathf.Min(1f, bAge[i] / 0.2f);
                // in alto (sotto la barra) o al bordo destro: scoppia da sola
                if (bAge[i] >= bLife[i] || bPos[i].y > 2.7f || bPos[i].x > 2.5f) bPop[i] = 0f;
            }
        }

        int BubbleAt(Vector2 w)
        {
            if (soapBubbles == null) return -1;
            for (int i = 0; i < soapBubbles.Length; i++)
            {
                if (soapBubbles[i] == null || bLife[i] <= 0f || bPop[i] >= 0f) continue;
                float r = bSize[i] * 0.5f + 0.12f;   // area generosa
                if (((Vector2)soapBubbles[i].transform.position - w).sqrMagnitude <= r * r) return i;
            }
            return -1;
        }

        void PopBubble(int i, bool sound)
        {
            if (i < 0 || bLife[i] <= 0f || bPop[i] >= 0f) return;
            bPop[i] = 0f;
            if (sound) Sfx.Play(this, "pop", 0.45f);
        }

        void StartPuffs()
        {
            if (smokePuffs == null) return;
            for (int i = 0; i < smokePuffs.Length; i++)
            {
                puffAge[i] = -0.1f * i;   // uno dopo l'altro
                if (smokePuffs[i] != null) smokePuffs[i].gameObject.SetActive(false);
            }
        }

        void TickPuffs(float dt)
        {
            if (smokePuffs == null) return;
            Vector3 m = mouth.position;
            for (int i = 0; i < smokePuffs.Length; i++)
            {
                SpriteRenderer sr = smokePuffs[i];
                if (sr == null || puffAge[i] == -1f) continue;
                puffAge[i] += dt;
                if (puffAge[i] < 0f) continue;
                float k = puffAge[i] / 0.8f;
                if (k >= 1f) { puffAge[i] = -1f; sr.gameObject.SetActive(false); continue; }
                sr.gameObject.SetActive(true);
                sr.transform.position = new Vector3(m.x + 0.15f + k * (0.45f + 0.15f * i), m.y + 0.05f + k * (0.5f + 0.1f * i), sr.transform.position.z);
                sr.transform.localScale = Vector3.one * Mathf.Lerp(0.5f, 1.25f, k);
                HalloweenKit.SetAlpha(sr, 0.9f * (1f - k));
            }
        }

        // ---------- Vittoria ----------

        IEnumerator Finale()
        {
            if (finishing) yield break;
            finishing = true;
            grip = Grip.None;
            if (dragger != null && dragger.Held != null) dragger.Release();
            if (flameItem != null) flameItem.Enabled = false;
            if (dragger != null) dragger.Stop(flameItem);
            HideSay();
            HalloweenKit.Show(fizzlePuff, false);
            dragonBody.localPosition = Vector3.zero;
            Vector3 m = mouth.position;
            // 1) la fiammella vola in bocca e sparisce: "Gulp!"
            if (flame.gameObject.activeSelf)
            {
                Vector3 from = flame.position;
                float t0 = 0f;
                while (t0 < 0.2f)
                {
                    t0 += Time.deltaTime;
                    float k = Mathf.Clamp01(t0 / 0.2f);
                    flame.position = Vector3.Lerp(from, new Vector3(m.x, m.y, from.z), k);
                    flame.localScale = Vector3.one * (1f - 0.85f * k);
                    yield return null;
                }
                flame.gameObject.SetActive(false);
            }
            for (int i = 0; i < soapBubbles.Length; i++) PopBubble(i, false);   // le bolle scoppiano tutte
            ShowHead(headPuff);
            Sfx.Play(this, "pop", 0.7f);
            Say("Gulp!", 0.9f);
            // 2) guance gonfie: il drago si gonfia, la bocca si illumina
            if (mouthGlow != null)
            {
                mouthGlow.gameObject.SetActive(true);
                mouthGlow.transform.position = new Vector3(m.x + 0.1f, m.y, mouthGlow.transform.position.z);
            }
            float t = 0f;
            while (t < 0.35f)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / 0.35f);
                dragonBody.localScale = new Vector3(bodyBase.x * (1f + 0.08f * k), bodyBase.y * (1f + 0.05f * k), 1f);
                dragonBody.localPosition = new Vector3(Mathf.Sin(t * 70f) * 0.015f * k, 0f, 0f);
                if (mouthGlow != null)
                {
                    mouthGlow.transform.localScale = Vector3.one * Mathf.Lerp(0.3f, 1.1f, k);
                    mouthGlow.color = new Color(1f, 0.6f, 0.2f, 0.7f * k);
                }
                yield return null;
            }
            // 3) la grande fiammata (scala e tremolio scritti, sotto la barra in alto)
            HideSay();
            ShowHead(headRoar);
            dragonBody.localPosition = Vector3.zero;
            Sfx.Play(this, "whoosh", 0.9f);
            Sfx.Play(this, "buzz", 0.35f);
            Haptics.Medium();
            if (burst != null) { burst.gameObject.SetActive(true); burst.localScale = new Vector3(0.1f, 0.6f, 1f); }
            if (burstSprite != null) burstSprite.color = Color.white;
            t = 0f;
            while (t < 0.6f)
            {
                t += Time.deltaTime;
                float grow = Mathf.Clamp01(t / 0.15f);
                float sx = Mathf.Lerp(0.1f, 1f, 1f - (1f - grow) * (1f - grow)) * (1f + 0.04f * Mathf.Sin(t * 40f));
                float sy = Mathf.Lerp(0.6f, 1f, grow) * (1f + 0.1f * Mathf.Sin(t * 33f));
                if (burst != null) burst.localScale = new Vector3(sx, sy, 1f);
                dragonBody.localScale = new Vector3(bodyBase.x * (1f + 0.08f * (1f - grow)), bodyBase.y * (1f + 0.05f * (1f - grow)), 1f);
                dragonBody.localPosition = new Vector3(-0.06f * Mathf.Sin(Mathf.Clamp01(t / 0.3f) * Mathf.PI), 0f, 0f);   // rinculo
                if (mouthGlow != null) mouthGlow.transform.localScale = Vector3.one * (1.2f + 0.2f * Mathf.Sin(t * 30f));
                yield return null;
            }
            // 4) la fiammata svanisce, il drago sorride con due sbuffi di fumo soddisfatti
            ShowHead(headHappy);
            StartPuffs();
            t = 0f;
            while (t < 0.25f)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / 0.25f);
                HalloweenKit.SetAlpha(burstSprite, 1f - k);
                if (burst != null) burst.localScale = new Vector3(1f + 0.1f * k, 1f - 0.3f * k, 1f);
                if (mouthGlow != null) mouthGlow.color = new Color(1f, 0.6f, 0.2f, 0.7f * (1f - k));
                yield return null;
            }
            if (burst != null) burst.gameObject.SetActive(false);
            HalloweenKit.Show(mouthGlow, false);
            dragonBody.localPosition = Vector3.zero;
            dragonBody.localScale = bodyBase;
            Sfx.Play(this, "ding", 0.6f);
            if (reward != null) yield return reward.Play(Compliment);
            if (!IsFinished && logic.TryComplete()) Win(Compliment);
        }

#if UNITY_EDITOR
        // ---------- Tasti di prova nell'Editor ----------

        void EditorKeys()
        {
            if (logic.InputLocked) return;
            if (Input.GetKeyDown(KeyCode.T)) React(logic.TapDragon());
            if (Input.GetKeyDown(KeyCode.B))
                for (int i = 0; i < soapBubbles.Length; i++)
                    if (bLife[i] > 0f && bPop[i] < 0f) { if (logic.PopBubble() == FirelessDragonEvent.Popped) PopBubble(i, true); break; }
            if (dragger.Held == null && logic.CanPick)
            {
                Vector3 m = mouth.position;
                if (Input.GetKeyDown(KeyCode.F)) EditorDrop(new Vector2(m.x, m.y), false);
                else if (Input.GetKeyDown(KeyCode.G)) EditorDrop(new Vector2(-1.8f, -4.0f), false);
                else if (Input.GetKeyDown(KeyCode.H)) EditorDrop(new Vector2(dragonBody.position.x + 0.2f, dragonBody.position.y + 1.0f), true);
            }
            if (Input.GetKeyDown(KeyCode.P)) CancelAll();
            if (Input.GetKeyDown(KeyCode.R)) OnLevelStart();
        }

        void EditorDrop(Vector2 at, bool onDragon)
        {
            if (logic.Pick() != FirelessDragonEvent.Picked) return;
            flame.position = new Vector3(at.x, at.y, flameHome.z);
            React(logic.Release(at.x, at.y, at.x, at.y, onDragon));
        }
#endif
    }
}
