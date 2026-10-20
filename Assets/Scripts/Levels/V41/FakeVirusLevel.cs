using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "The fake virus" — una creaturina-virus attraversa il desktop (finto) e si moltiplica in copie identiche.
    /// Solo l'originale starnutisce, ogni 5 secondi: trovalo e trascinalo nel cestino. Cancellare tre copie innocue
    /// = sconfitta. Nessun accesso a file veri: è tutto un disegno.
    /// </summary>
    public class FakeVirusLevel : LevelController
    {
        [SerializeField] Sprite idleSprite, sneezeSprite, puffSprite;
        [SerializeField] Transform bin, spawnPoint;
        [SerializeField] Collider2D binArea;
        [SerializeField] TextMesh statusText, triesText, achoo;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] SmokeEmitter[] confetti;
        [SerializeField] float minX = -2.25f, maxX = 2.25f, minY = -2.45f, maxY = 2.3f, speed = 0.8f, copyEvery = 2.4f, sneezeEvery = 5f;
        [SerializeField] int maxCopies = 8;

        class Critter { public Transform t; public SpriteRenderer sr; public Collider2D col; public Vector2 vel; public bool original; public float hop; }

        Camera cam;
        readonly List<Critter> critters = new List<Critter>();
        Critter held, original;
        Vector2 grabOffset;
        float copyTimer, sneezeTimer, achooTimer;
        bool busy;
        int deleted;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            original = Spawn(spawnPoint.localPosition, true);
            copyTimer = 1.2f;
            sneezeTimer = 2.5f;
            achoo.text = "";
            statusText.text = "1 SUSPICIOUS FILE";
            UpdateTries();
            S41GKit.Confetti(confetti, false);
            if (bubble != null) bubble.Show("Hee hee! Copy, copy, copy!");
        }

        void UpdateTries() { triesText.text = "COPIES DELETED: " + deleted + "/3"; }

        Critter Spawn(Vector2 at, bool isOriginal)
        {
            var go = new GameObject(isOriginal ? "Virus" : "Copy");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = at;
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = idleSprite;
            sr.sortingOrder = 10 + critters.Count % 8;
            CircleCollider2D col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.42f;
            float a = Random.Range(0f, Mathf.PI * 2f);
            var c = new Critter { t = go.transform, sr = sr, col = col, original = isOriginal, vel = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * speed, hop = Random.Range(0f, 3f) };
            critters.Add(c);
            return c;
        }

        void Update()
        {
            if (IsFinished || busy) return;
            float dt = Time.deltaTime;
            Wander(dt);
            Copies(dt);
            Sneeze(dt);
            Drag();
        }

        void Wander(float dt)
        {
            foreach (Critter c in critters)
            {
                if (c == held) continue;
                Vector3 p = c.t.localPosition + (Vector3)(c.vel * dt);
                if (p.x < minX || p.x > maxX) { c.vel.x = -c.vel.x; p.x = Mathf.Clamp(p.x, minX, maxX); }
                if (p.y < minY || p.y > maxY) { c.vel.y = -c.vel.y; p.y = Mathf.Clamp(p.y, minY, maxY); }
                c.t.localPosition = p;
                // zampettio: tutti uguali, così non si distinguono camminando
                c.hop += dt * 9f;
                c.t.localScale = new Vector3(1f + Mathf.Sin(c.hop) * 0.05f, 1f - Mathf.Sin(c.hop) * 0.05f, 1f);
                if (Random.value < dt * 0.3f) { float a = Random.Range(0f, Mathf.PI * 2f); c.vel = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * speed; }
            }
        }

        void Copies(float dt)
        {
            if (critters.Count - 1 >= maxCopies) return;
            copyTimer -= dt;
            if (copyTimer > 0f) return;
            copyTimer = copyEvery;
            // la copia nasce da una creaturina a caso (anche da un'altra copia)
            Critter from = critters[Random.Range(0, critters.Count)];
            if (from == held) return;
            Spawn(from.t.localPosition, false);
            Sfx.Play(this, "pop", 0.4f);
            statusText.text = critters.Count + " SUSPICIOUS FILES";
        }

        void Sneeze(float dt)
        {
            achooTimer -= dt;
            if (achooTimer <= 0f) { achoo.text = ""; if (original.sr.sprite != idleSprite) original.sr.sprite = idleSprite; }
            else achoo.transform.position = original.t.position + new Vector3(0f, 0.6f, 0f);
            sneezeTimer -= dt;
            if (sneezeTimer > 0f) return;
            sneezeTimer = sneezeEvery;
            original.sr.sprite = sneezeSprite;
            achoo.text = "ACHOO!";
            achooTimer = 0.9f;
            achoo.transform.position = original.t.position + new Vector3(0f, 0.6f, 0f);
            Sfx.Play(this, "woof", 0.5f);
            StartCoroutine(Puff(original.t.position));
        }

        IEnumerator Puff(Vector3 at)
        {
            var go = new GameObject("Puff");
            go.transform.SetParent(transform, false);
            go.transform.position = at + new Vector3(0.35f, -0.05f, 0f);
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = puffSprite;
            sr.sortingOrder = 25;
            sr.color = new Color(0.85f, 1f, 0.8f, 0.9f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.8f)
            {
                go.transform.localScale = Vector3.one * (0.3f + k * 0.5f);
                go.transform.position += new Vector3(0.6f * Time.deltaTime, 0.2f * Time.deltaTime, 0f);
                S41GKit.Alpha(sr, 0.9f * (1f - k));
                yield return null;
            }
            Destroy(go);
        }

        void Drag()
        {
            if (!PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out bool ended)) { if (held != null) Drop(); return; }
            Vector2 w = PointerInput.ToWorld(cam, sp);
            if (began && held == null)
            {
                for (int i = critters.Count - 1; i >= 0; i--)
                    if (critters[i].col.OverlapPoint(w)) { held = critters[i]; break; }
                if (held != null)
                {
                    grabOffset = (Vector2)held.t.position - w;
                    held.sr.sortingOrder = 30;
                    Sfx.Play(this, "tick", 0.6f);
                }
            }
            if (held != null)
            {
                Vector2 p = w + grabOffset;
                held.t.position = new Vector3(Mathf.Clamp(p.x, -2.6f, 2.6f), Mathf.Clamp(p.y, -3.6f, 2.6f), held.t.position.z);
                held.t.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(Time.time * 18f) * 10f);
                if (ended) Drop();
            }
        }

        void Drop()
        {
            Critter c = held;
            held = null;
            c.t.localRotation = Quaternion.identity;
            c.sr.sortingOrder = 10 + critters.IndexOf(c) % 8;
            if (!binArea.OverlapPoint(c.t.position)) return;
            if (c.original) StartCoroutine(Caught(c));
            else StartCoroutine(WrongCopy(c));
        }

        IEnumerator Vanish(Critter c)
        {
            critters.Remove(c);
            c.col.enabled = false;
            Sfx.Play(this, "pop", 0.7f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.2f) { c.t.localScale = Vector3.one * (1f - k); yield return null; }
            Destroy(c.t.gameObject);
        }

        IEnumerator WrongCopy(Critter c)
        {
            busy = true;
            deleted++;
            UpdateTries();
            yield return StartCoroutine(Vanish(c));
            Sfx.Play(this, "buzz", 0.6f);
            Haptics.Medium();
            StartCoroutine(S41GKit.Shake(bin, 0.1f, 0.4f));
            statusText.text = critters.Count + " SUSPICIOUS FILES";
            if (bubble != null) bubble.Show(deleted >= 3 ? "Ha! You deleted my decoys. I'm still here!" : "Nope! Just a harmless copy. ACH... no.");
            if (deleted >= 3)
            {
                yield return new WaitForSeconds(1.6f);
                Fail();
                yield break;
            }
            yield return new WaitForSeconds(0.6f);
            busy = false;
        }

        IEnumerator Caught(Critter c)
        {
            busy = true;
            achoo.text = "";
            yield return StartCoroutine(Vanish(c));
            Sfx.Play(this, "clunk", 0.8f);
            Haptics.Light();
            yield return StartCoroutine(S41GKit.Shake(bin, 0.15f, 0.5f));
            if (bubble != null) bubble.Show("Noooo! Not the bin! ACHOOOO...");
            // le copie spariscono una dopo l'altra
            while (critters.Count > 0)
            {
                yield return StartCoroutine(Vanish(critters[critters.Count - 1]));
                yield return new WaitForSeconds(0.08f);
            }
            statusText.text = "SCAN COMPLETE: 0 VIRUSES";
            Sfx.Play(this, "ding", 0.9f);
            S41GKit.Confetti(confetti, true);
            yield return new WaitForSeconds(2.0f);
            S41GKit.Confetti(confetti, false);
            Win();
        }
    }
}
