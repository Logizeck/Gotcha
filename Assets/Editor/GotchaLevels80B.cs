#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.EditorArt;
using Gotcha.Levels;

/// <summary>
/// Versione 0.80, livello 30 della roadmap (gruppo B): The Invisible Treasure. Lo sfondo (cielo, mare, isoletta di sabbia),
/// la palma, la mappa del tesoro in alto a sinistra (un foglio piccolo nel mondo, senza X disegnata) con sopra la X rossa
/// trascinabile ("v40b_x_mark", nel suo posto sulla mappa che indica il terreno accanto alla palma), la pala (origine
/// sulla punta) con l'anello tratteggiato che segna dove scava, le buche ("v40b_sand_hole", riciclate), le cianfrusaglie
/// (scarpone "fish_boot", lattina "tin_can", granchio), gli sbuffi di sabbia ("puff"), il baule ("chest"/"chest_open") con
/// le monete ("coin") e le scintille ("sparkle_star"), la riga dell'obiettivo e la scritta di commento (aiuti di Halloween,
/// ognuna col suo Fit) e la festa comune (RewardRig, alzata sopra la mappa: non copre mai il baule). Disegni nuovi
/// (prefisso v80_b_) in DoodleSpritesV80B. Niente Easter egg e niente Toni. L'elenco dei livelli (Define) lo aggiunge il
/// capo progetto.
/// </summary>
public static partial class GotchaProjectBuilder
{
    // ---------- Roadmap 30: The Invisible Treasure ----------

    static GameObject BuildV80InvisibleTreasure()
    {
        var root = new GameObject("Level_V80InvisibleTreasure");
        V80InvisibleTreasureLevel level = root.AddComponent<V80InvisibleTreasureLevel>();
        Transform r = root.transform;
        Art(r, "Island", "v80_b_island_bg", -20, false);

        // ---- la palma (un tocco: battuta) ----
        Vector2 palmAt = new Vector2(DoodleSprites.V80BPalmX, DoodleSprites.V80BPalmY);
        Transform palm = HW_Obj(r, "Palm", palmAt, "v80_b_palm", 2);
        BoxCollider2D palmCol = HW_Box(palm, new Vector2(1.0f, 2.3f), new Vector2(-0.2f, 1.2f));

        // ---- le buche (riciclate, solo decorazione), le cianfrusaglie e gli sbuffi di sabbia ----
        var holes = new List<Transform>();
        for (int i = 0; i < 5; i++)
        {
            Transform h = HW_Obj(r, "Hole", Vector2.zero, "v40b_sand_hole", 1, 0.8f);
            h.gameObject.SetActive(false);
            holes.Add(h);
        }
        Transform boot = HW_Obj(r, "Boot", Vector2.zero, "fish_boot", 4, 0.75f);
        boot.gameObject.SetActive(false);
        Transform can = HW_Obj(r, "Can", Vector2.zero, "tin_can", 4, 0.7f);
        can.gameObject.SetActive(false);
        Transform crab = HW_Obj(r, "Crab", Vector2.zero, "v80_b_crab", 4, 0.9f);
        crab.gameObject.SetActive(false);
        var puffs = new List<Transform>();
        for (int i = 0; i < 4; i++)
        {
            Transform pf = HW_Obj(r, "SandPuff", Vector2.zero, "puff", 9, 0.45f);
            pf.GetComponentInChildren<SpriteRenderer>().color = new Color(0.96f, 0.84f, 0.58f);
            pf.gameObject.SetActive(false);
            puffs.Add(pf);
        }

        // ---- il baule (origine in basso), le monete e le scintille ----
        Transform chest = Child(r, "Chest", Vector2.zero).transform;
        chest.localScale = Vector3.one * 0.75f;
        Transform chestClosed = Art(chest, "Closed", "chest", 5, false);
        Transform chestOpen = Art(chest, "Open", "chest_open", 5, false);
        chestOpen.gameObject.SetActive(false);
        chest.gameObject.SetActive(false);
        SpriteRenderer chestGlow = Art(r, "ChestGlow", "glow", 4, false).GetComponent<SpriteRenderer>();
        chestGlow.transform.localScale = Vector3.one * 2.6f;
        chestGlow.color = new Color(1f, 0.85f, 0.35f, 0f);
        var coins = new List<Transform>();
        for (int i = 0; i < 6; i++)
        {
            Transform c = HW_Obj(r, "Coin", Vector2.zero, "coin", 6, 0.7f);
            c.gameObject.SetActive(false);
            coins.Add(c);
        }
        var sparkles = new List<Transform>();
        for (int i = 0; i < 6; i++)
        {
            Transform s = HW_Obj(r, "Sparkle", Vector2.zero, "sparkle_star", 7, 0.5f);
            s.GetComponentInChildren<SpriteRenderer>().color = new Color(1f, 0.9f, 0.45f);
            s.gameObject.SetActive(false);
            sparkles.Add(s);
        }

        // ---- la mappa (in alto a sinistra) e la X che ci sta sopra ----
        Vector2 mapAt = new Vector2(-1.15f, 1.7f);
        Transform map = HW_Obj(r, "Map", mapAt, "v80_b_map", 10);
        BoxCollider2D mapCol = HW_Box(map, new Vector2(DoodleSprites.V80BMapHalfW * 2f, DoodleSprites.V80BMapHalfH * 2f));
        float s80 = DoodleSprites.V80BMapScale;
        Vector2 mapXLocal = new Vector2(DoodleSprites.V80BMapIslandX + (DoodleSprites.V80BSpotX - DoodleSprites.V80BIslandX) * s80,
            DoodleSprites.V80BMapIslandY + (DoodleSprites.V80BSpotY - DoodleSprites.V80BIslandY) * s80);
        Transform mapXHome = Child(map, "XHome", mapXLocal).transform;
        Transform xMark = HW_Obj(r, "X", mapAt + mapXLocal, "v40b_x_mark", 12, 0.42f);

        // ---- la pala (origine sulla punta) e l'anello "si scava qui" ----
        Vector2 shovelAt = new Vector2(-1.55f, -2.6f);
        Transform shovel = HW_Obj(r, "Shovel", shovelAt, "v80_b_shovel", 14, 0.9f);
        shovel.localRotation = Quaternion.Euler(0f, 0f, 12f);
        BoxCollider2D shovelCol = HW_Box(shovel, new Vector2(0.95f, 2.3f), new Vector2(0f, 1.05f));
        SpriteRenderer ring = Art(r, "DigRing", "v80_b_dig_ring", 13, false).GetComponent<SpriteRenderer>();
        ring.gameObject.SetActive(false);

        HW_Goal(r, "FIND THE TREASURE.");
        TextMesh info = HW_Toast(r, new Vector2(0f, -4.15f), out Transform infoHolder);
        SetField(level, "palm", palm);
        SetField(level, "palmArea", palmCol);
        SetArray(level, "holes", holes.ToArray());
        SetField(level, "boot", boot);
        SetField(level, "can", can);
        SetField(level, "crab", crab);
        SetArray(level, "puffs", puffs.ToArray());
        SetField(level, "chest", chest);
        SetField(level, "chestClosed", chestClosed);
        SetField(level, "chestOpen", chestOpen);
        SetField(level, "chestGlow", chestGlow);
        SetArray(level, "coins", coins.ToArray());
        SetArray(level, "sparkles", sparkles.ToArray());
        SetField(level, "map", map);
        SetField(level, "mapArea", mapCol);
        SetField(level, "mapXHome", mapXHome);
        SetField(level, "xMark", xMark);
        SetFloat(level, "xMapScale", 0.42f);
        SetFloat(level, "xGroundScale", 0.9f);
        SetField(level, "shovel", shovel);
        SetField(level, "shovelArea", shovelCol);
        SetField(level, "digRing", ring);
        SetFloat(level, "islandX", DoodleSprites.V80BIslandX);
        SetFloat(level, "islandY", DoodleSprites.V80BIslandY);
        SetFloat(level, "islandRx", DoodleSprites.V80BIslandRx);
        SetFloat(level, "islandRy", DoodleSprites.V80BIslandRy);
        SetFloat(level, "mapHalfW", DoodleSprites.V80BMapHalfW);
        SetFloat(level, "mapHalfH", DoodleSprites.V80BMapHalfH);
        SetFloat(level, "spotX", DoodleSprites.V80BSpotX);
        SetFloat(level, "spotY", DoodleSprites.V80BSpotY);
        SetFloat(level, "horizon", DoodleSprites.V80BHorizon);
        SetField(level, "infoText", info);
        SetField(level, "infoHolder", infoHolder);
        SetField(level, "reward", RewardRig(r, new Vector2(0f, 2.1f)));
        return SaveLevel(root, "V80InvisibleTreasure");
    }
}

public static partial class LevelHints
{
    static readonly Dictionary<string, string[]> V80B = new Dictionary<string, string[]>
    {
        { "invisible_treasure", new[] { "The X on the map can be moved.",
            "Drag the X from the map onto the sand, then dig there." } },
    };
}
#endif
