using System;
using System.Collections.Generic;
using UnityEngine;
namespace ObscureGames.Gameplay.Grid
{
    public class MergeCombos : MonoBehaviour
    {
        public static MergeCombos instance;

        public Transform mergeEffect;

        public MergeCombo[] mergeCombos;

        [Serializable]
        public class MergeCombo
        {
            public string comboName;
            public Sprite icon;
            public GameObject executeObject;
            public List<int> itemTypes = new List<int>();
        }

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(this.gameObject);
            }
        }

        public void MergeEffect(Vector3 targetPosition)
        {
            if (mergeEffect)
            {
                Transform newEffect = Instantiate(mergeEffect);

                newEffect.position = targetPosition;
            }
        }

        public MergeCombo GetCombo(List<GridItemView> powerups)
        {
            MergeCombo mergeCombo = null;

            for (int index = 0; index < mergeCombos.Length; index++)
            {
                mergeCombo = mergeCombos[index];

                for (int indexPowerup = 0; indexPowerup < powerups.Count; indexPowerup++)
                {
                    //if (mergeCombo != null ) print("test " + powerups[indexPowerup].type + " " + mergeCombo.itemTypes.Contains(powerups[indexPowerup].type));
                    if (mergeCombo != null && mergeCombo.itemTypes.Contains(powerups[indexPowerup].GridItemType) == false)
                    {
                        mergeCombo = null;
                        continue;
                    }
                }

                if (mergeCombo != null) break;
            }

            if (mergeCombo == null)
            {
                // Debug.Log("No combo found");
            }

            return mergeCombo;
        }
    }
}
