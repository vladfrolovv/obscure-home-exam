using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Controls time scale and framerate of game.
 also has a slowmotion method*/
public class TimeManager : MonoBehaviour
{
    public static TimeManager instance;

    [SerializeField] private int frameRate = 60;
    [SerializeField] private float gameSpeed = 1;

    private float slowmoTimeScale = 1;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = frameRate;
        Time.timeScale = gameSpeed;
    }

    public void SetGameSpeed( float setValue )
    {
        gameSpeed = Time.timeScale = setValue;
    }

    public void SetTimeScale(float setValue)
    {
        Time.timeScale = setValue;
    }

    public void SetFrameRate(int setValue)
    {
        frameRate = Application.targetFrameRate = setValue;
    }

    public void SlowMotion(float value, float time)
    {
        Time.timeScale = slowmoTimeScale = value;

        LeanTween.value(slowmoTimeScale, gameSpeed, time).setOnUpdate(SetTimeScale).setEaseInCubic();
    }


}
