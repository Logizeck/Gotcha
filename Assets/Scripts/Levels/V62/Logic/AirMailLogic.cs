namespace Gotcha.Levels.V62
{
    /// <summary>Stati del livello 253 (la lettera per il gatto sul tetto).</summary>
    public enum AirMailState
    {
        Ready,          // lettera sul tavolino, palloncino legato alla panchina
        LetterTied,     // la lettera è legata al palloncino, che è ancora legato
        Flying,         // palloncino libero con la lettera: sale e si guida inclinando
        PlaneFlying,    // l'aeroplanino è in volo (animazione, input bloccato)
        Delivering,     // il gatto prende la lettera (input bloccato)
        Done,
    }

    /// <summary>
    /// Logica pura del livello 253 "Air mail": l'aeroplanino cade a metà strada. Soluzione: legare la lettera al
    /// palloncino, toccare il nodo per liberarlo e guidarlo (inclinando) fino al tetto, lontano dalla candela: se ci
    /// passa sopra scoppia, la lettera cade e il palloncino ricompare legato. Nessuna dipendenza da Unity.
    /// </summary>
    public sealed class AirMailLogic
    {
        public AirMailState State { get; private set; } = AirMailState.Ready;
        public int Throws { get; private set; }
        public int Pops { get; private set; }
        public bool CompletionReported { get; private set; }
        AirMailState planeFrom = AirMailState.Ready;   // dove tornare quando l'aeroplanino atterra

        public bool InputLocked => State == AirMailState.PlaneFlying
            || State == AirMailState.Delivering || State == AirMailState.Done;
        public bool BalloonTied => State == AirMailState.Ready || State == AirMailState.LetterTied || State == AirMailState.PlaneFlying;
        public bool LetterOnBalloon => State == AirMailState.LetterTied || State == AirMailState.Flying;

        public void Reset()
        {
            State = AirMailState.Ready;
            planeFrom = AirMailState.Ready;
            Throws = 0;
            Pops = 0;
            CompletionReported = false;
        }

        /// <summary>Lancio dell'aeroplanino (solo da fermi, col palloncino legato).</summary>
        public bool TryThrowPlane()
        {
            if (State != AirMailState.Ready && State != AirMailState.LetterTied) return false;
            Throws++;
            planeFrom = State;
            State = AirMailState.PlaneFlying;
            return true;
        }

        /// <summary>L'aeroplanino è caduto ed è tornato al suo posto.</summary>
        public bool TryLandPlane()
        {
            if (State != AirMailState.PlaneFlying) return false;
            State = planeFrom;
            return true;
        }

        /// <summary>La lettera trascinata sul palloncino legato: si lega.</summary>
        public bool TryTieLetter()
        {
            if (State != AirMailState.Ready) return false;
            State = AirMailState.LetterTied;
            return true;
        }

        /// <summary>La lettera viene staccata dal palloncino (ancora legato) e torna sul tavolino.</summary>
        public bool TryUntieLetter()
        {
            if (State != AirMailState.LetterTied) return false;
            State = AirMailState.Ready;
            return true;
        }

        /// <summary>
        /// Tocco sul nodo: con la lettera il palloncino parte (true). Senza lettera non parte: non ha niente da
        /// consegnare (false, il livello lo dice).
        /// </summary>
        public bool TryReleaseKnot()
        {
            if (State != AirMailState.LetterTied) return false;
            State = AirMailState.Flying;
            return true;
        }

        /// <summary>Il palloncino passa sulla fiamma: scoppia, la lettera cade e il palloncino ricompare legato.</summary>
        public bool TryPop()
        {
            if (State != AirMailState.Flying) return false;
            Pops++;
            State = AirMailState.Ready;
            return true;
        }

        /// <summary>Il palloncino con la lettera arriva al tetto: il gatto la prende.</summary>
        public bool TryReachRoof()
        {
            if (State != AirMailState.Flying) return false;
            State = AirMailState.Delivering;
            return true;
        }

        public bool TryCompleteDelivery()
        {
            if (State != AirMailState.Delivering || CompletionReported) return false;
            State = AirMailState.Done;
            CompletionReported = true;
            return true;
        }

        /// <summary>
        /// La fiamma tocca il palloncino? Il palloncino (nodo in knotX, knotY; cupola alta height, raggio radius) scoppia
        /// se la punta della fiamma entra nella cupola o se la fiamma è proprio sotto il nodo, a poca distanza.
        /// </summary>
        public static bool FlameTouches(float knotX, float knotY, float radius, float height, float flameX, float flameY)
        {
            if (radius <= 0f || height <= 0f) return false;
            float cy = knotY + height * 0.5f;
            float dx = (flameX - knotX) / radius, dy = (flameY - cy) / (height * 0.5f);
            if (dx * dx + dy * dy <= 1f) return true;
            float below = knotY - flameY;
            return below >= 0f && below <= 0.45f && (flameX - knotX) * (flameX - knotX) <= radius * radius * 0.36f;
        }

        /// <summary>Il palloncino è sopra il tetto (zona a destra, in alto)?</summary>
        public static bool OverRoof(float knotX, float knotY, float roofMinX, float roofY) => knotX >= roofMinX && knotY >= roofY;
    }
}
