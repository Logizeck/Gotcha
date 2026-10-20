using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "The dirty secret" — lo schermo (finto) è pieno di ditate e unto. Strofina col dito per pulirlo: sopra il
    /// 95% compare il pulsante EXIT nascosto, toccalo. Nessuna sconfitta; ogni 40 s arriva un nuovo strato di sporco.
    /// Easter egg: strofina senza sosta per 8 s la macchia tonda in basso a sinistra: è una botola da cui esce l'omino
    /// in mutande che sporca il vetro da dentro, fa finta di niente, fa una pernacchia e scappa sulla sedia a rotelle.
    /// </summary>
    public class DirtySecretLevel : LevelController
    {
        [SerializeField] Sprite[] dirtSprites;
        [SerializeField] Sprite[] cleanerPoses;           // 0 sporca, 1 innocente, 2 pernacchia, 3 sedia
        [SerializeField] Transform exitButton, smudge, hatchHole, cleaner, rag, bubbleHolder;
        [SerializeField] Collider2D exitArea;
        [SerializeField] SpriteRenderer cleanerArt;
        [SerializeField] TextMesh percentText, alertText;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] SmokeEmitter[] confetti;
        [SerializeField] float cell = 0.45f, minX = -2.7f, maxX = 2.7f, minY = -4.4f, maxY = 2.5f, brush = 0.6f, layerEvery = 40f, eggSeconds = 8f;

        class Dirt { public SpriteRenderer sr; public float amount, alpha; }

        Camera cam;
        readonly List<Dirt> dirt = new List<Dirt>();
        Vector2 last;
        bool hasLast, busy, revealed;
        float layerTimer, eggTime, eggAway, lastMove;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            BuildDirt();
            exitButton.gameObject.SetActive(false);
            hatchHole.gameObject.SetActive(false);
            cleaner.gameObject.SetActive(false);
            alertText.text = "";
            layerTimer = layerEvery;
            S41GKit.Confetti(confetti, false);
            UpdatePercent();
        }

        void BuildDirt()
        {
            int k = 0;
            for (float y = minY; y <= maxY + 0.01f; y += cell)
                for (float x = minX; x <= maxX + 0.01f; x += cell)
                {
                    var go = new GameObject("Dirt");
                    go.transform.SetParent(transform, false);
                    go.transform.localPosition = new Vector3(x + Random.Range(-0.12f, 0.12f), y + Random.Range(-0.12f, 0.12f), 0f);
                    go.transform.localRotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
                    go.transform.localScale = Vector3.one * Random.Range(1.0f, 1.5f);
                    SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
                    sr.sprite = dirtSprites[(k++ + Random.Range(0, 2)) % dirtSprites.Length];
                    sr.sortingOrder = 10;
                    var d = new Dirt { sr = sr, amount = 1f, alpha = Random.Range(0.8f, 1f) };
                    Paint(d);
                    dirt.Add(d);
                }
        }

        static void Paint(Dirt d) { S41GKit.Alpha(d.sr, d.alpha * Mathf.Clamp01(d.amount * 1.3f)); }

        float Cleanliness()
        {
            float sum = 0f;
            foreach (Dirt d in dirt) sum += Mathf.Clamp01(d.amount);
            return 1f - sum / Mathf.Max(1, dirt.Count);
        }

        void UpdatePercent() { percentText.text = "CLEAN: " + Mathf.FloorToInt(Cleanliness() * 100f) + "%"; }

        void Update()
        {
            if (IsFinished || busy) return;
            float dt = Time.deltaTime;
            layerTimer -= dt;
            if (layerTimer <= 0f) { layerTimer = layerEvery; NewLayer(); }

            bool held = PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out bool ended) && !ended;
            if (!held) { hasLast = false; EggDecay(dt); return; }
            Vector2 w = PointerInput.ToWorld(cam, sp);
            if (began && revealed && exitArea.OverlapPoint(w)) { StartCoroutine(Exit()); return; }
            float moved = hasLast ? Vector2.Distance(w, last) : 0f;
            last = w;
            hasLast = true;
            if (moved > 0.01f) lastMove = Time.time;
            Rub(w, moved * 1.5f + dt * 0.3f);

            // la macchia tonda: va strofinata di continuo
            if (Vector2.Distance(w, smudge.position) < 0.65f && Time.time - lastMove < 0.4f)
            {
                eggTime += dt;
                eggAway = 0f;
                smudge.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(Time.time * 30f) * eggTime * 1.5f);
                if (Mathf.Floor(eggTime) != Mathf.Floor(eggTime - dt)) Sfx.Play(this, "tick", 0.3f + eggTime * 0.05f);
                if (eggTime >= eggSeconds) { StartCoroutine(Egg()); return; }
            }
            else EggDecay(dt);
        }

        void EggDecay(float dt)
        {
            eggAway += dt;
            if (eggAway > 0.35f) { eggTime = 0f; smudge.localRotation = Quaternion.identity; }
        }

        void Rub(Vector2 w, float amount)
        {
            bool any = false;
            foreach (Dirt d in dirt)
            {
                if (d.amount <= 0f) continue;
                float dist = Vector2.Distance(w, d.sr.transform.position);
                if (dist > brush) continue;
                d.amount -= amount * (1f - dist / brush) * 1.6f;
                if (d.amount < 0.04f) d.amount = 0f;
                Paint(d);
                any = true;
            }
            if (!any) return;
            UpdatePercent();
            if (!revealed && Cleanliness() >= 0.95f) Reveal();
        }

        void Reveal()
        {
            revealed = true;
            exitButton.gameObject.SetActive(true);
            Sfx.Play(this, "ding", 0.7f);
            StartCoroutine(S41GKit.Pop(exitButton, 1.3f, 0.35f));
        }

        void Smear(Vector2 at, float radius, float add)
        {
            foreach (Dirt d in dirt)
            {
                float dist = Vector2.Distance(at, d.sr.transform.position);
                if (dist > radius) continue;
                d.amount = Mathf.Min(1f, d.amount + add * (1f - dist / radius * 0.5f));
                Paint(d);
            }
        }

        void NewLayer()
        {
            for (int i = 0; i < 5; i++) Smear(new Vector2(Random.Range(minX, maxX), Random.Range(minY, maxY)), 0.9f, 0.8f);
            Sfx.Play(this, "bonk", 0.5f);
            Haptics.Light();
            if (bubble != null) bubble.Show("Who keeps touching the screen?!");
            UpdatePercent();
            if (revealed && Cleanliness() < 0.95f) { revealed = false; exitButton.gameObject.SetActive(false); }
        }

        IEnumerator Exit()
        {
            busy = true;
            Sfx.Play(this, "clunk", 0.7f);
            yield return StartCoroutine(S41GKit.Pop(exitButton, 0.8f, 0.2f));
            Sfx.Play(this, "ding", 0.9f);
            percentText.text = "SQUEAKY CLEAN!";
            S41GKit.Confetti(confetti, true);
            yield return new WaitForSeconds(1.8f);
            S41GKit.Confetti(confetti, false);
            Win();
        }

        void Pose(int i) { cleanerArt.sprite = cleanerPoses[i]; }

        void SayAt(string t)
        {
            if (bubble == null) return;
            bubble.transform.position = bubbleHolder.position;
            bubble.Show(t);
        }

        IEnumerator Egg()
        {
            busy = true;
            smudge.localRotation = Quaternion.identity;
            Sfx.Play(this, "clunk", 0.8f);
            // la botola si apre (cardine in alto)
            hatchHole.gameObject.SetActive(true);
            Vector3 s0 = smudge.localScale, p0 = smudge.localPosition;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.45f)
            {
                smudge.localScale = new Vector3(s0.x, s0.y * (1f - k), s0.z);
                smudge.localPosition = p0 + new Vector3(0f, 0.42f * k, 0f);
                yield return null;
            }
            smudge.gameObject.SetActive(false);
            // esce l'omino
            Pose(0);
            cleaner.gameObject.SetActive(true);
            Vector3 home = cleaner.localPosition, big = cleaner.localScale;
            cleaner.localPosition = hatchHole.localPosition + new Vector3(0f, -0.3f, 0f);
            cleaner.localScale = big * 0.2f;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.6f)
            {
                cleaner.localPosition = Vector3.Lerp(hatchHole.localPosition, home, k) + Vector3.up * Mathf.Sin(k * Mathf.PI) * 0.8f;
                cleaner.localScale = Vector3.Lerp(big * 0.2f, big, k);
                yield return null;
            }
            cleaner.localPosition = home;
            cleaner.localScale = big;
            // sporca il vetro da dentro, beato
            for (int i = 0; i < 6; i++)
            {
                Vector2 at = (Vector2)rag.position + new Vector2(Random.Range(-0.3f, 0.3f), Random.Range(-0.2f, 0.3f));
                Smear(at, 0.7f, 1f);
                Sfx.Play(this, "tick", 0.6f);
                cleaner.localPosition = home + new Vector3((i % 2 == 0 ? 0.15f : -0.15f), 0f, 0f);
                yield return new WaitForSeconds(0.22f);
            }
            UpdatePercent();
            cleaner.localPosition = home;
            // si accorge di te
            alertText.text = "!";
            Sfx.Play(this, "pop", 0.8f);
            yield return StartCoroutine(S41GKit.Pop(cleaner, 1.15f, 0.25f));
            yield return new WaitForSeconds(0.5f);
            alertText.text = "";
            rag.gameObject.SetActive(false);
            Pose(1);
            SayAt("Oh! Hi! I'm... the cleaner. Very busy cleaning.");
            for (float k = 0f; k < 1.8f; k += Time.deltaTime) { cleaner.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(k * 8f) * 5f); yield return null; }
            cleaner.localRotation = Quaternion.identity;
            // pernacchia
            Pose(2);
            SayAt("PFFFFBBBTTT!");
            Sfx.Play(this, "buzz", 0.9f);
            Haptics.Medium();
            yield return StartCoroutine(S41GKit.Shake(cleaner, 0.08f, 0.6f));
            // montagna di ditate
            foreach (Dirt d in dirt) { d.amount = 1f; d.alpha = 1f; Paint(d); }
            for (int i = 0; i < 8; i++) { Sfx.Play(this, "bonk", 0.4f); yield return new WaitForSeconds(0.07f); }
            Haptics.Heavy();
            percentText.text = "CLEAN: 0%";
            // fuga sulla sedia da ufficio
            Pose(3);
            SayAt("BYEEE!");
            Sfx.Play(this, "whoosh", 0.9f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.9f)
            {
                cleaner.localPosition = home + new Vector3(k * k * 6f, Mathf.Sin(k * 30f) * 0.04f, 0f);
                cleaner.localRotation = Quaternion.Euler(0, 0, -8f * k);
                yield return null;
            }
            cleaner.gameObject.SetActive(false);
            if (bubble != null) bubble.Hide();
            yield return new WaitForSeconds(0.6f);
            percentText.text = "...WELL, THAT EXPLAINS IT.";
            S41GKit.Confetti(confetti, true);
            yield return new WaitForSeconds(1.8f);
            S41GKit.Confetti(confetti, false);
            WinWithEgg("dirty_secret");
        }
    }
}
