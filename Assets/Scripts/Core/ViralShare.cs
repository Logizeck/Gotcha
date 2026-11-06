using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using Gotcha.UI;

namespace Gotcha.Core
{
    /// <summary>
    /// "Ti è piaciuto? Condividilo con un amico: +2 vite" (dalla 0.83). Quando proporlo e il limite di 3 premi al
    /// giorno stanno in ViralShareRules (logica pura); qui si salvano il contatore dei livelli superati (decide il
    /// "ogni 5 vittorie", non serve proteggerlo: non dà niente da solo) e i premi del giorno, firmati con HMAC come
    /// vite e hint (modificati a mano = limite raggiunto per oggi). Il giorno è quello del telefono: spostare la data
    /// indietro blocca i premi fino al giorno salvato; spostarla avanti non si può scoprire senza un server.
    /// </summary>
    public static class ViralShare
    {
        const string SolvedKey = "gotcha.viral.solved";
        const string DataKey = "gotcha.viral.v1";
        const string SigKey = "gotcha.viral.sig";
        const string Salt = "G0tcha!-friends-2b9e";

        /// <summary>Testo da condividere: invito breve + link personale per scaricare il gioco.</summary>
        public static string Text => "I just got tricked by a level in Gotcha! and loved it. Think you can beat it? Play free: " + InviteService.FriendLink;

        /// <summary>Un livello superato in più (si chiama una volta per ogni esito SOLVED). Restituisce il totale.</summary>
        public static int NoteSolved()
        {
            int n = PlayerPrefs.GetInt(SolvedKey, 0) + 1;
            if (n < 1) n = 1;
            PlayerPrefs.SetInt(SolvedKey, n);
            return n;
        }

        public static int Today => (int)(DateTime.Now.Date - new DateTime(2020, 1, 1)).TotalDays;

        /// <summary>Premi presi oggi (o il limite, se i dati sono stati alterati o la data è tornata indietro).</summary>
        public static int UsedToday
        {
            get
            {
                string data = PlayerPrefs.GetString(DataKey, "");
                if (string.IsNullOrEmpty(data)) return 0;
                string[] p = data.Split('|');
                if (PlayerPrefs.GetString(SigKey, "") != Sign(data) || p.Length != 2 ||
                    !int.TryParse(p[0], out int day) || !int.TryParse(p[1], out int count))
                    return ViralShareRules.DailyCap;
                return ViralShareRules.UsedToday(day, count, Today);
            }
        }

        public static bool CapReached => ViralShareRules.CapReached(UsedToday);

        /// <summary>Un premio dato adesso (dopo una condivisione verificata).</summary>
        public static void RecordReward()
        {
            int today = Today;
            int used = UsedToday;
            // con la data tornata indietro si resta sul giorno salvato (il blocco continua)
            string old = PlayerPrefs.GetString(DataKey, "");
            int day = today;
            if (!string.IsNullOrEmpty(old) && int.TryParse(old.Split('|')[0], out int d) && d > today) day = d;
            string data = day + "|" + Math.Min(ViralShareRules.DailyCap, used + 1);
            PlayerPrefs.SetString(DataKey, data);
            PlayerPrefs.SetString(SigKey, Sign(data));
            PlayerPrefs.Save();
        }

        static string Sign(string data)
        {
            byte[] key = Encoding.UTF8.GetBytes(Salt + SystemInfo.deviceUniqueIdentifier);
            using (var h = new HMACSHA256(key))
                return Convert.ToBase64String(h.ComputeHash(Encoding.UTF8.GetBytes(data)));
        }
    }
}
