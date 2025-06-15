using UnityEngine;
namespace OGClient.Gameplay
{
    [CreateAssetMenu(fileName = "GameplaySettings", menuName = "ScriptableObjects/Gameplay Settings")]
    public class ScriptableGameplaySettings : ScriptableObject
    {

        [field: SerializeField, Header("Base Gameplay Settings")]
        public float StartDelay { get; private set; } = 3f;

        [field: SerializeField]
        public float TimePerTurn { get; private set; } = 20f;

        [field: SerializeField]
        public int MovesPerTurn { get; private set; } = 2;

        [field: SerializeField]
        public int ExtraMovesPerLink { get; private set; } = 15;

        [field: SerializeField]
        public int RoundsPerGame { get; private set; } = 5;

    }
}
