using Fusion;
using System.Threading.Tasks;
using OGServer.Matchmaking;
using Zenject;
namespace OGClient.Networking.Mathchmaking
{
    public class MatchmakingController : SimulationBehaviour
    {

        private NetworkRunner _networkRunner;
        private MatchStartController _matchStartController;

        [Inject]
        public void Construct(NetworkRunner networkRunner, MatchStartController matchStartController)
        {
            _networkRunner = networkRunner;
            _matchStartController = matchStartController;
        }

        public async Task<bool> StartMatchmakingProcess()
        {
            StartGameArgs startGameArgs = new()
            {
                GameMode = GameMode.Shared,
                SessionName = string.Empty,
                PlayerCount = 2,
            };

            StartGameResult result = await _networkRunner.StartGame(startGameArgs);

            // _networkRunner.AddCallbacks(_matchStartController);

            return result.Ok;
        }

    }
}
