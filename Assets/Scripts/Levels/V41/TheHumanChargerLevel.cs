using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "The human charger" — un robot all'1% e una batteria gigante senza cavo. Soluzione: trascinare il fulmine
    /// della scritta "LOW POWER" sul robot: ogni fulmine carica il 25%, quattro volte. Se il tempo finisce prima, il robot
    /// si spegne (sconfitta). Non usa la batteria vera del telefono.
    /// </summary>
    public class TheHumanChargerLevel : S41ILevelBase
    {
        [SerializeField] Transform robot, bolt, timerFill, chargeFill;
        [SerializeField] Collider2D boltArea, robotArea, bigBatteryArea;
        [SerializeField] SpriteRenderer robotArt;
        [SerializeField] Sprite robotOnSprite;
        [SerializeField] TextMesh percentText;
        [SerializeField] float seconds = 35f;

        int charge = 1, dragId = int.MinValue;
        float timeLeft;
        Vector3 boltHome, boltScale;
        Vector2 grabOffset;

        protected override void Begin()
        {
            boltHome = bolt.localPosition;
            boltScale = bolt.localScale;
            timeLeft = seconds;
            Show();
            Say("Battery... 1%... need... power...", 2.6f);
        }

        void Show()
        {
            percentText.text = charge + "%";
            percentText.color = charge < 30 ? new Color(1f, 0.35f, 0.35f) : charge < 100 ? new Color(1f, 0.85f, 0.3f) : new Color(0.4f, 1f, 0.5f);
            chargeFill.localScale = new Vector3(Mathf.Clamp01(charge / 100f), 1f, 1f);
        }

        protected override void Tick()
        {
            if (busy) return;
            timeLeft -= Time.deltaTime;
            timerFill.localScale = new Vector3(Mathf.Clamp01(timeLeft / seconds), 1f, 1f);
            if (timeLeft <= 0f) { StartCoroutine(ShutDown()); return; }
            if (charge < 100) robot.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(clock * 2f) * 2f);

            foreach (Finger f in Fingers())
            {
                if (f.began && dragId == int.MinValue)
                {
                    if (boltArea.OverlapPoint(f.world)) { dragId = f.id; grabOffset = (Vector2)bolt.position - f.world; Sfx.Play(this, "tick", 0.7f); Haptics.Light(); }
                    else if (bigBatteryArea.OverlapPoint(f.world)) { Say("A huge battery... and no cable!"); Sfx.Play(this, "clunk", 0.5f); }
                    else if (robotArea.OverlapPoint(f.world)) Say("Poking me... does not... charge me.");
                }
                if (f.id != dragId) continue;
                if (f.ended) { dragId = int.MinValue; Drop(); return; }
                Vector2 p = f.world + grabOffset;
                bolt.position = new Vector3(p.x, p.y, bolt.position.z);
                bolt.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(clock * 25f) * 8f);
            }
            if (dragId != int.MinValue && Fingers().Count == 0) { dragId = int.MinValue; Drop(); }
        }

        void Drop()
        {
            bolt.localRotation = Quaternion.identity;
            if (robotArea.OverlapPoint(bolt.position)) StartCoroutine(Zap());
            else StartCoroutine(Move(bolt, boltHome, 0.25f));
        }

        IEnumerator Zap()
        {
            busy = true;
            Sfx.Play(this, "buzz", 0.8f);
            Haptics.Medium();
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.25f) { bolt.localScale = boltScale * (1f - k); yield return null; }
            bolt.gameObject.SetActive(false);
            int from = charge, to = charge == 1 ? 25 : Mathf.Min(100, charge + 25);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.5f)
            {
                charge = Mathf.RoundToInt(Mathf.Lerp(from, to, k));
                Show();
                if (robotArt != null) robotArt.color = Color.Lerp(Color.white, new Color(1f, 1f, 0.5f), Mathf.PingPong(k * 6f, 1f));
                yield return null;
            }
            charge = to;
            Show();
            if (robotArt != null) robotArt.color = Color.white;
            if (charge >= 100) { yield return StartCoroutine(FullyCharged()); yield break; }
            Say(charge == 25 ? "Bzzt! 25%! More!" : charge == 50 ? "Half charged! Again!" : "75%! One more!", 1.8f);
            // un nuovo fulmine ricresce nella scritta
            bolt.localPosition = boltHome;
            bolt.localScale = Vector3.zero;
            bolt.gameObject.SetActive(true);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.35f) { bolt.localScale = boltScale * k; yield return null; }
            bolt.localScale = boltScale;
            busy = false;
        }

        IEnumerator FullyCharged()
        {
            if (robotArt != null && robotOnSprite != null) robotArt.sprite = robotOnSprite;
            Sfx.Play(this, "ding", 0.8f);
            Say("100%! Thank you, human charger!", 0f);
            Vector3 home = robot.localPosition;
            for (float k = 0f; k < 1.2f; k += Time.deltaTime)
            {
                robot.localPosition = home + Vector3.up * Mathf.Abs(Mathf.Sin(k * 8f)) * 0.3f;
                robot.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(k * 12f) * 8f);
                yield return null;
            }
            robot.localPosition = home;
            robot.localRotation = Quaternion.identity;
            yield return StartCoroutine(Celebrate(1.4f));
        }

        IEnumerator ShutDown()
        {
            busy = true;
            percentText.text = "0%";
            Say("Shutting... dooown...", 0f);
            Sfx.Play(this, "buzz", 0.7f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.8f)
            {
                robot.localRotation = Quaternion.Euler(0, 0, -10f * k);
                if (robotArt != null) robotArt.color = Color.Lerp(Color.white, new Color(0.45f, 0.45f, 0.5f), k);
                yield return null;
            }
            Haptics.Medium();
            yield return new WaitForSeconds(1f);
            Fail();
        }
    }
}
