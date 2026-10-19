using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "Glass floor" — il topolino deve attraversare un ponte di vetro sopra il vuoto: sei file da due lastre,
    /// in ogni fila una è robusta e l'altra fragile (a caso a ogni partita), ma sembrano identiche. Soluzione: toccare le
    /// lastre per "bussare": quella robusta fa una nota GRAVE e sorda, quella fragile una nota ACUTA da cristallo (con la
    /// vibrazione e la notina che sale, grande e lenta o piccola e veloce). Poi trascinare il topolino una fila alla
    /// volta solo sulle lastre gravi. Sconfitta: due passi su lastre fragili (alla prima si salva per un pelo).
    /// </summary>
    public class GlassFloorLevel : LevelController
    {
        public const int Rows = 6;

        [SerializeField] Transform mouse;
        [SerializeField] SpriteRenderer[] tiles;      // fila * 2 + colonna (fila 0 in basso)
        [SerializeField] Sprite crackedSprite, shardSprite, noteSprite;
        [SerializeField] Transform startSpot, goalSpot, cheese;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] SpriteRenderer[] hearts;
        [SerializeField] float lowHz = 175f, highHz = 1568f;

        Camera cam;
        readonly int[] safeCol = new int[Rows];
        readonly bool[] broken = new bool[Rows * 2];
        int row = -1, strikes;
        bool dragging, busy;
        Vector3 restPos;
        float idle;
        bool nagged;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            for (int i = 0; i < Rows; i++) safeCol[i] = Random.value < 0.5f ? 0 : 1;
            restPos = startSpot.position;
            mouse.position = restPos;
            if (bubble != null) bubble.Hide();
            Kit41B.Hearts(hearts, 2);
        }

        Vector3 TilePos(int r, int c) => tiles[r * 2 + c].transform.position;

        int TileAt(Vector2 w)
        {
            for (int i = 0; i < tiles.Length; i++)
            {
                if (broken[i]) continue;
                Collider2D col = tiles[i].GetComponent<Collider2D>();
                if (col != null && col.OverlapPoint(w)) return i;
            }
            return -1;
        }

        void Update()
        {
            if (IsFinished || busy) return;
            idle += Time.deltaTime;
            if (!nagged && idle > 9f && row < 0) { nagged = true; Kit41B.Say(bubble, "Is it safe? Knock first!"); }
            if (!Kit41B.Pointer(cam, out Vector2 w, out bool began, out bool ended)) { if (dragging) Release(mouse.position); return; }

            if (began)
            {
                idle = 0f;
                if (Vector2.Distance(w, mouse.position + Vector3.up * 0.15f) < 0.6f) { dragging = true; Sfx.Play(this, "tick", 0.4f); return; }
                int t = TileAt(w);
                if (t >= 0) Knock(t);
                return;
            }
            if (!dragging) return;
            mouse.position = new Vector3(w.x, w.y, restPos.z);
            if (ended) Release(w);
        }

        void Knock(int index)
        {
            bool safe = index % 2 == safeCol[index / 2];
            if (safe) { Kit41B.Tone(this, lowHz, 0.6f, "thud", 0.9f); Haptics.Medium(); }
            else { Kit41B.Tone(this, highHz, 0.7f, "bell", 0.55f); Haptics.Light(); }
            StartCoroutine(Kit41B.Shake(tiles[index].transform, safe ? 0.03f : 0.05f, 0.25f));
            StartCoroutine(NoteFx(tiles[index].transform.position, safe));
        }

        IEnumerator NoteFx(Vector3 at, bool low)
        {
            if (noteSprite == null) yield break;
            var go = new GameObject("Note");
            go.transform.SetParent(transform, false);
            go.transform.position = at + new Vector3(Random.Range(-0.2f, 0.2f), 0.1f, 0f);
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = noteSprite;
            sr.sortingOrder = 30;
            float size = low ? 1.5f : 0.75f, time = low ? 1.1f : 0.6f;
            sr.color = low ? new Color(0.35f, 0.45f, 0.95f) : new Color(0.75f, 0.95f, 1f);
            Vector3 p0 = go.transform.position;
            for (float k = 0f; k < 1f; k += Time.deltaTime / time)
            {
                go.transform.position = p0 + new Vector3(Mathf.Sin(k * (low ? 4f : 14f)) * 0.08f, k * 0.7f, 0f);
                go.transform.localScale = Vector3.one * size * (0.6f + k * 0.4f);
                Color c = sr.color; c.a = 1f - k * k; sr.color = c;
                yield return null;
            }
            Destroy(go);
        }

        void Release(Vector2 w)
        {
            dragging = false;
            int next = row + 1;
            if (next >= Rows)
            {
                if (w.y > TilePos(Rows - 1, 0).y + 0.45f) { StartCoroutine(Finish()); return; }
                StartCoroutine(Hop(restPos, null));
                return;
            }
            int t = TileAt(w);
            if (t < 0 || t / 2 != next)
            {
                if (t >= 0 && t / 2 > next) Kit41B.Say(bubble, "One row at a time!");
                StartCoroutine(Hop(restPos, null));
                return;
            }
            StartCoroutine(Step(t));
        }

        IEnumerator Hop(Vector3 to, System.Action done)
        {
            busy = true;
            Vector3 from = mouse.position;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.25f)
            {
                mouse.position = Vector3.Lerp(from, to, k) + Vector3.up * Mathf.Sin(k * Mathf.PI) * 0.3f;
                yield return null;
            }
            mouse.position = to;
            busy = false;
            done?.Invoke();
        }

        IEnumerator Step(int t)
        {
            busy = true;
            Vector3 at = tiles[t].transform.position + new Vector3(0f, 0.12f, restPos.z);
            yield return Hop(at, null);
            busy = true;
            bool safe = t % 2 == safeCol[t / 2];
            if (safe)
            {
                Kit41B.Tone(this, lowHz, 0.4f, "thud", 0.7f);
                row = t / 2;
                restPos = at;
                busy = false;
                yield break;
            }
            // lastra fragile: si crepa sotto le zampe
            tiles[t].sprite = crackedSprite;
            Kit41B.Tone(this, highHz * 1.2f, 0.5f, "bell", 0.7f);
            Haptics.Medium();
            yield return Kit41B.Shake(tiles[t].transform, 0.08f, 0.4f);
            strikes++;
            Kit41B.Hearts(hearts, 2 - strikes);
            Shatter(t);
            if (strikes >= 2)
            {
                Kit41B.Say(bubble, "Eeeeeek!");
                Haptics.Heavy();
                Sfx.Play(this, "whoosh", 0.8f);
                Vector3 p = mouse.position;
                for (float k = 0f; k < 1f; k += Time.deltaTime / 0.9f)
                {
                    mouse.position = p + Vector3.down * k * k * 1.4f;
                    mouse.localScale = Vector3.one * (1f - k * 0.85f);
                    mouse.rotation = Quaternion.Euler(0f, 0f, k * 360f);
                    yield return null;
                }
                mouse.gameObject.SetActive(false);
                yield return new WaitForSeconds(0.4f);
                Fail();
                yield break;
            }
            // salvo per un pelo: torna indietro
            Kit41B.Say(bubble, "EEK! That one sounded... high!");
            Vector3 back = row < 0 ? startSpot.position : restPos;
            yield return Hop(back, null);
        }

        void Shatter(int t)
        {
            broken[t] = true;
            Vector3 c = tiles[t].transform.position;
            tiles[t].gameObject.SetActive(false);
            Sfx.Play(this, "pop", 0.8f);
            for (int i = 0; i < 7; i++) StartCoroutine(Shard(c + new Vector3(Random.Range(-0.45f, 0.45f), Random.Range(-0.2f, 0.2f), 0f)));
        }

        IEnumerator Shard(Vector3 at)
        {
            var go = new GameObject("Shard");
            go.transform.SetParent(transform, false);
            go.transform.position = at;
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = shardSprite;
            sr.sortingOrder = 6;
            Vector3 v = new Vector3(Random.Range(-1f, 1f), Random.Range(0.5f, 1.5f), 0f);
            float spin = Random.Range(-400f, 400f);
            for (float k = 0f; k < 1.2f; k += Time.deltaTime)
            {
                v.y -= 6f * Time.deltaTime;
                go.transform.position += v * Time.deltaTime;
                go.transform.Rotate(0f, 0f, spin * Time.deltaTime);
                go.transform.localScale = Vector3.one * (1f - k * 0.6f);
                yield return null;
            }
            Destroy(go);
        }

        IEnumerator Finish()
        {
            busy = true;
            yield return Hop(goalSpot.position, null);
            busy = true;
            Sfx.Play(this, "ding", 0.7f);
            Kit41B.Say(bubble, "Solid ground! And cheese!");
            if (cheese != null)
            {
                Vector3 s = cheese.localScale;
                for (float k = 0f; k < 1f; k += Time.deltaTime / 1.2f)
                {
                    cheese.localScale = s * (1f - k);
                    if (Mathf.Repeat(k, 0.25f) < Time.deltaTime / 1.2f) Sfx.Play(this, "tick", 0.5f);
                    yield return null;
                }
                cheese.gameObject.SetActive(false);
            }
            yield return Kit41B.Pop(mouse, 0.25f, 0.4f);
            yield return new WaitForSeconds(0.5f);
            Win();
        }
    }
}
