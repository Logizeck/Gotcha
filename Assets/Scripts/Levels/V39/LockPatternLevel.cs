using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Lock Screen — il telefono della mamma ha la sequenza di sblocco 3x3. Sul vetro restano le ditate unte lungo
    /// la sequenza segreta (il punto di partenza ha l'impronta più grande): basta ripassarle col dito. Vale in
    /// entrambi i versi. Tre tentativi: dopo il terzo errore il telefono si blocca per 30 secondi.
    /// </summary>
    public class LockPatternLevel : LevelController
    {
        [SerializeField] Transform phone;
        [SerializeField] Transform[] dots;            // 9, riga per riga dall'alto a sinistra
        [SerializeField] SpriteRenderer[] rings;      // 9, cerchi di selezione
        [SerializeField] LineRenderer trace;
        [SerializeField] Transform smudgeTemplate;
        [SerializeField] Transform fingerprint;
        [SerializeField] GameObject lockScreen;
        [SerializeField] GameObject padlockClosed;
        [SerializeField] GameObject padlockOpen;
        [SerializeField] Transform pizzaApp;
        [SerializeField] Transform orderButton;
        [SerializeField] TextMesh status;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] float pickRadius = 0.42f;

        static readonly int[][] Patterns =
        {
            new[] { 0, 1, 2, 4, 6, 7 },
            new[] { 6, 3, 0, 4, 8, 5 },
            new[] { 2, 5, 4, 3, 6, 7 },
            new[] { 0, 3, 6, 4, 2, 5 },
            new[] { 1, 3, 4, 5, 7 },
            new[] { 8, 7, 6, 4, 2, 1 },
        };

        static readonly Color RingIdle = new Color(1f, 1f, 1f, 0.9f), RingBad = new Color(1f, 0.35f, 0.35f, 1f), RingGood = new Color(0.4f, 1f, 0.5f, 1f);

        int[] secret;
        readonly List<int> picked = new List<int>();
        int attempts = 3;
        bool drawing, busy;
        Camera cam;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            secret = Patterns[Random.Range(0, Patterns.Length)];
            smudgeTemplate.gameObject.SetActive(false);
            pizzaApp.gameObject.SetActive(false);
            padlockOpen.SetActive(false);
            trace.positionCount = 0;
            ClearRings();
            // ditate lungo la sequenza segreta
            for (int i = 0; i + 1 < secret.Length; i++)
            {
                Vector3 a = dots[secret[i]].localPosition, b = dots[secret[i + 1]].localPosition;
                Transform s = Instantiate(smudgeTemplate, smudgeTemplate.parent);
                s.gameObject.SetActive(true);
                Vector3 d = b - a;
                s.localPosition = (a + b) * 0.5f + (Vector3)(Random.insideUnitCircle * 0.03f);
                s.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg + Random.Range(-3f, 3f));
                s.localScale = new Vector3(d.magnitude / 0.84f, Random.Range(0.85f, 1.1f), 1f);
            }
            fingerprint.localPosition = dots[secret[0]].localPosition + new Vector3(0.03f, -0.02f, 0f);
            fingerprint.localRotation = Quaternion.Euler(0, 0, Random.Range(-25f, 25f));
            Status("Draw the pattern to unlock");
            if (bubble != null) { bubble.Show("Hmm... the screen is a bit greasy."); StartCoroutine(HideLater(2.5f)); }
        }

        void Status(string s) { status.text = s; }

        void ClearRings()
        {
            foreach (SpriteRenderer r in rings) { r.gameObject.SetActive(false); r.color = RingIdle; }
        }

        void Update()
        {
            if (busy || IsFinished) return;
            if (!PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out bool ended)) { if (drawing) Release(); return; }
            Vector3 w = PointerInput.ToWorld(cam, sp);
            if (began && !PointerInput.IsOverUi())
            {
                drawing = true;
                picked.Clear();
                ClearRings();
            }
            if (!drawing) return;
            for (int i = 0; i < dots.Length; i++)
            {
                if (picked.Contains(i)) continue;
                if (Vector2.Distance(w, dots[i].position) > pickRadius) continue;
                // come sui telefoni veri: passando sopra un punto intermedio lo si prende
                if (picked.Count > 0)
                {
                    int last = picked[picked.Count - 1];
                    int r0 = last / 3, c0 = last % 3, r1 = i / 3, c1 = i % 3;
                    if ((r0 + r1) % 2 == 0 && (c0 + c1) % 2 == 0)
                    {
                        int mid = (r0 + r1) / 2 * 3 + (c0 + c1) / 2;
                        if (!picked.Contains(mid)) Pick(mid);
                    }
                }
                Pick(i);
            }
            trace.positionCount = picked.Count + (ended ? 0 : 1);
            for (int i = 0; i < picked.Count; i++) trace.SetPosition(i, dots[picked[i]].position);
            if (!ended && picked.Count > 0) trace.SetPosition(picked.Count, new Vector3(w.x, w.y, 0f));
            if (picked.Count == 0) trace.positionCount = 0;
            if (ended) Release();
        }

        void Pick(int i)
        {
            picked.Add(i);
            rings[i].gameObject.SetActive(true);
            Haptics.Light();
            Sfx.Play(this, "tick", 0.5f);
        }

        void Release()
        {
            drawing = false;
            if (picked.Count < 2) { picked.Clear(); ClearRings(); trace.positionCount = 0; return; }
            trace.positionCount = picked.Count;
            if (Matches()) StartCoroutine(Unlock());
            else StartCoroutine(Wrong());
        }

        bool Matches()
        {
            if (picked.Count != secret.Length) return false;
            bool fwd = true, back = true;
            for (int i = 0; i < secret.Length; i++)
            {
                if (picked[i] != secret[i]) fwd = false;
                if (picked[i] != secret[secret.Length - 1 - i]) back = false;
            }
            return fwd || back;
        }

        IEnumerator Wrong()
        {
            busy = true;
            attempts--;
            Haptics.Medium();
            Sfx.Play(this, "buzz", 0.6f);
            foreach (SpriteRenderer r in rings) r.color = RingBad;
            trace.startColor = trace.endColor = RingBad;
            Status(attempts > 0 ? "Wrong pattern (" + attempts + " left)" : "Wrong pattern");
            Vector3 home = phone.localPosition;
            for (float k = 0f; k < 0.45f; k += Time.deltaTime)
            {
                phone.localPosition = home + Vector3.right * Mathf.Sin(k * 70f) * 0.08f * (1f - k / 0.45f);
                yield return null;
            }
            phone.localPosition = home;
            yield return new WaitForSeconds(0.5f);
            trace.positionCount = 0;
            trace.startColor = trace.endColor = Color.white;
            picked.Clear();
            ClearRings();
            if (attempts <= 0)
            {
                Status("Phone locked for 30 seconds");
                Haptics.Medium();
                if (bubble != null) bubble.Show("Uh-oh. Mom will notice.");
                yield return new WaitForSeconds(1.8f);
                Fail();
                yield break;
            }
            busy = false;
        }

        IEnumerator Unlock()
        {
            busy = true;
            foreach (int i in picked) rings[i].color = RingGood;
            trace.startColor = trace.endColor = RingGood;
            padlockClosed.SetActive(false);
            padlockOpen.SetActive(true);
            Sfx.Play(this, "ding", 0.7f);
            Status("Unlocked!");
            yield return new WaitForSeconds(0.8f);
            trace.positionCount = 0;
            ClearRings();
            lockScreen.SetActive(false);
            fingerprint.gameObject.SetActive(false);
            pizzaApp.gameObject.SetActive(true);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.3f) { pizzaApp.localScale = Vector3.one * Mathf.Lerp(0.6f, 1f, k); yield return null; }
            pizzaApp.localScale = Vector3.one;
            yield return new WaitForSeconds(0.6f);
            // il "dito" preme ORDINA
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.25f) { orderButton.localScale = Vector3.one * (1f - Mathf.Sin(k * Mathf.PI) * 0.12f); yield return null; }
            orderButton.localScale = Vector3.one;
            Haptics.Light();
            Sfx.Play(this, "pop", 0.7f);
            if (bubble != null) bubble.Show("Pizza on the way! Shh...");
            yield return new WaitForSeconds(1.5f);
            Win();
        }

        IEnumerator HideLater(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (bubble != null && !busy) bubble.Hide();
        }
    }
}
