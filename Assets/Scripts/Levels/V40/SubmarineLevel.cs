using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello composito "Submarine" — 3 fasi.
    /// 1) Il sottomarino scende in una grotta: si guida inclinando il telefono a destra e a sinistra. Ogni urto
    ///    contro la roccia è un danno allo scafo (3 cuori, finiti = si perde). L'ossigeno cala: quando è basso il
    ///    fumetto dice "Blow to pump air!" e bisogna SOFFIARE nel microfono per riempirlo (a zero si perde).
    /// 2) Al relitto: il pulsante PING (3 ping) manda un'onda del sonar; quando l'onda passa sul masso giusto si
    ///    accende per un attimo un'eco verde. Toccare il masso: quello sbagliato nasconde una murena che morde
    ///    (un cuore in meno).
    /// 3) Il forziere ha una serratura a 3 rotelle (pesce, ancora, stella, conchiglia): la combinazione è disegnata
    ///    sulla bandiera del relitto. Toccare le rotelle per cambiarne il simbolo. Vittoria: il forziere si apre.
    /// </summary>
    public class SubmarineLevel : LevelController
    {
        [SerializeField] StageHud40A hud;
        [SerializeField] SpeechBubble bubble;
        [Header("Fase 1")]
        [SerializeField] GameObject caveScene;
        [SerializeField] Transform track;
        [SerializeField] float[] path;               // terne (y, centro x, mezza larghezza) del tunnel
        [SerializeField] Transform sub;
        [SerializeField] SpriteRenderer subArt;
        [SerializeField] Transform oxygenBar;
        [SerializeField] SpriteRenderer oxygenFill;
        [SerializeField] SmokeEmitter airBubbles;
        [SerializeField] float scrollSpeed = 0.85f, steer = 4.5f, drain = 0.05f, refill = 0.45f, blowLevel = 0.2f;
        [Header("Fase 2")]
        [SerializeField] GameObject floorScene;
        [SerializeField] Transform subParking;
        [SerializeField] Transform[] rocks;
        [SerializeField] Transform eel;
        [SerializeField] Collider2D pingButton;
        [SerializeField] Transform ring;
        [SerializeField] SpriteRenderer blip;
        [SerializeField] TextMesh pingsLabel;
        [SerializeField] Transform smallChest;
        [Header("Fase 3")]
        [SerializeField] SpriteRenderer[] flagSymbols;
        [SerializeField] Sprite[] symbolSprites;     // pesce, ancora, stella, conchiglia
        [SerializeField] GameObject lockScene;
        [SerializeField] SpriteRenderer dim;
        [SerializeField] SpriteRenderer bigChest;
        [SerializeField] Sprite chestOpen;
        [SerializeField] Collider2D[] dials;
        [SerializeField] SpriteRenderer[] dialSymbols;
        [SerializeField] SmokeEmitter treasureBubbles;

        const float TunnelEnd = -26f;

        Camera cam;
        float oxygen = 1f, bubbleUntil;
        int[] combo = new int[3], dialValue = new int[3];
        bool lost;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            floorScene.SetActive(false);
            lockScene.SetActive(false);
            for (int i = 0; i < 3; i++)
            {
                combo[i] = Random.Range(0, symbolSprites.Length);
                flagSymbols[i].sprite = symbolSprites[combo[i]];
            }
            StartCoroutine(Run());
        }

        void OnDestroy() { Inputs.ReleaseMic(this); }

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
            yield return Stage1Cave();
            if (lost) yield break;
            yield return hud.Clear("WRECK FOUND!");
            yield return Stage2Sonar();
            if (lost) yield break;
            yield return hud.Clear("TREASURE!");
            yield return Stage3Lock();
        }

        /// <summary>Un cuore in meno allo scafo; a zero si perde.</summary>
        IEnumerator Damage(string msg)
        {
            Haptics.Medium();
            Sfx.Play(this, "bonk", 0.8f);
            int left = hud.LoseTry();
            Say(left > 0 ? msg : "Hull breached! Glub glub...", 1.6f);
            for (float k = 0f; k < 0.6f; k += Time.deltaTime)
            {
                subArt.color = Mathf.Repeat(k, 0.15f) < 0.075f ? new Color(1f, 0.45f, 0.45f) : Color.white;
                yield return null;
            }
            subArt.color = Color.white;
            if (left <= 0) yield return Lose();
        }

        IEnumerator Lose()
        {
            lost = true;
            Inputs.ReleaseMic(this);
            if (airBubbles != null) airBubbles.Stop();
            Vector3 p = sub.localPosition;
            for (float k = 0f; k < 1.2f; k += Time.deltaTime)
            {
                sub.localPosition = p + Vector3.down * k * k * 1.5f;
                sub.localRotation = Quaternion.Euler(0, 0, -k * 30f);
                yield return null;
            }
            Fail();
        }

        // ---------- Fase 1: la grotta ----------

        IEnumerator Stage1Cave()
        {
            hud.Stage(0, "Tilt to steer down to the wreck!");
            Say("Tilt left and right. Don't scratch the paint!", 2.4f);
            float scroll = 0f, nag = 0f;
            float subY = sub.localPosition.y;
            bool warned = false;
            while (true)
            {
                float dt = Time.deltaTime;
                scroll += scrollSpeed * dt;
                track.localPosition = new Vector3(0f, scroll, 0f);
                float ty = subY - scroll;
                if (ty < TunnelEnd) break;

                // sterzo
                float tx = Mathf.Clamp(Inputs.Tilt().x, -1f, 1f);
                Vector3 p = sub.localPosition;
                p.x = Mathf.Clamp(p.x + tx * steer * dt, -2.6f, 2.6f);
                sub.localPosition = p;
                sub.localRotation = Quaternion.Euler(0, 0, -tx * 15f);
                subArt.flipX = tx < -0.05f;

                // urti con la roccia (si controlla il muso e la coda del sottomarino)
                float worst = 0f, push = 0f;
                foreach (float dy in new[] { -0.3f, 0f, 0.3f })
                {
                    Gotcha.Levels.TunnelMath40A.At(path, ty + dy, out float cx, out float w);
                    float over = Mathf.Abs(p.x - cx) - (w - 0.55f);
                    if (over > worst) { worst = over; push = cx - p.x; }
                }
                if (worst > 0f)
                {
                    p.x += Mathf.Sign(push) * (worst + 0.35f);
                    sub.localPosition = p;
                    yield return Damage("BONK! Hull damage!");
                    if (lost) yield break;
                }

                // ossigeno
                float mic = Inputs.Mic(this);
                bool blowing = mic > blowLevel;
                oxygen = Mathf.Clamp01(oxygen + (blowing ? refill : -drain) * dt);
                if (blowing && !airBubbles.IsPlaying) airBubbles.Play();
                else if (!blowing && airBubbles.IsPlaying) airBubbles.Stop();
                oxygenBar.localScale = new Vector3(oxygen, 1f, 1f);
                oxygenFill.color = oxygen < 0.35f ? (Mathf.Repeat(Time.time, 0.5f) < 0.25f ? new Color(1f, 0.3f, 0.3f) : new Color(1f, 0.7f, 0.3f)) : new Color(0.45f, 0.85f, 1f);
                nag -= dt;
                if (oxygen < 0.4f && !blowing && nag <= 0f)
                {
                    nag = 2.6f;
                    Say("Low oxygen! Blow to pump air!", 2.2f);
                    if (!warned) { warned = true; hud.Goal("BLOW into the mic to pump air!"); Haptics.Light(); }
                }
                if (blowing && warned && oxygen > 0.8f) { warned = false; hud.Goal("Tilt to steer down to the wreck!"); }
                if (oxygen <= 0f) { Say("Out of air!", 2f); yield return Lose(); yield break; }
                yield return null;
            }
            Inputs.ReleaseMic(this);
            airBubbles.Stop();
        }

        // ---------- Fase 2: il sonar ----------

        IEnumerator Stage2Sonar()
        {
            yield return hud.Fade(1f, 0.3f);
            caveScene.SetActive(false);
            floorScene.SetActive(true);
            sub.localPosition = subParking.localPosition;
            sub.localRotation = Quaternion.identity;
            subArt.flipX = false;
            ring.gameObject.SetActive(false);
            blip.enabled = false;
            smallChest.gameObject.SetActive(false);
            eel.gameObject.SetActive(false);
            yield return hud.Fade(0f, 0.3f);
            hud.Stage(1, "PING the sonar, then tap the rock with the echo");
            Say("The treasure is under one of these rocks...", 2.4f);

            int treasure = Random.Range(0, rocks.Length);
            int pings = 3;
            bool[] tried = new bool[rocks.Length];
            pingsLabel.text = "PINGS: " + pings;
            while (true)
            {
                if (PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out _) && began && !PointerInput.IsOverUi())
                {
                    Vector2 w = PointerInput.ToWorld(cam, sp);
                    if (pingButton.OverlapPoint(w))
                    {
                        if (pings > 0 && !ring.gameObject.activeSelf)
                        {
                            pings--;
                            pingsLabel.text = "PINGS: " + pings;
                            StartCoroutine(Ping(rocks[treasure]));
                        }
                        else if (pings == 0) Say("Sonar battery empty! Trust your memory.", 2f);
                    }
                    for (int i = 0; i < rocks.Length; i++)
                    {
                        Collider2D c = rocks[i].GetComponent<Collider2D>();
                        if (tried[i] || c == null || !c.OverlapPoint(w)) continue;
                        tried[i] = true;
                        if (i == treasure)
                        {
                            yield return LiftRock(rocks[i], true);
                            yield break;
                        }
                        yield return LiftRock(rocks[i], false);
                        if (lost) yield break;
                        break;
                    }
                }
                yield return null;
            }
        }

        IEnumerator Ping(Transform target)
        {
            Synth40A.Play(this, "ping", 0.7f);
            ring.gameObject.SetActive(true);
            ring.position = sub.position;
            SpriteRenderer rr = ring.GetComponentInChildren<SpriteRenderer>();
            float dist = Vector2.Distance(sub.position, target.position + Vector3.up * 0.4f);
            bool echoed = false;
            for (float r = 0.2f; r < 9f; r += Time.deltaTime * 5f)
            {
                ring.localScale = Vector3.one * r;
                if (rr != null) rr.color = new Color(1f, 1f, 1f, Mathf.Clamp01(1.2f - r / 9f));
                if (!echoed && r >= dist) { echoed = true; StartCoroutine(Blip(target)); }
                yield return null;
            }
            ring.gameObject.SetActive(false);
        }

        IEnumerator Blip(Transform target)
        {
            blip.enabled = true;
            blip.transform.position = new Vector3(target.position.x, target.position.y + 0.4f, blip.transform.position.z);
            Synth40A.Play(this, "ping", 0.18f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.8f)
            {
                blip.color = new Color(0.5f, 1f, 0.6f, Mathf.Sin(k * Mathf.PI) * 0.75f);
                blip.transform.localScale = Vector3.one * (1f + k * 0.4f);
                yield return null;
            }
            blip.enabled = false;
        }

        IEnumerator LiftRock(Transform rock, bool good)
        {
            Sfx.Play(this, "clunk", 0.6f);
            Vector3 home = rock.localPosition, worldHome = rock.position;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.35f) { rock.localPosition = home + new Vector3(0.5f * k, 0.25f * Mathf.Sin(k * Mathf.PI), 0f); yield return null; }
            if (good)
            {
                smallChest.gameObject.SetActive(true);
                smallChest.position = new Vector3(worldHome.x - 0.1f, worldHome.y, smallChest.position.z);
                Say("Found it! But it's locked...", 2f);
                yield return new WaitForSeconds(0.8f);
                yield break;
            }
            eel.gameObject.SetActive(true);
            eel.localPosition = home + new Vector3(-0.1f, -0.2f, 0f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.2f) { eel.localScale = Vector3.one * k; yield return null; }
            eel.localScale = Vector3.one;
            yield return Damage("CHOMP! A moray eel bit the hull!");
            if (lost) yield break;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.2f) { eel.localScale = Vector3.one * (1f - k); yield return null; }
            eel.gameObject.SetActive(false);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.3f) { rock.localPosition = Vector3.Lerp(home + Vector3.right * 0.5f, home, k); yield return null; }
            rock.localPosition = home;
            SpriteRenderer sr = rock.GetComponentInChildren<SpriteRenderer>();
            if (sr != null) sr.color = new Color(0.6f, 0.6f, 0.65f);   // già provato
        }

        // ---------- Fase 3: la serratura ----------

        IEnumerator Stage3Lock()
        {
            lockScene.SetActive(true);
            smallChest.gameObject.SetActive(false);
            for (int i = 0; i < 3; i++)
            {
                dialValue[i] = Random.Range(0, symbolSprites.Length);
                dialSymbols[i].sprite = symbolSprites[dialValue[i]];
            }
            if (Solved()) { dialValue[0] = (dialValue[0] + 1) % symbolSprites.Length; dialSymbols[0].sprite = symbolSprites[dialValue[0]]; }
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.3f) { dim.color = new Color(0f, 0.02f, 0.08f, 0.6f * k); yield return null; }
            hud.Stage(2, "Set the 3 dials. The code is on the flag!");
            Say("Look up at the wreck's flag...", 2.2f);
            while (!Solved())
            {
                if (PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out _) && began && !PointerInput.IsOverUi())
                {
                    Vector2 w = PointerInput.ToWorld(cam, sp);
                    for (int i = 0; i < dials.Length; i++)
                    {
                        if (!dials[i].OverlapPoint(w)) continue;
                        dialValue[i] = (dialValue[i] + 1) % symbolSprites.Length;
                        dialSymbols[i].sprite = symbolSprites[dialValue[i]];
                        Sfx.Play(this, "tick", 0.8f);
                        StartCoroutine(Spin(dials[i].transform));
                    }
                }
                yield return null;
            }
            yield return new WaitForSeconds(0.35f);
            yield return hud.Clear("UNLOCKED!");
            Sfx.Play(this, "clunk", 0.8f);
            bigChest.sprite = chestOpen;
            treasureBubbles.Play();
            Haptics.Light();
            Say("Pirate gold! Best dive ever!", 2.4f);
            Transform t = bigChest.transform;
            Vector3 s = t.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.5f) { t.localScale = s * (1f + Mathf.Sin(k * Mathf.PI) * 0.1f); yield return null; }
            t.localScale = s;
            yield return new WaitForSeconds(1.6f);
            Win();
        }

        bool Solved()
        {
            for (int i = 0; i < 3; i++) if (dialValue[i] != combo[i]) return false;
            return true;
        }

        IEnumerator Spin(Transform d)
        {
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.15f) { d.localRotation = Quaternion.Euler(0, 0, -90f * (1f - k)); yield return null; }
            d.localRotation = Quaternion.identity;
        }
    }

    /// <summary>Geometria del tunnel del sottomarino (stessa interpolazione del disegno "sub_tunnel").</summary>
    public static class TunnelMath40A
    {
        public static void At(float[] path, float y, out float cx, out float w)
        {
            int n = path.Length / 3;
            if (y >= path[0]) { cx = path[1]; w = path[2]; return; }
            for (int i = 0; i < n - 1; i++)
            {
                float y0 = path[i * 3], y1 = path[i * 3 + 3];
                if (y <= y0 && y >= y1)
                {
                    float t = (y0 - y) / (y0 - y1);
                    t = t * t * (3f - 2f * t);
                    cx = path[i * 3 + 1] + (path[i * 3 + 4] - path[i * 3 + 1]) * t;
                    w = path[i * 3 + 2] + (path[i * 3 + 5] - path[i * 3 + 2]) * t;
                    return;
                }
            }
            cx = path[n * 3 - 2]; w = path[n * 3 - 1];
        }
    }
}
