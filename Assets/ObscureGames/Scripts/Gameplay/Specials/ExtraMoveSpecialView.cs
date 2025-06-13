using Photon.Pun;
using System.Collections;
using ObscureGames.Gameplay.Grid;
using UnityEngine;

/*Adds an extra move to the player,
used as a component on a GridItem*/
namespace ObscureGames.Gameplay.Specials
{
    public class ExtraMoveSpecialView : BaseSpecialView
    {

        [SerializeField] private int changeValue = 1;
        [SerializeField] private string message = "EXTRA MOVE!";
        [SerializeField] private Color textColor = Color.white;

        protected override IEnumerator ExecutePatternCoroutine(GridTileView gridTileView, float delay)
        {
            yield return new WaitForSeconds(delay);

            if (GameManager.Instance.CurrentPlayer.photonView.IsMine)
            {
                GameManager.Instance.CurrentPlayer.photonView.RPC("ChangeMoves", RpcTarget.All, changeValue);
            }

            //GameManager.instance.currentPlayer.ChangeMoves(changeValue);

            GameManager.Instance.PlayerController.ToastView.SetToast(transform.position, message, textColor);
        }

    }
}
