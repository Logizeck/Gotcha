using UnityEngine;

namespace Gotcha.Levels
{
    /// <summary>Brano di sottofondo di un livello (o del menu).</summary>
    public enum MusicTrack { Pressure, Dungeon, Animals, Kitchen, Title }

    [CreateAssetMenu(menuName = "Gotcha/Level Definition")]
    public class LevelDefinition : ScriptableObject
    {
        public string levelId;
        public string title;
        [TextArea] public string hint;
        [Tooltip("I 3 suggerimenti (il primo è uguale a hint): ognuno costa un hint del portafoglio del giocatore.")]
        [TextArea] public string[] hints;
        public GameObject prefab;   // prefab con un LevelController sulla radice
        [Tooltip("Id dell'Easter egg nascosto nel livello (vuoto = nessuno): nel menu il livello ha un colore speciale.")]
        public string easterEggId;
        [Tooltip("Musica del livello: Dungeon (labirinti e chiavi), Animals (animali e natura), Kitchen (cibo), Pressure (il resto).")]
        public MusicTrack music = MusicTrack.Pressure;
    }
}
