using System;

namespace Gotcha.Levels.Halloween
{
    /// <summary>Stati del livello 245 (Toni il lupo-carlino).</summary>
    public enum WerepugState
    {
        BoxClosed,      // scatola chiusa, nessun biscotto in giro
        BiscuitReady,   // il biscotto (uno solo) è fuori, posato al suo posto
        Holding,        // il biscotto è in mano al giocatore
        Transforming,   // la luna è coperta: Toni torna normale (input bloccato)
        Done,           // finale concluso
    }

    /// <summary>
    /// Logica pura del livello 245 "Toni the were-pug": la luna piena trasforma Toni; il giocatore apre la scatola,
    /// prende l'unico biscotto e lo mette davanti alla luna, coprendola. Nessuna dipendenza da Unity.
    /// </summary>
    public sealed class WerepugLogic
    {
        /// <summary>Distanza massima centro-centro, in raggi della luna, per considerarla coperta.</summary>
        public const float DefaultCoverTolerance = 0.6f;
        /// <summary>Il biscotto deve essere grande almeno così (in raggi della luna) per coprirla.</summary>
        public const float MinCoverSize = 0.8f;

        public WerepugState State { get; private set; } = WerepugState.BoxClosed;
        /// <summary>Biscotti usciti dalla scatola: 0 o 1, mai di più.</summary>
        public int BiscuitsOut { get; private set; }
        public bool CompletionReported { get; private set; }
        public bool InputLocked => State == WerepugState.Transforming || State == WerepugState.Done;

        public void Reset()
        {
            State = WerepugState.BoxClosed;
            BiscuitsOut = 0;
            CompletionReported = false;
        }

        /// <summary>Toni ringhia se lo tocchi (solo finché è un mostro e non si sta trasformando).</summary>
        public bool CanGrowl => !InputLocked;

        /// <summary>Apre la scatola: esce un solo biscotto, la seconda volta non succede niente.</summary>
        public bool TryOpenBox()
        {
            if (State != WerepugState.BoxClosed) return false;
            State = WerepugState.BiscuitReady;
            BiscuitsOut = 1;
            return true;
        }

        public bool TryPickBiscuit()
        {
            if (State != WerepugState.BiscuitReady) return false;
            State = WerepugState.Holding;
            return true;
        }

        /// <summary>Rilascio del biscotto: true se copre la luna (parte la trasformazione), altrimenti torna al suo posto.</summary>
        public bool TryDropBiscuit(bool coversMoon)
        {
            if (State != WerepugState.Holding) return false;
            State = coversMoon ? WerepugState.Transforming : WerepugState.BiscuitReady;
            return coversMoon;
        }

        /// <summary>Fine dell'animazione: true una sola volta (è il momento di chiamare Win).</summary>
        public bool TryCompleteTransform()
        {
            if (State != WerepugState.Transforming || CompletionReported) return false;
            State = WerepugState.Done;
            CompletionReported = true;
            return true;
        }

        /// <summary>
        /// Verifica geometrica (unità mondo): il biscotto copre la luna se il suo centro è abbastanza vicino a quello
        /// della luna (tolleranza in raggi della luna) e se è grande quasi quanto lei.
        /// </summary>
        public static bool Covers(float biscuitX, float biscuitY, float biscuitRadius, float moonX, float moonY, float moonRadius,
            float tolerance = DefaultCoverTolerance)
        {
            if (moonRadius <= 0f || biscuitRadius <= 0f) return false;
            float dx = biscuitX - moonX, dy = biscuitY - moonY;
            float dist = (float)Math.Sqrt(dx * dx + dy * dy);
            return dist <= tolerance * moonRadius && biscuitRadius >= MinCoverSize * moonRadius;
        }
    }
}
