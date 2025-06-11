using System.Collections;
using ObscureGames.Gameplay.Grid.Configs;
using ObscureGames.Timers;
using UnityEngine;
using Zenject;
namespace ObscureGames.Gameplay.Grid
{
    public class GridItemDestroyerView : MonoBehaviour
    {

        [SerializeField] private float _executeDelay;
        [SerializeField] private Transform _destroyEffect;
        [SerializeField] private DestroyPatternInfo _destroyPatternInfo;

        private GridManager _gridManager;
        private PlayerController _playerController;
        private TimeController _timeController;

        private int _patternIndex;

        [Inject]
        public void Construct(GridManager gridManager, PlayerController playerController, TimeController timeController)
        {
            _gridManager = gridManager;
            _playerController = playerController;
            _timeController = timeController;
        }

        public void Execute(ExecuteData executeData)
        {
            StartCoroutine(ExecutePatternCoroutine(executeData.gridTile, executeData.delay));
        }

        private IEnumerator ExecutePatternCoroutine(GridTile gridTile, float delay)
        {
            _playerController.AddToExecuteList(gameObject);

            yield return new WaitForSeconds(delay + _executeDelay);

            _timeController.SlowMotion(0.2f, 0.1f);

            Vector2Int gridSize = _gridManager.GetGridSize();
            Vector2Int tileGridIndex = _playerController.GetIndexInGrid(gridTile);

            for (_patternIndex = 0; _patternIndex < _destroyPatternInfo.Directions.Count; _patternIndex++)
            {
                int tileX = Mathf.Clamp(tileGridIndex.x + _destroyPatternInfo.Directions[_patternIndex].x, 0, gridSize.x - 1);
                int tileY = Mathf.Clamp(tileGridIndex.y + _destroyPatternInfo.Directions[_patternIndex].y, 0, gridSize.y - 1);

                _playerController.CollectItemAtGrid(tileX, tileY, _destroyPatternInfo.Delay);
            }

            yield return new WaitForSeconds(0.2f);

            if (_destroyEffect) Instantiate(_destroyEffect, gridTile.transform.position, Quaternion.identity);

            _gridManager.ShakeBoard();

            _playerController.RemoveFromExecuteList(this.gameObject);
            _playerController.CheckExecuteLink();
        }
    }
}
