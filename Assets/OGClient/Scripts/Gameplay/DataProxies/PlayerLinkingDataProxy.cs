using System;
using UniRx;
namespace OGClient.Gameplay.DataProxies
{
    public class PlayerLinkingDataProxy
    {
        private readonly ReactiveProperty<bool> _hasControl = new(false);
        private readonly Subject<Unit> _tryTyExecute = new();

        public IReadOnlyReactiveProperty<bool> HasControl => _hasControl;
        public IObservable<Unit> TryToExecute => _tryTyExecute;

        public void ChangeControlState(bool hasControl)
        {
            _hasControl.Value = hasControl;
        }

        public void Reset()
        {
            _hasControl.Value = false;
        }
    }
}
