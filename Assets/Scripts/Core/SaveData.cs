using UnityEngine;

namespace Gotcha.Core
{
    /// <summary>Salvataggio minimo: livelli sbloccati ed Easter egg trovati.</summary>
    public static class SaveData
    {
        const string UnlockedKey = "gotcha.unlocked";
        const string EggsKey = "gotcha.eggs";

        public static int HighestUnlocked => PlayerPrefs.GetInt(UnlockedKey, 0);

        public static void UnlockUpTo(int index)
        {
            if (index <= HighestUnlocked) return;
            PlayerPrefs.SetInt(UnlockedKey, index);
            PlayerPrefs.Save();
        }

        public static bool HasEasterEgg(string id)
        {
            return ("|" + PlayerPrefs.GetString(EggsKey, "") + "|").Contains("|" + id + "|");
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
            PlayerPrefs.DeleteKey(EggsKey);
            PlayerPrefs.Save();
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
