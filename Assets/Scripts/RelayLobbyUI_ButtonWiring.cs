using UnityEngine;
using UnityEngine.UI;

public class RelayLobbyUI_ButtonWiring : MonoBehaviour
{
    public Button startGameButton;
    public Button inGameBackToMenuButton;
    public Button hostButton;
    public Button joinButton;
    public Button backToMenuButton;

    public void WireButtons(System.Action onStartGame, System.Action onBackToMenu, System.Action onHost, System.Action onJoin)
    {
        if (startGameButton != null)
            startGameButton.onClick.AddListener(() => onStartGame());
        if (inGameBackToMenuButton != null)
            inGameBackToMenuButton.onClick.AddListener(() => onBackToMenu());
        if (hostButton != null)
            hostButton.onClick.AddListener(() => onHost());
        if (joinButton != null)
            joinButton.onClick.AddListener(() => onJoin());
        if (backToMenuButton != null)
            backToMenuButton.onClick.AddListener(() => onBackToMenu());
    }
}
