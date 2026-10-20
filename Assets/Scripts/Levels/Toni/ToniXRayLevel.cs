using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello di Toni "TONI'S X-RAY" — Toni ha ingoiato la chiave del frigo e fa l'innocente. Lo scanner (una finestra
    /// a raggi X da trascinare) mostra dentro la pancia ossa, un calzino, una paperella e la chiave. Inclinando il
    /// telefono la chiave scivola nella pancia (fisica semplice) fino alla gola; poi si tocca il naso di Toni, che
    /// starnutisce e la sputa fuori. Tre starnuti con la chiave fuori posto = sconfitta. Senza accelerometro la
    /// chiave si sposta col dito dentro lo scanner.
    /// </summary>
    public class ToniXRayLevel : LevelController
    {
        [SerializeField] ToniPug toni;
        [SerializeField] Transform scanner, xKey, realKey, padlock, fridge;
        [SerializeField] Collider2D scannerArea;
        [SerializeField] SmokeEmitter sneezeCloud;
        [SerializeField] TextMesh strikeText;
        [SerializeField] SmokeEmitter[] confetti;
        [SerializeField] int strikesAllowed = 3;
        [SerializeField] float noseRadius = 0.5f, gravity = 3.2f;
        // la pancia (coordinate locali di Toni): ellisse + gola verso la bocca
        [SerializeField] Vector2 bellyCenter = new Vector2(0.2f, 0.06f), bellyRadii = new Vector2(0.65f, 0.27f);
        [SerializeField] Vector2 throatFrom = new Vector2(-0.35f, 0.06f), throatTo = new Vector2(-0.7f, 0.2f);
        [SerializeField] float throatRadius = 0.09f, readyDistance = 0.14f;

        Camera cam;
        bool busy, dragScanner, keyDrag, hasTilt, nosePress;
        int strikes;
        Vector2 keyPos, keyVel, dragOffset, pressAt;
        float pressTime;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            hasTilt = S41LKit.HasTilt();
            toni.Stand(false);
            toni.Face(false);
            toni.WakeUp(2);
            toni.LookPoint = toni.HeadWorld + new Vector3(0.3f, 2f, 0f);   // guarda in alto: "chi, io?"
            toni.Wag = 0.2f;
            keyPos = new Vector2(0.55f, -0.08f);
            realKey.gameObject.SetActive(false);
            sneezeCloud.Stop();
            S41LKit.Confetti(confetti, false);
            UpdateStrikes();
            PlaceKey();
            StartCoroutine(Intro());
        }

        IEnumerator Intro()
        {
            yield return new WaitForSeconds(0.6f);
            toni.Say("Fridge key? What fridge key?", 2.2f);
        }

        void UpdateStrikes() { strikeText.text = "SNEEZES " + strikes + "/" + strikesAllowed; }

        void PlaceKey()
        {
            Vector3 w = toni.transform.TransformPoint(keyPos);
            xKey.position = new Vector3(w.x, w.y, xKey.position.z);
        }

        // ---------- la pancia: distanza (negativa dentro) ----------

        float Sd(Vector2 p)
        {
            Vector2 q = (p - bellyCenter);
            float e = (new Vector2(q.x / bellyRadii.x, q.y / bellyRadii.y).magnitude - 1f) * Mathf.Min(bellyRadii.x, bellyRadii.y);
            Vector2 ab = throatTo - throatFrom;
            float t = Mathf.Clamp01(Vector2.Dot(p - throatFrom, ab) / ab.sqrMagnitude);
            float c = Vector2.Distance(p, throatFrom + ab * t) - throatRadius;
            return Mathf.Min(e, c);
        }

        void Constrain()
        {
            for (int i = 0; i < 4; i++)
            {
                float d = Sd(keyPos);
                if (d <= 0f) return;
                const float h = 0.002f;
                Vector2 g = new Vector2(Sd(keyPos + Vector2.right * h) - Sd(keyPos - Vector2.right * h), Sd(keyPos + Vector2.up * h) - Sd(keyPos - Vector2.up * h)).normalized;
                keyPos -= g * d;
                float vn = Vector2.Dot(keyVel, g);
                if (vn > 0f) keyVel -= g * vn * 1.3f;
            }
        }

        bool KeyReady => Vector2.Distance(keyPos, throatTo) < readyDistance;

        void Update()
        {
            if (IsFinished || busy) return;
            float dt = Time.deltaTime;
            bool down = S41LKit.Pointer(cam, out Vector2 w, out bool began, out bool ended);

            // fisica della chiave nella pancia: l'inclinazione è la gravità
            if (hasTilt && !keyDrag)
            {
                Vector2 g = Inputs.Tilt();
                keyVel += g * gravity * dt;
                keyVel *= 1f - Mathf.Min(1f, dt * 2.2f);
                keyPos += keyVel * dt;
                Constrain();
            }

            if (began)
            {
                pressAt = w;
                pressTime = Time.time;
                nosePress = Vector2.Distance(w, toni.NoseWorld) < noseRadius;
                if (!hasTilt && Vector2.Distance(w, xKey.position) < 0.45f && scannerArea.OverlapPoint(w)) keyDrag = true;
                else if (!nosePress && scannerArea.OverlapPoint(w)) { dragScanner = true; dragOffset = (Vector2)scanner.position - w; }
            }
            if (down && dragScanner)
            {
                Vector2 p = w + dragOffset;
                scanner.position = new Vector3(Mathf.Clamp(p.x, -2.2f, 2.2f), Mathf.Clamp(p.y, -3.8f, 2.4f), scanner.position.z);
            }
            if (down && keyDrag)
            {
                keyPos = Vector2.MoveTowards(keyPos, toni.transform.InverseTransformPoint(w), 2.5f * dt);
                keyVel = Vector2.zero;
                Constrain();
            }
            if (ended || !down)
            {
                if (nosePress && down && ended && Vector2.Distance(w, pressAt) < 0.3f && Time.time - pressTime < 0.6f) StartCoroutine(Sneeze());
                nosePress = dragScanner = keyDrag = false;
            }
            PlaceKey();
        }

        IEnumerator Sneeze()
        {
            busy = true;
            bool ready = KeyReady;
            Transform t = toni.transform;
            toni.Say("AH... AH...", 0.9f);
            Vector3 s0 = t.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.8f)
            {
                t.localScale = new Vector3(s0.x, s0.y * (1f + k * 0.08f), s0.z);
                t.localRotation = Quaternion.Euler(0, 0, -k * 8f);
                yield return null;
            }
            toni.MouthOpen(true);
            toni.Say("CHOO!", 0.9f);
            Sfx.Play(this, "whoosh", 1f);
            Haptics.Medium();
            sneezeCloud.transform.position = toni.MouthWorld;
            sneezeCloud.Play();
            t.localScale = s0;
            t.localRotation = Quaternion.Euler(0, 0, 6f);
            yield return new WaitForSeconds(0.25f);
            sneezeCloud.Stop();
            t.localRotation = Quaternion.identity;
            toni.MouthOpen(false);
            if (ready) { StartCoroutine(KeyOut()); yield break; }

            strikes++;
            UpdateStrikes();
            keyVel += new Vector2(Random.Range(-1f, 1f), 1f) * 1.5f;   // la chiave sobbalza nella pancia
            yield return new WaitForSeconds(0.4f);
            if (strikes >= strikesAllowed)
            {
                toni.Say("Too many sneezes. Nap time.", 1.6f);
                toni.Sleep();
                yield return new WaitForSeconds(1.6f);
                Fail();
                yield break;
            }
            toni.Say(strikes == 1 ? "Nothing came out..." : "Achoo... still nothing!", 1.4f);
            busy = false;
        }

        IEnumerator KeyOut()
        {
            xKey.gameObject.SetActive(false);
            realKey.gameObject.SetActive(true);
            Vector3 from = toni.MouthWorld, to = fridge.position + new Vector3(0.3f, -0.15f, 0f);
            from.z = to.z = realKey.position.z;
            Sfx.Play(this, "pop", 0.9f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.6f)
            {
                realKey.position = Vector3.Lerp(from, to, k) + Vector3.up * Mathf.Sin(k * Mathf.PI) * 1.4f;
                realKey.localRotation = Quaternion.Euler(0, 0, k * 540f);
                yield return null;
            }
            realKey.localRotation = Quaternion.Euler(0, 0, 90f);
            Sfx.Play(this, "clunk", 0.9f);
            Haptics.Light();
            toni.LookTarget = realKey;
            toni.Alert(true);
            yield return new WaitForSeconds(0.5f);
            toni.Alert(false);
            // la chiave apre il lucchetto del frigo
            yield return StartCoroutine(S41LKit.Move(realKey, realKey.parent.InverseTransformPoint(padlock.position + Vector3.right * 0.2f), 0.4f, 0.3f));
            Sfx.Play(this, "ding", 0.9f);
            Vector3 pp = padlock.localPosition;
            yield return StartCoroutine(S41LKit.Move(padlock, pp + new Vector3(-0.5f, -1.4f, 0f), 0.5f, 0.6f));
            realKey.gameObject.SetActive(false);
            toni.Say("FRIDGE TIME!", 1.6f, "woof");
            S41LKit.Confetti(confetti, true);
            yield return StartCoroutine(toni.Happy(1.8f));
            yield return new WaitForSeconds(0.6f);
            S41LKit.Confetti(confetti, false);
            Win();
        }
    }
}
