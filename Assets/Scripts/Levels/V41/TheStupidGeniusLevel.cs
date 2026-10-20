using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "The stupid genius" — uno scienziato ha costruito una macchina enorme per premere un pulsante a pochi
    /// centimetri. Soluzione: ignorare la macchina e toccare direttamente il pulsantino rosso. Avviare la macchina (START)
    /// provoca guasti sempre più stupidi: al terzo crolla tutto (sconfitta).
    /// </summary>
    public class TheStupidGeniusLevel : S41ILevelBase
    {
        [SerializeField] Transform machine, gear, glove, startButton, target, scientist, boom;
        [SerializeField] Collider2D startArea, targetArea, machineArea;
        [SerializeField] SmokeEmitter smoke;
        [SerializeField] SpriteRenderer scientistArt;
        [SerializeField] TextMesh triesText;

        int starts;
        Vector3 gearHome, gloveHome, machineHome, sciHome;

        protected override void Begin()
        {
            gearHome = gear.localPosition;
            gloveHome = glove.localPosition;
            machineHome = machine.localPosition;
            sciHome = scientist.localPosition;
            if (boom != null) boom.gameObject.SetActive(false);
            if (smoke != null) smoke.Stop();
            triesText.text = "MACHINE RUNS: " + Marks(0, 3);
            Say("Behold my Button-Pressing Machine!", 2.6f);
        }

        protected override void Tick()
        {
            if (!busy) gear.localRotation = Quaternion.Euler(0, 0, clock * 20f);
            if (busy || !Pointer(out Vector2 w, out bool began, out _) || !began) return;
            if (targetArea.OverlapPoint(w)) StartCoroutine(PressDirectly());
            else if (startArea.OverlapPoint(w)) StartCoroutine(RunMachine());
            else if (machineArea.OverlapPoint(w)) { Say("Careful! That's 40 years of genius!"); Sfx.Play(this, "clunk", 0.5f); }
        }

        IEnumerator RunMachine()
        {
            busy = true;
            starts++;
            triesText.text = "MACHINE RUNS: " + Marks(starts, 3);
            Sfx.Play(this, "clunk", 0.8f);
            yield return StartCoroutine(Pop(startButton, 0.85f, 0.2f));
            Say("Initiating sequence!", 1.5f);
            // la macchina si avvia: tremolio, ingranaggio veloce, fumo
            if (smoke != null) smoke.Play();
            for (float k = 0f; k < 1.4f; k += Time.deltaTime)
            {
                machine.localPosition = machineHome + new Vector3(Mathf.Sin(k * 70f) * 0.04f, Mathf.Sin(k * 53f) * 0.03f, 0f);
                gear.localRotation = Quaternion.Euler(0, 0, -k * 900f);
                if (Mathf.Repeat(k, 0.3f) < Time.deltaTime) Sfx.Play(this, "tick", 0.6f);
                yield return null;
            }
            machine.localPosition = machineHome;
            if (starts == 1) yield return StartCoroutine(GearFalls());
            else if (starts == 2) yield return StartCoroutine(GlovePunch());
            else { yield return StartCoroutine(Collapse()); yield break; }
            if (smoke != null) smoke.Stop();
            busy = false;
        }

        IEnumerator GearFalls()
        {
            Sfx.Play(this, "pop", 0.8f);
            Vector3 p = gear.localPosition;
            float vx = 1.6f, vy = 3f;
            while (p.y > -3.9f)
            {
                vy -= 12f * Time.deltaTime;
                p += new Vector3(vx, vy, 0f) * Time.deltaTime;
                gear.localPosition = p;
                gear.localRotation = Quaternion.Euler(0, 0, -p.x * 400f);
                yield return null;
            }
            Sfx.Play(this, "bonk", 0.8f);
            Haptics.Light();
            for (float k = 0f; k < 0.8f; k += Time.deltaTime)
            {
                p.x += 2.5f * Time.deltaTime;
                gear.localPosition = p;
                gear.localRotation = Quaternion.Euler(0, 0, -p.x * 400f);
                yield return null;
            }
            Say("A minor setback! Let me fix it.", 2f);
            yield return new WaitForSeconds(0.9f);
            gear.localPosition = gearHome;
            StartCoroutine(Pop(gear, 1.3f, 0.25f));
        }

        IEnumerator GlovePunch()
        {
            // il guantone parte... e sbaglia bersaglio: colpisce lo scienziato
            Sfx.Play(this, "whoosh", 0.8f);
            Vector3 from = glove.position, to = scientist.position + new Vector3(0f, 1.6f, 0f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.35f)
            {
                glove.position = Vector3.Lerp(from, to, k) + Vector3.up * Mathf.Sin(k * Mathf.PI) * 0.8f;
                yield return null;
            }
            Sfx.Play(this, "bonk", 1f);
            Haptics.Medium();
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.3f) { scientist.localRotation = Quaternion.Euler(0, 0, -25f * Mathf.Sin(k * Mathf.PI)); yield return null; }
            scientist.localRotation = Quaternion.identity;
            Say("Ow! Recalibrating...", 2f);
            yield return StartCoroutine(Move(glove, gloveHome, 0.4f));
        }

        IEnumerator Collapse()
        {
            Sfx.Play(this, "buzz", 0.9f);
            for (float k = 0f; k < 0.8f; k += Time.deltaTime)
            {
                machine.localPosition = machineHome + new Vector3(Mathf.Sin(k * 90f) * 0.12f, Mathf.Sin(k * 77f) * 0.08f, 0f);
                yield return null;
            }
            if (boom != null) { boom.gameObject.SetActive(true); StartCoroutine(Pop(boom, 1.4f, 0.3f)); }
            Sfx.Play(this, "bonk", 1f);
            Haptics.Heavy();
            // la macchina crolla e lo scienziato resta nero di fuliggine
            Vector3 from = machine.localPosition;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.6f)
            {
                machine.localPosition = from + Vector3.down * 2.5f * k * k;
                machine.localRotation = Quaternion.Euler(0, 0, 25f * k);
                if (scientistArt != null) scientistArt.color = Color.Lerp(Color.white, new Color(0.35f, 0.33f, 0.36f), k);
                yield return null;
            }
            Say("Back to the drawing board...", 0f);
            yield return new WaitForSeconds(1.6f);
            Fail();
        }

        IEnumerator PressDirectly()
        {
            busy = true;
            Sfx.Play(this, "clunk", 0.7f);
            Vector3 s = target.localScale;
            target.localScale = new Vector3(s.x * 1.1f, s.y * 0.7f, s.z);
            Haptics.Light();
            yield return new WaitForSeconds(0.25f);
            target.localScale = s;
            Say("It... just WORKS?! Why didn't I think of that?", 0f);
            // la macchina offesa si affloscia
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.6f) { machine.localRotation = Quaternion.Euler(0, 0, -6f * k); yield return null; }
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.5f)
            {
                scientist.localPosition = sciHome + Vector3.up * Mathf.Sin(k * Mathf.PI) * 0.5f;
                yield return null;
            }
            scientist.localPosition = sciHome;
            yield return StartCoroutine(Celebrate(1.8f));
        }
    }
}
