using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "Treasure map" — quattro fasi. 1) La mappa è strappata in 4 pezzi sparsi e girati: trascinali al loro posto e
    /// toccali per girarli (si incastrano solo dritti). 2) La mappa dà le istruzioni ("3 N, 2 E, ...") e il gatto pirata va
    /// guidato sull'isola con gli swipe, un passo per volta. Il trucco: la rosa dei venti dell'isola è girata, il Nord non è
    /// in alto. Un passo sbagliato rimanda il gatto alla barca. 3) Sulla X strofina la sabbia velocemente per scavare.
    /// 4) Il baule ha un lucchetto a 3 simboli: sono quelli disegnati negli angoli della mappa, nell'ordine dei numeri
    /// (la mappa si riapre toccando la mappina in alto a sinistra). 3 tentativi per il lucchetto. Percorso e simboli
    /// cambiano a ogni partita.
    /// </summary>
    public class TreasureMapLevel : LevelController
    {
        static readonly string[] DirName = { "E", "N", "W", "S" };   // 0 E, 1 N, 2 O, 3 S (antiorario)
        static readonly Vector2Int[] DirStep = { new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(-1, 0), new Vector2Int(0, -1) };

        [SerializeField] StageHud40B hud;
        [SerializeField] SpeechBubble bubble;
        [Header("Fase 1: la mappa")]
        [SerializeField] Transform[] pieces;          // pezzi (con Collider2D)
        [SerializeField] Transform[] targets;         // posto di ogni pezzo (figli di mapHolder)
        [SerializeField] Transform mapHolder;
        [SerializeField] SpriteRenderer outline;
        [SerializeField] SpriteRenderer[] cornerSymbols;   // simbolo nell'angolo esterno di ogni pezzo
        [SerializeField] TextMesh[] cornerNumbers;
        [SerializeField] Sprite[] symbolSprites;      // teschio, pappagallo, ancora, palma
        [SerializeField] SpriteRenderer[] cornerCompasses; // rosa dei venti nello stesso angolo (una sola accesa)
        [SerializeField] TextMesh mapText;
        [SerializeField] Transform mapIcon;           // dove si rimpicciolisce la mappa
        [SerializeField] Collider2D mapButton;
        [SerializeField] SpriteRenderer mapDim;
        [Header("Fase 2: l'isola")]
        [SerializeField] Transform beachGroup, islandGroup, chestGroup;
        [SerializeField] Transform cat;
        [SerializeField] Transform islandCompass;
        [SerializeField] TextMesh directions;
        [SerializeField] Transform gridOrigin;        // centro della casella (0,0) in basso a sinistra
        [SerializeField] float cell = 0.85f;
        [SerializeField] int cols = 5, rows = 6;
        [SerializeField] Vector2Int start = new Vector2Int(2, 0);
        [Header("Fase 3: lo scavo")]
        [SerializeField] Transform xMark;
        [SerializeField] Transform hole;
        [SerializeField] Transform[] sand;            // granelli che volano via
        [SerializeField] Transform digBar;
        [SerializeField] Transform smallChest;
        [SerializeField] float digNeeded = 16f, rubSpeed = 3f;
        [Header("Fase 4: il lucchetto")]
        [SerializeField] SpriteRenderer chestArt;
        [SerializeField] Sprite chestOpenSprite;
        [SerializeField] Collider2D[] dials;
        [SerializeField] SpriteRenderer[] dialSymbols;
        [SerializeField] Collider2D openButton;
        [SerializeField] Transform[] coins;

        int stage;
        bool busy, viewing;
        Camera cam;
        float bubbleUntil, clock;
        // fase 1
        int[] rot;
        bool[] placed;
        Renderer[][] pieceRenderers;
        int[][] baseOrders;
        int grabbed = -1;
        bool dragging;
        Vector2 grabOffset, grabStart;
        int[] answer = new int[3];
        // fase 2
        int compassTurn;
        readonly List<Vector2Int> legs = new List<Vector2Int>();   // x = direzione sullo schermo, y = passi
        int leg, legStep;
        Vector2Int pos;
        bool toldCompass;
        Vector2 swipeStart;
        bool swiping;
        // fase 3
        float dug;
        Vector2 lastRub;
        bool rubbing, toldFaster;
        int sandNext;
        // fase 4
        int[] dialValue = new int[3];

        public override void OnLevelStart()
        {
            cam = Camera.main;
            int n = pieces.Length;
            rot = new int[n];
            placed = new bool[n];
            pieceRenderers = new Renderer[n][];
            baseOrders = new int[n][];
            for (int i = 0; i < n; i++)
            {
                rot[i] = Random.Range(1, 4);
                pieces[i].localRotation = Quaternion.Euler(0, 0, rot[i] * 90f);
                pieceRenderers[i] = pieces[i].GetComponentsInChildren<Renderer>(true);
                baseOrders[i] = new int[pieceRenderers[i].Length];
                for (int k = 0; k < baseOrders[i].Length; k++) baseOrders[i][k] = pieceRenderers[i][k].sortingOrder;
            }
            // angoli: uno ha la rosa dei venti, gli altri tre i simboli numerati (a caso)
            int compassCorner = Random.Range(0, 4);
            var syms = new List<int> { 0, 1, 2 };
            var nums = new List<int> { 1, 2, 3 };
            for (int q = 0; q < 4; q++)
            {
                cornerCompasses[q].enabled = q == compassCorner;
                cornerSymbols[q].enabled = q != compassCorner;
                if (q == compassCorner)
                {
                    cornerNumbers[q].text = "";
                    continue;
                }
                int s = syms[Random.Range(0, syms.Count)]; syms.Remove(s);
                int num = nums[Random.Range(0, nums.Count)]; nums.Remove(num);
                cornerSymbols[q].sprite = symbolSprites[s];
                cornerNumbers[q].text = num.ToString();
                answer[num - 1] = s;
            }
            mapText.gameObject.SetActive(false);
            mapButton.enabled = false;
            mapDim.enabled = false;
            islandGroup.gameObject.SetActive(false);
            chestGroup.gameObject.SetActive(false);
            xMark.gameObject.SetActive(false);
            hole.gameObject.SetActive(false);
            smallChest.gameObject.SetActive(false);
            foreach (Transform t in sand) t.gameObject.SetActive(false);
            foreach (Transform t in coins) t.gameObject.SetActive(false);
            for (int i = 0; i < 3; i++) { dialValue[i] = Random.Range(0, symbolSprites.Length); dialSymbols[i].sprite = symbolSprites[dialValue[i]]; }
            hud.Begin(3);
            hud.Goal("Fix the torn map: drag the pieces, tap one to turn it.");
            Say("Arr! Me treasure map got torn to bits!", 2.6f);
        }

        void Say(string text, float seconds = 2.2f)
        {
            if (bubble == null) return;
            bubble.Show(text);
            bubbleUntil = clock + seconds;
        }

        void Update()
        {
            if (IsFinished) return;
            clock += Time.deltaTime;
            if (bubbleUntil > 0f && clock > bubbleUntil) { bubble.Hide(); bubbleUntil = 0f; }
            if (busy) return;
            if (viewing) { if (PointerInput.AnyTouchBegan()) StartCoroutine(CloseMap()); return; }
            if (stage >= 1 && Tapped(mapButton)) { StartCoroutine(OpenMap()); return; }
            switch (stage)
            {
                case 0: UpdatePieces(); break;
                case 1: UpdateWalk(); break;
                case 2: UpdateDig(); break;
                case 3: UpdateLock(); break;
            }
        }

        bool Tapped(Collider2D c)
        {
            if (c == null || !c.enabled || !PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out _) || !began || PointerInput.IsOverUi()) return false;
            return c.OverlapPoint(PointerInput.ToWorld(cam, sp));
        }

        // ---------- fase 1: ricomporre la mappa ----------

        void UpdatePieces()
        {
            if (!PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out bool ended)) { if (grabbed >= 0) Release(); return; }
            Vector2 w = PointerInput.ToWorld(cam, sp);
            if (began)
            {
                if (PointerInput.IsOverUi()) return;
                grabbed = Pick(w);
                if (grabbed < 0) return;
                grabOffset = (Vector2)pieces[grabbed].position - w;
                grabStart = w;
                dragging = false;
                Raise(grabbed, 20);
                return;
            }
            if (grabbed < 0) return;
            if (!dragging && (w - grabStart).magnitude > 0.15f) dragging = true;
            if (dragging)
            {
                Vector2 p = w + grabOffset;
                p.x = Mathf.Clamp(p.x, -2.4f, 2.4f);
                p.y = Mathf.Clamp(p.y, -4.0f, 2.8f);
                pieces[grabbed].position = new Vector3(p.x, p.y, pieces[grabbed].position.z);
            }
            if (ended) Release();
        }

        void Raise(int i, int by)
        {
            for (int k = 0; k < pieceRenderers[i].Length; k++) pieceRenderers[i][k].sortingOrder = baseOrders[i][k] + by;
        }

        int Pick(Vector2 w)
        {
            int best = -1;
            float bestD = float.MaxValue;
            for (int i = 0; i < pieces.Length; i++)
            {
                if (placed[i] || !pieces[i].GetComponent<Collider2D>().OverlapPoint(w)) continue;
                float d = Vector2.Distance(w, pieces[i].position);
                if (d < bestD) { bestD = d; best = i; }
            }
            return best;
        }

        void Release()
        {
            int i = grabbed;
            grabbed = -1;
            Raise(i, 0);
            if (!dragging)
            {
                rot[i] = (rot[i] + 3) % 4;
                Sfx.Play(this, "tick", 0.7f);
                StartCoroutine(Turn(i));
                return;
            }
            float d = Vector2.Distance(pieces[i].position, targets[i].position);
            if (d < 0.5f && rot[i] == 0) { StartCoroutine(Snap(i)); return; }
            if (d < 0.9f && rot[i] != 0) { Sfx.Play(this, "buzz", 0.4f); Say("Wrong way round! Tap it to turn it.", 1.6f); }
        }

        IEnumerator Turn(int i)
        {
            Quaternion from = pieces[i].localRotation, to = Quaternion.Euler(0, 0, rot[i] * 90f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.12f) { pieces[i].localRotation = Quaternion.Slerp(from, to, k); yield return null; }
            pieces[i].localRotation = to;
        }

        IEnumerator Snap(int i)
        {
            placed[i] = true;
            pieces[i].GetComponent<Collider2D>().enabled = false;
            Vector3 from = pieces[i].position, s = pieces[i].localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.12f)
            {
                pieces[i].position = Vector3.Lerp(from, targets[i].position, k);
                pieces[i].localScale = Vector3.Lerp(s, Vector3.one, k);
                yield return null;
            }
            pieces[i].SetParent(mapHolder, true);
            pieces[i].position = targets[i].position;
            pieces[i].localScale = Vector3.one;
            pieces[i].localRotation = Quaternion.identity;
            Sfx.Play(this, "clunk", 0.6f);
            Haptics.Light();
            foreach (bool b in placed) if (!b) yield break;
            StartCoroutine(MapDone());
        }

        IEnumerator MapDone()
        {
            busy = true;
            outline.enabled = false;
            MakeRoute();
            mapText.text = RouteText(false);
            mapText.gameObject.SetActive(true);
            Sfx.Play(this, "ding", 0.7f);
            Say("The map! Look at the corners... and the directions!", 3f);
            yield return new WaitForSeconds(3.2f);
            yield return hud.Next(1, "STAGE 2: FOLLOW THE MAP", "Swipe to walk the cat, one step per swipe. Mind the compass!");
            // la mappa si arrotola nell'angolo
            Vector3 p0 = mapHolder.position, s0 = mapHolder.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.5f)
            {
                float e = k * k * (3f - 2f * k);
                mapHolder.position = Vector3.Lerp(p0, mapIcon.position, e);
                mapHolder.localScale = Vector3.Lerp(s0, mapIcon.localScale, e);
                yield return null;
            }
            mapButton.enabled = true;
            beachGroup.gameObject.SetActive(false);
            islandGroup.gameObject.SetActive(true);
            pos = start;
            PlaceCat();
            directions.text = RouteText(true);
            stage = 1;
            busy = false;
        }

        // ---------- mappa ingrandita (fasi 2-4) ----------

        IEnumerator OpenMap()
        {
            busy = true;
            Sfx.Play(this, "whoosh", 0.4f);
            mapDim.enabled = true;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.3f)
            {
                mapHolder.position = Vector3.Lerp(mapIcon.position, Vector3.up * 0.2f, k);
                mapHolder.localScale = Vector3.Lerp(mapIcon.localScale, Vector3.one, k);
                yield return null;
            }
            mapHolder.position = Vector3.up * 0.2f;
            mapHolder.localScale = Vector3.one;
            viewing = true;
            busy = false;
        }

        IEnumerator CloseMap()
        {
            busy = true;
            viewing = false;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.25f)
            {
                mapHolder.position = Vector3.Lerp(Vector3.up * 0.2f, mapIcon.position, k);
                mapHolder.localScale = Vector3.Lerp(Vector3.one, mapIcon.localScale, k);
                yield return null;
            }
            mapHolder.position = mapIcon.position;
            mapHolder.localScale = mapIcon.localScale;
            mapDim.enabled = false;
            yield return null;   // il tocco che ha chiuso non vale come swipe
            busy = false;
        }

        // ---------- fase 2: la camminata ----------

        void MakeRoute()
        {
            compassTurn = Random.Range(1, 4);
            islandCompass.localRotation = Quaternion.Euler(0, 0, compassTurn * 90f);
            for (int attempt = 0; attempt < 300; attempt++)
            {
                legs.Clear();
                Vector2Int p = start;
                int prev = -1;
                bool ok = true;
                for (int l = 0; l < 4 && ok; l++)
                {
                    var choices = new List<int>();
                    for (int d = 0; d < 4; d++)
                    {
                        if (prev >= 0 && (d == prev || d == (prev + 2) % 4)) continue;
                        for (int len = 1; len <= 3; len++)
                        {
                            Vector2Int q = p + DirStep[d] * len;
                            if (q.x >= 0 && q.x < cols && q.y >= 0 && q.y < rows) choices.Add(d * 10 + len);
                        }
                    }
                    if (choices.Count == 0) { ok = false; break; }
                    int c = choices[Random.Range(0, choices.Count)];
                    legs.Add(new Vector2Int(c / 10, c % 10));
                    p += DirStep[c / 10] * (c % 10);
                    prev = c / 10;
                }
                if (ok && p != start && Mathf.Abs(p.x - start.x) + Mathf.Abs(p.y - start.y) >= 2) return;
            }
            legs.Clear();
            legs.Add(new Vector2Int(1, 3)); legs.Add(new Vector2Int(0, 2)); legs.Add(new Vector2Int(3, 1)); legs.Add(new Vector2Int(2, 1));
        }

        string RouteText(bool progress)
        {
            var parts = new List<string>();
            for (int l = 0; l < legs.Count; l++)
            {
                string t = legs[l].y + " " + DirName[(legs[l].x - compassTurn + 4) % 4];
                if (progress && l < leg) t = "<color=#A8957A>" + t + "</color>";
                else if (progress && l == leg) t = "<color=#C0392B>" + t + "</color>";
                parts.Add(t);
            }
            return string.Join(",  ", parts);
        }

        Vector3 CellPos(Vector2Int c) => gridOrigin.position + new Vector3(c.x * cell, c.y * cell, 0f);

        void PlaceCat() { cat.position = CellPos(pos); }

        void UpdateWalk()
        {
            if (!PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out bool ended)) { swiping = false; return; }
            Vector2 w = PointerInput.ToWorld(cam, sp);
            if (began) { if (PointerInput.IsOverUi()) return; swipeStart = w; swiping = true; return; }
            if (!swiping) return;
            Vector2 d = w - swipeStart;
            if (d.magnitude < 0.45f && !ended) return;
            swiping = false;
            if (d.magnitude < 0.45f) { Say("Swipe to walk, matey!", 1.4f); return; }
            int dir = Mathf.Abs(d.x) > Mathf.Abs(d.y) ? (d.x > 0f ? 0 : 2) : (d.y > 0f ? 1 : 3);
            Vector2Int next = pos + DirStep[dir];
            if (next.x < 0 || next.x >= cols || next.y < 0 || next.y >= rows) { Say("Splash? No thanks!", 1.4f); Sfx.Play(this, "bonk", 0.5f); return; }
            StartCoroutine(Step(dir, next));
        }

        IEnumerator Step(int dir, Vector2Int next)
        {
            busy = true;
            Vector3 from = cat.position, to = CellPos(next);
            if (dir == 0 || dir == 2) cat.localScale = new Vector3(dir == 2 ? -Mathf.Abs(cat.localScale.x) : Mathf.Abs(cat.localScale.x), cat.localScale.y, 1f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.18f)
            {
                cat.position = Vector3.Lerp(from, to, k) + Vector3.up * Mathf.Sin(k * Mathf.PI) * 0.12f;
                yield return null;
            }
            cat.position = to;
            pos = next;
            Sfx.Play(this, "tick", 0.6f);
            if (dir == legs[leg].x)
            {
                legStep++;
                if (legStep >= legs[leg].y) { leg++; legStep = 0; }
                directions.text = RouteText(true);
                if (leg >= legs.Count) { StartCoroutine(Found()); yield break; }
                busy = false;
                yield break;
            }
            // passo sbagliato: si torna alla barca
            Sfx.Play(this, "buzz", 0.5f);
            if (!toldCompass) { toldCompass = true; Say("Arr! Wrong way! Check the COMPASS, not the screen!", 2.8f); }
            else Say("Arr! That's not what the map says! Back to the boat!", 2.2f);
            yield return new WaitForSeconds(0.7f);
            from = cat.position;
            to = CellPos(start);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.6f) { cat.position = Vector3.Lerp(from, to, k) + Vector3.up * Mathf.Abs(Mathf.Sin(k * Mathf.PI * 3f)) * 0.1f; yield return null; }
            pos = start;
            PlaceCat();
            leg = 0;
            legStep = 0;
            directions.text = RouteText(true);
            busy = false;
        }

        IEnumerator Found()
        {
            busy = true;
            xMark.position = CellPos(pos);
            xMark.gameObject.SetActive(true);
            Sfx.Play(this, "ding", 0.8f);
            Vector3 s = xMark.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.35f) { xMark.localScale = s * (1f + Mathf.Sin(k * Mathf.PI) * 0.5f); yield return null; }
            xMark.localScale = s;
            Say("X marks the spot!", 1.8f);
            // il gatto si sposta di lato
            Vector3 from = cat.position, to = from + new Vector3(pos.x >= cols - 1 ? -0.75f : 0.75f, 0.1f, 0f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.3f) { cat.position = Vector3.Lerp(from, to, k); yield return null; }
            yield return hud.Next(2, "STAGE 3: DIG!", "Rub the sand on the X - FAST!");
            hole.position = xMark.position;
            hole.localScale = Vector3.one * 0.15f;
            hole.gameObject.SetActive(true);
            digBar.localScale = new Vector3(0f, 1f, 1f);
            digBar.parent.gameObject.SetActive(true);
            stage = 2;
            busy = false;
        }

        // ---------- fase 3: scavare ----------

        void UpdateDig()
        {
            float dt = Time.deltaTime;
            dug = Mathf.Max(0f, dug - dt * 0.6f);
            if (PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out bool ended) && !ended)
            {
                Vector2 w = PointerInput.ToWorld(cam, sp);
                bool near = Vector2.Distance(w, xMark.position) < 1.2f;
                if (began || !rubbing) { rubbing = near; lastRub = w; }
                else if (near && dt > 0f)
                {
                    float dist = Vector2.Distance(w, lastRub);
                    float speed = dist / dt;
                    if (speed > rubSpeed)
                    {
                        dug += dist;
                        if (Random.value < 0.35f) Fling();
                    }
                    else if (speed > 0.3f && !toldFaster && dug < 2f) { toldFaster = true; Say("Faster! Dig like a pirate!", 1.6f); }
                    lastRub = w;
                }
            }
            else rubbing = false;
            float f = Mathf.Clamp01(dug / digNeeded);
            hole.localScale = Vector3.one * Mathf.Lerp(0.15f, 1f, f);
            digBar.localScale = new Vector3(f, 1f, 1f);
            if (dug >= digNeeded) StartCoroutine(Dug());
        }

        void Fling()
        {
            Transform g = sand[sandNext];
            sandNext = (sandNext + 1) % sand.Length;
            StartCoroutine(Grain(g));
            if (Random.value < 0.3f) Sfx.Play(this, "tick", 0.3f);
        }

        IEnumerator Grain(Transform g)
        {
            g.gameObject.SetActive(true);
            Vector3 from = xMark.position, dir = new Vector3(Random.Range(-1f, 1f), Random.Range(0.3f, 1f), 0f).normalized;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.4f)
            {
                g.position = from + dir * k * 1.1f + Vector3.up * Mathf.Sin(k * Mathf.PI) * 0.3f;
                g.localScale = Vector3.one * (0.5f * (1f - k) + 0.1f);
                yield return null;
            }
            g.gameObject.SetActive(false);
        }

        IEnumerator Dug()
        {
            busy = true;
            digBar.parent.gameObject.SetActive(false);
            Sfx.Play(this, "clunk", 1f);
            Haptics.Medium();
            Say("CLUNK! Something hard!", 1.6f);
            smallChest.position = hole.position;
            smallChest.gameObject.SetActive(true);
            Vector3 s = smallChest.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.5f)
            {
                smallChest.localScale = s * Mathf.Lerp(0.2f, 1f, k);
                smallChest.position = hole.position + Vector3.up * Mathf.Sin(k * Mathf.PI) * 0.4f;
                yield return null;
            }
            smallChest.localScale = s;
            yield return new WaitForSeconds(0.6f);
            yield return hud.Next(3, "STAGE 4: THE LOCK", "Tap the dials to set the 3 symbols, then OPEN.");
            islandGroup.gameObject.SetActive(false);
            chestGroup.gameObject.SetActive(true);
            Say("Three symbols... where have I seen them? (tap the little map)", 3f);
            stage = 3;
            busy = false;
        }

        // ---------- fase 4: il lucchetto ----------

        void UpdateLock()
        {
            if (!PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out _) || !began || PointerInput.IsOverUi()) return;
            Vector2 w = PointerInput.ToWorld(cam, sp);
            for (int i = 0; i < dials.Length; i++)
            {
                if (!dials[i].OverlapPoint(w)) continue;
                dialValue[i] = (dialValue[i] + 1) % symbolSprites.Length;
                dialSymbols[i].sprite = symbolSprites[dialValue[i]];
                Sfx.Play(this, "tick", 0.8f);
                StartCoroutine(Spin(dials[i].transform));
                return;
            }
            if (openButton.OverlapPoint(w)) StartCoroutine(TryOpen());
        }

        IEnumerator Spin(Transform d)
        {
            Vector3 s = d.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.12f) { d.localScale = new Vector3(s.x, s.y * (1f - Mathf.Sin(k * Mathf.PI) * 0.3f), s.z); yield return null; }
            d.localScale = s;
        }

        IEnumerator TryOpen()
        {
            busy = true;
            bool ok = true;
            for (int i = 0; i < 3; i++) if (dialValue[i] != answer[i]) ok = false;
            if (ok) { StartCoroutine(Open()); yield break; }
            Sfx.Play(this, "bonk", 0.9f);
            Sfx.Play(this, "buzz", 0.5f);
            Haptics.Medium();
            Transform c = chestArt.transform;
            Vector3 p = c.localPosition;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.4f) { c.localPosition = p + new Vector3(Mathf.Sin(k * 50f) * 0.08f, Mathf.Abs(Mathf.Sin(k * 25f)) * 0.1f, 0f); yield return null; }
            c.localPosition = p;
            if (hud.LoseTry())
            {
                hud.Shout("THE CHEST WINS");
                Say("CHOMP! The chest sank back into the sand. Arr...", 2.6f);
                yield return new WaitForSeconds(2.4f);
                Fail();
                yield break;
            }
            Say(hud.TriesLeft == 2 ? "CHOMP! The chest bites back. Wrong symbols!" : "CHOMP! Last try, matey. Check the map corners!", 2.4f);
            busy = false;
        }

        IEnumerator Open()
        {
            chestArt.sprite = chestOpenSprite;
            Sfx.Play(this, "ding", 1f);
            Haptics.Medium();
            hud.SetStage(4);
            hud.Shout("TREASURE!");
            Vector3 c = chestArt.transform.position + new Vector3(0f, 0.8f, 0f);
            for (int i = 0; i < coins.Length; i++) { coins[i].gameObject.SetActive(true); StartCoroutine(Coin(coins[i], c, i)); }
            Say("Gold! Shiny! Mine! Er... OURS!", 2.6f);
            yield return new WaitForSeconds(2.2f);
            Win();
        }

        IEnumerator Coin(Transform t, Vector3 from, int i)
        {
            float vx = (i - (coins.Length - 1) * 0.5f) * 0.9f, vy = Random.Range(4f, 6f);
            Vector3 p = from;
            for (float k = 0f; k < 1.6f; k += Time.deltaTime)
            {
                vy -= 9f * Time.deltaTime;
                p += new Vector3(vx, vy, 0f) * Time.deltaTime;
                t.position = p;
                t.localRotation = Quaternion.Euler(0, 0, k * 400f * (i % 2 == 0 ? 1 : -1));
                if (i == 0 && Mathf.Repeat(k, 0.2f) < Time.deltaTime) Sfx.Play(this, "pop", 0.4f);
                yield return null;
            }
        }
    }
}
