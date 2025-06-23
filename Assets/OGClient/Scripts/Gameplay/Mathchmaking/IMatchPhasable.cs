using OGShared.Gameplay;
namespace OGClient.Gameplay.Mathchmaking
{
    public interface IMatchPhasable
    {
        public void OnGameSessionDataInitialized();
        public void OnMatchPhaseChanged(MatchPhase phase);
    }
}
