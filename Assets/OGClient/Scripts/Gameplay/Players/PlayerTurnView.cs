using TMPro;
using UnityEngine;
namespace OGClient.Gameplay.Players
{
    public class PlayerTurnView : MonoBehaviour
    {

        private const string AnimationParam = "Intro";

        [SerializeField] private TextMeshProUGUI _playerTurnText;
        [SerializeField] private Animator _playerTurnAnimator;

        public void ShowAnimation(string nickname)
        {
            _playerTurnText.SetText($"{nickname}'S TURN!");
            _playerTurnAnimator.Play(AnimationParam);
        }

    }
}
