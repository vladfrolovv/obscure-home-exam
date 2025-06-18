using UniRx;
namespace OGClient.Gameplay.DataProxies
{
    public class MovesDataProxy
    {

        private readonly ReactiveDictionary<int, int> _moves = new ReactiveDictionary<int, int>();
        public IReadOnlyReactiveDictionary<int, int> Moves => _moves;

        public void SetPlayerMoves(int playerId, int movesAmount)
        {
            _moves[playerId] = movesAmount;
        }

        public bool TryDecreasePlayerMoves(int playerId)
        {
            if (_moves.TryGetValue(playerId, out int currentMoves) && currentMoves > 0)
            {
                _moves[playerId] = currentMoves - 1;
                return true;
            }

            return false;
        }

        public int GetMovesLeft(int playerId)
        {
            if (_moves.TryGetValue(playerId, out int movesLeft))
            {
                return movesLeft;
            }

            return -1;
        }

    }
}
