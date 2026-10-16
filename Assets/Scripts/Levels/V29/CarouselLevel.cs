using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 88 — la giostra. Il gatto vuole fare TRE giri di giostra, ma la giostra gira solo se giri TU: tieni il
    /// telefono e gira su te stesso (giroscopio). Dopo tre giri fermati: la giostra si ferma e il gatto scende felice.
    /// Easter egg `dizzy_cat`: continua a girare oltre i cinque giri... (Senza giroscopio, e nell'Editor: fai girare
    /// il dito attorno alla giostra; Q/E.)
    /// </summary>
    public class CarouselLevel : LevelController
    {
        [SerializeField] Transform disk;              // ruota con l'angolo accumulato
        [SerializeField] Transform cat;
        [SerializeField] TextMesh counter;
        [SerializeField] TextMesh prompt;
        [SerializeField] GameObject dizzyEyes;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] int turnsNeeded = 3, dizzyTurns = 5;
        [SerializeField] float stopRate = 35f, stopHold = 1.0f;
        [SerializeField] string eggId = "dizzy_cat";

        float angle, still, lastFinger;
        bool fingerSpin, done;
        Camera cam;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            Input.gyro.enabled = SystemInfo.supportsGyroscope;
            dizzyEyes.SetActive(false);
            prompt.text = "SPIN AROUND!";
            Count();
            if (bubble != null) { bubble.Show("Three rides! Turn around with the phone!"); StartCoroutine(HideLater(2.4f)); }
        }

        float Turns => Mathf.Abs(angle) / 360f;
        void Count() { counter.text = Turns.ToString("0.0") + " / " + turnsNeeded; }

        void Update()
        {
            if (done || IsFinished) return;
            float dt = Time.deltaTime, rate = 0f;   // gradi al secondo attorno alla verticale
            if (SystemInfo.supportsGyroscope && Input.gyro.enabled)
            {
                Vector3 g = Input.gyro.gravity;
                if (g.sqrMagnitude > 0.01f) rate = -Vector3.Dot(Input.gyro.rotationRateUnbiased, g.normalized) * Mathf.Rad2Deg;
            }
            // alternativa: far girare il dito attorno alla giostra
            if (PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out _))
            {
                Vector2 w = PointerInput.ToWorld(cam, sp) - (Vector2)disk.position;
                float a = Mathf.Atan2(w.y, w.x) * Mathf.Rad2Deg;
                if (fingerSpin && !began) rate = Mathf.DeltaAngle(lastFinger, a) / Mathf.Max(dt, 0.001f);
                lastFinger = a;
                fingerSpin = true;
            }
            else fingerSpin = false;
#if UNITY_EDITOR
            if (Input.GetKey(KeyCode.Q)) rate = 240f;
            if (Input.GetKey(KeyCode.E)) rate = -240f;
#endif
            angle += rate * dt;
            disk.localRotation = Quaternion.Euler(0, 0, angle);
            cat.rotation = Quaternion.identity;   // il gatto resta dritto mentre gira con la giostra
            Count();

            if (Turns >= dizzyTurns) { StartCoroutine(Dizzy()); return; }
            if (Turns >= turnsNeeded)
            {
                prompt.text = "NOW STOP!";
                still = Mathf.Abs(rate) < stopRate ? still + dt : 0f;
                if (still > stopHold) StartCoroutine(Ride());
            }
            else prompt.text = "SPIN AROUND!";
        }

        IEnumerator Ride()
        {
            done = true;
            Sfx.Play(this, "ding", 0.8f);
            if (bubble != null) bubble.Show("Wheee! Best ride ever!");
            yield return new WaitForSeconds(1.5f);
            Win();
        }

        /// <summary>Easter egg: troppi giri, il gatto ha gli occhi a spirale (e anche tu, probabilmente).</summary>
        IEnumerator Dizzy()
        {
            done = true;
            dizzyEyes.SetActive(true);
            prompt.text = "";
            if (bubble != null) bubble.Show("Mrrow... the room is spinning...");
            Transform cam0 = Camera.main != null ? Camera.main.transform : null;
            Quaternion rot0 = cam0 != null ? cam0.rotation : Quaternion.identity;
            for (float k = 0f; k < 2f; k += Time.deltaTime)
            {
                if (cam0 != null) cam0.rotation = Quaternion.Euler(0, 0, Mathf.Sin(k * 6f) * 6f * (2f - k));
                cat.rotation = Quaternion.Euler(0, 0, Mathf.Sin(k * 9f) * 15f);
                yield return null;
            }
            if (cam0 != null) cam0.rotation = rot0;
            FailWithEgg(eggId);
        }

        IEnumerator HideLater(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (bubble != null && !done) bubble.Hide();
        }
    }
}
