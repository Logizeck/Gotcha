using UnityEngine;
using Gotcha.Sensors;

namespace Gotcha.UI
{
    /// <summary>
    /// Logo animato della schermata del titolo: la "G!" al neon è un piccolo labirinto e le palline cromate ci
    /// rotolano dentro seguendo l'inclinazione del telefono (frecce nell'Editor). Visibile solo col titolo aperto.
    /// </summary>
    public class TitleLogo : MonoBehaviour
    {
        public GameObject content;
        public Rigidbody2D[] balls;
        public float strength = 9f;

        Vector3[] start;

        void Awake()
        {
            start = new Vector3[balls.Length];
            for (int i = 0; i < balls.Length; i++)
            {
                start[i] = balls[i].transform.localPosition;
                balls[i].gravityScale = 0f;
            }
        }

        void Update()
        {
            bool show = GameUI.TitleShown;
            if (content.activeSelf != show)
            {
                content.SetActive(show);
                if (show) for (int i = 0; i < balls.Length; i++) balls[i].transform.localPosition = start[i];   // se fosse scappata
            }
        }

        void FixedUpdate()
        {
            if (!content.activeSelf) return;
            Vector3 t = SensorHub.Instance != null ? SensorHub.Instance.Tilt : Vector3.zero;
            Vector2 force = new Vector2(t.x, t.y);
            if (force.sqrMagnitude < 0.0004f) force = Vector2.zero;
            foreach (Rigidbody2D b in balls) b.AddForce(Vector2.ClampMagnitude(force, 1f) * strength * b.mass);
        }
    }
}
