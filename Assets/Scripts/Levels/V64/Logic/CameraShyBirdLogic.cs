using System;

namespace Gotcha.Levels.V64
{
    /// <summary>Fasi del livello 264.</summary>
    public enum ShyStage
    {
        Hunting,   // si gira per il parco con la fotocamera (posteriore o selfie)
        Snapped,   // foto col fotobomba: polaroid finale, input bloccato
        Done,
    }

    /// <summary>Quale fotocamera è attiva.</summary>
    public enum ShyFacing { Rear, Selfie }

    /// <summary>Cosa fa l'uccellino quando la fotocamera guarda il parco (posteriore).</summary>
    public enum BirdMood
    {
        Peeking,   // posato: sbircia curioso verso l'obiettivo (e lo segue se lo si punta altrove)
        Hopping,   // saltella verso il bordo dell'inquadratura (segue la fotocamera)
        Staring,   // si è accorto dell'obiettivo puntato su di lui: lo fissa, poi vola via
        Fleeing,   // vola via fuori dall'inquadratura
        Hidden,    // nascosto tra le foglie (fuori inquadratura), poi torna a sbirciare
        Covering,  // zoom addosso: si copre la faccia con l'ala ("No paparazzi.")
    }

    /// <summary>L'uccellino nella modalità selfie (non è più lui il soggetto: viene a fare il fotobomba).</summary>
    public enum SelfieBird
    {
        Away,      // fuori campo, sta per arrivare
        Arriving,  // vola verso la testa dell'avatar
        Posing,    // posato sulla testa, fa la V con l'ala (resta qualche secondo)
        Leaving,   // vola via (e poi torna)
    }

    /// <summary>Cosa è appena successo (il livello sceglie scritta, suono e animazione).</summary>
    public enum ShyEvent
    {
        None,
        Followed,     // l'inquadratura è andata altrove: l'uccellino saltella dietro alla fotocamera
        Landed,       // fine del saltello: di nuovo a sbirciare dal bordo
        Noticed,      // inquadrato al centro: fissa l'obiettivo
        Fled,         // vola via
        Hid,          // arrivato tra le foglie
        Reappeared,   // esce dalle foglie e torna a sbirciare
        CoveredFace,  // zoom addosso: si copre la faccia con l'ala
        Uncovered,    // zoom tolto (o inquadratura spostata): giù l'ala
        SelfieArrive, // selfie: arriva in volo
        SelfieLand,   // selfie: si posa sulla testa e fa la V
        SelfieLeave,  // selfie: riparte
        SelfieGone,   // selfie: è uscito di campo (tornerà)
    }

    /// <summary>Che foto è venuta.</summary>
    public enum ShyPhoto
    {
        None,
        Scenery,      // nessun uccellino nella foto: "Lovely branch." (o panchina, lampione...)
        Dodged,       // l'uccellino era inquadrato: ha sentito lo scatto ed è schizzato via (foto mossa)
        Wing,         // si copriva la faccia: la foto di un'ala
        Glare,        // col flash: solo riflesso (e l'uccellino ha gli occhiali da sole)
        SelfieAlone,  // selfie senza uccellino
        Photobomb,    // selfie con l'uccellino sulla testa: vittoria
    }

    /// <summary>
    /// Logica pura del livello 264 "The Camera-Shy Bird" — "Take a photo of the bird." Lo schermo è l'app della
    /// fotocamera: un panorama del parco che si sposta (Pan) e si ingrandisce (Zoom). L'uccellino odia essere il soggetto:
    /// inquadrato al centro fissa l'obiettivo e vola via tra le foglie; con lo zoom addosso si copre la faccia; col flash si
    /// mette gli occhiali da sole (e la foto è solo riflesso); se si scatta mentre è inquadrato schizza via. Indizio: quando
    /// la fotocamera guarda altrove, l'uccellino la segue saltellando e sbircia dal bordo dell'inquadratura.
    /// Soluzione: girare la fotocamera (selfie): non più puntato, l'uccellino viene a posarsi sulla testa dell'avatar e fa
    /// il fotobomba; scattare mentre è in posa (resta qualche secondo, tolleranza anche all'arrivo e alla partenza; se lo si
    /// manca torna). Nessuna mossa sbagliata fa perdere qualcosa. Coordinate: unità del panorama (= mondo a zoom 1), x
    /// rispetto al centro del mirino. Nessuna dipendenza da Unity.
    /// </summary>
    public sealed class CameraShyBirdLogic
    {
        // ---------- Regolazioni ----------
        /// <summary>Metà larghezza dell'inquadratura a zoom 1 (un po' dentro il bordo dello schermo).</summary>
        public float HalfWidth = 2.5f;
        /// <summary>Dove può posarsi l'uccellino lungo la siepe e i rami.</summary>
        public float PerchMin = -6.4f, PerchMax = 6.4f;
        /// <summary>Quanto si può spostare l'inquadratura (il panorama non finisce mai sullo schermo).</summary>
        public float PanMax = 4.2f;
        public float ZoomMin = 1f, ZoomMax = 3f;
        /// <summary>Uccellino entro questa frazione dal centro = puntato (se ne accorge).</summary>
        public float AimRadius = 0.42f;
        /// <summary>Entro questa frazione è nell'inquadratura (finisce nella foto).</summary>
        public float FrameEdge = 0.95f;
        /// <summary>Dove si mette a sbirciare quando segue la fotocamera (frazione dal centro).</summary>
        public float PeekEdge = 0.75f;
        /// <summary>Oltre questa frazione è fuori campo: dopo FollowDelay salta dietro all'inquadratura.</summary>
        public float OutEdge = 1.05f;
        /// <summary>Dove va a nascondersi quando scappa (frazione dal centro, fuori campo).</summary>
        public float FleeEdge = 1.45f;
        public float FollowDelay = 0.5f;
        public float HopSeconds = 0.45f;
        public float StareSeconds = 0.6f;
        public float FleeSeconds = 0.8f;
        public float HideSeconds = 1.2f;
        /// <summary>Zoom oltre cui, inquadrato, si copre la faccia (isteresi: la abbassa sotto ZoomUncover).</summary>
        public float ZoomCover = 1.8f, ZoomUncover = 1.5f;
        /// <summary>Coperto troppo a lungo: si stufa e vola via.</summary>
        public float CoverMaxSeconds = 3f;
        public float ZoomStepSize = 0.5f;
        // selfie
        public float ArriveDelay = 0.9f, ArriveAgainDelay = 1.5f;
        public float ArriveSeconds = 0.8f, PoseSeconds = 4f, LeaveSeconds = 0.6f;
        /// <summary>Frazione del volo d'arrivo oltre la quale lo scatto vale già (è praticamente in campo).</summary>
        public float ArriveGrace = 0.5f;
        /// <summary>Secondi dopo la ripartenza in cui lo scatto vale ancora.</summary>
        public float LeaveGrace = 0.35f;

        /// <summary>Inquadratura e uccellino all'inizio: si guarda la panchina, l'uccellino sbircia dal ramo sul bordo destro.</summary>
        public const float StartPan = -3.4f, StartBirdX = -1.5f;

        // ---------- Stato ----------
        public ShyStage Stage { get; private set; } = ShyStage.Hunting;
        public ShyFacing Facing { get; private set; } = ShyFacing.Rear;
        public BirdMood Mood { get; private set; } = BirdMood.Peeking;
        public SelfieBird Selfie { get; private set; } = SelfieBird.Away;
        public float Pan { get; private set; } = StartPan;
        public float Zoom { get; private set; } = 1f;
        public bool FlashOn { get; private set; }
        /// <summary>Posizione attuale dell'uccellino lungo il panorama (anche durante salti e voli).</summary>
        public float BirdX { get; private set; } = StartBirdX;
        public float FromX { get; private set; } = StartBirdX;
        public float ToX { get; private set; } = StartBirdX;
        /// <summary>Secondi passati nell'umore attuale (posteriore).</summary>
        public float MoodTime { get; private set; }
        /// <summary>Secondi passati nella fase attuale del selfie.</summary>
        public float SelfieTime { get; private set; }
        public bool SelfieVisited { get; private set; }

        public int Photos { get; private set; }
        public int Flips { get; private set; }
        public int Notices { get; private set; }
        public int Covers { get; private set; }
        public int Follows { get; private set; }
        public int FlashToggles { get; private set; }
        public int Dodges { get; private set; }
        public int Glares { get; private set; }
        public int Sceneries { get; private set; }
        public int SelfiesAlone { get; private set; }
        public int Arrivals { get; private set; }
        public bool CompletionReported { get; private set; }

        float followT;
        bool arrivedOnce;

        public bool InputLocked => Stage != ShyStage.Hunting;
        public bool Solved => Stage != ShyStage.Hunting;

        /// <summary>Posizione dell'uccellino nell'inquadratura: 0 = centro, ±1 = bordo (frazione di HalfWidth).</summary>
        public float BirdFrame => FrameOf(BirdX);
        public float FrameOf(float x) => (x - Pan) * Zoom / HalfWidth;
        /// <summary>Puntato: al centro dell'inquadratura posteriore.</summary>
        public bool Aimed => Facing == ShyFacing.Rear && Math.Abs(BirdFrame) < AimRadius;
        /// <summary>Visibile nell'inquadratura posteriore (non nascosto tra le foglie).</summary>
        public bool BirdInFrame => Facing == ShyFacing.Rear && Mood != BirdMood.Hidden && Math.Abs(BirdFrame) < FrameEdge;
        /// <summary>0..1 del salto o del volo in corso (per l'animazione).</summary>
        public float TravelProgress
        {
            get
            {
                if (Mood == BirdMood.Hopping) return Math.Min(1f, MoodTime / HopSeconds);
                if (Mood == BirdMood.Fleeing) return Math.Min(1f, MoodTime / FleeSeconds);
                return 1f;
            }
        }
        /// <summary>0..1 del volo d'arrivo o di partenza nel selfie.</summary>
        public float SelfieProgress
        {
            get
            {
                if (Selfie == SelfieBird.Arriving) return Math.Min(1f, SelfieTime / ArriveSeconds);
                if (Selfie == SelfieBird.Leaving) return Math.Min(1f, SelfieTime / LeaveSeconds);
                return Selfie == SelfieBird.Posing ? 1f : 0f;
            }
        }
        /// <summary>Selfie: l'uccellino è (abbastanza) in campo perché lo scatto lo prenda.</summary>
        public bool BirdInSelfie
        {
            get
            {
                if (Facing != ShyFacing.Selfie) return false;
                switch (Selfie)
                {
                    case SelfieBird.Posing: return true;
                    case SelfieBird.Arriving: return SelfieTime >= ArriveSeconds * ArriveGrace;
                    case SelfieBird.Leaving: return SelfieTime <= LeaveGrace;
                    default: return false;
                }
            }
        }
        public int WrongMoves => Notices + Covers + Dodges + Glares + Sceneries + SelfiesAlone;

        public void Reset()
        {
            Stage = ShyStage.Hunting;
            Facing = ShyFacing.Rear;
            Mood = BirdMood.Peeking;
            Selfie = SelfieBird.Away;
            Pan = StartPan;
            Zoom = 1f;
            FlashOn = false;
            BirdX = FromX = ToX = StartBirdX;
            MoodTime = SelfieTime = 0f;
            SelfieVisited = false;
            Photos = Flips = Notices = Covers = Follows = FlashToggles = Dodges = Glares = Sceneries = SelfiesAlone = Arrivals = 0;
            CompletionReported = false;
            followT = 0f;
            arrivedOnce = false;
        }

        // ---------- Inquadratura ----------

        /// <summary>Sposta l'inquadratura di dx unità del panorama (solo fotocamera posteriore). True se si è mossa.</summary>
        public bool PanBy(float dx)
        {
            if (InputLocked || Facing != ShyFacing.Rear || float.IsNaN(dx) || float.IsInfinity(dx)) return false;
            float p = Clamp(Pan + dx, -PanMax, PanMax);
            if (Math.Abs(p - Pan) < 1e-6f) return false;
            Pan = p;
            return true;
        }

        /// <summary>Imposta lo zoom (pizzico). Solo fotocamera posteriore: il selfie è sempre 1x. True se è cambiato.</summary>
        public bool SetZoom(float z)
        {
            if (InputLocked || Facing != ShyFacing.Rear || float.IsNaN(z) || float.IsInfinity(z)) return false;
            z = Clamp(z, ZoomMin, ZoomMax);
            if (Math.Abs(z - Zoom) < 1e-6f) return false;
            Zoom = z;
            return true;
        }

        /// <summary>Pulsanti +/- (alternativa al pizzico): un gradino di zoom.</summary>
        public bool ZoomStep(int dir) => dir != 0 && SetZoom(Zoom + Math.Sign(dir) * ZoomStepSize);

        /// <summary>Il flash (vale in tutte e due le fotocamere). Restituisce il nuovo stato.</summary>
        public bool ToggleFlash()
        {
            if (InputLocked) return FlashOn;
            FlashToggles++;
            FlashOn = !FlashOn;
            return FlashOn;
        }

        /// <summary>Gira la fotocamera (posteriore ↔ selfie). Pan e flash restano; lo zoom torna 1x. True se si è girata.</summary>
        public bool Flip()
        {
            if (InputLocked) return false;
            Flips++;
            Zoom = 1f;
            followT = 0f;
            if (Facing == ShyFacing.Rear)
            {
                Facing = ShyFacing.Selfie;
                Selfie = SelfieBird.Away;
                SelfieTime = 0f;
                SelfieVisited = true;
                arrivedOnce = false;
            }
            else
            {
                // di nuovo puntato dalla fotocamera posteriore: è subito timido, si rimette tra le foglie fuori campo
                Facing = ShyFacing.Rear;
                Selfie = SelfieBird.Away;
                SelfieTime = 0f;
                float x = FleeTarget();
                BirdX = FromX = ToX = x;
                Mood = BirdMood.Hidden;
                MoodTime = 0f;
            }
            return true;
        }

        /// <summary>Tocco sul mirino per mettere a fuoco proprio l'uccellino: se ne accorge (se stava sbirciando in campo).</summary>
        public bool FocusOnBird()
        {
            if (InputLocked || Facing != ShyFacing.Rear || Mood != BirdMood.Peeking || !BirdInFrame) return false;
            Notices++;
            SetMood(BirdMood.Staring);
            return true;
        }

        // ---------- Il tempo che passa ----------

        public ShyEvent Tick(float dt)
        {
            if (InputLocked || float.IsNaN(dt) || dt <= 0f) return ShyEvent.None;
            if (dt > 0.25f) dt = 0.25f;   // fotogramma lunghissimo (es. ritorno dalla pausa): niente salti di stato
            return Facing == ShyFacing.Selfie ? TickSelfie(dt) : TickRear(dt);
        }

        ShyEvent TickRear(float dt)
        {
            MoodTime += dt;
            float f = BirdFrame, af = Math.Abs(f);
            switch (Mood)
            {
                case BirdMood.Hopping:
                    BirdX = Lerp(FromX, ToX, TravelProgress);
                    if (MoodTime < HopSeconds) return ShyEvent.None;
                    BirdX = ToX;
                    SetMood(BirdMood.Peeking);
                    return ShyEvent.Landed;
                case BirdMood.Fleeing:
                    BirdX = Lerp(FromX, ToX, TravelProgress);
                    if (MoodTime < FleeSeconds) return ShyEvent.None;
                    BirdX = ToX;
                    SetMood(BirdMood.Hidden);
                    return ShyEvent.Hid;
                case BirdMood.Hidden:
                    if (MoodTime < HideSeconds) return ShyEvent.None;
                    SetMood(BirdMood.Peeking);
                    return ShyEvent.Reappeared;
                case BirdMood.Staring:
                    if (Zoom >= ZoomCover && af < FrameEdge)
                    {
                        Covers++;
                        SetMood(BirdMood.Covering);
                        return ShyEvent.CoveredFace;
                    }
                    if (MoodTime < StareSeconds) return ShyEvent.None;
                    StartFlee();
                    return ShyEvent.Fled;
                case BirdMood.Covering:
                    if (MoodTime >= CoverMaxSeconds) { StartFlee(); return ShyEvent.Fled; }
                    if (Zoom >= ZoomUncover && af < FrameEdge) return ShyEvent.None;
                    SetMood(BirdMood.Peeking);
                    return ShyEvent.Uncovered;
                default: // Peeking
                    if (Zoom >= ZoomCover && af < FrameEdge)
                    {
                        Covers++;
                        SetMood(BirdMood.Covering);
                        return ShyEvent.CoveredFace;
                    }
                    if (af < AimRadius)
                    {
                        Notices++;
                        SetMood(BirdMood.Staring);
                        return ShyEvent.Noticed;
                    }
                    if (af > OutEdge && Zoom < ZoomCover)
                    {
                        followT += dt;
                        if (followT < FollowDelay) return ShyEvent.None;
                        followT = 0f;
                        float side = f >= 0f ? 1f : -1f;
                        float target = Clamp(Pan + side * PeekEdge * HalfWidth / Zoom, PerchMin, PerchMax);
                        if (Math.Abs(FrameOf(target)) < AimRadius + 0.1f || Math.Abs(target - BirdX) < 0.05f) return ShyEvent.None;
                        Follows++;
                        FromX = BirdX;
                        ToX = target;
                        SetMood(BirdMood.Hopping);
                        return ShyEvent.Followed;
                    }
                    followT = 0f;
                    return ShyEvent.None;
            }
        }

        ShyEvent TickSelfie(float dt)
        {
            SelfieTime += dt;
            switch (Selfie)
            {
                case SelfieBird.Away:
                    if (SelfieTime < (arrivedOnce ? ArriveAgainDelay : ArriveDelay)) return ShyEvent.None;
                    Arrivals++;
                    arrivedOnce = true;
                    SetSelfie(SelfieBird.Arriving);
                    return ShyEvent.SelfieArrive;
                case SelfieBird.Arriving:
                    if (SelfieTime < ArriveSeconds) return ShyEvent.None;
                    SetSelfie(SelfieBird.Posing);
                    return ShyEvent.SelfieLand;
                case SelfieBird.Posing:
                    if (SelfieTime < PoseSeconds) return ShyEvent.None;
                    SetSelfie(SelfieBird.Leaving);
                    return ShyEvent.SelfieLeave;
                default: // Leaving
                    if (SelfieTime < LeaveSeconds) return ShyEvent.None;
                    SetSelfie(SelfieBird.Away);
                    return ShyEvent.SelfieGone;
            }
        }

        // ---------- Lo scatto ----------

        /// <summary>Il pulsante di scatto. Le foto sbagliate non costano niente; il fotobomba nel selfie vince (una volta).</summary>
        public ShyPhoto Shutter()
        {
            if (InputLocked) return ShyPhoto.None;
            Photos++;
            if (Facing == ShyFacing.Selfie)
            {
                if (BirdInSelfie)
                {
                    Stage = ShyStage.Snapped;
                    return ShyPhoto.Photobomb;
                }
                SelfiesAlone++;
                return ShyPhoto.SelfieAlone;
            }
            if (FlashOn) { Glares++; return ShyPhoto.Glare; }
            if (Mood == BirdMood.Covering && Math.Abs(BirdFrame) < FrameEdge) { Dodges++; return ShyPhoto.Wing; }
            if (BirdInFrame)
            {
                Dodges++;
                if (Mood != BirdMood.Fleeing) StartFlee();
                return ShyPhoto.Dodged;
            }
            Sceneries++;
            return ShyPhoto.Scenery;
        }

        /// <summary>Fine della polaroid: vittoria, una volta sola.</summary>
        public bool TryComplete()
        {
            if (Stage != ShyStage.Snapped || CompletionReported) return false;
            CompletionReported = true;
            Stage = ShyStage.Done;
            return true;
        }

        // ---------- Aiuti ----------

        void StartFlee()
        {
            FromX = BirdX;
            ToX = FleeTarget();
            SetMood(BirdMood.Fleeing);
        }

        /// <summary>Un posto fuori campo dalla parte dove già si trova l'uccellino (o dall'altra, se lì finisce il parco).</summary>
        float FleeTarget()
        {
            float side = BirdX >= Pan ? 1f : -1f;
            float reach = FleeEdge * HalfWidth / Zoom;
            float a = Clamp(Pan + side * reach, PerchMin, PerchMax);
            if (Math.Abs(FrameOf(a)) >= OutEdge) return a;
            float b = Clamp(Pan - side * reach, PerchMin, PerchMax);
            if (Math.Abs(FrameOf(b)) >= OutEdge) return b;
            return Math.Abs(FrameOf(a)) >= Math.Abs(FrameOf(b)) ? a : b;
        }

        void SetMood(BirdMood m)
        {
            Mood = m;
            MoodTime = 0f;
            followT = 0f;
        }

        void SetSelfie(SelfieBird s)
        {
            Selfie = s;
            SelfieTime = 0f;
        }

        static float Lerp(float a, float b, float t) => a + (b - a) * t;
        static float Clamp(float v, float lo, float hi) => v < lo ? lo : v > hi ? hi : v;
    }
}
