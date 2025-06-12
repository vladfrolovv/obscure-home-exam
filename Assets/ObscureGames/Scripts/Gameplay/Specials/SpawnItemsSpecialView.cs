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

        protected override IEnumerator ExecutePatternCoroutine(GridTile gridTile, float delay)
        {
            GameManager.instance.playerController.AddToExecuteList(this.gameObject);

            yield return new WaitForSeconds(delay);

            for (int spawnIndex = 0; spawnIndex < spawns.Length; spawnIndex++)
            {
                if (spawnOnRandomTile == true)
                {
                    GridTile targetTile = GridManager.instance.GetRandomTile();
                    gridTile = targetTile;
                }

                if (gridTile.GetCurrentItem() != null) GameManager.instance.playerController.CollectItemAtTile(gridTile, 0);

                GridManager.instance.SpawnItem(spawns[spawnIndex], gridTile, 0);

                if (autoTrigger == true) GameManager.instance.playerController.CollectItemAtTile(gridTile, spawnIndex * triggerRate);
            }

            GameManager.instance.playerController.RemoveFromExecuteList(this.gameObject);
            GameManager.instance.playerController.CheckExecuteLink();
        }

    }
}
