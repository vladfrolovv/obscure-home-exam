using System.Collections.Generic;
using System.Linq;
using Fusion;
using OGClient.Gameplay.Grid.Configs;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
namespace OGClient.Gameplay.Grid
{
    public class GridController : SimulationBehaviour
    {

        private const string GridIntroAnimationParam = "Intro";
        private const string GridShakeAnimationParam = "Shake";
        private const string SelectableAnimationParam = "Selectable";
        private const string HiddenAnimationParam = "Hidden";
        private const string FallAnimationParam = "Fall";
        private const string OutroAnimationParam = "Outro";
        private const string BounceAnimationParam = "Bounce";
        private const string IntroPowerupAnimationParam = "IntroPowerup";

        [Header("Base")]
        [SerializeField] private int _gridSeed = -1;

        [Header("View References")]
        [SerializeField] private Canvas _gridCanvas;

        [SerializeField] private Transform _gridHolder;
        [SerializeField] private GridLayoutGroup _gridLayoutGroup;
        [SerializeField] private GridTileView _tileViewPrefab;

        [field: SerializeField, Header("Grid Configuration")]
        public Vector2Int GridSize { get; private set; } = new (7, 7);

        [SerializeField] private List<GridTileView> _tiles = new ();
        [SerializeField] private List<GridTileView> _randomTiles = new ();

        [SerializeField] private int _tileListRandomIndex;
        [SerializeField] private ScriptableGridPattern _overrideGridPattern;

        [Header("Grid Items")]
        [SerializeField] private GridItemView _itemViewPrefab;
        [SerializeField] private ScriptableGridItem[] _itemsTypes;
        [SerializeField] private GridItemView[] _itemsPowerUps;

        [Header("Animations")]
        [SerializeField] private float _itemDropDelay = 0.05f;
        [SerializeField] private float _itemDropTime = 0.05f;
        [SerializeField] private Animator _gridAnimator;
        [SerializeField] private Color _gridTileColor1;
        [SerializeField] private Color _gridTileColor2;

        private DiContainer _diContainer;

        private int _cellSize;
        private int _gridX;
        private int _gridY;

        public bool IsDiagonalsAllowed { get; private set; } = true;
        public IEnumerable<GridTileView> Tiles => _tiles;

        [Networked] public int GridSeed { get; set; }
        [Networked] public int TileListRandomIndex { get; set; }

        public void ShakeBoard() => _gridAnimator.Play(GridShakeAnimationParam);
        public void HideGrid() => _gridCanvas.enabled = true;

        [Inject]
        public void Construct(DiContainer diContainer)
        {
            _diContainer = diContainer;
        }

        public void SetGridSize(Vector2Int setValue)
        {
            GridSize = setValue;

            bool useColumns = setValue.x > setValue.y;
            int constraintCount = useColumns ? setValue.x : setValue.y;

            _gridLayoutGroup.constraint = useColumns
                ? GridLayoutGroup.Constraint.FixedColumnCount
                : GridLayoutGroup.Constraint.FixedRowCount;

            _gridLayoutGroup.constraintCount = constraintCount;

            _cellSize = (270 - constraintCount) / constraintCount;
            _gridLayoutGroup.cellSize = new Vector2Int(_cellSize, _cellSize);
        }

        public void ShowGrid()
        {
            _gridCanvas.enabled = true;
            _gridAnimator.Play(GridIntroAnimationParam);
        }

        public void SpawnItem(int itemIndex, int gridX, int gridY, int offsetY)
        {
            int listIndex = GridSize.x * gridY + gridX;

            GridItemView newItemView =
                _diContainer.InstantiatePrefab(_itemViewPrefab, _tiles[listIndex].transform).GetComponent<GridItemView>();
            newItemView.SetType(itemIndex);
            newItemView.InstallGridItem(_itemsTypes[itemIndex]);
            newItemView.transform.localScale = Vector3.one;

            _tiles[listIndex].GridItemView = newItemView;

            if (offsetY > 0)
            {
                newItemView.GetComponent<RectTransform>().anchoredPosition += Vector2.up * offsetY;

                float dropDelay = _itemDropDelay * (GridSize.y - gridY);
                float dropTime = _itemDropTime * 2;

                _tiles[listIndex].GridItemView.PlayDelayedAnimation(BounceAnimationParam, dropDelay + dropTime);

                newItemView.IsSpawning = true;

                LeanTween.move(newItemView.gameObject, _tiles[listIndex].transform.position, dropTime).setEaseInCubic().setDelay(dropDelay).setOnComplete(() =>
                {
                    newItemView.IsSpawning = false;
                    newItemView.transform.position = _tiles[listIndex].transform.position;
                });
            }
        }

        public void SpawnItem(GridItemView spawnItemView, GridTileView parentTileView, float delay)
        {
            GridItemView newItemView = _diContainer.InstantiatePrefab(spawnItemView, parentTileView.transform).GetComponent<GridItemView>();

            newItemView.transform.localScale = Vector3.one;

            if (delay > 0)
            {
                newItemView.PlayAnimation(HiddenAnimationParam);
                newItemView.PlayDelayedAnimation(IntroPowerupAnimationParam, delay);
            }

            parentTileView.GridItemView = newItemView;
        }


        private void FillGrid()
        {
            if (_gridSeed != -1) Random.InitState(_gridSeed);
            int arrayIndex = 0;

            for (_gridY = 0; _gridY < GridSize.y; _gridY++)
            {
                for (_gridX = 0; _gridX < GridSize.x; _gridX++)
                {
                    int randomItem = Random.Range(0, _itemsTypes.Length);

                    if (_overrideGridPattern)
                    {
                        Vector2Int gridIndex = new Vector2Int(arrayIndex % GridSize.x, arrayIndex / GridSize.y);

                        if (_overrideGridPattern.Items[arrayIndex] < 0)
                        {
                            SpawnItem(_itemsPowerUps[_overrideGridPattern.Items[arrayIndex] * -1 - 1], _tiles[arrayIndex], 0);
                        }
                        else
                        {
                            SpawnItem(_overrideGridPattern.Items[arrayIndex], gridIndex.x, gridIndex.y, 0);
                        }

                        arrayIndex++;

                        if (arrayIndex >= GridSize.x * GridSize.y) return;
                    }
                    else
                    {
                        SpawnItem(randomItem, _gridX, _gridY, 0);
                    }
                }
            }
        }

        public void CollapseTiles()
        {
            // Check from bottom right to top left (from end of list to start)
            for (int listIndex = _tiles.Count - 1; listIndex >= 0; listIndex--)
            {
                GridItemView gridItemView = _tiles[listIndex].GridItemView;

                // If this grid tile is empty (has no item), check upwards until we find an item, and take it
                if (gridItemView == null)
                {
                    int checkIndex = listIndex;

                    while (checkIndex >= GridSize.x)
                    {
                        checkIndex -= GridSize.x;

                        // If we found an item in this tile, drop it to the tile below
                        if (_tiles[checkIndex].GridItemView)
                        {
                            _tiles[listIndex].GridItemView = _tiles[checkIndex].GridItemView;
                            _tiles[checkIndex].GridItemView = null;

                            _tiles[listIndex].GridItemView.transform.SetParent(_tiles[listIndex].transform);

                            float dropDelay = _itemDropDelay * (GridSize.x - (float)checkIndex / GridSize.x);
                            float dropTime = _itemDropTime;

                            _tiles[listIndex].GridItemView.PlayDelayedAnimation(FallAnimationParam, dropDelay);
                            _tiles[listIndex].GridItemView.PlayDelayedAnimation(BounceAnimationParam, dropDelay + dropTime);

                            LeanTween.move(_tiles[listIndex].GridItemView.gameObject, _tiles[listIndex].transform.position, dropTime).setEaseInCubic().setDelay(dropDelay);

                            break;
                        }
                    }
                }
            }

            Invoke(nameof(FillGridDrop), 0.01f * GridSize.y);
        }

        public void FillGridDrop()
        {
            Debug.Log($"Fill Grid Drop: {GridSize.x}x{GridSize.y}");
            for (_gridY = 0; _gridY < GridSize.y; _gridY++)
            {
                for (_gridX = 0; _gridX < GridSize.x; _gridX++)
                {
                    int listIndex = GridSize.x * _gridY + _gridX;
                    if (_tiles[listIndex].GridItemView == null)
                    {
                        int randomItem = Random.Range(0, _itemsTypes.Length);
                        SpawnItem(randomItem, _gridX, _gridY, GridSize.y * _cellSize);
                    }
                }
            }
        }

        public void ClearGrid()
        {
            for (int listIndex = 0; listIndex < _tiles.Count; listIndex++)
            {
                GridItemView gridItemView = _tiles[listIndex].GridItemView;

                _tiles[listIndex].SetControlsActive(false);

                if (gridItemView)
                {
                    gridItemView.PlayDelayedAnimation(OutroAnimationParam, listIndex * 0.02f);
                }
            }
        }

        public void DisableGrid()
        {
            for (int listIndex = 0; listIndex < _tiles.Count; listIndex++)
            {
                GridItemView gridItemView = _tiles[listIndex].GridItemView;

                _tiles[listIndex].SetControlsActive(false);

                if (gridItemView)
                {
                    gridItemView.SetAnimatorBool(SelectableAnimationParam, false);
                }
            }
        }

        public void EnableGrid()
        {
            for (int listIndex = 0; listIndex < _tiles.Count; listIndex++)
            {
                GridItemView gridItemView = _tiles[listIndex].GridItemView;

                _tiles[listIndex].SetControlsActive(true);

                if (gridItemView)
                {
                    gridItemView.SetAnimatorBool(SelectableAnimationParam, true);
                }
            }
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

        public GridTileView GetPowerupTile()
        {
            return _randomTiles.FirstOrDefault(tile =>
            {
                GridItemView item = tile.GridItemView;
                return item?.GridItemType < 0 && !item.IsClearing;
            });
        }

        private void Awake()
        {
            if (_gridSeed == -1)
            {
                Random.InitState(_gridSeed);
            }

            _randomTiles = _tiles;
            _randomTiles = _randomTiles.OrderBy(x => Random.value).ToList();

            _gridCanvas.enabled = false;
        }

        public void CreateGrid()
        {
            _tiles.Clear();

            SpawnAllTiles();
            SetConnections();
            FillGrid();
            ShowGrid();
        }

        private void SpawnAllTiles()
        {
            for (_gridY = 0; _gridY < GridSize.y; _gridY++)
            {
                for (_gridX = 0; _gridX < GridSize.x; _gridX++)
                {
                    GridTileView newTileView = _diContainer.InstantiatePrefab(_tileViewPrefab, _gridHolder).GetComponent<GridTileView>();
                    newTileView.InstallTileView((_gridX + _gridY) % 2 == 0 ? _gridTileColor1 : _gridTileColor2);

                    _tiles.Add(newTileView);
                }
            }
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