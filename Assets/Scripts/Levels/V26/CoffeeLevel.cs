using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 89 — il caffè. Inclina il telefono per inclinare la caffettiera: più la inclini, più il getto è forte
    /// e lungo (e arriva più lontano). Riempi la tazza fino alla riga, poi raddrizza. Troppo = trabocca.
    /// Easter egg `coffee_cat`: versarlo sul gatto che dorme lì accanto.
    /// </summary>
    public class CoffeeLevel : LevelController
    {
        [SerializeField] Transform pot;
        [SerializeField] Transform spout;
        [SerializeField] Transform mug;               // base al centro del fondo
        [SerializeField] Transform fill;              // scala y 0..1
        [SerializeField] Transform line;              // riga del livello giusto
        [SerializeField] Collider2D cat;
        [SerializeField] GameObject catSleep, catAwake;
        [SerializeField] Sprite dropSprite;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] float mugWidth = 0.8f, mugHeight = 1.0f;
        [SerializeField] float maxAngle = 115f, pourFrom = 45f;
        [SerializeField] float dropVolume = 0.012f;
        [SerializeField] float floorY = -3.4f;
        [SerializeField] string eggId = "coffee_cat";

        readonly SteadyTilt steady = new SteadyTilt { smoothing = 0.12f, deadZone = 0.015f, shakeLevel = 99f };
        readonly List<Transform> drops = new List<Transform>();
        readonly List<Vector2> vels = new List<Vector2>();
        float level, target, potAngle, spawn, idle, catDrops;
        bool done;

        public override void OnLevelStart()
        {
            target = Random.Range(0.55f, 0.85f);
            mug.position = new Vector3(Random.Range(0.4f, 1.9f), mug.position.y, mug.position.z);
            line.localPosition = new Vector3(line.localPosition.x, target * mugHeight, line.localPosition.z);
            catAwake.SetActive(false);
            Show();
            if (bubble != null) { bubble.Show("Fill it to the line. Not a drop more!"); StartCoroutine(HideLater(2.2f)); }
        }

        void Show() { fill.localScale = new Vector3(1f, Mathf.Clamp01(level), 1f); }

        void Update()
        {
            if (done || IsFinished) return;
            float dt = Time.deltaTime;
            float want = Mathf.Clamp(steady.Update(dt) * 380f, 0f, maxAngle);
            potAngle = Mathf.MoveTowards(potAngle, want, 160f * dt);
            pot.localRotation = Quaternion.Euler(0, 0, -potAngle);

            float flow = Mathf.InverseLerp(pourFrom, maxAngle, potAngle);
            if (flow > 0f)
            {
                spawn += dt * Mathf.Lerp(12f, 45f, flow);
                while (spawn >= 1f)
                {
                    spawn -= 1f;
                    Vector2 dir = Quaternion.Euler(0, 0, -potAngle) * Vector2.right;
                    Spawn(spout.position, dir * Mathf.Lerp(0.6f, 3.2f, flow) + Random.insideUnitCircle * 0.1f);
                }
                idle = 0f;
            }
            else idle += dt;

            for (int i = drops.Count - 1; i >= 0; i--)
            {
                Vector2 v = vels[i] + Vector2.down * 9f * dt;
                vels[i] = v;
                Transform d = drops[i];
                d.position += (Vector3)(v * dt);
                Vector2 p = d.position;
                float top = mug.position.y + mugHeight;
                if (Mathf.Abs(p.x - mug.position.x) < mugWidth * 0.5f && p.y < top && p.y > mug.position.y)
                {
                    level += dropVolume;
                    Kill(i);
                    Show();
                    if (level > 1f) { StartCoroutine(Overflow()); return; }
                    continue;
                }
                if (cat.OverlapPoint(p))
                {
                    catDrops++;
                    Kill(i);
                    if (catDrops >= 6) { StartCoroutine(WiredCat()); return; }
                    continue;
                }
                if (p.y < floorY) Kill(i);
            }

            if (idle > 1.2f && drops.Count == 0 && level > 0.05f)
            {
                if (Mathf.Abs(level - target) <= 0.07f) StartCoroutine(Perfect());
                else if (level > target) StartCoroutine(TooMuch());
            }
        }

        void Spawn(Vector2 at, Vector2 v)
        {
            var go = new GameObject("Drop");
            go.transform.SetParent(transform, false);
            go.transform.position = at;
            go.transform.localScale = Vector3.one * 0.45f;
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = dropSprite;
            sr.color = new Color(0.45f, 0.27f, 0.15f);
            sr.sortingOrder = 8;
            drops.Add(go.transform);
            vels.Add(v);
        }

        void Kill(int i) { Destroy(drops[i].gameObject); drops.RemoveAt(i); vels.RemoveAt(i); }

        IEnumerator Perfect()
        {
            done = true;
            if (bubble != null) bubble.Show("Exactly to the line. Barista!");
            yield return new WaitForSeconds(1.5f);
            Win();
        }

        IEnumerator TooMuch()
        {
            done = true;
            if (bubble != null) bubble.Show("Over the line! Where's the milk going?");
            yield return new WaitForSeconds(1.5f);
            Fail();
        }

        IEnumerator Overflow()
        {
            done = true;
            if (bubble != null) bubble.Show("SPLASH! It's everywhere!");
            yield return new WaitForSeconds(1.4f);
            Fail();
        }

        /// <summary>Easter egg: il gatto si sveglia pieno di caffeina e non dormirà mai più.</summary>
        IEnumerator WiredCat()
        {
            done = true;
            catSleep.SetActive(false);
            catAwake.SetActive(true);
            if (bubble != null) bubble.Show("MEOW!!! I CAN SEE SOUNDS!!!");
            Transform c = cat.transform;
            Vector3 p = c.position;
            for (float k = 0f; k < 1.6f; k += Time.deltaTime)
            {
                c.position = p + new Vector3(Mathf.Sin(k * 23f) * 1.2f, Mathf.Abs(Mathf.Sin(k * 17f)) * 0.8f, 0f);
                c.localRotation = Quaternion.Euler(0, 0, k * 720f);
                yield return null;
            }
            FailWithEgg(eggId);
        }

        IEnumerator HideLater(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (bubble != null && !done) bubble.Hide();
        }
    }
}
