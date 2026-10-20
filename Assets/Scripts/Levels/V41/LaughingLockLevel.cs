using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "The laughing lock" — il lucchetto ride senza sosta e la chiave rimbalza (tre inserimenti mentre ride
    /// spezzano la chiave = sconfitta). Soluzione: tieni premuta la maniglia: il lucchetto ride sempre più piano,
    /// si stanca e sbadiglia a bocca aperta; mentre sbadiglia trascina la chiave nella bocca.
    /// </summary>
    public class LaughingLockLevel : LevelController
    {
        [SerializeField] Transform lockBody, handle, key, keyHole, tiredFill, door;
        [SerializeField] SpriteRenderer lockArt;
        [SerializeField] Sprite laughSprite, tiredSprite, yawnSprite, openSprite;
        [SerializeField] Collider2D handleArea;
        [SerializeField] GameObject[] cracks;
        [SerializeField] TextMesh haText, triesText;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] SmokeEmitter[] confetti;
        [SerializeField] float tireSeconds = 3f, yawnSeconds = 3.5f;

        Camera cam;
        float tired, yawnLeft, haTimer;
        int keyFinger = -2, breaks;
        Vector2 grabOffset;
        Vector3 keyHome, handleHome;
        bool busy, holdTold;

        bool Yawning => yawnLeft > 0f;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            keyHome = key.localPosition;
            handleHome = handle.localEulerAngles;
            foreach (GameObject c in cracks) c.SetActive(false);
            S41KKit.Confetti(confetti, false);
            UpdateTries();
            if (bubble != null) bubble.Show("HAHAHA! A key? For ME? HAHAHA!");
        }

        void UpdateTries() { triesText.text = "KEY DAMAGE: " + breaks + "/3"; }

        void Update()
        {
            if (IsFinished || busy) return;
            bool handleHeld = false;
            foreach (S41KKit.Finger f in S41KKit.Fingers(cam))
            {
                if (f.began && keyFinger == -2 && Vector2.Distance(f.world, key.TransformPoint(new Vector3(-0.6f, 0f, 0f))) < 0.8f)
                {
                    keyFinger = f.id;
                    grabOffset = (Vector2)key.position - f.world;
                    Sfx.Play(this, "tick", 0.5f);
                }
                if (f.id == keyFinger)
                {
                    Vector2 p = f.world + grabOffset;
                    key.position = new Vector3(Mathf.Clamp(p.x, -2.5f, 2.5f), Mathf.Clamp(p.y, -4.0f, 2.2f), 0f);
                    if (f.ended) { keyFinger = -2; DropKey(); if (busy) return; }
                    continue;
                }
                if (!f.ended && handleArea.OverlapPoint(f.world)) handleHeld = true;
            }
            if (keyFinger != -2 && !KeyFingerAlive()) { keyFinger = -2; DropKey(); if (busy) return; }
#if UNITY_EDITOR
            if (Input.GetKey(KeyCode.H)) handleHeld = true;   // Editor: H = dito sulla maniglia
#endif
            handle.localEulerAngles = handleHome + new Vector3(0f, 0f, handleHeld ? -25f : 0f);

            if (Yawning)
            {
                if (!handleHeld) yawnLeft -= Time.deltaTime;
                if (yawnLeft <= 0f)
                {
                    tired = 0.35f;
                    lockArt.sprite = laughSprite;
                    if (bubble != null) bubble.Show("Mmh? Oh! HAHAHA! I'm awake again!");
                }
            }
            else
            {
                tired = Mathf.Clamp01(tired + (handleHeld ? Time.deltaTime / tireSeconds : -Time.deltaTime / 6f));
                if (handleHeld && !holdTold && bubble != null) { bubble.Show("Hihi, that tickles! Hahaha... ha... haaa..."); holdTold = true; }
                if (tired >= 1f) StartYawn();
                else lockArt.sprite = tired > 0.6f ? tiredSprite : laughSprite;
            }
            tiredFill.localScale = new Vector3(Yawning ? 1f : tired, 1f, 1f);

            // risata: dondola e scrive HA HA
            bool laughing = !Yawning && tired < 0.6f;
            float speed = Yawning ? 0f : Mathf.Lerp(18f, 4f, tired);
            lockBody.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(Time.time * speed) * (Yawning ? 0f : Mathf.Lerp(7f, 2f, tired)));
            haText.gameObject.SetActive(laughing);
            haTimer -= Time.deltaTime;
            if (laughing && haTimer <= 0f)
            {
                haTimer = 0.45f;
                S41KKit.Tone(this, Random.Range(420f, 560f), 0.15f, 0.25f);
                haText.transform.localScale = Vector3.one * Random.Range(0.85f, 1.15f);
            }
        }

        bool KeyFingerAlive()
        {
            foreach (S41KKit.Finger f in S41KKit.Fingers(cam)) if (f.id == keyFinger) return true;
            return false;
        }

        void StartYawn()
        {
            yawnLeft = yawnSeconds;
            lockArt.sprite = yawnSprite;
            Sfx.Play(this, "snore", 0.6f);
            if (bubble != null) bubble.Show("Ha... haaa... *YAAAWN*");
        }

        void DropKey()
        {
            if (Vector2.Distance(key.position, keyHole.position) > 0.75f) return;
            if (Yawning) StartCoroutine(WinRoutine());
            else StartCoroutine(Bounce());
        }

        IEnumerator Bounce()
        {
            busy = true;
            breaks++;
            UpdateTries();
            Sfx.Play(this, "bonk", 0.8f);
            Haptics.Medium();
            for (int i = 0; i < cracks.Length; i++) cracks[i].SetActive(i < breaks);
            StartCoroutine(S41KKit.Shake(lockBody, 0.1f, 0.4f));
            if (breaks >= 3)
            {
                if (bubble != null) bubble.Show("SNAP! HAHAHA! Your key is in pieces!");
                Haptics.Heavy();
                yield return StartCoroutine(S41KKit.Move(key, keyHome, 0.5f, 0.6f));
                key.localRotation = Quaternion.Euler(0f, 0f, 70f);
                yield return new WaitForSeconds(1.3f);
                Fail();
                yield break;
            }
            if (bubble != null) bubble.Show(breaks == 1 ? "HAHAHA! You can't get in while I laugh!" : "Careful, your key is cracking! HAHA!");
            yield return StartCoroutine(S41KKit.Move(key, keyHome, 0.45f, 0.6f));
            busy = false;
        }

        IEnumerator WinRoutine()
        {
            busy = true;
            haText.gameObject.SetActive(false);
            yield return StartCoroutine(S41KKit.Move(key, keyHole.localPosition, 0.2f));
            for (float t = 0f; t < 1f; t += Time.deltaTime / 0.35f)
            {
                key.localRotation = Quaternion.Euler(0f, 0f, -90f * t);
                yield return null;
            }
            Sfx.Play(this, "clunk", 0.9f);
            Haptics.Light();
            lockArt.sprite = openSprite;
            if (bubble != null) bubble.Show("Mmf! ...hey, no fair, I was yawning!");
            yield return new WaitForSeconds(0.6f);
            key.SetParent(lockBody, true);
            Sfx.Play(this, "bonk", 0.7f);
            yield return StartCoroutine(S41KKit.Move(lockBody, lockBody.localPosition + new Vector3(0f, -4.2f, 0f), 0.6f));
            Haptics.Medium();
            Sfx.Play(this, "ding", 0.9f);
            yield return StartCoroutine(S41KKit.ScaleTo(door, new Vector3(0.15f, 1f, 1f), 0.6f));
            S41KKit.Confetti(confetti, true);
            yield return new WaitForSeconds(1.8f);
            S41KKit.Confetti(confetti, false);
            Win();
        }
    }
}
