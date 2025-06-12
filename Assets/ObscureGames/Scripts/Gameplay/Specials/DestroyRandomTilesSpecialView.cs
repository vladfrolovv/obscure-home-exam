using System.Collections;
using ObscureGames.Gameplay.Grid;
using UnityEngine;
namespace ObscureGames.Gameplay.Specials
{
    public class DestroyRandomTilesSpecialView : BaseSpecialView
    {

        [SerializeField] private int destroyCount = 20;
        [SerializeField] private LineRenderer glowLineRenderer;
        [SerializeField] private Transform sourceEffect;
        [SerializeField] private float executeDelay = 2;
        [SerializeField] private Transform destroyEffect;

        protected override IEnumerator ExecutePatternCoroutine(GridTileView gridTileView, float delay)
        {
            GameManager.instance.playerController.AddToExecuteList(this.gameObject);

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
                    /*ParticleSystem newEffect = Instantiate(glowEffect);

                newEffect.transform.position = targetTile.transform.position;

                Destroy(newEffect.gameObject, 2.0f);*/

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

                    GameManager.instance.playerController.CollectItemAtTile(targetTileView, executeDelay * 0.9f);

                    //GameManager.instance.currentPlayer.AddBonus(1, 0.5f);
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

            GameManager.instance.playerController.RemoveFromExecuteList(this.gameObject);
            GameManager.instance.playerController.CheckExecuteLink();
        }

    }
}
