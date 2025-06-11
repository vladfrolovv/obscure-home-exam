using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GridItem : MonoBehaviour
{
    public ScriptableGridItem customItem;

    public int type = 0;

    [SerializeField] private Animator thisAnimator;
    [SerializeField] internal Canvas thisCanvas;

    [SerializeField] private Image iconImage;
    [SerializeField] private Image shadowImage;
    [SerializeField] private TextMeshProUGUI textObject;

    public Image glowImage;
    public Animator glowAnimator;
    internal Color color;

    public string clearAnimation = "";

    public Transform collectEffect;
    public string collectAnimation = "";

    internal bool isSpawning = false;
    internal bool isClearing = false;

    public bool canMerge = true;
    internal bool isMerging = false;

    public float throwDistance = 0.4f;

    [NonReorderable] public OrientedItem[] otherOrientations;

    public float extraExecuteTime = 0;

    internal bool isLastInLink = false;

    private void Start()
    {
        if (customItem)
        {
            Setup(customItem);
        }
    }

    private void OnValidate()
    {
        if ( Application.isEditor )
        {
            if (customItem)
            {
                Setup(customItem);
            }
        }
    }

    public void SetType(int setValue)
    {
        type = setValue;
    }

    public void Setup( ScriptableGridItem setValue )
    {
        iconImage.sprite = setValue.icon;
        shadowImage.sprite = setValue.shadow;
        glowImage.sprite = setValue.glow;
        color = setValue.color;

        textObject.SetText("");
    }

    public void PlayAnimation( string setValue )
    {
        thisAnimator.Play(setValue);
    }

    public void PlayAnimationDelayed(string setValue, float delay)
    {
        StartCoroutine(DelayedAnimation(setValue, delay));
    }

    IEnumerator DelayedAnimation(string setValue, float delay)
    {
        yield return new WaitForSeconds(delay);

        thisAnimator.Play(setValue);
    }

    public void SetAnimatorBool(string stateName, bool setValue)
    {
        thisAnimator.SetBool(stateName, setValue);
    }

    public void SetAnimatorTrigger(string stateName)
    {
        thisAnimator.SetTrigger(stateName);
    }

    public Image GetIconImage()
    {
        return iconImage;
    }

    public GridItem GetOtherOrientation(Vector2 orienation)
    {
        for (int index = 0; index < otherOrientations.Length; index++)
        {
            for (int indexB = 0; indexB < otherOrientations[index].orientations.Length; indexB++)
            {
                if (otherOrientations[index].orientations[indexB] == orienation)
                {
                    return otherOrientations[index].gridItem;
                }
            }
        }

        return null;
    }
}

