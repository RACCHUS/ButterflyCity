using UnityEngine;

public static class RelayLobbyUI_Utils
{
    public static void DumpUIHierarchy()
    {
        foreach (var canvas in GameObject.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
        {
            Debug.Log("[RelayLobbyUI] Canvas: " + canvas.name);
            DumpChildrenRecursive(canvas.transform, 1);
        }
        foreach (var goRoot in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
        {
            Debug.Log("[RelayLobbyUI] Root: " + goRoot.name);
            DumpChildrenRecursive(goRoot.transform, 1);
        }
    }
    private static void DumpChildrenRecursive(Transform parent, int depth)
    {
        string indent = new string(' ', depth * 2);
        foreach (Transform child in parent)
        {
            Debug.Log($"[RelayLobbyUI]{indent}{child.name} (active={child.gameObject.activeSelf})");
            DumpChildrenRecursive(child, depth + 1);
        }
    }
}
