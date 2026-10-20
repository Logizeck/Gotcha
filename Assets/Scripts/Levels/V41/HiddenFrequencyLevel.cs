using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "Hidden frequency" — una cassaforte risponde a tre frequenze; c'è solo una manopola senza numeri.
    /// Si gira la manopola piano e si guarda l'indicatore di vibrazione della serratura (più trema, più si è vicini):
    /// fermi sulla frequenza giusta, una delle tre luci diventa verde. Trovate tutte e tre, OPEN apre la cassaforte.
    /// Ogni ricerca ha un tempo massimo: superarlo tre volte surriscalda la serratura (sconfitta). Nessun suono forte.
    /// </summary>
    public class HiddenFrequencyLevel : S41JLevelBase
    {
        [SerializeField] Transform dial;
        [SerializeField] Collider2D dialArea;
        [SerializeField] SpriteRenderer[] lights;
        [SerializeField] Transform lockPlate;
        [SerializeField] SpriteRenderer wiggles;
        [SerializeField] Transform vibrationGauge;    // scala x 0..1
        [SerializeField] Collider2D openArea;
        [SerializeField] SpriteRenderer safeArt;
        [SerializeField] Sprite safeOpen;
        [SerializeField] GameObject doorParts;        // manopola, serratura e indicatore (spariscono all'apertura)
        [SerializeField] TextMesh timeText;
        [SerializeField] TextMesh triesText;

        const int MaxTries = 3;
        const float SearchTime = 30f, TurnsRange = 720f, Tolerance = 0.016f, LockTime = 0.5f;
        float[] targets = new float[3];
        bool[] found = new bool[3];
        int foundCount, tries;
        float freq = 0.0f, lastAngle, searchLeft = SearchTime, onTarget, speed, buzzTimer;
        bool turning, started;
        Vector3 plateHome;

        protected override void Begin()
        {
            // tre frequenze a caso in tre fasce ben separate
            targets[0] = Random.Range(0.12f, 0.27f);
            targets[1] = Random.Range(0.43f, 0.58f);
            targets[2] = Random.Range(0.74f, 0.9f);
            plateHome = lockPlate.localPosition;
            foreach (SpriteRenderer l in lights) if (l != null) l.color = new Color(0.55f, 0.55f, 0.6f);
            UpdateTexts();
            Say("Find my three frequencies... quietly.", 3f);
        }

        void UpdateTexts()
        {
            if (timeText != null) timeText.text = started ? "SEARCH TIME " + Mathf.CeilToInt(Mathf.Max(0f, searchLeft)) : "TURN THE DIAL";
            if (triesText != null) triesText.text = "OVERHEATS " + tries + "/" + MaxTries;
        }

        float Closeness(out int nearest)
        {
            nearest = -1;
            float best = 0f;
            for (int i = 0; i < 3; i++)
            {
                if (found[i]) continue;
                float c = Mathf.Clamp01(1f - Mathf.Abs(freq - targets[i]) / 0.13f);
                if (c > best) { best = c; nearest = i; }
            }
            return best;
        }

        protected override void Tick()
        {
            if (busy) return;
            float dt = Time.deltaTime;
            bool hadPointer = Pointer(out Vector2 w, out bool began, out bool ended);
            float delta = 0f;
            if (hadPointer)
            {
                Vector2 c = dial.position;
                float ang = Mathf.Atan2(w.y - c.y, w.x - c.x) * Mathf.Rad2Deg;
                if (began)
                {
                    if (dialArea != null && dialArea.OverlapPoint(w)) { turning = true; lastAngle = ang; started = true; }
                    else if (openArea != null && openArea.OverlapPoint(w)) { StartCoroutine(TryOpen()); return; }
                }
                else if (turning)
                {
                    // orario = frequenza più alta
                    delta = -Mathf.DeltaAngle(lastAngle, ang);
                    lastAngle = ang;
                }
                if (ended) turning = false;
            }
            else turning = false;

            float before = freq;
            freq = Mathf.Clamp01(freq + delta / TurnsRange);
            dial.localRotation = Quaternion.Euler(0f, 0f, -freq * TurnsRange);
            float turnSpeed = Mathf.Abs(freq - before) * TurnsRange / Mathf.Max(dt, 0.001f);
            speed = Mathf.Lerp(speed, turnSpeed, 0.2f);
            if (delta != 0f && Mathf.Abs(freq - before) > 0.002f) { buzzTimer -= 1f; if (buzzTimer < 0f) { buzzTimer = 6f; Sfx.Play(this, "tick", 0.12f); } }

            // indicatore di vibrazione: troppo veloce = solo rumore
            float close = Closeness(out int nearest);
            float shown = speed > 260f ? Random.Range(0.05f, 0.35f) : close * close;
            if (vibrationGauge != null) vibrationGauge.localScale = new Vector3(Mathf.Lerp(vibrationGauge.localScale.x, Mathf.Max(0.02f, shown), 0.25f), 1f, 1f);
            lockPlate.localPosition = plateHome + new Vector3(Mathf.Sin(clock * 70f) * 0.07f * shown, Mathf.Cos(clock * 53f) * 0.03f * shown, 0f);
            Alpha(wiggles, shown);
            if (shown > 0.5f && Random.value < shown * dt * 6f) Haptics.Light();

            // fermi sulla frequenza giusta: la luce diventa verde
            if (nearest >= 0 && Mathf.Abs(freq - targets[nearest]) < Tolerance && speed < 120f)
            {
                onTarget += dt;
                if (onTarget >= LockTime) { StartCoroutine(Found(nearest)); return; }
            }
            else onTarget = 0f;

            if (started && foundCount < 3)
            {
                searchLeft -= dt;
                if (searchLeft <= 0f) { StartCoroutine(Overheat()); return; }
            }
            UpdateTexts();
        }

        IEnumerator Found(int i)
        {
            busy = true;
            turning = false;
            found[i] = true;
            SpriteRenderer l = lights[foundCount];
            foundCount++;
            onTarget = 0f;
            searchLeft = SearchTime;
            if (l != null) l.color = new Color(0.4f, 1f, 0.45f);
            Sfx.Play(this, "ding", 0.35f);
            Haptics.Medium();
            if (l != null) yield return Pop(l.transform, 1.4f, 0.3f);
            Say(foundCount < 3 ? (foundCount == 1 ? "Mmm... that tickles!" : "One more frequency!") : "All three! Press OPEN.", 2.2f);
            UpdateTexts();
            busy = false;
        }

        IEnumerator Overheat()
        {
            busy = true;
            turning = false;
            tries++;
            UpdateTexts();
            Sfx.Play(this, "buzz", 0.5f);
            Haptics.Medium();
            Say(tries >= MaxTries ? "OVERHEATED! Lock jammed." : "Too slow! I'm overheating...", 2.2f);
            if (safeArt != null) safeArt.color = new Color(1f, 0.6f, 0.55f);
            yield return Shake(lockPlate, 0.1f, 0.6f);
            if (safeArt != null) safeArt.color = Color.white;
            if (tries >= MaxTries)
            {
                Haptics.Heavy();
                yield return new WaitForSeconds(0.8f);
                Fail();
                yield break;
            }
            searchLeft = SearchTime;
            started = false;
            UpdateTexts();
            busy = false;
        }

        IEnumerator TryOpen()
        {
            busy = true;
            turning = false;
            if (foundCount < 3)
            {
                Sfx.Play(this, "clunk", 0.4f);
                Say("Locked. " + foundCount + "/3 frequencies.", 1.8f);
                yield return Shake(lockPlate, 0.06f, 0.3f);
                busy = false;
                yield break;
            }
            Sfx.Play(this, "clunk", 0.8f);
            Haptics.Medium();
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.6f)
            {
                dial.localRotation = Quaternion.Euler(0f, 0f, -freq * TurnsRange - k * 360f);
                yield return null;
            }
            if (safeArt != null && safeOpen != null) safeArt.sprite = safeOpen;
            if (doorParts != null) doorParts.SetActive(false);
            Say("Click. Treasure!", 2f);
            yield return Celebrate();
        }
    }
}
