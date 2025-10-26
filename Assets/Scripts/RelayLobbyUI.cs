using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Threading.Tasks;

public class RelayLobbyUI : MonoBehaviour
{
    [Header("UI References")]
    public RelayLobbyManager relayManager;
    public Button hostButton;
    public TMP_Text joinCodeDisplay;
    public Button joinButton;
    public TMP_InputField joinCodeInput;
    public TMP_Text statusText;
    public GameObject menuPanel;
    public GameObject gamePanel;
    public Button backToMenuButton;

    // Helper components
    private RelayLobbyUI_PanelFinder panelFinder;
    private RelayLobbyUI_PlayerList playerList;
    private RelayLobbyUI_ButtonWiring buttonWiring;

    private void Awake()
    {
        // Attach or find helper components
        panelFinder = gameObject.GetComponent<RelayLobbyUI_PanelFinder>() ?? gameObject.AddComponent<RelayLobbyUI_PanelFinder>();
        playerList = gameObject.GetComponent<RelayLobbyUI_PlayerList>() ?? gameObject.AddComponent<RelayLobbyUI_PlayerList>();
        buttonWiring = gameObject.GetComponent<RelayLobbyUI_ButtonWiring>() ?? gameObject.AddComponent<RelayLobbyUI_ButtonWiring>();
    }

    private void Start()
    {
        // Find panels and cache references
    panelFinder.FindPanels();
    playerList.playerListContent = panelFinder.playerListContent;
    // Wire the player list to the RelayLobbyManager so it can poll connected players
    if (playerList != null && relayManager != null)
    {
        playerList.GetPlayerNames = relayManager.GetConnectedPlayerNames;
        // initial update
        try { playerList.UpdatePlayerList(playerList.GetPlayerNames()); } catch {}
    }

        // Auto-find UI elements by name if not assigned in Inspector
        if (relayManager == null)
            relayManager = FindFirstObjectByType<RelayLobbyManager>();
        if (hostButton == null)
            hostButton = GameObject.Find("HostButton")?.GetComponent<Button>();
        if (joinButton == null)
            joinButton = GameObject.Find("JoinButton")?.GetComponent<Button>();
        if (joinCodeDisplay == null)
            joinCodeDisplay = GameObject.Find("JoinCodeDisplay")?.GetComponent<TMP_Text>();
        if (joinCodeInput == null)
            joinCodeInput = GameObject.Find("JoinCodeInput")?.GetComponent<TMP_InputField>();
        if (statusText == null)
            statusText = GameObject.Find("StatusText")?.GetComponent<TMP_Text>();
        if (menuPanel == null)
            menuPanel = GameObject.Find("MenuPanel");
        if (gamePanel == null)
            gamePanel = GameObject.Find("GamePanel");

        // Wire up buttons
        buttonWiring.startGameButton = panelFinder.startGameButton;
        buttonWiring.inGameBackToMenuButton = panelFinder.inGameBackToMenuButton;
        buttonWiring.hostButton = hostButton;
        buttonWiring.joinButton = joinButton;
        buttonWiring.backToMenuButton = backToMenuButton;
        buttonWiring.WireButtons(OnStartGameClicked, ResetToMenu, OnHostButtonClicked, OnJoinButtonClicked);

        SetupUI();
        _ = EnableButtonsAfterInit();
    }

    private void SetupUI()
    {
        if (hostButton != null)
        {
            var hostText = hostButton.GetComponentInChildren<TMP_Text>();
            if (hostText != null) hostText.text = "HOST GAME";
            hostButton.interactable = false;
        }
        if (joinButton != null)
        {
            var joinText = joinButton.GetComponentInChildren<TMP_Text>();
            if (joinText != null) joinText.text = "JOIN GAME";
            joinButton.interactable = false;
        }
        if (joinCodeDisplay != null)
            joinCodeDisplay.text = "";
        if (statusText != null)
            statusText.text = "Initializing Unity Services...";
        if (menuPanel != null)
            menuPanel.SetActive(true);
        if (gamePanel != null)
            gamePanel.SetActive(false);
    }

    private async Task EnableButtonsAfterInit()
    {
        await Task.Yield();
        int attempts = 0;
        while (attempts < 50)
        {
            if (relayManager != null && relayManager.AreServicesReady())
                break;
            await Task.Delay(100);
            attempts++;
        }
        if (relayManager != null && relayManager.AreServicesReady())
        {
            if (hostButton != null)
                hostButton.interactable = true;
            if (joinButton != null)
                joinButton.interactable = true;
            if (statusText != null)
                statusText.text = "Ready to play!";
            
        }
        else
        {
            if (statusText != null)
                statusText.text = "Failed to initialize services";
            Debug.LogError("Services not ready after timeout");
        }
    }

    public void OnHostButtonClicked()
    {
        if (menuPanel != null) menuPanel.SetActive(false);
        _ = HostGameAsync();
    }

    public void OnJoinButtonClicked()
    {
        if (joinCodeInput == null)
        {
            Debug.LogError("Join code input field not assigned!");
            return;
        }
        string code = joinCodeInput.text.Trim().ToUpper();
        if (!string.IsNullOrEmpty(code))
        {
            Debug.Log($"[RelayLobbyUI] Attempting to join with code: {code}");
            _ = JoinGameAsync(code);
        }
        else
        {
            if (statusText != null)
                statusText.text = "Please enter a join code!";
            Debug.LogWarning("Join code is empty!");
        }
    }

    private async Task HostGameAsync()
    {
        if (relayManager == null)
        {
            Debug.LogError("RelayLobbyManager reference missing!");
            if (statusText != null)
                statusText.text = "RelayLobbyManager missing!";
            return;
        }
        if (statusText != null)
            statusText.text = "Creating lobby...";
        if (hostButton != null) hostButton.interactable = false;
        if (joinButton != null) joinButton.interactable = false;
        string joinCode = await relayManager.HostGame();
        Debug.Log($"[RelayLobbyUI] HostGameAsync: joinCode={joinCode}");
        if (!string.IsNullOrEmpty(joinCode))
        {
            if (joinCodeDisplay != null)
                joinCodeDisplay.text = "Join Code: " + joinCode;
            if (panelFinder.lobbyJoinCodeDisplay != null)
                panelFinder.lobbyJoinCodeDisplay.text = "Join Code: " + joinCode;
            if (statusText != null)
                statusText.text = "Hosting! Tanks should spawn. Share the join code.";
            playerList.UpdatePlayerList(new string[] { "Host" });
            if (menuPanel != null)
            {
                menuPanel.SetActive(false);
            
            }
            if (panelFinder.lobbyPanel != null) panelFinder.lobbyPanel.SetActive(true);
        }
        else
        {
            if (statusText != null)
                statusText.text = "Failed to create lobby!";
            if (hostButton != null) hostButton.interactable = true;
            if (joinButton != null) joinButton.interactable = true;
        }
    }

    private async Task JoinGameAsync(string joinCode)
    {
        if (relayManager == null)
        {
            Debug.LogError("RelayLobbyManager reference missing!");
            if (statusText != null)
                statusText.text = "RelayLobbyManager missing!";
            return;
        }
        if (statusText != null)
            statusText.text = "Joining lobby...";
        if (hostButton != null) hostButton.interactable = false;
        if (joinButton != null) joinButton.interactable = false;
        try
        {
            Debug.Log($"[RelayLobbyUI] Calling RelayLobbyManager.JoinGame with code: {joinCode}");
            await relayManager.JoinGame(joinCode);
            if (relayManager != null && relayManager.netManager != null && relayManager.netManager.IsClient && relayManager.netManager.IsConnectedClient)
            {
                if (statusText != null)
                    statusText.text = "Joined game!";
                if (menuPanel != null) menuPanel.SetActive(false);
                if (panelFinder.lobbyPanel != null) panelFinder.lobbyPanel.SetActive(true);
            }
            else
            {
                if (statusText != null)
                    statusText.text = "Failed to join game. Please check the join code and try again.";
                if (hostButton != null) hostButton.interactable = true;
                if (joinButton != null) joinButton.interactable = true;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to join game: {ex}\n{ex.StackTrace}");
            if (statusText != null)
                statusText.text = "Failed to join game. Please check the join code and try again.";
            if (hostButton != null) hostButton.interactable = true;
            if (joinButton != null) joinButton.interactable = true;
        }
    }

    public void ResetToMenu()
    {
        if (relayManager != null)
            relayManager.LeaveGame();
        if (menuPanel != null) menuPanel.SetActive(true);
        if (panelFinder.lobbyPanel != null) panelFinder.lobbyPanel.SetActive(false);
        playerList.UpdatePlayerList(new string[0]);
        if (joinCodeDisplay != null) joinCodeDisplay.text = "";
        if (joinCodeInput != null) joinCodeInput.text = "";
        if (statusText != null) statusText.text = "Ready to play!";
        if (hostButton != null) hostButton.interactable = true;
        if (joinButton != null) joinButton.interactable = true;
        
    }

    public void UpdatePlayerList(string[] playerNames)
    {
        playerList.UpdatePlayerList(playerNames);
    }

    private void OnStartGameClicked()
    {
        
    }
}
