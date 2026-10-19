using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "Ice cream shop" — ora di punta in gelateria: 6 clienti, ognuno con il suo scontrino (2-4 palline in un
    /// ordine preciso dal basso, più eventuali codette e ciliegina). Tocca le vaschette per impilare le palline, i
    /// barattoli per i condimenti, il bidone per buttare il cono sbagliato e SERVE per consegnare. La pazienza dei
    /// clienti è sempre più corta; un ordine sbagliato o in ritardo fa arrabbiare il cliente (massimo 2, al terzo si
    /// perde). Il trucco: il 5° cliente è il gatto che ordina "lo stesso del cliente di prima" — bisogna ricordarselo.
    /// </summary>
    public class IceCreamLevel : LevelController
    {
        [SerializeField] StageHud40D hud;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] Transform[] customers;       // radici dei 6 clienti (il 5° è il gatto)
        [SerializeField] Transform customerSpot;
        [SerializeField] Transform ticket;
        [SerializeField] SpriteRenderer[] ticketScoops;
        [SerializeField] SpriteRenderer ticketSprinkles, ticketCherry, ticketCone;
        [SerializeField] TextMesh ticketText;
        [SerializeField] Transform patience;          // perno della barra (scala x)
        [SerializeField] SpriteRenderer patienceFill;
        [SerializeField] SpriteRenderer[] stack;      // palline sul cono
        [SerializeField] SpriteRenderer stackSprinkles, stackCherry;
        [SerializeField] Transform coneHolder;
        [SerializeField] Collider2D[] tubs;
        [SerializeField] Collider2D shaker, cherryBowl, bin, serve;
        [SerializeField] SpriteRenderer[] strikes;    // le due faccine arrabbiate
        [SerializeField] Transform mood;              // cuore o rabbia sopra il cliente
        [SerializeField] SpriteRenderer moodArt;
        [SerializeField] Sprite happySprite, angrySprite;
        [SerializeField] float[] patienceSeconds = { 25f, 23f, 21f, 19f, 17f, 16f };

        static readonly Color[] Flavours =
        {
            new Color(1f, 0.6f, 0.72f),    // fragola
            new Color(0.58f, 0.38f, 0.26f), // cioccolato
            new Color(1f, 0.95f, 0.8f),    // vaniglia
            new Color(0.62f, 0.9f, 0.72f), // menta
        };

        class Order { public int[] scoops; public bool sprinkles, cherry; }

        Camera cam;
        Order[] orders;
        Order current;
        readonly int[] built = new int[4];
        int count, angry;
        bool hasSprinkles, hasCherry, open, waitingServe;
        float bubbleUntil, clock, timeLeft, timeMax;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            foreach (Transform c in customers) c.gameObject.SetActive(false);
            foreach (SpriteRenderer s in strikes) s.color = new Color(1f, 1f, 1f, 0.2f);
            mood.gameObject.SetActive(false);
            ticket.gameObject.SetActive(false);
            patience.parent.gameObject.SetActive(false);
            MakeOrders();
            ClearCone();
            StartCoroutine(Run());
        }

        void Say(string text, float seconds)
        {
            if (bubble == null) return;
            bubble.Show(text);
            bubbleUntil = clock + seconds;
        }

        void MakeOrders()
        {
            int[] sizes = { 2, 2, 3, 3, 3, 4 };
            orders = new Order[6];
            for (int i = 0; i < 6; i++)
            {
                if (i == 4) { orders[i] = orders[3]; continue; }   // il gatto: lo stesso del cliente di prima
                var o = new Order { scoops = new int[sizes[i]] };
                for (int k = 0; k < o.scoops.Length; k++)
                {
                    int f;
                    do f = Random.Range(0, Flavours.Length); while (k > 0 && f == o.scoops[k - 1]);
                    o.scoops[k] = f;
                }
                o.sprinkles = i == 1 || i == 3 || i == 5;
                o.cherry = i == 2 || i == 3;
                orders[i] = o;
            }
        }

        void ClearCone()
        {
            count = 0;
            hasSprinkles = hasCherry = false;
            foreach (SpriteRenderer s in stack) s.gameObject.SetActive(false);
            stackSprinkles.gameObject.SetActive(false);
            stackCherry.gameObject.SetActive(false);
        }

        void ShowTicket(Order o, bool secret)
        {
            ticket.gameObject.SetActive(true);
            ticketText.gameObject.SetActive(secret);
            ticketCone.gameObject.SetActive(!secret);
            for (int i = 0; i < ticketScoops.Length; i++)
            {
                bool on = !secret && i < o.scoops.Length;
                ticketScoops[i].gameObject.SetActive(on);
                if (on) ticketScoops[i].color = Flavours[o.scoops[i]];
            }
            int top = Mathf.Max(0, o.scoops.Length - 1);
            ticketSprinkles.gameObject.SetActive(!secret && o.sprinkles);
            ticketSprinkles.transform.position = ticketScoops[top].transform.position;
            ticketCherry.gameObject.SetActive(!secret && o.cherry);
            ticketCherry.transform.position = ticketScoops[top].transform.position + ticketScoops[top].transform.lossyScale.y * new Vector3(0f, 0.32f, 0f);
        }

        IEnumerator Run()
        {
            hud.Goal("Make the order on the ticket, then SERVE!");
            yield return new WaitForSeconds(0.4f);
            for (int i = 0; i < orders.Length; i++)
            {
                hud.SetStage(i);
                current = orders[i];
                Transform c = customers[i];
                // il cliente arriva
                c.gameObject.SetActive(true);
                Vector3 spot = customerSpot.position;
                c.position = spot + Vector3.left * 4f;
                for (float k = 0f; k < 1f; k += Time.deltaTime / 0.5f) { c.position = Vector3.Lerp(spot + Vector3.left * 4f, spot, 1f - (1f - k) * (1f - k)) + Vector3.up * Mathf.Abs(Mathf.Sin(k * 12f)) * 0.08f; yield return null; }
                c.position = spot;
                bool isCat = i == 4;
                ShowTicket(current, isCat);
                Sfx.Play(this, isCat ? "pop" : "tick", 0.7f);
                hud.Goal(isCat ? "Customer " + (i + 1) + "/6: \"Same as the last one!\"" : "Customer " + (i + 1) + "/6: make the order!");
                if (isCat) Say("Meow. Same as the last customer, please!", 3f);
                else if (i == 0) Say("Hi! Bottom scoop first, please.", 2.5f);
                timeMax = timeLeft = patienceSeconds[Mathf.Min(i, patienceSeconds.Length - 1)];
                patience.parent.gameObject.SetActive(true);
                ClearCone();
                open = true;
                waitingServe = true;
                bool ok = false;
                while (waitingServe)
                {
                    timeLeft -= Time.deltaTime;
                    float f = Mathf.Clamp01(timeLeft / timeMax);
                    patience.localScale = new Vector3(f, 1f, 1f);
                    patienceFill.color = f > 0.5f ? new Color(0.36f, 0.8f, 0.42f) : f > 0.25f ? new Color(1f, 0.8f, 0.25f) : new Color(0.95f, 0.3f, 0.25f);
                    if (f < 0.25f) c.localPosition += new Vector3(Mathf.Sin(clock * 40f) * 0.004f, 0f, 0f);
                    if (timeLeft <= 0f) { waitingServe = false; Say("Too slow! I'm leaving!", 2f); }
                    else if (TryInput(out bool servedNow, out bool right)) { if (servedNow) { waitingServe = false; ok = right; } }
                    yield return null;
                }
                open = false;
                patience.parent.gameObject.SetActive(false);
                c.position = spot;
                yield return Reaction(c, ok, isCat);
                if (IsFinished) yield break;
                ticket.gameObject.SetActive(false);
                ClearCone();
                // il cliente se ne va
                for (float k = 0f; k < 1f; k += Time.deltaTime / 0.45f) { c.position = Vector3.Lerp(spot, spot + Vector3.right * 4.5f, k * k); yield return null; }
                c.gameObject.SetActive(false);
                mood.gameObject.SetActive(false);
                yield return new WaitForSeconds(0.3f);
            }
            hud.SetStage(6);
            hud.Goal("SOLD OUT! Shop closed.");
            Sfx.Play(this, "ding", 0.9f);
            Haptics.Light();
            Say(angry == 0 ? "Six happy customers! Perfect!" : "Sold out! Time to lick the spoons.", 2.6f);
            mood.position = customerSpot.position + new Vector3(0f, 1.2f, 0f);
            moodArt.sprite = happySprite;
            mood.gameObject.SetActive(true);
            for (float k = 0f; k < 2.4f; k += Time.deltaTime) { mood.localScale = Vector3.one * (1f + Mathf.Sin(k * 8f) * 0.15f); yield return null; }
            Win();
        }

        IEnumerator Reaction(Transform c, bool ok, bool isCat)
        {
            mood.gameObject.SetActive(true);
            mood.position = c.position + new Vector3(0.75f, 1.7f, 0f);
            moodArt.sprite = ok ? happySprite : angrySprite;
            if (ok)
            {
                Sfx.Play(this, "ding", 0.7f);
                Say(isCat ? "Purrrfect. You remembered!" : "Yummy! Thanks!", 1.6f);
                // il cono vola al cliente
                Vector3 from = coneHolder.position;
                for (float k = 0f; k < 1f; k += Time.deltaTime / 0.35f) { coneHolder.position = Vector3.Lerp(from, c.position + Vector3.up * 0.4f, k); yield return null; }
                ClearCone();
                coneHolder.position = from;
                for (float k = 0f; k < 1f; k += Time.deltaTime / 0.5f) { c.localScale = Vector3.one * (1f + Mathf.Sin(k * Mathf.PI * 2f) * 0.06f); yield return null; }
                c.localScale = Vector3.one;
            }
            else
            {
                angry++;
                Haptics.Medium();
                Sfx.Play(this, "buzz", 0.8f);
                if (bubbleUntil < clock + 0.1f) Say(isCat ? "HISSS! That's NOT what he had!" : "That's not what I ordered!", 1.8f);
                if (angry - 1 < strikes.Length) strikes[angry - 1].color = Color.white;
                for (float k = 0f; k < 1f; k += Time.deltaTime / 0.6f) { c.localPosition += new Vector3(Mathf.Sin(k * 60f) * 0.02f, 0f, 0f); yield return null; }
                if (angry > 2)
                {
                    hud.Goal("Too many angry customers!");
                    Say("The shop gets a one-star review...", 2.5f);
                    yield return new WaitForSeconds(2.2f);
                    Fail();
                    yield break;
                }
            }
            yield return new WaitForSeconds(0.5f);
        }

        /// <summary>Gestisce i tocchi; true se è stato toccato qualcosa. servedNow = consegnato, right = ordine giusto.</summary>
        bool TryInput(out bool servedNow, out bool right)
        {
            servedNow = right = false;
            if (!open) return false;
            if (!PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out bool _) || !began) return false;
            if (PointerInput.IsOverUi()) return false;
            Vector2 w = PointerInput.ToWorld(cam, sp);
            for (int i = 0; i < tubs.Length; i++)
            {
                if (!tubs[i].OverlapPoint(w)) continue;
                StartCoroutine(Punch(tubs[i].transform));
                if (hasSprinkles || hasCherry) { Sfx.Play(this, "buzz", 0.4f); Say("Toppings go on TOP!", 1.4f); return true; }
                if (count >= stack.Length) { Sfx.Play(this, "buzz", 0.4f); Say("The cone is full!", 1.4f); return true; }
                built[count] = i;
                stack[count].color = Flavours[i];
                StartCoroutine(Plop(stack[count].transform));
                count++;
                Sfx.Play(this, "pop", 0.6f);
                return true;
            }
            if (shaker.OverlapPoint(w))
            {
                StartCoroutine(Punch(shaker.transform));
                if (count == 0 || hasSprinkles) { Sfx.Play(this, "buzz", 0.3f); return true; }
                hasSprinkles = true;
                stackSprinkles.transform.position = stack[count - 1].transform.position;
                stackSprinkles.gameObject.SetActive(true);
                Sfx.Play(this, "tick", 0.7f);
                return true;
            }
            if (cherryBowl.OverlapPoint(w))
            {
                StartCoroutine(Punch(cherryBowl.transform));
                if (count == 0 || hasCherry) { Sfx.Play(this, "buzz", 0.3f); return true; }
                hasCherry = true;
                stackCherry.transform.position = stack[count - 1].transform.position + new Vector3(0f, 0.3f, 0f);
                StartCoroutine(Plop(stackCherry.transform));
                Sfx.Play(this, "pop", 0.8f);
                return true;
            }
            if (bin.OverlapPoint(w))
            {
                StartCoroutine(Punch(bin.transform));
                if (count == 0) return true;
                Sfx.Play(this, "whoosh", 0.6f);
                ClearCone();
                return true;
            }
            if (serve.OverlapPoint(w))
            {
                StartCoroutine(Punch(serve.transform));
                if (count == 0) { Sfx.Play(this, "buzz", 0.3f); Say("An empty cone? Really?", 1.4f); return true; }
                servedNow = true;
                right = Matches();
                return true;
            }
            return false;
        }

        bool Matches()
        {
            if (count != current.scoops.Length || hasSprinkles != current.sprinkles || hasCherry != current.cherry) return false;
            for (int i = 0; i < count; i++) if (built[i] != current.scoops[i]) return false;
            return true;
        }

        IEnumerator Punch(Transform t)
        {
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.15f) { t.localScale = Vector3.one * (1f - Mathf.Sin(k * Mathf.PI) * 0.1f); yield return null; }
            t.localScale = Vector3.one;
        }

        IEnumerator Plop(Transform t)
        {
            t.gameObject.SetActive(true);
            Vector3 to = t.localPosition, from = to + Vector3.up * 0.6f;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.12f) { t.localPosition = Vector3.Lerp(from, to, k); yield return null; }
            t.localPosition = to;
        }

        void Update()
        {
            clock += Time.deltaTime;
            if (bubble != null && bubbleUntil > 0f && clock > bubbleUntil) { bubble.Hide(); bubbleUntil = 0f; }
        }
    }
}
