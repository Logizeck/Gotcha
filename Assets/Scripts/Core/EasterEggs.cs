using System.Collections.Generic;
using UnityEngine;

namespace Gotcha.Core
{
    /// <summary>
    /// Il catalogo degli Easter egg: ogni segreto ha un nome (mostrato quando lo trovi e nel messaggio di
    /// condivisione). Quando se ne aggiunge uno nuovo in un livello, va dato un nome anche qui.
    /// Condividere il video di un Easter egg dà un premio a scelta (15 minuti di vite infinite o 5 hint), una volta
    /// per ogni segreto.
    /// </summary>
    public static class EasterEggs
    {
        public const float RewardInfiniteSeconds = 15f * 60f;
        public const int RewardHints = 5;
        const string SharedKey = "gotcha.eggs.shared";

        static readonly Dictionary<string, string> Names = new Dictionary<string, string>
        {
            { "snowman_butt", "Carrot Bum" },
            { "candle_storm", "Hurricane Wish" },
            { "mecha_robot", "Mecha Mode" },
            { "hot_soup", "Toasted Mouse" },
            { "toni_toot", "Toni Rocket" },
            { "angry_bird", "Angry Bird" },
            { "sun_melt", "Meltdown" },
            { "spicy_pizza", "Inferno Pizza" },
            { "toni_tug", "Toni Wins" },
            { "burnt_umbrella", "Toasted Umbrella" },
            { "bad_breath", "Bad Breath" },
            { "dizzy_cat", "Dizzy Cat" },
            { "lunch_time", "Lunch Time" },
            { "speedrun", "Speedrunner" },
            { "cat_bonk", "Cat Bonk" },
            { "credits_skip", "Credits Skipper" },
        };

        public static string Name(string id)
        {
            if (string.IsNullOrEmpty(id)) return "";
            return Names.TryGetValue(id, out string n) ? n : id.Replace('_', ' ').ToUpperInvariant();
        }

        /// <summary>Testo da condividere insieme al video: nome del segreto e link (personale) per scaricare il gioco.</summary>
        public static string ShareText(string id) =>
            "I unlocked the secret \"" + Name(id) + "\" in Gotcha! Can you find it? Play free: " + InviteService.ShareLink(id);

        /// <summary>Il premio per la condivisione di questo segreto è già stato riscattato?</summary>
        public static bool RewardClaimed(string id) => ("|" + PlayerPrefs.GetString(SharedKey, "") + "|").Contains("|" + id + "|");

        public static void MarkRewardClaimed(string id)
        {
            if (string.IsNullOrEmpty(id) || RewardClaimed(id)) return;
            string s = PlayerPrefs.GetString(SharedKey, "");
            PlayerPrefs.SetString(SharedKey, string.IsNullOrEmpty(s) ? id : s + "|" + id);
            PlayerPrefs.Save();
        }
    }
}
