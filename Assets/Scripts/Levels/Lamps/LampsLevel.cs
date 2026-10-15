using System.Collections.Generic;
using UnityEngine;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 13 — le due lampade. Sul muro c'è una scritta: "YOU" in rosso e "WON" in blu. Il pulsante rosso accende
    /// la lampada rossa, il blu quella blu (finché sono premuti). Sotto la luce rossa la scritta rossa sparisce e si legge
    /// solo "WON"; sotto la luce blu si legge solo "YOU". Accendendole entrambe insieme (due dita) si legge "YOU WON".
    /// Nell'Editor: tasti R e B (oltre al mouse) per le due lampade.
    /// </summary>
    public class LampsLevel : LevelController
    {
        [SerializeField] Collider2D redButton;
        [SerializeField] Collider2D blueButton;
        [SerializeField] Transform redVisual;
        [SerializeField] Transform blueVisual;
        [SerializeField] SpriteRenderer wall;
        [SerializeField] TextMesh you;
        [SerializeField] TextMesh won;
        [SerializeField] GameObject redLight;
        [SerializeField] GameObject blueLight;
        [SerializeField] float holdToWin = 0.8f;

        static readonly Color WallOff = new Color(0.13f, 0.13f, 0.17f);
        static readonly Color WallRed = new Color(0.86f, 0.32f, 0.32f);
        static readonly Color WallBlue = new Color(0.32f, 0.42f, 0.9f);
        static readonly Color WallBoth = new Color(0.88f, 0.8f, 0.96f);
        static readonly Color InkRed = new Color(0.86f, 0.12f, 0.16f);
        static readonly Color InkBlue = new Color(0.12f, 0.26f, 0.9f);
        static readonly Color InkDark = new Color(0.1f, 0.05f, 0.1f);

        Camera cam;
        Vector3 redScale, blueScale;
        float bothFor;
        readonly List<Vector2> touches = new List<Vector2>();

        public override void OnLevelStart()
        {
            cam = Camera.main;
            if (redVisual != null) redScale = redVisual.localScale;
            if (blueVisual != null) blueScale = blueVisual.localScale;
            Apply(false, false);
        }

        void Update()
        {
            if (IsFinished) return;
            touches.Clear();
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch t = Input.GetTouch(i);
                if (t.phase != TouchPhase.Ended && t.phase != TouchPhase.Canceled) touches.Add(PointerInput.ToWorld(cam, t.position));
            }
            if (Input.touchCount == 0 && Input.GetMouseButton(0) && !PointerInput.IsOverUi()) touches.Add(PointerInput.ToWorld(cam, Input.mousePosition));

            bool red = Pressed(redButton), blue = Pressed(blueButton);
#if UNITY_EDITOR
            red |= Input.GetKey(KeyCode.R);
            blue |= Input.GetKey(KeyCode.B);
#endif
            Squash(redVisual, redScale, red);
            Squash(blueVisual, blueScale, blue);
            Apply(red, blue);

            bothFor = red && blue ? bothFor + Time.deltaTime : 0f;
            if (bothFor >= holdToWin) Win();
        }

        /// <summary>Colori di muro e scritte a seconda delle luci accese (inchiostro dello stesso colore della luce = invisibile).</summary>
        void Apply(bool red, bool blue)
        {
            if (redLight != null) redLight.SetActive(red);
            if (blueLight != null) blueLight.SetActive(blue);
            Color w, y, n;
            if (red && blue) { w = WallBoth; y = InkRed; n = InkBlue; }
            else if (red) { w = WallRed; y = WallRed; n = InkDark; }      // "YOU" rosso sparisce, "WON" si legge
            else if (blue) { w = WallBlue; y = InkDark; n = WallBlue; }   // "WON" blu sparisce, "YOU" si legge
            else { w = WallOff; y = WallOff; n = WallOff; }   // al buio la scritta non si vede affatto
            if (wall != null) wall.color = w;
            if (you != null) you.color = y;
            if (won != null) won.color = n;
        }

        bool Pressed(Collider2D button)
        {
            if (button == null) return false;
            foreach (Vector2 p in touches) if (button.OverlapPoint(p)) return true;
            return false;
        }

        static void Squash(Transform t, Vector3 baseScale, bool pressed)
        {
            if (t == null) return;
            Vector3 target = pressed ? new Vector3(baseScale.x, baseScale.y * 0.8f, baseScale.z) : baseScale;
            t.localScale = Vector3.Lerp(t.localScale, target, 25f * Time.deltaTime);
        }
    }
}
