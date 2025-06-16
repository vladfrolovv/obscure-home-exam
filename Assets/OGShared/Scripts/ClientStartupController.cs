using Fusion;
using Zenject;
using OGClient;
using UnityEngine;
using OGClient.Scenes;
using OGServer.Matchmaking;
namespace OGShared.Scripts
{
    public class ClientStartupController : MonoBehaviour
    {

        [SerializeField] private NetworkRunner _networkRunnerPrefab;
        [SerializeField] private MatchStartController _matchStartControllerPrefab;

        private DiContainer _diContainer;
        private NetworkRunner _networkRunner;

        [Inject]
        public void Construct(DiContainer diContainer)
        {
            _diContainer = diContainer;
        }

        private void Awake()
        {
            _networkRunner = Instantiate(_networkRunnerPrefab);
            if (IsDedicatedServer())
            {
                InstallDedicatedServer();
            }
            else
            {
                InstallClient();
            }
        }

        private async void InstallDedicatedServer()
        {
            Debug.Log($"Dedicated Server mode detected. Staying on Bootstrap scene.");
            _networkRunner.ProvideInput = false;

            _diContainer.BindInstance(_networkRunner).AsSingle();
            await _networkRunner.StartGame(new StartGameArgs()
            {
                GameMode = GameMode.Server,
                SessionName = ConstantsModel.BASE_ROOM_NAME,
                SceneManager = _networkRunner.SceneManager,
            });

            InstallDedicatedServerObjects();
        }

        private void InstallClient()
        {
            Debug.Log($"Detected client mode. Loading Main Menu scene.");

            _networkRunner.ProvideInput = true;
            SceneType.MainMenu.LoadScene();

        }

        private void InstallDedicatedServerObjects()
        {
            _networkRunner.Spawn(_matchStartControllerPrefab, Vector3.zero, Quaternion.identity);
        }

        private bool IsDedicatedServer()
        {
            return Application.isBatchMode;
        }

    }
}
