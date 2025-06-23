using System;
using UniRx;
namespace OGClient.Gameplay.DataProxies
{
    public class GridLinksDataProxy
    {

        private readonly ReactiveProperty<bool> _hasControl = new(false);
        private readonly Subject<Unit> _tryTyExecute = new();

        public IReadOnlyReactiveProperty<bool> HasControl => _hasControl;
        public IObservable<Unit> TryToExecute => _tryTyExecute;

        public void ChangeControlState(bool hasControl)
        {
            _hasControl.Value = hasControl;
        }

        public void TryToExecuteLink()
        {
            if (_hasControl.Value)
            {
                _tryTyExecute.OnNext(Unit.Default);
            }
        }

        public void Reset()
        {
            _hasControl.Value = false;
        }

    }
}
