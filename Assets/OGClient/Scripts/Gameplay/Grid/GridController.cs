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

        private readonly GridView _gridView;
        private readonly GridHolderView _gridHolderView;
        private readonly DiContainer _diContainer;
        private readonly GameSessionDataProxy _gameSessionDataProxy;
        private readonly ScriptableGridSettings _gridSettings;

        public GridModel Model { get; private set; }

        public GridController(DiContainer diContainer, GameSessionDataProxy gameSessionDataProxy, GridView gridView,
                              ScriptableGridSettings gridSettings, GridHolderView gridHolderView)
        {
            _gridView = gridView;
            _gridHolderView = gridHolderView;
            _diContainer = diContainer;
            _gameSessionDataProxy = gameSessionDataProxy;
            _gridSettings = gridSettings;

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
            Debug.Log($"Match phase changed to: {phase}");
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
            for (int listIndex = 0; listIndex < Model.Count; listIndex++)
            {
                GridItemView gridItemView = Model[listIndex].GridItemView;
                Model[listIndex].SetControlsActive(true);

                gridItemView?.SetAnimatorBool(SelectableAnimationParam, true);
            }
        }

        public void DisableGrid()
        {
            for (int listIndex = 0; listIndex < Model.Count; listIndex++)
            {
                GridItemView gridItemView = Model[listIndex].GridItemView;
                Model[listIndex].SetControlsActive(false);

                gridItemView?.SetAnimatorBool(SelectableAnimationParam, false);
            }
        }

        public void CollapseTiles()
        {
            for (int listIndex = Model.Count - 1; listIndex >= 0; listIndex--)
            {
                GridItemView gridItemView = Model[listIndex].GridItemView;

                if (gridItemView != null) continue;
                int checkIndex = listIndex;

                while (checkIndex >= Model.GridSize.x)
                {
                    checkIndex -= Model.GridSize.x;

                    if (!Model[checkIndex].GridItemView) break;
                    Model[listIndex].GridItemView = Model[checkIndex].GridItemView;
                    Model[checkIndex].GridItemView = null;
                    Model[listIndex].GridItemView.transform.SetParent(Model[listIndex].transform);

                    float dropDelay = _gridSettings.ItemDropDelay * (Model.GridSize.x - (float)checkIndex / Model.GridSize.x);
                    float dropTime = _gridSettings.ItemDropTime;

                    Model[listIndex].GridItemView.PlayDelayedAnimation(FallAnimationParam, dropDelay);
                    Model[listIndex].GridItemView.PlayDelayedAnimation(BounceAnimationParam, dropDelay + dropTime);

                    LeanTween.move(Model[listIndex].GridItemView.gameObject,
                        Model[listIndex].transform.position, dropTime).setEaseInCubic().setDelay(dropDelay);
                }
            }

            Observable.Timer(TimeSpan.FromSeconds(0.01f * Model.GridSize.y))
                .Subscribe(_ => FillGridDrop()).AddTo(_compositeDisposable);
        }

        private void FillGridDrop()
        {
            for (int y = 0; y < Model.GridSize.y; y++)
            {
                for (int x = 0; x < Model.GridSize.x; x++)
                {
                    int listIndex = Model.GridSize.x * y + x;
                    if (Model[listIndex].GridItemView != null) continue;

                    int randomItem = Random.Range(0, _gridSettings.ItemsTypes.Length);
                    CreateGridItem(randomItem, x, y, Model.GridSize.y * _gridView.CellSize);
                }
            }
        }

                private void InitializeGrid(Vector2Int gridSize, int seed = -1)
        {
            Model = new GridModel(gridSize, seed);

            SpawnTiles();
            FillGrid();

            _gridView.ShowGrid();
            _gridView.SetGridSize(gridSize);

            Model.InitializeGrid();
        }

        private void SpawnTiles()
        {
            for (int y = 0; y < Model.GridSize.y; y++)
            {
                for (int x = 0; x < Model.GridSize.x; x++)
                {
                    GridTileView newTileView = _diContainer.InstantiatePrefab(_gridSettings.TileViewPrefab, _gridHolderView.transform).GetComponent<GridTileView>();
                    newTileView.InstallTileView(
                        (x + y) % 2 == 0 ? _gridSettings.EvenTileColor : _gridSettings.OddTileColor);

                    Model.AddTile(newTileView);
                }
            }
        }

        private void FillGrid()
        {
            if (Model.Seed != -1)
            {
                Random.InitState(Model.Seed);
            }

            int arrayIndex = 0;
            for (int y = 0; y < Model.GridSize.y; y++)
            {
                for (int x = 0; x < Model.GridSize.x; x++)
                {
                    int randomItem = Random.Range(0, _gridSettings.ItemsTypes.Length);
                    if (_gridSettings.OverrideGridPattern)
                    {
                        Vector2Int gridIndex = new(arrayIndex % Model.GridSize.x, arrayIndex / Model.GridSize.y);
                        if (_gridSettings.OverrideGridPattern.Items[arrayIndex] < 0)
                        {
                            CreateGridItem(_gridSettings.PowerUpItems[_gridSettings.OverrideGridPattern.Items[arrayIndex] * -1 - 1], Model[arrayIndex], 0);
                        }
                        else
                        {
                            CreateGridItem(_gridSettings.OverrideGridPattern.Items[arrayIndex], gridIndex.x, gridIndex.y, 0);
                        }

                        arrayIndex++;

                        if (arrayIndex >= Model.GridSize.x * Model.GridSize.y) return;
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
            int listIndex = Model.GridSize.x * gridY + gridX;

            GridItemView newItemView =
                _diContainer.InstantiatePrefab(_gridSettings.ItemViewPrefab, Model[listIndex].transform).GetComponent<GridItemView>();
            newItemView.SetType(itemIndex);
            newItemView.InstallGridItem(_gridSettings.ItemsTypes[itemIndex]);
            newItemView.transform.localScale = Vector3.one;

            Model[listIndex].GridItemView = newItemView;

            if (offsetY > 0)
            {
                newItemView.GetComponent<RectTransform>().anchoredPosition += Vector2.up * offsetY;

                float dropDelay = _gridSettings.ItemDropDelay * (Model.GridSize.y - gridY);
                float dropTime = _gridSettings.ItemDropTime * 2;

                Model[listIndex].GridItemView.PlayDelayedAnimation(BounceAnimationParam, dropDelay + dropTime);

                newItemView.IsSpawning = true;

                LeanTween.move(newItemView.gameObject, Model[listIndex].transform.position, dropTime).setEaseInCubic().setDelay(dropDelay).setOnComplete(() =>
                {
                    newItemView.IsSpawning = false;
                    newItemView.transform.position = Model[listIndex].transform.position;
                });
            }
        }

        private void ClearGrid()
        {
            for (int listIndex = 0; listIndex < Model.Count; listIndex++)
            {
                GridItemView gridItemView = Model[listIndex].GridItemView;
                Model[listIndex].SetControlsActive(false);

                gridItemView?.PlayDelayedAnimation(OutroAnimationParam, listIndex * 0.02f);
            }
        }

        private void EndingMatchPhase()
        {
            ClearGrid();
            _gridView.HideGrid();
        }

    }
}