namespace Gotcha.Levels.V64
{
    /// <summary>Fasi del livello "The Lazy Sun".</summary>
    public enum LazySunStage
    {
        Playing,      // si prova di tutto
        Celebrating,  // la collina è abbassata abbastanza: alba e festa (input bloccato)
        Done,         // vittoria già data
    }

    /// <summary>Le cose che si possono toccare (senza trascinare).</summary>
    public enum LazySunSpot
    {
        Sky,       // cielo e stelle: "The sky ignores you." (le stelle brillano)
        Moon,      // la luna parla: "Not my shift."
        Rooster,   // il gallo apre un occhio: "Too early."
        Hill,      // la collina toccata e basta (non trascinata)
        Sun,       // il sole, solo dove già si vede sopra la collina: dorme ancora
        Ground,    // il prato davanti
    }

    /// <summary>La collina lasciata andare.</summary>
    public enum LazySunRelease
    {
        None,    // input bloccato o collina non tenuta
        Stays,   // sopra la soglia: resta dov'è (il sole, se si vede, continua a russare)
        Rises,   // abbassata oltre la soglia: il sole si sveglia, VITTORIA (una volta sola)
    }

    /// <summary>
    /// Logica pura del livello 24 della roadmap "The Lazy Sun" (id lazy_sun) — "MAKE THE SUN RISE." Paesaggio notturno
    /// con una grande collina tonda in basso, il cielo stellato, la luna e un gallo addormentato sullo steccato; da dietro
    /// la cima della collina escono dei "Zzz": il sole dorme lì dietro, col berretto da notte.
    /// Inganno: toccare cielo, luna, gallo, collina o prato non serve (battute, tutto reversibile); aspettare non serve
    /// (ogni <see cref="BigSnoreSeconds"/> secondi di quiete un russare più forte, mai una vittoria); il sole NON si
    /// trascina (<see cref="GrabSun"/>: "It's too heavy to lift. And asleep.").
    /// Soluzione esatta: trascinare GIÙ la collina (la linea dell'orizzonte). <see cref="Lowered"/> va da 0 (posizione
    /// iniziale) a 1 (profondità massima) ed è sempre limitato in quell'intervallo (verso l'alto oltre l'inizio non si
    /// va). Lasciata sotto la soglia <see cref="Threshold"/> la collina RESTA DOVE È stata lasciata (nessun ritorno, niente
    /// vicoli ciechi: la si può riprendere quando si vuole); lasciata oltre la soglia -> alba e vittoria una volta sola.
    /// La vittoria dipende solo dallo spostamento dell'orizzonte, mai dal sole. Cancel() (pausa, app in background,
    /// secondo dito, livello spento) molla la presa: la collina resta dov'è e NON vince (serve un rilascio vero).
    /// Non si può perdere. Nessuna dipendenza da Unity.
    /// </summary>
    public sealed class LazySunLogic
    {
        /// <summary>Frazione della corsa oltre la quale, al rilascio, il sole si sveglia.</summary>
        public const float Threshold = 0.75f;
        /// <summary>Da questa frazione in su il sole si vede già un po' sopra la collina (continua a russare).</summary>
        public const float PeekLowered = 0.3f;
        /// <summary>Secondi di quiete prima del russare forte ("ZZZ..."); poi si ricomincia a contare.</summary>
        public const float BigSnoreSeconds = 10f;

        public LazySunStage Stage { get; private set; } = LazySunStage.Playing;
        /// <summary>Quanto è abbassata la collina: 0 all'inizio, 1 alla profondità massima.</summary>
        public float Lowered { get; private set; }
        /// <summary>Il dito sta tenendo la collina.</summary>
        public bool Holding { get; private set; }
        /// <summary>Secondi di quiete accumulati (si azzerano a ogni gesto e dopo ogni russare forte).</summary>
        public float Idle { get; private set; }

        public int BigSnores { get; private set; }
        public int SunGrabs { get; private set; }
        public int ShortReleases { get; private set; }
        public int HillGrabs { get; private set; }
        readonly int[] taps = new int[6];

        public bool InputLocked => Stage != LazySunStage.Playing;
        /// <summary>Il sole spunta già un po' sopra la collina.</summary>
        public bool SunPeeking => Lowered >= PeekLowered;
        /// <summary>Lasciata adesso, la collina farebbe sorgere il sole.</summary>
        public bool PastThreshold => Lowered >= Threshold;
        public int Taps(LazySunSpot spot) => taps[(int)spot];
        public int WrongMoves => taps[0] + taps[1] + taps[2] + taps[3] + taps[4] + taps[5] + SunGrabs + ShortReleases;

        bool completed;

        public void Reset()
        {
            Stage = LazySunStage.Playing;
            Lowered = 0f;
            Holding = false;
            Idle = 0f;
            BigSnores = SunGrabs = ShortReleases = HillGrabs = 0;
            for (int i = 0; i < taps.Length; i++) taps[i] = 0;
            completed = false;
        }

        /// <summary>Il tempo passa: dopo <see cref="BigSnoreSeconds"/> di quiete il sole russa forte (true). Mai una vittoria.</summary>
        public bool Tick(float dt)
        {
            if (dt <= 0f || InputLocked || Holding) return false;
            Idle += dt;
            if (Idle < BigSnoreSeconds) return false;
            Idle = 0f;
            BigSnores++;
            return true;
        }

        /// <summary>Tocco (senza trascinare) su una delle cose della scena: solo una battuta.</summary>
        public bool Tap(LazySunSpot spot)
        {
            if (InputLocked || Holding) return false;
            if (spot == LazySunSpot.Sun && !SunPeeking) spot = LazySunSpot.Hill;   // il sole ancora coperto: si è toccata la collina
            taps[(int)spot]++;
            Idle = 0f;
            return true;
        }

        /// <summary>Tentativo di trascinare il sole: troppo pesante (e addormentato). Non si muove nulla.</summary>
        public bool GrabSun()
        {
            if (InputLocked || Holding) return false;
            SunGrabs++;
            Idle = 0f;
            return true;
        }

        /// <summary>Il dito comincia a trascinare la collina.</summary>
        public bool GrabHill()
        {
            if (InputLocked || Holding) return false;
            Holding = true;
            HillGrabs++;
            Idle = 0f;
            return true;
        }

        /// <summary>La collina segue il dito: <paramref name="lowered"/> viene limitato in [0, 1]. Restituisce il valore usato.</summary>
        public float DragHill(float lowered)
        {
            if (InputLocked || !Holding) return Lowered;
            Lowered = Clamp01(lowered);
            Idle = 0f;
            return Lowered;
        }

        /// <summary>Collina lasciata: oltre la soglia il sole si sveglia (vittoria), altrimenti resta dov'è.</summary>
        public LazySunRelease ReleaseHill()
        {
            if (InputLocked || !Holding) return LazySunRelease.None;
            Holding = false;
            Idle = 0f;
            if (Lowered < Threshold)
            {
                ShortReleases++;
                return LazySunRelease.Stays;
            }
            Lowered = 1f;   // scivola fino in fondo
            Stage = LazySunStage.Celebrating;
            return LazySunRelease.Rises;
        }

        /// <summary>Dito perso (pausa, app in background, secondo dito, livello spento): la collina resta dov'è, niente vittoria.</summary>
        public bool Cancel()
        {
            if (!Holding) return false;
            Holding = false;
            Idle = 0f;
            return true;
        }

        /// <summary>Fine della festa: true una volta sola (allora il livello chiama Win).</summary>
        public bool TryComplete()
        {
            if (Stage != LazySunStage.Celebrating || completed) return false;
            completed = true;
            Stage = LazySunStage.Done;
            return true;
        }

        // ---------- Geometria (dalle misure della collina, mai coordinate dello schermo) ----------

        /// <summary>
        /// Frazione abbassata mentre si trascina: quella al momento della presa più lo spostamento del dito verso il basso
        /// diviso la corsa (in unità), limitata in [0, 1].
        /// </summary>
        public static float LoweredFromDrag(float grabLowered, float startY, float fingerY, float travel)
        {
            if (travel <= 0f) return Clamp01(grabLowered);
            return Clamp01(grabLowered + (startY - fingerY) / travel);
        }

        /// <summary>
        /// Di quanto il bordo della collina (ellisse di mezza larghezza <paramref name="halfWidth"/> e altezza
        /// <paramref name="height"/>, cima in dx = 0) sta sotto la cima a distanza orizzontale <paramref name="dx"/>.
        /// </summary>
        public static float EdgeDrop(float dx, float halfWidth, float height)
        {
            if (halfWidth <= 0f) return height;
            float u = dx / halfWidth;
            float k = 1f - u * u;
            if (k <= 0f) return height;
            return height - height * (float)System.Math.Sqrt(k);
        }

        /// <summary>Il punto (altezza py) prende la collina: dal bordo (edgeY) in su di <paramref name="above"/>, in giù di <paramref name="below"/>.</summary>
        public static bool InHillGrab(float py, float edgeY, float above, float below)
        {
            return py <= edgeY + (above > 0f ? above : 0f) && py >= edgeY - (below > 0f ? below : 0f);
        }

        /// <summary>Il punto è sul sole (cerchio) E sopra il bordo della collina di almeno <paramref name="clearance"/> (cioè si vede).</summary>
        public static bool OnVisibleSun(float px, float py, float sx, float sy, float radius, float edgeY, float clearance)
        {
            float dx = px - sx, dy = py - sy;
            return dx * dx + dy * dy <= radius * radius && py > edgeY + clearance;
        }

        static float Clamp01(float v) => v < 0f ? 0f : v > 1f ? 1f : v;
    }
}
