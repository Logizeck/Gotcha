#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// La scena "SECRET FOUND!" degli Easter egg (GameUI, EggCelebration.cs): fanfara e sprite.
/// La fanfara EasterEggFanfare.ogg (5 s) è un effetto breve: Vorbis compresso in memoria, precaricata (deve partire
/// subito, senza attese di caricamento), non in streaming. Gli sprite sono quelli già generati dal builder
/// (raggiera, alone, stelline, coriandoli); se uno manca GameUI usa un disegno di riserva.
/// </summary>
public static partial class GotchaProjectBuilder
{
    const string EggFanfarePath = "Assets/Audio/Sfx/EasterEggFanfare.ogg";

    static void WireEggCelebration(Object ui)
    {
        if (ui == null) return;
        SetField(ui, "eggFanfare", EggFanfare());
        SetField(ui, "eggRaysSprite", EggSprite("burst_rays"));
        SetField(ui, "eggGlowSprite", EggSprite("glow"));
        SetField(ui, "eggSparkleSprite", EggSprite("sparkle_star"));
        SetField(ui, "eggStarSprite", EggSprite("goal_star"));
        SetField(ui, "eggConfettiSprite", EggSprite("confetti"));
    }

    static Sprite EggSprite(string name)
    {
        if (sprites.TryGetValue(name, out Sprite s) && s != null) return s;
        Debug.LogWarning("[Gotcha] Sprite per la scena degli Easter egg non trovato: " + name + " (si usa il disegno di riserva)");
        return null;
    }

    /// <summary>Importa la fanfara come effetto: Vorbis, compressa in memoria, precaricata, stereo.</summary>
    static AudioClip EggFanfare()
    {
        var importer = AssetImporter.GetAtPath(EggFanfarePath) as AudioImporter;
        if (importer == null) { Debug.LogWarning("[Gotcha] Fanfara degli Easter egg non trovata: " + EggFanfarePath + " (la scena resta muta)"); return null; }
        AudioImporterSampleSettings settings = importer.defaultSampleSettings;
        bool changed = settings.loadType != AudioClipLoadType.CompressedInMemory || settings.compressionFormat != AudioCompressionFormat.Vorbis ||
                       Mathf.Abs(settings.quality - 0.7f) > 0.001f || importer.loadInBackground || importer.forceToMono;
        settings.loadType = AudioClipLoadType.CompressedInMemory;   // 5 s: decomprimerla tutta in RAM non serve
        settings.compressionFormat = AudioCompressionFormat.Vorbis;
        settings.quality = 0.7f;
#if UNITY_2022_2_OR_NEWER
        changed |= !settings.preloadAudioData;
        settings.preloadAudioData = true;
#else
        changed |= !importer.preloadAudioData;
        importer.preloadAudioData = true;
#endif
        importer.defaultSampleSettings = settings;
        importer.loadInBackground = false;   // pronta subito: la fanfara parte nello stesso fotogramma della scena
        importer.forceToMono = false;
        if (changed) importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<AudioClip>(EggFanfarePath);
    }
}
#endif
