using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Core;
using Gotcha.Sensors;

namespace Gotcha.Levels
{
    /// <summary>Un dito sullo schermo (coordinate del mondo).</summary>
    public struct V64AFinger
    {
        public int id;
        public Vector2 world;
        public bool began, ended;
    }

    /// <summary>
    /// Dita multiple per i livelli 251/253/254: segue al massimo due dita per id (la terza viene ignorata), un dito che
    /// comincia sopra l'interfaccia (pausa, suggerimenti, pubblicità; PointerInput.IsOverUi) non viene seguito. Nell'Editor il mouse è un dito;
    /// tenendo premuto Shift, il primo clic "pianta" un secondo dito fermo in quel punto finché Shift resta premuto
    /// (come la seconda mano di The Crooked Bridge), così i gesti a due dita si provano anche senza touch.
    /// </summary>
    public static class V64AFingers
    {
        public const int MaxFingers = 2;
        const int MouseId = -1, EditorPinId = 900;

        static readonly List<V64AFinger> list = new List<V64AFinger>(4);
        static readonly List<int> tracked = new List<int>(4);
#if UNITY_EDITOR
        static bool pinActive, skipMouseUntilUp;
        static Vector2 pinWorld;
#endif

        public static void Reset()
        {
            list.Clear();
            tracked.Clear();
#if UNITY_EDITOR
            pinActive = skipMouseUntilUp = false;
#endif
        }

        /// <summary>Le dita di questo fotogramma (al massimo due). Da chiamare una volta per fotogramma.</summary>
        public static List<V64AFinger> Read(Camera cam)
        {
            list.Clear();
            if (cam == null) return list;
            int n = Input.touchCount;
            for (int i = 0; i < n; i++)
            {
                Touch t = Input.GetTouch(i);
                bool ended = t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled;
                bool began = t.phase == TouchPhase.Began;
                if (began && !tracked.Contains(t.fingerId))
                {
                    if (tracked.Count >= MaxFingers || (i == 0 && PointerInput.IsOverUi())) continue;
                    tracked.Add(t.fingerId);
                }
                if (!tracked.Contains(t.fingerId)) continue;
                list.Add(new V64AFinger { id = t.fingerId, world = PointerInput.ToWorld(cam, t.position), began = began, ended = ended });
                if (ended) tracked.Remove(t.fingerId);
            }
            // dita sparite senza "Ended" (es. app in pausa): si dimenticano
            if (n == 0 && tracked.Count > 0 && !Input.GetMouseButton(0) && !Input.GetMouseButtonUp(0)) tracked.Clear();
            if (n > 0) return list;

            Vector2 mouse = PointerInput.ToWorld(cam, Input.mousePosition);
            bool down = Input.GetMouseButtonDown(0), held = Input.GetMouseButton(0), up = Input.GetMouseButtonUp(0);
#if UNITY_EDITOR
            bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            if (shift && !pinActive && down && !PointerInput.IsOverUi())
            {
                pinActive = true;
                skipMouseUntilUp = true;
                pinWorld = mouse;
                list.Add(new V64AFinger { id = EditorPinId, world = pinWorld, began = true });
            }
            else if (pinActive && shift) list.Add(new V64AFinger { id = EditorPinId, world = pinWorld });
            else if (pinActive) { pinActive = false; list.Add(new V64AFinger { id = EditorPinId, world = pinWorld, ended = true }); }
            if (skipMouseUntilUp) { if (up || !held) skipMouseUntilUp = false; return list; }
#endif
            if (down)
            {
                if (PointerInput.IsOverUi()) return list;
                if (!tracked.Contains(MouseId)) tracked.Add(MouseId);
            }
            if (!tracked.Contains(MouseId)) return list;
            if (held || up || down) list.Add(new V64AFinger { id = MouseId, world = mouse, began = down, ended = up });
            if (up || !held) tracked.Remove(MouseId);
            return list;
        }
    }

    /// <summary>
    /// Microfono dei livelli 251/253/254: usa il microfono condiviso di SensorHub (Inputs.Mic, tasti B/N nell'Editor),
    /// solo mentre serve, e dice quando il microfono non c'è (permesso negato, nessun dispositivo) per mostrare
    /// l'alternativa accessibile.
    /// </summary>
    public sealed class V64AMic
    {
        readonly Object owner;
        bool on;
        float onSince;

        public V64AMic(Object owner) { this.owner = owner; }

        public bool On => on;

        /// <summary>Accende o spegne il microfono (idempotente).</summary>
        public void Use(bool want)
        {
            if (want == on) return;
            on = want;
            if (on) onSince = Time.time;
            else Inputs.ReleaseMic(owner);
        }

        /// <summary>Volume 0..1 (0 se spento).</summary>
        public float Level() => on ? Inputs.Mic(owner) : 0f;

        /// <summary>Il microfono dovrebbe registrare ma dopo un po' non c'è ancora: serve l'alternativa.</summary>
        public bool Unavailable(float grace = 1.5f)
        {
            if (!on || Time.time - onSince < grace) return false;
            return SensorHub.Instance == null || !SensorHub.Instance.MicActive;
        }
    }

    /// <summary>Suoni sintetizzati dei livelli 251/253/254 (squittio di gomma, palloncino che si sgonfia, gallo...).</summary>
    public static class V64ASound
    {
        const int Rate = 22050;
        static readonly Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>();
        static readonly System.Random rnd = new System.Random(64);

        public static void Play(Component owner, string kind, float volume = 0.6f)
        {
            if (owner == null) return;
            AudioClip c = Clip(kind);
            if (c == null) { Sfx.Play(owner, kind, volume); return; }
            AudioSource src = owner.GetComponent<AudioSource>();
            if (src == null) { src = owner.gameObject.AddComponent<AudioSource>(); src.playOnAwake = false; }
            src.PlayOneShot(c, volume);
        }

        static float N() => (float)rnd.NextDouble() * 2f - 1f;

        static AudioClip Clip(string kind)
        {
            if (cache.TryGetValue(kind, out AudioClip c) && c != null) return c;
            switch (kind)
            {
                case "squeak":   // gomma: due "iii" acuti che salgono e scendono
                    c = Make(kind, 0.26f, t =>
                    {
                        float f = 1500f + 650f * Mathf.Sin(t / 0.26f * Mathf.PI * 2f) + 90f * Mathf.Sin(t * 2f * Mathf.PI * 38f);
                        return Saw(t, f) * 0.45f * Mathf.Sin(t / 0.26f * Mathf.PI);
                    });
                    break;
                case "deflate":  // palloncino che si sgonfia volando via
                    c = Make(kind, 1.5f, t =>
                    {
                        float u = t / 1.5f;
                        float f = Mathf.Lerp(620f, 170f, u) * (1f + 0.12f * Mathf.Sin(t * 2f * Mathf.PI * 23f));
                        return (Saw(t, f) * 0.5f + N() * 0.18f) * (1f - u * 0.7f) * Mathf.Min(1f, t * 30f);
                    });
                    break;
                case "inflate":  // aria che entra
                    c = Make(kind, 0.35f, t => N() * 0.35f * Mathf.Sin(t / 0.35f * Mathf.PI) + Mathf.Sin(t * 2f * Mathf.PI * Mathf.Lerp(200f, 420f, t / 0.35f)) * 0.15f);
                    break;
                case "crow":     // chicchirichì: quattro sillabe con la voce che sale e scende
                    c = Crow();
                    break;
                case "ring":     // campanelli della sveglia (martelletto veloce)
                    c = Make(kind, 0.55f, t => (Mathf.Sin(t * 2f * Mathf.PI * 2350f) + 0.6f * Mathf.Sin(t * 2f * Mathf.PI * 3170f)) * 0.35f
                        * Mathf.Abs(Mathf.Sin(t * 2f * Mathf.PI * 11f)) * Mathf.Min(1f, (0.55f - t) * 12f));
                    break;
                case "ring_low": // la stessa suoneria ma attutita (attraverso il pavimento)
                    c = Make(kind, 0.55f, t => (Mathf.Sin(t * 2f * Mathf.PI * 700f) + 0.5f * Mathf.Sin(t * 2f * Mathf.PI * 1050f)) * 0.3f
                        * Mathf.Abs(Mathf.Sin(t * 2f * Mathf.PI * 11f)) * Mathf.Min(1f, (0.55f - t) * 12f));
                    break;
                case "glug":
                    c = Make(kind, 0.3f, t => Mathf.Sin(t * 2f * Mathf.PI * (180f + 260f * (t * 10f % 1f))) * 0.4f * Mathf.Sin(t / 0.3f * Mathf.PI));
                    break;
                case "bubbles":
                    c = Make(kind, 0.4f, t => Mathf.Sin(t * 2f * Mathf.PI * (500f + 700f * (t * 14f % 1f))) * 0.25f * Mathf.Exp(-(t * 14f % 1f) * 6f));
                    break;
                case "scrape":   // mobile pesante trascinato
                    {
                        float lp = 0f;
                        c = Make(kind, 0.4f, t => { lp += (N() - lp) * 0.08f; return lp * 1.6f * (0.6f + 0.4f * Mathf.Sin(t * 2f * Mathf.PI * 31f)) * Mathf.Sin(t / 0.4f * Mathf.PI); });
                    }
                    break;
                case "thuds":    // biglia che rimbalza di sotto (attutita)
                    c = Make(kind, 0.9f, t =>
                    {
                        float v = 0f;
                        float[] at = { 0f, 0.32f, 0.55f, 0.7f };
                        for (int i = 0; i < at.Length; i++)
                            if (t >= at[i]) { float k = t - at[i]; v += Mathf.Sin(k * 2f * Mathf.PI * 85f) * Mathf.Exp(-k * 28f) * (1f - i * 0.22f); }
                        return v * 0.7f;
                    });
                    break;
                case "boom":     // onda d'urto da cartone
                    c = Make(kind, 0.6f, t => (Mathf.Sin(t * 2f * Mathf.PI * Mathf.Lerp(140f, 40f, t / 0.6f)) * 0.8f + N() * 0.3f * Mathf.Exp(-t * 10f)) * Mathf.Exp(-t * 5f));
                    break;
                case "error":    // "bip-bop" del telefono
                    c = Make(kind, 0.3f, t => Mathf.Sign(Mathf.Sin(t * 2f * Mathf.PI * (t < 0.14f ? 420f : 280f))) * 0.18f * (t % 0.15f < 0.12f ? 1f : 0f));
                    break;
                case "unlock":   // tre note che salgono
                    c = Make(kind, 0.5f, t =>
                    {
                        float f = t < 0.12f ? 880f : t < 0.24f ? 1108f : 1318f;
                        return Mathf.Sin(t * 2f * Mathf.PI * f) * 0.35f * Mathf.Exp(-(t % 0.12f) * 8f);
                    });
                    break;
                default:
                    return null;
            }
            cache[kind] = c;
            return c;
        }

        static float Saw(float t, float f) => 2f * (t * f % 1f) - 1f;

        static AudioClip Crow()
        {
            // (inizio, durata, nota iniziale, nota finale)
            var syl = new[] { (0f, 0.16f, 520f, 600f), (0.2f, 0.12f, 560f, 620f), (0.36f, 0.18f, 650f, 720f), (0.6f, 0.75f, 760f, 520f) };
            float total = 1.4f;
            int n = (int)(total * Rate);
            var data = new float[n];
            float phase = 0f;
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)Rate;
                float v = 0f;
                foreach (var s in syl)
                {
                    if (t < s.Item1 || t > s.Item1 + s.Item2) continue;
                    float u = (t - s.Item1) / s.Item2;
                    float f = Mathf.Lerp(s.Item3, s.Item4, u) * (1f + 0.03f * Mathf.Sin(t * 2f * Mathf.PI * 30f));
                    phase += f / Rate;
                    if (phase > 1000f) phase -= 1000f;
                    for (int h = 1; h <= 5; h++) v += Mathf.Sin(2f * Mathf.PI * h * phase) / h;
                    v *= Mathf.Sin(u * Mathf.PI) * 0.35f;
                }
                data[i] = Mathf.Clamp(v + N() * 0.02f * (v != 0f ? 1f : 0f), -1f, 1f);
            }
            AudioClip clip = AudioClip.Create("v64_crow", n, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }

        static AudioClip Make(string name, float seconds, System.Func<float, float> f)
        {
            int n = Mathf.Max(1, (int)(seconds * Rate));
            var data = new float[n];
            for (int i = 0; i < n; i++) data[i] = Mathf.Clamp(f(i / (float)Rate), -1f, 1f);
            AudioClip clip = AudioClip.Create("v64_" + name, n, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
