using System;
using UniRx;
using UnityEngine;
using OGClient.Gameplay.Timers;
using OGClient.Gameplay.DataProxies;
using OGClient.Gameplay.Grid.Configs;
using OGClient.Gameplay.Grid.Models;
using UnityEngine.EventSystems;
namespace OGClient.Gameplay.Grid
{
    public class GridLinksController : IDisposable
    {

        private const string AddLinkProperty = "LinkAdd";
        private const string RemoveLinkProperty = "LinkRemove";
        private const string SelectableProperty = "Selectable";

        private readonly EventSystem _eventSystem;
        private readonly GridController _gridController;
        private readonly CompositeDisposable _compositeDisposable = new ();

        public GridLinksModel Model { get; private set; }

        public GridLinksController(GridController gridController, MatchTimerController matchTimerController, ScriptableGridLinkSettings gridLinkSettings, EventSystem eventSystem,
                                   GridLinksDataProxy gridLinksDataProxy)
        {
            _eventSystem = eventSystem;
            _gridController = gridController;

            matchTimerController.TimeUp.Subscribe(_ => OnControlStateChanged(false)).AddTo(_compositeDisposable);
            gridLinksDataProxy.HasControl.Subscribe(OnControlStateChanged).AddTo(_compositeDisposable);

            Model = new GridLinksModel(gridController.Model, gridLinkSettings);
            Model.LinkExecuted.Subscribe(OnLinkExecuted).AddTo(_compositeDisposable);
        }

        public void Dispose()
        {
            _compositeDisposable?.Dispose();
        }

        private void OnControlStateChanged(bool hasControl)
        {
            Debug.Log($"Control state changed: {hasControl}");

            _eventSystem.enabled = hasControl;
            if (hasControl)
            {
                CheckSelectables();
            }
            else
            {
                ResetSelectables();
            }
        }

        private void OnLinkExecuted(Unit unit)
        {
            ResetSelectables();
            if (Model.PowerupsInLinkCount < 1)
            {
                SpawnSpecial(Model.SpecialIndex, Model.ExecuteTotalTime);
            }

            // Invoke(nameof(RPC_RemoveFromExecuteList), executeTotalTime);
        }

        private void SpawnSpecial(int index, float delay)
        {
            if (index == -1) return;

            GridItemView gridItemView = Model.GetSpecial(index);
            _gridController.CreateGridItem(gridItemView, Model[^1], delay);
        }

        public void CheckSelectables()
        {
            for (int listIndex = _gridController.Model.Count - 1; listIndex >= 0; listIndex--)
            {
                GridItemView currentItemView = _gridController.Model[listIndex].GridItemView;

                if (currentItemView)
                {
                    if (currentItemView.GridItemType == Model.LinkType || Model.LinkType < 0 || currentItemView.GridItemType < 0)
                    {
                        currentItemView.SetAnimatorBool(SelectableProperty, true);
                    }
                    else
                    {
                        currentItemView.SetAnimatorBool(SelectableProperty, false);
                    }

                    // Set the color of the line based on the tile
                    if (Model.LinkType < 0)
                    {
                        _gridController.Model[listIndex].SetConnectorAnimator(true);
                        currentItemView.SetGlowAnimator(true);
                    }
                    else
                    {
                        _gridController.Model[listIndex].SetConnectorLineColor(currentItemView.Color);
                        _gridController.Model[listIndex].SetConnectorAnimator(false);

                        currentItemView.SetGlowAnimator(false);
                    }
                }
            }
        }

        public void ResetSelectables()
        {
            if (_gridController.Model == null) return;
            for (int listIndex = _gridController.Model.Count - 1; listIndex >= 0; listIndex--)
            {
                GridItemView currentItemView = _gridController.Model[listIndex].GridItemView;
                if (currentItemView)
                {
                    currentItemView.SetAnimatorBool(SelectableProperty, true);
                }
            }
        }

    }
}
