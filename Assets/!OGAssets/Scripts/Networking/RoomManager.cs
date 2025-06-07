using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using System.IO;

public class RoomManager : MonoBehaviourPunCallbacks, IInRoomCallbacks
{
    public static RoomManager Instance;
    private PhotonView PV;

    private void Awake()
    {
        if (!Instance)
        {
            Instance = this;
        }
        else
        {
            if (Instance != this)
            {
                Destroy(Instance.gameObject);
                Instance = this;
            }
        }

        DontDestroyOnLoad(gameObject);
        PV = GetComponent<PhotonView>();
    }

    public override void OnEnable()
    {
        base.OnEnable();
        PhotonNetwork.AddCallbackTarget(this);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        PhotonNetwork.RemoveCallbackTarget(this);
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        if (!PhotonNetwork.IsMasterClient)
            return;
        StartGame();
        /* photonPlayers = PhotonNetwork.PlayerList;
          playersInRoom = photonPlayers.Length;
         myNumberInRoom = playersInRoom;
         PhotonNetwork.NickName = myNumberInRoom.ToString();
         */
    }

    void StartGame()
    {
        PhotonNetwork.LoadLevel(1);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (scene.buildIndex == 1) // We're in the game scene
        {
            CreatePlayer();
            Debug.Log("Playerlist:");
            foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList)
            {
                Debug.Log(player);
                // do og magic on players
            }
        }
    }

    void CreatePlayer()
    {
        GameObject temp = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "NetworkPlayer"), Vector3.zero, Quaternion.identity);

    }
}
