using UniRx;
using System;
using UnityEngine;
using System.Collections.Generic;
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

        private readonly Subject<Unit> _executeLink = new();

        private MergeComboModel _mergeComboModel;

        private readonly GridModel _gridModel;
        private readonly ScriptableGridLinkSettings _gridLinkSettings;

        private readonly List<GridItemView> _powerupsInLink = new ();
        private readonly List<GameObject> _executeList = new ();
        private readonly List<GridTileView> _tilesLink = new ();
        private readonly List<SpecialLinkModel> _specialLinks = new();

        private int _specialIndex = -1;
        private int _currentLinkSize;
        private bool _isExecuting;
        private bool _thisClientIsCurrentPlayer;
        private float _executeTotalTime;
        private Vector2 _direction = Vector2.zero;

        public int Count => _tilesLink.Count;
        public GridTileView this[int index] => _tilesLink[index];
        public bool Contains(GridTileView tileView) => _tilesLink.Contains(tileView);

        public int LinkType { get; set; } = -1;

        public IObservable<Unit> LinkExecuted => _executeLink;

        public void SetThisClientIsCurrentPlayer(bool isCurrentPlayer)
        {
            _thisClientIsCurrentPlayer = isCurrentPlayer;
        }

        public void LinkStart(GridTileView tileView)
        {
            _tilesLink.Clear();
            _powerupsInLink.Clear();
            _mergeComboModel = null;

            // todo: fix booster activation
            // _gameManager.currentPlayer.booster.EndActivation();

            _direction = Vector2.zero;

            LinkAdd(tileView);
        }

        private void ExecuteLink()
        {
            if (_isExecuting) return;
            _isExecuting = true;

            // If the link is too short, deselect the tiles
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

                if (_thisClientIsCurrentPlayer)
                {
                    // photonView.RPC("CollectItemAtGrid", RpcTarget.All, tileGridIndex.x, tileGridIndex.y, executeTotalTime + extraExecuteTime);
                }

                if (gridItemView.GridItemType > -1 || _powerupsInLink.Count < 2) extraExecuteTime += gridItemView.ExtraExecuteTime;

                _executeTotalTime += tempExecuteTime;

                tempExecuteTime *= _gridLinkSettings.ExecuteTimeMultiplier;
                tempExecuteTime = Mathf.Clamp(tempExecuteTime, _gridLinkSettings.ExecuteTimeMinimum, 999);
            }

            _executeTotalTime += extraExecuteTime;


            if (_thisClientIsCurrentPlayer)
            {
                // todo: fix moves
                // _gameManager.CurrentPlayerController.photonView.RPC("ChangeMoves", RpcTarget.All, -1);

                if (_powerupsInLink.Count > 1)
                {
                    // todo: fix moves
                    // _gameManager.CurrentPlayerController.photonView.RPC("ChangeMoves", RpcTarget.All, 1);
                    // _gameManager.GridPlayerController.ToastView.SetToast(tileLink[tileLink.Count - 1].transform.position, "EXTRA MOVE!", new Color(1, 0.37f, 0.67f, 1));
                }

                if (_powerupsInLink.Count < 1)
                {
                    // todo: fix special
                    // photonView.RPC(nameof(SpawnSpecial), RpcTarget.All, specialIndex, executeTotalTime);
                }

                // Invoke(nameof(RPC_RemoveFromExecuteList), executeTotalTime);
            }

            // todo: add callback
            // _matchTimerController.PauseTimerFor(-1);
            // _gridLinksDataProxy.ChangeControlState(false);

            _executeLink.OnNext(Unit.Default);
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

        public void SpawnSpecial(int index, float delay)
        {
            if (index == -1) return;

            // todo: fix spawn specials
            // GridItemView gridItemView = _specialLinks[index].SpawnItemView;
            // _gridController.SpawnItem(gridItemView, _tilesLink[^1], delay);
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
