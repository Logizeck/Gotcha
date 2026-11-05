namespace Gotcha.Levels.V64
{
    /// <summary>Fasi del livello "The Invisible Treasure".</summary>
    public enum InvisibleTreasureStage
    {
        Playing,      // si prova di tutto
        Celebrating,  // si è scavato sulla X spostata a terra: il baule esce e si apre (input bloccato)
        Done,         // vittoria già data
    }

    /// <summary>Le cose che si possono toccare (senza trascinare).</summary>
    public enum InvisibleTreasureSpot
    {
        Map,       // la mappa (fuori dalla X)
        MapX,      // la X disegnata sulla mappa, toccata e basta
        GroundX,   // la X già spostata sulla sabbia, toccata e basta
        Shovel,    // la pala toccata e basta: "Drag me where you want to dig."
        Palm,      // la palma
        Sand,      // la sabbia a mani nude
        Sea,       // mare e cielo
    }

    /// <summary>Dove è finita la X lasciata andare.</summary>
    public enum InvisibleTreasureDrop
    {
        None,       // input bloccato o X non tenuta
        OnMap,      // lasciata sopra la mappa: torna al suo posto sulla mappa
        OffIsland,  // lasciata in mare, in cielo o fuori dall'isola: torna sulla mappa
        OnSand,     // lasciata sulla sabbia: resta lì (si può spostare ancora)
    }

    /// <summary>Cosa trova la pala lasciata andare.</summary>
    public enum InvisibleTreasureDig
    {
        None,      // input bloccato o pala non tenuta
        NotSand,   // la punta non è sulla sabbia (mare, cielo, mappa): la pala torna a posto, niente buca
        MapSpot,   // il posto indicato dalla mappa: "Nothing. The map lied."
        Sand,      // altrove: solo sabbia
        Boot,      // altrove: un vecchio scarpone
        Can,       // altrove: una lattina arrugginita
        Crab,      // altrove: un granchio che pizzica
        Treasure,  // sulla X spostata a terra: il baule! VITTORIA (una volta sola)
    }

    /// <summary>
    /// Logica pura del livello 30 della roadmap "The Invisible Treasure" (id invisible_treasure) — "FIND THE TREASURE."
    /// Un'isoletta di sabbia in mezzo al mare con una palma; in alto a sinistra una mappa del tesoro (un foglio piccolo,
    /// come un pezzo di interfaccia nel mondo) che disegna la stessa isola con la palma e una X rossa accanto; sulla sabbia
    /// una pala.
    /// Inganno: scavare (trascinare la pala e lasciarla: si scava dove sta la PUNTA) nel posto indicato dalla mappa non trova
    /// niente ("Nothing. The map lied."); scavare altrove trova solo sabbia o cianfrusaglie a rotazione (sabbia, scarpone,
    /// sabbia, lattina, granchio che pizzica...). Le buche restano solo come decorazione e non bloccano mai nulla.
    /// Soluzione esatta: trascinare la X DALLA MAPPA sulla SABBIA (dove si vuole: tutta l'isola tranne un bordino) e poi
    /// scavare lì con la pala (tolleranza <see cref="XRadius"/>): esce il baule, vittoria una volta sola.
    /// La X lasciata sopra la mappa, in mare, in cielo o fuori dall'isola torna al suo posto sulla mappa; quella a terra
    /// si può riprendere e spostare ancora (tutto reversibile). Coordinate della mappa e del terreno distinte: la mappa è
    /// un rettangolo a parte e non è mai sabbia. <see cref="CancelHold"/> (pausa, app in background, secondo dito, livello
    /// spento) molla la presa senza effetti: la X resta dov'era prima della presa, la pala torna a posto senza scavare.
    /// Non si può perdere. Nessuna dipendenza da Unity.
    /// </summary>
    public sealed class InvisibleTreasureLogic
    {
        /// <summary>Distanza (unità del mondo) entro cui la punta della pala "prende" la X a terra.</summary>
        public const float XRadius = 0.8f;
        /// <summary>Distanza entro cui la punta della pala è "nel posto della mappa".</summary>
        public const float MapSpotRadius = 0.7f;
        /// <summary>Bordino dell'isola (verso l'interno) che non conta come sabbia valida.</summary>
        public const float SandMargin = 0.12f;

        /// <summary>Cosa si trova scavando altrove, a rotazione.</summary>
        public static readonly InvisibleTreasureDig[] JunkCycle =
        {
            InvisibleTreasureDig.Sand, InvisibleTreasureDig.Boot, InvisibleTreasureDig.Sand,
            InvisibleTreasureDig.Can, InvisibleTreasureDig.Crab,
        };

        // ---- geometria (mondo): valori del builder, il livello li può riscrivere con Configure ----
        public float IslandX { get; private set; } = 0f;
        public float IslandY { get; private set; } = -1.45f;
        public float IslandRx { get; private set; } = 2.3f;
        public float IslandRy { get; private set; } = 2.0f;
        public float MapX { get; private set; } = -1.15f;
        public float MapY { get; private set; } = 1.7f;
        public float MapHalfW { get; private set; } = 1.1f;
        public float MapHalfH { get; private set; } = 0.7f;
        public float SpotX { get; private set; } = 0.5f;
        public float SpotY { get; private set; } = -0.75f;

        public InvisibleTreasureStage Stage { get; private set; } = InvisibleTreasureStage.Playing;
        /// <summary>La X è stata spostata sulla sabbia (altrimenti è sulla mappa).</summary>
        public bool XOnGround { get; private set; }
        /// <summary>Posizione (mondo) della X a terra; vale solo se <see cref="XOnGround"/>.</summary>
        public float GroundX { get; private set; }
        public float GroundY { get; private set; }
        public bool HoldingX { get; private set; }
        public bool HoldingShovel { get; private set; }

        public int Digs { get; private set; }
        public int MapSpotDigs { get; private set; }
        public int JunkDigs { get; private set; }
        public int NotSandDrops { get; private set; }
        public int XPlacements { get; private set; }
        public int XReturns { get; private set; }
        readonly int[] taps = new int[7];

        public bool InputLocked => Stage != InvisibleTreasureStage.Playing;
        public bool Holding => HoldingX || HoldingShovel;
        public int Taps(InvisibleTreasureSpot spot) => taps[(int)spot];
        public int WrongMoves
        {
            get
            {
                int n = MapSpotDigs + JunkDigs + NotSandDrops + XReturns;
                for (int i = 0; i < taps.Length; i++) n += taps[i];
                return n;
            }
        }

        bool completed;

        /// <summary>Misure della scena (dal builder). Chiamata una volta; Reset non le tocca.</summary>
        public void Configure(float islandX, float islandY, float islandRx, float islandRy,
            float mapX, float mapY, float mapHalfW, float mapHalfH, float spotX, float spotY)
        {
            IslandX = islandX; IslandY = islandY; IslandRx = islandRx; IslandRy = islandRy;
            MapX = mapX; MapY = mapY; MapHalfW = mapHalfW; MapHalfH = mapHalfH;
            SpotX = spotX; SpotY = spotY;
        }

        public void Reset()
        {
            Stage = InvisibleTreasureStage.Playing;
            XOnGround = false;
            GroundX = GroundY = 0f;
            HoldingX = HoldingShovel = false;
            Digs = MapSpotDigs = JunkDigs = NotSandDrops = XPlacements = XReturns = 0;
            for (int i = 0; i < taps.Length; i++) taps[i] = 0;
            completed = false;
        }

        // ---------- Domande sulla geometria ----------

        /// <summary>Il punto è sulla sabbia valida dell'isola (e non sulla mappa).</summary>
        public bool IsSand(float x, float y) => !IsOnMap(x, y) && InEllipse(x, y, IslandX, IslandY, IslandRx - SandMargin, IslandRy - SandMargin);

        /// <summary>Il punto è sopra il foglio della mappa.</summary>
        public bool IsOnMap(float x, float y) => InRect(x, y, MapX, MapY, MapHalfW, MapHalfH);

        /// <summary>Il punto è nel posto indicato dalla mappa (sul terreno).</summary>
        public bool IsMapSpot(float x, float y) => Near(x, y, SpotX, SpotY, MapSpotRadius);

        /// <summary>Il punto è sulla X spostata a terra (tolleranza <see cref="XRadius"/>).</summary>
        public bool IsOnGroundX(float x, float y) => XOnGround && Near(x, y, GroundX, GroundY, XRadius);

        // ---------- Gesti ----------

        /// <summary>Tocco (senza trascinare) su una delle cose della scena: solo una battuta.</summary>
        public bool Tap(InvisibleTreasureSpot spot)
        {
            if (InputLocked || Holding) return false;
            if (spot == InvisibleTreasureSpot.GroundX && !XOnGround) spot = InvisibleTreasureSpot.MapX;
            taps[(int)spot]++;
            return true;
        }

        /// <summary>Il dito comincia a trascinare la X (dalla mappa o da dove è a terra).</summary>
        public bool GrabX()
        {
            if (InputLocked || Holding) return false;
            HoldingX = true;
            return true;
        }

        /// <summary>X lasciata nel punto (x, y): sulla sabbia resta lì, altrimenti torna sulla mappa.</summary>
        public InvisibleTreasureDrop DropX(float x, float y)
        {
            if (InputLocked || !HoldingX) return InvisibleTreasureDrop.None;
            HoldingX = false;
            if (IsOnMap(x, y))
            {
                XOnGround = false;
                return InvisibleTreasureDrop.OnMap;
            }
            if (!IsSand(x, y))
            {
                XOnGround = false;
                XReturns++;
                return InvisibleTreasureDrop.OffIsland;
            }
            XOnGround = true;
            GroundX = x;
            GroundY = y;
            XPlacements++;
            return InvisibleTreasureDrop.OnSand;
        }

        /// <summary>Il dito comincia a trascinare la pala.</summary>
        public bool GrabShovel()
        {
            if (InputLocked || Holding) return false;
            HoldingShovel = true;
            return true;
        }

        /// <summary>
        /// Pala lasciata con la punta in (x, y): si scava lì. Sulla X a terra -> il tesoro (vittoria); nel posto della mappa
        /// -> niente; altrove sulla sabbia -> sabbia o cianfrusaglie a rotazione; fuori dalla sabbia -> non si scava.
        /// </summary>
        public InvisibleTreasureDig DropShovel(float x, float y)
        {
            if (InputLocked || !HoldingShovel) return InvisibleTreasureDig.None;
            HoldingShovel = false;
            if (!IsSand(x, y))
            {
                NotSandDrops++;
                return InvisibleTreasureDig.NotSand;
            }
            Digs++;
            if (IsOnGroundX(x, y))
            {
                Stage = InvisibleTreasureStage.Celebrating;
                return InvisibleTreasureDig.Treasure;
            }
            if (IsMapSpot(x, y))
            {
                MapSpotDigs++;
                return InvisibleTreasureDig.MapSpot;
            }
            InvisibleTreasureDig found = JunkCycle[JunkDigs % JunkCycle.Length];
            JunkDigs++;
            return found;
        }

        /// <summary>Dito perso (pausa, app in background, secondo dito, livello spento): nessun effetto, niente scavo.</summary>
        public bool CancelHold()
        {
            if (!Holding) return false;
            HoldingX = HoldingShovel = false;
            return true;
        }

        /// <summary>Fine della festa: true una volta sola (allora il livello chiama Win).</summary>
        public bool TryComplete()
        {
            if (Stage != InvisibleTreasureStage.Celebrating || completed) return false;
            completed = true;
            Stage = InvisibleTreasureStage.Done;
            return true;
        }

        // ---------- Geometria pura ----------

        public static bool InEllipse(float x, float y, float cx, float cy, float rx, float ry)
        {
            if (rx <= 0f || ry <= 0f) return false;
            float u = (x - cx) / rx, v = (y - cy) / ry;
            return u * u + v * v <= 1f;
        }

        public static bool InRect(float x, float y, float cx, float cy, float halfW, float halfH)
        {
            return x >= cx - halfW && x <= cx + halfW && y >= cy - halfH && y <= cy + halfH;
        }

        public static bool Near(float x, float y, float qx, float qy, float r)
        {
            float dx = x - qx, dy = y - qy;
            return dx * dx + dy * dy <= r * r;
        }

        /// <summary>
        /// Dal disegno della mappa al terreno: la mappa disegna l'isola (centro <paramref name="mapIslandX"/>,
        /// <paramref name="mapIslandY"/>, coordinate locali della mappa) rimpicciolita di <paramref name="scale"/>.
        /// </summary>
        public static void MapToGround(float mx, float my, float mapIslandX, float mapIslandY, float scale,
            float islandX, float islandY, out float gx, out float gy)
        {
            if (scale <= 0f) { gx = islandX; gy = islandY; return; }
            gx = islandX + (mx - mapIslandX) / scale;
            gy = islandY + (my - mapIslandY) / scale;
        }
    }
}
