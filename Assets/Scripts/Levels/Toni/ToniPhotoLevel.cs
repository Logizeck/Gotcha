using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 50 — "SAY CHEESE!". Serve una foto di Toni che guarda nell'obiettivo, ma Toni guarda SOLO i
    /// biscottini (o il tuo dito). Il trucco dei fotografi di cani: appoggiare il biscotto sopra la macchina
    /// fotografica, così guarda dritto nell'obiettivo. Se il biscotto finisce vicino alla bocca, se lo mangia.
    /// Ogni tanto sbatte le palpebre. 4 pose di pellicola: finite quelle, si perde.
    /// Easter egg: il biscotto in equilibrio sulla testa di Toni → foto di "Toni unicorno".
    /// </summary>
    public class ToniPhotoLevel : LevelController
    {
        [SerializeField] ToniPug toni;
        [SerializeField] Draggable2D bone;
        [SerializeField] Transform jar;
        [SerializeField] Transform lens;
        [SerializeField] TapButton shutter;
        [SerializeField] SpriteRenderer flash;
        [SerializeField] Transform polaroid;
        [SerializeField] Transform photoSlot;
        [SerializeField] TextMesh caption;
        [SerializeField] TextMesh filmLabel;
        [SerializeField] int film = 4;
        [SerializeField] float floorY = -3.3f;
        [SerializeField] float eatRadius = 0.75f;
        [SerializeField] float headTopRadius = 0.5f;
        [SerializeField] float cameraRadius = 0.9f;
        [SerializeField] float lookTolerance = 15f;

        enum Spot { Jar, Floor, Camera, Head, Air }

        Spot spot;
        bool busy, blinking;
        float nextBlink;
        Vector3 polaroidHome;
        GameObject copy;

        Vector3 HeadTop => toni.transform.TransformPoint(new Vector2(-0.72f, 0.95f));
        Vector3 CameraTop => lens.position + new Vector3(0.55f, 0.75f, 0f);

        public override void OnLevelStart()
        {
            toni.WakeUp(2);
            toni.LookTarget = bone.transform;
            toni.Wag = 0.4f;
            bone.Dropped += OnBoneDropped;
            bone.DragStarted += OnBoneTaken;
            shutter.Pressed += OnShutter;
            polaroidHome = polaroid.localPosition;
            polaroid.gameObject.SetActive(false);
            SetFlash(0f);
            PutInJar();
            UpdateFilm();
            nextBlink = Time.time + Random.Range(2f, 3.5f);
        }

        void OnDestroy()
        {
            if (bone != null) { bone.Dropped -= OnBoneDropped; bone.DragStarted -= OnBoneTaken; }
            if (shutter != null) shutter.Pressed -= OnShutter;
        }

        void Update()
        {
            if (IsFinished) return;
            if (!blinking && Time.time >= nextBlink && !busy) StartCoroutine(Blink());
            // con il biscotto in testa Toni guarda in su, strabico
            toni.LookPoint = spot == Spot.Head ? HeadTop + Vector3.up * 2f : (Vector3?)null;
            toni.LookTarget = spot == Spot.Head ? null : bone.transform;
        }

        IEnumerator Blink()
        {
            blinking = true;
            toni.Eyes(0);
            yield return new WaitForSeconds(0.16f);
            toni.Eyes(2);
            blinking = false;
            nextBlink = Time.time + Random.Range(2f, 4f);
        }

        void PutInJar()
        {
            spot = Spot.Jar;
            bone.gameObject.SetActive(true);
            bone.enabled = true;
            bone.transform.position = new Vector3(jar.position.x, jar.position.y + 0.55f, bone.transform.position.z);
            bone.transform.rotation = Quaternion.Euler(0, 0, 15f);
        }

        void OnBoneTaken(Draggable2D d) { spot = Spot.Air; }

        void OnBoneDropped(Draggable2D d, DropSlot slot)
        {
            if (IsFinished) return;
            Vector3 p = bone.transform.position;
            if (Vector2.Distance(p, HeadTop) < headTopRadius)
            {
                spot = Spot.Head;   // in equilibrio sulla testa
                bone.transform.position = new Vector3(HeadTop.x, HeadTop.y, p.z);
                bone.transform.rotation = Quaternion.identity;
                toni.Say("...?", 1f);
            }
            else if (Vector2.Distance(p, toni.MouthWorld) < eatRadius) StartCoroutine(EatIt());
            else if (Vector2.Distance(p, CameraTop) < cameraRadius)
            {
                spot = Spot.Camera;   // appoggiato sopra la macchina fotografica
                bone.transform.position = new Vector3(CameraTop.x, CameraTop.y, p.z);
                bone.transform.rotation = Quaternion.identity;
                Sfx.Play(this, "tick", 0.6f);
            }
            else StartCoroutine(FallToFloor());
        }

        IEnumerator EatIt()
        {
            busy = true;
            bone.enabled = false;
            yield return toni.Chomp(bone.transform);
            toni.Say("More?", 1.2f, "woof");
            yield return new WaitForSeconds(0.8f);
            PutInJar();
            busy = false;
        }

        IEnumerator FallToFloor()
        {
            spot = Spot.Floor;
            Transform t = bone.transform;
            float target = floorY + Random.Range(-0.15f, 0.15f), v = 0f;
            while (t.position.y > target)
            {
                if (Draggable2D.Active == bone) yield break;
                v += 18f * Time.deltaTime;
                t.position += Vector3.down * (v * Time.deltaTime);
                yield return null;
            }
            Sfx.Play(this, "tick", 0.5f);
        }

        void OnShutter(TapButton b)
        {
            if (IsFinished || busy || film <= 0 || Draggable2D.Active == bone) return;
            StartCoroutine(TakePhoto());
        }

        bool LooksAtCamera()
        {
            if (spot != Spot.Camera || !bone.gameObject.activeSelf) return false;
            Vector2 eye = toni.HeadWorld;
            return Vector2.Angle((Vector2)bone.transform.position - eye, (Vector2)lens.position - eye) < lookTolerance;
        }

        IEnumerator TakePhoto()
        {
            busy = true;
            film--;
            UpdateFilm();
            bool closed = blinking;
            bool perfect = !closed && LooksAtCamera();
            bool unicorn = !closed && spot == Spot.Head;
            string text = closed ? "EYES CLOSED!" : perfect ? "PERFECT!" : unicorn ? "UNICORN TONI!"
                : spot == Spot.Jar ? "LOOKING AT THE JAR..." : spot == Spot.Floor ? "LOOKING AT THE FLOOR..." : "LOOKING AWAY...";

            Sfx.Play(this, "pop", 0.9f);
            Haptics.Light();
            // lo scatto: si "congela" Toni nella polaroid
            if (copy != null) Destroy(copy);
            copy = Instantiate(toni.gameObject, photoSlot);
            copy.GetComponent<ToniPug>().enabled = false;
            copy.transform.localPosition = Vector3.zero;
            copy.transform.localRotation = Quaternion.identity;
            copy.transform.localScale = new Vector3(Mathf.Sign(toni.transform.localScale.x) * 0.42f, 0.42f, 1f);
            foreach (SpriteRenderer sr in copy.GetComponentsInChildren<SpriteRenderer>(true)) sr.sortingOrder += 80;
            if (unicorn)
            {
                GameObject b = Instantiate(bone.gameObject, copy.transform);
                b.GetComponent<Draggable2D>().enabled = false;
                b.transform.localPosition = new Vector3(-0.72f, 0.95f, 0f);   // coordinate di Toni: in cima alla testa
                b.transform.localRotation = Quaternion.identity;
                b.transform.localScale = Vector3.one * (bone.transform.lossyScale.y / Mathf.Abs(toni.transform.lossyScale.y));
                foreach (SpriteRenderer sr in b.GetComponentsInChildren<SpriteRenderer>(true)) sr.sortingOrder += 80;
            }
            for (float k = 1f; k > 0f; k -= Time.deltaTime / 0.35f) { SetFlash(k); yield return null; }
            SetFlash(0f);

            caption.text = text;
            polaroid.gameObject.SetActive(true);
            Vector3 from = polaroidHome + Vector3.down * 6f;
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.4f)
            {
                polaroid.localPosition = Vector3.Lerp(from, polaroidHome, 1f - (1f - k) * (1f - k));
                polaroid.localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(20f, -5f, k));
                yield return null;
            }
            polaroid.localPosition = polaroidHome;

            if (perfect || unicorn)
            {
                Sfx.Play(this, "ding", 0.8f);
                StartCoroutine(toni.Happy(1.4f));
                yield return new WaitForSeconds(1.8f);
                if (unicorn) WinWithEgg("toni_unicorn");
                else Win();
                yield break;
            }
            Sfx.Play(this, "buzz", 0.5f);
            yield return new WaitForSeconds(1.6f);
            if (film <= 0)
            {
                caption.text = "OUT OF FILM!";
                yield return new WaitForSeconds(1f);
                Fail();
                yield break;
            }
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.3f)
            {
                polaroid.localPosition = Vector3.Lerp(polaroidHome, from, k * k);
                yield return null;
            }
            polaroid.gameObject.SetActive(false);
            busy = false;
        }

        void UpdateFilm() { if (filmLabel != null) filmLabel.text = "FILM: " + film; }

        void SetFlash(float a)
        {
            if (flash == null) return;
            Color c = flash.color;
            c.a = a;
            flash.color = c;
            flash.enabled = a > 0.001f;
        }
    }
}
