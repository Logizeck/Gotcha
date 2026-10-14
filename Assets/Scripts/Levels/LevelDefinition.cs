using UnityEngine;

namespace Gotcha.Levels
{
    [CreateAssetMenu(menuName = "Gotcha/Level Definition")]
    public class LevelDefinition : ScriptableObject
    {
        public string levelId;
        public string title;
        [TextArea] public string hint;
        public GameObject prefab;   // prefab con un LevelController sulla radice
    }
}
