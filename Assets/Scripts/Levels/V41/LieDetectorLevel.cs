using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "Lie detector" — un robot ti interroga ("ARE YOU A HUMAN?") con i pulsanti YES/NO. Rispondere non serve:
    /// tre risposte e il robot ti classifica come tostapane (sconfitta). Il trucco: trascina la macchina della verità
    /// sul robot; alla domanda "sei umano?" lui dice di sì, l'ago impazzisce e il bugiardo viene smascherato.
    /// </summary>
    public class LieDetectorLevel : S41FLevelBase
    {
        [SerializeField] Transform robot;
        [SerializeField] Collider2D robotArea;
        [SerializeField] SpriteRenderer faceArt;
        [SerializeField] Sprite panicFace;
        [SerializeField] Transform detector;
        [SerializeField] Collider2D detectorArea;
        [SerializeField] Transform needle;
        [SerializeField] SpriteRenderer lamp;
        [SerializeField] Transform yesButton, noButton;
        [SerializeField] Collider2D yesArea, noArea;
        [SerializeField] TextMesh questionText, triesText, verdictText;
        [SerializeField] SmokeEmitter smoke;
        [SerializeField] Transform clampSpot;

        static readonly string[] Questions = { "ARE YOU A HUMAN?", "IS THE SKY BLUE?", "DO YOU TRUST ROBOTS?" };
        static readonly string[] Replies = { "HMM. SUSPICIOUS. *beep*", "NOTED. VERY... HUMAN-ISH.", "...BEEP BOOP. I MEAN, HA HA." };
        const int MaxAnswers = 3;

        int answers, question;
        bool dragging;
        Vector2 grabOffset;
        Vector3 detectorHome;
        float needleWild;

        protected override void Begin()
        {
            detectorHome = detector.localPosition;
            if (smoke != null) smoke.Stop();
            if (verdictText != null) verdictText.text = "";
            Lamp(false);
            ShowQuestion();
            UpdateTries();
            Say("GREETINGS, FELLOW HUMAN. ANSWER MY QUESTIONS.", 3f);
        }

        void ShowQuestion() { if (questionText != null) questionText.text = Questions[question % Questions.Length]; }
        void UpdateTries() { if (triesText != null) triesText.text = "ANSWERS " + answers + "/" + MaxAnswers; }
        void Lamp(bool lie) { if (lamp != null) lamp.color = lie ? new Color(1f, 0.25f, 0.25f, 0.95f) : new Color(0.45f, 1f, 0.5f, 0.6f); }

        protected override void Tick()
        {
            needle.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(clock * (6f + needleWild * 30f)) * (6f + needleWild * 50f));
            if (busy) return;
            if (!Pointer(out Vector2 w, out bool began, out bool ended)) { if (dragging) Release(); return; }
            if (began)
            {
                if (detectorArea != null && detectorArea.OverlapPoint(w)) { dragging = true; grabOffset = (Vector2)detector.position - w; Sfx.Play(this, "pop", 0.4f); }
                else if (yesArea != null && yesArea.OverlapPoint(w)) { StartCoroutine(Answer(yesButton)); return; }
                else if (noArea != null && noArea.OverlapPoint(w)) { StartCoroutine(Answer(noButton)); return; }
            }
            if (dragging)
            {
                Vector2 p = w + grabOffset;
                detector.position = new Vector3(Mathf.Clamp(p.x, -2.4f, 2.4f), Mathf.Clamp(p.y, -4.2f, 2.6f), detector.position.z);
                if (ended) Release();
            }
        }

        void Release()
        {
            dragging = false;
            if (robotArea != null && robotArea.OverlapPoint(detector.position)) { StartCoroutine(Expose()); return; }
            StartCoroutine(Move(detector, detectorHome, 0.25f));
        }

        IEnumerator Answer(Transform button)
        {
            busy = true;
            answers++;
            UpdateTries();
            Sfx.Play(this, "tick", 0.7f);
            yield return Pop(button, 1.15f, 0.15f);
            if (answers >= MaxAnswers)
            {
                Say("ANALYSIS COMPLETE: YOU ARE... A TOASTER.", 3f);
                Sfx.Play(this, "buzz", 0.8f);
                Haptics.Medium();
                if (verdictText != null) verdictText.text = "TOASTER";
                yield return Shake(robot, 0.06f, 0.5f);
                yield return new WaitForSeconds(1.6f);
                Fail();
                yield break;
            }
            Say(Replies[question % Replies.Length], 2.2f);
            question++;
            ShowQuestion();
            yield return new WaitForSeconds(0.4f);
            busy = false;
        }

        IEnumerator Expose()
        {
            busy = true;
            Sfx.Play(this, "clunk", 0.7f);
            Haptics.Light();
            yield return Move(detector, clampSpot.localPosition, 0.25f);
            if (questionText != null) questionText.text = "ROBOT: ARE YOU A HUMAN?";
            Say("WHAT IS THIS? FINE. YES. I AM 100% HUMAN.", 2.6f);
            yield return new WaitForSeconds(1.4f);
            // l'ago impazzisce
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.6f) { needleWild = k; yield return null; }
            Lamp(true);
            if (verdictText != null) verdictText.text = "LIE!";
            Sfx.Play(this, "buzz", 1f);
            Haptics.Heavy();
            if (faceArt != null && panicFace != null) faceArt.sprite = panicFace;
            if (smoke != null) smoke.Play();
            yield return Shake(robot, 0.12f, 0.8f);
            Say("ERROR! ERROR! ...OKAY. I AM A ROBOT. *sob*", 2.6f);
            yield return new WaitForSeconds(1.2f);
            needleWild = 0f;
            if (smoke != null) smoke.Stop();
            // il robot si affloscia
            Vector3 rs = robot.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.5f) { robot.localScale = new Vector3(rs.x * (1f + 0.08f * k), rs.y * (1f - 0.12f * k), rs.z); yield return null; }
            yield return Celebrate();
        }
    }
}
