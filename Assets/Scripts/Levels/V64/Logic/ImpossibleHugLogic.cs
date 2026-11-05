using System;

namespace Gotcha.Levels.V64
{
    /// <summary>Fasi del livello "The Impossible Hug".</summary>
    public enum ImpossibleHugStage
    {
        Playing,   // i due amici sono arrabbiati: si prova di tutto
        Hugging,   // si abbracciano: festa in corso (input bloccato)
        Done,      // vittoria già data
    }

    /// <summary>Risultato di uno spostamento di un dito.</summary>
    public enum ImpossibleHugMove
    {
        None,       // il dito non tiene nessuno (o input bloccato)
        Resisting,  // un amico solo, trascinato: punta i piedi (l'altro si allontana)
        Following,  // tutti e due tenuti: seguono le dita
        Hug,        // tutti e due tenuti e tutti e due nella zona centrale: si abbracciano (VITTORIA, una volta sola)
    }

    /// <summary>Risultato di un dito che si alza.</summary>
    public enum ImpossibleHugDrop
    {
        None,         // il dito non teneva nessuno (o input bloccato)
        Tap,          // tocco senza trascinare su un amico solo: "Hmph!"
        SteppedBack,  // trascinato da solo e lasciato: tutti e due fanno un passo indietro (oltre la partenza, poi a casa)
        Separated,    // erano tenuti tutti e due e un dito si è alzato: quell'amico torna indietro
    }

    /// <summary>
    /// Logica pura del livello 38 della roadmap "The Impossible Hug" (id impossible_hug) — "MAKE THE TWO FRIENDS MAKE UP."
    /// Due amici arrabbiati (a braccia conserte) stanno ai lati dello schermo, un cuore spezzato in mezzo.
    /// Coordinate LOCALI rispetto al centro della scena (0, 0): l'amico sinistro parte da (-<see cref="HomeX"/>, 0), il
    /// destro da (+<see cref="HomeX"/>, 0).
    /// Inganno: trascinato da solo (un dito), un amico punta i piedi: verso il centro avanza al massimo di
    /// <see cref="SoloReach"/> (frenato, mai raggiunto) e intanto l'altro si allontana (fino a <see cref="RetreatMax"/>);
    /// al rilascio tutti e due tornano a casa (il livello mostra il passo indietro). Così da soli non entrano MAI nella
    /// zona dell'abbraccio.
    /// Soluzione esatta: due dita diverse, una per amico (in qualunque ordine): tenuti tutti e due, seguono le dita
    /// liberamente (sempre dentro i limiti); quando TUTTI E DUE sono a distanza &lt;= <see cref="HugRadius"/> dal centro
    /// mentre sono tenuti, si abbracciano (una volta sola). Se un dito si alza, quell'amico torna indietro e l'altro
    /// torna a puntare i piedi: niente abbraccio finché non sono tenuti di nuovo tutti e due.
    /// Regole delle dita: un dito tiene al massimo un amico (lo stesso dito non può prenderli tutti e due), un amico è
    /// tenuto al massimo da un dito; quindi al massimo due dita contano. Cancel() (pausa, app in background, livello
    /// spento) lascia andare tutto senza contare niente. Non si può perdere. Nessuna dipendenza da Unity.
    /// </summary>
    public sealed class ImpossibleHugLogic
    {
        public const int Left = 0, Right = 1;
        /// <summary>Id "nessun dito".</summary>
        public const int NoFinger = int.MinValue;

        /// <summary>Distanza dal centro della casa di ciascun amico.</summary>
        public const float HomeX = 1.45f;
        /// <summary>Limiti dei centri degli amici (locali): restano sempre dentro lo schermo.</summary>
        public const float MaxX = 1.7f, MinY = -1.6f, MaxY = 2.0f;
        /// <summary>Raggio della zona centrale: tutti e due dentro (e tenuti) = abbraccio.</summary>
        public const float HugRadius = 0.7f;
        /// <summary>Tutti e due tenuti ed entro questo raggio: si guardano imbarazzati (stato intermedio, solo grafica).</summary>
        public const float ShyRadius = 1.15f;
        /// <summary>Di quanto al massimo un amico trascinato da solo avanza verso il centro (frenato, mai raggiunto).</summary>
        public const float SoloReach = 0.4f;
        /// <summary>Di quanto al massimo un amico trascinato da solo si sposta in su o in giù.</summary>
        public const float SoloLift = 0.3f;
        /// <summary>Di quanto al massimo l'altro si allontana mentre il primo viene trascinato verso il centro.</summary>
        public const float RetreatMax = 0.25f;
        /// <summary>Spostamento del dito oltre il quale non è più un tocco.</summary>
        public const float TapSlop = 0.15f;

        sealed class Friend
        {
            public float homeX, x, y, ox, oy, fx, fy;
            public int finger = NoFinger;
            public bool moved;
            public float sx, sy;   // dove il dito ha preso l'amico (per distinguere il tocco)
        }

        readonly Friend[] f = { new Friend { homeX = -HomeX }, new Friend { homeX = HomeX } };
        bool completed;

        public ImpossibleHugStage Stage { get; private set; } = ImpossibleHugStage.Playing;
        public bool InputLocked => Stage != ImpossibleHugStage.Playing;

        public int SoloDrags { get; private set; }
        public int Separations { get; private set; }
        public int Taps { get; private set; }
        public int RefusedGrabs { get; private set; }
        public int WrongMoves => SoloDrags + Separations;

        public ImpossibleHugLogic() { Reset(); }

        public void Reset()
        {
            Stage = ImpossibleHugStage.Playing;
            foreach (Friend a in f) { a.finger = NoFinger; a.moved = false; a.x = a.homeX; a.y = 0f; a.ox = a.oy = a.fx = a.fy = a.sx = a.sy = 0f; }
            SoloDrags = Separations = Taps = RefusedGrabs = 0;
            completed = false;
        }

        // ---------- Stato letto dal livello ----------

        static bool Valid(int who) => who == Left || who == Right;
        public float X(int who) => Valid(who) ? f[who].x : 0f;
        public float Y(int who) => Valid(who) ? f[who].y : 0f;
        public float HomeOf(int who) => Valid(who) ? f[who].homeX : 0f;
        public bool IsHeld(int who) => Valid(who) && f[who].finger != NoFinger;
        public int HolderOf(int who) => Valid(who) ? f[who].finger : NoFinger;
        public int HeldCount => (IsHeld(Left) ? 1 : 0) + (IsHeld(Right) ? 1 : 0);
        /// <summary>Tutti e due tenuti (da due dita diverse).</summary>
        public bool Together => HeldCount == 2;
        /// <summary>Distanza dal centro di un amico.</summary>
        public float Dist(int who) => Valid(who) ? (float)Math.Sqrt(f[who].x * f[who].x + f[who].y * f[who].y) : 0f;
        /// <summary>Tutti e due tenuti e già vicini: si guardano (stato intermedio).</summary>
        public bool Shy => Together && Dist(Left) <= ShyRadius && Dist(Right) <= ShyRadius;

        /// <summary>Chi tiene questo dito (-1 nessuno).</summary>
        public int HeldBy(int finger)
        {
            if (finger == NoFinger) return -1;
            for (int i = 0; i < 2; i++) if (f[i].finger == finger) return i;
            return -1;
        }

        /// <summary>Quanto sono lontani (0 = nella zona dell'abbraccio, 1 = a casa, oltre 1 = si sono allontanati).</summary>
        public float Apart
        {
            get
            {
                float d = (Dist(Left) + Dist(Right)) * 0.5f;
                float k = (d - HugRadius) / (HomeX - HugRadius);
                return k < 0f ? 0f : k;
            }
        }

        // ---------- Dita ----------

        /// <summary>
        /// Il dito <paramref name="finger"/> (in (fx, fy), locali) prende l'amico <paramref name="who"/>. Falso se l'input è
        /// bloccato, se l'amico è già tenuto, o se lo stesso dito tiene già l'altro (un dito non li prende mai tutti e due).
        /// </summary>
        public bool Grab(int finger, int who, float fx, float fy)
        {
            if (InputLocked || !Valid(who) || finger == NoFinger || Bad(fx) || Bad(fy)) return false;
            Friend a = f[who];
            if (a.finger != NoFinger) return false;
            if (HeldBy(finger) >= 0) { RefusedGrabs++; return false; }
            a.finger = finger;
            a.moved = false;
            a.fx = a.sx = fx;
            a.fy = a.sy = fy;
            Rebase();
            Solve();
            CheckHug();
            return true;
        }

        /// <summary>Il dito si sposta in (fx, fy) (locali).</summary>
        public ImpossibleHugMove Move(int finger, float fx, float fy)
        {
            if (InputLocked) return ImpossibleHugMove.None;
            int who = HeldBy(finger);
            if (who < 0 || Bad(fx) || Bad(fy)) return ImpossibleHugMove.None;
            Friend a = f[who];
            a.fx = fx;
            a.fy = fy;
            float dx = fx - a.sx, dy = fy - a.sy;
            if (dx * dx + dy * dy > TapSlop * TapSlop) a.moved = true;
            Solve();
            if (CheckHug()) return ImpossibleHugMove.Hug;
            return Together ? ImpossibleHugMove.Following : ImpossibleHugMove.Resisting;
        }

        /// <summary>Il dito si alza: l'amico che teneva torna indietro.</summary>
        public ImpossibleHugDrop Release(int finger)
        {
            int who = HeldBy(finger);
            if (who < 0) return ImpossibleHugDrop.None;
            bool together = Together;
            Friend a = f[who];
            bool moved = a.moved;
            a.finger = NoFinger;
            a.moved = false;
            if (InputLocked) return ImpossibleHugDrop.None;
            Solve();
            if (together) { Separations++; return ImpossibleHugDrop.Separated; }
            if (!moved) { Taps++; return ImpossibleHugDrop.Tap; }
            SoloDrags++;
            return ImpossibleHugDrop.SteppedBack;
        }

        /// <summary>Dita perse (pausa, app in background, livello spento): si lascia tutto, a casa, senza contare niente.</summary>
        public bool Cancel()
        {
            bool any = HeldCount > 0;
            foreach (Friend a in f) { a.finger = NoFinger; a.moved = false; }
            if (!InputLocked) Solve();
            return any;
        }

        /// <summary>Fine della festa: true una volta sola (allora il livello chiama Win).</summary>
        public bool TryComplete()
        {
            if (Stage != ImpossibleHugStage.Hugging || completed) return false;
            completed = true;
            Stage = ImpossibleHugStage.Done;
            return true;
        }

        // ---------- Calcolo delle posizioni ----------

        /// <summary>Lo scarto dito-amico si ricalcola a ogni cambio del numero di dita: nessun salto.</summary>
        void Rebase()
        {
            foreach (Friend a in f)
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
                foreach (Friend a in f) Place(a, a.fx + a.ox, a.fy + a.oy);
                return;
            }
            if (n == 0)
            {
                foreach (Friend a in f) { a.x = a.homeX; a.y = 0f; }
                return;
            }
            Friend h = IsHeld(Left) ? f[Left] : f[Right];
            Friend o = h == f[Left] ? f[Right] : f[Left];
            float inDir = h.homeX < 0f ? 1f : -1f;   // verso il centro
            float wantX = h.fx + h.ox, wantY = h.fy + h.oy;
            float inward = (wantX - h.homeX) * inDir;
            float got = inward > 0f ? Rubber(inward, SoloReach) : inward;   // verso il centro frenato, all'indietro libero
            float lift = wantY >= 0f ? Rubber(wantY, SoloLift) : -Rubber(-wantY, SoloLift);
            Place(h, h.homeX + got * inDir, lift);
            float retreat = got > 0f ? RetreatMax * Math.Min(1f, got / SoloReach) : 0f;
            Place(o, o.homeX + inDir * retreat, 0f);   // l'altro si allontana dalla parte opposta
        }

        static void Place(Friend a, float x, float y)
        {
            a.x = Clamp(x, -MaxX, MaxX);
            a.y = Clamp(y, MinY, MaxY);
        }

        bool CheckHug()
        {
            if (InputLocked || !Together) return false;
            if (Dist(Left) > HugRadius || Dist(Right) > HugRadius) return false;
            Stage = ImpossibleHugStage.Hugging;
            foreach (Friend a in f) { a.finger = NoFinger; a.moved = false; }
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
