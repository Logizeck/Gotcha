using System.Collections.Generic;
using UnityEngine;

namespace Gotcha.Levels
{
    [CreateAssetMenu(menuName = "Gotcha/Level Catalog")]
    public class LevelCatalog : ScriptableObject
    {
        public List<LevelDefinition> levels = new List<LevelDefinition>();
    }
}
