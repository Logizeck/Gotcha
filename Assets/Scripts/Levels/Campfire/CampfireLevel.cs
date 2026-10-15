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

        enum Phase { Rub, Blow, Done }
        Phase phase = Phase.Rub;
        int strokes;
        float lastX, swing, dir, size, strongFor, sparkFor;
        bool tracking;
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
            if (bubble != null) { bubble.Show("Brrr... so cold!"); StartCoroutine(HideLater(1.8f)); }
        }

        void OnDestroy() { if (SensorHub.Instance != null) SensorHub.Instance.ReleaseMic(this); }

        void Update()
        {
            if (IsFinished || phase == Phase.Done) return;
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
