using Fusion.Photon.Realtime;
using PlayFab;
using PlayFab.ClientModels;
using UniRx;
using UnityEngine;
using Zenject;
namespace OGServer.Authentication
{
    public class PhotonAuth : MonoBehaviour
    {

        private const string UsernameParamName = "username";
        private const string TokenParamName = "token";

        public static AuthenticationValues AuthValues { get; private set; }
        private PlayFabAuthDataProxy _playFabAuthDataProxy;

        [Inject]
        public void Construct(PlayFabAuthDataProxy playFabAuthDataProxy)
        {
            _playFabAuthDataProxy = playFabAuthDataProxy;
        }

        private void Awake()
        {
            _playFabAuthDataProxy.PlayFabToken.Subscribe(OnTokenReceived).AddTo(this);
        }

        private void OnTokenReceived(string token)
        {
            if (string.IsNullOrEmpty(token)) return;
            PlayFabClientAPI.GetPhotonAuthenticationToken(new GetPhotonAuthenticationTokenRequest()
            {
                PhotonApplicationId = PhotonAppSettings.Global.AppSettings.AppIdFusion
            }, AuthenticateWithPhoton, OnPlayFabError);
        }

        private void AuthenticateWithPhoton(GetPhotonAuthenticationTokenResult  authenticationTokenResult)
        {
            Debug.Log($"Photon token acquired: {authenticationTokenResult.PhotonCustomAuthenticationToken}. Authentication complete.");
            AuthenticationValues customAuthentication = new()
            {
                AuthType = CustomAuthenticationType.Custom
            };

            customAuthentication.AddAuthParameter(UsernameParamName, _playFabAuthDataProxy.PlayFabToken.Value);
            customAuthentication.AddAuthParameter(TokenParamName, authenticationTokenResult.PhotonCustomAuthenticationToken);

            AuthValues = customAuthentication;
        }

        private static void OnPlayFabError(PlayFabError obj)
        {
            Debug.Log(obj.GenerateErrorReport());
        }

    }
}
