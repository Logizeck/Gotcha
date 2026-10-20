using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "The escaping cursor" — un cursore a freccia scappa da ogni dito che lo insegue. Soluzione: trascinare il
    /// BERSAGLIO sotto la punta del cursore (lui scappa dal dito, non dal bersaglio). Dopo cinque inseguimenti (tocchi
    /// fuori dal bersaglio) il cursore esce dallo schermo: sconfitta.
    /// </summary>
    public class TheEscapingCursorLevel : S41ILevelBase
    {
        [SerializeField] Transform cursor, target;
        [SerializeField] Collider2D targetArea;
        [SerializeField] TextMesh triesText;
        [SerializeField] Vector2 boundsMin = new Vector2(-2.3f, -3.9f), boundsMax = new Vector2(2.4f, 2.2f);

        const int MaxChases = 5;
        int chases, dragId = int.MinValue;
        Vector2 wander, grabOffset;
        float nextWander;

        protected override void Begin()
        {
            triesText.text = "CHASES: " + Marks(0, MaxChases);
            wander = cursor.position;
            Say("Click the target!", 2f);
        }

        Vector2 Clamp(Vector2 p) => new Vector2(Mathf.Clamp(p.x, boundsMin.x, boundsMax.x), Mathf.Clamp(p.y, boundsMin.y, boundsMax.y));

        protected override void Tick()
        {
            if (busy) return;
            float dt = Time.deltaTime;
            Vector2 c = cursor.position;
            Vector2 flee = Vector2.zero;
            bool holding = false;
            foreach (Finger f in Fingers())
            {
                if (f.id == dragId)
                {
                    holding = !f.ended;
                    if (f.ended) { dragId = int.MinValue; continue; }
                    target.position = Clamp(f.world + grabOffset);
                    continue;
                }
                if (f.began)
                {
                    if (dragId == int.MinValue && targetArea.OverlapPoint(f.world))
                    {
                        dragId = f.id;
                        holding = true;
                        grabOffset = (Vector2)target.position - f.world;
                        Sfx.Play(this, "tick", 0.6f);
                        continue;
                    }
                    Chase(f.world);
                    if (busy) return;
                    c = cursor.position;
                }
                if (f.ended) continue;
                // scappa dal dito che lo insegue
                Vector2 d = c - f.world;
                if (d.magnitude < 1.8f) flee += d.normalized * (1.8f - d.magnitude);
            }

            if (flee.sqrMagnitude > 0.0001f) c += flee.normalized * 6f * dt;
            else
            {
                if (clock > nextWander || Vector2.Distance(c, wander) < 0.1f)
                {
                    wander = new Vector2(Random.Range(boundsMin.x, boundsMax.x), Random.Range(boundsMin.y, boundsMax.y));
                    nextWander = clock + Random.Range(2f, 4f);
                }
                c = Vector2.MoveTowards(c, wander, 0.7f * dt);
            }
            c = Clamp(c);
            cursor.position = new Vector3(c.x, c.y, cursor.position.z);
            cursor.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(clock * 6f) * 5f);

            // la punta del cursore sul bersaglio = clic!
            if (Vector2.Distance(c, target.position) < 0.45f) StartCoroutine(Clicked());
            else if (!holding && dragId != int.MinValue) dragId = int.MinValue;
        }

        void Chase(Vector2 finger)
        {
            chases++;
            triesText.text = "CHASES: " + Marks(chases, MaxChases);
            Sfx.Play(this, "whoosh", 0.6f);
            if (chases >= MaxChases) { StartCoroutine(Escape()); return; }
            // scatto verso il punto più lontano dal dito
            Vector2 best = cursor.position;
            float far = -1f;
            for (int i = 0; i < 8; i++)
            {
                Vector2 p = new Vector2(Random.Range(boundsMin.x, boundsMax.x), Random.Range(boundsMin.y, boundsMax.y));
                float d = Vector2.Distance(p, finger);
                if (d > far) { far = d; best = p; }
            }
            wander = best;
            nextWander = clock + 3f;
            cursor.position = Vector3.Lerp(cursor.position, best, 0.5f);
            string[] lines = { "Too slow!", "Nope!", "Catch me if you can!", "Hehe!" };
            Say(lines[(chases - 1) % lines.Length], 1.2f);
        }

        IEnumerator Escape()
        {
            busy = true;
            Say("Bye bye! I'm logging off!", 0f);
            Vector3 p = cursor.position;
            for (float k = 0f; k < 1.2f; k += Time.deltaTime)
            {
                p.x += 6f * Time.deltaTime;
                cursor.position = p;
                yield return null;
            }
            Haptics.Light();
            yield return new WaitForSeconds(0.6f);
            Fail();
        }

        IEnumerator Clicked()
        {
            busy = true;
            Sfx.Play(this, "clunk", 0.9f);
            Haptics.Light();
            Vector3 s = cursor.localScale;
            cursor.localScale = s * 0.8f;
            yield return new WaitForSeconds(0.12f);
            cursor.localScale = s;
            StartCoroutine(Pop(target, 1.4f, 0.35f));
            Say("CLICK! Hey... no fair!", 0f);
            yield return StartCoroutine(Celebrate(1.6f));
        }
    }
}
