using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels.V64;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 255 "The Invisible Password" — una porta blindata, una tastiera a quattro cifre, uno specchio "decorativo".
    /// I codici a caso danno frasi buffe (mai blocchi), "FORGOT PASSWORD" risponde "SO DID WE.", 0000 "NICE TRY.".
    /// Toccando la porta trema e al terzo colpo un cardine si allenta. Soluzione: un dito su ogni cardine, tirare in fuori
    /// insieme finché i perni cadono; la porta resta appesa alla serratura e, inclinando il telefono a destra, si apre
    /// mostrando il retro ("0000", falsa pista). Inclinando ancora il retro si riflette nello specchio: "8008". 8008 apre.
    /// Dietro la porta c'è uno sgabuzzino: "TOP SECRET: CLEANING SUPPLIES." e il mocio cade verso il giocatore.
    /// Logica in Gotcha.Levels.V64.InvisiblePasswordLogic (senza Unity): qui solo input e presentazione.
    /// </summary>
    public class V64InvisiblePasswordLevel : V62Level
    {
        [SerializeField] Transform door;                 // centro della porta chiusa
        [SerializeField] Transform doorFront;            // davanti (si stringe ruotando)
        [SerializeField] Transform doorBack;             // retro con "0000" (compare oltre i 90°)
        [SerializeField] SpriteRenderer doorFrontArt;
        [SerializeField] SpriteRenderer doorBackArt;
        [SerializeField] Collider2D doorArea;
        [SerializeField] float doorWidth = 2.1f;
        [SerializeField] Transform hingeTop, hingeBottom;
        [SerializeField] Collider2D hingeTopArea, hingeBottomArea;
        [SerializeField] Transform pinTop, pinBottom;
        [SerializeField] Transform mirror;
        [SerializeField] SpriteRenderer mirrorReflection;
        [SerializeField] Collider2D mirrorArea;
        [SerializeField] SpriteRenderer mirrorSparkle;
        [SerializeField] Collider2D cctvArea;
        [SerializeField] TextMesh cctvText;
        [SerializeField] Collider2D signArea;
        [SerializeField] SpriteRenderer statusLamp;
        [SerializeField] Transform[] keys;               // indice = cifra
        [SerializeField] Collider2D[] keyAreas;
        [SerializeField] Collider2D forgotArea;
        [SerializeField] Collider2D displayArea;
        [SerializeField] TextMesh display;
        [SerializeField] SpriteRenderer darkness;        // il buio dentro lo sgabuzzino
        [SerializeField] Transform mop;
        [SerializeField] SpriteRenderer mopArt;
        [SerializeField] float floorY = -2.75f;
        [SerializeField] TextMesh infoText;
        [SerializeField] Transform infoHolder;

        const int NoFinger = int.MinValue;
        const float PinSlide = 0.32f;    // quanto si vede uscire il perno (unità mondo) a fine corsa
        const float DoorSpeed = 170f;    // gradi al secondo dell'animazione verso l'angolo richiesto

        static readonly Color LampIdle = new Color(1f, 0.75f, 0.2f), LampRed = new Color(1f, 0.25f, 0.2f), LampGreen = new Color(0.35f, 0.95f, 0.4f);
        static readonly string[] CctvLines = { "CAM 1: A DOOR.", "CAM 2: STILL A DOOR.", "CAM 3: YOU. TAPPING A SCREEN." };
        static readonly string[] MirrorLines = { "Just a mirror. Purely decorative. Definitely.", "You look great. Still not the password." };

        readonly InvisiblePasswordLogic logic = new InvisiblePasswordLogic();
        Camera cam;
        HwToast toast;
        Vector3 frontScale, backScale, pinTopHome, pinBottomHome, hingeTopHome;
        Quaternion mopRot;
        float doorDeg, lampUntil, sway;
        int topFinger = NoFinger, bottomFinger = NoFinger, cctvTaps, mirrorTaps;
        Vector2 topStart, topNow, bottomStart, bottomNow;
        bool busy, pivotRight = true;
        readonly HashSet<int> alive = new HashSet<int>();

        public override void OnLevelStart()
        {
            cam = Camera.main;
            logic.Reset();
            ResetV62();
            toast = new HwToast(infoText, infoHolder);
            frontScale = doorFront.localScale;
            backScale = doorBack.localScale;
            pinTopHome = pinTop.localPosition;
            pinBottomHome = pinBottom.localPosition;
            hingeTopHome = hingeTop.localPosition;
            if (mop != null) mopRot = mop.rotation;
            doorDeg = 0f;
            pivotRight = true;
            SetDoor(0f);
            HalloweenKit.SetAlpha(mirrorReflection, 0f);
            HalloweenKit.Show(mirrorSparkle, false);
            HalloweenKit.SetAlpha(darkness, 0.94f);
            SetLamp(LampIdle);
            ShowEntry();
            if (cctvText != null) cctvText.text = "CAM 1  REC";
        }

        void Update()
        {
            toast.Tick();
            if (IsFinished || busy || logic.InputLocked) return;
            if (Time.timeScale <= 0f) { DropFingers(); return; }   // pausa o pubblicità: niente input né sensori
            if (lampUntil > 0f && Time.time >= lampUntil) { lampUntil = 0f; SetLamp(LampIdle); }
            ReadFingers();
            UpdatePins();
            if (busy) return;
            UpdateDoor();
        }

        // ---------- Dita ----------

        void ReadFingers()
        {
            alive.Clear();
            foreach (S41KKit.Finger f in S41KKit.Fingers(cam))
            {
                if (!f.ended) alive.Add(f.id);
                if (f.began)
                {
                    if (OverUi(f.id)) continue;
                    if (!logic.Detached && topFinger == NoFinger && f.id != bottomFinger && hingeTopArea.OverlapPoint(f.world))
                    { topFinger = f.id; topStart = topNow = f.world; Grab(hingeTop); continue; }
                    if (!logic.Detached && bottomFinger == NoFinger && f.id != topFinger && hingeBottomArea.OverlapPoint(f.world))
                    { bottomFinger = f.id; bottomStart = bottomNow = f.world; Grab(hingeBottom); continue; }
                    if (f.id != topFinger && f.id != bottomFinger) Tap(f.world);
                    continue;
                }
                if (f.id == topFinger) topNow = f.world;
                if (f.id == bottomFinger) bottomNow = f.world;
            }
            // dita sollevate o annullate (anche se il frame di "fine" si è perso)
            if (topFinger != NoFinger && !alive.Contains(topFinger)) topFinger = NoFinger;
            if (bottomFinger != NoFinger && !alive.Contains(bottomFinger)) bottomFinger = NoFinger;
        }

        void DropFingers() { topFinger = bottomFinger = NoFinger; }

        /// <summary>Un tocco che inizia sopra la UI (pausa, aiuti) non conta. Con più dita conta solo il primo.</summary>
        static bool OverUi(int fingerId)
        {
            if (Input.touchCount > 1 && Input.GetTouch(0).fingerId != fingerId) return false;
            return Gotcha.Core.PointerInput.IsOverUi();
        }

        void Grab(Transform hinge)
        {
            Haptics.Tap();
            Sfx.Play(this, "tick", 0.5f);
            StartCoroutine(HalloweenKit.Pop(hinge, 1.08f, 0.15f));
        }

        // ---------- Cardini ----------

        void UpdatePins()
        {
            if (logic.Detached) return;
            bool top = topFinger != NoFinger, bottom = bottomFinger != NoFinger;
            // "in fuori" = verso sinistra (fuori dalla porta) o lontano dall'altro cardine, il maggiore dei due
            float topPull = top ? Mathf.Max(topStart.x - topNow.x, topNow.y - topStart.y) : 0f;
            float bottomPull = bottom ? Mathf.Max(bottomStart.x - bottomNow.x, bottomStart.y - bottomNow.y) : 0f;
#if UNITY_EDITOR
            // Editor: Spazio = il secondo dito sull'altro cardine, che tira come il mouse
            if (Input.GetKey(KeyCode.Space))
            {
                if (top && !bottom) { bottom = true; bottomPull = topPull; }
                else if (bottom && !top) { top = true; topPull = bottomPull; }
            }
#endif
            PasswordReaction r = logic.UpdatePins(top, topPull, bottom, bottomPull);
            // i perni seguono: insieme escono davvero, uno solo si muove appena (e torna)
            float single = 0.08f;
            float vt = logic.PinProgress > 0f ? logic.PinProgress * PinSlide : (top ? Mathf.Clamp01(topPull / 0.5f) * single : 0f);
            float vb = logic.PinProgress > 0f ? logic.PinProgress * PinSlide : (bottom ? Mathf.Clamp01(bottomPull / 0.5f) * single : 0f);
            float loose = logic.HingeLoose ? 0.06f : 0f;
            pinTop.localPosition = pinTopHome + Vector3.up * Mathf.Max(vt, loose);
            pinBottom.localPosition = pinBottomHome + Vector3.down * vb;
            switch (r)
            {
                case PasswordReaction.Straining:
                    Squeak(520f, 0.25f);
                    toast.Show("One hinge at a time? It's a SECURITY door.", 1.8f);
                    StartCoroutine(HalloweenKit.Shake(top ? hingeTop : hingeBottom, 0.03f, 0.25f));
                    break;
                case PasswordReaction.Squeal:
                    Squeak(700f + logic.PinProgress * 500f, 0.35f);
                    Haptics.Light();
                    break;
                case PasswordReaction.PinsSlideBack:
                    Sfx.Play(this, "clunk", 0.4f);
                    break;
                case PasswordReaction.PinsOut:
                    StartCoroutine(PinsOut());
                    break;
            }
        }

        /// <summary>Cigolio metallico: due note acute stonate insieme.</summary>
        void Squeak(float hz, float volume)
        {
            S41KKit.Tone(this, hz, 0.25f, volume);
            S41KKit.Tone(this, hz * 1.07f, 0.2f, volume * 0.7f);
        }

        IEnumerator PinsOut()
        {
            busy = true;
            DropFingers();
            Squeak(1400f, 0.5f);
            Haptics.Medium();
            // i perni volano fuori e cadono a terra
            Vector3 a = pinTop.position, b = pinBottom.position;
            StartCoroutine(FallPin(pinTop, a + new Vector3(-0.35f, 0.35f, 0f), -0.45f));
            yield return FallPin(pinBottom, b + new Vector3(-0.35f, -0.1f, 0f), -0.85f);
            Sfx.Play(this, "clunk", 0.8f);
            yield return new WaitForSeconds(0.1f);
            Sfx.Play(this, "clunk", 0.6f);
            // la porta si siede un po' sulla serratura
            yield return HalloweenKit.Move(door, door.position + new Vector3(0f, -0.04f, 0f), 0.15f);
            toast.Show("The door is now held up by sheer willpower.", 2.4f);
            busy = false;
        }

        IEnumerator FallPin(Transform pin, Vector3 apex, float spinDir)
        {
            Vector3 from = pin.position;
            Vector3 land = new Vector3(apex.x - 0.15f, floorY - 0.05f, from.z);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.55f)
            {
                Vector3 p = k < 0.35f ? Vector3.Lerp(from, apex, k / 0.35f) : Vector3.Lerp(apex, land, (k - 0.35f) / 0.65f * ((k - 0.35f) / 0.65f));
                pin.position = p;
                pin.localRotation = Quaternion.Euler(0f, 0f, spinDir * 400f * k);
                yield return null;
            }
            pin.position = land;
            pin.localRotation = Quaternion.Euler(0f, 0f, 90f);
        }

        // ---------- Porta e inclinazione ----------

        void UpdateDoor()
        {
            PasswordReaction r = logic.UpdateTilt(Inputs.Tilt().x, Time.deltaTime);
            float target = logic.TargetDoorDeg;
            if (logic.Detached)
            {
                // porta appesa alla sola serratura: ondeggia appena (indizio che ora si muove)
                sway += Time.deltaTime;
                target += 2.5f * Mathf.Sin(sway * 2.1f) * (1f - Mathf.Clamp01(target / 30f));
            }
            float before = doorDeg;
            doorDeg = Mathf.MoveTowards(doorDeg, Mathf.Max(0f, target), DoorSpeed * Time.deltaTime);
            if (Mathf.FloorToInt(before / 45f) != Mathf.FloorToInt(doorDeg / 45f)) Sfx.Play(this, "clunk", 0.25f);   // cigolii
            SetDoor(doorDeg);
            HalloweenKit.SetAlpha(mirrorReflection, Mathf.Clamp01((doorDeg - 118f) / 22f));
            if (r == PasswordReaction.RearRevealed)
            {
                Sfx.Play(this, "pop", 0.5f);
                toast.Show("Something's written on the back...", 2f);
            }
            else if (r == PasswordReaction.MirrorRevealed) StartCoroutine(MirrorGlint());
        }

        /// <summary>
        /// Porta ruotata di deg gradi attorno al bordo destro (serratura) o sinistro (cardini): il davanti si stringe
        /// fino a 90°, poi compare il retro dall'altra parte del perno. Niente 3D: solo scale e spostamenti.
        /// </summary>
        void SetDoor(float deg)
        {
            float c = Mathf.Cos(deg * Mathf.Deg2Rad);
            float s = pivotRight ? 1f : -1f, half = doorWidth * 0.5f;
            bool front = c > 0.001f;
            doorFront.gameObject.SetActive(front);
            doorBack.gameObject.SetActive(!front);
            if (front)
            {
                doorFront.localScale = new Vector3(frontScale.x * c, frontScale.y, frontScale.z);
                doorFront.localPosition = new Vector3(s * half * (1f - c), 0f, 0f);
                float shade = Mathf.Lerp(0.62f, 1f, c);
                doorFrontArt.color = new Color(shade, shade, shade, 1f);
            }
            else
            {
                doorBack.localScale = new Vector3(backScale.x * -c, backScale.y, backScale.z);
                doorBack.localPosition = new Vector3(s * half * (1f - c), 0f, 0f);
                float shade = Mathf.Lerp(0.62f, 1f, -c);
                doorBackArt.color = new Color(shade, shade, shade, 1f);
            }
        }

        IEnumerator MirrorGlint()
        {
            Sfx.Play(this, "ding", 0.45f);
            Haptics.Light();
            if (mirrorSparkle != null)
            {
                HalloweenKit.Show(mirrorSparkle, true);
                Transform t = mirrorSparkle.transform;
                Vector3 s = t.localScale;
                for (float k = 0f; k < 1f; k += Time.deltaTime / 0.7f)
                {
                    t.localScale = s * Mathf.Sin(k * Mathf.PI) * 1.4f;
                    t.localRotation = Quaternion.Euler(0f, 0f, k * 180f);
                    yield return null;
                }
                t.localScale = s;
                HalloweenKit.Show(mirrorSparkle, false);
            }
            yield return HalloweenKit.Pop(mirror, 1.06f, 0.3f);
        }

        // ---------- Tocchi ----------

        void Tap(Vector2 w)
        {
            for (int d = 0; d < keyAreas.Length; d++)
                if (keyAreas[d] != null && keyAreas[d].OverlapPoint(w)) { PressKey(d); return; }
            if (forgotArea != null && forgotArea.OverlapPoint(w))
            {
                if (logic.ForgotPassword() == PasswordReaction.SoDidWe)
                {
                    Sfx.Play(this, "buzz", 0.35f);
                    StartCoroutine(HalloweenKit.Pop(forgotArea.transform, 1.1f, 0.2f));
                    toast.Show(logic.LastMessage, 2f);
                }
                return;
            }
            if (displayArea != null && displayArea.OverlapPoint(w))
            {
                if (logic.Clear() == PasswordReaction.Cleared) { Sfx.Play(this, "tick", 0.6f); ShowEntry(); }
                return;
            }
            if (mirrorArea != null && mirrorArea.OverlapPoint(w))
            {
                Sfx.Play(this, "tick", 0.5f);
                StartCoroutine(HalloweenKit.Wiggle(mirror, 3f, 0.4f, 30f));
                toast.Show(doorDeg > 125f ? "The mirror sees things differently." : MirrorLines[mirrorTaps++ % MirrorLines.Length], 2f);
                return;
            }
            if (cctvArea != null && cctvArea.OverlapPoint(w))
            {
                Sfx.Play(this, "tick", 0.5f);
                if (cctvText != null) cctvText.text = "CAM " + (cctvTaps % CctvLines.Length + 1) + "  REC";
                toast.Show(CctvLines[cctvTaps % CctvLines.Length], 1.8f);
                cctvTaps++;
                return;
            }
            if (signArea != null && signArea.OverlapPoint(w))
            {
                Sfx.Play(this, "tick", 0.5f);
                StartCoroutine(HalloweenKit.Wiggle(signArea.transform, 4f, 0.5f, 25f));
                toast.Show("You are not authorised. Neither are we.", 2f);
                return;
            }
            if (doorArea != null && doorArea.OverlapPoint(w)) KnockDoor();
        }

        void KnockDoor()
        {
            if (logic.Detached && doorDeg > 20f)
            {
                Sfx.Play(this, "tick", 0.4f);
                toast.Show("It's dark in there. Suspiciously dark.", 1.6f);
                return;
            }
            PasswordReaction r = logic.TapDoor();
            Sfx.Play(this, "bonk", 0.7f);
            Haptics.Light();
            StartCoroutine(HalloweenKit.Shake(door, 0.05f, 0.3f));
            if (r == PasswordReaction.HingeLoosens)
            {
                Squeak(600f, 0.4f);
                hingeTop.localPosition = hingeTopHome;
                hingeTop.localRotation = Quaternion.Euler(0f, 0f, 6f);
                toast.Show("*CREAK* ...something up there moved.", 2f);
            }
            else if (logic.DoorTaps == 1) toast.Show("Solid steel. Your knuckles disagree.", 1.6f);
            else if (logic.DoorTaps % 4 == 0) toast.Show("KNOCK KNOCK. Nobody's there. It's a door.", 1.6f);
        }

        void PressKey(int d)
        {
            if (keys != null && d < keys.Length && keys[d] != null) StartCoroutine(HalloweenKit.Pop(keys[d], 0.9f, 0.12f));
            Sfx.Play(this, "tick", 0.7f);
            Haptics.Tap();
            PasswordReaction r = logic.PressDigit(d);
            switch (r)
            {
                case PasswordReaction.Typed:
                    ShowEntry();
                    break;
                case PasswordReaction.Wrong:
                case PasswordReaction.NiceTry:
                    Sfx.Play(this, "buzz", 0.5f);
                    Haptics.Light();
                    SetLamp(LampRed, 1.2f);
                    StartCoroutine(Denied(r == PasswordReaction.NiceTry ? "NICE TRY" : "DENIED"));
                    toast.Show(logic.LastMessage, 2.2f);
                    break;
                case PasswordReaction.Unlocks:
                    StartCoroutine(Unlock());
                    break;
            }
        }

        IEnumerator Denied(string word)
        {
            if (display == null) yield break;
            display.text = word;
            display.color = new Color(1f, 0.4f, 0.35f);
            yield return HalloweenKit.Shake(display.transform, 0.04f, 0.35f);
            yield return new WaitForSeconds(0.5f);
            if (!logic.InputLocked && logic.Entry.Length == 0) ShowEntry();
        }

        void ShowEntry()
        {
            if (display == null) return;
            display.color = new Color(0.45f, 1f, 0.55f);
            string e = logic.Entry;
            var sb = new System.Text.StringBuilder(8);
            for (int i = 0; i < 4; i++) { if (i > 0) sb.Append(' '); sb.Append(i < e.Length ? e[i] : '_'); }
            display.text = sb.ToString();
        }

        void SetLamp(Color c, float seconds = 0f)
        {
            if (statusLamp != null) statusLamp.color = c;
            lampUntil = seconds > 0f ? Time.time + seconds : 0f;
        }

        // ---------- Finale ----------

        IEnumerator Unlock()
        {
            busy = true;
            DropFingers();
            SetLamp(LampGreen);
            if (display != null) { display.text = "ACCESS GRANTED"; display.color = new Color(0.45f, 1f, 0.55f); }
            Sfx.Play(this, "ding", 0.7f);
            Haptics.Medium();
            yield return new WaitForSeconds(0.35f);
            // la porta si spalanca (attorno alla serratura se è staccata, se no sui cardini)
            if (!logic.Detached) { pivotRight = false; doorDeg = 0f; }
            Sfx.Play(this, "whoosh", 0.8f);
            float from = doorDeg;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.7f)
            {
                doorDeg = Mathf.Lerp(from, 172f, 1f - (1f - k) * (1f - k));
                SetDoor(doorDeg);
                HalloweenKit.SetAlpha(mirrorReflection, pivotRight ? Mathf.Clamp01((doorDeg - 118f) / 22f) : 0f);
                yield return null;
            }
            doorDeg = 172f;
            SetDoor(doorDeg);
            Sfx.Play(this, "clunk", 0.7f);
            // si accende la luce: sgabuzzino delle scope
            yield return new WaitForSeconds(0.2f);
            Sfx.Play(this, "tick", 0.8f);
            HalloweenKit.SetAlpha(darkness, 0.4f);
            yield return new WaitForSeconds(0.08f);
            HalloweenKit.SetAlpha(darkness, 0.94f);
            yield return new WaitForSeconds(0.08f);
            yield return HalloweenKit.Fade(darkness, 0f, 0.2f);
            toast.Show("TOP SECRET: CLEANING SUPPLIES.", 3f);
            yield return new WaitForSeconds(0.4f);
            // il mocio cade lentamente verso il giocatore, oltre la soglia
            if (mop != null)
            {
                if (mopArt != null) mopArt.sortingOrder = 30;
                Vector3 p0 = mop.position, p1 = new Vector3(p0.x - 0.1f, floorY - 0.45f, p0.z);
                for (float k = 0f; k < 1f; k += Time.deltaTime / 0.9f)
                {
                    float e = k * k;
                    mop.position = Vector3.Lerp(p0, p1, e);
                    mop.rotation = mopRot * Quaternion.Euler(0f, 0f, 84f * e);
                    mop.localScale = Vector3.one * Mathf.Lerp(1f, 1.15f, e);
                    yield return null;
                }
                Sfx.Play(this, "bonk", 0.9f);
                Haptics.Light();
                yield return HalloweenKit.Shake(mop, 0.04f, 0.2f);
            }
            yield return Celebrate(1.3f);
            if (logic.TryComplete()) Win();
        }
    }
}
