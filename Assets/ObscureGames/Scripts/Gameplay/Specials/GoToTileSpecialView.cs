using System.Collections;
using ObscureGames.Gameplay.Grid;
using UnityEngine;
using Zenject;
namespace ObscureGames.Gameplay.Specials
{
    public class GoToTileSpecialView : BaseSpecialView
    {

        [SerializeField] private float moveTime = 1;
        [SerializeField] private GridItemView _carryItemView;

        private GameManager _gameManager;

        [Inject]
        public void Construct(GameManager gameManager)
        {
            _gameManager = gameManager;
        }

        protected override IEnumerator ExecutePatternCoroutine(GridTileView gridTileView, float delay)
        {
            GridItemView gridItemView = gridTileView.GridItemView;

            if (gridItemView == true)
            {
                _gameManager.GridPlayerController.AddToExecuteList(this.gameObject);

                gridItemView.IsClearing = true;

                if (gridItemView == true) LeanTween.cancel(gridItemView.gameObject);

                yield return new WaitForSeconds(delay);

                gridItemView.GridItemCanvas.sortingOrder *= 2;

                Vector3 randomOffset = UnityEngine.Random.insideUnitCircle * 0.5f;

                GridTileView targetTileView = GridController.GetPowerupTile();

                //if ( targetTile == null ) targetTile = GridManager.instance.GetBoosterTile();

                if (targetTileView == null)
                {
                    targetTileView = GridController.GetRandomTile();

                    int timeout = 20;

                    while (timeout > 0 && targetTileView.GridItemView == null)
                    {
                        targetTileView = GridController.GetRandomTile();

                        timeout--;
                    }
                }

                _gameManager.GridPlayerController.CollectItemAtTile(targetTileView, moveTime);

                gridItemView.TryToCollect();

                LeanTween.rotate(gridItemView.gameObject, Vector3.forward * UnityEngine.Random.Range(-30, 30), moveTime * 0.9f).setEaseOutSine();
                LeanTween.move(gridItemView.gameObject, gridItemView.transform.position + randomOffset, moveTime * 0.7f).setEaseOutSine().setOnComplete(() =>
                {
                    LeanTween.moveX(gridItemView.gameObject, targetTileView.transform.position.x, moveTime * 0.3f).setEaseInOutSine();
                    LeanTween.moveY(gridItemView.gameObject, targetTileView.transform.position.y, moveTime * 0.3f).setEaseInSine().setOnComplete(()=>
                    {
                        if (_carryItemView)
                        {
                            GridController.SpawnItem(_carryItemView, targetTileView, 0);
                            _gameManager.GridPlayerController.CollectItemAtTile(targetTileView, 0.0f);
                        }

                        gridItemView.TryToClear();
                        Destroy(gridItemView.gameObject);

                        _gameManager.GridPlayerController.RemoveFromExecuteList(this.gameObject);
                        _gameManager.GridPlayerController.CheckExecuteLink();
                    });
                });

            }
        }
    }
}
