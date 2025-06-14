using System;
using System.Collections.Generic;
using ObscureGames.Gameplay.DataProxies;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using ObscureGames.Gameplay.Grid;
using ObscureGames.Gameplay.UI;
using ObscureGames.Networking;
using Photon.Pun;
using Zenject;
namespace ObscureGames.Gameplay
{
    public class GameManager : MonoBehaviourPunCallbacks, IPunObservable
    {

        private readonly Dictionary<int, NetworkPlayerController> _players = new Dictionary<int, NetworkPlayerController>();

        [field: SerializeField, Header("Players")]
        public GridPlayerController GridPlayerController { get; private set; }

        [field: SerializeField]
        public NetworkPlayerController CurrentPlayerController { get; private set; }

        [Header("Timers and Rounds")]
        [SerializeField] private ProgressBarView _timerView;

        [SerializeField] private Animator _playerTurnAnimator;
        [SerializeField] private TextMeshProUGUI _playerTurnText;

        [SerializeField] private TextMeshProUGUI _roundsText;
        [SerializeField] private ProgressBarView _roundsBarView;
        [SerializeField] private TextMeshProUGUI _currentRoundText;

        [Header("Game Endings")]
        [SerializeField] private GameObject _gameOverScreen;
        [SerializeField] private TextMeshProUGUI _winnerText;
        [SerializeField] private Button _restartButton;

        [Header("Specials")]
        [SerializeField] private SpecialLink[] _specialLinks;

        private float _startDelay;
        private float _timePerRound;
        private float _timeLeft;
        private int _movesPerRound;
        private int _extraMoveAtLink;
        private int _rounds;
        private int _currentRound;

        private bool _timerIsActive;
        private bool _timeAlmostUp;

        private ScoreDataProxy _scoreDataProxy;
        private GridController _gridController;
        private ScriptableGameplaySettings _gameplaySettings;

        public int PlayerIndex { get; private set; } = 1;
        public Camera MainCamera { get; private set; }

        public SpecialLink[] SpecialLinks => _specialLinks;

        [Inject]
        public void Construct(GridController gridController, ScriptableGameplaySettings gameplaySettings, Camera mainCamera,
                              ScoreDataProxy scoreDataProxy)
        {
            _gridController = gridController;
            _gameplaySettings = gameplaySettings;
            _scoreDataProxy = scoreDataProxy;

            MainCamera = mainCamera;
        }

        private void InstallBaseSettings()
        {
            PlayerIndex = 1;
            _startDelay = _gameplaySettings.StartDelay;
            _movesPerRound = _gameplaySettings.MovesPerTurn;
            _extraMoveAtLink = _gameplaySettings.ExtraMovesPerLink;
            _timePerRound = _gameplaySettings.TimePerTurn;
            _rounds = _gameplaySettings.RoundsPerGame;
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(_gameplaySettings.StartDelay);
                stream.SendNext(PlayerIndex);

                stream.SendNext(_gameplaySettings.MovesPerTurn);
                stream.SendNext(_gameplaySettings.ExtraMovesPerLink);
                stream.SendNext(_gameplaySettings.TimePerTurn);
                stream.SendNext(_timeLeft);
                stream.SendNext(_gameplaySettings.RoundsPerGame);
                stream.SendNext(_currentRound);
            }
            else
            {
                _startDelay = (float)stream.ReceiveNext();
                PlayerIndex = (int)stream.ReceiveNext();
                _movesPerRound = (int)stream.ReceiveNext();
                _extraMoveAtLink = (int)stream.ReceiveNext();
                _timePerRound = (float)stream.ReceiveNext();
                _timeLeft = (float)stream.ReceiveNext();
                _rounds = (int)stream.ReceiveNext();
                _currentRound = (int)stream.ReceiveNext();
            }
        }

        private void Setup()
        {
            InstallBaseSettings();

            _gameOverScreen.SetActive(false);
            _roundsText.SetText("");

            if (_roundsBarView)
            {
                _roundsBarView.SetProgress(0);
                _roundsBarView.SetProgressMax(_rounds);
                _roundsBarView.Setup(null);
            }

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

        private void Start()
        {
            Setup();

            if (PhotonNetwork.LocalPlayer.IsMasterClient)
            {
                Invoke(nameof(RPC_StartMatch), _startDelay * 0.2f);
            }
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

                if (_timeLeft <= 10)
                {
                    photonView.RPC(nameof(TimeAlmostUp), RpcTarget.All);
                }
            }
            else if (_timerIsActive)
            {
                if (CurrentPlayerController.photonView.IsMine)
                {
                    photonView.RPC(nameof(TimeUp), RpcTarget.All);
                }
            }
        }

        void RPC_StartMatch()
        {
            photonView.RPC(nameof(StartMatch), RpcTarget.All);
        }

        [PunRPC]
        public void StartMatch()
        {
            _gridController.FillGrid();
            _gridController.ShowGrid();

            Invoke(nameof(ResetRounds), _startDelay);
            Invoke(nameof(SetCurrentPlayer), _startDelay);
        }

        public void RPC_NextPlayer()
        {
            photonView.RPC(nameof(NextPlayer), RpcTarget.All);
        }

        [PunRPC]
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

            if (_currentRound <= _rounds) SetCurrentPlayer();

            ResetTime();
            _timerView.ChangeProgress(1000);
        }


        void RPC_SetCurrentPlayer()
        {
            photonView.RPC(nameof(SetCurrentPlayer), RpcTarget.All);
        }

        public void SetCurrentPlayer()
        {
            CurrentPlayerController = _players[PlayerIndex];

            HighlightPlayer();

            CurrentPlayerController.SetMoves(_movesPerRound);
            if (CurrentPlayerController.MovesBarView) CurrentPlayerController.MovesBarView.SetProgress(_movesPerRound);

            if (_roundsBarView)
            {
                _roundsBarView.SetIncrementColor(CurrentPlayerController.PlayerColor);
                _roundsBarView.Bounce();
            }

            if (_timerView)
            {
                _timerView.SetBarColor(CurrentPlayerController.PlayerColor);
            }

            _playerTurnText.SetText(CurrentPlayerController.PlayerName + "'S TURN!");
            _playerTurnAnimator.Play("Intro");

            _roundsText.SetText(CurrentPlayerController.PlayerName + "'S TURN!");

            GridPlayerController.RegainControl();

            if (CurrentPlayerController.photonView.IsMine)
            {
                ResetTime();
                Invoke(nameof(RPC_StartTimer), 0.5f);
            }
        }

        public void HighlightPlayer()
        {
            for (int playerIndex = 1; playerIndex <= _players.Count; playerIndex++)
            {
                if (_players[playerIndex] == CurrentPlayerController) LeanTween.color(_players[playerIndex].avatarImage.rectTransform, Color.white, 0.5f);
                else LeanTween.color(_players[playerIndex].avatarImage.rectTransform, Color.gray, 0.5f);
            }
        }

        public void HidePlayers()
        {
            for (int playerIndex = 1; playerIndex <= _players.Count; playerIndex++)
            {
                //players[playerIndex].PlayerCanvas.GetComponent<CanvasGroup>().alpha = 0;
            }
        }

        public void ShowPlayers()
        {
            for (int playerIndex = 1; playerIndex <= _players.Count; playerIndex++)
            {
                LeanTween.alphaCanvas(_players[playerIndex].PlayerCanvas.GetComponent<CanvasGroup>(), 1, 0.3f);
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
            photonView.RPC(nameof(StartTimer), RpcTarget.All);
        }

        [PunRPC]
        public void StartTimer()
        {
            _timerIsActive = true;

            if (_timerView) _timerView.gameObject.SetActive(true);
        }

        public void PauseTime(float delay)
        {
            _timerIsActive = false;

            if (CurrentPlayerController.photonView.IsMine)
            {
                CancelInvoke(nameof(RPC_StartTimer));
                if (delay > 0) Invoke(nameof(RPC_StartTimer), delay);
            }
        }

        [PunRPC]
        public void TimeAlmostUp()
        {
            if (_timeAlmostUp == true) return;

            _timeAlmostUp = true;

            LeanTween.scale(_timerView.gameObject, Vector3.one * 1.1f, 0.5f).setLoopPingPong().setEaseInBack();
        }

        [PunRPC]
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

            if (CurrentPlayerController.photonView.IsMine)
            {
                CurrentPlayerController.photonView.RPC("SetMoves", RpcTarget.All, 0);
            }

            if (GridPlayerController) GridPlayerController.CancelExecuteLink();

            EndTurn();
        }

        public void EndTurn()
        {
            if (CurrentPlayerController.photonView.IsMine)
            {
                photonView.RPC(nameof(NextPlayer), RpcTarget.All);
            }
        }

        public void RPC_ResetRounds()
        {
            photonView.RPC(nameof(ResetRounds), RpcTarget.All);
        }

        public void ResetRounds()
        {
            _currentRound = 1;
            if (_roundsBarView) _roundsBarView.SetProgress(1);

            UpdateRounds();
        }

        public void NextRound()
        {
            _currentRound++;
            if (_roundsBarView) _roundsBarView.ChangeProgress(1);

            UpdateRounds();
        }


        public void UpdateRounds()
        {
            if (_currentRound > _rounds)
            {
                NetworkPlayerController winner = _players[1];

                for (int playerIndex = 1; playerIndex < _players.Count; playerIndex++)
                {
                    if (_scoreDataProxy.GetPlayerScore(playerIndex) > _scoreDataProxy.GetPlayerScore(1))
                    {
                        winner = _players[playerIndex];
                    }
                }

                // Check for tie
                int sameScore = 0;

                for (int playerIndex = 1; playerIndex < _players.Count; playerIndex++)
                {
                    if (_scoreDataProxy.GetPlayerScore(playerIndex) == _scoreDataProxy.GetPlayerScore(1))
                    {
                        sameScore++;
                    }
                }

                if (sameScore > 1)
                {
                    _rounds++;

                    _roundsText.SetText("TIEBREAKER!");
                    _currentRoundText.SetText("TIEBREAKER!");
                }
                else
                {
                    _winnerText.SetText(winner.PlayerName + " WINS!");

                    // FINISH MATCH
                    _gridController.ClearGrid();
                    _gridController.HideGrid();

                    Invoke(nameof(GameOver), 0.5f);
                }

            }
            else if (_currentRound == _rounds)
            {
                _roundsText.SetText("LAST ROUND!");
                _currentRoundText.SetText("LAST ROUND!");
            }
            else
            {
                _roundsText.SetText("ROUND " + _currentRound + "/" + _rounds);
                _currentRoundText.SetText("ROUND " + _currentRound);
            }
        }

        void RPC_GameOver()
        {
            photonView.RPC(nameof(GameOver), RpcTarget.All);
        }

        public void GameOver()
        {
            _gameOverScreen.SetActive(true);
            _restartButton.onClick.AddListener(Restart);

            _timerIsActive = false;

            NetworkPlayerController winner = _players[1];
            for (int playerIndex = 1; playerIndex <= _players.Count; playerIndex++)
            {
                if (_scoreDataProxy.GetPlayerScore(playerIndex) > _scoreDataProxy.GetPlayerScore(1))
                {
                    winner = _players[playerIndex];
                }
            }

            _winnerText.SetText(winner.PlayerName + " WINS!");

            _restartButton.onClick.AddListener(Restart);
        }

        public void Restart()
        {
            SceneManager.LoadScene("Menu");
        }

        public void SetRoundsPerMatch(int setValue)
        {
            _rounds = setValue;
        }

        public void SetMovesPerRound(int setValue)
        {
            _movesPerRound = setValue;
        }

        public void SetTimePerRound(float setValue)
        {
            _timePerRound = setValue;
        }

        public void SetExtraMoveAtLink(int setValue)
        {
            _extraMoveAtLink = setValue;
        }

        public int GetExtraMoveAtLink()
        {
            return _extraMoveAtLink;
        }

        public int GetSpecialLink(int index, int linkSize)
        {
            return _specialLinks[index].LinkSize;
        }

        public void SetSpecialLink(int index, int linkSize)
        {
            _specialLinks[index].LinkSize = linkSize;
        }

        public void AddNewPlayer(int index, NetworkPlayerController playerController)
        {
            if (!_players.ContainsKey(index))
            {
                _players.Add(index, playerController);
            }
        }

        [Serializable]
        public class SpecialLink
        {
            public int LinkSize = 4;
            public GridItemView SpawnItemView;
        }

    }
}