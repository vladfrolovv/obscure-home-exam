using TMPro;
using UniRx;
using Zenject;
using UnityEngine;
using OGClient.Gameplay.Players;
using OGShared.DataProxies;
namespace OGClient.Gameplay.UI
{
    public class MovesView : MonoBehaviour
    {

        [Header("Moves View")]
        [SerializeField] private TextMeshProUGUI _movesText;
        [SerializeField] private Animator _movesAnimator;
        [SerializeField] private ProgressBarView _movesBarView;

        private int _playerIndex;
        private MovesDataProxy _movesDataProxy;

        [Inject]
        public void Construct(MovesDataProxy movesDataProxy)
        {
            _movesDataProxy = movesDataProxy;
        }

        public void Setup(int playerIndex, int maxMoves, Color color, NetworkPlayerController playerController)
        {
            _playerIndex = playerIndex;
            _movesBarView.SetIncrementColor(color);
            _movesBarView.SetProgressMax(maxMoves);
            _movesBarView.Setup(playerController);
        }

        private void Awake()
        {
            _movesDataProxy.Moves.ObserveAdd().Subscribe(OnMovesAdd).AddTo(this);
            _movesDataProxy.Moves.ObserveReplace().Subscribe(OnMovesReplaced).AddTo(this);
        }

        private void OnMovesAdd(DictionaryAddEvent<int, int> addEvent)
        {
            OnMovesUpgrade(addEvent.Key, addEvent.Value);
        }

        private void OnMovesReplaced(DictionaryReplaceEvent<int, int> replaceEvent)
        {
            OnMovesUpgrade(replaceEvent.Key, replaceEvent.NewValue);
        }

        private void OnMovesUpgrade(int playerIndex, int movesLeft)
        {
            if (_playerIndex != playerIndex) return;

            _movesBarView.ChangeProgress(movesLeft);
            _movesText.SetText($"{movesLeft}");

            const string bounceAnimationProperty = "Bounce";
            _movesAnimator.Play(bounceAnimationProperty);
            _movesAnimator.Play($"{bounceAnimationProperty}2");
        }

    }
}
