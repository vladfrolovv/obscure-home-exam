using System.Collections.Generic;
using UnityEngine;
using System;
namespace OGClient.Gameplay.Grid.Models
{
    [Serializable]
    public class OrientedItemModel
    {

        [field: SerializeField] public GridItemView GridItemView { get; private set; }

        [SerializeField] private Vector2[] _orientations;
        public IReadOnlyList<Vector2> Orientations => _orientations;

    }
}
