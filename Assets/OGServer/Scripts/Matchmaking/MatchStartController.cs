using System.Linq;
using Fusion;
using OGClient;
using UnityEngine;
namespace OGServer.Matchmaking
{
    public class MatchStartController : SimulationBehaviour, IPlayerJoined
    {

        [SerializeField] private SceneRef _gameplayScene;

        public void PlayerJoined(PlayerRef player)
        {
            Debug.Log($"Runner is server: {Runner.IsServer} | Active players count: {Runner.ActivePlayers.Count()} | Required players: {ConstantsModel.PLAYERS_PER_MATCH}");
            if (!Runner.IsServer) return;
            if (Runner.ActivePlayers.Count() < ConstantsModel.PLAYERS_PER_MATCH) return;

            Runner.SceneManager.LoadScene(_gameplayScene, new NetworkLoadSceneParameters());
        }

    }
}
