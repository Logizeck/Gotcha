using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 3 — il carlino affamato. All'inizio il carlino è sveglio e abbaia: ha fame, e non stacca gli occhi
    /// dall'osso (dipinto nel quadro, poi in mano tua mentre lo trascini, poi per terra se cade). La soluzione è
    /// prendere l'osso dal quadro e darglielo: lo addenta, lo rosicchia, scodinzola e si addormenta. A quel punto
    /// bisogna lasciarlo dormire per 5 secondi senza toccare lo schermo né muovere il telefono. Il pulsantone
    /// "PRESS ME" è un'esca: toccarlo (o toccare qualsiasi cosa) lo sveglia.
    /// </summary>
    public class IdleLevel : LevelController
    {
        [SerializeField] Transform cat;
        [SerializeField] IdleTimer timer;
        [SerializeField] GameObject eyesOpen;
        [SerializeField] GameObject alert;
        [SerializeField] SmokeEmitter zzz;
        [SerializeField] SmokeEmitter hearts;
        [SerializeField] SpeechBubble meow;
        [SerializeField] Draggable2D fish;          // l'osso
        [SerializeField] Transform button;
        [Header("Carlino")]
        [SerializeField] Transform[] pupils;        // figli degli occhi aperti
        [SerializeField] Vector2[] eyeCenters;      // coordinate locali del carlino
        [SerializeField] float pupilTravel = 0.045f;
        [SerializeField] GameObject mouthOpen;
        [SerializeField] GameObject tongue;
        [SerializeField] Transform tail;
        [SerializeField] SmokeEmitter crumbs;

        [Header("Coordinate locali del gatto")]
        [SerializeField] Vector2 mouth = new Vector2(-0.74f, 0.12f);
        [SerializeField] Vector2 headCenter = new Vector2(-0.72f, 0.38f);
        [SerializeField] float feedRadius = 0.85f;
        [SerializeField] float floorY = -2.75f;
        [SerializeField] float awakeSeconds = 1.2f;
        [SerializeField] float meowEvery = 3f;

        enum State { Hungry, Eating, Sleeping }

        State state;
        float awakeUntil, nextMeow, wag = 0.3f;
        Vector3 pugScale;
        Quaternion pugRot;
        bool startled;
        Vector3 buttonScale;

        public override void OnLevelStart()
        {
            if (button != null) buttonScale = button.localScale;
            if (timer != null)
            {
                timer.enabled = false;   // l'attesa conta solo dopo che il gatto ha mangiato
                timer.Completed += OnCompleted;
                timer.Interrupted += OnInterrupted;
            }
            if (fish != null) fish.Dropped += OnFishDropped;
            if (meow != null) meow.Hide();
            if (mouthOpen != null) mouthOpen.SetActive(false);
            if (tongue != null) tongue.SetActive(false);
            if (crumbs != null) crumbs.Stop();
            pugScale = cat.localScale;
            pugRot = cat.localRotation;
            state = State.Hungry;
            nextMeow = Time.time + 1f;
            SetAwakeLook(true, false);
        }

        void OnDestroy()
        {
            if (timer != null)
            {
                timer.Completed -= OnCompleted;
                timer.Interrupted -= OnInterrupted;
            }
            if (fish != null) fish.Dropped -= OnFishDropped;
        }

        void Update()
        {
            if (IsFinished) return;
            bool pressed = Input.touchCount > 0 || Input.GetMouseButton(0);
            if (button != null)   // il pulsante si abbassa se lo tocchi (tanto non serve a niente...)
            {
                Vector3 target = pressed ? new Vector3(buttonScale.x, buttonScale.y * 0.82f, buttonScale.z) : buttonScale;
                button.localScale = Vector3.Lerp(button.localScale, target, 20f * Time.deltaTime);
            }

            if (state == State.Hungry && Time.time >= nextMeow && meow != null)
            {
                nextMeow = Time.time + meowEvery;
                meow.Show(Random.value < 0.5f ? "Woof!" : "Woof woof!");
                Sfx.Play(this, "woof", 0.7f);
                StartCoroutine(HideLater(meow, 1.2f));
            }
            if (state == State.Sleeping && startled && Time.time >= awakeUntil) GoToSleep();
        }

        /// <summary>Gli occhi seguono sempre l'osso (o il dito, se l'osso non c'è più); la coda scodinzola.</summary>
        void LateUpdate()
        {
            if (tail != null) tail.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(Time.time * (6f + wag * 20f)) * 25f * wag);
            if (pupils == null || eyeCenters == null) return;
            Vector3? look = null;
            if (fish != null && fish.gameObject.activeSelf) look = fish.transform.position;
            else if (PointerInput.TryGetPrimary(out Vector2 sp, out _, out _) && Camera.main != null) look = PointerInput.ToWorld(Camera.main, sp);
            for (int i = 0; i < pupils.Length && i < eyeCenters.Length; i++)
            {
                Vector2 c = eyeCenters[i];
                Vector2 to = look.HasValue ? (Vector2)cat.InverseTransformPoint(look.Value) - c : Vector2.down * 0.2f;
                // vicinissimo all'occhio: la pupilla resta al centro (non strabuzza)
                Vector2 off = to.sqrMagnitude > 0.0001f ? to.normalized * Mathf.Min(pupilTravel, to.magnitude * 0.6f) : Vector2.zero;
                Vector3 want = cat.TransformPoint(c + off);   // le pupille sono figlie degli occhi: si lavora in coordinate mondo
                want.z = pupils[i].position.z;
                pupils[i].position = Vector3.Lerp(pupils[i].position, want, 1f - Mathf.Exp(-20f * Time.deltaTime));
            }
        }

        // ---------- Pesce ----------

        void OnFishDropped(Draggable2D item, DropSlot slot)
        {
            if (state != State.Hungry) return;
            Vector2 local = cat.InverseTransformPoint(item.transform.position);
            if (Vector2.Distance(local, headCenter) <= feedRadius)
                StartCoroutine(EatRoutine());
            else
                StartCoroutine(FallToFloor(item.transform));
        }

        /// <summary>Il carlino addenta l'osso: spalanca la bocca, scatta, morde, rosicchia scuotendo la testa, inghiotte, si lecca i baffi, scodinzola e crolla addormentato.</summary>
        IEnumerator EatRoutine()
        {
            state = State.Eating;
            fish.enabled = false;
            if (meow != null) meow.Hide();
            Transform f = fish.transform;
            Vector3 mouthW = cat.TransformPoint(mouth);
            Vector3 front = cat.TransformPoint(mouth + new Vector2(-0.35f, 0.05f));
            mouthW.z = front.z = f.position.z;
            wag = 1f;
            // 1. l'osso vola davanti al muso, il carlino si prepara (si accuccia) e spalanca la bocca
            Vector3 from = f.position;
            Quaternion fromRot = f.rotation;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.3f)
            {
                f.position = Vector3.Lerp(from, front, k) + Vector3.up * Mathf.Sin(k * Mathf.PI) * 0.25f;
                f.rotation = Quaternion.Slerp(fromRot, Quaternion.identity, k);
                cat.localScale = new Vector3(pugScale.x * (1f + 0.04f * k), pugScale.y * (1f - 0.06f * k), pugScale.z);
                yield return null;
            }
            if (mouthOpen != null) mouthOpen.SetActive(true);
            yield return new WaitForSeconds(0.12f);
            // 2. scatto in avanti: GNAM!
            Vector3 home = cat.localPosition;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.1f)
            {
                cat.localPosition = home + new Vector3(-0.18f * k, 0.05f * k, 0f);
                cat.localScale = Vector3.Lerp(cat.localScale, new Vector3(pugScale.x * 0.97f, pugScale.y * 1.06f, pugScale.z), k);
                f.position = Vector3.Lerp(front, mouthW, k);
                yield return null;
            }
            if (mouthOpen != null) mouthOpen.SetActive(false);
            Sfx.Play(this, "bonk", 0.9f);
            Haptics.Light();
            if (crumbs != null) { crumbs.transform.position = mouthW; crumbs.Play(); }
            if (meow != null) { meow.Show("GNAM!", true); }
            // 3. rosicchia: tre morsi, scuotendo la testa; l'osso si accorcia
            Vector3 boneScale = f.localScale;
            for (int bite = 0; bite < 3; bite++)
            {
                for (float k = 0f; k < 1f; k += Time.deltaTime / 0.28f)
                {
                    float shake = Mathf.Sin(k * Mathf.PI * 2f) * 7f;
                    cat.localRotation = pugRot * Quaternion.Euler(0, 0, shake);
                    if (mouthOpen != null) mouthOpen.SetActive(k > 0.3f && k < 0.55f);
                    f.position = cat.TransformPoint(mouth) + new Vector3(0f, 0f, f.position.z - cat.position.z);
                    f.rotation = cat.rotation * Quaternion.Euler(0, 0, shake * 1.5f);
                    yield return null;
                }
                Sfx.Play(this, "tick", 1f);
                f.localScale = boneScale * (1f - (bite + 1) * 0.3f);
                if (crumbs != null) crumbs.transform.position = f.position;
            }
            cat.localRotation = pugRot;
            if (meow != null) meow.Hide();
            // 4. inghiotte (gulp) e si lecca i baffi
            f.gameObject.SetActive(false);
            if (crumbs != null) crumbs.Stop();
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.25f)
            {
                cat.localScale = new Vector3(pugScale.x, pugScale.y * (1f + Mathf.Sin(k * Mathf.PI) * 0.08f), pugScale.z);
                cat.localPosition = Vector3.Lerp(cat.localPosition, home, k);
                yield return null;
            }
            cat.localScale = pugScale;
            cat.localPosition = home;
            if (tongue != null)
                for (int i = 0; i < 2; i++) { tongue.SetActive(true); yield return new WaitForSeconds(0.18f); tongue.SetActive(false); yield return new WaitForSeconds(0.12f); }
            // 5. felice: cuoricini e coda che va a mille, poi crolla
            if (hearts != null) hearts.Play();
            yield return new WaitForSeconds(0.9f);
            if (hearts != null) hearts.Stop();
            wag = 0.15f;
            yield return new WaitForSeconds(0.3f);
            GoToSleep();
            if (timer != null) timer.enabled = true;
        }

        IEnumerator FallToFloor(Transform t)
        {
            float target = floorY + Random.Range(-0.3f, 0.2f);
            float v = 0f;
            while (t != null && t.position.y > target)
            {
                if (Draggable2D.Active != null && Draggable2D.Active.transform == t) yield break;
                v += 18f * Time.deltaTime;
                t.position += Vector3.down * (v * Time.deltaTime);
                yield return null;
            }
        }

        static IEnumerator HideLater(SpeechBubble b, float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (b != null) b.Hide();
        }

        // ---------- Sonno ----------

        void GoToSleep()
        {
            state = State.Sleeping;
            wag = 0.05f;
            startled = false;
            SetAwakeLook(false, false);
            if (zzz != null) zzz.Play();
        }

        void OnInterrupted()
        {
            if (IsFinished || state != State.Sleeping) return;
            awakeUntil = Time.time + awakeSeconds;
            if (startled) return;
            startled = true;
            SetAwakeLook(true, true);
            Sfx.Play(this, "woof", 0.8f);
            if (zzz != null) zzz.Stop();
        }

        void SetAwakeLook(bool eyes, bool exclamation)
        {
            if (eyesOpen != null) eyesOpen.SetActive(eyes);
            if (alert != null) alert.SetActive(exclamation);
        }

        void OnCompleted()
        {
            if (IsFinished) return;
            StartCoroutine(WinRoutine());
        }

        IEnumerator WinRoutine()
        {
            if (zzz != null) zzz.Stop();
            if (hearts != null) hearts.Play();
            yield return new WaitForSeconds(1.4f);
            if (hearts != null) hearts.Stop();
            Win();
        }
    }
}
