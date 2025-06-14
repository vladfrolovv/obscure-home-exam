using ObscureGames.Gameplay.Boosters;
using UnityEngine;
namespace ObscureGames.Debug
{
    [CreateAssetMenu(fileName = "PlayerProfile", menuName = "ScriptableObjects/Player Profile", order = 1)]
    public class ScriptablePlayerProfile : ScriptableObject
    {

        [field: SerializeField]
        public string PlayerName { get; private set; } = "Player";

        [field: SerializeField]
        public Sprite AvatarIcon { get; private set; }

        [field: SerializeField]
        public ScriptableBooster Booster { get; private set; }

    }

}