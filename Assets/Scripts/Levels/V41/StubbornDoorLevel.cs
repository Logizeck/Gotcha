using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "The stubborn door" — un portone con tre lucchetti e una sola chiave. La chiave si trascina in un
    /// lucchetto e si tocca per girarla: il lucchetto si apre ma la chiave resta incastrata (apre un solo lucchetto).
    /// Tirarla via è "forzarla": tre volte e si spezza (sconfitta). Trucco: si trascinano gli ALTRI due lucchetti sulla
    /// chiave inserita, che li apre lo stesso. Con tre lucchetti aperti il portone si spalanca e il topo esce.
    /// </summary>
    public class StubbornDoorLevel : LevelController
    {
        [SerializeField] Transform key;                 // punta della chiave nell'origine, rivolta a destra
        [SerializeField] SpriteRenderer[] cracks;       // crepe sulla chiave (figlie)
        [SerializeField] Transform[] locks;             // centro del corpo; buco della serratura in (0,-0.05)
        [SerializeField] SpriteRenderer[] lockArts;
        [SerializeField] Sprite openSprite;
        [SerializeField] Transform hinge;               // cerniera del portone (si schiaccia in x per aprirlo)
        [SerializeField] SpriteRenderer doorLight;
        [SerializeField] Transform mouse;
        [SerializeField] TextMesh goal;
        [SerializeField] SpriteRenderer[] hearts;
        [SerializeField] SpeechBubble bubble;

        Camera cam;
        Vector3 keyHome;
        Vector3[] lockHomes;
        bool[] open;
        int keyIn = -1, forced;
        bool turned, busy;
        int grabbed = -1;        // -1 niente, 0..2 lucchetto, 9 chiave
        Vector2 grabStart, grabOffset;
        bool moved, forcedThisDrag;
        float bubbleUntil;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            keyHome = key.position;
            lockHomes = new Vector3[locks.Length];
            for (int i = 0; i < locks.Length; i++) lockHomes[i] = locks[i].position;
            open = new bool[locks.Length];
            foreach (SpriteRenderer c in cracks) if (c != null) c.enabled = false;
            if (doorLight != null) doorLight.enabled = false;
            Kit41C.Hearts(hearts, 3);
            Kit41C.Goal(goal, "OPEN THE DOOR");
            Say("Three locks... and one key?", 2.2f);
        }

        void Say(string text, float seconds)
        {
            if (bubble == null) return;
            bubble.Show(text);
            bubbleUntil = Time.time + seconds;
        }

        Vector2 Keyhole(int i) => (Vector2)locks[i].position + new Vector2(0f, -0.05f);

        void Update()
        {
            if (IsFinished) return;
            if (bubble != null && bubbleUntil > 0f && Time.time > bubbleUntil) { bubble.Hide(); bubbleUntil = 0f; }
            if (busy) { grabbed = -1; return; }
            if (!Kit41C.Pointer(cam, out Vector2 w, out bool began, out bool ended)) { if (grabbed != -1) Release(); return; }
            if (began) { Grab(w); return; }
            if (grabbed == -1) return;
            if ((w - grabStart).magnitude > 0.2f) moved = true;
            if (grabbed == 9)
            {
                if (keyIn >= 0 && turned)
                {
                    // la chiave è incastrata: si può solo forzarla
                    if (moved && !forcedThisDrag) { forcedThisDrag = true; StartCoroutine(Force()); }
                }
                else if (keyIn < 0) key.position = Clamp(w + grabOffset);
            }
            else if (moved) locks[grabbed].position = Clamp(w + grabOffset);
            if (ended) Release();
        }

        static Vector3 Clamp(Vector2 p) => new Vector3(Mathf.Clamp(p.x, -2.5f, 2.6f), Mathf.Clamp(p.y, -4.3f, 2.3f), 0f);

        void Grab(Vector2 w)
        {
            grabbed = -1;
            moved = false;
            forcedThisDrag = false;
            grabStart = w;
            // i lucchetti stanno sopra la chiave
            float best = 0.5f;
            for (int i = 0; i < locks.Length; i++)
            {
                if (open[i]) continue;
                float d = Vector2.Distance(w, locks[i].position);
                if (d < best) { best = d; grabbed = i; }
            }
            if (grabbed >= 0)
            {
                grabOffset = (Vector2)locks[grabbed].position - w;
                lockArts[grabbed].sortingOrder = 14;
                return;
            }
            if (Vector2.Distance(w, (Vector2)key.position + new Vector2(-0.6f, 0f)) < 0.8f)
            {
                grabbed = 9;
                grabOffset = (Vector2)key.position - w;
                if (keyIn < 0) Sfx.Play(this, "tick", 0.5f);
            }
        }

        void Release()
        {
            int g = grabbed;
            grabbed = -1;
            if (g == 9)
            {
                if (keyIn >= 0)
                {
                    if (!moved && !turned) StartCoroutine(Turn());
                    else if (!moved && turned) Say("It's stuck in there now.", 1.6f);
                    return;
                }
                if (!moved) { StartCoroutine(Kit41C.Pop(key, 0.1f)); return; }
                int best = -1;
                float bd = 0.6f;
                for (int i = 0; i < locks.Length; i++)
                {
                    float d = Vector2.Distance(key.position, Keyhole(i));
                    if (d < bd) { bd = d; best = i; }
                }
                if (best >= 0) Insert(best);
                else StartCoroutine(MoveWorld(key, keyHome, 0.25f));
                return;
            }
            if (g < 0) return;
            lockArts[g].sortingOrder = 8;
            if (!moved) { StartCoroutine(Kit41C.Shake(locks[g], 0.05f)); Sfx.Play(this, "tick", 0.4f); return; }
            if (keyIn >= 0 && turned && g != keyIn && Vector2.Distance(Keyhole(g), key.position) < 0.65f)
            {
                StartCoroutine(OpenWithKey(g));
                return;
            }
            if (keyIn >= 0 && !turned && g != keyIn && Vector2.Distance(Keyhole(g), key.position) < 0.65f) Say("Turn the key first!", 1.5f);
            StartCoroutine(MoveWorld(locks[g], lockHomes[g], 0.25f));
        }

        IEnumerator MoveWorld(Transform t, Vector3 to, float seconds)
        {
            Vector3 from = t.position;
            for (float k = 0f; k < 1f; k += Time.deltaTime / seconds)
            {
                t.position = Vector3.Lerp(from, to, Mathf.SmoothStep(0f, 1f, k));
                yield return null;
            }
            t.position = to;
        }

        void Insert(int i)
        {
            keyIn = i;
            key.position = Keyhole(i);
            Sfx.Play(this, "clunk", 0.6f);
            Haptics.Light();
            Kit41C.Goal(goal, "TAP THE KEY TO TURN IT");
        }

        IEnumerator Turn()
        {
            busy = true;
            Vector3 s = key.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.35f)
            {
                key.localScale = new Vector3(s.x, s.y * Mathf.Cos(k * Mathf.PI), s.z);
                yield return null;
            }
            key.localScale = new Vector3(s.x, -s.y, s.z);
            turned = true;
            yield return Unlock(keyIn, false);
            Kit41C.Goal(goal, "TWO MORE LOCKS...");
            Say("Click! One down, two to go.", 2f);
            busy = false;
        }

        IEnumerator Unlock(int i, bool drop)
        {
            open[i] = true;
            lockArts[i].sprite = openSprite;
            Sfx.Play(this, "clunk", 0.7f);
            Sfx.Play(this, "ding", 0.4f);
            yield return Kit41C.Pop(locks[i], 0.2f);
            if (!drop) yield break;
            // il lucchetto aperto cade a terra
            Vector3 from = locks[i].position;
            Vector3 to = new Vector3(from.x + 0.3f, -3.85f, 0f);
            float v = 0f;
            Vector3 p = from;
            while (p.y > to.y)
            {
                v += 18f * Time.deltaTime;
                p.y -= v * Time.deltaTime;
                p.x = Mathf.MoveTowards(p.x, to.x, Time.deltaTime);
                locks[i].position = p;
                locks[i].Rotate(0f, 0f, -200f * Time.deltaTime);
                yield return null;
            }
            locks[i].position = new Vector3(p.x, to.y, 0f);
            Sfx.Play(this, "bonk", 0.7f);
            Haptics.Light();
        }

        IEnumerator Force()
        {
            busy = true;
            forced++;
            Kit41C.Hearts(hearts, 3 - forced);
            Sfx.Play(this, "clunk", 0.8f);
            Haptics.Medium();
            for (int i = 0; i < forced && i < cracks.Length; i++) if (cracks[i] != null) cracks[i].enabled = true;
            Vector3 p = key.localPosition;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.4f)
            {
                key.localPosition = p + new Vector3(-Mathf.Abs(Mathf.Sin(k * 30f)) * 0.08f, 0f, 0f);
                key.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(k * 40f) * 6f * (1f - k));
                yield return null;
            }
            key.localPosition = p;
            key.localRotation = Quaternion.identity;
            if (forced >= 3) { yield return Snap(); yield break; }
            Say(forced == 1 ? "CREAK! It only opens one lock... and it's stuck!" : "CRACK! Careful, it's breaking!", 2f);
            busy = false;
        }

        IEnumerator Snap()
        {
            Kit41C.Goal(goal, "THE KEY BROKE!");
            Sfx.Play(this, "bonk", 1f);
            Haptics.Heavy();
            Say("SNAP! Now we're locked in forever.", 2f);
            Vector3 from = key.position;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.6f)
            {
                key.position = from + new Vector3(-k * 0.5f, -k * k * 3f, 0f);
                key.localRotation = Quaternion.Euler(0f, 0f, k * 160f);
                yield return null;
            }
            yield return new WaitForSeconds(1.0f);
            Fail();
        }

        IEnumerator OpenWithKey(int g)
        {
            busy = true;
            // il lucchetto si infila sulla punta della chiave
            yield return MoveWorld(locks[g], key.position + new Vector3(0.05f, 0.05f, 0f), 0.15f);
            Sfx.Play(this, "tick", 0.6f);
            yield return new WaitForSeconds(0.15f);
            yield return Unlock(g, true);
            int left = 0;
            foreach (bool o in open) if (!o) left++;
            if (left > 0)
            {
                Kit41C.Goal(goal, left == 1 ? "ONE MORE LOCK!" : "TWO MORE LOCKS...");
                Say("The lock came to the key!", 1.8f);
                busy = false;
                yield break;
            }
            yield return Finale();
        }

        IEnumerator Finale()
        {
            Kit41C.Goal(goal, "THE DOOR IS OPEN!");
            // cade anche il primo lucchetto, con la chiave dentro
            key.SetParent(locks[keyIn], true);
            yield return Unlock(keyIn, true);
            if (doorLight != null) doorLight.enabled = true;
            Sfx.Play(this, "whoosh", 0.7f);
            Vector3 s = hinge.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.6f)
            {
                hinge.localScale = new Vector3(s.x * Mathf.Lerp(1f, 0.12f, Mathf.SmoothStep(0f, 1f, k)), s.y, s.z);
                yield return null;
            }
            Sfx.Play(this, "ding", 0.8f);
            if (mouse != null)
            {
                yield return MoveWorld(mouse, new Vector3(0.2f, -3.3f, 0f), 0.7f);
                Vector3 ms = mouse.localScale;
                for (float k = 0f; k < 1f; k += Time.deltaTime / 0.4f) { mouse.localScale = ms * (1f - k * 0.6f); yield return null; }
                mouse.gameObject.SetActive(false);
            }
            yield return new WaitForSeconds(0.6f);
            Win();
        }
    }
}
