using System;
using System.Collections.Generic;
using System.Diagnostics;
using OGClient.Gameplay.DataProxies;
using UniRx;
using UnityEngine;
using OGClient.Gameplay.Timers;
using OGClient.Gameplay.Grid.Configs;
using OGClient.Gameplay.Grid.MergeCombos;
using OGClient.Gameplay.Grid.Models;
using OGClient.Gameplay.UI;
using OGShared.DataProxies;
using UnityEngine.EventSystems;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;
namespace OGClient.Gameplay.Grid
{
    public class GridLinksController : IDisposable
    {

        public int LinkType { get; set; } = -1;

        private readonly List<GridItemView> _powerupsInLink = new ();
        private readonly List<GameObject> _executeList = new ();
        private readonly List<GridTileView> _tilesLink = new ();
        private readonly List<SpecialLinkModel> _specialLinks = new();

        private int _specialIndex = -1;
        private int _currentLinkSize;
        private float _executeTotalTime;
        private bool _isExecuting;
        private Vector2 _direction = Vector2.zero;

        private MergeComboModel _mergeComboModel;

        private readonly EventSystem _eventSystem;
        private readonly ToastView _toastView;
        private readonly MovesDataProxy _movesDataProxy;
        private readonly ScriptableGridLinkSettings _gridLinkSettings;
        private readonly MatchTimerController _matchTimerController;
        private readonly GridController _gridController;
        private readonly MergeCombosController _mergeCombosController;
        private readonly PlayerLinkingDataProxy _playerLinkingDataProxy;
        private readonly GridLinkingDataProxy _gridLinkingDataProxy;

        private readonly CompositeDisposable _compositeDisposable = new();
        private readonly CompositeDisposable _disposableGridLinkingEvents = new();
        private readonly ReactiveProperty<bool> _currentPlayerIsClient = new(false);

        public IReadOnlyList<GridTileView> TilesLink => _tilesLink;
        public IReadOnlyList<GridItemView> PowerupsInLink => _powerupsInLink;

        public void SetCurrentPlayerIsClient(bool client) => _currentPlayerIsClient.Value = client;

        public GridLinksController(GridController gridController, MergeCombosController mergeCombosController, MatchTimerController matchTimerController,
                                   ScriptableGridLinkSettings gridLinkSettings, EventSystem eventSystem, PlayerLinkingDataProxy playerLinkingDataProxy,
                                   GridLinkingDataProxy gridLinkingDataProxy, ToastView toastView, MovesDataProxy movesDataProxy)
        {
            _eventSystem = eventSystem;
            _gridController = gridController;
            _mergeCombosController = mergeCombosController;
            _matchTimerController = matchTimerController;
            _gridLinkSettings = gridLinkSettings;
            _playerLinkingDataProxy = playerLinkingDataProxy;
            _gridLinkingDataProxy = gridLinkingDataProxy;
            _toastView = toastView;
            _movesDataProxy = movesDataProxy;

            _specialLinks.AddRange(_gridLinkSettings.SpecialLinks);

            _gridLinkingDataProxy.GridItemCollected.Subscribe(CollectItemAtGrid).AddTo(_compositeDisposable);
            _gridLinkingDataProxy.LinkExecuted.Subscribe(_ => ExecuteLink()).AddTo(_compositeDisposable);

            _currentPlayerIsClient.Subscribe(OnCurrentPlayerSwitched).AddTo(_compositeDisposable);

            matchTimerController.TimeUp.Subscribe(_ => OnControlStateChanged(false)).AddTo(_compositeDisposable);
            playerLinkingDataProxy.HasControl.Subscribe(OnControlStateChanged).AddTo(_compositeDisposable);
        }

        private void OnCurrentPlayerSwitched(bool isClient)
        {
            if (isClient)
            {
                Debug.Log($"Current player is client. Sending inputs");
                _disposableGridLinkingEvents?.Dispose();
            }
            else
            {
                Debug.Log($"Current player is not client. receiving inputs");
                _gridLinkingDataProxy.StartedLink.Subscribe(LinkStartByGrid).AddTo(_disposableGridLinkingEvents);
                _gridLinkingDataProxy.LinkAdded.Subscribe(LinkAddedByGrid).AddTo(_disposableGridLinkingEvents);
                _gridLinkingDataProxy.LinkRemovedAfter.Subscribe(LinkRemoveAfterByGrid).AddTo(_disposableGridLinkingEvents);
            }
        }

        public void Dispose()
        {
            _compositeDisposable?.Dispose();
        }

        private void OnControlStateChanged(bool hasControl)
        {
            Debug.Log($"Control state changed: {hasControl}");
            _eventSystem.enabled = hasControl;
            if (hasControl)
            {
                CheckSelectables();
            }
            else
            {
                ResetSelectables();
            }
        }

        public void LinkStartByGrid(Vector2Int position)
        {
            Debug.Log($"[LINK_STARTED] {position} | Player Is Client {_currentPlayerIsClient.Value}");
            if (_currentPlayerIsClient.Value)
            {
                _gridLinkingDataProxy.RaiseLinkStarted(position);
            }

            _tilesLink.Clear();
            _powerupsInLink.Clear();
            _mergeComboModel = null;

            _direction = Vector2.zero;
            LinkAddedByGrid(position);
        }

        public void LinkAddedByGrid(Vector2Int position)
        {
            Debug.Log($"[LINK_ADDED] {position} | Player Is Client {_currentPlayerIsClient.Value}");
            GridTileView tileView = GetTileByGrid(position.x, position.y);
            _tilesLink.Add(tileView);

            if (_currentPlayerIsClient.Value && _tilesLink.Count > 1)
            {
                tileView.SetConnectorLineActive(true);
            }

            tileView.GridItemView.GridItemCanvas.overrideSorting = true;
            tileView.GridItemView.PlayAnimation("LinkAdd");
            tileView.SetClickSize(0.5f);

            if (tileView.GridItemView.GridItemType < 0)
            {
                _powerupsInLink.Add(tileView.GridItemView);
            }

            CheckSpecial();
            if (_currentPlayerIsClient.Value)
            {
                _gridLinkingDataProxy.RaiseLinkAdded(position);
            }
        }

        private void LinkRemoveByGrid(Vector2Int position)
        {
            GridTileView tileView = GetTileByGrid(position.x, position.y);
            _tilesLink.Remove(tileView);

            if (_currentPlayerIsClient.Value)
            {
                tileView.SetConnectorLineActive(false);
            }

            tileView.GridItemView.GetComponent<Canvas>().overrideSorting = false;
            tileView.GridItemView.PlayAnimation("LinkRemove");

            if (tileView.GridItemView.GridItemType < 0)
            {
                _powerupsInLink.Remove(tileView.GridItemView);
            }

            CheckSpecial();
            tileView.SetClickSize(1);
        }

        public void LinkRemoveAfterByGrid(Vector2Int position)
        {
            GridTileView tileView = GetTileByGrid(position.x, position.y);
            int correctTileIndex = GetIndexInLink(tileView);

            for (int tileIndex = _tilesLink.Count - 1; tileIndex > correctTileIndex; tileIndex--)
            {
                Vector2Int tile = GetIndexInGrid(_tilesLink[tileIndex]);
                LinkRemoveByGrid(tile);
            }

            if (_currentPlayerIsClient.Value)
            {
                _gridLinkingDataProxy.RaiseLinkRemovedAfter(position);
            }
        }

        private int GetIndexInLink(GridTileView tileView)
        {
            for (int tileIndex = 0; tileIndex < _tilesLink.Count; tileIndex++)
            {
                // Get the tile index in the current link of tiles
                if (_tilesLink[tileIndex] == tileView)
                {
                    return tileIndex;
                }
            }

            return -1;
        }

        private int GetIndexInList(GridTileView tileView)
        {
            for (int tileIndex = 0; tileIndex < _gridController.GridModel.Count; tileIndex++)
            {
                if (_gridController.GridModel[tileIndex] == tileView)
                {
                    return tileIndex;
                }
            }

            return -1;
        }

        public Vector2Int GetIndexInGrid(GridTileView tileView)
        {
            int tileListIndex = GetIndexInList(tileView);

            Vector2Int gridSize = _gridController.GridModel.GridSize;

            Vector2Int tileGridIndex = new Vector2Int(tileListIndex % gridSize.x, tileListIndex / gridSize.y);

            return tileGridIndex;
        }

        private GridTileView GetTileByGrid(int gridX, int gridY)
        {
            int listIndex = _gridController.GridModel.GridSize.x * gridY + gridX;
            GridTileView gridTileView = _gridController.GridModel[listIndex];

            return gridTileView;
        }

        private void ExecuteLink()
        {
            Debug.Log($"Is executing link: {_isExecuting} | Tiles Count {_tilesLink.Count}");
            if (_isExecuting) return;
            _isExecuting = true;

            ResetSelectables();
            if (_tilesLink.Count < _gridLinkSettings.MinimumLinkSize)
            {
                CancelExecuteLink();
                _isExecuting = false;

                return;
            }

            float tempExecuteTime = _gridLinkSettings.ExecuteTime;
            float extraExecuteTime = 0;
            _executeTotalTime = 0;
            for (int index = 0; index < _tilesLink.Count; index++)
            {
                LeanTween.rotate(_tilesLink[index].gameObject, Vector3.forward * 0, 0.2f);

                if (_currentPlayerIsClient.Value)
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

                if (index == _tilesLink.Count - 1)
                {
                    gridTileView.GridItemView.IsLastInLink = true;
                }

                if (_currentPlayerIsClient.Value)
                {
                    Debug.Log($"Trying to collect item at tile {tileGridIndex}");
                    _gridLinkingDataProxy.RaiseGridItemCollected(tileGridIndex);
                }

                if (gridItemView.GridItemType > -1 || _powerupsInLink.Count < 2) extraExecuteTime += gridItemView.ExtraExecuteTime;

                _executeTotalTime += tempExecuteTime;

                tempExecuteTime *= _gridLinkSettings.ExecuteTimeMultiplier;
                tempExecuteTime = Mathf.Clamp(tempExecuteTime, _gridLinkSettings.ExecuteTimeMinimum, 999);
            }

            _executeTotalTime += extraExecuteTime;

            if (_currentPlayerIsClient.Value)
            {
                _movesDataProxy.TriggerSpendMoveRequest(-1);
                if (_powerupsInLink.Count > 1)
                {
                    _movesDataProxy.TriggerSpendMoveRequest(-1);
                    _toastView.SetToast(_tilesLink[^1].transform.position, "EXTRA MOVE!", new Color(1, 0.37f, 0.67f, 1));
                }

                if (_powerupsInLink.Count < 1)
                {
                    // todo: fix spawn special RPC spawning
                    // photonView.RPC(nameof(SpawnSpecial), RpcTarget.All, specialIndex, executeTotalTime);
                }

                // todo: fix remove from execute list;
                // tbd: is not used for now
                // if (_currentPlayerIsClient.Value)
                // {
                //     _gridLinkingDataProxy.RaiseRemovedFromExecuteList();
                // }
            }

            _matchTimerController.PauseTimerFor(-1);
            _playerLinkingDataProxy.ChangeControlState(false);

            if (_currentPlayerIsClient.Value)
            {
                _gridLinkingDataProxy.RaiseLinkExecuted();
            }

            EndExecuteLink();
        }

        public void CancelExecuteLink()
        {
            while (_tilesLink.Count > 0)
            {
                Vector2Int tile = GetIndexInGrid(_tilesLink[0]);
                LinkRemoveByGrid(tile);
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

            // todo fix destination
            Collect(gridItemView, gridItemView.transform, delay, gridTileView);
            gridTileView.GridItemView = null;
        }

        public void CollectItemAtGrid(Vector2Int position)
        {
            int listIndex = _gridController.GridModel.GridSize.x * position.y + position.x;
            GridTileView gridTileView = _gridController.GridModel[listIndex];

            CollectItemAtTile(gridTileView, 0f);
        }

        public int CheckSpecial()
        {
            int longestSpecial = 0;
            _specialIndex = -1;

            // Check any special link size, and create powerups accordingly
            for (int index = 0; index < _specialLinks.Count; index++)
            {
                _currentLinkSize = _specialLinks[index].LinkSize;

                if (_currentLinkSize <= _tilesLink.Count && _currentLinkSize > longestSpecial)
                {
                    longestSpecial = _currentLinkSize;
                    _specialIndex = index;
                }
            }

            if (_specialIndex != -1)
            {
                GridItemView gridItemView = _specialLinks[_specialIndex].SpawnItemView;
                if (gridItemView != null && gridItemView.HasOtherOrientations)
                {
                    CheckDirection();
                    GridItemView tempGridItemView = _specialLinks[_specialIndex].SpawnItemView.GetOtherOrientation(_direction);
                    if (tempGridItemView != null)
                    {
                        _specialLinks[_specialIndex].SpawnItemView = tempGridItemView;
                    }
                }
            }

            return _specialIndex;
        }

        // todo: RPC
        public void SpawnSpecial(int index, float delay)
        {
            if (index == -1) return;
            GridItemView gridItemView = _specialLinks[index].SpawnItemView;
            _gridController.CreateGridItem(gridItemView, _tilesLink[^1], delay);

            if (!_currentPlayerIsClient.Value) return;

            // todo: Fix Spawn Special
            // if (_tilesLink.Count >= _gameManager.GetExtraMoveAtLink())
            // {
            //     _gameManager.CurrentPlayerController.photonView.RPC("ChangeMoves", RpcTarget.All, 1);
            // }
        }

        private void Collect(GridItemView gridItemView, Transform target, float delay, GridTileView gridTileView)
        {
            if (gridItemView.IsMerging == false && _mergeComboModel == null) gridItemView.SendMessage("Execute", new ExecuteDataModel(gridTileView, delay), SendMessageOptions.DontRequireReceiver);

            // This is here to make sure that combo powerups trigger if the last tile in the link is a powerup
            if (_powerupsInLink.Count > 1 && gridItemView.IsLastInLink == true)
            {
                gridItemView.IsClearing = gridItemView.IsMerging = false;
                //delay = executeTotalTime * 1.2f;
            }

            if (gridItemView.IsMerging)
            {
                gridItemView.PlayDelayedAnimation("LinkAdd", delay - 0.2f);
            }
            else if (!string.IsNullOrEmpty(gridItemView.ClearAnimationProperty))
            {
                gridItemView.PlayDelayedAnimation(gridItemView.ClearAnimationProperty, delay - 0.2f);
            }

            gridItemView.DisableGlow();
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
                    if (gridItemView.GridItemType < 0 && _powerupsInLink.Count > 1 && gridItemView.CanMerge == true) return;
                }

                // todo: fix this
                // if (_gameManager.CurrentPlayerController.photonView.IsMine)
                // {
                //     _gameManager.CurrentPlayerController.AddBonus(1, 0.5f);
                // }

                LeanTween.rotate(gridItemView.gameObject, Vector3.forward * Random.Range(-30, 30), _gridLinkSettings.CollectTime * 0.7f).setEaseOutSine();
                LeanTween.move(gridItemView.gameObject, gridItemView.transform.position + randomOffset, _gridLinkSettings.CollectTime * 0.7f).setEaseOutSine().setOnComplete(() =>
                {
                    LeanTween.scale(gridItemView.gameObject, Vector3.one * 0.6f, _gridLinkSettings.CollectTime * 0.3f).setEaseInSine();
                    LeanTween.moveX(gridItemView.gameObject, target.position.x, _gridLinkSettings.CollectTime * 0.3f).setEaseOutSine();
                    LeanTween.moveY(gridItemView.gameObject, target.position.y, _gridLinkSettings.CollectTime * 0.3f).setEaseInSine().setOnComplete(() =>
                    {
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

                _mergeCombosController.MergeEffect(gridTileView.transform.position);

                _mergeComboModel = _mergeCombosController.GetMergeCombo(_powerupsInLink);

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
            // _executeList.Add(addObject);
            // CancelInvoke(nameof(EndExecuteLink));
        }

        public void RemoveFromExecuteList()
        {
            // _executeList.Remove(this);
            // CheckExecuteLink();
        }

        public void CheckExecuteLink()
        {
            // if (_executeList.Count > 0) return;
            //
            // Observable.Timer(TimeSpan.FromSeconds(0.3f))
            //     .Subscribe(_ => EndExecuteLink()).AddTo(_compositeDisposable);
        }

        private void EndExecuteLink()
        {
            Debug.Log($"[CLIENT] Trying to end execute link");
            _isExecuting = false;
            _gridController.CollapseTiles();
            _matchTimerController.StartTimer();
            _playerLinkingDataProxy.ChangeControlState(true);
            _tilesLink.Clear();
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

        public void CheckSelectables()
        {
            for (int listIndex = _tilesLink.Count - 1; listIndex >= 0; listIndex--)
            {
                GridItemView currentItemView = _tilesLink[listIndex].GridItemView;

                if (currentItemView)
                {
                    if (currentItemView.GridItemType == LinkType || LinkType < 0 || currentItemView.GridItemType < 0)
                    {
                        currentItemView.SetAnimatorBool("Selectable", true);
                    }
                    else
                    {
                        currentItemView.SetAnimatorBool("Selectable", false);
                    }

                    // Set the color of the line based on the tile
                    if (LinkType < 0)
                    {
                        _tilesLink[listIndex].SetConnectorAnimator(true);
                        currentItemView.SetGlowAnimator(true);
                    }
                    else
                    {
                        _tilesLink[listIndex].SetConnectorLineColor(currentItemView.Color);
                        _tilesLink[listIndex].SetConnectorAnimator(false);

                        currentItemView.SetGlowAnimator(false);
                    }
                }
            }
        }

        private void ResetSelectables()
        {
            for (int listIndex = _tilesLink.Count - 1; listIndex >= 0; listIndex--)
            {
                GridItemView currentItemView = _tilesLink[listIndex].GridItemView;

                if (currentItemView)
                {
                    currentItemView.SetAnimatorBool("Selectable", true);
                }
            }
        }

        private void CheckDirection()
        {
            _direction = _tilesLink.Count < 2
                ? Vector2.zero
                : (_tilesLink[^1].transform.position - _tilesLink[^2].transform.position).normalized;
        }

    }
}
