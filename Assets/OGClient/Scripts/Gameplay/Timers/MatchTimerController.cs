using System;
using UniRx;
using UnityEngine;
namespace OGClient.Gameplay.Timers
{
    public class MatchTimerController
    {

        private const float EndingTime = 10f;

        private readonly CompositeDisposable _compositeDisposable = new();
        private readonly float _timePerTurn;

        private float _timeLeft;
        private bool _timerIsActive;

        private readonly Subject<Unit> _timeAlmostUp = new();
        private readonly Subject<Unit> _timeUp = new();
        private readonly Subject<Unit> _timeReset = new();
        private readonly Subject<Unit> _timerStarted = new();
        private readonly Subject<Unit> _timerPaused = new();

        private readonly Subject<float> _timeLeftChanged = new();

        public MatchTimerController(ScriptableGameplaySettings gameplaySettings)
        {
            _timePerTurn = gameplaySettings.TimePerTurn;
            Observable.EveryUpdate().Subscribe(OnTimerUpdate).AddTo(_compositeDisposable);
        }

        public void ResetTime()
        {
            _timeLeft = _timePerTurn;
            _timeReset?.OnNext(Unit.Default);
        }

        public void StartTimer()
        {
            _timerIsActive = true;
        }

        public void PauseTimerFor(float delay)
        {
            _timerIsActive = false;
            if (delay <= 0) return;
            Observable.Timer(TimeSpan.FromSeconds(delay))
                .Subscribe(_ => StartTimer())
                .AddTo(_compositeDisposable);
        }

        private void OnTimerUpdate(long l)
        {
            if (!_timerIsActive) return;
            if (_timeLeft > 0)
            {
                if (_timerIsActive)
                {
                    _timeLeft -= Time.deltaTime;
                }

                if (_timeLeft <= EndingTime)
                {
                    _timeAlmostUp?.OnNext(Unit.Default);
                }
            }
            else if (_timerIsActive)
            {
                _timerIsActive = false;
                _timeLeft = 0;

                _timeUp?.OnNext(Unit.Default);
            }
        }

        public IObservable<Unit> TimeAlmostUp => _timeAlmostUp;
        public IObservable<Unit> TimeUp => _timeUp;
        public IObservable<Unit> TimeReset => _timeReset;
        public IObservable<Unit> TimerStarted => _timerStarted;
        public IObservable<Unit> TimerPaused => _timerPaused;

        public IObservable<float> TimeLeftChanged => _timeLeftChanged;

    }
}
