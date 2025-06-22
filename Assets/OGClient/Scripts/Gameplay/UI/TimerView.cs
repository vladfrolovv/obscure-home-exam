using OGClient.Gameplay.Players;
using OGClient.Gameplay.Timers;
using OGShared.Gameplay;
using UniRx;
using UnityEngine;
using Zenject;
namespace OGClient.Gameplay.UI
{
    public class TimerView : MonoBehaviour
    {

        [SerializeField] private ProgressBarView _timerView;

        private GameManager _gameManager;
        private MatchTimerController _matchTimerController;
        private ScriptableGameplaySettings _gameplaySettings;

        private bool _timeAlmostUp;

        [Inject]
        public void Construct(MatchTimerController matchTimerController, ScriptableGameplaySettings scriptableGameplaySettings, GameManager gameManager)
        {
            _gameManager = gameManager;
            _matchTimerController = matchTimerController;
            _gameplaySettings = scriptableGameplaySettings;
        }

        public void ResetTime()
        {
            _timerView.Bounce();
            _timerView.ChangeProgress(1000);

            LeanTween.cancel(_timerView.gameObject);
        }

        private void Awake()
        {
            _matchTimerController.TimeLeftChanged.Subscribe(UpdateView).AddTo(this);
            _matchTimerController.TimeAlmostUp.Subscribe(OnTimeAlmostUp).AddTo(this);
            _matchTimerController.TimeUp.Subscribe(OnTimeUp).AddTo(this);

            _gameManager.PlayerSwitched.Subscribe(OnPlayerSwitched).AddTo(this);

            SetupTimerView();
        }

        private void UpdateView(float time)
        {
            _timerView.SetProgress(time);
        }

        private void OnPlayerSwitched(PlayerView view)
        {
            _timerView.SetBarColor(view.PlayerModel.Color);
        }

        private void OnTimeAlmostUp(Unit unit)
        {
            if (_timeAlmostUp) return;
            _timeAlmostUp = true;

            LeanTween.scale(_timerView.gameObject, Vector3.one * 1.1f, 0.5f).setLoopPingPong().setEaseInBack();
        }

        private void OnTimeUp(Unit unit)
        {
            LeanTween.scale(_timerView.gameObject, Vector3.one * 1, 0.3f).setEaseInBack();
            _timerView.Shake();
        }

        private void SetupTimerView()
        {
            _timerView.SetProgress(_gameplaySettings.TimePerTurn);
            _timerView.SetProgressMax(_gameplaySettings.TimePerTurn);
            _timerView.Setup(null);
            _timerView.gameObject.SetActive(false);
        }

    }
}
