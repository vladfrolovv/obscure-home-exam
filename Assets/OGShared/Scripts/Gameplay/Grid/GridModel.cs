using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using Random = System.Random;
namespace OGShared.Scripts.Gameplay.Grid
{
    public class GridModel
    {

        public GridModel(int width, int height, int seed)
        {
            Width = width;
            Height = height;

            _cells = new int[width * height];
            _randomGenerator = new Random(seed);
        }

        public int Width { get; }
        public int Height { get; }

        private readonly int[] _cells;
        private readonly Random _randomGenerator;

        private readonly Subject<CellSpawnModel> _onCellSpawn = new();
        private readonly Subject<CellMoveModel> _onCellMove = new();
        private readonly Subject<Unit> _onGridCleared = new();

        public void FillGrid(int[] itemTypes)
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width;  x++)
                {
                    int cellIndex = y * Width + x;
                    int itemIndex = itemTypes[_randomGenerator.Next(itemTypes.Length)];

                    _cells[cellIndex] = itemIndex;
                    _onCellSpawn?.OnNext(new CellSpawnModel(x, y, itemIndex));
                }
            }
        }

        public void Collapse()
        {
            for (int x = 0; x < Width; x++)
            {
                List<int> column = new ();
                for (int y = 0; y < Height; y++)
                {
                    column.Add(_cells[y*Width + x]);
                }

                column = column.Where(v => v != 0).ToList();
                int missing = Height - column.Count;
                column.AddRange(Enumerable.Repeat(0, missing));

                for (int y = 0; y < Height; y++)
                {
                    int oldValue = _cells[y*Width + x];
                    int newValue = column[y];
                    _cells[y * Width + x] = newValue;
                    if (oldValue != newValue)
                    {
                        _onCellMove?.OnNext(new CellMoveModel(y * Width + x, y * Width + x, x, y));
                    }
                }
            }
        }

        public void ClearGrid()
        {
            Array.Clear(_cells, 0, _cells.Length);
            _onGridCleared?.OnNext(Unit.Default);
        }

        public IObservable<CellSpawnModel> CellSpawned => _onCellSpawn;
        public IObservable<CellMoveModel> CellMoved => _onCellMove;

    }
}
