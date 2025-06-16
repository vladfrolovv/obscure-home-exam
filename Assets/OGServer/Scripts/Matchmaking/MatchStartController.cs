using System.Linq;
using Fusion;
using OGClient;
using UnityEngine;
using Zenject;
namespace OGServer.Matchmaking
{
    public class MatchStartController : NetworkBehaviour, IPlayerJoined
    {

        [SerializeField] private SceneRef _gameplayScene;

        [Inject] private NetworkRunner _networkRunner;

        public override void Spawned()
        {
            TryToLoadGame();
        }

        public void PlayerJoined(PlayerRef player)
        {
            TryToLoadGame();
        }

        private async void TryToLoadGame()
        {
            Debug.Log($"Active players count: {Runner.ActivePlayers.Count()} | Required players: {ConstantsModel.PLAYERS_PER_MATCH}");
            if (!Runner.IsServer) return;
            if (Runner.ActivePlayers.Count() == ConstantsModel.PLAYERS_PER_MATCH)
            {
                Debug.Log($"Trying to load gameplay scene for clients");
                await Runner.SceneManager.LoadScene(_gameplayScene, new NetworkLoadSceneParameters());
            }
        }

    }
}
