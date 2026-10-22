using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels.Halloween;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 250 "Trick or treat!" — cinque mostri (Toni lupo-carlino, zombie, strega, fantasma, zucca) alla porta di
    /// una vecchietta: "UNA CARAMELLA A BAMBINO". Il campanello dà UN cioccolatino, poi "Vi ho già visti!".
    /// Soluzione: un unico mostro travestito — lo zombie fa da corpo davanti alla porta, Toni si nasconde sotto i suoi
    /// vestiti e la zucca diventa la testa (in qualsiasi ordine), il fantasma avvolge tutti, la strega completa con
    /// cappello e incantesimo; al campanello la signora scappa e rovescia TUTTE le caramelle.
    /// Easter egg "true_monster_pug": staccare il braccio dello zombie, darlo a Toni, portare Toni davanti alla porta e
    /// suonare il campanello con la mano dello zombie: "CHE CARLINO ADORABILE!"... poi vede gli altri e scappa.
    /// </summary>
    public class HwTrickOrTreatLevel : LevelController
    {
        const string EggId = "true_monster_pug";

        [SerializeField] Transform zombie;
        [SerializeField] Collider2D zombieArea, zombieBodyArea;
        [SerializeField] GameObject zombieHead;
        [SerializeField] Transform zombieArm;          // il braccio non è figlio di nessuno: segue la spalla o il muso di Toni
        [SerializeField] Transform armAnchor;          // spalla dello zombie
        [SerializeField] Collider2D armArea;
        [SerializeField] Transform headSpot;
        [SerializeField] GameObject bigSheet, witchHat, tailPeek;
        [SerializeField] SmokeEmitter spell;
        [SerializeField] Transform toni;
        [SerializeField] Collider2D toniArea;
        [SerializeField] Transform toniMouth;
        [SerializeField] GameObject crown;
        [SerializeField] SmokeEmitter toniHearts;
        [SerializeField] Transform pumpkinKid;
        [SerializeField] Collider2D pumpkinArea;
        [SerializeField] Transform ghost;
        [SerializeField] Transform ghostArt;
        [SerializeField] Collider2D ghostArea;
        [SerializeField] Transform witch;
        [SerializeField] Collider2D witchArea;
        [SerializeField] GameObject doorClosed, doorOpen;
        [SerializeField] SpriteRenderer lady;
        [SerializeField] Sprite ladyNormal, ladyScared, ladyLove;
        [SerializeField] Transform bell;
        [SerializeField] Collider2D bellArea;
        [SerializeField] SmokeEmitter bellWaves;
        [SerializeField] Transform doorSpot, toniDoorSpot, armGroundSpot;
        [SerializeField] Transform chocolate;
        [SerializeField] Transform bucket;
        [SerializeField] Transform[] candies;
        [SerializeField] TextMesh candyText;
        [SerializeField] SpeechBubble gangBubble, ladyBubble;
        [SerializeField] GameObject eggBanner;
        [SerializeField] SmokeEmitter[] confetti;

        readonly TrickOrTreatLogic logic = new TrickOrTreatLogic();
        Camera cam;
        HwDragger drag;
        HwDragger.Item zombieItem, toniItem, pumpkinItem, ghostItem, witchItem, armItem;
        HwSay gang, ladySay;
        Vector3 armScale;
        ArmPlace armFrom;
        float ghostBob;
        bool busy, armMoving;
        Coroutine armRoutine;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            logic.Reset();
            drag = new HwDragger(this);
            zombieItem = drag.Add(zombie, zombieBodyArea, 1);
            ghostItem = drag.Add(ghost, ghostArea, 2);
            witchItem = drag.Add(witch, witchArea, 3);
            pumpkinItem = drag.Add(pumpkinKid, pumpkinArea, 4);
            toniItem = drag.Add(toni, toniArea, 5);
            armItem = drag.Add(zombieArm, armArea, 9);
            armItem.Lift = 1.1f;
            gang = new HwSay(gangBubble);
            ladySay = new HwSay(ladyBubble);
            armScale = zombieArm.localScale;
            armMoving = false;
            zombieHead.SetActive(true);
            bigSheet.SetActive(false);
            witchHat.SetActive(false);
            tailPeek.SetActive(false);
            crown.SetActive(false);
            eggBanner.SetActive(false);
            doorClosed.SetActive(true);
            doorOpen.SetActive(false);
            chocolate.gameObject.SetActive(false);
            bucket.gameObject.SetActive(false);
            foreach (Transform c in candies) c.gameObject.SetActive(false);
            if (spell != null) spell.Stop();
            if (toniHearts != null) toniHearts.Stop();
            if (bellWaves != null) bellWaves.Stop();
            HalloweenKit.Confetti(confetti, false);
            UpdateCandy(0);
            gang.Show("Trick or treat! Candy for all FIVE of us!", 2.6f);
        }

        void UpdateCandy(int n) { candyText.text = "CANDY: " + n + " / 5"; }

        void Update()
        {
            gang.Tick();
            ladySay.Tick();
            ghostBob += Time.deltaTime * 2f;
            ghostArt.localPosition = new Vector3(0f, Mathf.Sin(ghostBob) * 0.08f, 0f);
            armItem.Enabled = logic.CanGrabArm;
            zombieItem.Enabled = !logic.ZombieAtDoor;

            if (IsFinished || busy || logic.InputLocked) return;
            if (!HalloweenKit.ReadPointer(cam, out Vector2 w, out bool began, out bool ended))
            {
                if (drag.Held != null) Drop(drag.Release(), Vector2.zero, false);
                return;
            }
            if (began)
            {
                HwDragger.Item it = drag.TryPick(w);
                if (it == armItem) PickArm();
                else if (it == null && bellArea.OverlapPoint(w)) Ring(false);
            }
            if (drag.Held == null) return;
            drag.MoveHeld(w);
            if (ended) Drop(drag.Release(), w, true);
        }

        /// <summary>Il braccio segue il suo posto: la spalla dello zombie o il muso di Toni (per terra resta dov'è).</summary>
        void LateUpdate()
        {
            zombieArm.gameObject.SetActive(!(logic.Arm == ArmPlace.OnZombie && bigSheet.activeSelf));
            if (armMoving || drag == null || drag.Held == armItem) return;
            if (logic.Arm == ArmPlace.OnZombie) Place(armAnchor.position, armAnchor.rotation);
            else if (logic.Arm == ArmPlace.InToniMouth) Place(toniMouth.position, Quaternion.Euler(0f, 0f, -20f));
        }

        void Place(Vector3 p, Quaternion r)
        {
            zombieArm.position = new Vector3(p.x, p.y, zombieArm.position.z);
            zombieArm.rotation = r;
            armItem.Home = zombieArm.position;
        }

        void PickArm()
        {
            armFrom = logic.Arm;
            if (armRoutine != null) StopCoroutine(armRoutine);
            armMoving = false;
            Sfx.Play(this, armFrom == ArmPlace.OnZombie ? "pop" : "tick", 0.6f);
            if (armFrom == ArmPlace.OnZombie) gang.Show("Hey! That's MY arm!", 1.4f);
        }

        bool Hits(Collider2D area, Transform item, Vector2 finger, bool hasFinger) =>
            area != null && area.gameObject.activeInHierarchy && (area.OverlapPoint(item.position) || (hasFinger && area.OverlapPoint(finger)));

        void Drop(HwDragger.Item it, Vector2 finger, bool hasFinger)
        {
            if (it == null) return;
            if (it == armItem) { DropArm(finger, hasFinger); return; }
            if (it == zombieItem)
            {
                if (Vector2.Distance(zombie.position, doorSpot.position) < 1.4f && logic.TryZombieToDoor())
                {
                    it.Home = doorSpot.position;
                    drag.SendHome(it, 0.25f);
                    Sfx.Play(this, "clunk", 0.6f);
                    Haptics.Light();
                    gang.Show("BRAAAINS... I mean: trick or treat!", 1.8f);
                }
                else drag.SendHome(it);
                return;
            }
            bool onZombie = logic.ZombieAtDoor && Hits(zombieArea, it.T, finger, hasFinger);
            if (it == toniItem)
            {
                if (onZombie && logic.TryHideToni()) { StartCoroutine(Join(it, zombie.position + Vector3.up * 0.6f, () => tailPeek.SetActive(true), "*wiggle wiggle* Toni is in!")); return; }
                if (onZombie && logic.Arm != ArmPlace.OnZombie) gang.Show("Toni won't fit: the costume is missing an arm!", 2f);
                if (!onZombie && Vector2.Distance(toni.position, toniDoorSpot.position) < 1.0f)
                {
                    logic.TrySetToniAtDoor(true);
                    drag.SendTo(it, toniDoorSpot.position, 0.2f, 0f);
                    Sfx.Play(this, "woof", 0.6f);
                    return;
                }
                logic.TrySetToniAtDoor(false);
                drag.SendHome(it);
                return;
            }
            if (it == pumpkinItem)
            {
                if (onZombie && logic.TryPumpkinHead()) { StartCoroutine(PumpkinOnTop()); return; }
                drag.SendHome(it);
                return;
            }
            if (it == ghostItem)
            {
                if (onZombie && logic.TryGhostWrap()) { StartCoroutine(Join(it, zombie.position + Vector3.up * 1.2f, () => bigSheet.SetActive(true), "WHOOOSH! One big sheet for everyone!")); return; }
                if (onZombie) gang.Show("Boo? I can't wrap them yet!", 1.6f);
                drag.SendHome(it);
                return;
            }
            if (it == witchItem)
            {
                if (onZombie && logic.TryWitchSpell()) { StartCoroutine(WitchJoins()); return; }
                if (onZombie) gang.Show("Nothing to enchant yet, dearie!", 1.6f);
                drag.SendHome(it);
            }
        }

        // ---------- travestimento ----------

        IEnumerator Join(HwDragger.Item it, Vector3 into, System.Action show, string line)
        {
            busy = true;
            it.Enabled = false;
            StartCoroutine(HalloweenKit.ScaleTo(it.T, it.T.localScale * 0.3f, 0.25f));
            yield return HalloweenKit.Move(it.T, into, 0.25f);
            it.T.gameObject.SetActive(false);
            show();
            Sfx.Play(this, "whoosh", 0.6f);
            Haptics.Medium();
            gang.Show(line, 1.8f);
            yield return HalloweenKit.Wiggle(zombie, 6f, 0.5f, 25f);
            busy = false;
        }

        IEnumerator PumpkinOnTop()
        {
            busy = true;
            pumpkinItem.Enabled = false;
            yield return HalloweenKit.Move(pumpkinKid, headSpot.position, 0.3f, 0.5f);
            pumpkinKid.SetParent(zombie, true);
            zombieHead.SetActive(false);
            Sfx.Play(this, "pop", 0.7f);
            Haptics.Medium();
            gang.Show("New head! Much more stylish.", 1.8f);
            yield return HalloweenKit.Pop(zombie, 1.06f, 0.25f);
            busy = false;
        }

        IEnumerator WitchJoins()
        {
            busy = true;
            witchItem.Enabled = false;
            StartCoroutine(HalloweenKit.ScaleTo(witch, witch.localScale * 0.3f, 0.3f));
            yield return HalloweenKit.Move(witch, zombie.position + Vector3.up * 2.4f, 0.3f);
            witch.gameObject.SetActive(false);
            witchHat.SetActive(true);
            if (spell != null) spell.Play();
            Sfx.Play(this, "ding", 0.6f);
            Haptics.Heavy();
            gang.Show("Abracadabra... MONSTRO-FUSION!", 2f);
            yield return HalloweenKit.Pop(zombie, 1.12f, 0.4f);
            yield return new WaitForSeconds(0.6f);
            if (spell != null) spell.Stop();
            busy = false;
        }

        // ---------- braccio ----------

        void DropArm(Vector2 finger, bool hasFinger)
        {
            bool onBell = Hits(bellArea, zombieArm, finger, hasFinger);
            bool onToni = toni.gameObject.activeSelf && Hits(toniArea, zombieArm, finger, hasFinger);
            bool onZombie = Hits(zombieArea, zombieArm, finger, hasFinger) && !onToni;
            if (onBell)
            {
                if (armFrom == ArmPlace.OnZombie) logic.TryDetachArm();
                Ring(true);
                return;
            }
            if (onZombie)
            {
                logic.TryAttachArm();
                MoveArm(armAnchor.position, armAnchor.rotation);
                if (armFrom != ArmPlace.OnZombie) gang.Show("Ahh, much better. My arm!", 1.4f);
                return;
            }
            if (onToni)
            {
                if (armFrom == ArmPlace.OnZombie) logic.TryDetachArm();
                if (logic.TryGiveArmToToni() || armFrom == ArmPlace.InToniMouth) { ArmToMouth(); if (armFrom != ArmPlace.InToniMouth) gang.Show("Woof! *chomp* New chew toy!", 1.6f); return; }
            }
            if (armFrom == ArmPlace.InToniMouth) { ArmToMouth(); return; }
            if (armFrom == ArmPlace.OnZombie) logic.TryDetachArm();
            ArmToGround();
        }

        void ArmToMouth()
        {
            MoveArm(toniMouth.position, Quaternion.Euler(0f, 0f, -20f));
            Sfx.Play(this, "woof", 0.5f);
        }

        void ArmToGround()
        {
            Vector3 p = HalloweenKit.ClampSafe(zombieArm.position);
            MoveArm(new Vector3(p.x, Mathf.Min(p.y, armGroundSpot.position.y), zombieArm.position.z), Quaternion.Euler(0f, 0f, 10f));
            Sfx.Play(this, "bonk", 0.4f);
        }

        void MoveArm(Vector3 world, Quaternion rot)
        {
            if (armRoutine != null) StopCoroutine(armRoutine);
            armRoutine = StartCoroutine(ArmTo(world, rot));
        }

        /// <summary>Il braccio vola al suo nuovo posto (poi LateUpdate lo tiene attaccato alla spalla o al muso).</summary>
        IEnumerator ArmTo(Vector3 world, Quaternion rot)
        {
            armMoving = true;
            zombieArm.localScale = armScale;
            zombieArm.rotation = rot;
            yield return HalloweenKit.Move(zombieArm, new Vector3(world.x, world.y, zombieArm.position.z), 0.2f);
            armItem.Home = zombieArm.position;
            armMoving = false;
        }

        // ---------- campanello e signora ----------

        void Ring(bool withArm)
        {
            bool changed = logic.ChangedSinceLastRing;
            TrickRingResult r = withArm ? logic.TryRingWithArm() : logic.TryRingBell();
            if (r == TrickRingResult.Ignored) { if (withArm) ArmBack(); return; }
            StartCoroutine(HalloweenKit.Shake(bell, 0.05f, 0.3f));
            if (bellWaves != null) StartCoroutine(HalloweenKit.Burst(bellWaves, 0.4f));
            Sfx.Play(this, "ding", 0.8f);
            Haptics.Light();
            if (r == TrickRingResult.Scared) StartCoroutine(NormalEnding());
            else if (r == TrickRingResult.Egg) StartCoroutine(EggEnding());
            else StartCoroutine(LadyVisit(r, changed, withArm));
        }

        /// <summary>Dopo aver suonato con la mano, il braccio torna dov'era (in bocca a Toni) o cade per terra.</summary>
        void ArmBack()
        {
            if (armFrom == ArmPlace.InToniMouth && logic.Arm == ArmPlace.InToniMouth) ArmToMouth();
            else ArmToGround();
        }

        IEnumerator OpenDoor(Sprite face)
        {
            yield return new WaitForSeconds(0.4f);
            lady.sprite = face;
            doorClosed.SetActive(false);
            doorOpen.SetActive(true);
            Sfx.Play(this, "clunk", 0.6f);
            yield return HalloweenKit.Pop(doorOpen.transform, 1.04f, 0.2f);
        }

        void CloseDoor()
        {
            doorOpen.SetActive(false);
            doorClosed.SetActive(true);
            Sfx.Play(this, "clunk", 0.8f);
            Haptics.Light();
        }

        IEnumerator LadyVisit(TrickRingResult r, bool changed, bool withArm)
        {
            busy = true;
            if (withArm) ArmBack();
            yield return OpenDoor(ladyNormal);
            if (r == TrickRingResult.GaveChocolate)
            {
                ladySay.Show("How cute! Here's ONE chocolate. One per child!", 2.4f);
                yield return new WaitForSeconds(0.8f);
                chocolate.gameObject.SetActive(true);
                chocolate.position = lady.transform.position;
                yield return HalloweenKit.Move(chocolate, candyText.transform.position, 0.6f, 1f);
                chocolate.gameObject.SetActive(false);
                UpdateCandy(1);
                Sfx.Play(this, "pop", 0.6f);
                yield return new WaitForSeconds(1.2f);
                gang.Show("ONE?! For FIVE monsters?!", 2f);
            }
            else
            {
                string line = !changed ? "I've already seen you!"
                    : logic.GhostWrapped ? "A ghost with zombie feet? I've seen you all!"
                    : logic.ZombieAtDoor ? "Nice try, zombie. I've already seen you!"
                    : "I recognise every visitor, dearie!";
                ladySay.Show(line, 2.2f);
                Sfx.Play(this, "buzz", 0.4f);
                yield return new WaitForSeconds(1.8f);
            }
            CloseDoor();
            ladySay.Hide();
            yield return new WaitForSeconds(0.3f);
            logic.TryLadyLeaves();
            busy = false;
        }

        IEnumerator SpillCandy(Vector3 from, Vector3 around, float spread)
        {
            bucket.gameObject.SetActive(true);
            bucket.position = from;
            StartCoroutine(HalloweenKit.Wiggle(bucket, 40f, 0.6f, 20f));
            yield return HalloweenKit.Move(bucket, around + new Vector3(0.6f, 0.1f, 0f), 0.4f, 0.8f);
            Haptics.Medium();
            Sfx.Play(this, "bonk", 0.7f);
            for (int i = 0; i < candies.Length; i++)
            {
                Transform c = candies[i];
                c.gameObject.SetActive(true);
                c.position = bucket.position;
                float a = i / (float)candies.Length * Mathf.PI * 2f;
                Vector3 to = around + new Vector3(Mathf.Cos(a) * spread, Mathf.Sin(a) * spread * 0.35f, 0f);
                to = HalloweenKit.ClampSafe(to);
                StartCoroutine(HalloweenKit.Move(c, to, 0.5f + (i % 3) * 0.1f, 0.7f));
                if (i % 3 == 0) Sfx.Play(this, "tick", 0.5f);
            }
            yield return new WaitForSeconds(0.8f);
            UpdateCandy(candies.Length + 1);
        }

        // ---------- finali (si escludono: la logica ne permette uno solo) ----------

        IEnumerator NormalEnding()
        {
            busy = true;
            yield return OpenDoor(ladyNormal);
            yield return new WaitForSeconds(0.3f);
            if (spell != null) spell.Play();
            StartCoroutine(HalloweenKit.Pop(zombie, 1.2f, 0.5f));
            gang.Show("ROOOAAARRR!", 1.6f);
            Sfx.Play(this, "buzz", 0.9f);
            Haptics.Heavy();
            yield return new WaitForSeconds(0.4f);
            lady.sprite = ladyScared;
            ladySay.Show("AAAAAH! A REAL MONSTER!!! TAKE EVERYTHING!", 2.4f);
            yield return HalloweenKit.Shake(lady.transform, 0.08f, 0.8f);
            yield return SpillCandy(lady.transform.position, doorSpot.position + new Vector3(0.3f, -0.9f, 0f), 1.2f);
            CloseDoor();
            ladySay.Hide();
            if (spell != null) spell.Stop();
            // i cinque si separano e festeggiano
            bigSheet.SetActive(false);
            witchHat.SetActive(false);
            tailPeek.SetActive(false);
            zombieHead.SetActive(true);
            pumpkinKid.SetParent(transform, true);
            Vector3 c = zombie.position;
            Separate(toni, c + new Vector3(-1.1f, -1.2f, 0f));
            Separate(pumpkinKid, c + new Vector3(1.0f, -1.6f, 0f));
            Separate(ghost, c + new Vector3(-1.2f, 1.6f, 0f));
            Separate(witch, c + new Vector3(-1.9f, 0.1f, 0f));
            Sfx.Play(this, "whoosh", 0.8f);
            HalloweenKit.Confetti(confetti, true);
            Haptics.Success();
            gang.Show("TRICK OR TREAT! Candy for EVERYONE!", 2.6f);
            for (int k = 0; k < 3; k++)
            {
                StartCoroutine(HalloweenKit.Pop(toni, 1.15f, 0.3f));
                StartCoroutine(HalloweenKit.Pop(pumpkinKid, 1.15f, 0.3f));
                StartCoroutine(HalloweenKit.Pop(witch, 1.1f, 0.3f));
                yield return HalloweenKit.Pop(zombie, 1.08f, 0.3f);
                Sfx.Play(this, "pop", 0.5f);
            }
            yield return new WaitForSeconds(0.8f);
            HalloweenKit.Confetti(confetti, false);
            if (logic.TryCompleteEnding()) Win();
        }

        void Separate(Transform t, Vector3 to)
        {
            t.gameObject.SetActive(true);
            t.position = zombie.position + Vector3.up * 1f;
            HwDragger.Item it = t == toni ? toniItem : t == ghost ? ghostItem : t == witch ? witchItem : pumpkinItem;
            t.localScale = it.BaseScale;
            StartCoroutine(HalloweenKit.Move(t, HalloweenKit.ClampSafe(to), 0.45f, 0.9f));
        }

        IEnumerator EggEnding()
        {
            busy = true;
            // la mano torna in bocca a Toni
            ArmToMouth();
            yield return OpenDoor(ladyLove);
            ladySay.Show("WHAT AN ADORABLE PUG!", 2f);
            if (toniHearts != null) toniHearts.Play();
            Sfx.Play(this, "woof", 0.7f);
            yield return HalloweenKit.Wiggle(toni, 10f, 1.2f, 16f);
            ladySay.Show("*pat pat* Who's a good boy?", 1.6f);
            yield return new WaitForSeconds(1.6f);
            if (toniHearts != null) toniHearts.Stop();
            // poi vede gli altri mostri sullo sfondo
            lady.sprite = ladyScared;
            ladySay.Show("...WAIT. WHAT ARE THOSE?! AAAAH!", 2f);
            Sfx.Play(this, "buzz", 0.9f);
            Haptics.Heavy();
            StartCoroutine(HalloweenKit.Pop(zombie, 1.1f, 0.3f));
            StartCoroutine(HalloweenKit.Pop(witch, 1.1f, 0.3f));
            StartCoroutine(HalloweenKit.Pop(pumpkinKid, 1.1f, 0.3f));
            yield return HalloweenKit.Shake(lady.transform, 0.08f, 0.7f);
            yield return SpillCandy(lady.transform.position, toni.position + new Vector3(0f, -0.3f, 0f), 1.0f);
            CloseDoor();
            ladySay.Hide();
            // Toni scodinzola davanti alle caramelle, con la corona; gli altri si inchinano
            eggBanner.SetActive(true);
            StartCoroutine(HalloweenKit.Pop(eggBanner.transform, 1.15f, 0.35f));
            crown.SetActive(true);
            StartCoroutine(HalloweenKit.Pop(crown.transform, 1.3f, 0.3f));
            HalloweenKit.Confetti(confetti, true);
            Sfx.Play(this, "ding", 0.9f);
            Haptics.Success();
            foreach (Transform t in new[] { zombie, witch, ghost, pumpkinKid })
                if (t.gameObject.activeInHierarchy) StartCoroutine(Bow(t));
            yield return HalloweenKit.Wiggle(toni, 6f, 2.4f, 18f);
            HalloweenKit.Confetti(confetti, false);
            if (logic.TryCompleteEnding()) WinWithEgg(EggId);
        }

        IEnumerator Bow(Transform t)
        {
            Quaternion from = t.localRotation, to = from * Quaternion.Euler(0f, 0f, t.position.x < toni.position.x ? -25f : 25f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.4f)
            {
                t.localRotation = Quaternion.Slerp(from, to, k);
                yield return null;
            }
            t.localRotation = to;
        }
    }
}
