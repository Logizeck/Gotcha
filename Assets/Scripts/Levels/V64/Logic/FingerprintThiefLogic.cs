using System;

namespace Gotcha.Levels.V64
{
    /// <summary>Colore del LED accanto al lettore d'impronte.</summary>
    public enum FpLed { Off, Red, Yellow, Green }

    /// <summary>Esito di un tasto del tastierino.</summary>
    public enum FpPinResult
    {
        Ignored,     // telefono già sbloccato
        Digit,       // cifra aggiunta (PIN non ancora completo)
        Wrong,       // PIN completo e sbagliato (restano tentativi)
        LockedOut,   // terzo PIN sbagliato: "PHONE LOCKED — 30 SECONDS" (finto: il resto funziona)
        StillLocked, // tasto premuto durante il blocco
    }

    /// <summary>
    /// Logica pura del livello 251 "The Fingerprint Thief": nessun PIN funziona (tre errori = blocco finto di 30 s), il
    /// dito del giocatore è "UNKNOWN FINGERPRINT" e scuotere non serve. La foto del proprietario, posata sul lettore,
    /// si aggancia alla finestrella del lettore (LED giallo per un attimo); tenendola premuta con un dito e inclinando
    /// piano il telefono scivola nel vassoio finché il polpastrello della foto è sul lettore: un secondo così e il LED
    /// diventa verde. Contatto del dito e inclinazione sono valutati separatamente. Nessuna dipendenza da Unity.
    /// Coordinate: unità del mondo, x a destra, y in alto; la gravità è quella filtrata dello schermo.
    /// </summary>
    public sealed class FingerprintThiefLogic
    {
        public const int PinLength = 4, MaxAttempts = 3;
        public const float LockoutSeconds = 30f;
        public const float AlignSeconds = 1f;
        public const float LedFlashSeconds = 0.7f;
        public const float ShakeCooldown = 2.5f;
        /// <summary>Quanto si può spostare la foto agganciata (raggio del vassoio).</summary>
        public const float TrayRadius = 0.6f;
        /// <summary>Spostamento della foto per unità di gravità (rispetto a quando il dito l'ha premuta).</summary>
        public const float TiltGain = 2.2f;
        /// <summary>Velocità con cui la foto raggiunge la posizione voluta dalla gravità (1/s).</summary>
        public const float SlideRate = 5f;
        /// <summary>Distanza massima tra il polpastrello della foto e il centro del lettore (tolleranza larga).</summary>
        public const float AlignTolerance = 0.3f;
        /// <summary>Appena agganciata, il polpastrello della foto è un po' a destra del lettore: basta inclinare a sinistra.</summary>
        public const float DockMisalignX = 0.42f, DockMisalignY = 0f;

        static readonly string[] ScannerLines =
        {
            "UNKNOWN FINGERPRINT",
            "Still not your finger.",
            "Stop trying. We know it's you.",
            "Nice finger. Wrong owner.",
        };

        public int Entered { get; private set; }
        public int AttemptsLeft { get; private set; } = MaxAttempts;
        public int WrongPins { get; private set; }
        public float LockoutLeft { get; private set; }
        public bool IsLockedOut => LockoutLeft > 0f;
        public int ScannerTaps { get; private set; }
        public int Shakes { get; private set; }

        /// <summary>La foto è posata (agganciata) sul lettore.</summary>
        public bool Docked { get; private set; }
        /// <summary>Almeno un dito tiene premuta la foto agganciata.</summary>
        public bool Contact { get; private set; }
        /// <summary>Spostamento della foto agganciata rispetto al punto d'aggancio.</summary>
        public float OffsetX { get; private set; }
        public float OffsetY { get; private set; }
        public float AlignTimer { get; private set; }
        public bool Unlocked { get; private set; }
        public bool CompletionReported { get; private set; }
        /// <summary>Quante volte la foto è stata lasciata sul lettore senza tenerla (indizio del LED giallo).</summary>
        public int Drops { get; private set; }

        float baseX, baseY, g0x, g0y, flashLeft, shakeWait;
        FpLed flashColor;

        public void Reset()
        {
            Entered = 0;
            AttemptsLeft = MaxAttempts;
            WrongPins = 0;
            LockoutLeft = 0f;
            ScannerTaps = 0;
            Shakes = 0;
            Docked = Contact = Unlocked = CompletionReported = false;
            OffsetX = OffsetY = AlignTimer = 0f;
            baseX = baseY = g0x = g0y = 0f;
            flashLeft = shakeWait = 0f;
            Drops = 0;
        }

        public FpLed Led
        {
            get
            {
                if (Unlocked) return FpLed.Green;
                if (flashLeft > 0f) return flashColor;
                if (Docked && Contact) return FpLed.Yellow;
                return FpLed.Off;
            }
        }

        // ---------- Tastierino ----------

        public FpPinResult PressDigit(int digit)
        {
            if (Unlocked) return FpPinResult.Ignored;
            if (IsLockedOut) return FpPinResult.StillLocked;
            Entered++;
            if (Entered < PinLength) return FpPinResult.Digit;
            Entered = 0;
            WrongPins++;
            AttemptsLeft--;
            if (AttemptsLeft > 0) return FpPinResult.Wrong;
            AttemptsLeft = 0;
            LockoutLeft = LockoutSeconds;
            return FpPinResult.LockedOut;
        }

        /// <summary>Scorrere del tempo: blocco finto, lampeggi del LED, attesa tra due scossoni.</summary>
        /// <returns>True nel momento in cui il blocco finto finisce (tornano i tre tentativi).</returns>
        public bool Tick(float dt)
        {
            dt = Math.Max(0f, dt);
            if (flashLeft > 0f) flashLeft -= dt;
            if (shakeWait > 0f) shakeWait -= dt;
            if (!IsLockedOut) return false;
            LockoutLeft -= dt;
            if (LockoutLeft > 0f) return false;
            LockoutLeft = 0f;
            AttemptsLeft = MaxAttempts;
            return true;
        }

        /// <summary>Secondi di blocco da mostrare (arrotondati per eccesso).</summary>
        public int LockoutSecondsShown => (int)Math.Ceiling(LockoutLeft);

        // ---------- Lettore e scossoni ----------

        /// <summary>Il dito del giocatore sul lettore: LED rosso e una frase sempre più assurda.</summary>
        public string TapScanner()
        {
            if (Unlocked) return null;
            ScannerTaps++;
            Flash(FpLed.Red);
            int i = ScannerTaps <= ScannerLines.Length ? ScannerTaps - 1 : 1 + (ScannerTaps - 2) % (ScannerLines.Length - 1);
            return ScannerLines[i];
        }

        /// <summary>Scossone: true se va mostrata la reazione (non più di una ogni tanto).</summary>
        public bool Shake()
        {
            if (Unlocked || shakeWait > 0f) return false;
            shakeWait = ShakeCooldown;
            Shakes++;
            return true;
        }

        void Flash(FpLed c)
        {
            flashColor = c;
            flashLeft = LedFlashSeconds;
        }

        // ---------- La foto ----------

        /// <summary>La foto entra nella finestrella del lettore (si aggancia con il polpastrello un po' storto).</summary>
        public bool Dock()
        {
            if (Unlocked || Docked) return false;
            Docked = true;
            OffsetX = OffsetY = 0f;
            AlignTimer = 0f;
            return true;
        }

        /// <summary>La foto viene tirata via dal lettore.</summary>
        public void Undock()
        {
            if (Unlocked) return;
            Docked = false;
            Contact = false;
            AlignTimer = 0f;
            OffsetX = OffsetY = 0f;
        }

        /// <summary>
        /// Un dito preme (o lascia) la foto agganciata. Alla pressione si memorizza la gravità di quel momento: conta
        /// solo quanto si inclina da lì, non l'angolo assoluto. Lasciarla: LED giallo per un attimo (indizio).
        /// </summary>
        public void SetContact(bool on, float gx, float gy)
        {
            if (Unlocked) return;
            if (!Docked) { Contact = false; return; }
            if (on == Contact) return;
            Contact = on;
            AlignTimer = 0f;
            if (on)
            {
                baseX = OffsetX;
                baseY = OffsetY;
                g0x = gx;
                g0y = gy;
            }
            else
            {
                Drops++;
                Flash(FpLed.Yellow);
            }
        }

        /// <summary>Con la foto premuta, l'inclinazione la fa scivolare piano nel vassoio (mai fuori).</summary>
        public void Tilt(float gx, float gy, float dt)
        {
            if (Unlocked || !Docked || !Contact) return;
            float tx = baseX + (gx - g0x) * TiltGain;
            float ty = baseY + (gy - g0y) * TiltGain;
            float len = (float)Math.Sqrt(tx * tx + ty * ty);
            if (len > TrayRadius) { tx *= TrayRadius / len; ty *= TrayRadius / len; }
            float k = 1f - (float)Math.Exp(-SlideRate * Math.Max(0f, dt));
            OffsetX += (tx - OffsetX) * k;
            OffsetY += (ty - OffsetY) * k;
        }

        /// <summary>Distanza tra il polpastrello della foto e il centro del lettore.</summary>
        public float TipDistance
        {
            get
            {
                float dx = DockMisalignX + OffsetX, dy = DockMisalignY + OffsetY;
                return (float)Math.Sqrt(dx * dx + dy * dy);
            }
        }

        public bool Aligned => Docked && TipDistance <= AlignTolerance;

        /// <summary>Controllo dello sblocco: foto agganciata, premuta e allineata per un secondo.</summary>
        /// <returns>True nel momento in cui il telefono si sblocca.</returns>
        public bool Step(float dt)
        {
            if (Unlocked) return false;
            if (!(Docked && Contact && Aligned)) { AlignTimer = 0f; return false; }
            AlignTimer += Math.Max(0f, dt);
            if (AlignTimer < AlignSeconds) return false;
            Unlocked = true;
            Entered = 0;
            LockoutLeft = 0f;
            return true;
        }

        /// <summary>Fine della scenetta di vittoria: true una volta sola.</summary>
        public bool TryComplete()
        {
            if (!Unlocked || CompletionReported) return false;
            CompletionReported = true;
            return true;
        }
    }
}
