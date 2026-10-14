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

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (SystemInfo.supportsGyroscope) Input.gyro.enabled = true;
            Input.compass.enabled = true;
            lastAccel = Tilt;
        }

        void Update()
        {
            Vector3 a = Tilt;
            float delta = (a - lastAccel).magnitude;
            lastAccel = a;
            shake = Mathf.Max(delta * 10f, Mathf.Lerp(shake, 0f, 8f * Time.unscaledDeltaTime));

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

        public bool IsCharging => SystemInfo.batteryStatus == BatteryStatus.Charging;

        // ---------- Volume di sistema ----------

        public float SystemVolume01 => volumeCache;

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
#else
            return AudioListener.volume;
#endif
        }

        // ---------- Microfono ----------

        /// <summary>Idempotente: può essere richiamato ogni frame finché il permesso non arriva.</summary>
        public void EnableMic()
        {
            if (micClip != null) return;
#if UNITY_ANDROID && !UNITY_EDITOR
            string perm = UnityEngine.Android.Permission.Microphone;
            if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(perm))
            {
                if (!micPermissionAsked)
                {
                    micPermissionAsked = true;
                    UnityEngine.Android.Permission.RequestUserPermission(perm);
                }
                return;
            }
#endif
            if (Microphone.devices.Length == 0) return;
            micClip = Microphone.Start(null, true, 1, MicRate);
        }

        public void DisableMic()
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

        void OnDestroy() { if (Instance == this) DisableMic(); }
    }
}
