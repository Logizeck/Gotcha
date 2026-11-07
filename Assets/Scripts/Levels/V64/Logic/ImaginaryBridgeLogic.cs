using System;

namespace Gotcha.Levels.V64
{
    /// <summary>Fasi del livello "The Imaginary Bridge".</summary>
    public enum ImaginaryBridgeStage
    {
        Playing,      // si prova di tutto
        Celebrating,  // asse lunga posata sul canyon: l'omino attraversa (input bloccato)
        Done,         // vittoria già data
    }

    /// <summary>Risultato di un gesto finito (asse posata, pizzico finito, omino lasciato, tocco).</summary>
    public enum ImaginaryBridgeDrop
    {
        None,       // niente da fare (input bloccato o gesto non in corso)
        Unchanged,  // gesto senza effetto (pizzico senza cambiare lunghezza, asse lasciata dov'era, tocco qualunque)
        Placed,     // asse corta posata su una piattaforma
        Stretched,  // pizzico finito: l'asse ha cambiato lunghezza (ma non fa da ponte)
        WrongSpot,  // asse abbastanza lunga ma posata su una piattaforma, non sul canyon
        TooShort,   // asse corta lasciata sul canyon: cade nel vuoto e ricompare al suo posto ("Too short!")
        Slipped,    // asse lunga lasciata sul canyon ma con un capo nel vuoto: cade e ricompare al suo posto
        Walked,     // omino spostato sulla sua piattaforma (non fino al bordo)
        Refused,    // omino trascinato fino al bordo: non salta ("Nope. Not jumping that.") e fa un passo indietro
        Bridged,    // asse lunga con i due capi sulle due piattaforme: diventa un ponte, VITTORIA (una volta sola)
    }

    /// <summary>
    /// Logica pura del livello 110 della roadmap "The Imaginary Bridge" (id imaginary_bridge) — "CROSS THE CANYON." Due
    /// piattaforme di roccia separate da un canyon (bordi in x <see cref="LeftEdgeX"/> e <see cref="RightEdgeX"/>, piano
    /// in y <see cref="CliffY"/>), un'asse di legno corta (<see cref="MinLength"/>) e un omino sulla piattaforma di sinistra.
    /// Inganno: l'asse corta messa sul canyon cade nel vuoto e ricompare al suo posto (<see cref="ImaginaryBridgeDrop.TooShort"/>);
    /// trascinare l'omino lo porta fino al bordo ma non oltre (<see cref="ImaginaryBridgeDrop.Refused"/>). Tutto reversibile.
    /// Soluzione esatta: allungare l'asse col pizzico a due dita (lunghezza = iniziale × rapporto delle distanze delle
    /// dita, fra <see cref="MinLength"/> e <see cref="MaxLength"/>) fino ad almeno <see cref="NeedLength"/> e posarla sul
    /// canyon con i due capi nelle due zone di aggancio: capo sinistro in [<see cref="LeftZoneMin"/>,
    /// <see cref="LeftEdgeX"/> - <see cref="Support"/>], capo destro in [<see cref="RightEdgeX"/> + <see cref="Support"/>,
    /// <see cref="RightZoneMax"/>]. Allora <see cref="ImaginaryBridgeDrop.Bridged"/>: l'asse scatta nello stato stabile
    /// (orizzontale, centrata sul canyon in x <see cref="BridgeX"/>, sul piano delle rocce) e l'omino attraversa da solo.
    /// La lunghezza resta dopo il pizzico (anche dopo una caduta). Dal pizzico si passa al trascinamento senza blocchi:
    /// alzando un dito, l'altro continua a tenere l'asse (<see cref="EndPinch"/> con keepDragging).
    /// Limiti: l'asse resta nello schermo (capi in x ±<see cref="ScreenHalf"/>), non scende mai sotto il piano delle rocce
    /// e si solleva al massimo di <see cref="MaxLift"/>; posata è sempre sul piano delle rocce. L'omino resta sulla sua
    /// piattaforma (x in [<see cref="ManMinX"/>, <see cref="ManEdgeX"/>]): prima del ponte non può attraversare.
    /// Cancel() (pausa, app in background, livello spento) lascia andare tutto, senza vincere; un'asse tenuta sul canyon
    /// torna al suo posto. Non si può perdere. Nessuna dipendenza da Unity.
    /// </summary>
    public sealed class ImaginaryBridgeLogic
    {
        /// <summary>Piano delle due piattaforme (sotto dell'asse posata, piedi dell'omino).</summary>
        public const float CliffY = -1.0f;
        /// <summary>Bordi del canyon (fine della piattaforma di sinistra, inizio di quella di destra).</summary>
        public const float LeftEdgeX = -0.8f, RightEdgeX = 0.8f;
        /// <summary>Quanto ogni capo deve appoggiare sulla roccia oltre il bordo.</summary>
        public const float Support = 0.15f;
        /// <summary>Fine esterna delle due zone di aggancio (= bordo dello schermo utile).</summary>
        public const float LeftZoneMin = -ScreenHalf, RightZoneMax = ScreenHalf;
        /// <summary>I capi dell'asse restano in x ±ScreenHalf.</summary>
        public const float ScreenHalf = 2.6f;
        /// <summary>Lunghezza all'inizio (l'asse corta) e minimo dopo un pizzico che la accorcia.</summary>
        public const float MinLength = 1.0f;
        /// <summary>Lunghezza necessaria per il ponte (canyon + due appoggi).</summary>
        public const float NeedLength = RightEdgeX - LeftEdgeX + 2f * Support;
        /// <summary>Lunghezza massima (centrata, i capi restano dentro lo schermo).</summary>
        public const float MaxLength = 3.5f;
        /// <summary>Spessore dell'asse (l'omino che attraversa ci cammina sopra).</summary>
        public const float Thickness = 0.2f;
        /// <summary>Quanto si può sollevare l'asse tenuta.</summary>
        public const float MaxLift = 2.6f;
        /// <summary>Posizione iniziale (e "di casa") dell'asse: sulla piattaforma di sinistra, vicino al bordo.</summary>
        public const float PlankStartX = -1.35f;
        /// <summary>Centro del ponte (lo stato stabile in cui scatta l'asse giusta).</summary>
        public const float BridgeX = 0f;
        /// <summary>L'omino: partenza, limiti sulla sua piattaforma, passo indietro dal bordo, arrivo dall'altra parte.</summary>
        public const float ManStartX = -2.05f, ManMinX = -2.25f, ManEdgeX = LeftEdgeX - 0.3f;
        public const float EdgeNear = 0.25f, StepBack = 0.45f, ManFinalX = 1.45f;
        /// <summary>Spostamento minimo perché un trascinamento conti come tentativo.</summary>
        public const float MoveMin = 0.15f;

        public ImaginaryBridgeStage Stage { get; private set; } = ImaginaryBridgeStage.Playing;
        public bool InputLocked => Stage != ImaginaryBridgeStage.Playing;

        /// <summary>Lunghezza attuale dell'asse.</summary>
        public float Length { get; private set; } = MinLength;
        public bool LongEnough => Length >= NeedLength - 1e-4f;
        /// <summary>Centro dell'asse (x) e il suo lato di sotto (y: sul piano delle rocce, tranne mentre è tenuta sollevata).</summary>
        public float PlankX { get; private set; } = PlankStartX;
        public float PlankY { get; private set; } = CliffY;
        public float LeftEnd => PlankX - Length * 0.5f;
        public float RightEnd => PlankX + Length * 0.5f;
        /// <summary>Il centro dell'asse è sopra il vuoto (lasciata lì, cade se non fa da ponte).</summary>
        public bool OverGap => PlankX > LeftEdgeX && PlankX < RightEdgeX;
        /// <summary>I due capi sono nelle due zone di aggancio (lunga abbastanza e messa bene).</summary>
        public bool Spans => SpansAt(PlankX, Length);

        public bool PlankHeld { get; private set; }
        public bool Pinching { get; private set; }
        /// <summary>L'asse sta cadendo nel canyon (animazione del livello; finisce con FallDone). Intanto non si prende.</summary>
        public bool Falling { get; private set; }
        /// <summary>Da dove è caduta l'ultima volta (centro x e y; per l'animazione).</summary>
        public float FallFromX { get; private set; }
        public float FallFromY { get; private set; }
        public bool ManHeld { get; private set; }
        public float ManX { get; private set; } = ManStartX;

        public int ShortFalls { get; private set; }
        public int Slips { get; private set; }
        public int WrongSpots { get; private set; }
        public int Refusals { get; private set; }
        public int ManWalks { get; private set; }
        public int Stretches { get; private set; }
        public int PlankTaps { get; private set; }
        public int ManTaps { get; private set; }
        public int WrongMoves => ShortFalls + Slips + WrongSpots + Refusals;

        float pinchStart = MinLength, grabX, grabY, manGrabX;
        bool completed;

        public void Reset()
        {
            Stage = ImaginaryBridgeStage.Playing;
            Length = MinLength;
            PlankX = PlankStartX;
            PlankY = CliffY;
            PlankHeld = Pinching = Falling = ManHeld = false;
            FallFromX = PlankStartX;
            FallFromY = CliffY;
            ManX = ManStartX;
            ShortFalls = Slips = WrongSpots = Refusals = ManWalks = Stretches = PlankTaps = ManTaps = 0;
            pinchStart = MinLength;
            grabX = PlankStartX;
            grabY = CliffY;
            manGrabX = ManStartX;
            completed = false;
        }

        /// <summary>Un'asse di quella lunghezza con quel centro avrebbe i due capi nelle due zone di aggancio.</summary>
        public static bool SpansAt(float x, float length)
        {
            float l = x - length * 0.5f, r = x + length * 0.5f;
            const float eps = 1e-4f;
            return l >= LeftZoneMin - eps && l <= LeftEdgeX - Support + eps
                && r >= RightEdgeX + Support - eps && r <= RightZoneMax + eps;
        }

        /// <summary>Dove torna l'asse dopo una caduta: al suo posto sulla piattaforma di sinistra (dentro lo schermo).</summary>
        public static float HomeX(float length) => Math.Max(PlankStartX, -ScreenHalf + length * 0.5f);

        /// <summary>Il centro dell'asse limitato perché i capi restino nello schermo.</summary>
        public static float ClampX(float x, float length)
        {
            float half = Math.Max(0f, ScreenHalf - length * 0.5f);
            return Clamp(x, -half, half);
        }

        // ---------- L'asse: trascinamento ----------

        /// <summary>Un dito comincia a trascinare l'asse (non mentre cade).</summary>
        public bool GrabPlank()
        {
            if (InputLocked || PlankHeld || Pinching || Falling) return false;
            PlankHeld = true;
            grabX = PlankX;
            grabY = PlankY;
            return true;
        }

        /// <summary>L'asse tenuta va con il centro in x e il lato di sotto in y, limitata (nello schermo, mai sotto il piano delle rocce).</summary>
        public bool MovePlank(float x, float bottomY)
        {
            if (InputLocked || !PlankHeld) return false;
            PlankX = ClampX(x, Length);
            PlankY = Clamp(bottomY, CliffY, CliffY + MaxLift);
            return true;
        }

        /// <summary>Il dito lascia l'asse: scende sul piano delle rocce e si guarda dove è stata posata.</summary>
        public ImaginaryBridgeDrop ReleasePlank()
        {
            if (!PlankHeld) return ImaginaryBridgeDrop.None;
            PlankHeld = false;
            bool moved = Abs(PlankX - grabX) >= MoveMin || Abs(PlankY - grabY) >= MoveMin;
            if (InputLocked) { PlankY = CliffY; return ImaginaryBridgeDrop.None; }
            return Settle(moved, false);
        }

        /// <summary>Tocco (senza trascinare) sull'asse: solo una battuta (mai una vittoria). Non mentre cade.</summary>
        public bool TapPlank()
        {
            if (InputLocked || PlankHeld || Pinching || Falling) return false;
            PlankTaps++;
            return true;
        }

        // ---------- L'asse: pizzico (la soluzione) ----------

        /// <summary>Due dita sull'asse: comincia il pizzico (agisce solo sull'asse, che resta dov'è; il trascinamento diventa pizzico).</summary>
        public bool StartPinch()
        {
            if (InputLocked || Pinching || Falling) return false;
            if (!PlankHeld) { grabX = PlankX; grabY = PlankY; }
            PlankHeld = false;
            Pinching = true;
            pinchStart = Length;
            return true;
        }

        /// <summary>
        /// Rapporto distanza attuale / iniziale delle dita: lunghezza = iniziale × rapporto, sempre fra minimo e massimo;
        /// il centro resta fermo (spostato solo quanto serve perché i capi restino nello schermo).
        /// </summary>
        public float Pinch(float ratio)
        {
            if (InputLocked || !Pinching || float.IsNaN(ratio) || float.IsInfinity(ratio)) return Length;
            Length = Clamp(pinchStart * ratio, MinLength, MaxLength);
            PlankX = ClampX(PlankX, Length);
            return Length;
        }

        /// <summary>
        /// Un dito del pizzico si alza. Con <paramref name="keepDragging"/> l'altro dito resta sull'asse e continua a
        /// trascinarla (dove è posata si guarda al rilascio); senza, l'asse è posata lì dov'è. La lunghezza raggiunta resta.
        /// </summary>
        public ImaginaryBridgeDrop EndPinch(bool keepDragging)
        {
            if (!Pinching) return ImaginaryBridgeDrop.None;
            Pinching = false;
            bool changed = Abs(Length - pinchStart) >= 0.02f;
            if (changed && !InputLocked) Stretches++;
            pinchStart = Length;
            if (InputLocked) { PlankY = CliffY; return ImaginaryBridgeDrop.None; }
            if (keepDragging)
            {
                PlankHeld = true;
                return changed ? ImaginaryBridgeDrop.Stretched : ImaginaryBridgeDrop.Unchanged;
            }
            bool moved = Abs(PlankX - grabX) >= MoveMin || Abs(PlankY - grabY) >= MoveMin;
            return Settle(moved, changed);
        }

        /// <summary>L'asse è lasciata: ponte (vittoria), caduta nel canyon, posata su una piattaforma o niente.</summary>
        ImaginaryBridgeDrop Settle(bool moved, bool stretched)
        {
            if (Spans) return Win();
            if (OverGap)
            {
                // sul vuoto e non fa da ponte: cade e ricompare al suo posto (la lunghezza resta)
                FallFromX = PlankX;
                FallFromY = PlankY;
                Falling = true;
                PlankX = HomeX(Length);
                PlankY = CliffY;
                if (LongEnough) { Slips++; return ImaginaryBridgeDrop.Slipped; }
                ShortFalls++;
                return ImaginaryBridgeDrop.TooShort;
            }
            PlankY = CliffY;
            if (!moved && !stretched) return ImaginaryBridgeDrop.Unchanged;
            if (LongEnough) { WrongSpots++; return ImaginaryBridgeDrop.WrongSpot; }
            return stretched ? ImaginaryBridgeDrop.Stretched : ImaginaryBridgeDrop.Placed;
        }

        ImaginaryBridgeDrop Win()
        {
            PlankHeld = Pinching = ManHeld = Falling = false;
            PlankX = BridgeX;        // lo stato stabile: orizzontale, centrata sul canyon, sul piano delle rocce
            PlankY = CliffY;
            Stage = ImaginaryBridgeStage.Celebrating;
            return ImaginaryBridgeDrop.Bridged;
        }

        /// <summary>L'animazione della caduta è finita: l'asse (già al suo posto) si può riprendere.</summary>
        public void FallDone() => Falling = false;

        // ---------- L'omino (falsa pista) ----------

        /// <summary>Tocco sull'omino: parla (mai una vittoria).</summary>
        public bool TapMan()
        {
            if (InputLocked || ManHeld) return false;
            ManTaps++;
            return true;
        }

        /// <summary>Un dito prende l'omino.</summary>
        public bool GrabMan()
        {
            if (InputLocked || ManHeld) return false;
            ManHeld = true;
            manGrabX = ManX;
            return true;
        }

        /// <summary>L'omino tenuto va in x, ma resta sulla sua piattaforma: arriva al bordo, non oltre.</summary>
        public bool MoveMan(float x)
        {
            if (InputLocked || !ManHeld) return false;
            ManX = Clamp(x, ManMinX, ManEdgeX);
            return true;
        }

        /// <summary>Il dito lascia l'omino: al bordo si rifiuta di saltare e fa un passo indietro; altrove resta lì.</summary>
        public ImaginaryBridgeDrop ReleaseMan()
        {
            if (!ManHeld) return ImaginaryBridgeDrop.None;
            ManHeld = false;
            if (InputLocked) return ImaginaryBridgeDrop.None;
            if (ManX >= ManEdgeX - EdgeNear)
            {
                Refusals++;
                ManX = ManEdgeX - StepBack;
                return ImaginaryBridgeDrop.Refused;
            }
            if (Abs(ManX - manGrabX) < MoveMin) return ImaginaryBridgeDrop.Unchanged;
            ManWalks++;
            return ImaginaryBridgeDrop.Walked;
        }

        // ---------- Dita perse e fine ----------

        /// <summary>
        /// Dita perse (pausa, app in background, livello spento): si lascia andare tutto senza vincere. L'asse scende sul
        /// piano delle rocce; se era sul vuoto torna al suo posto (niente caduta da contare). La lunghezza raggiunta resta.
        /// </summary>
        public bool Cancel()
        {
            bool any = PlankHeld || Pinching || ManHeld || Falling;
            bool wasHolding = PlankHeld || Pinching;
            PlankHeld = Pinching = ManHeld = Falling = false;
            if (InputLocked) { PlankY = CliffY; return any; }
            if (wasHolding && OverGap) PlankX = HomeX(Length);
            PlankX = ClampX(PlankX, Length);
            PlankY = CliffY;
            pinchStart = Length;
            return any;
        }

        /// <summary>Fine della festa: true una volta sola (allora il livello chiama Win).</summary>
        public bool TryComplete()
        {
            if (Stage != ImaginaryBridgeStage.Celebrating || completed) return false;
            completed = true;
            Stage = ImaginaryBridgeStage.Done;
            return true;
        }

        static float Abs(float v) => v < 0f ? -v : v;
        static float Clamp(float v, float lo, float hi)
        {
            if (hi < lo) hi = lo;
            if (float.IsNaN(v)) return lo;
            return v < lo ? lo : v > hi ? hi : v;
        }
    }
}
