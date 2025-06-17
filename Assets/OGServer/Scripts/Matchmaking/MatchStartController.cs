using Fusion;
using OGClient;
using UnityEngine;
using System.Linq;
namespace OGServer.Matchmaking
{
    public class MatchStartController : NetworkBehaviour, IPlayerJoined
    {

        public bool MatchIsRunning { get; private set; } = false;

        [SerializeField] private SceneRef _gameplayScene;

        public override void Spawned()
        {
            TryToLoadGame();
        }

        public void PlayerJoined(PlayerRef player)
        {
            TryToLoadGame();
        }

        private void TryToLoadGame()
        {
            Debug.Log($"Active players count: {Runner.ActivePlayers.Count()} | Required players: {ConstantsModel.PLAYERS_PER_MATCH}");
            if (!Runner.IsServer) return;
            if (Runner.ActivePlayers.Count() != ConstantsModel.PLAYERS_PER_MATCH) return;

            Debug.Log($"Trying to load gameplay scene for clients");

            MatchIsRunning = true;
            Runner.LoadScene(_gameplayScene);
        }

    }
}
