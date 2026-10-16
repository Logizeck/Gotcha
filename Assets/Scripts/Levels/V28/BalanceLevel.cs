using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 78 — la moneta falsa. Nove monete d'oro uguali, ma una è di piombo (pesa di più). Tocca una moneta per
    /// spostarla: tavolo → piatto sinistro → piatto destro → tavolo. WEIGH pesa (solo DUE pesate!). Poi tocca la falsa.
    /// </summary>
    public class BalanceLevel : LevelController
    {
        [SerializeField] Transform[] coins;
        [SerializeField] Transform beam;              // pivot al centro, ruota
        [SerializeField] Transform leftPan, rightPan;
        [SerializeField] TapButton weighButton;
        [SerializeField] TextMesh weighLabel;
        [SerializeField] TextMesh info;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] float panReach = 1.5f;

        int fake, weighsLeft = 2;
        int[] place;                                  // 0 tavolo, 1 sinistra, 2 destra
        Vector3[] tablePos;
        float tilt, tiltTarget;
        bool picking, busy, done;
        Camera cam;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            fake = Random.Range(0, coins.Length);
            place = new int[coins.Length];
            tablePos = new Vector3[coins.Length];
            for (int i = 0; i < coins.Length; i++) tablePos[i] = coins[i].position;
            weighButton.Pressed += _ => { if (!busy && !done) StartCoroutine(Weigh()); };
            Info();
            if (bubble != null) { bubble.Show("One coin is lead. Two weighings. Go!"); StartCoroutine(HideLater(2.2f)); }
        }

        void Info()
        {
            if (info != null) info.text = picking ? "TAP THE FAKE COIN" : "WEIGHINGS LEFT: " + weighsLeft;
            if (weighLabel != null) weighLabel.text = picking ? "PICK" : "WEIGH";
        }

        void Update()
        {
            tilt = Mathf.MoveTowards(tilt, tiltTarget, 40f * Time.deltaTime);
            beam.localRotation = Quaternion.Euler(0, 0, tilt);
            float r = tilt * Mathf.Deg2Rad;
            Vector3 b = beam.position;
            leftPan.position = b + new Vector3(-Mathf.Cos(r) * panReach, -Mathf.Sin(r) * panReach - 0.9f, 0f);
            rightPan.position = b + new Vector3(Mathf.Cos(r) * panReach, Mathf.Sin(r) * panReach - 0.9f, 0f);
            if (!done) Layout();
            if (done || busy || IsFinished) return;
            if (!PointerInput.AnyTouchBegan() || !PointerInput.TryGetPrimary(out Vector2 sp, out _, out _)) return;
            Vector2 w = PointerInput.ToWorld(cam, sp);
            for (int i = 0; i < coins.Length; i++)
            {
                if (Vector2.Distance(w, coins[i].position) > 0.33f) continue;
                if (picking) { StartCoroutine(Pick(i)); return; }
                place[i] = (place[i] + 1) % 3;
                tiltTarget = 0f;   // spostando le monete la bilancia torna dritta
                Sfx.Play(this, "tick");
                return;
            }
        }

        void Layout()
        {
            int l = 0, rr = 0;
            for (int i = 0; i < coins.Length; i++)
            {
                Vector3 to = place[i] == 0 ? tablePos[i]
                    : place[i] == 1 ? leftPan.position + new Vector3(-0.42f + (l % 3) * 0.42f, 0.2f + (l++ / 3) * 0.2f, 0f)
                    : rightPan.position + new Vector3(-0.42f + (rr % 3) * 0.42f, 0.2f + (rr++ / 3) * 0.2f, 0f);
                coins[i].position = Vector3.Lerp(coins[i].position, to, 1f - Mathf.Exp(-14f * Time.deltaTime));
            }
        }

        IEnumerator Weigh()
        {
            if (picking) yield break;
            busy = true;
            float left = 0f, right = 0f;
            for (int i = 0; i < coins.Length; i++)
            {
                float m = i == fake ? 1.2f : 1f;
                if (place[i] == 1) left += m; else if (place[i] == 2) right += m;
            }
            Sfx.Play(this, "clunk", 0.8f);
            tiltTarget = Mathf.Approximately(left, right) ? 0f : (left > right ? 14f : -14f);   // piatto più pesante in basso
            weighsLeft--;
            yield return new WaitForSeconds(0.9f);
            if (weighsLeft <= 0) picking = true;
            Info();
            busy = false;
        }

        IEnumerator Pick(int i)
        {
            done = true;
            Vector3 p = coins[i].position;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.3f) { coins[i].position = p + Vector3.up * Mathf.Sin(k * Mathf.PI) * 0.4f; yield return null; }
            if (i == fake)
            {
                coins[i].GetComponentInChildren<SpriteRenderer>().color = new Color(0.6f, 0.62f, 0.7f);
                if (bubble != null) bubble.Show("LEAD! Busted, you fake.");
                yield return new WaitForSeconds(1.5f);
                Win();
            }
            else
            {
                if (bubble != null) bubble.Show("Real gold. Oops.");
                yield return new WaitForSeconds(1.5f);
                Fail();
            }
        }

        IEnumerator HideLater(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (bubble != null && !done) bubble.Hide();
        }
    }
}
