using UnityEngine;
namespace OGClient.Gameplay.Players
{
    [CreateAssetMenu(fileName = "PlayersProfiles", menuName = "ScriptableObjects/Players Profiles", order = 1)]
    public class ScriptablePlayersProfiles : ScriptableObject
    {

        [field: SerializeField, Header("Debug Avatars")]
        public Sprite MainAvatar { get; private set; }

        [field: SerializeField]
        public Sprite SideAvatar { get; private set; }

        [field: SerializeField, Header("Colors")]
        public Color MainColor { get; private set; }

        [field: SerializeField]
        public Color SideColor { get; private set; }

        public Sprite GetAvatar(bool isMain) => isMain ? MainAvatar : SideAvatar;
        public Color GetColor(bool isMain) => isMain ? MainColor : SideColor;

    }

}