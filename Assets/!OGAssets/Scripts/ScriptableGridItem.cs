using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*Defines a GridItem (ex: Blue item, Powerup item, etc)*/
[CreateAssetMenu(fileName = "Grid Item", menuName = "ScriptableObjects/Grid Item", order = 1)]
public class ScriptableGridItem : ScriptableObject
{
    public Sprite icon;
    public Sprite shadow;
    public Sprite glow;
    public Color color = Color.white;
}
