using System;

namespace Gotcha.Levels.V64
{
    /// <summary>Fasi del livello "The Fake Trophy".</summary>
    public enum FakeTrophyStage
    {
        Playing,      // si prova di tutto
        Celebrating,  // WIN è scattata sulla coppa: diventa d'oro, l'omino la solleva, la festa (input bloccato)
        Done,         // vittoria già data
    }

    /// <summary>Risultato di un gesto (o di un passo di un gesto) di The Fake Trophy.</summary>
    public enum FakeTrophyResult
    {
        None,           // niente da fare (input bloccato, gesto non in corso, un altro gesto già in corso)
        Pressing,       // pressione lunga su WIN in corso (l'anello si riempie: PressProgress)
        Detached,       // pressione lunga completata: WIN si stacca dall'insegna ed è trascinabile
        SignFlash,      // tocco sull'insegna o tocco breve su WIN (o dito scivolato via prima del distacco): WIN lampeggia e basta
        Moved,          // WIN trascinata (non ancora sulla coppa)
        Snapped,        // WIN arrivata nella zona centrale della coppa: scatta, la coppa diventa d'oro, VITTORIA (una volta sola)
        ReturnedToSign, // WIN lasciata altrove: torna sull'insegna (serve un'altra pressione lunga)
        Clink,          // tocco sulla coppa: "Clink. Hollow."
        TrophyMoved,    // coppa trascinata
        TrophyRejected, // coppa lasciata sull'omino: "It's not a real prize." e torna sul piedistallo
        TrophyReturned, // coppa lasciata altrove: torna sul piedistallo
        GuySigh,        // tocco sull'omino deluso
    }

    /// <summary>
    /// Logica pura del livello 129 della roadmap "The Fake Trophy" (id fake_trophy) — "WIN THE TROPHY." Una coppa grigia
    /// e anonima su un piedistallo, un omino deluso e una grande insegna luminosa con la parola WIN.
    /// Inganno: toccare la coppa fa solo "clink" (<see cref="FakeTrophyResult.Clink"/>); portare la coppa all'omino non
    /// serve, torna sul piedistallo (<see cref="FakeTrophyResult.TrophyRejected"/>); toccare l'insegna fa lampeggiare WIN e
    /// basta (<see cref="FakeTrophyResult.SignFlash"/>). Tutto reversibile, mai sconfitta.
    /// Soluzione esatta: pressione LUNGA su WIN (<see cref="HoldSeconds"/> secondi con il dito fermo entro
    /// <see cref="PressSlop"/>; un tocco breve o un dito che scivola via non la stacca), WIN si stacca e diventa trascinabile;
    /// quando il suo centro entra nella zona centrale della coppa (rettangolo dato al costruttore, generoso) scatta
    /// (<see cref="FakeTrophyResult.Snapped"/>): la coppa diventa d'oro, deterministico e una volta sola. WIN lasciata
    /// altrove torna sull'insegna (<see cref="FakeTrophyResult.ReturnedToSign"/>).
    /// Un gesto alla volta (le altre dita non fanno niente). Cancel() (pausa, app in background, livello spento) lascia
    /// andare tutto senza vincere: WIN torna sull'insegna, la coppa sul piedistallo. Non si può perdere. Nessuna dipendenza
    /// da Unity: le posizioni (insegna, coppa, omino) arrivano dal livello, così vale qualunque disposizione.
    /// </summary>
    public sealed class FakeTrophyLogic
    {
        /// <summary>Durata della pressione lunga che stacca WIN.</summary>
        public const float HoldSeconds = 0.7f;
        /// <summary>Quanto può muoversi il dito durante la pressione lunga senza annullarla.</summary>
        public const float PressSlop = 0.35f;
        /// <summary>Spostamento minimo perché il trascinamento della coppa conti (sotto è un tocco).</summary>
        public const float TapSlop = 0.15f;
        /// <summary>Centro di WIN e base della coppa trascinate restano in x ±ScreenHalf (così la parola e i manici restano nello schermo) e in y [MinY, MaxY] (sotto la riga dell'obiettivo).</summary>
        public const float ScreenHalf = 1.9f, MinY = -4.2f, MaxY = 2.9f;

        // disposizione di base (la stessa del builder GotchaLevels87B)
        public const float FloorY = -2.9f;
        public const float DefaultWordX = 0f, DefaultWordY = 1.85f;
        public const float DefaultTrophyX = 1.4f, DefaultTrophyY = -1.7f;
        public const float DefaultZoneX = 1.4f, DefaultZoneY = -0.8f, DefaultZoneHalfW = 0.6f, DefaultZoneHalfH = 0.6f;
        public const float DefaultGuyX = -1.35f, DefaultGuyY = -1.9f, DefaultGuyHalfW = 0.75f, DefaultGuyHalfH = 1.05f;

        public readonly float WordHomeX, WordHomeY, TrophyHomeX, TrophyHomeY;
        public readonly float ZoneX, ZoneY, ZoneHalfW, ZoneHalfH;
        public readonly float GuyX, GuyY, GuyHalfW, GuyHalfH;

        public FakeTrophyStage Stage { get; private set; } = FakeTrophyStage.Playing;
        public bool InputLocked => Stage != FakeTrophyStage.Playing;

        /// <summary>Pressione lunga su WIN in corso (prima del distacco).</summary>
        public bool Pressing { get; private set; }
        /// <summary>Quanto è piena la pressione lunga (0..1).</summary>
        public float PressProgress => Pressing ? Math.Min(1f, pressTime / HoldSeconds) : 0f;
        /// <summary>WIN staccata e tenuta dal dito.</summary>
        public bool WordHeld { get; private set; }
        /// <summary>WIN è sulla coppa (dopo lo scatto: la coppa è d'oro).</summary>
        public bool WordOnTrophy { get; private set; }
        public bool TrophyGold => WordOnTrophy;
        public bool TrophyHeld { get; private set; }
        /// <summary>Un gesto è già in corso (le altre dita non fanno niente).</summary>
        public bool Busy => Pressing || WordHeld || TrophyHeld;

        /// <summary>Centro di WIN (sull'insegna, tenuta o sulla coppa).</summary>
        public float WordX { get; private set; }
        public float WordY { get; private set; }
        /// <summary>Base della coppa (sul piedistallo, tranne mentre è tenuta).</summary>
        public float TrophyX { get; private set; }
        public float TrophyY { get; private set; }
        /// <summary>Da dove è stata lasciata l'ultima volta la coppa / WIN (per le animazioni di ritorno).</summary>
        public float DropX { get; private set; }
        public float DropY { get; private set; }

        public int SignFlashes { get; private set; }
        public int ShortPresses { get; private set; }
        public int Returns { get; private set; }
        public int Clinks { get; private set; }
        public int Rejections { get; private set; }
        public int TrophyReturns { get; private set; }
        public int GuySighs { get; private set; }
        public int Detaches { get; private set; }
        public int WrongMoves => SignFlashes + Returns + Clinks + Rejections + TrophyReturns;

        float pressTime, pressMoved, trophyMoved;
        bool completed;

        public FakeTrophyLogic() : this(DefaultWordX, DefaultWordY, DefaultTrophyX, DefaultTrophyY,
            DefaultZoneX, DefaultZoneY, DefaultZoneHalfW, DefaultZoneHalfH, DefaultGuyX, DefaultGuyY, DefaultGuyHalfW, DefaultGuyHalfH) { }

        /// <param name="wordX">Centro di WIN sull'insegna.</param>
        /// <param name="trophyX">Base della coppa sul piedistallo.</param>
        /// <param name="zoneX">Centro della zona centrale della coppa (dove WIN scatta) e sue mezze misure.</param>
        /// <param name="guyX">Centro dell'area dell'omino (dove la coppa viene rifiutata) e sue mezze misure.</param>
        public FakeTrophyLogic(float wordX, float wordY, float trophyX, float trophyY,
            float zoneX, float zoneY, float zoneHalfW, float zoneHalfH,
            float guyX, float guyY, float guyHalfW, float guyHalfH)
        {
            WordHomeX = wordX; WordHomeY = wordY;
            TrophyHomeX = trophyX; TrophyHomeY = trophyY;
            ZoneX = zoneX; ZoneY = zoneY; ZoneHalfW = Math.Max(0.05f, Math.Abs(zoneHalfW)); ZoneHalfH = Math.Max(0.05f, Math.Abs(zoneHalfH));
            GuyX = guyX; GuyY = guyY; GuyHalfW = Math.Max(0.05f, Math.Abs(guyHalfW)); GuyHalfH = Math.Max(0.05f, Math.Abs(guyHalfH));
            Reset();
        }

        /// <summary>Tutto da capo (rientro nel livello).</summary>
        public void Reset()
        {
            Stage = FakeTrophyStage.Playing;
            Pressing = WordHeld = WordOnTrophy = TrophyHeld = false;
            WordX = WordHomeX; WordY = WordHomeY;
            TrophyX = TrophyHomeX; TrophyY = TrophyHomeY;
            DropX = DropY = 0f;
            pressTime = pressMoved = trophyMoved = 0f;
            SignFlashes = ShortPresses = Returns = Clinks = Rejections = TrophyReturns = GuySighs = Detaches = 0;
            completed = false;
        }

        /// <summary>Il punto (x, y) è nella zona centrale della coppa (sul piedistallo).</summary>
        public bool InZone(float x, float y) => Math.Abs(x - ZoneX) <= ZoneHalfW && Math.Abs(y - ZoneY) <= ZoneHalfH;

        /// <summary>Il punto (x, y) è sull'omino.</summary>
        public bool OnGuy(float x, float y) => Math.Abs(x - GuyX) <= GuyHalfW && Math.Abs(y - GuyY) <= GuyHalfH;

        static float Clamp(float v, float lo, float hi) => v < lo ? lo : v > hi ? hi : v;

        // ---------- WIN: pressione lunga, distacco, trascinamento ----------

        /// <summary>Dito giù su WIN (sull'insegna): comincia la pressione lunga.</summary>
        public bool PressWord()
        {
            if (InputLocked || Busy || WordOnTrophy) return false;
            Pressing = true;
            pressTime = pressMoved = 0f;
            return true;
        }

        /// <summary>
        /// Un passo della pressione lunga: dt secondi passati, moved = distanza del dito dal punto di partenza. Il dito che
        /// si sposta oltre <see cref="PressSlop"/> annulla (WIN lampeggia e basta); al tempo pieno WIN si stacca.
        /// </summary>
        public FakeTrophyResult HoldWord(float dt, float moved)
        {
            if (InputLocked || !Pressing) return FakeTrophyResult.None;
            if (float.IsNaN(dt) || dt < 0f) dt = 0f;
            if (float.IsNaN(moved)) moved = 0f;
            pressMoved = Math.Max(pressMoved, moved);
            if (pressMoved > PressSlop)
            {
                Pressing = false;
                pressTime = 0f;
                ShortPresses++;
                SignFlashes++;
                return FakeTrophyResult.SignFlash;
            }
            pressTime += Math.Min(dt, 0.25f);   // un fotogramma lunghissimo non stacca da solo
            if (pressTime + 1e-5f < HoldSeconds) return FakeTrophyResult.Pressing;
            Pressing = false;
            pressTime = 0f;
            WordHeld = true;
            Detaches++;
            return FakeTrophyResult.Detached;
        }

        /// <summary>Dito alzato durante la pressione lunga (prima del distacco): tocco breve, WIN lampeggia.</summary>
        public FakeTrophyResult ReleasePress()
        {
            if (InputLocked || !Pressing) return FakeTrophyResult.None;
            Pressing = false;
            pressTime = 0f;
            ShortPresses++;
            SignFlashes++;
            return FakeTrophyResult.SignFlash;
        }

        /// <summary>WIN tenuta segue il dito (dentro lo schermo); nella zona centrale della coppa scatta.</summary>
        public FakeTrophyResult MoveWord(float x, float y)
        {
            if (InputLocked || !WordHeld) return FakeTrophyResult.None;
            if (float.IsNaN(x) || float.IsNaN(y)) return FakeTrophyResult.Moved;
            WordX = Clamp(x, -ScreenHalf, ScreenHalf);
            WordY = Clamp(y, MinY, MaxY);
            return InZone(WordX, WordY) ? Snap() : FakeTrophyResult.Moved;
        }

        /// <summary>Dito alzato con WIN in mano: sulla coppa scatta, altrimenti torna sull'insegna.</summary>
        public FakeTrophyResult ReleaseWord()
        {
            if (InputLocked || !WordHeld) return FakeTrophyResult.None;
            if (InZone(WordX, WordY)) return Snap();
            WordHeld = false;
            DropX = WordX; DropY = WordY;
            WordX = WordHomeX; WordY = WordHomeY;
            Returns++;
            return FakeTrophyResult.ReturnedToSign;
        }

        FakeTrophyResult Snap()
        {
            WordHeld = false;
            WordOnTrophy = true;
            WordX = ZoneX; WordY = ZoneY;
            Stage = FakeTrophyStage.Celebrating;
            return FakeTrophyResult.Snapped;
        }

        // ---------- Insegna, coppa, omino ----------

        /// <summary>Tocco sull'insegna (fuori da WIN, o mentre WIN non si può premere): WIN lampeggia e basta.</summary>
        public FakeTrophyResult TapSign()
        {
            if (InputLocked) return FakeTrophyResult.None;
            SignFlashes++;
            return FakeTrophyResult.SignFlash;
        }

        public FakeTrophyResult TapTrophy()
        {
            if (InputLocked || TrophyHeld) return FakeTrophyResult.None;
            Clinks++;
            return FakeTrophyResult.Clink;
        }

        public FakeTrophyResult TapGuy()
        {
            if (InputLocked) return FakeTrophyResult.None;
            GuySighs++;
            return FakeTrophyResult.GuySigh;
        }

        /// <summary>Presa della coppa (dopo che il dito si è mosso oltre il tocco).</summary>
        public bool GrabTrophy()
        {
            if (InputLocked || Busy) return false;
            TrophyHeld = true;
            trophyMoved = 0f;
            return true;
        }

        public FakeTrophyResult MoveTrophy(float x, float y)
        {
            if (InputLocked || !TrophyHeld) return FakeTrophyResult.None;
            if (float.IsNaN(x) || float.IsNaN(y)) return FakeTrophyResult.TrophyMoved;
            float nx = Clamp(x, -ScreenHalf, ScreenHalf), ny = Clamp(y, MinY, MaxY);
            trophyMoved = Math.Max(trophyMoved, (float)Math.Sqrt((nx - TrophyHomeX) * (nx - TrophyHomeX) + (ny - TrophyHomeY) * (ny - TrophyHomeY)));
            TrophyX = nx; TrophyY = ny;
            return FakeTrophyResult.TrophyMoved;
        }

        /// <summary>
        /// Coppa lasciata: torna SEMPRE sul piedistallo; sull'omino (base della coppa o il suo centro, 0.5 più su, nella sua
        /// area) è un rifiuto ("It's not a real prize."). Una coppa grigia non fa vincere mai.
        /// </summary>
        public FakeTrophyResult ReleaseTrophy()
        {
            if (InputLocked || !TrophyHeld) return FakeTrophyResult.None;
            TrophyHeld = false;
            DropX = TrophyX; DropY = TrophyY;
            bool onGuy = OnGuy(TrophyX, TrophyY) || OnGuy(TrophyX, TrophyY + 0.5f);
            TrophyX = TrophyHomeX; TrophyY = TrophyHomeY;
            if (onGuy) { Rejections++; return FakeTrophyResult.TrophyRejected; }
            if (trophyMoved >= TapSlop) TrophyReturns++;   // appena sfiorata: rimessa a posto, non conta come tentativo
            return FakeTrophyResult.TrophyReturned;
        }

        // ---------- Ciclo di vita ----------

        /// <summary>Pausa, app in background, livello spento: si molla tutto (WIN sull'insegna, coppa sul piedistallo), senza vincere.</summary>
        public void Cancel()
        {
            if (InputLocked) return;
            Pressing = false;
            pressTime = pressMoved = 0f;
            if (WordHeld) { WordHeld = false; WordX = WordHomeX; WordY = WordHomeY; }
            if (TrophyHeld) { TrophyHeld = false; TrophyX = TrophyHomeX; TrophyY = TrophyHomeY; }
        }

        /// <summary>A festa finita: vero una volta sola (poi Done).</summary>
        public bool TryComplete()
        {
            if (Stage != FakeTrophyStage.Celebrating || completed) return false;
            completed = true;
            Stage = FakeTrophyStage.Done;
            return true;
        }
    }
}
