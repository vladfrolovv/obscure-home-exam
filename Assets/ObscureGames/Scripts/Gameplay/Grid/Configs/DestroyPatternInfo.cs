using System.Collections.Generic;
using UnityEngine;
namespace ObscureGames.Gameplay.Grid.Configs
{
    [System.Serializable]
    public class DestroyPatternInfo
    {

        [field: SerializeField] public string PatternName { get; private set; } = "8 around tile";
        [field: SerializeField] public float Delay { get; private set; }

        [SerializeField] private Vector2Int[] _directions;

        public IReadOnlyList<Vector2Int> Directions => _directions;
    }
}
