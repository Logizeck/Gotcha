using UnityEngine;

namespace Gotcha.Sensors
{
    public enum SensorChannel
    {
        TiltX, TiltY, Shake, GyroSpeed, CompassHeading,
        MicLevel, SystemVolume, Battery, Charging
    }

    /// <summary>
    /// Unico punto di accesso ai sensori. I livelli non toccano mai le API del telefono:
    /// leggono da qui, così l'eventuale porting iOS richiede di cambiare solo questo file.
    /// Richiede Active Input Handling = "Input Manager (Old)" (default del template 2D).
    /// </summary>
    public class SensorHub : MonoBehaviour
    {
        public static SensorHub Instance { get; private set; }

        [SerializeField] float shakeThreshold = 2.2f;

        Vector3 lastAccel;
        float shake;

        AudioClip micClip;
        bool micPermissionAsked;
        readonly float[] micBuf = new float[256];
        const int MicRate = 16000;

        float volumeCache = 1f;
        float volumeTimer;
#if UNITY_ANDROID && !UNITY_EDITOR
        int keysFrame = -1, keysCache;
#else
        int editorDownPresses;
#endif
#if UNITY_EDITOR
        bool editorCharging;
        float editorVolume = 1f;   // nell'Editor il volume si simula coi tasti - e +
#endif

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (SystemInfo.supportsGyroscope) Input.gyro.enabled = true;
            Input.compass.enabled = true;
            lastAccel = Tilt;
        }

        void Start()
        {
            // Il permesso del microfono si chiede una volta sola all'avvio del gioco, non a metà livello.
            RequestMicPermission();
#if UNITY_ANDROID && !UNITY_EDITOR
            try { using (var keys = new AndroidJavaClass("com.gotcha.game.GotchaKeys")) keys.CallStatic("install"); }
            catch (System.Exception e) { Debug.LogWarning("[Gotcha] Tasti volume non intercettabili: " + e.Message); }
#endif
        }

        /// <summary>Mostra la richiesta di Android per il microfono, se non è già stato concesso o chiesto.</summary>
        public void RequestMicPermission()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            string perm = UnityEngine.Android.Permission.Microphone;
            if (micPermissionAsked || UnityEngine.Android.Permission.HasUserAuthorizedPermission(perm)) return;
            micPermissionAsked = true;
            UnityEngine.Android.Permission.RequestUserPermission(perm);
#endif
        }

        void Update()
        {
            Vector3 a = Tilt;
            float delta = (a - lastAccel).magnitude;
            lastAccel = a;
            shake = Mathf.Max(delta * 10f, Mathf.Lerp(shake, 0f, 8f * Time.unscaledDeltaTime));

#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus)) editorDownPresses++;
            if (Input.GetKeyDown(KeyCode.C)) editorCharging = !editorCharging;
            if (Input.GetKey(KeyCode.Minus) || Input.GetKey(KeyCode.KeypadMinus)) editorVolume = Mathf.Max(0f, editorVolume - 0.6f * Time.unscaledDeltaTime);
            if (Input.GetKey(KeyCode.Equals) || Input.GetKey(KeyCode.KeypadPlus)) editorVolume = Mathf.Min(1f, editorVolume + 0.6f * Time.unscaledDeltaTime);
#endif
            micPoll -= Time.unscaledDeltaTime;
            if (micPoll <= 0f) { micPoll = 0.3f; UpdateMic(); }

            volumeTimer -= Time.unscaledDeltaTime;
            if (volumeTimer <= 0f)
            {
                volumeTimer = 0.2f;
                volumeCache = ReadSystemVolume();
            }
        }

        // ---------- Accelerometro / giroscopio / bussola ----------

        public Vector3 Tilt
        {
            get
            {
                Vector3 a = Input.acceleration;
#if UNITY_EDITOR
                if (a.sqrMagnitude < 0.0001f)
                    a = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0f);
#endif
                return a;
            }
        }

        public bool IsShaking => shake > shakeThreshold * 10f;
        public float ShakeStrength => shake;

        public float GyroSpeed => SystemInfo.supportsGyroscope ? Input.gyro.rotationRateUnbiased.magnitude : 0f;
        public float CompassHeading => Input.compass.magneticHeading;

        // ---------- Batteria ----------

        public float Battery01
        {
            get { float b = SystemInfo.batteryLevel; return b < 0f ? 1f : b; }
        }

        /// <summary>Telefono collegato al caricatore (in carica o già pieno). Nell'Editor si simula col tasto C.</summary>
        public bool IsCharging
        {
            get
            {
#if UNITY_EDITOR
                return editorCharging;
#else
                BatteryStatus s = SystemInfo.batteryStatus;
                return s == BatteryStatus.Charging || s == BatteryStatus.Full;
#endif
            }
        }

        // ---------- Volume di sistema ----------

        public float SystemVolume01 => volumeCache;

        /// <summary>
        /// Quante volte è stato premuto il tasto "volume -" da quando il gioco è aperto (anche quando il volume è già
        /// al minimo e quindi il valore non cambia). Nell'Editor conta il tasto "-".
        /// </summary>
        public int VolumeDownPresses
        {
            get
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                if (Time.frameCount != keysFrame)
                {
                    keysFrame = Time.frameCount;
                    try { using (var keys = new AndroidJavaClass("com.gotcha.game.GotchaKeys")) keysCache = keys.CallStatic<int>("volumeDownCount"); }
                    catch { }
                }
                return keysCache;
#else
                return editorDownPresses;
#endif
            }
        }

        float ReadSystemVolume()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var activity = player.GetStatic<AndroidJavaObject>("currentActivity"))
                using (var am = activity.Call<AndroidJavaObject>("getSystemService", "audio"))
                {
                    int cur = am.Call<int>("getStreamVolume", 3);   // 3 = STREAM_MUSIC
                    int max = am.Call<int>("getStreamMaxVolume", 3);
                    return max > 0 ? cur / (float)max : 0f;
                }
            }
            catch { return 1f; }
#elif UNITY_EDITOR
            return editorVolume;
#else
            return AudioListener.volume;
#endif
        }

        // ---------- Microfono ----------

        // Il microfono è condiviso: ogni livello che lo usa si "iscrive" con UseMic(this) e si cancella con
        // ReleaseMic(this). Resta acceso finché c'è almeno un iscritto ancora vivo (chi viene distrutto decade da
        // solo), parte appena arriva il permesso e riparte se Android lo ferma (es. app in background).
        // Così il livello nuovo non perde il microfono quando quello vecchio viene distrutto subito dopo.
        readonly System.Collections.Generic.List<Object> micUsers = new System.Collections.Generic.List<Object>();
        float micPoll;

        public void UseMic(Object owner)
        {
            if (owner == null || micUsers.Contains(owner)) return;
            micUsers.Add(owner);
            micPermissionAsked = false;   // un livello col microfono: se il permesso manca, lo si richiede
            UpdateMic();
        }

        public void ReleaseMic(Object owner)
        {
            micUsers.Remove(owner);
            UpdateMic();
        }

        /// <summary>True se il microfono sta registrando davvero.</summary>
        public bool MicActive => micClip != null;

        void UpdateMic()
        {
            micUsers.RemoveAll(o => o == null);
            if (micUsers.Count == 0) { StopMic(); return; }
            if (micClip != null && !Microphone.IsRecording(null)) micClip = null;   // fermato dal sistema: si riparte
            if (micClip == null) StartMic();
        }

        void StartMic()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            string perm = UnityEngine.Android.Permission.Microphone;
            if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(perm))
            {
                RequestMicPermission();   // di norma già chiesto all'avvio; si riprova finché non arriva
                return;
            }
#endif
            if (Microphone.devices.Length == 0) return;
            micClip = Microphone.Start(null, true, 1, MicRate);
        }

        void StopMic()
        {
            if (micClip == null) return;
            Microphone.End(null);
            micClip = null;
        }

        public float MicLevel01
        {
            get
            {
                if (micClip == null) return 0f;
                int pos = Microphone.GetPosition(null) - micBuf.Length;
                if (pos < 0) return 0f;
                micClip.GetData(micBuf, pos);
                float sum = 0f;
                for (int i = 0; i < micBuf.Length; i++) sum += micBuf[i] * micBuf[i];
                return Mathf.Clamp01(Mathf.Sqrt(sum / micBuf.Length) * 4f);
            }
        }

        // ---------- Accesso generico per canale ----------

        public float Read(SensorChannel channel)
        {
            switch (channel)
            {
                case SensorChannel.TiltX: return Tilt.x;
                case SensorChannel.TiltY: return Tilt.y;
                case SensorChannel.Shake: return shake;
                case SensorChannel.GyroSpeed: return GyroSpeed;
                case SensorChannel.CompassHeading: return CompassHeading;
                case SensorChannel.MicLevel: return MicLevel01;
                case SensorChannel.SystemVolume: return SystemVolume01;
                case SensorChannel.Battery: return Battery01;
                case SensorChannel.Charging: return IsCharging ? 1f : 0f;
                default: return 0f;
            }
        }

        void OnDestroy() { if (Instance == this) StopMic(); }
    }
}
