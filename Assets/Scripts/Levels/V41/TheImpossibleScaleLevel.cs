using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "The impossible scale" — su una bilancia c'è un elefante da una parte e una piuma dall'altra. Spostare
    /// gli oggetti non serve (tre tentativi = la bilancia crolla, sconfitta). Il trucco: allarga la PIUMA con due dita
    /// (pinch) finché diventa una piuma gigante con un peso assurdo; quando la bilancia è in equilibrio si vince.
    /// </summary>
    public class TheImpossibleScaleLevel : S41FLevelBase
    {
        [SerializeField] Transform beam;               // perno dell'asta
        [SerializeField] Transform leftPan, rightPan;  // gancio dei piatti
        [SerializeField] Transform feather;            // base della piuma (figlio del piatto destro)
        [SerializeField] Transform elephant;           // piedi dell'elefante (figlio del piatto sinistro)
        [SerializeField] Collider2D featherArea, elephantArea;
        [SerializeField] TextMesh featherWeight, elephantWeight, triesText;
        [SerializeField] SpriteRenderer balanceLight;
        [SerializeField] float armLength = 1.75f, goalScale = 3.6f, maxScale = 4.6f;

        const float ElephantKg = 5000f;
        const int MaxTries = 3;
        float scale = 1f, angle, pinch0, scale0, balancedFor;
        bool pinching;
        Transform dragged;
        Vector3 dragHome;
        Vector2 dragStart;
        int tries;

        protected override void Begin()
        {
            if (elephantWeight != null) elephantWeight.text = "5000 KG";
            UpdateTries();
            angle = TargetAngle();
            Apply();
            Say("Balance the scale. Good luck with THAT.", 3f);
        }

        void UpdateTries() { if (triesText != null) triesText.text = "TRIES " + tries + "/" + MaxTries; }

        /// <summary>Peso della piuma: cresce in modo esponenziale (0.001 kg → 5000 kg alla scala obiettivo).</summary>
        float FeatherKg() => 0.001f * Mathf.Pow(10f, (scale - 1f) / (goalScale - 1f) * 6.699f);

        float TargetAngle()
        {
            float diff = Mathf.Log10(ElephantKg) - Mathf.Log10(FeatherKg());
            return Mathf.Clamp(diff * 7f, -22f, 22f);
        }

        protected override void Tick()
        {
            if (busy) return;
            ReadPinch();
            if (!pinching) ReadDrag();
            angle = Mathf.MoveTowards(angle, TargetAngle(), 40f * Time.deltaTime);
            Apply();
            float diff = Mathf.Abs(Mathf.Log10(ElephantKg) - Mathf.Log10(FeatherKg()));
            bool balanced = diff < 0.45f && Mathf.Abs(angle) < 4f;
            if (balanceLight != null) balanceLight.color = balanced ? new Color(0.45f, 1f, 0.5f, 0.9f) : new Color(1f, 0.4f, 0.35f, 0.6f);
            balancedFor = balanced ? balancedFor + Time.deltaTime : 0f;
            if (balancedFor > 0.8f) StartCoroutine(Balanced());
        }

        void ReadPinch()
        {
            if (Input.touchCount >= 2)
            {
                float d = Vector2.Distance(Input.GetTouch(0).position, Input.GetTouch(1).position);
                if (!pinching || Input.GetTouch(1).phase == TouchPhase.Began) { pinching = true; pinch0 = Mathf.Max(d, 1f); scale0 = scale; CancelDrag(); }
                scale = Mathf.Clamp(scale0 * Mathf.Pow(d / pinch0, 1.3f), 1f, maxScale);
            }
            else pinching = false;
#if UNITY_EDITOR
            float wheel = Input.mouseScrollDelta.y;
            if (Mathf.Abs(wheel) > 0.01f) scale = Mathf.Clamp(scale + wheel * 0.1f, 1f, maxScale);
            if (Input.GetKey(KeyCode.X)) scale = Mathf.Clamp(scale + Time.deltaTime * 0.8f, 1f, maxScale);
            if (Input.GetKey(KeyCode.Z)) scale = Mathf.Clamp(scale - Time.deltaTime * 0.8f, 1f, maxScale);
#endif
        }

        /// <summary>Trascinare gli oggetti senza cambiarli: tornano al loro posto e contano come tentativo.</summary>
        void ReadDrag()
        {
            if (!Pointer(out Vector2 w, out bool began, out bool ended)) { if (dragged != null) CancelDrag(); return; }
            if (began && Input.touchCount <= 1)
            {
                if (featherArea != null && featherArea.OverlapPoint(w)) dragged = feather;
                else if (elephantArea != null && elephantArea.OverlapPoint(w)) dragged = elephant;
                if (dragged != null) { dragHome = dragged.localPosition; dragStart = w; }
            }
            if (dragged == null) return;
            Vector2 delta = w - dragStart;
            dragged.localPosition = dragHome + (Vector3)(delta * (dragged == elephant ? 0.25f : 0.6f));
            if (ended)
            {
                bool moved = delta.magnitude > 0.5f;
                Transform t = dragged;
                CancelDrag();
                if (moved) StartCoroutine(MovedAttempt(t == elephant));
            }
        }

        void CancelDrag()
        {
            if (dragged != null) dragged.localPosition = dragHome;
            dragged = null;
        }

        void Apply()
        {
            beam.localRotation = Quaternion.Euler(0f, 0f, angle);
            float r = angle * Mathf.Deg2Rad;
            Vector3 arm = new Vector3(Mathf.Cos(r), Mathf.Sin(r), 0f) * armLength;
            leftPan.localPosition = beam.localPosition - arm;
            rightPan.localPosition = beam.localPosition + arm;
            feather.localScale = Vector3.one * scale;
            if (featherWeight != null)
            {
                float kg = FeatherKg();
                featherWeight.text = kg < 1f ? kg.ToString("0.000") + " KG" : kg < 100f ? kg.ToString("0.0") + " KG" : Mathf.RoundToInt(kg) + " KG";
            }
        }

        IEnumerator MovedAttempt(bool wasElephant)
        {
            busy = true;
            tries++;
            UpdateTries();
            Sfx.Play(this, "bonk", 0.8f);
            Haptics.Medium();
            if (tries >= MaxTries)
            {
                Say("CRACK! The poor scale gave up.", 2.5f);
                Sfx.Play(this, "clunk", 1f);
                Haptics.Heavy();
                for (float k = 0f; k < 1f; k += Time.deltaTime / 0.6f)
                {
                    angle = Mathf.Lerp(angle, 35f, k);
                    Apply();
                    yield return null;
                }
                yield return Shake(beam, 0.15f, 0.5f);
                yield return new WaitForSeconds(0.8f);
                Fail();
                yield break;
            }
            Say(wasElephant ? "The elephant won't budge. He weighs 5000 kg!" : "Moving it doesn't change its weight...", 2.5f);
            yield return Shake(wasElephant ? elephant : feather, 0.08f, 0.4f);
            busy = false;
        }

        IEnumerator Balanced()
        {
            busy = true;
            Sfx.Play(this, "ding", 0.9f);
            Haptics.Light();
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.4f) { angle = Mathf.Lerp(angle, 0f, k); Apply(); yield return null; }
            angle = 0f;
            Apply();
            if (featherWeight != null) featherWeight.text = "5000 KG";
            Say("Perfectly balanced. As all things should be.", 2.5f);
            // la piuma gigante solletica l'elefante
            for (float k = 0f; k < 1.4f; k += Time.deltaTime)
            {
                feather.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(k * 9f) * 12f);
                elephant.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(k * 22f) * 4f);
                yield return null;
            }
            feather.localRotation = Quaternion.identity;
            elephant.localRotation = Quaternion.identity;
            yield return Pop(elephant, 1.15f, 0.3f);
            yield return Celebrate();
        }
    }
}
