using OGClient.Gameplay;
using OGClient.Gameplay.Grid.MergeCombos;
using UnityEngine;
using Zenject;
namespace OGClient.Installers
{
    [CreateAssetMenu(fileName = "ConfigsInstaller", menuName = "ScriptableObjects/Configs Installer")]
    public class ConfigsInstaller : ScriptableObjectInstaller
    {

        [SerializeField] private ScriptableMergeCombos _scriptableMergeCombos;
        [SerializeField] private ScriptableGameplaySettings _scriptableGameplaySettings;

        public override void InstallBindings()
        {
            Container.BindInstance(_scriptableMergeCombos);
            Container.BindInstance(_scriptableGameplaySettings);
        }

    }
}
