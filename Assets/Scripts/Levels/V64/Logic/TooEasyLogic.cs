namespace Gotcha.Levels.V64
{
    /// <summary>Fase del livello "Too Easy!" (roadmap n. 34).</summary>
    public enum TooEasyStage
    {
        Playing,        // si gioca: il pulsante WIN scappa, le parole dell'obiettivo si toccano
        Celebrating,    // LEVEL toccata: la parola sparisce, festa (input chiuso)
    }

    /// <summary>Cosa succede premendo il pulsante WIN.</summary>
    public enum TooEasyPress
    {
        Ignored,    // durante il salto o dopo la vittoria: non conta
        Jumped,     // "TOO EASY!": il pulsante salta in un altro punto (non vince MAI)
    }

    /// <summary>Cosa succede toccando una parola dell'obiettivo.</summary>
    public enum TooEasyTap
    {
        Ignored,    // indice fuori dalla frase o dopo la vittoria
        Wrong,      // una parola qualsiasi (non quella giusta): oscilla, battuta
        Solved,     // la parola giusta (LEVEL): sparisce, vittoria (una volta sola)
    }

    /// <summary>
    /// Logica pura del livello "Too Easy!" (id too_easy, roadmap n. 34) — l'obiettivo "COMPLETE THIS LEVEL." È il
    /// rompicapo. In mezzo allo schermo c'è un grosso pulsante verde WIN: ogni pressione fa apparire "TOO EASY!" e il
    /// pulsante salta in un altro punto (sequenza fissa, sempre dentro i limiti); dopo qualche tentativo tira fuori la
    /// lingua, cambia scritta ("WIN?", poi "WIN!!") e prende in giro. Il pulsante NON fa MAI vincere.
    /// Soluzione esatta: toccare la parola LEVEL (indice <see cref="Target"/>) nella frase dell'obiettivo: sparisce
    /// ("COMPLETE THIS ___.") e il livello è completato perché non c'è più nessun livello. Le altre parole oscillano
    /// soltanto. Niente sconfitte, niente stati senza uscita: Cancel() (pausa, fuoco perso) chiude il salto sul posto.
    /// Le parole sono oggetti a sé: la riga si impagina dalle LARGHEZZE misurate (<see cref="Row"/>), si rimpicciolisce
    /// se non entra (<see cref="FitScale"/>) e ogni parola ha un'area di tocco generosa e senza sovrapposizioni
    /// (<see cref="HitSpans"/>): una traduzione cambierebbe solo le stringhe. Nessuna dipendenza da Unity.
    /// </summary>
    public sealed class TooEasyLogic
    {
        /// <summary>Durata del salto (animazione): le pressioni in questo tempo non contano.</summary>
        public const float JumpTime = 0.3f;
        /// <summary>Pressioni dopo cui il pulsante tira fuori la lingua.</summary>
        public const int TongueAfter = 3;
        /// <summary>Pressioni dopo cui la scritta diventa "WIN?".</summary>
        public const int UnsureAfter = 4;
        /// <summary>Pressioni dopo cui la scritta diventa "WIN!!".</summary>
        public const int ShoutAfter = 6;
        /// <summary>Pressioni dopo cui il pulsante dice una frase-indizio (una volta sola).</summary>
        public const int NudgeAfter = 8;
        /// <summary>Distanza minima fra due posti consecutivi del pulsante (nelle unità dei limiti di default).</summary>
        public const float MinJump = 1.0f;

        /// <summary>
        /// La sequenza fissa dei posti, normalizzata nei limiti (0 = minimo, 1 = massimo): coppie (u, v). Pensata perché
        /// due posti consecutivi siano lontani anche tornando all'inizio del giro.
        /// </summary>
        static readonly float[] Spots =
        {
            0.95f, 0.92f,
            0.05f, 0.28f,
            0.92f, 0.08f,
            0.10f, 0.95f,
            0.62f, 0.45f,
            0.00f, 0.02f,
            1.00f, 0.62f,
            0.30f, 0.70f,
            0.85f, 0.30f,
            0.15f, 0.55f,
        };

        public static int SpotCount => Spots.Length / 2;

        readonly float minX, maxX, minY, maxY, startX, startY;
        readonly int target, wordCount;
        int[] wordTaps;
        float jumpT;
        bool completed, nudged;

        /// <summary>
        /// Limiti del CENTRO del pulsante (già ristretti della sua mezza misura), posto di partenza, numero di parole
        /// della frase dell'obiettivo e indice della parola giusta (LEVEL).
        /// </summary>
        public TooEasyLogic(float minX, float maxX, float minY, float maxY, float startX, float startY, int wordCount = 4, int target = 2)
        {
            this.minX = Safe(System.Math.Min(minX, maxX));
            this.maxX = Safe(System.Math.Max(minX, maxX));
            this.minY = Safe(System.Math.Min(minY, maxY));
            this.maxY = Safe(System.Math.Max(minY, maxY));
            this.startX = Clamp(Safe(startX), this.minX, this.maxX);
            this.startY = Clamp(Safe(startY), this.minY, this.maxY);
            this.wordCount = wordCount < 1 ? 1 : wordCount;
            this.target = target < 0 ? 0 : target >= this.wordCount ? this.wordCount - 1 : target;
            Reset();
        }

        public TooEasyStage Stage { get; private set; }
        public float X { get; private set; }
        public float Y { get; private set; }
        /// <summary>Da dove è partito l'ultimo salto (per l'animazione e per il "TOO EASY!").</summary>
        public float FromX { get; private set; }
        public float FromY { get; private set; }
        /// <summary>Pressioni contate del pulsante WIN (quelle durante il salto non contano).</summary>
        public int Presses { get; private set; }
        public bool Jumping { get; private set; }
        /// <summary>Avanzamento del salto 0..1 (1 fuori dal salto).</summary>
        public float JumpProgress => Jumping ? Clamp(jumpT / JumpTime, 0f, 1f) : 1f;
        public int Target => target;
        public int WordCount => wordCount;
        public bool InputLocked => Stage != TooEasyStage.Playing;
        /// <summary>Dopo <see cref="TongueAfter"/> pressioni il pulsante fa la linguaccia.</summary>
        public bool Tongue => Presses >= TongueAfter;
        /// <summary>0 "WIN", 1 "WIN?", 2 "WIN!!".</summary>
        public int LabelStage => Presses >= ShoutAfter ? 2 : Presses >= UnsureAfter ? 1 : 0;
        public int WrongWordTaps
        {
            get
            {
                int n = 0;
                for (int i = 0; i < wordTaps.Length; i++) if (i != target) n += wordTaps[i];
                return n;
            }
        }
        public int WrongMoves => Presses + WrongWordTaps;
        public float MinX => minX;
        public float MaxX => maxX;
        public float MinY => minY;
        public float MaxY => maxY;

        public int WordTaps(int index) => index >= 0 && index < wordTaps.Length ? wordTaps[index] : 0;

        /// <summary>Si ricomincia da capo (rientro nel livello): stesso posto, stessa sequenza di salti.</summary>
        public void Reset()
        {
            Stage = TooEasyStage.Playing;
            X = FromX = startX;
            Y = FromY = startY;
            Presses = 0;
            Jumping = false;
            jumpT = 0f;
            wordTaps = new int[wordCount];
            completed = nudged = false;
        }

        /// <summary>Il posto n-esimo della sequenza (n da 0), dentro i limiti. Deterministico.</summary>
        public void SpotAt(int n, out float x, out float y)
        {
            int k = ((n % SpotCount) + SpotCount) % SpotCount;
            x = Clamp(minX + Spots[2 * k] * (maxX - minX), minX, maxX);
            y = Clamp(minY + Spots[2 * k + 1] * (maxY - minY), minY, maxY);
        }

        /// <summary>Il pulsante WIN viene premuto: "TOO EASY!" e salto al prossimo posto. Non vince mai.</summary>
        public TooEasyPress PressWin()
        {
            if (Stage != TooEasyStage.Playing || Jumping) return TooEasyPress.Ignored;
            FromX = X;
            FromY = Y;
            SpotAt(Presses, out float nx, out float ny);
            X = nx;
            Y = ny;
            Presses++;
            Jumping = true;
            jumpT = 0f;
            return TooEasyPress.Jumped;
        }

        /// <summary>
        /// La frase-indizio dopo <see cref="NudgeAfter"/> pressioni: true una volta sola (da chiamare dopo un Jumped).
        /// </summary>
        public bool TakeNudge()
        {
            if (nudged || Presses < NudgeAfter || Stage != TooEasyStage.Playing) return false;
            nudged = true;
            return true;
        }

        /// <summary>Tocco sulla parola numero <paramref name="index"/> della frase dell'obiettivo.</summary>
        public TooEasyTap TapWord(int index)
        {
            if (Stage != TooEasyStage.Playing || index < 0 || index >= wordCount) return TooEasyTap.Ignored;
            wordTaps[index]++;
            if (index != target) return TooEasyTap.Wrong;
            Stage = TooEasyStage.Celebrating;
            Jumping = false;
            jumpT = 0f;
            return TooEasyTap.Solved;
        }

        /// <summary>Passo di tempo. True quando un salto è appena finito.</summary>
        public bool Tick(float dt)
        {
            if (!Jumping) return false;
            if (!(dt > 0f)) return false;
            jumpT += dt;
            if (jumpT < JumpTime) return false;
            Jumping = false;
            jumpT = 0f;
            return true;
        }

        /// <summary>Dita perse (pausa, fuoco perso, livello spento): il salto si chiude sul posto nuovo.</summary>
        public void Cancel()
        {
            Jumping = false;
            jumpT = 0f;
        }

        /// <summary>True una volta sola, dopo la vittoria (il livello chiama Win solo allora).</summary>
        public bool TryComplete()
        {
            if (Stage != TooEasyStage.Celebrating || completed) return false;
            completed = true;
            return true;
        }

        // ---------- Impaginazione della frase (dalle larghezze misurate) ----------

        /// <summary>
        /// Centri delle parole in una riga centrata sull'origine: larghezze misurate e uno spazio fisso tra le parole
        /// (gaps[i] = spazio PRIMA della parola i; il primo è ignorato). Larghezze negative/NaN valgono 0.
        /// </summary>
        public static float[] Row(float[] widths, float[] gaps, out float total)
        {
            int n = widths == null ? 0 : widths.Length;
            var centers = new float[n];
            total = 0f;
            for (int i = 0; i < n; i++)
            {
                if (i > 0) total += Gap(gaps, i);
                total += Max0(widths[i]);
            }
            float x = -total * 0.5f;
            for (int i = 0; i < n; i++)
            {
                if (i > 0) x += Gap(gaps, i);
                float w = Max0(widths[i]);
                centers[i] = x + w * 0.5f;
                x += w;
            }
            return centers;
        }

        /// <summary>Scala (≤ 1) che fa entrare una riga larga <paramref name="total"/> in <paramref name="maxWidth"/>.</summary>
        public static float FitScale(float total, float maxWidth)
        {
            if (!(maxWidth > 0f) || !(total > maxWidth) || float.IsInfinity(total)) return float.IsInfinity(total) ? 0.01f : 1f;
            return System.Math.Max(0.01f, maxWidth / total);
        }

        /// <summary>
        /// Aree di tocco orizzontali delle parole (coordinate della riga): ognuna arriva a metà dello spazio verso le
        /// vicine (nessuna sovrapposizione, nessun buco) e le due esterne si allargano di <paramref name="outerPad"/>.
        /// Ogni area è larga almeno <paramref name="minWidth"/> quando c'è posto (le esterne si allargano verso fuori,
        /// le interne fino alle parole vicine, mai sopra di esse: ogni parola resta sempre tutta toccabile).
        /// </summary>
        public static void HitSpans(float[] centers, float[] widths, float outerPad, float minWidth, out float[] left, out float[] right)
        {
            int n = centers == null ? 0 : centers.Length;
            left = new float[n];
            right = new float[n];
            for (int i = 0; i < n; i++)
            {
                float w = widths != null && i < widths.Length ? Max0(widths[i]) : 0f;
                float l = centers[i] - w * 0.5f, r = centers[i] + w * 0.5f;
                if (i == 0) left[i] = l - Max0(outerPad);
                else
                {
                    float pw = widths != null && i - 1 < widths.Length ? Max0(widths[i - 1]) : 0f;
                    left[i] = (centers[i - 1] + pw * 0.5f + l) * 0.5f;
                }
                if (i == n - 1) right[i] = r + Max0(outerPad);
                else
                {
                    float nw = widths != null && i + 1 < widths.Length ? Max0(widths[i + 1]) : 0f;
                    right[i] = (r + centers[i + 1] - nw * 0.5f) * 0.5f;
                }
            }
            // parole cortissime (il punto): l'area si allarga fino a minWidth rubando spazio solo alle aree esterne
            for (int i = 0; i < n; i++)
            {
                float lack = Max0(minWidth) - (right[i] - left[i]);
                if (lack <= 0f) continue;
                if (i == n - 1) right[i] += lack;
                else if (i == 0) left[i] -= lack;
                else
                {
                    // interna: si allarga attorno al suo centro, le vicine si accorciano di conseguenza ma senza mai
                    // coprire le loro parole (ogni parola resta sempre tutta toccabile)
                    float prevEdge = centers[i - 1] + (widths != null && i - 1 < widths.Length ? Max0(widths[i - 1]) : 0f) * 0.5f;
                    float nextEdge = centers[i + 1] - (widths != null && i + 1 < widths.Length ? Max0(widths[i + 1]) : 0f) * 0.5f;
                    left[i] = System.Math.Max(prevEdge, left[i] - lack * 0.5f);
                    right[i] = System.Math.Min(nextEdge, right[i] + lack * 0.5f);
                    right[i - 1] = left[i];
                    left[i + 1] = right[i];
                }
            }
        }

        /// <summary>Quale area contiene x (-1 se nessuna).</summary>
        public static int SpanAt(float[] left, float[] right, float x)
        {
            if (left == null || right == null || float.IsNaN(x)) return -1;
            for (int i = 0; i < left.Length && i < right.Length; i++)
                if (x >= left[i] && x <= right[i]) return i;
            return -1;
        }

        static float Gap(float[] gaps, int i) => gaps != null && i < gaps.Length ? Max0(gaps[i]) : 0f;
        static float Max0(float v) => float.IsNaN(v) || v < 0f ? 0f : float.IsInfinity(v) ? 1e6f : v;
        static float Safe(float v) => float.IsNaN(v) || float.IsInfinity(v) ? 0f : v;
        static float Clamp(float v, float lo, float hi) => v < lo ? lo : v > hi ? hi : v;
    }
}
