using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "Sleeping guard" — una guardia dorme accanto alla chiave; un misuratore di rumore sale e scende col suo
    /// russare. Soluzione: trascinare la chiave fino alla serratura della porta solo mentre russa forte (il rumore copre
    /// lo strisciare). Muoverla nei momenti di silenzio lo fa svegliare un po'; alla terza volta si sveglia → Fail().
    /// </summary>
    public class SleepingGuardLevel : LevelController
    {
        [SerializeField] SpriteRenderer guard;
        [SerializeField] Sprite sleepSprite, awakeSprite;
        [SerializeField] Transform key;
        [SerializeField] Collider2D keyArea;
        [SerializeField] Transform keyhole;
        [SerializeField] Transform doorPanel;            // anta della porta (si apre scalando x)
        [SerializeField] Transform mouse;
        [SerializeField] Transform meterFill;            // pivot a sinistra, scala x = rumore
        [SerializeField] SpriteRenderer meterColor;
        [SerializeField] TextMesh meterLabel, strikesLabel;
        [SerializeField] Transform[] zzz;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] float loudThreshold = 0.55f, quietTravel = 0.25f, lockRadius = 0.75f;

        static readonly float[] Loud = { 2.4f, 2.0f, 2.8f, 2.2f };
        static readonly float[] Quiet = { 1.8f, 2.5f, 1.5f, 2.2f };

        Camera cam;
        float clock, phaseT, noise, quietMoved, cooldown, snoreAt, bubbleUntil;
        int cycle, strikes;
        bool loud, dragging, busy;
        Vector2 grab, lastKey;
        Vector3[] zzzHome;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            zzzHome = new Vector3[zzz.Length];
            for (int i = 0; i < zzz.Length; i++) zzzHome[i] = zzz[i].localPosition;
            loud = false;
            phaseT = 1.2f;
            UpdateStrikes();
            Say("Psst... get the key to the door!", 2.2f);
        }

        void UpdateStrikes() { if (strikesLabel != null) strikesLabel.text = "STIRRED: " + strikes + "/3"; }

        void Say(string text, float seconds)
        {
            if (bubble == null) return;
            bubble.Show(text);
            bubbleUntil = clock + seconds;
        }

        void Update()
        {
            if (IsFinished) return;
            float dt = Time.deltaTime;
            clock += dt;
            if (bubble != null && bubbleUntil > 0f && clock > bubbleUntil) { bubble.Hide(); bubbleUntil = 0f; }
            if (busy) return;

            // ciclo del russare: forte / silenzio
            phaseT -= dt;
            if (phaseT <= 0f)
            {
                loud = !loud;
                if (loud) { phaseT = Loud[cycle % Loud.Length]; snoreAt = 0f; }
                else { phaseT = Quiet[cycle % Quiet.Length]; cycle++; }
            }
            float target = loud ? 0.85f + Mathf.Sin(clock * 9f) * 0.12f : 0.12f + Mathf.Sin(clock * 3f) * 0.04f;
            noise = Mathf.MoveTowards(noise, target, dt * (loud ? 4f : 2.2f));
            if (loud)
            {
                snoreAt -= dt;
                if (snoreAt <= 0f) { Sfx.Play(this, "snore", 1f); snoreAt = 0.55f; }
            }
            Visuals();
            cooldown -= dt;

            if (!S41AKit.Pointer(cam, out Vector2 w, out bool began, out bool ended)) { dragging = false; return; }
            if (began && keyArea.OverlapPoint(w)) { dragging = true; grab = (Vector2)key.position - w; lastKey = key.position; quietMoved = 0f; return; }
            if (!dragging) return;
            Vector2 to = w + grab;
            to.x = Mathf.Clamp(to.x, -2.6f, 2.6f);
            to.y = Mathf.Clamp(to.y, -3.3f, 2.4f);
            key.position = new Vector3(to.x, to.y, key.position.z);
            float moved = Vector2.Distance(to, lastKey);
            lastKey = to;
            if (noise < loudThreshold && cooldown <= 0f)
            {
                quietMoved += moved;
                if (moved > 0.01f && Random.value < 0.3f) Sfx.Play(this, "tick", 0.5f);   // stride sul pavimento
                if (quietMoved > quietTravel) { StartCoroutine(Stir()); return; }
            }
            else quietMoved = Mathf.Max(0f, quietMoved - dt * 0.5f);
            if (ended)
            {
                dragging = false;
                if (Vector2.Distance(key.position, keyhole.position) < lockRadius) StartCoroutine(Escape());
            }
        }

        void Visuals()
        {
            if (meterFill != null) meterFill.localScale = new Vector3(Mathf.Clamp01(noise), 1f, 1f);
            bool safe = noise >= loudThreshold;
            if (meterColor != null) meterColor.color = safe ? new Color(0.36f, 0.8f, 0.42f) : new Color(0.95f, 0.4f, 0.35f);
            if (meterLabel != null) meterLabel.text = safe ? "SNOOORE! MOVE NOW" : "shhh... quiet...";
            guard.transform.localScale = new Vector3(1f + noise * 0.04f, 1f + noise * 0.03f, 1f);
            for (int i = 0; i < zzz.Length; i++)
            {
                float k = Mathf.Repeat(clock * 0.6f + i * 0.33f, 1f);
                zzz[i].gameObject.SetActive(loud);
                zzz[i].localPosition = zzzHome[i] + new Vector3(k * 0.6f, k * 0.9f, 0f);
                zzz[i].localScale = Vector3.one * Mathf.Lerp(0.6f, 1.3f, k);
            }
        }

        IEnumerator Stir()
        {
            busy = true;
            dragging = false;
            strikes++;
            UpdateStrikes();
            Sfx.Play(this, "clunk", 0.6f);
            guard.sprite = awakeSprite;
            Haptics.Light();
            if (strikes >= 3)
            {
                Say("HEY! WHO'S THERE?!", 1.8f);
                Sfx.Play(this, "buzz", 0.8f);
                Haptics.Medium();
                foreach (Transform z in zzz) z.gameObject.SetActive(false);
                yield return S41AKit.Bounce(guard.transform, 0.15f, 0.3f);
                yield return new WaitForSeconds(1.4f);
                Fail();
                yield break;
            }
            Say(strikes == 1 ? "Hm? ...what was that?" : "Mmh?! ...who's scraping?", 1.6f);
            yield return new WaitForSeconds(1.0f);
            guard.sprite = sleepSprite;
            cooldown = 0.8f;
            busy = false;
        }

        IEnumerator Escape()
        {
            busy = true;
            yield return S41AKit.Move(key, keyhole.position, 0.15f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.3f) { key.localRotation = Quaternion.Euler(0f, 0f, 90f * k); yield return null; }
            Sfx.Play(this, "clunk", 0.8f);
            Haptics.Light();
            Vector3 s = doorPanel.localScale;
            key.gameObject.SetActive(false);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.4f) { doorPanel.localScale = new Vector3(Mathf.Lerp(s.x, 0.12f, k), s.y, 1f); yield return null; }
            Say("Tiptoe... tiptoe...", 1.6f);
            Vector3 exit = new Vector3(doorPanel.position.x + 0.7f, mouse.position.y, 0f);
            yield return S41AKit.Move(mouse, exit, 1.0f, 0.08f);
            mouse.gameObject.SetActive(false);
            Sfx.Play(this, "snore", 1f);
            Sfx.Play(this, "ding", 0.6f);
            yield return new WaitForSeconds(1.2f);
            Win();
        }
    }
}
