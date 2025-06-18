using UnityEngine;
namespace OGClient.Gameplay.Players
{
    public struct PlayerModel
    {

        public PlayerModel(int playerIndex, bool isMain, string nickname, Color color)
        {
            PlayerIndex = playerIndex;
            IsMain = isMain;
            Nickname = nickname;
            Color = color;
        }

        public int PlayerIndex { get; }
        public bool IsMain { get; }
        public string Nickname { get; }
        public Color Color { get; }

    }
}
