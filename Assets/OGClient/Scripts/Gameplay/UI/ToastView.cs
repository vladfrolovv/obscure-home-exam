using UnityEngine;
using TMPro;
namespace OGClient.Gameplay.UI
{
    public class ToastView : MonoBehaviour
    {

        [Header("Animation")]
        [SerializeField] private Animator _thisAnimator;
        [SerializeField] private string _introAnimationProperty = "Intro";

        [Header("Text")]
        [SerializeField] private TextMeshProUGUI _text;

        public void SetToast(Vector3 position, string setMessage, Color setColor)
        {
            _text.SetText(setMessage);
            _text.color = setColor;

            _thisAnimator.Play(_introAnimationProperty);
        }

    }
}
