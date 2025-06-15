using System.Collections.Generic;
using UnityEngine;
namespace OGServer.Scripts
{
    public class BootstrapServer : MonoBehaviour
    {

        // private readonly List<ConnectedPlayer> _connectedPlayers = new();

        private void Awake()
        {
            StartRemoteServer();
        }

        private void StartRemoteServer()
        {
            Debug.Log("[ServerStartUp].StartRemoteServer");

            // PlayFabMultiplayerAgentAPI.Start();
            // PlayFabMultiplayerAgentAPI.IsDebugging = configuration.playFabDebugging;
            // PlayFabMultiplayerAgentAPI.OnMaintenanceCallback += OnMaintenance;
            // PlayFabMultiplayerAgentAPI.OnShutDownCallback += OnShutdown;
            // PlayFabMultiplayerAgentAPI.OnServerActiveCallback += OnServerActive;
            // PlayFabMultiplayerAgentAPI.OnAgentErrorCallback += OnAgentError;
            //
            // UNetServer.OnPlayerAdded.AddListener(OnPlayerAdded);
            // UNetServer.OnPlayerRemoved.AddListener(OnPlayerRemoved);
            //
            // StartCoroutine(ReadyForPlayers());
            // StartCoroutine(ShutdownServerInXTime());
        }

    }
}
