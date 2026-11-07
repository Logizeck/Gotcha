using System;

namespace Gotcha.Levels.V64
{
    /// <summary>Fasi del livello "The Broken Applause".</summary>
    public enum BrokenApplauseStage
    {
        Playing,   // le due mani sono separate, il pubblico si annoia: si prova di tutto
        Ovation,   // le mani si sono incontrate: applauso e standing ovation in corso (input bloccato)
        Done,      // vittoria già data
    }

    /// <summary>Risultato di uno spostamento di un dito.</summary>
    public enum BrokenApplauseMove
    {
        None,       // il dito non tiene nessuna mano (o input bloccato)
        Resisting,  // una mano sola, trascinata: fa resistenza (avanza appena, poi torna indietro)
        Following,  // tutte e due tenute: seguono le dita
        Clap,       // tutte e due tenute e si toccano: un battito (VITTORIA, una volta sola)
    }

    /// <summary>Risultato di un dito che si alza.</summary>
    public enum BrokenApplauseDrop
    {
        None,        // il dito non teneva nessuna mano (o input bloccato)
        Pat,         // tocco senza trascinare su una mano sola: "pat."
        SprangBack,  // trascinata da sola e lasciata: torna a posto con una molla
        Separated,   // erano tenute tutte e due e un dito si è alzato: quella mano torna a posto
    }

    /// <summary>
    /// Logica pura del livello 138 della roadmap "The Broken Applause" (id broken_applause) — "GET A ROUND OF APPLAUSE."
    /// Su un palco vuoto due grandi mani da cartone animato stanno ai lati dello schermo; sotto, il pubblico annoiato
    /// sbadiglia. Coordinate LOCALI rispetto al punto in mezzo alle due mani (0, 0): la mano sinistra parte da
    /// (-<see cref="HomeX"/>, 0), la destra da (+<see cref="HomeX"/>, 0).
    /// Inganno: una mano toccata da sola fa solo "pat."; trascinata da sola (un dito) fa resistenza: verso il centro avanza
    /// al massimo di <see cref="SoloReach"/> (frenata, mai raggiunto) e al rilascio torna a casa con una molla. L'altra
    /// resta ferma: da sola una mano non arriva MAI a toccare l'altra. Toccare il pubblico lo fa solo sbadigliare.
    /// Soluzione esatta: due dita diverse, una per mano (in qualunque ordine): tenute tutte e due, seguono le dita
    /// liberamente (sempre dentro i limiti); quando, mentre sono tenute, la distanza orizzontale tra i palmi scende a
    /// <see cref="ClapGap"/> o meno e sono alla stessa altezza (scarto verticale &lt;= <see cref="ClapYTol"/>), le mani si
    /// incontrano: un battito, poi l'ovazione (una volta sola). Se un dito si alza, quella mano torna a posto e l'altra
    /// torna a fare resistenza.
    /// Regole delle dita: un dito tiene al massimo una mano (lo stesso dito non può prenderle tutte e due), una mano è
    /// tenuta al massimo da un dito; quindi al massimo due dita contano. Cancel() (pausa, app in background, livello
    /// spento) lascia andare tutto senza contare niente. Non si può perdere. Nessuna dipendenza da Unity.
    /// </summary>
    public sealed class BrokenApplauseLogic
    {
        public const int Left = 0, Right = 1;
        /// <summary>Id "nessun dito".</summary>
        public const int NoFinger = int.MinValue;

        /// <summary>Distanza dal centro della casa di ciascuna mano.</summary>
        public const float HomeX = 1.5f;
        /// <summary>Limiti dei centri delle mani (locali): restano sempre dentro lo schermo e sotto la riga dell'obiettivo.</summary>
        public const float MaxX = 1.7f, MinY = -1.0f, MaxY = 0.8f;
        /// <summary>Distanza orizzontale tra i centri delle mani alla quale i palmi si toccano (battito).</summary>
        public const float ClapGap = 0.95f;
        /// <summary>Scarto verticale massimo perché le mani si incontrino davvero.</summary>
        public const float ClapYTol = 0.55f;
        /// <summary>Tutte e due tenute ed entro questa distanza: le mani fremono e il pubblico alza la testa (solo grafica).</summary>
        public const float NearGap = 1.9f;
        /// <summary>Di quanto al massimo una mano trascinata da sola avanza verso il centro (frenata, mai raggiunto).</summary>
        public const float SoloReach = 0.35f;
        /// <summary>Di quanto al massimo una mano trascinata da sola si sposta in su o in giù.</summary>
        public const float SoloLift = 0.25f;
        /// <summary>Spostamento del dito oltre il quale non è più un tocco.</summary>
        public const float TapSlop = 0.15f;
        /// <summary>Quante persone ci sono tra il pubblico (righe di teste).</summary>
        public const int AudienceSize = 9;

        sealed class Hand
        {
            public float homeX, x, y, ox, oy, fx, fy;
            public int finger = NoFinger;
            public bool moved;
            public float sx, sy;   // dove il dito ha preso la mano (per distinguere il tocco)
        }

        readonly Hand[] h = { new Hand { homeX = -HomeX }, new Hand { homeX = HomeX } };
        bool completed;

        public BrokenApplauseStage Stage { get; private set; } = BrokenApplauseStage.Playing;
        public bool InputLocked => Stage != BrokenApplauseStage.Playing;

        public int Pats { get; private set; }
        public int SoloDrags { get; private set; }
        public int Separations { get; private set; }
        public int RefusedGrabs { get; private set; }
        public int AudienceTaps { get; private set; }
        /// <summary>Quante volte è partita l'ovazione (0 o 1).</summary>
        public int OvationStarts { get; private set; }
        public int WrongMoves => SoloDrags + Separations;

        public BrokenApplauseLogic() { Reset(); }

        public void Reset()
        {
            Stage = BrokenApplauseStage.Playing;
            foreach (Hand a in h) { a.finger = NoFinger; a.moved = false; a.x = a.homeX; a.y = 0f; a.ox = a.oy = a.fx = a.fy = a.sx = a.sy = 0f; }
            Pats = SoloDrags = Separations = RefusedGrabs = AudienceTaps = OvationStarts = 0;
            completed = false;
        }

        // ---------- Stato letto dal livello ----------

        static bool Valid(int who) => who == Left || who == Right;
        public float X(int who) => Valid(who) ? h[who].x : 0f;
        public float Y(int who) => Valid(who) ? h[who].y : 0f;
        public float HomeOf(int who) => Valid(who) ? h[who].homeX : 0f;
        public bool IsHeld(int who) => Valid(who) && h[who].finger != NoFinger;
        public int HolderOf(int who) => Valid(who) ? h[who].finger : NoFinger;
        public int HeldCount => (IsHeld(Left) ? 1 : 0) + (IsHeld(Right) ? 1 : 0);
        /// <summary>Tutte e due tenute (da due dita diverse).</summary>
        public bool Together => HeldCount == 2;
        /// <summary>Distanza orizzontale tra i centri delle due mani (2 * HomeX a casa).</summary>
        public float Gap => h[Right].x - h[Left].x;
        /// <summary>Scarto verticale tra le due mani.</summary>
        public float DeltaY => Math.Abs(h[Right].y - h[Left].y);
        /// <summary>Tutte e due tenute e già vicine: fremono (stato intermedio).</summary>
        public bool Near => Together && Gap <= NearGap;

        /// <summary>Quanto sono lontane (0 = si toccano, 1 = a casa).</summary>
        public float Apart
        {
            get
            {
                float k = (Gap - ClapGap) / (2f * HomeX - ClapGap);
                return k < 0f ? 0f : k > 1.2f ? 1.2f : k;
            }
        }

        /// <summary>Quale mano tiene questo dito (-1 nessuna).</summary>
        public int HeldBy(int finger)
        {
            if (finger == NoFinger) return -1;
            for (int i = 0; i < 2; i++) if (h[i].finger == finger) return i;
            return -1;
        }

        // ---------- Dita ----------

        /// <summary>
        /// Il dito <paramref name="finger"/> (in (fx, fy), locali) prende la mano <paramref name="who"/>. Falso se l'input è
        /// bloccato, se la mano è già tenuta, o se lo stesso dito tiene già l'altra (un dito non le prende mai tutte e due).
        /// </summary>
        public bool Grab(int finger, int who, float fx, float fy)
        {
            if (InputLocked || !Valid(who) || finger == NoFinger || Bad(fx) || Bad(fy)) return false;
            Hand a = h[who];
            if (a.finger != NoFinger) return false;
            if (HeldBy(finger) >= 0) { RefusedGrabs++; return false; }
            a.finger = finger;
            a.moved = false;
            a.fx = a.sx = fx;
            a.fy = a.sy = fy;
            Rebase();
            Solve();
            CheckClap();
            return true;
        }

        /// <summary>Il dito si sposta in (fx, fy) (locali).</summary>
        public BrokenApplauseMove Move(int finger, float fx, float fy)
        {
            if (InputLocked) return BrokenApplauseMove.None;
            int who = HeldBy(finger);
            if (who < 0 || Bad(fx) || Bad(fy)) return BrokenApplauseMove.None;
            Hand a = h[who];
            a.fx = fx;
            a.fy = fy;
            float dx = fx - a.sx, dy = fy - a.sy;
            if (dx * dx + dy * dy > TapSlop * TapSlop) a.moved = true;
            Solve();
            if (CheckClap()) return BrokenApplauseMove.Clap;
            return Together ? BrokenApplauseMove.Following : BrokenApplauseMove.Resisting;
        }

        /// <summary>Il dito si alza: la mano che teneva torna a posto.</summary>
        public BrokenApplauseDrop Release(int finger)
        {
            int who = HeldBy(finger);
            if (who < 0) return BrokenApplauseDrop.None;
            bool together = Together;
            Hand a = h[who];
            bool moved = a.moved;
            a.finger = NoFinger;
            a.moved = false;
            if (InputLocked) return BrokenApplauseDrop.None;
            Rebase();
            Solve();
            if (together) { Separations++; return BrokenApplauseDrop.Separated; }
            if (!moved) { Pats++; return BrokenApplauseDrop.Pat; }
            SoloDrags++;
            return BrokenApplauseDrop.SprangBack;
        }

        /// <summary>Tocco sul pubblico: sbadiglia. Restituisce il numero del tocco (da 0) o -1 se l'input è bloccato.</summary>
        public int TapAudience()
        {
            if (InputLocked) return -1;
            return AudienceTaps++;
        }

        /// <summary>Dita perse (pausa, app in background, livello spento): si lascia tutto, a casa, senza contare niente.</summary>
        public bool Cancel()
        {
            bool any = HeldCount > 0;
            foreach (Hand a in h) { a.finger = NoFinger; a.moved = false; }
            if (!InputLocked) Solve();
            return any;
        }

        /// <summary>Fine della festa: true una volta sola (allora il livello chiama Win).</summary>
        public bool TryComplete()
        {
            if (Stage != BrokenApplauseStage.Ovation || completed) return false;
            completed = true;
            Stage = BrokenApplauseStage.Done;
            return true;
        }

        // ---------- Calcolo delle posizioni ----------

        /// <summary>Lo scarto dito-mano si ricalcola a ogni cambio del numero di dita: nessun salto.</summary>
        void Rebase()
        {
            foreach (Hand a in h)
            {
                if (a.finger == NoFinger) continue;
                a.ox = a.x - a.fx;
                a.oy = a.y - a.fy;
            }
        }

        void Solve()
        {
            int n = HeldCount;
            if (n == 2)
            {
                foreach (Hand a in h) Place(a, a.fx + a.ox, a.fy + a.oy);
                return;
            }
            if (n == 0)
            {
                foreach (Hand a in h) { a.x = a.homeX; a.y = 0f; }
                return;
            }
            Hand held = IsHeld(Left) ? h[Left] : h[Right];
            Hand other = held == h[Left] ? h[Right] : h[Left];
            float inDir = held.homeX < 0f ? 1f : -1f;   // verso il centro
            float wantX = held.fx + held.ox, wantY = held.fy + held.oy;
            float inward = (wantX - held.homeX) * inDir;
            float got = inward > 0f ? Rubber(inward, SoloReach) : inward;   // verso il centro frenata, all'indietro libera
            float lift = wantY >= 0f ? Rubber(wantY, SoloLift) : -Rubber(-wantY, SoloLift);
            Place(held, held.homeX + got * inDir, lift);
            other.x = other.homeX;   // l'altra resta a casa: non va incontro da sola
            other.y = 0f;
        }

        static void Place(Hand a, float x, float y)
        {
            a.x = Clamp(x, -MaxX, MaxX);
            a.y = Clamp(y, MinY, MaxY);
        }

        bool CheckClap()
        {
            if (InputLocked || !Together) return false;
            if (Gap > ClapGap || DeltaY > ClapYTol) return false;
            Stage = BrokenApplauseStage.Ovation;
            OvationStarts++;
            foreach (Hand a in h) { a.finger = NoFinger; a.moved = false; }
            return true;
        }

        /// <summary>Avanzata frenata: cresce sempre meno e resta sotto <paramref name="max"/>.</summary>
        public static float Rubber(float d, float max)
        {
            if (Bad(d) || d <= 0f) return 0f;
            return max * (float)(1.0 - Math.Exp(-d / max)) * 0.999f;
        }

        static bool Bad(float v) => float.IsNaN(v) || float.IsInfinity(v);
        static float Clamp(float v, float lo, float hi) => float.IsNaN(v) ? 0f : v < lo ? lo : v > hi ? hi : v;
    }
}
