using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "The cheap magician" — un mago da quattro soldi deve far sparire il coniglio dal cilindro, ma le sue
    /// formule magiche falliscono sempre. Soluzione: niente magia! Trascinare il coniglio fuori dal cilindro, poi
    /// trascinare il cilindro SOPRA il coniglio (si capovolge e lo copre): "TA-DAA!". Sconfitta: le formule magiche
    /// (pulsanti) trasformano il coniglio in una patata per qualche secondo; alla terza resta patata.
    /// </summary>
    public class CheapMagicianLevel : LevelController
    {
        [SerializeField] Transform rabbit;
        [SerializeField] SpriteRenderer rabbitArt;
        [SerializeField] Sprite rabbitSprite, potatoSprite;
        [SerializeField] Transform hat;                      // cilindro con l'apertura in alto
        [SerializeField] Transform hatBack;                  // interno scuro (segue il cilindro)
        [SerializeField] Transform magician;
        [SerializeField] Transform wand;
        [SerializeField] Collider2D[] spellButtons;
        [SerializeField] SpriteRenderer puff;
        [SerializeField] SpriteRenderer[] sparkles;
        [SerializeField] SpriteRenderer[] hearts;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] float floorMinY = -2.7f, floorMaxY = -1.3f;
        [SerializeField] Vector2 inHatOffset = new Vector2(0f, 0.55f);

        static readonly string[] Spells = { "ABRACADABRA!", "HOCUS POCUS!", "SIM SALA BIM!" };

        Camera cam;
        bool inHat = true, busy, dragRabbit, dragHat;
        int strikes;
        Vector2 grab;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            Kit41B.Hearts(hearts, 3);
            if (puff != null) puff.gameObject.SetActive(false);
            foreach (SpriteRenderer s in sparkles) if (s != null) s.gameObject.SetActive(false);
            PlaceRabbitInHat();
            Say("Behold! I shall make this rabbit... VANISH!", 2.5f);
        }

        IEnumerator Talk(string text, float seconds)
        {
            Kit41B.Say(bubble, text);
            yield return new WaitForSeconds(seconds);
            if (!IsFinished && bubble != null) bubble.Hide();
        }

        void Say(string text, float seconds = 2f)
        {
            StopCoroutineSafe();
            talk = StartCoroutine(Talk(text, seconds));
        }

        Coroutine talk;
        void StopCoroutineSafe() { if (talk != null) StopCoroutine(talk); talk = null; }

        void PlaceRabbitInHat()
        {
            rabbit.position = hat.position + (Vector3)inHatOffset;
        }

        bool Over(Transform t, Vector2 w, float r) => Vector2.Distance(w, t.position) < r;

        void Update()
        {
            if (IsFinished || busy) return;
            if (!Kit41B.Pointer(cam, out Vector2 w, out bool began, out bool ended))
            {
                if (dragRabbit || dragHat) Drop(dragRabbit ? (Vector2)rabbit.position : (Vector2)hat.position);
                return;
            }
            if (began)
            {
                for (int i = 0; i < spellButtons.Length; i++)
                    if (spellButtons[i] != null && spellButtons[i].OverlapPoint(w)) { StartCoroutine(Spell(i)); return; }
                Vector2 rabbitHead = rabbit.position + Vector3.up * 0.25f;
                if (Vector2.Distance(w, rabbitHead) < 0.6f && (!inHat || w.y > hat.position.y + 0.55f))
                {
                    dragRabbit = true;
                    grab = (Vector2)rabbit.position - w;
                    Sfx.Play(this, "pop", 0.4f);
                    return;
                }
                if (Over(hat, w, 0.75f))
                {
                    dragHat = true;
                    grab = (Vector2)hat.position - w;
                    Sfx.Play(this, "tick", 0.5f);
                    return;
                }
                return;
            }
            if (dragRabbit)
            {
                inHat = false;
                rabbit.position = new Vector3(w.x + grab.x, w.y + grab.y, rabbit.position.z);
                if (ended) Drop(w);
            }
            else if (dragHat)
            {
                hat.position = new Vector3(w.x + grab.x, w.y + grab.y, hat.position.z);
                SyncHatBack();
                if (inHat) PlaceRabbitInHat();
                if (ended) Drop(w);
            }
        }

        void SyncHatBack()
        {
            if (hatBack != null) hatBack.position = hat.position;
        }

        Vector3 OnFloor(Vector3 p) => new Vector3(Mathf.Clamp(p.x, -2.4f, 2.4f), Mathf.Clamp(p.y, floorMinY, floorMaxY), p.z);

        void Drop(Vector2 w)
        {
            if (dragRabbit)
            {
                dragRabbit = false;
                if (Vector2.Distance(rabbit.position, hat.position + (Vector3)inHatOffset) < 0.5f)
                {
                    inHat = true;
                    PlaceRabbitInHat();
                    Say("Back in the hat. Classic.");
                    return;
                }
                rabbit.position = OnFloor(rabbit.position);
                Sfx.Play(this, "tick", 0.5f);
                Say(Random.value < 0.5f ? "Hey! Put him back! Where's the magic?" : "That's... not how magic works.");
                return;
            }
            if (!dragHat) return;
            dragHat = false;
            if (!inHat && Vector2.Distance(hat.position, rabbit.position) < 0.85f) { StartCoroutine(TaDaa()); return; }
            hat.position = OnFloor(hat.position);
            SyncHatBack();
            if (inHat) { PlaceRabbitInHat(); Say("He's still IN the hat, genius."); }
            Sfx.Play(this, "clunk", 0.5f);
        }

        IEnumerator Spell(int i)
        {
            busy = true;
            Say(Spells[i % Spells.Length], 1.6f);
            if (wand != null) StartCoroutine(Wave());
            yield return new WaitForSeconds(0.6f);
            strikes++;
            Kit41B.Hearts(hearts, 3 - strikes);
            Sfx.Play(this, "pop", 0.8f);
            Haptics.Light();
            if (puff != null)
            {
                puff.gameObject.SetActive(true);
                puff.transform.position = rabbit.position + Vector3.up * 0.2f;
                StartCoroutine(PuffFx());
            }
            rabbitArt.sprite = potatoSprite;
            yield return Kit41B.Pop(rabbit, 0.3f, 0.3f);
            if (strikes >= 3)
            {
                Say("Well... it's a potato now. Forever.", 2.5f);
                yield return new WaitForSeconds(2f);
                Fail();
                yield break;
            }
            Say(strikes == 1 ? "Oops. Wrong spell. That's a potato." : "Hmm. Potato again.", 2f);
            yield return new WaitForSeconds(2.2f);
            Sfx.Play(this, "pop", 0.6f);
            rabbitArt.sprite = rabbitSprite;
            yield return Kit41B.Pop(rabbit, 0.2f, 0.25f);
            busy = false;
        }

        IEnumerator Wave()
        {
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.6f)
            {
                wand.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(k * Mathf.PI * 3f) * 25f);
                yield return null;
            }
            wand.localRotation = Quaternion.identity;
        }

        IEnumerator PuffFx()
        {
            Transform t = puff.transform;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.7f)
            {
                t.localScale = Vector3.one * Mathf.Lerp(0.6f, 1.8f, k);
                puff.color = new Color(1f, 1f, 1f, 1f - k);
                yield return null;
            }
            puff.gameObject.SetActive(false);
        }

        IEnumerator TaDaa()
        {
            busy = true;
            // il cilindro si capovolge sopra il coniglio
            Vector3 target = rabbit.position + new Vector3(0f, -0.15f, 0f);
            Vector3 from = hat.position;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.35f)
            {
                hat.position = Vector3.Lerp(from, target + Vector3.up * 0.6f, k);
                hat.localRotation = Quaternion.Euler(0f, 0f, 180f * k);
                SyncHatBack();
                yield return null;
            }
            if (hatBack != null) hatBack.gameObject.SetActive(false);
            hat.localRotation = Quaternion.Euler(0f, 0f, 180f);
            hat.position = target + Vector3.up * 0.6f;
            rabbit.gameObject.SetActive(false);
            Sfx.Play(this, "clunk", 0.8f);
            Haptics.Light();
            yield return Kit41B.Shake(hat, 0.05f, 0.3f);
            if (wand != null) StartCoroutine(Wave());
            Say("...TA-DAAA! It VANISHED!", 3f);
            for (int i = 0; i < sparkles.Length; i++) StartCoroutine(Sparkle(sparkles[i], i * 0.12f));
            Sfx.Play(this, "ding", 0.8f);
            if (magician != null) yield return Bow();
            yield return new WaitForSeconds(1.2f);
            Win();
        }

        IEnumerator Bow()
        {
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.8f)
            {
                magician.localRotation = Quaternion.Euler(0f, 0f, -Mathf.Sin(k * Mathf.PI) * 18f);
                yield return null;
            }
            magician.localRotation = Quaternion.identity;
        }

        IEnumerator Sparkle(SpriteRenderer s, float delay)
        {
            if (s == null) yield break;
            yield return new WaitForSeconds(delay);
            s.gameObject.SetActive(true);
            Vector3 p = s.transform.localPosition;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.9f)
            {
                s.transform.localPosition = p + Vector3.up * k * 0.5f;
                s.transform.localScale = Vector3.one * Mathf.Sin(k * Mathf.PI) * 0.6f;
                yield return null;
            }
            s.gameObject.SetActive(false);
        }
    }
}
