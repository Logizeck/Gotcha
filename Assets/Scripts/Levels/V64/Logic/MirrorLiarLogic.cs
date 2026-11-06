using System;
using System.Collections.Generic;

namespace Gotcha.Levels.V64
{
    /// <summary>Fasi del livello "Mirror Liar".</summary>
    public enum MirrorLiarStage
    {
        Playing,    // si prova di tutto
        Revealed,   // il falso non ha seguito il suo dito: lo specchio si crepa (festa, input bloccato)
        Done,       // vittoria già data
    }

    /// <summary>Cosa ha toccato un dito appena appoggiato.</summary>
    public enum MirrorLiarPress
    {
        None,        // niente (input bloccato, terzo dito, dito già seguito, vuoto)
        Left,        // il personaggio a sinistra dello specchio
        Right,       // il personaggio a destra dello specchio
        Mirror,      // lo specchio in mezzo
        SameFigure,  // secondo dito sullo stesso personaggio già tenuto (ignorato: "One finger per twin.")
    }

    /// <summary>Risultato di un movimento o di un dito alzato.</summary>
    public enum MirrorLiarEvent
    {
        None,          // niente da fare
        Moved,         // i due personaggi si sono mossi (sempre speculari)
        Tapped,        // tocco su un personaggio: la stessa reazione su entrambi
        Dropped,       // trascinamento a un dito finito: si sono mossi insieme, nessuna differenza
        Synced,        // due dita mosse in modo speculare (o quasi) e poi alzate: nessuna differenza
        MirrorTapped,  // tocco sullo specchio
        Revealed,      // VITTORIA: il falso non ha seguito il suo dito (una volta sola)
    }

    /// <summary>
    /// Logica pura del livello 91 della roadmap "Mirror Liar" (id mirror_liar) — "FIND THE REAL ONE." Due personaggi
    /// identici ai due lati di uno specchio verticale (x = <see cref="MirrorX"/>); entrambi dicono di essere quello vero.
    /// Inganno: con un dito non li si distingue: il tocco su uno fa reagire allo stesso modo anche l'altro
    /// (<see cref="MirrorLiarEvent.Tapped"/>) e trascinarne uno muove anche l'altro, specchiato rispetto alla linea dello
    /// specchio (x riflessa: 2·MirrorX − x, y uguale), qualunque dei due si prenda.
    /// Soluzione esatta: due dita, una su ciascun personaggio, mosse in modo diverso (direzioni opposte, parallele, una
    /// ferma e l'altra no...). Il vero segue il suo dito; il falso resta il riflesso del vero e ignora il suo dito. La
    /// distanza fra dove il dito del falso lo vorrebbe e il riflesso di dove il dito del vero vuole il vero
    /// (<see cref="Separation"/>, misurata sui bersagli NON limitati: due dita speculari restano a distanza 0 anche
    /// oltre i bordi) supera <see cref="RevealDistance"/>: <see cref="MirrorLiarEvent.Revealed"/>, lo specchio si crepa
    /// e il falso va in frantumi. Chi è il vero è casuale a ogni tentativo (<see cref="Reset"/> pesca il lato con il
    /// System.Random creato col seme del costruttore: nel gioco il seme è l'orologio, nei test è fisso;
    /// <see cref="SetRealSide"/> lo fissa per i test e i tasti di prova).
    /// Dita: al massimo due, per id (il terzo dito è ignorato), in qualunque ordine. Al passaggio da uno a due dita
    /// (e viceversa) le prese si "riallineano" sulla posizione attuale: niente salti. Alzando un dito si torna al
    /// comportamento speculare a un dito, col dito rimasto che continua a trascinare (niente blocchi).
    /// Limiti: ogni personaggio resta dalla sua parte, con i piedi sul pavimento (x in ±[<see cref="MinAbsX"/>,
    /// <see cref="MaxAbsX"/>], y in [<see cref="MinY"/>, <see cref="MaxY"/>]): resta sempre nello schermo.
    /// Cancel() (pausa, app in background, livello spento) lascia andare tutte le dita, senza vincere: i personaggi
    /// restano dove sono. Non si può perdere. Nessuna dipendenza da Unity.
    /// </summary>
    public sealed class MirrorLiarLogic
    {
        /// <summary>La linea dello specchio.</summary>
        public const float MirrorX = 0f;
        /// <summary>Partenza dei piedi dei personaggi (|x| e y).</summary>
        public const float StartAbsX = 1.25f, StartY = -2.5f;
        /// <summary>Dove possono stare i piedi (|x| dalla linea dello specchio e y sul pavimento).</summary>
        public const float MinAbsX = 0.9f, MaxAbsX = 1.75f, MinY = -3.5f, MaxY = -1.7f;
        /// <summary>Area di presa di un personaggio (generosa): mezza larghezza, sotto i piedi e sopra i piedi.</summary>
        public const float HitHalfWidth = 0.9f, HitBelow = 0.3f, HitAbove = 2.3f;
        /// <summary>Area dello specchio (mezza larghezza; in altezza dalla base alla cima).</summary>
        public const float MirrorHalfWidth = 0.35f, MirrorBottomY = -3.9f, MirrorTopY = 2.1f;
        /// <summary>Spostamento oltre il quale un dito non è più un tocco.</summary>
        public const float TapSlop = 0.15f;
        /// <summary>Quanto il falso deve "non seguire" il suo dito perché lo specchio si crepi.</summary>
        public const float RevealDistance = 0.9f;
        /// <summary>Movimento minimo (da quando ci sono due dita) perché una prova a due dita conti come tentativo.</summary>
        public const float SyncMinMove = 0.3f;
        public const int MaxFingers = 2;
        public const int LeftSide = 0, RightSide = 1;

        enum Kind { None, Figure, Mirror }

        sealed class Finger
        {
            public int id;
            public Kind kind;
            public int side;
            public float sx, sy, lx, ly;   // inizio (o ultimo riallineamento) e ultima posizione
            public float ox, oy;           // posizione dei piedi - dito, al riallineamento
            public float px, py;           // dove era il dito quando sono cominciate le due dita
            public bool moved;
        }

        readonly Random rng;
        readonly List<Finger> fingers = new List<Finger>(MaxFingers);
        float realX, realY;   // piedi del vero; il falso è il suo riflesso
        bool twoMoved, completed;

        public MirrorLiarStage Stage { get; private set; } = MirrorLiarStage.Playing;
        public bool InputLocked => Stage != MirrorLiarStage.Playing;

        /// <summary>Lato del personaggio vero (<see cref="LeftSide"/> o <see cref="RightSide"/>), pescato a ogni Reset.</summary>
        public int RealSide { get; private set; }
        public int FakeSide => 1 - RealSide;

        /// <summary>Quanto il falso "non segue" il suo dito adesso (0 senza due dita).</summary>
        public float Separation { get; private set; }
        /// <summary>Separation / RevealDistance, limitata a [0, 1] (per gli effetti: il falso trema, lo specchio scricchiola).</summary>
        public float Strain => Clamp(Separation / RevealDistance, 0f, 1f);
        /// <summary>Una presa per personaggio: le due dita tengono i due personaggi.</summary>
        public bool TwoHanded => HolderOf(LeftSide) != null && HolderOf(RightSide) != null;
        public int FingerCount => fingers.Count;

        public int Taps { get; private set; }
        public int SoloDrags { get; private set; }
        public int SyncTries { get; private set; }
        public int MirrorTaps { get; private set; }
        public int SameFigureTries { get; private set; }
        public int WrongMoves => Taps + SoloDrags + SyncTries + MirrorTaps;

        public MirrorLiarLogic(int seed = 91)
        {
            rng = new Random(seed);
            Reset();
        }

        /// <summary>Tutto da capo: personaggi a casa, nessun dito, nuovo lato vero (casuale).</summary>
        public void Reset()
        {
            Stage = MirrorLiarStage.Playing;
            fingers.Clear();
            RealSide = rng.Next(2);
            realX = SideSign(RealSide) * StartAbsX + MirrorX;
            realY = StartY;
            Separation = 0f;
            twoMoved = completed = false;
            Taps = SoloDrags = SyncTries = MirrorTaps = SameFigureTries = 0;
        }

        /// <summary>Fissa il lato vero (test e tasti di prova): solo durante il gioco e senza dita giù.</summary>
        public bool SetRealSide(int side)
        {
            if (InputLocked || fingers.Count > 0 || (side != LeftSide && side != RightSide)) return false;
            if (side == RealSide) return true;
            // la scena non cambia: il vero diventa il riflesso di prima
            realX = Mirror(realX);
            RealSide = side;
            return true;
        }

        // ---------- Posizioni ----------

        public static float Mirror(float x) => 2f * MirrorX - x;
        static float SideSign(int side) => side == LeftSide ? -1f : 1f;

        public float X(int side) => side == RealSide ? realX : Mirror(realX);
        public float Y(int side) => realY;
        public float LeftX => X(LeftSide);
        public float RightX => X(RightSide);
        public float FeetY => realY;

        public bool Held(int side) => HolderOf(side) != null;

        /// <summary>Dov'è il dito che tiene il personaggio (per gli effetti); false se nessuno lo tiene.</summary>
        public bool FingerTarget(int side, out float x, out float y)
        {
            Finger f = HolderOf(side);
            if (f == null) { x = y = 0f; return false; }
            Target(f, out x, out y);
            return true;
        }

        /// <summary>Su quale personaggio cade il punto (aree generose, ognuna dalla sua parte dello specchio); -1 nessuno.</summary>
        public int FigureAt(float x, float y)
        {
            for (int side = 0; side < 2; side++)
            {
                float fx = X(side);
                if ((x - MirrorX) * SideSign(side) <= 0f) continue;   // dall'altra parte dello specchio
                if (Math.Abs(x - fx) <= HitHalfWidth && y >= realY - HitBelow && y <= realY + HitAbove) return side;
            }
            return -1;
        }

        public static bool OnMirror(float x, float y) => Math.Abs(x - MirrorX) <= MirrorHalfWidth && y >= MirrorBottomY && y <= MirrorTopY;

        // ---------- Dita ----------

        public bool Tracks(int id) => Find(id) != null;

        public MirrorLiarPress Press(int id, float x, float y)
        {
            if (InputLocked || Find(id) != null || fingers.Count >= MaxFingers) return MirrorLiarPress.None;
            int side = FigureAt(x, y);
            var f = new Finger { id = id, sx = x, sy = y, lx = x, ly = y };
            MirrorLiarPress result;
            if (side >= 0)
            {
                if (HolderOf(side) != null)
                {
                    // secondo dito sullo stesso personaggio: occupa il posto ma non fa niente
                    SameFigureTries++;
                    f.kind = Kind.None;
                    fingers.Add(f);
                    return MirrorLiarPress.SameFigure;
                }
                f.kind = Kind.Figure;
                f.side = side;
                result = side == LeftSide ? MirrorLiarPress.Left : MirrorLiarPress.Right;
            }
            else if (OnMirror(x, y)) { f.kind = Kind.Mirror; result = MirrorLiarPress.Mirror; }
            else { f.kind = Kind.None; result = MirrorLiarPress.None; }
            fingers.Add(f);
            Rebase(f);
            if (TwoHanded) StartTwoHanded();
            return result;
        }

        public MirrorLiarEvent Move(int id, float x, float y)
        {
            Finger f = Find(id);
            if (f == null || InputLocked) return MirrorLiarEvent.None;
            f.lx = x;
            f.ly = y;
            if (!f.moved && Dist(x, y, f.sx, f.sy) > TapSlop) f.moved = true;
            if (f.kind != Kind.Figure) return MirrorLiarEvent.None;
            return Apply();
        }

        public MirrorLiarEvent Release(int id)
        {
            Finger f = Find(id);
            if (f == null) return MirrorLiarEvent.None;
            bool wasTwo = TwoHanded;
            bool sessionMoved = twoMoved;
            fingers.Remove(f);
            if (InputLocked) return MirrorLiarEvent.None;
            switch (f.kind)
            {
                case Kind.Mirror:
                    if (f.moved) return MirrorLiarEvent.None;
                    MirrorTaps++;
                    return MirrorLiarEvent.MirrorTapped;
                case Kind.Figure:
                    Separation = 0f;
                    twoMoved = false;
                    // il dito rimasto continua da solo, senza salti
                    foreach (Finger o in fingers) Rebase(o);
                    if (wasTwo)
                    {
                        if (sessionMoved) { SyncTries++; return MirrorLiarEvent.Synced; }
                        if (!f.moved) { Taps++; return MirrorLiarEvent.Tapped; }
                        return MirrorLiarEvent.None;
                    }
                    if (!f.moved) { Taps++; return MirrorLiarEvent.Tapped; }
                    SoloDrags++;
                    return MirrorLiarEvent.Dropped;
                default:
                    return MirrorLiarEvent.None;
            }
        }

        /// <summary>Pausa, app in background, livello spento: tutte le dita spariscono, i personaggi restano dove sono.</summary>
        public void Cancel()
        {
            fingers.Clear();
            Separation = 0f;
            twoMoved = false;
        }

        /// <summary>La vittoria si dà una volta sola, solo dopo la rivelazione.</summary>
        public bool TryComplete()
        {
            if (Stage != MirrorLiarStage.Revealed || completed) return false;
            completed = true;
            Stage = MirrorLiarStage.Done;
            return true;
        }

        // ---------- Interni ----------

        Finger Find(int id)
        {
            foreach (Finger f in fingers) if (f.id == id) return f;
            return null;
        }

        Finger HolderOf(int side)
        {
            foreach (Finger f in fingers) if (f.kind == Kind.Figure && f.side == side) return f;
            return null;
        }

        /// <summary>La presa riparte dalla posizione attuale del personaggio e del dito (niente salti).</summary>
        void Rebase(Finger f)
        {
            f.sx = f.lx;
            f.sy = f.ly;
            f.px = f.lx;
            f.py = f.ly;
            if (f.kind != Kind.Figure) return;
            f.ox = X(f.side) - f.lx;
            f.oy = Y(f.side) - f.ly;
        }

        void StartTwoHanded()
        {
            foreach (Finger o in fingers) Rebase(o);
            Separation = 0f;
            twoMoved = false;
        }

        /// <summary>Dove il dito vorrebbe i piedi del suo personaggio (senza limiti); fermo finché non supera il tocco.</summary>
        static void Target(Finger f, out float x, out float y)
        {
            if (!f.moved) { x = f.sx + f.ox; y = f.sy + f.oy; return; }
            x = f.lx + f.ox;
            y = f.ly + f.oy;
        }

        MirrorLiarEvent Apply()
        {
            Finger real = HolderOf(RealSide), fake = HolderOf(FakeSide);
            if (real != null && fake != null)
            {
                // due dita: il vero segue il suo dito, il falso resta il suo riflesso e ignora il dito
                Target(real, out float rx, out float ry);
                Target(fake, out float fx, out float fy);
                SetReal(rx, ry);
                Separation = Dist(Mirror(rx), ry, fx, fy);
                if (!twoMoved && (Dist(real.lx, real.ly, real.px, real.py) > SyncMinMove || Dist(fake.lx, fake.ly, fake.px, fake.py) > SyncMinMove)) twoMoved = true;
                if (Separation >= RevealDistance)
                {
                    Stage = MirrorLiarStage.Revealed;
                    fingers.Clear();
                    return MirrorLiarEvent.Revealed;
                }
                return MirrorLiarEvent.Moved;
            }
            Finger one = real ?? fake;
            if (one == null) return MirrorLiarEvent.None;
            // un dito solo: il personaggio preso lo segue, l'altro fa il riflesso (vero o falso, non si distinguono)
            Target(one, out float tx, out float ty);
            if (one.side == RealSide) SetReal(tx, ty);
            else SetReal(Mirror(tx), ty);
            Separation = 0f;
            return MirrorLiarEvent.Moved;
        }

        /// <summary>Piedi del vero, dentro la sua metà e sul pavimento (il riflesso resta così nell'altra metà).</summary>
        void SetReal(float x, float y)
        {
            float s = SideSign(RealSide);
            float ax = Clamp((x - MirrorX) * s, MinAbsX, MaxAbsX);
            realX = MirrorX + ax * s;
            realY = Clamp(y, MinY, MaxY);
        }

        static float Dist(float ax, float ay, float bx, float by)
        {
            float dx = ax - bx, dy = ay - by;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        static float Clamp(float v, float lo, float hi) => v < lo ? lo : (v > hi ? hi : v);
    }
}
