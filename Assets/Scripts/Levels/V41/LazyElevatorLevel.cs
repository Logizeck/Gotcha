using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "The lazy elevator" — l'ascensore deve portare il topolino al 5° piano (dove c'è il formaggio), ma il
    /// pulsante 5 è rotto: restano 1, 2, 3, 4. Soluzione: trascinare il pulsante 4 sotto l'1 (o un pulsante accanto a un
    /// altro): il display mostra la somma "4+1=5" e l'ascensore sale al 5. Anche 3+2 funziona. Premere i piani
    /// sbagliati fa fare all'ascensore svogliato un giro a vuoto; alla terza chiamata sbagliata si blocca → Fail().
    /// </summary>
    public class LazyElevatorLevel : LevelController
    {
        [SerializeField] Transform car;
        [SerializeField] Transform mouse;
        [SerializeField] Transform cheese;
        [SerializeField] Transform[] buttons;            // pulsanti 1, 2, 3, 4 (in quest'ordine)
        [SerializeField] Collider2D brokenButton;        // il 5 incerottato
        [SerializeField] TextMesh display;
        [SerializeField] TextMesh plusSign;
        [SerializeField] TextMesh strikesLabel;
        [SerializeField] SpriteRenderer[] sparks;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] float groundY = -4.07f, floorHeight = 1.2f, lazySpeed = 0.9f, dragStart = 0.2f, comboDistance = 0.95f;

        Camera cam;
        Vector3[] homes;
        int pressed = -1, wrong;
        bool dragging, busy;
        Vector2 pressAt, grabOffset;
        float clock, bubbleUntil, floorNow;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            homes = new Vector3[buttons.Length];
            for (int i = 0; i < buttons.Length; i++) homes[i] = buttons[i].position;
            foreach (SpriteRenderer s in sparks) s.enabled = false;
            if (plusSign != null) plusSign.gameObject.SetActive(false);
            SetCar(0f);
            ShowFloor("G");
            UpdateStrikes();
            Say("Floor 5, please. The cheese is up there!", 2.4f);
        }

        void Say(string text, float seconds)
        {
            if (bubble == null) return;
            bubble.Show(text);
            bubbleUntil = clock + seconds;
        }

        void ShowFloor(string s) { if (display != null) display.text = s; }

        void UpdateStrikes() { if (strikesLabel != null) strikesLabel.text = "WRONG CALLS: " + wrong + "/3"; }

        void SetCar(float floor)
        {
            floorNow = floor;
            Vector3 p = car.position;
            car.position = new Vector3(p.x, groundY + floor * floorHeight, p.z);
        }

        void Update()
        {
            if (IsFinished) return;
            clock += Time.deltaTime;
            if (bubble != null && bubbleUntil > 0f && clock > bubbleUntil) { bubble.Hide(); bubbleUntil = 0f; }
            if (busy) return;
            if (!S41AKit.Pointer(cam, out Vector2 w, out bool began, out bool ended)) return;

            if (began)
            {
                pressed = -1;
                for (int i = buttons.Length - 1; i >= 0; i--)
                    if (Vector2.Distance(w, buttons[i].position) < 0.5f) { pressed = i; break; }
                if (pressed < 0 && brokenButton != null && brokenButton.OverlapPoint(w)) { StartCoroutine(Broken()); return; }
                if (pressed >= 0) { pressAt = w; grabOffset = (Vector2)buttons[pressed].position - w; dragging = false; Sfx.Play(this, "tick", 0.6f); }
                return;
            }
            if (pressed < 0) return;
            Transform b = buttons[pressed];
            if (!dragging && Vector2.Distance(w, pressAt) > dragStart) { dragging = true; b.localScale *= 1.12f; }
            if (dragging)
            {
                Vector2 to = w + grabOffset;
                to.x = Mathf.Clamp(to.x, -2.5f, 2.5f);
                to.y = Mathf.Clamp(to.y, -4.3f, 3.4f);
                b.position = new Vector3(to.x, to.y, b.position.z);
                int partner = Partner(pressed);
                ShowFloor(partner >= 0 ? (pressed + 1) + "+" + (partner + 1) + "?" : (pressed + 1).ToString());
            }
            if (!ended) return;
            int n = pressed;
            pressed = -1;
            if (!dragging) { StartCoroutine(Call(n + 1)); return; }
            dragging = false;
            buttons[n].localScale /= 1.12f;
            int other = Partner(n);
            if (other < 0) { StartCoroutine(GoHome(n)); ShowFloor(FloorName(floorNow)); return; }
            StartCoroutine(Combine(n, other));
        }

        /// <summary>Pulsante vicino a quello trascinato (non quando lo hai solo lasciato al suo posto).</summary>
        int Partner(int moved)
        {
            Vector3 p = buttons[moved].position;
            if (Vector2.Distance(p, homes[moved]) < 0.35f) return -1;
            int best = -1;
            float bd = comboDistance;
            for (int i = 0; i < buttons.Length; i++)
            {
                if (i == moved) continue;
                float d = Vector2.Distance(p, buttons[i].position);
                if (d < bd) { bd = d; best = i; }
            }
            return best;
        }

        static string FloorName(float f) => f < 0.5f ? "G" : Mathf.RoundToInt(f).ToString();

        IEnumerator GoHome(int i)
        {
            yield return S41AKit.Move(buttons[i], homes[i], 0.2f);
        }

        IEnumerator Broken()
        {
            busy = true;
            foreach (SpriteRenderer s in sparks) { s.enabled = true; StartCoroutine(S41AKit.Bounce(s.transform, 0.5f, 0.2f)); }
            Sfx.Play(this, "buzz", 0.6f);
            Haptics.Light();
            ShowFloor("ERR");
            Say("Button 5 is OUT OF ORDER. Sorry not sorry.", 2f);
            yield return new WaitForSeconds(0.5f);
            foreach (SpriteRenderer s in sparks) s.enabled = false;
            ShowFloor(FloorName(floorNow));
            busy = false;
        }

        IEnumerator Combine(int a, int b)
        {
            busy = true;
            // il pulsante trascinato si aggancia accanto all'altro
            Vector3 dir = buttons[a].position - buttons[b].position;
            if (dir.sqrMagnitude < 0.0001f) dir = Vector3.down;
            Vector3 snap = buttons[b].position + dir.normalized * 0.8f;
            yield return S41AKit.Move(buttons[a], snap, 0.12f);
            Sfx.Play(this, "clunk", 0.7f);
            if (plusSign != null)
            {
                plusSign.gameObject.SetActive(true);
                plusSign.transform.position = (buttons[a].position + buttons[b].position) * 0.5f + new Vector3(0.45f, 0f, -0.1f);
            }
            int sum = a + b + 2;
            ShowFloor((b + 1) + "+" + (a + 1) + "=" + sum);
            yield return new WaitForSeconds(0.6f);
            if (sum == 5) { StartCoroutine(Victory()); yield break; }
            if (plusSign != null) plusSign.gameObject.SetActive(false);
            if (sum > 5)
            {
                Sfx.Play(this, "buzz", 0.6f);
                Say("Floor " + sum + "? This building has FIVE.", 1.8f);
                yield return GoHome(a);
                ShowFloor(FloorName(floorNow));
                busy = false;
                yield break;
            }
            yield return GoHome(a);
            busy = false;
            yield return Call(sum);
        }

        IEnumerator Ride(float to)
        {
            while (Mathf.Abs(floorNow - to) > 0.001f)
            {
                SetCar(Mathf.MoveTowards(floorNow, to, Time.deltaTime * lazySpeed));
                ShowFloor(FloorName(floorNow));
                yield return null;
            }
            SetCar(to);
            ShowFloor(FloorName(to));
        }

        IEnumerator Call(int floor)
        {
            busy = true;
            Sfx.Play(this, "ding", 0.4f);
            Say("*yawn* ...floor " + floor + ", if I must.", 1.6f);
            yield return new WaitForSeconds(0.5f);
            yield return Ride(floor);
            Sfx.Play(this, "ding", 0.6f);
            yield return S41AKit.Bounce(mouse, 0.2f, 0.25f);
            Say("No cheese here!", 1.2f);
            wrong++;
            UpdateStrikes();
            yield return new WaitForSeconds(0.8f);
            if (wrong >= 3) { yield return Stuck(); yield break; }
            Say("Back to the ground floor. Slooowly.", 1.4f);
            yield return Ride(0f);
            busy = false;
        }

        IEnumerator Stuck()
        {
            Say("That's it. I'm on strike.", 1.6f);
            float to = floorNow + 0.5f;
            while (floorNow < to) { SetCar(floorNow + Time.deltaTime * lazySpeed); yield return null; }
            Sfx.Play(this, "clunk", 1f);
            Sfx.Play(this, "buzz", 0.8f);
            Haptics.Heavy();
            ShowFloor("STUCK");
            foreach (SpriteRenderer s in sparks) s.enabled = true;
            Vector3 p = car.position;
            for (float t = 0f; t < 0.6f; t += Time.deltaTime)
            {
                car.position = p + (Vector3)(Random.insideUnitCircle * 0.06f);
                yield return null;
            }
            car.position = p;
            Say("Help! We're STUCK!", 1.6f);
            yield return new WaitForSeconds(1.3f);
            Fail();
        }

        IEnumerator Victory()
        {
            Sfx.Play(this, "ding", 0.9f);
            Haptics.Light();
            Say("FIVE?! Ugh... fine.", 1.4f);
            yield return new WaitForSeconds(0.4f);
            lazySpeed *= 1.6f;
            yield return Ride(5f);
            Sfx.Play(this, "ding", 0.9f);
            // il topolino esce e va al formaggio
            mouse.SetParent(transform, true);
            yield return S41AKit.Move(mouse, new Vector3(cheese.position.x - 0.45f, mouse.position.y, 0f), 0.8f, 0.15f);
            Say("CHEESE!", 1.2f);
            Sfx.Play(this, "pop", 0.8f);
            yield return S41AKit.Bounce(cheese, 0.3f, 0.3f);
            yield return S41AKit.Bounce(mouse, 0.25f, 0.3f);
            yield return new WaitForSeconds(0.8f);
            Win();
        }
    }
}
