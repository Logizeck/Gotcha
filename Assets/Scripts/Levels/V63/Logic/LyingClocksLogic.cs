using System;

namespace Gotcha.Levels.V63
{
    /// <summary>Stati del livello 257 (la porta che si apre solo all'ora giusta).</summary>
    public enum ClockState
    {
        Running,    // il tempo scorre, si imposta l'ora sul tastierino
        Opening,    // ora giusta: la porta si apre (input bloccato)
        Failed,     // tre errori: "Time's up. Literally."
        Done,
    }

    /// <summary>Come risponde la porta a OK.</summary>
    public enum ClockAnswer { None, Correct, Wrong, CopiedStopped, CopiedFast, CopiedCuckoo }

    /// <summary>
    /// Logica pura del livello 257 "The lying clocks". Il tempo di gioco scorre accelerato (un minuto ogni 3 secondi
    /// reali), con un'ora di partenza casuale. Tre orologi, tutti bugiardi: il cucù va alla velocità giusta ma è indietro
    /// di 10-25 minuti (e canta allo scoccare dell'ora VERA, quindi quando lui segna xx:(60 - ritardo)); la sveglia è
    /// ferma; l'orologio da timbro corre il doppio. Si risolve guardando quando canta il cucù (l'ora vera è HH:00, i
    /// cucù sono HH): si ricava il ritardo e si imposta l'ora vera corrente (±1 minuto). Tre errori e si perde.
    /// Le ore sono minuti da mezzogiorno/mezzanotte, 0-719 (orologi da 12 ore). Nessuna dipendenza da Unity.
    /// </summary>
    public sealed class LyingClocksLogic
    {
        public const float SecondsPerMinute = 3f;
        public const int Day = 720;
        public const int MaxMistakes = 3;
        /// <summary>Tolleranza (minuti di gioco) sull'ora inserita.</summary>
        public const int Tolerance = 1;
        public const int MinDelay = 10, MaxDelay = 25;
        /// <summary>Minuti che mancano all'ora piena alla partenza: il primo cucù arriva dopo 27-39 secondi reali.</summary>
        public const int MinLead = 9, MaxLead = 13;

        public ClockState State { get; private set; } = ClockState.Running;
        public int StartMinute { get; private set; }
        public int CuckooDelay { get; private set; }
        public int StoppedMinute { get; private set; }
        public int FastOffset { get; private set; }
        public float Elapsed { get; private set; }
        /// <summary>Tastierino: ora 1-12 e minuti 0-59.</summary>
        public int Hour { get; private set; } = 12;
        public int Minute { get; private set; }
        public int Mistakes { get; private set; }
        public int CuckooCalls { get; private set; }
        public ClockAnswer LastAnswer { get; private set; }
        public bool CompletionReported { get; private set; }

        public bool InputLocked => State != ClockState.Running;

        public LyingClocksLogic() { Reset(0); }

        /// <summary>Nuova partita: ore e ritardi casuali (dal seme), tastierino a 12:00.</summary>
        public void Reset(int seed)
        {
            var rnd = new Random(seed);
            int hour = rnd.Next(1, 13);
            int lead = rnd.Next(MinLead, MaxLead + 1);
            StartMinute = Wrap(hour * 60 - lead);
            CuckooDelay = rnd.Next(MinDelay, MaxDelay + 1);
            StoppedMinute = Wrap(StartMinute - rnd.Next(40, 171));
            FastOffset = rnd.Next(25, 51);
            State = ClockState.Running;
            Elapsed = 0f;
            Hour = 12;
            Minute = 0;
            Mistakes = 0;
            CuckooCalls = 0;
            LastAnswer = ClockAnswer.None;
            CompletionReported = false;
        }

        // ---------- gli orologi ----------

        public float TrueTime => WrapF(StartMinute + Elapsed / SecondsPerMinute);
        /// <summary>Il cucù: giusto di velocità, indietro di CuckooDelay minuti.</summary>
        public float CuckooTime => WrapF(TrueTime - CuckooDelay);
        /// <summary>La sveglia: ferma.</summary>
        public float StoppedTime => StoppedMinute;
        /// <summary>L'orologio da timbro: corre il doppio.</summary>
        public float FastTime => WrapF(StartMinute + FastOffset + 2f * Elapsed / SecondsPerMinute);
        /// <summary>Secondi reali al prossimo cucù.</summary>
        public float SecondsToNextCuckoo => (60f - TrueTime % 60f) * SecondsPerMinute;

        /// <summary>
        /// Fa scorrere il tempo (secondi reali). Se l'ora vera scocca, il cucù canta: restituisce quante volte (l'ora,
        /// 1-12), altrimenti 0.
        /// </summary>
        public int Tick(float dt)
        {
            if (dt <= 0f || State == ClockState.Failed || State == ClockState.Done) return 0;
            float before = StartMinute + Elapsed / SecondsPerMinute;
            Elapsed += dt;
            float after = StartMinute + Elapsed / SecondsPerMinute;
            if (Math.Floor(after / 60f) <= Math.Floor(before / 60f)) return 0;
            CuckooCalls++;
            return HourOf((float)(Math.Floor(after / 60f) * 60f));
        }

        // ---------- il tastierino ----------

        public int Entered => (Hour % 12) * 60 + Minute;

        public bool StepHour(int d)
        {
            if (InputLocked || d == 0) return false;
            Hour = ((Hour - 1 + d) % 12 + 12) % 12 + 1;
            return true;
        }

        public bool StepMinute(int d)
        {
            if (InputLocked || d == 0) return false;
            Minute = ((Minute + d) % 60 + 60) % 60;
            return true;
        }

        /// <summary>
        /// OK: l'ora inserita è quella vera (±1 minuto)? Se no è un errore, con la battuta per l'orologio da cui
        /// probabilmente è stata copiata; al terzo errore si perde.
        /// </summary>
        public ClockAnswer TryOk()
        {
            if (InputLocked) return ClockAnswer.None;
            int e = Entered;
            if (Near(e, TrueTime))
            {
                State = ClockState.Opening;
                return LastAnswer = ClockAnswer.Correct;
            }
            Mistakes++;
            if (Mistakes >= MaxMistakes) State = ClockState.Failed;
            if (Near(e, CuckooTime)) return LastAnswer = ClockAnswer.CopiedCuckoo;
            if (Near(e, StoppedTime)) return LastAnswer = ClockAnswer.CopiedStopped;
            if (Near(e, FastTime)) return LastAnswer = ClockAnswer.CopiedFast;
            return LastAnswer = ClockAnswer.Wrong;
        }

        public bool TryCompleteOpen()
        {
            if (State != ClockState.Opening || CompletionReported) return false;
            State = ClockState.Done;
            CompletionReported = true;
            return true;
        }

        // ---------- aiuti ----------

        static bool Near(int entered, float clock) => Diff(entered, (float)Math.Floor(clock)) <= Tolerance;

        /// <summary>Distanza in minuti su un quadrante da 12 ore.</summary>
        public static float Diff(float a, float b)
        {
            float d = Math.Abs(WrapF(a) - WrapF(b));
            return Math.Min(d, Day - d);
        }

        public static int Wrap(int m) => ((m % Day) + Day) % Day;
        public static float WrapF(float m) { float r = m % Day; return r < 0f ? r + Day : r; }
        /// <summary>L'ora (1-12) di un'ora del quadrante.</summary>
        public static int HourOf(float t) { int h = (int)Math.Floor(WrapF(t) / 60f) % 12; return h == 0 ? 12 : h; }
        public static int MinuteOf(float t) => (int)Math.Floor(WrapF(t)) % 60;
        public static string Format(int hour, int minute) => hour.ToString("00") + ":" + minute.ToString("00");

        /// <summary>Le battute della porta (in inglese, testo di gioco).</summary>
        public static string Answer(ClockAnswer a)
        {
            switch (a)
            {
                case ClockAnswer.Correct: return "Right on time!";
                case ClockAnswer.CopiedStopped: return "That one died yesterday.";
                case ClockAnswer.CopiedFast: return "That one drinks coffee.";
                case ClockAnswer.CopiedCuckoo: return "Close... but that bird is late.";
                case ClockAnswer.Wrong: return "Wrong! That clock lies.";
                default: return "";
            }
        }

        public const string FailTaunt = "Time's up. Literally.";

        static readonly string[] Radio =
        {
            "It's... time for music!",
            "The time is now. It's always now.",
            "Beep. Beep. Beeeep. That was the weather.",
            "And now a word from our sponsor: tick.",
            "It's a quarter past... something. Probably.",
            "Breaking news: clocks are liars.",
        };

        /// <summary>Le frasi inutili della radio, una dopo l'altra.</summary>
        public static string RadioLine(int n) => Radio[((n % Radio.Length) + Radio.Length) % Radio.Length];
    }
}
