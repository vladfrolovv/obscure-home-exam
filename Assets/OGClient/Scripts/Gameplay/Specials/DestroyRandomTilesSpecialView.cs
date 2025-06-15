using System.Collections;
using OGClient.Gameplay.Grid;
using UnityEngine;
using Zenject;
namespace OGClient.Gameplay.Specials
{
    public class DestroyRandomTilesSpecialView : BaseSpecialView
    {

        [SerializeField] private int destroyCount = 20;
        [SerializeField] private LineRenderer glowLineRenderer;
        [SerializeField] private Transform sourceEffect;
        [SerializeField] private float executeDelay = 2;
        [SerializeField] private Transform destroyEffect;

        private GameManager _gameManager;

        [Inject]
        public void Construct(GameManager gameManager)
        {
            _gameManager = gameManager;
        }

        protected override IEnumerator ExecutePatternCoroutine(GridTileView gridTileView, float delay)
        {
            _gameManager.GridPlayerController.AddToExecuteList(this.gameObject);

            GridItemView gridItemView = gridTileView.GridItemView;

            if (gridItemView) gridItemView.IsClearing = true;

            yield return new WaitForSeconds(delay);

            if (gridItemView) gridItemView.GridItemCanvas.sortingOrder *= 2;

            for (int destroyIndex = 0; destroyIndex < destroyCount; destroyIndex++)
            {
                GridTileView targetTileView = GridController.GetRandomTile();

                GridItemView targetItemView = targetTileView.GridItemView;

                if (targetItemView)
                {
                    LineRenderer newGlowLine = Instantiate(glowLineRenderer);

                    newGlowLine.SetPosition(0, gridTileView.transform.position);
                    newGlowLine.SetPosition(1, targetTileView.transform.position);

                    newGlowLine.startWidth = 0;

                    LeanTween.value(0, 0.3f, 0.4f).setDelay(Random.Range(0.0f, 0.8f)).setOnUpdate((float width) => newGlowLine.startWidth = width).setOnComplete(() =>
                    {
                        targetItemView.GridItemCanvas.overrideSorting = true;
                        targetItemView.PlayAnimation("LinkAdd");
                    });

                    Destroy(newGlowLine.gameObject, executeDelay);

                    _gameManager.GridPlayerController.CollectItemAtTile(targetTileView, executeDelay * 0.9f);
                }
            }

            if (sourceEffect) Instantiate(sourceEffect, gridTileView.transform.position, Quaternion.identity);

            yield return new WaitForSeconds(executeDelay);

            if (destroyEffect) Instantiate(destroyEffect, gridTileView.transform.position, Quaternion.identity);

            GridController.ShakeBoard();

            if (gridItemView)
            {
                gridItemView.TryToCollect();
                gridItemView.TryToClear();
                Destroy(gridItemView.gameObject);
            }

            _gameManager.GridPlayerController.RemoveFromExecuteList(this.gameObject);
            _gameManager.GridPlayerController.CheckExecuteLink();
        }

    }
}
