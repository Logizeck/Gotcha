using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gotcha.Components
{
    /// <summary>
    /// Il carlino nero, animato a pezzi (corpo, testa, orecchie, occhi, bocca, lingua, coda) senza Animator:
    /// ogni stato è una piccola "curva" calcolata in Update. Stati: Idle (respira e sbatte le palpebre),
    /// Sleep (occhi chiusi, punta della lingua, bolla al naso, Zzz), Bark (abbaia a ripetizione),
    /// Stare (ti fissa con la lingua di lato) e Sneeze (prende fiato e starnutisce goccioline, poi passa a Stare).
    /// Coordinate dei pezzi: origine tra le zampe a terra, testa centrata in (0, 1.5).
    /// </summary>
    public class PugRig : MonoBehaviour
    {
        public enum Mode { Idle, Sleep, Bark, Stare, Sneeze }

        [Header("Pezzi (pivot già sulle articolazioni)")]
        public Transform body;
        public Transform head;
        public Transform earL, earR;
        public Transform eyesOpen;
        public GameObject eyesClosed;
        public Transform mouthOpen;
        public Transform tongue;
        public GameObject blep;
        public Transform snot;
        public Transform tail;

        [Header("Effetti")]
        public SmokeEmitter zzz;
        public SpeechBubble bubble;
        public Sprite dropSprite;
        public Transform noseTip;
        public int dropCount = 11;

        [Header("Tempi")]
        public float barkPeriod = 1f;
        public float sneezeInhale = 0.9f;
        public float snotPeriod = 3.6f;

        public Mode Current { get; private set; } = Mode.Idle;
        /// <summary>Lo snot è abbastanza gonfio da poterlo scoppiare.</summary>
        public bool SnotIsBig => Current == Mode.Sleep && snot != null && snot.gameObject.activeSelf && snot.localScale.x > 1.1f;
        /// <summary>Parte nell'istante dell'ETCIÙ (goccioline in volo).</summary>
        public event Action Sneezed;
        /// <summary>Parte a ogni "BAU!".</summary>
        public event Action Barked;

        struct Drop { public Transform t; public SpriteRenderer sr; public Vector2 v; public float age; }

        readonly List<Drop> drops = new List<Drop>();
        Vector3 headPos, bodyScale, eyesScale, snotScale;
        float started, nextBlink = 2f, blinkUntil;
        int lastBark = -1;
        bool sneezed;

        void Awake()
        {
            headPos = head.localPosition;
            bodyScale = body.localScale;
            eyesScale = eyesOpen.localScale;
            if (snot != null) snotScale = snot.localScale;
            Apply(Mode.Idle);
        }

        public void Play(Mode mode)
        {
            Current = mode;
            started = Time.time;
            lastBark = -1;
            sneezed = false;
            Apply(mode);
        }

        void Apply(Mode mode)
        {
            bool sleep = mode == Mode.Sleep;
            eyesOpen.gameObject.SetActive(!sleep);
            if (eyesClosed != null) eyesClosed.SetActive(sleep);
            if (blep != null) blep.SetActive(sleep);
            if (snot != null) snot.gameObject.SetActive(sleep);
            if (tongue != null) tongue.gameObject.SetActive(mode == Mode.Stare);
            if (mouthOpen != null) mouthOpen.gameObject.SetActive(mode == Mode.Bark);
            if (zzz != null) { if (sleep) zzz.Play(); else zzz.Stop(); }
            if (bubble != null && mode != Mode.Bark && mode != Mode.Sneeze) bubble.Hide();
        }

        /// <summary>Scoppia la bolla al naso (il carlino resta addormentato solo se non lo si sveglia).</summary>
        public void PopSnot()
        {
            if (snot == null) return;
            snot.gameObject.SetActive(false);
            Sfx.Play(this, "pop", 0.7f);
        }

        void Update()
        {
            float t = Time.time - started;
            // valori a riposo, poi ogni stato li modifica
            Vector3 hp = headPos;
            float hr = 0f, earAngle = 0f, eyesY = 1f, eyesSize = 1f, tailAngle = 0f, mouth = 0f;
            Vector3 bs = bodyScale;
            float breathe = Mathf.Sin(Time.time * 2f * Mathf.PI / 3.2f);

            switch (Current)
            {
                case Mode.Idle:
                    bs = new Vector3(bodyScale.x * (1f + breathe * 0.006f), bodyScale.y * (1f + breathe * 0.012f), 1f);
                    eyesY = Blink();
                    tailAngle = Mathf.Sin(Time.time * 4f) * 10f;
                    break;

                case Mode.Sleep:
                {
                    float deep = Mathf.Sin(Time.time * 2f * Mathf.PI / 3.6f);
                    bs = new Vector3(bodyScale.x * (1f + deep * 0.012f), bodyScale.y * (1f + deep * 0.03f), 1f);
                    hp += new Vector3(0f, -0.09f + deep * 0.02f, 0f);
                    hr = 6f;
                    earAngle = 6f;
                    tailAngle = -6f;
                    if (snot != null && snot.gameObject.activeSelf)
                    {
                        float k = Mathf.Repeat(t, snotPeriod) / snotPeriod;   // cresce piano, scoppia da solo
                        float s = k < 0.82f ? Mathf.Lerp(0.15f, 1.6f, k / 0.82f) : k < 0.86f ? 1.9f : 0f;
                        snot.localScale = snotScale * s;
                    }
                    break;
                }

                case Mode.Bark:
                {
                    int n = Mathf.FloorToInt(t / barkPeriod);
                    float k = Mathf.Repeat(t, barkPeriod) / barkPeriod;
                    if (n != lastBark && k >= 0.15f)
                    {
                        lastBark = n;
                        Sfx.Play(this, "woof", 0.8f);
                        if (bubble != null) bubble.Show("WOOF!", true);
                        Barked?.Invoke();
                    }
                    if (bubble != null && k > 0.75f && bubble.gameObject.activeSelf) bubble.Hide();
                    float jerk = Bump(k, 0.12f, 0.22f, 0.5f);
                    hp += new Vector3(0f, 0.12f * jerk - 0.05f * Bump(k, 0f, 0.1f, 0.18f), 0f);
                    hr = -4f * jerk;
                    mouth = Bump(k, 0.1f, 0.2f, 0.5f);
                    earAngle = -26f * Bump(k, 0.12f, 0.22f, 0.4f) + 8f * Bump(k, 0.35f, 0.42f, 0.6f);
                    bs = new Vector3(bodyScale.x * (1f + 0.02f * Bump(k, 0f, 0.1f, 0.2f)), bodyScale.y * (1f + 0.03f * jerk), 1f);
                    tailAngle = Mathf.Sin(Time.time * 25f) * 14f;
                    break;
                }

                case Mode.Stare:
                    eyesSize = 1.08f;
                    hr = -8f * Mathf.SmoothStep(0f, 1f, Mathf.PingPong(t / 2.5f, 1f));   // inclina la testa
                    eyesY = Blink(5.5f);
                    if (tongue != null) tongue.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(t * 5.5f) * 7f);
                    tailAngle = Mathf.Sin(Time.time * 2.6f) * 12f;
                    bs = new Vector3(bodyScale.x * (1f + breathe * 0.006f), bodyScale.y * (1f + breathe * 0.012f), 1f);
                    break;

                case Mode.Sneeze:
                {
                    float burst = sneezeInhale;
                    if (t < burst)   // prende fiato: testa indietro, occhi che si stringono
                    {
                        float k = t / burst;
                        hp += new Vector3(0f, 0.08f * k, 0f);
                        hr = 6f * k;
                        eyesY = Mathf.Lerp(1f, 0.12f, k * k);
                        bs = new Vector3(bodyScale.x * (1f - 0.01f * k), bodyScale.y * (1f + 0.04f * k), 1f);
                    }
                    else
                    {
                        if (!sneezed)
                        {
                            sneezed = true;
                            SpawnDrops();
                            Sfx.Play(this, "achoo", 0.9f);
                            Haptics.Light();
                            if (bubble != null) bubble.Show("ACHOO!", true);
                            Sneezed?.Invoke();
                        }
                        float k = Mathf.Clamp01((t - burst) / 0.9f);
                        float snap = Mathf.Exp(-k * 5f) * Mathf.Cos(k * 14f);   // scatto in avanti che rimbalza
                        hp += new Vector3(0f, -0.13f * snap, 0f);
                        hr = -5f * snap;
                        eyesY = Mathf.Lerp(0.08f, 1f, Mathf.Clamp01((k - 0.35f) / 0.4f));
                        earAngle = -34f * snap;
                        bs = new Vector3(bodyScale.x * (1f + 0.03f * snap), bodyScale.y * (1f - 0.04f * snap), 1f);
                        if (t > burst + 1.6f) { if (bubble != null) bubble.Hide(); Play(Mode.Stare); }
                    }
                    break;
                }
            }

            head.localPosition = hp;
            head.localRotation = Quaternion.Euler(0, 0, hr);
            body.localScale = bs;
            eyesOpen.localScale = new Vector3(eyesScale.x * eyesSize, eyesScale.y * eyesSize * eyesY, eyesScale.z);
            if (earL != null) earL.localRotation = Quaternion.Euler(0, 0, earAngle);    // negativo = la punta si alza
            if (earR != null) earR.localRotation = Quaternion.Euler(0, 0, -earAngle);
            if (tail != null) tail.localRotation = Quaternion.Euler(0, 0, tailAngle);
            if (mouthOpen != null && Current == Mode.Bark)
            {
                // bocca aperta "a scatto" come nei cartoni (lo sprite contiene anche guance e naso: non si deforma)
                mouthOpen.gameObject.SetActive(mouth > 0.3f);
            }
            UpdateDrops();
        }

        /// <summary>Palpebre: 1 aperte, quasi 0 chiuse; un battito ogni "every" secondi circa.</summary>
        float Blink(float every = 4f)
        {
            if (Time.time >= nextBlink) { blinkUntil = Time.time + 0.14f; nextBlink = Time.time + every * UnityEngine.Random.Range(0.7f, 1.3f); }
            return Time.time < blinkUntil ? 0.08f : 1f;
        }

        /// <summary>Curva a campana: 0 prima di a, 1 in b, di nuovo 0 in c.</summary>
        static float Bump(float x, float a, float b, float c)
        {
            if (x <= a || x >= c) return 0f;
            return x < b ? Mathf.SmoothStep(0f, 1f, (x - a) / (b - a)) : Mathf.SmoothStep(1f, 0f, (x - b) / (c - b));
        }

        // ---------- goccioline dello starnuto ----------

        void SpawnDrops()
        {
            if (dropSprite == null || noseTip == null) return;
            for (int i = 0; i < dropCount; i++)
            {
                var go = new GameObject("SneezeDrop");
                go.transform.SetParent(transform.parent != null ? transform.parent : transform, true);
                go.transform.position = noseTip.position;
                SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = dropSprite;
                sr.color = new Color(0.8f, 0.93f, 1f, 0.95f);
                sr.sortingOrder = 45;
                float a = Mathf.Lerp(15f, 165f, (i + UnityEngine.Random.value * 0.8f) / dropCount) * Mathf.Deg2Rad;
                float speed = UnityEngine.Random.Range(2.2f, 3.6f);
                go.transform.localScale = Vector3.one * UnityEngine.Random.Range(0.6f, 1.15f);
                drops.Add(new Drop { t = go.transform, sr = sr, v = new Vector2(Mathf.Cos(a), Mathf.Sin(a) * 0.8f) * speed });
            }
        }

        void UpdateDrops()
        {
            for (int i = drops.Count - 1; i >= 0; i--)
            {
                Drop d = drops[i];
                if (d.t == null) { drops.RemoveAt(i); continue; }
                d.age += Time.deltaTime;
                d.v += Vector2.down * 7f * Time.deltaTime;
                d.t.position += (Vector3)(d.v * Time.deltaTime);
                d.t.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(d.v.y, d.v.x) * Mathf.Rad2Deg + 90f);
                Color c = d.sr.color;
                c.a = 0.95f * Mathf.Clamp01(1.3f - d.age);
                d.sr.color = c;
                if (d.age > 1.3f) { Destroy(d.t.gameObject); drops.RemoveAt(i); continue; }
                drops[i] = d;
            }
        }

        void OnDestroy()
        {
            foreach (Drop d in drops) if (d.t != null) Destroy(d.t.gameObject);
        }
    }
}
