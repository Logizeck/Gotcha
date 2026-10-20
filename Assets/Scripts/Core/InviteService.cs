using System;
using UnityEngine;

namespace Gotcha.Core
{
    /// <summary>
    /// Inviti agli amici. Condividere l'invito funziona già (menu di condivisione di Android con link e codice).
    /// Sapere che l'amico ha ACCETTATO richiede un server: il telefono di chi invita non può saperlo da solo.
    /// Quando ci sarà (es. Firebase + Play Install Referrer), il server chiamerà <see cref="OnFriendAccepted"/>
    /// e chi ha invitato riceverà la vita. Tutto il resto del gioco è già pronto per questo.
    /// </summary>
    public static class InviteService
    {
        const string CodeKey = "gotcha.inviteCode";
        /// <summary>Nome del pacchetto su Google Play (unico, NON si può più cambiare dopo il primo caricamento).
        /// Lo usa anche il builder per le impostazioni Android.</summary>
        public const string PackageName = "com.gotcha.zeck";
        public const string StoreUrl = "https://play.google.com/store/apps/details?id=" + PackageName;

        /// <summary>Codice personale (casuale, generato una volta) che identifica chi invita.</summary>
        public static string MyCode
        {
            get
            {
                string c = PlayerPrefs.GetString(CodeKey, "");
                if (string.IsNullOrEmpty(c))
                {
                    const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
                    var r = new System.Random(Guid.NewGuid().GetHashCode());
                    char[] a = new char[6];
                    for (int i = 0; i < a.Length; i++) a[i] = chars[r.Next(chars.Length)];
                    c = new string(a);
                    PlayerPrefs.SetString(CodeKey, c);
                    PlayerPrefs.Save();
                }
                return c;
            }
        }

        public static string InviteLink => StoreUrl + "&referrer=utm_source%3Dinvite%26utm_content%3D" + MyCode;

        /// <summary>
        /// Link personale per scaricare il gioco, da mettere nei video condivisi: contiene il codice di chi condivide
        /// (unico per giocatore) e il segreto trovato, così un domani il server può premiare chi porta nuovi giocatori.
        /// </summary>
        public static string ShareLink(string eggId) =>
            StoreUrl + "&referrer=utm_source%3Dshare%26utm_content%3D" + MyCode + "%26utm_campaign%3D" + Uri.EscapeDataString(eggId ?? "");

        public static void Share()
        {
            string text = "Can you beat me at Gotcha!? Every level is a trick. Download it: " + InviteLink;
#if UNITY_ANDROID && !UNITY_EDITOR
            try { using (var share = new AndroidJavaClass("com.gotcha.video.GotchaVideo")) share.CallStatic<bool>("shareText", text, "Invite a friend"); }
            catch (Exception e) { Debug.LogWarning("[Gotcha] Invito non condiviso: " + e.Message); }
#else
            GUIUtility.systemCopyBuffer = text;
            Debug.Log("[Gotcha] Invito (copiato negli appunti): " + text);
#endif
        }

        /// <summary>Da chiamare quando il server conferma che un amico ha accettato l'invito: una vita in regalo.</summary>
        public static void OnFriendAccepted(int friends = 1)
        {
            if (LivesManager.Instance != null) LivesManager.Instance.GrantLife(friends);
        }
    }
}
