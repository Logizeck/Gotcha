using System;

namespace Gotcha.Levels.V64
{
    /// <summary>Fasi del livello 254.</summary>
    public enum AlarmStage
    {
        Ringing,       // il comodino copre la crepa: si prova di tutto con la sveglia
        CrackOpen,     // il comodino è stato spostato: la crepa nel pavimento si vede (non si richiude più)
        Neighbour,     // la biglia è caduta di sotto: si vede il telefono del vicino, serve un suono forte
        Snoozed,       // l'onda d'urto ha fatto rimbalzare la biglia su SNOOZE (input bloccato)
        Done,
    }

    /// <summary>Cosa ha fatto la biglia in questo passo.</summary>
    public enum MarbleEvent { None, Bump, Fell }

    /// <summary>
    /// Logica pura del livello 254 "The Neighbour's Alarm": la sveglia sul comodino non c'entra (toccarla cambia l'ora
    /// e suona più forte, tenerla la zittisce ma il suono continua dal pavimento, il cuscino la attutisce soltanto,
    /// scuotere la fa cadere). Soluzione: spostare il comodino con due dita (è pesante), far rotolare la biglia nella
    /// crepa inclinando il telefono, poi un solo suono forte nel microfono. Nessuna dipendenza da Unity.
    /// Coordinate x nel mondo (la scena è vista di lato: la biglia rotola lungo il pavimento).
    /// </summary>
    public sealed class NeighboursAlarmLogic
    {
        public const float WallLeft = -2.3f, WallRight = 2.3f;
        public const float TableHomeX = 0.95f, TableHalfWidth = 0.5f;
        /// <summary>Il comodino si sposta solo verso il muro (a sinistra c'è il letto).</summary>
        public const float TableMaxOffset = 1.05f;
        /// <summary>Quanto si sposta il comodino per unità di movimento delle dita: è pesante.</summary>
        public const float TableWeight = 0.45f;
        /// <summary>Spostamento oltre il quale la crepa resta scoperta (e il comodino non torna più indietro).</summary>
        public const float RevealOffset = 0.75f;
        public const float CrackX = 0.95f, CrackHalfWidth = 0.22f;
        public const float MarbleStartX = -0.35f, MarbleRadius = 0.15f;
        public const float MarbleAccel = 7f, MarbleFriction = 1.4f, MarbleMaxSpeed = 3.2f, TiltDeadZone = 0.06f;
        public const float BumpSpeed = 0.35f;
        public const int MaxLoudness = 4;

        public AlarmStage Stage { get; private set; } = AlarmStage.Ringing;
        public float TableOffset { get; private set; }
        public float MarbleX { get; private set; } = MarbleStartX;
        public float MarbleV { get; private set; }
        public int ClockTaps { get; private set; }
        public int Loudness { get; private set; } = 1;
        public int ClockHour { get; private set; } = 7;
        public int ClockMinute { get; private set; }
        public bool ClockHeld { get; private set; }
        public bool PillowOnClock { get; private set; }
        public bool ClockFallen { get; private set; }
        public int HeavyPushes { get; private set; }
        public bool CompletionReported { get; private set; }

        public bool TableMoved => Stage != AlarmStage.Ringing;
        public float TableX => TableHomeX + TableOffset;
        public bool InputLocked => Stage == AlarmStage.Snoozed || Stage == AlarmStage.Done;
        /// <summary>Il microfono serve solo quando si vede il telefono del vicino.</summary>
        public bool NeedsMic => Stage == AlarmStage.Neighbour;
        /// <summary>L'allarme suona ancora (anche se la sveglia è tenuta o coperta: il suono viene da sotto).</summary>
        public bool Ringing => Stage != AlarmStage.Snoozed && Stage != AlarmStage.Done;

        public void Reset()
        {
            Stage = AlarmStage.Ringing;
            TableOffset = 0f;
            MarbleX = MarbleStartX;
            MarbleV = 0f;
            ClockTaps = 0;
            Loudness = 1;
            ClockHour = 7;
            ClockMinute = 0;
            ClockHeld = PillowOnClock = ClockFallen = false;
            HeavyPushes = 0;
            CompletionReported = false;
        }

        // ---------- La sveglia (falsa pista) ----------

        /// <summary>Toccare la sveglia: l'ora cambia e lo squillo diventa più forte.</summary>
        public bool TapClock()
        {
            if (InputLocked) return false;
            ClockTaps++;
            Loudness = Math.Min(MaxLoudness, Loudness + 1);
            ClockMinute += 17;
            ClockHour = (ClockHour + 1 + ClockMinute / 60) % 12;
            if (ClockHour == 0) ClockHour = 12;
            ClockMinute %= 60;
            return true;
        }

        /// <summary>Tenere la sveglia: smette di squillare lei, ma il suono continua dal pavimento.</summary>
        public void HoldClock(bool on) { ClockHeld = on && !InputLocked; }

        public void SetPillowOnClock(bool on) { PillowOnClock = on && !InputLocked; }

        /// <summary>Scossone: il comodino traballa e la sveglia cade (una volta). True se è appena caduta.</summary>
        public bool Shake()
        {
            if (InputLocked || ClockFallen) return false;
            ClockFallen = true;
            PillowOnClock = false;
            ClockHeld = false;
            return true;
        }

        // ---------- Il comodino ----------

        /// <summary>Un dito solo prova a spostare il comodino: non si muove.</summary>
        public void PushWithOneFinger() { if (!InputLocked) HeavyPushes++; }

        /// <summary>
        /// Due dita trascinano il comodino (dx = spostamento medio delle dita): si muove poco e solo verso il muro.
        /// True nel momento in cui la crepa viene scoperta.
        /// </summary>
        public bool DragTable(float fingersDx)
        {
            if (InputLocked) return false;
            float min = TableMoved ? RevealOffset : 0f;
            float next = Math.Max(min, Math.Min(TableMaxOffset, TableOffset + fingersDx * TableWeight));
            // la biglia non può finire schiacciata: il comodino non va a coprirla
            if (MarbleX + MarbleRadius > TableHomeX + next - TableHalfWidth && MarbleX < TableHomeX + next) next = TableOffset;
            TableOffset = next;
            if (Stage == AlarmStage.Ringing && TableOffset >= RevealOffset)
            {
                Stage = AlarmStage.CrackOpen;
                return true;
            }
            return false;
        }

        public bool CrackExposed => TableX - TableHalfWidth >= CrackX + CrackHalfWidth - 0.001f;

        // ---------- La biglia ----------

        /// <summary>La biglia rotola seguendo l'inclinazione (x della gravità filtrata).</summary>
        public MarbleEvent StepMarble(float tiltX, float dt)
        {
            if (InputLocked || Stage == AlarmStage.Neighbour) return MarbleEvent.None;
            dt = Math.Max(0f, Math.Min(0.1f, dt));
            float a = Math.Abs(tiltX) < TiltDeadZone ? 0f : tiltX * MarbleAccel;
            MarbleV += a * dt;
            MarbleV *= (float)Math.Exp(-MarbleFriction * dt);
            MarbleV = Math.Max(-MarbleMaxSpeed, Math.Min(MarbleMaxSpeed, MarbleV));
            float from = MarbleX;
            float to = MarbleX + MarbleV * dt;

            // la crepa (se scoperta): basta passarci sopra, anche velocemente
            if (CrackExposed)
            {
                float lo = Math.Min(from, to), hi = Math.Max(from, to);
                if (hi >= CrackX - CrackHalfWidth && lo <= CrackX + CrackHalfWidth)
                {
                    MarbleX = CrackX;
                    MarbleV = 0f;
                    Stage = AlarmStage.Neighbour;
                    return MarbleEvent.Fell;
                }
            }

            float right = Math.Min(WallRight, TableX - TableHalfWidth) - MarbleRadius;
            float left = WallLeft + MarbleRadius;
            MarbleEvent ev = MarbleEvent.None;
            if (to > right)
            {
                to = right;
                if (Math.Abs(MarbleV) > BumpSpeed) ev = MarbleEvent.Bump;
                MarbleV = -MarbleV * 0.35f;
            }
            else if (to < left)
            {
                to = left;
                if (Math.Abs(MarbleV) > BumpSpeed) ev = MarbleEvent.Bump;
                MarbleV = -MarbleV * 0.35f;
            }
            MarbleX = to;
            return ev;
        }

        // ---------- Il vicino ----------

        /// <summary>Un suono forte e breve (o il pulsante alternativo): l'onda d'urto. True una volta sola.</summary>
        public bool LoudSound()
        {
            if (Stage != AlarmStage.Neighbour) return false;
            Stage = AlarmStage.Snoozed;
            return true;
        }

        /// <summary>Fine della scenetta di vittoria: true una volta sola.</summary>
        public bool TryComplete()
        {
            if (Stage != AlarmStage.Snoozed || CompletionReported) return false;
            CompletionReported = true;
            Stage = AlarmStage.Done;
            return true;
        }
    }
}
