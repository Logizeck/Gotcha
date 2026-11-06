using System;

namespace Gotcha.Levels.V64
{
    /// <summary>Fasi del livello "The Short Ladder".</summary>
    public enum ShortLadderStage
    {
        Playing,      // si prova di tutto
        Celebrating,  // scala lunga sotto la finestra: l'omino sale ed entra (input bloccato)
        Done,         // vittoria già data
    }

    /// <summary>Risultato di un gesto finito (scala posata, pizzico finito, omino lasciato, tocco).</summary>
    public enum ShortLadderDrop
    {
        None,       // niente da fare (input bloccato o gesto non in corso)
        Unchanged,  // gesto senza effetto (pizzico senza cambiare lunghezza, tocco qualunque)
        Placed,     // scala corta posata lontano dalla finestra
        Stretched,  // pizzico finito: la scala ha cambiato lunghezza (ma non è nel posto giusto)
        WrongSpot,  // scala abbastanza lunga ma posata lontano dalla finestra ("Nice ladder. Wrong spot.")
        TooShort,   // scala corta sotto la finestra: l'omino sale, non ci arriva e riscende ("Hmm. Not quite.")
        Jumped,     // omino trascinato/sollevato e lasciato: ricade a terra ("Jumping won't help.")
        Reached,    // scala lunga sotto la finestra: VITTORIA (una volta sola)
    }

    /// <summary>
    /// Logica pura del livello 72 della roadmap "The Short Ladder" (id short_ladder) — "REACH THE WINDOW." Una parete di
    /// mattoni con una finestra molto in alto, un omino a terra e una scala troppo corta appoggiata al muro.
    /// Inganno: la scala corta messa sotto la finestra non ci arriva (l'omino sale, si ferma in cima e riscende:
    /// <see cref="ShortLadderDrop.TooShort"/>); sollevarla in alto non serve (posata, i piedi tornano sempre sul
    /// pavimento); spostare o sollevare l'omino è inutile (<see cref="ShortLadderDrop.Jumped"/>). Tutto reversibile.
    /// Soluzione esatta: allungare la scala col pizzico a due dita (pioli interi da <see cref="SegmentHeight"/>, da
    /// <see cref="MinSegments"/> a <see cref="MaxSegments"/>) fino ad almeno <see cref="NeedSegments"/> pioli e posarla
    /// nella colonna della finestra (|x - <see cref="WindowX"/>| ≤ <see cref="ColumnHalfWidth"/>): allora
    /// <see cref="ShortLadderDrop.Reached"/> e l'omino sale da solo. La lunghezza resta dopo il pizzico (anche se si
    /// accorcia, mai sotto il minimo). Dal pizzico si passa al trascinamento senza blocchi: alzando un dito, l'altro
    /// continua a tenere la scala (<see cref="EndPinch"/> con keepDragging).
    /// Limiti: la scala resta nello schermo (x in [<see cref="MinX"/>, <see cref="MaxX"/>]), non passa mai sotto il
    /// pavimento e la cima non supera <see cref="MaxTopY"/>; posata (o pizzicata) ha sempre i piedi sul pavimento.
    /// Cancel() (pausa, app in background, livello spento) lascia andare tutto, senza vincere. Non si può perdere.
    /// Nessuna dipendenza da Unity.
    /// </summary>
    public sealed class ShortLadderLogic
    {
        /// <summary>Altezza di un tratto della scala (un piolo), unità del mondo.</summary>
        public const float SegmentHeight = 0.5f;
        /// <summary>Pioli all'inizio (la scala corta) e minimo dopo un pizzico che la accorcia.</summary>
        public const int MinSegments = 4;
        /// <summary>Pioli necessari per arrivare alla finestra (cima ≥ davanzale - 0.15).</summary>
        public const int NeedSegments = 10;
        /// <summary>Pioli al massimo (la cima resta sotto la barra dell'obiettivo).</summary>
        public const int MaxSegments = 13;
        /// <summary>Quanti pioli aggiunge un pizzico che raddoppia la distanza delle dita (rapporto 2).</summary>
        public const float PinchGain = 8f;

        /// <summary>Quota del pavimento (piedi della scala e dell'omino).</summary>
        public const float FloorY = -3.6f;
        /// <summary>Cima della scala al massimo (anche sollevata).</summary>
        public const float MaxTopY = 3.0f;
        /// <summary>Centro della finestra e mezza larghezza della colonna valida per la scala.</summary>
        public const float WindowX = 0.9f, ColumnHalfWidth = 0.55f;
        /// <summary>Dove può stare il centro della scala (larga 0.8: resta in x ±2.25).</summary>
        public const float MinX = -1.85f, MaxX = 1.85f;
        /// <summary>Posizione iniziale della scala (lontana dalla finestra) e dell'omino.</summary>
        public const float LadderStartX = -0.5f, ManStartX = -1.75f;
        /// <summary>Dove può stare l'omino e quanto lo si può sollevare.</summary>
        public const float ManMinX = -2.1f, ManMaxX = 2.1f, ManMaxLift = 2.5f;
        /// <summary>Spostamento minimo perché un trascinamento dell'omino conti come tentativo.</summary>
        public const float MoveMin = 0.15f;

        public ShortLadderStage Stage { get; private set; } = ShortLadderStage.Playing;
        public bool InputLocked => Stage != ShortLadderStage.Playing;

        /// <summary>Pioli attuali della scala.</summary>
        public int Segments { get; private set; } = MinSegments;
        public float Length => Segments * SegmentHeight;
        public float TopY => BottomY + Length;
        public bool LongEnough => Segments >= NeedSegments;
        /// <summary>Centro della scala (x) e piedi (y: sul pavimento, tranne mentre è tenuta sollevata).</summary>
        public float LadderX { get; private set; } = LadderStartX;
        public float BottomY { get; private set; } = FloorY;
        public bool InColumn => Abs(LadderX - WindowX) <= ColumnHalfWidth;
        /// <summary>La scala posata arriva alla finestra (lunga abbastanza, nella colonna, piedi a terra).</summary>
        public bool Reaches => LongEnough && InColumn && BottomY <= FloorY + 1e-4f;

        public bool LadderHeld { get; private set; }
        public bool Pinching { get; private set; }
        public bool ManHeld { get; private set; }
        /// <summary>L'omino sta provando a salire la scala corta (animazione del livello; finisce con ClimbAttemptDone).</summary>
        public bool ClimbAttempt { get; private set; }
        public float ManX { get; private set; } = ManStartX;
        public float ManY { get; private set; } = FloorY;

        public int ShortClimbs { get; private set; }
        public int WrongSpots { get; private set; }
        public int ManDrags { get; private set; }
        public int Stretches { get; private set; }
        public int LadderTaps { get; private set; }
        public int ManTaps { get; private set; }
        public int WrongMoves => ShortClimbs + WrongSpots + ManDrags;

        int pinchStart;
        float manGrabX, manGrabY;
        bool completed;

        public void Reset()
        {
            Stage = ShortLadderStage.Playing;
            Segments = MinSegments;
            LadderX = LadderStartX;
            BottomY = FloorY;
            LadderHeld = Pinching = ManHeld = ClimbAttempt = false;
            ManX = ManStartX;
            ManY = FloorY;
            ShortClimbs = WrongSpots = ManDrags = Stretches = LadderTaps = ManTaps = 0;
            pinchStart = MinSegments;
            completed = false;
        }

        // ---------- La scala: trascinamento ----------

        /// <summary>Un dito comincia a trascinare la scala (un tentativo di salita in corso si interrompe: l'omino scende).</summary>
        public bool GrabLadder()
        {
            if (InputLocked || LadderHeld || Pinching) return false;
            LadderHeld = true;
            ClimbAttempt = false;
            return true;
        }

        /// <summary>
        /// La scala tenuta va con il centro in x e i piedi in y (coordinate del mondo), limitata: x in
        /// [<see cref="MinX"/>, <see cref="MaxX"/>], piedi mai sotto il pavimento, cima mai sopra <see cref="MaxTopY"/>.
        /// </summary>
        public bool MoveLadder(float x, float bottomY)
        {
            if (InputLocked || !LadderHeld) return false;
            LadderX = Clamp(x, MinX, MaxX);
            BottomY = Clamp(bottomY, FloorY, MaxTopY - Length);
            return true;
        }

        /// <summary>Il dito lascia la scala: i piedi tornano sul pavimento e si guarda dove è stata posata.</summary>
        public ShortLadderDrop ReleaseLadder()
        {
            if (!LadderHeld) return ShortLadderDrop.None;
            LadderHeld = false;
            BottomY = FloorY;
            if (InputLocked) return ShortLadderDrop.None;
            return Settle(true, false);
        }

        /// <summary>Tocco (senza trascinare) sulla scala: se è già al posto giusto e lunga, basta questo per far salire l'omino.</summary>
        public ShortLadderDrop TapLadder()
        {
            if (InputLocked || LadderHeld || Pinching) return ShortLadderDrop.None;
            LadderTaps++;
            BottomY = FloorY;
            return Reaches ? Win() : ShortLadderDrop.Unchanged;
        }

        // ---------- La scala: pizzico (la soluzione) ----------

        /// <summary>Due dita sulla scala: comincia il pizzico (la scala torna con i piedi a terra; il trascinamento diventa pizzico).</summary>
        public bool StartPinch()
        {
            if (InputLocked || Pinching) return false;
            LadderHeld = false;
            ClimbAttempt = false;
            Pinching = true;
            BottomY = FloorY;
            pinchStart = Segments;
            return true;
        }

        /// <summary>
        /// Rapporto distanza attuale / iniziale delle dita: la scala si allunga (o si accorcia) di pioli interi,
        /// pioli = round(iniziali + (rapporto - 1) * <see cref="PinchGain"/>), sempre fra minimo e massimo.
        /// </summary>
        public int Pinch(float ratio)
        {
            if (InputLocked || !Pinching || float.IsNaN(ratio) || float.IsInfinity(ratio)) return Segments;
            double s = Math.Round(pinchStart + (ratio - 1.0) * PinchGain, MidpointRounding.AwayFromZero);
            Segments = s < MinSegments ? MinSegments : s > MaxSegments ? MaxSegments : (int)s;
            return Segments;
        }

        /// <summary>
        /// Un dito del pizzico si alza. Con <paramref name="keepDragging"/> l'altro dito resta sulla scala e continua a
        /// trascinarla (niente blocchi; dove è posata si guarda al rilascio); senza, la scala è posata lì dov'è.
        /// La lunghezza raggiunta resta.
        /// </summary>
        public ShortLadderDrop EndPinch(bool keepDragging)
        {
            if (!Pinching) return ShortLadderDrop.None;
            Pinching = false;
            bool changed = Segments != pinchStart;
            if (changed && !InputLocked) Stretches++;
            pinchStart = Segments;
            if (InputLocked) return ShortLadderDrop.None;
            if (keepDragging)
            {
                LadderHeld = true;
                return changed ? ShortLadderDrop.Stretched : ShortLadderDrop.Unchanged;
            }
            return Settle(false, changed);
        }

        /// <summary>La scala è posata (piedi a terra): vittoria, prova dell'omino, scala lunga nel posto sbagliato o niente.</summary>
        ShortLadderDrop Settle(bool moved, bool stretched)
        {
            BottomY = FloorY;
            if (Reaches) return Win();
            if (InColumn)
            {
                if (!moved && !stretched) return ShortLadderDrop.Unchanged;
                ShortClimbs++;
                ClimbAttempt = !ManHeld;   // tenuto dal dito, l'omino non può salire: basta la battuta
                return ShortLadderDrop.TooShort;
            }
            if (LongEnough)
            {
                if (!moved && !stretched) return ShortLadderDrop.Unchanged;
                WrongSpots++;
                return ShortLadderDrop.WrongSpot;
            }
            if (stretched) return ShortLadderDrop.Stretched;
            return moved ? ShortLadderDrop.Placed : ShortLadderDrop.Unchanged;
        }

        ShortLadderDrop Win()
        {
            LadderHeld = Pinching = ManHeld = ClimbAttempt = false;
            BottomY = FloorY;
            ManY = FloorY;
            Stage = ShortLadderStage.Celebrating;
            return ShortLadderDrop.Reached;
        }

        /// <summary>L'omino ha finito di provare a salire la scala corta (è di nuovo a terra, in x).</summary>
        public void ClimbAttemptDone(float manX)
        {
            if (!ClimbAttempt) return;
            ClimbAttempt = false;
            PlaceMan(manX);
        }

        // ---------- L'omino (falsa pista) ----------

        /// <summary>Tocco sull'omino: parla (mai una vittoria). Non mentre sale.</summary>
        public bool TapMan()
        {
            if (InputLocked || ManHeld || ClimbAttempt) return false;
            ManTaps++;
            return true;
        }

        /// <summary>Un dito prende l'omino (non mentre sta salendo).</summary>
        public bool GrabMan()
        {
            if (InputLocked || ManHeld || ClimbAttempt) return false;
            ManHeld = true;
            manGrabX = ManX;
            manGrabY = ManY;
            return true;
        }

        /// <summary>L'omino tenuto va in (x, y) (piedi), limitato: resta nello schermo e si solleva al massimo di <see cref="ManMaxLift"/>.</summary>
        public bool MoveMan(float x, float y)
        {
            if (InputLocked || !ManHeld) return false;
            ManX = Clamp(x, ManMinX, ManMaxX);
            ManY = Clamp(y, FloorY, FloorY + ManMaxLift);
            return true;
        }

        /// <summary>Il dito lascia l'omino: ricade sul pavimento lì dov'è ("Jumping won't help." se è stato spostato).</summary>
        public ShortLadderDrop ReleaseMan()
        {
            if (!ManHeld) return ShortLadderDrop.None;
            ManHeld = false;
            bool moved = Abs(ManX - manGrabX) >= MoveMin || Abs(ManY - manGrabY) >= MoveMin;
            ManY = FloorY;
            if (InputLocked) return ShortLadderDrop.None;
            if (!moved) return ShortLadderDrop.Unchanged;
            ManDrags++;
            return ShortLadderDrop.Jumped;
        }

        /// <summary>Rimette l'omino a terra in x (dopo una salita interrotta o finita). Non mentre è tenuto.</summary>
        public void PlaceMan(float x)
        {
            if (ManHeld || float.IsNaN(x)) return;
            ManX = Clamp(x, ManMinX, ManMaxX);
            ManY = FloorY;
        }

        // ---------- Dita perse e fine ----------

        /// <summary>
        /// Dita perse (pausa, app in background, livello spento): si lascia andare tutto senza vincere; scala e omino con
        /// i piedi a terra, la prova di salita si interrompe. La lunghezza raggiunta col pizzico resta.
        /// </summary>
        public bool Cancel()
        {
            bool any = LadderHeld || Pinching || ManHeld || ClimbAttempt;
            LadderHeld = Pinching = ManHeld = ClimbAttempt = false;
            BottomY = FloorY;
            ManY = FloorY;
            pinchStart = Segments;
            return any;
        }

        /// <summary>Fine della festa: true una volta sola (allora il livello chiama Win).</summary>
        public bool TryComplete()
        {
            if (Stage != ShortLadderStage.Celebrating || completed) return false;
            completed = true;
            Stage = ShortLadderStage.Done;
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
