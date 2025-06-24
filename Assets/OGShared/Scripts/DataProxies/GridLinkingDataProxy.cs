using UniRx;
using System;
using UnityEngine;
namespace OGShared.DataProxies
{
    public class GridLinkingDataProxy
    {

        private readonly Subject<Vector2Int> _startedLink = new();
        private readonly Subject<Vector2Int> _linkAdded = new();
        private readonly Subject<Vector2Int> _linkRemovedAfter = new();
        private readonly Subject<Vector2Int> _gridItemCollected = new();
        private readonly Subject<Unit> _linkExecuted = new();

        public IObservable<Vector2Int> StartedLink => _startedLink;
        public IObservable<Vector2Int> LinkAdded => _linkAdded;
        public IObservable<Vector2Int> LinkRemovedAfter => _linkRemovedAfter;
        public IObservable<Vector2Int> GridItemCollected => _gridItemCollected;
        public IObservable<Unit> LinkExecuted => _linkExecuted;

        public void RaiseLinkStarted(Vector2Int pos) => _startedLink.OnNext(pos);
        public void RaiseLinkAdded(Vector2Int pos) => _linkAdded.OnNext(pos);
        public void RaiseLinkRemovedAfter(Vector2Int pos) => _linkRemovedAfter.OnNext(pos);
        public void RaiseGridItemCollected(Vector2Int pos) => _gridItemCollected.OnNext(pos);
        public void RaiseLinkExecuted() => _linkExecuted.OnNext(Unit.Default);

    }
}
