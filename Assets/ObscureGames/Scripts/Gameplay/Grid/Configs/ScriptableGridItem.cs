using UnityEngine;

namespace ObscureGames.Gameplay.Grid.Configs
{
    [CreateAssetMenu(fileName = "Grid Item", menuName = "ScriptableObjects/Grid Item", order = 1)]
    public class ScriptableGridItem : ScriptableObject
    {

        public Sprite icon;
        public Sprite shadow;
        public Sprite glow;
        public Color color = Color.white;

    }
}
