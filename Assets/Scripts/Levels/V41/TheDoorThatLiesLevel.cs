using System.Collections;
using UnityEngine;
using Gotcha.Components;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello "The door that lies" — tre porte sorridenti con le targhe SAFE, DANGER ed EXIT, ma le targhe mentono.
    /// Si trascinano le targhe da una porta all'altra e si guardano le reazioni: la vera uscita è quella che si
    /// terrorizza quando riceve la targa DANGER. Toccando una porta la si apre; due porte sbagliate = sconfitta.
    /// </summary>
    public class TheDoorThatLiesLevel : LevelController
    {
        [SerializeField] Transform[] doors, panels, plaques;
        [SerializeField] Collider2D[] doorAreas, plaqueAreas;
        [SerializeField] SpriteRenderer[] faces, sweats, doorways;
        [SerializeField] Sprite smileFace, scaredFace, smugFace, litDoorway, darkDoorway;
        [SerializeField] Transform glove, mouse;
        [SerializeField] TextMesh missText;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] SmokeEmitter[] confetti;
        [SerializeField] int missesAllowed = 2;

        const int Safe = 0, Danger = 1, Exit = 2;
        static readonly Vector3 PlaqueSpot = new Vector3(0f, 2.55f, 0f);

        Camera cam;
        int exitDoor, misses, dragPlaque = -1, dragFinger, pressDoor = -1, pressFinger;
        int[] plaqueOn;      // targa (0 SAFE, 1 DANGER, 2 EXIT) appesa a ciascuna porta
        bool busy;
        Coroutine facePop;
        float pressTime;
        Vector2 pressAt, dragOffset;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            exitDoor = Random.Range(0, doors.Length);
            // la vera uscita parte con SAFE; le altre due mentono con EXIT e DANGER
            plaqueOn = new int[doors.Length];
            int other = Random.Range(0, 2);
            for (int d = 0, k = 0; d < doors.Length; d++)
            {
                if (d == exitDoor) plaqueOn[d] = Safe;
                else plaqueOn[d] = (k++ == other) ? Exit : Danger;
            }
            for (int d = 0; d < doors.Length; d++)
            {
                plaques[plaqueOn[d]].position = doors[d].position + PlaqueSpot;
                doorways[d].gameObject.SetActive(false);
            }
            glove.gameObject.SetActive(false);
            mouse.gameObject.SetActive(false);
            S41HKit.Confetti(confetti, false);
            UpdateFaces(-1);
            UpdateMisses();
            S41HKit.Say(bubble, "Only one door is the real exit. The signs? They lie.");
        }

        void UpdateMisses() { if (missText != null) missText.text = "WRONG DOORS " + misses + "/" + missesAllowed; }

        void UpdateFaces(int changedDoor)
        {
            for (int d = 0; d < doors.Length; d++)
            {
                bool danger = plaqueOn[d] == Danger;
                faces[d].sprite = danger ? (d == exitDoor ? scaredFace : smugFace) : smileFace;
                sweats[d].gameObject.SetActive(danger && d == exitDoor);
            }
            if (changedDoor < 0) return;
            if (plaqueOn[changedDoor] == Danger)
            {
                if (changedDoor == exitDoor) { S41HKit.Say(bubble, "EEP! N-n-not DANGER! Anything but that!"); Haptics.Light(); }
                else S41HKit.Say(bubble, "Danger? Hehe. Fine by me.");
            }
            else S41HKit.Say(bubble, plaqueOn[changedDoor] == Exit ? "Ooh, EXIT! I'm totally the exit. Trust me." : "Safe and sound, that's me!");
            if (facePop != null) StopCoroutine(facePop);
            foreach (SpriteRenderer f in faces) f.transform.localScale = Vector3.one;
            facePop = StartCoroutine(S41HKit.Pop(faces[changedDoor].transform, 0.25f, 0.3f));
        }

        void Update()
        {
            if (IsFinished) return;
            // la porta terrorizzata trema
            for (int d = 0; d < doors.Length; d++)
                faces[d].transform.localRotation = Quaternion.Euler(0, 0, faces[d].sprite == scaredFace ? Mathf.Sin(Time.time * 50f) * 4f : 0f);
            if (busy) return;

            foreach (S41HKit.Finger f in S41HKit.Fingers(cam))
            {
                if (f.began && dragPlaque < 0 && pressDoor < 0)
                {
                    for (int p = 0; p < plaques.Length; p++)
                        if (plaqueAreas[p].OverlapPoint(f.world)) { dragPlaque = p; dragFinger = f.id; dragOffset = (Vector2)plaques[p].position - f.world; Sfx.Play(this, "tick", 0.5f); break; }
                    if (dragPlaque < 0)
                        for (int d = 0; d < doors.Length; d++)
                            if (doorAreas[d].OverlapPoint(f.world)) { pressDoor = d; pressFinger = f.id; pressTime = Time.time; pressAt = f.world; break; }
                }
                if (dragPlaque >= 0 && f.id == dragFinger)
                {
                    plaques[dragPlaque].position = (Vector3)(f.world + dragOffset);
                    if (f.ended) DropPlaque();
                }
                else if (pressDoor >= 0 && f.id == pressFinger && f.ended)
                {
                    int d = pressDoor;
                    pressDoor = -1;
                    if (Time.time - pressTime < 0.5f && Vector2.Distance(pressAt, f.world) < 0.4f && doorAreas[d].OverlapPoint(f.world)) StartCoroutine(Open(d));
                }
            }
            if (S41HKit.Fingers(cam).Count == 0) { if (dragPlaque >= 0) DropPlaque(); pressDoor = -1; }
        }

        void DropPlaque()
        {
            int p = dragPlaque;
            dragPlaque = -1;
            int from = System.Array.IndexOf(plaqueOn, p);
            int best = -1;
            float bestD = 1.1f;
            for (int d = 0; d < doors.Length; d++)
            {
                float dist = Vector2.Distance(plaques[p].position, doors[d].position + PlaqueSpot);
                if (dist < bestD) { bestD = dist; best = d; }
            }
            if (best >= 0 && best != from)
            {
                // scambio di targhe
                int other = plaqueOn[best];
                plaqueOn[best] = p;
                plaqueOn[from] = other;
                StartCoroutine(S41HKit.Move(plaques[other], plaques[other].parent.InverseTransformPoint(doors[from].position + PlaqueSpot), 0.25f, 0.3f));
                plaques[p].position = doors[best].position + PlaqueSpot;
                Sfx.Play(this, "clunk", 0.6f);
                UpdateFaces(best);
            }
            else StartCoroutine(S41HKit.Move(plaques[p], plaques[p].parent.InverseTransformPoint(doors[from].position + PlaqueSpot), 0.2f));
        }

        IEnumerator Swing(Transform panel, float from, float to)
        {
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.3f)
            {
                panel.localScale = new Vector3(Mathf.Lerp(from, to, Mathf.SmoothStep(0f, 1f, k)), 1f, 1f);
                yield return null;
            }
            panel.localScale = new Vector3(to, 1f, 1f);
        }

        IEnumerator Open(int d)
        {
            busy = true;
            bool real = d == exitDoor;
            doorways[d].sprite = real ? litDoorway : darkDoorway;
            doorways[d].gameObject.SetActive(true);
            faces[d].gameObject.SetActive(false);
            Sfx.Play(this, "whoosh", 0.6f);
            yield return StartCoroutine(Swing(panels[d], 1f, 0.12f));
            if (real)
            {
                Sfx.Play(this, "ding", 0.8f);
                S41HKit.Say(bubble, "The scared one was honest! FREEDOM!");
                mouse.gameObject.SetActive(true);
                Vector3 target = mouse.parent.InverseTransformPoint(doors[d].position + new Vector3(0f, 0.2f, 0f));
                yield return StartCoroutine(S41HKit.Move(mouse, target, 1.1f, 0.4f));
                mouse.gameObject.SetActive(false);
                S41HKit.Confetti(confetti, true);
                yield return new WaitForSeconds(2f);
                S41HKit.Confetti(confetti, false);
                Win();
                yield break;
            }
            // trappola: guantone a molla
            glove.position = doors[d].position + new Vector3(0f, 1.2f, 0f);
            glove.localScale = Vector3.one * 0.2f;
            glove.gameObject.SetActive(true);
            for (float k = 0f; k < 1f; k += Time.deltaTime / 0.15f) { glove.localScale = Vector3.one * Mathf.Lerp(0.2f, 1.3f, k); yield return null; }
            Sfx.Play(this, "bonk", 0.9f);
            Haptics.Medium();
            misses++;
            UpdateMisses();
            S41HKit.Say(bubble, "BOING! That door was lying!");
            yield return new WaitForSeconds(0.9f);
            glove.gameObject.SetActive(false);
            yield return StartCoroutine(Swing(panels[d], 0.12f, 1f));
            doorways[d].gameObject.SetActive(false);
            faces[d].gameObject.SetActive(true);
            if (misses >= missesAllowed)
            {
                S41HKit.Say(bubble, "Hehehe! Fooled you twice!");
                yield return new WaitForSeconds(1.2f);
                Fail();
                yield break;
            }
            S41HKit.Say(bubble, "Swap the signs and watch their faces...");
            busy = false;
        }
    }
}
