using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*Defines a tile clear/destroy pattern that can be executed on the grid*/
[CreateAssetMenu(fileName = "Destroy Pattern", menuName = "ScriptableObjects/Destroy Pattern", order = 1)]
public class ScriptableDestroyPattern : ScriptableObject
{
    public string patternName = "8 around tile";

    public float delay = 0;

    public Vector2Int[] directions;
}

