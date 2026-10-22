using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels.Halloween;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 249 "The vegetarian zombie" — lo zombie affamato dice "BRAAAINS!" ma rifiuta cervello e hamburger; il
    /// broccolo lo mangia ma non basta (i broccoli ricrescono, non finiscono mai). Soluzione: toccare le lettere del
    /// fumetto per toglierle (c'è il tasto annulla) finché resta esattamente "BRAIN": il fumetto brilla e diventa
    /// trascinabile; trascinato su un broccolo, lo zombie lo legge come un cervello e lo divora felice.
    /// </summary>
    public class HwZombieLevel : LevelController
    {
        [SerializeField] Transform zombie;
        [SerializeField] SpriteRenderer zombieHead;
        [SerializeField] Sprite headNormal, headHappy;
        [SerializeField] Collider2D zombieArea;
        [SerializeField] Transform mouth;
        [SerializeField] Transform balloon;
        [SerializeField] SpriteRenderer balloonFrame;
        [SerializeField] BoxCollider2D balloonArea;
        [SerializeField] SpriteRenderer balloonGlow;
        [SerializeField] Transform[] tiles;
        [SerializeField] Collider2D[] tileAreas;
        [SerializeField] Transform undoButton;
        [SerializeField] Collider2D undoArea;
        [SerializeField] float tileStep = 0.66f;
        [SerializeField] float frameHeight = 1.1f;
        [SerializeField] Transform brain, burger;
        [SerializeField] Collider2D brainArea, burgerArea;
        [SerializeField] Transform[] broccoli;
        [SerializeField] Collider2D[] broccoliAreas;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] SmokeEmitter hearts;
        [SerializeField] SmokeEmitter burp;
        [SerializeField] SmokeEmitter crumbs;

        readonly ZombieLogic logic = new ZombieLogic();
        Camera cam;
        HwDragger drag;
        HwDragger.Item balloonItem, brainItem, burgerItem;
        readonly List<HwDragger.Item> broccoliItems = new List<HwDragger.Item>();
        HwSay say;
        Vector3[] tileScale;
        bool[] gone;
        bool busy, wasReady;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            logic.Reset();
            drag = new HwDragger(this);
            say = new HwSay(bubble);
            balloonItem = drag.Add(balloon, balloonArea, 10);
            balloonItem.Lift = 0.9f;
            brainItem = drag.Add(brain, brainArea, 5);
            burgerItem = drag.Add(burger, burgerArea, 5);
            for (int i = 0; i < broccoli.Length; i++) broccoliItems.Add(drag.Add(broccoli[i], broccoliAreas[i], 5));
            tileScale = new Vector3[tiles.Length];
            gone = new bool[tiles.Length];
            for (int i = 0; i < tiles.Length; i++) { tileScale[i] = tiles[i].localScale; tiles[i].gameObject.SetActive(true); }
            zombieHead.sprite = headNormal;
            HalloweenKit.SetAlpha(balloonGlow, 0f);
            if (hearts != null) hearts.Stop();
            if (burp != null) burp.Stop();
            if (crumbs != null) crumbs.Stop();
            LayoutTiles(true);
        }

        /// <summary>Le lettere rimaste si ricompattano al centro; il fumetto si stringe con loro.</summary>
        void LayoutTiles(bool instant)
        {
            List<int> visible = logic.Word.VisibleIndices();
            float half = (visible.Count - 1) * 0.5f;
            for (int k = 0; k < visible.Count; k++)
            {
                Transform t = tiles[visible[k]];
                Vector3 target = new Vector3((k - half) * tileStep, 0f, t.localPosition.z);
                t.localPosition = instant ? target : Vector3.Lerp(t.localPosition, target, 14f * Time.deltaTime);
            }
            float w = visible.Count * tileStep + 0.45f;
            Vector2 size = balloonFrame.size;
            size.x = instant ? w : Mathf.Lerp(size.x, w, 14f * Time.deltaTime);
            size.y = frameHeight;
            balloonFrame.size = size;
            balloonArea.size = new Vector2(size.x, size.y);
            if (balloonGlow != null) balloonGlow.transform.localScale = new Vector3(size.x + 0.5f, size.y + 0.5f, 1f);
            undoButton.localPosition = new Vector3(size.x * 0.5f - 0.05f, size.y * 0.5f + 0.05f, undoButton.localPosition.z);
        }

        void Update()
        {
            say.Tick();
            if (!logic.InputLocked) LayoutTiles(false);
            bool ready = logic.BalloonReady || logic.State == ZombieState.HoldingBalloon;
            if (ready && !wasReady) StartCoroutine(BrainReady());
            if (!ready && wasReady) HalloweenKit.SetAlpha(balloonGlow, 0f);
            wasReady = ready;
            if (ready && logic.State != ZombieState.Devouring) HalloweenKit.SetAlpha(balloonGlow, 0.45f + 0.25f * Mathf.Sin(Time.time * 6f));
            balloonItem.Enabled = logic.BalloonReady;

            if (IsFinished || busy || logic.InputLocked) return;
            if (!HalloweenKit.ReadPointer(cam, out Vector2 w, out bool began, out bool ended))
            {
                if (drag.Held != null) Drop(drag.Release(), balloon.position);
                return;
            }
            if (began)
            {
                // prima il tasto annulla, poi il fumetto (solo se dice BRAIN), le lettere e il cibo
                if (undoArea.OverlapPoint(w)) { Undo(); return; }
                HwDragger.Item it = drag.TryPick(w);
                if (it == balloonItem) logic.TryPickBalloon();
                else if (it == null) TapLetter(w);
                else if (logic.State != ZombieState.Hungry) drag.Cancel();
            }
            if (drag.Held == null) return;
            drag.MoveHeld(w);
            if (ended) Drop(drag.Release(), w);
        }

        IEnumerator BrainReady()
        {
            Sfx.Play(this, "ding", 0.7f);
            Haptics.Light();
            yield return HalloweenKit.Wiggle(balloon, 5f, 0.5f, 25f);
        }

        void TapLetter(Vector2 w)
        {
            for (int i = 0; i < tiles.Length; i++)
            {
                if (gone[i] || !tileAreas[i].OverlapPoint(w)) continue;
                if (!logic.TryTapLetter(i)) { if (logic.Word.IsTarget) StartCoroutine(HalloweenKit.Pop(tiles[i], 1.1f, 0.15f)); return; }
                gone[i] = true;
                StartCoroutine(PopOut(i));
                Sfx.Play(this, "pop", 0.6f);
                Haptics.Tap();
                return;
            }
        }

        IEnumerator PopOut(int i)
        {
            Transform t = tiles[i];
            Vector3 from = t.localPosition;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.3f)
            {
                if (!gone[i]) { t.localScale = tileScale[i]; yield break; }   // annullato a metà volo
                t.localPosition = from + new Vector3(0f, k * 0.9f, 0f);
                t.localScale = tileScale[i] * (1f - k);
                yield return null;
            }
            if (!gone[i]) { t.localScale = tileScale[i]; yield break; }
            t.gameObject.SetActive(false);
            t.localScale = tileScale[i];
        }

        void Undo()
        {
            int i = logic.TryUndo();
            StartCoroutine(HalloweenKit.Pop(undoButton, 1.2f, 0.2f));
            if (i < 0) { Sfx.Play(this, "tick", 0.4f); return; }
            gone[i] = false;
            tiles[i].gameObject.SetActive(true);
            tiles[i].localScale = tileScale[i];
            StartCoroutine(HalloweenKit.Pop(tiles[i], 1.25f, 0.2f));
            Sfx.Play(this, "whoosh", 0.4f);
            Haptics.Tap();
        }

        void Drop(HwDragger.Item it, Vector2 finger)
        {
            if (it == null) return;
            if (it == balloonItem)
            {
                int b = BroccoliUnder(finger);
                if (logic.TryDropBalloon(b >= 0)) { StartCoroutine(Devour(b)); return; }
                drag.SendHome(it, 0.35f);
                return;
            }
            bool fed = zombieArea.OverlapPoint(it.T.position) || Vector2.Distance(it.T.position, mouth.position) < 1.0f;
            if (!fed) { drag.SendHome(it); return; }
            ZombieFood food = it == brainItem ? ZombieFood.Brain : it == burgerItem ? ZombieFood.Burger : ZombieFood.Broccoli;
            ZombieFeedResult r = logic.TryFeed(food);
            if (r == ZombieFeedResult.Ignored) { drag.SendHome(it); return; }
            StartCoroutine(Feed(food, r, it));
        }

        /// <summary>Broccolo (visibile) sotto il fumetto o sotto il dito; -1 se nessuno.</summary>
        int BroccoliUnder(Vector2 finger)
        {
            Bounds fb = balloonFrame.bounds;
            fb.Expand(new Vector3(0.3f, 0.3f, 10f));
            int best = -1;
            float bestD = float.MaxValue;
            for (int i = 0; i < broccoli.Length; i++)
            {
                if (!broccoli[i].gameObject.activeSelf) continue;
                Vector3 p = broccoli[i].position;
                float d = Vector2.Distance(finger, p);
                if ((d < 0.9f || fb.Contains(new Vector3(p.x, p.y, fb.center.z))) && d < bestD) { bestD = d; best = i; }
            }
            return best;
        }

        IEnumerator Feed(ZombieFood food, ZombieFeedResult r, HwDragger.Item it)
        {
            busy = true;
            if (r == ZombieFeedResult.Refused)
            {
                Sfx.Play(this, "buzz", 0.5f);
                Haptics.Light();
                say.Show(food == ZombieFood.Brain ? "EWW! Brains? I'm VEGETARIAN!" : "MEAT?! Yuck! No way!", 2f);
                drag.SendHome(it, 0.4f);
                yield return HalloweenKit.Wiggle(zombie, 6f, 0.8f, 22f);
            }
            else
            {
                it.Enabled = false;
                StartCoroutine(HalloweenKit.ScaleTo(it.T, it.T.localScale * 0.3f, 0.3f));
                yield return HalloweenKit.Move(it.T, mouth.position, 0.3f);
                it.T.gameObject.SetActive(false);
                if (crumbs != null) StartCoroutine(HalloweenKit.Burst(crumbs, 0.4f));
                for (int k = 0; k < 3; k++) { Sfx.Play(this, "bonk", 0.5f); Haptics.Tap(); yield return new WaitForSeconds(0.18f); }
                say.Show("Munch... meh. Still hungry. BRAAAINS!", 2.2f);
                yield return new WaitForSeconds(0.6f);
                it.T.localScale = it.BaseScale;
                RegrowIfNeeded(it);
            }
            logic.TryEndReaction();
            busy = false;
        }

        /// <summary>Ci sono sempre tanti broccoli quanti ne conta la logica: se serve, quello mangiato ricresce.</summary>
        void RegrowIfNeeded(HwDragger.Item eaten)
        {
            int visible = 0;
            foreach (Transform b in broccoli) if (b.gameObject.activeSelf) visible++;
            if (visible >= logic.Broccoli) return;
            eaten.T.position = eaten.Home;
            eaten.T.gameObject.SetActive(true);
            eaten.Enabled = true;
            StartCoroutine(HalloweenKit.Pop(eaten.T, 1.3f, 0.3f));
            Sfx.Play(this, "pop", 0.5f);
        }

        IEnumerator Devour(int b)
        {
            busy = true;
            Transform target = broccoli[b];
            // il fumetto si appiccica al broccolo come un'etichetta: "BRAIN"
            StartCoroutine(HalloweenKit.ScaleTo(balloon, balloon.localScale * 0.45f, 0.3f));
            yield return HalloweenKit.Move(balloon, target.position + new Vector3(0f, 0.55f, 0f), 0.3f);
            Sfx.Play(this, "pop", 0.7f);
            Haptics.Light();
            HalloweenKit.SetAlpha(balloonGlow, 0.8f);
            say.Show("...BRAIN? A BRAIN!!! *drool*", 1.8f);
            yield return HalloweenKit.Shake(zombie, 0.06f, 0.6f);
            yield return new WaitForSeconds(0.6f);
            // lo zombie si avventa
            Vector3 home = zombie.position;
            yield return HalloweenKit.Move(zombie, new Vector3(target.position.x - 1.2f, home.y, home.z), 0.4f, 0.4f);
            StartCoroutine(HalloweenKit.ScaleTo(balloon, Vector3.zero, 0.4f));
            yield return HalloweenKit.ScaleTo(target, Vector3.zero, 0.4f);
            if (crumbs != null) StartCoroutine(HalloweenKit.Burst(crumbs, 0.6f));
            for (int k = 0; k < 4; k++) { Sfx.Play(this, "bonk", 0.6f); Haptics.Medium(); yield return new WaitForSeconds(0.15f); }
            zombieHead.sprite = headHappy;
            if (hearts != null) hearts.Play();
            yield return new WaitForSeconds(0.6f);
            if (burp != null) StartCoroutine(HalloweenKit.Burst(burp, 0.5f));
            Sfx.Play(this, "buzz", 0.7f);
            Haptics.Heavy();
            say.Show("BUUURP! Best brain EVER!", 2.2f);
            yield return HalloweenKit.Pop(zombie, 1.1f, 0.3f);
            Haptics.Success();
            Sfx.Play(this, "ding", 0.8f);
            yield return new WaitForSeconds(1.6f);
            if (hearts != null) hearts.Stop();
            if (logic.TryCompleteEnding()) Win();
        }
    }
}
