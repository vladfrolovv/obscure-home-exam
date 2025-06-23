using UniRx;
using Fusion;
using Zenject;
using OGClient;
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

        public void InitializeMovesController()
        {
            if (!Object.HasStateAuthority) return;
            StartPlayerTurn(GetFirstPlayerOfMatch());

            Debug.Log($"[SERVER] is trying to initialize moves controller with First Player[{CurrentPlayerId}] and Standard Moves amount [{_gameplaySettings.MovesPerTurn}]");
            _movesDataProxy.LocalPlayerWantsToSpendMoves.Subscribe(RPC_TryToSpendMove).AddTo(this);
            _movesDataProxy.LocalPlayerWantsToResetMoves.Subscribe(_ => RPC_TryToResetMoves()).AddTo(this);
        }

        private int GetFirstPlayerOfMatch()
        {
            return 0;
        }

        private void AdvanceTurn()
        {
            if (!Object.HasStateAuthority) return;
            StartPlayerTurn(CurrentPlayerId == 1 ? 0 : 1);
        }

        private void StartPlayerTurn(int playerId)
        {
            if (!Object.HasStateAuthority) return;

            CurrentPlayerId = playerId;
            Moves.Add(CurrentPlayerId, _gameplaySettings.MovesPerTurn);
            RPC_UpdateMovesDataProxy();
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.Proxies)]
        private void RPC_UpdateMovesDataProxy()
        {
            Debug.Log($"Moves Data Proxy Updated with Player: {CurrentPlayerId}, Moves: {Moves[CurrentPlayerId]}");

            _movesDataProxy.SetPlayerMoves(CurrentPlayerId, Moves[CurrentPlayerId]);
            _movesDataProxy.SetCurrentTurnPlayer(CurrentPlayerId);
        }

        [Rpc(RpcSources.Proxies, RpcTargets.StateAuthority)]
        private void RPC_TryToSpendMove(int moves)
        {
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
        }

        [Rpc(RpcSources.Proxies, RpcTargets.StateAuthority)]
        private void RPC_TryToResetMoves()
        {
            int playerId = Object.InputAuthority.PlayerId;
            Debug.Log($"[SERVER] trying to reset moves for {playerId}");

            Moves.Set(playerId, _gameplaySettings.MovesPerTurn);
            _movesDataProxy.SetPlayerMoves(CurrentPlayerId, Moves[CurrentPlayerId]);
            if (Moves[playerId] == 0)
            {
                AdvanceTurn();
            }
        }

    }
}
