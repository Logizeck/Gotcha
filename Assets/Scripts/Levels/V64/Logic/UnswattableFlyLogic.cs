using System;

namespace Gotcha.Levels.V64
{
    /// <summary>Fasi del livello 267.</summary>
    public enum FlyStage
    {
        Buzzing,    // la mosca ronza per la cucina: si prova di tutto
        Leaving,    // la finestra è aperta: la mosca se ne accorge e va verso il varco (si può ancora richiudere)
        Outside,    // è uscita: saluta (input bloccato)
        Done,
    }

    /// <summary>Cosa è successo in un passo di Tick (al massimo uno per passo; gli altri aspettano il passo dopo).</summary>
    public enum FlyEvent
    {
        None,
        Bonk,           // la mosca va a sbattere contro il vetro (l'indizio, ogni qualche secondo)
        ScreenWalkOver, // finita la passeggiata sul vetro del telefono: torna a volare in cucina
        MaskOff,        // si toglie la maschera antigas (e starnutisce)
        NoticedWindow,  // si è accorta della finestra aperta: parte verso il varco
        FlewOut,        // è passata dal varco: è fuori
    }

    /// <summary>Esito di un colpo di paletta.</summary>
    public enum SwatResult
    {
        None,           // input bloccato
        Miss,           // schivata (sempre)
        SmashedCake,    // schivata, e la paletta ha spiaccicato la torta
    }

    /// <summary>Esito di un tocco sul gancio della finestra.</summary>
    public enum LatchResult
    {
        None,       // input bloccato
        Unlatched,  // sganciata: ora il vetro scorre
        Latched,    // richiusa col gancio
        Blocked,    // la finestra è aperta: il gancio non arriva
    }

    /// <summary>Cosa ha fatto un movimento della finestra.</summary>
    public enum WindowEvent
    {
        None,
        Blocked,        // agganciata: non si muove (il vetro sbatacchia)
        StartedLeaving, // aperta abbastanza: la mosca ci pensa lei
        LeaveCancelled, // richiusa prima che la mosca uscisse: resta dentro (nessun blocco, si può riaprire)
    }

    /// <summary>
    /// Logica pura del livello 267 "The Unswattable Fly" — "GET RID OF THE FLY!" Una mosca ronza per la cucina e si
    /// posa sulla torta. Le false piste non funzionano mai e non azzerano niente: toccarla (schiva prima che il dito
    /// arrivi: Dodge la sposta sempre lontano dal punto toccato), la paletta (schiva, e si spiaccica la torta), lo spray
    /// (si mette una mascherina antigas, la stanza si annebbia), coprire la torta (si posa sul vetro del telefono).
    /// Indizio: ogni qualche secondo sbatte contro il vetro della finestra chiusa ("bonk... bonk") e pensa a un albero.
    /// Soluzione: sganciare la finestra e aprirla: dopo un attimo la mosca esce da sola e saluta. Richiuderla prima che
    /// esca è permesso: resta dentro e si può riaprire. Nessuna dipendenza da Unity.
    /// </summary>
    public sealed class UnswattableFlyLogic
    {
        // ---- dove vola la mosca (coordinate del mondo; il livello usa gli stessi numeri) ----
        public const float MinX = -2.2f, MaxX = 2.2f, MinY = -3.4f, MaxY = 2.6f;
        /// <summary>Un dito che comincia entro questo raggio dalla mosca la fa scappare (area generosa).</summary>
        public const float DodgeRadius = 1.25f;
        /// <summary>Dopo lo scatto la mosca è almeno a questa distanza dal punto toccato.</summary>
        public const float MinDodge = 1.3f, DodgeStep = 1.8f;

        // ---- tempi ----
        public const float FirstBonk = 3f, BonkPeriod = 5.5f, KnockBonk = 0.35f;
        public const float MaskSeconds = 3.2f, FogSeconds = 2.4f, ScreenSeconds = 2.4f;
        /// <summary>Quanto ci mette ad accorgersi della finestra aperta, e poi ad arrivare al varco.</summary>
        public const float NoticeSeconds = 0.8f, TravelSeconds = 1.4f;
        public const float LeaveSeconds = NoticeSeconds + TravelSeconds;
        /// <summary>Apertura (0..1) oltre la quale il varco basta alla mosca.</summary>
        public const float OpenEnough = 0.35f;
        /// <summary>Passo massimo di Tick: dopo una pausa lunga niente salti.</summary>
        public const float MaxStep = 0.25f;

        public FlyStage Stage { get; private set; } = FlyStage.Buzzing;
        public bool Latched { get; private set; } = true;
        /// <summary>Quanto è alzato il vetro scorrevole (0 = chiuso, 1 = tutto aperto).</summary>
        public float Opening { get; private set; }
        public bool WindowOpen => Opening >= OpenEnough;
        /// <summary>Avanzamento dell'uscita (0..1): fino a NoticeSeconds si guarda intorno, poi vola verso il varco.</summary>
        public float LeaveProgress => Stage == FlyStage.Leaving ? Math.Min(1f, leaveT / LeaveSeconds) : (Stage == FlyStage.Buzzing ? 0f : 1f);
        /// <summary>Si è accorta della finestra aperta (finito il momento in cui si guarda intorno).</summary>
        public bool Noticed => InputLocked || (Stage == FlyStage.Leaving && leaveT >= NoticeSeconds);
        public float MaskLeft { get; private set; }
        public float FogLeft { get; private set; }
        public float ScreenLeft { get; private set; }
        public bool Masked => MaskLeft > 0f;
        public bool Foggy => FogLeft > 0f;
        /// <summary>La mosca passeggia sul vetro del telefono (dopo aver coperto la torta).</summary>
        public bool OnScreen => ScreenLeft > 0f;
        /// <summary>Quanto è spiaccicata la torta (0 = intatta, 3 = al massimo).</summary>
        public int CakeSquash => Math.Min(3, CakeSmashes);

        public int Taps { get; private set; }
        public int Swats { get; private set; }
        public int CakeSmashes { get; private set; }
        public int Sprays { get; private set; }
        public int Covers { get; private set; }
        public int Rattles { get; private set; }
        public int Knocks { get; private set; }
        public int Bonks { get; private set; }
        public int LeaveCancels { get; private set; }
        public bool CompletionReported { get; private set; }

        public bool InputLocked => Stage == FlyStage.Outside || Stage == FlyStage.Done;
        public bool Solved => InputLocked;
        /// <summary>Tutte le mosse sbagliate fatte finora (solo statistica: non cambiano niente).</summary>
        public int WrongMoves => Taps + Swats + Sprays + Covers + Rattles;

        float leaveT, bonkIn;
        bool pendingBonk, pendingScreen, pendingMask;

        public UnswattableFlyLogic() { Reset(); }

        public void Reset()
        {
            Stage = FlyStage.Buzzing;
            Latched = true;
            Opening = 0f;
            MaskLeft = FogLeft = ScreenLeft = leaveT = 0f;
            bonkIn = FirstBonk;
            pendingBonk = pendingScreen = pendingMask = false;
            Taps = Swats = CakeSmashes = Sprays = Covers = Rattles = Knocks = Bonks = LeaveCancels = 0;
            CompletionReported = false;
        }

        /// <summary>Il tempo che passa: maschera, nebbia e passeggiata sul vetro finiscono, la mosca sbatte contro la
        /// finestra a intervalli, e se la finestra è aperta va verso il varco ed esce.</summary>
        public FlyEvent Tick(float dt)
        {
            if (InputLocked) return FlyEvent.None;
            if (float.IsNaN(dt) || dt < 0f) dt = 0f;
            dt = Math.Min(dt, MaxStep);
            if (MaskLeft > 0f) { MaskLeft = Math.Max(0f, MaskLeft - dt); if (MaskLeft <= 0f) pendingMask = true; }
            if (FogLeft > 0f) FogLeft = Math.Max(0f, FogLeft - dt);
            if (ScreenLeft > 0f) { ScreenLeft = Math.Max(0f, ScreenLeft - dt); if (ScreenLeft <= 0f) pendingScreen = true; }
            if (Stage == FlyStage.Leaving)
            {
                pendingBonk = false;
                float before = leaveT;
                leaveT += dt;
                if (leaveT >= LeaveSeconds)
                {
                    Stage = FlyStage.Outside;
                    MaskLeft = ScreenLeft = 0f;
                    pendingBonk = pendingScreen = pendingMask = false;
                    return FlyEvent.FlewOut;
                }
                if (before < NoticeSeconds && leaveT >= NoticeSeconds) return FlyEvent.NoticedWindow;
            }
            else if (!OnScreen)
            {
                bonkIn -= dt;
                if (bonkIn <= 0f) { bonkIn = BonkPeriod; pendingBonk = true; }
            }
            if (pendingScreen) { pendingScreen = false; return FlyEvent.ScreenWalkOver; }
            if (pendingMask) { pendingMask = false; return FlyEvent.MaskOff; }
            if (pendingBonk && Stage == FlyStage.Buzzing && !OnScreen) { pendingBonk = false; Bonks++; return FlyEvent.Bonk; }
            return FlyEvent.None;
        }

        // ---------- Schivare (sempre) ----------

        /// <summary>Un dito (o la paletta) che arriva in (tx,ty) è abbastanza vicino alla mosca da farla scappare?</summary>
        public static bool Threatens(float tx, float ty, float flyX, float flyY, float radius = DodgeRadius)
        {
            float dx = flyX - tx, dy = flyY - ty;
            return dx * dx + dy * dy <= radius * radius;
        }

        /// <summary>
        /// Dove scappa la mosca quando un dito arriva in (tx,ty): lontano dal dito, dentro l'area di volo, sempre ad almeno
        /// MinDodge dal punto toccato (se il bordo la blocca prova altre direzioni; in ultimo l'angolo più lontano).
        /// </summary>
        public static void Dodge(float tx, float ty, float flyX, float flyY, out float toX, out float toY)
        {
            if (float.IsNaN(tx) || float.IsNaN(ty)) { tx = flyX; ty = flyY - 1f; }
            if (float.IsNaN(flyX) || float.IsNaN(flyY)) { flyX = 0f; flyY = 0f; }
            float dx = flyX - tx, dy = flyY - ty;
            float len = (float)Math.Sqrt(dx * dx + dy * dy);
            if (len < 0.05f)
            {
                // il dito è proprio sopra: scappa verso il centro della stanza (o in su)
                dx = (MinX + MaxX) * 0.5f - tx; dy = (MinY + MaxY) * 0.5f - ty;
                len = (float)Math.Sqrt(dx * dx + dy * dy);
                if (len < 0.05f) { dx = 0f; dy = 1f; len = 1f; }
            }
            dx /= len; dy /= len;
            float[] turns = { 0f, 40f, -40f, 80f, -80f, 120f, -120f, 160f, -160f, 180f };
            foreach (float deg in turns)
            {
                double a = deg * Math.PI / 180.0;
                float rx = (float)(dx * Math.Cos(a) - dy * Math.Sin(a));
                float ry = (float)(dx * Math.Sin(a) + dy * Math.Cos(a));
                float cx = Clamp(flyX + rx * DodgeStep, MinX, MaxX);
                float cy = Clamp(flyY + ry * DodgeStep, MinY, MaxY);
                if (Dist(cx, cy, tx, ty) >= MinDodge) { toX = cx; toY = cy; return; }
            }
            // in ultimo: l'angolo dell'area più lontano dal dito (l'area è molto più grande di MinDodge)
            toX = Math.Abs(tx - MinX) > Math.Abs(tx - MaxX) ? MinX : MaxX;
            toY = Math.Abs(ty - MinY) > Math.Abs(ty - MaxY) ? MinY : MaxY;
        }

        static float Clamp(float v, float lo, float hi) => v < lo ? lo : v > hi ? hi : v;
        static float Dist(float ax, float ay, float bx, float by) => (float)Math.Sqrt((ax - bx) * (ax - bx) + (ay - by) * (ay - by));

        // ---------- Le false piste ----------

        /// <summary>Un dito sulla mosca: schiva (anche mentre esce o passeggia sul vetro). 0 se l'input è bloccato.</summary>
        public int TapFly()
        {
            if (InputLocked) return 0;
            if (OnScreen) { ScreenLeft = 0f; pendingScreen = true; }   // dal vetro torna a volare in cucina
            return ++Taps;
        }

        /// <summary>Un colpo di paletta: la mosca schiva sempre; se la paletta cade sulla torta, la torta si spiaccica.</summary>
        public SwatResult Swat(bool onCake)
        {
            if (Stage != FlyStage.Buzzing) return SwatResult.None;
            Swats++;
            if (!onCake) return SwatResult.Miss;
            CakeSmashes++;
            return SwatResult.SmashedCake;
        }

        /// <summary>Lo spray: la mosca si mette la maschera antigas, la stanza si annebbia per un po'. 0 se bloccato.</summary>
        public int Spray()
        {
            if (Stage != FlyStage.Buzzing) return 0;
            MaskLeft = MaskSeconds;
            FogLeft = FogSeconds;
            pendingMask = false;
            return ++Sprays;
        }

        /// <summary>La torta coperta col dito: la mosca si posa sul vetro del telefono per un po'. 0 se c'è già o bloccato.</summary>
        public int CoverCake()
        {
            if (Stage != FlyStage.Buzzing || OnScreen) return 0;
            ScreenLeft = ScreenSeconds;
            pendingScreen = false;
            return ++Covers;
        }

        /// <summary>Nocche sul vetro della finestra: la mosca arriva presto a sbatterci contro anche lei.</summary>
        public int KnockGlass()
        {
            if (Stage != FlyStage.Buzzing) return 0;
            bonkIn = Math.Min(bonkIn, KnockBonk);
            return ++Knocks;
        }

        // ---------- La finestra ----------

        /// <summary>Il gancio toccato: sgancia/riaggancia (solo a finestra chiusa).</summary>
        public LatchResult ToggleLatch()
        {
            if (InputLocked) return LatchResult.None;
            if (Opening > 0.001f) return LatchResult.Blocked;
            Latched = !Latched;
            return Latched ? LatchResult.Latched : LatchResult.Unlatched;
        }

        /// <summary>Si prova ad alzare il vetro mentre è agganciato: sbatacchia. True se conta come tentativo.</summary>
        public bool Rattle()
        {
            if (InputLocked || !Latched) return false;
            Rattles++;
            return true;
        }

        /// <summary>
        /// Il vetro scorrevole portato a "amount" (0..1). Agganciato non si muove. Aperto abbastanza la mosca parte;
        /// richiuso prima che sia uscita, la mosca resta dentro e torna a ronzare (si può riaprire quando si vuole).
        /// </summary>
        public WindowEvent SetOpening(float amount)
        {
            if (InputLocked) return WindowEvent.None;
            if (float.IsNaN(amount)) amount = 0f;
            amount = Clamp(amount, 0f, 1f);
            if (Latched) { Opening = 0f; return amount > 0.001f ? WindowEvent.Blocked : WindowEvent.None; }
            Opening = amount;
            if (Stage == FlyStage.Buzzing && WindowOpen)
            {
                Stage = FlyStage.Leaving;
                leaveT = 0f;
                ScreenLeft = 0f;            // dal vetro del telefono va dritta alla finestra
                pendingScreen = pendingBonk = false;
                return WindowEvent.StartedLeaving;
            }
            if (Stage == FlyStage.Leaving && !WindowOpen)
            {
                Stage = FlyStage.Buzzing;
                leaveT = 0f;
                LeaveCancels++;
                bonkIn = BonkPeriod;
                return WindowEvent.LeaveCancelled;
            }
            return WindowEvent.None;
        }

        /// <summary>Fine dei saluti: vittoria, una volta sola.</summary>
        public bool TryComplete()
        {
            if (Stage != FlyStage.Outside || CompletionReported) return false;
            CompletionReported = true;
            Stage = FlyStage.Done;
            return true;
        }
    }
}
