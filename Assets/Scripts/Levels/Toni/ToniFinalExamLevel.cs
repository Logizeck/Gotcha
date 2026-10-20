using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello di Toni "TONI'S FINAL EXAM" — Toni, seduto sulla cattedra con gli occhialoni, affronta tre prove.
    /// Matematica: 2 + 2 = ? → BISCUIT. Memoria: si vedono osso, pallina e biscotto, poi spariscono sotto i bicchieri:
    /// Toni ricorda sempre il biscotto. Intelligenza: il diploma è appeso troppo in alto: va trascinato giù fino a
    /// Toni (non è Toni che deve muoversi). Due errori fanno ricominciare l'esame; alla seconda ripartenza = sconfitta.
    /// Finale: diploma "CERTIFIED GOOD BOY", tocco in testa, ruttone e professore svenuto.
    /// </summary>
    public class ToniFinalExamLevel : LevelController
    {
        [SerializeField] ToniPug toni;
        [SerializeField] TextMesh boardText, mistakesText, burpText;
        [SerializeField] Collider2D[] answerButtons;
        [SerializeField] TextMesh[] answerLabels;
        [SerializeField] Transform[] memItems;      // 0 osso, 1 pallina, 2 biscotto
        [SerializeField] Transform[] cups;
        [SerializeField] Collider2D[] cupAreas;
        [SerializeField] Transform diploma, mortarboard, professor, bigFace;
        [SerializeField] Collider2D diplomaArea;
        [SerializeField] SpeechBubble profBubble;
        [SerializeField] SmokeEmitter[] confetti;
        [SerializeField] int mistakesAllowed = 2, restartsAllowed = 2;

        static readonly string[][] Answers = { new[] { "3", "4", "BISCUIT" }, null, new[] { "JUMP!", "STRETCH", "BARK" } };

        Camera cam;
        int test, mistakes, restarts, biscuitCup;
        bool busy, dragging;
        Vector3 diplomaHome, mortarHome;
        Vector3[] cupHome;
        Vector2 dragOffset;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            diplomaHome = diploma.position;
            mortarHome = mortarboard.localPosition;
            cupHome = new Vector3[cups.Length];
            for (int i = 0; i < cups.Length; i++) cupHome[i] = cups[i].localPosition;
            mortarboard.gameObject.SetActive(false);
            bigFace.gameObject.SetActive(false);
            burpText.text = "";
            toni.Stand(false);
            toni.WakeUp(2);
            toni.Wag = 0.6f;
            S41LKit.Confetti(confetti, false);
            StartCoroutine(BeginExam(true));
        }

        void UpdateMistakes() { mistakesText.text = "MISTAKES " + mistakes + "/" + mistakesAllowed; }

        void ShowButtons(string[] labels)
        {
            for (int i = 0; i < answerButtons.Length; i++)
            {
                answerButtons[i].gameObject.SetActive(labels != null);
                if (labels != null) answerLabels[i].text = labels[i];
            }
        }

        IEnumerator BeginExam(bool first)
        {
            busy = true;
            test = 0;
            mistakes = 0;
            UpdateMistakes();
            diploma.position = diplomaHome;
            diploma.gameObject.SetActive(false);
            foreach (Transform m in memItems) m.gameObject.SetActive(false);
            for (int i = 0; i < cups.Length; i++) { cups[i].gameObject.SetActive(false); cups[i].localPosition = cupHome[i]; }
            ShowButtons(null);
            S41LKit.Say(profBubble, first ? "Final exam! Three tests." : "From the top. Test one. AGAIN.");
            if (first) toni.Say("I am a GENIUS.", 1.6f);
            yield return new WaitForSeconds(1.6f);
            yield return StartTest(0);
        }

        IEnumerator StartTest(int n)
        {
            busy = true;
            test = n;
            if (n == 0)
            {
                boardText.text = "TEST 1: MATHS\n2 + 2 = ?";
                ShowButtons(Answers[0]);
            }
            else if (n == 1)
            {
                ShowButtons(null);
                boardText.text = "TEST 2: MEMORY\nREMEMBER THESE!";
                // ordine a caso: osso, pallina, biscotto
                int[] order = { 0, 1, 2 };
                for (int i = 2; i > 0; i--) { int j = Random.Range(0, i + 1); (order[i], order[j]) = (order[j], order[i]); }
                for (int i = 0; i < 3; i++)
                {
                    Transform it = memItems[order[i]];
                    it.position = new Vector3(cups[i].position.x, cups[i].position.y + 0.4f, it.position.z);
                    it.gameObject.SetActive(true);
                    if (order[i] == 2) biscuitCup = i;
                }
                yield return new WaitForSeconds(2.4f);
                // i bicchieri scendono e coprono tutto
                for (int i = 0; i < cups.Length; i++)
                {
                    cups[i].gameObject.SetActive(true);
                    cups[i].localPosition = cupHome[i] + Vector3.up * 2f;
                    StartCoroutine(S41LKit.Move(cups[i], cupHome[i], 0.35f));
                }
                yield return new WaitForSeconds(0.4f);
                Sfx.Play(this, "clunk", 0.8f);
                foreach (Transform m in memItems) m.gameObject.SetActive(false);
                boardText.text = "TEST 2: MEMORY\nWHICH ONE DOES TONI REMEMBER?";
            }
            else
            {
                foreach (Transform c in cups) c.gameObject.SetActive(false);
                boardText.text = "TEST 3: INTELLIGENCE\nGET THE DIPLOMA";
                diploma.position = diplomaHome;
                diploma.gameObject.SetActive(true);
                ShowButtons(Answers[2]);
            }
            busy = false;
        }

        void Update()
        {
            if (IsFinished || busy) return;
            bool down = S41LKit.Pointer(cam, out Vector2 w, out bool began, out bool ended);
            if (began)
            {
                if (test == 2 && diplomaArea.OverlapPoint(w)) { dragging = true; dragOffset = (Vector2)diploma.position - w; Sfx.Play(this, "tick", 0.5f); return; }
                for (int i = 0; i < answerButtons.Length; i++)
                    if (answerButtons[i].gameObject.activeSelf && answerButtons[i].OverlapPoint(w)) { StartCoroutine(Answer(i)); return; }
                if (test == 1)
                    for (int i = 0; i < cupAreas.Length; i++)
                        if (cupAreas[i].OverlapPoint(w)) { StartCoroutine(PickCup(i)); return; }
            }
            if (dragging)
            {
                Vector2 p = w + dragOffset;
                diploma.position = new Vector3(Mathf.Clamp(p.x, -2.3f, 2.3f), Mathf.Clamp(p.y, -3.6f, 3.0f), diploma.position.z);
                if (ended || !down)
                {
                    dragging = false;
                    if (Vector2.Distance(diploma.position, toni.HeadWorld) < 1.3f) StartCoroutine(Finale());
                    else StartCoroutine(S41LKit.Move(diploma, diploma.parent.InverseTransformPoint(diplomaHome), 0.35f));
                }
            }
        }

        IEnumerator Answer(int i)
        {
            busy = true;
            StartCoroutine(S41LKit.Pop(answerButtons[i].transform, 0.15f, 0.2f));
            if (test == 0)
            {
                if (i == 2) { yield return Correct("BISCUIT! Obviously."); yield return StartTest(1); }
                else yield return Wrong(i == 1 ? "4?! Too obvious for a genius." : "3? Not even close.");
            }
            else
            {
                // prova 3: Toni prova a saltare / allungarsi / abbaiare... niente da fare
                string[] fails = { "Toni jumps... 2 cm.", "Toni stretches... and yawns.", "WOOF! The diploma ignores him." };
                if (i == 0) StartCoroutine(toni.Jump(0.2f, 0.3f));
                if (i == 2) toni.Bark();
                yield return Wrong(fails[i]);
            }
            if (!IsFinished) busy = false;
        }

        IEnumerator PickCup(int i)
        {
            busy = true;
            Vector3 up = cupHome[i] + Vector3.up * 0.8f;
            yield return StartCoroutine(S41LKit.Move(cups[i], up, 0.25f));
            if (i == biscuitCup)
            {
                memItems[2].gameObject.SetActive(true);
                yield return Correct("The biscuit, of course. Always the biscuit.");
                yield return StartTest(2);
            }
            else
            {
                yield return Wrong("Toni only remembers food. Duh.");
                yield return StartCoroutine(S41LKit.Move(cups[i], cupHome[i], 0.2f));
            }
            if (!IsFinished) busy = false;
        }

        IEnumerator Correct(string line)
        {
            Sfx.Play(this, "ding", 0.8f);
            Haptics.Light();
            S41LKit.Say(profBubble, line);
            toni.Wag = 1f;
            yield return new WaitForSeconds(1.4f);
            toni.Wag = 0.6f;
        }

        IEnumerator Wrong(string line)
        {
            mistakes++;
            UpdateMistakes();
            Sfx.Play(this, "buzz", 0.7f);
            Haptics.Medium();
            S41LKit.Say(profBubble, line);
            StartCoroutine(S41LKit.Shake(professor, 0.08f, 0.4f));
            yield return new WaitForSeconds(1.4f);
            if (mistakes < mistakesAllowed) yield break;
            restarts++;
            if (restarts >= restartsAllowed)
            {
                S41LKit.Say(profBubble, "FAILED! Come back next semester.");
                toni.Sleep();
                yield return new WaitForSeconds(1.6f);
                Fail();
                yield break;
            }
            yield return BeginExam(false);
        }

        IEnumerator Finale()
        {
            busy = true;
            ShowButtons(null);
            boardText.text = "CERTIFIED\nGOOD BOY";
            Sfx.Play(this, "ding", 1f);
            Haptics.Medium();
            yield return StartCoroutine(S41LKit.Move(diploma, diploma.parent.InverseTransformPoint(toni.HeadWorld + new Vector3(0.9f, -0.2f, 0f)), 0.4f, 0.2f));
            S41LKit.Say(profBubble, "Congratulations... I guess.");
            // il tocco cade dall'alto sulla testa
            mortarboard.gameObject.SetActive(true);
            mortarboard.localPosition = mortarHome + Vector3.up * 3f;
            yield return StartCoroutine(S41LKit.Move(mortarboard, mortarHome, 0.45f));
            Sfx.Play(this, "bonk", 0.8f);
            S41LKit.Confetti(confetti, true);
            yield return StartCoroutine(toni.Happy(1.2f));
            // si gira verso il giocatore... e rutta
            bigFace.gameObject.SetActive(true);
            Vector3 fs = bigFace.localScale;
            bigFace.localScale = fs * 0.2f;
            yield return StartCoroutine(S41LKit.ScaleTo(bigFace, fs, 0.3f));
            yield return new WaitForSeconds(0.5f);
            burpText.text = "BUUUUURP!";
            Sfx.Play(this, "buzz", 1f);
            Sfx.Play(this, "snore", 1f);
            Haptics.Heavy();
            StartCoroutine(S41LKit.Pop(burpText.transform, 0.4f, 0.4f));
            StartCoroutine(S41LKit.Shake(bigFace, 0.2f, 0.6f));
            yield return new WaitForSeconds(0.8f);
            bigFace.gameObject.SetActive(false);
            // il professore sviene
            profBubble.Hide();
            Vector3 pp = professor.localPosition;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.5f)
            {
                professor.localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(0f, 88f, k * k));
                professor.localPosition = pp + Vector3.down * 0.15f * k;
                yield return null;
            }
            Sfx.Play(this, "bonk", 1f);
            Haptics.Medium();
            yield return new WaitForSeconds(1.5f);
            S41LKit.Confetti(confetti, false);
            Win();
        }
    }
}
