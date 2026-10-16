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
    /// Easter egg: soffiando fortissimo (Editor: N) le candeline volano via dalla torta, fiamme comprese.
    /// </summary>
    public class BirthdayLevel : LevelController
    {
        [SerializeField] Transform[] flames;
        [SerializeField] SmokeEmitter smoke;
        [SerializeField] SmokeEmitter[] confetti;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] float blowThreshold = 0.3f;
        [SerializeField] float blowPerCandle = 0.22f;
        [SerializeField] Transform[] candles;
        [SerializeField] float stormLevel = 0.7f;      // soffio "uragano"...
        [SerializeField] float stormSeconds = 0.5f;    // ...tenuto così a lungo (in totale, con piccole pause)
        [SerializeField] string eggId = "candle_storm";

        int lit;
        float blowing, noMicFor, storm;
        bool done, warned;

        public override void OnLevelStart()
        {
            lit = flames.Length;
            if (bubble != null) bubble.Hide();
            if (SensorHub.Instance != null) SensorHub.Instance.UseMic(this);
            if (confetti != null) foreach (SmokeEmitter c in confetti) if (c != null) c.Stop();
        }

        void OnDestroy()
        {
            if (SensorHub.Instance != null) SensorHub.Instance.ReleaseMic(this);
        }

        void Update()
        {
            if (done || IsFinished) return;
            float level = 0f;
            if (SensorHub.Instance != null)
            {
                SensorHub.Instance.UseMic(this);
                level = SensorHub.Instance.MicLevel01;
            }
#if UNITY_EDITOR
            if (Input.GetKey(KeyCode.B)) level = 0.6f;
            if (Input.GetKey(KeyCode.N)) level = 1f;
#endif
            storm = level > stormLevel ? storm + Time.deltaTime : Mathf.Max(0f, storm - Time.deltaTime * 0.25f);   // il soffio del microfono è a scatti: non si azzera subito
            if (storm >= stormSeconds && candles != null && candles.Length > 0) { done = true; StartCoroutine(Hurricane()); return; }
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

        /// <summary>Easter egg: un soffio da uragano strappa le candeline dalla torta e le fa volare via.</summary>
        IEnumerator Hurricane()
        {
            if (bubble != null) bubble.Show("WHOOOSH!!");
            var flying = new System.Collections.Generic.List<Transform>();
            for (int i = 0; i < candles.Length; i++)
            {
                if (candles[i] == null) continue;
                // la fiamma vola insieme alla sua candelina
                if (i < flames.Length && flames[i] != null) flames[i].SetParent(candles[i], true);
                flying.Add(candles[i]);
            }
            var vel = new Vector2[flying.Count];
            var spin = new float[flying.Count];
            for (int i = 0; i < flying.Count; i++)
            {
                vel[i] = new Vector2(Random.Range(2.5f, 5f), Random.Range(4f, 7.5f));
                spin[i] = Random.Range(-720f, -240f);
            }
            if (smoke != null) { smoke.transform.position = flying.Count > 0 ? flying[0].position : transform.position; smoke.Play(); }
            for (float t = 0f; t < 1.8f; t += Time.deltaTime)
            {
                for (int i = 0; i < flying.Count; i++)
                {
                    vel[i].y -= 6f * Time.deltaTime;
                    flying[i].position += (Vector3)(vel[i] * Time.deltaTime);
                    flying[i].Rotate(0, 0, spin[i] * Time.deltaTime);
                }
                yield return null;
            }
            if (smoke != null) smoke.Stop();
            if (bubble != null) bubble.Show("...my candles?!");
            yield return new WaitForSeconds(1.2f);
            WinWithEgg(eggId);
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
