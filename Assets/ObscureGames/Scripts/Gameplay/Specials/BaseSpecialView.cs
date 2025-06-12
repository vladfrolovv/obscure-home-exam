using System.Collections;
using ObscureGames.Gameplay.Grid.Models;
using UnityEngine;
namespace ObscureGames.Gameplay.Specials
{
    public abstract class BaseSpecialView : MonoBehaviour
    {

        public virtual void Execute(ExecuteDataModel executeDataModel)
        {
            StartCoroutine(ExecutePatternCoroutine(executeDataModel.GridTile, executeDataModel.Delay));
        }

        protected abstract IEnumerator ExecutePatternCoroutine(GridTile gridTile, float delay);

    }
}
