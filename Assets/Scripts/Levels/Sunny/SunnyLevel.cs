using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 29 — giornata di sole. Il sole picchia sul pupazzo di neve: suda, arrossisce, si affloscia un po'.
    /// Trascina la nuvola davanti al sole per fargli ombra; il vento però la sposta piano piano. Se il caldo arriva
    /// al limite, parte la stessa tragedia dell'Easter egg del livello 11 (urlo, occhi che schizzano, scioglimento
    /// completo): Easter egg e video da condividere. Se resisti fino al tramonto, il sole scende dietro le colline,
    /// sorge la luna e il pupazzo è salvo.
    /// </summary>
    public class SunnyLevel : LevelController
    {
        [SerializeField] Transform sun;
        [SerializeField] Transform moon;
        [SerializeField] Draggable2D cloud;
        [SerializeField] SpriteRenderer sunbeam;
        [SerializeField] SpriteRenderer skyTint;
        [SerializeField] SnowmanMelter melter;
        [SerializeField] SnowmanMeltdown meltdown;
        [SerializeField] Transform[] eyes;
        [SerializeField] Transform[] attached;         // naso e altri oggetti non di neve
        [SerializeField] SpriteRenderer mouthSmile;
        [SerializeField] SpriteRenderer mouthO;
        [SerializeField] SmokeEmitter sweat;
        [SerializeField] TextMesh countdown;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] float coverRadius = 0.85f;
        [SerializeField] float heatSeconds = 5f;      // di sole pieno prima del punto di non ritorno
        [SerializeField] float sunsetAfter = 14f;
        [SerializeField] float wind = 0.32f;
        [SerializeField] float horizonY = -4.2f;      // dove spariscono sole e luna (dietro le colline)
        [SerializeField] string eggId = "sun_melt";

        float heat, timeLeft;
        bool over, worried;
        Vector3 moonUp;

        public override void OnLevelStart()
        {
            timeLeft = sunsetAfter;
            if (mouthO != null) mouthO.gameObject.SetActive(false);
            if (skyTint != null) skyTint.color = new Color(0.1f, 0.1f, 0.3f, 0f);
            if (moon != null) { moonUp = moon.localPosition; moon.localPosition = new Vector3(moonUp.x, horizonY, moonUp.z); }
            // tutto ciò che non è neve segue la neve fin da subito (la testa si affloscia appena col caldo)
            foreach (Transform t in new[] { meltdown.hat, meltdown.scarf, meltdown.armLeft, meltdown.armRight }) if (t != null) melter.Attach(t);
            foreach (Transform t in eyes) melter.Attach(t);
            foreach (Transform t in attached) melter.Attach(t);
            if (mouthSmile != null) melter.Attach(mouthSmile.transform, true);
            if (mouthO != null) melter.Attach(mouthO.transform, true);
            melter.Begin();
            if (melter.drips != null) melter.drips.Stop();
            if (melter.steam != null) melter.steam.Stop();
            if (bubble != null) { bubble.Show("It's sooo hot!"); StartCoroutine(HideLater(2f)); }
        }

        void OnDestroy()
        {
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
            if (sweat != null) { if (covered) sweat.Stop(); else sweat.Play(); }
            if (melter.drips != null) { if (covered || heat < 0.3f) melter.drips.Stop(); else melter.drips.Play(); }
            if (!covered)
            {
                heat += Time.deltaTime / heatSeconds;
                melter.SetProgress(heat * 0.1f);   // si affloscia appena: il vero scioglimento è la tragedia finale
                if (melter.body != null) melter.body.color = Color.Lerp(Color.white, new Color(1f, 0.82f, 0.85f), heat);
            }
            if (!worried && heat > 0.45f)
            {
                worried = true;
                if (mouthO != null) mouthO.gameObject.SetActive(true);
                if (mouthSmile != null) mouthSmile.gameObject.SetActive(false);
                if (bubble != null) { bubble.Show("Help! I'm getting soft!"); StartCoroutine(HideLater(1.5f)); }
            }

            timeLeft -= Time.deltaTime;
            if (countdown != null) countdown.text = "Sunset in " + Mathf.CeilToInt(Mathf.Max(0f, timeLeft));
            if (heat >= 1f) StartCoroutine(Meltdown());
            else if (timeLeft <= 0f) StartCoroutine(Sunset());
        }

        IEnumerator Meltdown()
        {
            over = true;
            cloud.enabled = false;
            if (countdown != null) countdown.text = "";
            if (sweat != null) sweat.Stop();
            if (meltdown != null)
            {
                // con un limite di tempo: anche se l'animazione si inceppasse, l'Easter egg viene comunque assegnato
                StartCoroutine(meltdown.Play(new List<Transform>(eyes), new List<Transform>(attached)));
                yield return null;
                for (float t = 0f; t < 10f && meltdown.Running; t += Time.deltaTime) yield return null;
            }
            FailWithEgg(eggId);
        }

        IEnumerator Sunset()
        {
            over = true;
            if (melter.drips != null) melter.drips.Stop();
            if (sweat != null) sweat.Stop();
            if (sunbeam != null) sunbeam.enabled = false;
            if (countdown != null) countdown.text = "";
            cloud.enabled = false;
            // il sole scende dietro le colline, il cielo si scalda e poi si fa notte, sorge la luna
            Vector3 from = sun.localPosition, to = new Vector3(from.x + 0.4f, horizonY, from.z);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 2.2f)
            {
                float e = k * k * (3f - 2f * k);
                sun.localPosition = Vector3.Lerp(from, to, e);
                if (skyTint != null) skyTint.color = k < 0.5f ? new Color(1f, 0.5f, 0.2f, k * 0.5f) : Color.Lerp(new Color(1f, 0.5f, 0.2f, 0.25f), new Color(0.06f, 0.07f, 0.25f, 0.5f), (k - 0.5f) * 2f);
                if (moon != null && k > 0.45f) moon.localPosition = Vector3.Lerp(new Vector3(moonUp.x, horizonY, moonUp.z), moonUp, Mathf.SmoothStep(0f, 1f, (k - 0.45f) / 0.55f));
                yield return null;
            }
            if (melter.body != null) melter.body.color = Color.white;
            if (mouthO != null) mouthO.gameObject.SetActive(false);
            if (mouthSmile != null) mouthSmile.gameObject.SetActive(true);
            if (bubble != null) bubble.Show("Ahh... nice and cold!");
            yield return new WaitForSeconds(1.5f);
            Win();
        }

        IEnumerator HideLater(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (bubble != null && !over) bubble.Hide();
        }
    }
}
