namespace OGClient.Networking.Mathchmaking
{
    public enum MatchmakingState
    {
        Authentication = 0,
        ReadyForMatchmaking = 1,
        MatchmakingInProgress = 2,
        WaitingForOtherPlayers = 3,
        MatchmakingFailed = 4,
    }
}
