using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "Hot potato" — una patata bollente passa tra quattro amici, sempre più veloce. Tocca chi sta per
    /// riceverla proprio quando sopra la sua testa si accende la lucina verde: così la prende al volo. Servono 5 passaggi;
    /// due errori di tempo (troppo presto, persona sbagliata o niente tocco) e la patata esplode in purè.
    /// </summary>
    public class HotPotatoLevel : LevelController
    {
        [SerializeField] Transform[] buddies;          // piedi di ogni amico (con Collider2D)
        [SerializeField] SpriteRenderer[] signals;     // lucine sopra le teste
        [SerializeField] Transform potato, mash;
        [SerializeField] TextMesh counter;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] SmokeEmitter[] confetti;
        [SerializeField] int passesNeeded = 5, mistakesAllowed = 2;
        [SerializeField] float handHeight = 1.25f;
        [SerializeField] float[] flightTimes = { 1.45f, 1.3f, 1.15f, 1.0f, 0.9f, 0.85f };

        static readonly Color Off = new Color(0.45f, 0.45f, 0.5f), Green = new Color(0.3f, 1f, 0.35f), Red = new Color(1f, 0.3f, 0.3f);

        Camera cam;
        Collider2D[] areas;
        int passes, mistakes, holder;
        Vector3[] homes;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            areas = new Collider2D[buddies.Length];
            homes = new Vector3[buddies.Length];
            for (int i = 0; i < buddies.Length; i++) { areas[i] = buddies[i].GetComponent<Collider2D>(); homes[i] = buddies[i].localPosition; }
            foreach (SpriteRenderer s in signals) s.color = Off;
            if (mash != null) mash.gameObject.SetActive(false);
            S41DKit.Confetti(confetti, false);
            UpdateCounter();
            StartCoroutine(Game());
        }

        Vector3 Hand(int i) => buddies[i].position + Vector3.up * handHeight;

        void UpdateCounter()
        {
            if (counter != null) counter.text = "CATCHES " + passes + "/" + passesNeeded + "     MISSES " + mistakes + "/" + mistakesAllowed;
        }

        void Say(string text) { if (bubble != null) bubble.Show(text); }

        int TappedBuddy()
        {
            if (!PointerInput.AnyTouchBegan()) return -2;
            if (!PointerInput.TryGetPrimary(out Vector2 sp, out _, out _)) return -2;
            Vector2 w = PointerInput.ToWorld(cam, sp);
            for (int i = 0; i < areas.Length; i++) if (areas[i] != null && areas[i].OverlapPoint(w)) return i;
            return -1;
        }

        IEnumerator Game()
        {
            potato.position = Hand(0);
            Say("HOT HOT HOT! Help us pass it!");
            yield return new WaitForSeconds(1.6f);
            if (bubble != null) bubble.Hide();
            int round = 0;
            while (passes < passesNeeded)
            {
                // chi la tiene saltella scottandosi
                for (float k = 0f; k < 0.55f; k += Time.deltaTime)
                {
                    potato.position = Hand(holder) + new Vector3(Mathf.Sin(k * 40f) * 0.06f, Mathf.Abs(Mathf.Sin(k * 20f)) * 0.15f, 0f);
                    yield return null;
                }
                int target = (holder + Random.Range(1, buddies.Length)) % buddies.Length;
                float d = flightTimes[Mathf.Min(round, flightTimes.Length - 1)];
                float window = Mathf.Max(0.38f, 0.55f - round * 0.04f);
                const float grace = 0.15f;
                Vector3 from = Hand(holder), to = Hand(target);
                Sfx.Play(this, "whoosh", 0.5f);
                int result = 0;   // 0 in attesa, 1 preso, -1 errore
                for (float t = 0f; t < d + grace; t += Time.deltaTime)
                {
                    float k = Mathf.Min(1f, t / d);
                    potato.position = Vector3.Lerp(from, to, k) + Vector3.up * Mathf.Sin(k * Mathf.PI) * 1.3f;
                    potato.localRotation = Quaternion.Euler(0, 0, t * 540f);
                    bool green = t >= d - window;
                    signals[target].color = green ? Green : Off;
                    if (result == 0)
                    {
                        int tap = TappedBuddy();
                        if (tap == target && green) result = 1;
                        else if (tap >= 0) { result = -1; signals[tap].color = Red; Sfx.Play(this, "buzz", 0.5f); }
                    }
                    if (result == 1 && t >= d) break;
                    yield return null;
                }
                foreach (SpriteRenderer s in signals) s.color = Off;
                potato.position = to;
                potato.localRotation = Quaternion.identity;
                holder = target;
                round++;
                if (result == 1)
                {
                    passes++;
                    UpdateCounter();
                    Sfx.Play(this, "pop", 0.8f);
                    Haptics.Light();
                    StartCoroutine(S41DKit.Pop(buddies[target], 1.15f, 0.2f));
                    continue;
                }
                // mancata: si scotta
                mistakes++;
                UpdateCounter();
                Haptics.Medium();
                Sfx.Play(this, "bonk", 0.8f);
                Say(mistakes >= mistakesAllowed ? "IT'S GONNA BLOW!" : "OUCH! HOT HOT HOT!");
                yield return StartCoroutine(Ouch(target));
                if (mistakes >= mistakesAllowed) { yield return StartCoroutine(Explode()); yield break; }
                yield return new WaitForSeconds(0.5f);
                if (bubble != null) bubble.Hide();
            }
            yield return StartCoroutine(Celebrate());
        }

        IEnumerator Ouch(int i)
        {
            Transform b = buddies[i];
            for (float k = 0f; k < 0.6f; k += Time.deltaTime)
            {
                b.localPosition = homes[i] + Vector3.up * Mathf.Abs(Mathf.Sin(k * 16f)) * 0.3f;
                potato.position = Hand(i) + Vector3.up * Mathf.Abs(Mathf.Sin(k * 16f)) * 0.5f;
                yield return null;
            }
            b.localPosition = homes[i];
        }

        IEnumerator Explode()
        {
            SpriteRenderer sr = potato.GetComponentInChildren<SpriteRenderer>();
            Vector3 s = potato.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.9f)
            {
                potato.localScale = s * (1f + k * 1.3f);
                potato.localPosition += (Vector3)(Random.insideUnitCircle * 0.03f);
                if (sr != null) sr.color = Color.Lerp(Color.white, new Color(1f, 0.35f, 0.25f), Mathf.PingPong(k * 6f, 1f));
                yield return null;
            }
            potato.gameObject.SetActive(false);
            Sfx.Play(this, "clunk", 1f);
            Sfx.Play(this, "pop", 1f);
            Haptics.Heavy();
            if (mash != null)
            {
                mash.gameObject.SetActive(true);
                mash.position = new Vector3(potato.position.x, potato.position.y, 0f);
                yield return StartCoroutine(S41DKit.Pop(mash, 1.4f, 0.3f));
            }
            Say("...Anyone want mashed potatoes?");
            yield return new WaitForSeconds(1.8f);
            Fail();
        }

        IEnumerator Celebrate()
        {
            SpriteRenderer sr = potato.GetComponentInChildren<SpriteRenderer>();
            if (sr != null) sr.color = new Color(1f, 0.95f, 0.75f);
            Say("It cooled down! Perfect baked potato!");
            Sfx.Play(this, "ding", 0.8f);
            S41DKit.Confetti(confetti, true);
            for (float k = 0f; k < 1.8f; k += Time.deltaTime)
            {
                for (int i = 0; i < buddies.Length; i++)
                    buddies[i].localPosition = homes[i] + Vector3.up * Mathf.Abs(Mathf.Sin(k * 9f + i)) * 0.3f;
                potato.position = Hand(holder) + Vector3.up * Mathf.Abs(Mathf.Sin(k * 9f + holder)) * 0.3f;
                yield return null;
            }
            S41DKit.Confetti(confetti, false);
            Win();
        }
    }
}
