using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 39 — la pista per la biglia. Disegna col dito fino a 3 rampe, poi premi GO: la biglia parte dalla
    /// mensola in alto e deve finire nella tazza, oltre il muretto. Se cade fuori torna al via (le rampe restano):
    /// si corregge e si riprova quante volte si vuole. Più a lungo si tiene premuto GO, più forte è la spinta
    /// (la barra accanto al pulsante si riempie in 1.2 secondi).
    /// </summary>
    public class MarbleRunLevel : LevelController
    {
        [SerializeField] Rigidbody2D marble;
        [SerializeField] Collider2D goButton;
        [SerializeField] Transform goVisual;
        [SerializeField] Transform powerFill;        // barra della spinta (scala x da 0 a 1)
        [SerializeField] float minPush = 1.5f;       // velocità di partenza con un tocco
        [SerializeField] float maxPush = 6.5f;       // velocità con la barra piena
        [SerializeField] float chargeSeconds = 1.2f;
        [SerializeField] float direction = 1f;       // 1 = verso destra, -1 = verso sinistra (variante specchiata)
        [SerializeField] Collider2D cupZone;
        [SerializeField] float lostBelow = -3.0f;   // sotto il piano di lavoro, fuori dalla tazza = mancata
        [SerializeField] SmokeEmitter confetti;
        [SerializeField] SpeechBubble bubble;

        Vector3 start;
        bool rolling, done;
        float inCup, rollTime, charge, still;
        Vector2 last;
        bool charging;
        Camera cam;
        Vector3 goScale;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            start = marble.transform.position;
            if (goVisual != null) goScale = goVisual.localScale;
            ShowPower(0f);
            Hold();
            if (confetti != null) confetti.Stop();
            if (bubble != null) { bubble.Show("Draw a path, then GO!"); StartCoroutine(HideLater(2f)); }
        }

        void Hold()
        {
            marble.bodyType = RigidbodyType2D.Kinematic;
            marble.transform.position = start;
            marble.position = start;
            marble.angularVelocity = 0f;
#if UNITY_6000_0_OR_NEWER
            marble.linearVelocity = Vector2.zero;
#else
            marble.velocity = Vector2.zero;
#endif
            rolling = false;
        }

        void Launch(float power)
        {
            Hold();
            marble.bodyType = RigidbodyType2D.Dynamic;
            Vector2 v = new Vector2(Mathf.Lerp(minPush, maxPush, power) * direction, 0f);
#if UNITY_6000_0_OR_NEWER
            marble.linearVelocity = v;
#else
            marble.velocity = v;
#endif
            marble.WakeUp();
            rolling = true;
            still = 0f;
            last = marble.position;
            rollTime = 0f;
            inCup = 0f;
        }

        void ShowPower(float k)
        {
            if (powerFill != null) powerFill.localScale = new Vector3(k, 1f, 1f);
        }

        /// <summary>GO: si carica tenendo premuto, parte al rilascio.</summary>
        void ChargeGo()
        {
            if (!PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out bool ended))
            {
                if (charging) Release();
                return;
            }
            if (began && goButton != null && goButton.OverlapPoint(PointerInput.ToWorld(cam, sp))) { charging = true; charge = 0f; }
            if (!charging) return;
            charge = Mathf.Min(1f, charge + Time.deltaTime / chargeSeconds);
            ShowPower(charge);
            if (goVisual != null) goVisual.localScale = new Vector3(goScale.x * 1.08f, goScale.y * (0.85f - charge * 0.1f), goScale.z);
            if (ended) Release();
        }

        void Release()
        {
            charging = false;
            if (goVisual != null) goVisual.localScale = goScale;
            Launch(charge);
        }

        void Update()
        {
            if (done || IsFinished) return;
            ChargeGo();
            if (!rolling) return;
            rollTime += Time.deltaTime;
            Vector2 p = marble.position;
            bool cupped = cupZone.OverlapPoint(p);
            inCup = cupped ? inCup + Time.deltaTime : 0f;
            if (inCup > 0.5f) { StartCoroutine(Score()); return; }
            // ferma fuori dalla tazza (spinta troppo debole o incastrata su una rampa): torna al via
            still = !cupped && (p - last).magnitude < 0.003f ? still + Time.deltaTime : 0f;
            last = p;
            if (still > 1.2f)
            {
                Hold();
                if (bubble != null) { bubble.Show("Stuck! Push harder?"); StartCoroutine(HideLater(1.4f)); }
                return;
            }
            if ((p.y < lostBelow && !cupped) || Mathf.Abs(p.x) > 3.4f || rollTime > 12f)
            {
                Hold();
                if (bubble != null) { bubble.Show("Missed! Fix the path."); StartCoroutine(HideLater(1.4f)); }
            }
        }

        IEnumerator Score()
        {
            done = true;
            marble.bodyType = RigidbodyType2D.Kinematic;   // ferma nella tazza: niente più discesa
#if UNITY_6000_0_OR_NEWER
            marble.linearVelocity = Vector2.zero;
#else
            marble.velocity = Vector2.zero;
#endif
            marble.angularVelocity = 0f;
            if (confetti != null) confetti.Play();
            if (bubble != null) bubble.Show("Hole in one!");
            yield return new WaitForSeconds(1.5f);
            if (confetti != null) confetti.Stop();
            Win();
        }

        IEnumerator HideLater(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (bubble != null && !done) bubble.Hide();
        }
    }
}
