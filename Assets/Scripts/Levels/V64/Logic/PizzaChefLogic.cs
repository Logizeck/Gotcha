using System;
using System.Collections.Generic;

namespace Gotcha.Levels.V64
{
    /// <summary>Fasi del livello "Pizza Chef" (ricetta -> condimenti -> forno -> taglio -> servizio -> verdetto).</summary>
    public enum PizzaChefPhase
    {
        Toppings,   // si condisce la pizza
        Baking,     // la pizza è in forno (animazione)
        Cutting,    // cotta: si taglia in quattro
        Serving,    // tagliata: una fetta a ogni topolino
        Judging,    // servite tutte le fette: animazione del verdetto in corso (esito non ancora dato)
        Done,       // esito dato (vittoria, sconfitta, uovo o ananas): non succede più niente
    }

    /// <summary>Esito di un ingrediente lasciato sulla pizza.</summary>
    public enum PizzaChefAdd
    {
        Ignored,     // livello finito: niente
        Added,       // nuovo ingrediente sulla pizza (i pezzi vanno sparsi)
        Duplicate,   // c'era già: niente
        TooLate,     // la pizza è già in forno / cotta: non si condisce più ("It's already baked!")
        Pineapple,   // ANANAS: sconfitta immediata con presa in giro, in qualunque fase (una volta sola)
    }

    /// <summary>Verdetto finale dopo aver servito tutte le fette.</summary>
    public enum PizzaChefVerdict
    {
        None,       // nessun verdetto (fase sbagliata o già dato)
        Win,        // esattamente la ricetta
        Missing,    // manca qualcosa
        Extra,      // c'è un ingrediente che non è sulla lavagna
        SpicyEgg,   // EASTER EGG: col peperoncino i topi strillano e vanno a fuoco
    }

    /// <summary>Un pezzo di condimento sulla pizza: posizione (coordinate della pizza, centro 0,0) e rotazione in gradi.</summary>
    public struct PizzaChefPiece
    {
        public float X, Y, Rotation;
        public PizzaChefPiece(float x, float y, float rotation) { X = x; Y = y; Rotation = rotation; }
    }

    /// <summary>
    /// Sparpaglia i pezzi di un condimento a caso dentro il disco della pizza: uniformi nel cerchio (meno un margine
    /// dal bordo), ruotati a caso, con una distanza minima dagli altri pezzi già sulla pizza (anche di altri
    /// ingredienti). Per ogni pezzo prova più candidati: tiene il primo abbastanza lontano da tutti, altrimenti il
    /// candidato più lontano dai vicini (così anche con la pizza piena non si blocca mai e non si ammucchia).
    /// Deterministico: dipende solo dal System.Random passato.
    /// </summary>
    public static class PizzaChefScatter
    {
        public const int Candidates = 40;

        public static List<PizzaChefPiece> Scatter(Random rng, int count, float radius, float minSpacing, IList<PizzaChefPiece> existing = null)
        {
            if (rng == null) throw new ArgumentNullException(nameof(rng));
            var placed = new List<PizzaChefPiece>();
            if (count <= 0 || radius <= 0f) return placed;
            float min2 = minSpacing * minSpacing;
            for (int n = 0; n < count; n++)
            {
                float bestX = 0f, bestY = 0f, bestD = -1f;
                for (int k = 0; k < Candidates; k++)
                {
                    // uniforme nel disco: r = R * sqrt(u)
                    double a = rng.NextDouble() * Math.PI * 2.0, r = radius * Math.Sqrt(rng.NextDouble());
                    float x = (float)(Math.Cos(a) * r), y = (float)(Math.Sin(a) * r);
                    float d = NearestSq(x, y, existing, placed);
                    if (d > bestD) { bestD = d; bestX = x; bestY = y; }
                    if (d >= min2) break;
                }
                placed.Add(new PizzaChefPiece(bestX, bestY, (float)(rng.NextDouble() * 360.0)));
            }
            return placed;
        }

        static float NearestSq(float x, float y, IList<PizzaChefPiece> a, List<PizzaChefPiece> b)
        {
            float best = float.MaxValue;
            if (a != null) foreach (PizzaChefPiece p in a) best = Math.Min(best, (p.X - x) * (p.X - x) + (p.Y - y) * (p.Y - y));
            foreach (PizzaChefPiece p in b) best = Math.Min(best, (p.X - x) * (p.X - x) + (p.Y - y) * (p.Y - y));
            return best;
        }
    }

    /// <summary>
    /// Logica pura del livello "Pizza Chef" (senza Unity, testabile). La ricetta della nonna è casuale (4 o 5
    /// ingredienti, mai l'ananas né il peperoncino, sempre almeno pomodoro o mozzarella); si condisce, si inforna, si
    /// taglia, si serve. L'ANANAS è sempre sconfitta con presa in giro, in qualunque momento prima dell'esito (anche
    /// sulla pizza in forno, già cotta o già tagliata, anche se il resto è giusto); il peperoncino (senza ananas) è
    /// l'easter egg "Inferno Pizza". Un solo esito per partita: dopo, ogni chiamata è ignorata.
    /// </summary>
    public class PizzaChefLogic
    {
        public const string Tomato = "tomato", Mozzarella = "mozzarella", Mushroom = "mushroom", Olive = "olive",
            Ham = "ham", Salami = "salami", Artichoke = "artichoke", Chili = "chili", Pineapple = "pineapple";

        /// <summary>Tutti gli ingredienti sul bancone (ordine delle ciotole e degli strati sulla pizza).</summary>
        public static readonly string[] Ingredients = { Tomato, Mozzarella, Mushroom, Olive, Ham, Salami, Artichoke, Chili, Pineapple };
        /// <summary>Nomi in inglese delle ciotole, stesso ordine di <see cref="Ingredients"/>.</summary>
        public static readonly string[] Labels = { "TOMATO", "MOZZARELLA", "MUSHROOMS", "OLIVES", "HAM", "SALAMI", "ARTICHOKES", "CHILI", "PINEAPPLE" };
        /// <summary>Ingredienti che possono finire nella ricetta (MAI ananas né peperoncino). Stesso ordine dei disegni a gesso.</summary>
        public static readonly string[] RecipePool = { Tomato, Mozzarella, Mushroom, Olive, Ham, Salami, Artichoke };
        public const int MinRecipe = 4, MaxRecipe = 5;

        /// <summary>Disco dove cadono i pezzi (dentro il sugo, raggio 0.92) e distanza minima tra due pezzi.</summary>
        public const float ScatterRadius = 0.78f, MinSpacing = 0.17f;

        /// <summary>Prese in giro per l'ananas (a rotazione).</summary>
        public static readonly string[] Taunts =
        {
            "PINEAPPLE? On PIZZA? Grandma has left the chat.",
            "Grandma is calling the Italian embassy.",
            "That's not a pizza. That's a fruit salad crime.",
            "Pineapple?! Grandma is rewriting her will. Without you.",
        };

        public static string Taunt(int turn) => Taunts[((turn % Taunts.Length) + Taunts.Length) % Taunts.Length];

        /// <summary>Quanti pezzi per condimento (pomodoro e mozzarella sono strati interi: 0).</summary>
        public static int PiecesFor(string id)
        {
            switch (id)
            {
                case Mushroom: return 9;
                case Olive: return 11;
                case Ham: return 7;
                case Salami: return 8;
                case Artichoke: return 7;
                case Chili: return 7;
                case Pineapple: return 9;
                default: return 0;
            }
        }

        public static string LabelOf(string id)
        {
            int i = Array.IndexOf(Ingredients, id);
            return i < 0 ? id.ToUpperInvariant() : Labels[i];
        }

        /// <summary>Ricetta casuale: 4 o 5 ingredienti dal pool (mai ananas/peperoncino), almeno pomodoro o mozzarella, in ordine di pool.</summary>
        public static string[] GenerateRecipe(Random rng)
        {
            if (rng == null) throw new ArgumentNullException(nameof(rng));
            var pool = new List<string>(RecipePool);
            for (int i = pool.Count - 1; i > 0; i--) { int j = rng.Next(i + 1); (pool[i], pool[j]) = (pool[j], pool[i]); }
            int n = rng.Next(MinRecipe, MaxRecipe + 1);
            List<string> pick = pool.GetRange(0, n);
            if (!pick.Contains(Tomato) && !pick.Contains(Mozzarella)) pick[n - 1] = rng.Next(2) == 0 ? Tomato : Mozzarella;
            pick.Sort((a, b) => Array.IndexOf(RecipePool, a).CompareTo(Array.IndexOf(RecipePool, b)));
            return pick.ToArray();
        }

        readonly Random rng;
        readonly HashSet<string> added = new HashSet<string>();
        readonly List<PizzaChefPiece> pieces = new List<PizzaChefPiece>();
        PizzaChefVerdict pending = PizzaChefVerdict.None;

        public string[] Required { get; private set; }
        public PizzaChefPhase Phase { get; private set; } = PizzaChefPhase.Toppings;
        /// <summary>Esito dato (vale solo in fase Done): Win/Missing/Extra/SpicyEgg, oppure None se è stato l'ananas.</summary>
        public PizzaChefVerdict Outcome { get; private set; } = PizzaChefVerdict.None;
        public bool PineappleFailed { get; private set; }
        public bool IsDone => Phase == PizzaChefPhase.Done;
        public IEnumerable<string> Added => added;
        public int AddedCount => added.Count;
        public bool Has(string id) => added.Contains(id);
        /// <summary>Tutti i pezzi già sulla pizza (per la distanza minima tra ingredienti diversi).</summary>
        public IReadOnlyList<PizzaChefPiece> Pieces => pieces;

        /// <summary>Partita col seme dato: ricetta casuale (o quella fissa passata) e pezzi sparsi riproducibili.</summary>
        public PizzaChefLogic(int seed, string[] fixedRecipe = null)
        {
            rng = new Random(seed);
            Required = fixedRecipe != null && fixedRecipe.Length > 0 ? (string[])fixedRecipe.Clone() : GenerateRecipe(rng);
        }

        /// <summary>Un ingrediente lasciato sulla pizza (o sugli spicchi, o sulla pizza in forno).</summary>
        public PizzaChefAdd TryAdd(string id)
        {
            if (Phase == PizzaChefPhase.Done || string.IsNullOrEmpty(id)) return PizzaChefAdd.Ignored;
            if (id == Pineapple)
            {
                // in qualunque fase prima dell'esito: sconfitta, e nient'altro (né vittoria né uovo) può più accadere
                added.Add(id);
                PineappleFailed = true;
                pending = PizzaChefVerdict.None;
                Outcome = PizzaChefVerdict.None;
                Phase = PizzaChefPhase.Done;
                return PizzaChefAdd.Pineapple;
            }
            if (Phase != PizzaChefPhase.Toppings) return PizzaChefAdd.TooLate;
            return added.Add(id) ? PizzaChefAdd.Added : PizzaChefAdd.Duplicate;
        }

        /// <summary>Pezzi sparsi a caso per un ingrediente appena aggiunto (vuoto per sugo e mozzarella).</summary>
        public List<PizzaChefPiece> PlacePieces(string id) => PlacePieces(PiecesFor(id));

        public List<PizzaChefPiece> PlacePieces(int count)
        {
            List<PizzaChefPiece> list = PizzaChefScatter.Scatter(rng, count, ScatterRadius, MinSpacing, pieces);
            pieces.AddRange(list);
            return list;
        }

        /// <summary>Si può infornare solo con almeno un condimento.</summary>
        public bool StartBake()
        {
            if (Phase != PizzaChefPhase.Toppings || added.Count == 0) return false;
            Phase = PizzaChefPhase.Baking;
            return true;
        }

        public bool BakeDone()
        {
            if (Phase != PizzaChefPhase.Baking) return false;
            Phase = PizzaChefPhase.Cutting;
            return true;
        }

        public bool CutDone()
        {
            if (Phase != PizzaChefPhase.Cutting) return false;
            Phase = PizzaChefPhase.Serving;
            return true;
        }

        /// <summary>Tutte le fette servite: calcola il verdetto (l'animazione parte, l'esito si dà con <see cref="Commit"/>).</summary>
        public PizzaChefVerdict ServedAll()
        {
            if (Phase != PizzaChefPhase.Serving) return PizzaChefVerdict.None;
            Phase = PizzaChefPhase.Judging;
            pending = Evaluate();
            return pending;
        }

        /// <summary>Verdetto per gli ingredienti attuali (l'ananas qui non può esserci: fa perdere subito).</summary>
        public PizzaChefVerdict Evaluate()
        {
            if (added.Contains(Chili)) return PizzaChefVerdict.SpicyEgg;
            foreach (string r in Required) if (!added.Contains(r)) return PizzaChefVerdict.Missing;
            foreach (string a in added) if (Array.IndexOf(Required, a) < 0) return PizzaChefVerdict.Extra;
            return PizzaChefVerdict.Win;
        }

        /// <summary>Fine dell'animazione del verdetto: dà l'esito una volta sola (None se intanto è arrivato l'ananas).</summary>
        public PizzaChefVerdict Commit()
        {
            if (Phase != PizzaChefPhase.Judging) return PizzaChefVerdict.None;
            Phase = PizzaChefPhase.Done;
            Outcome = pending;
            return pending;
        }
    }
}
