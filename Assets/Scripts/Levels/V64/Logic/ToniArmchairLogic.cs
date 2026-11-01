namespace Gotcha.Levels.V64
{
    /// <summary>Fasi del livello 275.</summary>
    public enum ToniArmchairStage
    {
        Playing,    // Toni dorme su una poltrona: si prova di tutto
        Settling,   // sei seduto sulla TUA poltrona: finale, input bloccato
        Done,       // vinto
        Failed,     // perso (la partita è cominciata senza di te, o l'aspirapolvere una volta di troppo)
    }

    /// <summary>Perché si è perso.</summary>
    public enum ToniArmchairFail
    {
        None,
        Match,      // il conto alla rovescia della TV è arrivato a 0: "You missed the match. Toni didn't."
        Vacuum,     // terzo aspirapolvere: Toni si sveglia furioso e si prende anche il telecomando
    }

    /// <summary>L'aspirapolvere acceso (azione da tre strike).</summary>
    public enum ToniArmchairVacuum
    {
        None,       // input bloccato
        Growl,      // 1°: "Toni growls."
        Grumpy,     // 2°: "Toni is getting VERY grumpy."
        Furious,    // 3°: si sveglia furioso, si prende il telecomando -> sconfitta
    }

    /// <summary>Dove si trova qualcuno (tu, cioè il cuscino "YOU", oppure Toni).</summary>
    public enum ToniArmchairSpot
    {
        Floor,      // per terra (solo tu: sul tappeto)
        Left,       // la tua poltrona preferita (col telecomando)
        Right,      // la poltrona identica, vuota
    }

    /// <summary>Le cose della stanza che si possono appoggiare sulla poltrona di destra.</summary>
    public enum ToniArmchairItem
    {
        Treats,     // la scatola dei biscotti
        Squeaky,    // la paperella che squittisce
        Vacuum,     // l'aspirapolvere
        Blanket,    // la coperta
        Mug,        // la tazza
        Remote,     // il telecomando
    }

    /// <summary>Sedersi su una poltrona (trascinare "YOU" o toccare la poltrona).</summary>
    public enum ToniArmchairSit
    {
        None,           // input bloccato
        Occupied,       // c'è Toni: rimbalzi via (lui brontola)
        ToniTakesIt,    // ti sei seduto sull'ALTRA poltrona: Toni si sveglia e viene a prendersela
        Wait,           // Toni sta ancora traslocando: non succede niente
        AlreadyThere,   // ci sei già seduto
        Won,            // la tua poltrona è libera e ti ci siedi: vittoria
    }

    /// <summary>Un oggetto appoggiato sulla poltrona di destra.</summary>
    public enum ToniArmchairPlace
    {
        None,           // input bloccato
        Suspicious,     // l'indizio: Toni apre un occhio e lo fissa ("...is that seat better than mine?")
        Ignored,        // Toni non è sull'altra poltrona: non gli importa
        Taken,          // sulla poltrona di destra ora c'è Toni (o ci sei tu): l'oggetto torna a posto
    }

    /// <summary>Le regole della sconfitta (visibili sullo schermo, mai una sorpresa).</summary>
    public static class ToniArmchairRules
    {
        /// <summary>Secondi del conto alla rovescia sulla TV: "THE MATCH STARTS IN 60".</summary>
        public const float MatchSeconds = 60f;
        /// <summary>Negli ultimi secondi il conto alla rovescia pulsa rosso.</summary>
        public const float UrgentSeconds = 10f;
        /// <summary>Al terzo aspirapolvere Toni si arrabbia davvero.</summary>
        public const int VacuumStrikes = 3;
        public const string MatchTaunt = "You missed the match. Toni didn't.";
        public const string VacuumTaunt = "Never vacuum a sleeping pug.";
    }

    /// <summary>
    /// Logica pura del livello 275 "Toni's Armchair" (un livello di Toni) — "Get your armchair back!" Nel soggiorno
    /// due poltrone identiche, una accanto all'altra: a sinistra la tua preferita (col telecomando e la tazza), a destra
    /// una uguale, vuota. Toni dorme raggomitolato sulla TUA e russa.
    /// False piste (nessuna azzera i progressi, niente sconfitta): spingerlo/trascinarlo via (diventa piatto e pesante,
    /// "400 kg", e torna giù), il biscotto (lo mangia senza aprire gli occhi), la paperella che squittisce (si muove un
    /// orecchio), l'aspirapolvere (russa più forte), il tocco (brontola nel sonno), sederti sulla tua poltrona (è
    /// occupata). Indizio: ogni volta che si appoggia qualcosa sulla poltrona di DESTRA, Toni apre un occhio e lo fissa
    /// sospettoso ("...is that seat better than mine?"): i carlini vogliono sempre il posto di qualcun altro.
    /// Soluzione: sederti sull'ALTRA poltrona (il cuscino "YOU" sulla poltrona di destra) -> Toni si sveglia, salta giù e
    /// si prende il TUO nuovo posto (buttandoti giù) -> la tua poltrona è libera -> "YOU" sulla poltrona di sinistra ->
    /// vittoria.
    /// Sconfitta (regola del proprietario, sempre annunciata): sulla TV il conto alla rovescia "THE MATCH STARTS IN 60"
    /// (se a 0 non sei sulla tua poltrona: "You missed the match. Toni didn't."; gli ultimi 10 s pulsa rosso) e
    /// l'aspirapolvere è un'azione da tre strike (1° "Toni growls.", 2° "Toni is getting VERY grumpy.", 3° Toni si
    /// sveglia furioso, si prende anche il telecomando: "Never vacuum a sleeping pug."). La sconfitta si segnala una
    /// volta sola e dopo non si può più vincere (e viceversa). Nessuna dipendenza da Unity.
    /// </summary>
    public sealed class ToniArmchairLogic
    {
        public ToniArmchairStage Stage { get; private set; } = ToniArmchairStage.Playing;
        /// <summary>La poltrona su cui dorme Toni (all'inizio la tua).</summary>
        public ToniArmchairSpot ToniAt { get; private set; } = ToniArmchairSpot.Left;
        /// <summary>Dove sei tu (il cuscino "YOU").</summary>
        public ToniArmchairSpot YouAt { get; private set; } = ToniArmchairSpot.Floor;
        /// <summary>True mentre Toni salta dalla tua poltrona all'altra (dopo che ti ci sei seduto).</summary>
        public bool ToniMoving { get; private set; }

        public int Pushes { get; private set; }
        public int Treats { get; private set; }
        public int Squeaks { get; private set; }
        public int VacuumRuns { get; private set; }
        public int ToniTaps { get; private set; }
        public int BlanketCovers { get; private set; }
        public int OccupiedTries { get; private set; }
        /// <summary>Quante volte Toni ha fissato un oggetto sulla poltrona di destra (l'indizio).</summary>
        public int Stares { get; private set; }
        public bool CompletionReported { get; private set; }
        public bool FailReported { get; private set; }
        public ToniArmchairFail FailReason { get; private set; }
        /// <summary>Secondi che mancano all'inizio della partita (conto alla rovescia sulla TV).</summary>
        public float TimeLeft { get; private set; } = ToniArmchairRules.MatchSeconds;
        public int RemoteTaps { get; private set; }

        /// <summary>Il conto alla rovescia corre solo mentre si gioca (seduto sulla tua poltrona si ferma).</summary>
        public bool ClockRunning => Stage == ToniArmchairStage.Playing;
        public bool Urgent => ClockRunning && TimeLeft <= ToniArmchairRules.UrgentSeconds;
        /// <summary>Secondi interi mostrati sulla TV (60, 59, ... 1, 0).</summary>
        public int SecondsShown => TimeLeft <= 0f ? 0 : (int)System.Math.Ceiling(TimeLeft - 1e-4);
        public bool IsFailed => Stage == ToniArmchairStage.Failed;

        public bool InputLocked => Stage != ToniArmchairStage.Playing || ToniMoving;
        /// <summary>La tua poltrona (quella di sinistra) è libera.</summary>
        public bool LeftFree => ToniAt != ToniArmchairSpot.Left;
        /// <summary>Toni ha cambiato poltrona (resta così: progresso conservato).</summary>
        public bool ToniSwitched => ToniAt == ToniArmchairSpot.Right && !ToniMoving;
        /// <summary>Tutte le mosse sbagliate fatte finora (solo statistica: non cambiano niente).</summary>
        public int WrongMoves => Pushes + Treats + Squeaks + VacuumRuns + ToniTaps + BlanketCovers + OccupiedTries;

        public void Reset()
        {
            Stage = ToniArmchairStage.Playing;
            ToniAt = ToniArmchairSpot.Left;
            YouAt = ToniArmchairSpot.Floor;
            ToniMoving = false;
            Pushes = Treats = Squeaks = VacuumRuns = ToniTaps = BlanketCovers = OccupiedTries = Stares = RemoteTaps = 0;
            CompletionReported = FailReported = false;
            FailReason = ToniArmchairFail.None;
            TimeLeft = ToniArmchairRules.MatchSeconds;
        }

        // ---------- Le false piste: Toni non si sposta mai ----------

        /// <summary>Spingere/trascinare Toni: diventa piatto e pesante e torna giù. Restituisce quante spinte (0 = bloccato).</summary>
        public int Push()
        {
            if (InputLocked) return 0;
            return ++Pushes;
        }

        /// <summary>Un biscotto: se lo mangia senza aprire gli occhi.</summary>
        public int GiveTreat()
        {
            if (InputLocked) return 0;
            return ++Treats;
        }

        /// <summary>La paperella che squittisce: si muove un orecchio, nient'altro.</summary>
        public int Squeak()
        {
            if (InputLocked) return 0;
            return ++Squeaks;
        }

        /// <summary>L'aspirapolvere acceso: russa più forte e brontola; al terzo si sveglia furioso (sconfitta).</summary>
        public ToniArmchairVacuum Vacuum()
        {
            if (InputLocked) return ToniArmchairVacuum.None;
            VacuumRuns++;
            if (VacuumRuns == 1) return ToniArmchairVacuum.Growl;
            if (VacuumRuns < ToniArmchairRules.VacuumStrikes) return ToniArmchairVacuum.Grumpy;
            Stage = ToniArmchairStage.Failed;
            FailReason = ToniArmchairFail.Vacuum;
            return ToniArmchairVacuum.Furious;
        }

        /// <summary>Toccare Toni: brontola nel sonno.</summary>
        public int TapToni()
        {
            if (InputLocked) return 0;
            return ++ToniTaps;
        }

        /// <summary>La coperta addosso a Toni: ancora più comodo, ancora più fermo.</summary>
        public int CoverToni()
        {
            if (InputLocked) return 0;
            return ++BlanketCovers;
        }

        /// <summary>Il telecomando: solo il pre-partita sulla TV (a Toni non importa). False se bloccato.</summary>
        public bool TapRemote()
        {
            if (InputLocked) return false;
            RemoteTaps++;
            return true;
        }

        // ---------- Il conto alla rovescia ----------

        /// <summary>
        /// Fa passare il tempo (secondi, già limitati da chi chiama). True se proprio adesso la partita è cominciata
        /// senza di te (sconfitta). Corre anche mentre Toni trasloca; si ferma quando sei sulla tua poltrona.
        /// </summary>
        public bool Tick(float dt)
        {
            if (!ClockRunning || dt <= 0f) return false;
            TimeLeft -= dt;
            if (TimeLeft > 0f) return false;
            TimeLeft = 0f;
            ToniMoving = false;
            Stage = ToniArmchairStage.Failed;
            FailReason = ToniArmchairFail.Match;
            return true;
        }

        /// <summary>La sconfitta va segnalata (una volta sola).</summary>
        public bool TryReportFail()
        {
            if (Stage != ToniArmchairStage.Failed || FailReported) return false;
            FailReported = true;
            return true;
        }

        // ---------- L'indizio ----------

        /// <summary>Un oggetto appoggiato sulla poltrona di destra.</summary>
        public ToniArmchairPlace PlaceOnRight(ToniArmchairItem item)
        {
            if (InputLocked) return ToniArmchairPlace.None;
            if (ToniAt == ToniArmchairSpot.Right || YouAt == ToniArmchairSpot.Right) return ToniArmchairPlace.Taken;
            Stares++;
            return ToniArmchairPlace.Suspicious;
        }

        // ---------- Sedersi ----------

        /// <summary>Sedersi su una poltrona (Left / Right). Floor non è una poltrona: non fa niente.</summary>
        public ToniArmchairSit Sit(ToniArmchairSpot seat)
        {
            if (Stage != ToniArmchairStage.Playing) return ToniArmchairSit.None;
            if (seat == ToniArmchairSpot.Floor) return ToniArmchairSit.None;
            // Toni sta ancora saltando sull'altra poltrona: tornare indietro adesso non fa niente (nessun blocco)
            if (ToniMoving) return ToniArmchairSit.Wait;
            if (YouAt == seat) return ToniArmchairSit.AlreadyThere;
            if (ToniAt == seat)
            {
                OccupiedTries++;
                return ToniArmchairSit.Occupied;
            }
            YouAt = seat;
            if (seat == ToniArmchairSpot.Left)
            {
                // la tua poltrona è libera (Toni è sull'altra): finale
                Stage = ToniArmchairStage.Settling;
                return ToniArmchairSit.Won;
            }
            // ti sei seduto sull'ALTRA poltrona: a Toni piace di più la tua di adesso
            ToniMoving = true;
            return ToniArmchairSit.ToniTakesIt;
        }

        /// <summary>Toni è arrivato sulla poltrona di destra e ti ha buttato giù (sul tappeto). False se non stava traslocando.</summary>
        public bool ToniArrived()
        {
            if (!ToniMoving || Stage != ToniArmchairStage.Playing) return false;
            ToniMoving = false;
            ToniAt = ToniArmchairSpot.Right;
            YouAt = ToniArmchairSpot.Floor;
            return true;
        }

        // ---------- Il finale ----------

        /// <summary>Fine del finale: vittoria una volta sola.</summary>
        public bool TryComplete()
        {
            if (Stage != ToniArmchairStage.Settling || CompletionReported) return false;
            CompletionReported = true;
            Stage = ToniArmchairStage.Done;
            return true;
        }
    }
}
