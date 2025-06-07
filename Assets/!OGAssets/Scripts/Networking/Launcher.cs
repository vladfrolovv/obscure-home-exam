using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;

public class Launcher : MonoBehaviourPunCallbacks
{
    public static Launcher Instance;

    private string gameVersion = "1";

    [SerializeField] private byte maxPlayersPerRoom = 2;

    private void Awake()
    {
        if (!Instance)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        Connect();
    }

    public void Connect()
    {
        if (!PhotonNetwork.IsConnected)
        {
            Debug.Log("Connecting to Master server...");
            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.GameVersion = gameVersion;
        }

    }

    public override void OnConnectedToMaster()
    {
        // Connect to the master server
        Debug.Log("Connected to Master server");
        Debug.Log("Joining lobby");
        PhotonNetwork.JoinLobby();
        PhotonNetwork.AutomaticallySyncScene = true;
    }
    public override void OnJoinedLobby()
    {
        // Join a lobby in master
        PhotonNetwork.NickName = $"Guest{Random.Range(1000, 9999)}";
        Debug.Log("Joined lobby");
    }

    public void FindMatch()
    {
        // Looking for opponent while in lobby
        Debug.Log("Finding match...");
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinedRoom()
    {
        // Room menu (Pre board scene)
        Debug.Log("Joined room");
        Photon.Realtime.Player[] players = PhotonNetwork.PlayerList;

        for (int i = 0; i < players.Count(); i++)
        {
            //Instantiate and set up player list item
        }
    }

    public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
    {
        // Start button visible to one side only?    
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        // If no room is found, create new one and wait for someone to randomly join
        Debug.Log("No random room found, creating new room");
        CreateRoom();
    }

    public void CreateRoom()
    {
        RoomOptions roomOps = new RoomOptions() { IsVisible = true, IsOpen = true, MaxPlayers = 2 };
        PhotonNetwork.CreateRoom($"Room{Random.Range(1000, 9999)}", roomOps);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        // If failed to create room, keep trying till it works
        Debug.Log("Room creation failed");
        PhotonNetwork.CreateRoom($"Room{Random.Range(1000, 9999)}", new RoomOptions { MaxPlayers = maxPlayersPerRoom });
    }

    public void LeaveRoom()
    {
        // Disconnect player from current match
        Debug.Log("leaving room");
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        // Remove the player clone from room list
        // check if he was the master
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        // Instantiate new player item clone and set up(name, avatar, level.. blabla)
    }
}
