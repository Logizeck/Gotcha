using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 14 — il biscotto nel barattolo. Il topolino ha fame ma il biscotto è dentro un barattolo aperto solo in
    /// alto, sulla mensola. La gravità segue il telefono: bisogna capovolgerlo perché il biscotto esca dal barattolo,
    /// poi inclinarlo e raddrizzarlo per farlo cadere dal topolino. (Nell'Editor: frecce.)
    /// Il biscotto è fragile: se sbatte troppo forte (telefono capovolto di colpo, caduta da troppo in alto)
    /// si spezza e il topolino resta a bocca asciutta.
    /// </summary>
    public class CookieLevel : LevelController
    {
        [SerializeField] Rigidbody2D cookie;
        [SerializeField] Transform mouse;
        [SerializeField] SmokeEmitter hearts;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] float eatDistance = 0.65f;
        [Tooltip("Resistenza dell'aria: il biscotto non accelera all'infinito.")]
        [SerializeField] float airDrag = 0.35f;
        [Tooltip("Attrito di rotolamento: un biscotto non gira come una ruota di bicicletta.")]
        [SerializeField] float rollingDrag = 1.2f;
        [Tooltip("Velocità d'urto oltre la quale il biscotto si spezza.")]
        [SerializeField] float breakImpact = 10.5f;
        [SerializeField] GameObject brokenCookie;
        [SerializeField] SmokeEmitter crumbsBurst;

        bool done;
        Vector2 lastPos;

        public override void OnLevelStart()
        {
            if (cookie != null) lastPos = cookie.position;
            if (brokenCookie != null) brokenCookie.SetActive(false);
            ImpactCrumbs impact = cookie != null ? cookie.GetComponent<ImpactCrumbs>() : null;
            if (impact != null) impact.Hit += OnHit;
            if (bubble != null) { bubble.Show("Cookie..."); StartCoroutine(HideLater(2f)); }
        }

        void Update()
        {
            if (done || IsFinished || cookie == null) return;
            if (Vector2.Distance(cookie.position, (Vector2)mouse.position + new Vector2(0f, 0.2f)) < eatDistance)
            {
                done = true;
                StartCoroutine(Eat());
            }
        }

        void FixedUpdate()
        {
            if (done || cookie == null || !cookie.simulated) return;
            // velocità ricavata dalla posizione (le API della velocità cambiano nome tra Unity 2022 e Unity 6)
            Vector2 v = (cookie.position - lastPos) / Time.fixedDeltaTime;
            lastPos = cookie.position;
            cookie.AddForce(-v * airDrag * cookie.mass);
            cookie.angularVelocity *= Mathf.Exp(-rollingDrag * Time.fixedDeltaTime);
        }

        void OnHit(float speed, Vector2 at)
        {
            if (done || IsFinished || speed < breakImpact) return;
            done = true;
            StartCoroutine(Break(at));
        }

        IEnumerator Break(Vector2 at)
        {
            Haptics.Medium();
            cookie.simulated = false;
            cookie.gameObject.SetActive(false);
            if (brokenCookie != null)
            {
                brokenCookie.transform.position = new Vector3(cookie.position.x, cookie.position.y, 0f);
                brokenCookie.SetActive(true);
            }
            if (crumbsBurst != null) { crumbsBurst.transform.position = at; crumbsBurst.Play(); }
            if (bubble != null) bubble.Show("NOOO! My cookie!");
            yield return new WaitForSeconds(0.3f);
            if (crumbsBurst != null) crumbsBurst.Stop();
            yield return new WaitForSeconds(1.2f);
            Fail();
        }

        IEnumerator Eat()
        {
            cookie.simulated = false;
            Transform t = cookie.transform;
            Vector3 from = t.position, to = mouse.position + new Vector3(-0.1f, 0.25f, 0f);
            Vector3 scale = t.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.5f)
            {
                t.position = Vector3.Lerp(from, to, k);
                t.localScale = scale * (1f - k);
                yield return null;
            }
            t.gameObject.SetActive(false);
            if (hearts != null) hearts.Play();
            if (bubble != null) bubble.Show("Yum!");
            yield return new WaitForSeconds(1.3f);
            if (hearts != null) hearts.Stop();
            Win();
        }

        IEnumerator HideLater(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (bubble != null && !done) bubble.Hide();
        }
    }
}
