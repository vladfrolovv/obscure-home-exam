using UnityEngine;
using Photon.Pun;
using Zenject;

namespace ObscureGames.Networking
{
    public class NetworkGameManager : MonoBehaviourPunCallbacks
    {

        [SerializeField] private NetworkPlayer _masterPlayerPrefab;
        [SerializeField] private NetworkPlayer _remotePlayerPrefab;

        private NetworkPlayer _masterPlayer;
        private NetworkPlayer _remotePlayer;

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
            if (PhotonNetwork.IsMasterClient)
            {
                _masterPlayer = CreateNetworkPlayerFrom(_masterPlayerPrefab);
            }
            else
            {
                _remotePlayer = CreateNetworkPlayerFrom(_remotePlayerPrefab);
            }
        }

        private NetworkPlayer CreateNetworkPlayerFrom(NetworkPlayer prefab)
        {
            NetworkPlayer networkPlayer = PhotonNetwork.Instantiate(prefab.name, new Vector3(0, 0, 0), Quaternion.identity).GetComponent<NetworkPlayer>();
            _diContainer.Inject(networkPlayer);

            return networkPlayer;
        }

    }
}