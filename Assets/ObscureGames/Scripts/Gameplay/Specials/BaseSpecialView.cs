using System.Collections;
using ObscureGames.Gameplay.Grid;
using ObscureGames.Gameplay.Grid.Models;
using UnityEngine;
using Zenject;
namespace ObscureGames.Gameplay.Specials
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
