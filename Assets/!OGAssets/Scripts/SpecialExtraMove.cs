using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*Adds an extra move to the player,
used as a component on a GridItem*/
public class SpecialExtraMove : MonoBehaviour
{
    [SerializeField] private int changeValue = 1;
    [SerializeField] private string message = "EXTRA MOVE!";
    [SerializeField] private Color textColor = Color.white;

    public void Execute(ExecuteData executeData)
    {
        StartCoroutine(ExecutePatternCoroutine(executeData.gridTile, executeData.delay));
    }

    IEnumerator ExecutePatternCoroutine(GridTile gridTile, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (GameManager.instance.currentPlayer.photonView.IsMine)
        {
            GameManager.instance.currentPlayer.photonView.RPC("ChangeMoves", RpcTarget.All, changeValue);
        }

        //GameManager.instance.currentPlayer.ChangeMoves(changeValue);

        GameManager.instance.playerController.toast.SetToast(transform.position, message, textColor);
    }
}
