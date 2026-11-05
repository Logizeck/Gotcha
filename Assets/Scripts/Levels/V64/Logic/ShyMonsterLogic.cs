namespace Gotcha.Levels.V64
{
    /// <summary>Fasi del livello "The Shy Monster".</summary>
    public enum ShyMonsterStage
    {
        Playing,      // si prova di tutto
        Celebrating,  // la medaglia è appuntata sul petto: il mostro arrossisce e scappa (input bloccato)
        Done,         // vittoria già data
    }

    /// <summary>Gli oggetti sul pavimento che si possono trascinare.</summary>
    public enum ShyMonsterItem
    {
        None,
        Medal,      // la coccarda "BEST MONSTER" (la soluzione)
        Sword,      // spadina (falsa pista)
        Slingshot,  // fionda (falsa pista)
    }

    /// <summary>Un colpo al mostro (tocco o colpetto col dito).</summary>
    public enum ShyMonsterHit
    {
        None,    // input bloccato o il dito tiene già qualcosa
        Grew,    // il mostro cresce di un gradino (ringhio, sopracciglia più scure)
        Maxed,   // è già al massimo: ruggisce ma non cresce più (resta nello schermo)
    }

    /// <summary>Un oggetto lasciato andare.</summary>
    public enum ShyMonsterDrop
    {
        None,      // input bloccato o oggetto non tenuto
        Floor,     // lasciato lontano dal mostro: torna sul pavimento
        Ignored,   // medaglia lasciata sul mostro ma non sul petto (testa, braccia...): "It didn't notice." e torna giù
        Tickled,   // arma lasciata sul mostro: ride ("Ha! Tickles.") e cresce di un gradino; l'arma torna giù
        Pinned,    // medaglia lasciata sul PETTO: VITTORIA (una volta sola)
    }

    /// <summary>
    /// Logica pura del livello 19 della roadmap "The Shy Monster" (id shy_monster) — "DEFEAT THE MONSTER." Un mostro
    /// enorme e apparentemente invincibile (corna, tanti denti, ma buffo) riempie la parte alta; sul pavimento c'è una
    /// coccarda abbandonata "BEST MONSTER" e, come false piste, una spadina e una fionda.
    /// Inganno: colpire il mostro (tocco o colpetto) lo fa CRESCERE di un gradino (<see cref="GrowthPerStep"/>, al
    /// massimo <see cref="MaxGrowth"/> gradini: resta dentro lo schermo); dopo <see cref="QuietSeconds"/> di calma torna
    /// piano piano normale (un gradino ogni <see cref="ShrinkStepSeconds"/>): tutto reversibile. Le armi lasciate sul
    /// mostro lo fanno solo ridere e crescere.
    /// Soluzione esatta: trascinare la coccarda sul PETTO del mostro e LASCIARLA lì (conta il rilascio dentro la zona del
    /// petto, <see cref="InChest"/>, calcolata dalla scala attuale del mostro: vale anche quando è cresciuto). Passarci
    /// sopra e lasciarla altrove non conta; lasciata su testa/braccia -> "It didn't notice." e torna sul pavimento.
    /// Un dito alla volta; Cancel() (pausa, app in background, secondo dito) rimette a terra quello che si teneva.
    /// Non si può perdere. Nessuna dipendenza da Unity.
    /// </summary>
    public sealed class ShyMonsterLogic
    {
        /// <summary>Gradini di crescita massimi (il mostro resta nello schermo).</summary>
        public const int MaxGrowth = 4;
        /// <summary>Quanto cresce la scala a ogni gradino (1 = normale).</summary>
        public const float GrowthPerStep = 0.08f;
        /// <summary>Secondi di calma (nessun colpo) prima che il mostro cominci a sgonfiarsi.</summary>
        public const float QuietSeconds = 2.5f;
        /// <summary>Secondi fra un gradino e l'altro mentre si sgonfia.</summary>
        public const float ShrinkStepSeconds = 0.9f;

        // zona del PETTO in coordinate locali del mostro (origine ai piedi, scala 1): sotto la faccia, sopra la pancia
        /// <summary>Centro del petto (sopra i piedi, scala 1).</summary>
        public const float ChestX = 0f, ChestY = 1.78f;
        /// <summary>Mezze misure della zona del petto (scala 1): generosa ma separata dalla testa (che parte da y 2.45).</summary>
        public const float ChestHalfW = 0.9f, ChestHalfH = 0.6f;

        public ShyMonsterStage Stage { get; private set; } = ShyMonsterStage.Playing;
        /// <summary>Gradini di crescita attuali (0 = normale, <see cref="MaxGrowth"/> = al massimo).</summary>
        public int Growth { get; private set; }
        /// <summary>Scala voluta del mostro (1 = normale).</summary>
        public float Scale => 1f + Growth * GrowthPerStep;
        public ShyMonsterItem Held { get; private set; } = ShyMonsterItem.None;
        public bool Holding => Held != ShyMonsterItem.None;
        public bool InputLocked => Stage != ShyMonsterStage.Playing;

        public int Hits { get; private set; }
        public int Tickles { get; private set; }
        public int Ignored { get; private set; }
        public int FloorDrops { get; private set; }
        public int MedalTaps { get; private set; }
        public int DecoyTaps { get; private set; }
        public int WrongMoves => Hits + Tickles + Ignored + FloorDrops;

        float quietT;
        bool completed;

        public void Reset()
        {
            Stage = ShyMonsterStage.Playing;
            Growth = 0;
            Held = ShyMonsterItem.None;
            Hits = Tickles = Ignored = FloorDrops = MedalTaps = DecoyTaps = 0;
            quietT = 0f;
            completed = false;
        }

        /// <summary>Il tempo passa: dopo un po' di calma il mostro si sgonfia di un gradino. True se ha appena perso un gradino.</summary>
        public bool Tick(float dt)
        {
            if (dt <= 0f || InputLocked || Growth <= 0) { if (Growth <= 0) quietT = 0f; return false; }
            quietT += dt;
            if (quietT < QuietSeconds) return false;
            Growth--;
            quietT = QuietSeconds - ShrinkStepSeconds;   // il prossimo gradino fra ShrinkStepSeconds
            if (Growth <= 0) quietT = 0f;
            return true;
        }

        /// <summary>Il mostro si arrabbia: un gradino in più (fino al massimo) e la calma riparte da zero.</summary>
        ShyMonsterHit Grow()
        {
            quietT = 0f;
            if (Growth >= MaxGrowth) return ShyMonsterHit.Maxed;
            Growth++;
            return ShyMonsterHit.Grew;
        }

        // ---------- Le false piste ----------

        /// <summary>Tocco o colpetto sul mostro: cresce (mai una vittoria).</summary>
        public ShyMonsterHit Hit()
        {
            if (InputLocked || Holding) return ShyMonsterHit.None;
            Hits++;
            return Grow();
        }

        /// <summary>Tocco (senza trascinare) su un oggetto del pavimento.</summary>
        public bool TapItem(ShyMonsterItem item)
        {
            if (InputLocked || Holding || item == ShyMonsterItem.None) return false;
            if (item == ShyMonsterItem.Medal) MedalTaps++;
            else DecoyTaps++;
            return true;
        }

        // ---------- Trascinare ----------

        /// <summary>Il dito comincia a trascinare un oggetto del pavimento.</summary>
        public bool Grab(ShyMonsterItem item)
        {
            if (InputLocked || Holding || item == ShyMonsterItem.None) return false;
            Held = item;
            return true;
        }

        /// <summary>
        /// L'oggetto tenuto viene lasciato. <paramref name="inChest"/>: il punto di rilascio è nella zona del petto;
        /// <paramref name="onMonster"/>: è sopra il mostro (testa, braccia, corpo). La medaglia vince SOLO sul petto.
        /// </summary>
        public ShyMonsterDrop Release(bool inChest, bool onMonster)
        {
            if (InputLocked || !Holding) return ShyMonsterDrop.None;
            ShyMonsterItem item = Held;
            Held = ShyMonsterItem.None;
            if (item == ShyMonsterItem.Medal)
            {
                if (inChest)
                {
                    Stage = ShyMonsterStage.Celebrating;
                    quietT = 0f;
                    return ShyMonsterDrop.Pinned;
                }
                if (onMonster) { Ignored++; return ShyMonsterDrop.Ignored; }
                FloorDrops++;
                return ShyMonsterDrop.Floor;
            }
            if (inChest || onMonster)
            {
                Tickles++;
                Grow();
                return ShyMonsterDrop.Tickled;
            }
            FloorDrops++;
            return ShyMonsterDrop.Floor;
        }

        /// <summary>Dito perso (pausa, app in background, secondo dito, livello spento): l'oggetto torna sul pavimento.</summary>
        public bool Cancel()
        {
            if (!Holding || InputLocked) return false;
            Held = ShyMonsterItem.None;
            return true;
        }

        /// <summary>Fine della festa: true una volta sola (allora il livello chiama Win).</summary>
        public bool TryComplete()
        {
            if (Stage != ShyMonsterStage.Celebrating || completed) return false;
            completed = true;
            Stage = ShyMonsterStage.Done;
            return true;
        }

        // ---------- Zone (dai limiti degli oggetti, mai coordinate dello schermo) ----------

        /// <summary>
        /// Punto (px, py) nella zona del petto di un mostro coi piedi in (feetX, feetY) e scala <paramref name="scale"/>
        /// (la zona cresce e sale insieme al mostro).
        /// </summary>
        public static bool InChest(float px, float py, float feetX, float feetY, float scale)
        {
            float s = Abs(scale);
            return InZone(px, py, feetX + ChestX * s, feetY + ChestY * s, ChestHalfW * s, ChestHalfH * s, 0f);
        }

        /// <summary>Punto (px, py) dentro il rettangolo (centro cx, cy; mezze misure halfW, halfH) allargato di margin per lato.</summary>
        public static bool InZone(float px, float py, float cx, float cy, float halfW, float halfH, float margin)
        {
            float m = margin > 0f ? margin : 0f;
            return Abs(px - cx) <= Abs(halfW) + m && Abs(py - cy) <= Abs(halfH) + m;
        }

        static float Abs(float v) => v < 0f ? -v : v;
    }
}
