using UniRx;
using System;
using UnityEngine;
namespace OGShared.DataProxies
{
    public class GridLinkingDataProxy
    {

        private readonly Subject<Vector2Int> _startedLink = new();
        private readonly Subject<Vector2Int> _linkAdded = new();
        private readonly Subject<Unit> _linkExecuted = new();

        public IObservable<Vector2Int> StartedLink => _startedLink;
        public IObservable<Vector2Int> LinkAdded => _linkAdded;
        public IObservable<Unit> LinkExecuted => _linkExecuted;

        public void RaiseLinkStarted(Vector2Int pos)
        {
            Debug.Log($"[GridLinkingDataProxy] Link Started");
            _startedLink.OnNext(pos);
        }
        public void RaiseLinkAdded(Vector2Int pos) => _linkAdded.OnNext(pos);
        public void RaiseLinkExecuted() => _linkExecuted.OnNext(Unit.Default);

    }
}
