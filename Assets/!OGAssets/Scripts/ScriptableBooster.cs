using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*Defines a GridItem (ex: Blue item, Powerup item, etc)*/
[CreateAssetMenu(fileName = "Booster", menuName = "ScriptableObjects/Booster", order = 1)]
public class ScriptableBooster : ScriptableObject
{
    public int itemType;
    public Sprite icon;
    public int cost = 20;
    public GameObject executeObject;
}
