using UnityEngine;
namespace OGClient.Popups
{
    public abstract class BasePopupView : MonoBehaviour
    {

        public virtual void Show(object data = null)
        {
            gameObject.SetActive(true);
        }

        public virtual void Hide()
        {
            gameObject.SetActive(false);
        }

    }
}
