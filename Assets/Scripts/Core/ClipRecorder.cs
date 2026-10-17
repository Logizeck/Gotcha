using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace Gotcha.Core
{
    /// <summary>
    /// Registra gli ultimi secondi di gioco per poterli condividere sui social quando si scopre un Easter egg.
    /// GameFlow la fa partire a ogni livello: lo schermo viene salvato a bassa frequenza (12 fps, 288×512),
    /// compresso in JPEG, in un buffer circolare (~4 MB); il suono del gioco (tutto quello che arriva all'
    /// AudioListener: musica ed effetti) in un altro buffer circolare, sincronizzato con i fotogrammi.
    /// Al momento della condivisione su Android i fotogrammi e l'audio vengono codificati in MP4 (H.264 + AAC) dal
    /// plugin GotchaVideo.java, il video viene salvato in galleria e si apre il menu di condivisione.
    /// Nell'Editor fotogrammi e audio (WAV) vengono salvati su disco per controllo.
    /// </summary>
    public class ClipRecorder : MonoBehaviour
    {
        public const int Width = 288, Height = 512;   // multipli di 16 (richiesto dai codificatori video)

        public int fps = 12;
        public float maxSeconds = 14f;
        [Range(30, 95)] public int jpegQuality = 80;
        [Tooltip("Facoltativo: logo disegnato nell'angolo del video (pubblicità gratuita quando viene condiviso).")]
        public Texture2D watermark;

        public static ClipRecorder Instance { get; private set; }
        public bool IsCapturing { get; private set; }
        public bool HasClip => frames.Count > 0 && !IsCapturing;
        public bool IsSharing { get; private set; }
        /// <summary>La vita in regalo per la condivisione di questa clip è già stata data?</summary>
        public bool RewardClaimed { get; set; }

        struct Frame
        {
            public byte[] jpg;
            public long audioPos;   // posizione nel flusso audio (campioni per canale) al momento della cattura
        }

        readonly List<Frame> frames = new List<Frame>();
        RenderTexture full, small;
        Coroutine loop;

        // ---------- audio: buffer circolare riempito dal thread audio (ClipAudioTap) ----------
        static readonly object audioLock = new object();
        static short[] audioRing;
        static long audioWritten;          // campioni per canale scritti da sempre
        static int audioChannels = 2;      // canali salvati (al massimo 2: AAC stereo)
        static int audioRate;

        void Awake()
        {
            Instance = this;
            audioRate = AudioSettings.outputSampleRate;
            lock (audioLock) audioRing = new short[Mathf.CeilToInt((maxSeconds + 2f) * audioRate) * audioChannels];
        }

        /// <summary>Aggancia il "rubinetto" audio all'AudioListener (lì passa il mix finale di tutto il gioco).</summary>
        void EnsureAudioTap()
        {
            AudioListener listener = Camera.main != null ? Camera.main.GetComponent<AudioListener>() : null;   // sta sulla camera
            if (listener != null && listener.GetComponent<ClipAudioTap>() == null) listener.gameObject.AddComponent<ClipAudioTap>();
        }

        /// <summary>Chiamato dal thread audio: copia (senza modificarlo) il mix in uscita nel buffer circolare.</summary>
        internal static void PushAudio(float[] data, int channels)
        {
            if (channels <= 0) return;
            lock (audioLock)
            {
                if (audioRing == null) return;
                int frameCount = data.Length / channels, cap = audioRing.Length / audioChannels;
                for (int i = 0; i < frameCount; i++)
                {
                    int slot = (int)((audioWritten + i) % cap) * audioChannels;
                    float l = data[i * channels], r = channels > 1 ? data[i * channels + 1] : l;
                    audioRing[slot] = (short)(Mathf.Clamp(l, -1f, 1f) * 32767f);
                    audioRing[slot + 1] = (short)(Mathf.Clamp(r, -1f, 1f) * 32767f);
                }
                audioWritten += frameCount;
            }
        }

        static long AudioNow { get { lock (audioLock) return audioWritten; } }

        /// <summary>PCM 16 bit little-endian stereo da "start" per "count" campioni (silenzio dove manca).</summary>
        static byte[] ReadAudio(long start, int count)
        {
            var bytes = new byte[count * audioChannels * 2];
            lock (audioLock)
            {
                if (audioRing == null) return bytes;
                int cap = audioRing.Length / audioChannels;
                for (int i = 0; i < count; i++)
                {
                    long pos = start + i;
                    if (pos < 0 || pos >= audioWritten || audioWritten - pos > cap) continue;   // non (più) nel buffer
                    int slot = (int)(pos % cap) * audioChannels;
                    for (int c = 0; c < audioChannels; c++)
                    {
                        short v = audioRing[slot + c];
                        int o = (i * audioChannels + c) * 2;
                        bytes[o] = (byte)(v & 0xff);
                        bytes[o + 1] = (byte)((v >> 8) & 0xff);
                    }
                }
            }
            return bytes;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
            ReleaseTextures();
        }

        /// <summary>Inizia (o ricomincia) a registrare, cancellando la clip precedente.</summary>
        public void Begin()
        {
            frames.Clear();
            RewardClaimed = false;
            EnsureAudioTap();
            IsCapturing = true;
            if (loop == null) loop = StartCoroutine(CaptureLoop());
        }

        /// <summary>Ferma la registrazione e tiene la clip pronta per la condivisione.</summary>
        public void Stop()
        {
            IsCapturing = false;
            if (loop != null) { StopCoroutine(loop); loop = null; }
        }

        /// <summary>Ferma e butta via tutto (es. si cambia livello senza condividere).</summary>
        public void Discard()
        {
            Stop();
            frames.Clear();
        }

        IEnumerator CaptureLoop()
        {
            var wait = new WaitForEndOfFrame();
            float next = 0f;
            while (IsCapturing)
            {
                yield return wait;
                if (Time.unscaledTime < next) continue;
                next = Time.unscaledTime + 1f / fps;
                CaptureFrame();
            }
        }

        void CaptureFrame()
        {
            if (full == null || full.width != Screen.width || full.height != Screen.height)
            {
                ReleaseTextures();
                full = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
                small = new RenderTexture(Width, Height, 0, RenderTextureFormat.ARGB32);
            }
            long audioPos = AudioNow;
            ScreenCapture.CaptureScreenshotIntoRenderTexture(full);
            // su alcune piattaforme la cattura arriva capovolta
            if (SystemInfo.graphicsUVStartsAtTop) Graphics.Blit(full, small, new Vector2(1f, -1f), new Vector2(0f, 1f));
            else Graphics.Blit(full, small);
            if (watermark != null) DrawWatermark();

            if (SystemInfo.supportsAsyncGPUReadback)
                AsyncGPUReadback.Request(small, 0, TextureFormat.RGBA32, r => OnReadback(r, audioPos));
            else
            {
                var tex = new Texture2D(Width, Height, TextureFormat.RGBA32, false);
                RenderTexture prev = RenderTexture.active;
                RenderTexture.active = small;
                tex.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
                RenderTexture.active = prev;
                Store(tex.EncodeToJPG(jpegQuality), audioPos);
                Destroy(tex);
            }
        }

        void DrawWatermark()
        {
            RenderTexture prev = RenderTexture.active;
            RenderTexture.active = small;
            GL.PushMatrix();
            GL.LoadPixelMatrix(0, Width, Height, 0);
            float size = Width * 0.22f;
            Graphics.DrawTexture(new Rect(Width - size - 8, 8, size, size), watermark);
            GL.PopMatrix();
            RenderTexture.active = prev;
        }

        void OnReadback(AsyncGPUReadbackRequest request, long audioPos)
        {
            if (request.hasError || this == null || !IsCapturing) return;
            NativeArray<byte> data = request.GetData<byte>();
            byte[] jpg = ImageConversion.EncodeArrayToJPG(data.ToArray(), UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm,
                                                          (uint)Width, (uint)Height, 0, jpegQuality);
            Store(jpg, audioPos);
        }

        void Store(byte[] jpg, long audioPos)
        {
            if (jpg == null) return;
            frames.Add(new Frame { jpg = jpg, audioPos = audioPos });
            int max = Mathf.CeilToInt(maxSeconds * fps);
            while (frames.Count > max) frames.RemoveAt(0);   // buffer circolare: restano gli ultimi secondi
        }

        void ReleaseTextures()
        {
            if (full != null) { full.Release(); Destroy(full); full = null; }
            if (small != null) { small.Release(); Destroy(small); small = null; }
        }

        /// <summary>Crea il video e apre la condivisione. onDone(true) se è andato tutto bene.</summary>
        public void Share(string message, Action<bool> onDone)
        {
            if (!HasClip || IsSharing) { onDone?.Invoke(false); return; }
            StartCoroutine(ShareRoutine(message, onDone));
        }

        IEnumerator ShareRoutine(string message, Action<bool> onDone)
        {
            IsSharing = true;
            bool ok = false;
            string path = Path.Combine(Application.temporaryCachePath, "gotcha_clip.mp4");
            // la colonna sonora lunga esattamente quanto i fotogrammi, a partire dal primo
            int audioCount = Mathf.RoundToInt(frames.Count / (float)fps * audioRate);
            byte[] pcm = ReadAudio(frames[0].audioPos, audioCount);
            yield return null;
#if UNITY_ANDROID && !UNITY_EDITOR
            AndroidJavaObject encoder = null;
            try
            {
                using (var cls = new AndroidJavaClass("com.gotcha.video.GotchaVideo"))
                    encoder = cls.CallStatic<AndroidJavaObject>("begin", path, Width, Height, fps);
            }
            catch (Exception e) { Debug.LogWarning("[Gotcha] Video non disponibile: " + e.Message); }
            if (encoder != null)
            {
                for (int i = 0; i < frames.Count; i++)
                {
                    var jpg = new sbyte[frames[i].jpg.Length];   // Java vuole byte con segno
                    Buffer.BlockCopy(frames[i].jpg, 0, jpg, 0, jpg.Length);
                    encoder.Call("addJpeg", jpg);
                    if (i % 4 == 3) yield return null;   // l'interfaccia resta viva
                }
                var pcmSigned = new sbyte[pcm.Length];
                Buffer.BlockCopy(pcm, 0, pcmSigned, 0, pcm.Length);
                encoder.Call("addAudio", pcmSigned, audioRate, audioChannels);
                yield return null;
                encoder.Call("finish");
                encoder.Dispose();
                using (var cls = new AndroidJavaClass("com.gotcha.video.GotchaVideo"))
                    ok = cls.CallStatic<bool>("share", path, "Gotcha!", message);
            }
#else
            // Editor / altre piattaforme: salva i fotogrammi per controllo
            string dir = Path.Combine(Application.persistentDataPath, "clip_frames");
            Directory.CreateDirectory(dir);
            for (int i = 0; i < frames.Count; i++) File.WriteAllBytes(Path.Combine(dir, "frame_" + i.ToString("000") + ".jpg"), frames[i].jpg);
            File.WriteAllBytes(Path.Combine(dir, "audio.wav"), Wav(pcm, audioRate, audioChannels));
            Debug.Log("[Gotcha] Clip di " + frames.Count + " fotogrammi (+ audio.wav) salvata in " + dir + " (il video MP4 e la condivisione funzionano su Android).");
            ok = true;
#endif
            IsSharing = false;
            onDone?.Invoke(ok);
        }

        static byte[] Wav(byte[] pcm, int rate, int channels)
        {
            using (var ms = new MemoryStream())
            using (var w = new BinaryWriter(ms))
            {
                w.Write(new[] { 'R', 'I', 'F', 'F' });
                w.Write(36 + pcm.Length);
                w.Write(new[] { 'W', 'A', 'V', 'E', 'f', 'm', 't', ' ' });
                w.Write(16);
                w.Write((short)1);
                w.Write((short)channels);
                w.Write(rate);
                w.Write(rate * channels * 2);
                w.Write((short)(channels * 2));
                w.Write((short)16);
                w.Write(new[] { 'd', 'a', 't', 'a' });
                w.Write(pcm.Length);
                w.Write(pcm);
                return ms.ToArray();
            }
        }
    }

    /// <summary>Sta sull'AudioListener: passa al registratore il suono del gioco così com'è (non lo modifica).</summary>
    public class ClipAudioTap : MonoBehaviour
    {
        void OnAudioFilterRead(float[] data, int channels) { ClipRecorder.PushAudio(data, channels); }
    }
}
