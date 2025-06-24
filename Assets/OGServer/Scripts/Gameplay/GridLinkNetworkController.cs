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
            _gridLinkingDataProxy.StartedLink.Subscribe(RPC_RequestLinkStart).AddTo(this);
            _gridLinkingDataProxy.LinkAdded.Subscribe(RPC_RequestLinkAdd).AddTo(this);
            _gridLinkingDataProxy.LinkRemovedAfter.Subscribe(RPC_RequestLinkRemoveAfter).AddTo(this);
            _gridLinkingDataProxy.GridItemCollected.Subscribe(RPC_RequestGridItemCollected).AddTo(this);
            _gridLinkingDataProxy.LinkExecuted.Subscribe(_ => RPC_RequestLinkExecute()).AddTo(this);
        }

        [Rpc(RpcSources.Proxies, RpcTargets.StateAuthority)]
        private void RPC_RequestLinkStart(Vector2Int pos)
        {
            if (!Object.HasStateAuthority) return;
            RPC_BroadcastLinkStarted(pos);
        }

        [Rpc(RpcSources.Proxies, RpcTargets.StateAuthority)]
        private void RPC_RequestLinkAdd(Vector2Int pos)
        {
            if (!Object.HasStateAuthority) return;
            RPC_BroadcastLinkAdd(pos);
        }

        [Rpc(RpcSources.Proxies, RpcTargets.StateAuthority)]
        private void RPC_RequestLinkRemoveAfter(Vector2Int pos)
        {
            if (!Object.HasStateAuthority) return;
            RPC_BroadcastLinkRemoveAfter(pos);
        }

        [Rpc(RpcSources.Proxies, RpcTargets.StateAuthority)]
        private void RPC_RequestGridItemCollected(Vector2Int pos)
        {
            if (!Object.HasStateAuthority) return;
            RPC_BroadcastGridItemCollected(pos);
        }

        [Rpc(RpcSources.Proxies, RpcTargets.StateAuthority)]
        private void RPC_RequestLinkExecute()
        {
            if (!Object.HasStateAuthority) return;
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
        private void RPC_BroadcastLinkRemoveAfter(Vector2Int pos)
        {
            Debug.Log($"[CLIENT {Runner.LocalPlayer} RECEIVED BroadcastLinkRemoveAfter @ {pos}");
            _gridLinkingDataProxy.RaiseLinkRemovedAfter(pos);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_BroadcastGridItemCollected(Vector2Int pos)
        {
            Debug.Log($"[CLIENT {Runner.LocalPlayer} RECEIVED BroadcastGridItemCollected @ {pos}");
            _gridLinkingDataProxy.RaiseGridItemCollected(pos);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_BroadcastLinkExecute()
        {
            Debug.Log($"CLIENT {Runner.LocalPlayer} RECEIVED BroadcastLinkExecute");
            _gridLinkingDataProxy.RaiseLinkExecuted();
        }

    }
}
