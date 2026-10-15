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
    /// Registra gli ultimi secondi di gioco per poterli condividere sui social (video "virale" dello scioglimento
    /// del pupazzo, ecc.). Durante la cattura salva lo schermo a bassa frequenza (12 fps, 288×512), compresso in JPEG,
    /// in un buffer circolare (poca memoria: ~4 MB). Al momento della condivisione su Android i fotogrammi vengono
    /// codificati in MP4 dal plugin GotchaVideo.java, il video viene salvato in galleria e si apre il menu di
    /// condivisione. Nell'Editor i fotogrammi vengono salvati come immagini per controllo.
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

        readonly List<byte[]> frames = new List<byte[]>();
        RenderTexture full, small;
        Coroutine loop;

        void Awake() { Instance = this; }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
            ReleaseTextures();
        }

        /// <summary>Inizia (o ricomincia) a registrare, cancellando la clip precedente.</summary>
        public void Begin()
        {
            frames.Clear();
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
            ScreenCapture.CaptureScreenshotIntoRenderTexture(full);
            // su alcune piattaforme la cattura arriva capovolta
            if (SystemInfo.graphicsUVStartsAtTop) Graphics.Blit(full, small, new Vector2(1f, -1f), new Vector2(0f, 1f));
            else Graphics.Blit(full, small);
            if (watermark != null) DrawWatermark();

            if (SystemInfo.supportsAsyncGPUReadback)
                AsyncGPUReadback.Request(small, 0, TextureFormat.RGBA32, OnReadback);
            else
            {
                var tex = new Texture2D(Width, Height, TextureFormat.RGBA32, false);
                RenderTexture prev = RenderTexture.active;
                RenderTexture.active = small;
                tex.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
                RenderTexture.active = prev;
                Store(tex.EncodeToJPG(jpegQuality));
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

        void OnReadback(AsyncGPUReadbackRequest request)
        {
            if (request.hasError || this == null) return;
            NativeArray<byte> data = request.GetData<byte>();
            byte[] jpg = ImageConversion.EncodeArrayToJPG(data.ToArray(), UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm,
                                                          (uint)Width, (uint)Height, 0, jpegQuality);
            Store(jpg);
        }

        void Store(byte[] jpg)
        {
            if (jpg == null) return;
            frames.Add(jpg);
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
                    var jpg = new sbyte[frames[i].Length];   // Java vuole byte con segno
                    Buffer.BlockCopy(frames[i], 0, jpg, 0, jpg.Length);
                    encoder.Call("addJpeg", jpg);
                    if (i % 4 == 3) yield return null;   // l'interfaccia resta viva
                }
                encoder.Call("finish");
                encoder.Dispose();
                using (var cls = new AndroidJavaClass("com.gotcha.video.GotchaVideo"))
                    ok = cls.CallStatic<bool>("share", path, "Gotcha!", message);
            }
#else
            // Editor / altre piattaforme: salva i fotogrammi per controllo
            string dir = Path.Combine(Application.persistentDataPath, "clip_frames");
            Directory.CreateDirectory(dir);
            for (int i = 0; i < frames.Count; i++) File.WriteAllBytes(Path.Combine(dir, "frame_" + i.ToString("000") + ".jpg"), frames[i]);
            Debug.Log("[Gotcha] Clip di " + frames.Count + " fotogrammi salvata in " + dir + " (il video MP4 e la condivisione funzionano su Android).");
            ok = true;
#endif
            IsSharing = false;
            onDone?.Invoke(ok);
        }
    }
}
