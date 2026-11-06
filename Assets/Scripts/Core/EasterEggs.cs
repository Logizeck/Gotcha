using System.Collections.Generic;
using UnityEngine;

namespace Gotcha.Core
{
    /// <summary>
    /// Il catalogo degli Easter egg: ogni segreto ha un nome (mostrato quando lo trovi e nel messaggio di
    /// condivisione). Quando se ne aggiunge uno nuovo in un livello, va dato un nome anche qui.
    /// Dalla 0.83 un segreto trovato si può condividere ogni volta: il video subito dopo averlo trovato (o ritrovato),
    /// il testo con il link più tardi dal puzzle di Toni (toccando il suo pezzo). Il premio a scelta (3 minuti di vite
    /// infinite o un hint gratis) resta UNO per segreto (<see cref="RewardClaimed"/>) e arriva solo se la condivisione
    /// risulta avvenuta (VerifiedShare).
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
            { "true_monster_pug", "The Real Monster Is The Pug" },
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
        /// (Fino alla 0.82) la condivisione si proponeva una volta sola, alla prima scoperta del segreto. Dalla 0.83
        /// GameUI non la usa più (il pulsante c'è sempre); resta per compatibilità con i dati salvati.
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
