using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using OGClient.Scenes;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace OGServer.Gameplay
{
    public class NetworkGameManager : SimulationBehaviour, INetworkRunnerCallbacks
    {

        [SerializeField] private NetworkPrefabRef _playerPrefab;

        public void SpawnPlayersInMatch()
        {
            if (!Runner.IsServer) return;
            foreach (PlayerRef runnerActivePlayer in Runner.ActivePlayers)
            {
                Runner.Spawn(_playerPrefab, Vector3.zero, Quaternion.identity, runnerActivePlayer);
            }
        }

        public void PlayerJoined(PlayerRef player)
        {
            if (!Runner.IsServer) return;
            Runner.Spawn(_playerPrefab, Vector3.zero, Quaternion.identity, player);
        }

        public void PlayerLeft(PlayerRef player)
        {
            if (!Runner.IsServer) return;
            NetworkObject playerObject = Runner.GetPlayerObject(player);
            if (playerObject == null) return;
            Runner.Despawn(playerObject);
        }

        public void OnSceneLoadDone(NetworkRunner runner)
        {
            if (!runner.IsServer) return;
            if (SceneManager.GetActiveScene().buildIndex != (int)SceneType.Gameplay) return;

            SpawnPlayersInMatch();
        }

        public void OnSceneLoadStart(NetworkRunner runner)
        {
        }

        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        {
        }

        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        {
        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
        }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
        }

        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
        {
        }

        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
        {
        }

        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
        {
        }

        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
        {
        }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
        {
        }

        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
        {
        }

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
        }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
        {
        }

        public void OnConnectedToServer(NetworkRunner runner)
        {
        }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {
        }

        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
        {
        }

        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
        {
        }

    }
}