using System;

namespace Gotcha.Levels.V64
{
    /// <summary>
    /// Gravità filtrata (passa-basso) vista nello schermo, per i livelli 251/253/254. Il gioco è bloccato in verticale:
    /// x verso destra, y verso l'alto (telefono dritto: y ≈ -1; capovolto: y ≈ +1). "Capovolto" ha l'isteresi (si
    /// accende sopra +0.5, si spegne sotto +0.2): niente angoli esatti, niente sfarfallio vicino alla soglia.
    /// Nessuna dipendenza da Unity.
    /// </summary>
    public sealed class V64AGravity
    {
        public const float InvertOn = 0.5f, InvertOff = 0.2f;
        /// <summary>Sotto questa intensità (telefono quasi piatto sul tavolo) l'angolo resta quello di prima.</summary>
        public const float MinTiltForAngle = 0.35f;

        /// <summary>Velocità del filtro (1/s): più alto = segue prima, più basso = più liscio.</summary>
        public float Smoothing = 6f;

        public float X { get; private set; }
        public float Y { get; private set; } = -1f;
        public bool Inverted { get; private set; }
        /// <summary>Direzione della gravità sullo schermo in gradi: 0 = in giù (telefono dritto), 180 = in su.</summary>
        public float AngleDeg { get; private set; }

        bool primed;

        public void Reset(float x = 0f, float y = -1f)
        {
            X = x;
            Y = y;
            Inverted = false;
            AngleDeg = 0f;
            primed = false;
        }

        /// <summary>Nuova lettura dell'accelerometro (già nel sistema dello schermo).</summary>
        public void Feed(float x, float y, float dt)
        {
            if (float.IsNaN(x) || float.IsNaN(y)) return;
            if (!primed) { X = x; Y = y; primed = true; }
            else
            {
                float k = 1f - (float)Math.Exp(-Smoothing * Math.Max(0f, dt));
                X += (x - X) * k;
                Y += (y - Y) * k;
            }
            if (!Inverted && Y > InvertOn) Inverted = true;
            else if (Inverted && Y < InvertOff) Inverted = false;
            if (Magnitude >= MinTiltForAngle) AngleDeg = (float)(Math.Atan2(X, -Y) * 180.0 / Math.PI);
        }

        public float Magnitude => (float)Math.Sqrt(X * X + Y * Y);
    }

    /// <summary>
    /// Soglia del microfono calibrata sul rumore di fondo: il fondo si aggiorna solo quando il suono è sotto la soglia,
    /// così un soffio lungo non "si mangia" la soglia. Riconosce solo l'ampiezza (niente parole).
    /// </summary>
    public sealed class V64AMicGate
    {
        public float MinThreshold = 0.22f;
        public float MaxThreshold = 0.75f;
        public float Factor = 1.6f;
        public float Margin = 0.1f;
        /// <summary>Costante di tempo (s) dell'adattamento al fondo.</summary>
        public float AmbientTau = 2f;

        public float Ambient { get; private set; } = 0.05f;

        public float Threshold => Math.Min(MaxThreshold, Math.Max(MinThreshold, Ambient * Factor + Margin));

        public void Reset(float ambient = 0.05f) { Ambient = ambient; }

        /// <summary>True se il livello supera la soglia (soffio o rumore forte).</summary>
        public bool Feed(float level, float dt)
        {
            if (float.IsNaN(level) || level < 0f) level = 0f;
            bool loud = level > Threshold;
            if (!loud)
            {
                float k = 1f - (float)Math.Exp(-Math.Max(0f, dt) / Math.Max(0.01f, AmbientTau));
                Ambient += (level - Ambient) * k;
            }
            return loud;
        }
    }

    /// <summary>
    /// Un solo suono breve e forte (battito di mani, "BAM!"): scatta sul fronte di salita, poi deve tornare il silenzio
    /// per un attimo prima di poter scattare di nuovo. All'inizio è disarmato: un rumore che c'era già quando la fase
    /// col microfono è cominciata non conta.
    /// </summary>
    public sealed class V64AImpulse
    {
        public float LoudMin = 0.45f;
        public float LoudAboveThreshold = 0.15f;
        public float QuietToArm = 0.25f;

        readonly V64AMicGate gate = new V64AMicGate();
        float quietFor;

        public bool Armed { get; private set; }
        public V64AMicGate Gate => gate;
        public float LoudLevel => Math.Max(LoudMin, gate.Threshold + LoudAboveThreshold);

        public void Reset()
        {
            gate.Reset();
            quietFor = 0f;
            Armed = false;
        }

        public bool Feed(float level, float dt)
        {
            float loudLevel = LoudLevel;
            bool above = gate.Feed(level, dt);
            if (!above) { quietFor += Math.Max(0f, dt); if (quietFor >= QuietToArm) Armed = true; return false; }
            quietFor = 0f;
            if (!Armed || level < loudLevel) return false;
            Armed = false;
            return true;
        }
    }
}
