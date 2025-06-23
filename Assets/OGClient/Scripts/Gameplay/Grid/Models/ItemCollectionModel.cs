namespace OGClient.Gameplay.Grid.Models
{
    public class ItemCollectionModel
    {

        public ItemCollectionModel(int x, int y, float executeTime)
        {
            X = x;
            Y = y;
            ExecuteTime = executeTime;
        }

        public int X { get; }
        public int Y { get; }
        public float ExecuteTime { get; }

    }
}
