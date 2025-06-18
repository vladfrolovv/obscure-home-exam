using System;
using System.Collections.Generic;
using UnityEngine;
namespace OGClient.Popups
{
    public class PopupsController : MonoBehaviour
    {

        [SerializeField] private List<PopupModel> _popupsMap = new ();

        public void ShowPopupByType(PopupType type, object data = null)
        {
            _popupsMap.Find(popup => popup.Type == type).View?.Show(data);
        }

        private void Awake()
        {
            _popupsMap.ForEach(popup => popup?.View.Hide());
        }

        [Serializable]
        public class PopupModel
        {
            [field: SerializeField] public PopupType Type { get; private set; }
            [field: SerializeField] public BasePopupView View { get; private set; }
        }

    }
}
