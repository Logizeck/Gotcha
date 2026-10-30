using System;
using System.Collections.Generic;

namespace Gotcha.UI
{
    /// <summary>Le icone animate dei consigli dei primi livelli (una per livello).</summary>
    public enum TipIcon
    {
        None,
        Pinch,       // due dita che si avvicinano su una chiave che rimpicciolisce
        Flat,        // il telefono che si sdraia a schermo in su (verso il sole) e poi dondola
        Clue,        // la lente che passa su piccoli oggetti che brillano
        Touch,       // il dito che tocca e poi striscia, con la scia
        Listen,      // l'orecchio con le onde sonore che pulsano
        Time,        // la sveglia con la lancetta che gira veloce
        Buttons,     // il telefono con i tasti laterali premuti dal dito
        Multitouch,  // due dita che premono insieme, con gli anelli
    }

    /// <summary>Un consiglio: il livello (per levelId), l'icona e la frase.</summary>
    public readonly struct TutorialTip
    {
        public readonly string LevelId;
        public readonly TipIcon Icon;
        public readonly string Caption;

        public TutorialTip(string levelId, TipIcon icon, string caption) { LevelId = levelId; Icon = icon; Caption = caption; }
    }

    /// <summary>
    /// Logica pura (senza Unity) del mini tutorial dei primi 8 livelli: quale consiglio mostrare, quando mostrarlo
    /// (finché il livello non è mai stato superato) e le curve della scheda (comparsa "a molla", permanenza, dissolvenza).
    /// La scheda vera la costruisce e anima GameUI.
    /// </summary>
    public static class TutorialTips
    {
        /// <summary>Quanto resta la scheda prima di svanire da sola (secondi, tempo reale).</summary>
        public const float ShowSeconds = 5f;
        /// <summary>Durata della comparsa "a molla".</summary>
        public const float PopSeconds = 0.32f;
        /// <summary>Durata della dissolvenza finale (anche dopo il tocco sulla scheda).</summary>
        public const float FadeSeconds = 0.45f;
        /// <summary>Scala iniziale della comparsa.</summary>
        public const float PopFrom = 0.6f;

        // per levelId (gli id dei Define del builder), non per posizione nell'elenco
        static readonly TutorialTip[] tips =
        {
            new TutorialTip("key_door", TipIcon.Pinch, "PINCH WITH TWO FINGERS"),
            new TutorialTip("tilt_maze", TipIcon.Flat, "HOLD THE PHONE FLAT, SCREEN UP, AND TILT"),
            new TutorialTip("do_nothing", TipIcon.Clue, "EVERYTHING ON SCREEN COULD BE A CLUE"),
            new TutorialTip("slice_apple", TipIcon.Touch, "TAP, DRAG AND SWIPE THINGS"),
            new TutorialTip("beep_code", TipIcon.Listen, "LISTEN! SOUND MATTERS"),
            new TutorialTip("defuse", TipIcon.Time, "WATCH THE CLOCK: TIME MATTERS"),
            new TutorialTip("shush", TipIcon.Buttons, "YOUR PHONE'S BUTTONS WORK TOO"),
            new TutorialTip("hold_both", TipIcon.Multitouch, "MULTITOUCH: USE MORE THAN ONE FINGER"),
        };

        public static IReadOnlyList<TutorialTip> All => tips;

        /// <summary>Il consiglio del livello, se ne ha uno.</summary>
        public static bool TryGet(string levelId, out TutorialTip tip)
        {
            if (!string.IsNullOrEmpty(levelId))
                foreach (TutorialTip t in tips)
                    if (string.Equals(t.LevelId, levelId, StringComparison.Ordinal)) { tip = t; return true; }
            tip = default;
            return false;
        }

        /// <summary>
        /// Livello superato almeno una volta: superare il livello di posizione i sblocca i + 1 (SaveData.UnlockUpTo),
        /// quindi è superato se il più alto sbloccato è oltre la sua posizione.
        /// </summary>
        public static bool Completed(int levelIndex, int highestUnlocked) => highestUnlocked > levelIndex;

        /// <summary>Il consiglio da mostrare all'avvio del livello: solo per i livelli col consiglio, e solo finché non sono stati superati.</summary>
        public static bool ShouldShow(string levelId, int levelIndex, int highestUnlocked, out TutorialTip tip)
        {
            if (levelIndex < 0 || !TryGet(levelId, out tip)) { tip = default; return false; }
            if (Completed(levelIndex, highestUnlocked)) { tip = default; return false; }
            return true;
        }

        /// <summary>Scala della scheda durante la comparsa: da PopFrom a 1 con un piccolo rimbalzo oltre 1 (ease-out-back).</summary>
        public static float PopScale(float age)
        {
            if (age <= 0f) return PopFrom;
            if (age >= PopSeconds) return 1f;
            float x = age / PopSeconds - 1f;
            const float k = 2.2f;
            float back = 1f + (k + 1f) * x * x * x + k * x * x;   // 0 → 1, supera 1 verso la fine
            return PopFrom + (1f - PopFrom) * back;
        }

        /// <summary>
        /// Trasparenza della scheda: entra nei primi istanti della comparsa, poi resta piena fino a fadeStart
        /// (ShowSeconds, oppure prima se il giocatore l'ha toccata) e sfuma in FadeSeconds.
        /// </summary>
        public static float Alpha(float age, float fadeStart)
        {
            float fadeIn = Clamp01(age / (PopSeconds * 0.5f));
            float fadeOut = 1f - Clamp01((age - fadeStart) / FadeSeconds);
            return Math.Min(fadeIn, fadeOut);
        }

        /// <summary>La scheda è sparita del tutto e si può spegnere.</summary>
        public static bool Finished(float age, float fadeStart) => age >= fadeStart + FadeSeconds;

        /// <summary>Quando parte la dissolvenza se il giocatore tocca la scheda ad age (mai più tardi di quella automatica).</summary>
        public static float FadeStartAfterTap(float age, float currentFadeStart) => Math.Min(currentFadeStart, Math.Max(0f, age));

        static float Clamp01(float v) => v < 0f ? 0f : v > 1f ? 1f : v;
    }
}
