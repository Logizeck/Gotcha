namespace Gotcha.Levels.V64
{
    /// <summary>Stato del livello "Love the Bug" (roadmap n. 8).</summary>
    public enum LoveTheBugState
    {
        Wandering,  // gli insetti camminano, nessun dito sopra
        Pressing,   // un dito tiene fermo un insetto: il cuore si riempie (se si alza presto è un tocco)
        Won,        // tenuto per HoldTime: diventato un cuore (festa, input chiuso)
    }

    /// <summary>Cosa è successo dopo un input o un passo di tempo.</summary>
    public enum LoveTheBugEvent
    {
        None,       // niente da mostrare
        Ignored,    // dito ignorato (secondo dito, oppure dopo la vittoria)
        Missed,     // dito sceso lontano da tutti gli insetti
        Pressed,    // un dito si è appoggiato su un insetto: si ferma, il cuore comincia a riempirsi
        Split,      // tocco veloce: l'insetto si sdoppia ("pop")
        Scatter,    // tocco veloce ma gli insetti sono già al massimo: scappano ridacchiando, nessun nuovo insetto
        LetGo,      // pressione lunga interrotta prima di HoldTime (dito alzato o scivolato via): niente sdoppiamento
        Slipped,    // il dito è scivolato via subito (meno di TapTime): annullato in silenzio, niente sdoppiamento
        Loved,      // tenuto HoldTime: VITTORIA (una volta sola)
    }

    /// <summary>
    /// Logica pura del livello "Love the Bug" (id love_the_bug, roadmap n. 8) — "GET RID OF THE BUG." Un insettino verde
    /// cammina sulla finestra di una app di fantasia che funziona male. Tappare un insetto lo SDOPPIA (al massimo
    /// <see cref="MaxBugs"/>; al massimo un tocco li fa solo sparpagliare), i difetti peggiorano e la barra torna indietro.
    /// Soluzione esatta: tenere il dito su un insetto (qualsiasi) per <see cref="HoldTime"/> s. Un tocco è SOLO una
    /// pressione più breve di <see cref="TapTime"/>: una pressione più lunga non sdoppia mai, nemmeno se interrotta.
    /// Alzare il dito o farlo scivolare oltre <see cref="SlipRadius"/> prima di HoldTime annulla (il rossore cala, l'insetto
    /// riparte). Il secondo dito è ignorato. Nessuna sconfitta, nessuno stato senza uscita: Cancel() (pausa, perdita del
    /// fuoco) torna sempre a Wandering senza contare niente. Percorsi deterministici (rimbalzo dentro i limiti), niente
    /// fisica né casualità. Coordinate del mondo; nessuna dipendenza da Unity.
    /// </summary>
    public sealed class LoveTheBugLogic
    {
        public const float HoldTime = 2.0f;      // pressione che vince
        public const float TapTime = 0.25f;      // sotto questa durata (e solo sotto) la pressione è un tocco
        public const int MaxBugs = 8;            // tetto degli insetti
        public const float HitRadius = 0.7f;     // raggio generoso per prendere un insetto (anche in movimento)
        public const float SlipRadius = 1.1f;    // il dito può muoversi fin qui dall'insetto senza annullare
        public const float Speed = 0.55f;        // velocità di camminata
        public const float ScatterTime = 0.7f;   // durata dello sparpagliamento al tetto
        public const float ScatterBoost = 2.6f;  // moltiplicatore della velocità mentre si sparpagliano
        public const float DrainTime = 0.5f;     // il rossore torna a zero in questo tempo dopo il rilascio
        public const float SplitGap = 0.4f;      // distanza a cui nasce il nuovo insetto
        public const int HintAfter = 5;          // tocchi sbagliati dopo cui arriva la frase sottile ("hug")
        public const float StartProgress = 0.42f;   // barra di caricamento all'inizio
        public const float ProgressPerBug = 0.05f;  // ogni insetto in più la fa tornare indietro di tanto
        public const float MinProgress = 0.06f;
        public const int NoFinger = int.MinValue;
        const float TimeEps = 1e-4f;             // tolleranza sugli arrotondamenti della somma dei fotogrammi

        readonly float minX, maxX, minY, maxY, startX, startY;
        readonly float[] bx = new float[MaxBugs], by = new float[MaxBugs], vx = new float[MaxBugs], vy = new float[MaxBugs], blush = new float[MaxBugs];
        int holder = NoFinger, pressed = -1, meterBug = -1, splits;
        float holdT, scatterT;
        bool completed, hintSaid;

        /// <summary>Limiti del CENTRO degli insetti (già ristretti della loro mezza misura) e posto di partenza.</summary>
        public LoveTheBugLogic(float minX, float maxX, float minY, float maxY, float startX, float startY)
        {
            this.minX = System.Math.Min(minX, maxX);
            this.maxX = System.Math.Max(minX, maxX);
            this.minY = System.Math.Min(minY, maxY);
            this.maxY = System.Math.Max(minY, maxY);
            this.startX = Clamp(startX, this.minX, this.maxX);
            this.startY = Clamp(startY, this.minY, this.maxY);
            Reset();
        }

        public LoveTheBugState State { get; private set; }
        /// <summary>Quanti insetti ci sono (1..MaxBugs).</summary>
        public int Count { get; private set; }
        /// <summary>Tocchi sbagliati (sdoppiamenti e sparpagliamenti).</summary>
        public int WrongTaps { get; private set; }
        /// <summary>Pressioni lunghe interrotte (per variare le battute).</summary>
        public int LetGos { get; private set; }
        public int Holder => holder;
        /// <summary>Insetto tenuto dal dito (-1 se nessuno). Dopo la vittoria: quello diventato cuore.</summary>
        public int PressedBug => pressed;
        /// <summary>Insetto su cui si mostra il cuore-misuratore (quello tenuto o l'ultimo lasciato), -1 se nessuno.</summary>
        public int MeterBug => meterBug;
        /// <summary>Riempimento del misuratore 0..1 (durante la pressione = tempo tenuto / HoldTime, poi cala).</summary>
        public float MeterFill => meterBug >= 0 && meterBug < Count ? blush[meterBug] : 0f;
        /// <summary>Avanzamento della pressione 0..1 (1 dopo la vittoria).</summary>
        public float HoldProgress => State == LoveTheBugState.Won ? 1f : State == LoveTheBugState.Pressing ? Clamp(holdT / HoldTime, 0f, 1f) : 0f;
        public float HeldSeconds => State == LoveTheBugState.Pressing ? holdT : 0f;
        /// <summary>Quanto è rotta la app 0..1 (cresce con gli insetti; 0 dopo la vittoria).</summary>
        public float Glitch => State == LoveTheBugState.Won ? 0f : (Count - 1) / (float)(MaxBugs - 1);
        /// <summary>Barra di caricamento 0..1: torna indietro con gli insetti, piena dopo la vittoria.</summary>
        public float Progress => State == LoveTheBugState.Won ? 1f : System.Math.Max(MinProgress, StartProgress - ProgressPerBug * (Count - 1));
        public bool Scattering => scatterT > 0f;
        public bool InputLocked => State == LoveTheBugState.Won;
        public float MinX => minX;
        public float MaxX => maxX;
        public float MinY => minY;
        public float MaxY => maxY;

        public float BugX(int i) => bx[i];
        public float BugY(int i) => by[i];
        public float BugVX(int i) => vx[i];
        public float BugVY(int i) => vy[i];
        public float Blush(int i) => blush[i];
        /// <summary>True se l'insetto i sta camminando (non tenuto, partita non finita).</summary>
        public bool Walking(int i) => i >= 0 && i < Count && i != pressed && State != LoveTheBugState.Won;

        /// <summary>Si ricomincia da capo (rientro nel livello): un insetto, stesso posto, stessi percorsi.</summary>
        public void Reset()
        {
            State = LoveTheBugState.Wandering;
            Count = 1;
            WrongTaps = LetGos = splits = 0;
            holder = NoFinger;
            pressed = meterBug = -1;
            holdT = scatterT = 0f;
            completed = hintSaid = false;
            for (int i = 0; i < MaxBugs; i++) { bx[i] = by[i] = vx[i] = vy[i] = blush[i] = 0f; }
            bx[0] = startX;
            by[0] = startY;
            Heading(0, 0);
        }

        /// <summary>Insetto più vicino entro HitRadius da (x, y), -1 se nessuno.</summary>
        public int BugAt(float x, float y)
        {
            int best = -1;
            float bestD = HitRadius * HitRadius;
            for (int i = 0; i < Count; i++)
            {
                float dx = x - bx[i], dy = y - by[i], d = dx * dx + dy * dy;
                if (d <= bestD) { bestD = d; best = i; }
            }
            return best;
        }

        /// <summary>La frase sottile dopo <see cref="HintAfter"/> tocchi sbagliati: true una volta sola.</summary>
        public bool TakeHint()
        {
            if (hintSaid || WrongTaps < HintAfter) return false;
            hintSaid = true;
            return true;
        }

        /// <summary>Un dito scende in (x, y).</summary>
        public LoveTheBugEvent Down(int finger, float x, float y)
        {
            if (State == LoveTheBugState.Won) return LoveTheBugEvent.Ignored;
            if (State == LoveTheBugState.Pressing) return finger == holder ? LoveTheBugEvent.None : LoveTheBugEvent.Ignored;
            int i = BugAt(x, y);
            if (i < 0) return LoveTheBugEvent.Missed;
            holder = finger;
            pressed = meterBug = i;
            holdT = 0f;
            blush[i] = 0f;
            State = LoveTheBugState.Pressing;
            return LoveTheBugEvent.Pressed;
        }

        /// <summary>Un dito già giù si sposta: se è quello che tiene e si allontana troppo dall'insetto, si annulla.</summary>
        public LoveTheBugEvent Move(int finger, float x, float y)
        {
            if (State != LoveTheBugState.Pressing || finger != holder) return LoveTheBugEvent.None;
            float dx = x - bx[pressed], dy = y - by[pressed];
            if (dx * dx + dy * dy <= SlipRadius * SlipRadius) return LoveTheBugEvent.None;
            bool longPress = holdT >= TapTime - TimeEps;
            Release();
            if (!longPress) return LoveTheBugEvent.Slipped;
            LetGos++;
            return LoveTheBugEvent.LetGo;
        }

        /// <summary>Un dito si alza. Tocco breve sull'insetto = sdoppiamento; pressione lunga interrotta = niente.</summary>
        public LoveTheBugEvent Up(int finger)
        {
            if (State != LoveTheBugState.Pressing || finger != holder) return LoveTheBugEvent.None;
            int i = pressed;
            bool tap = holdT < TapTime - TimeEps;
            Release();
            if (!tap) { LetGos++; return LoveTheBugEvent.LetGo; }
            WrongTaps++;
            if (Count >= MaxBugs)
            {
                scatterT = ScatterTime;
                for (int k = 0; k < Count; k++) { vx[k] = -vx[k]; vy[k] = -vy[k]; }
                return LoveTheBugEvent.Scatter;
            }
            SplitBug(i);
            return LoveTheBugEvent.Split;
        }

        /// <summary>Tutte le dita spariscono (pausa, fuoco perso, livello spento): niente sdoppiamenti né battute.</summary>
        public void Cancel()
        {
            if (State == LoveTheBugState.Pressing) Release();
            holder = NoFinger;
        }

        /// <summary>Passo di tempo: camminata, rossore che cala, pressione che si riempie.</summary>
        public LoveTheBugEvent Tick(float dt)
        {
            if (dt < 0f) dt = 0f;
            if (State == LoveTheBugState.Won) return LoveTheBugEvent.None;
            if (scatterT > 0f) scatterT = System.Math.Max(0f, scatterT - dt);
            float k = scatterT > 0f ? ScatterBoost : 1f;
            for (int i = 0; i < Count; i++)
            {
                if (i == pressed) continue;
                Walk(i, dt * k);
                if (blush[i] > 0f) blush[i] = System.Math.Max(0f, blush[i] - dt / DrainTime);
            }
            if (State == LoveTheBugState.Pressing)
            {
                holdT += dt;
                blush[pressed] = Clamp(holdT / HoldTime, 0f, 1f);
                if (holdT >= HoldTime - TimeEps)
                {
                    State = LoveTheBugState.Won;
                    blush[pressed] = 1f;
                    holder = NoFinger;
                    return LoveTheBugEvent.Loved;
                }
            }
            else if (meterBug >= 0 && blush[meterBug] <= 0f) meterBug = -1;
            return LoveTheBugEvent.None;
        }

        /// <summary>True una volta sola, dopo la vittoria (il livello chiama Win solo allora).</summary>
        public bool TryComplete()
        {
            if (State != LoveTheBugState.Won || completed) return false;
            completed = true;
            return true;
        }

        void Release()
        {
            holder = NoFinger;
            pressed = -1;
            holdT = 0f;
            State = LoveTheBugState.Wandering;
        }

        /// <summary>Il nuovo insetto nasce accanto a quello tappato; i due partono in direzioni diverse.</summary>
        void SplitBug(int i)
        {
            int n = Count;
            splits++;
            Heading(n, splits * 2);
            Heading(i, splits * 2 + 1);
            float len = (float)System.Math.Sqrt(vx[n] * vx[n] + vy[n] * vy[n]);
            float ox = len > 0f ? vx[n] / len * SplitGap : SplitGap, oy = len > 0f ? vy[n] / len * SplitGap : 0f;
            bx[n] = Clamp(bx[i] + ox, minX, maxX);
            by[n] = Clamp(by[i] + oy, minY, maxY);
            blush[n] = 0f;
            Count = n + 1;
        }

        /// <summary>Direzione deterministica n. k (angolo d'oro: mai due uguali di fila, mai casuale).</summary>
        void Heading(int i, int k)
        {
            double a = (28.0 + 137.508 * k) * System.Math.PI / 180.0;
            vx[i] = (float)System.Math.Cos(a) * Speed;
            vy[i] = (float)System.Math.Sin(a) * Speed;
            // niente camminate quasi verticali: sembrerebbero ferme in orizzontale
            if (System.Math.Abs(vx[i]) < 0.2f * Speed) vx[i] = (vx[i] < 0f ? -0.2f : 0.2f) * Speed;
        }

        /// <summary>Camminata a rimbalzo dentro i limiti.</summary>
        void Walk(int i, float dt)
        {
            float x = bx[i] + vx[i] * dt, y = by[i] + vy[i] * dt;
            if (x < minX) { x = minX + (minX - x); vx[i] = System.Math.Abs(vx[i]); }
            else if (x > maxX) { x = maxX - (x - maxX); vx[i] = -System.Math.Abs(vx[i]); }
            if (y < minY) { y = minY + (minY - y); vy[i] = System.Math.Abs(vy[i]); }
            else if (y > maxY) { y = maxY - (y - maxY); vy[i] = -System.Math.Abs(vy[i]); }
            bx[i] = Clamp(x, minX, maxX);
            by[i] = Clamp(y, minY, maxY);
        }

        static float Clamp(float v, float lo, float hi) => v < lo ? lo : v > hi ? hi : v;
    }
}
