using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using ObscureGames.Debug;
using ObscureGames.Gameplay.Grid;
using ObscureGames.Gameplay.UI;
using ObscureGames.Players;
using Photon.Pun;
using Zenject;

namespace ObscureGames.Gameplay
{
    public class GameManager : MonoBehaviourPunCallbacks, IPunObservable
    {

        public static GameManager Instance;

        private readonly Dictionary<int, NetworkPlayer> _players = new Dictionary<int, NetworkPlayer>();

        [field: SerializeField, Header("Players")]
        public PlayerController PlayerController { get; private set; }

        [field: SerializeField]
        public NetworkPlayer CurrentPlayer { get; private set; }

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

        private GridController _gridController;
        private ScriptableGameplaySettings _gameplaySettings;

        public int PlayerIndex { get; private set; } = 1;
        public Camera MainCamera { get; private set; }

        public SpecialLink[] SpecialLinks => _specialLinks;

        [Inject]
        public void Construct(GridController gridController, ScriptableGameplaySettings gameplaySettings, Camera mainCamera)
        {
            _gridController = gridController;
            _gameplaySettings = gameplaySettings;
            MainCamera = mainCamera;
        }

        [Serializable]
        public class SpecialLink
        {
            public int linkSize = 4;
            public GridItemView SpawnItemView;
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

        public void Setup()
        {
            _gameOverScreen.SetActive(false);
            if (DebugSettings.instance) DebugSettings.instance.AssignSettings();

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

            if (PlayerController) PlayerController.LoseControl(0);

            //players = players.OrderBy(x => Random.value).ToList();
        }

        // Start is called before the first frame update
        void Start()
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
                if (CurrentPlayer.photonView.IsMine)
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
            CurrentPlayer = _players[PlayerIndex];

            HighlightPlayer();

            CurrentPlayer.SetMoves(_movesPerRound);
            if (CurrentPlayer.MovesBarView) CurrentPlayer.MovesBarView.SetProgress(_movesPerRound);

            if (_roundsBarView)
            {
                _roundsBarView.SetIncrementColor(CurrentPlayer.playerColor);
                _roundsBarView.Bounce();
            }

            if (_timerView)
            {
                _timerView.SetBarColor(CurrentPlayer.playerColor);
            }

            _playerTurnText.SetText(CurrentPlayer.playerName + "'S TURN!");
            _playerTurnAnimator.Play("Intro");

            _roundsText.SetText(CurrentPlayer.playerName + "'S TURN!");

            PlayerController.RegainControl();

            if (CurrentPlayer.photonView.IsMine)
            {
                ResetTime();
                Invoke(nameof(RPC_StartTimer), 0.5f);
            }
        }

        public void HighlightPlayer()
        {
            for (int playerIndex = 1; playerIndex <= _players.Count; playerIndex++)
            {
                if (_players[playerIndex] == CurrentPlayer) LeanTween.color(_players[playerIndex].avatarImage.rectTransform, Color.white, 0.5f);
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

            if (CurrentPlayer.photonView.IsMine)
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

            if (CurrentPlayer.photonView.IsMine)
            {
                CurrentPlayer.photonView.RPC("SetMoves", RpcTarget.All, 0);
            }

            if (PlayerController) PlayerController.CancelExecuteLink();

            EndTurn();
        }

        public void EndTurn()
        {
            if (CurrentPlayer.photonView.IsMine)
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
                NetworkPlayer winner = _players[1];

                for (int playerIndex = 1; playerIndex < _players.Count; playerIndex++)
                {
                    if (_players[playerIndex].score > winner.score)
                    {
                        winner = _players[playerIndex];
                    }
                }

                // Check for tie
                int sameScore = 0;

                for (int playerIndex = 1; playerIndex < _players.Count; playerIndex++)
                {
                    if (_players[playerIndex].score == winner.score)
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
                    _winnerText.SetText(winner.playerName + " WINS!");

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

            NetworkPlayer winner = _players[1];

            for (int playerIndex = 1; playerIndex <= _players.Count; playerIndex++)
            {
                if (_players[playerIndex].score > winner.score)
                {
                    winner = _players[playerIndex];
                }
            }

            _winnerText.SetText(winner.playerName + " WINS!");

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
            return _specialLinks[index].linkSize;
        }

        public void SetSpecialLink(int index, int linkSize)
        {
            _specialLinks[index].linkSize = linkSize;
        }

        public void AddNewPlayer(int index, NetworkPlayer player)
        {
            if (!_players.ContainsKey(index))
            {
                _players.Add(index, player);
            }
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(this.gameObject);
            }
        }

    }
}