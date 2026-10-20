using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "The second key" — un baule con due serrature e una sola chiave. Il trucco: appoggiare la chiave sul
    /// comò davanti allo specchio e toccarne il riflesso, che diventa una seconda chiave vera; poi infilare le due
    /// chiavi nelle due serrature. Tre tentativi di aprire con una chiave sola = sconfitta.
    /// </summary>
    public class TheSecondKeyLevel : LevelController
    {
        [SerializeField] Transform[] keys, holes;
        [SerializeField] Collider2D[] keyAreas;
        [SerializeField] Transform dresserSpot, chest;
        [SerializeField] SpriteRenderer reflection, chestArt, treasureGlow, sparkle;
        [SerializeField] Collider2D reflectionArea;
        [SerializeField] Sprite chestOpenSprite;
        [SerializeField] TextMesh missText;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] SmokeEmitter[] confetti;
        [SerializeField] int missesAllowed = 3;
        [SerializeField] float snapDistance = 0.75f;

        Camera cam;
        int drag = -1, dragFinger, misses;
        int[] inHole;         // chiave infilata in ciascuna serratura (-1 = vuota)
        bool busy, copied;
        Vector2 dragOffset;
        Vector3[] keyHome;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            inHole = new[] { -1, -1 };
            keyHome = new Vector3[keys.Length];
            for (int i = 0; i < keys.Length; i++) keyHome[i] = keys[i].position;
            keys[1].gameObject.SetActive(false);
            reflection.gameObject.SetActive(false);
            treasureGlow.gameObject.SetActive(false);
            S41HKit.Alpha(sparkle, 0f);
            S41HKit.Confetti(confetti, false);
            UpdateMisses();
            S41HKit.Say(bubble, "Two locks. One key. Hmm...");
        }

        void UpdateMisses() { if (missText != null) missText.text = "FAILED TRIES " + misses + "/" + missesAllowed; }

        bool OnDresser(int k) => keys[k].gameObject.activeSelf && Vector2.Distance(keys[k].position, dresserSpot.position) < 0.15f;

        void Update()
        {
            if (IsFinished) return;
            // il riflesso compare quando una chiave è davanti allo specchio
            bool show = (OnDresser(0) || OnDresser(1)) && drag < 0;
            if (reflection.gameObject.activeSelf != show) reflection.gameObject.SetActive(show);
            if (show) S41HKit.Alpha(reflection, (copied ? 0.45f : 0.65f) + Mathf.Sin(Time.time * 3f) * 0.12f);
            if (busy) return;

            foreach (S41HKit.Finger f in S41HKit.Fingers(cam))
            {
                if (f.began && drag < 0)
                {
                    for (int k = keys.Length - 1; k >= 0; k--)
                    {
                        if (!keys[k].gameObject.activeSelf || System.Array.IndexOf(inHole, k) >= 0 || !keyAreas[k].OverlapPoint(f.world)) continue;
                        drag = k;
                        dragFinger = f.id;
                        dragOffset = (Vector2)keys[k].position - f.world;
                        keys[k].localRotation = Quaternion.identity;
                        Sfx.Play(this, "tick", 0.5f);
                        break;
                    }
                    if (drag < 0 && show && reflectionArea.OverlapPoint(f.world)) { StartCoroutine(Materialize()); return; }
                }
                if (drag >= 0 && f.id == dragFinger)
                {
                    Vector2 p = f.world + dragOffset;
                    keys[drag].position = new Vector3(Mathf.Clamp(p.x, -2.5f, 2.5f), Mathf.Clamp(p.y, -4.3f, 2.6f), 0f);
                    if (f.ended) { int k = drag; drag = -1; Drop(k); }
                }
            }
            if (drag >= 0 && S41HKit.Fingers(cam).Count == 0) { int k = drag; drag = -1; Drop(k); }
        }

        void Drop(int k)
        {
            Vector3 p = keys[k].position;
            if (Vector2.Distance(p, dresserSpot.position) < snapDistance)
            {
                keys[k].position = dresserSpot.position;
                Sfx.Play(this, "clunk", 0.5f);
                if (!copied) S41HKit.Say(bubble, "Look! There's a key in the mirror too...");
                return;
            }
            for (int h = 0; h < holes.Length; h++)
            {
                if (inHole[h] >= 0 || Vector2.Distance(p, holes[h].position) > snapDistance) continue;
                if (!keys[1].gameObject.activeSelf) { StartCoroutine(OneKeyTry(k)); return; }
                inHole[h] = k;
                keys[k].position = holes[h].position + new Vector3(0f, 0.35f, 0f);
                keys[k].localRotation = Quaternion.Euler(0, 0, -90f);
                Sfx.Play(this, "clunk", 0.8f);
                Haptics.Light();
                if (inHole[0] >= 0 && inHole[1] >= 0) StartCoroutine(Victory());
                else S41HKit.Say(bubble, "Click! One down...");
                return;
            }
        }

        IEnumerator Materialize()
        {
            busy = true;
            if (copied)
            {
                S41HKit.Say(bubble, "Only one copy per mirror!");
                yield return new WaitForSeconds(0.6f);
                busy = false;
                yield break;
            }
            copied = true;
            Sfx.Play(this, "ding", 0.7f);
            sparkle.transform.position = reflection.transform.position;
            Transform k2 = keys[1];
            k2.position = reflection.transform.position;
            k2.localScale = Vector3.one * 0.2f;
            k2.gameObject.SetActive(true);
            for (float t = 0f; t < 1f; t += Time.deltaTime / 0.5f)
            {
                k2.localScale = Vector3.one * Mathf.Lerp(0.2f, 1f, t);
                S41HKit.Alpha(sparkle, 1f - t);
                sparkle.transform.localScale = Vector3.one * (0.6f + t * 1.4f);
                yield return null;
            }
            S41HKit.Alpha(sparkle, 0f);
            k2.localScale = Vector3.one;
            yield return StartCoroutine(S41HKit.Move(k2, k2.parent.InverseTransformPoint(keyHome[1]), 0.5f, 0.4f));
            S41HKit.Say(bubble, "A REAL copy! Now: both keys!");
            busy = false;
        }

        IEnumerator OneKeyTry(int k)
        {
            busy = true;
            misses++;
            UpdateMisses();
            Sfx.Play(this, "bonk", 0.8f);
            Haptics.Medium();
            S41HKit.Say(bubble, misses == 1 ? "It won't budge with just one key!" : "Still one key. Still locked.");
            StartCoroutine(S41HKit.Shake(chest, 0.1f, 0.4f));
            yield return StartCoroutine(S41HKit.Move(keys[k], keys[k].parent.InverseTransformPoint(keyHome[k]), 0.4f, 0.4f));
            if (misses >= missesAllowed)
            {
                S41HKit.Say(bubble, "The chest is fed up. It ate the key!");
                keys[k].gameObject.SetActive(false);
                yield return new WaitForSeconds(1.3f);
                Fail();
                yield break;
            }
            busy = false;
        }

        IEnumerator Victory()
        {
            busy = true;
            yield return new WaitForSeconds(0.3f);
            foreach (Transform key in keys) key.gameObject.SetActive(false);
            chestArt.sprite = chestOpenSprite;
            treasureGlow.gameObject.SetActive(true);
            Sfx.Play(this, "ding", 0.9f);
            Haptics.Medium();
            StartCoroutine(S41HKit.Pop(chest, 0.15f, 0.3f));
            S41HKit.Say(bubble, "TREASURE! Mirrors are amazing.");
            S41HKit.Confetti(confetti, true);
            for (float t = 0f; t < 2.2f; t += Time.deltaTime)
            {
                treasureGlow.transform.localScale = Vector3.one * (1f + Mathf.Sin(t * 6f) * 0.08f);
                yield return null;
            }
            S41HKit.Confetti(confetti, false);
            Win();
        }
    }
}
