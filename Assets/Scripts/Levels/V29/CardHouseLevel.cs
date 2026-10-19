using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;
using Gotcha.Sensors;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 85 — il castello di carte. Tocca per posare la carta successiva. Il castello sente ogni tremolio della
    /// mano: tieni il telefono FERMO. E dalla finestra ogni tanto entra una folata (la tenda si muove): se posi una
    /// carta mentre la tenda sventola, il castello oscilla tantissimo. Aspetta la calma. Nove carte.
    /// </summary>
    public class CardHouseLevel : LevelController
    {
        [SerializeField] Transform tower;             // pivot alla base
        [SerializeField] Transform[] cards;           // in ordine di posa (nascoste all'inizio)
        [SerializeField] Transform curtain;
        [SerializeField] Transform swayBar;
        [SerializeField] TextMesh counter;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] float handGain = 2.2f, gustKick = 9f, placeKick = 1.5f, fallAt = 9f, settle = 2.2f;

        int placed;
        float sway, swayVel, gust, nextGust = 4f;
        Vector3 lastAccel;
        bool done, primed;

        public override void OnLevelStart()
        {
            foreach (Transform c in cards) c.gameObject.SetActive(false);
            Count();
            if (bubble != null) { bubble.Show("Steady hands... and mind the window."); StartCoroutine(HideLater(2.2f)); }
        }

        void Count() { counter.text = placed + " / " + cards.Length; }

        void Update()
        {
            if (done || IsFinished) return;
            float dt = Time.deltaTime;
            // tremolio della mano: variazioni dell'accelerometro
            Vector3 a = Input.acceleration;
            float jitter = primed ? (a - lastAccel).magnitude / Mathf.Max(dt, 0.005f) : 0f;
            lastAccel = a;
            primed = true;
#if UNITY_EDITOR
            jitter = Inputs.Shake() > 1f ? 6f : 0f;
#endif
            float height = 0.4f + placed * 0.15f;   // più è alto, più oscilla
            swayVel += (Random.value - 0.5f) * Mathf.Min(jitter, 30f) * handGain * height * dt;
            // folate dalla finestra
            nextGust -= dt;
            if (nextGust <= 0f) { gust = 1.6f; nextGust = Random.Range(4f, 7f); }
            if (gust > 0f) { gust -= dt; swayVel += Mathf.Sin(Time.time * 9f) * 0.6f * height * dt; }
            curtain.localRotation = Quaternion.Euler(0, 0, gust > 0f ? Mathf.Sin(Time.time * 12f) * 18f : Mathf.Sin(Time.time * 1.5f) * 2f);
            swayVel -= sway * settle * dt;
            swayVel *= Mathf.Exp(-1.6f * dt);
            sway += swayVel * dt * 10f;
            tower.localRotation = Quaternion.Euler(0, 0, sway);
            if (swayBar != null) swayBar.localScale = new Vector3(Mathf.Clamp01(Mathf.Abs(sway) / fallAt), 1f, 1f);
            if (Mathf.Abs(sway) > fallAt) { StartCoroutine(Collapse()); return; }

            if (PointerInput.AnyTouchBegan() && !PointerInput.IsOverUi() && placed < cards.Length)
            {
                cards[placed].gameObject.SetActive(true);
                StartCoroutine(Drop(cards[placed]));
                placed++;
                Count();
                Sfx.Play(this, "tick", 0.6f);
                swayVel += (Random.value < 0.5f ? -1f : 1f) * (placeKick + (gust > 0f ? gustKick : 0f)) * height;
                if (gust > 0f && bubble != null) { bubble.Show("Whoa, the wind!", true); StartCoroutine(HideLater(0.8f)); }
                if (placed >= cards.Length) StartCoroutine(AllPlaced());
            }
        }

        static IEnumerator Drop(Transform c)
        {
            Vector3 to = c.localPosition;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.15f) { c.localPosition = to + Vector3.up * (1f - k) * 0.6f; yield return null; }
            c.localPosition = to;
            Haptics.Light();
        }

        IEnumerator AllPlaced()
        {
            yield return new WaitForSeconds(1.5f);   // deve restare in piedi ancora un attimo
            if (done) yield break;
            done = true;
            Sfx.Play(this, "ding", 0.8f);
            if (bubble != null) bubble.Show("A masterpiece. Don't sneeze.");
            yield return new WaitForSeconds(1.4f);
            Win();
        }

        IEnumerator Collapse()
        {
            done = true;
            Haptics.Medium();
            Sfx.Play(this, "bonk", 0.8f);
            float side = Mathf.Sign(sway);
            var falling = new List<Transform>();
            foreach (Transform c in cards) if (c.gameObject.activeSelf) falling.Add(c);
            for (float k = 0f; k < 1.2f; k += Time.deltaTime)
            {
                for (int i = 0; i < falling.Count; i++)
                {
                    Transform c = falling[i];
                    c.position += new Vector3(side * (0.5f + i * 0.15f), -3f * k, 0f) * Time.deltaTime * 2f;
                    c.Rotate(0, 0, side * -200f * Time.deltaTime);
                }
                yield return null;
            }
            if (bubble != null) bubble.Show("...and down it goes.");
            yield return new WaitForSeconds(1f);
            Fail();
        }

        IEnumerator HideLater(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (bubble != null && !done) bubble.Hide();
        }
    }
}
