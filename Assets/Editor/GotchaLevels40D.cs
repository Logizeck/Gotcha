#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels;

/// <summary>Livelli compositi della versione 0.40 (gruppo D).</summary>
public static partial class GotchaProjectBuilder
{
    static List<LevelDefinition> Levels40D() => new List<LevelDefinition>
    {
        Define("Seasons", "garden_seasons", "GARDEN SEASONS", "One garden, four seasons. Read the goal at the top.", BuildSeasonsLevel()),
        Define("Museum", "museum_night", "MUSEUM NIGHT", "Stay out of the flashlight beam.", BuildMuseumLevel()),
        Define("IceCream", "ice_cream_shop", "ICE CREAM SHOP", "Read the ticket: the bottom scoop goes on first.", BuildIceCreamLevel()),
        Define("Cannon", "pirate_cannon", "PIRATE CANNON", "Watch the windsock before you fire.", BuildCannonLevel()),
    };

    // ---------- aiuti comuni del gruppo D ----------

    /// <summary>HUD delle fasi: pallini in alto, riga dell'obiettivo e scritta grande di passaggio.</summary>
    static StageHud40D V40D_Hud(Transform root, int stages)
    {
        Transform hud = Child(root, "Hud", Vector2.zero).transform;
        StageHud40D h = hud.gameObject.AddComponent<StageHud40D>();
        var dots = new List<SpriteRenderer>();
        for (int i = 0; i < stages; i++)
        {
            Transform d = Art(hud, "Dot" + i, "d40_dot", 52, false, new Vector2((i - (stages - 1) * 0.5f) * 0.42f, 3.42f));
            d.localScale = Vector3.one * 0.32f;
            dots.Add(d.GetComponent<SpriteRenderer>());
        }
        Bar(hud, "GoalBack", new Vector2(0f, 2.98f), new Vector2(5.7f, 0.46f), new Color(0.12f, 0.09f, 0.16f, 0.6f), 51);
        TextMesh goal = Label(hud, new Vector2(0f, 2.98f), 53, "", 0.045f);
        goal.color = Color.white;
        Fit(goal, 5.4f, 0.38f);
        Transform banner = Child(hud, "Banner", new Vector2(0f, 0.7f)).transform;
        Bar(banner, "Back", Vector2.zero, new Vector2(4.8f, 1.1f), new Color(0.12f, 0.09f, 0.16f, 0.85f), 70);
        TextMesh bt = Label(banner, Vector2.zero, 71, "", 0.11f);
        bt.color = new Color(1f, 0.85f, 0.3f);
        Fit(bt, 4.4f, 0.9f);
        SetArray(h, "dots", dots.ToArray());
        SetField(h, "goal", goal);
        SetField(h, "bannerRoot", banner);
        SetField(h, "banner", bt);
        return h;
    }

    static SpriteRenderer[] V40D_Hearts(Transform root)
    {
        var lives = new List<SpriteRenderer>();
        for (int i = 0; i < 3; i++)
        {
            Transform h = Art(root, "Life" + i, "heart", 52, false, new Vector2(-2.45f + i * 0.4f, 3.42f));
            h.localScale = Vector3.one * 0.6f;
            lives.Add(h.GetComponent<SpriteRenderer>());
        }
        return lives.ToArray();
    }

    /// <summary>Figlio con uno sprite ridotto: l'origine del disegno resta nell'origine del figlio.</summary>
    static Transform V40D_Scaled(Transform parent, string name, string sprite, int order, Vector2 pos, float scale)
    {
        Transform h = Child(parent, name, pos).transform;
        h.localScale = Vector3.one * scale;
        Art(h, "Art", sprite, order, false);
        return h;
    }

    // ---------- Garden seasons ----------

    static GameObject BuildSeasonsLevel()
    {
        var root = new GameObject("Level_Seasons");
        SeasonsLevel level = root.AddComponent<SeasonsLevel>();
        Transform r = root.transform;
        SpriteRenderer backdrop = Art(r, "Garden", "gs_bg", -20, false).GetComponent<SpriteRenderer>();
        SpriteRenderer snowGround = Art(r, "SnowGround", "gs_snow_ground", -19, false).GetComponent<SpriteRenderer>();

        // sole e nuvole
        Transform sun = Child(r, "Sun", new Vector2(1.4f, 2.0f)).transform;
        SpriteRenderer glow = Art(sun, "Glow", "glow", -17, false).GetComponent<SpriteRenderer>();
        glow.color = new Color(1f, 0.95f, 0.6f, 0.6f);
        V40D_Scaled(sun, "Look", "sun", -16, Vector2.zero, 0.7f);
        var clouds = new List<Transform>();
        foreach (Vector2 p in new[] { new Vector2(0.95f, 2.15f), new Vector2(1.85f, 2.0f), new Vector2(1.35f, 1.75f) })
        {
            Transform c = V40D_Scaled(r, "Cloud", "cloud", -15, p, 0.72f);
            c.gameObject.AddComponent<BoxCollider2D>().size = new Vector2(1.9f, 1.2f);
            clouds.Add(c);
        }

        // primavera
        Transform hole = Child(r, "Hole", new Vector2(0f, -1.95f)).transform;
        Art(hole, "Art", "gs_hole", -12, false);
        SpriteRenderer mound = Art(r, "Mound", "gs_mound", -11, false, new Vector2(0f, -1.9f)).GetComponent<SpriteRenderer>();
        Art(r, "Packet", "gs_packet", 5, false, new Vector2(1.95f, -4.0f));
        Transform seed = Child(r, "Seed", new Vector2(1.95f, -3.85f)).transform;
        Art(seed, "Art", "gs_seed", 10, false);
        seed.gameObject.AddComponent<BoxCollider2D>().size = new Vector2(1.0f, 1.1f);
        Transform can = Child(r, "Can", new Vector2(0.95f, -0.55f)).transform;
        Art(can, "Art", "gs_can", 8, false);
        Transform spout = Child(can, "Spout", new Vector2(-0.97f, 0.3f)).transform;
        var drops = new List<Transform>();
        for (int i = 0; i < 10; i++) drops.Add(V40D_Scaled(r, "Drop", "drop", 9, Vector2.zero, 0.5f));

        // estate
        Transform sprout = V40D_Scaled(r, "Sprout", "gs_sprout", -9, new Vector2(0f, -1.95f), 1f);
        Transform plant = Child(r, "Plant", new Vector2(0f, -1.95f)).transform;
        SpriteRenderer plantArt = Art(plant, "Art", "gs_plant", -9, false).GetComponent<SpriteRenderer>();
        Transform umbrella = V40D_Scaled(r, "Umbrella", "gs_umbrella", 12, new Vector2(-2.0f, -3.0f), 1f);
        var ub = umbrella.gameObject.AddComponent<BoxCollider2D>();
        ub.size = new Vector2(1.8f, 1.9f);
        ub.offset = new Vector2(0f, -0.35f);
        Transform shadow = V40D_Scaled(r, "Shadow", "gs_shadow", -10, new Vector2(0f, -1.95f), 1f);

        // autunno
        var pumpkins = new List<Transform>();
        foreach (float x in new[] { -2.05f, -0.7f, 0.7f, 2.05f }) pumpkins.Add(V40D_Scaled(r, "Pumpkin", "gs_pumpkin", -7, new Vector2(x, -2.5f), 1f));
        var holes = new List<Transform>();
        foreach (float x in new[] { -1.38f, 0f, 1.38f }) holes.Add(V40D_Scaled(r, "MoleHole", "gs_molehole", -8, new Vector2(x, -3.3f), 1f));
        Transform mole = Child(r, "Mole", new Vector2(0f, -4.4f)).transform;
        Art(mole, "Art", "gs_mole", -5, false).GetComponent<SpriteRenderer>().maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        var mb = mole.gameObject.AddComponent<BoxCollider2D>();
        mb.size = new Vector2(1.1f, 1.2f);
        mb.offset = new Vector2(0f, 0.5f);
        Transform cat = Child(r, "Cat", new Vector2(0f, -4.4f)).transform;
        Transform catLook = Child(cat, "Look", Vector2.zero).transform;
        catLook.localScale = Vector3.one * 0.5f;
        Art(catLook, "Art", "ic_cat_customer", -5, false).GetComponent<SpriteRenderer>().maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        var cb = cat.gameObject.AddComponent<BoxCollider2D>();
        cb.size = new Vector2(1.0f, 1.0f);
        cb.offset = new Vector2(0f, 0.45f);
        var mask = new GameObject("MoleMask");
        mask.transform.SetParent(r, false);
        mask.transform.localPosition = new Vector3(0f, -1.8f, 0f);
        mask.transform.localScale = new Vector3(6f, 3f, 1f);
        mask.AddComponent<SpriteMask>().sprite = sprites["plain"];
        Transform stars = Child(r, "Stars", Vector2.zero).transform;
        for (int i = 0; i < 3; i++)
        {
            float a = i * Mathf.PI * 2f / 3f;
            Art(stars, "Star", "sparkle_star", 20, false, new Vector2(Mathf.Cos(a), Mathf.Sin(a) * 0.5f) * 0.35f).localScale = Vector3.one * 0.6f;
        }

        // inverno
        Transform greenhouse = Child(r, "Greenhouse", new Vector2(0f, -2.95f)).transform;
        greenhouse.localScale = Vector3.one * 1.25f;
        Art(greenhouse, "Art", "gs_greenhouse", -4, false);
        var lumps = new List<Transform>();
        for (int j = 0; j < 6; j++)
            foreach (int side in new[] { -1, 1 })
            {
                float t = (j + 0.5f) / 6f;
                Vector2 p = Vector2.Lerp(new Vector2(-1.3f * side, 1.5f), new Vector2(0f, 2.35f), t) + new Vector2(0f, 0.13f);
                lumps.Add(V40D_Scaled(greenhouse, "Lump", "gs_snow_lump", -3, p, 1f));
            }
        var flakes = new List<Transform>();
        for (int i = 0; i < 18; i++) flakes.Add(Art(r, "Flake", "gs_flake", 25, false));
        var confetti = new List<Transform>();
        for (int i = 0; i < 14; i++) confetti.Add(V40D_Scaled(r, "Confetti", i % 3 == 0 ? "sparkle_star" : "confetti", 30, Vector2.zero, 0.8f));

        // barra (in basso)
        Transform meterRoot = Child(r, "Meter", new Vector2(0f, -4.55f)).transform;
        Transform meter = GaugeBar(meterRoot, "Bar", Vector2.zero, new Vector2(2.6f, 0.32f), new Color(0.12f, 0.09f, 0.16f, 0.8f), new Color(0.35f, 0.62f, 0.95f), 40);
        TextMesh meterLabel = Label(meterRoot, new Vector2(0f, 0.01f), 43, "", 0.035f);
        meterLabel.color = Color.white;
        Fit(meterLabel, 2.4f, 0.26f);

        SetField(level, "hud", V40D_Hud(r, 4));
        SetArray(level, "lives", V40D_Hearts(r));
        SetField(level, "bubble", SmallBubble(r, new Vector2(-0.9f, 1.0f)));
        SetField(level, "backdrop", backdrop);
        SetField(level, "meter", meter);
        SetField(level, "meterFill", meter.GetChild(0).GetComponent<SpriteRenderer>());
        SetField(level, "meterLabel", meterLabel);
        SetField(level, "seed", seed);
        SetField(level, "seedArea", seed.GetComponent<Collider2D>());
        SetField(level, "hole", hole);
        SetField(level, "mound", mound);
        SetField(level, "can", can);
        SetField(level, "spout", spout);
        SetArray(level, "drops", drops.ToArray());
        SetArray(level, "clouds", clouds.ToArray());
        SetField(level, "sun", sun);
        SetField(level, "sunGlow", glow);
        SetField(level, "sprout", sprout);
        SetField(level, "plant", plant);
        SetField(level, "plantArt", plantArt);
        SetField(level, "umbrella", umbrella);
        SetField(level, "umbrellaArea", ub);
        SetField(level, "shadow", shadow);
        SetArray(level, "pumpkins", pumpkins.ToArray());
        SetArray(level, "holes", holes.ToArray());
        SetField(level, "mole", mole);
        SetField(level, "cat", cat);
        SetField(level, "stars", stars);
        SetField(level, "greenhouse", greenhouse);
        SetArray(level, "lumps", lumps.ToArray());
        SetArray(level, "flakes", flakes.ToArray());
        SetField(level, "snowGround", snowGround);
        SetArray(level, "confetti", confetti.ToArray());
        return SaveLevel(root, "Seasons");
    }

    // ---------- Museum night ----------

    static GameObject BuildMuseumLevel()
    {
        var root = new GameObject("Level_Museum");
        MuseumLevel level = root.AddComponent<MuseumLevel>();
        Transform r = root.transform;
        Art(r, "Hall", "mu_bg", -20, false);

        Transform painting = V40D_Scaled(r, "Painting", "portrait", -10, new Vector2(1.75f, 0.55f), 0.75f);

        Transform guard = Child(r, "Guard", new Vector2(0.3f, -2.25f)).transform;
        Transform guardArt = Child(guard, "Flip", Vector2.zero).transform;
        guardArt.localScale = Vector3.one * 0.9f;
        // ordine: gatto (3) dietro le statue (5), luce della torcia (6) sopra, guardiano (8) davanti a tutto
        Art(guardArt, "Art", "mu_guard", 8, false);
        Transform apex = Child(guardArt, "Apex", new Vector2(0.8f, 1.12f)).transform;
        SpriteRenderer cone = Art(apex, "Cone", "mu_cone", 6, false).GetComponent<SpriteRenderer>();
        TextMesh countdown = Label(guard, new Vector2(0f, 2.35f), 30, "3", 0.12f);
        countdown.color = new Color(1f, 0.85f, 0.3f);
        Transform zzz = Art(guard, "Zzz", "zzz", 30, false, new Vector2(0.35f, 2.15f));

        // il gatto parte nascosto dietro la prima statua: all'inizio il guardiano non può vederlo
        Transform cat = Child(r, "Cat", new Vector2(-1.0f, -2.95f)).transform;
        Transform catArt = Child(cat, "Flip", Vector2.zero).transform;
        Art(catArt, "Art", "mu_burglar", 3, false);
        var col = cat.gameObject.AddComponent<BoxCollider2D>();
        col.size = new Vector2(1.4f, 1.5f);
        col.offset = new Vector2(0f, 0.55f);

        var statues = new List<Transform>();
        foreach (float x in new[] { -1.0f, 0.75f }) statues.Add(V40D_Scaled(r, "Statue", "mu_statue", 5, new Vector2(x, -3.05f), 1f));

        var lasers = new List<Transform>();
        foreach (float x in new[] { 1.25f, -0.12f, -1.85f })
        {
            Transform l = Child(r, "Laser", new Vector2(x, -2.25f)).transform;
            Art(l, "Beam", "laser", 6, false).localScale = new Vector3(1f, 1.6f, 1f);
            Art(l, "Top", "laser_node", 7, false, new Vector2(0f, 0.85f));
            Transform b = Art(l, "Bottom", "laser_node", 7, false, new Vector2(0f, -0.85f));
            b.localRotation = Quaternion.Euler(0, 0, 180f);
            lasers.Add(l);
        }

        Transform tray = Child(r, "Tray", new Vector2(0f, -4.1f)).transform;
        Bar(tray, "Back", Vector2.zero, new Vector2(5.4f, 1.3f), new Color(0.1f, 0.08f, 0.16f, 0.8f), 40);
        TextMesh trayLabel = Label(tray, new Vector2(-2.05f, 0.47f), 41, "BAG", 0.035f);
        trayLabel.color = new Color(1f, 1f, 1f, 0.7f);
        Transform bag = V40D_Scaled(tray, "Bag", "mu_bag", 45, new Vector2(-1.1f, 0f), 1f);
        Transform fake = V40D_Scaled(tray, "Fake", "mu_fake", 45, new Vector2(0.9f, 0f), 0.4f);
        fake.gameObject.AddComponent<BoxCollider2D>().size = new Vector2(2.6f, 3.0f);

        SpriteRenderer siren = Bar(r, "Siren", Vector2.zero, new Vector2(7f, 11f), new Color(1f, 0f, 0f, 0f), 35);
        Transform exitPoint = Child(r, "Exit", new Vector2(-2.2f, -2.95f)).transform;

        SetField(level, "hud", V40D_Hud(r, 3));
        SetArray(level, "lives", V40D_Hearts(r));
        SetField(level, "bubble", SmallBubble(r, new Vector2(-0.3f, 1.65f)));
        SetField(level, "cat", cat);
        SetField(level, "catArt", catArt);
        SetField(level, "catArea", col);
        SetField(level, "guard", guard);
        SetField(level, "guardArt", guardArt);
        SetField(level, "coneApex", apex);
        SetField(level, "cone", cone);
        SetArray(level, "statues", statues.ToArray());
        SetField(level, "siren", siren);
        SetField(level, "painting", painting);
        SetField(level, "fake", fake);
        SetField(level, "fakeArea", fake.GetComponent<Collider2D>());
        SetField(level, "bag", bag);
        SetField(level, "tray", tray);
        SetField(level, "countdown", countdown);
        SetArray(level, "lasers", lasers.ToArray());
        SetField(level, "zzz", zzz);
        SetField(level, "exitPoint", exitPoint);
        return SaveLevel(root, "Museum");
    }

    // ---------- Ice cream shop ----------

    static GameObject BuildIceCreamLevel()
    {
        var root = new GameObject("Level_IceCream");
        IceCreamLevel level = root.AddComponent<IceCreamLevel>();
        Transform r = root.transform;
        Art(r, "Shop", "ic_bg", -20, false);
        Art(r, "Counter", "ic_counter", -5, false);

        // clienti dietro al bancone
        Vector2 spot = new Vector2(-1.2f, 0.3f);
        Transform customerSpot = Child(r, "CustomerSpot", spot).transform;
        var customers = new List<Transform>();
        foreach (string s in new[] { "mouse", "sheep", "monster", "wolf", "ic_cat_customer", "snow_buddy" })
        {
            Transform c = Child(r, "Customer_" + s, spot).transform;
            Bounds b = sprites[s].bounds;
            float scale = Mathf.Min(1.75f / b.size.y, 1.9f / b.size.x);
            float bottom = centers[s].y + b.min.y;
            Transform look = Child(c, "Look", new Vector2(0f, -bottom * scale)).transform;
            look.localScale = Vector3.one * scale;
            Art(look, "Art", s, -8, false);
            customers.Add(c);
        }
        Transform mood = Child(r, "Mood", Vector2.zero).transform;
        SpriteRenderer moodArt = Art(mood, "Art", "heart", 20, false).GetComponent<SpriteRenderer>();

        // scontrino
        Transform ticket = Child(r, "Ticket", new Vector2(1.45f, 1.72f)).transform;
        ticket.localScale = Vector3.one * 0.8f;
        Art(ticket, "Paper", "ic_ticket", 2, false);
        Fit(Label(ticket, new Vector2(0f, 0.72f), 3, "ORDER", 0.045f), 1.1f, 0.2f);
        const float ts = 0.55f, coneTop = -0.2f;
        SpriteRenderer ticketCone = Art(V40D_ScaledHolder(ticket, "TCone", new Vector2(0f, coneTop), ts), "Art", "ic_cone", 3, false).GetComponent<SpriteRenderer>();
        var tScoops = new List<SpriteRenderer>();
        for (int i = 0; i < 4; i++)
            tScoops.Add(Art(V40D_ScaledHolder(ticket, "TScoop" + i, new Vector2(0f, coneTop + ts * (0.18f + i * 0.33f)), ts), "Art", "ic_scoop", 4 + i, false).GetComponent<SpriteRenderer>());
        SpriteRenderer tSprinkles = Art(V40D_ScaledHolder(ticket, "TSprinkles", Vector2.zero, ts), "Art", "ic_sprinkles", 9, false).GetComponent<SpriteRenderer>();
        SpriteRenderer tCherry = Art(V40D_ScaledHolder(ticket, "TCherry", Vector2.zero, ts), "Art", "cherry", 10, false).GetComponent<SpriteRenderer>();
        TextMesh tText = Label(ticket, new Vector2(0f, -0.05f), 5, "SAME AS\nTHE LAST\nCUSTOMER!\n- the cat", 0.06f);
        Fit(tText, 1.2f, 1.3f);

        // pazienza (sul bancone)
        Transform patienceRoot = Child(r, "Patience", new Vector2(-1.2f, 0.12f)).transform;
        Transform patience = GaugeBar(patienceRoot, "Bar", Vector2.zero, new Vector2(1.7f, 0.18f), new Color(0.12f, 0.09f, 0.16f, 0.8f), new Color(0.36f, 0.8f, 0.42f), 1);

        // cono in lavorazione
        Transform coneHolder = Child(r, "ConeHolder", new Vector2(0f, -0.85f)).transform;
        Art(coneHolder, "Cone", "ic_cone", 4, false);
        var stack = new List<SpriteRenderer>();
        for (int i = 0; i < 4; i++)
            stack.Add(Art(Child(coneHolder, "S" + i, new Vector2(0f, 0.2f + i * 0.33f)).transform, "Art", "ic_scoop", 5 + i, false).GetComponent<SpriteRenderer>());
        SpriteRenderer sSprinkles = Art(coneHolder, "Sprinkles", "ic_sprinkles", 10, false).GetComponent<SpriteRenderer>();
        SpriteRenderer sCherry = Art(coneHolder, "Cherry", "cherry", 11, false).GetComponent<SpriteRenderer>();

        // vaschette e strumenti
        string[] names = { "STRAWBERRY", "CHOCOLATE", "VANILLA", "MINT" };
        Color[] tints = { new Color(1f, 0.6f, 0.72f), new Color(0.58f, 0.38f, 0.26f), new Color(1f, 0.95f, 0.8f), new Color(0.62f, 0.9f, 0.72f) };
        var tubs = new List<Collider2D>();
        for (int i = 0; i < 4; i++)
        {
            Transform t = Child(r, "Tub" + i, new Vector2(-2.1f + i * 1.4f, -2.75f)).transform;
            Art(t, "Cream", "ic_tub_cream", 3, false, new Vector2(0f, 0.1f)).GetComponent<SpriteRenderer>().color = tints[i];
            Art(t, "Tub", "ic_tub", 4, false);
            Fit(Label(t, new Vector2(0f, -0.52f), 5, names[i], 0.03f), 1.25f, 0.2f);
            var bc = t.gameObject.AddComponent<BoxCollider2D>();
            bc.size = new Vector2(1.32f, 1.2f);
            bc.offset = new Vector2(0f, -0.05f);
            tubs.Add(bc);
        }
        Collider2D Tool(string name, string sprite, float x, string label)
        {
            Transform t = Child(r, name, new Vector2(x, -4.0f)).transform;
            Art(t, "Art", sprite, 4, false);
            if (label != null)
            {
                TextMesh l = Label(t, new Vector2(0f, 0.04f), 6, label, 0.06f);
                l.color = Color.white;
                Fit(l, 1.0f, 0.4f);
            }
            var bc = t.gameObject.AddComponent<BoxCollider2D>();
            bc.size = new Vector2(1.3f, 1.1f);
            return bc;
        }
        Collider2D shaker = Tool("Shaker", "ic_shaker", -2.1f, null);
        Collider2D cherryBowl = Tool("Cherries", "ic_cherry_bowl", -0.7f, null);
        Collider2D bin = Tool("Bin", "ic_bin", 0.7f, null);
        Collider2D serve = Tool("Serve", "ic_serve", 2.1f, "SERVE");
        var strikes = new List<SpriteRenderer>();
        for (int i = 0; i < 2; i++)
        {
            Transform s = Art(r, "Strike" + i, "anger", 52, false, new Vector2(2.1f + i * 0.42f, 3.42f));
            s.localScale = Vector3.one * 0.8f;
            strikes.Add(s.GetComponent<SpriteRenderer>());
        }

        SetField(level, "hud", V40D_Hud(r, 6));
        SetField(level, "bubble", SmallBubble(r, new Vector2(-0.35f, 2.35f)));
        SetArray(level, "customers", customers.ToArray());
        SetField(level, "customerSpot", customerSpot);
        SetField(level, "ticket", ticket);
        SetArray(level, "ticketScoops", tScoops.ToArray());
        SetField(level, "ticketSprinkles", tSprinkles);
        SetField(level, "ticketCherry", tCherry);
        SetField(level, "ticketCone", ticketCone);
        SetField(level, "ticketText", tText);
        SetField(level, "patience", patience);
        SetField(level, "patienceFill", patience.GetChild(0).GetComponent<SpriteRenderer>());
        SetArray(level, "stack", stack.ToArray());
        SetField(level, "stackSprinkles", sSprinkles);
        SetField(level, "stackCherry", sCherry);
        SetField(level, "coneHolder", coneHolder);
        SetArray(level, "tubs", tubs.ToArray());
        SetField(level, "shaker", shaker);
        SetField(level, "cherryBowl", cherryBowl);
        SetField(level, "bin", bin);
        SetField(level, "serve", serve);
        SetArray(level, "strikes", strikes.ToArray());
        SetField(level, "mood", mood);
        SetField(level, "moodArt", moodArt);
        SetField(level, "happySprite", sprites["heart"]);
        SetField(level, "angrySprite", sprites["anger"]);
        return SaveLevel(root, "IceCream");
    }

    static Transform V40D_ScaledHolder(Transform parent, string name, Vector2 pos, float scale)
    {
        Transform h = Child(parent, name, pos).transform;
        h.localScale = Vector3.one * scale;
        return h;
    }

    // ---------- Pirate cannon ----------

    static GameObject BuildCannonLevel()
    {
        var root = new GameObject("Level_Cannon");
        CannonLevel level = root.AddComponent<CannonLevel>();
        Transform r = root.transform;
        Art(r, "Sea", "pc_bg", -20, false);

        // nave nemica con i tre bersagli
        Transform ship = Child(r, "Ship", new Vector2(1.15f, 0f)).transform;
        ship.localScale = Vector3.one * 0.85f;
        Art(ship, "Art", "pc_ship", -10, false);
        Transform barrelT = Child(ship, "PowderBarrel", new Vector2(0.75f, 0.73f)).transform;
        Art(barrelT, "Art", "pc_barrel", -9, false, new Vector2(0f, -0.28f));
        Transform flag = Child(ship, "Flag", new Vector2(0.5f, 2.3f)).transform;
        Art(flag, "Art", "pc_flag", -11, false, new Vector2(-0.34f, 0f));
        Transform captain = Child(ship, "Captain", new Vector2(-0.55f, 0.45f)).transform;
        Art(captain, "Art", "pc_captain", -9, false);
        Transform hat = Child(captain, "Hat", new Vector2(0f, 1.0f)).transform;
        Art(hat, "Art", "pc_hat", -8, false, new Vector2(0f, -0.1f));
        Art(r, "Waves", "pc_waves", -6, false);

        // cannone
        Art(r, "CannonBase", "pc_cannon_base", 4, false, new Vector2(-1.85f, -2.6f));
        Transform barrel = Child(r, "Barrel", new Vector2(-1.85f, -2.6f)).transform;
        Art(barrel, "Art", "pc_cannon_barrel", 3, false);
        Transform muzzle = Child(barrel, "Muzzle", new Vector2(1.1f, 0f)).transform;
        var aimDots = new List<Transform>();
        for (int i = 0; i < 5; i++)
        {
            Transform d = Art(r, "Aim", "d40_dot", 6, false);
            d.localScale = Vector3.one * (0.14f - i * 0.015f);
            d.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.85f - i * 0.12f);
            aimDots.Add(d);
        }

        // pulsante e barra della potenza
        Transform fire = Child(r, "Fire", new Vector2(1.85f, -3.75f)).transform;
        Transform fireArt = Art(fire, "Art", "pc_fire", 10, false);
        TextMesh fl = Label(fireArt, new Vector2(0f, 0.03f), 12, "FIRE", 0.075f);
        fl.color = Color.white;
        Fit(fl, 0.85f, 0.4f);
        fire.gameObject.AddComponent<CircleCollider2D>().radius = 0.7f;
        Transform powerRoot = Child(r, "Power", new Vector2(0.8f, -3.75f)).transform;
        Bar(powerRoot, "Back", Vector2.zero, new Vector2(0.36f, 1.5f), new Color(0.12f, 0.09f, 0.16f, 0.85f), 9);
        Transform power = Child(powerRoot, "Pivot", new Vector2(0f, -0.7f)).transform;
        SpriteRenderer powerFill = Bar(power, "Fill", new Vector2(0f, 0.7f), new Vector2(0.26f, 1.4f), new Color(1f, 0.85f, 0.25f), 10);
        TextMesh pl = Label(powerRoot, new Vector2(0f, -0.92f), 10, "POWER", 0.035f);
        pl.color = Color.white;

        // proiettili, vento, effetti
        Transform ball = V40D_Scaled(r, "Ball", "pc_ball", 8, Vector2.zero, 1f);
        Transform hook = V40D_Scaled(r, "Hook", "pc_hook", 8, Vector2.zero, 1f);
        LineRenderer rope = Line(r, "Rope", 0.04f, new Color(0.55f, 0.38f, 0.2f), 7);
        Bar(r, "WindPole", new Vector2(-1.9f, 1.5f), new Vector2(0.06f, 0.6f), new Color(0.3f, 0.25f, 0.3f), 4);
        Transform windsock = Child(r, "Windsock", new Vector2(-1.9f, 1.75f)).transform;
        Art(windsock, "Art", "windsock", 5, false);
        Bar(r, "WindBack", new Vector2(-1.9f, 1.0f), new Vector2(1.45f, 0.34f), new Color(0.12f, 0.09f, 0.16f, 0.6f), 5);
        TextMesh windText = Label(r, new Vector2(-1.9f, 1.0f), 6, "WIND", 0.04f);
        windText.color = Color.white;
        Fit(windText, 1.35f, 0.28f);
        var ballIcons = new List<SpriteRenderer>();
        for (int i = 0; i < 6; i++) ballIcons.Add(Art(r, "BallIcon", "pc_ball", 12, false, new Vector2(-2.55f + i * 0.3f, -4.45f)).GetComponent<SpriteRenderer>());
        Transform boom = V40D_Scaled(r, "Boom", "boom", 20, Vector2.zero, 1f);
        Transform splash = V40D_Scaled(r, "Splash", "splash", -5, Vector2.zero, 1f);
        Transform chest = Child(r, "Chest", new Vector2(1.15f, 0.05f)).transform;
        SpriteRenderer chestArt = Art(chest, "Art", "pc_chest", -7, false).GetComponent<SpriteRenderer>();
        var coins = new List<Transform>();
        for (int i = 0; i < 12; i++) coins.Add(V40D_Scaled(r, "Coin", "coin", 25, Vector2.zero, 0.6f));
        var bubbles = new List<Transform>();
        for (int i = 0; i < 6; i++) bubbles.Add(V40D_Scaled(r, "Bubble", "soap_bubble", -5, Vector2.zero, 0.4f));

        SetField(level, "hud", V40D_Hud(r, 4));
        SetField(level, "bubble", SmallBubble(r, new Vector2(-0.5f, 1.55f)));
        SetField(level, "barrel", barrel);
        SetField(level, "muzzle", muzzle);
        SetArray(level, "aimDots", aimDots.ToArray());
        SetField(level, "fireButton", fire.GetComponent<Collider2D>());
        SetField(level, "fireArt", fireArt);
        SetField(level, "power", power);
        SetField(level, "powerFill", powerFill);
        SetField(level, "ball", ball);
        SetField(level, "hook", hook);
        SetField(level, "rope", rope);
        SetField(level, "ship", ship);
        SetArray(level, "targets", new Object[] { barrelT, flag, hat });
        SetField(level, "captain", captain);
        SetField(level, "windsock", windsock);
        SetField(level, "windText", windText);
        SetArray(level, "ballIcons", ballIcons.ToArray());
        SetField(level, "boom", boom);
        SetField(level, "splash", splash);
        SetField(level, "chest", chest);
        SetField(level, "chestArt", chestArt);
        SetField(level, "chestOpenSprite", sprites["pc_chest_open"]);
        SetArray(level, "coins", coins.ToArray());
        SetArray(level, "bubbles", bubbles.ToArray());
        return SaveLevel(root, "Cannon");
    }
}

public static partial class LevelHints
{
    static readonly Dictionary<string, string[]> V40D = new Dictionary<string, string[]>
    {
        { "garden_seasons", new[] {
            "Tilt LEFT to pour. In summer the shadow falls on the side away from the sun - and the sun moves.",
            "Seed in the hole, tilt left until the soil is dark; swipe the clouds; keep the umbrella's SHADOW on the plant; bonk the mole (never the cat); shake the phone in winter." } },
        { "museum_night", new[] {
            "Hide behind the statue pedestals and sneak behind the guard's back. Swap only after his yawn.",
            "Move when his beam points away; when he yawns he turns for 3 s - drop the fake on the frame. At the lasers, hold to walk and let go as soon as a beam flickers." } },
        { "ice_cream_shop", new[] {
            "Wrong scoop? Tap the bin. Toppings go on last. Remember each order...",
            "Build the ticket bottom-to-top, add sprinkles/cherry, tap SERVE. The cat wants exactly what customer 4 had - memorize it before he leaves!" } },
        { "pirate_cannon", new[] {
            "Wind pushes the ball: aim against it. Power goes up AND down - release at the right moment.",
            "Steep angle and high power for the flag, flatter for the barrel and the hat. After the ship sinks, shoot the hook at the floating chest the same way." } },
    };
}
#endif
