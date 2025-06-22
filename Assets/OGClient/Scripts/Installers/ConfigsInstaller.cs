using OGClient.Gameplay.Grid.Configs;
using OGClient.Gameplay.Grid.MergeCombos;
using OGClient.Gameplay.Mathchmaking;
using OGClient.Gameplay.Players;
using OGServer.Gameplay;
using OGShared.Gameplay;
using UnityEngine;
using Zenject;
namespace OGClient.Installers
{
    [CreateAssetMenu(fileName = "ConfigsInstaller", menuName = "ScriptableObjects/Configs Installer")]
    public class ConfigsInstaller : ScriptableObjectInstaller
    {

        [SerializeField] private ScriptableMergeCombos _scriptableMergeCombos;
        [SerializeField] private ScriptablePlayersProfiles _scriptablePlayersProfiles;
        [SerializeField] private ScriptableGameplaySettings _scriptableGameplaySettings;
        [SerializeField] private ScriptableMatchmakingState _scriptableMatchmakingState;
        [SerializeField] private ScriptableGridLinkSettings _scriptableGridLinkSettings;
        [SerializeField] private ScriptableGameSessionSettings _scriptableGameSessionSettings;

        public override void InstallBindings()
        {
            Container.BindInstance(_scriptableMergeCombos);
            Container.BindInstance(_scriptablePlayersProfiles);
            Container.BindInstance(_scriptableGameplaySettings);
            Container.BindInstance(_scriptableMatchmakingState);
            Container.BindInstance(_scriptableGridLinkSettings);
            Container.BindInstance(_scriptableGameSessionSettings);
        }

    }
}
