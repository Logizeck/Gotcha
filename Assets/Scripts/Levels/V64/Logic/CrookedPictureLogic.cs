using System;

namespace Gotcha.Levels.V64
{
    /// <summary>Fasi del livello 266.</summary>
    public enum CrookedStage
    {
        Calibrating,   // primi istanti: si misura come il giocatore tiene il telefono (angolo di partenza)
        Playing,       // la stanza è storta rispetto allo schermo
        Straightened,  // il telefono è stato inclinato quanto la stanza: festa finale, input bloccato
        Done,
    }

    /// <summary>Cosa è appena successo (il livello sceglie scritta, suono e animazione).</summary>
    public enum CrookedEvent
    {
        None,
        Calibrated,    // angolo di partenza misurato
        Aligning,      // il rollio relativo è entrato nella tolleranza: si comincia a contare
        LostAlign,     // uscito dalla tolleranza (con l'isteresi) prima del tempo
        WrongWay,      // inclinato dalla parte sbagliata per un po' (la stanza sembra ancora più storta)
        Flat,          // telefono quasi piatto: angolo inaffidabile, non conta niente
        Straightened,  // tenuto allineato abbastanza a lungo: vittoria (una volta sola)
    }

    /// <summary>
    /// Logica pura del livello 266 "The Crooked Picture" — "Straighten the picture." Il quadro sembra storto di 12 gradi
    /// ma è dritto: è TUTTA la stanza a essere disegnata inclinata (RoomAngle = rotazione z della stanza, negativa =
    /// in senso orario). Indizi che seguono la gravità VERA del telefono: il pelo dell'acqua nel bicchiere, la bolla
    /// della livella sul tavolo, la biglia sulla mensola. Le mosse sbagliate (girare il quadro, il nastro adesivo, il
    /// martello e il chiodo in più, toccare la livella o la biglia) contano solo per le battute e non cambiano niente.
    /// Soluzione: inclinare il telefono (rollio attorno all'asse dello schermo) quanto la stanza, così la stanza è dritta
    /// per chi guarda. Rollio = atan2(g.x, -g.y) della gravità filtrata (V64AGravity, x a destra e y in su nello schermo;
    /// telefono dritto = 0, positivo = telefono girato in senso orario), misurato RISPETTO all'angolo tenuto all'inizio
    /// (calibrazione). Serve un rollio relativo = RoomAngle ± EnterTol tenuto per HoldToWin secondi, con l'isteresi
    /// (si esce solo oltre ExitTol; uscendo il tempo cala piano invece di azzerarsi). Telefono quasi piatto (gravità nel
    /// piano dello schermo sotto MinReliable) = angolo inaffidabile: ignorato, niente vittorie per sbaglio; se resta piatto
    /// a lungo la calibrazione si rifà quando lo si riprende in mano. Nessuna dipendenza da Unity.
    /// </summary>
    public sealed class CrookedPictureLogic
    {
        // ---------- Regolazioni ----------
        /// <summary>Rotazione della stanza nello schermo (gradi, z di Unity): è anche il rollio relativo da raggiungere.</summary>
        public const float RoomAngle = -12f;
        public const float EnterTol = 4f, ExitTol = 6.5f;
        public const float HoldToWin = 1f;
        /// <summary>Uscendo dalla tolleranza il tempo accumulato cala con questa velocità (s al secondo).</summary>
        public const float HoldDecay = 2f;
        public const float CalibrateTime = 0.4f;
        /// <summary>Intensità minima della gravità nel piano dello schermo (in g) per fidarsi dell'angolo; isteresi.</summary>
        public const float MinReliable = 0.3f, ReliableOff = 0.22f;
        /// <summary>Dopo tanto tempo piatto la calibrazione si rifà (il telefono è stato posato e ripreso).</summary>
        public const float RecalibrateAfterFlat = 1.5f;
        /// <summary>Dalla parte sbagliata: oltre questo angolo (relativo, segno opposto all'obiettivo) per WrongWayTime.</summary>
        public const float WrongWayAngle = 7f, WrongWayRearm = 3f, WrongWayTime = 0.8f;
        /// <summary>Lunghezza della mensola per la biglia (-1..1) e gravità finta della biglia.</summary>
        public const float MarbleG = 9f, MarbleFriction = 1.6f, MarbleBounce = 0.35f;

        readonly V64AGravity gravity = new V64AGravity();

        public CrookedStage Stage { get; private set; }
        /// <summary>Angolo di partenza (gradi), valido dopo la calibrazione.</summary>
        public float Baseline { get; private set; }
        /// <summary>Rollio assoluto dell'ultima lettura affidabile (gradi).</summary>
        public float Roll { get; private set; }
        /// <summary>Rollio rispetto all'angolo di partenza (0 durante la calibrazione).</summary>
        public float Relative { get; private set; }
        public bool Reliable { get; private set; }
        public bool Aligned { get; private set; }
        /// <summary>Tempo accumulato in tolleranza (0..HoldToWin).</summary>
        public float Hold { get; private set; }
        public float HoldFraction => Math.Min(1f, Hold / HoldToWin);
        public int WrongWays { get; private set; }

        // biglia sulla mensola: posizione -1 (estremo sinistro) .. 1 (destro), velocità
        public float MarbleX { get; private set; }
        public float MarbleV { get; private set; }

        // contatori delle mosse sbagliate (solo per scegliere le battute)
        public int Twists { get; private set; }
        public int Tapes { get; private set; }
        public int Hammers { get; private set; }
        public int LevelTaps { get; private set; }
        public int MarbleTaps { get; private set; }
        public int GlassTaps { get; private set; }

        float calibSum, calibTime, flatFor, wrongFor;
        int calibCount;
        bool wrongArmed;
        bool completed;

        public CrookedPictureLogic() { Reset(); }

        public void Reset()
        {
            gravity.Reset();
            Stage = CrookedStage.Calibrating;
            Baseline = Roll = Relative = 0f;
            Reliable = Aligned = false;
            Hold = 0f;
            WrongWays = 0;
            MarbleX = 0.85f;   // a riposo verso il lato basso (destra)
            MarbleV = 0f;
            Twists = Tapes = Hammers = LevelTaps = MarbleTaps = GlassTaps = 0;
            calibSum = calibTime = flatFor = wrongFor = 0f;
            calibCount = 0;
            wrongArmed = true;
            completed = false;
        }

        public bool InputLocked => Stage == CrookedStage.Straightened || Stage == CrookedStage.Done;

        /// <summary>Inclinazione "vera" della stanza rispetto alla gravità del telefono (gradi): 0 = dritta per chi guarda.</summary>
        public float EffectiveTilt => InputLocked ? 0f : RoomAngle - Relative;

        /// <summary>Bolla della livella: -1..1 (negativo = verso sinistra, il lato alto della stanza); 0 = al centro.</summary>
        public float Bubble => Clamp(EffectiveTilt / Math.Abs(RoomAngle), -1f, 1f);

        /// <summary>Pelo dell'acqua rispetto al bicchiere (gradi, z locale nella stanza): resta orizzontale nel mondo vero.</summary>
        public float WaterTilt => Clamp(-EffectiveTilt, -35f, 35f);

        /// <summary>Quanto manca all'obiettivo (gradi, valore assoluto).</summary>
        public float Error => Math.Abs(Relative - RoomAngle);

        /// <summary>Pausa o perdita del focus: il filtro riparte dalla prossima lettura e il tempo in tolleranza si azzera.</summary>
        public void OnResume()
        {
            if (InputLocked) return;
            float x = gravity.X, y = gravity.Y;
            gravity.Reset(x, y);
            Hold = 0f;
            Aligned = false;
            wrongFor = 0f;
        }

        /// <summary>Nuova lettura dell'accelerometro (sistema dello schermo, in g) e passo di tempo.</summary>
        public CrookedEvent Feed(float gx, float gy, float dt)
        {
            if (dt < 0f || float.IsNaN(dt)) dt = 0f;
            if (InputLocked) return CrookedEvent.None;
            gravity.Feed(gx, gy, dt);
            TickMarble(dt);

            // affidabilità con isteresi: telefono quasi piatto = angolo che non vuol dire niente
            float mag = gravity.Magnitude;
            bool wasReliable = Reliable;
            Reliable = Reliable ? mag >= ReliableOff : mag >= MinReliable;
            if (!Reliable)
            {
                flatFor += dt;
                Aligned = false;
                Hold = Math.Max(0f, Hold - HoldDecay * dt);
                wrongFor = 0f;
                if (Stage == CrookedStage.Playing && flatFor >= RecalibrateAfterFlat) BeginCalibration();
                return wasReliable ? CrookedEvent.Flat : CrookedEvent.None;
            }
            flatFor = 0f;
            Roll = (float)(Math.Atan2(gravity.X, -gravity.Y) * 180.0 / Math.PI);

            if (Stage == CrookedStage.Calibrating)
            {
                calibSum += Roll;
                calibCount++;
                calibTime += dt;
                Relative = 0f;
                if (calibTime < CalibrateTime || calibCount < 3) return CrookedEvent.None;
                Baseline = calibSum / calibCount;
                Stage = CrookedStage.Playing;
                return CrookedEvent.Calibrated;
            }

            Relative = DeltaAngle(Baseline, Roll);
            float err = Math.Abs(Relative - RoomAngle);
            CrookedEvent ev = CrookedEvent.None;
            bool was = Aligned;
            Aligned = Aligned ? err <= ExitTol : err <= EnterTol;
            if (Aligned)
            {
                if (!was) ev = CrookedEvent.Aligning;
                Hold += dt;
                if (Hold >= HoldToWin)
                {
                    Hold = HoldToWin;
                    Stage = CrookedStage.Straightened;
                    return CrookedEvent.Straightened;
                }
            }
            else
            {
                if (was) ev = CrookedEvent.LostAlign;
                Hold = Math.Max(0f, Hold - HoldDecay * dt);
            }

            // dalla parte sbagliata (segno opposto all'obiettivo): una battuta per volta, si riarma tornando indietro
            float wrong = -Math.Sign(RoomAngle) * Relative;
            if (wrong >= WrongWayAngle)
            {
                wrongFor += dt;
                if (wrongArmed && wrongFor >= WrongWayTime)
                {
                    wrongArmed = false;
                    WrongWays++;
                    if (ev == CrookedEvent.None) ev = CrookedEvent.WrongWay;
                }
            }
            else
            {
                wrongFor = 0f;
                if (wrong < WrongWayRearm) wrongArmed = true;
            }
            return ev;
        }

        void BeginCalibration()
        {
            Stage = CrookedStage.Calibrating;
            calibSum = calibTime = 0f;
            calibCount = 0;
            Relative = 0f;
            Hold = 0f;
            Aligned = false;
        }

        // ---------- Biglia ----------

        void TickMarble(float dt)
        {
            if (dt <= 0f) return;
            // rotola verso il lato basso: inclinazione negativa (senso orario) = destra più in basso
            double tilt = EffectiveTilt * Math.PI / 180.0;
            float a = (float)(-Math.Sin(tilt) * MarbleG);
            MarbleV += a * dt;
            MarbleV -= MarbleV * Math.Min(1f, MarbleFriction * dt);
            MarbleX += MarbleV * dt;
            if (MarbleX > 1f) { MarbleX = 1f; MarbleV = -MarbleV * MarbleBounce; }
            else if (MarbleX < -1f) { MarbleX = -1f; MarbleV = -MarbleV * MarbleBounce; }
        }

        // ---------- Mosse sbagliate (non cambiano niente: solo il numero per la battuta) ----------

        /// <summary>Il quadro girato col dito (torna storto da solo).</summary>
        public int TwistPicture() => InputLocked ? 0 : ++Twists;

        /// <summary>Nastro adesivo sul quadro (si stacca).</summary>
        public int Tape() => InputLocked ? 0 : ++Tapes;

        /// <summary>Martello e chiodo in più (il quadro scivola indietro lo stesso).</summary>
        public int Hammer() => InputLocked ? 0 : ++Hammers;

        /// <summary>Tocco sulla livella (la bolla torna fuori centro).</summary>
        public int TapLevel() => InputLocked ? 0 : ++LevelTaps;

        public int TapGlass() => InputLocked ? 0 : ++GlassTaps;

        /// <summary>Tocco sulla biglia: una spinta verso il lato alto (poi torna giù da sola).</summary>
        public int TapMarble()
        {
            if (InputLocked) return 0;
            float low = EffectiveTilt < 0f ? 1f : EffectiveTilt > 0f ? -1f : (MarbleX >= 0f ? 1f : -1f);
            MarbleV = -low * 2.6f;
            return ++MarbleTaps;
        }

        /// <summary>Fine della festa finale: true una volta sola.</summary>
        public bool TryComplete()
        {
            if (Stage != CrookedStage.Straightened || completed) return false;
            completed = true;
            Stage = CrookedStage.Done;
            return true;
        }

        // ---------- Aiuti ----------

        static float Clamp(float v, float lo, float hi) => v < lo ? lo : v > hi ? hi : v;

        /// <summary>Differenza tra due angoli in gradi, in -180..180.</summary>
        public static float DeltaAngle(float from, float to)
        {
            float d = (to - from) % 360f;
            if (d > 180f) d -= 360f;
            else if (d < -180f) d += 360f;
            return d;
        }

        /// <summary>Gravità nello schermo per un telefono dritto girato di "rollDeg" (positivo = senso orario) e inclinato
        /// all'indietro di "pitchDeg" (0 = verticale, 90 = piatto sul tavolo). Per i tasti dell'Editor e per i test.</summary>
        public static void GravityFor(float rollDeg, float pitchDeg, out float gx, out float gy)
        {
            double r = rollDeg * Math.PI / 180.0, p = pitchDeg * Math.PI / 180.0;
            double inPlane = Math.Cos(p);
            gx = (float)(Math.Sin(r) * inPlane);
            gy = (float)(-Math.Cos(r) * inPlane);
        }
    }
}
