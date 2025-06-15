using System.Threading.Tasks;
using Fusion;
using UnityEngine;
namespace OGServer.Matchmaking
{
    public class MatchmakingController : MonoBehaviour
    {

        [SerializeField] private NetworkRunner _networkRunnerPrefab;

        public async Task<bool> StartMatchmakingProcess()
        {
            StartGameArgs startGameArgs = new()
            {
                GameMode = GameMode.Shared,
                SessionName = string.Empty,
                PlayerCount = 2,
            };

            NetworkRunner newRunner = Instantiate(_networkRunnerPrefab);
            StartGameResult result = await newRunner.StartGame(startGameArgs);

            return result.Ok;
        }

    }
}
