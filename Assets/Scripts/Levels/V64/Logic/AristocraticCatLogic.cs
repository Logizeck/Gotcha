namespace Gotcha.Levels.V64
{
    /// <summary>Stato del livello "The Aristocratic Cat" (roadmap n. 13).</summary>
    public enum AristocraticCatState
    {
        Perched,    // il gatto sta sdraiato sull'armadio (anche se il telefono è un po' inclinato)
        Clinging,   // telefono capovolto (e "armato"): il gatto si aggrappa, il tempo di tenuta scorre
        Fallen,     // tenuto HoldTime: scivola giù (festa, input chiuso)
    }

    /// <summary>Gli oggetti che si possono offrire al gatto (trascinandoli).</summary>
    public enum AristocraticCatItem { Ladder, Bowl, Toy }

    /// <summary>Cosa è successo dopo un input o un passo di tempo.</summary>
    public enum AristocraticCatEvent
    {
        None,         // niente da mostrare
        Ignored,      // input dopo la caduta
        Uneasy,       // il telefono si inclina molto (oltre metà strada verso il capovolto): "Hey!" (una volta per inclinazione)
        Clinging,     // capovolto davvero: si aggrappa con le unghie ("Hey! HEY!")
        ClimbedBack,  // raddrizzato prima di HoldTime: risale composto (annullato, reversibile)
        Slid,         // capovolto per HoldTime: VITTORIA (una volta sola)
        Poked,        // tocco sul gatto: "Do you mind?"
        LadderLeaned, // scala appoggiata all'armadio: "A LADDER? How common." (resta lì finché non la si sposta)
        BowlRefused,  // ciotola portata su verso il gatto: "I dine at eight." (torna a terra)
        ToyRefused,   // giocattolo offerto: occhiata annoiata, "Please." (torna a terra)
        Dropped,      // oggetto lasciato altrove: torna al suo posto, nessun commento
    }

    /// <summary>
    /// Logica pura del livello "The Aristocratic Cat" (id aristocratic_cat, roadmap n. 13) — "GET THE CAT DOWN." Un
    /// gatto snob (monocolo, farfallino) è sdraiato in cima a un armadio altissimo. Scala, ciotola e giocattolo non lo
    /// convincono (tutto reversibile, mai una sconfitta), toccarlo lo infastidisce. Soluzione esatta: capovolgere il
    /// telefono e tenerlo capovolto per <see cref="HoldTime"/> s: il gatto scivola giù dall'armadio.
    /// Gravità: accelerometro nello schermo (gioco bloccato in verticale: dritto y ≈ -1, capovolto y ≈ +1), filtrata
    /// passa-basso (<see cref="V64AGravity"/>, riusata), "capovolto" con isteresi (sopra <see cref="InvertOn"/>, finisce
    /// sotto <see cref="InvertOff"/>; con filtro e tenuta equivale a un telefono girato di circa 150-180°). Un telefono GIÀ capovolto all'avvio non
    /// vale: serve prima averlo visto non capovolto (y filtrata sotto <see cref="ArmBelow"/>), poi una transizione nuova.
    /// Raddrizzare prima di HoldTime annulla del tutto (il tempo riparte da zero). Cancel() (pausa, perdita del fuoco)
    /// torna sempre a Perched senza contare niente. Nessuna dipendenza da Unity.
    /// </summary>
    public sealed class AristocraticCatLogic
    {
        public const float InvertOn = 0.6f;     // y filtrata oltre cui il telefono è "capovolto" (oltre ~127° di rotazione; con filtro e tenuta: in pratica 150-180°)
        public const float InvertOff = 0.35f;   // isteresi: smette di essere capovolto sotto questa
        public const float ArmBelow = 0.2f;     // il telefono è stato visto "non capovolto" (si arma) sotto questa
        public const float LeanFrom = -0.2f;    // da qui il gatto comincia a sentire l'inclinazione (aggrappo visivo 0..1)
        public const float UneasyAt = 0.5f;     // inclinazione visiva oltre cui dice "Hey!"
        public const float UneasyReset = 0.15f; // sotto questa il "Hey!" si riarma
        public const float HoldTime = 0.7f;     // tenuta capovolta che vince
        public const float Smoothing = 6f;      // velocità del filtro passa-basso (1/s)
        public const int HintAfter = 5;         // tentativi sbagliati dopo cui arriva la frase sottile
        const float TimeEps = 1e-4f;

        readonly V64AGravity gravity = new V64AGravity();
        readonly float zoneMinX, zoneMaxX, zoneMinY, ladderMinX;
        float holdT;
        bool armed, inverted, uneasySaid, completed, hintSaid;

        /// <summary>
        /// Zona "verso il gatto" (coordinate del mondo): x in [zoneMinX, zoneMaxX] e y ≥ zoneMinY (ciotola e giocattolo
        /// offerti lì). La scala si appoggia all'armadio se lasciata con il centro a x ≥ ladderMinX (o nella zona).
        /// </summary>
        public AristocraticCatLogic(float zoneMinX, float zoneMaxX, float zoneMinY, float ladderMinX)
        {
            this.zoneMinX = System.Math.Min(zoneMinX, zoneMaxX);
            this.zoneMaxX = System.Math.Max(zoneMinX, zoneMaxX);
            this.zoneMinY = zoneMinY;
            this.ladderMinX = ladderMinX;
            gravity.Smoothing = Smoothing;
            Reset();
        }

        public AristocraticCatState State { get; private set; }
        /// <summary>Il telefono è stato visto non capovolto: da ora un capovolgimento conta.</summary>
        public bool Armed => armed;
        /// <summary>Capovolto (con isteresi), indipendentemente dall'armamento.</summary>
        public bool Inverted => inverted;
        /// <summary>Componente y della gravità filtrata (-1 dritto, +1 capovolto).</summary>
        public float GravityY => gravity.Y;
        public float GravityX => gravity.X;
        /// <summary>Aggrappo visivo 0..1 (0 se non armato: un telefono già capovolto all'avvio non mostra niente).</summary>
        public float Lean => !armed || State == AristocraticCatState.Fallen ? 0f : Clamp01((gravity.Y - LeanFrom) / (InvertOn - LeanFrom));
        /// <summary>Avanzamento della tenuta 0..1 (1 dopo la caduta).</summary>
        public float HoldProgress => State == AristocraticCatState.Fallen ? 1f : State == AristocraticCatState.Clinging ? Clamp01(holdT / HoldTime) : 0f;
        public float HeldSeconds => State == AristocraticCatState.Clinging ? holdT : 0f;
        public bool LadderLeaned { get; private set; }
        public int Pokes { get; private set; }
        public int LadderOffers { get; private set; }
        public int BowlOffers { get; private set; }
        public int ToyOffers { get; private set; }
        /// <summary>Raddrizzamenti prima del tempo (per variare le battute).</summary>
        public int ClimbBacks { get; private set; }
        /// <summary>Tentativi sbagliati (tocchi e offerte).</summary>
        public int WrongMoves { get; private set; }
        public bool InputLocked => State == AristocraticCatState.Fallen;

        /// <summary>Si ricomincia da capo (rientro nel livello): gatto sull'armadio, oggetti a casa, non armato.</summary>
        public void Reset()
        {
            State = AristocraticCatState.Perched;
            gravity.Reset();
            holdT = 0f;
            armed = inverted = uneasySaid = completed = hintSaid = false;
            LadderLeaned = false;
            Pokes = LadderOffers = BowlOffers = ToyOffers = ClimbBacks = WrongMoves = 0;
        }

        /// <summary>Pausa, app in sottofondo, perdita del fuoco: annulla la tenuta, il filtro riparte dalla prossima lettura.</summary>
        public void Cancel()
        {
            if (State == AristocraticCatState.Fallen) return;
            State = AristocraticCatState.Perched;
            holdT = 0f;
            inverted = uneasySaid = false;
            gravity.Reset();   // la prossima lettura riempie subito il filtro (niente salti "finti")
        }

        /// <summary>True se (x, y) è "verso il gatto" (in cima all'armadio o lì vicino).</summary>
        public bool InCatZone(float x, float y) => x >= zoneMinX && x <= zoneMaxX && y >= zoneMinY;

        /// <summary>Passo di tempo con la lettura dell'accelerometro (già nel sistema dello schermo, in g).</summary>
        public AristocraticCatEvent Tick(float dt, float ax, float ay)
        {
            if (State == AristocraticCatState.Fallen) return AristocraticCatEvent.None;
            if (dt < 0f || float.IsNaN(dt) || float.IsInfinity(dt)) dt = 0f;
            gravity.Feed(ax, ay, dt);
            float y = gravity.Y;
            if (!armed && y < ArmBelow) armed = true;
            if (!inverted && y > InvertOn) inverted = true;
            else if (inverted && y < InvertOff) inverted = false;

            if (State == AristocraticCatState.Clinging)
            {
                if (!inverted)
                {
                    State = AristocraticCatState.Perched;
                    holdT = 0f;
                    ClimbBacks++;
                    if (Lean < UneasyReset) uneasySaid = false;
                    return AristocraticCatEvent.ClimbedBack;
                }
                holdT += dt;
                if (holdT >= HoldTime - TimeEps)
                {
                    State = AristocraticCatState.Fallen;
                    return AristocraticCatEvent.Slid;
                }
                return AristocraticCatEvent.None;
            }

            // Perched
            if (inverted && armed)
            {
                State = AristocraticCatState.Clinging;
                holdT = 0f;
                uneasySaid = true;
                return AristocraticCatEvent.Clinging;
            }
            float lean = Lean;
            if (uneasySaid && lean < UneasyReset) uneasySaid = false;
            if (!uneasySaid && armed && lean >= UneasyAt)
            {
                uneasySaid = true;
                return AristocraticCatEvent.Uneasy;
            }
            return AristocraticCatEvent.None;
        }

        /// <summary>Un oggetto viene preso in mano: la scala appoggiata si stacca dall'armadio. False dopo la caduta.</summary>
        public bool Pick(AristocraticCatItem item)
        {
            if (State == AristocraticCatState.Fallen) return false;
            if (item == AristocraticCatItem.Ladder) LadderLeaned = false;
            return true;
        }

        /// <summary>Un oggetto viene lasciato in (x, y) (posizione dell'oggetto o del dito, la più generosa la sceglie il gioco).</summary>
        public AristocraticCatEvent Drop(AristocraticCatItem item, float x, float y)
        {
            if (State == AristocraticCatState.Fallen) return AristocraticCatEvent.Ignored;
            switch (item)
            {
                case AristocraticCatItem.Ladder:
                    if (x >= ladderMinX || InCatZone(x, y))
                    {
                        LadderLeaned = true;
                        LadderOffers++;
                        WrongMoves++;
                        return AristocraticCatEvent.LadderLeaned;
                    }
                    LadderLeaned = false;
                    return AristocraticCatEvent.Dropped;
                case AristocraticCatItem.Bowl:
                    if (!InCatZone(x, y)) return AristocraticCatEvent.Dropped;
                    BowlOffers++;
                    WrongMoves++;
                    return AristocraticCatEvent.BowlRefused;
                default:
                    if (!InCatZone(x, y)) return AristocraticCatEvent.Dropped;
                    ToyOffers++;
                    WrongMoves++;
                    return AristocraticCatEvent.ToyRefused;
            }
        }

        /// <summary>Tocco sul gatto.</summary>
        public AristocraticCatEvent Poke()
        {
            if (State == AristocraticCatState.Fallen) return AristocraticCatEvent.Ignored;
            Pokes++;
            WrongMoves++;
            return AristocraticCatEvent.Poked;
        }

        /// <summary>La frase sottile dopo <see cref="HintAfter"/> tentativi sbagliati: true una volta sola.</summary>
        public bool TakeHint()
        {
            if (hintSaid || WrongMoves < HintAfter || State == AristocraticCatState.Fallen) return false;
            hintSaid = true;
            return true;
        }

        /// <summary>Vittoria da registrare: true una volta sola, dopo la caduta.</summary>
        public bool TryComplete()
        {
            if (State != AristocraticCatState.Fallen || completed) return false;
            completed = true;
            return true;
        }

        static float Clamp01(float v) => v < 0f ? 0f : v > 1f ? 1f : v;
    }
}
