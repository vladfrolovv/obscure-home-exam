using UnityEngine;
using OGClient.Gameplay.Grid.Models;
namespace OGClient.Gameplay.Grid.Configs
{
    [CreateAssetMenu(fileName = "GridLinkSettings", menuName = "ScriptableObjects/GridLinkSettings", order = 1)]
    public class ScriptableGridLinkSettings : ScriptableObject
    {

        [field: SerializeField]
        public int MinimumLinkSize { get; private set; } = 2;

        [field: SerializeField]
        public float CollectTime { get; private set; } = 0.5f;

        [field: SerializeField]
        public float ExecuteTime { get; private set; } = 0.1f;

        [field: SerializeField]
        public float ExecuteTimeMultiplier { get; private set; } = 0.9f;

        [field: SerializeField]
        public float ExecuteTimeMinimum { get; private set; } = 0.01f;

        [field: SerializeField]
        public int LongestSpecial { get; private set; }

        [Header("References")]
        [SerializeField] private SpecialLinkModel[] _specialLinks;

        public SpecialLinkModel[] SpecialLinks => _specialLinks;

    }
}
