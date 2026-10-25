using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels.V63;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 258 "Shadow puppet" — un cane da guardia sta davanti alla porta, e i cani hanno paura dei gatti GIGANTI.
    /// A sinistra una lampada (spenta) e un supporto con quattro zone tratteggiate (testa, orecchie, coda) che scorre sul
    /// pavimento: più è vicino alla lampada, più grande sarà l'ombra sul muro. Sul ripiano: gomitolo, due fette di
    /// pizza, piuma, mela, stivale, zampa. Si compone la sagoma, poi si accende la lampada: il cane vede l'ombra e reagisce.
    /// Soluzione: gomitolo come testa, le due fette come orecchie, la piuma come coda, supporto vicino alla lampada.
    /// Ogni ombra sbagliata avvicina il cane di un passo; alla terza si perde ("Woof. That means 'no'.").
    /// </summary>
    public class V63ShadowPuppetLevel : V62Level
    {
        [SerializeField] Transform[] items;          // Yarn, PizzaA, PizzaB, Feather, Apple, Boot, Paw (ordine di PuppetItem)
        [SerializeField] Collider2D[] itemAreas;
        [SerializeField] SpriteRenderer[] itemArts;
        [SerializeField] Transform stand;            // il supporto (origine alla base): scorre sul pavimento
        [SerializeField] Collider2D standArea;
        [SerializeField] float[] notchX;             // Far, Mid, Near
        [SerializeField] Transform[] zones;          // Head, EarLeft, EarRight, Tail (figli del supporto)
        [SerializeField] SpriteRenderer[] zoneRings;
        [SerializeField] Transform shadowRoot;       // dove cade l'ombra della testa, sul muro
        [SerializeField] Transform[] shadowZones;
        [SerializeField] SpriteRenderer[] shadowArts;
        [SerializeField] float[] shadowScale = { 1f, 1.45f, 2.2f };
        [SerializeField] SpriteRenderer lightCone;
        [SerializeField] SpriteRenderer wallGlow;
        [SerializeField] Transform lamp;
        [SerializeField] Collider2D lampArea;
        [SerializeField] Transform dog;
        [SerializeField] Collider2D dogArea;
        [SerializeField] Transform doorPanel;
        [SerializeField] Collider2D doorArea;
        [SerializeField] SpeechBubble dogBubble;
        [SerializeField] TextMesh infoText;
        [SerializeField] Transform infoHolder;

        const float ZoneReach = 0.42f;
        static readonly Color ShadowColor = new Color(0.06f, 0.04f, 0.1f, 0.88f);

        readonly ShadowPuppetLogic logic = new ShadowPuppetLogic();
        Camera cam;
        HwDragger drag;
        HwDragger.Item[] dragItems;
        HwDragger.Item standItem;
        HwSay dogSay;
        HwToast toast;
        Vector3 dogHome, dogScale, doorScale;
        float standY, tapReadyAt;
        bool busy, distanceHinted;

        static int Index(PuppetItem it) => (int)it - 1;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            logic.Reset();
            ResetV62();
            dogSay = new HwSay(dogBubble);
            toast = new HwToast(infoText, infoHolder);
            drag = new HwDragger(this);
            dragItems = new HwDragger.Item[items.Length];
            for (int i = 0; i < items.Length; i++) dragItems[i] = drag.Add(items[i], itemAreas[i], 6);
            standItem = drag.Add(stand, standArea, 2);
            standY = stand.position.y;
            stand.position = new Vector3(notchX[(int)PuppetDistance.Far], standY, stand.position.z);
            standItem.Home = stand.position;
            dogHome = dog.position;
            dogScale = dog.localScale;
            doorScale = doorPanel.localScale;
            HalloweenKit.SetAlpha(lightCone, 0f);
            HalloweenKit.SetAlpha(wallGlow, 0f);
            HideShadows();
            UpdateRings();
            dogSay.Show("Grrr. Nobody gets past me.", 2.2f);
        }

        void Update()
        {
            dogSay.Tick();
            toast.Tick();
            if (IsFinished || busy || logic.InputLocked) return;
            if (!HalloweenKit.ReadPointer(cam, out Vector2 w, out bool began, out bool ended))
            {
                if (drag.Held != null) Drop(drag.Held, true);
                return;
            }
            if (began)
            {
                HwDragger.Item it = drag.TryPick(w);
                if (it != null) OnPick(it);
                else Tap(w);
            }
            if (drag.Held == null) return;
            drag.MoveHeld(w);
            if (drag.Held == standItem)
            {
                // il supporto scorre solo sul pavimento, tra la lampada e il cane
                float x = Mathf.Clamp(stand.position.x, Mathf.Min(notchX[0], notchX[2]), Mathf.Max(notchX[0], notchX[2]));
                stand.position = new Vector3(x, standY, stand.position.z);
            }
            if (ended) Drop(drag.Held, false);
        }

        PuppetItem ItemOf(HwDragger.Item it)
        {
            for (int i = 0; i < dragItems.Length; i++) if (dragItems[i] == it) return (PuppetItem)(i + 1);
            return PuppetItem.None;
        }

        void OnPick(HwDragger.Item it)
        {
            if (it == standItem) return;
            PuppetItem item = ItemOf(it);
            if (logic.TryRemove(item)) UpdateRings();
            it.T.SetParent(transform, true);
            it.T.localRotation = Quaternion.identity;
        }

        void Tap(Vector2 w)
        {
            if (lampArea.OverlapPoint(w)) { StartCoroutine(Show()); return; }
            if (dogArea.OverlapPoint(w) && Time.time >= tapReadyAt)
            {
                tapReadyAt = Time.time + 0.4f;
                Sfx.Play(this, "woof", 0.6f);
                StartCoroutine(HalloweenKit.Shake(dog, 0.03f, 0.25f));
                dogSay.Show("WOOF. Not scared of anything. Except... no. Nothing.", 2f);
                return;
            }
            if (doorArea.OverlapPoint(w))
            {
                Sfx.Play(this, "clunk", 0.4f);
                toast.Show("The dog is in the way.", 1.6f);
            }
        }

        void Drop(HwDragger.Item it, bool lostFinger)
        {
            drag.Release();
            if (it == standItem)
            {
                int best = 0;
                for (int i = 1; i < notchX.Length; i++)
                    if (Mathf.Abs(stand.position.x - notchX[i]) < Mathf.Abs(stand.position.x - notchX[best])) best = i;
                logic.TrySetDistance((PuppetDistance)best);
                standItem.Home = new Vector3(notchX[best], standY, stand.position.z);
                drag.SendHome(standItem, 0.2f);
                Sfx.Play(this, "tick", 0.5f);
                if (!distanceHinted && best != (int)PuppetDistance.Far) { distanceHinted = true; toast.Show("Closer to the lamp = bigger shadow.", 2f); }
                return;
            }
            PuppetItem item = ItemOf(it);
            int zone = lostFinger ? -1 : ZoneNear(it.T.position);
            if (zone >= 0 && logic.TryPlace(item, (PuppetZone)zone, out PuppetItem displaced))
            {
                if (displaced != PuppetItem.None) SendToTray(displaced);
                it.T.SetParent(zones[zone], true);
                it.T.localRotation = Quaternion.Euler(0f, 0f, ZoneAngle(item, (PuppetZone)zone));
                drag.SendTo(it, zones[zone].position, 0.15f, 0f);
                Sfx.Play(this, "pop", 0.5f);
                UpdateRings();
                return;
            }
            SendToTray(item);
        }

        int ZoneNear(Vector3 p)
        {
            int best = -1;
            float bd = ZoneReach;
            for (int i = 0; i < zones.Length; i++)
            {
                float d = Vector2.Distance(p, zones[i].position);
                if (d < bd) { bd = d; best = i; }
            }
            return best;
        }

        void SendToTray(PuppetItem item)
        {
            HwDragger.Item d = dragItems[Index(item)];
            d.T.SetParent(transform, true);
            d.T.localRotation = Quaternion.Euler(0f, 0f, TrayAngle(item));
            drag.SendHome(d, 0.3f);
        }

        /// <summary>Come si mette un oggetto in una zona: le fette di pizza a punta in su, la piuma dritta o di lato.</summary>
        static float ZoneAngle(PuppetItem item, PuppetZone zone)
        {
            bool ear = zone == PuppetZone.EarLeft || zone == PuppetZone.EarRight;
            switch (item)
            {
                case PuppetItem.PizzaA:
                case PuppetItem.PizzaB:
                    return zone == PuppetZone.EarLeft ? -128f : zone == PuppetZone.EarRight ? -142f : -135f;
                case PuppetItem.Feather: return ear ? 70f : zone == PuppetZone.Tail ? 50f : 0f;
                case PuppetItem.Paw: return zone == PuppetZone.Tail ? -60f : 0f;
                default: return 0f;
            }
        }

        /// <summary>Le zone vuote si vedono bene, quelle occupate quasi spariscono.</summary>
        void UpdateRings()
        {
            for (int z = 0; z < zoneRings.Length; z++)
                if (zoneRings[z] != null)
                    zoneRings[z].color = new Color(0.35f, 0.24f, 0.18f, logic.ZoneItem((PuppetZone)z) == PuppetItem.None ? 0.7f : 0.2f);
        }

        static float TrayAngle(PuppetItem item) => item == PuppetItem.Paw ? 90f : 0f;

        // ---------- l'ombra ----------

        void HideShadows()
        {
            foreach (SpriteRenderer s in shadowArts) if (s != null) s.enabled = false;
        }

        /// <summary>Le ombre sul muro: stesso disegno degli oggetti, nero, ingrandito secondo la distanza.</summary>
        void CastShadows()
        {
            float k = shadowScale[(int)logic.Distance];
            Vector3 head = zones[0].localPosition;
            for (int z = 0; z < zones.Length; z++)
            {
                PuppetItem item = logic.ZoneItem((PuppetZone)z);
                SpriteRenderer sh = shadowArts[z];
                if (item == PuppetItem.None) { sh.enabled = false; continue; }
                Transform src = items[Index(item)];
                SpriteRenderer art = itemArts[Index(item)];
                Transform zt = shadowZones[z];
                zt.localPosition = (zones[z].localPosition - head) * k;
                zt.localRotation = src.localRotation;
                zt.localScale = src.localScale * k;
                sh.sprite = art.sprite;
                sh.transform.localPosition = art.transform.localPosition;
                sh.transform.localRotation = art.transform.localRotation;
                sh.color = ShadowColor;
                sh.enabled = true;
            }
        }

        IEnumerator Show()
        {
            Silhouette s = logic.TryShow();
            if (logic.State == PuppetState.Composing) yield break;
            busy = true;
            drag.Cancel();
            Sfx.Play(this, "tick", 0.8f);
            StartCoroutine(HalloweenKit.Pop(lamp, 1.06f, 0.15f));
            CastShadows();
            StartCoroutine(HalloweenKit.Fade(lightCone, 0.55f, 0.2f));
            StartCoroutine(HalloweenKit.Fade(wallGlow, 0.5f, 0.2f));
            if (shadowRoot != null) StartCoroutine(HalloweenKit.Pop(shadowRoot, 1.06f, 0.25f));
            yield return new WaitForSeconds(0.9f);
            dogSay.Show(ShadowPuppetLogic.Reaction(s), 2.4f);
            if (s == Silhouette.BigCat) { yield return Flee(); yield break; }
            if (s == Silhouette.Empty)
            {
                Sfx.Play(this, "snore", 0.4f);
                yield return new WaitForSeconds(1.4f);
            }
            else
            {
                Haptics.Light();
                Sfx.Play(this, s == Silhouette.Kitten ? "mock_laugh" : "woof", 0.7f);
                if (s == Silhouette.Kitten)
                    for (int i = 0; i < 3; i++) yield return HalloweenKit.Move(dog, dog.position, 0.22f, 0.25f);   // saltella per giocare
                else
                    yield return HalloweenKit.Shake(dog, 0.04f, 0.5f);
                // un passo più vicino
                Vector3 step = dogHome + new Vector3(-0.32f * logic.Strikes, -0.12f * logic.Strikes, 0f);
                StartCoroutine(HalloweenKit.ScaleTo(dog, dogScale * (1f + 0.08f * logic.Strikes), 0.35f));
                yield return HalloweenKit.Move(dog, step, 0.35f, 0.1f);
                yield return new WaitForSeconds(0.8f);
            }
            // la lampada si spegne: si ricompone
            StartCoroutine(HalloweenKit.Fade(lightCone, 0f, 0.2f));
            yield return HalloweenKit.Fade(wallGlow, 0f, 0.2f);
            HideShadows();
            logic.TryFinishShow();
            if (logic.State == PuppetState.Failed)
            {
                Sfx.Play(this, "woof", 1f);
                Haptics.Medium();
                dogSay.Show("GRRRR... WOOF!", 1.6f);
                yield return HalloweenKit.Shake(dog, 0.08f, 0.8f);
                Fail(ShadowPuppetLogic.FailTaunt);
                yield break;
            }
            if (s != Silhouette.Empty)
                toast.Show(logic.Strikes >= ShadowPuppetLogic.MaxStrikes - 1 ? "One more wrong shadow and he snaps!" : "The dog is getting closer...", 1.8f);
            busy = false;
        }

        IEnumerator Flee()
        {
            Haptics.Medium();
            Sfx.Play(this, "boo", 0.7f);
            yield return HalloweenKit.Shake(dog, 0.1f, 0.8f);
            Sfx.Play(this, "whoosh", 0.9f);
            yield return HalloweenKit.Move(dog, dogHome + new Vector3(4.5f, 0.2f, 0f), 0.6f, 0.6f);
            HalloweenKit.Show(dog, false);
            yield return new WaitForSeconds(0.3f);
            Sfx.Play(this, "clunk", 0.7f);
            for (float t = 0f; t < 1f; t += Time.deltaTime / 0.6f)
            {
                float e = 1f - (1f - t) * (1f - t);
                doorPanel.localScale = new Vector3(Mathf.Lerp(doorScale.x, doorScale.x * 0.12f, e), doorScale.y, doorScale.z);
                yield return null;
            }
            toast.Show("The door is free!", 1.8f);
            yield return Celebrate(2f);
            if (logic.TryCompleteFlee()) Win();
        }
    }
}
