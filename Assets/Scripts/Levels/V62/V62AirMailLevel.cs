using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels.V62;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 253 "Air mail" — il gatto sul tetto aspetta una lettera. L'aeroplanino di carta cade a metà strada.
    /// Soluzione: legare la lettera al palloncino, toccare il nodo per liberarlo e guidarlo inclinando il telefono
    /// (o col dito) fino al tetto. Se passa sopra la candela scoppia: la lettera cade e il palloncino ricompare legato.
    /// </summary>
    public class V62AirMailLevel : V62Level
    {
        [SerializeField] Transform balloon;          // origine = nodo; il disegno è un figlio
        [SerializeField] GameObject balloonArt;
        [SerializeField] Collider2D balloonArea;
        [SerializeField] Transform letterSpot;       // figlio del palloncino: dove pende la lettera
        [SerializeField] Transform tiePoint;         // cima del paletto
        [SerializeField] Collider2D knotArea;
        [SerializeField] LineRenderer balloonString;
        [SerializeField] Transform letter;
        [SerializeField] Collider2D letterArea;
        [SerializeField] Transform plane;
        [SerializeField] Collider2D planeArea;
        [SerializeField] Transform flame;
        [SerializeField] Collider2D candleArea;
        [SerializeField] Collider2D roofZone;
        [SerializeField] Transform roofCat;
        [SerializeField] Transform catPaw;
        [SerializeField] Transform groundLeft;       // altezza del prato (dove cade la lettera)
        [SerializeField] SmokeEmitter popPuff;
        [SerializeField] SmokeEmitter hearts;
        [SerializeField] SpeechBubble catBubble;
        [SerializeField] TextMesh infoText;
        [SerializeField] Transform infoHolder;
        [SerializeField] float riseSpeed = 0.55f;
        [SerializeField] float maxKnotY = 2.2f;
        [SerializeField] float balloonRadius = 0.45f;
        [SerializeField] float balloonHeight = 1.2f;

        readonly AirMailLogic logic = new AirMailLogic();
        readonly SteadyTilt tilt = new SteadyTilt();
        Camera cam;
        HwDragger drag;
        HwDragger.Item letterItem;
        HwSay catSay;
        HwToast toast;
        Vector3 balloonHome, letterTableHome, planeHome, balloonScale;
        Transform hanging;    // cosa pende dal palloncino in volo (la lettera)
        bool busy, planePressed;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            logic.Reset();
            ResetV62();
            catSay = new HwSay(catBubble);
            toast = new HwToast(infoText, infoHolder);
            drag = new HwDragger(this);
            balloonHome = balloon.position;
            balloonScale = balloon.localScale;
            letterTableHome = letter.position;
            planeHome = plane.position;
            letterItem = drag.Add(letter, letterArea, 5);
            if (popPuff != null) popPuff.Stop();
            if (hearts != null) hearts.Stop();
            catSay.Show("Any mail for me?", 2f);
        }

        void Update()
        {
            catSay.Tick();
            toast.Tick();
            float lean = tilt.Update(Time.deltaTime);
            UpdateString();
            if (IsFinished) return;
            bool havePointer = HalloweenKit.ReadPointer(cam, out Vector2 w, out bool began, out bool ended);
            if (logic.State == AirMailState.Flying && !busy) Fly(lean, havePointer, w);
            if (busy || logic.InputLocked) return;
            if (!havePointer)
            {
                if (drag.Held != null) Drop(drag.Held, true);
                planePressed = false;
                return;
            }
            if (logic.State == AirMailState.Flying) return;   // in volo si guida soltanto
            if (began)
            {
                HwDragger.Item it = drag.TryPick(w);
                if (it != null) OnPick(it);
                else Tap(w);
            }
            if (planePressed && ended) { planePressed = false; if (logic.TryThrowPlane()) StartCoroutine(ThrowPlane()); }
            if (drag.Held == null) return;
            drag.MoveHeld(w);
            if (ended) Drop(drag.Held, false);
        }

        /// <summary>Il filo: legato al paletto finché il palloncino non parte, poi pende con quello che porta.</summary>
        void UpdateString()
        {
            if (balloonString == null) return;
            bool visible = balloonArt.activeSelf;
            balloonString.enabled = visible;
            if (!visible) return;
            Vector3 end = logic.BalloonTied ? tiePoint.position
                : hanging != null ? hanging.position + Vector3.up * 0.15f : balloon.position + Vector3.down * 0.4f;
            balloonString.SetPosition(0, balloon.position);
            balloonString.SetPosition(1, end);
        }

        /// <summary>Volo libero: sale piano; l'inclinazione (o il dito, in Editor e come aiuto) lo sposta di lato.</summary>
        void Fly(float lean, bool havePointer, Vector2 w)
        {
            float vx = Mathf.Clamp(lean * 7f, -1.6f, 1.6f);
            if (havePointer) vx = Mathf.Clamp((w.x - balloon.position.x) * 2.2f, -1.6f, 1.6f);
            Vector3 p = balloon.position;
            p.x = Mathf.Clamp(p.x + vx * Time.deltaTime, HalloweenKit.SafeMinX + 0.2f, HalloweenKit.SafeMaxX - 0.1f);
            p.y = Mathf.Min(maxKnotY, p.y + riseSpeed * Time.deltaTime);
            balloon.position = p;
            balloon.localRotation = Quaternion.Euler(0f, 0f, -vx * 6f);
            if (AirMailLogic.FlameTouches(p.x, p.y, balloonRadius, balloonHeight, flame.position.x, flame.position.y + 0.15f))
            {
                if (logic.TryPop()) StartCoroutine(Pop());
                return;
            }
            if ((roofZone.OverlapPoint(p) || hanging != null && roofZone.OverlapPoint(hanging.position)) && logic.TryReachRoof())
                StartCoroutine(Deliver());
        }

        void OnPick(HwDragger.Item it)
        {
            if (it == letterItem && letter.parent == letterSpot)
            {
                logic.TryUntieLetter();
                letter.SetParent(transform, true);
                letter.localRotation = Quaternion.identity;
                letterItem.Home = letterTableHome;
            }
        }

        void Tap(Vector2 w)
        {
            if (planeArea.OverlapPoint(w)) { planePressed = true; return; }
            if (knotArea.OverlapPoint(w))
            {
                if (logic.TryReleaseKnot())
                {
                    hanging = letter;
                    letterItem.Enabled = false;
                    Sfx.Play(this, "pop", 0.6f);
                    Haptics.Light();
                    toast.Show("Tilt the phone to steer!", 2.2f);
                }
                else
                {
                    Sfx.Play(this, "tick", 0.6f);
                    StartCoroutine(HalloweenKit.Wiggle(balloon, 10f, 0.5f, 16f));
                    toast.Show("Nothing to deliver yet.", 1.6f);
                }
                return;
            }
            if (balloonArea.OverlapPoint(w))
            {
                Sfx.Play(this, "bonk", 0.4f);
                StartCoroutine(HalloweenKit.Wiggle(balloon, 8f, 0.5f, 12f));
                return;
            }
            if (candleArea.OverlapPoint(w))
            {
                Sfx.Play(this, "tick", 0.5f);
                toast.Show("Hot! Keep balloons away.", 1.6f);
                return;
            }
            if (roofZone.OverlapPoint(w)) catSay.Show("Mail. Up here. Please.", 1.8f);
        }

        void Drop(HwDragger.Item it, bool lostFinger)
        {
            drag.Release();
            bool onBalloon = !lostFinger && (balloonArea.OverlapPoint(it.T.position) || Vector2.Distance(it.T.position, balloon.position) < 0.55f);
            if (it == letterItem)
            {
                if (onBalloon && logic.TryTieLetter())
                {
                    letter.SetParent(letterSpot, true);
                    drag.SendTo(letterItem, letterSpot.position, 0.15f, 0f);
                    Sfx.Play(this, "clunk", 0.5f);
                    toast.Show("Tied! Now untie the balloon...", 1.8f);
                    return;
                }
                drag.SendHome(letterItem, 0.35f);
            }
        }

        IEnumerator ThrowPlane()
        {
            busy = true;
            Sfx.Play(this, "whoosh", 0.7f);
            Haptics.Light();
            Vector3 peak = new Vector3(0.7f, -0.2f, plane.position.z);
            for (float t = 0f; t < 1f; t += Time.deltaTime / 0.7f)
            {
                Vector3 a = Vector3.Lerp(planeHome, peak, t);
                plane.position = a + Vector3.up * Mathf.Sin(t * Mathf.PI) * 0.8f;
                plane.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(30f, -10f, t));
                yield return null;
            }
            // stallo e caduta
            Vector3 ground = new Vector3(1.0f, groundLeft.position.y, plane.position.z);
            for (float t = 0f; t < 1f; t += Time.deltaTime / 0.8f)
            {
                plane.position = Vector3.Lerp(peak, ground, t * t) + Vector3.right * Mathf.Sin(t * 9f) * 0.12f;
                plane.localRotation = Quaternion.Euler(0f, 0f, -10f - 50f * t);
                yield return null;
            }
            Sfx.Play(this, "bonk", 0.5f);
            toast.Show("Not even close.", 1.6f);
            catSay.Show("Meh.", 1.2f);
            yield return new WaitForSeconds(0.7f);
            yield return HalloweenKit.Move(plane, planeHome, 0.4f, 0.3f);
            plane.localRotation = Quaternion.identity;
            logic.TryLandPlane();
            busy = false;
        }

        IEnumerator Pop()
        {
            busy = true;
            Sfx.Play(this, "pop", 1f);
            Sfx.Play(this, "bonk", 0.6f);
            Haptics.Medium();
            if (popPuff != null) { popPuff.transform.position = balloon.position + Vector3.up * 0.6f; StartCoroutine(HalloweenKit.Burst(popPuff, 0.25f)); }
            balloonArt.SetActive(false);
            toast.Show("Pop!", 1.4f);
            // la lettera cade a terra, dov'è
            hanging = null;
            letter.SetParent(transform, true);
            letter.localRotation = Quaternion.identity;
            Vector3 fall = new Vector3(Mathf.Clamp(letter.position.x, -2.3f, 2.3f), groundLeft.position.y, letter.position.z);
            yield return HalloweenKit.Move(letter, fall, 0.6f);
            letterItem.Home = fall;
            yield return Respawn();
            letterItem.Enabled = true;
            busy = false;
        }

        /// <summary>Il palloncino ricompare legato al paletto.</summary>
        IEnumerator Respawn()
        {
            yield return new WaitForSeconds(0.4f);
            balloon.position = balloonHome;
            balloon.localRotation = Quaternion.identity;
            balloon.localScale = balloonScale * 0.1f;
            balloonArt.SetActive(true);
            Sfx.Play(this, "pop", 0.5f);
            yield return HalloweenKit.ScaleTo(balloon, balloonScale, 0.3f);
        }

        IEnumerator Deliver()
        {
            busy = true;
            Sfx.Play(this, "ding", 0.6f);
            catSay.Show("MAIL! For ME!", 2f);
            // il gatto prende la lettera
            Vector3 paw = catPaw.position;
            yield return HalloweenKit.Move(balloon, paw + (balloon.position - letter.position), 0.5f);
            hanging = null;
            letter.SetParent(roofCat, true);
            yield return HalloweenKit.Move(letter, paw, 0.2f);
            Haptics.Medium();
            yield return HalloweenKit.Pop(roofCat, 1.12f, 0.3f);
            // il palloncino se ne va per conto suo
            StartCoroutine(HalloweenKit.Move(balloon, balloon.position + new Vector3(-0.6f, 5f, 0f), 2.5f));
            if (hearts != null) hearts.Play();
            yield return Celebrate(2.2f);
            if (hearts != null) hearts.Stop();
            if (logic.TryCompleteDelivery()) Win();
        }
    }
}
