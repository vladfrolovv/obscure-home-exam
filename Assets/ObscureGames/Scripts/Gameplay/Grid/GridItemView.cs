using System.Collections;
using ObscureGames.Gameplay.Grid.Configs;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GridItemView : MonoBehaviour
{

    public ScriptableGridItem customItem;

    public int type = 0;

    [SerializeField] private Animator _thisAnimator;
    [SerializeField] private Canvas _canvas;

    [SerializeField] private Image iconImage;
    [SerializeField] private Image shadowImage;
    [SerializeField] private TextMeshProUGUI textObject;

    public Canvas GridItemCanvas => _canvas;

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
        if (!Application.isEditor) return;
        ValidateCustomItem();
    }

    private void ValidateCustomItem()
    {
        if (!customItem) return;
        Setup(customItem);
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
        _thisAnimator.Play(setValue);
    }

    public void PlayAnimationDelayed(string setValue, float delay)
    {
        StartCoroutine(DelayedAnimation(setValue, delay));
    }

    IEnumerator DelayedAnimation(string setValue, float delay)
    {
        yield return new WaitForSeconds(delay);

        _thisAnimator.Play(setValue);
    }

    public void SetAnimatorBool(string stateName, bool setValue)
    {
        _thisAnimator.SetBool(stateName, setValue);
    }

    public void SetAnimatorTrigger(string stateName)
    {
        _thisAnimator.SetTrigger(stateName);
    }

    public Image GetIconImage()
    {
        return iconImage;
    }

    public GridItemView GetOtherOrientation(Vector2 orienation)
    {
        for (int index = 0; index < otherOrientations.Length; index++)
        {
            for (int indexB = 0; indexB < otherOrientations[index].orientations.Length; indexB++)
            {
                if (otherOrientations[index].orientations[indexB] == orienation)
                {
                    return otherOrientations[index].GridItemView;
                }
            }
        }

        return null;
    }
}

