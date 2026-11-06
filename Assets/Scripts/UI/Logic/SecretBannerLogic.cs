namespace Gotcha.UI
{
    /// <summary>
    /// Logica pura (senza Unity) del nastro dorato che, all'inizio di un livello con un Easter egg, avvisa che lì
    /// c'è un segreto da trovare. Il nastro scende da sotto i cuori, resta ~2,5 s e risale; un tocco lo fa risalire
    /// subito. Le frasi usano solo caratteri ASCII: i due font del gioco non hanno le stelle né la spunta
    /// (le stelline ai lati sono sprite).
    /// </summary>
    public static class SecretBannerLogic
    {
        /// <summary>Attesa dall'inizio del livello (il livello e l'eventuale consiglio compaiono prima).</summary>
        public const float Delay = 0.5f;
        public const float SlideSeconds = 0.35f;
        /// <summary>Permanenza a nastro fermo e completamente visibile.</summary>
        public const float HoldSeconds = 2.5f;

        public const string HiddenCaption = "SECRET EGG HIDDEN IN THIS LEVEL!";
        public const string FoundCaption = "SECRET EGG ALREADY FOUND";

        /// <summary>La frase: un richiamo vivace se il segreto è ancora da trovare, una conferma tranquilla se no.
        /// Senza le stelline-sprite si mettono asterischi ai lati.</summary>
        public static string Caption(bool alreadyFound, bool starIcons)
        {
            string c = alreadyFound ? FoundCaption : HiddenCaption;
            return starIcons ? c : "* " + c + " *";
        }

        /// <summary>Quando inizia a risalire (secondi dall'inizio del livello) se non viene toccato.</summary>
        public static float DefaultOutStart => Delay + SlideSeconds + HoldSeconds;

        /// <summary>Tocco a "age" secondi: risale da adesso (mai più tardi del previsto, mai prima di essere entrato).</summary>
        public static float OutStartAfterTap(float age, float outStart)
        {
            float t = age < Delay ? Delay : age;
            return t < outStart ? t : outStart;
        }

        /// <summary>Quanto è dentro il nastro (0 = nascosto sopra, 1 = in posizione), con un piccolo rimbalzo in entrata.</summary>
        public static float Visible(float age, float outStart)
        {
            if (age <= Delay) return 0f;
            float inK = (age - Delay) / SlideSeconds;
            float v = inK >= 1f ? 1f : BackOut(inK);
            if (age > outStart)
            {
                // in uscita parte da dove si trova (anche se toccato a metà entrata)
                float outK = (age - outStart) / SlideSeconds;
                if (outK >= 1f) return 0f;
                float from = outStart - Delay >= SlideSeconds ? 1f : BackOut((outStart - Delay) / SlideSeconds);
                return from * (1f - outK * outK);
            }
            return v;
        }

        /// <summary>Finito: si può spegnere.</summary>
        public static bool Finished(float age, float outStart) => age >= outStart + SlideSeconds;

        /// <summary>Riceve i tocchi solo mentre è (quasi) in posizione e non sta già uscendo.</summary>
        public static bool Tappable(float age, float outStart) => age > Delay && age < outStart;

        /// <summary>Luccichio che attraversa il nastro (0..1 della larghezza), un passaggio ogni 1,2 s; -1 = nessuno.</summary>
        public static float Shine(float age)
        {
            float t = age - Delay - SlideSeconds;
            if (t < 0f) return -1f;
            float k = (t % 1.2f) / 0.6f;
            return k > 1f ? -1f : k;
        }

        static float BackOut(float k)
        {
            const float s = 1.6f;
            float x = k - 1f;
            return 1f + x * x * ((s + 1f) * x + s);
        }
    }
}
