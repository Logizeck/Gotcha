using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 81 — il funambolo. Cammina da solo sul filo sopra il burrone, ma è instabile come un vero equilibrista:
    /// appena pende da una parte tende a cadere di lì. Inclina il telefono dalla parte OPPOSTA per riportarlo dritto.
    /// Raffiche di vento lo spingono all'improvviso. Se pende troppo, splash nel fiume. (Editor: frecce.)
    /// </summary>
    public class TightropeLevel : LevelController
    {
        [SerializeField] Transform acrobat;            // pivot ai piedi
        [SerializeField] Transform startPoint, endPoint;
        [SerializeField] float crossSeconds = 16f;
        [SerializeField] float instability = 2.4f;     // quanto in fretta cade se pende
        [SerializeField] float control = 7f;           // quanto conta l'inclinazione del telefono
        [SerializeField] float damping = 1.6f;
        [SerializeField] Vector2 gustEvery = new Vector2(2f, 4f);
        [SerializeField] float gustKick = 1.1f;
        [SerializeField] float fallAngle = 50f;
        [SerializeField] SmokeEmitter splash;
        [SerializeField] SpeechBubble bubble;

        readonly SteadyTilt steady = new SteadyTilt { smoothing = 0.08f, deadZone = 0.01f, shakeLevel = 99f };
        float angle, angVel, progress, gust;
        bool done;

        public override void OnLevelStart()
        {
            angle = Random.Range(-4f, 4f) * Mathf.Deg2Rad;
            gust = Random.Range(gustEvery.x, gustEvery.y);
            if (splash != null) splash.Stop();
            if (bubble != null) { bubble.Show("Keep me balanced!"); StartCoroutine(HideLater(1.8f)); }
            Place();
        }

        void Place()
        {
            acrobat.position = Vector3.Lerp(startPoint.position, endPoint.position, progress);
            acrobat.rotation = Quaternion.Euler(0, 0, -angle * Mathf.Rad2Deg);
        }

        void Update()
        {
            if (done || IsFinished) return;
            float dt = Time.deltaTime;
            float tilt = steady.Update(dt);
            // pendolo rovesciato: angle > 0 = pende a destra
            float acc = instability * Mathf.Sin(angle) + control * tilt - damping * angVel;
            gust -= dt;
            if (gust <= 0f)
            {
                gust = Random.Range(gustEvery.x, gustEvery.y);
                angVel += gustKick * (Random.value < 0.5f ? -1f : 1f);
                if (bubble != null && Random.value < 0.5f) { bubble.Show("Whoa! Wind!", true); StartCoroutine(HideLater(0.8f)); }
            }
            angVel += acc * dt;
            angle += angVel * dt;
            // cammina solo se è abbastanza dritto
            if (Mathf.Abs(angle) < 20f * Mathf.Deg2Rad) progress = Mathf.Min(1f, progress + dt / crossSeconds);
            Place();
            if (Mathf.Abs(angle) * Mathf.Rad2Deg > fallAngle) { StartCoroutine(Fall()); return; }
            if (progress >= 1f) StartCoroutine(Made());
        }

        IEnumerator Fall()
        {
            done = true;
            if (bubble != null) bubble.Show("AAAAAH!", true);
            Vector3 from = acrobat.position;
            float side = Mathf.Sign(angle);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.8f)
            {
                acrobat.position = from + new Vector3(side * k * 0.8f, -k * k * 4f, 0f);
                acrobat.rotation = Quaternion.Euler(0, 0, -side * (fallAngle + k * 160f));
                yield return null;
            }
            if (splash != null) { splash.transform.position = new Vector3(acrobat.position.x, -3.4f, 0f); splash.Play(); }
            if (bubble != null) bubble.Show("SPLASH!", true);
            yield return new WaitForSeconds(0.3f);
            if (splash != null) splash.Stop();
            acrobat.gameObject.SetActive(false);
            yield return new WaitForSeconds(0.9f);
            Fail();
        }

        IEnumerator Made()
        {
            done = true;
            acrobat.rotation = Quaternion.identity;
            if (bubble != null) bubble.Show("TA-DAAA!", true);
            Vector3 p = acrobat.position;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.5f) { acrobat.position = p + Vector3.up * Mathf.Sin(k * Mathf.PI) * 0.4f; yield return null; }
            acrobat.position = p;
            yield return new WaitForSeconds(1f);
            Win();
        }

        IEnumerator HideLater(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (bubble != null && !done) bubble.Hide();
        }
    }
}
