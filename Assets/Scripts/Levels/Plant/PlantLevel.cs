using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 26 — annaffia la piantina con la fionda. Ci sono 10 gocce; ne bastano 5 nel vaso.
    /// Si afferra la goccia nella fionda e la si tira all'indietro (lontano dal vaso): più la tendi e più va lontano,
    /// l'angolo decide la traiettoria (mostrata a puntini). Al rilascio la goccia parte verso il vaso.
    /// </summary>
    public class PlantLevel : LevelController
    {
        [SerializeField] Rigidbody2D[] drops;
        [SerializeField] Transform pocket;           // punto di riposo della goccia nella fionda
        [SerializeField] Transform bandBack, bandFront;   // punte dei due rebbi (gli elastici partono da lì)
        [SerializeField] LineRenderer bands;
        [SerializeField] Transform[] aimDots;
        [SerializeField] Collider2D potZone;
        [SerializeField] float groundY = -3.1f;
        [SerializeField] float maxPull = 1.4f;
        [SerializeField] float power = 6.8f;
        [SerializeField] float grabRadius = 0.8f;
        [SerializeField] Transform plantDry;
        [SerializeField] GameObject plantBloom;
        [SerializeField] SmokeEmitter sparkle;
        [SerializeField] SmokeEmitter splash;
        [SerializeField] TextMesh counter;
        [SerializeField] TextMesh ammo;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] int needed = 5;

        int next, absorbed;
        Rigidbody2D current;
        bool aiming, flying, done;
        float flyTime;
        Camera cam;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            foreach (Rigidbody2D d in drops) d.gameObject.SetActive(false);
            if (plantBloom != null) plantBloom.SetActive(false);
            if (sparkle != null) sparkle.Stop();
            if (splash != null) splash.Stop();
            ShowAim(false);
            if (bubble != null) { bubble.Show("Water... please..."); StartCoroutine(HideLater(2f)); }
            Load();
        }

        /// <summary>Mette la goccia successiva nella fionda.</summary>
        void Load()
        {
            current = null;
            UpdateTexts();
            if (next >= drops.Length) return;
            current = drops[next++];
            current.gameObject.SetActive(true);
            current.bodyType = RigidbodyType2D.Kinematic;
            current.position = pocket.position;
            current.transform.position = pocket.position;
            DrawBands(pocket.position);
        }

        void Update()
        {
            if (done || IsFinished) return;
            if (flying) { Fly(); return; }
            if (current == null) return;

            if (PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out bool ended))
            {
                Vector2 w = PointerInput.ToWorld(cam, sp);
                if (began && Vector2.Distance(w, pocket.position) < grabRadius) aiming = true;
                if (aiming)
                {
                    Vector2 pull = Vector2.ClampMagnitude(w - (Vector2)pocket.position, maxPull);
                    Vector2 at = (Vector2)pocket.position + pull;
                    current.transform.position = at;
                    DrawBands(at);
                    ShowTrajectory(at, -pull * power);
                    if (ended)
                    {
                        aiming = false;
                        ShowAim(false);
                        if (pull.magnitude < 0.2f) { current.transform.position = pocket.position; DrawBands(pocket.position); }
                        else Launch(at, -pull * power);
                    }
                }
            }
            else if (aiming)
            {
                aiming = false;
                ShowAim(false);
            }
        }

        void Launch(Vector2 at, Vector2 velocity)
        {
            current.position = at;
            current.bodyType = RigidbodyType2D.Dynamic;
            current.AddForce(velocity * current.mass, ForceMode2D.Impulse);   // impulso = velocità iniziale
            flying = true;
            flyTime = 0f;
            DrawBands(pocket.position);   // l'elastico torna dritto
        }

        void Fly()
        {
            flyTime += Time.deltaTime;
            Vector2 p = current.position;
            if (potZone.OverlapPoint(p)) { StartCoroutine(Absorbed()); return; }
            if (p.y < groundY || Mathf.Abs(p.x) > 3.6f || flyTime > 4f) StartCoroutine(Missed(p));
        }

        IEnumerator Absorbed()
        {
            flying = false;
            current.gameObject.SetActive(false);
            absorbed++;
            Grow();
            UpdateTexts();
            if (sparkle != null) { sparkle.Play(); yield return new WaitForSeconds(0.2f); sparkle.Stop(); }
            if (absorbed >= needed) { yield return Bloom(); yield break; }
            yield return new WaitForSeconds(0.3f);
            Load();
        }

        IEnumerator Missed(Vector2 at)
        {
            flying = false;
            current.gameObject.SetActive(false);
            if (splash != null) { splash.transform.position = new Vector3(at.x, Mathf.Max(at.y, groundY), 0f); splash.Play(); }
            yield return new WaitForSeconds(0.15f);
            if (splash != null) splash.Stop();
            if (drops.Length - next + absorbed < needed)
            {
                done = true;
                UpdateTexts();
                if (bubble != null) bubble.Show("Not enough water...");
                yield return new WaitForSeconds(1.3f);
                Fail();
                yield break;
            }
            yield return new WaitForSeconds(0.3f);
            Load();
        }

        void Grow()
        {
            if (plantDry == null) return;
            float k = Mathf.Clamp01(absorbed / (float)needed);
            plantDry.localScale = new Vector3(1f, 1f + k * 0.15f, 1f);
            plantDry.localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(0f, 12f, k));   // si raddrizza un po'
        }

        void UpdateTexts()
        {
            if (counter != null) counter.text = Mathf.Min(absorbed, needed) + " / " + needed;
            if (ammo != null) ammo.text = "DROPS LEFT: " + (drops.Length - next + (current != null && !flying ? 1 : 0));
        }

        void DrawBands(Vector2 at)
        {
            if (bands == null) return;
            bands.positionCount = 3;
            bands.SetPosition(0, bandBack.position);
            bands.SetPosition(1, new Vector3(at.x, at.y, 0f));
            bands.SetPosition(2, bandFront.position);
        }

        void ShowTrajectory(Vector2 from, Vector2 v)
        {
            Vector2 g = Physics2D.gravity * (current != null ? current.gravityScale : 1f);
            for (int i = 0; i < aimDots.Length; i++)
            {
                float t = (i + 1) * 0.08f;
                Vector2 p = from + v * t + 0.5f * g * t * t;
                aimDots[i].position = new Vector3(p.x, p.y, 0f);
                aimDots[i].gameObject.SetActive(true);
            }
        }

        void ShowAim(bool on) { foreach (Transform d in aimDots) d.gameObject.SetActive(on); }

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

        IEnumerator HideLater(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (bubble != null && !done) bubble.Hide();
        }
    }
}
