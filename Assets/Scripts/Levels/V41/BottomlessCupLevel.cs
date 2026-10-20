using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "The bottomless cup" — il topolino ha sete, ma il bicchiere ha un buco sul fondo e ogni goccia scappa.
    /// Soluzione: tieni un dito sul buco e con un secondo dito tieni aperto il rubinetto finché l'acqua arriva alla tacca.
    /// Sconfitta: tre riempimenti falliti (acqua versata col buco scoperto, o buco lasciato prima della tacca).
    /// </summary>
    public class BottomlessCupLevel : LevelController
    {
        [SerializeField] Collider2D holeArea, handleArea;
        [SerializeField] Transform handle, water, stream, hole, cork, mouse;
        [SerializeField] Sprite dropSprite;
        [SerializeField] TextMesh triesText;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] SmokeEmitter[] confetti;
        [SerializeField] float inflow = 0.2f, leak = 0.55f, target = 0.85f, innerHeight = 1.98f;

        Camera cam;
        readonly List<Transform> drops = new List<Transform>();
        readonly List<float> dropVy = new List<float>();
        float level, pourLeak, dripTimer, sayCooldown;
        bool wasPlugged, busy, needRelease;
        int failed;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            cork.gameObject.SetActive(false);
            SetWater();
            UpdateTries();
            S41GKit.Confetti(confetti, false);
            if (bubble != null) bubble.Show("Water... please... I'm SO thirsty!");
            sayCooldown = 2.5f;
        }

        void UpdateTries() { triesText.text = "FAILED FILLINGS: " + failed + "/3"; }

        void Say(string t) { if (bubble != null && sayCooldown <= 0f) { bubble.Show(t); sayCooldown = 2.5f; } }

        void SetWater()
        {
            water.localScale = new Vector3(1f, Mathf.Max(0.001f, level), 1f);
        }

        void Update()
        {
            float dt = Time.deltaTime;
            MoveDrops(dt);
            if (IsFinished || busy) return;
            sayCooldown -= dt;
            bool plugged = false, open = false;
            foreach (S41GKit.Finger f in S41GKit.Fingers(cam))
            {
                if (f.ended) continue;
                if (holeArea.OverlapPoint(f.world)) plugged = true;
                if (handleArea.OverlapPoint(f.world)) open = true;
            }
#if UNITY_EDITOR
            if (Input.GetKey(KeyCode.H)) plugged = true;   // Editor: H = dito sul buco
#endif
            if (!open) needRelease = false;
            if (needRelease) open = false;

            handle.localRotation = Quaternion.Euler(0, 0, open ? Mathf.MoveTowardsAngle(handle.localEulerAngles.z, 90f, 400f * dt) : Mathf.MoveTowardsAngle(handle.localEulerAngles.z, 0f, 400f * dt));
            if (open) level += inflow * dt;
            if (!plugged && level > 0f)
            {
                level -= leak * dt;
                dripTimer -= dt;
                if (dripTimer <= 0f) { Drip(); dripTimer = 0.05f; }
            }
            level = Mathf.Clamp01(level);
            SetWater();
            UpdateStream(open);

            // versare col buco scoperto: tutto scappa
            if (open && !plugged)
            {
                pourLeak += dt;
                if (pourLeak > 0.4f) Say("It's all leaking out the bottom!");
                if (pourLeak > 1.8f) { pourLeak = 0f; needRelease = true; StartCoroutine(FailedFilling("ALL THE WATER ESCAPED!")); return; }
            }
            else pourLeak = Mathf.Max(0f, pourLeak - dt);

            if (plugged && !wasPlugged) Sfx.Play(this, "tick", 0.5f);
            // buco lasciato prima della tacca
            if (wasPlugged && !plugged && level > 0.08f && level < target)
            {
                wasPlugged = false;
                StartCoroutine(FailedFilling("You let go too early!"));
                return;
            }
            wasPlugged = plugged;
            if (plugged && level >= target) StartCoroutine(Full());
        }

        void UpdateStream(bool open)
        {
            if (!open) { stream.localScale = new Vector3(1f, 0f, 1f); return; }
            float surface = water.position.y + level * innerHeight;
            float len = Mathf.Max(0f, stream.position.y - surface);
            stream.localScale = new Vector3(1f + Mathf.Sin(Time.time * 40f) * 0.08f, len, 1f);
        }

        void Drip()
        {
            Transform d = null;
            for (int i = 0; i < drops.Count; i++) if (!drops[i].gameObject.activeSelf) { d = drops[i]; dropVy[i] = 0f; break; }
            if (d == null)
            {
                var go = new GameObject("Drip");
                go.transform.SetParent(transform, false);
                SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = dropSprite;
                sr.sortingOrder = 4;
                d = go.transform;
                d.localScale = Vector3.one * 0.6f;
                drops.Add(d);
                dropVy.Add(0f);
            }
            d.gameObject.SetActive(true);
            d.position = hole.position + new Vector3(Random.Range(-0.06f, 0.06f), -0.05f, 0f);
        }

        void MoveDrops(float dt)
        {
            for (int i = 0; i < drops.Count; i++)
            {
                if (!drops[i].gameObject.activeSelf) continue;
                dropVy[i] -= 9f * dt;
                drops[i].position += new Vector3(0f, dropVy[i] * dt, 0f);
                if (drops[i].position.y < -5.5f) drops[i].gameObject.SetActive(false);
            }
        }

        IEnumerator FailedFilling(string line)
        {
            busy = true;
            failed++;
            UpdateTries();
            Sfx.Play(this, "buzz", 0.6f);
            Haptics.Medium();
            if (bubble != null) bubble.Show(line);
            stream.localScale = new Vector3(1f, 0f, 1f);
            // l'acqua rimasta scappa tutta
            while (level > 0f)
            {
                level = Mathf.Max(0f, level - leak * 2.5f * Time.deltaTime);
                SetWater();
                dripTimer -= Time.deltaTime;
                if (dripTimer <= 0f) { Drip(); dripTimer = 0.03f; }
                yield return null;
            }
            yield return StartCoroutine(S41GKit.Shake(mouse, 0.1f, 0.4f));
            if (failed >= 3)
            {
                if (bubble != null) bubble.Show("*cough* ...I'll just drink from the puddle.");
                yield return new WaitForSeconds(1.5f);
                Fail();
                yield break;
            }
            yield return new WaitForSeconds(0.6f);
            if (bubble != null) bubble.Show(failed == 1 ? "Try again... please?" : "Last try! I'm drying up!");
            sayCooldown = 2f;
            wasPlugged = false;
            busy = false;
        }

        IEnumerator Full()
        {
            busy = true;
            stream.localScale = new Vector3(1f, 0f, 1f);
            cork.gameObject.SetActive(true);
            Sfx.Play(this, "pop", 0.8f);
            yield return StartCoroutine(S41GKit.Pop(cork, 1.5f, 0.25f));
            Sfx.Play(this, "ding", 0.8f);
            Haptics.Light();
            if (bubble != null) bubble.Show("A FULL GLASS! *gulp gulp gulp*");
            Vector3 home = mouse.localPosition;
            yield return StartCoroutine(S41GKit.Move(mouse, home + new Vector3(0f, 0f, 0f), 0.3f, 0.5f));
            for (float k = 0f; k < 1f; k += Time.deltaTime / 1.2f)
            {
                level = Mathf.Lerp(target, 0.3f, k);
                SetWater();
                yield return null;
            }
            S41GKit.Confetti(confetti, true);
            if (bubble != null) bubble.Show("Aaah! Best. Glass. Ever.");
            yield return StartCoroutine(S41GKit.Pop(mouse, 1.3f, 0.4f));
            yield return new WaitForSeconds(1.6f);
            S41GKit.Confetti(confetti, false);
            Win();
        }
    }
}
