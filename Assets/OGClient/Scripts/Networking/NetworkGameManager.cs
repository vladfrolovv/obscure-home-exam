using Fusion;
using UnityEngine;
namespace OGClient.Networking
{
    public class NetworkGameManager : SimulationBehaviour, IPlayerJoined, IPlayerLeft
    {

        [SerializeField] private NetworkPrefabRef _playerPrefab;

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

    }
}