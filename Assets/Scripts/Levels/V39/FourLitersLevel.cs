using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Four liters — il classico indovinello delle caraffe. Il gatto vuole ESATTAMENTE 4 litri nella ciotola, ma hai
    /// solo una caraffa da 5 L e una da 3 L. Trascina una caraffa sotto il rubinetto per riempirla, su quell'altra per
    /// travasare (finché una si svuota o l'altra è piena), nel lavandino per svuotarla, sulla ciotola per versare tutto.
    /// Soluzione: riempi la 5, versala nella 3 (restano 2), svuota la 3, versa i 2 nella 3, riempi la 5, rabbocca la 3
    /// (nella 5 restano 4) e versa la 5 nella ciotola. Se nella ciotola non arrivano 4 litri il gatto rifiuta.
    /// </summary>
    public class FourLitersLevel : LevelController
    {
        [SerializeField] Transform[] jugs;          // 0 = 5 L, 1 = 3 L (origine = fondo interno)
        [SerializeField] Transform[] waters;        // perno della barra d'acqua (scala y = altezza)
        [SerializeField] TextMesh[] labels;
        [SerializeField] int[] capacity = { 5, 3 };
        [SerializeField] float literHeight = 0.36f;
        [SerializeField] Transform tapSpot;         // dove si appoggia il fondo della caraffa sotto il rubinetto
        [SerializeField] Transform tapMouth;
        [SerializeField] Transform sink;
        [SerializeField] Transform bowl;
        [SerializeField] Transform bowlWater;
        [SerializeField] SpriteRenderer stream;     // getto d'acqua (barra azzurra, scalata)
        [SerializeField] Transform[] pupils;        // gli occhi del gatto seguono la caraffa
        [SerializeField] SpeechBubble bubble;
        [SerializeField] int goal = 4;

        int[] amount;
        Vector3[] home;
        int dragging = -1;
        Vector3 grabOffset;
        Vector3 bowlWaterScale;
        Vector3[] pupilHome;
        bool busy, done;
        Camera cam;

        float H(int j) => capacity[j] * literHeight;
        Vector3 Center(int j) => jugs[j].position + jugs[j].rotation * new Vector3(0f, H(j) * 0.5f, 0f);
        Vector3 SpoutLocal(int j) => new Vector3(-0.72f, H(j) + 0.2f, 0f);

        public override void OnLevelStart()
        {
            cam = Camera.main;
            amount = new int[jugs.Length];
            home = new Vector3[jugs.Length];
            for (int j = 0; j < jugs.Length; j++)
            {
                home[j] = jugs[j].position;
                SetWater(j, 0f);
            }
            pupilHome = new Vector3[pupils.Length];
            for (int i = 0; i < pupils.Length; i++) pupilHome[i] = pupils[i].localPosition;
            bowlWaterScale = bowlWater.localScale;
            bowlWater.gameObject.SetActive(false);
            stream.enabled = false;
            if (bubble != null) { bubble.Show("I want EXACTLY 4 liters. Not 3. Not 5. FOUR.", true); StartCoroutine(HideLater(2.8f)); }
        }

        void SetWater(int j, float liters)
        {
            waters[j].localScale = new Vector3(1f, Mathf.Max(0.0001f, liters * literHeight), 1f);
            waters[j].gameObject.SetActive(liters > 0.01f);
            labels[j].text = Mathf.RoundToInt(liters) + " / " + capacity[j] + " L";
        }

        void Update()
        {
            LookAt();
            if (busy || done || IsFinished) return;
            if (!PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out bool ended))
            {
                if (dragging >= 0) Release();
                return;
            }
            Vector3 w = PointerInput.ToWorld(cam, sp);
            if (began && dragging < 0)
            {
                for (int j = 0; j < jugs.Length; j++)
                {
                    Vector3 c = Center(j);
                    if (Mathf.Abs(w.x - c.x) < 0.85f && Mathf.Abs(w.y - c.y) < H(j) * 0.5f + 0.4f)
                    {
                        dragging = j;
                        grabOffset = jugs[j].position - w;
                        SetOrder(j, 20);
                        Sfx.Play(this, "tick", 0.6f);
                    }
                }
                return;
            }
            if (dragging < 0) return;
            Vector3 to = w + grabOffset;
            to.x = Mathf.Clamp(to.x, -2.5f, 2.5f);
            to.y = Mathf.Clamp(to.y, -4.2f, 2.2f);
            jugs[dragging].position = to;
            if (ended) Release();
        }

        void SetOrder(int j, int lift)
        {
            foreach (SpriteRenderer r in jugs[j].GetComponentsInChildren<SpriteRenderer>(true)) r.sortingOrder += lift;
            foreach (MeshRenderer r in jugs[j].GetComponentsInChildren<MeshRenderer>(true)) r.sortingOrder += lift;
        }

        void LookAt()
        {
            Vector3 target = dragging >= 0 ? jugs[dragging].position : bowl.position + Vector3.up;
            for (int i = 0; i < pupils.Length; i++)
            {
                Vector3 d = target - pupils[i].parent.TransformPoint(pupilHome[i]);
                pupils[i].localPosition = pupilHome[i] + (Vector3)(Vector2.ClampMagnitude(d, 1f) * 0.12f);
            }
        }

        void Release()
        {
            int j = dragging;
            dragging = -1;
            SetOrder(j, -20);
            Vector3 c = Center(j);
            int other = 1 - j;
            float dOther = Vector2.Distance(c, Center(other));
            float dTap = Vector2.Distance(c, tapSpot.position + Vector3.up * H(j) * 0.5f);
            float dSink = Vector2.Distance(c, sink.position + Vector3.up * 0.4f);
            float dBowl = Vector2.Distance(c, bowl.position + Vector3.up * 0.4f);
            float best = Mathf.Min(Mathf.Min(dOther, dTap), Mathf.Min(dSink, dBowl));
            if (best > 1.15f) { StartCoroutine(GoHome(j)); return; }
            if (best == dTap) StartCoroutine(Fill(j));
            else if (best == dOther) StartCoroutine(PourInto(j, other));
            else if (best == dSink) StartCoroutine(Empty(j));
            else StartCoroutine(Serve(j));
        }

        IEnumerator Slide(Transform t, Vector3 pos, Quaternion rot, float seconds)
        {
            Vector3 p0 = t.position;
            Quaternion r0 = t.rotation;
            for (float k = 0f; k < 1f; k += Time.deltaTime / seconds)
            {
                float e = 1f - (1f - k) * (1f - k);
                t.position = Vector3.Lerp(p0, pos, e);
                t.rotation = Quaternion.Slerp(r0, rot, e);
                yield return null;
            }
            t.position = pos;
            t.rotation = rot;
        }

        IEnumerator GoHome(int j)
        {
            busy = true;
            yield return Slide(jugs[j], home[j], Quaternion.identity, 0.25f);
            busy = false;
        }

        /// <summary>
        /// Inclina la caraffa in modo che il beccuccio finisca sopra "target". Se così uscirebbe dallo schermo a destra,
        /// la caraffa si specchia e versa dall'altra parte (l'etichetta si raddrizza).
        /// </summary>
        IEnumerator Tilt(int j, Vector3 target)
        {
            Quaternion rot = Quaternion.Euler(0, 0, 75f);
            Vector3 pos = target - rot * SpoutLocal(j);
            if (pos.x > 2.3f)
            {
                Mirror(j, true);
                rot = Quaternion.Euler(0, 0, -75f);
                Vector3 s = SpoutLocal(j);
                pos = target - rot * new Vector3(-s.x, s.y, 0f);
            }
            SetOrder(j, 20);
            yield return Slide(jugs[j], pos, rot, 0.35f);
        }

        void Mirror(int j, bool on)
        {
            float sx = on ? -1f : 1f;
            jugs[j].localScale = new Vector3(sx, 1f, 1f);
            Vector3 ls = labels[j].transform.localScale;
            labels[j].transform.localScale = new Vector3(Mathf.Abs(ls.x) * sx, ls.y, ls.z);
        }

        IEnumerator Return(int j)
        {
            SetOrder(j, -20);
            yield return Slide(jugs[j], home[j], Quaternion.identity, 0.3f);
            Mirror(j, false);
        }

        void Stream(Vector3 from, Vector3 to)
        {
            stream.enabled = true;
            stream.transform.position = (from + to) * 0.5f;
            stream.transform.localScale = new Vector3(0.1f, Mathf.Max(0.01f, from.y - to.y), 1f);
        }

        IEnumerator Animate(int j, float from, float to, int other, float otherFrom, float otherTo, float seconds)
        {
            for (float k = 0f; k < 1f; k += Time.deltaTime / seconds)
            {
                SetWater(j, Mathf.Lerp(from, to, k));
                if (other >= 0) SetWater(other, Mathf.Lerp(otherFrom, otherTo, k));
                yield return null;
            }
            SetWater(j, to);
            if (other >= 0) SetWater(other, otherTo);
        }

        IEnumerator Fill(int j)
        {
            busy = true;
            yield return Slide(jugs[j], tapSpot.position, Quaternion.identity, 0.2f);
            if (amount[j] < capacity[j])
            {
                Stream(tapMouth.position, tapSpot.position + Vector3.up * amount[j] * literHeight);
                Sfx.Play(this, "whoosh", 0.5f);
                yield return Animate(j, amount[j], capacity[j], -1, 0, 0, 0.25f + 0.15f * (capacity[j] - amount[j]));
                amount[j] = capacity[j];
                stream.enabled = false;
            }
            yield return Slide(jugs[j], home[j], Quaternion.identity, 0.25f);
            busy = false;
        }

        IEnumerator PourInto(int j, int other)
        {
            busy = true;
            yield return Slide(jugs[other], home[other], Quaternion.identity, 0.01f);
            Vector3 mouth = home[other] + new Vector3(0f, H(other) + 0.35f, 0f);
            yield return Tilt(j, mouth);
            int moved = Mathf.Min(amount[j], capacity[other] - amount[other]);
            if (moved > 0)
            {
                Stream(mouth, home[other] + Vector3.up * amount[other] * literHeight);
                Sfx.Play(this, "whoosh", 0.5f);
                yield return Animate(j, amount[j], amount[j] - moved, other, amount[other], amount[other] + moved, 0.3f + 0.15f * moved);
                amount[j] -= moved;
                amount[other] += moved;
                stream.enabled = false;
            }
            else Sfx.Play(this, "bonk", 0.5f);
            yield return Return(j);
            busy = false;
        }

        IEnumerator Empty(int j)
        {
            busy = true;
            Vector3 mouth = sink.position + new Vector3(0f, 0.6f, 0f);
            yield return Tilt(j, mouth);
            if (amount[j] > 0)
            {
                Stream(mouth, sink.position);
                Sfx.Play(this, "whoosh", 0.5f);
                yield return Animate(j, amount[j], 0, -1, 0, 0, 0.25f + 0.1f * amount[j]);
                amount[j] = 0;
                stream.enabled = false;
            }
            yield return Return(j);
            busy = false;
        }

        IEnumerator Serve(int j)
        {
            busy = true;
            Vector3 mouth = bowl.position + new Vector3(0f, 0.9f, 0f);
            yield return Tilt(j, mouth);
            int poured = amount[j];
            if (poured > 0)
            {
                Stream(mouth, bowl.position);
                Sfx.Play(this, "whoosh", 0.5f);
                bowlWater.gameObject.SetActive(true);
                for (float k = 0f; k < 1f; k += Time.deltaTime / (0.3f + 0.12f * poured))
                {
                    SetWater(j, Mathf.Lerp(poured, 0f, k));
                    bowlWater.localScale = bowlWaterScale * Mathf.Lerp(0.3f, Mathf.Lerp(0.5f, 1f, poured / 5f), k);
                    yield return null;
                }
                SetWater(j, 0f);
                amount[j] = 0;
                stream.enabled = false;
            }
            StartCoroutine(Return(j));
            if (poured == goal) { StartCoroutine(Drink()); yield break; }
            if (bubble != null) bubble.Show(Complaint(poured), true);
            Sfx.Play(this, "buzz", 0.4f);
            yield return new WaitForSeconds(1.6f);
            // il gatto rovescia la ciotola con una zampata
            Sfx.Play(this, "bonk", 0.6f);
            Haptics.Light();
            Vector3 b0 = bowl.localPosition;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.3f)
            {
                bowl.localPosition = b0 + Vector3.right * Mathf.Sin(k * Mathf.PI * 4f) * 0.1f;
                bowlWater.localScale = bowlWaterScale * Mathf.Lerp(1f, 0f, k);
                yield return null;
            }
            bowl.localPosition = b0;
            bowlWater.gameObject.SetActive(false);
            if (bubble != null) bubble.Hide();
            busy = false;
        }

        static string Complaint(int liters)
        {
            switch (liters)
            {
                case 0: return "An empty jug? Hilarious.";
                case 1: return "That's not 4! That's ONE. I'm a cat, not a hamster!";
                case 2: return "That's not 4! Two is HALF of four!";
                case 3: return "That's not 4! Three... so close, yet so dry.";
                case 5: return "That's not 4! Five?! Do I look like a goldfish?";
                default: return "That's not 4!";
            }
        }

        IEnumerator Drink()
        {
            done = true;
            Sfx.Play(this, "ding", 0.8f);
            if (bubble != null) bubble.Show("Exactly four! Purr-fect math!", true);
            Vector3 w0 = bowlWater.localScale;
            for (float k = 0f; k < 1.6f; k += Time.deltaTime)
            {
                bowlWater.localScale = Vector3.Lerp(w0, w0 * 0.5f, k / 1.6f);
                if (Mathf.Repeat(k, 0.4f) < Time.deltaTime) Sfx.Play(this, "pop", 0.3f);   // slurp slurp
                yield return null;
            }
            Win();
        }

        IEnumerator HideLater(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (bubble != null && !done && !busy) bubble.Hide();
        }
    }
}
