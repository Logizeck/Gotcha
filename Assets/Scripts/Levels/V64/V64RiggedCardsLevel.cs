using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;
using Gotcha.Levels.V64;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 258 "The rigged three-card game" — un truffatore di strada mostra una Regina e mescola piano (sempre gli
    /// stessi scambi). Qualsiasi carta scelta è il due di picche (ride); spostare le carte col dito gliele fa riordinare;
    /// scuotere sparpaglia le monete (le raccoglie). Soluzione: la Regina non c'è, la si fabbrica. Due dita su due carte,
    /// telefono inclinato verso un lato del tavolo: la terza scivola e cade. Se era quella di mezzo scopre una macchiolina
    /// d'inchiostro a forma di corona (se ne vede appena un bordo sotto la carta) e il mazziere si china a cercare la
    /// carta. Si trascina la macchia su una delle due carte rimaste: diventa una Regina; la si tocca e si vince.
    /// Dopo la vittoria il cilindro abbandonato nasconde un Jolly (gag, non è un Easter egg).
    /// Logica in Gotcha.Levels.V64.RiggedCardsLogic (senza Unity): qui solo input e presentazione.
    /// </summary>
    public class V64RiggedCardsLevel : V62Level
    {
        [SerializeField] Transform[] cards;              // 3 carte (figlie della radice)
        [SerializeField] SpriteRenderer[] cardArts;
        [SerializeField] Collider2D[] cardAreas;
        [SerializeField] Sprite backSprite, twoSprite, queenInkSprite;
        [SerializeField] Sprite[] introFaces;            // le facce mostrate prima del mescolamento (la carta 0 è la "Regina")
        [SerializeField] Transform stain;
        [SerializeField] Collider2D stainArea;
        [SerializeField] Transform dealer;
        [SerializeField] Transform dealerHat;            // il cilindro in testa (figlio del mazziere)
        [SerializeField] Collider2D dealerArea;
        [SerializeField] SpeechBubble dealerBubble;
        [SerializeField] Transform miniDeck;             // il mazzo che controlla alla fine (nascosto)
        [SerializeField] Transform[] coins;
        [SerializeField] Collider2D coinsArea;
        [SerializeField] Transform droppedHat;           // il cilindro abbandonato sul tavolo (nascosto)
        [SerializeField] Collider2D hatArea;
        [SerializeField] Transform joker;                // il Jolly nascosto nel cilindro (nascosto)
        [SerializeField] TextMesh infoText;
        [SerializeField] Transform infoHolder;

        static readonly string[] RememberedLines = { "Ha! Great memory. Wrong card.", "That WAS the Queen. Was.", "Two of spades. Again. Tragic." };
        static readonly string[] OtherLines = { "Two of spades! Try again, pal.", "Nope. Deuce.", "Everybody's a two today." };
        static readonly string[] RearrangeLines = { "Hands off the merchandise!", "Ah-ah-ah. I do the moving.", "Let me just... fix that for you." };
        static readonly string[] SearchLines = { "Where'd that card go...?", "Is that a cat? Give it back.", "Found gum. Not a card." };

        const float CardY = -1.75f;
        const float TapTime = 0.35f, DragStart = 0.4f, InkReach = 0.75f;
        const float ShakeLevel = 12f;

        sealed class Touch
        {
            public int Card = -1;
            public Vector2 Start;
            public float T0;
            public bool Multi, Moved, Ink;
        }

        readonly RiggedCardsLogic logic = new RiggedCardsLogic();
        readonly Dictionary<int, Touch> touches = new Dictionary<int, Touch>();
        readonly List<int> stale = new List<int>();
        readonly HashSet<int> seen = new HashSet<int>();
        readonly bool[] held = new bool[RiggedCardsLogic.Cards];
        readonly bool[] flipping = new bool[RiggedCardsLogic.Cards];
        Camera cam;
        HwToast toast;
        HwSay dealerSay;
        Vector3 stainHome, dealerHome, hatRestLocal, jokerLocal;
        Vector3[] coinHome;
        Vector2 inkOffset;
        int[] baseOrder;
        int guessIdx, rearrangeIdx, searchIdx;
        float shakeReadyAt, peekReadyAt, nextSearchLine;
        bool busy, ducked, inkDragging, hatWindow, jokerShown;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            logic.Reset();
            ResetV62();
            toast = new HwToast(infoText, infoHolder);
            dealerSay = new HwSay(dealerBubble);
            stainHome = stain.localPosition;
            dealerHome = dealer.localPosition;
            hatRestLocal = droppedHat.localPosition;
            jokerLocal = joker.localPosition;
            coinHome = new Vector3[coins.Length];
            for (int i = 0; i < coins.Length; i++) coinHome[i] = coins[i].localPosition;
            baseOrder = new int[cardArts.Length];
            for (int i = 0; i < cardArts.Length; i++) baseOrder[i] = cardArts[i].sortingOrder;
            HalloweenKit.Show(droppedHat, false);
            HalloweenKit.Show(joker, false);
            HalloweenKit.Show(miniDeck, false);
            touches.Clear();
            StartCoroutine(Intro());
        }

        Vector3 SlotPos(int slot) => new Vector3(RiggedCardsLogic.SlotX[slot], CardY, 0f);

        // ---------- Mescolamento (sempre uguale) ----------

        IEnumerator Intro()
        {
            busy = true;
            for (int s = 0; s < RiggedCardsLogic.Cards; s++)
            {
                int c = RiggedCardsLogic.StartSlots[s];
                cards[c].localPosition = SlotPos(s);
                cardArts[c].sprite = introFaces[c];
            }
            dealerSay.Show("Keep your eye on the Queen!", 2f);
            yield return new WaitForSeconds(1.8f);
            for (int c = 0; c < cards.Length; c++) StartCoroutine(Flip(c, backSprite));
            yield return new WaitForSeconds(0.45f);
            var at = (int[])RiggedCardsLogic.StartSlots.Clone();
            foreach (int[] sw in RiggedCardsLogic.ShuffleSwaps)
            {
                int a = at[sw[0]], b = at[sw[1]];
                Sfx.Play(this, "whoosh", 0.35f);
                StartCoroutine(SlideCard(a, SlotPos(sw[1]), 0.5f, 0.35f));
                yield return SlideCard(b, SlotPos(sw[0]), 0.5f, -0.25f);
                at[sw[0]] = b;
                at[sw[1]] = a;
                yield return new WaitForSeconds(0.12f);
            }
            logic.FinishShuffle();
            for (int c = 0; c < cards.Length; c++) cards[c].localPosition = SlotPos(logic.SlotOf(c));
            dealerSay.Show("Find the Queen! Easy money.", 2f);
            busy = false;
        }

        // ---------- Ciclo ----------

        void Update()
        {
            dealerSay.Tick();
            toast.Tick();
            if (IsFinished) return;
            if (hatWindow) { HatGag(); return; }
            if (busy || logic.InputLocked) { touches.Clear(); inkDragging = false; return; }
            float dt = Time.deltaTime;
            ReadFingers();
            switch (logic.Tick(dt, Inputs.Tilt().x, held))
            {
                case CardsSlide.Started:
                    Sfx.Play(this, "whoosh", 0.3f);
                    cardArts[logic.FreeCard].sortingOrder = 30;
                    break;
                case CardsSlide.Cancelled:
                    for (int c = 0; c < cards.Length; c++)
                        if (!logic.IsGone(c)) { cardArts[c].sortingOrder = baseOrder[c]; StartCoroutine(SlideCard(c, SlotPos(logic.SlotOf(c)), 0.2f)); }
                    break;
                case CardsSlide.FellNothing: StartCoroutine(FellNothing(logic.FreeCard)); break;
                case CardsSlide.FellStain: StartCoroutine(FellStain(logic.FreeCard)); break;
            }
            if (logic.State == CardsState.Sliding && logic.FreeCard >= 0)
                cards[logic.FreeCard].localPosition = SlotPos(logic.SlotOf(logic.FreeCard)) + new Vector3(logic.Offset, 0f, 0f);
            Wobble();
            if (Inputs.Shake() > ShakeLevel && Time.time >= shakeReadyAt && logic.Shake())
            {
                shakeReadyAt = Time.time + 2f;
                StartCoroutine(SpillCoins());
            }
            if (ducked && Time.time >= nextSearchLine && logic.State == CardsState.StainExposed)
            {
                nextSearchLine = Time.time + 4.5f;
                dealerSay.Show(SearchLines[searchIdx++ % SearchLines.Length], 1.8f);
            }
        }

        /// <summary>
        /// Al massimo due dita. Un dito su una carta la tiene (due dita = due carte tenute); un tocco breve con un dito
        /// solo la sceglie; trascinarla con un dito solo fa intervenire il mazziere. La macchia, quando è scoperta, si
        /// trascina con un dito. Editor: i tasti 1-3 tengono la carta nella posizione 1-3 (seconda mano).
        /// </summary>
        void ReadFingers()
        {
            seen.Clear();
            foreach (S41KKit.Finger f in S41KKit.Fingers(cam))
            {
                if (f.ended)
                {
                    if (touches.TryGetValue(f.id, out Touch e)) { touches.Remove(f.id); EndTouch(e, f.world); }
                    continue;
                }
                seen.Add(f.id);
                if (!touches.TryGetValue(f.id, out Touch t))
                {
                    if (!f.began || touches.Count >= 2 || PointerInput.IsOverUi()) continue;
                    t = new Touch { Start = f.world, T0 = Time.time };
                    if (touches.Count > 0) { t.Multi = true; foreach (Touch o in touches.Values) o.Multi = true; }
                    if (logic.StainVisible && !inkDragging && stainArea.OverlapPoint(f.world))
                    {
                        t.Ink = true;
                        inkDragging = true;
                        inkOffset = (Vector2)stain.position - f.world;
                        stain.localScale = Vector3.one * 1.12f;
                        Haptics.Tap();
                    }
                    else
                    {
                        t.Card = CardUnder(f.world);
                        if (t.Card < 0) TapScenery(f.world);
                        else Haptics.Tap();
                    }
                    touches[f.id] = t;
                    continue;
                }
                if (t.Ink)
                {
                    Vector2 p = HalloweenKit.ClampSafe(f.world + inkOffset);
                    stain.position = new Vector3(p.x, p.y, stain.position.z);
                }
                else if (t.Card >= 0 && !t.Multi && !t.Moved && touches.Count == 1 && Vector2.Distance(f.world, t.Start) > DragStart)
                {
                    t.Moved = true;
                    Rearrange();
                }
            }
            // dita sparite senza "fine" (tocco annullato): si lasciano senza effetti
            stale.Clear();
            foreach (int id in touches.Keys) if (!seen.Contains(id)) stale.Add(id);
            foreach (int id in stale)
            {
                if (touches[id].Ink) DropInk();
                touches.Remove(id);
            }
            for (int c = 0; c < held.Length; c++) held[c] = false;
            foreach (Touch t in touches.Values)
                if (t.Card >= 0 && !t.Ink && !t.Moved && !logic.IsGone(t.Card)) held[t.Card] = true;
#if UNITY_EDITOR
            for (int s = 0; s < 3; s++) if (Input.GetKey(KeyCode.Alpha1 + s)) held[logic.CardAt(s)] = true;
#endif
            for (int c = 0; c < cards.Length; c++)
                if (!flipping[c]) cardArts[c].color = held[c] ? new Color(0.86f, 0.86f, 0.9f) : Color.white;
        }

        void EndTouch(Touch t, Vector2 w)
        {
            if (t.Ink) { DropInk(); return; }
            if (t.Card < 0 || t.Multi || t.Moved || Time.time - t.T0 > TapTime) return;
            PickCard(t.Card);
        }

        int CardUnder(Vector2 w)
        {
            int best = -1;
            for (int c = 0; c < cards.Length; c++)
            {
                if (logic.IsGone(c) || cardAreas[c] == null || !cardAreas[c].OverlapPoint(w)) continue;
                if (best < 0 || cardArts[c].sortingOrder > cardArts[best].sortingOrder) best = c;
            }
            return best;
        }

        void TapScenery(Vector2 w)
        {
            if (!logic.StainVisible && stainArea.OverlapPoint(w) && Time.time >= peekReadyAt)
            {
                peekReadyAt = Time.time + 2f;
                toast.Show("A tiny ink smudge... peeking out from under that card?", 2.2f);
                return;
            }
            if (coinsArea.OverlapPoint(w)) { dealerSay.Show(ducked ? "I HEARD THAT. Hands off my coins!" : "Those are MY coins.", 1.6f); Sfx.Play(this, "tick", 0.5f); return; }
            if (dealerArea.OverlapPoint(w) && !ducked)
            {
                dealerSay.Show("Pick a card, any card! They're all great.", 1.8f);
                StartCoroutine(HalloweenKit.Pop(dealer, 1.04f, 0.2f));
            }
        }

        // ---------- Scegliere, riordinare ----------

        void PickCard(int c)
        {
            if (flipping[c]) return;
            CardPick r = logic.Pick(c);
            if (r == CardPick.Queen) { StartCoroutine(Victory(c)); return; }
            if (r == CardPick.Ignored) return;
            StartCoroutine(Reveal(c, r == CardPick.TwoOfSpadesRemembered));
        }

        IEnumerator Reveal(int c, bool remembered)
        {
            flipping[c] = true;
            yield return Flip(c, twoSprite);
            if (logic.DealerDistracted) toast.Show("Still a two. Even with nobody watching.", 1.8f);
            else
            {
                Sfx.Play(this, "mock_laugh", 0.6f);
                string[] lines = remembered ? RememberedLines : OtherLines;
                dealerSay.Show(logic.Guesses >= 6 && guessIdx % 2 == 1 ? "Keep paying, keep guessing!" : lines[guessIdx % lines.Length], 1.8f);
                guessIdx++;
                StartCoroutine(HalloweenKit.Wiggle(dealer, 3f, 0.5f, 25f));
            }
            yield return new WaitForSeconds(0.9f);
            yield return Flip(c, logic.QueenCard == c ? queenInkSprite : backSprite);
            flipping[c] = false;
        }

        void Rearrange()
        {
            int[] sw = logic.TryRearrange();
            if (sw == null) return;
            Sfx.Play(this, "whoosh", 0.6f);
            Haptics.Light();
            dealerSay.Show(RearrangeLines[rearrangeIdx++ % RearrangeLines.Length], 1.6f);
            for (int c = 0; c < cards.Length; c++) StartCoroutine(SlideCard(c, SlotPos(logic.SlotOf(c)), 0.18f, c == logic.CardAt(sw[0]) ? 0.2f : 0f));
        }

        // ---------- Scivolamento e caduta ----------

        IEnumerator FellNothing(int c)
        {
            yield return FallOff(c);
            dealerSay.Show("Butterfingers! Nothing under there, pal.", 2f);
            Sfx.Play(this, "mock_laugh", 0.5f);
            yield return new WaitForSeconds(1.1f);
            cards[c].localPosition = SlotPos(logic.SlotOf(c));
            cards[c].localRotation = Quaternion.identity;
            cardArts[c].sortingOrder = baseOrder[c];
            cardArts[c].sprite = backSprite;
            HalloweenKit.Show(cards[c], true);
            logic.ReturnFallenCard();
            Sfx.Play(this, "pop", 0.5f);
            yield return HalloweenKit.Pop(cards[c], 1.15f, 0.2f);
        }

        IEnumerator FellStain(int c)
        {
            dealerSay.Show("Whoa! My card!", 1.4f);
            StartCoroutine(FallOff(c));
            yield return new WaitForSeconds(0.35f);
            // il mazziere si china sotto il tavolo a cercarla (resta distratto finché non c'è la Regina)
            ducked = true;
            nextSearchLine = Time.time + 2.5f;
            yield return HalloweenKit.Move(dealer, dealer.position + new Vector3(0f, -1.75f, 0f), 0.35f);
            toast.Show("Hm? Something's drawn on the cloth...", 2f);
        }

        IEnumerator FallOff(int c)
        {
            Sfx.Play(this, "whoosh", 0.5f);
            Haptics.Light();
            Transform t = cards[c];
            float dir = Mathf.Sign(logic.Offset == 0f ? 1f : logic.Offset);
            Vector3 v = new Vector3(dir * 3f, 0.6f, 0f);
            for (float k = 0f; k < 0.9f; k += Time.deltaTime)
            {
                v.y -= 14f * Time.deltaTime;
                t.localPosition += v * Time.deltaTime;
                t.Rotate(0f, 0f, -dir * 360f * Time.deltaTime);
                yield return null;
            }
            Sfx.Play(this, "bonk", 0.4f);
            HalloweenKit.Show(t, false);
        }

        // ---------- Inchiostro ----------

        void DropInk()
        {
            inkDragging = false;
            stain.localScale = Vector3.one;
            int best = -1;
            float bestD = InkReach;
            for (int c = 0; c < cards.Length; c++)
            {
                if (logic.IsGone(c)) continue;
                float d = Vector2.Distance(stain.position, cards[c].position);
                if (d < bestD) { bestD = d; best = c; }
            }
            if (best >= 0 && logic.TransferInk(best)) { StartCoroutine(MakeQueen(best)); return; }
            StartCoroutine(SlideLocal(stain, stainHome, 0.25f));
            Sfx.Play(this, "tick", 0.4f);
        }

        IEnumerator MakeQueen(int c)
        {
            Transform t = cards[c];
            yield return HalloweenKit.Move(stain, t.position, 0.12f);
            HalloweenKit.Show(stain, false);
            Sfx.Play(this, "pop", 0.8f);
            Haptics.Medium();
            yield return Flip(c, queenInkSprite);
            yield return HalloweenKit.Pop(t, 1.2f, 0.3f);
            toast.Show("The ink soaked in. That's a Queen now. Obviously.", 2.2f);
        }

        // ---------- Monete ----------

        IEnumerator SpillCoins()
        {
            Sfx.Play(this, "tick", 0.8f);
            Haptics.Light();
            for (int i = 0; i < coins.Length; i++)
                StartCoroutine(SlideLocal(coins[i], coinHome[i] + new Vector3(0.45f + 0.55f * i, -0.35f * (i % 2) - 0.15f, 0f), 0.25f));
            yield return new WaitForSeconds(0.5f);
            dealerSay.Show(ducked ? "Hey! Who's spilling my coins?!" : "Mine. Mine. Also mine.", 1.6f);
            yield return new WaitForSeconds(0.4f);
            for (int i = 0; i < coins.Length; i++)
            {
                StartCoroutine(SlideLocal(coins[i], coinHome[i], 0.2f));
                Sfx.Play(this, "tick", 0.4f);
                yield return new WaitForSeconds(0.12f);
            }
        }

        /// <summary>Carte inclinate senza tenerne due: tremano un po' verso il basso del tavolo (vorrebbero scivolare).</summary>
        void Wobble()
        {
            bool shiver = logic.State == CardsState.Playing && Mathf.Abs(logic.FilteredTilt) > 0.22f;
            for (int c = 0; c < cards.Length; c++)
            {
                if (logic.IsGone(c) || flipping[c]) continue;
                float z = shiver && !held[c] ? -logic.FilteredTilt * 5f + Mathf.Sin(Time.time * 40f + c) * 1.2f : 0f;
                cards[c].localRotation = Quaternion.Euler(0f, 0f, z);
            }
        }

        // ---------- Vittoria ----------

        IEnumerator Victory(int c)
        {
            busy = true;
            Transform q = cards[c];
            Sfx.Play(this, "ding", 0.7f);
            yield return HalloweenKit.Pop(q, 1.3f, 0.35f);
            if (ducked)
            {
                ducked = false;
                yield return HalloweenKit.Move(dealer, dealer.parent.TransformPoint(dealerHome), 0.3f);
            }
            dealerSay.Show("...a Queen?", 0.9f);
            yield return new WaitForSeconds(0.6f);
            HalloweenKit.Show(miniDeck, true);
            yield return HalloweenKit.Wiggle(miniDeck, 20f, 0.45f, 30f);
            HalloweenKit.Show(miniDeck, false);
            dealerSay.Show("HEY! THAT'S CHEATING!", 1.6f);
            Sfx.Play(this, "buzz", 0.8f);
            Haptics.Medium();
            yield return HalloweenKit.Shake(dealer, 0.08f, 0.5f);
            // prende le monete e scappa; il cilindro gli vola via e resta sul tavolo
            foreach (Transform coin in coins) StartCoroutine(HalloweenKit.Move(coin, dealer.position + new Vector3(0.4f, 0.1f, 0f), 0.2f));
            yield return new WaitForSeconds(0.2f);
            foreach (Transform coin in coins) coin.SetParent(dealer, true);
            Vector3 hatAt = dealerHat.position;
            HalloweenKit.Show(dealerHat, false);
            droppedHat.position = new Vector3(hatAt.x, hatAt.y, droppedHat.position.z);
            Vector3 hatRest = droppedHat.parent.TransformPoint(hatRestLocal);
            HalloweenKit.Show(droppedHat, true);
            Sfx.Play(this, "whoosh", 0.8f);
            StartCoroutine(HalloweenKit.Move(droppedHat, hatRest, 0.45f, 0.6f));
            yield return HalloweenKit.Move(dealer, dealer.position + new Vector3(4.5f, 0f, 0f), 0.5f, 0.15f);
            toast.Show("CONGRATULATIONS. YOU OUT-CHEATED A CHEATER.", 2.6f);
            StartCoroutine(Celebrate(1.6f));
            // gag: per un attimo si può toccare il cilindro abbandonato
            hatWindow = true;
            for (float k = 0f; k < 2.2f || (jokerShown && k < 3.4f); k += Time.deltaTime) yield return null;
            hatWindow = false;
            if (logic.TryComplete()) Win();
        }

        void HatGag()
        {
            if (jokerShown || !HalloweenKit.ReadPointer(cam, out Vector2 w, out bool began, out _) || !began) return;
            if (!hatArea.OverlapPoint(w)) return;
            jokerShown = true;
            StartCoroutine(RevealJoker());
        }

        IEnumerator RevealJoker()
        {
            joker.localPosition = jokerLocal;
            HalloweenKit.Show(joker, true);
            Sfx.Play(this, "pop", 0.8f);
            yield return SlideLocal(joker, jokerLocal + new Vector3(0f, 0.75f, 0f), 0.25f);
            Sfx.Play(this, "mock_laugh", 0.45f);   // il mazziere ride da fuori scena
            yield return HalloweenKit.Wiggle(joker, 8f, 0.6f, 20f);
        }

        // ---------- Animazioni ----------

        IEnumerator Flip(int c, Sprite to)
        {
            Transform t = cards[c];
            Vector3 s = t.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.09f) { t.localScale = new Vector3(s.x * (1f - k), s.y, s.z); yield return null; }
            cardArts[c].sprite = to;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.09f) { t.localScale = new Vector3(s.x * k, s.y, s.z); yield return null; }
            t.localScale = s;
        }

        IEnumerator SlideCard(int c, Vector3 to, float seconds, float hop = 0f)
        {
            Transform t = cards[c];
            Vector3 from = t.localPosition;
            for (float k = 0f; k < 1f; k += Time.deltaTime / Mathf.Max(0.01f, seconds))
            {
                float e = k * k * (3f - 2f * k);
                t.localPosition = Vector3.Lerp(from, to, e) + Vector3.up * Mathf.Sin(k * Mathf.PI) * hop;
                yield return null;
            }
            t.localPosition = to;
        }

        static IEnumerator SlideLocal(Transform t, Vector3 to, float seconds)
        {
            if (t == null) yield break;
            Vector3 from = t.localPosition;
            for (float k = 0f; k < 1f; k += Time.deltaTime / Mathf.Max(0.01f, seconds))
            {
                t.localPosition = Vector3.Lerp(from, to, 1f - (1f - k) * (1f - k));
                yield return null;
            }
            t.localPosition = to;
        }
    }
}
