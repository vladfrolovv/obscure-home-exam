using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Linq;
using Photon.Pun;

public class GameManager : MonoBehaviourPunCallbacks, IPunObservable
{
    public static GameManager instance;

    public float startDelay = 3;

    public PlayerController playerController;

    [SerializeField] internal Dictionary<int,NetworkPlayer> players = new Dictionary<int, NetworkPlayer>();
    public NetworkPlayer currentPlayer;
    [SerializeField] internal int playerIndex = 1;

    [SerializeField] private int movesPerRound = 2;
    
    [SerializeField] private int extraMoveAtLink = 15;

    [SerializeField] private float timePerRound = 20;
    private float timeLeft;

    [SerializeField] private ProgressBar timer;
    private bool timerRunning = false;
    private bool timeAlmostUp = false;

    [SerializeField] private Animator playerTurnAnimator;
    [SerializeField] private TextMeshProUGUI playerTurnText;

    [SerializeField] private TextMeshProUGUI roundsText;
    [SerializeField] private ProgressBar roundsBar;
    [SerializeField] private TextMeshProUGUI currentRoundText;
    [SerializeField] private int rounds = 5;
    [SerializeField] private int currentRound;

    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private TextMeshProUGUI winnerText;
    [SerializeField] private Button restartButton;

    [SerializeField] public SpecialLink[] specialLinks;

    public Camera MainCamera;

    [System.Serializable]
    public class SpecialLink
    {
        public int linkSize = 4;
        public GridItem spawnItem;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(startDelay);
            stream.SendNext(playerIndex);
            stream.SendNext(movesPerRound);
            stream.SendNext(extraMoveAtLink);
            stream.SendNext(timePerRound);
            stream.SendNext(timeLeft);
            stream.SendNext(rounds);
            stream.SendNext(currentRound);
        }
        else
        {
            startDelay = (float)stream.ReceiveNext();
            playerIndex = (int)stream.ReceiveNext();
            movesPerRound = (int)stream.ReceiveNext();
            extraMoveAtLink = (int)stream.ReceiveNext();
            timePerRound = (float)stream.ReceiveNext();
            timeLeft = (float)stream.ReceiveNext();
            rounds = (int)stream.ReceiveNext();
            currentRound = (int)stream.ReceiveNext();
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public void Setup()
    {
        gameOverScreen.SetActive(false);
        print("DebugSettings.instance " + DebugSettings.instance);
        if (DebugSettings.instance) DebugSettings.instance.AssignSettings();

        roundsText.SetText("");

        if (roundsBar)
        {
            roundsBar.SetProgress(0);
            roundsBar.SetProgressMax(rounds);
            roundsBar.Setup(null);
        }

        if ( timer )
        {
            timer.SetProgress(timePerRound);
            timer.SetProgressMax(timePerRound);
            timer.Setup(null);
            timer.gameObject.SetActive(false);
        }

        if (playerController) playerController.LoseControl(0);

        //players = players.OrderBy(x => Random.value).ToList();
    }

    // Start is called before the first frame update
    void Start()
    {
        Setup();

        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            Invoke(nameof(RPC_StartMatch), startDelay * 0.2f);
        }
    }

    private void Update()
    {
       if (timeLeft > 0)
        {
            if (timerRunning) timeLeft -= Time.deltaTime;

            if ( timer )
            {
                timer.SetProgress(timeLeft);
                timer.UpdateProgress(0);
            }

            if (timeLeft <= 10)
            {
                photonView.RPC(nameof(TimeAlmostUp),RpcTarget.All);
            }
        }
        else if (timerRunning)
        {
            if (currentPlayer.photonView.IsMine)
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
        //GridManager.instance.CreateGrid();
        GridManager.instance.FillGrid();
        GridManager.instance.ShowGrid();

        //Invoke(nameof(ShowPlayers), startDelay * 0.5f);

        Invoke(nameof(ResetRounds), startDelay * 1.0f);
        Invoke(nameof(SetCurrentPlayer), startDelay * 1.0f);
    }

    public void RPC_NextPlayer()
    {
        photonView.RPC(nameof(NextPlayer), RpcTarget.All);
    }

    [PunRPC]
    public void NextPlayer()
    {
        if (playerIndex < players.Count)
        {
            playerIndex++;
        }
        else
        {
            playerIndex = 1;
            NextRound();
        }

        if (currentRound <= rounds) SetCurrentPlayer();

        ResetTime();
        timer.ChangeProgress(1000);
    }


    void RPC_SetCurrentPlayer()
    {
        photonView.RPC(nameof(SetCurrentPlayer), RpcTarget.All);
    }

    public void SetCurrentPlayer()
    {
        currentPlayer = players[playerIndex];

        HighlightPlayer();

        //if (currentPlayer.photonView.IsMine){
        currentPlayer.SetMoves(movesPerRound);
        if (currentPlayer.movesBar) currentPlayer.movesBar.SetProgress(movesPerRound);

        if ( roundsBar )
        {
            roundsBar.SetIncrementColor(currentPlayer.playerColor);
            roundsBar.Bounce();
        }

        if ( timer )
        {
            timer.SetBarColor(currentPlayer.playerColor);
        }

        playerTurnText.SetText(currentPlayer.playerName + "'S TURN!");
        playerTurnAnimator.Play("Intro");

        roundsText.SetText(currentPlayer.playerName + "'S TURN!");
        
        playerController.RegainControl();

        if (currentPlayer.photonView.IsMine)
        {
			ResetTime();
            Invoke(nameof(RPC_StartTimer), 0.5f);
        }
    }

    public void HighlightPlayer()
    {
        for ( int playerIndex = 1; playerIndex <= players.Count; playerIndex++ )
        {
            if ( players[playerIndex] == currentPlayer ) LeanTween.color(players[playerIndex].avatarImage.rectTransform, Color.white, 0.5f);
            else LeanTween.color(players[playerIndex].avatarImage.rectTransform, Color.gray, 0.5f);
        }
    }

    public void HidePlayers()
    {
        for (int playerIndex = 1; playerIndex <= players.Count; playerIndex++)
        {
            //players[playerIndex].PlayerCanvas.GetComponent<CanvasGroup>().alpha = 0;
        }
    }

    public void ShowPlayers()
    {
        for (int playerIndex = 1; playerIndex <= players.Count; playerIndex++)
        {
            LeanTween.alphaCanvas(players[playerIndex].PlayerCanvas.GetComponent<CanvasGroup>(), 1, 0.3f);
        }
    }


    public void ResetTime()
    {
        timeLeft = timePerRound;
        timeAlmostUp = false;

        if (timer) timer.Bounce();

        LeanTween.cancel(timer.gameObject);
    }


    public void RPC_StartTimer()
    {
        photonView.RPC(nameof(StartTimer), RpcTarget.All);
    }

    [PunRPC]
    public void StartTimer()
    {
        timerRunning = true;

        if (timer) timer.gameObject.SetActive(true);
    }

    public void PauseTime( float delay )
    {
        timerRunning = false;

        if (currentPlayer.photonView.IsMine)
        {
            CancelInvoke(nameof(RPC_StartTimer));
            if (delay > 0) Invoke(nameof(RPC_StartTimer), delay);
        }
    }

    [PunRPC]
    public void TimeAlmostUp()
    {
        if (timeAlmostUp == true) return;

        timeAlmostUp = true;

        LeanTween.scale(timer.gameObject, Vector3.one * 1.1f, 0.5f).setLoopPingPong().setEaseInBack();
    }

    [PunRPC]
    public void TimeUp()
    {
        if (timerRunning == false) return;

        timerRunning = false;

        timeLeft = 0;

        if (timer)
        {
            LeanTween.scale(timer.gameObject, Vector3.one * 1, 0.3f).setEaseInBack();

            timer.Shake();
        }

        if (currentPlayer.photonView.IsMine)
        {
            currentPlayer.photonView.RPC("SetMoves",RpcTarget.All,0);
        }

        if (playerController) playerController.CancelExecuteLink();

        EndTurn();
    }

    public void EndTurn()
    {
        if (currentPlayer.photonView.IsMine)
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
        currentRound = 1;
        if ( roundsBar ) roundsBar.SetProgress(1);
        
        UpdateRounds();
    }

    public void NextRound()
    {
        currentRound++;
        if (roundsBar) roundsBar.ChangeProgress(1);

        UpdateRounds();
    }


    public void UpdateRounds()
    {
        if (currentRound > rounds)
        {
            NetworkPlayer winner = players[1];

            for (int playerIndex = 1; playerIndex < players.Count; playerIndex++)
            {
                if (players[playerIndex].score > winner.score)
                {
                    winner = players[playerIndex];
                }
            }

            // Check for tie
            int sameScore = 0;

            for (int playerIndex = 1; playerIndex < players.Count; playerIndex++)
            {
                if (players[playerIndex].score == winner.score)
                {
                    sameScore++;
                }
            }

            if (sameScore > 1)
            {
                rounds++;

                roundsText.SetText("TIEBREAKER!");
                currentRoundText.SetText("TIEBREAKER!");
            }
            else
            {
                winnerText.SetText(winner.playerName + " WINS!");

                // FINISH MATCH
                GridManager.instance.ClearGrid();
                GridManager.instance.HideGrid();

                Invoke(nameof(GameOver), 0.5f);
            }

        }
        else if (currentRound == rounds)
        {
            roundsText.SetText("LAST ROUND!");
            currentRoundText.SetText("LAST ROUND!");
        }
        else
        {
            roundsText.SetText("ROUND " + currentRound + "/" + rounds);
            currentRoundText.SetText("ROUND " + currentRound);
        }
    }

    void RPC_GameOver()
    {
        photonView.RPC(nameof(GameOver), RpcTarget.All);
    }

    public void GameOver()
    {
        gameOverScreen.SetActive(true);
        restartButton.onClick.AddListener(Restart);

        timerRunning = false;

        NetworkPlayer winner = players[1];

        for ( int playerIndex = 1; playerIndex <= players.Count; playerIndex++ )
        {
            if (players[playerIndex].score > winner.score )
            {
                winner = players[playerIndex];
            }
        }

        winnerText.SetText(winner.playerName + " WINS!");

        restartButton.onClick.AddListener(Restart);
    }

    public void Restart()
    {
        SceneManager.LoadScene("Menu");
    }

    public void SetRoundsPerMatch(int setValue)
    {
        rounds = setValue;
    }

    public void SetMovesPerRound(int setValue)
    {
        movesPerRound = setValue;
    }
    
    public void SetTimePerRound(float setValue)
    {
        timePerRound = setValue;
    }

    public void SetExtraMoveAtLink(int setValue)
    {
        extraMoveAtLink = setValue;
    }

    public int GetExtraMoveAtLink()
    {
        return extraMoveAtLink;
    }

    public int GetSpecialLink(int index, int linkSize)
    {
        return specialLinks[index].linkSize;
    }

    public void SetSpecialLink(int index, int linkSize)
    {
        specialLinks[index].linkSize = linkSize;
    }

}
