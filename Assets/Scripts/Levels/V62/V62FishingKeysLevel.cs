using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels.V62;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 251 "Fishing for keys" — la chiave è caduta nello scarico del lavandino e la porta è chiusa. Le dita
    /// nello scarico non ci passano; la canna da pesca calata a vuoto tira su uno stivale. Soluzione: trascinare la
    /// calamita (sulla mensola) sull'amo, poi la canna sopra lo scarico: la chiave sale attaccata alla calamita e va
    /// portata nella serratura. Easter egg: il pesce della ciotola del gatto sull'amo fa uscire un piranha che saluta.
    /// </summary>
    public class V62FishingKeysLevel : V62Level
    {
        [SerializeField] Transform rod;             // la punta della canna (il disegno è un figlio ruotato)
        [SerializeField] Collider2D rodArea;
        [SerializeField] Transform hook;            // amo (figlio della canna)
        [SerializeField] Transform baitSpot;        // dove si appende l'esca (figlio dell'amo)
        [SerializeField] LineRenderer fishingLine;
        [SerializeField] Transform magnet;
        [SerializeField] Collider2D magnetArea;
        [SerializeField] Transform fish;
        [SerializeField] Collider2D fishArea;
        [SerializeField] Collider2D drainZone;      // dove va lasciata la canna (sopra il lavandino)
        [SerializeField] Collider2D drainTapArea;   // lo scarico: il dito non ci passa
        [SerializeField] Transform drainPoint;
        [SerializeField] GameObject keyGlint;       // la chiave che si intravede giù nello scarico
        [SerializeField] Transform key;
        [SerializeField] Collider2D keyArea;
        [SerializeField] Transform boot;
        [SerializeField] Transform piranha;
        [SerializeField] Collider2D lockArea;
        [SerializeField] Transform lockPoint;
        [SerializeField] Transform doorPanel;
        [SerializeField] Collider2D doorArea;
        [SerializeField] Collider2D catArea;
        [SerializeField] SmokeEmitter splash;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] TextMesh infoText;
        [SerializeField] Transform infoHolder;

        const float HookDrop = 0.7f;       // lunghezza della lenza a riposo
        const float AttachRadius = 0.75f;  // quanto vicino all'amo va lasciata l'esca
        const float CastHeight = 1.5f;     // altezza dell'amo sopra lo scarico quando la canna è in posizione

        readonly FishingKeysLogic logic = new FishingKeysLogic();
        Camera cam;
        HwDragger drag;
        HwDragger.Item rodItem, magnetItem, fishItem, keyItem;
        HwSay say;
        HwToast toast;
        Vector3 rodHome, magnetHome, fishHome, doorScale;
        bool busy;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            logic.Reset();
            ResetV62();
            say = new HwSay(bubble);
            toast = new HwToast(infoText, infoHolder);
            drag = new HwDragger(this);
            rodHome = rod.position;
            magnetHome = magnet.position;
            fishHome = fish.position;
            doorScale = doorPanel.localScale;
            rodItem = drag.Add(rod, rodArea, 1);
            magnetItem = drag.Add(magnet, magnetArea, 5);
            fishItem = drag.Add(fish, fishArea, 5);
            keyItem = drag.Add(key, keyArea, 8);
            keyItem.Enabled = false;
            key.gameObject.SetActive(false);
            boot.gameObject.SetActive(false);
            piranha.gameObject.SetActive(false);
            keyGlint.SetActive(true);
            if (splash != null) splash.Stop();
            UpdateLine();
            toast.Show("The key fell down the drain!", 2.2f);
        }

        void Update()
        {
            say.Tick();
            toast.Tick();
            TickV62();
            UpdateLine();
            if (IsFinished || busy || logic.InputLocked) return;
            if (!HalloweenKit.ReadPointer(cam, out Vector2 w, out bool began, out bool ended))
            {
                if (drag.Held != null) Drop(drag.Held, w, true);
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
            if (ended) Drop(drag.Held, w, false);
        }

        /// <summary>La lenza va dalla punta della canna all'amo (che pende sempre dritto).</summary>
        void UpdateLine()
        {
            if (fishingLine == null) return;
            fishingLine.SetPosition(0, rod.position);
            fishingLine.SetPosition(1, hook.position + Vector3.up * 0.04f);
        }

        void OnPick(HwDragger.Item it)
        {
            // l'esca presa dall'amo si stacca
            if ((it == magnetItem || it == fishItem) && it.T.parent == baitSpot)
            {
                logic.TryDetach();
                it.T.SetParent(transform, true);
            }
            if (it == keyItem)
            {
                logic.TryPickKey();
                key.SetParent(transform, true);
                key.localRotation = Quaternion.identity;
            }
        }

        void Tap(Vector2 w)
        {
            if (drainTapArea.OverlapPoint(w))
            {
                if (logic.TryFinger())
                {
                    Haptics.Light();
                    Sfx.Play(this, "bonk", 0.5f);
                    toast.Show("Your finger doesn't fit!", 1.8f);
                    StartCoroutine(HalloweenKit.Shake(keyGlint.transform, 0.03f, 0.3f));
                }
                return;
            }
            if (lockArea.OverlapPoint(w) || doorArea.OverlapPoint(w))
            {
                Sfx.Play(this, "clunk", 0.5f);
                StartCoroutine(HalloweenKit.Shake(doorPanel, 0.04f, 0.25f));
                toast.Show(logic.KeyInDrain ? "Locked. The key is down the drain." : "Locked. Bring the key here!", 1.8f);
                return;
            }
            if (catArea != null && catArea.OverlapPoint(w))
            {
                Sfx.Play(this, "snore", 0.5f);
                say.Show("Zzz... mrrr... my fish...", 1.8f);
            }
        }

        void Drop(HwDragger.Item it, Vector2 w, bool lostFinger)
        {
            drag.Release();
            if (it == rodItem)
            {
                if (!lostFinger && drainZone.OverlapPoint(hook.position)) StartCoroutine(Cast());
                else drag.SendHome(rodItem, 0.35f);
                return;
            }
            if (it == magnetItem || it == fishItem)
            {
                HookBait bait = it == magnetItem ? HookBait.Magnet : HookBait.Fish;
                if (!lostFinger && Vector2.Distance(it.T.position, baitSpot.position) < AttachRadius && logic.TryAttach(bait))
                {
                    Attach(it.T);
                    Sfx.Play(this, "clunk", 0.6f);
                    Haptics.Light();
                    toast.Show(bait == HookBait.Magnet ? "Clack! The magnet sticks to the hook." : "A fish on a hook. Classic.", 1.6f);
                    return;
                }
                if (!lostFinger && drainTapArea.OverlapPoint(it.T.position))
                    toast.Show(bait == HookBait.Magnet ? "It can't reach the key from here." : "Not without a fishing rod!", 1.6f);
                drag.SendHome(it, 0.35f);
                return;
            }
            if (it == keyItem)
            {
                bool onLock = !lostFinger && (lockArea.OverlapPoint(key.position) || lockArea.OverlapPoint(w));
                if (logic.TryDropKey(onLock))
                {
                    keyItem.Enabled = false;
                    StartCoroutine(OpenDoor());
                    return;
                }
                drag.SendHome(keyItem, 0.35f);
            }
        }

        void Attach(Transform bait)
        {
            bait.SetParent(baitSpot, true);
            bait.localPosition = Vector3.zero;
        }

        /// <summary>La canna si cala nello scarico e tira su quello che c'è.</summary>
        IEnumerator Cast()
        {
            busy = true;
            CastResult result = logic.TryCast();
            rodItem.Enabled = false;
            // la canna si mette sopra lo scarico e la lenza scende giù
            Vector3 over = drainPoint.position + Vector3.up * (HookDrop + CastHeight);
            yield return HalloweenKit.Move(rod, new Vector3(over.x, over.y, rod.position.z), 0.3f);
            Vector3 rest = hook.localPosition;
            Vector3 down = rest + Vector3.down * (CastHeight + 0.05f);
            yield return MoveLocal(hook, down, 0.45f);
            Sfx.Play(this, "whoosh", 0.5f);
            if (splash != null) StartCoroutine(HalloweenKit.Burst(splash, 0.3f));
            ShowHookContents(false);
            yield return new WaitForSeconds(0.5f);
            yield return HalloweenKit.Shake(rod, 0.05f, 0.5f);
            Haptics.Light();
            // cosa abbocca
            Transform caught = result == CastResult.Key ? key : result == CastResult.Piranha ? piranha : boot;
            if (result == CastResult.Key) { keyGlint.SetActive(false); HookKey(); }
            else
            {
                caught.SetParent(baitSpot, true);
                caught.localPosition = new Vector3(0f, -0.25f, 0f);
                caught.gameObject.SetActive(true);
            }
            if (result == CastResult.Piranha) fish.gameObject.SetActive(false);   // se l'è mangiato
            ShowHookContents(true);
            Sfx.Play(this, "pop", 0.6f);
            if (splash != null) StartCoroutine(HalloweenKit.Burst(splash, 0.3f));
            yield return MoveLocal(hook, rest, 0.5f);
            logic.TryFinishCast();
            switch (result)
            {
                case CastResult.Key:
                    Haptics.Medium();
                    Sfx.Play(this, "ding", 0.7f);
                    toast.Show("The key! Stuck to the magnet!", 2f);
                    keyItem.Home = key.position;
                    keyItem.Enabled = true;
                    magnetItem.Enabled = false;   // la calamita resta sull'amo: ora conta solo la chiave
                    fishItem.Enabled = false;
                    busy = false;
                    yield break;
                case CastResult.Piranha:
                    yield return HalloweenKit.Wiggle(piranha, 18f, 1.0f, 14f);
                    toast.Show("A piranha?! \"Hi! Thanks for the snack!\"", 2.2f);
                    RevealEgg(FishingKeysLogic.EggId);
                    yield return HalloweenKit.Wiggle(piranha, 25f, 1.4f, 20f);
                    break;
                default:
                    Sfx.Play(this, "clunk", 0.5f);
                    toast.Show("Nice catch. Wrong one.", 1.8f);
                    yield return HalloweenKit.Wiggle(boot, 8f, 1.2f, 10f);
                    break;
            }
            // quello che non serve torna giù nello scarico; la canna torna al suo posto
            yield return MoveLocal(hook, down, 0.35f);
            caught.gameObject.SetActive(false);
            caught.SetParent(transform, true);
            if (splash != null) StartCoroutine(HalloweenKit.Burst(splash, 0.25f));
            yield return MoveLocal(hook, rest, 0.3f);
            if (result == CastResult.Piranha) StartCoroutine(RespawnFish());
            yield return HalloweenKit.Move(rod, rodHome, 0.35f, 0.3f);
            rodItem.Enabled = true;
            busy = false;
        }

        /// <summary>La chiave sale attaccata sotto la calamita.</summary>
        void HookKey()
        {
            key.SetParent(baitSpot, true);
            key.localPosition = new Vector3(0.62f, -0.28f, 0f);
            key.localRotation = Quaternion.Euler(0f, 0f, -15f);
            key.gameObject.SetActive(true);
        }

        void ShowHookContents(bool on)
        {
            hook.gameObject.SetActive(on);
        }

        IEnumerator RespawnFish()
        {
            yield return new WaitForSeconds(0.6f);
            fish.SetParent(transform, true);
            fish.position = fishHome;
            fish.localRotation = Quaternion.identity;
            fish.gameObject.SetActive(true);
            fishItem.Home = fishHome;
            Sfx.Play(this, "pop", 0.5f);
            yield return HalloweenKit.Pop(fish, 1.25f, 0.3f);
        }

        IEnumerator OpenDoor()
        {
            busy = true;
            yield return HalloweenKit.Move(key, lockPoint.position, 0.2f);
            Sfx.Play(this, "tick", 0.7f);
            // la chiave gira nella serratura (si "stringe" in larghezza come vista di taglio)
            Vector3 keyScale = key.localScale;
            for (float t = 0f; t < 1f; t += Time.deltaTime / 0.35f)
            {
                key.localScale = new Vector3(keyScale.x, keyScale.y * Mathf.Lerp(1f, 0.2f, t), keyScale.z);
                yield return null;
            }
            Sfx.Play(this, "clunk", 0.8f);
            Haptics.Medium();
            key.gameObject.SetActive(false);
            for (float t = 0f; t < 1f; t += Time.deltaTime / 0.7f)
            {
                float e = 1f - (1f - t) * (1f - t);
                doorPanel.localScale = new Vector3(Mathf.Lerp(doorScale.x, doorScale.x * 0.12f, e), doorScale.y, doorScale.z);
                yield return null;
            }
            toast.Show("Click! The door is open!", 1.8f);
            yield return Celebrate(2f);
            if (logic.TryCompleteOpening()) Victory(logic.EggFound, FishingKeysLogic.EggId);
        }

        static IEnumerator MoveLocal(Transform t, Vector3 to, float seconds)
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
