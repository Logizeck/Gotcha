using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;
using Gotcha.Sensors;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 101 — il carlino che dorme. La chiave è appesa al collare del carlino. Se lo tocchi si sveglia,
    /// abbaia e ti fissa con la lingua di fuori; alla terza volta è finita. Il trucco: prendere la pepiera, girarla
    /// sopra il suo naso e scuoterla (il telefono o il dito). ETCIÙ! Lo starnuto fa volare via la chiave, che poi va
    /// nella porta. Easter egg `snot_pop`: scoppiare la bolla al naso mentre è bella gonfia.
    /// </summary>
    public class PugLevel : LevelController
    {
        [SerializeField] PugRig pug;
        [SerializeField] Collider2D pugArea;
        [SerializeField] Collider2D snotArea;
        [SerializeField] Draggable2D key;
        [SerializeField] Draggable2D pepper;
        [SerializeField] Transform pepperSpout;
        [SerializeField] SmokeEmitter pepperDust;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] Vector2 keyLanding = new Vector2(0.4f, -3.35f);
        [SerializeField] float tickleRadius = 0.7f, tickleSeconds = 0.9f, wiggleSpeed = 5f;
        [SerializeField] int wakesAllowed = 3;
        [SerializeField] string eggId = "snot_pop";

        LockedDoor door;
        PinchScalable pinch;
        Camera cam;
        int wakes;
        float tickle, wiggle;
        Vector3 lastPepperPos;
        bool awake, sneezed, done;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            door = GetComponentInChildren<LockedDoor>(true);
            if (door != null) door.Opened += OnDoorOpened;
            key.enabled = false;   // finché resta appesa al collare non si può prendere (né rimpicciolire)
            pinch = key.GetComponent<PinchScalable>();
            if (pinch != null) pinch.enabled = false;
            key.DragStarted += Straighten;
            pepper.DragStarted += TurnPepper;
            pepper.Dropped += OnPepperDropped;
            pug.Sneezed += OnSneezed;
            lastPepperPos = pepper.transform.position;
            pug.Play(PugRig.Mode.Sleep);
            if (bubble != null) { bubble.Show("Shhh... he's sleeping on the key."); StartCoroutine(HideLater(2.4f)); }
        }

        void OnDestroy()
        {
            if (door != null) door.Opened -= OnDoorOpened;
            if (key != null) key.DragStarted -= Straighten;
            if (pepper != null) { pepper.DragStarted -= TurnPepper; pepper.Dropped -= OnPepperDropped; }
            if (pug != null) pug.Sneezed -= OnSneezed;
        }

        void Update()
        {
            if (done) return;
            if (PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out _) && began && !PointerInput.IsOverUi())
                OnTouch(PointerInput.ToWorld(cam, sp));
            UpdatePepper();
        }

        // ---------- tocchi: svegliare il carlino è vietato ----------

        void OnTouch(Vector2 world)
        {
            if (sneezed || awake) return;
            Collider2D pc = pepper.GetComponent<Collider2D>();
            if (pc != null && pc.OverlapPoint(world)) return;   // si sta prendendo la pepiera
            if (snotArea != null && snotArea.OverlapPoint(world) && pug.SnotIsBig) { StartCoroutine(SnotEgg()); return; }
            if (pugArea.OverlapPoint(world)) StartCoroutine(WakeUp());
        }

        IEnumerator WakeUp()
        {
            awake = true;
            wakes++;
            tickle = 0f;
            pepperDust.Stop();
            pug.Play(PugRig.Mode.Bark);
            yield return new WaitForSeconds(pug.barkPeriod * 2f - 0.2f);
            pug.Play(PugRig.Mode.Stare);
            if (bubble != null)
            {
                int left = wakesAllowed - wakes;
                bubble.Show(left <= 0 ? "That's it. No key for you!" : left == 1 ? "Last warning!" : "Don't touch the dog!");
            }
            yield return new WaitForSeconds(2.2f);
            if (bubble != null) bubble.Hide();
            if (wakes >= wakesAllowed)
            {
                done = true;
                Fail();
                yield break;
            }
            if (sneezed) yield break;
            pug.Play(PugRig.Mode.Sleep);
            awake = false;
        }

        IEnumerator SnotEgg()
        {
            done = true;
            pug.PopSnot();
            yield return new WaitForSeconds(0.25f);
            pug.Play(PugRig.Mode.Bark);
            if (bubble != null) bubble.Show("EWWW!");
            yield return new WaitForSeconds(1.8f);
            FailWithEgg(eggId);
        }

        // ---------- la pepiera ----------

        void TurnPepper(Draggable2D d) { d.transform.localRotation = Quaternion.Euler(0, 0, 180f); }   // tappo in giù

        void OnPepperDropped(Draggable2D d, DropSlot slot)
        {
            d.transform.localRotation = Quaternion.identity;
            pepperDust.Stop();
        }

        void UpdatePepper()
        {
            Vector3 pos = pepper.transform.position;
            float speed = Time.deltaTime > 0f ? (pos - lastPepperPos).magnitude / Time.deltaTime : 0f;
            lastPepperPos = pos;
            wiggle = Mathf.Lerp(wiggle, speed, 10f * Time.deltaTime);

            bool held = Draggable2D.Active == pepper;
            bool shaking = SensorHub.Instance != null && SensorHub.Instance.IsShaking;
            bool pouring = held && (shaking || wiggle > wiggleSpeed);
            if (pouring) pepperDust.Play(); else pepperDust.Stop();

            bool onNose = pouring && !sneezed && !awake && pug.noseTip != null &&
                          Vector2.Distance(pepperSpout.position, pug.noseTip.position) < tickleRadius &&
                          pepperSpout.position.y > pug.noseTip.position.y - 0.15f;
            tickle = onNose ? tickle + Time.deltaTime : Mathf.Max(0f, tickle - Time.deltaTime * 0.5f);
            if (tickle >= tickleSeconds) StartSneeze();
        }

        void StartSneeze()
        {
            sneezed = true;
            pepperDust.Stop();
            if (bubble != null) bubble.Hide();
            pug.Play(PugRig.Mode.Sneeze);
        }

        void OnSneezed() { StartCoroutine(KeyFlies()); }

        IEnumerator KeyFlies()
        {
            Transform k = key.transform;
            k.SetParent(transform, true);
            Vector3 from = k.position, to = new Vector3(keyLanding.x, keyLanding.y, from.z);
            float spin = 0f;
            for (float a = 0f; a < 1f; a += Time.deltaTime / 0.7f)
            {
                Vector3 p = Vector3.Lerp(from, to, a);
                p.y += Mathf.Sin(a * Mathf.PI) * 1.2f;   // arco
                k.position = p;
                spin += Time.deltaTime * 900f;
                k.localRotation = Quaternion.Euler(0, 0, spin);
                yield return null;
            }
            k.position = to;
            k.localRotation = Quaternion.identity;
            Sfx.Play(this, "clunk");
            key.enabled = true;
            if (pinch != null) pinch.enabled = true;
            yield return new WaitForSeconds(0.6f);
            if (bubble != null && !done) { bubble.Show("Bless you! Now, the door..."); StartCoroutine(HideLater(2f)); }
        }

        void Straighten(Draggable2D d) { d.transform.localRotation = Quaternion.identity; }

        void OnDoorOpened()
        {
            if (done) return;
            done = true;
            Win();
        }

        IEnumerator HideLater(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (bubble != null && !done) bubble.Hide();
        }
    }
}
