using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialDestroyRandomTiles : MonoBehaviour
{
    [SerializeField] private int destroyCount = 20;
    [SerializeField] private LineRenderer glowLineRenderer;
    [SerializeField] private Transform sourceEffect;
    [SerializeField] private float executeDelay = 2;
    [SerializeField] private Transform destroyEffect;

    public void Execute(ExecuteData executeData)
    {
        StartCoroutine(ExecutePatternCoroutine(executeData.gridTile, executeData.delay));
    }

    IEnumerator ExecutePatternCoroutine(GridTile gridTile, float delay)
    {
        GameManager.instance.playerController.AddToExecuteList(this.gameObject);

        GridItem gridItem = gridTile.GetCurrentItem();

        if (gridItem) gridItem.isClearing = true;

        yield return new WaitForSeconds(delay);

        if (gridItem) gridItem.thisCanvas.sortingOrder *= 2;

        for ( int destroyIndex = 0; destroyIndex < destroyCount; destroyIndex++ )
        {
            GridTile targetTile = GridManager.instance.GetRandomTile();

            GridItem targetItem = targetTile.GetCurrentItem();

            if (targetItem)
            {
                /*ParticleSystem newEffect = Instantiate(glowEffect);

                newEffect.transform.position = targetTile.transform.position;

                Destroy(newEffect.gameObject, 2.0f);*/

                LineRenderer newGlowLine = Instantiate(glowLineRenderer);

                newGlowLine.SetPosition(0, gridTile.transform.position);
                newGlowLine.SetPosition(1, targetTile.transform.position);

                newGlowLine.startWidth = 0;

                LeanTween.value(0, 0.3f, 0.4f).setDelay(Random.Range(0.0f, 0.8f)).setOnUpdate((float width)=> newGlowLine.startWidth = width).setOnComplete(()=>
                {
                    targetItem.thisCanvas.overrideSorting = true;
                    targetItem.PlayAnimation("LinkAdd");
                });

                Destroy(newGlowLine.gameObject, executeDelay);
                
                GameManager.instance.playerController.CollectItemAtTile(targetTile, executeDelay * 0.9f);

                //GameManager.instance.currentPlayer.AddBonus(1, 0.5f);
            }
        }

        if (sourceEffect) Instantiate(sourceEffect, gridTile.transform.position, Quaternion.identity);

        yield return new WaitForSeconds(executeDelay);

        if (destroyEffect) Instantiate(destroyEffect, gridTile.transform.position, Quaternion.identity);

        GridManager.instance.ShakeBoard();

        if (gridItem)
        {
            GameManager.instance.playerController.CollectAnimation(gridItem);


            GameManager.instance.playerController.ClearEffect(gridItem);
            Destroy(gridItem.gameObject);
        }

        GameManager.instance.playerController.RemoveFromExecuteList(this.gameObject);

        GameManager.instance.playerController.CheckExecuteLink();
    }
}
