using UnityEngine;
using Gotcha.Sensors;

namespace Gotcha.Components
{
    /// <summary>Mentre è attivo, la gravità 2D segue l'inclinazione del telefono (frecce nell'Editor).</summary>
    public class TiltGravity : MonoBehaviour
    {
        public float strength = 20f;
        Vector2 saved;

        void OnEnable() { saved = Physics2D.gravity; }
        void OnDisable() { Physics2D.gravity = saved; }

        void Update()
        {
            Vector3 t = SensorHub.Instance != null ? SensorHub.Instance.Tilt : Vector3.zero;
            Physics2D.gravity = new Vector2(t.x, t.y) * strength;
        }
    }
}
