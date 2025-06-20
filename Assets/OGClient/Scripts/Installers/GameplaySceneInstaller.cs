using System;
using OGClient.Gameplay;
using OGClient.Gameplay.DataProxies;
using OGClient.Gameplay.Grid;
using OGClient.Gameplay.Grid.MergeCombos;
using OGClient.Gameplay.Timers;
using OGClient.Gameplay.UI;
using OGClient.Popups;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;
namespace OGClient.Installers
{
    public class GameplaySceneInstaller : MonoInstaller
    {

        [Header("References")]
        [SerializeField] private Camera _camera;
        [SerializeField] private EventSystem _eventSystem;

        [Header("Controllers")]
        [SerializeField] private GameManager _gameManager;
        [SerializeField] private PopupsController _popupsController;
        [SerializeField] private GridController _gridController;

        [Header("Vies")]
        [SerializeField] private MergeEffectView _mergeEffectView;
        [SerializeField] private ToastView _toastView;

        public override void InstallBindings()
        {
            Container.BindInstance(_camera).AsSingle();
            Container.BindInstance(_eventSystem).AsSingle();

            // controllers
            Container.BindInstance(_gameManager).AsSingle();
            Container.BindInstance(_popupsController).AsSingle();
            Container.BindInstance(_gridController).AsSingle();

            // views
            Container.BindInstance(_mergeEffectView).AsSingle();
            Container.BindInstance(_toastView).AsSingle();

            Container.BindInterfacesAndSelfTo<TimeController>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<MatchTimerController>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<MergeCombosController>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<GridLinksController>().AsSingle().NonLazy();

            InstallDataProxies();
        }

        private void InstallDataProxies()
        {
            Container.BindInterfacesAndSelfTo<ScoreDataProxy>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<MovesDataProxy>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<RoundsDataProxy>().AsSingle().NonLazy();
        }

    }
}
