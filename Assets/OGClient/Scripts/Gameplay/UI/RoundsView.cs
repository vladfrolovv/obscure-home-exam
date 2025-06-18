using OGClient.Gameplay.DataProxies;
using TMPro;
using UniRx;
using UnityEngine;
using Zenject;
namespace OGClient.Gameplay.UI
{
    public class RoundsView : MonoBehaviour
    {

        [Header("View")]
        [SerializeField] private ProgressBarView _roundsBarView;
        [SerializeField] private TextMeshProUGUI _roundsText;
        [SerializeField] private TextMeshProUGUI _currentRoundText;

        private ScriptableGameplaySettings _gameplaySettings;
        private RoundsDataProxy _roundsDataProxy;

        [Inject]
        public void Construct(ScriptableGameplaySettings gameplaySettings)
        {
            _gameplaySettings = gameplaySettings;
        }

        private void RoundsTextChanged(string text) => _roundsText.SetText(text);
        private void CurrentRoundTextChanged(string text) => _currentRoundText.SetText(text);
        private void RoundProgressSet(float progress) => _roundsBarView.SetProgress(progress);
        private void RoundProgressChanged(float changeValue) => _roundsBarView.ChangeProgress(changeValue);

        private void Awake()
        {
            Setup();

            _roundsDataProxy.PlayerSwitch.Subscribe(OnPlayerSwitched).AddTo(this);
            _roundsDataProxy.RoundsTextChanged.Subscribe(RoundsTextChanged).AddTo(this);
            _roundsDataProxy.CurrentRoundTextChanged.Subscribe(CurrentRoundTextChanged).AddTo(this);
            _roundsDataProxy.RoundProgressSet.Subscribe(RoundProgressSet).AddTo(this);
            _roundsDataProxy.RoundProgressChanged.Subscribe(RoundProgressChanged).AddTo(this);
        }

        private void Setup()
        {
            _roundsText.SetText(string.Empty);
            _roundsBarView.SetProgress(0);
            _roundsBarView.SetProgressMax(_gameplaySettings.RoundsPerGame);
            _roundsBarView.Setup(null);
        }

        private void OnPlayerSwitched(PlayerSwitchModel model)
        {
            _roundsBarView.SetIncrementColor(model.NewPlayerColor);
            _roundsBarView.Bounce();
        }

    }
}
