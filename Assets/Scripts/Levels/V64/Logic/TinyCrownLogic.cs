using System;

namespace Gotcha.Levels.V64
{
    /// <summary>Fasi del livello "The Tiny Crown".</summary>
    public enum TinyCrownStage
    {
        Playing,      // si prova di tutto (la corona minuscola è sul cuscino)
        Celebrating,  // la corona grande è sulla testa: il re si inginocchia (input bloccato)
        Done,         // vittoria già data
    }

    /// <summary>Risultato del rilascio della corona (fine del trascinamento o fine del pizzico senza dita rimaste).</summary>
    public enum TinyCrownDrop
    {
        None,      // niente da rilasciare (o input bloccato)
        TooSmall,  // lasciata sulla testa ma ancora troppo piccola: scivola giù e torna sul cuscino (tiene la sua misura)
        Placed,    // lasciata lontano dalla testa: resta dove è stata lasciata (tiene la sua misura)
        Crowned,   // grande abbastanza e sulla testa: si aggancia da sola (VITTORIA, una volta sola)
    }

    /// <summary>
    /// Logica pura del livello 63 della roadmap "The Tiny Crown" (id tiny_crown) — "CROWN THE KING." Un re dalla testa
    /// ENORME (barbone) sul trono e, accanto, una corona minuscola su un cuscino.
    /// Inganno: trascinare la corona così com'è sulla testa non funziona: è troppo piccola, scivola giù dalla testona e
    /// torna sul cuscino ("It's a bit... small."). Toccare il re: "Where is my crown?".
    /// Soluzione esatta: allargare la corona col pizzico a due dita finché la scala arriva ad almeno
    /// <see cref="FitScale"/> (allora brilla, <see cref="BigEnough"/>), poi trascinarla sulla testa: dentro la zona generosa
    /// intorno alla cima della testa (<see cref="OnHead"/>) si aggancia da sola al posto giusto e il re si inginocchia.
    /// La scala è sempre limitata a [<see cref="MinScale"/>, <see cref="MaxScale"/>]; il pizzico agisce solo sulla corona.
    /// Dal pizzico al trascinamento: alzando un dito del pizzico, l'altro continua a trascinare la corona (nessun blocco,
    /// nessun salto: lo scarto dito-corona viene ricalcolato). Corona grande lasciata lontano dalla testa: resta dove è
    /// stata lasciata con la sua misura (scelta documentata), sempre dentro l'area sicura, quindi si può sempre riprendere.
    /// Cancel() (pausa, app in background, livello spento) molla tutto senza vincere (sopra la testa torna sul cuscino).
    /// Non si può perdere. Nessuna dipendenza da Unity. Coordinate del mondo.
    /// </summary>
    public sealed class TinyCrownLogic
    {
        /// <summary>Scala iniziale (la corona minuscola) e minima.</summary>
        public const float MinScale = 1f;
        /// <summary>Scala massima del pizzico.</summary>
        public const float MaxScale = 3f;
        /// <summary>Scala minima perché la corona vada bene alla testa (da qui brilla).</summary>
        public const float FitScale = 2.2f;
        /// <summary>Scala con cui la corona si aggancia sulla testa (misura giusta, qualunque fosse quella del rilascio).</summary>
        public const float CrownedScale = 2.6f;
        /// <summary>Distanza minima tra le dita all'inizio del pizzico (rapporti stabili anche con dita vicinissime).</summary>
        public const float MinPinchDistance = 0.25f;
        /// <summary>Zona di aggancio intorno alla cima della testa: mezza larghezza, quanto sotto e quanto sopra.</summary>
        public const float SnapHalfWidth = 1.15f, SnapBelow = 1.5f, SnapAbove = 1.0f;
        /// <summary>Area sicura per il centro della corona (resta sempre nello schermo e sotto le scritte in alto).</summary>
        public const float MinX = -2.2f, MaxX = 2.2f, MinY = -3.9f, MaxY = 2.5f;
        /// <summary>Spostamento minimo perché un trascinamento conti come tentativo.</summary>
        public const float MoveMin = 0.15f;

        public TinyCrownStage Stage { get; private set; } = TinyCrownStage.Playing;
        public bool InputLocked => Stage != TinyCrownStage.Playing;

        /// <summary>Centro della corona (mondo).</summary>
        public float X { get; private set; }
        public float Y { get; private set; }
        /// <summary>Scala attuale della corona (1 = minuscola).</summary>
        public float Scale { get; private set; } = MinScale;
        /// <summary>Grande abbastanza per la testa del re.</summary>
        public bool BigEnough => Scale >= FitScale - 1e-4f;
        /// <summary>La corona è sulla testa (posizione di aggancio raggiunta, solo a vittoria).</summary>
        public bool Crowned { get; private set; }

        public bool Held { get; private set; }
        public bool Pinching { get; private set; }

        public float HomeX { get; private set; }
        public float HomeY { get; private set; }
        public float HeadTopX { get; private set; }
        public float HeadTopY { get; private set; }

        public int SmallTries { get; private set; }
        public int OffHeadDrops { get; private set; }
        public int Pinches { get; private set; }
        public int KingTaps { get; private set; }
        public int CrownTaps { get; private set; }
        public int WrongMoves => SmallTries + OffHeadDrops;
        /// <summary>Scala della corona all'ultimo tentativo troppo piccolo (per la battuta "quasi").</summary>
        public float LastTryScale { get; private set; } = MinScale;

        float offX, offY, grabX, grabY, pinchD0 = 1f, pinchS0 = 1f, pinchPeakDelta;
        bool moved, completed;

        public TinyCrownLogic() : this(1.65f, -1.28f, -0.5f, 1.8f) { }

        /// <summary>home: centro della corona sul cuscino; headTop: cima della testa del re.</summary>
        public TinyCrownLogic(float homeX, float homeY, float headTopX, float headTopY)
        {
            HomeX = homeX; HomeY = homeY; HeadTopX = headTopX; HeadTopY = headTopY;
            Reset();
        }

        public void Reset()
        {
            Stage = TinyCrownStage.Playing;
            X = HomeX; Y = HomeY;
            Scale = MinScale;
            Crowned = Held = Pinching = moved = completed = false;
            offX = offY = grabX = grabY = pinchPeakDelta = 0f;
            pinchD0 = pinchS0 = 1f;
            SmallTries = OffHeadDrops = Pinches = KingTaps = CrownTaps = 0;
            LastTryScale = MinScale;
        }

        /// <summary>Il punto (x, y) è nella zona di aggancio intorno alla cima della testa.</summary>
        public bool OnHead(float x, float y)
        {
            float dx = x - HeadTopX, dy = y - HeadTopY;
            return Abs(dx) <= SnapHalfWidth && dy >= -SnapBelow && dy <= SnapAbove;
        }

        /// <summary>La corona in questo momento è sopra la zona della testa.</summary>
        public bool OverHead => OnHead(X, Y);

        // ---------- Tocchi (false piste) ----------

        /// <summary>Tocco sul re: "Where is my crown?" (mai una vittoria).</summary>
        public bool TapKing()
        {
            if (InputLocked) return false;
            KingTaps++;
            return true;
        }

        /// <summary>Tocco (senza trascinare) sulla corona.</summary>
        public bool TapCrown()
        {
            if (InputLocked || Held || Pinching) return false;
            CrownTaps++;
            return true;
        }

        // ---------- Trascinamento ----------

        /// <summary>Un dito prende la corona nel punto (fx, fy): si ricorda lo scarto, così la corona non salta.</summary>
        public bool Grab(float fx, float fy)
        {
            if (InputLocked || Held || Pinching || Bad(fx) || Bad(fy)) return false;
            Held = true;
            moved = false;
            offX = X - fx; offY = Y - fy;
            grabX = fx; grabY = fy;
            return true;
        }

        /// <summary>Il dito si sposta in (fx, fy): la corona lo segue, sempre dentro l'area sicura.</summary>
        public bool Drag(float fx, float fy)
        {
            if (InputLocked || !Held || Bad(fx) || Bad(fy)) return false;
            X = Clamp(fx + offX, MinX, MaxX);
            Y = Clamp(fy + offY, MinY, MaxY);
            if (Abs(fx - grabX) >= MoveMin || Abs(fy - grabY) >= MoveMin) moved = true;
            return true;
        }

        /// <summary>Il dito lascia la corona: si valuta dove e quanto è grande.</summary>
        public TinyCrownDrop Release()
        {
            if (!Held) return TinyCrownDrop.None;
            Held = false;
            if (InputLocked) return TinyCrownDrop.None;
            return Drop(moved);
        }

        // ---------- Pizzico (agisce solo sulla corona) ----------

        /// <summary>
        /// Due dita sulla corona, a distanza d: comincia il pizzico. Un trascinamento in corso si interrompe senza
        /// valutarlo (la corona resta dove si trova).
        /// </summary>
        public bool StartPinch(float distance)
        {
            if (InputLocked || Pinching || Bad(distance)) return false;
            Held = false;
            Pinching = true;
            pinchD0 = Math.Max(MinPinchDistance, distance);
            pinchS0 = Scale;
            pinchPeakDelta = 0f;
            return true;
        }

        /// <summary>Le dita sono a distanza d: la scala segue il rapporto d / d0, limitata a [MinScale, MaxScale].</summary>
        public float Pinch(float distance)
        {
            if (InputLocked || !Pinching || Bad(distance)) return Scale;
            Scale = Clamp(pinchS0 * Math.Max(0f, distance) / pinchD0, MinScale, MaxScale);
            float d = Abs(Scale - pinchS0);
            if (d > pinchPeakDelta) pinchPeakDelta = d;
            return Scale;
        }

        /// <summary>
        /// Un dito del pizzico si alza. Con <paramref name="keepFinger"/> l'altro dito (in fx, fy) continua a trascinare
        /// la corona (scarto ricalcolato: niente salti, niente blocchi) e il rilascio si valuterà quando si alza anche lui.
        /// Senza dita rimaste la corona viene "lasciata" dov'è (stesse regole del rilascio).
        /// </summary>
        public TinyCrownDrop EndPinch(bool keepFinger, float fx = 0f, float fy = 0f)
        {
            if (!Pinching) return TinyCrownDrop.None;
            Pinching = false;
            if (pinchPeakDelta >= 0.05f) Pinches++;
            pinchPeakDelta = 0f;
            if (InputLocked) return TinyCrownDrop.None;
            if (keepFinger && !Bad(fx) && !Bad(fy))
            {
                Held = true;
                moved = true;   // la corona era già "in mano": il rilascio conta
                offX = X - fx; offY = Y - fy;
                grabX = fx; grabY = fy;
                return TinyCrownDrop.None;
            }
            return Drop(false);   // pizzico finito sul posto: non è un tentativo sbagliato
        }

        // ---------- Valutazione ----------

        TinyCrownDrop Drop(bool counts)
        {
            if (OverHead)
            {
                if (BigEnough)
                {
                    Crowned = true;
                    Held = Pinching = false;
                    X = HeadTopX; Y = HeadTopY;
                    Scale = CrownedScale;
                    Stage = TinyCrownStage.Celebrating;
                    return TinyCrownDrop.Crowned;
                }
                // troppo piccola: scivola giù dalla testona e torna sul cuscino (con la misura che ha)
                SmallTries++;
                LastTryScale = Scale;
                X = HomeX; Y = HomeY;
                return TinyCrownDrop.TooSmall;
            }
            if (counts && BigEnough) OffHeadDrops++;
            return TinyCrownDrop.Placed;
        }

        // ---------- Dita perse e fine ----------

        /// <summary>
        /// Dita perse (pausa, app in background, livello spento): si molla tutto senza vincere. La corona resta dov'è e
        /// tiene la sua misura; se era sopra la testa torna sul cuscino (così non resta mai appoggiata sulla testa senza
        /// vittoria: stato sempre chiaro). Nessun contatore cambia.
        /// </summary>
        public bool Cancel()
        {
            bool any = Held || Pinching;
            Held = Pinching = false;
            pinchPeakDelta = 0f;
            if (!InputLocked && OverHead) { X = HomeX; Y = HomeY; }
            return any;
        }

        /// <summary>Fine della festa: true una volta sola (allora il livello chiama Win).</summary>
        public bool TryComplete()
        {
            if (Stage != TinyCrownStage.Celebrating || completed) return false;
            completed = true;
            Stage = TinyCrownStage.Done;
            return true;
        }

        static bool Bad(float v) => float.IsNaN(v) || float.IsInfinity(v);
        static float Abs(float v) => v < 0f ? -v : v;
        static float Clamp(float v, float lo, float hi) => float.IsNaN(v) ? lo : v < lo ? lo : v > hi ? hi : v;
    }
}
