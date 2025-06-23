using System.Collections;
using OGClient.Gameplay.Grid;
using UnityEngine;
using Zenject;
namespace OGClient.Gameplay.Specials
{
    public class SpawnItemsSpecialView : BaseSpecialView
    {

        [SerializeField] private GridItemView[] spawns;
        [SerializeField] private bool autoTrigger = true;
        [SerializeField] private float triggerRate = 0.2f;
        [SerializeField] private bool spawnOnRandomTile = false;

        private GridLinksController _gridLinksController;

        [Inject]
        public void Construct(GridLinksController gridLinksController)
        {
            _gridLinksController = gridLinksController;
        }

        protected override IEnumerator ExecutePatternCoroutine(GridTileView gridTileView, float delay)
        {
            _gridLinksController.AddToExecuteList(this.gameObject);

            yield return new WaitForSeconds(delay);

            for (int spawnIndex = 0; spawnIndex < spawns.Length; spawnIndex++)
            {
                if (spawnOnRandomTile == true)
                {
                    GridTileView targetTileView = GridController.GridModel.GetRandomTile();
                    gridTileView = targetTileView;
                }

                if (gridTileView.GridItemView != null) _gridLinksController.CollectItemAtTile(gridTileView, 0);

                GridController.CreateGridItem(spawns[spawnIndex], gridTileView, 0);

                if (autoTrigger == true) _gridLinksController.CollectItemAtTile(gridTileView, spawnIndex * triggerRate);
            }

            _gridLinksController.RemoveFromExecuteList(this.gameObject);
            _gridLinksController.CheckExecuteLink();
        }

    }
}
