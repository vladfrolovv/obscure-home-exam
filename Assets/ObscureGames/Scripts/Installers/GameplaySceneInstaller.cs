using ObscureGames.Gameplay;
using ObscureGames.Gameplay.DataProxies;
using ObscureGames.Gameplay.Grid;
using ObscureGames.Gameplay.Grid.MergeCombos;
using ObscureGames.Timers;
using UnityEngine;
using Zenject;
namespace ObscureGames.Installers
{
    public class GameplaySceneInstaller : MonoInstaller
    {

        [Header("References")]
        [SerializeField] private Camera _camera;

        [Header("Controllers")]
        [SerializeField] private GameManager _gameManager;
        [SerializeField] private GridController _gridController;
        [SerializeField] private GridPlayerController _gridPlayerController;

        [Header("Vies")]
        [SerializeField] private MergeEffectView _mergeEffectView;

        public override void InstallBindings()
        {
            Container.BindInstance(_camera).AsSingle();

            // controllers
            Container.BindInstance(_gameManager).AsSingle();
            Container.BindInstance(_gridController).AsSingle();
            Container.BindInstance(_gridPlayerController).AsSingle();

            // views
            Container.BindInstance(_mergeEffectView).AsSingle();

            Container.BindInterfacesAndSelfTo<TimeController>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<MergeCombosController>().AsSingle().NonLazy();

            InstallDataProxies();
        }

        private void InstallDataProxies()
        {
            Container.BindInterfacesAndSelfTo<ScoreDataProxy>().AsSingle().NonLazy();
        }

    }
}
