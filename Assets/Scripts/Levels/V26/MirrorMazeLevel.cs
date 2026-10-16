using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 91 — il sentiero allo specchio. Il topolino segue il dito... al contrario: sposti a destra, lui va a
    /// sinistra. Deve percorrere il sentiero stretto fino al formaggio senza toccare la recinzione elettrica.
    /// A metà strada passa davanti a un secondo specchio e si inverte ANCHE su e giù.
    /// </summary>
    public class MirrorMazeLevel : LevelController
    {
        [SerializeField] Transform mouse;
        [SerializeField] Transform[] path;
        [SerializeField] int flipAt = 3;              // dal tratto flipAt in poi anche l'asse verticale è invertito
        [SerializeField] float halfWidth = 0.45f;
        [SerializeField] TextMesh sign;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] float sensitivity = 0.75f;

        Vector2 lastFinger;
        bool holding, done, flipped;
        int reached;
        Camera cam;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            mouse.position = path[0].position;
            if (sign != null) sign.text = "<  >";
            if (bubble != null) { bubble.Show("Wait... left is right?!"); StartCoroutine(HideLater(2f)); }
        }

        float DistToPath(Vector2 p, out int seg)
        {
            float best = float.MaxValue;
            seg = 0;
            for (int i = 0; i + 1 < path.Length; i++)
            {
                Vector2 a = path[i].position, b = path[i + 1].position, ab = b - a;
                float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude);
                float d = Vector2.Distance(p, a + ab * t);
                if (d < best) { best = d; seg = i; }
            }
            return best;
        }

        void Update()
        {
            if (done || IsFinished) return;
            if (!PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out _)) { holding = false; return; }
            Vector2 w = PointerInput.ToWorld(cam, sp);
            if (began || !holding) { holding = true; lastFinger = w; return; }
            Vector2 d = (w - lastFinger) * sensitivity;
            lastFinger = w;
            d.x = -d.x;
            if (flipped) d.y = -d.y;
            // a piccoli passi: niente salti attraverso la recinzione
            int steps = Mathf.CeilToInt(d.magnitude / 0.05f);
            for (int s = 0; s < steps; s++)
            {
                Vector2 p = (Vector2)mouse.position + d / steps;
                mouse.position = new Vector3(p.x, p.y, mouse.position.z);
                if (DistToPath(p, out int seg) > halfWidth) { StartCoroutine(Zap()); return; }
                reached = Mathf.Max(reached, seg);
                if (!flipped && reached >= flipAt)
                {
                    flipped = true;
                    Sfx.Play(this, "whoosh");
                    if (sign != null) sign.text = "<  >\n^\nv";
                    if (bubble != null) { bubble.Show("Another mirror! Up is down now!"); StartCoroutine(HideLater(1.8f)); }
                }
                if (Vector2.Distance(p, path[path.Length - 1].position) < 0.25f) { StartCoroutine(Cheese()); return; }
            }
            if (Mathf.Abs(d.x) > 0.001f) mouse.localScale = new Vector3(Mathf.Abs(mouse.localScale.x) * (d.x > 0 ? -1f : 1f), mouse.localScale.y, 1f);
        }

        IEnumerator Zap()
        {
            Haptics.Medium();
            done = true;
            Sfx.Play(this, "buzz", 1f);
            if (bubble != null) bubble.Show("BZZZT!");
            Vector3 p = mouse.position;
            for (float k = 0f; k < 0.6f; k += Time.deltaTime) { mouse.position = p + (Vector3)Random.insideUnitCircle * 0.06f; yield return null; }
            yield return new WaitForSeconds(0.6f);
            Fail();
        }

        IEnumerator Cheese()
        {
            done = true;
            if (bubble != null) bubble.Show("Cheese! My brain hurts, but cheese!");
            yield return new WaitForSeconds(1.5f);
            Win();
        }

        IEnumerator HideLater(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (bubble != null && !done) bubble.Hide();
        }
    }
}
