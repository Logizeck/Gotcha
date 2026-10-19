using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "Robot factory" — due fasi. 1) Sul nastro passano dei robot a cui manca un pezzo (testa, braccio o
    /// gambe, indicato anche dal progetto blu): trascina dalla cassetta il pezzo giusto sul robot mentre passa. Pezzo
    /// sbagliato = il robot cade a pezzi e ne arriva un altro; servono 3 robot completi. 2) Programma il robot: griglia
    /// 5×5 con casse, 6 caselle di programma in cui trascinare le carte AVANTI / SINISTRA / DESTRA, poi RUN. Se sbatte
    /// o non arriva, torna al via (il programma resta). Ogni griglia ha una sola soluzione da 6 comandi (es. FFFRFF).
    /// Arrivato alla pila il robot si ricarica e balla.
    /// </summary>
    public class RobotFactoryLevel : LevelController
    {
        [SerializeField] TextMesh goalText;
        [SerializeField] SpriteRenderer[] dots;          // 2 fasi
        [SerializeField] GameObject stage1;
        [SerializeField] GameObject stage2;
        // fase 1
        [SerializeField] TextMesh counter;
        [SerializeField] GameObject[] blueprintIcons;    // testa, braccio, gambe
        [SerializeField] TextMesh blueprintLabel;
        [SerializeField] Transform[] cleats;             // tacche del nastro che scorrono
        [SerializeField] Transform body;
        [SerializeField] Collider2D bodyArea;
        [SerializeField] SpriteRenderer[] parts;         // torso, testa, braccio sx, braccio dx, gambe
        [SerializeField] Collider2D[] bins;              // testa, braccio, gambe
        [SerializeField] Sprite[] partSprites;           // testa, braccio, gambe
        [SerializeField] Transform held;
        [SerializeField] SpriteRenderer heldArt;
        [SerializeField] float beltSpeed = 0.75f, beltFrom = -3.9f, beltTo = 3.9f;
        // fase 2
        [SerializeField] Transform[] crates;             // 7 casse
        [SerializeField] Transform battery;
        [SerializeField] Transform bot;
        [SerializeField] Collider2D[] slots;             // 6 caselle del programma
        [SerializeField] SpriteRenderer[] slotCards;
        [SerializeField] Collider2D[] palette;           // avanti, sinistra, destra
        [SerializeField] Sprite[] cardSprites;
        [SerializeField] Transform heldCard;
        [SerializeField] Collider2D runButton;
        [SerializeField] SmokeEmitter sparks;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] float cell = 0.76f;
        [SerializeField] Vector2 gridCenter = new Vector2(0f, 1.15f);

        struct Layout { public Vector2Int start, bat; public int dir; public Vector2Int[] obs; }
        static readonly Layout[] Layouts =
        {
            new Layout { start = new Vector2Int(0, 0), dir = 0, bat = new Vector2Int(2, 3),
                obs = new[] { new Vector2Int(1, 0), new Vector2Int(1, 2), new Vector2Int(0, 4), new Vector2Int(3, 3), new Vector2Int(2, 1), new Vector2Int(3, 0), new Vector2Int(4, 2) } },
            new Layout { start = new Vector2Int(4, 0), dir = 0, bat = new Vector2Int(1, 2),
                obs = new[] { new Vector2Int(3, 1), new Vector2Int(4, 3), new Vector2Int(2, 1), new Vector2Int(1, 3), new Vector2Int(0, 1), new Vector2Int(2, 3), new Vector2Int(3, 4) } },
            new Layout { start = new Vector2Int(2, 0), dir = 0, bat = new Vector2Int(4, 3),
                obs = new[] { new Vector2Int(1, 1), new Vector2Int(3, 1), new Vector2Int(2, 4), new Vector2Int(4, 2), new Vector2Int(0, 3), new Vector2Int(3, 4), new Vector2Int(4, 0) } },
        };
        static readonly Vector2Int[] Dirs = { new Vector2Int(0, 1), new Vector2Int(1, 0), new Vector2Int(0, -1), new Vector2Int(-1, 0) };
        static readonly string[] PartNames = { "HEAD", "ARM", "LEGS" };
        static readonly Color Ghost = new Color(0.25f, 0.3f, 0.45f, 0.35f);
        static readonly Color Done = new Color(0.42f, 0.88f, 0.48f);

        int stage, built, missing, heldType = -1, cardType = -1, missingArm;
        bool busy, bodyActive, bodyFinished;
        float bubbleUntil, clock;
        Vector3[] partHome;
        Vector2 pressAt;
        // fase 2
        Layout lay;
        int[] program = { -1, -1, -1, -1, -1, -1 };
        Camera cam;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            partHome = new Vector3[parts.Length];
            for (int i = 0; i < parts.Length; i++) partHome[i] = parts[i].transform.localPosition;
            stage1.SetActive(true);
            stage2.SetActive(false);
            held.gameObject.SetActive(false);
            heldCard.gameObject.SetActive(false);
            foreach (SpriteRenderer d in dots) d.color = new Color(1f, 1f, 1f, 0.5f);
            dots[0].color = Color.white;
            goalText.text = "Give each robot its missing part!";
            counter.text = "ROBOTS: 0 / 3";
            NewBody();
        }

        void Say(string text, float secs)
        {
            if (bubble == null) return;
            bubble.Show(text);
            bubbleUntil = clock + secs;
        }

        void Update()
        {
            if (IsFinished) return;
            clock += Time.deltaTime;
            if (bubble != null && bubbleUntil > 0f && clock > bubbleUntil) { bubble.Hide(); bubbleUntil = 0f; }
            if (stage == 0) UpdateBelt();
            else if (!busy) UpdateProgram();
        }

        // ---------- Fase 1: il nastro ----------

        void NewBody()
        {
            missing = Random.Range(0, 3);
            missingArm = Random.Range(0, 2);
            for (int i = 0; i < parts.Length; i++)
            {
                parts[i].transform.localPosition = partHome[i];
                parts[i].transform.localRotation = Quaternion.identity;
                parts[i].color = Color.white;
            }
            SpriteRenderer gap = MissingRenderer();
            gap.color = Ghost;
            for (int i = 0; i < blueprintIcons.Length; i++) blueprintIcons[i].SetActive(i == missing);
            blueprintLabel.text = "NEEDS: " + PartNames[missing];
            body.localPosition = new Vector3(beltFrom, body.localPosition.y, 0f);
            body.gameObject.SetActive(true);
            bodyActive = true;
            bodyFinished = false;
        }

        SpriteRenderer MissingRenderer() => missing == 0 ? parts[1] : missing == 1 ? parts[2 + missingArm] : parts[4];

        void UpdateBelt()
        {
            float speed = bodyFinished ? beltSpeed * 3f : beltSpeed;
            foreach (Transform c in cleats)
            {
                Vector3 p = c.localPosition;
                p.x += beltSpeed * Time.deltaTime;
                if (p.x > 3.2f) p.x -= 6.4f;
                c.localPosition = p;
            }
            if (bodyActive)
            {
                body.localPosition += Vector3.right * speed * Time.deltaTime;
                if (body.localPosition.x > beltTo)
                {
                    bodyActive = false;
                    if (!bodyFinished) Say("Missed one!", 1.2f);
                    if (built >= 3) { StartCoroutine(ToStage2()); return; }
                    NewBody();
                }
            }
            if (busy) return;

            bool has = PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out bool ended);
            Vector2 w = has ? PointerInput.ToWorld(cam, sp) : Vector2.zero;
            if (heldType >= 0)
            {
                if (has) held.position = new Vector3(w.x, w.y, held.position.z);
                if (!has || ended)
                {
                    int type = heldType;
                    heldType = -1;
                    held.gameObject.SetActive(false);
                    bool over = bodyActive && !bodyFinished && (bodyArea.OverlapPoint(held.position) || Vector2.Distance(held.position, body.position) < 1.1f);
                    if (over) Attach(type);
                }
                return;
            }
            if (!has || !began || PointerInput.IsOverUi()) return;
            for (int i = 0; i < bins.Length; i++)
            {
                if (!bins[i].OverlapPoint(w)) continue;
                heldType = i;
                heldArt.sprite = partSprites[i];
                held.position = new Vector3(w.x, w.y, held.position.z);
                held.gameObject.SetActive(true);
                Sfx.Play(this, "tick", 0.5f);
                return;
            }
        }

        void Attach(int type)
        {
            if (type == missing)
            {
                MissingRenderer().color = Color.white;
                bodyFinished = true;
                built++;
                counter.text = "ROBOTS: " + built + " / 3";
                Sfx.Play(this, "clunk", 0.8f);
                Sfx.Play(this, "ding", 0.4f);
                Haptics.Light();
                StartCoroutine(Hop(body));
                if (built >= 3) Say("Three robots! Nice!", 1.5f);
            }
            else StartCoroutine(FallApart());
        }

        IEnumerator Hop(Transform t)
        {
            Vector3 s = t.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.25f)
            {
                t.localScale = new Vector3(s.x * (1f + Mathf.Sin(k * Mathf.PI) * 0.12f), s.y * (1f - Mathf.Sin(k * Mathf.PI) * 0.1f), 1f);
                yield return null;
            }
            t.localScale = s;
        }

        IEnumerator FallApart()
        {
            busy = true;
            bodyActive = false;
            Say("Wrong part! It fell apart...", 1.6f);
            Sfx.Play(this, "bonk", 0.9f);
            Haptics.Medium();
            var vel = new Vector2[parts.Length];
            var spin = new float[parts.Length];
            for (int i = 0; i < parts.Length; i++) { vel[i] = new Vector2(Random.Range(-1.5f, 1.5f), Random.Range(1.5f, 3f)); spin[i] = Random.Range(-400f, 400f); }
            bool landed = false;
            for (float k = 0f; k < 1.1f; k += Time.deltaTime)
            {
                for (int i = 0; i < parts.Length; i++)
                {
                    vel[i].y -= 12f * Time.deltaTime;
                    Transform p = parts[i].transform;
                    p.localPosition += (Vector3)(vel[i] * Time.deltaTime);
                    p.localRotation = Quaternion.Euler(0f, 0f, p.localEulerAngles.z + spin[i] * Time.deltaTime);
                }
                if (!landed && k > 0.45f) { landed = true; Sfx.Play(this, "clunk", 0.8f); Haptics.Medium(); }
                yield return null;
            }
            body.gameObject.SetActive(false);
            yield return new WaitForSeconds(0.3f);
            NewBody();
            busy = false;
        }

        IEnumerator ToStage2()
        {
            stage = 1;
            busy = true;
            dots[0].color = Done;
            dots[1].color = Color.white;
            Sfx.Play(this, "ding", 0.6f);
            goalText.text = "Now program the robot!";
            yield return new WaitForSeconds(0.6f);
            stage1.SetActive(false);
            stage2.SetActive(true);
            lay = Layouts[Random.Range(0, Layouts.Length)];
            for (int i = 0; i < crates.Length; i++)
            {
                crates[i].localPosition = CellPos(lay.obs[i]);
                StartCoroutine(Hop(crates[i]));
            }
            battery.localPosition = CellPos(lay.bat);
            ResetBot();
            for (int i = 0; i < slotCards.Length; i++) slotCards[i].gameObject.SetActive(false);
            goalText.text = "Program the robot to reach the battery!";
            Say("Drag cards into the slots, then RUN!", 2.5f);
            busy = false;
        }

        // ---------- Fase 2: il programma ----------

        Vector3 CellPos(Vector2Int c) => new Vector3(gridCenter.x + (c.x - 2) * cell, gridCenter.y + (c.y - 2) * cell, 0f);

        void ResetBot()
        {
            bot.localPosition = CellPos(lay.start);
            bot.localRotation = Quaternion.Euler(0f, 0f, -90f * lay.dir);
            bot.localScale = Vector3.one;
        }

        void UpdateProgram()
        {
            bool has = PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out bool ended);
            Vector2 w = has ? PointerInput.ToWorld(cam, sp) : Vector2.zero;
            if (cardType >= 0)
            {
                if (has) heldCard.position = new Vector3(w.x, w.y, heldCard.position.z);
                if (!has || ended)
                {
                    int type = cardType;
                    cardType = -1;
                    heldCard.gameObject.SetActive(false);
                    Vector2 at = heldCard.position;
                    if (Vector2.Distance(at, pressAt) < 0.25f) { PutCard(FirstEmpty(), type); return; }   // tocco: va nella prima casella libera
                    int best = -1;
                    float bestD = 0.7f;
                    for (int i = 0; i < slots.Length; i++)
                    {
                        float d = Vector2.Distance(at, slots[i].transform.position);
                        if (d < bestD) { bestD = d; best = i; }
                    }
                    PutCard(best, type);
                }
                return;
            }
            if (!has || !began || PointerInput.IsOverUi()) return;
            if (runButton.OverlapPoint(w)) { StartCoroutine(Run()); return; }
            for (int i = 0; i < palette.Length; i++)
            {
                if (!palette[i].OverlapPoint(w)) continue;
                cardType = i;
                pressAt = w;
                heldCard.GetComponentInChildren<SpriteRenderer>().sprite = cardSprites[i];
                heldCard.position = new Vector3(w.x, w.y, heldCard.position.z);
                heldCard.gameObject.SetActive(true);
                Sfx.Play(this, "tick", 0.5f);
                return;
            }
            for (int i = 0; i < slots.Length; i++)
            {
                if (!slots[i].OverlapPoint(w) || program[i] < 0) continue;
                program[i] = -1;
                slotCards[i].gameObject.SetActive(false);
                Sfx.Play(this, "pop", 0.5f);
                return;
            }
        }

        int FirstEmpty()
        {
            for (int i = 0; i < program.Length; i++) if (program[i] < 0) return i;
            return -1;
        }

        void PutCard(int slot, int type)
        {
            if (slot < 0) { Sfx.Play(this, "tick", 0.3f); return; }
            program[slot] = type;
            slotCards[slot].sprite = cardSprites[type];
            slotCards[slot].gameObject.SetActive(true);
            Sfx.Play(this, "clunk", 0.5f);
            StartCoroutine(Hop(slotCards[slot].transform));
        }

        IEnumerator Run()
        {
            busy = true;
            Sfx.Play(this, "clunk", 0.7f);
            bool any = false;
            foreach (int c in program) if (c >= 0) any = true;
            if (!any)
            {
                Say("The program is empty!", 1.5f);
                busy = false;
                yield break;
            }
            Vector2Int pos = lay.start;
            int dir = lay.dir;
            for (int i = 0; i < program.Length; i++)
            {
                if (program[i] < 0) continue;
                Transform card = slotCards[i].transform;
                card.localScale = Vector3.one * 1.2f;
                if (program[i] == 0)
                {
                    Vector2Int next = pos + Dirs[dir];
                    bool blocked = next.x < 0 || next.x > 4 || next.y < 0 || next.y > 4;
                    foreach (Vector2Int o in lay.obs) if (o == next) blocked = true;
                    Vector3 from = CellPos(pos), to = CellPos(next);
                    if (blocked)
                    {
                        for (float k = 0f; k < 1f; k += Time.deltaTime / 0.15f) { bot.localPosition = Vector3.Lerp(from, to, k * 0.35f); yield return null; }
                        Sfx.Play(this, "bonk", 1f);
                        Haptics.Medium();
                        sparks.transform.localPosition = Vector3.Lerp(from, to, 0.5f);
                        sparks.Play();
                        for (float k = 0f; k < 1f; k += Time.deltaTime / 0.4f)
                        {
                            bot.localPosition = Vector3.Lerp(from, to, 0.35f * (1f - k)) + (Vector3)(Random.insideUnitCircle * 0.04f);
                            yield return null;
                        }
                        sparks.Stop();
                        card.localScale = Vector3.one;
                        Say("BONK! Back to the start.", 1.6f);
                        yield return new WaitForSeconds(0.6f);
                        ResetBot();
                        busy = false;
                        yield break;
                    }
                    for (float k = 0f; k < 1f; k += Time.deltaTime / 0.35f) { bot.localPosition = Vector3.Lerp(from, to, k); yield return null; }
                    bot.localPosition = to;
                    pos = next;
                    Sfx.Play(this, "tick", 0.6f);
                    if (pos == lay.bat) { card.localScale = Vector3.one; StartCoroutine(Charge()); yield break; }
                }
                else
                {
                    int ndir = (dir + (program[i] == 1 ? 3 : 1)) % 4;
                    float a0 = -90f * dir, a1 = a0 + (program[i] == 1 ? 90f : -90f);
                    for (float k = 0f; k < 1f; k += Time.deltaTime / 0.25f) { bot.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(a0, a1, k)); yield return null; }
                    dir = ndir;
                    bot.localRotation = Quaternion.Euler(0f, 0f, -90f * dir);
                    Sfx.Play(this, "tick", 0.4f);
                }
                yield return new WaitForSeconds(0.1f);
                card.localScale = Vector3.one;
            }
            Say("Didn't reach the battery. Try again!", 1.8f);
            Sfx.Play(this, "buzz", 0.5f);
            yield return new WaitForSeconds(0.8f);
            ResetBot();
            busy = false;
        }

        IEnumerator Charge()
        {
            dots[1].color = Done;
            goalText.text = "Fully charged!";
            Sfx.Play(this, "ding", 0.8f);
            Vector3 bs = battery.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.8f)
            {
                battery.localScale = bs * (1f - k);
                bot.localScale = Vector3.one * (1f + Mathf.Sin(k * Mathf.PI * 4f) * 0.12f);
                yield return null;
            }
            battery.gameObject.SetActive(false);
            Say("Battery 100%! Dance time!", 2f);
            Vector3 home = bot.localPosition;
            for (float k = 0f; k < 2.2f; k += Time.deltaTime)
            {
                bot.localPosition = home + Vector3.up * Mathf.Abs(Mathf.Sin(k * 9f)) * 0.25f;
                bot.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(k * 9f) * 25f + k * 160f);
                if (Mathf.Repeat(k, 0.35f) < Time.deltaTime) Sfx.Play(this, "pop", 0.5f);
                yield return null;
            }
            bot.localPosition = home;
            Win();
        }
    }
}
