using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 62 — il ponte. Il topolino è su una rupe, il formaggio sull'altra, in mezzo il burrone col fiume.
    /// Col dito si disegna un ponte (inchiostro poco: la barra in alto), poi GO: il topolino cammina dritto.
    /// Se il ponte non arriva, ha un buco o è troppo ripido... splash nel fiume: fallito. Arriva al formaggio: vinto.
    /// I piccoli dislivelli del disegno non sono un problema: il topolino li supera con un saltello.
    /// </summary>
    public class BridgeLevel : LevelController
    {
        [SerializeField] Rigidbody2D mouse;
        [SerializeField] Transform mouseVisual;
        [SerializeField] StrokeDrawer pen;
        [SerializeField] TapButton go;
        [SerializeField] Transform cheese;
        [SerializeField] Transform inkBar;
        [SerializeField] float inkBudget = 3.6f;
        [SerializeField] float walkSpeed = 1.1f;
        [SerializeField] float walkForce = 14f;
        [SerializeField] float fallBelow = -3.3f;
        [Tooltip("Gradino massimo che il topolino supera con un saltello (unità di gioco).")]
        [SerializeField] float hopHeight = 0.28f;
        [SerializeField] float hopCooldown = 0.3f;
        [SerializeField] SmokeEmitter splash;
        [SerializeField] SmokeEmitter hearts;
        [SerializeField] SpeechBubble bubble;

        bool walking, done;
        float stuck, step;
        Vector2 lastPos;
        Vector3 visualHome;

        public override void OnLevelStart()
        {
            mouse.bodyType = RigidbodyType2D.Kinematic;
            if (mouseVisual != null) visualHome = mouseVisual.localPosition;
            if (splash != null) splash.Stop();
            if (hearts != null) hearts.Stop();
            go.Pressed += OnGo;
            if (bubble != null) { bubble.Show("Build me a bridge!"); StartCoroutine(HideLater(2f)); }
        }

        void OnDestroy() { if (go != null) go.Pressed -= OnGo; }

        float InkUsed()
        {
            float used = 0f;
            foreach (Transform stroke in pen.transform)
            {
                EdgeCollider2D e = stroke.GetComponent<EdgeCollider2D>();
                if (e == null) continue;
                for (int i = 1; i < e.points.Length; i++) used += Vector2.Distance(stroke.TransformPoint(e.points[i - 1]), stroke.TransformPoint(e.points[i]));
            }
            return used;
        }

        void OnGo(TapButton b)
        {
            if (walking || done) return;
            if (!pen.HasStroke) { if (bubble != null) { bubble.Show("No bridge, no way!"); StartCoroutine(HideLater(1.4f)); } return; }
            walking = true;
            pen.enabled = false;   // niente ritocchi a ponte "in uso"
            mouse.bodyType = RigidbodyType2D.Dynamic;
            lastPos = mouse.position;
            lastFixed = mouse.position.x;
            progressX = mouse.position.x;
            if (bubble != null) bubble.Hide();
        }

        void Update()
        {
            if (done || IsFinished) return;
            if (!walking)
            {
                float left = Mathf.Max(0f, inkBudget - InkUsed());
                pen.maxStrokeLength = Mathf.Max(0.05f, left);
                if (inkBar != null) inkBar.localScale = new Vector3(Mathf.Clamp01((left - (pen.IsDrawing ? pen.CurrentLength : 0f)) / inkBudget), 1f, 1f);
                return;
            }
            // passettini
            hopTimer -= Time.deltaTime;
            step += Time.deltaTime * 14f;
            if (mouseVisual != null) mouseVisual.localPosition = visualHome + Vector3.up * Mathf.Abs(Mathf.Sin(step)) * 0.05f;
            Vector2 p = mouse.position;
            if (p.y < fallBelow) { StartCoroutine(Splash()); return; }
            if (p.x > cheese.position.x - 0.35f) { StartCoroutine(Yum()); return; }
            // bloccato = non avanza in orizzontale (i saltelli sul posto non contano come progresso)
            if (p.x > progressX + 0.05f) { progressX = p.x; stuck = 0f; }
            else stuck += Time.deltaTime;
            lastPos = p;
            if (stuck > 2f) StartCoroutine(TooSteep());
        }

        void FixedUpdate()
        {
            if (!walking || done) return;
            // spinge in avanti solo finché non va già alla sua velocità: in salita ripida non ce la fa
            float vx = (mouse.position.x - lastFixed) / Time.fixedDeltaTime;
            lastFixed = mouse.position.x;
            if (vx < walkSpeed) mouse.AddForce(Vector2.right * walkForce * mouse.mass);
            // un gradino o un dislivello lo ferma: saltello di qualche pixel e riprova
            bool grounded = Mathf.Abs(Velocity.y) < 0.25f;
            blocked = vx < 0.15f && grounded ? blocked + Time.fixedDeltaTime : 0f;
            if (blocked > 0.12f && hopTimer <= 0f)
            {
                float g = Mathf.Abs(Physics2D.gravity.y) * mouse.gravityScale;
                Velocity = new Vector2(walkSpeed * 0.8f, Mathf.Sqrt(2f * g * hopHeight));
                hopTimer = hopCooldown;
                blocked = 0f;
                Sfx.Play(this, "pop", 0.3f);
            }
        }

        float lastFixed, blocked, hopTimer, progressX = float.MinValue;

        Vector2 Velocity
        {
#if UNITY_6000_0_OR_NEWER
            get => mouse.linearVelocity;
            set => mouse.linearVelocity = value;
#else
            get => mouse.velocity;
            set => mouse.velocity = value;
#endif
        }

        IEnumerator Splash()
        {
            done = true;
            if (splash != null) { splash.transform.position = new Vector3(mouse.position.x, fallBelow, 0f); splash.Play(); }
            if (bubble != null) bubble.Show("BLUB!");
            yield return new WaitForSeconds(0.3f);
            if (splash != null) splash.Stop();
            mouse.gameObject.SetActive(false);
            yield return new WaitForSeconds(1.1f);
            Fail();
        }

        IEnumerator TooSteep()
        {
            done = true;
            if (bubble != null) bubble.Show("Too steep... I give up!");
            yield return new WaitForSeconds(1.4f);
            Fail();
        }

        IEnumerator Yum()
        {
            done = true;
            mouse.bodyType = RigidbodyType2D.Kinematic;
            if (hearts != null) { hearts.transform.position = cheese.position + Vector3.up * 0.5f; hearts.Play(); }
            if (bubble != null) bubble.Show("CHEESE!!");
            Vector3 s = cheese.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.8f) { cheese.localScale = s * (1f - k); yield return null; }
            cheese.gameObject.SetActive(false);
            yield return new WaitForSeconds(0.8f);
            if (hearts != null) hearts.Stop();
            Win();
        }

        IEnumerator HideLater(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (bubble != null && !done && !walking) bubble.Hide();
        }
    }
}
