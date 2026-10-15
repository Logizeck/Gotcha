using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 26 — annaffia la piantina. Le gocce sono in un bicchiere sulla mensola e la gravità segue il telefono:
    /// bisogna inclinarlo (fino quasi a capovolgerlo) per versare l'acqua nel vaso. Bastano 18 gocce: la piantina
    /// fiorisce. Le gocce che cadono sul prato si perdono: se non ne restano abbastanza, si riprova.
    /// </summary>
    public class PlantLevel : LevelController
    {
        [SerializeField] Rigidbody2D[] drops;
        [SerializeField] Collider2D potZone;
        [SerializeField] float groundY = -3.2f;
        [SerializeField] Transform plantDry;
        [SerializeField] GameObject plantBloom;
        [SerializeField] SmokeEmitter sparkle;
        [SerializeField] TextMesh counter;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] int needed = 18;

        int absorbed, lost;
        bool done;

        public override void OnLevelStart()
        {
            if (plantBloom != null) plantBloom.SetActive(false);
            if (sparkle != null) sparkle.Stop();
            if (bubble != null) { bubble.Show("Water... please..."); StartCoroutine(HideLater(2f)); }
            UpdateCounter();
        }

        void Update()
        {
            if (done || IsFinished) return;
            foreach (Rigidbody2D d in drops)
            {
                if (d == null || !d.gameObject.activeSelf) continue;
                Vector2 p = d.position;
                if (potZone.OverlapPoint(p)) { d.gameObject.SetActive(false); absorbed++; Grow(); }
                else if (p.y < groundY) { d.gameObject.SetActive(false); lost++; }
            }
            UpdateCounter();
            if (absorbed >= needed) StartCoroutine(Bloom());
            else if (drops.Length - lost < needed) StartCoroutine(Dry());
        }

        void Grow()
        {
            if (plantDry == null) return;
            float k = Mathf.Clamp01(absorbed / (float)needed);
            plantDry.localScale = new Vector3(1f, 1f + k * 0.15f, 1f);
            plantDry.localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(0f, 12f, k));   // si raddrizza un po'
        }

        void UpdateCounter() { if (counter != null) counter.text = Mathf.Min(absorbed, needed) + " / " + needed; }

        IEnumerator Bloom()
        {
            done = true;
            if (plantDry != null) plantDry.gameObject.SetActive(false);
            if (plantBloom != null)
            {
                plantBloom.SetActive(true);
                Transform t = plantBloom.transform;
                Vector3 s = t.localScale;
                for (float k = 0f; k < 1f; k += Time.deltaTime / 0.5f) { t.localScale = new Vector3(s.x, s.y * Mathf.SmoothStep(0.2f, 1f, k), s.z); yield return null; }
                t.localScale = s;
            }
            if (sparkle != null) sparkle.Play();
            if (bubble != null) bubble.Show("Thank you!");
            yield return new WaitForSeconds(1.4f);
            if (sparkle != null) sparkle.Stop();
            Win();
        }

        IEnumerator Dry()
        {
            done = true;
            if (bubble != null) bubble.Show("Not enough water...");
            yield return new WaitForSeconds(1.2f);
            Fail();
        }

        IEnumerator HideLater(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (bubble != null && !done) bubble.Hide();
        }
    }
}
