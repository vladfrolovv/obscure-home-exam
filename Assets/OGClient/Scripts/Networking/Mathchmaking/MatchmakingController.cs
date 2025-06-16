using Fusion;
using UnityEngine;
using System.Threading.Tasks;
namespace OGClient.Networking.Mathchmaking
{
    public class MatchmakingController : SimulationBehaviour
    {

        public async Task<bool> StartMatchmakingProcess()
        {
            StartGameArgs startGameArgs = new()
            {
                GameMode = GameMode.Client,
                SessionName = $"OGRoom",
                PlayerCount = ConstantsModel.PLAYERS_PER_MATCH,
                SceneManager = FindObjectOfType<NetworkRunner>().SceneManager,
            };

            StartGameResult result = await FindObjectOfType<NetworkRunner>().StartGame(startGameArgs);
            if (!result.Ok)
            {
                Debug.LogError($"Failed to start game: {result.ShutdownReason}");
            }
            else
            {
                Debug.Log("Game started successfully!");
            }

            return result.Ok;
        }

    }
}
