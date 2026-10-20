using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "The glass cage" — il topo è chiuso sotto una campana di vetro (vista dall'alto) che ha un'apertura alla
    /// base. Con un dito la campana non si solleva: va ruotata con DUE dita finché l'apertura guarda il topo, che esce.
    /// Ruotarla mentre il topo ha il naso contro il vetro lo urta: tre urti = sconfitta. Nell'Editor: Q/E ruotano.
    /// </summary>
    public class TheGlassCageLevel : S41ILevelBase
    {
        [SerializeField] Transform dome, mouse, cheese, alert;
        [SerializeField] Collider2D domeArea;
        [SerializeField] TextMesh triesText;

        const float NearEdge = 0.85f, AlignTolerance = 24f;
        float gapAngle, mouseAngle, mouseR = 0.4f, targetR = 0.45f, phaseUntil, gestureAngle, gestureTurned, bumpCooldown;
        bool gesture, nearPhase;
        int bumps, liftNags, cycles;

        protected override void Begin()
        {
            mouseAngle = Random.Range(0, 2) == 0 ? -40f : 220f;
            gapAngle = mouseAngle + 180f + Random.Range(-30f, 30f);
            dome.localRotation = Quaternion.Euler(0, 0, gapAngle);
            triesText.text = "BUMPS: " + Marks(0, 3);
            if (alert != null) alert.gameObject.SetActive(false);
            PlaceMouse();
            Say("Squeak! Let me out!", 2f);
        }

        void PlaceMouse()
        {
            float a = mouseAngle * Mathf.Deg2Rad;
            Vector3 c = dome.localPosition;
            mouse.localPosition = new Vector3(c.x + Mathf.Cos(a) * mouseR, c.y + Mathf.Sin(a) * mouseR - 0.15f, mouse.localPosition.z);
            if (alert != null) alert.localPosition = mouse.localPosition + new Vector3(0.35f, 0.9f, 0f);
        }

        protected override void Tick()
        {
            if (busy) return;
            float dt = Time.deltaTime;
            bumpCooldown -= dt;

            // il topo va avanti e indietro: centro, poi naso contro il vetro
            if (clock > phaseUntil)
            {
                nearPhase = !nearPhase;
                if (!nearPhase && ++cycles % 2 == 0)
                {
                    // nuova direzione, mai già davanti all'apertura (niente vittorie per caso)
                    float a = mouseAngle + Random.Range(-70f, 70f);
                    if (Mathf.Abs(Mathf.DeltaAngle(a, gapAngle)) > 60f) mouseAngle = a;
                }
                targetR = nearPhase ? 1.0f : 0.45f;
                phaseUntil = clock + (nearPhase ? 1.8f : 2.2f);
            }
            mouseR = Mathf.MoveTowards(mouseR, targetR, 0.8f * dt);
            bool near = mouseR > NearEdge;
            if (alert != null) alert.gameObject.SetActive(near);
            PlaceMouse();

            // rotazione con due dita
            var fs = Fingers();
            float turn = 0f;
            if (fs.Count >= 2 && !fs[0].ended && !fs[1].ended)
            {
                Vector2 d = fs[1].world - fs[0].world;
                float ang = Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;
                if (!gesture || fs[0].began || fs[1].began) { gesture = true; gestureAngle = ang; gestureTurned = 0f; }
                else { turn = Mathf.DeltaAngle(gestureAngle, ang); gestureAngle = ang; }
            }
            else
            {
                if (gesture) Sfx.Play(this, "tick", 0.4f);
                gesture = false;
                if (fs.Count == 1 && fs[0].began && domeArea.OverlapPoint(fs[0].world))
                {
                    liftNags++;
                    Sfx.Play(this, "clunk", 0.5f);
                    StartCoroutine(Shake(dome, 0.05f, 0.25f));
                    Say(liftNags < 3 ? "Too heavy to lift!" : "It won't lift... maybe it turns?", 1.6f);
                }
            }
#if UNITY_EDITOR
            if (Input.GetKey(KeyCode.Q)) turn += 90f * dt;
            if (Input.GetKey(KeyCode.E)) turn -= 90f * dt;
            if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.E)) gestureTurned = 0f;
#endif
            if (Mathf.Abs(turn) > 0.01f)
            {
                gapAngle += turn;
                gestureTurned += Mathf.Abs(turn);
                dome.localRotation = Quaternion.Euler(0, 0, gapAngle);
                if (Mathf.Repeat(gapAngle, 12f) < Mathf.Abs(turn)) Sfx.Play(this, "tick", 0.3f);
                if (near && gestureTurned > 8f && bumpCooldown <= 0f) { StartCoroutine(Bump()); return; }
            }

            // apertura allineata col topo: esce
            if (!gesture && Mathf.Abs(turn) < 0.01f && mouseR > 0.35f && Mathf.Abs(Mathf.DeltaAngle(gapAngle, mouseAngle)) < AlignTolerance)
                StartCoroutine(WalkOut());
        }

        IEnumerator Bump()
        {
            busy = true;
            bumps++;
            triesText.text = "BUMPS: " + Marks(bumps, 3);
            Sfx.Play(this, "bonk", 0.9f);
            Haptics.Medium();
            gesture = false;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.3f) { mouseR = Mathf.Lerp(1f, 0.35f, k); PlaceMouse(); yield return null; }
            yield return StartCoroutine(Shake(mouse, 0.1f, 0.4f));
            if (bumps >= 3)
            {
                Say("Ouch! I'm too dizzy...", 0f);
                for (float k = 0f; k < 1f; k += Time.deltaTime / 0.6f) { mouse.localRotation = Quaternion.Euler(0, 0, 90f * k); yield return null; }
                yield return new WaitForSeconds(1f);
                Fail();
                yield break;
            }
            Say("Ouch! Not while my nose is on the glass!", 2.2f);
            nearPhase = false;
            targetR = 0.45f;
            phaseUntil = clock + 2.2f;
            bumpCooldown = 1f;
            gestureTurned = 0f;
            busy = false;
        }

        IEnumerator WalkOut()
        {
            busy = true;
            if (alert != null) alert.gameObject.SetActive(false);
            Sfx.Play(this, "ding", 0.6f);
            Say("An exit! Squeak!", 0f);
            // snap dell'apertura e uscita lungo il raggio
            mouseAngle = gapAngle;
            while (mouseR < 2.2f) { mouseR += 1.5f * Time.deltaTime; PlaceMouse(); yield return null; }
            yield return StartCoroutine(Walk(mouse, cheese.localPosition + new Vector3(-0.5f, 0.1f, 0f), 2.5f, 0.1f));
            Sfx.Play(this, "pop", 0.7f);
            StartCoroutine(Pop(cheese, 1.3f, 0.3f));
            yield return StartCoroutine(Celebrate(1.6f));
        }
    }
}
