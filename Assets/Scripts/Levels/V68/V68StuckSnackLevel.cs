using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels.V64;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 263 "The Stuck Snack" — "Get the snack." Un distributore in corridoio ha appena incastrato la barretta
    /// B3 sul bordo della spirale (lo si vede all'inizio); la stessa barretta è anche in A3, proprio sopra. Nella mano
    /// del giocatore c'è UNA moneta. False piste (nessuna azzera i progressi, la moneta torna sempre): toccare/prendere a
    /// calci il distributore, scuotere il telefono (l'adesivo "DO NOT ROCK THE MACHINE" lampeggia), infilare il braccio
    /// nello sportello (resta incastrato un attimo), ripremere B3 ("SOLD OUT"), comprare un altro scomparto (si incastra
    /// anche quello, poi "MACHINE ERROR — REFUNDED"), la vaschetta del resto (vuota: esce una falena).
    /// Soluzione: moneta (trascinata nella fessura o toccata) + A3, in qualunque ordine: la A3 cade, colpisce la B3 e
    /// cadono tutte e due; poi si prendono dallo sportello. Due al prezzo di una.
    /// Tutto lo scomparto (barretta + cartellino + bottoncino) è il tasto: niente gesti precisi.
    /// Niente microfono. La logica è in Gotcha.Levels.V64.StuckSnackLogic (senza Unity): qui solo input e presentazione.
    /// Tasti di prova nell'Editor: K moneta + A3, T prende dallo sportello, 1 calcio, 2 braccio, 3 B3, 4 C1, 5 resto
    /// (S = scossone, da Inputs.Shake).
    /// </summary>
    public class V68StuckSnackLevel : V62Level
    {
        [Header("Distributore")]
        [SerializeField] Transform machinePivot;        // perno in basso al centro: il distributore dondola attorno a questo
        [SerializeField] Collider2D bodyArea;           // tutto il distributore (ultima scelta: un calcio)
        [SerializeField] Transform[] snacks;            // 9 scomparti: A1 A2 A3 B1 B2 B3 C1 C2 C3
        [SerializeField] Transform[] coils;
        [SerializeField] Collider2D[] cellAreas;        // tutto lo scomparto è il tasto
        [SerializeField] TextMesh display;
        [SerializeField] SpriteRenderer sticker;
        [SerializeField] Transform coinSlot;
        [SerializeField] Collider2D coinSlotArea;
        [SerializeField] Transform changeCup;
        [SerializeField] Collider2D changeArea;
        [SerializeField] Transform flapPivot;           // cerniera in alto: si "apre" schiacciandolo in y
        [SerializeField] Collider2D flapArea;
        [SerializeField] SpriteRenderer flapGlow;
        [SerializeField] SmokeEmitter dust;
        [Header("Giocatore")]
        [SerializeField] Transform coin;
        [SerializeField] Collider2D coinArea;
        [SerializeField] Transform palm;
        [SerializeField] Collider2D palmArea;
        [SerializeField] Transform arm;                 // il braccio che sale nello sportello (mano in cima)
        [SerializeField] Transform loot;                // le due barrette in mano (figlio del braccio)
        [SerializeField] Transform moth;
        [SerializeField] TextMesh infoText;
        [SerializeField] Transform infoHolder;

        const float TapTime = 0.45f, TapSlop = 0.25f;
        const float SlotSnap = 0.8f;
        const float ShakeLevel = 12f, ShakeCooldown = 1.6f;
        const float PendingSeconds = 7f;
        const float ArmHiddenY = -6.2f;
        static readonly Color LedColor = new Color(0.45f, 1f, 0.55f);
        static readonly Color StickerFlash = new Color(1f, 0.35f, 0.3f);

        static readonly string[] KickLines = { "Ow.", "The machine is unimpressed.", "It wobbled. The snack didn't." };
        static readonly string[] ShakeLines = { "WARNING: DO NOT ROCK THE MACHINE.", "It rocks. It settles. The snack stays.", "The sticker was very clear about this." };
        static readonly string[] ArmLines = { "Your arm is stuck.", "Stuck again. The flap only opens for things that FALL.", "Your arm has learned nothing." };
        static readonly string[] SoldOutLines = { "SOLD OUT. (It's RIGHT THERE.)", "Still SOLD OUT. Coin returned.", "B3 has left the building. Almost." };
        static readonly string[] ErrorLines = { "Now you have TWO stuck snacks.", "Not that one either...", "Another one bites the edge." };
        static readonly string[] RefundLines = { "...MACHINE ERROR. REFUNDED.", "...nope. Refunded.", "...and back it goes. Refunded." };
        static readonly string[] ChangeLines = { "Empty. A moth flies out.", "Still empty. The moth was the change.", "Nothing. Not even a moth." };
        static readonly string[] NoCoinLines = { "Insert coin first.", "Coin first. Then the button.", "The machine wants money, not hope." };

        enum Kind { None, Coin, Flap, Other }

        sealed class Grip { public Kind kind; public Vector2 start, last; public float t0; public bool moved; }

        readonly StuckSnackLogic logic = new StuckSnackLogic();
        readonly Dictionary<int, Grip> grips = new Dictionary<int, Grip>();
        readonly List<int> gone = new List<int>(4);
        readonly Dictionary<Transform, Coroutine> wobbles = new Dictionary<Transform, Coroutine>();
        Camera cam;
        HwToast toast;
        HwDragger dragger;
        HwDragger.Item coinItem;
        Vector3[] snackPos, snackScale;
        SpriteRenderer[] snackArt;
        int snackOrder;
        Vector3[] coilScale;
        Vector3 coinHome, coinScale, armHome, mothHome, flapScale, mothScale, palmScale;
        Color stickerColor;
        int kickLines, shakeLines, armLines, soldLines, errorLines, changeLines, noCoinLines;
        float shakeReadyAt, pendingAt, nagAt;
        bool busy, finishing, introDone;

        static int Index(int row, int col) => row * StuckSnackLogic.Cols + col;
        static readonly int StuckIndex = Index(StuckSnackLogic.StuckRow, StuckSnackLogic.StuckCol);
        static readonly int SolutionIndex = Index(StuckSnackLogic.SolutionRow, StuckSnackLogic.SolutionCol);

        public override void OnLevelStart()
        {
            cam = Camera.main;
            StopAllCoroutines();
            logic.Reset();
            ResetV62();
            V64AFingers.Reset();
            grips.Clear();
            wobbles.Clear();
            busy = finishing = false;
            kickLines = shakeLines = armLines = soldLines = errorLines = changeLines = noCoinLines = 0;
            toast = new HwToast(infoText, infoHolder);
            if (snackPos == null)
            {
                snackPos = new Vector3[snacks.Length];
                snackScale = new Vector3[snacks.Length];
                snackArt = new SpriteRenderer[snacks.Length];
                for (int i = 0; i < snacks.Length; i++)
                {
                    snackPos[i] = snacks[i].localPosition;
                    snackScale[i] = snacks[i].localScale;
                    snackArt[i] = snacks[i].GetComponentInChildren<SpriteRenderer>(true);
                }
                snackOrder = snackArt[0] != null ? snackArt[0].sortingOrder : 0;
                coilScale = new Vector3[coils.Length];
                for (int i = 0; i < coils.Length; i++) coilScale[i] = coils[i].localScale;
                coinHome = coin.position;
                coinScale = coin.localScale;
                armHome = arm.position;
                mothHome = moth.position;
                mothScale = moth.localScale;
                flapScale = flapPivot.localScale;
                palmScale = palm.localScale;
                stickerColor = sticker.color;
            }
            dragger = new HwDragger(this);
            coinItem = dragger.Add(coin, coinArea, 2);
            coinItem.Home = coinHome;
            shakeReadyAt = Time.time + 0.5f;
            pendingAt = nagAt = -1f;
            RestPose();
            introDone = false;
            ShowSnack(StuckIndex, true);
            StartCoroutine(Intro());
        }

        /// <summary>Tutto al suo posto (la B3 incastrata, le altre a riposo, la moneta dove dice la logica).</summary>
        void RestPose()
        {
            machinePivot.localRotation = Quaternion.identity;
            for (int i = 0; i < snacks.Length; i++)
            {
                snacks[i].localPosition = snackPos[i];
                snacks[i].localRotation = Quaternion.identity;
                snacks[i].localScale = snackScale[i];
                if (snackArt[i] != null) snackArt[i].sortingOrder = snackOrder;
                coils[i].localScale = coilScale[i];
            }
            bool gone = logic.Stage != SnackStage.Stuck;
            ShowSnack(SolutionIndex, !gone);
            ShowSnack(StuckIndex, !gone);
            if (!gone) StuckPose(snacks[StuckIndex], StuckIndex, 1f);
            coin.localScale = coinScale;
            coin.position = coinHome;
            coin.localRotation = Quaternion.identity;
            HalloweenKit.Show(coin, logic.CoinInWallet);
            if (coinItem != null) coinItem.Enabled = true;
            arm.position = new Vector3(armHome.x, ArmHiddenY, armHome.z);
            HalloweenKit.Show(arm, false);
            HalloweenKit.Show(loot, false);
            moth.position = mothHome;
            moth.localScale = mothScale;
            HalloweenKit.Show(moth, false);
            flapPivot.localScale = flapScale;
            palm.localScale = palmScale;
            palm.localRotation = Quaternion.identity;
            sticker.color = stickerColor;
            HalloweenKit.Show(flapGlow, logic.Stage == SnackStage.InFlap);
            if (dust != null) dust.Stop();
            ShowDisplay();
        }

        void ShowSnack(int i, bool on) { if (i >= 0 && i < snacks.Length) HalloweenKit.Show(snacks[i], on); }

        /// <summary>La barretta sporge dal bordo della spirale, storta (k = 0 a riposo, 1 incastrata).</summary>
        void StuckPose(Transform s, int i, float k)
        {
            s.localPosition = snackPos[i] + new Vector3(0.08f, -0.17f, 0f) * k;
            s.localRotation = Quaternion.Euler(0f, 0f, -27f * k);
            s.localScale = snackScale[i] * (1f + 0.08f * k);
        }

        void OnDisable()
        {
            // pausa/pubblicità/livello spento: le animazioni si fermano con l'oggetto, si torna a uno stato valido
            CancelGrips();
            busy = false;
            wobbles.Clear();
            introDone = true;
            if (machinePivot != null) machinePivot.localRotation = Quaternion.identity;
            if (snackPos != null && !finishing) RestPose();
        }

        void OnEnable()
        {
            if (snackPos == null || IsFinished) return;
            // spento a metà della caduta: le barrette sono comunque nello sportello
            if (logic.Stage == SnackStage.Dropping) { logic.FinishDrop(); finishing = false; RestPose(); nagAt = Time.time + 5f; }
            else if (logic.Stage == SnackStage.Taken) StartCoroutine(Finale());
        }

        void OnApplicationPause(bool paused) { if (paused) CancelGrips(); }
        void OnApplicationFocus(bool focus) { if (!focus) CancelGrips(); }

        void CancelGrips()
        {
            grips.Clear();
            if (dragger != null && dragger.Held != null) dragger.Cancel();
        }

        void Update()
        {
            toast?.Tick();
            if (IsFinished || finishing) return;
            if (Time.timeScale <= 0f) { CancelGrips(); return; }   // pausa o pubblicità: niente input
            if (logic.Stage == SnackStage.InFlap) PulseFlap();
            if (pendingAt > 0f && Time.time >= pendingAt) { pendingAt = -1f; logic.ClearPending(); ShowDisplay(); }
            if (!introDone || busy) { CancelGrips(); return; }
#if UNITY_EDITOR
            EditorKeys();
            if (busy || finishing) return;
#endif
            if (Inputs.Shake() > ShakeLevel && Time.time >= shakeReadyAt) { shakeReadyAt = Time.time + ShakeCooldown; ShakeMachine(); return; }
            ReadFingers();
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
                if (g.kind == Kind.Coin) dragger.MoveHeld(f.world);
                if (f.ended) End(f.id, g);
                if (busy || finishing) { grips.Clear(); return; }
            }
            if (fingers.Count == 0 && grips.Count > 0)
            {
                gone.Clear();
                foreach (int id in grips.Keys) gone.Add(id);
                foreach (int id in gone) End(id, grips[id]);
            }
        }

        Grip Begin(Vector2 w)
        {
            var g = new Grip { start = w, last = w, t0 = Time.time, kind = Kind.Other };
            if (busy || finishing) { g.kind = Kind.None; return g; }
            if (logic.Stage == SnackStage.InFlap)
            {
                if (flapArea.OverlapPoint(w)) { g.kind = Kind.Flap; ReachFlap(); }
                return g;   // altrove: conta solo se il dito finisce trascinato dentro lo sportello
            }
            if (dragger.Held == null && dragger.TryPick(w) != null) { g.kind = Kind.Coin; return g; }
            if (flapArea.OverlapPoint(w)) { g.kind = Kind.Flap; ReachFlap(); return g; }
            return g;
        }

        void End(int id, Grip g)
        {
            grips.Remove(id);
            bool tap = !g.moved && Time.time - g.t0 <= TapTime;
            switch (g.kind)
            {
                case Kind.Coin:
                    DropCoin(tap);
                    break;
                case Kind.Other:
                    if (tap) TapScene(g.start);
                    else if (flapArea.OverlapPoint(g.last)) ReachFlap();   // trascinato dentro lo sportello
                    break;
            }
        }

        void TapScene(Vector2 w)
        {
            if (logic.Stage != SnackStage.Stuck) return;
            for (int i = 0; i < cellAreas.Length; i++)
                if (cellAreas[i] != null && cellAreas[i].OverlapPoint(w)) { PressSlot(i / StuckSnackLogic.Cols, i % StuckSnackLogic.Cols); return; }
            if (coinSlotArea.OverlapPoint(w)) { TapCoinSlot(); return; }
            if (changeArea.OverlapPoint(w)) { TapChange(); return; }
            if (palmArea.OverlapPoint(w))
            {
                StartCoroutine(HalloweenKit.Wiggle(palm, 5f, 0.3f, 30f));
                toast.Show(logic.CoinInWallet ? "One coin. That's all you've got." : "Your coin is in the machine. Pick a snack.", 1.6f);
                return;
            }
            if (bodyArea.OverlapPoint(w)) Kick(w);
        }

        // ---------- False piste ----------

        void Kick(Vector2 w)
        {
            int n = logic.Kick();
            if (n == 0) return;
            V68StuckSnackSound.Play(this, "bonk", 0.8f);
            Haptics.Light();
            float dir = w.x < machinePivot.position.x ? -1f : 1f;
            Wobble(machinePivot, 2.2f * dir, 0.45f, 30f);
            StartCoroutine(WiggleStuck(0.6f));
            toast.Show(KickLines[kickLines++ % KickLines.Length], 1.5f);
        }

        IEnumerator WiggleStuck(float seconds)
        {
            Transform s = snacks[StuckIndex];
            for (float k = 0f; k < seconds; k += Time.deltaTime)
            {
                if (logic.Stage != SnackStage.Stuck) yield break;
                StuckPose(s, StuckIndex, 1f + 0.18f * Mathf.Sin(k * 40f) * (1f - k / seconds));
                yield return null;
            }
            if (logic.Stage == SnackStage.Stuck) StuckPose(s, StuckIndex, 1f);
        }

        void ShakeMachine()
        {
            if (logic.Shake() == 0) return;
            StartCoroutine(Rock());
        }

        IEnumerator Rock()
        {
            busy = true;
            CancelGrips();
            if (wobbles.TryGetValue(machinePivot, out Coroutine w) && w != null) { StopCoroutine(w); wobbles.Remove(machinePivot); }
            Haptics.Medium();
            V68StuckSnackSound.Play(this, "rattle", 0.7f);
            toast.Show(ShakeLines[shakeLines++ % ShakeLines.Length], 1.8f);
            StartCoroutine(WiggleStuck(1.2f));
            for (float k = 0f; k < 1.2f; k += Time.deltaTime)
            {
                machinePivot.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(k * 14f) * 5f * (1f - k / 1.2f));
                bool on = Mathf.Repeat(k, 0.24f) < 0.12f;
                sticker.color = on ? StickerFlash : stickerColor;
                if (Mathf.Repeat(k, 0.24f) < Time.deltaTime) V68StuckSnackSound.Play(this, "beep_hi", 0.5f);
                yield return null;
            }
            machinePivot.localRotation = Quaternion.identity;
            sticker.color = stickerColor;
            V68StuckSnackSound.Play(this, "thud", 0.5f);
            busy = false;
        }

        void ReachFlap()
        {
            FlapReaction r = logic.ReachFlap();
            if (r == FlapReaction.ArmStuck) StartCoroutine(ArmStuck());
            else if (r == FlapReaction.Take) StartCoroutine(Finale());
        }

        IEnumerator ArmStuck()
        {
            busy = true;
            CancelGrips();
            yield return ArmIn();
            Haptics.Medium();
            V68StuckSnackSound.Play(this, "bonk", 0.6f);
            toast.Show(ArmLines[armLines++ % ArmLines.Length], 1.6f);
            // tira, tira... incastrato
            Vector3 p = arm.position;
            for (float k = 0f; k < 1.1f; k += Time.deltaTime)
            {
                arm.position = p + new Vector3(Mathf.Sin(k * 38f) * 0.04f, -Mathf.Abs(Mathf.Sin(k * 9f)) * 0.07f, 0f);
                yield return null;
            }
            arm.position = p;
            V68StuckSnackSound.Play(this, "pop", 0.7f);
            toast.Show("...Free.", 1.2f);
            yield return ArmOut(0.3f);
            busy = false;
        }

        IEnumerator ArmIn()
        {
            HalloweenKit.Show(arm, true);
            arm.position = new Vector3(armHome.x, ArmHiddenY, armHome.z);
            Sfx.Play(this, "whoosh", 0.4f);
            yield return HalloweenKit.Move(arm, armHome, 0.3f);
            flapPivot.localScale = new Vector3(flapScale.x, flapScale.y * 0.35f, flapScale.z);
            V68StuckSnackSound.Play(this, "clack", 0.5f);
        }

        IEnumerator ArmOut(float seconds)
        {
            flapPivot.localScale = flapScale;
            V68StuckSnackSound.Play(this, "clack", 0.4f);
            yield return HalloweenKit.Move(arm, new Vector3(armHome.x, ArmHiddenY, armHome.z), seconds);
            HalloweenKit.Show(arm, false);
        }

        void TapChange()
        {
            int n = logic.TapChange();
            if (n == 0) return;
            V68StuckSnackSound.Play(this, "clack", 0.5f);
            toast.Show(ChangeLines[changeLines++ % ChangeLines.Length], 1.7f);
            if (n <= 2 && !moth.gameObject.activeSelf) StartCoroutine(MothFlight());
        }

        IEnumerator MothFlight()
        {
            HalloweenKit.Show(moth, true);
            Vector3 from = changeCup.position;
            Vector3 to = new Vector3(2.9f, 2.4f, mothHome.z);
            V68StuckSnackSound.Play(this, "flutter", 0.6f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 1.6f)
            {
                Vector3 p = Vector3.Lerp(from, to, k * k);
                p += new Vector3(Mathf.Sin(k * 19f) * 0.25f, Mathf.Sin(k * 11f) * 0.2f, 0f) * Mathf.Min(1f, k * 4f);
                moth.position = p;
                moth.localScale = new Vector3(mothScale.x, mothScale.y * (0.5f + 0.5f * Mathf.Abs(Mathf.Sin(k * 60f))), mothScale.z);
                yield return null;
            }
            moth.localScale = mothScale;
            moth.position = mothHome;
            HalloweenKit.Show(moth, false);
        }

        // ---------- Moneta e tastiera ----------

        void DropCoin(bool tap)
        {
            dragger.Release();
            if (tap || Vector2.Distance(coin.position, coinSlot.position) < SlotSnap) { InsertCoin(); return; }
            dragger.SendHome(coinItem, 0.3f);
        }

        void TapCoinSlot()
        {
            if (logic.CoinInWallet) { InsertCoin(); return; }
            Sfx.Play(this, "tick", 0.5f);
            toast.Show("Your coin is already in. Pick a snack.", 1.6f);
        }

        void InsertCoin()
        {
            if (!logic.CoinInWallet) return;
            StartCoroutine(CoinIn());
        }

        IEnumerator CoinIn()
        {
            busy = true;
            CancelGrips();
            coinItem.Enabled = false;
            yield return HalloweenKit.Move(coin, coinSlot.position, 0.25f, 0.3f);
            // la moneta entra di taglio
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.12f)
            {
                coin.localScale = new Vector3(coinScale.x * (1f - k), coinScale.y, coinScale.z);
                yield return null;
            }
            HalloweenKit.Show(coin, false);
            coin.localScale = coinScale;
            V68StuckSnackSound.Play(this, "clink", 0.7f);
            Haptics.Tap();
            busy = false;
            pendingAt = -1f;
            int pr = logic.PendingRow, pc = logic.PendingCol;   // la scelta in memoria (se c'è) viene comprata adesso
            VendResult r = logic.InsertCoin();
            if (r == VendResult.NeedCoin) { ShowDisplay(); toast.Show("Clink. CREDIT: 1. Pick a snack.", 1.6f); }
            else Resolve(r, pr, pc);
        }

        void PressSlot(int row, int col)
        {
            VendResult r = logic.PressSlot(row, col);
            if (r == VendResult.None) return;
            V68StuckSnackSound.Play(this, "beep", 0.5f);
            Haptics.Tap();
            if (r == VendResult.NeedCoin)
            {
                pendingAt = Time.time + PendingSeconds;
                Transform s = snacks[Index(row, col)];
                if (Index(row, col) == StuckIndex) StartCoroutine(WiggleStuck(0.4f));
                else StartCoroutine(HalloweenKit.Pop(s, 1.08f, 0.2f));
                ShowDisplay();
                toast.Show(NoCoinLines[noCoinLines++ % NoCoinLines.Length], 1.5f);
                return;
            }
            Resolve(r, row, col);
        }

        void Resolve(VendResult r, int row, int col)
        {
            switch (r)
            {
                case VendResult.Vend: StartCoroutine(Drop()); break;
                case VendResult.SoldOut: StartCoroutine(SoldOut()); break;
                case VendResult.Error: if (StuckSnackLogic.ValidSlot(row, col)) StartCoroutine(Error(Index(row, col))); break;
            }
        }

        IEnumerator SoldOut()
        {
            busy = true;
            CancelGrips();
            SetDisplay("SOLD OUT");
            V64ASound.Play(this, "error", 0.6f);
            StartCoroutine(WiggleStuck(0.5f));
            toast.Show(SoldOutLines[soldLines++ % SoldOutLines.Length], 1.8f);
            yield return new WaitForSeconds(0.6f);
            yield return Refund();
            busy = false;
        }

        IEnumerator Error(int i)
        {
            busy = true;
            CancelGrips();
            SetDisplay(StuckSnackLogic.SlotName(i / StuckSnackLogic.Cols, i % StuckSnackLogic.Cols));
            Transform s = snacks[i];
            // la spirale gira, la barretta avanza e si incastra anche lei...
            V68StuckSnackSound.Play(this, "motor", 0.6f);
            yield return Turn(i, 0.55f, k => StuckPose(s, i, k));
            V68StuckSnackSound.Play(this, "clack", 0.4f);
            toast.Show(ErrorLines[errorLines % ErrorLines.Length], 1.4f);
            for (float k = 0f; k < 0.6f; k += Time.deltaTime)
            {
                StuckPose(s, i, 1f + 0.15f * Mathf.Sin(k * 35f));
                yield return null;
            }
            // ...poi il distributore ci ripensa: torna a posto e restituisce la moneta
            SetDisplay("MACHINE ERROR");
            V64ASound.Play(this, "error", 0.6f);
            yield return Turn(i, 0.45f, k => StuckPose(s, i, 1f - k));
            s.localPosition = snackPos[i];
            s.localRotation = Quaternion.identity;
            s.localScale = snackScale[i];
            SetDisplay("REFUNDED");
            toast.Show(RefundLines[errorLines++ % RefundLines.Length], 1.6f);
            yield return Refund();
            busy = false;
        }

        /// <summary>La spirale "gira" (si stringe e si allarga) mentre f(k) muove la barretta.</summary>
        IEnumerator Turn(int i, float seconds, System.Action<float> f)
        {
            Transform c = coils[i];
            for (float k = 0f; k < 1f; k += Time.deltaTime / seconds)
            {
                float e = 1f - (1f - k) * (1f - k);
                c.localScale = new Vector3(coilScale[i].x * (1f - 0.12f * Mathf.Abs(Mathf.Sin(k * Mathf.PI * 3f))), coilScale[i].y, coilScale[i].z);
                f(e);
                yield return null;
            }
            c.localScale = coilScale[i];
            f(1f);
        }

        /// <summary>La moneta ricade nella vaschetta del resto e torna in mano.</summary>
        IEnumerator Refund()
        {
            HalloweenKit.Show(coin, true);
            coin.position = changeCup.position;
            coin.localScale = coinScale * 0.6f;
            V68StuckSnackSound.Play(this, "clink", 0.6f);
            yield return new WaitForSeconds(0.25f);
            StartCoroutine(HalloweenKit.ScaleTo(coin, coinScale, 0.3f));
            yield return HalloweenKit.Move(coin, coinHome, 0.4f, 0.4f);
            coin.localScale = coinScale;
            coinItem.Enabled = true;
            StartCoroutine(HalloweenKit.Pop(palm, 1.06f, 0.2f));
            ShowDisplay();
        }

        // ---------- La soluzione ----------

        IEnumerator Drop()
        {
            finishing = true;
            CancelGrips();
            int a = SolutionIndex, b = StuckIndex;
            Transform sa = snacks[a], sb = snacks[b];
            // cadono davanti alle altre barrette
            if (snackArt[a] != null) snackArt[a].sortingOrder = snackOrder + 2;
            if (snackArt[b] != null) snackArt[b].sortingOrder = snackOrder + 1;
            SetDisplay("A3");
            V68StuckSnackSound.Play(this, "motor", 0.6f);
            yield return Turn(a, 0.5f, k => StuckPose(sa, a, k * 0.7f));
            // la A3 cade dritta sulla B3...
            Vector3 hit = snackPos[b] + new Vector3(0.05f, 0.35f, 0f);
            Vector3 from = sa.localPosition;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.22f)
            {
                sa.localPosition = Vector3.Lerp(from, hit, k * k);
                sa.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(-17f, -8f, k));
                yield return null;
            }
            V68StuckSnackSound.Play(this, "thud", 0.7f);
            Haptics.Light();
            // ...e se la porta giù: tutte e due nello sportello (dietro la parte bassa del distributore)
            Vector3 fa = sa.localPosition, fb = sb.localPosition;
            Quaternion rb = sb.localRotation;
            Vector3 down = new Vector3(0f, -2.6f, 0f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.4f)
            {
                float e = k * k;
                sa.localPosition = fa + down * e;
                sb.localPosition = fb + down * e * 0.97f;
                sa.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(-8f, 10f, k));
                sb.localRotation = Quaternion.Slerp(rb, Quaternion.Euler(0f, 0f, -35f), k);
                yield return null;
            }
            ShowSnack(a, false);
            ShowSnack(b, false);
            V68StuckSnackSound.Play(this, "thud", 0.9f);
            Haptics.Medium();
            if (dust != null) StartCoroutine(HalloweenKit.Burst(dust, 0.25f));
            StartCoroutine(HalloweenKit.Shake(flapPivot, 0.04f, 0.25f));
            yield return new WaitForSeconds(0.16f);
            V68StuckSnackSound.Play(this, "thud", 1f);
            Haptics.Medium();
            Wobble(machinePivot, 1.2f, 0.35f, 30f);
            toast.Show("Clunk. CLUNK.", 1.4f);
            logic.FinishDrop();
            SetDisplay("ENJOY!");
            HalloweenKit.Show(flapGlow, true);
            nagAt = Time.time + 5f;
            finishing = false;
        }

        void PulseFlap()
        {
            HalloweenKit.SetAlpha(flapGlow, 0.35f + 0.3f * Mathf.Sin(Time.time * 5f));
            if (nagAt > 0f && Time.time >= nagAt)
            {
                nagAt = Time.time + 6f;
                toast.Show("Your snacks are in the flap.", 1.6f);
                StartCoroutine(HalloweenKit.Shake(flapPivot, 0.03f, 0.3f));
            }
        }

        IEnumerator Finale()
        {
            finishing = true;
            CancelGrips();
            HalloweenKit.Show(flapGlow, false);
            yield return ArmIn();
            yield return new WaitForSeconds(0.2f);
            HalloweenKit.Show(loot, true);
            V68StuckSnackSound.Play(this, "rustle", 0.6f);
            // la mano esce con DUE barrette
            Vector3 up = armHome + new Vector3(0.3f, -0.85f, 0f);
            flapPivot.localScale = flapScale;
            yield return HalloweenKit.Move(arm, up, 0.35f);
            StartCoroutine(HalloweenKit.Pop(loot, 1.25f, 0.35f));
            toast.Show("TWO FOR THE PRICE OF ONE.", 2.2f);
            yield return Celebrate(1.4f);
            if (logic.TryComplete()) Win();
        }

        // ---------- Display ----------

        void ShowDisplay()
        {
            if (logic.Stage == SnackStage.InFlap) { SetDisplay("ENJOY!"); return; }
            if (logic.HasCredit) { SetDisplay("CREDIT 1\nCHOOSE"); return; }
            if (logic.HasPending) { SetDisplay(StuckSnackLogic.SlotName(logic.PendingRow, logic.PendingCol) + "\nINSERT COIN"); return; }
            SetDisplay("INSERT\nCOIN");
        }

        void SetDisplay(string s)
        {
            if (display == null) return;
            display.text = s;
            display.color = LedColor;
        }

        // ---------- Inizio: l'acquisto andato male ----------

        IEnumerator Intro()
        {
            int i = StuckIndex;
            Transform s = snacks[i];
            s.localPosition = snackPos[i];
            s.localRotation = Quaternion.identity;
            SetDisplay("B3");
            yield return new WaitForSeconds(0.35f);
            V68StuckSnackSound.Play(this, "motor", 0.6f);
            yield return Turn(i, 0.7f, k => StuckPose(s, i, k));
            V68StuckSnackSound.Play(this, "clack", 0.5f);
            for (float k = 0f; k < 0.5f; k += Time.deltaTime)
            {
                StuckPose(s, i, 1f + 0.12f * Mathf.Sin(k * 30f) * (1f - k / 0.5f));
                yield return null;
            }
            StuckPose(s, i, 1f);
            SetDisplay("ENJOY!");
            toast.Show("...and it's stuck.", 1.6f);
            yield return new WaitForSeconds(0.5f);
            ShowDisplay();
            introDone = true;
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
        }

#if UNITY_EDITOR
        // ---------- Tasti di prova nell'Editor ----------

        void EditorKeys()
        {
            if (Input.GetKeyDown(KeyCode.T) && logic.Stage == SnackStage.InFlap) { ReachFlap(); return; }
            if (logic.Stage != SnackStage.Stuck) return;
            if (Input.GetKeyDown(KeyCode.K))
            {
                // moneta + A3 (se la moneta è in mano, A3 resta in memoria e la moneta la compra)
                if (logic.CoinInWallet) { PressSlot(StuckSnackLogic.SolutionRow, StuckSnackLogic.SolutionCol); InsertCoin(); }
                else PressSlot(StuckSnackLogic.SolutionRow, StuckSnackLogic.SolutionCol);
                return;
            }
            if (Input.GetKeyDown(KeyCode.Alpha1)) Kick(bodyArea.bounds.center);
            if (Input.GetKeyDown(KeyCode.Alpha2)) ReachFlap();
            if (Input.GetKeyDown(KeyCode.Alpha3)) { PressSlot(StuckSnackLogic.StuckRow, StuckSnackLogic.StuckCol); if (logic.CoinInWallet && !busy) InsertCoin(); }
            if (Input.GetKeyDown(KeyCode.Alpha4)) { PressSlot(2, 0); if (logic.CoinInWallet && !busy) InsertCoin(); }
            if (Input.GetKeyDown(KeyCode.Alpha5)) TapChange();
        }
#endif
    }

    /// <summary>Suoni sintetizzati del livello 263: bip dei tasti, motorino della spirale, tonfi, moneta, falena, sacchetti.</summary>
    public static class V68StuckSnackSound
    {
        const int Rate = 22050;
        static readonly Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>();
        static readonly System.Random rnd = new System.Random(68);

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
                case "beep":     // tasto: bip corto
                    c = Make(kind, 0.08f, t => Mathf.Sign(Mathf.Sin(t * 2f * Mathf.PI * 1500f)) * 0.25f * Mathf.Min(1f, (0.08f - t) * 60f));
                    break;
                case "beep_hi":  // allarme dell'adesivo: bip acuto
                    c = Make(kind, 0.1f, t => Mathf.Sign(Mathf.Sin(t * 2f * Mathf.PI * 2600f)) * 0.22f * Mathf.Min(1f, (0.1f - t) * 50f));
                    break;
                case "motor":    // la spirale che gira: ronzio che sale
                    c = Make(kind, 0.6f, t => (Mathf.Sign(Mathf.Sin(t * 2f * Mathf.PI * (70f + t * 40f))) * 0.2f + N() * 0.06f) * Mathf.Min(1f, t * 20f) * Mathf.Min(1f, (0.6f - t) * 12f));
                    break;
                case "thud":     // barretta che cade sul fondo: tonfo sordo
                    c = Make(kind, 0.25f, t => (Mathf.Sin(t * 2f * Mathf.PI * Mathf.Lerp(120f, 55f, t / 0.25f)) * 0.9f + N() * 0.25f) * Mathf.Exp(-t * 18f));
                    break;
                case "clink":    // moneta: due tintinnii metallici
                    c = Make(kind, 0.35f, t => (Mathf.Sin(t * 2f * Mathf.PI * 3100f) + Mathf.Sin(t * 2f * Mathf.PI * 4650f) * 0.5f) * 0.3f
                        * (Mathf.Exp(-t * 30f) + (t > 0.12f ? Mathf.Exp(-(t - 0.12f) * 35f) * 0.7f : 0f)));
                    break;
                case "clack":    // sportello di plastica
                    c = Make(kind, 0.1f, t => (N() * 0.5f + Mathf.Sin(t * 2f * Mathf.PI * 600f) * 0.3f) * Mathf.Exp(-t * 60f));
                    break;
                case "rattle":   // il distributore che dondola: vetri e barrette che ballano
                    c = Make(kind, 1.1f, t => N() * 0.35f * (Mathf.Sin(t * 2f * Mathf.PI * 7f) > 0.3f ? 1f : 0.3f) * Mathf.Exp(-t * 1.8f));
                    break;
                case "flutter":  // ali della falena
                    c = Make(kind, 0.8f, t => N() * 0.25f * Mathf.Abs(Mathf.Sin(t * 2f * Mathf.PI * 18f)) * Mathf.Min(1f, (0.8f - t) * 5f));
                    break;
                case "rustle":   // incarti
                    {
                        float lp = 0f;
                        c = Make(kind, 0.5f, t => { lp += (N() - lp) * 0.6f; return lp * 0.5f * Mathf.Abs(Mathf.Sin(t * 2f * Mathf.PI * 9f)); });
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
            AudioClip clip = AudioClip.Create("v68_" + name, n, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
