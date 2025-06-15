using Fusion.Photon.Realtime;
using UniRx;
namespace OGClient.Gameplay.Authentication
{
    public class PhotonAuthDataProxy
    {

        private readonly ReactiveProperty<AuthenticationValues> _photonAuthValues = new();
        public IReadOnlyReactiveProperty<AuthenticationValues> PhotonAuthValues => _photonAuthValues;

        public void RecordLoginData(AuthenticationValues authValues)
        {
            _photonAuthValues.Value = authValues;
        }
        
    }
}
