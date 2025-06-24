using Fusion;
using UnityEngine;
using OGClient.Scenes;
using OGServer.Gameplay;
using OGServer.Matchmaking;
namespace OGShared
{
    public class ClientStartupController : MonoBehaviour
    {

        [Header("Prefabs")]
        [SerializeField] private NetworkRunner _networkRunnerPrefab;
        [SerializeField] private MatchStartController _matchStartControllerPrefab;
        [SerializeField] private NetworkGameManager _networkGameManagerPrefab;

        private NetworkRunner _networkRunner;

        private void Awake()
        {
            InstallNetworkRunner();
            if (IsDedicatedServer())
            {
                InstallDedicatedServer();
            }
            else
            {
                InstallClient();
            }
        }

        private void InstallNetworkRunner()
        {
            _networkRunner = Instantiate(_networkRunnerPrefab);
            DontDestroyOnLoad(_networkRunner.gameObject);
            NetworkRunnerInstance.Instance = _networkRunner;
        }

        private async void InstallDedicatedServer()
        {
            Debug.Log($"Dedicated Server mode detected. Staying on Bootstrap scene.");
            
            _networkRunner.ProvideInput = false;
            await _networkRunner.StartGame(new StartGameArgs
            {
                GameMode = GameMode.Server,
                SessionName = BaseConstants.BASE_ROOM_NAME,
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
            NetworkGameManager networkGameManager = _networkRunner.Spawn(_networkGameManagerPrefab, Vector3.zero, Quaternion.identity);

            _networkRunner.AddCallbacks(networkGameManager);
            DontDestroyOnLoad(networkGameManager.gameObject);
        }

        private static bool IsDedicatedServer()
        {
            return Application.isBatchMode;
        }

    }
}
