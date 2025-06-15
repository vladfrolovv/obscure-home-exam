using System.Collections;
using OGClient.Gameplay.Grid;
using OGClient.Gameplay.Grid.Models;
using UnityEngine;
using Zenject;
namespace OGClient.Gameplay.Specials
{
    public abstract class BaseSpecialView : MonoBehaviour
    {

        protected GridController GridController;

        public virtual void Execute(ExecuteDataModel executeDataModel)
        {
            StartCoroutine(ExecutePatternCoroutine(executeDataModel.GridTileView, executeDataModel.Delay));
        }

        [Inject]
        public void Construct(GridController gridController)
        {
            GridController = gridController;
        }

        protected abstract IEnumerator ExecutePatternCoroutine(GridTileView gridTileView, float delay);

    }
}
