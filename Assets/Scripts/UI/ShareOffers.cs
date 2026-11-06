using UnityEngine;
using UnityEngine.UI;
using Gotcha.Components;
using Gotcha.Core;
using Gotcha.Levels;

namespace Gotcha.UI
{
    /// <summary>
    /// (0.83) Le condivisioni dell'esito del livello e del puzzle di Toni:
    ///  - Easter egg: il pulsante di condivisione c'è OGNI volta che l'esito segue un segreto (il video degli ultimi
    ///    secondi se la clip c'è, altrimenti testo + link). Il premio a scelta resta uno per segreto
    ///    (EasterEggs.RewardClaimed) e arriva solo se la condivisione risulta avvenuta (VerifiedShare).
    ///  - Più tardi, dal puzzle di Toni: toccando un pezzo scoperto si condivide il segreto che lo ha sbloccato (solo
    ///    testo + link: il video esiste solo subito dopo averlo trovato).
    ///  - "LIKED IT? SHARE WITH A FRIEND: +2 LIVES" sull'esito SOLVED dei livelli memorabili (con la festa LevelReward)
    ///    o ogni 5 vittorie, solo se le vite non sono piene né infinite e se oggi non si sono già presi 3 premi
    ///    (ViralShareRules). Le 2 vite arrivano solo con la condivisione verificata, fino al massimo normale.
    /// </summary>
    public partial class GameUI
    {
        Button viralButton;
        Text viralLabel;
        bool shareUsesClip;

        void BuildViralButton(RectTransform resultCard)
        {
            viralButton = CreateButton(resultCard, "LIKED IT? SHARE WITH\nA FRIEND: +2 LIVES", EggColor, ShareWithFriend);
            viralLabel = viralButton.GetComponentInChildren<Text>();
            viralLabel.fontSize = 44;
            viralLabel.resizeTextMaxSize = 44;
            viralLabel.lineSpacing = 0.9f;
            Place(viralButton.transform, new Vector2(0.1f, 0.27f), new Vector2(0.9f, 0.41f));
            viralButton.gameObject.SetActive(false);
        }

        /// <summary>Quali pulsanti di condivisione mostrare sull'esito appena aperto.</summary>
        void SetupShareOffers(bool solved, string eggId)
        {
            bool egg = !string.IsNullOrEmpty(eggId);
            shareUsesClip = egg && ClipRecorder.Instance != null && ClipRecorder.Instance.HasClip;
            shareButton.gameObject.SetActive(egg);
            shareButton.interactable = true;
            if (egg)
            {
                bool reward = !EasterEggs.RewardClaimed(eggId);
                shareLabel.text = (shareUsesClip ? "SHARE VIDEO" : "SHARE THE SECRET") + (reward ? " + REWARD" : "");
                if (reward) resultBody.text += "\nShare it: 3 min of infinite lives or 1 free hint!";
            }

            int solvedCounter = solved ? ViralShare.NoteSolved() : 0;
            bool memorable = solved && flow.Current != null && flow.Current.GetComponentInChildren<LevelReward>(true) != null;
            LivesManager lm = flow.Lives;
            bool offer = lm != null && ViralShareRules.ShouldOffer(solved, memorable, solvedCounter, lm.Lives, LivesManager.MaxLives,
                                                                   lm.Infinite, ViralShare.UsedToday, egg);
            viralButton.gameObject.SetActive(offer);
            viralButton.interactable = true;
            viralLabel.text = "LIKED IT? SHARE WITH\nA FRIEND: +" + ViralShareRules.RewardLives + " LIVES";
        }

        /// <summary>Condivide il segreto dell'esito (video se c'è, se no testo + link); si può ripetere quante volte si vuole.</summary>
        void ShareClip()
        {
            if (string.IsNullOrEmpty(lastEggId) || VerifiedShare.Busy) return;
            if (shareUsesClip && (ClipRecorder.Instance == null || ClipRecorder.Instance.IsSharing)) return;
            string eggId = lastEggId;
            shareButton.interactable = false;
            shareLabel.text = shareUsesClip ? "PREPARING..." : "SHARING...";
            System.Action<bool> done = ok =>
            {
                if (shareButton == null) return;
                // il premio si sceglie una volta per ogni segreto, e solo se la condivisione risulta avvenuta
                if (ok && !EasterEggs.RewardClaimed(eggId))
                {
                    EasterEggs.MarkRewardClaimed(eggId);
                    lastEggId = eggId;
                    rewardPanel.SetActive(true);
                }
                shareLabel.text = ok ? "SHARE AGAIN" : "NOT SHARED - TRY AGAIN";
                shareButton.interactable = true;
            };
            // nome del segreto + link personale per scaricare il gioco
            if (shareUsesClip) ClipRecorder.Instance.Share(EasterEggs.ShareText(eggId), done);
            else VerifiedShare.ShareText(EasterEggs.ShareText(eggId), "Share the secret", done);
        }

        /// <summary>"Ti è piaciuto? Condividilo con un amico": 2 vite solo a condivisione verificata.</summary>
        void ShareWithFriend()
        {
            if (VerifiedShare.Busy) return;
            viralButton.interactable = false;
            viralLabel.text = "SHARING...";
            VerifiedShare.ShareText(ViralShare.Text, "Share with a friend", ok =>
            {
                if (viralButton == null) return;
                LivesManager lm = flow.Lives;
                if (ok && lm != null && !ViralShare.CapReached)
                {
                    lm.GrantLife(ViralShareRules.RewardLives);   // fino al massimo normale
                    ViralShare.RecordReward();
                    viralLabel.text = "THANKS! +" + ViralShareRules.RewardLives + " LIVES";
                    Haptics.Success();
                    return;   // resta spento: un premio per esito
                }
                viralLabel.text = ok ? "THANKS FOR SHARING!" : "NOT SHARED - TRY AGAIN";
                viralButton.interactable = !ok;
            });
        }

        /// <summary>Dal puzzle di Toni: condivide (testo + link) il segreto che ha sbloccato il pezzo.</summary>
        void SharePuzzlePiece(int eggIndex)
        {
            string[] ids = SaveData.EasterEggIds;
            if (eggIndex < 0 || eggIndex >= ids.Length || VerifiedShare.Busy) return;
            string eggId = ids[eggIndex];
            string name = "\"" + EasterEggs.Name(eggId) + "\"";
            puzzleInfo.text = "Sharing " + name + "...";
            VerifiedShare.ShareText(EasterEggs.ShareText(eggId), "Share the secret", ok =>
            {
                if (puzzleInfo == null) return;
                if (ok && !EasterEggs.RewardClaimed(eggId))
                {
                    // il premio di questo segreto non era ancora stato preso: arriva adesso (sempre uno per segreto)
                    EasterEggs.MarkRewardClaimed(eggId);
                    lastEggId = eggId;
                    rewardPanel.SetActive(true);
                }
                puzzleInfo.text = ok ? "Shared " + name + "!\nTap a piece to share its secret." : "Not shared.\nTap a piece to try again.";
            });
        }
    }
}
