using UnityEngine;
namespace OGShared.Scripts.Gameplay.Grid
{
    public struct CellSpawnModel
    {

        public CellSpawnModel(int x, int y, int itemType)
        {
            SpawnPosition = new Vector2Int(x, y);
            ItemType = itemType;
        }

        public Vector2Int SpawnPosition { get; }
        public int ItemType { get; }

    }
}
