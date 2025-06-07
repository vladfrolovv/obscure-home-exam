using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Toast : MonoBehaviour
{
    private Transform thisTransform;
    [SerializeField] private Animator thisAnimator;
    [SerializeField] private TextMeshProUGUI thisText;

    private void Start()
    {
        thisTransform = this.transform;
    }

    public void SetToast( Vector3 position, string setMessage, Color setColor )
    {
        //thisTransform.position = position;

        thisText.SetText(setMessage);
        thisText.color = setColor;

        thisAnimator.Play("Intro");
    }
}
