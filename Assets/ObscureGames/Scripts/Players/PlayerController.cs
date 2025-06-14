using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;
using ObscureGames.Gameplay;
using ObscureGames.Gameplay.Grid;
using ObscureGames.Gameplay.Grid.MergeCombos;
using ObscureGames.Gameplay.Grid.Models;
using ObscureGames.Gameplay.UI;
using Photon.Pun;
using Zenject;
using Random = UnityEngine.Random;
namespace ObscureGames.Players
{
    public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable
    {
        public List<GridTileView> tileLink = new List<GridTileView>();
        internal List<GameObject> executeList = new List<GameObject>();
        internal bool isExecuting = false;

        internal int linkType = -1; // -1 is any type
        public int minimumLinkSize = 2; // The minimum number of tiles needed in a link to execute it

        [SerializeField] private EventSystem eventSystem;

        private bool inControl = false;

        [SerializeField] private float collectTime = 0.5f;
        [SerializeField] private float executeTime = 0.1f;
        [SerializeField] private float executeTimeMultipier = 0.9f;
        [SerializeField] private float executeTimeMinimum = 0.01f;
        [SerializeField] private float executeTotalTime = 0;
        [SerializeField] private int longestSpecial;
        [SerializeField] private int currentLinkSize;
        public ToastView ToastView;

        public int specialIndex = -1;
        public List<GridItemView> powerupsInLink = new List<GridItemView>();

        public Vector2 direction = Vector2.zero;

        private MergeComboModel _mergeComboModel;
        public bool hasPowerups = false;

        private GameManager _gameManager;
        private GridController _gridController;
        private MergeCombosController _mergeCombosController;

        [Inject]
        public void Construct(GridController gridController, GameManager gameManager, MergeCombosController mergeCombosController)
        {
            _gridController = gridController;
            _gameManager = gameManager;
            _mergeCombosController = mergeCombosController;
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(isExecuting);
                stream.SendNext(linkType);
                stream.SendNext(minimumLinkSize);
                stream.SendNext(inControl);
                stream.SendNext(collectTime);
                stream.SendNext(executeTime);
                stream.SendNext(executeTimeMultipier);
                stream.SendNext(executeTimeMinimum);
                stream.SendNext(executeTotalTime);
                stream.SendNext(specialIndex);
                stream.SendNext(direction);
                stream.SendNext(hasPowerups);
            }
            else
            {
                isExecuting = (bool)stream.ReceiveNext();
                linkType = (int)stream.ReceiveNext();
                minimumLinkSize = (int)stream.ReceiveNext();
                inControl = (bool)stream.ReceiveNext();
                collectTime = (float)stream.ReceiveNext();
                executeTime = (float)stream.ReceiveNext();
                executeTimeMultipier = (float)stream.ReceiveNext();
                executeTimeMinimum = (float)stream.ReceiveNext();
                executeTotalTime = (float)stream.ReceiveNext();
                specialIndex = (int)stream.ReceiveNext();
                direction = (Vector2)stream.ReceiveNext();
                hasPowerups = (bool)stream.ReceiveNext();
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            ToastView = FindObjectOfType<ToastView>();
            eventSystem = FindObjectOfType<EventSystem>();
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetButtonUp("Fire1") && _gameManager.CurrentPlayer && _gameManager.CurrentPlayer.photonView.IsMine && _gameManager.CurrentPlayer.moves > 0)
            {
                //ExecuteLink();
                photonView.RPC("ExecuteLink", RpcTarget.All);
                //();
            }
        }

        public void LoseControl(float delay)
        {
            eventSystem.enabled = false;

            CancelInvoke(nameof(RegainControl));
            if (delay > 0) Invoke(nameof(RegainControl), delay);
        }

        public void RegainControl()
        {
            eventSystem.enabled = true;
        }

        [PunRPC]
        public void LinkStartByGrid(int gridX, int gridY)
        {

            tileLink.Clear();
            powerupsInLink.Clear();
            _mergeComboModel = null;

            direction = Vector2.zero;

            LinkAddByGrid(gridX, gridY);
        }

        [PunRPC]
        public void LinkAddByGrid(int gridX, int gridY)
        {
            GridTileView tileView = GetTileByGrid(gridX, gridY);
            tileLink.Add(tileView);


            if (_gameManager.CurrentPlayer.photonView.IsMine)
            {
                if (tileLink.Count > 1)
                    tileView.SetConnectorLineActive(true);
            }


            //LeanTween.rotate(tile.GetCurrentItem().gameObject, Vector3.forward * 10, 0.2f);

            tileView.GridItemView.GridItemCanvas.overrideSorting = true;

            tileView.GridItemView.PlayAnimation("LinkAdd");

            if (tileView.GridItemView.GridItemType < 0) powerupsInLink.Add(tileView.GridItemView);

            CheckSpecial();

            tileView.SetClickSize(0.5f);
        }

        public void LinkStart(GridTileView tileView)
        {
            tileLink.Clear();
            powerupsInLink.Clear();
            _mergeComboModel = null;
            //_gameManager.currentPlayer.booster.EndActivation();

            direction = Vector2.zero;

            LinkAdd(tileView);
        }



        public void LinkAdd(GridTileView tileView)
        {
            tileLink.Add(tileView);

            if (tileLink.Count > 1)
                tileView.SetConnectorLineActive(true);

            tileView.GridItemView.GridItemCanvas.overrideSorting = true;

            tileView.GridItemView.PlayAnimation("LinkAdd");

            if (tileView.GridItemView.GridItemType < 0) powerupsInLink.Add(tileView.GridItemView);

            CheckSpecial();

            tileView.SetClickSize(0.5f);
        }



        public void LinkRemove(GridTileView tileView)
        {
            tileLink.Remove(tileView);

            tileView.SetConnectorLineActive(false);

            //LeanTween.rotate(tile.GetCurrentItem().gameObject, Vector3.forward * 0, 0.2f);

            tileView.GridItemView.GetComponent<Canvas>().overrideSorting = false;

            tileView.GridItemView.PlayAnimation("LinkRemove");

            if (tileView.GridItemView.GridItemType < 0) powerupsInLink.Remove(tileView.GridItemView);

            CheckSpecial();

            tileView.SetClickSize(1);
        }

        public void LinkRemoveByGrid(int gridX, int gridY)
        {
            GridTileView tileView = GetTileByGrid(gridX, gridY);
            tileLink.Remove(tileView);

            if (_gameManager.CurrentPlayer.photonView.IsMine)
            {
                tileView.SetConnectorLineActive(false);
            }

            tileView.GridItemView.GetComponent<Canvas>().overrideSorting = false;

            tileView.GridItemView.PlayAnimation("LinkRemove");

            if (tileView.GridItemView.GridItemType < 0) powerupsInLink.Remove(tileView.GridItemView);

            CheckSpecial();
            tileView.SetClickSize(1);
        }

        [PunRPC]
        public void LinkRemoveAfterByGrid(int gridX, int gridY)
        {
            GridTileView tileView = GetTileByGrid(gridX, gridY);

            int correctTileIndex = GetIndexInLink(tileView);

            for (int tileIndex = tileLink.Count - 1; tileIndex > correctTileIndex; tileIndex--)
                //for (int tileIndex = correctTileIndex; tileIndex < tileLink.Count; tileIndex++)
            {
                Vector2Int tile1 = GetIndexInGrid(tileLink[tileIndex]);
                // Remove all the tiles after the correct one
                LinkRemoveByGrid(tile1.x, tile1.y);
            }
        }

        public void LinkRemoveAfter(GridTileView tileView)
        {
            int correctTileIndex = GetIndexInLink(tileView);

            for (int tileIndex = tileLink.Count - 1; tileIndex > correctTileIndex; tileIndex--)
                //for (int tileIndex = correctTileIndex; tileIndex < tileLink.Count; tileIndex++)
            {
                // Remove all the tiles after the correct one
                LinkRemove(tileLink[tileIndex]);
            }
        }

        public int GetIndexInLink(GridTileView tileView)
        {
            for (int tileIndex = 0; tileIndex < tileLink.Count; tileIndex++)
            {
                // Get the tile index in the current link of tiles
                if (tileLink[tileIndex] == tileView)
                {
                    return tileIndex;
                }
            }

            return -1;
        }

        public int GetIndexInList(GridTileView tileView)
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

        public GridTileView GetTileByGrid(int gridX, int gridY)
        {
            int listIndex = _gridController.GridSize.x * gridY + gridX;

            GridTileView gridTileView = _gridController.Tiles.ToArray()[listIndex];

            return gridTileView;
        }


        internal void SetMinimumLinkSize(int setValue)
        {
            minimumLinkSize = setValue;
        }

        [PunRPC]
        public void ExecuteLink()
        {
            if (isExecuting == true) return;
            isExecuting = true;

            //if (tileLink.Count > 0)
            ResetSelectables();

            // If the link is too short, deselect the tiles
            if (tileLink.Count < minimumLinkSize)
            {
                CancelExecuteLink();
                isExecuting = false;

                return;
            }

            AddToExecuteList(this.gameObject);

            float tempExecuteTime = executeTime;
            float extraExecuteTime = 0;
            executeTotalTime = 0;

            for (int index = 0; index < tileLink.Count; index++)
            {
                LeanTween.rotate(tileLink[index].gameObject, Vector3.forward * 0, 0.2f);

                if (_gameManager.CurrentPlayer.photonView.IsMine)
                {
                    tileLink[index].SetConnectorLineActive(false);
                }

                GridTileView gridTileView = tileLink[index];

                GridItemView gridItemView = gridTileView.GridItemView;
                Vector2Int tileGridIndex = GetIndexInGrid(gridTileView);
                if (gridTileView.GridItemView.GridItemType < 0)
                {
                    tempExecuteTime = executeTime;
                    hasPowerups = true;
                }

                if (index == tileLink.Count - 1) gridTileView.GridItemView.IsLastInLink = true;

                if (_gameManager.CurrentPlayer.photonView.IsMine)
                {
                    photonView.RPC("CollectItemAtGrid", RpcTarget.All, tileGridIndex.x, tileGridIndex.y, executeTotalTime + extraExecuteTime);
                }

                if (gridItemView.GridItemType > -1 || powerupsInLink.Count < 2) extraExecuteTime += gridItemView.ExtraExecuteTime;

                executeTotalTime += tempExecuteTime;

                tempExecuteTime *= executeTimeMultipier;

                tempExecuteTime = Mathf.Clamp(tempExecuteTime, executeTimeMinimum, 999);
            }

            executeTotalTime += extraExecuteTime;


            if (_gameManager.CurrentPlayer.photonView.IsMine)
            {
                _gameManager.CurrentPlayer.photonView.RPC("ChangeMoves", RpcTarget.All, -1);

                if (powerupsInLink.Count > 1)
                {
                    _gameManager.CurrentPlayer.photonView.RPC("ChangeMoves", RpcTarget.All, 1);

                    _gameManager.PlayerController.ToastView.SetToast(tileLink[tileLink.Count - 1].transform.position, "EXTRA MOVE!", new Color(1, 0.37f, 0.67f, 1));
                }

                if (powerupsInLink.Count < 1) photonView.RPC(nameof(SpawnSpecial), RpcTarget.All, specialIndex, executeTotalTime);

                Invoke(nameof(RPC_RemoveFromExecuteList), executeTotalTime);
            }

            _gameManager.PauseTime(0);
            LoseControl(0);
        }

        public void CancelExecuteLink()
        {
            while (tileLink.Count > 0)
            {
                Vector2Int tile = GetIndexInGrid(tileLink[0]);
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
            if (gridItemView.GridItemType < 0 && powerupsInLink.Count > 1)
            {
                gridItemView.IsMerging = true;
            }

            Collect(gridItemView, _gameManager.CurrentPlayer.bonusText.transform, delay, gridTileView);

            gridTileView.GridItemView = null;
        }

        [PunRPC]
        public void CollectItemAtGrid(int gridX, int gridY, float delay)
        {
            // Translate from grid index to list/array index
            int listIndex = _gridController.GridSize.x * gridY + gridX;

            GridTileView gridTileView = _gridController.Tiles.ToArray()[listIndex];

            CollectItemAtTile(gridTileView, delay);
        }

        //[PunRPC]
        public int CheckSpecial()
        {
            int longestSpecial = 0;
            specialIndex = -1;

            // Check any special link size, and create powerups accordingly
            for (int index = 0; index < _gameManager.SpecialLinks.Length; index++)
            {
                currentLinkSize = _gameManager.SpecialLinks[index].LinkSize;

                if (currentLinkSize <= tileLink.Count && currentLinkSize > longestSpecial)
                {
                    longestSpecial = currentLinkSize;
                    specialIndex = index;
                }
            }

            if (specialIndex != -1)
            {
                GridItemView gridItemView = _gameManager.SpecialLinks[specialIndex].SpawnItemView;
                if (gridItemView != null && gridItemView.HasOtherOrientations)
                {
                    CheckDirection();
                    GridItemView tempGridItemView = _gameManager.SpecialLinks[specialIndex].SpawnItemView.GetOtherOrientation(direction);
                    if (tempGridItemView != null)
                    {
                        _gameManager.SpecialLinks[specialIndex].SpawnItemView = tempGridItemView;
                    }
                }
            }

            return specialIndex;
        }

        [PunRPC]
        public void SpawnSpecial(int index, float delay)
        {
            if (index != -1)
            {
                GridItemView gridItemView = _gameManager.SpecialLinks[index].SpawnItemView;

                /*if ( _gameManager.specialLinks[index].spawnItem.otherOrientations.Length > 0 )
            {
                GridItem tempGridItem = _gameManager.specialLinks[index].spawnItem.GetOtherOrientation(direction);

                if (tempGridItem != null) gridItem = tempGridItem;
            }*/



                _gridController.SpawnItem(gridItemView, tileLink[tileLink.Count - 1], delay);

                if (_gameManager.CurrentPlayer.photonView.IsMine)
                {
                    if (tileLink.Count >= _gameManager.GetExtraMoveAtLink()) _gameManager.CurrentPlayer.photonView.RPC("ChangeMoves", RpcTarget.All, 1);
                }

                //_gameManager.currentPlayer.ChangeMoves(1);
            }
        }

        void Collect(GridItemView gridItemView, Transform target, float delay, GridTileView gridTileView)
        {
            if (gridItemView.IsMerging == false && _mergeComboModel == null) gridItemView.SendMessage("Execute", new ExecuteDataModel(gridTileView, delay), SendMessageOptions.DontRequireReceiver);

            // This is here to make sure that combo powerups trigger if the last tile in the link is a powerup
            if (powerupsInLink.Count > 1 && gridItemView.IsLastInLink == true)
            {
                gridItemView.IsClearing = gridItemView.IsMerging = false;
                //delay = executeTotalTime * 1.2f;
            }

            // TODO: fix it
            // if (gridItemView.isMerging)
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
            LeanTween.scale(gridItemView.gameObject, Vector3.one * 1.4f, collectTime * 0.3f).setDelay(delay).setEaseInSine().setOnComplete(() =>
            {
                if (gridItemView.IsClearing == true) return;

                gridItemView.IsClearing = true;
                gridItemView.TryToCollect();

                if (gridItemView.IsMerging == true) return;

                if (powerupsInLink.Count < 2 || gridItemView.GridItemType > -1 || gridItemView.IsLastInLink == false || isExecuting == false)
                {
                    gridItemView.TryToClear();
                }

                if (gridItemView.IsLastInLink == true)
                {
                    ExecuteLastInLink(gridTileView, target, collectTime * 0.3f);

                    if (gridItemView.GridItemType < 0 && powerupsInLink.Count > 1 && gridItemView.CanMerge == true) return;
                }

                if (_gameManager.CurrentPlayer.photonView.IsMine)
                {
                    _gameManager.CurrentPlayer.AddBonus(1, 0.5f);
                }

                LeanTween.rotate(gridItemView.gameObject, Vector3.forward * Random.Range(-30, 30), collectTime * 0.7f).setEaseOutSine();
                LeanTween.move(gridItemView.gameObject, gridItemView.transform.position + randomOffset, collectTime * 0.7f).setEaseOutSine().setOnComplete(() =>
                {
                    LeanTween.scale(gridItemView.gameObject, Vector3.one * 0.6f, collectTime * 0.3f).setEaseInSine();
                    LeanTween.moveX(gridItemView.gameObject, target.position.x, collectTime * 0.3f).setEaseOutSine();
                    LeanTween.moveY(gridItemView.gameObject, target.position.y, collectTime * 0.3f).setEaseInSine().setOnComplete(() =>
                    {
                        //MAJD: Here was the add points after the animation, moved it out for now

                        Destroy(gridItemView.gameObject);
                    });
                });
            });
        }

        public void ExecuteLastInLink(GridTileView gridTileView, Transform target, float delay)
        {
            if (isExecuting == false) return;

            // If we have multiple powerups in the link, merge and trigger them
            if (powerupsInLink.Count > 1)
            {
                for (int powerupIndex = 0; powerupIndex < powerupsInLink.Count; powerupIndex++)
                {
                    powerupsInLink[powerupIndex].IsMerging = false;
                    powerupsInLink[powerupIndex].IsClearing = false;

                    LeanTween.moveX(powerupsInLink[powerupIndex].gameObject, gridTileView.transform.position.x, delay).setEaseInOutSine();
                    LeanTween.moveY(powerupsInLink[powerupIndex].gameObject, gridTileView.transform.position.y, delay).setEaseInOutQuad();

                    Collect(powerupsInLink[powerupIndex], target, delay * 0.8f, gridTileView);
                }

                _mergeCombosController.MergeEffect(gridTileView.transform.position);

                _mergeComboModel = _mergeCombosController.GetMergeCombo(powerupsInLink);

                powerupsInLink.Clear();

                if (_mergeComboModel != null)
                {
                    GameObject executeObject = Instantiate(_mergeComboModel.ExecuteObject);
                    executeObject.SendMessage("Execute", new ExecuteDataModel(gridTileView, delay), SendMessageOptions.DontRequireReceiver);

                    _mergeComboModel = null;
                }
            }
        }

        public void AddToExecuteList(GameObject addObject)
        {
            executeList.Add(addObject);

            CancelInvoke(nameof(EndExecuteLink));
        }

        public void RemoveFromExecuteList(GameObject removeObject)
        {
            executeList.Remove(removeObject);

            CheckExecuteLink();
        }

        void RPC_RemoveFromExecuteList()
        {
            photonView.RPC("RemoveFromExecuteList", RpcTarget.All);
        }

        [PunRPC]
        void RemoveFromExecuteList()
        {
            executeList.Remove(this.gameObject);

            CheckExecuteLink();
        }

        public void CheckExecuteLink()
        {
            if (executeList.Count > 0) return;

            Invoke(nameof(EndExecuteLink), 0.3f);
        }

        public void EndExecuteLink()
        {
            isExecuting = false;

            if (_gameManager.CurrentPlayer.moves == 0)
            {
                _gameManager.EndTurn();
            }

            tileLink.Clear();

            _gridController.CollapseTiles();

            _gameManager.RPC_StartTimer();
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
                    if (currentItemView.GridItemType == linkType || linkType < 0 || currentItemView.GridItemType < 0)
                    {
                        currentItemView.SetAnimatorBool("Selectable", true);
                    }
                    else
                    {
                        currentItemView.SetAnimatorBool("Selectable", false);
                    }

                    // Set the color of the line based on the tile
                    if (linkType < 0)
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

        public void ResetSelectables()
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

        public void CheckDirection()
        {
            if (tileLink.Count < 2) direction = Vector2.zero;
            else direction = (tileLink[tileLink.Count - 1].transform.position - tileLink[tileLink.Count - 2].transform.position).normalized;
        }

        public void SetExecuteTime(float setExecuteTime, float setMultiplier, float setMinimum)
        {
            executeTime = setExecuteTime;
            executeTimeMultipier = setMultiplier;
            executeTimeMinimum = setMinimum;
        }

    }
}
