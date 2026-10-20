using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "The great escape" — un topo in cella, un gatto carceriere che si addormenta e si sveglia a turni. La
    /// soluzione: portare il cucchiaino sulla piastrella smossa (vicino al gatto) mentre lui dorme: la piastrella si
    /// alza e si apre il tunnel verso l'uscita. Tre movimenti nella sua zona di vista mentre è sveglio = sconfitta.
    /// Easter egg "Mouse Union": prima del tunnel, toccare le sbarre 2, 5, 1 e poi tenere premuta la ciotola 4 s:
    /// assemblea sindacale dei topi, il gatto eletto rappresentante firma per 23 ore di sonno al giorno.
    /// </summary>
    public class GreatEscapeLevel : LevelController
    {
        enum Cat { Asleep, Stirring, Awake }

        [SerializeField] Transform spoon, looseTile, mouse, cat, exitPoint, bowl;
        [SerializeField] Collider2D spoonArea, bowlArea;
        [SerializeField] Collider2D[] bars;
        [SerializeField] SpriteRenderer hole, catArt, cone, doorOpen;
        [SerializeField] Transform holdPivot;      // la barretta di carica (figlia di holdBar)
        [SerializeField] GameObject holdBar;
        [SerializeField] Sprite awakeSprite, sleepSprite;
        [SerializeField] GameObject alertMark;
        [SerializeField] SmokeEmitter zzz, dust;
        [SerializeField] LineRenderer tunnel;
        [SerializeField] TextMesh strikeText, banner;
        [SerializeField] SpeechBubble catBubble, mouseBubble;
        [SerializeField] SmokeEmitter[] confetti;
        // easter egg
        [SerializeField] GameObject[] protesterTemplates;
        [SerializeField] Transform[] spawnHoles;
        [SerializeField] Transform megaphone, contract;
        [SerializeField] int strikesAllowed = 3;
        [SerializeField] float zoneLeftX = -0.9f;

        static readonly int[] EggBars = { 1, 4, 0 };   // seconda, quinta, prima sbarra

        Camera cam;
        Cat state = Cat.Asleep;
        float stateTimer, moveInZone, holdTime;
        int strikes;
        bool busy, dragging, holding, armed, nommed;
        Vector2 dragOffset, lastSpoon;
        Vector3 spoonHome;
        readonly List<int> barTaps = new List<int>();

        public override void OnLevelStart()
        {
            cam = Camera.main;
            spoonHome = spoon.localPosition;
            hole.gameObject.SetActive(false);
            doorOpen.gameObject.SetActive(false);
            megaphone.gameObject.SetActive(false);
            contract.gameObject.SetActive(false);
            banner.text = "";
            foreach (GameObject g in protesterTemplates) g.SetActive(false);
            tunnel.positionCount = 0;
            holdBar.SetActive(false);
            S41LKit.Confetti(confetti, false);
            SetCat(Cat.Asleep, 3.5f);
            UpdateStrikes();
            StartCoroutine(Intro());
        }

        IEnumerator Intro()
        {
            yield return new WaitForSeconds(0.6f);
            if (!busy) S41LKit.Say(mouseBubble, "I need to get OUT of here!");
            yield return new WaitForSeconds(2.2f);
            if (!busy && mouseBubble != null) mouseBubble.Hide();
        }

        void UpdateStrikes() { strikeText.text = "SPOTTED " + strikes + "/" + strikesAllowed; }

        void SetCat(Cat s, float seconds)
        {
            state = s;
            stateTimer = seconds;
            catArt.sprite = s == Cat.Asleep ? sleepSprite : awakeSprite;
            cone.gameObject.SetActive(s == Cat.Awake);
            alertMark.SetActive(s == Cat.Stirring);
            if (s == Cat.Asleep) zzz.Play(); else zzz.Stop();
            if (s == Cat.Awake) Sfx.Play(this, "bonk", 0.4f);
        }

        void Update()
        {
            if (IsFinished || busy) return;
            // turni del gatto: dorme, si stiracchia (avviso), resta sveglio
            stateTimer -= Time.deltaTime;
            if (state == Cat.Awake) S41LKit.Alpha(cone, 0.75f + Mathf.Sin(Time.time * 6f) * 0.2f);
            if (stateTimer <= 0f)
            {
                if (state == Cat.Asleep) SetCat(Cat.Stirring, 1.1f);
                else if (state == Cat.Stirring) SetCat(Cat.Awake, Random.Range(2.4f, 3.0f));
                else SetCat(Cat.Asleep, Random.Range(4.0f, 5.5f));
            }
            HandleInput();
        }

        bool InZone(Vector2 p) => p.x > zoneLeftX && p.y < -0.3f;

        void HandleInput()
        {
            if (!S41LKit.Pointer(cam, out Vector2 w, out bool began, out bool ended))
            {
                if (dragging) DropSpoon();
                StopHold();
                return;
            }
            if (began)
            {
                if (spoonArea.OverlapPoint(w))
                {
                    dragging = true;
                    dragOffset = (Vector2)spoon.position - w;
                    lastSpoon = spoon.position;
                    moveInZone = 0f;
                    Sfx.Play(this, "tick", 0.5f);
                }
                else if (bowlArea.OverlapPoint(w)) { holding = true; holdTime = 0f; nommed = false; }
                else
                {
                    for (int i = 0; i < bars.Length; i++)
                        if (bars[i].OverlapPoint(w)) { TapBar(i); break; }
                }
            }
            if (dragging)
            {
                Vector2 p = w + dragOffset;
                p.x = Mathf.Clamp(p.x, -2.5f, 0.7f);
                p.y = Mathf.Clamp(p.y, -4.3f, -2.15f);
                spoon.position = new Vector3(p.x, p.y, spoon.position.z);
                float moved = Vector2.Distance(p, lastSpoon);
                lastSpoon = p;
                if (state == Cat.Awake && InZone(p))
                {
                    moveInZone += moved;
                    if (moveInZone > 0.12f) { dragging = false; StartCoroutine(Caught()); return; }
                }
                if (ended) DropSpoon();
            }
            if (holding)
            {
                if (ended || !bowlArea.OverlapPoint(w)) { StopHold(); return; }
                holdTime += Time.deltaTime;
                if (armed)
                {
                    holdBar.SetActive(true);
                    holdPivot.localScale = new Vector3(Mathf.Clamp01(holdTime / 4f), 1f, 1f);
                    if (holdTime >= 4f) { StopHold(); StartCoroutine(Union()); }
                }
                else if (holdTime > 0.5f && !nommed)
                {
                    nommed = true;
                    S41LKit.Say(mouseBubble, "Nom! Still hungry for FREEDOM.");
                    StartCoroutine(S41LKit.Pop(bowl, 0.12f));
                }
            }
        }

        void StopHold()
        {
            holding = false;
            holdTime = 0f;
            if (holdBar != null) holdBar.SetActive(false);
        }

        void TapBar(int i)
        {
            Sfx.Play(this, "ding", 0.25f);
            Haptics.Light();
            StartCoroutine(S41LKit.Shake(bars[i].transform, 0.04f, 0.3f));
            barTaps.Add(i);
            if (barTaps.Count > 3) barTaps.RemoveAt(0);
            bool match = barTaps.Count == 3;
            for (int k = 0; k < 3 && match; k++) match = barTaps[k] == EggBars[k];
            if (match && !armed)
            {
                armed = true;
                S41LKit.Say(mouseBubble, "Psst... the secret knock! Comrades?");
                StartCoroutine(S41LKit.Pop(mouse, 0.2f, 0.3f));
            }
        }

        void DropSpoon()
        {
            dragging = false;
            if (Vector2.Distance(spoon.position, looseTile.position) > 0.6f) { Sfx.Play(this, "tick", 0.4f); return; }
            if (state == Cat.Awake) { StartCoroutine(Caught()); return; }
            StartCoroutine(Pry());
        }

        IEnumerator Caught()
        {
            busy = true;
            strikes++;
            UpdateStrikes();
            Sfx.Play(this, "buzz", 0.7f);
            Haptics.Medium();
            alertMark.SetActive(true);
            S41LKit.Say(catBubble, strikes >= strikesAllowed ? "THAT'S IT! LOCKDOWN!" : strikes == 1 ? "HEY! I SAW THAT!" : "Again?! Drop the spoon!");
            StartCoroutine(S41LKit.Shake(cat, 0.08f, 0.4f));
            yield return StartCoroutine(S41LKit.Move(spoon, spoonHome, 0.5f, 0.5f));
            Sfx.Play(this, "clunk", 0.5f);
            yield return new WaitForSeconds(0.8f);
            alertMark.SetActive(false);
            if (strikes >= strikesAllowed) { yield return new WaitForSeconds(0.6f); Fail(); yield break; }
            catBubble.Hide();
            SetCat(Cat.Asleep, 3.5f);   // si rimette a dormire (soddisfatto)
            busy = false;
        }

        IEnumerator Pry()
        {
            busy = true;
            SetCat(Cat.Asleep, 99f);
            Vector3 edge = looseTile.localPosition + new Vector3(-0.35f, -0.3f, 0f);
            yield return StartCoroutine(S41LKit.Move(spoon, edge, 0.2f));
            // leva: il cucchiaino ruota e la piastrella si solleva
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.6f)
            {
                spoon.localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(0f, 35f, k));
                looseTile.localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(0f, 12f, k));
                yield return null;
            }
            Sfx.Play(this, "clunk", 0.8f);
            Haptics.Light();
            hole.gameObject.SetActive(true);
            dust.transform.position = looseTile.position;
            dust.Play();
            yield return StartCoroutine(S41LKit.Move(looseTile, looseTile.localPosition + new Vector3(-0.9f, -0.15f, 0f), 0.4f, 0.4f));
            dust.Stop();
            spoon.localRotation = Quaternion.identity;
            S41LKit.Say(mouseBubble, "A TUNNEL! Shhh...");
            // il topo corre al buco
            Vector3 holeLocal = hole.transform.localPosition;
            yield return StartCoroutine(S41LKit.Move(mouse, holeLocal + new Vector3(0f, 0.05f, 0f), 0.9f, 0.3f));
            Vector3 ms = mouse.localScale;
            yield return StartCoroutine(S41LKit.ScaleTo(mouse, ms * 0.3f, 0.3f));
            mouse.gameObject.SetActive(false);
            mouseBubble.Hide();
            // il tunnel si scava sotto il pavimento fino all'uscita
            Vector3 a = hole.transform.position, c = exitPoint.position;
            Vector3[] path = { a, new Vector3(a.x, -4.45f, 0f), new Vector3(c.x, -4.45f, 0f), c };
            tunnel.positionCount = 0;
            for (int i = 1; i < path.Length; i++)
            {
                for (float k = 0f; k < 1f; k += Time.deltaTime / 0.5f)
                {
                    tunnel.positionCount = i + 1;
                    for (int j = 0; j < i; j++) tunnel.SetPosition(j, path[j]);
                    tunnel.SetPosition(i, Vector3.Lerp(path[i - 1], path[i], k));
                    if (Random.value < 0.2f) Sfx.Play(this, "tick", 0.3f);
                    yield return null;
                }
                tunnel.SetPosition(i, path[i]);
            }
            // esce dalla tana dell'uscita
            mouse.position = exitPoint.position;
            mouse.localScale = ms * 0.3f;
            mouse.gameObject.SetActive(true);
            yield return StartCoroutine(S41LKit.ScaleTo(mouse, ms, 0.3f));
            Sfx.Play(this, "ding", 0.9f);
            Haptics.Medium();
            mouseBubble.transform.position = new Vector3(Mathf.Clamp(mouse.position.x, -1.6f, 1.6f), mouse.position.y + 1.3f, mouseBubble.transform.position.z);
            S41LKit.Say(mouseBubble, "FREEDOM!");
            S41LKit.Confetti(confetti, true);
            yield return StartCoroutine(S41LKit.Pop(mouse, 0.3f, 0.4f));
            yield return new WaitForSeconds(1.8f);
            S41LKit.Confetti(confetti, false);
            Win();
        }

        // ---------- Easter egg: il sindacato dei topi ----------

        IEnumerator Union()
        {
            busy = true;
            dragging = false;
            SetCat(Cat.Asleep, 99f);
            Sfx.Play(this, "ding", 0.8f);
            megaphone.gameObject.SetActive(true);
            megaphone.position = mouse.position + new Vector3(0.35f, 0.35f, 0f);
            S41LKit.Say(mouseBubble, "MOUSE UNION ASSEMBLY!");
            Haptics.Medium();
            yield return new WaitForSeconds(1.2f);

            // decine di topini escono dai muri coi cartelli
            var crowd = new List<Transform>();
            var targets = new List<Vector3>();
            for (int i = 0; i < 27; i++)
            {
                GameObject tpl = protesterTemplates[i % protesterTemplates.Length];
                Transform hole0 = spawnHoles[i % spawnHoles.Length];
                GameObject g = Instantiate(tpl, tpl.transform.parent);
                g.SetActive(true);
                g.transform.position = hole0.position;
                crowd.Add(g.transform);
                targets.Add(new Vector3(-2.35f + (i % 9) * 0.58f + (i / 9 % 2) * 0.25f, -2.45f - (i / 9) * 0.6f, 0f));
            }
            for (float k = 0f; k < 1f; k += Time.deltaTime / 1.6f)
            {
                for (int i = 0; i < crowd.Count; i++)
                {
                    float d = Mathf.Clamp01(k * 1.6f - (i % 9) * 0.07f);
                    crowd[i].position = Vector3.Lerp(spawnHoles[i % spawnHoles.Length].position, targets[i], d) + Vector3.up * Mathf.Abs(Mathf.Sin(d * 12f)) * 0.1f;
                }
                if (Random.value < 0.3f) Sfx.Play(this, "tick", 0.4f);
                yield return null;
            }
            banner.text = "MORE CHEESE! LESS CATS! PAID NAPS!";
            yield return Chant(crowd, targets, 1.6f);

            // il gatto si sveglia e prova a intervenire
            SetCat(Cat.Awake, 99f);
            cone.gameObject.SetActive(false);
            alertMark.SetActive(true);
            S41LKit.Say(catBubble, "WHAT IS GOING ON HERE?!");
            Haptics.Light();
            yield return StartCoroutine(S41LKit.Move(cat, cat.localPosition + new Vector3(-0.3f, 0f, 0f), 0.4f, 0.2f));
            yield return new WaitForSeconds(0.9f);
            alertMark.SetActive(false);
            // gli consegnano il megafono... e lo eleggono per sbaglio
            Vector3 to = cat.position + new Vector3(-0.45f, 0.85f, 0f);
            Vector3 from = megaphone.position;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.6f)
            {
                megaphone.position = Vector3.Lerp(from, to, k) + Vector3.up * Mathf.Sin(k * Mathf.PI) * 0.8f;
                yield return null;
            }
            S41LKit.Say(mouseBubble, "All in favor of HIM as staff rep?");
            yield return new WaitForSeconds(1.2f);
            banner.text = "AYE! AYE! AYE!";
            yield return Chant(crowd, targets, 1.0f);
            banner.text = "NEW STAFF REPRESENTATIVE: THE CAT";
            Sfx.Play(this, "ding", 0.8f);
            S41LKit.Say(catBubble, "...me?");
            yield return new WaitForSeconds(1.4f);
            // il contratto: 23 ore di sonno al giorno
            contract.gameObject.SetActive(true);
            Vector3 cs = contract.localScale;
            contract.localScale = cs * 0.2f;
            yield return StartCoroutine(S41LKit.ScaleTo(contract, cs, 0.35f));
            S41LKit.Say(catBubble, "23 hours of sleep? ...Where do I sign?");
            yield return new WaitForSeconds(1.6f);
            for (int i = 0; i < 4; i++) { Sfx.Play(this, "tick", 0.7f); yield return new WaitForSeconds(0.12f); }
            Sfx.Play(this, "clunk", 0.8f);
            Haptics.Light();
            yield return StartCoroutine(S41LKit.Pop(contract, 0.15f));
            yield return new WaitForSeconds(0.6f);
            contract.gameObject.SetActive(false);
            catBubble.Hide();
            // il gatto torna alla scrivania e si addormenta sopra
            megaphone.gameObject.SetActive(false);
            yield return StartCoroutine(S41LKit.Move(cat, cat.localPosition + new Vector3(0.3f, 0f, 0f), 0.4f, 0.15f));
            SetCat(Cat.Asleep, 99f);
            cat.localRotation = Quaternion.Euler(0, 0, 12f);
            banner.text = "MEETING ADJOURNED";
            // la porta principale si apre e i topi escono in fila
            doorOpen.gameObject.SetActive(true);
            Sfx.Play(this, "clunk", 0.7f);
            mouseBubble.Hide();
            crowd.Add(mouse);
            targets.Add(mouse.position);
            Vector3 door = doorOpen.transform.position + new Vector3(0f, -1.2f, 0f);
            var starts = new List<Vector3>();
            foreach (Transform t in crowd) starts.Add(t.position);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 2.6f)
            {
                for (int i = 0; i < crowd.Count; i++)
                {
                    float d = Mathf.Clamp01(k * 1.8f - i * 0.03f);
                    crowd[i].position = Vector3.Lerp(starts[i], door, d) + Vector3.up * Mathf.Abs(Mathf.Sin(d * 14f)) * 0.08f;
                    if (d >= 1f) crowd[i].gameObject.SetActive(false);
                }
                yield return null;
            }
            S41LKit.Confetti(confetti, true);
            yield return new WaitForSeconds(1.4f);
            S41LKit.Confetti(confetti, false);
            WinWithEgg("mouse_union");
        }

        IEnumerator Chant(List<Transform> crowd, List<Vector3> targets, float seconds)
        {
            Haptics.Light();
            for (float t = 0f; t < seconds; t += Time.deltaTime)
            {
                for (int i = 0; i < crowd.Count; i++)
                    crowd[i].position = targets[i] + Vector3.up * Mathf.Abs(Mathf.Sin(t * 9f + i)) * 0.12f;
                yield return null;
            }
        }
    }
}
