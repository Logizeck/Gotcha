using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 125 — "SWEET DREAMS". Toni dorme sul cuscino e sogna un osso (si vede nella nuvoletta). La cameretta è
    /// piena di cose che si possono spostare (calzino, pallina, ciabatta, orsetto, sveglia, campanello, piuma...), e
    /// ognuna messa sotto il naso di Toni gli strappa solo una reazione nel sonno. La soluzione: prendere l'osso DAL
    /// SOGNO (si trascina fuori dalla nuvoletta e diventa vero) e metterlo davanti al naso di Toni, che lo annusa, si
    /// sveglia e se lo mangia.
    /// </summary>
    public class ToniDreamLevel : LevelController
    {
        [SerializeField] ToniPug toni;
        [SerializeField] Transform cloud;               // la nuvoletta del sogno
        [SerializeField] Draggable2D dreamBone;         // l'osso sognato (trascinabile)
        [SerializeField] SpriteRenderer dreamBoneArt;
        [SerializeField] Draggable2D[] props;           // tutto il resto della stanza
        [SerializeField] string[] reactions;            // cosa borbotta Toni per ogni oggetto (stesso ordine di props)
        [SerializeField] SmokeEmitter sparkle;
        [SerializeField] float noseRange = 1.1f;
        [SerializeField] float cloudRadius = 1.4f;

        bool real, busy;
        Vector3 cloudScale, boneHome;
        float nextMumble;

        public override void OnLevelStart()
        {
            toni.Stand(false);
            toni.Face(false);
            toni.Sleep();
            cloudScale = cloud.localScale;
            boneHome = dreamBone.transform.localPosition;
            dreamBoneArt.color = new Color(0.75f, 0.85f, 1f, 0.75f);   // un osso "sognato": azzurrino e trasparente
            dreamBone.returnOnMiss = false;
            dreamBone.DragStarted += OnBoneGrabbed;
            dreamBone.Dropped += OnBoneDropped;
            foreach (Draggable2D d in props) { d.returnOnMiss = false; d.Dropped += OnPropDropped; }
            if (sparkle != null) sparkle.Stop();
            nextMumble = Time.time + 4f;
        }

        void OnDestroy()
        {
            if (dreamBone != null) { dreamBone.DragStarted -= OnBoneGrabbed; dreamBone.Dropped -= OnBoneDropped; }
            if (props != null) foreach (Draggable2D d in props) if (d != null) d.Dropped -= OnPropDropped;
        }

        void Update()
        {
            if (IsFinished) return;
            // la nuvoletta respira, l'osso sognato galleggia
            float t = Time.time;
            if (!busy) cloud.localScale = cloudScale * (1f + Mathf.Sin(t * 1.6f) * 0.025f);
            if (!real && Draggable2D.Active != dreamBone)
                dreamBone.transform.localPosition = boneHome + new Vector3(0f, Mathf.Sin(t * 2.2f) * 0.08f, 0f);
            if (!busy && t >= nextMumble)
            {
                nextMumble = t + Random.Range(5f, 8f);
                string[] lines = { "*mumble* ...bone...", "*slurp*", "Zzz... mmh, bone...", "*happy snore*" };
                toni.Say(lines[Random.Range(0, lines.Length)], 1.4f, "snore");
            }
        }

        bool NearNose(Transform item) => Vector2.Distance(item.position, toni.NoseWorld) <= noseRange;

        void OnBoneGrabbed(Draggable2D d)
        {
            if (real) return;
            StartCoroutine(BecomeReal());
        }

        /// <summary>Uscendo dalla nuvoletta l'osso diventa vero: si colora, luccica e il sogno si sgonfia un po'.</summary>
        IEnumerator BecomeReal()
        {
            while (!real && Draggable2D.Active == dreamBone)
            {
                float out01 = Mathf.Clamp01((Vector2.Distance(dreamBone.transform.position, cloud.position) - cloudRadius * 0.4f) / (cloudRadius * 0.8f));
                dreamBoneArt.color = Color.Lerp(new Color(0.75f, 0.85f, 1f, 0.75f), Color.white, out01);
                if (out01 >= 1f)
                {
                    real = true;
                    dreamBoneArt.color = Color.white;
                    Sfx.Play(this, "ding", 0.6f);
                    Haptics.Light();
                    if (sparkle != null) StartCoroutine(SparkleAt(dreamBone.transform.position));
                    toni.Say("*sniff*... ?", 1.2f);
                    StartCoroutine(Deflate());
                }
                yield return null;
            }
            if (!real) dreamBoneArt.color = new Color(0.75f, 0.85f, 1f, 0.75f);
        }

        IEnumerator SparkleAt(Vector3 at)
        {
            sparkle.transform.position = at;
            sparkle.Play();
            yield return new WaitForSeconds(0.4f);
            sparkle.Stop();
        }

        IEnumerator Deflate()
        {
            busy = true;
            Vector3 from = cloud.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.4f)
            {
                cloud.localScale = Vector3.Lerp(from, cloudScale * 0.8f, k);
                yield return null;
            }
            busy = false;
            cloudScale *= 0.8f;
        }

        void OnBoneDropped(Draggable2D d, DropSlot slot)
        {
            if (IsFinished) return;
            if (!real)
            {
                // lasciato dentro il sogno: torna a galleggiare al suo posto
                dreamBone.transform.localPosition = boneHome;
                return;
            }
            if (NearNose(dreamBone.transform)) StartCoroutine(WakeAndEat());
            else toni.Say("*sniff sniff*... Zzz", 1.2f, "snore");
        }

        void OnPropDropped(Draggable2D d, DropSlot slot)
        {
            if (IsFinished || busy) return;
            Haptics.Light();
            int i = System.Array.IndexOf(props, d);
            if (!NearNose(d.transform)) return;
            string line = reactions != null && i >= 0 && i < reactions.Length ? reactions[i] : "Zzz...";
            toni.Say(line, 1.6f, "snore");
            StartCoroutine(Twitch());
        }

        IEnumerator Twitch()
        {
            Transform tr = toni.transform;
            Vector3 s = tr.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.3f)
            {
                tr.localScale = new Vector3(s.x * (1f + Mathf.Sin(k * Mathf.PI * 3f) * 0.03f), s.y, s.z);
                yield return null;
            }
            tr.localScale = s;
        }

        IEnumerator WakeAndEat()
        {
            busy = true;
            dreamBone.enabled = false;
            toni.Say("*SNIFF SNIFF*", 0.9f, "snore");
            yield return new WaitForSeconds(0.9f);
            toni.WakeUp(1);
            yield return new WaitForSeconds(0.4f);
            toni.WakeUp(2);
            toni.Alert(true);
            toni.LookTarget = dreamBone.transform;
            toni.Say("BONE?! IT'S REAL!", 1.2f, "woof");
            Haptics.Medium();
            // la nuvoletta svanisce: il sogno si è avverato
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.4f)
            {
                cloud.localScale = cloudScale * (1f - k);
                yield return null;
            }
            cloud.gameObject.SetActive(false);
            yield return new WaitForSeconds(0.5f);
            toni.Alert(false);
            yield return StartCoroutine(toni.Chomp(dreamBone.transform));
            yield return StartCoroutine(toni.Happy(1.6f));
            toni.Say("Best dream EVER!", 1.5f, "woof");
            yield return new WaitForSeconds(1.2f);
            Win();
        }
    }
}
