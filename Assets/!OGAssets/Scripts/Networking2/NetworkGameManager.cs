using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
public class NetworkGameManager : MonoBehaviourPunCallbacks
{
    public NetworkPlayer MasterPlayer;
    public NetworkPlayer RemotePlayer;

    // Start is called before the first frame update
    void Start()
    {
        SpawnPlayers();
    }

    void SpawnPlayers()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            MasterPlayer = PhotonNetwork.Instantiate(MasterPlayer.name, new Vector3(0, 0, 0), Quaternion.identity).GetComponent<NetworkPlayer>();
        }
        else
        {
            RemotePlayer = PhotonNetwork.Instantiate(RemotePlayer.name, new Vector3(0, 0, 0), Quaternion.identity).GetComponent<NetworkPlayer>();
        }
    }
}
