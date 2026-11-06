using System.Collections.Generic;
using UnityEngine;
using Gotcha.Progress;

namespace Gotcha.Core
{
    /// <summary>
    /// Salvataggio minimo: livelli superati (per identificativo) ed Easter egg trovati.
    /// Dalla 0.76 i progressi sono l'elenco dei livelli superati, non più "la posizione più alta sbloccata": l'ordine dei
    /// livelli può cambiare (roadmap 1-300) senza perdere nulla. Il vecchio salvataggio viene convertito una volta sola
    /// (ProgressLogic.MigrateFromIndex con l'ordine della 0.75) e poi lasciato com'è.
    /// </summary>
    public static class SaveData
    {
        const string UnlockedKey = "gotcha.unlocked";            // fino alla 0.75: posizione più alta sbloccata
        const string CompletedKey = "gotcha.completed.v2";       // dalla 0.76: "id1|id2|..." dei livelli superati
        const string MigratedKey = "gotcha.completed.migrated";
        const string EggsKey = "gotcha.eggs";
        const string NoAdsKey = "gotcha.noads";

        static HashSet<string> completed;

        /// <summary>Pubblicità rimossa (acquisto da 4,99 €). Il negozio la ripristina comunque a ogni avvio.</summary>
        public static bool AdsRemoved
        {
            get => PlayerPrefs.GetInt(NoAdsKey, 0) == 1;
            set { PlayerPrefs.SetInt(NoAdsKey, value ? 1 : 0); PlayerPrefs.Save(); }
        }

        static HashSet<string> Completed
        {
            get
            {
                if (completed != null) return completed;
                completed = ProgressLogic.Parse(PlayerPrefs.GetString(CompletedKey, ""));
                if (PlayerPrefs.GetInt(MigratedKey, 0) == 0)
                {
                    // primo avvio con la 0.76: i livelli superati col vecchio ordine restano superati
                    foreach (string id in ProgressLogic.MigrateFromIndex(PlayerPrefs.GetInt(UnlockedKey, 0), LegacyOrder.V075))
                        completed.Add(id);
                    PlayerPrefs.SetString(CompletedKey, ProgressLogic.Join(completed));
                    PlayerPrefs.SetInt(MigratedKey, 1);
                    PlayerPrefs.Save();
                }
                return completed;
            }
        }

        public static bool IsCompleted(string levelId) => !string.IsNullOrEmpty(levelId) && Completed.Contains(levelId);

        public static int CompletedCount => Completed.Count;

        public static void MarkCompleted(string levelId)
        {
            if (string.IsNullOrEmpty(levelId) || !Completed.Add(levelId)) return;
            PlayerPrefs.SetString(CompletedKey, ProgressLogic.Join(completed));
            PlayerPrefs.Save();
        }

        public static bool HasEasterEgg(string id)
        {
            return ("|" + PlayerPrefs.GetString(EggsKey, "") + "|").Contains("|" + id + "|");
        }

        /// <summary>Gli Easter egg trovati, nell'ordine in cui sono stati scoperti (il k-esimo sblocca il pezzo con rank k).</summary>
        public static string[] EasterEggIds
        {
            get
            {
                string s = PlayerPrefs.GetString(EggsKey, "");
                return string.IsNullOrEmpty(s) ? new string[0] : s.Split('|');
            }
        }

        public static int EasterEggCount
        {
            get
            {
                string s = PlayerPrefs.GetString(EggsKey, "");
                return string.IsNullOrEmpty(s) ? 0 : s.Split('|').Length;
            }
        }

        /// <summary>Riparte da zero: livelli bloccati (tranne il primo) ed Easter egg dimenticati. Le vite restano.</summary>
        public static void ResetProgress()
        {
            PlayerPrefs.DeleteKey(UnlockedKey);
            PlayerPrefs.DeleteKey(CompletedKey);
            PlayerPrefs.SetInt(MigratedKey, 1);   // niente da convertire dopo un azzeramento
            PlayerPrefs.DeleteKey(EggsKey);
            PlayerPrefs.Save();
            completed = new HashSet<string>();
        }

        public static void AddEasterEgg(string id)
        {
            if (string.IsNullOrEmpty(id) || HasEasterEgg(id)) return;
            string s = PlayerPrefs.GetString(EggsKey, "");
            PlayerPrefs.SetString(EggsKey, string.IsNullOrEmpty(s) ? id : s + "|" + id);
            PlayerPrefs.Save();
        }
    }
}
