using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RelayLobbyUI_PanelFinder : MonoBehaviour
{
    public GameObject lobbyPanel;
    public TMP_Text lobbyJoinCodeDisplay;
    public Button startGameButton;
    public Button inGameBackToMenuButton;
    public GameObject playerListContent;

    public void FindPanels()
    {
        lobbyPanel = AutoLobbyGamePanelBootstrap.LobbyPanelInstance;
        if (lobbyPanel != null)
        {
            lobbyJoinCodeDisplay = lobbyPanel.transform.Find("LobbyJoinCodeDisplay")?.GetComponent<TMP_Text>();
            startGameButton = lobbyPanel.transform.Find("StartGameButton")?.GetComponent<Button>();
            inGameBackToMenuButton = lobbyPanel.transform.Find("BackToMenuButton")?.GetComponent<Button>();
            playerListContent = lobbyPanel.transform.Find("PlayerListScrollView/PlayerListContent")?.gameObject;
        }
        else
        {
            Debug.LogWarning("[RelayLobbyUI_PanelFinder] LobbyPanel not found!");
        }
    }
}
