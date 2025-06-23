using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
namespace OGClient.Gameplay.Grid
{
    public class GridView : MonoBehaviour
    {

        private const string GridIntroAnimationParam = "Intro";
        private const string GridShakeAnimationParam = "Shake";

        [Header("View References")]
        [SerializeField] private Canvas _gridCanvas;
        [SerializeField] private GridLayoutGroup _gridLayoutGroup;

        [Header("Animations")]
        [SerializeField] private Animator _gridAnimator;

        private GridController _gridController;

        [Inject]
        public void Construct(GridController gridController)
        {
            _gridController = gridController;
        }

        public void ShakeBoard()
        {
            _gridAnimator.Play(GridShakeAnimationParam);
        }

        private void SetGridSize(Vector2Int setValue)
        {
            int cellSize = 0;
            if ( setValue.x > setValue.y )
            {
                _gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                _gridLayoutGroup.constraintCount = setValue.x;

                cellSize = (270 - setValue.x) / setValue.x;
            }
            else
            {
                _gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedRowCount;
                _gridLayoutGroup.constraintCount = setValue.y;

                cellSize = (270 - setValue.y) / setValue.y;
            }

            _gridController.Model.SetCellSize(cellSize);
            _gridLayoutGroup.cellSize = new Vector2Int(cellSize, cellSize);
        }

        private void ShowGrid()
        {
            _gridCanvas.enabled = true;
            _gridAnimator.Play(GridIntroAnimationParam);
        }

        private void HideGrid()
        {
            _gridCanvas.enabled = false;
        }

        private void Awake()
        {
            _gridController.GridInitialized.Subscribe(OnGridInitialized).AddTo(this);
            _gridController.GridCleared.Subscribe(OnGridCleared).AddTo(this);
        }

        private void OnGridInitialized(Unit unit)
        {
            ShowGrid();
            SetGridSize(_gridController.Model.GridSize);
        }

        private void OnGridCleared(Unit unit)
        {
            HideGrid();
        }

    }
}
