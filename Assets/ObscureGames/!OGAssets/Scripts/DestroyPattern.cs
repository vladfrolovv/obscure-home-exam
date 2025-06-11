using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DestroyPattern
{
    public string patternName = "8 around tile";

    public float delay = 0;

    public Vector2Int[] directions;
}
