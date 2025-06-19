using Fusion;
using UnityEngine;
using System.Threading.Tasks;
using OGShared;
namespace OGClient.Gameplay.Mathchmaking
{
    public class MatchmakingController : SimulationBehaviour
    {

        public async Task<bool> StartMatchmakingProcess()
        {
            StartGameArgs startGameArgs = new()
            {
                GameMode = GameMode.Client,
                SessionName = ConstantsModel.BASE_ROOM_NAME,
                PlayerCount = ConstantsModel.PLAYERS_PER_MATCH,
                SceneManager = NetworkRunnerInstance.Instance.SceneManager,
            };

            StartGameResult result = await NetworkRunnerInstance.Instance.StartGame(startGameArgs);
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
