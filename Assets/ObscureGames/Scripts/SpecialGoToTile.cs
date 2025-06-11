using System.Collections;
using System.Collections.Generic;
using ObscureGames.Gameplay.Grid;
using UnityEngine;

public class SpecialGoToTile : MonoBehaviour
{
    [SerializeField] private float moveTime = 1;
    [SerializeField] private GridItemView _carryItemView;

    public void Execute(ExecuteData executeData)
    {
        StartCoroutine(ExecutePatternCoroutine(executeData.gridTile, executeData.delay));
    }

    IEnumerator ExecutePatternCoroutine(GridTile gridTile, float delay)
    {
        GridItemView gridItemView = gridTile.GetCurrentItem();

        if (gridItemView == true)
        {
            GameManager.instance.playerController.AddToExecuteList(this.gameObject);

            gridItemView.IsClearing = true;

            if (gridItemView == true) LeanTween.cancel(gridItemView.gameObject);

            yield return new WaitForSeconds(delay);
       
            gridItemView.GridItemCanvas.sortingOrder *= 2;

            Vector3 randomOffset = UnityEngine.Random.insideUnitCircle * 0.5f;

            GridTile targetTile = GridManager.instance.GetPowerupTile();

            //if ( targetTile == null ) targetTile = GridManager.instance.GetBoosterTile();

            if (targetTile == null)
            {
                targetTile = GridManager.instance.GetRandomTile();

                int timeout = 20;

                while (timeout > 0 && targetTile.GetCurrentItem() == null)
                {
                    targetTile = GridManager.instance.GetRandomTile();

                    timeout--;
                }
            }

            GameManager.instance.playerController.CollectItemAtTile(targetTile, moveTime);

            gridItemView.TryToCollect();

            LeanTween.rotate(gridItemView.gameObject, Vector3.forward * UnityEngine.Random.Range(-30, 30), moveTime * 0.9f).setEaseOutSine();
            LeanTween.move(gridItemView.gameObject, gridItemView.transform.position + randomOffset, moveTime * 0.7f).setEaseOutSine().setOnComplete(() =>
            {
                LeanTween.moveX(gridItemView.gameObject, targetTile.transform.position.x, moveTime * 0.3f).setEaseInOutSine();
                LeanTween.moveY(gridItemView.gameObject, targetTile.transform.position.y, moveTime * 0.3f).setEaseInSine().setOnComplete(()=>
                {
                    if (_carryItemView)
                    {
                        GridManager.instance.SpawnItem(_carryItemView, targetTile, 0);
                        GameManager.instance.playerController.CollectItemAtTile(targetTile, 0.0f);
                    }

                    gridItemView.TryToClear();
                    Destroy(gridItemView.gameObject);

                    GameManager.instance.playerController.RemoveFromExecuteList(this.gameObject);
                    GameManager.instance.playerController.CheckExecuteLink();
                });
            });

        }
    }
}
