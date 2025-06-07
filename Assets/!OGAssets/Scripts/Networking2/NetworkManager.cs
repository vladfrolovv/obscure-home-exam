using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;

public class NetworkManager : MonoBehaviourPunCallbacks
{

    public Button FindMatchButton;
    public TextMeshProUGUI status;

    bool isConnected = false;

    float retryAfter = 5f;

    float timer = 0f;

    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }
    // Start is called before the first frame update
    void Start()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            isConnected = true;
            FindMatchButton.interactable = true;
        } else
        {
            timer = 0;
            FindMatchButton.interactable = false;
            Connect();
        }
     
    }

    private void Update()
    {
        if (!isConnected)
        {
            if (timer < retryAfter)
            {
                timer += Time.deltaTime;
            }
            else
            {
                PhotonNetwork.Disconnect();
                Connect();
                timer = 0;
            }
        }
    }

    public void Connect()
    {
        try
        {
            PhotonNetwork.ConnectUsingSettings();
        } catch(UnityException e)
        {
            Connect();
        }
    }
    
    public void FindMatch()
    {
        FindMatchButton.gameObject.SetActive(false);
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnConnectedToMaster()
    {
        isConnected = true;
        FindMatchButton.interactable = true;
    }


    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 2});
    }

    public override void OnJoinedRoom()
    {
        status.SetText("Waiting For other player");
        PhotonNetwork.LocalPlayer.NickName = $"Guest{Random.Range(1000, 9999)}";
    }

    public override void OnJoinedLobby()
    {

        status.SetText("Joined lobby");
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        newPlayer.NickName = $"Guest{Random.Range(1000, 9999)}";
        //base.OnPlayerEnteredRoom(newPlayer);
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel(1);
        }
    }
}
