namespace OGClient.Gameplay.Players
{
    public struct PlayerModel
    {

        public PlayerModel(int playerIndex, bool isMain, string nickname)
        {
            PlayerIndex = playerIndex;
            IsMain = isMain;
            Nickname = nickname;
        }

        public int PlayerIndex { get; }
        public bool IsMain { get; }
        public string Nickname { get; }

    }
}
