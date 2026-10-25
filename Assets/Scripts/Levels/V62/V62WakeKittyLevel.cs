using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels.V62;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 252 "Wake up, Kitty!" — il gatto dorme sul suo cuscino ed è ora di cena. La sveglia la prende a zampate
    /// (e russa più forte), la lampada gli fa solo coprire gli occhi con la coda, la piuma lo fa stiracchiare. Il pesce
    /// portato verso di lui col topo libero se lo ruba il topo, che sfreccia fuori dalla tana ("Finders keepers!").
    /// Soluzione: il formaggio davanti alla tana (il topo lo porta dentro e resta a mangiarlo), poi il pesce sotto il
    /// naso del gatto.
    /// </summary>
    public class V62WakeKittyLevel : V62Level
    {
        [SerializeField] Transform cat;              // gatto e cuscino (si gira cambiando il segno della scala x)
        [SerializeField] Transform catBody;          // per i sobbalzi
        [SerializeField] SpriteRenderer catArt;
        [SerializeField] Sprite catSleep;
        [SerializeField] Collider2D catArea;
        [SerializeField] Transform nose;
        [SerializeField] Transform tailCover;        // la coda che copre gli occhi quando la luce è accesa
        [SerializeField] Transform awakeCat;         // il gatto seduto e sveglio (nascosto finché non si sveglia)
        [SerializeField] SmokeEmitter zzz;
        [SerializeField] SmokeEmitter hearts;
        [SerializeField] Transform mouse;
        [SerializeField] Transform mouseHole;
        [SerializeField] Collider2D holeArea;
        [SerializeField] Transform fish;
        [SerializeField] Collider2D fishArea;
        [SerializeField] Transform cheese;
        [SerializeField] Collider2D cheeseArea;
        [SerializeField] Transform feather;
        [SerializeField] Collider2D featherArea;
        [SerializeField] Transform alarm;
        [SerializeField] Collider2D alarmArea;
        [SerializeField] SmokeEmitter alarmWaves;
        [SerializeField] Collider2D lampArea;
        [SerializeField] SpriteRenderer lampGlow;
        [SerializeField] SpriteRenderer lampLight;
        [SerializeField] SpeechBubble catBubble;
        [SerializeField] SpeechBubble mouseBubble;
        [SerializeField] TextMesh infoText;
        [SerializeField] Transform infoHolder;

        const float NoseRadius = 0.7f;
        const float HoleRadius = 0.85f;

        readonly WakeKittyLogic logic = new WakeKittyLogic();
        Camera cam;
        HwDragger drag;
        HwDragger.Item fishItem, cheeseItem, featherItem;
        HwSay catSay, mouseSay;
        HwToast toast;
        Vector3 alarmHome, catScale, mouseHome, mouseScale;
        Coroutine peek;
        float zzzRate, nextPeek, tapReadyAt;
        bool busy, alarmAway;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            logic.Reset();
            ResetV62();
            catSay = new HwSay(catBubble);
            mouseSay = new HwSay(mouseBubble);
            toast = new HwToast(infoText, infoHolder);
            drag = new HwDragger(this);
            alarmHome = alarm.position;
            catScale = cat.localScale;
            mouseHome = mouse.position;
            mouseScale = mouse.localScale;
            fishItem = drag.Add(fish, fishArea, 5);
            cheeseItem = drag.Add(cheese, cheeseArea, 5);
            featherItem = drag.Add(feather, featherArea, 6);
            catArt.sprite = catSleep;
            HalloweenKit.Show(awakeCat, false);
            HalloweenKit.Show(tailCover, false);
            HalloweenKit.Show(mouse, false);   // il topo sta nella tana: si vede solo quando esce
            HalloweenKit.SetAlpha(lampGlow, 0f);
            HalloweenKit.SetAlpha(lampLight, 0f);
            if (zzz != null) { zzzRate = zzz.puffsPerSecond; zzz.Play(); }
            if (hearts != null) hearts.Stop();
            if (alarmWaves != null) alarmWaves.Stop();
            nextPeek = Time.time + 3f;
        }

        void Update()
        {
            catSay.Tick();
            mouseSay.Tick();
            toast.Tick();
            if (IsFinished) return;
            PeekNow();
            if (busy || logic.InputLocked) return;
            if (!HalloweenKit.ReadPointer(cam, out Vector2 w, out bool began, out bool ended))
            {
                if (drag.Held != null) Drop(drag.Held, w, true);
                return;
            }
            if (began)
            {
                HwDragger.Item it = drag.TryPick(w);
                if (it == fishItem) logic.TryPickFish();
                if (it == null) Tap(w);
            }
            if (drag.Held == null) return;
            drag.MoveHeld(w);
            if (drag.Held == fishItem && Vector2.Distance(fish.position, nose.position) < NoseRadius)
            {
                FishAtCat();
                return;
            }
            if (ended && drag.Held != null) Drop(drag.Held, w, false);
        }

        /// <summary>Ogni tanto il topo (libero) mette fuori il muso dalla tana: così il giocatore sa che c'è.</summary>
        void PeekNow()
        {
            if (Time.time < nextPeek || busy || peek != null) return;
            nextPeek = Time.time + Random.Range(6f, 9f);
            if (logic.MouseBusy) { mouseSay.Show("*nom nom nom*", 1.4f); return; }
            peek = StartCoroutine(Peek(logic.Steals > 0 ? "Got any more fish?" : "Squeak? Is that fish for me?"));
        }

        IEnumerator Peek(string line)
        {
            mouse.position = mouseHome;
            HalloweenKit.Show(mouse, true);
            Sfx.Play(this, "pop", 0.3f);
            yield return HalloweenKit.Pop(mouse, 1.15f, 0.25f);
            mouseSay.Show(line, 1.6f);
            yield return new WaitForSeconds(1.5f);
            HalloweenKit.Show(mouse, false);
            peek = null;
        }

        /// <summary>Interrompe il "musetto fuori" (il topo deve fare altro).</summary>
        void StopPeek()
        {
            if (peek != null) StopCoroutine(peek);
            peek = null;
            mouse.localScale = mouseScale;
        }

        void Tap(Vector2 w)
        {
            if (alarmArea.OverlapPoint(w) && !alarmAway)
            {
                if (logic.TapAlarm() == KittyReaction.SlapsAlarm) StartCoroutine(AlarmRings());
                return;
            }
            if (lampArea.OverlapPoint(w))
            {
                KittyReaction r = logic.TapLamp();
                Sfx.Play(this, "tick", 0.7f);
                StartCoroutine(HalloweenKit.Fade(lampGlow, logic.LampOn ? 0.75f : 0f, 0.2f));
                StartCoroutine(HalloweenKit.Fade(lampLight, logic.LampOn ? 0.35f : 0f, 0.2f));
                if (r == KittyReaction.HidesFromLight) StartCoroutine(HideFromLight());
                else HalloweenKit.Show(tailCover, false);
                return;
            }
            if (catArea.OverlapPoint(w))
            {
                if (Time.time < tapReadyAt) return;
                tapReadyAt = Time.time + 0.4f;
                Sfx.Play(this, "snore", 0.6f);
                StartCoroutine(HalloweenKit.Shake(catBody, 0.03f, 0.3f));
                catSay.Show(logic.Snore >= 3 ? "ZZZZZZ... RRRRONK!" : "Purrr... zzz... five more naps...", 1.8f);
                return;
            }
            if (holeArea.OverlapPoint(w))
            {
                Sfx.Play(this, "pop", 0.4f);
                mouseSay.Show(logic.MouseBusy ? "Busy. Cheese. Go away." : "Squeak! Nobody home!", 1.6f);
            }
        }

        void Drop(HwDragger.Item it, Vector2 w, bool lostFinger)
        {
            drag.Release();
            bool onCat = !lostFinger && (catArea.OverlapPoint(it.T.position) || catArea.OverlapPoint(w));
            if (it == fishItem)
            {
                if (onCat) { FishAtCat(); return; }
                logic.DropFishElsewhere();
                drag.SendHome(fishItem, 0.35f);
                return;
            }
            if (it == cheeseItem)
            {
                bool atHole = !lostFinger && Vector2.Distance(cheese.position, mouseHole.position) < HoleRadius;
                if (atHole && logic.TryGiveCheese() == KittyReaction.MouseTakesCheese) { StartCoroutine(MouseTakesCheese()); return; }
                if (onCat && logic.CheeseOnCat() == KittyReaction.IgnoresCheese)
                {
                    Sfx.Play(this, "snore", 0.5f);
                    catSay.Show("Cheese? I'm a CAT. Zzz...", 1.8f);
                }
                drag.SendHome(cheeseItem, 0.35f);
                return;
            }
            if (it == featherItem)
            {
                if (onCat && logic.FeatherOnCat() == KittyReaction.Stretches) StartCoroutine(Stretch());
                drag.SendHome(featherItem, 0.35f);
            }
        }

        IEnumerator Stretch()
        {
            busy = true;
            catSay.Show("Mmmrrr... *streeetch*", 1.6f);
            Vector3 s = catBody.localScale;
            for (float t = 0f; t < 1f; t += Time.deltaTime / 0.8f)
            {
                float k = Mathf.Sin(t * Mathf.PI);
                catBody.localScale = new Vector3(s.x * (1f + 0.18f * k), s.y * (1f - 0.08f * k), s.z);
                yield return null;
            }
            catBody.localScale = s;
            catSay.Show("...zzz.", 1.2f);
            busy = false;
        }

        IEnumerator AlarmRings()
        {
            busy = true;
            alarmAway = true;
            Sfx.Play(this, "buzz", 0.6f);
            if (alarmWaves != null) alarmWaves.Play();
            StartCoroutine(HalloweenKit.Shake(alarm, 0.06f, 0.7f));
            yield return new WaitForSeconds(0.7f);
            if (alarmWaves != null) alarmWaves.Stop();
            // la zampata: la sveglia vola via (e torna al suo posto)
            Sfx.Play(this, "bonk", 0.7f);
            Haptics.Light();
            StartCoroutine(HalloweenKit.Shake(catBody, 0.05f, 0.3f));
            tapReadyAt = Time.time + 0.4f;
            catSay.Show("*SWAT* ...ZZZ... RRRONK!", 1.8f);
            if (zzz != null) zzz.puffsPerSecond = zzzRate * logic.Snore;
            busy = false;
            yield return HalloweenKit.Move(alarm, alarmHome + new Vector3(1.4f, 0.2f, 0f), 0.35f, 0.6f);
            yield return new WaitForSeconds(1.2f);
            yield return HalloweenKit.Move(alarm, alarmHome, 0.4f, 0.3f);
            alarmAway = false;
        }

        /// <summary>Luce accesa: la coda sugli occhi e una rigirata dall'altra parte.</summary>
        IEnumerator HideFromLight()
        {
            busy = true;
            catSay.Show("Mrrf. Too bright.", 1.6f);
            Sfx.Play(this, "snore", 0.5f);
            Vector3 from = cat.localScale;
            Vector3 to = new Vector3(logic.FacingLeft ? Mathf.Abs(catScale.x) : -Mathf.Abs(catScale.x), catScale.y, catScale.z);
            for (float t = 0f; t < 1f; t += Time.deltaTime / 0.35f)
            {
                cat.localScale = new Vector3(Mathf.Lerp(from.x, to.x, t), catScale.y * (1f - 0.08f * Mathf.Sin(t * Mathf.PI)), catScale.z);
                yield return null;
            }
            cat.localScale = to;
            HalloweenKit.Show(tailCover, true);
            busy = false;
        }

        void FishAtCat()
        {
            drag.Release();
            KittyReaction r = logic.FishAtCat();
            if (r == KittyReaction.MouseSteals) StartCoroutine(MouseSteals());
            else if (r == KittyReaction.Wakes) StartCoroutine(WakeUp());
            else drag.SendHome(fishItem, 0.3f);
        }

        IEnumerator MouseSteals()
        {
            busy = true;
            fishItem.Enabled = false;
            Vector3 grab = fish.position;
            // il topo sfreccia fuori dalla tana, prende il pesce e torna dentro
            StopPeek();
            HalloweenKit.Show(mouse, true);
            mouse.position = mouseHome;
            Sfx.Play(this, "whoosh", 0.8f);
            yield return HalloweenKit.Move(mouse, new Vector3(grab.x, grab.y - 0.15f, mouseHome.z), 0.25f, 0.5f);
            fish.SetParent(mouse, true);
            Sfx.Play(this, "pop", 0.7f);
            Haptics.Light();
            mouseSay.Show("Finders keepers!", 1.8f);
            yield return HalloweenKit.Move(mouse, mouseHome, 0.3f, 0.4f);
            HalloweenKit.Show(mouse, false);
            fish.SetParent(transform, true);
            fish.gameObject.SetActive(false);
            catSay.Show("...zzz... fish?... zzz", 1.4f);
            busy = false;
            yield return new WaitForSeconds(2f);
            fish.position = fishItem.Home;
            fish.gameObject.SetActive(true);
            logic.TryRespawnFish();
            fishItem.Enabled = true;
            Sfx.Play(this, "pop", 0.5f);
            yield return HalloweenKit.Pop(fish, 1.3f, 0.3f);
        }

        IEnumerator MouseTakesCheese()
        {
            busy = true;
            cheeseItem.Enabled = false;
            yield return HalloweenKit.Move(cheese, mouseHole.position + new Vector3(-0.5f, -0.25f, 0f), 0.15f);
            StopPeek();
            mouse.position = mouseHome;
            HalloweenKit.Show(mouse, true);
            yield return HalloweenKit.Pop(mouse, 1.15f, 0.25f);
            mouseSay.Show("CHEESE?! Do not disturb!", 2f);
            Sfx.Play(this, "pop", 0.7f);
            Haptics.Light();
            yield return HalloweenKit.Move(mouse, cheese.position, 0.2f);
            cheese.SetParent(mouse, true);
            yield return HalloweenKit.Move(mouse, mouseHome, 0.3f);
            HalloweenKit.Show(mouse, false);
            cheese.gameObject.SetActive(false);
            toast.Show("The mouse is busy now.", 1.8f);
            nextPeek = Time.time + 2.5f;
            busy = false;
        }

        IEnumerator WakeUp()
        {
            busy = true;
            fishItem.Enabled = false;
            featherItem.Enabled = false;
            yield return HalloweenKit.Move(fish, nose.position + new Vector3(0f, -0.15f, 0f), 0.2f);
            if (zzz != null) zzz.Stop();
            HalloweenKit.Show(tailCover, false);
            // annusa...
            catSay.Show("*sniff* *sniff*", 1.4f);
            for (int i = 0; i < 3; i++)
            {
                Sfx.Play(this, "tick", 0.6f);
                yield return HalloweenKit.Pop(catBody, 1.04f, 0.18f);
            }
            yield return new WaitForSeconds(0.3f);
            // ...apre gli occhi e salta su contento
            HalloweenKit.Show(catBody, false);
            HalloweenKit.Show(awakeCat, true);
            fish.gameObject.SetActive(false);
            Sfx.Play(this, "pop", 0.9f);
            Haptics.Medium();
            yield return HalloweenKit.Move(awakeCat, awakeCat.position + Vector3.up * 0.01f, 0.35f, 0.8f);
            catSay.Show("FISH! Dinner is served!", 2.2f);
            if (hearts != null) hearts.Play();
            yield return Celebrate(2.2f);
            if (hearts != null) hearts.Stop();
            if (logic.TryCompleteWake()) Win();
        }
    }
}
