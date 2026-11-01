namespace Gotcha.Levels.V64
{
    /// <summary>Stato del pulsante timido (livello 2 della roadmap, "The Shy Button").</summary>
    public enum ShyButtonState
    {
        Free,       // fermo, aspetta: un dito che lo tocca diventa il "dito che tiene"
        Holding,    // un dito è appoggiato sopra da meno di PinTime secondi (se si alza prima: scappa)
        Pinned,     // un dito lo tiene da almeno PinTime: è bloccato, trema e suda ("Uh oh.")
        Dodging,    // sta scappando verso il nuovo posto (animazione breve): i tocchi sono ignorati
        Won,        // un secondo dito l'ha toccato mentre era bloccato: START (festa, input chiuso)
    }

    /// <summary>Cosa è successo dopo un input o un passo di tempo.</summary>
    public enum ShyButtonEvent
    {
        None,       // niente da mostrare
        Ignored,    // tocco durante la fuga o dopo la vittoria
        Flinch,     // tocco vicino ma fuori dal pulsante: sobbalza, non scappa
        Grabbed,    // un dito si è appoggiato sopra: comincia a contare il tempo per bloccarlo
        Pinned,     // il dito l'ha tenuto abbastanza: è bloccato
        Dodged,     // è scappato (tocco veloce, dito scivolato via o dito che lo bloccava alzato)
        Landed,     // fine della fuga: di nuovo fermo (Free)
        Missed,     // secondo dito mentre è bloccato, ma troppo lontano: non conta
        Started,    // secondo dito sopra (o appena fuori) mentre è bloccato: VITTORIA (una volta sola)
        Nudge,      // bloccato da un po' senza secondo dito: una battutina-indizio (una volta per blocco)
    }

    /// <summary>
    /// Logica pura del livello "The Shy Button" (id shy_button, roadmap n. 2) — "PRESS START." Un grosso pulsante verde
    /// START con gli occhi, nervoso. Un tocco veloce lo fa scappare (alla risalita del dito, mai all'istante) in un altro
    /// punto dentro i limiti dello schermo; dopo 3 fughe suda, dopo 5 c'è una frase-indizio.
    /// Soluzione esatta: tenere UN dito sul pulsante per almeno <see cref="PinTime"/> s (si blocca: "Uh oh.") e, senza
    /// alzarlo, toccarlo con un SECONDO dito (id diverso; vale anche appena fuori dal bordo, raggio generoso
    /// <see cref="SecondRadius"/>) -> START. Alzare (o far scivolare via) il dito che lo blocca prima del secondo tocco ->
    /// scappa di nuovo. Un dito solo non può mai vincere: la vittoria chiede un secondo id mentre il primo è ancora giù.
    /// Nessuna sconfitta, nessuno stato senza uscita: Cancel() (pausa, perdita del fuoco) torna sempre a Free sul posto.
    /// Coordinate del mondo; nessuna dipendenza da Unity. Ordine degli eventi nello stesso fotogramma: il livello passa
    /// prima i Down e poi gli Up (scelta generosa: tocco del secondo dito e risalita del primo insieme = vittoria).
    /// </summary>
    public sealed class ShyButtonLogic
    {
        public const float PinTime = 0.25f;          // quanto va tenuto il primo dito per bloccarlo
        public const float DodgeTime = 0.28f;        // durata della fuga (animazione): i tocchi in questo tempo non contano
        public const float GrabRadius = 1.25f;        // un dito che scende entro questo raggio dal centro "lo tocca"
        public const float FlinchRadius = 1.95f;      // vicino ma fuori: sobbalza soltanto
        public const float SecondRadius = 1.75f;      // il secondo dito vale fino a qui (generoso, anche appena fuori)
        public const float SlipRadius = 1.6f;        // il dito che tiene può muoversi fin qui senza "scivolare via"
        public const float MinJump = 1.6f;           // la fuga lo porta almeno a questa distanza dal posto di prima
        public const float NudgeTime = 3.0f;         // bloccato da tanto senza secondo dito: battutina
        public const int SweatAfter = 3;             // fughe dopo cui suda
        public const int HintAfter = 5;              // fughe dopo cui dice la frase-indizio (una volta)

        readonly float minX, maxX, minY, maxY, startX, startY;
        readonly int seed;
        System.Random rnd;

        int holder = NoFinger;
        float holdT, dodgeT, pinT;
        bool nudged, completed, hintSaid;

        public const int NoFinger = int.MinValue;

        /// <summary>Limiti del CENTRO del pulsante (già ristretti della sua mezza misura) e posto di partenza.</summary>
        public ShyButtonLogic(float minX, float maxX, float minY, float maxY, float startX, float startY, int seed = 76)
        {
            this.minX = System.Math.Min(minX, maxX);
            this.maxX = System.Math.Max(minX, maxX);
            this.minY = System.Math.Min(minY, maxY);
            this.maxY = System.Math.Max(minY, maxY);
            this.startX = Clamp(startX, this.minX, this.maxX);
            this.startY = Clamp(startY, this.minY, this.maxY);
            this.seed = seed;
            Reset();
        }

        public ShyButtonState State { get; private set; }
        public float X { get; private set; }
        public float Y { get; private set; }
        /// <summary>Da dove è partita l'ultima fuga (per l'animazione).</summary>
        public float FromX { get; private set; }
        public float FromY { get; private set; }
        public int Dodges { get; private set; }
        /// <summary>Dito che lo tiene (Holding/Pinned), altrimenti <see cref="NoFinger"/>.</summary>
        public int Holder => holder;
        /// <summary>0 nervoso, 1 suda (dopo 3 fughe), 2 nel panico (dopo 5).</summary>
        public int Nervous => Dodges >= HintAfter ? 2 : Dodges >= SweatAfter ? 1 : 0;
        public bool Sweating => Dodges >= SweatAfter || State == ShyButtonState.Pinned;
        /// <summary>Avanzamento della fuga 0..1 (1 fuori dalla fuga).</summary>
        public float DodgeProgress => State == ShyButtonState.Dodging ? Clamp(dodgeT / DodgeTime, 0f, 1f) : 1f;
        public bool InputLocked => State == ShyButtonState.Won;
        public float MinX => minX;
        public float MaxX => maxX;
        public float MinY => minY;
        public float MaxY => maxY;

        /// <summary>Si ricomincia da capo (rientro nel livello): stesso posto, stessa sequenza di fughe.</summary>
        public void Reset()
        {
            rnd = new System.Random(seed);
            State = ShyButtonState.Free;
            X = FromX = startX;
            Y = FromY = startY;
            Dodges = 0;
            holder = NoFinger;
            holdT = dodgeT = pinT = 0f;
            nudged = completed = hintSaid = false;
        }

        /// <summary>
        /// La frase-indizio dopo <see cref="HintAfter"/> fughe: true una volta sola (da chiamare dopo un Dodged).
        /// </summary>
        public bool TakeHint()
        {
            if (hintSaid || Dodges < HintAfter) return false;
            hintSaid = true;
            return true;
        }

        public bool Inside(float x, float y, float radius)
        {
            float dx = x - X, dy = y - Y;
            return dx * dx + dy * dy <= radius * radius;
        }

        /// <summary>Un dito scende in (x, y).</summary>
        public ShyButtonEvent Down(int finger, float x, float y)
        {
            switch (State)
            {
                case ShyButtonState.Won:
                case ShyButtonState.Dodging:
                    return ShyButtonEvent.Ignored;
                case ShyButtonState.Pinned:
                    if (finger == holder) return ShyButtonEvent.None;
                    if (!Inside(x, y, SecondRadius)) return ShyButtonEvent.Missed;
                    State = ShyButtonState.Won;
                    return ShyButtonEvent.Started;
                case ShyButtonState.Holding:
                    // secondo dito prima che sia bloccato: non conta, ma non lo fa nemmeno scappare
                    return ShyButtonEvent.None;
                default:
                    if (Inside(x, y, GrabRadius))
                    {
                        holder = finger;
                        holdT = 0f;
                        State = ShyButtonState.Holding;
                        return ShyButtonEvent.Grabbed;
                    }
                    return Inside(x, y, FlinchRadius) ? ShyButtonEvent.Flinch : ShyButtonEvent.None;
            }
        }

        /// <summary>Un dito già giù si sposta: se è quello che tiene e scivola via, il pulsante scappa.</summary>
        public ShyButtonEvent Move(int finger, float x, float y)
        {
            if ((State != ShyButtonState.Holding && State != ShyButtonState.Pinned) || finger != holder) return ShyButtonEvent.None;
            if (Inside(x, y, SlipRadius)) return ShyButtonEvent.None;
            return Escape(x, y);
        }

        /// <summary>Un dito si alza. Se è quello che tiene (bloccato o no), il pulsante scappa.</summary>
        public ShyButtonEvent Up(int finger)
        {
            if ((State != ShyButtonState.Holding && State != ShyButtonState.Pinned) || finger != holder) return ShyButtonEvent.None;
            return Escape(X, Y);
        }

        /// <summary>Tutte le dita spariscono (pausa, fuoco perso, livello spento): fermo dov'è, nessuna fuga contata.</summary>
        public void Cancel()
        {
            holder = NoFinger;
            holdT = pinT = 0f;
            if (State == ShyButtonState.Holding || State == ShyButtonState.Pinned) State = ShyButtonState.Free;
            else if (State == ShyButtonState.Dodging) { State = ShyButtonState.Free; dodgeT = 0f; }
        }

        /// <summary>Passo di tempo.</summary>
        public ShyButtonEvent Tick(float dt)
        {
            if (dt < 0f) dt = 0f;
            switch (State)
            {
                case ShyButtonState.Holding:
                    holdT += dt;
                    if (holdT >= PinTime)
                    {
                        State = ShyButtonState.Pinned;
                        pinT = 0f;
                        nudged = false;
                        return ShyButtonEvent.Pinned;
                    }
                    return ShyButtonEvent.None;
                case ShyButtonState.Pinned:
                    pinT += dt;
                    if (!nudged && pinT >= NudgeTime) { nudged = true; return ShyButtonEvent.Nudge; }
                    return ShyButtonEvent.None;
                case ShyButtonState.Dodging:
                    dodgeT += dt;
                    if (dodgeT >= DodgeTime) { State = ShyButtonState.Free; return ShyButtonEvent.Landed; }
                    return ShyButtonEvent.None;
                default:
                    return ShyButtonEvent.None;
            }
        }

        /// <summary>True una volta sola, dopo la vittoria (il livello chiama Win solo allora).</summary>
        public bool TryComplete()
        {
            if (State != ShyButtonState.Won || completed) return false;
            completed = true;
            return true;
        }

        ShyButtonEvent Escape(float awayX, float awayY)
        {
            holder = NoFinger;
            holdT = pinT = 0f;
            FromX = X;
            FromY = Y;
            PickSpot(awayX, awayY, out float nx, out float ny);
            X = nx;
            Y = ny;
            Dodges++;
            dodgeT = 0f;
            State = ShyButtonState.Dodging;
            return ShyButtonEvent.Dodged;
        }

        /// <summary>
        /// Nuovo posto: dentro i limiti, lontano almeno MinJump dal posto di prima e dal dito (se possibile); altrimenti il
        /// candidato più lontano. Sempre ristretto ai limiti.
        /// </summary>
        void PickSpot(float fingerX, float fingerY, out float bx, out float by)
        {
            bx = X;
            by = Y;
            float best = -1f;
            for (int i = 0; i < 16; i++)
            {
                float cx = minX + (float)rnd.NextDouble() * (maxX - minX);
                float cy = minY + (float)rnd.NextDouble() * (maxY - minY);
                float d0 = Dist(cx, cy, X, Y), d1 = Dist(cx, cy, fingerX, fingerY);
                float score = System.Math.Min(d0, d1);
                if (score >= MinJump) { bx = cx; by = cy; best = score; break; }
                if (score > best) { best = score; bx = cx; by = cy; }
            }
            bx = Clamp(bx, minX, maxX);
            by = Clamp(by, minY, maxY);
        }

        static float Dist(float ax, float ay, float bx, float by)
        {
            float dx = ax - bx, dy = ay - by;
            return (float)System.Math.Sqrt(dx * dx + dy * dy);
        }

        static float Clamp(float v, float lo, float hi) => v < lo ? lo : v > hi ? hi : v;
    }
}
