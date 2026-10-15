using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 29 — giornata di sole. Il sole picchia sul pupazzo di neve, che comincia a sciogliersi davvero
    /// (stesso scioglimento dell'Easter egg del livello 3). Trascina la nuvola davanti al sole per fargli ombra;
    /// il vento però la sposta piano piano. Resisti fino al tramonto. Se si scioglie del tutto: Easter egg e
    /// video da condividere.
    /// </summary>
    public class SunnyLevel : LevelController
    {
        [SerializeField] Transform sun;
        [SerializeField] Draggable2D cloud;
        [SerializeField] SpriteRenderer sunbeam;
        [SerializeField] SpriteRenderer skyTint;
        [SerializeField] SnowmanMelter melter;
        [SerializeField] Transform[] looseParts;      // cappello, sciarpa, braccia, occhi, naso (non sono neve)
        [SerializeField] SpriteRenderer mouthSmile;
        [SerializeField] SpriteRenderer mouthO;
        [SerializeField] SmokeEmitter sweat;
        [SerializeField] TextMesh countdown;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] float coverRadius = 0.85f;
        [SerializeField] float meltSeconds = 9f;      // di sole pieno per sciogliersi del tutto
        [SerializeField] float sunsetAfter = 14f;
        [SerializeField] float wind = 0.32f;
        [SerializeField] string eggId = "sun_melt";

        float exposure, timeLeft;
        bool over;

        public override void OnLevelStart()
        {
            timeLeft = sunsetAfter;
            if (mouthO != null) mouthO.gameObject.SetActive(false);
            if (skyTint != null) skyTint.color = new Color(0.1f, 0.1f, 0.3f, 0f);
            foreach (Transform t in looseParts) melter.Attach(t);
            if (mouthO != null) melter.Attach(mouthO.transform, true);
            if (mouthSmile != null) melter.Attach(mouthSmile.transform, true);
            melter.Begin();
            if (bubble != null) { bubble.Show("It's sooo hot!"); StartCoroutine(HideLater(2f)); }
            if (ClipRecorder.Instance != null) ClipRecorder.Instance.Begin();
        }

        void OnDestroy()
        {
            if (ClipRecorder.Instance != null && ClipRecorder.Instance.IsCapturing) ClipRecorder.Instance.Discard();
        }

        void Update()
        {
            if (over || IsFinished) return;
            // il vento sposta la nuvola quando non la stai tenendo
            if (Draggable2D.Active != cloud)
            {
                Vector3 p = cloud.transform.position;
                p.x -= wind * Time.deltaTime;
                if (p.x < -4f) p.x = 4f;   // rientra dall'altra parte
                cloud.transform.position = p;
            }
            bool covered = Vector2.Distance(cloud.transform.position, sun.position) < coverRadius;
            if (sunbeam != null) sunbeam.enabled = !covered;
            if (melter.drips != null) { if (covered) melter.drips.Stop(); else melter.drips.Play(); }
            if (melter.steam != null) { if (covered) melter.steam.Stop(); else melter.steam.Play(); }
            if (sweat != null) { if (covered) sweat.Stop(); else sweat.Play(); }
            if (!covered)
            {
                exposure += Time.deltaTime / meltSeconds;
                melter.SetProgress(exposure * exposure * 0.6f + exposure * 0.4f);
            }
            // a metà scioglimento il sorriso diventa una "O" di terrore
            if (exposure > 0.12f && mouthO != null && !mouthO.gameObject.activeSelf)
            {
                mouthO.gameObject.SetActive(true);
                if (mouthSmile != null) mouthSmile.gameObject.SetActive(false);
                if (bubble != null) { bubble.Show("I'm melting!"); StartCoroutine(HideLater(1.5f)); }
            }

            timeLeft -= Time.deltaTime;
            if (countdown != null) countdown.text = "Sunset in " + Mathf.CeilToInt(Mathf.Max(0f, timeLeft));
            if (melter.Progress >= 1f) StartCoroutine(Melted());
            else if (timeLeft <= 0f) StartCoroutine(Sunset());
        }

        IEnumerator Sunset()
        {
            over = true;
            melter.End();
            if (sweat != null) sweat.Stop();
            if (countdown != null) countdown.text = "";
            cloud.enabled = false;
            Vector3 from = sun.position;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 1.4f)
            {
                sun.position = from + Vector3.down * (k * k * 6f);
                if (skyTint != null) skyTint.color = new Color(0.1f, 0.1f, 0.3f, k * 0.45f);
                yield return null;
            }
            if (ClipRecorder.Instance != null) ClipRecorder.Instance.Discard();
            if (bubble != null) bubble.Show("Ahh... nice and cold!");
            yield return new WaitForSeconds(1.4f);
            Win();
        }

        IEnumerator Melted()
        {
            over = true;
            melter.End();
            if (sweat != null) sweat.Stop();
            if (bubble != null) bubble.Show("blub...");
            yield return new WaitForSeconds(1.5f);
            if (ClipRecorder.Instance != null) ClipRecorder.Instance.Stop();
            FailWithEgg(eggId);
        }

        IEnumerator HideLater(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (bubble != null && !over) bubble.Hide();
        }
    }
}
