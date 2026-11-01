using System;

namespace Gotcha.UI
{
    /// <summary>
    /// I tempi (secondi, tempo reale) della schermata "SECRET FOUND!" che precede l'esito del livello quando si trova
    /// un Easter egg. Due versioni: completa (~5 s, quanto la fanfara) alla prima scoperta, breve (~2.2 s) se il
    /// segreto era già stato trovato. Un valore negativo vuol dire "questa fase non c'è".
    /// </summary>
    public readonly struct EggCelebrationPlan
    {
        public readonly bool Full;           // versione completa (prima scoperta)
        public readonly bool PieceFlight;    // il pezzo nuovo del puzzle vola nell'anteprima
        public readonly float Duration;      // fine naturale (dissolvenza compresa)
        public readonly float SkipAfter;     // da qui in poi un tocco salta alla fine
        public readonly float EndFade;       // durata della dissolvenza finale naturale
        public readonly float VeilIn;        // il velo scuro arriva al massimo
        public readonly float RaysStart, RaysEnd;
        public readonly float DropStart, DropEnd;      // l'uovo cade dall'alto e rimbalza
        public readonly float SlamStart, SlamEnd;      // il titolo "sbatte" sullo schermo: SlamEnd = impatto
        public readonly float ShakeEnd;                // fine del tremolio dello schermo
        public readonly float NameStart, NameEnd;      // nome del segreto
        public readonly float HintAt;                  // compare "TAP TO CONTINUE"
        public readonly float PreviewStart, PreviewEnd;  // l'anteprima del puzzle di Toni
        public readonly float CountStart;              // il contatore inizia a salire
        public readonly float FlightStart, FlightEnd;  // volo del pezzo: FlightEnd = atterraggio

        EggCelebrationPlan(bool full, bool flight, float duration, float skipAfter, float endFade, float veilIn,
            float raysStart, float raysEnd, float dropStart, float dropEnd, float slamStart, float slamEnd, float shakeEnd,
            float nameStart, float nameEnd, float hintAt, float previewStart, float previewEnd, float countStart,
            float flightStart, float flightEnd)
        {
            Full = full; PieceFlight = flight; Duration = duration; SkipAfter = skipAfter; EndFade = endFade; VeilIn = veilIn;
            RaysStart = raysStart; RaysEnd = raysEnd; DropStart = dropStart; DropEnd = dropEnd;
            SlamStart = slamStart; SlamEnd = slamEnd; ShakeEnd = shakeEnd; NameStart = nameStart; NameEnd = nameEnd;
            HintAt = hintAt; PreviewStart = previewStart; PreviewEnd = previewEnd; CountStart = countStart;
            FlightStart = flightStart; FlightEnd = flightEnd;
        }

        /// <summary>Versione completa: 5 s, come la fanfara. Senza pezzo nuovo (puzzle già pieno) niente volo.</summary>
        public static EggCelebrationPlan Complete(bool pieceFlight) => new EggCelebrationPlan(
            true, pieceFlight, EggCelebrationTimeline.FullSeconds, EggCelebrationTimeline.SkipAfterFull, 0.6f, 0.35f,
            0.05f, 0.6f, 0.15f, 0.75f, 0.8f, 0.98f, 1.4f,
            1.15f, 1.5f, EggCelebrationTimeline.SkipAfterFull, 1.9f, 2.25f, pieceFlight ? 1.9f : -1f,
            pieceFlight ? 2.45f : -1f, pieceFlight ? 3.25f : -1f);

        /// <summary>Versione breve (segreto già trovato): la fanfara parte e sfuma, nessun pezzo nuovo.</summary>
        public static EggCelebrationPlan Short() => new EggCelebrationPlan(
            false, false, EggCelebrationTimeline.ShortSeconds, EggCelebrationTimeline.SkipAfterShort, 0.5f, 0.25f,
            0f, 0.4f, 0.05f, 0.45f, 0.45f, 0.6f, 0.9f,
            0.65f, 0.9f, EggCelebrationTimeline.SkipAfterShort, 0.7f, 0.95f, -1f,
            -1f, -1f);
    }

    /// <summary>
    /// L'orologio della sequenza: avanza a passi limitati (così dopo una pausa dell'app non salta tutto in un colpo e
    /// resta in sincrono con la fanfara, che Unity mette in pausa insieme all'app), gestisce il tocco per saltare e
    /// dice quando far comparire l'esito del livello (all'inizio della dissolvenza, così le due cose si incrociano).
    /// </summary>
    public sealed class EggCelebrationClock
    {
        public EggCelebrationPlan Plan { get; private set; }
        public float T { get; private set; }
        public float EndAt { get; private set; }
        public bool Skipped { get; private set; }
        /// <summary>Durata della dissolvenza in corso (naturale o dopo il salto).</summary>
        public float Fade { get; private set; }

        public EggCelebrationClock(EggCelebrationPlan plan) { Restart(plan); }

        public void Restart(EggCelebrationPlan plan)
        {
            Plan = plan;
            T = 0f;
            EndAt = plan.Duration;
            Fade = plan.EndFade;
            Skipped = false;
        }

        /// <summary>Avanza di dt (limitato a MaxStep: un fotogramma lunghissimo dopo il ritorno nell'app conta poco).</summary>
        public float Advance(float dt)
        {
            if (dt > 0f) T = Math.Min(T + Math.Min(dt, EggCelebrationTimeline.MaxStep), EndAt);
            return T;
        }

        public bool CanSkip => !Skipped && T >= Plan.SkipAfter && T < FadeStart;

        /// <summary>Tocco (o tasto Indietro): dopo SkipAfter si passa a una dissolvenza rapida. True se ha avuto effetto.</summary>
        public bool TrySkip()
        {
            if (!CanSkip) return false;
            Skipped = true;
            Fade = EggCelebrationTimeline.SkipFade;
            EndAt = T + Fade;
            return true;
        }

        public float FadeStart => EndAt - Fade;
        /// <summary>È ora di mostrare l'esito del livello (sotto la dissolvenza).</summary>
        public bool ResultDue => T >= FadeStart;
        public bool Done => T >= EndAt;

        /// <summary>Opacità di tutta la scena: entra col velo, esce con la dissolvenza.</summary>
        public float SceneAlpha => Math.Min(EggCelebrationTimeline.Seg(T, 0f, Plan.VeilIn), 1f - EggCelebrationTimeline.Seg(T, FadeStart, EndAt));

        /// <summary>
        /// Volume della fanfara (0..1): piena per tutta la versione completa (finisce da sola insieme alla scena); sfuma
        /// con la scena se la versione è breve o se si è saltato.
        /// </summary>
        public float FanfareVolume => Plan.Full && !Skipped ? 1f : 1f - EggCelebrationTimeline.Seg(T, FadeStart, EndAt);
    }

    /// <summary>Curve pure della sequenza (nessuna allocazione).</summary>
    public static class EggCelebrationTimeline
    {
        public const float FullSeconds = 5f;      // la durata della fanfara EasterEggFanfare.ogg
        public const float ShortSeconds = 2.2f;
        public const float SkipAfterFull = 1.5f;
        public const float SkipAfterShort = 0.8f;
        public const float SkipFade = 0.3f;
        public const float MaxStep = 1f / 15f;
        /// <summary>Dopo la fanfara la musica risale in questo tempo (era abbassata a zero).</summary>
        public const float MusicReturnSeconds = 0.8f;

        public static float Clamp01(float v) => v < 0f ? 0f : v > 1f ? 1f : v;

        /// <summary>Avanzamento 0..1 di t nell'intervallo [a, b] (b &lt;= a: gradino in a).</summary>
        public static float Seg(float t, float a, float b) => b <= a ? (t >= a ? 1f : 0f) : Clamp01((t - a) / (b - a));

        public static float SmoothStep(float x) { x = Clamp01(x); return x * x * (3f - 2f * x); }

        /// <summary>Uscita "con rimbalzo oltre il bersaglio" (supera 1 e poi ci torna).</summary>
        public static float EaseOutBack(float x, float s = 1.70158f)
        {
            x = Clamp01(x) - 1f;
            return 1f + (s + 1f) * x * x * x + s * x * x;
        }

        /// <summary>
        /// Caduta dell'uovo: 1 = fuori in alto, 0 = a posto. Sotto zero = rimbalzo oltre (l'uovo "affonda" e risale).
        /// </summary>
        public static float DropOffset(float t, EggCelebrationPlan p)
        {
            if (t <= p.DropStart) return 1f;
            return 1f - EaseOutBack(Seg(t, p.DropStart, p.DropEnd), 2.4f);
        }

        /// <summary>Molla smorzata dopo un impatto all'istante 'at': 0 prima, poi oscilla e si spegne.</summary>
        public static float Spring(float t, float at, float amplitude, float freq = 18f, float damping = 7f)
        {
            if (t < at) return 0f;
            float u = t - at;
            return amplitude * (float)(Math.Exp(-damping * u) * Math.Cos(freq * u));
        }

        /// <summary>Scala del titolo: enorme e invisibile, poi sbatte (accelerando) e trema un po' sul posto.</summary>
        public static float SlamScale(float t, EggCelebrationPlan p)
        {
            if (t < p.SlamStart) return 2.6f;
            if (t < p.SlamEnd) { float x = Seg(t, p.SlamStart, p.SlamEnd); return 2.6f + (1f - 2.6f) * x * x; }
            return 1f - Spring(t, p.SlamEnd, 0.12f, 22f, 8f);
        }

        public static float SlamAlpha(float t, EggCelebrationPlan p) => Seg(t, p.SlamStart, p.SlamStart + (p.SlamEnd - p.SlamStart) * 0.5f);

        /// <summary>Tremolio dello schermo (in px del canvas) dopo l'impatto del titolo: forte e smorzato.</summary>
        public static void Shake(float t, EggCelebrationPlan p, float amplitude, out float x, out float y)
        {
            if (t < p.SlamEnd || t >= p.ShakeEnd) { x = 0f; y = 0f; return; }
            float u = Seg(t, p.SlamEnd, p.ShakeEnd);
            float a = amplitude * (1f - u) * (1f - u);
            x = a * (float)Math.Sin(t * 97f);
            y = a * (float)Math.Cos(t * 131f + 0.7f);
        }

        /// <summary>
        /// Contatore "PUZZLE X / 50": sale da 0 fino al pezzo prima del nuovo mentre il pezzo vola, e scatta sul totale
        /// all'atterraggio. Senza volo mostra subito il totale.
        /// </summary>
        public static int CounterValue(float t, EggCelebrationPlan p, int total)
        {
            if (total < 0) total = 0;
            if (!p.PieceFlight || p.CountStart < 0f) return total;
            if (t >= p.FlightEnd) return total;
            int before = Math.Max(0, total - 1);
            float x = Seg(t, p.CountStart, p.FlightEnd - 0.1f);
            return Math.Min(before, (int)Math.Floor(SmoothStep(x) * before + 0.0001f));
        }

        /// <summary>Volo del pezzo: progresso 0..1 (accelera e frena) e altezza dell'arco 0..1 (massima a metà).</summary>
        public static float Flight(float t, EggCelebrationPlan p, out float arc)
        {
            if (!p.PieceFlight) { arc = 0f; return 0f; }
            float x = Seg(t, p.FlightStart, p.FlightEnd);
            arc = 4f * x * (1f - x);
            return SmoothStep(x);
        }

        /// <summary>Volume della musica durante il ritorno dopo la fanfara (u = secondi dalla fine).</summary>
        public static float MusicReturn(float u) => SmoothStep(Seg(u, 0f, MusicReturnSeconds));

        // ---------- pezzo nuovo del puzzle ----------

        /// <summary>
        /// Indice (riga * colonne + colonna) del pezzo scoperto per ultimo quando i pezzi sbloccati sono 'unlocked':
        /// stessa permutazione di EggPuzzle (System.Random(2026), Fisher-Yates). -1 se fuori dai limiti.
        /// GameUI verifica comunque con EggPuzzle.IsUnlocked prima di usarlo.
        /// </summary>
        public static int NewestPiece(int unlocked, int pieces = 50, int seed = 2026)
        {
            if (unlocked < 1 || unlocked > pieces) return -1;
            int[] order = PieceOrder(pieces, seed);
            return order[unlocked - 1];
        }

        static int[] cachedOrder;
        static int cachedPieces = -1, cachedSeed;

        /// <summary>order[k] = il pezzo che si scopre col (k+1)-esimo Easter egg.</summary>
        public static int[] PieceOrder(int pieces = 50, int seed = 2026)
        {
            if (cachedOrder != null && cachedPieces == pieces && cachedSeed == seed) return cachedOrder;
            var order = new int[pieces];
            for (int i = 0; i < pieces; i++) order[i] = i;
            var rnd = new Random(seed);
            for (int i = pieces - 1; i > 0; i--) { int j = rnd.Next(i + 1); int tmp = order[i]; order[i] = order[j]; order[j] = tmp; }
            cachedOrder = order;
            cachedPieces = pieces;
            cachedSeed = seed;
            return order;
        }
    }
}
