using Fusion;
using OGClient.Gameplay.DataProxies;
using UnityEngine;
using OGClient.Gameplay.UI;
using OGShared.Gameplay;
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
        private MovesDataProxy _movesDataProxy;
        private ScoreDataProxy _scoreDataProxy;

        public int MovesLeft => _movesDataProxy.GetMovesLeft(_playerIndex);
        public int Score => _scoreDataProxy.GetPlayerScore(_playerIndex);

        public MovesDataProxy Moves => _movesDataProxy;

        [Inject]
        public void Construct(MovesDataProxy movesDataProxy, ScoreDataProxy scoreDataProxy, GameManager gameManager)
        {
            _gameManager = gameManager;
            _movesDataProxy = movesDataProxy;
            _scoreDataProxy = scoreDataProxy;
        }

        public override void Spawned()
        {
            InstallPlayerViews();
        }

        private void InstallPlayerViews()
        {
            _playerIndex = Object.InputAuthority.RawEncoded;
            _isMainPlayer = Object.HasInputAuthority;

            PlayerModel playerModel = new (_playerIndex, isMain: _isMainPlayer, nickname: $"Player {_playerIndex}",
                _playersProfiles.GetColor(_isMainPlayer));

            _scoreView.SetPlayerIndex(_playerIndex);
            _movesView.Setup(_playerIndex, _gameplaySettings.MovesPerTurn, _playersProfiles.GetColor(_isMainPlayer), this);
            _playerView.InstallPlayerView(playerModel);

            _gameManager.AddNewPlayer(_playerIndex, this, _playerView);

            _movesDataProxy.SetPlayerMoves(_playerIndex, _gameplaySettings.MovesPerTurn);
            _scoreDataProxy.SetPlayerScore(_playerIndex, 0);
        }

    }
}