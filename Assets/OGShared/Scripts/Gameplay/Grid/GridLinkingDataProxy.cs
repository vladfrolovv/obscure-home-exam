using System;
using UniRx;
namespace OGShared.Gameplay.Grid
{
    public class GridLinkingDataProxy
    {

        private readonly Subject<Unit> _linkExecuted = new();
        public IObservable<Unit> LinkExecuted => _linkExecuted;

        public void OnLinkExecuted()
        {
            _linkExecuted?.OnNext(Unit.Default);
        }

    }
}
