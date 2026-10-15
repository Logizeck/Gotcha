using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Sensors;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 15 — buon compleanno! Sulla torta ci sono 5 candeline accese: si spengono soffiando davvero nel
    /// microfono del telefono. Le fiamme tremano quando soffi; ogni soffio deciso ne spegne una (con un filo di fumo).
    /// Spente tutte: coriandoli e "Happy birthday!". Nell'Editor si "soffia" tenendo premuto B.
    /// </summary>
    public class BirthdayLevel : LevelController
    {
        [SerializeField] Transform[] flames;
        [SerializeField] SmokeEmitter smoke;
        [SerializeField] SmokeEmitter[] confetti;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] float blowThreshold = 0.3f;
        [SerializeField] float blowPerCandle = 0.22f;

        int lit;
        float blowing, noMicFor;
        bool done, warned;

        public override void OnLevelStart()
        {
            lit = flames.Length;
            if (bubble != null) bubble.Hide();
            if (SensorHub.Instance != null) SensorHub.Instance.EnableMic();
            if (confetti != null) foreach (SmokeEmitter c in confetti) if (c != null) c.Stop();
        }

        void OnDestroy()
        {
            if (SensorHub.Instance != null) SensorHub.Instance.DisableMic();
        }

        void Update()
        {
            if (done || IsFinished) return;
            float level = 0f;
            if (SensorHub.Instance != null)
            {
                SensorHub.Instance.EnableMic();   // idempotente: aspetta il permesso
                level = SensorHub.Instance.MicLevel01;
            }
#if UNITY_EDITOR
            if (Input.GetKey(KeyCode.B)) level = 1f;
#endif
            // le fiamme tremano e si piegano col soffio
            for (int i = 0; i < flames.Length; i++)
            {
                if (flames[i] == null || !flames[i].gameObject.activeSelf) continue;
                float wobble = Mathf.Sin(Time.time * (13f + i * 2f)) * 0.06f;
                flames[i].localScale = new Vector3(1f + wobble, 1f - level * 0.35f - wobble, 1f);
                flames[i].localRotation = Quaternion.Euler(0, 0, -level * 40f + wobble * 60f);
            }

            blowing = level > blowThreshold ? blowing + Time.deltaTime : Mathf.Max(0f, blowing - Time.deltaTime * 0.5f);
            if (blowing >= blowPerCandle && lit > 0)
            {
                blowing = 0f;
                BlowOut();
            }

            // se il microfono non arriva (permesso negato o assente), lo si dice una volta
            noMicFor = level <= 0.0001f ? noMicFor + Time.deltaTime : 0f;
            if (!warned && noMicFor > 6f && bubble != null)
            {
                warned = true;
                bubble.Show("Make a wish... and BLOW!");
            }
        }

        void BlowOut()
        {
            // si spegne la candelina accesa più a sinistra
            for (int i = 0; i < flames.Length; i++)
            {
                if (flames[i] == null || !flames[i].gameObject.activeSelf) continue;
                flames[i].gameObject.SetActive(false);
                lit--;
                if (smoke != null) StartCoroutine(Puff(flames[i].position));
                break;
            }
            if (lit <= 0) { done = true; StartCoroutine(Party()); }
        }

        IEnumerator Puff(Vector3 at)
        {
            smoke.transform.position = at;
            smoke.Play();
            yield return new WaitForSeconds(0.25f);
            smoke.Stop();
        }

        IEnumerator Party()
        {
            if (bubble != null) bubble.Show("Happy birthday!");
            if (confetti != null) foreach (SmokeEmitter c in confetti) if (c != null) c.Play();
            yield return new WaitForSeconds(1.8f);
            if (confetti != null) foreach (SmokeEmitter c in confetti) if (c != null) c.Stop();
            Win();
        }
    }
}
