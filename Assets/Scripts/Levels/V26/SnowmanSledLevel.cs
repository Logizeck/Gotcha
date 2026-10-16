using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 85 — il pupazzo sul ghiaccio. Il sole attraversa il cielo e l'ombra dell'albero si sposta (all'alba è
    /// lunga da una parte, a mezzogiorno è sotto l'albero, al tramonto dall'altra). Il pupazzo scivola sul lago
    /// ghiacciato: inclinando il telefono lo si spinge, ma sul ghiaccio frena poco. Restare all'ombra fino al tramonto.
    /// Easter egg `growth_spurt`: sbattere fortissimo contro il tronco fa cadere tutta la neve dei rami sul pupazzo.
    /// </summary>
    public class SnowmanSledLevel : LevelController
    {
        [SerializeField] Transform snowman;            // pivot alla base
        [SerializeField] Transform sun;
        [SerializeField] Transform sunFrom, sunTo;
        [SerializeField] Transform tree;               // base del tronco
        [SerializeField] Transform shade;              // ellisse scura a terra (scala x = larghezza)
        [SerializeField] Transform heatBar;
        [SerializeField] SpriteRenderer body;
        [SerializeField] SmokeEmitter snowDump;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] float dayLength = 22f;
        [SerializeField] float push = 9f, friction = 0.6f, maxX = 2.5f;
        [SerializeField] float heatSeconds = 2.2f;
        [SerializeField] float crashSpeed = 4.2f;
        [SerializeField] string eggId = "growth_spurt";

        readonly SteadyTilt steady = new SteadyTilt { smoothing = 0.1f, deadZone = 0.02f, shakeLevel = 99f };
        float day, vx, heat;
        bool done;
        Vector3 baseScale;

        public override void OnLevelStart()
        {
            baseScale = snowman.localScale;
            if (snowDump != null) snowDump.Stop();
            Place();
            if (bubble != null) { bubble.Show("Keep me in the shade!"); StartCoroutine(HideLater(2f)); }
        }

        float ShadeX(out float half)
        {
            float k = day / dayLength;                         // 0 alba, 0.5 mezzogiorno, 1 tramonto
            float lean = Mathf.Cos(k * Mathf.PI);              // 1 → ombra a destra, -1 → a sinistra
            half = Mathf.Lerp(0.42f, 0.95f, Mathf.Abs(lean));  // più lunga col sole basso
            return tree.position.x + lean * 1.6f;
        }

        void Place()
        {
            float k = Mathf.Clamp01(day / dayLength);
            Vector3 a = sunFrom.position, b = sunTo.position;
            sun.position = Vector3.Lerp(a, b, k) + Vector3.up * Mathf.Sin(k * Mathf.PI) * 1.2f;
            float x = ShadeX(out float half);
            shade.position = new Vector3(x, shade.position.y, shade.position.z);
            shade.localScale = new Vector3(half * 2f, shade.localScale.y, 1f);
        }

        void Update()
        {
            if (done || IsFinished) return;
            float dt = Time.deltaTime;
            day += dt;
            Place();
            float tilt = steady.Update(dt);
            float before = snowman.position.x;
            vx += tilt * push * dt;
            vx -= vx * friction * dt;
            float x = before + vx * dt;
            if (Mathf.Abs(x) > maxX) { x = Mathf.Sign(x) * maxX; vx = -vx * 0.4f; }
            // attraversa il tronco: piano gli passa davanti, a tutta velocità ci sbatte
            float tx = tree.position.x;
            if (Mathf.Abs(vx) > crashSpeed && (before - tx) * (x - tx) <= 0f) { StartCoroutine(Crash()); return; }
            snowman.position = new Vector3(x, snowman.position.y, snowman.position.z);
            snowman.localRotation = Quaternion.Euler(0, 0, -vx * 3f);

            float sx = ShadeX(out float half);
            bool cool = Mathf.Abs(x - sx) < half - 0.15f;
            heat = Mathf.Clamp01(heat + (cool ? -0.5f : 1f) * dt / heatSeconds);
            if (heatBar != null) heatBar.localScale = new Vector3(heat, 1f, 1f);
            snowman.localScale = new Vector3(baseScale.x * (1f + heat * 0.15f), baseScale.y * (1f - heat * 0.3f), 1f);
            if (body != null) body.color = Color.Lerp(Color.white, new Color(0.75f, 0.9f, 1f), heat);
            if (heat >= 1f) { StartCoroutine(Melt()); return; }
            if (day >= dayLength) StartCoroutine(Sunset());
        }

        IEnumerator Melt()
        {
            done = true;
            if (bubble != null) bubble.Show("I'm... a puddle...");
            for (float k = 0f; k < 1f; k += Time.deltaTime / 1.2f)
            {
                snowman.localScale = new Vector3(baseScale.x * (1.15f + k), baseScale.y * Mathf.Lerp(0.7f, 0.05f, k), 1f);
                yield return null;
            }
            yield return new WaitForSeconds(0.5f);
            Fail();
        }

        IEnumerator Sunset()
        {
            done = true;
            if (bubble != null) bubble.Show("Phew! Sunset. Cool...");
            yield return new WaitForSeconds(1.5f);
            Win();
        }

        /// <summary>Easter egg: BONK contro l'albero, la neve dei rami gli cade addosso e diventa enorme.</summary>
        IEnumerator Crash()
        {
            done = true;
            Sfx.Play(this, "bonk", 0.9f);
            if (snowDump != null) { snowDump.transform.position = tree.position + Vector3.up * 2.4f; snowDump.Play(); }
            if (bubble != null) bubble.Show("BONK!");
            yield return new WaitForSeconds(0.6f);
            Vector3 s = snowman.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 1.2f)
            {
                snowman.localScale = Vector3.Lerp(s, baseScale * 2.2f, k * k);
                yield return null;
            }
            if (snowDump != null) snowDump.Stop();
            if (bubble != null) bubble.Show("Who needs shade now? I'M HUGE!");
            yield return new WaitForSeconds(1.8f);
            WinWithEgg(eggId);
        }

        IEnumerator HideLater(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (bubble != null && !done) bubble.Hide();
        }
    }
}
