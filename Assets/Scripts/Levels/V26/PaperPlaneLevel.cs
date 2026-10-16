using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 86 — l'aeroplanino di carta. Vola da solo verso destra e, uscito dallo schermo, rientra da sinistra.
    /// Inclinando il telefono si alza o si abbassa il muso: in picchiata prende velocità, in salita la perde (se va
    /// troppo piano stalla e cade). Soffiando (microfono) gli si dà una spinta, ma il fiato è poco (barra).
    /// Passa in tutti gli anelli senza toccare terra. (Editor: frecce = muso, B = soffio.)
    /// </summary>
    public class PaperPlaneLevel : LevelController
    {
        [SerializeField] Transform plane;
        [SerializeField] Transform[] rings;
        [SerializeField] Transform breathBar;
        [SerializeField] Sprite doneSprite;           // anello attraversato (verde)
        [SerializeField] SpeechBubble bubble;
        [SerializeField] float startSpeed = 2.6f, minSpeed = 1.1f, maxSpeed = 5f;
        [SerializeField] float pitchRate = 140f;      // gradi al secondo a telefono ben inclinato
        [SerializeField] float gravity = 4f;
        [SerializeField] float drag = 0.15f;
        [SerializeField] float breath = 2.5f;          // secondi di soffio disponibili
        [SerializeField] float blowThrust = 4f;
        [SerializeField] float groundY = -3.7f, ceilingY = 3.0f;

        readonly SteadyTilt steady = new SteadyTilt { smoothing = 0.06f, deadZone = 0.02f, shakeLevel = 99f };
        float pitch, speed, breathLeft;
        bool[] passed;
        int count;
        bool done;

        public override void OnLevelStart()
        {
            speed = startSpeed;
            breathLeft = breath;
            passed = new bool[rings.Length];
            if (bubble != null) { bubble.Show("Through every ring!"); StartCoroutine(HideLater(1.8f)); }
        }

        void OnDestroy() { Inputs.ReleaseMic(this); }

        void Update()
        {
            if (done || IsFinished) return;
            float dt = Time.deltaTime;
            // inclinare a destra = muso giù (come girare un volante)
            pitch = Mathf.Clamp(pitch - steady.Update(dt) * pitchRate * 4f * dt, -70f, 60f);
            float mic = Inputs.Mic(this);
            bool blowing = mic > 0.3f && breathLeft > 0f;
            if (blowing) { breathLeft -= dt; speed += blowThrust * dt; }
            if (breathBar != null) breathBar.localScale = new Vector3(Mathf.Clamp01(breathLeft / breath), 1f, 1f);
            float rad = pitch * Mathf.Deg2Rad;
            speed += (-Mathf.Sin(rad) * gravity - drag * speed) * dt;   // in picchiata accelera, in salita rallenta
            speed = Mathf.Clamp(speed, 0.2f, maxSpeed);
            if (speed < minSpeed) pitch = Mathf.MoveTowards(pitch, -60f, 90f * dt);   // stallo: il muso cade
            Vector3 p = plane.position + new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * speed * dt;
            if (p.y > ceilingY) { p.y = ceilingY; pitch = Mathf.Min(pitch, 0f); }
            if (p.x > 3.2f) p.x = -3.2f;   // fa il giro e rientra
            plane.position = p;
            plane.rotation = Quaternion.Euler(0, 0, pitch);

            for (int i = 0; i < rings.Length; i++)
            {
                if (passed[i] || Vector2.Distance(p, rings[i].position) > 0.38f) continue;
                passed[i] = true;
                count++;
                SpriteRenderer sr = rings[i].GetComponent<SpriteRenderer>();
                if (sr != null && doneSprite != null) sr.sprite = doneSprite;
                Sfx.Play(this, "ding", 0.4f);
                StartCoroutine(Pop(rings[i]));
                if (count >= rings.Length) { StartCoroutine(Landed()); return; }
            }
            if (p.y < groundY) StartCoroutine(Crash());
        }

        static IEnumerator Pop(Transform t)
        {
            Vector3 s = t.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.25f) { t.localScale = s * (1f + Mathf.Sin(k * Mathf.PI) * 0.35f); yield return null; }
            t.localScale = s;
        }

        IEnumerator Crash()
        {
            Haptics.Medium();
            done = true;
            if (bubble != null) bubble.Show("Crumple...");
            Vector3 s = plane.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.4f) { plane.localScale = new Vector3(s.x * (1f - k * 0.5f), s.y * (1f - k * 0.7f), 1f); yield return null; }
            yield return new WaitForSeconds(0.8f);
            Fail();
        }

        IEnumerator Landed()
        {
            done = true;
            if (bubble != null) bubble.Show("Perfect flight!");
            for (float k = 0f; k < 1.2f; k += Time.deltaTime)
            {
                plane.position += new Vector3(2.5f, 1.2f, 0f) * Time.deltaTime;
                plane.rotation = Quaternion.Euler(0, 0, 25f + Mathf.Sin(k * 10f) * 360f * k);   // giro della vittoria
                yield return null;
            }
            Win();
        }

        IEnumerator HideLater(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (bubble != null && !done) bubble.Hide();
        }
    }
}
