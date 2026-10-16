using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 82 — le ombre cinesi. Sul muro c'è l'ombra di un oggetto; l'oggetto vero è in mano tua ma storto,
    /// troppo grande o troppo piccolo. Con UN dito lo sposti, con DUE dita lo ruoti (girandole) e lo ingrandisci o
    /// rimpicciolisci (allargandole). Quando combacia con l'ombra per un attimo, passa al prossimo. Tre oggetti.
    /// (Editor: trascina col mouse, Q/E ruotano, Z/X scalano.)
    /// </summary>
    public class ShadowLevel : LevelController
    {
        [SerializeField] SpriteRenderer shadow;
        [SerializeField] SpriteRenderer item;
        [SerializeField] Sprite[] objects;
        [SerializeField] TextMesh counter;
        [SerializeField] Transform timeBar;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] float angleTolerance = 12f, scaleTolerance = 0.13f, posTolerance = 0.3f;
        [SerializeField] float seconds = 70f;

        int round;
        float hold, clock;
        bool done, busy;
        Camera cam;
        Vector2 lastA, lastB, lastOne;
        bool twoDown, oneDown;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            Setup();
            if (bubble != null) { bubble.Show("Match the shadow! Two fingers twist and pinch."); StartCoroutine(HideLater(2.4f)); }
        }

        void Setup()
        {
            counter.text = (round + 1) + " / " + objects.Length;
            shadow.sprite = item.sprite = objects[round];
            shadow.transform.localPosition = new Vector3(Random.Range(-0.8f, 0.8f), Random.Range(1.2f, 2.2f), 0f);
            shadow.transform.localRotation = Quaternion.Euler(0, 0, Random.Range(-60f, 60f));
            shadow.transform.localScale = Vector3.one * Random.Range(1.6f, 2.4f);
            item.transform.localPosition = new Vector3(Random.Range(-1f, 1f), -2.2f, 0f);
            item.transform.localRotation = Quaternion.Euler(0, 0, shadow.transform.localEulerAngles.z + Random.Range(70f, 150f) * (Random.value < 0.5f ? -1f : 1f));
            item.transform.localScale = shadow.transform.localScale * (Random.value < 0.5f ? 0.55f : 1.5f);
            hold = 0f;
        }

        void Update()
        {
            if (done || busy || IsFinished) return;
            clock += Time.deltaTime;
            if (timeBar != null) timeBar.localScale = new Vector3(Mathf.Clamp01(1f - clock / seconds), 1f, 1f);
            if (clock >= seconds) { StartCoroutine(Lost()); return; }
            Transform t = item.transform;
            if (Input.touchCount >= 2)
            {
                Vector2 a = cam.ScreenToWorldPoint(Input.GetTouch(0).position), b = cam.ScreenToWorldPoint(Input.GetTouch(1).position);
                if (twoDown)
                {
                    float rot = Vector2.SignedAngle(lastB - lastA, b - a);
                    float sc = (b - a).magnitude / Mathf.Max(0.05f, (lastB - lastA).magnitude);
                    t.Rotate(0, 0, rot);
                    t.localScale = Vector3.one * Mathf.Clamp(t.localScale.x * sc, 0.5f, 4f);
                    t.position += (Vector3)(((a + b) - (lastA + lastB)) * 0.5f);
                }
                lastA = a; lastB = b; twoDown = true; oneDown = false;
            }
            else
            {
                twoDown = false;
                if (PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out _) && !PointerInput.IsOverUi())
                {
                    Vector2 w = PointerInput.ToWorld(cam, sp);
                    if (oneDown && !began) t.position += (Vector3)(w - lastOne);
                    lastOne = w; oneDown = true;
                }
                else oneDown = false;
            }
#if UNITY_EDITOR
            if (Input.GetKey(KeyCode.Q)) t.Rotate(0, 0, 90f * Time.deltaTime);
            if (Input.GetKey(KeyCode.E)) t.Rotate(0, 0, -90f * Time.deltaTime);
            if (Input.GetKey(KeyCode.X)) t.localScale *= 1f + Time.deltaTime;
            if (Input.GetKey(KeyCode.Z)) t.localScale *= 1f - Time.deltaTime;
#endif
            float da = Mathf.Abs(Mathf.DeltaAngle(t.eulerAngles.z, shadow.transform.eulerAngles.z));
            float ds = Mathf.Abs(t.localScale.x / shadow.transform.localScale.x - 1f);
            float dp = Vector2.Distance(t.position, shadow.transform.position);
            bool match = da < angleTolerance && ds < scaleTolerance && dp < posTolerance;
            shadow.color = match ? new Color(0.2f, 0.5f, 0.25f, 0.8f) : new Color(0.08f, 0.06f, 0.12f, 0.75f);
            hold = match ? hold + Time.deltaTime : 0f;
            if (hold > 0.4f) StartCoroutine(Matched());
        }

        IEnumerator Matched()
        {
            busy = true;
            Sfx.Play(this, "ding", 0.6f);
            Transform t = item.transform, s = shadow.transform;
            Vector3 p = t.position; Quaternion r = t.rotation; Vector3 sc = t.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.25f)
            {
                t.position = Vector3.Lerp(p, s.position, k); t.rotation = Quaternion.Slerp(r, s.rotation, k); t.localScale = Vector3.Lerp(sc, s.localScale, k);
                yield return null;
            }
            yield return new WaitForSeconds(0.5f);
            round++;
            if (round >= objects.Length)
            {
                done = true;
                if (bubble != null) bubble.Show("Shadow master!");
                yield return new WaitForSeconds(1.4f);
                Win();
                yield break;
            }
            Setup();
            busy = false;
        }

        IEnumerator Lost()
        {
            done = true;
            if (bubble != null) bubble.Show("The lamp burned out!");
            yield return new WaitForSeconds(1.4f);
            Fail();
        }

        IEnumerator HideLater(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (bubble != null && !done) bubble.Hide();
        }
    }
}
