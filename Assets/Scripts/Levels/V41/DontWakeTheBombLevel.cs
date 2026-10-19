using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "Don't wake the bomb" — una bomba dei cartoni dorme sul cuscino; ogni tocco la fa ticchettare.
    /// Soluzione: prendere la punta della miccia e sfilarla piano piano, tenendo il dito sotto una certa velocità
    /// (misurata in frazioni dell'altezza dello schermo, quindi uguale su ogni telefono). Sconfitta: i movimenti bruschi
    /// (e i tocchi sulla bomba) riempiono il misuratore del risveglio; pieno = esplode in una nuvola di coriandoli.
    /// </summary>
    public class DontWakeTheBombLevel : LevelController
    {
        [SerializeField] Transform bomb;
        [SerializeField] SpriteRenderer bombArt;
        [SerializeField] Sprite asleepSprite, drowsySprite, awakeSprite;
        [SerializeField] Transform socket;               // dove la miccia entra nella bomba
        [SerializeField] Transform tip;                  // punta della miccia (si trascina)
        [SerializeField] LineRenderer fuse;
        [SerializeField] Collider2D bombArea;
        [SerializeField] Transform wakeBar;              // scala x 0..1
        [SerializeField] SpriteRenderer wakeFill;
        [SerializeField] Transform pullBar;              // quanto è uscita la miccia
        [SerializeField] Transform[] zzz;
        [SerializeField] SpriteRenderer boom;
        [SerializeField] SpriteRenderer[] confetti;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] float pullLength = 2.6f, speedLimit = 0.11f;   // altezze di schermo al secondo

        Camera cam;
        float wake, pulled, smoothSpeed, tickClock, clock;
        bool dragging, busy;
        Vector2 lastScreen;
        Vector3 tipStart;
        int warned;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            tipStart = tip.position;
            if (boom != null) boom.gameObject.SetActive(false);
            foreach (SpriteRenderer c in confetti) if (c != null) c.gameObject.SetActive(false);
            if (bubble != null) bubble.Hide();
            UpdateFuse();
            UpdateMeters();
        }

        void UpdateFuse()
        {
            if (fuse == null) return;
            const int n = 16;
            fuse.positionCount = n;
            Vector3 a = socket.position, b = tip.position;
            float slack = Mathf.Max(0f, 0.25f + pulled * pullLength - Vector2.Distance(a, b)) * 0.5f + 0.08f;
            for (int i = 0; i < n; i++)
            {
                float k = i / (n - 1f);
                Vector3 p = Vector3.Lerp(a, b, k);
                p.y -= Mathf.Sin(k * Mathf.PI) * slack;
                p.x += Mathf.Sin(k * Mathf.PI * 2f) * 0.04f;
                p.z = -0.01f;
                fuse.SetPosition(i, p);
            }
        }

        void UpdateMeters()
        {
            if (wakeBar != null) wakeBar.localScale = new Vector3(Mathf.Clamp01(wake), 1f, 1f);
            if (wakeFill != null) wakeFill.color = Color.Lerp(new Color(0.45f, 0.8f, 1f), new Color(1f, 0.3f, 0.25f), wake);
            if (pullBar != null) pullBar.localScale = new Vector3(Mathf.Clamp01(pulled), 1f, 1f);
            if (bombArt != null) bombArt.sprite = wake > 0.66f ? awakeSprite : wake > 0.33f ? drowsySprite : asleepSprite;
        }

        void Update()
        {
            if (IsFinished || busy) return;
            float dt = Time.deltaTime;
            clock += dt;

            // le Z del sonno e il ticchettio se si sta svegliando
            for (int i = 0; i < zzz.Length; i++)
            {
                float k = Mathf.Repeat(clock * 0.5f + i / (float)zzz.Length, 1f);
                zzz[i].localPosition = new Vector3(0.9f + k * 0.6f + Mathf.Sin(k * 7f) * 0.08f, 0.2f + k * 1.1f, 0f);
                zzz[i].localScale = Vector3.one * (0.3f + k * 0.4f) * (1f - wake);
            }
            if (wake > 0.25f)
            {
                tickClock -= dt;
                if (tickClock <= 0f) { Sfx.Play(this, "tick", 0.5f + wake * 0.4f); tickClock = Mathf.Lerp(0.6f, 0.15f, wake); StartCoroutine(Kit41B.Pop(bomb, 0.03f + wake * 0.04f, 0.12f)); }
            }
            wake = Mathf.Max(0f, wake - 0.06f * dt);

            if (!PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out bool ended)) { dragging = false; smoothSpeed = 0f; UpdateMeters(); return; }
            Vector2 w = PointerInput.ToWorld(cam, sp);
            if (began && !PointerInput.IsOverUi())
            {
                if (Vector2.Distance(w, tip.position) < 0.6f) { dragging = true; lastScreen = sp; smoothSpeed = 0f; }
                else if (bombArea != null && bombArea.OverlapPoint(w)) { Poke(); }
                UpdateMeters();
                return;
            }
            if (dragging)
            {
                // velocità del dito in altezze di schermo al secondo (stessa sensazione su ogni telefono)
                float speed = Vector2.Distance(sp, lastScreen) / Mathf.Max(1f, Screen.height) / Mathf.Max(dt, 0.001f);
                lastScreen = sp;
                smoothSpeed = Mathf.Lerp(smoothSpeed, speed, Mathf.Clamp01(dt * 12f));
                if (smoothSpeed > speedLimit)
                {
                    wake += (smoothSpeed / speedLimit - 1f) * 0.35f * dt;
                    if (warned < 2 && wake > 0.4f + warned * 0.25f) { warned++; Kit41B.Say(bubble, warned == 1 ? "Mmmh...?" : "*tick* ...who's there?"); }
                }
                tip.position = new Vector3(Mathf.Clamp(w.x, -2.6f, 2.6f), Mathf.Clamp(w.y, -4.4f, 2.6f), tip.position.z);
                float d = Vector2.Distance(tip.position, socket.position);
                pulled = Mathf.Max(pulled, Mathf.Clamp01((d - Vector2.Distance(tipStart, socket.position)) / pullLength));
                UpdateFuse();
                if (ended) dragging = false;
                if (pulled >= 1f) { StartCoroutine(Defused()); return; }
            }
            UpdateMeters();
            if (wake >= 1f) StartCoroutine(Explode());
        }

        void Poke()
        {
            wake += 0.18f;
            Sfx.Play(this, "tick", 0.8f);
            Haptics.Light();
            StartCoroutine(Kit41B.Pop(bomb, 0.08f, 0.15f));
            Kit41B.Say(bubble, "*TICK!*");
            if (wake >= 1f) StartCoroutine(Explode());
        }

        IEnumerator Defused()
        {
            busy = true;
            UpdateMeters();
            Sfx.Play(this, "pop", 0.9f);
            Haptics.Light();
            // la miccia si stacca e cade
            fuse.positionCount = 0;
            Vector3 p = tip.position;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.6f)
            {
                tip.position = p + Vector3.down * k * k * 2f;
                tip.localRotation = Quaternion.Euler(0f, 0f, k * 200f);
                yield return null;
            }
            tip.gameObject.SetActive(false);
            foreach (Transform z in zzz) z.localScale = Vector3.one * 0.4f;
            yield return new WaitForSeconds(0.6f);
            Kit41B.Say(bubble, "Zzz... *pfff*  ...I'm a ball now. Nice.");
            Sfx.Play(this, "snore", 0.7f);
            yield return Kit41B.Pop(bomb, 0.08f, 0.6f);
            Sfx.Play(this, "ding", 0.6f);
            yield return new WaitForSeconds(1.4f);
            Win();
        }

        IEnumerator Explode()
        {
            busy = true;
            wake = 1f;
            UpdateMeters();
            Kit41B.Say(bubble, "WHO WOKE ME UP?!");
            for (int i = 0; i < 6; i++) { Sfx.Play(this, "tick", 0.9f); yield return Kit41B.Pop(bomb, 0.12f, 0.12f); }
            if (bubble != null) bubble.Hide();
            Haptics.Heavy();
            Sfx.Play(this, "bonk", 1f);
            Sfx.Play(this, "pop", 1f);
            bomb.gameObject.SetActive(false);
            fuse.positionCount = 0;
            tip.gameObject.SetActive(false);
            if (boom != null) { boom.gameObject.SetActive(true); StartCoroutine(Kit41B.Pop(boom.transform, 0.3f, 0.3f)); }
            Color[] tints = { new Color(1f, 0.4f, 0.5f), new Color(0.4f, 0.8f, 1f), new Color(1f, 0.9f, 0.3f), new Color(0.6f, 1f, 0.5f), new Color(0.9f, 0.6f, 1f) };
            for (int i = 0; i < confetti.Length; i++) StartCoroutine(Confetto(confetti[i], tints[i % tints.Length]));
            yield return new WaitForSeconds(1.6f);
            Fail();
        }

        IEnumerator Confetto(SpriteRenderer c, Color tint)
        {
            if (c == null) yield break;
            c.gameObject.SetActive(true);
            c.color = tint;
            Vector3 p = bomb.position;
            Vector3 v = new Vector3(Random.Range(-3.5f, 3.5f), Random.Range(1f, 5f), 0f);
            float spin = Random.Range(-500f, 500f);
            c.transform.position = p;
            for (float t = 0f; t < 1.5f; t += Time.deltaTime)
            {
                v.y -= 5f * Time.deltaTime;
                v.x *= 1f - Time.deltaTime * 0.8f;
                c.transform.position += v * Time.deltaTime;
                c.transform.Rotate(0f, 0f, spin * Time.deltaTime);
                yield return null;
            }
        }
    }
}
