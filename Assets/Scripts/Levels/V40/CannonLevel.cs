using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "Pirate cannon" — trascina sul cannone per puntarlo, tieni premuto FIRE: la potenza sale e scende,
    /// rilascia per sparare. Il vento (la manica a vento e il numero) sposta la palla e cambia dopo ogni colpo.
    /// Colpisci i tre bersagli sulla nave che dondola: il barile di polvere, la bandiera in cima all'albero e il
    /// cappello del capitano (6 palle; finite le palle si perde). Dopo il terzo colpo la nave affonda e galleggia un
    /// forziere: con la stessa mira si spara un rampino legato a una corda per agganciarlo e tirarlo a riva (3 lanci).
    /// </summary>
    public class CannonLevel : LevelController
    {
        [SerializeField] StageHud40D hud;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] Transform barrel;            // canna (ruota attorno all'asse)
        [SerializeField] Transform muzzle;            // bocca (figlio della canna)
        [SerializeField] Transform[] aimDots;
        [SerializeField] Collider2D fireButton;
        [SerializeField] Transform fireArt;
        [SerializeField] Transform power;             // perno della barra verticale (scala y)
        [SerializeField] SpriteRenderer powerFill;
        [SerializeField] Transform ball;
        [SerializeField] Transform hook;
        [SerializeField] LineRenderer rope;
        [SerializeField] Transform ship;
        [SerializeField] Transform[] targets;         // barile, bandiera, cappello
        [SerializeField] Transform captain;
        [SerializeField] Transform windsock;
        [SerializeField] TextMesh windText;
        [SerializeField] SpriteRenderer[] ballIcons;
        [SerializeField] Transform boom;
        [SerializeField] Transform splash;
        [SerializeField] Transform chest;
        [SerializeField] SpriteRenderer chestArt;
        [SerializeField] Sprite chestOpenSprite;
        [SerializeField] Transform[] coins;
        [SerializeField] Transform[] bubbles;
        [SerializeField] float gravity = 9f, minSpeed = 5.5f, maxSpeed = 11.5f, hitRadius = 0.45f, waterY = -0.05f;

        static readonly float[] Winds = { 0.8f, -1.2f, 1.6f, -0.6f, 1.2f, -1.6f, 0f, 1f };
        static readonly string[] Ouch = { "BOOM! My powder!", "My FLAG!", "ARRR! My HAT!" };

        Camera cam;
        float angle = 45f, wind, bubbleUntil, clock, shipX, shipY;
        int balls = 6, hooks = 3, hits, windIndex;
        bool[] hit;
        bool aiming, charging, sinking;
        float chargeT, powerValue, lastPower = 0.6f;
        LineRenderer preview, trail;          // traiettoria prevista (arcobaleno) e scia del colpo
        Transform previewTarget;               // il bersaglio che la traiettoria prevista colpisce
        readonly System.Collections.Generic.List<Vector3> trailPts = new System.Collections.Generic.List<Vector3>();

        public override void OnLevelStart()
        {
            cam = Camera.main;
            hit = new bool[targets.Length];
            shipX = ship.position.x;
            shipY = ship.position.y;
            ball.gameObject.SetActive(false);
            hook.gameObject.SetActive(false);
            rope.gameObject.SetActive(false);
            boom.gameObject.SetActive(false);
            splash.gameObject.SetActive(false);
            chest.gameObject.SetActive(false);
            foreach (Transform c in coins) c.gameObject.SetActive(false);
            foreach (Transform b in bubbles) b.gameObject.SetActive(false);
            preview = MakeLine("Preview", 0.09f, 22);
            trail = MakeLine("Trail", 0.06f, 21);
            foreach (Transform d in aimDots) d.gameObject.SetActive(false);   // al loro posto la traiettoria completa
            SetWind(Winds[0]);
            SetAngle(45f);
            power.localScale = new Vector3(1f, 0f, 1f);
            StartCoroutine(Run());
        }

        void Say(string text, float seconds)
        {
            if (bubble == null) return;
            bubble.Show(text);
            bubbleUntil = clock + seconds;
        }

        void SetWind(float w)
        {
            wind = w;
            float mag = Mathf.Abs(w);
            windsock.localScale = new Vector3((w >= 0f ? 1f : -1f) * Mathf.Lerp(0.35f, 1f, mag / 1.6f), mag < 0.01f ? 0.5f : 1f, 1f);
            windText.text = mag < 0.01f ? "NO WIND" : "WIND " + (w > 0 ? "> " : "< ") + Mathf.RoundToInt(mag * 5f / 1.6f);
        }

        void SetAngle(float a)
        {
            angle = Mathf.Clamp(a, 5f, 85f);
            barrel.localRotation = Quaternion.Euler(0, 0, angle);
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            for (int i = 0; i < aimDots.Length; i++) aimDots[i].position = muzzle.position + (Vector3)(dir * (0.35f + i * 0.32f));
        }

        // ---------- traiettoria arcobaleno e raffiche ----------

        LineRenderer MakeLine(string name, float width, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            LineRenderer lr = go.AddComponent<LineRenderer>();
            lr.useWorldSpace = true;
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.widthMultiplier = width;
            lr.numCapVertices = 4;
            lr.numCornerVertices = 2;
            lr.sortingOrder = order;
            var g = new Gradient();
            g.SetKeys(
                new[] { new GradientColorKey(new Color(1f, 0.3f, 0.3f), 0f), new GradientColorKey(new Color(1f, 0.65f, 0.2f), 0.2f),
                        new GradientColorKey(new Color(1f, 0.95f, 0.3f), 0.4f), new GradientColorKey(new Color(0.4f, 0.95f, 0.45f), 0.6f),
                        new GradientColorKey(new Color(0.35f, 0.7f, 1f), 0.8f), new GradientColorKey(new Color(0.75f, 0.45f, 1f), 1f) },
                new[] { new GradientAlphaKey(0.95f, 0f), new GradientAlphaKey(0.85f, 0.7f), new GradientAlphaKey(0.35f, 1f) });
            lr.colorGradient = g;
            lr.positionCount = 0;
            return lr;
        }

        /// <summary>
        /// Traiettoria prevista con il vento di ADESSO (una raffica durante il volo può spostarla): arco arcobaleno
        /// fino al punto d'arrivo, e il bersaglio che verrebbe colpito pulsa.
        /// </summary>
        void UpdatePreview(float p)
        {
            if (preview == null) return;
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            Vector2 v = dir * Mathf.Lerp(minSpeed, maxSpeed, p);
            Vector2 pos = muzzle.position;
            var pts = new System.Collections.Generic.List<Vector3> { pos };
            Transform aimed = null;
            const float dt = 1f / 60f;
            for (int i = 0; i < 300 && aimed == null; i++)
            {
                v += new Vector2(wind, -gravity) * dt;
                pos += v * dt;
                if (i % 2 == 0) pts.Add(pos);
                if (chest.gameObject.activeInHierarchy) { if (Vector2.Distance(pos, chest.position) < 0.55f) aimed = chest; }
                else for (int t = 0; t < targets.Length; t++)
                        if (!hit[t] && Vector2.Distance(pos, targets[t].position) < hitRadius) { aimed = targets[t]; break; }
                if (pos.y < waterY && v.y < 0f && pos.x > -0.8f) break;
                if (pos.y < -5.5f || Mathf.Abs(pos.x) > 3.3f || pos.y > 9f) break;
            }
            pts.Add(pos);
            preview.positionCount = pts.Count;
            preview.SetPositions(pts.ToArray());
            if (previewTarget != aimed) { if (previewTarget != null) previewTarget.localScale = TargetScale(previewTarget); previewTarget = aimed; }
            if (previewTarget != null) previewTarget.localScale = TargetScale(previewTarget) * (1f + 0.12f * Mathf.Abs(Mathf.Sin(clock * 8f)));
        }

        readonly System.Collections.Generic.Dictionary<Transform, Vector3> targetScales = new System.Collections.Generic.Dictionary<Transform, Vector3>();
        Vector3 TargetScale(Transform t)
        {
            if (!targetScales.TryGetValue(t, out Vector3 s)) { s = t.localScale; targetScales[t] = s; }
            return s;
        }

        void HidePreview()
        {
            if (preview != null) preview.positionCount = 0;
            if (previewTarget != null) { previewTarget.localScale = TargetScale(previewTarget); previewTarget = null; }
        }

        void StartTrail(Vector2 p) { trailPts.Clear(); trailPts.Add(p); if (trail != null) trail.positionCount = 0; }

        void AddTrail(Vector2 p)
        {
            if (trail == null || ((Vector2)trailPts[trailPts.Count - 1] - p).sqrMagnitude < 0.01f) return;
            trailPts.Add(p);
            trail.positionCount = trailPts.Count;
            trail.SetPositions(trailPts.ToArray());
        }

        /// <summary>Dal secondo colpo in poi, a volte una raffica cambia il vento a metà volo.</summary>
        float GustTime() => windIndex > 0 && Random.value < 0.4f ? Random.Range(0.35f, 0.7f) : float.MaxValue;

        void Gust()
        {
            float w;
            do w = Winds[Random.Range(0, Winds.Length)]; while (Mathf.Approximately(w, wind));
            SetWind(w);
            Say("GUST! The wind changed!", 1.2f);
            Sfx.Play(this, "whoosh", 0.6f);
        }

        void Update()
        {
            clock += Time.deltaTime;
            if (bubble != null && bubbleUntil > 0f && clock > bubbleUntil) { bubble.Hide(); bubbleUntil = 0f; }
            // la nave (o il forziere) dondola sulle onde
            if (!sinking)
            {
                ship.position = new Vector3(shipX + Mathf.Sin(clock * 0.5f) * 0.25f, shipY + Mathf.Sin(clock * 1.6f) * 0.08f, ship.position.z);
                ship.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(clock * 1.1f) * 3f);
            }
            windsock.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(clock * 7f) * 4f * Mathf.Abs(wind));
        }

        /// <summary>Mira e carica; restituisce true quando si rilascia FIRE (power = potenza 0..1).</summary>
        bool AimAndCharge(out float shotPower)
        {
            shotPower = 0f;
            UpdatePreview(charging ? powerValue : lastPower);
            if (!PointerInput.TryGetPrimary(out Vector2 sp, out bool began, out bool ended))
            {
                aiming = false;
                if (charging) { charging = false; shotPower = powerValue; lastPower = powerValue; HidePreview(); return true; }
                return false;
            }
            Vector2 w = PointerInput.ToWorld(cam, sp);
            if (began)
            {
                if (PointerInput.IsOverUi()) return false;
                if (fireButton.OverlapPoint(w)) { charging = true; chargeT = 0f; fireArt.localScale = Vector3.one * 0.9f; Sfx.Play(this, "tick", 0.6f); }
                else if (Vector2.Distance(w, barrel.position) < 1.8f || w.y < -1.5f) aiming = true;
            }
            if (aiming)
            {
                Vector2 d = w - (Vector2)barrel.position;
                if (d.sqrMagnitude > 0.04f) SetAngle(Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg);
                if (ended) aiming = false;
            }
            if (charging)
            {
                chargeT += Time.deltaTime;
                powerValue = Mathf.PingPong(chargeT / 0.9f, 1f);
                power.localScale = new Vector3(1f, powerValue, 1f);
                powerFill.color = Color.Lerp(new Color(1f, 0.85f, 0.25f), new Color(0.95f, 0.25f, 0.2f), powerValue);
                if (ended) { charging = false; fireArt.localScale = Vector3.one; shotPower = powerValue; lastPower = powerValue; HidePreview(); return true; }
            }
            return false;
        }

        IEnumerator Run()
        {
            hud.SetStage(0);
            hud.Goal("Drag the cannon to aim. Hold FIRE, let go to shoot!");
            Say("Sink 'em! Hit the barrel, the flag and the hat!", 2.6f);
            while (hits < targets.Length)
            {
                if (balls <= 0)
                {
                    hud.Goal("Out of cannonballs!");
                    Say("Har har! Ye missed, landlubber!", 2.5f);
                    yield return new WaitForSeconds(2.2f);
                    Fail();
                    yield break;
                }
                if (AimAndCharge(out float p))
                {
                    balls--;
                    if (balls < ballIcons.Length) ballIcons[balls].color = new Color(1f, 1f, 1f, 0.2f);
                    yield return Shoot(p);
                    power.localScale = new Vector3(1f, 0f, 1f);
                    windIndex++;
                    SetWind(Winds[windIndex % Winds.Length]);
                }
                yield return null;
            }
            yield return Sink();
            yield return HookStage();
        }

        IEnumerator Shoot(float p)
        {
            Haptics.Light();
            Sfx.Play(this, "clunk", 0.9f);
            Sfx.Play(this, "whoosh", 0.5f);
            StartCoroutine(Recoil());
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            Vector2 v = dir * Mathf.Lerp(minSpeed, maxSpeed, p);
            Vector2 pos = muzzle.position;
            ball.gameObject.SetActive(true);
            int struck = -1;
            float flight = 0f, gustAt = GustTime();
            StartTrail(pos);
            while (true)
            {
                flight += Time.deltaTime;
                if (flight >= gustAt) { gustAt = float.MaxValue; Gust(); }
                v += new Vector2(wind, -gravity) * Time.deltaTime;
                pos += v * Time.deltaTime;
                ball.position = pos;
                AddTrail(pos);
                for (int i = 0; i < targets.Length; i++)
                    if (!hit[i] && Vector2.Distance(pos, targets[i].position) < hitRadius) { struck = i; break; }
                if (struck >= 0) break;
                if (pos.y < waterY && v.y < 0f && pos.x > -0.8f) break;   // ricade in mare (vicino alla nave)
                if (pos.y < -5.5f || Mathf.Abs(pos.x) > 3.3f || pos.y > 9f) break;
                yield return null;
            }
            ball.gameObject.SetActive(false);
            if (struck >= 0)
            {
                hit[struck] = true;
                hits++;
                yield return Explode(struck, pos);
            }
            else if (pos.y < waterY && pos.x > -0.8f && pos.y > -5.5f)
            {
                yield return Splash(pos);
                Say(balls > 0 ? "Splash! " + balls + " balls left." : "Splash!", 1.4f);
            }
            else Say("Missed! Mind the wind.", 1.4f);
        }

        IEnumerator Recoil()
        {
            Vector3 home = barrel.localPosition;
            Vector3 back = home - barrel.right * 0.15f;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.25f) { barrel.localPosition = Vector3.Lerp(back, home, k); yield return null; }
            barrel.localPosition = home;
        }

        IEnumerator Explode(int i, Vector2 at)
        {
            Haptics.Heavy();
            Sfx.Play(this, "clunk", 1f);
            Sfx.Play(this, "buzz", 0.5f);
            boom.position = at;
            boom.gameObject.SetActive(true);
            hud.SetStage(hits);
            Transform t = targets[i];
            Vector3 from = t.position;
            Vector3 v = new Vector3(Random.Range(0.5f, 1.5f), 3.5f, 0f);
            float spin = Random.Range(-600f, 600f);
            for (float k = 0f; k < 0.9f; k += Time.deltaTime)
            {
                boom.localScale = Vector3.one * (0.4f + k * 1.2f) * (k < 0.5f ? 1f : (0.9f - k) * 2.5f);
                v.y -= 9f * Time.deltaTime;
                t.position += v * Time.deltaTime;
                t.Rotate(0, 0, spin * Time.deltaTime);
                if (i == 0) t.localScale = Vector3.one * Mathf.Max(0f, 1f - k * 2f);
                if (i == 2 && captain != null) captain.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(k * 40f) * 10f * (0.9f - k));
                yield return null;
            }
            boom.gameObject.SetActive(false);
            if (captain != null) captain.localRotation = Quaternion.identity;
            t.gameObject.SetActive(false);
            t.position = from;
            Say(Ouch[Mathf.Clamp(i, 0, Ouch.Length - 1)], 1.6f);
            if (hits < targets.Length) hud.Goal("Hit " + hits + "/3! " + balls + " cannonballs left");
        }

        IEnumerator Splash(Vector2 at)
        {
            Sfx.Play(this, "whoosh", 0.4f);
            splash.position = new Vector3(at.x, waterY + 0.1f, 0f);
            splash.gameObject.SetActive(true);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.5f) { splash.localScale = new Vector3(0.6f + k * 0.4f, 0.4f + Mathf.Sin(k * Mathf.PI) * 0.8f, 1f); yield return null; }
            splash.gameObject.SetActive(false);
        }

        // ---------- la nave affonda, spunta il forziere ----------

        IEnumerator Sink()
        {
            sinking = true;
            hud.Goal("She's going down!");
            Say("Abandon ship!! Glub...", 2f);
            Sfx.Play(this, "whoosh", 0.8f);
            Vector3 from = ship.position;
            for (int i = 0; i < bubbles.Length; i++) bubbles[i].gameObject.SetActive(true);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 2.2f)
            {
                ship.position = from + Vector3.down * k * 3.2f;
                ship.localRotation = Quaternion.Euler(0, 0, -k * 25f);
                for (int i = 0; i < bubbles.Length; i++)
                {
                    float ph = Mathf.Repeat(k * 3f + i * 0.27f, 1f);
                    bubbles[i].position = new Vector3(from.x - 0.6f + i * 0.3f + Mathf.Sin(ph * 9f + i) * 0.1f, waterY - 0.3f + ph * 0.5f, 0f);
                    bubbles[i].localScale = Vector3.one * (0.3f + ph * 0.3f);
                }
                yield return null;
            }
            foreach (Transform b in bubbles) b.gameObject.SetActive(false);
            ship.gameObject.SetActive(false);
            Haptics.Light();
            chest.gameObject.SetActive(true);
            Vector3 up = new Vector3(from.x, waterY + 0.1f, 0f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.5f) { chest.position = up + Vector3.down * (1f - k) * 0.6f; yield return null; }
            Sfx.Play(this, "pop", 0.8f);
            yield return hud.Banner("TREASURE!");
        }

        IEnumerator HookStage()
        {
            hud.SetStage(3);
            hud.Goal("Hook the chest! Same aim, hold FIRE (3 hooks)");
            Say("Fire the rope hook at the chest!", 2.2f);
            Vector3 home = chest.position;
            float t = 0f;
            while (true)
            {
                t += Time.deltaTime;
                chest.position = home + new Vector3(Mathf.Sin(t * 0.6f) * 0.6f, Mathf.Sin(t * 2f) * 0.06f, 0f);
                chest.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(t * 1.7f) * 6f);
                if (AimAndCharge(out float p))
                {
                    hooks--;
                    var result = new bool[1];
                    yield return Throw(p, result);
                    power.localScale = new Vector3(1f, 0f, 1f);
                    if (result[0]) break;
                    windIndex++;
                    SetWind(Winds[windIndex % Winds.Length]);
                    if (hooks <= 0)
                    {
                        hud.Goal("Out of hooks!");
                        Say("Glub glub... the treasure sinks.", 2.4f);
                        for (float k = 0f; k < 1f; k += Time.deltaTime / 1.2f) { chest.position += Vector3.down * Time.deltaTime * 0.8f; yield return null; }
                        Fail();
                        yield break;
                    }
                    hud.Goal("Missed! " + hooks + (hooks == 1 ? " hook" : " hooks") + " left");
                }
                yield return null;
            }
            yield return Treasure();
        }

        /// <summary>Lancia il rampino; result[0] = agganciato.</summary>
        IEnumerator Throw(float p, bool[] result)
        {
            Sfx.Play(this, "whoosh", 0.8f);
            StartCoroutine(Recoil());
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            Vector2 v = dir * Mathf.Lerp(minSpeed, maxSpeed, p);
            Vector2 pos = muzzle.position;
            hook.gameObject.SetActive(true);
            rope.gameObject.SetActive(true);
            bool caught = false;
            float flight = 0f, gustAt = GustTime();
            StartTrail(pos);
            while (true)
            {
                flight += Time.deltaTime;
                if (flight >= gustAt) { gustAt = float.MaxValue; Gust(); }
                AddTrail(pos);
                v += new Vector2(wind, -gravity) * Time.deltaTime;
                pos += v * Time.deltaTime;
                hook.position = pos;
                hook.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg);
                rope.SetPosition(0, muzzle.position);
                rope.SetPosition(1, pos);
                if (Vector2.Distance(pos, chest.position) < 0.55f) { caught = true; break; }
                if ((pos.y < waterY - 0.1f && v.y < 0f) || pos.y < -5.5f || Mathf.Abs(pos.x) > 3.3f) break;
                yield return null;
            }
            if (!caught && pos.y >= -5f && pos.x > -0.8f) StartCoroutine(Splash(pos));
            result[0] = caught;
            if (caught) { Haptics.Medium(); Sfx.Play(this, "clunk", 0.8f); Say("Got it! Reel it in!", 1.6f); }
            // si riavvolge la corda (col forziere, se agganciato)
            Vector3 from = hook.position, cFrom = chest.position;
            Vector3 to = muzzle.position;
            float dur = caught ? 1.1f : 0.5f;
            for (float k = 0f; k < 1f; k += Time.deltaTime / dur)
            {
                Vector3 h = Vector3.Lerp(from, to, k) + Vector3.up * Mathf.Sin(k * Mathf.PI) * (caught ? 0.5f : 0.2f);
                hook.position = h;
                if (caught) chest.position = h + (cFrom - from) * (1f - k);
                rope.SetPosition(0, muzzle.position);
                rope.SetPosition(1, h);
                yield return null;
            }
            hook.gameObject.SetActive(false);
            rope.gameObject.SetActive(false);
        }

        IEnumerator Treasure()
        {
            hud.SetStage(4);
            hud.Goal("TREASURE!");
            Vector3 land = barrel.position + new Vector3(1.0f, -0.5f, 0f);
            Vector3 from = chest.position;
            chest.localRotation = Quaternion.identity;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.4f) { chest.position = Vector3.Lerp(from, land, k) + Vector3.up * Mathf.Sin(k * Mathf.PI) * 0.4f; yield return null; }
            chest.position = land;
            Haptics.Light();
            Sfx.Play(this, "clunk", 0.7f);
            yield return new WaitForSeconds(0.3f);
            chestArt.sprite = chestOpenSprite;
            Sfx.Play(this, "ding", 1f);
            Say("Yo ho ho! Gold!", 2.4f);
            Vector3[] cv = new Vector3[coins.Length];
            for (int i = 0; i < coins.Length; i++)
            {
                coins[i].gameObject.SetActive(true);
                coins[i].position = land + Vector3.up * 0.3f;
                cv[i] = new Vector3(Random.Range(-1.8f, 1.8f), Random.Range(4f, 7f), 0f);
            }
            for (float t = 0f; t < 2.2f; t += Time.deltaTime)
            {
                for (int i = 0; i < coins.Length; i++)
                {
                    cv[i].y -= 10f * Time.deltaTime;
                    coins[i].position += cv[i] * Time.deltaTime;
                    coins[i].localScale = new Vector3(Mathf.Cos(t * 10f + i), 1f, 1f) * 0.6f;
                }
                yield return null;
            }
            Win();
        }
    }
}
