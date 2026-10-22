using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "Detective" — tre sospettati (gatto, cane, pappagallo) e un caso a caso fra tre. 1) Tocca ogni sospettato
    /// per sentire il suo alibi. 2) Lente d'ingrandimento: allarga due dita sulla foto della scena del crimine e tocca i 3
    /// indizi minuscoli (impronte, una piuma, briciole) — si possono toccare solo ingranditi. Ogni indizio dice qualcosa
    /// di preciso SU QUESTO CASO: due scagionano gli innocenti e uno smaschera il bugiardo (es. caso della torta: impronte
    /// di cane ASCIUTTE accanto alla torta, ma il cane dice di aver dormito sotto la pioggia). Gli indizi trovati restano
    /// scritti in alto e si possono rileggere toccando le icone. 3) Trascina le manette su chi ha mentito. Accusa
    /// sbagliata = un tentativo in meno (3).
    /// </summary>
    public class DetectiveLevel : LevelController
    {
        const int Cat = 0, Dog = 1, Parrot = 2;

        static readonly string[] Titles = { "CASE: WHO ATE THE CAKE?", "CASE: THE COOKIE HEIST", "CASE: THE TOP-SHELF PIE" };
        static readonly string[] Facts = { "Fact: it rained ALL night.", "Fact: the cookies vanished at noon.", "Fact: the pie was on the HIGHEST shelf." };
        static readonly int[] Liar = { Dog, Cat, Parrot };
        static readonly string[][] Alibis =
        {
            new[] { "I got soaked chasing a moth. Then I dried off by the heater.", "I slept in the garden ALL night. Under the stars!", "I was in my cage all night, practicing my whistle." },
            new[] { "Cookies? Ew. I only eat fancy fish.", "At noon I was digging in the garden. Look at my paws!", "I was singing on the windowsill. Everyone heard me!" },
            new[] { "I tried to jump up there once. Too high, even for me.", "The top shelf? I can't even reach the sofa.", "Me? I can't fly! My wing hurts. Ouch. Ouch." },
        };
        static readonly string[] Confessions =
        {
            "FINE! I ate it. The garden was too wet... and the cake was too dry. Woof.",
            "Meow... the fancy fish was out of stock, OK?!",
            "SQUAWK! A miracle! My wing is healed! ...Also I ate the pie.",
        };
        // indizi per caso (impronte, piuma, briciole): frase completa e appunto breve
        static readonly string[][] ClueText =
        {
            new[] { "Big DOG paw prints right by the cake... and they're totally DRY!",
                    "The parrot's feather is inside its cage: the parrot stayed home all night.",
                    "Cake crumbs leading to the DOG bed." },
            new[] { "Muddy DOG paw prints going to the garden, not the kitchen.",
                    "A green feather on the windowsill: the parrot really was singing there.",
                    "Cookie crumbs... with a few CAT whiskers stuck in them!" },
            new[] { "Paw prints stop at the bottom of the shelf: no paws climbed up there.",
                    "A green feather on the TOP shelf, right next to the pie!",
                    "Pie crumbs fell from up high... like something FLEW away with it." },
        };
        static readonly string[][] ClueNotes =
        {
            new[] { "dry dog prints", "parrot stayed in", "crumbs to dog bed" },
            new[] { "dog went to garden", "parrot on sill", "cat whiskers in crumbs" },
            new[] { "no paws up the shelf", "feather on top shelf", "crumbs fell from above" },
        };

        static readonly string[][] Protests =
        {
            new[] { "Me?! I'm innocent AND fabulous.", "Me?! I'm a good boy! The goodest!", "Squawk! LAWYER! I want a LAWYER!" },
            new[] { "How DARE you. Check my alibi, human.", "Woof?! I was digging! Look at the mud!", "Squawk! I have witnesses! Everyone!" },
            new[] { "Me? I can't even reach it. Rude.", "I can't reach the sofa, remember?", "Squawk! Lawyer!" },
        };

        [SerializeField] StageHud40B hud;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] Transform lineupGroup, sceneGroup;
        [SerializeField] Transform[] suspects;           // gatto, cane, pappagallo (con Collider2D)
        [SerializeField] SpriteRenderer[] ticks;          // spunta "alibi sentito"
        [SerializeField] GameObject catWet, catCrumbs, dogMud;
        [SerializeField] Transform parrotWing;
        [SerializeField] TextMesh caseText;
        [SerializeField] Transform photo;                 // foto della scena (si ingrandisce e si sposta)
        [SerializeField] Transform[] clues;               // impronte, piuma, briciole (figli di photo)
        [SerializeField] Transform[] clueSpots;           // 2 posti possibili per ogni indizio (figli di photo)
        [SerializeField] SpriteRenderer[] clueMarks;      // cerchietto sull'indizio trovato
        [SerializeField] SpriteRenderer[] clueIcons;      // indizi trovati nella barra
        [SerializeField] TextMesh zoomText;
        [SerializeField] Transform cuffs;
        [SerializeField] Vector2 photoSize = new Vector2(5.2f, 6.6f);
        [SerializeField] float minZoomToSee = 2.2f, maxZoom = 5f;

        int caseId, stage;
        bool busy;
        bool[] heard = new bool[3];
        bool[] found = new bool[3];
        float zoom = 1f, lastPinch, bubbleUntil, clock;
        bool pinching, dragging, moved;
        Vector2 lastDrag, cuffOffset;
        Vector3 photoHome, cuffHome, parrotHome;
        bool holdingCuffs;
        Camera cam;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            caseId = Random.Range(0, Titles.Length);
            caseText.text = Titles[caseId] + "\n" + Facts[caseId];
            catWet.SetActive(caseId == 0);
            catCrumbs.SetActive(caseId == 1);
            dogMud.SetActive(caseId == 1);
            parrotHome = suspects[Parrot].localPosition;
            foreach (SpriteRenderer t in ticks) t.enabled = false;
            foreach (SpriteRenderer m in clueMarks) m.enabled = false;
            foreach (SpriteRenderer c in clueIcons) c.color = new Color(0f, 0f, 0f, 0.35f);
            for (int i = 0; i < clues.Length; i++) clues[i].localPosition = clueSpots[i * 2 + Random.Range(0, 2)].localPosition;
            photoHome = photo.localPosition;
            cuffHome = cuffs.position;
            cuffs.gameObject.SetActive(false);
            sceneGroup.gameObject.SetActive(false);
            hud.Begin(3);
            hud.Goal("Question the suspects: tap each one.");
        }

        void Say(string text, Transform who, float seconds)
        {
            if (bubble == null) return;
            Vector3 p = who != null ? who.position : Vector3.zero;
            bubble.transform.position = new Vector3(Mathf.Clamp(p.x, -1.05f, 1.05f), 0.95f, bubble.transform.position.z);
            bubble.Show(text);
            bubbleUntil = clock + seconds;
        }

        void Update()
        {
            if (IsFinished) return;
            clock += Time.deltaTime;
            if (bubbleUntil > 0f && clock > bubbleUntil) { bubble.Hide(); bubbleUntil = 0f; }
            // il pappagallo del caso 3 svolazza (e dice di non poter volare)
            Transform parrot = suspects[Parrot];
            if (caseId == 2)
            {
                parrot.localPosition = parrotHome + Vector3.up * (0.25f + Mathf.Sin(clock * 3f) * 0.12f);
                parrotWing.localRotation = Quaternion.Euler(0, 0, 40f + Mathf.Sin(clock * 30f) * 45f);
            }
            else parrotWing.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(clock * 1.5f) * 3f);
            if (busy) return;
            if (stage == 0) UpdateQuestion();
            else if (stage == 1) UpdateMagnifier();
            else if (stage == 2) UpdateAccuse();
        }

        int SuspectAt(Vector2 w)
        {
            for (int i = 0; i < suspects.Length; i++) if (suspects[i].GetComponent<Collider2D>().OverlapPoint(w)) return i;
            return -1;
        }

        // ---------- fase 1: gli alibi ----------

        void UpdateQuestion()
        {
            if (!PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out _) || !began || PointerInput.IsOverUi()) return;
            int s = SuspectAt(PointerInput.ToWorld(cam, sp));
            if (s < 0) return;
            Hear(s);
            if (heard[0] && heard[1] && heard[2]) StartCoroutine(ToScene());
        }

        void Hear(int s)
        {
            heard[s] = true;
            ticks[s].enabled = true;
            Say(Alibis[caseId][s], suspects[s], 4f);
            Sfx.Play(this, s == Dog ? "woof" : "pop", 0.5f);
            StartCoroutine(Hop(suspects[s]));
        }

        IEnumerator Hop(Transform t)
        {
            Vector3 s = t.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.2f) { t.localScale = new Vector3(s.x * (1f + Mathf.Sin(k * Mathf.PI) * 0.06f), s.y * (1f - Mathf.Sin(k * Mathf.PI) * 0.06f), s.z); yield return null; }
            t.localScale = s;
        }

        IEnumerator ToScene()
        {
            busy = true;
            yield return new WaitForSeconds(3.2f);
            bubble.Hide();
            bubbleUntil = 0f;
            yield return hud.Next(1, "STAGE 2: THE CRIME SCENE", "Pinch to zoom the photo. Find and tap 3 clues!");
            lineupGroup.gameObject.SetActive(false);
            sceneGroup.gameObject.SetActive(true);
            Zoom();
            stage = 1;
            busy = false;
        }

        // ---------- fase 2: la lente ----------

        void Zoom() { if (zoomText != null) zoomText.text = "ZOOM x" + zoom.ToString("0.0"); }

        void ZoomAt(Vector2 world, float factor)
        {
            float z = Mathf.Clamp(zoom * factor, 1f, maxZoom);
            factor = z / zoom;
            zoom = z;
            Vector3 p = photo.position;
            photo.position = (Vector3)(world + ((Vector2)p - world) * factor) + Vector3.forward * p.z;
            photo.localScale = Vector3.one * zoom;
            Clamp();
            Zoom();
        }

        void Clamp()
        {
            Vector2 half = photoSize * 0.5f * (zoom - 1f);
            Vector3 p = photo.localPosition;
            p.x = Mathf.Clamp(p.x, photoHome.x - half.x, photoHome.x + half.x);
            p.y = Mathf.Clamp(p.y, photoHome.y - half.y, photoHome.y + half.y);
            photo.localPosition = p;
        }

        void UpdateMagnifier()
        {
            if (Input.touchCount >= 2)
            {
                Vector2 a = PointerInput.ToWorld(cam, Input.GetTouch(0).position), b = PointerInput.ToWorld(cam, Input.GetTouch(1).position);
                float d = Vector2.Distance(a, b);
                if (pinching && lastPinch > 0.05f) ZoomAt((a + b) * 0.5f, d / lastPinch);
                lastPinch = d;
                pinching = true;
                dragging = false;
                return;
            }
            pinching = false;
#if UNITY_EDITOR
            float wheel = Input.mouseScrollDelta.y;
            if (Mathf.Abs(wheel) > 0.01f) ZoomAt(PointerInput.ToWorld(cam, Input.mousePosition), 1f + wheel * 0.15f);
#endif
            if (!PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out bool ended)) { dragging = false; return; }
            Vector2 w = PointerInput.ToWorld(cam, sp);
            if (began) { if (PointerInput.IsOverUi()) return; dragging = true; moved = false; lastDrag = w; return; }
            if (dragging && !ended)
            {
                Vector2 delta = w - lastDrag;
                if (delta.sqrMagnitude > 0.0009f) moved = true;
                if (moved) { photo.position += (Vector3)delta; Clamp(); }
                lastDrag = w;
            }
            if (ended && dragging && !moved && !TapClueIcon(w)) TapPhoto(w);
            if (ended) dragging = false;
        }

        void TapPhoto(Vector2 w)
        {
            for (int i = 0; i < clues.Length; i++)
            {
                if (found[i]) continue;
                float r = Mathf.Max(0.12f * zoom, 0.3f);
                if (Vector2.Distance(w, clues[i].position) > r) continue;
                if (zoom < minZoomToSee) { Say("Something tiny there... pinch to zoom in!", null, 1.8f); Sfx.Play(this, "tick", 0.5f); return; }
                StartCoroutine(Found(i));
                return;
            }
            if (zoom < minZoomToSee) Say("Too small to see. Spread two fingers to zoom!", null, 1.8f);
        }

        IEnumerator Found(int i)
        {
            busy = true;
            found[i] = true;
            clueMarks[i].enabled = true;
            clueIcons[i].color = Color.white;
            Sfx.Play(this, "ding", 0.7f);
            Haptics.Light();
            Say("CLUE: " + ClueText[caseId][i], null, 3.2f);
            ShowNotes();
            Transform m = clueMarks[i].transform;
            Vector3 s = m.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.35f) { m.localScale = s * (1f + Mathf.Sin(k * Mathf.PI) * 0.6f); yield return null; }
            m.localScale = s;
            if (!(found[0] && found[1] && found[2])) { busy = false; yield break; }
            yield return new WaitForSeconds(2.6f);
            // si torna alla foto intera e poi alla fila dei sospettati
            float from = zoom;
            Vector3 p0 = photo.localPosition;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.5f)
            {
                zoom = Mathf.Lerp(from, 1f, k);
                photo.localScale = Vector3.one * zoom;
                photo.localPosition = Vector3.Lerp(p0, photoHome, k);
                yield return null;
            }
            zoom = 1f;
            photo.localScale = Vector3.one;
            photo.localPosition = photoHome;
            yield return hud.Next(2, "STAGE 3: J'ACCUSE!", "Someone lied. Drag the handcuffs onto the liar!");
            sceneGroup.gameObject.SetActive(false);
            lineupGroup.gameObject.SetActive(true);
            cuffs.gameObject.SetActive(true);
            cuffs.position = cuffHome;
            stage = 2;
            busy = false;
        }

        /// <summary>In alto: il caso, il fatto e gli appunti sugli indizi trovati.</summary>
        void ShowNotes()
        {
            var notes = new System.Collections.Generic.List<string>();
            for (int i = 0; i < found.Length; i++) if (found[i]) notes.Add(ClueNotes[caseId][i]);
            caseText.text = Titles[caseId] + "\n" + Facts[caseId] + (notes.Count > 0 ? "\nClues: " + string.Join(", ", notes) : "");
        }

        /// <summary>Toccando l'icona di un indizio trovato si rilegge che cosa dice.</summary>
        bool TapClueIcon(Vector2 w)
        {
            for (int i = 0; i < clueIcons.Length; i++)
            {
                if (!found[i] || Vector2.Distance(w, clueIcons[i].transform.position) > 0.35f) continue;
                Say(ClueText[caseId][i], null, 3.2f);
                return true;
            }
            return false;
        }

        // ---------- fase 3: l'accusa ----------

        void UpdateAccuse()
        {
            if (!PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out bool ended))
            {
                if (holdingCuffs) { holdingCuffs = false; StartCoroutine(Return()); }
                return;
            }
            Vector2 w = PointerInput.ToWorld(cam, sp);
            if (began)
            {
                if (PointerInput.IsOverUi()) return;
                if (Vector2.Distance(w, cuffs.position) < 0.7f) { holdingCuffs = true; cuffOffset = (Vector2)cuffs.position - w; return; }
                if (TapClueIcon(w)) return;
                int s = SuspectAt(w);
                if (s >= 0) Hear(s);
                return;
            }
            if (!holdingCuffs) return;
            cuffs.position = (Vector3)(w + cuffOffset) + Vector3.forward * cuffs.position.z;
            if (!ended) return;
            holdingCuffs = false;
            int who = SuspectAt(cuffs.position);
            if (who < 0) { StartCoroutine(Return()); return; }
            StartCoroutine(Accuse(who));
        }

        IEnumerator Return()
        {
            Vector3 from = cuffs.position;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.2f) { cuffs.position = Vector3.Lerp(from, cuffHome, k); yield return null; }
            cuffs.position = cuffHome;
        }

        IEnumerator Accuse(int who)
        {
            busy = true;
            if (who == Liar[caseId])
            {
                // manette ai polsi e confessione
                Vector3 from = cuffs.position, to = suspects[who].position + new Vector3(0f, 0.35f, 0f);
                for (float k = 0f; k < 1f; k += Time.deltaTime / 0.15f) { cuffs.position = Vector3.Lerp(from, to, k); yield return null; }
                Sfx.Play(this, "clunk", 1f);
                Haptics.Medium();
                if (who == Parrot) { caseId = -1; suspects[Parrot].localPosition = parrotHome; }   // atterra
                hud.SetStage(3);
                hud.Shout("CASE CLOSED!");
                int c = System.Array.IndexOf(Liar, who);
                Say(Confessions[c], suspects[who], 4f);
                Transform t = suspects[who];
                Vector3 p = t.localPosition;
                for (float k = 0f; k < 1.2f; k += Time.deltaTime) { t.localPosition = p + new Vector3(Mathf.Sin(k * 40f) * 0.03f, 0f, 0f); cuffs.position = to + (t.localPosition - p); yield return null; }
                t.localPosition = p;
                yield return new WaitForSeconds(2.2f);
                Win();
                yield break;
            }
            Sfx.Play(this, "buzz", 0.7f);
            Say(Protests[caseId][who], suspects[who], 2.4f);
            StartCoroutine(Hop(suspects[who]));
            yield return Return();
            if (hud.LoseTry())
            {
                hud.Shout("CASE COLD");
                yield return new WaitForSeconds(0.8f);
                Say("While you argued, the real culprit tiptoed away...", null, 3f);
                Transform liar = suspects[Liar[caseId]];
                Vector3 p = liar.localPosition;
                for (float k = 0f; k < 1f; k += Time.deltaTime / 1.2f) { liar.localPosition = p + Vector3.right * k * 5f; yield return null; }
                yield return new WaitForSeconds(1.2f);
                Fail();
                yield break;
            }
            busy = false;
        }
    }
}
