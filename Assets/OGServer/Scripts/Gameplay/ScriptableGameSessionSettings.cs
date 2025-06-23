using UnityEngine;
namespace OGServer.Gameplay
{
    [CreateAssetMenu(fileName = "GameSessionSettings", menuName = "ScriptableObjects/Game Session Settings")]
    public class ScriptableGameSessionSettings : ScriptableObject
    {

        [field: SerializeField]
        public Vector2Int GridSize { get; private set; } = new(7, 7);

        [field: SerializeField]
        public int Rounds { get; private set; } = 5;

    }
}
