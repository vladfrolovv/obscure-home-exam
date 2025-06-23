using System;
using OGClient.Gameplay;
using OGClient.Gameplay.DataProxies;
using OGClient.Gameplay.Grid;
using OGClient.Gameplay.Grid.MergeCombos;
using OGClient.Gameplay.Players;
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
        [SerializeField] private PopupsController _popupsController;

        [Header("Vies")]
        [SerializeField] private MergeEffectView _mergeEffectView;
        [SerializeField] private ToastView _toastView;
        [SerializeField] private GridView _gridView;
        [SerializeField] private GridHolderView _gridHolderView;
        [SerializeField] private RoundsView _roundsView;
        [SerializeField] private PlayerTurnView _playerTurnView;

        public override void InstallBindings()
        {
            Container.BindInstance(_camera).AsSingle();
            Container.BindInstance(_eventSystem).AsSingle();

            // controllers
            Container.BindInstance(_popupsController).AsSingle();

            // views
            Container.BindInstance(_mergeEffectView).AsSingle();
            Container.BindInstance(_toastView).AsSingle();
            Container.BindInstance(_gridView).AsSingle();
            Container.BindInstance(_roundsView).AsSingle();
            Container.BindInstance(_gridHolderView).AsSingle();
            Container.BindInstance(_playerTurnView).AsSingle();

            Container.BindInterfacesAndSelfTo<TimeController>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<MatchTimerController>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<MergeCombosController>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<GridController>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<GridLinksController>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<PlayerLinkingController>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<GameManager>().AsSingle().NonLazy();

            InstallDataProxies();
        }

        private void InstallDataProxies()
        {
            Container.BindInterfacesAndSelfTo<ScoreDataProxy>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<PlayerLinkingDataProxy>().AsSingle().NonLazy();
        }

    }
}
