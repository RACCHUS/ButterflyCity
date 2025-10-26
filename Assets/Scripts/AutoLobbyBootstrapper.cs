using UnityEngine;

// Ensures all UI bootstrap scripts are created at runtime, in correct order
public class AutoLobbyBootstrapper : MonoBehaviour
{
    void Awake()
    {
        // Ensure UI scripts are present and only one instance exists
        if (FindFirstObjectByType<AutoLobbyUIBootstrap>() == null)
        {
            var go = new GameObject("AutoLobbyUIBootstrap");
            go.AddComponent<AutoLobbyUIBootstrap>();
        }
        if (FindFirstObjectByType<AutoLobbyGamePanelBootstrap>() == null)
        {
            var go = new GameObject("AutoLobbyGamePanelBootstrap");
            go.AddComponent<AutoLobbyGamePanelBootstrap>();
        }
        if (FindFirstObjectByType<AutoLobbyUIConnector>() == null)
        {
            var go = new GameObject("AutoLobbyUIConnector");
            go.AddComponent<AutoLobbyUIConnector>();
        }
    }
}
