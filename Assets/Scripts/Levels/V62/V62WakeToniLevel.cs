using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels.V62;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 252 "Wake up, Toni!" — Toni dorme sul suo cuscino ed è ora della passeggiata. La sveglia la prende a
    /// zampate (e russa più forte), la lampada lo fa solo girare dall'altra parte, il biscotto portato verso di lui
    /// col gatto libero se lo ruba il gatto ("Mine."). Soluzione: la piuma al gatto (ci gioca e resta occupato), poi
    /// il biscotto sotto il naso di Toni. Easter egg: la piuma strofinata su Toni 3 volte lo fa starnutire nel sonno.
    /// </summary>
    public class V62WakeToniLevel : V62Level
    {
        [SerializeField] Transform toni;             // Toni e il cuscino (si gira cambiando il segno della scala x)
        [SerializeField] Transform toniBody;         // solo Toni (per i sobbalzi)
        [SerializeField] SpriteRenderer toniArt;
        [SerializeField] Sprite toniLie;
        [SerializeField] Sprite toniStand;
        [SerializeField] Collider2D toniArea;
        [SerializeField] Transform nose;
        [SerializeField] GameObject eyesHalf;
        [SerializeField] GameObject eyesOpen;
        [SerializeField] Transform tail;
        [SerializeField] SmokeEmitter zzz;
        [SerializeField] SmokeEmitter sneezePuff;
        [SerializeField] SmokeEmitter hearts;
        [SerializeField] Transform cat;
        [SerializeField] Collider2D catArea;
        [SerializeField] Transform catPlaySpot;
        [SerializeField] Transform cookie;
        [SerializeField] Collider2D cookieArea;
        [SerializeField] Transform feather;
        [SerializeField] Collider2D featherArea;
        [SerializeField] Transform alarm;
        [SerializeField] Collider2D alarmArea;
        [SerializeField] SmokeEmitter alarmWaves;
        [SerializeField] Collider2D lampArea;
        [SerializeField] SpriteRenderer lampGlow;
        [SerializeField] SpriteRenderer lampLight;
        [SerializeField] SpeechBubble toniBubble;
        [SerializeField] SpeechBubble catBubble;
        [SerializeField] TextMesh infoText;
        [SerializeField] Transform infoHolder;

        const float NoseRadius = 0.65f;
        const float RubStep = 0.18f;   // spostamento minimo della piuma per contare un cambio di direzione

        readonly WakeToniLogic logic = new WakeToniLogic();
        Camera cam;
        HwDragger drag;
        HwDragger.Item cookieItem, featherItem;
        HwSay toniSay, catSay;
        HwToast toast;
        Vector3 catHome, alarmHome, toniScale;
        Coroutine catPlay;
        float zzzRate, rubAnchor;
        int rubDir;
        bool busy, alarmAway, eggShown, overToni;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            logic.Reset();
            ResetV62();
            toniSay = new HwSay(toniBubble);
            catSay = new HwSay(catBubble);
            toast = new HwToast(infoText, infoHolder);
            drag = new HwDragger(this);
            catHome = cat.position;
            alarmHome = alarm.position;
            toniScale = toni.localScale;
            cookieItem = drag.Add(cookie, cookieArea, 5);
            featherItem = drag.Add(feather, featherArea, 6);
            toniArt.sprite = toniLie;
            HalloweenKit.Show(eyesHalf, false);
            HalloweenKit.Show(eyesOpen, false);
            if (tail != null) tail.gameObject.SetActive(false);   // la codina si vede solo in piedi
            HalloweenKit.SetAlpha(lampGlow, 0f);
            HalloweenKit.SetAlpha(lampLight, 0f);
            if (zzz != null) { zzzRate = zzz.puffsPerSecond; zzz.Play(); }
            if (sneezePuff != null) sneezePuff.Stop();
            if (hearts != null) hearts.Stop();
            if (alarmWaves != null) alarmWaves.Stop();
        }

        void Update()
        {
            toniSay.Tick();
            catSay.Tick();
            toast.Tick();
            TickV62();
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
            if (drag.Held == cookieItem && Vector2.Distance(cookie.position, nose.position) < NoseRadius)
            {
                CookieAtToni();
                return;
            }
            if (drag.Held == featherItem) TrackRub();
            if (ended && drag.Held != null) Drop(drag.Held, w, false);
        }

        void OnPick(HwDragger.Item it)
        {
            if (it == cookieItem) logic.TryPickCookie();
            if (it == featherItem)
            {
                if (logic.TryTakeFeatherFromCat()) StopCatPlay();
                feather.localRotation = Quaternion.identity;
                rubDir = 0;
                overToni = false;
            }
        }

        void Tap(Vector2 w)
        {
            if (alarmArea.OverlapPoint(w) && !alarmAway)
            {
                if (logic.TapAlarm() == WakeReaction.SlapsAlarm) StartCoroutine(AlarmRings());
                return;
            }
            if (lampArea.OverlapPoint(w))
            {
                WakeReaction r = logic.TapLamp();
                Sfx.Play(this, "tick", 0.7f);
                StartCoroutine(HalloweenKit.Fade(lampGlow, logic.LampOn ? 0.75f : 0f, 0.2f));
                StartCoroutine(HalloweenKit.Fade(lampLight, logic.LampOn ? 0.35f : 0f, 0.2f));
                if (r == WakeReaction.TurnsOver) StartCoroutine(TurnOver());
                return;
            }
            if (toniArea.OverlapPoint(w))
            {
                Sfx.Play(this, "snore", 0.6f);
                StartCoroutine(HalloweenKit.Shake(toniBody, 0.03f, 0.3f));
                toniSay.Show(logic.Snore >= 3 ? "ZZZZZZ... SNOOORK!" : "Zzz... five more minutes...", 1.8f);
                return;
            }
            if (catArea.OverlapPoint(w))
            {
                Sfx.Play(this, "pop", 0.4f);
                catSay.Show(logic.CatBusy ? "Busy. Feather." : "Mrrr? I'm watching that cookie.", 1.8f);
            }
        }

        void Drop(HwDragger.Item it, Vector2 w, bool lostFinger)
        {
            drag.Release();
            if (it == cookieItem)
            {
                logic.DropCookieElsewhere();
                drag.SendHome(cookieItem, 0.35f);
                return;
            }
            if (it == featherItem)
            {
                logic.EndRub();
                overToni = false;
                if (!lostFinger && (catArea.OverlapPoint(feather.position) || catArea.OverlapPoint(w)) && logic.TryGiveFeatherToCat())
                {
                    // la piuma ora è del gatto: la muove lui (vedi CatPlays)
                    catPlay = StartCoroutine(CatPlays());
                    Sfx.Play(this, "pop", 0.6f);
                    catSay.Show("A FEATHER! Must... catch...", 2f);
                    return;
                }
                drag.SendHome(featherItem, 0.35f);
            }
        }

        /// <summary>Strofinate della piuma su Toni: conta i cambi di direzione mentre la punta è sopra di lui.</summary>
        void TrackRub()
        {
            Vector2 tip = feather.position;
            if (!toniArea.OverlapPoint(tip))
            {
                if (overToni) logic.EndRub();
                overToni = false;
                rubDir = 0;
                return;
            }
            if (!overToni) { overToni = true; rubAnchor = tip.x; rubDir = 0; }
            float dx = tip.x - rubAnchor;
            if (rubDir == 0)
            {
                if (Mathf.Abs(dx) >= RubStep) { rubDir = dx > 0f ? 1 : -1; rubAnchor = tip.x; }
                return;
            }
            if (Mathf.Sign(dx) == rubDir) { rubAnchor = tip.x; return; }
            if (Mathf.Abs(dx) < RubStep) return;
            rubDir = -rubDir;
            rubAnchor = tip.x;
            Sfx.Play(this, "tick", 0.5f);
            StartCoroutine(HalloweenKit.Shake(toniBody, 0.02f, 0.15f));
            if (logic.TryRub()) StartCoroutine(Sneeze());
        }

        IEnumerator Sneeze()
        {
            toniSay.Show("Ah... ah... ACHOO!", 1.6f);
            yield return new WaitForSeconds(0.35f);
            Sfx.Play(this, "whoosh", 0.8f);
            Haptics.Light();
            if (sneezePuff != null) StartCoroutine(HalloweenKit.Burst(sneezePuff, 0.35f));
            yield return HalloweenKit.Pop(toniBody, 1.12f, 0.25f);
            if (!eggShown) { eggShown = true; RevealEgg(WakeToniLogic.EggId); }
            yield return new WaitForSeconds(0.8f);
            toniSay.Show("...zzz...", 1.4f);
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
            // la zampata: la sveglia vola via
            Sfx.Play(this, "bonk", 0.7f);
            Haptics.Light();
            StartCoroutine(HalloweenKit.Shake(toniBody, 0.05f, 0.3f));
            toniSay.Show("*SLAP* ...ZZZ... SNOOORE!", 1.8f);
            if (zzz != null) zzz.puffsPerSecond = zzzRate * logic.Snore;
            busy = false;
            yield return HalloweenKit.Move(alarm, alarmHome + new Vector3(-1.6f, -0.3f, 0f), 0.35f, 0.6f);
            yield return new WaitForSeconds(1.2f);
            yield return HalloweenKit.Move(alarm, alarmHome, 0.4f, 0.3f);
            alarmAway = false;
        }

        IEnumerator TurnOver()
        {
            busy = true;
            toniSay.Show("Mmmph. Too bright.", 1.6f);
            Sfx.Play(this, "snore", 0.5f);
            Vector3 from = toni.localScale;
            Vector3 to = new Vector3(logic.FacingLeft ? Mathf.Abs(toniScale.x) : -Mathf.Abs(toniScale.x), toniScale.y, toniScale.z);
            for (float t = 0f; t < 1f; t += Time.deltaTime / 0.35f)
            {
                toni.localScale = new Vector3(Mathf.Lerp(from.x, to.x, t), toniScale.y * (1f - 0.08f * Mathf.Sin(t * Mathf.PI)), toniScale.z);
                yield return null;
            }
            toni.localScale = to;
            busy = false;
        }

        IEnumerator CatPlays()
        {
            for (float t = 0f; ; t += Time.deltaTime)
            {
                cat.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * 9f) * 9f);
                if (drag.Held != featherItem)
                    feather.position = catPlaySpot.position + new Vector3(Mathf.Sin(t * 7f) * 0.12f, Mathf.Abs(Mathf.Sin(t * 5f)) * 0.15f, 0f);
                yield return null;
            }
        }

        void StopCatPlay()
        {
            if (catPlay != null) StopCoroutine(catPlay);
            catPlay = null;
            cat.localRotation = Quaternion.identity;
            catSay.Show("Hey! My feather!", 1.4f);
        }

        void CookieAtToni()
        {
            drag.Release();
            WakeReaction r = logic.CookieAtToni();
            if (r == WakeReaction.CatSteals) StartCoroutine(CatSteals());
            else if (r == WakeReaction.Wakes) StartCoroutine(WakeUp());
            else drag.SendHome(cookieItem, 0.3f);
        }

        IEnumerator CatSteals()
        {
            busy = true;
            cookieItem.Enabled = false;
            Sfx.Play(this, "whoosh", 0.7f);
            Vector3 grab = cookie.position;
            yield return HalloweenKit.Move(cat, new Vector3(grab.x, grab.y - 0.3f, catHome.z), 0.3f, 0.8f);
            cookie.gameObject.SetActive(false);
            Sfx.Play(this, "pop", 0.7f);
            Haptics.Light();
            catSay.Show("Mine.", 1.8f);
            yield return HalloweenKit.Move(cat, catHome, 0.4f, 0.8f);
            busy = false;
            yield return new WaitForSeconds(2f);
            cookie.position = cookieItem.Home;
            cookie.gameObject.SetActive(true);
            logic.TryRespawnCookie();
            cookieItem.Enabled = true;
            Sfx.Play(this, "pop", 0.5f);
            yield return HalloweenKit.Pop(cookie, 1.3f, 0.3f);
        }

        IEnumerator WakeUp()
        {
            busy = true;
            cookieItem.Enabled = false;
            featherItem.Enabled = false;
            yield return HalloweenKit.Move(cookie, nose.position + new Vector3(0f, -0.25f, 0f), 0.2f);
            if (zzz != null) zzz.Stop();
            // annusa...
            toniSay.Show("*sniff* *sniff*", 1.4f);
            for (int i = 0; i < 3; i++)
            {
                Sfx.Play(this, "tick", 0.6f);
                yield return HalloweenKit.Pop(toniBody, 1.04f, 0.18f);
            }
            // ...apre un occhio...
            HalloweenKit.Show(eyesHalf, true);
            yield return new WaitForSeconds(0.7f);
            // ...e salta su felice
            HalloweenKit.Show(eyesHalf, false);
            HalloweenKit.Show(eyesOpen, true);
            toniArt.sprite = toniStand;
            if (tail != null) tail.gameObject.SetActive(true);
            cookie.gameObject.SetActive(false);
            Sfx.Play(this, "woof", 0.8f);
            Haptics.Medium();
            yield return HalloweenKit.Move(toniBody, toniBody.position + Vector3.up * 0.01f, 0.35f, 0.7f);
            toniSay.Show("WOOF! Cookie AND a walk?!", 2.2f);
            if (hearts != null) hearts.Play();
            StartCoroutine(Celebrate(2.2f));
            for (float t = 0f; t < 2.2f; t += Time.deltaTime)
            {
                if (tail != null) tail.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * 22f) * 25f);
                yield return null;
            }
            if (hearts != null) hearts.Stop();
            if (logic.TryCompleteWake()) Victory(logic.EggFound, WakeToniLogic.EggId);
        }
    }
}
