using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels.V63;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 256 "Chain reaction" — il campanello è in alto a destra, irraggiungibile. Una macchina alla Rube Goldberg
    /// incompleta: quattro slot tratteggiati lungo il percorso (in cima alla mensola, la rampa, il tavolo, prima del filo
    /// del palloncino) e cinque oggetti sul ripiano in basso (palla, fila di domino, candela, libro, calamita). GO fa
    /// partire la catena, scriptata anello per anello: ogni anello che funziona si illumina di verde, il primo rotto
    /// diventa rosso con una battuta; poi la scena si rimette com'era (gli oggetti restano negli slot).
    /// Soluzione: palla sulla mensola, libro come rampa, domino sul tavolo, candela sotto il filo -> DING.
    /// Il palloncino trascinato in uno slot vola via ("Bye bye, balloon.") e ricompare legato.
    /// </summary>
    public class V63ChainReactionLevel : V62Level
    {
        [SerializeField] Transform[] items;          // Ball, Book, Domino, Candle, Magnet (ordine di ChainItem)
        [SerializeField] Collider2D[] itemAreas;
        [SerializeField] float[] halfHeights;        // metà altezza di ogni oggetto (il perno è al centro del disegno)
        [SerializeField] Transform ballArt;          // il disegno della palla (gira quando rotola)
        [SerializeField] Transform[] dominoes;       // i pezzi della fila (perno alla base: cadono ruotando)
        [SerializeField] GameObject candleFlame;
        [SerializeField] Transform[] slotPoints;     // punto d'appoggio di ogni slot (per la rampa: il centro)
        [SerializeField] SpriteRenderer[] slotRings;
        [SerializeField] Transform[] linkPaths;      // tratteggi del percorso, uno per anello (0-4)
        [SerializeField] Transform shelfEdge;        // dove la palla lascia la mensola
        [SerializeField] Transform rampTop;
        [SerializeField] Transform rampBottom;
        [SerializeField] Transform floorEnd;         // fine del tavolo (la palla che va per conto suo)
        [SerializeField] Transform stringPoint;      // dove la fiamma tocca il filo
        [SerializeField] Transform balloon;          // origine = nodo; il disegno è un figlio
        [SerializeField] Collider2D balloonArea;
        [SerializeField] Transform balloonPeg;
        [SerializeField] LineRenderer balloonString;
        [SerializeField] Transform bellHit;          // dove il palloncino tocca il campanello (il nodo si ferma qui)
        [SerializeField] Transform bell;
        [SerializeField] Collider2D bellArea;
        [SerializeField] SmokeEmitter bellWaves;
        [SerializeField] SmokeEmitter burnPuff;
        [SerializeField] Transform goButton;
        [SerializeField] Collider2D goArea;
        [SerializeField] TextMesh infoText;
        [SerializeField] Transform infoHolder;

        const float SlotReach = 0.7f;
        const float RampAngle = -45f;
        const float RowHalf = 0.33f;      // metà della fila di domino (la palla la colpisce lì)
        static readonly Color Idle = new Color(1f, 1f, 1f, 0.75f);
        static readonly Color Good = new Color(0.45f, 1f, 0.45f, 1f);
        static readonly Color Bad = new Color(1f, 0.35f, 0.3f, 1f);

        readonly ChainReactionLogic logic = new ChainReactionLogic();
        Camera cam;
        HwDragger drag;
        HwDragger.Item[] dragItems;
        HwDragger.Item balloonItem;
        HwToast toast;
        Vector3 balloonHome, balloonScale;
        float tapReadyAt;
        bool busy, balloonGone;

        static int Index(ChainItem it) => (int)it - 1;
        Transform T(ChainItem it) => items[Index(it)];

        public override void OnLevelStart()
        {
            cam = Camera.main;
            logic.Reset();
            ResetV62();
            toast = new HwToast(infoText, infoHolder);
            drag = new HwDragger(this);
            dragItems = new HwDragger.Item[items.Length];
            for (int i = 0; i < items.Length; i++) dragItems[i] = drag.Add(items[i], itemAreas[i], 5);
            balloonHome = balloon.position;
            balloonScale = balloon.localScale;
            balloonItem = drag.Add(balloon, balloonArea, 4);
            foreach (SmokeEmitter e in new[] { bellWaves, burnPuff }) if (e != null) e.Stop();
            ColorAll(Idle);
            toast.Show("Build the machine. Then press GO!", 2.4f);
        }

        void Update()
        {
            toast.Tick();
            UpdateString();
            if (IsFinished || busy || logic.InputLocked) return;
            if (!HalloweenKit.ReadPointer(cam, out Vector2 w, out bool began, out bool ended))
            {
                if (drag.Held != null) Drop(drag.Held, true);
                return;
            }
            if (began)
            {
                HwDragger.Item it = drag.TryPick(w);
                if (it != null) OnPick(it);
                else Tap(w);
            }
            if (drag.Held == null) return;
            drag.MoveHeld(w);
            if (ended) Drop(drag.Held, false);
        }

        /// <summary>Il filo va dal paletto al nodo finché il palloncino è legato (anche mentre lo si trascina).</summary>
        void UpdateString()
        {
            if (balloonString == null) return;
            balloonString.enabled = !balloonGone;
            if (balloonGone) return;
            balloonString.SetPosition(0, balloonPeg.position);
            balloonString.SetPosition(1, balloon.position);
        }

        void OnPick(HwDragger.Item it)
        {
            ChainItem item = ItemOf(it);
            if (item != ChainItem.None && logic.TryRemove(item)) it.T.localRotation = Quaternion.identity;
            if (item == ChainItem.Book) it.T.localRotation = Quaternion.identity;
        }

        ChainItem ItemOf(HwDragger.Item it)
        {
            for (int i = 0; i < dragItems.Length; i++) if (dragItems[i] == it) return (ChainItem)(i + 1);
            return it == balloonItem ? ChainItem.Balloon : ChainItem.None;
        }

        void Tap(Vector2 w)
        {
            if (goArea.OverlapPoint(w)) { Go(); return; }
            if (bellArea.OverlapPoint(w) && Time.time >= tapReadyAt)
            {
                tapReadyAt = Time.time + 0.4f;
                Sfx.Play(this, "bonk", 0.4f);
                StartCoroutine(HalloweenKit.Shake(bell, 0.03f, 0.25f));
                toast.Show("Too far. Use your brain, not your finger.", 2f);
            }
        }

        int SlotNear(Vector3 p)
        {
            int best = -1;
            float bd = float.MaxValue;
            for (int i = 0; i < slotRings.Length; i++)
            {
                float reach = SlotReach * Mathf.Max(0.8f, slotRings[i].transform.lossyScale.x);
                float d = Vector2.Distance(p, slotRings[i].transform.position);
                if (d < reach && d < bd) { bd = d; best = i; }
            }
            return best;
        }

        void Drop(HwDragger.Item it, bool lostFinger)
        {
            drag.Release();
            ChainItem item = ItemOf(it);
            int slot = lostFinger ? -1 : SlotNear(it.T.position + Vector3.up * Lift(item));
            if (item == ChainItem.Balloon)
            {
                if (slot >= 0 && logic.TryBalloonInSlot()) { StartCoroutine(BalloonFliesAway()); return; }
                drag.SendHome(balloonItem, 0.35f);
                return;
            }
            if (slot >= 0 && logic.TryPlace(item, slot, out ChainItem displaced))
            {
                if (displaced != ChainItem.None) SendToTray(displaced);
                Seat(item, slot, 0.2f);
                Sfx.Play(this, "clunk", 0.5f);
                Haptics.Light();
                return;
            }
            SendToTray(item);
        }

        /// <summary>Quanto sta sopra il perno il centro del disegno (domino e candela hanno il perno alla base).</summary>
        static float Lift(ChainItem item) =>
            item == ChainItem.Domino ? 0.27f : item == ChainItem.Candle ? 0.4f : item == ChainItem.Balloon ? 0.55f : 0f;

        void SendToTray(ChainItem item)
        {
            HwDragger.Item d = dragItems[Index(item)];
            d.T.localRotation = Quaternion.identity;
            drag.SendHome(d, 0.35f);
        }

        /// <summary>Dove sta un oggetto in uno slot (e il libro nella rampa si inclina).</summary>
        Vector3 SeatOf(ChainItem item, int slot)
        {
            Vector3 p = slotPoints[slot].position;
            if (slot == 1) return p;
            return p + Vector3.up * halfHeights[Index(item)];
        }

        void Seat(ChainItem item, int slot, float seconds)
        {
            Transform t = T(item);
            t.localRotation = Quaternion.Euler(0f, 0f, item == ChainItem.Book && slot == 1 ? RampAngle : 0f);
            HwDragger.Item d = dragItems[Index(item)];
            drag.SendTo(d, SeatOf(item, slot), seconds, 0.15f);
        }

        // ---------- la catena ----------

        void Go()
        {
            if (!logic.TryGo(out ChainRun run)) return;
            drag.Cancel();
            StartCoroutine(Run(run));
        }

        IEnumerator Run(ChainRun run)
        {
            busy = true;
            ColorAll(Idle);
            Sfx.Play(this, "tick", 0.7f);
            yield return HalloweenKit.Pop(goButton, 0.85f, 0.15f);
            Transform ball = T(ChainItem.Ball);
            float r = halfHeights[Index(ChainItem.Ball)];
            bool done = false;
            // anello 0: la spinta in cima alla mensola
            if (run.Links == 0) { yield return BreakAtStart(run.Outcome); done = true; }
            if (!done)
            {
                Light(0, Good);
                yield return RollTo(ball, shelfEdge.position + Vector3.up * r, 0.6f);
                // anello 1: la rampa
                if (run.Links == 1) { yield return BreakAtRamp(run.Outcome, ball, r); done = true; }
            }
            if (!done)
            {
                Light(1, Good);
                Vector3 n = Quaternion.Euler(0f, 0f, RampAngle) * Vector3.up * r;
                yield return HalloweenKit.Move(ball, rampTop.position + n, 0.2f);
                yield return RollTo(ball, rampBottom.position + n, 0.45f);
                yield return RollTo(ball, slotPoints[2].position + new Vector3(-RowHalf - r, r, 0f), 0.25f);
                // anello 2: sul tavolo
                if (run.Links == 2) { yield return BreakOnFloor(run.Outcome, ball, r); done = true; }
            }
            if (!done)
            {
                Light(2, Good);
                Sfx.Play(this, "bonk", 0.4f);
                yield return Topple();
                // anello 3: prima del filo
                if (run.Links == 3) { yield return BreakBeforeString(run.Outcome); done = true; }
            }
            if (!done)
            {
                Light(3, Good);
                yield return Ding();
                yield break;
            }
            // una catena rotta: battuta, una pausa, e si rimette tutto com'era
            toast.Show(ChainReactionLogic.Joke(run.Outcome), 2.4f);
            if (run.BrokenSlot >= 0) Light(run.BrokenSlot, Bad);
            Haptics.Light();
            yield return new WaitForSeconds(2.0f);
            yield return Restore();
            logic.TryFinishRun();
            busy = false;
        }

        IEnumerator BreakAtStart(ChainOutcome o)
        {
            Sfx.Play(this, "bonk", 0.5f);
            switch (o)
            {
                case ChainOutcome.NothingToPush:
                    yield return HalloweenKit.Shake(goButton, 0.05f, 0.3f);
                    break;
                case ChainOutcome.StartDomino:
                    yield return Topple();
                    break;
                case ChainOutcome.StartCandle:
                    yield return Tip(T(ChainItem.Candle), -80f);
                    HalloweenKit.Show(candleFlame, false);
                    break;
                case ChainOutcome.StartBook:
                    yield return Tip(T(ChainItem.Book), -12f);
                    break;
                default:
                    yield return HalloweenKit.Wiggle(T(ChainItem.Magnet), 6f, 0.6f, 14f);
                    break;
            }
        }

        IEnumerator BreakAtRamp(ChainOutcome o, Transform ball, float r)
        {
            switch (o)
            {
                case ChainOutcome.NoRamp:
                    yield return HalloweenKit.Wiggle(ball, 10f, 0.8f, 16f);
                    break;
                case ChainOutcome.RampDomino:
                    Sfx.Play(this, "bonk", 0.5f);
                    yield return Topple();
                    yield return HalloweenKit.Wiggle(ball, 8f, 0.5f, 16f);
                    break;
                case ChainOutcome.RampCandle:
                    Sfx.Play(this, "bonk", 0.5f);
                    yield return Tip(T(ChainItem.Candle), -80f);
                    HalloweenKit.Show(candleFlame, false);
                    break;
                default:
                    yield return StickToMagnet(ball);
                    break;
            }
        }

        IEnumerator BreakOnFloor(ChainOutcome o, Transform ball, float r)
        {
            switch (o)
            {
                case ChainOutcome.BallRollsAway:
                    yield return RollTo(ball, floorEnd.position + Vector3.up * r, 0.9f);
                    yield return HalloweenKit.Move(ball, floorEnd.position + new Vector3(0.4f, -1.2f, 0f), 0.35f);
                    Sfx.Play(this, "bonk", 0.5f);
                    break;
                case ChainOutcome.MagnetGrabsBall:
                    yield return StickToMagnet(ball);
                    break;
                case ChainOutcome.HitsBook:
                    Sfx.Play(this, "bonk", 0.6f);
                    yield return HalloweenKit.Shake(T(ChainItem.Book), 0.05f, 0.3f);
                    yield return RollTo(ball, ball.position + Vector3.left * 0.25f, 0.25f);
                    break;
                case ChainOutcome.ToastedDominoes:
                    Sfx.Play(this, "bonk", 0.5f);
                    yield return Tip(T(ChainItem.Candle), -80f);
                    if (burnPuff != null)
                    {
                        burnPuff.transform.position = T(ChainItem.Domino).position;
                        StartCoroutine(HalloweenKit.Burst(burnPuff, 0.5f));
                    }
                    SetTint(T(ChainItem.Domino), new Color(0.35f, 0.3f, 0.3f));
                    HalloweenKit.Show(candleFlame, false);
                    break;
                default:   // HitsCandle
                    Sfx.Play(this, "bonk", 0.6f);
                    yield return Tip(T(ChainItem.Candle), -80f);
                    HalloweenKit.Show(candleFlame, false);
                    break;
            }
        }

        IEnumerator BreakBeforeString(ChainOutcome o)
        {
            Sfx.Play(this, "clunk", 0.5f);
            if (o == ChainOutcome.DominoOnMagnet) yield return HalloweenKit.Shake(T(ChainItem.Magnet), 0.05f, 0.4f);
            else if (o == ChainOutcome.DominoOnBook) yield return HalloweenKit.Shake(T(ChainItem.Book), 0.04f, 0.4f);
            else yield return new WaitForSeconds(0.4f);
        }

        /// <summary>La catena completa: la candela va sotto il filo, il filo brucia, il palloncino sale e... DING.</summary>
        IEnumerator Ding()
        {
            Transform candle = T(ChainItem.Candle);
            Vector3 under = new Vector3(stringPoint.position.x - 0.05f, candle.position.y, candle.position.z);
            yield return HalloweenKit.Move(candle, under, 0.35f);
            Sfx.Play(this, "whoosh", 0.6f);
            if (burnPuff != null) { burnPuff.transform.position = stringPoint.position; StartCoroutine(HalloweenKit.Burst(burnPuff, 0.4f)); }
            yield return new WaitForSeconds(0.4f);
            balloonGone = true;   // il filo è bruciato
            Light(4, Good);
            Sfx.Play(this, "pop", 0.5f);
            yield return HalloweenKit.Move(balloon, bellHit.position, 0.9f);
            Sfx.Play(this, "ding", 1f);
            if (bellWaves != null) StartCoroutine(HalloweenKit.Burst(bellWaves, 0.8f));
            StartCoroutine(HalloweenKit.Shake(bell, 0.06f, 0.6f));
            Haptics.Medium();
            toast.Show("DING!", 2f);
            logic.TryFinishRun();
            yield return Celebrate(2.2f);
            if (logic.TryCompleteRing()) Win();
        }

        IEnumerator StickToMagnet(Transform ball)
        {
            Transform magnet = T(ChainItem.Magnet);
            yield return HalloweenKit.Move(ball, magnet.position + Vector3.up * 0.05f, 0.25f, 0.2f);
            Sfx.Play(this, "clunk", 0.8f);
            yield return HalloweenKit.Shake(magnet, 0.05f, 0.3f);
        }

        /// <summary>La fila di domino cade da sinistra a destra.</summary>
        IEnumerator Topple()
        {
            foreach (Transform d in dominoes)
            {
                Sfx.Play(this, "tick", 0.5f);
                StartCoroutine(Tip(d, -72f, 0.12f));
                yield return new WaitForSeconds(0.08f);
            }
            yield return new WaitForSeconds(0.15f);
        }

        IEnumerator Tip(Transform t, float to, float seconds = 0.25f)
        {
            float from = t.localEulerAngles.z;
            for (float k = 0f; k < 1f; k += Time.deltaTime / seconds)
            {
                t.localRotation = Quaternion.Euler(0f, 0f, Mathf.LerpAngle(from, from + to, k * k));
                yield return null;
            }
            t.localRotation = Quaternion.Euler(0f, 0f, from + to);
        }

        IEnumerator RollTo(Transform ball, Vector3 to, float seconds)
        {
            Vector3 from = ball.position;
            float r = Mathf.Max(0.05f, halfHeights[Index(ChainItem.Ball)]);
            float spin0 = ballArt != null ? ballArt.localEulerAngles.z : 0f;
            float dist = Vector2.Distance(from, to);
            for (float k = 0f; k < 1f; k += Time.deltaTime / Mathf.Max(0.01f, seconds))
            {
                ball.position = Vector3.Lerp(from, to, k);
                if (ballArt != null) ballArt.localRotation = Quaternion.Euler(0f, 0f, spin0 - Mathf.Sign(to.x - from.x + 0.0001f) * dist * k / r * Mathf.Rad2Deg);
                yield return null;
            }
            ball.position = to;
        }

        /// <summary>Dopo un tentativo: tutto torna nei suoi slot (o sul ripiano), in piedi e acceso.</summary>
        IEnumerator Restore()
        {
            foreach (Transform d in dominoes) d.localRotation = Quaternion.identity;
            SetTint(T(ChainItem.Domino), Color.white);
            HalloweenKit.Show(candleFlame, true);
            if (ballArt != null) ballArt.localRotation = Quaternion.identity;
            for (int i = 0; i < items.Length; i++)
            {
                ChainItem it = (ChainItem)(i + 1);
                int slot = logic.SlotOf(it);
                if (slot >= 0) Seat(it, slot, 0.3f);
                else SendToTray(it);
            }
            ColorAll(Idle);
            yield return new WaitForSeconds(0.35f);
        }

        IEnumerator BalloonFliesAway()
        {
            busy = true;
            balloonItem.Enabled = false;
            balloonGone = true;
            toast.Show("Bye bye, balloon.", 1.8f);
            Sfx.Play(this, "whoosh", 0.6f);
            Vector3 from = balloon.position;
            for (float t = 0f; t < 1.6f; t += Time.deltaTime)
            {
                balloon.position = from + new Vector3(Mathf.Sin(t * 3f) * 0.25f, t * t * 3.5f, 0f);
                yield return null;
            }
            balloon.position = balloonHome;
            balloon.localScale = balloonScale * 0.1f;
            balloonGone = false;
            Sfx.Play(this, "pop", 0.5f);
            yield return HalloweenKit.ScaleTo(balloon, balloonScale, 0.3f);
            balloonItem.Enabled = true;
            busy = false;
        }

        // ---------- colori del percorso ----------

        void ColorAll(Color c)
        {
            for (int i = 0; i < linkPaths.Length; i++) Light(i, c);
            foreach (SpriteRenderer r in slotRings) if (r != null) r.color = c;
        }

        void Light(int link, Color c)
        {
            if (link < slotRings.Length && slotRings[link] != null) slotRings[link].color = c;
            if (link < 0 || link >= linkPaths.Length || linkPaths[link] == null) return;
            foreach (SpriteRenderer r in linkPaths[link].GetComponentsInChildren<SpriteRenderer>(true)) r.color = c;
        }

        static void SetTint(Transform t, Color c)
        {
            foreach (SpriteRenderer r in t.GetComponentsInChildren<SpriteRenderer>(true)) r.color = c;
        }
    }
}
