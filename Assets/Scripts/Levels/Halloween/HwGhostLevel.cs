using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels.Halloween;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 247 "The ghost who can't scare" — un fantasma sotto un lenzuolo bianco prova a spaventare l'uomo che dorme:
    /// il "BOO!" non funziona, la sveglia lo sveglia ma un tizio col lenzuolo non fa paura, la lampada si accende e si
    /// spegne. Soluzione: trascinare via il lenzuolo (sotto non c'è niente: il fantasma resta invisibile) e metterlo
    /// sull'uomo, che si sveglia, si vede fantasma nello specchio e si spaventa a morte. Funziona con sveglia e lampada in
    /// qualsiasi stato.
    /// </summary>
    public class HwGhostLevel : LevelController
    {
        [SerializeField] Transform ghost;
        [SerializeField] GameObject ghostSheetArt;
        [SerializeField] SpriteRenderer ghostInvisible;
        [SerializeField] Collider2D ghostArea;
        [SerializeField] Transform sheet;
        [SerializeField] Collider2D sheetArea;
        [SerializeField] SpriteRenderer sheetArt;
        [SerializeField] Transform sheetFloorSpot;
        [SerializeField] Transform man;
        [SerializeField] SpriteRenderer manArt;
        [SerializeField] Sprite manSleep, manAwake, manGhost;
        [SerializeField] Collider2D manArea;
        [SerializeField] SmokeEmitter zzz;
        [SerializeField] SpriteRenderer mirrorReflection;
        [SerializeField] Transform alarm;
        [SerializeField] Collider2D alarmArea;
        [SerializeField] SmokeEmitter alarmWaves;
        [SerializeField] Collider2D lampArea;
        [SerializeField] SpriteRenderer lampGlow;
        [SerializeField] SpriteRenderer roomDark;
        [SerializeField] SpeechBubble ghostBubble;
        [SerializeField] SpeechBubble manBubble;
        [SerializeField] SmokeEmitter[] confetti;
        [SerializeField] float pullDistance = 0.3f;

        readonly GhostLogic logic = new GhostLogic();
        Camera cam;
        HwDragger drag;
        HwDragger.Item sheetItem;
        HwSay ghostSay, manSay;
        Vector3 ghostHome;
        Vector2 pickPoint;
        bool pulled, busy;
        float sleepAt = -1f, bob;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            logic.Reset();
            drag = new HwDragger(this);
            sheetItem = drag.Add(sheet, sheetArea, 5);
            sheetItem.Lift = 1f;
            ghostSay = new HwSay(ghostBubble);
            manSay = new HwSay(manBubble);
            ghostHome = ghost.position;
            sheet.position = ghost.position;
            ShowCovered(true);
            manArt.sprite = manSleep;
            if (zzz != null) zzz.Play();
            if (alarmWaves != null) alarmWaves.Stop();
            HalloweenKit.SetAlpha(mirrorReflection, 0f);
            HalloweenKit.SetAlpha(roomDark, 0f);
            lampGlow.enabled = true;
            HalloweenKit.Confetti(confetti, false);
            ghostSay.Show("Tonight I'll scare him! Hee hee!", 2.5f);
        }

        void ShowCovered(bool covered)
        {
            ghostSheetArt.SetActive(covered);
            ghostInvisible.gameObject.SetActive(!covered);
            sheetArt.enabled = !covered;
        }

        void Update()
        {
            ghostSay.Tick();
            manSay.Tick();
            // il fantasma fluttua
            bob += Time.deltaTime * (logic.State == GhostState.Done || logic.State == GhostState.Ending ? 6f : 2.2f);
            ghost.position = ghostHome + new Vector3(0f, Mathf.Sin(bob) * 0.12f, 0f);
            if (logic.State == GhostState.Covered && drag.Held == null) sheet.position = ghost.position;
            if (sleepAt > 0f && Time.time >= sleepAt) FallAsleep();

            if (IsFinished || busy || logic.InputLocked) return;
            if (!HalloweenKit.ReadPointer(cam, out Vector2 w, out bool began, out bool ended))
            {
                if (drag.Held != null) Drop(sheet.position, false);
                return;
            }
            if (began)
            {
                if (drag.TryPick(w) != null) { pickPoint = w; pulled = logic.State != GhostState.Covered; if (pulled) logic.TryPickSheet(); }
                else Tap(w);
            }
            if (drag.Held == null) return;
            if (!pulled)
            {
                // finché il dito non si sposta è un tocco (BOO!), non uno strappo
                if (Vector2.Distance(w, pickPoint) < pullDistance) { if (ended) Drop(w, true); return; }
                pulled = true;
                logic.TryPickSheet();
                ShowCovered(false);
                Sfx.Play(this, "whoosh", 0.6f);
                Haptics.Light();
                ghostSay.Show("Hey! My sheet! ...wait, where am I?", 2.2f);
            }
            drag.MoveHeld(w);
            if (ended) Drop(w, false);
        }

        void Drop(Vector2 w, bool wasTap)
        {
            drag.Release();
            if (wasTap || !pulled)
            {
                sheet.position = ghost.position;
                Boo();
                return;
            }
            GhostDropTarget target = manArea.OverlapPoint(w) || manArea.OverlapPoint(sheet.position) ? GhostDropTarget.Man
                : Vector2.Distance(sheet.position, ghost.position) < 0.9f ? GhostDropTarget.Ghost : GhostDropTarget.Nowhere;
            if (logic.TryDropSheet(target)) { StartCoroutine(Ending()); return; }
            if (logic.State == GhostState.Covered)
            {
                ShowCovered(true);
                sheet.position = ghost.position;
                Sfx.Play(this, "pop", 0.4f);
                ghostSay.Show("Phew, dressed again.", 1.6f);
                return;
            }
            // lasciato altrove: cade per terra, sempre a portata di mano
            sheetItem.Home = sheetFloorSpot.position;
            drag.SendHome(sheetItem, 0.35f);
            Sfx.Play(this, "whoosh", 0.3f);
        }

        void Tap(Vector2 w)
        {
            if (alarmArea.OverlapPoint(w)) { RingAlarm(); return; }
            if (lampArea.OverlapPoint(w)) { ToggleLamp(); return; }
            if (ghostArea.OverlapPoint(w) && logic.GhostInvisible)
                ghostSay.Show("Hee hee... nobody can see me now!", 1.8f);
        }

        void Boo()
        {
            if (!logic.TryBoo()) return;
            Sfx.Play(this, "whoosh", 0.7f);
            Haptics.Light();
            StartCoroutine(HalloweenKit.Pop(ghost, 1.15f, 0.25f));
            ghostSay.Show(logic.Boos % 2 == 1 ? "BOOOO!" : "BOO! BOO!! ...boo?", 1.4f);
            if (logic.ManAwake) manSay.Show(logic.Boos % 2 == 1 ? "Nice bedsheet, pal." : "Is that my laundry?", 1.8f);
            else manSay.Show("Zzz... five more minutes... zzz", 1.6f);
        }

        void RingAlarm()
        {
            if (!logic.TryRingAlarm()) return;
            StartCoroutine(HalloweenKit.Shake(alarm, 0.06f, 0.6f));
            if (alarmWaves != null) StartCoroutine(HalloweenKit.Burst(alarmWaves, 0.6f));
            Sfx.Play(this, "ding", 0.7f);
            Haptics.Medium();
            manArt.sprite = manAwake;
            if (zzz != null) zzz.Stop();
            manSay.Show(logic.State == GhostState.Covered ? "Huh? Oh. A guy in a sheet. Goodnight." : "Who's there? ...nobody. Goodnight.", 2.4f);
            sleepAt = Time.time + 3.2f;
        }

        void FallAsleep()
        {
            sleepAt = -1f;
            if (!logic.TryManSleeps()) return;
            manArt.sprite = manSleep;
            if (zzz != null) zzz.Play();
        }

        void ToggleLamp()
        {
            if (!logic.TryToggleLamp()) return;
            Sfx.Play(this, "tick", 0.6f);
            lampGlow.enabled = logic.LampOn;
            StartCoroutine(HalloweenKit.Fade(roomDark, logic.LampOn ? 0f : 0.45f, 0.2f));
            if (logic.State == GhostState.Covered) ghostSay.Show(logic.LampOn ? "Lights on! BOO?" : "Spooky dark! BOO!", 1.4f);
        }

        IEnumerator Ending()
        {
            busy = true;
            sleepAt = -1f;
            // il lenzuolo vola sull'uomo
            yield return HalloweenKit.Move(sheet, man.position + new Vector3(0f, 0.3f, 0f), 0.3f, 0.3f);
            sheetArt.enabled = false;
            manArt.sprite = manGhost;
            if (zzz != null) zzz.Stop();
            Sfx.Play(this, "whoosh", 0.6f);
            Haptics.Light();
            yield return HalloweenKit.Pop(man, 1.1f, 0.25f);
            manSay.Show(logic.ManAwake ? "Hey! Who turned off the... huh?" : "Mmh...? What's going on?", 1.8f);
            yield return new WaitForSeconds(1.6f);
            // si guarda allo specchio
            manSay.Hide();
            yield return HalloweenKit.Fade(mirrorReflection, 1f, 0.5f);
            yield return new WaitForSeconds(0.4f);
            manSay.Show("A... A GHOST!!! AAAAAAH!", 2.2f);
            Sfx.Play(this, "buzz", 0.8f);
            Haptics.Heavy();
            StartCoroutine(HalloweenKit.Shake(mirrorReflection.transform, 0.05f, 1.2f));
            yield return HalloweenKit.Shake(man, 0.12f, 1.3f);
            // il fantasma vero festeggia
            ghostSay.Show("BOO-YAH! Best scare EVER!", 2.4f);
            HalloweenKit.Confetti(confetti, true);
            Sfx.Play(this, "ding", 0.8f);
            Haptics.Success();
            yield return HalloweenKit.Wiggle(ghost, 12f, 1.6f, 10f);
            HalloweenKit.Confetti(confetti, false);
            if (logic.TryCompleteEnding()) Win();
        }
    }
}
