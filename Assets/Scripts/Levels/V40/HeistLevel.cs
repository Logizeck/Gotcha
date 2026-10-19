using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello composito "Bank heist" — 3 fasi.
    /// 1) Nel caveau c'è un quadro: il dito muove una torcia UV, e solo dentro il suo cerchio di luce (maschera) si
    ///    vede un codice di 4 cifre scritto con l'inchiostro invisibile (a caso a ogni partita, in un punto a caso
    ///    del quadro). La targhetta "EST. 1887" è un'esca. Ogni cifra illuminata finisce sul bigliettino.
    /// 2) Tastierino del portellone: digitare il codice e OK (3 tentativi; finiti = allarme, si perde).
    /// 3) Il portellone resta bloccato dall'allarme: 4 fili colorati (ordine a caso) e un indovinello scritto a mano
    ///    (uno a caso tra quattro) dicono quale tagliare. Filo sbagliato = sirena, vibrazione forte, si perde.
    /// Vittoria: il volante gira, il portellone si apre e piovono monete d'oro.
    /// </summary>
    public class HeistLevel : LevelController
    {
        [SerializeField] StageHud40A hud;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] Transform wallScene;
        [SerializeField] Transform doorScene;
        [Header("Fase 1")]
        [SerializeField] SpriteRenderer[] inkDigits;
        [SerializeField] Sprite[] inkSprites;           // heist_ink_0..9
        [SerializeField] Transform[] inkSpots;          // dove può stare la riga del codice (figli del quadro)
        [SerializeField] Transform torch;               // torcia + luce viola + maschera (la luce è nell'origine)
        [SerializeField] TextMesh noteText;
        [Header("Fase 2")]
        [SerializeField] Transform keypad;
        [SerializeField] Collider2D[] keys;             // 0-9, 10 = C, 11 = OK
        [SerializeField] TextMesh display;
        [SerializeField] Transform wheel;
        [Header("Fase 3")]
        [SerializeField] Transform alarmBox;
        [SerializeField] Transform[] wires;             // figli "Top" e "Bot" (SpriteRenderer bianchi da tingere)
        [SerializeField] SpriteRenderer alarmLight;
        [SerializeField] TextMesh riddleText;
        [SerializeField] Transform riddleNote;
        [SerializeField] SpriteRenderer redFlash;
        [Header("Vittoria")]
        [SerializeField] Transform vaultDoor;
        [SerializeField] Transform[] coins;

        static readonly string[] ColorNames = { "red", "blue", "green", "yellow" };
        static readonly Color[] WireColors = { new Color(0.93f, 0.27f, 0.25f), new Color(0.27f, 0.52f, 0.95f), new Color(0.3f, 0.78f, 0.35f), new Color(1f, 0.85f, 0.2f) };

        Camera cam;
        string code = "";
        bool[] found;
        int[] order = new int[4];      // colore di ogni filo (da sinistra)
        int answer;                    // indice del filo da tagliare
        float bubbleUntil;
        Vector3 torchHome;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            for (int i = 0; i < 4; i++) code += (char)('0' + Random.Range(0, 10));
            // la riga del codice in uno dei posti possibili del quadro
            Transform spot = inkSpots[Random.Range(0, inkSpots.Length)];
            for (int i = 0; i < inkDigits.Length; i++)
            {
                Transform d = inkDigits[i].transform;
                d.SetParent(spot, false);
                d.localPosition = new Vector3((i - 1.5f) * 0.36f, Random.Range(-0.04f, 0.04f), 0f);
                d.localRotation = Quaternion.Euler(0, 0, Random.Range(-10f, 10f));
                inkDigits[i].sprite = inkSprites[code[i] - '0'];
            }
            found = new bool[inkDigits.Length];
            torchHome = torch.localPosition;
            doorScene.gameObject.SetActive(false);
            redFlash.enabled = false;
            alarmBox.gameObject.SetActive(false);
            riddleNote.gameObject.SetActive(false);
            foreach (Transform c in coins) c.gameObject.SetActive(false);
            UpdateNote();
            StartCoroutine(Run());
        }

        void Say(string text, float seconds)
        {
            if (bubble == null) return;
            bubble.Show(text);
            bubbleUntil = Time.time + seconds;
        }

        void Update()
        {
            if (bubble != null && bubbleUntil > 0f && Time.time > bubbleUntil) { bubble.Hide(); bubbleUntil = 0f; }
        }

        IEnumerator Run()
        {
            yield return Stage1Painting();
            yield return hud.Clear("CODE FOUND!");
            yield return SlideToDoor();
            bool ok = false;
            yield return Stage2Keypad(r => ok = r);
            if (!ok) yield break;
            yield return hud.Clear("ACCESS GRANTED!");
            yield return Stage3Wires(r => ok = r);
            if (!ok) yield break;
            yield return hud.Clear("ALARM OFF!");
            yield return OpenVault();
        }

        void UpdateNote()
        {
            string s = "";
            for (int i = 0; i < code.Length; i++) s += (found[i] ? code[i].ToString() : "_") + (i < code.Length - 1 ? " " : "");
            noteText.text = s;
        }

        // ---------- Fase 1: il quadro e la torcia UV ----------

        IEnumerator Stage1Painting()
        {
            hud.Stage(0, "Find the secret code! Drag the UV light");
            Say("The code is written in INVISIBLE ink...", 2.6f);
            var seen = new float[inkDigits.Length];
            int count = 0;
            while (count < inkDigits.Length)
            {
                bool down = PointerInput.TryGetPrimary(out Vector2 sp, out _, out _) && !PointerInput.IsOverUi();
                Vector3 target = torchHome;
                if (down)
                {
                    Vector2 w = PointerInput.ToWorld(cam, sp);
                    target = torch.parent.InverseTransformPoint(new Vector3(w.x, w.y + 0.9f, 0f));   // la luce sopra il dito
                    target.z = torchHome.z;
                }
                torch.localPosition = Vector3.Lerp(torch.localPosition, target, 18f * Time.deltaTime);
                for (int i = 0; i < inkDigits.Length; i++)
                {
                    if (found[i]) continue;
                    if (Vector2.Distance(torch.position, inkDigits[i].transform.position) < 0.5f) seen[i] += Time.deltaTime;
                    if (seen[i] > 0.3f)
                    {
                        found[i] = true;
                        count++;
                        UpdateNote();
                        Sfx.Play(this, "pop", 0.6f);
                        Haptics.Light();
                        if (count == 1) Say("A number! Keep looking nearby...", 1.8f);
                    }
                }
                yield return null;
            }
            yield return new WaitForSeconds(0.4f);
            Say("Code: " + code + ". To the vault door!", 2f);
            yield return new WaitForSeconds(0.6f);
        }

        IEnumerator SlideToDoor()
        {
            doorScene.gameObject.SetActive(true);
            Vector3 wallHome = wallScene.localPosition, doorHome = doorScene.localPosition;
            Sfx.Play(this, "whoosh", 0.5f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.6f)
            {
                float e = k * k * (3f - 2f * k);
                wallScene.localPosition = wallHome + Vector3.left * 7f * e;
                doorScene.localPosition = doorHome + Vector3.right * 7f * (1f - e);
                yield return null;
            }
            wallScene.gameObject.SetActive(false);
            doorScene.localPosition = doorHome;
        }

        // ---------- Fase 2: il tastierino ----------

        IEnumerator Stage2Keypad(System.Action<bool> done)
        {
            hud.Stage(1, "Type the code on the keypad");
            string typed = "";
            display.text = "----";
            while (true)
            {
                if (PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out _) && began && !PointerInput.IsOverUi())
                {
                    Vector2 w = PointerInput.ToWorld(cam, sp);
                    for (int i = 0; i < keys.Length; i++)
                    {
                        if (!keys[i].OverlapPoint(w)) continue;
                        StartCoroutine(Press(keys[i].transform));
                        Sfx.Play(this, "tick", 0.7f);
                        if (i < 10) { if (typed.Length < 4) typed += (char)('0' + i); }
                        else if (i == 10) typed = "";
                        else
                        {
                            if (typed == code) { display.color = new Color(0.5f, 1f, 0.5f); display.text = "OPEN"; Sfx.Play(this, "clunk", 0.8f); yield return new WaitForSeconds(0.5f); done(true); yield break; }
                            Sfx.Play(this, "buzz", 0.6f);
                            Haptics.Medium();
                            display.color = new Color(1f, 0.4f, 0.4f);
                            display.text = "ERROR";
                            int left = hud.LoseTry();
                            if (left <= 0)
                            {
                                yield return Busted("Too many wrong codes! ALARM!");
                                done(false);
                                yield break;
                            }
                            Say(typed == "1887" ? "1887 is just the year on the plaque!" : left + (left == 1 ? " try left!" : " tries left!"), 1.8f);
                            yield return new WaitForSeconds(0.7f);
                            display.color = new Color(0.55f, 1f, 0.7f);
                            typed = "";
                        }
                        display.text = typed.PadRight(4, '-');
                        break;
                    }
                }
                yield return null;
            }
        }

        IEnumerator Press(Transform k)
        {
            Vector3 s = k.localScale;
            k.localScale = s * 0.88f;
            yield return new WaitForSeconds(0.1f);
            k.localScale = s;
        }

        // ---------- Fase 3: i fili dell'allarme ----------

        void MakeRiddle()
        {
            int kind = Random.Range(0, 4);
            for (int guard = 0; guard < 500; guard++)
            {
                for (int i = 0; i < 4; i++) order[i] = i;
                for (int i = 3; i > 0; i--) { int j = Random.Range(0, i + 1); (order[i], order[j]) = (order[j], order[i]); }
                int red = System.Array.IndexOf(order, 0), blue = System.Array.IndexOf(order, 1), green = System.Array.IndexOf(order, 2), yellow = System.Array.IndexOf(order, 3);
                switch (kind)
                {
                    case 0:   // rosso subito a sinistra del giallo
                        if (red != yellow - 1) continue;
                        riddleText.text = "Not the colour of the sky,\nnot the colour of grass:\nthe wire LEFT of the yellow one.";
                        answer = red;
                        return;
                    case 1:   // blu nel mezzo, subito a destra del verde
                        if (blue < 1 || blue > 2 || green != blue - 1) continue;
                        riddleText.text = "Not on the ends.\nNot warm like fire or sun.\nIt's just RIGHT of the green one.";
                        answer = blue;
                        return;
                    case 2:   // tra rosso e verde, quello più a sinistra
                        riddleText.text = "Leave the sky. Leave the sun.\nOf the two wires left,\ncut the one more to the LEFT.";
                        answer = Mathf.Min(red, green);
                        return;
                    default:   // il filo "freddo" più lontano dal rosso
                        if (Mathf.Abs(blue - red) == Mathf.Abs(green - red)) continue;
                        riddleText.text = "Red & yellow are warm,\nblue & green are cold.\nCut the COLD wire FARTHEST from red.";
                        answer = Mathf.Abs(blue - red) > Mathf.Abs(green - red) ? blue : green;
                        return;
                }
            }
            // non dovrebbe mai servire: ripiego sull'indovinello sempre valido
            riddleText.text = "Leave the sky. Leave the sun.\nOf the two wires left,\ncut the one more to the LEFT.";
            answer = Mathf.Min(System.Array.IndexOf(order, 0), System.Array.IndexOf(order, 2));
        }

        IEnumerator Stage3Wires(System.Action<bool> done)
        {
            // il portellone non si apre: c'è l'allarme
            Sfx.Play(this, "clunk", 0.6f);
            for (float k = 0f; k < 0.5f; k += Time.deltaTime) { wheel.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(k * 30f) * 6f); yield return null; }
            wheel.localRotation = Quaternion.identity;
            Say("Stuck! The ALARM is still on...", 2f);
            MakeRiddle();
            for (int i = 0; i < wires.Length; i++)
                foreach (SpriteRenderer sr in wires[i].GetComponentsInChildren<SpriteRenderer>()) sr.color = WireColors[order[i]];
            Vector3 padHome = keypad.localPosition, boxHome = alarmBox.localPosition, noteHome = riddleNote.localPosition;
            alarmBox.gameObject.SetActive(true);
            riddleNote.gameObject.SetActive(true);
            Sfx.Play(this, "whoosh", 0.5f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.5f)
            {
                float e = k * k * (3f - 2f * k);
                keypad.localPosition = padHome + Vector3.down * 6f * e;
                alarmBox.localPosition = boxHome + Vector3.down * 6f * (1f - e);
                riddleNote.localPosition = noteHome + Vector3.left * 6f * (1f - e);
                yield return null;
            }
            alarmBox.localPosition = boxHome;
            riddleNote.localPosition = noteHome;
            keypad.gameObject.SetActive(false);
            hud.Stage(2, "Read the note. Tap ONE wire to cut it");

            while (true)
            {
                alarmLight.color = new Color(1f, 0.25f, 0.25f, 0.45f + Mathf.Sin(Time.time * 6f) * 0.25f);
                if (PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out _) && began && !PointerInput.IsOverUi())
                {
                    Vector2 w = PointerInput.ToWorld(cam, sp);
                    for (int i = 0; i < wires.Length; i++)
                    {
                        Collider2D c = wires[i].GetComponent<Collider2D>();
                        if (c == null || !c.OverlapPoint(w)) continue;
                        yield return Snip(wires[i]);
                        if (i == answer)
                        {
                            alarmLight.color = new Color(0.4f, 1f, 0.45f, 0.8f);
                            Say("Phew... the alarm is OFF.", 1.8f);
                            yield return new WaitForSeconds(0.6f);
                            done(true);
                        }
                        else
                        {
                            yield return Busted("WRONG WIRE! BUSTED!");
                            done(false);
                        }
                        yield break;
                    }
                }
                yield return null;
            }
        }

        IEnumerator Snip(Transform wire)
        {
            Sfx.Play(this, "pop", 0.9f);
            Sfx.Play(this, "tick", 0.9f);
            Transform top = wire.Find("Top"), bot = wire.Find("Bot");
            Vector3 tp = top.localPosition, bp = bot.localPosition;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.25f)
            {
                top.localPosition = tp + new Vector3(0.05f, 0.12f, 0f) * k;
                top.localRotation = Quaternion.Euler(0, 0, 12f * k);
                bot.localPosition = bp + new Vector3(-0.05f, -0.12f, 0f) * k;
                bot.localRotation = Quaternion.Euler(0, 0, -12f * k);
                yield return null;
            }
            yield return new WaitForSeconds(0.3f);
        }

        /// <summary>Allarme: sirena, schermo rosso che lampeggia, vibrazione forte, poi si perde.</summary>
        IEnumerator Busted(string msg)
        {
            Synth40A.Loop(this, "siren", 0.6f);
            Haptics.Heavy();
            Say(msg, 3f);
            redFlash.enabled = true;
            for (float k = 0f; k < 2.2f; k += Time.deltaTime)
            {
                float a = 0.15f + Mathf.Abs(Mathf.Sin(k * 7f)) * 0.35f;
                redFlash.color = new Color(1f, 0.1f, 0.1f, a);
                if (alarmLight != null) alarmLight.color = new Color(1f, 0.2f, 0.2f, a * 2f);
                yield return null;
            }
            Fail();
        }

        // ---------- Vittoria ----------

        IEnumerator OpenVault()
        {
            Sfx.Play(this, "clunk", 0.8f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 1.0f) { wheel.localRotation = Quaternion.Euler(0, 0, -360f * k * k); yield return null; }
            Sfx.Play(this, "whoosh", 0.7f);
            Vector3 home = vaultDoor.localPosition, scale = vaultDoor.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.7f)
            {
                float e = 1f - (1f - k) * (1f - k);
                vaultDoor.localScale = new Vector3(scale.x * Mathf.Lerp(1f, 0.15f, e), scale.y, scale.z);
                vaultDoor.localPosition = home + Vector3.left * 1.55f * scale.x * e;
                yield return null;
            }
            Say("We're RICH!", 2.5f);
            // pioggia di monete
            var vel = new float[coins.Length];
            var landed = new bool[coins.Length];
            for (int i = 0; i < coins.Length; i++)
            {
                coins[i].gameObject.SetActive(true);
                coins[i].localPosition = new Vector3(Random.Range(-2.5f, 2.5f), 4.5f + Random.Range(0f, 3.5f), 0f);
            }
            int bumps = 0;
            for (float t = 0f; t < 2.6f; t += Time.deltaTime)
            {
                for (int i = 0; i < coins.Length; i++)
                {
                    if (landed[i]) continue;
                    vel[i] -= 12f * Time.deltaTime;
                    Vector3 p = coins[i].localPosition + Vector3.up * vel[i] * Time.deltaTime;
                    coins[i].localRotation = Quaternion.Euler(0, 0, p.y * 90f);
                    float floor = -3.6f - (i % 4) * 0.2f;
                    if (p.y < floor)
                    {
                        p.y = floor;
                        landed[i] = true;
                        if (bumps++ % 3 == 0) { Haptics.Light(); Sfx.Play(this, "tick", 0.6f); }
                    }
                    coins[i].localPosition = p;
                }
                yield return null;
            }
            Sfx.Play(this, "ding", 0.8f);
            yield return new WaitForSeconds(0.4f);
            Win();
        }
    }
}
