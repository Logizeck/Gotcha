using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "The wrong shadow" — un gatto che dorme proietta un'ombra enorme a forma di mostro proprio sulla corsia
    /// del topo, che ha troppa paura di passare (tre tentativi di farlo passare = sconfitta). Il trucco: trascina la
    /// lampada dall'altra parte del gatto; l'ombra finisce dietro di lui, sul muro, e la strada è libera.
    /// </summary>
    public class TheWrongShadowLevel : S41FLevelBase
    {
        [SerializeField] Transform lamp;               // base della lampada
        [SerializeField] Collider2D lampArea;
        [SerializeField] SpriteRenderer lampArt;
        [SerializeField] Transform catFeet;            // perno dell'ombra
        [SerializeField] Transform shadow;
        [SerializeField] SpriteRenderer shadowArt;
        [SerializeField] Transform mouse;
        [SerializeField] Collider2D mouseArea;
        [SerializeField] TextMesh triesText;
        [SerializeField] float pathY = -3.95f, cheeseX = -1.75f, shadowLength = 3.0f;

        const int MaxTries = 3;
        Vector2 grabOffset;
        bool grabbing, saidFree;
        int tries;
        Vector3 mouseHome;

        protected override void Begin()
        {
            mouseHome = mouse.localPosition;
            UpdateTries();
            UpdateShadow();
            Say("A M-M-MONSTER! Tap me when it's safe...", 3f);
        }

        void UpdateTries() { if (triesText != null) triesText.text = "SCARES " + tries + "/" + MaxTries; }

        protected override void Tick()
        {
            UpdateShadow();
            if (busy) return;
            if (!Pointer(out Vector2 w, out bool began, out bool ended)) { grabbing = false; return; }
            if (began)
            {
                if (mouseArea != null && mouseArea.OverlapPoint(w)) { StartCoroutine(Run()); return; }
                if (lampArea != null && lampArea.OverlapPoint(w)) { grabbing = true; grabOffset = (Vector2)lamp.position - w; Sfx.Play(this, "tick", 0.5f); }
            }
            if (grabbing) DragLamp(w + grabOffset);
            if (ended)
            {
                grabbing = false;
                if (!Blocked(out _) && !saidFree) { saidFree = true; Say("Huh? The monster went up the wall...", 2.5f); }
            }
        }

        void DragLamp(Vector2 p)
        {
            p.x = Mathf.Clamp(p.x, -2.4f, 2.4f);
            p.y = Mathf.Clamp(p.y, -3.3f, -0.75f);
            Vector2 c = catFeet.position;
            Vector2 d = p - c;
            if (d.magnitude < 0.65f) p = c + (d.sqrMagnitude < 0.0001f ? Vector2.right : d.normalized) * 0.65f;
            lamp.position = new Vector3(p.x, p.y, lamp.position.z);
            // la lampada davanti al gatto (più in basso) si disegna sopra di lui
            if (lampArt != null) lampArt.sortingOrder = p.y < c.y ? 5 : 1;
        }

        Vector2 ShadowDir(out float scale)
        {
            Vector2 d = (Vector2)catFeet.position - (Vector2)lamp.position;
            float dist = d.magnitude;
            scale = Mathf.Clamp(2.8f / (dist + 0.6f), 0.75f, 1.35f);
            return dist < 0.001f ? Vector2.up : d / dist;
        }

        /// <summary>L'ombra copre la corsia del topo tra lui e il formaggio?</summary>
        bool Blocked(out float pathX)
        {
            Vector2 n = ShadowDir(out float scale);
            pathX = 0f;
            if (n.y > -0.25f) return false;
            float t = (pathY - catFeet.position.y) / n.y;
            if (t > shadowLength * scale + 0.3f) return false;
            pathX = catFeet.position.x + n.x * t;
            return pathX > cheeseX - 0.4f && pathX < mouseHome.x + 0.3f;
        }

        void UpdateShadow()
        {
            Vector2 n = ShadowDir(out float scale);
            float ang = Mathf.Atan2(n.y, n.x) * Mathf.Rad2Deg - 90f;
            shadow.position = catFeet.position;
            shadow.localRotation = Quaternion.Euler(0f, 0f, ang + Mathf.Sin(clock * 2.3f) * 2f);
            shadow.localScale = new Vector3(scale * (1f + Mathf.Sin(clock * 3.1f) * 0.03f), scale, 1f);
            Alpha(shadowArt, n.y < 0f ? 0.62f : 0.42f);
        }

        IEnumerator Run()
        {
            busy = true;
            grabbing = false;
            Sfx.Play(this, "pop", 0.6f);
            if (Blocked(out float px))
            {
                tries++;
                UpdateTries();
                float stop = Mathf.Min(mouse.localPosition.x, px + 0.75f);
                Say("Okay... okay... I can do this...", 1.5f);
                yield return Walk(mouse, new Vector3(stop, mouseHome.y, 0f), 1.6f);
                Say(tries >= MaxTries ? "AAAAAH! *faints*" : "EEEEK! THE MONSTER!", 2f);
                Sfx.Play(this, "bonk", 0.9f);
                Haptics.Medium();
                yield return Move(mouse, mouse.localPosition + Vector3.up * 0.01f, 0.25f, 0.5f);
                if (tries >= MaxTries)
                {
                    for (float k = 0f; k < 1f; k += Time.deltaTime / 0.4f) { mouse.localRotation = Quaternion.Euler(0, 0, -90f * k); yield return null; }
                    Sfx.Play(this, "clunk", 0.7f);
                    Haptics.Heavy();
                    yield return new WaitForSeconds(1.2f);
                    Fail();
                    yield break;
                }
                mouse.localScale = new Vector3(-1f, 1f, 1f);
                yield return Walk(mouse, mouseHome, 4.5f, 0.08f);
                mouse.localScale = Vector3.one;
                yield return Shake(mouse, 0.05f, 0.5f);
                busy = false;
                yield break;
            }
            // strada libera: il topo passa davanti al gatto e raggiunge il formaggio
            Say("Just a shadow? Pfft. I knew it.", 2.5f);
            yield return Walk(mouse, new Vector3(cheeseX, mouseHome.y, 0f), 1.8f);
            Sfx.Play(this, "pop", 0.8f);
            Haptics.Light();
            Say("CHEESE! Mine!", 2f);
            yield return Move(mouse, mouse.localPosition, 0.35f, 0.45f);
            yield return Move(mouse, mouse.localPosition, 0.35f, 0.45f);
            yield return Celebrate();
        }
    }
}
