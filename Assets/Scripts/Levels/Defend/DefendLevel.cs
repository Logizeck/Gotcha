using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 12 — proteggi il gatto. Il gatto dorme e una vespa inferocita prova a pungerlo. Col dito si disegnano
    /// linee (al massimo 3, le più vecchie spariscono) che fanno da barriera: la vespa ci rimbalza contro.
    /// Resisti 10 secondi e la vespa, sfinita, se ne va. Se raggiunge il gatto: MEOW! e si riprova.
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

        Vector2 dir, hoverHome;
        Vector3 bubbleHome;
        float remaining, flash, waiting;
        bool done;

        Vector2 CatPoint => (Vector2)cat.position + new Vector2(-0.4f, 0.2f);

        public override void OnLevelStart()
        {
            remaining = survive;
            waiting = warmUp;
            hoverHome = wasp.position;
            dir = Quaternion.Euler(0, 0, Random.Range(-50f, 50f)) * (CatPoint - (Vector2)wasp.position).normalized;
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

            // prima di attaccare la vespa ronza furiosa sul posto: c'è tempo per disegnare le barriere
            if (waiting > 0f)
            {
                waiting -= Time.deltaTime;
                Vector2 hover = hoverHome + new Vector2(Mathf.Sin(Time.time * 5.3f) * 0.35f, Mathf.Sin(Time.time * 8.1f) * 0.18f);
                float jitter = Mathf.Sin(Time.time * 47f) * 0.03f;   // trema dalla rabbia
                wasp.position = new Vector3(hover.x + jitter, hover.y, wasp.position.z);
                wasp.localScale = new Vector3(CatPoint.x < hover.x ? 1f : -1f, 1f, 1f);
                if (countdown != null) countdown.text = Mathf.CeilToInt(survive).ToString();
                if (bubble != null && waiting <= 0f) { bubble.Hide(); bubble.transform.position = bubbleHome; }
                if (waiting > 0f) return;
                dir = Quaternion.Euler(0, 0, Random.Range(-50f, 50f)) * (CatPoint - (Vector2)wasp.position).normalized;
            }

            remaining -= Time.deltaTime;
            if (countdown != null) countdown.text = Mathf.CeilToInt(Mathf.Max(0f, remaining)).ToString();
            if (flash > 0f) flash -= Time.deltaTime;
            if (waspBody != null) waspBody.color = flash > 0f ? new Color(1f, 0.6f, 0.6f) : Color.white;

            // punta il gatto, con un po' di zig-zag
            Vector2 pos = wasp.position;
            Vector2 to = (CatPoint - pos).normalized;
            dir = (dir + to * steer * Time.deltaTime).normalized;
            Vector2 wiggle = new Vector2(-dir.y, dir.x) * Mathf.Sin(Time.time * 11f) * 0.35f;
            Vector2 step = (dir + wiggle).normalized * speed * Time.deltaTime;

            // rimbalza sulle linee disegnate (geometria esatta: non può attraversarle né restarci dentro)
            Vector2 next = pos + step;
            if (pen != null && StrokeBarrier.Resolve(pen.transform, pos, ref next, waspRadius, pen.colliderRadius, out Vector2 normal))
            {
                if (Vector2.Dot(dir, normal) < 0f) dir = Vector2.Reflect(dir, normal).normalized;
                flash = 0.12f;
            }
            pos = next;

            // bordi dello schermo
            if (pos.x < boundsMin.x || pos.x > boundsMax.x) { dir.x = -dir.x; pos.x = Mathf.Clamp(pos.x, boundsMin.x, boundsMax.x); }
            if (pos.y < boundsMin.y || pos.y > boundsMax.y) { dir.y = -dir.y; pos.y = Mathf.Clamp(pos.y, boundsMin.y, boundsMax.y); }
            wasp.position = new Vector3(pos.x, pos.y, wasp.position.z);
            wasp.localScale = new Vector3(dir.x < 0f ? 1f : -1f, 1f, 1f);

            if (Vector2.Distance(pos, CatPoint) < catRadius) { done = true; StartCoroutine(Stung()); }
            else if (remaining <= 0f) { done = true; StartCoroutine(Survived()); }
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
            // la vespa, sfinita, se ne va
            for (float k = 0f; k < 1.5f; k += Time.deltaTime)
            {
                wasp.position += new Vector3(0.6f, 2.8f, 0f) * Time.deltaTime;
                yield return null;
            }
            if (buzz != null) buzz.Stop();
            Win();
        }
    }
}
