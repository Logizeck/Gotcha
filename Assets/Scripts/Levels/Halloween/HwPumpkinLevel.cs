using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels.Halloween;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 246 "The perfect pumpkin" — si intaglia la zucca col dito, a mano libera (nessuna forma pronta): servono
    /// due occhi (in alto, uno a sinistra e uno a destra), un naso al centro e una bocca larga in basso (regioni
    /// logiche invisibili e tolleranti, vedi PumpkinFaceValidator). L'accendino accende solo la candela, la candela va
    /// dentro la zucca e, come ultima mossa, si spegne la luce della stanza: la zucca si illumina, prende vita e sorride.
    /// </summary>
    public class HwPumpkinLevel : LevelController
    {
        [SerializeField] Transform face;              // centro della faccia della zucca
        [SerializeField] Vector2 faceRadius = new Vector2(1f, 0.85f);
        [SerializeField] Transform pumpkin;
        [SerializeField] Material cutMaterial;
        [SerializeField] Color cutColor = new Color(0.36f, 0.16f, 0.05f);
        [SerializeField] Color cutInnerColor = new Color(0.62f, 0.3f, 0.08f);
        [SerializeField] Color glowCutColor = new Color(1f, 0.82f, 0.3f);
        [SerializeField] Transform knife;
        [SerializeField] Collider2D knifeArea;
        [SerializeField] Transform candle;
        [SerializeField] Collider2D candleArea;
        [SerializeField] SpriteRenderer candleArt;
        [SerializeField] Transform candleWick;
        [SerializeField] GameObject flame;
        [SerializeField] SpriteRenderer candleGlow;
        [SerializeField] Transform candleInsideSpot;
        [SerializeField] Transform lighter;
        [SerializeField] Collider2D lighterArea;
        [SerializeField] Transform lighterTip;
        [SerializeField] SmokeEmitter lighterSpark;
        [SerializeField] Collider2D lampArea;
        [SerializeField] Transform lamp;
        [SerializeField] SpriteRenderer lampLight;
        [SerializeField] Collider2D switchArea;
        [SerializeField] SpriteRenderer switchArt;
        [SerializeField] Sprite switchOn, switchOff;
        [SerializeField] SpriteRenderer darkness;
        [SerializeField] SpriteRenderer pumpkinGlow;
        [SerializeField] Collider2D clearArea;
        [SerializeField] Transform clearButton;
        [SerializeField] SmokeEmitter smoke;
        [SerializeField] SpeechBubble bubble;
        [SerializeField] TextMesh infoText;
        [SerializeField] Transform infoHolder;

        readonly PumpkinLogic logic = new PumpkinLogic();
        readonly List<GameObject> cuts = new List<GameObject>();
        readonly List<Vector3> stroke = new List<Vector3>();
        Camera cam;
        HwDragger drag;
        HwDragger.Item candleItem, lighterItem;
        HwSay say;
        HwToast toast;
        GameObject cutting;
        LineRenderer[] cuttingLines;
        Vector3 knifeHome;
        Quaternion knifeRot;
        int candleOrder;
        bool busy, glowing;

        const float DarkAlpha = 0.72f;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            logic.Reset();
            drag = new HwDragger(this);
            candleItem = drag.Add(candle, candleArea, 5);
            lighterItem = drag.Add(lighter, lighterArea, 6);
            say = new HwSay(bubble);
            toast = new HwToast(infoText, infoHolder);
            knifeHome = knife.position;
            knifeRot = knife.rotation;
            candleOrder = candleArt.sortingOrder;
            flame.SetActive(false);
            candleGlow.gameObject.SetActive(false);
            HalloweenKit.SetAlpha(darkness, 0f);
            HalloweenKit.SetAlpha(pumpkinGlow, 0f);
            switchArt.sprite = switchOn;
            if (lampLight != null) lampLight.enabled = true;
            if (smoke != null) smoke.Stop();
            if (lighterSpark != null) lighterSpark.Stop();
        }

        void Update()
        {
            say.Tick();
            toast.Tick();
            if (flame.activeSelf) flame.transform.localScale = Vector3.one * (1f + Mathf.PerlinNoise(Time.time * 7f, 0.3f) * 0.25f);
            if (IsFinished || busy || logic.InputLocked) return;

            if (!HalloweenKit.ReadPointer(cam, out Vector2 w, out bool began, out bool ended))
            {
                if (drag.Held != null) Drop(drag.Release());
                if (cutting != null) FinishCut();
                return;
            }
            if (began)
            {
                HwDragger.Item it = drag.TryPick(w);
                if (it != null) { if (it == candleItem) candleArt.sortingOrder = 40; }
                else if (InsideFace(w, 1f)) StartCut(w);
                else Tap(w);
            }
            if (drag.Held != null)
            {
                drag.MoveHeld(w);
                if (ended) Drop(drag.Release());
            }
            else if (cutting != null)
            {
                AddCutPoint(w);
                knife.position = new Vector3(w.x + 0.25f, w.y + 0.3f, knife.position.z);
                if (ended) FinishCut();
            }
        }

        Vector2 Normalized(Vector2 world)
        {
            Vector2 l = face.InverseTransformPoint(world);
            return new Vector2(l.x / faceRadius.x, l.y / faceRadius.y);
        }

        bool InsideFace(Vector2 world, float r) => Normalized(world).sqrMagnitude <= r * r;

        // ---------- intaglio ----------

        void StartCut(Vector2 w)
        {
            cutting = new GameObject("Cut");
            cutting.transform.SetParent(face, false);
            cuttingLines = new LineRenderer[2];
            for (int i = 0; i < 2; i++)
            {
                var go = new GameObject(i == 0 ? "Edge" : "Inside");
                go.transform.SetParent(cutting.transform, false);
                LineRenderer lr = go.AddComponent<LineRenderer>();
                lr.useWorldSpace = false;
                lr.widthMultiplier = i == 0 ? 0.13f : 0.07f;
                lr.numCapVertices = 5;
                lr.numCornerVertices = 3;
                lr.sharedMaterial = cutMaterial;
                lr.startColor = lr.endColor = i == 0 ? cutColor : cutInnerColor;
                lr.sortingOrder = 12 + i;
                lr.positionCount = 0;
                cuttingLines[i] = lr;
            }
            stroke.Clear();
            knife.rotation = Quaternion.Euler(0f, 0f, -35f);
            AddCutPoint(w);
            Sfx.Play(this, "tick", 0.3f);
        }

        void AddCutPoint(Vector2 w)
        {
            Vector2 n = Normalized(w);
            if (n.magnitude > 0.97f) n = n.normalized * 0.97f;   // si intaglia solo dentro la faccia
            Vector3 local = new Vector3(n.x * faceRadius.x, n.y * faceRadius.y, 0f);
            if (stroke.Count > 0 && (stroke[stroke.Count - 1] - local).sqrMagnitude < 0.0016f) return;
            stroke.Add(local);
            Vector3[] arr = stroke.ToArray();
            foreach (LineRenderer lr in cuttingLines) { lr.positionCount = arr.Length; lr.SetPositions(arr); }
        }

        void FinishCut()
        {
            GameObject cut = cutting;
            cutting = null;
            StartCoroutine(HalloweenKit.Move(knife, knifeHome, 0.25f));
            knife.rotation = knifeRot;
            var pts = new List<HwPoint>(stroke.Count);
            foreach (Vector3 p in stroke) pts.Add(new HwPoint(p.x / faceRadius.x, p.y / faceRadius.y));
            int before = logic.LastReport.PartsFound;
            if (!logic.TryCarve(pts, out bool evicted)) { Destroy(cut); return; }
            cuts.Add(cut);
            if (evicted && cuts.Count > 0) { Destroy(cuts[0]); cuts.RemoveAt(0); }
            Haptics.Light();
            Sfx.Play(this, "pop", 0.35f);
            if (logic.LastReport.PartsFound > before) StartCoroutine(HalloweenKit.Pop(pumpkin, 1.03f, 0.15f));
            if (logic.FaceComplete && before < 4) toast.Show("What a face!");
            RefreshGlow();
        }

        void ClearCarving()
        {
            if (!logic.TryClearCarving()) return;
            foreach (GameObject c in cuts) if (c != null) Destroy(c);
            cuts.Clear();
            Sfx.Play(this, "whoosh", 0.5f);
            StartCoroutine(HalloweenKit.Pop(clearButton, 1.15f, 0.2f));
            StartCoroutine(HalloweenKit.Wiggle(pumpkin, 4f, 0.4f));
            toast.Show("Fresh pumpkin!");
        }

        // ---------- tocchi ----------

        void Tap(Vector2 w)
        {
            if (switchArea.OverlapPoint(w)) { Switch(); return; }
            if (clearArea.OverlapPoint(w)) { ClearCarving(); return; }
            if (knifeArea.OverlapPoint(w))
            {
                StartCoroutine(HalloweenKit.Wiggle(knife, 8f, 0.35f));
                Sfx.Play(this, "tick", 0.5f);
                toast.Show("Carve the pumpkin with your finger!");
                return;
            }
            if (lampArea.OverlapPoint(w))
            {
                StartCoroutine(HalloweenKit.Wiggle(lamp, 6f, 0.8f, 8f));
                Sfx.Play(this, "tick", 0.4f);
            }
        }

        void Switch()
        {
            PumpkinSwitchResult r = logic.ToggleLight();
            if (r == PumpkinSwitchResult.Ignored) return;
            Sfx.Play(this, "clunk", 0.6f);
            Haptics.Light();
            switchArt.sprite = logic.LightOn ? switchOn : switchOff;
            if (lampLight != null) lampLight.enabled = logic.LightOn;
            if (r == PumpkinSwitchResult.Win) { StartCoroutine(Ending()); return; }
            StartCoroutine(HalloweenKit.Fade(darkness, logic.LightOn ? 0f : DarkAlpha, 0.25f));
            RefreshGlow();
            if (!logic.LightOn) toast.Show(logic.CandleLit ? "Spooky..." : "So dark!");
        }

        // ---------- oggetti ----------

        void Drop(HwDragger.Item it)
        {
            if (it == null) return;
            if (it == candleItem)
            {
                candleArt.sortingOrder = candleOrder;
                if (InsideFace(candle.position, 0.85f) && logic.TrySetCandleInside(true))
                {
                    candleItem.Enabled = false;
                    StartCoroutine(PutCandleInside());
                }
                else drag.SendHome(it);
                return;
            }
            if (it == lighterItem)
            {
                PumpkinTarget target = LighterTarget(lighterTip.position);
                if (lighterSpark != null) StartCoroutine(HalloweenKit.Burst(lighterSpark, 0.15f));
                Sfx.Play(this, "tick", 0.7f);
                if (logic.TryUseLighter(target))
                {
                    flame.SetActive(true);
                    Haptics.Medium();
                    Sfx.Play(this, "whoosh", 0.4f);
                    RefreshGlow();
                    toast.Show(logic.CandleInside ? "The candle is lit!" : "Fwoosh! A little flame.");
                }
                else if (target == PumpkinTarget.Candle) toast.Show("Already lit!");
                else if (target != PumpkinTarget.Nothing) toast.Show("*click* ...nothing happens.");
                drag.SendHome(it);
            }
        }

        PumpkinTarget LighterTarget(Vector2 tip)
        {
            if (logic.CandleInside ? InsideFace(tip, 1.05f) : Vector2.Distance(tip, candleWick.position) < 0.6f) return PumpkinTarget.Candle;
            if (InsideFace(tip, 1.1f)) return PumpkinTarget.Pumpkin;
            if (knifeArea.OverlapPoint(tip)) return PumpkinTarget.Knife;
            if (lampArea.OverlapPoint(tip)) return PumpkinTarget.Lamp;
            if (switchArea.OverlapPoint(tip)) return PumpkinTarget.Switch;
            return PumpkinTarget.Nothing;
        }

        IEnumerator PutCandleInside()
        {
            busy = true;
            yield return HalloweenKit.Move(candle, candleInsideSpot.position, 0.25f, 0.4f);
            // dentro la zucca: dietro la buccia, se ne vede solo la luce
            candleArt.sortingOrder = 8;
            flame.GetComponent<SpriteRenderer>().sortingOrder = 9;
            candleGlow.transform.localScale = Vector3.one * 3.2f;
            Sfx.Play(this, "clunk", 0.5f);
            Haptics.Light();
            yield return HalloweenKit.Pop(pumpkin, 1.05f, 0.2f);
            toast.Show("The candle is inside!");
            RefreshGlow();
            busy = false;
        }

        /// <summary>Luce della candela e della faccia: la zucca brilla solo con la candela accesa dentro.</summary>
        void RefreshGlow()
        {
            candleGlow.gameObject.SetActive(logic.CandleLit);
            bool inside = logic.CandleLit && logic.CandleInside;
            HalloweenKit.SetAlpha(pumpkinGlow, inside ? (logic.LightOn ? 0.35f : 0.75f) : 0f);
            bool glow = inside && !logic.LightOn;
            if (glow == glowing) return;
            glowing = glow;
            foreach (GameObject c in cuts) Recolor(c, glow);
        }

        void Recolor(GameObject cut, bool glow)
        {
            if (cut == null) return;
            LineRenderer[] lines = cut.GetComponentsInChildren<LineRenderer>();
            for (int i = 0; i < lines.Length; i++)
            {
                Color c = glow ? (i == 0 ? new Color(1f, 0.55f, 0.1f) : glowCutColor) : (i == 0 ? cutColor : cutInnerColor);
                lines[i].startColor = lines[i].endColor = c;
                lines[i].sortingOrder = (glow ? 61 : 12) + i;
            }
        }

        // ---------- finale ----------

        IEnumerator Ending()
        {
            busy = true;
            yield return HalloweenKit.Fade(darkness, 0.85f, 0.4f);
            glowing = false;
            RefreshGlow();
            HalloweenKit.SetAlpha(pumpkinGlow, 0.9f);
            Haptics.Medium();
            Sfx.Play(this, "whoosh", 0.6f);
            yield return new WaitForSeconds(0.5f);
            // la faccia prende vita: saltello e sorriso (la bocca intagliata si piega all'insù)
            StartCoroutine(HalloweenKit.Pop(pumpkin, 1.12f, 0.3f));
            yield return Smile();
            if (smoke != null) StartCoroutine(HalloweenKit.Burst(smoke, 0.5f));
            Sfx.Play(this, "pop", 0.7f);
            Haptics.Success();
            say.Show("Hee hee! HAPPY HALLOWEEN!", 2.5f);
            for (float t = 0f; t < 1.8f; t += Time.deltaTime)
            {
                HalloweenKit.SetAlpha(pumpkinGlow, 0.75f + Mathf.PerlinNoise(t * 6f, 1.7f) * 0.25f);
                yield return null;
            }
            yield return HalloweenKit.Wiggle(pumpkin, 5f, 0.6f);
            if (logic.TryCompleteEnding()) Win();
        }

        IEnumerator Smile()
        {
            FaceReport report = logic.LastReport;
            var mouths = new List<LineRenderer>();
            var original = new List<Vector3[]>();
            float minX = float.MaxValue, maxX = float.MinValue;
            for (int i = 0; i < cuts.Count && i < report.StrokeParts.Length; i++)
            {
                if (report.StrokeParts[i] != FacePart.Mouth || cuts[i] == null) continue;
                foreach (LineRenderer lr in cuts[i].GetComponentsInChildren<LineRenderer>())
                {
                    var pts = new Vector3[lr.positionCount];
                    lr.GetPositions(pts);
                    foreach (Vector3 p in pts) { minX = Mathf.Min(minX, p.x); maxX = Mathf.Max(maxX, p.x); }
                    mouths.Add(lr);
                    original.Add(pts);
                }
            }
            if (mouths.Count == 0) yield break;
            float cx = (minX + maxX) * 0.5f, half = Mathf.Max(0.1f, (maxX - minX) * 0.5f);
            for (float k = 0f; k <= 1f; k += Time.deltaTime / 0.5f)
            {
                for (int m = 0; m < mouths.Count; m++)
                {
                    Vector3[] src = original[m];
                    var bent = new Vector3[src.Length];
                    for (int i = 0; i < src.Length; i++)
                    {
                        float u = (src[i].x - cx) / half;
                        bent[i] = src[i] + new Vector3(0f, k * 0.18f * u * u, 0f);
                    }
                    mouths[m].SetPositions(bent);
                }
                yield return null;
            }
        }
    }
}
