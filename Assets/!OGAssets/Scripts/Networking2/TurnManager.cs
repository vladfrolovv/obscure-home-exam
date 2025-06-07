using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using ExitGames.Client.Photon;
using UnityEngine.EventSystems;

public class TurnManager : MonoBehaviour, IOnEventCallback
{
    public static TurnManager Instance;
    public double turnDuration = 10.0;
    public double elapsedTime = 0f;
    double startTime;

    public string currentTurnId = "-1";

    public int playersCount = 2;

    public bool timerIsRunning;

    private Photon.Realtime.Player localPlayer;
    private Photon.Realtime.Player otherPlayer;
    private Photon.Realtime.Player activePlayer;

    ExitGames.Client.Photon.Hashtable CustomValue;

    public double timer = 10;
    public double timeLimit = 10;

    public GameObject lockCanvas;
    [SerializeField] private EventSystem eventSystem;
    private void Awake()
    {
        Application.targetFrameRate = 60;
        if (!Instance)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        Invoke("SetUpTurnManager", 2f);
    }
    public void SetUpTurnManager()
    {
        eventSystem = FindObjectOfType<EventSystem>();
        foreach (int playerId in PhotonNetwork.CurrentRoom.Players.Keys)
        {
            if (PhotonNetwork.CurrentRoom.Players[playerId].IsLocal)
            {
                SetLocalPlayer(PhotonNetwork.CurrentRoom.Players[playerId]);
            }
            else
            {
                otherPlayer = PhotonNetwork.CurrentRoom.Players[playerId];
            }

            Debug.Log(PhotonNetwork.CurrentRoom.Players[playerId].UserId);
        }

        PickRandomTurn();

        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            timer = timeLimit;
            ExitGames.Client.Photon.Hashtable ht = new ExitGames.Client.Photon.Hashtable() { { "Time", timer } };
            PhotonNetwork.CurrentRoom.SetCustomProperties(ht);
        } else
        {
            timer = (double)PhotonNetwork.CurrentRoom.CustomProperties["Time"];
        }

        timerIsRunning = true;
    }

    public void SetLocalPlayer(Photon.Realtime.Player player)
    {
        localPlayer = player;
    }

    public void SetOtherPlayer(Photon.Realtime.Player player)
    {
        otherPlayer = player;
    }
    public bool IsLocalPlayersTurn()
    {
        return localPlayer == activePlayer;
    }

    public bool CanPerformMove()
    {
        if (!IsLocalPlayersTurn())
            return false;
        return true;
    }

    public void PickRandomTurn()
    {
        int index = Random.Range(1, PhotonNetwork.CurrentRoom.Players.Count +1);

        SetTurn(PhotonNetwork.CurrentRoom.Players[index].UserId);
    }

    public void StartTimer()
    {
        timerIsRunning = true;
    }

    public void PauseTimer()
    {
        timerIsRunning = false;

    }
    public void ResetTimer()
    {
        timer = timeLimit;
        elapsedTime = 0f;
    }

    public void FinishTurn()
    {
        // CHECK IF LAST TURN AND END THE GAME
        SwitchTurn();
    }

    public void SwitchTurn()
    {
        ResetTimer();
        activePlayer = activePlayer == localPlayer ? otherPlayer : localPlayer;

        if (activePlayer != localPlayer)
        {
            lockCanvas.SetActive(true);
        }
        else
            lockCanvas.SetActive(false);
    }

    public void SetTurn(string id)
    {
        foreach(int index in PhotonNetwork.CurrentRoom.Players.Keys)
        {
            if(PhotonNetwork.CurrentRoom.Players[index].UserId == id)
            {
                currentTurnId = id;
                
                if(currentTurnId == localPlayer.UserId)
                {
                    activePlayer = localPlayer;
                }
                else
                {
                    activePlayer = otherPlayer;
                }
            }
        }
        if (activePlayer != localPlayer)
        {
            lockCanvas.SetActive(true);
        }
        else
            lockCanvas.SetActive(false);
    }

    public Dictionary<int, Photon.Realtime.Player> GetPlayersInCurrentRoom() =>
        PhotonNetwork.CurrentRoom.Players;

    public void OnEvent(EventData photonEvent)
    {
        throw new System.NotImplementedException();
    }
}
