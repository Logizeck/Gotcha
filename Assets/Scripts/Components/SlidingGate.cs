using UnityEngine;

namespace Gotcha.Components
{
    /// <summary>
    /// Cancello che si apre e si chiude da solo (labirinti con ostacoli mobili): scivola tra la posizione chiusa e
    /// quella aperta (di solito sopra un muro vicino), con una pausa a ogni estremo. È un corpo cinematico, quindi
    /// spinge la sfera invece di attraversarla.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class SlidingGate : MonoBehaviour
    {
        public Vector2 openOffset = new Vector2(0f, -1f);   // spostamento locale da chiuso ad aperto
        public float holdClosed = 1.4f, holdOpen = 1.6f, move = 0.45f;
        public float phase;                                // secondi di sfasamento tra cancelli diversi

        Rigidbody2D rb;
        Vector2 closed;
        float clock;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            closed = transform.localPosition;
            clock = phase;
        }

        void FixedUpdate()
        {
            clock += Time.fixedDeltaTime;
            float cycle = holdClosed + move + holdOpen + move;
            float t = Mathf.Repeat(clock, cycle), k;
            if (t < holdClosed) k = 0f;
            else if (t < holdClosed + move) k = Mathf.SmoothStep(0f, 1f, (t - holdClosed) / move);
            else if (t < holdClosed + move + holdOpen) k = 1f;
            else k = Mathf.SmoothStep(1f, 0f, (t - holdClosed - move - holdOpen) / move);
            Vector2 local = closed + openOffset * k;
            Vector2 world = transform.parent != null ? (Vector2)transform.parent.TransformPoint(local) : local;
            rb.MovePosition(world);
        }
    }
}
