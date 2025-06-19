using TMPro;
using UnityEngine;
using UnityEngine.UI;
using OGClient.Gameplay.UI;
namespace OGClient.Gameplay.Players
{
    public class PlayerView : MonoBehaviour
    {

        [Header("Configs")]
        [SerializeField] private ScriptablePlayersProfiles _playersProfiles;

        [Header("View")]
        [SerializeField] private TextMeshProUGUI _nicknameText;
        [SerializeField] private Image _avatarImage;
        [SerializeField] private Canvas _playerCanvas;
        [SerializeField] private ScoreView _scoreView;

        [Header("Canvas Settings")]
        [SerializeField] private RectTransform _avatarRT;
        [SerializeField] private RectTransform _avatarImageRT;
        [SerializeField] private RectTransform _movesBarRT;
        [SerializeField] private RectTransform _scoreRT;
        [SerializeField] private float _avatarOffsetX = 100;
        [SerializeField] private float _movesBarOffsetX = 75;
        [SerializeField] private float _scoreOffsetX = 40;

        [Header("Animations")]
        [SerializeField] private float _highlightSwitchDuration = .32f;

        public PlayerModel PlayerModel { get; private set; }

        public void HighlightPlayer(bool highlight) =>
            LeanTween.color(_avatarImage.rectTransform, highlight ? Color.white: Color.gray, _highlightSwitchDuration);

        public void InstallPlayerView(PlayerModel model)
        {
            PlayerModel = model;

            _nicknameText.text = model.Nickname;
            _avatarImage.sprite = model.IsMain ? _playersProfiles.MainAvatar : _playersProfiles.SideAvatar;

            _avatarRT.localPosition = new
                Vector3(model.IsMain ? -_avatarOffsetX : _avatarOffsetX, _avatarRT.localPosition.y, _avatarRT.localPosition.z);
            _movesBarRT.localPosition = new
                Vector3(model.IsMain ? -_movesBarOffsetX : _movesBarOffsetX, _movesBarRT.localPosition.y, _movesBarRT.localPosition.z);
            _scoreRT.localPosition = new
                Vector3(model.IsMain ? -_scoreOffsetX : _scoreOffsetX, _scoreRT.localPosition.y, _scoreRT.localPosition.z);

            _avatarImageRT.localScale = new Vector3(model.IsMain ? 1f : -1f, 1f, 1f);

            _playerCanvas.worldCamera = Camera.main;

            _scoreView.SetPlayerIndex(model.PlayerIndex);
        }

    }
}
