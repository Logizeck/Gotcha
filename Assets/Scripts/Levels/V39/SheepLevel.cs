using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Count the Sheep — il bambino non dorme: conta le pecore che saltano lo steccato (da sinistra a destra, a
    /// velocità e altezze diverse, a volte in coppia). Trucchi: una pecora si ferma sullo steccato e torna indietro,
    /// un lupo travestito da pecora (orecchie e coda) non conta, un agnellino passa SOTTO lo steccato e conta.
    /// Poi si scrive il numero sul tastierino. Giusto: il bambino si addormenta. Sbagliato: nuovo giro (3 tentativi).
    /// </summary>
    public class SheepLevel : LevelController
    {
        [SerializeField] Transform sheepTemplate;
        [SerializeField] Transform wolfTemplate;
        [SerializeField] Transform actors;
        [SerializeField] GameObject kidAwake;
        [SerializeField] GameObject kidAsleep;
        [SerializeField] Transform zzzTemplate;
        [SerializeField] GameObject pad;
        [SerializeField] TapButton[] keys;          // 0..9
        [SerializeField] TapButton delKey;
        [SerializeField] TapButton okKey;
        [SerializeField] TextMesh display;
        [SerializeField] Transform[] hearts;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] float groundY = -2.35f, fenceX = 0f, fenceTop = 0.87f, jumpHalf = 1.2f, startX = -3.8f, endX = 3.8f;

        enum Kind { Sheep, Back, Wolf, Lamb }
        int answer, tries = 3, running, order;
        string typed = "";
        bool padOpen, done;

        public override void OnLevelStart()
        {
            sheepTemplate.gameObject.SetActive(false);
            wolfTemplate.gameObject.SetActive(false);
            zzzTemplate.gameObject.SetActive(false);
            pad.SetActive(false);
            kidAsleep.SetActive(false);
            for (int i = 0; i < keys.Length; i++) { int n = i; keys[i].Pressed += _ => Type(n); }
            delKey.Pressed += _ => Delete();
            okKey.Pressed += _ => Confirm();
            StartCoroutine(Parade(true));
        }

        IEnumerator Parade(bool first)
        {
            if (bubble != null) bubble.Show(first ? "I can't sleep... Count the sheep for me!" : "Okay... count again, carefully!");
            yield return new WaitForSeconds(2.4f);
            if (bubble != null) bubble.Hide();
            answer = Random.Range(9, 15);
            // gruppi: pecore singole o in coppia, più i tre "speciali" sparsi a caso (mai per primi)
            var groups = new List<Kind[]>();
            int normals = answer - 1;   // l'agnellino conta
            while (normals > 0)
            {
                if (normals >= 2 && Random.value < 0.3f) { groups.Add(new[] { Kind.Sheep, Kind.Sheep }); normals -= 2; }
                else { groups.Add(new[] { Kind.Sheep }); normals--; }
            }
            foreach (Kind k in new[] { Kind.Back, Kind.Wolf, Kind.Lamb }) groups.Insert(Random.Range(1, groups.Count + 1), new[] { k });
            running = 0;
            foreach (Kind[] g in groups)
            {
                float speed = Random.Range(2.4f, 3.4f);
                for (int i = 0; i < g.Length; i++)
                {
                    StartCoroutine(Actor(g[i], speed, Random.Range(1.3f, 1.8f)));
                    if (i < g.Length - 1) yield return new WaitForSeconds(0.42f);
                }
                yield return new WaitForSeconds(Random.Range(0.75f, 1.25f));
            }
            while (running > 0) yield return null;
            yield return new WaitForSeconds(0.4f);
            OpenPad();
        }

        IEnumerator Actor(Kind kind, float speed, float apex)
        {
            running++;
            Transform t = Instantiate(kind == Kind.Wolf ? wolfTemplate : sheepTemplate, actors);
            t.gameObject.SetActive(true);
            float scale = kind == Kind.Lamb ? 0.42f : 0.8f;
            t.localScale = Vector3.one * scale;
            foreach (SpriteRenderer sr in t.GetComponentsInChildren<SpriteRenderer>()) sr.sortingOrder = 6 + (order++ % 20);
            float x = startX;
            float runPhase = Random.value * 6f;
            if (kind == Kind.Lamb)
            {
                // trotterella sotto lo steccato
                speed = 3.3f;
                while (x < endX)
                {
                    x += speed * Time.deltaTime;
                    runPhase += Time.deltaTime * 22f;
                    float duck = Mathf.Abs(x - fenceX) < 0.8f ? 0.85f : 1f;
                    t.localScale = new Vector3(scale, scale * duck, 1f);
                    t.localPosition = new Vector3(x, groundY + Mathf.Abs(Mathf.Sin(runPhase)) * 0.04f, 0f);
                    yield return null;
                }
            }
            else if (kind == Kind.Back)
            {
                float jumpFrom = fenceX - jumpHalf;
                while (x < jumpFrom) { x += speed * Time.deltaTime; runPhase += Time.deltaTime * 16f; t.localPosition = new Vector3(x, groundY + Mathf.Abs(Mathf.Sin(runPhase)) * 0.05f, 0f); yield return null; }
                Vector3 a = new Vector3(jumpFrom, groundY, 0f), top = new Vector3(fenceX, groundY + fenceTop, 0f);
                yield return Jump(t, a, top, 0.7f, 0.45f);
                // ci ripensa
                yield return new WaitForSeconds(0.5f);
                t.localScale = new Vector3(-scale, scale, 1f);
                yield return new WaitForSeconds(0.6f);
                yield return Jump(t, top, a, 0.5f, 0.45f);
                x = jumpFrom;
                while (x > startX) { x -= speed * Time.deltaTime; runPhase += Time.deltaTime * 16f; t.localPosition = new Vector3(x, groundY + Mathf.Abs(Mathf.Sin(runPhase)) * 0.05f, 0f); yield return null; }
            }
            else
            {
                while (x < endX)
                {
                    x += speed * Time.deltaTime;
                    runPhase += Time.deltaTime * 16f;
                    float u = (x - fenceX) / jumpHalf, y = groundY, rot = 0f;
                    if (Mathf.Abs(u) < 1f) { y += apex * (1f - u * u); rot = u * 18f; }
                    else y += Mathf.Abs(Mathf.Sin(runPhase)) * 0.05f;
                    t.localPosition = new Vector3(x, y, 0f);
                    t.localRotation = Quaternion.Euler(0, 0, rot);
                    yield return null;
                }
            }
            Destroy(t.gameObject);
            running--;
        }

        static IEnumerator Jump(Transform t, Vector3 from, Vector3 to, float height, float seconds)
        {
            for (float k = 0f; k < 1f; k += Time.deltaTime / seconds)
            {
                t.localPosition = Vector3.Lerp(from, to, k) + Vector3.up * Mathf.Sin(k * Mathf.PI) * height;
                yield return null;
            }
            t.localPosition = to;
        }

        void OpenPad()
        {
            typed = "";
            Show();
            pad.SetActive(true);
            padOpen = true;
            if (bubble != null) bubble.Show("So? How many?");
        }

        void Show() { display.text = typed.Length == 0 ? "_" : typed; }

        void Type(int n)
        {
            if (!padOpen || typed.Length >= 2) return;
            if (typed == "0") typed = "";
            typed += n;
            Sfx.Play(this, "tick", 0.6f);
            Show();
        }

        void Delete()
        {
            if (!padOpen || typed.Length == 0) return;
            typed = typed.Substring(0, typed.Length - 1);
            Sfx.Play(this, "tick", 0.5f);
            Show();
        }

        void Confirm()
        {
            if (!padOpen || typed.Length == 0) return;
            padOpen = false;
            pad.SetActive(false);
            if (int.Parse(typed) == answer) StartCoroutine(Sleep());
            else StartCoroutine(Wrong());
        }

        IEnumerator Sleep()
        {
            done = true;
            Sfx.Play(this, "ding", 0.6f);
            if (bubble != null) bubble.Show(answer + "... yawn...");
            yield return new WaitForSeconds(1.2f);
            if (bubble != null) bubble.Hide();
            kidAwake.SetActive(false);
            kidAsleep.SetActive(true);
            for (int i = 0; i < 3; i++)
            {
                Sfx.Play(this, "snore", 0.7f);
                StartCoroutine(Zzz());
                yield return new WaitForSeconds(0.7f);
            }
            yield return new WaitForSeconds(0.6f);
            Win();
        }

        IEnumerator Zzz()
        {
            Transform z = Instantiate(zzzTemplate, zzzTemplate.parent);
            z.gameObject.SetActive(true);
            Vector3 from = zzzTemplate.localPosition;
            for (float k = 0f; k < 1.6f; k += Time.deltaTime)
            {
                z.localPosition = from + new Vector3(Mathf.Sin(k * 4f) * 0.15f + k * 0.3f, k * 0.6f, 0f);
                z.localScale = Vector3.one * (0.6f + k * 0.5f);
                yield return null;
            }
            Destroy(z.gameObject);
        }

        IEnumerator Wrong()
        {
            Haptics.Medium();
            Sfx.Play(this, "buzz", 0.6f);
            tries--;
            if (tries >= 0 && tries < hearts.Length) hearts[tries].gameObject.SetActive(false);
            if (bubble != null) bubble.Show("Not sleepy at all... It was " + answer + "!");
            yield return new WaitForSeconds(2.2f);
            if (tries <= 0)
            {
                done = true;
                if (bubble != null) bubble.Show("Wide awake. Forever.");
                yield return new WaitForSeconds(1.3f);
                Fail();
                yield break;
            }
            if (!done) StartCoroutine(Parade(false));
        }
    }
}
