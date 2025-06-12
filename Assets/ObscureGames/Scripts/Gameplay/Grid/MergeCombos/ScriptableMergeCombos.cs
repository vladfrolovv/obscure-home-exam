using System.Collections.Generic;
using UnityEngine;
namespace ObscureGames.Gameplay.Grid.MergeCombos
{
    [CreateAssetMenu(fileName = "ScriptableMergeCombos", menuName = "ScriptableObjects/Merge Combos")]
    public class ScriptableMergeCombos : ScriptableObject
    {

        [SerializeField] private List<MergeComboModel> _mergeCombos = new List<MergeComboModel>();

        public int Count => _mergeCombos.Count;
        public MergeComboModel this[int index] => _mergeCombos[index];
        public IReadOnlyList<MergeComboModel> MergeCombos => _mergeCombos;

    }

}
