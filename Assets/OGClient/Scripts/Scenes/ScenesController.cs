using UnityEngine.SceneManagement;
namespace OGClient.Scenes
{
    public static class ScenesController
    {

        public static void LoadScene(this SceneType sceneType)
        {
            SceneManager.LoadScene((int)sceneType);
        }

    }
}
