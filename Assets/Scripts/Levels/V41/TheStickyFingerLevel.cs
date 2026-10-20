using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "The sticky finger" — una moneta piena di gomma da masticare va messa nella cassaforte, ma resta
    /// appiccicata: lasciata andare torna su, e la gomma non arriva fino alla cassaforte. Scuoterla di brutto tre volte
    /// la fa volare via (sconfitta). Il trucco: tieni la moneta con un dito e con un secondo dito trascina la
    /// cassaforte sotto di lei; quando la fessura è sotto la moneta, la cassaforte se la prende da sola.
    /// </summary>
    public class TheStickyFingerLevel : S41FLevelBase
    {
        [SerializeField] Transform anchor;            // grumo di gomma
        [SerializeField] Transform coin;
        [SerializeField] Collider2D coinArea;
        [SerializeField] Transform strand;            // filo di gomma (sprite 1x1 scalato)
        [SerializeField] Transform safe;              // piedi della cassaforte
        [SerializeField] Collider2D safeArea;
        [SerializeField] SpriteRenderer safeArt;
        [SerializeField] Sprite safeHappy;
        [SerializeField] TextMesh triesText;
        [SerializeField] float maxLength = 5.2f, restDrop = 1.35f, slotHeight = 1.15f, minY = -3.0f;

        const int MaxShakes = 3;
        const int None = int.MinValue;
        int coinFinger = None, safeFinger = None;
        float safeOffset, lastFast = -9f, shakeCooldown;
        Vector2 lastCoin, fastDir;
        int shakes;
        bool saidSticky;

        Vector2 Rest => (Vector2)anchor.position + Vector2.down * restDrop;

        protected override void Begin()
        {
            coin.position = new Vector3(Rest.x, Rest.y, coin.position.z);
            lastCoin = coin.position;
            UpdateTries();
            UpdateStrand();
            Say("Put the coin in the safe. Easy, right?", 2.5f);
        }

        void UpdateTries() { if (triesText != null) triesText.text = "SHAKES " + shakes + "/" + MaxShakes; }

        protected override void Tick()
        {
            if (busy) return;
            shakeCooldown -= Time.deltaTime;
            List<Finger> list = Fingers();
            bool coinHeld = false, safeHeld = false;
            foreach (Finger f in list)
            {
                if (f.began)
                {
                    if (coinFinger == None && coinArea != null && coinArea.OverlapPoint(f.world)) { coinFinger = f.id; Sfx.Play(this, "pop", 0.4f); }
                    else if (safeFinger == None && safeArea != null && safeArea.OverlapPoint(f.world)) { safeFinger = f.id; safeOffset = safe.position.x - f.world.x; Sfx.Play(this, "tick", 0.5f); }
                }
                if (f.id == coinFinger && !f.ended) { coinHeld = true; HoldCoin(f.world); }
                if (f.id == safeFinger && !f.ended)
                {
                    safeHeld = true;
                    safe.position = new Vector3(Mathf.Clamp(f.world.x + safeOffset, -2.1f, 2.1f), safe.position.y, safe.position.z);
                }
            }
            if (busy) return;
            if (!safeHeld) safeFinger = None;
            if (!coinHeld)
            {
                if (coinFinger != None && !saidSticky) { saidSticky = true; Say("It won't let go! This gum is SO sticky!", 2.5f); }
                coinFinger = None;
                Vector2 p = Vector2.Lerp(coin.position, Rest, 1f - Mathf.Exp(-Time.deltaTime * 7f));
                coin.position = new Vector3(p.x, p.y, coin.position.z);
            }
            UpdateStrand();
            lastCoin = coin.position;
            Vector2 slot = (Vector2)safe.position + Vector2.up * slotHeight;
            if (Vector2.Distance(coin.position, slot) < 0.45f) StartCoroutine(Deposit());
        }

        void HoldCoin(Vector2 target)
        {
            Vector2 a = anchor.position;
            target.y = Mathf.Max(target.y, minY);
            Vector2 d = target - a;
            bool stretched = d.magnitude > maxLength;
            if (stretched) target = a + d.normalized * maxLength;
            coin.position = new Vector3(target.x, target.y, coin.position.z);
            // scossoni bruschi: cambio di direzione improvviso ad alta velocità
            Vector2 v = (target - lastCoin) / Mathf.Max(Time.deltaTime, 0.001f);
            if (v.magnitude > 12f)
            {
                Vector2 dir = v.normalized;
                if (clock - lastFast < 0.3f && Vector2.Dot(dir, fastDir) < -0.3f && shakeCooldown <= 0f) { StartCoroutine(Shaken()); return; }
                fastDir = dir;
                lastFast = clock;
            }
        }

        void UpdateStrand()
        {
            if (strand == null) return;
            Vector2 a = anchor.position, c = coin.position;
            Vector2 d = c - a;
            float len = d.magnitude;
            strand.position = new Vector3((a.x + c.x) * 0.5f, (a.y + c.y) * 0.5f, strand.position.z);
            strand.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg - 90f);
            float thin = Mathf.Lerp(0.12f, 0.04f, Mathf.Clamp01(len / maxLength));
            strand.localScale = new Vector3(thin, Mathf.Max(0.01f, len), 1f);
        }

        IEnumerator Shaken()
        {
            busy = true;
            shakes++;
            shakeCooldown = 0.8f;
            UpdateTries();
            Haptics.Medium();
            Sfx.Play(this, "bonk", 0.7f);
            coinFinger = None;
            if (shakes >= MaxShakes)
            {
                Say("SPROING! The coin flew away!", 2.5f);
                if (strand != null) strand.gameObject.SetActive(false);
                Sfx.Play(this, "whoosh", 1f);
                Vector3 from = coin.position;
                for (float k = 0f; k < 1f; k += Time.deltaTime / 0.8f)
                {
                    coin.position = from + new Vector3(4f * k, 3f * k - 6f * k * k, 0f);
                    coin.localRotation = Quaternion.Euler(0, 0, -720f * k);
                    yield return null;
                }
                Haptics.Heavy();
                yield return new WaitForSeconds(0.8f);
                Fail();
                yield break;
            }
            Say(shakes == 1 ? "Shaking won't help, it's stuck!" : "Careful! The gum is about to snap!", 2.2f);
            yield return Shake(coin, 0.1f, 0.35f);
            busy = false;
        }

        IEnumerator Deposit()
        {
            busy = true;
            if (strand != null) strand.gameObject.SetActive(false);
            Sfx.Play(this, "pop", 0.9f);
            Vector3 slot = safe.position + Vector3.up * slotHeight;
            Vector3 from = coin.position;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.35f)
            {
                coin.position = Vector3.Lerp(from, slot, k);
                coin.localScale = Vector3.one * Mathf.Lerp(1f, 0.3f, k);
                yield return null;
            }
            coin.gameObject.SetActive(false);
            Haptics.Light();
            if (safeArt != null && safeHappy != null) safeArt.sprite = safeHappy;
            Say("CHA-CHING! Yum, minty.", 2.2f);
            yield return Pop(safe, 1.18f, 0.3f);
            yield return Celebrate();
        }
    }
}
