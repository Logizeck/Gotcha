using System;
using System.Collections.Generic;

namespace Gotcha.Levels.V64
{
    /// <summary>Fasi del livello 262 "The Immortal Candle".</summary>
    public enum CandleState
    {
        Lit,        // la fiamma è sullo stoppino (anche con un dito sopra: ridacchia e basta)
        Grabbed,    // due dita nella fiamma: la si tira su, ma è ancora attaccata allo stoppino
        Carried,    // staccata: segue le due dita, la candela è spenta
        Hovering,   // lasciata dopo lo stacco: resta sospesa un attimo (si può riprendere con due dita)
        Returning,  // torna da sola sullo stoppino (si può riprendere con due dita)
        Escaping,   // portata oltre il bordo in alto: scenetta finale, input bloccato
        Done,
    }

    /// <summary>Cosa è appena successo (il livello sceglie scritta, suono e animazione).</summary>
    public enum CandleEvent
    {
        None,
        Tickled,      // un dito sulla fiamma: si dimena e ridacchia (non si stacca)
        Grabbed,      // seconda dita nella fiamma: presa
        SnapBack,     // lasciata prima dello stacco: torna giù sullo stoppino
        Detached,     // le dita sono salite abbastanza: la fiamma si stacca, la candela si spegne
        Dropped,      // lasciata dopo lo stacco: resta sospesa
        Regrabbed,    // ripresa con due dita mentre era sospesa o tornava indietro
        Returning,    // comincia a tornare sullo stoppino
        Relit,        // è tornata sullo stoppino: candela di nuovo accesa
        Escaped,      // portata in cima: vittoria (scenetta)
        BlowGrow,     // soffio: la fiamma si allunga e cresce
        BlowTaunt,    // soffio (ogni tanto): "KEEP GOING, I'M GROWING!"
        FanOn,        // ventilatore acceso: la fiamma si piega dall'altra parte
        FanAgain,     // ventilatore toccato mentre gira: ricomincia il conto alla rovescia
        Poured,       // acqua versata sulla fiamma: vapore, la fiamma resta accesa
        PouredNothing,// acqua versata mentre la fiamma non c'è (è a spasso)
    }

    /// <summary>
    /// Logica pura del livello 262 "The Immortal Candle" — "Blow out the candle." Soffiare la fa solo crescere, il
    /// ventilatore la piega, l'acqua fa vapore, un dito la fa ridacchiare. Soluzione: due dita nella fiamma (in qualsiasi
    /// ordine), trascinarle in su finché la media delle due dita è salita di DetachRise (la fiamma si stacca, la candela è
    /// spenta) e portare la fiamma oltre TopY. Lasciata prima dello stacco torna sullo stoppino; lasciata dopo resta
    /// sospesa un attimo e poi torna (riprendibile con due dita). Coordinate del mondo (y in su), nessuna dipendenza da Unity.
    /// </summary>
    public sealed class ImmortalCandleLogic
    {
        // ---------- Regolazioni (permissive) ----------
        /// <summary>Raggio dell'area di presa attorno al centro della fiamma (generoso).</summary>
        public float GrabRadius = 1.1f;
        /// <summary>Di quanto deve salire la media delle due dita perché la fiamma si stacchi.</summary>
        public float DetachRise = 0.6f;
        /// <summary>Quanto si solleva (al massimo) la fiamma ancora attaccata mentre la si tira.</summary>
        public float PullLift = 0.3f;
        /// <summary>Il centro della fiamma oltre questa y = portata via (vittoria).</summary>
        public float TopY = 2.7f;
        /// <summary>La fiamma non esce mai da qui per sbaglio (MaxY > TopY: la cima si raggiunge).</summary>
        public float MinX = -2.3f, MaxX = 2.3f, MinY = -4.2f, MaxY = 3.3f;
        public float HoverSeconds = 0.8f;
        public float ReturnSeconds = 0.7f;
        public float FanSeconds = 3.5f;
        public float PourSeconds = 1.4f;
        /// <summary>Un soffio conta solo se il suono resta sopra soglia almeno così (niente schiocchi).</summary>
        public float BlowMinSeconds = 0.25f;
        /// <summary>Dopo così tanto suono continuo si smette di reagire (rumore di fondo forte): serve una pausa.</summary>
        public float BlowMaxSeconds = 2.5f;
        /// <summary>Silenzio necessario perché un soffio finisca (pause più brevi non lo spezzano).</summary>
        public float QuietGap = 0.2f;
        /// <summary>La frase della fiamma arriva ogni TauntEvery soffi (non a ogni rilevamento).</summary>
        public int TauntEvery = 3;
        /// <summary>Secondi per tornare alla misura normale dopo un soffio.</summary>
        public float GrowFallSeconds = 1f;

        // ---------- Stato ----------
        public CandleState State { get; private set; } = CandleState.Lit;
        public float HomeX { get; private set; }
        public float HomeY { get; private set; }
        public float FlameX { get; private set; }
        public float FlameY { get; private set; }
        /// <summary>0..1: quanto è tirata la fiamma ancora attaccata (per allungarla).</summary>
        public float Pull { get; private set; }
        /// <summary>0..1: quanto è cresciuta per il soffio.</summary>
        public float Grow { get; private set; }
        public bool Blowing { get; private set; }
        public bool FanOn { get; private set; }
        public bool GlassBusy { get; private set; }
        public int Tickles { get; private set; }
        public int BlowAttempts { get; private set; }
        public int FanTaps { get; private set; }
        public int Pours { get; private set; }
        public int SnapBacks { get; private set; }
        public int Drops { get; private set; }
        public bool CompletionReported { get; private set; }

        readonly Dictionary<int, float[]> fingers = new Dictionary<int, float[]>(4);
        readonly List<int> order = new List<int>(4);
        float startAvgY, offX, offY, timer, fanTimer, glassTimer, returnFromX, returnFromY;
        float loudFor, quietFor;
        bool inAttempt;

        public ImmortalCandleLogic(float homeX = 0f, float homeY = 0f)
        {
            HomeX = homeX;
            HomeY = homeY;
            Reset();
        }

        public bool CandleLit => State == CandleState.Lit || State == CandleState.Grabbed;
        public bool FlameAttached => CandleLit;
        public bool InputLocked => State == CandleState.Escaping || State == CandleState.Done;
        public int FlameFingerCount => fingers.Count;
        /// <summary>La fiamma attaccata va piegata dal vento del ventilatore.</summary>
        public bool FanBend => FanOn && FlameAttached;

        /// <summary>Posizione di riposo della fiamma (centro, sopra lo stoppino).</summary>
        public void SetHome(float x, float y)
        {
            HomeX = x;
            HomeY = y;
            if (FlameAttached) { FlameX = x; FlameY = y + PullLift * Pull; }
        }

        public void Reset()
        {
            State = CandleState.Lit;
            FlameX = HomeX;
            FlameY = HomeY;
            Pull = Grow = 0f;
            Blowing = FanOn = GlassBusy = false;
            Tickles = BlowAttempts = FanTaps = Pours = SnapBacks = Drops = 0;
            CompletionReported = false;
            fingers.Clear();
            order.Clear();
            startAvgY = offX = offY = timer = fanTimer = glassTimer = returnFromX = returnFromY = 0f;
            loudFor = quietFor = 0f;
            inAttempt = false;
        }

        public bool IsFlameFinger(int id) => fingers.ContainsKey(id);

        /// <summary>Il punto è nell'area di presa della fiamma (dov'è adesso)?</summary>
        public bool InGrabArea(float x, float y)
        {
            float dx = x - FlameX, dy = y - FlameY;
            return dx * dx + dy * dy <= GrabRadius * GrabRadius;
        }

        // ---------- Dita ----------

        /// <summary>
        /// Un dito appena appoggiato. Lo prende la fiamma solo se è nella sua area (al massimo due dita); il livello usa
        /// IsFlameFinger per sapere se il dito è suo o va al bicchiere/ventilatore.
        /// </summary>
        public CandleEvent FingerDown(int id, float x, float y)
        {
            if (InputLocked || fingers.ContainsKey(id) || fingers.Count >= 2 || !InGrabArea(x, y)) return CandleEvent.None;
            fingers[id] = new[] { x, y };
            order.Add(id);
            if (fingers.Count == 1)
            {
                if (State != CandleState.Lit) return CandleEvent.None;
                Tickles++;
                return CandleEvent.Tickled;
            }
            Avg(out float ax, out float ay);
            switch (State)
            {
                case CandleState.Lit:
                    State = CandleState.Grabbed;
                    startAvgY = ay;
                    Pull = 0f;
                    return CandleEvent.Grabbed;
                case CandleState.Hovering:
                case CandleState.Returning:
                    State = CandleState.Carried;
                    offX = FlameX - ax;
                    offY = FlameY - ay;
                    return CandleEvent.Regrabbed;
            }
            return CandleEvent.None;
        }

        /// <summary>Un dito si è spostato (dita non della fiamma: ignorate).</summary>
        public CandleEvent FingerMove(int id, float x, float y)
        {
            if (InputLocked || !fingers.TryGetValue(id, out float[] p)) return CandleEvent.None;
            p[0] = x;
            p[1] = y;
            if (fingers.Count < 2) return CandleEvent.None;
            Avg(out float ax, out float ay);
            if (State == CandleState.Grabbed)
            {
                float rise = ay - startAvgY;
                Pull = Clamp01(rise / Math.Max(0.01f, DetachRise));
                FlameX = HomeX;
                FlameY = HomeY + PullLift * Pull;
                if (rise < DetachRise) return CandleEvent.None;
                // si stacca: da qui segue le dita mantenendo la distanza dal loro punto medio
                State = CandleState.Carried;
                Pull = 0f;
                offX = FlameX - ax;
                offY = FlameY - ay;
                return CandleEvent.Detached;
            }
            if (State == CandleState.Carried) return Carry(ax, ay);
            return CandleEvent.None;
        }

        CandleEvent Carry(float ax, float ay)
        {
            FlameX = Math.Max(MinX, Math.Min(MaxX, ax + offX));
            FlameY = Math.Max(MinY, Math.Min(MaxY, ay + offY));
            if (FlameY < TopY) return CandleEvent.None;
            State = CandleState.Escaping;
            fingers.Clear();
            order.Clear();
            FanOn = false;
            Blowing = false;
            return CandleEvent.Escaped;
        }

        /// <summary>Un dito alzato (o perso).</summary>
        public CandleEvent FingerUp(int id)
        {
            if (!fingers.Remove(id)) return CandleEvent.None;
            order.Remove(id);
            if (InputLocked) return CandleEvent.None;
            return Released();
        }

        /// <summary>Pausa, perdita del fuoco, telefono che squilla: tutte le dita vengono lasciate (stato valido).</summary>
        public CandleEvent CancelFingers()
        {
            if (fingers.Count == 0) return CandleEvent.None;
            fingers.Clear();
            order.Clear();
            if (InputLocked) return CandleEvent.None;
            return Released();
        }

        CandleEvent Released()
        {
            if (fingers.Count >= 2) return CandleEvent.None;
            if (State == CandleState.Grabbed)
            {
                State = CandleState.Lit;
                Pull = 0f;
                FlameX = HomeX;
                FlameY = HomeY;
                SnapBacks++;
                return CandleEvent.SnapBack;
            }
            if (State == CandleState.Carried)
            {
                State = CandleState.Hovering;
                timer = HoverSeconds;
                Drops++;
                return CandleEvent.Dropped;
            }
            return CandleEvent.None;
        }

        void Avg(out float ax, out float ay)
        {
            ax = ay = 0f;
            int n = 0;
            foreach (int id in order)
            {
                float[] p = fingers[id];
                ax += p[0];
                ay += p[1];
                if (++n == 2) break;
            }
            if (n > 0) { ax /= n; ay /= n; }
        }

        // ---------- Tempo ----------

        /// <summary>Un passo di simulazione: ritorno della fiamma, ventilatore, bicchiere.</summary>
        public CandleEvent Tick(float dt)
        {
            dt = Math.Max(0f, dt);
            if (FanOn) { fanTimer -= dt; if (fanTimer <= 0f) FanOn = false; }
            if (GlassBusy) { glassTimer -= dt; if (glassTimer <= 0f) GlassBusy = false; }
            if (InputLocked) return CandleEvent.None;
            switch (State)
            {
                case CandleState.Hovering:
                    timer -= dt;
                    if (timer > 0f) return CandleEvent.None;
                    State = CandleState.Returning;
                    timer = 0f;
                    returnFromX = FlameX;
                    returnFromY = FlameY;
                    return CandleEvent.Returning;
                case CandleState.Returning:
                    timer += dt;
                    float t = Clamp01(timer / Math.Max(0.01f, ReturnSeconds));
                    float e = t * t * (3f - 2f * t);
                    FlameX = returnFromX + (HomeX - returnFromX) * e;
                    FlameY = returnFromY + (HomeY - returnFromY) * e;
                    if (t < 1f) return CandleEvent.None;
                    State = CandleState.Lit;
                    FlameX = HomeX;
                    FlameY = HomeY;
                    Pull = 0f;
                    // un dito rimasto appoggiato non diventa una presa a metà: si riparte puliti
                    fingers.Clear();
                    order.Clear();
                    return CandleEvent.Relit;
            }
            return CandleEvent.None;
        }

        // ---------- Mosse sbagliate ----------

        /// <summary>
        /// Microfono già filtrato dalla soglia calibrata (loud) o pulsante alternativo tenuto premuto. La fiamma cresce
        /// e torna normale; non si spegne mai. Un suono continuo lunghissimo (rumore di fondo) reagisce una volta sola.
        /// </summary>
        public CandleEvent BlowFeed(bool loud, float dt)
        {
            dt = Math.Max(0f, dt);
            CandleEvent ev = CandleEvent.None;
            if (loud) { loudFor += dt; quietFor = 0f; }
            else
            {
                quietFor += dt;
                if (quietFor >= QuietGap) { loudFor = 0f; inAttempt = false; }
            }
            bool blowing = !InputLocked && FlameAttached && loudFor >= BlowMinSeconds && loudFor <= BlowMinSeconds + BlowMaxSeconds
                && (loud || quietFor < QuietGap);
            if (blowing && !inAttempt)
            {
                inAttempt = true;
                BlowAttempts++;
                ev = TauntEvery > 0 && BlowAttempts % TauntEvery == 0 ? CandleEvent.BlowTaunt : CandleEvent.BlowGrow;
            }
            Blowing = blowing;
            if (blowing) Grow = Math.Min(1f, Grow + dt * 4f);
            else Grow = Math.Max(0f, Grow - dt / Math.Max(0.05f, GrowFallSeconds));
            return ev;
        }

        /// <summary>Tocco sul ventilatore: gira per qualche secondo, poi si ferma da solo.</summary>
        public CandleEvent TapFan()
        {
            if (InputLocked) return CandleEvent.None;
            FanTaps++;
            bool was = FanOn;
            FanOn = true;
            fanTimer = FanSeconds;
            return was ? CandleEvent.FanAgain : CandleEvent.FanOn;
        }

        /// <summary>Il bicchiere lasciato sopra la candela: versa (l'acqua non finisce mai), poi torna al suo posto.</summary>
        public bool CanPour => !InputLocked && !GlassBusy;

        public CandleEvent Pour()
        {
            if (!CanPour) return CandleEvent.None;
            GlassBusy = true;
            glassTimer = PourSeconds;
            Pours++;
            return FlameAttached ? CandleEvent.Poured : CandleEvent.PouredNothing;
        }

        // ---------- Fine ----------

        /// <summary>Fine della scenetta di vittoria: true una volta sola.</summary>
        public bool TryComplete()
        {
            if (State != CandleState.Escaping || CompletionReported) return false;
            CompletionReported = true;
            State = CandleState.Done;
            return true;
        }

        static float Clamp01(float v) => v < 0f ? 0f : v > 1f ? 1f : v;
    }
}
