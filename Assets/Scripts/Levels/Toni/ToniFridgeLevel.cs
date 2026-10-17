using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 75 — "WAKE UP, TONI!". È ora della passeggiata, ma Toni russa. La sveglia non serve (russa più
    /// forte), toccarlo nemmeno (si gira dall'altra parte), scuotere il telefono neanche. L'unico rumore che
    /// sveglia OGNI cane: la porta del frigo! Trascina la maniglia per aprirlo: Toni salta su, corre e ruba le
    /// salsicce. Easter egg: suonare la sveglia 5 volte... Toni risponde a modo suo (prrrt!).
    /// </summary>
    public class ToniFridgeLevel : LevelController
    {
        [SerializeField] ToniPug toni;
        [SerializeField] Transform fridge;           // centro del frigo
        [SerializeField] Vector2 fridgeHalf = new Vector2(0.8f, 1.7f);
        [SerializeField] Transform hinge;            // la porta ruota (si schiaccia) attorno al bordo destro
        [SerializeField] SpriteRenderer fridgeLight;
        [SerializeField] Transform sausages;
        [SerializeField] TapButton alarm;
        [SerializeField] Transform alarmArt;
        [SerializeField] SmokeEmitter toot;
        [SerializeField] float openDrag = 1.3f;
        [SerializeField] int ringsForEgg = 5;
        [SerializeField] float frontOfFridgeX = 0.2f;

        Camera cam;
        float open, openAtGrab, grabX, nextSnore, shakeCooldown, ringUntil;
        bool grabbing, awake, rolled;
        int rings;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            toni.Stand(false);
            toni.Sleep();
            alarm.Pressed += OnAlarm;
            SetOpen(0f);
            if (toot != null) toot.Stop();
            nextSnore = Time.time + 1.5f;
        }

        void OnDestroy() { if (alarm != null) alarm.Pressed -= OnAlarm; }

        void Update()
        {
            if (IsFinished) return;
            HandleDoor();
            if (ringUntil > Time.time && alarmArt != null)
            {
                alarmArt.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(Time.time * 60f) * 10f);
                if (Time.frameCount % 6 == 0) Sfx.Play(this, "tick", 0.7f);
            }
            else if (alarmArt != null) alarmArt.localRotation = Quaternion.identity;
            if (awake) return;

            if (Time.time >= nextSnore)
            {
                nextSnore = Time.time + Random.Range(3.5f, 5f);
                toni.Say(Random.value < 0.5f ? "zzz... *snort*" : "ZZZ...", 1.4f);
            }
            shakeCooldown -= Time.deltaTime;
            if (shakeCooldown <= 0f && Inputs.Shake() > 1.5f)
            {
                shakeCooldown = 1.2f;
                StartCoroutine(toni.Jump(0.2f, 0.25f));
                toni.Say("*snort*... zzz", 1.2f, "bonk");
            }
            if (PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out _) && began && !PointerInput.IsOverUi() && cam != null)
            {
                Vector2 l = toni.transform.InverseTransformPoint(PointerInput.ToWorld(cam, sp));
                if (l.x * l.x / 1.69f + (l.y - 0.2f) * (l.y - 0.2f) / 0.5f < 1f) RollOver();
            }
        }

        void RollOver()
        {
            rolled = !rolled;
            toni.Face(rolled);   // si gira dall'altra parte
            toni.Say(rolled ? "Five more minutes..." : "*snore*", 1.3f, "bonk");
        }

        void OnAlarm(TapButton b)
        {
            if (IsFinished) return;
            rings++;
            ringUntil = Time.time + 1f;
            Sfx.Play(this, "buzz", 0.7f);
            Haptics.Light();
            if (awake) return;
            if (rings >= ringsForEgg) { StartCoroutine(TootRoutine()); return; }
            string[] lines = { "zzz...", "ZZZ!", "*snore* *SNORE*", "Mmmh... no." };
            toni.Say(lines[Mathf.Min(rings - 1, lines.Length - 1)], 1.2f);
        }

        IEnumerator TootRoutine()
        {
            awake = true;   // basta interazioni
            alarm.interactable = false;
            yield return new WaitForSeconds(0.6f);
            toni.Say("PRRRRT!", 2f, "buzz");
            Haptics.Medium();
            if (toot != null) { toot.transform.position = toni.transform.TransformPoint(new Vector2(1.1f, 0.1f)); toot.Play(); }
            yield return new WaitForSeconds(1.6f);
            if (toot != null) toot.Stop();
            toni.Say("...zzz", 1.2f);
            yield return new WaitForSeconds(1f);
            FailWithEgg("toni_toot");
        }

        // ---------- Porta del frigo ----------

        void HandleDoor()
        {
            bool has = PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out bool ended) && cam != null;
            if (has && began && !PointerInput.IsOverUi())
            {
                Vector2 w = PointerInput.ToWorld(cam, sp);
                Vector2 l = fridge.InverseTransformPoint(w);
                // si prende la porta chiusa (tutto il frigo) o quella aperta (a destra della cerniera)
                bool onDoor = Mathf.Abs(l.y) < fridgeHalf.y && (open < 0.5f ? Mathf.Abs(l.x) < fridgeHalf.x : l.x > fridgeHalf.x - 0.3f);
                if (onDoor) { grabbing = true; grabX = w.x; openAtGrab = open; }
            }
            if (grabbing && has)
            {
                float x = PointerInput.ToWorld(cam, sp).x;
                SetOpen(Mathf.Clamp01(openAtGrab + (grabX - x) / openDrag));
            }
            if (grabbing && (!has || ended)) grabbing = false;
            if (!grabbing && open > 0f && open < 0.5f) SetOpen(Mathf.MoveTowards(open, 0f, 3f * Time.deltaTime));   // si richiude da sola
        }

        void SetOpen(float v)
        {
            bool wasOpen = open >= 0.5f;
            open = v;
            if (hinge != null) hinge.localScale = new Vector3(1f - 1.3f * open, 1f, 1f);
            if (fridgeLight != null) fridgeLight.color = new Color(1f, 0.95f, 0.6f, Mathf.Clamp01((open - 0.3f) * 2f) * 0.7f);
            if (open >= 0.5f && !wasOpen)
            {
                Sfx.Play(this, "clunk", 0.6f);
                if (!awake) StartCoroutine(WakeRoutine());
            }
        }

        IEnumerator WakeRoutine()
        {
            awake = true;
            alarm.interactable = false;
            yield return new WaitForSeconds(0.25f);
            toni.WakeUp(1);
            toni.Say("...?", 0.8f);
            yield return new WaitForSeconds(0.45f);
            toni.WakeUp(2);
            toni.Alert(true);
            toni.LookTarget = sausages;
            toni.Bark("FRIDGE!!");
            Haptics.Light();
            toni.Stand(true);
            yield return toni.Jump(0.5f, 0.35f);
            toni.Alert(false);
            float target = toni.transform.parent.InverseTransformPoint(new Vector3(fridge.position.x + frontOfFridgeX, 0f, 0f)).x;
            yield return toni.RunTo(target, 3.6f);
            // se nel frattempo hai chiuso il frigo, Toni piagnucola davanti alla porta finché non la riapri
            while (open < 0.5f)
            {
                toni.Say("*whine*", 0.9f);
                yield return new WaitForSeconds(1f);
            }
            yield return toni.Jump(0.6f, 0.35f);
            yield return toni.Chomp(sausages, false);
            sausages.SetParent(toni.transform, true);   // le salsicce restano in bocca
            toni.Face(false);
            yield return toni.Happy(1.2f);
            toni.Say("Walk? WALK!!", 1.4f, "woof");
            yield return new WaitForSeconds(1.2f);
            Win();
        }
    }
}
