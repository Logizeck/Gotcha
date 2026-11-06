using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels.V64;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 54 della roadmap "Luck Hack" (id luck_hack) — "FLIP HEADS." Una moneta sul panno verde di un tavolo da
    /// gioco e un grosso pulsante FLIP. Lanciata normalmente atterra SEMPRE su TAILS ("TAILS. Again.", "TAILS.
    /// Shocking."...); toccarla in volo la fa cadere subito su TAILS ("You touched it. TAILS."); toccata da ferma gira sul
    /// tavolo e resta TAILS. Tutto reversibile, mai una sconfitta.
    /// Soluzione: premere FLIP e, mentre la moneta è in aria (volo deterministico di 2 s + 0.3 s di tolleranza in cui
    /// traballa sul tavolo), capovolgere davvero il telefono: la moneta atterra (o si rigira) su HEADS, scintilla, festa
    /// comune "YOU CHEATED LUCK!" -> Win una volta sola. Il mondo NON ruota.
    /// Gravità: Inputs.Tilt filtrata da LuckHackLogic (V64AGravity, stesse soglie/isteresi del gatto aristocratico). Se
    /// al FLIP il telefono è già capovolto (o quasi) la moneta non parte: "Hold the phone normally to flip." (così un
    /// capovolgimento iniziato prima del lancio non vale mai). Durante il volo: "Flipping..." sul bordo del tavolo e
    /// l'ombra della moneta che si allarga e si stringe (segnale senza suggerimenti, dà il tempo di girare il telefono).
    /// Dito: HalloweenKit.ReadPointer (un dito; contano solo i tocchi, niente trascinamenti; due dita = nessun tocco).
    /// Pausa, app in background o livello spento: la moneta torna ferma su TAILS, niente conta.
    /// Tasti di prova nell'Editor: U capovolge/raddrizza il telefono (simulato), J inclinazione a metà strada (vicino
    /// alla soglia, non vale), F o Spazio premono FLIP, T tocca la moneta, H soluzione completa (FLIP + capovolto),
    /// P pausa simulata, R ricomincia.
    /// </summary>
    public class V83LuckHackLevel : LevelController
    {
        [Header("La moneta (radice al centro della moneta ferma)")]
        [SerializeField] Transform coin;
        [SerializeField] Transform coinSpin;        // si schiaccia (giravolta) e oscilla; dentro le due facce e il taglio
        [SerializeField] Transform faceHeads;
        [SerializeField] Transform faceTails;
        [SerializeField] Transform coinEdge;
        [SerializeField] Collider2D coinArea;       // sulla radice: segue la moneta in volo (generoso)
        [SerializeField] SpriteRenderer shadow;
        [SerializeField] SpriteRenderer coinGlow;
        [SerializeField] SpriteRenderer sparkle;
        [Header("Il pulsante FLIP")]
        [SerializeField] Transform flipButton;
        [SerializeField] Transform flipBody;
        [SerializeField] Collider2D flipArea;
        [Header("Scritte e festa")]
        [SerializeField] TextMesh cueText;          // "Flipping..." durante il volo
        [SerializeField] Transform cueHolder;
        [SerializeField] TextMesh scoreText;        // targhetta HEADS / TAILS sulla parete
        [SerializeField] TextMesh infoText;
        [SerializeField] Transform infoHolder;
        [SerializeField] LevelReward reward;
        [Header("Volo (coordinate del mondo)")]
        [SerializeField] float peakHeight = 3.25f;  // quanto sale il centro della moneta sopra il tavolo
        [SerializeField] int flightTurns = 6;       // giri completi in volo (deterministici)

        const string Compliment = "YOU CHEATED LUCK!";
        const string NeedUprightLine = "Hold the phone normally to flip.";
        const string CueLine = "Flipping...";
        static readonly string[] TailsLines = { "TAILS.", "TAILS. Again.", "TAILS. Shocking.", "TAILS. What are the odds?", "Still TAILS.", "TAILS. As always." };
        static readonly string[] TouchLines = { "You touched it. TAILS.", "Hands off! TAILS.", "Touched it again. TAILS." };
        static readonly string[] SpinLines = { "It spins... TAILS.", "Wheee... still TAILS.", "Spinning won't help. TAILS." };
        const float EdgeBelow = 0.16f, ShadowMin = 0.4f;

        LuckHackLogic logic;
        Camera cam;
        HwToast toast;
        Vector3 coinHome, shadowScale, flipScale, cueScale;
        Vector3 dropFrom;
        float shownAngle, dropAngle, flipPopT = 1f;
        int touchLines, spinLines, needLines;
        bool captured, finishing;
#if UNITY_EDITOR
        bool editorFlip, editorHalf;
#endif

        public override void OnLevelStart()
        {
            cam = Camera.main;
            StopAllCoroutines();
            if (!captured)
            {
                captured = true;
                logic = new LuckHackLogic();
                coinHome = coin.position;
                shadowScale = shadow != null ? shadow.transform.localScale : Vector3.one;
                flipScale = flipBody != null ? flipBody.localScale : Vector3.one;
                cueScale = cueHolder != null ? cueHolder.localScale : Vector3.one;
            }
            logic.Reset();
            if (reward != null) reward.ResetReward();
            finishing = false;
            touchLines = spinLines = needLines = 0;
            flipPopT = 1f;
            toast = new HwToast(infoText, infoHolder);
#if UNITY_EDITOR
            editorFlip = editorHalf = false;
#endif
            CoinRest();
            UpdateScore();
        }

        // ---------- Pose (sempre ricavate dalla logica: uno stato valido dopo pausa/spegnimento) ----------

        /// <summary>Moneta ferma sul tavolo, TAILS in su, niente effetti.</summary>
        void CoinRest()
        {
            shownAngle = 0f;
            coin.position = coinHome;
            coin.localScale = Vector3.one;
            coinSpin.localRotation = Quaternion.identity;
            SetFlip(0f, false);
            if (shadow != null) { shadow.transform.localScale = shadowScale; HalloweenKit.SetAlpha(shadow, 0.55f); }
            if (coinGlow != null) HalloweenKit.SetAlpha(coinGlow, 0f);
            HalloweenKit.Show(sparkle, false);
            HalloweenKit.Show(cueHolder, false);
            if (flipBody != null) flipBody.localScale = flipScale;
        }

        /// <summary>
        /// Angolo della moneta (gradi): 0 = TAILS verso di noi, 180 = HEADS. sideways=false: gira attorno all'asse
        /// orizzontale (lancio: si schiaccia in altezza); true: attorno a quello verticale (giravolta sul tavolo).
        /// </summary>
        void SetFlip(float angle, bool sideways)
        {
            float c = Mathf.Cos(angle * Mathf.Deg2Rad);
            float a = Mathf.Abs(c);
            bool tailsUp = c >= 0f;
            bool edge = a < EdgeBelow;
            HalloweenKit.Show(faceTails, !edge && tailsUp);
            HalloweenKit.Show(faceHeads, !edge && !tailsUp);
            HalloweenKit.Show(coinEdge, edge);
            if (coinEdge != null) coinEdge.localRotation = Quaternion.Euler(0f, 0f, sideways ? 90f : 0f);
            float s = Mathf.Max(a, EdgeBelow);
            Vector3 face = sideways ? new Vector3(s, 1f, 1f) : new Vector3(1f, s, 1f);
            if (faceTails != null) faceTails.localScale = face;
            if (faceHeads != null) faceHeads.localScale = face;
        }

        void UpdateScore()
        {
            if (scoreText == null || logic == null) return;
            int tails = logic.Flips + logic.Touches + logic.Spins;
            scoreText.text = "HEADS " + (logic.State == LuckHackState.Won ? 1 : 0) + "   TAILS " + tails;
        }

        // ---------- Ciclo di vita ----------

        void OnDisable()
        {
            // pausa/pubblicità/livello spento: la moneta torna ferma su TAILS, niente conta
            CancelAll();
            if (captured && !finishing && logic != null && !IsFinished && logic.State != LuckHackState.Won)
            {
                CoinRest();
                HalloweenKit.Show(infoHolder, false);
            }
        }

        void OnEnable()
        {
            if (!captured || IsFinished || logic == null) return;
            // spento a metà della festa: la si rifà da capo (la vittoria resta una sola, la logica fa da guardia)
            if (logic.State == LuckHackState.Won) { finishing = false; StartCoroutine(Finale(false)); }
        }

        void OnApplicationPause(bool paused) { if (paused) CancelAll(); }
        void OnApplicationFocus(bool focus) { if (!focus) CancelAll(); }

        /// <summary>Il volo si annulla (la moneta torna ferma su TAILS) e il filtro della gravità riparte.</summary>
        void CancelAll()
        {
#if UNITY_EDITOR
            editorFlip = editorHalf = false;
#endif
            if (logic == null || logic.InputLocked) return;
            bool wasMoving = logic.State != LuckHackState.Idle;
            logic.Cancel();
            if (wasMoving && captured && coin != null) CoinRest();
        }

        void Update()
        {
            toast?.Tick();
            if (logic == null || IsFinished || finishing || toast == null) return;
            if (Time.timeScale <= 0f) { CancelAll(); return; }   // pausa o pubblicità: niente input né sensori
#if UNITY_EDITOR
            EditorKeys();
            if (finishing) return;
#endif
            float dt = Time.deltaTime;
            Vector2 g = Gravity();
            React(logic.Tick(dt, g.x, g.y));
            if (finishing) return;
            ReadPointer();
            if (finishing) return;
            Pose(dt);
        }

        /// <summary>Gravità nello schermo (in g): accelerometro; nell'Editor anche i tasti U/J.</summary>
        Vector2 Gravity()
        {
            Vector2 g = Inputs.Tilt();
#if UNITY_EDITOR
            if (editorFlip) g = new Vector2(0f, 1f);
            else if (editorHalf) g = new Vector2(0f, 0.45f);
#endif
            return g;
        }

        // ---------- Dito (solo tocchi) ----------

        void ReadPointer()
        {
            if (!HalloweenKit.ReadPointer(cam, out Vector2 w, out bool began, out bool _)) return;
            if (began) Tap(w);
        }

        static bool Over(Collider2D c, Vector2 p) => c != null && c.gameObject.activeInHierarchy && c.OverlapPoint(p);

        void Tap(Vector2 w)
        {
            if (logic.InputLocked || finishing) return;
            if (Over(coinArea, w)) { TouchCoin(); return; }
            if (Over(flipArea, w)) PressFlip();
        }

        void TouchCoin()
        {
            LuckHackEvent e = logic.TouchCoin();
            if (e == LuckHackEvent.Touched) { dropFrom = coin.position; dropAngle = shownAngle; }
            React(e);
        }

        void PressFlip()
        {
            flipPopT = 0f;   // il pulsante si abbassa sempre (anche quando il lancio è ignorato)
            Sfx.Play(this, "tick", 0.35f);
            React(logic.PressFlip());
        }

        // ---------- Reazioni ----------

        void React(LuckHackEvent e)
        {
            switch (e)
            {
                case LuckHackEvent.Flipped:
                    shownAngle = 0f;
                    coinSpin.localRotation = Quaternion.identity;
                    if (cueText != null) cueText.text = CueLine;
                    HalloweenKit.Show(cueHolder, true);
                    HalloweenKit.Show(infoHolder, false);
                    V83LuckHackSound.Play(this, "flip", 0.5f);
                    Haptics.Tap();
                    break;
                case LuckHackEvent.NeedUpright:
                    toast.Show(NeedUprightLine, 2.0f);
                    if (needLines++ == 0) Haptics.Light();
                    V83LuckHackSound.Play(this, "buzz", 0.35f);
                    break;
                case LuckHackEvent.Hacked:
                    // la moneta "sente" il mondo girarsi: un luccichio (nessuna parola)
                    V83LuckHackSound.Play(this, "ting", 0.45f);
                    StartCoroutine(Glint());
                    break;
                case LuckHackEvent.TouchedDown:
                    V83LuckHackSound.Play(this, "clink", 0.55f);
                    Haptics.Tap();
                    break;
                case LuckHackEvent.Tails:
                    HalloweenKit.Show(cueHolder, false);
                    toast.Show(TailsLines[(logic.Flips - 1) % TailsLines.Length], 1.8f);
                    UpdateScore();
                    break;
                case LuckHackEvent.Touched:
                    HalloweenKit.Show(cueHolder, false);
                    toast.Show(TouchLines[touchLines++ % TouchLines.Length], 1.8f);
                    V83LuckHackSound.Play(this, "whoosh", 0.3f);
                    Haptics.Light();
                    UpdateScore();
                    break;
                case LuckHackEvent.Fell:
                    V83LuckHackSound.Play(this, "clink", 0.5f);
                    break;
                case LuckHackEvent.Spun:
                    toast.Show(SpinLines[spinLines++ % SpinLines.Length], 1.6f);
                    V83LuckHackSound.Play(this, "whirr", 0.45f);
                    UpdateScore();
                    break;
                case LuckHackEvent.LandedHeads:
                    StartCoroutine(Finale(false));
                    break;
                case LuckHackEvent.FlippedOver:
                    StartCoroutine(Finale(true));
                    break;
            }
        }

        // ---------- La moneta che vola, gira, traballa ----------

        void Pose(float dt)
        {
            float t = logic.StateTime;
            // il pulsante si abbassa e torna su
            if (flipBody != null)
            {
                if (flipPopT < 1f) flipPopT = Mathf.Min(1f, flipPopT + dt / 0.18f);
                float press = flipPopT < 1f ? Mathf.Sin(flipPopT * Mathf.PI) : 0f;
                flipBody.localScale = new Vector3(flipScale.x * (1f + 0.06f * press), flipScale.y * (1f - 0.16f * press), flipScale.z);
            }
            switch (logic.State)
            {
                case LuckHackState.Idle:
                    coin.position = coinHome;
                    coinSpin.localRotation = Quaternion.identity;
                    shownAngle = 0f;
                    SetFlip(0f, false);
                    ShadowAt(0f);
                    HalloweenKit.Show(cueHolder, false);
                    break;
                case LuckHackState.Spinning:
                {
                    // giravolta sul tavolo: rallenta e si ferma su TAILS (4 giri)
                    float k = Mathf.Clamp01(t / LuckHackLogic.SpinTime);
                    float ease = 1f - (1f - k) * (1f - k);
                    coin.position = coinHome;
                    SetFlip(360f * 4f * ease, true);
                    coinSpin.localRotation = Quaternion.Euler(0f, 0f, 4f * Mathf.Sin(t * 30f) * (1f - k));
                    ShadowAt(0f);
                    break;
                }
                case LuckHackState.Flying:
                {
                    float k = logic.Flight01;
                    float h = 4f * k * (1f - k);
                    coin.position = coinHome + new Vector3(0f, peakHeight * h, 0f);
                    // gira con velocità costante fino all'angolo d'arrivo: TAILS, oppure HEADS (mezzo giro in più) se "truccata"
                    float target = 360f * flightTurns + (logic.Hacked ? 180f : 0f);
                    float rem = LuckHackLogic.FlightTime - t;
                    shownAngle = rem <= dt ? target : shownAngle + (target - shownAngle) * Mathf.Clamp01(dt / Mathf.Max(rem, 1e-4f));
                    SetFlip(shownAngle, false);
                    coinSpin.localRotation = Quaternion.Euler(0f, 0f, 10f * Mathf.Sin(k * Mathf.PI * 3f));
                    ShadowAt(h);
                    PulseCue(t);
                    break;
                }
                case LuckHackState.Settling:
                {
                    // atterrata su TAILS: traballa
                    float s = t - LuckHackLogic.FlightTime;
                    float k = Mathf.Clamp01(s / LuckHackLogic.GraceTime);
                    coin.position = coinHome;
                    shownAngle = 360f * flightTurns;
                    SetFlip(0f, false);
                    coinSpin.localRotation = Quaternion.Euler(0f, 0f, 9f * Mathf.Sin(s * 45f) * (1f - k));
                    ShadowAt(0f);
                    PulseCue(t);
                    break;
                }
                case LuckHackState.Falling:
                {
                    // toccata: cade dritta sul tavolo e si mette su TAILS
                    float k = Mathf.Clamp01(t / LuckHackLogic.DropTime);
                    coin.position = Vector3.Lerp(dropFrom, coinHome, k * k);
                    float rest = Mathf.Ceil(dropAngle / 360f) * 360f;
                    shownAngle = Mathf.Lerp(dropAngle, rest, k);
                    SetFlip(shownAngle, false);
                    coinSpin.localRotation = Quaternion.Euler(0f, 0f, 20f * (1f - k) * Mathf.Sin(t * 50f));
                    ShadowAt(Mathf.Clamp01((coin.position.y - coinHome.y) / Mathf.Max(0.01f, peakHeight)));
                    break;
                }
            }
        }

        /// <summary>Ombra sul tavolo: più piccola e chiara quanto più la moneta è in alto (h 0..1).</summary>
        void ShadowAt(float h)
        {
            if (shadow == null) return;
            float k = Mathf.Lerp(1f, ShadowMin, Mathf.Clamp01(h));
            shadow.transform.localScale = new Vector3(shadowScale.x * k, shadowScale.y * k, shadowScale.z);
            HalloweenKit.SetAlpha(shadow, Mathf.Lerp(0.55f, 0.2f, Mathf.Clamp01(h)));
        }

        void PulseCue(float t)
        {
            if (cueHolder == null) return;
            HalloweenKit.Show(cueHolder, true);
            cueHolder.localScale = cueScale * (1f + 0.05f * Mathf.Sin(t * 8f));
        }

        IEnumerator Glint()
        {
            if (coinGlow == null) yield break;
            for (float t = 0f; t < 0.4f && !finishing; t += Time.deltaTime)
            {
                coinGlow.transform.position = new Vector3(coin.position.x, coin.position.y, coinGlow.transform.position.z);
                HalloweenKit.SetAlpha(coinGlow, 0.7f * Mathf.Sin(t / 0.4f * Mathf.PI));
                yield return null;
            }
            if (!finishing) HalloweenKit.SetAlpha(coinGlow, 0f);
        }

        // ---------- Vittoria ----------

        /// <summary>
        /// HEADS: (se capovolta durante la tolleranza la moneta prima saltella e si rigira) atterra con una scintilla,
        /// saltella contenta due volte nel bagliore, "HEADS!"; festa comune; Win una volta sola.
        /// </summary>
        IEnumerator Finale(bool flipOver)
        {
            if (finishing) yield break;
            finishing = true;
            HalloweenKit.Show(infoHolder, false);
            HalloweenKit.Show(cueHolder, false);
            coin.localScale = Vector3.one;
            if (flipOver)
            {
                // 1a) rigirata sul tavolo: piccolo salto e mezzo giro
                V83LuckHackSound.Play(this, "whoosh", 0.35f);
                for (float t = 0f; t < 0.35f; t += Time.deltaTime)
                {
                    float k = Mathf.Clamp01(t / 0.35f);
                    float h = 4f * k * (1f - k);
                    coin.position = coinHome + new Vector3(0f, 0.7f * h, 0f);
                    coinSpin.localRotation = Quaternion.identity;
                    SetFlip(180f * k, false);
                    ShadowAt(h * 0.25f);
                    yield return null;
                }
            }
            // 1) giù su HEADS
            coin.position = coinHome;
            coinSpin.localRotation = Quaternion.identity;
            SetFlip(180f, false);
            ShadowAt(0f);
            V83LuckHackSound.Play(this, "clink", 0.6f);
            Haptics.Medium();
            UpdateScore();
            toast.Show("HEADS!", 1.4f);
            // 2) scintilla e saltelli contenti nel bagliore
            V83LuckHackSound.Play(this, "ting", 0.55f);
            HalloweenKit.Show(sparkle, true);
            Vector3 sparkleAt = coinHome + new Vector3(0.55f, 0.55f, 0f);
            for (float t = 0f; t < 0.9f; t += Time.deltaTime)
            {
                float k = Mathf.Clamp01(t / 0.9f);
                float hop = Mathf.Abs(Mathf.Sin(k * Mathf.PI * 2f)) * 0.35f * (1f - 0.4f * k);
                coin.position = coinHome + new Vector3(0f, hop, 0f);
                float squash = hop < 0.03f ? 0.12f : 0f;
                coin.localScale = new Vector3(1f + squash, 1f - squash, 1f);
                coinSpin.localRotation = Quaternion.Euler(0f, 0f, 8f * Mathf.Sin(k * Mathf.PI * 4f));
                ShadowAt(hop / Mathf.Max(0.01f, peakHeight));
                if (coinGlow != null)
                {
                    coinGlow.transform.position = new Vector3(coin.position.x, coin.position.y, coinGlow.transform.position.z);
                    HalloweenKit.SetAlpha(coinGlow, 0.6f * Mathf.Clamp01(t * 5f));
                    coinGlow.transform.localScale = Vector3.one * (2.4f + 0.3f * Mathf.Sin(t * 10f));
                }
                if (sparkle != null)
                {
                    sparkle.transform.position = new Vector3(sparkleAt.x, sparkleAt.y + hop, sparkle.transform.position.z);
                    sparkle.transform.localScale = Vector3.one * (0.6f + 0.6f * Mathf.Abs(Mathf.Sin(t * 8f)));
                    sparkle.transform.localRotation = Quaternion.Euler(0f, 0f, t * 240f);
                }
                yield return null;
            }
            coin.position = coinHome;
            coin.localScale = Vector3.one;
            coinSpin.localRotation = Quaternion.identity;
            HalloweenKit.Show(sparkle, false);
            if (reward != null) yield return reward.Play(Compliment);
            else yield return new WaitForSeconds(1.5f);
            if (!IsFinished && logic.TryComplete()) Win(Compliment);
        }

#if UNITY_EDITOR
        // ---------- Tasti di prova nell'Editor ----------

        void EditorKeys()
        {
            if (logic.InputLocked) return;
            if (Input.GetKeyDown(KeyCode.U)) { editorFlip = !editorFlip; editorHalf = false; }
            if (Input.GetKeyDown(KeyCode.J)) { editorHalf = !editorHalf; editorFlip = false; }
            if (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.Space)) PressFlip();
            if (Input.GetKeyDown(KeyCode.T)) TouchCoin();
            if (Input.GetKeyDown(KeyCode.H))
            {
                // soluzione: telefono dritto, FLIP, poi capovolto durante il volo
                editorFlip = editorHalf = false;
                logic.Tick(0f, 0f, -1f);
                if (logic.State == LuckHackState.Idle || logic.State == LuckHackState.Spinning) PressFlip();
                if (logic.WindowOpen) editorFlip = true;
            }
            if (Input.GetKeyDown(KeyCode.P)) CancelAll();
            if (Input.GetKeyDown(KeyCode.R)) OnLevelStart();
        }
#endif
    }

    /// <summary>Suoni sintetizzati di Luck Hack: lancio (pollice sulla moneta), tintinnio d'atterraggio, ronzio della giravolta, luccichio, errore.</summary>
    public static class V83LuckHackSound
    {
        const int Rate = 22050;
        static readonly Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>();
        static readonly System.Random rnd = new System.Random(541);

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
                case "flip":    // pollice sulla moneta: "tic" metallico che sale
                    c = Make(kind, 0.35f, t => Mathf.Sin(t * 2f * Mathf.PI * Mathf.Lerp(2200f, 3400f, t / 0.35f)) * 0.22f * Mathf.Exp(-t * 9f) + N() * 0.2f * Mathf.Exp(-t * 60f));
                    break;
                case "clink":   // moneta che tocca il tavolo: due parziali metalliche
                    c = Make(kind, 0.45f, t => (Mathf.Sin(t * 2f * Mathf.PI * 2637f) * 0.6f + Mathf.Sin(t * 2f * Mathf.PI * 4186f) * 0.35f) * 0.3f * Mathf.Exp(-t * 11f));
                    break;
                case "whirr":   // giravolta sul tavolo: tintinnio che accelera
                    c = Make(kind, 0.85f, t =>
                    {
                        float f = Mathf.Lerp(9f, 30f, t / 0.85f);
                        float tick = Mathf.Exp(-((t * f) % 1f) * 9f);
                        return Mathf.Sin(t * 2f * Mathf.PI * 3000f) * tick * 0.18f * (1f - t / 0.85f);
                    });
                    break;
                case "ting":    // luccichio
                    c = Make(kind, 0.5f, t => (Mathf.Sin(t * 2f * Mathf.PI * 1760f) + 0.5f * Mathf.Sin(t * 2f * Mathf.PI * 2637f)) * 0.18f * Mathf.Exp(-t * 7f));
                    break;
                case "buzz":    // "no"
                    c = Make(kind, 0.25f, t => (2f * ((t * 130f) % 1f) - 1f) * 0.18f * Mathf.Min(1f, t * 80f) * Mathf.Clamp01((0.25f - t) * 20f));
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
            AudioClip clip = AudioClip.Create("v83b_" + name, n, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
