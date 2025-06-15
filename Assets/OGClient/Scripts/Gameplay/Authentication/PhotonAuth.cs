using UniRx;
using PlayFab;
using Zenject;
using UnityEngine;
using PlayFab.ClientModels;
using Fusion.Photon.Realtime;
namespace OGClient.Gameplay.Authentication
{
    public class PhotonAuth : MonoBehaviour
    {

        private const string UsernameParamName = "username";
        private const string TokenParamName = "token";

        private PhotonAuthDataProxy _photonAuthDataProxy;
        private PlayFabAuthDataProxy _playFabAuthDataProxy;

        [Inject]
        public void Construct(PlayFabAuthDataProxy playFabAuthDataProxy, PhotonAuthDataProxy photonAuthDataProxy)
        {
            _photonAuthDataProxy = photonAuthDataProxy;
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

            _photonAuthDataProxy.RecordLoginData(customAuthentication);
        }

        private static void OnPlayFabError(PlayFabError obj)
        {
            Debug.Log(obj.GenerateErrorReport());
        }

    }
}
