using ObscureGames.Gameplay.Boosters;
using UnityEngine;
namespace ObscureGames.Players
{
    /*Defines a GridItem (ex: Blue item, Powerup item, etc)*/
    [CreateAssetMenu(fileName = "PlayerProfile", menuName = "ScriptableObjects/Player Profile", order = 1)]
    public class ScriptablePlayerProfile : ScriptableObject
    {
        public string playerName = "Player";
        public Sprite avatarIcon;
        public ScriptableBooster booster;
    }

}