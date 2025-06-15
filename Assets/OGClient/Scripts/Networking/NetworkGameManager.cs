using UnityEngine;
using Zenject;
namespace OGClient.Networking
{
    public class NetworkGameManager : MonoBehaviour
    {

        [SerializeField] private NetworkPlayerController _masterPlayerControllerPrefab;
        [SerializeField] private NetworkPlayerController _remotePlayerControllerPrefab;

        private NetworkPlayerController _masterPlayerController;
        private NetworkPlayerController _remotePlayerController;

        private DiContainer _diContainer;

        [Inject]
        public void Construct(DiContainer diContainer)
        {
            _diContainer = diContainer;
        }

        private void Start()
        {
            SpawnPlayers();
        }

        private void SpawnPlayers()
        {
            // if (PhotonNetwork.IsMasterClient)
            {
                _masterPlayerController = CreateNetworkPlayerFrom(_masterPlayerControllerPrefab);
            }
            // else
            {
                _remotePlayerController = CreateNetworkPlayerFrom(_remotePlayerControllerPrefab);
            }
        }

        private NetworkPlayerController CreateNetworkPlayerFrom(NetworkPlayerController prefab)
        {
            // NetworkPlayerController networkPlayerController = PhotonNetwork.Instantiate(prefab.name, new Vector3(0, 0, 0), Quaternion.identity).GetComponent<NetworkPlayerController>();
            // _diContainer.Inject(networkPlayerController);

            return prefab;
        }

    }
}