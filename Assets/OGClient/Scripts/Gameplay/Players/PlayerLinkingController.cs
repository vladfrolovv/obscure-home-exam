using System;
using OGClient.Gameplay.DataProxies;
using UniRx;
using UnityEngine;
namespace OGClient.Gameplay.Players
{
    public class PlayerLinkingController : IDisposable
    {

        private readonly GameManager _gameManager;
        private readonly GridLinksDataProxy _gridLinksDataProxy;

        private IDisposable _disposableInput;
        private readonly CompositeDisposable _compositeDisposable = new();

        public PlayerLinkingController(GridLinksDataProxy gridLinksDataProxy, GameManager gameManager)
        {
            _gameManager = gameManager;
            _gridLinksDataProxy = gridLinksDataProxy;

            InstallInput();
            gridLinksDataProxy.HasControl.Subscribe(OnInputStateSwitched).AddTo(_compositeDisposable);
        }

        public void Dispose()
        {
            _disposableInput?.Dispose();
        }

        private void OnInputStateSwitched(bool hasInput)
        {
            Debug.Log($"Input state switched: {hasInput}");
            if (!hasInput)
            {
                _disposableInput?.Dispose();
            }
            else
            {
                InstallInput();
            }
        }

        private void InstallInput()
        {
            _disposableInput = Observable.EveryUpdate().Subscribe(OnInputUpdate);
        }

        private void OnInputUpdate(long l)
        {
            if (!_gameManager.ThisClientIsCurrentPlayer || _gameManager.CurrentPlayerMovesLeft <= 0) return;
            if (!Input.GetMouseButtonUp(0) || !_gridLinksDataProxy.HasControl.Value) return;

            _gridLinksDataProxy.TryToExecuteLink();
        }

    }
}
