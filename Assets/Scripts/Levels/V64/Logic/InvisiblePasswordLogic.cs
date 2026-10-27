using System;

namespace Gotcha.Levels.V64
{
    /// <summary>Stati del livello 255 "The Invisible Password".</summary>
    public enum PasswordDoorState
    {
        Hinged,     // porta chiusa sui suoi cardini (si può solo digitare, bussare, tirare i perni)
        Detached,   // i perni dei cardini sono fuori: la porta pende dalla serratura e segue l'inclinazione
        Unlocking,  // codice giusto: animazione finale (input bloccato)
        Done,
    }

    /// <summary>Cosa succede dopo un'azione del giocatore (la presentazione sceglie animazioni e suoni).</summary>
    public enum PasswordReaction
    {
        None,
        Typed,          // una cifra in più sul display
        Cleared,
        Wrong,          // codice sbagliato (frase buffa, mai blocco)
        NiceTry,        // 0000
        SoDidWe,        // "FORGOT PASSWORD"
        DoorShakes,     // tocco sulla porta
        HingeLoosens,   // al terzo tocco un cardine si allenta (indizio)
        Straining,      // un perno solo tirato: non basta
        Squeal,         // entrambi i perni escono un po' (cigolio metallico)
        PinsSlideBack,  // dita staccate prima della fine: i perni rientrano
        PinsOut,        // i perni cadono: la porta è staccata
        RearRevealed,   // la porta si apre abbastanza da mostrare il retro ("0000")
        MirrorRevealed, // il retro si riflette nello specchio ("8008")
        Unlocks,        // 8008: la porta si apre
    }

    /// <summary>
    /// Logica pura del livello 255 "The Invisible Password": tastiera a quattro cifre che non si blocca mai, i due
    /// cardini da sfilare con due dita insieme, la porta staccata che si apre inclinando il telefono a destra, il
    /// finto "0000" sul retro e il vero codice "8008" visibile solo nel riflesso dello specchio.
    /// Il codice giusto vale sempre (anche se il giocatore lo sa già): nessun blocco, nessun riavvio.
    /// Nessuna dipendenza da Unity.
    /// </summary>
    public sealed class InvisiblePasswordLogic
    {
        public const string Code = "8008";
        public const string Decoy = "0000";
        /// <summary>Quanto (unità mondo) va tirato OGNI perno, insieme all'altro, perché esca.</summary>
        public const float PinPullNeeded = 0.6f;
        /// <summary>Angolo (gradi) a cui il retro della porta diventa leggibile, con isteresi.</summary>
        public const float RearShowDeg = 105f, RearHideDeg = 95f;
        /// <summary>Angolo a cui il retro si riflette nello specchio, con isteresi.</summary>
        public const float MirrorShowDeg = 140f, MirrorHideDeg = 125f;
        /// <summary>Inclinazione a destra (accelerazione x filtrata) per l'apertura massima e per l'inizio.</summary>
        public const float TiltStart = 0.1f, TiltFull = 0.6f, MaxDoorDeg = 165f;

        public static readonly string[] WrongLines =
        {
            "WRONG. BUT VERY CONFIDENT.",
            "ACCESS DENIED. NICE ENERGY, THOUGH.",
            "INCORRECT. THE DOOR IS LAUGHING AT YOU.",
            "NOPE. HAVE YOU TRIED BEING THE OWNER?",
            "ERROR 404: PASSWORD NOT FOUND.",
            "WRONG AGAIN. THE KEYPAD IS EMBARRASSED FOR YOU.",
        };
        public static readonly string[] NiceTryLines = { "NICE TRY.", "STILL A NICE TRY. STILL WRONG.", "0000? THAT'S WHAT THEY WANT YOU TO THINK." };
        public const string SoDidWeLine = "SO DID WE.";
        public const string ObviousLine = "1234? SERIOUSLY?";

        public PasswordDoorState State { get; private set; } = PasswordDoorState.Hinged;
        public string Entry { get; private set; } = "";
        public string LastMessage { get; private set; } = "";
        public int WrongCount { get; private set; }
        public int NiceTryCount { get; private set; }
        public int DoorTaps { get; private set; }
        public bool HingeLoose { get; private set; }
        /// <summary>Quanto sono usciti i perni (0..1; 1 = fuori). Torna a 0 se si staccano le dita prima della fine.</summary>
        public float PinProgress { get; private set; }
        public bool Detached => pinsOut;
        public float SmoothedTilt { get; private set; }
        /// <summary>Angolo di apertura (gradi) richiesto dall'inclinazione; 0 finché la porta è sui cardini.</summary>
        public float TargetDoorDeg { get; private set; }
        public bool RearVisible { get; private set; }
        public bool MirrorAligned { get; private set; }
        public bool RearSeen { get; private set; }
        public bool MirrorSeen { get; private set; }
        public bool CompletionReported { get; private set; }
        public bool InputLocked => State == PasswordDoorState.Unlocking || State == PasswordDoorState.Done;

        bool pinsOut, strainingShown, tiltPrimed;
        int lastSquealStep;

        public void Reset()
        {
            State = PasswordDoorState.Hinged;
            Entry = "";
            LastMessage = "";
            WrongCount = NiceTryCount = DoorTaps = 0;
            HingeLoose = false;
            PinProgress = 0f;
            SmoothedTilt = 0f;
            TargetDoorDeg = 0f;
            RearVisible = MirrorAligned = RearSeen = MirrorSeen = false;
            CompletionReported = false;
            pinsOut = strainingShown = tiltPrimed = false;
            lastSquealStep = 0;
        }

        // ---------- Tastiera ----------

        public PasswordReaction PressDigit(int digit)
        {
            if (InputLocked || digit < 0 || digit > 9) return PasswordReaction.None;
            Entry += (char)('0' + digit);
            if (Entry.Length < 4) return PasswordReaction.Typed;
            string code = Entry;
            Entry = "";
            if (code == Code)
            {
                State = PasswordDoorState.Unlocking;
                LastMessage = "ACCESS GRANTED.";
                return PasswordReaction.Unlocks;
            }
            if (code == Decoy)
            {
                LastMessage = NiceTryLines[Math.Min(NiceTryCount, NiceTryLines.Length - 1)];
                NiceTryCount++;
                return PasswordReaction.NiceTry;
            }
            LastMessage = code == "1234" ? ObviousLine : WrongLines[WrongCount % WrongLines.Length];
            WrongCount++;
            return PasswordReaction.Wrong;
        }

        public PasswordReaction Clear()
        {
            if (InputLocked || Entry.Length == 0) return PasswordReaction.None;
            Entry = "";
            return PasswordReaction.Cleared;
        }

        public PasswordReaction ForgotPassword()
        {
            if (InputLocked) return PasswordReaction.None;
            LastMessage = SoDidWeLine;
            return PasswordReaction.SoDidWe;
        }

        // ---------- Porta e cardini ----------

        /// <summary>Tocco sulla porta: trema; al terzo tocco un cardine si allenta (una volta sola).</summary>
        public PasswordReaction TapDoor()
        {
            if (InputLocked) return PasswordReaction.None;
            DoorTaps++;
            if (!HingeLoose && !Detached && DoorTaps >= 3)
            {
                HingeLoose = true;
                return PasswordReaction.HingeLoosens;
            }
            return PasswordReaction.DoorShakes;
        }

        /// <summary>
        /// Stato dei due perni in questo frame: per ognuno se è tenuto da un dito e di quanto è stato tirato "in fuori"
        /// (unità mondo). Escono solo tirati INSIEME; se un dito si stacca rientrano (ma niente altro si azzera).
        /// </summary>
        public PasswordReaction UpdatePins(bool topHeld, float topPull, bool bottomHeld, float bottomPull)
        {
            if (InputLocked || Detached) return PasswordReaction.None;
            if (topHeld && bottomHeld)
            {
                strainingShown = false;
                float p = Math.Min(Math.Max(0f, topPull), Math.Max(0f, bottomPull)) / PinPullNeeded;
                if (p > PinProgress) PinProgress = Math.Min(1f, p);
                if (PinProgress >= 1f)
                {
                    pinsOut = true;
                    State = PasswordDoorState.Detached;
                    return PasswordReaction.PinsOut;
                }
                int step = (int)(PinProgress * 4f);
                if (step > lastSquealStep) { lastSquealStep = step; return PasswordReaction.Squeal; }
                return PasswordReaction.None;
            }
            if (PinProgress > 0f)
            {
                PinProgress = 0f;
                lastSquealStep = 0;
                return PasswordReaction.PinsSlideBack;
            }
            if ((topHeld && topPull > 0.15f) || (bottomHeld && bottomPull > 0.15f))
            {
                if (strainingShown) return PasswordReaction.None;
                strainingShown = true;
                return PasswordReaction.Straining;
            }
            if (!topHeld && !bottomHeld) strainingShown = false;
            return PasswordReaction.None;
        }

        // ---------- Inclinazione ----------

        /// <summary>
        /// Inclinazione laterale del telefono (accelerazione x, positiva = verso destra). Filtrata (passa basso), mappata
        /// sull'angolo della porta staccata; retro e specchio hanno soglie con isteresi, quindi niente angoli esatti.
        /// Restituisce RearRevealed / MirrorRevealed la prima volta che succedono.
        /// </summary>
        public PasswordReaction UpdateTilt(float tiltX, float dt)
        {
            if (float.IsNaN(tiltX)) tiltX = 0f;
            tiltX = Math.Max(-1.5f, Math.Min(1.5f, tiltX));
            if (!tiltPrimed) { SmoothedTilt = tiltX; tiltPrimed = true; }
            else SmoothedTilt += (tiltX - SmoothedTilt) * Math.Min(1f, Math.Max(0f, dt) * 6f);
            if (InputLocked) return PasswordReaction.None;
            if (!Detached) { TargetDoorDeg = 0f; RearVisible = MirrorAligned = false; return PasswordReaction.None; }
            float k = (SmoothedTilt - TiltStart) / (TiltFull - TiltStart);
            TargetDoorDeg = Math.Max(0f, Math.Min(1f, k)) * MaxDoorDeg;
            return Angle(TargetDoorDeg);
        }

        /// <summary>Aggiorna le soglie per l'angolo raggiunto (anche dall'animazione).</summary>
        public PasswordReaction Angle(float deg)
        {
            PasswordReaction r = PasswordReaction.None;
            if (!RearVisible && deg >= RearShowDeg) RearVisible = true;
            else if (RearVisible && deg < RearHideDeg) RearVisible = false;
            if (!MirrorAligned && deg >= MirrorShowDeg) MirrorAligned = true;
            else if (MirrorAligned && deg < MirrorHideDeg) MirrorAligned = false;
            if (RearVisible && !RearSeen) { RearSeen = true; r = PasswordReaction.RearRevealed; }
            if (MirrorAligned && !MirrorSeen) { MirrorSeen = true; r = PasswordReaction.MirrorRevealed; }
            return r;
        }

        public bool TryComplete()
        {
            if (State != PasswordDoorState.Unlocking || CompletionReported) return false;
            State = PasswordDoorState.Done;
            CompletionReported = true;
            return true;
        }
    }
}
