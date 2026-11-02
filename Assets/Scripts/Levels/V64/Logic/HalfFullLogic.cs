namespace Gotcha.Levels.V64
{
    /// <summary>Fasi del livello "Half Full".</summary>
    public enum HalfFullStage
    {
        Playing,      // si prova di tutto
        Celebrating,  // il fumetto dice "HALF FULL!": sorriso, scintille, festa (input bloccato)
        Done,         // vittoria già data
    }

    /// <summary>Dov'è la parola EMPTY (nel fumetto all'inizio).</summary>
    public enum HalfFullEmptyAt
    {
        Bubble,   // al suo posto: "HALF EMPTY!"
        Held,     // il dito la sta trascinando
        Floor,    // lasciata fuori dal fumetto: è caduta a terra (si può riprendere)
    }

    /// <summary>Dov'è la parola FULL (sul cartello "STAY FULL" all'inizio).</summary>
    public enum HalfFullFullAt
    {
        Sign,     // sul cartello
        Held,     // il dito la sta trascinando
        Bubble,   // nel buco del fumetto: "HALF FULL!" (vittoria)
    }

    /// <summary>Cosa c'è nel posto della parola dopo HALF.</summary>
    public enum HalfFullSlot
    {
        Empty,    // "HALF EMPTY!"
        Gap,      // "HALF ___!"
        Full,     // "HALF FULL!"
    }

    /// <summary>EMPTY lasciata andare.</summary>
    public enum HalfFullEmptyDrop
    {
        None,       // input bloccato o EMPTY non tenuta
        Returned,   // lasciata dentro il fumetto: torna al suo posto
        Floor,      // lasciata fuori: cade a terra, il fumetto mostra il buco (la prima volta il pessimista si stupisce)
    }

    /// <summary>FULL lasciata andare.</summary>
    public enum HalfFullFullDrop
    {
        None,       // input bloccato o FULL non tenuta
        Returned,   // lasciata fuori dal buco: torna sul cartello
        Placed,     // nel buco (EMPTY era già via): "HALF FULL!" -> vittoria
        Swapped,    // sopra EMPTY ancora al suo posto: regola ACCETTA, EMPTY cade a terra e FULL prende il posto -> vittoria
    }

    /// <summary>Acqua versata nel bicchiere (falsa pista).</summary>
    public enum HalfFullPour
    {
        None,       // input bloccato
        Higher,     // l'acqua sale: "It's still half EMPTY. Just... higher."
        Brim,       // pieno fino all'orlo: "Now it's going to spill." (poi l'acqua torna a metà da sola)
        Spilling,   // sta già traboccando: niente di nuovo
    }

    /// <summary>
    /// Logica pura del livello 10 della roadmap "Half Full" — "MAKE THE PESSIMIST HAPPY." Un omino triste guarda un
    /// bicchiere pieno a metà; il suo fumetto dice "HALF EMPTY!" (HALF ed EMPTY sono parole separate); sul muro un
    /// cartellino "STAY FULL" (STAY e FULL separate, FULL si può trascinare).
    /// Inganno: versare acqua o portargli il bicchiere non cambia niente ("It's still half EMPTY. Just... higher."); pieno
    /// fino all'orlo "Now it's going to spill." e l'acqua torna a metà (reversibile).
    /// Soluzione: trascinare EMPTY fuori dal fumetto (cade a terra, resta il buco "HALF ___!") e poi FULL dal cartello
    /// nel buco -> "HALF FULL!" -> vittoria (una volta sola).
    /// Regola documentata (ACCETTA, generosa): FULL lasciata sopra EMPTY ancora al suo posto la sostituisce (EMPTY cade).
    /// EMPTY lasciata dentro il fumetto torna al suo posto; FULL lasciata fuori dal buco torna al cartello. HALF e STAY
    /// non si trascinano. Dito perso (pausa, app in background): la parola tenuta torna da dove era partita.
    /// Il buco e le zone di rilascio si calcolano dalle LARGHEZZE delle parole (vedi <see cref="Row"/>, <see cref="InSlot"/>),
    /// così funzionano anche con traduzioni più lunghe o più corte. Non si può perdere. Nessuna dipendenza da Unity.
    /// </summary>
    public sealed class HalfFullLogic
    {
        /// <summary>Gradini d'acqua sopra la metà: all'ultimo il bicchiere è pieno fino all'orlo.</summary>
        public const int PourSteps = 3;
        /// <summary>Secondi in cui il bicchiere trabocca prima di tornare a metà.</summary>
        public const float SpillSeconds = 1.4f;
        /// <summary>Margine orizzontale della zona di rilascio attorno al buco (unità del mondo).</summary>
        public const float SlotMarginX = 0.35f;
        /// <summary>Margine verticale della zona di rilascio attorno al buco (unità del mondo).</summary>
        public const float SlotMarginY = 0.4f;
        /// <summary>Larghezza minima del buco (anche con parole cortissime resta un bersaglio comodo).</summary>
        public const float MinSlotWidth = 0.6f;

        public HalfFullStage Stage { get; private set; } = HalfFullStage.Playing;
        public HalfFullEmptyAt EmptyAt { get; private set; } = HalfFullEmptyAt.Bubble;
        public HalfFullFullAt FullAt { get; private set; } = HalfFullFullAt.Sign;
        /// <summary>Gradini d'acqua sopra la metà (0 = mezzo bicchiere).</summary>
        public int Water { get; private set; }
        public bool Spilling { get; private set; }
        public int Pours { get; private set; }
        public int GlassOffers { get; private set; }
        public int ManTaps { get; private set; }
        public int SignTaps { get; private set; }
        public int StuckWordTaps { get; private set; }
        public int EmptyReturns { get; private set; }
        public int FullReturns { get; private set; }
        public int EmptyDrops { get; private set; }
        public int WrongMoves => Pours + GlassOffers + ManTaps + SignTaps + StuckWordTaps + EmptyReturns + FullReturns;
        public bool InputLocked => Stage != HalfFullStage.Playing;
        public bool Holding => EmptyAt == HalfFullEmptyAt.Held || FullAt == HalfFullFullAt.Held;
        /// <summary>Riempimento del bicchiere (0..1): 0.5 all'inizio, 1 pieno fino all'orlo.</summary>
        public float WaterFraction => 0.5f + 0.5f * Water / PourSteps;

        public HalfFullSlot Slot =>
            FullAt == HalfFullFullAt.Bubble ? HalfFullSlot.Full :
            EmptyAt == HalfFullEmptyAt.Bubble ? HalfFullSlot.Empty : HalfFullSlot.Gap;

        HalfFullEmptyAt emptyFrom = HalfFullEmptyAt.Bubble;
        float spillTimer;
        bool completed;

        public void Reset()
        {
            Stage = HalfFullStage.Playing;
            EmptyAt = HalfFullEmptyAt.Bubble;
            FullAt = HalfFullFullAt.Sign;
            emptyFrom = HalfFullEmptyAt.Bubble;
            Water = 0;
            Spilling = false;
            spillTimer = 0f;
            Pours = GlassOffers = ManTaps = SignTaps = StuckWordTaps = EmptyReturns = FullReturns = EmptyDrops = 0;
            completed = false;
        }

        /// <summary>Il tempo passa: il bicchiere che trabocca torna a metà dopo <see cref="SpillSeconds"/>. True quando torna.</summary>
        public bool Tick(float dt)
        {
            if (!Spilling || dt <= 0f) return false;
            spillTimer += dt;
            if (spillTimer < SpillSeconds) return false;
            Spilling = false;
            spillTimer = 0f;
            Water = 0;
            return true;
        }

        // ---------- Le false piste ----------

        /// <summary>Acqua dalla brocca (o tocco sul bicchiere): sale di un gradino; all'orlo trabocca e poi torna a metà.</summary>
        public HalfFullPour Pour()
        {
            if (InputLocked) return HalfFullPour.None;
            if (Spilling) return HalfFullPour.Spilling;
            Pours++;
            Water++;
            if (Water < PourSteps) return HalfFullPour.Higher;
            Water = PourSteps;
            Spilling = true;
            spillTimer = 0f;
            return HalfFullPour.Brim;
        }

        /// <summary>Il bicchiere portato all'omino: niente da fare, resta pessimista.</summary>
        public bool OfferGlass()
        {
            if (InputLocked) return false;
            GlassOffers++;
            return true;
        }

        public bool TapMan()
        {
            if (InputLocked) return false;
            ManTaps++;
            return true;
        }

        public bool TapSign()
        {
            if (InputLocked) return false;
            SignTaps++;
            return true;
        }

        /// <summary>Tocco (o tentativo di trascinare) HALF, "!" o STAY: sono incollate, oscillano e basta.</summary>
        public bool TapStuckWord()
        {
            if (InputLocked) return false;
            StuckWordTaps++;
            return true;
        }

        // ---------- Le parole ----------

        /// <summary>Il dito prende EMPTY (dal fumetto o da terra).</summary>
        public bool GrabEmpty()
        {
            if (InputLocked || Holding || EmptyAt == HalfFullEmptyAt.Held) return false;
            emptyFrom = EmptyAt;
            EmptyAt = HalfFullEmptyAt.Held;
            return true;
        }

        /// <summary>EMPTY lasciata: dentro il fumetto torna al suo posto, fuori cade a terra.</summary>
        public HalfFullEmptyDrop ReleaseEmpty(bool insideBubble)
        {
            if (InputLocked || EmptyAt != HalfFullEmptyAt.Held) return HalfFullEmptyDrop.None;
            if (insideBubble)
            {
                EmptyAt = HalfFullEmptyAt.Bubble;
                if (emptyFrom == HalfFullEmptyAt.Bubble) EmptyReturns++;
                return HalfFullEmptyDrop.Returned;
            }
            EmptyAt = HalfFullEmptyAt.Floor;
            EmptyDrops++;
            return HalfFullEmptyDrop.Floor;
        }

        /// <summary>Il dito prende FULL dal cartello.</summary>
        public bool GrabFull()
        {
            if (InputLocked || Holding || FullAt != HalfFullFullAt.Sign) return false;
            FullAt = HalfFullFullAt.Held;
            return true;
        }

        /// <summary>FULL lasciata: nel buco vince (anche sopra EMPTY, che cade: regola ACCETTA), altrove torna al cartello.</summary>
        public HalfFullFullDrop ReleaseFull(bool inSlot)
        {
            if (InputLocked || FullAt != HalfFullFullAt.Held) return HalfFullFullDrop.None;
            if (!inSlot)
            {
                FullAt = HalfFullFullAt.Sign;
                FullReturns++;
                return HalfFullFullDrop.Returned;
            }
            bool swap = EmptyAt == HalfFullEmptyAt.Bubble;
            if (swap) EmptyAt = HalfFullEmptyAt.Floor;
            FullAt = HalfFullFullAt.Bubble;
            Stage = HalfFullStage.Celebrating;
            Spilling = false;
            spillTimer = 0f;
            Water = 0;
            return swap ? HalfFullFullDrop.Swapped : HalfFullFullDrop.Placed;
        }

        /// <summary>Dito perso a metà presa (pausa, app in background, livello spento): la parola torna da dove era partita.</summary>
        public bool Cancel()
        {
            bool any = false;
            if (EmptyAt == HalfFullEmptyAt.Held) { EmptyAt = emptyFrom; any = true; }
            if (FullAt == HalfFullFullAt.Held) { FullAt = HalfFullFullAt.Sign; any = true; }
            return any;
        }

        /// <summary>Fine della festa: true una volta sola (allora il livello chiama Win).</summary>
        public bool TryComplete()
        {
            if (Stage != HalfFullStage.Celebrating || completed) return false;
            completed = true;
            Stage = HalfFullStage.Done;
            return true;
        }

        // ---------- Impaginazione (dalle larghezze delle parole, mai coordinate fisse) ----------

        /// <summary>
        /// Centri delle parole di una riga centrata in 0: <paramref name="widths"/> le larghezze, <paramref name="gaps"/> lo
        /// spazio PRIMA di ciascuna parola (gaps[0] ignorato). Restituisce i centri e in <paramref name="total"/> la
        /// larghezza dell'intera riga.
        /// </summary>
        public static float[] Row(float[] widths, float[] gaps, out float total)
        {
            int n = widths == null ? 0 : widths.Length;
            var centers = new float[n];
            total = 0f;
            for (int i = 0; i < n; i++)
            {
                if (i > 0) total += gaps != null && i < gaps.Length ? Max0(gaps[i]) : 0f;
                total += Max0(widths[i]);
            }
            float x = -total * 0.5f;
            for (int i = 0; i < n; i++)
            {
                if (i > 0) x += gaps != null && i < gaps.Length ? Max0(gaps[i]) : 0f;
                float w = Max0(widths[i]);
                centers[i] = x + w * 0.5f;
                x += w;
            }
            return centers;
        }

        /// <summary>Larghezza del posto dopo HALF: la parola che c'è (EMPTY o FULL) o il buco "___", mai sotto il minimo.</summary>
        public static float SlotWidth(HalfFullSlot slot, float emptyWidth, float fullWidth, float gapWidth)
        {
            float w = slot == HalfFullSlot.Empty ? emptyWidth : slot == HalfFullSlot.Full ? fullWidth : gapWidth;
            return w < MinSlotWidth ? MinSlotWidth : w;
        }

        /// <summary>
        /// Zona di rilascio di FULL: un rettangolo centrato sul posto, largo quanto il più largo tra il posto e la parola
        /// trascinata più i margini, alto quanto la riga più i margini. (px, py) è il centro della parola trascinata.
        /// </summary>
        public static bool InSlot(float px, float py, float slotX, float slotY, float slotWidth, float wordWidth, float lineHeight)
        {
            float hw = (slotWidth > wordWidth ? slotWidth : wordWidth) * 0.5f + SlotMarginX;
            float hh = lineHeight * 0.5f + SlotMarginY;
            return Abs(px - slotX) <= hw && Abs(py - slotY) <= hh;
        }

        /// <summary>Punto dentro un rettangolo (centro, mezze misure): per il fumetto.</summary>
        public static bool InRect(float px, float py, float cx, float cy, float halfW, float halfH) =>
            Abs(px - cx) <= halfW && Abs(py - cy) <= halfH;

        static float Max0(float v) => v > 0f ? v : 0f;
        static float Abs(float v) => v < 0f ? -v : v;
    }
}
