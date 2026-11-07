namespace Gotcha.Levels.V64
{
    /// <summary>Fasi del livello "The Fireless Dragon".</summary>
    public enum FirelessDragonStage
    {
        Playing,      // si prova di tutto
        Celebrating,  // fiammella inghiottita: fiammata e festa (input bloccato)
        Done,         // vittoria già data
    }

    /// <summary>Dov'è la fiammella trascinabile.</summary>
    public enum FirelessDragonFlame
    {
        OnFire,    // sospesa sopra il falò, si può afferrare
        Held,      // nel dito
        Fizzling,  // lasciata altrove: si spegne, poi ne ricompare una sul falò
        Eaten,     // inghiottita dal drago (vittoria)
    }

    /// <summary>Cosa è successo dopo un input o un passo di tempo.</summary>
    public enum FirelessDragonEvent
    {
        None,       // niente da mostrare
        Ignored,    // input dopo la vittoria
        Bubbles,    // tocco sul drago: solo altre bolle di sapone
        Popped,     // tocco su una bolla: "pop" (solo estetica, non conta come tentativo)
        Cough,      // telefono scosso: il drago tossisce (sbuffo di fumo grigio, "*cough*")
        Picked,     // fiammella afferrata dal falò
        Returned,   // fiammella lasciata vicino al falò (o dito annullato): torna su, nessun commento
        Fizzled,    // fiammella lasciata altrove: si spegne (tentativo sbagliato)
        Singed,     // fiammella lasciata sul corpo del drago ma non in bocca: "Ow!" e si spegne
        Respawned,  // dopo lo spegnimento ne ricompare una nuova sul falò
        Absorbed,   // fiammella in bocca al drago: VITTORIA (una volta sola)
    }

    /// <summary>
    /// Logica pura del livello 119 della roadmap "The Fireless Dragon" (id fireless_dragon) — "LIGHT THE DRAGON'S FIRE."
    /// Un draghetto colorato soffia solo bolle di sapone; accanto c'è un piccolo falò acceso. Inganno: toccare il drago
    /// fa solo altre bolle (<see cref="FirelessDragonEvent.Bubbles"/>), scuotere il telefono lo fa tossire
    /// (<see cref="FirelessDragonEvent.Cough"/>, con una pausa minima <see cref="CoughCooldown"/> fra due colpi di tosse).
    /// Soluzione esatta: afferrare la fiammella sopra il falò e portarla nella bocca del drago (cerchio di centro
    /// mouthX/mouthY e raggio mouthRadius, generoso): basta entrarci trascinando, oppure lasciarla lì (vale anche il dito,
    /// il più generoso dei due). La fiammella lasciata altrove si spegne e dopo <see cref="RespawnTime"/> s ne ricompare
    /// una nuova sul falò (reversibile, mai una sconfitta); lasciata vicino al falò (entro fireRadius) torna su e basta.
    /// Cancel() (pausa, perdita del fuoco, secondo dito) rimette la fiammella sul falò senza contare niente.
    /// Nessuna dipendenza da Unity.
    /// </summary>
    public sealed class FirelessDragonLogic
    {
        public const float CoughCooldown = 1.0f;  // s fra due colpi di tosse (lo scuotimento dura molti fotogrammi)
        public const float RespawnTime = 0.6f;    // s dallo spegnimento alla fiammella nuova sul falò
        public const int HintAfter = 5;           // tentativi sbagliati dopo cui arriva la frase sottile

        readonly float mouthX, mouthY, mouthR2, fireX, fireY, fireR2;
        float coughWait, respawnT;
        bool completed, hintSaid;

        public FirelessDragonLogic(float mouthX, float mouthY, float mouthRadius, float fireX, float fireY, float fireRadius)
        {
            this.mouthX = mouthX;
            this.mouthY = mouthY;
            mouthR2 = mouthRadius * mouthRadius;
            this.fireX = fireX;
            this.fireY = fireY;
            fireR2 = fireRadius * fireRadius;
            Reset();
        }

        public FirelessDragonStage Stage { get; private set; }
        public FirelessDragonFlame Flame { get; private set; }
        public bool InputLocked => Stage != FirelessDragonStage.Playing;
        public bool CanPick => Stage == FirelessDragonStage.Playing && Flame == FirelessDragonFlame.OnFire;
        public int Taps { get; private set; }
        public int Coughs { get; private set; }
        public int Fizzles { get; private set; }
        public int Pops { get; private set; }
        /// <summary>Tentativi sbagliati (tocchi sul drago, tosse, fiammelle spente).</summary>
        public int WrongMoves { get; private set; }
        /// <summary>0..1 durante lo spegnimento (per l'animazione), 0 altrimenti.</summary>
        public float FizzleProgress => Flame == FirelessDragonFlame.Fizzling ? Clamp01(respawnT / RespawnTime) : 0f;

        /// <summary>Si ricomincia da capo (rientro nel livello).</summary>
        public void Reset()
        {
            Stage = FirelessDragonStage.Playing;
            Flame = FirelessDragonFlame.OnFire;
            coughWait = respawnT = 0f;
            completed = hintSaid = false;
            Taps = Coughs = Fizzles = Pops = WrongMoves = 0;
        }

        /// <summary>Pausa, app in sottofondo, secondo dito: la fiammella torna sul falò (anche se si stava spegnendo).</summary>
        public FirelessDragonEvent Cancel()
        {
            if (Stage != FirelessDragonStage.Playing) return FirelessDragonEvent.None;
            if (Flame == FirelessDragonFlame.OnFire) return FirelessDragonEvent.None;
            Flame = FirelessDragonFlame.OnFire;
            respawnT = 0f;
            return FirelessDragonEvent.Returned;
        }

        public bool InMouth(float x, float y) => Sq(x - mouthX) + Sq(y - mouthY) <= mouthR2;
        public bool NearFire(float x, float y) => Sq(x - fireX) + Sq(y - fireY) <= fireR2;

        /// <summary>Passo di tempo: pausa della tosse e ricomparsa della fiammella.</summary>
        public FirelessDragonEvent Tick(float dt)
        {
            if (dt < 0f || float.IsNaN(dt) || float.IsInfinity(dt)) dt = 0f;
            if (coughWait > 0f) coughWait -= dt;
            if (Stage != FirelessDragonStage.Playing || Flame != FirelessDragonFlame.Fizzling) return FirelessDragonEvent.None;
            respawnT += dt;
            if (respawnT < RespawnTime - 1e-4f) return FirelessDragonEvent.None;
            Flame = FirelessDragonFlame.OnFire;
            respawnT = 0f;
            return FirelessDragonEvent.Respawned;
        }

        /// <summary>Tocco sul drago: solo bolle.</summary>
        public FirelessDragonEvent TapDragon()
        {
            if (Stage != FirelessDragonStage.Playing) return FirelessDragonEvent.Ignored;
            Taps++;
            WrongMoves++;
            return FirelessDragonEvent.Bubbles;
        }

        /// <summary>Tocco su una bolla: scoppia (non è un tentativo).</summary>
        public FirelessDragonEvent PopBubble()
        {
            if (Stage != FirelessDragonStage.Playing) return FirelessDragonEvent.Ignored;
            Pops++;
            return FirelessDragonEvent.Popped;
        }

        /// <summary>Telefono scosso (oltre la soglia del gioco): tosse, al massimo una ogni <see cref="CoughCooldown"/> s.</summary>
        public FirelessDragonEvent Shake()
        {
            if (Stage != FirelessDragonStage.Playing) return FirelessDragonEvent.Ignored;
            if (coughWait > 0f) return FirelessDragonEvent.None;
            coughWait = CoughCooldown;
            Coughs++;
            WrongMoves++;
            return FirelessDragonEvent.Cough;
        }

        /// <summary>Il dito prende la fiammella sul falò. None se non c'è (si sta spegnendo) o se il livello è finito.</summary>
        public FirelessDragonEvent Pick()
        {
            if (!CanPick) return FirelessDragonEvent.None;
            Flame = FirelessDragonFlame.Held;
            return FirelessDragonEvent.Picked;
        }

        /// <summary>La fiammella tenuta si sposta in (x, y): entrata in bocca, viene inghiottita subito.</summary>
        public FirelessDragonEvent Move(float x, float y)
        {
            if (Stage != FirelessDragonStage.Playing || Flame != FirelessDragonFlame.Held) return FirelessDragonEvent.None;
            return InMouth(x, y) ? Absorb() : FirelessDragonEvent.None;
        }

        /// <summary>
        /// La fiammella viene lasciata: (x, y) posizione della fiammella, (fx, fy) del dito (vale il più generoso);
        /// onDragon: lasciata sul corpo del drago (fuori dalla bocca).
        /// </summary>
        public FirelessDragonEvent Release(float x, float y, float fx, float fy, bool onDragon)
        {
            if (Stage != FirelessDragonStage.Playing) return FirelessDragonEvent.Ignored;
            if (Flame != FirelessDragonFlame.Held) return FirelessDragonEvent.None;
            if (InMouth(x, y) || InMouth(fx, fy)) return Absorb();
            if (NearFire(x, y))
            {
                Flame = FirelessDragonFlame.OnFire;
                return FirelessDragonEvent.Returned;
            }
            Flame = FirelessDragonFlame.Fizzling;
            respawnT = 0f;
            Fizzles++;
            WrongMoves++;
            return onDragon ? FirelessDragonEvent.Singed : FirelessDragonEvent.Fizzled;
        }

        FirelessDragonEvent Absorb()
        {
            Flame = FirelessDragonFlame.Eaten;
            Stage = FirelessDragonStage.Celebrating;
            return FirelessDragonEvent.Absorbed;
        }

        /// <summary>La frase sottile dopo <see cref="HintAfter"/> tentativi sbagliati: true una volta sola.</summary>
        public bool TakeHint()
        {
            if (hintSaid || WrongMoves < HintAfter || Stage != FirelessDragonStage.Playing) return false;
            hintSaid = true;
            return true;
        }

        /// <summary>Vittoria da registrare: true una volta sola, dopo che il drago ha inghiottito la fiammella.</summary>
        public bool TryComplete()
        {
            if (Stage != FirelessDragonStage.Celebrating || completed) return false;
            completed = true;
            Stage = FirelessDragonStage.Done;
            return true;
        }

        static float Sq(float v) => v * v;
        static float Clamp01(float v) => v < 0f ? 0f : v > 1f ? 1f : v;
    }
}
