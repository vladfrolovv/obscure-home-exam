using UnityEngine;
namespace OGShared.Scripts.Gameplay.Grid
{
    public struct CellMoveModel
    {

        public CellMoveModel(int fromIndex, int toIndex, int x, int y)
        {
            FromIndex = fromIndex;
            ToIndex = toIndex;
            Position = new Vector2Int(x, y);
        }

        public int FromIndex { get; }
        public int ToIndex { get; }
        public Vector2Int Position { get;  }

    }
}
