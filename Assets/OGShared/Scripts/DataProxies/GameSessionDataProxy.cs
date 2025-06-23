using UniRx;
using System;
using OGShared.Gameplay;
using UnityEngine;
namespace OGShared.DataProxies
{
    public class GameSessionDataProxy
    {

        private readonly Subject<Unit> _initialized = new();
        private readonly Subject<MatchPhase> _matchPhaseChanged = new();
        private readonly ReactiveProperty<int> _rounds = new(0);
        private readonly ReactiveProperty<int> _currentRound = new(0);

        public Vector2Int GridSize { get; private set; } = new(7, 7);
        public int Seed { get; private set; } = -1;

        public void SetGridSize(Vector2Int size) => GridSize = size;
        public void SetSeed(int value) => Seed = value;
        public void SetRounds(int value) => _rounds.Value = value;
        public void SetCurrentRound(int value) => _currentRound.Value = value;
        public void SetMatchPhase(MatchPhase p) => _matchPhaseChanged.OnNext(p);
        public void RaiseInitialized() => _initialized.OnNext(Unit.Default);

        public IObservable<Unit> Initialized => _initialized;
        public IObservable<MatchPhase> MatchPhaseChanged => _matchPhaseChanged;
        public IReadOnlyReactiveProperty<int> Rounds => _rounds;
        public IReadOnlyReactiveProperty<int> CurrentRound=> _currentRound;

    }
}
