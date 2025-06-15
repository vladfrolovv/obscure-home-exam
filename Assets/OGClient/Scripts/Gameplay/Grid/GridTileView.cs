using System;
using Photon.Pun;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;
using OGClient.Utils;
namespace OGClient.Gameplay.Grid
{
    public class GridTileView : MonoBehaviour
    {

        [Header("Controls")]
        [SerializeField] private EventTrigger _eventTrigger;

        [Header("View")]
        [SerializeField] private Image _tileImage;
        [SerializeField] private Color _tileColor;
        [SerializeField] private Transform _connector;
        [SerializeField] private GameObject _connectorLine;
        [SerializeField] private Image _connectorLineColor;
        [SerializeField] private Image _buttonImage;
        [SerializeField] private Image _glowImage;

        [Header("Animations")]
        [SerializeField] private Animator _connectorAnimator;
        [SerializeField] private float _appearTime = 0.5f;
        [SerializeField] private float _appearDelayMultiplier = 0.02f;

        [Header("Glowing Effect")]
        [SerializeField] private float _glowUpTime = 0.3f;
        [SerializeField] private float _glowDownTime = 0.2f;
        [SerializeField] private float _glowAlpha = 0.6f;

        public GridItemView GridItemView { get; set; }

        private GridTileView _rightItemView;
        private GridTileView _leftItemView;
        private GridTileView _topItemView;
        private GridTileView _bottomItemView;

        private GameManager _gameManager;
        private GridController _gridController;
        private GridPlayerController _gridPlayerController;

        public void SetControlsActive(bool active) => _eventTrigger.enabled = active;
        public void SetConnectorLineActive(bool active) => _connectorLine.SetActive(active);
        public void SetConnectorLineColor(Color color) => _connectorLineColor.color = color;
        public void SetConnectorAnimator(bool setValue) => _connectorAnimator.enabled = setValue;
        public void SetClickSize(float setValue) => _buttonImage.transform.localScale = Vector3.one * setValue;

        [Inject]
        public void Construct(GridController gridController, GameManager gameManager, GridPlayerController gridPlayerController)
        {
            _gameManager = gameManager;
            _gridController = gridController;
            _gridPlayerController = gridPlayerController;
        }

        public void InstallTileConnections(GridTileView rightItemView, GridTileView leftItemView, GridTileView topItemView, GridTileView bottomItemView)
        {
            _rightItemView = rightItemView;
            _leftItemView = leftItemView;
            _topItemView = topItemView;
            _bottomItemView = bottomItemView;
        }

        public void InstallTileView(Color color)
        {
            _tileImage.color = _tileColor = color;
            // transform.localScale = Vector3.zero;
            //
            // LeanTween
            //     .scale(gameObject, Vector3.one, _appearTime)
            //     .setDelay(_gridController.GridSize.x * _gridController.GridSize.y * _appearDelayMultiplier)
            //     .setEaseOutBounce();
        }

        public void Select()
        {
            if (GridItemView == null) return;
            if (!_gameManager.CurrentPlayerController.photonView.IsMine) return;
            if (_gameManager.CurrentPlayerController.moves <= 0) return;
            if (_gameManager.PlayerIndex != PhotonNetwork.LocalPlayer.ActorNumber) return;

            // Rules for linking:
            // 1. Must be adjacent to the last tile in the link sequence (up/down/left/right)
            // 2. Must be first in link sequence or same type
            // 3. Must not already exist in the link
            // 4. If we go back to a previous tile in a link, remove it

            // Check the last tile in the link sequence.
            // We will use this to check if we are connecting to same type

            GridTileView lastTileViewInLink = null;
            if (_gridPlayerController.tileLink.Count > 0) lastTileViewInLink = _gridPlayerController.tileLink[_gridPlayerController.tileLink.Count - 1];

            // If this tile was already added to the link, backtrack to it
            if (_gridPlayerController.tileLink.Contains(this))
            {
                _gridPlayerController.linkType = GridItemView.GridItemType;
                _gridPlayerController.CheckSelectables();

                // Remove all items in the link after this one
                Vector2Int gridTilePos = _gridPlayerController.GetIndexInGrid(this);
                _gridPlayerController.photonView.RPC("LinkRemoveAfterByGrid", RpcTarget.All, gridTilePos.x, gridTilePos.y);

                return;
            }

            // If we're not tapping and holding the tap on the screen, return
            if (!Input.GetMouseButton(0)) return;

            bool goodLink = false;

            // If this is the first tile in the link, add it regardless of type match
            if (_gridPlayerController.tileLink.Count == 0)
            {
                _gridPlayerController.linkType = GridItemView.GridItemType;
                _gridPlayerController.CheckSelectables();

                _connectorLine.SetActive(false);

                Vector2Int gridTilePos = _gridPlayerController.GetIndexInGrid(this);
                _gridPlayerController.photonView.RPC("LinkStartByGrid", RpcTarget.All, gridTilePos.x, gridTilePos.y);

                return;
            }

            // Check if the item type matches the last tile in the link
            if (lastTileViewInLink && (GridItemView.GridItemType == lastTileViewInLink.GridItemView.GridItemType || GridItemView.GridItemType < 0 || lastTileViewInLink.GridItemView.GridItemType < 0))
            {
                if (_topItemView && _topItemView == lastTileViewInLink) // Connect FROM the tile above this one
                {
                    goodLink = true;

                    _connector.localEulerAngles = Vector3.forward * 90;
                }
                else if (_bottomItemView && _bottomItemView == lastTileViewInLink) // Connect FROM the tile below this one
                {
                    goodLink = true;

                    _connector.localEulerAngles = Vector3.forward * 270;
                }
                else if (_leftItemView && _leftItemView == lastTileViewInLink) // Connect FROM the tile left of this one
                {
                    goodLink = true;

                    _connector.localEulerAngles = Vector3.forward * 180;
                }
                else if (_rightItemView && _rightItemView == lastTileViewInLink) // Connect FROM the tile right of this one
                {
                    goodLink = true;

                    _connector.localEulerAngles = Vector3.forward * 0;
                }
                else if (_gridController.IsDiagonalsAllowed) // Check diagonal connections
                {
                    if (_topItemView && _topItemView._rightItemView && _topItemView._rightItemView == lastTileViewInLink) // Connect FROM top right tile
                    {
                        goodLink = true;

                        _connector.localEulerAngles = Vector3.forward * 45;
                    }
                    else if (_topItemView && _topItemView._leftItemView && _topItemView._leftItemView == lastTileViewInLink)
                    {
                        goodLink = true;

                        _connector.localEulerAngles = Vector3.forward * 135;
                    }
                    else if (_bottomItemView && _bottomItemView._rightItemView && _bottomItemView._rightItemView == lastTileViewInLink) // Connect FROM bottom right tile
                    {
                        goodLink = true;

                        _connector.localEulerAngles = Vector3.forward * 315;
                    }
                    else if (_bottomItemView && _bottomItemView._leftItemView && _bottomItemView._leftItemView == lastTileViewInLink)
                    {
                        goodLink = true;

                        _connector.localEulerAngles = Vector3.forward * 225;
                    }
                }
            }

            if (goodLink == true)
            {
                _gridPlayerController.linkType = GridItemView.GridItemType;

                _gridPlayerController.CheckSelectables();

                Vector2Int gridTilePos = _gridPlayerController.GetIndexInGrid(this);
                _gridPlayerController.photonView.RPC("LinkAddByGrid", RpcTarget.All, gridTilePos.x, gridTilePos.y);
            }
        }

        public void Glow(float delay)
        {
            Observable.Timer(TimeSpan.FromSeconds(delay)).Subscribe(delegate
            {
                LeanTween.color(_glowImage.rectTransform, new Color(1, 1, 1, _glowAlpha), _glowUpTime).setOnComplete(() =>
                {
                    LeanTween.color(_glowImage.rectTransform, Color.clear, _glowDownTime);
                });
            });
        }

        private void Awake()
        {
            SetConnectorLineActive(false);

            _eventTrigger.EventTriggerSubscription(EventTriggerType.PointerDown, Select);
            _eventTrigger.EventTriggerSubscription(EventTriggerType.PointerEnter, Select);
        }

        private void OnDrawGizmosSelected()
        {
            if (_topItemView)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, _topItemView.transform.position);
            }

            if (_bottomItemView)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(transform.position, _bottomItemView.transform.position);
            }

            if (_rightItemView)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, _rightItemView.transform.position);
            }

            if (_leftItemView)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(transform.position, _leftItemView.transform.position);
            }

        }

    }
}


