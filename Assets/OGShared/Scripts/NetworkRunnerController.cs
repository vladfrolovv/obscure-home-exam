using Fusion;
using UnityEngine;
namespace OGShared.Scripts
{
    public class NetworkRunnerController : MonoBehaviour
    {

        public static NetworkRunner Instance { get; private set; }


        private void Awake()
        {
            NetworkRunner runner = GetComponent<NetworkRunner>();
            if (Instance != null && Instance != runner)
            {
                Destroy(gameObject);
                return;
            }

            Instance = runner;
            DontDestroyOnLoad(gameObject);
        }

    }
}

