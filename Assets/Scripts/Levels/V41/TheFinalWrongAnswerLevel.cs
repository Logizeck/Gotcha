using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "The final wrong answer" — un quiz: "WHAT IS 2 + 2?" e tre porte con le risposte 3, 4 e FISH.
    /// Ogni porta è sbagliata (anche il 4: "TOO OBVIOUS!"); due risposte = sconfitta. Il trucco: non si sceglie
    /// nessuna porta, si trascina giù la domanda stessa: cade sul pavimento e dietro c'era un'apertura nel muro.
    /// </summary>
    public class TheFinalWrongAnswerLevel : S41JLevelBase
    {
        [SerializeField] Transform question;
        [SerializeField] Collider2D questionArea;
        [SerializeField] Transform hole;
        [SerializeField] Transform[] doors;
        [SerializeField] SpriteRenderer[] doorArts;
        [SerializeField] Collider2D[] doorAreas;
        [SerializeField] Sprite doorClosed, doorOpen;
        [SerializeField] GameObject[] doorSurprises;  // cosa c'è dietro ogni porta
        [SerializeField] Transform mouse;
        [SerializeField] TextMesh triesText;
        [SerializeField] SpriteRenderer buzzerLight;
        [SerializeField] float floorY = -3.9f;

        const int MaxTries = 2;
        static readonly string[] Verdicts = { "WRONG! 2 + 2 is not 3!", "TOO OBVIOUS!", "FISH?! ...no." };
        int tries;
        bool grabbing;
        Vector2 grabOffset;
        Vector3 questionHome;

        protected override void Begin()
        {
            questionHome = question.localPosition;
            foreach (GameObject g in doorSurprises) if (g != null) g.SetActive(false);
            if (buzzerLight != null) buzzerLight.color = new Color(1f, 1f, 1f, 0f);
            UpdateTries();
            Say("Final question! Pick a door!", 2.5f);
        }

        void UpdateTries() { if (triesText != null) triesText.text = "WRONG ANSWERS " + tries + "/" + MaxTries; }

        protected override void Tick()
        {
            if (busy) return;
            if (!Pointer(out Vector2 w, out bool began, out bool ended)) { if (grabbing) StartCoroutine(Release()); return; }
            if (began)
            {
                if (questionArea != null && questionArea.OverlapPoint(w))
                {
                    grabbing = true;
                    grabOffset = (Vector2)question.position - w;
                    Sfx.Play(this, "tick", 0.5f);
                }
                else
                {
                    for (int i = 0; i < doorAreas.Length; i++)
                        if (doorAreas[i] != null && doorAreas[i].OverlapPoint(w)) { StartCoroutine(OpenDoor(i)); return; }
                }
            }
            if (grabbing)
            {
                Vector2 p = w + grabOffset;
                question.position = new Vector3(Mathf.Clamp(p.x, -1.8f, 1.8f), Mathf.Clamp(p.y, -4.2f, 2.4f), question.position.z);
                question.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(clock * 8f) * 3f);
            }
            if (ended && grabbing) StartCoroutine(Release());
        }

        IEnumerator OpenDoor(int i)
        {
            busy = true;
            tries++;
            UpdateTries();
            Sfx.Play(this, "clunk", 0.6f);
            if (doorArts[i] != null) doorArts[i].sprite = doorOpen;
            if (doorSurprises[i] != null) doorSurprises[i].SetActive(true);
            yield return new WaitForSeconds(0.35f);
            Sfx.Play(this, "buzz", 0.7f);
            Haptics.Medium();
            if (buzzerLight != null) buzzerLight.color = new Color(1f, 0.3f, 0.3f, 0.6f);
            Say(Verdicts[i], 2f);
            yield return Shake(doors[i], 0.08f, 0.4f);
            yield return new WaitForSeconds(0.6f);
            if (buzzerLight != null) buzzerLight.color = new Color(1f, 1f, 1f, 0f);
            if (tries >= MaxTries)
            {
                Say("GAME OVER! Thanks for playing!", 2f);
                Sfx.Play(this, "bonk", 0.8f);
                Haptics.Heavy();
                yield return new WaitForSeconds(1.2f);
                Fail();
                yield break;
            }
            if (doorSurprises[i] != null) doorSurprises[i].SetActive(false);
            if (doorArts[i] != null) doorArts[i].sprite = doorClosed;
            Say("One more wrong answer and you're out!", 2.2f);
            busy = false;
        }

        IEnumerator Release()
        {
            busy = true;
            grabbing = false;
            question.localRotation = Quaternion.identity;
            if (Vector3.Distance(question.localPosition, questionHome) < 1.0f)
            {
                // spostata poco: torna al suo chiodo
                yield return Move(question, questionHome, 0.2f);
                busy = false;
                yield break;
            }
            // la domanda cade sul pavimento
            Vector3 p = question.localPosition;
            float v = 0f;
            while (p.y > floorY)
            {
                v += 14f * Time.deltaTime;
                p.y = Mathf.Max(floorY, p.y - v * Time.deltaTime);
                question.localPosition = p;
                yield return null;
            }
            Sfx.Play(this, "clunk", 0.9f);
            Haptics.Medium();
            question.localRotation = Quaternion.Euler(0f, 0f, p.x > 0f ? -6f : 6f);
            question.localScale = new Vector3(1.05f, 0.9f, 1f);
            Say("Wait... what's behind the question?!", 2f);
            yield return Pop(hole, 1.15f, 0.3f);
            yield return new WaitForSeconds(0.3f);
            // il topo salta nell'apertura
            Sfx.Play(this, "whoosh", 0.6f);
            yield return Move(mouse, hole.localPosition + new Vector3(0f, -0.4f, 0f), 0.9f, 1.4f);
            Sfx.Play(this, "pop", 0.8f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.3f) { mouse.localScale = Vector3.one * (1f - k * 0.6f); yield return null; }
            mouse.gameObject.SetActive(false);
            Say("The answer was: NO ANSWER!", 2.2f);
            yield return Celebrate();
        }
    }
}
