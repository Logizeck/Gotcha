using System;

namespace Gotcha.Levels.V64
{
    /// <summary>Fasi del livello 261.</summary>
    public enum DarkStage
    {
        Dark,      // la stanza è al buio: si prova di tutto (le tende possono essere già un po' aperte)
        Sunlit,    // le tende sono state aperte abbastanza: entra il sole (input bloccato, festa finale)
        Done,
    }

    /// <summary>Lato della tenda: la sinistra si apre verso sinistra, la destra verso destra.</summary>
    public enum CurtainSide { Left, Right }

    /// <summary>Cosa succede toccando l'interruttore.</summary>
    public enum SwitchReaction { None, Nothing, BulbGone, StillNothing }

    /// <summary>
    /// Logica pura del livello 261 "The Dark Room" — "Turn on the light." Stanza al buio: l'interruttore non fa niente
    /// (la lampadina non c'è), la lampada da tavolo è staccata e il filo è rosicchiato dal topo (attaccarla fa scintille),
    /// i fiammiferi sono bagnati, la radio parla di un pomeriggio di sole, l'orologio segna le 2 del pomeriggio.
    /// Soluzione: aprire le tende (trascinandone una di lato, o tutte e due con due dita, o tirando il cordino) oltre
    /// una soglia generosa: era giorno. Nessuna mossa sbagliata azzera i progressi; le tende restano dove le si lascia.
    /// Nessuna dipendenza da Unity. Le distanze sono in unità del mondo.
    /// </summary>
    public sealed class DarkRoomLogic
    {
        /// <summary>Quanto può scorrere al massimo ciascuna metà della tenda verso l'esterno.</summary>
        public const float MaxSlide = 1.0f;
        /// <summary>Apertura totale (somma dei due lati) oltre la quale entra il sole: basta un lato solo.</summary>
        public const float OpenThreshold = 0.7f;
        /// <summary>Tirando il cordino in giù, ogni unità di trascinamento apre le tende di tanto (divisa sui due lati).</summary>
        public const float CordGain = 1.2f;
        /// <summary>Dopo quante accensioni a vuoto la lampada oscilla e si vede che manca la lampadina.</summary>
        public const int BulbRevealFlips = 2;

        public DarkStage Stage { get; private set; } = DarkStage.Dark;
        public float LeftOpen { get; private set; }
        public float RightOpen { get; private set; }
        public bool SwitchOn { get; private set; }
        public int SwitchFlips { get; private set; }
        public bool BulbSeen { get; private set; }
        public int LampTaps { get; private set; }
        public int PlugAttempts { get; private set; }
        public bool CableSeen { get; private set; }
        public int MatchAttempts { get; private set; }
        public int RadioTaps { get; private set; }
        public int ClockTaps { get; private set; }
        public int CurtainTaps { get; private set; }
        public int MouseTaps { get; private set; }
        public bool CompletionReported { get; private set; }

        /// <summary>Apertura totale delle tende (somma dei due lati).</summary>
        public float Gap => LeftOpen + RightOpen;
        /// <summary>0 = tende chiuse, 1 = abbastanza aperte per vincere.</summary>
        public float OpenFraction => Math.Min(1f, Gap / OpenThreshold);
        public bool InputLocked => Stage != DarkStage.Dark;
        public bool Solved => Stage != DarkStage.Dark;
        /// <summary>Tutte le mosse sbagliate fatte finora (solo statistica: non cambiano niente).</summary>
        public int WrongMoves => SwitchFlips + LampTaps + PlugAttempts + MatchAttempts + RadioTaps + ClockTaps;

        public void Reset()
        {
            Stage = DarkStage.Dark;
            LeftOpen = RightOpen = 0f;
            SwitchOn = false;
            SwitchFlips = 0;
            BulbSeen = false;
            LampTaps = PlugAttempts = MatchAttempts = RadioTaps = ClockTaps = CurtainTaps = MouseTaps = 0;
            CableSeen = false;
            CompletionReported = false;
        }

        // ---------- Le false piste ----------

        /// <summary>L'interruttore: clic... niente. Alla seconda volta la lampada oscilla e si vede il portalampada vuoto.</summary>
        public SwitchReaction FlipSwitch()
        {
            if (InputLocked) return SwitchReaction.None;
            SwitchFlips++;
            SwitchOn = !SwitchOn;
            if (SwitchFlips < BulbRevealFlips) return SwitchReaction.Nothing;
            if (!BulbSeen) { BulbSeen = true; return SwitchReaction.BulbGone; }
            return SwitchReaction.StillNothing;
        }

        /// <summary>La lampada da tavolo: "Unplugged." Restituisce quante volte la si è toccata.</summary>
        public int TapLamp()
        {
            if (InputLocked) return 0;
            return ++LampTaps;
        }

        /// <summary>La spina portata alla presa: scintille (il filo è rosicchiato), il topo fa il vago.</summary>
        public int TryPlug()
        {
            if (InputLocked) return 0;
            CableSeen = true;
            return ++PlugAttempts;
        }

        /// <summary>I fiammiferi (sulla candela o sfregati): bagnati. Restituisce il numero di tentativi.</summary>
        public int TryMatches()
        {
            if (InputLocked) return 0;
            return ++MatchAttempts;
        }

        /// <summary>La radio: alterna il notiziario ("...un bellissimo pomeriggio di sole...") e il cinguettio.</summary>
        public int TapRadio()
        {
            if (InputLocked) return 0;
            return ++RadioTaps;
        }

        /// <summary>L'orologio: "Tick. Tock. 2 PM."</summary>
        public int TapClock()
        {
            if (InputLocked) return 0;
            return ++ClockTaps;
        }

        /// <summary>Un tocco (senza trascinare) sulla tenda: si muove appena, la luce sul bordo tremola.</summary>
        public int TapCurtain()
        {
            if (InputLocked) return 0;
            return ++CurtainTaps;
        }

        public int TapMouse()
        {
            if (InputLocked) return 0;
            return ++MouseTaps;
        }

        // ---------- La soluzione: le tende ----------

        /// <summary>
        /// Trascina una metà della tenda di dx (unità del mondo; verso l'esterno = apre, verso l'interno = richiude).
        /// Resta dove la si lascia. True nel momento in cui le tende sono abbastanza aperte (una volta sola).
        /// </summary>
        public bool DragCurtain(CurtainSide side, float dx)
        {
            if (InputLocked || float.IsNaN(dx)) return false;
            if (side == CurtainSide.Left) LeftOpen = Clamp(LeftOpen - dx, 0f, MaxSlide);
            else RightOpen = Clamp(RightOpen + dx, 0f, MaxSlide);
            return CheckOpen();
        }

        /// <summary>Tira il cordino (dy &lt; 0 = in giù = apre, in su = niente). True quando le tende si aprono abbastanza.</summary>
        public bool PullCord(float dy)
        {
            if (InputLocked || float.IsNaN(dy) || dy >= 0f) return false;
            float add = -dy * CordGain * 0.5f;
            LeftOpen = Clamp(LeftOpen + add, 0f, MaxSlide);
            RightOpen = Clamp(RightOpen + add, 0f, MaxSlide);
            return CheckOpen();
        }

        bool CheckOpen()
        {
            if (Gap + 1e-5f < OpenThreshold) return false;
            Stage = DarkStage.Sunlit;
            return true;
        }

        /// <summary>Fine della festa: vittoria, una volta sola.</summary>
        public bool TryComplete()
        {
            if (Stage != DarkStage.Sunlit || CompletionReported) return false;
            CompletionReported = true;
            Stage = DarkStage.Done;
            return true;
        }

        static float Clamp(float v, float lo, float hi) => v < lo ? lo : v > hi ? hi : v;
    }
}
