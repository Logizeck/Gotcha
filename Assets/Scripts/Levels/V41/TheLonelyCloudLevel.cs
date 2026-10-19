using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "The lonely cloud" — una nuvoletta piange sopra il prato, ma le gocce cadono lontano dal fiore assetato.
    /// La nuvola non si sposta (è troppo triste per volare): bisogna allargarla con due dita in orizzontale finché copre
    /// anche il vaso; la pioggia cade su tutta la sua larghezza e il fiore si riprende. In 25 s il fiore appassisce.
    /// </summary>
    public class TheLonelyCloudLevel : LevelController
    {
        [SerializeField] Transform cloud;              // bordo sinistro della nuvola
        [SerializeField] Transform[] puffs;            // sbuffi (ognuno con contorno + riempimento)
        [SerializeField] Transform face;
        [SerializeField] SpriteRenderer faceArt, flowerArt;
        [SerializeField] Sprite happyFace, happyFlower, dropSprite;
        [SerializeField] Transform flower;             // base del vaso
        [SerializeField] Transform timerFill;
        [SerializeField] BoxCollider2D cloudArea;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] SmokeEmitter[] confetti;
        [SerializeField] float minWidth = 1.4f, maxWidth = 5.0f, seconds = 25f, dropsNeeded = 10f, potHalf = 0.42f, groundY = -3.35f;

        class Drop { public Transform t; public float vy; public bool alive; }

        Camera cam;
        readonly List<Drop> drops = new List<Drop>();
        float width, timeLeft, spawn, water, sayCooldown;
        float pinchStartDx, pinchStartW;
        bool pinching, busy, dragging;
        Vector3 cloudHome;
        Vector2 dragStart;
        float[] puffSeed;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            width = minWidth;
            timeLeft = seconds;
            cloudHome = cloud.localPosition;
            puffSeed = new float[puffs.Length];
            for (int i = 0; i < puffs.Length; i++) puffSeed[i] = Random.value * 6f;
            Layout();
            S41DKit.Confetti(confetti, false);
            if (bubble != null) bubble.Show("*sob* Nobody needs me... my rain never helps.");
            sayCooldown = 3f;
        }

        void Say(string t) { if (bubble != null && sayCooldown <= 0f) { bubble.Show(t); sayCooldown = 2.5f; } }

        /// <summary>Distribuisce gli sbuffi sulla larghezza attuale: due file, quella alta più rada.</summary>
        void Layout()
        {
            int n = puffs.Length, low = (n * 2) / 3, high = n - low;
            for (int i = 0; i < n; i++)
            {
                bool top = i >= low;
                int j = top ? i - low : i;
                int m = top ? high : low;
                float x = width * (j + 0.5f) / m;
                if (top) x = 0.35f + (width - 0.7f) * (m > 1 ? j / (float)(m - 1) : 0.5f);
                float y = top ? 0.32f : -0.05f + Mathf.Sin(j * 2.1f) * 0.06f;
                puffs[i].localPosition = new Vector3(x, y, 0f);
                puffs[i].localScale = Vector3.one * (top ? 0.95f : 0.8f) * (1f + Mathf.Sin(Time.time * 1.5f + puffSeed[i]) * 0.03f);
            }
            face.localPosition = new Vector3(width * 0.5f, 0.05f, 0f);
            if (cloudArea != null) { cloudArea.size = new Vector2(width + 0.4f, 1.6f); cloudArea.offset = new Vector2(width * 0.5f, 0.1f); }
        }

        void Update()
        {
            if (IsFinished) return;
            float dt = Time.deltaTime;
            sayCooldown -= dt;
            if (!busy)
            {
                HandleInput();
                timeLeft -= dt;
                if (timerFill != null) timerFill.localScale = new Vector3(Mathf.Max(0f, timeLeft / seconds), 1f, 1f);
                float wilt = 1f - water / dropsNeeded;
                if (flower != null) flower.localRotation = Quaternion.Euler(0, 0, -Mathf.Lerp(0f, 10f, 1f - timeLeft / seconds) * wilt);
                if (flowerArt != null) flowerArt.color = Color.Lerp(Color.white, new Color(0.75f, 0.65f, 0.5f), (1f - timeLeft / seconds) * wilt);
                if (timeLeft <= 0f) { StartCoroutine(Wilted()); return; }
            }
            Layout();
            Rain(dt);
        }

        void HandleInput()
        {
            if (Input.touchCount >= 2)
            {
                float dx = Mathf.Abs(PointerInput.ToWorld(cam, Input.GetTouch(0).position).x - PointerInput.ToWorld(cam, Input.GetTouch(1).position).x);
                dx = Mathf.Max(dx, 0.3f);
                if (!pinching) { pinching = true; pinchStartDx = dx; pinchStartW = width; }
                width = Mathf.Clamp(pinchStartW * dx / pinchStartDx, minWidth, maxWidth);
                dragging = false;
                cloud.localPosition = cloudHome;
                return;
            }
            pinching = false;
#if UNITY_EDITOR
            // Editor: rotella del mouse o [ ] per allargare
            width = Mathf.Clamp(width + Input.mouseScrollDelta.y * 0.25f + (Input.GetKey(KeyCode.RightBracket) ? 2f : 0f) * Time.deltaTime - (Input.GetKey(KeyCode.LeftBracket) ? 2f : 0f) * Time.deltaTime, minWidth, maxWidth);
#endif
            // un dito solo: la nuvola si lascia tirare un po' ma torna al suo posto
            if (PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out bool ended))
            {
                Vector2 w = PointerInput.ToWorld(cam, sp);
                if (began && cloudArea != null && cloudArea.OverlapPoint(w)) { dragging = true; dragStart = w; }
                if (dragging)
                {
                    Vector2 off = Vector2.ClampMagnitude(w - dragStart, 0.45f);
                    cloud.localPosition = cloudHome + (Vector3)off;
                    if (off.magnitude > 0.3f) Say("I can't move... I'm too sad to float. *sob*");
                }
                if (ended) dragging = false;
            }
            else dragging = false;
            if (!dragging) cloud.localPosition = Vector3.Lerp(cloud.localPosition, cloudHome, Time.deltaTime * 8f);
        }

        void Rain(float dt)
        {
            if (!busy || water >= dropsNeeded)
            {
                spawn += dt * (busy ? 0f : 7f + width * 1.5f);
                while (spawn >= 1f && !busy)
                {
                    spawn -= 1f;
                    Drop d = drops.Find(x => !x.alive);
                    if (d == null)
                    {
                        var go = new GameObject("Drop");
                        go.transform.SetParent(transform, false);
                        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
                        sr.sprite = dropSprite;
                        sr.sortingOrder = 3;
                        d = new Drop { t = go.transform };
                        drops.Add(d);
                    }
                    d.alive = true;
                    d.vy = -3.5f;
                    d.t.gameObject.SetActive(true);
                    d.t.position = cloud.position + new Vector3(Random.Range(0.15f, width - 0.15f), -0.35f, 0f);
                }
            }
            float potTop = flower.position.y + 0.68f;
            foreach (Drop d in drops)
            {
                if (!d.alive) continue;
                d.vy -= 6f * dt;
                d.t.position += new Vector3(0f, d.vy * dt, 0f);
                Vector3 p = d.t.position;
                bool overPot = Mathf.Abs(p.x - flower.position.x) < potHalf;
                if (overPot && p.y < potTop)
                {
                    d.alive = false;
                    d.t.gameObject.SetActive(false);
                    if (!busy) Watered();
                }
                else if (p.y < groundY) { d.alive = false; d.t.gameObject.SetActive(false); }
            }
        }

        void Watered()
        {
            water += 1f;
            Sfx.Play(this, "tick", 0.5f);
            if (water == 1f) { sayCooldown = 0f; Say("My rain... is reaching her?!"); }
            if (water >= dropsNeeded) StartCoroutine(Bloom());
        }

        IEnumerator Bloom()
        {
            busy = true;
            if (flowerArt != null) { flowerArt.sprite = happyFlower; flowerArt.color = Color.white; }
            flower.localRotation = Quaternion.identity;
            if (faceArt != null) faceArt.sprite = happyFace;
            Sfx.Play(this, "ding", 0.8f);
            Haptics.Light();
            yield return StartCoroutine(S41DKit.Pop(flower, 1.25f, 0.4f));
            if (bubble != null) bubble.Show("She's blooming! I'm not lonely anymore!");
            S41DKit.Confetti(confetti, true);
            yield return new WaitForSeconds(2.0f);
            S41DKit.Confetti(confetti, false);
            Win();
        }

        IEnumerator Wilted()
        {
            busy = true;
            if (flowerArt != null) flowerArt.color = new Color(0.6f, 0.5f, 0.4f);
            Sfx.Play(this, "bonk", 0.7f);
            Haptics.Medium();
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.8f) { flower.localRotation = Quaternion.Euler(0, 0, -10f - 25f * k); yield return null; }
            if (bubble != null) bubble.Show("Too late... *SOOOB*");
            yield return new WaitForSeconds(1.6f);
            Fail();
        }
    }
}
