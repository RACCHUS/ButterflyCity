using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AutoLobbyUIBootstrap : MonoBehaviour
{
    void Awake()
    {
        // Ensure Canvas exists
    Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("Canvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGO.AddComponent<GraphicRaycaster>();
        }

        // Ensure EventSystem exists
        if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            // Use the new Input System UI module for compatibility
            es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        // Create Menu Panel with VerticalLayoutGroup
        GameObject menuPanel = new GameObject("MenuPanel");
        menuPanel.transform.SetParent(canvas.transform, false);
        if (menuPanel.GetComponent<RectTransform>() == null)
            menuPanel.AddComponent<RectTransform>();
        var menuRect = menuPanel.GetComponent<RectTransform>();
        menuRect.anchorMin = new Vector2(0.5f, 0.5f);
        menuRect.anchorMax = new Vector2(0.5f, 0.5f);
        menuRect.pivot = new Vector2(0.5f, 0.5f);
        menuRect.anchoredPosition = Vector2.zero;
        menuRect.sizeDelta = new Vector2(600, 600);
        var menuImage = menuPanel.AddComponent<Image>();
        menuImage.color = new Color(0.1f, 0.1f, 0.15f, 0.98f);
        var layoutGroup = menuPanel.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
        layoutGroup.childAlignment = TextAnchor.MiddleCenter;
        layoutGroup.spacing = 16f;
        layoutGroup.childForceExpandHeight = false;
        layoutGroup.childForceExpandWidth = true;
        var fitter = menuPanel.AddComponent<UnityEngine.UI.ContentSizeFitter>();
        fitter.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;

        // Title
        GameObject titleGO = new GameObject("Title");
        titleGO.transform.SetParent(menuPanel.transform, false);
        var titleText = titleGO.AddComponent<TextMeshProUGUI>();
        titleText.text = "Butterfly City Networking";
        titleText.fontSize = 32;
        titleText.alignment = TextAlignmentOptions.Center;
        var titleLE = titleGO.AddComponent<UnityEngine.UI.LayoutElement>();
        titleLE.preferredHeight = 60;
        titleLE.minHeight = 50;

        // Host Button
        GameObject hostBtnGO = new GameObject("HostButton");
        hostBtnGO.transform.SetParent(menuPanel.transform, false);
        var hostBtn = hostBtnGO.AddComponent<Button>();
        var hostImg = hostBtnGO.AddComponent<Image>();
        hostImg.color = new Color(0.2f, 0.4f, 0.2f, 1f);
        var hostLE = hostBtnGO.AddComponent<UnityEngine.UI.LayoutElement>();
        hostLE.preferredHeight = 60;
        hostLE.minHeight = 50;
        hostLE.preferredWidth = 300;
        // Host Button Text
        var hostTextGO = new GameObject("Text");
        hostTextGO.transform.SetParent(hostBtnGO.transform, false);
        var hostText = hostTextGO.AddComponent<TextMeshProUGUI>();
        hostText.text = "HOST GAME";
        hostText.fontSize = 24;
        hostText.alignment = TextAlignmentOptions.Center;

        // Wire up Host button to show GamePanel if it exists
        hostBtn.onClick.AddListener(() => {
            var lobbyPanel = AutoLobbyGamePanelBootstrap.LobbyPanelInstance;
            if (lobbyPanel != null) {
                lobbyPanel.SetActive(true);
            } else {
                Debug.LogWarning("[AutoLobbyUIBootstrap] LobbyPanel not found when Host clicked");
            }
        });

        // Join Code Input
        GameObject joinInputGO = new GameObject("JoinCodeInput");
        joinInputGO.transform.SetParent(menuPanel.transform, false);
        var joinInputLE = joinInputGO.AddComponent<UnityEngine.UI.LayoutElement>();
        joinInputLE.preferredHeight = 40;
        joinInputLE.minHeight = 32;
        joinInputLE.preferredWidth = 300;
        var joinInput = joinInputGO.AddComponent<TMP_InputField>();
        // Input background
        var joinInputImg = joinInputGO.AddComponent<Image>();
        joinInputImg.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        joinInput.targetGraphic = joinInputImg;
    // Input placeholder
    var placeholderGO = new GameObject("Placeholder");
    placeholderGO.transform.SetParent(joinInputGO.transform, false);
    var placeholder = placeholderGO.AddComponent<TextMeshProUGUI>();
    placeholder.text = "Enter Join Code...";
    placeholder.fontSize = 18;
    placeholder.color = new Color(0.7f, 0.7f, 0.7f, 0.7f);
        placeholder.alignment = TextAlignmentOptions.Center;
        placeholder.verticalAlignment = VerticalAlignmentOptions.Middle;
        var placeholderRect = placeholderGO.GetComponent<RectTransform>();
        placeholderRect.anchorMin = Vector2.zero;
        placeholderRect.anchorMax = Vector2.one;
        placeholderRect.offsetMin = Vector2.zero;
        placeholderRect.offsetMax = Vector2.zero;
        joinInput.placeholder = placeholder;

        // Add highlight effect for selection
        var highlightGO = new GameObject("Highlight");
        highlightGO.transform.SetParent(joinInputGO.transform, false);
        var highlightImg = highlightGO.AddComponent<Image>();
        highlightImg.color = new Color(0.4f, 0.6f, 1f, 0.25f);
        var highlightRect = highlightGO.GetComponent<RectTransform>();
        highlightRect.anchorMin = Vector2.zero;
        highlightRect.anchorMax = Vector2.one;
        highlightRect.offsetMin = Vector2.zero;
        highlightRect.offsetMax = Vector2.zero;
        highlightGO.SetActive(false);

        // Show highlight and hide placeholder on select, revert on deselect
        joinInput.onSelect.AddListener((_) => {
            highlightGO.SetActive(true);
            if (string.IsNullOrEmpty(joinInput.text)) placeholderGO.SetActive(false);
        });
        joinInput.onDeselect.AddListener((_) => {
            highlightGO.SetActive(false);
            if (string.IsNullOrEmpty(joinInput.text)) placeholderGO.SetActive(true);
        });
    // Input text
    var inputTextGO = new GameObject("Text");
    inputTextGO.transform.SetParent(joinInputGO.transform, false);
    var inputText = inputTextGO.AddComponent<TextMeshProUGUI>();
    inputText.fontSize = 18;
    inputText.color = Color.white;
    inputText.alignment = TextAlignmentOptions.Left;
    inputText.verticalAlignment = VerticalAlignmentOptions.Middle;
    var inputTextRect = inputTextGO.GetComponent<RectTransform>();
    inputTextRect.anchorMin = Vector2.zero;
    inputTextRect.anchorMax = Vector2.one;
    inputTextRect.offsetMin = new Vector2(10, 0);
    inputTextRect.offsetMax = new Vector2(-10, 0);
    joinInput.textComponent = inputText;

        // Join Button
        GameObject joinBtnGO = new GameObject("JoinButton");
        joinBtnGO.transform.SetParent(menuPanel.transform, false);
        var joinBtn = joinBtnGO.AddComponent<Button>();
        var joinImg = joinBtnGO.AddComponent<Image>();
        joinImg.color = new Color(0.2f, 0.2f, 0.4f, 1f);
        var joinLE = joinBtnGO.AddComponent<UnityEngine.UI.LayoutElement>();
        joinLE.preferredHeight = 60;
        joinLE.minHeight = 50;
        joinLE.preferredWidth = 300;
        // Join Button Text
        var joinTextGO = new GameObject("Text");
        joinTextGO.transform.SetParent(joinBtnGO.transform, false);
        var joinText = joinTextGO.AddComponent<TextMeshProUGUI>();
        joinText.text = "JOIN GAME";
        joinText.fontSize = 24;
        joinText.alignment = TextAlignmentOptions.Center;

        // Status Text
        GameObject statusGO = new GameObject("StatusText");
        statusGO.transform.SetParent(menuPanel.transform, false);
        var statusText = statusGO.AddComponent<TextMeshProUGUI>();
        statusText.text = "Initializing...";
        statusText.fontSize = 18;
        statusText.alignment = TextAlignmentOptions.Center;
        var statusLE = statusGO.AddComponent<UnityEngine.UI.LayoutElement>();
        statusLE.preferredHeight = 40;
        statusLE.minHeight = 32;
        statusLE.preferredWidth = 400;

        // Join Code Display
        GameObject joinCodeDisplayGO = new GameObject("JoinCodeDisplay");
        joinCodeDisplayGO.transform.SetParent(menuPanel.transform, false);
        var joinCodeDisplay = joinCodeDisplayGO.AddComponent<TextMeshProUGUI>();
        joinCodeDisplay.text = "";
        joinCodeDisplay.fontSize = 18;
        joinCodeDisplay.alignment = TextAlignmentOptions.Center;
        var joinCodeLE = joinCodeDisplayGO.AddComponent<UnityEngine.UI.LayoutElement>();
        joinCodeLE.preferredHeight = 30;
        joinCodeLE.minHeight = 24;
        joinCodeLE.preferredWidth = 400;

        // Back to Menu Button
        GameObject backBtnGO = new GameObject("BackToMenuButton");
        backBtnGO.transform.SetParent(menuPanel.transform, false);
        var backBtn = backBtnGO.AddComponent<Button>();
        var backImg = backBtnGO.AddComponent<Image>();
        backImg.color = new Color(0.3f, 0.2f, 0.2f, 1f);
        var backLE = backBtnGO.AddComponent<UnityEngine.UI.LayoutElement>();
        backLE.preferredHeight = 50;
        backLE.minHeight = 40;
        backLE.preferredWidth = 200;
        // Back Button Text
        var backTextGO = new GameObject("Text");
        backTextGO.transform.SetParent(backBtnGO.transform, false);
        var backText = backTextGO.AddComponent<TextMeshProUGUI>();
        backText.text = "Back to Menu";
        backText.fontSize = 24;
        backText.alignment = TextAlignmentOptions.Center;
    }
}
