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

        public int CellSize { get; private set; }

        [Inject]
        public void Construct(GridController gridController)
        {
            _gridController = gridController;
        }

        public void SetGridSize(Vector2Int setValue)
        {
            if ( setValue.x > setValue.y )
            {
                _gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                _gridLayoutGroup.constraintCount = setValue.x;

                CellSize = (270 - setValue.x) / setValue.x;
            }
            else
            {
                _gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedRowCount;
                _gridLayoutGroup.constraintCount = setValue.y;

                CellSize = (270 - setValue.y) / setValue.y;
            }

            _gridLayoutGroup.cellSize = new Vector2Int(CellSize, CellSize);
        }

        public void ShowGrid()
        {
            _gridCanvas.enabled = true;
            _gridAnimator.Play(GridIntroAnimationParam);
        }

        public void HideGrid()
        {
            _gridCanvas.enabled = false;
        }

        public void ShakeBoard()
        {
            _gridAnimator.Play(GridShakeAnimationParam);
        }

    }
}
