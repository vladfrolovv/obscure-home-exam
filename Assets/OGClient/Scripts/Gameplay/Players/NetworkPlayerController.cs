using Fusion;
using OGClient.Gameplay.DataProxies;
using UnityEngine;
using OGClient.Gameplay.UI;
using OGShared;
using OGShared.DataProxies;
using OGShared.Gameplay;
using UniRx;
using Zenject;
namespace OGClient.Gameplay.Players
{
    public class NetworkPlayerController : NetworkBehaviour
    {

        [Header("Configs")]
        [SerializeField] private ScriptableGameplaySettings _gameplaySettings;
        [SerializeField] private ScriptablePlayersProfiles _playersProfiles;

        [Header("Views")]
        [SerializeField] private ScoreView _scoreView;
        [SerializeField] private MovesView _movesView;
        [SerializeField] private PlayerView _playerView;

        private int _playerIndex;
        private bool _isMainPlayer;

        private GameManager _gameManager;
        private ScoreDataProxy _scoreDataProxy;
        private ClientsAvailabilityController _availabilityController;

        [Inject]
        public void Construct(ScoreDataProxy scoreDataProxy, GameManager gameManager, ClientsAvailabilityController clientsAvailabilityController)
        {
            _gameManager = gameManager;
            _scoreDataProxy = scoreDataProxy;
            _availabilityController = clientsAvailabilityController;
        }

        public override void Spawned()
        {
            InstallPlayerViews();

            if (!Object.HasInputAuthority) return;
            RPC_ClientReady();
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_ClientReady()
        {
            Debug.Log($"[SERVER] Player {Object.InputAuthority} is ready.");
            _availabilityController.OnClientReady(Object.InputAuthority);
        }

        private void InstallPlayerViews()
        {
            _playerIndex = _gameManager.AddNewPlayer(this, _playerView);
            _scoreDataProxy.SetPlayerScore(_playerIndex, 0);
            _isMainPlayer = Object.HasInputAuthority;
            PlayerModel playerModel = new (_playerIndex, isMain: _isMainPlayer, nickname: $"Player {_playerIndex}",
                _playersProfiles.GetColor(_isMainPlayer));

            _scoreView.SetPlayerIndex(_playerIndex);
            _movesView.Setup(_playerIndex, _gameplaySettings.MovesPerTurn, _playersProfiles.GetColor(_isMainPlayer), this);
            _playerView.InstallPlayerView(playerModel);
        }

    }
}