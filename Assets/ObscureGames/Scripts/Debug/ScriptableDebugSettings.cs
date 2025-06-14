using UnityEngine;
namespace ObscureGames.Debug
{
    [CreateAssetMenu(fileName = "Debug Settings", menuName = "ScriptableObjects/Debug Settings", order = 1)]
    public class ScriptableDebugSettings : ScriptableObject
    {
        public float gameSpeed = 1;
        public int roundsPerMatch = 5;
        public int movesPerRound = 2;
        public float timePerRound = 30;
        public Vector2Int gridSize = new Vector2Int(7, 7);
        public int gridSeed = -1;
        public int minimumLinkSize = 2;
        public bool allowDiagonal = true;
        public float executeTime = 0.1f;
        public float executeTimeMultiplier = 0.95f;
        public float executeTimeMinimum = 0.05f;
        public float itemDropDelay = 0.05f;
        public float itemDropTime = 0.3f;
        public int propellerAtLink = 4;
        public int rocketAtLink = 7;
        public int bombAtLink = 10;
        public int discoAtLink = 13;
    }
}