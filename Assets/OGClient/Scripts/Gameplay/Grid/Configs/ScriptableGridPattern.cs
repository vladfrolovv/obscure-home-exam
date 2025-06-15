using UnityEngine;
namespace OGClient.Gameplay.Grid.Configs
{
    [CreateAssetMenu(fileName = "Grid Pattern", menuName = "ScriptableObjects/Grid Pattern", order = 1)]

    public class ScriptableGridPattern : ScriptableObject
    {

        [SerializeField] private int[] _items;
        public int[] Items => _items;

    }
}
