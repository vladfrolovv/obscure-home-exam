using UniRx;
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using OGClient.Gameplay.Grid.Configs;
using Random = UnityEngine.Random;
using OGClient.Gameplay.Grid.MergeCombos;
namespace OGClient.Gameplay.Grid.Models
{
    public class GridLinksModel
    {

        private const string AddLinkProperty = "LinkAdd";
        private const string RemoveLinkProperty = "LinkRemove";
        private const string SelectableProperty = "Selectable";

        public GridLinksModel(GridModel gridModel, ScriptableGridLinkSettings gridLinkSettings)
        {
            _gridModel = gridModel;
            _gridLinkSettings = gridLinkSettings;
            _specialLinks.AddRange(gridLinkSettings.SpecialLinks);
        }

        private MergeComboModel _mergeComboModel;

        private readonly GridModel _gridModel;
        private readonly ScriptableGridLinkSettings _gridLinkSettings;

        private readonly List<GridItemView> _powerupsInLink = new ();
        private readonly List<GameObject> _executeList = new ();
        private readonly List<GridTileView> _tilesLink = new ();
        private readonly List<SpecialLinkModel> _specialLinks = new();

        public int SpecialIndex { get; private set; } = -1;
        public float ExecuteTotalTime { get; private set; }


        private int _currentLinkSize;
        private bool _isExecuting;

        // todo: add switch
        private bool _thisClientIsCurrentPlayer = true;
        private Vector2 _direction = Vector2.zero;

        public int LinkType { get; set; } = -1;
        public int Count => _tilesLink.Count;
        public int PowerupsInLinkCount => _powerupsInLink.Count;
        public GridTileView this[int index] => _tilesLink[index];
        public GridItemView GetSpecial(int index) => _specialLinks[index].SpawnItemView;
        public bool Contains(GridTileView tileView) => _tilesLink.Contains(tileView);

        private readonly Subject<Unit> _linkExecuted = new();
        private readonly Subject<Unit> _startedLink = new();
        private readonly Subject<List<ItemCollectionModel>> _collectedItems = new();
        public IObservable<Unit> LinkExecuted => _linkExecuted;
        public IObservable<Unit> StartedLink => _startedLink;
        public IObservable<List<ItemCollectionModel>> CollectedItems => _collectedItems;

        public void SetThisClientIsCurrentPlayer(bool isCurrentPlayer)
        {
            _thisClientIsCurrentPlayer = isCurrentPlayer;
        }

        public void LinkStart(GridTileView tileView)
        {
            _startedLink?.OnNext(Unit.Default);
            _tilesLink.Clear();
            _powerupsInLink.Clear();
            _mergeComboModel = null;
            _direction = Vector2.zero;
            LinkAdd(tileView);
        }

        // todo: RPC
        public void LinkStartByGrid(int gridX, int gridY)
        {
            _tilesLink.Clear();
            _powerupsInLink.Clear();
            _mergeComboModel = null;
            _direction = Vector2.zero;

            LinkAddByGrid(gridX, gridY);
        }

        public bool TryToExecuteLink()
        {
            if (_isExecuting) return false;
            _isExecuting = true;

            if (_tilesLink.Count < _gridLinkSettings.MinimumLinkSize)
            {
                CancelExecuteLink();
                _isExecuting = false;

                return false;
            }

            float tempExecuteTime = _gridLinkSettings.ExecuteTime;
            float extraExecuteTime = 0;
            ExecuteTotalTime = 0;

            // todo: collect on this client
            List<ItemCollectionModel> itemsCollected = new();
            for (int index = 0; index < _tilesLink.Count; index++)
            {
                LeanTween.rotate(_tilesLink[index].gameObject, Vector3.forward * 0, 0.2f);
                if (_thisClientIsCurrentPlayer)
                {
                    _tilesLink[index].SetConnectorLineActive(false);
                }

                GridTileView gridTileView = _tilesLink[index];

                GridItemView gridItemView = gridTileView.GridItemView;
                Vector2Int tileGridIndex = GetIndexInGrid(gridTileView);
                if (gridTileView.GridItemView.GridItemType < 0)
                {
                    tempExecuteTime = _gridLinkSettings.ExecuteTime;
                }

                if (index == _tilesLink.Count - 1) gridTileView.GridItemView.IsLastInLink = true;
                itemsCollected.Add(new ItemCollectionModel(tileGridIndex.x, tileGridIndex.y, ExecuteTotalTime + extraExecuteTime));

                if (gridItemView.GridItemType > -1 || _powerupsInLink.Count < 2) extraExecuteTime += gridItemView.ExtraExecuteTime;

                ExecuteTotalTime += tempExecuteTime;

                tempExecuteTime *= _gridLinkSettings.ExecuteTimeMultiplier;
                tempExecuteTime = Mathf.Clamp(tempExecuteTime, _gridLinkSettings.ExecuteTimeMinimum, 999);
            }

            ExecuteTotalTime += extraExecuteTime;

            _collectedItems?.OnNext(itemsCollected);
            _linkExecuted?.OnNext(Unit.Default);

            Debug.Log($"Trying to execute link: {string.Join(", ", _tilesLink.Select(x => x.gameObject.name))}");
            return true;
        }

        // todo: RPC
        public void LinkAddByGrid(int gridX, int gridY)
        {
            GridTileView tileView = GetTileByGrid(gridX, gridY);
            _tilesLink.Add(tileView);

            if (_thisClientIsCurrentPlayer)
            {
                if (_tilesLink.Count > 1)
                {
                    tileView.SetConnectorLineActive(true);
                }
            }

            tileView.GridItemView.GridItemCanvas.overrideSorting = true;
            tileView.GridItemView.PlayAnimation(AddLinkProperty);

            if (tileView.GridItemView.GridItemType < 0) _powerupsInLink.Add(tileView.GridItemView);

            CheckSpecial();

            tileView.SetClickSize(0.5f);
        }

        private void LinkAdd(GridTileView tileView)
        {
            _tilesLink.Add(tileView);
            if (_tilesLink.Count > 1)
            {
                tileView.SetConnectorLineActive(true);
            }

            tileView.GridItemView.GridItemCanvas.overrideSorting = true;
            tileView.GridItemView.PlayAnimation(AddLinkProperty);

            if (tileView.GridItemView.GridItemType < 0)
            {
                _powerupsInLink.Add(tileView.GridItemView);
            }

            CheckSpecial();
            tileView.SetClickSize(0.5f);
        }

        private void LinkRemove(GridTileView tileView)
        {
            _tilesLink.Remove(tileView);

            tileView.SetConnectorLineActive(false);
            tileView.GridItemView.GetComponent<Canvas>().overrideSorting = false;
            tileView.GridItemView.PlayAnimation(RemoveLinkProperty);

            if (tileView.GridItemView.GridItemType < 0)
            {
                _powerupsInLink.Remove(tileView.GridItemView);
            }

            CheckSpecial();
            tileView.SetClickSize(1);
        }

        private void LinkRemoveByGrid(int gridX, int gridY)
        {
            GridTileView tileView = GetTileByGrid(gridX, gridY);
            _tilesLink.Remove(tileView);

            if (_thisClientIsCurrentPlayer)
            {
                tileView.SetConnectorLineActive(false);
            }

            tileView.GridItemView.GetComponent<Canvas>().overrideSorting = false;
            tileView.GridItemView.PlayAnimation(RemoveLinkProperty);

            if (tileView.GridItemView.GridItemType < 0)
            {
                _powerupsInLink.Remove(tileView.GridItemView);
            }

            CheckSpecial();
            tileView.SetClickSize(1);
        }

        // todo make this RPC
        public void LinkRemoveAfterByGrid(int gridX, int gridY)
        {
            GridTileView tileView = GetTileByGrid(gridX, gridY);
            int correctTileIndex = GetIndexInLink(tileView);
            for (int tileIndex = _tilesLink.Count - 1; tileIndex > correctTileIndex; tileIndex--)
            {
                Vector2Int tile1 = GetIndexInGrid(_tilesLink[tileIndex]);
                LinkRemoveByGrid(tile1.x, tile1.y);
            }
        }

        public void LinkRemoveAfter(GridTileView tileView)
        {
            int correctTileIndex = GetIndexInLink(tileView);
            for (int tileIndex = _tilesLink.Count - 1; tileIndex > correctTileIndex; tileIndex--)
            {
                LinkRemove(_tilesLink[tileIndex]);
            }
        }

        private int GetIndexInLink(GridTileView tileView)
        {
            for (int tileIndex = 0; tileIndex < _tilesLink.Count; tileIndex++)
            {
                if (_tilesLink[tileIndex] == tileView)
                {
                    return tileIndex;
                }
            }

            return -1;
        }

        private int GetIndexInList(GridTileView tileView)
        {
            for (int tileIndex = 0; tileIndex < _gridModel.Count; tileIndex++)
            {
                if (_gridModel[tileIndex] == tileView)
                    return tileIndex;
            }

            return -1;
        }

        public Vector2Int GetIndexInGrid(GridTileView tileView)
        {
            int tileListIndex = GetIndexInList(tileView);
            Vector2Int gridSize = _gridModel.GridSize;
            Vector2Int tileGridIndex = new (tileListIndex % gridSize.x, tileListIndex / gridSize.y);

            return tileGridIndex;
        }

        private GridTileView GetTileByGrid(int gridX, int gridY)
        {
            int listIndex = _gridModel.GridSize.x * gridY + gridX;
            GridTileView gridTileView = _gridModel[listIndex];

            return gridTileView;
        }

        public void CancelExecuteLink()
        {
            while (_tilesLink.Count > 0)
            {
                Vector2Int tile = GetIndexInGrid(_tilesLink[0]);
                LinkRemoveByGrid(tile.x, tile.y);
            }
        }


        public void CollectItemAtTile(GridTileView gridTileView, float delay)
        {
            if (gridTileView == null) return;

            gridTileView.SetClickSize(1);

            GridItemView gridItemView = gridTileView.GridItemView;

            gridTileView.Glow(delay);

            if (gridItemView == null || gridItemView.IsSpawning) return;
            if (gridItemView.transform.parent && gridItemView.transform.parent.parent && gridItemView.transform.parent.parent.parent) gridItemView.transform.SetParent(gridItemView.transform.parent.parent.parent);

            // Gathering multiple powerups in link
            if (gridItemView.GridItemType < 0 && _powerupsInLink.Count > 1)
            {
                gridItemView.IsMerging = true;
            }

            // todo: fix collecting items
            // Collect(gridItemView, _gameManager.CurrentPlayerController.bonusText.transform, delay, gridTileView);

            gridTileView.GridItemView = null;
        }

        public void CollectItemAtGrid(int gridX, int gridY, float delay)
        {
            int listIndex = _gridModel.GridSize.x * gridY + gridX;
            GridTileView gridTileView = _gridModel[listIndex];

            CollectItemAtTile(gridTileView, delay);
        }

        public int CheckSpecial()
        {
            int longestSpecial = 0;
            SpecialIndex = -1;

            // Check any special link size, and create powerups accordingly
            for (int index = 0; index < _specialLinks.Count; index++)
            {
                _currentLinkSize = _specialLinks[index].LinkSize;

                if (_currentLinkSize <= _tilesLink.Count && _currentLinkSize > longestSpecial)
                {
                    longestSpecial = _currentLinkSize;
                    SpecialIndex = index;
                }
            }

            if (SpecialIndex != -1)
            {
                GridItemView gridItemView = _specialLinks[SpecialIndex].SpawnItemView;
                if (gridItemView != null && gridItemView.HasOtherOrientations)
                {
                    CheckDirection();
                    GridItemView tempGridItemView = _specialLinks[SpecialIndex].SpawnItemView.GetOtherOrientation(_direction);
                    if (tempGridItemView != null)
                    {
                        _specialLinks[SpecialIndex].SpawnItemView = tempGridItemView;
                    }
                }
            }

            return SpecialIndex;
        }

        private void Collect(GridItemView gridItemView, Transform target, float delay, GridTileView gridTileView)
        {
            if (gridItemView.IsMerging == false && _mergeComboModel == null) gridItemView.SendMessage("Execute", new ExecuteDataModel(gridTileView, delay), SendMessageOptions.DontRequireReceiver);

            // This is here to make sure that combo powerups trigger if the last tile in the link is a powerup
            if (_powerupsInLink.Count > 1 && gridItemView.IsLastInLink == true)
            {
                gridItemView.IsClearing = gridItemView.IsMerging = false;
            }

            if (gridItemView.IsMerging)
            {
                gridItemView.PlayDelayedAnimation(AddLinkProperty, delay - 0.2f);
            }

            // todo: fix these animations
            // else if (gridItemView.clearAnimation != "")
            // {
            //     gridItemView.PlayDelayedAnimation(gridItemView.clearAnimation, delay - 0.2f);
            // }
            //
            // gridItemView.glowAnimator.enabled = false;
            // gridItemView.glowImage.color = Color.white;

            Vector3 randomOffset = Random.insideUnitCircle * gridItemView.ThrowDistance;
            LeanTween.scale(gridItemView.gameObject, Vector3.one * 1.4f, _gridLinkSettings.CollectTime * 0.3f).setDelay(delay).setEaseInSine().setOnComplete(() =>
            {
                if (gridItemView.IsClearing) return;

                gridItemView.IsClearing = true;
                gridItemView.TryToCollect();

                if (gridItemView.IsMerging) return;
                if (_powerupsInLink.Count < 2 || gridItemView.GridItemType > -1 || gridItemView.IsLastInLink == false || _isExecuting == false)
                {
                    gridItemView.TryToClear();
                }

                if (gridItemView.IsLastInLink)
                {
                    ExecuteLastInLink(gridTileView, target, _gridLinkSettings.CollectTime * 0.3f);

                    if (gridItemView.GridItemType < 0 && _powerupsInLink.Count > 1 && gridItemView.CanMerge) return;
                }

                if (_thisClientIsCurrentPlayer)
                {
                    // todo: fix this;
                    // _gameManager.CurrentPlayerController.AddBonus(1, 0.5f);
                }

                LeanTween.rotate(gridItemView.gameObject, Vector3.forward * Random.Range(-30, 30), _gridLinkSettings.CollectTime * 0.7f).setEaseOutSine();
                LeanTween.move(gridItemView.gameObject, gridItemView.transform.position + randomOffset, _gridLinkSettings.CollectTime * 0.7f).setEaseOutSine().setOnComplete(() =>
                {
                    LeanTween.scale(gridItemView.gameObject, Vector3.one * 0.6f, _gridLinkSettings.CollectTime * 0.3f).setEaseInSine();
                    LeanTween.moveX(gridItemView.gameObject, target.position.x, _gridLinkSettings.CollectTime * 0.3f).setEaseOutSine();
                    LeanTween.moveY(gridItemView.gameObject, target.position.y, _gridLinkSettings.CollectTime * 0.3f).setEaseInSine().setOnComplete(() =>
                    {
                        //MAJD: Here was the add points after the animation, moved it out for now
                        UnityEngine.Object.Destroy(gridItemView.gameObject);
                    });
                });
            });
        }

        private void ExecuteLastInLink(GridTileView gridTileView, Transform target, float delay)
        {
            if (_isExecuting == false) return;

            // If we have multiple powerups in the link, merge and trigger them
            if (_powerupsInLink.Count > 1)
            {
                for (int powerupIndex = 0; powerupIndex < _powerupsInLink.Count; powerupIndex++)
                {
                    _powerupsInLink[powerupIndex].IsMerging = false;
                    _powerupsInLink[powerupIndex].IsClearing = false;

                    LeanTween.moveX(_powerupsInLink[powerupIndex].gameObject, gridTileView.transform.position.x, delay).setEaseInOutSine();
                    LeanTween.moveY(_powerupsInLink[powerupIndex].gameObject, gridTileView.transform.position.y, delay).setEaseInOutQuad();

                    Collect(_powerupsInLink[powerupIndex], target, delay * 0.8f, gridTileView);
                }

                // todo: fix effect, make it RPC
                // _mergeCombosController.MergeEffect(gridTileView.transform.position);
                // _mergeComboModel = _mergeCombosController.GetMergeCombo(_powerupsInLink);

                _powerupsInLink.Clear();

                if (_mergeComboModel != null)
                {
                    GameObject executeObject = UnityEngine.Object.Instantiate(_mergeComboModel.ExecuteObject);
                    executeObject.SendMessage("Execute", new ExecuteDataModel(gridTileView, delay), SendMessageOptions.DontRequireReceiver);

                    _mergeComboModel = null;
                }
            }
        }

        public void AddToExecuteList(GameObject addObject)
        {
            _executeList.Add(addObject);
            // CancelInvoke(nameof(EndExecuteLink));
        }

        public void RemoveFromExecuteList(GameObject removeObject)
        {
            _executeList.Remove(removeObject);

            CheckExecuteLink();
        }

        public void CheckExecuteLink()
        {
            if (_executeList.Count > 0) return;
            Observable.Timer(TimeSpan.FromSeconds(0.3f))
                .Subscribe(_ => EndExecuteLink());
        }

        private void EndExecuteLink()
        {
            _isExecuting = false;
            _tilesLink.Clear();

            // todo: add callback
            // _gridController.CollapseTiles();
            // _matchTimerController.StartTimer();
            // _gridLinksDataProxy.ChangeControlState(true);
        }

        public void PushBack(GridTileView gridTileView, Vector2 direction)
        {
            if (gridTileView == null) return;

            GridItemView gridItemView = gridTileView.GridItemView;

            if (gridItemView == null) return;

            LeanTween.moveLocal(gridItemView.gameObject, direction * 5, 0.3f).setEaseOutCubic().setOnComplete(() =>
            {
                LeanTween.moveLocal(gridItemView.gameObject, Vector2.zero, 0.5f).setEaseInSine();
            });
        }

        private void CheckDirection()
        {
            _direction = _tilesLink.Count < 2
                ? Vector2.zero
                : (_tilesLink[^1].transform.position - _tilesLink[^2].transform.position).normalized;
        }
        
    }
}
