using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "The overworked robot" — il robot deve premere quattro interruttori NELLO STESSO MOMENTO, ma ha solo due
    /// mani e gli interruttori sono sparsi (e tornano su appena lasciati). Soluzione: trascina gli interruttori a coppie
    /// nelle due zone sotto le mani; poi PRESS: ogni mano ne preme due insieme. Tre tentativi falliti = sconfitta.
    /// </summary>
    public class OverworkedRobotLevel : LevelController
    {
        [SerializeField] SpriteRenderer robotArt;
        [SerializeField] Sprite happySprite, onSprite, offSprite;
        [SerializeField] Transform robot, handL, handR, shoulderL, shoulderR, pressButton;
        [SerializeField] LineRenderer armL, armR;
        [SerializeField] Transform[] switches, slots;      // slot 0,1 = mano sinistra; 2,3 = mano destra
        [SerializeField] SpriteRenderer[] switchArts;
        [SerializeField] Collider2D pressArea;
        [SerializeField] TextMesh triesText;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] SmokeEmitter[] confetti;
        [SerializeField] float snapRadius = 0.7f;

        Camera cam;
        int[] slotOf;
        int held = -1, tries;
        Vector2 grabOffset, grabStart;
        Vector3 homeL, homeR;
        bool busy;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            slotOf = new int[switches.Length];
            for (int i = 0; i < slotOf.Length; i++) slotOf[i] = -1;
            homeL = handL.localPosition;
            homeR = handR.localPosition;
            UpdateTries();
            S41GKit.Confetti(confetti, false);
            if (bubble != null) bubble.Show("Four switches. At ONCE. With two hands. Sigh.");
        }

        void UpdateTries() { triesText.text = "FAILED ATTEMPTS: " + tries + "/3"; }

        void LateUpdate()
        {
            armL.SetPosition(0, shoulderL.position); armL.SetPosition(1, handL.position + Vector3.up * 0.25f);
            armR.SetPosition(0, shoulderR.position); armR.SetPosition(1, handR.position + Vector3.up * 0.25f);
        }

        void Update()
        {
            if (IsFinished || busy) return;
            if (!PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out bool ended)) { if (held >= 0) Release(Vector2.zero, false); return; }
            Vector2 w = PointerInput.ToWorld(cam, sp);
            if (began)
            {
                if (pressArea.OverlapPoint(w)) { StartCoroutine(Attempt()); return; }
                for (int i = 0; i < switches.Length; i++)
                    if (Vector2.Distance(w, switches[i].position) < 0.5f)
                    {
                        held = i;
                        grabOffset = (Vector2)switches[i].position - w;
                        grabStart = w;
                        if (slotOf[i] >= 0) slotOf[i] = -1;
                        Sfx.Play(this, "tick", 0.5f);
                        break;
                    }
            }
            if (held >= 0)
            {
                Vector2 p = w + grabOffset;
                switches[held].position = new Vector3(Mathf.Clamp(p.x, -2.5f, 2.5f), Mathf.Clamp(p.y, -3.9f, -1.6f), 0f);
                if (ended) Release(w, true);
            }
        }

        void Release(Vector2 w, bool real)
        {
            int i = held;
            held = -1;
            if (real && Vector2.Distance(w, grabStart) < 0.15f) { StartCoroutine(Flick(i)); return; }
            int best = -1;
            float bd = snapRadius;
            for (int s = 0; s < slots.Length; s++)
            {
                if (Occupied(s)) continue;
                float d = Vector2.Distance(switches[i].position, slots[s].position);
                if (d < bd) { bd = d; best = s; }
            }
            if (best < 0) return;
            slotOf[i] = best;
            switches[i].position = slots[best].position;
            Sfx.Play(this, "clunk", 0.5f);
        }

        bool Occupied(int s) { foreach (int o in slotOf) if (o == s) return true; return false; }

        /// <summary>Un tocco accende l'interruttore per un attimo: torna giù da solo.</summary>
        IEnumerator Flick(int i)
        {
            switchArts[i].sprite = onSprite;
            Sfx.Play(this, "tick", 0.7f);
            yield return new WaitForSeconds(0.35f);
            switchArts[i].sprite = offSprite;
            Sfx.Play(this, "tick", 0.4f);
            if (bubble != null) bubble.Show("They spring back! All four must be pressed together.");
        }

        IEnumerator Attempt()
        {
            busy = true;
            StartCoroutine(S41GKit.Pop(pressButton, 1.2f, 0.2f));
            bool grouped = true;
            foreach (int s in slotOf) if (s < 0) grouped = false;
            if (grouped)
            {
                Vector3 l = new Vector3((slots[0].localPosition.x + slots[1].localPosition.x) * 0.5f, slots[0].localPosition.y + 0.15f, 0f);
                Vector3 r = new Vector3((slots[2].localPosition.x + slots[3].localPosition.x) * 0.5f, slots[2].localPosition.y + 0.15f, 0f);
                StartCoroutine(S41GKit.Move(handL, l, 0.35f));
                yield return StartCoroutine(S41GKit.Move(handR, r, 0.35f));
                handL.localScale = handR.localScale = new Vector3(1.8f, 1f, 1f);
                foreach (SpriteRenderer a in switchArts) a.sprite = onSprite;
                Sfx.Play(this, "clunk", 0.9f);
                Haptics.Medium();
                yield return new WaitForSeconds(0.3f);
                robotArt.sprite = happySprite;
                Sfx.Play(this, "ding", 0.9f);
                if (bubble != null) bubble.Show("ALL FOUR AT ONCE! I'm a GENIUS! (You are too.)");
                yield return StartCoroutine(S41GKit.Pop(robot, 1.15f, 0.4f));
                S41GKit.Confetti(confetti, true);
                yield return new WaitForSeconds(1.8f);
                S41GKit.Confetti(confetti, false);
                Win();
                yield break;
            }

            // una alla volta: quando una mano si sposta, l'interruttore precedente torna giù
            if (bubble != null) bubble.Show("I'll just... press them one at a time...");
            int[] order = { 0, 1, 2, 3 };
            System.Array.Sort(order, (a, b) => switches[a].position.x.CompareTo(switches[b].position.x));
            int prevL = -1, prevR = -1;
            for (int k = 0; k < order.Length; k++)
            {
                int i = order[k];
                bool left = k % 2 == 0;
                Transform hand = left ? handL : handR;
                int prev = left ? prevL : prevR;
                if (prev >= 0) { switchArts[prev].sprite = offSprite; Sfx.Play(this, "tick", 0.4f); }
                yield return StartCoroutine(S41GKit.Move(hand, switches[i].localPosition + new Vector3(0f, 0.2f, 0f), 0.3f, 0.3f));
                switchArts[i].sprite = onSprite;
                Sfx.Play(this, "clunk", 0.6f);
                if (left) prevL = i; else prevR = i;
            }
            yield return new WaitForSeconds(0.2f);
            foreach (SpriteRenderer a in switchArts) a.sprite = offSprite;
            tries++;
            UpdateTries();
            Sfx.Play(this, "buzz", 0.7f);
            Haptics.Medium();
            StartCoroutine(S41GKit.Move(handL, homeL, 0.3f));
            yield return StartCoroutine(S41GKit.Move(handR, homeR, 0.3f));
            yield return StartCoroutine(S41GKit.Shake(robot, 0.12f, 0.5f));
            if (tries >= 3)
            {
                if (bubble != null) bubble.Show("ERROR... OVERHEATING... *bzzt* I quit.");
                yield return new WaitForSeconds(1.6f);
                Fail();
                yield break;
            }
            if (bubble != null) bubble.Show(tries == 1 ? "ERROR: they must all be ON at the same time!" : "Two hands aren't enough! Help me out here!");
            busy = false;
        }
    }
}
