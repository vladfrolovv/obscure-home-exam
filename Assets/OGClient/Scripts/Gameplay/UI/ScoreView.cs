using OGClient.Gameplay.DataProxies;
using OGClient.Gameplay.Players;
using TMPro;
using UniRx;
using UnityEngine;
using Zenject;
namespace OGClient.Gameplay.UI
{
    public class ScoreView : MonoBehaviour, IPlayerIndexSetter
    {

        [SerializeField] private TextMeshProUGUI _scoreText;

        private int _playerIndex;
        private ScoreDataProxy _scoreDataProxy;

        public void SetPlayerIndex(int playerIndex) => _playerIndex = playerIndex;

        [Inject]
        public void Construct(ScoreDataProxy scoreDataProxy)
        {
            _scoreDataProxy = scoreDataProxy;
        }

        private void Awake()
        {
            _scoreDataProxy.Score.ObserveReplace().Subscribe(OnScoreReplaced).AddTo(this);
        }

        private void OnScoreReplaced(DictionaryReplaceEvent<int, int> replaceEvent)
        {
            if (replaceEvent.Key != _playerIndex) return;
            if (replaceEvent.NewValue == replaceEvent.OldValue) return;

            _scoreText.text = $"{replaceEvent.NewValue}";
        }

    }
}
