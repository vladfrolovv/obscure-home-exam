using ObscureGames.Timers;
using UnityEngine;
using Zenject;
namespace ObscureGames.Installers
{
    public class GameplaySceneInstaller : MonoInstaller
    {

        [Header("Controllers")]
        [SerializeField] private GameManager _gameManager;
        [SerializeField] private GridManager _gridManager;
        [SerializeField] private PlayerController _playerController;

        public override void InstallBindings()
        {
            Container.BindInstance(_gameManager).AsSingle();
            Container.BindInstance(_gridManager).AsSingle();
            Container.BindInstance(_playerController).AsSingle();

            Container.BindInterfacesAndSelfTo<TimeController>().AsSingle().NonLazy();
        }

    }
}
