using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*Defines a grid pattern that contains GridItems (items, powerups, etc)*/
[CreateAssetMenu(fileName = "Grid Pattern", menuName = "ScriptableObjects/Grid Pattern", order = 1)]
public class ScriptableGridPattern : ScriptableObject
{
    public int[] items;
}
