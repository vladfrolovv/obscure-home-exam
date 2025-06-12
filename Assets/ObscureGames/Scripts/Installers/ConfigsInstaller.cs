using ObscureGames.Gameplay.Grid.MergeCombos;
using UnityEngine;
using Zenject;
namespace ObscureGames.Installers
{
    [CreateAssetMenu(fileName = "ConfigsInstaller", menuName = "ScriptableObjects/Configs Installer")]
    public class ConfigsInstaller : ScriptableObjectInstaller
    {

        [SerializeField] private ScriptableMergeCombos _scriptableMergeCombos;

        public override void InstallBindings()
        {
            Container.BindInstance(_scriptableMergeCombos);
        }

    }
}
