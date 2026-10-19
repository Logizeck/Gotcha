using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello composito "Morning routine" — il gatto deve prepararsi, in 4 fasi.
    /// 1) La sveglia suona ma il gatto dorme: toccarlo non basta ("5 more minutes"), bisogna SCUOTERE il telefono.
    /// 2) La stanza è buia: trascinare di lato le due tende (se le lasci a metà tornano indietro).
    /// 3) Colazione: inclinare il telefono a sinistra per versare i cereali; più inclini più ne escono. Troppo
    ///    inclinato = rovesci la scatola (un tentativo perso, si ricomincia); oltre la linea = trabocca. Bisogna
    ///    raddrizzare la scatola quando i cereali sono sulla linea arancione.
    /// 4) Toccare il cartone del latte, poi TENERE premuto sulla ciotola: il latte scende sempre più veloce;
    ///    lasciare quando arriva alla fascia verde (sotto: "A bit more!", sopra il bordo: trabocca, tentativo perso).
    /// 3 tentativi in tutto. Vittoria: il gatto mangia felice.
    /// </summary>
    public class MorningLevel : LevelController
    {
        [SerializeField] StageHud40A hud;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] GameObject bedroom;
        [SerializeField] GameObject kitchen;
        [Header("Fase 1")]
        [SerializeField] Transform clock;
        [SerializeField] GameObject clockWaves;
        [SerializeField] Collider2D clockArea;
        [SerializeField] Transform sleepyCat;
        [SerializeField] GameObject catEyes;
        [SerializeField] Collider2D catArea;
        [SerializeField] Transform zzz;
        [Header("Fase 2")]
        [SerializeField] Transform curtainLeft;
        [SerializeField] Transform curtainRight;
        [SerializeField] SpriteRenderer darkness;
        [SerializeField] SpriteRenderer sunGlow;
        [Header("Fase 3")]
        [SerializeField] Transform cerealBox;
        [SerializeField] Transform boxMouth;
        [SerializeField] Transform[] flakes;
        [SerializeField] Transform bowl;
        [SerializeField] Transform cerealFill;
        [Header("Fase 4")]
        [SerializeField] Transform milkCarton;
        [SerializeField] Transform milkSpout;
        [SerializeField] Collider2D milkArea;
        [SerializeField] Collider2D bowlArea;
        [SerializeField] Transform milkFill;
        [SerializeField] SpriteRenderer milkZone;
        [SerializeField] SpriteRenderer stream;
        [SerializeField] SpriteRenderer kitchenCat;
        [SerializeField] Sprite catHappy;

        // geometria della ciotola (coordinate locali della ciotola)
        const float BowlBottom = -0.6f, CerealLine = -0.05f, MilkLine = 0.35f, Rim = 0.6f;

        Camera cam;
        bool failing;
        float cereal, milk;               // livelli: 1 = sulla linea
        Vector3[] flakeVel;
        float bubbleUntil;
        Vector3 boxHome, cartonHome;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            flakeVel = new Vector3[flakes.Length];
            foreach (Transform f in flakes) f.gameObject.SetActive(false);
            bedroom.SetActive(true);
            kitchen.SetActive(false);
            catEyes.SetActive(false);
            stream.enabled = false;
            milkZone.enabled = false;
            boxHome = cerealBox.localPosition;
            cartonHome = milkCarton.localPosition;
            SetCereal(0f);
            SetMilk(0f);
            StartCoroutine(Run());
        }

        void Say(string text, float seconds)
        {
            if (bubble == null) return;
            bubble.Show(text);
            bubbleUntil = Time.time + seconds;
        }

        void Update()
        {
            if (bubble != null && bubbleUntil > 0f && Time.time > bubbleUntil) { bubble.Hide(); bubbleUntil = 0f; }
        }

        IEnumerator Run()
        {
            yield return Stage1Alarm();
            yield return hud.Clear("RISE AND SHINE!");
            yield return Stage2Curtains();
            yield return hud.Clear("GOOD MORNING!");
            yield return hud.Fade(1f, 0.35f);
            bedroom.SetActive(false);
            kitchen.SetActive(true);
            yield return hud.Fade(0f, 0.35f);
            yield return Stage3Cereal();
            if (failing) yield break;
            yield return hud.Clear("PERFECT POUR!");
            yield return Stage4Milk();
            if (failing) yield break;
            yield return Breakfast();
        }

        bool Tapped(out Vector2 w)
        {
            w = default;
            if (!PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out _) || !began || PointerInput.IsOverUi()) return false;
            w = PointerInput.ToWorld(cam, sp);
            return true;
        }

        // ---------- Fase 1: la sveglia ----------

        IEnumerator Stage1Alarm()
        {
            hud.Stage(0, "Wake the cat up!");
            AudioSource ring = Synth40A.Loop(this, "ring", 0.35f);
            Vector3 clockHome = clock.localPosition, catHome = sleepyCat.localPosition, zHome = zzz.localPosition;
            float wake = 0f, snoreAt = 1f;
            int taps = 0;
            string[] sleepy = { "Zzz... 5 more minutes...", "Mrrr... it's Sunday...", "Zzz... the bed is comfy..." };
            while (wake < 1f)
            {
                float dt = Time.deltaTime;
                clock.localPosition = clockHome + new Vector3(Mathf.Sin(Time.time * 70f) * 0.03f, 0f, 0f);
                clock.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(Time.time * 55f) * 4f);
                zzz.localPosition = zHome + new Vector3(Mathf.Sin(Time.time * 2f) * 0.08f, Mathf.Repeat(Time.time * 0.3f, 0.4f), 0f);
                snoreAt -= dt;
                if (snoreAt <= 0f) { snoreAt = 2.2f; Sfx.Play(this, "snore", 0.35f); }

                if (Tapped(out Vector2 w))
                {
                    if (catArea.OverlapPoint(w))
                    {
                        taps++;
                        Sfx.Play(this, "pop", 0.5f);
                        Say(taps >= 3 ? "Zzz... only an EARTHQUAKE could wake me..." : sleepy[(taps - 1) % sleepy.Length], 2.2f);
                    }
                    else if (clockArea.OverlapPoint(w)) Say("The alarm isn't enough. Zzz...", 1.8f);
                }

                float shake = Inputs.Shake();
                if (shake > 4f) wake += dt * 0.65f;
                else wake = Mathf.Max(0f, wake - dt * 0.12f);
                float j = wake * 0.12f;
                sleepyCat.localPosition = catHome + (Vector3)(Random.insideUnitCircle * (shake > 4f ? j : 0f));
                if (wake > 0.3f && wake < 0.32f) Say("Huh?! The bed is shaking!", 1.2f);
                yield return null;
            }
            sleepyCat.localPosition = catHome;
            clock.localPosition = clockHome;
            clock.localRotation = Quaternion.identity;
            ring.Stop();
            clockWaves.SetActive(false);
            zzz.gameObject.SetActive(false);
            catEyes.SetActive(true);
            Haptics.Medium();
            Sfx.Play(this, "bonk", 0.7f);
            Say("MRAOW! I'm up! ...Why is it so dark?", 2.2f);
            for (float k = 0f; k < 0.4f; k += Time.deltaTime) { sleepyCat.localPosition = catHome + Vector3.up * Mathf.Sin(k / 0.4f * Mathf.PI) * 0.35f; yield return null; }
            sleepyCat.localPosition = catHome;
        }

        // ---------- Fase 2: le tende ----------

        IEnumerator Stage2Curtains()
        {
            hud.Stage(1, "Let the sun in! Drag the curtains open");
            const float closedX = 0.68f, openX = 2.05f;
            Transform held = null;
            float grabOffset = 0f;
            float openL = 0f, openR = 0f;
            while (openL < 0.92f || openR < 0.92f)
            {
                bool down = PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out _) && !PointerInput.IsOverUi();
                Vector2 w = down ? PointerInput.ToWorld(cam, sp) : Vector2.zero;
                if (down && began)
                {
                    foreach (Transform c in new[] { curtainLeft, curtainRight })
                    {
                        Collider2D col = c.GetComponent<Collider2D>();
                        if (col != null && col.OverlapPoint(w)) { held = c; grabOffset = c.position.x - w.x; Sfx.Play(this, "whoosh", 0.25f); }
                    }
                }
                if (!down) held = null;
                if (held != null)
                {
                    float x = Mathf.Abs(w.x + grabOffset - transform.position.x);
                    float t = Mathf.Clamp01((x - closedX) / (openX - closedX));
                    if (held == curtainLeft) openL = t; else openR = t;
                }
                // le tende lasciate a metà ricadono al centro
                if (held != curtainLeft && openL < 0.7f) openL = Mathf.Max(0f, openL - Time.deltaTime * 0.8f);
                if (held != curtainRight && openR < 0.7f) openR = Mathf.Max(0f, openR - Time.deltaTime * 0.8f);
                if (held != curtainLeft && openL >= 0.7f) openL = Mathf.MoveTowards(openL, 1f, Time.deltaTime * 2f);
                if (held != curtainRight && openR >= 0.7f) openR = Mathf.MoveTowards(openR, 1f, Time.deltaTime * 2f);
                SetCurtain(curtainLeft, -1f, openL, closedX, openX);
                SetCurtain(curtainRight, 1f, openR, closedX, openX);
                float light = (openL + openR) * 0.5f;
                darkness.color = new Color(0.02f, 0.02f, 0.08f, 0.72f * (1f - light));
                sunGlow.color = new Color(1f, 0.95f, 0.7f, 0.5f * light);
                yield return null;
            }
            SetCurtain(curtainLeft, -1f, 1f, closedX, openX);
            SetCurtain(curtainRight, 1f, 1f, closedX, openX);
            darkness.enabled = false;
            Say("Ahh, sunshine! Now... BREAKFAST!", 2f);
            yield return new WaitForSeconds(0.6f);
        }

        void SetCurtain(Transform c, float side, float t, float closedX, float openX)
        {
            Vector3 p = c.localPosition;
            p.x = side * Mathf.Lerp(closedX, openX, t);
            c.localPosition = p;
            c.localScale = new Vector3(Mathf.Lerp(1f, 0.6f, t), 1f, 1f);   // si raccoglie aprendosi
        }

        // ---------- Fase 3: i cereali ----------

        void SetCereal(float f)
        {
            cereal = f;
            float top = BowlBottom + f * (CerealLine - BowlBottom);
            cerealFill.localPosition = new Vector3(0f, top, 0f);
        }

        void SetMilk(float m)
        {
            milk = m;
            float top = BowlBottom + m * (MilkLine - BowlBottom);
            Vector3 s = milkFill.localScale;
            milkFill.localPosition = new Vector3(0f, top - s.y * 0.5f, 0f);
        }

        float SurfaceWorldY() => bowl.TransformPoint(new Vector3(0f, Mathf.Max(BowlBottom + cereal * (CerealLine - BowlBottom), BowlBottom + milk * (MilkLine - BowlBottom)), 0f)).y;

        IEnumerator Stage3Cereal()
        {
            hud.Stage(2, "Tilt the phone to pour cereal up to the line!");
            Say("Tilt LEFT to pour. Gently!", 2.4f);
            float angle = 0f, overFor = 0f, uprightFor = 0f, spawn = 0f;
            while (true)
            {
                float dt = Time.deltaTime;
                Vector2 tilt = Inputs.Tilt();
                float target = Mathf.Clamp(-tilt.x * 105f, 0f, 120f);
                angle = Mathf.Lerp(angle, target, 6f * dt);
                cerealBox.localRotation = Quaternion.Euler(0, 0, angle);

                // rovesciata: troppo inclinata
                overFor = angle > 92f ? overFor + dt : 0f;
                if (overFor > 0.2f) { yield return Spill("Whoa! You dumped the WHOLE box!"); if (failing) yield break; angle = 0f; continue; }

                if (angle > 35f)
                {
                    float rate = Mathf.Lerp(0.06f, 0.5f, Mathf.InverseLerp(35f, 88f, angle));
                    SetCereal(cereal + rate * dt);
                    spawn -= dt;
                    if (spawn <= 0f) { spawn = Mathf.Lerp(0.18f, 0.04f, Mathf.InverseLerp(35f, 88f, angle)); Drop(boxMouth.position, 0.4f); }
                    if (Random.value < dt * 6f) Synth40A.Play(this, "munch", 0.12f);
                }
                MoveFlakes(dt);

                if (cereal > 1.3f) { yield return Spill("Too much! It's overflowing!"); if (failing) yield break; angle = 0f; continue; }
                bool good = cereal >= 0.85f && cereal <= 1.3f;
                uprightFor = good && angle < 25f ? uprightFor + dt : 0f;
                if (uprightFor > 0.6f) break;
                yield return null;
            }
            cerealBox.localRotation = Quaternion.identity;
            foreach (Transform f in flakes) f.gameObject.SetActive(false);
            Say("Crunchy! Now the milk.", 1.6f);
            // la scatola esce di scena a destra
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.4f) { cerealBox.localPosition = boxHome + Vector3.right * 3f * k * k; yield return null; }
            cerealBox.gameObject.SetActive(false);
        }

        void Drop(Vector3 from, float spread)
        {
            for (int i = 0; i < flakes.Length; i++)
            {
                if (flakes[i].gameObject.activeSelf) continue;
                flakes[i].gameObject.SetActive(true);
                flakes[i].position = from + new Vector3(Random.Range(-0.05f, 0.05f), 0f, 0f);
                flakeVel[i] = new Vector3(Random.Range(-spread, spread * 0.3f), Random.Range(-0.3f, 0.3f), 0f);
                return;
            }
        }

        void MoveFlakes(float dt)
        {
            float floor = SurfaceWorldY();
            for (int i = 0; i < flakes.Length; i++)
            {
                if (!flakes[i].gameObject.activeSelf) continue;
                flakeVel[i] += Vector3.down * 9f * dt;
                flakes[i].position += flakeVel[i] * dt;
                flakes[i].Rotate(0, 0, 400f * dt);
                if (flakes[i].position.y < floor || flakes[i].position.y < -4.8f) flakes[i].gameObject.SetActive(false);
            }
        }

        /// <summary>Cereali dappertutto: un tentativo perso, ciotola svuotata e scatola di nuovo piena.</summary>
        IEnumerator Spill(string msg)
        {
            Haptics.Medium();
            Sfx.Play(this, "bonk", 0.7f);
            Synth40A.Play(this, "munch", 0.5f);
            Say(msg, 2f);
            Vector3 mouth = boxMouth.position;
            for (float k = 0f; k < 0.6f; k += Time.deltaTime)
            {
                if (Random.value < 0.7f) Drop(mouth, 1.6f);
                MoveFlakes(Time.deltaTime);
                yield return null;
            }
            if (hud.LoseTry() <= 0) { yield return FailOut("The kitchen is a total mess!"); yield break; }
            yield return new WaitForSeconds(0.5f);
            foreach (Transform f in flakes) f.gameObject.SetActive(false);
            cerealBox.localRotation = Quaternion.identity;
            SetCereal(0f);
            Say("New box, clean bowl. Try again!", 1.8f);
        }

        IEnumerator FailOut(string msg)
        {
            failing = true;
            Say(msg, 3f);
            Sfx.Play(this, "buzz", 0.6f);
            yield return new WaitForSeconds(1.6f);
            Fail();
        }

        // ---------- Fase 4: il latte ----------

        IEnumerator Stage4Milk()
        {
            hud.Stage(3, "Tap the milk, then HOLD on the bowl!");
            milkZone.enabled = true;
            bool lifted = false;
            float holdFor = 0f;
            Vector3 pourPos = cartonHome + new Vector3(-0.55f, 1.95f, 0f);
            while (true)
            {
                bool down = PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out bool ended) && !PointerInput.IsOverUi();
                Vector2 w = down ? PointerInput.ToWorld(cam, sp) : Vector2.zero;
                if (!lifted)
                {
                    if (down && began && milkArea.OverlapPoint(w))
                    {
                        lifted = true;
                        Sfx.Play(this, "whoosh", 0.4f);
                        for (float k = 0f; k < 1f; k += Time.deltaTime / 0.35f)
                        {
                            milkCarton.localPosition = Vector3.Lerp(cartonHome, pourPos, k);
                            yield return null;
                        }
                        milkCarton.localPosition = pourPos;
                        hud.Goal("Now HOLD on the bowl. Let go in the green zone!");
                        continue;
                    }
                    if (down && began && bowlArea.OverlapPoint(w)) Say("Grab the milk first! Tap the carton.", 1.8f);
                    yield return null;
                    continue;
                }

                bool pouring = down && bowlArea.OverlapPoint(w) && !ended;
                float tilt = pouring ? 80f : 15f;
                milkCarton.localRotation = Quaternion.Lerp(milkCarton.localRotation, Quaternion.Euler(0, 0, tilt), 10f * Time.deltaTime);
                if (pouring)
                {
                    holdFor += Time.deltaTime;
                    float rate = Mathf.Lerp(0.18f, 0.85f, Mathf.Clamp01(holdFor / 2.2f));   // sempre più veloce
                    if (Quaternion.Angle(milkCarton.localRotation, Quaternion.Euler(0, 0, 80f)) < 20f) SetMilk(milk + rate * Time.deltaTime);
                    ShowStream(true);
                    if (milk > 1.27f) { yield return Overflow(); if (failing) yield break; holdFor = 0f; continue; }
                }
                else
                {
                    ShowStream(false);
                    if (holdFor > 0f)
                    {
                        holdFor = 0f;
                        if (milk >= 0.88f && milk <= 1.12f) break;
                        if (milk < 0.88f) Say("A bit more!", 1.2f);
                        else Say("Careful, almost overflowing!", 1.4f);
                    }
                }
                yield return null;
            }
            ShowStream(false);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.35f)
            {
                milkCarton.localPosition = Vector3.Lerp(pourPos, cartonHome, k);
                milkCarton.localRotation = Quaternion.Lerp(milkCarton.localRotation, Quaternion.identity, k);
                yield return null;
            }
            milkCarton.localRotation = Quaternion.identity;
            milkZone.enabled = false;
            yield return hud.Clear("JUST RIGHT!");
        }

        void ShowStream(bool on)
        {
            stream.enabled = on;
            if (!on) return;
            Vector3 top = milkSpout.position;
            float bottom = SurfaceWorldY();
            float len = Mathf.Max(0.05f, top.y - bottom);
            stream.transform.position = new Vector3(top.x, bottom + len * 0.5f, stream.transform.position.z);
            stream.transform.localScale = new Vector3(0.09f + Mathf.Sin(Time.time * 40f) * 0.01f, len, 1f);
        }

        IEnumerator Overflow()
        {
            ShowStream(false);
            Haptics.Medium();
            Sfx.Play(this, "bonk", 0.6f);
            Say("Overflow! Milk everywhere!", 2f);
            Vector3 home = bowl.localPosition;
            for (float k = 0f; k < 0.5f; k += Time.deltaTime) { bowl.localPosition = home + new Vector3(Mathf.Sin(k * 60f) * 0.04f, 0f, 0f); yield return null; }
            bowl.localPosition = home;
            if (hud.LoseTry() <= 0) { yield return FailOut("Milk on the floor... the cat is sad."); yield break; }
            SetMilk(0f);
            Say("Fresh bowl of cereal. Again!", 1.6f);
            yield return new WaitForSeconds(0.4f);
        }

        // ---------- Vittoria ----------

        IEnumerator Breakfast()
        {
            kitchenCat.sprite = catHappy;
            Say("Purrrfect breakfast!", 2.5f);
            Vector3 catHome = kitchenCat.transform.localPosition;
            for (float k = 0f; k < 2.2f; k += Time.deltaTime)
            {
                kitchenCat.transform.localPosition = catHome + Vector3.up * Mathf.Abs(Mathf.Sin(k * 9f)) * 0.06f;
                if (Mathf.Repeat(k, 0.3f) < Time.deltaTime) Synth40A.Play(this, "munch", 0.5f);
                SetMilk(Mathf.Max(0f, milk - Time.deltaTime * 0.35f));
                SetCereal(Mathf.Max(0f, cereal - Time.deltaTime * 0.35f));
                yield return null;
            }
            kitchenCat.transform.localPosition = catHome;
            Sfx.Play(this, "ding", 0.8f);
            yield return new WaitForSeconds(0.5f);
            Win();
        }
    }
}
