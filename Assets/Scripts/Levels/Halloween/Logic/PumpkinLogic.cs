using System.Collections.Generic;

namespace Gotcha.Levels.Halloween
{
    public enum PumpkinPhase { Preparing, Ending, Done }

    /// <summary>Su cosa viene usato l'accendino.</summary>
    public enum PumpkinTarget { Nothing, Candle, Pumpkin, Knife, Lamp, Switch }

    public enum PumpkinSwitchResult { Ignored, TurnedOn, TurnedOff, Win }

    /// <summary>
    /// Logica pura del livello 246 "The perfect pumpkin": faccia intagliata (2 occhi + naso + bocca), candela accesa,
    /// candela dentro la zucca e, come ultima azione, luce della stanza spenta. Nessuna dipendenza da Unity.
    /// </summary>
    public sealed class PumpkinLogic
    {
        public PumpkinFaceValidator Face { get; }
        public FaceReport LastReport { get; private set; } = new FaceReport();
        public PumpkinPhase Phase { get; private set; } = PumpkinPhase.Preparing;
        public bool CandleLit { get; private set; }
        public bool CandleInside { get; private set; }
        public bool LightOn { get; private set; } = true;
        public bool CompletionReported { get; private set; }

        public bool FaceComplete => LastReport.IsComplete;
        /// <summary>Tutto pronto tranne la luce: manca solo spegnerla.</summary>
        public bool ReadyForDark => FaceComplete && CandleLit && CandleInside;
        public bool InputLocked => Phase != PumpkinPhase.Preparing;

        public PumpkinLogic(PumpkinFaceConfig config = null) { Face = new PumpkinFaceValidator(config); }

        public void Reset()
        {
            Face.Reset();
            LastReport = new FaceReport();
            Phase = PumpkinPhase.Preparing;
            CandleLit = false;
            CandleInside = false;
            LightOn = true;
            CompletionReported = false;
        }

        /// <summary>Un tratto intagliato (coordinate normalizzate della faccia). False se scartato o a gioco finito.</summary>
        public bool TryCarve(IList<HwPoint> points, out bool evictedOldest)
        {
            evictedOldest = false;
            if (InputLocked) return false;
            if (!Face.AddStroke(points, out evictedOldest)) return false;
            LastReport = Face.Evaluate();
            return true;
        }

        /// <summary>Zucca nuova: cancella l'intaglio.</summary>
        public bool TryClearCarving()
        {
            if (InputLocked) return false;
            Face.Reset();
            LastReport = new FaceReport();
            return true;
        }

        /// <summary>L'accendino accende solo la candela (e solo se è spenta); su tutto il resto non fa niente.</summary>
        public bool TryUseLighter(PumpkinTarget target)
        {
            if (InputLocked || target != PumpkinTarget.Candle || CandleLit) return false;
            CandleLit = true;
            return true;
        }

        /// <summary>La candela (una sola) entra o esce dalla zucca. True se lo stato è cambiato.</summary>
        public bool TrySetCandleInside(bool inside)
        {
            if (InputLocked || CandleInside == inside) return false;
            CandleInside = inside;
            return true;
        }

        /// <summary>Interruttore della stanza. Spegnere quando tutto il resto è pronto è la mossa vincente.</summary>
        public PumpkinSwitchResult ToggleLight()
        {
            if (InputLocked) return PumpkinSwitchResult.Ignored;
            LightOn = !LightOn;
            if (LightOn) return PumpkinSwitchResult.TurnedOn;
            if (!ReadyForDark) return PumpkinSwitchResult.TurnedOff;
            Phase = PumpkinPhase.Ending;
            return PumpkinSwitchResult.Win;
        }

        public bool TryCompleteEnding()
        {
            if (Phase != PumpkinPhase.Ending || CompletionReported) return false;
            Phase = PumpkinPhase.Done;
            CompletionReported = true;
            return true;
        }
    }
}
