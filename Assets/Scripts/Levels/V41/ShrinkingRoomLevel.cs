using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "The shrinking room" — il topo è in una stanza le cui pareti si stringono (in 20 secondi lo
    /// schiacciano). Le pareti sono spinte dai bordi dello schermo: la stanza è più grande del display. Trucco: si
    /// rimpicciolisce TUTTA la stanza col pizzico a due dita; quando ci sta dentro lo schermo le pareti tornano al loro
    /// posto (si allontanano, in coordinate della stanza), compare l'uscita e il topo scappa.
    /// Nell'Editor: rotellina del mouse o Z/X per il pizzico.
    /// </summary>
    public class ShrinkingRoomLevel : LevelController
    {
        [SerializeField] Transform room;             // contenitore scalato (pavimento, pareti, topo, uscita)
        [SerializeField] Transform wallLeft, wallRight, wallTop, wallBottom;
        [SerializeField] Transform exitDoor;          // figlio della parete destra
        [SerializeField] Transform mouse;
        [SerializeField] TextMesh goal;
        [SerializeField] SpeechBubble bubble;

        const float Seconds = 20f, MinScale = 0.5f, SolveScale = 0.6f, Mouse = 0.4f;
        static readonly Vector2 ScreenHalf = new Vector2(2.45f, 3.2f);     // mezza area visibile per la stanza
        static readonly Vector2 Natural = new Vector2(4.2f, 5.8f);     // mezza misura vera della stanza

        Camera cam;
        float scale = 1f, t, pinch0, scale0, bubbleUntil;
        bool pinching, busy, warned;
        Vector2 half;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            if (exitDoor != null) exitDoor.gameObject.SetActive(false);
            Kit41C.Goal(goal, "DON'T LET HIM GET SQUASHED");
            Say("HELP! The walls are moving!", 2.2f);
            Apply();
        }

        void Say(string text, float seconds)
        {
            if (bubble == null) return;
            bubble.Show(text);
            bubbleUntil = Time.time + seconds;
        }

        void Update()
        {
            if (IsFinished) return;
            if (bubble != null && bubbleUntil > 0f && Time.time > bubbleUntil) { bubble.Hide(); bubbleUntil = 0f; }
            if (busy) return;
            t += Time.deltaTime;
            ReadPinch();
            Apply();
            if (!warned && t > 10f) { warned = true; Say("It's getting cosy in here!", 1.8f); }
            if (mouse != null)
            {
                float panic = Mathf.Clamp01(t / Seconds);
                mouse.localPosition = new Vector3(Mathf.Sin(Time.time * 40f) * 0.03f * panic, 0f, 0f);
            }
            if (scale <= SolveScale + 0.001f) { StartCoroutine(Escape()); return; }
            if (half.x < Mouse + 0.05f || half.y < Mouse + 0.05f) StartCoroutine(Squash());
        }

        void ReadPinch()
        {
            if (Input.touchCount >= 2)
            {
                float d = Vector2.Distance(Input.GetTouch(0).position, Input.GetTouch(1).position);
                if (!pinching || Input.GetTouch(1).phase == TouchPhase.Began) { pinching = true; pinch0 = Mathf.Max(d, 1f); scale0 = scale; }
                scale = Mathf.Clamp(scale0 * d / pinch0, MinScale, 1f);
            }
            else pinching = false;
#if UNITY_EDITOR
            float wheel = Input.mouseScrollDelta.y;
            if (Mathf.Abs(wheel) > 0.01f) scale = Mathf.Clamp(scale + wheel * 0.05f, MinScale, 1f);
            if (Input.GetKey(KeyCode.Z)) scale = Mathf.Clamp(scale - Time.deltaTime * 0.5f, MinScale, 1f);
            if (Input.GetKey(KeyCode.X)) scale = Mathf.Clamp(scale + Time.deltaTime * 0.5f, MinScale, 1f);
#endif
        }

        /// <summary>Le pareti (coordinate della stanza) sono spinte dai bordi dello schermo e avanzano col tempo.</summary>
        void Apply()
        {
            float p = Mathf.Clamp01(t / Seconds) * 0.85f;
            Vector2 limit = new Vector2(Mathf.Min(Natural.x, ScreenHalf.x / scale), Mathf.Min(Natural.y, ScreenHalf.y / scale));
            half = limit * (1f - p);
            Place(half);
        }

        void Place(Vector2 h)
        {
            room.localScale = Vector3.one * scale;
            wallLeft.localPosition = new Vector3(-h.x - 0.25f, 0f, 0f);
            wallRight.localPosition = new Vector3(h.x + 0.25f, 0f, 0f);
            wallTop.localPosition = new Vector3(0f, h.y + 0.25f, 0f);
            wallBottom.localPosition = new Vector3(0f, -h.y - 0.25f, 0f);
        }

        IEnumerator Escape()
        {
            busy = true;
            Kit41C.Goal(goal, "THE ROOM FITS THE SCREEN!");
            Sfx.Play(this, "whoosh", 0.7f);
            Vector2 from = half;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.8f)
            {
                half = Vector2.Lerp(from, Natural, Mathf.SmoothStep(0f, 1f, k));
                Place(half);
                yield return null;
            }
            Place(Natural);
            if (exitDoor != null)
            {
                exitDoor.gameObject.SetActive(true);
                yield return Kit41C.Pop(exitDoor, 0.3f);
            }
            Sfx.Play(this, "ding", 0.7f);
            Say("Phew! Room to breathe... and an EXIT!", 2f);
            yield return new WaitForSeconds(0.6f);
            // il topo corre verso l'uscita sulla parete destra
            if (mouse != null)
            {
                Vector3 to = new Vector3(Natural.x - 0.1f, 0f, 0f);
                yield return Kit41C.Move(mouse, to, 1.0f, 0.15f);
                Vector3 s = mouse.localScale;
                for (float k = 0f; k < 1f; k += Time.deltaTime / 0.3f) { mouse.localScale = s * (1f - k); yield return null; }
                mouse.gameObject.SetActive(false);
            }
            Sfx.Play(this, "pop", 0.6f);
            yield return new WaitForSeconds(0.6f);
            Win();
        }

        IEnumerator Squash()
        {
            busy = true;
            Kit41C.Goal(goal, "SQUISH!");
            Sfx.Play(this, "bonk", 1f);
            Haptics.Heavy();
            if (mouse != null)
            {
                Vector3 s = mouse.localScale;
                for (float k = 0f; k < 1f; k += Time.deltaTime / 0.25f)
                {
                    mouse.localScale = new Vector3(s.x * (1f - k * 0.7f), s.y * (1f + k * 0.2f), 1f);
                    yield return null;
                }
            }
            Say("Too... tight...", 1.6f);
            yield return new WaitForSeconds(1.2f);
            Fail();
        }
    }
}
