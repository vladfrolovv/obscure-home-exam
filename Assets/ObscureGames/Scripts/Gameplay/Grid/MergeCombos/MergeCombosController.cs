using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace ObscureGames.Gameplay.Grid.MergeCombos
{
    public class MergeCombosController
    {

        private readonly MergeEffectView _mergeEffectView;
        private readonly ScriptableMergeCombos _mergeCombosConfig;

        public MergeCombosController(MergeEffectView mergeEffectView, ScriptableMergeCombos mergeCombosConfig)
        {
            _mergeEffectView = mergeEffectView;
            _mergeCombosConfig = mergeCombosConfig;
        }

        public void MergeEffect(Vector3 targetPosition)
        {
            if (_mergeEffectView == null) return;
            Object.Instantiate(_mergeEffectView, targetPosition, Quaternion.identity);
        }

        public MergeComboModel GetMergeCombo(List<GridItemView> powerups)
        {
            if (powerups == null || powerups.Count == 0)
                return null;

            return _mergeCombosConfig.MergeCombos
                .FirstOrDefault(combo => powerups.All(pu => combo.ItemTypes.Contains(pu.GridItemType)));
        }
    }
}
