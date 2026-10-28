using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels.V64;
using Gotcha.Sensors;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 256 "The Incompetent Magician" — il mago (quello di "The cheap magician") vuole far sparire il coniglio
    /// ma rovina ogni trucco: il coniglio nel cilindro ne esce col cilindro in testa, nelle scatole ricompare fuori,
    /// la bacchetta fa solo coriandoli, le formule al microfono non fanno niente, toccato si inchina e non aiuta.
    /// Soluzione: un dito sugli occhi del mago (li chiude), un piccolo suono al microfono: il coniglio sente e indica
    /// la botola nascosta. Tolte le dita il mago si stropiccia gli occhi per qualche secondo: aprire la botola e
    /// trascinarci il coniglio. Il mago riapre gli occhi, palco vuoto, si inchina fiero; il coniglio sbuca dal sipario
    /// col cartello "WHAT AN IDIOT". Senza microfono: tenere coperti gli occhi ancora un po' (compare un distintivo).
    /// Logica in Gotcha.Levels.V64.IncompetentMagicianLogic (senza Unity): qui solo input e presentazione.
    /// </summary>
    public class V64IncompetentMagicianLevel : V62Level
    {
        [SerializeField] Transform magician;
        [SerializeField] Collider2D faceArea;            // generosa: metà sinistra = occhio sinistro, metà destra = destro
        [SerializeField] Collider2D magicianArea;
        [SerializeField] SpriteRenderer eyesClosed;
        [SerializeField] Transform glove;
        [SerializeField] Transform wand;
        [SerializeField] Collider2D wandArea;
        [SerializeField] SmokeEmitter wandConfetti;
        [SerializeField] Transform rabbit;
        [SerializeField] Collider2D rabbitArea;
        [SerializeField] Transform rabbitBrows;
        [SerializeField] Transform rabbitPaw;
        [SerializeField] Transform hat;
        [SerializeField] Transform hatBack;
        [SerializeField] Collider2D hatArea;
        [SerializeField] Transform[] boxes;
        [SerializeField] Collider2D[] boxAreas;
        [SerializeField] SpriteRenderer puff;
        [SerializeField] Transform trapdoorLid;
        [SerializeField] Collider2D trapdoorArea;
        [SerializeField] SpriteRenderer trapHole;
        [SerializeField] SpriteRenderer trapGlow;
        [SerializeField] Transform audience;
        [SerializeField] Transform signRabbit;           // il coniglio col cartello dietro il sipario (nascosto)
        [SerializeField] Transform micBadge;             // alternativa senza microfono (nascosta)
        [SerializeField] Transform micGauge;             // barra che si riempie (scala x)
        [SerializeField] SpeechBubble magicianBubble;
        [SerializeField] SpeechBubble rabbitBubble;
        [SerializeField] Vector2 floorMin = new Vector2(-2.2f, -3.6f), floorMax = new Vector2(2.2f, -1.7f);
        [SerializeField] TextMesh infoText;
        [SerializeField] Transform infoHolder;

        const float TrapRadius = 0.65f, HatRadius = 0.6f, LidOpenDrag = 0.4f, LidOpenX = 1.05f;
        static readonly string[] BowLines = { "Thank you, thank you! No autographs.", "A magician never helps. I mean... reveals.", "*bows deeper* You're welcome." };
        static readonly string[] SpellLines = { "ABRACADABRA!", "HOCUS POCUS!", "ALAKAZAM!" };

        readonly IncompetentMagicianLogic logic = new IncompetentMagicianLogic();
        readonly MagicianMicGate gate = new MagicianMicGate();
        Camera cam;
        HwDragger drag;
        HwDragger.Item rabbitItem;
        HwSay magSay, rabSay;
        HwToast toast;
        Vector3 lidHome, hatHome, hatScale, magScale, pawScale;
        Vector2 lidGrab;
        bool busy, draggingLid, micReleased, almostSaid;
        float glowT;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            logic.Reset();
            gate.Reset();
            ResetV62();
            magSay = new HwSay(magicianBubble);
            rabSay = new HwSay(rabbitBubble);
            toast = new HwToast(infoText, infoHolder);
            drag = new HwDragger(this);
            rabbitItem = drag.Add(rabbit, rabbitArea, 5);
            lidHome = trapdoorLid.localPosition;
            hatHome = hat.position;
            hatScale = hat.localScale;
            magScale = magician.localScale;
            if (rabbitPaw != null) pawScale = rabbitPaw.localScale;
            HalloweenKit.Show(eyesClosed, false);
            HalloweenKit.Show(glove, false);
            HalloweenKit.Show(rabbitPaw, false);
            HalloweenKit.Show(trapHole, false);
            HalloweenKit.Show(trapGlow, false);
            HalloweenKit.Show(signRabbit, false);
            HalloweenKit.Show(micBadge, false);
            HalloweenKit.Show(puff, false);
            if (wandConfetti != null) wandConfetti.Stop();
            micReleased = false;
            magSay.Show("Behold! I shall make this rabbit... DISAPPEAR!", 2.6f);
        }

        void OnDisable() { Inputs.ReleaseMic(this); }
        void OnDestroy() { Inputs.ReleaseMic(this); }

        void Update()
        {
            magSay.Tick();
            rabSay.Tick();
            toast.Tick();
            if (IsFinished || logic.InputLocked) return;
            if (Time.timeScale <= 0f) return;   // pausa o pubblicità: niente input né microfono
            if (busy) { if (drag.Held != null) drag.Cancel(); return; }
            ReadEyes();
            if (busy) return;
            ReadMic();
            if (busy) return;
            ReadPointer();
            Present();
        }

        // ---------- Occhi (un dito) ----------

        void ReadEyes()
        {
            // basta un dito sulla faccia (zona generosa) per coprire gli occhi: con due dita il riconoscimento era
            // incerto su alcuni telefoni. Il dito deve restare giù fino al sussurro.
            bool covered = false;
            foreach (S41KKit.Finger f in S41KKit.Fingers(cam))
                if (!f.ended && faceArea.OverlapPoint(f.world)) { covered = true; break; }
            if (covered && drag.Held != null) drag.Cancel();
            MagicianReaction r = logic.SetEyesCovered(covered);
            switch (r)
            {
                case MagicianReaction.EyesClosed:
                    HalloweenKit.Show(eyesClosed, true);
                    HalloweenKit.Show(glove, false);
                    Sfx.Play(this, "pop", 0.4f);
                    Haptics.Tap();
                    magSay.Show(logic.TrapdoorFound ? "Not again! Who turned off the lights?" : "Hey! Who turned off the lights?!", 2f);
                    break;
                case MagicianReaction.EyesOpen:
                    OpenEyes();
                    magSay.Show("Aha! I can see again. Where was I?", 1.6f);
                    break;
                case MagicianReaction.RubsEyes:
                    almostSaid = false;
                    HalloweenKit.Show(glove, true);
                    magSay.Show("Ow, my eyes... give me a sec...", 2.2f);
                    break;
            }
        }

        void OpenEyes()
        {
            HalloweenKit.Show(eyesClosed, false);
            HalloweenKit.Show(glove, false);
        }

        // ---------- Microfono ----------

        void ReadMic()
        {
            bool sound = false;
            bool micOk = SensorHub.Instance != null && SensorHub.Instance.MicActive;
            if (logic.NeedsMic)
            {
                sound = gate.Feed(Inputs.Mic(this), Time.deltaTime);
#if UNITY_EDITOR
                micOk |= Input.GetKey(KeyCode.B) || Input.GetKey(KeyCode.N);
#endif
            }
            else if (!micReleased)
            {
                micReleased = true;
                Inputs.ReleaseMic(this);   // la botola è scoperta: il microfono non serve più
            }
            MagicianReaction r = logic.Tick(Time.deltaTime, sound, micOk);
            bool showBadge = logic.State == MagicianState.Blindfolded && logic.FallbackReady(micOk);
            if (micBadge != null && micBadge.gameObject.activeSelf != showBadge) HalloweenKit.Show(micBadge, showBadge);
            if (showBadge && micGauge != null)
                micGauge.localScale = new Vector3(Mathf.Clamp01(logic.FallbackProgress / IncompetentMagicianLogic.FallbackHold), 1f, 1f);
            switch (r)
            {
                case MagicianReaction.RabbitPoints:
                    HalloweenKit.Show(micBadge, false);
                    StartCoroutine(RabbitPoints());
                    break;
                case MagicianReaction.FailedSpell:
                    StartCoroutine(FailedSpell());
                    break;
                case MagicianReaction.EyesOpen:
                    OpenEyes();
                    magSay.Show("Right! Where's that rabbit?", 1.6f);
                    break;
                case MagicianReaction.MagicianSlamsShut:
                    OpenEyes();
                    StartCoroutine(SlamShut());
                    break;
            }
            if (logic.State == MagicianState.RubbingEyes && logic.GraceLeft < 2f && !almostSaid)
            {
                almostSaid = true;
                magSay.Show("Okay... almost... seeing shapes...", 1.6f);
            }
        }

        IEnumerator RabbitPoints()
        {
            busy = true;
            if (drag.Held != null) drag.Cancel();
            Sfx.Play(this, "tick", 0.6f);
            HalloweenKit.Show(rabbitBrows, false);
            rabSay.Show("Psst! Down there.", 2.4f);
            yield return HalloweenKit.Pop(rabbit, 1.12f, 0.25f);
            // il braccino indica la botola, che si illumina
            if (rabbitPaw != null)
            {
                HalloweenKit.Show(rabbitPaw, true);
                Vector2 d = (Vector2)(trapdoorLid.position - rabbitPaw.position);
                rabbitPaw.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg);
                rabbitPaw.localScale = pawScale;
            }
            HalloweenKit.Show(trapGlow, true);
            Sfx.Play(this, "ding", 0.4f);
            busy = false;
            yield return new WaitForSeconds(2.2f);
            HalloweenKit.Show(rabbitPaw, false);
            HalloweenKit.Show(rabbitBrows, true);
        }

        IEnumerator FailedSpell()
        {
            magSay.Show(SpellLines[(logic.Spells - 1) % SpellLines.Length], 1.4f);
            yield return Wave();
            Sfx.Play(this, "pop", 0.5f);
            yield return new WaitForSeconds(0.5f);
            if (!logic.InputLocked && logic.Watching) magSay.Show("...hm. Nothing. Must be the acoustics.", 1.6f);
            rabSay.Show("*sigh*", 1.2f);
        }

        IEnumerator Wave()
        {
            if (wand == null) yield break;
            Quaternion home = wand.localRotation;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.5f)
            {
                wand.localRotation = home * Quaternion.Euler(0f, 0f, Mathf.Sin(k * Mathf.PI * 3f) * 25f);
                yield return null;
            }
            wand.localRotation = home;
        }

        IEnumerator SlamShut()
        {
            busy = true;
            magSay.Show("Who opened that?! Someone could fall in!", 2f);
            yield return CloseLid();
            Sfx.Play(this, "bonk", 0.7f);
            Haptics.Light();
            StartCoroutine(HalloweenKit.Shake(magician, 0.05f, 0.25f));
            busy = false;
        }

        IEnumerator CloseLid()
        {
            HalloweenKit.Show(trapHole, false);
            Vector3 from = trapdoorLid.localPosition;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.2f)
            {
                trapdoorLid.localPosition = Vector3.Lerp(from, lidHome, k);
                yield return null;
            }
            trapdoorLid.localPosition = lidHome;
            Sfx.Play(this, "clunk", 0.6f);
        }

        // ---------- Un dito: coniglio, botola, tocchi ----------

        void ReadPointer()
        {
            if (logic.EyesClosed && logic.State == MagicianState.Blindfolded) { if (drag.Held != null) drag.Cancel(); draggingLid = false; return; }
            if (!HalloweenKit.ReadPointer(cam, out Vector2 w, out bool began, out bool ended))
            {
                if (drag.Held != null) DropRabbit(true);
                if (draggingLid) { draggingLid = false; LidBack(); }
                return;
            }
            if (began)
            {
                if (faceArea.OverlapPoint(w)) return;   // le dita sugli occhi le gestisce ReadEyes
                if (drag.TryPick(w) == rabbitItem) { Sfx.Play(this, "pop", 0.3f); return; }
                if (trapdoorArea.OverlapPoint(w)) { BeginLid(w); return; }
                Tap(w);
                return;
            }
            if (drag.Held != null)
            {
                drag.MoveHeld(w);
                if (ended) DropRabbit(false);
                return;
            }
            if (draggingLid)
            {
                float dx = Mathf.Clamp(w.x - lidGrab.x, -0.2f, LidOpenX);
                trapdoorLid.localPosition = lidHome + new Vector3(dx, 0f, 0f);
                if (Vector2.Distance(w, lidGrab) >= LidOpenDrag) { draggingLid = false; OpenLid(); }
                else if (ended) { draggingLid = false; LidBack(); }
            }
        }

        void BeginLid(Vector2 w)
        {
            if (logic.TrapdoorOpen) return;
            if (!logic.TrapdoorFound)
            {
                // botola ancora nascosta: suona vuoto (indizio), nient'altro
                Sfx.Play(this, "clunk", 0.35f);
                StartCoroutine(HalloweenKit.Shake(trapdoorLid, 0.015f, 0.15f));
                if (logic.TryOpenTrapdoor() == MagicianReaction.HollowFloor) toast.Show("*tok tok* ...hollow?", 1.2f);
                return;
            }
            if (logic.Watching)
            {
                logic.TryOpenTrapdoor();
                StartCoroutine(HalloweenKit.Shake(trapdoorLid, 0.03f, 0.25f));
                Sfx.Play(this, "bonk", 0.5f);
                magSay.Show("Hey! Hands off my stage floor!", 1.8f);
                return;
            }
            draggingLid = true;
            lidGrab = w;
            Haptics.Tap();
        }

        void OpenLid()
        {
            MagicianReaction r = logic.TryOpenTrapdoor();
            if (r != MagicianReaction.TrapdoorOpens) { LidBack(); return; }
            trapdoorLid.localPosition = lidHome + new Vector3(LidOpenX, 0f, 0f);
            HalloweenKit.Show(trapHole, true);
            Sfx.Play(this, "clunk", 0.7f);
            Haptics.Light();
        }

        void LidBack() { trapdoorLid.localPosition = lidHome; }

        void DropRabbit(bool lostFinger)
        {
            drag.Release();
            Vector2 p = rabbit.position;
            if (!lostFinger && logic.TrapdoorOpen && Vector2.Distance(p, trapdoorLid.parent.position) < TrapRadius)
            {
                MagicianReaction r = logic.DropRabbitInTrapdoor();
                if (r == MagicianReaction.RabbitVanishes) { StartCoroutine(Vanish()); return; }
                if (r == MagicianReaction.MagicianCatchesRabbit)
                {
                    magSay.Show("Not yet! I haven't said the magic words!", 1.8f);
                    StartCoroutine(CloseLid());
                    drag.SendHome(rabbitItem, 0.35f);
                    return;
                }
            }
            if (!lostFinger && Vector2.Distance(p, hat.position + Vector3.up * 0.4f) < HatRadius)
            {
                if (logic.RabbitIntoHat() == MagicianReaction.RabbitWearsHat) { StartCoroutine(WearsHat()); return; }
            }
            for (int i = 0; boxAreas != null && i < boxAreas.Length; i++)
                if (!lostFinger && boxAreas[i] != null && boxAreas[i].OverlapPoint(p))
                {
                    if (logic.RabbitIntoBox() == MagicianReaction.RabbitReappears) { StartCoroutine(OutOfBox(i)); return; }
                }
            // altrove: resta sul palco
            Vector2 q = new Vector2(Mathf.Clamp(p.x, floorMin.x, floorMax.x), Mathf.Clamp(p.y, floorMin.y, floorMax.y));
            drag.SendTo(rabbitItem, new Vector3(q.x, q.y, rabbit.position.z), 0.15f, 0.05f);
            if (!lostFinger && logic.TrapdoorFound && !logic.TrapdoorOpen && Vector2.Distance(p, trapdoorLid.parent.position) < TrapRadius)
                rabSay.Show("*taps foot* Open it first.", 1.4f);
        }

        void Tap(Vector2 w)
        {
            if (wandArea != null && wandArea.OverlapPoint(w))
            {
                if (logic.TapWand() == MagicianReaction.Confetti) StartCoroutine(Confetti());
                return;
            }
            if (magicianArea != null && magicianArea.OverlapPoint(w))
            {
                if (logic.TapMagician() == MagicianReaction.Bows) StartCoroutine(Bow(1));
                else if (logic.State == MagicianState.RubbingEyes) magSay.Show("Who's there? I'm busy rubbing!", 1.4f);
                return;
            }
            if (hatArea != null && hatArea.OverlapPoint(w))
            {
                Sfx.Play(this, "tick", 0.5f);
                StartCoroutine(HalloweenKit.Wiggle(hat, 6f, 0.4f, 25f));
                return;
            }
            for (int i = 0; boxAreas != null && i < boxAreas.Length; i++)
                if (boxAreas[i] != null && boxAreas[i].OverlapPoint(w))
                {
                    Sfx.Play(this, "bonk", 0.4f);
                    StartCoroutine(HalloweenKit.Shake(boxes[i], 0.04f, 0.25f));
                    return;
                }
        }

        // ---------- Reazioni sbagliate ----------

        IEnumerator Confetti()
        {
            busy = true;
            Sfx.Play(this, "pop", 0.8f);
            StartCoroutine(Wave());
            yield return HalloweenKit.Burst(wandConfetti, 0.5f);
            magSay.Show("BRAVO! Bravo, me!", 1.6f);
            for (int i = 0; i < 3; i++)
            {
                Sfx.Play(this, "tick", 0.8f);
                yield return HalloweenKit.Pop(magician, 1.04f, 0.15f);
            }
            busy = false;
        }

        IEnumerator Bow(int times)
        {
            busy = true;
            magSay.Show(BowLines[(logic.Bows - 1) % BowLines.Length], 1.8f);
            for (int i = 0; i < times; i++) yield return BowOnce();
            busy = false;
        }

        IEnumerator BowOnce()
        {
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.55f)
            {
                magician.localRotation = Quaternion.Euler(0f, 0f, -Mathf.Sin(k * Mathf.PI) * 22f);
                yield return null;
            }
            magician.localRotation = Quaternion.identity;
        }

        IEnumerator WearsHat()
        {
            busy = true;
            Vector3 head = rabbit.position;
            yield return HalloweenKit.Move(rabbit, hat.position + Vector3.up * 0.45f, 0.12f);
            Sfx.Play(this, "pop", 0.7f);
            // salta fuori col cilindro in testa
            HalloweenKit.Show(hatBack, false);
            Vector3 land = new Vector3(Mathf.Clamp(hat.position.x + 0.95f, floorMin.x, floorMax.x), Mathf.Clamp(hat.position.y - 0.1f, floorMin.y, floorMax.y), rabbit.position.z);
            hat.localScale = hatScale * 0.5f;
            Vector3 from = rabbit.position;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.4f)
            {
                rabbit.position = Vector3.Lerp(from, land, k) + Vector3.up * Mathf.Sin(k * Mathf.PI) * 0.6f;
                hat.position = rabbit.position + new Vector3(0f, 0.55f, 0f);
                yield return null;
            }
            rabbit.position = land;
            hat.position = rabbit.position + new Vector3(0f, 0.55f, 0f);
            rabSay.Show(logic.HatTries > 1 ? "Still not funny." : "Nice try. Love the hat, though.", 1.6f);
            magSay.Show("Ta-da...? He's... wearing it.", 1.6f);
            yield return new WaitForSeconds(1.1f);
            // e lo rilancia al suo posto
            Sfx.Play(this, "whoosh", 0.5f);
            Vector3 hf = hat.position;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.35f)
            {
                hat.position = Vector3.Lerp(hf, hatHome, k) + Vector3.up * Mathf.Sin(k * Mathf.PI) * 0.5f;
                hat.localScale = Vector3.Lerp(hatScale * 0.5f, hatScale, k);
                yield return null;
            }
            hat.position = hatHome;
            hat.localScale = hatScale;
            HalloweenKit.Show(hatBack, true);
            Sfx.Play(this, "clunk", 0.5f);
            rabbitItem.Home = rabbit.position;
            busy = false;
        }

        IEnumerator OutOfBox(int i)
        {
            busy = true;
            Transform box = boxes[i];
            yield return HalloweenKit.Move(rabbit, box.position, 0.12f);
            HalloweenKit.Show(rabbit, false);
            Sfx.Play(this, "pop", 0.6f);
            StartCoroutine(HalloweenKit.Shake(box, 0.05f, 0.4f));
            magSay.Show("And now... he's GONE!", 1.4f);
            yield return new WaitForSeconds(0.6f);
            // ...ricompare fuori dalla stessa scatola, davanti
            Vector3 outside = new Vector3(Mathf.Clamp(box.position.x, floorMin.x, floorMax.x), floorMax.y - 0.15f, rabbit.position.z);
            rabbit.position = outside;
            HalloweenKit.Show(rabbit, true);
            if (puff != null) { HalloweenKit.Show(puff, true); puff.transform.position = outside + Vector3.up * 0.2f; StartCoroutine(PuffFx()); }
            Sfx.Play(this, "pop", 0.8f);
            yield return HalloweenKit.Pop(rabbit, 1.15f, 0.25f);
            rabSay.Show(logic.BoxTries > 1 ? "Still here." : "I'm right here. Outside. Of the box.", 1.6f);
            magSay.Show("...Ta-da? Oh.", 1.2f);
            rabbitItem.Home = rabbit.position;
            busy = false;
        }

        IEnumerator PuffFx()
        {
            Transform t = puff.transform;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.5f)
            {
                t.localScale = Vector3.one * Mathf.Lerp(0.6f, 1.5f, k);
                HalloweenKit.SetAlpha(puff, 1f - k);
                yield return null;
            }
            HalloweenKit.Show(puff, false);
        }

        // ---------- Presentazione continua ----------

        void Present()
        {
            if (trapGlow != null && trapGlow.gameObject.activeSelf)
            {
                glowT += Time.deltaTime;
                Color c = new Color(1f, 0.9f, 0.35f, logic.TrapdoorOpen ? 0.35f : 0.45f + 0.35f * Mathf.Sin(glowT * 4f));
                trapGlow.color = c;
            }
            if (glove != null && glove.gameObject.activeSelf)
                glove.localPosition = new Vector3(Mathf.Sin(Time.time * 14f) * 0.03f, glove.localPosition.y, glove.localPosition.z);
        }

        // ---------- Finale ----------

        IEnumerator Vanish()
        {
            busy = true;
            rabbitItem.Enabled = false;
            Vector3 hole = trapdoorLid.parent.position;
            yield return HalloweenKit.Move(rabbit, new Vector3(hole.x, hole.y + 0.25f, rabbit.position.z), 0.12f);
            // giù nella botola
            Sfx.Play(this, "whoosh", 0.8f);
            Vector3 s = rabbit.localScale, p = rabbit.position;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.35f)
            {
                rabbit.localScale = new Vector3(s.x * (1f - 0.3f * k), s.y * (1f - k), s.z);
                rabbit.position = p + Vector3.down * 0.35f * k;
                yield return null;
            }
            HalloweenKit.Show(rabbit, false);
            HalloweenKit.Show(trapGlow, false);
            yield return CloseLid();
            yield return new WaitForSeconds(0.25f);
            // il mago apre gli occhi: palco vuoto
            OpenEyes();
            magSay.Show("...?!", 0.8f);
            for (int i = 0; i < 2; i++)
            {
                magician.localScale = new Vector3(-magScale.x, magScale.y, magScale.z);
                yield return new WaitForSeconds(0.18f);
                magician.localScale = magScale;
                yield return new WaitForSeconds(0.18f);
            }
            // si inchina, convinto di essere un genio; applausi
            magSay.Show("TA-DAAAA! I'm a GENIUS!", 2.6f);
            StartCoroutine(Applause(2.6f));
            StartCoroutine(Celebrate(2.6f));
            StartCoroutine(SignRabbit(0.9f));
            for (int i = 0; i < 4; i++) yield return BowOnce();
            if (logic.TryComplete()) Win();
        }

        IEnumerator Applause(float seconds)
        {
            Vector3 home = audience != null ? audience.localPosition : Vector3.zero;
            for (float t = 0f; t < seconds; t += 0.09f)
            {
                Sfx.Play(this, "tick", Random.Range(0.4f, 0.9f));
                if (audience != null) audience.localPosition = home + Vector3.up * Mathf.Abs(Mathf.Sin(t * 9f)) * 0.06f;
                yield return new WaitForSeconds(0.09f);
            }
            if (audience != null) audience.localPosition = home;
        }

        IEnumerator SignRabbit(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (signRabbit == null) yield break;
            HalloweenKit.Show(signRabbit, true);
            Vector3 to = signRabbit.position + Vector3.left * 0.95f;
            Sfx.Play(this, "pop", 0.5f);
            yield return HalloweenKit.Move(signRabbit, to, 0.35f, 0.08f);
            yield return HalloweenKit.Wiggle(signRabbit, 4f, 0.8f, 10f);
        }
    }
}
