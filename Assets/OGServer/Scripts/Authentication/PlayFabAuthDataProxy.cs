using UniRx;
namespace OGServer.Authentication
{
    public class PlayFabAuthDataProxy
    {

        private readonly ReactiveProperty<string> _playFabToken = new(string.Empty);
        public IReadOnlyReactiveProperty<string> PlayFabToken => _playFabToken;

        public void RecordLoginData(string playFabId)
        {
            _playFabToken.Value = playFabId;
        }

    }
}
