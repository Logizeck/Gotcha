using System;

namespace Gotcha.Levels.V64
{
    /// <summary>Fasi del livello 265.</summary>
    public enum SinkStage
    {
        Floating,   // la barchetta imbarca acqua dal buco: si prova di tutto
        Sinking,    // piena fino all'orlo: va a fondo e torna su vuota (input bloccato, niente sconfitta)
        Draining,   // il tappo è stato tirato: il "mare" se ne va (input bloccato)
        Landed,     // la barchetta è appoggiata sul fondo della vasca: festa finale
        Done,
    }

    /// <summary>Cosa è successo in un passo di Tick.</summary>
    public enum SinkEvent
    {
        None,
        PatchPeeled,    // il nastro adesivo si è inzuppato ed è venuto via: il buco perde di nuovo
        BoatFull,       // la barchetta è piena: va a fondo (poi Resurface)
    }

    /// <summary>Esito di una tirata della catenella.</summary>
    public enum PullResult
    {
        None,       // input bloccato
        Partial,    // tirata a metà: il tappo si muove appena; lasciandola torna giù (non si perde niente)
        Unplugged,  // tappo fuori: la vasca si svuota (la soluzione)
    }

    /// <summary>
    /// Logica pura del livello 265 "Don't Let It Sink" — "DON'T LET THE BOAT SINK!" Una barchetta in "mare aperto"
    /// imbarca acqua da un buco: l'indicatore sale piano (circa 75 secondi per riempirla). In realtà il mare è una
    /// vasca da bagno (bordo di smalto all'orizzonte, piastrelle in cielo, una paperella, la schiuma, la catenella del
    /// tappo nell'angolo in basso).
    /// False piste (nessuna azzera i progressi): svuotare col secchiello (il livello scende un po', poi l'acqua torna),
    /// tappare il buco col dito (la perdita si ferma solo finché lo si tiene), il nastro adesivo (si inzuppa e si
    /// stacca dopo qualche secondo), la paperella (squittisce e guarda la catenella), inclinare/scuotere il telefono
    /// (onde, il capitano ha il mal di mare). Se la barchetta si riempie non si perde: va a fondo e torna su vuota.
    /// Soluzione: tirare fuori la catenella col tappo: l'acqua se ne va e la barchetta si posa sul fondo della vasca.
    /// Non può affondare se non c'è acqua. Nessuna dipendenza da Unity.
    /// </summary>
    public sealed class DontLetItSinkLogic
    {
        /// <summary>Secondi per riempire la barchetta dal vuoto con il buco aperto.</summary>
        public const float FillSeconds = 75f;
        /// <summary>Quanto toglie una secchiata (frazione della barchetta) e in quanti secondi l'acqua torna.</summary>
        public const float BailAmount = 0.18f, RushBackSeconds = 1.6f;
        /// <summary>Quanto regge il nastro adesivo prima di inzupparsi e staccarsi.</summary>
        public const float PatchSeconds = 3.5f;
        /// <summary>Inclinazione laterale (gravità filtrata, x) che fa le onde, con isteresi.</summary>
        public const float TiltOn = 0.45f, TiltOff = 0.25f;
        /// <summary>Passo massimo di Tick: dopo una pausa lunga l'acqua non fa un salto.</summary>
        public const float MaxStep = 0.25f;

        public SinkStage Stage { get; private set; } = SinkStage.Floating;
        /// <summary>Acqua nella barchetta, 0 = vuota, 1 = piena (va a fondo).</summary>
        public float Level { get; private set; }
        /// <summary>Acqua tolta col secchiello che sta per tornare dentro.</summary>
        public float RushBack { get; private set; }
        public bool FingerOnHole { get; private set; }
        public bool Patched => patchLeft > 0f;
        public float PatchLeft => patchLeft;
        /// <summary>Quanto è tirata la catenella (0 = a riposo, 1 = tappo fuori).</summary>
        public float Pull { get; private set; }
        public bool Leaking => Stage == SinkStage.Floating && !FingerOnHole && !Patched;

        public int Bails { get; private set; }
        public int FingerPlugs { get; private set; }
        public int Patches { get; private set; }
        public int DuckTaps { get; private set; }
        public int Rocks { get; private set; }
        public int Sinks { get; private set; }
        public int PartialPulls { get; private set; }
        public bool CompletionReported { get; private set; }

        public bool InputLocked => Stage != SinkStage.Floating;
        public bool Solved => Stage == SinkStage.Draining || Stage == SinkStage.Landed || Stage == SinkStage.Done;
        /// <summary>Tutte le mosse sbagliate fatte finora (solo statistica: non cambiano niente).</summary>
        public int WrongMoves => Bails + FingerPlugs + Patches + DuckTaps + Rocks + PartialPulls;

        readonly V64AGravity gravity = new V64AGravity();
        float patchLeft;
        bool tilted;

        public void Reset()
        {
            Stage = SinkStage.Floating;
            Level = RushBack = Pull = patchLeft = 0f;
            FingerOnHole = tilted = false;
            Bails = FingerPlugs = Patches = DuckTaps = Rocks = Sinks = PartialPulls = 0;
            CompletionReported = false;
            gravity.Reset(0f, -1f);
        }

        /// <summary>
        /// Il tempo che passa: l'acqua entra dal buco (se non è tappato), torna quella tolta col secchiello, il nastro
        /// si inzuppa. Al massimo un evento per passo.
        /// </summary>
        public SinkEvent Tick(float dt)
        {
            if (Stage != SinkStage.Floating || float.IsNaN(dt) || dt <= 0f) return SinkEvent.None;
            dt = Math.Min(dt, MaxStep);
            SinkEvent ev = SinkEvent.None;
            if (patchLeft > 0f)
            {
                patchLeft -= dt;
                if (patchLeft <= 0f) { patchLeft = 0f; ev = SinkEvent.PatchPeeled; }
            }
            float add = 0f;
            if (!FingerOnHole && !Patched) add += dt / FillSeconds;
            if (RushBack > 0f)
            {
                float back = Math.Min(RushBack, BailAmount / RushBackSeconds * dt);
                RushBack -= back;
                add += back;
            }
            Level = Math.Min(1f, Level + add);
            if (Level >= 1f)
            {
                Stage = SinkStage.Sinking;
                Sinks++;
                FingerOnHole = false;
                patchLeft = 0f;
                Pull = 0f;
                return SinkEvent.BoatFull;
            }
            return ev;
        }

        // ---------- Le false piste ----------

        /// <summary>Una secchiata: il livello scende un po' (0 se la barchetta è già vuota), poi l'acqua torna.</summary>
        public float Bail()
        {
            if (Stage != SinkStage.Floating) return 0f;
            Bails++;
            float taken = Math.Min(BailAmount, Level);
            Level -= taken;
            RushBack += taken;
            return taken;
        }

        /// <summary>Il dito sul buco (true) o tolto (false). Restituisce true se è cambiato qualcosa.</summary>
        public bool SetFinger(bool on)
        {
            if (on && Stage != SinkStage.Floating) return false;
            if (on == FingerOnHole) return false;
            FingerOnHole = on;
            if (on) FingerPlugs++;
            return true;
        }

        /// <summary>Il nastro adesivo sul buco: regge PatchSeconds secondi. 0 se c'è già (o input bloccato).</summary>
        public int ApplyPatch()
        {
            if (Stage != SinkStage.Floating || Patched) return 0;
            patchLeft = PatchSeconds;
            return ++Patches;
        }

        /// <summary>La paperella toccata: squittisce e guarda la catenella.</summary>
        public int TapDuck()
        {
            if (Stage != SinkStage.Floating) return 0;
            return ++DuckTaps;
        }

        /// <summary>Onde (telefono scosso o inclinato): il capitano ha il mal di mare.</summary>
        public int Rock()
        {
            if (Stage != SinkStage.Floating) return 0;
            return ++Rocks;
        }

        /// <summary>
        /// Gravità dello schermo (x verso destra, y verso l'alto): true quando il telefono si inclina di lato oltre
        /// TiltOn (una volta sola finché non torna sotto TiltOff). Non conta come onda finché la barchetta non galleggia.
        /// </summary>
        public bool FeedTilt(float x, float y, float dt)
        {
            gravity.Feed(x, y, dt);
            float ax = Math.Abs(gravity.X);
            if (!tilted && ax > TiltOn) { tilted = true; return Stage == SinkStage.Floating; }
            if (tilted && ax < TiltOff) tilted = false;
            return false;
        }

        /// <summary>Inclinazione laterale filtrata (-1..1) per far dondolare la scena.</summary>
        public float TiltX => gravity.X;

        // ---------- Affondare e tornare su ----------

        /// <summary>La barchetta è tornata a galla vuota: si ricomincia a galleggiare, nient'altro cambia.</summary>
        public bool Resurface()
        {
            if (Stage != SinkStage.Sinking) return false;
            Stage = SinkStage.Floating;
            Level = 0f;
            RushBack = 0f;
            return true;
        }

        // ---------- La catenella del tappo ----------

        /// <summary>La catenella tirata fino a "amount" (0..1). Arrivata in fondo il tappo esce: la soluzione.</summary>
        public PullResult PullChain(float amount)
        {
            if (Stage != SinkStage.Floating) return PullResult.None;
            if (float.IsNaN(amount)) amount = 0f;
            Pull = Math.Max(0f, Math.Min(1f, amount));
            if (Pull < 1f) return PullResult.Partial;
            Stage = SinkStage.Draining;
            FingerOnHole = false;
            patchLeft = 0f;
            RushBack = 0f;
            return PullResult.Unplugged;
        }

        /// <summary>La catenella lasciata a metà: torna giù (il tappo resta dov'è). True se era stata tirata.</summary>
        public bool ReleaseChain()
        {
            if (Stage != SinkStage.Floating) return false;
            bool was = Pull > 0f;
            if (was) PartialPulls++;
            Pull = 0f;
            return was;
        }

        /// <summary>La vasca è vuota: la barchetta è appoggiata sul fondo.</summary>
        public bool FinishDrain()
        {
            if (Stage != SinkStage.Draining) return false;
            Stage = SinkStage.Landed;
            return true;
        }

        /// <summary>Fine della festa: vittoria, una volta sola.</summary>
        public bool TryComplete()
        {
            if (Stage != SinkStage.Landed || CompletionReported) return false;
            CompletionReported = true;
            Stage = SinkStage.Done;
            return true;
        }

        /// <summary>Solo per i tasti di prova: aggiunge acqua di colpo (per vedere la barchetta affondare).</summary>
        public void AddWater(float amount)
        {
            if (Stage != SinkStage.Floating || float.IsNaN(amount)) return;
            Level = Math.Max(0f, Math.Min(0.999f, Level + amount));
        }
    }
}
