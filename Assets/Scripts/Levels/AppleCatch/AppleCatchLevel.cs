using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Sensors;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 38 — raccogli le mele. Scuoti il telefono: dall'albero cadono una o due mele. Il cesto si sposta
    /// inclinando il telefono: prendine 5. Se ne cadono troppe per terra, si riprova.
    /// (Editor: tasto S per scuotere, frecce per il cesto.)
    /// </summary>
    public class AppleCatchLevel : LevelController
    {
        [SerializeField] Transform[] apples;
        [SerializeField] Transform tree;
        [SerializeField] Transform basket;
        [SerializeField] TextMesh counter;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] int needed = 5;
        [SerializeField] float shakeStrength = 11f;
        [SerializeField] float basketRange = 2.1f;
        [SerializeField] float basketHalfWidth = 0.65f;
        [SerializeField] float basketTopY = -2.55f;
        [SerializeField] float groundY = -3.8f;
        [SerializeField] float gravity = 9f;

        class Falling { public Transform t; public float vy, vx, spin; }
        readonly List<Falling> falling = new List<Falling>();
        readonly List<Transform> onTree = new List<Transform>();
        int caught, lost;
        float cooldown;
        bool done;
        readonly SteadyTilt steady = new SteadyTilt { shakeLevel = 8f, shakeHold = 0.5f };
        Vector3 treeHome;

        public override void OnLevelStart()
        {
            onTree.AddRange(apples);
            if (tree != null) treeHome = tree.localPosition;
            UpdateCounter();
            if (bubble != null) { bubble.Show("Hungry! Any apples?"); StartCoroutine(HideLater(2f)); }
        }

        void Update()
        {
            if (done || IsFinished) return;
            // filtrata: niente tremolio, e scuotere l'albero non sposta il cesto
            float bx = Mathf.Clamp(steady.Update(Time.deltaTime) * 5f, -basketRange, basketRange);
            basket.position = new Vector3(Mathf.Lerp(basket.position.x, bx, 1f - Mathf.Exp(-7f * Time.deltaTime)), basket.position.y, basket.position.z);

            float shake = SensorHub.Instance != null ? SensorHub.Instance.ShakeStrength : 0f;
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.S)) shake = 20f;
#endif
            cooldown -= Time.deltaTime;
            if (shake > shakeStrength && cooldown <= 0f && onTree.Count > 0)
            {
                cooldown = 0.45f;
                int n = Mathf.Min(onTree.Count, Random.value < 0.4f ? 2 : 1);
                for (int i = 0; i < n; i++) Drop();
                if (tree != null) StartCoroutine(Rustle());
            }

            for (int i = falling.Count - 1; i >= 0; i--)
            {
                Falling f = falling[i];
                f.vy -= gravity * Time.deltaTime;
                Vector3 p = f.t.position + new Vector3(f.vx, f.vy, 0f) * Time.deltaTime;
                f.t.position = p;
                f.t.Rotate(0, 0, f.spin * Time.deltaTime);
                if (f.vy < 0f && p.y <= basketTopY && p.y > basketTopY - 0.35f && Mathf.Abs(p.x - basket.position.x) < basketHalfWidth)
                {
                    // dentro il cesto
                    falling.RemoveAt(i);
                    f.t.SetParent(basket, true);
                    f.t.localPosition = new Vector3(Random.Range(-0.35f, 0.35f), 0.66f + caught * 0.03f, 0f);
                    f.t.localRotation = Quaternion.identity;
                    caught++;
                    UpdateCounter();
                    if (caught >= needed) { StartCoroutine(Yum()); return; }
                }
                else if (p.y < groundY)
                {
                    falling.RemoveAt(i);
                    f.t.gameObject.SetActive(false);
                    lost++;
                    if (bubble != null) { bubble.Show("Bonk!"); StartCoroutine(HideLater(0.7f)); }
                }
            }
            if (onTree.Count + falling.Count + caught < needed) StartCoroutine(Hungry());
        }

        void Drop()
        {
            int i = Random.Range(0, onTree.Count);
            Transform a = onTree[i];
            onTree.RemoveAt(i);
            falling.Add(new Falling { t = a, vy = Random.Range(0.5f, 1.5f), vx = Random.Range(-0.5f, 0.5f), spin = Random.Range(-200f, 200f) });
        }

        IEnumerator Rustle()
        {
            for (float k = 0f; k < 0.35f; k += Time.deltaTime)
            {
                tree.localPosition = treeHome + Vector3.right * Mathf.Sin(k * 60f) * 0.06f;
                yield return null;
            }
            tree.localPosition = treeHome;
        }

        void UpdateCounter() { if (counter != null) counter.text = Mathf.Min(caught, needed) + " / " + needed; }

        IEnumerator Yum()
        {
            done = true;
            if (bubble != null) bubble.Show("Apple pie tonight!");
            yield return new WaitForSeconds(1.5f);
            Win();
        }

        IEnumerator Hungry()
        {
            done = true;
            if (bubble != null) bubble.Show("Too many on the ground...");
            yield return new WaitForSeconds(1.5f);
            Fail();
        }

        IEnumerator HideLater(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (bubble != null && !done) bubble.Hide();
        }
    }
}
