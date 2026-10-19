using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 25 — "STUCK!". Toni voleva uscire in giardino dalla gattaiola... ed è rimasto incastrato a metà: la
    /// testa fuori, il sedere (con la coda che scodinzola e le zampe che pedalano) dentro casa. Spingerlo non basta.
    /// Sul tavolo ci sono un osso (si agita e basta), una piuma (ride e scalcia) e il burro: imburrato, una spinta
    /// e SCHLUPP! esce. Quando è imburrato si VEDE: sedere lucido con riflessi e stelline che brillano, gocce di burro,
    /// la scritta "SLIPPERY!" e un freccione che indica la spinta (verso il muro, a destra).
    /// </summary>
    public class ToniStuckLevel : LevelController
    {
        [SerializeField] ToniPug toni;
        [SerializeField] Transform flap;
        [SerializeField] Draggable2D butter;
        [SerializeField] Draggable2D bone;
        [SerializeField] Draggable2D feather;
        [SerializeField] SmokeEmitter shine;
        [SerializeField] SmokeEmitter toot;
        [Header("Imburrato: si vede!")]
        [SerializeField] SpriteRenderer gloss;        // riflessi lucidi sul sedere (figlio di Toni)
        [SerializeField] SmokeEmitter drips;          // gocce di burro che colano
        [SerializeField] Transform arrow;             // freccione "spingi di qua"
        [SerializeField] TextMesh slippery;           // scritta "SLIPPERY!"
        [SerializeField] float pushDistance = 0.35f;

        Camera cam;
        Vector3 home;
        bool greased, busy, pushing;
        float pushStartX, nextMumble, wiggle;
        Vector2 pushStart;
        Vector3 arrowHome, slipperyScale;
        float greasedAt;
        int pushes;

        Vector3 Butt => toni.transform.TransformPoint(new Vector2(0.75f, 0.15f));

        public override void OnLevelStart()
        {
            cam = Camera.main;
            toni.Stand(true);
            toni.Face(true);   // testa a destra, fuori in giardino
            toni.WakeUp(2);
            toni.Wag = 0.7f;
            home = toni.transform.localPosition;
            foreach (Draggable2D d in new[] { butter, bone, feather }) d.Dropped += OnItemDropped;
            if (shine != null) shine.Stop();
            if (toot != null) toot.Stop();
            if (drips != null) drips.Stop();
            if (gloss != null) gloss.gameObject.SetActive(false);
            if (arrow != null) { arrowHome = arrow.localPosition; arrow.gameObject.SetActive(false); }
            if (slippery != null) { slipperyScale = slippery.transform.localScale; slippery.gameObject.SetActive(false); }
            nextMumble = Time.time + 3f;
            toni.Say("I'm STUCK! Push me!", 2.2f);
        }

        void OnDestroy()
        {
            foreach (Draggable2D d in new[] { butter, bone, feather }) if (d != null) d.Dropped -= OnItemDropped;
        }

        void Update()
        {
            if (IsFinished) return;
            if (!busy)
            {
                // si dimena: le zampe pedalano a vuoto
                wiggle += Time.deltaTime * 9f;
                toni.transform.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(wiggle) * 2.5f);
                if (Time.time >= nextMumble)
                {
                    nextMumble = Time.time + Random.Range(3f, 4.5f);
                    string[] lines = { "Mmmpf!", "Help?", "*wiggle wiggle*", "I'm stuck!" };
                    toni.Say(lines[Random.Range(0, lines.Length)], 1.2f);
                }
                if (shine != null && greased) shine.transform.position = Butt;
            }
            if (greased) ShowGreased();
            HandlePush();
        }

        /// <summary>Su Toni (dentro o fuori casa, coordinate locali di Toni)?</summary>
        bool OnToni(Vector2 w)
        {
            Vector2 l = toni.transform.InverseTransformPoint(w);
            return l.x > -1.25f && l.x < 1.35f && Mathf.Abs(l.y - 0.1f) < 0.85f;
        }

        // spinta = un tocco su Toni oppure un trascinamento verso il muro
        void HandlePush()
        {
            if (busy || cam == null || Draggable2D.Active != null) { pushing = false; return; }
            if (!PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out bool ended)) { pushing = false; return; }
            Vector2 w = PointerInput.ToWorld(cam, sp);
            bool onArrow = greased && arrow != null && arrow.gameObject.activeSelf && Vector2.Distance(w, arrow.position) < 1.0f;
            if (began && !PointerInput.IsOverUi() && (OnToni(w) || onArrow)) { pushing = true; pushStartX = w.x; pushStart = w; }
            if (!pushing) return;
            if (w.x - pushStartX > pushDistance) { pushing = false; Push(); return; }
            if (ended)
            {
                pushing = false;
                if (Vector2.Distance(w, pushStart) < 0.3f) Push();   // un tocco vale come spinta
            }
        }

        void Push()
        {
            if (greased) { StartCoroutine(SlideOut()); return; }
            pushes++;
            string[] lines = { "Mmmpf!", "Nope!", "STUCK!", "*wheeze*", "Harder?", "Ouch!", "...my belly!" };
            toni.Say(lines[Mathf.Min(pushes - 1, lines.Length - 1)], 1f, "bonk");
            Haptics.Light();
            StartCoroutine(Squeeze());
        }

        IEnumerator Squeeze()
        {
            busy = true;
            Vector3 s = toni.transform.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.35f)
            {
                float b = Mathf.Sin(k * Mathf.PI);
                toni.transform.localPosition = home + new Vector3(0.18f * b, 0f, 0f);
                toni.transform.localScale = new Vector3(s.x * (1f + 0.06f * b), s.y * (1f - 0.08f * b), s.z);
                if (flap != null) flap.localRotation = Quaternion.Euler(0, 0, 55f + 8f * b);
                yield return null;
            }
            toni.transform.localPosition = home;
            toni.transform.localScale = s;
            busy = false;
        }

        void OnItemDropped(Draggable2D d, DropSlot slot)
        {
            // basta lasciarlo su Toni (o vicino al sedere): niente mira di precisione
            if (busy || IsFinished || (!OnToni(d.transform.position) && Vector2.Distance(d.transform.position, Butt) > 1.4f)) return;
            if (d == butter) StartCoroutine(Grease());
            else if (d == bone) StartCoroutine(Excited());
            else if (d == feather) StartCoroutine(Giggle());
        }

        IEnumerator Grease()
        {
            busy = true;
            butter.enabled = false;
            butter.gameObject.SetActive(true);
            Vector3 from = Butt;
            Vector3 b0 = butter.transform.localScale;
            // spalma il burro sul sedere di Toni (avanti e indietro), poi il panetto è finito
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.6f)
            {
                butter.transform.position = new Vector3(from.x + Mathf.Sin(k * Mathf.PI * 6f) * 0.25f, from.y + 0.1f, butter.transform.position.z);
                butter.transform.localScale = b0 * (1f - 0.7f * k);
                yield return null;
            }
            butter.gameObject.SetActive(false);
            greased = true;
            greasedAt = Time.time;
            Sfx.Play(this, "pop", 0.7f);
            Haptics.Light();
            if (shine != null) { shine.transform.position = Butt; shine.Play(); }
            if (drips != null) { drips.transform.position = Butt; drips.Play(); }
            if (gloss != null) gloss.gameObject.SetActive(true);
            if (slippery != null) slippery.gameObject.SetActive(true);
            toni.Say("Ooh... slippery!", 1.4f);
            yield return new WaitForSeconds(0.5f);
            if (arrow != null) arrow.gameObject.SetActive(true);   // la freccia arriva dopo la scritta
            busy = false;
        }

        /// <summary>Burro ben visibile: riflessi che pulsano, "SLIPPERY!" che salta fuori e poi resta piccola, freccia che spinge.</summary>
        void ShowGreased()
        {
            float t = Time.time - greasedAt;
            if (gloss != null) gloss.color = new Color(1f, 1f, 1f, 0.75f + 0.25f * Mathf.Sin(t * 5f));
            if (drips != null) drips.transform.position = Butt + new Vector3(0f, -0.25f, 0f);
            if (slippery != null && slippery.gameObject.activeSelf)
            {
                // pop elastico, poi un leggero ondeggiare
                float pop = t < 0.35f ? Mathf.Lerp(0f, 1.3f, t / 0.35f) : t < 0.55f ? Mathf.Lerp(1.3f, 1f, (t - 0.35f) / 0.2f) : 1f + Mathf.Sin(t * 4f) * 0.04f;
                slippery.transform.localScale = slipperyScale * pop;
                slippery.transform.localRotation = Quaternion.Euler(0, 0, 8f + Mathf.Sin(t * 3f) * 4f);
            }
            if (arrow != null && arrow.gameObject.activeSelf)
                arrow.localPosition = arrowHome + new Vector3(Mathf.Abs(Mathf.Sin(t * 4f)) * 0.25f, 0f, 0f);   // "spingi di qua!"
        }

        IEnumerator Excited()
        {
            busy = true;
            toni.Bark("WOOF?! BONE?!");
            toni.Wag = 1.5f;
            for (float k = 0f; k < 1.2f; k += Time.deltaTime)
            {
                toni.transform.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(k * 40f) * 6f);
                yield return null;
            }
            toni.Say("...still stuck.", 1.2f);
            toni.Wag = 0.7f;
            busy = false;
        }

        IEnumerator Giggle()
        {
            busy = true;
            toni.Say("Hehehe! Stop it!", 1.4f);
            Sfx.Play(this, "pop", 0.4f);
            for (float k = 0f; k < 1.2f; k += Time.deltaTime)
            {
                toni.transform.localPosition = home + new Vector3(0f, Mathf.Abs(Mathf.Sin(k * 25f)) * 0.08f, 0f);
                yield return null;
            }
            toni.transform.localPosition = home;
            busy = false;
        }

        IEnumerator SlideOut()
        {
            busy = true;
            if (shine != null) shine.Stop();
            if (drips != null) drips.Stop();
            if (arrow != null) arrow.gameObject.SetActive(false);
            if (slippery != null) slippery.gameObject.SetActive(false);
            toni.Say("SCHLUPP!", 1f, "whoosh");
            Haptics.Medium();
            Vector3 start = toni.transform.localPosition;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.35f)
            {
                toni.transform.localPosition = start + new Vector3(2.6f * k * k, Mathf.Sin(k * Mathf.PI) * 0.2f, 0f);
                toni.transform.localRotation = Quaternion.identity;
                if (flap != null) flap.localRotation = Quaternion.Euler(0, 0, 70f * (1f - k));
                yield return null;
            }
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.8f)   // lo sportello dondola
            {
                if (flap != null) flap.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(k * Mathf.PI * 4f) * 25f * (1f - k));
                yield return null;
            }
            toni.Face(false);
            yield return toni.Happy(1f);
            toni.Bark("FREEDOM!");
            yield return new WaitForSeconds(1.2f);
            Win();
        }

    }
}
