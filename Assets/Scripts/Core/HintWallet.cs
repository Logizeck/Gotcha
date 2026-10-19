using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Gotcha.Core
{
    /// <summary>
    /// Gli hint del giocatore. Ogni livello ha 3 suggerimenti (dal più vago alla soluzione) e ognuno costa un hint.
    /// Gli hint NON si ricaricano col tempo: si guadagnano guardando una pubblicità (+1), condividendo il video di un
    /// Easter egg (+5, se si sceglie questo premio) o comprando un pacchetto (3 o 15). Un suggerimento sbloccato resta
    /// sbloccato per quel livello. Il saldo è firmato come le vite (non si modifica a mano nei salvataggi).
    /// </summary>
    public static class HintWallet
    {
        public const int StartingHints = 5;
        const string DataKey = "gotcha.hints.v1";
        const string SigKey = "gotcha.hints.sig";
        const string Salt = "G0tcha!-bulbs-2c9e";
        const string RevealedPrefix = "gotcha.hint.";

        public static event Action Changed;

        static int? balance;

        public static int Balance
        {
            get
            {
                if (!balance.HasValue) balance = Load();
                return balance.Value;
            }
        }

        public static void Add(int count)
        {
            if (count <= 0) return;
            Save(Balance + count);
        }

        /// <summary>Quanti suggerimenti di questo livello sono già sbloccati (0-3).</summary>
        public static int Revealed(string levelId) => PlayerPrefs.GetInt(RevealedPrefix + levelId, 0);

        /// <summary>Sblocca il prossimo suggerimento del livello, se c'è un hint da spendere.</summary>
        public static bool TryReveal(string levelId, int available)
        {
            int shown = Revealed(levelId);
            if (shown >= available || (Balance <= 0 && !BetaMode.Active)) return false;
            PlayerPrefs.SetInt(RevealedPrefix + levelId, shown + 1);
            if (BetaMode.Active) { PlayerPrefs.Save(); Changed?.Invoke(); }   // beta tester: hint infiniti
            else Save(Balance - 1);
            return true;
        }

        static int Load()
        {
            string data = PlayerPrefs.GetString(DataKey, "");
            if (string.IsNullOrEmpty(data)) { Save(StartingHints); return StartingHints; }
            if (PlayerPrefs.GetString(SigKey, "") != Sign(data) || !int.TryParse(data, out int n) || n < 0)
            {
                Debug.LogWarning("[Gotcha] Hint non validi: si riparte da zero.");
                Save(0);
                return 0;
            }
            return n;
        }

        static void Save(int n)
        {
            balance = Mathf.Max(0, n);
            string data = balance.Value.ToString();
            PlayerPrefs.SetString(DataKey, data);
            PlayerPrefs.SetString(SigKey, Sign(data));
            PlayerPrefs.Save();
            Changed?.Invoke();
        }

        static string Sign(string data)
        {
            byte[] key = Encoding.UTF8.GetBytes(Salt + SystemInfo.deviceUniqueIdentifier);
            using (var h = new HMACSHA256(key))
                return Convert.ToBase64String(h.ComputeHash(Encoding.UTF8.GetBytes(data)));
        }
    }
}
