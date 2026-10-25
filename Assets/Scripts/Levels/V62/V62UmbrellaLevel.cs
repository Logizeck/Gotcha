using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels.V62;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 255 "Upside-down umbrella" — la pianta nella serra ha sete, ma il tetto di vetro la ripara dalla
    /// pioggia e il rubinetto non ha la manopola. L'ombrello aperto sotto la pioggia ripara e basta. Soluzione:
    /// aprirlo (tocco), girarlo a testa in giù (doppio tocco o il pulsante tondo), riempirlo sotto la pioggia, portarlo
    /// sopra la pianta e versare (inclinando il telefono o toccandolo). Versare altrove spreca l'acqua (si riempie di
    /// nuovo).
    /// </summary>
    public class V62UmbrellaLevel : V62Level
    {
        [SerializeField] Transform umbrella;         // origine = centro della cupola
        [SerializeField] Transform umbrellaArt;      // si ruota di 180° quando è rovesciato
        [SerializeField] SpriteRenderer umbrellaSprite;
        [SerializeField] Sprite closedSprite;
        [SerializeField] Sprite openSprite;
        [SerializeField] Collider2D umbrellaArea;
        [SerializeField] Transform water;
        [SerializeField] Transform flipButton;
        [SerializeField] Collider2D flipArea;
        [SerializeField] Collider2D rainZone;
        [SerializeField] Collider2D plantZone;
        [SerializeField] Transform plant;
        [SerializeField] SpriteRenderer plantArt;
        [SerializeField] Collider2D plantArea;
        [SerializeField] Collider2D faucetArea;
        [SerializeField] SmokeEmitter rain;
        [SerializeField] SmokeEmitter pourStream;
        [SerializeField] SmokeEmitter sparkle;
        [SerializeField] SmokeEmitter hearts;
        [SerializeField] TextMesh infoText;
        [SerializeField] Transform infoHolder;
        [SerializeField] Color thirstyTint = new Color(0.72f, 0.66f, 0.42f);
        [SerializeField] float thirstyAngle = -20f;

        const float DoubleTap = 0.4f;
        const float TapMove = 0.2f;
        const float TiltToPour = 0.35f;

        readonly UmbrellaLogic logic = new UmbrellaLogic();
        readonly SteadyTilt tilt = new SteadyTilt();
        Camera cam;
        HwDragger drag;
        HwDragger.Item umbrellaItem;
        HwToast toast;
        Vector3 waterScale, plantScale;
        Vector2 pressAt;
        float pressTime, lastTap = -10f;
        bool busy, inRain, dryShown, fullShown;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            logic.Reset();
            ResetV62();
            toast = new HwToast(infoText, infoHolder);
            drag = new HwDragger(this);
            waterScale = water.localScale;
            plantScale = plant.localScale;
            umbrellaItem = drag.Add(umbrella, umbrellaArea, 5);
            umbrellaSprite.sprite = closedSprite;
            umbrellaArt.localRotation = Quaternion.identity;
            water.gameObject.SetActive(false);
            flipButton.gameObject.SetActive(false);
            plantArt.color = thirstyTint;
            plant.localRotation = Quaternion.Euler(0f, 0f, thirstyAngle);
            if (rain != null) rain.Play();
            foreach (SmokeEmitter e in new[] { pourStream, sparkle, hearts }) if (e != null) e.Stop();
            toast.Show("The plant is so thirsty...", 2f);
        }

        void Update()
        {
            toast.Tick();
            float lean = tilt.Update(Time.deltaTime);
            if (IsFinished || busy || logic.InputLocked) return;
            UpdateRain();
            // versare inclinando il telefono (l'ombrello rovesciato e con dell'acqua, non in mano)
            if (drag.Held == null && logic.Pose == UmbrellaPose.Inverted && logic.Water > 0.05f && Mathf.Abs(lean) > TiltToPour)
            {
                PourNow(lean > 0f ? -1f : 1f);
                return;
            }
            if (!HalloweenKit.ReadPointer(cam, out Vector2 w, out bool began, out bool ended))
            {
                if (drag.Held != null) Release(w, true);
                return;
            }
            if (began)
            {
                if (flipButton.gameObject.activeSelf && flipArea.OverlapPoint(w)) { Flip(); return; }
                if (drag.TryPick(w) != null) { pressAt = w; pressTime = Time.time; }
                else Tap(w);
            }
            if (drag.Held == null) return;
            drag.MoveHeld(w);
            if (ended) Release(w, false);
        }

        /// <summary>Pioggia sull'ombrello: rovesciato si riempie, dritto ripara ("Nice and dry.").</summary>
        void UpdateRain()
        {
            bool now = rainZone.OverlapPoint(umbrella.position);
            if (now && logic.Rain(Time.deltaTime)) ShowWater();
            if (now && !inRain && logic.Pose == UmbrellaPose.Open && !dryShown)
            {
                dryShown = true;
                toast.Show("Nice and dry.", 1.6f);
            }
            if (!now) dryShown = false;
            if (now && logic.Full && !fullShown) { fullShown = true; toast.Show("Full to the brim!", 1.4f); Haptics.Light(); }
            if (!logic.Full) fullShown = false;
            inRain = now;
        }

        void ShowWater()
        {
            float k = logic.Water;
            water.gameObject.SetActive(k > 0f && logic.Pose == UmbrellaPose.Inverted);
            water.localScale = new Vector3(waterScale.x * Mathf.Sqrt(k), waterScale.y * k, waterScale.z);
        }

        void Release(Vector2 w, bool lostFinger)
        {
            drag.Release();
            bool tap = !lostFinger && Time.time - pressTime < 0.3f && Vector2.Distance(w, pressAt) < TapMove;
            umbrellaItem.Home = umbrella.position;   // resta dove lo si lascia (e si può riprendere)
            if (tap) { TapUmbrella(); return; }
            if (logic.Pose == UmbrellaPose.Inverted && logic.Water > 0f && plantZone.OverlapPoint(umbrella.position))
                toast.Show(logic.Full ? "Now pour it! Tilt the phone or tap." : "Not full yet...", 1.6f);
        }

        void TapUmbrella()
        {
            if (logic.TryOpen())
            {
                umbrellaSprite.sprite = openSprite;
                flipButton.gameObject.SetActive(true);
                Sfx.Play(this, "whoosh", 0.6f);
                StartCoroutine(HalloweenKit.Pop(umbrella, 1.12f, 0.25f));
                return;
            }
            if (logic.Pose == UmbrellaPose.Inverted && logic.Water > 0f) { PourNow(1f); return; }
            if (Time.time - lastTap < DoubleTap) { lastTap = -10f; Flip(); return; }
            lastTap = Time.time;
        }

        void Tap(Vector2 w)
        {
            if (faucetArea.OverlapPoint(w))
            {
                Sfx.Play(this, "clunk", 0.5f);
                toast.Show("No handle. No water.", 1.6f);
                return;
            }
            if (plantArea.OverlapPoint(w))
            {
                StartCoroutine(HalloweenKit.Wiggle(plant, 4f, 0.5f, 12f));
                toast.Show("Water... please...", 1.4f);
            }
        }

        void Flip()
        {
            if (!logic.TryFlip(plantZone.OverlapPoint(umbrella.position), out PourResult poured)) return;
            Sfx.Play(this, "whoosh", 0.5f);
            if (poured != PourResult.Nothing) { StartCoroutine(Pour(poured, 1f, true)); return; }
            StartCoroutine(TurnArt(logic.Pose == UmbrellaPose.Inverted ? 180f : 0f));
            if (logic.Pose == UmbrellaPose.Inverted) toast.Show("Upside down!", 1.2f);
        }

        void PourNow(float side)
        {
            PourResult r = logic.TryPour(plantZone.OverlapPoint(umbrella.position));
            if (r != PourResult.Nothing) StartCoroutine(Pour(r, side, false));
        }

        /// <summary>L'ombrello è sopra la serra, ma fuori (sul tetto di vetro)?</summary>
        bool AboveGreenhouse()
        {
            Bounds b = plantZone.bounds;
            Vector3 p = umbrella.position;
            return p.x >= b.min.x && p.x <= b.max.x && p.y > b.max.y;
        }

        IEnumerator TurnArt(float to)
        {
            busy = true;
            float from = umbrellaArt.localEulerAngles.z;
            for (float t = 0f; t < 1f; t += Time.deltaTime / 0.3f)
            {
                umbrellaArt.localRotation = Quaternion.Euler(0f, 0f, Mathf.LerpAngle(from, to, t));
                yield return null;
            }
            umbrellaArt.localRotation = Quaternion.Euler(0f, 0f, to);
            ShowWater();
            busy = false;
        }

        /// <summary>L'acqua esce dall'ombrello: sulla pianta (piena) la fa fiorire, altrove si spreca.</summary>
        IEnumerator Pour(PourResult result, float side, bool flipAfter)
        {
            busy = true;
            drag.Cancel();
            Sfx.Play(this, "whoosh", 0.8f);
            float baseAngle = umbrellaArt.localEulerAngles.z;
            for (float t = 0f; t < 1f; t += Time.deltaTime / 0.25f)
            {
                umbrellaArt.localRotation = Quaternion.Euler(0f, 0f, baseAngle + side * 60f * t);
                yield return null;
            }
            if (pourStream != null) { pourStream.transform.position = umbrella.position + new Vector3(side * 0.6f, -0.2f, 0f); pourStream.Play(); }
            Vector3 ws = water.localScale;
            for (float t = 0f; t < 1f; t += Time.deltaTime / 0.8f)
            {
                water.localScale = new Vector3(ws.x, ws.y * (1f - t), ws.z);
                yield return null;
            }
            if (pourStream != null) pourStream.Stop();
            water.gameObject.SetActive(false);
            float end = flipAfter ? 0f : baseAngle;
            for (float t = 0f; t < 1f; t += Time.deltaTime / 0.25f)
            {
                umbrellaArt.localRotation = Quaternion.Euler(0f, 0f, Mathf.LerpAngle(baseAngle + side * 60f, end, t));
                yield return null;
            }
            umbrellaArt.localRotation = Quaternion.Euler(0f, 0f, end);
            if (result == PourResult.Bloom) { yield return Bloom(); yield break; }
            toast.Show(result == PourResult.NotEnough ? "More! It's still thirsty."
                : AboveGreenhouse() ? "The glass roof is in the way!" : "Splash! All gone.", 1.6f);
            Sfx.Play(this, "pop", 0.4f);
            busy = false;
        }

        IEnumerator Bloom()
        {
            Haptics.Medium();
            Sfx.Play(this, "ding", 0.7f);
            if (sparkle != null) sparkle.Play();
            Color from = plantArt.color;
            for (float t = 0f; t < 1f; t += Time.deltaTime / 1.0f)
            {
                plantArt.color = Color.Lerp(from, Color.white, t);
                plant.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(thirstyAngle, 0f, t));
                plant.localScale = plantScale * (1f + 0.15f * Mathf.Sin(t * Mathf.PI));
                yield return null;
            }
            plant.localScale = plantScale;
            if (sparkle != null) sparkle.Stop();
            if (hearts != null) hearts.Play();
            toast.Show("Ahhh. Much better!", 2f);
            yield return Celebrate(2.2f);
            if (hearts != null) hearts.Stop();
            if (logic.TryCompleteBloom()) Win();
        }
    }
}
