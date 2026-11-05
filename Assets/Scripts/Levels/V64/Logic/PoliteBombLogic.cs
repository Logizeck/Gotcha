using System;

namespace Gotcha.Levels.V64
{
    /// <summary>Fasi del livello "The Polite Bomb" (roadmap n. 42).</summary>
    public enum PoliteBombStage
    {
        Ticking,    // il conto alla rovescia gira (o la bomba sta facendo lo sbuffo e ricomincia): si prova di tutto
        Defused,    // la batteria è fuori: display spento, festa in corso (input bloccato)
        Done,       // vittoria già data
    }

    /// <summary>Cosa succede in un passo di tempo.</summary>
    public enum PoliteBombTick
    {
        None,       // niente di nuovo da mostrare
        Second,     // il numero sul display è cambiato (tic)
        Puff,       // arrivato a zero: sbuffo innocuo "pfff... boom?" (niente sconfitta), pausa imbarazzata
        Restart,    // finita la pausa: il conto riparte da 00:10, velocità normale
    }

    /// <summary>Cosa succede premendo STOP.</summary>
    public enum PoliteBombStop
    {
        Ignored,    // input bloccato (festa): non conta
        Faster,     // "STOP? Sure! Faster!": il timer accelera (non vince MAI)
        TopSpeed,   // già alla velocità massima: resta lì (battuta diversa)
        Resting,    // durante lo sbuffo: la bomba sta ricominciando (conta come tentativo, nessun effetto)
    }

    /// <summary>Risultato di uno spostamento del dito che tiene la batteria.</summary>
    public enum PoliteBombMove
    {
        None,       // il dito non tiene la batteria (o input bloccato)
        Pulling,    // la batteria esce un po' ma non abbastanza (la bomba si preoccupa da Worried in su)
        Unplugged,  // tirata abbastanza lontano: STACCATA (VITTORIA, una volta sola; non può più tornare dentro)
    }

    /// <summary>Risultato di un dito che lascia la batteria.</summary>
    public enum PoliteBombDrop
    {
        None,       // il dito non teneva la batteria
        Tap,        // toccata senza trascinare: resta al suo posto
        SnapBack,   // trascinata poco e lasciata: torna nel vano con un clic
    }

    /// <summary>
    /// Logica pura del livello 42 della roadmap "The Polite Bomb" (id polite_bomb) — "STOP THE COUNTDOWN."
    /// Una bomba-cartone educatissima ha un display con 00:10, un grosso pulsante STOP, tre fili colorati e, sul fianco,
    /// un vano con la batteria che sporge un po'.
    /// Inganno: STOP fa ACCELERARE il timer (velocità +<see cref="SpeedStep"/> per pressione fino a <see cref="MaxSpeed"/>);
    /// i fili danno solo messaggi d'errore buffi sul display; toccare la bomba la fa protestare con garbo. Niente di
    /// questo fa mai vincere. Il timer è una GAG: a zero fa uno sbuffo innocuo (<see cref="PoliteBombTick.Puff"/>), resta
    /// imbarazzato per <see cref="PuffPause"/> secondi e riparte da <see cref="StartSeconds"/> a velocità normale
    /// (<see cref="PoliteBombTick.Restart"/>): mai sconfitte, mai vite perse.
    /// Soluzione esatta: trascinare la batteria FUORI dal vano: quando lo spostamento del dito (dalla presa) arriva a
    /// <see cref="RemoveDistance"/> in qualunque direzione tranne "dentro" la bomba (la componente x verso la bomba è
    /// limitata a <see cref="PushIn"/>), la batteria si stacca: il timer si ferma, il display si spegne, input bloccato,
    /// la batteria non può più tornare (Grab rifiutato, Release senza effetto). Trascinata meno e lasciata, torna nel vano.
    /// Coordinate: spostamenti del dito in unità del mondo (la logica non sa dove sia la bomba). Cancel() (pausa, app in
    /// background, livello spento) rimette la batteria nel vano senza contare niente. Nessuna dipendenza da Unity.
    /// </summary>
    public sealed class PoliteBombLogic
    {
        /// <summary>Id "nessun dito".</summary>
        public const int NoFinger = int.MinValue;
        /// <summary>Da dove parte il conto alla rovescia (00:10).</summary>
        public const float StartSeconds = 10f;
        /// <summary>Di quanto accelera il timer a ogni STOP.</summary>
        public const float SpeedStep = 1f;
        /// <summary>Velocità massima del timer (secondi del display per secondo vero).</summary>
        public const float MaxSpeed = 4f;
        /// <summary>Quanto dura lo sbuffo imbarazzato a 00:00 prima che il conto riparta.</summary>
        public const float PuffPause = 1.6f;
        /// <summary>Spostamento del dito (dalla presa) che stacca la batteria: generoso.</summary>
        public const float RemoveDistance = 0.85f;
        /// <summary>Di quanto al massimo la batteria si lascia spingere "dentro" la bomba (verso −x).</summary>
        public const float PushIn = 0.05f;
        /// <summary>Da questa distanza in su la bomba si preoccupa ("W-wait!"): stato intermedio, solo grafica.</summary>
        public const float WorryDistance = 0.3f;
        /// <summary>Spostamento oltre il quale non è più un tocco.</summary>
        public const float TapSlop = 0.15f;
        /// <summary>Quanti fili ha la bomba.</summary>
        public const int WireCount = 3;

        int finger = NoFinger;
        float sx, sy, ox, oy;
        bool moved, completed;

        public PoliteBombStage Stage { get; private set; } = PoliteBombStage.Ticking;
        public bool InputLocked => Stage != PoliteBombStage.Ticking;

        /// <summary>Secondi rimasti sul display (0 durante lo sbuffo).</summary>
        public float Remaining { get; private set; } = StartSeconds;
        /// <summary>Velocità attuale del timer (1 = normale).</summary>
        public float Speed { get; private set; } = 1f;
        /// <summary>Secondi di sbuffo che restano (&gt; 0: la bomba è imbarazzata, il timer è fermo a 00:00).</summary>
        public float PuffLeft { get; private set; }
        public bool Puffing => PuffLeft > 0f;

        /// <summary>La batteria è fuori (definitivo).</summary>
        public bool BatteryRemoved { get; private set; }
        public bool BatteryHeld => finger != NoFinger;
        public int BatteryFinger => finger;
        /// <summary>Spostamento attuale della batteria rispetto al vano (0 se non è tenuta).</summary>
        public float BatteryX => ox;
        public float BatteryY => oy;
        public float BatteryDistance => (float)Math.Sqrt(ox * ox + oy * oy);
        /// <summary>Quanto è uscita (0 nel vano, 1 = si stacca).</summary>
        public float Pull => Math.Min(1f, BatteryDistance / RemoveDistance);
        /// <summary>La batteria è tenuta ed è uscita abbastanza da far preoccupare la bomba.</summary>
        public bool Worried => BatteryHeld && BatteryDistance >= WorryDistance;

        public int Stops { get; private set; }
        public int WireTouches { get; private set; }
        public int Pokes { get; private set; }
        public int Puffs { get; private set; }
        public int SnapBacks { get; private set; }
        public int BatteryTaps { get; private set; }
        /// <summary>Tentativi sbagliati (tutti reversibili): STOP, fili, colpetti alla bomba, batteria lasciata a metà.</summary>
        public int WrongMoves => Stops + WireTouches + Pokes + SnapBacks;

        public PoliteBombLogic() { Reset(); }

        public void Reset()
        {
            Stage = PoliteBombStage.Ticking;
            Remaining = StartSeconds;
            Speed = 1f;
            PuffLeft = 0f;
            BatteryRemoved = false;
            finger = NoFinger;
            sx = sy = ox = oy = 0f;
            moved = completed = false;
            Stops = WireTouches = Pokes = Puffs = SnapBacks = BatteryTaps = 0;
        }

        static bool Bad(float v) => float.IsNaN(v) || float.IsInfinity(v);

        // ---------- Display ----------

        /// <summary>Il numero intero sul display (arrotondato in su: 10, 9, ... 1, poi 0 allo sbuffo).</summary>
        public int DisplaySeconds => Puffing ? 0 : Math.Max(0, Math.Min((int)StartSeconds, (int)Math.Ceiling(Remaining - 1e-4f)));

        /// <summary>Il testo del display ("00:07"); vuoto quando la batteria è fuori (display spento).</summary>
        public string Display => BatteryRemoved ? "" : "00:" + DisplaySeconds.ToString("00");

        // ---------- Tempo ----------

        /// <summary>Fa passare <paramref name="dt"/> secondi veri. Con la batteria fuori il timer è fermo per sempre.</summary>
        public PoliteBombTick Tick(float dt)
        {
            if (Stage != PoliteBombStage.Ticking || Bad(dt) || dt <= 0f) return PoliteBombTick.None;
            if (Puffing)
            {
                PuffLeft -= dt;
                if (PuffLeft > 0f) return PoliteBombTick.None;
                PuffLeft = 0f;
                Remaining = StartSeconds;
                Speed = 1f;
                return PoliteBombTick.Restart;
            }
            int before = DisplaySeconds;
            Remaining -= dt * Speed;
            if (Remaining <= 0f)
            {
                // la gag: sbuffo innocuo, nessuna sconfitta
                Remaining = 0f;
                PuffLeft = PuffPause;
                Puffs++;
                return PoliteBombTick.Puff;
            }
            return DisplaySeconds != before ? PoliteBombTick.Second : PoliteBombTick.None;
        }

        // ---------- Le false piste ----------

        /// <summary>Il grande pulsante STOP: accelera il timer. Non vince MAI.</summary>
        public PoliteBombStop PressStop()
        {
            if (InputLocked) return PoliteBombStop.Ignored;
            Stops++;
            if (Puffing) return PoliteBombStop.Resting;
            if (Speed >= MaxSpeed) return PoliteBombStop.TopSpeed;
            Speed = Math.Min(MaxSpeed, Speed + SpeedStep);
            return PoliteBombStop.Faster;
        }

        /// <summary>Un filo toccato: solo un messaggio d'errore. Restituisce il numero d'ordine del tocco (−1 se ignorato).</summary>
        public int TouchWire(int wire)
        {
            if (InputLocked || wire < 0 || wire >= WireCount) return -1;
            return WireTouches++;
        }

        /// <summary>Colpetto sulla bomba: protesta educata. Restituisce il numero d'ordine (−1 se ignorato).</summary>
        public int Poke()
        {
            if (InputLocked) return -1;
            return Pokes++;
        }

        // ---------- La batteria (la soluzione) ----------

        /// <summary>
        /// Il dito <paramref name="id"/> (in (fx, fy), mondo) prende la batteria. Falso se l'input è bloccato, se la batteria
        /// è già fuori o già tenuta da un altro dito.
        /// </summary>
        public bool GrabBattery(int id, float fx, float fy)
        {
            if (InputLocked || BatteryRemoved || id == NoFinger || finger != NoFinger || Bad(fx) || Bad(fy)) return false;
            finger = id;
            sx = fx;
            sy = fy;
            ox = oy = 0f;
            moved = false;
            return true;
        }

        /// <summary>Il dito che tiene la batteria si sposta in (fx, fy). Abbastanza lontano: la batteria si stacca.</summary>
        public PoliteBombMove MoveBattery(int id, float fx, float fy)
        {
            if (InputLocked || id == NoFinger || id != finger || Bad(fx) || Bad(fy)) return PoliteBombMove.None;
            float rx = fx - sx, dy = fy - sy;
            if (rx * rx + dy * dy > TapSlop * TapSlop) moved = true;   // il dito si è mosso: non è più un tocco
            ox = Math.Max(-PushIn, rx);
            oy = dy;
            float d = BatteryDistance;
            if (d < RemoveDistance) return PoliteBombMove.Pulling;
            // staccata: definitivo, il timer si ferma, input bloccato
            BatteryRemoved = true;
            finger = NoFinger;
            Stage = PoliteBombStage.Defused;
            return PoliteBombMove.Unplugged;
        }

        /// <summary>Il dito lascia la batteria: se non si è staccata torna nel vano.</summary>
        public PoliteBombDrop ReleaseBattery(int id)
        {
            if (id == NoFinger || id != finger) return PoliteBombDrop.None;
            finger = NoFinger;
            ox = oy = 0f;
            if (InputLocked) return PoliteBombDrop.None;
            if (!moved) { BatteryTaps++; return PoliteBombDrop.Tap; }
            SnapBacks++;
            return PoliteBombDrop.SnapBack;
        }

        /// <summary>Pausa, app in background, livello spento: la batteria torna nel vano, niente conta.</summary>
        public void Cancel()
        {
            finger = NoFinger;
            ox = oy = 0f;
            moved = false;
        }

        /// <summary>Vero una volta sola, dopo che la batteria è stata staccata.</summary>
        public bool TryComplete()
        {
            if (Stage != PoliteBombStage.Defused || completed) return false;
            completed = true;
            Stage = PoliteBombStage.Done;
            return true;
        }
    }
}
