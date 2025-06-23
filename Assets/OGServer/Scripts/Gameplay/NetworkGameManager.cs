using UniRx;
using System;
using Fusion;
using UnityEngine;
using Fusion.Sockets;
using System.Collections.Generic;
using OGClient;
using OGShared.DataProxies;
using OGShared.Gameplay;
using UnityEngine.SceneManagement;
using Zenject;
namespace OGServer.Gameplay
{
    public class NetworkGameManager : NetworkBehaviour, INetworkRunnerCallbacks
    {

        [Networked] private Vector2Int GridSize { get; set; }
        [Networked] private int Seed { get; set; }
        [Networked] private int Rounds { get; set; }
        [Networked] private int CurrentRound { get; set; }

        [SerializeField] private SceneRef _gameplayScene;
        [SerializeField] private NetworkPrefabRef _playerPrefab;

        private GameSessionDataProxy _gameSessionDataProxy;
        private ScriptableGameSessionSettings _gameSessionSettings;

        [Inject]
        public void Construct(GameSessionDataProxy gameSessionDataProxy, ScriptableGameSessionSettings scriptableGameSessionSettings)
        {
            _gameSessionDataProxy = gameSessionDataProxy;
            _gameSessionSettings = scriptableGameSessionSettings;
        }

        public override void Spawned()
        {
            if (!Runner.IsServer) return;
            _gameSessionDataProxy.SetMatchPhase(MatchPhase.Waiting);
        }

        public void OnSceneLoadDone(NetworkRunner runner)
        {
            if (!runner.IsServer) return;
            if (SceneManager.GetActiveScene().buildIndex == _gameplayScene.AsIndex)
            {
                OnMatchStarted(runner);
            }
        }

        private void OnMatchStarted(NetworkRunner runner)
        {
            _gameSessionDataProxy.SetMatchPhase(MatchPhase.Starting);

            GridSize = _gameSessionSettings.GridSize;
            Seed = UnityEngine.Random.Range(0, int.MaxValue);
            Rounds = _gameSessionSettings.Rounds;

            foreach (PlayerRef playerRef in runner.ActivePlayers)
            {
                runner.Spawn(_playerPrefab, Vector3.zero, Quaternion.identity, playerRef);
            }

            RPC_InitializeSession(GridSize, Seed, Rounds);
            Observable.Timer(TimeSpan.FromSeconds(BaseConstants.GAME_START_DELAY))
                .Subscribe(_ =>
                {
                    _gameSessionDataProxy.SetMatchPhase(MatchPhase.Playing);
                    Rpc_SetMatchPhase(MatchPhase.Playing);
                }).AddTo(this);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.Proxies)]
        private void RPC_InitializeSession(Vector2Int size, int seed, int rounds)
        {
            _gameSessionDataProxy.SetGridSize(size);
            _gameSessionDataProxy.SetSeed(seed);
            _gameSessionDataProxy.SetRounds(rounds);
            _gameSessionDataProxy.RaiseInitialized();

            Debug.Log($"[CLIENT] Session init: seed={seed}, rounds={rounds}");
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.Proxies)]
        private void Rpc_SetMatchPhase(MatchPhase phase)
        {
            _gameSessionDataProxy.SetMatchPhase(phase);
            Debug.Log($"[CLIENT] Match phase set to {phase}");
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