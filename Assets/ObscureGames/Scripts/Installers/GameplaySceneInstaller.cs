using ObscureGames.Timers;
using Zenject;
namespace ObscureGames.Installers
{
    public class GameplaySceneInstaller : MonoInstaller
    {

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<TimeController>().AsSingle().NonLazy();
        }

    }
}
