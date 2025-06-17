using Fusion;
using Zenject;
using OGClient;
using UnityEngine;
using OGClient.Scenes;
using OGServer.Gameplay;
using OGServer.Matchmaking;
namespace OGShared.Scripts
{
    public class ClientStartupController : MonoBehaviour
    {

        [SerializeField] private NetworkRunner _networkRunnerPrefab;

        [Header("Systems")]
        [SerializeField] private MatchStartController _matchStartControllerPrefab;
        [SerializeField] private NetworkGameManager _networkGameManager;

        private NetworkRunner _networkRunner;

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
            _networkRunner.Spawn(_networkGameManager, Vector3.zero, Quaternion.identity);
        }

        private bool IsDedicatedServer()
        {
            return Application.isBatchMode;
        }

    }
}
