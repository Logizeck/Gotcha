using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "The moon thief" — un ladro col retino vuole rubare la luna. Afferrare il ladro non serve (scappa; tre
    /// tentativi = sconfitta). Soluzione: trascina la luna dietro una nuvola, poi trascina la nuvola fuori dallo schermo:
    /// il ladro non trova più la luna e si arrende.
    /// </summary>
    public class MoonThiefLevel : LevelController
    {
        [SerializeField] Transform moon, thief, net;
        [SerializeField] Transform[] clouds;
        [SerializeField] SpriteRenderer moonArt, thiefArt;
        [SerializeField] Sprite sadSprite;
        [SerializeField] Collider2D thiefArea;
        [SerializeField] TextMesh triesText;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] SmokeEmitter[] confetti;
        [SerializeField] float hideRadius = 1.0f;

        Camera cam;
        Vector3[] cloudHome;
        int dragCloud = -1, hiddenIn = -1, grabs;
        bool dragMoon, busy;
        Vector2 grabOffset;
        float netTimer = 2.5f;
        int moonOrder;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            moonOrder = moonArt.sortingOrder;
            cloudHome = new Vector3[clouds.Length];
            for (int i = 0; i < clouds.Length; i++) cloudHome[i] = clouds[i].localPosition;
            S41KKit.Confetti(confetti, false);
            UpdateTries();
            if (bubble != null) bubble.Show("Heh heh... that shiny moon is MINE tonight!");
        }

        void UpdateTries() { triesText.text = "GRAB ATTEMPTS: " + grabs + "/3"; }

        void Update()
        {
            if (IsFinished || busy) return;
            NetSwing();
            if (!PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out bool ended))
            {
                if (dragMoon || dragCloud >= 0) Release();
                return;
            }
            Vector2 w = PointerInput.ToWorld(cam, sp);
            if (began)
            {
                if (hiddenIn < 0 && Vector2.Distance(w, moon.position) < 0.85f)
                {
                    dragMoon = true;
                    grabOffset = (Vector2)moon.position - w;
                    Sfx.Play(this, "tick", 0.5f);
                }
                else
                {
                    for (int i = 0; i < clouds.Length; i++)
                    {
                        Vector2 d = w - (Vector2)clouds[i].position;
                        if (Mathf.Abs(d.x) < 1.15f && Mathf.Abs(d.y) < 0.65f)
                        {
                            dragCloud = i;
                            grabOffset = (Vector2)clouds[i].position - w;
                            Sfx.Play(this, "whoosh", 0.3f);
                            break;
                        }
                    }
                    if (dragCloud < 0 && thiefArea.OverlapPoint(w)) { StartCoroutine(GrabThief()); return; }
                }
            }
            if (dragMoon)
            {
                Vector2 p = w + grabOffset;
                moon.position = new Vector3(Mathf.Clamp(p.x, -2.4f, 2.4f), Mathf.Clamp(p.y, -1.2f, 2.6f), 0f);
            }
            else if (dragCloud >= 0)
            {
                Vector2 p = w + grabOffset;
                clouds[dragCloud].position = new Vector3(Mathf.Clamp(p.x, -4f, 4f), Mathf.Clamp(p.y, -1.0f, 2.6f), 0f);
                if (Mathf.Abs(clouds[dragCloud].position.x) > 3.3f) { CloudGone(dragCloud); return; }
            }
            if (ended) Release();
        }

        void Release()
        {
            if (dragMoon)
            {
                dragMoon = false;
                for (int i = 0; i < clouds.Length; i++)
                {
                    if (Vector2.Distance(moon.position, clouds[i].position) > hideRadius) continue;
                    hiddenIn = i;
                    moon.SetParent(clouds[i], true);
                    StartCoroutine(S41KKit.Move(moon, new Vector3(0.1f, 0.05f, 0f), 0.25f));
                    moonArt.sortingOrder = clouds[i].GetComponentInChildren<SpriteRenderer>().sortingOrder - 1;
                    Sfx.Play(this, "pop", 0.5f);
                    if (bubble != null) bubble.Show("Hey! Who turned off the moon? ...is it in that cloud?");
                    break;
                }
            }
            else if (dragCloud >= 0)
            {
                int i = dragCloud;
                dragCloud = -1;
                if (Mathf.Abs(clouds[i].position.x) > 2.9f) CloudGone(i);
            }
        }

        void CloudGone(int i)
        {
            dragCloud = -1;
            if (i == hiddenIn) { StartCoroutine(WinRoutine(i)); return; }
            // nuvola vuota: rientra pian piano
            StartCoroutine(S41KKit.Move(clouds[i], cloudHome[i], 1.2f));
            if (bubble != null) bubble.Show("Moving clouds? Pfff. The moon is still right there!");
        }

        void NetSwing()
        {
            netTimer -= Time.deltaTime;
            float swing = netTimer < 0.6f && netTimer > 0f ? Mathf.Sin((0.6f - netTimer) / 0.6f * Mathf.PI) * 35f : 0f;
            net.localRotation = Quaternion.Euler(0f, 0f, -swing);
            if (netTimer > 0f) return;
            netTimer = Random.Range(2.5f, 3.5f);
            Sfx.Play(this, "whoosh", 0.35f);
            if (bubble == null || Random.value > 0.5f) return;
            if (hiddenIn >= 0) bubble.Show("I KNOW it's in that cloud! Come here, cloud!");
            else bubble.Show(Random.value < 0.5f ? "Almost... got it... hup!" : "Just a little higher...");
        }

        IEnumerator GrabThief()
        {
            busy = true;
            grabs++;
            UpdateTries();
            Sfx.Play(this, "whoosh", 0.7f);
            Vector3 home = thief.localPosition;
            yield return StartCoroutine(S41KKit.Move(thief, home + new Vector3(0.6f, 0f, 0f), 0.2f, 0.4f));
            if (grabs >= 3)
            {
                if (bubble != null) bubble.Show("Too slow! The moon is mine! MWAHAHA!");
                Haptics.Medium();
                if (hiddenIn >= 0) moon.SetParent(thief.parent, true);
                yield return StartCoroutine(S41KKit.Move(moon, thief.localPosition + new Vector3(0.5f, 1.3f, 0f), 0.6f, 0.5f));
                moon.SetParent(thief, true);
                yield return StartCoroutine(S41KKit.Move(thief, home + new Vector3(-4f, 0f, 0f), 1.0f, 0.3f));
                Fail();
                yield break;
            }
            if (bubble != null) bubble.Show(grabs == 1 ? "Ha! You can't catch me!" : "Too slippery for you!");
            yield return StartCoroutine(S41KKit.Move(thief, home, 0.3f, 0.2f));
            busy = false;
        }

        IEnumerator WinRoutine(int i)
        {
            busy = true;
            Sfx.Play(this, "whoosh", 0.8f);
            Vector3 to = clouds[i].localPosition + new Vector3(Mathf.Sign(clouds[i].localPosition.x) * 3f, 0f, 0f);
            yield return StartCoroutine(S41KKit.Move(clouds[i], to, 0.6f));
            net.localRotation = Quaternion.identity;
            if (bubble != null) bubble.Show("WHAT?! Where did the moon go?!");
            for (int k = 0; k < 3; k++)
            {
                thief.localScale = new Vector3(-thief.localScale.x, thief.localScale.y, 1f);
                Sfx.Play(this, "tick", 0.5f);
                yield return new WaitForSeconds(0.35f);
            }
            thief.localScale = new Vector3(Mathf.Abs(thief.localScale.x), thief.localScale.y, 1f);
            thiefArt.sprite = sadSprite;
            if (bubble != null) bubble.Show("No moon, no job. I give up... *sniff*");
            Sfx.Play(this, "bonk", 0.6f);
            yield return new WaitForSeconds(1.2f);
            if (bubble != null) bubble.Hide();
            StartCoroutine(S41KKit.Move(thief, thief.localPosition + new Vector3(-3.6f, 0f, 0f), 1.6f));
            S41KKit.Confetti(confetti, true);
            Sfx.Play(this, "ding", 0.8f);
            // la nuvola riporta la luna al suo posto
            moon.SetParent(transform, true);
            moonArt.sortingOrder = moonOrder;
            moon.position = new Vector3(1.4f, 4.2f, 0f);
            yield return StartCoroutine(S41KKit.Move(moon, new Vector3(1.3f, 1.8f, 0f), 1.0f));
            yield return new WaitForSeconds(0.9f);
            S41KKit.Confetti(confetti, false);
            Win();
        }
    }
}
