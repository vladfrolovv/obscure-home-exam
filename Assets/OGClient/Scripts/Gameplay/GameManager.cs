using UniRx;
using System;
using OGShared;
using UnityEngine;
using OGClient.Popups;
using OGShared.Gameplay;
using OGClient.Gameplay.UI;
using OGShared.DataProxies;
using OGClient.Gameplay.Grid;
using OGClient.Gameplay.Timers;
using OGClient.Gameplay.Players;
using System.Collections.Generic;
using OGClient.Gameplay.DataProxies;
using OGClient.Gameplay.Mathchmaking;
using OGClient.Gameplay.UI.GameOver;
namespace OGClient.Gameplay
{
    public class GameManager : IDisposable, IMatchPhasable
    {

        private readonly Dictionary<MatchPhase, Action> _matchPhaseActions = new();
        private readonly Dictionary<int, NetworkPlayerController> _players = new();
        private readonly Dictionary<int, PlayerView> _playerViews = new();

        private readonly CompositeDisposable _compositeDisposable = new();

        private int _playerIndex;
        private NetworkPlayerController _currentNetworkController;

        private readonly ToastView _toastView;
        private readonly RoundsView _roundsView;
        private readonly PlayerTurnView _playerTurnView;
        private readonly ScoreDataProxy _scoreDataProxy;
        private readonly PopupsController _popupsController;
        private readonly GridLinksDataProxy _gridLinksDataProxy;
        private readonly MovesDataProxy _movesDataProxy;
        private readonly MatchTimerController _matchTimerController;
        private readonly ScriptableGameplaySettings _gameplaySettings;
        private readonly GameSessionDataProxy _gameSessionDataProxy;
        private readonly GridLinksController _gridLinksController;

        public bool ThisClientIsCurrentPlayer => _currentNetworkController != null && _currentNetworkController.HasInputAuthority;
        public int CurrentPlayerMovesLeft => _movesDataProxy.GetMovesLeft(_playerIndex);

        public GameManager(ScoreDataProxy scoreDataProxy, PopupsController popupsController, MatchTimerController matchTimerController, ScriptableGameplaySettings gameplaySettings,
                           GameSessionDataProxy gameSessionDataProxy, GridLinksDataProxy gridLinksDataProxy, RoundsView roundsView, PlayerTurnView playerTurnView,
                           GridLinksController gridLinksController, ToastView toastView, MovesDataProxy movesDataProxy)
        {
            if (NetworkRunnerInstance.Instance.IsServer) return;

            _toastView = toastView;
            _roundsView = roundsView;
            _playerTurnView = playerTurnView;
            _movesDataProxy = movesDataProxy;
            _scoreDataProxy = scoreDataProxy;
            _gameplaySettings = gameplaySettings;
            _popupsController = popupsController;
            _gridLinksDataProxy = gridLinksDataProxy;
            _matchTimerController = matchTimerController;
            _gameSessionDataProxy = gameSessionDataProxy;
            _gridLinksController = gridLinksController;

            _matchPhaseActions.Add(MatchPhase.Starting, StartMatchPhase);
            _matchPhaseActions.Add(MatchPhase.Playing, PlayingMatchPhase);
            _matchPhaseActions.Add(MatchPhase.LastRound, LastRoundPhase);
            _matchPhaseActions.Add(MatchPhase.Ending, EndMatchPhase);

            _gameSessionDataProxy.Initialized.Subscribe(_ => OnGameSessionDataInitialized()).AddTo(_compositeDisposable);
            _matchTimerController.TimeUp.Subscribe(_ => TimeUp()).AddTo(_compositeDisposable);
            _movesDataProxy.CurrentPlayerIdChanged.Subscribe(NextPlayer).AddTo(_compositeDisposable);
        }

        public void Dispose()
        {
            _compositeDisposable?.Dispose();
        }

        public void AddNewPlayer(int index, NetworkPlayerController playerController, PlayerView playerView)
        {
            int absoluteIndex = _players.Count;
            Debug.Log($"[CLIENT] Adding player {absoluteIndex}");

            _players.TryAdd(absoluteIndex, playerController);
            _playerViews.TryAdd(absoluteIndex, playerView);
        }

        public void OnMatchPhaseChanged(MatchPhase phase)
        {
            Debug.Log($"[CLIENT][GAME_MANAGER] Match phase changed to: {phase}");
            _matchPhaseActions.TryGetValue(phase, out Action action);
            action?.Invoke();
        }

        public void OnGameSessionDataInitialized()
        {
            _gameSessionDataProxy.MatchPhaseChanged.Subscribe(OnMatchPhaseChanged).AddTo(_compositeDisposable);
            _gameSessionDataProxy.CurrentRound.Subscribe(RoundUpdate).AddTo(_compositeDisposable);

            OnMatchPhaseChanged(MatchPhase.Starting);
        }

        private void OnLinkExecuted(Unit unit)
        {
            _movesDataProxy.TriggerSpendMoveRequest(-1);
            if (_gridLinksController.Model.PowerupsInLinkCount > 1)
            {
                _movesDataProxy.TriggerSpendMoveRequest(1);
                _toastView.SetToast(_gridLinksController.Model[^1].transform.position, "EXTRA MOVE!", new Color(1, 0.37f, 0.67f, 1));
            }

            _matchTimerController.PauseTimerFor(-1);
            _gridLinksDataProxy.ChangeControlState(false);
        }

        private void StartMatchPhase()
        {
            _gridLinksDataProxy.ChangeControlState(false);
            _gridLinksController.Model.LinkExecuted.Subscribe(OnLinkExecuted).AddTo(_compositeDisposable);
        }

        private void PlayingMatchPhase()
        {
            ResetRounds();
            SetCurrentPlayer();
        }

        private void LastRoundPhase()
        {
            _roundsView.SetRoundsText(_gameplaySettings[MatchPhase.LastRound]);
            _roundsView.SetCurrentRoundText(_gameplaySettings[MatchPhase.LastRound]);
        }

        private void EndMatchPhase()
        {
            _matchTimerController.PauseTimerFor(-1);
            _popupsController.ShowPopupByType(PopupType.GameOver, new GameOverPopupModel($"{GetMatchWinner().PlayerModel.Nickname} WINS!"));
        }

        private void NextPlayer(int playerIndex)
        {
            _playerIndex = playerIndex;
            if (_gameSessionDataProxy.CurrentRound.Value <= _gameSessionDataProxy.Rounds.Value)
            {
                SetCurrentPlayer();
            }

            _matchTimerController.ResetTime();
        }

        private void SetCurrentPlayer()
        {
            _currentNetworkController = _players[_playerIndex];
            _movesDataProxy.TriggerResetMovesRequest();

            _playerTurnView.ShowAnimation(_playerViews[_playerIndex].PlayerModel.Nickname);
            _roundsView.SetRoundsText($"{_playerViews[_playerIndex].PlayerModel.Nickname + "'S TURN!"}");

            _gridLinksDataProxy.ChangeControlState(true);
            if (ThisClientIsCurrentPlayer)
            {
                _matchTimerController.ResetTime();
                _matchTimerController.StartTimer();
            }

            HighlightPlayer();
            _roundsView.OnPlayerSwitched(_playerViews[_playerIndex]);
        }

        private void HighlightPlayer()
        {
            for (int playerIndex = 0; playerIndex < _players.Count; playerIndex++)
            {
                _playerViews[playerIndex].HighlightPlayer(_playerIndex == playerIndex);
            }
        }

        private void TimeUp()
        {
            Debug.Log($"Time is Up. trying to cancel execution and end turn.");
            if (ThisClientIsCurrentPlayer)
            {
                _movesDataProxy.TriggerSpendMoveRequest(0);
            }
            _gridLinksDataProxy.ChangeControlState(true);
            // NextPlayer();
        }

        private void ResetRounds()
        {
            _roundsView.SetRoundProgress(1);
            // todo: update rounds view; set current round "1"
        }

        private void RoundUpdate(int currentRound)
        {
            _roundsView.SetRoundsText($"{_gameplaySettings[MatchPhase.Playing]} {currentRound}/{_gameSessionDataProxy.Rounds.Value}");
            _roundsView.SetCurrentRoundText($"{_gameplaySettings[MatchPhase.Playing]} {currentRound}");;
        }

        private PlayerView GetMatchWinner()
        {
            return _scoreDataProxy.GetPlayerScore(0) > _scoreDataProxy.GetPlayerScore(1)
                ? _playerViews[0]
                : _playerViews[1];
        }

    }
}