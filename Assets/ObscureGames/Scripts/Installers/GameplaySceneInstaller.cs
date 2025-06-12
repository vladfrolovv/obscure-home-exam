using ObscureGames.Gameplay;
using ObscureGames.Gameplay.Grid;
using ObscureGames.Gameplay.Grid.MergeCombos;
using ObscureGames.Players;
using ObscureGames.Timers;
using UnityEngine;
using Zenject;
namespace ObscureGames.Installers
{
    public class GameplaySceneInstaller : MonoInstaller
    {

        [Header("Controllers")]
        [SerializeField] private GameManager _gameManager;
        [SerializeField] private GridController _gridController;
        [SerializeField] private PlayerController _playerController;

        [Header("Vies")]
        [SerializeField] private MergeEffectView _mergeEffectView;

        public override void InstallBindings()
        {
            // controllers
            Container.BindInstance(_gameManager).AsSingle();
            Container.BindInstance(_gridController).AsSingle();
            Container.BindInstance(_playerController).AsSingle();

            // views
            Container.BindInstance(_mergeEffectView).AsSingle();

            Container.BindInterfacesAndSelfTo<TimeController>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<MergeCombosController>().AsSingle().NonLazy();
        }

    }
}
