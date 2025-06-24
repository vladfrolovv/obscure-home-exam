using UniRx;
using Fusion;
using Zenject;
using UnityEngine;
using OGShared.DataProxies;
namespace OGServer.Gameplay
{
    public class GridLinkNetworkController : NetworkBehaviour
    {

        private GridLinkingDataProxy _gridLinkingDataProxy;

        [Inject]
        public void Construct(GridLinkingDataProxy gridLinkingDataProxy)
        {
            _gridLinkingDataProxy = gridLinkingDataProxy;
        }

        public override void Spawned()
        {
            Debug.Log($"[Spawned] Client={Runner.LocalPlayer}, InputAuth={Object.HasInputAuthority}");

            _gridLinkingDataProxy.StartedLink.Subscribe(RPC_RequestLinkStart).AddTo(this);
            _gridLinkingDataProxy.LinkAdded.Subscribe(RPC_RequestLinkAdd).AddTo(this);
            _gridLinkingDataProxy.LinkExecuted.Subscribe(_ => RPC_RequestLinkExecute()).AddTo(this);
        }

        [Rpc(RpcSources.Proxies, RpcTargets.StateAuthority)]
        private void RPC_RequestLinkStart(Vector2Int pos)
        {
            if (!Object.HasStateAuthority) return;
            Debug.Log($"[SERVER] RPC Request for link start at position {pos} from player {Object.InputAuthority.RawEncoded}");
            RPC_BroadcastLinkStarted(pos);
        }

        [Rpc(RpcSources.Proxies, RpcTargets.StateAuthority)]
        private void RPC_RequestLinkAdd(Vector2Int pos)
        {
            if (!Object.HasStateAuthority) return;
            Debug.Log($"[SERVER] RPC Request for link add at position {pos} from player {Object.InputAuthority.RawEncoded}");
            RPC_BroadcastLinkAdd(pos);
        }

        [Rpc(RpcSources.Proxies, RpcTargets.StateAuthority)]
        private void RPC_RequestLinkExecute()
        {
            if (!Object.HasStateAuthority) return;

            Debug.Log($"[SERVER] RPC Request for link execute from player {Object.InputAuthority.RawEncoded}");
            RPC_BroadcastLinkExecute();
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_BroadcastLinkStarted(Vector2Int pos)
        {
            Debug.Log($"[CLIENT {Runner.LocalPlayer} RECEIVED BroadcastLinkStarted @ {pos}");
            _gridLinkingDataProxy.RaiseLinkStarted(pos);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_BroadcastLinkAdd(Vector2Int pos)
        {
            Debug.Log($"[CLIENT {Runner.LocalPlayer} RECEIVED BroadcastLinkAdd @ {pos}");
            _gridLinkingDataProxy.RaiseLinkAdded(pos);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_BroadcastLinkExecute()
        {
            Debug.Log($"[CLIENT {Runner.LocalPlayer} RECEIVED BroadcastLinkExecute");
            _gridLinkingDataProxy.RaiseLinkExecuted();
        }

    }
}
