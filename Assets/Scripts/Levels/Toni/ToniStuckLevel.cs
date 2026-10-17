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
    /// e SCHLUPP! esce. Easter egg `toni_toot`: spingerlo tante volte senza burro... la pressione trova un'altra
    /// via d'uscita e Toni parte a razzo.
    /// </summary>
    public class ToniStuckLevel : LevelController
    {
        [SerializeField] ToniPug toni;
        [SerializeField] Transform wall;
        [SerializeField] Transform flap;
        [SerializeField] Draggable2D butter;
        [SerializeField] Draggable2D bone;
        [SerializeField] Draggable2D feather;
        [SerializeField] SmokeEmitter shine;
        [SerializeField] SmokeEmitter toot;
        [SerializeField] int pushesForEgg = 8;
        [SerializeField] float pushDistance = 0.5f;

        Camera cam;
        Vector3 home;
        bool greased, busy, pushing;
        float pushStartX, nextMumble, wiggle;
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
            nextMumble = Time.time + 1.2f;
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
            HandlePush();
        }

        void HandlePush()
        {
            if (busy || cam == null || Draggable2D.Active != null) { pushing = false; return; }
            if (!PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out bool ended)) { pushing = false; return; }
            Vector2 w = PointerInput.ToWorld(cam, sp);
            if (began && !PointerInput.IsOverUi())
            {
                Vector2 l = toni.transform.InverseTransformPoint(w);
                if (w.x < wall.position.x && Mathf.Abs(l.y - 0.1f) < 0.8f && Mathf.Abs(l.x - 0.4f) < 0.9f) { pushing = true; pushStartX = w.x; }
                else if (w.x > wall.position.x && Vector2.Distance(w, toni.HeadWorld) < 0.8f) toni.Say("Mmm?", 0.8f);
            }
            if (!pushing) return;
            if (w.x - pushStartX > pushDistance) { pushing = false; Push(); }
            if (ended) pushing = false;
        }

        void Push()
        {
            if (greased) { StartCoroutine(SlideOut()); return; }
            pushes++;
            if (pushes >= pushesForEgg) { StartCoroutine(TootOut()); return; }
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
            if (busy || IsFinished || Vector2.Distance(d.transform.position, Butt) > 1.1f) return;
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
            Sfx.Play(this, "pop", 0.7f);
            if (shine != null) { shine.transform.position = Butt; shine.Play(); }
            toni.Say("...slippery?", 1.2f);
            busy = false;
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

        IEnumerator TootOut()
        {
            busy = true;
            toni.Say("...uh oh.", 0.9f);
            yield return new WaitForSeconds(0.9f);
            toni.Say("PRRRRRRT!!", 1.5f, "buzz");
            Haptics.Heavy();
            if (toot != null) { toot.transform.position = Butt; toot.Play(); }
            Vector3 start = toni.transform.localPosition;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.5f)   // a razzo, fuori dallo schermo
            {
                toni.transform.localPosition = start + new Vector3(7f * k * k, 1.5f * k, 0f);
                toni.transform.localRotation = Quaternion.Euler(0, 0, 15f * k);
                if (toot != null) toot.transform.position = Butt;
                if (flap != null) flap.localRotation = Quaternion.Euler(0, 0, 80f * (1f - k));
                yield return null;
            }
            yield return new WaitForSeconds(0.8f);
            if (toot != null) toot.Stop();
            WinWithEgg("toni_toot");
        }
    }
}
