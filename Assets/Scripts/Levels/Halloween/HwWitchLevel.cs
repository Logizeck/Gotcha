using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels.Halloween;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 248 "The witch and the impossible potion" — la strega vuole diventare bella. Rospo, occhio, fungo, cuore e
    /// mela nel calderone danno solo reazioni comiche temporanee (rospo, mela, occhi dappertutto...), nessuna
    /// combinazione funziona. Soluzione: la parola BEAUTIFUL della frase in alto è un pezzo a sé (la frase è composta da
    /// pezzi affiancati): trascinata nel calderone la pozione diventa rosa, la strega la beve e diventa glamour (il nasone
    /// resta uguale) e si fa un selfie. Lasciata altrove la parola torna nella frase.
    /// </summary>
    public class HwWitchLevel : LevelController
    {
        [SerializeField] Transform witch;
        [SerializeField] SpriteRenderer witchArt;
        [SerializeField] Sprite witchNormal, witchGlam, witchToad, witchApple;
        [SerializeField] GameObject eyesOverlay;
        [SerializeField] SmokeEmitter hearts;
        [SerializeField] SmokeEmitter witchPuff;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] Transform cauldron;
        [SerializeField] Collider2D cauldronArea;
        [SerializeField] SpriteRenderer brew;
        [SerializeField] SmokeEmitter brewBubbles;
        [SerializeField] SmokeEmitter splash;
        [SerializeField] Color brewColor = new Color(0.45f, 0.85f, 0.35f);
        [SerializeField] Color pinkColor = new Color(1f, 0.45f, 0.75f);
        [SerializeField] Transform[] ingredients;       // nell'ordine di WitchIngredient
        [SerializeField] Collider2D[] ingredientAreas;
        [SerializeField] TextMesh[] rowPieces;          // la riga con la parola: "TO BE", "BEAUTIFUL", "."
        [SerializeField] int wordIndex = 1;
        [SerializeField] BoxCollider2D wordArea;
        [SerializeField] SpriteRenderer gapMark;
        [SerializeField] float rowY = 2.95f;
        [SerializeField] float rowSpace = 0.14f;
        [SerializeField] GameObject phone;
        [SerializeField] SpriteRenderer flash;
        [SerializeField] SmokeEmitter[] confetti;

        readonly WitchLogic logic = new WitchLogic();
        Camera cam;
        HwDragger drag;
        HwDragger.Item wordItem;
        HwDragger.Item[] ingredientItems;
        HwSay say;
        Transform word;
        Vector3 witchScale, wordScale;
        Vector3 wordSlot;
        bool wordFlying, busy;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            logic.Reset();
            drag = new HwDragger(this);
            say = new HwSay(bubble);
            word = rowPieces[wordIndex].transform;
            wordScale = word.localScale;
            witchScale = witch.localScale;
            wordItem = drag.Add(word, wordArea, 10);
            wordItem.Lift = 1.15f;
            ingredientItems = new HwDragger.Item[ingredients.Length];
            for (int i = 0; i < ingredients.Length; i++) ingredientItems[i] = drag.Add(ingredients[i], ingredientAreas[i], 5);
            witchArt.sprite = witchNormal;
            witchArt.color = Color.white;
            eyesOverlay.SetActive(false);
            phone.SetActive(false);
            HalloweenKit.SetAlpha(flash, 0f);
            brew.color = brewColor;
            if (brewBubbles != null) { brewBubbles.tint = brewColor; brewBubbles.Play(); }
            if (hearts != null) hearts.Stop();
            if (witchPuff != null) witchPuff.Stop();
            if (splash != null) splash.Stop();
            HalloweenKit.Confetti(confetti, false);
            gapMark.enabled = false;
            LayoutRow();
            say.Show("I want to be BEAUTIFUL! Help me brew it!", 2.6f);
        }

        /// <summary>
        /// Dispone i pezzi della riga uno accanto all'altro, centrati, misurando il testo disegnato (niente indici fissi
        /// nella stringa). Se la parola è fuori, al suo posto resta il vuoto con un trattino.
        /// </summary>
        void LayoutRow()
        {
            float total = 0f;
            var widths = new float[rowPieces.Length];
            for (int i = 0; i < rowPieces.Length; i++)
            {
                MeshRenderer mr = rowPieces[i].GetComponent<MeshRenderer>();
                float w = mr.bounds.size.x;
                // la parola si misura alla sua scala di riposo (mentre la si trascina è un po' più grande)
                if (i == wordIndex && word.localScale.x > 0.001f) w *= wordScale.x / word.localScale.x;
                widths[i] = Mathf.Max(0.05f, w);
                total += widths[i];
            }
            total += rowSpace * (rowPieces.Length - 1);
            float x = -total * 0.5f;
            for (int i = 0; i < rowPieces.Length; i++)
            {
                // il punto finale si attacca alla parola
                if (i > 0) x += rowPieces[i].text == "." ? 0f : rowSpace;
                Vector3 p = new Vector3(transform.position.x + x + widths[i] * 0.5f, transform.position.y + rowY, rowPieces[i].transform.position.z);
                if (i == wordIndex) wordSlot = p;
                else rowPieces[i].transform.position = p;
                x += widths[i];
            }
            Vector3 ls = word.lossyScale;
            Vector3 size = rowPieces[wordIndex].GetComponent<MeshRenderer>().bounds.size;
            wordArea.size = new Vector2(Mathf.Max(1.8f, size.x / Mathf.Max(0.01f, ls.x) + 0.3f), Mathf.Max(0.8f, size.y / Mathf.Max(0.01f, ls.y) + 0.3f));
            gapMark.transform.position = wordSlot + new Vector3(0f, -0.2f, 0f);
            gapMark.transform.localScale = new Vector3(widths[wordIndex], 0.06f, 1f);
        }

        void LateUpdate()
        {
            LayoutRow();
            bool home = logic.WordInSentence && !wordFlying && drag.Held != wordItem;
            if (home && !logic.InputLocked) word.position = wordSlot;
            gapMark.enabled = !home && !logic.InputLocked;
        }

        void Update()
        {
            say.Tick();
            wordItem.Enabled = logic.WordInSentence && !wordFlying && !logic.InputLocked;
            if (IsFinished || busy || logic.InputLocked) return;
            if (!HalloweenKit.ReadPointer(cam, out Vector2 w, out bool began, out bool ended))
            {
                if (drag.Held != null) Drop(drag.Release());
                return;
            }
            if (began)
            {
                HwDragger.Item it = drag.TryPick(w);
                if (it == wordItem && !logic.TryPickWord()) drag.Cancel();
                else if (it == null && witchArt.bounds.Contains(new Vector3(w.x, w.y, witchArt.bounds.center.z)))
                    say.Show(logic.State == WitchState.Reacting ? "Don't poke me now!" : "Do I look beautiful yet? Not yet!", 1.6f);
            }
            if (drag.Held == null) return;
            drag.MoveHeld(w);
            if (ended) Drop(drag.Release());
        }

        bool InCauldron(Vector3 p) => cauldronArea.OverlapPoint(p);

        void Drop(HwDragger.Item it)
        {
            if (it == null) return;
            if (it == wordItem)
            {
                if (logic.TryDropWord(InCauldron(word.position)))
                {
                    StartCoroutine(Ending());
                    return;
                }
                if (InCauldron(word.position)) say.Show("Not now! I'm a bit busy here!", 1.6f);
                StartCoroutine(WordBack());
                return;
            }
            int index = System.Array.IndexOf(ingredientItems, it);
            if (index < 0) return;
            if (InCauldron(it.T.position))
            {
                if (logic.TryAddIngredient((WitchIngredient)index)) { StartCoroutine(Reaction((WitchIngredient)index, it)); return; }
                say.Show("One at a time, dearie!", 1.4f);
            }
            drag.SendHome(it);
        }

        IEnumerator WordBack()
        {
            wordFlying = true;
            Sfx.Play(this, "whoosh", 0.4f);
            yield return HalloweenKit.Move(word, wordSlot, 0.35f, 0.4f);
            word.localScale = wordScale;
            wordFlying = false;
        }

        IEnumerator Reaction(WitchIngredient ing, HwDragger.Item item)
        {
            // l'ingrediente cade nel calderone (torna sullo scaffale dopo la reazione: non si esaurisce mai)
            item.Enabled = false;
            Vector3 itemScale = item.T.localScale;
            StartCoroutine(HalloweenKit.ScaleTo(item.T, itemScale * 0.2f, 0.25f));
            yield return HalloweenKit.Move(item.T, cauldron.position + Vector3.up * 0.5f, 0.25f);
            item.T.gameObject.SetActive(false);
            item.T.localScale = itemScale;
            if (splash != null) StartCoroutine(HalloweenKit.Burst(splash, 0.25f));
            Sfx.Play(this, "pop", 0.6f);
            Haptics.Light();
            yield return new WaitForSeconds(0.25f);
            if (witchPuff != null) StartCoroutine(HalloweenKit.Burst(witchPuff, 0.3f));
            Haptics.Medium();
            switch (ing)
            {
                case WitchIngredient.Toad:
                    witchArt.sprite = witchToad;
                    say.Show("RIBBIT?! ...I mean: YUCK!", 1.8f);
                    Sfx.Play(this, "bonk", 0.7f);
                    yield return HalloweenKit.Pop(witch, 1.2f, 0.3f);
                    yield return new WaitForSeconds(1.5f);
                    break;
                case WitchIngredient.Apple:
                    witchArt.sprite = witchApple;
                    say.Show("I'm an APPLE?! Not my type!", 1.8f);
                    Sfx.Play(this, "bonk", 0.7f);
                    yield return HalloweenKit.Wiggle(witch, 10f, 1.8f, 12f);
                    break;
                case WitchIngredient.Eye:
                    eyesOverlay.SetActive(true);
                    say.Show("I can see EVERYTHING! Even my nose!", 1.8f);
                    Sfx.Play(this, "buzz", 0.5f);
                    yield return HalloweenKit.Shake(witch, 0.05f, 1.8f);
                    break;
                case WitchIngredient.Mushroom:
                    say.Show("I'm tiny... and SPOTTY?!", 1.8f);
                    Sfx.Play(this, "pop", 0.7f);
                    yield return HalloweenKit.ScaleTo(witch, witchScale * 0.45f, 0.3f);
                    yield return HalloweenKit.Wiggle(witch, 15f, 1.2f, 14f);
                    break;
                default:
                    witchArt.color = new Color(1f, 0.75f, 0.85f);
                    if (hearts != null) hearts.Play();
                    say.Show("I'm in love... with my BROOM!", 1.8f);
                    Sfx.Play(this, "ding", 0.5f);
                    yield return HalloweenKit.Wiggle(witch, 6f, 1.8f, 6f);
                    break;
            }
            // torna normale e si riprova
            if (witchPuff != null) StartCoroutine(HalloweenKit.Burst(witchPuff, 0.25f));
            witchArt.sprite = witchNormal;
            witchArt.color = Color.white;
            eyesOverlay.SetActive(false);
            if (hearts != null) hearts.Stop();
            witch.localScale = witchScale;
            witch.localRotation = Quaternion.identity;
            item.T.position = item.Home;
            item.T.gameObject.SetActive(true);
            StartCoroutine(HalloweenKit.Pop(item.T, 1.2f, 0.25f));
            item.Enabled = true;
            logic.TryEndReaction();
            say.Show(ing == WitchIngredient.Toad ? "Phew. Still not beautiful." : "Back to normal... still not beautiful!", 1.8f);
        }

        IEnumerator Ending()
        {
            busy = true;
            wordFlying = true;
            // la parola si scioglie nel calderone: pozione rosa
            StartCoroutine(HalloweenKit.ScaleTo(word, wordScale * 0.2f, 0.4f));
            yield return HalloweenKit.Move(word, cauldron.position + Vector3.up * 0.4f, 0.4f);
            word.gameObject.SetActive(false);
            Sfx.Play(this, "pop", 0.8f);
            Haptics.Medium();
            if (splash != null) { splash.tint = pinkColor; StartCoroutine(HalloweenKit.Burst(splash, 0.5f)); }
            if (brewBubbles != null) brewBubbles.tint = pinkColor;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.6f)
            {
                brew.color = Color.Lerp(brewColor, pinkColor, k);
                yield return null;
            }
            brew.color = pinkColor;
            say.Show("PINK! It smells like... BEAUTY!", 1.8f);
            yield return new WaitForSeconds(1.2f);
            say.Show("*GLUG GLUG GLUG*", 1.2f);
            Sfx.Play(this, "bonk", 0.6f);
            yield return HalloweenKit.Wiggle(witch, 8f, 1.0f, 20f);
            if (witchPuff != null) witchPuff.Play();
            Haptics.Heavy();
            Sfx.Play(this, "whoosh", 0.8f);
            yield return new WaitForSeconds(0.5f);
            witchArt.sprite = witchGlam;
            if (witchPuff != null) witchPuff.Stop();
            yield return HalloweenKit.Pop(witch, 1.15f, 0.35f);
            say.Show("GORGEOUS! ...the nose stays, though.", 2.2f);
            yield return new WaitForSeconds(1.8f);
            // selfie!
            phone.SetActive(true);
            yield return HalloweenKit.Pop(phone.transform, 1.2f, 0.25f);
            say.Show("Selfie time! #NoFilter", 1.8f);
            yield return new WaitForSeconds(0.6f);
            HalloweenKit.SetAlpha(flash, 0.95f);
            Sfx.Play(this, "tick", 0.9f);
            Haptics.Light();
            yield return HalloweenKit.Fade(flash, 0f, 0.5f);
            HalloweenKit.Confetti(confetti, true);
            Sfx.Play(this, "ding", 0.8f);
            Haptics.Success();
            yield return new WaitForSeconds(1.4f);
            HalloweenKit.Confetti(confetti, false);
            if (logic.TryCompleteBrew()) Win();
        }
    }
}
