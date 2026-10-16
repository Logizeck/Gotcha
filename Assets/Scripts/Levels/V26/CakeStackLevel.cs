using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 95 — la torta a piani. Ogni strato scorre avanti e indietro sopra la torta: tocca per lasciarlo cadere.
    /// Quello che sporge viene tagliato e cade, quindi gli strati diventano sempre più stretti. Ogni terzo strato è
    /// di gelatina: tremola e cambia velocità. Otto strati e ci va la ciliegina.
    /// </summary>
    public class CakeStackLevel : LevelController
    {
        [SerializeField] SpriteRenderer layerTemplate;   // sprite "plain" disattivato
        [SerializeField] Transform plate;               // centro del piatto, in cima
        [SerializeField] Transform cherry;
        [SerializeField] TextMesh counter;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] int needed = 8;
        [SerializeField] float startWidth = 2.4f, layerHeight = 0.38f;
        [SerializeField] float speed = 2.2f, speedUp = 0.25f, sweep = 2.3f;

        static readonly Color[] cakes = { new Color(0.98f, 0.85f, 0.6f), new Color(0.55f, 0.33f, 0.22f), new Color(1f, 0.7f, 0.78f) };
        readonly List<Transform> stack = new List<Transform>();
        Transform moving;
        float width, centerX, phase, top;
        int count;
        bool done, busy;
        Camera cam;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            width = startWidth;
            centerX = plate.position.x;
            top = plate.position.y;
            cherry.gameObject.SetActive(false);
            Count();
            Next();
            if (bubble != null) { bubble.Show("Tap to drop each layer!"); StartCoroutine(HideLater(1.6f)); }
        }

        bool Jelly => count % 3 == 2;
        void Count() { counter.text = count + " / " + needed; }

        Transform Layer(float w, Color c)
        {
            SpriteRenderer sr = Instantiate(layerTemplate, transform);
            sr.gameObject.SetActive(true);
            sr.color = c;
            sr.transform.localScale = new Vector3(w, layerHeight, 1f);
            sr.sortingOrder = 5 + count;
            // bordo scuro (doodle)
            SpriteRenderer edge = Instantiate(layerTemplate, sr.transform);
            edge.gameObject.SetActive(true);
            edge.transform.localPosition = Vector3.zero;
            edge.transform.localScale = new Vector3(1f + 0.06f / w, 1f + 0.06f / layerHeight, 1f);
            edge.color = new Color(0.17f, 0.13f, 0.2f);
            edge.sortingOrder = sr.sortingOrder - 1;
            sr.sortingOrder += 20;
            return sr.transform;
        }

        void Next()
        {
            Color c = Jelly ? new Color(0.45f, 0.95f, 0.5f, 0.9f) : cakes[count % cakes.Length];
            moving = Layer(width, c);
            phase = Random.value < 0.5f ? 0f : Mathf.PI;
        }

        void Update()
        {
            if (done || busy || IsFinished || moving == null) return;
            float v = speed + speedUp * count;
            phase += Time.deltaTime * v / sweep * (Jelly ? 0.6f + Mathf.Abs(Mathf.Sin(Time.time * 3f)) * 1.4f : 1f);
            float x = centerX + Mathf.Sin(phase) * sweep;
            moving.position = new Vector3(x, top + layerHeight * 0.5f + 1.2f, 0f);
            if (Jelly) moving.localScale = new Vector3(width * (1f + Mathf.Sin(Time.time * 18f) * 0.03f), layerHeight, 1f);
            if (PointerInput.AnyTouchBegan() && !PointerInput.IsOverUi()) StartCoroutine(Drop(x));
        }

        IEnumerator Drop(float x)
        {
            busy = true;
            Transform layer = moving;
            moving = null;
            float y0 = layer.position.y, y1 = top + layerHeight * 0.5f;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.15f) { layer.position = new Vector3(x, Mathf.Lerp(y0, y1, k * k), 0f); yield return null; }
            float left = Mathf.Max(x - width * 0.5f, centerX - width * 0.5f), right = Mathf.Min(x + width * 0.5f, centerX + width * 0.5f);
            float overlap = right - left;
            if (overlap <= 0.05f)
            {
                StartCoroutine(Fall(layer, Mathf.Sign(x - centerX)));
                if (bubble != null) bubble.Show("SPLAT. Missed the cake.");
                done = true;
                yield return new WaitForSeconds(1.4f);
                Fail();
                yield break;
            }
            if (Mathf.Abs(x - centerX) < 0.07f)
            {
                layer.position = new Vector3(centerX, y1, 0f);
                Sfx.Play(this, "ding", 0.3f);
                if (bubble != null) { bubble.Show("Perfect!", true); StartCoroutine(HideLater(0.6f)); }
            }
            else
            {
                // pezzo tagliato che cade
                float cutW = width - overlap;
                float cutX = x > centerX ? right + cutW * 0.5f : left - cutW * 0.5f;
                Transform cut = Layer(cutW, layer.GetComponent<SpriteRenderer>().color);
                cut.position = new Vector3(cutX, y1, 0f);
                StartCoroutine(Fall(cut, Mathf.Sign(x - centerX)));
                width = overlap;
                centerX = (left + right) * 0.5f;
                layer.position = new Vector3(centerX, y1, 0f);
                layer.localScale = new Vector3(width, layerHeight, 1f);
                Sfx.Play(this, "tick", 0.8f);
            }
            stack.Add(layer);
            top += layerHeight;
            count++;
            Count();
            if (count >= needed)
            {
                done = true;
                cherry.gameObject.SetActive(true);
                Vector3 to = new Vector3(centerX, top + 0.15f, 0f);
                for (float k = 0f; k < 1f; k += Time.deltaTime / 0.4f) { cherry.position = to + Vector3.up * (1f - k * k) * 3f; yield return null; }
                cherry.position = to;
                if (bubble != null) bubble.Show("A masterpiece! (a slightly thin one)");
                yield return new WaitForSeconds(1.5f);
                Win();
                yield break;
            }
            Next();
            busy = false;
        }

        IEnumerator Fall(Transform t, float side)
        {
            Vector3 p = t.position;
            for (float k = 0f; k < 1.2f; k += Time.deltaTime)
            {
                t.position = p + new Vector3(side * k * 0.8f, -k * k * 6f, 0f);
                t.localRotation = Quaternion.Euler(0, 0, -side * k * 120f);
                yield return null;
            }
            Destroy(t.gameObject);
        }

        IEnumerator HideLater(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (bubble != null && !done) bubble.Hide();
        }
    }
}
