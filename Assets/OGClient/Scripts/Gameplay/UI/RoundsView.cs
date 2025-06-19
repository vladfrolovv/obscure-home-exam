using OGClient.Gameplay.DataProxies;
using OGClient.Gameplay.Players;
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

        private GameManager _gameManager;
        private RoundsDataProxy _roundsDataProxy;
        private ScriptableGameplaySettings _gameplaySettings;

        [Inject]
        public void Construct(ScriptableGameplaySettings gameplaySettings, GameManager gameManager)
        {
            _gameManager = gameManager;
            _gameplaySettings = gameplaySettings;
        }

        public void SetRoundsText(string text) => _roundsText.SetText(text);
        public void SetCurrentRoundText(string text) => _currentRoundText.SetText(text);
        public void SetRoundProgress(float progress) => _roundsBarView.SetProgress(progress);
        public void ChangeRoundProgress(float changeValue) => _roundsBarView.ChangeProgress(changeValue);

        private void Awake()
        {
            _gameManager.PlayerSwitched.Subscribe(OnPlayerSwitched).AddTo(this);

            Setup();
        }

        private void OnPlayerSwitched(PlayerView view)
        {
            _roundsBarView.SetIncrementColor(view.PlayerModel.Color);
            _roundsBarView.Bounce();
        }

        private void Setup()
        {
            _roundsText.SetText(string.Empty);
            _roundsBarView.SetProgress(0);
            _roundsBarView.SetProgressMax(_gameplaySettings.RoundsPerGame);
            _roundsBarView.Setup(null);
        }

    }
}
