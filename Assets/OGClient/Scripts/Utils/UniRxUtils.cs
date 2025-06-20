using System;
using UniRx;
namespace OGClient.Utils
{
    public static class UniRxUtils
    {

        public static IObservable<Unit> WaitUntilObs(Func<bool> yourCondition)
        {
            return Observable
                .EveryUpdate()
                .Where(_ => yourCondition())
                .First()
                .AsUnitObservable();
        }

    }
}
