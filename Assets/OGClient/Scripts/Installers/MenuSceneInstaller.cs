using Fusion;
using OGClient.Networking.Mathchmaking;
using OGServer.Matchmaking;
using UnityEngine;
using Zenject;
namespace OGClient.Installers
{
    public class MenuSceneInstaller : MonoInstaller
    {

        [SerializeField] private NetworkRunner _networkRunner;
        [SerializeField] private MatchmakingController _matchmakingController;
        [SerializeField] private MatchStartController _matchStartController;

        public override void InstallBindings()
        {
            Container.BindInstance(_networkRunner).AsSingle();
            Container.BindInstance(_matchmakingController).AsSingle();
            Container.BindInstance(_matchStartController).AsSingle();
        }

    }
}
