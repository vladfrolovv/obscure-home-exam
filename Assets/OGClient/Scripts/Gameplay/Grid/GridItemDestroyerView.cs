using System.Collections;
using OGClient.Gameplay.Grid.Models;
using OGClient.Gameplay.Timers;
using UnityEngine;
using Zenject;
namespace OGClient.Gameplay.Grid
{
    public class GridItemDestroyerView : MonoBehaviour
    {

        [SerializeField] private float _executeDelay;
        [SerializeField] private Transform _destroyEffect;
        [SerializeField] private DestroyPatternModel _destroyPatternModel;

        private GridController _gridController;
        private GridView _gridView;
        private GridLinksController _gridLinksController;
        private TimeController _timeController;

        private int _patternIndex;

        [Inject]
        public void Construct(GridController gridController, GridLinksController gridLinksController, TimeController timeController,
                              GridView gridView)
        {
            _gridController = gridController;
            _gridLinksController = gridLinksController;
            _timeController = timeController;
            _gridView = gridView;
        }

        public void Execute(ExecuteDataModel executeDataModel)
        {
            StartCoroutine(ExecutePatternCoroutine(executeDataModel.GridTileView, executeDataModel.Delay));
        }

        private IEnumerator ExecutePatternCoroutine(GridTileView gridTileView, float delay)
        {
            _gridLinksController.AddToExecuteList(gameObject);

            yield return new WaitForSeconds(delay + _executeDelay);

            _timeController.SlowMotion(0.2f, 0.1f);

            Vector2Int gridSize = _gridController.GridModel.GridSize;
            Vector2Int tileGridIndex = _gridLinksController.GetIndexInGrid(gridTileView);

            for (_patternIndex = 0; _patternIndex < _destroyPatternModel.Directions.Count; _patternIndex++)
            {
                int tileX = Mathf.Clamp(tileGridIndex.x + _destroyPatternModel.Directions[_patternIndex].x, 0, gridSize.x - 1);
                int tileY = Mathf.Clamp(tileGridIndex.y + _destroyPatternModel.Directions[_patternIndex].y, 0, gridSize.y - 1);

                // , _destroyPatternModel.Delay;
                _gridLinksController.CollectItemAtGrid(new Vector2Int(tileX, tileY));
            }

            yield return new WaitForSeconds(0.2f);

            if (_destroyEffect) Instantiate(_destroyEffect, gridTileView.transform.position, Quaternion.identity);

            _gridView.ShakeBoard();

            // _gridLinksController.RemoveFromExecuteList(this.gameObject);
            _gridLinksController.CheckExecuteLink();
        }
    }
}
