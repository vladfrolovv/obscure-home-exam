using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridItemDestroyer : MonoBehaviour
{
    public float executeDelay = 0.0f;

    [SerializeField] private Transform destroyEffect;

    [NonReorderable] public DestroyPattern destroyPattern;
    private int patternIndex = 0;


    public void Execute(ExecuteData executeData)
    {
        StartCoroutine(ExecutePatternCoroutine(executeData.gridTile, executeData.delay));
    }

    IEnumerator ExecutePatternCoroutine(GridTile gridTile, float delay)
    {
        GameManager.instance.playerController.AddToExecuteList(this.gameObject);

        yield return new WaitForSeconds(delay);

        yield return new WaitForSeconds(executeDelay);

        TimeManager.instance.SlowMotion(0.2f, 0.1f);

        Vector2Int gridSize = GridManager.instance.GetGridSize();

        Vector2Int tileGridIndex = GameManager.instance.playerController.GetIndexInGrid(gridTile);

        for ( patternIndex = 0; patternIndex < destroyPattern.directions.Length; patternIndex++ )
        {
            int tileX = Mathf.Clamp(tileGridIndex.x + destroyPattern.directions[patternIndex].x, 0, gridSize.x - 1);
            int tileY = Mathf.Clamp(tileGridIndex.y + destroyPattern.directions[patternIndex].y, 0, gridSize.y - 1);

            GameManager.instance.playerController.CollectItemAtGrid(tileX, tileY, destroyPattern.delay);
        }

        yield return new WaitForSeconds(0.2f);

        if (destroyEffect) Instantiate(destroyEffect, gridTile.transform.position, Quaternion.identity);

        GridManager.instance.ShakeBoard();

        GameManager.instance.playerController.RemoveFromExecuteList(this.gameObject);

        GameManager.instance.playerController.CheckExecuteLink();
    }
}
