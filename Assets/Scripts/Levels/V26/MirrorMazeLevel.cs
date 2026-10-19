using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 91 — il sentiero allo specchio. Il topolino segue il dito... al contrario: sposti a destra, lui va a
    /// sinistra. Deve percorrere il sentiero stretto fino al formaggio senza toccare la recinzione elettrica.
    /// A metà strada passa davanti a un secondo specchio e si inverte ANCHE su e giù. Una legenda in basso mostra
    /// sempre dove va il topo per ogni direzione del dito (frecce rosse lampeggianti = invertite) e un'animazione
    /// annuncia ogni inversione.
    /// </summary>
    public class MirrorMazeLevel : LevelController
    {
        [SerializeField] Transform mouse;
        [SerializeField] Transform[] path;
        [SerializeField] int flipAt = 3;              // dal tratto flipAt in poi anche l'asse verticale è invertito
        [SerializeField] float halfWidth = 0.45f;
        [SerializeField] SpriteRenderer[] arrows;     // legenda: casella del dito 0 destra, 1 sinistra, 2 su, 3 giù; la freccia punta dove va il topo
        [SerializeField] SpriteRenderer lrIcon, udIcon;
        [SerializeField] TextMesh lrText, udText;
        [SerializeField] Transform bigIcon;           // icona grande al centro quando cambia l'inversione
        [SerializeField] SpriteRenderer flash;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] float sensitivity = 0.75f;

        Vector2 lastFinger;
        bool holding, done, flipped, busy;
        float[] arrowAngle = new float[4];
        static readonly Color Swapped = new Color(1f, 0.3f, 0.3f), Normal = new Color(0.55f, 0.85f, 0.55f), Off = new Color(0.75f, 0.75f, 0.75f);
        int reached;
        Camera cam;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            mouse.position = path[0].position;
            arrowAngle[0] = 180f; arrowAngle[1] = 0f; arrowAngle[2] = 90f; arrowAngle[3] = 270f;
            for (int i = 0; i < 4; i++) arrows[i].transform.localRotation = Quaternion.Euler(0, 0, arrowAngle[i]);
            lrText.text = "LEFT <-> RIGHT\nSWAPPED";
            lrText.color = Swapped;
            udText.text = "UP & DOWN\nNORMAL";
            udText.color = Off;
            lrIcon.color = Off;
            udIcon.color = Off;
            flash.gameObject.SetActive(false);
            bigIcon.gameObject.SetActive(false);
            StartCoroutine(Announce(false));
            if (bubble != null) { bubble.Show("Wait... left is right?!"); StartCoroutine(HideLater(2.4f)); }
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
            // legenda: le frecce invertite lampeggiano
            float blink = 0.5f + 0.5f * Mathf.Sin(Time.time * 9f);
            for (int i = 0; i < 4; i++)
            {
                bool sw = i < 2 || flipped;
                arrows[i].color = sw ? Color.Lerp(Swapped, Color.white, blink * 0.6f) : Normal;
                arrows[i].transform.localScale = Vector3.one * (sw ? 1f + 0.12f * blink : 1f);
            }
            if (busy) { holding = false; return; }
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
                    StartCoroutine(Announce(true));
                    if (bubble != null) { bubble.Show("Another mirror! Up is down now!"); StartCoroutine(HideLater(2.4f)); }
                    if (Mathf.Abs(d.x) > 0.001f) mouse.localScale = new Vector3(Mathf.Abs(mouse.localScale.x) * (d.x > 0 ? -1f : 1f), mouse.localScale.y, 1f);
                    return;   // il topo si ferma: si riparte col nuovo verso
                }
                if (Vector2.Distance(p, path[path.Length - 1].position) < 0.25f) { StartCoroutine(Cheese()); return; }
            }
            if (Mathf.Abs(d.x) > 0.001f) mouse.localScale = new Vector3(Mathf.Abs(mouse.localScale.x) * (d.x > 0 ? -1f : 1f), mouse.localScale.y, 1f);
        }

        /// <summary>Annuncia un'inversione: lampo, icona grande che si specchia, poi vola nella legenda.</summary>
        IEnumerator Announce(bool vertical)
        {
            busy = true;
            holding = false;
            Sfx.Play(this, "whoosh", 0.9f);
            if (vertical) Haptics.Light();
            flash.gameObject.SetActive(true);
            SpriteRenderer target = vertical ? udIcon : lrIcon;
            bigIcon.gameObject.SetActive(true);
            bigIcon.position = new Vector3(0f, 0.6f, bigIcon.position.z);
            bigIcon.localRotation = Quaternion.Euler(0, 0, vertical ? 90f : 0f);
            float baseY = Mathf.Abs(mouse.localScale.y);
            for (float t = 0f; t < 1.1f; t += Time.deltaTime)
            {
                flash.color = new Color(1f, 1f, 1f, Mathf.Clamp01(0.55f - t * 1.5f));
                float pop = Mathf.Min(1f, t / 0.2f) * 2.6f;
                // si specchia due volte (come girarsi davanti allo specchio)
                bigIcon.localScale = new Vector3(pop * Mathf.Cos(t * Mathf.PI * 2f / 0.55f), pop, 1f);
                if (vertical) mouse.localScale = new Vector3(mouse.localScale.x, baseY * Mathf.Cos(t * Mathf.PI * 2f / 0.55f), 1f);
                yield return null;
            }
            flash.gameObject.SetActive(false);
            mouse.localScale = new Vector3(mouse.localScale.x, baseY, 1f);
            Vector3 from = bigIcon.position, to = target.transform.position;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.35f)
            {
                bigIcon.position = Vector3.Lerp(from, to, k);
                bigIcon.localScale = Vector3.one * Mathf.Lerp(2.6f, target.transform.lossyScale.y, k);
                yield return null;
            }
            bigIcon.gameObject.SetActive(false);
            target.color = Swapped;
            Sfx.Play(this, "pop", 0.7f);
            if (vertical)
            {
                udText.text = "UP <-> DOWN\nSWAPPED";
                udText.color = Swapped;
                // le frecce su/giù fanno mezzo giro e ora puntano al contrario
                for (float k = 0f; k < 1f; k += Time.deltaTime / 0.4f)
                {
                    arrows[2].transform.localRotation = Quaternion.Euler(0, 0, 90f + 180f * k);
                    arrows[3].transform.localRotation = Quaternion.Euler(0, 0, 270f + 180f * k);
                    yield return null;
                }
                arrows[2].transform.localRotation = Quaternion.Euler(0, 0, 270f);
                arrows[3].transform.localRotation = Quaternion.Euler(0, 0, 90f);
            }
            busy = false;
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
