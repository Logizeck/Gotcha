using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 79 — trova le differenze. Due disegni uguali... quasi: 5 differenze (un oggetto sparito, un colore
    /// diverso, uno capovolto, uno più grande, uno ruotato), scelte a caso ogni volta. Tocca le differenze su uno dei
    /// due disegni. Tre tocchi sbagliati o tempo scaduto = perso.
    /// </summary>
    public class SpotLevel : LevelController
    {
        [SerializeField] Transform[] top, bottom;     // stessi oggetti nelle due figure
        [SerializeField] SpriteRenderer markerTemplate;
        [SerializeField] TextMesh counter;
        [SerializeField] Transform timeBar;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] int differences = 5, maxWrong = 3;
        [SerializeField] float seconds = 60f, tapRadius = 0.5f;

        readonly List<int> diff = new List<int>();
        readonly HashSet<int> found = new HashSet<int>();
        int wrong;
        float clock;
        bool done;
        Camera cam;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            var idx = new List<int>();
            for (int i = 0; i < top.Length; i++) idx.Add(i);
            for (int k = 0; k < differences; k++)
            {
                int j = Random.Range(0, idx.Count);
                diff.Add(idx[j]);
                idx.RemoveAt(j);
            }
            for (int k = 0; k < diff.Count; k++)
            {
                // a volte la differenza è nel disegno di sopra
                Transform t = Random.value < 0.5f ? bottom[diff[k]] : top[diff[k]];
                switch (k % 5)
                {
                    case 0: t.gameObject.SetActive(false); break;
                    case 1: foreach (SpriteRenderer sr in t.GetComponentsInChildren<SpriteRenderer>()) sr.color = new Color(0.45f, 0.75f, 1f); break;
                    case 2: t.localScale = new Vector3(-t.localScale.x, t.localScale.y, 1f); break;
                    case 3: t.localScale *= 1.35f; break;
                    default: t.localRotation = Quaternion.Euler(0, 0, 35f); break;
                }
            }
            Count();
            if (bubble != null) { bubble.Show("Five differences. Look closely!"); StartCoroutine(HideLater(2f)); }
        }

        void Count() { counter.text = found.Count + " / " + differences + "      X " + wrong + "/" + maxWrong; }

        void Update()
        {
            if (done || IsFinished) return;
            clock += Time.deltaTime;
            if (timeBar != null) timeBar.localScale = new Vector3(Mathf.Clamp01(1f - clock / seconds), 1f, 1f);
            if (clock >= seconds) { StartCoroutine(Lost("Time's up!")); return; }
            if (!PointerInput.AnyTouchBegan() || !PointerInput.TryGetPrimary(out Vector2 sp, out _, out _)) return;
            Vector2 w = PointerInput.ToWorld(cam, sp);
            foreach (int i in diff)
            {
                if (found.Contains(i)) continue;
                if (Vector2.Distance(w, top[i].parent.TransformPoint(top[i].localPosition)) > tapRadius &&
                    Vector2.Distance(w, bottom[i].parent.TransformPoint(bottom[i].localPosition)) > tapRadius) continue;
                found.Add(i);
                Mark(top[i]);
                Mark(bottom[i]);
                Sfx.Play(this, "ding", 0.4f);
                Count();
                if (found.Count >= differences) StartCoroutine(AllFound());
                return;
            }
            // già trovata? niente penalità
            foreach (int i in found)
                if (Vector2.Distance(w, top[i].position) < tapRadius || Vector2.Distance(w, bottom[i].position) < tapRadius) return;
            wrong++;
            Sfx.Play(this, "buzz", 0.6f);
            Count();
            if (wrong >= maxWrong) StartCoroutine(Lost("Three wrong taps!"));
        }

        void Mark(Transform t)
        {
            SpriteRenderer m = Instantiate(markerTemplate, transform);
            m.gameObject.SetActive(true);
            m.transform.position = t.parent.TransformPoint(t.localPosition);
        }

        IEnumerator AllFound()
        {
            done = true;
            if (bubble != null) bubble.Show("Eagle eyes!");
            yield return new WaitForSeconds(1.4f);
            Win();
        }

        IEnumerator Lost(string msg)
        {
            done = true;
            // mostra quelle mancanti
            foreach (int i in diff) if (!found.Contains(i)) { Mark(top[i]); Mark(bottom[i]); }
            if (bubble != null) bubble.Show(msg);
            yield return new WaitForSeconds(1.8f);
            Fail();
        }

        IEnumerator HideLater(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (bubble != null && !done) bubble.Hide();
        }
    }
}
