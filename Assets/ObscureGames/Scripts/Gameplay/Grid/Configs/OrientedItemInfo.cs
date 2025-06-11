using System.Collections.Generic;
using UnityEngine;
using System;
namespace ObscureGames.Gameplay.Grid.Configs
{
    [Serializable]
    public class OrientedItemInfo
    {

        [field: SerializeField] public GridItemView GridItemView { get; private set; }

        [SerializeField] private Vector2[] _orientations;
        public IReadOnlyList<Vector2> Orientations => _orientations;

    }
}
