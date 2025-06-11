using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExecuteData
{
    public GridTile gridTile;
    public float delay;

    public ExecuteData(GridTile setGridTile, float setDelay)
    {
        gridTile = setGridTile;
        delay = setDelay;
    }
}
