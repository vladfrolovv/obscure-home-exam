using UnityEngine.SceneManagement;
namespace ObscureGames.Scenes
{
    public static class ScenesController
    {

        public static void LoadScene(this SceneType sceneType)
        {
            SceneManager.LoadScene((int)sceneType);
        }

    }
}
