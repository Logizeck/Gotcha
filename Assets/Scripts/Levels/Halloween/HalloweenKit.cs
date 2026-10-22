using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;
using Gotcha.Levels.Halloween;

namespace Gotcha.Levels
{
    /// <summary>
    /// Aiuti condivisi dello speciale Halloween (livelli 245-250): lettura del dito, area sicura dello schermo,
    /// piccole animazioni (spostamenti, rimbalzi, tremolii, dissolvenze) e coriandoli.
    /// La logica dei livelli è in Gotcha.Levels.Halloween (cartella Logic, senza Unity): qui solo input e presentazione.
    /// </summary>
    public static class HalloweenKit
    {
        // area in cui restano sempre gli oggetti importanti (sotto la barra in alto, dentro i bordi su 16:9 - 20:9)
        public const float SafeMinX = -2.55f, SafeMaxX = 2.55f, SafeMinY = -4.5f, SafeMaxY = 3.0f;

        public static Vector2 ClampSafe(Vector2 p) =>
            new Vector2(Mathf.Clamp(p.x, SafeMinX, SafeMaxX), Mathf.Clamp(p.y, SafeMinY, SafeMaxY));

        /// <summary>
        /// Dito (o mouse) in coordinate mondo. False se non c'è nessun dito o ce ne sono due (il gesto si annulla);
        /// un tocco che inizia sopra la UI non conta come inizio.
        /// </summary>
        public static bool ReadPointer(Camera cam, out Vector2 world, out bool began, out bool ended)
        {
            world = Vector2.zero;
            began = ended = false;
            if (cam == null || Input.touchCount >= 2) return false;
            if (!PointerInput.TryGetPrimary(out Vector2 sp, out began, out ended)) return false;
            if (began && PointerInput.IsOverUi()) began = false;
            world = PointerInput.ToWorld(cam, sp);
            return true;
        }

        public static HwPoint ToHw(Vector2 v) => new HwPoint(v.x, v.y);

        /// <summary>Sposta (posizione mondo) con un'eventuale parabola verso l'alto, morbido in arrivo.</summary>
        public static IEnumerator Move(Transform t, Vector3 to, float seconds, float arc = 0f)
        {
            if (t == null) yield break;
            Vector3 from = t.position;
            for (float k = 0f; k < 1f; k += Time.deltaTime / Mathf.Max(0.01f, seconds))
            {
                float e = 1f - (1f - k) * (1f - k);
                t.position = Vector3.Lerp(from, to, e) + Vector3.up * (arc * Mathf.Sin(k * Mathf.PI));
                yield return null;
            }
            t.position = to;
        }

        /// <summary>Scala verso un valore (morbido).</summary>
        public static IEnumerator ScaleTo(Transform t, Vector3 to, float seconds)
        {
            if (t == null) yield break;
            Vector3 from = t.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / Mathf.Max(0.01f, seconds))
            {
                t.localScale = Vector3.Lerp(from, to, 1f - (1f - k) * (1f - k));
                yield return null;
            }
            t.localScale = to;
        }

        /// <summary>Rimbalzo di scala (da 1 a peak e ritorno).</summary>
        public static IEnumerator Pop(Transform t, float peak, float seconds)
        {
            if (t == null) yield break;
            Vector3 s = t.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / Mathf.Max(0.01f, seconds))
            {
                t.localScale = s * (1f + (peak - 1f) * Mathf.Sin(k * Mathf.PI));
                yield return null;
            }
            t.localScale = s;
        }

        /// <summary>Tremolio sul posto (torna esattamente dov'era).</summary>
        public static IEnumerator Shake(Transform t, float amplitude, float seconds)
        {
            if (t == null) yield break;
            Vector3 home = t.localPosition;
            for (float k = 0f; k < seconds; k += Time.deltaTime)
            {
                float a = amplitude * (1f - k / seconds);
                t.localPosition = home + new Vector3(Random.Range(-a, a), Random.Range(-a, a), 0f);
                yield return null;
            }
            t.localPosition = home;
        }

        /// <summary>Oscillazione (rotazione) avanti e indietro, per "gongolare" o tremare di paura.</summary>
        public static IEnumerator Wiggle(Transform t, float degrees, float seconds, float speed = 18f)
        {
            if (t == null) yield break;
            Quaternion home = t.localRotation;
            for (float k = 0f; k < seconds; k += Time.deltaTime)
            {
                t.localRotation = home * Quaternion.Euler(0f, 0f, Mathf.Sin(k * speed) * degrees);
                yield return null;
            }
            t.localRotation = home;
        }

        public static IEnumerator Fade(SpriteRenderer sr, float to, float seconds)
        {
            if (sr == null) yield break;
            float from = sr.color.a;
            for (float k = 0f; k < 1f; k += Time.deltaTime / Mathf.Max(0.01f, seconds))
            {
                SetAlpha(sr, Mathf.Lerp(from, to, k));
                yield return null;
            }
            SetAlpha(sr, to);
        }

        public static void SetAlpha(SpriteRenderer sr, float a)
        {
            if (sr == null) return;
            Color c = sr.color;
            c.a = a;
            sr.color = c;
        }

        public static void Show(Component c, bool on) { if (c != null) c.gameObject.SetActive(on); }
        public static void Show(GameObject g, bool on) { if (g != null) g.SetActive(on); }

        public static void Confetti(SmokeEmitter[] confetti, bool on)
        {
            if (confetti == null) return;
            foreach (SmokeEmitter c in confetti)
                if (c != null) { if (on) c.Play(); else c.Stop(); }
        }

        /// <summary>Uno sbuffo singolo (l'emettitore va per un attimo e si ferma).</summary>
        public static IEnumerator Burst(SmokeEmitter e, float seconds)
        {
            if (e == null) yield break;
            e.Play();
            yield return new WaitForSeconds(seconds);
            e.Stop();
        }
    }

    /// <summary>
    /// Trascinamento col dito di più oggetti (uno alla volta): si afferra l'oggetto più in alto sotto il dito, lo si
    /// tiene dentro l'area sicura e lo si rimanda a casa con un'animazione che si interrompe se lo si riafferra.
    /// Il livello decide cosa succede al rilascio (posizioni nel mondo, niente coordinate dello schermo).
    /// </summary>
    public sealed class HwDragger
    {
        public sealed class Item
        {
            public readonly Transform T;
            public readonly Collider2D Area;
            public Vector3 Home;
            public bool Enabled = true;
            public int Priority;
            public float Lift = 1.08f;
            internal Vector3 BaseScale;
            internal Coroutine Moving;

            public Item(Transform t, Collider2D area, int priority)
            {
                T = t;
                Area = area;
                Priority = priority;
                Home = t.position;
                BaseScale = t.localScale;
            }
        }

        readonly MonoBehaviour host;
        readonly List<Item> items = new List<Item>();
        Vector2 offset;

        public Item Held { get; private set; }

        public HwDragger(MonoBehaviour host) { this.host = host; }

        public Item Add(Transform t, Collider2D area, int priority = 0)
        {
            var it = new Item(t, area, priority);
            items.Add(it);
            return it;
        }

        /// <summary>Afferra l'oggetto abilitato (priorità più alta) sotto il dito; null se non c'è niente.</summary>
        public Item TryPick(Vector2 world)
        {
            if (Held != null) return null;
            Item best = null;
            foreach (Item it in items)
            {
                if (!it.Enabled || it.T == null || it.Area == null || !it.T.gameObject.activeInHierarchy) continue;
                if (!it.Area.OverlapPoint(world)) continue;
                if (best == null || it.Priority > best.Priority) best = it;
            }
            if (best == null) return null;
            Stop(best);
            Held = best;
            offset = (Vector2)best.T.position - world;
            best.T.localScale = best.BaseScale * best.Lift;
            Haptics.Tap();
            return best;
        }

        public void MoveHeld(Vector2 world)
        {
            if (Held == null) return;
            Vector2 p = HalloweenKit.ClampSafe(world + offset);
            Held.T.position = new Vector3(p.x, p.y, Held.T.position.z);
        }

        /// <summary>Lascia l'oggetto tenuto (resta dov'è: decide il livello) e lo restituisce.</summary>
        public Item Release()
        {
            Item it = Held;
            Held = null;
            if (it != null && it.T != null) it.T.localScale = it.BaseScale;
            return it;
        }

        /// <summary>Riporta l'oggetto a casa (animato).</summary>
        public void SendHome(Item it, float seconds = 0.3f) { if (it != null) SendTo(it, it.Home, seconds); }

        public void SendTo(Item it, Vector3 world, float seconds = 0.3f, float arc = 0.25f)
        {
            if (it == null || it.T == null || host == null) return;
            Stop(it);
            if (!host.isActiveAndEnabled) { it.T.position = world; return; }
            it.Moving = host.StartCoroutine(HalloweenKit.Move(it.T, world, seconds, arc));
        }

        /// <summary>La scala di riposo è cambiata (es. l'oggetto è stato ingrandito): la si memorizza.</summary>
        public void SetBaseScale(Item it, Vector3 scale) { if (it != null) { it.BaseScale = scale; if (Held != it && it.T != null) it.T.localScale = scale; } }

        public void Stop(Item it)
        {
            if (it == null || it.Moving == null || host == null) return;
            host.StopCoroutine(it.Moving);
            it.Moving = null;
        }

        /// <summary>Annulla la presa (es. il livello si blocca per un'animazione): l'oggetto torna a casa.</summary>
        public void Cancel()
        {
            Item it = Release();
            if (it != null) SendHome(it);
        }
    }

    /// <summary>Scritta di commento (TextMesh) che compare con un rimbalzo e sparisce da sola.</summary>
    public sealed class HwToast
    {
        readonly TextMesh text;
        readonly Transform holder;
        float hideAt = -1f, popT = 1f;
        Vector3 baseScale = Vector3.one;

        /// <summary>holder: l'oggetto da mostrare/nascondere (es. la fascia scura con la scritta).</summary>
        public HwToast(TextMesh text, Transform holder)
        {
            this.text = text;
            this.holder = holder != null ? holder : (text != null ? text.transform : null);
            if (this.holder != null) { baseScale = this.holder.localScale; this.holder.gameObject.SetActive(false); }
        }

        public void Show(string message, float seconds = 1.8f)
        {
            if (text == null || holder == null) return;
            text.text = message;
            holder.gameObject.SetActive(true);
            hideAt = Time.time + seconds;
            popT = 0f;
        }

        public void Tick()
        {
            if (holder == null) return;
            if (popT < 1f)
            {
                popT = Mathf.Min(1f, popT + Time.deltaTime / 0.2f);
                holder.localScale = baseScale * (1f + 0.15f * Mathf.Sin(popT * Mathf.PI));
            }
            if (hideAt > 0f && Time.time >= hideAt) { hideAt = -1f; holder.gameObject.SetActive(false); }
        }
    }

    /// <summary>Fumetto che si nasconde da solo dopo qualche secondo.</summary>
    public sealed class HwSay
    {
        readonly SpeechBubble bubble;
        float hideAt = -1f;

        public HwSay(SpeechBubble bubble) { this.bubble = bubble; }

        public void Show(string text, float seconds = 2.2f)
        {
            if (bubble == null) return;
            bubble.Show(text, true);
            hideAt = Time.time + seconds;
        }

        public void Hide()
        {
            hideAt = -1f;
            if (bubble != null) bubble.Hide();
        }

        public void Tick()
        {
            if (hideAt > 0f && Time.time >= hideAt) Hide();
        }
    }
}
