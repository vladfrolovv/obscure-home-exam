namespace ObscureGames.Gameplay.Grid.Models
{
    public class ExecuteDataModel
    {

        public ExecuteDataModel(GridTile gridTile, float delay)
        {
            GridTile = gridTile;
            Delay = delay;
        }

        public GridTile GridTile { get; }
        public float Delay { get; }

    }

}