using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace ObscureGames.Gameplay.UI
{
    public class ProgressBarView : MonoBehaviour
    {

        [SerializeField] private Animator progressAnimator;
        [SerializeField] private Image progressImage;
        [SerializeField] private Image progressImageBack;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private Transform incrementHolder;
        [SerializeField] private RectTransform increment;

        [SerializeField] private float progress = 0;
        [SerializeField] private float progressMax = 20;
        [SerializeField] private bool showText = true;
        [SerializeField] private bool showMax = true;

        public void Setup(NetworkPlayer player)
        {
            if (progressText)
            {
                if (showText)
                {
                    if (showMax)
                    {
                        progressText.SetText(progress + "/" + progressMax);
                    }
                    else
                    {
                        progressText.SetText(progress.ToString());
                    }
                }
                else
                {
                    progressText.SetText(string.Empty);
                }
            }

            if (incrementHolder && increment)
            {
                Image incrementImage = increment.Find("Full").GetComponent<Image>();
                Image incrementImageBack = increment.Find("Empty").GetComponent<Image>();

                if (player != null) SetBarColor(incrementImage, incrementImageBack, player.playerColor);

                incrementImage.enabled = false;

                for (int index = 0; index < progressMax; index++)
                {
                    RectTransform newIncrement = Instantiate(increment, incrementHolder);

                    TextMeshProUGUI incrementText = newIncrement.GetComponentInChildren<TextMeshProUGUI>();

                    if (showText == true) incrementText.SetText((index + 1).ToString());
                    else incrementText.SetText("");
                }

                Destroy(increment.gameObject);
            }
        }

        public void ChangeProgress(float changeValue)
        {
            progress += changeValue;

            UpdateProgress(changeValue);
        }

        public void UpdateProgress(float changeValue)
        {
            progress = Mathf.Clamp(progress, 0, progressMax);

            if (showText == true)
            {
                if (progressText)
                {
                    if (showMax == true) progressText.SetText(progress + "/" + progressMax);
                    else progressText.SetText(progress.ToString());
                }
            }

            float targetFill = progress / progressMax;

            if (progressImage) progressImage.fillAmount = targetFill;

            if (incrementHolder)
            {
                for (int index = 0; index < incrementHolder.childCount; index++)
                {
                    Image incrementImage = incrementHolder.GetChild(index).Find("Full").GetComponent<Image>();

                    incrementImage.enabled = (index < progress);

                    if (changeValue < 0 && index == progress)
                    {
                        incrementHolder.GetChild(index).localScale = Vector3.one;

                        //LeanTween.color(incrementImage.rectTransform, Color.clear, 0.3f).setLoopPingPong(1).setEaseInElastic();

                        LeanTween.scale(incrementHolder.GetChild(index).gameObject, Vector3.one * 1.15f, 0.1f).setLoopPingPong(1).setEaseOutSine();
                    }
                    else if (changeValue > 0 && index == progress - 1)
                    {
                        incrementHolder.GetChild(index).localScale = Vector3.one;

                        //LeanTween.color(incrementImage.rectTransform, Color.clear, 0.3f).setLoopPingPong(1).setEaseInElastic();

                        LeanTween.scale(incrementHolder.GetChild(index).gameObject, Vector3.one * 1.15f, 0.2f).setLoopPingPong(1).setEaseOutSine();
                    }
                }
            }
        }

        public float GetProgress()
        {
            float targetFill = progress / progressMax;

            return targetFill;
        }

        public void SetProgress(float setValue)
        {
            progress = setValue;

            UpdateProgress(0);
        }

        public void SetProgressMax(float setValue)
        {
            progressMax = setValue;
        }

        public void SetBarColor(Color setValue)
        {
            if (progressImage) progressImage.color = setValue;
            if (progressImageBack) progressImageBack.color = setValue * 0.5f + Color.black;
        }

        public void SetBarColor(Image image, Image imageBack, Color setValue)
        {
            if (image) image.color = setValue;
            if (imageBack) imageBack.color = setValue * 0.5f + Color.black;
        }

        public void SetIncrementColor(Color setValue)
        {
            for (int index = 0; index < incrementHolder.childCount; index++)
            {
                Image incrementImage = incrementHolder.GetChild(index).Find("Full").GetComponent<Image>();
                Image incrementImageBack = incrementHolder.GetChild(index).Find("Empty").GetComponent<Image>();

                SetBarColor(incrementImage, incrementImageBack, setValue);
            }
        }

        public void Hide()
        {
            progressAnimator.Play("Hide");
        }

        public void Show()
        {
            progressAnimator.Play("Show");
        }

        public void Bounce()
        {
            progressAnimator.Play("Bounce");
            progressAnimator.Play("Bounce2");
        }

        public void Shake()
        {
            progressAnimator.Play("Shake");
        }

    }
}
