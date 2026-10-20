using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "The impatient clock" — un orologio che urla di essere in ritardo va messo sulle 12:00, ma le lancette
    /// lasciate libere tornano indietro da sole. Il trucco: un dito tiene ferma la lancetta dei minuti mentre l'altro
    /// porta quella delle ore; con entrambe sul 12 si staccano le due dita insieme (finestra di tolleranza).
    /// Tre tentativi a due mani finiti con lancette disallineate = sconfitta.
    /// </summary>
    public class TheImpatientClockLevel : S41JLevelBase
    {
        [SerializeField] Transform center;
        [SerializeField] Transform hourHand, minuteHand;
        [SerializeField] SpriteRenderer faceArt;
        [SerializeField] Sprite happyFace;
        [SerializeField] Transform clockBody;
        [SerializeField] SpriteRenderer twelveGlow;
        [SerializeField] TextMesh triesText;
        [SerializeField] float hourRest = 270f, minuteRest = 180f;

        const int MaxTries = 3;
        const float Tolerance = 14f, ReleaseWindow = 0.45f, DriftSpeed = 70f, HourLength = 1.25f, MinuteLength = 1.9f;
        const int EditorPin = -50;

        // stato di una lancetta: angolo in gradi in senso orario dal 12
        class Hand
        {
            public Transform t;
            public float angle, rest, grabOffset, releaseAngle, releaseTime;
            public int finger = -99;
            public bool Held => finger != -99;
        }

        Hand hour, minute;
        int tries;
        bool bothHeld, saidSlip;

        protected override void Begin()
        {
            hour = new Hand { t = hourHand, angle = hourRest, rest = hourRest };
            minute = new Hand { t = minuteHand, angle = minuteRest, rest = minuteRest };
            Apply(hour);
            Apply(minute);
            UpdateTries();
            Say("I'M LATE! Set me to 12 o'clock! NOW!", 3f);
        }

        void UpdateTries() { if (triesText != null) triesText.text = "MISALIGNED " + tries + "/" + MaxTries; }

        static void Apply(Hand h) => h.t.localRotation = Quaternion.Euler(0f, 0f, -h.angle);

        float AngleOf(Vector2 w)
        {
            Vector2 d = w - (Vector2)center.position;
            return Mathf.Repeat(90f - Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg, 360f);
        }

        static float DistToHand(Vector2 w, Vector2 c, float angle, float length)
        {
            float r = angle * Mathf.Deg2Rad;
            Vector2 tip = c + new Vector2(Mathf.Sin(r), Mathf.Cos(r)) * length;
            Vector2 ab = tip - c;
            float t = Mathf.Clamp01(Vector2.Dot(w - c, ab) / ab.sqrMagnitude);
            return Vector2.Distance(w, c + ab * t);
        }

        static bool OnTwelve(float a) => Mathf.Abs(Mathf.DeltaAngle(a, 0f)) <= Tolerance;

        protected override void Tick()
        {
            if (busy) return;
            Vector2 c = center.position;
            foreach (Finger f in Fingers())
            {
                if (f.began && !(hour.Held && minute.Held))
                {
                    float dh = hour.Held ? 99f : DistToHand(f.world, c, hour.angle, HourLength + 0.2f);
                    float dm = minute.Held ? 99f : DistToHand(f.world, c, minute.angle, MinuteLength + 0.2f);
                    Hand pick = dm < dh ? minute : hour;
                    if (Mathf.Min(dh, dm) < 0.55f)
                    {
                        pick.finger = f.id;
                        pick.grabOffset = Mathf.DeltaAngle(AngleOf(f.world), pick.angle);
                        Sfx.Play(this, "tick", 0.5f);
                    }
                }
                foreach (Hand h in new[] { hour, minute })
                {
                    if (h.finger != f.id) continue;
                    h.angle = Mathf.Repeat(AngleOf(f.world) + h.grabOffset, 360f);
                    if (f.ended) Release(h);
                }
            }
#if UNITY_EDITOR
            // Editor: Spazio tiene ferma la lancetta dei minuti dov'è (secondo dito)
            if (Input.GetKeyDown(KeyCode.Space)) minute.finger = EditorPin;   // anche se la teneva il mouse: passa allo Spazio
            if (Input.GetKeyUp(KeyCode.Space) && minute.finger == EditorPin) Release(minute);
#endif
            // dita sparite senza "ended"
            if (Input.touchCount == 0 && !Input.GetMouseButton(0))
            {
                if (hour.Held && hour.finger != EditorPin) Release(hour);
                if (minute.Held && minute.finger != EditorPin) Release(minute);
            }
            if (hour.Held && minute.Held) bothHeld = true;

            // le lancette libere tornano indietro
            foreach (Hand h in new[] { hour, minute })
            {
                if (!h.Held) h.angle = Mathf.Repeat(Mathf.MoveTowardsAngle(h.angle, h.rest, DriftSpeed * Time.deltaTime), 360f);
                Apply(h);
            }
            if (twelveGlow != null) Alpha(twelveGlow, (OnTwelve(hour.angle) ? 0.45f : 0f) + (OnTwelve(minute.angle) ? 0.45f : 0f));

            if (!hour.Held && !minute.Held && (hour.releaseTime > 0f || minute.releaseTime > 0f)) Judge();
        }

        void Release(Hand h)
        {
            if (!h.Held) return;
            h.finger = -99;
            h.releaseAngle = h.angle;
            h.releaseTime = clock;
        }

        /// <summary>Tutte le dita staccate: valuta il tentativo.</summary>
        void Judge()
        {
            bool together = Mathf.Abs(hour.releaseTime - minute.releaseTime) <= ReleaseWindow;
            bool aligned = OnTwelve(hour.releaseAngle) && OnTwelve(minute.releaseAngle);
            bool wasBoth = bothHeld;
            hour.releaseTime = minute.releaseTime = 0f;
            bothHeld = false;
            if (wasBoth && together && aligned) { StartCoroutine(Victory()); return; }
            if (!wasBoth)
            {
                if (!saidSlip) { saidSlip = true; Say("Ugh! My hands keep slipping back!", 2.4f); }
                return;
            }
            StartCoroutine(Misaligned(aligned ? "Let go TOGETHER!" : "That's not 12:00!"));
        }

        IEnumerator Misaligned(string line)
        {
            busy = true;
            tries++;
            UpdateTries();
            Sfx.Play(this, "buzz", 0.6f);
            Haptics.Medium();
            Say(tries >= MaxTries ? "TOO LATE! I'm going without you!" : line, 2.2f);
            yield return Shake(clockBody, 0.12f, 0.5f);
            if (tries >= MaxTries)
            {
                Sfx.Play(this, "bonk", 0.9f);
                Haptics.Heavy();
                // l'orologio scappa via di corsa
                yield return Move(clockBody, clockBody.localPosition + new Vector3(4.5f, 0.3f, 0f), 0.6f, 0.6f);
                yield return new WaitForSeconds(0.4f);
                Fail();
                yield break;
            }
            busy = false;
        }

        IEnumerator Victory()
        {
            busy = true;
            hour.angle = minute.angle = 0f;
            Apply(hour);
            Apply(minute);
            if (faceArt != null && happyFace != null) faceArt.sprite = happyFace;
            Say("DING DONG! Right on time!", 2.5f);
            for (int i = 0; i < 3; i++)
            {
                Sfx.Play(this, "ding", 0.7f);
                Haptics.Light();
                yield return Shake(clockBody, 0.08f, 0.3f);
                yield return new WaitForSeconds(0.15f);
            }
            yield return Celebrate();
        }
    }
}
