namespace Gotcha.Levels.V64
{
    /// <summary>Fasi del livello 271.</summary>
    public enum DrawerStage
    {
        Closed,     // il cassetto è "bloccato": si prova di tutto
        Opening,    // clic! il cassetto esce da solo (animazione): input bloccato
        Open,       // aperto: le chiavi aspettano di essere prese
        Taking,     // le chiavi sono state prese: il finale
        Done,
    }

    /// <summary>Tirare la maniglia (decorativa).</summary>
    public enum DrawerPull
    {
        None,       // input bloccato
        Budge,      // non si muove
        HandleOff,  // tirando forte la maniglia resta in mano: "Decorative. Of course."
        NoHandle,   // la maniglia non c'è più: due buchini
    }

    /// <summary>Gli attrezzi portati sul cassettone.</summary>
    public enum DrawerTool
    {
        Lube,       // lo spray lubrificante
        Screwdriver,// il cacciavite nella fessura
        Hairdryer,  // l'asciugacapelli ("il legno si gonfia con l'umidità")
    }

    /// <summary>Cosa fa un attrezzo.</summary>
    public enum DrawerToolResult
    {
        None,
        Squeak,      // lubrificante: cigolio, sempre bloccato (e ora lucido)
        Shinier,     // lubrificante di nuovo: ancora più lucido
        Bends,       // il cacciavite si piega: la fessura è troppo sottile
        AlreadyBent, // già piegato
        Blows,       // asciugacapelli: niente
    }

    /// <summary>Premere il frontale di un cassetto.</summary>
    public enum DrawerPush
    {
        None,       // input bloccato o cassetto inesistente
        Nudge,      // tocco leggero: il frontale cede un pelo verso dentro e torna (indizio)
        Click,      // spinta decisa sul cassetto giusto: clic, si apre da solo (la soluzione)
        OtherPops,  // spinta decisa su un altro cassetto: si apre da solo (vuoto) e si richiude
    }

    /// <summary>
    /// Logica pura del livello 271 "The Stuck Drawer" — "Open the drawer!" Un cassettone moderno con i frontali lisci;
    /// solo il cassetto in alto ha una maniglietta (decorativa) e dentro ci sono le chiavi della macchina.
    /// False piste (nessuna azzera i progressi, niente sconfitta): tirare la maniglia (non si muove; tirando forte resta
    /// in mano, si può rimettere), lo spray lubrificante (cigola, resta bloccato e lucido), il cacciavite nella fessura
    /// (si piega), l'asciugacapelli (niente), dare una botta al mobile (traballa: il cassetto in basso scatta fuori e
    /// rientra da solo, l'indizio), unghie sul frontale liscio.
    /// Indizio: il foglietto sul muro "No handles! Push to open :)", gli altri cassetti senza maniglie, il cassetto che
    /// scatta fuori alla botta, il tocco leggero che fa cedere il frontale verso dentro.
    /// Soluzione: premere (con decisione) il frontale del cassetto in alto: clic, esce da solo; poi prendere le chiavi.
    /// Nessuna dipendenza da Unity.
    /// </summary>
    public sealed class StuckDrawerLogic
    {
        /// <summary>Cassetti: 0 quello in alto (con la maniglia e le chiavi), 1 in mezzo, 2 in basso.</summary>
        public const int Target = 0, Drawers = 3;
        /// <summary>Quanto tempo (s) tenere premuto perché sia una spinta decisa.</summary>
        public const float FirmPress = 0.3f;
        /// <summary>Tirata (in unità di mondo) oltre la quale la maniglia si stacca.</summary>
        public const float HardPull = 0.9f;
        /// <summary>Dopo tante tirate deboli la successiva stacca comunque la maniglia.</summary>
        public const int PullsBeforeSnap = 3;

        public DrawerStage Stage { get; private set; } = DrawerStage.Closed;
        public bool HandleOn { get; private set; } = true;
        public bool Lubed { get; private set; }
        public bool ScrewdriverBent { get; private set; }
        public bool NoteRead { get; private set; }
        public bool LampOn { get; private set; }

        public int Pulls { get; private set; }
        /// <summary>Tirate deboli dall'ultima volta che la maniglia è stata (ri)messa.</summary>
        public int WeakPulls { get; private set; }
        public int HandleOffs { get; private set; }
        public int HandleBacks { get; private set; }
        public int Sprays { get; private set; }
        public int Pries { get; private set; }
        public int Blows { get; private set; }
        public int Bumps { get; private set; }
        public int Nudges { get; private set; }
        public int Scratches { get; private set; }
        public int OtherOpens { get; private set; }
        public int NoteReads { get; private set; }
        public int LampTaps { get; private set; }
        public int RugTaps { get; private set; }
        public bool CompletionReported { get; private set; }

        public bool InputLocked => Stage != DrawerStage.Closed;
        public bool DrawerOpen => Stage == DrawerStage.Open || Stage == DrawerStage.Taking || Stage == DrawerStage.Done;
        public bool KeysTaken => Stage == DrawerStage.Taking || Stage == DrawerStage.Done;
        /// <summary>Tutte le mosse sbagliate fatte finora (solo statistica: non cambiano niente).</summary>
        public int WrongMoves => Pulls + Sprays + Pries + Blows + Bumps + Nudges + Scratches + OtherOpens;

        public void Reset()
        {
            Stage = DrawerStage.Closed;
            HandleOn = true;
            Lubed = ScrewdriverBent = NoteRead = LampOn = false;
            Pulls = WeakPulls = HandleOffs = HandleBacks = Sprays = Pries = Blows = Bumps = Nudges = Scratches = 0;
            OtherOpens = NoteReads = LampTaps = RugTaps = 0;
            CompletionReported = false;
        }

        // ---------- La maniglia ----------

        /// <summary>Tirare la maniglia di quanto (unità di mondo): forte o alla quarta volta resta in mano.</summary>
        public DrawerPull PullHandle(float distance)
        {
            if (InputLocked) return DrawerPull.None;
            if (!HandleOn) return DrawerPull.NoHandle;
            Pulls++;
            if (distance >= HardPull || WeakPulls >= PullsBeforeSnap)
            {
                HandleOn = false;
                WeakPulls = 0;
                HandleOffs++;
                return DrawerPull.HandleOff;
            }
            WeakPulls++;
            return DrawerPull.Budge;
        }

        /// <summary>Rimettere la maniglia al suo posto. True se era staccata.</summary>
        public bool PutHandleBack()
        {
            if (HandleOn || Stage == DrawerStage.Done) return false;
            HandleOn = true;
            WeakPulls = 0;
            HandleBacks++;
            return true;
        }

        // ---------- I frontali ----------

        /// <summary>Premere il frontale del cassetto (0..2) per tanti secondi.</summary>
        public DrawerPush Press(int drawer, float seconds)
        {
            if (InputLocked || drawer < 0 || drawer >= Drawers) return DrawerPush.None;
            if (seconds < FirmPress) { Nudges++; return DrawerPush.Nudge; }
            if (drawer != Target) { OtherOpens++; return DrawerPush.OtherPops; }
            Stage = DrawerStage.Opening;
            return DrawerPush.Click;
        }

        /// <summary>Strisciare le dita sul frontale liscio (cercare di tirarlo senza maniglia). False se bloccato.</summary>
        public bool Scratch()
        {
            if (InputLocked) return false;
            Scratches++;
            return true;
        }

        /// <summary>Una botta al mobile (tocco sul fianco o scossone): traballa e un cassetto scatta fuori e rientra.</summary>
        public bool Bump()
        {
            if (InputLocked) return false;
            Bumps++;
            return true;
        }

        // ---------- Gli attrezzi ----------

        public DrawerToolResult UseTool(DrawerTool tool)
        {
            if (InputLocked) return DrawerToolResult.None;
            switch (tool)
            {
                case DrawerTool.Lube:
                    Sprays++;
                    if (Lubed) return DrawerToolResult.Shinier;
                    Lubed = true;
                    return DrawerToolResult.Squeak;
                case DrawerTool.Screwdriver:
                    Pries++;
                    if (ScrewdriverBent) return DrawerToolResult.AlreadyBent;
                    ScrewdriverBent = true;
                    return DrawerToolResult.Bends;
                default:
                    Blows++;
                    return DrawerToolResult.Blows;
            }
        }

        // ---------- Il resto della stanza ----------

        public void ReadNote()
        {
            if (InputLocked) return;
            NoteRead = true;
            NoteReads++;
        }

        /// <summary>Accende/spegne la lampada; restituisce lo stato nuovo.</summary>
        public bool TapLamp()
        {
            if (InputLocked) return LampOn;
            LampTaps++;
            LampOn = !LampOn;
            return LampOn;
        }

        public bool TapRug()
        {
            if (InputLocked) return false;
            RugTaps++;
            return true;
        }

        // ---------- Il finale ----------

        /// <summary>Fine dell'animazione di apertura (o ripresa dopo una pausa). True se il cassetto era in apertura.</summary>
        public bool FinishOpening()
        {
            if (Stage != DrawerStage.Opening) return false;
            Stage = DrawerStage.Open;
            return true;
        }

        /// <summary>Prendere le chiavi dal cassetto aperto. True la prima volta.</summary>
        public bool TakeKeys()
        {
            if (Stage != DrawerStage.Open) return false;
            Stage = DrawerStage.Taking;
            return true;
        }

        /// <summary>Fine del finale: vittoria, una volta sola.</summary>
        public bool TryComplete()
        {
            if (Stage != DrawerStage.Taking || CompletionReported) return false;
            CompletionReported = true;
            Stage = DrawerStage.Done;
            return true;
        }
    }
}
