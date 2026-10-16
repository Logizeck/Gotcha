using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 100 — il trofeo. Il gran finale usa tutto quello che hai imparato: il trofeo è sullo scaffale più alto
    /// (scuoti per farlo cadere sul cuscino), è coperto di polvere (soffia) ed è opaco (strofinalo col dito finché
    /// brilla). Easter egg `polish_maniac`: continuare a lucidarlo anche dopo... finché non ne resta niente.
    /// </summary>
    public class TrophyLevel : LevelController
    {
        [SerializeField] Transform trophy;
        [SerializeField] Transform landing;           // sul cuscino
        [SerializeField] SpriteRenderer dust;
        [SerializeField] SpriteRenderer shine;
        [SerializeField] SmokeEmitter dustCloud;
        [SerializeField] SmokeEmitter confetti;
        [SerializeField] TextMesh theEnd;
        [SerializeField] TextMesh step;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] float shakeNeeded = 1.0f, blowNeeded = 2.2f, rubNeeded = 14f;
        [SerializeField] float maniacSeconds = 2.5f;
        [SerializeField] string eggId = "polish_maniac";

        int phase;
        float shook, blown, rubbed, extra, windowLeft;
        Vector2 lastFinger;
        bool fingerDown, done;
        Vector3 shelfPos, baseScale;
        Camera cam;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            shelfPos = trophy.position;
            baseScale = trophy.localScale;
            shine.color = new Color(1f, 1f, 1f, 0f);
            theEnd.gameObject.SetActive(false);
            if (dustCloud != null) dustCloud.Stop();
            if (confetti != null) confetti.Stop();
            Step("1. Get the trophy down");
            if (bubble != null) { bubble.Show("The final trophy! It's... up there."); StartCoroutine(HideLater(2.2f)); }
        }

        void OnDestroy() { Inputs.ReleaseMic(this); }

        void Step(string s) { if (step != null) step.text = s; }

        void Update()
        {
            if (done || IsFinished) return;
            float dt = Time.deltaTime;
            switch (phase)
            {
                case 0:
                    float s = Inputs.Shake();
                    trophy.position = shelfPos + new Vector3(Mathf.Sin(Time.time * 40f) * Mathf.Min(s, 20f) * 0.004f, 0f, 0f);
                    if (s > 6f) shook += dt;
                    if (shook >= shakeNeeded) StartCoroutine(FallDown());
                    break;
                case 1:
                    bool blowing = Inputs.Mic(this) > 0.3f;
                    if (blowing) blown += dt;
                    if (dustCloud != null) { if (blowing && !dustCloud.IsPlaying) dustCloud.Play(); else if (!blowing && dustCloud.IsPlaying) dustCloud.Stop(); }
                    dust.color = new Color(1f, 1f, 1f, 1f - blown / blowNeeded);
                    if (blown >= blowNeeded)
                    {
                        if (dustCloud != null) dustCloud.Stop();
                        Inputs.ReleaseMic(this);
                        phase = 2;
                        Step("3. Polish it!");
                        if (bubble != null) { bubble.Show("*cough* Now make it SHINE."); StartCoroutine(HideLater(1.6f)); }
                    }
                    break;
                case 2:
                case 3:
                    float r = Rub();
                    if (phase == 2)
                    {
                        rubbed += r;
                        shine.color = new Color(1f, 1f, 1f, Mathf.Clamp01(rubbed / rubNeeded));
                        if (rubbed >= rubNeeded) StartCoroutine(Shiny());
                    }
                    else
                    {
                        // dopo il "THE END" c'è ancora qualcuno che lucida...
                        windowLeft -= dt;
                        if (r > 0f) extra += dt;
                        trophy.localScale = baseScale * Mathf.Lerp(1f, 0.15f, extra / maniacSeconds);
                        if (extra >= maniacSeconds) { StartCoroutine(Maniac()); return; }
                        if (windowLeft <= 0f) { done = true; Win(); }
                    }
                    break;
            }
        }

        /// <summary>Strada percorsa dal dito sopra il trofeo in questo frame.</summary>
        float Rub()
        {
            if (!PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out _)) { fingerDown = false; return 0f; }
            Vector2 w = PointerInput.ToWorld(cam, sp);
            if (began || !fingerDown) { fingerDown = true; lastFinger = w; return 0f; }
            float d = Vector2.Distance(w, lastFinger);
            lastFinger = w;
            return Vector2.Distance(w, trophy.position + Vector3.up * 0.6f) < 1.1f ? d : 0f;
        }

        IEnumerator FallDown()
        {
            phase = -1;
            Sfx.Play(this, "whoosh");
            Vector3 from = trophy.position, to = landing.position;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.6f)
            {
                trophy.position = Vector3.Lerp(from, to, k) + Vector3.up * Mathf.Sin(k * Mathf.PI) * 0.6f;
                trophy.localRotation = Quaternion.Euler(0, 0, k * 360f);
                yield return null;
            }
            trophy.position = to;
            trophy.localRotation = Quaternion.identity;
            Sfx.Play(this, "bonk", 0.5f);
            if (bubble != null) { bubble.Show("Soft landing. Very dusty, though."); StartCoroutine(HideLater(1.8f)); }
            Step("2. Blow the dust off");
            phase = 1;
        }

        IEnumerator Shiny()
        {
            phase = -1;
            Sfx.Play(this, "ding", 1f);
            if (confetti != null) confetti.Play();
            theEnd.gameObject.SetActive(true);
            Step("");
            if (bubble != null) bubble.Show("YOU DID IT! All 100 levels!");
            yield return new WaitForSeconds(0.6f);
            windowLeft = 3.5f;
            phase = 3;
        }

        /// <summary>Easter egg: a forza di lucidarlo, il trofeo si consuma fino a sparire.</summary>
        IEnumerator Maniac()
        {
            done = true;
            if (confetti != null) confetti.Stop();
            trophy.gameObject.SetActive(false);
            theEnd.text = "THE... END?";
            if (bubble != null) bubble.Show("You polished it AWAY. Impressive. And weird.");
            yield return new WaitForSeconds(2.2f);
            WinWithEgg(eggId);
        }

        IEnumerator HideLater(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (bubble != null && !done) bubble.Hide();
        }
    }
}
