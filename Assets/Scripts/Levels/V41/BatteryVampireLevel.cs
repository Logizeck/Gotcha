using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "Battery vampire" — un vampiretto elettronico succhia energia da una batteria virtuale (non quella vera
    /// del telefono): in 30 secondi arriva a 0% e si perde. Toccarlo non serve (schiva). Trucco: si scambiano i simboli
    /// + e − trascinandone uno sull'altro: con la polarità invertita il vampiro prende la scossa e scappa.
    /// </summary>
    public class BatteryVampireLevel : LevelController
    {
        [SerializeField] Transform fill;             // barra della carica (si allunga in x da sinistra)
        [SerializeField] SpriteRenderer fillArt;
        [SerializeField] TextMesh percent;
        [SerializeField] Transform plus, minus;
        [SerializeField] Transform vampire;
        [SerializeField] SpriteRenderer vampireArt;
        [SerializeField] Sprite zapSprite;
        [SerializeField] LineRenderer cable;
        [SerializeField] Transform terminal;
        [SerializeField] SpriteRenderer[] bolts;
        [SerializeField] TextMesh goal;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] float fillLeft = -1.62f, fillWidth = 3.24f, drainSeconds = 30f;

        Camera cam;
        float charge = 1f, bubbleUntil, dodgeUntil;
        Vector3 plusHome, minusHome, vampHome;
        Transform grabbed;
        Vector2 grabStart, grabOffset;
        bool busy, moved;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            plusHome = plus.position;
            minusHome = minus.position;
            vampHome = vampire.position;
            foreach (SpriteRenderer b in bolts) if (b != null) b.gameObject.SetActive(false);
            Kit41C.Goal(goal, "SAVE THE BATTERY!");
            Say("Slurp! Delicious electrons!", 2.2f);
            ShowCharge();
        }

        void Say(string text, float seconds)
        {
            if (bubble == null) return;
            bubble.Show(text);
            bubbleUntil = Time.time + seconds;
        }

        void ShowCharge()
        {
            float w = fillWidth * Mathf.Clamp01(charge);
            fill.localScale = new Vector3(Mathf.Max(0.001f, w), fill.localScale.y, 1f);
            fill.localPosition = new Vector3(fillLeft + w * 0.5f, fill.localPosition.y, fill.localPosition.z);
            if (fillArt != null) fillArt.color = Color.Lerp(new Color(1f, 0.3f, 0.3f), new Color(0.4f, 0.9f, 0.4f), Mathf.Clamp01((charge - 0.15f) / 0.5f));
            if (percent != null) percent.text = Mathf.CeilToInt(Mathf.Clamp01(charge) * 100f) + "%";
        }

        void Update()
        {
            if (IsFinished) return;
            if (bubble != null && bubbleUntil > 0f && Time.time > bubbleUntil) { bubble.Hide(); bubbleUntil = 0f; }
            UpdateCable();
            if (busy) { grabbed = null; return; }
            charge -= Time.deltaTime / drainSeconds;
            ShowCharge();
            // il vampiro ondeggia mentre succhia
            if (Time.time > dodgeUntil) vampire.position = vampHome + new Vector3(0f, Mathf.Sin(Time.time * 5f) * 0.05f, 0f);
            if (charge <= 0f) { StartCoroutine(Empty()); return; }

            if (!Kit41C.Pointer(cam, out Vector2 w, out bool began, out bool ended)) { if (grabbed != null) Drop(); return; }
            if (began)
            {
                if (Vector2.Distance(w, plus.position) < 0.55f) grabbed = plus;
                else if (Vector2.Distance(w, minus.position) < 0.55f) grabbed = minus;
                else if (Vector2.Distance(w, vampire.position) < 0.7f) { StartCoroutine(Dodge()); return; }
                if (grabbed != null)
                {
                    grabStart = w;
                    moved = false;
                    grabOffset = (Vector2)grabbed.position - w;
                    Sfx.Play(this, "tick", 0.5f);
                }
                return;
            }
            if (grabbed == null) return;
            if ((w - grabStart).magnitude > 0.15f) moved = true;
            Vector2 p = w + grabOffset;
            grabbed.position = new Vector3(Mathf.Clamp(p.x, -2.5f, 2.5f), Mathf.Clamp(p.y, -4.3f, 2.3f), 0f);
            if (ended) Drop();
        }

        void UpdateCable()
        {
            if (cable == null || terminal == null) return;
            cable.SetPosition(0, vampire.position + new Vector3(0f, -0.3f, 0f));
            cable.SetPosition(1, terminal.position);
        }

        void Drop()
        {
            Transform g = grabbed;
            grabbed = null;
            Transform other = g == plus ? minus : plus;
            Vector3 home = g == plus ? plusHome : minusHome;
            if (moved && Vector2.Distance(g.position, other.position) < 0.75f) { StartCoroutine(Swap()); return; }
            if (!moved) Say(g == plus ? "Plus. Vampires love plus." : "Minus. Boring.", 1.3f);
            StartCoroutine(Kit41C.MoveWorld(g, home, 0.2f));
        }

        IEnumerator Dodge()
        {
            dodgeUntil = Time.time + 0.6f;
            Sfx.Play(this, "whoosh", 0.4f);
            Say("Bleh! Can't catch me!", 1.3f);
            yield return Kit41C.MoveWorld(vampire, vampHome + new Vector3(0.2f, 0.7f, 0f), 0.25f);
            yield return Kit41C.MoveWorld(vampire, vampHome, 0.3f);
        }

        IEnumerator Swap()
        {
            busy = true;
            Vector3 a = plus.position, b = minus.position;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.35f)
            {
                plus.position = Vector3.Lerp(a, minusHome, Mathf.SmoothStep(0f, 1f, k)) + Vector3.up * Mathf.Sin(k * Mathf.PI) * 0.4f;
                minus.position = Vector3.Lerp(b, plusHome, Mathf.SmoothStep(0f, 1f, k)) - Vector3.up * Mathf.Sin(k * Mathf.PI) * 0.4f;
                yield return null;
            }
            plus.position = minusHome;
            minus.position = plusHome;
            Sfx.Play(this, "clunk", 0.7f);
            yield return new WaitForSeconds(0.25f);
            // polarità invertita: scossa!
            Kit41C.Goal(goal, "REVERSED POLARITY!");
            if (vampireArt != null && zapSprite != null) vampireArt.sprite = zapSprite;
            Haptics.Heavy();
            for (int k = 0; k < 8; k++)
            {
                Sfx.Play(this, "buzz", 0.6f);
                for (int i = 0; i < bolts.Length; i++)
                {
                    if (bolts[i] == null) continue;
                    bolts[i].gameObject.SetActive((i + k) % 2 == 0);
                    bolts[i].transform.position = vampire.position + (Vector3)(Random.insideUnitCircle * 0.7f);
                    bolts[i].transform.localRotation = Quaternion.Euler(0f, 0f, Random.Range(-40f, 40f));
                }
                vampire.position = vampHome + (Vector3)(Random.insideUnitCircle * 0.08f);
                yield return new WaitForSeconds(0.1f);
            }
            foreach (SpriteRenderer bo in bolts) if (bo != null) bo.gameObject.SetActive(false);
            Say("YEOWCH! Too spicy!", 1.6f);
            if (cable != null) cable.enabled = false;
            // scappa via in alto a sinistra
            Vector3 from = vampire.position;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.9f)
            {
                vampire.position = Vector3.Lerp(from, new Vector3(-3.8f, 4.5f, 0f), k * k);
                vampire.Rotate(0f, 0f, 500f * Time.deltaTime);
                yield return null;
            }
            vampire.gameObject.SetActive(false);
            // la batteria si ricarica
            Kit41C.Goal(goal, "BATTERY SAVED!");
            float c0 = charge;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.8f)
            {
                charge = Mathf.Lerp(c0, 1f, k);
                ShowCharge();
                yield return null;
            }
            charge = 1f;
            ShowCharge();
            Sfx.Play(this, "ding", 0.8f);
            yield return new WaitForSeconds(0.9f);
            Win();
        }

        IEnumerator Empty()
        {
            busy = true;
            charge = 0f;
            ShowCharge();
            Kit41C.Goal(goal, "BATTERY EMPTY!");
            Sfx.Play(this, "buzz", 0.8f);
            Haptics.Medium();
            Say("BURP! Thanks for dinner!", 2f);
            yield return Kit41C.Pop(vampire, 0.3f, 0.4f);
            yield return new WaitForSeconds(1.0f);
            Fail();
        }
    }
}
