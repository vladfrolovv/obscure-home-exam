using System.Collections.Generic;
using System.Linq;
using ObscureGames.Players;
using UnityEngine;
namespace ObscureGames.Gameplay.UI
{
    public class RandomTextView : MonoBehaviour
    {

        [SerializeField] private Player[] playerObjects;
        [SerializeField] private List<string> names = new List<string>
        {
            "HASAN", "HUSSAIN", "MUHAMMAD", "AYMAN", "ASMA", "MAJD", "PAVEL", "JAVI", "YADI", "DANIEL", "TAMER"
        };

        void Awake()
        {
            names = names.OrderBy(x => Random.value).ToList();
            for (int textIndex = 0; textIndex < playerObjects.Length; textIndex++)
            {
                playerObjects[textIndex].playerName = names[textIndex];
            }
        }

    }
}
