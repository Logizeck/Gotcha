using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels.V64;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 269 "The Cat Burglar" — "GET THE COOKIE JAR DOWN!" Un soggiorno: il barattolo dei biscotti è su una mensola
    /// altissima; sotto, i mobili fanno da scala (sedia, tavolino, credenza, mensola). Sul pavimento un gatto seduto, una
    /// scaletta corta, una pila di libri, un cuscino; sul tavolino un puntatore laser.
    /// False piste (nessuna azzera niente, niente sconfitta): la scaletta sotto la mensola ("Still a mile away."), i libri
    /// impilati sotto la mensola (traballano e crollano, poi tornano in pila), toccare il barattolo (troppo in alto),
    /// scuotere il telefono (il barattolo traballa ma resta lì), tirare su il gatto (si allunga come un liquido e ricola giù),
    /// il puntino su un ripiano troppo alto (il gatto lo fissa e scodinzola, ma non salta).
    /// Indizio: gli occhi del gatto seguono tutto quello che si muove, ogni tanto guarda il barattolo (e ci pensa su); acceso
    /// il laser, il puntino rosso tremola e il gatto drizza le orecchie.
    /// Soluzione: accendere il laser (tocco) e trascinare il puntino: il gatto lo insegue di ripiano in ripiano fino alla
    /// mensola, dove spinge piano piano il barattolo giù dal bordo. Senza niente sotto si rompe ("Oops. Cookies everywhere.",
    /// poi ne ricompare uno); col cuscino sotto la mensola atterra morbido: "THE CAT DID IT." / "TECHNICALLY, SO DID YOU."
    /// Niente microfono; lo scuotimento (Inputs.Shake) è solo una falsa pista. La logica è in
    /// Gotcha.Levels.V64.CatBurglarLogic (senza Unity): qui solo input e presentazione.
    /// Tasti di prova nell'Editor: L laser, 0-4 puntino su pavimento/sedia/tavolino/credenza/mensola, C cuscino sotto la
    /// mensola o via, K un passo della soluzione, J tocco sul barattolo, D gatto tirato su, B libri, P scaletta (S = scossone,
    /// da Inputs.Shake).
    /// </summary>
    public class V72CatBurglarLevel : V62Level
    {
        [Header("Gatto")]
        [SerializeField] Transform cat;                 // piedi del gatto (posizionato dalla logica)
        [SerializeField] Transform catBody;             // si allunga, si schiaccia, si dimena
        [SerializeField] Collider2D catArea;
        [SerializeField] Transform pupilL;
        [SerializeField] Transform pupilR;
        [SerializeField] Transform paw;                 // la zampa che spinge il barattolo
        [SerializeField] TextMesh catMark;              // il "!" delle orecchie dritte
        [SerializeField] SpeechBubble catBubble;
        [SerializeField] Transform thought;             // nuvoletta col barattolo che cade
        [Header("Barattolo")]
        [SerializeField] Transform jar;
        [SerializeField] Transform jarArt;
        [SerializeField] Collider2D jarArea;
        [SerializeField] Transform shards;
        [Header("Oggetti")]
        [SerializeField] Transform cushion;
        [SerializeField] Collider2D cushionArea;
        [SerializeField] Transform ladder;
        [SerializeField] Collider2D ladderArea;
        [SerializeField] Transform books;
        [SerializeField] Transform[] bookArt;
        [SerializeField] Collider2D booksArea;
        [Header("Laser")]
        [SerializeField] Transform pen;
        [SerializeField] Collider2D penArea;
        [SerializeField] Transform penTip;
        [SerializeField] Transform penHand;             // dove sta il puntatore "in mano" una volta preso
        [SerializeField] Transform beamRoot;            // il raggio: origine sulla punta, scala x = lunghezza
        [SerializeField] SpriteRenderer beam;
        [SerializeField] Transform dot;
        [SerializeField] SpriteRenderer dotArt;
        [SerializeField] TextMesh infoText;
        [SerializeField] Transform infoHolder;

        const float TapTime = 0.45f, TapSlop = 0.25f, DotGrab = 0.7f, DotLift = 0.3f, CatPull = 0.45f;
        const float ShakeLevel = 12f, ShakeCooldown = 1.4f, TooHighCooldown = 1.6f;
        const float CushionY = -3.3f, JarOnCushionY = -3.12f, FloorY = -3.5f;
        const float PupilReach = 0.028f, MaxStretch = 1.6f;

        static readonly string[] JarTapLines = { "Out of reach.", "Way up there.", "Your arms aren't that long." };
        static readonly string[] ShakeLines = { "It's glued to the shelf by stubbornness.", "Wobble. Nope.", "It's not coming down for THAT." };
        static readonly string[] LadderLines = { "Still a mile away.", "Short ladder. Tall shelf.", "Nope. Not even close." };
        static readonly string[] LadderTapLines = { "A tiny stepladder.", "Two whole steps." };
        static readonly string[] BookLines = { "Timber!", "That went well.", "Books are not stairs." };
        static readonly string[] BookTapLines = { "Wobbly.", "A very unstable stack." };
        static readonly string[] DragLines = { "Cats are liquid.", "It just... poured back down.", "Boneless. Totally boneless." };
        static readonly string[] CatTapLines = { "Mrrp?", "Purrr.", "Mew." };
        static readonly string[] TooHighLines = { "Too high. Even for a cat.", "One jump at a time.", "It needs a step in between." };
        static readonly string[] CushionLines = { "Nice and fluffy.", "A soft spot. Just in case." };

        enum Kind { None, Dot, Pen, Item, Cat, Jar, Other }

        sealed class Grip { public Kind kind; public Vector2 start, last, offset; public float t0; public bool moved; }

        readonly CatBurglarLogic logic = new CatBurglarLogic();
        readonly Dictionary<int, Grip> grips = new Dictionary<int, Grip>();
        readonly List<int> gone = new List<int>(4);
        Camera cam;
        HwToast toast;
        HwSay say;
        HwDragger dragger;
        HwDragger.Item cushionItem, ladderItem, booksItem;
        Vector3 catScale, bodyScale, pupilLHome, pupilRHome, jarArtPos, cushionScale, cushionHome, ladderHome, booksHome, penHome, thoughtScale, bubbleHome;
        Quaternion penRot;
        Vector3[] bookHome;
        Quaternion[] bookRot;
        Vector2 dotWorld, lookAt;
        bool ready, finishing, penInHand, penMoving, toppling, ladderBusy, smug;
        int jarTapLines, shakeLines, ladderLines, ladderTapLines, bookLines, bookTapLines, dragLines, catTapLines, tooHighLines, cushionLines;
        int tooHighSeen;
        float shakeReadyAt, tooHighReadyAt, glanceAt, glanceUntil, thinkAt, thinkUntil;
        float perkT = 9f, landT = 9f, flopT = 9f, wiggleT = 9f, wobbleT = 9f, flickerT = 9f, newJarT = 9f, plopT = 9f;
        float stretch, flopFrom;

        Vector2 CushionSpot => new Vector2(CatBurglarLogic.LandX, CushionY);

        public override void OnLevelStart()
        {
            cam = Camera.main;
            StopAllCoroutines();
            logic.Reset();
            ResetV62();
            V64AFingers.Reset();
            grips.Clear();
            finishing = penInHand = penMoving = toppling = ladderBusy = smug = false;
            jarTapLines = shakeLines = ladderLines = ladderTapLines = bookLines = bookTapLines = dragLines = catTapLines = tooHighLines = cushionLines = 0;
            tooHighSeen = 0;
            shakeReadyAt = tooHighReadyAt = 0f;
            glanceAt = Time.time + 3f;
            thinkAt = Time.time + 7f;
            glanceUntil = thinkUntil = 0f;
            perkT = landT = flopT = wiggleT = wobbleT = flickerT = newJarT = plopT = 9f;
            stretch = 0f;
            toast = new HwToast(infoText, infoHolder);
            say = new HwSay(catBubble);
            if (!ready)
            {
                ready = true;
                catScale = cat.localScale;
                bodyScale = catBody.localScale;
                pupilLHome = pupilL.localPosition;
                pupilRHome = pupilR.localPosition;
                jarArtPos = jarArt.localPosition;
                cushionScale = cushion.localScale;
                cushionHome = cushion.position;
                ladderHome = ladder.position;
                booksHome = books.position;
                penHome = pen.position;
                penRot = pen.rotation;
                thoughtScale = thought.localScale;
                bubbleHome = catBubble != null ? catBubble.transform.position : Vector3.zero;
                bookHome = new Vector3[bookArt.Length];
                bookRot = new Quaternion[bookArt.Length];
                for (int i = 0; i < bookArt.Length; i++) { bookHome[i] = bookArt[i].localPosition; bookRot[i] = bookArt[i].localRotation; }
            }
            dragger = new HwDragger(this);
            cushionItem = dragger.Add(cushion, cushionArea, 3);
            cushionItem.Home = cushionHome;
            ladderItem = dragger.Add(ladder, ladderArea, 2);
            ladderItem.Home = ladderHome;
            booksItem = dragger.Add(books, booksArea, 1);
            booksItem.Home = booksHome;
            cushion.position = cushionHome;
            ladder.position = ladderHome;
            books.position = booksHome;
            pen.position = penHome;
            pen.rotation = penRot;
            dotWorld = new Vector2(logic.CatX, FloorY);
            lookAt = (Vector2)jar.position;
            RestPose();
            Present(0f);
        }

        /// <summary>Tutto al suo posto secondo la logica (oggetti a casa o sotto la mensola, niente animazioni a metà).</summary>
        void RestPose()
        {
            cushion.localScale = cushionScale;
            if (dragger != null && dragger.Held == null)
            {
                Vector3 c = logic.CushionUnder ? new Vector3(CushionSpot.x, CushionSpot.y, cushion.position.z) : cushionHome;
                cushion.position = c;
                ladder.position = ladderHome;
                books.position = booksHome;
            }
            for (int i = 0; i < bookArt.Length; i++) { bookArt[i].localPosition = bookHome[i]; bookArt[i].localRotation = bookRot[i]; }
            toppling = ladderBusy = false;
            if (cushionItem != null) cushionItem.Enabled = !logic.InputLocked;
            if (ladderItem != null) ladderItem.Enabled = !logic.InputLocked;
            if (booksItem != null) booksItem.Enabled = !logic.InputLocked;
            if (penMoving) { penMoving = false; if (penInHand) PenToHand(true); }
            jarArt.localPosition = jarArtPos;
            jarArt.localRotation = Quaternion.identity;
            HalloweenKit.Show(thought, false);
            thought.localScale = thoughtScale;
            HalloweenKit.Show(catMark, false);
            stretch = 0f;
            say?.Hide();
        }

        void OnDisable()
        {
            // pausa/pubblicità/livello spento: le animazioni si fermano con l'oggetto, si torna a uno stato valido
            CancelGrips();
            if (!ready) return;
            if (!logic.InputLocked) { RestPose(); Present(0f); }
        }

        void OnEnable()
        {
            if (!ready || IsFinished) return;
            if (logic.InputLocked)
            {
                // spento durante la festa: il barattolo è comunque sul cuscino
                finishing = true;
                if (logic.TryComplete()) Win();
            }
        }

        void OnApplicationPause(bool paused) { if (paused) CancelGrips(); }
        void OnApplicationFocus(bool focus) { if (!focus) CancelGrips(); }

        /// <summary>Si lascia tutto: l'oggetto preso torna a casa (il cuscino, se era sotto la mensola, ci torna), il gatto si sgonfia.</summary>
        void CancelGrips()
        {
            grips.Clear();
            if (dragger != null && dragger.Held != null)
            {
                HwDragger.Item it = dragger.Release();
                if (it == cushionItem && ready && CatBurglarLogic.InCushionZone(cushion.position.x, cushion.position.y) && !logic.InputLocked)
                {
                    logic.SetCushion(true);
                    dragger.SendTo(it, new Vector3(CushionSpot.x, CushionSpot.y, cushion.position.z), 0.2f, 0f);
                }
                else if (it != null) dragger.SendHome(it);
            }
            if (stretch > 0.01f) { flopFrom = stretch; flopT = 0f; }
            stretch = 0f;
        }

        void Update()
        {
            toast?.Tick();
            say?.Tick();
            if (IsFinished) return;
            float dt = Time.deltaTime;
            if (finishing) { Present(dt); return; }
            if (Time.timeScale <= 0f) { CancelGrips(); return; }
            switch (logic.Tick(dt))
            {
                case CatEvent.Pounce: CatSound.Play(this, "pounce", 0.45f); break;
                case CatEvent.Landed: landT = 0f; CatSound.Play(this, "land", 0.5f); break;
                case CatEvent.StartedPushing: StartedPushing(); break;
                case CatEvent.Nudge: CatSound.Play(this, "scrape", 0.55f); break;
                case CatEvent.JarTipped: CatSound.Play(this, "whoosh", 0.6f); break;
                case CatEvent.JarShattered: Shattered(); break;
                case CatEvent.NewJar: NewJar(); break;
                case CatEvent.JarOnCushion: StartCoroutine(Victory()); Present(dt); return;
            }
            Present(dt);
            if (logic.InputLocked) { CancelGrips(); return; }
#if UNITY_EDITOR
            EditorKeys();
            if (logic.InputLocked || finishing) return;
#endif
            if (Inputs.Shake() > ShakeLevel && Time.time >= shakeReadyAt) { shakeReadyAt = Time.time + ShakeCooldown; ShakePhone(); }
            ReadFingers();
        }

        // ---------- Presentazione (tutto letto dalla logica a ogni fotogramma) ----------

        void Present(float dt)
        {
            perkT += dt; landT += dt; flopT += dt; wiggleT += dt; wobbleT += dt; flickerT += dt; newJarT += dt; plopT += dt;
            PoseCat();
            PoseJar();
            PoseDot();
            Idle();
            Eyes(dt);
        }

        Vector2 CatFeet()
        {
            if (logic.Jumping)
            {
                float p = logic.JumpProgress;
                float y0 = CatBurglarLogic.SurfaceY(logic.JumpFrom), y1 = CatBurglarLogic.SurfaceY(logic.JumpTo);
                float arc = Mathf.Max(0.45f, Mathf.Abs(y1 - y0) * 0.35f + 0.35f);
                float e = p * p * (3f - 2f * p);
                return new Vector2(Mathf.Lerp(logic.JumpFromX, logic.JumpToX, e), Mathf.Lerp(y0, y1, e) + arc * Mathf.Sin(p * Mathf.PI));
            }
            return new Vector2(logic.CatX, CatBurglarLogic.SurfaceY(logic.Perch));
        }

        void PoseCat()
        {
            Vector2 f = CatFeet();
            cat.position = new Vector3(f.x, f.y, cat.position.z);
            float sx = 1f, sy = 1f, rot = 0f;
            if (logic.Jumping)
            {
                float p = logic.JumpProgress;
                sy = 1f + 0.18f * Mathf.Sin(p * Mathf.PI);
                sx = 1f - 0.1f * Mathf.Sin(p * Mathf.PI);
                rot = (logic.JumpToX > logic.JumpFromX ? -12f : 12f) * Mathf.Sin(p * Mathf.PI);
            }
            else if (stretch > 0f)
            {
                // tirato su col dito: si allunga come un liquido
                sy = 1f + stretch;
                sx = 1f / Mathf.Sqrt(sy);
                rot = Mathf.Sin(Time.time * 9f) * 4f;
            }
            else if (flopT < 0.7f)
            {
                // ricola giù: si schiaccia, poi torna gatto
                float k = flopT / 0.7f;
                float s = Mathf.Lerp(flopFrom, 0f, Mathf.Min(1f, k * 3f));
                float squash = k < 0.33f ? 0f : 0.35f * Mathf.Sin((k - 0.33f) / 0.67f * Mathf.PI);
                sy = 1f + s - squash;
                sx = 1f + squash * 0.8f;
            }
            else if (landT < 0.18f)
            {
                float k = Mathf.Sin(landT / 0.18f * Mathf.PI);
                sy = 1f - 0.15f * k;
                sx = 1f + 0.12f * k;
            }
            else sy = 1f + 0.015f * Mathf.Sin(Time.time * 2.5f);   // respira
            if (perkT < 0.35f) { float k = Mathf.Sin(perkT / 0.35f * Mathf.PI); sy *= 1f + 0.12f * k; sx *= 1f + 0.05f * k; }
            if (wiggleT < 0.9f) rot += Mathf.Sin(wiggleT * 24f) * 7f * (1f - wiggleT / 0.9f);   // scodinzola (vorrebbe saltare)
            if (logic.Pushing) rot += -6f * logic.PawOut;   // si sporge verso il barattolo
            catBody.localScale = new Vector3(bodyScale.x * sx, bodyScale.y * sy, bodyScale.z);
            catBody.localRotation = Quaternion.Euler(0f, 0f, rot);
            bool pawOn = logic.Pushing && logic.PawOut > 0.01f;
            HalloweenKit.Show(paw, pawOn);
            if (pawOn) paw.localScale = new Vector3(0.3f + 0.7f * logic.PawOut, 1f, 1f);
            HalloweenKit.Show(catMark, perkT < 0.9f);
            if (catMark != null && perkT < 0.9f) catMark.transform.localScale = Vector3.one * (1f + 0.3f * Mathf.Sin(Mathf.Min(1f, perkT / 0.25f) * Mathf.PI));
            if (catBubble != null && catBubble.gameObject.activeSelf)
            {
                Vector2 b = HalloweenKit.ClampSafe(new Vector2(f.x + (f.x > 0.6f ? -0.95f : 0.95f), f.y + 1.15f));
                catBubble.transform.position = new Vector3(b.x, Mathf.Min(b.y, 2.0f), bubbleHome.z);
            }
        }

        void PoseJar()
        {
            CookieJarStage st = logic.JarStage;
            bool broken = st == CookieJarStage.Broken;
            HalloweenKit.Show(jar, !broken);
            HalloweenKit.Show(shards, broken);
            if (broken) return;
            float shelfY = CatBurglarLogic.SurfaceY(CatPerch.Shelf);
            float rot = 0f;
            if (st == CookieJarStage.OnShelf)
            {
                jar.position = new Vector3(logic.JarX, shelfY, jar.position.z);
                if (logic.Pushing) rot = -4f * logic.PawOut;
                if (wobbleT < 0.9f) rot += Mathf.Sin(wobbleT * 30f) * 9f * (1f - wobbleT / 0.9f);
            }
            else
            {
                float p = logic.FallProgress;
                bool soft = logic.CushionUnder || st != CookieJarStage.Falling;
                float endY = soft ? JarOnCushionY : FloorY;
                float x = Mathf.Lerp(CatBurglarLogic.EdgeX, CatBurglarLogic.LandX, Mathf.Min(1f, p * 2.5f));
                jar.position = new Vector3(x, Mathf.Lerp(shelfY, endY, p * p), jar.position.z);
                rot = -40f * Mathf.Sin(p * Mathf.PI);
            }
            jarArt.localRotation = Quaternion.Euler(0f, 0f, rot);
            float pop = newJarT < 0.3f ? Mathf.Min(1f, newJarT / 0.3f) : 1f;
            float squash = plopT < 0.35f ? Mathf.Sin(plopT / 0.35f * Mathf.PI) : 0f;
            jar.localScale = new Vector3(pop * (1f + 0.15f * squash), pop * (1f - 0.18f * squash), 1f);
            if (finishing) cushion.localScale = new Vector3(cushionScale.x * (1f + 0.12f * squash), cushionScale.y * (1f - 0.3f * squash), cushionScale.z);
        }

        void PoseDot()
        {
            bool on = logic.LaserOn && !logic.InputLocked;
            HalloweenKit.Show(dot, on);
            HalloweenKit.Show(beamRoot, on && penInHand && !penMoving);
            if (!on) return;
            Vector2 jit = new Vector2(Mathf.Sin(Time.time * 37f), Mathf.Cos(Time.time * 29f)) * 0.012f;
            dot.position = new Vector3(dotWorld.x + jit.x, dotWorld.y + jit.y, dot.position.z);
            float a = flickerT < 0.8f ? (Mathf.Repeat(flickerT * 14f, 1f) < 0.5f ? 0.25f : 1f) : 1f;
            HalloweenKit.SetAlpha(dotArt, a);
            if (penInHand && !penMoving)
            {
                Vector2 tip = penTip.position;
                Vector2 d = (Vector2)dot.position - tip;
                // il puntatore "in mano" punta sempre verso il puntino
                float ang = Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;
                pen.rotation = Quaternion.Euler(0f, 0f, ang);
                tip = penTip.position;
                d = (Vector2)dot.position - tip;
                beamRoot.position = new Vector3(tip.x, tip.y, beamRoot.position.z);
                beamRoot.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg);
                beamRoot.localScale = new Vector3(d.magnitude, 1f, 1f);
                HalloweenKit.SetAlpha(beam, 0.45f * a);
            }
        }

        /// <summary>L'indizio a riposo: ogni tanto il gatto guarda il barattolo, e ogni tanto ci pensa su (lo vede cadere).</summary>
        void Idle()
        {
            float t = Time.time;
            bool busy = logic.Jumping || logic.Pushing || logic.JarStage != CookieJarStage.OnShelf || stretch > 0f;
            if (t >= glanceAt) { glanceUntil = t + 1.3f; glanceAt = t + Random.Range(4.5f, 6.5f); }
            bool showThought = !busy && t < thinkUntil;
            if (t >= thinkAt)
            {
                thinkAt = t + Random.Range(10f, 13f);
                if (!busy && !(logic.LaserOn && logic.DotPerch != CatPerch.None)) { thinkUntil = t + 1.8f; glanceUntil = thinkUntil; showThought = true; }
            }
            HalloweenKit.Show(thought, showThought);
            if (showThought)
            {
                Vector2 f = CatFeet();
                float k = Mathf.Min(1f, (1.8f - (thinkUntil - t)) / 0.15f);
                thought.position = new Vector3(Mathf.Clamp(f.x + 0.75f, -1.9f, 1.9f), Mathf.Min(f.y + 1.45f, 2.0f), thought.position.z);
                thought.localScale = thoughtScale * k;
            }
        }

        /// <summary>Gli occhi seguono quello che si muove: il puntino, l'oggetto preso, il dito; altrimenti, ogni tanto, il barattolo.</summary>
        void Eyes(float dt)
        {
            Vector2 eye = (Vector2)catBody.position + Vector2.up * 0.66f;
            Vector2 target = eye + Vector2.down * 0.3f;
            bool forward = false;
            if (smug) target = (Vector2)jar.position;
            else if (logic.Staring) forward = true;   // il classico sguardo, dritto verso di te
            else if (logic.Pushing || logic.JarStage == CookieJarStage.Falling) target = jar.position;
            else if (dragger != null && dragger.Held != null) target = dragger.Held.T.position;
            else if (logic.LaserOn) target = dotWorld;
            else if (Time.time < glanceUntil) target = jar.position;
            else if (grips.Count > 0) foreach (Grip g in grips.Values) { target = g.last; break; }
            lookAt = Vector2.Lerp(lookAt, target, Mathf.Min(1f, dt * 10f + (dt <= 0f ? 1f : 0f)));
            Vector2 d = lookAt - eye;
            Vector2 off = forward || d.sqrMagnitude < 1e-4f ? Vector2.zero : d.normalized * PupilReach * Mathf.Min(1f, d.magnitude / 0.6f);
            pupilL.localPosition = pupilLHome + (Vector3)off;
            pupilR.localPosition = pupilRHome + (Vector3)off;
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
                Drag(g);
                if (f.ended) End(f.id, g);
                if (finishing || logic.InputLocked) { grips.Clear(); return; }
            }
            if (fingers.Count == 0 && grips.Count > 0)
            {
                gone.Clear();
                foreach (int id in grips.Keys) gone.Add(id);
                foreach (int id in gone) End(id, grips[id]);
            }
        }

        bool Holding(Kind kind)
        {
            foreach (Grip g in grips.Values) if (g.kind == kind) return true;
            return false;
        }

        Grip Begin(Vector2 w)
        {
            var g = new Grip { start = w, last = w, t0 = Time.time, kind = Kind.Other };
            if (finishing || logic.InputLocked) { g.kind = Kind.None; return g; }
            // il puntino rosso ha la precedenza (area generosa): è il gioco principale
            if (logic.LaserOn && !Holding(Kind.Dot) && Vector2.Distance(w, dotWorld) <= DotGrab) { g.kind = Kind.Dot; g.offset = dotWorld - w; return g; }
            if (!penMoving && penArea.OverlapPoint(w)) { g.kind = Kind.Pen; return g; }
            if (dragger.Held == null && dragger.TryPick(w) != null)
            {
                g.kind = Kind.Item;
                if (dragger.Held == cushionItem) logic.SetCushion(false);   // in mano non è sotto la mensola
                return g;
            }
            if (!logic.Jumping && !Holding(Kind.Cat) && catArea.OverlapPoint(w)) { g.kind = Kind.Cat; return g; }
            if (jar.gameObject.activeInHierarchy && jarArea.OverlapPoint(w)) { g.kind = Kind.Jar; return g; }
            if (logic.LaserOn && !Holding(Kind.Dot))
            {
                // un dito altrove col laser acceso: il puntino salta lì (un po' sopra il dito, per vederlo)
                g.kind = Kind.Dot;
                g.offset = Vector2.up * DotLift;
                MoveDot(w + g.offset);
            }
            return g;
        }

        void Drag(Grip g)
        {
            switch (g.kind)
            {
                case Kind.Dot: MoveDot(g.last + g.offset); break;
                case Kind.Item: dragger.MoveHeld(g.last); break;
                case Kind.Cat:
                    if (logic.Jumping) { g.kind = Kind.None; stretch = 0f; break; }
                    stretch = Mathf.Clamp((g.last.y - g.start.y) * 0.9f, 0f, MaxStretch);
                    break;
            }
        }

        void End(int id, Grip g)
        {
            grips.Remove(id);
            bool tap = !g.moved && Time.time - g.t0 <= TapTime;
            switch (g.kind)
            {
                case Kind.Pen: if (tap || !g.moved) TogglePen(); break;
                case Kind.Item: DropItem(tap); break;
                case Kind.Cat: ReleaseCat(g, tap); break;
                case Kind.Jar: if (tap) TapJar(); break;
            }
        }

        // ---------- Il laser e il puntino ----------

        void TogglePen()
        {
            switch (logic.ToggleLaser())
            {
                case LaserResult.On:
                    CatSound.Play(this, "click", 0.6f);
                    Haptics.Tap();
                    if (!penInHand) { penInHand = true; PenToHand(false); }
                    // il puntino compare accanto al gatto, tremola; il gatto drizza le orecchie
                    dotWorld = new Vector2(logic.DotX, CatBurglarLogic.SurfaceY(logic.DotPerch) + 0.04f);
                    flickerT = 0f;
                    perkT = 0f;
                    CatSound.Play(this, "chirp", 0.5f);
                    toast.Show(logic.Perks == 1 ? "Click. A tiny red dot." : "Click.", 1.4f);
                    break;
                case LaserResult.Off:
                    CatSound.Play(this, "click", 0.45f);
                    toast.Show("Click. Off.", 1.0f);
                    break;
            }
        }

        void PenToHand(bool instant)
        {
            Vector3 to = new Vector3(penHand.position.x, penHand.position.y, pen.position.z);
            if (instant || !isActiveAndEnabled) { pen.position = to; pen.rotation = Quaternion.Euler(0f, 0f, 120f); penMoving = false; return; }
            StartCoroutine(PenFly(to));
        }

        IEnumerator PenFly(Vector3 to)
        {
            penMoving = true;
            Vector3 from = pen.position;
            Quaternion r0 = pen.rotation, r1 = Quaternion.Euler(0f, 0f, 120f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.3f)
            {
                float e = 1f - (1f - k) * (1f - k);
                pen.position = Vector3.Lerp(from, to, e) + Vector3.up * (0.4f * Mathf.Sin(k * Mathf.PI));
                pen.rotation = Quaternion.Slerp(r0, r1, e);
                yield return null;
            }
            pen.position = to;
            penMoving = false;
        }

        void MoveDot(Vector2 w)
        {
            dotWorld = HalloweenKit.ClampSafe(w);
            DotResult r = logic.PointDot(dotWorld.x, dotWorld.y);
            if (r == DotResult.TooHigh && logic.TooHighs != tooHighSeen)
            {
                tooHighSeen = logic.TooHighs;
                wiggleT = 0f;
                if (Time.time >= tooHighReadyAt)
                {
                    tooHighReadyAt = Time.time + TooHighCooldown;
                    CatSound.Play(this, "chirp", 0.45f);
                    toast.Show(TooHighLines[tooHighLines++ % TooHighLines.Length], 1.5f);
                }
            }
        }

        // ---------- Le false piste ----------

        void TapJar()
        {
            if (logic.TapJar() == 0) return;
            Sfx.Play(this, "tick", 0.4f);
            toast.Show(JarTapLines[jarTapLines++ % JarTapLines.Length], 1.4f);
        }

        void ShakePhone()
        {
            if (logic.Shake() == 0) return;
            wobbleT = 0f;
            Haptics.Light();
            CatSound.Play(this, "rattle", 0.6f);
            toast.Show(ShakeLines[shakeLines++ % ShakeLines.Length], 1.9f);
        }

        void ReleaseCat(Grip g, bool tap)
        {
            float s = stretch;
            stretch = 0f;
            if (s >= CatPull * 0.9f || g.last.y - g.start.y >= CatPull)
            {
                if (logic.DragCatUp() == 0) return;
                flopFrom = s;
                flopT = 0f;
                CatSound.Play(this, "slide", 0.55f);
                toast.Show(DragLines[dragLines++ % DragLines.Length], 1.7f);
                return;
            }
            if (s > 0.01f) { flopFrom = s; flopT = 0.5f; }
            if (tap)
            {
                perkT = 0.2f;
                CatSound.Play(this, catTapLines % 3 == 1 ? "purr" : "meow", 0.45f);
                say.Show(CatTapLines[catTapLines++ % CatTapLines.Length], 1.2f);
            }
        }

        void DropItem(bool tap)
        {
            HwDragger.Item it = dragger.Release();
            if (it == null) return;
            Vector2 at = it.T.position;
            bool under = CatBurglarLogic.InCushionZone(at.x, at.y);
            if (it == cushionItem)
            {
                if (under)
                {
                    logic.SetCushion(true);
                    dragger.SendTo(it, new Vector3(CushionSpot.x, CushionSpot.y, cushion.position.z), 0.2f, 0.1f);
                    CatSound.Play(this, "plop", 0.4f);
                    toast.Show(CushionLines[cushionLines++ % CushionLines.Length], 1.4f);
                }
                else
                {
                    logic.SetCushion(false);
                    dragger.SendHome(it);
                }
                return;
            }
            if (it == ladderItem)
            {
                if (under && !ladderBusy) { StartCoroutine(LadderTry()); return; }
                dragger.SendHome(it);
                if (tap) toast.Show(LadderTapLines[ladderTapLines++ % LadderTapLines.Length], 1.3f);
                return;
            }
            if (it == booksItem)
            {
                if (under && !toppling) { StartCoroutine(Topple()); return; }
                dragger.SendHome(it);
                if (tap) { StartCoroutine(HalloweenKit.Wiggle(bookArt[bookArt.Length - 1], 8f, 0.4f, 30f)); toast.Show(BookTapLines[bookTapLines++ % BookTapLines.Length], 1.3f); }
            }
        }

        /// <summary>La scaletta sotto la mensola: ci si sale col pensiero... e la mensola è ancora lassù. Poi torna a posto.</summary>
        IEnumerator LadderTry()
        {
            ladderBusy = true;
            ladderItem.Enabled = false;
            ladder.position = new Vector3(CatBurglarLogic.LandX - 0.35f, FloorY, ladder.position.z);
            CatSound.Play(this, "clank", 0.6f);
            if (logic.LadderUnderShelf() > 0) toast.Show(LadderLines[ladderLines++ % LadderLines.Length], 1.8f);
            yield return HalloweenKit.Shake(ladder, 0.03f, 0.3f);
            yield return new WaitForSeconds(0.9f);
            dragger.SendHome(ladderItem, 0.35f);
            yield return new WaitForSeconds(0.35f);
            ladderItem.Enabled = !logic.InputLocked;
            ladderBusy = false;
        }

        /// <summary>I libri impilati sotto la mensola: traballano, crollano, si sparpagliano e poi tornano in pila.</summary>
        IEnumerator Topple()
        {
            toppling = true;
            booksItem.Enabled = false;
            books.position = new Vector3(CatBurglarLogic.LandX - 0.3f, FloorY, books.position.z);
            logic.StackBooks();
            // traballano...
            for (float k = 0f; k < 0.6f; k += Time.deltaTime)
            {
                float a = Mathf.Sin(k * 22f) * 6f * (0.4f + k);
                for (int i = 0; i < bookArt.Length; i++)
                {
                    bookArt[i].localRotation = bookRot[i] * Quaternion.Euler(0f, 0f, a * (i + 1) * 0.6f);
                    bookArt[i].localPosition = bookHome[i] + new Vector3(Mathf.Sin(k * 22f) * 0.03f * i, 0f, 0f);
                }
                yield return null;
            }
            // ...e giù
            CatSound.Play(this, "topple", 0.7f);
            Haptics.Medium();
            toast.Show(BookLines[bookLines++ % BookLines.Length], 1.6f);
            var from = new Vector3[bookArt.Length];
            for (int i = 0; i < bookArt.Length; i++) from[i] = bookArt[i].localPosition;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.35f)
            {
                for (int i = 0; i < bookArt.Length; i++)
                {
                    float side = i % 2 == 0 ? -1f : 1f;
                    Vector3 to = new Vector3(side * (0.35f + 0.25f * i), bookHome[0].y, 0f);
                    bookArt[i].localPosition = Vector3.Lerp(from[i], to, k) + Vector3.up * (0.2f * Mathf.Sin(k * Mathf.PI));
                    bookArt[i].localRotation = bookRot[i] * Quaternion.Euler(0f, 0f, side * -20f * k * (i % 3));
                }
                yield return null;
            }
            yield return new WaitForSeconds(0.9f);
            for (int i = 0; i < bookArt.Length; i++) { bookArt[i].localPosition = bookHome[i]; bookArt[i].localRotation = bookRot[i]; }
            dragger.SendHome(booksItem, 0.35f);
            yield return new WaitForSeconds(0.35f);
            booksItem.Enabled = !logic.InputLocked;
            toppling = false;
        }

        // ---------- Il gatto sulla mensola ----------

        void StartedPushing()
        {
            CatSound.Play(this, "chirp", 0.4f);
            toast.Show(logic.CushionUnder ? "Oh no. It's doing the cat thing." : "Uh-oh. It's doing the cat thing.", 1.8f);
        }

        void Shattered()
        {
            CatSound.Play(this, "crash", 0.85f);
            Haptics.Medium();
            perkT = 0f;
            toast.Show("Oops. Cookies everywhere.", 2.0f);
            if (logic.LaserOn) dotWorld = new Vector2(logic.DotX, CatBurglarLogic.SurfaceY(logic.DotPerch) + 0.04f);
        }

        void NewJar()
        {
            newJarT = 0f;
            CatSound.Play(this, "pop", 0.5f);
            toast.Show("Good thing there's a spare jar.", 1.8f);
        }

        /// <summary>Plof sul cuscino: il gatto guarda giù soddisfatto, le due scritte, i coriandoli, vittoria.</summary>
        IEnumerator Victory()
        {
            finishing = true;
            grips.Clear();
            if (dragger.Held != null) dragger.Cancel();
            stretch = 0f;
            plopT = 0f;
            CatSound.Play(this, "plop", 0.8f);
            Haptics.Medium();
            yield return new WaitForSeconds(0.35f);
            smug = true;
            perkT = 0.5f;
            CatSound.Play(this, "purr", 0.5f);
            say.Show("Mrrp.", 1.4f);
            toast.Show("THE CAT DID IT.", 1.5f);
            yield return new WaitForSeconds(1.4f);
            toast.Show("TECHNICALLY, SO DID YOU.", 2.2f);
            yield return new WaitForSeconds(0.5f);
            yield return Celebrate(1.3f);
            if (logic.TryComplete()) Win();
        }

#if UNITY_EDITOR
        // ---------- Tasti di prova nell'Editor ----------

        void EditorKeys()
        {
            if (Input.GetKeyDown(KeyCode.L)) TogglePen();
            for (int i = 0; i < CatBurglarLogic.PerchCount; i++)
                if (Input.GetKeyDown(KeyCode.Alpha0 + i)) EditorDot((CatPerch)i);
            if (Input.GetKeyDown(KeyCode.C)) EditorCushion(!logic.CushionUnder);
            if (Input.GetKeyDown(KeyCode.K))
            {
                // un passo della soluzione: cuscino, laser, poi il puntino un ripiano più su
                if (!logic.CushionUnder) EditorCushion(true);
                else if (!logic.LaserOn) TogglePen();
                else if (!logic.Jumping && logic.Perch != CatPerch.Shelf) EditorDot(logic.Perch + 1);
            }
            if (Input.GetKeyDown(KeyCode.J)) TapJar();
            if (Input.GetKeyDown(KeyCode.D) && logic.DragCatUp() > 0) { flopFrom = 1.2f; flopT = 0f; toast.Show(DragLines[dragLines++ % DragLines.Length], 1.7f); }
            if (Input.GetKeyDown(KeyCode.B) && !toppling && booksItem.Enabled) StartCoroutine(Topple());
            if (Input.GetKeyDown(KeyCode.P) && !ladderBusy && ladderItem.Enabled) StartCoroutine(LadderTry());
        }

        void EditorDot(CatPerch p)
        {
            if (!logic.LaserOn) return;
            float x = (CatBurglarLogic.MinX(p) + CatBurglarLogic.MaxX(p)) * 0.5f;
            MoveDot(new Vector2(x, CatBurglarLogic.SurfaceY(p) + 0.1f));
        }

        void EditorCushion(bool under)
        {
            if (dragger.Held != null) return;
            if (!logic.SetCushion(under)) return;
            dragger.SendTo(cushionItem, under ? new Vector3(CushionSpot.x, CushionSpot.y, cushion.position.z) : cushionItem.Home, 0.25f);
        }
#endif

        /// <summary>Suoni sintetizzati del livello 269 (prefisso v72_a_): clic del laser, salti, vetro che striscia, crollo, plof.</summary>
        static class CatSound
        {
            const int Rate = 22050;
            static readonly Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>();
            static readonly System.Random rnd = new System.Random(72);

            public static void Play(Component owner, string kind, float volume = 0.6f)
            {
                if (owner == null) return;
                if (owner is Behaviour b && !b.isActiveAndEnabled) return;
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
                    case "click":    // l'interruttore del laser
                        c = Make(kind, 0.06f, t => (Mathf.Sin(t * 2f * Mathf.PI * 2200f) * 0.5f + N() * 0.3f) * Mathf.Exp(-t * 90f));
                        break;
                    case "chirp":    // il "chiacchiericcio" del gatto che vede il puntino: tre trilli acuti
                        c = Make(kind, 0.36f, t => Mathf.Sin(t * 2f * Mathf.PI * (900f + 300f * Mathf.Sin(t * 60f))) * 0.3f * Mathf.Sin((t % 0.12f) / 0.12f * Mathf.PI));
                        break;
                    case "meow":     // miao: sale e scende
                        c = Make(kind, 0.45f, t =>
                        {
                            float f = 520f + 260f * Mathf.Sin(t / 0.45f * Mathf.PI);
                            return (Mathf.Sin(t * 2f * Mathf.PI * f) * 0.35f + Mathf.Sin(t * 4f * Mathf.PI * f) * 0.12f) * Mathf.Sin(t / 0.45f * Mathf.PI);
                        });
                        break;
                    case "purr":     // fusa: rombo basso a impulsi
                        c = Make(kind, 0.7f, t => (Mathf.Sin(t * 2f * Mathf.PI * 60f) * 0.4f + N() * 0.15f) * (0.5f + 0.5f * Mathf.Sin(t * 2f * Mathf.PI * 24f)) * Mathf.Sin(t / 0.7f * Mathf.PI));
                        break;
                    case "pounce":   // lo slancio
                        {
                            float lp = 0f;
                            c = Make(kind, 0.22f, t => { lp += (N() - lp) * 0.2f; return lp * 0.8f * Mathf.Sin(t / 0.22f * Mathf.PI); });
                        }
                        break;
                    case "land":     // zampe morbide che atterrano
                        c = Make(kind, 0.12f, t => Mathf.Sin(t * 2f * Mathf.PI * Mathf.Lerp(140f, 80f, t / 0.12f)) * 0.6f * Mathf.Exp(-t * 35f));
                        break;
                    case "scrape":   // il barattolo che striscia sul legno
                        c = Make(kind, 0.25f, t => (N() * 0.3f + Mathf.Sin(t * 2f * Mathf.PI * 330f) * 0.15f) * Mathf.Sin(t / 0.25f * Mathf.PI));
                        break;
                    case "whoosh":   // giù!
                        {
                            float lp = 0f;
                            c = Make(kind, 0.6f, t => { lp += (N() - lp) * Mathf.Lerp(0.05f, 0.3f, t / 0.6f); return lp * 1.2f * Mathf.Sin(t / 0.6f * Mathf.PI); });
                        }
                        break;
                    case "crash":    // vetro in mille pezzi
                        c = Make(kind, 0.8f, t => (N() * 0.55f * Mathf.Exp(-t * 6f) + Mathf.Sin(t * 2f * Mathf.PI * 3100f) * 0.2f * Mathf.Exp(-t * 9f)
                            + Mathf.Sin(t * 2f * Mathf.PI * 4700f) * 0.15f * Mathf.Exp(-((t * 9f) % 1f) * 6f) * Mathf.Exp(-t * 3f)) * Mathf.Min(1f, t * 400f));
                        break;
                    case "plop":     // sul cuscino
                        c = Make(kind, 0.25f, t => Mathf.Sin(t * 2f * Mathf.PI * Mathf.Lerp(220f, 90f, t / 0.25f)) * 0.7f * Mathf.Exp(-t * 14f));
                        break;
                    case "pop":      // il barattolo nuovo
                        c = Make(kind, 0.12f, t => Mathf.Sin(t * 2f * Mathf.PI * Mathf.Lerp(500f, 900f, t / 0.12f)) * 0.4f * Mathf.Sin(t / 0.12f * Mathf.PI));
                        break;
                    case "rattle":   // il barattolo che traballa
                        c = Make(kind, 0.4f, t => (Mathf.Sin(t * 2f * Mathf.PI * 1200f) * 0.25f + N() * 0.2f) * Mathf.Exp(-((t * 12f) % 1f) * 8f) * (1f - t / 0.4f));
                        break;
                    case "slide":    // il gatto liquido che cola giù
                        c = Make(kind, 0.5f, t => Mathf.Sin(t * 2f * Mathf.PI * Mathf.Lerp(700f, 160f, t / 0.5f)) * 0.3f * Mathf.Sin(t / 0.5f * Mathf.PI));
                        break;
                    case "clank":    // la scaletta
                        c = Make(kind, 0.3f, t => (Mathf.Sin(t * 2f * Mathf.PI * 620f) * 0.35f + Mathf.Sin(t * 2f * Mathf.PI * 1430f) * 0.2f + N() * 0.1f) * Mathf.Exp(-t * 14f));
                        break;
                    case "topple":   // i libri che cadono: tre tonfi
                        c = Make(kind, 0.5f, t => (Mathf.Sin(t * 2f * Mathf.PI * 110f) * 0.5f + N() * 0.3f) * Mathf.Exp(-((t * 6f) % 1f) * 10f) * (1f - t / 0.5f));
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
                AudioClip clip = AudioClip.Create("v72_a_" + name, n, 1, Rate, false);
                clip.SetData(data, 0);
                return clip;
            }
        }
    }
}
