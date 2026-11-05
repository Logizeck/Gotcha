using System;

namespace Gotcha.Levels.V64
{
    /// <summary>Fasi del livello "The Sad Balloon".</summary>
    public enum SadBalloonStage
    {
        Playing,      // si prova di tutto (il palloncino è legato al sasso)
        Celebrating,  // il nodo si è sciolto: il palloncino sorride e vola via (input bloccato)
        Done,         // vittoria già data
    }

    /// <summary>Risultato di un movimento del sasso.</summary>
    public enum SadBalloonStone
    {
        None,     // input bloccato o sasso non tenuto
        Moving,   // il sasso si sposta, il nodo tiene ancora
        Untied,   // il sasso è andato abbastanza lontano: il nodo si scioglie (VITTORIA, una volta sola)
    }

    /// <summary>Risultato del rilascio di un gesto.</summary>
    public enum SadBalloonDrop
    {
        None,         // niente da rilasciare (o input bloccato)
        FellBack,     // palloncino tirato su e lasciato: lo spago lo riporta giù ("It wants to... it can't.")
        SlidBack,     // sasso lasciato prima della soglia: torna al suo posto, nodo ancora legato
        BiggerStuck,  // pizzico finito dopo averlo gonfiato: si sgonfia ("Bigger, but still stuck.")
        Unchanged,    // pizzico finito senza averlo gonfiato davvero
    }

    /// <summary>
    /// Logica pura del livello 27 della roadmap "The Sad Balloon" (id sad_balloon) — "MAKE THE BALLOON FLY." Un
    /// palloncino rosso triste è legato con uno spago a un sassolino sul prato (il nodo intorno al sasso si vede).
    /// Inganno: trascinare il palloncino in su tende lo spago (lo spostamento verso l'alto è frenato e non supera mai
    /// <see cref="MaxStretch"/>) e al rilascio ricade; pizzicarlo con due dita lo gonfia un po' (al massimo
    /// <see cref="MaxPinchScale"/>) e alla fine si sgonfia; toccarlo lo fa sospirare. Tutto reversibile, mai una vittoria.
    /// (Niente microfono: il soffio è stato lasciato fuori di proposito, bastano i gesti col dito.)
    /// Soluzione esatta: trascinare il SASSO lontano dalla sua casa: appena la distanza supera <see cref="UntieDistance"/>
    /// DURANTE il trascinamento il nodo si scioglie subito (riscontro immediato) e il palloncino vola via. Lasciato prima
    /// della soglia, il sasso torna al suo posto e il nodo resta legato. Finché il nodo è legato il palloncino non vola
    /// mai (<see cref="CanFly"/> falso, salita limitata dallo spago).
    /// Palloncino, pizzico e sasso possono essere tenuti da dita diverse (al massimo due); Cancel() (pausa, app in
    /// background, livello spento) lascia andare tutto senza sciogliere niente. Non si può perdere. Nessuna dipendenza da Unity.
    /// </summary>
    public sealed class SadBalloonLogic
    {
        /// <summary>Distanza (unità del mondo) del sasso dalla sua casa oltre la quale il nodo si scioglie.</summary>
        public const float UntieDistance = 1.0f;
        /// <summary>Di quanto al massimo lo spago lascia salire il palloncino tirato (frenato, mai raggiunto).</summary>
        public const float MaxStretch = 0.9f;
        /// <summary>Di quanto al massimo il palloncino si può tirare giù (lo spago si allenta).</summary>
        public const float MaxDip = 0.8f;
        /// <summary>Di quanto al massimo il palloncino si sposta di lato.</summary>
        public const float MaxSway = 1.0f;
        /// <summary>Spostamento minimo perché una tirata conti come tentativo (sotto è solo un tocco lungo).</summary>
        public const float PullMin = 0.15f;
        /// <summary>Gonfiaggio massimo col pizzico (1 = normale): resta nello schermo.</summary>
        public const float MaxPinchScale = 1.3f;
        /// <summary>Quanto il pizzico allargato gonfia (scala = 1 + (rapporto - 1) * guadagno).</summary>
        public const float PinchGain = 0.5f;
        /// <summary>Gonfiaggio minimo perché il pizzico conti come "più grande".</summary>
        public const float PinchCounts = 1.06f;

        public SadBalloonStage Stage { get; private set; } = SadBalloonStage.Playing;
        /// <summary>Il nodo è ancora legato al sasso.</summary>
        public bool Tied { get; private set; } = true;
        /// <summary>Il palloncino può volare (solo a nodo sciolto).</summary>
        public bool CanFly => !Tied;
        public bool InputLocked => Stage != SadBalloonStage.Playing;

        public bool BalloonHeld { get; private set; }
        public bool StoneHeld { get; private set; }
        public bool Pinching { get; private set; }
        /// <summary>Spostamento attuale del palloncino tirato (rispetto al riposo), già limitato dallo spago.</summary>
        public float BalloonX { get; private set; }
        public float BalloonY { get; private set; }
        /// <summary>Distanza attuale del sasso dalla sua casa.</summary>
        public float StoneDistance { get; private set; }
        /// <summary>Scala voluta del palloncino per il pizzico (1 = normale).</summary>
        public float PinchScale { get; private set; } = 1f;
        float pinchPeak = 1f;

        public int BalloonPulls { get; private set; }
        public int Pinches { get; private set; }
        public int ShortDrags { get; private set; }
        public int Sighs { get; private set; }
        public int StoneTaps { get; private set; }
        public int WrongMoves => BalloonPulls + Pinches + ShortDrags;

        bool completed;

        public void Reset()
        {
            Stage = SadBalloonStage.Playing;
            Tied = true;
            BalloonHeld = StoneHeld = Pinching = false;
            BalloonX = BalloonY = StoneDistance = 0f;
            PinchScale = pinchPeak = 1f;
            BalloonPulls = Pinches = ShortDrags = Sighs = StoneTaps = 0;
            completed = false;
        }

        // ---------- Il palloncino (false piste) ----------

        /// <summary>Tocco sul palloncino: sospira (mai una vittoria).</summary>
        public bool TapBalloon()
        {
            if (InputLocked || Pinching) return false;
            Sighs++;
            return true;
        }

        /// <summary>Un dito comincia a tirare il palloncino.</summary>
        public bool GrabBalloon()
        {
            if (InputLocked || BalloonHeld || Pinching) return false;
            BalloonHeld = true;
            BalloonX = BalloonY = 0f;
            return true;
        }

        /// <summary>
        /// Il dito tira il palloncino di (dx, dy) dal riposo: lo spago lo trattiene. In su la salita è frenata e resta
        /// sempre sotto <see cref="MaxStretch"/>; in giù al massimo <see cref="MaxDip"/>; di lato al massimo
        /// <see cref="MaxSway"/>. Ritorna false se il palloncino non è tenuto.
        /// </summary>
        public bool PullBalloon(float dx, float dy)
        {
            if (InputLocked || !BalloonHeld) return false;
            BalloonX = Clamp(dx, -MaxSway, MaxSway);
            BalloonY = Stretch(dy);
            return true;
        }

        /// <summary>Salita permessa dallo spago per una tirata di dy (frenata in su, limitata in giù).</summary>
        public static float Stretch(float dy)
        {
            if (float.IsNaN(dy)) return 0f;
            if (dy <= 0f) return dy < -MaxDip ? -MaxDip : dy;
            return MaxStretch * (float)(1.0 - Math.Exp(-dy / MaxStretch)) * 0.999f;
        }

        /// <summary>Il dito lascia il palloncino: ricade (lo spago lo riporta al riposo).</summary>
        public SadBalloonDrop ReleaseBalloon()
        {
            if (!BalloonHeld) return SadBalloonDrop.None;
            BalloonHeld = false;
            bool pulled = Abs(BalloonX) >= PullMin || Abs(BalloonY) >= PullMin;
            BalloonX = BalloonY = 0f;
            if (InputLocked) return SadBalloonDrop.None;
            if (!pulled) return SadBalloonDrop.Unchanged;
            BalloonPulls++;
            return SadBalloonDrop.FellBack;
        }

        // ---------- Il pizzico (falsa pista) ----------

        /// <summary>Due dita sul palloncino: comincia il pizzico (una tirata in corso si lascia andare, senza contarla).</summary>
        public bool StartPinch()
        {
            if (InputLocked || Pinching) return false;
            BalloonHeld = false;
            BalloonX = BalloonY = 0f;
            Pinching = true;
            PinchScale = pinchPeak = 1f;
            return true;
        }

        /// <summary>Le dita si allargano (rapporto distanza attuale / iniziale): il palloncino si gonfia un po', fino al massimo.</summary>
        public float Pinch(float ratio)
        {
            if (InputLocked || !Pinching || float.IsNaN(ratio)) return PinchScale;
            PinchScale = Clamp(1f + (ratio - 1f) * PinchGain, 1f, MaxPinchScale);
            if (PinchScale > pinchPeak) pinchPeak = PinchScale;
            return PinchScale;
        }

        /// <summary>Un dito del pizzico si alza: il palloncino si sgonfia (torna normale).</summary>
        public SadBalloonDrop EndPinch()
        {
            if (!Pinching) return SadBalloonDrop.None;
            Pinching = false;
            bool grew = pinchPeak >= PinchCounts;
            PinchScale = pinchPeak = 1f;
            if (InputLocked) return SadBalloonDrop.None;
            if (!grew) return SadBalloonDrop.Unchanged;
            Pinches++;
            return SadBalloonDrop.BiggerStuck;
        }

        // ---------- Il sasso (la soluzione) ----------

        /// <summary>Tocco (senza trascinare) sul sasso.</summary>
        public bool TapStone()
        {
            if (InputLocked || StoneHeld) return false;
            StoneTaps++;
            return true;
        }

        /// <summary>Un dito comincia a trascinare il sasso.</summary>
        public bool GrabStone()
        {
            if (InputLocked || StoneHeld || !Tied) return false;
            StoneHeld = true;
            StoneDistance = 0f;
            return true;
        }

        /// <summary>
        /// Il sasso è a (dx, dy) dalla sua casa. Oltre <see cref="UntieDistance"/> il nodo si scioglie SUBITO (durante il
        /// trascinamento): <see cref="SadBalloonStone.Untied"/> una volta sola, poi l'input è bloccato.
        /// </summary>
        public SadBalloonStone MoveStone(float dx, float dy)
        {
            if (InputLocked || !StoneHeld) return SadBalloonStone.None;
            float d = (float)Math.Sqrt(dx * dx + dy * dy);
            StoneDistance = float.IsNaN(d) ? 0f : d;
            if (StoneDistance < UntieDistance) return SadBalloonStone.Moving;
            Tied = false;
            StoneHeld = BalloonHeld = Pinching = false;
            BalloonX = BalloonY = 0f;
            PinchScale = pinchPeak = 1f;
            Stage = SadBalloonStage.Celebrating;
            return SadBalloonStone.Untied;
        }

        /// <summary>Il sasso viene lasciato prima della soglia: torna al suo posto, il nodo resta legato.</summary>
        public SadBalloonDrop ReleaseStone()
        {
            if (!StoneHeld) return SadBalloonDrop.None;
            StoneHeld = false;
            StoneDistance = 0f;
            if (InputLocked) return SadBalloonDrop.None;
            ShortDrags++;
            return SadBalloonDrop.SlidBack;
        }

        // ---------- Dita perse e fine ----------

        /// <summary>Dita perse (pausa, app in background, livello spento): si lascia andare tutto, il nodo NON si scioglie.</summary>
        public bool Cancel()
        {
            bool any = BalloonHeld || StoneHeld || Pinching;
            BalloonHeld = StoneHeld = Pinching = false;
            BalloonX = BalloonY = StoneDistance = 0f;
            PinchScale = pinchPeak = 1f;
            return any;
        }

        /// <summary>Fine della festa: true una volta sola (allora il livello chiama Win).</summary>
        public bool TryComplete()
        {
            if (Stage != SadBalloonStage.Celebrating || completed) return false;
            completed = true;
            Stage = SadBalloonStage.Done;
            return true;
        }

        static float Abs(float v) => v < 0f ? -v : v;
        static float Clamp(float v, float lo, float hi) => float.IsNaN(v) ? (lo > 0f ? lo : hi < 0f ? hi : 0f) : v < lo ? lo : v > hi ? hi : v;
    }
}
