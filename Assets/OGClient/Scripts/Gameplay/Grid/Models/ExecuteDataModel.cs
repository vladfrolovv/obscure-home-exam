namespace OGClient.Gameplay.Grid.Models
{
    public class ExecuteDataModel
    {

        public ExecuteDataModel(GridTileView gridTileView, float delay)
        {
            GridTileView = gridTileView;
            Delay = delay;
        }

        public GridTileView GridTileView { get; }
        public float Delay { get; }

    }

}