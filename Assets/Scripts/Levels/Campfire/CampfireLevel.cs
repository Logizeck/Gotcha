using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;
using Gotcha.Sensors;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 24 — il falò. 1) Strofina il bastoncino sulla legna, avanti e indietro (col dito), finché esce
    /// una scintilla; 2) soffia PIANO nel microfono per far crescere la fiamma. Un soffio troppo forte la spegne
    /// e si ricomincia a strofinare. (Editor: tasto B = soffio leggero, N = soffio forte.)
    /// Varianti con la pioggia: senza riparo la legna si bagna (le strofinate si perdono) e la fiamma si spegne;
    /// l'ombrello va trascinato sopra il fuoco, ma non troppo vicino o prende fuoco (Easter egg `burnt_umbrella`).
    /// Intanto il topolino ha sempre più freddo: se il fuoco non parte in tempo, congela.
    /// </summary>
    public class CampfireLevel : LevelController
    {
        [SerializeField] Collider2D logsArea;
        [SerializeField] Transform stick;
        [SerializeField] Transform flame;
        [SerializeField] SpriteRenderer glow;
        [SerializeField] SmokeEmitter smoke;
        [SerializeField] SmokeEmitter sparks;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] int strokesNeeded = 10;
        [SerializeField] float gentleMin = 0.07f;
        [SerializeField] float strongAbove = 0.85f;
        [SerializeField] float growSeconds = 3f;

        [Header("Pioggia (varianti)")]
        [SerializeField] bool rain;
        [SerializeField] Draggable2D umbrella;
        [SerializeField] SpriteRenderer umbrellaArt;
        [SerializeField] Sprite umbrellaBurnt;
        [SerializeField] SmokeEmitter umbrellaFire;
        [SerializeField] Transform[] rainDrops;
        [SerializeField] float rainSpeed = 9f;
        [SerializeField] float coverHalfWidth = 0.85f;     // metà larghezza utile dell'ombrello
        [SerializeField] float burnGap = 0.35f;            // più vicino di così alla punta della fiamma: brucia
        [SerializeField] float wetEvery = 1.1f;            // senza riparo, ogni tanto si perde una strofinata
        [SerializeField] float coldSeconds = 38f;
        [SerializeField] Transform mouse;
        [SerializeField] SpriteRenderer mouseArt;
        [SerializeField] SpriteRenderer iceBlock;
        [SerializeField] string eggId = "burnt_umbrella";

        enum Phase { Rub, Blow, Done }
        Phase phase = Phase.Rub;
        int strokes;
        float lastX, swing, dir, size, strongFor, sparkFor;
        bool tracking;
        float cold, wetTimer;
        Camera cam;
        Vector3 stickHome;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            if (SensorHub.Instance != null) SensorHub.Instance.UseMic(this);
            if (stick != null) stickHome = stick.position;
            flame.gameObject.SetActive(false);
            if (glow != null) glow.color = new Color(1f, 0.6f, 0.2f, 0f);
            if (smoke != null) smoke.Stop();
            if (sparks != null) sparks.Stop();
            if (bubble != null) { bubble.Show(rain ? "Brrr... and it's raining!" : "Brrr... so cold!"); StartCoroutine(HideLater(1.8f)); }
            if (umbrellaFire != null) umbrellaFire.Stop();
            if (iceBlock != null) iceBlock.color = new Color(1f, 1f, 1f, 0f);
            if (umbrella != null) umbrella.gameObject.SetActive(rain);
            if (rainDrops != null)
                foreach (Transform d in rainDrops) { d.gameObject.SetActive(rain); d.position = new Vector3(Random.Range(-3.2f, 3.2f), Random.Range(-4f, 4f), 0f); }
        }

        /// <summary>L'ombrello ripara il fuoco se la sua cupola sta sopra la legna.</summary>
        bool Covered()
        {
            if (umbrella == null) return false;
            Vector3 u = umbrella.transform.position;
            return Mathf.Abs(u.x - flame.position.x) < coverHalfWidth && u.y > flame.position.y + 0.4f;
        }

        float FlameTop => flame.position.y + 0.3f * (flame.gameObject.activeSelf ? flame.localScale.y : 0f);

        void Rain()
        {
            if (rainDrops == null) return;
            Vector3 u = umbrella != null ? umbrella.transform.position : new Vector3(99f, 99f, 0f);
            foreach (Transform d in rainDrops)
            {
                Vector3 p = d.position + new Vector3(-0.15f, -1f, 0f) * rainSpeed * Time.deltaTime;
                bool onUmbrella = Mathf.Abs(p.x - u.x) < 0.9f && p.y < u.y + 0.45f && p.y > u.y - 0.1f;
                if (p.y < -4.2f || onUmbrella) p = new Vector3(Random.Range(-3f, 3.6f), Random.Range(4.2f, 5f), 0f);
                d.position = p;
            }
        }

        /// <summary>Freddo del topolino: sale senza fuoco, scende col fuoco acceso. Al massimo: congelato.</summary>
        bool Chill()
        {
            if (mouse == null || coldSeconds <= 0f) return false;
            bool warm = phase == Phase.Blow && flame.gameObject.activeSelf && flame.localScale.y > 2.5f;
            cold = Mathf.Clamp01(cold + Time.deltaTime / (warm ? -12f : coldSeconds));
            if (mouseArt != null) mouseArt.color = Color.Lerp(Color.white, new Color(0.6f, 0.8f, 1f), cold);
            if (cold >= 1f) { StartCoroutine(Frozen()); return true; }
            return false;
        }

        void OnDestroy() { if (SensorHub.Instance != null) SensorHub.Instance.ReleaseMic(this); }

        void Update()
        {
            if (IsFinished || phase == Phase.Done) return;
            if (rain)
            {
                Rain();
                if (Chill()) return;
                // ombrello troppo vicino alla fiamma: prende fuoco
                if (umbrella != null && flame.gameObject.activeSelf && Mathf.Abs(umbrella.transform.position.x - flame.position.x) < coverHalfWidth
                    && umbrella.transform.position.y < FlameTop + burnGap && umbrella.transform.position.y > flame.position.y)
                {
                    StartCoroutine(UmbrellaBurns());
                    return;
                }
                if (!Covered())
                {
                    wetTimer -= Time.deltaTime;
                    if (phase == Phase.Rub && wetTimer <= 0f && strokes > 0)
                    {
                        wetTimer = wetEvery;
                        strokes--;   // la legna si bagna
                        if (bubble != null && Random.value < 0.3f) { bubble.Show("The wood is getting wet!"); StartCoroutine(HideLater(1.2f)); }
                    }
                }
                else wetTimer = wetEvery;
            }
            sparkFor -= Time.deltaTime;
            if (sparks != null && sparkFor <= 0f) sparks.Stop();
            if (phase == Phase.Rub) Rub();
            else Blow();
        }

        void Rub()
        {
            if (PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out bool ended) && !ended)
            {
                Vector2 w = PointerInput.ToWorld(cam, sp);
                if (logsArea.OverlapPoint(w) || tracking)
                {
                    if (stick != null) stick.position = new Vector3(w.x, Mathf.Max(w.y, stickHome.y - 0.1f), stick.position.z);
                    if (tracking && !began)
                    {
                        float dx = w.x - lastX;
                        if (Mathf.Abs(dx) > 0.001f)
                        {
                            float d = Mathf.Sign(dx);
                            if (d != dir && swing > 0.25f)   // cambio di direzione dopo un bel tratto: una strofinata
                            {
                                strokes++;
                                swing = 0f;
                                Haptics.Light();
                                if (smoke != null) smoke.Play();
                                if (sparks != null) { sparks.transform.position = new Vector3(w.x, stickHome.y - 0.35f, 0f); sparks.Play(); sparkFor = 0.12f; }
                            }
                            if (d != dir) swing = 0f;
                            dir = d;
                            swing += Mathf.Abs(dx);
                        }
                    }
                    tracking = true;
                    lastX = w.x;
                }
            }
            else
            {
                tracking = false;
                if (stick != null) stick.position = Vector3.Lerp(stick.position, stickHome, 10f * Time.deltaTime);
            }

            if (strokes >= strokesNeeded)
            {
                phase = Phase.Blow;
                size = 0f;
                flame.gameObject.SetActive(true);
                if (smoke != null) smoke.Stop();
                if (bubble != null) { bubble.Show("A spark! Now... gently!"); StartCoroutine(HideLater(1.8f)); }
            }
        }

        void Blow()
        {
            float level = SensorHub.Instance != null ? SensorHub.Instance.MicLevel01 : 0f;
#if UNITY_EDITOR
            if (Input.GetKey(KeyCode.B)) level = 0.35f;
            if (Input.GetKey(KeyCode.N)) level = 1f;
#endif
            strongFor = level > strongAbove ? strongFor + Time.deltaTime : 0f;
            if (strongFor > 0.25f) { StartCoroutine(BlownOut()); return; }
            if (level > gentleMin) size = Mathf.Min(1f, size + Time.deltaTime / growSeconds);
            if (rain && !Covered())
            {
                size -= Time.deltaTime / 2.2f;   // la pioggia la spegne
                if (size <= -0.05f) { StartCoroutine(RainedOut()); return; }
            }

            float s = Mathf.Lerp(1.3f, 5.5f, size);
            float flicker = 1f + Mathf.Sin(Time.time * 17f) * 0.06f + Mathf.Sin(Time.time * 29f) * 0.04f;
            flame.localScale = new Vector3(s * (2f - flicker), s * flicker * (1f + level * 0.3f), 1f);
            flame.localRotation = Quaternion.Euler(0, 0, -level * 25f);
            if (glow != null) glow.color = new Color(1f, 0.6f, 0.2f, Mathf.Lerp(0.15f, 0.8f, size));
            if (size >= 1f) StartCoroutine(Cozy());
        }

        IEnumerator BlownOut()
        {
            phase = Phase.Rub;
            strokes = strokesNeeded / 2;   // la legna è ancora calda: basta meno
            flame.gameObject.SetActive(false);
            if (glow != null) glow.color = new Color(1f, 0.6f, 0.2f, 0f);
            if (smoke != null) smoke.Play();
            if (bubble != null) bubble.Show("Too strong! Poof...");
            yield return new WaitForSeconds(0.8f);
            if (smoke != null) smoke.Stop();
            yield return new WaitForSeconds(0.8f);
            if (bubble != null && phase != Phase.Done) bubble.Hide();
        }

        IEnumerator RainedOut()
        {
            phase = Phase.Rub;
            strokes = 0;
            flame.gameObject.SetActive(false);
            if (glow != null) glow.color = new Color(1f, 0.6f, 0.2f, 0f);
            if (smoke != null) smoke.Play();
            if (bubble != null) bubble.Show("Tsss... the rain put it out!");
            yield return new WaitForSeconds(0.8f);
            if (smoke != null) smoke.Stop();
        }

        /// <summary>Easter egg: ombrello troppo vicino alla fiamma. Prende fuoco, si accartoccia, la pioggia spegne tutto.</summary>
        IEnumerator UmbrellaBurns()
        {
            phase = Phase.Done;
            if (umbrella != null) umbrella.enabled = false;
            Haptics.Medium();
            if (umbrellaFire != null) { umbrellaFire.transform.position = umbrella.transform.position + Vector3.up * 0.25f; umbrellaFire.Play(); }
            if (bubble != null) bubble.Show("My umbrella! FIRE!!");
            for (float k = 0f; k < 1.3f; k += Time.deltaTime)
            {
                if (umbrellaArt != null) umbrellaArt.color = Color.Lerp(Color.white, new Color(0.35f, 0.3f, 0.3f), k / 1.3f);
                umbrella.transform.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(k * 25f) * 8f);
                Rain();
                yield return null;
            }
            if (umbrellaArt != null && umbrellaBurnt != null) { umbrellaArt.sprite = umbrellaBurnt; umbrellaArt.color = Color.white; }
            if (umbrellaFire != null) umbrellaFire.Stop();
            umbrella.transform.localRotation = Quaternion.Euler(0, 0, 12f);
            // senza ombrello la pioggia spegne il fuoco: pfff
            flame.gameObject.SetActive(false);
            if (glow != null) glow.color = new Color(1f, 0.6f, 0.2f, 0f);
            if (smoke != null) smoke.Play();
            for (float k = 0f; k < 1.5f; k += Time.deltaTime) { Rain(); yield return null; }
            if (smoke != null) smoke.Stop();
            if (bubble != null) bubble.Show("...well, that went well.");
            for (float k = 0f; k < 1.2f; k += Time.deltaTime) { Rain(); yield return null; }
            FailWithEgg(eggId);
        }

        /// <summary>Il topolino congela: diventa azzurro, smette di tremare e resta chiuso in un cubetto di ghiaccio.</summary>
        IEnumerator Frozen()
        {
            phase = Phase.Done;
            Tremble shiver = mouse.GetComponent<Tremble>();
            if (bubble != null) bubble.Show("Brrr... brr... b...");
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.9f)
            {
                if (shiver != null) mouse.localPosition += (Vector3)(Random.insideUnitCircle * 0.01f * (1f - k));
                Rain();
                yield return null;
            }
            if (shiver != null) shiver.enabled = false;
            if (mouseArt != null) mouseArt.color = new Color(0.55f, 0.75f, 1f);
            // il ghiaccio cresce intorno: scatta, luccica
            if (iceBlock != null)
            {
                Transform ice = iceBlock.transform;
                Vector3 full = ice.localScale;
                for (float k = 0f; k < 1f; k += Time.deltaTime / 0.5f)
                {
                    float pop = k < 0.7f ? k / 0.7f * 1.1f : Mathf.Lerp(1.1f, 1f, (k - 0.7f) / 0.3f);
                    ice.localScale = new Vector3(full.x * pop, full.y * Mathf.Min(1f, k * 1.6f), 1f);
                    iceBlock.color = new Color(1f, 1f, 1f, Mathf.Min(0.85f, k * 1.5f));
                    Rain();
                    yield return null;
                }
                ice.localScale = full;
            }
            Haptics.Medium();
            if (sparks != null) { sparks.transform.position = mouse.position + Vector3.up * 0.5f; sparks.tint = new Color(0.8f, 0.95f, 1f); sparks.Play(); }
            if (bubble != null) bubble.Show("*frozen*");
            for (float k = 0f; k < 0.3f; k += Time.deltaTime) { Rain(); yield return null; }
            if (sparks != null) sparks.Stop();
            for (float k = 0f; k < 1.2f; k += Time.deltaTime) { Rain(); yield return null; }
            Fail();
        }

        IEnumerator Cozy()
        {
            phase = Phase.Done;
            if (sparks != null) { sparks.transform.position = flame.position + Vector3.up * 0.6f; sparks.Play(); }
            if (bubble != null) bubble.Show("Ahhh... cozy!");
            for (float k = 0f; k < 1.6f; k += Time.deltaTime)
            {
                float flicker = 1f + Mathf.Sin(Time.time * 17f) * 0.06f;
                flame.localScale = new Vector3(5.5f * (2f - flicker), 5.5f * flicker, 1f);
                yield return null;
            }
            if (sparks != null) sparks.Stop();
            Win();
        }

        IEnumerator HideLater(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (bubble != null && phase != Phase.Done) bubble.Hide();
        }
    }
}
