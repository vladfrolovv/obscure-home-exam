using OGClient.Gameplay;
using OGClient.Gameplay.DataProxies;
using OGClient.Gameplay.Players;
using OGClient.Gameplay.UI;
using UnityEngine.UI;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Zenject;
namespace OGClient.Networking
{
    public class NetworkPlayerController : MonoBehaviourPunCallbacks, IPunObservable
    {

        private const string BounceUpAnimatorProperty = "Bounce";
        private const string BounceDownAnimatorProperty = "Bounce2";

        [SerializeField] private ScriptablePlayerProfile _playerProfile;
        [SerializeField] private string _playerName = "Player 1";
        [SerializeField] private Color _playerColor = Color.blue;
        [SerializeField] private int _playerIndex;

        public int bonus = 0;
        public float bonusDelay = 0;

        public int moves = 0;

        public TextMeshProUGUI nameText;
        public Image avatarImage;

        public TextMeshProUGUI scoreText;

        public TextMeshProUGUI bonusText;
        public Animator bonusAnimator;

        public TextMeshProUGUI movesText;
        public Animator movesAnimator;
        public ProgressBarView MovesBarView;

        [SerializeField] private Canvas _playerCanvas;

        private bool _canControl;

        public Canvas PlayerCanvas => _playerCanvas;
        public Color PlayerColor => _playerColor;
        public string PlayerName => _playerName;

        private ScoreDataProxy _scoreDataProxy;
        private GameManager _gameManager;

        [Inject]
        public void Construct(GameManager gameManager, ScoreDataProxy scoreDataProxy)
        {
            _gameManager = gameManager;
            _scoreDataProxy = scoreDataProxy;
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            //sync score
            if (stream.IsWriting)
            {
                stream.SendNext(_scoreDataProxy.GetPlayerScore(_playerIndex));
                stream.SendNext(_playerName);
                stream.SendNext(bonus);
                stream.SendNext(bonusDelay);
                stream.SendNext(moves);
                stream.SendNext(_canControl);
                stream.SendNext(_playerIndex);
            }
            else
            {
                _scoreDataProxy.IncreasePlayerScore(_playerIndex, (int)stream.ReceiveNext());
                _playerName = (string)stream.ReceiveNext();
                nameText.SetText(_playerName);
                bonus = (int)stream.ReceiveNext();
                bonusDelay = (float)stream.ReceiveNext();
                moves = (int)stream.ReceiveNext();
                movesText.SetText(moves.ToString());
                _canControl = (bool)stream.ReceiveNext();
                _playerIndex = (int)stream.ReceiveNext();
            }
        }

        private void Start()
        {
            _gameManager.AddNewPlayer(photonView.OwnerActorNr, this);
            _playerCanvas.worldCamera = _gameManager.MainCamera;

            _playerName = PhotonNetwork.LocalPlayer.NickName;
            if (photonView.IsMine)
            {
                photonView.RPC(nameof(RPC_Setup), RpcTarget.All);
            }
        }

        void Update()
        {
            if (photonView.IsMine)
            {
                if (bonusDelay > 0) bonusDelay -= Time.deltaTime;
                else
                {
                    if (bonus > 0)
                    {
                        photonView.RPC(nameof(RemoveBonus), RpcTarget.All, 1);
                        photonView.RPC(nameof(RPC_ChangeScore), RpcTarget.All, 1);
                    }
                }
            }
        }

        [PunRPC]
        public void RPC_Setup()
        {
            nameText.SetText(_playerName);
            if (_playerProfile)
            {
                avatarImage.sprite = _playerProfile.AvatarIcon;
            }

            _playerIndex = photonView.OwnerActorNr;

            photonView.RPC(nameof(SetScore), RpcTarget.All, _scoreDataProxy.GetPlayerScore(_playerIndex));

            photonView.RPC(nameof(SetMoves), RpcTarget.All, moves);
            MovesBarView.SetProgressMax(moves);
            MovesBarView.Setup(this);

            photonView.RPC(nameof(SetBonus), RpcTarget.All, 0);
        }

        [PunRPC]
        void RPC_AddBonus(int addBonus, float setDelay)
        {
            bonus += addBonus;
            photonView.RPC(nameof(UpdateBonus), RpcTarget.All);

            bonusAnimator.Play(BounceUpAnimatorProperty);
            bonusAnimator.Play(BounceDownAnimatorProperty);

            bonusDelay = setDelay;
        }

        public void AddBonus(int addBonus, float setDelay)
        {
            photonView.RPC(nameof(RPC_AddBonus), RpcTarget.All, addBonus, setDelay);
        }

        [PunRPC]
        public void RemoveBonus(int removeBonus)
        {
            bonus -= removeBonus;
            photonView.RPC(nameof(UpdateBonus), RpcTarget.All);
        }

        [PunRPC]
        void RPC_ChangeScore(int value)
        {
            _scoreDataProxy.IncreasePlayerScore(_playerIndex, value);
            photonView.RPC(nameof(UpdateScore), RpcTarget.All);
        }

        public void ChangeScore(int changeValue)
        {
            _scoreDataProxy.IncreasePlayerScore(_playerIndex, changeValue);

            photonView.RPC(nameof(UpdateScore), RpcTarget.All);
        }

        [PunRPC]
        public void SetScore(int setValue)
        {
            _scoreDataProxy.SetPlayerScore(_playerIndex, setValue);
            photonView.RPC(nameof(UpdateScore), RpcTarget.All);
        }

        [PunRPC]
        public void UpdateScore()
        {
            scoreText.SetText(_scoreDataProxy.GetPlayerScore(_playerIndex).ToString("000"));
        }

        [PunRPC]
        public void SetBonus(int setValue)
        {
            bonus = setValue;

            photonView.RPC(nameof(UpdateBonus), RpcTarget.All);
        }

        [PunRPC]
        public void UpdateBonus()
        {
            bonusText.gameObject.SetActive(bonus > 0);

            bonusText.SetText("+" + bonus);
        }

        [PunRPC]
        public void SetMoves(int setValue)
        {
            moves = setValue;
            photonView.RPC(nameof(UpdateMoves), RpcTarget.All);
        }

        [PunRPC]
        public void ChangeMoves(int changeValue)
        {
            moves += changeValue;

            if (MovesBarView) MovesBarView.ChangeProgress(changeValue);

            photonView.RPC(nameof(UpdateMoves), RpcTarget.All);
        }

        [PunRPC]
        public void UpdateMoves()
        {
            movesText.SetText(moves.ToString());

            movesAnimator.Play(BounceUpAnimatorProperty);
            movesAnimator.Play(BounceDownAnimatorProperty);
        }

    }
}