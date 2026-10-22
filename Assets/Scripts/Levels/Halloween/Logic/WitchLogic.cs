namespace Gotcha.Levels.Halloween
{
    public enum WitchIngredient { Toad, Eye, Mushroom, Heart, Apple }

    public enum WitchState
    {
        Idle,       // la strega aspetta un ingrediente
        Reacting,   // reazione comica in corso (temporanea)
        Brewing,    // la parola BEAUTIFUL è nel calderone: pozione rosa e finale
        Done,
    }

    /// <summary>
    /// Logica pura del livello 248 "The witch and the impossible potion": nessun ingrediente funziona (ognuno dà una
    /// reazione temporanea); la soluzione è trascinare la parola BEAUTIFUL della frase nel calderone.
    /// </summary>
    public sealed class WitchLogic
    {
        public WitchState State { get; private set; } = WitchState.Idle;
        public WitchIngredient? Reaction { get; private set; }
        /// <summary>La parola è al suo posto nella frase (false mentre la si trascina).</summary>
        public bool WordInSentence { get; private set; } = true;
        public int IngredientsTried { get; private set; }
        public bool CompletionReported { get; private set; }

        public bool InputLocked => State == WitchState.Brewing || State == WitchState.Done;

        public void Reset()
        {
            State = WitchState.Idle;
            Reaction = null;
            WordInSentence = true;
            IngredientsTried = 0;
            CompletionReported = false;
        }

        /// <summary>Un ingrediente nel calderone: parte la sua reazione (solo se la strega non sta già reagendo).</summary>
        public bool TryAddIngredient(WitchIngredient ingredient)
        {
            if (State != WitchState.Idle) return false;
            State = WitchState.Reacting;
            Reaction = ingredient;
            IngredientsTried++;
            return true;
        }

        /// <summary>La strega torna normale.</summary>
        public bool TryEndReaction()
        {
            if (State != WitchState.Reacting) return false;
            State = WitchState.Idle;
            Reaction = null;
            return true;
        }

        public bool TryPickWord()
        {
            if (InputLocked || !WordInSentence) return false;
            WordInSentence = false;
            return true;
        }

        /// <summary>
        /// Rilascio della parola. True se finisce nel calderone e la strega è libera (parte il finale); altrimenti la
        /// parola torna nella frase (anche se la strega sta reagendo: il finale non parte durante una reazione).
        /// </summary>
        public bool TryDropWord(bool inCauldron)
        {
            if (WordInSentence || InputLocked) return false;
            if (inCauldron && State == WitchState.Idle)
            {
                State = WitchState.Brewing;
                return true;
            }
            WordInSentence = true;
            return false;
        }

        public bool TryCompleteBrew()
        {
            if (State != WitchState.Brewing || CompletionReported) return false;
            State = WitchState.Done;
            CompletionReported = true;
            return true;
        }
    }
}
