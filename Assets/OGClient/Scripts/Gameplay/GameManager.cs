using UniRx;
using System;
using Zenject;
using OGShared;
using UnityEngine;
using OGClient.Popups;
using OGClient.Gameplay.UI;
using OGClient.Gameplay.Grid;
using OGClient.Gameplay.Timers;
using OGClient.Gameplay.Players;
using System.Collections.Generic;
using OGClient.Gameplay.DataProxies;
using OGClient.Gameplay.UI.GameOver;
using OGShared.DataProxies;
using OGShared.Gameplay;
namespace OGClient.Gameplay
{
    public class GameManager : MonoBehaviour
    {

        private readonly Dictionary<MatchPhase, Action> _matchPhaseActions = new();
        private readonly Dictionary<int, NetworkPlayerController> _players = new();
        private readonly Dictionary<int, PlayerView> _playerViews = new();

        [Header("Views")]
        [SerializeField] private RoundsView _roundsView;
        [SerializeField] private PlayerTurnView _playerTurnView;

        private int _playerIndex;

        private GridController _gridController;
        private ScoreDataProxy _scoreDataProxy;
        private PopupsController _popupsController;
        private GridLinksDataProxy _gridLinksDataProxy;
        private MatchTimerController _matchTimerController;
        private ScriptableGameplaySettings _gameplaySettings;
        private GameSessionDataProxy _gameSessionDataProxy;

        // todo: remove, use GameSessionDataProxy instead
        private readonly Subject<PlayerView> _playerSwitched = new();
        public IObservable<PlayerView> PlayerSwitched => _playerSwitched;

        public bool ThisClientIsCurrentPlayer => NetworkPlayerController != null && NetworkPlayerController.HasInputAuthority;
        public NetworkPlayerController NetworkPlayerController { get; private set; }

        [Inject]
        public void Construct(GridController gridController, ScoreDataProxy scoreDataProxy, PopupsController popupsController,
                              MatchTimerController matchTimerController, ScriptableGameplaySettings gameplaySettings,
                              GameSessionDataProxy gameSessionDataProxy, GridLinksDataProxy gridLinksDataProxy)
        {
            _gridController = gridController;
            _scoreDataProxy = scoreDataProxy;
            _gameplaySettings = gameplaySettings;
            _popupsController = popupsController;
            _gridLinksDataProxy = gridLinksDataProxy;
            _matchTimerController = matchTimerController;
            _gameSessionDataProxy = gameSessionDataProxy;
        }

        public void AddNewPlayer(int index, NetworkPlayerController playerController, PlayerView playerView)
        {
            int absoluteIndex = _players.Count;

            _players.TryAdd(absoluteIndex, playerController);
            _playerViews.TryAdd(absoluteIndex, playerView);
        }

        private void Awake()
        {
            if (NetworkRunnerInstance.Instance.IsServer) return;

            _matchPhaseActions.Add(MatchPhase.Starting, StartMatchPhase);
            _matchPhaseActions.Add(MatchPhase.Playing, PlayingMatchPhase);
            _matchPhaseActions.Add(MatchPhase.LastRound, LastRoundPhase);
            _matchPhaseActions.Add(MatchPhase.Ending, EndMatchPhase);

            _gameSessionDataProxy.Initialized.Subscribe(_ => OnGameSessionDataInitialized()).AddTo(this);
            _matchTimerController.TimeUp.Subscribe(_ => TimeUp()).AddTo(this);
        }

        private void OnGameSessionDataInitialized()
        {
            _gameSessionDataProxy.MatchPhaseChanged.Subscribe(OnMatchPhaseChanged).AddTo(this);
            _gameSessionDataProxy.CurrentRound.Subscribe(RoundUpdate).AddTo(this);

            _gridController.InitializeGrid(_gameSessionDataProxy.Seed);

            OnMatchPhaseChanged(MatchPhase.Starting);
        }

        private void OnMatchPhaseChanged(MatchPhase phase)
        {
            Debug.Log($"Match phase changed to: {phase}");
            _matchPhaseActions.TryGetValue(phase, out Action action);
            action?.Invoke();
        }

        private void StartMatchPhase()
        {
            _gridLinksDataProxy.ChangeControlState(false);
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
            _gridController.ClearGrid();
            _gridController.HideGrid();

            _matchTimerController.PauseTimerFor(-1);
            _popupsController.ShowPopupByType(PopupType.GameOver, new GameOverPopupModel($"{GetMatchWinner().PlayerModel.Nickname} WINS!"));
        }

        private void NextPlayer()
        {
            _playerIndex = _playerIndex < _players.Count ? _playerIndex + 1 : 0;
            if (_gameSessionDataProxy.CurrentRound.Value <= _gameSessionDataProxy.Rounds.Value)
            {
                SetCurrentPlayer();
            }

            NextRound();
            _matchTimerController.ResetTime();
        }

        private void SetCurrentPlayer()
        {
            Debug.Log($"Game Manager: Setting current player to index {_playerIndex}");
            NetworkPlayerController = _players[_playerIndex];
            NetworkPlayerController.Moves.SetPlayerMoves(_playerIndex, _gameplaySettings.MovesPerTurn);

            _playerTurnView.ShowAnimation(_playerViews[_playerIndex].PlayerModel.Nickname);
            _roundsView.SetRoundsText($"{_playerViews[_playerIndex].PlayerModel.Nickname + "'S TURN!"}");

            _gridLinksDataProxy.ChangeControlState(true);
            if (ThisClientIsCurrentPlayer)
            {
                _matchTimerController.ResetTime();
                _matchTimerController.StartTimer();
            }

            HighlightPlayer();
            _playerSwitched.OnNext(_playerViews[_playerIndex]);
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
            // if (CurrentPlayerController.photonView.IsMine)
            // {
            //     CurrentPlayerController.photonView.RPC("SetMoves", RpcTarget.All, 0);
            // }

            Debug.Log($"Time is Up. trying to cancel execution and end turn.");
            _gridLinksDataProxy.ChangeControlState(true);
            NextPlayer();
        }

        private void ResetRounds()
        {
            _roundsView.SetRoundProgress(1);
            // todo: update rounds view; set current round "1"
        }

        private void NextRound()
        {
            if (_playerIndex != 0) return;

            _roundsView.ChangeRoundProgress(1);
            // todo: update rounds view; set current round "_roundsDataProxy.CurrentRound.Value + 1"
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