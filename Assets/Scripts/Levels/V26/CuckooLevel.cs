using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 93 — l'orologio a cucù. Il biglietto sotto l'orologio è un indovinello che dice che ore sono.
    /// Trascina le lancette (la lunga scatta di 5 minuti, la corta è libera: a "e mezza" deve stare a metà!) e
    /// premi il pomello. Ora sbagliata: il cucù esce e ti becca. Easter egg `lunch_time`: mezzogiorno in punto.
    /// </summary>
    public class CuckooLevel : LevelController
    {
        [SerializeField] Transform hourHand, minuteHand;   // pivot al centro, a 0° puntano verso l'alto
        [SerializeField] Collider2D face;
        [SerializeField] TapButton setButton;
        [SerializeField] TextMesh riddle;
        [SerializeField] Transform cuckoo;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] float hourTolerance = 18f;
        [SerializeField] string eggId = "lunch_time";

        static readonly (string text, int h, int m)[] riddles =
        {
            ("Lunch was at 12:30.\nThat was 3 hours ago.", 3, 30),
            ("In two hours\nit will be 11 o'clock.", 9, 0),
            ("Half past four...\nminus a quarter of an hour.", 4, 15),
            ("The cat fell asleep at 1:00\nand slept 90 minutes.", 2, 30),
            ("A quarter to eight.", 7, 45),
            ("45 minutes after\nhalf past ten.", 11, 15),
            ("Bedtime is 9:00.\nIt's in 3 and a half hours.", 5, 30),
        };

        int answerH, answerM;
        float hourAngle, minuteAngle;   // gradi in senso orario dalle 12
        Transform dragging;
        bool done, busy;
        Camera cam;
        Vector3 cuckooIn;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            var r = riddles[Random.Range(0, riddles.Length)];
            riddle.text = r.text;
            answerH = r.h;
            answerM = r.m;
            hourAngle = 10f * 30f + 6f;   // 10:10, come in vetrina
            minuteAngle = 60f;
            Apply();
            cuckooIn = cuckoo.localPosition;
            cuckoo.gameObject.SetActive(false);
            setButton.Pressed += _ => { if (!busy && !done) StartCoroutine(Check()); };
        }

        void Apply()
        {
            hourHand.localRotation = Quaternion.Euler(0, 0, -hourAngle);
            minuteHand.localRotation = Quaternion.Euler(0, 0, -minuteAngle);
        }

        void Update()
        {
            if (done || busy || IsFinished) return;
            if (!PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out bool ended)) { dragging = null; return; }
            Vector2 w = PointerInput.ToWorld(cam, sp);
            Vector2 c = face.transform.position;
            float a = Mathf.Repeat(90f - Mathf.Atan2(w.y - c.y, w.x - c.x) * Mathf.Rad2Deg, 360f);
            if (began && face.OverlapPoint(w))
            {
                // si prende la lancetta più vicina al dito (la lunga se il dito è lontano dal centro)
                float dm = Mathf.Abs(Mathf.DeltaAngle(a, minuteAngle)), dh = Mathf.Abs(Mathf.DeltaAngle(a, hourAngle));
                float far = Vector2.Distance(w, c);
                dragging = (dm < dh && dm < 40f) || (far > 0.9f && dh > 25f) ? minuteHand : hourHand;
            }
            if (dragging == minuteHand) minuteAngle = Mathf.Repeat(Mathf.Round(a / 30f) * 30f, 360f);
            else if (dragging == hourHand) hourAngle = a;
            if (dragging != null) Apply();
            if (ended) { if (dragging != null) Sfx.Play(this, "tick"); dragging = null; }
        }

        IEnumerator Check()
        {
            busy = true;
            int m = Mathf.RoundToInt(minuteAngle / 6f) % 60;
            float want = (answerH % 12) * 30f + answerM * 0.5f;
            bool ok = m == answerM && Mathf.Abs(Mathf.DeltaAngle(hourAngle, want)) <= hourTolerance;
            bool noon = m == 0 && Mathf.Abs(Mathf.DeltaAngle(hourAngle, 0f)) <= hourTolerance;
            // fuori il cucù
            cuckoo.gameObject.SetActive(true);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.25f) { cuckoo.localPosition = cuckooIn + Vector3.down * 0.55f * k; yield return null; }
            if (ok)
            {
                done = true;
                for (int i = 0; i < Mathf.Max(1, answerH % 12); i++) { Sfx.Play(this, "ding", 0.4f); if (bubble != null) bubble.Show("CUCKOO!", true); yield return new WaitForSeconds(0.35f); }
                yield return new WaitForSeconds(0.8f);
                Win();
                yield break;
            }
            if (noon)
            {
                done = true;
                if (bubble != null) bubble.Show("Noon?! LUNCH BREAK. Back in an hour.", true);
                yield return new WaitForSeconds(2f);
                FailWithEgg(eggId);
                yield break;
            }
            done = true;
            if (bubble != null) bubble.Show("WRONG! *peck peck peck*", true);
            for (int i = 0; i < 3; i++)
            {
                Sfx.Play(this, "bonk", 0.8f);
                for (float k = 0f; k < 1f; k += Time.deltaTime / 0.12f) { cuckoo.localPosition = cuckooIn + Vector3.down * (0.55f + Mathf.Sin(k * Mathf.PI) * 0.4f); yield return null; }
            }
            yield return new WaitForSeconds(0.8f);
            Fail();
        }
    }
}
