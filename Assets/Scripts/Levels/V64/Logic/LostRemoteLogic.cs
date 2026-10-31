namespace Gotcha.Levels.V64
{
    /// <summary>Fasi del livello 270.</summary>
    public enum RemoteStage
    {
        Playing,    // si prova di tutto
        Switching,  // il canale bello sta arrivando (neve, poi il programma nuovo): input bloccato
        Done,
    }

    /// <summary>Cosa c'è sotto un cuscino del divano (sempre la stessa cosa per quel cuscino).</summary>
    public enum CushionFind
    {
        None,       // input bloccato o cuscino che non esiste
        Coins,      // cuscino di sinistra: due monetine
        Sock,       // cuscino di mezzo: un calzino
        Mouse,      // cuscino di destra: briciole e un topolino che dorme
    }

    /// <summary>Il gatto toccato.</summary>
    public enum CatResult
    {
        None,
        RollsOver,  // la prima volta si gira dall'altra parte: salta fuori il telecomando
        Sleeps,     // già girato: continua a dormire
    }

    /// <summary>I tasti del telecomando premuti (non funziona mai).</summary>
    public enum RemoteResult
    {
        None,       // input bloccato o telecomando non ancora trovato
        Empty,      // "Battery compartment: empty."
        Dead,       // con le pile dell'orologio: "These are dead too."
    }

    /// <summary>L'orologio a muro toccato (o il telecomando portato sull'orologio).</summary>
    public enum ClockResult
    {
        None,
        Wrong,          // va avanti, sbagliato di tre ore (telecomando non ancora trovato: le pile non servono)
        BatteriesTaken, // le pile passano nel telecomando, l'orologio si ferma
        Stopped,        // già fermo: niente da prendere
    }

    /// <summary>Urlare al televisore (microfono) o toccarne lo schermo: il programma va avanti lo stesso.</summary>
    public enum ShoutResult
    {
        None,
        NextHour,   // in diretta: "Hour 38."
        Rerun,      // su una replica: non può sentirti, è registrata
        Off,        // staccato: schermo nero
    }

    /// <summary>Un tastino del canale sul fianco del televisore.</summary>
    public enum ChannelResult
    {
        None,       // input bloccato
        NoPower,    // staccato: i tasti non fanno niente
        Down,       // CH-: una replica ancora peggiore ("HOUR 36")
        Bottom,     // CH- sul canale più basso: niente da fare, è vernice fino in fondo
        Up,         // CH+ da una replica: si risale, ma è sempre vernice
        Good,       // CH+ dalla diretta: il canale bello (la soluzione)
    }

    /// <summary>
    /// Logica pura del livello 270 "The Lost Remote" — "Change the channel!" Il televisore manda un programma orribile
    /// ("PAINT DRYING LIVE — HOUR 37") e il telecomando non si trova. False piste (nessuna azzera i progressi, niente
    /// sconfitta): alzare i cuscini del divano (monetine, un calzino, briciole e un topolino che dorme), toccare il gatto
    /// (si gira e salta fuori il telecomando, che però non ha le pile; le pile dell'orologio sono scariche anche loro),
    /// urlare al televisore o toccarne lo schermo (passa solo un'altra ora), staccare la spina (schermo nero: non è
    /// cambiare canale, si riattacca).
    /// Soluzione: il tastino "CH +" sul fianco del televisore (l'indizio è la pagina del libretto con la freccia sul
    /// fianco). Decisione sul "CH -": i canali sotto la diretta sono repliche della stessa vernice (HOUR 36, HOUR 35),
    /// "CH +" risale di un canale alla volta; si vince solo arrivando col "CH +" al canale sopra la diretta (il canale
    /// bello). Il telecomando non cambia mai canale. Nessuna dipendenza da Unity.
    /// </summary>
    public sealed class LostRemoteLogic
    {
        /// <summary>Canali: 1-2 repliche, 3 la diretta della vernice (all'inizio), 4 il canale bello.</summary>
        public const int LowestChannel = 1, LiveChannel = 3, GoodChannel = 4;
        public const int StartHour = 37;
        public const int Cushions = 3;

        public RemoteStage Stage { get; private set; } = RemoteStage.Playing;
        public int Channel { get; private set; } = LiveChannel;
        /// <summary>L'ora della diretta (sale a ogni urlo/tocco sullo schermo).</summary>
        public int LiveHour { get; private set; } = StartHour;
        public bool Plugged { get; private set; } = true;
        public bool CatRolled { get; private set; }
        public bool RemoteFound => CatRolled;
        /// <summary>Le pile dell'orologio sono nel telecomando (e l'orologio è fermo).</summary>
        public bool ClockBatteriesInRemote { get; private set; }
        public bool ClockRunning => !ClockBatteriesInRemote;
        public bool ManualRead { get; private set; }

        readonly int[] lifts = new int[Cushions];
        public int Shouts { get; private set; }
        public int ScreenTaps { get; private set; }
        public int RemotePresses { get; private set; }
        public int ClockTaps { get; private set; }
        public int CatTaps { get; private set; }
        public int Unplugs { get; private set; }
        public int Replugs { get; private set; }
        public int ChannelDowns { get; private set; }
        public int ManualReads { get; private set; }
        public int Smacks { get; private set; }
        public bool CompletionReported { get; private set; }

        public bool InputLocked => Stage != RemoteStage.Playing;
        /// <summary>Sullo schermo c'è ancora (o di nuovo) la vernice che asciuga.</summary>
        public bool PaintDrying => Channel <= LiveChannel;
        public bool OnRerun => Channel < LiveChannel;
        /// <summary>L'ora mostrata dal canale attuale (le repliche sono indietro di un'ora per canale).</summary>
        public int ShownHour => LiveHour - (LiveChannel - System.Math.Min(Channel, LiveChannel));
        public int LiftsOf(int cushion) => cushion >= 0 && cushion < Cushions ? lifts[cushion] : 0;
        public int CushionLifts => lifts[0] + lifts[1] + lifts[2];
        public bool AllCushionsChecked => lifts[0] > 0 && lifts[1] > 0 && lifts[2] > 0;
        /// <summary>Tutte le mosse sbagliate fatte finora (solo statistica: non cambiano niente).</summary>
        public int WrongMoves => CushionLifts + Shouts + ScreenTaps + RemotePresses + ClockTaps + CatTaps + Unplugs + ChannelDowns + Smacks;

        /// <summary>Il titolo del programma sullo schermo ("" se è spento).</summary>
        public string ScreenTitle
        {
            get
            {
                if (!Plugged) return "";
                if (Channel >= GoodChannel) return "CAT DISCO — 24/7";
                if (Channel == LiveChannel) return "PAINT DRYING LIVE — HOUR " + ShownHour;
                return "PAINT DRYING — HOUR " + ShownHour + " (RERUN)";
            }
        }

        public void Reset()
        {
            Stage = RemoteStage.Playing;
            Channel = LiveChannel;
            LiveHour = StartHour;
            Plugged = true;
            CatRolled = ClockBatteriesInRemote = ManualRead = false;
            for (int i = 0; i < Cushions; i++) lifts[i] = 0;
            Shouts = ScreenTaps = RemotePresses = ClockTaps = CatTaps = Unplugs = Replugs = ChannelDowns = ManualReads = Smacks = 0;
            CompletionReported = false;
        }

        // ---------- Le false piste ----------

        /// <summary>Alzare un cuscino del divano (0 sinistra, 1 mezzo, 2 destra): sotto c'è sempre la stessa cosa.</summary>
        public CushionFind LiftCushion(int cushion)
        {
            if (InputLocked || cushion < 0 || cushion >= Cushions) return CushionFind.None;
            lifts[cushion]++;
            return cushion == 0 ? CushionFind.Coins : cushion == 1 ? CushionFind.Sock : CushionFind.Mouse;
        }

        /// <summary>Toccare il gatto: la prima volta si gira e scopre il telecomando.</summary>
        public CatResult TapCat()
        {
            if (InputLocked) return CatResult.None;
            CatTaps++;
            if (CatRolled) return CatResult.Sleeps;
            CatRolled = true;
            return CatResult.RollsOver;
        }

        /// <summary>Premere i tasti del telecomando: non cambia mai canale.</summary>
        public RemoteResult PressRemote()
        {
            if (InputLocked || !RemoteFound) return RemoteResult.None;
            RemotePresses++;
            return ClockBatteriesInRemote ? RemoteResult.Dead : RemoteResult.Empty;
        }

        /// <summary>L'orologio a muro: col telecomando trovato e vuoto gli si rubano le pile (si ferma).</summary>
        public ClockResult TapClock()
        {
            if (InputLocked) return ClockResult.None;
            ClockTaps++;
            if (ClockBatteriesInRemote) return ClockResult.Stopped;
            if (!RemoteFound) return ClockResult.Wrong;
            ClockBatteriesInRemote = true;
            return ClockResult.BatteriesTaken;
        }

        /// <summary>Urlare al televisore (microfono).</summary>
        public ShoutResult Shout()
        {
            if (InputLocked) return ShoutResult.None;
            Shouts++;
            return Yell();
        }

        /// <summary>Toccare lo schermo (è anche l'alternativa all'urlo quando il microfono non c'è).</summary>
        public ShoutResult TapScreen()
        {
            if (InputLocked) return ShoutResult.None;
            ScreenTaps++;
            return Yell();
        }

        ShoutResult Yell()
        {
            if (!Plugged) return ShoutResult.Off;
            if (OnRerun) return ShoutResult.Rerun;
            LiveHour++;
            return ShoutResult.NextHour;
        }

        /// <summary>Uno scossone al telefono (= una botta al televisore): un attimo di neve, poi lo stesso programma.</summary>
        public bool Smack()
        {
            if (InputLocked) return false;
            Smacks++;
            return Plugged;
        }

        /// <summary>Staccare la spina: schermo nero (non è cambiare canale). True se era attaccata.</summary>
        public bool Unplug()
        {
            if (InputLocked || !Plugged) return false;
            Plugged = false;
            Unplugs++;
            return true;
        }

        /// <summary>Riattaccare la spina: torna lo stesso canale di prima. True se era staccata.</summary>
        public bool Replug()
        {
            if (InputLocked || Plugged) return false;
            Plugged = true;
            Replugs++;
            return true;
        }

        /// <summary>Aprire il libretto delle istruzioni (la pagina con la freccia sul fianco del televisore).</summary>
        public void ReadManual()
        {
            if (InputLocked) return;
            ManualRead = true;
            ManualReads++;
        }

        // ---------- I tastini sul fianco ----------

        /// <summary>CH + (up = true) o CH - (up = false) sul fianco del televisore.</summary>
        public ChannelResult PressChannel(bool up)
        {
            if (InputLocked) return ChannelResult.None;
            if (!Plugged) return ChannelResult.NoPower;
            if (!up)
            {
                ChannelDowns++;
                if (Channel <= LowestChannel) return ChannelResult.Bottom;
                Channel--;
                return ChannelResult.Down;
            }
            Channel++;
            if (Channel < GoodChannel) return ChannelResult.Up;
            Channel = GoodChannel;
            Stage = RemoteStage.Switching;
            return ChannelResult.Good;
        }

        // ---------- Il finale ----------

        /// <summary>Fine del cambio canale: vittoria, una volta sola.</summary>
        public bool TryComplete()
        {
            if (Stage != RemoteStage.Switching || CompletionReported) return false;
            CompletionReported = true;
            Stage = RemoteStage.Done;
            return true;
        }
    }
}
