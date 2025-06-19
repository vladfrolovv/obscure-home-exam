using UniRx;
using System;
using Zenject;
using UnityEngine;
using OGClient.Popups;
using OGServer.Gameplay;
using OGClient.Gameplay.UI;
using OGClient.Gameplay.Grid;
using OGClient.Gameplay.Timers;
using OGClient.Gameplay.Players;
using System.Collections.Generic;
using OGClient.Gameplay.DataProxies;
using OGClient.Gameplay.UI.GameOver;
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
        private NetworkPlayerController _currentPlayerController;

        private GridController _gridController;
        private ScoreDataProxy _scoreDataProxy;
        private PopupsController _popupsController;
        private GridLinksController _gridLinksController;
        private MatchTimerController _matchTimerController;
        private ScriptableGameplaySettings _gameplaySettings;

        private readonly Subject<PlayerView> _playerSwitched = new();
        public IObservable<PlayerView> PlayerSwitched => _playerSwitched;

        public void AddNewPlayer(int index, NetworkPlayerController playerController) => _players.TryAdd(index, playerController);
        public void AddNewPlayerView(int index, PlayerView playerView) => _playerViews.TryAdd(index, playerView);

        [Inject]
        public void Construct(GridController gridController, ScoreDataProxy scoreDataProxy, PopupsController popupsController,
                              MatchTimerController matchTimerController, ScriptableGameplaySettings gameplaySettings,
                              GridLinksController gridLinksController)
        {
            _gridController = gridController;
            _scoreDataProxy = scoreDataProxy;
            _gameplaySettings = gameplaySettings;
            _popupsController = popupsController;
            _gridLinksController = gridLinksController;
            _matchTimerController = matchTimerController;
        }

        private void Awake()
        {
            _matchPhaseActions.Add(MatchPhase.Starting, StartMatchPhase);
            _matchPhaseActions.Add(MatchPhase.LastRound, LastRoundPhase);
            _matchPhaseActions.Add(MatchPhase.Ending, EndMatchPhase);

            _matchTimerController.TimeUp.Subscribe(_ => TimeUp()).AddTo(this);
            _matchPhaseActions[MatchPhase.Starting]?.Invoke();
        }

        private void Start()
        {
            _gridController.CreateGrid();

            // NetworkGameManager.Instance.MatchPhaseChanged.Subscribe(OnMatchPhaseChanged).AddTo(this);
            // NetworkGameManager.Instance.RoundChanged.Subscribe(RoundUpdate).AddTo(this);
        }

        private void OnMatchPhaseChanged(MatchPhase phase)
        {
            Debug.Log($"Match phase changed to: {phase}");
            _matchPhaseActions.TryGetValue(phase, out Action action);
            action?.Invoke();
        }

        private void StartMatchPhase()
        {
            _gridLinksController.LoseControl(0);

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
            if (NetworkGameManager.Instance.CurrentRound <= NetworkGameManager.Instance.Rounds)
            {
                SetCurrentPlayer();
            }

            NextRound();
            _matchTimerController.ResetTime();
        }

        private void SetCurrentPlayer()
        {
            return;
            _currentPlayerController = _players[_playerIndex];
            _currentPlayerController.Moves.SetPlayerMoves(_playerIndex, _gameplaySettings.MovesPerTurn);
            _playerTurnView.ShowAnimation(_playerViews[_playerIndex].PlayerModel.Nickname);
            _roundsView.SetRoundsText($"{_playerViews[_playerIndex].PlayerModel.Nickname + "'S TURN!"}");
            _gridLinksController.RegainControl();

            HighlightPlayer();
            _playerSwitched.OnNext(_playerViews[_playerIndex]);

            // todo: change to fusion 2
            // if (CurrentPlayerController.photonView.IsMine)
            // {
            //     _matchTimerController.ResetTime();
            //     Invoke(nameof(RPC_StartTimer), 0.5f);
            // }
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

            _gridLinksController.CancelExecuteLink();
            EndTurn();
        }

        private void EndTurn()
        {
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

        private void RoundUpdate((int, int) roundInfo)
        {
            (int currentRound, int rounds) = roundInfo;

            _roundsView.SetRoundsText($"{_gameplaySettings[MatchPhase.Playing]} {currentRound}/{rounds}");
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