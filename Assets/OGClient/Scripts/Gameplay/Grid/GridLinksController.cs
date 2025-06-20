using System;
using UniRx;
using UnityEngine;
using System.Linq;
using OGClient.Gameplay.Timers;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using OGClient.Gameplay.Grid.Models;
using OGClient.Gameplay.Grid.Configs;
using OGClient.Gameplay.Grid.MergeCombos;
using UnityEngine.EventSystems;
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

        private readonly GameManager _gameManager;
        private readonly EventSystem _eventSystem;
        private readonly ScriptableGridLinkSettings _gridLinkSettings;
        private readonly MatchTimerController _matchTimerController;
        private readonly GridController _gridController;
        private readonly MergeCombosController _mergeCombosController;

        private readonly CompositeDisposable _compositeDisposable = new ();

        public IReadOnlyList<GridTileView> TilesLink => _tilesLink;

        public GridLinksController(GridController gridController, MergeCombosController mergeCombosController, MatchTimerController matchTimerController,
                                   ScriptableGridLinkSettings gridLinkSettings, EventSystem eventSystem, GameManager gameManager)
        {
            _eventSystem = eventSystem;
            _gridController = gridController;
            _mergeCombosController = mergeCombosController;
            _matchTimerController = matchTimerController;
            _gridLinkSettings = gridLinkSettings;
            _gameManager = gameManager;

            _specialLinks.AddRange(_gridLinkSettings.SpecialLinks);

            Observable.EveryUpdate().Subscribe(CheckForLmbUp).AddTo(_compositeDisposable);
        }

        public void Dispose()
        {
            _compositeDisposable?.Dispose();
        }

        public void LoseControlFor(float delay = -1)
        {
            _eventSystem.enabled = false;

            if (delay > 0)
            {
                Observable.Timer(TimeSpan.FromSeconds(delay)).Subscribe(_ => RegainControl()).AddTo(_compositeDisposable);
            }
        }

        public void RegainControl()
        {
            _eventSystem.enabled = true;
        }

        public void LinkStartByGrid(int gridX, int gridY)
        {

            _tilesLink.Clear();
            _powerupsInLink.Clear();
            _mergeComboModel = null;

            _direction = Vector2.zero;

            LinkAddByGrid(gridX, gridY);
        }

        public void LinkAddByGrid(int gridX, int gridY)
        {
            GridTileView tileView = GetTileByGrid(gridX, gridY);
            _tilesLink.Add(tileView);

            // if (_gameManager.CurrentPlayerController.photonView.IsMine)
            // {
            //     if (tileLink.Count > 1)
            //         tileView.SetConnectorLineActive(true);
            // }

            //LeanTween.rotate(tile.GetCurrentItem().gameObject, Vector3.forward * 10, 0.2f);

            tileView.GridItemView.GridItemCanvas.overrideSorting = true;

            tileView.GridItemView.PlayAnimation("LinkAdd");

            if (tileView.GridItemView.GridItemType < 0) _powerupsInLink.Add(tileView.GridItemView);

            CheckSpecial();

            tileView.SetClickSize(0.5f);
        }

        public void LinkStart(GridTileView tileView)
        {
            _tilesLink.Clear();
            _powerupsInLink.Clear();
            _mergeComboModel = null;
            //_gameManager.currentPlayer.booster.EndActivation();

            _direction = Vector2.zero;

            LinkAdd(tileView);
        }

        private void CheckForLmbUp(long l)
        {
            if (!Input.GetMouseButtonUp(0)) return;
            // !(_gameManager.CurrentPlayerController && _gameManager.CurrentPlayerController.photonView.IsMine && _gameManager.CurrentPlayerController.moves > 0)

            ExecuteLink();
        }

        private void LinkAdd(GridTileView tileView)
        {
            _tilesLink.Add(tileView);

            if (_tilesLink.Count > 1)
                tileView.SetConnectorLineActive(true);

            tileView.GridItemView.GridItemCanvas.overrideSorting = true;

            tileView.GridItemView.PlayAnimation("LinkAdd");

            if (tileView.GridItemView.GridItemType < 0) _powerupsInLink.Add(tileView.GridItemView);

            CheckSpecial();

            tileView.SetClickSize(0.5f);
        }



        private void LinkRemove(GridTileView tileView)
        {
            _tilesLink.Remove(tileView);

            tileView.SetConnectorLineActive(false);

            //LeanTween.rotate(tile.GetCurrentItem().gameObject, Vector3.forward * 0, 0.2f);

            tileView.GridItemView.GetComponent<Canvas>().overrideSorting = false;

            tileView.GridItemView.PlayAnimation("LinkRemove");

            if (tileView.GridItemView.GridItemType < 0) _powerupsInLink.Remove(tileView.GridItemView);

            CheckSpecial();

            tileView.SetClickSize(1);
        }

        private void LinkRemoveByGrid(int gridX, int gridY)
        {
            GridTileView tileView = GetTileByGrid(gridX, gridY);
            _tilesLink.Remove(tileView);

            // if (_gameManager.CurrentPlayerController.photonView.IsMine)
            // {
            //     tileView.SetConnectorLineActive(false);
            // }

            tileView.GridItemView.GetComponent<Canvas>().overrideSorting = false;

            tileView.GridItemView.PlayAnimation("LinkRemove");

            if (tileView.GridItemView.GridItemType < 0) _powerupsInLink.Remove(tileView.GridItemView);

            CheckSpecial();
            tileView.SetClickSize(1);
        }

        public void LinkRemoveAfterByGrid(int gridX, int gridY)
        {
            GridTileView tileView = GetTileByGrid(gridX, gridY);

            int correctTileIndex = GetIndexInLink(tileView);

            for (int tileIndex = _tilesLink.Count - 1; tileIndex > correctTileIndex; tileIndex--)
                //for (int tileIndex = correctTileIndex; tileIndex < tileLink.Count; tileIndex++)
            {
                Vector2Int tile1 = GetIndexInGrid(_tilesLink[tileIndex]);
                // Remove all the tiles after the correct one
                LinkRemoveByGrid(tile1.x, tile1.y);
            }
        }

        public void LinkRemoveAfter(GridTileView tileView)
        {
            int correctTileIndex = GetIndexInLink(tileView);

            for (int tileIndex = _tilesLink.Count - 1; tileIndex > correctTileIndex; tileIndex--)
                //for (int tileIndex = correctTileIndex; tileIndex < tileLink.Count; tileIndex++)
            {
                // Remove all the tiles after the correct one
                LinkRemove(_tilesLink[tileIndex]);
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
            List<GridTileView> tileList = _gridController.Tiles.ToList();
            for (int tileIndex = 0; tileIndex < tileList.Count; tileIndex++)
            {
                // Get the tile index in the list of all tiles
                if (tileList[tileIndex] == tileView)
                {
                    return tileIndex;
                }
            }

            return -1;
        }

        public Vector2Int GetIndexInGrid(GridTileView tileView)
        {
            int tileListIndex = GetIndexInList(tileView);

            Vector2Int gridSize = _gridController.GridSize;

            Vector2Int tileGridIndex = new Vector2Int(tileListIndex % gridSize.x, tileListIndex / gridSize.y);

            return tileGridIndex;
        }

        private GridTileView GetTileByGrid(int gridX, int gridY)
        {
            int listIndex = _gridController.GridSize.x * gridY + gridX;

            GridTileView gridTileView = _gridController.Tiles.ToArray()[listIndex];

            return gridTileView;
        }

        private void ExecuteLink()
        {
            if (_isExecuting == true) return;
            _isExecuting = true;

            //if (tileLink.Count > 0)
            ResetSelectables();

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

                // if (_gameManager.CurrentPlayerController.photonView.IsMine)
                // {
                //     tileLink[index].SetConnectorLineActive(false);
                // }

                GridTileView gridTileView = _tilesLink[index];

                GridItemView gridItemView = gridTileView.GridItemView;
                Vector2Int tileGridIndex = GetIndexInGrid(gridTileView);
                if (gridTileView.GridItemView.GridItemType < 0)
                {
                    tempExecuteTime = _gridLinkSettings.ExecuteTime;
                }

                if (index == _tilesLink.Count - 1) gridTileView.GridItemView.IsLastInLink = true;

                // if (_gameManager.CurrentPlayerController.photonView.IsMine)
                // {
                //     photonView.RPC("CollectItemAtGrid", RpcTarget.All, tileGridIndex.x, tileGridIndex.y, executeTotalTime + extraExecuteTime);
                // }

                if (gridItemView.GridItemType > -1 || _powerupsInLink.Count < 2) extraExecuteTime += gridItemView.ExtraExecuteTime;

                _executeTotalTime += tempExecuteTime;

                tempExecuteTime *= _gridLinkSettings.ExecuteTimeMultiplier;
                tempExecuteTime = Mathf.Clamp(tempExecuteTime, _gridLinkSettings.ExecuteTimeMinimum, 999);
            }

            _executeTotalTime += extraExecuteTime;

            //
            // if (_gameManager.CurrentPlayerController.photonView.IsMine)
            // {
            //     _gameManager.CurrentPlayerController.photonView.RPC("ChangeMoves", RpcTarget.All, -1);
            //
            //     if (powerupsInLink.Count > 1)
            //     {
            //         _gameManager.CurrentPlayerController.photonView.RPC("ChangeMoves", RpcTarget.All, 1);
            //
            //         _gameManager.GridPlayerController.ToastView.SetToast(tileLink[tileLink.Count - 1].transform.position, "EXTRA MOVE!", new Color(1, 0.37f, 0.67f, 1));
            //     }
            //
            //     if (powerupsInLink.Count < 1) photonView.RPC(nameof(SpawnSpecial), RpcTarget.All, specialIndex, executeTotalTime);
            //
            //     Invoke(nameof(RPC_RemoveFromExecuteList), executeTotalTime);
            // }

            _matchTimerController.PauseTimerFor(-1);
            LoseControlFor(0);
        }

        public void CancelExecuteLink()
        {
            while (_tilesLink.Count > 0)
            {
                Vector2Int tile = GetIndexInGrid(_tilesLink[0]);
                // Remove all the tiles after the correct one
                LinkRemoveByGrid(tile.x, tile.y);
                //LinkRemove();
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

            // Collect(gridItemView, _gameManager.CurrentPlayerController.bonusText.transform, delay, gridTileView);

            gridTileView.GridItemView = null;
        }

        public void CollectItemAtGrid(int gridX, int gridY, float delay)
        {
            // Translate from grid index to list/array index
            int listIndex = _gridController.GridSize.x * gridY + gridX;

            GridTileView gridTileView = _gridController.Tiles.ToArray()[listIndex];

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
            if (index != -1)
            {
                GridItemView gridItemView = _specialLinks[index].SpawnItemView;
                /*if ( _specialLinks[index].spawnItem.otherOrientations.Length > 0 )
            {
                GridItem tempGridItem = _specialLinks[index].spawnItem.GetOtherOrientation(direction);

                if (tempGridItem != null) gridItem = tempGridItem;
            }*/



                _gridController.SpawnItem(gridItemView, _tilesLink[_tilesLink.Count - 1], delay);

                // if (_gameManager.CurrentPlayerController.photonView.IsMine)
                // {
                //     if (tileLink.Count >= _gameManager.GetExtraMoveAtLink()) _gameManager.CurrentPlayerController.photonView.RPC("ChangeMoves", RpcTarget.All, 1);
                // }

                //_gameManager.currentPlayer.ChangeMoves(1);
            }
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

            // TODO: fix it
            // if (gridItemView.IsMerging)
            // {
            //     gridItemView.PlayDelayedAnimation("LinkAdd", delay - 0.2f);
            // }
            // else if (gridItemView.clearAnimation != "")
            // {
            //     gridItemView.PlayDelayedAnimation(gridItemView.clearAnimation, delay - 0.2f);
            // }
            //
            // gridItemView.glowAnimator.enabled = false;
            // gridItemView.glowImage.color = Color.white;

            Vector3 randomOffset = Random.insideUnitCircle * gridItemView.ThrowDistance;

            //LeanTween.cancel(gridItem.gameObject);
            LeanTween.scale(gridItemView.gameObject, Vector3.one * 1.4f, _gridLinkSettings.CollectTime * 0.3f).setDelay(delay).setEaseInSine().setOnComplete(() =>
            {
                if (gridItemView.IsClearing == true) return;

                gridItemView.IsClearing = true;
                gridItemView.TryToCollect();

                if (gridItemView.IsMerging == true) return;

                if (_powerupsInLink.Count < 2 || gridItemView.GridItemType > -1 || gridItemView.IsLastInLink == false || _isExecuting == false)
                {
                    gridItemView.TryToClear();
                }

                if (gridItemView.IsLastInLink == true)
                {
                    ExecuteLastInLink(gridTileView, target, _gridLinkSettings.CollectTime * 0.3f);

                    if (gridItemView.GridItemType < 0 && _powerupsInLink.Count > 1 && gridItemView.CanMerge == true) return;
                }

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
                .Subscribe(_ => EndExecuteLink()).AddTo(_compositeDisposable);
        }

        private void EndExecuteLink()
        {
            _isExecuting = false;

            // todo: fix end turn callback
            if (_gameManager.NetworkPlayerController.MovesLeft == 0)
            {
                // _gameManager.EndTurn();
            }

            _tilesLink.Clear();

            _gridController.CollapseTiles();

            _matchTimerController.StartTimer();
            RegainControl();
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
            List<GridTileView> tileList = _gridController.Tiles.ToList();
            for (int listIndex = tileList.Count - 1; listIndex >= 0; listIndex--)
            {
                GridItemView currentItemView = tileList[listIndex].GridItemView;

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
                        tileList[listIndex].SetConnectorAnimator(true);
                        currentItemView.SetGlowAnimator(true);
                    }
                    else
                    {
                        tileList[listIndex].SetConnectorLineColor(currentItemView.Color);
                        tileList[listIndex].SetConnectorAnimator(false);

                        currentItemView.SetGlowAnimator(false);
                    }
                }
            }
        }

        private void ResetSelectables()
        {
            List<GridTileView> tileList = _gridController.Tiles.ToList();

            for (int listIndex = tileList.Count - 1; listIndex >= 0; listIndex--)
            {
                GridItemView currentItemView = tileList[listIndex].GridItemView;

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
