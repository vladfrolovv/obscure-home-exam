using System;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using TMPro;
using OGClient.Gameplay.DataProxies;
using OGClient.Gameplay.Grid;
using OGClient.Gameplay.Players;
using OGClient.Gameplay.UI;
using OGClient.Gameplay.UI.GameOver;
using OGClient.Popups;
using OGServer.Gameplay;
using UniRx;
using Zenject;
namespace OGClient.Gameplay
{
    public class GameManager : MonoBehaviour
    {

        private readonly Dictionary<int, NetworkPlayerController> _players = new();
        private readonly Dictionary<MatchPhase, Action> _matchPhaseActions = new();

        [field: SerializeField, Header("Players")]
        public GridPlayerController GridPlayerController { get; private set; }

        [field: SerializeField]
        public NetworkPlayerController CurrentPlayerController { get; private set; }

        [Header("Timers and Rounds")]
        [SerializeField] private ProgressBarView _timerView;
        [SerializeField] private Animator _playerTurnAnimator;
        [SerializeField] private TextMeshProUGUI _playerTurnText;

        [Header("Specials")]
        [SerializeField] private SpecialLink[] _specialLinks;

        private float _startDelay;
        private float _timePerRound;
        private float _timeLeft;

        private bool _timerIsActive;
        private bool _timeAlmostUp;

        private PopupsController _popupsController;
        private NetworkGameManager _networkGameManager;
        private ScoreDataProxy _scoreDataProxy;
        private RoundsDataProxy _roundsDataProxy;
        private GridController _gridController;
        private ScriptableGameplaySettings _gameplaySettings;

        public int PlayerIndex { get; private set; } = 1;

        public SpecialLink[] SpecialLinks => _specialLinks;

        public void AddNewPlayer(int index, NetworkPlayerController playerController) => _players.TryAdd(index, playerController);

        [Inject]
        public void Construct(GridController gridController, ScriptableGameplaySettings gameplaySettings, ScoreDataProxy scoreDataProxy,
                              RoundsDataProxy roundsDataProxy, NetworkGameManager networkGameManager, PopupsController popupsController)
        {
            _popupsController = popupsController;
            _gridController = gridController;
            _gameplaySettings = gameplaySettings;
            _scoreDataProxy = scoreDataProxy;
            _roundsDataProxy = roundsDataProxy;
            _networkGameManager = networkGameManager;
        }

        private void Awake()
        {
            _matchPhaseActions.Add(MatchPhase.Starting, StartMatch);
            _matchPhaseActions.Add(MatchPhase.Ending, EndMatch);

            _networkGameManager.MatchPhaseChanged.Subscribe(OnMatchPhaseChanged).AddTo(this);
        }

        private void Setup()
        {
            _startDelay = _gameplaySettings.StartDelay;
            _timePerRound = _gameplaySettings.TimePerTurn;

            if (_timerView)
            {
                _timerView.SetProgress(_timePerRound);
                _timerView.SetProgressMax(_timePerRound);
                _timerView.Setup(null);
                _timerView.gameObject.SetActive(false);
            }

            if (GridPlayerController)
            {
                GridPlayerController.LoseControl(0);
            }
        }

        private void OnMatchPhaseChanged(MatchPhase phase)
        {
            Debug.Log($"Match phase changed to: {phase}");
            _matchPhaseActions.TryGetValue(phase, out Action action);
            action?.Invoke();
        }

        private void Update()
        {
            if (_timeLeft > 0)
            {
                if (_timerIsActive) _timeLeft -= Time.deltaTime;

                if (_timerView)
                {
                    _timerView.SetProgress(_timeLeft);
                    _timerView.UpdateProgress(0);
                }

                // if (_timeLeft <= 10)
                // {
                //     photonView.RPC(nameof(TimeAlmostUp), RpcTarget.All);
                // }
            }
            else if (_timerIsActive)
            {
                // if (CurrentPlayerController.photonView.IsMine)
                // {
                //     photonView.RPC(nameof(TimeUp), RpcTarget.All);
                // }
            }
        }

        private void StartMatch()
        {
            Setup();

            _gridController.FillGrid();
            _gridController.ShowGrid();

            Invoke(nameof(ResetRounds), _startDelay);
            Invoke(nameof(SetCurrentPlayer), _startDelay);
        }

        public void RPC_NextPlayer()
        {
            // photonView.RPC(nameof(NextPlayer), RpcTarget.All);
        }

        public void NextPlayer()
        {
            if (PlayerIndex < _players.Count)
            {
                PlayerIndex++;
            }
            else
            {
                PlayerIndex = 1;
                NextRound();
            }

            if (_roundsDataProxy.CanSetPlayerUp)
            {
                // add callback
                SetCurrentPlayer();
            }

            ResetTime();
            _timerView.ChangeProgress(1000);
        }


        void RPC_SetCurrentPlayer()
        {
            // photonView.RPC(nameof(SetCurrentPlayer), RpcTarget.All);
        }

        public void SetCurrentPlayer()
        {
            CurrentPlayerController = _players[PlayerIndex];

            HighlightPlayer();


            // todo:
            // CurrentPlayerController.SetMoves(_movesPerRound);
            // if (CurrentPlayerController.MovesBarView) CurrentPlayerController.MovesBarView.SetProgress(_movesPerRound);

            _roundsDataProxy.OnPlayerSwitched(
                new PlayerSwitchModel(CurrentPlayerController.PlayerModel.Color));

            if (_timerView)
            {
                // _timerView.SetBarColor(CurrentPlayerController.PlayerColor);
            }

            // _playerTurnText.SetText(CurrentPlayerController.PlayerName + "'S TURN!");
            _playerTurnAnimator.Play("Intro");

            // _roundsText.SetText(CurrentPlayerController.PlayerName + "'S TURN!");

            GridPlayerController.RegainControl();

            // if (CurrentPlayerController.photonView.IsMine)
            // {
            //     ResetTime();
            //     Invoke(nameof(RPC_StartTimer), 0.5f);
            // }
        }

        public void HighlightPlayer()
        {
            for (int playerIndex = 1; playerIndex <= _players.Count; playerIndex++)
            {
                // if (_players[playerIndex] == CurrentPlayerController) LeanTween.color(_players[playerIndex].avatarImage.rectTransform, Color.white, 0.5f);
                // else LeanTween.color(_players[playerIndex].avatarImage.rectTransform, Color.gray, 0.5f);
            }
        }

        public void ResetTime()
        {
            _timeLeft = _timePerRound;
            _timeAlmostUp = false;

            if (_timerView) _timerView.Bounce();

            LeanTween.cancel(_timerView.gameObject);
        }


        public void RPC_StartTimer()
        {
            // photonView.RPC(nameof(StartTimer), RpcTarget.All);
        }

        public void StartTimer()
        {
            _timerIsActive = true;

            if (_timerView) _timerView.gameObject.SetActive(true);
        }

        public void PauseTime(float delay)
        {
            _timerIsActive = false;

            // if (CurrentPlayerController.photonView.IsMine)
            // {
            //     CancelInvoke(nameof(RPC_StartTimer));
            //     if (delay > 0) Invoke(nameof(RPC_StartTimer), delay);
            // }
        }

        public void TimeAlmostUp()
        {
            if (_timeAlmostUp == true) return;

            _timeAlmostUp = true;

            LeanTween.scale(_timerView.gameObject, Vector3.one * 1.1f, 0.5f).setLoopPingPong().setEaseInBack();
        }

        public void TimeUp()
        {
            if (_timerIsActive == false) return;

            _timerIsActive = false;

            _timeLeft = 0;

            if (_timerView)
            {
                LeanTween.scale(_timerView.gameObject, Vector3.one * 1, 0.3f).setEaseInBack();

                _timerView.Shake();
            }

            // if (CurrentPlayerController.photonView.IsMine)
            // {
            //     CurrentPlayerController.photonView.RPC("SetMoves", RpcTarget.All, 0);
            // }

            if (GridPlayerController) GridPlayerController.CancelExecuteLink();

            EndTurn();
        }

        public void EndTurn()
        {
            NextPlayer();
        }

        private void ResetRounds()
        {
            _roundsDataProxy.SetCurrentRound(1);
            _roundsDataProxy.SetRoundProgress(1);

            OnRoundsUpdate();
        }

        private void NextRound()
        {
            _roundsDataProxy.SetCurrentRound(_roundsDataProxy.CurrentRound.Value + 1);
            _roundsDataProxy.ChangeRoundProgress(1);

            OnRoundsUpdate();
        }

        private void OnRoundsUpdate()
        {
            if (_roundsDataProxy.CurrentRound.Value > _roundsDataProxy.Rounds.Value)
            {
                _gridController.ClearGrid();
                _gridController.HideGrid();

                // todo: make this a server callback
                EndMatch();
            }
            else if (_roundsDataProxy.CurrentRound.Value == _roundsDataProxy.Rounds.Value)
            {
                _roundsDataProxy.OnRoundsTextChanged($"LAST ROUND!");
                _roundsDataProxy.OnCurrentRoundTextChanged($"LAST ROUND!");
            }
            else
            {
                _roundsDataProxy.OnRoundsTextChanged($"ROUND {_roundsDataProxy.CurrentRound.Value}/{_roundsDataProxy.Rounds.Value}");
                _roundsDataProxy.OnCurrentRoundTextChanged($"ROUND {_roundsDataProxy.CurrentRound.Value}");;
            }
        }

        private void EndMatch()
        {
            _timerIsActive = false;
            NetworkPlayerController winner = _players[1];
            for (int playerIndex = 1; playerIndex <= _players.Count; playerIndex++)
            {
                if (_scoreDataProxy.GetPlayerScore(playerIndex) > _scoreDataProxy.GetPlayerScore(1))
                {
                    winner = _players[playerIndex];
                }
            }

            _popupsController.ShowPopupByType(PopupType.GameOver, new GameOverPopupModel($"{winner.PlayerModel.Nickname} WINS!"));
        }

        [Serializable]
        public class SpecialLink
        {
            public int LinkSize = 4;
            public GridItemView SpawnItemView;
        }

    }
}