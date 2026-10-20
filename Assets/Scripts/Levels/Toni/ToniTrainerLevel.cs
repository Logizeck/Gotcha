using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello di Toni "TONI'S PERSONAL TRAINER" — un percorso con tre ostacoli, ma Toni preferisce stare sdraiato.
    /// La soluzione: trascinare un biscotto lungo il percorso senza darglielo: Toni lo segue, salta gli ostacoli e
    /// arriva al traguardo. Se il biscotto passa troppo vicino alla bocca se lo mangia e torna a dormire (ne compare
    /// un altro). Tre biscotti mangiati prima dell'arrivo = sconfitta.
    /// </summary>
    public class ToniTrainerLevel : LevelController
    {
        [SerializeField] ToniPug toni;
        [SerializeField] Transform biscuit, finish;
        [SerializeField] Transform[] obstacles;
        [SerializeField] TextMesh eatenText;
        [SerializeField] SmokeEmitter[] confetti;
        [SerializeField] int eatsAllowed = 3;
        [SerializeField] float wakeRange = 2.3f, eatRange = 0.6f, speed = 1.6f, jumpHalfWidth = 0.4f, jumpHeight = 0.62f, mouthOffset = 0.47f;

        Camera cam;
        bool busy, awake, dragging;
        int eaten;
        float baseGround, farTime;
        Vector3 biscuitHome, biscuitScale;
        Vector2 dragOffset;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            biscuitHome = biscuit.position;
            biscuitScale = biscuit.localScale;
            toni.Face(true);
            toni.Stand(false);
            toni.Sleep();
            S41LKit.Confetti(confetti, false);
            UpdateText();
            StartCoroutine(Intro());
        }

        IEnumerator Intro()
        {
            yield return new WaitForSeconds(0.5f);
            toni.Say("Training? Zzz... no thanks.", 2f);
        }

        void UpdateText() { eatenText.text = "BISCUITS EATEN " + eaten + "/" + eatsAllowed; }

        /// <summary>Altezza del salto: un arco sopra ogni ostacolo.</summary>
        float Arc(float x)
        {
            float h = 0f;
            foreach (Transform o in obstacles)
            {
                float d = Mathf.Abs(x - o.localPosition.x) / jumpHalfWidth;
                if (d < 1f) h = Mathf.Max(h, jumpHeight * (1f - d * d));
            }
            return h;
        }

        bool InJump(float x)
        {
            foreach (Transform o in obstacles) if (Mathf.Abs(x - o.localPosition.x) < jumpHalfWidth) return true;
            return false;
        }

        void Update()
        {
            if (IsFinished || busy) return;
            HandleDrag();
            Vector2 b = biscuit.position;
            if (!awake)
            {
                if (Vector2.Distance(b, toni.HeadWorld) < wakeRange) StartCoroutine(WakeUp());
                return;
            }
            if (Vector2.Distance(b, toni.MouthWorld) < eatRange) { StartCoroutine(Eat()); return; }

            // insegue il biscotto (la bocca verso il biscotto); sopra un ostacolo non si ferma a metà salto
            Transform t = toni.transform;
            float x = t.localPosition.x;
            float bx = t.parent.InverseTransformPoint(b).x;
            float dir = bx > x ? 1f : -1f;
            float target = bx - dir * mouthOffset;
            if (InJump(x) && Mathf.Abs(target - x) < jumpHalfWidth * 2f) target = x + Mathf.Sign(target - x + 0.0001f) * jumpHalfWidth * 2f;
            target = Mathf.Clamp(target, -2.1f, finish.localPosition.x + 0.1f);
            toni.GroundY = baseGround + Arc(x);
            if (toni.StepToward(target, speed)) toni.Land();
            Vector3 p = t.localPosition;
            if (InJump(p.x)) { p.y = baseGround + Arc(p.x); t.localPosition = p; }

            // biscotto lontano troppo a lungo: torna a sdraiarsi
            if (Vector2.Distance(b, toni.HeadWorld) > 3.4f) farTime += Time.deltaTime; else farTime = 0f;
            if (farTime > 3f && !InJump(p.x)) { farTime = 0f; GoToSleep("Meh. Too far. Zzz..."); }

            if (p.x >= finish.localPosition.x) StartCoroutine(Arrive());
        }

        void HandleDrag()
        {
            bool down = S41LKit.Pointer(cam, out Vector2 w, out bool began, out bool ended);
            if (began && biscuit.gameObject.activeSelf && Vector2.Distance(w, biscuit.position) < 0.6f) { dragging = true; dragOffset = (Vector2)biscuit.position - w; }
            if (!dragging) return;
            Vector2 p = w + dragOffset;
            biscuit.position = new Vector3(Mathf.Clamp(p.x, -2.5f, 2.5f), Mathf.Clamp(p.y, -3.9f, 2.6f), biscuit.position.z);
            if (ended || !down) dragging = false;
        }

        IEnumerator WakeUp()
        {
            busy = true;
            toni.WakeUp(1);
            toni.LookTarget = biscuit;
            Sfx.Play(this, "snore", 0.4f);
            yield return new WaitForSeconds(0.35f);
            toni.WakeUp(2);
            toni.Stand(true);
            baseGround = toni.GroundY;
            toni.Alert(true);
            toni.Say("Sniff sniff... BISCUIT?!", 1.3f, "woof");
            yield return new WaitForSeconds(0.5f);
            toni.Alert(false);
            awake = true;
            farTime = 0f;
            busy = false;
        }

        void GoToSleep(string line)
        {
            awake = false;
            toni.GroundY = baseGround;
            toni.Land();
            toni.Stand(false);
            toni.Sleep();
            toni.Say(line, 1.4f);
        }

        IEnumerator Eat()
        {
            busy = true;
            dragging = false;
            yield return StartCoroutine(toni.Chomp(biscuit));
            eaten++;
            UpdateText();
            Haptics.Light();
            if (eaten >= eatsAllowed)
            {
                GoToSleep("Three biscuits. Training over. Zzz...");
                yield return new WaitForSeconds(1.6f);
                Fail();
                yield break;
            }
            GoToSleep(eaten == 1 ? "Yum. Nap time." : "Yum again. Zzz...");
            yield return new WaitForSeconds(1.0f);
            // un biscotto nuovo dal barattolo
            biscuit.position = biscuitHome;
            biscuit.localScale = biscuitScale * 0.2f;
            biscuit.gameObject.SetActive(true);
            yield return StartCoroutine(S41LKit.ScaleTo(biscuit, biscuitScale, 0.3f));
            Sfx.Play(this, "pop", 0.6f);
            busy = false;
        }

        IEnumerator Arrive()
        {
            busy = true;
            dragging = false;
            toni.Land();
            toni.Say("I DID IT! I'm an ATHLETE!", 1.6f, "woof");
            Sfx.Play(this, "ding", 0.9f);
            Haptics.Medium();
            S41LKit.Confetti(confetti, true);
            // premio: il biscotto, finalmente
            yield return StartCoroutine(toni.Chomp(biscuit));
            yield return StartCoroutine(toni.Happy(1.6f));
            yield return new WaitForSeconds(0.4f);
            S41LKit.Confetti(confetti, false);
            Win();
        }
    }
}
