using System;
using UniRx;
namespace OGShared.DataProxies
{
    public class MovesDataProxy
    {

        private readonly ReactiveDictionary<int, int> _moves = new();
        private readonly Subject<int> _currentPlayerId = new();
        private readonly Subject<int> _spendMoves = new();
        private readonly Subject<Unit> _resetMoves = new();

        public IReadOnlyReactiveDictionary<int, int> Moves => _moves;
        public IObservable<int> CurrentPlayerIdChanged => _currentPlayerId;
        public IObservable<int> SpendMoves => _spendMoves;
        public IObservable<Unit> ResetMoves => _resetMoves;

        public int CurrentPlayerId { get; private set; } = 0;

        public void SetCurrentTurnPlayer(int playerId)
        {
            CurrentPlayerId = playerId;
            _currentPlayerId.OnNext(playerId);
        }

        public void SetPlayerMoves(int playerId, int movesAmount) => _moves[playerId] = movesAmount;
        public int GetMovesLeft(int playerId) => _moves.TryGetValue(playerId, out var movesLeft) ? movesLeft : -1;
        public void RaiseSpendMoveRequest(int amount) => _spendMoves.OnNext(amount);
        public void RaiseResetMovesRequest() => _resetMoves.OnNext(Unit.Default);

    }
}
