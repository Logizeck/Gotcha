using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Gotcha.Core
{
    /// <summary>
    /// Modalità beta tester: si attiva dalle Impostazioni con la password. Dà vite infinite, hint infiniti e niente
    /// pubblicità (i livelli si sbloccano comunque giocando). La password non è scritta in chiaro: si confronta l'hash.
    /// </summary>
    public static class BetaMode
    {
        const string Key = "gotcha.beta";
        const string PasswordHash = "0a2de4058e3a1185116ebd8943f32bd76fc13c4e73ca8ea91488980e1462e773";

        public static event Action Changed;

        static bool? active;

        public static bool Active
        {
            get
            {
                if (!active.HasValue) active = PlayerPrefs.GetString(Key, "") == Stamp();
                return active.Value;
            }
        }

        /// <summary>Attiva la modalità beta se la password è giusta (maiuscole e minuscole contano).</summary>
        public static bool TryActivate(string password)
        {
            if (string.IsNullOrEmpty(password) || Hash("gotcha-beta:" + password.Trim()) != PasswordHash) return false;
            PlayerPrefs.SetString(Key, Stamp());
            PlayerPrefs.Save();
            active = true;
            Changed?.Invoke();
            return true;
        }

        public static void Deactivate()
        {
            PlayerPrefs.DeleteKey(Key);
            PlayerPrefs.Save();
            active = false;
            Changed?.Invoke();
        }

        // il valore salvato è legato al dispositivo: copiare il file delle preferenze su un altro telefono non basta
        static string Stamp() => Hash("beta-on:" + SystemInfo.deviceUniqueIdentifier).Substring(0, 24);

        static string Hash(string s)
        {
            using (var sha = SHA256.Create())
            {
                byte[] h = sha.ComputeHash(Encoding.UTF8.GetBytes(s));
                var sb = new StringBuilder(h.Length * 2);
                foreach (byte b in h) sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }
    }
}
