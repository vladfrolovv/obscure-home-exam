using Fusion;
using OGShared.Scripts.Gameplay.Grid;
using UnityEngine;
using UniRx;
namespace OGServer.Scripts.Gameplay
{
    public class GridServerController : SimulationBehaviour
    {

        [Networked] public int GridSeed { get; set; }
        [Networked] public Vector2Int GridSize { get; set; }

        private GridModel _gridModel;
        private bool _initialized;

        public override void FixedUpdateNetwork()
        {
            if (!Runner.IsServer) return;
            if (_initialized)
            {
                // ProcessClientRequests();
                return;
            }

            _initialized = true;

            GridSeed = Random.Range(int.MinValue, int.MaxValue);
            GridSize = new Vector2Int(7, 7);

            _gridModel = new GridModel(GridSize.x, GridSize.y, GridSeed);

            _gridModel.CellSpawned.Subscribe(delegate (CellSpawnModel cellSpawnModel)
            {
            });

            // _gridModel.FillGrid();
        }

    }

}
