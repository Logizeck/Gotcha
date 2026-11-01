#if UNITY_EDITOR
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels;

/// <summary>
/// Festa di vittoria comune dei livelli nuovi (roadmap 1-300, dalla 0.76): RewardRig costruisce sotto la radice del
/// livello un LevelReward già collegato (complimento, bagliore neon, raggi, coriandoli). Il livello lo riceve con
/// SetField(level, "reward", RewardRig(root.transform)) e a vittoria fa  yield return reward.Play(testo); Win(testo);
/// </summary>
public static partial class GotchaProjectBuilder
{
    static LevelReward RewardRig(Transform root, Vector2 at = default, Color? neon = null)
    {
        Transform holder = Child(root, "Reward", at == default ? new Vector2(0f, 1.2f) : at).transform;
        SpriteRenderer rays = Art(holder, "Rays", "burst_rays", 60, false).GetComponent<SpriteRenderer>();
        rays.transform.localScale = Vector3.one * 2.6f;
        rays.color = new Color(1f, 0.95f, 0.6f, 0f);
        SpriteRenderer glow = Art(holder, "Glow", "glow", 61, false).GetComponent<SpriteRenderer>();
        glow.color = new Color(0.35f, 1f, 0.75f, 0f);
        Bar(holder, "Back", Vector2.zero, new Vector2(5.2f, 0.95f), new Color(0.08f, 0.05f, 0.14f, 0.82f), 62);
        TextMesh text = Label(holder, Vector2.zero, 63, "", 0.07f);
        text.color = Color.white;
        text.fontStyle = FontStyle.Bold;
        Fit(text, 4.9f, 0.8f);
        holder.gameObject.SetActive(false);

        LevelReward reward = root.gameObject.AddComponent<LevelReward>();
        SetField(reward, "holder", holder);
        SetField(reward, "compliment", text);
        SetField(reward, "glow", glow);
        SetField(reward, "rays", rays);
        SetArray(reward, "confetti", HW_Confetti(root));
        if (neon.HasValue)
        {
            var so = new UnityEditor.SerializedObject(reward);
            so.FindProperty("neon").colorValue = neon.Value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
        return reward;
    }
}
#endif
