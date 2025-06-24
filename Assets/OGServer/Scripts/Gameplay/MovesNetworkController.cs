using UniRx;
using Fusion;
using Zenject;
using OGShared;
using OGShared.Gameplay;
using OGShared.DataProxies;
using UnityEngine;
namespace OGServer.Gameplay
{
    public class MovesNetworkController : NetworkBehaviour
    {

        [Networked, Capacity(BaseConstants.PLAYERS_PER_MATCH)]
        private NetworkDictionary<int, int> Moves => default;
        [Networked] private int CurrentPlayerId { get; set; }

        private MovesDataProxy _movesDataProxy;
        private ScriptableGameplaySettings _gameplaySettings;

        [Inject]
        public void Construct(MovesDataProxy movesDataProxy, ScriptableGameplaySettings gameplaySettings)
        {
            _movesDataProxy = movesDataProxy;
            _gameplaySettings = gameplaySettings;
        }

        public override void Spawned()
        {
            if (!Object.HasStateAuthority) return;

            StartPlayerTurn(GetFirstPlayerOfMatch());

            Debug.Log($"[SERVER] is trying to initialize moves controller with First Player[{CurrentPlayerId}] "
                      + $"Standard Moves amount [{_gameplaySettings.MovesPerTurn}]");

            _movesDataProxy.LocalPlayerWantsToSpendMoves.Subscribe(RPC_RequestTryToSpendMove).AddTo(this);
            _movesDataProxy.LocalPlayerWantsToResetMoves.Subscribe(_ => RPC_RequestTryToResetMoves()).AddTo(this);
        }
        
        private void AdvanceTurn()
        {
            if (!Object.HasStateAuthority) return;

            Debug.Log($"Turning to next player. Current Player: {CurrentPlayerId}");
            StartPlayerTurn(CurrentPlayerId == 1 ? 0 : 1);
        }

        private void StartPlayerTurn(int playerId)
        {
            if (!Object.HasStateAuthority) return;

            CurrentPlayerId = playerId;
            Moves.Add(CurrentPlayerId, _gameplaySettings.MovesPerTurn);

            RPC_BroadcastUpdateMovesDataProxy();
            RPC_BroadcastPlayerTurnStarted(CurrentPlayerId);
        }

        [Rpc(RpcSources.Proxies, RpcTargets.StateAuthority)]
        private void RPC_RequestTryToSpendMove(int moves)
        {
            if (!Object.HasStateAuthority) return;
            int playerId = Object.InputAuthority.PlayerId;
            Debug.Log($"[SERVER] trying to spend move for {playerId}");

            if (playerId != CurrentPlayerId) return;
            if (!Moves.TryGet(playerId, out int movesLeft) || movesLeft <= 0) return;

            int spendMoves = moves == 0 ? -Moves[playerId] : moves;
            Moves.Set(playerId, movesLeft + spendMoves);
            _movesDataProxy.SetPlayerMoves(CurrentPlayerId, Moves[CurrentPlayerId]);
            if (Moves[playerId] == 0)
            {
                AdvanceTurn();
            }

            RPC_BroadcastUpdateMovesDataProxy();
        }

        [Rpc(RpcSources.Proxies, RpcTargets.StateAuthority)]
        private void RPC_RequestTryToResetMoves()
        {
            if (!Object.HasStateAuthority) return;
            int playerId = Object.InputAuthority.PlayerId;
            Debug.Log($"[SERVER] trying to reset moves for {playerId}");

            Moves.Set(playerId, _gameplaySettings.MovesPerTurn);
            _movesDataProxy.SetPlayerMoves(CurrentPlayerId, Moves[CurrentPlayerId]);
            if (Moves[playerId] == 0)
            {
                AdvanceTurn();
            }

            RPC_BroadcastUpdateMovesDataProxy();
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.Proxies)]
        private void RPC_BroadcastUpdateMovesDataProxy()
        {
            Debug.Log($"Moves Data Proxy Updated with Player: {CurrentPlayerId}, Moves: {Moves[CurrentPlayerId]}");
            _movesDataProxy.SetPlayerMoves(CurrentPlayerId, Moves[CurrentPlayerId]);
            _movesDataProxy.SetCurrentTurnPlayer(CurrentPlayerId);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_BroadcastPlayerTurnStarted(int playerId)
        {
            Debug.Log($"[SERVER] Player Turn Started for Player: {playerId}");
            _movesDataProxy.SetCurrentTurnPlayer(playerId);
        }
        
        private static int GetFirstPlayerOfMatch()
        {
            return Random.Range(0, BaseConstants.PLAYERS_PER_MATCH);
        }

    }
}
