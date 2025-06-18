using TMPro;
using UniRx;
using Zenject;
using UnityEngine;
using UnityEngine.UI;
using OGClient.Gameplay.Authentication;
namespace OGClient.Gameplay.Mathchmaking
{
    public class MatchmakingConnectionView : MonoBehaviour
    {

        [Header("UI")]
        [SerializeField] private Button _findMatchButton;
        [SerializeField] private TextMeshProUGUI _statusText;

        private ScriptableMatchmakingState _scriptableMatchmakingState;
        private PhotonAuthDataProxy _photonAuthDataProxy;
        private MatchmakingController _matchmakingController;

        private void SetButtonInteractable(bool interactable) => _findMatchButton.interactable = interactable;

        [Inject]
        public void Construct(MatchmakingController matchmakingController, PhotonAuthDataProxy photonAuthDataProxy, ScriptableMatchmakingState scriptableMatchmakingState)
        {
            _scriptableMatchmakingState = scriptableMatchmakingState;
            _matchmakingController = matchmakingController;
            _photonAuthDataProxy = photonAuthDataProxy;
        }

        private void Awake()
        {
            _findMatchButton.OnClickAsObservable().Subscribe(_ => JoinMatchmakingQueue()).AddTo(this);
            // _photonAuthDataProxy.PhotonAuthValues.Subscribe(_ => OnAuthenticationCompleted()).AddTo(this);
            // SetAuthenticationState();

            OnAuthenticationCompleted();
        }

        private void SetAuthenticationState()
        {
            SetButtonInteractable(false);
            _statusText.text = _scriptableMatchmakingState[MatchmakingState.Authentication];
        }

        private async void OnAuthenticationCompleted()
        {
            SetButtonInteractable(true);
            _statusText.text = _scriptableMatchmakingState[MatchmakingState.ReadyForMatchmaking];
        }

        private void JoinMatchmakingQueue()
        {
            MatchmakingProcess();
        }

        private async void MatchmakingProcess()
        {
            _statusText.text = _scriptableMatchmakingState[MatchmakingState.MatchmakingInProgress];
            SetButtonInteractable(false);

            bool isConnected = await _matchmakingController.StartMatchmakingProcess();
            if (isConnected)
            {
                _statusText.text = _scriptableMatchmakingState[MatchmakingState.WaitingForOtherPlayers];
            }
            else
            {
                _statusText.text = _scriptableMatchmakingState[MatchmakingState.MatchmakingFailed];
                SetButtonInteractable(true);
            }
        }

    }

}