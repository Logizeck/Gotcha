using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;
using Gotcha.Sensors;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello composito "Haunted house" — 3 fasi.
    /// 1) Salone buio: il dito è una torcia (si vede solo intorno al dito). Trovare le 3 candele nascoste (posti a
    ///    caso); due occhi gialli nel buio sono solo uno spavento.
    /// 2) Accendere le candele: trascinare il fiammifero e strofinarlo VELOCE avanti e indietro sulla striscia della
    ///    scatola finché si accende, poi portarlo su ogni candela prima che si consumi (8 secondi). 3 fiammiferi; le
    ///    candele accese restano accese.
    /// 3) Appare un fantasma che sussurra una parola: è minuscola e quasi invisibile ("Turn it UP!"). Bisogna
    ///    premere il tasto VOLUME SU del telefono: a ogni pressione il sussurro si sente più forte e la scritta
    ///    cresce. Poi toccare la porta con quella parola (le altre due sono quasi uguali). Porta sbagliata: "BOO!",
    ///    vibrazione e un cuore in meno (3). Vittoria: il fantasma saluta.
    /// (Editor: tasto "=" = volume su.)
    /// </summary>
    public class HauntedLevel : LevelController
    {
        [SerializeField] StageHud40A hud;
        [SerializeField] SpeechBubble bubble;
        [Header("Fase 1")]
        [SerializeField] Transform torch;               // sprite "darkness" col buco, segue il dito
        [SerializeField] SpriteRenderer blackout;
        [SerializeField] SpriteRenderer dim;
        [SerializeField] Transform[] candles;           // figli "Art", "Flame", "Sparkle"
        [SerializeField] Transform[] candleSpots;
        [SerializeField] Transform eyes;
        [SerializeField] TextMesh foundLabel;
        [Header("Fase 2")]
        [SerializeField] GameObject matchKit;
        [SerializeField] Collider2D strikeArea;
        [SerializeField] Transform match;
        [SerializeField] SpriteRenderer matchArt;
        [SerializeField] Sprite matchSprite;
        [SerializeField] Sprite burntSprite;
        [SerializeField] GameObject matchFlame;
        [SerializeField] Transform burnBar;
        [SerializeField] TextMesh matchesLabel;
        [SerializeField] float burnSeconds = 8f, rubNeeded = 3f, rubSpeed = 2.5f;
        [Header("Fase 3")]
        [SerializeField] Transform ghost;
        [SerializeField] SpriteRenderer ghostArt;
        [SerializeField] Sprite ghostCalm;
        [SerializeField] Sprite ghostWave;
        [SerializeField] Sprite ghostBoo;
        [SerializeField] TextMesh whisper;
        [SerializeField] Collider2D[] doors;
        [SerializeField] TextMesh[] doorLabels;
        [SerializeField] SpriteRenderer[] doorArts;
        [SerializeField] Sprite doorOpen;

        static readonly string[][] Words =
        {
            new[] { "MOON", "NOON", "MOOD" }, new[] { "13", "31", "18" }, new[] { "BAT", "CAT", "RAT" },
            new[] { "LEFT", "LIFT", "LOFT" }, new[] { "CANDLE", "HANDLE", "CANDY" }, new[] { "BONE", "CONE", "BONK" },
        };

        Camera cam;
        float bubbleUntil;
        int lastKeyCount, volumeLevel;
        float lastVolume, keyGuard;
        bool ducked;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            // candele e occhi in posti a caso
            var order = new System.Collections.Generic.List<int>();
            for (int i = 0; i < candleSpots.Length; i++) order.Add(i);
            for (int i = order.Count - 1; i > 0; i--) { int j = Random.Range(0, i + 1); (order[i], order[j]) = (order[j], order[i]); }
            for (int i = 0; i < candles.Length; i++)
            {
                candles[i].position = candleSpots[order[i]].position;
                SetCandle(candles[i], false);
                candles[i].Find("Sparkle").gameObject.SetActive(false);
            }
            eyes.position = candleSpots[order[candles.Length]].position + new Vector3(0f, 0.4f, 0f);
            torch.gameObject.SetActive(false);
            blackout.enabled = true;
            dim.enabled = false;
            matchKit.SetActive(false);
            ghost.gameObject.SetActive(false);
            whisper.transform.parent.gameObject.SetActive(false);
            foundLabel.text = "";
            StartCoroutine(Run());
        }

        void OnDestroy()
        {
            if (ducked && MusicPlayer.Instance != null) MusicPlayer.Instance.Unduck(this);
        }

        void Say(string text, float seconds)
        {
            if (bubble == null) return;
            bubble.Show(text);
            bubbleUntil = Time.time + seconds;
        }

        void Update()
        {
            if (bubble != null && bubbleUntil > 0f && Time.time > bubbleUntil) { bubble.Hide(); bubbleUntil = 0f; }
        }

        static void SetCandle(Transform c, bool lit)
        {
            c.Find("Flame").gameObject.SetActive(lit);
        }

        Vector3 Wick(Transform c) => c.TransformPoint(new Vector3(0f, 0.75f, 0f));

        IEnumerator Run()
        {
            yield return Stage1Search();
            yield return hud.Clear("3 CANDLES!");
            bool ok = false;
            yield return Stage2Light(r => ok = r);
            if (!ok) yield break;
            yield return hud.Clear("LET THERE BE LIGHT!");
            yield return Stage3Whisper();
        }

        // ---------- Fase 1: la torcia ----------

        IEnumerator Stage1Search()
        {
            hud.Stage(0, "Your finger is a flashlight: find 3 candles!");
            Say("It's so dark in here...", 2f);
            var lit = new float[candles.Length];
            var found = new bool[candles.Length];
            int count = 0;
            float eyesLit = 0f;
            while (count < candles.Length)
            {
                bool on = PointerInput.TryGetPrimary(out Vector2 sp, out _, out _) && !PointerInput.IsOverUi();
                torch.gameObject.SetActive(on);
                blackout.enabled = !on;
                if (on)
                {
                    Vector2 w = PointerInput.ToWorld(cam, sp);
                    torch.position = new Vector3(w.x, w.y, torch.position.z);
                    for (int i = 0; i < candles.Length; i++)
                    {
                        if (found[i]) continue;
                        if (Vector2.Distance(w, candles[i].position + Vector3.up * 0.35f) < 0.8f) lit[i] += Time.deltaTime;
                        if (lit[i] > 0.3f)
                        {
                            found[i] = true;
                            count++;
                            candles[i].Find("Sparkle").gameObject.SetActive(true);
                            Sfx.Play(this, "ding", 0.5f);
                            Haptics.Light();
                            foundLabel.text = count + " / " + candles.Length;
                        }
                    }
                    // gli occhi nel buio: spavento e spariscono altrove
                    if (eyes.gameObject.activeSelf && Vector2.Distance(w, eyes.position) < 0.8f)
                    {
                        eyesLit += Time.deltaTime;
                        if (eyesLit > 0.25f)
                        {
                            eyesLit = 0f;
                            Sfx.Play(this, "whoosh", 0.6f);
                            Haptics.Light();
                            Say("...was that a pair of EYES?!", 2f);
                            eyes.gameObject.SetActive(false);
                        }
                    }
                }
                yield return null;
            }
            torch.gameObject.SetActive(false);
            blackout.enabled = false;
            dim.enabled = true;
            dim.color = new Color(0.02f, 0.01f, 0.05f, 0.82f);
            foreach (Transform c in candles)
            {
                foreach (SpriteRenderer sr in c.GetComponentsInChildren<SpriteRenderer>(true)) sr.sortingOrder += 60;   // sopra il buio
                c.Find("Sparkle").gameObject.SetActive(false);
            }
            foundLabel.text = "";
        }

        // ---------- Fase 2: il fiammifero ----------

        IEnumerator Stage2Light(System.Action<bool> done)
        {
            hud.Stage(1, "Rub the match FAST on the box, then light the candles!");
            matchKit.SetActive(true);
            Vector3 home = match.localPosition;
            int matches = 3, litCount = 0;
            var candleLit = new bool[candles.Length];
            while (litCount < candles.Length)
            {
                // un fiammifero nuovo
                match.localPosition = home;
                match.localRotation = Quaternion.identity;
                matchArt.sprite = matchSprite;
                matchArt.color = Color.white;
                matchFlame.SetActive(false);
                burnBar.parent.gameObject.SetActive(false);
                matchesLabel.text = "MATCHES: " + matches;
                bool held = false, lit = false;
                Vector2 grab = Vector2.zero, lastHead = Vector2.zero;
                float rub = 0f, burn = 0f, scratchAt = 0f;
                while (true)
                {
                    float dt = Time.deltaTime;
                    bool down = PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out _) && !PointerInput.IsOverUi();
                    Vector2 w = down ? PointerInput.ToWorld(cam, sp) : Vector2.zero;
                    if (down && began && Vector2.Distance(w, match.position + Vector3.down * 0.4f) < 0.75f) { held = true; grab = (Vector2)match.position - w; }
                    if (!down) held = false;
                    if (held)
                    {
                        Vector2 p = w + grab;
                        p.x = Mathf.Clamp(p.x, -2.7f, 2.7f);
                        p.y = Mathf.Clamp(p.y, -3.9f, 3.3f);
                        match.position = new Vector3(p.x, p.y, match.position.z);
                    }
                    Vector2 head = match.position;
                    if (!lit)
                    {
                        float speed = dt > 0f ? Mathf.Abs(head.x - lastHead.x) / dt : 0f;
                        if (held && strikeArea.OverlapPoint(head) && speed > rubSpeed)
                        {
                            rub += Mathf.Abs(head.x - lastHead.x);
                            scratchAt -= dt;
                            if (scratchAt <= 0f) { scratchAt = 0.09f; Synth40A.Play(this, "scratch", 0.5f); }
                        }
                        else rub = Mathf.Max(0f, rub - dt * 0.8f);
                        if (rub >= rubNeeded)
                        {
                            lit = true;
                            matchFlame.SetActive(true);
                            burnBar.parent.gameObject.SetActive(true);
                            Synth40A.Play(this, "flare", 0.7f);
                            Haptics.Light();
                            hud.Goal("Quick! Touch each candle wick with the flame!");
                        }
                    }
                    else
                    {
                        burn += dt;
                        burnBar.localScale = new Vector3(Mathf.Clamp01(1f - burn / burnSeconds), 1f, 1f);
                        matchFlame.transform.localScale = Vector3.one * Mathf.Lerp(1f, 0.6f, burn / burnSeconds) * (1f + Mathf.Sin(Time.time * 25f) * 0.08f);
                        for (int i = 0; i < candles.Length; i++)
                        {
                            if (candleLit[i] || Vector2.Distance(head, Wick(candles[i])) > 0.55f) continue;
                            candleLit[i] = true;
                            litCount++;
                            SetCandle(candles[i], true);
                            Synth40A.Play(this, "flare", 0.5f);
                            Haptics.Light();
                            dim.color = new Color(0.02f, 0.01f, 0.05f, Mathf.Lerp(0.82f, 0.35f, litCount / (float)candles.Length));
                        }
                        if (litCount >= candles.Length) break;
                        if (burn >= burnSeconds) break;
                    }
                    lastHead = head;
                    yield return null;
                }
                if (litCount >= candles.Length) break;
                // si è consumato
                matchFlame.SetActive(false);
                burnBar.parent.gameObject.SetActive(false);
                matchArt.sprite = burntSprite;
                Sfx.Play(this, "buzz", 0.4f);
                matches--;
                matchesLabel.text = "MATCHES: " + matches;
                Say(matches > 0 ? "Ouch! Burned out. Strike another!" : "No more matches...", 2f);
                Vector3 from = match.position;
                for (float k = 0f; k < 1f; k += Time.deltaTime / 0.6f)
                {
                    match.position = from + Vector3.down * k * k * 1.5f;
                    match.localRotation = Quaternion.Euler(0, 0, k * 70f);
                    matchArt.color = new Color(1f, 1f, 1f, 1f - k);
                    yield return null;
                }
                if (matches <= 0)
                {
                    Sfx.Play(this, "boo", 0.7f);
                    yield return new WaitForSeconds(1.2f);
                    Fail();
                    done(false);
                    yield break;
                }
                hud.Goal("Rub the match FAST on the box, then light the candles!");
            }
            matchFlame.SetActive(false);
            matchKit.SetActive(false);
            done(true);
        }

        // ---------- Fase 3: il sussurro ----------

        int VolumeKeyCount()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try { using (var keys = new AndroidJavaClass("com.gotcha.game.GotchaKeys")) return keys.CallStatic<int>("volumeUpCount"); }
            catch { return 0; }
#else
            return 0;
#endif
        }

        /// <summary>True nel frame in cui è stato premuto "volume su" (contatore dei tasti, o volume salito, o "=" nell'Editor).</summary>
        bool VolumeUpPressed()
        {
            bool pressed = false;
            int keysNow = VolumeKeyCount();
            if (keysNow > lastKeyCount) { pressed = true; keyGuard = 0.6f; }
            lastKeyCount = keysNow;
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.Equals) || Input.GetKeyDown(KeyCode.KeypadPlus)) { pressed = true; keyGuard = 0.6f; }
#endif
            float v = SensorHub.Instance != null ? SensorHub.Instance.SystemVolume01 : lastVolume;
            keyGuard -= Time.deltaTime;
            if (v > lastVolume + 0.001f && keyGuard <= 0f) pressed = true;   // se il tasto non si può leggere, basta il volume salito
            lastVolume = v;
            return pressed;
        }

        void ShowWhisper()
        {
            float[] alpha = { 0.12f, 0.3f, 0.55f, 0.8f, 1f };
            float[] size = { 0.28f, 0.45f, 0.65f, 0.85f, 1f };
            whisper.color = new Color(0.85f, 0.95f, 1f, alpha[volumeLevel]);
            whisper.transform.parent.localScale = Vector3.one * size[volumeLevel];   // si scala il contenitore
        }

        IEnumerator Stage3Whisper()
        {
            string[] set = Words[Random.Range(0, Words.Length)];
            string word = set[0];
            // porte in ordine a caso
            var labels = new System.Collections.Generic.List<string>(set);
            for (int i = labels.Count - 1; i > 0; i--) { int j = Random.Range(0, i + 1); (labels[i], labels[j]) = (labels[j], labels[i]); }
            for (int i = 0; i < doorLabels.Length; i++) doorLabels[i].text = labels[i];

            if (MusicPlayer.Instance != null) { MusicPlayer.Instance.Duck(this, 0.2f); ducked = true; }
            ghost.gameObject.SetActive(true);
            ghostArt.sprite = ghostCalm;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.6f) { ghostArt.color = new Color(1f, 1f, 1f, k * 0.9f); yield return null; }
            whisper.transform.parent.gameObject.SetActive(true);
            whisper.text = "psst... " + word + "... " + word + "...";
            volumeLevel = 0;
            ShowWhisper();
            lastKeyCount = VolumeKeyCount();
            lastVolume = SensorHub.Instance != null ? SensorHub.Instance.SystemVolume01 : 1f;
            hud.Stage(2, "Hear the ghost's word, then tap that door");
            Say("Turn it UP!", 2.5f);
            float whisperAt = 0.3f, nag = 6f;
            Vector3 ghostHome = ghost.localPosition;
            while (true)
            {
                float dt = Time.deltaTime;
                ghost.localPosition = ghostHome + Vector3.up * Mathf.Sin(Time.time * 2f) * 0.1f;
                whisperAt -= dt;
                if (whisperAt <= 0f) { whisperAt = 3f; Synth40A.Play(this, "whisper", 0.06f + volumeLevel * 0.2f); }
                if (VolumeUpPressed() && volumeLevel < 4)
                {
                    volumeLevel++;
                    ShowWhisper();
                    Synth40A.Play(this, "whisper", 0.06f + volumeLevel * 0.2f);
                    whisperAt = 3f;
                    if (volumeLevel == 4) Say("Now I can hear it!", 1.6f);
                }
                nag -= dt;
                if (nag <= 0f && volumeLevel < 2) { nag = 6f; Say("I can't hear it! Turn it UP!", 2.2f); }

                if (PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out _) && began && !PointerInput.IsOverUi())
                {
                    Vector2 w = PointerInput.ToWorld(cam, sp);
                    Collider2D gc = ghost.GetComponent<Collider2D>();
                    if (gc != null && gc.OverlapPoint(w)) Say(volumeLevel < 3 ? "(whisper whisper...) Turn it UP!" : "(" + word.ToLower() + "...)", 2f);
                    for (int i = 0; i < doors.Length; i++)
                    {
                        if (!doors[i].OverlapPoint(w)) continue;
                        if (doorLabels[i].text == word) { yield return Goodbye(i, ghostHome); yield break; }
                        yield return Boo(ghostHome);
                        if (hud.TriesLeft <= 0) { yield return new WaitForSeconds(0.4f); Fail(); yield break; }
                        break;
                    }
                }
                yield return null;
            }
        }

        IEnumerator Boo(Vector3 ghostHome)
        {
            ghostArt.sprite = ghostBoo;
            Synth40A.Play(this, "boo", 0.9f);
            Haptics.Medium();
            Say("BOO!!!", 1.4f);
            hud.LoseTry();
            Vector3 s = ghost.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.35f)
            {
                ghost.localScale = s * Mathf.Lerp(1f, 2.2f, Mathf.Sin(k * Mathf.PI * 0.5f));
                ghost.localPosition = Vector3.Lerp(ghostHome, new Vector3(0f, 0.2f, ghostHome.z), k);
                yield return null;
            }
            yield return new WaitForSeconds(0.5f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.4f)
            {
                ghost.localScale = s * Mathf.Lerp(2.2f, 1f, k);
                ghost.localPosition = Vector3.Lerp(new Vector3(0f, 0.2f, ghostHome.z), ghostHome, k);
                yield return null;
            }
            ghost.localScale = s;
            ghost.localPosition = ghostHome;
            ghostArt.sprite = ghostCalm;
            if (hud.TriesLeft > 0) Say("Wrong door! Listen better...", 1.8f);
        }

        IEnumerator Goodbye(int door, Vector3 ghostHome)
        {
            doorArts[door].sprite = doorOpen;
            Sfx.Play(this, "clunk", 0.7f);
            yield return hud.Clear("YOU'RE FREE!");
            ghostArt.sprite = ghostWave;
            Say("Bye bye! Come back for Halloween!", 2.5f);
            Sfx.Play(this, "ding", 0.7f);
            Vector3 target = doors[door].transform.position;
            for (float k = 0f; k < 2f; k += Time.deltaTime)
            {
                ghost.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(k * 10f) * 6f);
                if (k > 1f)
                {
                    float e = k - 1f;
                    ghost.position = Vector3.Lerp(ghost.parent.TransformPoint(ghostHome), new Vector3(target.x, target.y + 0.9f, ghost.position.z), e);
                    ghostArt.color = new Color(1f, 1f, 1f, 0.9f * (1f - e));
                }
                yield return null;
            }
            if (MusicPlayer.Instance != null) { MusicPlayer.Instance.Unduck(this); ducked = false; }
            Win();
        }
    }
}
