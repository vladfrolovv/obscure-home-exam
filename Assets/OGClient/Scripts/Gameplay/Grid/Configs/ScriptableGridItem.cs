using UnityEngine;
namespace OGClient.Gameplay.Grid.Configs
{
    [CreateAssetMenu(fileName = "Grid Item", menuName = "ScriptableObjects/Grid Item", order = 1)]
    public class ScriptableGridItem : ScriptableObject
    {

        [field: SerializeField]
        public Sprite Icon { get; private set; }

        [field: SerializeField]
        public Sprite Shadow { get; private set; }

        [field: SerializeField]
        public Sprite Glow { get; private set; }

        [field: SerializeField]
        public Color Color { get; private set; } = Color.white;

    }
}
