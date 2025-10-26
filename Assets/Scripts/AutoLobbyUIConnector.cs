using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AutoLobbyUIConnector : MonoBehaviour
{
    public RelayLobbyUI relayLobbyUI;
    public RelayLobbyManager relayLobbyManager;

    void Awake()
    {
        // Ensure AutoLobbyGamePanelBootstrap runs before RelayLobbyUI
        AutoLobbyGamePanelBootstrap gamePanelBootstrap = FindFirstObjectByType<AutoLobbyGamePanelBootstrap>();
        if (gamePanelBootstrap == null)
        {
            GameObject gamePanelGO = new GameObject("AutoLobbyGamePanelBootstrap");
            gamePanelBootstrap = gamePanelGO.AddComponent<AutoLobbyGamePanelBootstrap>();
        }

        // Find or create RelayLobbyManager
        relayLobbyManager = FindFirstObjectByType<RelayLobbyManager>();
        if (relayLobbyManager == null)
        {
            GameObject mgrGO = new GameObject("RelayLobbyManager");
            relayLobbyManager = mgrGO.AddComponent<RelayLobbyManager>();
        }

        // Find or create RelayLobbyUI
        relayLobbyUI = FindFirstObjectByType<RelayLobbyUI>();
        if (relayLobbyUI == null)
        {
            GameObject uiGO = new GameObject("RelayLobbyUI");
            relayLobbyUI = uiGO.AddComponent<RelayLobbyUI>();
        }

        // Wire up UI fields from runtime-created objects
        relayLobbyUI.relayManager = relayLobbyManager;
        relayLobbyUI.hostButton = GameObject.Find("HostButton")?.GetComponent<Button>();
        relayLobbyUI.joinButton = GameObject.Find("JoinButton")?.GetComponent<Button>();
        relayLobbyUI.backToMenuButton = GameObject.Find("BackToMenuButton")?.GetComponent<Button>();
        relayLobbyUI.joinCodeDisplay = GameObject.Find("JoinCodeDisplay")?.GetComponent<TMP_Text>();
        relayLobbyUI.joinCodeInput = GameObject.Find("JoinCodeInput")?.GetComponent<TMP_InputField>();
        relayLobbyUI.statusText = GameObject.Find("StatusText")?.GetComponent<TMP_Text>();
    relayLobbyUI.menuPanel = GameObject.Find("MenuPanel");
    // No longer assign gamePanel, use lobbyPanel logic
    }
}
