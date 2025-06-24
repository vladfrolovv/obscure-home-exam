using System;
using UniRx;
namespace OGShared.DataProxies
{
    public class MovesDataProxy
    {

        private readonly ReactiveDictionary<int, int> _moves = new();
        private readonly Subject<int> _currentPlayerId = new();
        private readonly Subject<int> _localPlayerWantsToSpendMoves = new();
        private readonly Subject<Unit> _localPlayerWantsToResetMoves = new();

        public IReadOnlyReactiveDictionary<int, int> Moves => _moves;
        public IObservable<int> CurrentPlayerIdChanged => _currentPlayerId;

        public IObservable<int> LocalPlayerWantsToSpendMoves => _localPlayerWantsToSpendMoves;
        public IObservable<Unit> LocalPlayerWantsToResetMoves => _localPlayerWantsToResetMoves;

        public int CurrentPlayerId { get; private set; } = 0;

        public void SetPlayerMoves(int playerId, int movesAmount)
        {
            _moves[playerId] = movesAmount;
        }

        public void SetCurrentTurnPlayer(int playerId)
        {
            CurrentPlayerId = playerId;
            _currentPlayerId.OnNext(playerId);
        }

        public int GetMovesLeft(int playerId)
        {
            return _moves.TryGetValue(playerId, out var movesLeft) ? movesLeft : -1;
        }

        public void TriggerSpendMoveRequest(int amount)
        {
            _localPlayerWantsToSpendMoves.OnNext(amount);
        }

        public void TriggerResetMovesRequest()
        {
            _localPlayerWantsToResetMoves?.OnNext(Unit.Default);
        }

    }
}
