using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "Back to the future" — la porta che serve allo scienziato esiste solo nel futuro (il cartello dice il
    /// giorno). Soluzione: sfoglia il calendario-macchina del tempo con swipe verso l'alto (più lungo lo swipe, più
    /// pagine) fino al giorno esatto, senza superarlo: la porta compare. Se resti fermo il tempo scivola indietro.
    /// Superare la data tre volte inceppa la macchina = sconfitta.
    /// </summary>
    public class BackToTheFutureLevel : LevelController
    {
        [SerializeField] Transform calendar, page, flyPage, door, scientist;
        [SerializeField] TextMesh dayText, weekText, flyDayText, triesText;
        [SerializeField] SpriteRenderer doorArt, doorGhost;
        [SerializeField] Collider2D calendarArea;
        [SerializeField] SmokeEmitter smoke;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] SmokeEmitter[] confetti;
        [SerializeField] int startDay = 20, targetDay = 29;
        [SerializeField] float pageSwipe = 0.6f, slipSeconds = 4.5f;

        static readonly string[] Week = { "TUESDAY", "WEDNESDAY", "THURSDAY", "FRIDAY", "SATURDAY", "SUNDAY", "MONDAY" };

        Camera cam;
        int day, jams;
        bool busy, swiping;
        Vector2 swipeStart;
        float idle;
        Vector3 pageHome;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            day = startDay;
            pageHome = flyPage.localPosition;
            flyPage.gameObject.SetActive(false);
            S41KKit.Alpha(doorArt, 0f);
            if (smoke != null) smoke.Stop();
            S41KKit.Confetti(confetti, false);
            ShowDay();
            UpdateTries();
            if (bubble != null) bubble.Show("My door will be built on October " + targetDay + ". I can't wait that long!");
        }

        void ShowDay()
        {
            dayText.text = day > 31 ? (day - 31).ToString() : day.ToString();
            weekText.text = (day > 31 ? "NOVEMBER - " : "OCTOBER - ") + Week[(day - startDay + 700) % 7];
        }

        void UpdateTries() { triesText.text = "TIME JAMS: " + jams + "/3"; }

        void Update()
        {
            if (IsFinished || busy) return;
            // il presente ti richiama: se resti fermo si torna indietro di un giorno
            if (day > startDay && !swiping)
            {
                idle += Time.deltaTime;
                if (idle > slipSeconds) { idle = 0f; StartCoroutine(Slip()); return; }
            }
            if (!PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out bool ended)) { swiping = false; return; }
            Vector2 w = PointerInput.ToWorld(cam, sp);
            if (began && calendarArea.OverlapPoint(w)) { swiping = true; swipeStart = w; idle = 0f; }
            if (swiping)
            {
                float dy = w.y - swipeStart.y;
                page.localPosition = new Vector3(0f, Mathf.Clamp(dy, 0f, 0.5f) * 0.3f, 0f);
            }
            if (swiping && ended)
            {
                swiping = false;
                page.localPosition = Vector3.zero;
                float dy = w.y - swipeStart.y;
                if (dy > 0.3f) StartCoroutine(Flip(Mathf.Max(1, Mathf.RoundToInt(dy / pageSwipe))));
                else if (dy < -0.3f && day > startDay) { day--; ShowDay(); Sfx.Play(this, "tick", 0.6f); }
                else if (bubble != null) bubble.Show("Swipe the pages UP to travel forward!");
            }
        }

        IEnumerator Slip()
        {
            busy = true;
            day--;
            ShowDay();
            Sfx.Play(this, "whoosh", 0.4f);
            if (bubble != null) bubble.Show("Hurry! The present is pulling us back!");
            yield return StartCoroutine(S41KKit.Shake(calendar, 0.06f, 0.3f));
            busy = false;
        }

        IEnumerator Flip(int pages)
        {
            busy = true;
            for (int k = 0; k < pages; k++)
            {
                flyDayText.text = dayText.text;
                flyPage.gameObject.SetActive(true);
                flyPage.localPosition = pageHome;
                flyPage.localRotation = Quaternion.identity;
                day++;
                ShowDay();
                Sfx.Play(this, "tick", 0.7f);
                float dur = pages > 3 ? 0.07f : 0.14f;
                for (float t = 0f; t < 1f; t += Time.deltaTime / dur)
                {
                    flyPage.localPosition = pageHome + new Vector3(t * 0.6f, t * 1.4f, 0f);
                    flyPage.localRotation = Quaternion.Euler(0f, 0f, -t * 40f);
                    yield return null;
                }
                flyPage.gameObject.SetActive(false);
                if (day > targetDay) { yield return StartCoroutine(Jam()); yield break; }
            }
            idle = 0f;
            if (day == targetDay) { yield return StartCoroutine(WinRoutine()); yield break; }
            busy = false;
        }

        IEnumerator Jam()
        {
            jams++;
            UpdateTries();
            Sfx.Play(this, "buzz", 0.8f);
            Sfx.Play(this, "bonk", 0.7f);
            Haptics.Heavy();
            if (smoke != null) smoke.Play();
            if (bubble != null) bubble.Show("TOO FAR! The time machine is jammed!");
            yield return StartCoroutine(S41KKit.Shake(calendar, 0.18f, 0.8f));
            if (smoke != null) smoke.Stop();
            if (jams >= 3)
            {
                if (bubble != null) bubble.Show("It's broken... we're stuck in the present forever.");
                yield return new WaitForSeconds(1.5f);
                Fail();
                yield break;
            }
            day = startDay;
            ShowDay();
            if (bubble != null) bubble.Show("Back to today. Stop EXACTLY on the " + targetDay + "th!");
            busy = false;
        }

        IEnumerator WinRoutine()
        {
            Sfx.Play(this, "ding", 0.9f);
            if (bubble != null) bubble.Show("It's the day! Look, my door!");
            for (float t = 0f; t < 1f; t += Time.deltaTime / 0.9f)
            {
                S41KKit.Alpha(doorArt, t);
                S41KKit.Alpha(doorGhost, 1f - t);
                door.localScale = Vector3.one * (0.9f + 0.1f * t);
                yield return null;
            }
            S41KKit.Alpha(doorArt, 1f);
            Haptics.Light();
            yield return new WaitForSeconds(0.4f);
            Vector3 target = new Vector3(door.localPosition.x, scientist.localPosition.y, 0f);
            yield return StartCoroutine(S41KKit.Move(scientist, target, 1.2f, 0.15f));
            StartCoroutine(S41KKit.ScaleTo(scientist, new Vector3(0.05f, 0.05f, 1f), 0.5f));
            S41KKit.Confetti(confetti, true);
            if (bubble != null) bubble.Show("See you in the future!");
            yield return new WaitForSeconds(1.8f);
            S41KKit.Confetti(confetti, false);
            Win();
        }
    }
}
