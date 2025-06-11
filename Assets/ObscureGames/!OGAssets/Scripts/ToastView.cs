using UnityEngine;
using TMPro;
namespace ObscureGames
{
    public class ToastView : MonoBehaviour
    {

        [SerializeField] private Animator thisAnimator;
        [SerializeField] private TextMeshProUGUI thisText;

        public void SetToast(Vector3 position, string setMessage, Color setColor)
        {
            thisText.SetText(setMessage);
            thisText.color = setColor;

            thisAnimator.Play("Intro");
        }

    }
}
