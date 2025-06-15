using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace OGClient.Networking
{
    public class NetworkConnectionController : MonoBehaviour, INetworkRunnerCallbacks
    {

        [Header("UI")]
        [SerializeField] private Button             _findMatchButton;
        [SerializeField] private TextMeshProUGUI    _statusText;
        [SerializeField] private float              _retryDelay = 5f;

        private NetworkRunner _runner;
        private bool _isConnecting;
        private float _retryTimer;

        private void Awake()
        {
            _findMatchButton.onClick.AddListener(OnFindMatchClicked);
            _findMatchButton.interactable = false;
            _statusText.text = "Not connected";
        }

        private void Start()
        {
            _runner = gameObject.AddComponent<NetworkRunner>();
            _runner.ProvideInput = true;

            _runner.AddCallbacks(this);

            BeginConnect();
        }

        private void Update()
        {
            if (!_isConnecting) return;
            _retryTimer += Time.deltaTime;
            if (_retryTimer >= _retryDelay)
            {
                _retryTimer = 0;
                _statusText.text = "Retrying...";

                BeginConnect();
            }
        }

        private void BeginConnect()
        {
            _isConnecting = true;

            StartGameArgs args = new()
            {
                GameMode      = GameMode.AutoHostOrClient,
                SessionName   = "MySession",
                SceneManager  = gameObject.AddComponent<NetworkSceneManagerDefault>(),
                // Scene         = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex,
                PlayerCount = 2
            };

            _ = _runner.StartGame(args);
            _statusText.text = "Connecting...";
            _findMatchButton.interactable = false;
        }

        private void OnFindMatchClicked()
        {
            BeginConnect();
        }

        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        {
            throw new NotImplementedException();
        }

        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        {
            throw new NotImplementedException();
        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            if (runner.LocalPlayer == player)
            {
                // Ми успішно підключилися і стали гравцем
                _isConnecting = false;
                _statusText.text = runner.IsServer
                    ? "Hosting match"
                    : "Joined match";
                _findMatchButton.gameObject.SetActive(false);
            }
            else
            {
                _statusText.text = "Player joined: " + player;
                // if (runner.IsServer && runner.ActivePlayers.Count == 2)
                {
                    // runner.SetActiveScene("GameScene");
                }
            }
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            _statusText.text = $"Player left: {player}";
        }

        public void OnConnectedToServer(NetworkRunner runner)       { }
        public void OnSessionListUpdated(NetworkRunner runner, System.Collections.Generic.List<SessionInfo> sessionList) { }
        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
        {
            throw new NotImplementedException();
        }

        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
        {
            throw new NotImplementedException();
        }

        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
        {
            throw new NotImplementedException();
        }

        public void OnInput(NetworkRunner runner, NetworkInput input) { }
        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            _statusText.text = "Shutdown: " + shutdownReason;
            _isConnecting = false;
            _findMatchButton.interactable = true;
        }

        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
        {
            throw new NotImplementedException();
        }

        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
        {
            throw new NotImplementedException();
        }

        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
        {
            throw new NotImplementedException();
        }

        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
        {
            throw new NotImplementedException();
        }

        public void OnSceneLoadDone(NetworkRunner runner) { }
        public void OnSceneLoadStart(NetworkRunner runner) { }

    }

}