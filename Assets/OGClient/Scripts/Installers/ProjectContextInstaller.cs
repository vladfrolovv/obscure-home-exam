using OGServer.Authentication;
using Zenject;
namespace OGClient.Installers
{
    public class ProjectContextInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<PlayFabAuthDataProxy>().AsCached().NonLazy();
            Container.BindInterfacesAndSelfTo<PhotonAuthDataProxy>().AsCached().NonLazy();

        }
    }
}
