using OGClient.Gameplay.Authentication;
using Zenject;
namespace OGClient.Installers
{
    public class ProjectContextInstaller : MonoInstaller
    {

        public static ProjectContextInstaller Instance { get; private set; }

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<PlayFabAuthDataProxy>().AsCached().NonLazy();
            Container.BindInterfacesAndSelfTo<PhotonAuthDataProxy>().AsCached().NonLazy();
        }

        public void Inject<T>(T instance)
        {
            Container.Inject(instance);
        }

        private void Awake()
        {
            Instance = this;
        }

    }
}
