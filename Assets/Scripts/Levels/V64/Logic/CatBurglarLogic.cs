using System;

namespace Gotcha.Levels.V64
{
    /// <summary>I ripiani su cui può stare il gatto, dal pavimento alla mensola (in ordine di altezza: la "scala" dei mobili).</summary>
    public enum CatPerch
    {
        None = -1,
        Floor = 0,
        Chair = 1,
        Table = 2,
        Cupboard = 3,
        Shelf = 4,
    }

    /// <summary>Dove si trova il barattolo dei biscotti.</summary>
    public enum CookieJarStage
    {
        OnShelf,    // sulla mensola (eventualmente già spinto un po' verso il bordo)
        Falling,    // il gatto l'ha buttato giù: sta cadendo
        Broken,     // è caduto sul pavimento: cocci e biscotti; fra un attimo ne compare un altro
        Landed,     // è caduto sul cuscino: vittoria (input bloccato)
        Done,
    }

    /// <summary>Cosa è successo in un passo di Tick (al massimo un evento per passo).</summary>
    public enum CatEvent
    {
        None,
        Pounce,         // il gatto spicca il salto verso il puntino (su un altro ripiano)
        Landed,         // il gatto è atterrato sul nuovo ripiano
        StartedPushing, // arrivato accanto al barattolo, comincia a spingerlo (da gatto)
        Nudge,          // una zampata: il barattolo si sposta un po' verso il bordo
        JarTipped,      // il barattolo ha passato il bordo: cade
        JarShattered,   // caduto sul pavimento: in mille pezzi (il gatto salta giù sulla credenza)
        JarOnCushion,   // caduto sul cuscino: salvo! (vittoria)
        NewJar,         // un barattolo nuovo è ricomparso sulla mensola
    }

    /// <summary>Esito di un movimento del puntino rosso.</summary>
    public enum DotResult
    {
        None,       // laser spento o input bloccato
        Watching,   // il puntino non è su nessun ripiano (muro, aria): il gatto lo guarda e basta
        SamePerch,  // sul ripiano del gatto: lo insegue camminando
        Chase,      // su un ripiano raggiungibile: il gatto salta (dopo un attimo)
        TooHigh,    // su un ripiano troppo alto da qui: il gatto lo fissa e scodinzola, ma non salta
    }

    /// <summary>Esito di un tocco sul puntatore laser.</summary>
    public enum LaserResult { None, On, Off }

    /// <summary>
    /// Logica pura del livello 269 "The Cat Burglar" — "GET THE COOKIE JAR DOWN!" Il barattolo dei biscotti è su una
    /// mensola altissima. Le false piste non funzionano mai e non azzerano niente: la scaletta sotto la mensola (troppo
    /// corta), la pila di libri (traballa e crolla), toccare il barattolo (troppo in alto), scuotere il telefono (il
    /// barattolo traballa ma resta lì), tirare su il gatto (è liquido: cola giù). Indizio: il gatto segue con gli occhi
    /// tutto quello che si muove; acceso il laser, il puntino rosso tremola e il gatto drizza le orecchie.
    /// Soluzione: accendere il laser e guidare il gatto col puntino su per i mobili (sedia, tavolo, credenza, mensola: il
    /// puntino "conta" solo sui ripiani, con un'aggancio generoso al più vicino; il gatto sale di un ripiano alla volta
    /// e scende dove vuole). Sulla mensola il gatto, da gatto, spinge piano piano il barattolo giù dal bordo: senza niente
    /// di morbido sotto si rompe (nessuna sconfitta: dopo un attimo ne ricompare uno e il gatto scende sulla credenza);
    /// col cuscino sotto la mensola atterra morbido e si vince, una volta sola. Nessuna dipendenza da Unity.
    /// </summary>
    public sealed class CatBurglarLogic
    {
        // ---- i ripiani (coordinate del mondo: il livello usa gli stessi numeri) ----
        public const int PerchCount = 5;
        static readonly float[] PerchY = { -3.5f, -2.6f, -1.65f, -0.45f, 1.35f };
        static readonly float[] PerchMinX = { -2.1f, -2.2f, -1.5f, -0.05f, 0.45f };
        static readonly float[] PerchMaxX = { 2.1f, -1.75f, -0.4f, 1.15f, 1.95f };

        /// <summary>Un puntino entro questa distanza da un ripiano conta come "sul ripiano" (aggancio generoso).</summary>
        public const float SnapRadius = 1.0f;

        // ---- il barattolo e il cuscino ----
        public const float JarStartX = 1.1f, EdgeX = 1.95f, LandX = 2.05f;
        /// <summary>Distanza fra il gatto e il barattolo quando lo spinge.</summary>
        public const float PushGap = 0.62f;
        /// <summary>Una zampata sposta il barattolo di NudgeStep in NudgeSeconds, dopo una pausa di WindUp (l'ultima è più lunga: lo sguardo).</summary>
        public const float NudgeStep = 0.18f, NudgeSeconds = 0.22f, WindUp = 0.38f, StareWindUp = 1.0f;
        /// <summary>Il cuscino conta "sotto la mensola" se il centro è entro questa distanza orizzontale dal punto d'arrivo.</summary>
        public const float CushionZone = 0.85f, CushionMaxY = -1.8f;

        // ---- tempi del gatto ----
        public const float CatSpeed = 2.2f, JumpSeconds = 0.5f, PounceDelay = 0.2f;
        public const float FallSeconds = 0.7f, BrokenSeconds = 2.2f;
        /// <summary>Passo massimo di Tick: dopo una pausa lunga niente salti.</summary>
        public const float MaxStep = 0.25f;

        public CookieJarStage JarStage { get; private set; } = CookieJarStage.OnShelf;
        /// <summary>Posizione orizzontale del barattolo sulla mensola.</summary>
        public float JarX { get; private set; } = JarStartX;
        /// <summary>Avanzamento della caduta (0..1).</summary>
        public float FallProgress => JarStage == CookieJarStage.Falling ? Math.Min(1f, fallT / FallSeconds) : (JarStage == CookieJarStage.OnShelf ? 0f : 1f);

        public CatPerch Perch { get; private set; } = CatPerch.Floor;
        public float CatX { get; private set; }
        public bool Jumping { get; private set; }
        public CatPerch JumpFrom { get; private set; }
        public CatPerch JumpTo { get; private set; }
        public float JumpFromX { get; private set; }
        public float JumpToX { get; private set; }
        /// <summary>Avanzamento del salto (0..1).</summary>
        public float JumpProgress => Jumping ? Math.Min(1f, jumpT / JumpSeconds) : 0f;
        /// <summary>Il gatto sta spingendo il barattolo (accanto ad esso, sulla mensola).</summary>
        public bool Pushing { get; private set; }
        /// <summary>Durante la spinta: la zampa si sta allungando (fase di spinta, 0..1), 0 mentre carica.</summary>
        public float PawOut { get; private set; }
        /// <summary>Prima dell'ultima zampata il gatto si ferma e guarda (il classico sguardo).</summary>
        public bool Staring => Pushing && LastNudge && pushT < StareWindUp;

        public bool LaserOn { get; private set; }
        public CatPerch DotPerch { get; private set; } = CatPerch.None;
        public float DotX { get; private set; }
        public bool CushionUnder { get; private set; }

        public int LadderTries { get; private set; }
        public int BookTopples { get; private set; }
        public int JarTaps { get; private set; }
        public int Shakes { get; private set; }
        public int CatDrags { get; private set; }
        public int TooHighs { get; private set; }
        public int Perks { get; private set; }
        public int Shatters { get; private set; }
        public int Nudges { get; private set; }
        public bool CompletionReported { get; private set; }

        public bool InputLocked => JarStage == CookieJarStage.Landed || JarStage == CookieJarStage.Done;
        public bool Solved => InputLocked;
        /// <summary>Tutte le mosse sbagliate fatte finora (solo statistica: non cambiano niente).</summary>
        public int WrongMoves => LadderTries + BookTopples + JarTaps + Shakes + CatDrags;

        public const float CatStartX = -0.95f;

        float jumpT, dotRest, fallT, brokenT, pushT;
        bool pushStartedReported;
        bool LastNudge => JarX + NudgeStep >= EdgeX - 1e-4f;

        public CatBurglarLogic() { Reset(); }

        public void Reset()
        {
            JarStage = CookieJarStage.OnShelf;
            JarX = JarStartX;
            Perch = CatPerch.Floor;
            CatX = CatStartX;
            Jumping = Pushing = false;
            JumpFrom = JumpTo = CatPerch.Floor;
            JumpFromX = JumpToX = CatStartX;
            PawOut = 0f;
            LaserOn = false;
            DotPerch = CatPerch.None;
            DotX = 0f;
            CushionUnder = false;
            jumpT = dotRest = fallT = brokenT = pushT = 0f;
            pushStartedReported = false;
            LadderTries = BookTopples = JarTaps = Shakes = CatDrags = TooHighs = Perks = Shatters = Nudges = 0;
            CompletionReported = false;
        }

        // ---------- Geometria ----------

        public static float SurfaceY(CatPerch p) => p == CatPerch.None ? float.NaN : PerchY[(int)p];
        public static float MinX(CatPerch p) => p == CatPerch.None ? float.NaN : PerchMinX[(int)p];
        public static float MaxX(CatPerch p) => p == CatPerch.None ? float.NaN : PerchMaxX[(int)p];

        /// <summary>Il gatto da "from" può saltare su "to"? Un ripiano alla volta in su, dove vuole in giù.</summary>
        public static bool Reachable(CatPerch from, CatPerch to)
        {
            if (from == CatPerch.None || to == CatPerch.None) return false;
            return (int)to == (int)from + 1 || (int)to < (int)from;
        }

        /// <summary>
        /// Il ripiano più vicino al punto (x,y) entro SnapRadius (distanza dal bordo superiore del ripiano), con la x
        /// riportata dentro il ripiano; None se il punto è lontano da tutti (muro, aria).
        /// </summary>
        public static CatPerch NearestPerch(float x, float y, out float onX)
        {
            onX = x;
            if (float.IsNaN(x) || float.IsNaN(y)) return CatPerch.None;
            CatPerch best = CatPerch.None;
            float bestD = SnapRadius;
            for (int i = 0; i < PerchCount; i++)
            {
                float cx = Clamp(x, PerchMinX[i], PerchMaxX[i]);
                float dx = x - cx, dy = y - PerchY[i];
                if (i == 0 && dy < 0f) dy = 0f;   // tutto il pavimento davanti conta come pavimento
                float d = (float)Math.Sqrt(dx * dx + dy * dy);
                if (d <= bestD) { bestD = d; best = (CatPerch)i; onX = cx; }
            }
            return best;
        }

        /// <summary>Dove può stare il gatto su un ripiano (sulla mensola: a sinistra del barattolo, se c'è).</summary>
        public float CatRange(CatPerch p, float x)
        {
            if (p == CatPerch.None) return CatX;
            float lo = PerchMinX[(int)p], hi = PerchMaxX[(int)p];
            if (p == CatPerch.Shelf && JarStage == CookieJarStage.OnShelf) hi = Math.Min(hi, JarX - PushGap);
            if (hi < lo) hi = lo;
            return Clamp(x, lo, hi);
        }

        static float Clamp(float v, float lo, float hi) => v < lo ? lo : v > hi ? hi : v;

        // ---------- Il laser ----------

        /// <summary>Il puntatore toccato: si accende (il puntino compare accanto al gatto, che drizza le orecchie) o si spegne.</summary>
        public LaserResult ToggleLaser()
        {
            if (InputLocked) return LaserResult.None;
            LaserOn = !LaserOn;
            if (!LaserOn)
            {
                DotPerch = CatPerch.None;
                dotRest = 0f;
                return LaserResult.Off;
            }
            Perks++;
            DotPerch = Perch == CatPerch.None ? CatPerch.Floor : (Jumping ? JumpTo : Perch);
            float baseX = Jumping ? JumpToX : CatX;
            DotX = Clamp(baseX + (baseX > 0.5f ? -0.6f : 0.6f), PerchMinX[(int)DotPerch], PerchMaxX[(int)DotPerch]);
            dotRest = 0f;
            return LaserResult.On;
        }

        /// <summary>Il puntino rosso portato in (x,y) col dito: si aggancia al ripiano più vicino, se ce n'è uno.</summary>
        public DotResult PointDot(float x, float y)
        {
            if (!LaserOn || InputLocked) return DotResult.None;
            CatPerch before = DotPerch;
            CatPerch p = NearestPerch(x, y, out float onX);
            DotPerch = p;
            DotX = onX;
            if (p != before) dotRest = 0f;
            if (p == CatPerch.None) return DotResult.Watching;
            CatPerch from = Jumping ? JumpTo : Perch;
            if (p == from) return DotResult.SamePerch;
            if (Reachable(from, p)) return DotResult.Chase;
            if (p != before) TooHighs++;
            return DotResult.TooHigh;
        }

        /// <summary>Il puntino attira il gatto verso un altro ripiano raggiungibile?</summary>
        bool DotPulls(out CatPerch to)
        {
            to = DotPerch;
            return LaserOn && DotPerch != CatPerch.None && DotPerch != Perch && Reachable(Perch, DotPerch);
        }

        // ---------- Il tempo ----------

        /// <summary>
        /// Il tempo che passa: il gatto salta e insegue il puntino, sulla mensola spinge il barattolo, il barattolo cade
        /// (sul cuscino: vittoria; sul pavimento: cocci e, dopo un attimo, un barattolo nuovo).
        /// </summary>
        public CatEvent Tick(float dt)
        {
            if (InputLocked) return CatEvent.None;
            if (float.IsNaN(dt) || dt < 0f) dt = 0f;
            dt = Math.Min(dt, MaxStep);

            // ---- il barattolo ----
            if (JarStage == CookieJarStage.Falling)
            {
                fallT += dt;
                if (fallT >= FallSeconds)
                {
                    if (CushionUnder)
                    {
                        JarStage = CookieJarStage.Landed;
                        Pushing = false;
                        PawOut = 0f;
                        return CatEvent.JarOnCushion;
                    }
                    JarStage = CookieJarStage.Broken;
                    brokenT = 0f;
                    Shatters++;
                    Pushing = false;
                    PawOut = 0f;
                    // il gatto, spaventato dal botto, salta giù sulla credenza (e il puntino lo segue lì)
                    if (Perch == CatPerch.Shelf && !Jumping) StartJump(CatPerch.Cupboard, CatX);
                    if (LaserOn) { DotPerch = Jumping ? JumpTo : Perch; DotX = Jumping ? JumpToX : CatX; dotRest = 0f; }
                    return CatEvent.JarShattered;
                }
            }
            else if (JarStage == CookieJarStage.Broken)
            {
                brokenT += dt;
                if (brokenT >= BrokenSeconds && !Jumping)
                {
                    JarStage = CookieJarStage.OnShelf;
                    JarX = JarStartX;
                    pushT = 0f;
                    pushStartedReported = false;
                    return CatEvent.NewJar;
                }
            }

            // ---- il gatto in volo ----
            if (Jumping)
            {
                jumpT += dt;
                if (jumpT < JumpSeconds) return CatEvent.None;
                Jumping = false;
                Perch = JumpTo;
                CatX = CatRange(Perch, JumpToX);
                jumpT = 0f;
                dotRest = 0f;
                return CatEvent.Landed;
            }
            if (JarStage == CookieJarStage.Broken) return CatEvent.None;   // spaventato: aspetta il barattolo nuovo

            // ---- il puntino lo chiama altrove ----
            if (DotPulls(out CatPerch target))
            {
                Pushing = false;
                PawOut = 0f;
                dotRest += dt;
                if (dotRest >= PounceDelay)
                {
                    StartJump(target, DotX);
                    return CatEvent.Pounce;
                }
                return CatEvent.None;
            }

            // ---- sulla mensola: il barattolo ----
            if (Perch == CatPerch.Shelf && JarStage == CookieJarStage.OnShelf)
            {
                float spot = JarX - PushGap;
                if (CatX < spot - 0.02f)
                {
                    Pushing = false;
                    PawOut = 0f;
                    CatX = Math.Min(spot, CatX + CatSpeed * 0.5f * dt);   // ci va con calma, da gatto
                    return CatEvent.None;
                }
                CatX = spot;
                if (!Pushing) { Pushing = true; pushT = 0f; }
                CatEvent ev = CatEvent.None;
                if (!pushStartedReported) { pushStartedReported = true; ev = CatEvent.StartedPushing; }
                float wind = LastNudge ? StareWindUp : WindUp;
                float before = pushT;
                pushT += dt;
                if (pushT <= wind) { PawOut = 0f; return ev; }
                if (before <= wind && ev == CatEvent.None) { Nudges++; ev = CatEvent.Nudge; }
                float k = Math.Min(1f, (pushT - wind) / NudgeSeconds);
                float kBefore = Math.Max(0f, Math.Min(1f, (before - wind) / NudgeSeconds));
                PawOut = (float)Math.Sin(k * Math.PI);
                JarX += NudgeStep * (k - kBefore);
                CatX = JarX - PushGap;
                if (k >= 1f) { pushT = 0f; PawOut = 0f; }
                if (JarX >= EdgeX - 1e-4f)
                {
                    JarX = EdgeX;
                    JarStage = CookieJarStage.Falling;
                    fallT = 0f;
                    Pushing = false;
                    PawOut = 0f;
                    return CatEvent.JarTipped;
                }
                return ev;
            }
            Pushing = false;
            PawOut = 0f;

            // ---- sullo stesso ripiano del puntino: lo insegue camminando ----
            if (LaserOn && DotPerch == Perch && DotPerch != CatPerch.None)
            {
                float to = CatRange(Perch, DotX);
                float step = CatSpeed * dt;
                CatX = Math.Abs(to - CatX) <= step ? to : CatX + Math.Sign(to - CatX) * step;
            }
            return CatEvent.None;
        }

        void StartJump(CatPerch to, float x)
        {
            JumpFrom = Perch;
            JumpFromX = CatX;
            JumpTo = to;
            Jumping = true;
            jumpT = 0f;
            Perch = to;                       // (per CatRange: la mensola tiene conto del barattolo)
            JumpToX = CatRange(to, x);
            Perch = JumpFrom;
            Pushing = false;
            PawOut = 0f;
            dotRest = 0f;
        }

        // ---------- Il cuscino ----------

        /// <summary>Il centro del cuscino (x,y) è nella zona sotto il bordo della mensola? (zona generosa)</summary>
        public static bool InCushionZone(float x, float y)
        {
            if (float.IsNaN(x) || float.IsNaN(y)) return false;
            return Math.Abs(x - LandX) <= CushionZone && y <= CushionMaxY;
        }

        /// <summary>Il cuscino messo sotto la mensola (true) o portato via (false). True se è cambiato qualcosa.</summary>
        public bool SetCushion(bool under)
        {
            if (InputLocked || CushionUnder == under) return false;
            CushionUnder = under;
            return true;
        }

        // ---------- Le false piste (non cambiano niente) ----------

        /// <summary>La scaletta sotto la mensola: troppo corta. 0 se bloccato.</summary>
        public int LadderUnderShelf() => InputLocked ? 0 : ++LadderTries;

        /// <summary>La pila di libri per arrampicarsi: traballa e crolla. 0 se bloccato.</summary>
        public int StackBooks() => InputLocked ? 0 : ++BookTopples;

        /// <summary>Un tocco sul barattolo: troppo in alto. 0 se non è sulla mensola.</summary>
        public int TapJar() => JarStage != CookieJarStage.OnShelf ? 0 : ++JarTaps;

        /// <summary>Il telefono scosso: il barattolo traballa ma non si muove. 0 se non è sulla mensola.</summary>
        public int Shake() => JarStage != CookieJarStage.OnShelf ? 0 : ++Shakes;

        /// <summary>Il gatto tirato su col dito: si allunga come un liquido e ricola giù. 0 se sta saltando o bloccato.</summary>
        public int DragCatUp() => InputLocked || Jumping ? 0 : ++CatDrags;

        /// <summary>Fine della festa: vittoria, una volta sola.</summary>
        public bool TryComplete()
        {
            if (JarStage != CookieJarStage.Landed || CompletionReported) return false;
            CompletionReported = true;
            JarStage = CookieJarStage.Done;
            return true;
        }
    }
}
