using UniRx;
using System;
using Fusion;
using UnityEngine;
using Fusion.Sockets;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
namespace OGServer.Gameplay
{
    public class NetworkGameManager : NetworkBehaviour, INetworkRunnerCallbacks
    {

        private const float MatchStartDelay = 1f;

        [Networked] public PlayerRef CurrentPlayer { get; private set; } = PlayerRef.None;

        [Networked] public int Rounds { get; set; }
        [Networked] public int CurrentRound { get; set; }

        private MatchPhase _matchPhase;

        private MatchPhase MatchPhase
        {
            get => _matchPhase;
            set
            {
                _matchPhase = value;
                _matchPhaseChanged.OnNext(value);
            }
        }

        [SerializeField] private SceneRef _gameplayScene;
        [SerializeField] private NetworkPrefabRef _playerPrefab;

        private readonly Subject<MatchPhase> _matchPhaseChanged = new();
        public IObservable<MatchPhase> MatchPhaseChanged => _matchPhaseChanged;

        public override void Spawned()
        {
            if (!Runner.IsServer) return;
            MatchPhase = MatchPhase.Waiting;
        }

        public void OnSceneLoadDone(NetworkRunner runner)
        {
            if (!runner.IsServer) return;
            if (SceneManager.GetActiveScene().buildIndex != _gameplayScene.AsIndex) return;

            SpawnPlayersInMatch(runner);
            MatchPhase = MatchPhase.Starting;

            Observable.Timer(TimeSpan.FromSeconds(MatchStartDelay)).Subscribe(delegate
            {
                MatchPhase = MatchPhase.Playing;
            }).AddTo(this);
        }

        private void SpawnPlayersInMatch(NetworkRunner runner)
        {
            if (!runner.IsServer) return;
            foreach (PlayerRef playerRef in runner.ActivePlayers)
            {
                runner.Spawn(_playerPrefab, Vector3.zero, Quaternion.identity, playerRef);
            }
        }


        #region INetworkRunnerCallbacks Implementation
        public void OnSceneLoadStart(NetworkRunner runner) { }
        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
        public void OnInput(NetworkRunner runner, NetworkInput input) { }
        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
        public void OnConnectedToServer(NetworkRunner runner) { }
        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        #endregion
    }
}