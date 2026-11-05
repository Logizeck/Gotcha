using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels.V64;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 13 della roadmap "The Aristocratic Cat" (id aristocratic_cat) — "GET THE CAT DOWN." Un gatto snob bianco
    /// (monocolo d'oro, farfallino) è sdraiato in cima a un armadio altissimo. Sul pavimento: una scala, una ciotola di
    /// porcellana e un topolino-giocattolo. Gli oggetti non lo convincono: la scala appoggiata all'armadio ("A LADDER? How
    /// common.", resta lì finché non la si sposta), la ciotola portata su ("I dine at eight.", torna a terra), il
    /// giocattolo (occhiata annoiata, "Please.", torna a terra); toccarlo lo infastidisce ("Do you mind?"). Tutto
    /// reversibile, mai una sconfitta.
    /// Soluzione: capovolgere davvero il telefono e tenerlo capovolto ~0.7 s: il gatto si aggrappa ("Hey! HEY!", unghie
    /// che graffiano), poi scivola lungo il piano, cade (giravolta da gatto, niente fisica: animazione scritta), rimbalza
    /// sul pavimento, si ricompone e si liscia il pelo: "I MEANT TO DO THAT!" (fumetto, LevelReward, Win). Il mondo NON
    /// ruota. Raddrizzare prima del tempo annulla (il gatto risale composto). Un telefono già capovolto all'avvio non
    /// vale finché non è stato visto non capovolto. Logica in Gotcha.Levels.V64.AristocraticCatLogic.
    /// Tasti di prova nell'Editor: U capovolge/raddrizza (simulato; anche la freccia su), J inclinazione a metà strada
    /// (avanti/indietro), T tocca il gatto, L appoggia la scala all'armadio, K riporta la scala al muro, F offre la
    /// ciotola, Y offre il giocattolo, P pausa simulata (dita e tenuta annullate), R ricomincia il livello.
    /// </summary>
    public class V78AristocraticCatLevel : LevelController
    {
        [Header("Gatto (radice ai piedi, sul piano dell'armadio)")]
        [SerializeField] Transform cat;
        [SerializeField] Transform catPivot;        // perno al centro del corpo (inclinazione e giravolta)
        [SerializeField] Transform poseLie;
        [SerializeField] Transform poseAnnoyed;
        [SerializeField] Transform poseCling;
        [SerializeField] Transform poseFall;
        [SerializeField] Transform poseSit;
        [SerializeField] Transform poseGroom;
        [SerializeField] Collider2D catArea;
        [SerializeField] SpeechBubble catBubble;
        [SerializeField] Transform bubbleHolder;    // genitore del fumetto: si sposta lui (il fumetto gestisce da sé il rientro nello schermo)
        [Header("Oggetti sul pavimento")]
        [SerializeField] Transform ladder;
        [SerializeField] Collider2D ladderArea;
        [SerializeField] Transform ladderLean;      // segnaposto: scala appoggiata all'armadio (posizione e rotazione)
        [SerializeField] Transform bowl;
        [SerializeField] Collider2D bowlArea;
        [SerializeField] Transform toy;
        [SerializeField] Collider2D toyArea;
        [Header("Effetti")]
        [SerializeField] SpriteRenderer scratches;  // graffi sul bordo dell'armadio
        [SerializeField] SpriteRenderer dust;       // sbuffo dell'atterraggio
        [SerializeField] SpriteRenderer sparkle;    // luccichio del pelo lisciato
        [Header("Geometria (coordinate del mondo)")]
        [SerializeField] float zoneMinX = -0.45f;   // zona "verso il gatto" per ciotola e giocattolo
        [SerializeField] float zoneMaxX = 2.45f;
        [SerializeField] float zoneMinY = 0.0f;
        [SerializeField] float ladderMinX = -1.2f;  // la scala lasciata con il centro oltre qui si appoggia all'armadio
        [SerializeField] float edgeX = 0.45f;       // dove arriva la radice del gatto scivolando (testa oltre il bordo)
        [SerializeField] Vector2 landAt = new Vector2(0f, -3.3f);
        [SerializeField] Vector2 bubblePerched = new Vector2(-0.03f, 1.3f);   // fumetto rispetto alla radice del gatto
        [SerializeField] Vector2 bubbleFloor = new Vector2(0.52f, 2.1f);
        [SerializeField] LevelReward reward;

        const string Compliment = "I MEANT TO DO THAT!";
        const string HintLine = "Not even gravity moves me.";
        static readonly string[] PokeLines = { "Do you mind?", "I'm resting.", "Do you MIND?" };
        static readonly string[] LadderLines = { "A LADDER? How common.", "Still a ladder. Still common." };
        static readonly string[] BowlLines = { "I dine at eight.", "It is NOT eight." };
        static readonly string[] ToyLines = { "Please.", "I'm not a kitten." };
        static readonly string[] ClimbLines = { "Hmph. As I was.", "I was NOT slipping.", "Ahem. Carry on." };
        const float TapSlop = 0.18f, SayGap = 0.35f;
        const float LadderHomeRot = -3f;

        enum Kind { None, Item, Cat }

        AristocraticCatLogic logic;
        Camera cam;
        HwDragger dragger;
        HwDragger.Item ladderItem, bowlItem, toyItem;
        Vector3 catHome, ladderHome, bowlHome, toyHome;
        Quaternion leanRot = Quaternion.identity;
        Kind grip;
        Vector2 gripStart, gripLast;
        bool gripMoved, captured, finishing;
        float shownSlide, shownTilt, annoyedUntil, sayHideAt = -1f, lastSayAt = -10f, scratchTick;
        int pokeLines, ladderLines, bowlLines, toyLines, climbLines;
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
                logic = new AristocraticCatLogic(zoneMinX, zoneMaxX, zoneMinY, ladderMinX);
                catHome = cat.position;
                ladderHome = ladder.position;
                bowlHome = bowl.position;
                toyHome = toy.position;
                leanRot = ladderLean != null ? ladderLean.rotation : Quaternion.Euler(0f, 0f, -11f);
            }
            logic.Reset();
            finishing = false;
            grip = Kind.None;
            pokeLines = ladderLines = bowlLines = toyLines = climbLines = 0;
            shownSlide = shownTilt = 0f;
            annoyedUntil = -1f;
            lastSayAt = -10f;
            dragger = new HwDragger(this);
            ladderItem = dragger.Add(ladder, ladderArea, 10);
            bowlItem = dragger.Add(bowl, bowlArea, 11);
            toyItem = dragger.Add(toy, toyArea, 12);
            if (reward != null) reward.ResetReward();
#if UNITY_EDITOR
            editorFlip = editorHalf = false;
#endif
            RestItems();
            CatRest();
            HideSay();
        }

        // ---------- Pose (sempre ricavate dalla logica: uno stato valido dopo pausa/spegnimento) ----------

        /// <summary>Oggetti fermi al loro posto: la scala appoggiata se la logica lo dice, il resto a casa.</summary>
        void RestItems()
        {
            if (dragger == null) return;
            if (dragger.Held != null) dragger.Release();
            foreach (HwDragger.Item it in new[] { ladderItem, bowlItem, toyItem }) dragger.Stop(it);
            bool leaned = logic.LadderLeaned && ladderLean != null;
            ladder.position = leaned ? new Vector3(ladderLean.position.x, ladderLean.position.y, ladderHome.z) : ladderHome;
            ladder.rotation = leaned ? leanRot : Quaternion.Euler(0f, 0f, LadderHomeRot);
            ladderItem.Home = ladderHome;
            bowl.position = bowlHome;
            toy.position = toyHome;
            ladder.localScale = bowl.localScale = toy.localScale = Vector3.one;
        }

        /// <summary>Gatto di nuovo sdraiato sull'armadio.</summary>
        void CatRest()
        {
            cat.position = catHome;
            cat.localScale = Vector3.one;
            catPivot.localRotation = Quaternion.identity;
            shownSlide = shownTilt = 0f;
            ShowPose(poseLie);
            HalloweenKit.SetAlpha(scratches, 0f);
            HalloweenKit.Show(dust, false);
            HalloweenKit.Show(sparkle, false);
        }

        void ShowPose(Transform pose)
        {
            foreach (Transform t in new[] { poseLie, poseAnnoyed, poseCling, poseFall, poseSit, poseGroom })
                if (t != null) t.gameObject.SetActive(t == pose);
        }

        // ---------- Ciclo di vita ----------

        void OnDisable()
        {
            // pausa/pubblicità/livello spento: dita e tenuta annullate, oggetti fermi al loro posto
            CancelAll();
            if (captured && !finishing && dragger != null && !IsFinished)
            {
                RestItems();
                CatRest();
                HideSay();
            }
        }

        void OnEnable()
        {
            if (!captured || IsFinished || logic == null) return;
            // spento a metà della festa: si rifà il finale da capo (Win una volta sola, la logica fa da guardia)
            if (logic.State == AristocraticCatState.Fallen) { finishing = false; StartCoroutine(Finale()); }
        }

        void OnApplicationPause(bool paused) { if (paused) CancelAll(); }
        void OnApplicationFocus(bool focus) { if (!focus) CancelAll(); }

        /// <summary>Il dito sparisce (l'oggetto tenuto torna a casa) e la tenuta capovolta si annulla.</summary>
        void CancelAll()
        {
            grip = Kind.None;
            if (dragger != null && dragger.Held != null)
            {
                HwDragger.Item it = dragger.Release();
                if (it == ladderItem) ladder.rotation = Quaternion.Euler(0f, 0f, LadderHomeRot);
                if (it != null) dragger.SendHome(it, 0.2f);
            }
#if UNITY_EDITOR
            editorFlip = editorHalf = false;
#endif
            if (logic == null || logic.InputLocked) return;
            logic.Cancel();
        }

        void Update()
        {
            SayTick();
            if (logic == null || IsFinished || finishing || dragger == null) return;
            if (Time.timeScale <= 0f) { CancelAll(); return; }   // pausa o pubblicità: niente input né sensori
#if UNITY_EDITOR
            EditorKeys();
            if (finishing) return;
#endif
            Vector2 g = Gravity();
            React(logic.Tick(Time.deltaTime, g.x, g.y));
            if (finishing) return;
            ReadPointer();
            if (finishing) return;
            PoseCat(Time.deltaTime);
        }

        /// <summary>Gravità nello schermo (in g): accelerometro; nell'Editor anche i tasti U/J (e le frecce).</summary>
        Vector2 Gravity()
        {
            Vector2 g = Inputs.Tilt();
#if UNITY_EDITOR
            if (editorFlip) g = new Vector2(0f, 1f);
            else if (editorHalf) g = new Vector2(0f, 0.45f);
#endif
            return g;
        }

        // ---------- Dito ----------

        void ReadPointer()
        {
            if (!HalloweenKit.ReadPointer(cam, out Vector2 w, out bool began, out bool ended))
            {
                if (grip != Kind.None) End(gripLast);   // dito sparito o secondo dito: si lascia dov'è (come un rilascio)
                return;
            }
            if (began)
            {
                if (grip != Kind.None) End(gripLast);
                Begin(w);
            }
            if (grip == Kind.None) return;
            gripLast = w;
            if (Vector2.Distance(w, gripStart) > TapSlop) gripMoved = true;
            if (grip == Kind.Item && gripMoved) dragger.MoveHeld(w);
            if (ended) End(w);
        }

        void Begin(Vector2 w)
        {
            grip = Kind.None;
            gripStart = gripLast = w;
            gripMoved = false;
            if (logic.InputLocked) return;
            HwDragger.Item it = dragger.TryPick(w);
            if (it != null)
            {
                grip = Kind.Item;
                if (it == ladderItem)
                {
                    logic.Pick(AristocraticCatItem.Ladder);
                    ladder.rotation = Quaternion.identity;
                }
                Sfx.Play(this, "tick", 0.4f);
                return;
            }
            if (catArea != null && catArea.OverlapPoint(w))
            {
                grip = Kind.Cat;
                React(logic.Poke());   // il tocco risponde subito (anche a raffica)
            }
        }

        void End(Vector2 finger)
        {
            Kind k = grip;
            grip = Kind.None;
            if (k != Kind.Item || dragger.Held == null) return;
            HwDragger.Item it = dragger.Release();
            Dropped(it, it.T.position, finger);
        }

        /// <summary>Un oggetto è stato lasciato: la logica decide se è un'offerta al gatto (generosa: oggetto o dito).</summary>
        void Dropped(HwDragger.Item it, Vector2 at, Vector2 finger)
        {
            if (it == null) return;
            AristocraticCatItem kind = it == ladderItem ? AristocraticCatItem.Ladder : it == bowlItem ? AristocraticCatItem.Bowl : AristocraticCatItem.Toy;
            Vector2 p = at;
            if (kind == AristocraticCatItem.Ladder) { if (finger.x > p.x) p.x = finger.x; }
            else if (!logic.InCatZone(at.x, at.y) && logic.InCatZone(finger.x, finger.y)) p = finger;
            AristocraticCatEvent e = logic.Drop(kind, p.x, p.y);
            if (e == AristocraticCatEvent.LadderLeaned && ladderLean != null)
            {
                dragger.SendTo(it, new Vector3(ladderLean.position.x, ladderLean.position.y, ladderHome.z), 0.25f, 0.1f);
                ladder.rotation = leanRot;
            }
            else
            {
                if (it == ladderItem) ladder.rotation = Quaternion.Euler(0f, 0f, LadderHomeRot);
                dragger.SendHome(it, e == AristocraticCatEvent.Dropped ? 0.3f : 0.45f);
                if (e != AristocraticCatEvent.Dropped) Sfx.Play(this, "whoosh", 0.3f);
                else Sfx.Play(this, "clunk", 0.25f);
            }
            React(e);
        }

        // ---------- Reazioni ----------

        void React(AristocraticCatEvent e)
        {
            switch (e)
            {
                case AristocraticCatEvent.Uneasy:
                    Say("Hey!", 1.0f);
                    Haptics.Tap();
                    break;
                case AristocraticCatEvent.Clinging:
                    Say("Hey! HEY!", 1.4f);
                    Sfx.Play(this, "whoosh", 0.4f);
                    Haptics.Light();
                    scratchTick = 0f;
                    break;
                case AristocraticCatEvent.ClimbedBack:
                    Say(ClimbLines[climbLines++ % ClimbLines.Length], 1.6f);
                    Sfx.Play(this, "tick", 0.3f);
                    break;
                case AristocraticCatEvent.Slid:
                    StartCoroutine(Finale());
                    break;
                case AristocraticCatEvent.Poked:
                    Annoyed(1.2f);
                    Haptics.Tap();
                    Say(HintOr(PokeLines[pokeLines++ % PokeLines.Length]), 1.5f);
                    break;
                case AristocraticCatEvent.LadderLeaned:
                    Annoyed(1.4f);
                    Sfx.Play(this, "clunk", 0.5f);
                    Say(HintOr(LadderLines[ladderLines++ % LadderLines.Length]), 2.0f);
                    break;
                case AristocraticCatEvent.BowlRefused:
                    Annoyed(1.0f);
                    Say(HintOr(BowlLines[bowlLines++ % BowlLines.Length]), 1.8f);
                    break;
                case AristocraticCatEvent.ToyRefused:
                    Annoyed(1.4f);   // occhiata annoiata
                    Say(HintOr(ToyLines[toyLines++ % ToyLines.Length]), 1.5f);
                    break;
            }
        }

        /// <summary>Dopo qualche tentativo sbagliato, una volta sola, la frase sottile al posto della battuta.</summary>
        string HintOr(string line) => logic.TakeHint() ? HintLine : line;

        void Annoyed(float seconds) { annoyedUntil = Time.time + seconds; }

        // ---------- Fumetto del gatto (si adatta al testo e resta nello schermo) ----------

        void Say(string text, float seconds)
        {
            if (catBubble == null) return;
            Vector2 off = logic.State == AristocraticCatState.Fallen ? bubbleFloor : bubblePerched;
            if (bubbleHolder != null) bubbleHolder.position = new Vector3(cat.position.x + off.x, cat.position.y + off.y, bubbleHolder.position.z);
            bool quiet = Time.time - lastSayAt < SayGap;   // a raffica: niente miagolio a ogni tocco
            lastSayAt = Time.time;
            catBubble.Show(text, quiet);
            sayHideAt = Time.time + seconds;
        }

        void HideSay()
        {
            sayHideAt = -1f;
            if (catBubble != null) catBubble.Hide();
        }

        void SayTick()
        {
            if (sayHideAt > 0f && Time.time >= sayHideAt) HideSay();
        }

        // ---------- Il gatto che sente l'inclinazione ----------

        void PoseCat(float dt)
        {
            float t = Time.time;
            bool clinging = logic.State == AristocraticCatState.Clinging;
            float lean = logic.Lean, hold = logic.HoldProgress;
            // scivola un po' verso il bordo (aggrappato: di più col tempo); risale piano quando si raddrizza
            float target = clinging ? 0.12f + 0.3f * hold : lean * 0.06f;
            shownSlide = Mathf.MoveTowards(shownSlide, target, dt * (target > shownSlide ? 2.5f : 0.6f));
            float tiltTarget = clinging ? 6f + 8f * hold : lean * 5f;
            shownTilt = Mathf.MoveTowards(shownTilt, tiltTarget, dt * 60f);
            cat.position = catHome + new Vector3(-shownSlide, 0f, 0f);
            float shiver = clinging ? Mathf.Sin(t * 45f) * 1.5f : lean > AristocraticCatLogic.UneasyAt ? Mathf.Sin(t * 30f) * 0.8f : 0f;
            catPivot.localRotation = Quaternion.Euler(0f, 0f, shownTilt + shiver);
            bool scared = clinging || lean >= AristocraticCatLogic.UneasyAt || shownSlide > 0.05f;
            ShowPose(scared ? poseCling : t < annoyedUntil ? poseAnnoyed : poseLie);
            if (scratches != null)
            {
                float a = scratches.color.a;
                float to = clinging ? 0.5f + 0.5f * hold : 0f;
                HalloweenKit.SetAlpha(scratches, Mathf.MoveTowards(a, to, dt * (to > a ? 4f : 1.5f)));
            }
            if (clinging)
            {
                // unghie che graffiano il legno
                scratchTick -= dt;
                if (scratchTick <= 0f)
                {
                    scratchTick = 0.14f;
                    Sfx.Play(this, "tick", 0.35f);
                    Haptics.Tap();
                }
            }
        }

        // ---------- Vittoria ----------

        IEnumerator Finale()
        {
            if (finishing) yield break;
            finishing = true;
            grip = Kind.None;
            if (dragger != null && dragger.Held != null)
            {
                HwDragger.Item it = dragger.Release();
                if (it == ladderItem) ladder.rotation = logic.LadderLeaned ? leanRot : Quaternion.Euler(0f, 0f, LadderHomeRot);
                if (it != null) dragger.SendHome(it, 0.2f);
            }
            HideSay();
            HalloweenKit.Show(dust, false);
            HalloweenKit.Show(sparkle, false);
            cat.localScale = Vector3.one;
            ShowPose(poseCling);
            // 1) scivola lungo il piano fino al bordo, unghie che graffiano
            Vector3 from = cat.position;
            Vector3 edge = new Vector3(edgeX, catHome.y, catHome.z);
            float tilt0 = shownTilt;
            Sfx.Play(this, "whoosh", 0.6f);
            float t = 0f;
            while (t < 0.35f)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / 0.35f);
                cat.position = Vector3.Lerp(from, edge, k * k);
                catPivot.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(tilt0, 22f, k) + Mathf.Sin(t * 50f) * 2f);
                if (scratches != null) HalloweenKit.SetAlpha(scratches, 1f);
                yield return null;
            }
            // 2) cade: giravolta da gatto, arco verso il pavimento
            ShowPose(poseFall);
            Haptics.Light();
            Vector3 land = new Vector3(landAt.x, landAt.y, catHome.z);
            t = 0f;
            while (t < 0.5f)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / 0.5f);
                Vector3 p = Vector3.Lerp(edge, land, k);
                p.y = Mathf.Lerp(edge.y, land.y, k * k) + Mathf.Sin(k * Mathf.PI) * 0.35f;
                cat.position = p;
                catPivot.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(22f, 360f, k));
                yield return null;
            }
            // 3) atterra e rimbalza
            cat.position = land;
            catPivot.localRotation = Quaternion.identity;
            ShowPose(poseSit);
            Sfx.Play(this, "bonk", 0.8f);
            Haptics.Medium();
            if (dust != null)
            {
                dust.gameObject.SetActive(true);
                dust.transform.position = new Vector3(land.x, land.y + 0.1f, dust.transform.position.z);
                StartCoroutine(Puff(dust));
            }
            t = 0f;
            while (t < 0.35f)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / 0.35f);
                float squash = k < 0.3f ? Mathf.Sin(k / 0.3f * Mathf.PI) : 0f;
                cat.localScale = new Vector3(1f + 0.25f * squash, 1f - 0.25f * squash, 1f);
                float hop = k >= 0.3f ? Mathf.Sin((k - 0.3f) / 0.7f * Mathf.PI) * 0.3f : 0f;
                cat.position = land + new Vector3(0f, hop, 0f);
                yield return null;
            }
            cat.position = land;
            cat.localScale = Vector3.one;
            // 4) si ricompone e si liscia il pelo: "I MEANT TO DO THAT!"
            Say(Compliment, 3.5f);
            HalloweenKit.Show(sparkle, true);
            if (sparkle != null) sparkle.transform.position = new Vector3(land.x + 0.45f, land.y + 1.0f, sparkle.transform.position.z);
            t = 0f;
            while (t < 0.75f)
            {
                t += Time.deltaTime;
                ShowPose(Mathf.FloorToInt(t / 0.18f) % 2 == 0 ? poseGroom : poseSit);
                if (sparkle != null)
                {
                    sparkle.transform.localScale = Vector3.one * (0.6f + 0.5f * Mathf.Abs(Mathf.Sin(t * 9f)));
                    sparkle.transform.localRotation = Quaternion.Euler(0f, 0f, t * 180f);
                }
                yield return null;
            }
            ShowPose(poseSit);
            HalloweenKit.Show(sparkle, false);
            Sfx.Play(this, "ding", 0.6f);
            if (reward != null) yield return reward.Play(Compliment);
            if (!IsFinished && logic.TryComplete()) Win(Compliment);
        }

        IEnumerator Puff(SpriteRenderer p)
        {
            float t = 0f;
            while (t < 0.4f && p != null)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / 0.4f);
                p.transform.localScale = new Vector3(Mathf.Lerp(1f, 2.4f, k), Mathf.Lerp(0.6f, 1.1f, k), 1f);
                HalloweenKit.SetAlpha(p, 0.85f * (1f - k));
                yield return null;
            }
            if (p != null) p.gameObject.SetActive(false);
        }

#if UNITY_EDITOR
        // ---------- Tasti di prova nell'Editor ----------

        void EditorKeys()
        {
            if (logic.InputLocked) return;
            if (Input.GetKeyDown(KeyCode.U)) { editorFlip = !editorFlip; editorHalf = false; }
            if (Input.GetKeyDown(KeyCode.J)) { editorHalf = !editorHalf; editorFlip = false; }
            if (Input.GetKeyDown(KeyCode.T)) React(logic.Poke());
            if (Input.GetKeyDown(KeyCode.L) && dragger.Held == null && ladderLean != null)
            {
                logic.Pick(AristocraticCatItem.Ladder);
                Dropped(ladderItem, ladderLean.position, ladderLean.position);
            }
            if (Input.GetKeyDown(KeyCode.K) && dragger.Held == null)
            {
                logic.Pick(AristocraticCatItem.Ladder);
                Dropped(ladderItem, ladderHome, ladderHome);
            }
            Vector2 offer = new Vector2(catHome.x - 0.6f, catHome.y + 0.3f);
            if (Input.GetKeyDown(KeyCode.F) && dragger.Held == null) { bowl.position = new Vector3(offer.x, offer.y, bowlHome.z); Dropped(bowlItem, offer, offer); }
            if (Input.GetKeyDown(KeyCode.Y) && dragger.Held == null) { toy.position = new Vector3(offer.x, offer.y, toyHome.z); Dropped(toyItem, offer, offer); }
            if (Input.GetKeyDown(KeyCode.P)) CancelAll();
            if (Input.GetKeyDown(KeyCode.R)) OnLevelStart();
        }
#endif
    }
}
