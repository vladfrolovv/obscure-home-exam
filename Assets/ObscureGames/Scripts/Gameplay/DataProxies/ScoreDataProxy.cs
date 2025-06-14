using UniRx;
namespace ObscureGames.Gameplay.DataProxies
{
    public class ScoreDataProxy
    {

        private readonly ReactiveDictionary<int, int> _score = new ReactiveDictionary<int, int>();
        public IReadOnlyReactiveDictionary<int, int> Score => _score;

        public void IncreasePlayerScore(int playerId, int amount)
        {
            if (_score.ContainsKey(playerId))
            {
                _score[playerId] += amount;
            }
            else
            {
                _score.Add(playerId, amount);
            }
        }

        public void SetPlayerScore(int playerId, int score)
        {
            if (_score.ContainsKey(score))
            {
                _score[playerId] = score;
            } else
            {
                _score.Add(playerId, score);
            }
        }

        public int GetPlayerScore(int playerId)
        {
            _score.TryGetValue(playerId, out int score);
            return score;
        }

    }
}
