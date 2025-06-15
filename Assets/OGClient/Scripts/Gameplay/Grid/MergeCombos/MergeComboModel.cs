using System;
using System.Collections.Generic;
using UnityEngine;
namespace OGClient.Gameplay.Grid.MergeCombos
{
    [Serializable]
    public class MergeComboModel
    {

        [field: SerializeField]
        public string ComboName { get; private set; }

        [field: SerializeField]
        public Sprite Icon { get; private set; }

        [field: SerializeField]
        public GameObject ExecuteObject { get; private set; }

        [SerializeField] private List<int> _itemTypes = new List<int>();
        public IReadOnlyList<int> ItemTypes => _itemTypes;

    }
}
