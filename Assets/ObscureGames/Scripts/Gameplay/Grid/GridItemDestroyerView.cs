using System.Collections;
using ObscureGames.Gameplay.Grid.Models;
using ObscureGames.Timers;
using UnityEngine;
using Zenject;
namespace ObscureGames.Gameplay.Grid
{
    public class GridItemDestroyerView : MonoBehaviour
    {

        [SerializeField] private float _executeDelay;
        [SerializeField] private Transform _destroyEffect;
        [SerializeField] private DestroyPatternModel _destroyPatternModel;

        private GridController _gridController;
        private GridPlayerController _gridPlayerController;
        private TimeController _timeController;

        private int _patternIndex;

        [Inject]
        public void Construct(GridController gridController, GridPlayerController gridPlayerController, TimeController timeController)
        {
            _gridController = gridController;
            _gridPlayerController = gridPlayerController;
            _timeController = timeController;
        }

        public void Execute(ExecuteDataModel executeDataModel)
        {
            StartCoroutine(ExecutePatternCoroutine(executeDataModel.GridTileView, executeDataModel.Delay));
        }

        private IEnumerator ExecutePatternCoroutine(GridTileView gridTileView, float delay)
        {
            _gridPlayerController.AddToExecuteList(gameObject);

            yield return new WaitForSeconds(delay + _executeDelay);

            _timeController.SlowMotion(0.2f, 0.1f);

            Vector2Int gridSize = _gridController.GridSize;
            Vector2Int tileGridIndex = _gridPlayerController.GetIndexInGrid(gridTileView);

            for (_patternIndex = 0; _patternIndex < _destroyPatternModel.Directions.Count; _patternIndex++)
            {
                int tileX = Mathf.Clamp(tileGridIndex.x + _destroyPatternModel.Directions[_patternIndex].x, 0, gridSize.x - 1);
                int tileY = Mathf.Clamp(tileGridIndex.y + _destroyPatternModel.Directions[_patternIndex].y, 0, gridSize.y - 1);

                _gridPlayerController.CollectItemAtGrid(tileX, tileY, _destroyPatternModel.Delay);
            }

            yield return new WaitForSeconds(0.2f);

            if (_destroyEffect) Instantiate(_destroyEffect, gridTileView.transform.position, Quaternion.identity);

            _gridController.ShakeBoard();

            _gridPlayerController.RemoveFromExecuteList(this.gameObject);
            _gridPlayerController.CheckExecuteLink();
        }
    }
}
