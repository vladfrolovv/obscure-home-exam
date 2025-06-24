using System;
using OGClient.Gameplay.DataProxies;
using OGShared.DataProxies;
using UniRx;
using UnityEngine;
namespace OGClient.Gameplay.Players
{
    public class PlayerLinkingController : IDisposable
    {

        private readonly GridLinkingDataProxy _gridLinkingDataProxy;
        private readonly PlayerLinkingDataProxy _playerLinkingDataProxy;
        private readonly GameManager _gameManager;

        private IDisposable _disposableInput;
        private readonly CompositeDisposable _compositeDisposable = new();

        public PlayerLinkingController(GameManager gameManager, PlayerLinkingDataProxy playerLinkingDataProxy, GridLinkingDataProxy gridLinkingDataProxy)
        {
            _gameManager = gameManager;
            _gridLinkingDataProxy = gridLinkingDataProxy;
            _playerLinkingDataProxy = playerLinkingDataProxy;

            InstallInput();
            playerLinkingDataProxy.HasControl.Subscribe(OnInputStateSwitched).AddTo(_compositeDisposable);
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
            if (!_gameManager.CurrentPlayerIsClient || _gameManager.CurrentPlayerMovesLeft <= 0) return;
            if (!Input.GetMouseButtonUp(0) || !_playerLinkingDataProxy.HasControl.Value) return;

            _gridLinkingDataProxy.RaiseLinkExecuted();
        }

    }
}
