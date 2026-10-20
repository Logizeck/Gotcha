using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "The fake boss" — un boss gigante con barra della vita e attacchi annunciati (corsie rosse); l'eroe
    /// non ha armi e può solo schivare. Se vieni colpito o sopravvivi a tre cicli senza risolvere arriva l'attacco
    /// finale = sconfitta. Soluzione: trascina via la barra della vita: il boss non sa più quanto è forte, si
    /// rimpicciolisce in un pupazzetto imbarazzato e scappa.
    /// </summary>
    public class FakeBossLevel : LevelController
    {
        [SerializeField] Transform boss, healthBar, hero;
        [SerializeField] SpriteRenderer bossArt, heroArt, flash;
        [SerializeField] Sprite puppetSprite;
        [SerializeField] SpriteRenderer[] lanes;
        [SerializeField] TextMesh triesText, warnText;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] SmokeEmitter dust;
        [SerializeField] SmokeEmitter[] confetti;
        [SerializeField] float[] laneX = { -1.7f, 0f, 1.7f };
        [SerializeField] float hitHalfWidth = 0.95f, removeDistance = 1.3f;

        Camera cam;
        Coroutine cycle;
        Vector3 barHome, bossScale;
        bool dragBar, dragHero, busy, barTold, solved;
        Vector2 grabOffset;
        int survived;
        readonly bool[] danger = new bool[3];

        public override void OnLevelStart()
        {
            cam = Camera.main;
            barHome = healthBar.localPosition;
            bossScale = boss.localScale;
            foreach (SpriteRenderer l in lanes) S41KKit.Alpha(l, 0f);
            S41KKit.Alpha(flash, 0f);
            warnText.text = "";
            if (dust != null) dust.Stop();
            S41KKit.Confetti(confetti, false);
            UpdateTries();
            if (bubble != null) bubble.Show("I AM MEGA DOOM! BEHOLD MY 9999 HP!");
            cycle = StartCoroutine(Attacks());
        }

        void UpdateTries() { triesText.text = "ATTACKS SURVIVED: " + survived + "/3"; }

        void Update()
        {
            if (IsFinished || busy) return;
            if (!PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out bool ended)) { if (dragBar) DropBar(); dragHero = false; return; }
            Vector2 w = PointerInput.ToWorld(cam, sp);
            if (began)
            {
                Vector2 d = w - (Vector2)healthBar.position;
                if (Mathf.Abs(d.x) < 2.3f && Mathf.Abs(d.y) < 0.45f)
                {
                    dragBar = true;
                    grabOffset = (Vector2)healthBar.position - w;
                    Sfx.Play(this, "tick", 0.6f);
                    if (bubble != null && !barTold) { bubble.Show("HEY! Don't touch my stats!"); barTold = true; }
                }
                else if (w.y < -0.8f) dragHero = true;
            }
            if (dragBar)
            {
                healthBar.position = w + grabOffset;
                if (ended) DropBar();
            }
            else if (dragHero)
            {
                Vector3 p = hero.localPosition;
                p.x = Mathf.MoveTowards(p.x, Mathf.Clamp(w.x, -2.3f, 2.3f), Time.deltaTime * 9f);
                hero.localPosition = p;
                if (ended) dragHero = false;
            }
        }

        void DropBar()
        {
            dragBar = false;
            if (Vector2.Distance(healthBar.localPosition, barHome) > removeDistance) { StartCoroutine(WinRoutine()); return; }
            StartCoroutine(S41KKit.Move(healthBar, barHome, 0.25f));
            Sfx.Play(this, "clunk", 0.4f);
        }

        IEnumerator Attacks()
        {
            yield return new WaitForSeconds(2.5f);
            string[] shouts = { "FEEL MY FISTS!", "DOUBLE DOOM SLAM!", "YOU CANNOT DODGE FOREVER!" };
            while (true)
            {
                if (survived >= 3) { yield return StartCoroutine(Ultimate()); yield break; }
                // due corsie pericolose, quella sicura cambia ogni volta
                int heroLane = 0;
                for (int i = 1; i < 3; i++) if (Mathf.Abs(hero.localPosition.x - laneX[i]) < Mathf.Abs(hero.localPosition.x - laneX[heroLane])) heroLane = i;
                int safe = (heroLane + Random.Range(1, 3)) % 3;
                for (int i = 0; i < 3; i++) danger[i] = i != safe;
                if (bubble != null) bubble.Show(shouts[survived]);
                warnText.text = "DODGE!";
                Sfx.Play(this, "buzz", 0.35f);
                for (float t = 0f; t < 1.6f; t += Time.deltaTime)
                {
                    float a = 0.25f + 0.25f * Mathf.Sin(t * 18f);
                    for (int i = 0; i < 3; i++) S41KKit.Alpha(lanes[i], danger[i] ? a : 0f);
                    boss.localScale = bossScale * (1f + 0.06f * t / 1.6f);
                    yield return null;
                }
                // impatto
                for (int i = 0; i < 3; i++) S41KKit.Alpha(lanes[i], danger[i] ? 0.8f : 0f);
                boss.localScale = bossScale;
                Sfx.Play(this, "bonk", 1f);
                Haptics.Heavy();
                if (dust != null) dust.Play();
                StartCoroutine(S41KKit.Shake(boss, 0.15f, 0.4f));
                bool hit = false;
                for (int i = 0; i < 3; i++) if (danger[i] && Mathf.Abs(hero.localPosition.x - laneX[i]) < hitHalfWidth) hit = true;
                if (hit) { yield return StartCoroutine(HeroHit("BONK! Weaponless heroes squash easily.")); yield break; }
                yield return new WaitForSeconds(0.35f);
                if (dust != null) dust.Stop();
                foreach (SpriteRenderer l in lanes) S41KKit.Alpha(l, 0f);
                warnText.text = "";
                survived++;
                UpdateTries();
                yield return new WaitForSeconds(1.6f);
            }
        }

        IEnumerator Ultimate()
        {
            if (bubble != null) bubble.Show("ENOUGH! ULTIMATE 9999 DAMAGE ATTACK!");
            warnText.text = "NO ESCAPE!";
            for (float t = 0f; t < 1.5f; t += Time.deltaTime)
            {
                if (solved) yield break;
                S41KKit.Alpha(flash, 0.35f + 0.3f * Mathf.Sin(t * 20f));
                yield return null;
            }
            S41KKit.Alpha(flash, 0.9f);
            yield return StartCoroutine(HeroHit("Should have checked my stats..."));
        }

        IEnumerator HeroHit(string line)
        {
            if (solved) yield break;
            busy = true;
            Sfx.Play(this, "bonk", 1f);
            Haptics.Heavy();
            hero.localScale = new Vector3(1.5f, 0.35f, 1f);
            heroArt.color = new Color(1f, 0.7f, 0.7f);
            if (bubble != null) bubble.Show(line);
            yield return new WaitForSeconds(1.5f);
            Fail();
        }

        IEnumerator WinRoutine()
        {
            busy = true;
            solved = true;
            if (cycle != null) StopCoroutine(cycle);
            foreach (SpriteRenderer l in lanes) S41KKit.Alpha(l, 0f);
            S41KKit.Alpha(flash, 0f);
            warnText.text = "";
            boss.localScale = bossScale;
            if (dust != null) dust.Stop();
            Sfx.Play(this, "whoosh", 0.8f);
            StartCoroutine(S41KKit.Move(healthBar, healthBar.localPosition + new Vector3(0f, -9f, 0f), 0.8f));
            if (bubble != null) bubble.Show("Wait... where are my HP? How strong am I supposed to be?!");
            yield return new WaitForSeconds(1.3f);
            Sfx.Play(this, "pop", 0.8f);
            yield return StartCoroutine(S41KKit.ScaleTo(boss, bossScale * 0.2f, 0.5f));
            bossArt.sprite = puppetSprite;
            yield return StartCoroutine(S41KKit.ScaleTo(boss, bossScale * 0.55f, 0.25f));
            if (bubble != null) bubble.Show("...um. I'm actually just a puppet. Bye!");
            yield return new WaitForSeconds(1.2f);
            if (bubble != null) bubble.Hide();
            StartCoroutine(S41KKit.Move(boss, boss.localPosition + new Vector3(-4.5f, 0f, 0f), 1.1f, 0.3f));
            Sfx.Play(this, "ding", 0.9f);
            yield return StartCoroutine(S41KKit.Move(hero, hero.localPosition, 0.5f, 0.6f));
            S41KKit.Confetti(confetti, true);
            yield return new WaitForSeconds(1.8f);
            S41KKit.Confetti(confetti, false);
            Win();
        }
    }
}
