using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AutoLobbyGamePanelBootstrap : MonoBehaviour
{
    public static GameObject LobbyPanelInstance { get; private set; }
    public GameObject lobbyPanel;
    public TMP_Text lobbyJoinCodeDisplay;
    public Button startGameButton;
    public Button backToMenuButton;
    public GameObject playerListContent;

    void Awake()
    {
        // Find or create Canvas
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("Canvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGO.AddComponent<GraphicRaycaster>();
        }
        // Create Lobby Panel
    lobbyPanel = new GameObject("LobbyPanel");
    
    lobbyPanel.transform.SetParent(canvas.transform, false);
    LobbyPanelInstance = lobbyPanel;
        var lobbyRect = lobbyPanel.AddComponent<RectTransform>();
        lobbyRect.anchorMin = new Vector2(0.5f, 0.5f);
        lobbyRect.anchorMax = new Vector2(0.5f, 0.5f);
        lobbyRect.pivot = new Vector2(0.5f, 0.5f);
        lobbyRect.anchoredPosition = Vector2.zero;
        lobbyRect.sizeDelta = new Vector2(600, 600);
        var lobbyImage = lobbyPanel.AddComponent<Image>();
        lobbyImage.color = new Color(0.12f, 0.12f, 0.18f, 0.98f);
        var layoutGroup = lobbyPanel.AddComponent<VerticalLayoutGroup>();
        layoutGroup.childAlignment = TextAnchor.UpperCenter;
        layoutGroup.spacing = 16f;
        layoutGroup.childForceExpandHeight = false;
        layoutGroup.childForceExpandWidth = true;
        var fitter = lobbyPanel.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        lobbyPanel.SetActive(false);
        

    // Lobby Join Code Display
    GameObject joinCodeGO = new GameObject("LobbyJoinCodeDisplay");
    joinCodeGO.transform.SetParent(lobbyPanel.transform, false);
    lobbyJoinCodeDisplay = joinCodeGO.AddComponent<TextMeshProUGUI>();
    lobbyJoinCodeDisplay.text = "";
    lobbyJoinCodeDisplay.fontSize = 22;
    lobbyJoinCodeDisplay.alignment = TextAlignmentOptions.Center;
    var joinCodeLE = joinCodeGO.AddComponent<LayoutElement>();
    joinCodeLE.preferredHeight = 40;
    joinCodeLE.minHeight = 32;
    joinCodeLE.preferredWidth = 400;

    // Player List Label
    GameObject playerListLabelGO = new GameObject("PlayerListLabel");
    playerListLabelGO.transform.SetParent(lobbyPanel.transform, false);
        var playerListLabel = playerListLabelGO.AddComponent<TextMeshProUGUI>();
        playerListLabel.text = "Players in Lobby:";
        playerListLabel.fontSize = 18;
        playerListLabel.alignment = TextAlignmentOptions.Left;
        var playerListLabelLE = playerListLabelGO.AddComponent<LayoutElement>();
        playerListLabelLE.preferredHeight = 28;
        playerListLabelLE.minHeight = 20;
        playerListLabelLE.preferredWidth = 400;

        // Player List Scroll View (with proper Viewport hierarchy)
        GameObject scrollViewGO = new GameObject("PlayerListScrollView");
        scrollViewGO.transform.SetParent(lobbyPanel.transform, false);
        var scrollRect = scrollViewGO.AddComponent<RectTransform>();
        scrollRect.sizeDelta = new Vector2(400, 120);
        var scrollView = scrollViewGO.AddComponent<ScrollRect>();
        var scrollViewImg = scrollViewGO.AddComponent<Image>();
        scrollViewImg.color = new Color(0.18f, 0.18f, 0.22f, 0.8f);
        var scrollLE = scrollViewGO.AddComponent<LayoutElement>();
        scrollLE.preferredHeight = 120;
        scrollLE.minHeight = 80;
        scrollLE.preferredWidth = 400;

        // Viewport
        GameObject viewportGO = new GameObject("Viewport");
        viewportGO.transform.SetParent(scrollViewGO.transform, false);
        var viewportRect = viewportGO.AddComponent<RectTransform>();
        viewportRect.anchorMin = new Vector2(0, 0);
        viewportRect.anchorMax = new Vector2(1, 1);
        viewportRect.pivot = new Vector2(0.5f, 0.5f);
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;
    // Add a transparent Image (optional) and use RectMask2D which is simpler and more reliable for UI clipping
    var viewportImg = viewportGO.AddComponent<Image>();
    viewportImg.color = new Color(1, 1, 1, 0); // Transparent graphic
    var viewportMask = viewportGO.AddComponent<UnityEngine.UI.RectMask2D>();
        scrollView.viewport = viewportRect;

        // Content for player list
        playerListContent = new GameObject("PlayerListContent");
        playerListContent.transform.SetParent(viewportGO.transform, false);
        var contentRect = playerListContent.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0, 0);
        var contentLayout = playerListContent.AddComponent<VerticalLayoutGroup>();
        contentLayout.childAlignment = TextAnchor.UpperLeft;
        contentLayout.spacing = 4f;
        contentLayout.childForceExpandHeight = false;
        contentLayout.childForceExpandWidth = true;
        contentLayout.padding = new RectOffset(8, 8, 8, 8);
        scrollView.content = contentRect;

    // Start Game Button
    GameObject startBtnGO = new GameObject("StartGameButton");
    startBtnGO.transform.SetParent(lobbyPanel.transform, false);
        startGameButton = startBtnGO.AddComponent<Button>();
        var startImg = startBtnGO.AddComponent<Image>();
        startImg.color = new Color(0.2f, 0.5f, 0.2f, 1f);
        var startLE = startBtnGO.AddComponent<LayoutElement>();
        startLE.preferredHeight = 50;
        startLE.minHeight = 40;
        startLE.preferredWidth = 220;
        // Start Button Text
        var startTextGO = new GameObject("Text");
        startTextGO.transform.SetParent(startBtnGO.transform, false);
        var startText = startTextGO.AddComponent<TextMeshProUGUI>();
        startText.text = "START GAME";
        startText.fontSize = 22;
        startText.alignment = TextAlignmentOptions.Center;

    // Back to Menu Button
    GameObject backBtnGO = new GameObject("BackToMenuButton");
    backBtnGO.transform.SetParent(lobbyPanel.transform, false);
        backToMenuButton = backBtnGO.AddComponent<Button>();
        var backImg = backBtnGO.AddComponent<Image>();
        backImg.color = new Color(0.3f, 0.2f, 0.2f, 1f);
        var backLE = backBtnGO.AddComponent<LayoutElement>();
        backLE.preferredHeight = 50;
        backLE.minHeight = 40;
        backLE.preferredWidth = 200;
        // Back Button Text
        var backTextGO = new GameObject("Text");
        backTextGO.transform.SetParent(backBtnGO.transform, false);
        var backText = backTextGO.AddComponent<TextMeshProUGUI>();
        backText.text = "Back to Menu";
        backText.fontSize = 22;
        backText.alignment = TextAlignmentOptions.Center;
    }
}
