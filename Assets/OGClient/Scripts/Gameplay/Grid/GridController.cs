using UniRx;
using System;
using Zenject;
using UnityEngine;
using OGShared.Gameplay;
using OGShared.DataProxies;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using OGClient.Gameplay.Grid.Models;
using OGClient.Gameplay.Mathchmaking;
using OGClient.Gameplay.Grid.Configs;
namespace OGClient.Gameplay.Grid
{
    public class GridController : IDisposable, IMatchPhasable
    {

        private const string FallAnimationParam = "Fall";
        private const string OutroAnimationParam = "Outro";
        private const string HiddenAnimationParam = "Hidden";
        private const string BounceAnimationParam = "Bounce";
        private const string SelectableAnimationParam = "Selectable";
        private const string IntroPowerupAnimationParam = "IntroPowerup";

        private readonly Dictionary<MatchPhase, Action> _matchPhaseActions = new();
        private readonly CompositeDisposable _compositeDisposable = new();

        private int _tileListRandomIndex;

        private readonly DiContainer _diContainer;
        private readonly GridHolderView _gridHolderView;
        private readonly GameSessionDataProxy _gameSessionDataProxy;
        private readonly ScriptableGridSettings _gridSettings;
        private readonly ScriptableGridLinkSettings _gridLinkSettings;

        private readonly Subject<Unit> _gridInitialized = new();
        private readonly Subject<Unit> _gridCleared = new();

        public IObservable<Unit> GridInitialized => _gridInitialized;
        public IObservable<Unit> GridCleared => _gridCleared;

        public GridModel GridModel { get; private set; }
        public GridLinksModel GridLinksModel { get; private set; }

        public bool AllowDiagonals => _gridSettings.AllowDiagonals;

        public GridController(GameSessionDataProxy gameSessionDataProxy, ScriptableGridSettings gridSettings, GridHolderView gridHolderView, DiContainer diContainer,
                              ScriptableGridLinkSettings gridLinkSettings)
        {
            _diContainer = diContainer;
            _gridHolderView = gridHolderView;
            _gameSessionDataProxy = gameSessionDataProxy;
            _gridSettings = gridSettings;
            _gridLinkSettings = gridLinkSettings;

            _matchPhaseActions.Add(MatchPhase.Ending, EndingMatchPhase);

            _gameSessionDataProxy.Initialized.Subscribe(_ => OnGameSessionDataInitialized()).AddTo(_compositeDisposable);
            _gameSessionDataProxy.MatchPhaseChanged.Subscribe(OnMatchPhaseChanged).AddTo(_compositeDisposable);
        }

        public void Dispose()
        {
            _compositeDisposable?.Dispose();
        }

        public void OnGameSessionDataInitialized()
        {
            InitializeGrid(_gameSessionDataProxy.GridSize, _gameSessionDataProxy.Seed);
        }

        public void OnMatchPhaseChanged(MatchPhase phase)
        {
            Debug.Log($"[CLIENT][GRID_CONTROLLER] Match phase changed to: {phase}");
            _matchPhaseActions.TryGetValue(phase, out Action action);
            action?.Invoke();
        }

        public void CreateGridItem(GridItemView spawnItemView, GridTileView parentTileView, float delay)
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

        public void EnableGrid()
        {
            for (int listIndex = 0; listIndex < GridModel.Count; listIndex++)
            {
                GridItemView gridItemView = GridModel[listIndex].GridItemView;
                GridModel[listIndex].SetControlsActive(true);

                gridItemView?.SetAnimatorBool(SelectableAnimationParam, true);
            }
        }

        public void DisableGrid()
        {
            for (int listIndex = 0; listIndex < GridModel.Count; listIndex++)
            {
                GridItemView gridItemView = GridModel[listIndex].GridItemView;
                GridModel[listIndex].SetControlsActive(false);

                gridItemView?.SetAnimatorBool(SelectableAnimationParam, false);
            }
        }

        public void CollapseTiles()
        {
            for (int listIndex = GridModel.Count - 1; listIndex >= 0; listIndex--)
            {
                GridItemView gridItemView = GridModel[listIndex].GridItemView;

                if (gridItemView != null) continue;
                int checkIndex = listIndex;

                while (checkIndex >= GridModel.GridSize.x)
                {
                    checkIndex -= GridModel.GridSize.x;

                    if (!GridModel[checkIndex].GridItemView) break;
                    GridModel[listIndex].GridItemView = GridModel[checkIndex].GridItemView;
                    GridModel[checkIndex].GridItemView = null;
                    GridModel[listIndex].GridItemView.transform.SetParent(GridModel[listIndex].transform);

                    float dropDelay = _gridSettings.ItemDropDelay * (GridModel.GridSize.x - (float)checkIndex / GridModel.GridSize.x);
                    float dropTime = _gridSettings.ItemDropTime;

                    GridModel[listIndex].GridItemView.PlayDelayedAnimation(FallAnimationParam, dropDelay);
                    GridModel[listIndex].GridItemView.PlayDelayedAnimation(BounceAnimationParam, dropDelay + dropTime);

                    LeanTween.move(GridModel[listIndex].GridItemView.gameObject,
                        GridModel[listIndex].transform.position, dropTime).setEaseInCubic().setDelay(dropDelay);
                }
            }

            Observable.Timer(TimeSpan.FromSeconds(0.01f * GridModel.GridSize.y))
                .Subscribe(_ => FillGridDrop()).AddTo(_compositeDisposable);
        }

        private void FillGridDrop()
        {
            for (int y = 0; y < GridModel.GridSize.y; y++)
            {
                for (int x = 0; x < GridModel.GridSize.x; x++)
                {
                    int listIndex = GridModel.GridSize.x * y + x;
                    if (GridModel[listIndex].GridItemView != null) continue;

                    int randomItem = Random.Range(0, _gridSettings.ItemsTypes.Length);
                    CreateGridItem(randomItem, x, y, GridModel.GridSize.y * GridModel.CellSize);
                }
            }
        }

        private void InitializeGrid(Vector2Int gridSize, int seed = -1)
        {
            GridModel = new GridModel(gridSize, seed);
            GridLinksModel = new GridLinksModel(GridModel, _gridLinkSettings);

            SpawnTiles();
            FillGrid();

            GridModel.InitializeGrid();
            _gridInitialized?.OnNext(Unit.Default);
        }

        private void SpawnTiles()
        {
            for (int y = 0; y < GridModel.GridSize.y; y++)
            {
                for (int x = 0; x < GridModel.GridSize.x; x++)
                {
                    GridTileView newTileView = _diContainer.InstantiatePrefab(_gridSettings.TileViewPrefab, _gridHolderView.transform).GetComponent<GridTileView>();
                    newTileView.InstallTileView(
                        (x + y) % 2 == 0 ? _gridSettings.EvenTileColor : _gridSettings.OddTileColor);
                    newTileView.gameObject.name = $"GridTile_{x}_{y}";

                    GridModel.AddTile(newTileView);
                }
            }
        }

        private void FillGrid()
        {
            if (GridModel.Seed != -1)
            {
                Random.InitState(GridModel.Seed);
            }

            int arrayIndex = 0;
            for (int y = 0; y < GridModel.GridSize.y; y++)
            {
                for (int x = 0; x < GridModel.GridSize.x; x++)
                {
                    int randomItem = Random.Range(0, _gridSettings.ItemsTypes.Length);
                    if (_gridSettings.OverrideGridPattern)
                    {
                        Vector2Int gridIndex = new(arrayIndex % GridModel.GridSize.x, arrayIndex / GridModel.GridSize.y);
                        if (_gridSettings.OverrideGridPattern.Items[arrayIndex] < 0)
                        {
                            CreateGridItem(_gridSettings.PowerUpItems[_gridSettings.OverrideGridPattern.Items[arrayIndex] * -1 - 1], GridModel[arrayIndex], 0);
                        }
                        else
                        {
                            CreateGridItem(_gridSettings.OverrideGridPattern.Items[arrayIndex], gridIndex.x, gridIndex.y, 0);
                        }

                        arrayIndex++;

                        if (arrayIndex >= GridModel.GridSize.x * GridModel.GridSize.y) return;
                    }
                    else
                    {
                        CreateGridItem(randomItem, x, y, 0);
                    }
                }
            }
        }

        private void CreateGridItem(int itemIndex, int gridX, int gridY, int offsetY)
        {
            int listIndex = GridModel.GridSize.x * gridY + gridX;

            GridItemView newItemView =
                _diContainer.InstantiatePrefab(_gridSettings.ItemViewPrefab, GridModel[listIndex].transform).GetComponent<GridItemView>();
            newItemView.SetType(itemIndex);
            newItemView.InstallGridItem(_gridSettings.ItemsTypes[itemIndex]);
            newItemView.transform.localScale = Vector3.one;

            GridModel[listIndex].GridItemView = newItemView;

            if (offsetY > 0)
            {
                newItemView.GetComponent<RectTransform>().anchoredPosition += Vector2.up * offsetY;

                float dropDelay = _gridSettings.ItemDropDelay * (GridModel.GridSize.y - gridY);
                float dropTime = _gridSettings.ItemDropTime * 2;

                GridModel[listIndex].GridItemView.PlayDelayedAnimation(BounceAnimationParam, dropDelay + dropTime);

                newItemView.IsSpawning = true;

                LeanTween.move(newItemView.gameObject, GridModel[listIndex].transform.position, dropTime).setEaseInCubic().setDelay(dropDelay).setOnComplete(() =>
                {
                    newItemView.IsSpawning = false;
                    newItemView.transform.position = GridModel[listIndex].transform.position;
                });
            }
        }

        private void ClearGrid()
        {
            for (int listIndex = 0; listIndex < GridModel.Count; listIndex++)
            {
                GridItemView gridItemView = GridModel[listIndex].GridItemView;
                GridModel[listIndex].SetControlsActive(false);

                gridItemView?.PlayDelayedAnimation(OutroAnimationParam, listIndex * 0.02f);
            }
        }

        private void EndingMatchPhase()
        {
            ClearGrid();
            _gridCleared?.OnNext(Unit.Default);
        }

    }
}