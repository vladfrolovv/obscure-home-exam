using System.Collections;
using ObscureGames.Gameplay.Grid;
using UnityEngine;
using Zenject;
namespace ObscureGames.Gameplay.Specials
{
    public class SpawnItemsSpecialView : BaseSpecialView
    {

        [SerializeField] private GridItemView[] spawns;
        [SerializeField] private bool autoTrigger = true;
        [SerializeField] private float triggerRate = 0.2f;
        [SerializeField] private bool spawnOnRandomTile = false;

        private GameManager _gameManager;

        [Inject]
        public void Construct(GameManager gameManager)
        {
            _gameManager = gameManager;
        }

        protected override IEnumerator ExecutePatternCoroutine(GridTileView gridTileView, float delay)
        {
            _gameManager.GridPlayerController.AddToExecuteList(this.gameObject);

            yield return new WaitForSeconds(delay);

            for (int spawnIndex = 0; spawnIndex < spawns.Length; spawnIndex++)
            {
                if (spawnOnRandomTile == true)
                {
                    GridTileView targetTileView = GridController.GetRandomTile();
                    gridTileView = targetTileView;
                }

                if (gridTileView.GridItemView != null) _gameManager.GridPlayerController.CollectItemAtTile(gridTileView, 0);

                GridController.SpawnItem(spawns[spawnIndex], gridTileView, 0);

                if (autoTrigger == true) _gameManager.GridPlayerController.CollectItemAtTile(gridTileView, spawnIndex * triggerRate);
            }

            _gameManager.GridPlayerController.RemoveFromExecuteList(this.gameObject);
            _gameManager.GridPlayerController.CheckExecuteLink();
        }

    }
}
