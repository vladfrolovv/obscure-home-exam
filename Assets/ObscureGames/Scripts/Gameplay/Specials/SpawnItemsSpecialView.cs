using System.Collections;
using ObscureGames.Gameplay.Grid;
using UnityEngine;
namespace ObscureGames.Gameplay.Specials
{
    public class SpawnItemsSpecialView : BaseSpecialView
    {

        [SerializeField] private GridItemView[] spawns;
        [SerializeField] private bool autoTrigger = true;
        [SerializeField] private float triggerRate = 0.2f;
        [SerializeField] private bool spawnOnRandomTile = false;

        protected override IEnumerator ExecutePatternCoroutine(GridTileView gridTileView, float delay)
        {
            GameManager.Instance.PlayerController.AddToExecuteList(this.gameObject);

            yield return new WaitForSeconds(delay);

            for (int spawnIndex = 0; spawnIndex < spawns.Length; spawnIndex++)
            {
                if (spawnOnRandomTile == true)
                {
                    GridTileView targetTileView = GridController.GetRandomTile();
                    gridTileView = targetTileView;
                }

                if (gridTileView.GridItemView != null) GameManager.Instance.PlayerController.CollectItemAtTile(gridTileView, 0);

                GridController.SpawnItem(spawns[spawnIndex], gridTileView, 0);

                if (autoTrigger == true) GameManager.Instance.PlayerController.CollectItemAtTile(gridTileView, spawnIndex * triggerRate);
            }

            GameManager.Instance.PlayerController.RemoveFromExecuteList(this.gameObject);
            GameManager.Instance.PlayerController.CheckExecuteLink();
        }

    }
}
