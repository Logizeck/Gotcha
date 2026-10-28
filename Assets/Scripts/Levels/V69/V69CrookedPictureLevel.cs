using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels.V64;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 266 "The Crooked Picture" — "Straighten the picture." Un soggiorno con un quadro (il gatto nobile col
    /// monocolo) appeso a un chiodo, storto di 12 gradi... ma il quadro è dritto: è TUTTA la stanza (Room, ruotata di
    /// CrookedPictureLogic.RoomAngle) a essere storta rispetto allo schermo: pavimento, righe della carta da parati,
    /// finestra, mensola, tavolino, lampada. Indizi che seguono la gravità vera del telefono: il pelo dell'acqua nel
    /// bicchiere resta orizzontale nello schermo (non col tavolo), la biglia sulla mensola rotola sempre verso il lato
    /// basso, la bolla della livella sul tavolo è fuori centro. Mosse sbagliate: girare il quadro col dito (torna storto
    /// dondolando), il nastro adesivo (si stacca), il martello col chiodo in più (il quadro scivola indietro lo stesso),
    /// toccare la livella, il bicchiere o la biglia. Soluzione: inclinare il telefono quanto la stanza (rispetto a come lo
    /// si teneva all'inizio) e tenerlo così un secondo: la stanza si raddrizza nello schermo, la bolla scatta al centro,
    /// il gatto del quadro fa l'occhiolino: "THE PICTURE WAS FINE." "YOU WERE CROOKED." Niente microfono.
    /// La logica è in Gotcha.Levels.V64.CrookedPictureLogic (senza Unity): qui solo input e presentazione.
    /// Tasti di prova nell'Editor (senza accelerometro): Q / freccia sinistra = telefono in senso antiorario, E / freccia
    /// destra = orario, R = di nuovo dritto, F = telefono piatto sì/no, T = nastro, H = martello, L = livella,
    /// G = bicchiere, M = biglia.
    /// </summary>
    public class V69CrookedPictureLevel : V62Level
    {
        [Header("Stanza e quadro")]
        [SerializeField] Transform room;               // tutta la stanza, ruotata di RoomAngle (si raddrizza alla fine)
        [SerializeField] Transform pictureSwing;       // perno nel chiodo: il quadro gira attorno al chiodo
        [SerializeField] Collider2D pictureArea;
        [SerializeField] SpriteRenderer wink;          // occhiolino del gatto del quadro (finale)
        [SerializeField] Transform[] tapeStrips;       // i due pezzi di nastro sugli angoli del quadro
        [SerializeField] Transform extraNail;          // il chiodo in più del martello
        [Header("Indizi")]
        [SerializeField] Transform waterPivot;         // pelo dell'acqua nel bicchiere (z locale = WaterTilt)
        [SerializeField] Transform glass;
        [SerializeField] Collider2D glassArea;
        [SerializeField] Transform spiritLevel;
        [SerializeField] Transform levelBubble;
        [SerializeField] Collider2D levelArea;
        [SerializeField] Transform marble;
        [SerializeField] Transform marbleTrack;        // centro della corsa della biglia sulla mensola (coordinate della stanza)
        [SerializeField] Collider2D marbleArea;
        [Header("Attrezzi")]
        [SerializeField] Transform tapeRoll;
        [SerializeField] Collider2D tapeArea;
        [SerializeField] Transform hammer;
        [SerializeField] Collider2D hammerArea;
        [SerializeField] TextMesh infoText;
        [SerializeField] Transform infoHolder;

        const float BubbleTravel = 0.15f;      // metà corsa della bolla nella fiala (coordinate della livella)
        const float MarbleSpan = 0.48f;        // metà corsa della biglia sulla mensola (coordinate della stanza)
        const float MarbleRadius = 0.14f;
        const float TapSlop = 0.18f;
        const float MaxTwist = 40f;
        const float SpringK = 70f, SpringDamp = 4.5f;

        static readonly string[] TwistLines = { "It keeps swinging back.", "Back to crooked. It insists.", "It REALLY likes that angle." };
        static readonly string[] PokeLines = { "Wobble. Still crooked.", "It just wobbles back." };
        static readonly string[] TapeLines = { "The tape peeled off.", "Peeled again. It's not the tape's fault.", "Tape: 0. Picture: 3." };
        static readonly string[] HammerLines = { "...and it slid right back.", "More nails. Same angle.", "The nails agree with the picture." };
        static readonly string[] LevelLines = { "The level says the PICTURE is fine.", "Still off-centre. The level has opinions.", "The bubble always runs to the same side." };
        static readonly string[] GlassLines = { "The water is perfectly level.", "Level water. Crooked glass. Hmm.", "The water knows which way is down." };
        static readonly string[] MarbleLines = { "It rolls back. Every time.", "Downhill is always that way.", "That marble REALLY wants to go right." };
        static readonly string[] WrongWayLines = { "Now it looks even MORE crooked.", "That made everything worse." };
        static readonly string[] ShakeLines = { "Shaking the house won't fix it.", "The picture survived. Barely." };

        readonly CrookedPictureLogic logic = new CrookedPictureLogic();
        Camera cam;
        HwToast toast;
        int pictureFinger = int.MinValue;
        Vector2 gripStart;
        float gripAngle0, gripRot0;
        bool gripMoved;
        float picRot, picVel;          // rotazione del quadro rispetto alla stanza (molla che lo riporta a 0)
        bool picForced;                // il nastro o il martello lo tengono fermo (niente molla)
        float shownBubble = -1f, shownWater, bubbleJiggle, slosh, sloshVel, marbleSpin;
        float shakeReady;
        Vector3 hammerHome, tapeRollHome, bubbleHome, marbleHome;
        Vector3 hammerScale, tapeRollScale, swingScale, levelScale, bubbleScale, nailScale;
        Vector3[] stripHome, stripScale;
        Quaternion[] stripRot;
        Quaternion hammerRot;
        bool homesSaved, busy, finishing, needResume;
        int twistLines, pokeLines, tapeLines, hammerLines, wrongLines, shakeLines;
#if UNITY_EDITOR
        float simRoll;
        bool simFlat;
#endif

        public override void OnLevelStart()
        {
            cam = Camera.main;
            logic.Reset();
            ResetV62();
            V64AFingers.Reset();
            toast = new HwToast(infoText, infoHolder);
            SaveHomes();
            RestoreProps();
            busy = finishing = needResume = false;
            twistLines = pokeLines = tapeLines = hammerLines = wrongLines = shakeLines = 0;
            shownBubble = logic.Bubble;
            shownWater = logic.WaterTilt;
            bubbleJiggle = slosh = sloshVel = 0f;
            shakeReady = 0f;
            HalloweenKit.Show(extraNail, false);
            HalloweenKit.Show(wink, false);
            room.localRotation = Quaternion.Euler(0f, 0f, CrookedPictureLogic.RoomAngle);
#if UNITY_EDITOR
            simRoll = 0f;
            simFlat = false;
#endif
            ShowAll(0f);
        }

        void SaveHomes()
        {
            if (homesSaved) return;
            homesSaved = true;
            hammerHome = hammer.localPosition;
            hammerRot = hammer.localRotation;
            tapeRollHome = tapeRoll.localPosition;
            bubbleHome = levelBubble.localPosition;
            marbleHome = marble.localPosition;
            hammerScale = hammer.localScale;
            tapeRollScale = tapeRoll.localScale;
            swingScale = pictureSwing.localScale;
            levelScale = spiritLevel.localScale;
            bubbleScale = levelBubble.localScale;
            nailScale = extraNail.localScale;
            int n = tapeStrips != null ? tapeStrips.Length : 0;
            stripHome = new Vector3[n];
            stripScale = new Vector3[n];
            stripRot = new Quaternion[n];
            for (int i = 0; i < n; i++)
            {
                if (tapeStrips[i] == null) continue;
                stripHome[i] = tapeStrips[i].localPosition;
                stripScale[i] = tapeStrips[i].localScale;
                stripRot[i] = tapeStrips[i].localRotation;
            }
        }

        /// <summary>Riporta gli oggetti delle animazioni interrotte al loro posto (lo stato vero è nella logica).</summary>
        void RestoreProps()
        {
            if (!homesSaved) return;
            hammer.localPosition = hammerHome;
            hammer.localRotation = hammerRot;
            hammer.localScale = hammerScale;
            tapeRoll.localPosition = tapeRollHome;
            tapeRoll.localScale = tapeRollScale;
            RestoreTapeStrips();
            picRot = picVel = 0f;
            picForced = false;
            pictureFinger = int.MinValue;
            pictureSwing.localRotation = Quaternion.identity;
            pictureSwing.localScale = swingScale;
            spiritLevel.localScale = levelScale;
            levelBubble.localScale = bubbleScale;
            extraNail.localScale = nailScale;
            glass.localRotation = Quaternion.identity;
        }

        void OnDisable()
        {
            CancelGrips();
            // le animazioni si fermano con l'oggetto: niente nastro o martello a metà al ritorno
            busy = false;
            RestoreProps();
        }

        void OnEnable()
        {
            needResume = true;
            // se il livello è stato spento a metà del finale, lo si riprende (vittoria comunque una volta sola)
            if (logic.Stage == CrookedStage.Straightened && !IsFinished && cam != null) StartCoroutine(Finale());
        }

        void OnApplicationPause(bool paused) { if (paused) { CancelGrips(); needResume = true; } }
        void OnApplicationFocus(bool focus) { if (!focus) { CancelGrips(); needResume = true; } }

        /// <summary>Pausa, perdita del focus, pubblicità: si lascia il quadro (torna storto da solo).</summary>
        void CancelGrips()
        {
            pictureFinger = int.MinValue;
            V64AFingers.Reset();
        }

        void Update()
        {
            toast?.Tick();
            if (IsFinished || finishing || logic.InputLocked) return;
            if (Time.timeScale <= 0f) { CancelGrips(); needResume = true; return; }   // pausa o pubblicità
            float dt = Time.deltaTime;
            if (needResume) { needResume = false; logic.OnResume(); }
#if UNITY_EDITOR
            EditorKeys(dt);
#endif
            Vector2 g = ReadGravity(dt);
            CrookedEvent ev = logic.Feed(g.x, g.y, dt);
            OnEvent(ev);
            if (ev == CrookedEvent.Straightened) return;
            ReadFingers();
            CheckShake();
            ShowAll(dt);
        }

        /// <summary>Gravità nello schermo (in g): l'accelerometro via SensorHub; nell'Editor senza sensore, i tasti.</summary>
        Vector2 ReadGravity(float dt)
        {
            Vector2 g = Inputs.Tilt();
#if UNITY_EDITOR
            if (Input.acceleration.sqrMagnitude < 0.0001f)
            {
                CrookedPictureLogic.GravityFor(simRoll, simFlat ? 88f : 20f, out float gx, out float gy);
                g = new Vector2(gx, gy);
            }
#endif
            return g;
        }

        void OnEvent(CrookedEvent ev)
        {
            switch (ev)
            {
                case CrookedEvent.Aligning:
                    Sfx.Play(this, "tick", 0.5f);
                    Haptics.Tap();
                    break;
                case CrookedEvent.WrongWay:
                    toast.Show(WrongWayLines[wrongLines++ % WrongWayLines.Length], 1.6f);
                    break;
                case CrookedEvent.Straightened:
                    StopAllCoroutines();
                    RestoreProps();
                    StartCoroutine(Finale());
                    break;
            }
        }

        // ---------- Dita ----------

        void ReadFingers()
        {
            List<V64AFinger> fingers = V64AFingers.Read(cam);
            bool pictureSeen = false;
            foreach (V64AFinger f in fingers)
            {
                if (f.began && pictureFinger == int.MinValue) Begin(f);
                if (f.id != pictureFinger) continue;
                pictureSeen = true;
                if (Vector2.Distance(f.world, gripStart) > TapSlop) gripMoved = true;
                if (f.ended) { ReleasePicture(); continue; }
                Vector2 v = f.world - (Vector2)pictureSwing.position;
                if (v.sqrMagnitude > 0.04f)
                {
                    float a = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
                    picRot = Mathf.Clamp(gripRot0 + Mathf.DeltaAngle(gripAngle0, a), -MaxTwist, MaxTwist);
                    picVel = 0f;
                }
            }
            // dito perso senza "fine" (tocco annullato): si lascia il quadro
            if (pictureFinger != int.MinValue && !pictureSeen && fingers.Count == 0) ReleasePicture();
        }

        void Begin(V64AFinger f)
        {
            Vector2 w = f.world;
            // prima gli oggetti piccoli (aree larghe), poi il quadro
            if (!busy && tapeArea.OverlapPoint(w)) { StartCoroutine(TapeIt()); return; }
            if (!busy && hammerArea.OverlapPoint(w)) { StartCoroutine(HammerIt()); return; }
            if (marbleArea.OverlapPoint(w)) { TapMarble(); return; }
            if (levelArea.OverlapPoint(w)) { TapLevel(); return; }
            if (glassArea.OverlapPoint(w)) { TapGlass(); return; }
            if (!busy && pictureArea.OverlapPoint(w))
            {
                pictureFinger = f.id;
                gripStart = w;
                gripMoved = false;
                Vector2 v = w - (Vector2)pictureSwing.position;
                gripAngle0 = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
                gripRot0 = picRot;
                Haptics.Tap();
            }
        }

        void ReleasePicture()
        {
            pictureFinger = int.MinValue;
            if (gripMoved && Mathf.Abs(picRot) > 2.5f)
            {
                int n = logic.TwistPicture();
                if (n > 0) toast.Show(TwistLines[(n - 1) % TwistLines.Length], 1.6f);
                Sfx.Play(this, "whoosh", 0.35f);
            }
            else
            {
                picVel += 60f;   // un colpetto: dondola
                toast.Show(PokeLines[pokeLines++ % PokeLines.Length], 1.3f);
                Sfx.Play(this, "tick", 0.4f);
            }
        }

        // ---------- Mosse sbagliate ----------

        void TapMarble()
        {
            if (logic.TapMarble() == 0) return;
            Sfx.Play(this, "tick", 0.5f);
            Haptics.Tap();
            toast.Show(MarbleLines[(logic.MarbleTaps - 1) % MarbleLines.Length], 1.6f);
        }

        void TapLevel()
        {
            int n = logic.TapLevel();
            if (n == 0) return;
            bubbleJiggle = 1f;   // la bolla corre verso il centro, ci ripensa e torna dov'era
            Sfx.Play(this, "tick", 0.5f);
            Haptics.Tap();
            StartCoroutine(HalloweenKit.Pop(spiritLevel, 1.08f, 0.2f));
            toast.Show(LevelLines[(n - 1) % LevelLines.Length], 1.8f);
        }

        void TapGlass()
        {
            int n = logic.TapGlass();
            if (n == 0) return;
            sloshVel += 160f;
            StartCoroutine(HalloweenKit.Wiggle(glass, 4f, 0.3f, 30f));
            V64ASound.Play(this, "glug", 0.4f);
            Haptics.Tap();
            toast.Show(GlassLines[(n - 1) % GlassLines.Length], 1.8f);
        }

        /// <summary>Nastro adesivo: il quadro si raddrizza nello schermo, due pezzi di nastro... che si staccano.</summary>
        IEnumerator TapeIt()
        {
            int n = logic.Tape();
            if (n == 0) yield break;
            busy = true;
            ReleaseGripSilently();
            Haptics.Tap();
            StartCoroutine(HalloweenKit.Pop(tapeRoll, 1.2f, 0.2f));
            V64ASound.Play(this, "scrape", 0.4f);
            yield return ForcePicture(-CrookedPictureLogic.RoomAngle, 0.3f);
            foreach (Transform t in tapeStrips)
            {
                HalloweenKit.Show(t, true);
                HalloweenKit.SetAlpha(t.GetComponentInChildren<SpriteRenderer>(), 1f);
                Sfx.Play(this, "pop", 0.5f);
                StartCoroutine(HalloweenKit.Pop(t, 1.25f, 0.15f));
                yield return new WaitForSeconds(0.15f);
            }
            toast.Show("Taped. Perfectly straight!", 1.2f);
            yield return new WaitForSeconds(1.2f);
            // si stacca: i pezzi si arricciano e cadono, il quadro torna dondolando
            V64ASound.Play(this, "scrape", 0.5f);
            foreach (Transform t in tapeStrips)
            {
                StartCoroutine(HalloweenKit.Move(t, t.position + Vector3.down * 0.9f, 0.6f, 0.15f));
                StartCoroutine(HalloweenKit.Wiggle(t, 50f, 0.6f, 14f));
                StartCoroutine(HalloweenKit.Fade(t.GetComponentInChildren<SpriteRenderer>(), 0f, 0.6f));
            }
            picForced = false;
            picVel = -40f;
            toast.Show(TapeLines[(n - 1) % TapeLines.Length], 1.8f);
            yield return new WaitForSeconds(0.65f);
            RestoreTapeStrips();
            busy = false;
        }

        void RestoreTapeStrips()
        {
            if (tapeStrips == null || stripHome == null) return;
            for (int i = 0; i < tapeStrips.Length && i < stripHome.Length; i++)
            {
                Transform t = tapeStrips[i];
                if (t == null) continue;
                HalloweenKit.Show(t, false);
                t.localPosition = stripHome[i];
                t.localScale = stripScale[i];
                t.localRotation = stripRot[i];
                HalloweenKit.SetAlpha(t.GetComponentInChildren<SpriteRenderer>(), 1f);
            }
        }

        /// <summary>Martello e chiodo in più: il quadro sembra dritto... e scivola indietro lo stesso.</summary>
        IEnumerator HammerIt()
        {
            int n = logic.Hammer();
            if (n == 0) yield break;
            busy = true;
            ReleaseGripSilently();
            Haptics.Tap();
            Vector3 at = extraNail.position + new Vector3(0.42f, -0.05f, 0f);
            hammer.localRotation = hammerRot;
            yield return HalloweenKit.Move(hammer, at, 0.35f, 0.5f);
            for (int i = 0; i < 2; i++)
            {
                yield return HalloweenKit.Wiggle(hammer, 25f, 0.12f, 26f);
                Sfx.Play(this, "clunk", 0.8f);
                Haptics.Light();
                if (i == 0 && !extraNail.gameObject.activeSelf) { HalloweenKit.Show(extraNail, true); StartCoroutine(HalloweenKit.Pop(extraNail, 1.4f, 0.15f)); }
            }
            yield return ForcePicture(-CrookedPictureLogic.RoomAngle, 0.25f);
            toast.Show(n == 1 ? "An extra nail. Very professional." : "Another nail. Straight again!", 1.2f);
            StartCoroutine(HalloweenKit.Move(hammer, hammer.parent.TransformPoint(hammerHome), 0.4f, 0.3f));
            yield return new WaitForSeconds(0.9f);
            // scivola indietro piano piano
            V64ASound.Play(this, "scrape", 0.45f);
            yield return ForcePicture(0f, 0.8f);
            picForced = false;
            picVel = -25f;
            toast.Show(HammerLines[(n - 1) % HammerLines.Length], 1.8f);
            hammer.localPosition = hammerHome;
            hammer.localRotation = hammerRot;
            busy = false;
        }

        IEnumerator ForcePicture(float to, float seconds)
        {
            picForced = true;
            float from = picRot;
            for (float t = 0f; t < 1f; t += Time.deltaTime / Mathf.Max(0.01f, seconds))
            {
                picRot = Mathf.Lerp(from, to, t * t * (3f - 2f * t));
                picVel = 0f;
                yield return null;
            }
            picRot = to;
        }

        void ReleaseGripSilently()
        {
            pictureFinger = int.MinValue;
        }

        void CheckShake()
        {
            if (Time.time < shakeReady || Inputs.Shake() < 15f) return;
            shakeReady = Time.time + 2.5f;
            if (!picForced) picVel += 90f;
            Haptics.Light();
            toast.Show(ShakeLines[shakeLines++ % ShakeLines.Length], 1.6f);
        }

        // ---------- Presentazione ----------

        void ShowAll(float dt)
        {
            // il quadro: segue il dito, o la molla lo riporta dritto rispetto alla stanza (cioè storto nello schermo)
            if (pictureFinger == int.MinValue && !picForced)
            {
                picVel += (-SpringK * picRot - SpringDamp * picVel) * dt;
                picRot += picVel * dt;
                if (Mathf.Abs(picRot) < 0.02f && Mathf.Abs(picVel) < 0.05f) picRot = picVel = 0f;
            }
            pictureSwing.localRotation = Quaternion.Euler(0f, 0f, picRot);

            // la bolla: verso il lato alto; toccando la livella corre al centro e torna
            shownBubble = Mathf.Lerp(shownBubble, logic.Bubble, 1f - Mathf.Exp(-6f * dt));
            float b = shownBubble;
            if (bubbleJiggle > 0f)
            {
                bubbleJiggle = Mathf.Max(0f, bubbleJiggle - dt / 0.9f);
                float k = Mathf.Sin((1f - bubbleJiggle) * Mathf.PI);
                b = Mathf.Lerp(b, 0f, k * 0.8f) + Mathf.Sin(Time.time * 30f) * 0.06f * bubbleJiggle;
            }
            levelBubble.localPosition = bubbleHome + new Vector3(Mathf.Clamp(b, -1.1f, 1.1f) * BubbleTravel, 0f, 0f);

            // l'acqua: orizzontale nel mondo vero (più lo sciabordio quando si tocca il bicchiere)
            shownWater = Mathf.LerpAngle(shownWater, logic.WaterTilt, 1f - Mathf.Exp(-8f * dt));
            sloshVel += (-90f * slosh - 5f * sloshVel) * dt;
            slosh += sloshVel * dt;
            waterPivot.localRotation = Quaternion.Euler(0f, 0f, shownWater + slosh);

            // la biglia sulla mensola (rotola davvero)
            float x = logic.MarbleX * MarbleSpan;
            Vector3 p = marbleTrack.localPosition + new Vector3(x, 0f, 0f);
            float dx = p.x - marble.localPosition.x;
            marble.localPosition = new Vector3(p.x, marbleHome.y, marbleHome.z);
            marbleSpin -= dx / MarbleRadius * Mathf.Rad2Deg;
            marble.localRotation = Quaternion.Euler(0f, 0f, marbleSpin);
        }

        // ---------- Finale ----------

        IEnumerator Finale()
        {
            finishing = true;
            busy = true;
            CancelGrips();
            RestoreTapeStrips();
            Haptics.Light();
            // la stanza si raddrizza nello schermo (così si può rimettere dritto il telefono)
            float from = room.localEulerAngles.z;
            float bubble0 = (levelBubble.localPosition.x - bubbleHome.x) / BubbleTravel;
            float water0 = shownWater;
            float pic0 = picRot;
            Sfx.Play(this, "whoosh", 0.5f);
            for (float t = 0f; t < 1f; t += Time.deltaTime / 1.0f)
            {
                float e = t * t * (3f - 2f * t);
                room.localRotation = Quaternion.Euler(0f, 0f, Mathf.LerpAngle(from, 0f, e));
                levelBubble.localPosition = bubbleHome + new Vector3(Mathf.Lerp(bubble0, 0f, e) * BubbleTravel, 0f, 0f);
                waterPivot.localRotation = Quaternion.Euler(0f, 0f, Mathf.LerpAngle(water0, 0f, e));
                picRot = Mathf.Lerp(pic0, 0f, e);
                pictureSwing.localRotation = Quaternion.Euler(0f, 0f, picRot);
                yield return null;
            }
            room.localRotation = Quaternion.identity;
            levelBubble.localPosition = bubbleHome;
            waterPivot.localRotation = Quaternion.identity;
            pictureSwing.localRotation = Quaternion.identity;
            // clic della bolla al centro
            Sfx.Play(this, "tick", 0.9f);
            Haptics.Light();
            yield return HalloweenKit.Pop(levelBubble, 1.6f, 0.15f);
            // occhiolino
            HalloweenKit.Show(wink, true);
            Sfx.Play(this, "pop", 0.7f);
            StartCoroutine(HalloweenKit.Pop(pictureSwing, 1.06f, 0.25f));
            toast.Show("THE PICTURE WAS FINE.", 1.3f);
            yield return new WaitForSeconds(1.2f);
            toast.Show("YOU WERE CROOKED.", 2.2f);
            yield return Celebrate(1.3f);
            if (logic.TryComplete()) Win();
        }

#if UNITY_EDITOR
        void EditorKeys(float dt)
        {
            if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.LeftArrow)) simRoll -= 25f * dt;
            if (Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.RightArrow)) simRoll += 25f * dt;
            simRoll = Mathf.Clamp(simRoll, -60f, 60f);
            if (Input.GetKeyDown(KeyCode.R)) simRoll = 0f;
            if (Input.GetKeyDown(KeyCode.F)) simFlat = !simFlat;
            if (Input.GetKeyDown(KeyCode.T) && !busy) StartCoroutine(TapeIt());
            if (Input.GetKeyDown(KeyCode.H) && !busy) StartCoroutine(HammerIt());
            if (Input.GetKeyDown(KeyCode.L)) TapLevel();
            if (Input.GetKeyDown(KeyCode.G)) TapGlass();
            if (Input.GetKeyDown(KeyCode.M)) TapMarble();
        }
#endif
    }
}
