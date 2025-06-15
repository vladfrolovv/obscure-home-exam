using TMPro;
using UniRx;
using UnityEngine;
using Zenject;
namespace OGClient.Gameplay.Authentication
{
    public class PlayFabTokenView : MonoBehaviour
    {

        [SerializeField] private TextMeshProUGUI _tokenText;

        private PlayFabAuthDataProxy _playFabAuthDataProxy;

        [Inject]
        public void Construct(PlayFabAuthDataProxy playFabAuthDataProxy)
        {
            _playFabAuthDataProxy = playFabAuthDataProxy;
        }

        private void Awake()
        {
            _playFabAuthDataProxy.PlayFabToken.Subscribe(data =>
            {
                _tokenText.text = string.IsNullOrEmpty(data) ? $"Token not available." : $"{data}";
            }).AddTo(this);
        }

    }
}
