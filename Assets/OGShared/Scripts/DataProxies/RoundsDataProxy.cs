using System;
using UniRx;
namespace OGShared.DataProxies
{
    public class RoundsDataProxy
    {

        private readonly ReactiveProperty<int> _rounds = new ();
        private readonly ReactiveProperty<int> _currentRound = new ();

        public IObservable<int> Rounds => _rounds;
        public IObservable<int> CurrentRound => _currentRound;

    }
}
