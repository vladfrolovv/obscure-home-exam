using Photon.Pun;
using System.Collections;
using OGClient.Gameplay.Grid;
using UnityEngine;
using Zenject;

/*Adds an extra move to the player,
used as a component on a GridItem*/
namespace OGClient.Gameplay.Specials
{
    public class ExtraMoveSpecialView : BaseSpecialView
    {

        [SerializeField] private int changeValue = 1;
        [SerializeField] private string message = "EXTRA MOVE!";
        [SerializeField] private Color textColor = Color.white;

        private GameManager _gameManager;

        [Inject]
        public void Construct(GameManager gameManager)
        {
            _gameManager = gameManager;
        }

        protected override IEnumerator ExecutePatternCoroutine(GridTileView gridTileView, float delay)
        {
            yield return new WaitForSeconds(delay);

            if (_gameManager.CurrentPlayerController.photonView.IsMine)
            {
                _gameManager.CurrentPlayerController.photonView.RPC("ChangeMoves", RpcTarget.All, changeValue);
            }

            _gameManager.GridPlayerController.ToastView.SetToast(transform.position, message, textColor);
        }

    }
}
