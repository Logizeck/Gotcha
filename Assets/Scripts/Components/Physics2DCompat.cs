using UnityEngine;

namespace Gotcha.Components
{
    /// <summary>
    /// Velocità e attrito dei Rigidbody2D con lo stesso codice in Unity 2022.3 e Unity 6: in Unity 6 "velocity" e
    /// "drag" si chiamano "linearVelocity" e "linearDamping" (i nomi vecchi sono obsoleti e l'aggiornamento automatico
    /// di Unity riscriverebbe i file sul PC, creando differenze con la repository).
    /// </summary>
    public static class Physics2DCompat
    {
        public static Vector2 GetVelocity(this Rigidbody2D rb)
        {
#if UNITY_6000_0_OR_NEWER
            return rb.linearVelocity;
#else
            return rb.velocity;
#endif
        }

        public static void SetVelocity(this Rigidbody2D rb, Vector2 v)
        {
#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = v;
#else
            rb.velocity = v;
#endif
        }

        public static void SetLinearDrag(this Rigidbody2D rb, float drag)
        {
#if UNITY_6000_0_OR_NEWER
            rb.linearDamping = drag;
#else
            rb.drag = drag;
#endif
        }
    }
}
