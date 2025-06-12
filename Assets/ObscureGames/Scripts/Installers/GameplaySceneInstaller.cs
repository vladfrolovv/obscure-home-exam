using ObscureGames.Gameplay;
using ObscureGames.Gameplay.Grid;
using ObscureGames.Players;
using ObscureGames.Timers;
using UnityEngine;
using Zenject;
namespace ObscureGames.Installers
{
    public class GameplaySceneInstaller : MonoInstaller
    {

        [SerializeField] private GameManager _gameManager;
        [SerializeField] private GridController _gridController;
        [SerializeField] private PlayerController _playerController;

        public override void InstallBindings()
        {
            Container.BindInstance(_gameManager).AsSingle();
            Container.BindInstance(_gridController).AsSingle();
            Container.BindInstance(_playerController).AsSingle();

            Container.BindInterfacesAndSelfTo<TimeController>().AsSingle().NonLazy();
        }

    }
}
