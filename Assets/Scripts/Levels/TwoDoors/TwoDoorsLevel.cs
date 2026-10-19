using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 74 — due porte. Una rossa con la serratura tonda, una blu con la serratura quadrata. Le chiavi sono
    /// nascoste: quella rossa (anello tondo) è "dipinta" nel pavimento a rombi, quella blu (anello quadrato) è un pezzo
    /// del recinto nel quadro; toccandole si staccano. Dietro una porta c'è una ragazza bellissima, dietro l'altra un
    /// mostro (a caso, ma sempre uno per porta). Entrambi bussano: il mostro più forte (la porta trema di più, cade
    /// polvere, il colpo è più cupo). Si apre la porta giusta: vinto; quella del mostro: fallito.
    /// </summary>
    public class TwoDoorsLevel : LevelController
    {
        [SerializeField] DropSlot[] keyholes;          // 0 rossa, 1 blu
        [SerializeField] Transform[] panels;           // ante (pivot sui cardini)
        [SerializeField] Transform lady, monster;
        [SerializeField] Draggable2D[] keys;           // 0 rossa, 1 blu
        [SerializeField] GameObject floorCamo;
        [SerializeField] SpriteRenderer painting;
        [SerializeField] Sprite paintingEmpty;
        [SerializeField] SmokeEmitter dust;
        [SerializeField] SmokeEmitter hearts;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] Vector2 knockEvery = new Vector2(2.2f, 3.6f);

        int ladyDoor;
        bool done;
        AudioSource sfx;
        AudioClip softKnock, hardKnock;
        Vector3[] panelHome;

        public override void OnLevelStart()
        {
            sfx = gameObject.AddComponent<AudioSource>();
            softKnock = Knock(1f);
            hardKnock = Knock(0.45f);
            ladyDoor = Random.Range(0, 2);
            Transform[] behind = { null, null };
            behind[ladyDoor] = lady;
            behind[1 - ladyDoor] = monster;
            for (int i = 0; i < 2; i++)
            {
                behind[i].position = new Vector3(keyholes[i].transform.parent.parent.position.x, behind[i].position.y, behind[i].position.z);
                behind[i].gameObject.SetActive(false);
            }
            panelHome = new Vector3[panels.Length];
            for (int i = 0; i < panels.Length; i++) panelHome[i] = panels[i].localPosition;
            foreach (DropSlot k in keyholes) k.Filled += OnKey;
            foreach (Draggable2D k in keys)
            {
                foreach (SpriteRenderer r in k.GetComponentsInChildren<SpriteRenderer>(true)) r.enabled = false;   // nascoste finché non le tocchi
                k.DragStarted += Reveal;
            }
            if (dust != null) dust.Stop();
            if (hearts != null) hearts.Stop();
            StartCoroutine(Knocking());
            if (bubble != null) { bubble.Show("Two doors... which one?"); StartCoroutine(HideLater(2f)); }
        }

        void OnDestroy()
        {
            foreach (AudioClip c in new[] { softKnock, hardKnock }) if (c != null) Destroy(c);
            if (keyholes != null) foreach (DropSlot k in keyholes) if (k != null) k.Filled -= OnKey;
        }

        void Reveal(Draggable2D key)
        {
            foreach (SpriteRenderer r in key.GetComponentsInChildren<SpriteRenderer>(true)) r.enabled = true;
            key.transform.localRotation = Quaternion.identity;
            key.transform.localScale = Vector3.one * 0.5f;   // staccata dal quadro/pavimento torna a grandezza normale
            if (key == keys[0] && floorCamo != null) floorCamo.SetActive(false);
            if (key == keys[1] && painting != null && paintingEmpty != null) painting.sprite = paintingEmpty;
        }

        IEnumerator Knocking()
        {
            int door = Random.Range(0, 2);
            yield return new WaitForSeconds(1.2f);
            while (!done)
            {
                bool monsterHere = door != ladyDoor;
                yield return KnockOn(door, monsterHere);
                door = 1 - door;
                yield return new WaitForSeconds(Random.Range(knockEvery.x, knockEvery.y));
            }
        }

        /// <summary>Colpi alla porta: la ragazza due colpetti leggeri, il mostro tre colpi forti con polvere.</summary>
        IEnumerator KnockOn(int door, bool strong)
        {
            int count = strong ? 3 : 2;
            float amp = strong ? 0.07f : 0.03f;
            for (int i = 0; i < count && !done; i++)
            {
                sfx.PlayOneShot(strong ? hardKnock : softKnock, strong ? 1f : 0.6f);
                if (strong) Haptics.Light();
                if (strong && dust != null) { dust.transform.position = panels[door].position + new Vector3(0.66f, 2.55f, 0f); dust.Play(); }
                for (float k = 0f; k < 0.16f; k += Time.deltaTime)
                {
                    float e = 1f - k / 0.16f;
                    panels[door].localPosition = panelHome[door] + new Vector3(Mathf.Sin(k * 120f) * amp * e, 0f, 0f);
                    panels[door].localRotation = Quaternion.Euler(0, 0, strong ? Mathf.Sin(k * 90f) * 1.5f * e : 0f);
                    yield return null;
                }
                panels[door].localPosition = panelHome[door];
                panels[door].localRotation = Quaternion.identity;
                if (dust != null) dust.Stop();
                yield return new WaitForSeconds(strong ? 0.22f : 0.18f);
            }
        }

        void OnKey(DropSlot slot, Draggable2D key)
        {
            if (done) return;
            done = true;
            int door = System.Array.IndexOf(keyholes, slot);
            StartCoroutine(Open(door));
        }

        IEnumerator Open(int door)
        {
            yield return new WaitForSeconds(0.3f);
            Transform who = door == ladyDoor ? lady : monster;
            who.gameObject.SetActive(true);
            Transform panel = panels[door];
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.5f)
            {
                panel.localScale = new Vector3(Mathf.Lerp(1f, 0.12f, k), 1f, 1f);   // l'anta ruota sui cardini
                yield return null;
            }
            if (door == ladyDoor)
            {
                if (hearts != null) { hearts.transform.position = who.position + Vector3.up * 2f; hearts.Play(); }
                if (bubble != null) bubble.Show("My hero! <3");
                yield return new WaitForSeconds(1.8f);
                if (hearts != null) hearts.Stop();
                Win();
            }
            else
            {
                sfx.PlayOneShot(hardKnock, 1f);
                Haptics.Medium();
                if (bubble != null) bubble.Show("RAAAAAWR!!!");
                Vector3 s = who.localScale, home = transform.localPosition;
                for (float k = 0f; k < 0.8f; k += Time.deltaTime)
                {
                    who.localScale = s * (1f + Mathf.Sin(Mathf.Min(1f, k / 0.3f) * Mathf.PI * 0.5f) * 0.25f);   // si avventa
                    transform.localPosition = home + (Vector3)(Random.insideUnitCircle * 0.08f);
                    yield return null;
                }
                transform.localPosition = home;
                yield return new WaitForSeconds(0.8f);
                Fail();
            }
        }

        /// <summary>Colpo alla porta sintetizzato: tonfo legnoso (pitch 1 = leggero, più basso = colpo da mostro).</summary>
        static AudioClip Knock(float pitch)
        {
            const int rate = 22050;
            int n = rate / 4;
            var data = new float[n];
            var rnd = new System.Random(9);
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)rate;
                float env = Mathf.Exp(-t * 40f * pitch);
                float body = Mathf.Sin(t * 2f * Mathf.PI * 180f * pitch) + 0.5f * Mathf.Sin(t * 2f * Mathf.PI * 420f * pitch);
                float click = ((float)rnd.NextDouble() * 2f - 1f) * Mathf.Exp(-t * 300f);
                data[i] = Mathf.Clamp((body * 0.6f + click * 0.5f) * env * (pitch < 0.8f ? 1.2f : 0.7f), -1f, 1f);
            }
            AudioClip clip = AudioClip.Create("knock", n, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }

        IEnumerator HideLater(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (bubble != null && !done) bubble.Hide();
        }
    }
}
