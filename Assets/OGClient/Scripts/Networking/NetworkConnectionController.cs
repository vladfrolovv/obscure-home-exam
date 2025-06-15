using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;
using Random = UnityEngine.Random;
namespace OGClient.Networking
{
    public class NetworkConnectionController : MonoBehaviourPunCallbacks
    {

        [SerializeField] private Button _findMatchButton;
        [SerializeField] private TextMeshProUGUI _statusText;
        [SerializeField] private float _retryDelay = 5f;

        private bool _isConnected;
        private float _retryTimer;

        private void Awake()
        {
            PhotonNetwork.AutomaticallySyncScene = true;
        }

        private void Start()
        {
            if (PhotonNetwork.IsConnectedAndReady)
            {
                _isConnected = true;
                _findMatchButton.interactable = true;
            }
            else
            {
                _retryTimer = 0;
                _findMatchButton.interactable = false;
                Connect();
            }
        }

        private void Update()
        {
            if (_isConnected) return;
            if (_retryTimer < _retryDelay)
            {
                _retryTimer += Time.deltaTime;
            }
            else
            {
                PhotonNetwork.Disconnect();
                Connect();
                _retryTimer = 0;
            }
        }

        public void Connect()
        {
            try
            {
                PhotonNetwork.ConnectUsingSettings();
            }
            catch (UnityException e)
            {
                Connect();
            }
        }

        public void FindMatch()
        {
            _findMatchButton.gameObject.SetActive(false);
            PhotonNetwork.JoinRandomRoom();
        }

        public override void OnConnectedToMaster()
        {
            _isConnected = true;
            _findMatchButton.interactable = true;
        }


        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 2 });
        }

        public override void OnJoinedRoom()
        {
            _statusText.SetText("Waiting For other player");
            PhotonNetwork.LocalPlayer.NickName = $"Guest{Random.Range(1000, 9999)}";
        }

        public override void OnJoinedLobby()
        {

            _statusText.SetText("Joined lobby");
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            newPlayer.NickName = $"Guest{Random.Range(1000, 9999)}";
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.LoadLevel(1);
            }
        }
    }

}