using System;

namespace Gotcha.UI
{
    /// <summary>Le piccole animazioni dei cuori delle vite (una alla volta per cuore, scelte a caso).</summary>
    public enum HeartMove
    {
        None,       // fermo (tra un'animazione e l'altra)
        Hop,        // saltello con schiacciamento all'atterraggio e allungamento in volo
        Pulse,      // battito doppio "tu-tum"
        Pirouette,  // giravolta attorno all'asse verticale (scala x che passa per lo zero e si ribalta)
        Wiggle,     // dondolio a destra e sinistra che si smorza
    }

    /// <summary>La posa di un cuore in un istante: spostamento verticale (px del canvas), scale e rotazione Z (gradi).</summary>
    public struct HeartPose
    {
        public float OffsetY, ScaleX, ScaleY, RotationZ;

        public static HeartPose Rest => new HeartPose { ScaleX = 1f, ScaleY = 1f };
    }

    /// <summary>
    /// Logica pura (senza Unity) dei cuori "vivi" della barra delle vite: ogni cuore ha il suo generatore casuale
    /// (System.Random con un seme suo, quindi ripetibile nei test) e, a intervalli casuali di 1,5-4 s, fa una delle
    /// mosse di <see cref="HeartMove"/>, mai la stessa due volte di fila; tra una mossa e l'altra sta fermo.
    /// Il tempo è quello reale (GameUI passa Time.unscaledTime). Nessuna allocazione dopo la costruzione.
    /// </summary>
    public sealed class HeartIdle
    {
        public const float MinGap = 1.5f, MaxGap = 4f;
        public const float HopSeconds = 0.55f, PulseSeconds = 0.6f, PirouetteSeconds = 0.7f, WiggleSeconds = 0.65f;
        /// <summary>Altezza del saltello (px del canvas 1080x1920).</summary>
        public const float HopHeight = 26f;

        readonly Random rnd;
        HeartMove move = HeartMove.None, last = HeartMove.None;
        float moveStart, nextStart;
        bool started;

        public HeartIdle(int seed) { rnd = new Random(seed); }

        public HeartMove Current => move;
        /// <summary>Quando parte la prossima mossa (dopo la fine di quella in corso).</summary>
        public float NextStart => nextStart;

        public static float Duration(HeartMove m)
        {
            switch (m)
            {
                case HeartMove.Hop: return HopSeconds;
                case HeartMove.Pulse: return PulseSeconds;
                case HeartMove.Pirouette: return PirouetteSeconds;
                case HeartMove.Wiggle: return WiggleSeconds;
                default: return 0f;
            }
        }

        float Range(float a, float b) => a + (float)rnd.NextDouble() * (b - a);

        /// <summary>Riporta il cuore fermo (es. dopo che si è rotto o riempito): la prossima mossa arriva dopo una pausa.</summary>
        public void Rest(float now)
        {
            started = true;
            move = HeartMove.None;
            nextStart = now + Range(MinGap, MaxGap);
        }

        /// <summary>Avanza il programma fino a "now" e restituisce la posa. Se il tempo salta avanti, non recupera le mosse perse.</summary>
        public HeartPose Sample(float now)
        {
            if (!started)
            {
                started = true;
                // primo avvio: ogni cuore parte sfasato, così non si muovono mai tutti insieme
                nextStart = now + Range(0.3f, MaxGap);
            }
            if (move != HeartMove.None && now >= moveStart + Duration(move))
            {
                last = move;
                move = HeartMove.None;
                nextStart = moveStart + Duration(last) + Range(MinGap, MaxGap);
            }
            if (move == HeartMove.None && now >= nextStart)
            {
                // quattro mosse, mai la stessa di prima: si sceglie tra le altre tre
                int idx;
                if (last == HeartMove.None) idx = rnd.Next(4);
                else
                {
                    idx = rnd.Next(3);
                    if (idx >= (int)last - 1) idx++;
                }
                move = (HeartMove)(idx + 1);
                // se il programma era rimasto indietro (gioco in pausa, app in background) si parte da adesso
                moveStart = now - nextStart > 0.25f ? now : nextStart;
            }
            if (move == HeartMove.None) return HeartPose.Rest;
            float k = (now - moveStart) / Duration(move);
            return Pose(move, k < 0f ? 0f : k > 1f ? 1f : k);
        }

        /// <summary>La posa della mossa m al punto k (0..1). Inizio e fine coincidono con la posa a riposo.</summary>
        public static HeartPose Pose(HeartMove m, float k)
        {
            HeartPose p = HeartPose.Rest;
            switch (m)
            {
                case HeartMove.Hop:
                {
                    // 0-0.18 si accuccia e si rialza (carica), 0.18-0.78 in volo (parabola), 0.78-1 atterra schiacciandosi
                    if (k < 0.18f)
                    {
                        float s = (float)Math.Sin(k / 0.18f * Math.PI);
                        p.ScaleX = 1f + 0.16f * s;
                        p.ScaleY = 1f - 0.2f * s;
                    }
                    else if (k < 0.78f)
                    {
                        float a = (k - 0.18f) / 0.6f;
                        p.OffsetY = HopHeight * 4f * a * (1f - a);
                        float stretch = (float)Math.Sin(a * Math.PI);   // allungato a metà salto e mentre ricade
                        p.ScaleX = 1f - 0.08f * stretch;
                        p.ScaleY = 1f + 0.1f * stretch;
                    }
                    else
                    {
                        float b = (k - 0.78f) / 0.22f;
                        float s = (float)Math.Sin(b * Math.PI);
                        p.ScaleX = 1f + 0.14f * s;
                        p.ScaleY = 1f - 0.16f * s;
                    }
                    break;
                }
                case HeartMove.Pulse:
                {
                    // "tu-tum": due colpi, il secondo più piccolo
                    float s = Bump(k, 0.05f, 0.32f) * 0.24f + Bump(k, 0.38f, 0.7f) * 0.15f;
                    p.ScaleX = p.ScaleY = 1f + s;
                    break;
                }
                case HeartMove.Pirouette:
                {
                    // giro completo attorno all'asse verticale: la larghezza segue il coseno (si vede il "retro"
                    // ribaltato a metà giro), con un piccolo sollevamento e una lieve inclinazione
                    double e = Ease(k);
                    p.ScaleX = (float)Math.Cos(e * Math.PI * 2.0);
                    if (Math.Abs(p.ScaleX) < 0.08f) p.ScaleX = p.ScaleX < 0f ? -0.08f : 0.08f;   // mai una riga invisibile
                    p.OffsetY = (float)Math.Sin(k * Math.PI) * 10f;
                    p.ScaleY = 1f + (float)Math.Sin(k * Math.PI) * 0.06f;
                    p.RotationZ = (float)Math.Sin(k * Math.PI * 2.0) * 6f;
                    break;
                }
                case HeartMove.Wiggle:
                {
                    float damp = 1f - k;
                    p.RotationZ = (float)Math.Sin(k * Math.PI * 6.0) * 16f * damp;
                    p.ScaleX = p.ScaleY = 1f + (float)Math.Sin(k * Math.PI) * 0.05f;
                    break;
                }
            }
            return p;
        }

        /// <summary>Campana liscia tra a e b (0 fuori), massimo 1 a metà.</summary>
        static float Bump(float k, float a, float b)
        {
            if (k <= a || k >= b) return 0f;
            float x = (k - a) / (b - a);
            return (float)Math.Sin(x * Math.PI);
        }

        static double Ease(float k) => k * k * (3.0 - 2.0 * k);
    }
}
