using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;

public class NetworkPlayer : MonoBehaviourPunCallbacks, IPunObservable
{
    public ScriptablePlayerProfile playerProfile;

    public string playerName = "Player 1";
    public Color playerColor = Color.blue;

    public Sprite RedCharacterIcon;
    public int playerIndex;

    public int score = 0;
    public int bonus = 0;
    public float bonusDelay = 0;

    public int moves = 0;

    public TextMeshProUGUI nameText;
    public Image avatarImage;

    public TextMeshProUGUI scoreText;

    public TextMeshProUGUI bonusText;
    public Animator bonusAnimator;

    public TextMeshProUGUI movesText;
    public Animator movesAnimator;
    public ProgressBar movesBar;

    //public Booster booster;

    public GameObject PlayerCanvas;

    public bool canControl = false;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        //sync score
        if (stream.IsWriting)
        {
            stream.SendNext(score);
            stream.SendNext(playerName);
            stream.SendNext(bonus);
            stream.SendNext(bonusDelay);
            stream.SendNext(moves);
            stream.SendNext(canControl);
            stream.SendNext(playerIndex);
        }
        else
        {
            score = (int)stream.ReceiveNext();
            playerName = (string)stream.ReceiveNext();
            nameText.SetText(playerName);
            bonus = (int)stream.ReceiveNext();
            bonusDelay = (float)stream.ReceiveNext();
            moves = (int)stream.ReceiveNext();
            movesText.SetText(moves.ToString());
            canControl = (bool)stream.ReceiveNext();
            playerIndex = (int)stream.ReceiveNext();
        }
    }

    void Awake()
    {
        GameManager.instance.players.Add(photonView.OwnerActorNr, this);
        PlayerCanvas.GetComponent<Canvas>().worldCamera = GameManager.instance.MainCamera;
        playerName = PhotonNetwork.LocalPlayer.NickName;
        if (photonView.IsMine)
        {
            photonView.RPC(nameof(RPC_Setup), RpcTarget.All);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine)
        {
            if (bonusDelay > 0) bonusDelay -= Time.deltaTime;
            else
            {
                if (bonus > 0)
                {
                    photonView.RPC(nameof(RemoveBonus), RpcTarget.All, 1);
                    photonView.RPC(nameof(RPC_ChangeScore), RpcTarget.All,1);
                }
            }
        }
    }

    [PunRPC]
    public void RPC_Setup()
    {
        nameText.SetText(playerName);

        if (playerProfile)
        {
            avatarImage.sprite = playerProfile.avatarIcon;
            //booster.SetBoosterProfile(playerProfile.booster);
        }

        playerIndex = photonView.OwnerActorNr;

        photonView.RPC(nameof(SetScore), RpcTarget.All, score);

        photonView.RPC(nameof(SetMoves), RpcTarget.All, moves);
        movesBar.SetProgressMax(moves);
        movesBar.Setup(this);

        photonView.RPC(nameof(SetBonus), RpcTarget.All, 0);

        //booster.SetItemType(playerIndex - 1);
        //booster.SetItemTypeIcon(GridManager.instance.itemTypes[playerIndex - 1]);
    }

    [PunRPC]
    void RPC_AddBonus(int addBonus, float setDelay)
    {
        bonus += addBonus;
        photonView.RPC(nameof(UpdateBonus), RpcTarget.All);

        bonusAnimator.Play("Bounce");
        bonusAnimator.Play("Bounce2");

        bonusDelay = setDelay;
    }

    public void AddBonus(int addBonus, float setDelay)
    {
        photonView.RPC(nameof(RPC_AddBonus), RpcTarget.All, addBonus, setDelay);
    }

    [PunRPC]
    public void RemoveBonus(int removeBonus)
    {
        bonus -= removeBonus;
        photonView.RPC(nameof(UpdateBonus), RpcTarget.All);
    }

    [PunRPC]
    void RPC_ChangeScore(int value)
    {
        score += value;
        photonView.RPC(nameof(UpdateScore),RpcTarget.All);
    }

    public void ChangeScore(int changeValue)
    {
        score += changeValue;

        photonView.RPC(nameof(UpdateScore), RpcTarget.All);
    }

    [PunRPC]
    public void SetScore(int setValue)
    {
        score = setValue;

        photonView.RPC(nameof(UpdateScore), RpcTarget.All);
    }

    [PunRPC]
    public void UpdateScore()
    {
        scoreText.SetText(score.ToString("000"));
    }

    [PunRPC]
    public void SetBonus(int setValue)
    {
        bonus = setValue;

        photonView.RPC(nameof(UpdateBonus), RpcTarget.All);
    }

    [PunRPC]
    public void UpdateBonus()
    {
        bonusText.gameObject.SetActive(bonus > 0);

        bonusText.SetText("+" + bonus.ToString());
    }

    [PunRPC]
    public void SetMoves(int setValue)
    {
        moves = setValue;
        photonView.RPC(nameof(UpdateMoves), RpcTarget.All);
    }

    [PunRPC]
    public void ChangeMoves(int changeValue)
    {
        moves += changeValue;

        if (movesBar) movesBar.ChangeProgress(changeValue);

        photonView.RPC(nameof(UpdateMoves), RpcTarget.All);
    }

    [PunRPC]
    public void UpdateMoves()
    {
        movesText.SetText(moves.ToString());

        movesAnimator.Play("Bounce");
        movesAnimator.Play("Bounce2");
    }
}
