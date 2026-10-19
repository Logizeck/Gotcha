using System.Collections.Generic;
using UnityEngine;

namespace Gotcha.Core
{
    /// <summary>
    /// Il catalogo degli Easter egg: ogni segreto ha un nome (mostrato quando lo trovi e nel messaggio di
    /// condivisione). Quando se ne aggiunge uno nuovo in un livello, va dato un nome anche qui.
    /// Il video di un Easter egg si può condividere solo quando il segreto viene trovato per la prima volta (e mai più
    /// dopo averlo condiviso); condividerlo dà un premio a scelta: 3 minuti di vite infinite o un hint gratis.
    /// </summary>
    public static class EasterEggs
    {
        public const float RewardInfiniteSeconds = 3f * 60f;
        public const int RewardHints = 1;
        const string SharedKey = "gotcha.eggs.shared";
        const string OfferedKey = "gotcha.eggs.offered";

        static readonly Dictionary<string, string> Names = new Dictionary<string, string>
        {
            { "snowman_butt", "Carrot Bum" },
            { "candle_storm", "Hurricane Wish" },
            { "mecha_robot", "Mecha Mode" },
            { "hot_soup", "Toasted Mouse" },
            { "angry_bird", "Angry Bird" },
            { "spicy_pizza", "Inferno Pizza" },
            { "burnt_umbrella", "Toasted Umbrella" },
            { "bad_breath", "Bad Breath" },
            { "dizzy_cat", "Dizzy Cat" },
            { "lunch_time", "Lunch Time" },
            { "crane_perfect", "Master Builder" },
            { "glass_smash", "Shattered Screen" },
            { "gravity_resigned", "Gravity Resigned" },
            { "pixel_rebellion", "Pixel Rebellion" },
            { "dirty_secret", "The Dirty Secret" },
            { "adpocalypse", "Adpocalypse" },
            { "mouse_union", "Mouse Union" },
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

        /// <summary>
        /// La condivisione si propone una volta sola: alla prima scoperta del segreto (se non è già stato condiviso).
        /// Restituisce true se va proposta adesso, e la segna come proposta.
        /// </summary>
        public static bool OfferShareOnce(string id)
        {
            if (string.IsNullOrEmpty(id) || RewardClaimed(id)) return false;
            string s = PlayerPrefs.GetString(OfferedKey, "");
            if (("|" + s + "|").Contains("|" + id + "|")) return false;
            PlayerPrefs.SetString(OfferedKey, string.IsNullOrEmpty(s) ? id : s + "|" + id);
            PlayerPrefs.Save();
            return true;
        }

        public static void MarkRewardClaimed(string id)
        {
            if (string.IsNullOrEmpty(id) || RewardClaimed(id)) return;
            string s = PlayerPrefs.GetString(SharedKey, "");
            PlayerPrefs.SetString(SharedKey, string.IsNullOrEmpty(s) ? id : s + "|" + id);
            PlayerPrefs.Save();
        }
    }
}
