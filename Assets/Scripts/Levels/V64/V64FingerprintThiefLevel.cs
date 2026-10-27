using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels.V64;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 251 "The Fingerprint Thief" — "Unlock the phone without knowing the PIN." Sul tavolo un telefono bloccato
    /// (tastierino, lettore d'impronte con il LED, "3 attempts remaining") e una foto del proprietario che saluta con la
    /// mano alzata. Nessun PIN va bene (al terzo errore un blocco finto di 30 secondi), il dito del giocatore è
    /// "UNKNOWN FINGERPRINT", scuotere fa cadere il telefono di lato ("VIOLENCE IS NOT A PASSWORD.").
    /// La foto si trascina (con un dito, o tenendola con un dito e spingendola con un altro); sopra il lettore diventa un
    /// po' trasparente e si aggancia alla finestrella: lasciata lì fa solo lampeggiare il LED giallo. Tenendola premuta e
    /// inclinando piano il telefono scivola nel vassoio: quando il polpastrello dell'indice della foto è sul lettore, un
    /// secondo e il LED diventa verde. "FACE ID ENABLED", faccione, "SECURITY LEVEL: ABSOLUTELY TERRIBLE."
    /// La logica è in Gotcha.Levels.V64.FingerprintThiefLogic (senza Unity): qui solo input e presentazione.
    /// </summary>
    public class V64FingerprintThiefLevel : V62Level
    {
        [SerializeField] Transform phone;              // il telefono intero (tremolio e caduta di lato)
        [SerializeField] Transform[] keys;             // tasti 0..9
        [SerializeField] Collider2D[] keyAreas;
        [SerializeField] TextMesh pinText;
        [SerializeField] TextMesh statusText;
        [SerializeField] Transform scanner;            // centro del lettore
        [SerializeField] Collider2D scannerArea;
        [SerializeField] SpriteRenderer scannerGlow;
        [SerializeField] SpriteRenderer led;
        [SerializeField] SpriteRenderer ledGlow;
        [SerializeField] Transform photo;
        [SerializeField] Collider2D photoArea;
        [SerializeField] Transform photoTip;           // polpastrello dell'indice nella foto
        [SerializeField] SpriteRenderer[] photoArts;
        [SerializeField] GameObject lockScreen;        // tastierino, testi e lettore
        [SerializeField] GameObject unlockScreen;      // schermo sbloccato
        [SerializeField] TextMesh unlockText;
        [SerializeField] Transform bigFace;            // il faccione del Face ID
        [SerializeField] TextMesh infoText;
        [SerializeField] Transform infoHolder;

        // area in cui può stare il centro della foto (resta tutta dentro lo schermo, anche sui telefoni più corti)
        const float PhotoMinX = -1.35f, PhotoMaxX = 1.35f, PhotoMinY = -3.4f, PhotoMaxY = 1.9f;
        const float UndockDistance = 0.9f;
        const float ShakeLevel = 14f;

        static readonly string[] WrongPinLines = { "WRONG PIN", "STILL WRONG", "NOPE. NOT EVEN CLOSE." };
        static readonly string[] LockedTapLines = { "Patience is also not a password.", "The phone is sulking. Try something else.", "Locked. Not broken. Just rude." };

        readonly FingerprintThiefLogic logic = new FingerprintThiefLogic();
        readonly V64AGravity gravity = new V64AGravity();
        readonly Dictionary<int, Vector2> photoFingers = new Dictionary<int, Vector2>();   // dita sulla foto (ultima posizione)
        readonly Dictionary<int, Vector2> pushFingers = new Dictionary<int, Vector2>();    // secondo dito che spinge la foto
        Camera cam;
        HwToast toast;
        Vector3 phoneHome, photoHome, dockPos;
        Quaternion phoneRot;
        Vector2 undockFrom;
        bool busy, clearBeforeRedock, celebrating;
        int lockedTaps;
        Coroutine photoMove, flashKey;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            logic.Reset();
            gravity.Reset();
            ResetV62();
            V64AFingers.Reset();
            toast = new HwToast(infoText, infoHolder);
            phoneHome = phone.localPosition;
            phoneRot = phone.localRotation;
            photoHome = photo.position;
            // la foto si aggancia con il polpastrello un po' in alto a destra del lettore
            Vector3 tipLocal = photoTip.position - photo.position;
            dockPos = scanner.position - tipLocal + new Vector3(FingerprintThiefLogic.DockMisalignX, FingerprintThiefLogic.DockMisalignY, 0f);
            dockPos.z = photo.position.z;
            HalloweenKit.Show(lockScreen, true);
            HalloweenKit.Show(unlockScreen, false);
            HalloweenKit.Show(bigFace, false);
            SetPhotoAlpha(1f);
            UpdateTexts();
            ShowLed();
        }

        void OnDisable()
        {
            photoFingers.Clear();
            pushFingers.Clear();
        }

        void Update()
        {
            toast.Tick();
            if (IsFinished || celebrating) return;
            if (Time.timeScale <= 0f) { photoFingers.Clear(); pushFingers.Clear(); return; }   // pausa o pubblicità: niente input né sensori
            float dt = Time.deltaTime;
            Vector2 tilt = Inputs.Tilt();
            gravity.Feed(tilt.x, tilt.y, dt);
            if (logic.Tick(dt)) toast.Show("Lockout over. Three more wrong guesses available.", 1.8f);
            UpdateTexts();

            if (!busy && Inputs.Shake() > ShakeLevel && logic.Shake()) StartCoroutine(ShakeReaction());

            ReadFingers();
            if (logic.Docked)
            {
                logic.SetContact(photoFingers.Count > 0, gravity.X, gravity.Y);
                logic.Tilt(gravity.X, gravity.Y, dt);
                if (photoMove == null) photo.position = dockPos + new Vector3(logic.OffsetX, logic.OffsetY, 0f);
                if (logic.Step(dt)) { StartCoroutine(Unlock()); return; }
            }
            SetPhotoAlpha(PhotoOverScanner() ? 0.72f : 1f);
            ShowLed();
        }

        // ---------- Dita ----------

        void ReadFingers()
        {
            List<V64AFinger> fingers = V64AFingers.Read(cam);
            Vector2 photoDelta = Vector2.zero, pushDelta = Vector2.zero;
            int photoMoved = 0, pushMoved = 0;
            foreach (V64AFinger f in fingers)
            {
                if (f.began)
                {
                    if (photoArea.OverlapPoint(f.world))
                    {
                        photoFingers[f.id] = f.world;
                        if (photoMove != null) { StopCoroutine(photoMove); photoMove = null; }
                        if (logic.Docked && photoFingers.Count == 1) undockFrom = f.world;
                        Haptics.Tap();
                    }
                    else if (photoFingers.Count > 0) pushFingers[f.id] = f.world;   // il secondo dito spinge la foto
                    else if (!busy) Tap(f.world);
                }
                if (f.began) { if (f.ended) { photoFingers.Remove(f.id); pushFingers.Remove(f.id); } continue; }
                if (photoFingers.TryGetValue(f.id, out Vector2 last))
                {
                    photoDelta += f.world - last;
                    photoMoved++;
                    photoFingers[f.id] = f.world;
                    if (logic.Docked && Vector2.Distance(f.world, undockFrom) > UndockDistance) Undock(f.world - undockFrom);
                }
                else if (pushFingers.TryGetValue(f.id, out Vector2 plast))
                {
                    pushDelta += f.world - plast;
                    pushMoved++;
                    pushFingers[f.id] = f.world;
                }
                if (f.ended) { photoFingers.Remove(f.id); pushFingers.Remove(f.id); }
            }
            if (photoFingers.Count == 0) pushFingers.Clear();
            if (logic.Docked) return;

            // foto libera: la sposta il dito che spinge (se c'è), altrimenti il dito che la tiene
            Vector2 move = pushMoved > 0 ? pushDelta / pushMoved : photoMoved > 0 ? photoDelta / photoMoved : Vector2.zero;
            if (move.sqrMagnitude > 0f) MovePhoto((Vector2)photo.position + move);
            bool over = PhotoOverScanner();
            if (!over) clearBeforeRedock = false;
            if (over && !clearBeforeRedock && photoFingers.Count > 0 && move.sqrMagnitude > 0f) Dock();
        }

        void MovePhoto(Vector2 p)
        {
            p.x = Mathf.Clamp(p.x, PhotoMinX, PhotoMaxX);
            p.y = Mathf.Clamp(p.y, PhotoMinY, PhotoMaxY);
            photo.position = new Vector3(p.x, p.y, photo.position.z);
        }

        /// <summary>La foto copre il lettore (tolleranza larga: basta che il lettore sia sotto la foto).</summary>
        bool PhotoOverScanner() => photoArea.OverlapPoint(scanner.position);

        void Dock()
        {
            if (!logic.Dock()) return;
            Sfx.Play(this, "clunk", 0.5f);
            Haptics.Light();
            foreach (KeyValuePair<int, Vector2> kv in photoFingers) { undockFrom = kv.Value; break; }
            if (photoMove != null) StopCoroutine(photoMove);
            photoMove = StartCoroutine(SnapPhoto(dockPos));
        }

        void Undock(Vector2 pulled)
        {
            logic.Undock();
            clearBeforeRedock = true;
            if (photoMove != null) { StopCoroutine(photoMove); photoMove = null; }
            MovePhoto((Vector2)dockPos + pulled);
            Sfx.Play(this, "tick", 0.5f);
        }

        IEnumerator SnapPhoto(Vector3 to)
        {
            yield return HalloweenKit.Move(photo, to, 0.15f);
            photoMove = null;
        }

        void Tap(Vector2 w)
        {
            if (scannerArea.OverlapPoint(w))
            {
                string line = logic.TapScanner();
                if (line == null) return;
                Sfx.Play(this, "buzz", 0.45f);
                Haptics.Light();
                StartCoroutine(HalloweenKit.Pop(scanner, 1.08f, 0.2f));
                toast.Show(line, 1.8f);
                return;
            }
            for (int i = 0; i < keyAreas.Length; i++)
            {
                if (keyAreas[i] == null || !keyAreas[i].OverlapPoint(w)) continue;
                PressKey(i);
                return;
            }
        }

        void PressKey(int digit)
        {
            FpPinResult r = logic.PressDigit(digit);
            if (r == FpPinResult.Ignored) return;
            if (flashKey != null) StopCoroutine(flashKey);
            flashKey = StartCoroutine(HalloweenKit.Pop(keys[digit], 0.88f, 0.12f));
            switch (r)
            {
                case FpPinResult.Digit:
                    Sfx.Play(this, "tick", 0.6f);
                    break;
                case FpPinResult.Wrong:
                    V64ASound.Play(this, "error", 0.7f);
                    Haptics.Light();
                    StartCoroutine(HalloweenKit.Shake(phone, 0.05f, 0.3f));
                    toast.Show(WrongPinLines[(logic.WrongPins - 1) % WrongPinLines.Length], 1.5f);
                    break;
                case FpPinResult.LockedOut:
                    V64ASound.Play(this, "error", 0.8f);
                    Haptics.Medium();
                    StartCoroutine(HalloweenKit.Shake(phone, 0.08f, 0.4f));
                    toast.Show("PHONE LOCKED — 30 SECONDS", 2f);
                    break;
                case FpPinResult.StillLocked:
                    Sfx.Play(this, "bonk", 0.4f);
                    toast.Show(LockedTapLines[lockedTaps++ % LockedTapLines.Length], 1.6f);
                    break;
            }
            UpdateTexts();
        }

        // ---------- Presentazione ----------

        void UpdateTexts()
        {
            if (logic.Unlocked) return;
            if (logic.IsLockedOut)
            {
                statusText.text = "PHONE LOCKED — " + logic.LockoutSecondsShown + " SECONDS";
                statusText.color = new Color(1f, 0.45f, 0.4f);
                pinText.text = "";
                return;
            }
            statusText.text = logic.AttemptsLeft + (logic.AttemptsLeft == 1 ? " attempt remaining" : " attempts remaining");
            statusText.color = new Color(0.85f, 0.88f, 1f);
            var sb = new System.Text.StringBuilder(8);
            for (int i = 0; i < FingerprintThiefLogic.PinLength; i++) sb.Append(i < logic.Entered ? "• " : "_ ");
            pinText.text = sb.ToString().TrimEnd();
        }

        void ShowLed()
        {
            Color c;
            switch (logic.Led)
            {
                case FpLed.Red: c = new Color(1f, 0.25f, 0.2f); break;
                case FpLed.Yellow: c = new Color(1f, 0.85f, 0.15f); break;
                case FpLed.Green: c = new Color(0.3f, 1f, 0.35f); break;
                default: c = new Color(0.35f, 0.35f, 0.42f); break;
            }
            // allineata e premuta: il giallo pulsa sempre più in fretta mentre conta il secondo
            if (logic.Led == FpLed.Yellow && logic.AlignTimer > 0f)
                c = Color.Lerp(c, Color.white, 0.5f * (0.5f + 0.5f * Mathf.Sin(Time.time * 30f)));
            if (led != null) led.color = c;
            bool lit = logic.Led != FpLed.Off;
            if (ledGlow != null) ledGlow.color = new Color(c.r, c.g, c.b, lit ? 0.7f : 0f);
            if (scannerGlow != null) scannerGlow.color = new Color(c.r, c.g, c.b, lit ? 0.45f : 0f);
        }

        void SetPhotoAlpha(float a)
        {
            if (photoArts == null) return;
            foreach (SpriteRenderer sr in photoArts) HalloweenKit.SetAlpha(sr, a);
        }

        IEnumerator ShakeReaction()
        {
            busy = true;
            Haptics.Medium();
            Sfx.Play(this, "clunk", 0.7f);
            toast.Show("VIOLENCE IS NOT A PASSWORD.", 1.8f);
            yield return HalloweenKit.Shake(phone, 0.12f, 0.45f);
            // il telefono cade un po' di lato, poi si rimette dritto
            Quaternion side = phoneRot * Quaternion.Euler(0f, 0f, -7f);
            for (float t = 0f; t < 1f; t += Time.deltaTime / 0.18f)
            {
                phone.localRotation = Quaternion.Slerp(phoneRot, side, t);
                phone.localPosition = phoneHome + new Vector3(0.12f * t, -0.05f * t, 0f);
                yield return null;
            }
            Sfx.Play(this, "bonk", 0.6f);
            yield return new WaitForSeconds(1f);
            for (float t = 0f; t < 1f; t += Time.deltaTime / 0.3f)
            {
                phone.localRotation = Quaternion.Slerp(side, phoneRot, t);
                phone.localPosition = phoneHome + new Vector3(0.12f * (1f - t), -0.05f * (1f - t), 0f);
                yield return null;
            }
            phone.localRotation = phoneRot;
            phone.localPosition = phoneHome;
            busy = false;
        }

        IEnumerator Unlock()
        {
            celebrating = true;
            photoFingers.Clear();
            pushFingers.Clear();
            ShowLed();
            V64ASound.Play(this, "unlock", 0.8f);
            Haptics.Light();
            yield return new WaitForSeconds(0.35f);
            HalloweenKit.Show(lockScreen, false);
            HalloweenKit.Show(unlockScreen, true);
            unlockText.text = "FACE ID ENABLED";
            StartCoroutine(HalloweenKit.Move(photo, photoHome, 0.35f, 0.3f));
            SetPhotoAlpha(1f);
            yield return new WaitForSeconds(0.6f);
            // il faccione che ti fissa
            HalloweenKit.Show(bigFace, true);
            Vector3 s = bigFace.localScale;
            bigFace.localScale = s * 0.1f;
            Sfx.Play(this, "pop", 0.9f);
            Haptics.Medium();
            yield return HalloweenKit.ScaleTo(bigFace, s * 1.08f, 0.18f);
            yield return HalloweenKit.ScaleTo(bigFace, s, 0.1f);
            yield return new WaitForSeconds(0.5f);
            toast.Show("SECURITY LEVEL: ABSOLUTELY TERRIBLE.", 2.5f);
            yield return Celebrate(1.8f);
            if (logic.TryComplete()) Win();
        }
    }
}
