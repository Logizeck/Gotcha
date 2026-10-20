using System;
using UnityEngine;

namespace Gotcha.Core
{
    /// <summary>
    /// Obiettivo giornaliero: chi gioca ogni giorno e supera almeno 5 livelli riceve un premio. I giorni consecutivi
    /// formano una serie (streak) e il premio cresce fino al 7° giorno, poi il ciclo ricomincia (la serie continua).
    /// Saltare un giorno azzera la serie. Il giorno è quello locale del telefono.
    /// </summary>
    public static class DailyReward
    {
        public const int LevelsNeeded = 5;

        const string DayKey = "gotcha.daily.day";          // giorno (yyyyMMdd) a cui si riferisce il contatore
        const string CountKey = "gotcha.daily.count";      // livelli superati oggi
        const string DoneKey = "gotcha.daily.done";        // premio di oggi già dato (1/0)
        const string StreakKey = "gotcha.daily.streak";    // giorni consecutivi con l'obiettivo raggiunto
        const string LastDoneKey = "gotcha.daily.lastdone"; // ultimo giorno (yyyyMMdd) con l'obiettivo raggiunto

        /// <summary>Premio appena guadagnato: (giorno della serie, testo del premio).</summary>
        public static event Action<int, string> Earned;

        static string Today => DateTime.Now.ToString("yyyyMMdd");
        static string Yesterday => DateTime.Now.AddDays(-1).ToString("yyyyMMdd");

        static void Roll()
        {
            if (PlayerPrefs.GetString(DayKey, "") == Today) return;
            PlayerPrefs.SetString(DayKey, Today);
            PlayerPrefs.SetInt(CountKey, 0);
            PlayerPrefs.SetInt(DoneKey, 0);
            PlayerPrefs.Save();
        }

        public static int LevelsToday { get { Roll(); return PlayerPrefs.GetInt(CountKey, 0); } }
        public static bool DoneToday { get { Roll(); return PlayerPrefs.GetInt(DoneKey, 0) == 1; } }

        /// <summary>Serie attuale: conta solo se l'ultimo obiettivo raggiunto è di oggi o di ieri.</summary>
        public static int Streak
        {
            get
            {
                string last = PlayerPrefs.GetString(LastDoneKey, "");
                return last == Today || last == Yesterday ? PlayerPrefs.GetInt(StreakKey, 0) : 0;
            }
        }

        /// <summary>Il premio per il giorno n della serie (ciclo di 7 giorni).</summary>
        public static string RewardText(int day)
        {
            switch ((day - 1) % 7 + 1)
            {
                case 1: return "1 free hint";
                case 2: return "5 minutes of infinite lives";
                case 3: return "2 free hints";
                case 4: return "10 minutes of infinite lives";
                case 5: return "3 free hints";
                case 6: return "15 minutes of infinite lives";
                default: return "5 free hints + 30 minutes of infinite lives";
            }
        }

        static void Grant(int day)
        {
            switch ((day - 1) % 7 + 1)
            {
                case 1: HintWallet.Add(1); break;
                case 2: Infinite(5f); break;
                case 3: HintWallet.Add(2); break;
                case 4: Infinite(10f); break;
                case 5: HintWallet.Add(3); break;
                case 6: Infinite(15f); break;
                default: HintWallet.Add(5); Infinite(30f); break;
            }
        }

        static void Infinite(float minutes)
        {
            if (LivesManager.Instance != null) LivesManager.Instance.GrantInfinite(minutes * 60f);
        }

        /// <summary>Da chiamare a ogni livello superato.</summary>
        public static void RegisterSolved()
        {
            Roll();
            int count = PlayerPrefs.GetInt(CountKey, 0) + 1;
            PlayerPrefs.SetInt(CountKey, count);
            if (count >= LevelsNeeded && PlayerPrefs.GetInt(DoneKey, 0) == 0)
            {
                int day = Streak + 1;
                PlayerPrefs.SetInt(DoneKey, 1);
                PlayerPrefs.SetInt(StreakKey, day);
                PlayerPrefs.SetString(LastDoneKey, Today);
                Grant(day);
                PlayerPrefs.Save();
                Earned?.Invoke(day, RewardText(day));
                return;
            }
            PlayerPrefs.Save();
        }

        /// <summary>Riga per la schermata del titolo / dei risultati.</summary>
        public static string Status()
        {
            int streak = Streak;
            string s = streak > 1 ? " - streak: " + streak + " days" : streak == 1 ? " - streak: 1 day" : "";
            if (DoneToday) return "Daily goal done! Come back tomorrow for: " + RewardText(streak + 1) + s;
            return "Daily goal: beat " + LevelsNeeded + " levels (" + LevelsToday + "/" + LevelsNeeded + "). Reward: " + RewardText(streak + 1) + s;
        }
    }
}
