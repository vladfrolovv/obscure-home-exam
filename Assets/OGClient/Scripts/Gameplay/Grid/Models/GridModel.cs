using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace OGClient.Gameplay.Grid.Models
{
    public class GridModel
    {

        public GridModel(Vector2Int size, int seed = -1)
        {
            GridSize = size;
            Seed = seed;
        }

        private readonly List<GridTileView> _tiles = new ();
        private readonly List<GridTileView> _randomTiles = new ();

        private int _tileListRandomIndex;

        public Vector2Int GridSize { get; private set; }
        public int Seed { get; private set; }

        public GridTileView this[int index] => _tiles[index];
        public int Count => _tiles.Count;

        public int CellSize { get; private set; }

        public void SetCellSize(int cellSize)
        {
            CellSize = cellSize;
        }

        public void AddTile(GridTileView tile)
        {
            _tiles.Add(tile);
            _randomTiles.Add(tile);
        }

        public void InitializeGrid()
        {
            _randomTiles.AddRange(_tiles.OrderBy(x => Random.value).ToList());
            SetConnections();
        }

        public GridTileView GetPowerupTile()
        {
            return _randomTiles.FirstOrDefault(tile => tile.GridItemView is { GridItemType: < 0, IsClearing: false });
        }

        public GridTileView GetRandomTile()
        {
            GridTileView randomTileView = _randomTiles[_tileListRandomIndex];
            if (_tileListRandomIndex < _randomTiles.Count - 1)
            {
                _tileListRandomIndex++;
            }
            else
            {
                _tileListRandomIndex = 0;
            }

            return randomTileView;
        }

        private void SetConnections()
        {
            int width  = GridSize.x;
            int height = GridSize.y;
            int count  = _tiles.Count;

            for (int i = 0; i < count; i++)
            {
                int x = i % width;
                int y = i / width;

                GridTileView left = x > 0 ? _tiles[i - 1] : null;
                GridTileView right = x < width - 1 ? _tiles[i + 1] : null;
                GridTileView top = y > 0 ? _tiles[i - width] : null;
                GridTileView bottom = y < height - 1 ? _tiles[i + width] : null;

                _tiles[i].InstallTileConnections(right, left, top, bottom);
            }
        }

    }
}
