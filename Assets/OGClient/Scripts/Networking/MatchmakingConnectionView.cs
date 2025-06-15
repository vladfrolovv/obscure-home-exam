using OGServer.Matchmaking;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniRx;
using Zenject;
namespace OGClient.Networking
{
    public class MatchmakingConnectionView : MonoBehaviour
    {

        [Header("UI")]
        [SerializeField] private Button _findMatchButton;
        [SerializeField] private TextMeshProUGUI _statusText;

        private MatchmakingController _matchmakingController;

        [Inject]
        public void Construct(MatchmakingController matchmakingController)
        {
            _matchmakingController = matchmakingController;
        }

        private void Awake()
        {
            _findMatchButton.OnClickAsObservable().Subscribe(_ => JoinMatchmakingQueue());
            _findMatchButton.interactable = false;
            _statusText.text = $"Not connected!";
        }

        private async void JoinMatchmakingQueue()
        {
            StartMatchmakingProcess();
        }

        private async void StartMatchmakingProcess()
        {
            _statusText.text = "Connecting...";
            _findMatchButton.interactable = false;

            bool isConnected = await _matchmakingController.StartMatchmakingProcess();
            if (isConnected)
            {
                _statusText.text = "Waiting for other player!";
            }
            else
            {
                _statusText.text = "Failed to connect. Try again later...";
                _findMatchButton.interactable = true;
            }
        }

    }

}