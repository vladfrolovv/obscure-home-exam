using OGClient.Utils;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using Zenject;
namespace OGClient.Gameplay.Authentication
{
    public class PlayFabAuth : MonoBehaviour
    {

        private PlayFabAuthDataProxy _playFabAuthDataProxy;

        [Inject]
        public void Construct(PlayFabAuthDataProxy playFabAuthDataProxy)
        {
            _playFabAuthDataProxy = playFabAuthDataProxy;
        }

        private void Awake()
        {
            TryToLogin();
        }

        private void TryToLogin()
        {
            LoginWithCustomIDRequest loginWithCustomIDRequest = new()
            {
                CustomId = PlayerDataGenerator.TryToCreateOrGetToken(),
                CreateAccount = true,
            };

            PlayFabClientAPI.LoginWithCustomID(loginWithCustomIDRequest, OnSuccessfulLogin, OnLoginError);
        }

        private void OnSuccessfulLogin(LoginResult loginResult)
        {
            Debug.Log($"PlayFab token acquired: {loginResult.PlayFabId}. Authentication complete.");
            _playFabAuthDataProxy.RecordLoginData(loginResult.PlayFabId);
        }

        private void OnLoginError(PlayFabError error)
        {
            Debug.Log($"Error: {error.Error}");
        }

    }
}
