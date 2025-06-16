using UnityEngine;
namespace OGShared.Scripts
{
    public class DontDestroyOnLoadView : MonoBehaviour
    {

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

    }
}

