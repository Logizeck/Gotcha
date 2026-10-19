using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 60 — la torcia. Stanza buia: il dito è una torcia (si vede solo dove tocchi). Bisogna trovare
    /// l'interruttore e premerlo (toccarlo di nuovo mentre è illuminato). Ma dal soffitto pendono pipistrelli
    /// addormentati: se la luce resta su uno di loro, si sveglia e strilla. Si cerca piano, a colpi di luce brevi.
    /// </summary>
    public class FlashlightLevel : LevelController
    {
        [SerializeField] Transform torch;              // sprite "darkness" col buco: segue il dito
        [SerializeField] SpriteRenderer blackout;      // buio totale quando non tocchi
        [SerializeField] Transform[] bats;             // figli "Sleep" e "Awake"
        [SerializeField] Transform[] spots;            // posti possibili per pipistrelli e interruttore
        [SerializeField] Collider2D lightSwitch;
        [SerializeField] SpriteRenderer switchArt;
        [SerializeField] Sprite switchOn;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] float lightRadius = 0.85f;
        [SerializeField] float wakeAfter = 0.7f;

        float[] lit;
        float switchLitAt = -10f;
        bool done;
        Camera cam;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            // a caso: dove stanno l'interruttore e i pipistrelli
            var order = new System.Collections.Generic.List<int>();
            for (int i = 0; i < spots.Length; i++) order.Add(i);
            for (int i = order.Count - 1; i > 0; i--) { int j = Random.Range(0, i + 1); (order[i], order[j]) = (order[j], order[i]); }
            lightSwitch.transform.position = spots[order[0]].position;
            for (int i = 0; i < bats.Length; i++) bats[i].position = spots[order[1 + i]].position + new Vector3(0f, 0.35f, 0f);
            lit = new float[bats.Length];
            foreach (Transform b in bats) SetBat(b, false);
            torch.gameObject.SetActive(false);
            blackout.color = new Color(0.01f, 0.01f, 0.03f, 1f);
            if (bubble != null) { bubble.Show("Who turned off the lights?"); StartCoroutine(HideLater(2f)); }
        }

        static void SetBat(Transform b, bool awake)
        {
            b.Find("Sleep").gameObject.SetActive(!awake);
            b.Find("Awake").gameObject.SetActive(awake);
        }

        void Update()
        {
            if (done || IsFinished) return;
            bool on = PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out _) && !PointerInput.IsOverUi();
            torch.gameObject.SetActive(on);
            blackout.enabled = !on;
            if (!on) { for (int i = 0; i < lit.Length; i++) lit[i] = Mathf.Max(0f, lit[i] - Time.deltaTime); return; }
            Vector2 w = PointerInput.ToWorld(cam, sp);
            torch.position = new Vector3(w.x, w.y, torch.position.z);

            for (int i = 0; i < bats.Length; i++)
            {
                bool hit = Vector2.Distance(w, bats[i].position + new Vector3(0f, -0.35f, 0f)) < lightRadius;
                lit[i] = hit ? lit[i] + Time.deltaTime : Mathf.Max(0f, lit[i] - Time.deltaTime);
                // si agita nel sonno: avviso
                bats[i].localRotation = Quaternion.Euler(0, 0, lit[i] > 0.2f ? Mathf.Sin(Time.time * 30f) * 8f : 0f);
                if (lit[i] > wakeAfter) { StartCoroutine(Screech(bats[i])); return; }
            }
            bool switchLit = Vector2.Distance(w, lightSwitch.transform.position) < lightRadius;
            // si preme toccandolo di nuovo dopo averlo visto (un tocco a caso al buio non basta)
            if (began && lightSwitch.OverlapPoint(w) && Time.time - switchLitAt < 3f && Time.time - switchLitAt > 0.05f) { StartCoroutine(LightsOn()); return; }
            if (switchLit) switchLitAt = Time.time;
        }

        IEnumerator LightsOn()
        {
            done = true;
            if (switchArt != null && switchOn != null) switchArt.sprite = switchOn;
            torch.gameObject.SetActive(false);
            blackout.enabled = true;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.4f) { blackout.color = new Color(0.01f, 0.01f, 0.03f, 1f - k); yield return null; }
            blackout.enabled = false;
            if (bubble != null) bubble.Show("Click! Shhh... they're sleeping.");
            yield return new WaitForSeconds(1.6f);
            Win();
        }

        IEnumerator Screech(Transform bat)
        {
            done = true;
            SetBat(bat, true);
            if (bubble != null) bubble.Show("SCREEEEECH!!");
            Vector3 from = bat.position, scale = bat.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.7f)
            {
                bat.position = Vector3.Lerp(from, new Vector3(0f, 0f, from.z), k) + Vector3.up * Mathf.Sin(k * 20f) * 0.15f;
                bat.localScale = scale * Mathf.Lerp(1f, 3.5f, k);   // ti vola in faccia
                yield return null;
            }
            Haptics.Medium();
            yield return new WaitForSeconds(0.5f);
            Fail();
        }

        IEnumerator HideLater(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (bubble != null && !done) bubble.Hide();
        }
    }
}
