using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "One too many" — sei galline e un pollaio con cinque posti: "FIT EVERYONE INSIDE". Le galline si
    /// trascinano nel pollaio, ma la gallina vanitosa sul bordo della pozzanghera non entra senza la sua "gemella":
    /// provarci fa incastrare due galline nella porta (3 volte = sconfitta). Trucco: la sesta gallina è solo il riflesso
    /// nella pozzanghera. Si trascina via la pozzanghera, il riflesso sparisce, restano cinque galline e entrano da sole.
    /// </summary>
    public class OneTooManyLevel : LevelController
    {
        [SerializeField] Transform[] hens;              // 0..3 normali, 4 = la vanitosa (piedi nell'origine)
        [SerializeField] Transform puddle;
        [SerializeField] SpriteRenderer reflection;     // figlio della pozzanghera
        [SerializeField] Transform coopDoor;            // punto della porta del pollaio
        [SerializeField] SpriteRenderer[] nestHeads;    // testoline nelle 5 finestrelle (spente all'inizio)
        [SerializeField] SpriteRenderer[] feathers;
        [SerializeField] TextMesh counter;
        [SerializeField] TextMesh goal;
        [SerializeField] SpriteRenderer[] hearts;
        [SerializeField] SpeechBubble bubble;

        const int Vain = 4;
        const float PuddleTopY = 0.48f;   // bordo alto della pozzanghera (dove la vanitosa ha i piedi)

        Camera cam;
        int strikes, inside;
        bool[] inCoop;
        Vector3[] homes;
        bool busy, solved;
        int grabbed = -1;            // gallina presa, -2 = pozzanghera
        Vector2 grabOffset;
        float bubbleUntil;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            inCoop = new bool[hens.Length];
            homes = new Vector3[hens.Length];
            for (int i = 0; i < hens.Length; i++) homes[i] = hens[i].position;
            foreach (SpriteRenderer h in nestHeads) if (h != null) h.enabled = false;
            foreach (SpriteRenderer f in feathers) if (f != null) f.gameObject.SetActive(false);
            Kit41C.Hearts(hearts, 3);
            Kit41C.Goal(goal, "FIT EVERYONE INSIDE");
            UpdateCounter(6);
            Say("Six of us, five nests. Hmm.", 2.5f);
        }

        void Say(string text, float seconds)
        {
            if (bubble == null) return;
            bubble.Show(text);
            bubbleUntil = Time.time + seconds;
        }

        void UpdateCounter(int hensCount)
        {
            if (counter != null) counter.text = "HENS: " + hensCount + "   NESTS: 5";
        }

        void Update()
        {
            if (IsFinished) return;
            if (bubble != null && bubbleUntil > 0f && Time.time > bubbleUntil) { bubble.Hide(); bubbleUntil = 0f; }
            if (busy || solved) { grabbed = -1; return; }
            if (!Kit41C.Pointer(cam, out Vector2 w, out bool began, out bool ended)) { if (grabbed != -1) Release(w, true); return; }
            if (began) { Grab(w); return; }
            if (grabbed == -1) return;
            Vector2 target = w + grabOffset;
            target.x = Mathf.Clamp(target.x, -2.4f, 2.4f);
            target.y = Mathf.Clamp(target.y, -4.1f, 1.2f);
            if (grabbed == -2)
            {
                target.y = Mathf.Min(target.y, -0.9f);
                puddle.position = target;
                UpdateReflection();
            }
            else hens[grabbed].position = target;
            if (ended) Release(w, false);
        }

        void Grab(Vector2 w)
        {
            float best = 0.6f;
            grabbed = -1;
            for (int i = 0; i < hens.Length; i++)
            {
                if (inCoop[i]) continue;
                float d = Vector2.Distance(w, (Vector2)hens[i].position + Vector2.up * 0.5f);
                if (d < best) { best = d; grabbed = i; }
            }
            if (grabbed >= 0)
            {
                grabOffset = (Vector2)hens[grabbed].position - w;
                Sfx.Play(this, "tick", 0.5f);
                return;
            }
            Vector2 local = w - (Vector2)puddle.position;
            if ((local.x * local.x) / (1.35f * 1.35f) + (local.y * local.y) / (0.62f * 0.62f) <= 1f)
            {
                grabbed = -2;
                grabOffset = (Vector2)puddle.position - w;
                Sfx.Play(this, "pop", 0.4f);
            }
        }

        void Release(Vector2 w, bool lost)
        {
            int i = grabbed;
            grabbed = -1;
            if (i == -2) { CheckPuddle(); return; }
            if (i < 0) return;
            bool atCoop = !lost && Vector2.Distance(hens[i].position, coopDoor.position) < 1.1f;
            if (atCoop)
            {
                if (i == Vain && ReflectionAlpha() > 0.05f) { StartCoroutine(Squeeze()); return; }
                StartCoroutine(Enter(i));
                return;
            }
            if (i == Vain && ReflectionAlpha() > 0.05f)
            {
                // la vanitosa non lascia la sua "gemella"
                Say("I'm not leaving my twin!", 1.8f);
                StartCoroutine(Kit41C.MoveWorld(hens[i], homes[i], 0.25f, 0.15f));
                return;
            }
            homes[i] = hens[i].position;
        }

        float ReflectionAlpha()
        {
            Vector2 edge = (Vector2)puddle.position + Vector2.up * PuddleTopY;
            float d = Vector2.Distance(edge, homes[Vain]);
            return 1f - Mathf.Clamp01((d - 0.25f) / 0.5f);
        }

        void UpdateReflection()
        {
            if (reflection == null) return;
            Color c = reflection.color;
            c.a = inCoop[Vain] ? 0f : ReflectionAlpha();
            reflection.color = c;
        }

        void CheckPuddle()
        {
            UpdateReflection();
            if (ReflectionAlpha() <= 0.02f && !inCoop[Vain]) StartCoroutine(Solved());
        }

        IEnumerator Enter(int i)
        {
            busy = true;
            inCoop[i] = true;
            Transform h = hens[i];
            yield return Kit41C.MoveWorld(h, coopDoor.position, 0.25f, 0.2f);
            Vector3 s = h.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.2f) { h.localScale = s * (1f - k); yield return null; }
            h.gameObject.SetActive(false);
            h.localScale = s;
            if (inside < nestHeads.Length && nestHeads[inside] != null)
            {
                nestHeads[inside].enabled = true;
                StartCoroutine(Kit41C.Pop(nestHeads[inside].transform, 0.3f));
            }
            inside++;
            Sfx.Play(this, "pop", 0.6f);
            busy = false;
            if (inside >= 5) yield return Finale();
        }

        IEnumerator Squeeze()
        {
            busy = true;
            Transform h = hens[Vain];
            Say("Me AND my twin! Squeeze!", 1.6f);
            yield return Kit41C.MoveWorld(h, coopDoor.position, 0.2f, 0.1f);
            // incastrate nella porta
            Vector3 s = h.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.35f)
            {
                h.localScale = new Vector3(s.x * (1f + Mathf.Sin(k * Mathf.PI * 4f) * 0.25f), s.y * (1f - Mathf.Sin(k * Mathf.PI * 4f) * 0.2f), 1f);
                yield return null;
            }
            h.localScale = s;
            Sfx.Play(this, "bonk", 0.8f);
            Haptics.Medium();
            StartCoroutine(Kit41C.Burst(feathers, coopDoor.position + Vector3.up * 0.3f, 1.1f));
            strikes++;
            Kit41C.Hearts(hearts, 3 - strikes);
            yield return Kit41C.MoveWorld(h, homes[Vain], 0.35f, 0.4f);
            if (strikes >= 3)
            {
                Kit41C.Goal(goal, "STUCK IN THE DOOR!");
                Say("BAWK! We're stuck forever!", 2f);
                yield return new WaitForSeconds(1.3f);
                Fail();
                yield break;
            }
            Say(strikes == 1 ? "Six hens, five nests... BAWK!" : "Still one too many!", 1.8f);
            busy = false;
        }

        IEnumerator Solved()
        {
            solved = true;
            busy = true;
            UpdateCounter(5);
            Sfx.Play(this, "ding", 0.6f);
            Kit41C.Goal(goal, "ONLY FIVE HENS!");
            yield return Kit41C.Pop(hens[Vain], 0.2f);
            Say("Wait... where did my twin go?", 2f);
            yield return new WaitForSeconds(1.2f);
            // le galline rimaste entrano una alla volta
            for (int i = 0; i < hens.Length; i++)
            {
                if (inCoop[i]) continue;
                inCoop[i] = true;
                Transform h = hens[i];
                yield return Kit41C.MoveWorld(h, coopDoor.position, 0.35f, 0.25f);
                h.gameObject.SetActive(false);
                if (inside < nestHeads.Length && nestHeads[inside] != null)
                {
                    nestHeads[inside].enabled = true;
                    StartCoroutine(Kit41C.Pop(nestHeads[inside].transform, 0.3f));
                }
                inside++;
                Sfx.Play(this, "pop", 0.6f);
            }
            yield return Finale();
        }

        IEnumerator Finale()
        {
            busy = true;
            solved = true;
            Kit41C.Goal(goal, "EVERYONE'S INSIDE!");
            for (int b = 0; b < 3; b++)
            {
                foreach (SpriteRenderer h in nestHeads) if (h != null && h.enabled) StartCoroutine(Kit41C.Pop(h.transform, 0.25f));
                Sfx.Play(this, "pop", 0.5f);
                yield return new WaitForSeconds(0.3f);
            }
            Sfx.Play(this, "ding", 0.8f);
            yield return new WaitForSeconds(0.9f);
            Win();
        }
    }
}
