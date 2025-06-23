using OGClient.Gameplay.Authentication;
using OGClient.Gameplay.DataProxies;
using OGShared;
using OGShared.DataProxies;
using OGShared.Gameplay.Grid;
using Zenject;
namespace OGClient.Installers
{
    public class ProjectContextInstaller : MonoInstaller
    {

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<PlayFabAuthDataProxy>().AsCached().NonLazy();
            Container.BindInterfacesAndSelfTo<PhotonAuthDataProxy>().AsCached().NonLazy();

            Container.BindInterfacesAndSelfTo<ClientsAvailabilityController>().AsSingle().NonLazy();

            Container.BindInterfacesAndSelfTo<MovesDataProxy>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<RoundsDataProxy>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<GridLinkingDataProxy>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<GameSessionDataProxy>().AsCached().NonLazy();
        }

    }
}
