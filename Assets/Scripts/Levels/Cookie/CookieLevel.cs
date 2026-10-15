using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 14 — il biscotto nel barattolo. Il topolino ha fame ma il biscotto è dentro un barattolo aperto solo in
    /// alto, sulla mensola. La gravità segue il telefono: bisogna capovolgerlo perché il biscotto esca dal barattolo,
    /// poi inclinarlo e raddrizzarlo per farlo cadere dal topolino. (Nell'Editor: frecce.)
    /// </summary>
    public class CookieLevel : LevelController
    {
        [SerializeField] Rigidbody2D cookie;
        [SerializeField] Transform mouse;
        [SerializeField] SmokeEmitter hearts;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] float eatDistance = 0.65f;

        bool done;

        public override void OnLevelStart()
        {
            if (bubble != null) { bubble.Show("Cookie..."); StartCoroutine(HideLater(2f)); }
        }

        void Update()
        {
            if (done || IsFinished || cookie == null) return;
            if (Vector2.Distance(cookie.position, (Vector2)mouse.position + new Vector2(0f, 0.2f)) < eatDistance)
            {
                done = true;
                StartCoroutine(Eat());
            }
        }

        IEnumerator Eat()
        {
            cookie.simulated = false;
            Transform t = cookie.transform;
            Vector3 from = t.position, to = mouse.position + new Vector3(-0.1f, 0.25f, 0f);
            Vector3 scale = t.localScale;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.5f)
            {
                t.position = Vector3.Lerp(from, to, k);
                t.localScale = scale * (1f - k);
                yield return null;
            }
            t.gameObject.SetActive(false);
            if (hearts != null) hearts.Play();
            if (bubble != null) bubble.Show("Yum!");
            yield return new WaitForSeconds(1.3f);
            if (hearts != null) hearts.Stop();
            Win();
        }

        IEnumerator HideLater(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (bubble != null && !done) bubble.Hide();
        }
    }
}
