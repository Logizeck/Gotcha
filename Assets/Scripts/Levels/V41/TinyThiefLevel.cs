using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "Tiny thief" — la corona è sparita dal cuscino: l'ha presa un ladro minuscolo, un puntino quasi
    /// invisibile tra i mobili, che scappa verso una delle due uscite (la tana nel muro o la finestra, a caso). Soluzione:
    /// ingrandire la stanza col pizzico (e spostarla con un dito) per trovarlo e vedere dove va, poi trascinare il cesto
    /// dal vassoio sul suo percorso, vicino all'uscita: ci passa sotto e resta intrappolato. Toccarlo non basta (scatta
    /// avanti ridendo), e anche il cesto calato proprio addosso a lui lo fa scappare. Sconfitta: dopo 25 s esce.
    /// (Editor: rotella del mouse per lo zoom.)
    /// </summary>
    public class TinyThiefLevel : LevelController
    {
        [SerializeField] Transform room;                 // contenitore della stanza (si ingrandisce)
        [SerializeField] Transform thief;
        [SerializeField] SpriteRenderer thiefArt;
        [SerializeField] SpriteRenderer glint;           // luccichio della corona rubata
        [SerializeField] Transform[] routeHole, routeWindow;
        [SerializeField] Transform basket;
        [SerializeField] SpriteRenderer crownOnCushion;
        [SerializeField] Transform timeBar;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] float escapeSeconds = 25f, maxZoom = 4.5f, trapRadius = 0.4f;   // raggio della trappola in unità schermo al momento del lancio
        [SerializeField] Vector2 frameMin = new Vector2(-2.9f, -5f), frameMax = new Vector2(2.9f, 3f);

        Camera cam;
        Transform[] route;
        float[] cum;
        float length, travelled, speed, clock;
        float zoom = 1f, pinchStart, zoomStart;
        Vector2 pinchLocal, lastPan;
        bool pinching, panning, dragBasket, basketPlaced, busy;
        Vector3 basketHome, basketScale;
        float trapLocal;
        Quaternion basketHomeRot;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            route = Random.value < 0.5f ? routeHole : routeWindow;
            cum = new float[route.Length];
            for (int i = 1; i < route.Length; i++) cum[i] = cum[i - 1] + Vector2.Distance(route[i - 1].localPosition, route[i].localPosition);
            length = cum[route.Length - 1];
            speed = length / escapeSeconds;
            thief.localPosition = route[0].localPosition;
            basketHome = basket.position;
            basketHomeRot = basket.rotation;
            basketScale = basket.localScale;
            if (crownOnCushion != null) crownOnCushion.enabled = false;
            if (bubble != null) bubble.Hide();
            StartCoroutine(Intro());
        }

        IEnumerator Intro()
        {
            yield return new WaitForSeconds(0.6f);
            if (!IsFinished && !busy) Kit41B.Say(bubble, "My CROWN! Who took it?!");
            yield return new WaitForSeconds(2.2f);
            if (!IsFinished && !busy && bubble != null) bubble.Hide();
        }

        Vector2 PathPoint(float d)
        {
            d = Mathf.Clamp(d, 0f, length);
            for (int i = 1; i < route.Length; i++)
                if (d <= cum[i]) return Vector2.Lerp(route[i - 1].localPosition, route[i].localPosition, Mathf.InverseLerp(cum[i - 1], cum[i], d));
            return route[route.Length - 1].localPosition;
        }

        void Update()
        {
            if (IsFinished || busy) return;
            float dt = Time.deltaTime;
            clock += dt;
            HandleZoom();
            HandlePointer();

            // il ladro corre
            Vector2 before = thief.localPosition;
            travelled += speed * dt;
            Vector2 now = PathPoint(travelled);
            thief.localPosition = new Vector3(now.x, now.y + Mathf.Abs(Mathf.Sin(clock * 14f)) * 0.012f, thief.localPosition.z);
            if (Mathf.Abs(now.x - before.x) > 0.0001f && thiefArt != null) thiefArt.flipX = now.x < before.x;
            if (glint != null) glint.color = new Color(1f, 0.95f, 0.6f, Mathf.Max(0f, Mathf.Sin(clock * 3.1f)) * 0.9f);
            if (timeBar != null) timeBar.localScale = new Vector3(Mathf.Clamp01(1f - travelled / length), 1f, 1f);

            if (basketPlaced && Vector2.Distance(basket.localPosition, thief.localPosition) < trapLocal) { StartCoroutine(Caught()); return; }
            if (travelled >= length) StartCoroutine(Escaped());
        }

        void ApplyRoom(Vector2 p)
        {
            float s = zoom;
            p.x = Mathf.Clamp(p.x, frameMax.x * (1f - s), frameMin.x * (1f - s));
            p.y = Mathf.Clamp(p.y, frameMax.y * (1f - s), frameMin.y * (1f - s));
            room.localScale = Vector3.one * s;
            room.localPosition = new Vector3(p.x, p.y, room.localPosition.z);
        }

        Vector2 ToRoom(Vector2 world) => ((Vector2)world - (Vector2)room.position) / zoom;

        void HandleZoom()
        {
            if (Input.touchCount >= 2 && !dragBasket)
            {
                Touch a = Input.GetTouch(0), b = Input.GetTouch(1);
                float d = Vector2.Distance(a.position, b.position);
                Vector2 mid = PointerInput.ToWorld(cam, (a.position + b.position) * 0.5f);
                if (!pinching) { pinching = true; panning = false; pinchStart = Mathf.Max(d, 1f); zoomStart = zoom; pinchLocal = ToRoom(mid); return; }
                zoom = Mathf.Clamp(zoomStart * d / pinchStart, 1f, maxZoom);
                ApplyRoom(mid - pinchLocal * zoom);
                return;
            }
            if (pinching && Input.touchCount == 0) pinching = false;
#if UNITY_EDITOR
            float wheel = Input.mouseScrollDelta.y;
            if (Mathf.Abs(wheel) > 0.01f)
            {
                Vector2 m = PointerInput.ToWorld(cam, Input.mousePosition);
                Vector2 local = ToRoom(m);
                zoom = Mathf.Clamp(zoom * (1f + wheel * 0.1f), 1f, maxZoom);
                ApplyRoom(m - local * zoom);
            }
#endif
        }

        void HandlePointer()
        {
            if (pinching) return;
            if (!Kit41B.Pointer(cam, out Vector2 w, out bool began, out bool ended))
            {
                if (dragBasket) DropBasket(basket.position);
                panning = false;
                return;
            }
            if (began)
            {
                if (Vector2.Distance(w, basket.position) < Mathf.Max(0.55f, basketPlaced ? trapLocal * zoom : 0f))
                {
                    dragBasket = true;
                    basketPlaced = false;
                    basket.SetParent(transform, true);
                    basket.localScale = basketScale;
                    basket.rotation = basketHomeRot;
                    Sfx.Play(this, "tick", 0.5f);
                    return;
                }
                Vector2 tw = thief.position;
                if (Vector2.Distance(w, tw) < Mathf.Max(0.3f, 0.12f * zoom)) { StartCoroutine(Dodge("Hee hee! Too slow!")); return; }
                panning = true;
                lastPan = w;
                return;
            }
            if (dragBasket)
            {
                basket.position = new Vector3(w.x, w.y, basket.position.z);
                if (ended) DropBasket(w);
                return;
            }
            if (panning)
            {
                ApplyRoom((Vector2)room.localPosition + (w - lastPan));
                lastPan = w;
                if (ended) panning = false;
            }
        }

        void DropBasket(Vector2 w)
        {
            dragBasket = false;
            if (Vector2.Distance(w, basketHome) < 0.7f || w.y > frameMax.y - 0.2f)
            {
                basket.position = basketHome;
                basket.rotation = basketHomeRot;
                return;
            }
            // capovolto sul pavimento: diventa parte della stanza (si ingrandisce con lei)
            basket.SetParent(room, true);
            basket.rotation = basketHomeRot * Quaternion.Euler(0f, 0f, 180f);
            basketPlaced = true;
            trapLocal = trapRadius / zoom;
            Sfx.Play(this, "clunk", 0.7f);
            Haptics.Light();
            StartCoroutine(Kit41B.Pop(basket, 0.15f, 0.2f));
            if (Vector2.Distance(basket.localPosition, thief.localPosition) < trapLocal)
                StartCoroutine(Dodge("Missed me!", trapLocal * 2.2f + 0.1f));
        }

        IEnumerator Dodge(string taunt, float jump = 0.35f)
        {
            Sfx.Play(this, "whoosh", 0.5f);
            Kit41B.Say(bubble, taunt);
            if (bubble != null) bubble.transform.position = new Vector3(Mathf.Clamp(thief.position.x, -1.6f, 1.6f), Mathf.Clamp(thief.position.y + 0.9f, -3.2f, 2.2f), bubble.transform.position.z);
            travelled = Mathf.Min(length - 0.05f, travelled + jump);
            yield return new WaitForSeconds(1.4f);
            if (!IsFinished && !busy && bubble != null) bubble.Hide();
        }

        IEnumerator Caught()
        {
            busy = true;
            Sfx.Play(this, "bonk", 0.8f);
            Haptics.Medium();
            thief.gameObject.SetActive(false);
            for (int i = 0; i < 4; i++) { yield return Kit41B.Shake(basket, 0.05f, 0.2f); Sfx.Play(this, "tick", 0.5f); }
            Kit41B.Say(bubble, "Gotcha, tiny!");
            if (bubble != null) bubble.transform.position = new Vector3(0f, 1.6f, bubble.transform.position.z);
            Vector2 p0 = room.localPosition;
            float z0 = zoom;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.8f)
            {
                zoom = Mathf.Lerp(z0, 1f, k);
                room.localScale = Vector3.one * zoom;
                room.localPosition = Vector2.Lerp(p0, Vector2.zero, k);
                yield return null;
            }
            zoom = 1f;
            room.localScale = Vector3.one;
            room.localPosition = Vector3.zero;
            if (crownOnCushion != null) { crownOnCushion.enabled = true; StartCoroutine(Kit41B.Pop(crownOnCushion.transform, 0.4f, 0.4f)); }
            Sfx.Play(this, "ding", 0.8f);
            yield return new WaitForSeconds(1.3f);
            Win();
        }

        IEnumerator Escaped()
        {
            busy = true;
            Sfx.Play(this, "whoosh", 0.8f);
            Vector3 s = thief.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.4f) { thief.localScale = s * (1f - k); yield return null; }
            thief.gameObject.SetActive(false);
            Kit41B.Say(bubble, "So long, giant!");
            yield return new WaitForSeconds(1.4f);
            Fail();
        }
    }
}
