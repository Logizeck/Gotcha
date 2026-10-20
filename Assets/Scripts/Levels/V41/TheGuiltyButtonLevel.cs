using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "The guilty button" — tre pulsanti interrogati da un detective giurano di essere innocenti; uno nasconde
    /// la chiave. Tenendo premuto un pulsante lo si mette sotto pressione e suda: quello colpevole perde una goccia
    /// METALLICA invece che d'acqua. Un tocco breve lo accusa; due accuse sbagliate = sconfitta.
    /// </summary>
    public class TheGuiltyButtonLevel : S41FLevelBase
    {
        [SerializeField] Transform[] suspects;          // base dei pulsanti
        [SerializeField] SpriteRenderer[] suspectArts;
        [SerializeField] Sprite idleSprite, pressedSprite;
        [SerializeField] Collider2D[] suspectAreas;
        [SerializeField] SpriteRenderer[] drops;        // gocce riutilizzate (una per pulsante)
        [SerializeField] Sprite waterDrop, metalDrop;
        [SerializeField] SpriteRenderer sparkle;
        [SerializeField] Transform key, detective;
        [SerializeField] TextMesh triesText;
        [SerializeField] float tapSeconds = 0.3f, sweatEvery = 0.75f, floorY = -1.55f;

        const int MaxWrong = 2;
        int guilty, wrong, held = -1;
        float heldFor, sweatTimer;
        bool[] dropBusy;

        static readonly string[] Alibis = { "I was home all night!", "Me? I'm just a button!", "I've never even SEEN a key!" };

        protected override void Begin()
        {
            guilty = Random.Range(0, suspects.Length);
            dropBusy = new bool[suspects.Length];
            foreach (SpriteRenderer d in drops) if (d != null) d.gameObject.SetActive(false);
            if (sparkle != null) sparkle.gameObject.SetActive(false);
            if (key != null) key.gameObject.SetActive(false);
            UpdateTries();
            Say("One of them hid the key. Hold them to put on the pressure, TAP to accuse!", 4f);
        }

        void UpdateTries() { if (triesText != null) triesText.text = "WRONG ACCUSATIONS " + wrong + "/" + MaxWrong; }

        protected override void Tick()
        {
            if (busy) return;
            bool has = Pointer(out Vector2 w, out bool began, out bool ended);
            if (has && began)
            {
                held = -1;
                for (int i = 0; i < suspectAreas.Length; i++)
                    if (suspectAreas[i] != null && suspectAreas[i].OverlapPoint(w)) { held = i; heldFor = 0f; sweatTimer = 0.45f; }
            }
            if (held < 0) return;
            bool inside = has && suspectAreas[held].OverlapPoint(w);
            if (has && !ended && inside)
            {
                heldFor += Time.deltaTime;
                Press(held, true);
                if (heldFor > tapSeconds)
                {
                    sweatTimer -= Time.deltaTime;
                    if (sweatTimer <= 0f) { sweatTimer = sweatEvery; StartCoroutine(Sweat(held)); }
                }
                return;
            }
            int i0 = held;
            float t = heldFor;
            held = -1;
            Press(i0, false);
            if (ended && inside && t <= tapSeconds) StartCoroutine(Accuse(i0));
        }

        void Press(int i, bool down)
        {
            if (suspectArts[i] != null) suspectArts[i].sprite = down ? pressedSprite : idleSprite;
            suspectArts[i].transform.localPosition = new Vector3(down ? Mathf.Sin(clock * 50f) * 0.02f * Mathf.Min(1f, heldFor) : 0f, suspectArts[i].transform.localPosition.y, 0f);
        }

        IEnumerator Sweat(int i)
        {
            if (dropBusy[i] || drops[i] == null) yield break;
            dropBusy[i] = true;
            bool metal = i == guilty;
            SpriteRenderer d = drops[i];
            d.sprite = metal ? metalDrop : waterDrop;
            d.gameObject.SetActive(true);
            Alpha(d, 1f);
            Vector3 start = suspects[i].position + new Vector3(0.38f, 0.95f, 0f);
            d.transform.position = start;
            d.transform.localScale = Vector3.one * (metal ? 1.4f : 1.2f);
            if (Random.value < 0.5f && bubble != null) Say(Alibis[i % Alibis.Length], 1.6f);
            // scivola lungo la cupola, poi cade
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.35f) { d.transform.position = start + new Vector3(0.12f * k, -0.35f * k, 0f); yield return null; }
            Vector3 from = d.transform.position;
            float v = 0f;
            while (d.transform.position.y > floorY)
            {
                v += 12f * Time.deltaTime;
                d.transform.position += Vector3.down * v * Time.deltaTime;
                yield return null;
            }
            d.transform.position = new Vector3(from.x, floorY, from.z);
            if (metal)
            {
                Sfx.Play(this, "tick", 1f);
                Sfx.Play(this, "ding", 0.25f);
                if (sparkle != null)
                {
                    sparkle.gameObject.SetActive(true);
                    sparkle.transform.position = d.transform.position + Vector3.up * 0.1f;
                    StartCoroutine(Pop(sparkle.transform, 1.5f, 0.3f));
                }
                yield return new WaitForSeconds(0.6f);
                if (sparkle != null) sparkle.gameObject.SetActive(false);
            }
            else Sfx.Play(this, "pop", 0.25f);
            for (float k = 1f; k > 0f; k -= Time.deltaTime / 0.4f) { Alpha(d, k); yield return null; }
            d.gameObject.SetActive(false);
            dropBusy[i] = false;
        }

        IEnumerator Accuse(int i)
        {
            busy = true;
            Sfx.Play(this, "bonk", 0.7f);
            yield return Pop(suspects[i], 1.15f, 0.2f);
            if (i == guilty)
            {
                Say("OKAY, OKAY! I did it! *cough*", 2.5f);
                Haptics.Light();
                yield return Shake(suspects[i], 0.08f, 0.5f);
                Sfx.Play(this, "pop", 1f);
                key.gameObject.SetActive(true);
                key.position = suspects[i].position + Vector3.up * 1.0f;
                Vector3 to = detective != null ? detective.position + new Vector3(-0.4f, 1.1f, 0f) : key.position + Vector3.up;
                Vector3 from = key.position;
                for (float k = 0f; k < 1f; k += Time.deltaTime / 0.7f)
                {
                    key.position = Vector3.Lerp(from, to, k) + Vector3.up * Mathf.Sin(k * Mathf.PI) * 1.2f;
                    key.localRotation = Quaternion.Euler(0, 0, 540f * k);
                    yield return null;
                }
                key.localRotation = Quaternion.identity;
                Sfx.Play(this, "ding", 0.6f);
                yield return Pop(key, 1.3f, 0.3f);
                yield return Celebrate();
                yield break;
            }
            wrong++;
            UpdateTries();
            Haptics.Medium();
            Sfx.Play(this, "buzz", 0.6f);
            if (wrong >= MaxWrong)
            {
                Say("Two innocent buttons accused. You're off the case!", 2.5f);
                yield return new WaitForSeconds(1.8f);
                Fail();
                yield break;
            }
            Say("INNOCENT! How DARE you! *sniff*", 2.2f);
            yield return Shake(suspects[i], 0.1f, 0.4f);
            busy = false;
        }
    }
}
