using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels.V64;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 264 "The Camera-Shy Bird" — "Take a photo of the bird." Lo schermo è l'app della fotocamera: un dito
    /// trascina l'inquadratura sul parco, due dita (o i pulsanti -/+, o la pillola 1x/2x) fanno lo zoom, c'è il flash, il
    /// pulsante di scatto e l'icona per girare la fotocamera (sembra decorativa). L'uccellino odia essere il soggetto:
    /// inquadrato al centro fissa l'obiettivo e scappa tra le foglie (anche avvicinandosi piano), con lo zoom si copre la
    /// faccia ("No paparazzi."), col flash si mette gli occhialini da sole e la foto è solo riflesso, scattando mentre è in
    /// campo schizza via, e una foto senza di lui è "Lovely branch.". Indizio: quando la fotocamera guarda altrove
    /// l'uccellino la segue saltellando e sbircia curioso dal bordo. Soluzione: girare la fotocamera (selfie):
    /// l'uccellino si posa sulla testa dell'avatar e fa la V; scattare mentre è in posa (resta qualche secondo, torna se lo
    /// si manca). Niente microfono né sensori. La logica è in Gotcha.Levels.V64.CameraShyBirdLogic (senza Unity): qui solo
    /// input e presentazione. Tasti di prova nell'Editor: frecce = inquadrare, Z/X = zoom, L = flash, F = gira la
    /// fotocamera, Spazio = scatto, Shift + clic = secondo dito (pizzico).
    /// </summary>
    public class V68CameraShyBirdLevel : V62Level
    {
        [Header("Mirino")]
        [SerializeField] Transform views;               // si schiaccia in x quando si gira la fotocamera
        [SerializeField] Transform park;                // il panorama (posizione e scala = inquadratura e zoom)
        [SerializeField] Transform selfieView;
        [SerializeField] Transform[] landmarks;         // per la didascalia delle foto senza uccellino ("Lovely bench.")
        [SerializeField] Collider2D viewArea;
        [Header("Uccellino (fotocamera posteriore)")]
        [SerializeField] Transform bird;                // perno ai piedi, figlio del panorama
        [SerializeField] Transform birdPose;            // si gira verso l'obiettivo (scala x) e si inclina
        [SerializeField] SpriteRenderer birdArt;
        [SerializeField] SpriteRenderer birdShades;
        [SerializeField] Transform birdLeaves;
        [SerializeField] TextMesh birdMark;
        [SerializeField] Sprite peekSprite;
        [SerializeField] Sprite stareSprite;
        [SerializeField] Sprite hideSprite;
        [SerializeField] Sprite flySprite;
        [SerializeField] Sprite peaceSprite;
        [SerializeField] SpeechBubble birdBubble;
        [Header("Selfie")]
        [SerializeField] Transform avatar;
        [SerializeField] SpriteRenderer avatarBlink;
        [SerializeField] Transform selfieBird;
        [SerializeField] Transform selfieBirdPose;
        [SerializeField] SpriteRenderer selfieBirdArt;
        [SerializeField] SpriteRenderer selfieShades;
        [SerializeField] Transform selfiePerch;
        [SerializeField] Transform selfieFrom;
        [SerializeField] Transform selfieTo;
        [Header("Effetti e foto")]
        [SerializeField] Transform grid;                // griglia dei terzi del mirino (non finisce nella foto)
        [SerializeField] Transform focusBox;
        [SerializeField] Transform speedLines;
        [SerializeField] SpriteRenderer glare;
        [SerializeField] SpriteRenderer shutterFlash;
        [SerializeField] Transform photoFrame;
        [SerializeField] TextMesh photoCaption;
        [Header("App della fotocamera")]
        [SerializeField] Transform flashButton;
        [SerializeField] SpriteRenderer flashIcon;
        [SerializeField] SpriteRenderer flashSlash;
        [SerializeField] Collider2D flashArea;
        [SerializeField] TextMesh zoomText;
        [SerializeField] Collider2D zoomPillArea;
        [SerializeField] Collider2D zoomOutArea;
        [SerializeField] Collider2D zoomInArea;
        [SerializeField] Transform shutterButton;
        [SerializeField] Collider2D shutterArea;
        [SerializeField] Transform flipButton;
        [SerializeField] Collider2D flipArea;
        [SerializeField] Transform thumb;
        [SerializeField] SpriteRenderer thumbFill;
        [SerializeField] Collider2D thumbArea;
        [SerializeField] TextMesh infoText;
        [SerializeField] Transform infoHolder;

        const float TapTime = 0.4f, TapSlop = 0.2f;
        const float PhotoHold = 1.35f;
        const float FlipHalf = 0.13f;
        const float FocusTapRadius = 0.75f;   // tocco per mettere a fuoco: quanto vicino all'uccellino (mondo)
        const float LandmarkReach = 1.4f;     // didascalia: punto di riferimento entro questa distanza dal centro (a zoom 1)

        static readonly string[] NoticeLines = { "It saw the lens.", "Spotted. By the bird.", "It does NOT want to be the subject." };
        static readonly string[] SneakLines = { "Sneaking won't help. It watches the LENS, not you.", "Slowly... nope. It's watching the lens." };
        static readonly string[] FleeLines = { "...and it's gone. Behind the leaves.", "Camera-shy. Extremely.", "Gone again." };
        static readonly string[] CoverLines = { "No paparazzi.", "No photos, please!", "Talk to my agent." };
        static readonly string[] DodgeLines = { "It heard the click.", "A blurry blue streak. Very artistic.", "Too slow. It's a professional." };
        static readonly string[] WingLines = { "A lovely wing.", "Still a wing." };
        static readonly string[] AloneLines = { "Nice selfie. No bird.", "Just you. Again.", "Wait for your feathered friend..." };
        static readonly string[] ThumbLines = { "Gallery: {0} photos. Birds: 0.", "Gallery: {0} photos. Still no birds." };

        enum Kind { None, View }

        sealed class Grip { public Kind kind; public Vector2 start, last; public float t0; public bool moved; }

        readonly CameraShyBirdLogic logic = new CameraShyBirdLogic();
        readonly Dictionary<int, Grip> grips = new Dictionary<int, Grip>();
        readonly List<int> gone = new List<int>(4);
        Camera cam;
        HwToast toast;
        HwSay birdSay;
        readonly Dictionary<Transform, Coroutine> bumps = new Dictionary<Transform, Coroutine>();
        readonly Dictionary<Transform, Vector3> bumpHomes = new Dictionary<Transform, Vector3>();
        Vector3 parkHome, birdPoseHome, selfieBirdScale, birdBubbleHome, frameHome, frameScale;
        Color flashIconColor;
        float pinchDist, pinchZoom, panSpeed, lastPanAt = -10f, markUntil, focusUntil;
        int noticeLines, sneakLines, fleeLines, coverLines, dodgeLines, wingLines, aloneLines, thumbLines;
        bool busy, finishing, followHintShown, missHintShown;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            logic.Reset();
            ResetV62();
            V64AFingers.Reset();
            grips.Clear();
            busy = finishing = false;
            followHintShown = missHintShown = false;
            noticeLines = sneakLines = fleeLines = coverLines = dodgeLines = wingLines = aloneLines = thumbLines = 0;
            pinchDist = 0f;
            panSpeed = 0f;
            lastPanAt = -10f;
            markUntil = focusUntil = 0f;
            toast = new HwToast(infoText, infoHolder);
            birdSay = new HwSay(birdBubble);
            parkHome = park.localPosition;
            birdPoseHome = birdPose.localPosition;
            selfieBirdScale = selfieBird.localScale;
            frameHome = photoFrame.position;
            frameScale = photoFrame.localScale;
            RestoreBumps();
            if (birdBubble != null) birdBubbleHome = birdBubble.transform.position;
            flashIconColor = flashIcon.color;
            HalloweenKit.Show(avatarBlink, false);
            HalloweenKit.Show(birdMark, false);
            HalloweenKit.Show(focusBox, false);
            HalloweenKit.Show(speedLines, false);
            HalloweenKit.Show(photoFrame, false);
            HalloweenKit.Show(grid, true);
            HalloweenKit.SetAlpha(glare, 0f);
            HalloweenKit.SetAlpha(shutterFlash, 0f);
            HalloweenKit.Show(glare, false);
            HalloweenKit.Show(shutterFlash, false);
            flipButton.localRotation = Quaternion.identity;
            ShowAll();
        }

        void OnDisable()
        {
            CancelGrips();
            // le animazioni si fermano con l'oggetto: niente scatto o giro a metà al ritorno (lo stato viene dalla logica)
            busy = false;
            if (views != null) views.localScale = Vector3.one;
            RestoreBumps();
            if (photoFrame != null && frameScale != Vector3.zero) { photoFrame.position = frameHome; photoFrame.localScale = frameScale; }
            if (photoFrame != null) HalloweenKit.Show(photoFrame, false);
            if (grid != null) HalloweenKit.Show(grid, true);
            if (speedLines != null) HalloweenKit.Show(speedLines, false);
            if (glare != null) HalloweenKit.Show(glare, false);
            if (shutterFlash != null) HalloweenKit.Show(shutterFlash, false);
            if (flipButton != null) flipButton.localRotation = Quaternion.identity;
            birdSay?.Hide();
        }

        void OnEnable()
        {
            // se il livello è stato spento a metà della polaroid finale, la si riprende (vittoria comunque una volta sola)
            if (logic.Stage == ShyStage.Snapped && !IsFinished && cam != null) StartCoroutine(Photobomb());
        }
        void OnApplicationPause(bool paused) { if (paused) CancelGrips(); }
        void OnApplicationFocus(bool focus) { if (!focus) CancelGrips(); }

        /// <summary>Pausa, perdita del focus, pubblicità: si lasciano le dita (inquadratura e zoom restano dove sono).</summary>
        void CancelGrips()
        {
            grips.Clear();
            pinchDist = 0f;
        }

        void Update()
        {
            toast?.Tick();
            birdSay?.Tick();
            if (IsFinished || finishing || logic.InputLocked) return;
            if (Time.timeScale <= 0f) { CancelGrips(); return; }   // pausa o pubblicità: niente input
            if (busy) { ShowAll(); return; }                         // foto o giro in corso: tutto fermo (la foto è "congelata")
#if UNITY_EDITOR
            EditorKeys();
            if (busy || logic.InputLocked) return;
#endif
            ReadFingers();
            if (busy || logic.InputLocked) return;
            panSpeed = Mathf.Lerp(panSpeed, 0f, Mathf.Clamp01(Time.deltaTime * 3f));
            OnEvent(logic.Tick(Time.deltaTime));
            ShowAll();
        }

        // ---------- Dita ----------

        void ReadFingers()
        {
            List<V64AFinger> fingers = V64AFingers.Read(cam);
            foreach (V64AFinger f in fingers)
            {
                if (f.began) grips[f.id] = Begin(f.world);
                if (busy) return;
                if (!grips.TryGetValue(f.id, out Grip g)) continue;
                if (Vector2.Distance(f.world, g.start) > TapSlop) g.moved = true;
                if (f.ended) { End(f.id, g, f.world); continue; }
            }
            // dita perse senza "fine" (tocco annullato)
            if (fingers.Count == 0 && grips.Count > 0)
            {
                gone.Clear();
                foreach (int id in grips.Keys) gone.Add(id);
                foreach (int id in gone) grips.Remove(id);
                pinchDist = 0f;
            }
            // mirino: un dito inquadra, due dita fanno lo zoom
            int views0 = 0;
            Vector2 a = Vector2.zero, b = Vector2.zero;
            int idA = 0;
            foreach (V64AFinger f in fingers)
            {
                if (f.ended || !grips.TryGetValue(f.id, out Grip g) || g.kind != Kind.View) continue;
                if (views0 == 0) { a = f.world; idA = f.id; } else b = f.world;
                views0++;
            }
            if (views0 >= 2)
            {
                float d = Vector2.Distance(a, b);
                if (pinchDist <= 0.01f) { pinchDist = Mathf.Max(0.2f, d); pinchZoom = logic.Zoom; }
                else if (logic.Facing == ShyFacing.Rear) logic.SetZoom(pinchZoom * d / pinchDist);
                else if (Mathf.Abs(d - pinchDist) > 0.6f) SelfieNoZoom();
            }
            else
            {
                pinchDist = 0f;
                if (views0 == 1)
                {
                    Grip g = grips[idA];
                    Vector2 delta = a - g.last;
                    if (logic.Facing == ShyFacing.Rear && Mathf.Abs(delta.x) > 0.0001f)
                    {
                        float before = logic.Pan;
                        if (logic.PanBy(-delta.x / logic.Zoom))
                        {
                            float dt = Mathf.Max(0.001f, Time.deltaTime);
                            panSpeed = Mathf.Lerp(panSpeed, Mathf.Abs(logic.Pan - before) / dt, 0.25f);
                            lastPanAt = Time.time;
                        }
                    }
                }
            }
            foreach (V64AFinger f in fingers)
                if (grips.TryGetValue(f.id, out Grip g)) g.last = f.world;
        }

        Grip Begin(Vector2 w)
        {
            var g = new Grip { start = w, last = w, t0 = Time.time, kind = Kind.None };
            // prima i pulsanti dell'app (agiscono subito), poi il mirino
            if (shutterArea.OverlapPoint(w)) { PressShutter(); return g; }
            if (flipArea.OverlapPoint(w)) { PressFlip(); return g; }
            if (flashArea.OverlapPoint(w)) { PressFlash(); return g; }
            if (zoomInArea.OverlapPoint(w)) { PressZoom(+1); return g; }
            if (zoomOutArea.OverlapPoint(w)) { PressZoom(-1); return g; }
            if (zoomPillArea.OverlapPoint(w)) { PressZoomPill(); return g; }
            if (thumbArea.OverlapPoint(w)) { PressThumb(); return g; }
            if (viewArea.OverlapPoint(w)) g.kind = Kind.View;
            return g;
        }

        void End(int id, Grip g, Vector2 w)
        {
            grips.Remove(id);
            pinchDist = 0f;
            if (g.kind == Kind.View && !g.moved && Time.time - g.t0 <= TapTime) TapView(w);
        }

        // ---------- I pulsanti ----------

        void PressShutter()
        {
            if (busy) return;
            ShyPhoto kind = logic.Shutter();
            if (kind == ShyPhoto.None) return;
            Bump(shutterButton, 0.85f, 0.15f);
            Haptics.Tap();
            CancelGrips();
            if (kind == ShyPhoto.Photobomb) { StartCoroutine(Photobomb()); return; }
            StartCoroutine(Photo(kind));
        }

        void PressFlip()
        {
            if (busy || logic.InputLocked) return;
            Haptics.Tap();
            CancelGrips();
            StartCoroutine(Flip());
        }

        void PressFlash()
        {
            if (busy || logic.InputLocked) return;
            bool on = logic.ToggleFlash();
            Haptics.Tap();
            V68ShyBirdSound.Play(this, "click", 0.5f);
            Bump(flashButton, 1.2f, 0.18f);
            bool birdSees = logic.Facing == ShyFacing.Rear ? logic.BirdInFrame && logic.Mood != BirdMood.Covering
                                                            : logic.Selfie == SelfieBird.Posing;
            if (on && birdSees)
            {
                Sfx.Play(this, "pop", 0.5f);
                Bump(logic.Facing == ShyFacing.Rear ? birdShades.transform : selfieShades.transform, 1.5f, 0.25f);
                toast.Show("The bird put on tiny sunglasses.", 1.8f);
            }
            else toast.Show(on ? "Flash: ON." : "Flash: OFF.", 1.1f);
            ShowAll();
        }

        void PressZoom(int dir)
        {
            if (busy || logic.InputLocked) return;
            if (logic.Facing == ShyFacing.Selfie) { SelfieNoZoom(); return; }
            if (logic.ZoomStep(dir)) { Haptics.Tap(); V68ShyBirdSound.Play(this, "click", 0.3f); }
            ShowAll();
        }

        /// <summary>La pillola dello zoom, come nelle app vere: 1x ↔ 2x.</summary>
        void PressZoomPill()
        {
            if (busy || logic.InputLocked) return;
            if (logic.Facing == ShyFacing.Selfie) { SelfieNoZoom(); return; }
            if (logic.SetZoom(logic.Zoom < 1.5f ? 2f : 1f)) { Haptics.Tap(); V68ShyBirdSound.Play(this, "click", 0.3f); }
            ShowAll();
        }

        void SelfieNoZoom()
        {
            pinchDist = 0f;
            toast.Show("Selfie camera: 1x only.", 1.2f);
        }

        void PressThumb()
        {
            if (busy) return;
            Bump(thumb, 1.15f, 0.2f);
            if (logic.Photos == 0) { toast.Show("No photos yet.", 1.2f); return; }
            toast.Show(string.Format(ThumbLines[thumbLines++ % ThumbLines.Length], logic.Photos), 1.6f);
        }

        /// <summary>Tocco sul mirino: il riquadro giallo della messa a fuoco; se è sull'uccellino, lui se ne accorge.</summary>
        void TapView(Vector2 w)
        {
            if (busy) return;
            focusBox.position = new Vector3(w.x, w.y, focusBox.position.z);
            focusUntil = Time.time + 0.6f;
            V68ShyBirdSound.Play(this, "click", 0.2f);
            if (logic.Facing != ShyFacing.Rear) return;
            if (Vector2.Distance(w, birdPose.position) <= FocusTapRadius * Mathf.Max(1f, logic.Zoom * 0.7f) && logic.FocusOnBird())
                OnEvent(ShyEvent.Noticed);
        }

        /// <summary>Rimbalzo di scala che non si accumula: uno nuovo riparte dalla misura di riposo.</summary>
        void Bump(Transform t, float peak, float seconds)
        {
            if (t == null || !isActiveAndEnabled) return;
            if (!bumpHomes.TryGetValue(t, out Vector3 home)) { home = t.localScale; bumpHomes[t] = home; }
            if (bumps.TryGetValue(t, out Coroutine c) && c != null) StopCoroutine(c);
            t.localScale = home;
            bumps[t] = StartCoroutine(BumpRun(t, home, peak, seconds));
        }

        IEnumerator BumpRun(Transform t, Vector3 home, float peak, float seconds)
        {
            for (float k = 0f; k < 1f; k += Time.deltaTime / seconds)
            {
                t.localScale = home * (1f + (peak - 1f) * Mathf.Sin(k * Mathf.PI));
                yield return null;
            }
            t.localScale = home;
            bumps.Remove(t);
        }

        void RestoreBumps()
        {
            foreach (KeyValuePair<Transform, Vector3> kv in bumpHomes)
                if (kv.Key != null) kv.Key.localScale = kv.Value;
            bumps.Clear();
        }

        // ---------- Reazioni dell'uccellino ----------

        void OnEvent(ShyEvent e)
        {
            switch (e)
            {
                case ShyEvent.Followed:
                    V68ShyBirdSound.Play(this, "flutter", 0.35f);
                    markUntil = Time.time + 1.3f;
                    birdMark.text = "?";
                    if (logic.Follows >= 2 && !followHintShown)
                    {
                        followHintShown = true;
                        toast.Show("Is it... following the camera?", 1.8f);
                    }
                    break;
                case ShyEvent.Landed:
                case ShyEvent.Reappeared:
                    markUntil = Time.time + 0.8f;
                    birdMark.text = "?";
                    if (e == ShyEvent.Reappeared) V68ShyBirdSound.Play(this, "tweet", 0.35f);
                    break;
                case ShyEvent.Noticed:
                    V68ShyBirdSound.Play(this, "alarm", 0.6f);
                    birdMark.text = "!";
                    markUntil = Time.time + logic.StareSeconds + 0.1f;
                    focusUntil = Time.time + logic.StareSeconds;
                    bool sneaky = Time.time - lastPanAt < 1f && panSpeed < 0.9f;
                    toast.Show(sneaky ? SneakLines[sneakLines++ % SneakLines.Length] : NoticeLines[noticeLines++ % NoticeLines.Length], 1.8f);
                    break;
                case ShyEvent.Fled:
                    V68ShyBirdSound.Play(this, "flutter", 0.6f);
                    markUntil = 0f;
                    toast.Show(FleeLines[fleeLines++ % FleeLines.Length], 1.6f);
                    break;
                case ShyEvent.Hid:
                    Sfx.Play(this, "whoosh", 0.25f);
                    break;
                case ShyEvent.CoveredFace:
                    V68ShyBirdSound.Play(this, "alarm", 0.4f);
                    markUntil = 0f;
                    focusUntil = Time.time + 1.2f;
                    Say(CoverLines[coverLines++ % CoverLines.Length], birdPose.position);
                    break;
                case ShyEvent.SelfieArrive:
                    V68ShyBirdSound.Play(this, "flutter", 0.55f);
                    break;
                case ShyEvent.SelfieLand:
                    V68ShyBirdSound.Play(this, "cheese", 0.6f);
                    Bump(selfieBirdPose, 1.15f, 0.25f);
                    Say(logic.Arrivals <= 1 ? "CHEESE!" : "Again? CHEESE!", selfiePerch.position);
                    break;
                case ShyEvent.SelfieLeave:
                    V68ShyBirdSound.Play(this, "flutter", 0.45f);
                    birdSay.Hide();
                    if (!missHintShown) { missHintShown = true; toast.Show("Missed it. Don't worry, it'll be back.", 1.8f); }
                    break;
            }
        }

        void Say(string line, Vector3 near)
        {
            if (birdBubble == null) return;
            float x = Mathf.Clamp(near.x, -1.4f, 1.4f), y = Mathf.Min(near.y + 1.0f, 2.35f);
            birdBubble.transform.position = new Vector3(x, y, birdBubbleHome.z);
            birdSay.Show(line, 1.6f);
        }

        // ---------- Presentazione ----------

        void ShowAll()
        {
            bool rear = logic.Facing == ShyFacing.Rear;
            HalloweenKit.Show(park, rear);
            HalloweenKit.Show(selfieView, !rear);
            // inquadratura: il panorama si sposta e si ingrandisce attorno al centro del mirino
            float z = logic.Zoom;
            park.localPosition = new Vector3(parkHome.x - logic.Pan * z, parkHome.y, parkHome.z);
            park.localScale = new Vector3(z, z, 1f);
            if (zoomText != null) zoomText.text = (rear ? z : 1f).ToString("0.#", System.Globalization.CultureInfo.InvariantCulture) + "x";
            HalloweenKit.Show(flashSlash, !logic.FlashOn);
            flashIcon.color = logic.FlashOn ? new Color(1f, 0.85f, 0.3f, flashIconColor.a) : flashIconColor;
            if (rear) ShowRearBird(); else ShowSelfieBird();
            bool focusOn = Time.time < focusUntil;
            if (focusOn && rear && (logic.Mood == BirdMood.Staring || logic.Mood == BirdMood.Covering))
                focusBox.position = new Vector3(birdPose.position.x, birdPose.position.y, focusBox.position.z);
            HalloweenKit.Show(focusBox, focusOn);
        }

        void ShowRearBird()
        {
            BirdMood m = logic.Mood;
            float y = 0f;
            if (m == BirdMood.Hopping) y = 0.35f * Mathf.Sin(logic.TravelProgress * Mathf.PI);
            else if (m == BirdMood.Fleeing) y = 0.9f * Mathf.Sin(logic.TravelProgress * Mathf.PI) + 0.4f * logic.TravelProgress * (1f - logic.TravelProgress);
            bird.localPosition = new Vector3(logic.BirdX, y, bird.localPosition.z);

            Sprite s = peekSprite;
            if (m == BirdMood.Staring) s = stareSprite;
            else if (m == BirdMood.Covering) s = hideSprite;
            else if (m == BirdMood.Hopping || m == BirdMood.Fleeing) s = flySprite;
            if (birdArt.sprite != s) birdArt.sprite = s;

            // si gira verso l'obiettivo (il disegno guarda a destra); in volo guarda dove va
            bool faceLeft = (m == BirdMood.Hopping || m == BirdMood.Fleeing) ? logic.ToX < logic.FromX : logic.BirdX > logic.Pan;
            Vector3 ps = birdPose.localScale;
            birdPose.localScale = new Vector3(faceLeft ? -Mathf.Abs(ps.x) : Mathf.Abs(ps.x), ps.y, ps.z);
            birdPose.localPosition = birdPoseHome + (m == BirdMood.Hidden ? Vector3.down * 0.34f : Vector3.zero);
            HalloweenKit.Show(birdLeaves, m == BirdMood.Hidden);
            HalloweenKit.Show(birdShades, logic.FlashOn && m != BirdMood.Covering && m != BirdMood.Hidden);
            HalloweenKit.Show(birdMark, Time.time < markUntil && m != BirdMood.Hidden);
            if (busy) return;   // foto congelata: niente dondolii
            float tilt = 0f;
            if (m == BirdMood.Peeking) tilt = 8f * Mathf.Sin(Time.time * 3.1f) + 6f;   // testa inclinata, curioso
            else if (m == BirdMood.Staring) tilt = 3f * Mathf.Sin(Time.time * 40f);
            birdPose.localRotation = Quaternion.Euler(0f, 0f, faceLeft ? -tilt : tilt);
        }

        void ShowSelfieBird()
        {
            SelfieBird st = logic.Selfie;
            float p = logic.SelfieProgress;
            Vector3 perch = selfiePerch.position;
            bool visible = st != SelfieBird.Away;
            HalloweenKit.Show(selfieBird, visible);
            if (!visible) return;
            Vector3 pos;
            bool faceLeft = false;
            if (st == SelfieBird.Arriving)
            {
                pos = Vector3.Lerp(selfieFrom.position, perch, 1f - (1f - p) * (1f - p)) + Vector3.up * (0.6f * Mathf.Sin(p * Mathf.PI));
                faceLeft = true;
            }
            else if (st == SelfieBird.Leaving)
            {
                pos = Vector3.Lerp(perch, selfieTo.position, p * p) + Vector3.up * (0.5f * Mathf.Sin(p * Mathf.PI));
                faceLeft = true;
            }
            else pos = perch + (busy ? Vector3.zero : Vector3.up * (0.04f * Mathf.Abs(Mathf.Sin(Time.time * 4f))));   // in posa: saltella sul posto
            selfieBird.position = new Vector3(pos.x, pos.y, selfieBird.position.z);
            Sprite s = st == SelfieBird.Posing ? peaceSprite : flySprite;
            if (selfieBirdArt.sprite != s) selfieBirdArt.sprite = s;
            selfieBird.localScale = new Vector3(faceLeft ? -Mathf.Abs(selfieBirdScale.x) : Mathf.Abs(selfieBirdScale.x), selfieBirdScale.y, selfieBirdScale.z);
            HalloweenKit.Show(selfieShades, logic.FlashOn);
        }

        // ---------- Foto e giro della fotocamera ----------

        IEnumerator Photo(ShyPhoto kind)
        {
            busy = true;
            V68ShyBirdSound.Play(this, "shutter", 0.8f);
            ShowAll();
            // cosa si vede nella foto (congelata): scie se l'uccellino scappa, riflesso col flash
            bool streak = kind == ShyPhoto.Dodged;
            if (streak)
            {
                speedLines.position = new Vector3(birdPose.position.x + (logic.ToX < logic.FromX ? 0.55f : -0.55f), birdPose.position.y, speedLines.position.z);
                Vector3 ls = speedLines.localScale;
                speedLines.localScale = new Vector3(logic.ToX < logic.FromX ? Mathf.Abs(ls.x) : -Mathf.Abs(ls.x), ls.y, ls.z);
            }
            HalloweenKit.Show(speedLines, streak);
            bool flashed = kind == ShyPhoto.Glare;
            HalloweenKit.Show(glare, flashed);
            HalloweenKit.SetAlpha(glare, flashed ? 0.97f : 0f);
            photoCaption.text = Caption(kind);
            yield return ShutterBlink(flashed);
            HalloweenKit.Show(grid, false);
            HalloweenKit.Show(photoFrame, true);
            Vector3 fs = frameScale;
            photoFrame.localScale = fs * 0.85f;
            yield return HalloweenKit.ScaleTo(photoFrame, fs, 0.15f);
            yield return new WaitForSeconds(PhotoHold);
            // la foto vola nell'anteprima
            Vector3 from = frameHome;
            for (float t = 0f; t < 1f; t += Time.deltaTime / 0.22f)
            {
                photoFrame.position = Vector3.Lerp(from, thumb.position, t);
                photoFrame.localScale = Vector3.Lerp(fs, fs * 0.15f, t);
                yield return null;
            }
            photoFrame.position = from;
            photoFrame.localScale = fs;
            HalloweenKit.Show(photoFrame, false);
            HalloweenKit.Show(grid, true);
            HalloweenKit.Show(speedLines, false);
            HalloweenKit.Show(glare, false);
            thumbFill.color = flashed ? Color.white : new Color(0.62f, 0.83f, 0.98f);
            Bump(thumb, 1.15f, 0.2f);
            busy = false;
        }

        IEnumerator ShutterBlink(bool flashed)
        {
            HalloweenKit.Show(shutterFlash, true);
            HalloweenKit.SetAlpha(shutterFlash, flashed ? 1f : 0.85f);
            yield return HalloweenKit.Fade(shutterFlash, 0f, flashed ? 0.35f : 0.18f);
            HalloweenKit.Show(shutterFlash, false);
        }

        string Caption(ShyPhoto kind)
        {
            switch (kind)
            {
                case ShyPhoto.Dodged: return DodgeLines[dodgeLines++ % DodgeLines.Length];
                case ShyPhoto.Wing: return WingLines[wingLines++ % WingLines.Length];
                case ShyPhoto.Glare:
                    if (logic.Facing == ShyFacing.Rear && logic.BirdInFrame) return "Flash glare. And tiny sunglasses.";
                    return "Flash glare. Very artistic.";
                case ShyPhoto.SelfieAlone: return AloneLines[aloneLines++ % AloneLines.Length];
                default: return "Lovely " + Subject() + ".";
            }
        }

        /// <summary>Cosa c'è al centro della foto senza uccellino: il punto di riferimento più vicino, altrimenti la siepe.</summary>
        string Subject()
        {
            string best = "hedge";
            float bestD = LandmarkReach / logic.Zoom;
            if (landmarks == null) return best;
            foreach (Transform t in landmarks)
            {
                if (t == null) continue;
                float d = Mathf.Abs(t.localPosition.x - logic.Pan);
                if (d < bestD) { bestD = d; best = t.name.ToLowerInvariant(); }
            }
            return best;
        }

        IEnumerator Flip()
        {
            busy = true;
            bool toSelfie = logic.Facing == ShyFacing.Rear;
            bool wasPosing = logic.Selfie == SelfieBird.Posing;
            Sfx.Play(this, "whoosh", 0.5f);
            Quaternion r0 = flipButton.localRotation;
            for (float t = 0f; t < 1f; t += Time.deltaTime / FlipHalf)
            {
                views.localScale = new Vector3(Mathf.Lerp(1f, 0.04f, t), 1f, 1f);
                flipButton.localRotation = r0 * Quaternion.Euler(0f, 0f, -90f * t);
                yield return null;
            }
            logic.Flip();
            birdSay.Hide();
            markUntil = focusUntil = 0f;
            ShowAll();
            for (float t = 0f; t < 1f; t += Time.deltaTime / FlipHalf)
            {
                views.localScale = new Vector3(Mathf.Lerp(0.04f, 1f, t), 1f, 1f);
                flipButton.localRotation = r0 * Quaternion.Euler(0f, 0f, -90f - 90f * t);
                yield return null;
            }
            views.localScale = Vector3.one;
            flipButton.localRotation = Quaternion.identity;
            if (!toSelfie && wasPosing) toast.Show("...and it's camera-shy again.", 1.6f);
            busy = false;
        }

        /// <summary>La polaroid finale: l'avatar e l'uccellino che fa la V. L'avatar ha chiuso gli occhi.</summary>
        IEnumerator Photobomb()
        {
            finishing = true;
            busy = true;
            CancelGrips();
            birdSay.Hide();
            V68ShyBirdSound.Play(this, "shutter", 0.9f);
            // la foto: l'uccellino in posa sulla testa (anche se lo scatto l'ha preso un attimo prima o dopo)
            HalloweenKit.Show(selfieView, true);
            HalloweenKit.Show(park, false);
            HalloweenKit.Show(selfieBird, true);
            selfieBird.position = new Vector3(selfiePerch.position.x, selfiePerch.position.y, selfieBird.position.z);
            selfieBird.localScale = new Vector3(Mathf.Abs(selfieBirdScale.x), selfieBirdScale.y, selfieBirdScale.z);
            selfieBirdArt.sprite = peaceSprite;
            HalloweenKit.Show(selfieShades, logic.FlashOn);
            HalloweenKit.Show(avatarBlink, true);
            photoCaption.text = "Me + my new friend";
            yield return ShutterBlink(logic.FlashOn);
            HalloweenKit.Show(grid, false);
            HalloweenKit.Show(focusBox, false);
            HalloweenKit.Show(photoFrame, true);
            photoFrame.position = frameHome;
            photoFrame.localScale = frameScale * 0.85f;
            yield return HalloweenKit.ScaleTo(photoFrame, frameScale, 0.15f);
            V68ShyBirdSound.Play(this, "cheese", 0.7f);
            toast.Show("THE BIRD IS IN THE PHOTO.", 1.4f);
            yield return new WaitForSeconds(1.3f);
            toast.Show("YOU, HOWEVER, BLINKED.", 1.8f);
            StartCoroutine(HalloweenKit.Wiggle(selfieBirdPose, 8f, 0.8f, 14f));
            Bump(avatar, 1.04f, 0.3f);
            yield return Celebrate(1.1f);
            if (logic.TryComplete()) Win();
        }

#if UNITY_EDITOR
        // ---------- Tasti di prova nell'Editor ----------

        void EditorKeys()
        {
            if (Input.GetKey(KeyCode.LeftArrow)) logic.PanBy(-3f * Time.deltaTime / logic.Zoom);
            if (Input.GetKey(KeyCode.RightArrow)) logic.PanBy(3f * Time.deltaTime / logic.Zoom);
            if (Input.GetKeyDown(KeyCode.Z)) PressZoom(+1);
            if (Input.GetKeyDown(KeyCode.X)) PressZoom(-1);
            if (Input.GetKeyDown(KeyCode.L)) PressFlash();
            if (Input.GetKeyDown(KeyCode.F)) PressFlip();
            else if (Input.GetKeyDown(KeyCode.Space)) PressShutter();
        }
#endif
    }

    /// <summary>Suoni sintetizzati del livello 264: scatto, clic delle icone, battito d'ali, cinguettii.</summary>
    public static class V68ShyBirdSound
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
                case "shutter":  // otturatore: "clic-clac"
                    c = Make(kind, 0.16f, t => N() * (Mathf.Exp(-t * 160f) + (t > 0.08f ? Mathf.Exp(-(t - 0.08f) * 120f) * 0.8f : 0f)) * 0.8f);
                    break;
                case "click":    // icona toccata
                    c = Make(kind, 0.04f, t => Mathf.Sin(t * 2f * Mathf.PI * 2200f) * Mathf.Exp(-t * 180f) * 0.5f);
                    break;
                case "flutter":  // battito d'ali: sbuffi di fruscio veloci
                    c = Make(kind, 0.45f, t => N() * 0.5f * Mathf.Max(0f, Mathf.Sin(t * 2f * Mathf.PI * 18f)) * Mathf.Min(1f, (0.45f - t) * 8f));
                    break;
                case "tweet":    // un cinguettio solo
                    c = Make(kind, 0.12f, t => Mathf.Sin(2f * Mathf.PI * (3000f + 9000f * t) * t) * Mathf.Sin(t / 0.12f * Mathf.PI) * 0.35f);
                    break;
                case "alarm":    // "!": due versi acuti e secchi
                    c = Make(kind, 0.22f, t =>
                    {
                        float k = t % 0.11f;
                        return Mathf.Sin(2f * Mathf.PI * (4200f - 8000f * k) * k) * Mathf.Exp(-k * 30f) * 0.4f;
                    });
                    break;
                case "cheese":   // trillo felice che sale
                    c = Make(kind, 0.5f, t => Mathf.Sin(2f * Mathf.PI * (2400f + 1800f * t + 300f * Mathf.Sin(t * 2f * Mathf.PI * 22f)) * t)
                        * Mathf.Sin(t / 0.5f * Mathf.PI) * 0.3f);
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
