using System.Collections;
using OGClient.Gameplay.Grid;
using OGClient.Gameplay.UI;
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

        private ToastView _toastView;
        private GridLinksController _gridLinksController;

        [Inject]
        public void Construct(GridLinksController gridLinksController, ToastView toastView)
        {
            _gridLinksController = gridLinksController;
            _toastView = toastView;
        }

        protected override IEnumerator ExecutePatternCoroutine(GridTileView gridTileView, float delay)
        {
            yield return new WaitForSeconds(delay);

            // if (_gameManager.CurrentPlayerController.photonView.IsMine)
            // {
            //     _gameManager.CurrentPlayerController.photonView.RPC("ChangeMoves", RpcTarget.All, changeValue);
            // }

            _toastView.SetToast(transform.position, message, textColor);
        }

    }
}
