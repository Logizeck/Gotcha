using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels.V64;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 42 della roadmap "The Polite Bomb" (id polite_bomb) — "STOP THE COUNTDOWN." Su un banco da officina una
    /// bomba-cartone educatissima (papillon, faccina gentile) ha un display che conta da 00:10; sotto, montato sul fronte
    /// del banco, un grosso pulsante STOP; dal display escono tre fili colorati che penzolano a sinistra; sul fianco destro
    /// c'è un vano metallico con una batteria che sporge un po'.
    /// Inganno: STOP fa ACCELERARE il timer ("STOP? Sure! Faster!", fino a 4x); i fili fanno apparire messaggi d'errore
    /// sul display ("WRONG WIRE.", "ALSO WRONG WIRE.", "ERR: TOO MANY WIRES") e una scusa nel fumetto; toccare la bomba la
    /// fa protestare con garbo. Il timer è una GAG: a 00:00 fa uno sbuffo innocuo ("pfff... boom?"), la bomba arrossisce
    /// imbarazzata e il conto riparte da 00:10 a velocità normale. Mai sconfitte, mai vite perse (niente Fail).
    /// Soluzione: trascinare la batteria fuori dal vano (abbastanza lontano, in qualunque direzione tranne dentro la
    /// bomba: PoliteBombLogic.RemoveDistance). Mentre esce la bomba si preoccupa ("W-wait! I need that!"); lasciata prima
    /// torna nel vano con un clic. Staccata: la batteria vola sul banco e resta lì (input bloccato, non può tornare), il
    /// display sfarfalla e si spegne, la miccia si spegne con uno sbuffetto, la bomba si addormenta ("zzz") e si affloscia;
    /// compare la coppa e la festa comune LevelReward "WORLD SAVED. ABSOLUTE GENIUS!" -> Win una volta sola.
    /// Dita: V64AFingers (al massimo due; un dito solo tiene la batteria, l'altro può premere STOP o i fili). Pausa, app in
    /// background o livello spento rimettono la batteria nel vano (Cancel) senza contare niente. Logica in
    /// Gotcha.Levels.V64.PoliteBombLogic.
    /// Tasti di prova nell'Editor: B soluzione (batteria tirata fuori), N batteria tirata poco e lasciata (torna),
    /// T tocco sulla batteria, S STOP, W un filo (a turno), K colpetto alla bomba, Z salta a 00:00 (sbuffo),
    /// P pausa simulata (dita perse), R ricomincia.
    /// </summary>
    public class V82PoliteBombLevel : LevelController
    {
        [Header("La bomba")]
        [SerializeField] Transform look;            // figlio per gli effetti (pulsazione, tremolio, afflosciata)
        [SerializeField] Collider2D bodyArea;       // colpetti alla bomba
        [SerializeField] Transform[] faces;         // 0 educata, 1 servizievole, 2 preoccupata, 3 imbarazzata, 4 addormentata
        [SerializeField] TextMesh display;
        [SerializeField] SpriteRenderer displayGlow;
        [SerializeField] Transform spark;
        [Header("I fili (0 rosso, 1 blu, 2 giallo)")]
        [SerializeField] Transform[] wires;
        [SerializeField] Collider2D[] wireAreas;
        [Header("La batteria (la soluzione)")]
        [SerializeField] Transform battery;
        [SerializeField] Collider2D batteryArea;
        [SerializeField] Transform batteryRest;     // dove si posa sul banco una volta staccata
        [SerializeField] float batteryMaxX = 2.2f;  // la batteria trascinata resta dentro lo schermo
        [Header("Il pulsante STOP")]
        [SerializeField] Transform stopCap;
        [SerializeField] Collider2D stopArea;
        [Header("Effetti, scritte e festa")]
        [SerializeField] SpriteRenderer puff;
        [SerializeField] Transform zzz;
        [SerializeField] Transform trophy;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] TextMesh infoText;
        [SerializeField] Transform infoHolder;
        [SerializeField] LevelReward reward;

        const string Compliment = "WORLD SAVED. ABSOLUTE GENIUS!";
        const int FacePolite = 0, FaceEager = 1, FaceWorried = 2, FaceEmbarrassed = 3, FaceSleepy = 4;
        const float Follow = 28f, Spring = 70f, Damping = 9f, PuffTime = 0.9f, ErrorTime = 1.3f, StopPressTime = 0.22f;

        static readonly string[] StopLines = { "STOP? Sure! Faster!", "Stopping... at double speed!", "You did say FASTER, right?", "Happy to help! Faster!" };
        static readonly string[] TopSpeedLines = { "This IS my top speed. Sorry!", "I can't go faster. I tried!" };
        static readonly string[] WireErrors = { "WRONG WIRE.", "ALSO WRONG\nWIRE.", "ERR: TOO\nMANY WIRES", "NOPE :)", "ERR 404:\nCLUE NOT FOUND" };
        static readonly string[] WireLines = { "Oh dear. Not that one.", "That one is decorative.", "Stop pulling my spaghetti!", "The wires are just for show!" };
        static readonly string[] PokeLines = { "Please don't poke me.", "I'm ticking, not tickling.", "Excuse me!", "Rude. But I forgive you." };
        static readonly string[] PuffLines = { "pfff... boom?", "Pfff. Sorry, that was a dud.", "Pfff... how embarrassing." };
        static readonly string[] RestartLines = { "Not a boom. Just a puff.", "Restarting... politely.", "It's trying its best." };
        static readonly string[] WorryLines = { "W-wait! I need that!", "Hey! That's my battery!", "Please put that back!" };
        static readonly string[] SnapLines = { "Phew. Thank you!", "Click. All better.", "Don't scare me like that!" };
        static readonly Color DisplayGreen = new Color(0.62f, 1f, 0.7f), DisplayAmber = new Color(1f, 0.82f, 0.3f), DisplayRed = new Color(1f, 0.42f, 0.35f);

        PoliteBombLogic logic;
        Camera cam;
        HwToast toast;
        HwSay say;
        readonly HashSet<int> seen = new HashSet<int>();
        float[] wireKick;
        Vector3 lookScale, lookPos, sparkScale, trophyScale, puffScale, zzzScale, batteryScale, capPos, puffPos;
        Vector3 batteryHomeLocal;   // centro della batteria nel vano, nello spazio di "look"
        Vector2 batteryVis, batteryVel;
        float pulse, kick, stopPress, puffT = -1f, errorUntil = -1f, faceUntil = -1f, glowAlpha = 1f;
        int tempFace = FacePolite;
        string errorText = "";
        int stopLines, topLines, wireLines, pokeLines, puffLines, restartLines, worryLines, snapLines, wireErrors, nextWire;
        bool worrySpoken, captured, finishing;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            StopAllCoroutines();
            if (!captured)
            {
                captured = true;
                logic = new PoliteBombLogic();
                lookScale = look.localScale;
                lookPos = look.localPosition;
                batteryHomeLocal = look.InverseTransformPoint(battery.position);
                batteryScale = battery.localScale;
                if (spark != null) sparkScale = spark.localScale;
                if (trophy != null) trophyScale = trophy.localScale;
                if (zzz != null) zzzScale = zzz.localScale;
                if (puff != null) { puffScale = puff.transform.localScale; puffPos = puff.transform.position; }
                if (stopCap != null) capPos = stopCap.localPosition;
                wireKick = new float[wires != null ? wires.Length : 0];
            }
            logic.Reset();
            if (reward != null) reward.ResetReward();
            V64AFingers.Reset();
            finishing = false;
            worrySpoken = false;
            stopLines = topLines = wireLines = pokeLines = puffLines = restartLines = worryLines = snapLines = wireErrors = nextWire = 0;
            pulse = kick = stopPress = 0f;
            puffT = errorUntil = faceUntil = -1f;
            tempFace = FacePolite;
            errorText = "";
            say = new HwSay(bubble);
            say.Hide();
            toast = new HwToast(infoText, infoHolder);
            RestPose();
        }

        /// <summary>Tutto come all'inizio (o come dice la logica): batteria nel vano, display acceso, miccia accesa.</summary>
        void RestPose()
        {
            look.localScale = lookScale;
            look.localPosition = lookPos;
            look.localRotation = Quaternion.identity;
            for (int i = 0; i < wireKick.Length; i++)
            {
                wireKick[i] = 0f;
                if (wires[i] != null) wires[i].localRotation = Quaternion.identity;
            }
            if (stopCap != null) stopCap.localPosition = capPos;
            battery.localScale = batteryScale;
            battery.rotation = Quaternion.identity;
            batteryVis = Vector2.zero;
            batteryVel = Vector2.zero;
            PlaceBattery();
            glowAlpha = 1f;
            if (displayGlow != null) HalloweenKit.SetAlpha(displayGlow, 1f);
            if (display != null) { display.text = logic.Display; display.color = DisplayGreen; }
            if (spark != null) { spark.localScale = sparkScale; HalloweenKit.Show(spark, true); }
            if (puff != null) { HalloweenKit.Show(puff, false); puff.transform.localScale = puffScale; puff.transform.position = puffPos; }
            if (zzz != null) { zzz.localScale = zzzScale; HalloweenKit.Show(zzz, false); }
            if (trophy != null) { trophy.localScale = trophyScale; HalloweenKit.Show(trophy, false); }
            ShowFace(FacePolite);
        }

        void ShowFace(int f)
        {
            if (faces == null) return;
            for (int i = 0; i < faces.Length; i++) HalloweenKit.Show(faces[i], i == f);
        }

        /// <summary>La batteria nel vano (segue la bomba che pulsa) più lo spostamento mostrato, dentro lo schermo.</summary>
        void PlaceBattery()
        {
            Vector3 home = look.TransformPoint(batteryHomeLocal);
            Vector2 p = new Vector2(home.x + batteryVis.x, home.y + batteryVis.y);
            p.x = Mathf.Min(p.x, batteryMaxX);
            p = HalloweenKit.ClampSafe(p);
            battery.position = new Vector3(p.x, p.y, battery.position.z);
        }

        // ---------- Ciclo di vita ----------

        void OnDisable()
        {
            // pausa/pubblicità/livello spento: si lascia la batteria (torna nel vano), stato valido
            CancelAll();
            if (captured && !finishing && !IsFinished && logic != null && !logic.InputLocked) { batteryVis = Vector2.zero; PlaceBattery(); }
        }

        void OnEnable()
        {
            if (!captured || IsFinished || logic == null) return;
            // spento a metà del finale: lo si rifà da capo (la vittoria resta una sola)
            if (logic.Stage == PoliteBombStage.Defused) { finishing = false; StartCoroutine(Finale()); }
        }

        void OnApplicationPause(bool paused) { if (paused) CancelAll(); }
        void OnApplicationFocus(bool focus) { if (!focus) CancelAll(); }

        void CancelAll()
        {
            V64AFingers.Reset();
            worrySpoken = false;
            if (logic != null && !logic.InputLocked) logic.Cancel();
        }

        void Update()
        {
            toast?.Tick();
            say?.Tick();
            if (IsFinished || toast == null || logic == null || finishing) return;
            if (Time.timeScale <= 0f) { CancelAll(); return; }   // pausa o pubblicità: niente input
            OnTick(logic.Tick(Time.deltaTime));
#if UNITY_EDITOR
            EditorKeys();
#endif
            if (!finishing) ReadFingers();
            if (!finishing) Pose();
        }

        void OnTick(PoliteBombTick t)
        {
            switch (t)
            {
                case PoliteBombTick.Second:
                    pulse = 1f;
                    V82PoliteBombSound.Play(this, "tick", 0.25f + 0.05f * logic.Speed);
                    break;
                case PoliteBombTick.Puff:
                    // la gag: sbuffo innocuo, la bomba arrossisce (niente sconfitta)
                    puffT = 0f;
                    kick = 1f;
                    errorUntil = -1f;
                    V82PoliteBombSound.Play(this, "pfff", 0.6f);
                    Haptics.Light();
                    say.Show(PuffLines[puffLines++ % PuffLines.Length], 1.5f);
                    break;
                case PoliteBombTick.Restart:
                    V82PoliteBombSound.Play(this, "ding", 0.4f);
                    toast.Show(RestartLines[restartLines++ % RestartLines.Length], 1.5f);
                    break;
            }
        }

        /// <summary>
        /// La scena di ogni fotogramma (tutto ricavato da timer e logica, niente coroutine da interrompere): display,
        /// pulsazione al tic, tremolio dello sbuffo, scintilla, calotta di STOP, fili che dondolano, faccia, batteria.
        /// </summary>
        void Pose()
        {
            float dt = Mathf.Min(Time.deltaTime, 0.05f);
            float now = Time.time;

            // display
            if (display != null)
            {
                if (errorUntil > now) { display.text = errorText; display.color = DisplayRed; }
                else if (logic.Puffing) { display.text = Mathf.Sin(now * 14f) > 0f ? "00:00" : ""; display.color = DisplayRed; }
                else
                {
                    display.text = logic.Display;
                    display.color = logic.Speed >= 3f ? DisplayRed : logic.Speed >= 2f ? DisplayAmber : DisplayGreen;
                }
            }
            float flicker = logic.Worried ? 0.55f + 0.45f * Mathf.Abs(Mathf.Sin(now * 31f)) : 1f;   // la batteria esce: lo schermo sfarfalla
            if (displayGlow != null) HalloweenKit.SetAlpha(displayGlow, glowAlpha * flicker);

            // bomba: pulsazione al tic e tremolio dello sbuffo
            pulse = Mathf.Max(0f, pulse - dt * 6f);
            kick = Mathf.Max(0f, kick - dt * 2.2f);
            float s = 1f + 0.035f * pulse;
            look.localScale = new Vector3(lookScale.x * s, lookScale.y * s, lookScale.z);
            look.localPosition = lookPos + new Vector3(Mathf.Sin(now * 55f) * 0.05f * kick, Mathf.Sin(now * 43f) * 0.03f * kick, 0f);
            look.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(now * 37f) * 4f * kick);

            // scintilla della miccia (spenta durante lo sbuffo)
            if (spark != null)
            {
                bool on = !logic.Puffing;
                if (spark.gameObject.activeSelf != on) spark.gameObject.SetActive(on);
                float k = (0.85f + 0.25f * Mathf.Abs(Mathf.Sin(now * 23f))) * (1f + 0.15f * (logic.Speed - 1f));
                spark.localScale = sparkScale * k;
                spark.localRotation = Quaternion.Euler(0f, 0f, now * 220f);
            }

            // sbuffo
            if (puff != null && puffT >= 0f)
            {
                puffT += dt / PuffTime;
                if (puffT >= 1f) { puffT = -1f; HalloweenKit.Show(puff, false); }
                else
                {
                    HalloweenKit.Show(puff, true);
                    puff.transform.localScale = puffScale * Mathf.Lerp(0.5f, 1.3f, Mathf.Sqrt(puffT));
                    puff.transform.position = puffPos + new Vector3(0f, 0.5f * puffT, 0f);
                    HalloweenKit.SetAlpha(puff, 1f - puffT * puffT);
                }
            }

            // calotta di STOP
            stopPress = Mathf.Max(0f, stopPress - dt / StopPressTime);
            if (stopCap != null) stopCap.localPosition = capPos + new Vector3(0f, -0.1f * Mathf.Sin(stopPress * Mathf.PI * 0.5f), 0f);

            // fili che dondolano
            for (int i = 0; i < wireKick.Length; i++)
            {
                wireKick[i] = Mathf.Max(0f, wireKick[i] - dt * 1.6f);
                if (wires[i] != null) wires[i].localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(now * 18f + i) * 10f * wireKick[i]);
            }

            // faccia
            int face = FacePolite;
            if (logic.Puffing) face = FaceEmbarrassed;
            else if (logic.Worried) face = FaceWorried;
            else if (faceUntil > now) face = tempFace;
            ShowFace(face);

            // batteria: tenuta segue la logica da vicino, lasciata torna con una molla
            Vector2 target = new Vector2(logic.BatteryX, logic.BatteryY);
            if (logic.BatteryHeld)
            {
                batteryVis = Vector2.Lerp(batteryVis, target, 1f - Mathf.Exp(-dt * Follow));
                batteryVel = Vector2.zero;
            }
            else
            {
                batteryVel += (-(batteryVis - target) * Spring - batteryVel * Damping) * dt;
                batteryVis += batteryVel * dt;
            }
            PlaceBattery();
            battery.rotation = Quaternion.Euler(0f, 0f, logic.BatteryHeld ? Mathf.Sin(now * 30f) * 3f * logic.Pull : 0f);
        }

        void TempFace(int f, float seconds)
        {
            tempFace = f;
            faceUntil = Time.time + seconds;
        }

        // ---------- Dita ----------

        void ReadFingers()
        {
            List<V64AFinger> fingers = V64AFingers.Read(cam);
            seen.Clear();
            foreach (V64AFinger f in fingers)
            {
                seen.Add(f.id);
                if (f.began)
                {
                    if (logic.BatteryFinger == f.id) EndFinger(f.id);
                    BeginFinger(f.id, f.world);
                    if (finishing) return;
                }
                if (logic.BatteryFinger == f.id)
                {
                    MoveFinger(f.id, f.world);
                    if (finishing) return;
                }
                if (f.ended && logic.BatteryFinger == f.id) EndFinger(f.id);
            }
            // dito sparito senza "Ended": la batteria torna nel vano
            if (logic.BatteryHeld && !seen.Contains(logic.BatteryFinger)) EndFinger(logic.BatteryFinger);
        }

        static bool Over(Collider2D c, Vector2 p) => c != null && c.gameObject.activeInHierarchy && c.OverlapPoint(p);

        /// <summary>Un dito che scende: batteria (si afferra), STOP, un filo (il più vicino), la bomba. Una cosa sola per dito.</summary>
        void BeginFinger(int id, Vector2 w)
        {
            if (finishing || logic.InputLocked) return;
            if (Over(batteryArea, w))
            {
                if (logic.GrabBattery(id, w.x, w.y))
                {
                    worrySpoken = false;
                    Haptics.Tap();
                    V82PoliteBombSound.Play(this, "click", 0.25f);
                }
                return;
            }
            if (Over(stopArea, w)) { PressStop(); return; }
            int wire = -1;
            float best = float.MaxValue;
            for (int i = 0; wireAreas != null && i < wireAreas.Length; i++)
            {
                if (!Over(wireAreas[i], w)) continue;
                float d = Vector2.Distance(w, wireAreas[i].bounds.center);
                if (d < best) { best = d; wire = i; }
            }
            if (wire >= 0) { TouchWire(wire); return; }
            if (Over(bodyArea, w)) Poke();
        }

        void MoveFinger(int id, Vector2 w)
        {
            PoliteBombMove m = logic.MoveBattery(id, w.x, w.y);
            if (m == PoliteBombMove.Unplugged) { StartCoroutine(Finale()); return; }
            if (m == PoliteBombMove.Pulling && logic.Worried && !worrySpoken)
            {
                // stato intermedio: la bomba capisce e si preoccupa (una volta per presa)
                worrySpoken = true;
                say.Show(WorryLines[worryLines++ % WorryLines.Length], 1.4f);
                V82PoliteBombSound.Play(this, "boing", 0.3f);
                Haptics.Light();
            }
        }

        void EndFinger(int id)
        {
            PoliteBombDrop d = logic.ReleaseBattery(id);
            worrySpoken = false;
            if (d == PoliteBombDrop.SnapBack)
            {
                V82PoliteBombSound.Play(this, "click", 0.5f);
                Haptics.Light();
                say.Show(SnapLines[snapLines++ % SnapLines.Length], 1.4f);
                TempFace(FaceEager, 0.8f);
            }
            else if (d == PoliteBombDrop.Tap)
            {
                say.Show("Careful with that, please.", 1.3f);
                TempFace(FaceWorried, 0.6f);
            }
        }

        // ---------- Le false piste ----------

        void PressStop()
        {
            PoliteBombStop r = logic.PressStop();
            if (r == PoliteBombStop.Ignored) return;
            stopPress = 1f;
            Haptics.Tap();
            V82PoliteBombSound.Play(this, "beep", 0.45f);
            switch (r)
            {
                case PoliteBombStop.Faster:
                    pulse = 1f;
                    TempFace(FaceEager, 1.2f);
                    say.Show(StopLines[stopLines++ % StopLines.Length], 1.5f);
                    break;
                case PoliteBombStop.TopSpeed:
                    TempFace(FaceEager, 1.2f);
                    say.Show(TopSpeedLines[topLines++ % TopSpeedLines.Length], 1.5f);
                    break;
                case PoliteBombStop.Resting:
                    say.Show("One moment. Rebooting...", 1.2f);
                    break;
            }
        }

        void TouchWire(int i)
        {
            if (logic.TouchWire(i) < 0) return;
            if (i < wireKick.Length) wireKick[i] = 1f;
            errorText = WireErrors[wireErrors++ % WireErrors.Length];
            errorUntil = Time.time + ErrorTime;
            V82PoliteBombSound.Play(this, "buzz", 0.4f);
            Haptics.Light();
            say.Show(WireLines[wireLines++ % WireLines.Length], 1.6f);
        }

        void Poke()
        {
            if (logic.Poke() < 0) return;
            kick = Mathf.Max(kick, 0.35f);
            V82PoliteBombSound.Play(this, "boing", 0.4f);
            TempFace(FaceEmbarrassed, 0.7f);
            say.Show(PokeLines[pokeLines++ % PokeLines.Length], 1.4f);
        }

        // ---------- La soluzione ----------

        /// <summary>
        /// La batteria è staccata: vola sul banco e resta lì (input bloccato: non torna più), il display sfarfalla e si
        /// spegne, la miccia si spegne con uno sbuffetto, la bomba si addormenta e si affloscia ("zzz"), compare la coppa;
        /// festa comune; Win una volta sola. Rifatto da capo se il livello si spegne a metà.
        /// </summary>
        IEnumerator Finale()
        {
            if (finishing) yield break;
            finishing = true;
            V64AFingers.Reset();
            say.Hide();
            HalloweenKit.Show(infoHolder, false);
            errorUntil = -1f;
            for (int i = 0; i < wireKick.Length; i++) { wireKick[i] = 0f; if (wires[i] != null) wires[i].localRotation = Quaternion.identity; }
            if (stopCap != null) stopCap.localPosition = capPos;
            look.localPosition = lookPos;
            look.localRotation = Quaternion.identity;
            look.localScale = lookScale;
            ShowFace(FaceWorried);

            // 1) la batteria salta fuori e si posa sul banco (non può più tornare)
            V82PoliteBombSound.Play(this, "pop", 0.6f);
            Haptics.Medium();
            battery.localScale = batteryScale;
            Vector3 from = battery.position;
            Vector3 to = batteryRest != null ? batteryRest.position : from;
            to.z = from.z;
            Quaternion rFrom = battery.rotation, rTo = Quaternion.Euler(0f, 0f, -12f);
            for (float t = 0f; t < 0.4f; t += Time.deltaTime)
            {
                float k = Mathf.Clamp01(t / 0.4f);
                battery.position = Vector3.Lerp(from, to, 1f - (1f - k) * (1f - k)) + Vector3.up * (0.5f * Mathf.Sin(k * Mathf.PI));
                battery.rotation = Quaternion.Slerp(rFrom, rTo, k);
                yield return null;
            }
            battery.position = to;
            battery.rotation = rTo;

            // 2) il display sfarfalla e si spegne, la miccia si spegne con uno sbuffetto
            V82PoliteBombSound.Play(this, "power", 0.55f);
            string last = display != null ? display.text : "";
            for (int i = 0; i < 6; i++)
            {
                bool on = i % 2 == 1;
                if (displayGlow != null) HalloweenKit.SetAlpha(displayGlow, on ? 0.6f : 0.1f);
                if (display != null) display.text = on ? last : "";
                yield return new WaitForSeconds(0.07f);
            }
            glowAlpha = 0f;
            if (displayGlow != null) HalloweenKit.SetAlpha(displayGlow, 0f);
            if (display != null) display.text = logic.Display;   // vuoto: display spento
            HalloweenKit.Show(spark, false);
            if (puff != null && spark != null)
            {
                puff.transform.position = spark.position;
                puff.transform.localScale = puffScale * 0.45f;
                HalloweenKit.SetAlpha(puff, 0.9f);
                HalloweenKit.Show(puff, true);
                StartCoroutine(HalloweenKit.Fade(puff, 0f, 0.6f));
            }

            // 3) si addormenta: faccia assonnata, si affloscia un po', "zzz"
            ShowFace(FaceSleepy);
            for (float t = 0f; t < 0.35f; t += Time.deltaTime)
            {
                float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / 0.35f));
                look.localScale = new Vector3(lookScale.x * (1f + 0.05f * k), lookScale.y * (1f - 0.06f * k), lookScale.z);
                look.localPosition = lookPos + new Vector3(0f, -0.06f * k, 0f);
                yield return null;
            }
            if (zzz != null) { HalloweenKit.Show(zzz, true); StartCoroutine(Snooze()); }
            V82PoliteBombSound.Play(this, "snore", 0.4f);
            yield return new WaitForSeconds(0.2f);

            // 4) la coppa e la festa comune
            if (trophy != null)
            {
                HalloweenKit.Show(trophy, true);
                trophy.localScale = trophyScale * 0.01f;
                StartCoroutine(HalloweenKit.ScaleTo(trophy, trophyScale, 0.3f));
            }
            if (reward != null) yield return reward.Play(Compliment);
            else yield return new WaitForSeconds(1.5f);
            if (logic.TryComplete()) Win(Compliment);
        }

        /// <summary>Gli "zzz" salgono piano ondeggiando sopra la bomba addormentata.</summary>
        IEnumerator Snooze()
        {
            Vector3 home = zzz.position;
            for (float t = 0f; t < 5f; t += Time.deltaTime)
            {
                float k = (t % 1.6f) / 1.6f;
                zzz.position = home + new Vector3(Mathf.Sin(t * 3f) * 0.08f, 0.35f * k, 0f);
                zzz.localScale = zzzScale * (0.8f + 0.3f * Mathf.Sin(k * Mathf.PI));
                yield return null;
            }
        }

#if UNITY_EDITOR
        // ---------- Tasti di prova nell'Editor ----------

        const int KeyFinger = 901;

        void EditorKeys()
        {
            if (logic.InputLocked) return;
            if (Input.GetKeyDown(KeyCode.R)) { OnLevelStart(); return; }
            if (Input.GetKeyDown(KeyCode.P)) CancelAll();
            if (logic.BatteryHeld) return;
            Vector2 b = battery.position;
            if (Input.GetKeyDown(KeyCode.B) && logic.GrabBattery(KeyFinger, b.x, b.y))
            {
                for (int s = 1; s <= 10 && !finishing; s++) MoveFinger(KeyFinger, b + new Vector2(0.3f, -1.0f) * (s / 10f));
                if (!finishing) EndFinger(KeyFinger);
            }
            if (Input.GetKeyDown(KeyCode.N) && logic.GrabBattery(KeyFinger, b.x, b.y))
            {
                MoveFinger(KeyFinger, b + new Vector2(0.45f, -0.1f));
                batteryVis = new Vector2(logic.BatteryX, logic.BatteryY);
                EndFinger(KeyFinger);
            }
            if (Input.GetKeyDown(KeyCode.T) && logic.GrabBattery(KeyFinger, b.x, b.y)) EndFinger(KeyFinger);
            if (Input.GetKeyDown(KeyCode.S)) PressStop();
            if (Input.GetKeyDown(KeyCode.W)) TouchWire(nextWire++ % PoliteBombLogic.WireCount);
            if (Input.GetKeyDown(KeyCode.K)) Poke();
            if (Input.GetKeyDown(KeyCode.Z) && !logic.Puffing) OnTick(logic.Tick(logic.Remaining / logic.Speed + 0.01f));
        }
#endif
    }

    /// <summary>Suoni sintetizzati di The Polite Bomb: tic, bip di STOP, ronzio d'errore, "pfff", ding, boing, clic, pop, spegnimento, russare.</summary>
    public static class V82PoliteBombSound
    {
        const int Rate = 22050;
        static readonly Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>();
        static readonly System.Random rnd = new System.Random(842);

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
                case "tick":    // tic secco di orologio
                    c = Make(kind, 0.04f, t => (N() * 0.5f + Mathf.Sin(t * 2f * Mathf.PI * 2400f) * 0.4f) * Mathf.Exp(-t * 140f));
                    break;
                case "beep":    // "bi-bip!" servizievole che sale
                    c = Make(kind, 0.2f, t =>
                    {
                        float f = t < 0.09f ? 880f : 1320f;
                        return Mathf.Sign(Mathf.Sin(t * 2f * Mathf.PI * f)) * 0.18f * Mathf.Exp(-(t % 0.1f) * 18f);
                    });
                    break;
                case "buzz":    // ronzio d'errore basso
                    c = Make(kind, 0.3f, t => Mathf.Sign(Mathf.Sin(t * 2f * Mathf.PI * 110f)) * 0.22f * Mathf.Min(1f, t * 80f) * Mathf.Clamp01((0.3f - t) * 12f));
                    break;
                case "pfff":    // sbuffo di fumo floscio che si sgonfia
                    {
                        float lp = 0f;
                        c = Make(kind, 0.7f, t =>
                        {
                            lp += (N() - lp) * Mathf.Lerp(0.5f, 0.08f, t / 0.7f);
                            return lp * 0.7f * Mathf.Min(1f, t * 30f) * Mathf.Exp(-t * 3.5f);
                        });
                        break;
                    }
                case "ding":    // si riparte: "ding" educato
                    c = Make(kind, 0.4f, t => (Mathf.Sin(t * 2f * Mathf.PI * 1046f) + 0.4f * Mathf.Sin(t * 2f * Mathf.PI * 2092f)) * 0.2f * Mathf.Exp(-t * 8f));
                    break;
                case "boing":   // molla
                    c = Make(kind, 0.3f, t =>
                    {
                        float f = 300f + 180f * Mathf.Sin(t * 2f * Mathf.PI * 14f) * Mathf.Exp(-t * 6f);
                        return Mathf.Sin(t * 2f * Mathf.PI * f) * 0.3f * Mathf.Exp(-t * 7f);
                    });
                    break;
                case "click":   // la batteria nel vano
                    c = Make(kind, 0.05f, t => (N() * 0.4f + Mathf.Sin(t * 2f * Mathf.PI * 1800f) * 0.3f) * Mathf.Exp(-t * 90f));
                    break;
                case "pop":     // staccata!
                    c = Make(kind, 0.15f, t => N() * 0.35f * Mathf.Exp(-t * 50f) + Mathf.Sin(t * 2f * Mathf.PI * Mathf.Lerp(500f, 900f, t / 0.15f)) * 0.35f * Mathf.Exp(-t * 22f));
                    break;
                case "power":   // spegnimento: tono che scende
                    c = Make(kind, 0.6f, t =>
                    {
                        float f = Mathf.Lerp(700f, 60f, Mathf.Sqrt(t / 0.6f));
                        return Mathf.Sin(t * 2f * Mathf.PI * f) * 0.25f * Mathf.Clamp01((0.6f - t) * 6f);
                    });
                    break;
                case "snore":   // russare morbido
                    {
                        float lp = 0f;
                        c = Make(kind, 0.8f, t =>
                        {
                            lp += (N() - lp) * 0.06f;
                            float env = Mathf.Sin(Mathf.Clamp01(t / 0.8f) * Mathf.PI);
                            return (lp * 0.6f + Mathf.Sin(t * 2f * Mathf.PI * 90f) * 0.12f) * env;
                        });
                        break;
                    }
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
            AudioClip clip = AudioClip.Create("v82a_" + name, n, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
