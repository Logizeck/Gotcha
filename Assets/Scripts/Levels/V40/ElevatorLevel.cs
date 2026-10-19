using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "Elevator logic" — un palazzo di 5 piani, quattro animali (gatto, cane, pappagallo, coniglio) nell'atrio:
    /// ognuno abita a un piano diverso e il foglio degli indizi (uno di tre rompicapi con soluzione unica) dice quale.
    /// Si trascina (o si tocca) un animale nella cabina (2 posti), si tocca un piano per farla salire, si tocca un
    /// animale nella cabina per farlo scendere. Trucco: gatto e cane non viaggiano insieme (soffio, e il secondo esce).
    /// Quando tutti e quattro sono sistemati il palazzo si illumina: verde chi è giusto, rosso chi è sbagliato (torna
    /// nell'atrio e si perde un tentativo, 3 in tutto). Soluzioni: A gatto 4, cane 2, pappagallo 1, coniglio 5;
    /// B gatto 5, cane 3, pappagallo 4, coniglio 1; C gatto 3, cane 1, pappagallo 5, coniglio 2.
    /// </summary>
    public class ElevatorLevel : LevelController
    {
        [SerializeField] Transform car;
        [SerializeField] Transform[] animals;            // 0 gatto, 1 cane, 2 pappagallo, 3 coniglio (piedi nell'origine)
        [SerializeField] Collider2D[] floorAreas;        // 0 atrio, 1..5 piani
        [SerializeField] SpriteRenderer[] floorLights;   // velo colorato di ogni piano (0 = atrio)
        [SerializeField] TextMesh clueText;
        [SerializeField] SpriteRenderer[] dots;
        [SerializeField] SpriteRenderer[] hearts;
        [SerializeField] TextMesh goal;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] float baseY = -4.25f, floorH = 0.95f, carX = -1.975f;

        static readonly string[][] Clues =
        {
            new[] { "The parrot lives lower than everyone.", "The dog doesn't live on floor 3.", "The cat lives above the dog.",
                    "The bunny lives on an odd floor.", "The bunny lives right above the cat." },
            new[] { "The dog doesn't live on floor 1.", "The dog and the parrot live on next-door floors.", "The parrot lives on an even floor.",
                    "The cat lives right above the parrot.", "The bunny lives on an odd floor." },
            new[] { "The bunny and the parrot are NOT next-door.", "The dog lives lower than everyone.", "The parrot lives on an odd floor.",
                    "The bunny lives right above the dog.", "The cat doesn't live on floor 4." },
        };
        static readonly int[][] Solutions = { new[] { 4, 2, 1, 5 }, new[] { 5, 3, 4, 1 }, new[] { 3, 1, 5, 2 } };
        static readonly float[] SlotX = { -0.75f, 0f, 0.75f, 1.5f };
        const int InCar = -1;

        Camera cam;
        int puzzle, carFloor, tries = 3;
        int[] where, carSeat;
        bool[] locked;
        bool busy;
        int grabbed = -1;
        Vector2 grabStart, grabOffset;
        bool dragging;
        float bubbleUntil, clock;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            puzzle = Random.Range(0, Clues.Length);
            if (clueText != null)
            {
                string s = "";
                for (int i = 0; i < Clues[puzzle].Length; i++) s += (i > 0 ? "\n" : "") + (i + 1) + ". " + Clues[puzzle][i];
                clueText.text = s;
            }
            where = new int[animals.Length];
            locked = new bool[animals.Length];
            carSeat = new[] { -1, -1 };
            carFloor = 0;
            car.localPosition = new Vector3(carX, FloorY(0), 0f);
            for (int i = 0; i < animals.Length; i++) animals[i].localPosition = FloorSpot(i, 0);
            foreach (SpriteRenderer l in floorLights) if (l != null) l.color = Color.clear;
            E40Kit.Hearts(hearts, tries);
            RefreshDots();
            UpdateGoal();
            Say("Everyone needs to get home!", 2.2f);
        }

        float FloorY(int f) => baseY + f * floorH + 0.04f;
        Vector3 FloorSpot(int i, int f) => new Vector3(SlotX[i], FloorY(f) + 0.06f, 0f);
        Vector3 SeatSpot(int seat) => new Vector3(carX + (seat == 0 ? -0.26f : 0.26f), car.localPosition.y + 0.09f, 0f);

        void Say(string text, float seconds)
        {
            if (bubble == null) return;
            bubble.Show(text);
            bubbleUntil = clock + seconds;
        }

        void RefreshDots()
        {
            if (dots == null) return;
            for (int i = 0; i < dots.Length; i++)
                if (dots[i] != null) dots[i].color = locked[i] ? new Color(0.36f, 0.85f, 0.42f) : new Color(1f, 1f, 1f, 0.35f);
        }

        void UpdateGoal()
        {
            int inCar = (carSeat[0] >= 0 ? 1 : 0) + (carSeat[1] >= 0 ? 1 : 0);
            if (inCar == 0) E40Kit.Goal(goal, carFloor == 0 ? "Drag an animal into the elevator" : "Tap a floor to call the elevator");
            else if (carFloor == 0) E40Kit.Goal(goal, "Tap a floor to ride up (2 seats)");
            else E40Kit.Goal(goal, "Tap a rider to let it out here");
        }

        void Update()
        {
            if (IsFinished) return;
            clock += Time.deltaTime;
            if (bubble != null && bubbleUntil > 0f && clock > bubbleUntil) { bubble.Hide(); bubbleUntil = 0f; }
            if (busy) { grabbed = -1; return; }
            if (!PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out bool ended)) { if (grabbed >= 0) Release(grabStart); return; }
            Vector2 w = PointerInput.ToWorld(cam, sp);
            if (began)
            {
                if (PointerInput.IsOverUi()) return;
                grabbed = Pick(w);
                if (grabbed >= 0)
                {
                    grabStart = w;
                    grabOffset = (Vector2)animals[grabbed].position - w;
                    dragging = false;
                    return;
                }
                for (int f = 0; f < floorAreas.Length; f++)
                    if (floorAreas[f] != null && floorAreas[f].OverlapPoint(w)) { TapFloor(f); return; }
                return;
            }
            if (grabbed < 0) return;
            if (!dragging && (w - grabStart).magnitude > 0.18f) dragging = true;
            if (dragging) animals[grabbed].position = (Vector3)(w + grabOffset);
            if (ended) Release(w);
        }

        int Pick(Vector2 w)
        {
            int best = -1;
            float bd = 0.5f;
            for (int i = 0; i < animals.Length; i++)
            {
                if (locked[i]) continue;
                Vector2 c = (Vector2)animals[i].position + Vector2.up * 0.35f;
                float d = Vector2.Distance(c, w);
                if (d < bd) { bd = d; best = i; }
            }
            return best;
        }

        void Release(Vector2 w)
        {
            int i = grabbed;
            grabbed = -1;
            if (i < 0) return;
            if (!dragging) { TapAnimal(i); return; }
            dragging = false;
            bool overCar = Mathf.Abs(w.x - carX) < 0.75f && w.y > car.position.y - 0.3f && w.y < car.position.y + 1.1f;
            if (where[i] != InCar && overCar && where[i] == carFloor) { Enter(i); return; }
            if (where[i] == InCar && !overCar) { Exit(i); return; }
            if (where[i] != InCar && where[i] != carFloor && overCar) Say("The elevator isn't on this floor!", 1.6f);
            StartCoroutine(SlideBack(i));
        }

        IEnumerator SlideBack(int i)
        {
            Vector3 to = where[i] == InCar ? SeatSpot(SeatOf(i)) : FloorSpot(i, where[i]);
            yield return Walk(animals[i], to, 0.18f);
        }

        int SeatOf(int i) => carSeat[0] == i ? 0 : carSeat[1] == i ? 1 : 0;

        void TapAnimal(int i)
        {
            if (where[i] == InCar) { Exit(i); return; }
            if (where[i] == carFloor) { Enter(i); return; }
            StartCoroutine(Ride(where[i]));   // l'ascensore arriva al piano dell'animale
        }

        void TapFloor(int f)
        {
            if (f == carFloor) { StartCoroutine(E40Kit.Pop(car, 0.06f)); return; }
            StartCoroutine(Ride(f));
        }

        void Enter(int i)
        {
            int seat = carSeat[0] < 0 ? 0 : carSeat[1] < 0 ? 1 : -1;
            if (seat < 0)
            {
                Say("Full! Only 2 seats.", 1.5f);
                Sfx.Play(this, "bonk", 0.6f);
                StartCoroutine(SlideBack(i));
                return;
            }
            int other = carSeat[1 - seat];
            if (other >= 0 && ((i == 0 && other == 1) || (i == 1 && other == 0)))
            {
                // gatto e cane non viaggiano insieme
                Say("HISS! WOOF! Cat and dog won't ride together!", 2f);
                Sfx.Play(this, "woof", 0.8f);
                Haptics.Light();
                StartCoroutine(SlideBack(i));
                StartCoroutine(E40Kit.Shake(car, 0.08f));
                return;
            }
            carSeat[seat] = i;
            where[i] = InCar;
            animals[i].SetParent(car, true);
            Sfx.Play(this, "tick", 0.6f);
            StartCoroutine(Walk(animals[i], SeatSpot(seat), 0.2f));
            UpdateGoal();
        }

        void Exit(int i)
        {
            int seat = SeatOf(i);
            carSeat[seat] = -1;
            where[i] = carFloor;
            animals[i].SetParent(transform, true);
            Sfx.Play(this, "pop", 0.5f);
            StartCoroutine(ExitThenCheck(i));
        }

        IEnumerator ExitThenCheck(int i)
        {
            busy = true;
            yield return Walk(animals[i], FloorSpot(i, carFloor), 0.3f);
            busy = false;
            UpdateGoal();
            bool all = true;
            for (int k = 0; k < animals.Length; k++) if (where[k] < 1) all = false;
            if (all) StartCoroutine(Check());
        }

        IEnumerator Walk(Transform t, Vector3 to, float seconds)
        {
            Vector3 from = t.localPosition;
            if (t.parent == car) to = car.InverseTransformPoint(transform.TransformPoint(to));
            for (float k = 0f; k < 1f; k += Time.deltaTime / seconds)
            {
                t.localPosition = Vector3.Lerp(from, to, k) + Vector3.up * Mathf.Abs(Mathf.Sin(k * Mathf.PI * 2f)) * 0.06f;
                yield return null;
            }
            t.localPosition = to;
        }

        IEnumerator Ride(int f)
        {
            busy = true;
            Sfx.Play(this, "whoosh", 0.5f);
            Vector3 from = car.localPosition, to = new Vector3(carX, FloorY(f), 0f);
            float secs = 0.25f + Mathf.Abs(f - carFloor) * 0.22f;
            for (float k = 0f; k < 1f; k += Time.deltaTime / secs)
            {
                car.localPosition = Vector3.Lerp(from, to, Mathf.SmoothStep(0f, 1f, k));
                yield return null;
            }
            car.localPosition = to;
            carFloor = f;
            Sfx.Play(this, "ding", 0.5f);
            busy = false;
            UpdateGoal();
        }

        IEnumerator Check()
        {
            busy = true;
            E40Kit.Goal(goal, "Lights on... let's see!");
            yield return new WaitForSeconds(0.4f);
            int[] sol = Solutions[puzzle];
            bool allRight = true;
            for (int i = 0; i < animals.Length; i++)
            {
                bool ok = where[i] == sol[i];
                if (!ok) allRight = false;
                SpriteRenderer l = floorLights[where[i]];
                if (l != null) l.color = ok ? new Color(0.4f, 1f, 0.4f, 0.45f) : new Color(1f, 0.3f, 0.3f, 0.45f);
                if (ok) locked[i] = true;
                Sfx.Play(this, ok ? "ding" : "buzz", 0.5f);
                yield return new WaitForSeconds(0.35f);
            }
            RefreshDots();
            if (allRight) { yield return Home(); yield break; }

            tries--;
            E40Kit.Hearts(hearts, tries);
            Haptics.Medium();
            if (tries <= 0)
            {
                Say("Nobody's home. Game over!", 2f);
                yield return new WaitForSeconds(1.4f);
                Fail();
                yield break;
            }
            Say(tries == 1 ? "Wrong floors! Last try." : "Some are on the wrong floor!", 2f);
            yield return new WaitForSeconds(1.0f);
            // chi è sbagliato torna giù nell'atrio
            for (int i = 0; i < animals.Length; i++)
            {
                if (locked[i]) continue;
                yield return Teleport(i);
            }
            foreach (SpriteRenderer l in floorLights) if (l != null) l.color = Color.clear;
            for (int i = 0; i < animals.Length; i++)
                if (locked[i] && floorLights[where[i]] != null) floorLights[where[i]].color = new Color(0.4f, 1f, 0.4f, 0.3f);
            if (carFloor != 0) yield return Ride(0);
            busy = false;
            UpdateGoal();
        }

        IEnumerator Teleport(int i)
        {
            Transform a = animals[i];
            Vector3 s = a.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.2f) { a.localScale = s * (1f - k); yield return null; }
            where[i] = 0;
            a.localPosition = FloorSpot(i, 0);
            Sfx.Play(this, "whoosh", 0.4f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.2f) { a.localScale = s * k; yield return null; }
            a.localScale = s;
        }

        IEnumerator Home()
        {
            E40Kit.Goal(goal, "Everybody's home!");
            Say("Home sweet home!", 3f);
            for (int b = 0; b < 6; b++)
            {
                for (int f = 1; f < floorLights.Length; f++)
                    if (floorLights[f] != null) floorLights[f].color = (b + f) % 2 == 0 ? new Color(1f, 0.95f, 0.4f, 0.5f) : new Color(0.4f, 1f, 0.4f, 0.35f);
                foreach (Transform a in animals) StartCoroutine(E40Kit.Pop(a, 0.15f, 0.2f));
                Sfx.Play(this, "pop", 0.5f);
                yield return new WaitForSeconds(0.25f);
            }
            Sfx.Play(this, "ding", 0.8f);
            yield return new WaitForSeconds(1.0f);
            Win();
        }
    }
}
