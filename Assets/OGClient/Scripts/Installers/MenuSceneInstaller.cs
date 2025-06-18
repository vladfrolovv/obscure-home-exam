using OGClient.Gameplay.Mathchmaking;
using UnityEngine;
using Zenject;
namespace OGClient.Installers
{
    public class MenuSceneInstaller : MonoInstaller
    {

        [SerializeField] private MatchmakingController _matchmakingController;

        public override void InstallBindings()
        {
            Container.BindInstance(_matchmakingController).AsSingle();
        }

    }
}
