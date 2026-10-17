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
        [SerializeField] Transform bigTongue;
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
            if (bigTongue != null) bigTongue.gameObject.SetActive(false);
            if (candleBubble != null) candleBubble.Hide();
            nextBark = Time.time + 2.5f;
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
            yield return new WaitForSeconds(0.6f);
            toni.Say("!!!", 0.8f);
            yield return new WaitForSeconds(0.6f);

            // tre leccate e lo schermo è pulito
            Vector3 splatScale = splat.transform.localScale;
            bigTongue.gameObject.SetActive(true);
            Vector3 t0 = bigTongue.position;
            for (int lick = 0; lick < 3; lick++)
            {
                float side = lick % 2 == 0 ? -1f : 1f;
                for (float k = 0f; k < 1f; k += Time.deltaTime / 0.45f)
                {
                    bigTongue.position = t0 + new Vector3(side * Mathf.Lerp(-0.8f, 0.8f, k), Mathf.Sin(k * Mathf.PI) * 6.5f, 0f);
                    bigTongue.localRotation = Quaternion.Euler(0, 0, side * Mathf.Lerp(15f, -15f, k));
                    yield return null;
                }
                Sfx.Play(this, "whoosh", 0.6f);
                float left = 1f - (lick + 1) / 3f;
                splat.color = new Color(1f, 1f, 1f, left);
                splat.transform.localScale = splatScale * (0.6f + 0.4f * left);
            }
            bigTongue.gameObject.SetActive(false);
            splat.gameObject.SetActive(false);
            if (confetti != null) confetti.Play();
            StartCoroutine(toni.Happy(2f));
            toni.Say("Best birthday ever!", 2f, "woof");
            Sfx.Play(this, "ding", 0.8f);
            yield return new WaitForSeconds(2.2f);
            Win();
        }
    }
}
