using System.Collections.Generic;
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

        [Networked] private int CurrentPlayerId { get; set; }
        private readonly Dictionary<int, int> _moves = new (BaseConstants.PLAYERS_PER_MATCH);

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
            _movesDataProxy.SpendMoves.Subscribe(RPC_RequestTryToSpendMove).AddTo(this);
            _movesDataProxy.ResetMoves.Subscribe(_ => RPC_RequestTryToResetMoves()).AddTo(this);
        }

        public void InitializeMovesNetworkController()
        {
            if (!Object.HasStateAuthority) return;
            StartPlayerTurn(GetFirstPlayerOfMatch());
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
            _moves[playerId] = _gameplaySettings.MovesPerTurn;

            Debug.Log($"[SERVER] is trying to initialize moves controller with First Player[{CurrentPlayerId}] "
                      + $"Standard Moves amount [{_gameplaySettings.MovesPerTurn}]");

            RPC_BroadcastUpdateMovesDataProxy(CurrentPlayerId, _moves[CurrentPlayerId]);
            RPC_BroadcastPlayerTurnStarted(CurrentPlayerId);
        }

        [Rpc(RpcSources.Proxies, RpcTargets.StateAuthority)]
        private void RPC_RequestTryToSpendMove(int moves)
        {
            if (!Object.HasStateAuthority) return;
            if (_moves.TryGetValue(CurrentPlayerId, out int left) && left > 0)
            {
                int spend = moves == 0 ? left : moves;
                _moves[CurrentPlayerId] = left - spend;
                Debug.Log($"[SERVER] Player {CurrentPlayerId} spent {spend}, left {_moves[CurrentPlayerId]}");

                if (_moves[CurrentPlayerId] <= 0)
                    AdvanceTurn();

                RPC_BroadcastUpdateMovesDataProxy(CurrentPlayerId, _moves[CurrentPlayerId]);
            }
        }

        [Rpc(RpcSources.Proxies, RpcTargets.StateAuthority)]
        private void RPC_RequestTryToResetMoves()
        {
            if (!Object.HasStateAuthority) return;
            _moves[CurrentPlayerId] = _gameplaySettings.MovesPerTurn;
            Debug.Log($"[SERVER] Player {CurrentPlayerId} reset moves to {_moves[CurrentPlayerId]}");

            if (_moves[CurrentPlayerId] <= 0)
                AdvanceTurn();

            RPC_BroadcastUpdateMovesDataProxy(CurrentPlayerId, _moves[CurrentPlayerId]);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.Proxies)]
        private void RPC_BroadcastUpdateMovesDataProxy(int playerId, int moves)
        {
            Debug.Log($"[CLIENT] UpdateMovesDataProxy: Player {playerId}, Moves {moves}");
            _movesDataProxy.SetPlayerMoves(playerId, moves);
            _movesDataProxy.SetCurrentTurnPlayer(playerId);
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
