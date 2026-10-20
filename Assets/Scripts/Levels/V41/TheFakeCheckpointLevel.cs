using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "The fake checkpoint" — a metà strada c'è un checkpoint con la bandierina: passandoci sopra l'eroe
    /// viene "salvato" e rispedito all'inizio (tre passaggi = sconfitta). Il trucco: trascina la bandierina dal palo
    /// fino alla porta finale; il checkpoint ora è all'uscita e l'eroe ci arriva. Non tocca il vero salvataggio.
    /// </summary>
    public class TheFakeCheckpointLevel : S41JLevelBase
    {
        [SerializeField] Transform hero;
        [SerializeField] Collider2D heroArea;
        [SerializeField] Collider2D goArea;
        [SerializeField] Transform flag;
        [SerializeField] Collider2D flagArea;
        [SerializeField] Transform flagHome;          // cima del palo
        [SerializeField] Transform doorSpot;          // posto della bandierina accanto alla porta
        [SerializeField] SpriteRenderer doorArt;
        [SerializeField] Sprite doorOpen;
        [SerializeField] SmokeEmitter poof;
        [SerializeField] SpriteRenderer sparkle;
        [SerializeField] TextMesh banner;             // scritta "CHECKPOINT!"
        [SerializeField] TextMesh triesText;
        [SerializeField] float doorX = 2.2f;

        const int MaxTries = 3;
        const float SnapDistance = 1.3f;
        int tries;
        bool grabbing, atDoor;
        Vector2 grabOffset;
        Vector3 heroHome;

        protected override void Begin()
        {
            heroHome = hero.localPosition;
            if (poof != null) poof.Stop();
            Alpha(sparkle, 0f);
            if (banner != null) banner.text = "";
            UpdateTries();
            Say("A checkpoint! How convenient. Press GO!", 2.8f);
        }

        void UpdateTries() { if (triesText != null) triesText.text = "RESPAWNS " + tries + "/" + MaxTries; }

        protected override void Tick()
        {
            if (busy) return;
            if (!Pointer(out Vector2 w, out bool began, out bool ended)) { if (grabbing) DropFlag(); return; }
            if (began)
            {
                if (!atDoor && flagArea != null && flagArea.OverlapPoint(w))
                {
                    grabbing = true;
                    grabOffset = (Vector2)flag.position - w;
                    Sfx.Play(this, "tick", 0.5f);
                }
                else if ((goArea != null && goArea.OverlapPoint(w)) || (heroArea != null && heroArea.OverlapPoint(w))) { StartCoroutine(Run()); return; }
            }
            if (grabbing)
            {
                Vector2 p = w + grabOffset;
                flag.position = new Vector3(Mathf.Clamp(p.x, -2.5f, 2.6f), Mathf.Clamp(p.y, -2.6f, 2.6f), flag.position.z);
                flag.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(clock * 10f) * 5f);
            }
            if (ended && grabbing) DropFlag();
        }

        void DropFlag()
        {
            grabbing = false;
            flag.localRotation = Quaternion.identity;
            if (Vector2.Distance(flag.position, doorSpot.position) < SnapDistance)
            {
                atDoor = true;
                StartCoroutine(Move(flag, doorSpot.localPosition, 0.15f));
                Sfx.Play(this, "clunk", 0.6f);
                Haptics.Light();
                Say("The checkpoint... moved?", 2f);
            }
            else
            {
                StartCoroutine(Move(flag, flagHome.localPosition, 0.2f));
                Sfx.Play(this, "pop", 0.4f);
            }
        }

        IEnumerator Banner(string text)
        {
            if (banner == null) yield break;
            banner.text = text;
            yield return Pop(banner.transform, 1.3f, 0.3f);
            yield return new WaitForSeconds(0.6f);
            banner.text = "";
        }

        IEnumerator Run()
        {
            busy = true;
            grabbing = false;
            Say("Let's go!", 1.2f);
            float target = atDoor ? doorX - 0.5f : flag.localPosition.x;
            yield return Walk(hero, new Vector3(target, heroHome.y, 0f), 2.2f, 0.08f);
            // la bandierina si attiva
            sparkle.transform.position = flag.position;
            Alpha(sparkle, 1f);
            StartCoroutine(Pop(sparkle.transform, 1.5f, 0.4f));
            Sfx.Play(this, "ding", 0.5f);
            if (atDoor)
            {
                yield return Banner("CHECKPOINT = EXIT!");
                Alpha(sparkle, 0f);
                if (doorArt != null && doorOpen != null) doorArt.sprite = doorOpen;
                Sfx.Play(this, "clunk", 0.8f);
                Haptics.Medium();
                Say("Saved AND finished. Genius!", 2.2f);
                yield return Walk(hero, new Vector3(doorX, heroHome.y, 0f), 1.5f, 0.08f);
                for (float k = 0f; k < 1f; k += Time.deltaTime / 0.35f) { hero.localScale = Vector3.one * (1f - k * 0.7f); yield return null; }
                hero.gameObject.SetActive(false);
                yield return Celebrate();
                yield break;
            }
            tries++;
            UpdateTries();
            yield return Banner("CHECKPOINT!");
            Alpha(sparkle, 0f);
            // "salvato"... e rispedito all'inizio
            if (poof != null) { poof.transform.position = hero.position + Vector3.up * 0.5f; poof.Play(); }
            Sfx.Play(this, "whoosh", 0.8f);
            Haptics.Medium();
            hero.gameObject.SetActive(false);
            yield return new WaitForSeconds(0.35f);
            if (poof != null) poof.Stop();
            hero.localPosition = heroHome;
            hero.gameObject.SetActive(true);
            Sfx.Play(this, "pop", 0.6f);
            yield return Pop(hero, 1.25f, 0.25f);
            string[] lines = { "Wait... I'm back at the start?!", "Again?! This checkpoint is broken!", "I'm stuck in a loop... forever..." };
            Say(lines[Mathf.Min(tries - 1, lines.Length - 1)], 2.4f);
            if (tries >= MaxTries)
            {
                Haptics.Heavy();
                for (float k = 0f; k < 1f; k += Time.deltaTime / 0.8f) { hero.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(k * 30f) * 15f); yield return null; }
                yield return new WaitForSeconds(0.8f);
                Fail();
                yield break;
            }
            busy = false;
        }
    }
}
