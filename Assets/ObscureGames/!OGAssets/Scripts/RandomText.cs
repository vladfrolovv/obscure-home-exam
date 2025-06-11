using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

/*Defines a set of texts that can be assigned randomly to a text object*/
public class RandomText : MonoBehaviour
{
    [SerializeField] private Player[] playerObjects;
    [SerializeField] private List<string> names = new List<string>{ "HASAN", "HUSSAIN", "MUHAMMAD", "AYMAN", "ASMA", "MAJD", "PAVEL", "JAVI", "YADI", "DANIEL", "TAMER" };

    // Start is called before the first frame update
    void Awake()
    {
        names = names.OrderBy(x => Random.value).ToList();

        for ( int textIndex = 0; textIndex < playerObjects.Length; textIndex++ )
        {
            playerObjects[textIndex].playerName = names[textIndex];
        }
    }
}
