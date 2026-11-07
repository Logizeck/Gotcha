using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels.V64;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 101 della roadmap "The Upside-Down Rain" (id upside_down_rain) — "KEEP HIM DRY." Un omino triste sotto un
    /// ombrello pieno di buchi e una nuvola nera: la pioggia passa dai buchi e gli cade in testa (misuratore WET, gocce
    /// addosso, pozzanghera, "Brrr!"). Inclinare l'ombrello col dito non serve (i buchi viaggiano con lui), tappare i buchi
    /// col dito nemmeno (la goccia passa da un altro buco: "There are too many holes."), toccare l'omino o la nuvola
    /// neppure. A bagnato pieno: battuta, si scrolla come un cane e si asciuga un po' (nessun Fail, tutto reversibile).
    /// Soluzione: capovolgere il telefono (UpsideDownRainLogic: V64AGravity con soglie/isteresi/armamento della clessidra
    /// bugiarda e di Luck Hack). Le gocce invertono la traiettoria e tornano su nella nuvola, che se le "risucchia"; se
    /// l'omino resta asciutto 1.5 s col telefono capovolto: la nuvola se ne va, esce il sole, l'omino chiude l'ombrello e
    /// sorride; festa comune "YOU FIXED THE WEATHER!" -> Win una volta sola. Raddrizzato prima: riprende a piovere.
    /// La UI non ruota mai (scritte, misuratore e fumetti restano dritti nello schermo). Pioggia: gocce in una piscina
    /// fissa con moto scritto (niente fisica); la goccia che colpisce la testa segue la logica (DripProgress).
    /// Dito: HalloweenKit.ReadPointer (un dito; con due dita il gesto si annulla senza conseguenze). Pausa, app in
    /// background o livello spento: presa annullata, ombrello dritto, tenuta azzerata, il telefono va rivisto dritto.
    /// Tasti di prova nell'Editor: U capovolge/raddrizza il telefono (simulato), J inclinazione a metà strada (non vale),
    /// M sposta l'ombrello, O tappa un buco, B tocca l'omino, C tocca la nuvola, H soluzione (capovolto), P pausa simulata,
    /// R ricomincia.
    /// </summary>
    public class V86UpsideDownRainLevel : LevelController
    {
        [Header("Il cielo")]
        [SerializeField] Transform cloud;
        [SerializeField] SpriteRenderer cloudArt;
        [SerializeField] Collider2D cloudArea;
        [SerializeField] Transform sun;
        [SerializeField] SpriteRenderer sunGlow;
        [Header("L'ombrello (origine alla mano: inclinarlo lo ruota attorno alla mano)")]
        [SerializeField] Transform umbrella;
        [SerializeField] Transform umbrellaOpen;
        [SerializeField] Transform umbrellaFolded;
        [SerializeField] Collider2D canopyArea;
        [SerializeField] Transform[] holes;           // centri dei buchi (figli dell'ombrello); i primi tre sopra la testa
        [SerializeField] Transform[] holeGlows;       // evidenziazione del buco sotto il dito (nascosta)
        [SerializeField] float rimY = 1.45f, canopyHx = 1.35f, canopyH = 0.9f, holeR = 0.11f;
        [Header("L'omino")]
        [SerializeField] Transform man;
        [SerializeField] Collider2D manArea;
        [SerializeField] Transform headTop;
        [SerializeField] Transform faceSad;
        [SerializeField] Transform faceShiver;
        [SerializeField] Transform faceHappy;
        [SerializeField] SpriteRenderer drips;
        [SerializeField] SpriteRenderer puddle;
        [SerializeField] SpeechBubble manBubble;
        [Header("La pioggia (piscina fissa, moto scritto)")]
        [SerializeField] Transform[] drops;
        [SerializeField] Transform dripDrop;          // la goccia che passa dal buco e colpisce la testa (segue la logica)
        [SerializeField] Transform[] splashes;
        [SerializeField] float groundY = -2.7f, cloudBottom = 1.8f;
        [Header("Misuratore WET, scritte e festa")]
        [SerializeField] Transform meterFill;         // scala x = bagnato * meterWidth (ancorato a sinistra)
        [SerializeField] SpriteRenderer meterFillArt;
        [SerializeField] Transform meter;
        [SerializeField] float meterWidth = 3.3f;
        [SerializeField] TextMesh infoText;
        [SerializeField] Transform infoHolder;
        [SerializeField] LevelReward reward;

        const string Compliment = "YOU FIXED THE WEATHER!";
        const float MaxTilt = 30f, TiltPerUnit = 32f, DragStart = 0.2f, MovedTilt = 8f, HoleTap = 0.34f;
        const float RainRate = 13f, RiseRate = 15f, FallSpeed = 7f, RiseSpeed = 8f;
        static readonly string[] MoveLines = { "Still leaking.", "The holes travel with the umbrella.", "Rain doesn't care where you point it." };
        static readonly string[] PlugLines = { "There are too many holes.", "The drop just used another hole.", "You'd need forty fingers." };
        static readonly string[] CloudLines = { "The cloud ignores you.", "Clouds don't take requests.", "It's raining harder out of spite." };
        static readonly string[] ManLines = { "Brrr!", "I'm s-s-soaked.", "Poking me won't dry me." };
        static readonly string[] HitLines = { "Brrr!", "Achoo!", "Drip. Drip. Drip." };
        static readonly string[] SoakLines = { "Maximum soggy.", "I can't get any wetter!", "I'm basically a sponge." };
        static readonly string[] ShookLines = { "Shook it off. A bit.", "Okay. Slightly less soggy.", "Here it comes again." };
        static readonly string[] StopLines = { "And down it comes again.", "Gravity is back. Sadly.", "Rain resumes. As usual." };

        static readonly Color MeterDry = new Color(0.56f, 0.82f, 1f), MeterWet = new Color(0.2f, 0.45f, 1f);

        sealed class Drop
        {
            public Transform T;
            public Vector2 P;
            public float Vy;
            public bool On, Through;
        }

        UpsideDownRainLogic logic;
        Camera cam;
        HwToast toast;
        HwSay say;
        Drop[] pool;
        float[] splashT;
        int nextSplash;
        Vector3 cloudHome, sunHome, manHome, umbrellaHome, meterScale, cloudScale, sunScale;
        Color cloudColor = Color.white;
        float spawnAcc, tilt, shiverT, cloudPopT = 1f, cloudShakeT = 1f, meterPopT = 1f, wiggleT = 1f, soakShakeT = 1f;
        int moveLines, plugLines, cloudLines, manLines, hitLines, soakLines, shookLines, stopLines;
        Vector2 dripFrom;
        bool captured, finishing;
        // dito
        enum TouchMode { None, Pending, Drag, Plug }
        TouchMode mode;
        float touchStartX, maxTiltSeen;
        int plugIndex = -1;
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
                logic = new UpsideDownRainLogic();
                cloudHome = cloud != null ? cloud.localPosition : Vector3.zero;
                cloudScale = cloud != null ? cloud.localScale : Vector3.one;
                cloudColor = cloudArt != null ? cloudArt.color : Color.white;
                sunHome = sun != null ? sun.localPosition : Vector3.zero;
                sunScale = sun != null ? sun.localScale : Vector3.one;
                manHome = man != null ? man.localPosition : Vector3.zero;
                umbrellaHome = umbrella != null ? umbrella.localPosition : Vector3.zero;
                meterScale = meter != null ? meter.localScale : Vector3.one;
                int n = drops != null ? drops.Length : 0;
                pool = new Drop[n];
                for (int i = 0; i < n; i++) pool[i] = new Drop { T = drops[i] };
                splashT = new float[splashes != null ? splashes.Length : 0];
            }
            logic.Reset();
            if (reward != null) reward.ResetReward();
            finishing = false;
            moveLines = plugLines = cloudLines = manLines = hitLines = soakLines = shookLines = stopLines = 0;
            toast = new HwToast(infoText, infoHolder);
            say = new HwSay(manBubble);
            say.Hide();
#if UNITY_EDITOR
            editorFlip = editorHalf = false;
#endif
            RestPose();
        }

        // ---------- Pose (sempre ricavate dalla logica: uno stato valido dopo pausa/spegnimento) ----------

        /// <summary>Tutto a riposo: ombrello aperto e dritto, nuvola al suo posto, sole nascosto, niente gocce in volo.</summary>
        void RestPose()
        {
            mode = TouchMode.None;
            plugIndex = -1;
            tilt = maxTiltSeen = 0f;
            spawnAcc = 0f;
            shiverT = 0f;
            cloudPopT = cloudShakeT = meterPopT = wiggleT = soakShakeT = 1f;
            if (umbrella != null) { umbrella.localPosition = umbrellaHome; umbrella.localRotation = Quaternion.identity; }
            HalloweenKit.Show(umbrellaOpen, true);
            HalloweenKit.Show(umbrellaFolded, false);
            if (umbrellaOpen != null) umbrellaOpen.localScale = Vector3.one;
            if (holeGlows != null) foreach (Transform g in holeGlows) HalloweenKit.Show(g, false);
            if (cloud != null) { cloud.localPosition = cloudHome; cloud.localScale = cloudScale; }
            if (cloudArt != null) cloudArt.color = cloudColor;
            if (sun != null) { sun.localPosition = sunHome; sun.localScale = sunScale; HalloweenKit.Show(sun, false); }
            if (sunGlow != null) HalloweenKit.SetAlpha(sunGlow, 0f);
            if (man != null) { man.localPosition = manHome; man.localRotation = Quaternion.identity; }
            if (meter != null) meter.localScale = meterScale;
            ClearRain();
            SetFace();
            ShowWetness(logic.Wetness);
        }

        void ClearRain()
        {
            if (pool != null) foreach (Drop d in pool) { d.On = false; HalloweenKit.Show(d.T, false); }
            HalloweenKit.Show(dripDrop, false);
            if (splashes != null) for (int i = 0; i < splashes.Length; i++) { splashT[i] = 0f; HalloweenKit.Show(splashes[i], false); }
        }

        void SetFace()
        {
            bool happy = finishing || (logic != null && (logic.Reversing || logic.State == UpsideDownRainState.Won));
            bool shiver = !happy && (shiverT > 0f || (logic != null && logic.State == UpsideDownRainState.Soaked));
            HalloweenKit.Show(faceHappy, happy);
            HalloweenKit.Show(faceShiver, shiver);
            HalloweenKit.Show(faceSad, !happy && !shiver);
        }

        /// <summary>Misuratore, gocce addosso e pozzanghera seguono il bagnato.</summary>
        void ShowWetness(float w)
        {
            w = Mathf.Clamp01(w);
            if (meterFill != null) meterFill.localScale = new Vector3(Mathf.Max(0.001f, w * meterWidth), 1f, 1f);
            if (meterFillArt != null) meterFillArt.color = Color.Lerp(MeterDry, MeterWet, w);
            if (drips != null) HalloweenKit.SetAlpha(drips, Mathf.Clamp01((w - 0.15f) / 0.6f));
            if (puddle != null)
            {
                puddle.transform.localScale = new Vector3(0.22f + 0.4f * w, 0.5f + 0.4f * w, 1f);
                HalloweenKit.SetAlpha(puddle, 0.35f + 0.6f * w);
            }
        }

        // ---------- Ciclo di vita ----------

        void OnDisable()
        {
            CancelAll();
            if (captured && !finishing && logic != null && !IsFinished && logic.State != UpsideDownRainState.Won)
            {
                RestPose();
                HalloweenKit.Show(infoHolder, false);
                say?.Hide();
            }
        }

        void OnEnable()
        {
            if (!captured || IsFinished || logic == null) return;
            // spento a metà della festa: la si rifà da capo (la vittoria resta una sola, la logica fa da guardia)
            if (logic.State == UpsideDownRainState.Won) { finishing = false; StartCoroutine(Finale()); }
        }

        void OnApplicationPause(bool paused) { if (paused) CancelAll(); }
        void OnApplicationFocus(bool focus) { if (!focus) CancelAll(); }

        /// <summary>Presa del dito annullata (ombrello di nuovo dritto), tenuta capovolta azzerata, filtro ripartito.</summary>
        void CancelAll()
        {
#if UNITY_EDITOR
            editorFlip = editorHalf = false;
#endif
            EndTouch(false);
            if (logic == null || logic.InputLocked) return;
            logic.Cancel();
        }

        void Update()
        {
            toast?.Tick();
            say?.Tick();
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

        // ---------- Dito: inclinare l'ombrello, tappare i buchi, toccare omino e nuvola ----------

        static bool Over(Collider2D c, Vector2 p) => c != null && c.gameObject.activeInHierarchy && c.OverlapPoint(p);

        void ReadPointer()
        {
            if (!HalloweenKit.ReadPointer(cam, out Vector2 w, out bool began, out bool ended))
            {
                EndTouch(false);   // nessun dito o due dita: il gesto si annulla senza conseguenze
                return;
            }
            if (began) BeginTouch(w);
            else if (mode != TouchMode.None) MoveTouch(w);
            if (ended) EndTouch(true);
        }

        void BeginTouch(Vector2 w)
        {
            EndTouch(false);
            if (logic.InputLocked || finishing) return;
            if (Over(canopyArea, w))
            {
                touchStartX = w.x;
                maxTiltSeen = 0f;
                int h = NearestHole(w);
                if (h >= 0)
                {
                    mode = TouchMode.Plug;
                    plugIndex = h;
                    if (holeGlows != null && h < holeGlows.Length) HalloweenKit.Show(holeGlows[h], true);
                    Sfx.Play(this, "tick", 0.3f);
                    React(logic.PlugHole());
                }
                else mode = TouchMode.Pending;
                return;
            }
            if (Over(cloudArea, w)) { React(logic.TapCloud()); return; }
            if (Over(manArea, w)) React(logic.TapMan());
        }

        void MoveTouch(Vector2 w)
        {
            float dx = w.x - touchStartX;
            if (mode != TouchMode.Drag && Mathf.Abs(dx) > DragStart)
            {
                // il dito si muove: diventa un trascinamento (il buco tappato si libera)
                UnplugGlow();
                mode = TouchMode.Drag;
                Haptics.Tap();
            }
            if (mode == TouchMode.Drag)
            {
                tilt = Mathf.Clamp(-dx * TiltPerUnit, -MaxTilt, MaxTilt);   // dito a destra: la cupola va a destra
                maxTiltSeen = Mathf.Max(maxTiltSeen, Mathf.Abs(tilt));
            }
        }

        /// <summary>Fine del tocco: con <paramref name="real"/> il dito si è alzato davvero (altrimenti è un annullamento).</summary>
        void EndTouch(bool real)
        {
            TouchMode was = mode;
            mode = TouchMode.None;
            UnplugGlow();
            if (!real || logic == null || logic.InputLocked || finishing) return;
            if (was == TouchMode.Drag && maxTiltSeen >= MovedTilt) React(logic.MoveUmbrella());
            else if (was == TouchMode.Pending) { wiggleT = 0f; Sfx.Play(this, "tick", 0.25f); }   // tocco sulla tela: traballa
        }

        void UnplugGlow()
        {
            if (plugIndex >= 0 && holeGlows != null && plugIndex < holeGlows.Length) HalloweenKit.Show(holeGlows[plugIndex], false);
            plugIndex = -1;
        }

        int NearestHole(Vector2 w)
        {
            if (holes == null) return -1;
            int best = -1;
            float bd = HoleTap;
            for (int i = 0; i < holes.Length; i++)
            {
                if (holes[i] == null) continue;
                float d = Vector2.Distance(holes[i].position, w);
                if (d < bd) { bd = d; best = i; }
            }
            return best;
        }

        // ---------- Reazioni ----------

        void React(UpsideDownRainEvent e)
        {
            switch (e)
            {
                case UpsideDownRainEvent.Dripped:
                    StartDrip();
                    break;
                case UpsideDownRainEvent.Hit:
                    LandDrip();
                    if (logic.Hits % 3 == 1) say.Show(HitLines[hitLines++ % HitLines.Length], 1.4f);
                    break;
                case UpsideDownRainEvent.Soaked:
                    LandDrip();
                    say.Show(SoakLines[soakLines++ % SoakLines.Length], 2.2f);
                    V86UpsideDownRainSound.Play(this, "brr", 0.5f);
                    soakShakeT = 0f;
                    Haptics.Light();
                    break;
                case UpsideDownRainEvent.Shook:
                    say.Show(ShookLines[shookLines++ % ShookLines.Length], 1.8f);
                    break;
                case UpsideDownRainEvent.UmbrellaMoved:
                    toast.Show(MoveLines[moveLines++ % MoveLines.Length], 1.8f);
                    V86UpsideDownRainSound.Play(this, "buzz", 0.3f);
                    break;
                case UpsideDownRainEvent.HolePlugged:
                    toast.Show(PlugLines[plugLines++ % PlugLines.Length], 1.8f);
                    break;
                case UpsideDownRainEvent.ManTapped:
                    say.Show(ManLines[manLines++ % ManLines.Length], 1.6f);
                    V86UpsideDownRainSound.Play(this, "brr", 0.4f);
                    shiverT = 0.6f;
                    break;
                case UpsideDownRainEvent.CloudTapped:
                    toast.Show(CloudLines[cloudLines++ % CloudLines.Length], 1.8f);
                    V86UpsideDownRainSound.Play(this, "rumble", 0.45f);
                    cloudShakeT = 0f;
                    break;
                case UpsideDownRainEvent.Reversing:
                    StartReverse();
                    break;
                case UpsideDownRainEvent.ReverseStopped:
                    toast.Show(StopLines[stopLines++ % StopLines.Length], 1.6f);
                    FlipRain(false);
                    break;
                case UpsideDownRainEvent.Fixed:
                    StartCoroutine(Finale());
                    break;
            }
        }

        // ---------- La goccia che passa dal buco ----------

        /// <summary>Sceglie da quale buco passa la goccia: uno sopra la testa, mai quello tappato dal dito.</summary>
        void StartDrip()
        {
            Vector2 head = headTop != null ? (Vector2)headTop.position : Vector2.zero;
            dripFrom = new Vector2(head.x, cloudBottom);   // ombrello spostato del tutto: piove dritto dalla nuvola
            if (holes != null && holes.Length > 0)
            {
                var near = new List<int>();
                for (int i = 0; i < holes.Length; i++)
                    if (holes[i] != null && i != plugIndex && Mathf.Abs(holes[i].position.x - head.x) < 0.6f && holes[i].position.y > head.y) near.Add(i);
                if (near.Count > 0) dripFrom = holes[near[logic.Drips % near.Count]].position;
            }
            if (dripDrop != null) { dripDrop.position = new Vector3(dripFrom.x, dripFrom.y, dripDrop.position.z); HalloweenKit.Show(dripDrop, true); }
            V86UpsideDownRainSound.Play(this, "drip", 0.25f);
        }

        void LandDrip()
        {
            HalloweenKit.Show(dripDrop, false);
            if (headTop != null) Splash(headTop.position, 0.32f);
            V86UpsideDownRainSound.Play(this, "splat", 0.4f);
            shiverT = 0.5f;
            meterPopT = 0f;
            Haptics.Tap();
        }

        void PoseDrip()
        {
            if (dripDrop == null) return;
            if (!logic.DripInFlight) { if (dripDrop.gameObject.activeSelf) HalloweenKit.Show(dripDrop, false); return; }
            Vector2 head = headTop != null ? (Vector2)headTop.position : Vector2.zero;
            float k = logic.DripProgress;
            Vector2 p = Vector2.Lerp(dripFrom, head, k * k * 0.4f + k * 0.6f);
            dripDrop.position = new Vector3(p.x, p.y, dripDrop.position.z);
        }

        // ---------- La pioggia al contrario ----------

        void StartReverse()
        {
            // la goccia in volo diventa una goccia che risale
            if (dripDrop != null && dripDrop.gameObject.activeSelf)
            {
                Drop d = Free();
                if (d != null) Launch(d, dripDrop.position, RiseSpeed);
                HalloweenKit.Show(dripDrop, false);
            }
            FlipRain(true);
            say.Show("Wait... what?", 1.4f);
            V86UpsideDownRainSound.Play(this, "whoosh", 0.45f);
            Haptics.Medium();
        }

        /// <summary>Tutte le gocce in volo cambiano verso (su: tornano alla nuvola; giù: ricadono).</summary>
        void FlipRain(bool up)
        {
            if (pool == null) return;
            foreach (Drop d in pool)
                if (d.On) { d.Vy = up ? RiseSpeed : -FallSpeed; d.Through = up; }
        }

        Drop Free()
        {
            if (pool == null) return null;
            foreach (Drop d in pool) if (!d.On) return d;
            return null;
        }

        void Launch(Drop d, Vector2 p, float vy)
        {
            d.On = true;
            d.Through = false;
            d.P = p;
            d.Vy = vy;
            if (d.T != null) { d.T.position = new Vector3(p.x, p.y, d.T.position.z); d.T.localRotation = Quaternion.Euler(0f, 0f, vy > 0f ? 180f : 0f); }
            HalloweenKit.Show(d.T, true);
        }

        /// <summary>La goccia è sulla tela (e non sopra un buco: da lì passa)?</summary>
        bool OnCanopy(Drop d)
        {
            if (umbrella == null || umbrellaOpen == null || !umbrellaOpen.gameObject.activeInHierarchy) return false;
            Vector2 l = umbrella.InverseTransformPoint(d.P);
            if (l.y < rimY - 0.05f || Mathf.Abs(l.x) >= canopyHx) return false;
            float u = l.x / canopyHx;
            float top = rimY + canopyH * Mathf.Sqrt(Mathf.Max(0f, 1f - u * u));
            if (l.y > top) return false;
            if (holes != null)
                foreach (Transform h in holes)
                    if (h != null && Vector2.Distance(h.localPosition, l) < holeR + 0.06f) { d.Through = true; return false; }
            return true;
        }

        void Rain(float dt, bool spawn)
        {
            if (pool == null) return;
            bool up = logic != null && (logic.Reversing || logic.State == UpsideDownRainState.Won);
            if (spawn)
            {
                spawnAcc += dt * (up ? RiseRate : RainRate);
                while (spawnAcc >= 1f)
                {
                    spawnAcc -= 1f;
                    Drop d = Free();
                    if (d == null) break;
                    if (up)
                    {
                        // risalgono dalla strada, dalla pozzanghera e dall'omino
                        Vector2 from = Random.value < 0.35f && man != null
                            ? (Vector2)man.position + new Vector2(Random.Range(-0.35f, 0.35f), Random.Range(0.3f, 1.8f))
                            : new Vector2(Random.Range(-2.45f, 2.45f), groundY + Random.Range(-0.1f, 0.1f));
                        Launch(d, from, RiseSpeed * Random.Range(0.85f, 1.15f));
                    }
                    else Launch(d, new Vector2(Random.Range(-2.5f, 2.5f), cloudBottom - Random.Range(0f, 0.2f)), -FallSpeed * Random.Range(0.9f, 1.1f));
                }
            }
            foreach (Drop d in pool)
            {
                if (!d.On) continue;
                d.P.y += d.Vy * dt;
                if (d.Vy < 0f)
                {
                    bool land = (!d.Through && OnCanopy(d)) || d.P.y <= groundY || Over(manArea, d.P);
                    if (land) { Splash(d.P, 0.22f); Off(d); continue; }
                }
                else if (d.P.y >= cloudBottom)
                {
                    cloudPopT = 0f;   // la nuvola se la risucchia
                    Off(d);
                    continue;
                }
                if (d.T != null)
                {
                    d.T.position = new Vector3(d.P.x, d.P.y, d.T.position.z);
                    d.T.localRotation = Quaternion.Euler(0f, 0f, d.Vy > 0f ? 180f : 0f);
                }
            }
        }

        void Off(Drop d) { d.On = false; HalloweenKit.Show(d.T, false); }

        void Splash(Vector2 p, float size)
        {
            if (splashes == null || splashes.Length == 0) return;
            int i = nextSplash++ % splashes.Length;
            Transform s = splashes[i];
            if (s == null) return;
            s.position = new Vector3(p.x, p.y, s.position.z);
            s.localScale = Vector3.one * size;
            splashT[i] = 0.001f;
            HalloweenKit.Show(s, true);
        }

        void PoseSplashes(float dt)
        {
            if (splashes == null) return;
            for (int i = 0; i < splashes.Length; i++)
            {
                if (splashT[i] <= 0f || splashes[i] == null) continue;
                splashT[i] += dt;
                float k = splashT[i] / 0.22f;
                if (k >= 1f) { splashT[i] = 0f; HalloweenKit.Show(splashes[i], false); continue; }
                Vector3 s = splashes[i].localScale;
                splashes[i].localScale = new Vector3(s.x, Mathf.Max(0.01f, s.x * (1f - k * 0.6f)), 1f);
            }
        }

        // ---------- Pose per fotogramma ----------

        void Pose(float dt)
        {
            Rain(dt, true);
            PoseDrip();
            PoseSplashes(dt);
            if (shiverT > 0f) shiverT -= dt;
            SetFace();
            ShowWetness(logic.Wetness);
            // l'ombrello: segue il dito, poi torna dritto; traballa se toccato
            if (mode != TouchMode.Drag) tilt = Mathf.Lerp(tilt, 0f, Mathf.Clamp01(dt * 10f));
            float wob = 0f;
            if (wiggleT < 1f) { wiggleT = Mathf.Min(1f, wiggleT + dt / 0.4f); wob = 6f * Mathf.Sin(wiggleT * Mathf.PI * 4f) * (1f - wiggleT); }
            if (umbrella != null) umbrella.localRotation = Quaternion.Euler(0f, 0f, tilt + wob);
            // l'omino: trema se bagnato, si scrolla come un cane quando è fradicio
            if (man != null)
            {
                float x = 0f, rot = 0f;
                if (soakShakeT < 1f) { soakShakeT = Mathf.Min(1f, soakShakeT + dt / 0.9f); rot = 9f * Mathf.Sin(soakShakeT * Mathf.PI * 10f) * (1f - soakShakeT); }
                if (logic.Wetness > 0.75f && !logic.Reversing) x = 0.015f * Mathf.Sin(Time.time * 60f);
                man.localPosition = manHome + new Vector3(x, 0f, 0f);
                man.localRotation = Quaternion.Euler(0f, 0f, rot);
            }
            // la nuvola: dondola, sobbalza quando "risucchia" una goccia, trema se toccata
            if (cloud != null)
            {
                if (cloudPopT < 1f) cloudPopT = Mathf.Min(1f, cloudPopT + dt / 0.18f);
                float inhale = logic.Reversing ? 0.05f * logic.DryProgress : 0f;
                cloud.localScale = cloudScale * (1f + inhale + 0.04f * (cloudPopT < 1f ? Mathf.Sin(cloudPopT * Mathf.PI) : 0f));
                Vector3 off = new Vector3(0.06f * Mathf.Sin(Time.time * 0.8f), 0f, 0f);
                if (cloudShakeT < 1f) { cloudShakeT = Mathf.Min(1f, cloudShakeT + dt / 0.4f); off.x += 0.08f * Mathf.Sin(cloudShakeT * Mathf.PI * 8f) * (1f - cloudShakeT); }
                cloud.localPosition = cloudHome + off;
            }
            // il misuratore sobbalza a ogni goccia
            if (meter != null)
            {
                if (meterPopT < 1f) meterPopT = Mathf.Min(1f, meterPopT + dt / 0.2f);
                meter.localScale = meterScale * (1f + 0.08f * (meterPopT < 1f ? Mathf.Sin(meterPopT * Mathf.PI) : 0f));
            }
        }

        // ---------- Vittoria ----------

        /// <summary>
        /// Le ultime gocce tornano su, la nuvola se ne va e sbuca il sole; l'omino chiude l'ombrello, si asciuga, sorride e
        /// fa due saltelli; festa comune; Win una volta sola.
        /// </summary>
        IEnumerator Finale()
        {
            if (finishing) yield break;
            finishing = true;
            EndTouch(false);
            HalloweenKit.Show(infoHolder, false);
            say?.Hide();
            HalloweenKit.Show(dripDrop, false);
            if (umbrella != null) umbrella.localRotation = Quaternion.identity;
            if (man != null) { man.localPosition = manHome; man.localRotation = Quaternion.identity; }
            SetFace();
            float w0 = logic.Wetness;
            // 1) la nuvola se ne va, il sole sbuca
            V86UpsideDownRainSound.Play(this, "whoosh", 0.4f);
            HalloweenKit.Show(sun, true);
            for (float t = 0f; t < 0.8f; t += Time.deltaTime)
            {
                float k = Mathf.Clamp01(t / 0.8f);
                float e = k * k * (3f - 2f * k);
                Rain(Time.deltaTime, false);
                PoseSplashes(Time.deltaTime);
                if (cloud != null) { cloud.localPosition = cloudHome + new Vector3(-2.6f * e, 0.35f * e, 0f); cloud.localScale = cloudScale * (1f - 0.25f * e); }
                if (cloudArt != null) { Color c = cloudColor; c.a = cloudColor.a * (1f - e); cloudArt.color = c; }
                if (sun != null) { sun.localPosition = sunHome + new Vector3(-0.2f * e, 0.05f * e, 0f); sun.localScale = sunScale * (0.6f + 0.4f * e); sun.localRotation = Quaternion.Euler(0f, 0f, 25f * e); }
                if (sunGlow != null) HalloweenKit.SetAlpha(sunGlow, 0.6f * e);
                ShowWetness(w0 * (1f - e));
                yield return null;
            }
            ClearRain();
            ShowWetness(0f);
            // 2) l'ombrello si chiude
            V86UpsideDownRainSound.Play(this, "fold", 0.5f);
            for (float t = 0f; t < 0.25f; t += Time.deltaTime)
            {
                float k = Mathf.Clamp01(t / 0.25f);
                if (umbrellaOpen != null) umbrellaOpen.localScale = new Vector3(1f - 0.85f * k, 1f, 1f);
                yield return null;
            }
            HalloweenKit.Show(umbrellaOpen, false);
            HalloweenKit.Show(umbrellaFolded, true);
            if (umbrellaFolded != null) StartCoroutine(HalloweenKit.Pop(umbrellaFolded, 1.12f, 0.2f));
            // 3) sorride e fa due saltelli
            say?.Show("Sunshine!", 1.6f);
            Haptics.Success();
            for (float t = 0f; t < 0.6f; t += Time.deltaTime)
            {
                float hop = Mathf.Abs(Mathf.Sin(Mathf.Clamp01(t / 0.6f) * Mathf.PI * 2f)) * 0.22f;
                if (man != null) man.localPosition = manHome + new Vector3(0f, hop, 0f);
                if (umbrella != null) umbrella.localPosition = umbrellaHome + new Vector3(0f, hop, 0f);
                if (sun != null) sun.localRotation = Quaternion.Euler(0f, 0f, 25f + 30f * t);
                yield return null;
            }
            if (man != null) man.localPosition = manHome;
            if (umbrella != null) umbrella.localPosition = umbrellaHome;
            V86UpsideDownRainSound.Play(this, "ting", 0.55f);
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
            if (Input.GetKeyDown(KeyCode.M)) { tilt = Random.value < 0.5f ? -MaxTilt : MaxTilt; React(logic.MoveUmbrella()); }
            if (Input.GetKeyDown(KeyCode.O)) { wiggleT = 0f; React(logic.PlugHole()); }
            if (Input.GetKeyDown(KeyCode.B)) React(logic.TapMan());
            if (Input.GetKeyDown(KeyCode.C)) React(logic.TapCloud());
            if (Input.GetKeyDown(KeyCode.H)) { editorHalf = false; editorFlip = true; }   // soluzione: capovolto (vince se prima era dritto)
            if (Input.GetKeyDown(KeyCode.P)) CancelAll();
            if (Input.GetKeyDown(KeyCode.R)) OnLevelStart();
        }
#endif
    }

    /// <summary>Suoni sintetizzati della pioggia al contrario: goccia, splat, brividi, tuono della nuvola, risucchio, ombrello che si chiude.</summary>
    public static class V86UpsideDownRainSound
    {
        const int Rate = 22050;
        static readonly Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>();
        static readonly System.Random rnd = new System.Random(861);

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
                case "drip":    // "plic" che sale
                    c = Make(kind, 0.12f, t => Mathf.Sin(t * 2f * Mathf.PI * Mathf.Lerp(700f, 1600f, t / 0.12f)) * 0.25f * Mathf.Exp(-t * 30f));
                    break;
                case "splat":   // goccia in testa
                    c = Make(kind, 0.18f, t => (N() * 0.5f + Mathf.Sin(t * 2f * Mathf.PI * 420f)) * 0.2f * Mathf.Exp(-t * 25f));
                    break;
                case "brr":     // brividi: vibrato basso
                    c = Make(kind, 0.5f, t => Mathf.Sin(t * 2f * Mathf.PI * (180f + 30f * Mathf.Sin(t * 2f * Mathf.PI * 22f))) * 0.2f * Mathf.Sin(t / 0.5f * Mathf.PI));
                    break;
                case "rumble":  // la nuvola brontola
                    c = Make(kind, 0.6f, t => N() * 0.25f * Mathf.Sin(t / 0.6f * Mathf.PI) * (0.6f + 0.4f * Mathf.Sin(t * 2f * Mathf.PI * 9f)));
                    break;
                case "whoosh":  // la pioggia torna su: fruscio che sale
                    c = Make(kind, 0.55f, t => (N() * 0.6f + 0.4f * Mathf.Sin(t * 2f * Mathf.PI * Mathf.Lerp(200f, 900f, t / 0.55f))) * 0.2f * Mathf.Sin(t / 0.55f * Mathf.PI));
                    break;
                case "fold":    // ombrello che si chiude: "fsst-clic"
                    c = Make(kind, 0.3f, t => t < 0.22f ? N() * 0.15f * (1f - t / 0.22f) : Mathf.Sin(t * 2f * Mathf.PI * 2200f) * 0.25f * Mathf.Exp(-(t - 0.22f) * 60f));
                    break;
                case "ting":    // luccichio finale
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
            AudioClip clip = AudioClip.Create("v86a_" + name, n, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
