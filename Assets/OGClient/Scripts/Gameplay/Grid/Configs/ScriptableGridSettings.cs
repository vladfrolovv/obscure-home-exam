using UnityEngine;
namespace OGClient.Gameplay.Grid.Configs
{
    [CreateAssetMenu(fileName = "GridSettings", menuName = "ScriptableObjects/Grid Settings", order = 1)]
    public class ScriptableGridSettings : ScriptableObject
    {

        [field: SerializeField, Header("View")]
        public Color EvenTileColor { get; private set; }

        [field: SerializeField]
        public Color OddTileColor { get; private set; }

        [field: SerializeField, Header("Animations")]
        public float ItemDropDelay { get; private set; } = 0.05f;

        [field: SerializeField]
        public float ItemDropTime { get; private set; } = 0.05f;

        [field: SerializeField]
        public bool AllowDiagonals { get; private set; } = true;

        [field: SerializeField, Header("Other Configs")]
        public ScriptableGridPattern OverrideGridPattern { get; private set; }

        [field: SerializeField]
        public ScriptableGridItem[] ItemsTypes { get; private set; }

        [field: SerializeField]
        public GridItemView[] PowerUpItems { get; private set; }

        // todo: make zenject factory
        [field: SerializeField, Header("Prefabs")]
        public GridTileView TileViewPrefab { get; private set; }

        [field: SerializeField]
        public GridItemView ItemViewPrefab { get; private set; }


    }
}
