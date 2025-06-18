using System;
using UniRx;
using UnityEngine;
namespace OGClient.Gameplay.DataProxies
{
    public class RoundsDataProxy
    {

        private readonly Subject<PlayerSwitchModel> _playerSwitch = new ();
        private readonly Subject<string> _roundsTextChanged = new ();
        private readonly Subject<string> _currentRoundTextChanged = new ();
        private readonly Subject<float> _roundProgressChanged = new ();
        private readonly Subject<float> _changeRoundProgress = new ();

        private readonly ReactiveProperty<int> _rounds = new ();
        private readonly ReactiveProperty<int> _currentRound = new ();

        public void OnPlayerSwitched(PlayerSwitchModel playerSwitchModel) => _playerSwitch?.OnNext(playerSwitchModel);
        public void OnRoundsTextChanged(string text) => _roundsTextChanged?.OnNext(text);
        public void OnCurrentRoundTextChanged(string text) => _currentRoundTextChanged?.OnNext(text);

        public void SetRoundProgress(float progress) => _roundProgressChanged?.OnNext(progress);
        public void ChangeRoundProgress(float progress) => _changeRoundProgress?.OnNext(progress);

        public void SetRoundsAmount(int rounds) => _rounds.Value = rounds;
        public void SetCurrentRound(int currentRound) => _currentRound.Value = currentRound;

        public IObservable<PlayerSwitchModel> PlayerSwitch => _playerSwitch;
        public IObservable<string> RoundsTextChanged => _roundsTextChanged;
        public IObservable<string> CurrentRoundTextChanged => _currentRoundTextChanged;
        public IObservable<float> RoundProgressSet => _roundProgressChanged;
        public IObservable<float> RoundProgressChanged => _changeRoundProgress;

        public IReadOnlyReactiveProperty<int> Rounds => _rounds;
        public IReadOnlyReactiveProperty<int> CurrentRound => _currentRound;

        public bool CanSetPlayerUp => _currentRound.Value <= _rounds.Value;

    }

    public class PlayerSwitchModel
    {

        public PlayerSwitchModel(Color newPlayerColor)
        {
            NewPlayerColor = newPlayerColor;
        }

        public Color NewPlayerColor { get; set; }

    }

}
