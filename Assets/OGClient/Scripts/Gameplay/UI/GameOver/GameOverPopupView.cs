using OGClient.Popups;
using TMPro;
using UnityEngine;
namespace OGClient.Gameplay.UI.GameOver
{
    public class GameOverPopupView : BasePopupView
    {

        [SerializeField] private TextMeshProUGUI _gameOverText;

        public override void Show(object data = null)
        {
            if (data is GameOverPopupModel model)
            {
                _gameOverText.text = model.GameOverText;
            }

            base.Show(data);
        }

    }
}
