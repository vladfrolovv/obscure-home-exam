using UnityEngine;
namespace OGClient.Timers
{
    public class TimeController
    {

        private const int DefaultFrameRate = 60;
        private const float DefaultGameSpeed = 1;

        private float _gameplayTimeScale = DefaultGameSpeed;
        private float _slowMoTimeScale = DefaultGameSpeed;

        public TimeController()
        {
            Application.targetFrameRate = DefaultFrameRate;
            Time.timeScale = DefaultGameSpeed;
        }

        public void SetGameSpeed(float setValue)
        {
            _gameplayTimeScale = Time.timeScale = setValue;
        }

        public void SetFrameRate(int setValue)
        {
            Application.targetFrameRate = setValue;
        }

        public void SlowMotion(float value, float time)
        {
            Time.timeScale = _slowMoTimeScale = value;
            LeanTween.value(_slowMoTimeScale, _gameplayTimeScale, time).setOnUpdate(SetTimeScale).setEaseInCubic();
        }

        private void SetTimeScale(float setValue)
        {
            Time.timeScale = setValue;
        }

    }
}
