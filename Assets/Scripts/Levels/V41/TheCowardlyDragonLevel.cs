using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "The cowardly dragon" — il drago dovrebbe bruciare la barricata ma ha paura del fuoco: chiedergli di
    /// soffiare (tasto o tocco sul drago) produce solo tosse, e alla terza volta scappa (sconfitta). Il trucco: trascina
    /// il SOLE dietro di lui, basso e vicino: la sua ombra diventa enorme e mostruosa sulla parete davanti; il drago la
    /// vede, si spaventa e starnutisce una fiammata che brucia la barricata.
    /// </summary>
    public class TheCowardlyDragonLevel : S41JLevelBase
    {
        [SerializeField] Transform dragon;            // piedi del drago (guarda a destra)
        [SerializeField] SpriteRenderer dragonArt;
        [SerializeField] Sprite idleSprite, scaredSprite;
        [SerializeField] Collider2D dragonArea;
        [SerializeField] Transform sun;
        [SerializeField] Collider2D sunArea;
        [SerializeField] Transform shadow;            // ombra sulla parete (base in basso al centro)
        [SerializeField] SpriteRenderer shadowArt;
        [SerializeField] Transform barricade;
        [SerializeField] SpriteRenderer barricadeArt;
        [SerializeField] Transform fire;              // fiammata (base alla bocca, si allunga verso destra)
        [SerializeField] Transform mouth;
        [SerializeField] SmokeEmitter flames;
        [SerializeField] SmokeEmitter cough;
        [SerializeField] Collider2D blowArea;
        [SerializeField] TextMesh triesText;

        const int MaxTries = 3;
        const float ScaryScale = 1.5f, FearTime = 0.8f;
        bool grabbing;
        Vector2 grabOffset;
        int tries;
        float fear;
        Vector3 dragonHome;

        protected override void Begin()
        {
            dragonHome = dragon.localPosition;
            if (fire != null) fire.gameObject.SetActive(false);
            if (flames != null) flames.Stop();
            if (cough != null) cough.Stop();
            UpdateTries();
            UpdateShadow(out _);
            Say("Burn it? With FIRE? Fire is scary!", 3f);
        }

        void UpdateTries() { if (triesText != null) triesText.text = "BLOW ATTEMPTS " + tries + "/" + MaxTries; }

        Vector2 Body => (Vector2)dragon.position + new Vector2(0f, 1.0f);

        /// <summary>Ombra proiettata sulla parete: va dalla parte opposta al sole e cresce quando il sole è vicino.</summary>
        void UpdateShadow(out bool scary)
        {
            Vector2 b = Body, s = sun.position;
            float dist = Mathf.Max(0.6f, Vector2.Distance(b, s));
            float scale = Mathf.Clamp(3.4f / dist, 0.5f, 2.6f);
            float x = Mathf.Clamp(dragon.position.x + (b.x - s.x) * 0.8f, -2.2f, 2.2f);
            shadow.position = new Vector3(x, dragon.position.y + 0.1f, shadow.position.z);
            shadow.localScale = new Vector3(scale * (1f + Mathf.Sin(clock * 3f) * 0.02f), scale, 1f);
            // più il sole è alto, più l'ombra è sbiadita
            Alpha(shadowArt, Mathf.Clamp(0.75f - (s.y - b.y) * 0.08f, 0.3f, 0.75f));
            scary = x > dragon.position.x + 0.6f && scale >= ScaryScale;
        }

        protected override void Tick()
        {
            UpdateShadow(out bool scary);
            if (busy) return;
            // paura crescente: il drago trema guardando l'ombra
            fear = scary ? fear + Time.deltaTime : Mathf.Max(0f, fear - Time.deltaTime * 2f);
            float shiver = fear / FearTime;
            dragon.localPosition = dragonHome + new Vector3(Mathf.Sin(clock * 55f) * 0.04f * shiver, 0f, 0f);
            if (dragonArt != null) dragonArt.sprite = shiver > 0.3f ? scaredSprite : idleSprite;
            if (fear >= FearTime) { grabbing = false; StartCoroutine(Sneeze()); return; }

            if (!Pointer(out Vector2 w, out bool began, out bool ended)) { grabbing = false; return; }
            if (began)
            {
                if (sunArea != null && sunArea.OverlapPoint(w)) { grabbing = true; grabOffset = (Vector2)sun.position - w; Sfx.Play(this, "tick", 0.5f); }
                else if ((blowArea != null && blowArea.OverlapPoint(w)) || (dragonArea != null && dragonArea.OverlapPoint(w))) { StartCoroutine(Cough()); return; }
            }
            if (grabbing)
            {
                Vector2 p = w + grabOffset;
                sun.position = new Vector3(Mathf.Clamp(p.x, -2.5f, 2.5f), Mathf.Clamp(p.y, -1.9f, 2.9f), sun.position.z);
            }
            if (ended) grabbing = false;
        }

        IEnumerator Cough()
        {
            busy = true;
            grabbing = false;
            tries++;
            UpdateTries();
            Say(tries >= MaxTries ? "NOPE! I quit!" : tries == 1 ? "Hhhhh... *cough cough*" : "I c-can't! It's too HOT!", 2f);
            Sfx.Play(this, "whoosh", 0.5f);
            if (cough != null) { cough.transform.position = mouth.position; cough.Play(); }
            yield return Shake(dragon, 0.06f, 0.6f);
            if (cough != null) cough.Stop();
            if (tries >= MaxTries)
            {
                // il drago scappa via a gambe levate
                if (dragonArt != null) dragonArt.sprite = scaredSprite;
                dragon.localScale = new Vector3(-dragon.localScale.x, dragon.localScale.y, 1f);
                Sfx.Play(this, "bonk", 0.8f);
                Haptics.Medium();
                yield return Walk(dragon, dragon.localPosition + new Vector3(-4.5f, 0f, 0f), 6f, 0.15f);
                yield return new WaitForSeconds(0.6f);
                Fail();
                yield break;
            }
            busy = false;
        }

        IEnumerator Sneeze()
        {
            busy = true;
            if (dragonArt != null) dragonArt.sprite = scaredSprite;
            Say("AAAH! A MONSTER! A-A-A...", 1.6f);
            Sfx.Play(this, "pop", 0.8f);
            Haptics.Medium();
            yield return Move(dragon, dragonHome, 0.3f, 0.5f);
            // inspira...
            Vector3 s = dragon.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.6f) { dragon.localScale = new Vector3(s.x * (1f - k * 0.08f), s.y * (1f + k * 0.12f), 1f); yield return null; }
            dragon.localScale = s;
            Say("ACHOOOO!", 1.6f);
            Sfx.Play(this, "whoosh", 1f);
            Haptics.Heavy();
            fire.gameObject.SetActive(true);
            fire.position = mouth.position;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.35f) { fire.localScale = new Vector3(Mathf.Lerp(0.1f, 1f, k), Mathf.Lerp(0.4f, 1f, k), 1f); yield return null; }
            fire.localScale = Vector3.one;
            if (flames != null) flames.Play();
            Haptics.Heavy();
            Sfx.Play(this, "clunk", 0.8f);
            // la barricata brucia e crolla
            for (float k = 0f; k < 1f; k += Time.deltaTime / 1.3f)
            {
                if (barricadeArt != null) barricadeArt.color = Color.Lerp(Color.white, new Color(0.15f, 0.1f, 0.1f), k);
                fire.localScale = new Vector3(1f, 1f + Mathf.Sin(k * 40f) * 0.08f, 1f);
                yield return null;
            }
            fire.gameObject.SetActive(false);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.5f) { barricade.localScale = new Vector3(1f + k * 0.2f, 1f - k, 1f); yield return null; }
            barricade.gameObject.SetActive(false);
            if (flames != null) flames.Stop();
            Haptics.Medium();
            if (dragonArt != null) dragonArt.sprite = idleSprite;
            Say("...I totally meant to do that.", 2.5f);
            yield return Move(dragon, dragonHome, 0.35f, 0.3f);
            yield return Celebrate();
        }
    }
}
