using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace Gotcha.Core
{
    /// <summary>
    /// Prezzi del negozio nella valuta del giocatore. Il listino è in euro (StoreManager); la valuta si ricava dalla
    /// regione del telefono e il prezzo convertito viene arrotondato al prezzo "da negozio" più vicino (0,99 / 160 / 1400...).
    /// Il cambio è indicativo: con Google Play attivo vale il prezzo vero del negozio (già nella valuta dell'utente) e
    /// qui si sistema solo la scrittura. Il font del gioco disegna male i simboli non ASCII (€, £, ¥...): si scrive il
    /// codice della valuta (EUR, GBP, JPY); il dollaro USA resta "$".
    /// </summary>
    public static class LocalPrice
    {
        /// <summary>Quante unità della valuta vale 1 euro (circa, ottobre 2026).</summary>
        static readonly Dictionary<string, double> PerEuro = new Dictionary<string, double>
        {
            { "EUR", 1 }, { "USD", 1.08 }, { "GBP", 0.85 }, { "CHF", 0.95 }, { "JPY", 160 }, { "CNY", 7.8 },
            { "INR", 90 }, { "BRL", 5.9 }, { "CAD", 1.48 }, { "AUD", 1.65 }, { "NZD", 1.8 }, { "MXN", 19.5 },
            { "PLN", 4.3 }, { "SEK", 11.5 }, { "NOK", 11.6 }, { "DKK", 7.46 }, { "CZK", 25 }, { "HUF", 395 },
            { "RON", 4.97 }, { "BGN", 1.96 }, { "TRY", 37 }, { "RUB", 100 }, { "UAH", 44 }, { "KRW", 1450 },
            { "ZAR", 20 }, { "IDR", 17500 }, { "PHP", 62 }, { "THB", 38 }, { "VND", 27000 }, { "MYR", 5 },
            { "SGD", 1.45 }, { "HKD", 8.4 }, { "TWD", 34 }, { "SAR", 4.05 }, { "AED", 3.97 }, { "ILS", 4 },
            { "EGP", 52 }, { "NGN", 1650 }, { "KZT", 520 }, { "COP", 4400 }, { "CLP", 1000 }, { "PEN", 4.05 },
            { "ARS", 1100 }, { "PKR", 300 }, { "BDT", 130 }, { "MAD", 10.8 }, { "QAR", 3.93 }, { "KES", 140 },
        };

        /// <summary>Valute che si scrivono col punto decimale (le altre con la virgola).</summary>
        static readonly HashSet<string> DotDecimal = new HashSet<string>
        {
            "USD", "GBP", "CHF", "CNY", "INR", "CAD", "AUD", "NZD", "MXN", "ILS", "SGD", "HKD", "MYR", "PHP", "THB",
            "TWD", "SAR", "AED", "EGP", "NGN", "PKR", "BDT", "QAR", "KES", "PEN",
        };

        /// <summary>Valute senza decimali (yen, won...): sotto 100 si arrotonda alla decina.</summary>
        static readonly HashSet<string> NoDecimals = new HashSet<string> { "JPY", "KRW", "IDR", "VND", "CLP", "COP", "HUF", "KZT" };

        static string currency;

        /// <summary>Codice della valuta del giocatore (EUR se non si riesce a capirla o se non è in tabella).</summary>
        public static string Currency
        {
            get
            {
                if (currency != null) return currency;
                string c = DetectCurrency();
                currency = c != null && PerEuro.ContainsKey(c) ? c : "EUR";
                return currency;
            }
        }

        /// <summary>Prezzo di listino (in euro) già convertito e scritto per il giocatore.</summary>
        public static string FromEuro(double euro) => Format(Convert(euro, Currency), Currency);

        /// <summary>Conversione e arrotondamento al prezzo da negozio più vicino.</summary>
        public static double Convert(double euro, string code)
        {
            double rate = PerEuro.TryGetValue(code ?? "EUR", out double r) ? r : 1;
            double v = euro * rate;
            if (code != null && NoDecimals.Contains(code) && v < 100) return Math.Max(10, Math.Round(v / 10) * 10);
            return Nice(v);
        }

        /// <summary>Arrotonda a un prezzo "da cartellino": 0,x9 sotto 10, x,99 sotto 100, poi decine / centinaia / migliaia.</summary>
        public static double Nice(double v)
        {
            if (v <= 0) return 0;
            if (v < 10) return Math.Max(0.19, Math.Round(v * 10) / 10 - 0.01);
            if (v < 100) return Math.Round(v) - 0.01;
            if (v < 1000) return Math.Round(v / 10) * 10;
            if (v < 10000) return Math.Round(v / 100) * 100;
            return Math.Round(v / 1000) * 1000;
        }

        /// <summary>Scrittura leggibile con il font del gioco: "0,99 EUR", "$0.99", "160 JPY".</summary>
        public static string Format(double amount, string code)
        {
            bool whole = amount >= 100 || Math.Abs(amount - Math.Round(amount)) < 0.001;
            string n = amount.ToString(whole ? "0" : "0.00", CultureInfo.InvariantCulture);
            if (!whole && !DotDecimal.Contains(code)) n = n.Replace('.', ',');
            return code == "USD" ? "$" + n : n + " " + code;
        }

        /// <summary>
        /// Prezzo arrivato da Google Play (già nella valuta giusta): si sostituiscono solo i simboli che il font non
        /// disegna bene con il codice della valuta.
        /// </summary>
        public static string Readable(string storePrice)
        {
            if (string.IsNullOrEmpty(storePrice)) return storePrice;
            string s = storePrice.Replace(' ', ' ').Trim();
            foreach (var kv in new[] { ("€", "EUR"), ("£", "GBP"), ("¥", "JPY"), ("₹", "INR"), ("₩", "KRW"), ("₺", "TRY"),
                                       ("₽", "RUB"), ("₴", "UAH"), ("₱", "PHP"), ("฿", "THB"), ("₫", "VND"), ("₪", "ILS"),
                                       ("zł", "PLN"), ("₦", "NGN"), ("₸", "KZT") })
            {
                if (!s.Contains(kv.Item1)) continue;
                s = (s.Replace(kv.Item1, "").Trim() + " " + kv.Item2).Trim();
            }
            return s;
        }

        static string DetectCurrency()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (var locale = new AndroidJavaClass("java.util.Locale"))
                using (var def = locale.CallStatic<AndroidJavaObject>("getDefault"))
                using (var cur = new AndroidJavaClass("java.util.Currency"))
                using (var inst = cur.CallStatic<AndroidJavaObject>("getInstance", def))
                    return inst.Call<string>("getCurrencyCode");
            }
            catch { }
#endif
            try { return new RegionInfo(CultureInfo.CurrentCulture.Name).ISOCurrencySymbol; }
            catch { return null; }
        }
    }
}
