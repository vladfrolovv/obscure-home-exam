using System;
using UnityEngine;
namespace OGClient.Gameplay.Grid.Models
{
    [Serializable]
    public class SpecialLinkModel
    {
        [field: SerializeField]
        public int LinkSize { get; private set; } = 4;

        [field: SerializeField]
        public GridItemView SpawnItemView { get; set; }
    }
}
