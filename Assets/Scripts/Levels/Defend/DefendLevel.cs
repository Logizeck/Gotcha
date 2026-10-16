using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 12 — proteggi il gatto. Il gatto dorme e una vespa inferocita prova a pungerlo. Col dito si disegnano
    /// linee (al massimo 3, le più vecchie spariscono) che fanno da barriera: la vespa ci rimbalza contro.
    /// Resisti 10 secondi e la vespa, sfinita, se ne va. Se raggiunge il gatto: MEOW! e si riprova.
    /// Varianti: più vespe (<see cref="extraWasps"/>) e barriere "spostabili": una linea che non tocca il gatto
    /// (non è fissata a lui) viene spinta via un po' a ogni urto.
    /// </summary>
    public class DefendLevel : LevelController
    {
        [SerializeField] Transform wasp;
        [SerializeField] SpriteRenderer waspBody;
        [SerializeField] Transform cat;
        [SerializeField] GameObject catEyesOpen;
        [SerializeField] GameObject catAlert;
        [SerializeField] SmokeEmitter zzz;
        [SerializeField] StrokeDrawer pen;
        [SerializeField] TextMesh countdown;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] AudioSource buzz;
        [SerializeField] GameObject waspAnger;          // vena della rabbia che pulsa sulla testa
        [SerializeField] float warmUp = 3f;             // secondi in cui la vespa ronza furiosa in alto prima di attaccare
        [SerializeField] float survive = 10f;
        [SerializeField] float speed = 3.0f;
        [SerializeField] float steer = 2.4f;
        [SerializeField] float catRadius = 0.75f;
        [SerializeField] float waspRadius = 0.28f;
        [SerializeField] Vector2 boundsMin = new Vector2(-2.7f, -3.6f);
        [SerializeField] Vector2 boundsMax = new Vector2(2.7f, 3.4f);
        [SerializeField] Transform[] extraWasps;
        [SerializeField] bool pushUnanchored;
        [SerializeField] float anchorReach = 0.45f;     // una linea che passa così vicino al gatto è "fissata"
        [SerializeField] float pushPerHit = 0.09f;

        class Bug
        {
            public Transform t;
            public SpriteRenderer body;
            public Vector2 dir, home;
            public float flash, phase;
        }

        readonly System.Collections.Generic.List<Bug> bugs = new System.Collections.Generic.List<Bug>();
        Vector3 bubbleHome;
        float remaining, waiting;
        bool done;

        Vector2 CatPoint => (Vector2)cat.position + new Vector2(-0.4f, 0.2f);

        public override void OnLevelStart()
        {
            remaining = survive;
            waiting = warmUp;
            bugs.Add(new Bug { t = wasp, body = waspBody, home = wasp.position });
            if (extraWasps != null)
                foreach (Transform w in extraWasps)
                    if (w != null) bugs.Add(new Bug { t = w, body = w.GetComponentInChildren<SpriteRenderer>(), home = w.position, phase = Random.Range(0f, 6f) });
            foreach (Bug b in bugs) b.dir = Quaternion.Euler(0, 0, Random.Range(-50f, 50f)) * (CatPoint - b.home).normalized;
            if (catEyesOpen != null) catEyesOpen.SetActive(false);
            if (catAlert != null) catAlert.SetActive(false);
            if (zzz != null) zzz.Play();
            if (bubble != null)
            {
                bubbleHome = bubble.transform.position;
                bubble.transform.position = wasp.position + new Vector3(1.5f, -0.75f, 0f);   // la vespa minaccia
                bubble.Show("BZZZ!!", true);   // parla la vespa, non il gatto
            }
            if (buzz != null)
            {
                buzz.clip = WaspLevel.BuzzClip();
                buzz.loop = true;
                buzz.volume = 0.35f;
                buzz.Play();
            }
        }

        void OnDestroy()
        {
            if (buzz != null && buzz.clip != null) Destroy(buzz.clip);
        }

        void Update()
        {
            if (done || IsFinished) return;
            if (waspAnger != null) waspAnger.transform.localScale = Vector3.one * (1f + Mathf.Abs(Mathf.Sin(Time.time * 9f)) * 0.35f);

            // prima di attaccare le vespe ronzano furiose sul posto: c'è tempo per disegnare le barriere
            if (waiting > 0f)
            {
                waiting -= Time.deltaTime;
                foreach (Bug b in bugs)
                {
                    float t = Time.time + b.phase;
                    Vector2 hover = b.home + new Vector2(Mathf.Sin(t * 5.3f) * 0.35f, Mathf.Sin(t * 8.1f) * 0.18f);
                    float jitter = Mathf.Sin(t * 47f) * 0.03f;   // trema dalla rabbia
                    b.t.position = new Vector3(hover.x + jitter, hover.y, b.t.position.z);
                    b.t.localScale = new Vector3(CatPoint.x < hover.x ? 1f : -1f, 1f, 1f);
                }
                if (countdown != null) countdown.text = Mathf.CeilToInt(survive).ToString();
                if (bubble != null && waiting <= 0f) { bubble.Hide(); bubble.transform.position = bubbleHome; }
                if (waiting > 0f) return;
                foreach (Bug b in bugs) b.dir = Quaternion.Euler(0, 0, Random.Range(-50f, 50f)) * (CatPoint - (Vector2)b.t.position).normalized;
            }

            remaining -= Time.deltaTime;
            if (countdown != null) countdown.text = Mathf.CeilToInt(Mathf.Max(0f, remaining)).ToString();
            foreach (Bug b in bugs)
            {
                if (MoveBug(b)) { done = true; StartCoroutine(Stung()); return; }
            }
            if (remaining <= 0f) { done = true; StartCoroutine(Survived()); }
        }

        /// <summary>Muove una vespa; true se ha punto il gatto.</summary>
        bool MoveBug(Bug b)
        {
            if (b.flash > 0f) b.flash -= Time.deltaTime;
            if (b.body != null) b.body.color = b.flash > 0f ? new Color(1f, 0.6f, 0.6f) : Color.white;

            // punta il gatto, con un po' di zig-zag
            Vector2 pos = b.t.position;
            Vector2 to = (CatPoint - pos).normalized;
            b.dir = (b.dir + to * steer * Time.deltaTime).normalized;
            Vector2 wiggle = new Vector2(-b.dir.y, b.dir.x) * Mathf.Sin((Time.time + b.phase) * 11f) * 0.35f;
            Vector2 next = pos + (b.dir + wiggle).normalized * speed * Time.deltaTime;

            // prima i bordi dello schermo, poi le linee: così il rimbalzo sul bordo non la spinge oltre una linea
            if (next.x < boundsMin.x || next.x > boundsMax.x) { b.dir.x = -b.dir.x; next.x = Mathf.Clamp(next.x, boundsMin.x, boundsMax.x); }
            if (next.y < boundsMin.y || next.y > boundsMax.y) { b.dir.y = -b.dir.y; next.y = Mathf.Clamp(next.y, boundsMin.y, boundsMax.y); }

            // rimbalza sulle linee disegnate (geometria esatta, anche la linea che si sta ancora disegnando)
            if (pen != null && StrokeBarrier.Resolve(pen.transform, pos, ref next, waspRadius, pen.colliderRadius, out Vector2 normal, out Transform stroke))
            {
                if (Vector2.Dot(b.dir, normal) < 0f) b.dir = Vector2.Reflect(b.dir, normal).normalized;
                b.flash = 0.12f;
                if (pushUnanchored && stroke != null && !Anchored(stroke)) Push(stroke, -normal);
            }
            b.t.position = new Vector3(next.x, next.y, b.t.position.z);
            b.t.localScale = new Vector3(b.dir.x < 0f ? 1f : -1f, 1f, 1f);

            // punge solo se arriva al gatto senza una linea in mezzo
            return Vector2.Distance(next, CatPoint) < catRadius && (pen == null || !StrokeBarrier.Blocks(pen.transform, next, CatPoint, 0f));
        }

        /// <summary>Una linea è fissata al gatto se in qualche punto lo tocca.</summary>
        bool Anchored(Transform stroke)
        {
            EdgeCollider2D e = stroke.GetComponent<EdgeCollider2D>();
            if (e == null) return true;   // ancora in disegno: non si sposta
            foreach (Vector2 p in e.points)
                if (Vector2.Distance(stroke.TransformPoint(p), CatPoint) < catRadius + anchorReach) return true;
            return false;
        }

        void Push(Transform stroke, Vector2 dir)
        {
            stroke.position += (Vector3)(dir * pushPerHit);
            stroke.Rotate(0, 0, Random.Range(-4f, 4f));
            if (bubble != null && Random.value < 0.25f) { bubble.transform.position = (Vector3)stroke.position + new Vector3(0.8f, 0.5f, 0f); bubble.Show("Shove!", true); StartCoroutine(BubbleBack()); }
        }

        IEnumerator BubbleBack()
        {
            yield return new WaitForSeconds(0.6f);
            if (bubble != null && !done) { bubble.Hide(); bubble.transform.position = bubbleHome; }
        }

        IEnumerator Stung()
        {
            if (zzz != null) zzz.Stop();
            if (bubble != null) bubble.Hide();
            if (catEyesOpen != null) catEyesOpen.SetActive(true);
            if (catAlert != null) catAlert.SetActive(true);
            if (bubble != null) bubble.Show("MEOW!!");
            Vector3 start = cat.localPosition;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.5f)
            {
                cat.localPosition = start + Vector3.up * Mathf.Sin(k * Mathf.PI) * 0.6f;
                yield return null;
            }
            cat.localPosition = start;
            yield return new WaitForSeconds(0.6f);
            Fail();
        }

        IEnumerator Survived()
        {
            if (countdown != null) countdown.text = "";
            if (waspAnger != null) waspAnger.SetActive(false);
            if (bubble != null) bubble.Show("Phew!");
            // le vespe, sfinite, se ne vanno
            for (float k = 0f; k < 1.5f; k += Time.deltaTime)
            {
                foreach (Bug b in bugs) b.t.position += new Vector3(0.6f, 2.8f, 0f) * Time.deltaTime;
                yield return null;
            }
            if (buzz != null) buzz.Stop();
            Win();
        }
    }
}
