using System.Collections;
using OGClient.Gameplay.Grid;
using UnityEngine;
using Zenject;
namespace OGClient.Gameplay.Specials
{
    public class GoToTileSpecialView : BaseSpecialView
    {

        [SerializeField] private float moveTime = 1;
        [SerializeField] private GridItemView _carryItemView;

        private GridLinksController _gridLinksController;

        [Inject]
        public void Construct(GridLinksController gridLinksController)
        {
            _gridLinksController = gridLinksController;
        }

        protected override IEnumerator ExecutePatternCoroutine(GridTileView gridTileView, float delay)
        {
            GridItemView gridItemView = gridTileView.GridItemView;

            if (gridItemView == true)
            {
                _gridLinksController.AddToExecuteList(this.gameObject);

                gridItemView.IsClearing = true;

                if (gridItemView == true) LeanTween.cancel(gridItemView.gameObject);

                yield return new WaitForSeconds(delay);

                gridItemView.GridItemCanvas.sortingOrder *= 2;

                Vector3 randomOffset = UnityEngine.Random.insideUnitCircle * 0.5f;

                GridTileView targetTileView = GridController.GridModel.GetPowerupTile();

                //if ( targetTile == null ) targetTile = GridManager.instance.GetBoosterTile();

                if (targetTileView == null)
                {
                    targetTileView = GridController.GridModel.GetRandomTile();

                    int timeout = 20;

                    while (timeout > 0 && targetTileView.GridItemView == null)
                    {
                        targetTileView = GridController.GridModel.GetRandomTile();

                        timeout--;
                    }
                }

                _gridLinksController.CollectItemAtTile(targetTileView, moveTime);

                gridItemView.TryToCollect();

                LeanTween.rotate(gridItemView.gameObject, Vector3.forward * UnityEngine.Random.Range(-30, 30), moveTime * 0.9f).setEaseOutSine();
                LeanTween.move(gridItemView.gameObject, gridItemView.transform.position + randomOffset, moveTime * 0.7f).setEaseOutSine().setOnComplete(() =>
                {
                    LeanTween.moveX(gridItemView.gameObject, targetTileView.transform.position.x, moveTime * 0.3f).setEaseInOutSine();
                    LeanTween.moveY(gridItemView.gameObject, targetTileView.transform.position.y, moveTime * 0.3f).setEaseInSine().setOnComplete(()=>
                    {
                        if (_carryItemView)
                        {
                            GridController.CreateGridItem(_carryItemView, targetTileView, 0);
                            _gridLinksController.CollectItemAtTile(targetTileView, 0.0f);
                        }

                        gridItemView.TryToClear();
                        Destroy(gridItemView.gameObject);

                        // _gridLinksController.RemoveFromExecuteList(this.gameObject);
                        _gridLinksController.CheckExecuteLink();
                    });
                });

            }
        }
    }
}
