using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels.V62;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 254 "Mirror, mirror" — il gatto sulla mensola spinge il vaso prezioso verso il bordo, un colpetto al
    /// secondo (~25 s). Toccarlo lo fa solo arrabbiare (poi spinge più veloce), il pesce lo mangia e riprende, il
    /// campanello lo ignora, il vaso è troppo pesante. Soluzione: lo specchio sulla mensola davanti al gatto, che vede
    /// il suo riflesso, si gonfia, soffia e scappa. Se il vaso cade il livello è perso ("Cats 1, You 0.").
    /// Easter egg: lo specchio davanti a Toni, che dorme sul tappeto: si sveglia, abbaia al riflesso e si riaddormenta.
    /// </summary>
    public class V62MirrorCatLevel : V62Level
    {
        [SerializeField] Transform cat;
        [SerializeField] Collider2D catArea;
        [SerializeField] Transform catFace;          // punto davanti al muso (per lo specchio e il pesce)
        [SerializeField] Transform vase;
        [SerializeField] Collider2D vaseArea;
        [SerializeField] Transform vaseEdge;         // dove il vaso perde l'appoggio
        [SerializeField] Transform floorSpot;        // dove si rompe
        [SerializeField] GameObject shards;
        [SerializeField] Transform mirror;
        [SerializeField] Collider2D mirrorArea;
        [SerializeField] GameObject catReflection;
        [SerializeField] GameObject toniReflection;
        [SerializeField] Transform bell;
        [SerializeField] Collider2D bellArea;
        [SerializeField] SmokeEmitter bellWaves;
        [SerializeField] Transform fish;
        [SerializeField] Collider2D fishArea;
        [SerializeField] Transform toni;
        [SerializeField] Collider2D toniArea;
        [SerializeField] Transform toniFace;
        [SerializeField] GameObject toniEyes;
        [SerializeField] SmokeEmitter zzz;
        [SerializeField] SmokeEmitter fur;
        [SerializeField] SpeechBubble catBubble;
        [SerializeField] SpeechBubble toniBubble;
        [SerializeField] TextMesh infoText;
        [SerializeField] Transform infoHolder;
        [SerializeField] float mirrorReach = 1.05f;

        readonly MirrorCatLogic logic = new MirrorCatLogic();
        Camera cam;
        HwDragger drag;
        HwDragger.Item mirrorItem, fishItem;
        HwSay catSay, toniSay;
        HwToast toast;
        Vector3 vaseStart, catOffset, catScale;
        bool busy, eggShown, toniBarking;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            logic.Reset();
            ResetV62();
            catSay = new HwSay(catBubble);
            toniSay = new HwSay(toniBubble);
            toast = new HwToast(infoText, infoHolder);
            drag = new HwDragger(this);
            vaseStart = vase.position;
            catOffset = cat.position - vase.position;
            catScale = cat.localScale;
            mirrorItem = drag.Add(mirror, mirrorArea, 6);
            fishItem = drag.Add(fish, fishArea, 5);
            HalloweenKit.Show(shards, false);
            HalloweenKit.Show(catReflection, false);
            HalloweenKit.Show(toniReflection, false);
            HalloweenKit.Show(toniEyes, false);
            if (bellWaves != null) bellWaves.Stop();
            if (fur != null) fur.Stop();
            if (zzz != null) zzz.Play();
            catSay.Show("Push. Push. Push.", 1.8f);
        }

        void Update()
        {
            catSay.Tick();
            toniSay.Tick();
            toast.Tick();
            TickV62();
            if (IsFinished || busy) return;
            int pushes = logic.Tick(Time.deltaTime);
            if (pushes > 0) Shove();
            FollowVase();
            if (logic.State == MirrorCatState.Fallen) { drag.Cancel(); StartCoroutine(VaseFalls()); return; }
            if (logic.InputLocked) return;
            if (!HalloweenKit.ReadPointer(cam, out Vector2 w, out bool began, out bool ended))
            {
                if (drag.Held != null) Drop(drag.Held, true);
                return;
            }
            if (began && drag.TryPick(w) == null) Tap(w);
            if (drag.Held == null) return;
            drag.MoveHeld(w);
            if (ended) Drop(drag.Held, false);
        }

        /// <summary>Il vaso scivola verso la sua posizione (una spinta = un passo) e il gatto gli sta dietro.</summary>
        void FollowVase()
        {
            float x = Mathf.Lerp(vaseStart.x, vaseEdge.position.x, logic.Progress);
            Vector3 v = vase.position;
            v.x = Mathf.MoveTowards(v.x, x, Time.deltaTime * 0.6f);
            vase.position = v;
            if (logic.State == MirrorCatState.Pushing || logic.State == MirrorCatState.Paused)
                cat.position = new Vector3(vase.position.x + catOffset.x, cat.position.y, cat.position.z);
        }

        void Shove()
        {
            Sfx.Play(this, "tick", 0.45f);
            StartCoroutine(HalloweenKit.Wiggle(cat, 7f, 0.25f, 25f));
            if (logic.PushesLeft <= 5) catSay.Show(logic.PushesLeft <= 2 ? "Almost..." : "Heh heh.", 0.9f);
        }

        void Tap(Vector2 w)
        {
            if (catArea.OverlapPoint(w))
            {
                if (logic.TryTouchCat())
                {
                    Sfx.Play(this, "bonk", 0.5f);
                    Haptics.Light();
                    catSay.Show("Don't touch me.", 1.6f);
                    StartCoroutine(HalloweenKit.Pop(cat, 1.08f, 0.3f));
                }
                return;
            }
            if (vaseArea.OverlapPoint(w))
            {
                if (logic.TryDragVase())
                {
                    Sfx.Play(this, "clunk", 0.5f);
                    StartCoroutine(HalloweenKit.Shake(vase, 0.03f, 0.25f));
                    toast.Show("Too heavy!", 1.4f);
                }
                return;
            }
            if (bellArea.OverlapPoint(w))
            {
                if (logic.TryRingBell())
                {
                    Sfx.Play(this, "ding", 0.6f);
                    if (bellWaves != null) StartCoroutine(HalloweenKit.Burst(bellWaves, 0.5f));
                    StartCoroutine(HalloweenKit.Pop(bell, 1.1f, 0.2f));
                    catSay.Show("...", 1.2f);
                }
                return;
            }
            if (toniArea.OverlapPoint(w) && !toniBarking)
            {
                Sfx.Play(this, "snore", 0.5f);
                toniSay.Show("Zzz...", 1.2f);
            }
        }

        void Drop(HwDragger.Item it, bool lostFinger)
        {
            drag.Release();
            if (it == fishItem)
            {
                if (!lostFinger && (catArea.OverlapPoint(fish.position) || Vector2.Distance(fish.position, catFace.position) < 0.8f) && logic.TryFeedFish())
                {
                    StartCoroutine(EatFish());
                    return;
                }
                drag.SendHome(fishItem, 0.35f);
                return;
            }
            if (it == mirrorItem)
            {
                MirrorTarget target = MirrorTarget.Nowhere;
                if (!lostFinger && Vector2.Distance(mirror.position, catFace.position) < mirrorReach) target = MirrorTarget.InFrontOfCat;
                else if (!lostFinger && !toniBarking && Vector2.Distance(mirror.position, toniFace.position) < mirrorReach) target = MirrorTarget.InFrontOfToni;
                bool scared = logic.TryDropMirror(target);
                if (scared) { StartCoroutine(CatScared()); return; }
                if (target == MirrorTarget.InFrontOfToni) { StartCoroutine(ToniBarks()); return; }
                drag.SendHome(mirrorItem, 0.35f);
            }
        }

        IEnumerator EatFish()
        {
            fishItem.Enabled = false;
            yield return HalloweenKit.Move(fish, catFace.position, 0.15f);
            fish.gameObject.SetActive(false);
            Sfx.Play(this, "pop", 0.7f);
            catSay.Show("*GULP* Thanks. Still pushing.", 2f);
            yield return new WaitForSeconds(2.5f);
            fish.position = fishItem.Home;
            fish.gameObject.SetActive(true);
            fishItem.Enabled = true;
            yield return HalloweenKit.Pop(fish, 1.25f, 0.3f);
        }

        IEnumerator ToniBarks()
        {
            toniBarking = true;
            mirrorItem.Enabled = false;
            yield return HalloweenKit.Move(mirror, toniFace.position + new Vector3(-0.55f, 0.1f, 0f), 0.2f);
            HalloweenKit.Show(toniReflection, true);
            if (zzz != null) zzz.Stop();
            yield return new WaitForSeconds(0.3f);
            HalloweenKit.Show(toniEyes, true);
            for (int i = 0; i < 2; i++)
            {
                Sfx.Play(this, "woof", 0.8f);
                toniSay.Show(i == 0 ? "WOOF?!" : "WOOF! WOOF! Another Toni!", 1.4f);
                yield return HalloweenKit.Shake(toni, 0.06f, 0.4f);
                yield return new WaitForSeconds(0.25f);
            }
            if (!eggShown) { eggShown = true; RevealEgg(MirrorCatLogic.EggId); }
            yield return new WaitForSeconds(0.6f);
            HalloweenKit.Show(toniEyes, false);
            HalloweenKit.Show(toniReflection, false);
            toniSay.Show("...zzz...", 1.2f);
            if (zzz != null) zzz.Play();
            drag.SendHome(mirrorItem, 0.4f);
            yield return new WaitForSeconds(0.4f);
            mirrorItem.Enabled = true;
            toniBarking = false;
        }

        IEnumerator CatScared()
        {
            busy = true;
            mirrorItem.Enabled = false;
            fishItem.Enabled = false;
            // lo specchio si appoggia sulla mensola, di fronte al gatto
            yield return HalloweenKit.Move(mirror, catFace.position + new Vector3(0.75f, 0f, 0f), 0.2f);
            HalloweenKit.Show(catReflection, true);
            catSay.Show("...?!", 0.8f);
            yield return new WaitForSeconds(0.6f);
            // pelo ritto, si gonfia, soffia
            if (fur != null) fur.Play();
            Sfx.Play(this, "buzz", 0.7f);
            Haptics.Medium();
            StartCoroutine(HalloweenKit.ScaleTo(cat, catScale * 1.3f, 0.3f));
            catSay.Show("HSSSSS!", 1.4f);
            yield return HalloweenKit.Shake(cat, 0.08f, 0.8f);
            if (fur != null) fur.Stop();
            // e scappa giù dalla mensola
            Sfx.Play(this, "whoosh", 0.8f);
            StartCoroutine(HalloweenKit.ScaleTo(cat, catScale, 0.3f));
            yield return HalloweenKit.Move(cat, cat.position + new Vector3(2.4f, -3.5f, 0f), 0.6f, 0.8f);
            cat.gameObject.SetActive(false);
            HalloweenKit.Show(catReflection, false);
            toast.Show("The vase is safe!", 2f);
            yield return Celebrate(2f);
            if (logic.TryCompleteSave()) Victory(logic.EggFound, MirrorCatLogic.EggId);
        }

        IEnumerator VaseFalls()
        {
            busy = true;
            catSay.Show("Oops.", 1.5f);
            Vector3 from = vase.position;
            Vector3 edge = new Vector3(vaseEdge.position.x, from.y, from.z);
            for (float t = 0f; t < 1f; t += Time.deltaTime / 0.35f)
            {
                vase.position = Vector3.Lerp(from, edge, t) + new Vector3(-0.25f * t, 0f, 0f);
                vase.localRotation = Quaternion.Euler(0f, 0f, 35f * t);
                yield return null;
            }
            yield return HalloweenKit.Move(vase, floorSpot.position, 0.45f);
            vase.gameObject.SetActive(false);
            HalloweenKit.Show(shards, true);
            Sfx.Play(this, "bonk", 1f);
            Sfx.Play(this, "clunk", 1f);
            Haptics.Medium();
            yield return new WaitForSeconds(1.2f);
            Fail("Cats 1, You 0.");
        }
    }
}
