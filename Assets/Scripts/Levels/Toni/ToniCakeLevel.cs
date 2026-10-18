using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 100 — "TONI'S BIRTHDAY". Toni deve spegnere la candelina... ma un carlino non sa soffiare.
    /// Toccarlo lo fa abbaiare (la fiamma tremola e basta), toccare la candela scotta. La soluzione: fargli il
    /// solletico sul naso con la piuma finché non starnutisce. ETCIÙ! La candelina si spegne... e la torta vola
    /// dritta sullo schermo. Toni la lecca via, a grandi linguate.
    /// </summary>
    public class ToniCakeLevel : LevelController
    {
        [SerializeField] ToniPug toni;
        [SerializeField] Draggable2D feather;
        [SerializeField] Transform cake;
        [SerializeField] Transform flame;
        [SerializeField] SmokeEmitter candleSmoke;
        [SerializeField] SmokeEmitter sneezeCloud;
        [SerializeField] SmokeEmitter confetti;
        [SerializeField] SpriteRenderer splat;
        [SerializeField] Transform bigFace;            // il muso di Toni in primo piano, dietro la panna sul vetro
        [SerializeField] Transform tonguePivot;        // base della linguona (nella bocca del muso grande)
        [SerializeField] Transform[] cream;            // gli schizzi di panna sullo schermo
        [SerializeField] SpeechBubble candleBubble;
        [SerializeField] float tickleRadius = 0.6f;
        [SerializeField] float sneezeAt = 4f;

        Camera cam;
        float tickle, nextBark, flicker, hideCandleBubble = -1f;
        Vector3 lastFeather, flameScale;
        int stage;
        bool sneezed;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            toni.Stand(true);
            toni.WakeUp(2);
            toni.LookTarget = flame;
            toni.Wag = 0.6f;
            flameScale = flame.localScale;
            lastFeather = feather.transform.position;
            if (splat != null) splat.gameObject.SetActive(false);
            if (bigFace != null) bigFace.gameObject.SetActive(false);
            if (cream != null) foreach (Transform c in cream) if (c != null) c.gameObject.SetActive(false);
            if (candleBubble != null) candleBubble.Hide();
            nextBark = Time.time + 2.5f;
        }

        static float DistanceToSegment(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a;
            float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / Mathf.Max(ab.sqrMagnitude, 1e-4f));
            return Vector2.Distance(p, a + ab * t);
        }

        /// <summary>Uno schizzo leccato via: si schiaccia, sbiadisce e sparisce.</summary>
        IEnumerator Wipe(Transform c)
        {
            Vector3 s0 = c.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.18f)
            {
                c.localScale = new Vector3(s0.x * (1f + 0.3f * k), s0.y * (1f - k), s0.z);
                yield return null;
            }
            c.gameObject.SetActive(false);
        }

        void Update()
        {
            if (IsFinished || sneezed) return;
            if (hideCandleBubble > 0f && Time.time > hideCandleBubble) { hideCandleBubble = -1f; candleBubble.Hide(); }
            // la fiamma danza (e tremola di più quando Toni abbaia)
            flicker = Mathf.MoveTowards(flicker, 0f, Time.deltaTime * 2f);
            flame.localScale = new Vector3(flameScale.x * (1f + Mathf.Sin(Time.time * 17f) * 0.06f - flicker * 0.3f), flameScale.y * (1f + Mathf.Sin(Time.time * 23f) * 0.08f - flicker * 0.4f), 1f);
            flame.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(Time.time * 11f) * 5f + flicker * 25f * Mathf.Sin(Time.time * 40f));

            bool dragging = Draggable2D.Active == feather;
            toni.LookTarget = dragging ? feather.transform : flame;
            Vector3 f = feather.transform.position;
            if (dragging && Vector2.Distance(f, toni.NoseWorld) < tickleRadius)
                tickle += Vector2.Distance(f, lastFeather) * 1.6f;   // conta il movimento della piuma sul naso
            else
                tickle = Mathf.MoveTowards(tickle, 0f, Time.deltaTime * 0.8f);
            lastFeather = f;
            UpdateTickle();

            if (!dragging && Time.time >= nextBark && stage == 0)
            {
                nextBark = Time.time + Random.Range(4f, 6f);
                toni.Say(Random.value < 0.5f ? "Cake? CAKE!" : "Woof!", 1.1f, "woof");
                flicker = 1f;
            }
            if (!dragging && PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out _) && began && !PointerInput.IsOverUi() && cam != null)
            {
                Vector2 w = PointerInput.ToWorld(cam, sp);
                if (Vector2.Distance(w, flame.position) < 0.45f)
                {
                    candleBubble.Show("OUCH! HOT!");
                    hideCandleBubble = Time.time + 1.2f;
                    Sfx.Play(this, "buzz", 0.4f);
                }
                else if (Vector2.Distance(w, toni.HeadWorld) < 0.8f)
                {
                    toni.Say(Random.value < 0.5f ? "WOOF!" : "*pant pant*", 1f, "woof");
                    flicker = 1f;   // il fiatone muove appena la fiamma: ci vuole ben altro
                }
            }
        }

        void UpdateTickle()
        {
            int s = tickle > sneezeAt ? 4 : tickle > sneezeAt * 0.7f ? 3 : tickle > sneezeAt * 0.45f ? 2 : tickle > sneezeAt * 0.2f ? 1 : 0;
            if (s == stage) return;
            if (s > stage)
            {
                string[] lines = { "", "ah...", "aah...", "AAAH...", "" };
                if (s < 4) toni.Say(lines[s], 1.5f);
                toni.MouthOpen(s >= 2);
                toni.Eyes(s >= 2 ? 1 : 2);
            }
            else if (s == 0) { toni.MouthOpen(false); toni.Eyes(2); toni.Say("...hm?", 0.8f); }
            stage = s;
            if (stage == 4) StartCoroutine(SneezeRoutine());
        }

        IEnumerator SneezeRoutine()
        {
            sneezed = true;
            feather.enabled = false;   // la piuma vola via con lo starnuto
            toni.Eyes(0);
            toni.MouthOpen(false);
            // testa indietro... ETCIÙ!
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.25f) { toni.transform.localRotation = Quaternion.Euler(0, 0, -10f * k); yield return null; }
            toni.transform.localRotation = Quaternion.Euler(0, 0, 6f);
            toni.MouthOpen(true);
            toni.Say("ACHOO!!", 1.6f, "whoosh");
            Sfx.Play(this, "pop", 1f);
            Haptics.Medium();
            if (sneezeCloud != null) { sneezeCloud.transform.position = toni.NoseWorld; sneezeCloud.Play(); }
            Vector3 fp = feather.transform.position;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.3f)
            {
                feather.transform.position = fp + new Vector3(-2.5f * k, 1.5f * k, 0f);
                feather.transform.Rotate(0, 0, 900f * Time.deltaTime);
                flame.localScale = Vector3.Lerp(flame.localScale, Vector3.zero, k);
                yield return null;
            }
            flame.gameObject.SetActive(false);
            feather.gameObject.SetActive(false);
            if (candleSmoke != null) { candleSmoke.transform.position = flame.position; candleSmoke.Play(); }
            if (sneezeCloud != null) sneezeCloud.Stop();
            toni.MouthOpen(false);
            toni.transform.localRotation = Quaternion.identity;
            toni.Eyes(2);
            yield return new WaitForSeconds(0.35f);

            // ...e la torta vola dritta contro lo schermo
            Vector3 c0 = cake.position, s0 = cake.localScale;
            Vector3 center = cam != null ? new Vector3(cam.transform.position.x, cam.transform.position.y, c0.z) : new Vector3(0, 0, c0.z);
            Sfx.Play(this, "whoosh", 0.8f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.4f)
            {
                cake.position = Vector3.Lerp(c0, center, k) + Vector3.up * Mathf.Sin(k * Mathf.PI) * 1.2f;
                cake.localScale = s0 * Mathf.Lerp(1f, 4.5f, k * k);
                cake.Rotate(0, 0, 500f * Time.deltaTime);
                yield return null;
            }
            cake.gameObject.SetActive(false);
            if (candleSmoke != null) candleSmoke.Stop();
            splat.gameObject.SetActive(true);
            splat.color = Color.white;
            Sfx.Play(this, "bonk", 1f);
            Haptics.Heavy();
            // gli schizzi di panna arrivano sul vetro uno dopo l'altro
            var creamScale = new Vector3[cream.Length];
            for (int i = 0; i < cream.Length; i++)
            {
                creamScale[i] = cream[i].localScale;
                cream[i].gameObject.SetActive(true);
                cream[i].localScale = Vector3.zero;
            }
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.35f)
            {
                for (int i = 0; i < cream.Length; i++)
                {
                    float t = Mathf.Clamp01(k * 1.6f - i * 0.06f);
                    cream[i].localScale = creamScale[i] * (t < 0.7f ? t / 0.7f * 1.15f : Mathf.Lerp(1.15f, 1f, (t - 0.7f) / 0.3f));
                }
                yield return null;
            }
            yield return new WaitForSeconds(0.4f);

            // il muso di Toni sale da sotto, dietro la panna, e si schiaccia contro il vetro
            bigFace.gameObject.SetActive(true);
            Vector3 faceHome = bigFace.localPosition, faceScale = bigFace.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.5f)
            {
                float e = 1f - (1f - k) * (1f - k);
                bigFace.localPosition = faceHome + Vector3.down * 6f * (1f - e);
                yield return null;
            }
            bigFace.localPosition = faceHome;
            Sfx.Play(this, "bonk", 0.6f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.2f)   // "SPLOT": si spiaccica sul vetro
            {
                bigFace.localScale = new Vector3(faceScale.x * (1f + 0.08f * Mathf.Sin(k * Mathf.PI)), faceScale.y * (1f - 0.06f * Mathf.Sin(k * Mathf.PI)), faceScale.z);
                yield return null;
            }
            bigFace.localScale = faceScale;
            toni.Say("Mmmh... CAKE!", 1f, "woof");

            // leccate: la lingua si allunga verso uno schizzo, spazza di lato e pulisce tutto quello che tocca
            var left = new System.Collections.Generic.List<int>();
            for (int i = 0; i < cream.Length; i++) left.Add(i);
            float splatAlpha = 1f;
            for (int lick = 0; lick < 8 && left.Count > 0; lick++)
            {
                Vector3 mouthW = tonguePivot.position;
                // bersaglio: lo schizzo più lontano tra quelli rimasti (così ogni passata ne prende tanti)
                int target = left[0];
                foreach (int i in left) if ((cream[i].position - mouthW).sqrMagnitude > (cream[target].position - mouthW).sqrMagnitude) target = i;
                Vector2 to = cream[target].position - mouthW;
                float reach = to.magnitude + 0.35f, aim = Mathf.Atan2(to.y, to.x) * Mathf.Rad2Deg - 90f;
                float side = lick % 2 == 0 ? -1f : 1f, from = aim - 28f * side, until = aim + 28f * side;
                float sx = tonguePivot.localScale.x;
                tonguePivot.gameObject.SetActive(true);
                // fuori la lingua
                for (float k = 0f; k < 1f; k += Time.deltaTime / 0.15f)
                {
                    tonguePivot.rotation = Quaternion.Euler(0, 0, from);
                    tonguePivot.localScale = new Vector3(sx, Mathf.Lerp(0.05f, reach / 2.2f, k) / bigFace.lossyScale.y, 1f);
                    yield return null;
                }
                Sfx.Play(this, "whoosh", 0.7f);
                Haptics.Light();
                // passata di lato: pulisce
                for (float k = 0f; k < 1f; k += Time.deltaTime / 0.35f)
                {
                    float ang = Mathf.Lerp(from, until, Mathf.SmoothStep(0f, 1f, k));
                    tonguePivot.rotation = Quaternion.Euler(0, 0, ang);
                    bigFace.localScale = faceScale * (1f + 0.03f * Mathf.Sin(k * Mathf.PI));
                    Vector3 tip = mouthW + Quaternion.Euler(0, 0, ang) * Vector3.up * reach;
                    for (int n = left.Count - 1; n >= 0; n--)
                    {
                        Transform c = cream[left[n]];
                        if (DistanceToSegment(c.position, mouthW, tip) < 0.75f)
                        {
                            StartCoroutine(Wipe(c));
                            left.RemoveAt(n);
                        }
                    }
                    splatAlpha = Mathf.MoveTowards(splatAlpha, left.Count / (float)cream.Length, Time.deltaTime * 3f);
                    splat.color = new Color(1f, 1f, 1f, splatAlpha);
                    yield return null;
                }
                // dentro la lingua (si lecca i baffi)
                for (float k = 0f; k < 1f; k += Time.deltaTime / 0.15f)
                {
                    tonguePivot.localScale = new Vector3(sx, Mathf.Lerp(reach / 2.2f, 0.05f, k) / bigFace.lossyScale.y, 1f);
                    yield return null;
                }
                tonguePivot.gameObject.SetActive(false);
                Sfx.Play(this, "pop", 0.4f);
                yield return new WaitForSeconds(0.08f);
            }
            foreach (int i in left) StartCoroutine(Wipe(cream[i]));
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.25f) { splat.color = new Color(1f, 1f, 1f, splatAlpha * (1f - k)); yield return null; }
            splat.gameObject.SetActive(false);
            toni.Say("*burp*", 1f, "bonk");
            yield return new WaitForSeconds(0.5f);
            // il muso torna giù
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.4f)
            {
                bigFace.localPosition = faceHome + Vector3.down * 6f * k * k;
                yield return null;
            }
            bigFace.gameObject.SetActive(false);
            if (confetti != null) confetti.Play();
            StartCoroutine(toni.Happy(2f));
            toni.Say("Best birthday ever!", 2f, "woof");
            Sfx.Play(this, "ding", 0.8f);
            yield return new WaitForSeconds(2.2f);
            Win();
        }
    }
}
