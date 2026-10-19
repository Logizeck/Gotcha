using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "Kitchen escape" — un topolino chiuso in cucina, con un vassoio dell'inventario da 4 posti in basso.
    /// Fase 1: raccogli calamita (sul frigo), forchetta (per terra) e spago (nel cassetto: toccalo per aprirlo).
    /// Fase 2: nel vassoio trascina lo spago sulla calamita → "calamita con lo spago".
    /// Fase 3: trascinala sullo scarico del lavello: si pesca la chiave caduta giù.
    /// Fase 4: il pensile ha una serratura E un blocco a prova di bambino: chiave sulla serratura, poi pizzica il blocco con
    /// due dita. Dentro c'è il formaggio. Combinazioni sbagliate fanno perdere uno dei 3 tentativi (la forchetta è un'esca).
    /// </summary>
    public class KitchenEscapeLevel : LevelController
    {
        const int Magnet = 0, Fork = 1, Twine = 2, MagnetTwine = 3, KeyItem = 4;

        [SerializeField] StageHud40B hud;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] Transform mouse;
        [SerializeField] Collider2D mouseArea;
        [SerializeField] Transform[] items;          // calamita, forchetta, spago, calamita+spago, chiave (con Collider2D)
        [SerializeField] Transform[] slots;          // i 4 posti del vassoio
        [SerializeField] Collider2D drawerArea;
        [SerializeField] SpriteRenderer drawerArt;
        [SerializeField] Sprite drawerOpenSprite;
        [SerializeField] Collider2D drainArea;
        [SerializeField] Transform drainPoint;
        [SerializeField] LineRenderer fishLine;
        [SerializeField] Collider2D cupboardArea;
        [SerializeField] SpriteRenderer cupboardArt;
        [SerializeField] Sprite cupboardOpenSprite;
        [SerializeField] Collider2D lockArea;
        [SerializeField] Collider2D latchArea;
        [SerializeField] SpriteRenderer latchArt;
        [SerializeField] Sprite latchOpenSprite;
        [SerializeField] Transform cheese;
        [SerializeField] Transform cheeseLanding;
        [SerializeField] float dragStart = 0.15f, dropRadius = 0.6f;

        int stage;
        int[] slotItem;
        bool[] inTray;
        Collider2D[] cols;
        SpriteRenderer[] arts;
        int[] baseOrder;
        bool busy, drawerOpen, unlocked, latchOpen;
        int grabbed = -1;
        bool dragging;
        Vector2 grabStart, grabOffset;
        float pinchStart, bubbleUntil, clock, rodScale;
        Camera cam;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            slotItem = new[] { -1, -1, -1, -1 };
            inTray = new bool[items.Length];
            cols = new Collider2D[items.Length];
            arts = new SpriteRenderer[items.Length];
            baseOrder = new int[items.Length];
            for (int i = 0; i < items.Length; i++)
            {
                cols[i] = items[i].GetComponent<Collider2D>();
                arts[i] = items[i].GetComponentInChildren<SpriteRenderer>();
                baseOrder[i] = arts[i].sortingOrder;
            }
            rodScale = items[MagnetTwine].localScale.x;
            items[Twine].gameObject.SetActive(false);
            items[MagnetTwine].gameObject.SetActive(false);
            items[KeyItem].gameObject.SetActive(false);
            cheese.gameObject.SetActive(false);
            fishLine.enabled = false;
            hud.Begin(3);
            hud.Goal("Collect useful things: tap to pick them up.");
            Say("Squeak! Locked in! The cheese is in that cupboard...", 3f);
        }

        void Say(string text, float seconds = 2.2f)
        {
            if (bubble == null) return;
            bubble.Show(text);
            bubbleUntil = clock + seconds;
        }

        void Update()
        {
            if (IsFinished) return;
            clock += Time.deltaTime;
            if (bubbleUntil > 0f && clock > bubbleUntil) { bubble.Hide(); bubbleUntil = 0f; }
            if (busy) return;
            if (stage == 3 && !latchOpen && Pinch()) return;
            if (!PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out bool ended)) { if (grabbed >= 0) Release(grabStart); return; }
            Vector2 w = PointerInput.ToWorld(cam, sp);
            if (began)
            {
                if (PointerInput.IsOverUi()) return;
                int t = TrayItemAt(w);
                if (t >= 0)
                {
                    grabbed = t;
                    grabStart = w;
                    grabOffset = (Vector2)items[t].position - w;
                    dragging = false;
                    arts[t].sortingOrder = 45;
                    return;
                }
                Tap(w);
                return;
            }
            if (grabbed < 0) return;
            if (!dragging && (w - grabStart).magnitude > dragStart) dragging = true;
            if (dragging) items[grabbed].position = (Vector3)(w + grabOffset) + Vector3.forward * items[grabbed].position.z;
            if (ended) Release(w);
        }

        // ---------- pizzico sul blocco (due dita che si avvicinano) ----------

        bool Pinch()
        {
            if (Input.touchCount >= 2)
            {
                Vector2 a = PointerInput.ToWorld(cam, Input.GetTouch(0).position), b = PointerInput.ToWorld(cam, Input.GetTouch(1).position);
                float d = Vector2.Distance(a, b);
                if (pinchStart <= 0f)
                {
                    // vale solo se le dita sono vicine al blocco
                    if (Vector2.Distance((a + b) * 0.5f, latchArea.bounds.center) < 1.6f) pinchStart = d;
                }
                else if (d < pinchStart * 0.7f || pinchStart - d > 0.6f) { pinchStart = 0f; StartCoroutine(OpenLatch()); }
                if (grabbed >= 0) { arts[grabbed].sortingOrder = baseOrder[grabbed]; StartCoroutine(Fly(grabbed, slots[SlotOf(grabbed)].position)); grabbed = -1; }
                return true;
            }
            pinchStart = 0f;
#if UNITY_EDITOR
            if (Input.mouseScrollDelta.y < -0.01f) { StartCoroutine(OpenLatch()); return true; }
#endif
            return false;
        }

        // ---------- tocchi sulla scena ----------

        void Tap(Vector2 w)
        {
            for (int i = 0; i < items.Length; i++)
                if (!inTray[i] && items[i].gameObject.activeSelf && cols[i].OverlapPoint(w)) { Pick(i); return; }
            if (drawerArea.OverlapPoint(w))
            {
                if (!drawerOpen) OpenDrawer();
                else Say("An empty drawer. Well, now it is.");
                return;
            }
            if (drainArea.OverlapPoint(w))
            {
                Say(stage < 2 ? "The key fell down the drain! Too deep for my paws." : "Something shiny down there...");
                Sfx.Play(this, "tick", 0.5f);
                return;
            }
            if (lockArea.gameObject.activeSelf && lockArea.OverlapPoint(w)) { Say(unlocked ? "Unlocked!" : "Locked. I need a key."); return; }
            if (latchArea.gameObject.activeSelf && latchArea.OverlapPoint(w))
            {
                Say(latchOpen ? "Child-lock: defeated." : "Child-proof! Squeeze it with TWO fingers.");
                Sfx.Play(this, "clunk", 0.4f);
                return;
            }
            if (cupboardArea.OverlapPoint(w)) { Say("Locked AND child-proofed. Who has kids here?!"); return; }
            if (mouseArea.OverlapPoint(w)) { Say("Squeak! I smell cheese. Help me!"); Sfx.Play(this, "pop", 0.4f); }
        }

        void OpenDrawer()
        {
            drawerOpen = true;
            drawerArt.sprite = drawerOpenSprite;
            Sfx.Play(this, "whoosh", 0.5f);
            items[Twine].gameObject.SetActive(true);
            Say("A ball of string!", 1.6f);
        }

        void Pick(int i)
        {
            int s = FreeSlot();
            if (s < 0) return;
            slotItem[s] = i;
            inTray[i] = true;
            Sfx.Play(this, "pop", 0.7f);
            Haptics.Light();
            StartCoroutine(Fly(i, slots[s].position));
            if (stage == 0 && inTray[Magnet] && inTray[Fork] && inTray[Twine]) StartCoroutine(StageCombine());
        }

        int FreeSlot()
        {
            for (int s = 0; s < slotItem.Length; s++) if (slotItem[s] < 0) return s;
            return -1;
        }

        int SlotOf(int item)
        {
            for (int s = 0; s < slotItem.Length; s++) if (slotItem[s] == item) return s;
            return -1;
        }

        int TrayItemAt(Vector2 w)
        {
            for (int s = 0; s < slotItem.Length; s++)
                if (slotItem[s] >= 0 && Vector2.Distance(w, slots[s].position) < 0.55f) return slotItem[s];
            return -1;
        }

        IEnumerator Fly(int i, Vector3 to)
        {
            Transform t = items[i];
            Vector3 from = t.position;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.3f)
            {
                t.position = Vector3.Lerp(from, to, k) + Vector3.up * Mathf.Sin(k * Mathf.PI) * 0.4f;
                yield return null;
            }
            t.position = to;
        }

        // ---------- rilascio di un oggetto del vassoio ----------

        void Release(Vector2 w)
        {
            int i = grabbed;
            grabbed = -1;
            arts[i].sortingOrder = baseOrder[i];
            int home = SlotOf(i);
            if (!dragging)
            {
                Say(Describe(i), 1.8f);
                items[i].position = slots[home].position;
                return;
            }
            Vector2 p = items[i].position;
            // su un altro oggetto del vassoio: combinazione
            for (int s = 0; s < slotItem.Length; s++)
            {
                int j = slotItem[s];
                if (j < 0 || j == i || Vector2.Distance(p, slots[s].position) > dropRadius) continue;
                Combine(i, j);
                return;
            }
            if (Near(drainArea, p)) { DropOnDrain(i); return; }
            if (lockArea.gameObject.activeSelf && Near(lockArea, p)) { DropOnLock(i); return; }
            if (latchArea.gameObject.activeSelf && Near(latchArea, p)) { DropOnLatch(i); return; }
            if (mouseArea.OverlapPoint(p)) Say(i == Fork ? "A fork? I'm a mouse, not a monster!" : "Squeak? I can't eat that.");
            StartCoroutine(Fly(i, slots[home].position));
        }

        bool Near(Collider2D c, Vector2 p) => c.OverlapPoint(p) || Vector2.Distance(c.bounds.center, p) < dropRadius;

        string Describe(int i)
        {
            switch (i)
            {
                case Magnet: return "A strong fridge magnet.";
                case Fork: return "A fork. Very pointy. Very useless?";
                case Twine: return "A long piece of string.";
                case MagnetTwine: return "A magnet on a string. A fishing rod!";
                default: return "The key! Shiny.";
            }
        }

        void Back(int i) => StartCoroutine(Fly(i, slots[SlotOf(i)].position));

        void Wrong(int i, string line)
        {
            Say(line, 2.2f);
            Sfx.Play(this, "buzz", 0.5f);
            Back(i);
            if (hud.LoseTry()) StartCoroutine(Lose());
        }

        void Combine(int a, int b)
        {
            if (stage == 0) { Say("First grab everything you can find!"); Back(a); return; }
            if ((a == Magnet && b == Twine) || (a == Twine && b == Magnet)) { StartCoroutine(MakeRod(a, b)); return; }
            bool fork = a == Fork || b == Fork, magnet = a == Magnet || b == Magnet || a == MagnetTwine || b == MagnetTwine;
            if (fork && magnet) Wrong(a, "A magnetic fork? Science says no.");
            else if (fork) Wrong(a, "Fork + string = spaghetti? Nope.");
            else if (a == KeyItem || b == KeyItem) Wrong(a, "Don't tie the key up! It goes in a lock.");
            else Wrong(a, "Those two don't go together.");
        }

        void DropOnDrain(int i)
        {
            if (i == MagnetTwine && stage == 2) { StartCoroutine(Fish()); return; }
            if (stage == 0) { Say("First grab everything you can find!"); Back(i); return; }
            if (i == Magnet) Wrong(i, "Too short! It needs something to hang from.");
            else if (i == Twine) Wrong(i, "String alone won't grab a key.");
            else if (i == Fork) Wrong(i, "Fishing with a fork? Forking hopeless.");
            else Wrong(i, "The key is already out!");
        }

        void DropOnLock(int i)
        {
            if (i == KeyItem && stage == 3) { StartCoroutine(Unlock()); return; }
            if (stage == 0) { Say("First grab everything you can find!"); Back(i); return; }
            if (i == Fork) Wrong(i, "Picking the lock with a fork? Not in this kitchen.");
            else if (i == MagnetTwine) Wrong(i, "That's a fishing rod, not a key.");
            else Wrong(i, "That doesn't fit the lock.");
        }

        void DropOnLatch(int i)
        {
            if (stage == 0) { Say("First grab everything you can find!"); Back(i); return; }
            if (i == KeyItem) { Say("No keyhole on the child-lock. Squeeze it with TWO fingers!"); Back(i); return; }
            Wrong(i, i == Fork ? "Forks are for cheese, not locks." : "The child-lock laughs at you.");
        }

        // ---------- fasi ----------

        IEnumerator StageCombine()
        {
            busy = true;
            yield return new WaitForSeconds(0.35f);
            yield return hud.Next(1, "STAGE 2: TINKER", "Combine two items: drag one onto another in the tray.");
            stage = 1;
            busy = false;
        }

        IEnumerator MakeRod(int a, int b)
        {
            busy = true;
            int sa = SlotOf(a), sb = SlotOf(b), keep = SlotOf(Magnet);
            Vector3 mid = slots[keep].position;
            yield return Fly(a == Magnet ? b : a, mid);
            Sfx.Play(this, "pop", 0.8f);
            items[Magnet].gameObject.SetActive(false);
            items[Twine].gameObject.SetActive(false);
            slotItem[sa] = -1;
            slotItem[sb] = -1;
            inTray[Magnet] = inTray[Twine] = false;
            slotItem[keep] = MagnetTwine;
            inTray[MagnetTwine] = true;
            Transform rod = items[MagnetTwine];
            rod.position = mid;
            rod.gameObject.SetActive(true);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.3f) { rod.localScale = Vector3.one * Mathf.Lerp(0.2f, 1f, k) * rodScale * (1f + Mathf.Sin(k * Mathf.PI) * 0.3f); yield return null; }
            rod.localScale = Vector3.one * rodScale;
            Say("Magnet on a string! Now let's go fishing.");
            yield return hud.Next(2, "STAGE 3: GONE FISHING", "Get the key out of the sink drain!");
            stage = 2;
            busy = false;
        }

        IEnumerator Fish()
        {
            busy = true;
            int s = SlotOf(MagnetTwine);
            Transform rod = items[MagnetTwine];
            Vector3 top = drainPoint.position + new Vector3(0f, 1.6f, 0f), dip = drainPoint.position + new Vector3(0f, 0.15f, 0f);
            Transform key = items[KeyItem];
            fishLine.enabled = true;
            Sfx.Play(this, "whoosh", 0.5f);
            // giù nello scarico
            Vector3 from = rod.position;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.35f) { rod.position = Vector3.Lerp(from, top, k); Line(top, rod); yield return null; }
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.6f)
            {
                rod.position = Vector3.Lerp(top, dip, k);
                rod.localScale = Vector3.one * Mathf.Lerp(1f, 0.4f, k) * rodScale;
                Line(top, rod);
                yield return null;
            }
            Say("Fishing... fishing...", 1.5f);
            for (float k = 0f; k < 1.2f; k += Time.deltaTime)
            {
                rod.position = dip + new Vector3(Mathf.Sin(k * 25f) * 0.04f, 0f, 0f);
                Line(top, rod);
                if (Mathf.Repeat(k, 0.3f) < Time.deltaTime) Sfx.Play(this, "tick", 0.6f);
                yield return null;
            }
            Sfx.Play(this, "clunk", 0.8f);
            Haptics.Light();
            // su con la chiave attaccata
            key.gameObject.SetActive(true);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.6f)
            {
                rod.position = Vector3.Lerp(dip, top, k);
                rod.localScale = Vector3.one * Mathf.Lerp(0.4f, 1f, k) * rodScale;
                key.position = rod.position + new Vector3(0.05f, -0.4f, 0f);
                Line(top, rod);
                yield return null;
            }
            fishLine.enabled = false;
            Say("GOT IT! The key!", 1.8f);
            Haptics.Medium();
            int ks = FreeSlot();
            slotItem[ks] = KeyItem;
            inTray[KeyItem] = true;
            StartCoroutine(Fly(KeyItem, slots[ks].position));
            yield return Fly(MagnetTwine, slots[s].position);
            yield return new WaitForSeconds(0.3f);
            yield return hud.Next(3, "STAGE 4: THE CUPBOARD", "Unlock the cupboard, then beat the child-lock!");
            stage = 3;
            busy = false;
        }

        void Line(Vector3 top, Transform rod)
        {
            fishLine.SetPosition(0, top);
            fishLine.SetPosition(1, rod.position + new Vector3(0f, 0.35f * rod.localScale.x / Mathf.Max(0.01f, rodScale), 0f));
        }

        IEnumerator Unlock()
        {
            busy = true;
            Transform key = items[KeyItem];
            int s = SlotOf(KeyItem);
            slotItem[s] = -1;
            inTray[KeyItem] = false;
            Vector3 from = key.position, to = lockArea.bounds.center + new Vector3(0.45f, 0f, 0f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.25f) { key.position = Vector3.Lerp(from, to, k); yield return null; }
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.35f) { key.localRotation = Quaternion.Euler(k * 90f, 0f, 0f); yield return null; }
            Sfx.Play(this, "clunk", 0.9f);
            Haptics.Light();
            unlocked = true;
            yield return new WaitForSeconds(0.3f);
            key.gameObject.SetActive(false);
            lockArea.gameObject.SetActive(false);
            if (!latchOpen) { Say("Click! Now that silly child-lock... pinch it!"); hud.Goal("Squeeze the child-lock with TWO fingers!"); }
            busy = false;
            if (latchOpen) StartCoroutine(OpenCupboard());
        }

        IEnumerator OpenLatch()
        {
            busy = true;
            latchOpen = true;
            latchArt.sprite = latchOpenSprite;
            Sfx.Play(this, "clunk", 0.7f);
            Sfx.Play(this, "pop", 0.6f);
            Haptics.Light();
            Vector3 s = latchArt.transform.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.3f) { latchArt.transform.localScale = s * (1f + Mathf.Sin(k * Mathf.PI) * 0.25f); yield return null; }
            latchArt.transform.localScale = s;
            yield return new WaitForSeconds(0.3f);
            latchArea.gameObject.SetActive(false);
            if (!unlocked) { Say("Child-lock: beaten! But the key lock is still shut."); hud.Goal("Now use the key on the lock!"); }
            busy = false;
            if (unlocked) StartCoroutine(OpenCupboard());
        }

        IEnumerator OpenCupboard()
        {
            busy = true;
            hud.Goal("");
            yield return new WaitForSeconds(0.2f);
            cupboardArt.sprite = cupboardOpenSprite;
            Sfx.Play(this, "whoosh", 0.7f);
            cheese.gameObject.SetActive(true);
            Say("CHEEEESE!", 2f);
            yield return new WaitForSeconds(0.7f);
            // il formaggio cade giù dal topolino
            Vector3 from = cheese.position, to = cheeseLanding.position;
            float spin = 0f;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.7f)
            {
                cheese.position = new Vector3(Mathf.Lerp(from.x, to.x, k), Mathf.Lerp(from.y, to.y, k * k) + Mathf.Sin(k * Mathf.PI) * 0.5f, from.z);
                spin += 400f * Time.deltaTime;
                cheese.localRotation = Quaternion.Euler(0, 0, spin);
                yield return null;
            }
            cheese.position = to;
            cheese.localRotation = Quaternion.identity;
            Sfx.Play(this, "bonk", 0.8f);
            Haptics.Medium();
            hud.SetStage(4);
            hud.Shout("ESCAPED! (WITH CHEESE)");
            Vector3 m = mouse.localPosition;
            for (int j = 0; j < 3; j++)
                for (float k = 0f; k < 1f; k += Time.deltaTime / 0.3f) { mouse.localPosition = m + Vector3.up * Mathf.Sin(k * Mathf.PI) * 0.35f; yield return null; }
            mouse.localPosition = m;
            Say("Nom nom nom. Best escape ever.", 3f);
            yield return new WaitForSeconds(1.6f);
            Win();
        }

        IEnumerator Lose()
        {
            busy = true;
            hud.Goal("");
            Say("Out of ideas... I'll just starve here. Squeak.", 3f);
            Vector3 m = mouse.localPosition;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.8f) { mouse.localPosition = m + new Vector3(Mathf.Sin(k * 40f) * 0.04f, 0f, 0f); yield return null; }
            mouse.localPosition = m;
            yield return new WaitForSeconds(1.4f);
            Fail();
        }
    }
}
